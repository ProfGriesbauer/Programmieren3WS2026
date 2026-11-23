using System.Collections.Generic;

namespace OOPGames
{
    // The narrow view the collision resolver gets of the session. The session implements
    // it explicitly, so these mutating methods are not part of its public surface.
    internal interface IG10_ShooterCollisionContext
    {
        IG10_Collidable PlayerCollider { get; }
        IG10_Collidable BossCollider { get; }
        IReadOnlyList<G10_SolarShooterEnemy> Enemies { get; }
        IReadOnlyList<G10_SolarShooterProjectile> PlayerProjectiles { get; }
        IReadOnlyList<G10_SolarShooterProjectile> EnemyProjectiles { get; }
        IReadOnlyList<G10_SolarShooterPowerCell> PowerCells { get; }
        void RemovePlayerProjectile(G10_SolarShooterProjectile projectile);
        void DestroyEnemy(G10_SolarShooterEnemy enemy, int weaponLevel);
        void RemoveEnemy(G10_SolarShooterEnemy enemy);
        void RemoveEnemyProjectile(G10_SolarShooterProjectile projectile);
        void HitPlayer();
        void HitBoss();
        void CollectPowerCell(G10_SolarShooterPowerCell powerCell);
    }

    internal sealed class G10_CollisionResolver
    {
        static bool Intersects(IG10_Collidable first, IG10_Collidable second)
        {
            return first.Bounds.Intersects(second.Bounds);
        }

        public void ResolvePlayerShots(IG10_ShooterCollisionContext world)
        {
            for (int projectileIndex = world.PlayerProjectiles.Count - 1; projectileIndex >= 0; projectileIndex--)
            {
                G10_SolarShooterProjectile projectile = world.PlayerProjectiles[projectileIndex];
                if (world.BossCollider != null && Intersects(world.BossCollider, projectile))
                {
                    world.RemovePlayerProjectile(projectile);
                    world.HitBoss();
                    continue;
                }

                for (int enemyIndex = world.Enemies.Count - 1; enemyIndex >= 0; enemyIndex--)
                {
                    G10_SolarShooterEnemy enemy = world.Enemies[enemyIndex];
                    if (!Intersects(projectile, enemy)) continue;

                    world.RemovePlayerProjectile(projectile);
                    world.DestroyEnemy(enemy, projectile.WeaponLevel);
                    break;
                }
            }
        }

        public void ResolveEnemyHits(IG10_ShooterCollisionContext world)
        {
            for (int i = world.EnemyProjectiles.Count - 1; i >= 0; i--)
            {
                G10_SolarShooterProjectile projectile = world.EnemyProjectiles[i];
                if (Intersects(projectile, world.PlayerCollider))
                {
                    world.RemoveEnemyProjectile(projectile);
                    world.HitPlayer();
                }
            }

            for (int i = world.Enemies.Count - 1; i >= 0; i--)
            {
                G10_SolarShooterEnemy enemy = world.Enemies[i];
                if (Intersects(world.PlayerCollider, enemy))
                {
                    world.RemoveEnemy(enemy);
                    world.HitPlayer();
                }
            }
        }

        public void ResolvePowerCells(IG10_ShooterCollisionContext world)
        {
            for (int i = world.PowerCells.Count - 1; i >= 0; i--)
            {
                G10_SolarShooterPowerCell cell = world.PowerCells[i];
                if (!Intersects(world.PlayerCollider, cell)) continue;

                world.CollectPowerCell(cell);
            }
        }
    }
}
