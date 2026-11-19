using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Data of one stage. The stages differ only in data; the rules are the same everywhere.
    public sealed class G10_BomberStage
    {
        public G10_BomberStage(double brickDensity, G10_BomberItemKind powerUp, G10_BomberEnemyKind doorGuard, params G10_BomberEnemyKind[] enemies)
        {
            if (brickDensity < 0 || brickDensity > 0.9) throw new ArgumentOutOfRangeException(nameof(brickDensity));
            if (powerUp == G10_BomberItemKind.None || powerUp == G10_BomberItemKind.Exit) throw new ArgumentException("A stage hides a real power-up.", nameof(powerUp));
            if (enemies == null || enemies.Length == 0) throw new ArgumentException("A stage needs enemies.", nameof(enemies));
            BrickDensity = brickDensity;
            PowerUp = powerUp;
            DoorGuard = doorGuard;
            Enemies = enemies;
        }

        public double BrickDensity { get; private set; }
        public G10_BomberItemKind PowerUp { get; private set; }
        // Enemies of this kind come out when a flame hits the exit.
        public G10_BomberEnemyKind DoorGuard { get; private set; }
        public IReadOnlyList<G10_BomberEnemyKind> Enemies { get; private set; }
    }

    public static class G10_BomberStages
    {
        static readonly G10_BomberStage[] _All =
        {
            new G10_BomberStage(0.30, G10_BomberItemKind.FireUp, G10_BomberEnemyKind.Spark,
                G10_BomberEnemyKind.Puff, G10_BomberEnemyKind.Puff, G10_BomberEnemyKind.Puff,
                G10_BomberEnemyKind.Puff, G10_BomberEnemyKind.Puff),
            new G10_BomberStage(0.35, G10_BomberItemKind.BombUp, G10_BomberEnemyKind.Spark,
                G10_BomberEnemyKind.Puff, G10_BomberEnemyKind.Puff, G10_BomberEnemyKind.Puff,
                G10_BomberEnemyKind.Spark, G10_BomberEnemyKind.Spark),
            new G10_BomberStage(0.40, G10_BomberItemKind.SpeedUp, G10_BomberEnemyKind.Drift,
                G10_BomberEnemyKind.Puff, G10_BomberEnemyKind.Puff, G10_BomberEnemyKind.Spark,
                G10_BomberEnemyKind.Spark, G10_BomberEnemyKind.Drift, G10_BomberEnemyKind.Drift),
            new G10_BomberStage(0.40, G10_BomberItemKind.Remote, G10_BomberEnemyKind.Hunter,
                G10_BomberEnemyKind.Spark, G10_BomberEnemyKind.Spark, G10_BomberEnemyKind.Spark,
                G10_BomberEnemyKind.Drift, G10_BomberEnemyKind.Drift, G10_BomberEnemyKind.Drift),
            new G10_BomberStage(0.45, G10_BomberItemKind.BombUp, G10_BomberEnemyKind.Hunter,
                G10_BomberEnemyKind.Spark, G10_BomberEnemyKind.Spark, G10_BomberEnemyKind.Drift,
                G10_BomberEnemyKind.Drift, G10_BomberEnemyKind.Hunter, G10_BomberEnemyKind.Hunter),
            new G10_BomberStage(0.45, G10_BomberItemKind.FireUp, G10_BomberEnemyKind.Hunter,
                G10_BomberEnemyKind.Drift, G10_BomberEnemyKind.Drift, G10_BomberEnemyKind.Drift,
                G10_BomberEnemyKind.Hunter, G10_BomberEnemyKind.Hunter, G10_BomberEnemyKind.Hunter),
            new G10_BomberStage(0.50, G10_BomberItemKind.SpeedUp, G10_BomberEnemyKind.Phantom,
                G10_BomberEnemyKind.Spark, G10_BomberEnemyKind.Spark, G10_BomberEnemyKind.Hunter,
                G10_BomberEnemyKind.Hunter, G10_BomberEnemyKind.Hunter, G10_BomberEnemyKind.Phantom),
            new G10_BomberStage(0.50, G10_BomberItemKind.FireUp, G10_BomberEnemyKind.Phantom,
                G10_BomberEnemyKind.Drift, G10_BomberEnemyKind.Drift, G10_BomberEnemyKind.Hunter,
                G10_BomberEnemyKind.Hunter, G10_BomberEnemyKind.Hunter, G10_BomberEnemyKind.Phantom,
                G10_BomberEnemyKind.Phantom)
        };

        public static int Count { get { return _All.Length; } }

        // stage is 1-based.
        public static G10_BomberStage Get(int stage)
        {
            if (stage < 1 || stage > _All.Length) throw new ArgumentOutOfRangeException(nameof(stage));
            return _All[stage - 1];
        }
    }
}
