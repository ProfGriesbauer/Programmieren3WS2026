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

    internal abstract class G10_SolarShooterMode
    {
        public abstract string Id { get; }
        public abstract string Name { get; }
        public virtual IG10_ModeHud Hud { get { return G10_EmptyModeHud.Instance; } }

        public virtual G10_SolarShooterSession CreateSession(G10_SolarShooterModeSave savedRun)
        {
            if (savedRun == null || !savedRun.HasResume)
                return new G10_SolarShooterSession(G10_SolarShooterStage.All);
            return new G10_SolarShooterSession(
                G10_SolarShooterStage.All,
                savedRun.ResumeStageIndex,
                savedRun.ResumeScore,
                savedRun.ResumeWeaponLevel);
        }

        public virtual void Update(
            G10_SolarShooterSession session,
            float deltaSeconds,
            G10_SolarShooterInput input)
        {
            session.Update(deltaSeconds, input);
        }

        // Counterpart to CreateSession: each mode writes the data it needs to resume.
        public virtual void WriteSave(
            G10_SolarShooterModeSave save,
            G10_SolarShooterSession session,
            bool canResume)
        {
            if (save == null) throw new ArgumentNullException("save");
            if (session == null) throw new ArgumentNullException("session");
            save.HasResume = canResume;
            save.ResumeStageIndex = session.StageIndex;
            save.ResumeScore = session.Score;
            save.ResumeWeaponLevel = session.Player.WeaponLevel;
            save.BestScore = Math.Max(save.BestScore, session.Score);
            save.BestStage = Math.Max(save.BestStage, session.StageNumber);
            save.BestWeaponLevel = Math.Max(save.BestWeaponLevel, session.Player.WeaponLevel);
        }
    }

    internal sealed class G10_ClassicSolarShooterMode : G10_SolarShooterMode
    {
        public override string Id { get { return "classic"; } }
        public override string Name { get { return "KLASSIK"; } }
    }

    internal sealed class G10_MissionSolarShooterMode : G10_SolarShooterMode
    {
        G10_MissionDirector _Director;

        public override string Id { get { return "missions"; } }
        public override string Name { get { return "MISSIONEN"; } }
        public override IG10_ModeHud Hud
        {
            get { return _Director == null ? G10_EmptyModeHud.Instance : (IG10_ModeHud)_Director; }
        }

        public override G10_SolarShooterSession CreateSession(G10_SolarShooterModeSave savedRun)
        {
            G10_SolarShooterSession session = base.CreateSession(savedRun);
            bool resume = savedRun != null && savedRun.HasResume;
            _Director = new G10_MissionDirector(
                session,
                resume ? savedRun.ResumeMissionsCompleted : 0,
                resume ? savedRun.ResumeMissionsAttempted : 0,
                resume ? savedRun.ResumePreviousMissionId : null);
            return session;
        }

        public override void Update(
            G10_SolarShooterSession session,
            float deltaSeconds,
            G10_SolarShooterInput input)
        {
            base.Update(session, deltaSeconds, input);
            _Director.Update(deltaSeconds);
        }

        public override void WriteSave(
            G10_SolarShooterModeSave save,
            G10_SolarShooterSession session,
            bool canResume)
        {
            base.WriteSave(save, session, canResume);
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
