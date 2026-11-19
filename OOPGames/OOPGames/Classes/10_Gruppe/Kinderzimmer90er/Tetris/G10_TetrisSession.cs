using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace OOPGames
{
    internal static class G10_TetrisConfig
    {
        public const int Columns = 10;
        public const int Rows = 20;
        public const float FixedDeltaSeconds = 0.04f;
        public const float HorizontalRepeatDelay = 0.2f;
        public const float HorizontalRepeatInterval = 0.1f;
        public const float BaseDropInterval = 0.65f;
        public const float SoftDropInterval = 0.08f;
        public const float MinimumDropInterval = 0.08f;
        public const int LinesPerLevel = 10;
    }

    internal readonly struct G10_TetrominoCell
    {
        public G10_TetrominoCell(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }

    internal abstract class G10_Tetromino
    {
        protected abstract G10_TetrominoCell[] Cells { get; }
        public abstract Color Color { get; }
        public virtual int NextRotation(int currentRotation)
        {
            return (currentRotation + 1) % 4;
        }

        public int CellX(int index, int rotation)
        {
            G10_TetrominoCell cell = Cells[index];
            for (int i = 0; i < rotation; i++)
                cell = new G10_TetrominoCell(3 - cell.Y, cell.X);
            return cell.X;
        }

        public int CellY(int index, int rotation)
        {
            G10_TetrominoCell cell = Cells[index];
            for (int i = 0; i < rotation; i++)
                cell = new G10_TetrominoCell(3 - cell.Y, cell.X);
            return cell.Y;
        }

        public bool ContainsCell(int x, int y)
        {
            for (int i = 0; i < Cells.Length; i++)
                if (Cells[i].X == x && Cells[i].Y == y)
                    return true;
            return false;
        }

        public static IReadOnlyList<G10_Tetromino> CreateAll()
        {
            return new G10_Tetromino[]
            {
                new G10_ITetromino(),
                new G10_OTetromino(),
                new G10_TTetromino(),
                new G10_STetromino(),
                new G10_ZTetromino(),
                new G10_JTetromino(),
                new G10_LTetromino()
            };
        }
    }

    internal sealed class G10_ITetromino : G10_Tetromino
    {
        static readonly G10_TetrominoCell[] Shape =
        {
            new G10_TetrominoCell(0, 1), new G10_TetrominoCell(1, 1),
            new G10_TetrominoCell(2, 1), new G10_TetrominoCell(3, 1)
        };
        protected override G10_TetrominoCell[] Cells { get { return Shape; } }
        public override Color Color { get { return System.Windows.Media.Color.FromRgb(62, 215, 224); } }
    }

    internal sealed class G10_OTetromino : G10_Tetromino
    {
        static readonly G10_TetrominoCell[] Shape =
        {
            new G10_TetrominoCell(1, 0), new G10_TetrominoCell(2, 0),
            new G10_TetrominoCell(1, 1), new G10_TetrominoCell(2, 1)
        };
        protected override G10_TetrominoCell[] Cells { get { return Shape; } }
        public override Color Color { get { return System.Windows.Media.Color.FromRgb(239, 209, 94); } }
        public override int NextRotation(int currentRotation) { return currentRotation; }
    }

    internal sealed class G10_TTetromino : G10_Tetromino
    {
        static readonly G10_TetrominoCell[] Shape =
        {
            new G10_TetrominoCell(1, 0), new G10_TetrominoCell(0, 1),
            new G10_TetrominoCell(1, 1), new G10_TetrominoCell(2, 1)
        };
        protected override G10_TetrominoCell[] Cells { get { return Shape; } }
        public override Color Color { get { return System.Windows.Media.Color.FromRgb(180, 119, 222); } }
    }

    internal sealed class G10_STetromino : G10_Tetromino
    {
        static readonly G10_TetrominoCell[] Shape =
        {
            new G10_TetrominoCell(1, 0), new G10_TetrominoCell(2, 0),
            new G10_TetrominoCell(0, 1), new G10_TetrominoCell(1, 1)
        };
        protected override G10_TetrominoCell[] Cells { get { return Shape; } }
        public override Color Color { get { return System.Windows.Media.Color.FromRgb(115, 214, 143); } }
    }

    internal sealed class G10_ZTetromino : G10_Tetromino
    {
        static readonly G10_TetrominoCell[] Shape =
        {
            new G10_TetrominoCell(0, 0), new G10_TetrominoCell(1, 0),
            new G10_TetrominoCell(1, 1), new G10_TetrominoCell(2, 1)
        };
        protected override G10_TetrominoCell[] Cells { get { return Shape; } }
        public override Color Color { get { return System.Windows.Media.Color.FromRgb(235, 105, 117); } }
    }

    internal sealed class G10_JTetromino : G10_Tetromino
    {
        static readonly G10_TetrominoCell[] Shape =
        {
            new G10_TetrominoCell(0, 0), new G10_TetrominoCell(0, 1),
            new G10_TetrominoCell(1, 1), new G10_TetrominoCell(2, 1)
        };
        protected override G10_TetrominoCell[] Cells { get { return Shape; } }
        public override Color Color { get { return System.Windows.Media.Color.FromRgb(105, 145, 232); } }
    }

    internal sealed class G10_LTetromino : G10_Tetromino
    {
        static readonly G10_TetrominoCell[] Shape =
        {
            new G10_TetrominoCell(2, 0), new G10_TetrominoCell(0, 1),
            new G10_TetrominoCell(1, 1), new G10_TetrominoCell(2, 1)
        };
        protected override G10_TetrominoCell[] Cells { get { return Shape; } }
        public override Color Color { get { return System.Windows.Media.Color.FromRgb(236, 157, 83); } }
    }

    internal sealed class G10_FallingPiece
    {
        public G10_FallingPiece(G10_Tetromino shape, int x, int y)
        {
            Shape = shape;
            X = x;
            Y = y;
        }

        public G10_Tetromino Shape { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Rotation { get; private set; }

        public int CellX(int index) { return X + Shape.CellX(index, Rotation); }
        public int CellY(int index) { return Y + Shape.CellY(index, Rotation); }
        public bool ContainsCell(int x, int y)
        {
            for (int i = 0; i < 4; i++)
                if (CellX(i) == x && CellY(i) == y)
                    return true;
            return false;
        }

        public void MoveTo(int x, int y) { X = x; Y = y; }
        public void SetRotation(int rotation) { Rotation = rotation; }
    }

    internal sealed class G10_TetrominoBag
    {
        readonly Random _Random = new Random();
        readonly List<G10_Tetromino> _Pieces = new List<G10_Tetromino>();
        readonly IReadOnlyList<G10_Tetromino> _AllPieces = G10_Tetromino.CreateAll();
        int _NextIndex;

        public void Reset()
        {
            _Pieces.Clear();
            _NextIndex = 0;
        }

        public G10_Tetromino Draw()
        {
            if (_NextIndex >= _Pieces.Count)
                Refill();
            return _Pieces[_NextIndex++];
        }

        void Refill()
        {
            _Pieces.Clear();
            _Pieces.AddRange(_AllPieces);
            for (int i = _Pieces.Count - 1; i > 0; i--)
            {
                int swapIndex = _Random.Next(i + 1);
                G10_Tetromino piece = _Pieces[i];
                _Pieces[i] = _Pieces[swapIndex];
                _Pieces[swapIndex] = piece;
            }
            _NextIndex = 0;
        }
    }

    internal sealed class G10_TetrisBoard
    {
        readonly G10_Tetromino[,] _Cells =
            new G10_Tetromino[G10_TetrisConfig.Rows, G10_TetrisConfig.Columns];

        public void Clear()
        {
            Array.Clear(_Cells, 0, _Cells.Length);
        }

        public G10_Tetromino GetCell(int x, int y, G10_FallingPiece activePiece, bool includeActivePiece)
        {
            if (x < 0 || x >= G10_TetrisConfig.Columns || y < 0 || y >= G10_TetrisConfig.Rows)
                return null;
            if (includeActivePiece && activePiece != null && activePiece.ContainsCell(x, y))
                return activePiece.Shape;
            return _Cells[y, x];
        }

        public bool CanPlace(G10_FallingPiece piece, int x, int y, int rotation)
        {
            for (int i = 0; i < 4; i++)
            {
                int cellX = x + piece.Shape.CellX(i, rotation);
                int cellY = y + piece.Shape.CellY(i, rotation);
                if (cellX < 0 || cellX >= G10_TetrisConfig.Columns || cellY >= G10_TetrisConfig.Rows)
                    return false;
                if (cellY >= 0 && _Cells[cellY, cellX] != null)
                    return false;
            }
            return true;
        }

        public bool Lock(G10_FallingPiece piece)
        {
            for (int i = 0; i < 4; i++)
                if (piece.CellY(i) < 0)
                    return false;

            for (int i = 0; i < 4; i++)
                _Cells[piece.CellY(i), piece.CellX(i)] = piece.Shape;
            return true;
        }

        public int ClearFullLines()
        {
            int cleared = 0;
            for (int row = G10_TetrisConfig.Rows - 1; row >= 0; row--)
            {
                bool full = true;
                for (int column = 0; column < G10_TetrisConfig.Columns; column++)
                    if (_Cells[row, column] == null) { full = false; break; }

                if (!full) continue;
                cleared++;
                for (int copyRow = row; copyRow > 0; copyRow--)
                    for (int column = 0; column < G10_TetrisConfig.Columns; column++)
                        _Cells[copyRow, column] = _Cells[copyRow - 1, column];
                for (int column = 0; column < G10_TetrisConfig.Columns; column++)
                    _Cells[0, column] = null;
                row++;
            }
            return cleared;
        }
    }

    internal sealed class G10_TetrisScoreKeeper
    {
        static readonly int[] LineScores = { 0, 100, 300, 500, 800 };
        int _Score;
        int _Lines;

        public int Score { get { return _Score; } }
        public int Lines { get { return _Lines; } }
        public int Level { get { return 1 + _Lines / G10_TetrisConfig.LinesPerLevel; } }

        public void Reset()
        {
            _Score = 0;
            _Lines = 0;
        }

        public void AddSoftDropPoint() { _Score++; }
        public void AddHardDropPoints(int distance) { _Score += distance * 2; }

        public void AddClearedLines(int count)
        {
            _Lines += count;
            _Score += LineScores[count] * Level;
        }
    }

    internal sealed class G10_TetrisPieceController
    {
        static readonly int[] HorizontalKicks = { 0, -1, 1, -2, 2 };
        float _HorizontalRepeatTimer;
        int _HorizontalDirection;

        public void Reset()
        {
            _HorizontalDirection = 0;
            _HorizontalRepeatTimer = 0;
        }

        public void UpdateHorizontal(
            G10_TetrisBoard board,
            G10_FallingPiece piece,
            float direction,
            float deltaSeconds)
        {
            int nextDirection = Math.Sign(direction);
            if (nextDirection == 0)
            {
                _HorizontalDirection = 0;
                _HorizontalRepeatTimer = 0;
                return;
            }

            if (nextDirection != _HorizontalDirection)
            {
                _HorizontalDirection = nextDirection;
                _HorizontalRepeatTimer = G10_TetrisConfig.HorizontalRepeatDelay;
                TryMove(board, piece, _HorizontalDirection, 0);
                return;
            }

            _HorizontalRepeatTimer -= deltaSeconds;
            if (_HorizontalRepeatTimer <= 0)
            {
                TryMove(board, piece, _HorizontalDirection, 0);
                _HorizontalRepeatTimer = G10_TetrisConfig.HorizontalRepeatInterval;
            }
        }

        public void TryRotate(G10_TetrisBoard board, G10_FallingPiece piece)
        {
            int nextRotation = piece.Shape.NextRotation(piece.Rotation);
            if (nextRotation == piece.Rotation) return;
            foreach (int kick in HorizontalKicks)
            {
                int nextX = piece.X + kick;
                if (!board.CanPlace(piece, nextX, piece.Y, nextRotation)) continue;
                piece.MoveTo(nextX, piece.Y);
                piece.SetRotation(nextRotation);
                return;
            }
        }

        public int HardDrop(G10_TetrisBoard board, G10_FallingPiece piece)
        {
            int distance = 0;
            while (TryMove(board, piece, 0, 1)) distance++;
            return distance;
        }

        public bool TryMove(G10_TetrisBoard board, G10_FallingPiece piece, int dx, int dy)
        {
            int nextX = piece.X + dx;
            int nextY = piece.Y + dy;
            if (!board.CanPlace(piece, nextX, nextY, piece.Rotation))
                return false;
            piece.MoveTo(nextX, nextY);
            return true;
        }
    }

    internal sealed class G10_TetrisSession
    {
        readonly G10_TetrisBoard _Board = new G10_TetrisBoard();
        readonly G10_TetrominoBag _Bag = new G10_TetrominoBag();
        readonly G10_TetrisScoreKeeper _ScoreKeeper = new G10_TetrisScoreKeeper();
        readonly G10_TetrisPieceController _PieceController = new G10_TetrisPieceController();
        G10_FallingPiece _CurrentPiece;
        float _GravityTimer;

        public bool IsGameOver { get; private set; }
        public int Score { get { return _ScoreKeeper.Score; } }
        public int Lines { get { return _ScoreKeeper.Lines; } }
        public int Level { get { return _ScoreKeeper.Level; } }
        public G10_Tetromino NextPiece { get; private set; }
        public G10_FallingPiece CurrentPiece { get { return _CurrentPiece; } }

        public void Start()
        {
            _Board.Clear();
            _Bag.Reset();
            _ScoreKeeper.Reset();
            _GravityTimer = 0;
            _PieceController.Reset();
            IsGameOver = false;
            NextPiece = _Bag.Draw();
            SpawnPiece();
        }

        public void Update(float deltaSeconds, G10_TetrisInput input)
        {
            if (IsGameOver)
            {
                if (input.Confirm) Start();
                return;
            }

            _PieceController.UpdateHorizontal(_Board, _CurrentPiece, input.Horizontal, deltaSeconds);
            if (input.Rotate) _PieceController.TryRotate(_Board, _CurrentPiece);
            if (input.HardDrop)
            {
                _ScoreKeeper.AddHardDropPoints(
                    _PieceController.HardDrop(_Board, _CurrentPiece));
                LockPiece();
                return;
            }

            float dropInterval = input.SoftDrop
                ? G10_TetrisConfig.SoftDropInterval
                : Math.Max(G10_TetrisConfig.MinimumDropInterval,
                    G10_TetrisConfig.BaseDropInterval * (float)Math.Pow(0.82, Level - 1));

            _GravityTimer += deltaSeconds;
            while (_GravityTimer >= dropInterval)
            {
                _GravityTimer -= dropInterval;
                if (!_PieceController.TryMove(_Board, _CurrentPiece, 0, 1))
                {
                    LockPiece();
                    break;
                }

                if (input.SoftDrop) _ScoreKeeper.AddSoftDropPoint();
            }
        }

        public G10_Tetromino GetCell(int x, int y)
        {
            return _Board.GetCell(x, y, _CurrentPiece, !IsGameOver);
        }

        void LockPiece()
        {
            if (!_Board.Lock(_CurrentPiece))
            {
                IsGameOver = true;
                return;
            }

            _ScoreKeeper.AddClearedLines(_Board.ClearFullLines());
            SpawnPiece();
            _GravityTimer = 0;
        }

        void SpawnPiece()
        {
            _CurrentPiece = new G10_FallingPiece(NextPiece, 3, 0);
            NextPiece = _Bag.Draw();
            if (!_Board.CanPlace(_CurrentPiece, _CurrentPiece.X, _CurrentPiece.Y, _CurrentPiece.Rotation))
                IsGameOver = true;
        }
    }
}
