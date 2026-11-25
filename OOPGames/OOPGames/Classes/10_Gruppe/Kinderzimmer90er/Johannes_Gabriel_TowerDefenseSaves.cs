using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OOPGames
{
    // State of a started map. Enemies are not stored: a resumed map restarts the saved wave.
    public sealed class G10_TowerDefenseRunState
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
        public int[] MasteryLevels { get; set; } = new int[0];
        // True for a run in endless mode (waves after 50).
        public bool Endless { get; set; }
        // Seed of the random endless waves, so a resumed endless run gets the same waves again.
        public int EndlessSeed { get; set; }

        public G10_TowerUpgrades GetUpgrades(int spot)
        {
            G10_TowerUpgrades upgrades = new G10_TowerUpgrades();
            if (spot < TowerTiers.Length)
            {
                upgrades.Tier = Clamp(TowerTiers[spot], 1, G10_TowerUpgrades.MaxTier);
                upgrades.Damage = Clamp(At(DamageLevels, spot), 0, G10_TowerUpgrades.MaxDamage);
                upgrades.Range = Clamp(At(RangeLevels, spot), 0, G10_TowerUpgrades.MaxRange);
                upgrades.Speed = Clamp(At(SpeedLevels, spot), 0, G10_TowerUpgrades.MaxSpeed);
                // Mastery only counts while everything else is maxed.
                if (upgrades.IsComplete) upgrades.Mastery = Math.Max(0, At(MasteryLevels, spot));
            }
            else if (TowerLevels != null && spot < TowerLevels.Length)
            {
                upgrades.Damage = Clamp(TowerLevels[spot] - 1, 0, G10_TowerUpgrades.MaxDamage);
            }
            return upgrades;
        }

        public void SetUpgrades(G10_TowerUpgrades[] upgrades)
        {
            TowerLevels = null;
            TowerTiers = new int[upgrades.Length];
            DamageLevels = new int[upgrades.Length];
            RangeLevels = new int[upgrades.Length];
            SpeedLevels = new int[upgrades.Length];
            MasteryLevels = new int[upgrades.Length];
            for (int i = 0; i < upgrades.Length; i++)
            {
                if (upgrades[i] == null) continue;
                TowerTiers[i] = upgrades[i].Tier;
                DamageLevels[i] = upgrades[i].Damage;
                RangeLevels[i] = upgrades[i].Range;
                SpeedLevels[i] = upgrades[i].Speed;
                MasteryLevels[i] = upgrades[i].Mastery;
            }
        }

        static int At(int[] values, int index) { return values != null && index < values.Length ? values[index] : 0; }
        static int Clamp(int value, int min, int max) { return Math.Max(min, Math.Min(max, value)); }
    }

    public sealed class G10_TowerDefenseSaveSlot
    {
        public int UnlockedMaps { get; set; } = 1;
        // Key = map index.
        public Dictionary<int, G10_TowerDefenseRunState> Runs { get; set; } = new Dictionary<int, G10_TowerDefenseRunState>();
        public DateTime? LastPlayed { get; set; }
        // Map that was played last, -1 if none.
        public int LastMap { get; set; } = -1;
        // Starts the next wave automatically shortly after a wave has been beaten.
        public bool AutoStart { get; set; }
        // Key = map index, value = bit mask of the difficulties the map was beaten on (bit 0 = Easy ... bit 3 = God).
        public Dictionary<int, int> CompletedDifficulties { get; set; } = new Dictionary<int, int>();
        // Key = map index, value = highest wave number beaten on the map in endless mode (at least 50, the
        // first 50 waves count as well).
        public Dictionary<int, int> EndlessRecords { get; set; } = new Dictionary<int, int>();

        public int EndlessRecord(int mapIndex)
        {
            int wave;
            return EndlessRecords != null && EndlessRecords.TryGetValue(mapIndex, out wave) ? wave : 0;
        }

        public bool HasCompleted(int mapIndex, int difficulty)
        {
            int mask;
            return CompletedDifficulties != null && CompletedDifficulties.TryGetValue(mapIndex, out mask) && (mask & (1 << difficulty)) != 0;
        }

        // True when the map was beaten on all four difficulties.
        public bool HasCompletedAll(int mapIndex)
        {
            for (int d = 0; d < 4; d++) if (!HasCompleted(mapIndex, d)) return false;
            return true;
        }

        [JsonIgnore]
        public bool IsEmpty { get { return LastPlayed == null; } }

        public bool IsUnlocked(int mapIndex)
        {
            return mapIndex >= 0 && mapIndex < UnlockedMaps;
        }

        public G10_TowerDefenseRunState GetRun(int mapIndex)
        {
            G10_TowerDefenseRunState run;
            return Runs.TryGetValue(mapIndex, out run) ? run : null;
        }
    }

    // Three independent save slots, stored next to the Kinderzimmer highscores.
    static class G10_TowerDefenseSaves
    {
        public const int SlotCount = 3;
        static readonly string _Folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programmieren3WS2026", "Kinderzimmer90er");
        static G10_TowerDefenseSaveSlot[] _Slots;

        // Can be redirected (e.g. for tests) before the first access to Slots.
        internal static string FilePath { get; set; } = Path.Combine(_Folder, "towerdefense_saves.json");

        public static IList<G10_TowerDefenseSaveSlot> Slots
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
            Slots[slotIndex] = new G10_TowerDefenseSaveSlot();
            Save();
        }

        // Records the beaten difficulty and drops the finished run.
        // The next map is only unlocked when unlocksNext is true (beaten on
        // Normal or harder); returns true if a new map was unlocked.
        public static bool CompleteMap(int slotIndex, int mapIndex, int difficulty, bool unlocksNext)
        {
            G10_TowerDefenseSaveSlot slot = Slots[slotIndex];
            slot.Runs.Remove(mapIndex);
            if (slot.CompletedDifficulties == null) slot.CompletedDifficulties = new Dictionary<int, int>();
            int mask;
            slot.CompletedDifficulties.TryGetValue(mapIndex, out mask);
            slot.CompletedDifficulties[mapIndex] = mask | (1 << difficulty);
            int unlocked = unlocksNext ? Math.Min(G10_TowerDefenseMaps.Maps.Count, mapIndex + 2) : slot.UnlockedMaps;
            bool newMap = unlocked > slot.UnlockedMaps;
            if (newMap) slot.UnlockedMaps = unlocked;
            Touch(slot, mapIndex);
            Save();
            return newMap;
        }

        public static void StoreRun(int slotIndex, int mapIndex, G10_TowerDefenseRunState run)
        {
            G10_TowerDefenseSaveSlot slot = Slots[slotIndex];
            slot.Runs[mapIndex] = run;
            Touch(slot, mapIndex);
            Save();
        }

        // Stores a beaten endless wave if it is a new record (saved together with the next Save()).
        public static bool RecordEndlessWave(int slotIndex, int mapIndex, int wave)
        {
            G10_TowerDefenseSaveSlot slot = Slots[slotIndex];
            if (slot.EndlessRecords == null) slot.EndlessRecords = new Dictionary<int, int>();
            if (wave <= slot.EndlessRecord(mapIndex)) return false;
            slot.EndlessRecords[mapIndex] = wave;
            return true;
        }

        public static void DiscardRun(int slotIndex, int mapIndex)
        {
            Slots[slotIndex].Runs.Remove(mapIndex);
            Save();
        }

        static void Touch(G10_TowerDefenseSaveSlot slot, int mapIndex)
        {
            slot.LastPlayed = DateTime.Now;
            slot.LastMap = mapIndex;
        }

        static G10_TowerDefenseSaveSlot[] Load()
        {
            G10_TowerDefenseSaveSlot[] slots = new G10_TowerDefenseSaveSlot[SlotCount];
            for (int i = 0; i < SlotCount; i++) slots[i] = new G10_TowerDefenseSaveSlot();

            if (File.Exists(FilePath))
            {
                G10_TowerDefenseSaveSlot[] stored = JsonSerializer.Deserialize<G10_TowerDefenseSaveSlot[]>(File.ReadAllText(FilePath));
                if (stored == null || stored.Length != SlotCount)
                    throw new InvalidDataException("Die Tower-Defense-Spielstände sind ungültig: " + FilePath);
                for (int i = 0; i < SlotCount; i++)
                {
                    if (stored[i] == null) continue;
                    if (stored[i].Runs == null) stored[i].Runs = new Dictionary<int, G10_TowerDefenseRunState>();
                    stored[i].UnlockedMaps = Math.Max(1, Math.Min(G10_TowerDefenseMaps.Maps.Count, stored[i].UnlockedMaps));
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
                    slots[0].UnlockedMaps = Math.Max(1, Math.Min(G10_TowerDefenseMaps.Maps.Count, unlocked));
                    slots[0].LastPlayed = File.GetLastWriteTime(oldProgress);
                }
            }
            return slots;
        }
    }
}
