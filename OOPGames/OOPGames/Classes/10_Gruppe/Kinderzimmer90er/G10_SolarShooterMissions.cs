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

    internal interface IG10_MissionReward
    {
        string Description { get; }
        void Apply(G10_SolarShooterSession session);
    }

    internal interface IG10_MissionPenalty
    {
        string Description { get; }
        void Apply(G10_SolarShooterSession session);
    }

    internal sealed class G10_MissionScoreReward : IG10_MissionReward
    {
        readonly int _Points;
        public G10_MissionScoreReward(int points) { _Points = points; }
        public string Description { get { return "+" + _Points + " PUNKTE"; } }
        public void Apply(G10_SolarShooterSession session) { session.AddScore(_Points); }
    }

    internal sealed class G10_MissionExtraLifeReward : IG10_MissionReward
    {
        public string Description { get { return "EXTRA-LEBEN"; } }
        public void Apply(G10_SolarShooterSession session) { session.GrantExtraLife(); }
    }

    internal sealed class G10_MissionWeaponReward : IG10_MissionReward
    {
        public string Description { get { return "WAFFENSTUFE"; } }
        public void Apply(G10_SolarShooterSession session) { session.Player.UpgradeWeapon(); }
    }

    internal sealed class G10_MissionShieldReward : IG10_MissionReward
    {
        public string Description { get { return "SCHILD"; } }
        public void Apply(G10_SolarShooterSession session) { session.ActivateShield(5); }
    }

    internal sealed class G10_NoMissionPenalty : IG10_MissionPenalty
    {
        public string Description { get { return "KEINE STRAFE"; } }
        public void Apply(G10_SolarShooterSession session) { }
    }

    internal sealed class G10_ScoreMissionPenalty : IG10_MissionPenalty
    {
        readonly int _Points;
        public G10_ScoreMissionPenalty(int points) { _Points = points; }
        public string Description { get { return "-" + _Points + " PUNKTE"; } }
        public void Apply(G10_SolarShooterSession session) { session.ApplyScorePenalty(_Points); }
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
        readonly string _TargetType;
        int _TargetDestroyed;

        public G10_TargetEnemyMission(Random random)
            : base(G10_SolarShooterMissionConfig.TargetEnemyDuration,
                new G10_MissionScoreReward(G10_SolarShooterMissionConfig.TargetEnemyReward),
                new G10_ScoreMissionPenalty(G10_SolarShooterMissionConfig.TargetEnemyPenalty))
        {
            string[] enemyTypes = { "Driftling", "Needlewing", "Bulwark" };
            _TargetType = enemyTypes[random.Next(enemyTypes.Length)];
        }

        public override string Id { get { return "target-enemy"; } }
        public override string Title { get { return "PRIORIZIERE " + _TargetType.ToUpperInvariant(); } }
        public override string Description {         get
        {
            return "ZERSTOERE " + G10_SolarShooterMissionConfig.TargetEnemiesToDestroy +
                " " + _TargetType.ToUpperInvariant();
        } }
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
            if (args.EnemyType == _TargetType)
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

    internal sealed class G10_MissionDirector
    {
        readonly Func<Random, IG10_Mission>[] _Factories =
        {
            random => new G10_NoShotMission(),
            random => new G10_DestroyFiveMission(),
            random => new G10_SurviveMission(),
            random => new G10_TargetEnemyMission(random),
            random => new G10_CollectPowerCellsMission(),
            random => new G10_StayLowMission(),
            random => new G10_SingleShotMission()
        };
        readonly Random _Random = new Random();
        readonly G10_SolarShooterSession _Session;
        string _PreviousMissionId;
        float _NextMissionDelay = G10_SolarShooterMissionConfig.FirstMissionDelay;
        float _BannerSeconds;
        float _ComboSeconds;
        bool _OutcomeApplied;

        public G10_MissionDirector(
            G10_SolarShooterSession session,
            int missionsCompleted,
            int missionsAttempted,
            string previousMissionId)
        {
            _Session = session;
            MissionsCompleted = Math.Max(0, missionsCompleted);
            MissionsAttempted = Math.Max(MissionsCompleted, missionsAttempted);
            _PreviousMissionId = previousMissionId;
            _Session.ShotFired += OnShotFired;
            _Session.EnemyDestroyed += OnEnemyDestroyed;
            _Session.PlayerHit += OnPlayerHit;
            _Session.PowerCellCollected += OnPowerCellCollected;
            _Session.PowerCellEscaped += OnPowerCellEscaped;
        }

        public IG10_Mission CurrentMission { get; private set; }
        public string BannerText { get; private set; }
        public string BannerDetails { get; private set; }
        public int MissionsCompleted { get; private set; }
        public int MissionsAttempted { get; private set; }
        public int ComboCount { get; private set; }
        public string PreviousMissionId { get { return _PreviousMissionId; } }

        public void Update(float deltaSeconds)
        {
            if (_Session.IsGameOver || _Session.IsVictory) return;
            if (_ComboSeconds > 0)
            {
                _ComboSeconds = Math.Max(0, _ComboSeconds - deltaSeconds);
                if (_ComboSeconds == 0) ComboCount = 0;
            }
            if (CurrentMission != null && CurrentMission.Status == G10_MissionStatus.Active)
            {
                CurrentMission.Update(deltaSeconds, _Session);
                ResolveMission();
            }

            if (_BannerSeconds > 0)
            {
                _BannerSeconds = Math.Max(0, _BannerSeconds - deltaSeconds);
                if (_BannerSeconds == 0)
                {
                    BannerText = null;
                    BannerDetails = null;
                    if (CurrentMission != null &&
                        CurrentMission.Status != G10_MissionStatus.Active)
                    {
                        CurrentMission = null;
                        _NextMissionDelay = Math.Max(
                            G10_SolarShooterMissionConfig.MinimumMissionGap,
                            G10_SolarShooterMissionConfig.MissionGap -
                            _Session.StageIndex * G10_SolarShooterMissionConfig.StageGapReduction);
                    }
                }
                return;
            }

            if (CurrentMission != null) return;
            _NextMissionDelay -= deltaSeconds;
            if (_NextMissionDelay <= 0) StartNextMission();
        }

        void StartNextMission()
        {
            List<IG10_Mission> choices = new List<IG10_Mission>();
            foreach (Func<Random, IG10_Mission> factory in _Factories)
            {
                IG10_Mission candidate = factory(_Random);
                if (candidate.Id != _PreviousMissionId) choices.Add(candidate);
            }
            CurrentMission = choices[_Random.Next(choices.Count)];
            _PreviousMissionId = CurrentMission.Id;
            MissionsAttempted++;
            BannerText = "MISSION: " + CurrentMission.Title + " / " +
                CurrentMission.TimeLimitSeconds.ToString("0") + "s";
            BannerDetails = CurrentMission.Description + " / " +
                CurrentMission.Reward.Description;
            _BannerSeconds = G10_SolarShooterMissionConfig.BannerDuration;
            _OutcomeApplied = false;
        }

        void OnShotFired(object sender, G10_ShotFiredEventArgs args)
        {
            if (CurrentMission != null) CurrentMission.OnShotFired(args);
            ResolveMission();
        }

        void OnEnemyDestroyed(object sender, G10_EntityDestroyedEventArgs args)
        {
            if (_ComboSeconds <= 0) ComboCount = 0;
            ComboCount = Math.Min(
                ComboCount + 1, G10_SolarShooterMissionConfig.MaximumComboMultiplier);
            _ComboSeconds = G10_SolarShooterMissionConfig.ComboWindowSeconds;
            int multiplier = ComboCount;
            if (multiplier > 1)
                _Session.AddScore(args.PointValue * (multiplier - 1));

            if (CurrentMission != null) CurrentMission.OnEnemyDestroyed(args);
            ResolveMission();
        }

        void OnPlayerHit(object sender, G10_EntityDestroyedEventArgs args)
        {
            ComboCount = 0;
            _ComboSeconds = 0;
            if (CurrentMission != null) CurrentMission.OnPlayerHit();
            ResolveMission();
        }

        void OnPowerCellCollected(object sender, EventArgs args)
        {
            if (CurrentMission != null) CurrentMission.OnPowerCellCollected();
            ResolveMission();
        }

        void OnPowerCellEscaped(object sender, EventArgs args)
        {
            if (CurrentMission != null) CurrentMission.OnPowerCellEscaped();
            ResolveMission();
        }

        void ResolveMission()
        {
            if (CurrentMission == null || CurrentMission.Status == G10_MissionStatus.Active ||
                _OutcomeApplied) return;
            _OutcomeApplied = true;
            if (CurrentMission.Status == G10_MissionStatus.Succeeded)
            {
                CurrentMission.Reward.Apply(_Session);
                MissionsCompleted++;
                BannerText = "ERFOLG: " + CurrentMission.Reward.Description;
                BannerDetails = CurrentMission.Title;
            }
            else
            {
                CurrentMission.Penalty.Apply(_Session);
                BannerText = "MISSION FEHLGESCHLAGEN";
                BannerDetails = "STRAFE: " + CurrentMission.Penalty.Description;
            }
            _BannerSeconds = G10_SolarShooterMissionConfig.BannerDuration;
        }
    }
}
