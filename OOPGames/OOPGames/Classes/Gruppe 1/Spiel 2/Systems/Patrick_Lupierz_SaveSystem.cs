using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OOPGames
{
    public class G1C_SaveNpc
    {
        public string Name { get; set; }
        public int Trust { get; set; }
        public int Respect { get; set; }
        public int Status { get; set; }
        public int Purse { get; set; }
        public int Mood { get; set; }
        public G1C_NpcMemory Memory { get; set; }
    }

    public class G1C_SaveTask
    {
        public string Giver { get; set; }
        public int State { get; set; }
        public bool[] Collected { get; set; }
        public int AvailableAgainTick { get; set; }
    }

    // Entwickler: Patrick Lupierz – kompletter Spielstand als einfache Datenklasse (wird als JSON gespeichert)
    public class G1C_SaveData
    {
        public int Version { get; set; }
        public G1C_PlayerProfile Profile { get; set; }
        public int Location { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Facing { get; set; }
        public int Coins { get; set; }
        public Dictionary<string, int> Inventory { get; set; }
        public List<G1C_ActiveBuff> Buffs { get; set; }
        public int ClockTicks { get; set; }
        public int PlayTicks { get; set; }
        public int GamesPlayed { get; set; }
        public int Reputation { get; set; }
        public List<G1C_SaveNpc> Npcs { get; set; }
        public List<G1C_SaveTask> Tasks { get; set; }
        public List<G1C_Sms> Messages { get; set; }
        public Dictionary<string, int> Trash { get; set; }
    }

    // =====================================================
    // Klasse: G1C_SaveSystem
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Liest und schreibt Spielstände als versionierte JSON-Dateien im Ordner
    //               "G1Casino_Saves" neben der OOPGames.exe (wie der Grafikordner, außerhalb des
    //               Quellcodes). Beschädigte oder fremde Dateien werden als "nicht ladbar" gemeldet,
    //               statt das Spiel abstürzen zu lassen. Geschrieben wird erst in eine Zwischendatei.
    // =====================================================
    public static class G1C_SaveSystem
    {
        public const int SlotCount = 3;
        public const int CurrentVersion = 1;

        public static string Folder { get; set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "G1Casino_Saves");

        private static string FileOf(int slot)
        {
            return Path.Combine(Folder, "spielstand_" + slot + ".json");
        }

        public static bool Save(int slot, G1C_SaveData data)
        {
            if (slot < 1 || slot > SlotCount || data == null) return false;
            try
            {
                data.Version = CurrentVersion;
                Directory.CreateDirectory(Folder);
                string temp = FileOf(slot) + ".tmp";
                File.WriteAllText(temp, JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true }));
                File.Move(temp, FileOf(slot), true);
                return true;
            }
            catch (Exception)
            {
                return false; // z.B. Ordner schreibgeschützt – der Aufrufer meldet es dem Spieler
            }
        }

        /// <summary>Lädt einen Spielstand; null, wenn er fehlt, beschädigt ist oder eine andere Version hat.</summary>
        public static G1C_SaveData Load(int slot)
        {
            try
            {
                if (slot < 1 || slot > SlotCount || !File.Exists(FileOf(slot))) return null;
                G1C_SaveData data = JsonSerializer.Deserialize<G1C_SaveData>(File.ReadAllText(FileOf(slot)));
                bool valid = data != null && data.Version == CurrentVersion && data.Profile != null &&
                             G1C_PlayerProfile.IsValidName(data.Profile.Name) && data.Coins >= 0 && data.ClockTicks >= 0 &&
                             Enum.IsDefined(typeof(G1C_WorldLocation), data.Location) &&
                             Enum.IsDefined(typeof(G1C_Direction), data.Facing);
                return valid ? data : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static bool Exists(int slot)
        {
            return slot >= 1 && slot <= SlotCount && File.Exists(FileOf(slot));
        }

        // Entwickler: Patrick Lupierz – Zeile für die Speicherplatz-Auswahl
        public static string Describe(int slot)
        {
            if (!Exists(slot)) return "Platz " + slot + ": - leer -";
            G1C_SaveData data = Load(slot);
            if (data == null) return "Platz " + slot + ": nicht ladbar (beschädigt)";

            G1C_GameClock clock = new G1C_GameClock { TotalTicks = data.ClockTicks };
            int minutes = data.PlayTicks / 25 / 60;
            return "Platz " + slot + ": " + data.Profile.Name + " - " +
                   G1C_SceneTransitionManager.DisplayName((G1C_WorldLocation)data.Location) +
                   " - Tag " + clock.Day + ", " + clock.TimeText + " - " + minutes / 60 + "h " + (minutes % 60).ToString("00") + "m";
        }
    }
}
