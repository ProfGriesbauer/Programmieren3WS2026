using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Strategy: rewards and penalties are interchangeable objects that a mission carries.
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
        public void Apply(G10_SolarShooterSession session) { session.ActivateShield(G10_SolarShooterMissionConfig.ShieldDuration); }
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
}
