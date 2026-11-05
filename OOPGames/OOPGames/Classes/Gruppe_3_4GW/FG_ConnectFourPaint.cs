using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
    
            public int Row { get; private set; }
            public int Column { get; private set; }
            public int Player { get; private set; }
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
        private static readonly int[,] WinningDirections =
        {
            { 0, 1 },
            { 1, 0 },
            { 1, 1 },
            { 1, -1 }
        };
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
        private bool _winningBlinkActive;
        private DateTime _winningBlinkStarted;
    
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
            bool[,] winningTokens = FindWinningTokens();
            bool blinkOn = UpdateWinningBlink(winningTokens);
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
                    AddToken(canvas, player, row, column,
                        null,
                        winningTokens[row, column] && blinkOn);
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
            double? top = null,
            bool highlighted = false)
        {
            if (player <= 0)
            {
                return;
            }
    
            Brush tokenBrush = player == 1
                ? PlayerOneBrush
                : player == 2
                    ? PlayerTwoBrush
                    : Brushes.White;
            Ellipse token = new Ellipse
            {
                Width = Math.Max(1, _cellSize - 8),
                Height = Math.Max(1, _cellSize - 8),
                Fill = tokenBrush,
                Stroke = highlighted ? Brushes.White : Brushes.Black,
                StrokeThickness = highlighted ? 4 : 1,
                Opacity = 1
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
                Fill = BoardBrush,
                Stroke = BoardBorderBrush,
                StrokeThickness = 2
            };
            canvas.Children.Add(board);
        }
    
        private bool[,] FindWinningTokens()
        {
            int[,] field = _displayedBoard;
            bool[,] winningTokens = new bool[FG_ConnectFourField.Rows, FG_ConnectFourField.Columns];
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
                        int rowStep = WinningDirections[direction, 0];
                        int columnStep = WinningDirections[direction, 1];
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
                            for (int step = 0; step < 4; step++)
                            {
                                winningTokens[
                                    row + rowStep * step,
                                    column + columnStep * step] = true;
                            }
                        }
                    }
                }
            }
    
            return winningTokens;
        }
    
        private bool UpdateWinningBlink(bool[,] winningTokens)
        {
            bool hasWinningTokens = false;
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    if (winningTokens[row, column])
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
