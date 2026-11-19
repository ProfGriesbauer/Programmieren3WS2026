using System.Windows.Input;

namespace OOPGames
{
    public sealed class Gruppe3DragRaceHumanPlayer : IGruppe3DragRaceHumanPlayer
    {
        private int _playerNumber;

        public string Name { get { return "Gruppe 3 Mensch Drag Race"; } }
        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber) { _playerNumber = playerNumber; }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is IGruppe3DragRaceField;
        }

        public IGamePlayer Clone()
        {
            Gruppe3DragRaceHumanPlayer clone = new Gruppe3DragRaceHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            return GetMove(selection, field as IGruppe3DragRaceField);
        }

        public IGruppe3DragRaceMove GetMove(
            IMoveSelection selection, IGruppe3DragRaceField field)
        {
            if (field == null || selection == null)
            {
                return null;
            }

            if (!field.CarSelectionComplete)
            {
                IClickSelection clickSelection = selection as IClickSelection;
                IGruppe3DragRacePainter painter =
                    OOPGamesManager.Singleton.ActivePainter as IGruppe3DragRacePainter;
                if (clickSelection == null || painter == null)
                {
                    return null;
                }

                int carIndex = painter.CarAt(
                    clickSelection.XClickPos, clickSelection.YClickPos);
                return carIndex >= 0
                    ? new Gruppe3DragRaceMove(_playerNumber, false, false, carIndex)
                    : null;
            }

            if (!field.IsRaceStarted)
            {
                return null;
            }

            IKeySelection keySelection = selection as IKeySelection;
            if (keySelection == null)
            {
                return null;
            }

            bool accelerate = _playerNumber == 1
                ? keySelection.Key == Key.W
                : keySelection.Key == Key.NumPad8 ||
                    keySelection.Key == Key.Up;
            bool brake = _playerNumber == 1
                ? keySelection.Key == Key.S
                : keySelection.Key == Key.NumPad5 ||
                    keySelection.Key == Key.Down;
            return accelerate || brake
                ? new Gruppe3DragRaceMove(_playerNumber, accelerate, brake)
                : null;
        }
    }

    public sealed class Gruppe3DragRaceComputerPlayer : IGruppe3DragRaceComputerPlayer
    {
        private int _playerNumber;

        public string Name { get { return "Gruppe 3 Computer Drag Race"; } }
        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber) { _playerNumber = playerNumber; }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is IGruppe3DragRaceField;
        }

        public IGamePlayer Clone()
        {
            Gruppe3DragRaceComputerPlayer clone = new Gruppe3DragRaceComputerPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IGameField field)
        {
            IGruppe3DragRaceField raceField = field as IGruppe3DragRaceField;
            return GetMove(raceField);
        }

        public IGruppe3DragRaceMove GetMove(IGruppe3DragRaceField field)
        {
            if (field == null)
            {
                return null;
            }

            if (!field.CarSelectionComplete)
            {
                int carIndex = _random.Next(9);
                if (_playerNumber == 2 && carIndex == field.PlayerOneCar)
                {
                    carIndex = (carIndex + 1) % 9;
                }

                return new Gruppe3DragRaceMove(
                    _playerNumber, false, false, carIndex);
            }

            return field.IsRaceStarted
                ? new Gruppe3DragRaceMove(_playerNumber, true, false)
                : null;
        }

        private static readonly System.Random _random = new System.Random();
    }
}
