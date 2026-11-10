namespace OOPGames
{
    internal static class G9_VierGewinntBoard
    {
        internal const int Rows = 6;
        internal const int Columns = 7;
    }

    public class G9_VierGewinntField : I4GWFeld
    {
        private readonly int[,] _cells = new int[G9_VierGewinntBoard.Rows, G9_VierGewinntBoard.Columns];

        public int this[int row, int column]
        {
            get
            {
                if (!IsInBounds(row, column))
                {
                    return -1;
                }

                return _cells[row, column];
            }
            set
            {
                if (IsInBounds(row, column))
                {
                    _cells[row, column] = value;
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is IG9_4GewinntPainter;
        }

        private static bool IsInBounds(int row, int column)
        {
            return row >= 0 && row < G9_VierGewinntBoard.Rows
                && column >= 0 && column < G9_VierGewinntBoard.Columns;
        }
    }

    public class G9_VierGewinntMove : I4GWMove
    {
        public G9_VierGewinntMove(int row, int column, int playerNumber)
        {
            Row = row;
            Column = column;
            PlayerNumber = playerNumber;
        }

        public int Row { get; }
        public int Column { get; }
        public int PlayerNumber { get; }
    }

    public class G9_VierGewinntRules : IG9_4GewinntRules
    {
        private readonly G9_VierGewinntField _field = new G9_VierGewinntField();

        public string Name { get { return "G9 Till Markhof Vier Gewinnt Rules"; } }
        public I4GWFeld VierGewinntField { get { return _field; } }
        public IGameField CurrentField { get { return _field; } }

        public bool MovesPossible
        {
            get
            {
                if (CheckIfPLayerWon() > 0)
                {
                    return false;
                }

                for (int column = 0; column < G9_VierGewinntBoard.Columns; column++)
                {
                    if (_field[0, column] == 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void DoMove(IPlayMove move)
        {
            if (move is I4GWMove fourWinsMove)
            {
                Do4GewinntMove(fourWinsMove);
            }
        }

        public void Do4GewinntMove(I4GWMove move)
        {
            if (move == null || move.PlayerNumber < 1 || move.PlayerNumber > 2 || !MovesPossible)
            {
                return;
            }

            int landingRow = GetLandingRow(_field, move.Column);
            if (landingRow >= 0 && landingRow == move.Row)
            {
                _field[landingRow, move.Column] = move.PlayerNumber;
            }
        }

        public void ClearField()
        {
            for (int row = 0; row < G9_VierGewinntBoard.Rows; row++)
            {
                for (int column = 0; column < G9_VierGewinntBoard.Columns; column++)
                {
                    _field[row, column] = 0;
                }
            }
        }

        public int CheckIfPLayerWon()
        {
            for (int row = 0; row < G9_VierGewinntBoard.Rows; row++)
            {
                for (int column = 0; column < G9_VierGewinntBoard.Columns; column++)
                {
                    int player = _field[row, column];
                    if (player > 0
                        && (HasFour(row, column, 0, 1, player)
                            || HasFour(row, column, 1, 0, player)
                            || HasFour(row, column, 1, 1, player)
                            || HasFour(row, column, 1, -1, player)))
                    {
                        return player;
                    }
                }
            }

            return -1;
        }

        internal static int GetLandingRow(I4GWFeld field, int column)
        {
            if (field == null || column < 0 || column >= G9_VierGewinntBoard.Columns)
            {
                return -1;
            }

            for (int row = G9_VierGewinntBoard.Rows - 1; row >= 0; row--)
            {
                if (field[row, column] == 0)
                {
                    return row;
                }
            }

            return -1;
        }

        private bool HasFour(int row, int column, int rowStep, int columnStep, int player)
        {
            for (int step = 1; step < 4; step++)
            {
                int nextRow = row + rowStep * step;
                int nextColumn = column + columnStep * step;
                if (nextRow < 0 || nextRow >= G9_VierGewinntBoard.Rows
                    || nextColumn < 0 || nextColumn >= G9_VierGewinntBoard.Columns
                    || _field[nextRow, nextColumn] != player)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
