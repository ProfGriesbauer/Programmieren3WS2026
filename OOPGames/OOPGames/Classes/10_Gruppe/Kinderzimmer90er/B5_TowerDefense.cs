using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    public enum TowerDefenseDifficulty
    {
        Easy,
        Normal,
        Hard,
        God
    }

    public static class TowerDefenseDifficultyRules
    {
        public static double GetMultiplier(TowerDefenseDifficulty difficulty)
        {
            switch (difficulty)
            {
                case TowerDefenseDifficulty.Easy: return 0.75;
                case TowerDefenseDifficulty.Hard: return 1.35;
                case TowerDefenseDifficulty.God: return 1.8;
                default: return 1.0;
            }
        }

        public static int ScaleBaseValue(int baseValue, TowerDefenseDifficulty difficulty)
        {
            if (baseValue < 0) throw new ArgumentOutOfRangeException(nameof(baseValue));
            return Math.Max(1, (int)Math.Round(baseValue * GetMultiplier(difficulty)));
        }
    }

    public sealed class TowerDefenseMapDefinition
    {
        public TowerDefenseMapDefinition(string name, IList<Point> towerSpots, IList<Point> path)
        {
            Name = name;
            TowerSpots = towerSpots;
            Path = path;
        }

        public string Name { get; private set; }
        public IList<Point> TowerSpots { get; private set; }
        public IList<Point> Path { get; private set; }
    }

    public static class TowerDefenseMaps
    {
        static readonly IList<TowerDefenseMapDefinition> _Maps = new List<TowerDefenseMapDefinition>
        {
            new TowerDefenseMapDefinition(
                "Gruene Wiese",
                new List<Point>
                {
                    new Point(24, 97), new Point(73, 60), new Point(101, 133),
                    new Point(156, 94), new Point(248, 87)
                },
                new List<Point>
                {
                    new Point(-10, 119), new Point(43, 119), new Point(43, 77),
                    new Point(126, 77), new Point(126, 116), new Point(216, 116),
                    new Point(216, 63), new Point(181, 63), new Point(181, 38),
                    new Point(160, 38), new Point(160, 26)
                })
        };

        public static IList<TowerDefenseMapDefinition> Maps { get { return _Maps; } }
    }

    enum TowerDefenseScreen
    {
        MainMenu,
        DifficultySelection,
        MapSelection,
        Map,
        TowerPlacement,
        TowerUpgrade
    }

    public sealed class B5RoomTowerDefenseGame : IB5RoomMiniGame
    {
        const int DesignWidth = 320;
        const int DesignHeight = 180;
        static readonly string[] DifficultyNames = { "Easy", "Normal", "Hard", "God" };
        static readonly string[] TowerNames =
        {
            "Physical Archer", "Fire Mage", "Ice Mage", "Poison Archer",
            "Strom Mage", "Earth Mage", "Flamethrower", "Ice Area Tower"
        };
        static readonly Func<Tower>[] TowerCreators =
        {
            () => new PhysicalArcher(), () => new FireMage(), () => new IceMage(), () => new PoisonArcher(),
            () => new StromMage(), () => new EarthMage(), () => new Flamethrower(), () => new IceAreaTower()
        };
        bool _Exit;
        TowerDefenseScreen _Screen;
        int _MenuSelection;
        int _DifficultySelection;
        int _MapSelection;
        int _SelectedSpot;
        int _TowerSelection;
        int _UpgradeSelection;
        int _Gold;
        readonly Tower[] _Towers = new Tower[5];
        readonly int[] _TowerLevels = new int[5];
        TowerDefensePixelSurface _PixelSurface;
        TowerDefenseDifficulty _Difficulty;
        TowerDefenseMapDefinition _Map;
        string _StatusMessage;

        public string Title { get { return "Tower Defense"; } }
        public int Width { get { return DesignWidth * 2; } }
        public int Height { get { return DesignHeight * 2; } }
        public bool ExitRequested { get { return _Exit; } }
        public int CurrentWave { get { return 1; } }
        public int ExpectedEnemies { get { return 10; } }
        public int Gold { get { return _Gold; } }
        public string Difficulty { get { return DifficultyNames[(int)_Difficulty]; } }
        public string MapName { get { return _Map == null ? "Keine" : _Map.Name; } }
        public int MainBuildingHealth { get { return 100; } }
        public string Status { get { return _Screen == TowerDefenseScreen.Map ? _StatusMessage : "Auswahl"; } }
        public TowerDefenseDifficulty SelectedDifficulty { get { return _Difficulty; } }
        public TowerDefenseMapDefinition SelectedMap { get { return _Map; } }
        public double EnemyStrengthMultiplier { get { return TowerDefenseDifficultyRules.GetMultiplier(_Difficulty); } }

        public void Start()
        {
            _Exit = false;
            _Screen = TowerDefenseScreen.MainMenu;
            _MenuSelection = 0;
            _DifficultySelection = (int)TowerDefenseDifficulty.Normal;
            _MapSelection = 0;
            _SelectedSpot = 0;
            _TowerSelection = 0;
            _UpgradeSelection = 0;
            _Gold = 500;
            Array.Clear(_Towers, 0, _Towers.Length);
            Array.Clear(_TowerLevels, 0, _TowerLevels.Length);
            _Difficulty = TowerDefenseDifficulty.Normal;
            _Map = null;
            _StatusMessage = "Bereit";
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            foreach (Key key in keys)
            {
                if (key == Key.Back)
                {
                    GoBack();
                    continue;
                }

                if (_Screen == TowerDefenseScreen.Map &&
                    (B5RoomKeys.IsLeft(key) || B5RoomKeys.IsRight(key) ||
                     B5RoomKeys.IsUp(key) || B5RoomKeys.IsDown(key)))
                {
                    MoveSpot(B5RoomKeys.IsRight(key) || B5RoomKeys.IsDown(key) ? 1 : -1);
                }
                else if ((_Screen == TowerDefenseScreen.TowerPlacement ||
                    _Screen == TowerDefenseScreen.TowerUpgrade) &&
                    (B5RoomKeys.IsUp(key) || B5RoomKeys.IsDown(key)))
                {
                    MoveMenuSelection(B5RoomKeys.IsDown(key) ? 1 : -1);
                }
                else if (key == Key.Enter || key == Key.Z)
                    ConfirmSelection();
            }
        }

        public void Draw(Canvas canvas)
        {
            if (_PixelSurface == null || !ReferenceEquals(canvas.Tag, _PixelSurface))
            {
                canvas.Children.Clear();
                _PixelSurface = new TowerDefensePixelSurface(DesignWidth, DesignHeight, 2);
                canvas.Tag = _PixelSurface;
                canvas.Children.Add(_PixelSurface.Image);
            }

            _PixelSurface.Begin();
            switch (_Screen)
            {
                case TowerDefenseScreen.MainMenu:
                    DrawMainMenu(canvas);
                    break;
                case TowerDefenseScreen.DifficultySelection:
                    DrawDifficultySelection(canvas);
                    break;
                case TowerDefenseScreen.MapSelection:
                    DrawMapSelection(canvas);
                    break;
                case TowerDefenseScreen.TowerPlacement:
                    DrawMap(canvas);
                    DrawTowerPlacementMenu(canvas);
                    break;
                case TowerDefenseScreen.TowerUpgrade:
                    DrawMap(canvas);
                    DrawTowerUpgradeMenu(canvas);
                    break;
                default:
                    DrawMap(canvas);
                    break;
            }
            _PixelSurface.End();
        }

        void MoveSelection(int direction)
        {
            if (_Screen == TowerDefenseScreen.MainMenu)
                _MenuSelection = (_MenuSelection + direction + 2) % 2;
            else if (_Screen == TowerDefenseScreen.DifficultySelection)
                _DifficultySelection = (_DifficultySelection + direction + DifficultyNames.Length) % DifficultyNames.Length;
            else if (_Screen == TowerDefenseScreen.MapSelection)
                _MapSelection = (_MapSelection + direction + TowerDefenseMaps.Maps.Count) % TowerDefenseMaps.Maps.Count;
        }

        void MoveSpot(int direction)
        {
            _SelectedSpot = (_SelectedSpot + direction + _Map.TowerSpots.Count) % _Map.TowerSpots.Count;
        }

        void MoveMenuSelection(int direction)
        {
            if (_Screen == TowerDefenseScreen.TowerPlacement)
                _TowerSelection = (_TowerSelection + direction + TowerNames.Length) % TowerNames.Length;
            else
                _UpgradeSelection = (_UpgradeSelection + direction + 2) % 2;
        }

        void ConfirmSelection()
        {
            if (_Screen == TowerDefenseScreen.MainMenu)
            {
                if (_MenuSelection == 0) _Screen = TowerDefenseScreen.DifficultySelection;
                else _Exit = true;
            }
            else if (_Screen == TowerDefenseScreen.DifficultySelection)
            {
                _Difficulty = (TowerDefenseDifficulty)_DifficultySelection;
                _Screen = TowerDefenseScreen.MapSelection;
            }
            else if (_Screen == TowerDefenseScreen.MapSelection)
            {
                _Map = TowerDefenseMaps.Maps[_MapSelection];
                _SelectedSpot = 0;
                _StatusMessage = "Bereit";
                _Screen = TowerDefenseScreen.Map;
            }
            else if (_Screen == TowerDefenseScreen.Map)
            {
                _TowerSelection = 0;
                _UpgradeSelection = 0;
                _Screen = _Towers[_SelectedSpot] == null
                    ? TowerDefenseScreen.TowerPlacement
                    : TowerDefenseScreen.TowerUpgrade;
            }
            else if (_Screen == TowerDefenseScreen.TowerPlacement)
            {
                BuildTower();
            }
            else if (_Screen == TowerDefenseScreen.TowerUpgrade)
            {
                if (_UpgradeSelection == 0) UpgradeTower();
                else SellTower();
            }
        }

        void GoBack()
        {
            if (_Screen == TowerDefenseScreen.DifficultySelection) _Screen = TowerDefenseScreen.MainMenu;
            else if (_Screen == TowerDefenseScreen.MapSelection) _Screen = TowerDefenseScreen.DifficultySelection;
            else if (_Screen == TowerDefenseScreen.Map) _Screen = TowerDefenseScreen.MapSelection;
            else if (_Screen == TowerDefenseScreen.TowerPlacement || _Screen == TowerDefenseScreen.TowerUpgrade)
                _Screen = TowerDefenseScreen.Map;
        }

        void BuildTower()
        {
            Tower tower = TowerCreators[_TowerSelection]();
            if (_Gold < tower.Cost)
            {
                _StatusMessage = "Zu wenig Gold";
                return;
            }

            _Gold -= tower.Cost;
            _Towers[_SelectedSpot] = tower;
            _TowerLevels[_SelectedSpot] = 1;
            _StatusMessage = tower.Name + " gebaut";
            _Screen = TowerDefenseScreen.Map;
        }

        void UpgradeTower()
        {
            Tower tower = _Towers[_SelectedSpot];
            int upgradeCost = tower.Cost / 2 * _TowerLevels[_SelectedSpot];
            if (_Gold < upgradeCost)
            {
                _StatusMessage = "Zu wenig Gold";
                return;
            }

            _Gold -= upgradeCost;
            _TowerLevels[_SelectedSpot]++;
            _StatusMessage = tower.Name + " verbessert";
            _Screen = TowerDefenseScreen.Map;
        }

        void SellTower()
        {
            Tower tower = _Towers[_SelectedSpot];
            _Gold += tower.Cost / 2;
            _Towers[_SelectedSpot] = null;
            _TowerLevels[_SelectedSpot] = 0;
            _StatusMessage = "Turm verkauft";
            _Screen = TowerDefenseScreen.Map;
        }

        void DrawMainMenu(Canvas canvas)
        {
            DrawBackground(canvas, Color.FromRgb(13, 22, 42));
            DrawTitle(canvas, "TOWER DEFENSE", 56);
            B5RoomDraw.Text(canvas, "BAUE TÜRME. HALTE DIE WELLEN AUF.", 48, 78, 8, Color.FromRgb(169, 191, 232));
            DrawMenuEntry(canvas, "NEUES SPIEL", 0, 105);
            DrawMenuEntry(canvas, "ZURUECK", 1, 128);
            B5RoomDraw.Text(canvas, "PFEILE / WASD  AUSWAEHLEN     ENTER / Z  BESTAETIGEN", 28, 163, 7, Color.FromRgb(133, 154, 193));
        }

        void DrawDifficultySelection(Canvas canvas)
        {
            DrawBackground(canvas, Color.FromRgb(16, 29, 47));
            DrawTitle(canvas, "SCHWIERIGKEIT", 30);
            B5RoomDraw.Text(canvas, "GEGNERWERTE WERDEN MIT DEM BASISWERT SKALIERT", 42, 63, 7, Color.FromRgb(164, 184, 216));
            for (int i = 0; i < DifficultyNames.Length; i++)
            {
                DrawMenuEntry(canvas, DifficultyNames[i], i, 83 + i * 19);
                B5RoomDraw.Text(canvas, GetMultiplierText((TowerDefenseDifficulty)i), 190, 86 + i * 19, 7, Color.FromRgb(190, 203, 224));
            }
        }

        void DrawMapSelection(Canvas canvas)
        {
            DrawBackground(canvas, Color.FromRgb(18, 40, 35));
            DrawTitle(canvas, "KARTE AUSWAEHLEN", 30);
            TowerDefenseMapDefinition map = TowerDefenseMaps.Maps[_MapSelection];
            DrawMapPreview(canvas, map, 43);
            B5RoomDraw.Text(canvas, map.Name.ToUpperInvariant(), 116, 145, 10, Color.FromRgb(242, 222, 144));
            B5RoomDraw.Text(canvas, "ENTER / Z  KARTE STARTEN", 102, 164, 7, Color.FromRgb(190, 215, 187));
        }

        void DrawMap(Canvas canvas)
        {
            DrawBackground(canvas, Color.FromRgb(104, 157, 59));
            DrawHud(canvas);
            DrawMapPreview(canvas, _Map, 27);
            DrawTowerMarker(canvas);
            B5RoomDraw.Text(canvas, "ENTER: TURM / UPGRADE     BACKSPACE: ZURUECK", 72, 171, 7, Color.FromRgb(238, 242, 188));
        }

        void DrawTowerPlacementMenu(Canvas canvas)
        {
            DrawMenuPanel(canvas, "TURM BAUEN");
            for (int i = 0; i < TowerNames.Length; i++)
            {
                Tower tower = TowerCreators[i]();
                double x = i < 4 ? 42 : 178;
                double y = 55 + (i % 4) * 21;
                bool selected = i == _TowerSelection;
                DrawPanelEntry(canvas, TowerNames[i], "G" + tower.Cost, x, y, selected);
            }
            B5RoomDraw.Text(canvas, "BACKSPACE  ZURUECK", 105, 163, 7, Color.FromRgb(190, 215, 187));
        }

        void DrawTowerUpgradeMenu(Canvas canvas)
        {
            DrawMenuPanel(canvas, "TURM VERWALTEN");
            Tower tower = _Towers[_SelectedSpot];
            int upgradeCost = tower.Cost / 2 * _TowerLevels[_SelectedSpot];
            B5RoomDraw.Text(canvas, tower.Name.ToUpperInvariant() + "  LV " + _TowerLevels[_SelectedSpot],
                72, 58, 9, Color.FromRgb(242, 222, 144));
            DrawPanelEntry(canvas, "UPGRADE", "G" + upgradeCost, 78, 82, _UpgradeSelection == 0);
            DrawPanelEntry(canvas, "VERKAUFEN", "G" + tower.Cost / 2, 78, 106, _UpgradeSelection == 1);
            B5RoomDraw.Text(canvas, "BACKSPACE  ZURUECK", 105, 163, 7, Color.FromRgb(190, 215, 187));
        }

        static void DrawMenuPanel(Canvas canvas, string title)
        {
            B5RoomDraw.Rect(canvas, 28, 31, 264, 133, Color.FromRgb(16, 29, 47));
            B5RoomDraw.Rect(canvas, 30, 33, 260, 129, Color.FromRgb(26, 48, 86));
            B5RoomDraw.Rect(canvas, 30, 33, 260, 2, Color.FromRgb(111, 177, 201));
            B5RoomDraw.Rect(canvas, 30, 160, 260, 2, Color.FromRgb(50, 81, 127));
            B5RoomDraw.Rect(canvas, 30, 33, 2, 129, Color.FromRgb(73, 113, 157));
            B5RoomDraw.Rect(canvas, 288, 33, 2, 129, Color.FromRgb(14, 25, 50));
            B5RoomDraw.Text(canvas, title, 111, 40, 12, Color.FromRgb(244, 221, 143));
        }

        static void DrawPanelEntry(Canvas canvas, string name, string cost, double x, double y, bool selected)
        {
            if (selected) B5RoomDraw.Rect(canvas, x - 5, y - 3, 112, 16, Color.FromRgb(50, 93, 177));
            B5RoomDraw.Text(canvas, (selected ? "> " : "  ") + name, x, y, 8,
                selected ? Colors.White : Color.FromRgb(184, 199, 224));
            B5RoomDraw.Text(canvas, cost, x + 84, y, 7, Color.FromRgb(255, 220, 110));
        }

        void DrawTowerMarker(Canvas canvas)
        {
            if (_Map == null) return;
            Point spot = _Map.TowerSpots[_SelectedSpot];
            Color marker = _Towers[_SelectedSpot] == null
                ? Color.FromRgb(105, 190, 255)
                : Color.FromRgb(255, 218, 91);
            B5RoomDraw.Rect(canvas, spot.X - 17, spot.Y + 25 - 19, 3, 9, marker);
            B5RoomDraw.Rect(canvas, spot.X + 14, spot.Y + 25 - 19, 3, 9, marker);
            B5RoomDraw.Rect(canvas, spot.X - 17, spot.Y + 25 + 10, 3, 9, marker);
            B5RoomDraw.Rect(canvas, spot.X + 14, spot.Y + 25 + 10, 3, 9, marker);
            B5RoomDraw.Text(canvas, (_SelectedSpot + 1).ToString(), spot.X - 3, spot.Y + 25 - 29, 7, marker);
        }

        void DrawMapPreview(Canvas canvas, TowerDefenseMapDefinition map, double top)
        {
            if (_Screen == TowerDefenseScreen.Map)
                DrawMapScenery(canvas, top);

            Color pathColor = Color.FromRgb(151, 105, 78);
            Color pathEdge = Color.FromRgb(68, 61, 58);
            for (int i = 0; i < map.Path.Count - 1; i++)
            {
                Point from = map.Path[i];
                Point to = map.Path[i + 1];
                if (Math.Abs(from.X - to.X) > Math.Abs(from.Y - to.Y))
                {
                    double x = Math.Min(from.X, to.X);
                    B5RoomDraw.Rect(canvas, x, from.Y + top - 8, Math.Abs(to.X - from.X), 16, pathEdge);
                    B5RoomDraw.Rect(canvas, x, from.Y + top - 5, Math.Abs(to.X - from.X), 10, pathColor);
                    for (double stoneX = x + 5; stoneX < x + Math.Abs(to.X - from.X); stoneX += 12)
                        B5RoomDraw.Rect(canvas, stoneX, from.Y + top - 1, 7, 1, Color.FromRgb(190, 137, 94));
                }
                else
                {
                    double y = Math.Min(from.Y, to.Y);
                    B5RoomDraw.Rect(canvas, from.X - 8, y + top, 16, Math.Abs(to.Y - from.Y), pathEdge);
                    B5RoomDraw.Rect(canvas, from.X - 5, y + top, 10, Math.Abs(to.Y - from.Y), pathColor);
                    for (double stoneY = y + 5; stoneY < y + Math.Abs(to.Y - from.Y); stoneY += 12)
                        B5RoomDraw.Rect(canvas, from.X - 1, stoneY + top, 1, 7, Color.FromRgb(190, 137, 94));
                }
            }

            for (int i = 0; i < map.TowerSpots.Count; i++)
            {
                Point spot = map.TowerSpots[i];
                DrawStonePedestal(canvas, spot.X, spot.Y + top);
                if (_Towers[i] != null && _Screen == TowerDefenseScreen.Map)
                    DrawTowerSymbol(canvas, spot.X, spot.Y + top, _Towers[i], _TowerLevels[i]);
            }

            DrawMainBuilding(canvas, 160, 39 + top);
        }

        static void DrawStonePedestal(Canvas canvas, double x, double y)
        {
            Color darkStone = Color.FromRgb(61, 60, 57);
            Color stone = Color.FromRgb(126, 124, 108);
            Color lightStone = Color.FromRgb(184, 176, 143);
            Color crack = Color.FromRgb(83, 79, 69);
            B5RoomDraw.Rect(canvas, x - 13, y + 6, 26, 6, darkStone);
            B5RoomDraw.Rect(canvas, x - 11, y, 22, 8, stone);
            B5RoomDraw.Rect(canvas, x - 7, y - 3, 14, 4, lightStone);
            B5RoomDraw.Rect(canvas, x - 9, y + 1, 5, 4, lightStone);
            B5RoomDraw.Rect(canvas, x - 3, y + 1, 2, 4, crack);
            B5RoomDraw.Rect(canvas, x + 3, y - 2, 2, 3, crack);
            B5RoomDraw.Rect(canvas, x + 7, y + 3, 3, 1, crack);
        }

        static void DrawTowerSymbol(Canvas canvas, double x, double y, Tower tower, int level)
        {
            TowerDefensePixelSurface surface = canvas.Tag as TowerDefensePixelSurface;
            if (surface == null) throw new InvalidOperationException("Tower sprites require the pixel surface.");
            surface.Sprite(GetTowerSpriteIndex(tower), x - 16, y - 43, 32, 48);
            B5RoomDraw.Rect(canvas, x - 10, y + 2, 20, 2, Color.FromRgb(38, 42, 44));
            for (int pip = 0; pip < Math.Min(level, 5); pip++)
                B5RoomDraw.Rect(canvas, x - 8 + pip * 4, y + 2, 2, 2, Color.FromRgb(246, 202, 76));
        }

        static int GetTowerSpriteIndex(Tower tower)
        {
            switch (tower.Name)
            {
                case "Physical Archer": return 0;
                case "Poison Archer": return 1;
                case "Fire Mage": return 2;
                case "Ice Mage": return 3;
                case "Earth Mage": return 4;
                case "Strom Mage": return 5;
                case "Flamethrower": return 6;
                case "Ice Area Tower": return 7;
                default: throw new ArgumentException("No sprite is configured for this tower.", nameof(tower));
            }
        }

        static void DrawTowerUnit(Canvas canvas, double x, double y, Tower tower)
        {
            if (tower.Name == "Physical Archer" || tower.Name == "Poison Archer")
            {
                Color tunic = tower.Name == "Poison Archer" ? Color.FromRgb(67, 137, 71) : Color.FromRgb(172, 91, 45);
                Color hood = tower.Name == "Poison Archer" ? Color.FromRgb(42, 91, 51) : Color.FromRgb(116, 62, 40);
                B5RoomDraw.Rect(canvas, x - 3, y - 5, 6, 7, tunic);
                B5RoomDraw.Rect(canvas, x - 2, y - 8, 4, 4, Color.FromRgb(216, 164, 109));
                B5RoomDraw.Rect(canvas, x - 3, y - 9, 6, 2, hood);
                    B5RoomDraw.Rect(canvas, x - 4, y - 7, 1, 5, Color.FromRgb(240, 197, 128));
                B5RoomDraw.Rect(canvas, x + 3, y - 5, 2, 10, Color.FromRgb(115, 72, 39));
                B5RoomDraw.Rect(canvas, x + 5, y - 4, 1, 8, Color.FromRgb(216, 168, 93));
                B5RoomDraw.Rect(canvas, x + 6, y - 3, 5, 1, Color.FromRgb(231, 211, 151));
                if (tower.Name == "Poison Archer")
                    B5RoomDraw.Rect(canvas, x - 7, y - 2, 3, 4, Color.FromRgb(151, 78, 185));
            }
            else if (tower.Name == "Fire Mage")
            {
                DrawMage(canvas, x, y, Color.FromRgb(175, 46, 42), Color.FromRgb(255, 143, 45));
                B5RoomDraw.Rect(canvas, x + 5, y - 10, 2, 10, Color.FromRgb(92, 54, 42));
                B5RoomDraw.Rect(canvas, x + 7, y - 13, 4, 5, Color.FromRgb(255, 196, 54));
                B5RoomDraw.Rect(canvas, x + 8, y - 16, 2, 4, Color.FromRgb(255, 92, 36));
            }
            else if (tower.Name == "Ice Mage")
            {
                DrawMage(canvas, x, y, Color.FromRgb(64, 119, 184), Color.FromRgb(123, 221, 239));
                B5RoomDraw.Rect(canvas, x + 5, y - 11, 2, 12, Color.FromRgb(112, 80, 56));
                B5RoomDraw.Rect(canvas, x + 7, y - 15, 5, 5, Color.FromRgb(135, 238, 255));
            }
            else if (tower.Name == "Strom Mage")
            {
                DrawMage(canvas, x, y, Color.FromRgb(112, 65, 171), Color.FromRgb(217, 183, 255));
                B5RoomDraw.Rect(canvas, x + 5, y - 11, 2, 11, Color.FromRgb(95, 78, 58));
                B5RoomDraw.Rect(canvas, x + 7, y - 14, 3, 4, Color.FromRgb(255, 238, 105));
                B5RoomDraw.Rect(canvas, x + 10, y - 10, 3, 2, Color.FromRgb(255, 238, 105));
                B5RoomDraw.Rect(canvas, x + 8, y - 8, 2, 4, Color.FromRgb(255, 238, 105));
            }
            else if (tower.Name == "Earth Mage")
            {
                DrawMage(canvas, x, y, Color.FromRgb(122, 79, 46), Color.FromRgb(203, 164, 91));
                B5RoomDraw.Rect(canvas, x + 5, y - 7, 7, 6, Color.FromRgb(105, 77, 52));
                B5RoomDraw.Rect(canvas, x + 7, y - 10, 5, 4, Color.FromRgb(157, 126, 78));
            }
            else if (tower.Name == "Flamethrower")
            {
                B5RoomDraw.Rect(canvas, x - 4, y - 7, 8, 8, Color.FromRgb(52, 65, 73));
                B5RoomDraw.Rect(canvas, x - 3, y - 10, 5, 4, Color.FromRgb(91, 110, 117));
                B5RoomDraw.Rect(canvas, x + 3, y - 5, 8, 3, Color.FromRgb(68, 72, 75));
                B5RoomDraw.Rect(canvas, x + 10, y - 7, 4, 3, Color.FromRgb(255, 141, 45));
                B5RoomDraw.Rect(canvas, x + 12, y - 10, 3, 3, Color.FromRgb(255, 217, 68));
            }
            else
            {
                B5RoomDraw.Rect(canvas, x - 5, y - 7, 10, 8, Color.FromRgb(73, 123, 177));
                B5RoomDraw.Rect(canvas, x - 3, y - 12, 6, 5, Color.FromRgb(129, 210, 235));
                B5RoomDraw.Rect(canvas, x + 4, y - 5, 8, 3, Color.FromRgb(70, 94, 116));
                B5RoomDraw.Rect(canvas, x + 10, y - 8, 5, 5, Color.FromRgb(152, 238, 255));
            }
        }

        static void DrawMage(Canvas canvas, double x, double y, Color robe, Color accent)
        {
            B5RoomDraw.Rect(canvas, x - 4, y - 5, 8, 7, robe);
            B5RoomDraw.Rect(canvas, x - 2, y - 9, 4, 4, Color.FromRgb(222, 171, 116));
            B5RoomDraw.Rect(canvas, x - 5, y - 11, 10, 3, accent);
            B5RoomDraw.Rect(canvas, x - 1, y - 13, 2, 3, accent);
        }

        static void DrawMainBuilding(Canvas canvas, double x, double y)
        {
            Color stone = Color.FromRgb(137, 133, 119);
            Color stoneLight = Color.FromRgb(190, 181, 148);
            Color stoneDark = Color.FromRgb(65, 63, 59);
            Color roof = Color.FromRgb(91, 61, 58);
            Color wood = Color.FromRgb(70, 43, 38);
            B5RoomDraw.Rect(canvas, x - 34, y + 17, 68, 5, Color.FromRgb(57, 67, 55));
            B5RoomDraw.Rect(canvas, x - 28, y - 9, 56, 27, stoneDark);
            B5RoomDraw.Rect(canvas, x - 23, y - 15, 46, 25, stone);
            B5RoomDraw.Rect(canvas, x - 36, y - 4, 15, 22, stone);
            B5RoomDraw.Rect(canvas, x + 21, y - 4, 15, 22, stone);
            B5RoomDraw.Rect(canvas, x - 39, y - 11, 21, 7, roof);
            B5RoomDraw.Rect(canvas, x + 18, y - 11, 21, 7, roof);
            B5RoomDraw.Rect(canvas, x - 31, y - 16, 62, 7, roof);
            B5RoomDraw.Rect(canvas, x - 25, y - 22, 50, 7, Color.FromRgb(116, 95, 82));
            B5RoomDraw.Rect(canvas, x - 5, y - 5, 10, 23, wood);
            B5RoomDraw.Rect(canvas, x - 2, y + 7, 2, 2, Color.FromRgb(241, 199, 77));
            B5RoomDraw.Rect(canvas, x - 30, y + 1, 8, 7, Color.FromRgb(70, 127, 139));
            B5RoomDraw.Rect(canvas, x + 22, y + 1, 8, 7, Color.FromRgb(70, 127, 139));
            B5RoomDraw.Rect(canvas, x - 31, y - 1, 10, 2, stoneLight);
            B5RoomDraw.Rect(canvas, x + 21, y - 1, 10, 2, stoneLight);
            B5RoomDraw.Rect(canvas, x - 17, y - 7, 3, 3, stoneLight);
            B5RoomDraw.Rect(canvas, x + 12, y - 1, 3, 3, stoneLight);
            B5RoomDraw.Rect(canvas, x - 18, y + 10, 2, 2, stoneDark);
            B5RoomDraw.Rect(canvas, x + 14, y + 10, 2, 2, stoneDark);
            B5RoomDraw.Rect(canvas, x - 2, y - 27, 4, 7, Color.FromRgb(213, 178, 72));
            B5RoomDraw.Rect(canvas, x + 2, y - 27, 7, 2, Color.FromRgb(188, 64, 57));
            B5RoomDraw.Text(canvas, "BASE", x - 13, y + 24, 6, Color.FromRgb(232, 210, 157));
        }

        static void DrawMapScenery(Canvas canvas, double top)
        {
            Color grassDark = Color.FromRgb(68, 103, 48);
            Color grassLight = Color.FromRgb(111, 151, 59);
            Color grassBright = Color.FromRgb(151, 176, 70);
            DrawWater(canvas, 0, top + 5, 62, 24);
            DrawWater(canvas, 242, top + 7, 78, 20);
            for (int i = 0; i < 34; i++)
            {
                double x = 8 + (i * 47) % 304;
                double y = top + 8 + (i * 29) % 125;
                B5RoomDraw.Rect(canvas, x, y, 3, 2, i % 2 == 0 ? grassDark : grassLight);
                if (i % 3 == 0)
                    B5RoomDraw.Rect(canvas, x + 4, y - 2, 2, 4, grassBright);
            }

            B5RoomDraw.Rect(canvas, 0, top + 148, 320, 3, Color.FromRgb(54, 80, 44));
            DrawTree(canvas, 22, top + 39);
            DrawTree(canvas, 273, top + 32);
            DrawTree(canvas, 260, top + 147);
            DrawTree(canvas, 116, top + 132);
            DrawTree(canvas, 178, top + 34);
            DrawHouse(canvas, 18, top + 145, Color.FromRgb(142, 77, 45));
            DrawHouse(canvas, 286, top + 38, Color.FromRgb(113, 69, 48));
            DrawBridge(canvas, 194, top + 89);
            DrawFence(canvas, 65, top + 43, 36);
            DrawFence(canvas, 226, top + 132, 28);
            DrawRock(canvas, 13, top + 70);
            DrawRock(canvas, 279, top + 111);
            DrawRock(canvas, 111, top + 30);
            DrawFlowerPatch(canvas, 86, top + 145);
            DrawFlowerPatch(canvas, 239, top + 37);
        }

        static void DrawWater(Canvas canvas, double x, double y, double width, double height)
        {
            B5RoomDraw.Rect(canvas, x, y, width, height, Color.FromRgb(76, 142, 157));
            B5RoomDraw.Rect(canvas, x, y, width, 3, Color.FromRgb(156, 190, 171));
            B5RoomDraw.Rect(canvas, x + 5, y + 9, 15, 2, Color.FromRgb(103, 177, 174));
            B5RoomDraw.Rect(canvas, x + width - 24, y + height - 7, 18, 2, Color.FromRgb(51, 105, 127));
            B5RoomDraw.Rect(canvas, x + 28, y + 4, 2, 3, Color.FromRgb(190, 212, 178));
        }

        static void DrawBridge(Canvas canvas, double x, double y)
        {
            B5RoomDraw.Rect(canvas, x, y, 35, 9, Color.FromRgb(70, 46, 38));
            B5RoomDraw.Rect(canvas, x + 2, y - 2, 31, 3, Color.FromRgb(143, 88, 57));
            for (int plank = 0; plank < 5; plank++)
                B5RoomDraw.Rect(canvas, x + 4 + plank * 6, y + 2, 3, 6, Color.FromRgb(178, 113, 66));
            B5RoomDraw.Rect(canvas, x - 2, y - 5, 3, 14, Color.FromRgb(58, 45, 39));
            B5RoomDraw.Rect(canvas, x + 34, y - 5, 3, 14, Color.FromRgb(58, 45, 39));
        }

        static void DrawFence(Canvas canvas, double x, double y, double width)
        {
            Color post = Color.FromRgb(75, 50, 39);
            Color rail = Color.FromRgb(146, 88, 54);
            B5RoomDraw.Rect(canvas, x, y, width, 3, rail);
            B5RoomDraw.Rect(canvas, x, y + 8, width, 3, rail);
            for (double postX = x; postX <= x + width; postX += 12)
                B5RoomDraw.Rect(canvas, postX, y - 3, 3, 15, post);
        }

        static void DrawRock(Canvas canvas, double x, double y)
        {
            B5RoomDraw.Rect(canvas, x - 5, y + 2, 10, 4, Color.FromRgb(60, 67, 62));
            B5RoomDraw.Rect(canvas, x - 3, y - 1, 7, 4, Color.FromRgb(119, 121, 105));
            B5RoomDraw.Rect(canvas, x - 1, y - 1, 3, 2, Color.FromRgb(181, 172, 137));
        }

        static void DrawFlowerPatch(Canvas canvas, double x, double y)
        {
            Color stem = Color.FromRgb(58, 105, 45);
            Color flower = Color.FromRgb(225, 203, 111);
            B5RoomDraw.Rect(canvas, x, y, 1, 5, stem);
            B5RoomDraw.Rect(canvas, x + 4, y - 2, 1, 6, stem);
            B5RoomDraw.Rect(canvas, x + 8, y + 1, 1, 4, stem);
            B5RoomDraw.Rect(canvas, x - 1, y - 1, 3, 2, flower);
            B5RoomDraw.Rect(canvas, x + 3, y - 4, 3, 2, Color.FromRgb(238, 127, 91));
            B5RoomDraw.Rect(canvas, x + 7, y, 3, 2, Color.FromRgb(220, 111, 171));
        }

        static void DrawTree(Canvas canvas, double x, double y)
        {
            B5RoomDraw.Rect(canvas, x - 3, y + 10, 6, 12, Color.FromRgb(77, 53, 38));
            B5RoomDraw.Rect(canvas, x - 5, y + 12, 2, 8, Color.FromRgb(124, 76, 43));
            B5RoomDraw.Rect(canvas, x - 10, y - 1, 20, 14, Color.FromRgb(55, 82, 40));
            B5RoomDraw.Rect(canvas, x - 7, y - 7, 14, 10, Color.FromRgb(89, 123, 45));
            B5RoomDraw.Rect(canvas, x - 3, y - 10, 7, 6, Color.FromRgb(139, 163, 59));
            B5RoomDraw.Rect(canvas, x - 8, y + 1, 4, 3, Color.FromRgb(156, 177, 64));
            B5RoomDraw.Rect(canvas, x + 4, y - 4, 3, 3, Color.FromRgb(173, 186, 72));
        }

        static void DrawHouse(Canvas canvas, double x, double y, Color roof)
        {
            B5RoomDraw.Rect(canvas, x - 15, y - 5, 30, 17, Color.FromRgb(161, 151, 122));
            B5RoomDraw.Rect(canvas, x - 18, y - 10, 36, 5, roof);
            B5RoomDraw.Rect(canvas, x - 11, y - 16, 22, 7, roof);
            B5RoomDraw.Rect(canvas, x - 3, y + 3, 6, 9, Color.FromRgb(99, 57, 38));
            B5RoomDraw.Rect(canvas, x - 12, y - 1, 5, 5, Color.FromRgb(91, 147, 153));
            B5RoomDraw.Rect(canvas, x + 7, y - 1, 5, 5, Color.FromRgb(91, 147, 153));
        }

        void DrawHud(Canvas canvas)
        {
            B5RoomDraw.Rect(canvas, 0, 0, DesignWidth, 24, Color.FromRgb(29, 47, 57));
            B5RoomDraw.Rect(canvas, 0, 23, DesignWidth, 2, Color.FromRgb(71, 103, 91));
            DrawHudCell(canvas, 4, "GOLD", Gold.ToString(), 0);
            DrawHudCell(canvas, 82, "LEBEN", MainBuildingHealth + "%", 1);
            DrawHudCell(canvas, 164, "WELLE", CurrentWave.ToString(), 2);
            DrawHudCell(canvas, 242, "FEINDE", ExpectedEnemies.ToString(), 3);
        }

        static void DrawHudCell(Canvas canvas, double x, string label, string value, int icon)
        {
            Color divider = Color.FromRgb(67, 93, 82);
            if (x > 4)
                B5RoomDraw.Rect(canvas, x - 6, 4, 1, 16, divider);

            if (icon == 0) DrawGoldIcon(canvas, x, 6);
            else if (icon == 1) DrawHeartIcon(canvas, x, 6);
            else if (icon == 2) DrawWaveIcon(canvas, x, 6);
            else DrawEnemyIcon(canvas, x, 6);

            B5RoomDraw.Text(canvas, label, x + 17, 3, 6, Color.FromRgb(174, 197, 180));
            B5RoomDraw.Text(canvas, value, x + 17, 10, 9, Color.FromRgb(255, 235, 159));
        }

        static void DrawGoldIcon(Canvas canvas, double x, double y)
        {
            Color gold = Color.FromRgb(247, 191, 55);
            B5RoomDraw.Rect(canvas, x + 5, y, 5, 12, gold);
            B5RoomDraw.Rect(canvas, x + 3, y + 2, 9, 8, gold);
            B5RoomDraw.Rect(canvas, x + 5, y + 3, 5, 6, Color.FromRgb(255, 226, 104));
            B5RoomDraw.Rect(canvas, x + 7, y + 4, 1, 4, Color.FromRgb(194, 126, 35));
        }

        static void DrawHeartIcon(Canvas canvas, double x, double y)
        {
            Color red = Color.FromRgb(220, 67, 68);
            B5RoomDraw.Rect(canvas, x + 3, y + 2, 4, 4, red);
            B5RoomDraw.Rect(canvas, x + 8, y + 2, 4, 4, red);
            B5RoomDraw.Rect(canvas, x + 2, y + 5, 11, 5, red);
            B5RoomDraw.Rect(canvas, x + 5, y + 10, 5, 3, red);
            B5RoomDraw.Rect(canvas, x + 5, y + 3, 2, 2, Color.FromRgb(255, 144, 125));
        }

        static void DrawWaveIcon(Canvas canvas, double x, double y)
        {
            Color flag = Color.FromRgb(92, 178, 197);
            B5RoomDraw.Rect(canvas, x + 4, y, 2, 13, Color.FromRgb(190, 166, 101));
            B5RoomDraw.Rect(canvas, x + 6, y + 1, 7, 5, flag);
            B5RoomDraw.Rect(canvas, x + 8, y + 3, 3, 2, Color.FromRgb(188, 236, 205));
            B5RoomDraw.Rect(canvas, x + 2, y + 12, 8, 2, Color.FromRgb(190, 166, 101));
        }

        static void DrawEnemyIcon(Canvas canvas, double x, double y)
        {
            Color helmet = Color.FromRgb(139, 145, 137);
            B5RoomDraw.Rect(canvas, x + 3, y + 3, 10, 7, helmet);
            B5RoomDraw.Rect(canvas, x + 5, y + 1, 6, 3, helmet);
            B5RoomDraw.Rect(canvas, x + 2, y + 8, 12, 3, Color.FromRgb(75, 82, 77));
            B5RoomDraw.Rect(canvas, x + 5, y + 6, 2, 2, Color.FromRgb(37, 45, 42));
            B5RoomDraw.Rect(canvas, x + 9, y + 6, 2, 2, Color.FromRgb(37, 45, 42));
        }

        static void DrawBackground(Canvas canvas, Color color)
        {
            B5RoomDraw.Rect(canvas, 0, 0, 320, 180, color);
            Color shadow = Color.FromArgb(22, 0, 0, 0);
            for (int y = 8; y < 180; y += 16)
                B5RoomDraw.Rect(canvas, 0, y, 320, 1, shadow);
            for (int x = 8; x < 320; x += 24)
                B5RoomDraw.Rect(canvas, x, 0, 1, 180, shadow);
        }

        static void DrawTitle(Canvas canvas, string title, double y)
        {
            B5RoomDraw.Text(canvas, title, 90, y + 2, 17, Color.FromRgb(91, 61, 58));
            B5RoomDraw.Text(canvas, title, 88, y, 17, Color.FromRgb(244, 221, 143));
            B5RoomDraw.Rect(canvas, 72, y + 23, 176, 2, Color.FromRgb(92, 130, 185));
            B5RoomDraw.Rect(canvas, 92, y + 26, 136, 1, Color.FromRgb(42, 67, 109));
        }

        void DrawMenuEntry(Canvas canvas, string text, int index, double y)
        {
            bool selected = (_Screen == TowerDefenseScreen.MainMenu && _MenuSelection == index) ||
                (_Screen == TowerDefenseScreen.DifficultySelection && _DifficultySelection == index);
            Color background = selected ? Color.FromRgb(50, 93, 177) : Color.FromArgb(0, 0, 0, 0);
            B5RoomDraw.Rect(canvas, 82, y - 2, 156, 16, background);
            if (selected)
            {
                B5RoomDraw.Rect(canvas, 82, y - 2, 2, 16, Color.FromRgb(115, 218, 228));
                B5RoomDraw.Rect(canvas, 236, y - 2, 2, 16, Color.FromRgb(115, 218, 228));
            }
            B5RoomDraw.Text(canvas, (selected ? "> " : "  ") + text, 94, y, 11,
                selected ? Colors.White : Color.FromRgb(184, 199, 224));
        }

        static string GetMultiplierText(TowerDefenseDifficulty difficulty)
        {
            return "x" + TowerDefenseDifficultyRules.GetMultiplier(difficulty).ToString("0.00");
        }
    }
}
