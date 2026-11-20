using System.Collections.Generic;

namespace OOPGames
{
    public enum G10_CoinPhase
    {
        // Short pause at the start of a level.
        Ready,
        Playing,
        Paused,
        // A dust bunny caught the player or the time ran out; costs a life.
        Hit,
        LevelCleared,
        GameOver
    }

    public enum G10_PickupKind
    {
        Coin,
        // Worth much more than a coin, but only stays for a few seconds.
        Diamond
    }

    // Read-only view of a "Muenzen sammeln" game for the renderer. The renderer only ever sees
    // these interfaces, never the model classes, so it cannot change the game.
    public interface IG10_CoinView
    {
        int Columns { get; }
        int Rows { get; }
        bool IsWall(G10_Tile tile);
        G10_CoinPhase Phase { get; }
        // Ticks spent in the current phase; drives blinking and banners.
        int PhaseTicks { get; }
        int Score { get; }
        int HighScore { get; }
        bool IsNewHighScore { get; }
        int Level { get; }
        // Lives left, including the one being played.
        int Lives { get; }
        int CoinsCollected { get; }
        int CoinsNeeded { get; }
        int TicksLeft { get; }
        // True when the last life was lost because the time ran out.
        bool TimeRanOut { get; }
        // Points for the time left, added when the level was cleared.
        int TimeBonus { get; }
        IG10_CoinPlayerView Player { get; }
        IReadOnlyList<G10_Tile> DustBunnies { get; }
        IReadOnlyList<IG10_PickupView> Pickups { get; }
    }

    public interface IG10_CoinPlayerView
    {
        G10_Tile Tile { get; }
        G10_Direction Facing { get; }
        // Number of steps taken so far; drives the walk animation.
        int Steps { get; }
        // Blinks after losing a life and cannot be caught meanwhile.
        bool IsInvulnerable { get; }
    }

    public interface IG10_PickupView
    {
        G10_Tile Tile { get; }
        G10_PickupKind Kind { get; }
        // Ticks until a diamond vanishes; coins stay until collected.
        int TicksLeft { get; }
    }
}
