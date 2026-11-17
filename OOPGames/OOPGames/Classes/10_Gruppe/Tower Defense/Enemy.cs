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

        // Armor reduces damage by percent: 150 armor halves it, 450 armor leaves a quarter.
        public const double ArmorConstant = 150;

        // Multiplies all incoming damage, e.g. 1 / difficulty multiplier (acts like more health).
        public double IncomingDamageFactor { get; set; } = 1.0;

        double _EffectDamageCarry;

        public double DamageFactor(DamageType damageType, bool useArmor)
        {
            double factor = IncomingDamageFactor;
            if (ResistantDamageTypes.Contains(damageType)) factor *= 0.5;
            if (WeakAgainst.Contains(damageType)) factor *= 1.5;
            if (useArmor) factor *= ArmorConstant / (ArmorConstant + CurrentArmor);
            return factor;
        }

        // Direct hit: weakness +50 %, resistance -50 %, then armor. Every hit deals at least 1.
        public int TakeDamage(int damage, DamageType damageType)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative.");
            }

            if (IsDefeated || damage == 0)
            {
                return 0;
            }

            int actualDamage = Math.Max(1, (int)Math.Round(damage * DamageFactor(damageType, true)));
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

            // Burn counts as fire, poison as poison damage. Both ignore armor; fractions of a point
            // are carried over, so short ticks still add up.
            int appliedDamage = 0;
            if (!IsDefeated && StatusEffectTimeRemaining > 0)
            {
                _EffectDamageCarry += BurnDamagePerSecond * seconds * DamageFactor(DamageType.Fire, false)
                    + PoisonDamagePerSecond * seconds * DamageFactor(DamageType.Poison, false);
                int whole = (int)Math.Floor(_EffectDamageCarry);
                _EffectDamageCarry -= whole;
                appliedDamage = Math.Min(whole, CurrentHealth);
                CurrentHealth -= appliedDamage;
            }
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
