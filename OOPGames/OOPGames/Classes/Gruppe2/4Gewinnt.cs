using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    internal static class Gruppe2_ConnectFourConstants
    {
        public const int Rows = 6;
        public const int Columns = 7;
        public const int CellSize = 40;
    }

    public class Gruppe2_ConnectFourField : I4GWFeld
    {
        private readonly int[,] _cells =
            new int[Gruppe2_ConnectFourConstants.Rows, Gruppe2_ConnectFourConstants.Columns];

        public int this[int row, int column]
        {
            get
            {
                if (!IsValidCell(row, column))
                {
                    return -1;
                }

                return _cells[row, column];
            }
            set
            {
                if (IsValidCell(row, column))
                {
                    _cells[row, column] = value;
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }

        private static bool IsValidCell(int row, int column)
        {
            return row >= 0 && row < Gruppe2_ConnectFourConstants.Rows
                && column >= 0 && column < Gruppe2_ConnectFourConstants.Columns;
        }
    }

    public class Gruppe2_ConnectFourMove : I4GWMove
    {
        public Gruppe2_ConnectFourMove(int row, int column, int playerNumber)
        {
            Row = row;
            Column = column;
            PlayerNumber = playerNumber;
        }

        public int Row { get; }

        public int Column { get; }

        public int PlayerNumber { get; }
    }

    public class Gruppe2_ConnectFourRules : IGameRules
    {
        private readonly I4GWFeld _field = new Gruppe2_ConnectFourField();

        public string Name { get { return "Group 2 - Connect Four Rules"; } }

        public I4GWFeld ConnectFourField { get { return _field; } }

        public IGameField CurrentField { get { return _field; } }

        public bool MovesPossible
        {
            get
            {
                if (CheckIfPLayerWon() > 0)
                {
                    return false;
                }

                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    if (Gruppe2_ConnectFourLogic.FindAvailableRow(_field, column) >= 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void DoMove(IPlayMove move)
        {
            if (!(move is I4GWMove connectFourMove)
                || connectFourMove.PlayerNumber < 1
                || connectFourMove.PlayerNumber > 2
                || CheckIfPLayerWon() > 0)
            {
                return;
            }

            int availableRow = Gruppe2_ConnectFourLogic.FindAvailableRow(_field, connectFourMove.Column);
            if (availableRow < 0 || connectFourMove.Row != availableRow)
            {
                return;
            }

            _field[availableRow, connectFourMove.Column] = connectFourMove.PlayerNumber;
        }

        public void ClearField()
        {
            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    _field[row, column] = 0;
                }
            }
        }

        public int CheckIfPLayerWon()
        {
            return Gruppe2_ConnectFourLogic.FindWinner(_field);
        }

        public bool IsDraw
        {
            get { return !MovesPossible && CheckIfPLayerWon() < 0; }
        }
    }

    internal static class Gruppe2_ConnectFourLogic
    {
        private static readonly int[,] Directions =
        {
            { 0, 1 },
            { 1, 0 },
            { 1, 1 },
            { 1, -1 }
        };

        public static bool ConnectsFour(I4GWFeld field, int row, int column, int playerNumber)
        {
            for (int direction = 0; direction < Directions.GetLength(0); direction++)
            {
                int rowStep = Directions[direction, 0];
                int columnStep = Directions[direction, 1];
                int count = 1
                    + CountDirection(field, row, column, rowStep, columnStep, playerNumber)
                    + CountDirection(field, row, column, -rowStep, -columnStep, playerNumber);

                if (count >= 4)
                {
                    return true;
                }
            }

            return false;
        }

        public static int FindWinner(I4GWFeld field)
        {
            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    int playerNumber = field[row, column];
                    if (playerNumber > 0
                        && ConnectsFour(field, row, column, playerNumber))
                    {
                        return playerNumber;
                    }
                }
            }

            return -1;
        }

        public static int FindAvailableRow(I4GWFeld field, int column)
        {
            if (column < 0 || column >= Gruppe2_ConnectFourConstants.Columns)
            {
                return -1;
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

        private static int CountDirection(
            I4GWFeld field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int playerNumber)
        {
            int count = 0;
            int currentRow = row + rowStep;
            int currentColumn = column + columnStep;

            while (currentRow >= 0
                && currentRow < Gruppe2_ConnectFourConstants.Rows
                && currentColumn >= 0
                && currentColumn < Gruppe2_ConnectFourConstants.Columns
                && field[currentRow, currentColumn] == playerNumber)
            {
                count++;
                currentRow += rowStep;
                currentColumn += columnStep;
            }

            return count;
        }
    }

    public class Gruppe2_ConnectFourPainter : IPaintGame
    {
        public string Name { get { return "Group 2 - Connect Four Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (canvas == null || !(currentField is I4GWFeld field))
            {
                return;
            }

            canvas.Children.Clear();
            canvas.Background = Brushes.White;

            int width = Gruppe2_ConnectFourConstants.Columns * Gruppe2_ConnectFourConstants.CellSize;
            int height = Gruppe2_ConnectFourConstants.Rows * Gruppe2_ConnectFourConstants.CellSize;
            Rectangle board = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = Brushes.RoyalBlue
            };
            canvas.Children.Add(board);

            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    Ellipse piece = new Ellipse
                    {
                        Width = Gruppe2_ConnectFourConstants.CellSize - 8,
                        Height = Gruppe2_ConnectFourConstants.CellSize - 8,
                        Fill = GetPlayerBrush(field[row, column])
                    };
                    Canvas.SetLeft(piece, column * Gruppe2_ConnectFourConstants.CellSize + 4);
                    Canvas.SetTop(piece, row * Gruppe2_ConnectFourConstants.CellSize + 4);
                    canvas.Children.Add(piece);
                }
            }

            if (IsDraw(field))
            {
                TextBlock drawMessage = new TextBlock
                {
                    Text = "Draw",
                    FontSize = 22,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    Background = Brushes.White,
                    Padding = new Thickness(8)
                };
                Canvas.SetLeft(drawMessage, 75);
                Canvas.SetTop(drawMessage, 100);
                canvas.Children.Add(drawMessage);
            }
        }

        private static Brush GetPlayerBrush(int playerNumber)
        {
            if (playerNumber == 1)
            {
                return Brushes.Red;
            }

            if (playerNumber == 2)
            {
                return Brushes.Yellow;
            }

            return Brushes.White;
        }

        private static bool IsDraw(I4GWFeld field)
        {
            if (Gruppe2_ConnectFourLogic.FindWinner(field) > 0)
            {
                return false;
            }

            for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
            {
                if (Gruppe2_ConnectFourLogic.FindAvailableRow(field, column) >= 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class Gruppe2_ConnectFourHumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber;

        public string Name { get { return "Group 2 - Connect Four Human Player"; } }

        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public IGamePlayer Clone()
        {
            Gruppe2_ConnectFourHumanPlayer clone = new Gruppe2_ConnectFourHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (!(selection is IClickSelection click)
                || !(field is I4GWFeld connectFourField)
                || _playerNumber < 1
                || _playerNumber > 2
                || click.XClickPos < 0
                || click.XClickPos >= Gruppe2_ConnectFourConstants.Columns * Gruppe2_ConnectFourConstants.CellSize
                || click.YClickPos < 0
                || click.YClickPos >= Gruppe2_ConnectFourConstants.Rows * Gruppe2_ConnectFourConstants.CellSize)
            {
                return null;
            }

            int column = click.XClickPos / Gruppe2_ConnectFourConstants.CellSize;
            int row = Gruppe2_ConnectFourLogic.FindAvailableRow(connectFourField, column);
            if (row < 0)
            {
                return null;
            }

            return new Gruppe2_ConnectFourMove(row, column, _playerNumber);
        }
    }

    public class Gruppe2_ConnectFourComputerPlayer : IComputerGamePlayer
    {
        private static readonly int[] PreferredColumns = { 3, 2, 4, 1, 5, 0, 6 };
        private int _playerNumber;

        public string Name { get { return "Group 2 - Connect Four Computer Player"; } }

        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public IGamePlayer Clone()
        {
            Gruppe2_ConnectFourComputerPlayer clone = new Gruppe2_ConnectFourComputerPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }

        public IPlayMove GetMove(IGameField field)
        {
            if (!(field is I4GWFeld connectFourField)
                || _playerNumber < 1
                || _playerNumber > 2
                || Gruppe2_ConnectFourLogic.FindWinner(connectFourField) > 0)
            {
                return null;
            }

            int opponentNumber = _playerNumber == 1 ? 2 : 1;
            foreach (int column in PreferredColumns)
            {
                int row = Gruppe2_ConnectFourLogic.FindAvailableRow(connectFourField, column);
                if (row >= 0
                    && Gruppe2_ConnectFourLogic.ConnectsFour(
                        connectFourField, row, column, _playerNumber))
                {
                    return new Gruppe2_ConnectFourMove(row, column, _playerNumber);
                }
            }

            foreach (int column in PreferredColumns)
            {
                int row = Gruppe2_ConnectFourLogic.FindAvailableRow(connectFourField, column);
                if (row >= 0
                    && Gruppe2_ConnectFourLogic.ConnectsFour(
                        connectFourField, row, column, opponentNumber))
                {
                    return new Gruppe2_ConnectFourMove(row, column, _playerNumber);
                }
            }

            foreach (int column in PreferredColumns)
            {
                int row = Gruppe2_ConnectFourLogic.FindAvailableRow(connectFourField, column);
                if (row >= 0)
                {
                    return new Gruppe2_ConnectFourMove(row, column, _playerNumber);
                }
            }

            return null;
        }
    }
}