using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OOPGames
{
    // State of a started map. Enemies are not stored: a resumed map restarts the saved wave.
    public sealed class TowerDefenseRunState
    {
        public int Difficulty { get; set; }
        public int Wave { get; set; } = 1;
        public int Gold { get; set; }
        public int MainBuildingHealth { get; set; } = 100;
        // Tower name per pedestal, null for an empty pedestal.
        public string[] Towers { get; set; } = new string[0];
        // Only written by older versions (one combined level per tower); read for compatibility.
        public int[] TowerLevels { get; set; } = new int[0];
        // Upgrade state per pedestal (see TowerUpgrades).
        public int[] TowerTiers { get; set; } = new int[0];
        public int[] DamageLevels { get; set; } = new int[0];
        public int[] RangeLevels { get; set; } = new int[0];
        public int[] SpeedLevels { get; set; } = new int[0];

        public TowerUpgrades GetUpgrades(int spot)
        {
            TowerUpgrades upgrades = new TowerUpgrades();
            if (spot < TowerTiers.Length)
            {
                upgrades.Tier = Clamp(TowerTiers[spot], 1, TowerUpgrades.MaxTier);
                upgrades.Damage = Clamp(At(DamageLevels, spot), 0, TowerUpgrades.MaxDamage);
                upgrades.Range = Clamp(At(RangeLevels, spot), 0, TowerUpgrades.MaxRange);
                upgrades.Speed = Clamp(At(SpeedLevels, spot), 0, TowerUpgrades.MaxSpeed);
            }
            else if (TowerLevels != null && spot < TowerLevels.Length)
            {
                upgrades.Damage = Clamp(TowerLevels[spot] - 1, 0, TowerUpgrades.MaxDamage);
            }
            return upgrades;
        }

        public void SetUpgrades(TowerUpgrades[] upgrades)
        {
            TowerLevels = null;
            TowerTiers = new int[upgrades.Length];
            DamageLevels = new int[upgrades.Length];
            RangeLevels = new int[upgrades.Length];
            SpeedLevels = new int[upgrades.Length];
            for (int i = 0; i < upgrades.Length; i++)
            {
                if (upgrades[i] == null) continue;
                TowerTiers[i] = upgrades[i].Tier;
                DamageLevels[i] = upgrades[i].Damage;
                RangeLevels[i] = upgrades[i].Range;
                SpeedLevels[i] = upgrades[i].Speed;
            }
        }

        static int At(int[] values, int index) { return values != null && index < values.Length ? values[index] : 0; }
        static int Clamp(int value, int min, int max) { return Math.Max(min, Math.Min(max, value)); }
    }

    public sealed class TowerDefenseSaveSlot
    {
        public int UnlockedMaps { get; set; } = 1;
        // Key = map index.
        public Dictionary<int, TowerDefenseRunState> Runs { get; set; } = new Dictionary<int, TowerDefenseRunState>();
        public DateTime? LastPlayed { get; set; }
        // Map that was played last, -1 if none.
        public int LastMap { get; set; } = -1;

        [JsonIgnore]
        public bool IsEmpty { get { return LastPlayed == null; } }

        public bool IsUnlocked(int mapIndex)
        {
            return mapIndex >= 0 && mapIndex < UnlockedMaps;
        }

        public TowerDefenseRunState GetRun(int mapIndex)
        {
            TowerDefenseRunState run;
            return Runs.TryGetValue(mapIndex, out run) ? run : null;
        }
    }

    // Three independent save slots, stored next to the Kinderzimmer highscores.
    static class TowerDefenseSaves
    {
        public const int SlotCount = 3;
        static readonly string _Folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programmieren3WS2026", "Kinderzimmer90er");
        static TowerDefenseSaveSlot[] _Slots;

        // Can be redirected (e.g. for tests) before the first access to Slots.
        internal static string FilePath { get; set; } = Path.Combine(_Folder, "towerdefense_saves.json");

        public static IList<TowerDefenseSaveSlot> Slots
        {
            get
            {
                if (_Slots == null) _Slots = Load();
                return _Slots;
            }
        }

        public static void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath, JsonSerializer.Serialize(Slots, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static void Delete(int slotIndex)
        {
            Slots[slotIndex] = new TowerDefenseSaveSlot();
            Save();
        }

        // Unlocks the next map and drops the finished run. Returns true when a new map was unlocked.
        public static bool CompleteMap(int slotIndex, int mapIndex)
        {
            TowerDefenseSaveSlot slot = Slots[slotIndex];
            slot.Runs.Remove(mapIndex);
            int unlocked = Math.Min(TowerDefenseMaps.Maps.Count, mapIndex + 2);
            bool newMap = unlocked > slot.UnlockedMaps;
            if (newMap) slot.UnlockedMaps = unlocked;
            Touch(slot, mapIndex);
            Save();
            return newMap;
        }

        public static void StoreRun(int slotIndex, int mapIndex, TowerDefenseRunState run)
        {
            TowerDefenseSaveSlot slot = Slots[slotIndex];
            slot.Runs[mapIndex] = run;
            Touch(slot, mapIndex);
            Save();
        }

        public static void DiscardRun(int slotIndex, int mapIndex)
        {
            Slots[slotIndex].Runs.Remove(mapIndex);
            Save();
        }

        static void Touch(TowerDefenseSaveSlot slot, int mapIndex)
        {
            slot.LastPlayed = DateTime.Now;
            slot.LastMap = mapIndex;
        }

        static TowerDefenseSaveSlot[] Load()
        {
            TowerDefenseSaveSlot[] slots = new TowerDefenseSaveSlot[SlotCount];
            for (int i = 0; i < SlotCount; i++) slots[i] = new TowerDefenseSaveSlot();

            if (File.Exists(FilePath))
            {
                TowerDefenseSaveSlot[] stored = JsonSerializer.Deserialize<TowerDefenseSaveSlot[]>(File.ReadAllText(FilePath));
                if (stored == null || stored.Length != SlotCount)
                    throw new InvalidDataException("Die Tower-Defense-Spielstaende sind ungueltig: " + FilePath);
                for (int i = 0; i < SlotCount; i++)
                {
                    if (stored[i] == null) continue;
                    if (stored[i].Runs == null) stored[i].Runs = new Dictionary<int, TowerDefenseRunState>();
                    stored[i].UnlockedMaps = Math.Max(1, Math.Min(TowerDefenseMaps.Maps.Count, stored[i].UnlockedMaps));
                    slots[i] = stored[i];
                }
                return slots;
            }

            // Older versions only stored the unlocked maps; they become save slot 1.
            string oldProgress = Path.Combine(Path.GetDirectoryName(FilePath), "towerdefense_progress.json");
            if (File.Exists(oldProgress))
            {
                Dictionary<string, int> data = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(oldProgress));
                int unlocked;
                if (data != null && data.TryGetValue("UnlockedMaps", out unlocked))
                {
                    slots[0].UnlockedMaps = Math.Max(1, Math.Min(TowerDefenseMaps.Maps.Count, unlocked));
                    slots[0].LastPlayed = File.GetLastWriteTime(oldProgress);
                }
            }
            return slots;
        }
    }
}
