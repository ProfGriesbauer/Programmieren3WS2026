using System;

namespace OOPGames
{
    public static class G10_PacManConstants
    {
        public const int TileSize = 8;
        // Positions are stored in sub-pixels so movement stays exact with whole numbers.
        public const int SubPixels = 16;
        public const int TileSubPixels = TileSize * SubPixels;
        public const int TicksPerSecond = 25;
        // 100 % speed in the original is about 75.75 px/s, i.e. 3.03 px per 40 ms tick.
        // Stored as sub-pixels per tick times 100 to keep fractions.
        public const int FullSpeedSubPixelsTimes100 = 4848;

        public static int Seconds(double seconds)
        {
            return (int)Math.Round(seconds * TicksPerSecond);
        }
    }

    public enum G10_TileKind
    {
        Wall,
        Floor,
        GhostDoor,
        // Floor in the side tunnels; ghosts are slowed down here.
        Tunnel
    }

    public struct G10_Tile : IEquatable<G10_Tile>
    {
        public G10_Tile(int col, int row)
        {
            Col = col;
            Row = row;
        }

        public int Col { get; private set; }
        public int Row { get; private set; }

        // Centre of the tile in maze pixels.
        public int CenterX { get { return Col * G10_PacManConstants.TileSize + G10_PacManConstants.TileSize / 2; } }
        public int CenterY { get { return Row * G10_PacManConstants.TileSize + G10_PacManConstants.TileSize / 2; } }

        public static G10_Tile FromPixel(int x, int y)
        {
            return new G10_Tile(FloorDiv(x, G10_PacManConstants.TileSize), FloorDiv(y, G10_PacManConstants.TileSize));
        }

        public G10_Tile Neighbour(G10_Direction direction)
        {
            return Offset(direction.DeltaX(), direction.DeltaY());
        }

        public G10_Tile Offset(int cols, int rows)
        {
            return new G10_Tile(Col + cols, Row + rows);
        }

        public int DistanceSquaredTo(G10_Tile other)
        {
            int dx = Col - other.Col;
            int dy = Row - other.Row;
            return dx * dx + dy * dy;
        }

        public bool Equals(G10_Tile other) { return Col == other.Col && Row == other.Row; }
        public override bool Equals(object obj) { return obj is G10_Tile && Equals((G10_Tile)obj); }
        public override int GetHashCode() { return Col * 397 ^ Row; }
        public static bool operator ==(G10_Tile a, G10_Tile b) { return a.Equals(b); }
        public static bool operator !=(G10_Tile a, G10_Tile b) { return !a.Equals(b); }
        public override string ToString() { return "(" + Col + ", " + Row + ")"; }

        static int FloorDiv(int value, int divisor)
        {
            int result = value / divisor;
            if (value % divisor != 0 && value < 0) result--;
            return result;
        }
    }

    // One score popup ("400" after eating a ghost) at a maze pixel position.
    public struct G10_PointsPopup
    {
        public G10_PointsPopup(int pixelX, int pixelY, int points)
        {
            PixelX = pixelX;
            PixelY = pixelY;
            Points = points;
        }

        public int PixelX { get; private set; }
        public int PixelY { get; private set; }
        public int Points { get; private set; }
    }
}
