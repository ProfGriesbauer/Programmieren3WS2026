using System;
using System.Collections.Generic;
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
            int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };
            for (int direction = 0; direction < 4; direction++)
            {
                int count = 1;
                int rowStep = directions[direction, 0];
                int columnStep = directions[direction, 1];
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
        internal const int Left = 20;
        internal const int Top = 80;
        internal const int Cell = 50;

        internal static int ColumnAt(int x, int y)
        {
            if (x < Left || y < Top ||
                x >= Left + G1_VierGewinntLogic.Columns * Cell ||
                y >= Top + G1_VierGewinntLogic.Rows * Cell)
            {
                return -1;
            }

            return (x - Left) / Cell;
        }
    }

    public sealed class G1_VierGewinntField : I4GWFeld
    {
        private readonly int[,] _cells = new int[G1_VierGewinntLogic.Rows, G1_VierGewinntLogic.Columns];

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
                connectFourMove.PlayerNumber != _nextPlayer)
            {
                return;
            }

            int row = G1_VierGewinntLogic.FreeRow(_field, connectFourMove.Column);
            if (row < 0)
            {
                return;
            }

            _field[row, connectFourMove.Column] = connectFourMove.PlayerNumber;
            _nextPlayer = _nextPlayer == 1 ? 2 : 1;
        }

        public int CheckIfPLayerWon()
        {
            return G1_VierGewinntLogic.Winner(_field);
        }
    }

    public sealed class G1_VierGewinntPaint : IPaintGame
    {
        public string Name { get { return "Gruppe 1 Vier Gewinnt Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            I4GWFeld field = currentField as I4GWFeld;
            canvas.Children.Clear();
            canvas.Background = Brushes.White;
            if (field == null)
            {
                return;
            }

            AddText(canvas, "Klick in eine Spalte oder Taste 1-7", 20, 8, 14);
            for (int column = 0; column < G1_VierGewinntLogic.Columns; column++)
            {
                AddText(canvas, (column + 1).ToString(),
                    G1_VierGewinntLayout.Left + column * G1_VierGewinntLayout.Cell + 20,
                    G1_VierGewinntLayout.Top - 22,
                    14);
            }

            Rectangle board = new Rectangle
            {
                Width = G1_VierGewinntLogic.Columns * G1_VierGewinntLayout.Cell,
                Height = G1_VierGewinntLogic.Rows * G1_VierGewinntLayout.Cell,
                Fill = Brushes.RoyalBlue,
                Stroke = Brushes.DarkBlue,
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
                    Brush fill = player == 1 ? Brushes.Red
                        : player == 2 ? Brushes.Gold
                        : Brushes.WhiteSmoke;
                    Ellipse disc = new Ellipse
                    {
                        Width = G1_VierGewinntLayout.Cell - 10,
                        Height = G1_VierGewinntLayout.Cell - 10,
                        Fill = fill,
                        Stroke = Brushes.DarkSlateGray,
                        StrokeThickness = 1
                    };
                    Canvas.SetLeft(disc, G1_VierGewinntLayout.Left
                        + column * G1_VierGewinntLayout.Cell + 5);
                    Canvas.SetTop(disc, G1_VierGewinntLayout.Top
                        + row * G1_VierGewinntLayout.Cell + 5);
                    canvas.Children.Add(disc);
                }
            }

            int winner = G1_VierGewinntLogic.Winner(field);
            if (winner > 0)
            {
                AddText(canvas, "Spieler " + winner + " hat gewonnen!",
                    G1_VierGewinntLayout.Left, 30, 14);
            }
            else if (G1_VierGewinntLogic.IsFull(field))
            {
                AddText(canvas, "Unentschieden!",
                    G1_VierGewinntLayout.Left, 30, 14);
            }
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
                G1_VierGewinntLogic.Winner(connectFourField) > 0)
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
        private const int MaximumNodes = 250000;
        private const int WinScore = 100000;
        private static readonly int[] OrderedColumns = { 3, 2, 4, 1, 5, 0, 6 };
        private readonly Dictionary<SearchKey, SearchEntry> _transpositions =
            new Dictionary<SearchKey, SearchEntry>();
        private int _playerNumber;
        private int _nodes;
        private bool _searchAborted;

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
                G1_VierGewinntLogic.Winner(connectFourField) > 0)
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
                bestColumn = threatColumn;
            }

            if (bestColumn < 0)
            {
                bestColumn = OrderedColumns[0];
                _nodes = 0;
                _searchAborted = false;
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
                    ? WinScore
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
            if (_nodes++ >= MaximumNodes)
            {
                _searchAborted = true;
                return Evaluate(board, player);
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

            if (depth <= 0)
            {
                return Evaluate(board, player);
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
                    ? WinScore
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

        private static int Evaluate(int[,] board, int player)
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

            int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };
            for (int row = 0; row < G1_VierGewinntLogic.Rows; row++)
            {
                for (int column = 0; column < G1_VierGewinntLogic.Columns; column++)
                {
                    for (int direction = 0; direction < 4; direction++)
                    {
                        int endRow = row + 3 * directions[direction, 0];
                        int endColumn = column + 3 * directions[direction, 1];
                        if (!G1_VierGewinntLogic.IsInside(endRow, endColumn))
                        {
                            continue;
                        }

                        int own = 0;
                        int other = 0;
                        for (int offset = 0; offset < 4; offset++)
                        {
                            int cell = board[row + offset * directions[direction, 0],
                                column + offset * directions[direction, 1]];
                            if (cell == player)
                            {
                                own++;
                            }
                            else if (cell == opponent)
                            {
                                other++;
                            }
                        }

                        if (own > 0 && other > 0)
                        {
                            continue;
                        }

                        if (own > 0)
                        {
                            score += own == 3 ? 100 : own == 2 ? 10 : 1;
                        }
                        else if (other > 0)
                        {
                            score -= other == 3 ? 120 : other == 2 ? 12 : 1;
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
