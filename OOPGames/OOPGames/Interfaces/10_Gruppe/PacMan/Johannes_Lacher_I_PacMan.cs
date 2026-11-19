using System.Collections.Generic;

namespace OOPGames
{
    public enum G10_GhostKind
    {
        Blinky,
        Pinky,
        Inky,
        Clyde
    }

    public enum G10_GhostModeKind
    {
        InHouse,
        LeavingHouse,
        Scatter,
        Chase,
        Frightened,
        Eaten
    }

    public enum G10_CollectibleKind
    {
        None,
        Dot,
        PowerPellet
    }

    public enum G10_FruitKind
    {
        Cherry,
        Strawberry,
        Orange,
        Apple,
        Melon,
        Bell,
        Key
    }

    public enum G10_PacManPhaseKind
    {
        Ready,
        Playing,
        Paused,
        Dying,
        LevelCleared,
        GameOver
    }

    // Things that happened during one tick and can be heard. The game only reports them;
    // the audio decides what to play.
    public enum G10_SoundCue
    {
        GameStart,
        Chomp,
        GhostEaten,
        FruitEaten,
        ExtraLife,
        Death
    }

    public enum G10_Sound
    {
        StartJingle,
        ChompA,
        ChompB,
        GhostEaten,
        FruitEaten,
        ExtraLife,
        Death,
        // Loops while playing; only one of them runs at a time.
        Siren,
        Frightened,
        Retreat
    }

    // Plays sound clips. Several different sounds may play at the same time;
    // playing a sound again restarts it.
    public interface IG10_SoundOutput
    {
        void Play(G10_Sound sound);
        void Stop(G10_Sound sound);
        void StopAll();
    }

    // Read-only view of the game for the renderer. The renderer must not change the game,
    // so it only ever sees these interfaces, never the model classes.
    public interface IG10_PacManView
    {
        G10_PacManPhaseKind Phase { get; }
        // Ticks spent in the current phase; drives the death animation and the maze flash.
        int PhaseTicks { get; }
        int Score { get; }
        int HighScore { get; }
        int Level { get; }
        // Lives left in reserve (not counting the Pac-Man on the field).
        int Lives { get; }
        IG10_MazeView Maze { get; }
        IG10_ActorView PacMan { get; }
        IReadOnlyList<IG10_GhostView> Ghosts { get; }
        // Null while no bonus fruit is on the field.
        IG10_FruitView Fruit { get; }
        // Fruits of the current and previous levels, newest last (for the HUD).
        IReadOnlyList<G10_FruitKind> FruitHistory { get; }
        // True during the last part of the frightened time: frightened ghosts flash white.
        bool FrightenedFlashing { get; }
        // Score popups (eaten ghost, eaten fruit) that are currently visible.
        IReadOnlyList<G10_PointsPopup> Popups { get; }
        // Sound cues raised since the previous update.
        IReadOnlyList<G10_SoundCue> SoundCues { get; }
    }

    public interface IG10_MazeView
    {
        int Columns { get; }
        int Rows { get; }
        // Increases whenever the walls change (new level / layout), so the renderer can cache them.
        int Version { get; }
        G10_TileKind KindAt(G10_Tile tile);
        G10_CollectibleKind CollectibleAt(G10_Tile tile);
    }

    public interface IG10_ActorView
    {
        // Centre of the actor in maze pixels (origin = top-left corner of the maze).
        int PixelX { get; }
        int PixelY { get; }
        G10_Direction Direction { get; }
        bool IsMoving { get; }
        // Ticks the actor has moved so far; drives the walk animation.
        int MoveTicks { get; }
        bool Visible { get; }
    }

    public interface IG10_GhostView : IG10_ActorView
    {
        G10_GhostKind Kind { get; }
        G10_GhostModeKind Mode { get; }
    }

    public interface IG10_FruitView
    {
        int PixelX { get; }
        int PixelY { get; }
        G10_FruitKind Kind { get; }
    }

    // Chase target of a ghost (Strategy pattern).
    public interface IG10_TargetStrategy
    {
        G10_Tile TargetTile(G10_Ghost ghost, G10_GhostContext context);
    }

    // Phase of a game (State pattern). Update returns the phase for the next tick.
    public interface IG10_PacManPhase
    {
        G10_PacManPhaseKind Kind { get; }
        IG10_PacManPhase Update(G10_PacManSession session, G10_PacManInput input);
    }
}
