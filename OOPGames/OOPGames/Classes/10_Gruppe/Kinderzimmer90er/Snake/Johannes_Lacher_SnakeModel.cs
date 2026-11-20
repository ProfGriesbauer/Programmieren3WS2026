using System;
using System.Collections.Generic;

namespace OOPGames
{
    public static class G10_SnakeConfig
    {
        public const int Columns = 19;
        public const int Rows = 15;
        public const int StartLength = 4;
        public const int ApplesPerLevel = 5;
        public const int MaxLevel = 9;
        // A bug appears after every few apples.
        public const int ApplesPerBug = 4;
        // The hub ticks every 40 ms.
        public const double TickSeconds = 0.04;
        public const int DyingTicks = 30;
        public const int BugLifetimeTicks = 150;

        // Level 1 moves 5 tiles per second, level 9 about 12.
        public static double StepsPerSecond(int level)
        {
            return 5 + (level - 1) * 0.9;
        }

        public static int ApplePoints(int level)
        {
            return 10 * level;
        }

        // The sooner the bug is caught, the more it is worth.
        public static int BugPoints(int level, int ticksLeft)
        {
            return level * (20 + ticksLeft / 5);
        }
    }

    // The tiles of the snake, head first. Growing is spread over the next steps: the tail
    // simply stays where it is while growth is pending.
    public sealed class G10_SnakeBody
    {
        readonly List<G10_Tile> _Segments = new List<G10_Tile>();
        int _Growth;

        public G10_SnakeBody(G10_Tile head, G10_Direction heading, int length)
        {
            Heading = heading;
            for (int i = 0; i < length; i++)
                _Segments.Add(head.Offset(-heading.DeltaX() * i, -heading.DeltaY() * i));
        }

        public IReadOnlyList<G10_Tile> Segments { get { return _Segments; } }
        public G10_Tile Head { get { return _Segments[0]; } }
        public G10_Direction Heading { get; private set; }
        public int Length { get { return _Segments.Count; } }

        public void Grow(int segments)
        {
            _Growth += segments;
        }

        public bool Occupies(G10_Tile tile)
        {
            return _Segments.Contains(tile);
        }

        // The tail leaves its tile in the same step, so the head may follow it there,
        // unless the snake is growing and the tail stays.
        public bool WouldBite(G10_Tile next)
        {
            int count = _Growth > 0 ? _Segments.Count : _Segments.Count - 1;
            for (int i = 0; i < count; i++)
                if (_Segments[i] == next) return true;
            return false;
        }

        public void MoveTo(G10_Tile next, G10_Direction heading)
        {
            _Segments.Insert(0, next);
            Heading = heading;
            if (_Growth > 0) _Growth--;
            else _Segments.RemoveAt(_Segments.Count - 1);
        }
    }

    // Keeps up to two turns pressed between two steps, so quick turns (e.g. up, then left)
    // are not lost. A turn back into the snake's own neck is never accepted.
    public sealed class G10_TurnBuffer
    {
        const int Capacity = 2;
        readonly Queue<G10_Direction> _Turns = new Queue<G10_Direction>();
        G10_Direction _Last;

        public void Reset(G10_Direction heading)
        {
            _Turns.Clear();
            _Last = heading;
        }

        public bool Add(G10_Direction direction)
        {
            if (direction == G10_Direction.None || direction == _Last || direction == _Last.Opposite()) return false;
            if (_Turns.Count == Capacity) return false;
            _Turns.Enqueue(direction);
            _Last = direction;
            return true;
        }

        public G10_Direction Next(G10_Direction heading)
        {
            return _Turns.Count > 0 ? _Turns.Dequeue() : heading;
        }
    }

    // One game of Snake. Knows nothing about keys, WPF or files.
    public sealed class G10_SnakeSession : IG10_SnakeView
    {
        readonly Random _Random;
        readonly G10_SnakeBody _Body;
        readonly G10_TurnBuffer _Turns = new G10_TurnBuffer();
        readonly int _StartHighScore;
        double _StepProgress;
        int _Apples;

        public G10_SnakeSession(int highScore, int randomSeed)
        {
            _Random = new Random(randomSeed);
            _StartHighScore = highScore;
            HighScore = highScore;
            Level = 1;
            _Body = new G10_SnakeBody(new G10_Tile(Columns / 2, Rows / 2), G10_Direction.Right, G10_SnakeConfig.StartLength);
            _Turns.Reset(_Body.Heading);
            PlaceApple();
            Phase = G10_SnakePhase.Ready;
        }

        public int Columns { get { return G10_SnakeConfig.Columns; } }
        public int Rows { get { return G10_SnakeConfig.Rows; } }
        public G10_SnakePhase Phase { get; private set; }
        public int PhaseTicks { get; private set; }
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public bool IsNewHighScore { get { return Score > _StartHighScore; } }
        public int Level { get; private set; }
        public bool Won { get; private set; }
        public IReadOnlyList<G10_Tile> Body { get { return _Body.Segments; } }
        public G10_Direction Heading { get { return _Body.Heading; } }
        public G10_Tile Apple { get; private set; }
        public bool HasBug { get { return BugTicksLeft > 0; } }
        public G10_Tile Bug { get; private set; }
        public int BugTicksLeft { get; private set; }
        public int Length { get { return _Body.Length; } }

        public void Update(G10_SnakeInput input)
        {
            PhaseTicks++;
            switch (Phase)
            {
                case G10_SnakePhase.Ready:
                    bool turned = false;
                    foreach (G10_Direction turn in input.Turns) turned |= _Turns.Add(turn);
                    if (turned || input.Confirm) SetPhase(G10_SnakePhase.Playing);
                    break;
                case G10_SnakePhase.Playing:
                    if (input.Confirm)
                    {
                        SetPhase(G10_SnakePhase.Paused);
                        return;
                    }
                    foreach (G10_Direction turn in input.Turns) _Turns.Add(turn);
                    Advance();
                    if (HasBug) BugTicksLeft--;
                    break;
                case G10_SnakePhase.Paused:
                    if (input.Confirm) SetPhase(G10_SnakePhase.Playing);
                    break;
                case G10_SnakePhase.Dying:
                    if (PhaseTicks >= G10_SnakeConfig.DyingTicks) SetPhase(G10_SnakePhase.GameOver);
                    break;
            }
        }

        void Advance()
        {
            _StepProgress += G10_SnakeConfig.StepsPerSecond(Level) * G10_SnakeConfig.TickSeconds;
            while (_StepProgress >= 1 && Phase == G10_SnakePhase.Playing)
            {
                _StepProgress -= 1;
                Step();
            }
        }

        void Step()
        {
            G10_Direction heading = _Turns.Next(_Body.Heading);
            G10_Tile next = _Body.Head.Neighbour(heading);
            if (!IsInside(next) || _Body.WouldBite(next))
            {
                SetPhase(G10_SnakePhase.Dying);
                return;
            }

            _Body.MoveTo(next, heading);
            if (next == Apple) EatApple();
            else if (HasBug && next == Bug) EatBug();
        }

        void EatApple()
        {
            AddPoints(G10_SnakeConfig.ApplePoints(Level));
            _Apples++;
            _Body.Grow(1);
            Level = Math.Min(G10_SnakeConfig.MaxLevel, 1 + _Apples / G10_SnakeConfig.ApplesPerLevel);
            if (!PlaceApple())
            {
                Won = true;
                SetPhase(G10_SnakePhase.GameOver);
                return;
            }
            if (_Apples % G10_SnakeConfig.ApplesPerBug == 0 && !HasBug) PlaceBug();
        }

        void EatBug()
        {
            AddPoints(G10_SnakeConfig.BugPoints(Level, BugTicksLeft));
            BugTicksLeft = 0;
        }

        void AddPoints(int points)
        {
            Score += points;
            HighScore = Math.Max(HighScore, Score);
        }

        bool PlaceApple()
        {
            List<G10_Tile> free = FreeTiles();
            if (free.Count == 0) return false;
            Apple = free[_Random.Next(free.Count)];
            return true;
        }

        void PlaceBug()
        {
            List<G10_Tile> free = FreeTiles();
            free.Remove(Apple);
            if (free.Count == 0) return;
            Bug = free[_Random.Next(free.Count)];
            BugTicksLeft = G10_SnakeConfig.BugLifetimeTicks;
        }

        List<G10_Tile> FreeTiles()
        {
            List<G10_Tile> free = new List<G10_Tile>();
            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Columns; col++)
                {
                    G10_Tile tile = new G10_Tile(col, row);
                    if (!_Body.Occupies(tile) && !(HasBug && tile == Bug)) free.Add(tile);
                }
            }
            return free;
        }

        bool IsInside(G10_Tile tile)
        {
            return tile.Col >= 0 && tile.Col < Columns && tile.Row >= 0 && tile.Row < Rows;
        }

        void SetPhase(G10_SnakePhase phase)
        {
            Phase = phase;
            PhaseTicks = 0;
        }
    }
}
