using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class Gruppe2_SubwayRules : IGameRules2
    {
        private readonly Random _random = new Random();
        private Gruppe2_SubwayField _field = new Gruppe2_SubwayField();
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
                    _field.Lane = Math.Max(0, _field.Lane - 1);
                    break;
                case Key.Right:
                case Key.D:
                    _field.Lane = Math.Min(2, _field.Lane + 1);
                    break;
                case Key.Up:
                case Key.W:
                    if (_field.Level == Gruppe2_SubwayLevel.Track)
                    {
                        _field.JumpDurationTicks = _field.HasUpgrade(Gruppe2_SubwayUpgradeKind.JumpStick) ? 42 : 30;
                        _field.JumpTicks = _field.JumpDurationTicks;
                        _field.RollTicks = 0;
                        _field.Level = Gruppe2_SubwayLevel.Air;
                    }
                    break;
                case Key.Down:
                case Key.S:
                    _field.RollTicks = 24;
                    if (_field.Level == Gruppe2_SubwayLevel.Air)
                    {
                        _field.Level = Gruppe2_SubwayLevel.Track;
                        _field.JumpTicks = 0;
                        _field.JumpDurationTicks = 0;
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

            _field.Distance += _field.Speed;
            _field.Score = (int)_field.Distance + _field.Coins * 10;
            _field.Speed = Math.Min(2.1, 1 + _field.Score / 1500.0);

            TickTimers();
            SpawnWaves();
            AdvanceObjects();
            CollectRewards();
            ResolveObstacles();

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

            if (_field.JumpTicks > 0)
            {
                _field.JumpTicks--;
                if (_field.JumpTicks == 0 && _field.JetpackTicks == 0
                    && _field.Level == Gruppe2_SubwayLevel.Air)
                {
                    _field.Level = Gruppe2_SubwayLevel.Track;
                }
            }

            if (_field.RollTicks > 0)
            {
                _field.RollTicks--;
            }

            if (_field.RoofTicks > 0)
            {
                _field.RoofTicks--;
                if (_field.RoofTicks == 0 && _field.Level == Gruppe2_SubwayLevel.TrainRoof)
                {
                    _field.Level = Gruppe2_SubwayLevel.Track;
                    SetMessage("Das Zugdach endet - zurueck auf die Gleise!");
                }
            }

            if (_field.JetpackTicks > 0)
            {
                _field.JetpackTicks--;
                if (_field.JetpackTicks == 0 && _field.JumpTicks == 0
                    && _field.Level == Gruppe2_SubwayLevel.Air)
                {
                    _field.Level = Gruppe2_SubwayLevel.Track;
                }
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
            while (_field.Distance >= _nextWaveAt)
            {
                int safeLane = _random.Next(0, 3);
                List<int> blockedLanes = Enumerable.Range(0, 3)
                    .Where(lane => lane != safeLane)
                    .OrderBy(_ => _random.Next())
                    .Take(_random.Next(1, 3))
                    .ToList();

                foreach (int lane in blockedLanes)
                {
                    Gruppe2_SubwayObstacleKind kind = (Gruppe2_SubwayObstacleKind)_random.Next(0, 9);
                    if (kind == Gruppe2_SubwayObstacleKind.Train)
                    {
                        _field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                            Gruppe2_SubwayObstacleKind.CargoRamp, lane, 112));
                        _field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                            kind, lane, 82, _random.Next(0, 2) == 0));
                    }
                    else
                    {
                        _field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(kind, lane, 82));
                    }
                }

                if (_random.Next(0, 4) == 0)
                {
                    _field.MutableRewards.Add(new Gruppe2_SubwayReward(
                        Gruppe2_SubwayRewardKind.UpgradeBox, safeLane, 82,
                        (Gruppe2_SubwayUpgradeKind)_random.Next(0, 5)));
                }
                else
                {
                    int coinCount = _random.Next(3, 7);
                    for (int coin = 0; coin < coinCount; coin++)
                    {
                        _field.MutableRewards.Add(new Gruppe2_SubwayReward(
                            Gruppe2_SubwayRewardKind.Coin, safeLane, 82 + coin * 5));
                    }
                }

                _nextWaveAt += 74;
            }
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

            _field.MutableObstacles.RemoveAll(obstacle => obstacle.Distance < -10);
            _field.MutableRewards.RemoveAll(reward => reward.Distance < -4);
        }

        private void CollectRewards()
        {
            foreach (Gruppe2_SubwayReward reward in _field.MutableRewards.ToArray())
            {
                bool inRange = reward.Distance <= 1;
                bool attracted = reward.Kind == Gruppe2_SubwayRewardKind.Coin
                    && _field.HasUpgrade(Gruppe2_SubwayUpgradeKind.Magnet)
                    && Math.Abs(reward.Lane - _field.Lane) <= 1
                    && reward.Distance < 18;

                if (!inRange || (reward.Lane != _field.Lane && !attracted))
                {
                    continue;
                }

                if (reward.Kind == Gruppe2_SubwayRewardKind.Coin)
                {
                    _field.Coins += _field.HasUpgrade(Gruppe2_SubwayUpgradeKind.DoubleCoins) ? 2 : 1;
                    SetMessage("Muenze gesammelt!");
                }
                else
                {
                    GrantUpgrade(reward.Upgrade);
                }

                _field.MutableRewards.Remove(reward);
            }
        }

        private void GrantUpgrade(Gruppe2_SubwayUpgradeKind upgrade)
        {
            int duration = upgrade == Gruppe2_SubwayUpgradeKind.Surfboard ? 500 : 220;
            _field.MutableUpgrades[upgrade] = duration;
            switch (upgrade)
            {
                case Gruppe2_SubwayUpgradeKind.Jetpack:
                    _field.JetpackTicks = duration;
                    _field.Level = Gruppe2_SubwayLevel.Air;
                    break;
                case Gruppe2_SubwayUpgradeKind.JumpStick:
                    _field.JumpDurationTicks = 42;
                    _field.JumpTicks = Math.Max(_field.JumpTicks, _field.JumpDurationTicks);
                    _field.Level = Gruppe2_SubwayLevel.Air;
                    break;
            }

            SetMessage("Upgrade: " + UpgradeName(upgrade));
        }

        private void ResolveObstacles()
        {
            foreach (Gruppe2_SubwayObstacle obstacle in _field.MutableObstacles
                .Where(item => item.Distance <= 0.5 && item.Lane == _field.Lane)
                .ToArray())
            {
                bool airborne = _field.Level == Gruppe2_SubwayLevel.Air;
                bool onRoof = _field.Level == Gruppe2_SubwayLevel.TrainRoof;
                bool rolling = _field.RollTicks > 0;

                switch (obstacle.Kind)
                {
                    case Gruppe2_SubwayObstacleKind.Train:
                        if (!airborne && !onRoof)
                        {
                            HitOrEnd(true, "Zug! Weiche rechtzeitig aus oder springe ueber die Rampe aufs Dach.");
                        }
                        else if (onRoof)
                        {
                            _field.RoofTicks = 42;
                        }
                        break;
                    case Gruppe2_SubwayObstacleKind.CargoRamp:
                        if (_field.JumpTicks > 0)
                        {
                            _field.Level = Gruppe2_SubwayLevel.TrainRoof;
                            _field.RoofTicks = 120;
                            SetMessage("Auf dem Zugdach!");
                        }
                        else
                        {
                            HitOrEnd(false, "Rampe beruehrt - der Bahnbaer kommt naeher!");
                        }
                        break;
                    case Gruppe2_SubwayObstacleKind.LargeBarrier:
                        if (!rolling && !onRoof)
                        {
                            HitOrEnd(true, "Grosse Schranke! Nur ducken hilft.");
                        }
                        break;
                    case Gruppe2_SubwayObstacleKind.SmallBarrier:
                        if (!rolling && !airborne && !onRoof)
                        {
                            HitOrEnd(false, "Schranke gestreift - der Bahnbaer kommt naeher!");
                        }
                        break;
                    case Gruppe2_SubwayObstacleKind.Bush:
                    case Gruppe2_SubwayObstacleKind.PlantPot:
                    case Gruppe2_SubwayObstacleKind.Curb:
                        if (!airborne && !onRoof)
                        {
                            HitOrEnd(false, "Hindernis gestreift - der Bahnbaer kommt naeher!");
                        }
                        break;
                    case Gruppe2_SubwayObstacleKind.CenterPillar:
                    case Gruppe2_SubwayObstacleKind.Signal:
                        if (!airborne && !onRoof)
                        {
                            HitOrEnd(true, "Frontal gegen ein Hindernis!");
                        }
                        break;
                }

                _field.MutableObstacles.Remove(obstacle);
                if (_field.GameOver)
                {
                    break;
                }
            }
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
