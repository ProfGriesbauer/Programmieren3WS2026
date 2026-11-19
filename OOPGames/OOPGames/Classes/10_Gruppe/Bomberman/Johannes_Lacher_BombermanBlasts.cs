using System;
using System.Collections.Generic;

namespace OOPGames
{
    public sealed class G10_BomberBomb : IG10_BomberBombView
    {
        public static readonly int FuseTicks = G10_BomberConstants.Seconds(2.5);

        public G10_BomberBomb(G10_BomberCell cell, int range, bool isRemote)
        {
            if (range < 1) throw new ArgumentOutOfRangeException(nameof(range));
            Cell = cell;
            Range = range;
            IsRemote = isRemote;
        }

        public G10_BomberCell Cell { get; private set; }
        public int Range { get; private set; }
        public int AgeTicks { get; private set; }
        // Remote bombs wait for the detonator instead of a fuse.
        public bool IsRemote { get; private set; }
        public bool FuseBurntDown { get { return !IsRemote && AgeTicks >= FuseTicks; } }

        public void Tick()
        {
            AgeTicks++;
        }
    }

    public sealed class G10_BomberFlame : IG10_BomberFlameView
    {
        public static readonly int Duration = G10_BomberConstants.Seconds(0.5);

        public G10_BomberFlame(G10_BomberCell cell, G10_BomberFlameShape shape)
        {
            Cell = cell;
            Shape = shape;
        }

        public G10_BomberCell Cell { get; private set; }
        public G10_BomberFlameShape Shape { get; private set; }
        public int AgeTicks { get; private set; }
        public bool IsOut { get { return AgeTicks >= Duration; } }

        public void Tick()
        {
            AgeTicks++;
        }
    }

    // Owns all bombs and flames. Explosions spread in four directions, destroy the first brick in
    // their way, burn visible power-ups and set off other bombs (chain reaction).
    public sealed class G10_BomberBlastSystem
    {
        readonly G10_BomberArena _Arena;
        readonly List<G10_BomberBomb> _Bombs = new List<G10_BomberBomb>();
        readonly List<G10_BomberFlame> _Flames = new List<G10_BomberFlame>();

        public G10_BomberBlastSystem(G10_BomberArena arena)
        {
            if (arena == null) throw new ArgumentNullException(nameof(arena));
            _Arena = arena;
        }

        public IReadOnlyList<G10_BomberBomb> Bombs { get { return _Bombs; } }
        public IReadOnlyList<G10_BomberFlame> Flames { get { return _Flames; } }
        bool _ExitHit;

        // True once after a flame touched the exit.
        public bool ConsumeExitHit()
        {
            bool hit = _ExitHit;
            _ExitHit = false;
            return hit;
        }

        public bool HasBombAt(G10_BomberCell cell)
        {
            return _Bombs.Exists(b => b.Cell == cell);
        }

        public bool IsBurning(G10_BomberCell cell)
        {
            return _Flames.Exists(f => f.Cell == cell);
        }

        public bool TryPlace(G10_BomberCell cell, G10_BomberPlayerStats stats)
        {
            if (_Bombs.Count >= stats.BombCapacity || HasBombAt(cell)) return false;
            if (_Arena.KindAt(cell) != G10_BomberCellKind.Floor) return false;
            _Bombs.Add(new G10_BomberBomb(cell, stats.FireRange, stats.HasRemote));
            return true;
        }

        // The detonator sets off the oldest remote bomb.
        public void DetonateOldestRemote()
        {
            G10_BomberBomb bomb = _Bombs.Find(b => b.IsRemote);
            if (bomb != null) Explode(bomb);
        }

        public void Tick()
        {
            _Flames.ForEach(f => f.Tick());
            _Flames.RemoveAll(f => f.IsOut);
            _Bombs.ForEach(b => b.Tick());

            // A bomb also goes off when it lies in a flame that is still burning.
            G10_BomberBomb due;
            while ((due = _Bombs.Find(b => b.FuseBurntDown || IsBurning(b.Cell))) != null)
            {
                Explode(due);
            }
        }

        void Explode(G10_BomberBomb first)
        {
            Queue<G10_BomberBomb> pending = new Queue<G10_BomberBomb>();
            pending.Enqueue(first);
            _Bombs.Remove(first);
            while (pending.Count > 0)
            {
                G10_BomberBomb bomb = pending.Dequeue();
                _Flames.Add(new G10_BomberFlame(bomb.Cell, G10_BomberFlameShape.Center));
                if (_Arena.VisibleItemAt(bomb.Cell) == G10_BomberItemKind.Exit) _ExitHit = true;
                foreach (G10_Direction direction in G10_DirectionExtensions.TieBreakOrder)
                {
                    Spread(bomb, direction, pending);
                }
            }
        }

        void Spread(G10_BomberBomb bomb, G10_Direction direction, Queue<G10_BomberBomb> pending)
        {
            G10_BomberCell cell = bomb.Cell;
            for (int distance = 1; distance <= bomb.Range; distance++)
            {
                cell = cell.Neighbour(direction);
                if (_Arena.KindAt(cell) == G10_BomberCellKind.Pillar) return;
                if (_Arena.DestroyBrick(cell)) return;
                if (_Arena.BurnTicksAt(cell) > 0) return;

                G10_BomberBomb other = _Bombs.Find(b => b.Cell == cell);
                if (other != null)
                {
                    _Bombs.Remove(other);
                    pending.Enqueue(other);
                    return;
                }

                bool end = distance == bomb.Range;
                G10_BomberItemKind item = _Arena.VisibleItemAt(cell);
                if (item == G10_BomberItemKind.Exit) _ExitHit = true;
                else if (item != G10_BomberItemKind.None)
                {
                    // Flames burn power-ups that lie in the open.
                    _Arena.RemoveItem(cell);
                    end = true;
                }
                _Flames.Add(new G10_BomberFlame(cell, end ? EndShape(direction) : MiddleShape(direction)));
                if (end) return;
            }
        }

        static G10_BomberFlameShape MiddleShape(G10_Direction direction)
        {
            return direction.IsHorizontal() ? G10_BomberFlameShape.Horizontal : G10_BomberFlameShape.Vertical;
        }

        static G10_BomberFlameShape EndShape(G10_Direction direction)
        {
            switch (direction)
            {
                case G10_Direction.Left: return G10_BomberFlameShape.EndLeft;
                case G10_Direction.Right: return G10_BomberFlameShape.EndRight;
                case G10_Direction.Up: return G10_BomberFlameShape.EndUp;
                default: return G10_BomberFlameShape.EndDown;
            }
        }
    }
}
