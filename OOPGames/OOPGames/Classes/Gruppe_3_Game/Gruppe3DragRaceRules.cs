using System;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class Gruppe3DragRaceRules : IGruppe3DragRaceRules
    {
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
                return _field.Winner == 0 &&
                    (selectionInProgress || _field.IsRaceStarted);
            }
        }

        public void StartedGameCall()
        {
            ClearField();
            _countdownStarted = DateTime.UtcNow;
            _field.CountdownText = "PLAYER 1: AUTO AUSWÄHLEN";
        }

        public void ClearField()
        {
            _field.PlayerOneDistance = 0;
            _field.PlayerTwoDistance = 0;
            _field.IsRaceStarted = false;
            _field.CountdownText = "PLAYER 1: AUTO AUSWÄHLEN";
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
            _field.PlayerOneGear = 1;
            _field.PlayerTwoGear = 1;
            _playerOneShiftUpPressed = false;
            _playerOneShiftDownPressed = false;
            _playerTwoShiftUpPressed = false;
            _playerTwoShiftDownPressed = false;
            _playerOneDownshiftUntil = DateTime.MinValue;
            _playerTwoDownshiftUntil = DateTime.MinValue;
            _playerOneDownshiftLimit = 16;
            _playerTwoDownshiftLimit = 16;
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
                if (raceMove.CarIndex < 0 || raceMove.CarIndex > 2)
                {
                    return;
                }

                if (raceMove.PlayerNumber == 1 && _field.PlayerOneCar < 0)
                {
                    _field.PlayerOneCar = raceMove.CarIndex;
                    SetSelectionFeedback(1, raceMove.CarIndex);
                    _field.CountdownText = "PLAYER 2: AUTO AUSWÄHLEN";
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
                        _field.CountdownText = "3";
                    }

                    return;
                }

                return;
            }

            if (!_field.IsRaceStarted)
            {
                double elapsed = (DateTime.UtcNow - _countdownStarted).TotalSeconds;
                if (elapsed >= 3)
                {
                    _field.IsRaceStarted = true;
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
            ApplyDownshiftLimit(
                ref _playerOneSpeed,
                _playerOneDownshiftLimit,
                _playerOneDownshiftUntil);
            ApplyDownshiftLimit(
                ref _playerTwoSpeed,
                _playerTwoDownshiftLimit,
                _playerTwoDownshiftUntil);
            _field.PlayerOneDistance = Advance(
                _field.PlayerOneDistance,
                ref _playerOneSpeed,
                _playerOneAccelerating,
                _playerOneBraking,
                _field.PlayerOneGear);
            _field.PlayerTwoDistance = Advance(
                _field.PlayerTwoDistance,
                ref _playerTwoSpeed,
                _playerTwoAccelerating,
                _playerTwoBraking,
                _field.PlayerTwoGear);
            _field.PlayerOneSpeed = _playerOneSpeed;
            _field.PlayerTwoSpeed = _playerTwoSpeed;
            _field.PlayerOneRpm =
                GetCurrentRpm(
                    _playerOneSpeed,
                    _field.PlayerOneGear,
                    _playerOneDownshiftUntil);
            _field.PlayerTwoRpm =
                GetCurrentRpm(
                    _playerTwoSpeed,
                    _field.PlayerTwoGear,
                    _playerTwoDownshiftUntil);

            if (_field.PlayerOneDistance >= Gruppe3DragRaceField.FinishDistance ||
                _field.PlayerTwoDistance >= Gruppe3DragRaceField.FinishDistance)
            {
                if (_field.PlayerOneDistance >= Gruppe3DragRaceField.FinishDistance &&
                    _field.PlayerTwoDistance >= Gruppe3DragRaceField.FinishDistance)
                {
                    _field.Winner = -1;
                    _field.CountdownText = "UNENTSCHIEDEN!";
                }
                else
                {
                    _field.Winner = _field.PlayerOneDistance >=
                        Gruppe3DragRaceField.FinishDistance ? 1 : 2;
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
            int gear)
        {
            if (braking)
            {
                speed = Math.Max(0, speed - 0.45);
            }
            else if (accelerating)
            {
                double rpm = GetRpm(speed, gear);
                bool atRedline = rpm >= 7800;
                if (!atRedline)
                {
                    double[] accelerationByGear =
                    {
                        0.62, 0.50, 0.40, 0.32, 0.25, 0.20
                    };
                    speed = Math.Min(
                        16.0,
                        speed + accelerationByGear[Math.Max(0, gear - 1)]);
                }
            }
            else
            {
                speed = Math.Max(0, speed - 0.05);
            }

            return Math.Max(0, distance + speed);
        }

        private static double GetRpm(double speed, int gear)
        {
            double[] gearStarts = { 0, 3.0, 5.5, 8.0, 10.5, 12.8 };
            double[] gearEnds = { 4.5, 7.2, 9.8, 12.2, 14.5, 16.0 };
            int gearIndex = Math.Max(0, Math.Min(5, gear - 1));
            double progress = (speed - gearStarts[gearIndex]) /
                (gearEnds[gearIndex] - gearStarts[gearIndex]);
            progress = Math.Max(0, Math.Min(1, progress));
            return 2500 + progress * 5500;
        }

        private static double GetCurrentRpm(
            double speed, int gear, DateTime downshiftUntil)
        {
            if (DateTime.UtcNow < downshiftUntil)
            {
                double normalRpm = GetRpm(speed, gear);
                return Math.Min(8200, normalRpm + 1800);
            }

            return GetRpm(speed, gear);
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

        private static double GetGearMaximum(int gear)
        {
            double[] gearEnds = { 4.5, 7.2, 9.8, 12.2, 14.5, 16.0 };
            return gearEnds[Math.Max(0, Math.Min(5, gear - 1))];
        }

        private void SetSelectionFeedback(int playerNumber, int carIndex)
        {
            _field.SelectionFeedbackText =
                "PLAYER " + playerNumber + ": AUTO " + (carIndex + 1) + " GEWÄHLT!";
            _field.SelectionFeedbackUntil = DateTime.UtcNow.AddSeconds(1.2);
        }

        private void ApplyComputerControls()
        {
            foreach (IGamePlayer player in OOPGamesManager.Singleton.ActivePlayers)
            {
                if (!(player is IComputerGamePlayer))
                {
                    continue;
                }

                if (player.PlayerNumber == 1)
                {
                    _playerOneAccelerating = true;
                    _playerOneBraking = false;
                    if (_field.PlayerOneRpm >= 7300 &&
                        _field.PlayerOneGear < 6)
                    {
                        _field.PlayerOneGear++;
                    }
                }
                else if (player.PlayerNumber == 2)
                {
                    _playerTwoAccelerating = true;
                    _playerTwoBraking = false;
                    if (_field.PlayerTwoRpm >= 7300 &&
                        _field.PlayerTwoGear < 6)
                    {
                        _field.PlayerTwoGear++;
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
                            Math.Min(6, _field.PlayerOneGear + 1);
                    }
                    if (shiftDownPressed && !_playerOneShiftDownPressed)
                    {
                        int newGear = Math.Max(1, _field.PlayerOneGear - 1);
                        if (newGear != _field.PlayerOneGear)
                        {
                            _field.PlayerOneGear = newGear;
                            _playerOneDownshiftLimit =
                                GetGearMaximum(newGear);
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
                            Math.Min(6, _field.PlayerTwoGear + 1);
                    }
                    if (shiftDownPressed && !_playerTwoShiftDownPressed)
                    {
                        int newGear = Math.Max(1, _field.PlayerTwoGear - 1);
                        if (newGear != _field.PlayerTwoGear)
                        {
                            _field.PlayerTwoGear = newGear;
                            _playerTwoDownshiftLimit =
                                GetGearMaximum(newGear);
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
