using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OOPGames
{
    // High score list of the best runs: score = kills minus DeathPenalty per death. The running
    // run is updated live (every kill and death), so it climbs the list while it is played.
    // Entries stay when their save slot is deleted. Stored as JSON.
    public sealed class ArenaScoreboard
    {
        public const int DeathPenalty = 5;
        public const int MaximumEntries = 10;
        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions { WriteIndented = true };
        private readonly string _filePath;
        private readonly List<ArenaScoreEntry> _entries = new List<ArenaScoreEntry>();

        private ArenaScoreboard(string filePath)
        {
            _filePath = filePath;
        }

        public static string DefaultFilePath
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OOPGames", "ArenaSurvival", "highscores.json");
            }
        }

        // Best runs first.
        public IReadOnlyList<ArenaScoreEntry> Entries { get { return _entries.AsReadOnly(); } }

        public static int GetScore(int kills, int deaths)
        {
            return kills - DeathPenalty * deaths;
        }

        public static ArenaScoreboard Load(string filePath)
        {
            ArenaScoreboard scoreboard = new ArenaScoreboard(filePath);
            try
            {
                if (File.Exists(filePath))
                {
                    List<ArenaScoreEntry> entries =
                        JsonSerializer.Deserialize<List<ArenaScoreEntry>>(
                            File.ReadAllText(filePath), JsonOptions);
                    if (entries != null)
                    {
                        scoreboard._entries.AddRange(entries.Where(entry => entry != null));
                        scoreboard.SortAndTrim();
                    }
                }
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException
                || exception is JsonException)
            {
                // An unreadable list starts empty.
            }
            return scoreboard;
        }

        // Inserts or updates the entry of a run; returns its rank (1 = best) or 0 when it
        // does not make the list.
        public int Record(string runId, string label, int kills, int deaths, DateTime date)
        {
            if (string.IsNullOrEmpty(runId))
            {
                throw new ArgumentException("A run id is required.", nameof(runId));
            }

            ArenaScoreEntry entry = _entries.FirstOrDefault(item => item.RunId == runId);
            int score = GetScore(kills, deaths);
            if (entry != null && entry.Kills == kills && entry.Deaths == deaths
                && entry.Label == label)
            {
                return _entries.IndexOf(entry) + 1;
            }
            if (entry == null)
            {
                entry = new ArenaScoreEntry { RunId = runId };
                _entries.Add(entry);
            }
            entry.Label = label;
            entry.Kills = kills;
            entry.Deaths = deaths;
            entry.Score = score;
            entry.Date = date;
            SortAndTrim();
            Save();
            return _entries.IndexOf(entry) + 1;
        }

        public void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
                File.WriteAllText(_filePath, JsonSerializer.Serialize(_entries, JsonOptions));
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException)
            {
                // The list is kept in memory until the next successful save.
            }
        }

        private void SortAndTrim()
        {
            List<ArenaScoreEntry> sorted = _entries
                .OrderByDescending(entry => entry.Score)
                .ThenByDescending(entry => entry.Kills)
                .ThenBy(entry => entry.Date)
                .Take(MaximumEntries)
                .ToList();
            _entries.Clear();
            _entries.AddRange(sorted);
        }
    }

    public sealed class ArenaScoreEntry
    {
        public string RunId { get; set; }
        public string Label { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Score { get; set; }
        public DateTime Date { get; set; }
    }
}
