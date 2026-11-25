using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Read-only view of the falling piece for the renderer and the effects; only the
    // piece controller may move it.
    internal interface IG10_FallingPieceView
    {
        G10_Tetromino Shape { get; }
        int X { get; }
        int Y { get; }
        int Rotation { get; }
        IReadOnlyList<G10_TetrominoCell> Cells { get; }
    }

    internal sealed class G10_FallingPiece : IG10_FallingPieceView
    {
        public G10_FallingPiece(G10_Tetromino shape, int x, int y)
        {
            if (shape == null) throw new ArgumentNullException("shape");
            Shape = shape;
            X = x;
            Y = y;
        }

        public G10_Tetromino Shape { get; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Rotation { get; private set; }
        public IReadOnlyList<G10_TetrominoCell> Cells { get { return Shape.Cells(Rotation); } }

        public void Place(int x, int y, int rotation)
        {
            X = x;
            Y = y;
            Rotation = rotation;
        }
    }

    // Row 0 is the top. The first HiddenRows rows lie above the visible well; pieces
    // spawn there, and a piece that locks completely inside them ends the game.
    internal sealed class G10_TetrisBoard
    {
        readonly G10_Tetromino[,] _Cells =
            new G10_Tetromino[G10_TetrisConfig.Rows, G10_TetrisConfig.Columns];

        public G10_Tetromino this[int column, int row]
        {
            get
            {
                if (!IsInside(column, row)) return null;
                return _Cells[row, column];
            }
        }

        public void Clear()
        {
            Array.Clear(_Cells, 0, _Cells.Length);
        }

        public bool CanPlace(G10_Tetromino shape, int x, int y, int rotation)
        {
            foreach (G10_TetrominoCell cell in shape.Cells(rotation))
            {
                int column = x + cell.X;
                int row = y + cell.Y;
                if (!IsInside(column, row) || _Cells[row, column] != null)
                    return false;
            }
            return true;
        }

        public int DropDistance(G10_FallingPiece piece)
        {
            int distance = 0;
            while (CanPlace(piece.Shape, piece.X, piece.Y + distance + 1, piece.Rotation))
                distance++;
            return distance;
        }

        // Returns false (lock out) if no cell of the piece reaches the visible well.
        public bool Lock(G10_FallingPiece piece)
        {
            bool visible = false;
            foreach (G10_TetrominoCell cell in piece.Cells)
            {
                _Cells[piece.Y + cell.Y, piece.X + cell.X] = piece.Shape;
                if (piece.Y + cell.Y >= G10_TetrisConfig.HiddenRows) visible = true;
            }
            return visible;
        }

        public IReadOnlyList<int> FindFullRows()
        {
            List<int> rows = new List<int>();
            for (int row = 0; row < G10_TetrisConfig.Rows; row++)
                if (IsRowFull(row)) rows.Add(row);
            return rows;
        }

        public void RemoveRows(IReadOnlyCollection<int> rows)
        {
            int target = G10_TetrisConfig.Rows - 1;
            for (int source = G10_TetrisConfig.Rows - 1; source >= 0; source--)
            {
                if (Contains(rows, source)) continue;
                if (target != source)
                    for (int column = 0; column < G10_TetrisConfig.Columns; column++)
                        _Cells[target, column] = _Cells[source, column];
                target--;
            }

            for (int row = target; row >= 0; row--)
                for (int column = 0; column < G10_TetrisConfig.Columns; column++)
                    _Cells[row, column] = null;
        }

        bool IsRowFull(int row)
        {
            for (int column = 0; column < G10_TetrisConfig.Columns; column++)
                if (_Cells[row, column] == null) return false;
            return true;
        }

        static bool Contains(IReadOnlyCollection<int> rows, int row)
        {
            foreach (int candidate in rows)
                if (candidate == row) return true;
            return false;
        }

        static bool IsInside(int column, int row)
        {
            return column >= 0 && column < G10_TetrisConfig.Columns &&
                   row >= 0 && row < G10_TetrisConfig.Rows;
        }
    }
}
