using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Everything needed to continue a run exactly where it was left: the run progress, the
    // player and every enemy with their statuses. Short-lived state (running attacks,
    // cooldowns, ground traces, particles, slow motion) is not saved. Stored as JSON.
    public sealed class ArenaSaveGame
    {
        public const int CurrentVersion = 1;

        public int Version { get; set; } = CurrentVersion;
        public string RunId { get; set; }
        public DateTime SavedAt { get; set; }
        public string MapFileName { get; set; }
        public int Score { get; set; }
        public int Deaths { get; set; }
        public string SelectedWeaponId { get; set; }
        public string AnimationProfileName { get; set; }
        public double ZoomLevel { get; set; } = 1;
        public bool ShowDamageRanges { get; set; }
        public bool ShowDebugHud { get; set; }
        public double GameSpeed { get; set; } = 1;
        public double EnemySpawnElapsedSeconds { get; set; }
        public double AimDirectionX { get; set; }
        public double AimDirectionY { get; set; } = 1;
        public ArenaProgressionSave Progression { get; set; }
        public ArenaEntitySave Player { get; set; }
        public List<ArenaEntitySave> Enemies { get; set; } = new List<ArenaEntitySave>();
        // Mirror images of the Mirror Image ability (added with that ability; older saves
        // simply have none).
        public List<ArenaMirrorImageSave> MirrorImages { get; set; } =
            new List<ArenaMirrorImageSave>();
        // Remaining cooldowns of the player's weapons and abilities by weapon id, so loading
        // cannot reset e.g. the long Mirror Image cooldown.
        public Dictionary<string, double> PlayerCooldowns { get; set; } =
            new Dictionary<string, double>();
    }

    public sealed class ArenaMirrorImageSave
    {
        public ArenaEntitySave State { get; set; }
        public double StationarySeconds { get; set; }
        public double InvulnerableSeconds { get; set; }
    }

    public sealed class ArenaProgressionSave
    {
        public int UniversalPoints { get; set; }
        public int TotalKills { get; set; }
        public bool IsGodMode { get; set; }
        public Dictionary<string, int> WeaponPoints { get; set; } =
            new Dictionary<string, int>();
        public Dictionary<string, int> SkillRanks { get; set; } =
            new Dictionary<string, int>();
        public Dictionary<string, int> WeaponUpgradeRanks { get; set; } =
            new Dictionary<string, int>();
        public Dictionary<string, string> ForcedForkChoices { get; set; } =
            new Dictionary<string, string>();
        public string[] EntityAbilities { get; set; } = Array.Empty<string>();
        public Dictionary<string, string[]> WeaponAbilities { get; set; } =
            new Dictionary<string, string[]>();
    }

    public sealed class ArenaEntitySave
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int HitPoints { get; set; }
        public string Facing { get; set; }
        public List<ArenaStatusSave> Statuses { get; set; } = new List<ArenaStatusSave>();
        public List<ArenaDamageOverTimeSave> DamageOverTime { get; set; } =
            new List<ArenaDamageOverTimeSave>();
        public double WeaknessMultiplier { get; set; } = 1;
        public double WeaknessSeconds { get; set; }
    }

    public sealed class ArenaStatusSave
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double SpeedMultiplier { get; set; }
        public double RemainingSeconds { get; set; }
    }

    public sealed class ArenaDamageOverTimeSave
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double DamagePerSecond { get; set; }
        public double RemainingSeconds { get; set; }
        public double SpreadRadius { get; set; }
        // The player caused it (kill credit); otherwise it has no source.
        public bool FromPlayer { get; set; }
    }
}
