using System;
using System.Collections.Generic;

namespace OOPGames
{
    internal interface IG10_HighscoreStore
    {
        int Best { get; }
        void Submit(int score);
    }

    internal sealed class G10_TetrisClearEventArgs : EventArgs
    {
        public G10_TetrisClearEventArgs(int lines, int points, int combo, bool backToBack, bool levelUp)
        {
            Lines = lines;
            Points = points;
            Combo = combo;
            BackToBack = backToBack;
            LevelUp = levelUp;
        }

        public int Lines { get; }
        public int Points { get; }
        public int Combo { get; }
        public bool BackToBack { get; }
        public bool LevelUp { get; }
    }

    internal sealed class G10_TetrisHardDropEventArgs : EventArgs
    {
        public G10_TetrisHardDropEventArgs(IG10_FallingPieceView piece, int distance)
        {
            Piece = piece;
            Distance = distance;
        }

        public IG10_FallingPieceView Piece { get; }
        public int Distance { get; }
    }

    // Read-only view for the renderer.
    internal interface IG10_TetrisView
    {
        G10_TetrisState State { get; }
        float StateSeconds { get; }
        float PlaySeconds { get; }
        int Score { get; }
        int Lines { get; }
        int Level { get; }
        int Best { get; }
        bool IsNewBest { get; }
        int Combo { get; }
        bool BackToBack { get; }
        // Null while no piece is falling.
        IG10_FallingPieceView CurrentPiece { get; }
        int GhostY { get; }
        float LockProgress { get; }
        G10_Tetromino HeldPiece { get; }
        bool CanHold { get; }
        IReadOnlyList<G10_Tetromino> NextPieces { get; }
        IReadOnlyList<int> ClearingRows { get; }
        G10_Tetromino GetCell(int column, int row);
        event EventHandler<G10_TetrisClearEventArgs> LinesCleared;
        event EventHandler<G10_TetrisHardDropEventArgs> HardDropped;
    }

    internal sealed class G10_TetrisSession : IG10_TetrisView
    {
        static readonly int[] NoRows = new int[0];

        readonly G10_TetrisBoard _Board = new G10_TetrisBoard();
        readonly G10_TetrominoBag _Bag;
        readonly G10_TetrisScoreKeeper _ScoreKeeper = new G10_TetrisScoreKeeper();
        readonly G10_TetrisPieceController _Controller;
        readonly IG10_HighscoreStore _Highscore;
        IG10_TetrisPhase _PhaseBeforePause;
        float _SecondsBeforePause;
        bool _HoldUsed;

        public G10_TetrisSession(IG10_HighscoreStore highscore)
            : this(highscore, new Random()) { }

        public G10_TetrisSession(IG10_HighscoreStore highscore, Random random)
        {
            if (highscore == null) throw new ArgumentNullException("highscore");
            _Highscore = highscore;
            _Bag = new G10_TetrominoBag(random);
            _Controller = new G10_TetrisPieceController(_Board);
            ClearingRows = NoRows;
        }

        public event EventHandler<G10_TetrisClearEventArgs> LinesCleared;
        public event EventHandler<G10_TetrisHardDropEventArgs> HardDropped;

        internal IG10_TetrisPhase Phase { get; private set; }
        public G10_TetrisState State { get { return Phase.State; } }
        public float StateSeconds { get; private set; }
        public float PlaySeconds { get; private set; }
        public int Score { get { return _ScoreKeeper.Score; } }
        public int Lines { get { return _ScoreKeeper.Lines; } }
        public int Level { get { return _ScoreKeeper.Level; } }
        public int Combo { get { return _ScoreKeeper.Combo; } }
        public bool BackToBack { get { return _ScoreKeeper.BackToBack; } }
        public int Best { get { return Math.Max(_Highscore.Best, Score); } }
        public bool IsNewBest { get; private set; }
        // The view only gets the read-only piece; the session itself works with the model.
        IG10_FallingPieceView IG10_TetrisView.CurrentPiece { get { return _Controller.Piece; } }
        G10_FallingPiece CurrentPiece { get { return _Controller.Piece; } }
        public int GhostY { get { return _Controller.GhostY; } }
        public float LockProgress { get { return _Controller.LockProgress; } }
        public G10_Tetromino HeldPiece { get; private set; }
        public bool CanHold { get { return !_HoldUsed; } }
        public IReadOnlyList<G10_Tetromino> NextPieces { get { return _Bag.Upcoming; } }
        public IReadOnlyList<int> ClearingRows { get; private set; }

        // Called by the game. A restart after game over goes through the game over phase.
        public void Start()
        {
            ResetRound();
            Enter(G10_TetrisPhases.Ready);
        }

        public G10_Tetromino GetCell(int column, int row)
        {
            return _Board[column, row];
        }

        public void Update(float deltaSeconds, G10_TetrisInput input)
        {
            StateSeconds += deltaSeconds;
            if (State != G10_TetrisState.Paused && State != G10_TetrisState.GameOver)
                PlaySeconds += deltaSeconds;
            TransitionTo(Phase.Update(this, deltaSeconds, input));
        }

        // ---- Operations for the phases; each one returns the phase that should follow ----

        internal IG10_TetrisPhase PhaseBeforePause { get { return _PhaseBeforePause; } }

        internal void ResetRound()
        {
            _Board.Clear();
            _Bag.Reset();
            _ScoreKeeper.Reset();
            _Controller.Remove();
            _Controller.ResetShift();
            ClearingRows = NoRows;
            HeldPiece = null;
            _HoldUsed = false;
            IsNewBest = false;
            PlaySeconds = 0;
        }

        internal IG10_TetrisPhase UpdatePlaying(float deltaSeconds, G10_TetrisInput input)
        {
            if (input.Hold && !_HoldUsed)
                return Hold();

            if (input.RotateClockwise) _Controller.Rotate(true);
            if (input.RotateCounterClockwise) _Controller.Rotate(false);
            _Controller.Shift(input.HorizontalPressed, input.Horizontal, deltaSeconds);

            if (input.HardDrop)
            {
                int distance = _Controller.HardDrop();
                _ScoreKeeper.AddHardDrop(distance);
                EventHandler<G10_TetrisHardDropEventArgs> hardDropped = HardDropped;
                if (hardDropped != null)
                    hardDropped(this, new G10_TetrisHardDropEventArgs(CurrentPiece, distance));
                return LockPiece();
            }

            int softDropRows;
            bool lockNow = _Controller.ApplyGravity(deltaSeconds, input.SoftDrop, Level, out softDropRows);
            _ScoreKeeper.AddSoftDrop(softDropRows);
            return lockNow ? LockPiece() : G10_TetrisPhases.Playing;
        }

        internal IG10_TetrisPhase FinishClear()
        {
            _Board.RemoveRows(ClearingRows);
            ClearingRows = NoRows;
            return SpawnNext();
        }

        internal IG10_TetrisPhase SpawnNext()
        {
            return _Controller.Spawn(_Bag.Draw()) ? G10_TetrisPhases.Playing : EndGame();
        }

        IG10_TetrisPhase Hold()
        {
            G10_Tetromino current = CurrentPiece.Shape;
            G10_Tetromino next = HeldPiece ?? _Bag.Draw();
            HeldPiece = current;
            _HoldUsed = true;
            return _Controller.Spawn(next) ? G10_TetrisPhases.Playing : EndGame();
        }

        IG10_TetrisPhase LockPiece()
        {
            bool visible = _Board.Lock(CurrentPiece);
            _Controller.Remove();
            _HoldUsed = false;
            if (!visible) return EndGame();

            ClearingRows = _Board.FindFullRows();
            G10_TetrisClearEventArgs clear = _ScoreKeeper.AddLock(ClearingRows.Count);
            if (clear == null) return SpawnNext();

            EventHandler<G10_TetrisClearEventArgs> linesCleared = LinesCleared;
            if (linesCleared != null) linesCleared(this, clear);
            return G10_TetrisPhases.Clearing;
        }

        IG10_TetrisPhase EndGame()
        {
            IsNewBest = Score > 0 && Score > _Highscore.Best;
            _Highscore.Submit(Score);
            return G10_TetrisPhases.GameOver;
        }

        // The only place where the phase changes. Pausing remembers the interrupted phase
        // and its elapsed time, so resuming continues exactly where the round stopped.
        void TransitionTo(IG10_TetrisPhase next)
        {
            if (next == null) throw new InvalidOperationException("A Tetris phase returned no next phase.");
            if (next == Phase) return;

            if (next == G10_TetrisPhases.Paused)
            {
                _PhaseBeforePause = Phase;
                _SecondsBeforePause = StateSeconds;
                Enter(next);
                return;
            }

            if (Phase == G10_TetrisPhases.Paused)
            {
                Phase = next;
                StateSeconds = _SecondsBeforePause;
                _Controller.ResetShift();
                return;
            }

            Enter(next);
        }

        void Enter(IG10_TetrisPhase phase)
        {
            Phase = phase;
            StateSeconds = 0;
        }
    }
}
