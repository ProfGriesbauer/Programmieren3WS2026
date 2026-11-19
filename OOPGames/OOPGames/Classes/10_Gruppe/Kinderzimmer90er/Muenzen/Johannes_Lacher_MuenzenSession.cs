using System;
using System.Collections.Generic;

namespace OOPGames
{
    // One game of "Muenzen sammeln": collect enough coins in each level before the time runs
    // out and keep away from the dust bunnies. Knows nothing about keys, WPF or files.
    public sealed class G10_CoinSession : IG10_CoinView
    {
        readonly Random _Random;
        readonly int _StartHighScore;
        readonly G10_CoinPlayer _Player = new G10_CoinPlayer();
        readonly List<G10_DustBunny> _Bunnies = new List<G10_DustBunny>();
        readonly List<G10_Tile> _BunnyTiles = new List<G10_Tile>();
        readonly List<G10_CoinPickup> _Pickups = new List<G10_CoinPickup>();
        G10_CoinLevel _Level;
        G10_CoinArena _Arena;
        int _DiamondTimer;

        public G10_CoinSession(int highScore, int randomSeed)
        {
            _Random = new Random(randomSeed);
            _StartHighScore = highScore;
            HighScore = highScore;
            Lives = G10_CoinConfig.StartLives;
            StartLevel(1);
        }

        public int Columns { get { return _Arena.Columns; } }
        public int Rows { get { return _Arena.Rows; } }
        public bool IsWall(G10_Tile tile) { return _Arena.IsWall(tile); }
        public G10_CoinPhase Phase { get; private set; }
        public int PhaseTicks { get; private set; }
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public bool IsNewHighScore { get { return Score > _StartHighScore; } }
        public int Level { get { return _Level.Number; } }
        public int Lives { get; private set; }
        public int CoinsCollected { get; private set; }
        public int CoinsNeeded { get { return _Level.CoinsNeeded; } }
        public int TicksLeft { get; private set; }
        public bool TimeRanOut { get; private set; }
        public int TimeBonus { get; private set; }
        public IG10_CoinPlayerView Player { get { return _Player; } }
        public IReadOnlyList<G10_Tile> DustBunnies { get { return _BunnyTiles; } }
        public IReadOnlyList<IG10_PickupView> Pickups { get { return _Pickups; } }

        public void Update(G10_CoinInput input)
        {
            PhaseTicks++;
            switch (Phase)
            {
                case G10_CoinPhase.Ready:
                    if (PhaseTicks >= G10_CoinConfig.ReadyTicks) SetPhase(G10_CoinPhase.Playing);
                    break;
                case G10_CoinPhase.Playing:
                    if (input.Confirm) SetPhase(G10_CoinPhase.Paused);
                    else Play(input);
                    break;
                case G10_CoinPhase.Paused:
                    if (input.Confirm) SetPhase(G10_CoinPhase.Playing);
                    break;
                case G10_CoinPhase.Hit:
                    if (PhaseTicks >= G10_CoinConfig.HitTicks) Recover();
                    break;
                case G10_CoinPhase.LevelCleared:
                    if (PhaseTicks >= G10_CoinConfig.LevelClearedTicks) StartLevel(_Level.Number + 1);
                    break;
            }
        }

        void Play(G10_CoinInput input)
        {
            if (_Player.Update(input, _Arena)) Collect();
            if (Phase != G10_CoinPhase.Playing) return;

            _BunnyTiles.Clear();
            foreach (G10_DustBunny bunny in _Bunnies)
            {
                bunny.Update(_Arena, _Level.BunnyStepTicks);
                _BunnyTiles.Add(bunny.Tile);
            }
            if (!_Player.IsInvulnerable && _BunnyTiles.Contains(_Player.Tile))
            {
                LoseLife(false);
                return;
            }

            UpdateDiamonds();
            if (--TicksLeft <= 0) LoseLife(true);
        }

        void Collect()
        {
            G10_CoinPickup pickup = _Pickups.Find(p => p.Tile == _Player.Tile);
            if (pickup == null) return;
            _Pickups.Remove(pickup);
            AddPoints(pickup.Points);
            if (pickup.Kind != G10_PickupKind.Coin) return;

            CoinsCollected++;
            if (CoinsCollected >= CoinsNeeded) ClearLevel();
            else RefillCoins();
        }

        void UpdateDiamonds()
        {
            foreach (G10_CoinPickup pickup in _Pickups) pickup.Tick();
            _Pickups.RemoveAll(p => p.IsGone);
            if (_Pickups.Exists(p => p.Kind == G10_PickupKind.Diamond)) return;
            if (++_DiamondTimer < G10_CoinConfig.DiamondInterval) return;
            _DiamondTimer = 0;
            _Pickups.Add(new G10_CoinPickup(FreeTile(), G10_PickupKind.Diamond));
        }

        // Keeps a few coins on the field, but never more than are still needed.
        void RefillCoins()
        {
            int wanted = Math.Min(G10_CoinConfig.CoinsOnField, CoinsNeeded - CoinsCollected);
            int onField = _Pickups.FindAll(p => p.Kind == G10_PickupKind.Coin).Count;
            for (int i = onField; i < wanted; i++)
                _Pickups.Add(new G10_CoinPickup(FreeTile(), G10_PickupKind.Coin));
        }

        // A random floor tile that is empty and not right next to the player.
        G10_Tile FreeTile()
        {
            List<G10_Tile> near = new List<G10_Tile>();
            List<G10_Tile> far = new List<G10_Tile>();
            foreach (G10_Tile tile in _Arena.FloorTiles())
            {
                if (tile == _Player.Tile || _BunnyTiles.Contains(tile) || _Pickups.Exists(p => p.Tile == tile)) continue;
                if (tile.DistanceSquaredTo(_Player.Tile) <= 2) near.Add(tile);
                else far.Add(tile);
            }
            List<G10_Tile> choices = far.Count > 0 ? far : near;
            return choices[_Random.Next(choices.Count)];
        }

        void ClearLevel()
        {
            // Seconds rounded up, as the clock shows them.
            int seconds = (TicksLeft + G10_CoinConfig.TicksPerSecond - 1) / G10_CoinConfig.TicksPerSecond;
            TimeBonus = seconds * G10_CoinConfig.TimeBonusPerSecond;
            AddPoints(TimeBonus);
            SetPhase(G10_CoinPhase.LevelCleared);
        }

        void LoseLife(bool timeRanOut)
        {
            Lives--;
            TimeRanOut = timeRanOut;
            SetPhase(G10_CoinPhase.Hit);
        }

        // After a lost life the player starts again at the start tile, with full time if it
        // had run out. The coins collected so far are kept.
        void Recover()
        {
            if (Lives == 0)
            {
                SetPhase(G10_CoinPhase.GameOver);
                return;
            }
            if (TimeRanOut) TicksLeft = _Level.TimeTicks;
            _Player.Respawn(_Arena.PlayerStart, true);
            SetPhase(G10_CoinPhase.Playing);
            // A coin may have appeared on the start tile in the meantime.
            Collect();
        }

        void StartLevel(int number)
        {
            _Level = G10_CoinLevel.ForLevel(number);
            _Arena = new G10_CoinArena(_Level.Layout);
            _Player.Respawn(_Arena.PlayerStart, false);
            _Pickups.Clear();
            CoinsCollected = 0;
            TicksLeft = _Level.TimeTicks;
            TimeBonus = 0;
            TimeRanOut = false;
            _DiamondTimer = 0;
            SpawnBunnies();
            RefillCoins();
            SetPhase(G10_CoinPhase.Ready);
        }

        // Dust bunnies start away from the player, on tiles they can leave diagonally.
        void SpawnBunnies()
        {
            _Bunnies.Clear();
            _BunnyTiles.Clear();
            int minDistance = G10_CoinConfig.SpawnDistance * G10_CoinConfig.SpawnDistance;
            List<G10_Tile> candidates = _Arena.FloorTiles().FindAll(tile =>
                tile.DistanceSquaredTo(_Arena.PlayerStart) >= minDistance && CanMoveDiagonally(tile));
            for (int i = 0; i < _Level.DustBunnies && candidates.Count > 0; i++)
            {
                int index = _Random.Next(candidates.Count);
                G10_DustBunny bunny = new G10_DustBunny(candidates[index], _Random);
                candidates.RemoveAt(index);
                _Bunnies.Add(bunny);
                _BunnyTiles.Add(bunny.Tile);
            }
        }

        bool CanMoveDiagonally(G10_Tile tile)
        {
            return !_Arena.IsBlocked(tile.Offset(1, 1)) || !_Arena.IsBlocked(tile.Offset(1, -1)) ||
                   !_Arena.IsBlocked(tile.Offset(-1, 1)) || !_Arena.IsBlocked(tile.Offset(-1, -1));
        }

        void AddPoints(int points)
        {
            Score += points;
            HighScore = Math.Max(HighScore, Score);
        }

        void SetPhase(G10_CoinPhase phase)
        {
            Phase = phase;
            PhaseTicks = 0;
        }
    }
}
