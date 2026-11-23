using System.Collections.Generic;

namespace OOPGames
{
    internal static class Gruppe2_ConnectFourLogic     //Statische Logikklasse für das Spiel "Vier Gewinnt" von Gruppe 2. Sie enthält Methoden zur Überprüfung von Gewinnern, verfügbaren Zügen und Verbindungen von vier Spielsteinen. Diese Klasse ist intern, da sie nur innerhalb des Projekts verwendet wird.
    {
        internal static IReadOnlyList<(int RowStep, int ColumnStep)> Directions { get; } =
            System.Array.AsReadOnly(new (int RowStep, int ColumnStep)[]
            {
                (0, 1),
                (1, 0),
                (1, 1),
                (1, -1)
            });

        public static int FindWinner(I4GWFeld field)
        {
            foreach (WinningLine line in FindWinningLines(field))
            {
                return line.PlayerNumber;
            }

            return -1;
        }

        public static bool IsDraw(I4GWFeld field)
        {
            return FindWinner(field) < 0 && !HasAvailableMove(field);
        }

        public static bool HasAvailableMove(I4GWFeld field)
        {
            for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
            {
                if (FindAvailableRow(field, column) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static IEnumerable<WinningLine> FindWinningLines(I4GWFeld field)
        {
            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    int playerNumber = field[row, column];
                    if (playerNumber <= 0)
                    {
                        continue;
                    }

                    foreach ((int rowStep, int columnStep) in Directions)
                    {
                        int previousRow = row - rowStep;
                        int previousColumn = column - columnStep;
                        if (IsInsideBoard(previousRow, previousColumn)
                            && field[previousRow, previousColumn] == playerNumber)
                        {
                            continue;
                        }

                        int endRow = row;
                        int endColumn = column;
                        int count = 1;
                        while (IsInsideBoard(endRow + rowStep, endColumn + columnStep)
                            && field[endRow + rowStep, endColumn + columnStep] == playerNumber)
                        {
                            endRow += rowStep;
                            endColumn += columnStep;
                            count++;
                        }

                        if (count >= 4)
                        {
                            yield return new WinningLine(
                                playerNumber, row, column, endRow, endColumn);
                        }
                    }
                }
            }
        }

        public struct WinningLine
        {
            public WinningLine(int playerNumber, int startRow, int startColumn, int endRow, int endColumn)
            {
                PlayerNumber = playerNumber;
                StartRow = startRow;
                StartColumn = startColumn;
                EndRow = endRow;
                EndColumn = endColumn;
            }

            public int PlayerNumber { get; }
            public int StartRow { get; }
            public int StartColumn { get; }
            public int EndRow { get; }
            public int EndColumn { get; }
        }

        public static int FindAvailableRow(I4GWFeld field, int column)     //Methode, die die nächste verfügbare Zeile in einer Spalte findet. Sie durchläuft die Zeilen von unten nach oben und überprüft, ob die Zelle leer ist. Wenn ja, gibt sie die Zeilennummer zurück, sonst -1.
        {
            if (column < 0 || column >= Gruppe2_ConnectFourConstants.Columns)     //von unten nach oben, da die Spielsteine nach unten fallen. Wenn die Spalte ungültig ist, wird -1 zurückgegeben.
            {
                return -1;     //Wenn die Spalte ungültig ist, wird -1 zurückgegeben.
            }

            for (int row = Gruppe2_ConnectFourConstants.Rows - 1; row >= 0; row--)
            {
                if (field[row, column] == 0)
                {
                    return row;
                }
            }

            return -1;
        }

        private static bool IsInsideBoard(int row, int column)
        {
            return row >= 0
                && row < Gruppe2_ConnectFourConstants.Rows
                && column >= 0
                && column < Gruppe2_ConnectFourConstants.Columns;
        }

    }
}
