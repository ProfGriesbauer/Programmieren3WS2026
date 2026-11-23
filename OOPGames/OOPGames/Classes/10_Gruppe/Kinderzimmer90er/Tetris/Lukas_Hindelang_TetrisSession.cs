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
        public G10_TetrisHardDropEventArgs(G10_FallingPiece piece, int distance)
        {
            Piece = piece;
            Distance = distance;
        }

        public G10_FallingPiece Piece { get; }
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
        G10_FallingPiece CurrentPiece { get; }
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
        IG10_TetrisPhase _StateBeforePause;
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
        public G10_FallingPiece CurrentPiece { get { return _Controller.Piece; } }
        public int GhostY { get { return _Controller.GhostY; } }
        public float LockProgress { get { return _Controller.LockProgress; } }
        public G10_Tetromino HeldPiece { get; private set; }
        public bool CanHold { get { return !_HoldUsed; } }
        public IReadOnlyList<G10_Tetromino> NextPieces { get { return _Bag.Upcoming; } }
        public IReadOnlyList<int> ClearingRows { get; private set; }

        public void Start()
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
            SetState(G10_TetrisPhases.Ready);
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
            IG10_TetrisPhase next = Phase.Update(this, deltaSeconds, input);
            if (next != Phase) Phase = next;
        }

        internal void UpdatePlaying(float deltaSeconds, G10_TetrisInput input)
        {
            if (input.Hold && !_HoldUsed)
            {
                Hold();
                return;
            }

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
                LockPiece();
                return;
            }

            int softDropRows;
            bool lockNow = _Controller.ApplyGravity(deltaSeconds, input.SoftDrop, Level, out softDropRows);
            _ScoreKeeper.AddSoftDrop(softDropRows);
            if (lockNow) LockPiece();
        }

        void Hold()
        {
            G10_Tetromino current = CurrentPiece.Shape;
            G10_Tetromino next = HeldPiece ?? _Bag.Draw();
            HeldPiece = current;
            _HoldUsed = true;
            if (!_Controller.Spawn(next)) EndGame();
        }

        void LockPiece()
        {
            bool visible = _Board.Lock(CurrentPiece);
            _Controller.Remove();
            _HoldUsed = false;
            if (!visible)
            {
                EndGame();
                return;
            }

            ClearingRows = _Board.FindFullRows();
            G10_TetrisClearEventArgs clear = _ScoreKeeper.AddLock(ClearingRows.Count);
            if (clear == null)
            {
                SpawnNext();
                return;
            }

            SetState(G10_TetrisPhases.Clearing);
            EventHandler<G10_TetrisClearEventArgs> linesCleared = LinesCleared;
            if (linesCleared != null) linesCleared(this, clear);
        }

        internal void FinishClear()
        {
            _Board.RemoveRows(ClearingRows);
            ClearingRows = NoRows;
            SetState(G10_TetrisPhases.Playing);
            SpawnNext();
        }

        internal void SpawnNext()
        {
            if (!_Controller.Spawn(_Bag.Draw())) EndGame();
        }

        internal void Pause()
        {
            _StateBeforePause = Phase;
            _SecondsBeforePause = StateSeconds;
            SetState(G10_TetrisPhases.Paused);
        }

        internal void Resume()
        {
            Phase = _StateBeforePause;
            StateSeconds = _SecondsBeforePause;
            _Controller.ResetShift();
        }

        void EndGame()
        {
            IsNewBest = Score > 0 && Score > _Highscore.Best;
            _Highscore.Submit(Score);
            SetState(G10_TetrisPhases.GameOver);
        }

        internal void SetState(IG10_TetrisPhase phase)
        {
            if (phase == null) throw new ArgumentNullException("phase");
            Phase = phase;
            StateSeconds = 0;
        }
    }
}
