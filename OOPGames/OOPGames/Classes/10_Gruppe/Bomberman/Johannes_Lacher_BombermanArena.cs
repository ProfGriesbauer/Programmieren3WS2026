using System;
using System.Collections.Generic;

namespace OOPGames
{
    // The playing field: outer wall, pillar grid, bricks and the items hidden under them.
    // Knows nothing about bombs or actors.
    public sealed class G10_BomberArena : IG10_BomberArenaView
    {
        // A destroyed brick burns this long before it becomes floor and shows its item.
        public static readonly int BurnTicks = G10_BomberConstants.Seconds(0.5);

        static int _NextVersion;

        readonly G10_BomberCellKind[] _Kinds;
        readonly G10_BomberItemKind[] _Items;
        readonly bool[] _Uncovered;
        readonly int[] _Burn;

        public G10_BomberArena(int columns, int rows)
        {
            if (columns < 5 || columns % 2 == 0) throw new ArgumentOutOfRangeException(nameof(columns), "Needs an odd number of at least 5.");
            if (rows < 5 || rows % 2 == 0) throw new ArgumentOutOfRangeException(nameof(rows), "Needs an odd number of at least 5.");
            Columns = columns;
            Rows = rows;
            _Kinds = new G10_BomberCellKind[columns * rows];
            _Items = new G10_BomberItemKind[columns * rows];
            _Uncovered = new bool[columns * rows];
            _Burn = new int[columns * rows];
            Version = ++_NextVersion;
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    bool border = col == 0 || row == 0 || col == columns - 1 || row == rows - 1;
                    bool pillar = col % 2 == 0 && row % 2 == 0;
                    _Kinds[row * columns + col] = border || pillar ? G10_BomberCellKind.Pillar : G10_BomberCellKind.Floor;
                }
            }
        }

        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public int Version { get; private set; }

        public bool IsInside(G10_BomberCell cell)
        {
            return cell.Col >= 0 && cell.Row >= 0 && cell.Col < Columns && cell.Row < Rows;
        }

        public G10_BomberCellKind KindAt(G10_BomberCell cell)
        {
            return IsInside(cell) ? _Kinds[Index(cell)] : G10_BomberCellKind.Pillar;
        }

        public G10_BomberItemKind ItemAt(G10_BomberCell cell)
        {
            return IsInside(cell) ? _Items[Index(cell)] : G10_BomberItemKind.None;
        }

        public G10_BomberItemKind VisibleItemAt(G10_BomberCell cell)
        {
            return IsInside(cell) && _Uncovered[Index(cell)] ? _Items[Index(cell)] : G10_BomberItemKind.None;
        }

        public int BurnTicksAt(G10_BomberCell cell)
        {
            return IsInside(cell) ? _Burn[Index(cell)] : 0;
        }

        // Pillars always block; bricks (also burning ones) block unless the walker passes through walls.
        public bool BlocksWalking(G10_BomberCell cell, bool passBricks)
        {
            G10_BomberCellKind kind = KindAt(cell);
            if (kind == G10_BomberCellKind.Pillar) return true;
            if (passBricks) return false;
            return kind == G10_BomberCellKind.Brick || BurnTicksAt(cell) > 0;
        }

        public IEnumerable<G10_BomberCell> Cells()
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    yield return new G10_BomberCell(col, row);
                }
            }
        }

        public void PlaceBrick(G10_BomberCell cell)
        {
            if (KindAt(cell) != G10_BomberCellKind.Floor) throw new InvalidOperationException("Bricks only go on floor: " + cell);
            _Kinds[Index(cell)] = G10_BomberCellKind.Brick;
        }

        public void HideItem(G10_BomberCell cell, G10_BomberItemKind item)
        {
            if (KindAt(cell) != G10_BomberCellKind.Brick) throw new InvalidOperationException("Items are hidden under bricks: " + cell);
            _Items[Index(cell)] = item;
        }

        // Returns false if there was no brick.
        public bool DestroyBrick(G10_BomberCell cell)
        {
            if (KindAt(cell) != G10_BomberCellKind.Brick) return false;
            int index = Index(cell);
            _Kinds[index] = G10_BomberCellKind.Floor;
            _Burn[index] = BurnTicks;
            return true;
        }

        public void RemoveItem(G10_BomberCell cell)
        {
            if (!IsInside(cell)) return;
            _Items[Index(cell)] = G10_BomberItemKind.None;
        }

        // Lets burning bricks burn down; an item under one becomes visible afterwards.
        public void Tick()
        {
            for (int i = 0; i < _Burn.Length; i++)
            {
                if (_Burn[i] == 0) continue;
                _Burn[i]--;
                if (_Burn[i] == 0) _Uncovered[i] = true;
            }
        }

        int Index(G10_BomberCell cell)
        {
            return cell.Row * Columns + cell.Col;
        }
    }

    // Builds the arena of a stage: bricks at random, then the exit and the power-up under two of them.
    public static class G10_BomberArenaGenerator
    {
        public static readonly G10_BomberCell PlayerStart = new G10_BomberCell(1, 1);

        public static G10_BomberArena Build(G10_BomberStage stage, Random random)
        {
            G10_BomberArena arena = new G10_BomberArena(G10_BomberConstants.Columns, G10_BomberConstants.Rows);
            List<G10_BomberCell> bricks = new List<G10_BomberCell>();
            List<G10_BomberCell> spare = new List<G10_BomberCell>();
            foreach (G10_BomberCell cell in arena.Cells())
            {
                if (arena.KindAt(cell) != G10_BomberCellKind.Floor || IsStartZone(cell)) continue;
                if (random.NextDouble() < stage.BrickDensity) bricks.Add(cell);
                else spare.Add(cell);
            }

            // Two items need two bricks; low densities could leave too few.
            while (bricks.Count < 2)
            {
                int pick = random.Next(spare.Count);
                bricks.Add(spare[pick]);
                spare.RemoveAt(pick);
            }

            foreach (G10_BomberCell cell in bricks) arena.PlaceBrick(cell);
            int exit = random.Next(bricks.Count);
            arena.HideItem(bricks[exit], G10_BomberItemKind.Exit);
            bricks.RemoveAt(exit);
            arena.HideItem(bricks[random.Next(bricks.Count)], stage.PowerUp);
            return arena;
        }

        // The player needs free cells around the start to get away from the first bomb.
        public static bool IsStartZone(G10_BomberCell cell)
        {
            return cell.ManhattanTo(PlayerStart) <= 1;
        }
    }
}
