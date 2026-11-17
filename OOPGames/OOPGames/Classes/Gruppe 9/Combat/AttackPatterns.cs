using System;
using System.Collections.Generic;
using System.Windows;

namespace OOPGames
{
    public sealed class MeleeConeAttackPattern : IAttackPattern
    {
        public MeleeConeAttackPattern(
            double range, double halfAngleRadians, double durationSeconds)
        {
            if (range <= 0) throw new ArgumentOutOfRangeException(nameof(range));
            if (halfAngleRadians <= 0 || halfAngleRadians >= Math.PI)
            {
                throw new ArgumentOutOfRangeException(nameof(halfAngleRadians));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            Range = range;
            DebugHalfAngleRadians = halfAngleRadians;
            DurationSeconds = durationSeconds;
        }

        public double Range { get; }
        public double DebugHalfAngleRadians { get; }
        public double DebugRadius { get { return 0; } }
        public double DurationSeconds { get; }
        public string Name { get { return "Melee cone"; } }
        public AttackVisualKind VisualKind { get { return AttackVisualKind.Cone; } }

        public IAttackInstance BeginAttack(
            Entity source, double directionX, double directionY, Weapon weapon)
        {
            return new MeleeConeAttackInstance(
                source, directionX, directionY, weapon, this);
        }
    }

    public sealed class SweepingArcAttackPattern : IAttackPattern
    {
        public SweepingArcAttackPattern(
            double range, double totalArcRadians, double hitHalfAngleRadians,
            double durationSeconds)
        {
            if (range <= 0) throw new ArgumentOutOfRangeException(nameof(range));
            if (totalArcRadians <= 0 || totalArcRadians >= Math.PI * 2)
            {
                throw new ArgumentOutOfRangeException(nameof(totalArcRadians));
            }
            if (hitHalfAngleRadians <= 0 || hitHalfAngleRadians >= Math.PI / 2)
            {
                throw new ArgumentOutOfRangeException(nameof(hitHalfAngleRadians));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            Range = range;
            TotalArcRadians = totalArcRadians;
            HitHalfAngleRadians = hitHalfAngleRadians;
            DurationSeconds = durationSeconds;
        }

        public double Range { get; }
        public double TotalArcRadians { get; }
        public double HitHalfAngleRadians { get; }
        public double DurationSeconds { get; }
        public double DebugHalfAngleRadians { get { return TotalArcRadians / 2; } }
        public double DebugRadius { get { return 0; } }
        public string Name { get { return "Sweeping arc"; } }
        public AttackVisualKind VisualKind { get { return AttackVisualKind.Sweep; } }

        public IAttackInstance BeginAttack(
            Entity source, double directionX, double directionY, Weapon weapon)
        {
            return new SweepingArcAttackInstance(
                source, directionX, directionY, weapon, this);
        }
    }

    public sealed class ProjectileAttackPattern : IAttackPattern
    {
        public ProjectileAttackPattern(
            double range, double speed, double radius)
        {
            if (range <= 0) throw new ArgumentOutOfRangeException(nameof(range));
            if (speed <= 0) throw new ArgumentOutOfRangeException(nameof(speed));
            if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius));

            Range = range;
            Speed = speed;
            ProjectileRadius = radius;
        }

        public double Range { get; }
        public double Speed { get; }
        public double ProjectileRadius { get; }
        public double DebugHalfAngleRadians { get { return 0; } }
        public double DebugRadius { get { return ProjectileRadius; } }
        public string Name { get { return "Projectile"; } }
        public AttackVisualKind VisualKind { get { return AttackVisualKind.Projectile; } }

        public IAttackInstance BeginAttack(
            Entity source, double directionX, double directionY, Weapon weapon)
        {
            return new ProjectileAttackInstance(
                source, directionX, directionY, weapon, this);
        }
    }

    public sealed class SelfPulseAttackPattern : IAttackPattern
    {
        public SelfPulseAttackPattern(double radius, double durationSeconds)
        {
            if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius));
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            Range = radius;
            DurationSeconds = durationSeconds;
        }

        public double Range { get; }
        public double DurationSeconds { get; }
        public double DebugHalfAngleRadians { get { return Math.PI; } }
        public double DebugRadius { get { return Range; } }
        public string Name { get { return "Self pulse"; } }
        public AttackVisualKind VisualKind { get { return AttackVisualKind.Burst; } }

        public IAttackInstance BeginAttack(
            Entity source, double directionX, double directionY, Weapon weapon)
        {
            return new SelfPulseAttackInstance(source, weapon, this);
        }
    }

    internal static class AttackGeometry
    {
        public static bool IsInsideCone(
            Entity target, double centerX, double centerY, double directionX,
            double directionY, double range, double halfAngleRadians)
        {
            double offsetX = target.PositionX - centerX;
            double offsetY = target.PositionY - centerY;
            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            if (distance > range + target.Size / 2)
            {
                return false;
            }

            if (distance == 0)
            {
                return true;
            }

            double forward = offsetX * directionX + offsetY * directionY;
            return forward >= 0
                && Math.Abs(offsetX * directionY - offsetY * directionX)
                    <= forward * Math.Tan(halfAngleRadians) + target.Size / 2;
        }

        public static void NormalizeDirection(
            double x, double y, out double normalizedX, out double normalizedY)
        {
            double length = Math.Sqrt(x * x + y * y);
            normalizedX = length == 0 ? 1 : x / length;
            normalizedY = length == 0 ? 0 : y / length;
        }

        public static void RotateDirection(
            double directionX, double directionY, double angle,
            out double rotatedX, out double rotatedY)
        {
            rotatedX = directionX * Math.Cos(angle) - directionY * Math.Sin(angle);
            rotatedY = directionX * Math.Sin(angle) + directionY * Math.Cos(angle);
        }

        public static double DistanceToSegment(
            Point point, Point start, Point end)
        {
            double segmentX = end.X - start.X;
            double segmentY = end.Y - start.Y;
            double lengthSquared = segmentX * segmentX + segmentY * segmentY;
            if (lengthSquared == 0)
            {
                double dx = point.X - start.X;
                double dy = point.Y - start.Y;
                return Math.Sqrt(dx * dx + dy * dy);
            }

            double projection = ((point.X - start.X) * segmentX
                + (point.Y - start.Y) * segmentY) / lengthSquared;
            projection = Math.Max(0, Math.Min(1, projection));
            double closestX = start.X + projection * segmentX;
            double closestY = start.Y + projection * segmentY;
            double offsetX = point.X - closestX;
            double offsetY = point.Y - closestY;
            return Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
        }
    }

    internal sealed class MeleeConeAttackInstance : IAttackInstance
    {
        private readonly Entity _source;
        private readonly MeleeConeAttackPattern _pattern;
        private double _elapsedSeconds;
        private bool _resolved;

        public MeleeConeAttackInstance(
            Entity source, double directionX, double directionY,
            Weapon weapon, MeleeConeAttackPattern pattern)
        {
            _source = source;
            _pattern = pattern;
            AttackGeometry.NormalizeDirection(
                directionX, directionY, out double normalizedX, out double normalizedY);
            VisualState = new AttackVisualState(
                source, AttackVisualKind.Cone, weapon.Effect.Kind,
                weapon.AnimationPlaceholder, pattern.Range, 0,
                pattern.DebugHalfAngleRadians)
            {
                DirectionX = normalizedX,
                DirectionY = normalizedY
            };
        }

        public bool IsComplete { get { return _elapsedSeconds >= _pattern.DurationSeconds; } }
        public AttackVisualState VisualState { get; }

        public IReadOnlyList<Entity> Update(
            double deltaSeconds, IReadOnlyList<Entity> targets)
        {
            _elapsedSeconds += deltaSeconds;
            VisualState.CenterX = _source.PositionX;
            VisualState.CenterY = _source.PositionY;
            VisualState.Progress = Math.Min(1, _elapsedSeconds / _pattern.DurationSeconds);

            if (_resolved)
            {
                return Array.Empty<Entity>();
            }

            _resolved = true;
            List<Entity> hits = new List<Entity>();
            foreach (Entity target in targets)
            {
                if (AttackGeometry.IsInsideCone(
                    target, _source.PositionX, _source.PositionY,
                    VisualState.DirectionX, VisualState.DirectionY,
                    _pattern.Range, _pattern.DebugHalfAngleRadians))
                {
                    hits.Add(target);
                }
            }
            return hits;
        }
    }

    internal sealed class SweepingArcAttackInstance : IAttackInstance
    {
        private readonly Entity _source;
        private readonly SweepingArcAttackPattern _pattern;
        private readonly double _baseDirectionX;
        private readonly double _baseDirectionY;
        private double _elapsedSeconds;

        public SweepingArcAttackInstance(
            Entity source, double directionX, double directionY,
            Weapon weapon, SweepingArcAttackPattern pattern)
        {
            _source = source;
            _pattern = pattern;
            AttackGeometry.NormalizeDirection(
                directionX, directionY, out double normalizedX, out double normalizedY);
            _baseDirectionX = normalizedX;
            _baseDirectionY = normalizedY;
            VisualState = new AttackVisualState(
                source, AttackVisualKind.Sweep, weapon.Effect.Kind,
                weapon.AnimationPlaceholder, pattern.Range, 0,
                pattern.HitHalfAngleRadians);
        }

        public bool IsComplete { get { return _elapsedSeconds >= _pattern.DurationSeconds; } }
        public AttackVisualState VisualState { get; }

        public IReadOnlyList<Entity> Update(
            double deltaSeconds, IReadOnlyList<Entity> targets)
        {
            _elapsedSeconds += deltaSeconds;
            VisualState.Progress = Math.Min(1, _elapsedSeconds / _pattern.DurationSeconds);
            VisualState.CenterX = _source.PositionX;
            VisualState.CenterY = _source.PositionY;

            double sweepAngle = -_pattern.TotalArcRadians / 2
                + _pattern.TotalArcRadians * VisualState.Progress;
            AttackGeometry.RotateDirection(
                _baseDirectionX, _baseDirectionY, sweepAngle,
                out double directionX, out double directionY);
            VisualState.DirectionX = directionX;
            VisualState.DirectionY = directionY;

            List<Entity> hits = new List<Entity>();
            foreach (Entity target in targets)
            {
                if (AttackGeometry.IsInsideCone(
                    target, _source.PositionX, _source.PositionY,
                    directionX, directionY, _pattern.Range,
                    _pattern.HitHalfAngleRadians))
                {
                    hits.Add(target);
                }
            }
            return hits;
        }
    }

    internal sealed class ProjectileAttackInstance : IAttackInstance
    {
        private readonly Entity _source;
        private readonly ProjectileAttackPattern _pattern;
        private readonly double _directionX;
        private readonly double _directionY;
        private double _travelled;
        private bool _hasHit;

        public ProjectileAttackInstance(
            Entity source, double directionX, double directionY,
            Weapon weapon, ProjectileAttackPattern pattern)
        {
            _source = source;
            _pattern = pattern;
            AttackGeometry.NormalizeDirection(
                directionX, directionY, out double normalizedX, out double normalizedY);
            _directionX = normalizedX;
            _directionY = normalizedY;
            double startOffset = source.Size / 2 + pattern.ProjectileRadius;
            VisualState = new AttackVisualState(
                source, AttackVisualKind.Projectile, weapon.Effect.Kind,
                weapon.AnimationPlaceholder, pattern.Range,
                pattern.ProjectileRadius, 0)
            {
                DirectionX = _directionX,
                DirectionY = _directionY,
                ProjectileX = source.PositionX + _directionX * startOffset,
                ProjectileY = source.PositionY + _directionY * startOffset
            };
        }

        public bool IsComplete
        {
            get { return _hasHit || _travelled >= _pattern.Range || !_source.IsAlive; }
        }
        public AttackVisualState VisualState { get; }

        public IReadOnlyList<Entity> Update(
            double deltaSeconds, IReadOnlyList<Entity> targets)
        {
            if (IsComplete)
            {
                return Array.Empty<Entity>();
            }

            double previousX = VisualState.ProjectileX;
            double previousY = VisualState.ProjectileY;
            double step = Math.Min(
                _pattern.Speed * deltaSeconds, _pattern.Range - _travelled);
            VisualState.ProjectileX += _directionX * step;
            VisualState.ProjectileY += _directionY * step;
            _travelled += step;
            VisualState.Progress = Math.Min(1, _travelled / _pattern.Range);

            Entity firstHit = null;
            double firstHitDistance = double.MaxValue;
            foreach (Entity target in targets)
            {
                double distance = AttackGeometry.DistanceToSegment(
                    new Point(target.PositionX, target.PositionY),
                    new Point(previousX, previousY),
                    new Point(VisualState.ProjectileX, VisualState.ProjectileY));
                if (distance <= _pattern.ProjectileRadius + target.Size / 2
                    && distance < firstHitDistance)
                {
                    firstHit = target;
                    firstHitDistance = distance;
                }
            }

            if (firstHit == null)
            {
                return Array.Empty<Entity>();
            }

            _hasHit = true;
            return new[] { firstHit };
        }
    }

    internal sealed class SelfPulseAttackInstance : IAttackInstance
    {
        private readonly Entity _source;
        private readonly SelfPulseAttackPattern _pattern;
        private double _elapsedSeconds;
        private bool _resolved;

        public SelfPulseAttackInstance(
            Entity source, Weapon weapon, SelfPulseAttackPattern pattern)
        {
            _source = source;
            _pattern = pattern;
            VisualState = new AttackVisualState(
                source, AttackVisualKind.Burst, weapon.Effect.Kind,
                weapon.AnimationPlaceholder, pattern.Range, pattern.Range, Math.PI);
        }

        public bool IsComplete { get { return _elapsedSeconds >= _pattern.DurationSeconds; } }
        public AttackVisualState VisualState { get; }

        public IReadOnlyList<Entity> Update(
            double deltaSeconds, IReadOnlyList<Entity> targets)
        {
            _elapsedSeconds += deltaSeconds;
            VisualState.CenterX = _source.PositionX;
            VisualState.CenterY = _source.PositionY;
            VisualState.Progress = Math.Min(1, _elapsedSeconds / _pattern.DurationSeconds);

            if (_resolved)
            {
                return Array.Empty<Entity>();
            }

            _resolved = true;
            return new[] { _source };
        }
    }
}
