using System;
using System.Collections.Generic;

namespace OOPGames
{
    internal enum G10_ShooterBackdrop
    {
        ShardField,
        SporeGrove,
        Ironworks
    }

    internal sealed class G10_SolarShooterStage
    {
        public G10_SolarShooterStage(
            string name,
            G10_ShooterBackdrop backdrop,
            int enemiesToDefeat,
            int bossHealth,
            string bossName,
            float bossTempo,
            float bossAmplitude,
            float bossShotInterval,
            int bossScore,
            float spawnInterval,
            IG10_BossAttack bossAttack)
        {
            Name = name;
            Backdrop = backdrop;
            EnemiesToDefeat = enemiesToDefeat;
            BossHealth = bossHealth;
            BossName = bossName;
            BossTempo = bossTempo;
            BossAmplitude = bossAmplitude;
            BossShotInterval = bossShotInterval;
            BossScore = bossScore;
            SpawnInterval = spawnInterval;
            BossAttack = bossAttack;
        }

        public string Name { get; private set; }
        public G10_ShooterBackdrop Backdrop { get; private set; }
        public int EnemiesToDefeat { get; private set; }
        public int BossHealth { get; private set; }
        public string BossName { get; private set; }
        public float BossTempo { get; private set; }
        public float BossAmplitude { get; private set; }
        public float BossShotInterval { get; private set; }
        public int BossScore { get; private set; }
        public float SpawnInterval { get; private set; }
        public IG10_BossAttack BossAttack { get; private set; }

        public static IReadOnlyList<G10_SolarShooterStage> All { get; } =
            Array.AsReadOnly(new[]
            {
                new G10_SolarShooterStage("RIFT", G10_ShooterBackdrop.ShardField, 10, 15, "THE DRIFTER",
                    0.8f, 38, 1.05f, 1500, 1.05f, new G10_AimedBossAttack()),
                new G10_SolarShooterStage("CANOPY", G10_ShooterBackdrop.SporeGrove, 12, 19, "ROOT ENGINE",
                    0.98f, 35, 0.89f, 2000, 0.93f, new G10_SpreadBossAttack(false)),
                new G10_SolarShooterStage("IRONWORKS", G10_ShooterBackdrop.Ironworks, 14, 24, "NULL FOUNDRY",
                    1.16f, 32, 0.73f, 2500, 0.81f, new G10_SpreadBossAttack(true))
            });
    }

    internal sealed class G10_ShooterProgress
    {
        public int Score { get; private set; }
        public int EnemiesDefeated { get; private set; }

        public G10_ShooterProgress(int score)
        {
            Score = Math.Max(0, score);
        }

        public bool RegisterEnemyDefeated(int pointValue)
        {
            Score += pointValue;
            EnemiesDefeated++;
            return EnemiesDefeated % 4 == 0;
        }

        public void RegisterBossDefeated(int pointValue)
        {
            Score += pointValue;
        }

        public void RegisterPowerCellOverflow(int pointValue)
        {
            Score += pointValue;
        }

        public void ApplyPenalty(int points)
        {
            Score = Math.Max(0, Score - Math.Max(0, points));
        }

        public void AddScore(int points)
        {
            Score += Math.Max(0, points);
        }

        public void ResetStage()
        {
            EnemiesDefeated = 0;
        }
    }
}
