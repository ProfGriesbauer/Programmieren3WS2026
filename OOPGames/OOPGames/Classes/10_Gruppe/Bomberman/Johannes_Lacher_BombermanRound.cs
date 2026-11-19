using System;
using System.Collections.Generic;

namespace OOPGames
{
    public enum G10_BomberRoundResult
    {
        Running,
        PlayerDied,
        StageCleared
    }

    public sealed class G10_BomberScore
    {
        public G10_BomberScore(int highScore)
        {
            HighScore = Math.Max(0, highScore);
        }

        public int Score { get; private set; }
        public int HighScore { get; private set; }

        public void Add(int points)
        {
            if (points < 0) throw new ArgumentOutOfRangeException(nameof(points));
            Score += points;
            HighScore = Math.Max(HighScore, Score);
        }
    }

    // One attempt at one stage: arena, player, enemies, bombs and the timer.
    // A lost life throws the round away and the stage starts again with a new one.
    // Where enemies come from is up to the G10_BomberSpawner.
    public sealed class G10_BomberRound : IG10_BomberObstacles
    {
        public const int StageSeconds = 200;
        const int PowerUpPoints = 1000;
        // Centres closer than this on both axes mean the enemy caught the player.
        const int TouchPixels = 11;
        static readonly int PopupTicks = G10_BomberConstants.Seconds(1);
        static readonly int TimeUpWarningTicks = G10_BomberConstants.Seconds(2);

        readonly G10_BomberStage _Stage;
        readonly G10_BomberScore _Score;
        readonly ICollection<G10_BomberSoundCue> _Cues;
        readonly G10_BomberArena _Arena;
        readonly G10_BomberBlastSystem _Blasts;
        readonly G10_BomberPlayer _Player;
        readonly List<G10_BomberEnemy> _Enemies = new List<G10_BomberEnemy>();
        readonly G10_BomberSpawner _Spawner;
        readonly List<G10_BomberPopup> _Popups = new List<G10_BomberPopup>();
        readonly G10_BomberBrainContext _BrainContext;
        int _TicksLeft = StageSeconds * G10_BomberConstants.TicksPerSecond;
        int _TimeUpWarning;
        bool _ExitWasOpen;

        // cues collects what can be heard; the session empties it before each update.
        public G10_BomberRound(G10_BomberStage stage, G10_BomberPlayerStats stats, G10_BomberScore score, Random random, ICollection<G10_BomberSoundCue> cues)
        {
            if (stage == null) throw new ArgumentNullException(nameof(stage));
            if (score == null) throw new ArgumentNullException(nameof(score));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (cues == null) throw new ArgumentNullException(nameof(cues));
            _Stage = stage;
            _Score = score;
            _Cues = cues;
            _Arena = G10_BomberArenaGenerator.Build(stage, random);
            _Blasts = new G10_BomberBlastSystem(_Arena);
            _Player = new G10_BomberPlayer(this, stats);
            _BrainContext = new G10_BomberBrainContext(random);
            _Spawner = new G10_BomberSpawner(_Arena, this, _Enemies, random);
            _Spawner.SpawnAwayFrom(_Player.Cell, stage.Enemies);
        }

        public G10_BomberArena Arena { get { return _Arena; } }
        public G10_BomberPlayer Player { get { return _Player; } }
        public IReadOnlyList<G10_BomberEnemy> Enemies { get { return _Enemies; } }
        public IReadOnlyList<G10_BomberBomb> Bombs { get { return _Blasts.Bombs; } }
        public IReadOnlyList<G10_BomberFlame> Flames { get { return _Blasts.Flames; } }
        public IReadOnlyList<G10_BomberPopup> Popups { get { return _Popups; } }
        public G10_BomberCell ExitCell { get { return _Spawner.ExitCell; } }
        public int SecondsLeft { get { return (_TicksLeft + G10_BomberConstants.TicksPerSecond - 1) / G10_BomberConstants.TicksPerSecond; } }
        public bool TimeUpWarning { get { return _TimeUpWarning > 0; } }
        public bool ExitOpen { get { return !_Enemies.Exists(e => !e.IsDying) && !_Spawner.DoorGuardsPending; } }

        public bool IsBlocked(G10_BomberCell cell, bool passBricks)
        {
            return _Arena.BlocksWalking(cell, passBricks) || _Blasts.HasBombAt(cell);
        }

        public G10_BomberRoundResult Tick(G10_BomberInput input)
        {
            bool wasAlive = !_Player.IsDead;
            int bombsBefore = _Blasts.Bombs.Count;
            _Arena.Tick();
            TickTimer();
            if (wasAlive) HandlePlayer(input, ref bombsBefore);
            else _Player.Update(G10_Direction.None);

            _Blasts.Tick();
            if (_Blasts.Bombs.Count < bombsBefore) _Cues.Add(G10_BomberSoundCue.Explosion);
            if (_Blasts.ConsumeExitHit()) _Spawner.ExitHit();
            if (_Spawner.Tick(_Stage.DoorGuard, _Player.Cell)) _Cues.Add(G10_BomberSoundCue.DoorGuards);
            MoveEnemies();
            BurnEnemies();
            AgePopups();
            CheckExitOpened();

            if (!wasAlive) return G10_BomberRoundResult.Running;
            if (_Blasts.IsBurning(_Player.Cell) || _Enemies.Exists(e => !e.IsDying && e.Touches(_Player, TouchPixels)))
            {
                _Player.Kill();
                _Cues.Add(G10_BomberSoundCue.PlayerDied);
                return G10_BomberRoundResult.PlayerDied;
            }
            if (ExitOpen && _Player.Cell == ExitCell && _Arena.VisibleItemAt(ExitCell) == G10_BomberItemKind.Exit)
                return G10_BomberRoundResult.StageCleared;
            return G10_BomberRoundResult.Running;
        }

        // bombsBefore is raised for a new bomb, so only bombs that went off count as an explosion.
        void HandlePlayer(G10_BomberInput input, ref int bombsBefore)
        {
            if (input.PlaceBomb && _Blasts.TryPlace(_Player.Cell, _Player.Stats))
            {
                bombsBefore++;
                _Cues.Add(G10_BomberSoundCue.BombPlaced);
            }
            if (input.Detonate && _Player.HasRemote) _Blasts.DetonateOldestRemote();
            _Player.Update(input.Move);

            G10_BomberItemKind item = _Arena.VisibleItemAt(_Player.Cell);
            if (item != G10_BomberItemKind.None && item != G10_BomberItemKind.Exit)
            {
                _Player.Stats.Apply(item);
                _Arena.RemoveItem(_Player.Cell);
                _Score.Add(PowerUpPoints);
                _Cues.Add(G10_BomberSoundCue.PowerUp);
            }
        }

        // When the time runs out, phantoms come to hunt the player.
        void TickTimer()
        {
            if (_TimeUpWarning > 0) _TimeUpWarning--;
            if (_TicksLeft == 0) return;
            _TicksLeft--;
            if (_TicksLeft > 0) return;
            _TimeUpWarning = TimeUpWarningTicks;
            _Spawner.SpawnPhantoms(_Player.Cell);
            _Cues.Add(G10_BomberSoundCue.TimeUp);
        }

        void MoveEnemies()
        {
            _BrainContext.PlayerCell = _Player.Cell;
            _BrainContext.PlayerAlive = !_Player.IsDead;
            foreach (G10_BomberEnemy enemy in _Enemies)
            {
                enemy.Update(_BrainContext);
            }
            _Enemies.RemoveAll(e => e.IsGone);
        }

        void BurnEnemies()
        {
            foreach (G10_BomberEnemy enemy in _Enemies)
            {
                if (enemy.IsDying || !_Blasts.IsBurning(enemy.Cell)) continue;
                enemy.Kill();
                _Score.Add(enemy.Points);
                _Popups.Add(new G10_BomberPopup(enemy.PixelX, enemy.PixelY, enemy.Points, PopupTicks));
                _Cues.Add(G10_BomberSoundCue.EnemyDefeated);
            }
        }

        void AgePopups()
        {
            for (int i = _Popups.Count - 1; i >= 0; i--)
            {
                G10_BomberPopup aged = _Popups[i].Aged();
                if (aged.TicksLeft <= 0) _Popups.RemoveAt(i);
                else _Popups[i] = aged;
            }
        }

        void CheckExitOpened()
        {
            bool open = ExitOpen;
            if (open && !_ExitWasOpen) _Cues.Add(G10_BomberSoundCue.ExitOpened);
            _ExitWasOpen = open;
        }
    }
}
