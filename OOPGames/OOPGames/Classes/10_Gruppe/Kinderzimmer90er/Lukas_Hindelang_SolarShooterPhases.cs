namespace OOPGames
{
    // Read-only name of the current phase for the renderer and the game.
    internal enum G10_ShooterPhase
    {
        Playing,
        BossWarning,
        StageTransition,
        GameOver,
        Victory
    }

    // State pattern (like the Tetris phases): each phase of a run decides what happens in
    // its tick and returns the phase for the next tick. Only the session switches phases.
    internal interface IG10_ShooterPhase
    {
        G10_ShooterPhase Kind { get; }

        // Returns itself to stay in this phase, or the phase to switch to.
        IG10_ShooterPhase Update(G10_SolarShooterSession session, float deltaSeconds, G10_SolarShooterInput input);
    }

    internal static class G10_ShooterPhases
    {
        public static readonly IG10_ShooterPhase Playing = new G10_ShooterPlayingPhase();
        public static readonly IG10_ShooterPhase BossWarning = new G10_ShooterBossWarningPhase();
        public static readonly IG10_ShooterPhase StageTransition = new G10_ShooterStageTransitionPhase();
        public static readonly IG10_ShooterPhase GameOver = new G10_ShooterFinishedPhase(G10_ShooterPhase.GameOver);
        public static readonly IG10_ShooterPhase Victory = new G10_ShooterFinishedPhase(G10_ShooterPhase.Victory);
    }

    internal sealed class G10_ShooterPlayingPhase : IG10_ShooterPhase
    {
        public G10_ShooterPhase Kind { get { return G10_ShooterPhase.Playing; } }

        public IG10_ShooterPhase Update(G10_SolarShooterSession session, float deltaSeconds, G10_SolarShooterInput input)
        {
            session.UpdateShip(deltaSeconds, input);
            session.UpdateCombat(deltaSeconds, input);

            if (session.Player.Lives <= 0) return G10_ShooterPhases.GameOver;
            if (session.IsBossDefeated) return G10_ShooterPhases.StageTransition;
            if (session.IsBossDue)
            {
                session.BeginBossEncounter();
                return G10_ShooterPhases.BossWarning;
            }
            return this;
        }
    }

    // The ship can still move while the boss warning is shown, but nothing fights.
    internal sealed class G10_ShooterBossWarningPhase : IG10_ShooterPhase
    {
        const float Seconds = 1.5f;

        public G10_ShooterPhase Kind { get { return G10_ShooterPhase.BossWarning; } }

        public IG10_ShooterPhase Update(G10_SolarShooterSession session, float deltaSeconds, G10_SolarShooterInput input)
        {
            session.UpdateShip(deltaSeconds, input);
            return session.PhaseSeconds >= Seconds ? G10_ShooterPhases.Playing : this;
        }
    }

    internal sealed class G10_ShooterStageTransitionPhase : IG10_ShooterPhase
    {
        const float Seconds = 1.8f;

        public G10_ShooterPhase Kind { get { return G10_ShooterPhase.StageTransition; } }

        public IG10_ShooterPhase Update(G10_SolarShooterSession session, float deltaSeconds, G10_SolarShooterInput input)
        {
            session.UpdateShip(deltaSeconds, input);
            if (session.PhaseSeconds < Seconds) return this;
            if (session.IsLastStage) return G10_ShooterPhases.Victory;
            session.AdvanceStage();
            return G10_ShooterPhases.Playing;
        }
    }

    // Game over and victory end the run: nothing moves any more.
    internal sealed class G10_ShooterFinishedPhase : IG10_ShooterPhase
    {
        readonly G10_ShooterPhase _Kind;

        public G10_ShooterFinishedPhase(G10_ShooterPhase kind)
        {
            _Kind = kind;
        }

        public G10_ShooterPhase Kind { get { return _Kind; } }

        public IG10_ShooterPhase Update(G10_SolarShooterSession session, float deltaSeconds, G10_SolarShooterInput input)
        {
            return this;
        }
    }
}
