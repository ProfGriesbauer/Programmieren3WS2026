using System;
using System.Collections.Generic;

namespace OOPGames
{
    public sealed class ArenaCombatSystem
    {
        private readonly Dictionary<Entity, double> _cooldownSeconds =
            new Dictionary<Entity, double>();
        private readonly List<RunningAttack> _runningAttacks =
            new List<RunningAttack>();

        public event Action<Entity, Entity> EntityDefeated;

        public int ActiveAttackCount { get { return _runningAttacks.Count; } }

        public bool TryStartAttack(Entity source, double directionX, double directionY)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            if (!source.IsAlive || source.EquippedWeapon == null)
            {
                return false;
            }
            if (_cooldownSeconds.TryGetValue(source, out double remaining)
                && remaining > 0)
            {
                return false;
            }

            double length = Math.Sqrt(
                directionX * directionX + directionY * directionY);
            if (length == 0 && source.EquippedWeapon.TargetingMode
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

            Weapon weapon = source.EquippedWeapon;
            IAttackInstance instance = weapon.AttackPattern.BeginAttack(
                source, directionX, directionY, weapon);
            _runningAttacks.Add(new RunningAttack(source, weapon, instance));
            _cooldownSeconds[source] = weapon.CooldownSeconds;
            source.HasActiveAttack = true;
            source.CurrentAttackVisual = instance.VisualState;
            return true;
        }

        public double GetCooldownRemaining(Entity entity)
        {
            return _cooldownSeconds.TryGetValue(entity, out double remaining)
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

        public void Update(double deltaSeconds, IReadOnlyList<Entity> entities)
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
                double remaining = _cooldownSeconds[owner] - deltaSeconds;
                if (remaining <= 0 || !owner.IsAlive)
                {
                    _cooldownSeconds.Remove(owner);
                }
                else
                {
                    _cooldownSeconds[owner] = remaining;
                }
            }

            for (int i = _runningAttacks.Count - 1; i >= 0; i--)
            {
                RunningAttack attack = _runningAttacks[i];
                if (!attack.Source.IsAlive)
                {
                    CompleteAttack(attack);
                    _runningAttacks.RemoveAt(i);
                    continue;
                }

                List<Entity> targets = GetTargets(attack, entities);
                IReadOnlyList<Entity> hits =
                    attack.Instance.Update(deltaSeconds, targets);
                attack.Source.CurrentAttackVisual = attack.Instance.VisualState;
                attack.Source.HasActiveAttack = !attack.Instance.IsComplete;

                foreach (Entity target in hits)
                {
                    if (!target.IsAlive || !attack.HitEntities.Add(target))
                    {
                        continue;
                    }

                    bool wasAlive = target.IsAlive;
                    attack.Weapon.Effect.Apply(attack.Source, target);
                    if (wasAlive && !target.IsAlive)
                    {
                        Action<Entity, Entity> handler = EntityDefeated;
                        if (handler != null)
                        {
                            handler(attack.Source, target);
                        }
                    }
                }

                if (attack.Instance.IsComplete)
                {
                    CompleteAttack(attack);
                    _runningAttacks.RemoveAt(i);
                }
            }

            foreach (Entity entity in entities)
            {
                entity.UpdateStatusEffects(deltaSeconds);
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

        private static void CompleteAttack(RunningAttack attack)
        {
            if (ReferenceEquals(
                attack.Source.CurrentAttackVisual, attack.Instance.VisualState))
            {
                attack.Source.CurrentAttackVisual = null;
                attack.Source.HasActiveAttack = false;
            }
        }

        private sealed class RunningAttack
        {
            public RunningAttack(Entity source, Weapon weapon, IAttackInstance instance)
            {
                Source = source;
                Weapon = weapon;
                Instance = instance;
                HitEntities = new HashSet<Entity>();
            }

            public Entity Source { get; }
            public Weapon Weapon { get; }
            public IAttackInstance Instance { get; }
            public HashSet<Entity> HitEntities { get; }
        }
    }
}
