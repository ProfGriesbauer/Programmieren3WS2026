using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    internal static class G1_VierGewinntLogic
    {
        internal const int Rows = 6;
        internal const int Columns = 7;
        internal static readonly int[] RowSteps = { 0, 1, 1, 1 };
        internal static readonly int[] ColumnSteps = { 1, 0, 1, -1 };

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

        internal static int Winner(I4GWFeld field)
        {
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
                        int rowStep = RowSteps[direction];
                        int columnStep = ColumnSteps[direction];
                        while (count < 4 &&
                               IsInside(row + count * rowStep, column + count * columnStep) &&
                               field[row + count * rowStep, column + count * columnStep] == player)
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

        internal static bool WinsAt(int[,] board, int row, int column, int player)
        {
            for (int direction = 0; direction < 4; direction++)
            {
                int count = 1;
                int rowStep = RowSteps[direction];
                int columnStep = ColumnSteps[direction];
                count += Count(board, row, column, rowStep, columnStep, player);
                count += Count(board, row, column, -rowStep, -columnStep, player);
                if (count >= 4)
                {
                    return true;
                }
            }

            return false;
        }

        private static int Count(int[,] board, int row, int column, int rowStep, int columnStep, int player)
        {
            int count = 0;
            int currentRow = row + rowStep;
            int currentColumn = column + columnStep;
            while (IsInside(currentRow, currentColumn) && board[currentRow, currentColumn] == player)
            {
                count++;
                currentRow += rowStep;
                currentColumn += columnStep;
            }

            return count;
        }
    }

    internal static class G1_VierGewinntLayout
    {
        internal static double Left = 20;
        internal static double Top = 80;
        internal static double Cell = 50;

        internal static void CenterOn(Canvas canvas)
        {
            double canvasWidth = canvas.ActualWidth;
            double canvasHeight = canvas.ActualHeight;

            if (canvasWidth > 0 && canvasHeight > 0)
            {
                double availableWidth = Math.Max(0, canvasWidth - 24);
                double availableHeight = Math.Max(0, canvasHeight - 95);
                Cell = Math.Min(50, Math.Min(
                    availableWidth / G1_VierGewinntLogic.Columns,
                    availableHeight / G1_VierGewinntLogic.Rows));
            }
            else
            {
                Cell = 50;
            }

            double boardWidth = G1_VierGewinntLogic.Columns * Cell;
            double boardHeight = G1_VierGewinntLogic.Rows * Cell;
            Left = canvasWidth > boardWidth ? (canvasWidth - boardWidth) / 2 : 20;

            double freeHeight = canvasHeight - 65 - boardHeight - 30;
            Top = freeHeight > 0 ? 65 + freeHeight / 2 + 30 : 80;
        }

        internal static int ColumnAt(int x, int y)
        {
            if (x < Left || y < Top - 32 ||
                x >= Left + G1_VierGewinntLogic.Columns * Cell ||
                y >= Top + G1_VierGewinntLogic.Rows * Cell)
            {
                return -1;
            }

            return (int)((x - Left) / Cell);
        }
    }

    public sealed class G1_VierGewinntField : I4GWFeld
    {
        private readonly int[,] _cells = new int[G1_VierGewinntLogic.Rows, G1_VierGewinntLogic.Columns];
        internal int NextPlayer { get; set; } = 1;

        public int this[int row, int column]
        {
            get
            {
                return G1_VierGewinntLogic.IsInside(row, column) ? _cells[row, column] : -1;
            }
            set
            {
                if (G1_VierGewinntLogic.IsInside(row, column))
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

    public sealed class G1_VierGewinntMove : I4GWMove
    {
        public G1_VierGewinntMove(int row, int column, int playerNumber)
        {
            Row = row;
            Column = column;
            PlayerNumber = playerNumber;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
        public int PlayerNumber { get; private set; }
    }

    public sealed class G1_VierGewinntRules : IGameRules
    {
        private G1_VierGewinntField _field = new G1_VierGewinntField();
        private int _nextPlayer = 1;

        public string Name { get { return "Gruppe 1 Vier Gewinnt Regeln"; } }
        public IGameField CurrentField { get { return _field; } }

        public bool MovesPossible
        {
            get
            {
                return G1_VierGewinntLogic.Winner(_field) < 0 &&
                       !G1_VierGewinntLogic.IsFull(_field);
            }
        }

        public void ClearField()
        {
            _field = new G1_VierGewinntField();
            _nextPlayer = 1;
        }

        public void DoMove(IPlayMove move)
        {
            I4GWMove connectFourMove = move as I4GWMove;
            if (connectFourMove == null || !MovesPossible ||
                connectFourMove.PlayerNumber != _nextPlayer ||
                connectFourMove.PlayerNumber < 1 || connectFourMove.PlayerNumber > 2)
            {
                return;
            }

            int row = G1_VierGewinntLogic.FreeRow(_field, connectFourMove.Column);
            if (row < 0 || connectFourMove.Row != row)
            {
                return;
            }

            _field[row, connectFourMove.Column] = connectFourMove.PlayerNumber;
            _nextPlayer = _nextPlayer == 1 ? 2 : 1;
            _field.NextPlayer = _nextPlayer;
        }

        public int CheckIfPLayerWon()
        {
            return G1_VierGewinntLogic.Winner(_field);
        }
    }

    public sealed class G1_VierGewinntPaint : IPaintGame
    {
        private static readonly Brush BackgroundBrush = new SolidColorBrush(Color.FromRgb(0x03, 0x04, 0x5E));
        private static readonly Brush FrameBrush = new SolidColorBrush(Color.FromRgb(0x77, 0x8D, 0xA9));
        private static readonly Brush PlayerOneBrush = new SolidColorBrush(Color.FromRgb(0xFC, 0xA3, 0x11));
        private static readonly Brush PlayerTwoBrush = new SolidColorBrush(Color.FromRgb(0xD0, 0x00, 0x00));
        private Canvas _canvas;
        private IGameField _currentField;

        public string Name { get { return "Gruppe 1 Vier Gewinnt Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (!ReferenceEquals(canvas, _canvas))
            {
                if (_canvas != null)
                {
                    _canvas.SizeChanged -= Canvas_SizeChanged;
                }

                _canvas = canvas;
                _canvas.SizeChanged += Canvas_SizeChanged;
            }

            _currentField = currentField;
            I4GWFeld field = currentField as I4GWFeld;
            canvas.Children.Clear();
            canvas.Background = BackgroundBrush;
            if (field == null)
            {
                return;
            }

            G1_VierGewinntLayout.CenterOn(canvas);
            string inputHint = G1_VierGewinntLayout.Cell < 32
                ? "1-7 / Klick"
                : "Spalte anklicken / Taste 1-7";
            AddText(canvas, inputHint, 20, 8,
                Math.Max(8, Math.Min(12, G1_VierGewinntLayout.Cell * 0.24)), Brushes.White);
            int winner = G1_VierGewinntLogic.Winner(field);
            bool draw = winner < 0 && G1_VierGewinntLogic.IsFull(field);
            if (winner > 0)
            {
                AddText(canvas, "Spieler " + winner + " hat gewonnen",
                    G1_VierGewinntLayout.Left, 30, 14,
                    winner == 1 ? PlayerOneBrush : PlayerTwoBrush);
            }
            else if (draw)
            {
                AddText(canvas, "Unentschieden",
                    G1_VierGewinntLayout.Left, 30, 14, Brushes.White);
            }
            else
            {
                int currentPlayer = field is G1_VierGewinntField
                    ? ((G1_VierGewinntField)field).NextPlayer
                    : 1;
                Brush playerBrush = currentPlayer == 1 ? PlayerOneBrush : PlayerTwoBrush;
                bool hasCanvasSize = canvas.ActualWidth > 0;
                double indicatorSize = hasCanvasSize
                    ? Math.Min(38, G1_VierGewinntLayout.Cell * 0.76)
                    : 38;
                double labelLeft = hasCanvasSize
                    ? Math.Max(8, canvas.ActualWidth - indicatorSize - 62)
                    : 278;
                double indicatorLeft = hasCanvasSize ? labelLeft + 51 : 329;
                double indicatorTop = hasCanvasSize ? 2 + (38 - indicatorSize) / 2 : 2;
                double numberFontSize = hasCanvasSize
                    ? Math.Min(24, indicatorSize * 0.63)
                    : 24;
                AddText(canvas, "Spieler", labelLeft, 5,
                    hasCanvasSize
                        ? Math.Max(9, Math.Min(14, G1_VierGewinntLayout.Cell * 0.28))
                        : 11,
                    Brushes.White);
                Ellipse playerIndicator = new Ellipse
                {
                    Width = indicatorSize,
                    Height = indicatorSize,
                    Fill = playerBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = hasCanvasSize ? Math.Max(1, indicatorSize * 0.05) : 2
                };
                Canvas.SetLeft(playerIndicator, indicatorLeft);
                Canvas.SetTop(playerIndicator, indicatorTop);
                canvas.Children.Add(playerIndicator);
                AddText(canvas, currentPlayer.ToString(),
                    hasCanvasSize
                        ? indicatorLeft + (indicatorSize - numberFontSize * 0.55) / 2
                        : 340,
                    hasCanvasSize ? indicatorTop + indicatorSize * 0.12 : 5,
                    numberFontSize, Brushes.White);
            }

            for (int column = 0; column < G1_VierGewinntLogic.Columns; column++)
            {
                TextBlock columnNumber = new TextBlock
                {
                    Text = (column + 1).ToString(),
                    Width = G1_VierGewinntLayout.Cell,
                    TextAlignment = TextAlignment.Center,
                    FontSize = Math.Max(8, Math.Min(16, G1_VierGewinntLayout.Cell * 0.32)),
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brushes.White
                };
                Canvas.SetLeft(columnNumber,
                    G1_VierGewinntLayout.Left + column * G1_VierGewinntLayout.Cell);
                Canvas.SetTop(columnNumber, G1_VierGewinntLayout.Top - 27);
                canvas.Children.Add(columnNumber);
            }

            Rectangle board = new Rectangle
            {
                Width = G1_VierGewinntLogic.Columns * G1_VierGewinntLayout.Cell,
                Height = G1_VierGewinntLogic.Rows * G1_VierGewinntLayout.Cell,
                Fill = FrameBrush,
                Stroke = FrameBrush,
                StrokeThickness = 2
            };
            Canvas.SetLeft(board, G1_VierGewinntLayout.Left);
            Canvas.SetTop(board, G1_VierGewinntLayout.Top);
            canvas.Children.Add(board);

            for (int row = 0; row < G1_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G1_VierGewinntLogic.Columns; column++)
                {
                    int player = field[row, column];
                    Brush fill = player == 1 ? PlayerOneBrush
                        : player == 2 ? PlayerTwoBrush
                        : BackgroundBrush;
                    double discInset = G1_VierGewinntLayout.Cell * 0.1;
                    Ellipse disc = new Ellipse
                    {
                        Width = G1_VierGewinntLayout.Cell - 2 * discInset,
                        Height = G1_VierGewinntLayout.Cell - 2 * discInset,
                        Fill = fill,
                        Stroke = Brushes.DarkSlateGray,
                        StrokeThickness = Math.Max(0.5, G1_VierGewinntLayout.Cell * 0.02)
                    };
                    Canvas.SetLeft(disc, G1_VierGewinntLayout.Left
                        + column * G1_VierGewinntLayout.Cell + discInset);
                    Canvas.SetTop(disc, G1_VierGewinntLayout.Top
                        + row * G1_VierGewinntLayout.Cell + discInset);
                    canvas.Children.Add(disc);
                }
            }

        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas canvas = sender as Canvas;
            if (canvas != null && _currentField != null)
            {
                PaintGameField(canvas, _currentField);
            }
        }

        private static void AddText(Canvas canvas, string text, double left, double top,
            double fontSize, Brush foreground)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                Foreground = foreground,
                FontWeight = FontWeights.SemiBold
            };
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            canvas.Children.Add(label);
        }
    }

    public sealed class G1_VierGewinntHumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber;

        public string Name { get { return "Gruppe 1 Vier Gewinnt Human Player"; } }
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
            G1_VierGewinntHumanPlayer clone = new G1_VierGewinntHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            I4GWFeld connectFourField = field as I4GWFeld;
            if (connectFourField == null || _playerNumber < 1 || _playerNumber > 2 ||
                G1_VierGewinntLogic.Winner(connectFourField) > 0 ||
                G1_VierGewinntLogic.IsFull(connectFourField))
            {
                return null;
            }

            int column = -1;
            IClickSelection click = selection as IClickSelection;
            if (click != null)
            {
                column = G1_VierGewinntLayout.ColumnAt(click.XClickPos, click.YClickPos);
            }
            else
            {
                IKeySelection keySelection = selection as IKeySelection;
                if (keySelection != null)
                {
                    column = ColumnFromKey(keySelection.Key);
                }
            }

            int row = G1_VierGewinntLogic.FreeRow(connectFourField, column);
            return row >= 0 ? new G1_VierGewinntMove(row, column, _playerNumber) : null;
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

    public sealed class G1_VierGewinntComputerPlayer : IComputerGamePlayer
    {
        private const int MaximumDepth = 12;
        private const int MaximumNodes = 500000;
        private const int MaximumSearchMilliseconds = 850;
        private const int WinScore = 100000;
        private const int QuiescenceDepth = 6;
        private static readonly int[] OrderedColumns = { 3, 2, 4, 1, 5, 0, 6 };
        private readonly Dictionary<SearchKey, SearchEntry> _transpositions =
            new Dictionary<SearchKey, SearchEntry>();
        private int _playerNumber;
        private int _nodes;
        private bool _searchAborted;
        private Stopwatch _searchTimer;

        public string Name { get { return "Gruppe 1 Vier Gewinnt Alpha-Beta-KI"; } }
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
            G1_VierGewinntComputerPlayer clone = new G1_VierGewinntComputerPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IGameField field)
        {
            I4GWFeld connectFourField = field as I4GWFeld;
            if (connectFourField == null || _playerNumber < 1 || _playerNumber > 2 ||
                G1_VierGewinntLogic.Winner(connectFourField) > 0 ||
                G1_VierGewinntLogic.IsFull(connectFourField))
            {
                return null;
            }

            int[,] board = new int[G1_VierGewinntLogic.Rows, G1_VierGewinntLogic.Columns];
            int[] heights = new int[G1_VierGewinntLogic.Columns];
            int occupied = 0;
            for (int row = 0; row < G1_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G1_VierGewinntLogic.Columns; column++)
                {
                    board[row, column] = connectFourField[row, column];
                    if (board[row, column] != 0)
                    {
                        occupied++;
                    }
                }
            }

            for (int column = 0; column < G1_VierGewinntLogic.Columns; column++)
            {
                heights[column] = G1_VierGewinntLogic.FreeRow(connectFourField, column);
            }

            int bestColumn = -1;
            int opponent = _playerNumber == 1 ? 2 : 1;
            int opponentThreats = 0;
            int threatColumn = -1;
            for (int columnIndex = 0; columnIndex < OrderedColumns.Length; columnIndex++)
            {
                int column = OrderedColumns[columnIndex];
                int row = heights[column];
                if (row >= 0)
                {
                    board[row, column] = _playerNumber;
                    bool wins = G1_VierGewinntLogic.WinsAt(board, row, column, _playerNumber);
                    board[row, column] = 0;
                    if (wins)
                    {
                        return CreateMove(heights, column);
                    }
                }
            }

            for (int columnIndex = 0; columnIndex < OrderedColumns.Length; columnIndex++)
            {
                int column = OrderedColumns[columnIndex];
                int row = heights[column];
                if (row >= 0)
                {
                    board[row, column] = opponent;
                    bool opponentWins = G1_VierGewinntLogic.WinsAt(board, row, column, opponent);
                    board[row, column] = 0;
                    if (opponentWins)
                    {
                        opponentThreats++;
                        threatColumn = column;
                    }
                }
            }

            if (opponentThreats == 1)
            {
                return CreateMove(heights, threatColumn);
            }

            if (bestColumn < 0)
            {
                bestColumn = OrderedColumns[0];
            }

            _nodes = 0;
            _searchAborted = false;
            _searchTimer = Stopwatch.StartNew();
            _transpositions.Clear();
            int depthLimit = Math.Min(MaximumDepth, G1_VierGewinntLogic.Rows *
                G1_VierGewinntLogic.Columns - occupied);

            for (int depth = 1; depth <= depthLimit; depth++)
            {
                int candidate = FindBestMove(board, heights, depth);
                if (_searchAborted)
                {
                    break;
                }

                bestColumn = candidate;
            }

            return bestColumn >= 0 ? CreateMove(heights, bestColumn) : null;
        }

        private int FindBestMove(int[,] board, int[] heights, int depth)
        {
            int bestColumn = -1;
            int bestScore = int.MinValue;
            int alpha = -WinScore * 2;
            int beta = WinScore * 2;

            for (int index = 0; index < OrderedColumns.Length; index++)
            {
                int column = OrderedColumns[index];
                int row = heights[column];
                if (row < 0)
                {
                    continue;
                }

                board[row, column] = _playerNumber;
                heights[column]--;
                int score = G1_VierGewinntLogic.WinsAt(board, row, column, _playerNumber)
                    ? GetWinScore(board)
                    : -Search(board, heights, _playerNumber == 1 ? 2 : 1, depth - 1, -beta, -alpha);
                heights[column]++;
                board[row, column] = 0;

                if (_searchAborted)
                {
                    break;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumn = column;
                }

                if (score > alpha)
                {
                    alpha = score;
                }
            }

            return bestColumn;
        }

        private int Search(int[,] board, int[] heights, int player, int depth, int alpha, int beta)
        {
            if (!VisitSearchNode())
            {
                return Evaluate(board, heights, player);
            }

            if (depth <= 0)
            {
                return Quiescence(board, heights, player, QuiescenceDepth);
            }

            int originalAlpha = alpha;
            int originalBeta = beta;
            SearchKey key = CreateKey(board, player);
            int preferredColumn = -1;

            SearchEntry cached;
            if (_transpositions.TryGetValue(key, out cached))
            {
                preferredColumn = cached.BestColumn;
                if (cached.Depth >= depth)
                {
                    if (cached.Bound == SearchBound.Exact)
                    {
                        return cached.Score;
                    }

                    if (cached.Bound == SearchBound.Lower)
                    {
                        alpha = Math.Max(alpha, cached.Score);
                    }
                    else
                    {
                        beta = Math.Min(beta, cached.Score);
                    }

                    if (alpha >= beta)
                    {
                        return cached.Score;
                    }
                }
            }

            bool hasMove = false;
            int bestScore = int.MinValue;
            int bestColumn = -1;
            for (int index = 0; index < OrderedColumns.Length; index++)
            {
                int column = SelectColumn(index, preferredColumn);
                int row = heights[column];
                if (row < 0)
                {
                    continue;
                }

                hasMove = true;
                board[row, column] = player;
                heights[column]--;
                int score = G1_VierGewinntLogic.WinsAt(board, row, column, player)
                    ? GetWinScore(board)
                    : -Search(board, heights, player == 1 ? 2 : 1, depth - 1, -beta, -alpha);
                heights[column]++;
                board[row, column] = 0;

                if (_searchAborted)
                {
                    return 0;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumn = column;
                }

                if (score > alpha)
                {
                    alpha = score;
                }

                if (alpha >= beta)
                {
                    break;
                }
            }

            if (!hasMove)
            {
                return 0;
            }

            SearchBound bound = bestScore <= originalAlpha ? SearchBound.Upper
                : bestScore >= originalBeta ? SearchBound.Lower
                : SearchBound.Exact;
            _transpositions[key] = new SearchEntry(depth, bestScore, bound, bestColumn);
            return bestScore;
        }

        private int Quiescence(int[,] board, int[] heights, int player, int remainingDepth)
        {
            if (!VisitSearchNode())
            {
                return Evaluate(board, heights, player);
            }

            int opponent = player == 1 ? 2 : 1;
            int opponentThreats = 0;
            int threatColumn = -1;
            bool hasLegalMove = false;

            for (int index = 0; index < OrderedColumns.Length; index++)
            {
                int column = OrderedColumns[index];
                int row = heights[column];
                if (row < 0)
                {
                    continue;
                }

                hasLegalMove = true;
                board[row, column] = player;
                bool wins = G1_VierGewinntLogic.WinsAt(board, row, column, player);
                int winScore = wins ? GetWinScore(board) : 0;
                board[row, column] = 0;
                if (wins)
                {
                    return winScore;
                }

                board[row, column] = opponent;
                bool opponentWins = G1_VierGewinntLogic.WinsAt(board, row, column, opponent);
                board[row, column] = 0;
                if (opponentWins)
                {
                    opponentThreats++;
                    threatColumn = column;
                }
            }

            if (!hasLegalMove)
            {
                return 0;
            }

            if (opponentThreats == 0)
            {
                return Evaluate(board, heights, player);
            }

            if (opponentThreats > 1 || remainingDepth <= 0)
            {
                return -WinScore;
            }

            int forcedRow = heights[threatColumn];
            board[forcedRow, threatColumn] = player;
            heights[threatColumn]--;
            int score = -Quiescence(board, heights, opponent, remainingDepth - 1);
            heights[threatColumn]++;
            board[forcedRow, threatColumn] = 0;
            return score;
        }

        private static int GetWinScore(int[,] board)
        {
            int occupied = 0;
            for (int row = 0; row < G1_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G1_VierGewinntLogic.Columns; column++)
                {
                    if (board[row, column] != 0)
                    {
                        occupied++;
                    }
                }
            }

            return WinScore + G1_VierGewinntLogic.Rows *
                G1_VierGewinntLogic.Columns - occupied;
        }

        private bool VisitSearchNode()
        {
            if (_searchAborted)
            {
                return false;
            }

            if (_nodes++ >= MaximumNodes)
            {
                _searchAborted = true;
                return false;
            }

            if ((_nodes & 255) == 0)
            {
                if (_searchTimer.ElapsedMilliseconds >= MaximumSearchMilliseconds)
                {
                    _searchAborted = true;
                    return false;
                }
            }

            return true;
        }

        private static int SelectColumn(int index, int preferredColumn)
        {
            if (preferredColumn < 0)
            {
                return OrderedColumns[index];
            }

            if (index == 0)
            {
                return preferredColumn;
            }

            int orderIndex = 0;
            int selectedColumn = -1;
            for (int selection = 1; selection <= index; selection++)
            {
                while (OrderedColumns[orderIndex] == preferredColumn)
                {
                    orderIndex++;
                }

                selectedColumn = OrderedColumns[orderIndex++];
            }

            return selectedColumn;
        }

        private static int Evaluate(int[,] board, int[] heights, int player)
        {
            int opponent = player == 1 ? 2 : 1;
            int score = 0;

            for (int row = 0; row < G1_VierGewinntLogic.Rows; row++)
            {
                if (board[row, G1_VierGewinntLogic.Columns / 2] == player)
                {
                    score += 4;
                }
                else if (board[row, G1_VierGewinntLogic.Columns / 2] == opponent)
                {
                    score -= 4;
                }
            }

            for (int row = 0; row < G1_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G1_VierGewinntLogic.Columns; column++)
                {
                    for (int direction = 0; direction < 4; direction++)
                    {
                        int rowStep = G1_VierGewinntLogic.RowSteps[direction];
                        int columnStep = G1_VierGewinntLogic.ColumnSteps[direction];
                        int endRow = row + 3 * rowStep;
                        int endColumn = column + 3 * columnStep;
                        if (!G1_VierGewinntLogic.IsInside(endRow, endColumn))
                        {
                            continue;
                        }

                        int own = 0;
                        int other = 0;
                        int emptyRow = -1;
                        int emptyColumn = -1;
                        for (int offset = 0; offset < 4; offset++)
                        {
                            int windowRow = row + offset * rowStep;
                            int windowColumn = column + offset * columnStep;
                            int cell = board[windowRow, windowColumn];
                            if (cell == player)
                            {
                                own++;
                            }
                            else if (cell == opponent)
                            {
                                other++;
                            }
                            else
                            {
                                emptyRow = windowRow;
                                emptyColumn = windowColumn;
                            }
                        }

                        if (own > 0 && other > 0)
                        {
                            continue;
                        }

                        if (own > 0)
                        {
                            score += own == 3 ? 100 : own == 2 ? 10 : 1;
                            if (own == 3 && emptyColumn >= 0)
                            {
                                score += emptyRow == heights[emptyColumn] ? 250 : 0;
                            }
                        }
                        else if (other > 0)
                        {
                            score -= other == 3 ? 120 : other == 2 ? 12 : 1;
                            if (other == 3 && emptyColumn >= 0)
                            {
                                score -= emptyRow == heights[emptyColumn] ? 300 : 0;
                            }
                        }
                    }
                }
            }

            return score;
        }

        private static SearchKey CreateKey(int[,] board, int player)
        {
            ulong playerOne = 0;
            ulong playerTwo = 0;
            for (int row = 0; row < G1_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G1_VierGewinntLogic.Columns; column++)
                {
                    ulong bit = 1UL << (row * G1_VierGewinntLogic.Columns + column);
                    if (board[row, column] == 1)
                    {
                        playerOne |= bit;
                    }
                    else if (board[row, column] == 2)
                    {
                        playerTwo |= bit;
                    }
                }
            }

            return new SearchKey(playerOne, playerTwo, player);
        }

        private G1_VierGewinntMove CreateMove(int[] heights, int column)
        {
            return new G1_VierGewinntMove(heights[column], column, _playerNumber);
        }

        private enum SearchBound
        {
            Exact,
            Lower,
            Upper
        }

        private struct SearchEntry
        {
            internal SearchEntry(int depth, int score, SearchBound bound, int bestColumn)
            {
                Depth = depth;
                Score = score;
                Bound = bound;
                BestColumn = bestColumn;
            }

            internal int Depth;
            internal int Score;
            internal SearchBound Bound;
            internal int BestColumn;
        }

        private struct SearchKey : IEquatable<SearchKey>
        {
            internal SearchKey(ulong playerOne, ulong playerTwo, int playerToMove)
            {
                PlayerOne = playerOne;
                PlayerTwo = playerTwo;
                PlayerToMove = playerToMove;
            }

            private ulong PlayerOne;
            private ulong PlayerTwo;
            private int PlayerToMove;

            public bool Equals(SearchKey other)
            {
                return PlayerOne == other.PlayerOne && PlayerTwo == other.PlayerTwo &&
                       PlayerToMove == other.PlayerToMove;
            }

            public override bool Equals(object obj)
            {
                return obj is SearchKey && Equals((SearchKey)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(PlayerOne, PlayerTwo, PlayerToMove);
            }
        }
    }
}
