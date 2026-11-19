using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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
        public TowerDefenseMapDefinition(string name, string description, string backgroundFile, Point mainBuilding,
            IList<Point> towerSpots, IList<Point> path)
        {
            Name = name;
            Description = description;
            BackgroundFile = backgroundFile;
            MainBuilding = mainBuilding;
            TowerSpots = towerSpots;
            Path = path;
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        // Pre-rendered map image in Assets/TowerDefense. Path, spots and decorations are baked into it,
        // so the coordinates below must match Tower Defense/SpriteGenerator/Generate-TowerDefenseSprites.ps1.
        public string BackgroundFile { get; private set; }
        // Gate threshold of the main building, where the path ends.
        public Point MainBuilding { get; private set; }
        public IList<Point> TowerSpots { get; private set; }
        public IList<Point> Path { get; private set; }
    }

    public static class TowerDefenseMaps
    {
        // Unlock order. From map to map the path gets shorter and there are fewer pedestals
        // (10, 10, 9, 8), so enemies reach the castle sooner and fewer towers can defend it.
        // Every pedestal reaches the path; must match Maps.All in the sprite generator.
        static readonly IList<TowerDefenseMapDefinition> _Maps = new List<TowerDefenseMapDefinition>
        {
            new TowerDefenseMapDefinition(
                "Grüne Wiese", "LANGER SCHLANGENWEG",
                "map_gruene_wiese.png",
                new Point(286, 70),
                Points(16, 46, 132, 46, 198, 56, 70, 62, 196, 100, 16, 102, 132, 102, 236, 106, 66, 108, 264, 124),
                Points(-10, 128, 40, 128, 40, 14, 100, 14, 100, 128, 164, 128, 164, 14, 228, 14, 228, 90, 286, 90, 286, 70)),
            new TowerDefenseMapDefinition(
                "Herbstwald", "MITTELLANGER WALDWEG",
                "map_herbstwald.png",
                new Point(280, 60),
                Points(124, 54, 232, 58, 42, 74, 74, 76, 126, 98, 170, 102, 228, 110, 258, 110, 76, 120, 198, 136),
                Points(-10, 40, 100, 40, 100, 120, 200, 120, 200, 76, 280, 76, 280, 60)),
            new TowerDefenseMapDefinition(
                "Wüstenpass", "KURZER PASS",
                "map_wuestenpass.png",
                new Point(272, 60),
                Points(160, 58, 190, 58, 224, 58, 46, 82, 90, 82, 144, 110, 232, 110, 44, 134, 80, 134),
                Points(-10, 100, 120, 100, 120, 76, 272, 76, 272, 60)),
            new TowerDefenseMapDefinition(
                "Frostgipfel", "KURZER BERGPFAD",
                "map_frostgipfel.png",
                new Point(196, 80),
                Points(144, 38, 14, 52, 42, 70, 92, 70, 148, 82, 112, 116, 140, 134, 170, 134),
                Points(-10, 36, 120, 36, 120, 100, 196, 100, 196, 80))
        };

        public static IList<TowerDefenseMapDefinition> Maps { get { return _Maps; } }

        static IList<Point> Points(params int[] coordinates)
        {
            List<Point> points = new List<Point>();
            for (int i = 0; i + 1 < coordinates.Length; i += 2)
                points.Add(new Point(coordinates[i], coordinates[i + 1]));
            return points;
        }
    }

    enum TowerDefenseScreen
    {
        MainMenu,
        SlotSelection,
        DifficultySelection,
        MapSelection,
        Map,
        TowerPlacement,
        TowerUpgrade
    }

    // Modal question shown on top of the current screen.
    enum TowerDefenseDialog
    {
        None,
        ResumeRun,
        DeleteSlot,
        Defeat
    }

    // Implements IB5RoomOwnBackKey: Esc / X open the in-game menu instead of closing the game.
    public sealed partial class B5RoomTowerDefenseGame : IB5RoomOwnBackKey
    {
        // Clickable region registered while drawing; the next Update hit-tests mouse input against it.
        sealed class ClickArea
        {
            public Rect Area;
            public Action Hover;
            public Action Click;
        }

        const int DesignWidth = 320;
        const int DesignHeight = 180;
        const int MapTop = 27;
        static readonly string[] DifficultyNames = { "Easy", "Normal", "Hard", "God" };
        static readonly string[] DamageTypeNames = { "PHYSISCH", "FEUER", "EIS", "ERDE", "STURM", "GIFT" };
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
        // Enemies that march across the menu screens.
        static readonly string[] MenuParade =
        {
            "Goblin", "Orc", "Skeleton Knight", "Goblin Shaman", "Troll", "Orc Lord",
            "Dark Elf", "Fire Elemental", "Vampire", "Stone Golem", "Skeleton King", "Wyvern"
        };

        // Stone UI palette (matches the tower sprites).
        static readonly Color Outline = Color.FromRgb(43, 37, 32);
        static readonly Color StoneDark = Color.FromRgb(94, 87, 76);
        static readonly Color Stone = Color.FromRgb(138, 130, 116);
        static readonly Color StoneLight = Color.FromRgb(170, 162, 147);
        static readonly Color StoneHighlight = Color.FromRgb(207, 199, 180);
        static readonly Color PanelInside = Color.FromRgb(52, 45, 40);
        static readonly Color PanelInsideDark = Color.FromRgb(40, 34, 30);
        static readonly Color Selection = Color.FromRgb(112, 82, 48);
        static readonly Color SelectionEdge = Color.FromRgb(234, 178, 52);
        static readonly Color TitleGold = Color.FromRgb(255, 224, 108);
        static readonly Color TextLight = Color.FromRgb(232, 226, 208);
        static readonly Color TextDim = Color.FromRgb(170, 162, 147);
        static readonly Color TextGold = Color.FromRgb(255, 214, 96);

        bool _Exit;
        TowerDefenseScreen _Screen;
        int _MenuSelection;
        int _DifficultySelection;
        int _MapSelection;
        int _SelectedSpot;
        int _TowerSelection;
        int _UpgradeSelection;
        int _Gold;
        int _Tick;
        Tower[] _Towers = new Tower[0];
        TowerUpgrades[] _Upgrades = new TowerUpgrades[0];
        TowerDefensePixelSurface _PixelSurface;
        TowerDefenseDifficulty _Difficulty;
        TowerDefenseMapDefinition _Map;
        int _MapIndex = -1;
        string _StatusMessage;
        string _MapSelectionMessage;
        bool _Paused;
        int _PauseSelection;
        int _SlotSelection;
        int _SlotIndex = -1;
        int _Wave = 1;
        int _MainBuildingHealth = 100;
        TowerDefenseDialog _Dialog;
        int _DialogSelection;
        Point? _LastMouse;
        readonly List<ClickArea> _ClickAreas = new List<ClickArea>();
        TowerDefenseBattle _Battle;
        int _GameSpeed = 1;
        bool _ShowWavePreview;
        string _Banner;
        int _BannerTicks;

        // Seconds simulated per Update call (the room timer ticks every 40 ms).
        const double TickSeconds = 0.04;
        // Castle repair after every beaten wave, in percent of its maximum health.
        const int RepairPerWave = 3;

        public string Title { get { return "Tower Defense"; } }
        // The game logic works in 320x180 logical pixels; the screen is rendered at 1920x1080 (factor 6).
        // Sprites are scaled with nearest neighbour, so higher resolution sprites can replace them later.
        const int PixelScale = 6;
        public int Width { get { return DesignWidth * PixelScale; } }
        public int Height { get { return DesignHeight * PixelScale; } }
        public bool ExitRequested { get { return _Exit; } }
        public int CurrentWave { get { return _Wave; } }
        // Enemies still to defeat in the running wave, or the size of the next wave while building.
        public int ExpectedEnemies
        {
            get
            {
                if (_Battle == null) return 0;
                if (_Battle.Phase == TowerDefenseBattlePhase.Running) return _Battle.RemainingEnemies;
                return TowerDefenseWaves.Get(_MapIndex, _Wave).Spawns.Count;
            }
        }
        public int Gold { get { return _Gold; } }
        public string Difficulty { get { return DifficultyNames[(int)_Difficulty]; } }
        public string MapName { get { return _Map == null ? "Keine" : _Map.Name; } }
        public int MainBuildingHealth { get { return _Battle != null ? _Battle.HealthPercent : _MainBuildingHealth; } }
        public bool WaveRunning { get { return _Battle != null && _Battle.Phase == TowerDefenseBattlePhase.Running; } }
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
            _Tick = 0;
            _Towers = new Tower[0];
            _Upgrades = new TowerUpgrades[0];
            _Difficulty = TowerDefenseDifficulty.Normal;
            _Map = null;
            _MapIndex = -1;
            _StatusMessage = "Bereit";
            _MapSelectionMessage = null;
            _Paused = false;
            _PauseSelection = 0;
            _SlotSelection = 0;
            _SlotIndex = -1;
            _Wave = 1;
            _MainBuildingHealth = 100;
            _Dialog = TowerDefenseDialog.None;
            _DialogSelection = 0;
            _LastMouse = null;
            _ClickAreas.Clear();
            _Battle = null;
            _GameSpeed = 1;
            _ShowWavePreview = false;
            _Banner = null;
            _BannerTicks = 0;
            _ShowHelp = false;
            _HelpPage = 0;
        }

        public bool IsPaused { get { return _Paused; } }

        public int CurrentMapIndex { get { return _MapIndex; } }

        public int CurrentSlotIndex { get { return _SlotIndex; } }

        TowerDefenseSaveSlot CurrentSlot { get { return TowerDefenseSaves.Slots[_SlotIndex]; } }

        // Called when a wave has been beaten. This is the only checkpoint during a map: the save
        // then holds gold, towers and upgrades as they are at the start of the next wave. Anything
        // changed afterwards is lost when the map is left before that wave is beaten.
        void CompleteWave()
        {
            if (_Map == null) return;
            if (_Wave >= TowerDefenseWaves.WaveCount)
            {
                CompleteCurrentMap();
                return;
            }
            _Battle.Heal(RepairPerWave);
            int bonus = _Battle.CurrentWave.CompletionBonus;
            _Wave++;
            SaveRun();
            _StatusMessage = "Welle " + (_Wave - 1) + " geschafft";
            ShowBanner("WELLE " + (_Wave - 1) + " GESCHAFFT  +" + bonus + " GOLD");
        }

        // Called when the last wave of the current map has been beaten.
        void CompleteCurrentMap()
        {
            if (_Map == null) return;
            bool unlocked = TowerDefenseSaves.CompleteMap(_SlotIndex, _MapIndex);
            _Map = null;
            _Battle = null;
            bool hasNext = _MapIndex + 1 < TowerDefenseMaps.Maps.Count;
            _MapSelection = hasNext ? _MapIndex + 1 : _MapIndex;
            _MapSelectionMessage = !hasNext ? "ALLE KARTEN GESCHAFFT!"
                : unlocked ? "NEUE KARTE FREIGESCHALTET!" : "KARTE GESCHAFFT!";
            _Screen = TowerDefenseScreen.MapSelection;
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            _Tick++;
            if (_BannerTicks > 0) _BannerTicks--;
            HandleMouse();
            foreach (Key key in keys)
            {
                if (_ShowHelp)
                {
                    HandleHelpKey(key);
                    continue;
                }

                if (_Dialog != TowerDefenseDialog.None && !_Paused)
                {
                    HandleDialogKey(key);
                    continue;
                }

                if (key == Key.Escape || key == Key.X)
                {
                    _Paused = !_Paused;
                    _PauseSelection = 0;
                    continue;
                }

                if (_Paused)
                {
                    HandlePauseKey(key);
                    continue;
                }

                if (key == Key.Back)
                {
                    GoBack();
                    continue;
                }

                if (key == Key.E && _Screen == TowerDefenseScreen.Map)
                {
                    _ShowWavePreview = !_ShowWavePreview;
                }
                else if (key == Key.Space && _Screen == TowerDefenseScreen.Map)
                {
                    StartNextWave();
                }
                else if (key == Key.T && _Screen == TowerDefenseScreen.Map)
                {
                    ToggleSpeed();
                }
                else if (key == Key.Delete && _Screen == TowerDefenseScreen.SlotSelection)
                {
                    AskDeleteSlot();
                }
                else if (_Screen == TowerDefenseScreen.Map &&
                    (B5RoomKeys.IsLeft(key) || B5RoomKeys.IsRight(key) ||
                     B5RoomKeys.IsUp(key) || B5RoomKeys.IsDown(key)))
                {
                    MoveSpot(B5RoomKeys.IsLeft(key) ? -1 : B5RoomKeys.IsRight(key) ? 1 : 0,
                        B5RoomKeys.IsUp(key) ? -1 : B5RoomKeys.IsDown(key) ? 1 : 0);
                }
                else if ((_Screen == TowerDefenseScreen.TowerPlacement ||
                    _Screen == TowerDefenseScreen.TowerUpgrade) &&
                    (B5RoomKeys.IsUp(key) || B5RoomKeys.IsDown(key)))
                {
                    MoveMenuSelection(B5RoomKeys.IsDown(key) ? 1 : -1);
                }
                else if ((_Screen == TowerDefenseScreen.MainMenu ||
                    _Screen == TowerDefenseScreen.SlotSelection ||
                    _Screen == TowerDefenseScreen.DifficultySelection ||
                    _Screen == TowerDefenseScreen.MapSelection) &&
                    (B5RoomKeys.IsUp(key) || B5RoomKeys.IsDown(key) ||
                     B5RoomKeys.IsLeft(key) || B5RoomKeys.IsRight(key)))
                {
                    MoveSelection(B5RoomKeys.IsDown(key) || B5RoomKeys.IsRight(key) ? 1 : -1);
                }
                else if (key == Key.Enter || key == Key.Z)
                    ConfirmSelection();
            }
            StepBattle();
        }

        // ---------------- waves ----------------

        void CreateBattle()
        {
            _Battle = new TowerDefenseBattle(_MapIndex, _Map.Path, _Map.MainBuilding, _Map.TowerSpots,
                _Towers, _Upgrades, TowerDefenseDifficultyRules.GetMultiplier(_Difficulty), _MainBuildingHealth,
                gold => _Gold += gold);
            _ShowWavePreview = false;
            _GameSpeed = 1;
        }

        // The battle only runs on the plain map view; menus, dialogs and the pause menu freeze it.
        void StepBattle()
        {
            if (_Battle == null || _Screen != TowerDefenseScreen.Map || _Paused || _ShowHelp || _Dialog != TowerDefenseDialog.None)
                return;

            for (int step = 0; step < _GameSpeed; step++)
            {
                _Battle.Step(TickSeconds);
                if (_Battle.ConsumeWaveCompleted())
                {
                    CompleteWave();
                    return;
                }
                if (_Battle.Phase == TowerDefenseBattlePhase.Lost)
                {
                    _Dialog = TowerDefenseDialog.Defeat;
                    _DialogSelection = 0;
                    return;
                }
            }
        }

        void StartNextWave()
        {
            if (_Battle == null || _Battle.Phase != TowerDefenseBattlePhase.Building) return;
            _Battle.StartWave(_Wave);
            TowerDefenseWave wave = _Battle.CurrentWave;
            _StatusMessage = "Welle " + _Wave + " läuft";
            if (wave.Boss != null)
                ShowBanner((wave.IsMiniBoss ? "MINIBOSS: " : "BOSS: ") + wave.Boss.ToUpperInvariant());
            else
                ShowBanner("WELLE " + _Wave + " / " + TowerDefenseWaves.WaveCount);
        }

        void ToggleSpeed()
        {
            _GameSpeed = _GameSpeed % 3 + 1;
        }

        void ShowBanner(string text)
        {
            _Banner = text;
            _BannerTicks = 60;
        }

        public void Draw(Canvas canvas)
        {
            if (_PixelSurface == null || !ReferenceEquals(canvas.Tag, _PixelSurface))
            {
                canvas.Children.Clear();
                _PixelSurface = new TowerDefensePixelSurface(DesignWidth, DesignHeight, PixelScale);
                canvas.Tag = _PixelSurface;
                canvas.Children.Add(_PixelSurface.Image);
            }

            _PixelSurface.Begin();
            _ClickAreas.Clear();
            switch (_Screen)
            {
                case TowerDefenseScreen.MainMenu:
                    DrawMainMenu();
                    break;
                case TowerDefenseScreen.SlotSelection:
                    DrawSlotSelection();
                    break;
                case TowerDefenseScreen.DifficultySelection:
                    DrawDifficultySelection();
                    break;
                case TowerDefenseScreen.MapSelection:
                    DrawMapSelection();
                    break;
                case TowerDefenseScreen.TowerPlacement:
                    DrawMap();
                    DrawTowerPlacementMenu();
                    break;
                case TowerDefenseScreen.TowerUpgrade:
                    DrawMap();
                    DrawTowerUpgradeMenu();
                    break;
                default:
                    DrawMap();
                    break;
            }
            DrawFooterBar();
            if (_Dialog != TowerDefenseDialog.None) DrawDialog();
            if (_Paused) DrawPauseMenu();
            if (_ShowHelp) DrawHelp();
            _PixelSurface.End();
        }

        // ---------------- save slots ----------------

        void SelectSlot(int slotIndex)
        {
            _SlotIndex = slotIndex;
            TowerDefenseSaveSlot slot = CurrentSlot;
            _MapSelection = slot.LastMap >= 0 ? slot.LastMap : slot.UnlockedMaps - 1;
            _MapSelectionMessage = null;
            _Screen = TowerDefenseScreen.MapSelection;
        }

        void AskDeleteSlot()
        {
            if (TowerDefenseSaves.Slots[_SlotSelection].IsEmpty) return;
            _Dialog = TowerDefenseDialog.DeleteSlot;
            _DialogSelection = 0;
        }

        // Stores the running map in the current slot. Enemies are not stored; the wave restarts on resume.
        void SaveRun()
        {
            if (_Map == null || _SlotIndex < 0) return;
            TowerDefenseRunState run = new TowerDefenseRunState
            {
                Difficulty = (int)_Difficulty,
                Wave = _Wave,
                Gold = _Gold,
                MainBuildingHealth = MainBuildingHealth,
                Towers = new string[_Towers.Length],
                TowerLevels = null
            };
            for (int i = 0; i < _Towers.Length; i++)
                run.Towers[i] = _Towers[i] == null ? null : _Towers[i].Name;
            run.SetUpgrades(_Upgrades);
            TowerDefenseSaves.StoreRun(_SlotIndex, _MapIndex, run);
        }

        static Tower CreateTower(string name)
        {
            int index = Array.IndexOf(TowerNames, name);
            if (index < 0) throw new InvalidDataException("Unbekannter Turm im Spielstand: " + name);
            return TowerCreators[index]();
        }

        // ---------------- dialogs ----------------

        string[] DialogItems()
        {
            if (_Dialog == TowerDefenseDialog.ResumeRun) return new[] { "WEITERSPIELEN", "NEU BEGINNEN", "ABBRECHEN" };
            if (_Dialog == TowerDefenseDialog.Defeat) return new[] { "WELLE NEU", "KARTENWAHL" };
            return new[] { "NEIN", "JA, LÖSCHEN" };
        }

        // The defeat question cannot be dismissed: the lost battle must not continue.
        void CloseDialog()
        {
            if (_Dialog != TowerDefenseDialog.Defeat) _Dialog = TowerDefenseDialog.None;
        }

        void HandleDialogKey(Key key)
        {
            int count = DialogItems().Length;
            if (key == Key.Escape || key == Key.X || key == Key.Back)
                CloseDialog();
            else if (B5RoomKeys.IsUp(key) || B5RoomKeys.IsDown(key))
                _DialogSelection = (_DialogSelection + (B5RoomKeys.IsDown(key) ? 1 : -1) + count) % count;
            else if (key == Key.Enter || key == Key.Z)
                ActivateDialogItem(_DialogSelection);
        }

        void ActivateDialogItem(int index)
        {
            TowerDefenseDialog dialog = _Dialog;
            _Dialog = TowerDefenseDialog.None;
            if (dialog == TowerDefenseDialog.ResumeRun)
            {
                if (index == 0) ResumeMap(_MapSelection);
                else if (index == 1) _Screen = TowerDefenseScreen.DifficultySelection;
            }
            else if (dialog == TowerDefenseDialog.Defeat)
            {
                // Back to the last checkpoint (start of the lost wave) or to the map selection.
                if (index == 0) ResumeMap(_MapIndex);
                else
                {
                    _MapSelection = _MapIndex;
                    _Map = null;
                    _Battle = null;
                    _Screen = TowerDefenseScreen.MapSelection;
                }
            }
            else if (index == 1)
            {
                TowerDefenseSaves.Delete(_SlotSelection);
            }
        }

        void DrawDialog()
        {
            _ClickAreas.Clear();
            string[] items = DialogItems();
            string title, info;
            TowerDefenseIcon[] icons;
            if (_Dialog == TowerDefenseDialog.ResumeRun)
            {
                TowerDefenseRunState run = CurrentSlot.GetRun(_MapSelection);
                int towers = 0;
                foreach (string tower in run.Towers) if (tower != null) towers++;
                title = "WEITERSPIELEN?";
                info = TowerDefenseMaps.Maps[_MapSelection].Name.ToUpperInvariant() + "  WELLE " + run.Wave +
                    "  GOLD " + run.Gold + "  TÜRME " + towers + "  " + DifficultyNames[run.Difficulty].ToUpperInvariant();
                icons = new[] { TowerDefenseIcon.Upgrade, TowerDefenseIcon.Sword, TowerDefenseIcon.Wave };
            }
            else if (_Dialog == TowerDefenseDialog.Defeat)
            {
                title = "BURG ZERSTÖRT";
                info = "WELLE " + _Wave + " VERLOREN - NEUSTART AB WELLENBEGINN";
                icons = new[] { TowerDefenseIcon.Upgrade, TowerDefenseIcon.Wave };
            }
            else
            {
                title = "LÖSCHEN?";
                info = "SPIELSTAND " + (_SlotSelection + 1) + " WIRKLICH LÖSCHEN?";
                icons = new[] { TowerDefenseIcon.Heart, TowerDefenseIcon.Skull };
            }

            Surface.Rect(0, 0, DesignWidth, DesignHeight, Color.FromArgb(150, 0, 0, 0));
            double height = 52 + items.Length * 21;
            double top = 88 - height / 2;
            DrawPanel(54, top, 212, height);
            AddClickArea(54, top, 212, height, null, null);
            DrawTitle(title, top + 7);
            ShadowText(info, 160 - info.Length * 1.65, top + 30, 5, TextLight);
            for (int i = 0; i < items.Length; i++)
            {
                int index = i;
                DrawMenuEntry(items[i], icons[i], _DialogSelection == i, top + 42 + i * 21,
                    () => _DialogSelection = index, () => ActivateDialogItem(index));
            }
        }

        // ---------------- pause menu ----------------

        string[] PauseItems()
        {
            return _Screen == TowerDefenseScreen.MainMenu
                ? new[] { "FORTSETZEN", "ANLEITUNG", "SPIEL VERLASSEN" }
                : new[] { "FORTSETZEN", "ANLEITUNG", "HAUPTMENÜ", "SPIEL VERLASSEN" };
        }

        void HandlePauseKey(Key key)
        {
            int count = PauseItems().Length;
            if (B5RoomKeys.IsUp(key) || B5RoomKeys.IsDown(key))
                _PauseSelection = (_PauseSelection + (B5RoomKeys.IsDown(key) ? 1 : -1) + count) % count;
            else if (key == Key.Enter || key == Key.Z)
                ActivatePauseItem(_PauseSelection);
            else if (key == Key.Back)
                _Paused = false;
        }

        void ActivatePauseItem(int index)
        {
            string item = PauseItems()[index];
            if (item == "ANLEITUNG")
            {
                OpenHelp();
                return;
            }
            _Paused = false;
            if (item == "HAUPTMENÜ") ReturnToMainMenu();
            else if (item == "SPIEL VERLASSEN")
            {
                _Exit = true;
            }
        }

        void ReturnToMainMenu()
        {
            _Dialog = TowerDefenseDialog.None;
            _Screen = TowerDefenseScreen.MainMenu;
            _MenuSelection = 0;
            _Map = null;
            _Battle = null;
            _MapIndex = -1;
            _Towers = new Tower[0];
            _Upgrades = new TowerUpgrades[0];
            _MapSelectionMessage = null;
        }

        void DrawPauseMenu()
        {
            _ClickAreas.Clear();
            string[] items = PauseItems();
            Surface.Rect(0, 0, DesignWidth, DesignHeight, Color.FromArgb(150, 0, 0, 0));
            double height = 44 + items.Length * 21;
            double top = 90 - height / 2;
            DrawPanel(80, top, 160, height);
            AddClickArea(80, top, 160, height, null, null);
            DrawTitle("MENÜ", top + 8);
            Dictionary<string, TowerDefenseIcon> icons = new Dictionary<string, TowerDefenseIcon>
            {
                { "FORTSETZEN", TowerDefenseIcon.Upgrade }, { "ANLEITUNG", TowerDefenseIcon.Range },
                { "HAUPTMENÜ", TowerDefenseIcon.Star }, { "SPIEL VERLASSEN", TowerDefenseIcon.Wave }
            };
            for (int i = 0; i < items.Length; i++)
            {
                int index = i;
                TowerDefenseIcon icon = icons[items[i]];
                DrawMenuEntry(items[i], icon, _PauseSelection == i, top + 36 + i * 21,
                    () => _PauseSelection = index, () => ActivatePauseItem(index));
            }
        }

        // ---------------- mouse ----------------

        void HandleMouse()
        {
            if (_PixelSurface == null) return;
            Point? mouse = _PixelSurface.MousePosition;
            if (mouse.HasValue && mouse != _LastMouse)
            {
                ClickArea hovered = HitTest(mouse.Value);
                if (hovered != null && hovered.Hover != null) hovered.Hover();
            }
            _LastMouse = mouse;

            foreach (TowerDefenseClick click in _PixelSurface.DrainClicks())
            {
                if (click.Right)
                {
                    MouseBack();
                    continue;
                }

                ClickArea area = HitTest(click.Position);
                if (area != null)
                {
                    if (area.Hover != null) area.Hover();
                    if (area.Click != null) area.Click();
                }
                else if (_ShowHelp) { }
                else if (_Paused) _Paused = false;
                else if (_Dialog != TowerDefenseDialog.None) CloseDialog();
                else if (_Screen == TowerDefenseScreen.TowerPlacement || _Screen == TowerDefenseScreen.TowerUpgrade)
                    _Screen = TowerDefenseScreen.Map;
            }
        }

        // Right click: closes the topmost menu, but never leaves a running map by accident.
        void MouseBack()
        {
            if (_ShowHelp) CloseHelp();
            else if (_Paused) _Paused = false;
            else if (_Dialog != TowerDefenseDialog.None) CloseDialog();
            else if (_Screen != TowerDefenseScreen.Map && _Screen != TowerDefenseScreen.MainMenu) GoBack();
        }

        // Areas registered later lie on top.
        ClickArea HitTest(Point position)
        {
            for (int i = _ClickAreas.Count - 1; i >= 0; i--)
                if (_ClickAreas[i].Area.Contains(position)) return _ClickAreas[i];
            return null;
        }

        void AddClickArea(double x, double y, double width, double height, Action hover, Action click)
        {
            _ClickAreas.Add(new ClickArea { Area = new Rect(x, y, width, height), Hover = hover, Click = click });
        }

        bool IsMouseOver(double x, double y, double width, double height)
        {
            Point? mouse = Surface.MousePosition;
            return mouse.HasValue && new Rect(x, y, width, height).Contains(mouse.Value);
        }

        // ---------------- game logic ----------------

        void MoveSelection(int direction)
        {
            if (_Screen == TowerDefenseScreen.MainMenu)
                _MenuSelection = (_MenuSelection + direction + 3) % 3;
            else if (_Screen == TowerDefenseScreen.SlotSelection)
                _SlotSelection = (_SlotSelection + direction + TowerDefenseSaves.SlotCount) % TowerDefenseSaves.SlotCount;
            else if (_Screen == TowerDefenseScreen.DifficultySelection)
                _DifficultySelection = (_DifficultySelection + direction + DifficultyNames.Length) % DifficultyNames.Length;
            else if (_Screen == TowerDefenseScreen.MapSelection)
            {
                _MapSelection = (_MapSelection + direction + TowerDefenseMaps.Maps.Count) % TowerDefenseMaps.Maps.Count;
                _MapSelectionMessage = null;
            }
        }

        // Jumps to the pedestal that lies closest to the pressed direction. Only pedestals within
        // about 72 degrees of that direction count; the angle weighs more than the distance, so a
        // pedestal in the same row wins over a closer one that lies far to the side.
        void MoveSpot(int dx, int dy)
        {
            Point current = _Map.TowerSpots[_SelectedSpot];
            int best = -1;
            double bestScore = double.MaxValue;
            for (int i = 0; i < _Map.TowerSpots.Count; i++)
            {
                if (i == _SelectedSpot) continue;
                double vx = _Map.TowerSpots[i].X - current.X, vy = _Map.TowerSpots[i].Y - current.Y;
                double along = vx * dx + vy * dy, across = Math.Abs(vx * dy - vy * dx);
                if (along <= 0) continue;
                double angle = Math.Atan2(across, along);
                if (angle > 1.25) continue;
                double score = Math.Sqrt(vx * vx + vy * vy) * (1 + 2.5 * angle * angle);
                if (score < bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }
            if (best >= 0) _SelectedSpot = best;
        }

        void MoveMenuSelection(int direction)
        {
            if (_Screen == TowerDefenseScreen.TowerPlacement)
                _TowerSelection = (_TowerSelection + direction + TowerNames.Length) % TowerNames.Length;
            else
                _UpgradeSelection = (_UpgradeSelection + direction + 5) % 5;
        }

        void ConfirmSelection()
        {
            _RangeSpot = -1;
            if (_Screen == TowerDefenseScreen.MainMenu)
            {
                if (_MenuSelection == 0)
                {
                    _SlotSelection = Math.Max(0, _SlotIndex);
                    _Screen = TowerDefenseScreen.SlotSelection;
                }
                else if (_MenuSelection == 1) OpenHelp();
                else _Exit = true;
            }
            else if (_Screen == TowerDefenseScreen.SlotSelection)
            {
                SelectSlot(_SlotSelection);
            }
            else if (_Screen == TowerDefenseScreen.MapSelection)
            {
                if (!CurrentSlot.IsUnlocked(_MapSelection))
                {
                    _MapSelectionMessage = "ERST " + TowerDefenseMaps.Maps[_MapSelection - 1].Name.ToUpperInvariant() + " SCHAFFEN";
                    return;
                }
                if (CurrentSlot.GetRun(_MapSelection) != null)
                {
                    _Dialog = TowerDefenseDialog.ResumeRun;
                    _DialogSelection = 0;
                    return;
                }
                _Screen = TowerDefenseScreen.DifficultySelection;
            }
            else if (_Screen == TowerDefenseScreen.DifficultySelection)
            {
                _Difficulty = (TowerDefenseDifficulty)_DifficultySelection;
                StartMap(_MapSelection);
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
                if (_UpgradeSelection < 4) UpgradeTower((TowerUpgradePath)_UpgradeSelection);
                else SellTower();
            }
        }

        void StartMap(int mapIndex)
        {
            _MapIndex = mapIndex;
            _Map = TowerDefenseMaps.Maps[mapIndex];
            _Towers = new Tower[_Map.TowerSpots.Count];
            _Upgrades = new TowerUpgrades[_Map.TowerSpots.Count];
            _Gold = 500;
            _Wave = 1;
            _MainBuildingHealth = 100;
            _SelectedSpot = 0;
            _StatusMessage = "Bereit";
            _MapSelectionMessage = null;
            _Screen = TowerDefenseScreen.Map;
            CreateBattle();
            SaveRun();
        }

        // Continues a saved map. The saved wave starts again from its beginning.
        void ResumeMap(int mapIndex)
        {
            TowerDefenseRunState run = CurrentSlot.GetRun(mapIndex);
            _MapIndex = mapIndex;
            _Map = TowerDefenseMaps.Maps[mapIndex];
            _Difficulty = (TowerDefenseDifficulty)run.Difficulty;
            _Gold = run.Gold;
            _Wave = run.Wave;
            _MainBuildingHealth = run.MainBuildingHealth;
            _Towers = new Tower[_Map.TowerSpots.Count];
            _Upgrades = new TowerUpgrades[_Map.TowerSpots.Count];
            for (int i = 0; i < _Towers.Length && i < run.Towers.Length; i++)
            {
                if (run.Towers[i] == null) continue;
                _Towers[i] = CreateTower(run.Towers[i]);
                _Upgrades[i] = run.GetUpgrades(i);
            }
            _SelectedSpot = 0;
            CreateBattle();
            _StatusMessage = "Welle " + _Wave + " neu gestartet";
            _MapSelectionMessage = null;
            _Screen = TowerDefenseScreen.Map;
        }

        void GoBack()
        {
            if (_Screen == TowerDefenseScreen.SlotSelection) _Screen = TowerDefenseScreen.MainMenu;
            else if (_Screen == TowerDefenseScreen.DifficultySelection) _Screen = TowerDefenseScreen.MapSelection;
            else if (_Screen == TowerDefenseScreen.MapSelection) _Screen = TowerDefenseScreen.SlotSelection;
            else if (_Screen == TowerDefenseScreen.Map)
            {
                _Map = null;
                _Battle = null;
                _Screen = TowerDefenseScreen.MapSelection;
            }
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
            _Upgrades[_SelectedSpot] = new TowerUpgrades();
            _StatusMessage = tower.Name + " gebaut";
            _Screen = TowerDefenseScreen.Map;
        }

        // The upgrade menu stays open, so several upgrades can be bought in a row.
        void UpgradeTower(TowerUpgradePath path)
        {
            Tower tower = _Towers[_SelectedSpot];
            TowerUpgrades upgrades = _Upgrades[_SelectedSpot];
            if (upgrades.IsMaxed(path))
            {
                _StatusMessage = "Schon maximal";
                return;
            }
            int cost = upgrades.Cost(tower, path);
            if (_Gold < cost)
            {
                _StatusMessage = "Zu wenig Gold";
                return;
            }

            _Gold -= cost;
            upgrades.Apply(path);
            _StatusMessage = path == TowerUpgradePath.Tier
                ? tower.Name + " Stufe " + TierName(upgrades.Tier)
                : UpgradeNames[(int)path] + " verbessert";
        }

        static readonly string[] UpgradeNames = { "Schaden", "Reichweite", "Tempo", "Turmstufe" };

        static string TierName(int tier)
        {
            return tier == 3 ? "III" : tier == 2 ? "II" : "I";
        }

        void SellTower()
        {
            Tower tower = _Towers[_SelectedSpot];
            _Gold += TowerDefenseBattle.SellValue(tower, _Upgrades[_SelectedSpot]);
            _Towers[_SelectedSpot] = null;
            _Upgrades[_SelectedSpot] = null;
            _StatusMessage = "Turm verkauft";
            _Screen = TowerDefenseScreen.Map;
        }

        // ---------------- menu screens ----------------

        void DrawMainMenu()
        {
            DrawMenuBackground();
            DrawPanel(70, 30, 180, 108);
            DrawTitle("TOWER DEFENSE", 40);
            ShadowText("BAUE TÜRME. HALTE DIE WELLEN AUF.", 86, 64, 6, TextDim);
            DrawMenuEntry("SPIELEN", TowerDefenseIcon.Sword, _MenuSelection == 0, 76,
                () => _MenuSelection = 0, ConfirmSelection);
            DrawMenuEntry("ANLEITUNG", TowerDefenseIcon.Range, _MenuSelection == 1, 95,
                () => _MenuSelection = 1, ConfirmSelection);
            DrawMenuEntry("SPIEL VERLASSEN", TowerDefenseIcon.Wave, _MenuSelection == 2, 114,
                () => _MenuSelection = 2, ConfirmSelection);
            DrawParade();
        }

        void DrawSlotSelection()
        {
            DrawMenuBackground();
            DrawPanel(36, 16, 248, 146);
            DrawTitle("SPIELSTAND WÄHLEN", 24);
            for (int i = 0; i < TowerDefenseSaves.SlotCount; i++)
                DrawSlotCard(i, 48, 50 + i * 35);
        }

        void DrawSlotCard(int slotIndex, double x, double y)
        {
            const double width = 224, height = 31;
            TowerDefenseSaveSlot slot = TowerDefenseSaves.Slots[slotIndex];
            bool selected = _SlotSelection == slotIndex;
            int index = slotIndex;
            AddClickArea(x, y, width, height, () => _SlotSelection = index, () => SelectSlot(index));

            Surface.Rect(x, y, width, height, selected ? SelectionEdge : Outline);
            Surface.Rect(x + 1, y + 1, width - 2, height - 2, selected ? Selection : PanelInsideDark);
            Surface.Rect(x + 1, y + 1, width - 2, 1, selected ? Color.FromRgb(150, 110, 62) : StoneDark);
            ShadowText((slotIndex + 1).ToString(), x + 6, y + 6, 14, selected ? TitleGold : TextDim);
            ShadowText("SPIELSTAND " + (slotIndex + 1), x + 26, y + 4, 8, selected ? TitleGold : TextLight);

            if (slot.IsEmpty)
            {
                ShadowText("LEER - NEUES SPIEL BEGINNEN", x + 26, y + 18, 5, TextDim);
                return;
            }

            string detail = "KARTEN " + slot.UnlockedMaps + "/" + TowerDefenseMaps.Maps.Count + " FREI";
            if (slot.LastMap >= 0)
            {
                detail += "   ZULETZT " + TowerDefenseMaps.Maps[slot.LastMap].Name.ToUpperInvariant();
                TowerDefenseRunState run = slot.GetRun(slot.LastMap);
                if (run != null) detail += " - WELLE " + run.Wave;
            }
            ShadowText(detail, x + 26, y + 18, 5, TextLight);

            for (int map = 0; map < TowerDefenseMaps.Maps.Count; map++)
            {
                double dx = x + width - 44 + map * 9;
                Surface.Rect(dx, y + 5, 6, 6, Outline);
                Surface.Rect(dx + 1, y + 6, 4, 4, slot.IsUnlocked(map) ? SelectionEdge : StoneDark);
            }
            string date = slot.LastPlayed.Value.ToString("dd.MM. HH:mm");
            Surface.Text(date, x + width - 6 - date.Length * 2.75, y + 19, 5, TextDim);
        }

        void DrawDifficultySelection()
        {
            DrawMenuBackground();
            DrawPanel(60, 22, 200, 124);
            DrawTitle("SCHWIERIGKEIT", 32);
            ShadowText("GEGNERWERTE WERDEN MIT DEM BASISWERT SKALIERT", 72, 55, 5, TextDim);
            for (int i = 0; i < DifficultyNames.Length; i++)
            {
                double y = 68 + i * 18;
                bool selected = _DifficultySelection == i;
                int index = i;
                AddClickArea(70, y, 180, 15, () => _DifficultySelection = index, ConfirmSelection);
                DrawSelectionBar(70, y, 180, 15, selected);
                ShadowText(DifficultyNames[i].ToUpperInvariant(), 78, y + 2, 9, selected ? TitleGold : TextLight);
                for (int skull = 0; skull <= i; skull++)
                    Icon(TowerDefenseIcon.Skull, 160 + skull * 10, y + 3, 8);
                ShadowText(GetMultiplierText((TowerDefenseDifficulty)i), 210, y + 3, 7, TextGold);
            }
            DrawParade();
        }

        void DrawMapSelection()
        {
            DrawMenuBackground();
            DrawPanel(56, 14, 208, 146);
            DrawTitle("KARTE AUSWÄHLEN", 21);
            TowerDefenseMapDefinition map = TowerDefenseMaps.Maps[_MapSelection];
            bool unlocked = CurrentSlot.IsUnlocked(_MapSelection);
            TowerDefenseRunState savedRun = CurrentSlot.GetRun(_MapSelection);

            const double previewX = 80, previewY = 45, scale = 0.5;
            AddClickArea(previewX - 2, previewY - 2, 164, 82, null, ConfirmSelection);
            Surface.Rect(previewX - 2, previewY - 2, 164, 82, Outline);
            Surface.Rect(previewX - 1, previewY - 1, 162, 80, unlocked ? StoneHighlight : StoneDark);
            Surface.Sprite(TowerDefenseSprites.Map(map.BackgroundFile), previewX, previewY,
                TowerDefenseSprites.MapImageWidth * scale, 78);
            Surface.Sprite(TowerDefenseSprites.MainBuilding(0),
                previewX + (map.MainBuilding.X - 32) * scale, previewY + (map.MainBuilding.Y + 2 - 44) * scale, 32, 24);
            if (!unlocked)
            {
                Surface.Rect(previewX, previewY, 160, 78, Color.FromArgb(175, 16, 14, 20));
                Icon(TowerDefenseIcon.Lock, 144, previewY + 16, 32);
                ShadowText("GESPERRT", 138, previewY + 54, 8, TextLight);
            }
            else if (savedRun != null)
            {
                string saved = "GESPEICHERT: WELLE " + savedRun.Wave;
                double badgeWidth = saved.Length * 2.75 + 18;
                Surface.Rect(previewX + 2, previewY + 2, badgeWidth, 11, Outline);
                Surface.Rect(previewX + 3, previewY + 3, badgeWidth - 2, 9, Selection);
                Icon(TowerDefenseIcon.Wave, previewX + 4, previewY + 3, 8);
                Surface.Text(saved, previewX + 14, previewY + 3, 5, TitleGold);
            }

            DrawArrowButton("<", 60, 74, () => MoveSelection(-1));
            DrawArrowButton(">", 244, 74, () => MoveSelection(1));

            string title = (_MapSelection + 1) + "/" + TowerDefenseMaps.Maps.Count + "  " + map.Name.ToUpperInvariant();
            ShadowText(title, 160 - title.Length * 2.75, 126, 10, unlocked ? TitleGold : TextDim);
            double lineWidth = (_MapSelection + 1) * 9 + 4 + map.Description.Length * 3.3;
            double lineX = 160 - lineWidth / 2;
            for (int skull = 0; skull <= _MapSelection; skull++)
                Icon(TowerDefenseIcon.Skull, lineX + skull * 9, 140, 8);
            ShadowText(map.Description, lineX + (_MapSelection + 1) * 9 + 4, 141, 6, TextLight);
            DrawMapProgressDots(150);

            if (_MapSelectionMessage != null)
            {
                double width = _MapSelectionMessage.Length * 4.4 + 12;
                Surface.Rect(160 - width / 2, 100, width, 14, Outline);
                Surface.Rect(160 - width / 2 + 1, 101, width - 2, 12, Selection);
                ShadowText(_MapSelectionMessage, 160 - width / 2 + 6, 102, 8, TitleGold);
            }
        }

        void DrawArrowButton(string arrow, double x, double y, Action click)
        {
            bool hovered = IsMouseOver(x, y, 14, 18);
            AddClickArea(x, y, 14, 18, null, click);
            if (hovered)
            {
                Surface.Rect(x, y, 14, 18, SelectionEdge);
                Surface.Rect(x + 1, y + 1, 12, 16, Selection);
            }
            ShadowText(arrow, x + 4, y + 4, 12, TitleGold);
        }

        // One dot per map below the selection panel: gold = unlocked, grey = locked, framed = selected.
        void DrawMapProgressDots(double y)
        {
            int count = TowerDefenseMaps.Maps.Count;
            double left = 160 - count * 5;
            for (int i = 0; i < count; i++)
            {
                double x = left + i * 10;
                int index = i;
                AddClickArea(x - 2, y, 10, 11, null, () => { _MapSelection = index; _MapSelectionMessage = null; });
                Surface.Rect(x, y + 2, 6, 6, Outline);
                Surface.Rect(x + 1, y + 3, 4, 4, CurrentSlot.IsUnlocked(i) ? SelectionEdge : StoneDark);
                if (i == _MapSelection) Surface.Rect(x + 1, y + 9, 4, 1, TitleGold);
            }
        }

        void DrawMenuBackground()
        {
            TowerDefenseMapDefinition map = TowerDefenseMaps.Maps[0];
            Surface.Rect(0, 0, DesignWidth, DesignHeight, Color.FromRgb(98, 148, 66));
            Surface.Sprite(TowerDefenseSprites.Map(map.BackgroundFile), 0, 25,
                TowerDefenseSprites.MapImageWidth, TowerDefenseSprites.MapImageHeight);
            DrawMainBuilding(map, MapTop);
            Surface.Rect(0, 0, DesignWidth, DesignHeight, Color.FromArgb(150, 14, 18, 24));
        }

        // A small procession of enemy sprites walking along the bottom edge.
        void DrawParade()
        {
            const double groundY = 166;
            Surface.Rect(0, groundY - 2, DesignWidth, 4, Color.FromArgb(120, 0, 0, 0));
            double offset = (_Tick * 0.6) % 36;
            for (int i = 0; i < 11; i++)
            {
                string name = MenuParade[(i + _Tick / 60) % MenuParade.Length];
                double x = -36 + i * 36 + offset;
                int frame = (_Tick / 6 + i) % 2;
                if (TowerDefenseSprites.IsBoss(name))
                    Surface.Sprite(TowerDefenseSprites.Boss(name, frame), x - 16, groundY - 28, 32, 32);
                else
                    Surface.Sprite(TowerDefenseSprites.Enemy(name, frame), x - 8, groundY - 13, 16, 16);
            }
        }

        // ---------------- map ----------------

        void DrawMap()
        {
            Surface.Sprite(TowerDefenseSprites.Map(_Map.BackgroundFile), 0, MapTop - 2,
                TowerDefenseSprites.MapImageWidth, TowerDefenseSprites.MapImageHeight);
            DrawMainBuilding(_Map, MapTop);
            if (_Screen == TowerDefenseScreen.Map)
            {
                Tower selected = _Towers[_SelectedSpot];
                double range = selected == null ? 0 : TowerDefenseBattle.TowerRange(selected, _Upgrades[_SelectedSpot]);
                if (RangeVisible(range) && selected != null) DrawRangeCircle(_Map.TowerSpots[_SelectedSpot], range);
            }

            // Towers and enemies back to front, so lower sprites overlap the ones behind them.
            // Click areas follow the same order, so the frontmost tower wins the hit test.
            List<KeyValuePair<double, int>> order = new List<KeyValuePair<double, int>>();
            for (int i = 0; i < _Map.TowerSpots.Count; i++) order.Add(new KeyValuePair<double, int>(_Map.TowerSpots[i].Y, i));
            if (_Battle != null)
                for (int e = 0; e < _Battle.Enemies.Count; e++)
                    order.Add(new KeyValuePair<double, int>(_Battle.Enemies[e].Y - 1, -1 - e));
            order.Sort((a, b) => a.Key.CompareTo(b.Key));
            foreach (KeyValuePair<double, int> entry in order)
            {
                if (entry.Value < 0)
                {
                    int enemyIndex = -1 - entry.Value;
                    DrawEnemy(_Battle.Enemies[enemyIndex], enemyIndex);
                    continue;
                }
                int i = entry.Value;
                Point spot = _Map.TowerSpots[i];
                double x = spot.X, y = spot.Y + MapTop;
                int index = i;
                Action select = () => _SelectedSpot = index;
                Action open = () => { _SelectedSpot = index; ConfirmSelection(); };
                if (_Towers[i] == null)
                {
                    AddClickArea(x - 15, y - 10, 30, 19, select, open);
                    continue;
                }
                AddClickArea(x - 16, y - 40, 32, 48, select, open);
                DrawTowerSprite(_Towers[i], _Upgrades[i], x, y, HidesEnemy(spot) ? TowerSeeThroughOpacity : 1);
            }
            DrawProjectilesAndEffects();
            DrawTowerMarker();
            DrawHud();
            DrawBanner();
            if (_ShowWavePreview) DrawWavePreview();
        }

        void DrawEnemy(TowerDefenseBattleEnemy enemy, int index)
        {
            double x = enemy.X, y = enemy.Y + MapTop;
            int frame = (_Tick / 6 + index) % 2;
            Enemy stats = enemy.Enemy;
            double barWidth;
            double top;
            if (TowerDefenseSprites.IsBoss(enemy.Name))
            {
                Surface.Sprite(TowerDefenseSprites.Boss(enemy.Name, frame), x - 16, y - 28, 32, 32, enemy.FacingLeft);
                barWidth = 24;
                top = y - 31;
            }
            else
            {
                Surface.Sprite(TowerDefenseSprites.Enemy(enemy.Name, frame), x - 8, y - 13, 16, 16, enemy.FacingLeft);
                barWidth = 12;
                top = y - 15;
            }
            if (enemy.HitFlash > 0) Surface.Rect(x - barWidth / 2, top + 3, barWidth, 1, Color.FromArgb(160, 255, 255, 255));

            // Health bar once damaged (always for bosses), plus a dot per active status effect.
            if (stats.CurrentHealth < stats.MaxHealth || enemy.IsBoss)
            {
                double fill = Math.Max(1, Math.Round(barWidth * stats.CurrentHealth / (double)stats.MaxHealth));
                Surface.Rect(x - barWidth / 2 - 1, top - 1, barWidth + 2, 3, Outline);
                Surface.Rect(x - barWidth / 2, top, fill, 1, enemy.IsBoss ? Color.FromRgb(230, 90, 220) : Color.FromRgb(120, 220, 90));
            }
            double dotX = x + barWidth / 2 + 2;
            if (stats.BurnDamagePerSecond > 0) { Surface.Rect(dotX, top - 1, 2, 2, Color.FromRgb(255, 140, 40)); dotX += 3; }
            if (stats.PoisonDamagePerSecond > 0) { Surface.Rect(dotX, top - 1, 2, 2, Color.FromRgb(120, 220, 80)); dotX += 3; }
            if (stats.CurrentMoveSpeed < stats.MoveSpeed) { Surface.Rect(dotX, top - 1, 2, 2, Color.FromRgb(150, 220, 255)); dotX += 3; }
            if (stats.CurrentArmor < stats.Armor) Surface.Rect(dotX, top - 1, 2, 2, Color.FromRgb(200, 150, 90));
        }

        void DrawProjectilesAndEffects()
        {
            if (_Battle == null) return;
            foreach (TowerDefenseProjectile projectile in _Battle.Projectiles)
                Surface.Sprite(TowerDefenseSprites.DamageIcon(projectile.DamageType),
                    projectile.X - 3, projectile.Y + MapTop - 3, 6, 6);

            foreach (TowerDefenseEffect effect in _Battle.Effects)
            {
                if (effect.Text != null)
                {
                    double rise = (1 - effect.TimeLeft / effect.Duration) * 8;
                    Point p = effect.Points[0];
                    ShadowText(effect.Text, p.X - effect.Text.Length * 1.4, p.Y + MapTop - rise, 5, TitleGold);
                    continue;
                }
                for (int i = 0; i + 1 < effect.Points.Count; i++)
                    DrawBolt(effect.Points[i], effect.Points[i + 1]);
            }
        }

        // Jagged lightning line between two map points.
        void DrawBolt(Point from, Point to)
        {
            double dx = to.X - from.X, dy = to.Y - from.Y;
            int steps = Math.Max(1, (int)(Math.Sqrt(dx * dx + dy * dy) / 3));
            for (int s = 0; s <= steps; s++)
            {
                double t = s / (double)steps;
                double jitter = s == 0 || s == steps ? 0 : ((s * 7 + _Tick) % 3) - 1;
                double x = from.X + dx * t + jitter, y = from.Y + dy * t + MapTop + jitter;
                Surface.Rect(x - 1, y - 1, 3, 3, Color.FromArgb(120, 200, 170, 255));
                Surface.Rect(x, y, 1, 1, Color.FromRgb(255, 250, 200));
            }
        }

        void DrawRangeCircle(Point spot, double range)
        {
            DrawRangeRing(spot.X, spot.Y - 6 + MapTop, range, 25, 169);
        }

        static readonly TimeSpan RangeShowTime = TimeSpan.FromSeconds(3);
        int _RangeSpot = -1;
        Tower _RangeTower;
        double _RangeValue;
        DateTime _RangeShownSince;

        // The range shows for a few seconds after a tower gets selected, built or upgraded.
        // Hovering the same tower again does not restart the timer; clicking or Enter does.
        bool RangeVisible(double range)
        {
            Tower tower = _Towers[_SelectedSpot];
            if (_SelectedSpot != _RangeSpot || tower != _RangeTower || range != _RangeValue)
            {
                _RangeSpot = _SelectedSpot;
                _RangeTower = tower;
                _RangeValue = range;
                _RangeShownSince = DateTime.UtcNow;
            }
            return DateTime.UtcNow - _RangeShownSince < RangeShowTime;
        }

        static readonly Color RangeRingColor = Color.FromRgb(120, 235, 255);
        static readonly Color RangeRingShadow = Color.FromArgb(200, 20, 16, 30);

        // Dotted ring; every dot has a dark border so it stays visible on light and dark maps.
        void DrawRangeRing(double cx, double cy, double radius, double minY, double maxY)
        {
            int points = Math.Max(24, (int)(radius * 0.8));
            for (int i = 0; i < points; i++)
            {
                double angle = i * Math.PI * 2 / points;
                double x = Math.Round(cx + Math.Cos(angle) * radius), y = Math.Round(cy + Math.Sin(angle) * radius);
                if (y < minY || y > maxY) continue;
                Surface.Rect(x - 1, y - 1, 4, 4, RangeRingShadow);
                Surface.Rect(x, y, 2, 2, RangeRingColor);
            }
        }

        void DrawBanner()
        {
            if (_BannerTicks <= 0 || _Banner == null) return;
            double width = _Banner.Length * 4.4 + 16;
            double x = 160 - width / 2;
            Surface.Rect(x, 30, width, 15, Outline);
            Surface.Rect(x + 1, 31, width - 2, 13, Selection);
            Surface.Rect(x + 1, 31, width - 2, 1, SelectionEdge);
            ShadowText(_Banner, x + 8, 33, 8, TitleGold);
        }

        // Window below the FEINDE cell: which enemy types the current or next wave brings (no counts).
        void DrawWavePreview()
        {
            TowerDefenseWave wave = WaveRunning ? _Battle.CurrentWave : TowerDefenseWaves.Get(_MapIndex, _Wave);
            IList<string> types = wave.EnemyTypes;
            const double width = 124, x = 192;
            double height = 20 + types.Count * 17;
            const double y = 24;
            AddClickArea(x, y, width, height, null, () => _ShowWavePreview = false);
            Surface.Rect(x, y, width, height, Outline);
            Surface.Rect(x + 1, y + 1, width - 2, height - 2, StoneDark);
            Surface.Rect(x + 2, y + 2, width - 4, height - 4, PanelInside);
            string title = (WaveRunning ? "WELLE " : "NÄCHSTE WELLE ") + wave.Number;
            ShadowText(title, x + 6, y + 4, 6, TitleGold);
            ShadowText("x", x + width - 10, y + 3, 6, TextDim);
            for (int i = 0; i < types.Count; i++)
            {
                string name = types[i];
                double rowY = y + 16 + i * 17;
                bool boss = TowerDefenseSprites.IsBoss(name);
                Surface.Rect(x + 5, rowY, 18, 16, PanelInsideDark);
                if (boss)
                    Surface.Sprite(TowerDefenseSprites.Boss(name, 0), x + 6, rowY, 16, 16);
                else
                    Surface.Sprite(TowerDefenseSprites.Enemy(name, 0), x + 6, rowY, 16, 16);
                ShadowText(name.ToUpperInvariant(), x + 27, rowY + 2, 5, boss ? TitleGold : TextLight);
                if (boss)
                {
                    string label = wave.IsMiniBoss ? "MINIBOSS" : "BOSS";
                    Surface.Text(label, x + width - 5 - label.Length * 2.75, rowY + 2, 5, Color.FromRgb(230, 110, 200));
                }
                DrawPreviewDamageTypes(PreviewEnemy(name), x + 27, rowY + 9);
            }
        }

        static readonly Color WeakColor = Color.FromRgb(120, 220, 90);
        static readonly Color ResistColor = Color.FromRgb(222, 96, 64);
        static readonly Dictionary<string, Enemy> _PreviewEnemies = new Dictionary<string, Enemy>();

        static Enemy PreviewEnemy(string name)
        {
            Enemy enemy;
            if (!_PreviewEnemies.TryGetValue(name, out enemy))
            {
                enemy = TowerDefenseWaves.CreateEnemy(name, 1);
                _PreviewEnemies[name] = enemy;
            }
            return enemy;
        }

        // One line with weakness and resistance icons, same labels as in the help page.
        void DrawPreviewDamageTypes(Enemy enemy, double x, double y)
        {
            ShadowText("SCHWACH", x, y, 5, WeakColor);
            x += 7 * 2.75 + 1;
            foreach (DamageType type in enemy.WeakAgainst) { Icon(TowerDefenseSprites.DamageIcon(type), x, y, 7); x += 8; }
            x += 3;
            ShadowText("RESIST.", x, y, 5, ResistColor);
            x += 7 * 2.75 + 1;
            foreach (DamageType type in enemy.ResistantDamageTypes) { Icon(TowerDefenseSprites.DamageIcon(type), x, y, 7); x += 8; }
        }

        void DrawMainBuilding(TowerDefenseMapDefinition map, double top)
        {
            int health = MainBuildingHealth;
            int stage = health > 66 ? 0 : health > 33 ? 1 : 2;
            Surface.Sprite(TowerDefenseSprites.MainBuilding(stage),
                map.MainBuilding.X - 32, map.MainBuilding.Y + top - 44, 64, 48);
        }

        static readonly Color DamageColor = Color.FromRgb(232, 92, 70);
        static readonly Color RangeColor = Color.FromRgb(110, 210, 110);
        static readonly Color SpeedColor = Color.FromRgb(250, 210, 80);

        const double TowerSeeThroughOpacity = 0.4;

        // True when an enemy walks behind the tower on this pedestal, so the tower would hide it.
        bool HidesEnemy(Point spot)
        {
            if (_Battle == null) return false;
            foreach (TowerDefenseBattleEnemy enemy in _Battle.Enemies)
            {
                if (enemy.Y - 1 >= spot.Y) continue;
                double half = enemy.IsBoss ? 14 : 7, height = enemy.IsBoss ? 28 : 13;
                bool overlapX = enemy.X + half > spot.X - 13 && enemy.X - half < spot.X + 13;
                bool overlapY = enemy.Y + 2 > spot.Y - 42 && enemy.Y - height < spot.Y + 2;
                if (overlapX && overlapY) return true;
            }
            return false;
        }

        void DrawTowerSprite(Tower tower, TowerUpgrades upgrades, double x, double y, double opacity = 1)
        {
            if (opacity < 1) Surface.Sprite(TowerDefenseSprites.Tower(tower.Name), x - 16, y - 43, 32, 48, opacity);
            else Surface.Sprite(TowerDefenseSprites.Tower(tower.Name), x - 16, y - 43, 32, 48);
            if (upgrades.Tier > 1) DrawTierBadge(x + 9, y - 25, upgrades.Tier);
            if (upgrades.TotalLevels == 0) return;

            // One bar per upgrade path below the pedestal, 2 px per level.
            int widest = Math.Max(upgrades.Damage, Math.Max(upgrades.Range, upgrades.Speed));
            double left = x - widest;
            Surface.Rect(left - 1, y + 2, widest * 2 + 2, 7, Outline);
            Surface.Rect(left, y + 3, upgrades.Damage * 2, 1, DamageColor);
            Surface.Rect(left, y + 5, upgrades.Range * 2, 1, RangeColor);
            Surface.Rect(left, y + 7, upgrades.Speed * 2, 1, SpeedColor);
        }

        // Small shield with one dot per tier: silver for tier II, gold for tier III.
        void DrawTierBadge(double x, double y, int tier)
        {
            Color fill = tier >= 3 ? SelectionEdge : Color.FromRgb(196, 204, 214);
            Surface.Rect(x - 1, y - 1, 9, 9, Outline);
            Surface.Rect(x + 1, y + 8, 5, 1, Outline);
            Surface.Rect(x, y, 7, 7, fill);
            Surface.Rect(x + 1, y + 7, 5, 1, fill);
            Surface.Rect(x, y, 7, 1, Color.FromRgb(255, 248, 220));
            for (int dot = 0; dot < tier; dot++)
                Surface.Rect(x + 1 + dot * 2, y + 3, 1, 2, Color.FromRgb(120, 36, 32));
        }

        void DrawTowerMarker()
        {
            Point spot = _Map.TowerSpots[_SelectedSpot];
            double x = spot.X, y = spot.Y + MapTop;
            bool occupied = _Towers[_SelectedSpot] != null;
            Color marker = occupied ? SelectionEdge : Color.FromRgb(120, 220, 240);
            double pulse = (_Tick / 8) % 2;
            double left = x - 16 - pulse, right = x + 13 + pulse, top = y - 8 - pulse, bottom = y + 6 + pulse;
            DrawCorner(left, top, 1, 1, marker);
            DrawCorner(right, top, -1, 1, marker);
            DrawCorner(left, bottom, 1, -1, marker);
            DrawCorner(right, bottom, -1, -1, marker);

            double bounce = (_Tick / 6) % 4 < 2 ? 0 : 2;
            double cursorY = (occupied ? y - 58 : y - 22) - bounce;
            Icon(TowerDefenseIcon.Cursor, x - 8, cursorY, 16);
        }

        void DrawCorner(double x, double y, int dx, int dy, Color color)
        {
            double hx = dx > 0 ? x : x - 2;
            double vy = dy > 0 ? y : y - 2;
            Surface.Rect(hx - 1, y - 1, 6, 3, Outline);
            Surface.Rect(x - 1, vy - 1, 3, 6, Outline);
            Surface.Rect(dx > 0 ? x : x - 3, y, 5, 1, color);
            Surface.Rect(x, dy > 0 ? y : y - 3, 1, 5, color);
        }

        // ---------------- HUD ----------------

        void DrawHud()
        {
            Surface.Rect(0, 0, DesignWidth, 25, Outline);
            Surface.Rect(0, 0, DesignWidth, 23, StoneDark);
            Surface.Rect(0, 0, DesignWidth, 1, Stone);
            Surface.Rect(0, 22, DesignWidth, 1, Color.FromRgb(74, 68, 60));
            DrawHudCell(3, TowerDefenseIcon.Coin, "GOLD", Gold.ToString());
            DrawHudCell(67, TowerDefenseIcon.Heart, "LEBEN", MainBuildingHealth + "%");
            DrawHudCell(131, TowerDefenseIcon.Wave, "WELLE", CurrentWave + "/" + TowerDefenseWaves.WaveCount);
            DrawHudCell(195, TowerDefenseIcon.Skull, WaveRunning ? "FEINDE" : "NÄCHSTE", ExpectedEnemies.ToString());
            // The enemy cell opens the wave preview (mouse or E).
            bool hovered = IsMouseOver(195, 3, 60, 17);
            AddClickArea(195, 3, 60, 17, null, () => _ShowWavePreview = !_ShowWavePreview);
            if (hovered || _ShowWavePreview)
            {
                Surface.Rect(195, 3, 60, 1, SelectionEdge);
                Surface.Rect(195, 19, 60, 1, SelectionEdge);
            }
            Surface.Text(_ShowWavePreview ? "^" : "v", 247, 10, 6, hovered ? TitleGold : TextDim);
            DrawHudInfo(259);
        }

        void DrawHudCell(double x, TowerDefenseIcon icon, string label, string value, double y = 3)
        {
            Surface.Rect(x, y, 60, 17, Outline);
            Surface.Rect(x + 1, y + 1, 58, 15, PanelInside);
            Surface.Rect(x + 1, y + 1, 58, 1, PanelInsideDark);
            Icon(icon, x + 2, y + 1, 16);
            Surface.Text(label, x + 20, y, 5, TextDim);
            ShadowText(value, x + 20, y + 6, 8, TitleGold);
        }

        void DrawHudInfo(double x)
        {
            Surface.Rect(x, 3, 58, 17, Outline);
            Surface.Rect(x + 1, 4, 56, 15, PanelInside);
            for (int skull = 0; skull <= (int)_Difficulty; skull++)
                Icon(TowerDefenseIcon.Skull, x + 3 + skull * 7, 5, 6);
            Surface.Text(Difficulty.ToUpperInvariant(), x + 32, 4, 5, TextDim);
            string status = Status.ToUpperInvariant();
            if (status.Length > 17) status = status.Substring(0, 17);
            Surface.Text(status, x + 3, 12, 5, TextLight);
        }

        // ---------------- tower menus ----------------

        void DrawTowerPlacementMenu()
        {
            _ClickAreas.Clear();
            Surface.Rect(0, 0, DesignWidth, DesignHeight, Color.FromArgb(110, 0, 0, 0));
            DrawPanel(14, 28, 292, 136);
            AddClickArea(14, 28, 292, 136, null, null);
            DrawTitle("TURM BAUEN", 36);
            for (int i = 0; i < TowerNames.Length; i++)
            {
                Tower option = TowerCreators[i]();
                double y = 55 + i * 12;
                bool selected = i == _TowerSelection;
                bool affordable = _Gold >= option.Cost;
                int index = i;
                AddClickArea(24, y, 148, 12, () => _TowerSelection = index, BuildTower);
                DrawSelectionBar(24, y, 148, 12, selected);
                Icon(TowerDefenseSprites.DamageIcon(option.DamageType), 28, y + 2, 8);
                ShadowText(option.Name.ToUpperInvariant(), 40, y + 2, 6, selected ? TitleGold : TextLight);
                Icon(TowerDefenseIcon.Coin, 140, y + 2, 8);
                ShadowText(option.Cost.ToString(), 150, y + 2, 6, affordable ? TextGold : Color.FromRgb(222, 96, 64));
            }

            Tower tower = TowerCreators[_TowerSelection]();
            DrawTowerShowcase(tower, 178, 56);
            AddClickArea(178, 56, 48, 72, null, BuildTower);
            DrawTowerStats(tower, 230, 58);
            ShadowText("KLICK / ENTER  BAUEN    RECHTSKLICK / BACKSPACE  ZURÜCK", 64, 153, 5, TextDim);
        }

        void DrawTowerUpgradeMenu()
        {
            _ClickAreas.Clear();
            Surface.Rect(0, 0, DesignWidth, DesignHeight, Color.FromArgb(110, 0, 0, 0));
            DrawPanel(16, 28, 288, 140);
            AddClickArea(16, 28, 288, 140, null, null);
            DrawTitle("TURM VERWALTEN", 34);
            Tower tower = _Towers[_SelectedSpot];
            TowerUpgrades upgrades = _Upgrades[_SelectedSpot];
            DrawTowerShowcase(tower, 26, 60);
            if (upgrades.Tier > 1) DrawTierBadge(26 + 33, 60 + 33, upgrades.Tier);

            ShadowText(tower.Name.ToUpperInvariant(), 84, 56, 7, TitleGold);
            ShadowText("TURMSTUFE " + TierName(upgrades.Tier), 230, 57, 6, upgrades.Tier > 1 ? TitleGold : TextLight);

            // Current values; the selected upgrade shows its result in green.
            TowerUpgrades preview = _UpgradeSelection < 4 ? upgrades.Preview((TowerUpgradePath)_UpgradeSelection) : upgrades;
            DrawUpgradeStat(TowerDefenseIcon.Sword, TowerDefenseBattle.TowerDamage(tower, upgrades).ToString(),
                TowerDefenseBattle.TowerDamage(tower, preview).ToString(), 84);
            DrawUpgradeStat(TowerDefenseIcon.Range, (TowerDefenseBattle.TowerRange(tower, upgrades) / TowerDefenseBattle.RangeUnit).ToString("0.0"),
                (TowerDefenseBattle.TowerRange(tower, preview) / TowerDefenseBattle.RangeUnit).ToString("0.0"), 156);
            DrawUpgradeStat(TowerDefenseIcon.Speed, TowerDefenseBattle.TowerAttackSpeed(tower, upgrades).ToString("0.00"),
                TowerDefenseBattle.TowerAttackSpeed(tower, preview).ToString("0.00"), 226);
            DrawEffectInfo(tower, upgrades, preview, 26, 134);

            string[] labels =
            {
                "SCHADEN  +" + (int)(TowerUpgrades.DamagePerLevel * 100) + "%",
                "REICHWEITE  +" + (int)(TowerUpgrades.RangePerLevel * 100) + "%",
                "TEMPO  +" + (int)(TowerUpgrades.SpeedPerLevel * 100) + "%",
                upgrades.IsMaxed(TowerUpgradePath.Tier) ? "TURMSTUFE" : "TURMSTUFE " + TierName(upgrades.Tier + 1),
                "VERKAUFEN"
            };
            TowerDefenseIcon[] icons = { TowerDefenseIcon.Sword, TowerDefenseIcon.Range, TowerDefenseIcon.Speed, TowerDefenseIcon.Star, TowerDefenseIcon.Sell };
            Color[] colors = { DamageColor, RangeColor, SpeedColor, TitleGold, TextGold };
            for (int i = 0; i < labels.Length; i++)
            {
                double y = 78 + i * 15;
                int index = i;
                bool selected = _UpgradeSelection == i;
                Action click = i < 4 ? (Action)(() => UpgradeTower((TowerUpgradePath)index)) : SellTower;
                AddClickArea(82, y, 214, 14, () => _UpgradeSelection = index, click);
                DrawSelectionBar(82, y, 214, 14, selected);
                Icon(icons[i], 85, y + 3, 8);
                ShadowText(labels[i], 97, y + 3, 6, selected ? TitleGold : TextLight);

                if (i == 4)
                {
                    Icon(TowerDefenseIcon.Coin, 246, y + 3, 8);
                    ShadowText("+" + TowerDefenseBattle.SellValue(tower, upgrades), 256, y + 3, 6, TextGold);
                    continue;
                }

                TowerUpgradePath path = (TowerUpgradePath)i;
                int max = TowerUpgrades.MaxLevel(path);
                int current = upgrades.Level(path);
                if (path == TowerUpgradePath.Tier)
                    ShadowText(TierName(current) + "/III", 198, y + 3, 5, TextDim);
                else
                {
                    // Pips: filled levels in the path colour.
                    for (int pip = 0; pip < max; pip++)
                        Surface.Rect(190 + pip * 5, y + 5, 4, 4, pip < current ? colors[i] : PanelInsideDark);
                }

                if (upgrades.IsMaxed(path))
                {
                    ShadowText("MAX", 256, y + 3, 6, TextDim);
                    continue;
                }
                int cost = upgrades.Cost(tower, path);
                Icon(TowerDefenseIcon.Coin, 246, y + 3, 8);
                ShadowText(cost.ToString(), 256, y + 3, 6, _Gold >= cost ? TextGold : Color.FromRgb(222, 96, 64));
            }

            ShadowText("KLICK / ENTER  KAUFEN    RECHTSKLICK / BACKSPACE  ZURÜCK", 84, 155, 5, TextDim);
        }

        static readonly Color BurnColor = Color.FromRgb(255, 140, 40);
        static readonly Color PoisonColor = Color.FromRgb(120, 220, 80);
        static readonly Color SlowColor = Color.FromRgb(150, 220, 255);
        static readonly Color ArmorBreakColor = Color.FromRgb(200, 150, 90);

        // Small box below the tower picture: the tower effect, its strength and how long it lasts.
        // Burn and poison grow with the damage upgrade, so the selected upgrade shows its result in green.
        void DrawEffectInfo(Tower tower, TowerUpgrades upgrades, TowerUpgrades preview, double x, double y)
        {
            string name, value, next = null;
            Color color;
            bool timed = true;
            switch (tower.Effect)
            {
                case TowerEffect.Burn:
                case TowerEffect.PoisonDamageOverTime:
                    bool burn = tower.Effect == TowerEffect.Burn;
                    name = burn ? "BRAND" : "GIFT";
                    color = burn ? BurnColor : PoisonColor;
                    value = TowerDefenseBattle.TowerEffectDamage(tower, upgrades) + "/S";
                    next = TowerDefenseBattle.TowerEffectDamage(tower, preview) + "/S";
                    break;
                case TowerEffect.Slow:
                    name = "LANGSAM";
                    color = SlowColor;
                    value = "-" + (int)Math.Round((1 - tower.SlowFactor) * 100) + "% TEMPO";
                    break;
                case TowerEffect.ArmorBreak:
                    name = "RÜSTUNG";
                    color = ArmorBreakColor;
                    value = "-" + tower.EffectStrength;
                    break;
                case TowerEffect.ChainLightning:
                    name = "KETTE";
                    color = SlowColor;
                    value = "x" + tower.MaxChainTargets + " ZIELE";
                    timed = false;
                    break;
                default:
                    return;
            }

            Surface.Rect(x, y, 48, 18, Outline);
            Surface.Rect(x + 1, y + 1, 46, 16, PanelInsideDark);
            ShadowText(name, x + 3, y + 2, 5, color);
            if (timed)
            {
                string duration = Tower.StatusEffectDuration.ToString("0") + "S";
                ShadowText(duration, x + 45 - duration.Length * 2.75, y + 2, 5, TextDim);
            }
            ShadowText(value, x + 3, y + 9, 5, TextLight);
            if (next != null && next != value) ShadowText(">" + next, x + 5 + value.Length * 2.75, y + 9, 5, RangeColor);
        }

        void DrawUpgradeStat(TowerDefenseIcon icon, string value, string next, double x)
        {
            Icon(icon, x, 66, 8);
            ShadowText(value, x + 11, 67, 5, TextLight);
            if (next != value) ShadowText(">" + next, x + 13 + value.Length * 2.75, 67, 5, RangeColor);
        }

        // Tower standing on a pedestal inside a small framed window (48x72).
        void DrawTowerShowcase(Tower tower, double x, double y)
        {
            Surface.Rect(x, y, 48, 72, Outline);
            Surface.Rect(x + 1, y + 1, 46, 70, StoneLight);
            Surface.Rect(x + 2, y + 2, 44, 68, Color.FromRgb(98, 148, 66));
            Surface.Rect(x + 2, y + 2, 44, 40, Color.FromRgb(118, 176, 196));
            Surface.Rect(x + 2, y + 38, 44, 4, Color.FromRgb(150, 196, 206));
            double spotX = x + 24, spotY = y + 58;
            Surface.Sprite(TowerDefenseSprites.Pedestal(), spotX - 16, spotY - 24, 32, 32);
            Surface.Sprite(TowerDefenseSprites.Tower(tower.Name), spotX - 16, spotY - 43, 32, 48);
        }

        void DrawTowerStats(Tower tower, double x, double y)
        {
            DrawStat(TowerDefenseSprites.DamageIcon(tower.DamageType), DamageTypeNames[(int)tower.DamageType], x, y);
            DrawStat(TowerDefenseSprites.Icon(TowerDefenseIcon.Sword), "SCHADEN " + tower.Damage, x, y + 14);
            DrawStat(TowerDefenseSprites.Icon(TowerDefenseIcon.Speed), "TEMPO " + tower.AttackSpeed.ToString("0.0"), x, y + 28);
            DrawStat(TowerDefenseSprites.Icon(TowerDefenseIcon.Range), "REICHW. " + tower.AttackRange.ToString("0.#"), x, y + 42);
            string mode = tower.Effect == TowerEffect.ChainLightning ? "KETTE x" + tower.MaxChainTargets
                : tower.Effect == TowerEffect.Burn ? "BRAND " + tower.EffectDamagePerSecond + "/S"
                : tower.Effect == TowerEffect.PoisonDamageOverTime ? "GIFT " + tower.EffectDamagePerSecond + "/S"
                : tower.Effect == TowerEffect.Slow ? "LANGSAM -" + (int)Math.Round((1 - tower.SlowFactor) * 100) + "%"
                : tower.Effect == TowerEffect.ArmorBreak ? "RÜSTUNG -" + tower.EffectStrength
                : tower.AttackMode == AttackMode.AreaDamage ? "FLÄCHE" : "EINZELZIEL";
            DrawStat(TowerDefenseSprites.Icon(TowerDefenseIcon.Physical), mode, x, y + 56);
        }

        void DrawStat(BitmapSource icon, string text, double x, double y)
        {
            Icon(icon, x, y, 8);
            ShadowText(text, x + 11, y + 1, 5, TextLight);
        }

        // ---------------- shared UI pieces ----------------

        TowerDefensePixelSurface Surface { get { return _PixelSurface; } }

        void Icon(TowerDefenseIcon icon, double x, double y, double size)
        {
            Surface.Sprite(TowerDefenseSprites.Icon(icon), x, y, size, size);
        }

        void Icon(BitmapSource icon, double x, double y, double size)
        {
            Surface.Sprite(icon, x, y, size, size);
        }

        void ShadowText(string text, double x, double y, int size, Color color)
        {
            Surface.Text(text, x + 1, y + 1, size, Outline);
            Surface.Text(text, x, y, size, color);
        }

        // Stone frame with a dark inner field, matching the tower walls.
        void DrawPanel(double x, double y, double width, double height)
        {
            Surface.Rect(x + 2, y + height, width - 2, 2, Color.FromArgb(90, 0, 0, 0));
            Surface.Rect(x, y, width, height, Outline);
            Surface.Rect(x + 1, y + 1, width - 2, height - 2, Stone);
            Surface.Rect(x + 1, y + 1, width - 2, 1, StoneHighlight);
            Surface.Rect(x + 1, y + 1, 1, height - 2, StoneLight);
            Surface.Rect(x + 1, y + height - 2, width - 2, 1, StoneDark);
            Surface.Rect(x + width - 2, y + 1, 1, height - 2, StoneDark);
            for (double bx = x + 6; bx < x + width - 6; bx += 12)
            {
                Surface.Rect(bx, y + 2, 1, 2, StoneDark);
                Surface.Rect(bx + 6, y + height - 4, 1, 2, StoneDark);
            }
            Surface.Rect(x + 4, y + 4, width - 8, height - 8, Outline);
            Surface.Rect(x + 5, y + 5, width - 10, height - 10, PanelInside);
            Surface.Rect(x + 5, y + 5, width - 10, 2, PanelInsideDark);
        }

        void DrawTitle(string title, double y)
        {
            double width = title.Length * 7.2;
            double x = 160 - width / 2;
            ShadowText(title, x, y, 13, TitleGold);
            Surface.Rect(x - 6, y + 17, width + 12, 1, Color.FromRgb(170, 112, 30));
            Surface.Rect(x + 6, y + 19, width - 12, 1, Color.FromRgb(112, 82, 48));
        }

        void DrawMenuEntry(string text, TowerDefenseIcon icon, bool selected, double y, Action hover, Action click)
        {
            AddClickArea(92, y, 136, 17, hover, click);
            DrawSelectionBar(92, y, 136, 17, selected);
            Icon(icon, 98, y, 16);
            ShadowText(text, 120, y + 3, 10, selected ? TitleGold : TextLight);
        }

        void DrawSelectionBar(double x, double y, double width, double height, bool selected)
        {
            if (!selected) return;
            Surface.Rect(x, y, width, height, SelectionEdge);
            Surface.Rect(x + 1, y + 1, width - 2, height - 2, Selection);
            Surface.Rect(x + 1, y + 1, width - 2, 1, Color.FromRgb(150, 110, 62));
        }

        // Bottom bar: control hints on the left, MENUE / ZURUECK buttons for the mouse on the right.
        void DrawFooterBar()
        {
            string hint;
            bool back = true, menu = true;
            switch (_Screen)
            {
                case TowerDefenseScreen.MainMenu:
                    hint = "MAUS / PFEILE  WÄHLEN   KLICK / ENTER  BESTÄTIGEN   ESC  MENÜ";
                    back = false;
                    menu = false;
                    break;
                case TowerDefenseScreen.SlotSelection:
                    hint = "KLICK / ENTER  LADEN   ENTF  LÖSCHEN";
                    break;
                case TowerDefenseScreen.DifficultySelection:
                    hint = "KLICK / ENTER  KARTE STARTEN   RECHTSKLICK  ZURÜCK";
                    break;
                case TowerDefenseScreen.MapSelection:
                    hint = CurrentSlot.IsUnlocked(_MapSelection)
                        ? "KLICK AUF KARTE / ENTER  STARTEN   < >  WECHSELN"
                        : "GESPERRT - SCHAFFE ZUERST DIE VORHERIGE KARTE";
                    break;
                case TowerDefenseScreen.Map:
                    hint = WaveRunning
                        ? "KLICK / PFEILE  PODEST   T  TEMPO   E  GEGNER   ESC  MENÜ"
                        : "LEERTASTE  WELLE STARTEN   ENTER  BAUEN   E  GEGNER";
                    back = false;
                    break;
                default:
                    hint = "KLICK  WÄHLEN   RECHTSKLICK  SCHLIESSEN   ESC  MENÜ";
                    break;
            }
            bool delete = _Screen == TowerDefenseScreen.SlotSelection && !TowerDefenseSaves.Slots[_SlotSelection].IsEmpty;
            if (_Dialog != TowerDefenseDialog.None)
            {
                hint = _Dialog == TowerDefenseDialog.Defeat ? "KLICK / ENTER  AUSWÄHLEN" : "KLICK / ENTER  AUSWÄHLEN   ESC  ABBRECHEN";
                back = menu = delete = false;
            }
            if (_Paused)
            {
                hint = "KLICK / ENTER  AUSWÄHLEN   ESC  WEITERSPIELEN";
                back = menu = delete = false;
            }
            if (_ShowHelp)
            {
                hint = "PFEILE / KLICK  BLÄTTERN   ESC  SCHLIESSEN";
                back = menu = delete = false;
            }

            Surface.Rect(0, 170, DesignWidth, 10, Color.FromArgb(200, 20, 18, 16));
            Surface.Rect(0, 170, DesignWidth, 1, Color.FromArgb(200, 0, 0, 0));
            Surface.Text(hint, 4, 171, 5, TextDim);
            double right = DesignWidth - 2;
            if (menu) right = DrawFooterButton("MENÜ", right, () => { _Paused = true; _PauseSelection = 0; });
            if (back) right = DrawFooterButton("ZURÜCK", right, GoBack);
            if (menu && _Screen == TowerDefenseScreen.Map && _Battle != null)
            {
                right = DrawFooterButton("TEMPO x" + _GameSpeed, right, ToggleSpeed);
                if (_Battle.Phase == TowerDefenseBattlePhase.Building)
                    right = DrawFooterButton("WELLE STARTEN", right, StartNextWave);
            }
            if (delete) DrawFooterButton("LÖSCHEN", right, AskDeleteSlot);
        }

        // Draws a small button that ends at x = right and returns the x where the next button may end.
        double DrawFooterButton(string label, double right, Action click)
        {
            double width = label.Length * 3 + 8, x = right - width, y = 171;
            bool hovered = IsMouseOver(x, y, width, 9);
            AddClickArea(x, y, width, 9, null, click);
            Surface.Rect(x, y, width, 9, Outline);
            Surface.Rect(x + 1, y + 1, width - 2, 7, hovered ? Selection : StoneDark);
            Surface.Rect(x + 1, y + 1, width - 2, 1, hovered ? SelectionEdge : Stone);
            Surface.Text(label, x + 4, y + 1, 5, hovered ? TitleGold : TextLight);
            return x - 3;
        }

        static string GetMultiplierText(TowerDefenseDifficulty difficulty)
        {
            return "x" + TowerDefenseDifficultyRules.GetMultiplier(difficulty).ToString("0.00");
        }
    }
}
