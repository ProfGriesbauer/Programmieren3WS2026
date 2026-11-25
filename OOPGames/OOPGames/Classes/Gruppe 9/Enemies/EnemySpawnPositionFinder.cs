using System;
using System.Collections.Generic;
using System.Windows;

namespace OOPGames
{
    // Finds free positions for enemies: anywhere in the arena out of the player's way,
    // inside spawn regions, or right beside the player (dev tool test spawn). The arena is
    // asked for its walkable area, the player and whether a hitbox overlaps an enemy, so the
    // finder always sees the current state.
    internal sealed class EnemySpawnPositionFinder
    {
        // Random tries before a spawn is given up (and skipped by the caller).
        public const int AttemptLimit = 10000;
        public const int RegionAttemptLimit = 2000;
        // Beside the player: rings of eight directions, this far apart beyond the hitboxes.
        private const int NearPlayerRings = 4;
        private const int NearPlayerDirections = 8;
        private const double NearPlayerGap = 40;

        private readonly Func<Rect> _movementArea;
        private readonly Func<PlayerCharacter> _player;
        private readonly Func<Rect, bool> _overlapsEnemy;

        public EnemySpawnPositionFinder(Func<Rect> movementArea,
            Func<PlayerCharacter> player, Func<Rect, bool> overlapsEnemy)
        {
            _movementArea = movementArea ?? throw new ArgumentNullException(nameof(movementArea));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _overlapsEnemy = overlapsEnemy ?? throw new ArgumentNullException(nameof(overlapsEnemy));
        }

        // A random position inside the arena (and inside one of the regions, if any) at
        // least minimumDistance from the player that overlaps no enemy; null when none is
        // found.
        public Point? FindAwayFromPlayer(double size, string spriteSheet,
            double minimumDistance, IReadOnlyList<ArenaMapRegion> regions = null)
        {
            return regions != null && regions.Count > 0
                ? FindInRegions(size, spriteSheet, minimumDistance, regions)
                : FindInArena(size, spriteSheet, minimumDistance);
        }

        // The first free position on rings around the player that overlaps neither the
        // player nor an enemy; null when there is none.
        public Point? FindBesidePlayer(double size, string spriteSheet)
        {
            PlayerCharacter player = _player();
            Rect hitbox = PursuerEnemy.GetHitboxOffsetForSize(size, spriteSheet);
            Rect playerHitbox = player.Hitbox;
            Rect area = _movementArea();
            double baseDistance = Math.Max(playerHitbox.Width, playerHitbox.Height) / 2
                + Math.Max(hitbox.Width, hitbox.Height) / 2 + NearPlayerGap;
            for (int ring = 1; ring <= NearPlayerRings; ring++)
            {
                for (int step = 0; step < NearPlayerDirections; step++)
                {
                    double angle = step * Math.PI * 2 / NearPlayerDirections;
                    double x = player.PositionX + Math.Cos(angle) * baseDistance * ring;
                    double y = player.PositionY + Math.Sin(angle) * baseDistance * ring;
                    Rect placed = Place(hitbox, x, y);
                    if (area.Contains(placed) && !_overlapsEnemy(placed)
                        && !AttackGeometry.RectsOverlap(placed, playerHitbox))
                    {
                        return new Point(x, y);
                    }
                }
            }
            return null;
        }

        private Point? FindInArena(double size, string spriteSheet, double minimumDistance)
        {
            PlayerCharacter player = _player();
            Rect hitbox = PursuerEnemy.GetHitboxOffsetForSize(size, spriteSheet);
            Rect area = _movementArea();
            double minimumX = area.Left - hitbox.Left;
            double maximumX = area.Right - hitbox.Right;
            double minimumY = area.Top - hitbox.Top;
            double maximumY = area.Bottom - hitbox.Bottom;
            if (maximumX < minimumX || maximumY < minimumY)
            {
                return null;
            }

            for (int attempt = 0; attempt < AttemptLimit; attempt++)
            {
                double x = minimumX + Random.Shared.NextDouble() * (maximumX - minimumX);
                double y = minimumY + Random.Shared.NextDouble() * (maximumY - minimumY);
                if (IsFarEnough(player, x, y, minimumDistance)
                    && !_overlapsEnemy(Place(hitbox, x, y)))
                {
                    return new Point(x, y);
                }
            }
            return null;
        }

        private Point? FindInRegions(double size, string spriteSheet,
            double minimumDistance, IReadOnlyList<ArenaMapRegion> regions)
        {
            PlayerCharacter player = _player();
            Rect hitbox = PursuerEnemy.GetHitboxOffsetForSize(size, spriteSheet);
            Rect area = _movementArea();
            double totalArea = 0;
            foreach (ArenaMapRegion region in regions)
            {
                totalArea += region.Area;
            }

            for (int attempt = 0; attempt < RegionAttemptLimit; attempt++)
            {
                Point point = PickRegion(regions, totalArea).GetRandomPoint(Random.Shared);
                Rect placed = Place(hitbox, point.X, point.Y);
                if (area.Contains(placed)
                    && IsFarEnough(player, point.X, point.Y, minimumDistance)
                    && !_overlapsEnemy(placed))
                {
                    return point;
                }
            }
            return null;
        }

        // Larger shapes get proportionally more spawn points.
        private static ArenaMapRegion PickRegion(
            IReadOnlyList<ArenaMapRegion> regions, double totalArea)
        {
            double roll = Random.Shared.NextDouble() * totalArea;
            foreach (ArenaMapRegion region in regions)
            {
                roll -= region.Area;
                if (roll < 0)
                {
                    return region;
                }
            }
            return regions[regions.Count - 1];
        }

        private static bool IsFarEnough(
            PlayerCharacter player, double x, double y, double minimumDistance)
        {
            double offsetX = x - player.PositionX;
            double offsetY = y - player.PositionY;
            return offsetX * offsetX + offsetY * offsetY >= minimumDistance * minimumDistance;
        }

        private static Rect Place(Rect hitboxOffset, double x, double y)
        {
            return new Rect(x + hitboxOffset.X, y + hitboxOffset.Y,
                hitboxOffset.Width, hitboxOffset.Height);
        }
    }
}
