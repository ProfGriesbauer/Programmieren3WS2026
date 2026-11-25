using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Strategy: each weapon level is its own IG10_Weapon.
    internal interface IG10_Weapon
    {
        float Cooldown { get; }
        void Fire(G10_SolarShooterPlayer player, IList<G10_SolarShooterProjectile> projectiles);
    }

    internal sealed class G10_SingleShotWeapon : IG10_Weapon
    {
        public float Cooldown { get { return 0.24f; } }
        public void Fire(G10_SolarShooterPlayer player, IList<G10_SolarShooterProjectile> projectiles)
        {
            projectiles.Add(Create(player, 0, player.WeaponLevel));
        }
        public static G10_SolarShooterProjectile Create(
            G10_SolarShooterPlayer player,
            float horizontalOffset,
            int weaponLevel)
        {
            return new G10_SolarShooterProjectile(
                player.X + horizontalOffset, player.Y - 7, horizontalOffset * 1.5f, -150, weaponLevel);
        }
    }

    internal sealed class G10_DoubleShotWeapon : IG10_Weapon
    {
        public float Cooldown { get { return 0.24f; } }
        public void Fire(G10_SolarShooterPlayer player, IList<G10_SolarShooterProjectile> projectiles)
        {
            projectiles.Add(G10_SingleShotWeapon.Create(player, -3, player.WeaponLevel));
            projectiles.Add(G10_SingleShotWeapon.Create(player, 3, player.WeaponLevel));
        }
    }

    internal sealed class G10_TripleShotWeapon : IG10_Weapon
    {
        public float Cooldown { get { return 0.3f; } }
        public void Fire(G10_SolarShooterPlayer player, IList<G10_SolarShooterProjectile> projectiles)
        {
            projectiles.Add(G10_SingleShotWeapon.Create(player, -5, player.WeaponLevel));
            projectiles.Add(G10_SingleShotWeapon.Create(player, 0, player.WeaponLevel));
            projectiles.Add(G10_SingleShotWeapon.Create(player, 5, player.WeaponLevel));
        }
    }

    // Strategy: every stage gives its boss a different attack pattern.
    internal interface IG10_BossAttack
    {
        void Fire(G10_SolarShooterBoss boss, IList<G10_SolarShooterProjectile> projectiles);
    }

    internal sealed class G10_AimedBossAttack : IG10_BossAttack
    {
        public void Fire(G10_SolarShooterBoss boss, IList<G10_SolarShooterProjectile> projectiles)
        {
            projectiles.Add(new G10_SolarShooterProjectile(
                boss.X, boss.Y + 8, boss.AimDirection * 22, 72));
        }
    }

    internal sealed class G10_SpreadBossAttack : IG10_BossAttack
    {
        readonly bool _ExtraDownShot;

        public G10_SpreadBossAttack(bool extraDownShot)
        {
            _ExtraDownShot = extraDownShot;
        }

        public void Fire(G10_SolarShooterBoss boss, IList<G10_SolarShooterProjectile> projectiles)
        {
            projectiles.Add(new G10_SolarShooterProjectile(boss.X - 10, boss.Y + 7, -32, 68));
            projectiles.Add(new G10_SolarShooterProjectile(boss.X, boss.Y + 9, 0, 74));
            projectiles.Add(new G10_SolarShooterProjectile(boss.X + 10, boss.Y + 7, 32, 68));
            if (_ExtraDownShot && boss.AttackNumber % 2 == 0)
                projectiles.Add(new G10_SolarShooterProjectile(boss.X, boss.Y + 8, 0, 76));
        }
    }

    internal sealed class G10_SolarShooterBoss : G10_ShooterEntity, IG10_ShooterBossView
    {
        readonly G10_SolarShooterStage _Stage;
        readonly G10_SolarShooterPlayer _Player;
        float _Phase;
        float _ShotCooldown = 1.1f;

        public G10_SolarShooterBoss(G10_SolarShooterStage stage, G10_SolarShooterPlayer player)
        {
            _Stage = stage;
            _Player = player;
            Health = stage.BossHealth;
            MaxHealth = stage.BossHealth;
            X = G10_ShooterConfig.BossStartX;
            Y = G10_ShooterConfig.BossStartY;
        }

        public int Health { get; private set; }
        public int MaxHealth { get; private set; }
        public int AttackNumber { get; private set; }
        public float AimDirection { get { return Math.Sign(_Player.X - X); } }
        public override G10_SolarShooterBounds Bounds
        {
            get { return new G10_SolarShooterBounds(X - 21, Y - 9, 42, 18); }
        }

        public override void Update(float deltaSeconds)
        {
            _Phase += deltaSeconds * _Stage.BossTempo;
            X = 80 + (float)Math.Sin(_Phase) * _Stage.BossAmplitude;
            Y = 31 + (float)Math.Sin(_Phase * 1.7f) * 4;
        }

        public bool TryFire(float deltaSeconds)
        {
            _ShotCooldown -= deltaSeconds;
            if (_ShotCooldown > 0) return false;
            _ShotCooldown = _Stage.BossShotInterval;
            AttackNumber++;
            return true;
        }

        public void TakeHit() { Health = Math.Max(0, Health - 1); }
    }
}
