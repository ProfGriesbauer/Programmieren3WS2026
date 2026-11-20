namespace OOPGames
{
    public enum G10_Direction
    {
        None,
        Up,
        Left,
        Down,
        Right
    }

    public static class G10_DirectionExtensions
    {
        // Fixed order for checking all directions; Pac-Man ghosts also break ties with it.
        public static readonly G10_Direction[] TieBreakOrder =
        {
            G10_Direction.Up, G10_Direction.Left, G10_Direction.Down, G10_Direction.Right
        };

        public static int DeltaX(this G10_Direction direction)
        {
            if (direction == G10_Direction.Left) return -1;
            if (direction == G10_Direction.Right) return 1;
            return 0;
        }

        public static int DeltaY(this G10_Direction direction)
        {
            if (direction == G10_Direction.Up) return -1;
            if (direction == G10_Direction.Down) return 1;
            return 0;
        }

        public static G10_Direction Opposite(this G10_Direction direction)
        {
            switch (direction)
            {
                case G10_Direction.Up: return G10_Direction.Down;
                case G10_Direction.Down: return G10_Direction.Up;
                case G10_Direction.Left: return G10_Direction.Right;
                case G10_Direction.Right: return G10_Direction.Left;
                default: return G10_Direction.None;
            }
        }

        public static bool IsHorizontal(this G10_Direction direction)
        {
            return direction == G10_Direction.Left || direction == G10_Direction.Right;
        }
    }
}
