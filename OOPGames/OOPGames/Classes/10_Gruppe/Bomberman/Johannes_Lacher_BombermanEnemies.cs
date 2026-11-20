using System;
using System.Collections.Generic;

namespace OOPGames
{
    // What an enemy brain may look at when it decides.
    public sealed class G10_BomberBrainContext
    {
        public G10_BomberBrainContext(Random random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            Random = random;
        }

        public Random Random { get; private set; }
        public G10_BomberCell PlayerCell { get; set; }
        public bool PlayerAlive { get; set; }
    }

    // Walks straight on and turns now and then at crossings; turns back only at dead ends.
    public sealed class G10_BomberWanderBrain : IG10_BomberEnemyBrain
    {
        readonly double _TurnChance;

        public G10_BomberWanderBrain(double turnChance)
        {
            if (turnChance < 0 || turnChance > 1) throw new ArgumentOutOfRangeException(nameof(turnChance));
            _TurnChance = turnChance;
        }

        public G10_Direction ChooseDirection(G10_BomberEnemy enemy, G10_BomberBrainContext context)
        {
            List<G10_Direction> options = new List<G10_Direction>();
            foreach (G10_Direction direction in G10_DirectionExtensions.TieBreakOrder)
            {
                if (direction != enemy.Direction.Opposite() && enemy.CanWalkInto(enemy.Cell.Neighbour(direction)))
                    options.Add(direction);
            }

            bool canGoOn = options.Contains(enemy.Direction);
            if (canGoOn && (options.Count == 1 || context.Random.NextDouble() >= _TurnChance)) return enemy.Direction;
            if (options.Count > 0) return options[context.Random.Next(options.Count)];

            G10_Direction back = enemy.Direction.Opposite();
            return back != G10_Direction.None && enemy.CanWalkInto(enemy.Cell.Neighbour(back)) ? back : G10_Direction.None;
        }
    }

    // Takes the shortest way to the player when the player is close enough by path length;
    // otherwise it falls back to another brain.
    public sealed class G10_BomberChaseBrain : IG10_BomberEnemyBrain
    {
        readonly int _SenseRange;
        readonly IG10_BomberEnemyBrain _Fallback;

        public G10_BomberChaseBrain(int senseRange, IG10_BomberEnemyBrain fallback)
        {
            if (senseRange < 1) throw new ArgumentOutOfRangeException(nameof(senseRange));
            if (fallback == null) throw new ArgumentNullException(nameof(fallback));
            _SenseRange = senseRange;
            _Fallback = fallback;
        }

        public G10_Direction ChooseDirection(G10_BomberEnemy enemy, G10_BomberBrainContext context)
        {
            if (context.PlayerAlive)
            {
                G10_Direction step = FirstStepTowards(enemy, context.PlayerCell);
                if (step != G10_Direction.None) return step;
            }
            return _Fallback.ChooseDirection(enemy, context);
        }

        // Breadth-first search over the cells the enemy can walk into.
        G10_Direction FirstStepTowards(G10_BomberEnemy enemy, G10_BomberCell goal)
        {
            G10_BomberCell start = enemy.Cell;
            if (start == goal || start.ManhattanTo(goal) > _SenseRange) return G10_Direction.None;

            Dictionary<G10_BomberCell, G10_Direction> firstStep = new Dictionary<G10_BomberCell, G10_Direction>();
            Queue<KeyValuePair<G10_BomberCell, int>> open = new Queue<KeyValuePair<G10_BomberCell, int>>();
            firstStep[start] = G10_Direction.None;
            open.Enqueue(new KeyValuePair<G10_BomberCell, int>(start, 0));
            while (open.Count > 0)
            {
                KeyValuePair<G10_BomberCell, int> current = open.Dequeue();
                if (current.Value >= _SenseRange) continue;
                foreach (G10_Direction direction in G10_DirectionExtensions.TieBreakOrder)
                {
                    G10_BomberCell next = current.Key.Neighbour(direction);
                    if (firstStep.ContainsKey(next) || !enemy.CanWalkInto(next)) continue;
                    G10_Direction first = current.Key == start ? direction : firstStep[current.Key];
                    if (next == goal) return first;
                    firstStep[next] = first;
                    open.Enqueue(new KeyValuePair<G10_BomberCell, int>(next, current.Value + 1));
                }
            }
            return G10_Direction.None;
        }
    }

    // Fixed data of an enemy kind.
    public sealed class G10_BomberEnemyType
    {
        public G10_BomberEnemyType(G10_BomberEnemyKind kind, int speed, int points, bool passesBricks, IG10_BomberEnemyBrain brain)
        {
            if (speed <= 0) throw new ArgumentOutOfRangeException(nameof(speed));
            if (brain == null) throw new ArgumentNullException(nameof(brain));
            Kind = kind;
            Speed = speed;
            Points = points;
            PassesBricks = passesBricks;
            Brain = brain;
        }

        public G10_BomberEnemyKind Kind { get; private set; }
        // Sub-pixels per tick; the player starts with 10.
        public int Speed { get; private set; }
        public int Points { get; private set; }
        public bool PassesBricks { get; private set; }
        public IG10_BomberEnemyBrain Brain { get; private set; }
    }

    public static class G10_BomberEnemyTypes
    {
        // Brains keep no state, so all enemies of a kind share one.
        static readonly Dictionary<G10_BomberEnemyKind, G10_BomberEnemyType> _Types = new Dictionary<G10_BomberEnemyKind, G10_BomberEnemyType>
        {
            { G10_BomberEnemyKind.Puff, new G10_BomberEnemyType(G10_BomberEnemyKind.Puff, 6, 100, false, new G10_BomberWanderBrain(0.25)) },
            { G10_BomberEnemyKind.Spark, new G10_BomberEnemyType(G10_BomberEnemyKind.Spark, 8, 200, false, new G10_BomberChaseBrain(4, new G10_BomberWanderBrain(0.4))) },
            { G10_BomberEnemyKind.Drift, new G10_BomberEnemyType(G10_BomberEnemyKind.Drift, 7, 400, true, new G10_BomberWanderBrain(0.5)) },
            { G10_BomberEnemyKind.Hunter, new G10_BomberEnemyType(G10_BomberEnemyKind.Hunter, 11, 800, false, new G10_BomberChaseBrain(10, new G10_BomberWanderBrain(0.5))) },
            { G10_BomberEnemyKind.Phantom, new G10_BomberEnemyType(G10_BomberEnemyKind.Phantom, 13, 1000, true, new G10_BomberChaseBrain(30, new G10_BomberWanderBrain(0.5))) }
        };

        public static G10_BomberEnemyType Get(G10_BomberEnemyKind kind)
        {
            return _Types[kind];
        }
    }

    public sealed class G10_BomberEnemy : G10_BomberActor, IG10_BomberEnemyView
    {
        // A hit enemy stays visible this long before it disappears.
        public static readonly int DyingDuration = G10_BomberConstants.Seconds(0.8);

        readonly G10_BomberEnemyType _Type;

        public G10_BomberEnemy(IG10_BomberObstacles obstacles, G10_BomberEnemyType type, G10_BomberCell cell, G10_Direction direction) : base(obstacles)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            _Type = type;
            PlaceAt(cell, direction);
        }

        public G10_BomberEnemyKind Kind { get { return _Type.Kind; } }
        public int Points { get { return _Type.Points; } }
        public bool IsDying { get; private set; }
        public int DyingTicks { get; private set; }
        public bool IsGone { get { return IsDying && DyingTicks >= DyingDuration; } }

        protected override bool PassesBricks { get { return _Type.PassesBricks; } }

        public void Kill()
        {
            IsDying = true;
            FinishTick(false);
        }

        // Walks from cell centre to cell centre; the brain picks the way at each centre.
        // A bomb dropped in its way makes the enemy turn around.
        public void Update(G10_BomberBrainContext context)
        {
            if (IsDying)
            {
                DyingTicks++;
                return;
            }

            int distance = _Type.Speed;
            bool moved = false;
            while (distance > 0)
            {
                if (IsAtCenter)
                {
                    G10_Direction next = _Type.Brain.ChooseDirection(this, context);
                    if (next == G10_Direction.None || !CanWalkInto(Cell.Neighbour(next))) break;
                    Direction = next;
                }
                else if (IsLeavingCell() && !CanWalkInto(Cell.Neighbour(Direction)))
                {
                    Direction = Direction.Opposite();
                }

                int step = Math.Min(distance, DistanceToNextCenter());
                Shift(Direction.IsHorizontal(), Sign(Direction) * step);
                distance -= step;
                moved = true;
            }
            FinishTick(moved);
        }
    }
}
