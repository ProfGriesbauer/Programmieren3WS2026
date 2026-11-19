using System;
using System.Collections.Generic;

namespace OOPGames
{
    internal static class G10_TetrisConfig
    {
        public const int Columns = 10;
        public const int VisibleRows = 20;
        public const int HiddenRows = 2;
        public const int Rows = VisibleRows + HiddenRows;
        public const int PreviewCount = 3;
        public const float FixedDeltaSeconds = 0.04f;
        public const float ReadySeconds = 1.4f;
        public const float AutoShiftDelay = 0.16f;
        public const float AutoShiftInterval = 0.08f;
        public const float SoftDropInterval = 0.04f;
        public const float LockDelay = 0.5f;
        public const int MaxLockResets = 15;
        public const float LineClearSeconds = 0.36f;
        public const float GameOverInputDelay = 0.8f;
        public const int LinesPerLevel = 10;

        // Game Boy Tetris: frames per row at 59.73 fps. Level 1 uses the first entry.
        static readonly int[] FramesPerRow =
            { 53, 49, 45, 41, 37, 33, 28, 22, 17, 11, 10, 9, 8, 7, 6, 6, 5, 5, 4, 4, 3 };

        public static int MaxLevel { get { return FramesPerRow.Length; } }

        public static float DropInterval(int level)
        {
            int index = Math.Max(0, Math.Min(FramesPerRow.Length - 1, level - 1));
            return FramesPerRow[index] / 59.73f;
        }
    }

    internal enum G10_TetrisState
    {
        Ready,
        Playing,
        Clearing,
        Paused,
        GameOver
    }

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

    // 7-bag randomizer with a preview queue: every shape comes once per bag of seven.
    internal sealed class G10_TetrominoBag
    {
        readonly Random _Random;
        readonly IReadOnlyList<G10_Tetromino> _AllPieces = G10_Tetromino.CreateAll();
        readonly List<G10_Tetromino> _Queue = new List<G10_Tetromino>();

        public G10_TetrominoBag(Random random)
        {
            if (random == null) throw new ArgumentNullException("random");
            _Random = random;
        }

        public IReadOnlyList<G10_Tetromino> Upcoming { get { return _Queue; } }

        public void Reset()
        {
            _Queue.Clear();
            Fill();
        }

        public G10_Tetromino Draw()
        {
            Fill();
            G10_Tetromino piece = _Queue[0];
            _Queue.RemoveAt(0);
            Fill();
            return piece;
        }

        void Fill()
        {
            while (_Queue.Count <= G10_TetrisConfig.PreviewCount)
            {
                int start = _Queue.Count;
                _Queue.AddRange(_AllPieces);
                for (int i = _Queue.Count - 1; i > start; i--)
                {
                    int swapIndex = start + _Random.Next(i - start + 1);
                    G10_Tetromino piece = _Queue[i];
                    _Queue[i] = _Queue[swapIndex];
                    _Queue[swapIndex] = piece;
                }
            }
        }
    }

    // Guideline scoring: line clears times level, combo bonus, back-to-back Tetris bonus.
    internal sealed class G10_TetrisScoreKeeper
    {
        static readonly int[] LineScores = { 0, 100, 300, 500, 800 };

        public int Score { get; private set; }
        public int Lines { get; private set; }
        public int Combo { get; private set; }
        public bool BackToBack { get; private set; }
        public int Level
        {
            get { return Math.Min(G10_TetrisConfig.MaxLevel, 1 + Lines / G10_TetrisConfig.LinesPerLevel); }
        }

        public void Reset()
        {
            Score = 0;
            Lines = 0;
            Combo = -1;
            BackToBack = false;
        }

        public void AddSoftDrop(int rows) { Score += rows; }
        public void AddHardDrop(int rows) { Score += rows * 2; }

        public G10_TetrisClearEventArgs AddLock(int clearedLines)
        {
            if (clearedLines == 0)
            {
                Combo = -1;
                return null;
            }

            int level = Level;
            Combo++;
            bool tetris = clearedLines == 4;
            bool backToBack = tetris && BackToBack;
            int points = LineScores[clearedLines] * level;
            if (backToBack) points += points / 2;
            points += 50 * Combo * level;

            BackToBack = tetris;
            Score += points;
            Lines += clearedLines;
            return new G10_TetrisClearEventArgs(clearedLines, points, Combo, backToBack, Level > level);
        }
    }

    // Moves the falling piece: auto shift, SRS rotation, gravity and lock delay.
    internal sealed class G10_TetrisPieceController
    {
        readonly G10_TetrisBoard _Board;
        int _ShiftDirection;
        float _ShiftTimer;
        float _GravityTimer;
        float _LockTimer;
        int _LockResets;
        int _LowestY;

        public G10_TetrisPieceController(G10_TetrisBoard board)
        {
            if (board == null) throw new ArgumentNullException("board");
            _Board = board;
        }

        public G10_FallingPiece Piece { get; private set; }
        public float LockProgress { get { return Math.Min(1, _LockTimer / G10_TetrisConfig.LockDelay); } }
        public int GhostY { get { return Piece == null ? 0 : Piece.Y + _Board.DropDistance(Piece); } }

        bool IsGrounded
        {
            get { return !_Board.CanPlace(Piece.Shape, Piece.X, Piece.Y + 1, Piece.Rotation); }
        }

        // The lowest row of the spawn rotation sits in the first visible row; one row higher
        // if that is blocked. Returns false if the piece does not fit at all (block out).
        public bool Spawn(G10_Tetromino shape)
        {
            int bottom = 0;
            foreach (G10_TetrominoCell cell in shape.Cells(0))
                bottom = Math.Max(bottom, cell.Y);

            int x = (G10_TetrisConfig.Columns - shape.BoxSize) / 2;
            int y = G10_TetrisConfig.HiddenRows - bottom;
            if (!_Board.CanPlace(shape, x, y, 0)) y--;
            Piece = new G10_FallingPiece(shape, x, y);
            _GravityTimer = 0;
            _LockTimer = 0;
            _LockResets = 0;
            _LowestY = y;
            return _Board.CanPlace(shape, x, y, 0);
        }

        public void Remove()
        {
            Piece = null;
        }

        public void ResetShift()
        {
            _ShiftDirection = 0;
            _ShiftTimer = 0;
        }

        // A fresh press moves at once; holding the key repeats after a short delay.
        public void Shift(int pressedDirection, int heldDirection, float deltaSeconds)
        {
            if (pressedDirection != 0)
            {
                _ShiftDirection = pressedDirection;
                _ShiftTimer = G10_TetrisConfig.AutoShiftDelay;
                TryMove(pressedDirection, 0);
                return;
            }

            if (heldDirection == 0)
            {
                ResetShift();
                return;
            }

            if (heldDirection != _ShiftDirection)
            {
                _ShiftDirection = heldDirection;
                _ShiftTimer = G10_TetrisConfig.AutoShiftDelay;
                return;
            }

            _ShiftTimer -= deltaSeconds;
            while (_ShiftTimer <= 0)
            {
                _ShiftTimer += G10_TetrisConfig.AutoShiftInterval;
                if (!TryMove(_ShiftDirection, 0)) break;
            }
        }

        public bool Rotate(bool clockwise)
        {
            if (!Piece.Shape.CanRotate) return false;

            int target = (Piece.Rotation + (clockwise ? 1 : 3)) % 4;
            foreach (G10_TetrominoCell kick in Piece.Shape.Kicks.Offsets(Piece.Rotation, clockwise))
            {
                int x = Piece.X + kick.X;
                int y = Piece.Y + kick.Y;
                if (!_Board.CanPlace(Piece.Shape, x, y, target)) continue;
                Piece.Place(x, y, target);
                OnMoved();
                return true;
            }
            return false;
        }

        public int HardDrop()
        {
            int distance = _Board.DropDistance(Piece);
            Piece.Place(Piece.X, Piece.Y + distance, Piece.Rotation);
            return distance;
        }

        // Returns true once the piece has rested on the ground for the whole lock delay.
        public bool ApplyGravity(float deltaSeconds, bool softDrop, int level, out int softDropRows)
        {
            softDropRows = 0;
            float interval = G10_TetrisConfig.DropInterval(level);
            if (softDrop) interval = Math.Min(interval, G10_TetrisConfig.SoftDropInterval);

            _GravityTimer += deltaSeconds;
            while (_GravityTimer >= interval)
            {
                _GravityTimer -= interval;
                if (!TryMove(0, 1))
                {
                    _GravityTimer = 0;
                    break;
                }
                if (softDrop) softDropRows++;
            }

            if (!IsGrounded)
            {
                _LockTimer = 0;
                return false;
            }

            _LockTimer += deltaSeconds;
            return _LockTimer >= G10_TetrisConfig.LockDelay;
        }

        bool TryMove(int dx, int dy)
        {
            int x = Piece.X + dx;
            int y = Piece.Y + dy;
            if (!_Board.CanPlace(Piece.Shape, x, y, Piece.Rotation))
                return false;
            Piece.Place(x, y, Piece.Rotation);
            OnMoved();
            return true;
        }

        // Move reset: moving or turning a grounded piece restarts the lock delay, but only
        // MaxLockResets times per row, so a piece cannot be kept alive forever.
        void OnMoved()
        {
            if (Piece.Y > _LowestY)
            {
                _LowestY = Piece.Y;
                _LockResets = 0;
                _LockTimer = 0;
                return;
            }

            if (_LockTimer > 0 && _LockResets < G10_TetrisConfig.MaxLockResets)
            {
                _LockResets++;
                _LockTimer = 0;
            }
        }
    }

    internal sealed class G10_TetrisSession : IG10_TetrisView
    {
        static readonly int[] NoRows = new int[0];

        readonly G10_TetrisBoard _Board = new G10_TetrisBoard();
        readonly G10_TetrominoBag _Bag;
        readonly G10_TetrisScoreKeeper _ScoreKeeper = new G10_TetrisScoreKeeper();
        readonly G10_TetrisPieceController _Controller;
        readonly IG10_HighscoreStore _Highscore;
        G10_TetrisState _StateBeforePause;
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

        public G10_TetrisState State { get; private set; }
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
            SetState(G10_TetrisState.Ready);
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

            switch (State)
            {
                case G10_TetrisState.Ready:
                    if (StateSeconds >= G10_TetrisConfig.ReadySeconds)
                    {
                        SetState(G10_TetrisState.Playing);
                        SpawnNext();
                    }
                    break;
                case G10_TetrisState.Playing:
                    if (input.Start) Pause();
                    else UpdatePlaying(deltaSeconds, input);
                    break;
                case G10_TetrisState.Clearing:
                    if (input.Start) Pause();
                    else if (StateSeconds >= G10_TetrisConfig.LineClearSeconds) FinishClear();
                    break;
                case G10_TetrisState.Paused:
                    if (input.Start) Resume();
                    break;
                case G10_TetrisState.GameOver:
                    if (input.Start && StateSeconds >= G10_TetrisConfig.GameOverInputDelay) Start();
                    break;
            }
        }

        void UpdatePlaying(float deltaSeconds, G10_TetrisInput input)
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

            SetState(G10_TetrisState.Clearing);
            EventHandler<G10_TetrisClearEventArgs> linesCleared = LinesCleared;
            if (linesCleared != null) linesCleared(this, clear);
        }

        void FinishClear()
        {
            _Board.RemoveRows(ClearingRows);
            ClearingRows = NoRows;
            SetState(G10_TetrisState.Playing);
            SpawnNext();
        }

        void SpawnNext()
        {
            if (!_Controller.Spawn(_Bag.Draw())) EndGame();
        }

        void Pause()
        {
            _StateBeforePause = State;
            _SecondsBeforePause = StateSeconds;
            SetState(G10_TetrisState.Paused);
        }

        void Resume()
        {
            State = _StateBeforePause;
            StateSeconds = _SecondsBeforePause;
            _Controller.ResetShift();
        }

        void EndGame()
        {
            IsNewBest = Score > 0 && Score > _Highscore.Best;
            _Highscore.Submit(Score);
            SetState(G10_TetrisState.GameOver);
        }

        void SetState(G10_TetrisState state)
        {
            State = state;
            StateSeconds = 0;
        }
    }
}
