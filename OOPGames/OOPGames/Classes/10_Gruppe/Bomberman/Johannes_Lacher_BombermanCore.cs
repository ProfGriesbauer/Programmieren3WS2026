using System;

namespace OOPGames
{
    public static class G10_BomberConstants
    {
        public const int TileSize = 16;
        public const int Columns = 15;
        public const int Rows = 13;
        // Positions are stored in sub-pixels so slow speeds stay exact with whole numbers.
        public const int SubPixels = 8;
        public const int TileSubPixels = TileSize * SubPixels;
        public const int TicksPerSecond = 25;

        public static int Seconds(double seconds)
        {
            return (int)Math.Round(seconds * TicksPerSecond);
        }
    }

    public struct G10_BomberCell : IEquatable<G10_BomberCell>
    {
        public G10_BomberCell(int col, int row)
        {
            Col = col;
            Row = row;
        }

        public int Col { get; private set; }
        public int Row { get; private set; }

        public int CenterX { get { return Col * G10_BomberConstants.TileSize + G10_BomberConstants.TileSize / 2; } }
        public int CenterY { get { return Row * G10_BomberConstants.TileSize + G10_BomberConstants.TileSize / 2; } }

        public G10_BomberCell Neighbour(G10_Direction direction)
        {
            return new G10_BomberCell(Col + direction.DeltaX(), Row + direction.DeltaY());
        }

        public int ManhattanTo(G10_BomberCell other)
        {
            return Math.Abs(Col - other.Col) + Math.Abs(Row - other.Row);
        }

        public bool Equals(G10_BomberCell other) { return Col == other.Col && Row == other.Row; }
        public override bool Equals(object obj) { return obj is G10_BomberCell && Equals((G10_BomberCell)obj); }
        public override int GetHashCode() { return Col * 397 ^ Row; }
        public static bool operator ==(G10_BomberCell a, G10_BomberCell b) { return a.Equals(b); }
        public static bool operator !=(G10_BomberCell a, G10_BomberCell b) { return !a.Equals(b); }
        public override string ToString() { return "(" + Col + ", " + Row + ")"; }
    }

    // Input of one tick, already free of keys.
    public struct G10_BomberInput
    {
        public static readonly G10_BomberInput Neutral = new G10_BomberInput(G10_Direction.None, false, false, false);

        public G10_BomberInput(G10_Direction move, bool placeBomb, bool detonate, bool confirm)
        {
            Move = move;
            PlaceBomb = placeBomb;
            Detonate = detonate;
            Confirm = confirm;
        }

        // Direction held down right now; None stands still.
        public G10_Direction Move { get; private set; }
        public bool PlaceBomb { get; private set; }
        public bool Detonate { get; private set; }
        // Pause, resume, new game after GAME OVER.
        public bool Confirm { get; private set; }
    }

    // Score shown for a moment where an enemy died.
    public struct G10_BomberPopup
    {
        public G10_BomberPopup(int pixelX, int pixelY, int points, int ticksLeft)
        {
            PixelX = pixelX;
            PixelY = pixelY;
            Points = points;
            TicksLeft = ticksLeft;
        }

        public int PixelX { get; private set; }
        public int PixelY { get; private set; }
        public int Points { get; private set; }
        public int TicksLeft { get; private set; }

        public G10_BomberPopup Aged()
        {
            return new G10_BomberPopup(PixelX, PixelY - (TicksLeft % 3 == 0 ? 1 : 0), Points, TicksLeft - 1);
        }
    }
}
