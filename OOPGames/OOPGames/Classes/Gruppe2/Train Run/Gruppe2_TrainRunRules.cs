using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class Gruppe2_TrainRunRules : IGameRules2
    {
        private const double SignalLaneChangeCollisionDistance = 8;
        private readonly Random _random = new Random();
        private static readonly Gruppe2_TrainRunObstacleKind[] SpawnableObstacleKinds =
        {
            Gruppe2_TrainRunObstacleKind.SmallBarrier,
            Gruppe2_TrainRunObstacleKind.LargeBarrier,
            Gruppe2_TrainRunObstacleKind.Bush,
            Gruppe2_TrainRunObstacleKind.PlantPot,
            Gruppe2_TrainRunObstacleKind.Train,
            Gruppe2_TrainRunObstacleKind.CargoRamp,
            Gruppe2_TrainRunObstacleKind.Curb,
            Gruppe2_TrainRunObstacleKind.CargoWagon
        };
        // Every object (trains, obstacles, signals, coins) appears on the same line near
        // the horizon of the 3D scene; the longest convoy still ends inside the visible track.
        private const double SpawnDistance = 420;
        private const double WaveSpacing = 74;
        // Free space kept between a new object and a train already standing in the lane.
        private const double TrainClearance = 10;
        // Free track behind the end of a train: the drop from the roof takes about 22 units,
        // after landing the runner needs time to react. Scaled with the speed at spawn time.
        private const double ReactionDistanceAfterTrain = 60;
        private bool _trackPrefilled;
        private const int MinimumDropTicks = 12;
        // Short air time: the runner is back on the ground quickly (25 frames per second).
        // Coins stay visible behind the runner until they have left the camera view.
        private const double RewardRemoveBehindDistance = -45;
        private const double RewardCollectBehindDistance = -4;
        private const int UpgradeDurationTicks = 220;
        // At least about 12 seconds (25 ticks per second) between two upgrade coins.
        private const int UpgradeSpawnPauseTicks = 300;
        private static readonly Dictionary<Gruppe2_TrainRunUpgradeKind, int> UpgradeWeights =
            new Dictionary<Gruppe2_TrainRunUpgradeKind, int>
            {
                { Gruppe2_TrainRunUpgradeKind.Magnet, 3 },
                { Gruppe2_TrainRunUpgradeKind.DoubleCoins, 3 },
                { Gruppe2_TrainRunUpgradeKind.Surfboard, 2 },
                { Gruppe2_TrainRunUpgradeKind.JumpStick, 2 },
                { Gruppe2_TrainRunUpgradeKind.Jetpack, 1 }
            };
        private const int NormalJumpTicks = 20;
        private const int JumpStickJumpTicks = 30;
        // Only one of these can be active at a time; magnet and double coins combine freely.
        private static readonly Gruppe2_TrainRunUpgradeKind[] ExclusiveUpgrades =
        {
            Gruppe2_TrainRunUpgradeKind.JumpStick,
            Gruppe2_TrainRunUpgradeKind.Jetpack,
            Gruppe2_TrainRunUpgradeKind.Surfboard
        };
        private Gruppe2_TrainRunField _field = new Gruppe2_TrainRunField();
        private readonly HashSet<Gruppe2_TrainRunObstacle> _passedTrainParts =
            new HashSet<Gruppe2_TrainRunObstacle>();
        private readonly HashSet<Gruppe2_TrainRunReward> _missedRewards =
            new HashSet<Gruppe2_TrainRunReward>();
        // State of the coin trail at jetpack flight level (see ExtendJetpackCoinTrail).
        private int _ticksSinceUpgradeSpawn;
        private bool _jetpackTrailActive;
        private double _jetpackTrailNext;
        private int _jetpackTrailLane;
        private int _jetpackTrailCoinsInRow;
        private double _nextWaveAt;

        public string Name { get { return "Gruppe 2 - Train Run Regeln"; } }
        public IGameField CurrentField { get { return _field; } }
        public bool MovesPossible { get { return !_field.GameOver; } }

        public void StartedGameCall()
        {
        }

        public void ClearField()
        {
            _field.Reset();
            _passedTrainParts.Clear();
            _missedRewards.Clear();
            _jetpackTrailActive = false;
            _ticksSinceUpgradeSpawn = 0;
            _trackPrefilled = false;
            _nextWaveAt = 25;
        }

        public void DoMove(IPlayMove move)
        {
            IGruppe2_TrainRunMove trainRunMove = move as IGruppe2_TrainRunMove;
            if (trainRunMove == null || !MovesPossible
                || trainRunMove.PlayerNumber < 1 || trainRunMove.PlayerNumber > 2)
            {
                return;
            }

            switch (trainRunMove.Key)
            {
                case Key.Left:
                case Key.A:
                    ChangeLane(-1);
                    break;
                case Key.Right:
                case Key.D:
                    ChangeLane(1);
                    break;
                case Key.Up:
                case Key.W:
                    bool onRoof = _field.Level == Gruppe2_TrainRunLevel.TrainRoof;
                    if ((_field.Level == Gruppe2_TrainRunLevel.Track || onRoof) && _field.JumpTicks == 0)
                    {
                        _field.JumpDurationTicks = _field.HasUpgrade(Gruppe2_TrainRunUpgradeKind.JumpStick)
                            ? JumpStickJumpTicks
                            : NormalJumpTicks;
                        _field.JumpTicks = _field.JumpDurationTicks;
                        _field.RollTicks = 0;
                        // A jump on a roof stays a roof jump: the arc starts at roof height and
                        // can carry the runner over a gap onto the next train.
                        if (!onRoof)
                        {
                            _field.Level = Gruppe2_TrainRunLevel.Air;
                        }
                    }
                    break;
                case Key.Down:
                case Key.S:
                    if (_field.JetpackTicks > 0)
                    {
                        // No rolling or landing while flying with the jetpack.
                        break;
                    }

                    _field.RollTicks = 24;
                    if (_field.Level == Gruppe2_TrainRunLevel.TrainRoof)
                    {
                        // Cancels a roof jump; above a gap the runner then drops off.
                        EndJump();
                    }
                    else if (_field.Level == Gruppe2_TrainRunLevel.Air)
                    {
                        EndJump();
                        _field.DropTicks = 0;
                        Land();
                    }
                    break;
                default:
                    return;
            }
        }

        public int CheckIfPLayerWon()
        {
            return -1;
        }

        public void TickGameCall()
        {
            if (_field.GameOver)
            {
                return;
            }

            _field.AnimationTicks++;
            _ticksSinceUpgradeSpawn++;
            _field.Distance += _field.Speed;
            _field.Score = (int)_field.Distance + _field.Coins * 10;
            // The speed keeps rising with the score, without an upper limit.
            _field.Speed = 1 + _field.Score / 1500.0;

            TickTimers();
            SpawnWaves();
            ExtendJetpackCoinTrail();
            AdvanceObjects();
            CollectRewards();
            ResolveObstacles();
            UpdateTrainRoof();

            if (_field.GuardGap < 100)
            {
                _field.GuardGap = Math.Min(100, _field.GuardGap + 0.08);
            }

            if (_field.MessageTicks > 0)
            {
                _field.MessageTicks--;
            }
        }

        private void TickTimers()
        {
            DecreaseTimers(_field.MutableUpgrades);
            _field.GuardIntroTicks = Math.Max(0, _field.GuardIntroTicks - 1);

            _field.JumpTicks = Math.Max(0, _field.JumpTicks - 1);
            _field.DropTicks = Math.Max(0, _field.DropTicks - 1);
            _field.RollTicks = Math.Max(0, _field.RollTicks - 1);
            _field.JetpackTicks = Math.Max(0, _field.JetpackTicks - 1);

            // Sinking at the end of a jetpack flight: if a train is below once the runner
            // is down to roof height, the flight ends on its roof.
            bool sinking = _field.JetpackTicks > 0 && _field.JetpackTicks < Gruppe2_TrainRunLayout.JetpackClimbTicks;
            if (sinking
                && _field.JetpackHeight * Gruppe2_TrainRunLayout.JetpackAltitudeInRoofHeights <= 1
                && Gruppe2_TrainRunLayout.IsCarUnderRunner(_field.Obstacles, _field.Lane))
            {
                _field.JetpackTicks = 0;
                _field.MutableUpgrades.Remove(Gruppe2_TrainRunUpgradeKind.Jetpack);
            }

            bool stillInAir = _field.JumpTicks > 0 || _field.DropTicks > 0 || _field.JetpackTicks > 0;
            if (_field.Level == Gruppe2_TrainRunLevel.Air && !stillInAir)
            {
                Land();
            }
        }

        /// <summary>
        /// A train roof can only be reached without a ramp by jumping with the
        /// jump stick, or by flying with the jetpack. A normal jump is too low.
        /// </summary>
        private bool CanReachTrainRoof()
        {
            bool jumpingWithStick = _field.JumpTicks > 0
                && _field.HasUpgrade(Gruppe2_TrainRunUpgradeKind.JumpStick);
            return jumpingWithStick || _field.JetpackTicks > 0;
        }

        /// <summary>
        /// The front of a car reaches the runner: walk on (roof), fly over (jetpack),
        /// land on the roof (jump stick) or crash.
        /// </summary>
        private void ReachCarFront(string crashMessage)
        {
            if (_field.Level == Gruppe2_TrainRunLevel.TrainRoof || _field.JetpackTicks > 0)
            {
                return;
            }

            if (!CanReachTrainRoof())
            {
                HitOrEnd(true, crashMessage);
                return;
            }

            ClimbOntoRoof("Mit dem Huepfstab aufs Zugdach!");
        }

        /// <summary>Stops a running jump (the jump arc ends at once).</summary>
        private void EndJump()
        {
            _field.JumpTicks = 0;
            _field.JumpDurationTicks = 0;
        }

        /// <summary>Puts the runner onto the train roof (via the ramp or with the jump stick).</summary>
        private void ClimbOntoRoof(string message)
        {
            EndJump();
            _field.Level = Gruppe2_TrainRunLevel.TrainRoof;
            SetMessage(message);
        }

        private void Land()
        {
            _field.Level = Gruppe2_TrainRunLayout.IsCarUnderRunner(_field.Obstacles, _field.Lane)
                ? Gruppe2_TrainRunLevel.TrainRoof
                : Gruppe2_TrainRunLevel.Track;
        }

        private void UpdateTrainRoof()
        {
            // While jumping on the roof the runner is in the air and does not fall into a gap;
            // only when the jump ends without a car below does the drop start.
            if (_field.Level == Gruppe2_TrainRunLevel.TrainRoof
                && _field.JumpTicks == 0
                && !Gruppe2_TrainRunLayout.IsCarUnderRunner(_field.Obstacles, _field.Lane))
            {
                // Drop off in a small arc. The fall lasts until the car that was just left
                // is behind the camera, so the camera never ends up inside it.
                double dropDistance = -Gruppe2_TrainRunLayout.TrainRemoveBehindDistance + 2;
                _field.DropDurationTicks = Math.Max(MinimumDropTicks,
                    (int)Math.Ceiling(dropDistance / _field.Speed));
                _field.DropTicks = _field.DropDurationTicks;
                _field.Level = Gruppe2_TrainRunLevel.Air;
                SetMessage("Das Zugdach endet - zurueck auf die Gleise!");
            }
        }

        private static void DecreaseTimers(Dictionary<Gruppe2_TrainRunUpgradeKind, int> upgrades)
        {
            foreach (Gruppe2_TrainRunUpgradeKind upgrade in upgrades.Keys.ToArray())
            {
                upgrades[upgrade]--;
                if (upgrades[upgrade] <= 0)
                {
                    upgrades.Remove(upgrade);
                }
            }
        }

        private void SpawnWaves()
        {
            if (!_trackPrefilled)
            {
                // At the start the track between the runner and the spawn line is filled,
                // so the first obstacles do not need the whole way from the horizon.
                _trackPrefilled = true;
                for (double distance = SpawnDistance - 4 * WaveSpacing; distance < SpawnDistance; distance += WaveSpacing)
                {
                    SpawnWave(distance);
                }
            }

            while (_field.Distance >= _nextWaveAt)
            {
                SpawnWave(SpawnDistance);
                _nextWaveAt += WaveSpacing;
            }
        }

        /// <summary>
        /// One wave at the given distance: obstacles or trains on one or two lanes,
        /// coins or an upgrade box on the safe lane.
        /// The safe lane never has a train ahead, and trains are only placed on other lanes,
        /// so at least one lane always stays free of trains.
        /// </summary>
        private void SpawnWave(double distance)
        {
            List<int> trainFreeLanes = Enumerable.Range(0, 3)
                .Where(lane => !Gruppe2_TrainRunLayout.HasTrainAhead(_field.Obstacles, lane))
                .ToList();
            int safeLane = trainFreeLanes.Count > 0
                ? trainFreeLanes[_random.Next(trainFreeLanes.Count)]
                : _random.Next(0, 3);
            List<int> blockedLanes = Enumerable.Range(0, 3)
                .Where(lane => lane != safeLane)
                .OrderBy(_ => _random.Next())
                .Take(_random.Next(1, 3))
                .ToList();

            foreach (int lane in blockedLanes)
            {
                Gruppe2_TrainRunObstacleKind kind =
                    SpawnableObstacleKinds[_random.Next(SpawnableObstacleKinds.Length)];
                switch (kind)
                {
                    case Gruppe2_TrainRunObstacleKind.CargoRamp:
                        // Ramp, one to five cargo wagons and a locomotive: walkable, different lengths.
                        SpawnTrain(lane, distance, true, _random.Next(1, 6));
                        break;
                    case Gruppe2_TrainRunObstacleKind.Train:
                        // Locomotive in front pulling one to four wagons: has to be dodged.
                        SpawnTrain(lane, distance, false, _random.Next(1, 5));
                        break;
                    default:
                        double length = Gruppe2_TrainRunLayout.IsCar(kind) ? Gruppe2_TrainRunLayout.CarSpacing : 0;
                        if (IsLaneFree(lane, distance, distance + length))
                        {
                            _field.MutableObstacles.Add(new Gruppe2_TrainRunObstacle(kind, lane, distance));
                        }
                        break;
                }
            }

            if (_random.Next(0, 2) == 0)
            {
                _field.MutableObstacles.Add(new Gruppe2_TrainRunObstacle(
                    Gruppe2_TrainRunObstacleKind.Signal, _random.Next(0, 2), distance));
            }

            // Upgrades are rare: a fixed pause in ticks (independent of the speed, because waves
            // come faster the faster the runner is), then a chance per wave.
            if (_ticksSinceUpgradeSpawn >= UpgradeSpawnPauseTicks && _random.Next(0, 3) == 0)
            {
                _ticksSinceUpgradeSpawn = 0;
                _field.MutableRewards.Add(new Gruppe2_TrainRunReward(
                    Gruppe2_TrainRunRewardKind.UpgradeBox, safeLane, distance, ChooseUpgrade()));
            }
            else
            {
                int coinCount = _random.Next(3, 7);
                for (int coin = 0; coin < coinCount; coin++)
                {
                    _field.MutableRewards.Add(new Gruppe2_TrainRunReward(
                        Gruppe2_TrainRunRewardKind.Coin, safeLane, distance + coin * 5));
                }
            }
        }

        /// <summary>
        /// Places a train whose front is at <paramref name="front"/>.
        /// With ramp: ramp, wagons, locomotive at the end (the runner walks up and over).
        /// Without ramp: locomotive in front, wagons behind (the runner has to dodge).
        /// Nothing is placed if another train already stands in that part of the lane.
        /// </summary>
        private void SpawnTrain(int lane, double front, bool withRamp, int wagonCount)
        {
            double spacing = Gruppe2_TrainRunLayout.CarSpacing;
            double start = withRamp ? front - Gruppe2_TrainRunLayout.RampLength : front;
            double end = front + (wagonCount + 1) * spacing;
            if (!IsLaneFree(lane, start, end))
            {
                return;
            }

            if (withRamp)
            {
                _field.MutableObstacles.Add(new Gruppe2_TrainRunObstacle(
                    Gruppe2_TrainRunObstacleKind.CargoRamp, lane, front));
            }

            for (int car = 0; car <= wagonCount; car++)
            {
                bool locomotive = withRamp ? car == wagonCount : car == 0;
                _field.MutableObstacles.Add(new Gruppe2_TrainRunObstacle(
                    locomotive ? Gruppe2_TrainRunObstacleKind.Train : Gruppe2_TrainRunObstacleKind.CargoWagon,
                    lane, front + car * spacing));
            }

            SpawnRoofCoins(lane, front, end);
        }

        /// <summary>
        /// A row of coins along the roofs of a train, for runners who got up there via
        /// the ramp or with the jump stick.
        /// </summary>
        private void SpawnRoofCoins(int lane, double trainStart, double trainEnd)
        {
            const double CoinSpacing = 5;
            for (double distance = trainStart + 4; distance < trainEnd - 4; distance += CoinSpacing)
            {
                _field.MutableRewards.Add(new Gruppe2_TrainRunReward(
                    Gruppe2_TrainRunRewardKind.Coin, lane, distance, onRoof: true));
            }
        }

        /// <summary>
        /// New objects always appear behind (further away than) the trains already on the
        /// track, so the gap behind the end of a train is the one that matters for reacting.
        /// </summary>
        private bool IsLaneFree(int lane, double from, double to)
        {
            double gapBehindTrain = Math.Max(TrainClearance, ReactionDistanceAfterTrain * _field.Speed);
            return !Gruppe2_TrainRunLayout.IsLaneOccupiedByTrain(
                _field.Obstacles, lane, from - gapBehindTrain, to + TrainClearance);
        }

        private void AdvanceObjects()
        {
            foreach (Gruppe2_TrainRunObstacle obstacle in _field.MutableObstacles)
            {
                obstacle.Distance -= _field.Speed;
            }

            foreach (Gruppe2_TrainRunReward reward in _field.MutableRewards)
            {
                reward.Distance -= _field.Speed;
            }

            _passedTrainParts.RemoveWhere(Gruppe2_TrainRunLayout.IsBehindRunner);
            _field.MutableObstacles.RemoveAll(Gruppe2_TrainRunLayout.IsBehindRunner);
            _field.MutableRewards.RemoveAll(reward => reward.Distance < RewardRemoveBehindDistance);
            _missedRewards.RemoveWhere(reward => reward.Distance < RewardRemoveBehindDistance);
        }

        private void CollectRewards()
        {
            foreach (Gruppe2_TrainRunReward reward in _field.MutableRewards.ToArray())
            {
                // Collected only while passing it (not later, when it is already behind the runner).
                // At high speed a coin moves further than the collection window in one tick,
                // so a coin that was in or ahead of the window at the start of this tick counts as well.
                bool passedThisTick = reward.Distance + _field.Speed > RewardCollectBehindDistance;
                bool inRange = reward.Distance <= 1
                    && (reward.Distance > RewardCollectBehindDistance || passedThisTick);
                bool attracted = reward.Kind == Gruppe2_TrainRunRewardKind.Coin
                    && _field.HasUpgrade(Gruppe2_TrainRunUpgradeKind.Magnet)
                    && Math.Abs(reward.Lane - _field.Lane) <= 1
                    && reward.Distance < Gruppe2_TrainRunLayout.MagnetRange;

                // Without the magnet only coins on the runner's own height level count
                // (ground, train roof or jetpack flight level). The magnet pulls coins in
                // from every level and from the neighbouring lanes.
                bool sameSpot = reward.Lane == _field.Lane
                    && Gruppe2_TrainRunLayout.LevelOf(reward) == Gruppe2_TrainRunLayout.LevelOf(_field);
                if (!inRange || (!sameSpot && !attracted))
                {
                    continue;
                }

                if (reward.Kind == Gruppe2_TrainRunRewardKind.Coin)
                {
                    _field.Coins += _field.HasUpgrade(Gruppe2_TrainRunUpgradeKind.DoubleCoins) ? 2 : 1;
                    SetMessage("Muenze gesammelt!");
                }
                else if (_missedRewards.Contains(reward) || IsAboveUpgradeCoin(reward)
                    || IsBlockedByOtherExclusiveUpgrade(reward.Upgrade))
                {
                    // Jumped over, or blocked because jump stick, jetpack and surfboard exclude
                    // each other: the upgrade coin stays on the track and is missed for good -
                    // also when landing right behind it or when the active upgrade runs out.
                    _missedRewards.Add(reward);
                    continue;
                }
                else
                {
                    GrantUpgrade(reward.Upgrade);
                }

                _field.MutableRewards.Remove(reward);
            }
        }

        /// <summary>
        /// True if the runner passes over an upgrade coin instead of running into it: high in a
        /// jump, dropping off a roof, flying with the jetpack or on another height level.
        /// At the very start and end of a jump the feet are still low enough to take it.
        /// </summary>
        private bool IsAboveUpgradeCoin(Gruppe2_TrainRunReward reward)
        {
            const double ClearanceHeight = 0.35;
            bool sameLevel = Gruppe2_TrainRunLayout.LevelOf(reward) == Gruppe2_TrainRunLayout.LevelOf(_field);
            return !sameLevel
                || _field.JumpHeight > ClearanceHeight
                || _field.DropHeight > ClearanceHeight
                || _field.JetpackHeight > ClearanceHeight;
        }

        /// <summary>
        /// Picks an upgrade by weight (the jetpack is the rarest). Upgrades blocked by the active
        /// jump stick / jetpack / surfboard do not appear at all.
        /// </summary>
        private Gruppe2_TrainRunUpgradeKind ChooseUpgrade()
        {
            List<KeyValuePair<Gruppe2_TrainRunUpgradeKind, int>> available = UpgradeWeights
                .Where(entry => !IsBlockedByOtherExclusiveUpgrade(entry.Key))
                .ToList();
            int roll = _random.Next(available.Sum(entry => entry.Value));
            foreach (KeyValuePair<Gruppe2_TrainRunUpgradeKind, int> entry in available)
            {
                if (roll < entry.Value)
                {
                    return entry.Key;
                }

                roll -= entry.Value;
            }

            return Gruppe2_TrainRunUpgradeKind.Magnet;
        }

        private bool IsBlockedByOtherExclusiveUpgrade(Gruppe2_TrainRunUpgradeKind upgrade)
        {
            return ExclusiveUpgrades.Contains(upgrade)
                && ExclusiveUpgrades.Any(active => active != upgrade && _field.HasUpgrade(active));
        }

        private void GrantUpgrade(Gruppe2_TrainRunUpgradeKind upgrade)
        {
            // All upgrades last equally long (the surfboard as long as the jetpack).
            int duration = UpgradeDurationTicks;
            _field.MutableUpgrades[upgrade] = duration;
            if (ExclusiveUpgrades.Contains(upgrade))
            {
                // The other two exclusive upgrades are blocked now: their coins vanish from the track.
                _field.MutableRewards.RemoveAll(reward => reward.Kind == Gruppe2_TrainRunRewardKind.UpgradeBox
                    && reward.Upgrade != upgrade
                    && ExclusiveUpgrades.Contains(reward.Upgrade));
            }
            switch (upgrade)
            {
                case Gruppe2_TrainRunUpgradeKind.Jetpack:
                    StartJetpackFlight(duration);
                    break;
                // The jump stick only becomes active here; the high jump happens
                // the next time the player presses jump (see DoMove).
            }

            SetMessage("Upgrade: " + UpgradeName(upgrade));
        }

        /// <summary>
        /// Jetpack: climb in an arc to the flight level above the train roofs, fly there
        /// until the upgrade runs out, then sink back in the same arc (see JetpackHeight).
        /// A trail of coins floats at flight level along the way.
        /// </summary>
        private void StartJetpackFlight(int duration)
        {
            // A second jetpack while flying keeps the current height (no new climb).
            int climb = Gruppe2_TrainRunLayout.JetpackClimbTicks;
            int currentClimb = _field.JetpackTicks > 0
                ? Math.Min(climb, Math.Min(_field.JetpackDurationTicks - _field.JetpackTicks, _field.JetpackTicks))
                : 0;
            _field.JetpackTicks = duration;
            _field.JetpackDurationTicks = duration + currentClimb;
            EndJump();
            _field.DropTicks = 0;
            _field.RollTicks = 0;
            _field.Level = Gruppe2_TrainRunLevel.Air;
            StartJetpackCoinTrail();
        }

        /// <summary>Starts (or, for a second jetpack while flying, continues) the coin trail at flight level.</summary>
        private void StartJetpackCoinTrail()
        {
            if (!_jetpackTrailActive)
            {
                _jetpackTrailActive = true;
                _jetpackTrailNext = _field.Distance + Gruppe2_TrainRunLayout.JetpackClimbTicks * _field.Speed;
                _jetpackTrailLane = _field.Lane;
                _jetpackTrailCoinsInRow = 0;
            }

            ExtendJetpackCoinTrail();
        }

        /// <summary>
        /// Coins at flight level, in short rows that switch to a neighbouring lane. The trail is
        /// extended every tick up to the spawn line, as long as the coins still arrive before the
        /// runner sinks again - so it lasts for the whole flight, however fast the runner is.
        /// </summary>
        private void ExtendJetpackCoinTrail()
        {
            const double CoinSpacing = 6;
            const int CoinsPerRow = 8;
            while (_jetpackTrailActive)
            {
                double distance = _jetpackTrailNext - _field.Distance;
                if (distance > SpawnDistance)
                {
                    return;
                }

                double cruiseTicksLeft = _field.JetpackTicks - Gruppe2_TrainRunLayout.JetpackClimbTicks;
                if (distance / _field.Speed > cruiseTicksLeft)
                {
                    _jetpackTrailActive = false;
                    return;
                }

                if (distance > 0)
                {
                    _field.MutableRewards.Add(new Gruppe2_TrainRunReward(
                        Gruppe2_TrainRunRewardKind.Coin, _jetpackTrailLane, distance, elevated: true));
                }

                _jetpackTrailNext += CoinSpacing;
                if (++_jetpackTrailCoinsInRow == CoinsPerRow)
                {
                    _jetpackTrailCoinsInRow = 0;
                    _jetpackTrailLane = _jetpackTrailLane == 0 ? 1
                        : _jetpackTrailLane == 2 ? 1
                        : _jetpackTrailLane + (_random.Next(0, 2) == 0 ? -1 : 1);
                }
            }
        }

        private void ResolveObstacles()
        {
            foreach (Gruppe2_TrainRunObstacle obstacle in _field.MutableObstacles
                .Where(item => item.Kind != Gruppe2_TrainRunObstacleKind.Signal
                    && item.Distance <= Gruppe2_TrainRunLayout.CollisionDistance
                    && !_passedTrainParts.Contains(item))
                .ToArray())
            {
                bool trainPart = Gruppe2_TrainRunLayout.IsTrainPart(obstacle.Kind);
                if (trainPart)
                {
                    // Train parts stay in the list until their far end has passed,
                    // so they are drawn under the runner; they only collide once.
                    _passedTrainParts.Add(obstacle);
                }

                if (_field.JetpackTicks > 0)
                {
                    // Flying with the jetpack: everything on the track passes below.
                    if (!trainPart)
                    {
                        _field.MutableObstacles.Remove(obstacle);
                    }

                    continue;
                }

                if (obstacle.Lane != _field.Lane)
                {
                    continue;
                }

                bool airborne = _field.Level == Gruppe2_TrainRunLevel.Air;
                bool onRoof = _field.Level == Gruppe2_TrainRunLevel.TrainRoof;
                bool rolling = _field.RollTicks > 0;

                switch (obstacle.Kind)
                {
                    case Gruppe2_TrainRunObstacleKind.Train:
                        ReachCarFront("Zug! Weiche aus oder nimm die Rampe - aufspringen nur mit Huepfstab.");
                        break;
                    case Gruppe2_TrainRunObstacleKind.CargoRamp:
                        // The runner walks up the ramp automatically; only a jetpack flies over it.
                        if (_field.JetpackTicks == 0)
                        {
                            ClimbOntoRoof("Auf dem Zugdach!");
                        }
                        break;
                    case Gruppe2_TrainRunObstacleKind.SmallBarrier:
                        HitUnless(airborne || onRoof, true, "Kleine Absperrung! Springe darueber.");
                        break;
                    case Gruppe2_TrainRunObstacleKind.CargoWagon:
                        ReachCarFront("Gueterwagen! Nimm die Rampe - aufspringen nur mit Huepfstab.");
                        break;
                    case Gruppe2_TrainRunObstacleKind.LargeBarrier:
                        HitUnless(rolling || onRoof, true, "Grosse Schranke! Nur ducken hilft.");
                        break;
                    case Gruppe2_TrainRunObstacleKind.Bush:
                        HitUnless(airborne || onRoof, true, "Busch! Weiche rechtzeitig aus oder springe darueber.");
                        break;
                    case Gruppe2_TrainRunObstacleKind.PlantPot:
                        HitUnless(airborne || onRoof, true, "Pflanzkuebel! Weiche aus oder springe darueber.");
                        break;
                    case Gruppe2_TrainRunObstacleKind.Curb:
                        HitUnless(airborne || onRoof, false, "Hindernis gestreift - der Bahnbaer kommt naeher!");
                        break;
                }

                if (!trainPart)
                {
                    _field.MutableObstacles.Remove(obstacle);
                }

                if (_field.GameOver)
                {
                    break;
                }
            }
        }

        private void ChangeLane(int direction)
        {
            int previousLane = _field.Lane;
            int targetLane = Math.Max(0, Math.Min(2, previousLane + direction));
            if (targetLane == previousLane)
            {
                return;
            }

            if (_field.Level != Gruppe2_TrainRunLevel.TrainRoof && !CanReachTrainRoof()
                && Gruppe2_TrainRunLayout.IsCarUnderRunner(_field.Obstacles, targetLane))
            {
                // Running sideways into a train is fatal (the surfboard still saves the runner).
                HitOrEnd(true, "Seitlich gegen den Zug gelaufen!");
                return;
            }

            _field.Lane = targetLane;
            ResolveSignalLaneChange(previousLane, targetLane);
        }

        private void ResolveSignalLaneChange(int previousLane, int currentLane)
        {
            if (previousLane == currentLane || _field.JetpackTicks > 0)
            {
                // Flying with the jetpack passes high above the signals.
                return;
            }

            int crossedBoundary = Math.Min(previousLane, currentLane);
            Gruppe2_TrainRunObstacle signal = _field.MutableObstacles
                .Where(obstacle => obstacle.Kind == Gruppe2_TrainRunObstacleKind.Signal
                    && obstacle.Lane == crossedBoundary
                    && obstacle.Distance >= -SignalLaneChangeCollisionDistance
                    && obstacle.Distance <= SignalLaneChangeCollisionDistance)
                .OrderBy(obstacle => Math.Abs(obstacle.Distance))
                .FirstOrDefault();

            if (signal == null)
            {
                return;
            }

            _field.MutableObstacles.Remove(signal);
            HitOrEnd(false, "Ampel gestreift - der Bahnbaer kommt naeher!");
        }

        /// <summary>Hits the runner (fatal or not) unless the obstacle was avoided.</summary>
        private void HitUnless(bool avoided, bool fatal, string message)
        {
            if (!avoided)
            {
                HitOrEnd(fatal, message);
            }
        }

        private void HitOrEnd(bool fatal, string message)
        {
            if (_field.HasUpgrade(Gruppe2_TrainRunUpgradeKind.Surfboard))
            {
                _field.MutableUpgrades.Remove(Gruppe2_TrainRunUpgradeKind.Surfboard);
                SetMessage("Surfboard hat dich gerettet!");
                return;
            }

            if (fatal)
            {
                EndRun(message);
                return;
            }

            _field.GuardGap = Math.Max(0, _field.GuardGap - 24);
            SetMessage(message);
        }

        private void EndRun(string message)
        {
            _field.GameOver = true;
            SetMessage(message + " Spiel vorbei!");
        }

        private void SetMessage(string message)
        {
            _field.Message = message;
            _field.MessageTicks = 65;
        }

        private static string UpgradeName(Gruppe2_TrainRunUpgradeKind upgrade)
        {
            switch (upgrade)
            {
                case Gruppe2_TrainRunUpgradeKind.Magnet: return "Magnet";
                case Gruppe2_TrainRunUpgradeKind.DoubleCoins: return "doppelte Muenzen";
                case Gruppe2_TrainRunUpgradeKind.Surfboard: return "Surfboard-Schutz";
                case Gruppe2_TrainRunUpgradeKind.JumpStick: return "Huepfstab";
                case Gruppe2_TrainRunUpgradeKind.Jetpack: return "Jetpack";
                default: return "Geschwindigkeitsboost";
            }
        }
    }
}
