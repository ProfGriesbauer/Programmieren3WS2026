namespace OOPGames
{
    public enum G10_CollisionOutcome
    {
        None,
        PacManDies,
        GhostEaten
    }

    // Behaviour of a ghost in one mode (State pattern). The ghost delegates to its mode and never
    // has to ask which mode it is in. By default a mode moves the ghost through the maze towards
    // its target tile.
    public abstract class G10_GhostMode
    {
        public abstract G10_GhostModeKind Kind { get; }
        public virtual bool CanUseDoor { get { return false; } }
        // Scatter and chase follow the global timetable and can be frightened.
        public virtual bool FollowsSchedule { get { return false; } }

        public virtual void Update(G10_Ghost ghost, G10_GhostContext context)
        {
            ghost.MoveInMaze(SpeedPercent(ghost, context));
        }

        public virtual G10_Direction ChooseDirection(G10_Ghost ghost, G10_GhostContext context)
        {
            return ghost.DirectionTowards(TargetTile(ghost, context));
        }

        public abstract G10_CollisionOutcome OnCollision(G10_Ghost ghost, G10_PacMan pacMan);

        protected virtual G10_Tile TargetTile(G10_Ghost ghost, G10_GhostContext context)
        {
            return ghost.Tile;
        }

        protected virtual int SpeedPercent(G10_Ghost ghost, G10_GhostContext context)
        {
            return context.Maze.IsTunnel(ghost.Tile) ? context.Level.GhostTunnelSpeed : context.Level.GhostSpeed;
        }
    }

    // Waits in the house and bobs up and down until the ghost house releases it.
    public sealed class G10_InHouseMode : G10_GhostMode
    {
        const int BobRange = 4;
        const int Speed = 40;

        public override G10_GhostModeKind Kind { get { return G10_GhostModeKind.InHouse; } }

        public override void Update(G10_Ghost ghost, G10_GhostContext context)
        {
            int centerY = context.Maze.Layout.HouseCenterY;
            bool goingUp = ghost.Direction != G10_Direction.Down;
            int targetY = goingUp ? centerY - BobRange : centerY + BobRange;
            if (ghost.MoveStraightTo(ghost.PixelX, targetY, Speed)) ghost.ReverseDirection();
        }

        public override G10_CollisionOutcome OnCollision(G10_Ghost ghost, G10_PacMan pacMan)
        {
            return G10_CollisionOutcome.None;
        }
    }

    // Moves to the middle of the house, up through the door, then joins the active mode.
    public sealed class G10_LeavingHouseMode : G10_GhostMode
    {
        const int Speed = 50;

        public override G10_GhostModeKind Kind { get { return G10_GhostModeKind.LeavingHouse; } }
        public override bool CanUseDoor { get { return true; } }

        public override void Update(G10_Ghost ghost, G10_GhostContext context)
        {
            G10_PixelPoint exit = context.Maze.Layout.HouseExit;
            bool atDoorColumn = ghost.PixelX == exit.X;
            int targetY = atDoorColumn ? exit.Y : ghost.PixelY;
            if (ghost.MoveStraightTo(exit.X, targetY, Speed) && atDoorColumn)
            {
                ghost.ReturnToActiveMode(context);
                ghost.LeaveHouseTo(G10_Direction.Left);
            }
        }

        public override G10_CollisionOutcome OnCollision(G10_Ghost ghost, G10_PacMan pacMan)
        {
            return G10_CollisionOutcome.None;
        }
    }

    public sealed class G10_ScatterMode : G10_GhostMode
    {
        public override G10_GhostModeKind Kind { get { return G10_GhostModeKind.Scatter; } }
        public override bool FollowsSchedule { get { return true; } }

        public override G10_CollisionOutcome OnCollision(G10_Ghost ghost, G10_PacMan pacMan)
        {
            return G10_CollisionOutcome.PacManDies;
        }

        protected override G10_Tile TargetTile(G10_Ghost ghost, G10_GhostContext context)
        {
            return ghost.ScatterCorner;
        }
    }

    public sealed class G10_ChaseMode : G10_GhostMode
    {
        public override G10_GhostModeKind Kind { get { return G10_GhostModeKind.Chase; } }
        public override bool FollowsSchedule { get { return true; } }

        public override G10_CollisionOutcome OnCollision(G10_Ghost ghost, G10_PacMan pacMan)
        {
            return G10_CollisionOutcome.PacManDies;
        }

        protected override G10_Tile TargetTile(G10_Ghost ghost, G10_GhostContext context)
        {
            return ghost.TargetStrategy.TargetTile(ghost, context);
        }
    }

    // Slow, random turns, and can be eaten.
    public sealed class G10_FrightenedMode : G10_GhostMode
    {
        public override G10_GhostModeKind Kind { get { return G10_GhostModeKind.Frightened; } }

        public override G10_Direction ChooseDirection(G10_Ghost ghost, G10_GhostContext context)
        {
            return ghost.RandomDirection(context.Random);
        }

        public override G10_CollisionOutcome OnCollision(G10_Ghost ghost, G10_PacMan pacMan)
        {
            ghost.SetMode(new G10_EatenMode());
            return G10_CollisionOutcome.GhostEaten;
        }

        protected override int SpeedPercent(G10_Ghost ghost, G10_GhostContext context)
        {
            return context.Maze.IsTunnel(ghost.Tile) ? context.Level.GhostTunnelSpeed : context.Level.GhostFrightenedSpeed;
        }
    }

    // Only the eyes are left: they hurry back to the door, drop into the house and leave again.
    public sealed class G10_EatenMode : G10_GhostMode
    {
        const int Speed = 200;
        // Half a tile: the eyes pass the door column within this distance at full speed.
        const int DoorTolerance = 4;

        bool _Entering;

        public override G10_GhostModeKind Kind { get { return G10_GhostModeKind.Eaten; } }
        public override bool CanUseDoor { get { return _Entering; } }

        public override void Update(G10_Ghost ghost, G10_GhostContext context)
        {
            G10_MazeLayout layout = context.Maze.Layout;
            if (!_Entering)
            {
                ghost.MoveInMaze(Speed);
                _Entering = ghost.PixelY == layout.HouseExit.Y && System.Math.Abs(ghost.PixelX - layout.HouseExit.X) <= DoorTolerance;
                return;
            }
            if (ghost.MoveStraightTo(layout.HouseExit.X, layout.HouseCenterY, Speed)) ghost.SetMode(new G10_LeavingHouseMode());
        }

        public override G10_CollisionOutcome OnCollision(G10_Ghost ghost, G10_PacMan pacMan)
        {
            return G10_CollisionOutcome.None;
        }

        protected override G10_Tile TargetTile(G10_Ghost ghost, G10_GhostContext context)
        {
            G10_PixelPoint exit = context.Maze.Layout.HouseExit;
            return G10_Tile.FromPixel(exit.X, exit.Y);
        }
    }
}
