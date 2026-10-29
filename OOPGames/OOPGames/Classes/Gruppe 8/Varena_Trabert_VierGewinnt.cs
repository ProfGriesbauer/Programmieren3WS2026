using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

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
        private const double DropMilliseconds = 400;
        private const double CelebrationMilliseconds = 5000;
        private const int ConfettiCount = 100;
        private static readonly Color[] ConfettiColors =
        {
            Colors.Gold,
            Colors.Lime,
            Colors.DeepPink,
            Colors.DodgerBlue,
            Colors.Orange,
            Colors.Violet
        };

        private readonly Queue<AnimatedMove> _queuedMoves = new Queue<AnimatedMove>();
        private readonly int[,] _displayedCells = new int[G8_VierGewinntLogic.Rows, G8_VierGewinntLogic.Columns];
        private readonly List<ConfettiPiece> _confetti = new List<ConfettiPiece>();
        private readonly Random _random = new Random();
        private int[,] _lastObservedCells;
        private AnimatedMove _activeMove;
        private int _celebrationWinner;
        private DateTime _celebrationStartedAt;
        private bool _hasObservedField;

        public string Name { get { return "Gruppe 8 Vier Gewinnt Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            I4GWFeld field = currentField as I4GWFeld;
            if (field == null)
            {
                canvas.Children.Clear();
                return;
            }

            ObserveField(field);
            DateTime now = DateTime.UtcNow;
            AdvanceAnimations(now);
            int winner = G8_VierGewinntLogic.Winner(field);
            UpdateCelebration(winner, now);

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
                    int player = _displayedCells[row, column];
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

            DrawActiveMove(canvas, now);
            DrawWinnerCelebration(canvas, now);

            if (winner <= 0 && G8_VierGewinntLogic.IsFull(field))
            {
                AddText(canvas, "Unentschieden!",
                    G8_VierGewinntLayout.Left, 30, 14);
            }
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }

        private void ObserveField(I4GWFeld field)
        {
            int[,] currentCells = CaptureField(field);
            if (!_hasObservedField)
            {
                CopyField(currentCells, _displayedCells);
                _lastObservedCells = currentCells;
                _hasObservedField = true;
                return;
            }

            List<AnimatedMove> newMoves = new List<AnimatedMove>();
            bool requiresReset = false;
            for (int row = 0; row < G8_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G8_VierGewinntLogic.Columns; column++)
                {
                    int previousValue = _lastObservedCells[row, column];
                    int currentValue = currentCells[row, column];
                    if (previousValue == currentValue)
                    {
                        continue;
                    }

                    if (previousValue == 0 && currentValue > 0)
                    {
                        newMoves.Add(new AnimatedMove(row, column, currentValue));
                    }
                    else
                    {
                        requiresReset = true;
                    }
                }
            }

            if (requiresReset)
            {
                _queuedMoves.Clear();
                _activeMove = null;
                CopyField(currentCells, _displayedCells);
            }
            else
            {
                foreach (AnimatedMove move in newMoves)
                {
                    _queuedMoves.Enqueue(move);
                }
            }

            _lastObservedCells = currentCells;
        }

        private void AdvanceAnimations(DateTime now)
        {
            while (true)
            {
                if (_activeMove == null)
                {
                    if (_queuedMoves.Count == 0)
                    {
                        return;
                    }

                    _activeMove = _queuedMoves.Dequeue();
                    _activeMove.StartedAt = now;
                }

                double elapsedMilliseconds = (now - _activeMove.StartedAt).TotalMilliseconds;
                if (elapsedMilliseconds < DropMilliseconds)
                {
                    return;
                }

                _displayedCells[_activeMove.Row, _activeMove.Column] = _activeMove.PlayerNumber;
                _activeMove = null;
            }
        }

        private void DrawActiveMove(Canvas canvas, DateTime now)
        {
            if (_activeMove == null)
            {
                return;
            }

            double elapsedMilliseconds = (now - _activeMove.StartedAt).TotalMilliseconds;
            double progress = Math.Min(1, elapsedMilliseconds / DropMilliseconds);
            double diameter = G8_VierGewinntLayout.Cell - 10;
            double startTop = G8_VierGewinntLayout.Top - diameter;
            double destinationTop = G8_VierGewinntLayout.Top
                + _activeMove.Row * G8_VierGewinntLayout.Cell + 5;
            Ellipse fallingDisc = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                Fill = _activeMove.PlayerNumber == 1 ? PlayerOneBrush : PlayerTwoBrush,
                Stroke = _activeMove.PlayerNumber == 1 ? Brushes.Navy : Brushes.DeepPink,
                StrokeThickness = 2,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(fallingDisc, G8_VierGewinntLayout.Left
                + _activeMove.Column * G8_VierGewinntLayout.Cell + 5);
            Canvas.SetTop(fallingDisc, startTop + (destinationTop - startTop) * progress);
            canvas.Children.Add(fallingDisc);
        }

        private void UpdateCelebration(int winner, DateTime now)
        {
            if (winner <= 0)
            {
                _celebrationWinner = 0;
                _confetti.Clear();
                return;
            }

            if (_activeMove != null || _queuedMoves.Count > 0 || _celebrationWinner == winner)
            {
                return;
            }

            _celebrationWinner = winner;
            _celebrationStartedAt = now;
            _confetti.Clear();

            double centerX = G8_VierGewinntLayout.Left
                + G8_VierGewinntLogic.Columns * G8_VierGewinntLayout.Cell / 2.0;
            double centerY = G8_VierGewinntLayout.Top
                + G8_VierGewinntLogic.Rows * G8_VierGewinntLayout.Cell / 2.0;
            for (int i = 0; i < ConfettiCount; i++)
            {
                double angle = _random.NextDouble() * Math.PI * 2;
                double speed = 70 + _random.NextDouble() * 210;
                _confetti.Add(new ConfettiPiece(
                    centerX,
                    centerY,
                    Math.Cos(angle) * speed,
                    Math.Sin(angle) * speed - 70,
                    180 + _random.NextDouble() * 100,
                    5 + _random.NextDouble() * 7,
                    ConfettiColors[_random.Next(ConfettiColors.Length)],
                    _random.NextDouble() * 360));
            }
        }

        private void DrawWinnerCelebration(Canvas canvas, DateTime now)
        {
            if (_celebrationWinner <= 0)
            {
                return;
            }

            double elapsedSeconds = (now - _celebrationStartedAt).TotalSeconds;
            if (elapsedSeconds < 0 || elapsedSeconds >= CelebrationMilliseconds / 1000)
            {
                return;
            }

            double progress = elapsedSeconds / (CelebrationMilliseconds / 1000);
            foreach (ConfettiPiece piece in _confetti)
            {
                double x = piece.OriginX + piece.VelocityX * elapsedSeconds;
                double y = piece.OriginY + piece.VelocityY * elapsedSeconds
                    + 0.5 * piece.Gravity * elapsedSeconds * elapsedSeconds;
                double size = piece.Size;
                byte alpha = (byte)(255 * Math.Max(0, 1 - progress * 0.45));
                Rectangle confetti = new Rectangle
                {
                    Width = size,
                    Height = size * 0.65,
                    Fill = new SolidColorBrush(Color.FromArgb(alpha,
                        piece.Color.R, piece.Color.G, piece.Color.B)),
                    IsHitTestVisible = false,
                    RenderTransform = new RotateTransform(
                        piece.InitialRotation + elapsedSeconds * 300, size / 2, size / 2)
                };
                Canvas.SetLeft(confetti, x);
                Canvas.SetTop(confetti, y);
                canvas.Children.Add(confetti);
            }

            TextBlock winnerText = new TextBlock
            {
                Text = "Spieler " + _celebrationWinner + " hat gewonnen!",
                Width = G8_VierGewinntLogic.Columns * G8_VierGewinntLayout.Cell - 20,
                FontSize = 30,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.Gold,
                Background = new SolidColorBrush(Color.FromArgb(220, 20, 20, 20)),
                Padding = new Thickness(8),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(winnerText, G8_VierGewinntLayout.Left + 10);
            Canvas.SetTop(winnerText,
                G8_VierGewinntLayout.Top
                + G8_VierGewinntLogic.Rows * G8_VierGewinntLayout.Cell / 2.0 - 35);
            canvas.Children.Add(winnerText);
        }

        private static int[,] CaptureField(I4GWFeld field)
        {
            int[,] cells = new int[G8_VierGewinntLogic.Rows, G8_VierGewinntLogic.Columns];
            for (int row = 0; row < G8_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G8_VierGewinntLogic.Columns; column++)
                {
                    cells[row, column] = field[row, column];
                }
            }

            return cells;
        }

        private static void CopyField(int[,] source, int[,] destination)
        {
            for (int row = 0; row < G8_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G8_VierGewinntLogic.Columns; column++)
                {
                    destination[row, column] = source[row, column];
                }
            }
        }

        private sealed class AnimatedMove
        {
            internal AnimatedMove(int row, int column, int playerNumber)
            {
                Row = row;
                Column = column;
                PlayerNumber = playerNumber;
            }

            internal int Row { get; private set; }
            internal int Column { get; private set; }
            internal int PlayerNumber { get; private set; }
            internal DateTime StartedAt { get; set; }
        }

        private sealed class ConfettiPiece
        {
            internal ConfettiPiece(
                double originX,
                double originY,
                double velocityX,
                double velocityY,
                double gravity,
                double size,
                Color color,
                double initialRotation)
            {
                OriginX = originX;
                OriginY = originY;
                VelocityX = velocityX;
                VelocityY = velocityY;
                Gravity = gravity;
                Size = size;
                Color = color;
                InitialRotation = initialRotation;
            }

            internal double OriginX { get; private set; }
            internal double OriginY { get; private set; }
            internal double VelocityX { get; private set; }
            internal double VelocityY { get; private set; }
            internal double Gravity { get; private set; }
            internal double Size { get; private set; }
            internal Color Color { get; private set; }
            internal double InitialRotation { get; private set; }
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
                || G8_VierGewinntLogic.Winner(connectFourField) > 0
                || G8_VierGewinntLogic.IsFull(connectFourField))
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
        private const int SearchDepth = 6;
        private const int WinScore = 100000;
        private static readonly int[] PreferredColumns = { 3, 2, 4, 1, 5, 0, 6 };
        private static readonly int[,] Directions =
        {
            { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 }
        };
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

            WaitForComputerMove();

            int[,] board = CopyField(connectFourField);
            int bestScore = int.MinValue;
            int bestColumn = -1;
            int equallyGoodMoves = 0;
            foreach (int column in PreferredColumns)
            {
                int row = FreeRow(board, column);
                if (row < 0)
                {
                    continue;
                }

                board[row, column] = _playerNumber;
                int score = Search(board, SearchDepth - 1, OpponentOf(_playerNumber),
                    int.MinValue, int.MaxValue);
                board[row, column] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumn = column;
                    equallyGoodMoves = 1;
                }
                else if (score == bestScore)
                {
                    equallyGoodMoves++;
                    if (Random.Shared.Next(equallyGoodMoves) == 0)
                    {
                        bestColumn = column;
                    }
                }
            }

            return bestColumn < 0
                ? null
                : (IPlayMove)new G8_VierGewinntMove(
                    G8_VierGewinntLogic.FreeRow(connectFourField, bestColumn),
                    bestColumn,
                    _playerNumber);
        }

        private int Search(int[,] board, int depth, int currentPlayer, int alpha, int beta)
        {
            int winner = Winner(board);
            if (winner == _playerNumber)
            {
                return WinScore + depth;
            }

            if (winner == OpponentOf(_playerNumber))
            {
                return -WinScore - depth;
            }

            if (depth == 0 || IsFull(board))
            {
                return Evaluate(board);
            }

            bool maximizing = currentPlayer == _playerNumber;
            int bestScore = maximizing ? int.MinValue : int.MaxValue;
            foreach (int column in PreferredColumns)
            {
                int row = FreeRow(board, column);
                if (row < 0)
                {
                    continue;
                }

                board[row, column] = currentPlayer;
                int score = Search(board, depth - 1, OpponentOf(currentPlayer), alpha, beta);
                board[row, column] = 0;

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

        private int Evaluate(int[,] board)
        {
            int score = 0;
            for (int row = 0; row < G8_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G8_VierGewinntLogic.Columns; column++)
                {
                    int player = board[row, column];
                    int centerValue = 3 - Math.Abs(3 - column);
                    if (player == _playerNumber)
                    {
                        score += centerValue * 4;
                    }
                    else if (player == OpponentOf(_playerNumber))
                    {
                        score -= centerValue * 4;
                    }
                }
            }

            for (int row = 0; row < G8_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G8_VierGewinntLogic.Columns; column++)
                {
                    for (int direction = 0; direction < 4; direction++)
                    {
                        int rowStep = Directions[direction, 0];
                        int columnStep = Directions[direction, 1];
                        int endRow = row + 3 * rowStep;
                        int endColumn = column + 3 * columnStep;
                        if (!IsInside(endRow, endColumn))
                        {
                            continue;
                        }

                        int ownPieces = 0;
                        int opponentPieces = 0;
                        for (int offset = 0; offset < 4; offset++)
                        {
                            int player = board[row + offset * rowStep, column + offset * columnStep];
                            if (player == _playerNumber)
                            {
                                ownPieces++;
                            }
                            else if (player == OpponentOf(_playerNumber))
                            {
                                opponentPieces++;
                            }
                        }

                        if (opponentPieces == 0)
                        {
                            score += WindowValue(ownPieces);
                        }
                        else if (ownPieces == 0)
                        {
                            score -= WindowValue(opponentPieces) * 2;
                        }
                    }
                }
            }

            return score;
        }

        private static int WindowValue(int pieces)
        {
            switch (pieces)
            {
                case 1:
                    return 2;
                case 2:
                    return 12;
                case 3:
                    return 80;
                default:
                    return 0;
            }
        }

        private static int[,] CopyField(I4GWFeld field)
        {
            int[,] board = new int[G8_VierGewinntLogic.Rows, G8_VierGewinntLogic.Columns];
            for (int row = 0; row < G8_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G8_VierGewinntLogic.Columns; column++)
                {
                    board[row, column] = field[row, column];
                }
            }

            return board;
        }

        private static int FreeRow(int[,] board, int column)
        {
            for (int row = G8_VierGewinntLogic.Rows - 1; row >= 0; row--)
            {
                if (board[row, column] == 0)
                {
                    return row;
                }
            }

            return -1;
        }

        private static bool IsFull(int[,] board)
        {
            for (int column = 0; column < G8_VierGewinntLogic.Columns; column++)
            {
                if (FreeRow(board, column) >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        private static int Winner(int[,] board)
        {
            for (int row = 0; row < G8_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G8_VierGewinntLogic.Columns; column++)
                {
                    int player = board[row, column];
                    if (player == 0)
                    {
                        continue;
                    }

                    for (int direction = 0; direction < 4; direction++)
                    {
                        int rowStep = Directions[direction, 0];
                        int columnStep = Directions[direction, 1];
                        int endRow = row + 3 * rowStep;
                        int endColumn = column + 3 * columnStep;
                        if (!IsInside(endRow, endColumn))
                        {
                            continue;
                        }

                        if (board[row + rowStep, column + columnStep] == player
                            && board[row + 2 * rowStep, column + 2 * columnStep] == player
                            && board[endRow, endColumn] == player)
                        {
                            return player;
                        }
                    }
                }
            }

            return -1;
        }

        private static bool IsInside(int row, int column)
        {
            return row >= 0 && row < G8_VierGewinntLogic.Rows
                && column >= 0 && column < G8_VierGewinntLogic.Columns;
        }

        private static int OpponentOf(int player)
        {
            return player == 1 ? 2 : 1;
        }

        private static void WaitForComputerMove()
        {
            Application application = Application.Current;
            Dispatcher dispatcher = application == null ? null : application.Dispatcher;
            if (dispatcher == null || !dispatcher.CheckAccess())
            {
                Thread.Sleep(500);
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

    }
}
