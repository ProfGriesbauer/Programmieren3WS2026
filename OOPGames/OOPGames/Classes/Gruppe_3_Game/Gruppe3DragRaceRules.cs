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
        private DateTime _countdownStarted;
        private readonly Random _random = new Random();

        public string Name { get { return "Gruppe 3 Retro Drag Race Regeln"; } }
        public IGameField CurrentField { get { return _field; } }
        public IGruppe3DragRaceField DragRaceField { get { return _field; } }
        public bool MovesPossible
        {
            get
            {
                return _field.Winner == 0 &&
                    (!_field.CarSelectionComplete || _field.IsRaceStarted);
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
            _playerOneAccelerating = false;
            _playerOneBraking = false;
            _playerTwoAccelerating = false;
            _playerTwoBraking = false;
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
                if (raceMove.CarIndex < 0 || raceMove.CarIndex > 8)
                {
                    return;
                }

                if (raceMove.PlayerNumber == 1 && _field.PlayerOneCar < 0)
                {
                    _field.PlayerOneCar = raceMove.CarIndex;
                    _field.CountdownText = "PLAYER 2: AUTO AUSWÄHLEN";
                }
                else if (raceMove.PlayerNumber == 2 && _field.PlayerTwoCar < 0)
                {
                    _field.PlayerTwoCar = raceMove.CarIndex;
                }

                if (_field.PlayerOneCar >= 0 && _field.PlayerTwoCar >= 0)
                {
                    _field.CarSelectionComplete = true;
                    _countdownStarted = DateTime.UtcNow;
                    _field.CountdownText = "3";
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
            _field.PlayerOneDistance = Advance(
                _field.PlayerOneDistance, _playerOneAccelerating, _playerOneBraking);
            _field.PlayerTwoDistance = Advance(
                _field.PlayerTwoDistance, _playerTwoAccelerating, _playerTwoBraking);

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

        private static double Advance(double distance, bool accelerating, bool braking)
        {
            double speed = accelerating ? 4.0 : 0.6;
            if (braking)
            {
                speed = -2.0;
            }

            return Math.Max(0, distance + speed);
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
                }
                else if (player.PlayerNumber == 2)
                {
                    _playerTwoAccelerating = true;
                    _playerTwoBraking = false;
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
                }
                else if (player.PlayerNumber == 2)
                {
                    _playerTwoAccelerating =
                        Keyboard.IsKeyDown(Key.NumPad8) ||
                        Keyboard.IsKeyDown(Key.Up);
                    _playerTwoBraking =
                        Keyboard.IsKeyDown(Key.NumPad5) ||
                        Keyboard.IsKeyDown(Key.Down);
                }
            }
        }
    }
}
