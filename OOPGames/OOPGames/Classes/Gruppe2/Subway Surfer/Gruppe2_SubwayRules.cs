using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class Gruppe2_SubwayRules : IGameRules2
    {
        private const double SignalLaneChangeCollisionDistance = 8;
        private readonly Random _random = new Random();
        private static readonly Gruppe2_SubwayObstacleKind[] SpawnableObstacleKinds =
        {
            Gruppe2_SubwayObstacleKind.SmallBarrier,
            Gruppe2_SubwayObstacleKind.LargeBarrier,
            Gruppe2_SubwayObstacleKind.Bush,
            Gruppe2_SubwayObstacleKind.PlantPot,
            Gruppe2_SubwayObstacleKind.Train,
            Gruppe2_SubwayObstacleKind.CargoRamp,
            Gruppe2_SubwayObstacleKind.Curb,
            Gruppe2_SubwayObstacleKind.CargoWagon
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
        private const int NormalJumpTicks = 20;
        private const int JumpStickJumpTicks = 30;
        // Only one of these can be active at a time; magnet and double coins combine freely.
        private static readonly Gruppe2_SubwayUpgradeKind[] ExclusiveUpgrades =
        {
            Gruppe2_SubwayUpgradeKind.JumpStick,
            Gruppe2_SubwayUpgradeKind.Jetpack,
            Gruppe2_SubwayUpgradeKind.Surfboard
        };
        private Gruppe2_SubwayField _field = new Gruppe2_SubwayField();
        private readonly HashSet<Gruppe2_SubwayObstacle> _passedTrainParts =
            new HashSet<Gruppe2_SubwayObstacle>();
        private readonly HashSet<Gruppe2_SubwayReward> _missedRewards =
            new HashSet<Gruppe2_SubwayReward>();
        private double _nextWaveAt;

        public string Name { get { return "Gruppe 2 - Subway Surfer Regeln"; } }
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
            _trackPrefilled = false;
            _nextWaveAt = 25;
        }

        public void DoMove(IPlayMove move)
        {
            IGruppe2_SubwayMove subwayMove = move as IGruppe2_SubwayMove;
            if (subwayMove == null || !MovesPossible
                || subwayMove.PlayerNumber < 1 || subwayMove.PlayerNumber > 2)
            {
                return;
            }

            switch (subwayMove.Key)
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
                    bool onRoof = _field.Level == Gruppe2_SubwayLevel.TrainRoof;
                    if ((_field.Level == Gruppe2_SubwayLevel.Track || onRoof) && _field.JumpTicks == 0)
                    {
                        _field.JumpDurationTicks = _field.HasUpgrade(Gruppe2_SubwayUpgradeKind.JumpStick)
                            ? JumpStickJumpTicks
                            : NormalJumpTicks;
                        _field.JumpTicks = _field.JumpDurationTicks;
                        _field.RollTicks = 0;
                        // A jump on a roof stays a roof jump: the arc starts at roof height and
                        // can carry the runner over a gap onto the next train.
                        if (!onRoof)
                        {
                            _field.Level = Gruppe2_SubwayLevel.Air;
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
                    if (_field.Level == Gruppe2_SubwayLevel.TrainRoof)
                    {
                        // Cancels a roof jump; above a gap the runner then drops off.
                        _field.JumpTicks = 0;
                        _field.JumpDurationTicks = 0;
                    }
                    else if (_field.Level == Gruppe2_SubwayLevel.Air)
                    {
                        _field.JumpTicks = 0;
                        _field.JumpDurationTicks = 0;
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
            _field.Distance += _field.Speed;
            _field.Score = (int)_field.Distance + _field.Coins * 10;
            // The speed keeps rising with the score, without an upper limit.
            _field.Speed = 1 + _field.Score / 1500.0;

            TickTimers();
            SpawnWaves();
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
            bool sinking = _field.JetpackTicks > 0 && _field.JetpackTicks < Gruppe2_SubwayTrainLayout.JetpackClimbTicks;
            if (sinking
                && _field.JetpackHeight * Gruppe2_SubwayTrainLayout.JetpackAltitudeInRoofHeights <= 1
                && Gruppe2_SubwayTrainLayout.IsCarUnderRunner(_field.Obstacles, _field.Lane))
            {
                _field.JetpackTicks = 0;
                _field.MutableUpgrades.Remove(Gruppe2_SubwayUpgradeKind.Jetpack);
            }

            bool stillInAir = _field.JumpTicks > 0 || _field.DropTicks > 0 || _field.JetpackTicks > 0;
            if (_field.Level == Gruppe2_SubwayLevel.Air && !stillInAir)
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
                && _field.HasUpgrade(Gruppe2_SubwayUpgradeKind.JumpStick);
            return jumpingWithStick || _field.JetpackTicks > 0;
        }

        /// <summary>
        /// The front of a car reaches the runner: walk on (roof), fly over (jetpack),
        /// land on the roof (jump stick) or crash.
        /// </summary>
        private void ReachCarFront(string crashMessage)
        {
            if (_field.Level == Gruppe2_SubwayLevel.TrainRoof || _field.JetpackTicks > 0)
            {
                return;
            }

            if (!CanReachTrainRoof())
            {
                HitOrEnd(true, crashMessage);
                return;
            }

            _field.JumpTicks = 0;
            _field.JumpDurationTicks = 0;
            _field.Level = Gruppe2_SubwayLevel.TrainRoof;
            SetMessage("Mit dem Huepfstab aufs Zugdach!");
        }

        private void Land()
        {
            _field.Level = Gruppe2_SubwayTrainLayout.IsCarUnderRunner(_field.Obstacles, _field.Lane)
                ? Gruppe2_SubwayLevel.TrainRoof
                : Gruppe2_SubwayLevel.Track;
        }

        private void UpdateTrainRoof()
        {
            // While jumping on the roof the runner is in the air and does not fall into a gap;
            // only when the jump ends without a car below does the drop start.
            if (_field.Level == Gruppe2_SubwayLevel.TrainRoof
                && _field.JumpTicks == 0
                && !Gruppe2_SubwayTrainLayout.IsCarUnderRunner(_field.Obstacles, _field.Lane))
            {
                // Drop off in a small arc. The fall lasts until the car that was just left
                // is behind the camera, so the camera never ends up inside it.
                double dropDistance = -Gruppe2_SubwayTrainLayout.TrainRemoveBehindDistance + 2;
                _field.DropDurationTicks = Math.Max(MinimumDropTicks,
                    (int)Math.Ceiling(dropDistance / _field.Speed));
                _field.DropTicks = _field.DropDurationTicks;
                _field.Level = Gruppe2_SubwayLevel.Air;
                SetMessage("Das Zugdach endet - zurueck auf die Gleise!");
            }
        }

        private static void DecreaseTimers(Dictionary<Gruppe2_SubwayUpgradeKind, int> upgrades)
        {
            foreach (Gruppe2_SubwayUpgradeKind upgrade in upgrades.Keys.ToArray())
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
                .Where(lane => !Gruppe2_SubwayTrainLayout.HasTrainAhead(_field.Obstacles, lane))
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
                Gruppe2_SubwayObstacleKind kind =
                    SpawnableObstacleKinds[_random.Next(SpawnableObstacleKinds.Length)];
                switch (kind)
                {
                    case Gruppe2_SubwayObstacleKind.CargoRamp:
                        // Ramp, one to five cargo wagons and a locomotive: walkable, different lengths.
                        SpawnTrain(lane, distance, true, _random.Next(1, 6));
                        break;
                    case Gruppe2_SubwayObstacleKind.Train:
                        // Locomotive in front pulling one to four wagons: has to be dodged.
                        SpawnTrain(lane, distance, false, _random.Next(1, 5));
                        break;
                    default:
                        double length = Gruppe2_SubwayTrainLayout.IsCar(kind) ? Gruppe2_SubwayTrainLayout.CarSpacing : 0;
                        if (IsLaneFree(lane, distance, distance + length))
                        {
                            _field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(kind, lane, distance));
                        }
                        break;
                }
            }

            if (_random.Next(0, 2) == 0)
            {
                _field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                    Gruppe2_SubwayObstacleKind.Signal, _random.Next(0, 2), distance));
            }

            if (_random.Next(0, 4) == 0)
            {
                _field.MutableRewards.Add(new Gruppe2_SubwayReward(
                    Gruppe2_SubwayRewardKind.UpgradeBox, safeLane, distance,
                    (Gruppe2_SubwayUpgradeKind)_random.Next(0, 5)));
            }
            else
            {
                int coinCount = _random.Next(3, 7);
                for (int coin = 0; coin < coinCount; coin++)
                {
                    _field.MutableRewards.Add(new Gruppe2_SubwayReward(
                        Gruppe2_SubwayRewardKind.Coin, safeLane, distance + coin * 5));
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
            double spacing = Gruppe2_SubwayTrainLayout.CarSpacing;
            double start = withRamp ? front - Gruppe2_SubwayTrainLayout.RampLength : front;
            double end = front + (wagonCount + 1) * spacing;
            if (!IsLaneFree(lane, start, end))
            {
                return;
            }

            if (withRamp)
            {
                _field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                    Gruppe2_SubwayObstacleKind.CargoRamp, lane, front));
            }

            for (int car = 0; car <= wagonCount; car++)
            {
                bool locomotive = withRamp ? car == wagonCount : car == 0;
                _field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                    locomotive ? Gruppe2_SubwayObstacleKind.Train : Gruppe2_SubwayObstacleKind.CargoWagon,
                    lane, front + car * spacing));
            }
        }

        /// <summary>
        /// New objects always appear behind (further away than) the trains already on the
        /// track, so the gap behind the end of a train is the one that matters for reacting.
        /// </summary>
        private bool IsLaneFree(int lane, double from, double to)
        {
            double gapBehindTrain = Math.Max(TrainClearance, ReactionDistanceAfterTrain * _field.Speed);
            return !Gruppe2_SubwayTrainLayout.IsLaneOccupiedByTrain(
                _field.Obstacles, lane, from - gapBehindTrain, to + TrainClearance);
        }

        private void AdvanceObjects()
        {
            foreach (Gruppe2_SubwayObstacle obstacle in _field.MutableObstacles)
            {
                obstacle.Distance -= _field.Speed;
            }

            foreach (Gruppe2_SubwayReward reward in _field.MutableRewards)
            {
                reward.Distance -= _field.Speed;
            }

            _passedTrainParts.RemoveWhere(Gruppe2_SubwayTrainLayout.IsBehindRunner);
            _field.MutableObstacles.RemoveAll(Gruppe2_SubwayTrainLayout.IsBehindRunner);
            _field.MutableRewards.RemoveAll(reward => reward.Distance < -4);
            _missedRewards.RemoveWhere(reward => reward.Distance < -4);
        }

        private void CollectRewards()
        {
            foreach (Gruppe2_SubwayReward reward in _field.MutableRewards.ToArray())
            {
                bool inRange = reward.Distance <= 1;
                bool attracted = reward.Kind == Gruppe2_SubwayRewardKind.Coin
                    && _field.HasUpgrade(Gruppe2_SubwayUpgradeKind.Magnet)
                    && Math.Abs(reward.Lane - _field.Lane) <= 1
                    && reward.Distance < Gruppe2_SubwayTrainLayout.MagnetRange;

                // Coins at flight level can only be reached while flying high, ground coins
                // only below it.
                bool flyingHigh = _field.JetpackHeight > 0.5;
                if (!inRange || (reward.Lane != _field.Lane && !attracted) || reward.Elevated != flyingHigh)
                {
                    continue;
                }

                if (reward.Kind == Gruppe2_SubwayRewardKind.Coin)
                {
                    _field.Coins += _field.HasUpgrade(Gruppe2_SubwayUpgradeKind.DoubleCoins) ? 2 : 1;
                    SetMessage("Muenze gesammelt!");
                }
                else if (_missedRewards.Contains(reward) || IsBlockedByOtherExclusiveUpgrade(reward.Upgrade))
                {
                    // Jump stick, jetpack and surfboard exclude each other: the reward stays
                    // on the track and is missed - also if the active one runs out a moment later.
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

        private bool IsBlockedByOtherExclusiveUpgrade(Gruppe2_SubwayUpgradeKind upgrade)
        {
            return ExclusiveUpgrades.Contains(upgrade)
                && ExclusiveUpgrades.Any(active => active != upgrade && _field.HasUpgrade(active));
        }

        private void GrantUpgrade(Gruppe2_SubwayUpgradeKind upgrade)
        {
            int duration = upgrade == Gruppe2_SubwayUpgradeKind.Surfboard ? 500 : 220;
            _field.MutableUpgrades[upgrade] = duration;
            switch (upgrade)
            {
                case Gruppe2_SubwayUpgradeKind.Jetpack:
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
            int climb = Gruppe2_SubwayTrainLayout.JetpackClimbTicks;
            int currentClimb = _field.JetpackTicks > 0
                ? Math.Min(climb, Math.Min(_field.JetpackDurationTicks - _field.JetpackTicks, _field.JetpackTicks))
                : 0;
            _field.JetpackTicks = duration;
            _field.JetpackDurationTicks = duration + currentClimb;
            _field.JumpTicks = 0;
            _field.JumpDurationTicks = 0;
            _field.DropTicks = 0;
            _field.RollTicks = 0;
            _field.Level = Gruppe2_SubwayLevel.Air;
            SpawnJetpackCoins(duration - climb);
        }

        /// <summary>Coins at flight level, in short rows that switch to a neighbouring lane.</summary>
        private void SpawnJetpackCoins(int cruiseTicks)
        {
            const double CoinSpacing = 6;
            const int CoinsPerRow = 8;
            double start = Gruppe2_SubwayTrainLayout.JetpackClimbTicks * _field.Speed;
            double end = Math.Min(SpawnDistance, cruiseTicks * _field.Speed);
            int lane = _field.Lane;
            int coinInRow = 0;
            for (double distance = start; distance < end; distance += CoinSpacing)
            {
                _field.MutableRewards.Add(new Gruppe2_SubwayReward(
                    Gruppe2_SubwayRewardKind.Coin, lane, distance, elevated: true));
                if (++coinInRow == CoinsPerRow)
                {
                    coinInRow = 0;
                    lane = lane == 0 ? 1 : lane == 2 ? 1 : lane + (_random.Next(0, 2) == 0 ? -1 : 1);
                }
            }
        }

        private void ResolveObstacles()
        {
            foreach (Gruppe2_SubwayObstacle obstacle in _field.MutableObstacles
                .Where(item => item.Kind != Gruppe2_SubwayObstacleKind.Signal
                    && item.Distance <= Gruppe2_SubwayTrainLayout.CollisionDistance
                    && !_passedTrainParts.Contains(item))
                .ToArray())
            {
                bool trainPart = Gruppe2_SubwayTrainLayout.IsTrainPart(obstacle.Kind);
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

                bool airborne = _field.Level == Gruppe2_SubwayLevel.Air;
                bool onRoof = _field.Level == Gruppe2_SubwayLevel.TrainRoof;
                bool rolling = _field.RollTicks > 0;

                switch (obstacle.Kind)
                {
                    case Gruppe2_SubwayObstacleKind.Train:
                        ReachCarFront("Zug! Weiche aus oder nimm die Rampe - aufspringen nur mit Huepfstab.");
                        break;
                    case Gruppe2_SubwayObstacleKind.CargoRamp:
                        // The runner walks up the ramp automatically; only a jetpack flies over it.
                        if (_field.JetpackTicks == 0)
                        {
                            _field.JumpTicks = 0;
                            _field.JumpDurationTicks = 0;
                            _field.Level = Gruppe2_SubwayLevel.TrainRoof;
                            SetMessage("Auf dem Zugdach!");
                        }
                        break;
                    case Gruppe2_SubwayObstacleKind.SmallBarrier:
                        if (!airborne && !onRoof)
                        {
                            HitOrEnd(true, "Kleine Absperrung! Springe darueber.");
                        }
                        break;
                    case Gruppe2_SubwayObstacleKind.CargoWagon:
                        ReachCarFront("Gueterwagen! Nimm die Rampe - aufspringen nur mit Huepfstab.");
                        break;
                    case Gruppe2_SubwayObstacleKind.LargeBarrier:
                        if (!rolling && !onRoof)
                        {
                            HitOrEnd(true, "Grosse Schranke! Nur ducken hilft.");
                        }
                        break;
                    case Gruppe2_SubwayObstacleKind.Bush:
                        if (!airborne && !onRoof)
                        {
                            HitOrEnd(true, "Busch! Weiche rechtzeitig aus oder springe darueber.");
                        }
                        break;
                    case Gruppe2_SubwayObstacleKind.PlantPot:
                        if (!airborne && !onRoof)
                        {
                            HitOrEnd(true, "Pflanzkuebel! Weiche aus oder springe darueber.");
                        }
                        break;
                    case Gruppe2_SubwayObstacleKind.Curb:
                        if (!airborne && !onRoof)
                        {
                            HitOrEnd(false, "Hindernis gestreift - der Bahnbaer kommt naeher!");
                        }
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

            if (_field.Level != Gruppe2_SubwayLevel.TrainRoof && !CanReachTrainRoof()
                && Gruppe2_SubwayTrainLayout.IsCarUnderRunner(_field.Obstacles, targetLane))
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
            Gruppe2_SubwayObstacle signal = _field.MutableObstacles
                .Where(obstacle => obstacle.Kind == Gruppe2_SubwayObstacleKind.Signal
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

        private void HitOrEnd(bool fatal, string message)
        {
            if (_field.HasUpgrade(Gruppe2_SubwayUpgradeKind.Surfboard))
            {
                _field.MutableUpgrades.Remove(Gruppe2_SubwayUpgradeKind.Surfboard);
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

        private static string UpgradeName(Gruppe2_SubwayUpgradeKind upgrade)
        {
            switch (upgrade)
            {
                case Gruppe2_SubwayUpgradeKind.Magnet: return "Magnet";
                case Gruppe2_SubwayUpgradeKind.DoubleCoins: return "doppelte Muenzen";
                case Gruppe2_SubwayUpgradeKind.Surfboard: return "Surfboard-Schutz";
                case Gruppe2_SubwayUpgradeKind.JumpStick: return "Huepfstab";
                case Gruppe2_SubwayUpgradeKind.Jetpack: return "Jetpack";
                default: return "Geschwindigkeitsboost";
            }
        }
    }
}
