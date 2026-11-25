using System;

namespace OOPGames
{
    public sealed class Gruppe3DragRaceRules : IGruppe3DragRaceRules
    {
        private readonly Gruppe3DragRaceField _field =
            new Gruppe3DragRaceField();
        private readonly Random _random = new Random();
        private readonly Gruppe3DragRaceRacerState _playerOne;
        private readonly Gruppe3DragRaceRacerState _playerTwo;
        private DateTime _countdownStarted;
        private DateTime _raceStartedAt;

        public Gruppe3DragRaceRules()
        {
            _playerOne = new Gruppe3DragRaceRacerState(1, _random);
            _playerTwo = new Gruppe3DragRaceRacerState(2, _random);
        }

        public string Name { get { return "Gruppe 3 Retro Drag Race Regeln"; } }
        public IGameField CurrentField { get { return _field; } }
        public IGruppe3DragRaceField DragRaceField { get { return _field; } }

        public bool MovesPossible
        {
            get
            {
                bool selectionInProgress = !_field.CarSelectionComplete &&
                    !(_playerOne.CarIndex >= 0 && _playerTwo.CarIndex >= 0);
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
            _field.IsRaceStarted = false;
            _field.CountdownText = "PLAYER 1: AUTO AUSWAEHLEN";
            _field.Winner = 0;
            _field.CarSelectionComplete = false;
            _field.SelectionFeedbackText = null;
            _field.SelectionFeedbackUntil = DateTime.MinValue;
            _countdownStarted = DateTime.MinValue;
            _raceStartedAt = DateTime.MinValue;
            _playerOne.Reset();
            _playerTwo.Reset();
            SyncField();
        }

        public void DoMove(IPlayMove move)
        {
            Gruppe3DragRaceMove raceMove = move as Gruppe3DragRaceMove;
            if (raceMove == null ||
                raceMove.PlayerNumber < 1 ||
                raceMove.PlayerNumber > 2)
            {
                return;
            }

            Gruppe3DragRaceRacerState racer = GetRacer(raceMove.PlayerNumber);
            if (!_field.CarSelectionComplete)
            {
                if (raceMove.CarIndex < 0 ||
                    raceMove.CarIndex >= Gruppe3DragRaceCars.All.Count ||
                    racer.CarIndex >= 0)
                {
                    return;
                }

                racer.CarIndex = raceMove.CarIndex;
                racer.SyncToField(_field);
                SetSelectionFeedback(raceMove.PlayerNumber, raceMove.CarIndex);
                if (raceMove.PlayerNumber == 1)
                {
                    _field.CountdownText = "PLAYER 2: AUTO AUSWAEHLEN";
                }

                return;
            }

            racer.Accelerating = raceMove.Accelerate;
            racer.Braking = raceMove.Brake;
        }

        public void TickGameCall()
        {
            DateTime now = DateTime.UtcNow;
            if (_field.Winner != 0)
            {
                _playerOne.RollOut(now);
                _playerTwo.RollOut(now);
                SyncField();
                return;
            }

            if (!_field.CarSelectionComplete)
            {
                TryCompleteCarSelection(now);
                return;
            }

            if (!_field.IsRaceStarted)
            {
                TickCountdown(now);
                return;
            }

            TickRace(now);
        }

        public int CheckIfPLayerWon()
        {
            return _field.Winner;
        }

        private Gruppe3DragRaceRacerState GetRacer(int playerNumber)
        {
            return playerNumber == 1 ? _playerOne : _playerTwo;
        }

        private void TryCompleteCarSelection(DateTime now)
        {
            if (_playerOne.CarIndex < 0 || _playerTwo.CarIndex < 0 ||
                now < _field.SelectionFeedbackUntil)
            {
                return;
            }

            _field.CarSelectionComplete = true;
            _countdownStarted = now;
            _playerOne.ConfigureForCountdown(
                OOPGamesManager.Singleton.ActivePlayers, _countdownStarted);
            _playerTwo.ConfigureForCountdown(
                OOPGamesManager.Singleton.ActivePlayers, _countdownStarted);
            _field.CountdownText = "3";
            SyncField();
        }

        private void TickCountdown(DateTime now)
        {
            _playerOne.ApplyCountdownInput(now);
            _playerTwo.ApplyCountdownInput(now);
            _playerOne.UpdateCountdownRpm();
            _playerTwo.UpdateCountdownRpm();

            double elapsed = (now - _countdownStarted).TotalSeconds;
            if (elapsed < Gruppe3DragRaceConstants.CountdownSeconds && HandleFalseStart(now))
            {
                return;
            }

            if (elapsed >= Gruppe3DragRaceConstants.CountdownSeconds)
            {
                _field.IsRaceStarted = true;
                _raceStartedAt = now;
                _playerOne.OnRaceStarted(_raceStartedAt);
                _playerTwo.OnRaceStarted(_raceStartedAt);
                _field.CountdownText = "GO!";
            }
            else
            {
                _field.CountdownText = Math.Ceiling(Gruppe3DragRaceConstants.CountdownSeconds - elapsed).ToString();
            }

            SyncField();
        }

        private void TickRace(DateTime now)
        {
            _playerOne.ApplyRaceInput(now, _raceStartedAt);
            _playerTwo.ApplyRaceInput(now, _raceStartedAt);
            _playerOne.StepRace(now, _raceStartedAt);
            _playerTwo.StepRace(now, _raceStartedAt);
            SyncField();

            if (_playerOne.Finished && _playerTwo.Finished)
            {
                DetermineWinner();
            }
        }

        private bool HandleFalseStart(DateTime now)
        {
            bool playerOneFalseStart = _playerOne.HasFalseStart();
            bool playerTwoFalseStart = _playerTwo.HasFalseStart();
            if (!playerOneFalseStart && !playerTwoFalseStart)
            {
                return false;
            }

            double reactionTimeMs =
                (now - _countdownStarted.AddSeconds(Gruppe3DragRaceConstants.CountdownSeconds)).TotalMilliseconds;
            if (playerOneFalseStart)
            {
                _playerOne.ApplyFalseStart(now, reactionTimeMs);
            }
            if (playerTwoFalseStart)
            {
                _playerTwo.ApplyFalseStart(now, reactionTimeMs);
            }

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

            _playerOne.RecordCurrentFinishSpeed();
            _playerTwo.RecordCurrentFinishSpeed();
            SyncField();
            return true;
        }

        private void DetermineWinner()
        {
            double playerOneTime = _playerOne.RaceTimeSeconds.Value;
            double playerTwoTime = _playerTwo.RaceTimeSeconds.Value;
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

        private void SetSelectionFeedback(int playerNumber, int carIndex)
        {
            _field.SelectionFeedbackText =
                "PLAYER " + playerNumber + ": AUTO " + (carIndex + 1) +
                " GEWAEHLT!";
            _field.SelectionFeedbackUntil = DateTime.UtcNow.AddSeconds(Gruppe3DragRaceConstants.SelectionFeedbackDurationSeconds);
        }

        private void SyncField()
        {
            _playerOne.SyncToField(_field);
            _playerTwo.SyncToField(_field);
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
    }
}
