using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;

namespace OOPGames
{
    public enum ArenaRegionShape
    {
        Rectangle,
        Circle
    }

    // One shape of a named spawn region in world units. Rectangles use X/Y as top-left
    // corner with Width/Height, circles X/Y as centre with Radius. Several shapes with the
    // same name form one region (e.g. an L shape or two separate clearings).
    public sealed class ArenaMapRegion
    {
        public string Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ArenaRegionShape Shape { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Radius { get; set; }

        [JsonIgnore]
        public Rect Bounds
        {
            get
            {
                return Shape == ArenaRegionShape.Circle
                    ? new Rect(X - Radius, Y - Radius, Radius * 2, Radius * 2)
                    : new Rect(X, Y, Width, Height);
            }
        }

        [JsonIgnore]
        public double Area
        {
            get
            {
                return Shape == ArenaRegionShape.Circle
                    ? Math.PI * Radius * Radius
                    : Width * Height;
            }
        }

        public bool Contains(double x, double y)
        {
            if (Shape == ArenaRegionShape.Circle)
            {
                double offsetX = x - X;
                double offsetY = y - Y;
                return offsetX * offsetX + offsetY * offsetY <= Radius * Radius;
            }
            return x >= X && x <= X + Width && y >= Y && y <= Y + Height;
        }

        // A uniformly distributed point inside the shape.
        public Point GetRandomPoint(Random random)
        {
            if (Shape == ArenaRegionShape.Circle)
            {
                double angle = random.NextDouble() * Math.PI * 2;
                double distance = Radius * Math.Sqrt(random.NextDouble());
                return new Point(X + Math.Cos(angle) * distance, Y + Math.Sin(angle) * distance);
            }
            return new Point(X + random.NextDouble() * Width, Y + random.NextDouble() * Height);
        }

        public ArenaMapRegion Copy()
        {
            return (ArenaMapRegion)MemberwiseClone();
        }

        // German description of an invalid shape, or null.
        public string Validate()
        {
            if (String.IsNullOrWhiteSpace(Name))
            {
                return "Jede Region braucht einen Namen.";
            }
            if (!Enum.IsDefined(typeof(ArenaRegionShape), Shape))
            {
                return "„" + Name + "“: unbekannte Form.";
            }
            bool sizeValid = Shape == ArenaRegionShape.Circle
                ? Radius > 0
                : Width > 0 && Height > 0;
            if (!sizeValid || double.IsNaN(X) || double.IsNaN(Y))
            {
                return "„" + Name + "“: Größe muss größer als 0 sein.";
            }
            return null;
        }
    }

    // Spawn regions per map, stored next to the map as <map>.regions.json in
    // Assets/Background (in git, shared by the team). Maps without the file have no regions.
    public static class ArenaMapRegionRepository
    {
        public const string FileSuffix = ".regions.json";
        private static readonly JsonSerializerOptions JsonOptions =
            new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNameCaseInsensitive = true
            };

        // The test harness points this at a temporary folder.
        internal static string DirectoryOverride { get; set; }

        public static string RegionsDirectory
        {
            get { return DirectoryOverride ?? ArenaMapAssets.BackgroundDirectory; }
        }

        public static string GetPath(string mapFileName)
        {
            if (String.IsNullOrWhiteSpace(mapFileName)
                || mapFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                throw new ArgumentException("Ungültiger Map-Name.", nameof(mapFileName));
            }
            return Path.Combine(RegionsDirectory,
                Path.GetFileNameWithoutExtension(mapFileName) + FileSuffix);
        }

        // The map's regions; an empty list when it has none. Broken files throw.
        public static IReadOnlyList<ArenaMapRegion> Load(string mapFileName)
        {
            string path = GetPath(mapFileName);
            if (!File.Exists(path))
            {
                return Array.Empty<ArenaMapRegion>();
            }
            List<ArenaMapRegion> regions = JsonSerializer.Deserialize<List<ArenaMapRegion>>(
                File.ReadAllText(path), JsonOptions) ?? new List<ArenaMapRegion>();
            foreach (ArenaMapRegion region in regions)
            {
                string problem = region == null ? "Leerer Eintrag." : region.Validate();
                if (problem != null)
                {
                    throw new InvalidDataException(problem);
                }
                region.Name = region.Name.Trim();
            }
            return regions;
        }

        // Saves the regions; an empty list removes the file.
        public static void Save(string mapFileName, IEnumerable<ArenaMapRegion> regions)
        {
            List<ArenaMapRegion> list = regions?.ToList() ?? new List<ArenaMapRegion>();
            foreach (ArenaMapRegion region in list)
            {
                string problem = region == null ? "Leerer Eintrag." : region.Validate();
                if (problem != null)
                {
                    throw new InvalidDataException(problem);
                }
                region.Name = region.Name.Trim();
            }

            string path = GetPath(mapFileName);
            if (list.Count == 0)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                return;
            }
            File.WriteAllText(path, JsonSerializer.Serialize(list, JsonOptions));
        }

        // Region names of all maps (choices in the enemy editor), sorted.
        public static IReadOnlyList<string> GetAllRegionNames()
        {
            SortedSet<string> names = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (string path in Directory.GetFiles(RegionsDirectory, "*" + FileSuffix))
                {
                    try
                    {
                        string map = Path.GetFileName(path);
                        map = map.Substring(0, map.Length - FileSuffix.Length);
                        foreach (ArenaMapRegion region in Load(map))
                        {
                            names.Add(region.Name);
                        }
                    }
                    catch (Exception exception) when (exception is JsonException
                        || exception is InvalidDataException || exception is IOException)
                    {
                        // A broken file contributes no names; the regions tab reports it.
                    }
                }
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException)
            {
            }
            return names.ToArray();
        }
    }
}
