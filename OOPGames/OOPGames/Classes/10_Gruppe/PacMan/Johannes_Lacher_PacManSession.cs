using System;
using System.Collections.Generic;

namespace OOPGames
{
    public enum G10_PlayResult
    {
        Continue,
        PacManDied,
        LevelCleared
    }

    // One game of Pac-Man. Runs a tick in a fixed order and reacts to what the parts report
    // (collect results, collision outcomes, schedule events). Score, ghost logic and timing live
    // in their own classes; the session only connects them.
    public sealed class G10_PacManSession : IG10_PacManView
    {
        public static readonly int ReadyTicks = G10_PacManConstants.Seconds(2);
        public static readonly int DyingTicks = 65;
        public static readonly int LevelClearedTicks = G10_PacManConstants.Seconds(2);
        static readonly int GhostEatenFreezeTicks = G10_PacManConstants.Seconds(1);
        static readonly int FruitPopupTicks = G10_PacManConstants.Seconds(2);
        static readonly int FruitLifetimeTicks = G10_PacManConstants.Seconds(9.5);
        static readonly int[] FruitDotCounts = { 70, 170 };
        const int FruitHistoryLength = 7;

        readonly G10_Maze _Maze;
        readonly G10_PacMan _PacMan;
        readonly List<G10_Ghost> _Ghosts = new List<G10_Ghost>();
        readonly G10_GhostContext _Context;
        readonly G10_ModeScheduler _Scheduler;
        readonly G10_GhostHouse _House;
        readonly G10_ScoreKeeper _Score;
        readonly List<G10_FruitKind> _FruitHistory = new List<G10_FruitKind>();
        readonly List<G10_PointsPopup> _Popups = new List<G10_PointsPopup>();
        readonly List<int> _PopupTicks = new List<int>();
        readonly List<G10_SoundCue> _SoundCues = new List<G10_SoundCue>();
        IG10_PacManPhase _Phase;
        G10_LevelSettings _Settings;
        G10_BonusFruit _Fruit;
        int _FreezeTicks;
        int _UpdateCount;

        public G10_PacManSession(int highScore, int randomSeed) : this(G10_MazeLayouts.Classic, highScore, randomSeed)
        {
        }

        public G10_PacManSession(G10_MazeLayout layout, int highScore, int randomSeed)
        {
            _Maze = new G10_Maze(layout);
            _PacMan = new G10_PacMan(_Maze);
            _Ghosts.Add(new G10_Ghost(G10_GhostKind.Blinky, _Maze, new G10_DirectChase()));
            _Ghosts.Add(new G10_Ghost(G10_GhostKind.Pinky, _Maze, new G10_Ambush()));
            _Ghosts.Add(new G10_Ghost(G10_GhostKind.Inky, _Maze, new G10_Flank()));
            _Ghosts.Add(new G10_Ghost(G10_GhostKind.Clyde, _Maze, new G10_Shy()));
            _Context = new G10_GhostContext(_Maze, _PacMan, new Random(randomSeed));
            _Context.Blinky = _Ghosts[0];
            _Score = new G10_ScoreKeeper(highScore);
            _Settings = G10_LevelTable.ForLevel(1);
            _Scheduler = new G10_ModeScheduler(_Settings);
            _House = new G10_GhostHouse(_Ghosts);
            StartLevel(1);
            _Phase = G10_PacManPhases.Ready;
        }

        public G10_PacManPhaseKind Phase { get { return _Phase.Kind; } }
        public int PhaseTicks { get; private set; }
        public int Score { get { return _Score.Score; } }
        public int HighScore { get { return _Score.HighScore; } }
        public int Level { get { return _Settings.Number; } }
        public int Lives { get { return _Score.Lives; } }
        public IG10_MazeView Maze { get { return _Maze; } }
        public IG10_ActorView PacMan { get { return _PacMan; } }
        public IReadOnlyList<IG10_GhostView> Ghosts { get { return _Ghosts; } }
        public IG10_FruitView Fruit { get { return _Fruit; } }
        public IReadOnlyList<G10_FruitKind> FruitHistory { get { return _FruitHistory; } }
        public bool FrightenedFlashing { get { return _Scheduler.IsFlashing; } }
        public IReadOnlyList<G10_PointsPopup> Popups { get { return _Popups; } }
        public IReadOnlyList<G10_SoundCue> SoundCues { get { return _SoundCues; } }

        public void Update(G10_PacManInput input)
        {
            _SoundCues.Clear();
            if (_UpdateCount++ == 0) _SoundCues.Add(G10_SoundCue.GameStart);
            IG10_PacManPhase next = _Phase.Update(this, input);
            TickPopups();
            if (next == _Phase)
            {
                PhaseTicks++;
                return;
            }
            _Phase = next;
            PhaseTicks = 0;
        }

        internal void StartLevel(int level)
        {
            _Settings = G10_LevelTable.ForLevel(level);
            _Context.Level = _Settings;
            _Maze.Refill();
            _House.StartLevel(_Settings);
            _FruitHistory.Add(_Settings.Fruit);
            if (_FruitHistory.Count > FruitHistoryLength) _FruitHistory.RemoveAt(0);
            ResetActors();
        }

        internal void StartLevelAfterLostLife()
        {
            _House.StartAfterLostLife();
            ResetActors();
        }

        // Returns false when no life is left.
        internal bool LoseLife()
        {
            return _Score.LoseLife();
        }

        internal void ShowGameOver()
        {
            _PacMan.Visible = false;
            foreach (G10_Ghost ghost in _Ghosts) ghost.Visible = false;
        }

        internal G10_PlayResult PlayTick(G10_PacManInput input)
        {
            if (_FreezeTicks > 0)
            {
                _FreezeTicks--;
                if (_FreezeTicks == 0) EndFreeze();
                return G10_PlayResult.Continue;
            }

            int livesBefore = _Score.Lives;
            _PacMan.BeginTick();
            foreach (G10_Ghost ghost in _Ghosts) ghost.BeginTick();

            _PacMan.RequestDirection(input.Direction);
            _PacMan.Move(_Scheduler.IsFrightened ? _Settings.PacManFrightenedSpeed : _Settings.PacManSpeed);
            HandleCollect(_Maze.Collect(_PacMan.Tile));
            UpdateFruit();

            HandleScheduleEvent(_Scheduler.Tick());
            _Context.ActiveMode = _Scheduler.ActiveMode;
            _House.Tick();
            foreach (G10_Ghost ghost in _Ghosts) ghost.Update(_Context);

            G10_PlayResult result = CheckCollisions() ? G10_PlayResult.PacManDied
                : _Maze.DotsLeft == 0 ? G10_PlayResult.LevelCleared : G10_PlayResult.Continue;
            if (_Score.Lives > livesBefore) _SoundCues.Add(G10_SoundCue.ExtraLife);
            return result;
        }

        void ResetActors()
        {
            G10_MazeLayout layout = _Maze.Layout;
            _PacMan.Reset(layout.PacManStart);
            foreach (G10_Ghost ghost in _Ghosts)
            {
                bool outside = ghost.Kind == G10_GhostKind.Blinky;
                ghost.PlaceAt(layout.GhostStart(ghost.Kind), outside ? G10_Direction.Left : G10_Direction.Up);
                if (outside) ghost.SetMode(new G10_ScatterMode());
                else ghost.SetMode(new G10_InHouseMode());
            }
            _Scheduler.Reset(_Settings);
            _Context.ActiveMode = _Scheduler.ActiveMode;
            _Score.ResetGhostCombo();
            _Fruit = null;
            _FreezeTicks = 0;
            _Popups.Clear();
            _PopupTicks.Clear();
        }

        void HandleCollect(G10_CollectResult result)
        {
            if (result.IsNothing) return;
            _Score.Add(result.Points);
            _SoundCues.Add(G10_SoundCue.Chomp);
            _House.OnDotEaten();
            if (result.Effect == G10_CollectEffect.FrightenGhosts) FrightenGhosts();
            if (Array.IndexOf(FruitDotCounts, _Maze.DotsEaten) >= 0)
                _Fruit = new G10_BonusFruit(_Settings.Fruit, _Settings.FruitPoints, _Maze.Layout.FruitPosition, FruitLifetimeTicks);
        }

        void FrightenGhosts()
        {
            _Score.ResetGhostCombo();
            _Scheduler.StartFrightened(_Settings.FrightenedTicks);
            foreach (G10_Ghost ghost in _Ghosts)
            {
                if (!ghost.Mode.FollowsSchedule) continue;
                // From level 17 on there is no frightened time: the ghosts only turn around.
                if (_Settings.FrightenedTicks > 0) ghost.SetMode(new G10_FrightenedMode());
                ghost.ReverseDirection();
            }
        }

        void UpdateFruit()
        {
            if (_Fruit == null) return;
            if (_Fruit.IsTouchedBy(_PacMan))
            {
                G10_CollectResult result = _Fruit.Collect();
                _Score.Add(result.Points);
                _SoundCues.Add(G10_SoundCue.FruitEaten);
                AddPopup(new G10_PointsPopup(_Fruit.PixelX, _Fruit.PixelY, result.Points), FruitPopupTicks);
                _Fruit = null;
                return;
            }
            _Fruit.Tick();
            if (_Fruit.IsExpired) _Fruit = null;
        }

        void HandleScheduleEvent(G10_ScheduleEvent scheduleEvent)
        {
            if (scheduleEvent == G10_ScheduleEvent.None) return;
            _Context.ActiveMode = _Scheduler.ActiveMode;
            foreach (G10_Ghost ghost in _Ghosts)
            {
                if (scheduleEvent == G10_ScheduleEvent.ActiveModeChanged && ghost.Mode.FollowsSchedule)
                {
                    ghost.ReturnToActiveMode(_Context);
                    ghost.ReverseDirection();
                }
                else if (scheduleEvent == G10_ScheduleEvent.FrightenedEnded && ghost.Mode.Kind == G10_GhostModeKind.Frightened)
                {
                    ghost.ReturnToActiveMode(_Context);
                }
            }
        }

        // Returns true when Pac-Man was caught.
        bool CheckCollisions()
        {
            foreach (G10_Ghost ghost in _Ghosts)
            {
                if (!Touches(_PacMan, ghost)) continue;
                G10_CollisionOutcome outcome = ghost.Mode.OnCollision(ghost, _PacMan);
                if (outcome == G10_CollisionOutcome.PacManDies)
                {
                    _SoundCues.Add(G10_SoundCue.Death);
                    return true;
                }
                if (outcome == G10_CollisionOutcome.GhostEaten)
                {
                    _SoundCues.Add(G10_SoundCue.GhostEaten);
                    int points = _Score.AwardGhost();
                    AddPopup(new G10_PointsPopup(ghost.PixelX, ghost.PixelY, points), GhostEatenFreezeTicks);
                    StartFreeze(ghost);
                    return false;
                }
            }
            return false;
        }

        // Same tile, or both swapped tiles in this tick (otherwise fast actors pass through each other).
        static bool Touches(G10_MovingEntity a, G10_MovingEntity b)
        {
            if (a.Tile == b.Tile) return true;
            return a.Tile == b.PreviousTile && b.Tile == a.PreviousTile;
        }

        // After eating a ghost the game stops briefly; Pac-Man and the eaten ghost are hidden
        // while the points are shown.
        void StartFreeze(G10_Ghost eaten)
        {
            _FreezeTicks = GhostEatenFreezeTicks;
            _PacMan.Visible = false;
            eaten.Visible = false;
        }

        void EndFreeze()
        {
            _PacMan.Visible = true;
            foreach (G10_Ghost ghost in _Ghosts) ghost.Visible = true;
        }

        void AddPopup(G10_PointsPopup popup, int ticks)
        {
            _Popups.Add(popup);
            _PopupTicks.Add(ticks);
        }

        void TickPopups()
        {
            for (int i = _Popups.Count - 1; i >= 0; i--)
            {
                _PopupTicks[i]--;
                if (_PopupTicks[i] > 0) continue;
                _Popups.RemoveAt(i);
                _PopupTicks.RemoveAt(i);
            }
        }
    }

    // The phases of a game (State pattern). They hold no state of their own, so one instance
    // of each is shared.
    public static class G10_PacManPhases
    {
        public static readonly IG10_PacManPhase Ready = new G10_ReadyPhase();
        public static readonly IG10_PacManPhase Playing = new G10_PlayingPhase();
        public static readonly IG10_PacManPhase Paused = new G10_PausedPhase();
        public static readonly IG10_PacManPhase Dying = new G10_DyingPhase();
        public static readonly IG10_PacManPhase LevelCleared = new G10_LevelClearedPhase();
        public static readonly IG10_PacManPhase GameOver = new G10_GameOverPhase();
    }

    public sealed class G10_ReadyPhase : IG10_PacManPhase
    {
        public G10_PacManPhaseKind Kind { get { return G10_PacManPhaseKind.Ready; } }

        public IG10_PacManPhase Update(G10_PacManSession session, G10_PacManInput input)
        {
            return session.PhaseTicks + 1 >= G10_PacManSession.ReadyTicks ? G10_PacManPhases.Playing : this;
        }
    }

    public sealed class G10_PlayingPhase : IG10_PacManPhase
    {
        public G10_PacManPhaseKind Kind { get { return G10_PacManPhaseKind.Playing; } }

        public IG10_PacManPhase Update(G10_PacManSession session, G10_PacManInput input)
        {
            if (input.Confirm) return G10_PacManPhases.Paused;
            G10_PlayResult result = session.PlayTick(input);
            if (result == G10_PlayResult.PacManDied) return G10_PacManPhases.Dying;
            if (result == G10_PlayResult.LevelCleared) return G10_PacManPhases.LevelCleared;
            return this;
        }
    }

    public sealed class G10_PausedPhase : IG10_PacManPhase
    {
        public G10_PacManPhaseKind Kind { get { return G10_PacManPhaseKind.Paused; } }

        public IG10_PacManPhase Update(G10_PacManSession session, G10_PacManInput input)
        {
            return input.Confirm ? G10_PacManPhases.Playing : this;
        }
    }

    public sealed class G10_DyingPhase : IG10_PacManPhase
    {
        public G10_PacManPhaseKind Kind { get { return G10_PacManPhaseKind.Dying; } }

        public IG10_PacManPhase Update(G10_PacManSession session, G10_PacManInput input)
        {
            if (session.PhaseTicks + 1 < G10_PacManSession.DyingTicks) return this;
            if (!session.LoseLife())
            {
                session.ShowGameOver();
                return G10_PacManPhases.GameOver;
            }
            session.StartLevelAfterLostLife();
            return G10_PacManPhases.Ready;
        }
    }

    public sealed class G10_LevelClearedPhase : IG10_PacManPhase
    {
        public G10_PacManPhaseKind Kind { get { return G10_PacManPhaseKind.LevelCleared; } }

        public IG10_PacManPhase Update(G10_PacManSession session, G10_PacManInput input)
        {
            if (session.PhaseTicks + 1 < G10_PacManSession.LevelClearedTicks) return this;
            session.StartLevel(session.Level + 1);
            return G10_PacManPhases.Ready;
        }
    }

    // Final phase; the adapter starts a new game when the player confirms.
    public sealed class G10_GameOverPhase : IG10_PacManPhase
    {
        public G10_PacManPhaseKind Kind { get { return G10_PacManPhaseKind.GameOver; } }

        public IG10_PacManPhase Update(G10_PacManSession session, G10_PacManInput input)
        {
            return this;
        }
    }
}
