using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    internal static class G8_VierGewinntLogic
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
    }

    internal static class G8_VierGewinntLayout
    {
        internal const int Left = 20;
        internal const int Top = 80;
        internal const int Cell = 50;

        internal static int ColumnAt(int x, int y)
        {
            if (x < Left || y < Top
                || x >= Left + G8_VierGewinntLogic.Columns * Cell
                || y >= Top + G8_VierGewinntLogic.Rows * Cell)
            {
                return -1;
            }

            return (x - Left) / Cell;
        }
    }

    public sealed class G8_VierGewinntField : I4GWFeld
    {
        private readonly int[,] _cells = new int[G8_VierGewinntLogic.Rows, G8_VierGewinntLogic.Columns];

        public int this[int row, int column]
        {
            get
            {
                return G8_VierGewinntLogic.IsInside(row, column) ? _cells[row, column] : -1;
            }
            set
            {
                if (G8_VierGewinntLogic.IsInside(row, column))
                {
                    _cells[row, column] = value;
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }
    }

    public sealed class G8_VierGewinntMove : I4GWMove
    {
        public G8_VierGewinntMove(int row, int column, int playerNumber)
        {
            Row = row;
            Column = column;
            PlayerNumber = playerNumber;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
        public int PlayerNumber { get; private set; }
    }

    public sealed class G8_VierGewinntRules : IGameRules2
    {
        private G8_VierGewinntField _field = new G8_VierGewinntField();
        private int _nextPlayer = 1;

        public string Name { get { return "Gruppe 8 Vier Gewinnt Regeln"; } }
        public IGameField CurrentField { get { return _field; } }

        public bool MovesPossible
        {
            get
            {
                return G8_VierGewinntLogic.Winner(_field) < 0
                    && !G8_VierGewinntLogic.IsFull(_field);
            }
        }

        public void ClearField()
        {
            _field = new G8_VierGewinntField();
            _nextPlayer = 1;
        }

        public void DoMove(IPlayMove move)
        {
            I4GWMove connectFourMove = move as I4GWMove;
            if (connectFourMove == null || !MovesPossible
                || connectFourMove.PlayerNumber != _nextPlayer)
            {
                return;
            }

            int row = G8_VierGewinntLogic.FreeRow(_field, connectFourMove.Column);
            if (row < 0)
            {
                return;
            }

            _field[row, connectFourMove.Column] = connectFourMove.PlayerNumber;
            _nextPlayer = _nextPlayer == 1 ? 2 : 1;
        }

        public int CheckIfPLayerWon()
        {
            return G8_VierGewinntLogic.Winner(_field);
        }

        public void StartedGameCall()
        {
        }

        public void TickGameCall()
        {
        }
    }

    public sealed class G8_VierGewinntPaint : IPaintGame2
    {
        private static readonly Brush PlayerOneBrush = new SolidColorBrush(Color.FromRgb(30, 144, 255));
        private static readonly Brush PlayerTwoBrush = new SolidColorBrush(Color.FromRgb(255, 105, 180));

        public string Name { get { return "Gruppe 8 Vier Gewinnt Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            I4GWFeld field = currentField as I4GWFeld;
            if (field == null)
            {
                canvas.Children.Clear();
                return;
            }

            canvas.Children.Clear();
            canvas.Background = Brushes.White;

            AddText(canvas, "Klick in eine Spalte oder Taste 1-7", 20, 8, 14);
            for (int column = 0; column < G8_VierGewinntLogic.Columns; column++)
            {
                AddText(canvas, (column + 1).ToString(),
                    G8_VierGewinntLayout.Left + column * G8_VierGewinntLayout.Cell + 20,
                    G8_VierGewinntLayout.Top - 22,
                    14);
            }

            Rectangle board = new Rectangle
            {
                Width = G8_VierGewinntLogic.Columns * G8_VierGewinntLayout.Cell,
                Height = G8_VierGewinntLogic.Rows * G8_VierGewinntLayout.Cell,
                Fill = Brushes.Black,
                Stroke = Brushes.DimGray,
                StrokeThickness = 2
            };
            Canvas.SetLeft(board, G8_VierGewinntLayout.Left);
            Canvas.SetTop(board, G8_VierGewinntLayout.Top);
            canvas.Children.Add(board);

            for (int row = 0; row < G8_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G8_VierGewinntLogic.Columns; column++)
                {
                    int player = field[row, column];
                    Brush fill = player == 1 ? PlayerOneBrush
                        : player == 2 ? PlayerTwoBrush
                        : Brushes.DimGray;
                    Brush outline = player == 1 ? Brushes.Navy
                        : player == 2 ? Brushes.DeepPink
                        : Brushes.Gray;
                    double diameter = G8_VierGewinntLayout.Cell - 10;

                    Ellipse disc = new Ellipse
                    {
                        Width = diameter,
                        Height = diameter,
                        Fill = fill,
                        Stroke = outline,
                        StrokeThickness = 2
                    };
                    Canvas.SetLeft(disc, G8_VierGewinntLayout.Left
                        + column * G8_VierGewinntLayout.Cell + 5);
                    Canvas.SetTop(disc, G8_VierGewinntLayout.Top
                        + row * G8_VierGewinntLayout.Cell + 5);
                    canvas.Children.Add(disc);
                }
            }

            int winner = G8_VierGewinntLogic.Winner(field);
            if (winner > 0)
            {
                AddText(canvas, "Spieler " + winner + " hat gewonnen!",
                    G8_VierGewinntLayout.Left, 30, 14);
            }
            else if (G8_VierGewinntLogic.IsFull(field))
            {
                AddText(canvas, "Unentschieden!",
                    G8_VierGewinntLayout.Left, 30, 14);
            }
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }

        private static void AddText(Canvas canvas, string text, double left, double top, double fontSize)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            canvas.Children.Add(label);
        }
    }

    public sealed class G8_VierGewinntHumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber;

        public string Name { get { return "Gruppe 8 Vier Gewinnt Human Player"; } }
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
            G8_VierGewinntHumanPlayer clone = new G8_VierGewinntHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            I4GWFeld connectFourField = field as I4GWFeld;
            if (connectFourField == null || _playerNumber < 1 || _playerNumber > 2
                || G8_VierGewinntLogic.Winner(connectFourField) > 0)
            {
                return null;
            }

            int column = -1;
            IClickSelection click = selection as IClickSelection;
            if (click != null)
            {
                column = G8_VierGewinntLayout.ColumnAt(click.XClickPos, click.YClickPos);
            }
            else
            {
                IKeySelection keySelection = selection as IKeySelection;
                if (keySelection != null)
                {
                    column = ColumnFromKey(keySelection.Key);
                }
            }

            int row = G8_VierGewinntLogic.FreeRow(connectFourField, column);
            return row >= 0 ? new G8_VierGewinntMove(row, column, _playerNumber) : null;
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

    public sealed class G8_VierGewinntComputerPlayer : IComputerGamePlayer
    {
        private int _playerNumber;

        public string Name { get { return "Gruppe 8 Vier Gewinnt Computer Player"; } }
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
            G8_VierGewinntComputerPlayer clone = new G8_VierGewinntComputerPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IGameField field)
        {
            I4GWFeld connectFourField = field as I4GWFeld;
            if (connectFourField == null || _playerNumber < 1 || _playerNumber > 2
                || G8_VierGewinntLogic.Winner(connectFourField) > 0)
            {
                return null;
            }

            int opponent = _playerNumber == 1 ? 2 : 1;
            int[] preferredColumns = { 3, 2, 4, 1, 5, 0, 6 };

            foreach (int column in preferredColumns)
            {
                if (WinsAfterDrop(connectFourField, column, _playerNumber))
                {
                    return CreateMove(connectFourField, column);
                }
            }

            foreach (int column in preferredColumns)
            {
                if (WinsAfterDrop(connectFourField, column, opponent))
                {
                    return CreateMove(connectFourField, column);
                }
            }

            foreach (int column in preferredColumns)
            {
                if (G8_VierGewinntLogic.FreeRow(connectFourField, column) >= 0)
                {
                    return CreateMove(connectFourField, column);
                }
            }

            return null;
        }

        private G8_VierGewinntMove CreateMove(I4GWFeld field, int column)
        {
            return new G8_VierGewinntMove(
                G8_VierGewinntLogic.FreeRow(field, column), column, _playerNumber);
        }

        private static bool WinsAfterDrop(I4GWFeld field, int column, int player)
        {
            int row = G8_VierGewinntLogic.FreeRow(field, column);
            if (row < 0)
            {
                return false;
            }

            G8_VierGewinntField copy = new G8_VierGewinntField();
            for (int currentRow = 0; currentRow < G8_VierGewinntLogic.Rows; currentRow++)
            {
                for (int currentColumn = 0; currentColumn < G8_VierGewinntLogic.Columns; currentColumn++)
                {
                    copy[currentRow, currentColumn] = field[currentRow, currentColumn];
                }
            }

            copy[row, column] = player;
            return G8_VierGewinntLogic.Winner(copy) == player;
        }
    }
}
