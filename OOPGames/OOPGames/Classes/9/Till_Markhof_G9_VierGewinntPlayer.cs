using System.Windows.Controls;

namespace OOPGames
{
    public class G9_Human4GewinntPlayer : IG9_Human4GewinntPlayer
    {
        private const double BoardLeft = 20;
        private const double BoardTop = 20;
        private const double CellSize = 60;
        private int _playerNumber;

        public string Name { get { return "G9 Till Markhof Vier Gewinnt Mensch"; } }
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
            G9_Human4GewinntPlayer clone = new G9_Human4GewinntPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (!(field is I4GWFeld fourWinsField))
            {
                return null;
            }

            return GetMove(selection, fourWinsField);
        }

        public I4GWMove GetMove(IMoveSelection selection, I4GWFeld field)
        {
            if (!(selection is IClickSelection click)
                || field == null
                || _playerNumber < 1
                || _playerNumber > 2)
            {
                return null;
            }

            double x = click.XClickPos - BoardLeft;
            double y = click.YClickPos - BoardTop;
            if (x < 0 || x >= G9_VierGewinntBoard.Columns * CellSize
                || y < 0 || y >= G9_VierGewinntBoard.Rows * CellSize)
            {
                return null;
            }

            int column = (int)(x / CellSize);
            int row = G9_VierGewinntRules.GetLandingRow(field, column);
            return row < 0 ? null : new G9_VierGewinntMove(row, column, _playerNumber);
        }
    }

    public class G9_Computer4GewinntPlayer : IG9_Computer4GewinntPlayer
    {
        private static readonly int[] PreferredColumns = { 3, 2, 4, 1, 5, 0, 6 };
        private int _playerNumber;

        public string Name { get { return "G9 Till Markhof Vier Gewinnt Bot"; } }
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
            G9_Computer4GewinntPlayer clone = new G9_Computer4GewinntPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IGameField field)
        {
            if (!(field is I4GWFeld fourWinsField))
            {
                return null;
            }

            return GetMove(fourWinsField);
        }

        public I4GWMove GetMove(I4GWFeld field)
        {
            if (field == null || _playerNumber < 1 || _playerNumber > 2)
            {
                return null;
            }

            int column = FindWinningColumn(field, _playerNumber);
            if (column < 0)
            {
                column = FindWinningColumn(field, _playerNumber == 1 ? 2 : 1);
            }

            if (column < 0)
            {
                foreach (int preferredColumn in PreferredColumns)
                {
                    if (G9_VierGewinntRules.GetLandingRow(field, preferredColumn) >= 0)
                    {
                        column = preferredColumn;
                        break;
                    }
                }
            }

            int row = G9_VierGewinntRules.GetLandingRow(field, column);
            return row < 0 ? null : new G9_VierGewinntMove(row, column, _playerNumber);
        }

        private static int FindWinningColumn(I4GWFeld field, int player)
        {
            foreach (int column in PreferredColumns)
            {
                int row = G9_VierGewinntRules.GetLandingRow(field, column);
                if (row >= 0 && IsWinningMove(field, row, column, player))
                {
                    return column;
                }
            }

            return -1;
        }

        private static bool IsWinningMove(I4GWFeld field, int row, int column, int player)
        {
            return CountInDirection(field, row, column, 0, 1, player) >= 4
                || CountInDirection(field, row, column, 1, 0, player) >= 4
                || CountInDirection(field, row, column, 1, 1, player) >= 4
                || CountInDirection(field, row, column, 1, -1, player) >= 4;
        }

        private static int CountInDirection(
            I4GWFeld field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player)
        {
            return 1
                + CountFrom(field, row, column, rowStep, columnStep, player)
                + CountFrom(field, row, column, -rowStep, -columnStep, player);
        }

        private static int CountFrom(
            I4GWFeld field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player)
        {
            int count = 0;
            int nextRow = row + rowStep;
            int nextColumn = column + columnStep;
            while (nextRow >= 0 && nextRow < G9_VierGewinntBoard.Rows
                && nextColumn >= 0 && nextColumn < G9_VierGewinntBoard.Columns
                && field[nextRow, nextColumn] == player)
            {
                count++;
                nextRow += rowStep;
                nextColumn += columnStep;
            }

            return count;
        }
    }
}
