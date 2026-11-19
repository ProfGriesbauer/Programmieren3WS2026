using System;
using System.Collections.Generic;
using System.Windows;

namespace OOPGames
{
    public sealed class ArenaCombatSystem
    {
        private readonly Dictionary<Entity, Dictionary<string, double>> _cooldownSeconds =
            new Dictionary<Entity, Dictionary<string, double>>();
        private readonly List<RunningAttack> _runningAttacks =
            new List<RunningAttack>();

        public event Action<Entity, Entity, Weapon> EntityDefeated;

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
            if (GetCooldownRemaining(source, weapon) > 0)
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
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            List<Entity> cooldownOwners = new List<Entity>(_cooldownSeconds.Keys);
            foreach (Entity owner in cooldownOwners)
            {
                Dictionary<string, double> weaponCooldowns = _cooldownSeconds[owner];
                List<string> weaponIds = new List<string>(weaponCooldowns.Keys);
                foreach (string weaponId in weaponIds)
                {
                    double remaining = weaponCooldowns[weaponId] - deltaSeconds;
                    if (remaining <= 0 || !owner.IsAlive)
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
                    attack.Instance.Update(deltaSeconds, targets);
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
                    if (target.IsAlive && !target.IsInvulnerable)
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

                if (attack.Instance.IsComplete)
                {
                    CompleteAttack(attack);
                    _runningAttacks.RemoveAt(i);
                    RefreshSourceAttackState(attack.Source);
                }
            }

            List<SpreadRequest> spreadRequests = new List<SpreadRequest>();
            foreach (Entity entity in entities)
            {
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
                    if (effect.AdvanceSpreadClock(deltaSeconds))
                    {
                        spreadRequests.Add(new SpreadRequest(effect, entity));
                    }
                }
                entity.RemoveExpiredDamageOverTimeEffects();
            }

            foreach (SpreadRequest spread in spreadRequests)
            {
                foreach (Entity target in entities)
                {
                    if (!target.IsAlive || ReferenceEquals(target, spread.SourceEntity)
                        || target.Team == CombatTeam.Neutral)
                    {
                        continue;
                    }

                    double offsetX = target.PositionX - spread.SourceEntity.PositionX;
                    double offsetY = target.PositionY - spread.SourceEntity.PositionY;
                    double spreadDistance = spread.Effect.SpreadRadius
                        + target.Size / 2 + spread.SourceEntity.Size / 2;
                    if (offsetX * offsetX + offsetY * offsetY
                        <= spreadDistance * spreadDistance)
                    {
                        target.ApplyDamageOverTime(
                            spread.Effect.Id, spread.Effect.Name,
                            spread.Effect.DamagePerSecond,
                            spread.Effect.RemainingSeconds,
                            spread.Effect.SpreadRadius,
                            spread.Effect.Source, spread.Effect.SourceWeapon);
                    }
                }
            }
        }

        public void Clear()
        {
            foreach (RunningAttack attack in _runningAttacks)
            {
                CompleteAttack(attack);
            }
            _runningAttacks.Clear();
            _cooldownSeconds.Clear();
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
            if (distance <= 0)
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
                    double radius = target.Size / 2;
                    nextX = Math.Max(bounds.Left + radius,
                        Math.Min(bounds.Right - radius, nextX));
                    nextY = Math.Max(bounds.Top + radius,
                        Math.Min(bounds.Bottom - radius, nextY));
                }

                bool overlaps = false;
                foreach (Entity other in entities)
                {
                    if (ReferenceEquals(other, target) || !other.IsAlive)
                    {
                        continue;
                    }

                    double minimumDistance = (target.Size + other.Size) / 2;
                    double candidateX = nextX - other.PositionX;
                    double candidateY = nextY - other.PositionY;
                    if (candidateX * candidateX + candidateY * candidateY
                        < minimumDistance * minimumDistance)
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
        }

        private sealed class SpreadRequest
        {
            public SpreadRequest(
                ActiveDamageOverTimeEffect effect, Entity sourceEntity)
            {
                Effect = effect;
                SourceEntity = sourceEntity;
            }

            public ActiveDamageOverTimeEffect Effect { get; }
            public Entity SourceEntity { get; }
        }
    }
}
