using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OOPGames
{
    // Enemy kinds as JSON files under Classes/Gruppe 9/Assets/Enemies/Types (in git, shared
    // by the team). Standard.json may adjust the standard enemy; without it the values from
    // the code apply.
    public static class EnemyTypeRepository
    {
        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true,
                // Only the values that differ from the standard enemy are written.
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

        // The test harness points this at a temporary folder so tests neither depend on
        // nor change the team's enemy kinds.
        internal static string DirectoryOverride { get; set; }

        public static string TypesDirectory
        {
            get
            {
                string directory = DirectoryOverride ?? Path.Combine(
                    PainterG9.FindAssetDirectory("Enemies"), "Types");
                Directory.CreateDirectory(directory);
                return directory;
            }
        }

        public static IReadOnlyList<string> GetTypeIds()
        {
            List<string> ids = new List<string> { EnemyType.StandardId };
            ids.AddRange(Directory.GetFiles(TypesDirectory, "*.json")
                .Select(Path.GetFileNameWithoutExtension)
                .Where(EnemyType.IsValidId)
                .OrderBy(id => id, StringComparer.OrdinalIgnoreCase));
            return ids.Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        }

        // The stored definition, or an empty one for the standard enemy without a file.
        public static EnemyTypeDefinition LoadDefinition(string id)
        {
            string path = GetPath(id);
            if (!File.Exists(path))
            {
                if (String.Equals(id, EnemyType.StandardId, StringComparison.OrdinalIgnoreCase))
                {
                    return new EnemyTypeDefinition { Id = EnemyType.StandardId };
                }
                throw new FileNotFoundException("Die Gegnerart existiert nicht.", path);
            }

            EnemyTypeDefinition definition =
                JsonSerializer.Deserialize<EnemyTypeDefinition>(
                    File.ReadAllText(path), JsonOptions);
            if (definition == null)
            {
                throw new InvalidDataException("Die Gegnerart-Datei ist leer.");
            }
            // The file name is the id; a different id inside the file is ignored.
            definition.Id = Path.GetFileNameWithoutExtension(path);
            return definition;
        }

        public static void SaveDefinition(EnemyTypeDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            EnemyType.Resolve(definition);
            File.WriteAllText(GetPath(definition.Id),
                JsonSerializer.Serialize(definition, JsonOptions));
        }

        // Deleting Standard only removes its adjustments.
        public static void DeleteDefinition(string id)
        {
            string path = GetPath(id);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        // Every kind that can be loaded, Standard first. Broken files are skipped and
        // described in problems; invalid values fall back to the standard values.
        public static IReadOnlyList<EnemyType> LoadAll(ICollection<string> problems = null)
        {
            List<EnemyType> types = new List<EnemyType>();
            IReadOnlyList<string> ids;
            try
            {
                ids = GetTypeIds();
            }
            catch (Exception exception) when (IsFileProblem(exception))
            {
                problems?.Add("Gegnerarten konnten nicht gelesen werden: " + exception.Message);
                return new[] { EnemyType.Standard };
            }

            foreach (string id in ids)
            {
                try
                {
                    types.Add(EnemyType.Resolve(LoadDefinition(id), problems));
                }
                catch (Exception exception) when (IsFileProblem(exception))
                {
                    problems?.Add(id + ": Datei kann nicht gelesen werden ("
                        + exception.Message + ")");
                    if (String.Equals(id, EnemyType.StandardId,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        types.Add(EnemyType.Standard);
                    }
                }
            }
            return types;
        }

        private static string GetPath(string id)
        {
            if (!EnemyType.IsValidId(id))
            {
                throw new ArgumentException(
                    "Die Gegner-Id darf nur Buchstaben, Ziffern, - und _ enthalten (1–40 Zeichen).",
                    nameof(id));
            }
            return Path.Combine(TypesDirectory, id + ".json");
        }

        private static bool IsFileProblem(Exception exception)
        {
            return exception is IOException
                || exception is UnauthorizedAccessException
                || exception is JsonException
                || exception is InvalidDataException
                || exception is ArgumentException;
        }
    }
}
