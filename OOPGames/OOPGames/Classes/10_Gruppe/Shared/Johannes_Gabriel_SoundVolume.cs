using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace OOPGames
{
    // Master volume for all games of group 10 in steps of 5 % (level 0 = silent ... 20 = 100 %).
    // Every sound that is played through G10_MediaClipOutput follows it. The last value is
    // remembered between sessions; the very first start begins at 50 %.
    public static class G10_SoundVolume
    {
        public const int MaxLevel = 20;
        public const int PercentPerLevel = 5;
        public const int DefaultLevel = 10;
        const string PercentKey = "Percent";
        // Older files stored a level from 0 to 10 (10 % steps).
        const string OldLevelKey = "Level";

        static readonly string _Path = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programmieren3WS2026", "Kinderzimmer90er", "sound_settings.json");
        static int _Level = Load();

        // Raised on the UI thread after every change of Level.
        public static event Action Changed;

        public static int Level
        {
            get { return _Level; }
            set
            {
                int level = Math.Max(0, Math.Min(MaxLevel, value));
                if (level == _Level) return;
                _Level = level;
                LastChanged = DateTime.Now;
                Save();
                Action changed = Changed;
                if (changed != null) changed();
            }
        }

        // Volume for WPF MediaPlayer (0.0 - 1.0).
        public static double Volume { get { return _Level / (double)MaxLevel; } }

        public static int Percent { get { return _Level * PercentPerLevel; } }

        public static bool IsMuted { get { return _Level == 0; } }

        // Time of the last change; the room shows the volume panel briefly after a change.
        public static DateTime LastChanged { get; private set; } = DateTime.MinValue;

        public static void Increase() { Level = _Level + 1; }
        public static void Decrease() { Level = _Level - 1; }

        // No stored value (first start) or an unreadable file: 50 %.
        static int Load()
        {
            try
            {
                if (!File.Exists(_Path)) return DefaultLevel;
                Dictionary<string, int> data = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(_Path));
                if (data == null) return DefaultLevel;
                int value;
                if (data.TryGetValue(PercentKey, out value)) return Math.Max(0, Math.Min(MaxLevel, value / PercentPerLevel));
                if (data.TryGetValue(OldLevelKey, out value)) return Math.Max(0, Math.Min(MaxLevel, value * 2));
                return DefaultLevel;
            }
            catch (IOException) { return DefaultLevel; }
            catch (UnauthorizedAccessException) { return DefaultLevel; }
            catch (JsonException) { return DefaultLevel; }
        }

        // A volume that cannot be stored still applies for this session.
        static void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_Path));
                File.WriteAllText(_Path, JsonSerializer.Serialize(new Dictionary<string, int> { { PercentKey, Percent } }));
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
