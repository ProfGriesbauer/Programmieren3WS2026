using System;
using System.Collections.Generic;

namespace OOPGames
{
    public sealed class Gruppe2_SubwayField : IGruppe2_SubwayField
    {
        private readonly List<Gruppe2_SubwayObstacle> _obstacles = new List<Gruppe2_SubwayObstacle>();
        private readonly List<Gruppe2_SubwayReward> _rewards = new List<Gruppe2_SubwayReward>();
        private readonly Dictionary<Gruppe2_SubwayUpgradeKind, int> _activeUpgrades =
            new Dictionary<Gruppe2_SubwayUpgradeKind, int>();

        public int Lane { get; internal set; }
        public Gruppe2_SubwayLevel Level { get; internal set; }
        public double JumpHeight
        {
            get
            {
                if (JumpTicks <= 0 || JumpDurationTicks <= 0)
                {
                    return 0;
                }

                double progress = 1 - JumpTicks / (double)JumpDurationTicks;
                return Math.Sin(Math.PI * progress);
            }
        }
        public int Score { get; internal set; }
        public int Coins { get; internal set; }
        public double Speed { get; internal set; }
        public double GuardGap { get; internal set; }
        public bool GuardVisible { get { return GuardGap < 92; } }
        public bool GameOver { get; internal set; }
        public string Message { get; internal set; }
        public IReadOnlyList<Gruppe2_SubwayObstacle> Obstacles { get { return _obstacles; } }
        public IReadOnlyList<Gruppe2_SubwayReward> Rewards { get { return _rewards; } }
        public IReadOnlyDictionary<Gruppe2_SubwayUpgradeKind, int> ActiveUpgrades { get { return _activeUpgrades; } }
        internal List<Gruppe2_SubwayObstacle> MutableObstacles { get { return _obstacles; } }
        internal List<Gruppe2_SubwayReward> MutableRewards { get { return _rewards; } }
        internal Dictionary<Gruppe2_SubwayUpgradeKind, int> MutableUpgrades { get { return _activeUpgrades; } }
        public double Distance { get; internal set; }
        internal int JumpTicks { get; set; }
        internal int JumpDurationTicks { get; set; }
        public int RollTicks { get; internal set; }
        internal int RoofTicks { get; set; }
        internal int JetpackTicks { get; set; }
        public int MessageTicks { get; internal set; }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is Gruppe2_SubwayPainter;
        }

        internal bool HasUpgrade(Gruppe2_SubwayUpgradeKind upgrade)
        {
            return _activeUpgrades.ContainsKey(upgrade);
        }

        internal void Reset()
        {
            _obstacles.Clear();
            _rewards.Clear();
            _activeUpgrades.Clear();
            Lane = 1;
            Level = Gruppe2_SubwayLevel.Track;
            Score = 0;
            Coins = 0;
            Speed = 1;
            GuardGap = 100;
            GameOver = false;
            Message = "Steuerung: A/D wechseln, W springen, S rollen";
            Distance = 0;
            JumpTicks = 0;
            JumpDurationTicks = 0;
            RollTicks = 0;
            RoofTicks = 0;
            JetpackTicks = 0;
            MessageTicks = 100;
        }
    }
}
