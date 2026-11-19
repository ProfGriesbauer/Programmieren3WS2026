using System.Collections.Generic;

namespace OOPGames
{
    internal abstract class G10_SolarShooterMode
    {
        public abstract string Id { get; }
        public abstract string Name { get; }

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

        public virtual int MissionsCompleted { get { return 0; } }
        public virtual G10_MissionDirector MissionDirector { get { return null; } }
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
        public override int MissionsCompleted
        {
            get { return _Director == null ? 0 : _Director.MissionsCompleted; }
        }
        public override G10_MissionDirector MissionDirector { get { return _Director; } }

        public override G10_SolarShooterSession CreateSession(G10_SolarShooterModeSave savedRun)
        {
            G10_SolarShooterSession session = base.CreateSession(savedRun);
            _Director = new G10_MissionDirector(
                session,
                savedRun == null ? 0 : savedRun.ResumeMissionsCompleted,
                savedRun == null ? 0 : savedRun.ResumeMissionsAttempted,
                savedRun == null ? null : savedRun.ResumePreviousMissionId);
            return session;
        }

        public override void Update(
            G10_SolarShooterSession session,
            float deltaSeconds,
            G10_SolarShooterInput input)
        {
            session.Update(deltaSeconds, input);
            _Director.Update(deltaSeconds);
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
