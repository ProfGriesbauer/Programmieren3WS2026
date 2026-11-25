using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // Hitboxes that match the textures. The sprite-sheet JSON only describes full, untrimmed
    // frames, so the visible (non-transparent) pixels of a character's clips are measured once
    // from the PNG. The result is the rectangle around everything the character shows in those
    // clips, relative to the frame center and as a fraction of the frame height, so it scales
    // with the size the painter draws the frame at.
    public static class EntityHitboxCatalog
    {
        public const string BoxingKnight = "BoxingKnight";
        public const string Kurt = "Kurt";
        public const string Enemy = "Enemie1";

        // Sprite sheets and the clips that define the visible body. Characters use their idle
        // and walk clips; the enemy uses its idle clips, because its walk frames are stretched
        // motion smears. The paths match the animation profiles in PainterG9.
        private static readonly Dictionary<string, SpriteSource> Sources =
            new Dictionary<string, SpriteSource>(StringComparer.Ordinal)
            {
                {
                    BoxingKnight, new SpriteSource(
                        new[] { "Character", "BoxingKnight", "BoxingKnightSpritesheet.png" },
                        new[] { "Character", "BoxingKnight", "BoxingKnightAnimation.json" },
                        CharacterClips)
                },
                {
                    Kurt, new SpriteSource(
                        new[] { "Character", "Kurt", "Kurt.png" },
                        new[] { "Character", "Kurt", "Kurt.json" },
                        CharacterClips)
                },
                {
                    Enemy, new SpriteSource(
                        new[] { "Enemies", "Enemie1.png" },
                        new[] { "Enemies", "Enemie1.json" },
                        EnemyClips)
                }
            };

        private static string[] EnemyClips
        {
            // Enemie1 names its idle clips without spaces; newer sheets (Frog) with.
            get
            {
                return new[]
                {
                    "IdleFront", "IdleBack", "idleLeft",
                    "Idle Front", "Idle Back", "Idle Left", "Idle Right", "Idle"
                };
            }
        }

        // Sprite sheets of enemy kinds (Assets/Enemies/<name>.png with <name>.json) are
        // measured like Enemie1, from the same idle clips; null when they cannot be read.
        public static Rect? GetEnemyHitboxOffset(string spriteSheetFile, double drawnFrameHeight)
        {
            if (String.IsNullOrWhiteSpace(spriteSheetFile))
            {
                return null;
            }
            string name = Path.GetFileNameWithoutExtension(spriteSheetFile);
            if (String.Equals(name, Enemy, StringComparison.OrdinalIgnoreCase))
            {
                return GetHitboxOffset(Enemy, drawnFrameHeight);
            }

            string key = "Enemies/" + name;
            lock (MeasureLock)
            {
                if (!Sources.ContainsKey(key))
                {
                    Sources.Add(key, new SpriteSource(
                        new[] { "Enemies", name + ".png" },
                        new[] { "Enemies", name + ".json" },
                        EnemyClips));
                }
            }
            return GetHitboxOffset(key, drawnFrameHeight);
        }

        private static string[] CharacterClips
        {
            get
            {
                return new[]
                {
                    "Idle Front", "Idle Back", "Idle Right", "Idle Left",
                    "Walk Forwards", "Walk Backwards", "Walk Right", "Walk Left"
                };
            }
        }

        // Measured visible area per sprite name; null when the assets could not be measured.
        private static readonly Dictionary<string, Rect?> MeasuredFractions =
            new Dictionary<string, Rect?>(StringComparer.Ordinal);
        private static readonly object MeasureLock = new object();

        // Hitbox relative to the entity position in world units, for a frame drawn
        // drawnFrameHeight units tall; null when the sprite is unknown or cannot be read.
        public static Rect? GetHitboxOffset(string spriteName, double drawnFrameHeight)
        {
            if (spriteName == null || drawnFrameHeight <= 0)
            {
                return null;
            }

            Rect? fraction;
            lock (MeasureLock)
            {
                if (!MeasuredFractions.TryGetValue(spriteName, out fraction))
                {
                    fraction = Sources.TryGetValue(spriteName, out SpriteSource source)
                        ? Measure(source)
                        : null;
                    MeasuredFractions.Add(spriteName, fraction);
                }
            }

            if (!fraction.HasValue)
            {
                return null;
            }

            Rect value = fraction.Value;
            return new Rect(
                value.X * drawnFrameHeight, value.Y * drawnFrameHeight,
                value.Width * drawnFrameHeight, value.Height * drawnFrameHeight);
        }

        private static Rect? Measure(SpriteSource source)
        {
            try
            {
                return MeasureVisibleArea(
                    PainterG9.FindAssetPath(source.SheetPath),
                    PainterG9.FindAssetPath(source.MetadataPath),
                    source.Clips);
            }
            catch (Exception exception) when (
                exception is IOException
                || exception is UnauthorizedAccessException
                || exception is JsonException
                || exception is InvalidOperationException
                || exception is KeyNotFoundException
                || exception is NotSupportedException
                || exception is ArgumentException)
            {
                // Without readable assets the entity keeps its default hitbox.
                return null;
            }
        }

        // Union of the non-transparent pixels of all frames of the clips, relative to the
        // frame center and divided by the frame height.
        internal static Rect? MeasureVisibleArea(
            string sheetPath, string metadataPath, IEnumerable<string> clipNames)
        {
            BitmapImage sheet = new BitmapImage();
            sheet.BeginInit();
            sheet.UriSource = new Uri(sheetPath, UriKind.Absolute);
            sheet.CacheOption = BitmapCacheOption.OnLoad;
            sheet.EndInit();
            sheet.Freeze();
            FormatConvertedBitmap pixels = new FormatConvertedBitmap(
                sheet, PixelFormats.Bgra32, null, 0);
            int sheetWidth = pixels.PixelWidth;
            int sheetHeight = pixels.PixelHeight;
            byte[] data = new byte[sheetWidth * sheetHeight * 4];
            pixels.CopyPixels(data, sheetWidth * 4, 0);

            using (JsonDocument metadata = JsonDocument.Parse(File.ReadAllText(metadataPath)))
            {
                List<Int32Rect> frames = new List<Int32Rect>();
                foreach (JsonProperty frame in
                    metadata.RootElement.GetProperty("frames").EnumerateObject())
                {
                    JsonElement bounds = frame.Value.GetProperty("frame");
                    frames.Add(new Int32Rect(
                        bounds.GetProperty("x").GetInt32(), bounds.GetProperty("y").GetInt32(),
                        bounds.GetProperty("w").GetInt32(), bounds.GetProperty("h").GetInt32()));
                }

                HashSet<string> wanted = new HashSet<string>(clipNames, StringComparer.Ordinal);
                int minimumX = int.MaxValue;
                int minimumY = int.MaxValue;
                int maximumX = int.MinValue;
                int maximumY = int.MinValue;
                double frameWidth = 0;
                double frameHeight = 0;
                foreach (JsonElement tag in
                    metadata.RootElement.GetProperty("meta").GetProperty("frameTags").EnumerateArray())
                {
                    if (!wanted.Contains(tag.GetProperty("name").GetString()))
                    {
                        continue;
                    }

                    int first = tag.GetProperty("from").GetInt32();
                    int last = Math.Min(frames.Count - 1, tag.GetProperty("to").GetInt32());
                    for (int index = first; index <= last; index++)
                    {
                        Int32Rect frame = frames[index];
                        frameWidth = frame.Width;
                        frameHeight = frame.Height;
                        for (int y = 0; y < frame.Height; y++)
                        {
                            int sheetY = frame.Y + y;
                            if (sheetY < 0 || sheetY >= sheetHeight)
                            {
                                continue;
                            }
                            for (int x = 0; x < frame.Width; x++)
                            {
                                int sheetX = frame.X + x;
                                if (sheetX < 0 || sheetX >= sheetWidth
                                    || data[(sheetY * sheetWidth + sheetX) * 4 + 3] == 0)
                                {
                                    continue;
                                }
                                minimumX = Math.Min(minimumX, x);
                                minimumY = Math.Min(minimumY, y);
                                maximumX = Math.Max(maximumX, x + 1);
                                maximumY = Math.Max(maximumY, y + 1);
                            }
                        }
                    }
                }

                if (frameHeight <= 0 || maximumX <= minimumX || maximumY <= minimumY)
                {
                    return null;
                }

                return new Rect(
                    (minimumX - frameWidth / 2) / frameHeight,
                    (minimumY - frameHeight / 2) / frameHeight,
                    (maximumX - minimumX) / frameHeight,
                    (maximumY - minimumY) / frameHeight);
            }
        }

        private sealed class SpriteSource
        {
            public SpriteSource(string[] sheetPath, string[] metadataPath, string[] clips)
            {
                SheetPath = sheetPath;
                MetadataPath = metadataPath;
                Clips = clips;
            }

            public string[] SheetPath { get; }
            public string[] MetadataPath { get; }
            public string[] Clips { get; }
        }
    }
}
