using System;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class Gruppe3DragRaceRules : IGruppe3DragRaceRules
    {
        private const double RollOutDeceleration = 0.1;
        private const double ComputerFalseStartChance = 0.1;
        private const double ComputerShiftVariation = 0.05;
        private readonly Gruppe3DragRaceField _field = new Gruppe3DragRaceField();
        private bool _playerOneAccelerating;
        private bool _playerOneBraking;
        private bool _playerTwoAccelerating;
        private bool _playerTwoBraking;
        private double _playerOneSpeed;
        private double _playerTwoSpeed;
        private bool _playerOneShiftUpPressed;
        private bool _playerOneShiftDownPressed;
        private bool _playerTwoShiftUpPressed;
        private bool _playerTwoShiftDownPressed;
        private DateTime _playerOneDownshiftUntil;
        private DateTime _playerTwoDownshiftUntil;
        private double _playerOneDownshiftLimit;
        private double _playerTwoDownshiftLimit;
        private DateTime _countdownStarted;
        private bool _playerOneIsComputer;
        private bool _playerTwoIsComputer;
        private bool _playerOneComputerFalseStart;
        private bool _playerTwoComputerFalseStart;
        private DateTime _playerOneFalseStartAt;
        private DateTime _playerTwoFalseStartAt;
        private DateTime _playerOneComputerStartAt;
        private DateTime _playerTwoComputerStartAt;
        private double _playerOneComputerShiftRpm;
        private double _playerTwoComputerShiftRpm;
        private bool _playerOneFinished;
        private bool _playerTwoFinished;
        private DateTime _raceStartedAt;
        private readonly Random _random = new Random();

        public string Name { get { return "Gruppe 3 Retro Drag Race Regeln"; } }
        public IGameField CurrentField { get { return _field; } }
        public IGruppe3DragRaceField DragRaceField { get { return _field; } }
        public bool MovesPossible
        {
            get
            {
                bool selectionInProgress = !_field.CarSelectionComplete &&
                    !(_field.PlayerOneCar >= 0 && _field.PlayerTwoCar >= 0);
                bool humanCountdownInProgress =
                    _field.CarSelectionComplete &&
                    !_field.IsRaceStarted &&
                    HasHumanPlayer();
                return _field.Winner == 0 &&
                    (selectionInProgress ||
                     _field.IsRaceStarted ||
                     humanCountdownInProgress);
            }
        }

        public void StartedGameCall()
        {
            ClearField();
            _countdownStarted = DateTime.UtcNow;
            _field.CountdownText = "PLAYER 1: AUTO AUSWAEHLEN";
        }

        public void ClearField()
        {
            _field.PlayerOneDistance = 0;
            _field.PlayerTwoDistance = 0;
            _field.IsRaceStarted = false;
            _field.CountdownText = "PLAYER 1: AUTO AUSWAEHLEN";
            _field.Winner = 0;
            _field.PlayerOneCar = -1;
            _field.PlayerTwoCar = -1;
            _field.CarSelectionComplete = false;
            _field.SelectionFeedbackText = null;
            _field.SelectionFeedbackUntil = DateTime.MinValue;
            _playerOneAccelerating = false;
            _playerOneBraking = false;
            _playerTwoAccelerating = false;
            _playerTwoBraking = false;
            _playerOneSpeed = 0;
            _playerTwoSpeed = 0;
            _field.PlayerOneSpeed = 0;
            _field.PlayerTwoSpeed = 0;
            _field.PlayerOneRpm = 0;
            _field.PlayerTwoRpm = 0;
            _field.PlayerOneGear = 0;
            _field.PlayerTwoGear = 0;
            _playerOneShiftUpPressed = false;
            _playerOneShiftDownPressed = false;
            _playerTwoShiftUpPressed = false;
            _playerTwoShiftDownPressed = false;
            _playerOneDownshiftUntil = DateTime.MinValue;
            _playerTwoDownshiftUntil = DateTime.MinValue;
            _playerOneDownshiftLimit =
                Gruppe3DragRaceCars.Get(0).MaxSpeed;
            _playerTwoDownshiftLimit =
                Gruppe3DragRaceCars.Get(0).MaxSpeed;
            _playerOneIsComputer = false;
            _playerTwoIsComputer = false;
            _playerOneComputerFalseStart = false;
            _playerTwoComputerFalseStart = false;
            _playerOneFalseStartAt = DateTime.MaxValue;
            _playerTwoFalseStartAt = DateTime.MaxValue;
            _playerOneComputerStartAt = DateTime.MaxValue;
            _playerTwoComputerStartAt = DateTime.MaxValue;
            _playerOneComputerShiftRpm = 0;
            _playerTwoComputerShiftRpm = 0;
            _playerOneFinished = false;
            _playerTwoFinished = false;
            _raceStartedAt = DateTime.MinValue;
            _field.PlayerOneRaceTimeSeconds = null;
            _field.PlayerTwoRaceTimeSeconds = null;
            _field.PlayerOneFinishSpeedKmh = null;
            _field.PlayerTwoFinishSpeedKmh = null;
            _field.PlayerOneReactionTimeMs = null;
            _field.PlayerTwoReactionTimeMs = null;
        }

        public void DoMove(IPlayMove move)
        {
            Gruppe3DragRaceMove raceMove = move as Gruppe3DragRaceMove;
            if (raceMove == null || raceMove.PlayerNumber < 1 || raceMove.PlayerNumber > 2)
            {
                return;
            }

            if (!_field.CarSelectionComplete)
            {
                if (raceMove.CarIndex < 0 ||
                    raceMove.CarIndex >= Gruppe3DragRaceCars.All.Count)
                {
                    return;
                }

                if (raceMove.PlayerNumber == 1 && _field.PlayerOneCar < 0)
                {
                    _field.PlayerOneCar = raceMove.CarIndex;
                    SetSelectionFeedback(1, raceMove.CarIndex);
                    _field.CountdownText = "PLAYER 2: AUTO AUSWAEHLEN";
                }
                else if (raceMove.PlayerNumber == 2 && _field.PlayerTwoCar < 0)
                {
                    _field.PlayerTwoCar = raceMove.CarIndex;
                    SetSelectionFeedback(2, raceMove.CarIndex);
                }

                return;
            }

            if (raceMove.PlayerNumber == 1)
            {
                _playerOneAccelerating = raceMove.Accelerate;
                _playerOneBraking = raceMove.Brake;
            }
            else
            {
                _playerTwoAccelerating = raceMove.Accelerate;
                _playerTwoBraking = raceMove.Brake;
            }
        }

        public void TickGameCall()
        {
            if (_field.Winner != 0)
            {
                RollOutAfterFinish();
                return;
            }

            if (!_field.CarSelectionComplete)
            {
                if (_field.PlayerOneCar >= 0 &&
                    _field.PlayerTwoCar >= 0)
                {
                    if (DateTime.UtcNow >= _field.SelectionFeedbackUntil)
                    {
                        _field.CarSelectionComplete = true;
                        _countdownStarted = DateTime.UtcNow;
                        ConfigureComputerBehavior();
                        _field.CountdownText = "3";
                    }

                    return;
                }

                return;
            }

            if (!_field.IsRaceStarted)
            {
                DateTime now = DateTime.UtcNow;
                ApplyComputerCountdownControls(now);
                ApplyKeyboardControls();
                UpdateCountdownRpm();
                double elapsed = (now - _countdownStarted).TotalSeconds;
                if (elapsed < 3 && HandleFalseStart())
                {
                    return;
                }

                if (elapsed >= 3)
                {
                    _field.IsRaceStarted = true;
                    _field.PlayerOneGear = _playerOneIsComputer
                        ? 0
                        : Math.Max(1, _field.PlayerOneGear);
                    _field.PlayerTwoGear = _playerTwoIsComputer
                        ? 0
                        : Math.Max(1, _field.PlayerTwoGear);
                    _raceStartedAt = DateTime.UtcNow;
                    SetComputerReactionTimes(_raceStartedAt);
                    RecordImmediateHumanReactions();
                    _field.CountdownText = "GO!";
                }
                else
                {
                    _field.CountdownText = Math.Ceiling(3 - elapsed).ToString();
                }

                return;
            }

            ApplyComputerControls();
            ApplyKeyboardControls();
            DateTime raceNow = DateTime.UtcNow;
            RecordHumanReactions(raceNow);
            if (_playerOneFinished)
            {
                _field.PlayerOneDistance = RollOutPlayer(
                    _field.PlayerOneDistance, ref _playerOneSpeed);
            }
            else
            {
                ApplyDownshiftLimit(
                    ref _playerOneSpeed,
                    _playerOneDownshiftLimit,
                    _playerOneDownshiftUntil);
                _field.PlayerOneDistance = Advance(
                    _field.PlayerOneDistance,
                    ref _playerOneSpeed,
                    _playerOneAccelerating,
                    _playerOneBraking,
                    _field.PlayerOneGear,
                    _field.PlayerOneCar);
                if (_field.PlayerOneDistance >=
                    Gruppe3DragRaceField.FinishDistance)
                {
                    RecordPlayerFinish(1, raceNow);
                }
            }

            if (_playerTwoFinished)
            {
                _field.PlayerTwoDistance = RollOutPlayer(
                    _field.PlayerTwoDistance, ref _playerTwoSpeed);
            }
            else
            {
                ApplyDownshiftLimit(
                    ref _playerTwoSpeed,
                    _playerTwoDownshiftLimit,
                    _playerTwoDownshiftUntil);
                _field.PlayerTwoDistance = Advance(
                    _field.PlayerTwoDistance,
                    ref _playerTwoSpeed,
                    _playerTwoAccelerating,
                    _playerTwoBraking,
                    _field.PlayerTwoGear,
                    _field.PlayerTwoCar);
                if (_field.PlayerTwoDistance >=
                    Gruppe3DragRaceField.FinishDistance)
                {
                    RecordPlayerFinish(2, raceNow);
                }
            }

            _field.PlayerOneSpeed = _playerOneSpeed;
            _field.PlayerTwoSpeed = _playerTwoSpeed;
            _field.PlayerOneRpm = _field.PlayerOneGear == 0
                ? UpdateNeutralRpm(
                    _field.PlayerOneRpm,
                    Gruppe3DragRaceCars.Get(_field.PlayerOneCar),
                    _playerOneAccelerating)
                : GetCurrentRpm(
                    _playerOneSpeed,
                    _field.PlayerOneGear,
                    _field.PlayerOneCar,
                    _playerOneDownshiftUntil);
            _field.PlayerTwoRpm = _field.PlayerTwoGear == 0
                ? UpdateNeutralRpm(
                    _field.PlayerTwoRpm,
                    Gruppe3DragRaceCars.Get(_field.PlayerTwoCar),
                    _playerTwoAccelerating)
                : GetCurrentRpm(
                    _playerTwoSpeed,
                    _field.PlayerTwoGear,
                    _field.PlayerTwoCar,
                    _playerTwoDownshiftUntil);

            if (_playerOneFinished && _playerTwoFinished)
            {
                double playerOneTime =
                    _field.PlayerOneRaceTimeSeconds.Value;
                double playerTwoTime =
                    _field.PlayerTwoRaceTimeSeconds.Value;
                if (playerOneTime == playerTwoTime)
                {
                    _field.Winner = -1;
                    _field.CountdownText = "UNENTSCHIEDEN!";
                }
                else
                {
                    _field.Winner = playerOneTime < playerTwoTime ? 1 : 2;
                    _field.CountdownText = "PLAYER " + _field.Winner + " WINS!";
                }
            }
        }

        public int CheckIfPLayerWon()
        {
            return _field.Winner;
        }

        private static double Advance(
            double distance,
            ref double speed,
            bool accelerating,
            bool braking,
            int gear,
            int carIndex)
        {
            Cars car = Gruppe3DragRaceCars.Get(carIndex);
            if (braking)
            {
                speed = Math.Max(0, speed - 0.45);
            }
            else if (accelerating)
            {
                if (gear <= 0)
                {
                    return Math.Max(
                        0,
                        distance + speed *
                            Gruppe3DragRaceField.TrackDistanceScale);
                }

                double rpm = car.GetRpm(speed, gear);
                bool atRedline = rpm >= car.OverRevMaxRpm;
                if (!atRedline)
                {
                    speed = Math.Min(
                        car.GetGearOverRevSpeedLimit(gear),
                        speed +
                            car.AccelerationByGear[car.ClampGear(gear) - 1] *
                            car.AccelerationScale *
                            car.GetPowerFactor(rpm));
                }
            }
            else
            {
                speed = Math.Max(0, speed - 0.05);
            }

            return Math.Max(
                0,
                distance + speed * Gruppe3DragRaceField.TrackDistanceScale);
        }

        private void UpdateCountdownRpm()
        {
            Cars playerOneCar =
                Gruppe3DragRaceCars.Get(_field.PlayerOneCar);
            Cars playerTwoCar =
                Gruppe3DragRaceCars.Get(_field.PlayerTwoCar);

            _field.PlayerOneRpm = _field.PlayerOneGear == 0
                ? UpdateNeutralRpm(
                    _field.PlayerOneRpm,
                    playerOneCar,
                    _playerOneAccelerating)
                : playerOneCar.GetRpm(
                    _playerOneSpeed, _field.PlayerOneGear);
            _field.PlayerTwoRpm = _field.PlayerTwoGear == 0
                ? UpdateNeutralRpm(
                    _field.PlayerTwoRpm,
                    playerTwoCar,
                    _playerTwoAccelerating)
                : playerTwoCar.GetRpm(
                    _playerTwoSpeed, _field.PlayerTwoGear);
        }

        private static double UpdateNeutralRpm(
            double currentRpm,
            Cars car,
            bool accelerating)
        {
            double idleRpm = car.IdleRpm;
            double rpmChange = (car.MaxRpm - idleRpm) * 0.08;
            double rpm = Math.Max(idleRpm, currentRpm);
            return accelerating
                ? Math.Min(car.MaxRpm, rpm + rpmChange)
                : Math.Max(idleRpm, rpm - rpmChange);
        }

        private void ConfigureComputerBehavior()
        {
            _playerOneIsComputer = IsComputerPlayer(1);
            _playerTwoIsComputer = IsComputerPlayer(2);
            _playerOneComputerFalseStart =
                _playerOneIsComputer &&
                _random.NextDouble() < ComputerFalseStartChance;
            _playerTwoComputerFalseStart =
                _playerTwoIsComputer &&
                _random.NextDouble() < ComputerFalseStartChance;
            _playerOneFalseStartAt = GetFalseStartTime(
                _playerOneComputerFalseStart);
            _playerTwoFalseStartAt = GetFalseStartTime(
                _playerTwoComputerFalseStart);
            _playerOneComputerStartAt = DateTime.MaxValue;
            _playerTwoComputerStartAt = DateTime.MaxValue;
            _playerOneComputerShiftRpm =
                GetRandomShiftRpm(_field.PlayerOneCar);
            _playerTwoComputerShiftRpm =
                GetRandomShiftRpm(_field.PlayerTwoCar);
        }

        private DateTime GetFalseStartTime(bool willFalseStart)
        {
            return willFalseStart
                ? _countdownStarted.AddMilliseconds(_random.Next(700, 2700))
                : DateTime.MaxValue;
        }

        private void ApplyComputerCountdownControls(DateTime now)
        {
            if (_playerOneComputerFalseStart &&
                now >= _playerOneFalseStartAt)
            {
                _field.PlayerOneGear = 1;
                _playerOneAccelerating = true;
            }

            if (_playerTwoComputerFalseStart &&
                now >= _playerTwoFalseStartAt)
            {
                _field.PlayerTwoGear = 1;
                _playerTwoAccelerating = true;
            }
        }

        private void SetComputerReactionTimes(DateTime now)
        {
            _playerOneComputerStartAt = _playerOneIsComputer
                ? now.AddMilliseconds(_random.Next(100, 401))
                : DateTime.MinValue;
            _playerTwoComputerStartAt = _playerTwoIsComputer
                ? now.AddMilliseconds(_random.Next(100, 401))
                : DateTime.MinValue;
        }

        private double GetRandomShiftRpm(int carIndex)
        {
            Cars car = Gruppe3DragRaceCars.Get(carIndex);
            double variation =
                (car.MaxRpm - car.IdleRpm) * ComputerShiftVariation;
            return car.OptimalShiftRpm +
                (_random.NextDouble() * 2 - 1) * variation;
        }

        private static bool IsComputerPlayer(int playerNumber)
        {
            foreach (IGamePlayer player in OOPGamesManager.Singleton.ActivePlayers)
            {
                if (player.PlayerNumber == playerNumber)
                {
                    return player is IComputerGamePlayer;
                }
            }

            return false;
        }

        private bool HandleFalseStart()
        {
            bool playerOneFalseStart =
                _field.PlayerOneGear > 0 && _playerOneAccelerating;
            bool playerTwoFalseStart =
                _field.PlayerTwoGear > 0 && _playerTwoAccelerating;
            if (!playerOneFalseStart && !playerTwoFalseStart)
            {
                return false;
            }

            ApplyFalseStartMovement(playerOneFalseStart, playerTwoFalseStart);

            if (playerOneFalseStart && playerTwoFalseStart)
            {
                _field.Winner = -1;
                _field.CountdownText = "BEIDE FEHLSTART! UNENTSCHIEDEN!";
            }
            else
            {
                int falseStartingPlayer = playerOneFalseStart ? 1 : 2;
                _field.Winner = falseStartingPlayer == 1 ? 2 : 1;
                _field.CountdownText =
                    "PLAYER " + falseStartingPlayer +
                    " FEHLSTART! PLAYER " + _field.Winner + " GEWINNT!";
            }

            DateTime now = DateTime.UtcNow;
            double falseStartReactionMs =
                (now - _countdownStarted.AddSeconds(3)).TotalMilliseconds;
            if (playerOneFalseStart)
            {
                _field.PlayerOneReactionTimeMs = falseStartReactionMs;
            }
            if (playerTwoFalseStart)
            {
                _field.PlayerTwoReactionTimeMs = falseStartReactionMs;
            }
            _field.PlayerOneFinishSpeedKmh =
                GetSpeedKmh(_playerOneSpeed, _field.PlayerOneCar);
            _field.PlayerTwoFinishSpeedKmh =
                GetSpeedKmh(_playerTwoSpeed, _field.PlayerTwoCar);

            return true;
        }

        private void ApplyFalseStartMovement(
            bool playerOneFalseStart,
            bool playerTwoFalseStart)
        {
            if (playerOneFalseStart)
            {
                _field.PlayerOneDistance = Advance(
                    _field.PlayerOneDistance,
                    ref _playerOneSpeed,
                    true,
                    false,
                    _field.PlayerOneGear,
                    _field.PlayerOneCar);
            }
            if (playerTwoFalseStart)
            {
                _field.PlayerTwoDistance = Advance(
                    _field.PlayerTwoDistance,
                    ref _playerTwoSpeed,
                    true,
                    false,
                    _field.PlayerTwoGear,
                    _field.PlayerTwoCar);
            }

            _field.PlayerOneSpeed = _playerOneSpeed;
            _field.PlayerTwoSpeed = _playerTwoSpeed;
            _field.PlayerOneRpm = GetCurrentRpm(
                _playerOneSpeed,
                _field.PlayerOneGear,
                _field.PlayerOneCar,
                DateTime.MinValue);
            _field.PlayerTwoRpm = GetCurrentRpm(
                _playerTwoSpeed,
                _field.PlayerTwoGear,
                _field.PlayerTwoCar,
                DateTime.MinValue);
        }

        private void RollOutAfterFinish()
        {
            _field.PlayerOneDistance = RollOutPlayer(
                _field.PlayerOneDistance, ref _playerOneSpeed);
            _field.PlayerTwoDistance = RollOutPlayer(
                _field.PlayerTwoDistance, ref _playerTwoSpeed);
            _field.PlayerOneSpeed = _playerOneSpeed;
            _field.PlayerTwoSpeed = _playerTwoSpeed;
            _field.PlayerOneRpm = GetCurrentRpm(
                _playerOneSpeed,
                _field.PlayerOneGear,
                _field.PlayerOneCar,
                DateTime.MinValue);
            _field.PlayerTwoRpm = GetCurrentRpm(
                _playerTwoSpeed,
                _field.PlayerTwoGear,
                _field.PlayerTwoCar,
                DateTime.MinValue);
        }

        private static double RollOutPlayer(
            double distance, ref double speed)
        {
            speed = Math.Max(0, speed - RollOutDeceleration);
            return distance +
                speed * Gruppe3DragRaceField.TrackDistanceScale;
        }

        private void RecordPlayerFinish(int playerNumber, DateTime now)
        {
            if (playerNumber == 1)
            {
                _playerOneFinished = true;
                _field.PlayerOneRaceTimeSeconds =
                Math.Max(0, (now - _raceStartedAt).TotalSeconds);
                _field.PlayerOneFinishSpeedKmh =
                GetSpeedKmh(_playerOneSpeed, _field.PlayerOneCar);
            }
            else
            {
                _playerTwoFinished = true;
                _field.PlayerTwoRaceTimeSeconds =
                Math.Max(0, (now - _raceStartedAt).TotalSeconds);
                _field.PlayerTwoFinishSpeedKmh =
                GetSpeedKmh(_playerTwoSpeed, _field.PlayerTwoCar);
            }
        }

        private static double GetSpeedKmh(double speed, int carIndex)
        {
            Cars car = Gruppe3DragRaceCars.Get(carIndex);
            return car.MaxSpeed <= 0
                ? 0
                : Math.Max(0, speed) / car.MaxSpeed * 300;
        }

        private void RecordImmediateHumanReactions()
        {
            if (IsHumanPlayer(1) && _playerOneAccelerating)
            {
                _field.PlayerOneReactionTimeMs = 0;
            }
            if (IsHumanPlayer(2) && _playerTwoAccelerating)
            {
                _field.PlayerTwoReactionTimeMs = 0;
            }
        }

        private void RecordHumanReactions(DateTime now)
        {
            if (IsHumanPlayer(1) && _playerOneAccelerating &&
                !_field.PlayerOneReactionTimeMs.HasValue)
            {
                _field.PlayerOneReactionTimeMs =
                (now - _raceStartedAt).TotalMilliseconds;
            }
            if (IsHumanPlayer(2) && _playerTwoAccelerating &&
                !_field.PlayerTwoReactionTimeMs.HasValue)
            {
                _field.PlayerTwoReactionTimeMs =
                (now - _raceStartedAt).TotalMilliseconds;
            }
        }

        private static bool IsHumanPlayer(int playerNumber)
        {
            foreach (IGamePlayer player in OOPGamesManager.Singleton.ActivePlayers)
            {
                if (player.PlayerNumber == playerNumber)
                {
                return player is IHumanGamePlayer;
                }
            }

            return false;
        }

        private static bool HasHumanPlayer()
        {
            foreach (IGamePlayer player in OOPGamesManager.Singleton.ActivePlayers)
            {
                if (player is IHumanGamePlayer)
                {
                    return true;
                }
            }

            return false;
        }

        private static double GetCurrentRpm(
            double speed,
            int gear,
            int carIndex,
            DateTime downshiftUntil)
        {
            Cars car = Gruppe3DragRaceCars.Get(carIndex);
            if (DateTime.UtcNow < downshiftUntil)
            {
                double normalRpm = car.GetRpm(speed, gear);
                return Math.Min(
                    car.MaxRpm,
                    normalRpm + car.GetGearRpmDrop(gear));
            }

            return car.GetRpm(speed, gear);
        }

        private static void ApplyDownshiftLimit(
            ref double speed,
            double limit,
            DateTime downshiftUntil)
        {
            if (DateTime.UtcNow >= downshiftUntil)
            {
                return;
            }

            if (speed > limit)
            {
                speed = Math.Max(limit, speed - 0.9);
            }
        }

        private static double GetGearMaximum(int carIndex, int gear)
        {
            return Gruppe3DragRaceCars.Get(carIndex).GetGearMaximum(gear);
        }

        private void SetSelectionFeedback(int playerNumber, int carIndex)
        {
            _field.SelectionFeedbackText =
                "PLAYER " + playerNumber + ": AUTO " + (carIndex + 1) +
                " GEWAEHLT!";
            _field.SelectionFeedbackUntil = DateTime.UtcNow.AddSeconds(1.2);
        }

        private void ApplyComputerControls()
        {
            DateTime now = DateTime.UtcNow;
            foreach (IGamePlayer player in OOPGamesManager.Singleton.ActivePlayers)
            {
                if (!(player is IComputerGamePlayer))
                {
                    continue;
                }

                if (player.PlayerNumber == 1)
                {
                    if (_playerOneFinished)
                    {
                        continue;
                    }

                    if (now < _playerOneComputerStartAt)
                    {
                        _field.PlayerOneGear = 0;
                        _playerOneAccelerating = true;
                        _playerOneBraking = false;
                        continue;
                    }

                    if (_field.PlayerOneGear == 0)
                    {
                        _field.PlayerOneGear = 1;
                        _field.PlayerOneReactionTimeMs =
                            (now - _raceStartedAt).TotalMilliseconds;
                    }

                    _playerOneAccelerating = true;
                    _playerOneBraking = false;
                    Cars car = Gruppe3DragRaceCars.Get(_field.PlayerOneCar);
                    if (_field.PlayerOneRpm >= _playerOneComputerShiftRpm &&
                        _field.PlayerOneGear < car.GearEnds.Count)
                    {
                        _field.PlayerOneGear++;
                        _playerOneComputerShiftRpm =
                            GetRandomShiftRpm(_field.PlayerOneCar);
                    }
                }
                else if (player.PlayerNumber == 2)
                {
                    if (_playerTwoFinished)
                    {
                        continue;
                    }

                    if (now < _playerTwoComputerStartAt)
                    {
                        _field.PlayerTwoGear = 0;
                        _playerTwoAccelerating = true;
                        _playerTwoBraking = false;
                        continue;
                    }

                    if (_field.PlayerTwoGear == 0)
                    {
                        _field.PlayerTwoGear = 1;
                        _field.PlayerTwoReactionTimeMs =
                            (now - _raceStartedAt).TotalMilliseconds;
                    }

                    _playerTwoAccelerating = true;
                    _playerTwoBraking = false;
                    Cars car = Gruppe3DragRaceCars.Get(_field.PlayerTwoCar);
                    if (_field.PlayerTwoRpm >= _playerTwoComputerShiftRpm &&
                        _field.PlayerTwoGear < car.GearEnds.Count)
                    {
                        _field.PlayerTwoGear++;
                        _playerTwoComputerShiftRpm =
                            GetRandomShiftRpm(_field.PlayerTwoCar);
                    }
                }
            }
        }

        private void ApplyKeyboardControls()
        {
            foreach (IGamePlayer player in OOPGamesManager.Singleton.ActivePlayers)
            {
                if (!(player is IHumanGamePlayer))
                {
                    continue;
                }

                if (player.PlayerNumber == 1)
                {
                    _playerOneAccelerating =
                        Keyboard.IsKeyDown(Key.W);
                    _playerOneBraking =
                        Keyboard.IsKeyDown(Key.S);
                    bool shiftUpPressed =
                        Keyboard.IsKeyDown(Key.Q);
                    bool shiftDownPressed =
                        Keyboard.IsKeyDown(Key.E);
                    if (shiftUpPressed && !_playerOneShiftUpPressed)
                    {
                        _field.PlayerOneGear =
                            Math.Min(
                                Gruppe3DragRaceCars.Get(
                                    _field.PlayerOneCar).GearEnds.Count,
                                _field.PlayerOneGear + 1);
                    }
                    if (shiftDownPressed && !_playerOneShiftDownPressed &&
                        _field.PlayerOneGear > 1)
                    {
                        int newGear = Math.Max(1, _field.PlayerOneGear - 1);
                        if (newGear != _field.PlayerOneGear)
                        {
                            _field.PlayerOneGear = newGear;
                            _playerOneDownshiftLimit =
                                GetGearMaximum(_field.PlayerOneCar, newGear);
                            _playerOneDownshiftUntil =
                                DateTime.UtcNow.AddMilliseconds(650);
                        }
                    }
                    _playerOneShiftUpPressed = shiftUpPressed;
                    _playerOneShiftDownPressed = shiftDownPressed;
                }
                else if (player.PlayerNumber == 2)
                {
                    _playerTwoAccelerating =
                        Keyboard.IsKeyDown(Key.NumPad8) ||
                        Keyboard.IsKeyDown(Key.Up);
                    _playerTwoBraking =
                        Keyboard.IsKeyDown(Key.NumPad5) ||
                        Keyboard.IsKeyDown(Key.Down);
                    bool shiftUpPressed =
                        Keyboard.IsKeyDown(Key.NumPad7) ||
                        Keyboard.IsKeyDown(Key.PageUp);
                    bool shiftDownPressed =
                        Keyboard.IsKeyDown(Key.NumPad9) ||
                        Keyboard.IsKeyDown(Key.PageDown);
                    if (shiftUpPressed && !_playerTwoShiftUpPressed)
                    {
                        _field.PlayerTwoGear =
                            Math.Min(
                                Gruppe3DragRaceCars.Get(
                                    _field.PlayerTwoCar).GearEnds.Count,
                                _field.PlayerTwoGear + 1);
                    }
                    if (shiftDownPressed && !_playerTwoShiftDownPressed &&
                        _field.PlayerTwoGear > 1)
                    {
                        int newGear = Math.Max(1, _field.PlayerTwoGear - 1);
                        if (newGear != _field.PlayerTwoGear)
                        {
                            _field.PlayerTwoGear = newGear;
                            _playerTwoDownshiftLimit =
                                GetGearMaximum(_field.PlayerTwoCar, newGear);
                            _playerTwoDownshiftUntil =
                                DateTime.UtcNow.AddMilliseconds(650);
                        }
                    }
                    _playerTwoShiftUpPressed = shiftUpPressed;
                    _playerTwoShiftDownPressed = shiftDownPressed;
                }
            }
        }
    }
}
