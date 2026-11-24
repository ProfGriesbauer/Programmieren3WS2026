using System;

namespace OOPGames
{
    internal static class WeaponElementEffectFactory
    {
        public static ICombatEffect Create(
            WeaponElement element, int rank, double fireSpreadRadius)
        {
            switch (element)
            {
                case WeaponElement.Poison:
                    return new PoisonEffect(
                        2 + rank * 2, 2.5 + rank * 0.5);
                case WeaponElement.Lightning:
                    return new StunEffect(0.35 + rank * 0.2);
                case WeaponElement.Fire:
                    return new BurningEffect(
                        2 + rank * 2, 2.5 + rank * 0.5, fireSpreadRadius);
                case WeaponElement.Freeze:
                    return new FreezeEffect(0.45 + rank * 0.2);
                case WeaponElement.BlackSteel:
                    // Weakened enemies deal 30 % less damage, 5 % less per element rank.
                    return new WeaknessEffect(
                        0.7 - rank * 0.05, 3 + rank * 0.5);
                default:
                    throw new ArgumentOutOfRangeException(nameof(element));
            }
        }
    }
}
