namespace OOPGames
{
    public enum G10_PongPhase
    {
        // Choose one or two players.
        Menu,
        // The ball waits in the middle for a moment before it is served.
        Serving,
        Playing,
        Paused,
        GameOver
    }

    public enum G10_PongMode
    {
        OnePlayer,
        TwoPlayers
    }

    public enum G10_PongSide
    {
        Left,
        Right
    }

    // Read-only view of a Pong match for the renderer. The renderer only ever sees this
    // interface, never the model classes, so it cannot change the game.
    public interface IG10_PongView
    {
        G10_PongPhase Phase { get; }
        // Ticks spent in the current phase; drives blinking.
        int PhaseTicks { get; }
        G10_PongMode Mode { get; }
        // 0 = one player, 1 = two players.
        int MenuSelection { get; }
        int LeftScore { get; }
        int RightScore { get; }
        // False before the first point of a match.
        bool HasScored { get; }
        G10_PongSide LastScorer { get; }
        G10_PongSide Winner { get; }
        // Paddle hits since the last serve.
        int Rally { get; }
        // Longest rally against the computer, also from earlier games.
        int BestRally { get; }
        bool IsNewBestRally { get; }
        // Top edges of the paddles and the top-left corner of the ball, in screen pixels.
        double LeftPaddleY { get; }
        double RightPaddleY { get; }
        double BallX { get; }
        double BallY { get; }
        double BallVelocityX { get; }
        double BallVelocityY { get; }
    }

    // Decides how far a paddle moves in one tick (Strategy pattern): the keys of a player
    // or the computer.
    public interface IG10_PaddleControl
    {
        double Move(G10_PongInput input, G10_PongBall ball, G10_PongPaddle paddle);
    }
}
