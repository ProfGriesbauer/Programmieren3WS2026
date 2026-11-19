using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OOPGames
{
    internal sealed class G10_SolarShooterModeSave
    {
        public bool HasResume { get; set; }
        public int ResumeStageIndex { get; set; }
        public int ResumeScore { get; set; }
        public int ResumeWeaponLevel { get; set; } = 1;
        public int ResumeMissionsCompleted { get; set; }
        public int ResumeMissionsAttempted { get; set; }
        public string ResumePreviousMissionId { get; set; }
        public int BestScore { get; set; }
        public int BestStage { get; set; }
        public int BestWeaponLevel { get; set; } = 1;
        public int BestMissionsCompleted { get; set; }
    }

    internal sealed class G10_SolarShooterSaveData
    {
        public Dictionary<string, G10_SolarShooterModeSave> Modes { get; set; } =
            new Dictionary<string, G10_SolarShooterModeSave>();

        public G10_SolarShooterModeSave GetMode(string modeId)
        {
            G10_SolarShooterModeSave save;
            if (!Modes.TryGetValue(modeId, out save))
            {
                save = new G10_SolarShooterModeSave();
                Modes.Add(modeId, save);
            }
            return save;
        }
    }

    internal static class G10_SolarShooterSaves
    {
        static readonly string Folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programmieren3WS2026", "Kinderzimmer90er");
        static G10_SolarShooterSaveData _Data;

        internal static string FilePath { get; set; } =
            Path.Combine(Folder, "solarshooter_saves.json");

        public static G10_SolarShooterSaveData Data
        {
            get
            {
                if (_Data == null) _Data = Load();
                return _Data;
            }
        }

        public static void Save()
        {
            string directory = Path.GetDirectoryName(FilePath);
            if (string.IsNullOrEmpty(directory))
                throw new InvalidOperationException("The SolarShooter save path must include a directory.");
            Directory.CreateDirectory(directory);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(
                Data, new JsonSerializerOptions { WriteIndented = true }));
        }

        static G10_SolarShooterSaveData Load()
        {
            if (!File.Exists(FilePath)) return new G10_SolarShooterSaveData();
            G10_SolarShooterSaveData data =
                JsonSerializer.Deserialize<G10_SolarShooterSaveData>(File.ReadAllText(FilePath));
            if (data == null || data.Modes == null)
                throw new InvalidDataException("Der SolarShooter-Spielstand ist ungueltig: " + FilePath);
            return data;
        }
    }
}
