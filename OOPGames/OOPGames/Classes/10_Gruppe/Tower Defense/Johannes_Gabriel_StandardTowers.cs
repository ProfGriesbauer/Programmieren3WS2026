using System.Collections.Generic;

namespace OOPGames
{
    public class G10_PhysicalArcher : G10_Tower
    {
        public G10_PhysicalArcher()
            : base("Physical Archer", 25, 1.5, G10_DamageType.Physical, 6, G10_AttackMode.SingleTarget, 100)
        {
        }
    }

    public class G10_FireMage : G10_Tower
    {
        public G10_FireMage()
            : base("Fire Mage", 40, 0.8, G10_DamageType.Fire, 5, G10_AttackMode.SingleTarget, 150,
                G10_TowerEffect.Burn, 8, StatusEffectDuration)
        {
        }
    }

    public class G10_IceMage : G10_Tower
    {
        public G10_IceMage()
            : base("Ice Mage", 20, 1.0, G10_DamageType.Ice, 5, G10_AttackMode.SingleTarget, 140,
                G10_TowerEffect.Slow, 0, StatusEffectDuration, 0.5)
        {
        }
    }

    public class G10_PoisonArcher : G10_Tower
    {
        public G10_PoisonArcher()
            : base("Poison Archer", 30, 1.2, G10_DamageType.Poison, 6, G10_AttackMode.SingleTarget, 130,
                G10_TowerEffect.PoisonDamageOverTime, 6, StatusEffectDuration)
        {
        }
    }

    public partial class G10_StromMage : G10_Tower
    {
        public G10_StromMage()
            : base("Strom Mage", 35, 0.7, G10_DamageType.Storm, 4, G10_AttackMode.AreaDamage, 200,
                G10_TowerEffect.ChainLightning, 0, 0, 1, 6)
        {
        }
    }

    public class G10_EarthMage : G10_Tower
    {
        public G10_EarthMage()
            : base("Earth Mage", 35, 0.8, G10_DamageType.Earth, 4, G10_AttackMode.SingleTarget, 170,
                G10_TowerEffect.ArmorBreak, 0, StatusEffectDuration, 1, 1, 20)
        {
        }
    }

    public partial class G10_StromMage
    {
        public IList<G10_Enemy> GetChainTargets(IList<G10_Enemy> enemies, int firstTargetIndex)
        {
            if (enemies == null)
            {
                throw new System.ArgumentNullException(nameof(enemies));
            }

            if (firstTargetIndex < 0 || firstTargetIndex >= enemies.Count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(firstTargetIndex));
            }

            IList<G10_Enemy> targets = new List<G10_Enemy>();
            targets.Add(enemies[firstTargetIndex]);

            for (int distance = 1; targets.Count < MaxChainTargets; distance++)
            {
                int leftIndex = firstTargetIndex - distance;
                if (leftIndex >= 0 && !enemies[leftIndex].IsDefeated)
                {
                    targets.Add(enemies[leftIndex]);
                }

                int rightIndex = firstTargetIndex + distance;
                if (targets.Count < MaxChainTargets && rightIndex < enemies.Count && !enemies[rightIndex].IsDefeated)
                {
                    targets.Add(enemies[rightIndex]);
                }

                if (leftIndex < 0 && rightIndex >= enemies.Count)
                {
                    break;
                }
            }

            return targets;
        }
    }

    public class G10_Flamethrower : G10_Tower
    {
        public G10_Flamethrower()
            : base("Flamethrower", 30, 1.0, G10_DamageType.Fire, 3, G10_AttackMode.AreaDamage, 180,
                G10_TowerEffect.Burn, 10, StatusEffectDuration)
        {
        }
    }

    public class G10_IceAreaTower : G10_Tower
    {
        public G10_IceAreaTower()
            : base("Ice Area Tower", 25, 0.9, G10_DamageType.Ice, 3, G10_AttackMode.AreaDamage, 190,
                G10_TowerEffect.Slow, 0, StatusEffectDuration, 0.5)
        {
        }
    }
}
