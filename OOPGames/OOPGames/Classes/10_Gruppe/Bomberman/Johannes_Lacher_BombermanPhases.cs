namespace OOPGames
{
    // The phases keep no state of their own (the session counts the ticks), so one instance
    // of each is enough.
    public static class G10_BomberPhases
    {
        public static readonly IG10_BomberPhase Intro = new G10_BomberIntroPhase();
        public static readonly IG10_BomberPhase Playing = new G10_BomberPlayingPhase();
        public static readonly IG10_BomberPhase Paused = new G10_BomberPausedPhase();
        public static readonly IG10_BomberPhase Dying = new G10_BomberDyingPhase();
        public static readonly IG10_BomberPhase StageClear = new G10_BomberStageClearPhase();
        public static readonly IG10_BomberPhase GameOver = new G10_BomberEndPhase(G10_BomberPhaseKind.GameOver);
        public static readonly IG10_BomberPhase Victory = new G10_BomberEndPhase(G10_BomberPhaseKind.Victory);
    }

    // "STAGE n" on a dark screen before the stage starts.
    public sealed class G10_BomberIntroPhase : IG10_BomberPhase
    {
        public static readonly int Duration = G10_BomberConstants.Seconds(2.2);

        public G10_BomberPhaseKind Kind { get { return G10_BomberPhaseKind.StageIntro; } }

        public IG10_BomberPhase Update(G10_BomberSession session, G10_BomberInput input)
        {
            return session.PhaseTicks >= Duration ? G10_BomberPhases.Playing : this;
        }
    }

    public sealed class G10_BomberPlayingPhase : IG10_BomberPhase
    {
        public G10_BomberPhaseKind Kind { get { return G10_BomberPhaseKind.Playing; } }

        public IG10_BomberPhase Update(G10_BomberSession session, G10_BomberInput input)
        {
            if (input.Confirm) return G10_BomberPhases.Paused;
            switch (session.TickRound(input))
            {
                case G10_BomberRoundResult.PlayerDied:
                    return G10_BomberPhases.Dying;
                case G10_BomberRoundResult.StageCleared:
                    session.AwardTimeBonus();
                    return G10_BomberPhases.StageClear;
                default:
                    return this;
            }
        }
    }

    public sealed class G10_BomberPausedPhase : IG10_BomberPhase
    {
        public G10_BomberPhaseKind Kind { get { return G10_BomberPhaseKind.Paused; } }

        public IG10_BomberPhase Update(G10_BomberSession session, G10_BomberInput input)
        {
            return input.Confirm ? G10_BomberPhases.Playing : this;
        }
    }

    // The world keeps running while the player's death animation plays.
    public sealed class G10_BomberDyingPhase : IG10_BomberPhase
    {
        public static readonly int Duration = G10_BomberConstants.Seconds(2);

        public G10_BomberPhaseKind Kind { get { return G10_BomberPhaseKind.Dying; } }

        public IG10_BomberPhase Update(G10_BomberSession session, G10_BomberInput input)
        {
            session.TickRound(G10_BomberInput.Neutral);
            if (session.PhaseTicks < Duration) return this;
            return session.LoseLife() ? G10_BomberPhases.Intro : G10_BomberPhases.GameOver;
        }
    }

    public sealed class G10_BomberStageClearPhase : IG10_BomberPhase
    {
        public static readonly int Duration = G10_BomberConstants.Seconds(2.5);

        public G10_BomberPhaseKind Kind { get { return G10_BomberPhaseKind.StageClear; } }

        public IG10_BomberPhase Update(G10_BomberSession session, G10_BomberInput input)
        {
            if (session.PhaseTicks < Duration) return this;
            return session.AdvanceStage() ? G10_BomberPhases.Intro : G10_BomberPhases.Victory;
        }
    }

    // GAME OVER or the end of the last stage. The game adapter starts a new session on Confirm.
    public sealed class G10_BomberEndPhase : IG10_BomberPhase
    {
        readonly G10_BomberPhaseKind _Kind;

        public G10_BomberEndPhase(G10_BomberPhaseKind kind)
        {
            _Kind = kind;
        }

        public G10_BomberPhaseKind Kind { get { return _Kind; } }

        public IG10_BomberPhase Update(G10_BomberSession session, G10_BomberInput input)
        {
            return this;
        }
    }
}
