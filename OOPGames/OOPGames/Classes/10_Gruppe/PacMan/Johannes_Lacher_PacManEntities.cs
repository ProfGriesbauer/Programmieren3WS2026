using System;

namespace OOPGames
{
    // Shared movement of Pac-Man and the ghosts. Positions are the actor's centre in sub-pixels.
    // Actors only change direction at tile centres; Move walks from centre to centre and asks the
    // subclass at every centre where to go next (Template Method).
    public abstract class G10_MovingEntity : IG10_ActorView
    {
        const int Half = G10_PacManConstants.TileSubPixels / 2;

        int _X;
        int _Y;
        int _SpeedRemainder;

        protected G10_MovingEntity(G10_Maze maze)
        {
            if (maze == null) throw new ArgumentNullException(nameof(maze));
            Maze = maze;
            Visible = true;
        }

        protected G10_Maze Maze { get; private set; }
        public int PixelX { get { return FloorDiv(_X, G10_PacManConstants.SubPixels); } }
        public int PixelY { get { return FloorDiv(_Y, G10_PacManConstants.SubPixels); } }
        public G10_Direction Direction { get; protected set; }
        public bool IsMoving { get; private set; }
        public int MoveTicks { get; private set; }
        public bool Visible { get; set; }
        public G10_Tile Tile { get { return G10_Tile.FromPixel(PixelX, PixelY); } }
        // Tile at the start of the current tick; used to detect actors swapping tiles.
        public G10_Tile PreviousTile { get; private set; }

        protected bool IsAtTileCenter
        {
            get { return Mod(_X - Half, G10_PacManConstants.TileSubPixels) == 0 && Mod(_Y - Half, G10_PacManConstants.TileSubPixels) == 0; }
        }

        public void PlaceAt(G10_PixelPoint position, G10_Direction direction)
        {
            _X = position.X * G10_PacManConstants.SubPixels;
            _Y = position.Y * G10_PacManConstants.SubPixels;
            _SpeedRemainder = 0;
            Direction = direction;
            IsMoving = false;
            MoveTicks = 0;
            Visible = true;
            PreviousTile = Tile;
        }

        public void BeginTick()
        {
            PreviousTile = Tile;
        }

        // Moves along the maze for one tick. Never skips a tile centre, even at high speed.
        public void Move(int speedPercent)
        {
            int distance = DistanceThisTick(speedPercent);
            bool moved = false;
            while (distance > 0)
            {
                if (IsAtTileCenter)
                {
                    G10_Direction next = ChooseDirectionAtCenter();
                    if (next == G10_Direction.None || !CanEnter(Tile.Neighbour(next))) break;
                    Direction = next;
                }
                if (Direction == G10_Direction.None) break;

                int step = Math.Min(distance, DistanceToNextCenter());
                _X = Maze.WrapX(_X + Direction.DeltaX() * step);
                _Y += Direction.DeltaY() * step;
                distance -= step;
                moved = true;
            }
            FinishTick(moved);
        }

        // Moves straight to a pixel point, ignoring walls: first horizontally, then vertically.
        // Used inside the ghost house. Returns true when the point is reached.
        protected bool MoveTowards(int pixelX, int pixelY, int speedPercent)
        {
            int distance = DistanceThisTick(speedPercent);
            int targetX = pixelX * G10_PacManConstants.SubPixels;
            int targetY = pixelY * G10_PacManConstants.SubPixels;
            bool moved = false;
            if (_X != targetX)
            {
                int step = Math.Min(distance, Math.Abs(targetX - _X));
                Direction = targetX > _X ? G10_Direction.Right : G10_Direction.Left;
                _X += Math.Sign(targetX - _X) * step;
                distance -= step;
                moved = step > 0;
            }
            if (_X == targetX && _Y != targetY && distance > 0)
            {
                int step = Math.Min(distance, Math.Abs(targetY - _Y));
                Direction = targetY > _Y ? G10_Direction.Down : G10_Direction.Up;
                _Y += Math.Sign(targetY - _Y) * step;
                moved = moved || step > 0;
            }
            FinishTick(moved);
            return _X == targetX && _Y == targetY;
        }

        protected abstract bool CanEnter(G10_Tile tile);

        // Direction to take from the tile centre the actor stands on; None stops it.
        protected abstract G10_Direction ChooseDirectionAtCenter();

        void FinishTick(bool moved)
        {
            IsMoving = moved;
            if (moved) MoveTicks++;
        }

        int DistanceThisTick(int speedPercent)
        {
            int total = speedPercent * G10_PacManConstants.FullSpeedSubPixelsTimes100 + _SpeedRemainder;
            _SpeedRemainder = total % 10000;
            return total / 10000;
        }

        int DistanceToNextCenter()
        {
            int position = Direction.IsHorizontal() ? _X : _Y;
            int offset = Mod(position - Half, G10_PacManConstants.TileSubPixels);
            if (offset == 0) return G10_PacManConstants.TileSubPixels;
            bool forward = Direction == G10_Direction.Right || Direction == G10_Direction.Down;
            return forward ? G10_PacManConstants.TileSubPixels - offset : offset;
        }

        static int Mod(int value, int divisor)
        {
            return ((value % divisor) + divisor) % divisor;
        }

        static int FloorDiv(int value, int divisor)
        {
            return (value - Mod(value, divisor)) / divisor;
        }
    }

    // Pac-Man knows nothing about keys: the session passes the requested direction.
    public sealed class G10_PacMan : G10_MovingEntity
    {
        G10_Direction _Requested;

        public G10_PacMan(G10_Maze maze) : base(maze)
        {
        }

        public void Reset(G10_PixelPoint start)
        {
            _Requested = G10_Direction.None;
            PlaceAt(start, G10_Direction.Left);
        }

        // Stores the direction until the maze allows it. Turning around works at once.
        public void RequestDirection(G10_Direction direction)
        {
            if (direction == G10_Direction.None) return;
            _Requested = direction;
            if (direction == Direction.Opposite()) Direction = direction;
        }

        protected override bool CanEnter(G10_Tile tile)
        {
            return Maze.IsWalkable(tile, false);
        }

        protected override G10_Direction ChooseDirectionAtCenter()
        {
            if (_Requested != G10_Direction.None && CanEnter(Tile.Neighbour(_Requested))) return _Requested;
            if (CanEnter(Tile.Neighbour(Direction))) return Direction;
            return G10_Direction.None;
        }
    }
}
