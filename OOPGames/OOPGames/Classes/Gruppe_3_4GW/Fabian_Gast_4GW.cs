using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OOPGames
{
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

    public sealed class FG_ConnectFourMove : IFG_ConnectFourMove
    {
        public FG_ConnectFourMove(int row, int column, int playerNumber)
        {
            Row = row;
            Column = column;
            PlayerNumber = playerNumber;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
        public int PlayerNumber { get; private set; }
    }

    public sealed class FG_ConnectFourRules : IFG_ConnectFourRules
    {
        private readonly FG_ConnectFourField _field = new FG_ConnectFourField();

        public string Name { get { return "FG Vier gewinnt Regeln"; } }
        public IGameField CurrentField { get { return _field; } }
        public IFG_ConnectFourField ConnectFourField { get { return _field; } }

        public bool MovesPossible
        {
            get
            {
                if (CheckIfPLayerWon() > 0)
                {
                    return false;
                }

                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    if (_field[0, column] == 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void ClearField()
        {
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    _field[row, column] = 0;
                }
            }
        }

        public void DoMove(IPlayMove move)
        {
            I4GWMove gameMove = move as I4GWMove;
            if (gameMove == null)
            {
                return;
            }

            if (gameMove.PlayerNumber < 1 ||
                gameMove.PlayerNumber > 2 ||
                gameMove.Column < 0 ||
                gameMove.Column >= FG_ConnectFourField.Columns ||
                CheckIfPLayerWon() > 0)
            {
                return;
            }

            for (int row = FG_ConnectFourField.Rows - 1; row >= 0; row--)
            {
                if (_field[row, gameMove.Column] == 0)
                {
                    _field[row, gameMove.Column] = gameMove.PlayerNumber;
                    return;
                }
            }
        }

        public void DoConnectFourMove(IFG_ConnectFourMove move)
        {
            DoMove(move);
        }

        public int CheckIfPLayerWon()
        {
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    int player = _field[row, column];
                    if (player > 0 &&
                        (HasLine(row, column, 0, 1, player) ||
                         HasLine(row, column, 1, 0, player) ||
                         HasLine(row, column, 1, 1, player) ||
                         HasLine(row, column, 1, -1, player)))
                    {
                        return player;
                    }
                }
            }

            return -1;
        }

        private bool HasLine(int row, int column, int rowStep, int columnStep, int player)
        {
            for (int index = 1; index < 4; index++)
            {
                int nextRow = row + rowStep * index;
                int nextColumn = column + columnStep * index;
                if (nextRow < 0 ||
                    nextRow >= FG_ConnectFourField.Rows ||
                    nextColumn < 0 ||
                    nextColumn >= FG_ConnectFourField.Columns ||
                    _field[nextRow, nextColumn] != player)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public sealed class FG_ConnectFourPaint : IPaintGame2, IFG_ConnectFourPaint
    {
        private sealed class BoardMove
        {
            public BoardMove(int row, int column, int player)
            {
                Row = row;
                Column = column;
                Player = player;
            }

            public int Row { get; private set; }
            public int Column { get; private set; }
            public int Player { get; private set; }
        }

        private const double Padding = 10;
        private const double BoardBorderWidth = 8;
        private const double AnimationDurationMilliseconds = 350;
        private static double _cellSize = 65;
        private static double _left = Padding;
        private static double _top = Padding;
        private readonly int[,] _displayedBoard =
            new int[FG_ConnectFourField.Rows, FG_ConnectFourField.Columns];
        private int[,] _previousBoard;
        private readonly Queue<BoardMove> _pendingMoves = new Queue<BoardMove>();
        private bool _isAnimating;
        private int _fallingRow;
        private int _fallingColumn;
        private int _fallingPlayer;
        private DateTime _animationStarted;
        private readonly DispatcherTimer _animationTimer;
        private Canvas _animationCanvas;
        private I4GWFeld _animationField;

        public FG_ConnectFourPaint()
        {
            _animationTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            _animationTimer.Tick += AnimationTimer_Tick;
        }

        public static double CellSize { get { return _cellSize; } }
        public static double Left { get { return _left; } }
        public static bool IsAnyAnimating { get; private set; }
        public string Name { get { return "FG Vier gewinnt Darstellung"; } }

        public int ColumnAt(int x, int y)
        {
            if (x < _left ||
                x >= _left + _cellSize * FG_ConnectFourField.Columns ||
                y < _top ||
                y >= _top + _cellSize * FG_ConnectFourField.Rows)
            {
                return -1;
            }

            return (int)((x - _left) / _cellSize);
        }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            I4GWFeld field = currentField as I4GWFeld;
            if (field == null)
            {
                return;
            }

            canvas.Background = Brushes.White;
            UpdateBoardGeometry(canvas);
            _animationCanvas = canvas;
            _animationField = field;
            StartAnimationIfNeeded(field);
            DrawField(canvas, field);
        }

        public void PaintConnectFourField(Canvas canvas, IFG_ConnectFourField currentField)
        {
            PaintGameField(canvas, currentField);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            if (!_isAnimating)
            {
                PaintGameField(canvas, currentField);
            }
        }

        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            if (_animationCanvas == null || _animationField == null)
            {
                _animationTimer.Stop();
                _isAnimating = false;
                IsAnyAnimating = false;
                return;
            }

            UpdateBoardGeometry(_animationCanvas);
            DrawField(_animationCanvas, _animationField);
        }

        private void DrawField(Canvas canvas, I4GWFeld field)
        {
            canvas.Children.Clear();
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    if (_isAnimating &&
                        row == _fallingRow &&
                        column == _fallingColumn)
                    {
                        continue;
                    }

                    int player = _displayedBoard[row, column];
                    AddToken(canvas, player, row, column);
                }
            }

            if (_isAnimating)
            {
                double elapsed = (DateTime.UtcNow - _animationStarted).TotalMilliseconds;
                double progress = Math.Min(1, elapsed / AnimationDurationMilliseconds);
                double easedProgress = 1 - Math.Pow(1 - progress, 2);
                double startTop = _top - _cellSize + 4;
                double targetTop = _top + _fallingRow * _cellSize + 4;
                AddToken(canvas, _fallingPlayer, _fallingRow, _fallingColumn,
                    startTop + (targetTop - startTop) * easedProgress);

                if (progress >= 1)
                {
                    _displayedBoard[_fallingRow, _fallingColumn] = _fallingPlayer;
                    _isAnimating = false;
                    IsAnyAnimating = false;
                    _animationTimer.Stop();
                    StartAnimationIfNeeded(field);
                }
            }

            AddBoardInFront(canvas);
            DrawWinningLine(canvas);
        }

        private void AddToken(
            Canvas canvas,
            int player,
            int row,
            int column,
            double? top = null)
        {
            if (player <= 0)
            {
                return;
            }

            Brush tokenBrush = player == 1
                ? new SolidColorBrush(Color.FromRgb(220, 40, 40))
                : player == 2
                    ? new SolidColorBrush(Color.FromRgb(245, 215, 35))
                    : Brushes.White;
            Ellipse token = new Ellipse
            {
                Width = Math.Max(1, _cellSize - 8),
                Height = Math.Max(1, _cellSize - 8),
                Fill = tokenBrush,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(token, _left + column * _cellSize + 4);
            Canvas.SetTop(token, top ?? _top + row * _cellSize + 4);
            canvas.Children.Add(token);
        }

        private void AddBoardInFront(Canvas canvas)
        {
            GeometryGroup boardGeometry = new GeometryGroup
            {
                FillRule = FillRule.EvenOdd
            };
            boardGeometry.Children.Add(new RectangleGeometry(new Rect(
                _left - BoardBorderWidth,
                _top - BoardBorderWidth,
                _cellSize * FG_ConnectFourField.Columns + BoardBorderWidth * 2,
                _cellSize * FG_ConnectFourField.Rows + BoardBorderWidth * 2)));

            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    boardGeometry.Children.Add(new EllipseGeometry(
                        new Point(
                            _left + column * _cellSize + _cellSize / 2,
                            _top + row * _cellSize + _cellSize / 2),
                        Math.Max(1, _cellSize / 2 - 4),
                        Math.Max(1, _cellSize / 2 - 4)));
                }
            }

            Path board = new Path
            {
                Data = boardGeometry,
                Fill = new SolidColorBrush(Color.FromRgb(30, 100, 190)),
                Stroke = new SolidColorBrush(Color.FromRgb(20, 70, 140)),
                StrokeThickness = 2
            };
            canvas.Children.Add(board);
        }

        private void DrawWinningLine(Canvas canvas)
        {
            int[,] field = _displayedBoard;
            int[,] directions =
            {
                { 0, 1 },
                { 1, 0 },
                { 1, 1 },
                { 1, -1 }
            };

            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    int player = field[row, column];
                    if (player <= 0)
                    {
                        continue;
                    }

                    for (int direction = 0; direction < 4; direction++)
                    {
                        int rowStep = directions[direction, 0];
                        int columnStep = directions[direction, 1];
                        int endRow = row + rowStep * 3;
                        int endColumn = column + columnStep * 3;
                        if (endRow < 0 ||
                            endRow >= FG_ConnectFourField.Rows ||
                            endColumn < 0 ||
                            endColumn >= FG_ConnectFourField.Columns)
                        {
                            continue;
                        }

                        bool isWinningLine = true;
                        for (int step = 1; step < 4; step++)
                        {
                            if (field[row + rowStep * step, column + columnStep * step] != player)
                            {
                                isWinningLine = false;
                                break;
                            }
                        }

                        if (isWinningLine)
                        {
                            Line winningLine = new Line
                            {
                                X1 = _left + column * _cellSize + _cellSize / 2,
                                Y1 = _top + row * _cellSize + _cellSize / 2,
                                X2 = _left + endColumn * _cellSize + _cellSize / 2,
                                Y2 = _top + endRow * _cellSize + _cellSize / 2,
                                Stroke = new SolidColorBrush(Color.FromRgb(30, 100, 190)),
                                StrokeThickness = 24,
                                StrokeStartLineCap = PenLineCap.Round,
                                StrokeEndLineCap = PenLineCap.Round,
                                IsHitTestVisible = false
                            };
                            canvas.Children.Add(winningLine);
                            return;
                        }
                    }
                }
            }
        }

        private void StartAnimationIfNeeded(I4GWFeld field)
        {
            int[,] currentField = CaptureField(field);
            if (_previousBoard == null)
            {
                _previousBoard = currentField;
                CopyField(field);
                return;
            }

            bool requiresReset = false;
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    if (currentField[row, column] != 0 &&
                        _previousBoard[row, column] != 0 &&
                        currentField[row, column] != _previousBoard[row, column])
                    {
                        requiresReset = true;
                    }

                    if (_previousBoard[row, column] > 0 &&
                        currentField[row, column] == 0)
                    {
                        requiresReset = true;
                    }
                }
            }

            if (requiresReset)
            {
                _pendingMoves.Clear();
                _isAnimating = false;
                IsAnyAnimating = false;
                _animationTimer.Stop();
                CopyField(field);
                _previousBoard = currentField;
                return;
            }

            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    if (_previousBoard[row, column] == 0 &&
                        currentField[row, column] > 0)
                    {
                        _pendingMoves.Enqueue(new BoardMove(
                            row, column, currentField[row, column]));
                    }
                }
            }
            _previousBoard = currentField;

            if (_isAnimating)
            {
                return;
            }

            if (_pendingMoves.Count > 0)
            {
                BoardMove move = _pendingMoves.Dequeue();
                _fallingRow = move.Row;
                _fallingColumn = move.Column;
                _fallingPlayer = move.Player;
                _animationStarted = DateTime.UtcNow;
                _isAnimating = true;
                IsAnyAnimating = true;
                _animationTimer.Start();
            }
        }

        private int[,] CaptureField(I4GWFeld field)
        {
            int[,] copy = new int[FG_ConnectFourField.Rows, FG_ConnectFourField.Columns];
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    copy[row, column] = field[row, column];
                }
            }

            return copy;
        }

        private void CopyField(I4GWFeld field)
        {
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    _displayedBoard[row, column] = field[row, column];
                }
            }
        }

        private void UpdateBoardGeometry(Canvas canvas)
        {
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                return;
            }

            double availableWidth = Math.Max(1, width - Padding * 2);
            double availableHeight = Math.Max(1, height - Padding * 2);
            _cellSize = Math.Min(
                availableWidth / FG_ConnectFourField.Columns,
                availableHeight / FG_ConnectFourField.Rows);
            _left = (width - _cellSize * FG_ConnectFourField.Columns) / 2;
            _top = (height - _cellSize * FG_ConnectFourField.Rows) / 2;
        }
    }

    public sealed class FG_ConnectFourHumanPlayer : IFG_HumanConnectFourPlayer
    {
        private int _playerNumber;

        public string Name { get { return "FG Mensch Vier gewinnt"; } }
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
            FG_ConnectFourHumanPlayer clone = new FG_ConnectFourHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (FG_ConnectFourPaint.IsAnyAnimating)
            {
                return null;
            }

            I4GWFeld gameField = field as I4GWFeld;
            if (gameField == null)
            {
                return null;
            }

            int column = GetSelectedColumn(selection);
            if (column < 0 ||
                column >= FG_ConnectFourField.Columns ||
                gameField[0, column] != 0)
            {
                return null;
            }

            return new FG_ConnectFourMove(
                GetLandingRow(gameField, column),
                column,
                _playerNumber);
        }

        private static int GetSelectedColumn(IMoveSelection selection)
        {
            IClickSelection click = selection as IClickSelection;
            if (click != null)
            {
                IConnectFourPainter painter =
                    OOPGamesManager.Singleton.ActivePainter as IConnectFourPainter;
                int fallbackColumn =
                    (int)((click.XClickPos - FG_ConnectFourPaint.Left) /
                        FG_ConnectFourPaint.CellSize);
                int column = painter == null
                    ? fallbackColumn
                    : painter.ColumnAt(click.XClickPos, click.YClickPos);
                return column < 0 &&
                    fallbackColumn >= 0 &&
                    fallbackColumn < FG_ConnectFourField.Columns
                    ? fallbackColumn
                    : column;
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

        public IFG_ConnectFourMove GetMove(
            IMoveSelection selection,
            IFG_ConnectFourField field)
        {
            return GetMove(selection, (IGameField)field) as IFG_ConnectFourMove;
        }

        private int GetLandingRow(I4GWFeld field, int column)
        {
            for (int row = FG_ConnectFourField.Rows - 1; row >= 0; row--)
            {
                if (field[row, column] == 0)
                {
                    return row;
                }
            }

            return -1;
        }

    }

    public sealed class FG_ConnectFourComputerPlayer : IFG_ComputerConnectFourPlayer
    {
        private const int SearchDepth = 4;
        private static readonly Random Random = new Random();
        private int _playerNumber;

        public string Name { get { return "FG Computer Vier gewinnt"; } }
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
            FG_ConnectFourComputerPlayer clone = new FG_ConnectFourComputerPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IGameField field)
        {
            I4GWFeld gameField = field as I4GWFeld;
            if (gameField == null || _playerNumber < 1)
            {
                return null;
            }

            WaitForComputerMove();

            int opponent = _playerNumber == 1 ? 2 : 1;
            int bestScore = int.MinValue;
            List<int> bestColumns = new List<int>();
            for (int column = 0; column < FG_ConnectFourField.Columns; column++)
            {
                int row = GetLandingRow(gameField, column);
                if (row < 0)
                {
                    continue;
                }

                gameField[row, column] = _playerNumber;
                int score = HasWon(gameField, row, column, _playerNumber)
                    ? 1000000
                    : Minimax(gameField, SearchDepth - 1, opponent, false, int.MinValue, int.MaxValue);
                gameField[row, column] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumns.Clear();
                    bestColumns.Add(column);
                }
                else if (score == bestScore)
                {
                    bestColumns.Add(column);
                }
            }

            if (bestColumns.Count == 0)
            {
                return null;
            }

            int selectedColumn = bestColumns[Random.Next(bestColumns.Count)];
            return new FG_ConnectFourMove(
                GetLandingRow(gameField, selectedColumn),
                selectedColumn,
                _playerNumber);
        }

        public IFG_ConnectFourMove GetMove(IFG_ConnectFourField field)
        {
            return GetMove((IGameField)field) as IFG_ConnectFourMove;
        }

        private static void WaitForComputerMove()
        {
            Application application = Application.Current;
            Dispatcher dispatcher = application == null ? null : application.Dispatcher;
            if (dispatcher == null || !dispatcher.CheckAccess())
            {
                System.Threading.Thread.Sleep(500);
                return;
            }

            DispatcherFrame frame = new DispatcherFrame();
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background, dispatcher);
            timer.Interval = TimeSpan.FromMilliseconds(500);
            EventHandler onTimerTick = null;
            onTimerTick = delegate
            {
                timer.Stop();
                timer.Tick -= onTimerTick;
                frame.Continue = false;
            };
            timer.Tick += onTimerTick;
            timer.Start();

            Dispatcher.PushFrame(frame);
        }

        private int Minimax(
            I4GWFeld field,
            int depth,
            int player,
            bool maximizing,
            int alpha,
            int beta)
        {
            if (depth == 0)
            {
                return Evaluate(field);
            }

            List<int> columns = GetAvailableColumns(field);
            if (columns.Count == 0)
            {
                return 0;
            }

            int bestScore = maximizing ? int.MinValue : int.MaxValue;
            foreach (int column in columns)
            {
                int row = GetLandingRow(field, column);
                field[row, column] = player;
                int score;
                if (HasWon(field, row, column, player))
                {
                    score = maximizing ? 1000000 + depth : -1000000 - depth;
                }
                else
                {
                    int nextPlayer = player == 1 ? 2 : 1;
                    score = Minimax(field, depth - 1, nextPlayer, !maximizing, alpha, beta);
                }

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

            return bestScore;
        }

        private int Evaluate(I4GWFeld field)
        {
            int opponent = _playerNumber == 1 ? 2 : 1;
            int score = 0;

            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                if (field[row, FG_ConnectFourField.Columns / 2] == _playerNumber)
                {
                    score += 4;
                }
            }

            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    score += ScoreWindow(field, row, column, 0, 1, _playerNumber, opponent);
                    score += ScoreWindow(field, row, column, 1, 0, _playerNumber, opponent);
                    score += ScoreWindow(field, row, column, 1, 1, _playerNumber, opponent);
                    score += ScoreWindow(field, row, column, 1, -1, _playerNumber, opponent);
                }
            }

            return score;
        }

        private int ScoreWindow(
            I4GWFeld field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player,
            int opponent)
        {
            int playerCount = 0;
            int opponentCount = 0;
            int emptyCount = 0;

            for (int index = 0; index < 4; index++)
            {
                int nextRow = row + rowStep * index;
                int nextColumn = column + columnStep * index;
                if (nextRow < 0 ||
                    nextRow >= FG_ConnectFourField.Rows ||
                    nextColumn < 0 ||
                    nextColumn >= FG_ConnectFourField.Columns)
                {
                    return 0;
                }

                int value = field[nextRow, nextColumn];
                if (value == player)
                {
                    playerCount++;
                }
                else if (value == opponent)
                {
                    opponentCount++;
                }
                else if (value == 0)
                {
                    emptyCount++;
                }
                else
                {
                    return 0;
                }
            }

            if (playerCount == 4)
            {
                return 1000000;
            }
            if (opponentCount == 4)
            {
                return -1000000;
            }
            if (playerCount == 3 && emptyCount == 1)
            {
                return 80;
            }
            if (playerCount == 2 && emptyCount == 2)
            {
                return 12;
            }
            if (opponentCount == 3 && emptyCount == 1)
            {
                return -100;
            }
            if (opponentCount == 2 && emptyCount == 2)
            {
                return -15;
            }

            return 0;
        }

        private List<int> GetAvailableColumns(I4GWFeld field)
        {
            List<int> columns = new List<int>();
            for (int column = 0; column < FG_ConnectFourField.Columns; column++)
            {
                if (field[0, column] == 0)
                {
                    columns.Add(column);
                }
            }

            return columns;
        }

        private int GetLandingRow(I4GWFeld field, int column)
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

        private bool HasWon(I4GWFeld field, int row, int column, int player)
        {
            return Count(field, row, column, 0, 1, player) +
                       Count(field, row, column, 0, -1, player) - 1 >= 4 ||
                   Count(field, row, column, 1, 0, player) >= 4 ||
                   Count(field, row, column, 1, 1, player) +
                       Count(field, row, column, -1, -1, player) - 1 >= 4 ||
                   Count(field, row, column, 1, -1, player) +
                       Count(field, row, column, -1, 1, player) - 1 >= 4;
        }

        private int Count(
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
    }
}
