using System;
using System.IO;
using System.Text.Json;

namespace OOPGames
{
    // Five save slots as JSON files (slot1.json … slot5.json) in one folder, by default the
    // local application data folder next to the key bindings. Unreadable files count as empty.
    public sealed class ArenaSaveGameRepository
    {
        public const int SlotCount = 5;
        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions { WriteIndented = true };
        private readonly string _folder;

        public ArenaSaveGameRepository(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentException("A save folder is required.", nameof(folder));
            }
            _folder = folder;
        }

        public static string DefaultFolder
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OOPGames", "ArenaSurvival", "SaveGames");
            }
        }

        public static string GetSlotName(int slot)
        {
            return "Slot " + (slot + 1);
        }

        public ArenaSaveGame Load(int slot)
        {
            string path = GetPath(slot);
            try
            {
                if (!File.Exists(path))
                {
                    return null;
                }
                ArenaSaveGame game = JsonSerializer.Deserialize<ArenaSaveGame>(
                    File.ReadAllText(path), JsonOptions);
                return game != null && game.Progression != null && game.Player != null
                    ? game
                    : null;
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException
                || exception is JsonException)
            {
                return null;
            }
        }

        public bool Save(int slot, ArenaSaveGame game)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }
            string path = GetPath(slot);
            try
            {
                Directory.CreateDirectory(_folder);
                // Write to a temporary file first, so a crash never leaves half a save.
                string temporary = path + ".tmp";
                File.WriteAllText(temporary, JsonSerializer.Serialize(game, JsonOptions));
                File.Copy(temporary, path, true);
                File.Delete(temporary);
                return true;
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException)
            {
                return false;
            }
        }

        public void Delete(int slot)
        {
            try
            {
                File.Delete(GetPath(slot));
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException)
            {
                // A save that cannot be deleted simply stays.
            }
        }

        public bool IsUsed(int slot)
        {
            return Load(slot) != null;
        }

        public int? FindFreeSlot()
        {
            for (int slot = 0; slot < SlotCount; slot++)
            {
                if (!IsUsed(slot))
                {
                    return slot;
                }
            }
            return null;
        }

        // The slot that was saved last; used to continue at program start.
        public int? FindMostRecentSlot()
        {
            int? mostRecent = null;
            DateTime latest = DateTime.MinValue;
            for (int slot = 0; slot < SlotCount; slot++)
            {
                ArenaSaveGame game = Load(slot);
                if (game != null && game.SavedAt >= latest)
                {
                    latest = game.SavedAt;
                    mostRecent = slot;
                }
            }
            return mostRecent;
        }

        private string GetPath(int slot)
        {
            if (slot < 0 || slot >= SlotCount)
            {
                throw new ArgumentOutOfRangeException(nameof(slot));
            }
            return Path.Combine(_folder, "slot" + (slot + 1) + ".json");
        }
    }
}
