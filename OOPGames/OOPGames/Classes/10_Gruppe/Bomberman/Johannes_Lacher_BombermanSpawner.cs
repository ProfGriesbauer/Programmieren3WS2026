using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Decides where and when enemies enter a round: at the start, from the exit after a flame hit it,
    // and as phantoms when the time is up.
    public sealed class G10_BomberSpawner
    {
        const int MaxEnemies = 12;
        const int DoorGuardCount = 3;
        const int PhantomCount = 4;
        // Enemies start at least this far (in cells) from the player.
        const int SafeDistance = 6;
        // Door guards wait until the player is at least this far (in cells) from the exit,
        // so they never appear right on top of the player.
        const int DoorGuardDistance = 2;
        // Door guards come out when the flame that hit the exit has died down.
        static readonly int DoorGuardDelay = G10_BomberFlame.Duration + 1;

        readonly G10_BomberArena _Arena;
        readonly IG10_BomberObstacles _Obstacles;
        readonly IList<G10_BomberEnemy> _Enemies;
        readonly Random _Random;
        int _DoorGuardTicks;

        // enemies is the round's enemy list; the spawner adds new enemies to it.
        public G10_BomberSpawner(G10_BomberArena arena, IG10_BomberObstacles obstacles, IList<G10_BomberEnemy> enemies, Random random)
        {
            if (arena == null) throw new ArgumentNullException(nameof(arena));
            if (obstacles == null) throw new ArgumentNullException(nameof(obstacles));
            if (enemies == null) throw new ArgumentNullException(nameof(enemies));
            if (random == null) throw new ArgumentNullException(nameof(random));
            _Arena = arena;
            _Obstacles = obstacles;
            _Enemies = enemies;
            _Random = random;
            foreach (G10_BomberCell cell in arena.Cells())
            {
                if (arena.ItemAt(cell) == G10_BomberItemKind.Exit) ExitCell = cell;
            }
        }

        public G10_BomberCell ExitCell { get; private set; }
        public bool DoorGuardsPending { get; private set; }

        public void SpawnAwayFrom(G10_BomberCell playerCell, IEnumerable<G10_BomberEnemyKind> kinds)
        {
            foreach (G10_BomberEnemyKind kind in kinds)
            {
                Spawn(kind, FreeCellAwayFrom(playerCell));
            }
        }

        public void SpawnPhantoms(G10_BomberCell playerCell)
        {
            for (int i = 0; i < PhantomCount; i++)
            {
                Spawn(G10_BomberEnemyKind.Phantom, FreeCellAwayFrom(playerCell));
            }
        }

        // A second hit while guards are already waiting changes nothing.
        public void ExitHit()
        {
            if (DoorGuardsPending) return;
            DoorGuardsPending = true;
            _DoorGuardTicks = DoorGuardDelay;
        }

        // Returns true when the door guards came out in this tick.
        public bool Tick(G10_BomberEnemyKind doorGuard, G10_BomberCell playerCell)
        {
            if (!DoorGuardsPending) return false;
            if (_DoorGuardTicks > 0)
            {
                _DoorGuardTicks--;
                return false;
            }
            if (playerCell.ManhattanTo(ExitCell) < DoorGuardDistance) return false;
            DoorGuardsPending = false;
            for (int i = 0; i < DoorGuardCount; i++)
            {
                Spawn(doorGuard, ExitCell);
            }
            return true;
        }

        void Spawn(G10_BomberEnemyKind kind, G10_BomberCell cell)
        {
            if (_Enemies.Count >= MaxEnemies) return;
            G10_Direction direction = G10_DirectionExtensions.TieBreakOrder[_Random.Next(4)];
            _Enemies.Add(new G10_BomberEnemy(_Obstacles, G10_BomberEnemyTypes.Get(kind), cell, direction));
        }

        // Prefers open floor far from the player that no other enemy uses yet.
        G10_BomberCell FreeCellAwayFrom(G10_BomberCell playerCell)
        {
            List<G10_BomberCell> far = new List<G10_BomberCell>();
            List<G10_BomberCell> any = new List<G10_BomberCell>();
            foreach (G10_BomberCell cell in _Arena.Cells())
            {
                if (_Obstacles.IsBlocked(cell, false) || G10_BomberArenaGenerator.IsStartZone(cell) || cell == playerCell) continue;
                if (IsTaken(cell)) continue;
                any.Add(cell);
                if (cell.ManhattanTo(playerCell) >= SafeDistance) far.Add(cell);
            }
            List<G10_BomberCell> pool = far.Count > 0 ? far : any;
            if (pool.Count == 0) return ExitCell;
            return pool[_Random.Next(pool.Count)];
        }

        bool IsTaken(G10_BomberCell cell)
        {
            foreach (G10_BomberEnemy enemy in _Enemies)
            {
                if (enemy.Cell == cell) return true;
            }
            return false;
        }
    }
}
