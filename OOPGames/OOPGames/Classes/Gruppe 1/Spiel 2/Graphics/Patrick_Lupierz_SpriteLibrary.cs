using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    /// <summary>
    /// Verwaltet alle Grafiken über eine Sprite-ID (z.B. "player_down_0").
    ///
    /// Austauschbarkeit: Liegt im Ordner "G1Casino_Assets" neben der .exe eine
    /// PNG-Datei mit dem Namen der Sprite-ID (z.B. "player_down_0.png"), wird diese
    /// verwendet. Sonst wird die eingebaute Pixel-Art-Grafik erzeugt.
    /// Die Größe der PNG ist frei (z.B. 90x90 Pixel), sie wird passend skaliert.
    /// </summary>
    public class G1C_SpriteLibrary
    {
        public const string AssetFolderName = "G1Casino_Assets";

        private readonly Dictionary<string, BitmapSource> _cache = new Dictionary<string, BitmapSource>();

        public string AssetFolder { get; }

        public G1C_SpriteLibrary()
            : this(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AssetFolderName))
        {
        }

        public G1C_SpriteLibrary(string assetFolder)
        {
            AssetFolder = assetFolder;
        }

        public BitmapSource Get(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return CreateMissingSprite();
            }

            BitmapSource bitmap;
            if (_cache.TryGetValue(id, out bitmap))
            {
                return bitmap;
            }

            bitmap = LoadFromFile(id) ?? G1C_BuiltInSprites.Create(id) ?? CreateMissingSprite();
            _cache[id] = bitmap;
            return bitmap;
        }

        /// <summary>Verhältnis Höhe/Breite einer Grafik (für unten ausgerichtetes Zeichnen).</summary>
        public double GetAspect(string id)
        {
            BitmapSource bitmap = Get(id);
            return bitmap.PixelWidth > 0 ? bitmap.PixelHeight / (double)bitmap.PixelWidth : 1.0;
        }

        private BitmapSource LoadFromFile(string id)
        {
            try
            {
                string file = Path.Combine(AssetFolder, id + ".png");
                if (!File.Exists(file)) return null;

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(file, UriKind.Absolute);
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch (Exception)
            {
                // Defekte oder gesperrte Datei: auf die eingebaute Grafik zurückfallen
                return null;
            }
        }

        // Magenta/Schwarz-Schachbrett: zeigt deutlich, dass eine Grafik fehlt
        private static BitmapSource CreateMissingSprite()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(8, 8);
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    canvas.Set(x, y, ((x / 2 + y / 2) % 2 == 0) ? Colors.Magenta : Colors.Black);
                }
            }
            return canvas.ToBitmap();
        }
    }
}
