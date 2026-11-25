using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // Location of "Classes/10_Gruppe/Tower Defense/Assets". The PNGs are plain files in our group folder (no
    // Resource entry in OOPGames.csproj, which belongs to all groups). The compiler puts the path of this source
    // file into sourceFile, so the folder is found next to it. Fallback: search upwards from the program folder.
    static class G10_TowerDefenseAssets
    {
        public static readonly string Folder = FindFolder();

        public static string PathOf(string relativePath)
        {
            string path = Path.Combine(Folder, relativePath);
            if (!File.Exists(path))
                throw new FileNotFoundException("Tower Defense asset not found. Expected in " + Folder, path);
            return path;
        }

        static string FindFolder([CallerFilePath] string sourceFile = "")
        {
            string relative = Path.Combine("Classes", "10_Gruppe", "Tower Defense", "Assets");
            if (!string.IsNullOrEmpty(sourceFile))
            {
                string folder = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(sourceFile), "..", "..", "..", relative));
                if (Directory.Exists(folder)) return folder;
            }

            for (DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
            {
                string folder = Path.Combine(directory.FullName, relative);
                if (Directory.Exists(folder)) return folder;
            }
            return Path.Combine(AppContext.BaseDirectory, relative);
        }
    }

    // One PNG with cached sub images. Coordinates are image pixels.
    sealed class G10_TowerDefenseImage
    {
        readonly BitmapImage _Image;
        readonly Dictionary<Int32Rect, BitmapSource> _Regions = new Dictionary<Int32Rect, BitmapSource>();

        public G10_TowerDefenseImage(string relativePath)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(G10_TowerDefenseAssets.PathOf(relativePath), UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            _Image = image;
        }

        public BitmapSource Whole { get { return _Image; } }
        public int PixelWidth { get { return _Image.PixelWidth; } }
        public int PixelHeight { get { return _Image.PixelHeight; } }

        public BitmapSource Region(int x, int y, int width, int height)
        {
            Int32Rect rect = new Int32Rect(x, y, width, height);
            BitmapSource region;
            if (!_Regions.TryGetValue(rect, out region))
            {
                CroppedBitmap cropped = new CroppedBitmap(_Image, rect);
                cropped.Freeze();
                region = cropped;
                _Regions.Add(rect, region);
            }
            return region;
        }
    }

    sealed class G10_TowerDefenseAnimation
    {
        public int Row;
        public int Frames;
        public double Fps;
        public bool Loop;

        public double Duration { get { return Frames / Fps; } }
    }

    // A sprite sheet from sprites.json: rows = animations, columns = frames. All values in 1080p pixels.
    sealed class G10_TowerDefenseSprite
    {
        readonly Dictionary<string, G10_TowerDefenseAnimation> _Animations = new Dictionary<string, G10_TowerDefenseAnimation>();
        G10_TowerDefenseImage _Sheet;

        public G10_TowerDefenseSprite(string key, string file, int frameWidth, int frameHeight, Point pivot, Point? muzzle)
        {
            Key = key;
            File = file;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            Pivot = pivot;
            Muzzle = muzzle;
        }

        public string Key { get; private set; }
        public string File { get; private set; }
        public int FrameWidth { get; private set; }
        public int FrameHeight { get; private set; }
        // Foot point inside a frame.
        public Point Pivot { get; private set; }
        // Towers only: where projectiles leave the sprite (inside a frame).
        public Point? Muzzle { get; private set; }

        public void AddAnimation(string name, G10_TowerDefenseAnimation animation) { _Animations[name] = animation; }

        public bool HasAnimation(string name) { return _Animations.ContainsKey(name); }

        public G10_TowerDefenseAnimation Animation(string name)
        {
            G10_TowerDefenseAnimation animation;
            if (!_Animations.TryGetValue(name, out animation))
                throw new ArgumentException("Sprite " + Key + " has no animation " + name + ".", nameof(name));
            return animation;
        }

        public BitmapSource Frame(string animation, int frame)
        {
            G10_TowerDefenseAnimation a = Animation(animation);
            if (_Sheet == null) _Sheet = new G10_TowerDefenseImage(File);
            int column = Math.Max(0, Math.Min(a.Frames - 1, frame));
            return _Sheet.Region(column * FrameWidth, a.Row * FrameHeight, FrameWidth, FrameHeight);
        }

        // Frame of an animation after `seconds`: loops wrap around, other animations stop on their last frame.
        public BitmapSource FrameAt(string animation, double seconds)
        {
            G10_TowerDefenseAnimation a = Animation(animation);
            int frame = (int)Math.Floor(Math.Max(0, seconds) * a.Fps);
            frame = a.Loop ? frame % a.Frames : Math.Min(a.Frames - 1, frame);
            return Frame(animation, frame);
        }
    }

    enum G10_TowerDefenseIcon
    {
        Coin, Heart, Wave, Skull, Physical, Fire, Ice, Earth, Storm, Poison,
        Upgrade, Sell, Cursor, Star, Sword, Range, Speed, Lock
    }

    // Central access to the sprites in Tower Defense/Assets (described by sprites.json, see LIESMICH_Sprites.txt).
    static class G10_TowerDefenseSprites
    {
        // Device pixels per logical UI unit: the game draws 320x180 UI units on a 1920x1080 surface.
        public const double PixelsPerUnit = 6;

        static readonly Dictionary<string, G10_TowerDefenseSprite> _Sprites = LoadSprites();
        static readonly G10_TowerDefenseImage _Icons = new G10_TowerDefenseImage("ui_icons.png");
        static readonly Dictionary<string, G10_TowerDefenseImage> _MapLayers = new Dictionary<string, G10_TowerDefenseImage>();

        // Game names (Tower.Name / Enemy.Name) to sprite keys.
        static readonly Dictionary<string, string> _Keys = new Dictionary<string, string>
        {
            { "Physical Archer", "tuerme/physical_archer" }, { "Poison Archer", "tuerme/poison_archer" },
            { "Fire Mage", "tuerme/fire_mage" }, { "Ice Mage", "tuerme/ice_mage" }, { "Earth Mage", "tuerme/earth_mage" },
            { "Strom Mage", "tuerme/storm_mage" }, { "Flamethrower", "tuerme/flamethrower" },
            { "Ice Area Tower", "tuerme/ice_area_tower" },

            { "Goblin", "gegner/goblin" }, { "Goblin Archer", "gegner/goblin_archer" }, { "Goblin Shaman", "gegner/goblin_shaman" },
            { "Orc", "gegner/orc" }, { "Orc Berserker", "gegner/orc_berserker" }, { "Orc Warlord", "gegner/orc_warlord" },
            { "Skeleton", "gegner/skeleton" }, { "Skeleton Knight", "gegner/skeleton_knight" }, { "Troll", "gegner/troll" },
            { "Stone Golem", "gegner/stone_golem" }, { "Fire Elemental", "gegner/fire_element" },
            { "Ice Elemental", "gegner/ice_element" }, { "Storm Elemental", "gegner/storm_element" },
            { "Earth Elemental", "gegner/earth_element" }, { "Dark Elf", "gegner/dark_elf" }, { "Vampire", "gegner/vampire" },
            { "Wyvern", "gegner/wyvern" }, { "Fire Golem", "gegner/fire_golem" }, { "Dragon", "gegner/dragon" },
            { "Arcane Titan", "gegner/arcane_titan" },

            { "Fire Dragon", "minibosse/fire_dragon" }, { "Skeleton Mage", "minibosse/skeleton_mage" },
            { "Dark Elf King", "minibosse/dark_elf_king" }, { "All Elements Golem", "minibosse/all_elements_golem" },
            { "Goblin Lord", "minibosse/goblin_lord" },

            { "Skeleton King", "bosse/skeletonking" }, { "Orc Lord", "bosse/orc_lord" },
            { "Elemental God", "bosse/elemental_god" }, { "Vampire Lord", "bosse/vampire_lord" },
            { "Elder Dragon", "bosse/elder_dragon" }
        };

        public static G10_TowerDefenseSprite Get(string key)
        {
            G10_TowerDefenseSprite sprite;
            if (!_Sprites.TryGetValue(key, out sprite)) throw new ArgumentException("Unknown sprite: " + key, nameof(key));
            return sprite;
        }

        // Tower, enemy, mini boss or boss by its name in the game.
        public static G10_TowerDefenseSprite ForName(string name)
        {
            string key;
            if (!_Keys.TryGetValue(name, out key)) throw new ArgumentException("No sprite is configured for " + name + ".", nameof(name));
            return Get(key);
        }

        public static bool IsBoss(string name)
        {
            string key;
            return _Keys.TryGetValue(name, out key) && (key.StartsWith("bosse/") || key.StartsWith("minibosse/"));
        }

        public static G10_TowerDefenseSprite Castle { get { return Get("hauptgebaeude/schloss"); } }

        // intakt > 66 %, beschaedigt 33 - 66 %, ruine below.
        public static string CastleAnimation(int healthPercent)
        {
            return healthPercent > 66 ? "intakt" : healthPercent > 33 ? "beschaedigt" : "ruine";
        }

        public static G10_TowerDefenseSprite Effect(string name) { return Get("effekte/" + name); }

        // Map layer of a map from Assets/karten: "boden", "deko", "bruecken" or "komplett" (1920x1056 each).
        public static BitmapSource MapLayer(string mapId, string layer)
        {
            return MapImage(mapId, layer).Whole;
        }

        public static BitmapSource MapRegion(string mapId, string layer, int x, int y, int width, int height)
        {
            return MapImage(mapId, layer).Region(x, y, width, height);
        }

        static G10_TowerDefenseImage MapImage(string mapId, string layer)
        {
            string file = Path.Combine("karten", mapId + "_" + layer + ".png");
            G10_TowerDefenseImage image;
            if (!_MapLayers.TryGetValue(file, out image))
            {
                image = new G10_TowerDefenseImage(file);
                _MapLayers.Add(file, image);
            }
            return image;
        }

        // UI icons (16x16 pixel art stored with a 4x scale, 64x64 image pixels per icon).
        public static BitmapSource Icon(G10_TowerDefenseIcon icon)
        {
            return _Icons.Region((int)icon * 64, 0, 64, 64);
        }

        public static BitmapSource DamageIcon(G10_DamageType damageType)
        {
            return Icon(G10_TowerDefenseIcon.Physical + (int)damageType);
        }

        static Dictionary<string, G10_TowerDefenseSprite> LoadSprites()
        {
            Dictionary<string, G10_TowerDefenseSprite> sprites = new Dictionary<string, G10_TowerDefenseSprite>();
            using (JsonDocument document = JsonDocument.Parse(System.IO.File.ReadAllText(G10_TowerDefenseAssets.PathOf("sprites.json"))))
            {
                foreach (JsonProperty entry in document.RootElement.GetProperty("sprites").EnumerateObject())
                {
                    JsonElement s = entry.Value;
                    JsonElement pivot = s.GetProperty("pivot");
                    Point? muzzle = null;
                    JsonElement origin;
                    if (s.TryGetProperty("schuss_ursprung", out origin))
                        muzzle = new Point(origin[0].GetDouble(), origin[1].GetDouble());
                    G10_TowerDefenseSprite sprite = new G10_TowerDefenseSprite(entry.Name, s.GetProperty("file").GetString(),
                        s.GetProperty("frame_w").GetInt32(), s.GetProperty("frame_h").GetInt32(),
                        new Point(pivot[0].GetDouble(), pivot[1].GetDouble()), muzzle);
                    foreach (JsonProperty animation in s.GetProperty("animations").EnumerateObject())
                    {
                        sprite.AddAnimation(animation.Name, new G10_TowerDefenseAnimation
                        {
                            Row = animation.Value.GetProperty("row").GetInt32(),
                            Frames = animation.Value.GetProperty("frames").GetInt32(),
                            Fps = animation.Value.GetProperty("fps").GetDouble(),
                            Loop = animation.Value.GetProperty("loop").GetBoolean()
                        });
                    }
                    sprites.Add(entry.Name, sprite);
                }
            }
            return sprites;
        }
    }
}
