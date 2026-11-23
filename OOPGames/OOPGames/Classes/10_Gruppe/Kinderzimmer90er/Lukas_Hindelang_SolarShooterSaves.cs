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

    // Persistence is injected into the game (like IG10_HighscoreStore in Tetris), so the
    // game logic can be tested with an in-memory store instead of the real save file.
    internal interface IG10_ShooterSaveStore
    {
        G10_SolarShooterModeSave GetMode(string modeId);
        void Save();
    }

    internal sealed class G10_FileShooterSaveStore : IG10_ShooterSaveStore
    {
        public static readonly string DefaultFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programmieren3WS2026", "Kinderzimmer90er", "solarshooter_saves.json");

        readonly string _FilePath;
        G10_SolarShooterSaveData _Data;

        public G10_FileShooterSaveStore()
            : this(DefaultFilePath) { }

        public G10_FileShooterSaveStore(string filePath)
        {
            if (string.IsNullOrEmpty(Path.GetDirectoryName(filePath)))
                throw new ArgumentException("The SolarShooter save path must include a directory.", "filePath");
            _FilePath = filePath;
        }

        G10_SolarShooterSaveData Data
        {
            get
            {
                if (_Data == null) _Data = Load();
                return _Data;
            }
        }

        public G10_SolarShooterModeSave GetMode(string modeId)
        {
            return Data.GetMode(modeId);
        }

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_FilePath));
            File.WriteAllText(_FilePath, JsonSerializer.Serialize(
                Data, new JsonSerializerOptions { WriteIndented = true }));
        }

        G10_SolarShooterSaveData Load()
        {
            if (!File.Exists(_FilePath)) return new G10_SolarShooterSaveData();
            G10_SolarShooterSaveData data =
                JsonSerializer.Deserialize<G10_SolarShooterSaveData>(File.ReadAllText(_FilePath));
            if (data == null || data.Modes == null)
                throw new InvalidDataException("Der SolarShooter-Spielstand ist ungueltig: " + _FilePath);
            return data;
        }
    }
}
