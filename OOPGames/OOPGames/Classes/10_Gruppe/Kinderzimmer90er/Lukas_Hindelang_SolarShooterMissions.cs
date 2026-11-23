using System;
using System.Collections.Generic;

namespace OOPGames
{
    internal static class G10_SolarShooterMissionConfig
    {
        public const float FirstMissionDelay = 4;
        public const float MissionGap = 3;
        public const float MinimumMissionGap = 1.5f;
        public const float StageGapReduction = 0.5f;
        public const float BannerDuration = 2.5f;
        public const float NoShotDuration = 10;
        public const float DestroyFiveDuration = 15;
        public const float SurviveDuration = 20;
        public const float TargetEnemyDuration = 22;
        public const float CollectCellsDuration = 30;
        public const float StayLowDuration = 16;
        public const float SingleShotDuration = 20;
        public const float ShieldDuration = 5;
        public const int EnemiesToDestroy = 5;
        public const int TargetEnemiesToDestroy = 3;
        public const int CellsToCollect = 2;
        public const float StayLowDurationRequired = 8;
        public const int NoShotReward = 400;
        public const int EnemyHuntPenalty = 150;
        public const int TargetEnemyReward = 600;
        public const int TargetEnemyPenalty = 200;
        public const int SingleShotReward = 500;
        public const int SingleShotPenalty = 150;
        public const float ComboWindowSeconds = 3;
        public const int MaximumComboMultiplier = 4;
    }

    internal enum G10_MissionStatus
    {
        Active,
        Succeeded,
        Failed
    }

    internal interface IG10_Mission
    {
        string Id { get; }
        string Title { get; }
        string Description { get; }
        string ProgressText { get; }
        float TimeLimitSeconds { get; }
        float RemainingSeconds { get; }
        G10_MissionStatus Status { get; }
        IG10_MissionReward Reward { get; }
        IG10_MissionPenalty Penalty { get; }
        void Update(float deltaSeconds, G10_SolarShooterSession session);
        void OnShotFired(G10_ShotFiredEventArgs args);
        void OnEnemyDestroyed(G10_EntityDestroyedEventArgs args);
        void OnPlayerHit();
        void OnPowerCellCollected();
        void OnPowerCellEscaped();
    }

    internal abstract class G10_SolarShooterMission : IG10_Mission
    {
        protected G10_SolarShooterMission(
            float timeLimitSeconds,
            IG10_MissionReward reward,
            IG10_MissionPenalty penalty)
        {
            TimeLimitSeconds = timeLimitSeconds;
            RemainingSeconds = timeLimitSeconds;
            Reward = reward;
            Penalty = penalty;
        }

        public abstract string Id { get; }
        public abstract string Title { get; }
        public abstract string Description { get; }
        public abstract string ProgressText { get; }
        public float TimeLimitSeconds { get; private set; }
        public float RemainingSeconds { get; private set; }
        public G10_MissionStatus Status { get; private set; }
        public IG10_MissionReward Reward { get; private set; }
        public IG10_MissionPenalty Penalty { get; private set; }

        public void Update(float deltaSeconds, G10_SolarShooterSession session)
        {
            if (Status != G10_MissionStatus.Active) return;
            OnUpdate(deltaSeconds, session);
            if (Status != G10_MissionStatus.Active) return;
            RemainingSeconds = Math.Max(0, RemainingSeconds - deltaSeconds);
            if (RemainingSeconds <= 0) OnTimeExpired();
        }

        public virtual void OnShotFired(G10_ShotFiredEventArgs args) { }
        public virtual void OnEnemyDestroyed(G10_EntityDestroyedEventArgs args) { }
        public virtual void OnPlayerHit() { }
        public virtual void OnPowerCellCollected() { }
        public virtual void OnPowerCellEscaped() { }
        protected virtual void OnUpdate(float deltaSeconds, G10_SolarShooterSession session) { }
        protected virtual void OnTimeExpired() { Succeed(); }
        protected void Succeed() { Status = G10_MissionStatus.Succeeded; }
        protected void Fail() { Status = G10_MissionStatus.Failed; }
    }

    internal sealed class G10_NoShotMission : G10_SolarShooterMission
    {
        public G10_NoShotMission()
            : base(G10_SolarShooterMissionConfig.NoShotDuration,
                new G10_MissionScoreReward(G10_SolarShooterMissionConfig.NoShotReward),
                new G10_NoMissionPenalty()) { }
        public override string Id { get { return "no-shot"; } }
        public override string Title { get { return "FEUERPAUSE"; } }
        public override string Description
        {
            get { return G10_SolarShooterMissionConfig.NoShotDuration + " SEKUNDEN NICHT SCHIESSEN"; }
        }
        public override string ProgressText { get { return RemainingSeconds.ToString("0.0") + "s"; } }
        public override void OnShotFired(G10_ShotFiredEventArgs args) { Fail(); }
    }

    internal sealed class G10_DestroyFiveMission : G10_SolarShooterMission
    {
        int _Destroyed;
        public G10_DestroyFiveMission()
            : base(G10_SolarShooterMissionConfig.DestroyFiveDuration,
                new G10_MissionWeaponReward(),
                new G10_ScoreMissionPenalty(G10_SolarShooterMissionConfig.EnemyHuntPenalty)) { }
        public override string Id { get { return "destroy-five"; } }
        public override string Title { get { return "GEGNERJAGD"; } }
        public override string Description
        {
            get
            {
                return "ZERSTOERE " + G10_SolarShooterMissionConfig.EnemiesToDestroy +
                    " GEGNER IN " + G10_SolarShooterMissionConfig.DestroyFiveDuration + " SEKUNDEN";
            }
        }
        public override string ProgressText
        {
            get
            {
                return _Destroyed + "/" + G10_SolarShooterMissionConfig.EnemiesToDestroy +
                    "  " + RemainingSeconds.ToString("0.0") + "s";
            }
        }
        public override void OnEnemyDestroyed(G10_EntityDestroyedEventArgs args)
        {
            _Destroyed++;
            if (_Destroyed >= G10_SolarShooterMissionConfig.EnemiesToDestroy) Succeed();
        }
        protected override void OnTimeExpired() { Fail(); }
    }

    internal sealed class G10_SurviveMission : G10_SolarShooterMission
    {
        public G10_SurviveMission()
            : base(G10_SolarShooterMissionConfig.SurviveDuration,
                new G10_MissionExtraLifeReward(), new G10_NoMissionPenalty()) { }
        public override string Id { get { return "survive-unhit"; } }
        public override string Title { get { return "KEIN TREFFER"; } }
        public override string Description { get { return "UEBERLEBE 20 SEKUNDEN OHNE TREFFER"; } }
        public override string ProgressText { get { return RemainingSeconds.ToString("0.0") + "s"; } }
        public override void OnPlayerHit() { Fail(); }
    }

    internal sealed class G10_TargetEnemyMission : G10_SolarShooterMission
    {
        static readonly G10_ShooterEnemyKind[] Targets =
        {
            G10_ShooterEnemyKind.Driftling,
            G10_ShooterEnemyKind.Needlewing,
            G10_ShooterEnemyKind.Bulwark
        };

        readonly G10_ShooterEnemyKind _Target;
        int _TargetDestroyed;

        public G10_TargetEnemyMission(Random random)
            : base(G10_SolarShooterMissionConfig.TargetEnemyDuration,
                new G10_MissionScoreReward(G10_SolarShooterMissionConfig.TargetEnemyReward),
                new G10_ScoreMissionPenalty(G10_SolarShooterMissionConfig.TargetEnemyPenalty))
        {
            _Target = Targets[random.Next(Targets.Length)];
        }

        string TargetName { get { return _Target.ToString().ToUpperInvariant(); } }

        public override string Id { get { return "target-enemy"; } }
        public override string Title { get { return "PRIORIZIERE " + TargetName; } }
        public override string Description
        {
            get
            {
                return "ZERSTOERE " + G10_SolarShooterMissionConfig.TargetEnemiesToDestroy +
                    " " + TargetName;
            }
        }
        public override string ProgressText
        {
            get
            {
                return _TargetDestroyed + "/" + G10_SolarShooterMissionConfig.TargetEnemiesToDestroy +
                    "  " + RemainingSeconds.ToString("0.0") + "s";
            }
        }
        public override void OnEnemyDestroyed(G10_EntityDestroyedEventArgs args)
        {
            if (args.EnemyKind == _Target)
            {
                _TargetDestroyed++;
                if (_TargetDestroyed >= G10_SolarShooterMissionConfig.TargetEnemiesToDestroy) Succeed();
            }
            else
            {
                _TargetDestroyed = Math.Max(0, _TargetDestroyed - 1);
            }
        }
        protected override void OnTimeExpired() { Fail(); }
    }

    internal sealed class G10_CollectPowerCellsMission : G10_SolarShooterMission
    {
        int _Collected;
        public G10_CollectPowerCellsMission()
            : base(G10_SolarShooterMissionConfig.CollectCellsDuration,
                new G10_MissionWeaponReward(), new G10_NoMissionPenalty()) { }
        public override string Id { get { return "collect-cells"; } }
        public override string Title { get { return "ENERGIEBERGUNG"; } }
        public override string Description
        {
            get { return "SAMMLE " + G10_SolarShooterMissionConfig.CellsToCollect + " POWER-CELLS"; }
        }
        public override string ProgressText
        {
            get
            {
                return _Collected + "/" + G10_SolarShooterMissionConfig.CellsToCollect +
                    "  " + RemainingSeconds.ToString("0.0") + "s";
            }
        }
        public override void OnPowerCellCollected()
        {
            _Collected++;
            if (_Collected >= G10_SolarShooterMissionConfig.CellsToCollect) Succeed();
        }
        public override void OnPowerCellEscaped() { Fail(); }
        protected override void OnTimeExpired() { Fail(); }
    }

    internal sealed class G10_StayLowMission : G10_SolarShooterMission
    {
        float _SecondsInLowerThird;
        public G10_StayLowMission()
            : base(G10_SolarShooterMissionConfig.StayLowDuration,
                new G10_MissionShieldReward(), new G10_NoMissionPenalty()) { }
        public override string Id { get { return "stay-low"; } }
        public override string Title { get { return "TIEFFLUG"; } }
        public override string Description
        {
            get
            {
                return "BLEIBE " + G10_SolarShooterMissionConfig.StayLowDurationRequired +
                    " SEKUNDEN IM UNTEREN DRITTEL";
            }
        }
        public override string ProgressText
        {
            get
            {
                return _SecondsInLowerThird.ToString("0.0") + "/" +
                    G10_SolarShooterMissionConfig.StayLowDurationRequired + "s";
            }
        }
        protected override void OnUpdate(float deltaSeconds, G10_SolarShooterSession session)
        {
            if (session.Player.Y < G10_ShooterConfig.LowerThirdY)
            {
                _SecondsInLowerThird = 0;
                return;
            }
            _SecondsInLowerThird += deltaSeconds;
            if (_SecondsInLowerThird >= G10_SolarShooterMissionConfig.StayLowDurationRequired) Succeed();
        }
        protected override void OnTimeExpired()
        {
            if (_SecondsInLowerThird < G10_SolarShooterMissionConfig.StayLowDurationRequired) Fail();
        }
    }

    internal sealed class G10_SingleShotMission : G10_SolarShooterMission
    {
        public G10_SingleShotMission()
            : base(G10_SolarShooterMissionConfig.SingleShotDuration,
                new G10_MissionScoreReward(G10_SolarShooterMissionConfig.SingleShotReward),
                new G10_ScoreMissionPenalty(G10_SolarShooterMissionConfig.SingleShotPenalty)) { }
        public override string Id { get { return "single-shot"; } }
        public override string Title { get { return "PRÄZISIONSFEUER"; } }
        public override string Description { get { return "ZERSTOERE KEINEN GEGNER MIT DREIFACHSCHUSS"; } }
        public override string ProgressText { get { return RemainingSeconds.ToString("0.0") + "s"; } }
        public override void OnEnemyDestroyed(G10_EntityDestroyedEventArgs args)
        {
            if (args.WeaponLevel >= 3) Fail();
        }
    }
}
