using System;

namespace OOPGames
{
    // Sizes in screen pixels, speeds in pixels per tick (the hub ticks every 40 ms).
    public static class G10_PongConfig
    {
        public const double Width = 160;
        public const double Top = 18;
        public const double Bottom = 142;
        public const double PaddleWidth = 4;
        public const double PaddleHeight = 22;
        public const double LeftPaddleX = 8;
        public const double RightPaddleX = Width - LeftPaddleX - PaddleWidth;
        public const double BallSize = 4;
        public const double PlayerSpeed = 2.6;
        public const double CpuSpeed = 1.8;
        public const double ServeSpeed = 2.2;
        public const double SpeedUp = 0.15;
        public const double MaxSpeed = 4.6;
        public const int PointsToWin = 7;
        public const int ServeTicks = 38;
        // Hitting the ball with the edge of the paddle sends it off at this angle.
        public static readonly double MaxBounceAngle = 55 * Math.PI / 180;
        public static readonly double MaxServeAngle = 30 * Math.PI / 180;
    }

    public sealed class G10_PongPaddle
    {
        public G10_PongPaddle(double x)
        {
            X = x;
            Center();
        }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double CenterY { get { return Y + G10_PongConfig.PaddleHeight / 2; } }

        public void Center()
        {
            Y = (G10_PongConfig.Top + G10_PongConfig.Bottom - G10_PongConfig.PaddleHeight) / 2;
        }

        public void Move(double distance)
        {
            Y = Math.Max(G10_PongConfig.Top, Math.Min(G10_PongConfig.Bottom - G10_PongConfig.PaddleHeight, Y + distance));
        }
    }

    public sealed class G10_PongBall
    {
        public G10_PongBall()
        {
            Reset();
        }

        public double X { get; private set; }
        public double Y { get; private set; }
        public double VelocityX { get; private set; }
        public double VelocityY { get; private set; }
        public double Speed { get; private set; }
        public double CenterY { get { return Y + G10_PongConfig.BallSize / 2; } }

        // Back to the middle, standing still.
        public void Reset()
        {
            X = (G10_PongConfig.Width - G10_PongConfig.BallSize) / 2;
            Y = (G10_PongConfig.Top + G10_PongConfig.Bottom - G10_PongConfig.BallSize) / 2;
            VelocityX = 0;
            VelocityY = 0;
            Speed = G10_PongConfig.ServeSpeed;
        }

        // direction: -1 to the left, 1 to the right. angle: 0 is straight, positive is downwards.
        public void Launch(int direction, double angle)
        {
            Speed = G10_PongConfig.ServeSpeed;
            SetVelocity(direction, angle);
        }

        // Moves one tick and bounces off the top and bottom walls.
        public void Move()
        {
            X += VelocityX;
            Y += VelocityY;
            if (Y < G10_PongConfig.Top)
            {
                Y = 2 * G10_PongConfig.Top - Y;
                VelocityY = Math.Abs(VelocityY);
            }
            double lowest = G10_PongConfig.Bottom - G10_PongConfig.BallSize;
            if (Y > lowest)
            {
                Y = 2 * lowest - Y;
                VelocityY = -Math.Abs(VelocityY);
            }
        }

        // Checks whether the ball crossed the front of the paddle during the last move
        // (it can be faster than the paddle is wide) and sends it back if so. Where the ball
        // meets the paddle sets the new angle; every hit makes the ball a little faster.
        public bool TryBounce(G10_PongPaddle paddle, G10_PongSide side, double previousX, double previousY)
        {
            bool left = side == G10_PongSide.Left;
            double face = left ? paddle.X + G10_PongConfig.PaddleWidth : paddle.X;
            double before = left ? previousX : previousX + G10_PongConfig.BallSize;
            double after = left ? X : X + G10_PongConfig.BallSize;
            bool crossed = left ? before >= face && after < face : before <= face && after > face;
            if (!crossed) return false;

            double share = (before - face) / (before - after);
            double yAtFace = previousY + share * (Y - previousY);
            if (yAtFace + G10_PongConfig.BallSize < paddle.Y || yAtFace > paddle.Y + G10_PongConfig.PaddleHeight) return false;

            double reach = (G10_PongConfig.PaddleHeight + G10_PongConfig.BallSize) / 2;
            double offset = (yAtFace + G10_PongConfig.BallSize / 2 - paddle.CenterY) / reach;
            offset = Math.Max(-1, Math.Min(1, offset));
            X = left ? face : face - G10_PongConfig.BallSize;
            Y = yAtFace;
            Speed = Math.Min(G10_PongConfig.MaxSpeed, Speed + G10_PongConfig.SpeedUp);
            SetVelocity(left ? 1 : -1, offset * G10_PongConfig.MaxBounceAngle);
            return true;
        }

        void SetVelocity(int direction, double angle)
        {
            VelocityX = direction * Speed * Math.Cos(angle);
            VelocityY = Speed * Math.Sin(angle);
        }
    }

    // A player's keys move the paddle.
    public sealed class G10_HumanPaddleControl : IG10_PaddleControl
    {
        readonly G10_PongSide _Side;

        public G10_HumanPaddleControl(G10_PongSide side)
        {
            _Side = side;
        }

        public double Move(G10_PongInput input, G10_PongBall ball, G10_PongPaddle paddle)
        {
            int axis = _Side == G10_PongSide.Left ? input.Left : input.Right;
            return axis * G10_PongConfig.PlayerSpeed;
        }
    }

    // The computer follows the ball while it comes towards its paddle, but is slower than a
    // player. It aims at a random point of its paddle, so it sends the ball back at changing
    // angles, and steep fast balls get past it.
    public sealed class G10_CpuPaddleControl : IG10_PaddleControl
    {
        const double DeadZone = 1.5;
        readonly Random _Random;
        double _Aim;
        bool _BallWasComing;

        public G10_CpuPaddleControl(Random random)
        {
            _Random = random;
        }

        public double Move(G10_PongInput input, G10_PongBall ball, G10_PongPaddle paddle)
        {
            bool coming = ball.VelocityX > 0;
            if (coming && !_BallWasComing)
                _Aim = (_Random.NextDouble() * 2 - 1) * (G10_PongConfig.PaddleHeight / 2 - 2);
            _BallWasComing = coming;

            double target = coming ? ball.CenterY + _Aim : (G10_PongConfig.Top + G10_PongConfig.Bottom) / 2;
            double distance = target - paddle.CenterY;
            if (Math.Abs(distance) < DeadZone) return 0;
            return Math.Max(-G10_PongConfig.CpuSpeed, Math.Min(G10_PongConfig.CpuSpeed, distance));
        }
    }

    // One Pong session: the menu and any number of matches. Knows nothing about keys, WPF or files.
    public sealed class G10_PongSession : IG10_PongView
    {
        readonly Random _Random;
        readonly int _StartBestRally;
        readonly G10_PongPaddle _Left = new G10_PongPaddle(G10_PongConfig.LeftPaddleX);
        readonly G10_PongPaddle _Right = new G10_PongPaddle(G10_PongConfig.RightPaddleX);
        readonly G10_PongBall _Ball = new G10_PongBall();
        IG10_PaddleControl _LeftControl;
        IG10_PaddleControl _RightControl;
        G10_PongPhase _PhaseBeforePause;
        int _TicksBeforePause;
        int _ServeDirection;

        public G10_PongSession(int bestRally, int randomSeed)
        {
            _Random = new Random(randomSeed);
            _StartBestRally = bestRally;
            BestRally = bestRally;
            Phase = G10_PongPhase.Menu;
        }

        public G10_PongPhase Phase { get; private set; }
        public int PhaseTicks { get; private set; }
        public G10_PongMode Mode { get; private set; }
        public int MenuSelection { get; private set; }
        public int LeftScore { get; private set; }
        public int RightScore { get; private set; }
        public bool HasScored { get; private set; }
        public G10_PongSide LastScorer { get; private set; }
        public G10_PongSide Winner { get; private set; }
        public int Rally { get; private set; }
        public int BestRally { get; private set; }
        public bool IsNewBestRally { get { return BestRally > _StartBestRally; } }
        public double LeftPaddleY { get { return _Left.Y; } }
        public double RightPaddleY { get { return _Right.Y; } }
        public double BallX { get { return _Ball.X; } }
        public double BallY { get { return _Ball.Y; } }
        public double BallVelocityX { get { return _Ball.VelocityX; } }
        public double BallVelocityY { get { return _Ball.VelocityY; } }

        public void Update(G10_PongInput input)
        {
            PhaseTicks++;
            switch (Phase)
            {
                case G10_PongPhase.Menu:
                    UpdateMenu(input);
                    break;
                case G10_PongPhase.Serving:
                case G10_PongPhase.Playing:
                    if (input.Confirm)
                    {
                        _PhaseBeforePause = Phase;
                        _TicksBeforePause = PhaseTicks;
                        SetPhase(G10_PongPhase.Paused);
                        return;
                    }
                    MovePaddles(input);
                    if (Phase == G10_PongPhase.Playing) MoveBall();
                    else if (PhaseTicks >= G10_PongConfig.ServeTicks) Serve();
                    break;
                case G10_PongPhase.Paused:
                    if (input.Confirm)
                    {
                        // A serve countdown goes on where it stopped.
                        Phase = _PhaseBeforePause;
                        PhaseTicks = _TicksBeforePause;
                    }
                    break;
                case G10_PongPhase.GameOver:
                    if (input.Confirm) SetPhase(G10_PongPhase.Menu);
                    break;
            }
        }

        void UpdateMenu(G10_PongInput input)
        {
            if (input.ModeKey == 1 || input.ModeKey == 2)
            {
                MenuSelection = input.ModeKey - 1;
                StartMatch();
                return;
            }
            if (input.MenuStep != 0) MenuSelection = 1 - MenuSelection;
            if (input.Confirm) StartMatch();
        }

        void StartMatch()
        {
            Mode = MenuSelection == 0 ? G10_PongMode.OnePlayer : G10_PongMode.TwoPlayers;
            _LeftControl = new G10_HumanPaddleControl(G10_PongSide.Left);
            _RightControl = Mode == G10_PongMode.OnePlayer
                ? (IG10_PaddleControl)new G10_CpuPaddleControl(_Random)
                : new G10_HumanPaddleControl(G10_PongSide.Right);
            LeftScore = 0;
            RightScore = 0;
            HasScored = false;
            _Left.Center();
            _Right.Center();
            _ServeDirection = _Random.Next(2) == 0 ? -1 : 1;
            StartServe();
        }

        void StartServe()
        {
            Rally = 0;
            _Ball.Reset();
            SetPhase(G10_PongPhase.Serving);
        }

        void Serve()
        {
            double angle = (_Random.NextDouble() * 2 - 1) * G10_PongConfig.MaxServeAngle;
            _Ball.Launch(_ServeDirection, angle);
            SetPhase(G10_PongPhase.Playing);
        }

        void MovePaddles(G10_PongInput input)
        {
            _Left.Move(_LeftControl.Move(input, _Ball, _Left));
            _Right.Move(_RightControl.Move(input, _Ball, _Right));
        }

        void MoveBall()
        {
            double previousX = _Ball.X;
            double previousY = _Ball.Y;
            _Ball.Move();
            bool hit = _Ball.VelocityX < 0
                ? _Ball.TryBounce(_Left, G10_PongSide.Left, previousX, previousY)
                : _Ball.TryBounce(_Right, G10_PongSide.Right, previousX, previousY);
            if (hit) CountHit();

            if (_Ball.X + G10_PongConfig.BallSize < 0) ScorePoint(G10_PongSide.Right);
            else if (_Ball.X > G10_PongConfig.Width) ScorePoint(G10_PongSide.Left);
        }

        void CountHit()
        {
            Rally++;
            // Rallies only count against the computer; two players could keep one going forever.
            if (Mode == G10_PongMode.OnePlayer) BestRally = Math.Max(BestRally, Rally);
        }

        void ScorePoint(G10_PongSide side)
        {
            if (side == G10_PongSide.Left) LeftScore++;
            else RightScore++;
            HasScored = true;
            LastScorer = side;
            if (Math.Max(LeftScore, RightScore) >= G10_PongConfig.PointsToWin)
            {
                Winner = side;
                SetPhase(G10_PongPhase.GameOver);
                return;
            }
            // The ball is served towards the side that lost the point.
            _ServeDirection = side == G10_PongSide.Left ? 1 : -1;
            StartServe();
        }

        void SetPhase(G10_PongPhase phase)
        {
            Phase = phase;
            PhaseTicks = 0;
        }
    }
}
