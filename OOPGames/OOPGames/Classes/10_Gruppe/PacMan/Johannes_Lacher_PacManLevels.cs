using System;
using System.Collections.Generic;

namespace OOPGames
{
    // All numbers that change from level to level. The game logic reads them from here
    // instead of asking which level it is.
    public sealed class G10_LevelSettings
    {
        public int Number { get; internal set; }
        // Speeds in percent of the full speed.
        public int PacManSpeed { get; internal set; }
        public int PacManFrightenedSpeed { get; internal set; }
        public int GhostSpeed { get; internal set; }
        public int GhostTunnelSpeed { get; internal set; }
        public int GhostFrightenedSpeed { get; internal set; }
        public int FrightenedTicks { get; internal set; }
        public G10_FruitKind Fruit { get; internal set; }
        public int FruitPoints { get; internal set; }
        // Scatter, chase, scatter, ... in ticks; the last entry lasts forever.
        public int[] ModeTicks { get; internal set; }
        public int InkyDotLimit { get; internal set; }
        public int ClydeDotLimit { get; internal set; }
        public int IdleReleaseTicks { get; internal set; }

        public int DotLimit(G10_GhostKind kind)
        {
            if (kind == G10_GhostKind.Inky) return InkyDotLimit;
            if (kind == G10_GhostKind.Clyde) return ClydeDotLimit;
            return 0;
        }
    }

    // Level data close to the arcade original.
    public static class G10_LevelTable
    {
        static readonly int[] FrightenedSeconds = { 6, 5, 4, 3, 2, 5, 2, 2, 1, 5, 2, 1, 1, 3, 1, 1, 0, 1 };
        static readonly G10_FruitKind[] Fruits =
        {
            G10_FruitKind.Cherry, G10_FruitKind.Strawberry, G10_FruitKind.Orange, G10_FruitKind.Orange,
            G10_FruitKind.Apple, G10_FruitKind.Apple, G10_FruitKind.Melon, G10_FruitKind.Melon,
            G10_FruitKind.Bell, G10_FruitKind.Bell, G10_FruitKind.Bell, G10_FruitKind.Bell
        };
        static readonly Dictionary<G10_FruitKind, int> FruitPoints = new Dictionary<G10_FruitKind, int>
        {
            { G10_FruitKind.Cherry, 100 },
            { G10_FruitKind.Strawberry, 300 },
            { G10_FruitKind.Orange, 500 },
            { G10_FruitKind.Apple, 700 },
            { G10_FruitKind.Melon, 1000 },
            { G10_FruitKind.Bell, 3000 },
            { G10_FruitKind.Key, 5000 }
        };

        static readonly Dictionary<int, G10_LevelSettings> _Cache = new Dictionary<int, G10_LevelSettings>();

        public static G10_LevelSettings ForLevel(int level)
        {
            if (level < 1) throw new ArgumentOutOfRangeException(nameof(level));
            G10_LevelSettings settings;
            if (!_Cache.TryGetValue(level, out settings))
            {
                settings = Create(level);
                _Cache[level] = settings;
            }
            return settings;
        }

        static G10_LevelSettings Create(int level)
        {
            int tier = level == 1 ? 0 : level <= 4 ? 1 : 2;
            G10_FruitKind fruit = level <= Fruits.Length ? Fruits[level - 1] : G10_FruitKind.Key;
            return new G10_LevelSettings
            {
                Number = level,
                PacManSpeed = new[] { 80, 90, 100 }[tier] - (level >= 21 ? 10 : 0),
                PacManFrightenedSpeed = new[] { 90, 95, 100 }[tier],
                GhostSpeed = new[] { 75, 85, 95 }[tier],
                GhostTunnelSpeed = new[] { 40, 45, 50 }[tier],
                GhostFrightenedSpeed = new[] { 50, 55, 60 }[tier],
                FrightenedTicks = G10_PacManConstants.Seconds(level <= FrightenedSeconds.Length ? FrightenedSeconds[level - 1] : 0),
                Fruit = fruit,
                FruitPoints = FruitPoints[fruit],
                ModeTicks = ModeTimetable(tier),
                InkyDotLimit = level == 1 ? 30 : 0,
                ClydeDotLimit = level == 1 ? 60 : level == 2 ? 50 : 0,
                IdleReleaseTicks = G10_PacManConstants.Seconds(level < 5 ? 4 : 3)
            };
        }

        static int[] ModeTimetable(int tier)
        {
            double[] seconds;
            if (tier == 0) seconds = new double[] { 7, 20, 7, 20, 5, 20, 5, 0 };
            else if (tier == 1) seconds = new double[] { 7, 20, 7, 20, 5, 1033, 0.04, 0 };
            else seconds = new double[] { 5, 20, 5, 20, 5, 1037, 0.04, 0 };

            int[] ticks = new int[seconds.Length];
            for (int i = 0; i < seconds.Length; i++) ticks[i] = Math.Max(1, G10_PacManConstants.Seconds(seconds[i]));
            return ticks;
        }
    }
}
