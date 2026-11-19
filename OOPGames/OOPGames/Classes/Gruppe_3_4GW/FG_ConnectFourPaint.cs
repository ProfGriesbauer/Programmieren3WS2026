using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
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

            public int Row { get; }
            public int Column { get; }
            public int Player { get; }
        }

        private const double Padding = 10;
        private const double BoardBorderWidth = 8;
        private const double AnimationDurationMilliseconds = 350;
        private const double WinningBlinkDurationMilliseconds = 300;
        private static readonly Brush PlayerOneBrush =
            new SolidColorBrush(Color.FromRgb(220, 40, 40));
        private static readonly Brush PlayerTwoBrush =
            new SolidColorBrush(Color.FromRgb(245, 215, 35));
        private static readonly Brush BoardBrush =
            new SolidColorBrush(Color.FromRgb(30, 100, 190));
        private static readonly Brush BoardBorderBrush =
            new SolidColorBrush(Color.FromRgb(20, 70, 140));
        internal const double DefaultCellSize = 65;
        internal const double DefaultLeft = Padding;
        private double _cellSize = DefaultCellSize;
        private double _left = DefaultLeft;
        private double _top = Padding;
        private readonly int[,] _displayedBoard =
            new int[FG_ConnectFourField.Rows, FG_ConnectFourField.Columns];
        private int[,] _previousBoard;
        private readonly Queue<BoardMove> _pendingMoves = new Queue<BoardMove>();
        private bool _isAnimating;
        private int _fallingRow;
        private int _fallingColumn;
        private int _fallingPlayer;
        private DateTime _animationStarted;
        private bool _winningBlinkActive;
        private DateTime _winningBlinkStarted;
        private readonly bool[,] _winningTokens =
            new bool[FG_ConnectFourField.Rows, FG_ConnectFourField.Columns];
        private bool _winningTokensDirty = true;
        private readonly Ellipse[,] _tokenVisuals =
            new Ellipse[FG_ConnectFourField.Rows, FG_ConnectFourField.Columns];
        private Ellipse _fallingToken;
        private Path _boardVisual;
        private Canvas _visualCanvas;

        public double CellSize { get { return _cellSize; } }
        public double Left { get { return _left; } }
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
            StartAnimationIfNeeded(field);
            DrawField(canvas, field);
        }

        public void PaintConnectFourField(Canvas canvas, IFG_ConnectFourField currentField)
        {
            PaintGameField(canvas, currentField);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }

        private void DrawField(Canvas canvas, I4GWFeld field)
        {
            EnsureVisuals(canvas);
            EnsureWinningTokens();
            bool blinkOn = UpdateWinningBlink();
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    int player = _displayedBoard[row, column];
                    bool isFallingToken = _isAnimating &&
                        row == _fallingRow &&
                        column == _fallingColumn;
                    UpdateTokenVisual(
                        _tokenVisuals[row, column],
                        player,
                        row,
                        column,
                        null,
                        _winningTokens[row, column] && blinkOn && !isFallingToken);
                    _tokenVisuals[row, column].Visibility =
                        player > 0 && !isFallingToken
                            ? Visibility.Visible
                            : Visibility.Hidden;
                }
            }

            if (_isAnimating)
            {
                double elapsed = (DateTime.UtcNow - _animationStarted).TotalMilliseconds;
                double progress = Math.Min(1, elapsed / AnimationDurationMilliseconds);
                double easedProgress = 1 - Math.Pow(1 - progress, 2);
                double startTop = _top - _cellSize + 4;
                double targetTop = _top + _fallingRow * _cellSize + 4;
                UpdateTokenVisual(
                    _fallingToken,
                    _fallingPlayer,
                    _fallingRow,
                    _fallingColumn,
                    startTop + (targetTop - startTop) * easedProgress,
                    false);
                _fallingToken.Visibility = Visibility.Visible;

                if (progress >= 1)
                {
                    _displayedBoard[_fallingRow, _fallingColumn] = _fallingPlayer;
                    _isAnimating = false;
                    IsAnyAnimating = false;
                    _winningTokensDirty = true;
                    UpdateTokenVisual(
                        _tokenVisuals[_fallingRow, _fallingColumn],
                        _fallingPlayer,
                        _fallingRow,
                        _fallingColumn);
                    _tokenVisuals[_fallingRow, _fallingColumn].Visibility =
                        Visibility.Visible;
                    _fallingToken.Visibility = Visibility.Hidden;
                    StartAnimationIfNeeded(field);
                }
            }
            else
            {
                _fallingToken.Visibility = Visibility.Hidden;
            }

            UpdateBoardVisual();
        }

        private void EnsureVisuals(Canvas canvas)
        {
            if (ReferenceEquals(_visualCanvas, canvas))
            {
                return;
            }

            canvas.Children.Clear();
            _visualCanvas = canvas;
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    _tokenVisuals[row, column] = CreateTokenVisual();
                    canvas.Children.Add(_tokenVisuals[row, column]);
                }
            }

            _fallingToken = CreateTokenVisual();
            canvas.Children.Add(_fallingToken);
            _boardVisual = new Path
            {
                IsHitTestVisible = false
            };
            canvas.Children.Add(_boardVisual);
        }

        private static Ellipse CreateTokenVisual()
        {
            return new Ellipse
            {
                Visibility = Visibility.Hidden,
                IsHitTestVisible = false
            };
        }

        private void UpdateTokenVisual(
            Ellipse token,
            int player,
            int row,
            int column,
            double? top = null,
            bool highlighted = false)
        {
            Brush tokenBrush = player == 1
                ? PlayerOneBrush
                : player == 2
                    ? PlayerTwoBrush
                    : Brushes.White;
            token.Width = Math.Max(1, _cellSize - 8);
            token.Height = Math.Max(1, _cellSize - 8);
            token.Fill = tokenBrush;
            token.Stroke = highlighted ? Brushes.White : Brushes.Black;
            token.StrokeThickness = highlighted ? 4 : 1;
            Canvas.SetLeft(token, _left + column * _cellSize + 4);
            Canvas.SetTop(token, top ?? _top + row * _cellSize + 4);
        }

        private void UpdateBoardVisual()
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

            _boardVisual.Data = boardGeometry;
            _boardVisual.Fill = BoardBrush;
            _boardVisual.Stroke = BoardBorderBrush;
            _boardVisual.StrokeThickness = 2;
        }

        private void EnsureWinningTokens()
        {
            if (!_winningTokensDirty)
            {
                return;
            }

            bool[,] updatedTokens =
                FG_ConnectFourBoardLogic.GetWinningTokens(_displayedBoard);
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    _winningTokens[row, column] = updatedTokens[row, column];
                }
            }

            _winningTokensDirty = false;
        }

        private bool UpdateWinningBlink()
        {
            bool hasWinningTokens = false;
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    if (_winningTokens[row, column])
                    {
                        hasWinningTokens = true;
                        break;
                    }
                }

                if (hasWinningTokens)
                {
                    break;
                }
            }

            if (!hasWinningTokens)
            {
                _winningBlinkActive = false;
                return false;
            }

            if (!_winningBlinkActive)
            {
                _winningBlinkStarted = DateTime.UtcNow;
                _winningBlinkActive = true;
            }

            double elapsed = (DateTime.UtcNow - _winningBlinkStarted).TotalMilliseconds;
            return ((int)(elapsed / WinningBlinkDurationMilliseconds) % 2) == 0;
        }

        private void StartAnimationIfNeeded(I4GWFeld field)
        {
            int[,] currentField = CaptureField(field);
            if (_previousBoard == null)
            {
                _previousBoard = currentField;
                CopyBoard(field);
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
                CopyBoard(field);
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

        private void CopyBoard(I4GWFeld field)
        {
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    _displayedBoard[row, column] = field[row, column];
                }
            }

            _winningTokensDirty = true;
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
}
