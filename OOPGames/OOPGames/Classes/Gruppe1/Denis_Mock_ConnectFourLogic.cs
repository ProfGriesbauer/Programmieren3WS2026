namespace OOPGames
{
    internal static class G1_ConnectFourLogic
    {
        internal const int Rows = 6;
        internal const int Columns = 7;

        internal static bool IsInside(int row, int column)
        {
            return row >= 0 && row < Rows && column >= 0 && column < Columns;
        }

        internal static int FreeRow(I4GWFeld field, int column)
        {
            if (column < 0 || column >= Columns)
            {
                return -1;
            }

            for (int row = Rows - 1; row >= 0; row--)
            {
                if (field[row, column] == 0)
                {
                    return row;
                }
            }

            return -1;
        }

        internal static bool IsFull(I4GWFeld field)
        {
            for (int column = 0; column < Columns; column++)
            {
                if (FreeRow(field, column) >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        internal static int Winner(I4GWFeld field)
        {
            int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };

            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    int player = field[row, column];
                    if (player <= 0)
                    {
                        continue;
                    }

                    for (int direction = 0; direction < 4; direction++)
                    {
                        int count = 1;
                        int rowStep = directions[direction, 0];
                        int columnStep = directions[direction, 1];

                        while (count < 4
                            && IsInside(row + count * rowStep, column + count * columnStep)
                            && field[row + count * rowStep, column + count * columnStep] == player)
                        {
                            count++;
                        }

                        if (count == 4)
                        {
                            return player;
                        }
                    }

                }
            }

            return -1;
        }

        internal static Gruppe1_ConnectFourField Copy(I4GWFeld source)
        {
            Gruppe1_ConnectFourField copy = new Gruppe1_ConnectFourField();
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    copy[row, column] = source[row, column];
                }
            }

            return copy;
        }

        internal static bool WinsAfterDrop(I4GWFeld field, int column, int player)
        {
            int row = FreeRow(field, column);
            if (row < 0)
            {
                return false;
            }

            Gruppe1_ConnectFourField copy = Copy(field);
            copy[row, column] = player;
            return Winner(copy) == player;
        }
    }

    internal static class G1_ConnectFourLayout
    {
        internal const int Left = 20;
        internal const int Top = 80;
        internal const int CellSize = 50;

        internal static int ColumnAt(int x, int y)
        {
            if (x < Left || y < Top
                || x >= Left + G1_ConnectFourLogic.Columns * CellSize
                || y >= Top + G1_ConnectFourLogic.Rows * CellSize)
            {
                return -1;
            }

            return (x - Left) / CellSize;
        }
    }
}
