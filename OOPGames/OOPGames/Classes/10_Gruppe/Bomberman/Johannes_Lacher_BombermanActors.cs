using System;

namespace OOPGames
{
    // Shared position and grid movement of the player and the enemies. Positions are the
    // actor's centre in sub-pixels. Actors always walk in lanes through the cell centres.
    public abstract class G10_BomberActor : IG10_BomberActorView
    {
        const int Tile = G10_BomberConstants.TileSubPixels;
        const int Half = Tile / 2;

        int _X;
        int _Y;

        protected G10_BomberActor(IG10_BomberObstacles obstacles)
        {
            if (obstacles == null) throw new ArgumentNullException(nameof(obstacles));
            Obstacles = obstacles;
        }

        protected IG10_BomberObstacles Obstacles { get; private set; }
        public int PixelX { get { return _X / G10_BomberConstants.SubPixels; } }
        public int PixelY { get { return _Y / G10_BomberConstants.SubPixels; } }
        public G10_Direction Direction { get; protected set; }
        public bool IsMoving { get; private set; }
        public int MoveTicks { get; private set; }
        // The cell that holds the actor's centre; flames and items act on this cell.
        public G10_BomberCell Cell { get { return new G10_BomberCell(_X / Tile, _Y / Tile); } }

        protected abstract bool PassesBricks { get; }

        protected bool IsAtCenter { get { return AlongOffset(true) == 0 && AlongOffset(false) == 0; } }

        public bool CanWalkInto(G10_BomberCell cell)
        {
            return !Obstacles.IsBlocked(cell, PassesBricks);
        }

        // True if both centres are closer than the distance on both axes.
        public bool Touches(IG10_BomberActorView other, int pixels)
        {
            return Math.Abs(PixelX - other.PixelX) < pixels && Math.Abs(PixelY - other.PixelY) < pixels;
        }

        public void PlaceAt(G10_BomberCell cell, G10_Direction direction)
        {
            _X = cell.Col * Tile + Half;
            _Y = cell.Row * Tile + Half;
            Direction = direction;
            IsMoving = false;
            MoveTicks = 0;
        }

        // Offset of the centre from the current cell centre on one axis.
        protected int AlongOffset(bool horizontal)
        {
            return horizontal ? _X - Cell.Col * Tile - Half : _Y - Cell.Row * Tile - Half;
        }

        protected void Shift(bool horizontal, int amount)
        {
            if (horizontal) _X += amount;
            else _Y += amount;
        }

        // Distance until the next cell centre in the current direction.
        protected int DistanceToNextCenter()
        {
            int ahead = AlongOffset(Direction.IsHorizontal()) * Sign(Direction);
            return ahead < 0 ? -ahead : Tile - ahead;
        }

        // True while the actor has passed its cell centre and walks into the neighbour cell.
        protected bool IsLeavingCell()
        {
            return AlongOffset(Direction.IsHorizontal()) * Sign(Direction) > 0;
        }

        protected void FinishTick(bool moved)
        {
            IsMoving = moved;
            if (moved) MoveTicks++;
        }

        protected static int Sign(G10_Direction direction)
        {
            return direction == G10_Direction.Right || direction == G10_Direction.Down ? 1 : -1;
        }
    }

    // Upgrades the player has collected.
    public sealed class G10_BomberPlayerStats
    {
        public const int MaxBombs = 8;
        public const int MaxFire = 8;
        public const int MaxSpeedLevel = 3;
        const int BaseSpeed = 10;
        const int SpeedPerLevel = 2;

        public G10_BomberPlayerStats()
        {
            BombCapacity = 1;
            FireRange = 1;
        }

        public int BombCapacity { get; private set; }
        public int FireRange { get; private set; }
        public int SpeedLevel { get; private set; }
        public bool HasRemote { get; private set; }
        public int SpeedSubPixels { get { return BaseSpeed + SpeedLevel * SpeedPerLevel; } }

        public void Apply(G10_BomberItemKind item)
        {
            switch (item)
            {
                case G10_BomberItemKind.BombUp: BombCapacity = Math.Min(MaxBombs, BombCapacity + 1); break;
                case G10_BomberItemKind.FireUp: FireRange = Math.Min(MaxFire, FireRange + 1); break;
                case G10_BomberItemKind.SpeedUp: SpeedLevel = Math.Min(MaxSpeedLevel, SpeedLevel + 1); break;
                case G10_BomberItemKind.Remote: HasRemote = true; break;
            }
        }

        // Losing a life costs the detonator; bombs, fire and speed stay.
        public void LoseLife()
        {
            HasRemote = false;
        }
    }

    // The player knows nothing about keys: the round passes the held direction.
    public sealed class G10_BomberPlayer : G10_BomberActor, IG10_BomberPlayerView
    {
        readonly G10_BomberPlayerStats _Stats;

        public G10_BomberPlayer(IG10_BomberObstacles obstacles, G10_BomberPlayerStats stats) : base(obstacles)
        {
            if (stats == null) throw new ArgumentNullException(nameof(stats));
            _Stats = stats;
            PlaceAt(G10_BomberArenaGenerator.PlayerStart, G10_Direction.Down);
        }

        public G10_BomberPlayerStats Stats { get { return _Stats; } }
        public int BombCapacity { get { return _Stats.BombCapacity; } }
        public int FireRange { get { return _Stats.FireRange; } }
        public int SpeedLevel { get { return _Stats.SpeedLevel; } }
        public bool HasRemote { get { return _Stats.HasRemote; } }
        public bool IsDead { get; private set; }

        protected override bool PassesBricks { get { return false; } }

        public void Kill()
        {
            IsDead = true;
            FinishTick(false);
        }

        public void Update(G10_Direction requested)
        {
            if (IsDead || requested == G10_Direction.None)
            {
                FinishTick(false);
                return;
            }

            Direction = requested;
            int distance = _Stats.SpeedSubPixels;
            bool moved = false;
            while (distance > 0)
            {
                int step = Step(requested, distance);
                if (step == 0) break;
                distance -= step;
                moved = true;
            }
            FinishTick(moved);
        }

        // One straight piece of movement; returns how far it went.
        // Off the lane, the player first slides into a lane that leads on in the wanted direction
        // (corner assist), so the player does not have to hit gaps exactly.
        int Step(G10_Direction direction, int distance)
        {
            bool horizontal = direction.IsHorizontal();
            G10_BomberCell cell = Cell;
            int side = AlongOffset(!horizontal);
            if (side != 0)
            {
                int target;
                if (CanWalkInto(cell.Neighbour(direction))) target = 0;
                else
                {
                    G10_Direction sideDirection = horizontal
                        ? (side > 0 ? G10_Direction.Down : G10_Direction.Up)
                        : (side > 0 ? G10_Direction.Right : G10_Direction.Left);
                    G10_BomberCell sideCell = cell.Neighbour(sideDirection);
                    if (!CanWalkInto(sideCell) || !CanWalkInto(sideCell.Neighbour(direction))) return 0;
                    target = Math.Sign(side) * G10_BomberConstants.TileSubPixels;
                }
                int sideStep = Math.Min(distance, Math.Abs(target - side));
                Shift(!horizontal, Math.Sign(target - side) * sideStep);
                return sideStep;
            }

            int limit;
            if (!IsLeavingCell() && AlongOffset(horizontal) != 0) limit = DistanceToNextCenter();
            else limit = CanWalkInto(cell.Neighbour(direction)) ? DistanceToNextCenter() : 0;
            int step = Math.Min(distance, limit);
            Shift(horizontal, Sign(direction) * step);
            return step;
        }
    }
}
