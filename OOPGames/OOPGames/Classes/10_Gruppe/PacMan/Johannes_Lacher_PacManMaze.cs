using System;
using System.Collections.Generic;

namespace OOPGames
{
    public struct G10_PixelPoint
    {
        public G10_PixelPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; private set; }
        public int Y { get; private set; }
    }

    // Maze as data: rows of text plus the positions the game needs. A new maze is a new layout,
    // not new code. Characters: '#' wall, '.' dot, 'o' power pellet, ' ' floor, '-' ghost door,
    // 'T' tunnel floor (ghosts are slower there).
    public sealed class G10_MazeLayout
    {
        readonly Dictionary<G10_GhostKind, G10_PixelPoint> _GhostStarts;
        readonly Dictionary<G10_GhostKind, G10_Tile> _ScatterCorners;

        public G10_MazeLayout(string[] rows, G10_PixelPoint pacManStart, G10_PixelPoint houseExit, int houseCenterY,
            G10_PixelPoint fruitPosition, Dictionary<G10_GhostKind, G10_PixelPoint> ghostStarts,
            Dictionary<G10_GhostKind, G10_Tile> scatterCorners)
        {
            if (rows == null || rows.Length == 0) throw new ArgumentException("The layout needs rows.", nameof(rows));
            foreach (string row in rows)
            {
                if (row.Length != rows[0].Length) throw new ArgumentException("All layout rows must have the same length.", nameof(rows));
            }
            Rows = rows;
            PacManStart = pacManStart;
            HouseExit = houseExit;
            HouseCenterY = houseCenterY;
            FruitPosition = fruitPosition;
            _GhostStarts = ghostStarts;
            _ScatterCorners = scatterCorners;
        }

        public string[] Rows { get; private set; }
        public G10_PixelPoint PacManStart { get; private set; }
        // Point just above the ghost door where ghosts leave the house.
        public G10_PixelPoint HouseExit { get; private set; }
        public int HouseCenterY { get; private set; }
        public G10_PixelPoint FruitPosition { get; private set; }

        public G10_PixelPoint GhostStart(G10_GhostKind kind) { return _GhostStarts[kind]; }
        public G10_Tile ScatterCorner(G10_GhostKind kind) { return _ScatterCorners[kind]; }
    }

    public static class G10_MazeLayouts
    {
        public static readonly G10_MazeLayout Classic = new G10_MazeLayout(
            new[]
            {
                "############################",
                "#............##............#",
                "#.####.#####.##.#####.####.#",
                "#o####.#####.##.#####.####o#",
                "#.####.#####.##.#####.####.#",
                "#..........................#",
                "#.####.##.########.##.####.#",
                "#.####.##.########.##.####.#",
                "#......##....##....##......#",
                "######.##### ## #####.######",
                "######.##### ## #####.######",
                "######.##          ##.######",
                "######.## ###--### ##.######",
                "######.## #      # ##.######",
                "TTTTTT.   #      #   .TTTTTT",
                "######.## #      # ##.######",
                "######.## ######## ##.######",
                "######.##          ##.######",
                "######.## ######## ##.######",
                "######.## ######## ##.######",
                "#............##............#",
                "#.####.#####.##.#####.####.#",
                "#.####.#####.##.#####.####.#",
                "#o..##.......  .......##..o#",
                "###.##.##.########.##.##.###",
                "###.##.##.########.##.##.###",
                "#......##....##....##......#",
                "#.##########.##.##########.#",
                "#.##########.##.##########.#",
                "#..........................#",
                "############################"
            },
            new G10_PixelPoint(112, 188),
            new G10_PixelPoint(112, 92),
            116,
            new G10_PixelPoint(112, 140),
            new Dictionary<G10_GhostKind, G10_PixelPoint>
            {
                { G10_GhostKind.Blinky, new G10_PixelPoint(112, 92) },
                { G10_GhostKind.Pinky, new G10_PixelPoint(112, 116) },
                { G10_GhostKind.Inky, new G10_PixelPoint(96, 116) },
                { G10_GhostKind.Clyde, new G10_PixelPoint(128, 116) }
            },
            new Dictionary<G10_GhostKind, G10_Tile>
            {
                { G10_GhostKind.Blinky, new G10_Tile(25, -3) },
                { G10_GhostKind.Pinky, new G10_Tile(2, -3) },
                { G10_GhostKind.Inky, new G10_Tile(27, 31) },
                { G10_GhostKind.Clyde, new G10_Tile(0, 31) }
            });
    }

    public sealed class G10_Maze : IG10_MazeView
    {
        static int _NextVersion = 1;

        readonly G10_TileKind[,] _Kinds;
        readonly G10_GridCollectible[,] _Items;

        public G10_Maze(G10_MazeLayout layout)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            Layout = layout;
            Rows = layout.Rows.Length;
            Columns = layout.Rows[0].Length;
            Version = _NextVersion++;
            _Kinds = new G10_TileKind[Columns, Rows];
            _Items = new G10_GridCollectible[Columns, Rows];
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                    _Kinds[col, row] = KindOf(layout.Rows[row][col]);
            }
            Refill();
        }

        public G10_MazeLayout Layout { get; private set; }
        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public int Version { get; private set; }
        public int TotalDots { get; private set; }
        public int DotsEaten { get; private set; }
        public int DotsLeft { get { return TotalDots - DotsEaten; } }
        public int WidthSubPixels { get { return Columns * G10_PacManConstants.TileSubPixels; } }

        // Puts all dots and power pellets back for a new level.
        public void Refill()
        {
            TotalDots = 0;
            DotsEaten = 0;
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    char c = Layout.Rows[row][col];
                    _Items[col, row] = c == '.' ? G10_Dot.Instance : c == 'o' ? G10_PowerPellet.Instance : null;
                    if (_Items[col, row] != null) TotalDots++;
                }
            }
        }

        // Columns wrap around (tunnel); rows outside the maze are walls.
        public G10_TileKind KindAt(G10_Tile tile)
        {
            if (tile.Row < 0 || tile.Row >= Rows) return G10_TileKind.Wall;
            return _Kinds[WrapColumn(tile.Col), tile.Row];
        }

        public G10_CollectibleKind CollectibleAt(G10_Tile tile)
        {
            if (tile.Row < 0 || tile.Row >= Rows) return G10_CollectibleKind.None;
            G10_GridCollectible item = _Items[WrapColumn(tile.Col), tile.Row];
            return item == null ? G10_CollectibleKind.None : item.Kind;
        }

        public bool IsWalkable(G10_Tile tile, bool canUseDoor)
        {
            G10_TileKind kind = KindAt(tile);
            if (kind == G10_TileKind.GhostDoor) return canUseDoor;
            return kind != G10_TileKind.Wall;
        }

        public bool IsTunnel(G10_Tile tile)
        {
            return KindAt(tile) == G10_TileKind.Tunnel;
        }

        // Removes the item on the tile and returns what it was worth. Empty tiles return CollectResult.Nothing.
        public G10_CollectResult Collect(G10_Tile tile)
        {
            if (tile.Row < 0 || tile.Row >= Rows) return G10_CollectResult.Nothing;
            int col = WrapColumn(tile.Col);
            G10_GridCollectible item = _Items[col, tile.Row];
            if (item == null) return G10_CollectResult.Nothing;
            _Items[col, tile.Row] = null;
            DotsEaten++;
            return item.Collect();
        }

        public int WrapX(int subPixelX)
        {
            int width = WidthSubPixels;
            if (subPixelX < 0) return subPixelX + width;
            if (subPixelX >= width) return subPixelX - width;
            return subPixelX;
        }

        int WrapColumn(int col)
        {
            return ((col % Columns) + Columns) % Columns;
        }

        static G10_TileKind KindOf(char c)
        {
            switch (c)
            {
                case '#': return G10_TileKind.Wall;
                case '-': return G10_TileKind.GhostDoor;
                case 'T': return G10_TileKind.Tunnel;
                case '.':
                case 'o':
                case ' ': return G10_TileKind.Floor;
                default: throw new ArgumentException("Unknown maze character '" + c + "'.");
            }
        }
    }
}
