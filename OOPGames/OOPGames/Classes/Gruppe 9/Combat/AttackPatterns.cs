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

    public sealed class SpinningAttackPattern : IAttackPattern
    {
        public SpinningAttackPattern(
            double range, int rotations, double hitHalfAngleRadians,
            double durationSeconds)
        {
            if (range <= 0) throw new ArgumentOutOfRangeException(nameof(range));
            if (rotations <= 0) throw new ArgumentOutOfRangeException(nameof(rotations));
            if (hitHalfAngleRadians <= 0 || hitHalfAngleRadians >= Math.PI / 2)
            {
                throw new ArgumentOutOfRangeException(nameof(hitHalfAngleRadians));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            Range = range;
            Rotations = rotations;
            HitHalfAngleRadians = hitHalfAngleRadians;
            DurationSeconds = durationSeconds;
        }

        public double Range { get; }
        public int Rotations { get; }
        public double HitHalfAngleRadians { get; }
        public double DurationSeconds { get; }
        public double DebugHalfAngleRadians { get { return Math.PI; } }
        public double DebugRadius { get { return 0; } }
        public string Name { get { return "Spinning slash"; } }
        public AttackVisualKind VisualKind { get { return AttackVisualKind.Sweep; } }

        public IAttackInstance BeginAttack(
            Entity source, double directionX, double directionY, Weapon weapon)
        {
            return new SpinningAttackInstance(
                source, directionX, directionY, weapon, this);
        }
    }

    public sealed class ProjectileAttackPattern : IAttackPattern
    {
        public ProjectileAttackPattern(
            double range, double speed, double radius,
            double autoAimHalfAngleRadians = 0,
            double homingTurnRateRadiansPerSecond = 0)
        {
            if (range <= 0) throw new ArgumentOutOfRangeException(nameof(range));
            if (speed <= 0) throw new ArgumentOutOfRangeException(nameof(speed));
            if (radius <= 0) throw new ArgumentOutOfRangeException(nameof(radius));
            if (autoAimHalfAngleRadians < 0
                || autoAimHalfAngleRadians > Math.PI)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(autoAimHalfAngleRadians));
            }
            if (homingTurnRateRadiansPerSecond < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(homingTurnRateRadiansPerSecond));
            }

            Range = range;
            Speed = speed;
            ProjectileRadius = radius;
            AutoAimHalfAngleRadians = autoAimHalfAngleRadians;
            HomingTurnRateRadiansPerSecond = homingTurnRateRadiansPerSecond;
        }

        public double Range { get; }
        public double Speed { get; }
        public double ProjectileRadius { get; }
        public double AutoAimHalfAngleRadians { get; }
        public double HomingTurnRateRadiansPerSecond { get; }
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

    public sealed class RadialAttackPattern : IAttackPattern
    {
        public RadialAttackPattern(double radius, double durationSeconds)
        {
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }
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
        public string Name { get { return "Radial blast"; } }
        public AttackVisualKind VisualKind { get { return AttackVisualKind.Burst; } }

        public IAttackInstance BeginAttack(
            Entity source, double directionX, double directionY, Weapon weapon)
        {
            return new RadialAttackInstance(source, weapon, this);
        }
    }

    public static class AttackGeometry
    {
        // Part of a blade weapon's reach in front of the body that the blade does not cover:
        // the blade starts at the grip and its tip lies at reach - BladeGripGap.
        public const double BladeGripGap = 8;
        // Half the blade width per unit of blade length (3 pixels wide, 23 pixels long).
        public const double BladeHalfWidthPerLength = 1.5 / 23;
        // Angle step a blade may sweep per hit test, in units of movement at its tip.
        private const double BladeSampleDistance = 6;

        // Number of points along the axis of a cone used to find hitboxes that cross it.
        private const int ConeAxisSamples = 8;

        public static bool RectsOverlap(Rect first, Rect second)
        {
            return first.Left < second.Right && second.Left < first.Right
                && first.Top < second.Bottom && second.Top < first.Bottom;
        }

        public static Point ClosestPointOnRect(Point point, Rect rect)
        {
            return new Point(
                Math.Max(rect.Left, Math.Min(rect.Right, point.X)),
                Math.Max(rect.Top, Math.Min(rect.Bottom, point.Y)));
        }

        public static double DistanceToRect(Point point, Rect rect)
        {
            Point closest = ClosestPointOnRect(point, rect);
            return (point - closest).Length;
        }

        public static double DistanceBetweenRects(Rect first, Rect second)
        {
            double gapX = Math.Max(0, Math.Max(first.Left - second.Right, second.Left - first.Right));
            double gapY = Math.Max(0, Math.Max(first.Top - second.Bottom, second.Top - first.Bottom));
            return Math.Sqrt(gapX * gapX + gapY * gapY);
        }

        public static double SegmentDistanceToRect(Point start, Point end, Rect rect)
        {
            if (rect.Contains(start) || rect.Contains(end))
            {
                return 0;
            }

            Point topLeft = rect.TopLeft;
            Point topRight = rect.TopRight;
            Point bottomRight = rect.BottomRight;
            Point bottomLeft = rect.BottomLeft;
            if (SegmentsIntersect(start, end, topLeft, topRight)
                || SegmentsIntersect(start, end, topRight, bottomRight)
                || SegmentsIntersect(start, end, bottomRight, bottomLeft)
                || SegmentsIntersect(start, end, bottomLeft, topLeft))
            {
                return 0;
            }

            return Math.Min(
                Math.Min(DistanceToRect(start, rect), DistanceToRect(end, rect)),
                Math.Min(
                    Math.Min(DistanceToSegment(topLeft, start, end),
                        DistanceToSegment(topRight, start, end)),
                    Math.Min(DistanceToSegment(bottomRight, start, end),
                        DistanceToSegment(bottomLeft, start, end))));
        }

        private static bool SegmentsIntersect(Point a, Point b, Point c, Point d)
        {
            double d1 = Cross(c, d, a);
            double d2 = Cross(c, d, b);
            double d3 = Cross(a, b, c);
            double d4 = Cross(a, b, d);
            return ((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0))
                && ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0));
        }

        private static double Cross(Point origin, Point first, Point second)
        {
            return (first.X - origin.X) * (second.Y - origin.Y)
                - (first.Y - origin.Y) * (second.X - origin.X);
        }

        // Center distance at which two hitboxes (relative to their positions) start to touch
        // when the second lies in the unit direction from the first.
        public static double GetContactDistance(
            Rect first, Rect second, double directionX, double directionY)
        {
            double limitX = double.PositiveInfinity;
            double limitY = double.PositiveInfinity;
            if (directionX > 1e-12) limitX = (first.Right - second.Left) / directionX;
            else if (directionX < -1e-12) limitX = (first.Left - second.Right) / directionX;
            if (directionY > 1e-12) limitY = (first.Bottom - second.Top) / directionY;
            else if (directionY < -1e-12) limitY = (first.Top - second.Bottom) / directionY;
            double contact = Math.Min(limitX, limitY);
            return double.IsInfinity(contact) ? 0 : Math.Max(0, contact);
        }

        // How far the second hitbox must move in the unit direction so it no longer overlaps
        // the first; infinite when that direction cannot separate them.
        public static double GetSeparationDistance(
            Rect first, Rect second, double directionX, double directionY)
        {
            double distance = double.PositiveInfinity;
            if (directionX > 1e-12)
                distance = Math.Min(distance, (first.Right - second.Left) / directionX);
            else if (directionX < -1e-12)
                distance = Math.Min(distance, (first.Left - second.Right) / directionX);
            if (directionY > 1e-12)
                distance = Math.Min(distance, (first.Bottom - second.Top) / directionY);
            else if (directionY < -1e-12)
                distance = Math.Min(distance, (first.Top - second.Bottom) / directionY);
            return double.IsInfinity(distance) ? distance : Math.Max(0, distance);
        }

        public static PlayerFacing GetFacingForDirection(double directionX, double directionY)
        {
            return Math.Abs(directionX) >= Math.Abs(directionY)
                ? (directionX < 0 ? PlayerFacing.Left : PlayerFacing.Right)
                : (directionY < 0 ? PlayerFacing.Back : PlayerFacing.Front);
        }

        // Places the blade of an entity: the grip lies in its hand for the given facing, and
        // the blade points from the grip at the point of the direction at the given reach.
        public static void SetBlade(
            AttackVisualState visual, Entity source, PlayerFacing facing,
            double directionX, double directionY, double reach)
        {
            Vector hand = source.GetHandOffset(facing);
            Vector blade = new Vector(directionX, directionY) * reach - hand;
            if (blade.Length < 1e-9)
            {
                blade = new Vector(directionX, directionY);
            }
            blade.Normalize();
            double length = Math.Max(0, reach - BladeGripGap);
            visual.BladeStartX = source.PositionX + hand.X;
            visual.BladeStartY = source.PositionY + hand.Y;
            visual.BladeEndX = visual.BladeStartX + blade.X * length;
            visual.BladeEndY = visual.BladeStartY + blade.Y * length;
            visual.BladeHalfWidth = Math.Max(1e-6, length * BladeHalfWidthPerLength);
        }

        public static bool IsTouchingBlade(Entity target, AttackVisualState visual)
        {
            return SegmentDistanceToRect(
                    new Point(visual.BladeStartX, visual.BladeStartY),
                    new Point(visual.BladeEndX, visual.BladeEndY),
                    target.Hitbox)
                <= visual.BladeHalfWidth;
        }

        // Number of intermediate blade positions needed so a fast swing cannot skip a target.
        public static int GetBladeSampleCount(double angleRadians, double reach)
        {
            return Math.Max(1, (int)Math.Ceiling(
                Math.Abs(angleRadians) * reach / BladeSampleDistance));
        }

        // True when any part of the target's hitbox lies inside the cone.
        public static bool IsInsideCone(
            Entity target, double centerX, double centerY, double directionX,
            double directionY, double range, double halfAngleRadians)
        {
            Rect hitbox = target.Hitbox;
            Point center = new Point(centerX, centerY);
            if (hitbox.Contains(center))
            {
                return true;
            }
            if (DistanceToRect(center, hitbox) > range)
            {
                return false;
            }

            List<Point> candidates = new List<Point>
            {
                ClosestPointOnRect(center, hitbox),
                new Point(hitbox.X + hitbox.Width / 2, hitbox.Y + hitbox.Height / 2),
                hitbox.TopLeft, hitbox.TopRight, hitbox.BottomLeft, hitbox.BottomRight
            };
            for (int sample = 1; sample <= ConeAxisSamples; sample++)
            {
                double along = range * sample / ConeAxisSamples;
                candidates.Add(ClosestPointOnRect(new Point(
                    centerX + directionX * along, centerY + directionY * along), hitbox));
            }

            double tangent = Math.Tan(halfAngleRadians);
            foreach (Point point in candidates)
            {
                double offsetX = point.X - centerX;
                double offsetY = point.Y - centerY;
                double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
                if (distance > range + 1e-9)
                {
                    continue;
                }
                if (distance == 0)
                {
                    return true;
                }

                double forward = offsetX * directionX + offsetY * directionY;
                if (forward >= 0
                    && Math.Abs(offsetX * directionY - offsetY * directionX)
                        <= forward * tangent + 1e-9)
                {
                    return true;
                }
            }
            return false;
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
        private readonly PlayerFacing _facing;
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
            // The player keeps the strike facing during the whole swing, so does the hand.
            _facing = AttackGeometry.GetFacingForDirection(normalizedX, normalizedY);
            VisualState = new AttackVisualState(
                source, AttackVisualKind.Sweep, weapon.Effect.Kind,
                weapon.AnimationPlaceholder, pattern.Range, 0,
                pattern.HitHalfAngleRadians);
            SetDirection(GetSweepAngle(0));
        }

        public bool IsComplete { get { return _elapsedSeconds >= _pattern.DurationSeconds; } }
        public AttackVisualState VisualState { get; }

        // The damage area is the blade itself, swept from the previous to the current
        // position of the swing.
        public IReadOnlyList<Entity> Update(
            double deltaSeconds, IReadOnlyList<Entity> targets)
        {
            double previousAngle = GetSweepAngle(VisualState.Progress);
            _elapsedSeconds += deltaSeconds;
            VisualState.Progress = Math.Min(1, _elapsedSeconds / _pattern.DurationSeconds);
            VisualState.CenterX = _source.PositionX;
            VisualState.CenterY = _source.PositionY;
            double angle = GetSweepAngle(VisualState.Progress);

            List<Entity> hits = new List<Entity>();
            int samples = AttackGeometry.GetBladeSampleCount(angle - previousAngle, _pattern.Range);
            for (int sample = 1; sample <= samples; sample++)
            {
                SetDirection(previousAngle + (angle - previousAngle) * sample / samples);
                foreach (Entity target in targets)
                {
                    if (!hits.Contains(target)
                        && AttackGeometry.IsTouchingBlade(target, VisualState))
                    {
                        hits.Add(target);
                    }
                }
            }
            return hits;
        }

        private double GetSweepAngle(double progress)
        {
            return -_pattern.TotalArcRadians / 2 + _pattern.TotalArcRadians * progress;
        }

        private void SetDirection(double sweepAngle)
        {
            AttackGeometry.RotateDirection(
                _baseDirectionX, _baseDirectionY, sweepAngle,
                out double directionX, out double directionY);
            VisualState.DirectionX = directionX;
            VisualState.DirectionY = directionY;
            AttackGeometry.SetBlade(
                VisualState, _source, _facing, directionX, directionY, _pattern.Range);
        }
    }

    internal sealed class SpinningAttackInstance : IAttackInstance
    {
        private readonly Entity _source;
        private readonly SpinningAttackPattern _pattern;
        private readonly double _baseDirectionX;
        private readonly double _baseDirectionY;
        private readonly Dictionary<Entity, int> _lastHitRotation =
            new Dictionary<Entity, int>();
        private double _elapsedSeconds;

        public SpinningAttackInstance(
            Entity source, double directionX, double directionY,
            Weapon weapon, SpinningAttackPattern pattern)
        {
            _source = source;
            _pattern = pattern;
            AttackGeometry.NormalizeDirection(
                directionX, directionY, out double baseDirectionX,
                out double baseDirectionY);
            _baseDirectionX = baseDirectionX;
            _baseDirectionY = baseDirectionY;
            VisualState = new AttackVisualState(
                source, AttackVisualKind.Sweep, weapon.Effect.Kind,
                weapon.AnimationPlaceholder, pattern.Range, 0,
                pattern.HitHalfAngleRadians);
            SetDirection(0);
        }

        public bool IsComplete
        {
            get { return _elapsedSeconds >= _pattern.DurationSeconds; }
        }
        public AttackVisualState VisualState { get; }

        // The damage area is the blade itself, which revolves with the player; each target
        // can be hit once per revolution.
        public IReadOnlyList<Entity> Update(
            double deltaSeconds, IReadOnlyList<Entity> targets)
        {
            const double fullRotation = Math.PI * 2;
            double previousAngle = fullRotation * _pattern.Rotations * VisualState.Progress;
            _elapsedSeconds += deltaSeconds;
            VisualState.Progress = Math.Min(
                1, _elapsedSeconds / _pattern.DurationSeconds);
            VisualState.CenterX = _source.PositionX;
            VisualState.CenterY = _source.PositionY;
            double angle = fullRotation * _pattern.Rotations * VisualState.Progress;

            List<Entity> hits = new List<Entity>();
            int samples = AttackGeometry.GetBladeSampleCount(angle - previousAngle, _pattern.Range);
            for (int sample = 1; sample <= samples; sample++)
            {
                double sampleAngle = previousAngle + (angle - previousAngle) * sample / samples;
                int rotation = Math.Min(_pattern.Rotations - 1,
                    (int)Math.Floor(sampleAngle / fullRotation));
                SetDirection(sampleAngle);
                foreach (Entity target in targets)
                {
                    int lastHitRotation = _lastHitRotation.TryGetValue(
                        target, out int previousHitRotation)
                        ? previousHitRotation
                        : -1;
                    if (rotation > lastHitRotation
                        && AttackGeometry.IsTouchingBlade(target, VisualState))
                    {
                        _lastHitRotation[target] = rotation;
                        hits.Add(target);
                    }
                }
            }
            return hits;
        }

        // The spin shows the view of the current direction, so the grip moves to the hand
        // of that view.
        private void SetDirection(double angle)
        {
            AttackGeometry.RotateDirection(
                _baseDirectionX, _baseDirectionY, angle,
                out double directionX, out double directionY);
            VisualState.DirectionX = directionX;
            VisualState.DirectionY = directionY;
            AttackGeometry.SetBlade(
                VisualState, _source,
                AttackGeometry.GetFacingForDirection(directionX, directionY),
                directionX, directionY, _pattern.Range);
        }
    }

    internal sealed class ProjectileAttackInstance : IAttackInstance
    {
        private readonly Entity _source;
        private readonly ProjectileAttackPattern _pattern;
        private readonly double _launchDirectionX;
        private readonly double _launchDirectionY;
        private double _directionX;
        private double _directionY;
        private Entity _autoAimTarget;
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
            _launchDirectionX = normalizedX;
            _launchDirectionY = normalizedY;
            _directionX = normalizedX;
            _directionY = normalizedY;
            double startOffset = source.Size / 2 + pattern.ProjectileRadius;
            Point launchPoint = source.GetProjectileLaunchPoint(
                weapon, normalizedX, normalizedY, startOffset);
            VisualState = new AttackVisualState(
                source, AttackVisualKind.Projectile, weapon.Effect.Kind,
                weapon.AnimationPlaceholder, pattern.Range,
                pattern.ProjectileRadius, 0)
            {
                DirectionX = _directionX,
                DirectionY = _directionY,
                ProjectileX = launchPoint.X,
                ProjectileY = launchPoint.Y
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

            if (_pattern.AutoAimHalfAngleRadians > 0)
            {
                if (_autoAimTarget == null || !_autoAimTarget.IsAlive)
                {
                    _autoAimTarget = FindAutoAimTarget(targets);
                }
                if (_autoAimTarget != null)
                {
                    SteerTowardTarget(deltaSeconds);
                }
            }

            double previousX = VisualState.ProjectileX;
            double previousY = VisualState.ProjectileY;
            double step = Math.Min(
                _pattern.Speed * deltaSeconds, _pattern.Range - _travelled);
            VisualState.ProjectileX += _directionX * step;
            VisualState.ProjectileY += _directionY * step;
            VisualState.DirectionX = _directionX;
            VisualState.DirectionY = _directionY;
            _travelled += step;
            VisualState.Progress = Math.Min(1, _travelled / _pattern.Range);

            Entity firstHit = null;
            double firstHitDistance = double.MaxValue;
            foreach (Entity target in targets)
            {
                double distance = AttackGeometry.SegmentDistanceToRect(
                    new Point(previousX, previousY),
                    new Point(VisualState.ProjectileX, VisualState.ProjectileY),
                    target.Hitbox);
                if (distance <= _pattern.ProjectileRadius
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

        private Entity FindAutoAimTarget(IReadOnlyList<Entity> targets)
        {
            Entity nearestTarget = null;
            double nearestDistanceSquared = double.MaxValue;
            double minimumDot = Math.Cos(_pattern.AutoAimHalfAngleRadians);
            foreach (Entity target in targets)
            {
                double offsetX = target.PositionX - _source.PositionX;
                double offsetY = target.PositionY - _source.PositionY;
                double distanceSquared = offsetX * offsetX + offsetY * offsetY;
                if (AttackGeometry.DistanceToRect(
                    new Point(_source.PositionX, _source.PositionY), target.Hitbox)
                    > _pattern.Range)
                {
                    continue;
                }
                if (distanceSquared == 0)
                {
                    return target;
                }

                double distance = Math.Sqrt(distanceSquared);
                double dot = (_launchDirectionX * offsetX
                    + _launchDirectionY * offsetY) / distance;
                if (dot >= minimumDot && distanceSquared < nearestDistanceSquared)
                {
                    nearestTarget = target;
                    nearestDistanceSquared = distanceSquared;
                }
            }

            return nearestTarget;
        }

        private void SteerTowardTarget(double deltaSeconds)
        {
            double targetX = _autoAimTarget.PositionX - VisualState.ProjectileX;
            double targetY = _autoAimTarget.PositionY - VisualState.ProjectileY;
            double targetLength = Math.Sqrt(targetX * targetX + targetY * targetY);
            if (targetLength == 0)
            {
                return;
            }

            targetX /= targetLength;
            targetY /= targetLength;
            double dot = Math.Max(-1, Math.Min(1,
                _directionX * targetX + _directionY * targetY));
            double angle = Math.Atan2(
                _directionX * targetY - _directionY * targetX, dot);
            double maximumTurn = _pattern.HomingTurnRateRadiansPerSecond
                * deltaSeconds;
            double turn = Math.Sign(angle)
                * Math.Min(Math.Abs(angle), maximumTurn);
            AttackGeometry.RotateDirection(
                _directionX, _directionY, turn,
                out _directionX, out _directionY);
            AttackGeometry.NormalizeDirection(
                _directionX, _directionY, out _directionX, out _directionY);
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
                weapon.AnimationPlaceholder, pattern.Range, 0, Math.PI);
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
            VisualState.Radius = _pattern.Range * VisualState.Progress;

            if (_resolved)
            {
                return Array.Empty<Entity>();
            }

            _resolved = true;
            return new[] { _source };
        }
    }

    // A wave that starts at the feet of the source and runs outwards at constant speed: its
    // radius grows from 0 to Range over the duration, and each target is hit once when the
    // wave front reaches its hitbox.
    internal sealed class RadialAttackInstance : IAttackInstance
    {
        private readonly RadialAttackPattern _pattern;
        private readonly HashSet<Entity> _hitTargets = new HashSet<Entity>();
        private readonly Point _origin;
        private double _elapsedSeconds;

        public RadialAttackInstance(
            Entity source, Weapon weapon, RadialAttackPattern pattern)
        {
            _pattern = pattern;
            _origin = new Point(source.PositionX, source.Hitbox.Bottom);
            VisualState = new AttackVisualState(
                source, AttackVisualKind.Burst, weapon.Effect.Kind,
                weapon.AnimationPlaceholder, pattern.Range, 0, Math.PI)
            {
                CenterX = _origin.X,
                CenterY = _origin.Y
            };
        }

        public bool IsComplete { get { return _elapsedSeconds >= _pattern.DurationSeconds; } }
        public AttackVisualState VisualState { get; }

        public IReadOnlyList<Entity> Update(
            double deltaSeconds, IReadOnlyList<Entity> targets)
        {
            _elapsedSeconds += deltaSeconds;
            VisualState.Progress = Math.Min(1, _elapsedSeconds / _pattern.DurationSeconds);
            VisualState.Radius = _pattern.Range * VisualState.Progress;

            List<Entity> hits = new List<Entity>();
            foreach (Entity target in targets)
            {
                if (!_hitTargets.Contains(target)
                    && AttackGeometry.DistanceToRect(_origin, target.Hitbox)
                        <= VisualState.Radius)
                {
                    _hitTargets.Add(target);
                    hits.Add(target);
                }
            }
            return hits;
        }
    }
}
