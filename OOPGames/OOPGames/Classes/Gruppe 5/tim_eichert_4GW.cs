using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OOPGames
{
    internal static class G5_4GWLogic
    {
        internal const int Rows = 6;
        internal const int Columns = 7;
        internal const double Left = 10;
        internal const double Top = 10;
        internal const double CellSize = 36;
        internal const int SearchDepth = 5;
        private static readonly int[] PreferredColumns = { 3, 2, 4, 1, 5, 0, 6 };
        internal static int ComputerDelayMilliseconds { get; private set; } = 500;

        internal static void SetComputerDelay(double milliseconds)
        {
            ComputerDelayMilliseconds = (int)Math.Round(milliseconds / 100) * 100;
        }

        internal static void WaitForComputerDelay()
        {
            int delay = ComputerDelayMilliseconds;
            if (delay <= 0)
            {
                return;
            }

            DispatcherFrame frame = new DispatcherFrame();
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(delay)
            };
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                frame.Continue = false;
            };
            timer.Start();
            Dispatcher.PushFrame(frame);
        }

        internal static int FindOpenRow(I4GWFeld field, int column)
        {
            if (field == null || column < 0 || column >= Columns)
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

        internal static bool HasWinner(I4GWFeld field, out int winner)
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    int player = field[row, column];
                    if (player > 0 &&
                        (HasLine(field, row, column, 0, 1, player) ||
                         HasLine(field, row, column, 1, 0, player) ||
                         HasLine(field, row, column, 1, 1, player) ||
                         HasLine(field, row, column, 1, -1, player)))
                    {
                        winner = player;
                        return true;
                    }
                }
            }

            winner = -1;
            return false;
        }

        internal static bool TryFindWinningLine(
            I4GWFeld field,
            out int startRow,
            out int startColumn,
            out int rowStep,
            out int columnStep)
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
                        int currentRowStep = directions[direction, 0];
                        int currentColumnStep = directions[direction, 1];
                        if (HasLine(field, row, column, currentRowStep, currentColumnStep, player))
                        {
                            startRow = row;
                            startColumn = column;
                            rowStep = currentRowStep;
                            columnStep = currentColumnStep;
                            return true;
                        }
                    }
                }
            }

            startRow = -1;
            startColumn = -1;
            rowStep = 0;
            columnStep = 0;
            return false;
        }

        internal static bool IsFull(I4GWFeld field)
        {
            for (int column = 0; column < Columns; column++)
            {
                if (FindOpenRow(field, column) >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool WinsAfterDrop(I4GWFeld field, int row, int column, int player)
        {
            int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };
            for (int direction = 0; direction < 4; direction++)
            {
                int rowStep = directions[direction, 0];
                int columnStep = directions[direction, 1];
                int count = 1
                    + CountDirection(field, row, column, rowStep, columnStep, player)
                    + CountDirection(field, row, column, -rowStep, -columnStep, player);
                if (count >= 4)
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool WinsAfterDrop(int[,] field, int row, int column, int player)
        {
            int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };
            for (int direction = 0; direction < 4; direction++)
            {
                int rowStep = directions[direction, 0];
                int columnStep = directions[direction, 1];
                int count = 1
                    + CountDirection(field, row, column, rowStep, columnStep, player)
                    + CountDirection(field, row, column, -rowStep, -columnStep, player);
                if (count >= 4)
                {
                    return true;
                }
            }

            return false;
        }

        internal static int FindOpenRow(int[,] field, int column)
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

        internal static int Search(int[,] field, int depth, int player, int computer,
            int lastRow, int lastColumn, int alpha, int beta)
        {
            int previousPlayer = player == 1 ? 2 : 1;
            if (lastRow >= 0 && WinsAfterDrop(field, lastRow, lastColumn, previousPlayer))
            {
                return previousPlayer == computer ? 100000 + depth : -100000 - depth;
            }

            if (depth == 0)
            {
                return Evaluate(field, computer);
            }

            bool maximizing = player == computer;
            int bestScore = maximizing ? int.MinValue : int.MaxValue;
            bool foundMove = false;
            for (int index = 0; index < PreferredColumns.Length; index++)
            {
                int column = PreferredColumns[index];
                int row = FindOpenRow(field, column);
                if (row < 0)
                {
                    continue;
                }

                foundMove = true;
                field[row, column] = player;
                int score = Search(field, depth - 1, previousPlayer, computer,
                    row, column, alpha, beta);
                field[row, column] = 0;

                if (maximizing)
                {
                    bestScore = Math.Max(bestScore, score);
                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    bestScore = Math.Min(bestScore, score);
                    beta = Math.Min(beta, bestScore);
                }

                if (beta <= alpha)
                {
                    break;
                }
            }

            return foundMove ? bestScore : 0;
        }

        private static int Evaluate(int[,] field, int computer)
        {
            int opponent = computer == 1 ? 2 : 1;
            int score = 0;

            for (int row = 0; row < Rows; row++)
            {
                if (field[row, Columns / 2] == computer)
                {
                    score += 6;
                }
                else if (field[row, Columns / 2] == opponent)
                {
                    score -= 6;
                }
            }

            int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    for (int direction = 0; direction < 4; direction++)
                    {
                        int rowStep = directions[direction, 0];
                        int columnStep = directions[direction, 1];
                        int endRow = row + 3 * rowStep;
                        int endColumn = column + 3 * columnStep;
                        if (!IsInside(endRow, endColumn))
                        {
                            continue;
                        }

                        int computerCount = 0;
                        int opponentCount = 0;
                        for (int offset = 0; offset < 4; offset++)
                        {
                            int cell = field[row + offset * rowStep, column + offset * columnStep];
                            if (cell == computer)
                            {
                                computerCount++;
                            }
                            else if (cell == opponent)
                            {
                                opponentCount++;
                            }
                        }

                        if (opponentCount == 0)
                        {
                            score += computerCount == 3 ? 120
                                : computerCount == 2 ? 18
                                : computerCount == 1 ? 2 : 0;
                        }
                        else if (computerCount == 0)
                        {
                            score -= opponentCount == 3 ? 140
                                : opponentCount == 2 ? 20
                                : opponentCount == 1 ? 2 : 0;
                        }
                    }
                }
            }

            return score;
        }

        private static bool HasLine(
            I4GWFeld field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player)
        {
            for (int index = 1; index < 4; index++)
            {
                int nextRow = row + rowStep * index;
                int nextColumn = column + columnStep * index;
                if (!IsInside(nextRow, nextColumn) ||
                    field[nextRow, nextColumn] != player)
                {
                    return false;
                }
            }

            return true;
        }

        private static int CountDirection(
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
            while (IsInside(nextRow, nextColumn) &&
                   field[nextRow, nextColumn] == player)
            {
                count++;
                nextRow += rowStep;
                nextColumn += columnStep;
            }

            return count;
        }

        private static int CountDirection(
            int[,] field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player)
        {
            int count = 0;
            int nextRow = row + rowStep;
            int nextColumn = column + columnStep;
            while (IsInside(nextRow, nextColumn) && field[nextRow, nextColumn] == player)
            {
                count++;
                nextRow += rowStep;
                nextColumn += columnStep;
            }

            return count;
        }

        private static bool IsInside(int row, int column)
        {
            return row >= 0 && row < Rows && column >= 0 && column < Columns;
        }
    }

    public sealed class G5_4GWFeld : IG5_4GWFeld
    {
        private readonly int[,] _cells = new int[G5_4GWLogic.Rows, G5_4GWLogic.Columns];

        public int this[int row, int column]
        {
            get
            {
                if (row < 0 || row >= G5_4GWLogic.Rows ||
                    column < 0 || column >= G5_4GWLogic.Columns)
                {
                    return -1;
                }

                return _cells[row, column];
            }
            set
            {
                if (row >= 0 && row < G5_4GWLogic.Rows &&
                    column >= 0 && column < G5_4GWLogic.Columns)
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

    public sealed class G5_4GWZug : IG5_4GWMove
    {
        public G5_4GWZug(int row, int column, int playerNumber)
        {
            Row = row;
            Column = column;
            PlayerNumber = playerNumber;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
        public int PlayerNumber { get; private set; }
    }

    public sealed class G5_4GWRules : IG5_4GWRules
    {
        private readonly IG5_4GWFeld _field = new G5_4GWFeld();
        private int _nextPlayer = 1;

        public string Name { get { return "Gruppe 5 Vier gewinnt Regeln"; } }
        public IGameField CurrentField { get { return _field; } }
        public IG5_4GWFeld VierGewinntFeld { get { return _field; } }

        public bool IsDraw
        {
            get
            {
                int winner;
                return !G5_4GWLogic.HasWinner(_field, out winner) &&
                    G5_4GWLogic.IsFull(_field);
            }
        }

        public bool MovesPossible
        {
            get
            {
                int winner;
                return !G5_4GWLogic.HasWinner(_field, out winner) &&
                    !G5_4GWLogic.IsFull(_field);
            }
        }

        public void ClearField()
        {
            for (int row = 0; row < G5_4GWLogic.Rows; row++)
            {
                for (int column = 0; column < G5_4GWLogic.Columns; column++)
                {
                    _field[row, column] = 0;
                }
            }

            _nextPlayer = 1;
        }

        public void DoMove(IPlayMove move)
        {
            I4GWMove fourWinsMove = move as I4GWMove;
            if (fourWinsMove == null ||
                fourWinsMove.PlayerNumber != _nextPlayer ||
                fourWinsMove.PlayerNumber < 1 ||
                fourWinsMove.PlayerNumber > 2 ||
                !MovesPossible)
            {
                return;
            }

            int row = G5_4GWLogic.FindOpenRow(_field, fourWinsMove.Column);
            if (row < 0)
            {
                return;
            }

            _field[row, fourWinsMove.Column] = fourWinsMove.PlayerNumber;
            _nextPlayer = _nextPlayer == 1 ? 2 : 1;
        }

        public int CheckIfPLayerWon()
        {
            int winner;
            return G5_4GWLogic.HasWinner(_field, out winner) ? winner : -1;
        }
    }

    public sealed class G5_4GWPainter : IG5_4GWPainter
    {
        public string Name { get { return "Gruppe 5 Vier gewinnt Darstellung"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            I4GWFeld field = currentField as I4GWFeld;
            if (canvas == null || field == null)
            {
                return;
            }

            canvas.Children.Clear();
            canvas.Background = Brushes.White;
            Rectangle board = new Rectangle
            {
                Width = G5_4GWLogic.CellSize * G5_4GWLogic.Columns,
                Height = G5_4GWLogic.CellSize * G5_4GWLogic.Rows,
                Fill = Brushes.RoyalBlue
            };
            Canvas.SetLeft(board, G5_4GWLogic.Left);
            Canvas.SetTop(board, G5_4GWLogic.Top);
            canvas.Children.Add(board);

            int winningRow;
            int winningColumn;
            int winningRowStep;
            int winningColumnStep;
            bool hasWinningLine = G5_4GWLogic.TryFindWinningLine(
                field, out winningRow, out winningColumn,
                out winningRowStep, out winningColumnStep);

            for (int row = 0; row < G5_4GWLogic.Rows; row++)
            {
                for (int column = 0; column < G5_4GWLogic.Columns; column++)
                {
                    int player = field[row, column];
                    bool isWinningToken = false;
                    if (hasWinningLine)
                    {
                        for (int offset = 0; offset < 4; offset++)
                        {
                            if (row == winningRow + offset * winningRowStep &&
                                column == winningColumn + offset * winningColumnStep)
                            {
                                isWinningToken = true;
                                break;
                            }
                        }
                    }

                    Ellipse token = new Ellipse
                    {
                        Width = G5_4GWLogic.CellSize - 10,
                        Height = G5_4GWLogic.CellSize - 10,
                        Fill = player == 1
                            ? Brushes.Red
                            : player == 2 ? Brushes.Yellow : Brushes.White,
                        Stroke = isWinningToken ? Brushes.Gold : Brushes.Navy,
                        StrokeThickness = isWinningToken ? 5 : 1
                    };
                    Canvas.SetLeft(token, G5_4GWLogic.Left + column * G5_4GWLogic.CellSize + 5);
                    Canvas.SetTop(token, G5_4GWLogic.Top + row * G5_4GWLogic.CellSize + 5);
                    canvas.Children.Add(token);
                }
            }

            AddColumnNumbers(canvas);
            AddComputerSpeedControl(canvas);
        }

        private static void AddColumnNumbers(Canvas canvas)
        {
            for (int column = 0; column < G5_4GWLogic.Columns; column++)
            {
                TextBlock number = new TextBlock
                {
                    Text = (column + 1).ToString(),
                    Width = G5_4GWLogic.CellSize,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(number, G5_4GWLogic.Left + column * G5_4GWLogic.CellSize);
                Canvas.SetTop(number, G5_4GWLogic.Top + G5_4GWLogic.Rows * G5_4GWLogic.CellSize);
                canvas.Children.Add(number);
            }
        }

        private static void AddComputerSpeedControl(Canvas canvas)
        {
            TextBlock label = new TextBlock
            {
                Text = "Computerzug-Abstand: " +
                    G5_4GWLogic.ComputerDelayMilliseconds + " ms",
                Width = G5_4GWLogic.CellSize * G5_4GWLogic.Columns,
                FontSize = 12,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(label, G5_4GWLogic.Left);
            Canvas.SetTop(label, G5_4GWLogic.Top +
                G5_4GWLogic.Rows * G5_4GWLogic.CellSize + 22);
            canvas.Children.Add(label);

            Slider delaySlider = new Slider
            {
                Minimum = 0,
                Maximum = 1500,
                Value = G5_4GWLogic.ComputerDelayMilliseconds,
                TickFrequency = 100,
                SmallChange = 100,
                LargeChange = 100,
                IsSnapToTickEnabled = true,
                Width = G5_4GWLogic.CellSize * G5_4GWLogic.Columns,
                Height = 22,
                ToolTip = "Verzögerung zwischen Computerzügen"
            };
            delaySlider.ValueChanged += (sender, args) =>
            {
                G5_4GWLogic.SetComputerDelay(args.NewValue);
                label.Text = "Computerzug-Abstand: " +
                    G5_4GWLogic.ComputerDelayMilliseconds + " ms";
            };
            delaySlider.AddHandler(
                UIElement.MouseDownEvent,
                new System.Windows.Input.MouseButtonEventHandler((sender, args) =>
                {
                    args.Handled = true;
                }),
                true);
            Canvas.SetLeft(delaySlider, G5_4GWLogic.Left);
            Canvas.SetTop(delaySlider, G5_4GWLogic.Top +
                G5_4GWLogic.Rows * G5_4GWLogic.CellSize + 42);
            canvas.Children.Add(delaySlider);
        }
    }

    public sealed class G5_4GWHumanPlayer : IG5_4GWHumanPlayer
    {
        private int _playerNumber;

        public string Name { get { return "Gruppe 5 Vier gewinnt Mensch"; } }
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
            G5_4GWHumanPlayer clone = new G5_4GWHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            I4GWFeld fourWinsField = field as I4GWFeld;
            if (fourWinsField == null ||
                _playerNumber < 1 || _playerNumber > 2 ||
                HasWinner(fourWinsField))
            {
                return null;
            }

            int column = GetSelectedColumn(selection);
            int row = G5_4GWLogic.FindOpenRow(fourWinsField, column);
            return row < 0 ? null : new G5_4GWZug(row, column, _playerNumber);
        }

        private static bool HasWinner(I4GWFeld field)
        {
            int winner;
            return G5_4GWLogic.HasWinner(field, out winner);
        }

        private static int GetSelectedColumn(IMoveSelection selection)
        {
            IClickSelection click = selection as IClickSelection;
            if (click != null)
            {
                double boardWidth = G5_4GWLogic.CellSize * G5_4GWLogic.Columns;
                double boardHeight = G5_4GWLogic.CellSize * G5_4GWLogic.Rows;
                if (click.XClickPos < G5_4GWLogic.Left ||
                    click.XClickPos >= G5_4GWLogic.Left + boardWidth ||
                    click.YClickPos < G5_4GWLogic.Top ||
                    click.YClickPos >= G5_4GWLogic.Top + boardHeight)
                {
                    return -1;
                }

                return (int)((click.XClickPos - G5_4GWLogic.Left) /
                    G5_4GWLogic.CellSize);
            }

            IKeySelection keySelection = selection as IKeySelection;
            if (keySelection == null)
            {
                return -1;
            }

            if (keySelection.Key >= Key.D1 && keySelection.Key <= Key.D7)
            {
                return (int)keySelection.Key - (int)Key.D1;
            }

            if (keySelection.Key >= Key.NumPad1 && keySelection.Key <= Key.NumPad7)
            {
                return (int)keySelection.Key - (int)Key.NumPad1;
            }

            return -1;
        }
    }

    public sealed class G5_4GWComputerPlayer : IG5_4GWComputerPlayer
    {
        private int _playerNumber;

        public string Name { get { return "Gruppe 5 Vier gewinnt Computer"; } }
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
            G5_4GWComputerPlayer clone = new G5_4GWComputerPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IGameField field)
        {
            I4GWFeld fourWinsField = field as I4GWFeld;
            if (fourWinsField == null ||
                _playerNumber < 1 || _playerNumber > 2)
            {
                return null;
            }

            G5_4GWLogic.WaitForComputerDelay();

            int winner;
            if (G5_4GWLogic.HasWinner(fourWinsField, out winner))
            {
                return null;
            }

            int[,] board = new int[G5_4GWLogic.Rows, G5_4GWLogic.Columns];
            for (int row = 0; row < G5_4GWLogic.Rows; row++)
            {
                for (int column = 0; column < G5_4GWLogic.Columns; column++)
                {
                    board[row, column] = fourWinsField[row, column];
                }
            }

            int opponent = _playerNumber == 1 ? 2 : 1;
            int bestColumn = -1;
            int bestRow = -1;
            int bestScore = int.MinValue;
            int alpha = int.MinValue;

            for (int columnIndex = 0; columnIndex < 7; columnIndex++)
            {
                int column = GetPreferredColumn(columnIndex);
                int row = G5_4GWLogic.FindOpenRow(board, column);
                if (row < 0)
                {
                    continue;
                }

                board[row, column] = _playerNumber;
                int score = G5_4GWLogic.Search(board, G5_4GWLogic.SearchDepth - 1,
                    opponent, _playerNumber, row, column, alpha, int.MaxValue);
                board[row, column] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumn = column;
                    bestRow = row;
                }

                alpha = Math.Max(alpha, bestScore);
            }

            return bestColumn < 0 ? null : new G5_4GWZug(bestRow, bestColumn, _playerNumber);
        }

        private static int GetPreferredColumn(int index)
        {
            switch (index)
            {
                case 0: return 3;
                case 1: return 2;
                case 2: return 4;
                case 3: return 1;
                case 4: return 5;
                case 5: return 0;
                default: return 6;
            }
        }
    }
}
