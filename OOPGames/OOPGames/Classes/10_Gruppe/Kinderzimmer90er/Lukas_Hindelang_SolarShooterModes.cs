using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Mode-specific HUD texts. The renderer only draws these generic slots and does not
    // know which mode delivers them; a null text means "nothing to show".
    internal interface IG10_ModeHud
    {
        string BannerTitle { get; }
        string BannerDetails { get; }
        string TaskTitle { get; }
        string TaskDescription { get; }
        string ComboText { get; }
        string SummaryText { get; }
    }

    // Null Object for modes without an extra HUD.
    internal sealed class G10_EmptyModeHud : IG10_ModeHud
    {
        public static readonly G10_EmptyModeHud Instance = new G10_EmptyModeHud();

        G10_EmptyModeHud() { }

        public string BannerTitle { get { return null; } }
        public string BannerDetails { get { return null; } }
        public string TaskTitle { get { return null; } }
        public string TaskDescription { get { return null; } }
        public string ComboText { get { return null; } }
        public string SummaryText { get { return null; } }
    }

    // A mode is only the choice in the menu: name, id and a factory for runs. It keeps no
    // state of a game in progress, so the same mode object can start any number of runs.
    internal abstract class G10_SolarShooterMode
    {
        public abstract string Id { get; }
        public abstract string Name { get; }

        // Starts a fresh run, or resumes the one stored in savedRun.
        public abstract G10_SolarShooterRun StartRun(G10_SolarShooterModeSave savedRun);

        protected static bool CanResume(G10_SolarShooterModeSave savedRun)
        {
            return savedRun != null && savedRun.HasResume;
        }

        protected static G10_SolarShooterSession CreateSession(G10_SolarShooterModeSave savedRun)
        {
            if (!CanResume(savedRun))
                return new G10_SolarShooterSession(G10_SolarShooterStage.All);
            return new G10_SolarShooterSession(
                G10_SolarShooterStage.All,
                savedRun.ResumeStageIndex,
                savedRun.ResumeScore,
                savedRun.ResumeWeaponLevel);
        }
    }

    // One game in progress: the session plus everything the mode adds to it. The run owns
    // its session, so update, saving and HUD always refer to the same game.
    internal class G10_SolarShooterRun
    {
        public G10_SolarShooterRun(G10_SolarShooterMode mode, G10_SolarShooterSession session)
        {
            if (mode == null) throw new ArgumentNullException("mode");
            if (session == null) throw new ArgumentNullException("session");
            Mode = mode;
            Session = session;
        }

        public G10_SolarShooterMode Mode { get; }
        public G10_SolarShooterSession Session { get; }
        public bool IsFinished { get { return Session.IsGameOver || Session.IsVictory; } }
        public virtual IG10_ModeHud Hud { get { return G10_EmptyModeHud.Instance; } }

        public virtual void Update(float deltaSeconds, G10_SolarShooterInput input)
        {
            Session.Update(deltaSeconds, input);
        }

        // Counterpart to StartRun: each run writes the data it needs to resume.
        public virtual void WriteSave(G10_SolarShooterModeSave save, bool canResume)
        {
            if (save == null) throw new ArgumentNullException("save");
            save.HasResume = canResume;
            save.ResumeStageIndex = Session.StageIndex;
            save.ResumeScore = Session.Score;
            save.ResumeWeaponLevel = Session.Player.WeaponLevel;
            save.BestScore = Math.Max(save.BestScore, Session.Score);
            save.BestStage = Math.Max(save.BestStage, Session.StageNumber);
            save.BestWeaponLevel = Math.Max(save.BestWeaponLevel, Session.Player.WeaponLevel);
        }
    }

    internal sealed class G10_ClassicSolarShooterMode : G10_SolarShooterMode
    {
        public override string Id { get { return "classic"; } }
        public override string Name { get { return "KLASSIK"; } }

        public override G10_SolarShooterRun StartRun(G10_SolarShooterModeSave savedRun)
        {
            return new G10_SolarShooterRun(this, CreateSession(savedRun));
        }
    }

    internal sealed class G10_MissionSolarShooterMode : G10_SolarShooterMode
    {
        public override string Id { get { return "missions"; } }
        public override string Name { get { return "MISSIONEN"; } }

        public override G10_SolarShooterRun StartRun(G10_SolarShooterModeSave savedRun)
        {
            G10_SolarShooterSession session = CreateSession(savedRun);
            bool resume = CanResume(savedRun);
            G10_MissionDirector director = new G10_MissionDirector(
                session,
                resume ? savedRun.ResumeMissionsCompleted : 0,
                resume ? savedRun.ResumeMissionsAttempted : 0,
                resume ? savedRun.ResumePreviousMissionId : null);
            return new G10_MissionSolarShooterRun(this, session, director);
        }
    }

    internal sealed class G10_MissionSolarShooterRun : G10_SolarShooterRun
    {
        readonly G10_MissionDirector _Director;

        public G10_MissionSolarShooterRun(
            G10_SolarShooterMode mode,
            G10_SolarShooterSession session,
            G10_MissionDirector director)
            : base(mode, session)
        {
            if (director == null) throw new ArgumentNullException("director");
            _Director = director;
        }

        public override IG10_ModeHud Hud { get { return _Director; } }

        public override void Update(float deltaSeconds, G10_SolarShooterInput input)
        {
            base.Update(deltaSeconds, input);
            _Director.Update(deltaSeconds);
        }

        public override void WriteSave(G10_SolarShooterModeSave save, bool canResume)
        {
            base.WriteSave(save, canResume);
            save.ResumeMissionsCompleted = _Director.MissionsCompleted;
            save.ResumeMissionsAttempted = _Director.MissionsAttempted;
            save.ResumePreviousMissionId = _Director.PreviousMissionId;
            save.BestMissionsCompleted = Math.Max(save.BestMissionsCompleted, _Director.MissionsCompleted);
        }
    }

    internal static class G10_SolarShooterModeCatalog
    {
        public static IReadOnlyList<G10_SolarShooterMode> CreateModes()
        {
            return new G10_SolarShooterMode[]
            {
                new G10_ClassicSolarShooterMode(),
                new G10_MissionSolarShooterMode()
            };
        }
    }
}
