using System;

namespace OOPGames
{
    // Upgradable spreading of the lightning element: when a lightning weapon hits, the bolt
    // jumps from the target to the nearest further opponent within JumpRadius, from there to
    // the next and so on, MaximumJumps times, applying Effect (the lightning stun) to each.
    public sealed class ChainLightning
    {
        public ChainLightning(int maximumJumps, double jumpRadius, ICombatEffect effect)
        {
            if (maximumJumps <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maximumJumps));
            }
            if (jumpRadius <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(jumpRadius));
            }

            MaximumJumps = maximumJumps;
            JumpRadius = jumpRadius;
            Effect = effect ?? throw new ArgumentNullException(nameof(effect));
        }

        public int MaximumJumps { get; }
        // Largest gap between two hitboxes the bolt can jump over.
        public double JumpRadius { get; }
        public ICombatEffect Effect { get; }

        // One more opponent and a larger jump radius per upgrade rank.
        public static ChainLightning ForRank(int rank, int elementRank)
        {
            if (rank <= 0)
            {
                return null;
            }
            return new ChainLightning(
                rank, 50 + rank * 25,
                WeaponElementEffectFactory.Create(WeaponElement.Lightning, elementRank, 0));
        }
    }
}
