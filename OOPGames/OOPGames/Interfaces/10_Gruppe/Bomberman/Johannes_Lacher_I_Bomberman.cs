using System.Collections.Generic;

namespace OOPGames
{
    public enum G10_BomberCellKind
    {
        Floor,
        // Indestructible: the outer wall and the pillar grid.
        Pillar,
        // Destructible; may hide the exit or a power-up.
        Brick
    }

    public enum G10_BomberItemKind
    {
        None,
        Exit,
        BombUp,
        FireUp,
        SpeedUp,
        // Bombs no longer explode on their own; the player detonates them.
        Remote
    }

    public enum G10_BomberEnemyKind
    {
        Puff,
        Spark,
        Drift,
        Hunter,
        // Appears when the time runs out.
        Phantom
    }

    public enum G10_BomberPhaseKind
    {
        StageIntro,
        Playing,
        Paused,
        Dying,
        StageClear,
        GameOver,
        Victory
    }

    // Things that happened during one update and can be heard. The game only reports them;
    // the audio decides what to play.
    public enum G10_BomberSoundCue
    {
        BombPlaced,
        Explosion,
        PowerUp,
        EnemyDefeated,
        PlayerDied,
        TimeUp,
        ExitOpened,
        DoorGuards
    }

    // Which flame sprite a burning cell shows.
    public enum G10_BomberFlameShape
    {
        Center,
        Horizontal,
        Vertical,
        EndLeft,
        EndRight,
        EndUp,
        EndDown
    }

    // Read-only view of the game for the renderer. The renderer never sees the model classes,
    // so it cannot change the game.
    public interface IG10_BomberView
    {
        G10_BomberPhaseKind Phase { get; }
        // Ticks spent in the current phase; drives intro, death and clear animations.
        int PhaseTicks { get; }
        // 1-based.
        int Stage { get; }
        int StageCount { get; }
        int Score { get; }
        int HighScore { get; }
        // Lives left in reserve (not counting the one on the field).
        int Lives { get; }
        int SecondsLeft { get; }
        // Score added for the remaining time when the stage was cleared.
        int TimeBonus { get; }
        // True for a moment after the timer hit zero and the phantoms appeared.
        bool TimeUpWarning { get; }
        // All enemies are gone: the exit lets the player out.
        bool ExitOpen { get; }
        IG10_BomberArenaView Arena { get; }
        IG10_BomberPlayerView Player { get; }
        IReadOnlyList<IG10_BomberEnemyView> Enemies { get; }
        IReadOnlyList<IG10_BomberBombView> Bombs { get; }
        IReadOnlyList<IG10_BomberFlameView> Flames { get; }
        IReadOnlyList<G10_BomberPopup> Popups { get; }
        // Sound cues raised during the last update.
        IReadOnlyList<G10_BomberSoundCue> SoundCues { get; }
    }

    public interface IG10_BomberArenaView
    {
        int Columns { get; }
        int Rows { get; }
        // Increases when a new arena is built, so the renderer can cache floor and pillars.
        int Version { get; }
        G10_BomberCellKind KindAt(G10_BomberCell cell);
        // Only items that are uncovered; hidden ones report None.
        G10_BomberItemKind VisibleItemAt(G10_BomberCell cell);
        // Ticks a destroyed brick still burns; 0 if the cell is not burning.
        int BurnTicksAt(G10_BomberCell cell);
    }

    public interface IG10_BomberActorView
    {
        // Centre of the actor in arena pixels (origin = top-left corner of the arena).
        int PixelX { get; }
        int PixelY { get; }
        G10_Direction Direction { get; }
        bool IsMoving { get; }
        // Ticks the actor has moved so far; drives the walk animation.
        int MoveTicks { get; }
    }

    public interface IG10_BomberPlayerView : IG10_BomberActorView
    {
        int BombCapacity { get; }
        int FireRange { get; }
        int SpeedLevel { get; }
        bool HasRemote { get; }
        bool IsDead { get; }
    }

    public interface IG10_BomberEnemyView : IG10_BomberActorView
    {
        G10_BomberEnemyKind Kind { get; }
        bool IsDying { get; }
        int DyingTicks { get; }
    }

    public interface IG10_BomberBombView
    {
        G10_BomberCell Cell { get; }
        int AgeTicks { get; }
        bool IsRemote { get; }
    }

    public interface IG10_BomberFlameView
    {
        G10_BomberCell Cell { get; }
        G10_BomberFlameShape Shape { get; }
        int AgeTicks { get; }
    }

    // What a moving actor needs to know about the field: arena cells and bombs.
    public interface IG10_BomberObstacles
    {
        bool IsBlocked(G10_BomberCell cell, bool passBricks);
    }

    // How an enemy picks its way at a cell centre (Strategy pattern).
    public interface IG10_BomberEnemyBrain
    {
        // None keeps the enemy standing still for this tick.
        G10_Direction ChooseDirection(G10_BomberEnemy enemy, G10_BomberBrainContext context);
    }

    // Phase of a game (State pattern). Update returns the phase for the next tick.
    public interface IG10_BomberPhase
    {
        G10_BomberPhaseKind Kind { get; }
        IG10_BomberPhase Update(G10_BomberSession session, G10_BomberInput input);
    }
}
