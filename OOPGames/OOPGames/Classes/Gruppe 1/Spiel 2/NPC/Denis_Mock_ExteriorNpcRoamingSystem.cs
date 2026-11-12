using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Entwickler: Denis Mock
    // Bewegt Stadtbewohner über den vorhandenen 40-ms-Framework-Tick innerhalb ihrer Bezirke.
    public sealed class G1C_ExteriorNpcRoamingSystem
    {
        private const int StepDurationTicks = 7;

        private readonly G1C_ExteriorWorld _world;
        private readonly G1C_Player _player;
        private readonly Random _random;
        private readonly List<Patrol> _patrols = new List<Patrol>();

        // Entwickler: Denis Mock
        public G1C_ExteriorNpcRoamingSystem(G1C_ExteriorWorld world, G1C_Player player, Random random)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _random = random ?? throw new ArgumentNullException(nameof(random));

            AddPatrol(world.TownNpcs[0], 28, 32, 38, 39, 8, 18);
            AddPatrol(world.TownNpcs[1], 139, 32, 158, 39, 10, 22);
            AddPatrol(world.TownNpcs[2], 28, 151, 38, 158, 7, 17);
            AddPatrol(world.TownNpcs[3], 139, 151, 158, 158, 12, 24);
        }

        // Entwickler: Denis Mock
        public void Update(int tick)
        {
            foreach (Patrol patrol in _patrols)
            {
                patrol.Npc.AdvanceMovement();
                if (patrol.Npc.IsMoving || tick < patrol.NextMoveTick) continue;

                List<G1C_Direction> steps = GetAvailableSteps(patrol);
                if (steps.Count == 0)
                {
                    patrol.NextMoveTick = tick + 4;
                    continue;
                }

                if (steps.Count > 1 && patrol.LastDirection.HasValue)
                    steps.Remove(G1C_DirectionHelper.Opposite(patrol.LastDirection.Value));

                G1C_Direction direction = steps[_random.Next(steps.Count)];
                int targetX = patrol.Npc.X + G1C_DirectionHelper.DeltaX(direction);
                int targetY = patrol.Npc.Y + G1C_DirectionHelper.DeltaY(direction);
                patrol.Npc.BeginMoveTo(targetX, targetY, direction, StepDurationTicks);
                patrol.LastDirection = direction;
                patrol.NextMoveTick = tick + StepDurationTicks + _random.Next(patrol.MinimumPauseTicks,
                    patrol.MaximumPauseTicks + 1);
            }
        }

        // Entwickler: Denis Mock
        private void AddPatrol(G1C_Npc npc, int minX, int minY, int maxX, int maxY,
                               int minimumPauseTicks, int maximumPauseTicks)
        {
            if (npc == null) throw new ArgumentNullException(nameof(npc));
            _patrols.Add(new Patrol(npc, minX, minY, maxX, maxY, minimumPauseTicks, maximumPauseTicks,
                                    _random.Next(1, maximumPauseTicks + 1)));
        }

        // Entwickler: Denis Mock
        private List<G1C_Direction> GetAvailableSteps(Patrol patrol)
        {
            List<G1C_Direction> steps = new List<G1C_Direction>
            {
                G1C_Direction.Up,
                G1C_Direction.Down,
                G1C_Direction.Left,
                G1C_Direction.Right
            };
            steps.RemoveAll(direction => !CanMoveTo(patrol,
                patrol.Npc.X + G1C_DirectionHelper.DeltaX(direction),
                patrol.Npc.Y + G1C_DirectionHelper.DeltaY(direction)));
            return steps;
        }

        // Entwickler: Denis Mock
        private bool CanMoveTo(Patrol patrol, int x, int y)
        {
            if (x < patrol.MinX || x > patrol.MaxX || y < patrol.MinY || y > patrol.MaxY) return false;
            if (!_world.Map.IsWalkable(x, y) || _world.Map.GetTransitionAt(x, y).HasValue) return false;
            if (_world.Map.IsDoorApproach(x, y)) return false;
            if (x == _player.TileX && y == _player.TileY) return false;

            foreach (G1C_WorldObject obj in _world.Objects)
            {
                if (obj != patrol.Npc && obj.Solid && obj.Occupies(x, y)) return false;
            }
            return true;
        }

        // Entwickler: Denis Mock
        private sealed class Patrol
        {
            // Entwickler: Denis Mock
            public Patrol(G1C_Npc npc, int minX, int minY, int maxX, int maxY,
                          int minimumPauseTicks, int maximumPauseTicks, int initialDelayTicks)
            {
                Npc = npc;
                MinX = minX;
                MinY = minY;
                MaxX = maxX;
                MaxY = maxY;
                MinimumPauseTicks = minimumPauseTicks;
                MaximumPauseTicks = maximumPauseTicks;
                NextMoveTick = initialDelayTicks;
            }

            public G1C_Npc Npc { get; }
            public int MinX { get; }
            public int MinY { get; }
            public int MaxX { get; }
            public int MaxY { get; }
            public int MinimumPauseTicks { get; }
            public int MaximumPauseTicks { get; }
            public int NextMoveTick { get; set; }
            public G1C_Direction? LastDirection { get; set; }
        }
    }
}
