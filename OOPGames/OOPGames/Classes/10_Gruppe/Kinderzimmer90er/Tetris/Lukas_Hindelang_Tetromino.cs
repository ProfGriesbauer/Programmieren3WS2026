using System.Collections.Generic;

namespace OOPGames
{
    // The seven shapes. The model only knows the kind; the renderer chooses the colour.
    internal enum G10_TetrominoKind
    {
        I,
        O,
        T,
        S,
        Z,
        J,
        L
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

    // SRS wall kicks (Super Rotation System). The offsets are given like in the SRS
    // tables (y up) for the four clockwise turns and stored with y pointing down.
    // A counter-clockwise turn A -> B uses the negated offsets of the clockwise turn B -> A.
    internal sealed class G10_TetrisKickTable
    {
        public static readonly G10_TetrisKickTable Standard = new G10_TetrisKickTable(new[]
        {
            new[] { 0, 0, -1, 0, -1, 1, 0, -2, -1, -2 },
            new[] { 0, 0, 1, 0, 1, -1, 0, 2, 1, 2 },
            new[] { 0, 0, 1, 0, 1, 1, 0, -2, 1, -2 },
            new[] { 0, 0, -1, 0, -1, -1, 0, 2, -1, 2 }
        });

        public static readonly G10_TetrisKickTable IPiece = new G10_TetrisKickTable(new[]
        {
            new[] { 0, 0, -2, 0, 1, 0, -2, -1, 1, 2 },
            new[] { 0, 0, -1, 0, 2, 0, -1, 2, 2, -1 },
            new[] { 0, 0, 2, 0, -1, 0, 2, 1, -1, -2 },
            new[] { 0, 0, 1, 0, -2, 0, 1, -2, -2, 1 }
        });

        readonly G10_TetrominoCell[][] _Clockwise = new G10_TetrominoCell[4][];
        readonly G10_TetrominoCell[][] _CounterClockwise = new G10_TetrominoCell[4][];

        G10_TetrisKickTable(int[][] clockwiseYUp)
        {
            for (int from = 0; from < 4; from++)
            {
                int[] pairs = clockwiseYUp[from];
                int to = (from + 1) % 4;
                _Clockwise[from] = new G10_TetrominoCell[pairs.Length / 2];
                _CounterClockwise[to] = new G10_TetrominoCell[pairs.Length / 2];
                for (int i = 0; i < pairs.Length / 2; i++)
                {
                    _Clockwise[from][i] = new G10_TetrominoCell(pairs[i * 2], -pairs[i * 2 + 1]);
                    _CounterClockwise[to][i] = new G10_TetrominoCell(-pairs[i * 2], pairs[i * 2 + 1]);
                }
            }
        }

        public IReadOnlyList<G10_TetrominoCell> Offsets(int fromRotation, bool clockwise)
        {
            return clockwise ? _Clockwise[fromRotation] : _CounterClockwise[fromRotation];
        }
    }

    // One instance per shape is shared by the bag, the board and the falling piece.
    // The four rotation states are turned clockwise inside the shape's bounding box once.
    internal abstract class G10_Tetromino
    {
        readonly G10_TetrominoCell[][] _Rotations = new G10_TetrominoCell[4][];

        protected G10_Tetromino(int boxSize, params G10_TetrominoCell[] spawnCells)
        {
            BoxSize = boxSize;
            _Rotations[0] = spawnCells;
            for (int rotation = 1; rotation < 4; rotation++)
            {
                G10_TetrominoCell[] previous = _Rotations[rotation - 1];
                G10_TetrominoCell[] turned = new G10_TetrominoCell[previous.Length];
                for (int i = 0; i < previous.Length; i++)
                    turned[i] = new G10_TetrominoCell(boxSize - 1 - previous[i].Y, previous[i].X);
                _Rotations[rotation] = turned;
            }
        }

        public int BoxSize { get; }
        public abstract G10_TetrominoKind Kind { get; }
        public string Name { get { return Kind.ToString(); } }
        public virtual bool CanRotate { get { return true; } }
        public virtual G10_TetrisKickTable Kicks { get { return G10_TetrisKickTable.Standard; } }

        public IReadOnlyList<G10_TetrominoCell> Cells(int rotation)
        {
            return _Rotations[rotation];
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

        protected static G10_TetrominoCell C(int x, int y)
        {
            return new G10_TetrominoCell(x, y);
        }
    }

    internal sealed class G10_ITetromino : G10_Tetromino
    {
        public G10_ITetromino() : base(4, C(0, 1), C(1, 1), C(2, 1), C(3, 1)) { }
        public override G10_TetrominoKind Kind { get { return G10_TetrominoKind.I; } }
        public override G10_TetrisKickTable Kicks { get { return G10_TetrisKickTable.IPiece; } }
    }

    internal sealed class G10_OTetromino : G10_Tetromino
    {
        public G10_OTetromino() : base(4, C(1, 0), C(2, 0), C(1, 1), C(2, 1)) { }
        public override G10_TetrominoKind Kind { get { return G10_TetrominoKind.O; } }
        public override bool CanRotate { get { return false; } }
    }

    internal sealed class G10_TTetromino : G10_Tetromino
    {
        public G10_TTetromino() : base(3, C(1, 0), C(0, 1), C(1, 1), C(2, 1)) { }
        public override G10_TetrominoKind Kind { get { return G10_TetrominoKind.T; } }
    }

    internal sealed class G10_STetromino : G10_Tetromino
    {
        public G10_STetromino() : base(3, C(1, 0), C(2, 0), C(0, 1), C(1, 1)) { }
        public override G10_TetrominoKind Kind { get { return G10_TetrominoKind.S; } }
    }

    internal sealed class G10_ZTetromino : G10_Tetromino
    {
        public G10_ZTetromino() : base(3, C(0, 0), C(1, 0), C(1, 1), C(2, 1)) { }
        public override G10_TetrominoKind Kind { get { return G10_TetrominoKind.Z; } }
    }

    internal sealed class G10_JTetromino : G10_Tetromino
    {
        public G10_JTetromino() : base(3, C(0, 0), C(0, 1), C(1, 1), C(2, 1)) { }
        public override G10_TetrominoKind Kind { get { return G10_TetrominoKind.J; } }
    }

    internal sealed class G10_LTetromino : G10_Tetromino
    {
        public G10_LTetromino() : base(3, C(2, 0), C(0, 1), C(1, 1), C(2, 1)) { }
        public override G10_TetrominoKind Kind { get { return G10_TetrominoKind.L; } }
    }
}
