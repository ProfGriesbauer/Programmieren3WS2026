using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    internal sealed class ArenaMapDefinition
    {
        public ArenaMapDefinition(string fileName, BitmapSource image, double width, double height)
        {
            FileName = fileName;
            Image = image;
            Width = width;
            Height = height;
        }

        public string FileName { get; }
        public BitmapSource Image { get; }
        public double Width { get; }
        public double Height { get; }
    }

    internal sealed class ArenaMapElement
    {
        public ArenaMapElement(string name, BitmapSource image)
        {
            Name = name;
            Image = image;
        }

        public string Name { get; }
        public BitmapSource Image { get; }
    }

    internal static class ArenaMapAssets
    {
        private const int MinimumMapDimension = 512;
        private const int MaximumMapDimension = 8192;
        private const long MaximumMapPixels = 16777216;
        private const string DefaultMapFileName = "Background1.png";
        private const string SelectionFileName = "ArenaMapSelection.json";

        public static string BackgroundDirectory
        {
            get { return PainterG9.FindAssetDirectory("Background"); }
        }

        public static string ElementDirectory
        {
            get
            {
                return PainterG9.FindAssetDirectory(
                    "Background", "BackgroundElemetns");
            }
        }

        public static ArenaMapDefinition LoadSelectedMap()
        {
            string directory = BackgroundDirectory;
            string fileName = ReadSelectedMapFileName(directory);
            return LoadMap(fileName);
        }

        public static IReadOnlyList<string> GetMapFileNames()
        {
            string directory = BackgroundDirectory;
            return Directory.GetFiles(directory, "*.png", SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public static ArenaMapDefinition LoadMap(string fileName)
        {
            string safeFileName = ValidateMapFileName(fileName);
            string path = Path.Combine(BackgroundDirectory, safeFileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The selected arena map does not exist.", path);
            }

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();

            double width = String.Equals(
                safeFileName, DefaultMapFileName, StringComparison.OrdinalIgnoreCase)
                ? ArenaField.MapWidth
                : image.PixelWidth;
            double height = String.Equals(
                safeFileName, DefaultMapFileName, StringComparison.OrdinalIgnoreCase)
                ? ArenaField.MapHeight
                : image.PixelHeight;
            if (!String.Equals(
                safeFileName, DefaultMapFileName, StringComparison.OrdinalIgnoreCase))
            {
                ValidateDimensions(image.PixelWidth, image.PixelHeight);
            }
            return new ArenaMapDefinition(safeFileName, image, width, height);
        }

        public static void SetDefaultMap(string fileName)
        {
            string safeFileName = ValidateMapFileName(fileName);
            string path = Path.Combine(BackgroundDirectory, safeFileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The selected arena map does not exist.", path);
            }

            string settingsPath = Path.Combine(BackgroundDirectory, SelectionFileName);
            File.WriteAllText(settingsPath, JsonSerializer.Serialize(
                new ArenaMapSelection { FileName = safeFileName },
                new JsonSerializerOptions { WriteIndented = true }));
        }

        public static IReadOnlyList<ArenaMapElement> LoadMapElements()
        {
            string directory = ElementDirectory;
            ArenaSpriteAtlas atlas = ArenaSpriteAtlas.Load(
                Path.Combine(directory, "BackgroundElements.png"),
                Path.Combine(directory, "BackgroundElements.json"));
            return atlas.Sprites;
        }

        public static BitmapSource LoadMapBase()
        {
            return LoadMapBase("BackgroundColour1.png");
        }

        public static IReadOnlyList<string> GetMapBaseFileNames()
        {
            return Directory.GetFiles(
                    ElementDirectory, "BackgroundColour*.png",
                    SearchOption.TopDirectoryOnly)
                .Select(Path.GetFileName)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        public static BitmapSource LoadMapBase(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName)
                || !String.Equals(Path.GetFileName(fileName), fileName,
                    StringComparison.Ordinal)
                || !fileName.StartsWith(
                    "BackgroundColour", StringComparison.OrdinalIgnoreCase)
                || !String.Equals(Path.GetExtension(fileName), ".png",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Choose a BackgroundColour PNG from the map asset folder.",
                    nameof(fileName));
            }
            string path = Path.Combine(ElementDirectory, fileName);
            return LoadBitmap(path);
        }

        public static string SaveMap(
            string name, int width, int height,
            BitmapSource baseImage, IEnumerable<ArenaMapPlacement> placements)
        {
            ValidateDimensions(width, height);
            if (baseImage == null)
            {
                throw new ArgumentNullException(nameof(baseImage));
            }
            if (placements == null)
            {
                throw new ArgumentNullException(nameof(placements));
            }

            string safeName = ValidateMapName(name);
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext drawing = visual.RenderOpen())
            {
                drawing.DrawImage(baseImage, new Rect(0, 0, width, height));
                foreach (ArenaMapPlacement placement in placements)
                {
                    if (placement == null || placement.Image == null)
                    {
                        throw new InvalidDataException(
                            "A map element is missing its sprite image.");
                    }
                    drawing.DrawImage(
                        placement.Image,
                        new Rect(placement.X, placement.Y,
                            placement.Image.PixelWidth, placement.Image.PixelHeight));
                }
            }

            RenderTargetBitmap rendered = new RenderTargetBitmap(
                width, height, 96, 96, PixelFormats.Pbgra32);
            rendered.Render(visual);
            rendered.Freeze();

            string directory = BackgroundDirectory;
            string fileName = safeName + ".png";
            string path = Path.Combine(directory, fileName);
            string temporaryPath = path + ".tmp";
            try
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(rendered));
                using (FileStream stream = new FileStream(
                    temporaryPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    encoder.Save(stream);
                }
                File.Move(temporaryPath, path, true);
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }

            return fileName;
        }

        public static void ValidateDimensions(int width, int height)
        {
            if (width < MinimumMapDimension || height < MinimumMapDimension
                || width > MaximumMapDimension || height > MaximumMapDimension
                || (long)width * height > MaximumMapPixels)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(width),
                    "Map dimensions must be at least 512 px, at most 8192 px per side, "
                    + "and no larger than 16,777,216 total pixels.");
            }
        }

        private static string ReadSelectedMapFileName(string directory)
        {
            string settingsPath = Path.Combine(directory, SelectionFileName);
            if (!File.Exists(settingsPath))
            {
                return DefaultMapFileName;
            }

            ArenaMapSelection selection;
            try
            {
                selection = JsonSerializer.Deserialize<ArenaMapSelection>(
                    File.ReadAllText(settingsPath));
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException(
                    "The arena map selection file contains invalid JSON.", exception);
            }
            if (selection == null || String.IsNullOrWhiteSpace(selection.FileName))
            {
                throw new InvalidDataException(
                    "The arena map selection file does not contain a map file name.");
            }
            return ValidateMapFileName(selection.FileName);
        }

        private static string ValidateMapFileName(string fileName)
        {
            if (String.IsNullOrWhiteSpace(fileName)
                || !String.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal)
                || !String.Equals(Path.GetExtension(fileName), ".png",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException(
                    "Choose a PNG map file from the Background folder.", nameof(fileName));
            }
            return fileName;
        }

        private static string ValidateMapName(string name)
        {
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Enter a name for the map.", nameof(name));
            }

            string fileName = Path.GetFileNameWithoutExtension(name.Trim());
            if (String.IsNullOrWhiteSpace(fileName)
                || fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                || fileName.Contains("/") || fileName.Contains("\\"))
            {
                throw new ArgumentException(
                    "The map name contains invalid file-name characters.", nameof(name));
            }
            return fileName;
        }

        private static BitmapImage LoadBitmap(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The arena map base image was not found.", path);
            }
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }

        private sealed class ArenaMapSelection
        {
            public string FileName { get; set; }
        }
    }

    internal sealed class ArenaMapPlacement
    {
        public ArenaMapPlacement(BitmapSource image, double x, double y)
        {
            Image = image;
            X = x;
            Y = y;
        }

        public BitmapSource Image { get; }
        public double X { get; }
        public double Y { get; }
    }

    internal sealed class ArenaAtlasSprite
    {
        public ArenaAtlasSprite(string name, BitmapSource image)
        {
            Name = name;
            Image = image;
        }

        public string Name { get; }
        public BitmapSource Image { get; }
    }

    internal sealed class ArenaSpriteAtlas
    {
        private readonly Dictionary<string, ArenaAtlasSprite> _sprites =
            new Dictionary<string, ArenaAtlasSprite>(StringComparer.OrdinalIgnoreCase);

        private ArenaSpriteAtlas()
        {
        }

        public IReadOnlyList<ArenaMapElement> Sprites
        {
            get
            {
                return _sprites.Values.Select(sprite =>
                    new ArenaMapElement(sprite.Name, sprite.Image)).ToArray();
            }
        }

        public static ArenaSpriteAtlas Load(string imagePath, string metadataPath)
        {
            BitmapImage sheet = LoadBitmap(imagePath);
            ArenaSpriteAtlas atlas = new ArenaSpriteAtlas();
            using (JsonDocument metadata = JsonDocument.Parse(File.ReadAllText(metadataPath)))
            {
                List<BitmapSource> frames = new List<BitmapSource>();
                foreach (JsonProperty property in metadata.RootElement
                    .GetProperty("frames").EnumerateObject())
                {
                    JsonElement frame = property.Value.GetProperty("frame");
                    int x = frame.GetProperty("x").GetInt32();
                    int y = frame.GetProperty("y").GetInt32();
                    int width = frame.GetProperty("w").GetInt32();
                    int height = frame.GetProperty("h").GetInt32();
                    if (property.Value.GetProperty("rotated").GetBoolean()
                        || x < 0 || y < 0 || width <= 0 || height <= 0
                        || x > sheet.PixelWidth - width
                        || y > sheet.PixelHeight - height)
                    {
                        throw new InvalidDataException(
                            "The sprite atlas contains invalid frame metadata.");
                    }
                    CroppedBitmap cropped = new CroppedBitmap(
                        sheet, new Int32Rect(x, y, width, height));
                    cropped.Freeze();
                    frames.Add(cropped);
                }

                foreach (JsonElement tag in metadata.RootElement
                    .GetProperty("meta").GetProperty("frameTags").EnumerateArray())
                {
                    string name = tag.GetProperty("name").GetString();
                    int from = tag.GetProperty("from").GetInt32();
                    int to = tag.GetProperty("to").GetInt32();
                    if (String.IsNullOrWhiteSpace(name) || from < 0
                        || to < from || to >= frames.Count)
                    {
                        throw new InvalidDataException(
                            "The sprite atlas contains invalid frame tags.");
                    }
                    if (to != from)
                    {
                        throw new InvalidDataException(
                            "Map and skilltree editor sprites must use one-frame tags.");
                    }
                    if (atlas._sprites.ContainsKey(name))
                    {
                        throw new InvalidDataException(
                            "The sprite atlas contains duplicate frame-tag names.");
                    }
                    atlas._sprites.Add(name,
                        new ArenaAtlasSprite(name, frames[from]));
                }
            }
            return atlas;
        }

        public ArenaAtlasSprite GetSprite(string name)
        {
            if (!_sprites.TryGetValue(name, out ArenaAtlasSprite sprite))
            {
                throw new KeyNotFoundException(
                    "The sprite atlas does not contain a sprite named '" + name + "'.");
            }
            return sprite;
        }

        public IReadOnlyList<ArenaAtlasSprite> GetSprites()
        {
            return _sprites.Values.ToArray();
        }

        private static BitmapImage LoadBitmap(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("The sprite atlas image was not found.", path);
            }
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }
    }
}
