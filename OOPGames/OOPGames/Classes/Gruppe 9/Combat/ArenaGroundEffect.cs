using System;
using System.Collections.Generic;
using System.Windows;

namespace OOPGames
{
    // A short-lived area on the ground that applies an effect to entities touching it once:
    // the fire trail of burning entities and the element traces of the staff strike. The
    // particle renderer draws it with the particle clip named by Visual.
    public sealed class ArenaGroundEffect
    {
        private readonly HashSet<Entity> _affected = new HashSet<Entity>();

        public ArenaGroundEffect(
            string visual, double centerX, double centerY, double radius,
            double durationSeconds, ICombatEffect effect, Entity source,
            Entity ignoredEntity, bool affectsSourceTeam)
        {
            if (string.IsNullOrWhiteSpace(visual))
            {
                throw new ArgumentException("A visual is required.", nameof(visual));
            }
            if (radius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(radius));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            Visual = visual;
            CenterX = centerX;
            CenterY = centerY;
            Radius = radius;
            DurationSeconds = durationSeconds;
            RemainingSeconds = durationSeconds;
            Effect = effect;
            Source = source;
            IgnoredEntity = ignoredEntity;
            AffectsSourceTeam = affectsSourceTeam;
            SourceTeam = source == null ? CombatTeam.Neutral : source.Team;
        }

        // Particle clip that shows the area.
        public string Visual { get; }
        public double CenterX { get; }
        public double CenterY { get; }
        public double Radius { get; }
        public double DurationSeconds { get; }
        public double RemainingSeconds { get; internal set; }
        // Applied once to each entity that touches the area; null for a purely visual trace.
        public ICombatEffect Effect { get; }
        // Entity credited for the effect (e.g. the one who set the fire).
        public Entity Source { get; }
        // Entity the area never affects, e.g. the burning entity that dropped the trail.
        public Entity IgnoredEntity { get; }
        // False: only entities of other teams than the source are affected.
        public bool AffectsSourceTeam { get; }
        public CombatTeam SourceTeam { get; }
        public bool IsExpired { get { return RemainingSeconds <= 0; } }
        public double RemainingFraction
        {
            get { return Math.Max(0, Math.Min(1, RemainingSeconds / DurationSeconds)); }
        }

        internal bool TryAffect(Entity target)
        {
            if (Effect == null || IsExpired || !target.IsAlive
                || target.Team == CombatTeam.Neutral
                || ReferenceEquals(target, IgnoredEntity)
                || (!AffectsSourceTeam && target.Team == SourceTeam)
                || _affected.Contains(target)
                || AttackGeometry.DistanceToRect(
                    new Point(CenterX, CenterY), target.Hitbox) > Radius)
            {
                return false;
            }

            _affected.Add(target);
            Effect.Apply(Source, target);
            return true;
        }
    }
}
