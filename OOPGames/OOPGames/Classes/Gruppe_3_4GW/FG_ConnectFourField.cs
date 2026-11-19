using System;

namespace OOPGames
{
    internal static class FG_ConnectFourBoardLogic
    {
        internal const int WinLength = 4;
        internal const int PlayerCount = 2;
        internal static readonly int[,] Directions =
        {
            { 0, 1 },
            { 1, 0 },
            { 1, 1 },
            { 1, -1 }
        };

        internal static int GetLandingRow(I4GWFeld field, int column)
        {
            if (column < 0 || column >= FG_ConnectFourField.Columns)
            {
                return -1;
            }

            for (int row = FG_ConnectFourField.Rows - 1; row >= 0; row--)
            {
                if (field[row, column] == 0)
                {
                    return row;
                }
            }

            return -1;
        }

        internal static int FindWinner(I4GWFeld field)
        {
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    int player = field[row, column];
                    if (player > 0 && HasWinningLine(field, row, column, player))
                    {
                        return player;
                    }
                }
            }

            return -1;
        }

        internal static bool HasWinningLine(
            I4GWFeld field,
            int row,
            int column,
            int player)
        {
            for (int direction = 0; direction < Directions.GetLength(0); direction++)
            {
                int rowStep = Directions[direction, 0];
                int columnStep = Directions[direction, 1];
                if (Count(field, row, column, rowStep, columnStep, player) >= WinLength)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool[,] GetWinningTokens(I4GWFeld field)
        {
            bool[,] winningTokens =
                new bool[FG_ConnectFourField.Rows, FG_ConnectFourField.Columns];

            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    int player = field[row, column];
                    if (player <= 0)
                    {
                        continue;
                    }

                    for (int direction = 0; direction < Directions.GetLength(0); direction++)
                    {
                        int rowStep = Directions[direction, 0];
                        int columnStep = Directions[direction, 1];
                        if (Count(field, row, column, rowStep, columnStep, player) <
                            WinLength)
                        {
                            continue;
                        }

                        for (int step = 0; step < WinLength; step++)
                        {
                            winningTokens[
                                row + rowStep * step,
                                column + columnStep * step] = true;
                        }
                    }
                }
            }

            return winningTokens;
        }

        internal static bool[,] GetWinningTokens(int[,] field)
        {
            bool[,] winningTokens =
                new bool[FG_ConnectFourField.Rows, FG_ConnectFourField.Columns];

            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    int player = field[row, column];
                    if (player <= 0)
                    {
                        continue;
                    }

                    for (int direction = 0; direction < Directions.GetLength(0); direction++)
                    {
                        int rowStep = Directions[direction, 0];
                        int columnStep = Directions[direction, 1];
                        if (Count(field, row, column, rowStep, columnStep, player) <
                            WinLength)
                        {
                            continue;
                        }

                        for (int step = 0; step < WinLength; step++)
                        {
                            winningTokens[
                                row + rowStep * step,
                                column + columnStep * step] = true;
                        }
                    }
                }
            }

            return winningTokens;
        }

        private static int Count(
            I4GWFeld field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player)
        {
            int count = 0;
            while (row >= 0 &&
                   row < FG_ConnectFourField.Rows &&
                   column >= 0 &&
                   column < FG_ConnectFourField.Columns &&
                   field[row, column] == player)
            {
                count++;
                row += rowStep;
                column += columnStep;
            }

            return count;
        }

        private static int Count(
            int[,] field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player)
        {
            int count = 0;
            while (row >= 0 &&
                   row < FG_ConnectFourField.Rows &&
                   column >= 0 &&
                   column < FG_ConnectFourField.Columns &&
                   field[row, column] == player)
            {
                count++;
                row += rowStep;
                column += columnStep;
            }

            return count;
        }
    }

    public sealed class FG_ConnectFourField : IFG_ConnectFourField
    {
        public const int Rows = 6;
        public const int Columns = 7;
        private readonly int[,] _board = new int[Rows, Columns];

        public int this[int row, int column]
        {
            get
            {
                if (row < 0 || row >= Rows || column < 0 || column >= Columns)
                {
                    return -1;
                }

                return _board[row, column];
            }
            set
            {
                if (row >= 0 && row < Rows && column >= 0 && column < Columns)
                {
                    _board[row, column] = value;
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }

    }
}
