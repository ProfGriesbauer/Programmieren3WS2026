using System;
using System.IO;
using System.Text.Json;

namespace OOPGames
{
    // Small menu preferences that survive restarts, stored next to the key bindings in the
    // local application data folder. Missing or broken files give the defaults.
    public sealed class ArenaPreferences
    {
        private static ArenaPreferences _default;
        private readonly string _filePath;
        private bool _askBeforeSkillPointPayment = true;

        private ArenaPreferences(string filePath)
        {
            _filePath = filePath;
        }

        public static string DefaultFilePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OOPGames", "ArenaSurvival", "preferences.json");
            }
        }

        // Shared preferences of the running game.
        public static ArenaPreferences Default
        {
            get { return _default ?? (_default = Load(DefaultFilePath)); }
        }

        // Ask before a weapon upgrade is paid partly with skill points; switched off by the
        // "Nicht erneut fragen" option of that question. Saved on every change.
        public bool AskBeforeSkillPointPayment
        {
            get { return _askBeforeSkillPointPayment; }
            set
            {
                if (_askBeforeSkillPointPayment == value)
                {
                    return;
                }
                _askBeforeSkillPointPayment = value;
                Save();
            }
        }

        public static ArenaPreferences Load(string filePath)
        {
            ArenaPreferences preferences = new ArenaPreferences(filePath);
            try
            {
                if (File.Exists(filePath))
                {
                    StoredPreferences stored = JsonSerializer.Deserialize<StoredPreferences>(
                        File.ReadAllText(filePath));
                    if (stored != null)
                    {
                        preferences._askBeforeSkillPointPayment =
                            stored.AskBeforeSkillPointPayment;
                    }
                }
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException
                || exception is JsonException)
            {
                // Unreadable preferences fall back to the defaults.
            }
            return preferences;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                File.WriteAllText(_filePath, JsonSerializer.Serialize(new StoredPreferences
                {
                    AskBeforeSkillPointPayment = _askBeforeSkillPointPayment
                }));
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException)
            {
                // A read-only folder only loses the preference for the next start.
            }
        }

        private sealed class StoredPreferences
        {
            public bool AskBeforeSkillPointPayment { get; set; } = true;
        }
    }
}
