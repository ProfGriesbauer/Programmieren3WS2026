using System;
using System.Collections.Generic;

namespace OOPGames
{
    public static class G10_CoinConfig
    {
        public const int Columns = 19;
        public const int Rows = 13;
        public const int StartLives = 3;
        // The hub ticks every 40 ms.
        public const int TicksPerSecond = 25;
        public const int ReadyTicks = 38;
        public const int HitTicks = 38;
        public const int LevelClearedTicks = 63;
        public const int InvulnerableTicks = 50;
        public const int CoinsOnField = 3;
        public const int CoinPoints = 10;
        public const int DiamondPoints = 100;
        public const int TimeBonusPerSecond = 10;
        public const int DiamondInterval = 250;
        public const int DiamondLifetime = 125;
        // Dust bunnies do not appear closer to the player than this (in tiles).
        public const int SpawnDistance = 6;
    }

    // Settings of one level. The layouts repeat, but every round gets harder.
    public sealed class G10_CoinLevel
    {
        public int Number { get; private set; }
        public string[] Layout { get; private set; }
        public int DustBunnies { get; private set; }
        // A dust bunny moves one tile every this many ticks.
        public int BunnyStepTicks { get; private set; }
        public int CoinsNeeded { get; private set; }
        public int TimeTicks { get; private set; }

        public static G10_CoinLevel ForLevel(int number)
        {
            return new G10_CoinLevel
            {
                Number = number,
                Layout = G10_CoinLayouts.All[(number - 1) % G10_CoinLayouts.All.Length],
                DustBunnies = Math.Min(5, 1 + number / 2),
                BunnyStepTicks = Math.Max(3, 8 - number / 2),
                CoinsNeeded = Math.Min(20, 8 + 2 * number),
                TimeTicks = 45 * G10_CoinConfig.TicksPerSecond
            };
        }
    }

    // '#' toy block, '.' floor, 'P' start of the player. Outside the field counts as wall.
    // Corridors are at least two tiles wide: dust bunnies move diagonally and cannot follow
    // a corridor of width one, so it would be a safe hiding place.
    public static class G10_CoinLayouts
    {
        public static readonly string[][] All =
        {
            new[]
            {
                "...................",
                "...................",
                "..##...........##..",
                "..#.............#..",
                "...................",
                "......#.....#......",
                ".........P.........",
                "......#.....#......",
                "...................",
                "..#.............#..",
                "..##...........##..",
                "...................",
                "..................."
            },
            new[]
            {
                "...................",
                "...................",
                "..######...######..",
                "...................",
                "...................",
                "...................",
                "..######.P.######..",
                "...................",
                "...................",
                "...................",
                "..######...######..",
                "...................",
                "..................."
            },
            new[]
            {
                "...................",
                "...................",
                "...#...#...#...#...",
                "...................",
                "...#...#...#...#...",
                "...................",
                ".........P.........",
                "...................",
                "...#...#...#...#...",
                "...................",
                "...#...#...#...#...",
                "...................",
                "..................."
            },
            new[]
            {
                "...................",
                "...................",
                "..####.......####..",
                "..#.....###.....#..",
                "..#.............#..",
                ".......#...#.......",
                ".........P.........",
                ".......#...#.......",
                "..#.............#..",
                "..#.....###.....#..",
                "..####.......####..",
                "...................",
                "..................."
            }
        };
    }

    // The walls of one level.
    public sealed class G10_CoinArena
    {
        readonly bool[,] _Walls;

        public G10_CoinArena(string[] layout)
        {
            Rows = layout.Length;
            Columns = layout[0].Length;
            _Walls = new bool[Columns, Rows];
            for (int row = 0; row < Rows; row++)
            {
                if (layout[row].Length != Columns) throw new ArgumentException("All rows of a layout need the same length.");
                for (int col = 0; col < Columns; col++)
                {
                    char symbol = layout[row][col];
                    _Walls[col, row] = symbol == '#';
                    if (symbol == 'P') PlayerStart = new G10_Tile(col, row);
                }
            }
        }

        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public G10_Tile PlayerStart { get; private set; }

        public bool IsInside(G10_Tile tile)
        {
            return tile.Col >= 0 && tile.Col < Columns && tile.Row >= 0 && tile.Row < Rows;
        }

        public bool IsWall(G10_Tile tile)
        {
            return IsInside(tile) && _Walls[tile.Col, tile.Row];
        }

        public bool IsBlocked(G10_Tile tile)
        {
            return !IsInside(tile) || _Walls[tile.Col, tile.Row];
        }

        public List<G10_Tile> FloorTiles()
        {
            List<G10_Tile> floor = new List<G10_Tile>();
            for (int row = 0; row < Rows; row++)
                for (int col = 0; col < Columns; col++)
                    if (!_Walls[col, row]) floor.Add(new G10_Tile(col, row));
            return floor;
        }
    }
}
