using System;
using System.Collections.Generic;
using System.Windows;

namespace OOPGames
{
    public sealed class ArenaCombatSystem
    {
        private const double CooldownTolerance = 1e-9;
        private readonly Dictionary<Entity, Dictionary<string, double>> _cooldownSeconds =
            new Dictionary<Entity, Dictionary<string, double>>();
        private readonly List<RunningAttack> _runningAttacks =
            new List<RunningAttack>();
        private readonly List<ArenaGroundEffect> _groundEffects =
            new List<ArenaGroundEffect>();
        private readonly Dictionary<Entity, FireTrailState> _fireTrails =
            new Dictionary<Entity, FireTrailState>();

        // Burning entities drop a patch of fire at their feet every FireTrailDropSeconds while
        // they move. The trail only shows the fire: entities catch fire solely when a burning
        // entity hits them, then they burn for at least IgnitionBurnSeconds.
        public const double FireTrailDropSeconds = 0.2;
        public const double FireTrailMinimumDistance = 6;
        public const double FireTrailRadius = 12;
        public const double FireTrailDurationSeconds = 1.5;
        public const double IgnitionBurnSeconds = 2;
        // Staff strike traces: one per StrikeTraceSpacing units of the strike's reach.
        public const double StrikeTraceSpacing = 22;
        public const double StrikeTraceRadius = 14;
        public const double StrikeTraceDurationSeconds = 1.6;

        public IReadOnlyList<ArenaGroundEffect> GroundEffects
        {
            get { return _groundEffects.AsReadOnly(); }
        }

        public void AddGroundEffect(ArenaGroundEffect groundEffect)
        {
            if (groundEffect == null)
            {
                throw new ArgumentNullException(nameof(groundEffect));
            }
            _groundEffects.Add(groundEffect);
        }

        public event Action<Entity, Entity, Weapon> EntityDefeated;
        // A lightning bolt jumped from the first entity to the second (Kettenblitz).
        public event Action<Entity, Entity> LightningChained;

        public int ActiveAttackCount { get { return _runningAttacks.Count; } }

        public bool TryStartAttack(Entity source, double directionX, double directionY)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (source.EquippedWeapon == null)
            {
                return false;
            }

            return TryStartAttack(
                source, source.EquippedWeapon, directionX, directionY);
        }

        public bool TryStartAttack(
            Entity source, Weapon weapon, double directionX, double directionY)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (weapon == null)
            {
                throw new ArgumentNullException(nameof(weapon));
            }
            if (!source.IsAlive)
            {
                return false;
            }
            if (GetCooldownRemaining(source, weapon) > 0
                || !IsReadyAfterEquip(source, weapon))
            {
                return false;
            }

            double length = Math.Sqrt(
                directionX * directionX + directionY * directionY);
            if (length == 0 && weapon.TargetingMode
                != WeaponTargetingMode.Self)
            {
                return false;
            }
            if (length != 0)
            {
                directionX /= length;
                directionY /= length;
            }
            else
            {
                directionX = 1;
            }

            IAttackInstance instance = weapon.AttackPattern.BeginAttack(
                source, directionX, directionY, weapon);
            instance.VisualState.Appearance = weapon.Appearance;
            _runningAttacks.Add(new RunningAttack(source, weapon, instance));
            if (!_cooldownSeconds.TryGetValue(
                source, out Dictionary<string, double> weaponCooldowns))
            {
                weaponCooldowns = new Dictionary<string, double>();
                _cooldownSeconds.Add(source, weaponCooldowns);
            }
            weaponCooldowns[weapon.Id] = weapon.CooldownSeconds;
            source.HasActiveAttack = true;
            source.CurrentAttackVisual = instance.VisualState;
            return true;
        }

        // A freshly equipped weapon with an equip time (the bow being drawn) cannot attack
        // until that time has passed.
        private static bool IsReadyAfterEquip(Entity source, Weapon weapon)
        {
            return weapon.EquipReadySeconds <= 0
                || source.EquippedWeapon == null
                || source.EquippedWeapon.Id != weapon.Id
                || source.WeaponEquippedSeconds >= weapon.EquipReadySeconds;
        }

        public double GetCooldownRemaining(Entity entity)
        {
            return entity == null || entity.EquippedWeapon == null
                ? 0
                : GetCooldownRemaining(entity, entity.EquippedWeapon);
        }

        public double GetCooldownRemaining(Entity entity, Weapon weapon)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            if (weapon == null)
            {
                throw new ArgumentNullException(nameof(weapon));
            }

            return _cooldownSeconds.TryGetValue(
                    entity, out Dictionary<string, double> weaponCooldowns)
                && weaponCooldowns.TryGetValue(weapon.Id, out double remaining)
                    ? Math.Max(0, remaining)
                    : 0;
        }

        public IReadOnlyList<AttackVisualState> GetActiveVisuals()
        {
            List<AttackVisualState> visuals =
                new List<AttackVisualState>(_runningAttacks.Count);
            foreach (RunningAttack attack in _runningAttacks)
            {
                visuals.Add(attack.Instance.VisualState);
            }
            return visuals.AsReadOnly();
        }

        public void Update(
            double deltaSeconds, IReadOnlyList<Entity> entities,
            Rect? movementBounds = null)
        {
            if (deltaSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }

            Update(entities, movementBounds, entity => deltaSeconds);
        }

        // Advances every entity by its own game time: cooldowns and status effects of an
        // entity and the attacks it started all use getDeltaSeconds(entity). Bullet Time uses
        // this to keep the player at normal speed inside the slowed arena.
        public void Update(
            IReadOnlyList<Entity> entities, Rect? movementBounds,
            Func<Entity, double> getDeltaSeconds)
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }
            if (getDeltaSeconds == null)
            {
                throw new ArgumentNullException(nameof(getDeltaSeconds));
            }

            Func<Entity, double> deltaFor = entity =>
            {
                double delta = getDeltaSeconds(entity);
                if (delta < 0 || double.IsNaN(delta))
                {
                    throw new ArgumentOutOfRangeException(nameof(getDeltaSeconds));
                }
                return delta;
            };

            List<Entity> cooldownOwners = new List<Entity>(_cooldownSeconds.Keys);
            foreach (Entity owner in cooldownOwners)
            {
                Dictionary<string, double> weaponCooldowns = _cooldownSeconds[owner];
                double deltaSeconds = deltaFor(owner);
                List<string> weaponIds = new List<string>(weaponCooldowns.Keys);
                foreach (string weaponId in weaponIds)
                {
                    double remaining = weaponCooldowns[weaponId] - deltaSeconds;
                    // The tolerance lets a cooldown that equals an attack's duration end in the
                    // same tick as the attack despite rounding.
                    if (remaining <= CooldownTolerance || !owner.IsAlive)
                    {
                        weaponCooldowns.Remove(weaponId);
                    }
                    else
                    {
                        weaponCooldowns[weaponId] = remaining;
                    }
                }
                if (weaponCooldowns.Count == 0)
                {
                    _cooldownSeconds.Remove(owner);
                }
            }

            for (int i = _runningAttacks.Count - 1; i >= 0; i--)
            {
                RunningAttack attack = _runningAttacks[i];
                if (!attack.Source.IsAlive)
                {
                    CompleteAttack(attack);
                    _runningAttacks.RemoveAt(i);
                    RefreshSourceAttackState(attack.Source);
                    continue;
                }

                List<Entity> targets = GetTargets(attack, entities);
                IReadOnlyList<Entity> hits =
                    attack.Instance.Update(deltaFor(attack.Source), targets);
                attack.Source.CurrentAttackVisual = attack.Instance.VisualState;
                attack.Source.HasActiveAttack = true;

                foreach (Entity target in hits)
                {
                    if (!target.IsAlive)
                    {
                        continue;
                    }
                    int hitCount = attack.HitCounts.TryGetValue(
                        target, out int previousHitCount)
                        ? previousHitCount
                        : 0;
                    if (hitCount >= attack.Weapon.MaximumHitsPerTarget)
                    {
                        continue;
                    }
                    attack.HitCounts[target] = hitCount + 1;

                    bool wasAlive = target.IsAlive;
                    attack.Source.EffectWeaponContext = attack.Weapon;
                    try
                    {
                        attack.Weapon.Effect.Apply(attack.Source, target);
                    }
                    finally
                    {
                        attack.Source.EffectWeaponContext = null;
                    }
                    SpreadChainLightning(attack, target, entities);
                    IgniteFromBurningAttacker(attack.Source, target);
                    // Invulnerability (godmode) only prevents damage; hits still push.
                    if (target.IsAlive)
                    {
                        ApplyKnockback(
                            attack.Source, target, attack.Weapon.KnockbackDistance,
                            entities, movementBounds);
                    }
                    if (wasAlive && !target.IsAlive)
                    {
                        Action<Entity, Entity, Weapon> handler = EntityDefeated;
                        if (handler != null)
                        {
                            handler(attack.Source, target, attack.Weapon);
                        }
                    }
                }

                PlaceAttackTraces(attack);
                if (attack.Instance.IsComplete)
                {
                    CompleteAttack(attack);
                    _runningAttacks.RemoveAt(i);
                    RefreshSourceAttackState(attack.Source);
                }
            }

            foreach (Entity entity in entities)
            {
                double deltaSeconds = deltaFor(entity);
                entity.UpdateStatusEffects(deltaSeconds);
                foreach (ActiveDamageOverTimeEffect effect
                    in new List<ActiveDamageOverTimeEffect>(
                        entity.ActiveDamageOverTimeEffects))
                {
                    bool wasAlive = entity.IsAlive;
                    int damage = effect.AdvanceDamage(deltaSeconds);
                    if (damage > 0)
                    {
                        entity.TakeDamage(damage);
                    }
                    if (wasAlive && !entity.IsAlive)
                    {
                        Action<Entity, Entity, Weapon> handler = EntityDefeated;
                        if (handler != null && effect.Source != null)
                        {
                            handler(effect.Source, entity,
                                effect.SourceWeapon);
                        }
                    }
                }
                entity.RemoveExpiredDamageOverTimeEffects();
            }

            UpdateFireTrails(entities, deltaFor);
            UpdateGroundEffects(entities, deltaFor(null));
        }

        public void Clear()
        {
            foreach (RunningAttack attack in _runningAttacks)
            {
                CompleteAttack(attack);
            }
            _runningAttacks.Clear();
            _cooldownSeconds.Clear();
            _groundEffects.Clear();
            _fireTrails.Clear();
        }

        private static List<Entity> GetTargets(
            RunningAttack attack, IReadOnlyList<Entity> entities)
        {
            List<Entity> targets = new List<Entity>();
            foreach (Entity entity in entities)
            {
                if (!entity.IsAlive)
                {
                    continue;
                }

                switch (attack.Weapon.TargetingMode)
                {
                    case WeaponTargetingMode.Self:
                        if (ReferenceEquals(entity, attack.Source))
                        {
                            targets.Add(entity);
                        }
                        break;
                    case WeaponTargetingMode.Allies:
                        if (entity.Team == attack.Source.Team)
                        {
                            targets.Add(entity);
                        }
                        break;
                    default:
                        if (!ReferenceEquals(entity, attack.Source)
                            && entity.Team != CombatTeam.Neutral
                            && entity.Team != attack.Source.Team)
                        {
                            targets.Add(entity);
                        }
                        break;
                }
            }
            return targets;
        }

        private static void ApplyKnockback(
            Entity source, Entity target, double distance,
            IReadOnlyList<Entity> entities, Rect? movementBounds)
        {
            if (distance <= 0 || !target.CanBePushed)
            {
                return;
            }

            double offsetX = target.PositionX - source.PositionX;
            double offsetY = target.PositionY - source.PositionY;
            double length = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            if (length == 0)
            {
                return;
            }

            double directionX = offsetX / length;
            double directionY = offsetY / length;
            double step = distance;
            for (int attempt = 0; attempt < 8 && step > 0.25; attempt++)
            {
                double nextX = target.PositionX + directionX * step;
                double nextY = target.PositionY + directionY * step;
                if (movementBounds.HasValue)
                {
                    Rect bounds = movementBounds.Value;
                    Rect hitbox = target.HitboxOffset;
                    nextX = Math.Max(bounds.Left - hitbox.Left,
                        Math.Min(bounds.Right - hitbox.Right, nextX));
                    nextY = Math.Max(bounds.Top - hitbox.Top,
                        Math.Min(bounds.Bottom - hitbox.Bottom, nextY));
                }

                bool overlaps = false;
                foreach (Entity other in entities)
                {
                    if (ReferenceEquals(other, target) || !other.IsAlive)
                    {
                        continue;
                    }

                    if (AttackGeometry.RectsOverlap(
                        target.GetHitboxAt(nextX, nextY), other.Hitbox))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    target.PositionX = nextX;
                    target.PositionY = nextY;
                    return;
                }
                step /= 2;
            }
        }

        private static void CompleteAttack(RunningAttack attack)
        {
            if (ReferenceEquals(
                attack.Source.CurrentAttackVisual, attack.Instance.VisualState))
            {
                attack.Source.CurrentAttackVisual = null;
                attack.Source.HasActiveAttack = false;
            }
        }

        // Kettenblitz: from the hit target the bolt jumps to the nearest opponent within the
        // jump radius, then on from there, applying the lightning effect to each one once.
        private void SpreadChainLightning(
            RunningAttack attack, Entity firstTarget, IReadOnlyList<Entity> entities)
        {
            ChainLightning chain = attack.Weapon.ChainLightning;
            if (chain == null)
            {
                return;
            }

            HashSet<Entity> struck = new HashSet<Entity> { firstTarget };
            Entity current = firstTarget;
            for (int jump = 0; jump < chain.MaximumJumps; jump++)
            {
                Entity next = null;
                double nextDistance = double.MaxValue;
                foreach (Entity candidate in entities)
                {
                    if (!candidate.IsAlive || struck.Contains(candidate)
                        || ReferenceEquals(candidate, attack.Source)
                        || candidate.Team == CombatTeam.Neutral
                        || candidate.Team == attack.Source.Team)
                    {
                        continue;
                    }
                    double distance = AttackGeometry.DistanceBetweenRects(
                        current.Hitbox, candidate.Hitbox);
                    if (distance <= chain.JumpRadius && distance < nextDistance)
                    {
                        next = candidate;
                        nextDistance = distance;
                    }
                }
                if (next == null)
                {
                    return;
                }

                struck.Add(next);
                attack.Source.EffectWeaponContext = attack.Weapon;
                try
                {
                    chain.Effect.Apply(attack.Source, next);
                }
                finally
                {
                    attack.Source.EffectWeaponContext = null;
                }
                LightningChained?.Invoke(current, next);
                current = next;
            }
        }

        // Fire spreads only by hits: a burning attacker sets what it hits on fire.
        private static void IgniteFromBurningAttacker(Entity attacker, Entity target)
        {
            if (!target.IsAlive)
            {
                return;
            }
            foreach (ActiveDamageOverTimeEffect effect in attacker.ActiveDamageOverTimeEffects)
            {
                if (effect.Id == "burning" && !effect.IsExpired)
                {
                    target.ApplyDamageOverTime(
                        effect.Id, effect.Name, effect.DamagePerSecond,
                        Math.Max(IgnitionBurnSeconds, effect.RemainingSeconds), 0,
                        effect.Source, effect.SourceWeapon);
                    return;
                }
            }
        }

        // Line attacks with a trail leave traces behind their tip as it advances.
        private void PlaceAttackTraces(RunningAttack attack)
        {
            AttackVisualState visual = attack.Instance.VisualState;
            if (!attack.Weapon.HasTrail || visual.VisualKind != AttackVisualKind.Cone)
            {
                return;
            }

            int traces = Math.Max(1, (int)Math.Ceiling(visual.Range / StrikeTraceSpacing));
            int reached = Math.Min(traces, (int)Math.Floor(visual.Progress * traces + 1e-9));
            while (attack.TracesPlaced < reached)
            {
                double along = visual.Range * (attack.TracesPlaced + 0.5) / traces;
                _groundEffects.Add(new ArenaGroundEffect(
                    attack.Weapon.TrailVisual,
                    visual.CenterX + visual.DirectionX * along,
                    visual.CenterY + visual.DirectionY * along,
                    StrikeTraceRadius, StrikeTraceDurationSeconds,
                    attack.Weapon.TrailEffect, attack.Source, attack.Source, false));
                attack.TracesPlaced++;
            }
        }

        private void UpdateFireTrails(
            IReadOnlyList<Entity> entities, Func<Entity, double> deltaFor)
        {
            foreach (Entity entity in entities)
            {
                ActiveDamageOverTimeEffect burning = null;
                foreach (ActiveDamageOverTimeEffect effect in entity.ActiveDamageOverTimeEffects)
                {
                    if (effect.Id == "burning" && !effect.IsExpired)
                    {
                        burning = effect;
                    }
                }
                if (burning == null || !entity.IsAlive)
                {
                    _fireTrails.Remove(entity);
                    continue;
                }

                Point feet = new Point(entity.PositionX, entity.Hitbox.Bottom);
                if (!_fireTrails.TryGetValue(entity, out FireTrailState trail))
                {
                    trail = new FireTrailState(feet);
                    _fireTrails.Add(entity, trail);
                }
                trail.SecondsUntilDrop -= deltaFor(entity);
                if (trail.SecondsUntilDrop > CooldownTolerance
                    || (feet - trail.LastDrop).Length < FireTrailMinimumDistance)
                {
                    continue;
                }

                trail.SecondsUntilDrop = FireTrailDropSeconds;
                trail.LastDrop = feet;
                _groundEffects.Add(new ArenaGroundEffect(
                    "FireTrail", feet.X, feet.Y, FireTrailRadius, FireTrailDurationSeconds,
                    null, burning.Source, entity, true));
            }
        }

        private void UpdateGroundEffects(IReadOnlyList<Entity> entities, double deltaSeconds)
        {
            for (int i = _groundEffects.Count - 1; i >= 0; i--)
            {
                ArenaGroundEffect groundEffect = _groundEffects[i];
                groundEffect.RemainingSeconds -= deltaSeconds;
                if (groundEffect.IsExpired)
                {
                    _groundEffects.RemoveAt(i);
                    continue;
                }
                foreach (Entity entity in entities)
                {
                    groundEffect.TryAffect(entity);
                }
            }
        }

        private void RefreshSourceAttackState(Entity source)
        {
            if (source.IsAlive)
            {
                for (int i = _runningAttacks.Count - 1; i >= 0; i--)
                {
                    if (ReferenceEquals(_runningAttacks[i].Source, source))
                    {
                        source.CurrentAttackVisual =
                            _runningAttacks[i].Instance.VisualState;
                        source.HasActiveAttack = true;
                        return;
                    }
                }
            }

            source.CurrentAttackVisual = null;
            source.HasActiveAttack = false;
        }

        private sealed class RunningAttack
        {
            public RunningAttack(Entity source, Weapon weapon, IAttackInstance instance)
            {
                Source = source;
                Weapon = weapon;
                Instance = instance;
                HitCounts = new Dictionary<Entity, int>();
            }

            public Entity Source { get; }
            public Weapon Weapon { get; }
            public IAttackInstance Instance { get; }
            public Dictionary<Entity, int> HitCounts { get; }
            public int TracesPlaced { get; set; }
        }

        private sealed class FireTrailState
        {
            public FireTrailState(Point position)
            {
                LastDrop = position;
                // The first patch is dropped as soon as the entity has moved a little.
                SecondsUntilDrop = 0;
            }

            public Point LastDrop { get; set; }
            public double SecondsUntilDrop { get; set; }
        }
    }
}
