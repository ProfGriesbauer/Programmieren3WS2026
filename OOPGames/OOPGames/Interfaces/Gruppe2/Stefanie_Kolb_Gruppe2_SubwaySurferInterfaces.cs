using System.Collections.Generic;
using System.Windows.Input;

namespace OOPGames
{
    public enum Gruppe2_SubwayLevel
    {
        Track,
        TrainRoof,
        Air
    }

    public enum Gruppe2_SubwayObstacleKind
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

    public enum Gruppe2_SubwayRewardKind
    {
        Coin,
        UpgradeBox
    }

    public enum Gruppe2_SubwayUpgradeKind
    {
        Magnet,
        DoubleCoins,
        Surfboard,
        JumpStick,
        Jetpack
    }

    public interface IGruppe2_SubwayMove : IPlayMove
    {
        Key Key { get; }
    }

    public interface IGruppe2_SubwayField : IGameField
    {
        int Lane { get; }
        Gruppe2_SubwayLevel Level { get; }
        double JumpHeight { get; }
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
        IReadOnlyList<Gruppe2_SubwayObstacle> Obstacles { get; }
        IReadOnlyList<Gruppe2_SubwayReward> Rewards { get; }
        IReadOnlyDictionary<Gruppe2_SubwayUpgradeKind, int> ActiveUpgrades { get; }
    }

    public sealed class Gruppe2_SubwayObstacle
    {
        internal Gruppe2_SubwayObstacle(
            Gruppe2_SubwayObstacleKind kind,
            int lane,
            double distance,
            bool moving = false)
        {
            Kind = kind;
            Lane = lane;
            Distance = distance;
            Moving = moving;
        }

        public Gruppe2_SubwayObstacleKind Kind { get; }
        public int Lane { get; }
        public double Distance { get; internal set; }
        public bool Moving { get; }
    }

    public sealed class Gruppe2_SubwayReward
    {
        internal Gruppe2_SubwayReward(
            Gruppe2_SubwayRewardKind kind,
            int lane,
            double distance,
            Gruppe2_SubwayUpgradeKind upgrade = Gruppe2_SubwayUpgradeKind.Magnet)
        {
            Kind = kind;
            Lane = lane;
            Distance = distance;
            Upgrade = upgrade;
        }

        public Gruppe2_SubwayRewardKind Kind { get; }
        public int Lane { get; }
        public double Distance { get; internal set; }
        public Gruppe2_SubwayUpgradeKind Upgrade { get; }
    }
}
