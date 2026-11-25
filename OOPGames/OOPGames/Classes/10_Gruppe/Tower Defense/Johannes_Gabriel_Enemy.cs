using System;
using System.Collections.Generic;

namespace OOPGames
{
    public class G10_Enemy
    {
        public G10_Enemy(
            string name,
            double moveSpeed,
            int health,
            int armor,
            int damageOnMainBuilding,
            IEnumerable<G10_DamageType> resistantDamageTypes,
            IEnumerable<G10_DamageType> weakAgainst,
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
            ResistantDamageTypes = new HashSet<G10_DamageType>(resistantDamageTypes ?? Array.Empty<G10_DamageType>());
            WeakAgainst = new HashSet<G10_DamageType>(weakAgainst ?? Array.Empty<G10_DamageType>());
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
        public ISet<G10_DamageType> ResistantDamageTypes { get; private set; }
        public ISet<G10_DamageType> WeakAgainst { get; private set; }
        public int Reward { get; private set; }
        public bool IsDefeated { get { return CurrentHealth <= 0; } }
        public int BurnDamagePerSecond { get; private set; }
        public int PoisonDamagePerSecond { get; private set; }
        public double BurnTimeRemaining { get; private set; }
        public double PoisonTimeRemaining { get; private set; }
        public double SlowTimeRemaining { get; private set; }
        public double ArmorBreakTimeRemaining { get; private set; }

        // Longest running effect; 0 when the enemy has no effect at all.
        public double StatusEffectTimeRemaining
        {
            get { return Math.Max(Math.Max(BurnTimeRemaining, PoisonTimeRemaining), Math.Max(SlowTimeRemaining, ArmorBreakTimeRemaining)); }
        }

        // Armor reduces damage by percent: 150 armor halves it, 450 armor leaves a quarter.
        public const double ArmorConstant = 150;

        // Multiplies all incoming damage, e.g. 1 / difficulty multiplier (acts like more health).
        public double IncomingDamageFactor { get; set; } = 1.0;

        double _EffectDamageCarry;

        public double DamageFactor(G10_DamageType damageType, bool useArmor)
        {
            double factor = IncomingDamageFactor;
            if (ResistantDamageTypes.Contains(damageType)) factor *= 0.5;
            if (WeakAgainst.Contains(damageType)) factor *= 1.5;
            if (useArmor) factor *= ArmorConstant / (ArmorConstant + CurrentArmor);
            return factor;
        }

        // Direct hit: weakness +50 %, resistance -50 %, then armor. Every hit deals at least 1.
        public int TakeDamage(int damage, G10_DamageType damageType)
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

        public void ApplyEffect(G10_Tower tower)
        {
            if (tower == null)
            {
                throw new ArgumentNullException(nameof(tower));
            }

            // Every effect has its own timer. A new hit of the same effect restarts it, so an effect
            // ends EffectDuration seconds after the last hit of that effect.
            switch (tower.Effect)
            {
                case G10_TowerEffect.Burn:
                    BurnDamagePerSecond = tower.EffectDamagePerSecond;
                    BurnTimeRemaining = tower.EffectDuration;
                    break;
                case G10_TowerEffect.PoisonDamageOverTime:
                    PoisonDamagePerSecond = tower.EffectDamagePerSecond;
                    PoisonTimeRemaining = tower.EffectDuration;
                    break;
                case G10_TowerEffect.Slow:
                    CurrentMoveSpeed = MoveSpeed * tower.SlowFactor;
                    SlowTimeRemaining = tower.EffectDuration;
                    break;
                case G10_TowerEffect.ArmorBreak:
                    CurrentArmor = Math.Max(0, Armor - tower.EffectStrength);
                    ArmorBreakTimeRemaining = tower.EffectDuration;
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
            if (!IsDefeated)
            {
                double burnSeconds = Math.Min(seconds, BurnTimeRemaining);
                double poisonSeconds = Math.Min(seconds, PoisonTimeRemaining);
                _EffectDamageCarry += BurnDamagePerSecond * burnSeconds * DamageFactor(G10_DamageType.Fire, false)
                    + PoisonDamagePerSecond * poisonSeconds * DamageFactor(G10_DamageType.Poison, false);
                int whole = (int)Math.Floor(_EffectDamageCarry);
                _EffectDamageCarry -= whole;
                appliedDamage = Math.Min(whole, CurrentHealth);
                CurrentHealth -= appliedDamage;
            }

            BurnTimeRemaining = Math.Max(0, BurnTimeRemaining - seconds);
            PoisonTimeRemaining = Math.Max(0, PoisonTimeRemaining - seconds);
            SlowTimeRemaining = Math.Max(0, SlowTimeRemaining - seconds);
            ArmorBreakTimeRemaining = Math.Max(0, ArmorBreakTimeRemaining - seconds);
            if (BurnTimeRemaining == 0) BurnDamagePerSecond = 0;
            if (PoisonTimeRemaining == 0) PoisonDamagePerSecond = 0;
            if (SlowTimeRemaining == 0) CurrentMoveSpeed = MoveSpeed;
            if (ArmorBreakTimeRemaining == 0) CurrentArmor = Armor;
            if (BurnTimeRemaining == 0 && PoisonTimeRemaining == 0) _EffectDamageCarry = 0;

            return appliedDamage;
        }
    }
}
