using System.Windows.Input;

namespace OOPGames
{
    public sealed class Gruppe1_ConnectFourHumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber;

        public string Name { get { return "Gruppe 1 Connect Four Human Player"; } }

        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }

        public IGamePlayer Clone()
        {
            Gruppe1_ConnectFourHumanPlayer clone = new Gruppe1_ConnectFourHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField gameField)
        {
            I4GWFeld field = gameField as I4GWFeld;
            if (field == null || _playerNumber < 1 || _playerNumber > 2
                || G1_ConnectFourLogic.Winner(field) > 0)
            {
                return null;
            }

            int column = -1;
            IClickSelection click = selection as IClickSelection;
            if (click != null)
            {
                IG1_ConnectFourPainter painter =
                    OOPGamesManager.Singleton.ActivePainter as IG1_ConnectFourPainter;
                column = painter != null
                    ? painter.ColumnAt(click.XClickPos, click.YClickPos)
                    : G1_ConnectFourLayout.ColumnAt(click.XClickPos, click.YClickPos);
            }
            else
            {
                IKeySelection keySelection = selection as IKeySelection;
                if (keySelection != null)
                {
                    column = ColumnFromKey(keySelection.Key);
                }
            }

            int row = G1_ConnectFourLogic.FreeRow(field, column);
            return row >= 0
                ? new Gruppe1_ConnectFourMove(row, column, _playerNumber)
                : null;
        }

        private static int ColumnFromKey(Key key)
        {
            if (key >= Key.D1 && key <= Key.D7)
            {
                return (int)key - (int)Key.D1;
            }

            if (key >= Key.NumPad1 && key <= Key.NumPad7)
            {
                return (int)key - (int)Key.NumPad1;
            }

            return -1;
        }
    }

}
