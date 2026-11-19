using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // Loads a PNG from "Classes/10_Gruppe/Tower Defense/Assets". All sheets store their pixel art with a 4x scale,
    // so coordinates passed to Region are in art pixels (the same unit the game draws in).
    sealed class TowerDefenseSpriteSheet
    {
        const int SourceScale = 4;
        static readonly string AssetFolder = FindAssetFolder();
        readonly BitmapImage _Image;
        readonly Dictionary<Int32Rect, BitmapSource> _Regions = new Dictionary<Int32Rect, BitmapSource>();

        public TowerDefenseSpriteSheet(string fileName)
        {
            string path = Path.Combine(AssetFolder, fileName);
            if (!File.Exists(path))
                throw new FileNotFoundException("Tower Defense graphic not found. Expected in " + AssetFolder, path);

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            _Image = image;
        }

        // The PNGs are plain files in our group folder (no Resource entry in OOPGames.csproj, which belongs to all
        // groups). The compiler puts the path of this source file into sourceFile, so the folder is found next to it.
        // Fallback, e.g. when the source path is not available: search upwards from the program folder.
        static string FindAssetFolder([CallerFilePath] string sourceFile = "")
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

        public BitmapSource Region(int x, int y, int width, int height)
        {
            Int32Rect rect = new Int32Rect(x * SourceScale, y * SourceScale, width * SourceScale, height * SourceScale);
            BitmapSource frame;
            if (!_Regions.TryGetValue(rect, out frame))
            {
                CroppedBitmap cropped = new CroppedBitmap(_Image, rect);
                cropped.Freeze();
                frame = cropped;
                _Regions.Add(rect, frame);
            }
            return frame;
        }

        public BitmapSource Cell(int column, int row, int cellWidth, int cellHeight)
        {
            return Region(column * cellWidth, row * cellHeight, cellWidth, cellHeight);
        }
    }

    enum TowerDefenseIcon
    {
        Coin, Heart, Wave, Skull, Physical, Fire, Ice, Earth, Storm, Poison,
        Upgrade, Sell, Cursor, Star, Sword, Range, Speed, Lock
    }

    // Central access to all Tower Defense sprite sheets. The sheet layouts are described in
    // Tower Defense/SpriteGenerator/README.md.
    static class TowerDefenseSprites
    {
        public const int EnemySize = 16;
        public const int BossSize = 32;
        public const int MapImageWidth = 320;
        public const int MapImageHeight = 155;

        static readonly string[] _TowerNames =
        {
            "Physical Archer", "Poison Archer", "Fire Mage", "Ice Mage",
            "Earth Mage", "Strom Mage", "Flamethrower", "Ice Area Tower"
        };

        static readonly string[] _EnemyNames =
        {
            "Goblin", "Goblin Archer", "Goblin Shaman", "Orc", "Orc Berserker", "Orc Warlord",
            "Skeleton", "Skeleton Knight", "Troll", "Stone Golem", "Fire Elemental", "Ice Elemental",
            "Storm Elemental", "Earth Elemental", "Dark Elf", "Vampire", "Wyvern", "Fire Golem",
            "Dragon", "Arcane Titan"
        };

        static readonly string[] _BossNames =
        {
            "Skeleton King", "Orc Lord", "Elemental God", "Vampire Lord", "Elder Dragon",
            "Fire Dragon", "Skeleton Mage", "Dark Elf King", "All Elements Golem", "Goblin Lord"
        };

        static readonly TowerDefenseSpriteSheet _Towers = new TowerDefenseSpriteSheet("tower_spritesheet.png");
        static readonly TowerDefenseSpriteSheet _Enemies = new TowerDefenseSpriteSheet("enemy_spritesheet.png");
        static readonly TowerDefenseSpriteSheet _Bosses = new TowerDefenseSpriteSheet("boss_spritesheet.png");
        static readonly TowerDefenseSpriteSheet _Icons = new TowerDefenseSpriteSheet("ui_icons.png");
        static readonly TowerDefenseSpriteSheet _MainBuilding = new TowerDefenseSpriteSheet("main_building.png");
        static readonly TowerDefenseSpriteSheet _Scenery = new TowerDefenseSpriteSheet("scenery_sprites.png");
        static readonly Dictionary<string, TowerDefenseSpriteSheet> _Maps = new Dictionary<string, TowerDefenseSpriteSheet>();

        public static IList<string> EnemyNames { get { return _EnemyNames; } }
        public static IList<string> BossNames { get { return _BossNames; } }

        // 32x48, the tower spot lies at (16, 43) inside the frame.
        public static BitmapSource Tower(string towerName)
        {
            int index = Array.IndexOf(_TowerNames, towerName);
            if (index < 0) throw new ArgumentException("No sprite is configured for this tower.", nameof(towerName));
            return _Towers.Cell(index, 0, 32, 48);
        }

        // 16x16, two walking frames. The feet stand on row 13.
        public static BitmapSource Enemy(string enemyName, int frame)
        {
            int index = Array.IndexOf(_EnemyNames, enemyName);
            if (index < 0) throw new ArgumentException("No sprite is configured for this enemy.", nameof(enemyName));
            return _Enemies.Cell(index, frame & 1, EnemySize, EnemySize);
        }

        // 32x32, two walking frames. The feet stand on row 28.
        public static BitmapSource Boss(string bossName, int frame)
        {
            int index = Array.IndexOf(_BossNames, bossName);
            if (index < 0) throw new ArgumentException("No sprite is configured for this boss.", nameof(bossName));
            return _Bosses.Cell(index, frame & 1, BossSize, BossSize);
        }

        public static bool IsBoss(string name)
        {
            return Array.IndexOf(_BossNames, name) >= 0;
        }

        // 16x16
        public static BitmapSource Icon(TowerDefenseIcon icon)
        {
            return _Icons.Cell((int)icon, 0, 16, 16);
        }

        public static BitmapSource DamageIcon(DamageType damageType)
        {
            return Icon(TowerDefenseIcon.Physical + (int)damageType);
        }

        // 64x48, stage 0 = intact, 1 = damaged, 2 = ruined. The gate threshold lies at (32, 44).
        public static BitmapSource MainBuilding(int stage)
        {
            return _MainBuilding.Cell(Math.Max(0, Math.Min(2, stage)), 0, 64, 48);
        }

        // 32x32, the tower spot lies at (16, 24).
        public static BitmapSource Pedestal()
        {
            return _Scenery.Region(64, 32, 32, 32);
        }

        // Pre-composed map background (320x155). Row 0 lies two pixels above map coordinate y = 0.
        public static BitmapSource Map(string fileName)
        {
            return MapRegion(fileName, 0, 0, MapImageWidth, MapImageHeight);
        }

        // Part of a map background in image pixels (image row = map y + 2).
        public static BitmapSource MapRegion(string fileName, int x, int y, int width, int height)
        {
            TowerDefenseSpriteSheet sheet;
            if (!_Maps.TryGetValue(fileName, out sheet))
            {
                sheet = new TowerDefenseSpriteSheet(fileName);
                _Maps.Add(fileName, sheet);
            }
            return sheet.Region(x, y, width, height);
        }
    }
}
