using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Moves the falling piece: auto shift, SRS rotation, gravity and lock delay.
    internal sealed class G10_TetrisPieceController
    {
        readonly G10_TetrisBoard _Board;
        int _ShiftDirection;
        float _ShiftTimer;
        float _GravityTimer;
        float _LockTimer;
        int _LockResets;
        int _LowestY;

        public G10_TetrisPieceController(G10_TetrisBoard board)
        {
            if (board == null) throw new ArgumentNullException("board");
            _Board = board;
        }

        public G10_FallingPiece Piece { get; private set; }
        public float LockProgress { get { return Math.Min(1, _LockTimer / G10_TetrisConfig.LockDelay); } }
        public int GhostY { get { return Piece == null ? 0 : Piece.Y + _Board.DropDistance(Piece); } }

        bool IsGrounded
        {
            get { return !_Board.CanPlace(Piece.Shape, Piece.X, Piece.Y + 1, Piece.Rotation); }
        }

        // The lowest row of the spawn rotation sits in the first visible row; one row higher
        // if that is blocked. Returns false if the piece does not fit at all (block out).
        public bool Spawn(G10_Tetromino shape)
        {
            int bottom = 0;
            foreach (G10_TetrominoCell cell in shape.Cells(0))
                bottom = Math.Max(bottom, cell.Y);

            int x = (G10_TetrisConfig.Columns - shape.BoxSize) / 2;
            int y = G10_TetrisConfig.HiddenRows - bottom;
            if (!_Board.CanPlace(shape, x, y, 0)) y--;
            Piece = new G10_FallingPiece(shape, x, y);
            _GravityTimer = 0;
            _LockTimer = 0;
            _LockResets = 0;
            _LowestY = y;
            return _Board.CanPlace(shape, x, y, 0);
        }

        public void Remove()
        {
            Piece = null;
        }

        public void ResetShift()
        {
            _ShiftDirection = 0;
            _ShiftTimer = 0;
        }

        // A fresh press moves at once; holding the key repeats after a short delay.
        public void Shift(int pressedDirection, int heldDirection, float deltaSeconds)
        {
            if (pressedDirection != 0)
            {
                _ShiftDirection = pressedDirection;
                _ShiftTimer = G10_TetrisConfig.AutoShiftDelay;
                TryMove(pressedDirection, 0);
                return;
            }

            if (heldDirection == 0)
            {
                ResetShift();
                return;
            }

            if (heldDirection != _ShiftDirection)
            {
                _ShiftDirection = heldDirection;
                _ShiftTimer = G10_TetrisConfig.AutoShiftDelay;
                return;
            }

            _ShiftTimer -= deltaSeconds;
            while (_ShiftTimer <= 0)
            {
                _ShiftTimer += G10_TetrisConfig.AutoShiftInterval;
                if (!TryMove(_ShiftDirection, 0)) break;
            }
        }

        public bool Rotate(bool clockwise)
        {
            if (!Piece.Shape.CanRotate) return false;

            int target = (Piece.Rotation + (clockwise ? 1 : 3)) % 4;
            foreach (G10_TetrominoCell kick in Piece.Shape.Kicks.Offsets(Piece.Rotation, clockwise))
            {
                int x = Piece.X + kick.X;
                int y = Piece.Y + kick.Y;
                if (!_Board.CanPlace(Piece.Shape, x, y, target)) continue;
                Piece.Place(x, y, target);
                OnMoved();
                return true;
            }
            return false;
        }

        public int HardDrop()
        {
            int distance = _Board.DropDistance(Piece);
            Piece.Place(Piece.X, Piece.Y + distance, Piece.Rotation);
            return distance;
        }

        // Returns true once the piece has rested on the ground for the whole lock delay.
        public bool ApplyGravity(float deltaSeconds, bool softDrop, int level, out int softDropRows)
        {
            softDropRows = 0;
            float interval = G10_TetrisConfig.DropInterval(level);
            if (softDrop) interval = Math.Min(interval, G10_TetrisConfig.SoftDropInterval);

            _GravityTimer += deltaSeconds;
            while (_GravityTimer >= interval)
            {
                _GravityTimer -= interval;
                if (!TryMove(0, 1))
                {
                    _GravityTimer = 0;
                    break;
                }
                if (softDrop) softDropRows++;
            }

            if (!IsGrounded)
            {
                _LockTimer = 0;
                return false;
            }

            _LockTimer += deltaSeconds;
            return _LockTimer >= G10_TetrisConfig.LockDelay;
        }

        bool TryMove(int dx, int dy)
        {
            int x = Piece.X + dx;
            int y = Piece.Y + dy;
            if (!_Board.CanPlace(Piece.Shape, x, y, Piece.Rotation))
                return false;
            Piece.Place(x, y, Piece.Rotation);
            OnMoved();
            return true;
        }

        // Move reset: moving or turning a grounded piece restarts the lock delay, but only
        // MaxLockResets times per row, so a piece cannot be kept alive forever.
        void OnMoved()
        {
            if (Piece.Y > _LowestY)
            {
                _LowestY = Piece.Y;
                _LockResets = 0;
                _LockTimer = 0;
                return;
            }

            if (_LockTimer > 0 && _LockResets < G10_TetrisConfig.MaxLockResets)
            {
                _LockResets++;
                _LockTimer = 0;
            }
        }
    }
}
