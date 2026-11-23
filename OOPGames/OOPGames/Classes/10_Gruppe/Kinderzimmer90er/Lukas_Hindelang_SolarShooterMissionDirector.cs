using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Picks the next mission, forwards the session events to it, applies its reward or
    // penalty and keeps the combo counter. Doubles as the HUD source of the mission mode.
    internal sealed class G10_MissionDirector : IG10_ModeHud
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

        bool HasActiveMission
        {
            get { return CurrentMission != null && CurrentMission.Status == G10_MissionStatus.Active; }
        }

        string IG10_ModeHud.BannerTitle { get { return BannerText; } }
        string IG10_ModeHud.BannerDetails { get { return BannerDetails; } }
        string IG10_ModeHud.TaskTitle
        {
            get { return HasActiveMission ? CurrentMission.Title + ": " + CurrentMission.ProgressText : null; }
        }
        string IG10_ModeHud.TaskDescription
        {
            get { return HasActiveMission ? CurrentMission.Description : null; }
        }
        string IG10_ModeHud.ComboText
        {
            get { return ComboCount > 1 ? "COMBO x" + ComboCount : null; }
        }
        string IG10_ModeHud.SummaryText
        {
            get { return "MISSIONEN GESCHAFFT: " + MissionsCompleted + "/" + MissionsAttempted; }
        }

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
