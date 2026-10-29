using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OOPGames
{
    internal sealed class FG_ConnectFourPlayedMove
    {
        public FG_ConnectFourPlayedMove(int row, int column, int player)
        {
            Row = row;
            Column = column;
            Player = player;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
        public int Player { get; private set; }
    }

    public sealed class FG_ConnectFourField : I4GWFeld
    {
        public const int Rows = 6;
        public const int Columns = 7;
        private readonly int[,] _cells = new int[Rows, Columns];
        internal List<FG_ConnectFourPlayedMove> MoveHistory { get; private set; } =
            new List<FG_ConnectFourPlayedMove>();

        public int this[int row, int column]
        {
            get
            {
                if (row < 0 || row >= Rows || column < 0 || column >= Columns)
                {
                    return -1;
                }

                return _cells[row, column];
            }
            set
            {
                if (row >= 0 && row < Rows && column >= 0 && column < Columns)
                {
                    _cells[row, column] = value;
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is FG_ConnectFourPaint;
        }

        internal void ClearMoveHistory()
        {
            MoveHistory.Clear();
        }

        internal void AddPlayedMove(int row, int column, int player)
        {
            MoveHistory.Add(new FG_ConnectFourPlayedMove(row, column, player));
        }
    }

    public sealed class FG_ConnectFourMove : I4GWMove
    {
        public FG_ConnectFourMove(int column, int playerNumber)
        {
            Column = column;
            PlayerNumber = playerNumber;
        }

        public int Row { get { return -1; } }
        public int Column { get; private set; }
        public int PlayerNumber { get; private set; }
    }

    public sealed class FG_ConnectFourRules : IGameRules
    {
        private readonly FG_ConnectFourField _field = new FG_ConnectFourField();

        public string Name { get { return "FG Vier gewinnt Regeln"; } }
        public IGameField CurrentField { get { return _field; } }

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
            _field.ClearMoveHistory();
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
            DoConnectFourMove(move as I4GWMove);
        }

        private void DoConnectFourMove(I4GWMove move)
        {
            if (move == null ||
                move.PlayerNumber < 1 ||
                move.PlayerNumber > 2 ||
                move.Column < 0 ||
                move.Column >= FG_ConnectFourField.Columns ||
                CheckIfPLayerWon() > 0)
            {
                return;
            }

            for (int row = FG_ConnectFourField.Rows - 1; row >= 0; row--)
            {
                if (_field[row, move.Column] == 0)
                {
                    _field[row, move.Column] = move.PlayerNumber;
                    _field.AddPlayedMove(row, move.Column, move.PlayerNumber);
                    return;
                }
            }
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
                if (_field[row + rowStep * index, column + columnStep * index] != player)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public sealed class FG_ConnectFourPaint : IPaintGame2
    {
        private const double Padding = 10;
        private const double BoardBorderWidth = 8;
        private const double AnimationDurationMilliseconds = 350;
        private static double _cellSize = 65;
        private static double _left = Padding;
        private static double _top = Padding;
        private readonly int[,] _renderedField = new int[FG_ConnectFourField.Rows, FG_ConnectFourField.Columns];
        private bool _hasSnapshot;
        private bool _isAnimating;
        private int _animatedRow;
        private int _animatedColumn;
        private int _animatedPlayer;
        private DateTime _animationStarted;
        private int _nextMoveIndex;
        private readonly DispatcherTimer _animationTimer;
        private Canvas _animationCanvas;
        private FG_ConnectFourField _animationField;

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
        public bool IsAnimating { get { return _isAnimating; } }

        public string Name { get { return "FG Vier gewinnt Darstellung"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            FG_ConnectFourField field = currentField as FG_ConnectFourField;
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

        private void DrawField(Canvas canvas, FG_ConnectFourField field)
        {
            canvas.Children.Clear();
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    if (_isAnimating && row == _animatedRow && column == _animatedColumn)
                    {
                        continue;
                    }

                    int player = _isAnimating
                        ? _renderedField[row, column]
                        : field[row, column];
                    AddToken(canvas, player, row, column);
                }
            }

            if (_isAnimating)
            {
                double elapsed = (DateTime.UtcNow - _animationStarted).TotalMilliseconds;
                double progress = Math.Min(1, elapsed / AnimationDurationMilliseconds);
                double easedProgress = 1 - Math.Pow(1 - progress, 2);
                double startTop = _top - _cellSize + 4;
                double targetTop = _top + _animatedRow * _cellSize + 4;
                AddToken(canvas, _animatedPlayer, _animatedRow, _animatedColumn,
                    startTop + (targetTop - startTop) * easedProgress);

                if (progress >= 1)
                {
                    _renderedField[_animatedRow, _animatedColumn] = _animatedPlayer;
                    _nextMoveIndex++;
                    _isAnimating = false;
                    IsAnyAnimating = false;
                    _animationTimer.Stop();
                    StartAnimationIfNeeded(field);
                }
            }

            AddBoardInFront(canvas);
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

        private void StartAnimationIfNeeded(FG_ConnectFourField field)
        {
            if (field.MoveHistory.Count == 0)
            {
                _animationTimer.Stop();
                _isAnimating = false;
                IsAnyAnimating = false;
                _nextMoveIndex = 0;
                _hasSnapshot = false;
            }

            if (!_hasSnapshot)
            {
                CopyField(field);
                _hasSnapshot = true;
                return;
            }

            if (_isAnimating)
            {
                return;
            }

            if (_nextMoveIndex < field.MoveHistory.Count)
            {
                FG_ConnectFourPlayedMove move = field.MoveHistory[_nextMoveIndex];
                _animatedRow = move.Row;
                _animatedColumn = move.Column;
                _animatedPlayer = move.Player;
                _animationStarted = DateTime.UtcNow;
                _isAnimating = true;
                IsAnyAnimating = true;
                _animationTimer.Start();
                return;
            }

            if (!HasSameField(field))
            {
                CopyField(field);
            }
        }

        private bool HasSameField(FG_ConnectFourField field)
        {
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    if (_renderedField[row, column] != field[row, column])
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private void CopyField(FG_ConnectFourField field)
        {
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    _renderedField[row, column] = field[row, column];
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

    public sealed class FG_ConnectFourHumanPlayer : IHumanGamePlayer
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

            IClickSelection click = selection as IClickSelection;
            I4GWFeld connectFourField = field as I4GWFeld;
            if (click == null || connectFourField == null)
            {
                return null;
            }

            int column = (int)((click.XClickPos - FG_ConnectFourPaint.Left) / FG_ConnectFourPaint.CellSize);
            if (click.XClickPos < FG_ConnectFourPaint.Left ||
                column < 0 ||
                column >= FG_ConnectFourField.Columns ||
                connectFourField[0, column] != 0)
            {
                return null;
            }

            return new FG_ConnectFourMove(column, _playerNumber);
        }
    }

    public sealed class FG_ConnectFourComputerPlayer : IComputerGamePlayer
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
            I4GWFeld connectFourField = field as I4GWFeld;
            if (connectFourField == null || _playerNumber < 1)
            {
                return null;
            }

            int opponent = _playerNumber == 1 ? 2 : 1;
            int bestScore = int.MinValue;
            List<int> bestColumns = new List<int>();
            for (int column = 0; column < FG_ConnectFourField.Columns; column++)
            {
                int row = GetLandingRow(connectFourField, column);
                if (row < 0)
                {
                    continue;
                }

                connectFourField[row, column] = _playerNumber;
                int score = HasWon(connectFourField, row, column, _playerNumber)
                    ? 1000000
                    : Minimax(connectFourField, SearchDepth - 1, opponent, false, int.MinValue, int.MaxValue);
                connectFourField[row, column] = 0;

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

            return new FG_ConnectFourMove(
                bestColumns[Random.Next(bestColumns.Count)],
                _playerNumber);
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
                int value = field[row + rowStep * index, column + columnStep * index];
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
            while (field[row, column] == player)
            {
                count++;
                row += rowStep;
                column += columnStep;
            }

            return count;
        }
    }
}
