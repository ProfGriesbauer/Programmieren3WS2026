using System;
using System.Collections.Generic;

namespace OOPGames
{
    public class Enemy
    {
        public Enemy(
            string name,
            double moveSpeed,
            int health,
            int armor,
            int damageOnMainBuilding,
            IEnumerable<DamageType> resistantDamageTypes,
            IEnumerable<DamageType> weakAgainst,
            int reward)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("An enemy needs a name.", nameof(name));
            }

            if (moveSpeed < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(moveSpeed), "Move speed cannot be negative.");
            }

            if (health <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(health), "Health must be greater than zero.");
            }

            if (armor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(armor), "Armor cannot be negative.");
            }

            if (damageOnMainBuilding < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damageOnMainBuilding), "Building damage cannot be negative.");
            }

            if (reward < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(reward), "Reward cannot be negative.");
            }

            Name = name;
            MoveSpeed = moveSpeed;
            MaxHealth = health;
            CurrentHealth = health;
            CurrentMoveSpeed = moveSpeed;
            CurrentArmor = armor;
            Armor = armor;
            DamageOnMainBuilding = damageOnMainBuilding;
            ResistantDamageTypes = new HashSet<DamageType>(resistantDamageTypes ?? Array.Empty<DamageType>());
            WeakAgainst = new HashSet<DamageType>(weakAgainst ?? Array.Empty<DamageType>());
            Reward = reward;
        }

        public string Name { get; private set; }
        public double MoveSpeed { get; private set; }
        public double CurrentMoveSpeed { get; private set; }
        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public int Armor { get; private set; }
        public int CurrentArmor { get; private set; }
        public int DamageOnMainBuilding { get; private set; }
        public ISet<DamageType> ResistantDamageTypes { get; private set; }
        public ISet<DamageType> WeakAgainst { get; private set; }
        public int Reward { get; private set; }
        public bool IsDefeated { get { return CurrentHealth <= 0; } }
        public int BurnDamagePerSecond { get; private set; }
        public int PoisonDamagePerSecond { get; private set; }
        public double StatusEffectTimeRemaining { get; private set; }

        public int TakeDamage(int damage, DamageType damageType)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative.");
            }

            if (IsDefeated || ResistantDamageTypes.Contains(damageType))
            {
                return 0;
            }

            int adjustedDamage = damage;
            if (WeakAgainst.Contains(damageType))
            {
                adjustedDamage = (int)Math.Ceiling(damage * 1.5);
            }

            int actualDamage = Math.Max(0, adjustedDamage - CurrentArmor);
            int appliedDamage = Math.Min(actualDamage, CurrentHealth);
            CurrentHealth -= appliedDamage;
            return appliedDamage;
        }

        public void ApplyEffect(Tower tower)
        {
            if (tower == null)
            {
                throw new ArgumentNullException(nameof(tower));
            }

            switch (tower.Effect)
            {
                case TowerEffect.Burn:
                    BurnDamagePerSecond = tower.EffectDamagePerSecond;
                    StatusEffectTimeRemaining = Math.Max(StatusEffectTimeRemaining, tower.EffectDuration);
                    break;
                case TowerEffect.PoisonDamageOverTime:
                    PoisonDamagePerSecond = tower.EffectDamagePerSecond;
                    StatusEffectTimeRemaining = Math.Max(StatusEffectTimeRemaining, tower.EffectDuration);
                    break;
                case TowerEffect.Slow:
                    CurrentMoveSpeed = MoveSpeed * tower.SlowFactor;
                    StatusEffectTimeRemaining = Math.Max(StatusEffectTimeRemaining, tower.EffectDuration);
                    break;
                case TowerEffect.ArmorBreak:
                    CurrentArmor = Math.Max(0, Armor - tower.EffectStrength);
                    StatusEffectTimeRemaining = Math.Max(StatusEffectTimeRemaining, tower.EffectDuration);
                    break;
            }
        }

        public int TickStatusEffects(double seconds)
        {
            if (seconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(seconds), "Time cannot be negative.");
            }

            int damage = (int)Math.Floor((BurnDamagePerSecond + PoisonDamagePerSecond) * seconds);
            int appliedDamage = TakeDamage(damage, DamageType.Physical);
            StatusEffectTimeRemaining = Math.Max(0, StatusEffectTimeRemaining - seconds);

            if (StatusEffectTimeRemaining == 0)
            {
                BurnDamagePerSecond = 0;
                PoisonDamagePerSecond = 0;
                CurrentMoveSpeed = MoveSpeed;
                CurrentArmor = Armor;
            }

            return appliedDamage;
        }
    }
}
