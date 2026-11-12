using System.Collections.Generic;

namespace OOPGames
{
    public class PhysicalArcher : Tower
    {
        public PhysicalArcher()
            : base("Physical Archer", 25, 1.5, DamageType.Physical, 6, AttackMode.SingleTarget, 100)
        {
        }
    }

    public class FireMage : Tower
    {
        public FireMage()
            : base("Fire Mage", 40, 0.8, DamageType.Fire, 5, AttackMode.SingleTarget, 150,
                TowerEffect.Burn, 8, 4)
        {
        }
    }

    public class IceMage : Tower
    {
        public IceMage()
            : base("Ice Mage", 20, 1.0, DamageType.Ice, 5, AttackMode.SingleTarget, 140,
                TowerEffect.Slow, 0, 3, 0.5)
        {
        }
    }

    public class PoisonArcher : Tower
    {
        public PoisonArcher()
            : base("Poison Archer", 30, 1.2, DamageType.Poison, 6, AttackMode.SingleTarget, 130,
                TowerEffect.PoisonDamageOverTime, 6, 5)
        {
        }
    }

    public partial class StromMage : Tower
    {
        public StromMage()
            : base("Strom Mage", 35, 0.7, DamageType.Storm, 4, AttackMode.AreaDamage, 200,
                TowerEffect.ChainLightning, 0, 0, 1, 6)
        {
        }
    }

    public class EarthMage : Tower
    {
        public EarthMage()
            : base("Earth Mage", 35, 0.8, DamageType.Earth, 4, AttackMode.SingleTarget, 170,
                TowerEffect.ArmorBreak, 0, 4, 1, 1, 20)
        {
        }
    }

    public partial class StromMage
    {
        public IList<Enemy> GetChainTargets(IList<Enemy> enemies, int firstTargetIndex)
        {
            if (enemies == null)
            {
                throw new System.ArgumentNullException(nameof(enemies));
            }

            if (firstTargetIndex < 0 || firstTargetIndex >= enemies.Count)
            {
                throw new System.ArgumentOutOfRangeException(nameof(firstTargetIndex));
            }

            IList<Enemy> targets = new List<Enemy>();
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

    public class Flamethrower : Tower
    {
        public Flamethrower()
            : base("Flamethrower", 30, 1.0, DamageType.Fire, 3, AttackMode.AreaDamage, 180,
                TowerEffect.Burn, 10, 3)
        {
        }
    }

    public class IceAreaTower : Tower
    {
        public IceAreaTower()
            : base("Ice Area Tower", 25, 0.9, DamageType.Ice, 3, AttackMode.AreaDamage, 190,
                TowerEffect.Slow, 0, 2, 0.5)
        {
        }
    }
}
