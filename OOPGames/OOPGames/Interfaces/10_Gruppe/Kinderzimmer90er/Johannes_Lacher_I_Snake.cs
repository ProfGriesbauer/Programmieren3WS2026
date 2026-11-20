using System.Collections.Generic;

namespace OOPGames
{
    public enum G10_SnakePhase
    {
        // Waiting for the first direction key; the snake does not move yet.
        Ready,
        Playing,
        Paused,
        // The snake crashed and flashes for a moment before the game is over.
        Dying,
        GameOver
    }

    // Read-only view of a Snake game for the renderer. The renderer only ever sees this
    // interface, never the model classes, so it cannot change the game.
    public interface IG10_SnakeView
    {
        int Columns { get; }
        int Rows { get; }
        G10_SnakePhase Phase { get; }
        // Ticks spent in the current phase; drives blinking and the crash animation.
        int PhaseTicks { get; }
        int Score { get; }
        int HighScore { get; }
        bool IsNewHighScore { get; }
        int Level { get; }
        // True when the snake filled the whole field.
        bool Won { get; }
        // Head first, tail last.
        IReadOnlyList<G10_Tile> Body { get; }
        G10_Direction Heading { get; }
        G10_Tile Apple { get; }
        bool HasBug { get; }
        G10_Tile Bug { get; }
        // Ticks until the bug runs away; 0 while there is no bug.
        int BugTicksLeft { get; }
    }
}
