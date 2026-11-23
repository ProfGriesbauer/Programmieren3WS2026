using System;
using System.Collections.Generic;

namespace OOPGames
{
    // State pattern: each phase of a Tetris round decides what happens in its tick.
    internal enum G10_TetrisState
    {
        Ready,
        Playing,
        Clearing,
        Paused,
        GameOver
    }

    internal interface IG10_TetrisPhase
    {
        G10_TetrisState State { get; }
        IG10_TetrisPhase Update(G10_TetrisSession session, float deltaSeconds, G10_TetrisInput input);
    }

    internal static class G10_TetrisPhases
    {
        public static readonly IG10_TetrisPhase Ready = new G10_TetrisReadyPhase();
        public static readonly IG10_TetrisPhase Playing = new G10_TetrisPlayingPhase();
        public static readonly IG10_TetrisPhase Clearing = new G10_TetrisClearingPhase();
        public static readonly IG10_TetrisPhase Paused = new G10_TetrisPausedPhase();
        public static readonly IG10_TetrisPhase GameOver = new G10_TetrisGameOverPhase();
    }

    internal sealed class G10_TetrisReadyPhase : IG10_TetrisPhase
    {
        public G10_TetrisState State { get { return G10_TetrisState.Ready; } }

        public IG10_TetrisPhase Update(G10_TetrisSession session, float deltaSeconds, G10_TetrisInput input)
        {
            if (session.StateSeconds >= G10_TetrisConfig.ReadySeconds)
            {
                session.SetState(G10_TetrisPhases.Playing);
                session.SpawnNext();
            }
            return session.Phase;
        }
    }

    internal sealed class G10_TetrisPlayingPhase : IG10_TetrisPhase
    {
        public G10_TetrisState State { get { return G10_TetrisState.Playing; } }

        public IG10_TetrisPhase Update(G10_TetrisSession session, float deltaSeconds, G10_TetrisInput input)
        {
            if (input.Start) session.Pause();
            else session.UpdatePlaying(deltaSeconds, input);
            return session.Phase;
        }
    }

    internal sealed class G10_TetrisClearingPhase : IG10_TetrisPhase
    {
        public G10_TetrisState State { get { return G10_TetrisState.Clearing; } }

        public IG10_TetrisPhase Update(G10_TetrisSession session, float deltaSeconds, G10_TetrisInput input)
        {
            if (input.Start) session.Pause();
            else if (session.StateSeconds >= G10_TetrisConfig.LineClearSeconds)
                session.FinishClear();
            return session.Phase;
        }
    }

    internal sealed class G10_TetrisPausedPhase : IG10_TetrisPhase
    {
        public G10_TetrisState State { get { return G10_TetrisState.Paused; } }

        public IG10_TetrisPhase Update(G10_TetrisSession session, float deltaSeconds, G10_TetrisInput input)
        {
            if (input.Start) session.Resume();
            return session.Phase;
        }
    }

    internal sealed class G10_TetrisGameOverPhase : IG10_TetrisPhase
    {
        public G10_TetrisState State { get { return G10_TetrisState.GameOver; } }

        public IG10_TetrisPhase Update(G10_TetrisSession session, float deltaSeconds, G10_TetrisInput input)
        {
            if (input.Start && session.StateSeconds >= G10_TetrisConfig.GameOverInputDelay)
                session.Start();
            return session.Phase;
        }
    }
}
