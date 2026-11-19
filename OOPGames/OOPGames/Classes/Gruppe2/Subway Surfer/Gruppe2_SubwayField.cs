using System;
using System.Collections.Generic;

namespace OOPGames
{
    public sealed class Gruppe2_SubwayField : IGruppe2_SubwayField
    {
        internal const int GuardIntroFollowDurationTicks = 25;
        internal const int GuardIntroFallBehindDurationTicks = 75;
        internal const int GuardIntroDurationTicks =
            GuardIntroFollowDurationTicks + GuardIntroFallBehindDurationTicks;
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
        public double DropHeight
        {
            get
            {
                if (DropTicks <= 0 || DropDurationTicks <= 0)
                {
                    return 0;
                }

                // Parabola like a real fall: a small hop forward, then down onto the track.
                const double Hop = 0.6;
                double progress = 1 - DropTicks / (double)DropDurationTicks;
                return 1 + Hop * progress - (1 + Hop) * progress * progress;
            }
        }
        public double JetpackHeight
        {
            get
            {
                if (JetpackTicks <= 0 || JetpackDurationTicks <= 0)
                {
                    return 0;
                }

                // Climb at the start, sink at the end - both with the same smooth arc.
                int climb = Gruppe2_SubwayTrainLayout.JetpackClimbTicks;
                int elapsed = JetpackDurationTicks - JetpackTicks;
                double progress = Math.Min(1.0, Math.Min(elapsed, JetpackTicks) / (double)climb);
                return progress * progress * (3 - 2 * progress);
            }
        }
        public int Score { get; internal set; }
        public int Coins { get; internal set; }
        public double Speed { get; internal set; }
        public double GuardGap { get; internal set; }
        public bool GuardVisible { get { return GuardIntroTicks > 0 || GuardGap < 92; } }
        public bool GameOver { get; internal set; }
        public string Message { get; internal set; }
        public IReadOnlyList<Gruppe2_SubwayObstacle> Obstacles { get { return _obstacles; } }
        public IReadOnlyList<Gruppe2_SubwayReward> Rewards { get { return _rewards; } }
        public IReadOnlyDictionary<Gruppe2_SubwayUpgradeKind, int> ActiveUpgrades { get { return _activeUpgrades; } }
        internal List<Gruppe2_SubwayObstacle> MutableObstacles { get { return _obstacles; } }
        internal List<Gruppe2_SubwayReward> MutableRewards { get { return _rewards; } }
        internal Dictionary<Gruppe2_SubwayUpgradeKind, int> MutableUpgrades { get { return _activeUpgrades; } }
        public double Distance { get; internal set; }
        internal int AnimationTicks { get; set; }
        internal int JumpTicks { get; set; }
        internal int JumpDurationTicks { get; set; }
        internal int DropTicks { get; set; }
        internal int DropDurationTicks { get; set; }
        public int RollTicks { get; internal set; }
        internal int JetpackTicks { get; set; }
        internal int JetpackDurationTicks { get; set; }
        internal int GuardIntroTicks { get; set; }
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
            AnimationTicks = 0;
            JumpTicks = 0;
            JumpDurationTicks = 0;
            DropTicks = 0;
            DropDurationTicks = 0;
            RollTicks = 0;
            JetpackTicks = 0;
            JetpackDurationTicks = 0;
            GuardIntroTicks = GuardIntroDurationTicks;
            MessageTicks = 100;
        }
    }
}
