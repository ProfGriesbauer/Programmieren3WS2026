using System;
using System.Collections.Generic;

namespace OOPGames
{
    internal enum G10_Tetromino
    {
        I,
        O,
        T,
        S,
        Z,
        J,
        L
    }

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

    internal static class G10_TetrominoShapes
    {
        static readonly int[][,] Shapes =
        {
            new[,] { { 0, 1 }, { 1, 1 }, { 2, 1 }, { 3, 1 } },
            new[,] { { 1, 0 }, { 2, 0 }, { 1, 1 }, { 2, 1 } },
            new[,] { { 1, 0 }, { 0, 1 }, { 1, 1 }, { 2, 1 } },
            new[,] { { 1, 0 }, { 2, 0 }, { 0, 1 }, { 1, 1 } },
            new[,] { { 0, 0 }, { 1, 0 }, { 1, 1 }, { 2, 1 } },
            new[,] { { 0, 0 }, { 0, 1 }, { 1, 1 }, { 2, 1 } },
            new[,] { { 2, 0 }, { 0, 1 }, { 1, 1 }, { 2, 1 } }
        };

        public static int CellX(G10_Tetromino piece, int index, int rotation)
        {
            int x = Shapes[(int)piece][index, 0];
            int y = Shapes[(int)piece][index, 1];
            for (int i = 0; i < rotation; i++)
            {
                int nextX = 3 - y;
                y = x;
                x = nextX;
            }
            return x;
        }

        public static int CellY(G10_Tetromino piece, int index, int rotation)
        {
            int x = Shapes[(int)piece][index, 0];
            int y = Shapes[(int)piece][index, 1];
            for (int i = 0; i < rotation; i++)
            {
                int nextX = 3 - y;
                y = x;
                x = nextX;
            }
            return y;
        }

        public static bool IsPreviewCell(G10_Tetromino piece, int x, int y)
        {
            for (int i = 0; i < 4; i++)
                if (Shapes[(int)piece][i, 0] == x && Shapes[(int)piece][i, 1] == y)
                    return true;
            return false;
        }
    }

    internal sealed class G10_TetrisSession
    {
        readonly byte[,] _Board = new byte[G10_TetrisConfig.Rows, G10_TetrisConfig.Columns];
        readonly List<G10_Tetromino> _Bag = new List<G10_Tetromino>();
        readonly Random _Random = new Random();
        G10_Tetromino _CurrentPiece;
        int _Rotation;
        int _X;
        int _Y;
        int _BagIndex;
        int _HorizontalDirection;
        float _HorizontalRepeatTimer;
        float _GravityTimer;
        int _Score;
        int _Lines;

        public bool IsGameOver { get; private set; }
        public int Score { get { return _Score; } }
        public int Lines { get { return _Lines; } }
        public int Level { get { return 1 + _Lines / G10_TetrisConfig.LinesPerLevel; } }
        public G10_Tetromino NextPiece { get; private set; }

        public void Start()
        {
            Array.Clear(_Board, 0, _Board.Length);
            _Bag.Clear();
            _BagIndex = 0;
            _Score = 0;
            _Lines = 0;
            _GravityTimer = 0;
            _HorizontalDirection = 0;
            _HorizontalRepeatTimer = 0;
            IsGameOver = false;
            NextPiece = DrawFromBag();
            SpawnPiece();
        }

        public void Update(float deltaSeconds, G10_TetrisInput input)
        {
            if (IsGameOver)
            {
                if (input.Confirm) Start();
                return;
            }

            UpdateHorizontal(input.Horizontal, deltaSeconds);

            if (input.Rotate) TryRotate();
            if (input.HardDrop)
            {
                HardDrop();
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
                if (!TryMove(0, 1))
                {
                    LockPiece();
                    break;
                }

                if (input.SoftDrop) _Score++;
            }
        }

        public byte GetCell(int x, int y)
        {
            if (x < 0 || x >= G10_TetrisConfig.Columns || y < 0 || y >= G10_TetrisConfig.Rows)
                return 0;

            if (!IsGameOver)
            {
                for (int i = 0; i < 4; i++)
                    if (_X + G10_TetrominoShapes.CellX(_CurrentPiece, i, _Rotation) == x &&
                        _Y + G10_TetrominoShapes.CellY(_CurrentPiece, i, _Rotation) == y)
                        return (byte)((int)_CurrentPiece + 1);
            }

            return _Board[y, x];
        }

        void UpdateHorizontal(float direction, float deltaSeconds)
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
                TryMove(_HorizontalDirection, 0);
                return;
            }

            _HorizontalRepeatTimer -= deltaSeconds;
            if (_HorizontalRepeatTimer <= 0)
            {
                TryMove(_HorizontalDirection, 0);
                _HorizontalRepeatTimer = G10_TetrisConfig.HorizontalRepeatInterval;
            }
        }

        void TryRotate()
        {
            int nextRotation = (_Rotation + 1) % 4;
            int[] horizontalKicks = { 0, -1, 1, -2, 2 };
            foreach (int kick in horizontalKicks)
            {
                if (!Collides(_X + kick, _Y, nextRotation))
                {
                    _X += kick;
                    _Rotation = nextRotation;
                    return;
                }
            }
        }

        void HardDrop()
        {
            int distance = 0;
            while (TryMove(0, 1)) distance++;
            _Score += distance * 2;
            LockPiece();
        }

        bool TryMove(int dx, int dy)
        {
            if (Collides(_X + dx, _Y + dy, _Rotation)) return false;
            _X += dx;
            _Y += dy;
            return true;
        }

        bool Collides(int x, int y, int rotation)
        {
            for (int i = 0; i < 4; i++)
            {
                int cellX = x + G10_TetrominoShapes.CellX(_CurrentPiece, i, rotation);
                int cellY = y + G10_TetrominoShapes.CellY(_CurrentPiece, i, rotation);
                if (cellX < 0 || cellX >= G10_TetrisConfig.Columns || cellY >= G10_TetrisConfig.Rows)
                    return true;
                if (cellY >= 0 && _Board[cellY, cellX] != 0) return true;
            }
            return false;
        }

        void LockPiece()
        {
            for (int i = 0; i < 4; i++)
            {
                int x = _X + G10_TetrominoShapes.CellX(_CurrentPiece, i, _Rotation);
                int y = _Y + G10_TetrominoShapes.CellY(_CurrentPiece, i, _Rotation);
                if (y < 0)
                {
                    IsGameOver = true;
                    return;
                }
                _Board[y, x] = (byte)((int)_CurrentPiece + 1);
            }

            ClearLines();
            _CurrentPiece = NextPiece;
            NextPiece = DrawFromBag();
            _X = 3;
            _Y = 0;
            _Rotation = 0;
            _GravityTimer = 0;
            if (Collides(_X, _Y, _Rotation)) IsGameOver = true;
        }

        void ClearLines()
        {
            int cleared = 0;
            for (int row = G10_TetrisConfig.Rows - 1; row >= 0; row--)
            {
                bool full = true;
                for (int column = 0; column < G10_TetrisConfig.Columns; column++)
                    if (_Board[row, column] == 0) { full = false; break; }

                if (!full) continue;
                cleared++;
                for (int copyRow = row; copyRow > 0; copyRow--)
                    for (int column = 0; column < G10_TetrisConfig.Columns; column++)
                        _Board[copyRow, column] = _Board[copyRow - 1, column];
                for (int column = 0; column < G10_TetrisConfig.Columns; column++)
                    _Board[0, column] = 0;
                row++;
            }

            int[] lineScores = { 0, 100, 300, 500, 800 };
            _Lines += cleared;
            _Score += lineScores[cleared] * Level;
        }

        G10_Tetromino DrawFromBag()
        {
            if (_BagIndex >= _Bag.Count)
            {
                _Bag.Clear();
                foreach (G10_Tetromino piece in Enum.GetValues(typeof(G10_Tetromino)))
                    _Bag.Add(piece);
                for (int i = _Bag.Count - 1; i > 0; i--)
                {
                    int swapIndex = _Random.Next(i + 1);
                    G10_Tetromino piece = _Bag[i];
                    _Bag[i] = _Bag[swapIndex];
                    _Bag[swapIndex] = piece;
                }
                _BagIndex = 0;
            }
            return _Bag[_BagIndex++];
        }

        void SpawnPiece()
        {
            _CurrentPiece = NextPiece;
            NextPiece = DrawFromBag();
            _X = 3;
            _Y = 0;
            _Rotation = 0;
            if (Collides(_X, _Y, _Rotation)) IsGameOver = true;
        }
    }
}
