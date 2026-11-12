using System;

namespace OOPGames
{
    public class MainBuilding : Tower
    {
        public MainBuilding(
            string name,
            int damage,
            double attackSpeed,
            double attackRange,
            int health)
            : base(name, damage, attackSpeed, DamageType.Physical, attackRange, AttackMode.SingleTarget, 0)
        {
            if (health <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(health), "Health must be greater than zero.");
            }

            MaxHealth = health;
            CurrentHealth = health;
        }

        public int MaxHealth { get; private set; }
        public int CurrentHealth { get; private set; }
        public bool IsDestroyed { get { return CurrentHealth <= 0; } }

        public int TakeDamage(int damage)
        {
            if (damage < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(damage), "Damage cannot be negative.");
            }

            int appliedDamage = Math.Min(damage, CurrentHealth);
            CurrentHealth -= appliedDamage;
            return appliedDamage;
        }
    }
}
