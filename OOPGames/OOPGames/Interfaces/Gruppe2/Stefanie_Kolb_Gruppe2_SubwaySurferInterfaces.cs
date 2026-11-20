using System.Collections.Generic;
using System.Windows.Input;

namespace OOPGames
{
    public enum Gruppe2_TrainRunLevel
    {
        Track,
        TrainRoof,
        Air
    }

    public enum Gruppe2_TrainRunObstacleKind
    {
        SmallBarrier,
        LargeBarrier,
        Bush,
        PlantPot,
        Train,
        CargoRamp,
        Curb,
        CenterPillar,
        Signal,
        CargoWagon
    }

    public enum Gruppe2_TrainRunRewardKind
    {
        Coin,
        UpgradeBox
    }

    public enum Gruppe2_TrainRunUpgradeKind
    {
        Magnet,
        DoubleCoins,
        Surfboard,
        JumpStick,
        Jetpack
    }

    public interface IGruppe2_TrainRunMove : IPlayMove
    {
        Key Key { get; }
    }

    public interface IGruppe2_TrainRunField : IGameField
    {
        int Lane { get; }
        Gruppe2_TrainRunLevel Level { get; }
        double JumpHeight { get; }
        /// <summary>
        /// Height while dropping off a train roof, as a fraction of the roof height:
        /// starts at 1, rises slightly (small arc) and ends at 0 on the track.
        /// </summary>
        double DropHeight { get; }
        /// <summary>
        /// Jetpack flight: 0 on the ground, rises in an arc to 1 (flight level above the
        /// train roofs), stays at 1 while flying and sinks back in the same arc at the end.
        /// </summary>
        double JetpackHeight { get; }
        int Score { get; }
        int Coins { get; }
        double Distance { get; }
        double Speed { get; }
        double GuardGap { get; }
        int RollTicks { get; }
        int MessageTicks { get; }
        bool GuardVisible { get; }
        bool GameOver { get; }
        string Message { get; }
        IReadOnlyList<Gruppe2_TrainRunObstacle> Obstacles { get; }
        IReadOnlyList<Gruppe2_TrainRunReward> Rewards { get; }
        IReadOnlyDictionary<Gruppe2_TrainRunUpgradeKind, int> ActiveUpgrades { get; }
    }

    public sealed class Gruppe2_TrainRunObstacle
    {
        internal Gruppe2_TrainRunObstacle(
            Gruppe2_TrainRunObstacleKind kind,
            int lane,
            double distance,
            bool moving = false)
        {
            Kind = kind;
            Lane = lane;
            Distance = distance;
            Moving = moving;
        }

        public Gruppe2_TrainRunObstacleKind Kind { get; }
        public int Lane { get; }
        public double Distance { get; internal set; }
        public bool Moving { get; }
    }

    public sealed class Gruppe2_TrainRunReward
    {
        internal Gruppe2_TrainRunReward(
            Gruppe2_TrainRunRewardKind kind,
            int lane,
            double distance,
            Gruppe2_TrainRunUpgradeKind upgrade = Gruppe2_TrainRunUpgradeKind.Magnet,
            bool elevated = false,
            bool onRoof = false)
        {
            Kind = kind;
            Lane = lane;
            Distance = distance;
            Upgrade = upgrade;
            Elevated = elevated;
            OnRoof = onRoof;
        }

        public Gruppe2_TrainRunRewardKind Kind { get; }
        public int Lane { get; }
        public double Distance { get; internal set; }
        public Gruppe2_TrainRunUpgradeKind Upgrade { get; }
        /// <summary>True for coins floating at jetpack flight level; only collectable while flying.</summary>
        public bool Elevated { get; }
        /// <summary>True for coins lying on a train roof; only collectable while on the roof.</summary>
        public bool OnRoof { get; }
    }
}
