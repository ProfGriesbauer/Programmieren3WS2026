using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Everything a ghost needs for one tick. One instance per game; the session keeps it up to date.
    public sealed class G10_GhostContext
    {
        public G10_GhostContext(G10_Maze maze, G10_PacMan pacMan, Random random)
        {
            Maze = maze;
            PacMan = pacMan;
            Random = random;
            ActiveMode = G10_GhostModeKind.Scatter;
        }

        public G10_Maze Maze { get; private set; }
        public G10_PacMan PacMan { get; private set; }
        // Flank (Inky) aims relative to Blinky.
        public G10_Ghost Blinky { get; internal set; }
        public Random Random { get; private set; }
        public G10_LevelSettings Level { get; internal set; }
        // Scatter or chase, as set by the mode scheduler for all ghosts.
        public G10_GhostModeKind ActiveMode { get; internal set; }
    }

    public sealed class G10_Ghost : G10_MovingEntity, IG10_GhostView
    {
        G10_GhostContext _Context;

        public G10_Ghost(G10_GhostKind kind, G10_Maze maze, IG10_TargetStrategy targetStrategy) : base(maze)
        {
            if (targetStrategy == null) throw new ArgumentNullException(nameof(targetStrategy));
            Kind = kind;
            TargetStrategy = targetStrategy;
            ScatterCorner = maze.Layout.ScatterCorner(kind);
            Mode = new G10_InHouseMode();
        }

        public G10_GhostKind Kind { get; private set; }
        public IG10_TargetStrategy TargetStrategy { get; private set; }
        public G10_Tile ScatterCorner { get; private set; }
        public G10_GhostMode Mode { get; private set; }
        G10_GhostModeKind IG10_GhostView.Mode { get { return Mode.Kind; } }

        public void SetMode(G10_GhostMode mode)
        {
            if (mode == null) throw new ArgumentNullException(nameof(mode));
            Mode = mode;
        }

        // Back to the mode that is active for all ghosts right now (scatter or chase).
        public void ReturnToActiveMode(G10_GhostContext context)
        {
            SetMode(context.ActiveMode == G10_GhostModeKind.Chase ? (G10_GhostMode)new G10_ChaseMode() : new G10_ScatterMode());
        }

        public void ReverseDirection()
        {
            Direction = Direction.Opposite();
        }

        // Direction a ghost takes when it steps out of the house door.
        public void LeaveHouseTo(G10_Direction direction)
        {
            Direction = direction;
        }

        public void Update(G10_GhostContext context)
        {
            _Context = context;
            Mode.Update(this, context);
        }

        public void MoveInMaze(int speedPercent)
        {
            Move(speedPercent);
        }

        public bool MoveStraightTo(int pixelX, int pixelY, int speedPercent)
        {
            return MoveTowards(pixelX, pixelY, speedPercent);
        }

        // Standard ghost rule: from the possible directions (no turning back, no walls) take the one
        // whose next tile is closest to the target; ties in the order up, left, down, right.
        public G10_Direction DirectionTowards(G10_Tile target)
        {
            G10_Direction best = G10_Direction.None;
            int bestDistance = int.MaxValue;
            foreach (G10_Direction direction in G10_DirectionExtensions.TieBreakOrder)
            {
                if (!IsOption(direction)) continue;
                int distance = Tile.Neighbour(direction).DistanceSquaredTo(target);
                if (distance < bestDistance)
                {
                    best = direction;
                    bestDistance = distance;
                }
            }
            return best == G10_Direction.None ? Direction.Opposite() : best;
        }

        public G10_Direction RandomDirection(Random random)
        {
            int options = 0;
            foreach (G10_Direction direction in G10_DirectionExtensions.TieBreakOrder)
            {
                if (IsOption(direction)) options++;
            }
            if (options == 0) return Direction.Opposite();
            int pick = random.Next(options);
            foreach (G10_Direction direction in G10_DirectionExtensions.TieBreakOrder)
            {
                if (!IsOption(direction)) continue;
                if (pick == 0) return direction;
                pick--;
            }
            return Direction.Opposite();
        }

        protected override bool CanEnter(G10_Tile tile)
        {
            return Maze.IsWalkable(tile, Mode.CanUseDoor);
        }

        protected override G10_Direction ChooseDirectionAtCenter()
        {
            return Mode.ChooseDirection(this, _Context);
        }

        bool IsOption(G10_Direction direction)
        {
            return direction != Direction.Opposite() && CanEnter(Tile.Neighbour(direction));
        }
    }

    // Decides when the ghosts waiting in the house may leave: by eaten dots, or when Pac-Man
    // has not eaten anything for a while.
    public sealed class G10_GhostHouse
    {
        // Release order of the ghosts that start inside the house.
        static readonly G10_GhostKind[] ReleaseOrder = { G10_GhostKind.Pinky, G10_GhostKind.Inky, G10_GhostKind.Clyde };
        // After a lost life one shared counter is used instead of the personal ones.
        static readonly Dictionary<G10_GhostKind, int> GlobalLimits = new Dictionary<G10_GhostKind, int>
        {
            { G10_GhostKind.Pinky, 7 },
            { G10_GhostKind.Inky, 17 },
            { G10_GhostKind.Clyde, 32 }
        };

        readonly IReadOnlyList<G10_Ghost> _Ghosts;
        readonly Dictionary<G10_GhostKind, int> _DotCounters = new Dictionary<G10_GhostKind, int>();
        G10_LevelSettings _Level;
        bool _UseGlobalCounter;
        int _GlobalCounter;
        int _IdleTicks;

        public G10_GhostHouse(IReadOnlyList<G10_Ghost> ghosts)
        {
            _Ghosts = ghosts;
        }

        public void StartLevel(G10_LevelSettings level)
        {
            _Level = level;
            foreach (G10_GhostKind kind in ReleaseOrder) _DotCounters[kind] = 0;
            _UseGlobalCounter = false;
            _IdleTicks = 0;
        }

        public void StartAfterLostLife()
        {
            _UseGlobalCounter = true;
            _GlobalCounter = 0;
            _IdleTicks = 0;
        }

        public void OnDotEaten()
        {
            _IdleTicks = 0;
            if (_UseGlobalCounter)
            {
                _GlobalCounter++;
                return;
            }
            G10_Ghost waiting = NextWaitingGhost();
            if (waiting != null) _DotCounters[waiting.Kind]++;
        }

        public void Tick()
        {
            G10_Ghost waiting = NextWaitingGhost();
            if (waiting == null) return;

            _IdleTicks++;
            bool release = _UseGlobalCounter
                ? _GlobalCounter >= GlobalLimits[waiting.Kind]
                : _DotCounters[waiting.Kind] >= _Level.DotLimit(waiting.Kind);
            if (_IdleTicks >= _Level.IdleReleaseTicks)
            {
                release = true;
                _IdleTicks = 0;
            }
            if (release) waiting.SetMode(new G10_LeavingHouseMode());
        }

        G10_Ghost NextWaitingGhost()
        {
            foreach (G10_GhostKind kind in ReleaseOrder)
            {
                foreach (G10_Ghost ghost in _Ghosts)
                {
                    if (ghost.Kind == kind && ghost.Mode.Kind == G10_GhostModeKind.InHouse) return ghost;
                }
            }
            return null;
        }
    }
}
