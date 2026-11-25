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
    public enum G10_TowerDefenseDifficulty
    {
        Easy,
        Normal,
        Hard,
        God
    }

    public static class G10_TowerDefenseDifficultyRules
    {
        public static double GetMultiplier(G10_TowerDefenseDifficulty difficulty)
        {
            switch (difficulty)
            {
                case G10_TowerDefenseDifficulty.Easy: return 0.75;
                case G10_TowerDefenseDifficulty.Hard: return 1.35;
                case G10_TowerDefenseDifficulty.God: return 1.8;
                default: return 1.0;
            }
        }

        // How much stronger the castle's shots get per wave (fraction of its base damage of 30).
        // Wave 50: Easy x3,45, Normal x2,7, Hard x1,98, God x1,49 - always less than one upgraded tower.
        public static double CastleGrowthPerWave(G10_TowerDefenseDifficulty difficulty)
        {
            switch (difficulty)
            {
                case G10_TowerDefenseDifficulty.Easy: return 0.05;
                case G10_TowerDefenseDifficulty.Hard: return 0.02;
                case G10_TowerDefenseDifficulty.God: return 0.01;
                default: return 0.035;
            }
        }

        public static int ScaleBaseValue(int baseValue, G10_TowerDefenseDifficulty difficulty)
        {
            if (baseValue < 0) throw new ArgumentOutOfRangeException(nameof(baseValue));
            return Math.Max(1, (int)Math.Round(baseValue * GetMultiplier(difficulty)));
        }
    }

    enum G10_TowerDefenseScreen
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
    enum G10_TowerDefenseDialog
    {
        None,
        ResumeRun,
        DeleteSlot,
        Defeat,
        MapComplete
    }

    // Implements IG10_RoomOwnBackKey: Esc / X open the in-game menu instead of closing the game.
    public sealed partial class G10_RoomTowerDefenseGame : IG10_RoomOwnBackKey
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
        static readonly string[] DifficultyNames = { "Easy", "Normal", "Hard", "God" };
        static readonly string[] DamageTypeNames = { "PHYSISCH", "FEUER", "EIS", "ERDE", "STURM", "GIFT" };
        static readonly string[] TowerNames =
        {
            "Physical Archer", "Fire Mage", "Ice Mage", "Poison Archer",
            "Strom Mage", "Earth Mage", "Flamethrower", "Ice Area Tower"
        };
        static readonly Func<G10_Tower>[] TowerCreators =
        {
            () => new G10_PhysicalArcher(), () => new G10_FireMage(), () => new G10_IceMage(), () => new G10_PoisonArcher(),
            () => new G10_StromMage(), () => new G10_EarthMage(), () => new G10_Flamethrower(), () => new G10_IceAreaTower()
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
        G10_TowerDefenseScreen _Screen;
        int _MenuSelection;
        int _DifficultySelection;
        int _MapSelection;
        int _SelectedSpot;
        int _TowerSelection;
        int _UpgradeSelection;
        int _Gold;
        int _Tick;
        G10_Tower[] _Towers = new G10_Tower[0];
        G10_TowerUpgrades[] _Upgrades = new G10_TowerUpgrades[0];
        G10_TowerDefensePixelSurface _PixelSurface;
        G10_TowerDefenseDifficulty _Difficulty;
        G10_TowerDefenseMapDefinition _Map;
        int _MapIndex = -1;
        string _StatusMessage;
        string _MapSelectionMessage;
        bool _Paused;
        int _PauseSelection;
        int _SlotSelection;
        int _SlotIndex = -1;
        int _Wave = 1;
        int _MainBuildingHealth = 100;
        G10_TowerDefenseDialog _Dialog;
        int _DialogSelection;
        Point? _LastMouse;
        readonly List<ClickArea> _ClickAreas = new List<ClickArea>();
        G10_TowerDefenseBattle _Battle;
        int _GameSpeed = 1;
        // Game seconds left until auto start begins the next wave; 0 = no countdown running.
        double _AutoStartCountdown;
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
                if (_Battle.Phase == G10_TowerDefenseBattlePhase.Running) return _Battle.RemainingEnemies;
                return G10_TowerDefenseWaves.Get(_MapIndex, _Wave, _EndlessSeed).Spawns.Count;
            }
        }
        public int Gold { get { return _Gold; } }
        public string Difficulty { get { return DifficultyNames[(int)_Difficulty]; } }
        public string MapName { get { return _Map == null ? "Keine" : _Map.Name; } }
        public int MainBuildingHealth { get { return _Battle != null ? _Battle.HealthPercent : _MainBuildingHealth; } }
        public bool WaveRunning { get { return _Battle != null && _Battle.Phase == G10_TowerDefenseBattlePhase.Running; } }
        public string Status { get { return _Screen == G10_TowerDefenseScreen.Map ? _StatusMessage : "Auswahl"; } }
        public G10_TowerDefenseDifficulty SelectedDifficulty { get { return _Difficulty; } }
        public G10_TowerDefenseMapDefinition SelectedMap { get { return _Map; } }
        public double EnemyStrengthMultiplier { get { return G10_TowerDefenseDifficultyRules.GetMultiplier(_Difficulty); } }

        public void Start()
        {
            _Exit = false;
            _Screen = G10_TowerDefenseScreen.MainMenu;
            _MenuSelection = 0;
            _DifficultySelection = (int)G10_TowerDefenseDifficulty.Normal;
            _MapSelection = 0;
            _SelectedSpot = 0;
            _TowerSelection = 0;
            _UpgradeSelection = 0;
            _Gold = 500;
            _Tick = 0;
            _Towers = new G10_Tower[0];
            _Upgrades = new G10_TowerUpgrades[0];
            _Difficulty = G10_TowerDefenseDifficulty.Normal;
            _Map = null;
            _MapIndex = -1;
            _StatusMessage = "Bereit";
            _MapSelectionMessage = null;
            _Paused = false;
            _PauseSelection = 0;
            _SlotSelection = 0;
            _SlotIndex = -1;
            _Wave = 1;
            _Endless = false;
            _MainBuildingHealth = 100;
            _Dialog = G10_TowerDefenseDialog.None;
            _DialogSelection = 0;
            _LastMouse = null;
            _ClickAreas.Clear();
            _Battle = null;
            _GameSpeed = 1;
            _AutoStartCountdown = 0;
            _ShowWavePreview = false;
            _Banner = null;
            _BannerTicks = 0;
            _ShowHelp = false;
            _HelpPage = 0;
        }

        public bool IsPaused { get { return _Paused; } }

        public int CurrentMapIndex { get { return _MapIndex; } }

        public int CurrentSlotIndex { get { return _SlotIndex; } }

        G10_TowerDefenseSaveSlot CurrentSlot { get { return G10_TowerDefenseSaves.Slots[_SlotIndex]; } }

        // Called when a wave has been beaten. This is the only checkpoint during a map: the save
        // then holds gold, towers and upgrades as they are at the start of the next wave. Anything
        // changed afterwards is lost when the map is left before that wave is beaten.
        void CompleteWave()
        {
            if (_Map == null) return;
            if (!_Endless && _Wave >= G10_TowerDefenseWaves.WaveCount)
            {
                CompleteCurrentMap();
                return;
            }
            _Battle.Heal(RepairPerWave);
            int bonus = _Battle.CurrentWave.CompletionBonus;
            _Wave++;
            if (_Endless) G10_TowerDefenseSaves.RecordEndlessWave(_SlotIndex, _MapIndex, _Wave - 1);
            SaveRun();
            _StatusMessage = "Welle " + (_Wave - 1) + " geschafft";
            ShowBanner("WELLE " + (_Wave - 1) + " GESCHAFFT  +" + bonus + " GOLD");
            if (AutoStart) _AutoStartCountdown = AutoStartDelay;
        }

        // Pause between a beaten wave and the automatic start of the next one, in game seconds (so it
        // gets shorter with TEMPO x2/x3): 3 game seconds = 2 real seconds at TEMPO x1. Long enough to read
        // the banner and let the last gold drop land.
        const double AutoStartDelay = 3.0;

        bool AutoStart { get { return _SlotIndex >= 0 && CurrentSlot.AutoStart; } }

        void ToggleAutoStart()
        {
            if (_SlotIndex < 0) return;
            CurrentSlot.AutoStart = !CurrentSlot.AutoStart;
            G10_TowerDefenseSaves.Save();
            bool idle = _Battle != null && _Battle.Phase == G10_TowerDefenseBattlePhase.Building;
            _AutoStartCountdown = CurrentSlot.AutoStart && idle ? AutoStartDelay : 0;
            _StatusMessage = CurrentSlot.AutoStart ? "Autostart an" : "Autostart aus";
        }

        // Counts down in game time while the battle runs (menus and the pause menu stop it).
        void StepAutoStart(double seconds)
        {
            if (_AutoStartCountdown <= 0) return;
            if (!AutoStart || _Battle == null || _Battle.Phase != G10_TowerDefenseBattlePhase.Building)
            {
                _AutoStartCountdown = 0;
                return;
            }
            _AutoStartCountdown -= seconds;
            if (_AutoStartCountdown <= 0)
            {
                _AutoStartCountdown = 0;
                StartNextWave();
            }
        }

        // Only a map beaten on Normal, Hard or God unlocks the next one; Easy is for practice.
        static bool UnlocksNextMap(G10_TowerDefenseDifficulty difficulty)
        {
            return difficulty != G10_TowerDefenseDifficulty.Easy;
        }

        // Called when the last wave of the current map has been beaten.
        void CompleteCurrentMap()
        {
            if (_Map == null) return;
            bool countsForUnlock = UnlocksNextMap(_Difficulty);
            _CompletedUnlockedNext = G10_TowerDefenseSaves.CompleteMap(_SlotIndex, _MapIndex, (int)_Difficulty, countsForUnlock);
            _CompletedMap = _MapIndex;
            // The battle stays loaded behind the "map completed" menu, so the endless mode can continue it.
            _MapSelection = _MapIndex;
            _MapSelectionMessage = null;
            _Screen = G10_TowerDefenseScreen.Map;
            _Dialog = G10_TowerDefenseDialog.MapComplete;
            _DialogSelection = 0;
        }

        // Map of the last "map completed" menu and whether that win unlocked the next map.
        int _CompletedMap = -1;
        bool _CompletedUnlockedNext;
        // Endless mode: the map continues after wave 50 until the castle falls.
        bool _Endless;
        // Endless record of this map when the endless run began (to recognise a new record).
        int _EndlessRecordBefore;
        // Random seed of the current endless run: decides which enemies and bosses come after wave 50.
        int _EndlessSeed;

        // Continues the beaten map with wave 51; towers, upgrades and gold stay.
        void StartEndless()
        {
            _Endless = true;
            _EndlessSeed = new Random().Next(1, int.MaxValue);
            _EndlessRecordBefore = CurrentSlot.EndlessRecord(_MapIndex);
            // The record counts all waves of the map: reaching the endless mode means wave 50 is beaten.
            G10_TowerDefenseSaves.RecordEndlessWave(_SlotIndex, _MapIndex, G10_TowerDefenseWaves.WaveCount);
            _Wave = G10_TowerDefenseWaves.WaveCount + 1;
            _Battle.Heal(RepairPerWave);
            SaveRun();
            _StatusMessage = "Endlos-Modus";
            ShowBanner("ENDLOS-MODUS  WELLE " + _Wave);
            if (AutoStart) _AutoStartCountdown = AutoStartDelay;
        }

        // Closes the current map and shows the map selection.
        void LeaveMap()
        {
            _Map = null;
            _Battle = null;
            _Endless = false;
            _MapSelection = Math.Max(0, _MapIndex);
            _Screen = G10_TowerDefenseScreen.MapSelection;
        }

        bool CompletedNextOpen
        {
            get { return _CompletedMap + 1 < G10_TowerDefenseMaps.Maps.Count && CurrentSlot.IsUnlocked(_CompletedMap + 1); }
        }

        // First difficulty not beaten yet on this map (or the last played one when all are done).
        int SuggestedDifficulty(int mapIndex)
        {
            for (int d = 0; d < DifficultyNames.Length; d++)
                if (!CurrentSlot.HasCompleted(mapIndex, d)) return d;
            return (int)_Difficulty;
        }

        // NAECHSTE KARTE: the next map (resume question if it has a saved run, otherwise its difficulty
        // selection). SCHWIERIGKEIT WAEHLEN: difficulty selection of the same map.
        void ActivateMapCompleteItem(string item)
        {
            if (item == "ENDLOS-MODUS")
            {
                StartEndless();
                return;
            }
            LeaveMap();
            if (item == "NÄCHSTE KARTE")
            {
                _MapSelection = _CompletedMap + 1;
                if (CurrentSlot.GetRun(_MapSelection) != null)
                {
                    _Dialog = G10_TowerDefenseDialog.ResumeRun;
                    _DialogSelection = 0;
                    return;
                }
                _DifficultySelection = (int)_Difficulty;
                _Screen = G10_TowerDefenseScreen.DifficultySelection;
            }
            else if (item == "SCHWIERIGKEIT WÄHLEN")
            {
                _MapSelection = _CompletedMap;
                _DifficultySelection = SuggestedDifficulty(_CompletedMap);
                _Screen = G10_TowerDefenseScreen.DifficultySelection;
            }
        }

        // Music for the current situation: menu screens, the map's own song, or the (mini) boss theme
        // while a (mini) boss is on the field. Esc menu, help and dialogs dim it.
        void UpdateMusic()
        {
            G10_TowerDefenseTrack track = G10_TowerDefenseTrack.Menu;
            bool onMap = _Screen == G10_TowerDefenseScreen.Map || _Screen == G10_TowerDefenseScreen.TowerPlacement || _Screen == G10_TowerDefenseScreen.TowerUpgrade;
            if (onMap && _Map != null && _Battle != null)
            {
                track = G10_TowerDefenseSongs.ForMap(_MapIndex);
                if (_Battle.Phase == G10_TowerDefenseBattlePhase.Running && _Battle.Enemies.Exists(e => e.IsBoss))
                    track = _Battle.CurrentWave.IsMiniBoss ? G10_TowerDefenseTrack.MiniBoss : G10_TowerDefenseTrack.Boss;
            }
            G10_TowerDefenseMusic.Update(track, _Paused || _ShowHelp || _Dialog != G10_TowerDefenseDialog.None);
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            _Tick++;
            UpdateMusic();
            if (_BannerTicks > 0) _BannerTicks--;
            HandleMouse();
            foreach (Key key in keys)
            {
                if (_ShowHelp)
                {
                    HandleHelpKey(key);
                    continue;
                }

                if (_Dialog != G10_TowerDefenseDialog.None && !_Paused)
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

                if (key == Key.E && _Screen == G10_TowerDefenseScreen.Map)
                {
                    _ShowWavePreview = !_ShowWavePreview;
                }
                else if (key == Key.Space && _Screen == G10_TowerDefenseScreen.Map)
                {
                    StartNextWave();
                }
                else if (key == Key.T && _Screen == G10_TowerDefenseScreen.Map)
                {
                    ToggleSpeed();
                }
                else if (key == Key.R && _Screen == G10_TowerDefenseScreen.Map)
                {
                    ToggleAutoStart();
                }
                else if (key == Key.Delete && _Screen == G10_TowerDefenseScreen.SlotSelection)
                {
                    AskDeleteSlot();
                }
                else if (_Screen == G10_TowerDefenseScreen.Map &&
                    (G10_RoomKeys.IsLeft(key) || G10_RoomKeys.IsRight(key) ||
                     G10_RoomKeys.IsUp(key) || G10_RoomKeys.IsDown(key)))
                {
                    MoveSpot(G10_RoomKeys.IsLeft(key) ? -1 : G10_RoomKeys.IsRight(key) ? 1 : 0,
                        G10_RoomKeys.IsUp(key) ? -1 : G10_RoomKeys.IsDown(key) ? 1 : 0);
                }
                else if ((_Screen == G10_TowerDefenseScreen.TowerPlacement ||
                    _Screen == G10_TowerDefenseScreen.TowerUpgrade) &&
                    (G10_RoomKeys.IsUp(key) || G10_RoomKeys.IsDown(key)))
                {
                    MoveMenuSelection(G10_RoomKeys.IsDown(key) ? 1 : -1);
                }
                else if ((_Screen == G10_TowerDefenseScreen.MainMenu ||
                    _Screen == G10_TowerDefenseScreen.SlotSelection ||
                    _Screen == G10_TowerDefenseScreen.DifficultySelection ||
                    _Screen == G10_TowerDefenseScreen.MapSelection) &&
                    (G10_RoomKeys.IsUp(key) || G10_RoomKeys.IsDown(key) ||
                     G10_RoomKeys.IsLeft(key) || G10_RoomKeys.IsRight(key)))
                {
                    MoveSelection(G10_RoomKeys.IsDown(key) || G10_RoomKeys.IsRight(key) ? 1 : -1);
                }
                else if (key == Key.Enter || key == Key.Z)
                    ConfirmSelection();
            }
            StepBattle();
        }

        // ---------------- waves ----------------

        void CreateBattle()
        {
            _Battle = new G10_TowerDefenseBattle(_MapIndex, _Map.Routes, _Map.MainBuilding, _Map.TowerSpots,
                _Towers, _Upgrades,
                G10_TowerDefenseDifficultyRules.GetMultiplier(_Difficulty) * G10_TowerDefenseMaps.EnemyStrength(_MapIndex),
                _MainBuildingHealth, MuzzleOffset, gold => _Gold += gold,
                G10_TowerDefenseDifficultyRules.CastleGrowthPerWave(_Difficulty));
            _ShowWavePreview = false;
            _GameSpeed = 1;
        }

        // Projectile start relative to the tower's foot point, from "schuss_ursprung" in sprites.json.
        static Vector MuzzleOffset(G10_Tower tower)
        {
            G10_TowerDefenseSprite sprite = G10_TowerDefenseSprites.ForName(tower.Name);
            Point muzzle = sprite.Muzzle ?? new Point(sprite.Pivot.X, sprite.Pivot.Y - 100);
            return muzzle - sprite.Pivot;
        }

        // The battle only runs on the plain map view; menus, dialogs and the pause menu freeze it.
        void StepBattle()
        {
            if (_Battle == null || _Screen != G10_TowerDefenseScreen.Map || _Paused || _ShowHelp || _Dialog != G10_TowerDefenseDialog.None)
                return;

            // Game time per tick: TEMPO x1 = 1.5 x real time, x2 and x3 double and triple that. The time is
            // split into equal sub-steps of at most one tick (40 ms), so fast tempos stay as exact as x1.
            double gameSeconds = TickSeconds * BaseSpeed * _GameSpeed;
            int steps = (int)Math.Ceiling(gameSeconds / TickSeconds - 1e-9);
            double stepSeconds = gameSeconds / steps;
            for (int step = 0; step < steps; step++)
            {
                StepAutoStart(stepSeconds);
                _Battle.Step(stepSeconds);
                if (_Battle.ConsumeWaveCompleted())
                {
                    CompleteWave();
                    return;
                }
                if (_Battle.Phase == G10_TowerDefenseBattlePhase.Lost)
                {
                    // The castle has fallen: the run is over, its checkpoint is removed.
                    G10_TowerDefenseSaves.DiscardRun(_SlotIndex, _MapIndex);
                    _Dialog = G10_TowerDefenseDialog.Defeat;
                    _DialogSelection = 0;
                    return;
                }
            }
        }

        void StartNextWave()
        {
            if (_Battle == null || _Battle.Phase != G10_TowerDefenseBattlePhase.Building) return;
            _Battle.StartWave(_Wave, _EndlessSeed);
            G10_TowerDefenseWave wave = _Battle.CurrentWave;
            _StatusMessage = "Welle " + _Wave + " läuft";
            if (wave.Boss != null)
                ShowBanner((wave.IsMiniBoss ? "MINIBOSS: " : "BOSS: ") + wave.Boss.ToUpperInvariant());
            else
                ShowBanner(_Endless ? "ENDLOS  WELLE " + _Wave : "WELLE " + _Wave + " / " + G10_TowerDefenseWaves.WaveCount);
        }

        // Game speed of TEMPO x1 relative to real time (the middle between the former x1 and x2).
        // TEMPO x2 / x3 are 2 / 3 times this value.
        const double BaseSpeed = 1.5;

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
                _PixelSurface = new G10_TowerDefensePixelSurface(DesignWidth, DesignHeight, PixelScale);
                canvas.Tag = _PixelSurface;
                canvas.Children.Add(_PixelSurface.Image);
            }

            _PixelSurface.Begin();
            _ClickAreas.Clear();
            switch (_Screen)
            {
                case G10_TowerDefenseScreen.MainMenu:
                    DrawMainMenu();
                    break;
                case G10_TowerDefenseScreen.SlotSelection:
                    DrawSlotSelection();
                    break;
                case G10_TowerDefenseScreen.DifficultySelection:
                    DrawDifficultySelection();
                    break;
                case G10_TowerDefenseScreen.MapSelection:
                    DrawMapSelection();
                    break;
                case G10_TowerDefenseScreen.TowerPlacement:
                    DrawMap();
                    DrawTowerPlacementMenu();
                    break;
                case G10_TowerDefenseScreen.TowerUpgrade:
                    DrawMap();
                    DrawTowerUpgradeMenu();
                    break;
                default:
                    DrawMap();
                    break;
            }
            DrawFooterBar();
            if (_Dialog != G10_TowerDefenseDialog.None) DrawDialog();
            if (_Paused) DrawPauseMenu();
            if (_ShowHelp) DrawHelp();
            _PixelSurface.End();
        }

        // ---------------- save slots ----------------

        void SelectSlot(int slotIndex)
        {
            _SlotIndex = slotIndex;
            G10_TowerDefenseSaveSlot slot = CurrentSlot;
            _MapSelection = slot.LastMap >= 0 ? slot.LastMap : slot.UnlockedMaps - 1;
            _MapSelectionMessage = null;
            _Screen = G10_TowerDefenseScreen.MapSelection;
        }

        void AskDeleteSlot()
        {
            if (G10_TowerDefenseSaves.Slots[_SlotSelection].IsEmpty) return;
            _Dialog = G10_TowerDefenseDialog.DeleteSlot;
            _DialogSelection = 0;
        }

        // Stores the running map in the current slot. Enemies are not stored; the wave restarts on resume.
        void SaveRun()
        {
            if (_Map == null || _SlotIndex < 0) return;
            G10_TowerDefenseRunState run = new G10_TowerDefenseRunState
            {
                Difficulty = (int)_Difficulty,
                Wave = _Wave,
                Endless = _Endless,
                EndlessSeed = _EndlessSeed,
                Gold = _Gold,
                MainBuildingHealth = MainBuildingHealth,
                Towers = new string[_Towers.Length],
                TowerLevels = null
            };
            for (int i = 0; i < _Towers.Length; i++)
                run.Towers[i] = _Towers[i] == null ? null : _Towers[i].Name;
            run.SetUpgrades(_Upgrades);
            G10_TowerDefenseSaves.StoreRun(_SlotIndex, _MapIndex, run);
        }

        static G10_Tower CreateTower(string name)
        {
            int index = Array.IndexOf(TowerNames, name);
            if (index < 0) throw new InvalidDataException("Unbekannter Turm im Spielstand: " + name);
            return TowerCreators[index]();
        }

        // ---------------- dialogs ----------------

        string[] DialogItems()
        {
            if (_Dialog == G10_TowerDefenseDialog.ResumeRun) return new[] { "WEITERSPIELEN", "NEU BEGINNEN", "ABBRECHEN" };
            // After an endless run the difficulty choice is offered as well, like after wave 50.
            if (_Dialog == G10_TowerDefenseDialog.Defeat)
                return _Endless
                    ? new[] { "NEUSTARTEN", "SCHWIERIGKEIT WÄHLEN", "ANDERE KARTE", "HAUPTMENÜ" }
                    : new[] { "NEUSTARTEN", "ANDERE KARTE", "HAUPTMENÜ" };
            if (_Dialog == G10_TowerDefenseDialog.MapComplete)
                return CompletedNextOpen
                    ? new[] { "NÄCHSTE KARTE", "SCHWIERIGKEIT WÄHLEN", "ENDLOS-MODUS" }
                    : new[] { "SCHWIERIGKEIT WÄHLEN", "KARTENAUSWAHL", "ENDLOS-MODUS" };
            return new[] { "NEIN", "JA, LÖSCHEN" };
        }

        // Defeat and "map completed" cannot be dismissed: the player has to choose how to go on.
        void CloseDialog()
        {
            if (_Dialog != G10_TowerDefenseDialog.Defeat && _Dialog != G10_TowerDefenseDialog.MapComplete) _Dialog = G10_TowerDefenseDialog.None;
        }

        void HandleDialogKey(Key key)
        {
            int count = DialogItems().Length;
            if (key == Key.Escape || key == Key.X || key == Key.Back)
                CloseDialog();
            else if (G10_RoomKeys.IsUp(key) || G10_RoomKeys.IsDown(key))
                _DialogSelection = (_DialogSelection + (G10_RoomKeys.IsDown(key) ? 1 : -1) + count) % count;
            else if (key == Key.Enter || key == Key.Z)
                ActivateDialogItem(_DialogSelection);
        }

        void ActivateDialogItem(int index)
        {
            G10_TowerDefenseDialog dialog = _Dialog;
            string item = DialogItems()[index];
            _Dialog = G10_TowerDefenseDialog.None;
            if (dialog == G10_TowerDefenseDialog.MapComplete)
            {
                ActivateMapCompleteItem(item);
            }
            else if (dialog == G10_TowerDefenseDialog.ResumeRun)
            {
                if (index == 0) ResumeMap(_MapSelection);
                else if (index == 1) _Screen = G10_TowerDefenseScreen.DifficultySelection;
            }
            else if (dialog == G10_TowerDefenseDialog.Defeat)
            {
                // The run is over (its save was removed): start again at wave 1, pick another map or quit.
                if (item == "NEUSTARTEN") StartMap(_MapIndex);
                else if (item == "SCHWIERIGKEIT WÄHLEN")
                {
                    LeaveMap();
                    _DifficultySelection = SuggestedDifficulty(_MapIndex);
                    _Screen = G10_TowerDefenseScreen.DifficultySelection;
                }
                else if (item == "ANDERE KARTE") LeaveMap();
                else ReturnToMainMenu();
            }
            else if (index == 1)
            {
                G10_TowerDefenseSaves.Delete(_SlotSelection);
            }
        }

        void DrawDialog()
        {
            _ClickAreas.Clear();
            string[] items = DialogItems();
            string title, info, highlight = null;
            G10_TowerDefenseIcon[] icons;
            if (_Dialog == G10_TowerDefenseDialog.MapComplete)
            {
                title = "KARTE ABGESCHLOSSEN";
                info = G10_TowerDefenseMaps.Maps[_CompletedMap].Name.ToUpperInvariant() + " AUF " +
                    DifficultyNames[(int)_Difficulty].ToUpperInvariant() + " GESCHAFFT";
                bool hasNext = _CompletedMap + 1 < G10_TowerDefenseMaps.Maps.Count;
                if (_CompletedUnlockedNext)
                    highlight = "NEUE KARTE FREIGESCHALTET: " + G10_TowerDefenseMaps.Maps[_CompletedMap + 1].Name.ToUpperInvariant();
                else if (!hasNext)
                    highlight = CurrentSlot.HasCompletedAll(_CompletedMap) ? "ALLE KARTEN UND SCHWIERIGKEITEN GESCHAFFT!" : "ALLE KARTEN GESCHAFFT!";
                else if (!CompletedNextOpen)
                    highlight = "NÄCHSTE KARTE AB NORMAL";
                else if (CurrentSlot.HasCompletedAll(_CompletedMap))
                    highlight = "ALLE SCHWIERIGKEITEN GESCHAFFT!";
                icons = CompletedNextOpen
                    ? new[] { G10_TowerDefenseIcon.Star, G10_TowerDefenseIcon.Skull, G10_TowerDefenseIcon.Upgrade }
                    : new[] { G10_TowerDefenseIcon.Skull, G10_TowerDefenseIcon.Wave, G10_TowerDefenseIcon.Upgrade };
            }
            else if (_Dialog == G10_TowerDefenseDialog.ResumeRun)
            {
                G10_TowerDefenseRunState run = CurrentSlot.GetRun(_MapSelection);
                int towers = 0;
                foreach (string tower in run.Towers) if (tower != null) towers++;
                title = "WEITERSPIELEN?";
                info = G10_TowerDefenseMaps.Maps[_MapSelection].Name.ToUpperInvariant() + (run.Endless ? "  ENDLOS" : "") + "  WELLE " + run.Wave +
                    "  GOLD " + run.Gold + "  TÜRME " + towers + "  " + DifficultyNames[run.Difficulty].ToUpperInvariant();
                icons = new[] { G10_TowerDefenseIcon.Upgrade, G10_TowerDefenseIcon.Sword, G10_TowerDefenseIcon.Wave };
            }
            else if (_Dialog == G10_TowerDefenseDialog.Defeat)
            {
                title = "BURG ZERSTÖRT";
                string mapName = _Map != null ? _Map.Name.ToUpperInvariant() : "";
                if (_Endless)
                {
                    int record = CurrentSlot.EndlessRecord(_MapIndex);
                    info = "ENDLOS-MODUS  " + mapName + "  WELLE " + _Wave + " VERLOREN";
                    if (record > 0)
                        highlight = record > _EndlessRecordBefore
                            ? "NEUER REKORD: BIS WELLE " + record + " GESCHAFFT!"
                            : "REKORD: BIS WELLE " + record + " GESCHAFFT";
                }
                else
                {
                    info = mapName + "  WELLE " + _Wave + " VERLOREN";
                }
                icons = _Endless
                    ? new[] { G10_TowerDefenseIcon.Upgrade, G10_TowerDefenseIcon.Skull, G10_TowerDefenseIcon.Wave, G10_TowerDefenseIcon.Heart }
                    : new[] { G10_TowerDefenseIcon.Upgrade, G10_TowerDefenseIcon.Wave, G10_TowerDefenseIcon.Heart };
            }
            else
            {
                title = "LÖSCHEN?";
                info = "SPIELSTAND " + (_SlotSelection + 1) + " WIRKLICH LÖSCHEN?";
                icons = new[] { G10_TowerDefenseIcon.Heart, G10_TowerDefenseIcon.Skull };
            }

            Surface.Rect(0, 0, DesignWidth, DesignHeight, Color.FromArgb(150, 0, 0, 0));
            double extra = highlight != null ? 9 : 0;
            double height = 52 + extra + items.Length * 21;
            double top = 88 - height / 2;
            DrawPanel(54, top, 212, height);
            AddClickArea(54, top, 212, height, null, null);
            DrawTitle(title, top + 7);
            ShadowText(info, 160 - info.Length * 1.65, top + 30, 5, TextLight);
            if (highlight != null) ShadowText(highlight, 160 - highlight.Length * 1.65, top + 39, 5, TitleGold);
            for (int i = 0; i < items.Length; i++)
            {
                int index = i;
                DrawMenuEntry(items[i], icons[i], _DialogSelection == i, top + 42 + extra + i * 21,
                    () => _DialogSelection = index, () => ActivateDialogItem(index));
            }
        }

        // ---------------- pause menu ----------------

        string[] PauseItems()
        {
            return _Screen == G10_TowerDefenseScreen.MainMenu
                ? new[] { "FORTSETZEN", "ANLEITUNG", "SPIEL VERLASSEN" }
                : new[] { "FORTSETZEN", "ANLEITUNG", "HAUPTMENÜ", "SPIEL VERLASSEN" };
        }

        void HandlePauseKey(Key key)
        {
            int count = PauseItems().Length;
            if (G10_RoomKeys.IsUp(key) || G10_RoomKeys.IsDown(key))
                _PauseSelection = (_PauseSelection + (G10_RoomKeys.IsDown(key) ? 1 : -1) + count) % count;
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
            _Dialog = G10_TowerDefenseDialog.None;
            _Endless = false;
            _Screen = G10_TowerDefenseScreen.MainMenu;
            _MenuSelection = 0;
            _Map = null;
            _Battle = null;
            _MapIndex = -1;
            _Towers = new G10_Tower[0];
            _Upgrades = new G10_TowerUpgrades[0];
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
            Dictionary<string, G10_TowerDefenseIcon> icons = new Dictionary<string, G10_TowerDefenseIcon>
            {
                { "FORTSETZEN", G10_TowerDefenseIcon.Upgrade }, { "ANLEITUNG", G10_TowerDefenseIcon.Range },
                { "HAUPTMENÜ", G10_TowerDefenseIcon.Star }, { "SPIEL VERLASSEN", G10_TowerDefenseIcon.Wave }
            };
            for (int i = 0; i < items.Length; i++)
            {
                int index = i;
                G10_TowerDefenseIcon icon = icons[items[i]];
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

            foreach (G10_TowerDefenseClick click in _PixelSurface.DrainClicks())
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
                else if (_Dialog != G10_TowerDefenseDialog.None) CloseDialog();
                else if (_Screen == G10_TowerDefenseScreen.TowerPlacement || _Screen == G10_TowerDefenseScreen.TowerUpgrade)
                    _Screen = G10_TowerDefenseScreen.Map;
            }
        }

        // Right click: closes the topmost menu, but never leaves a running map by accident.
        void MouseBack()
        {
            if (_ShowHelp) CloseHelp();
            else if (_Paused) _Paused = false;
            else if (_Dialog != G10_TowerDefenseDialog.None) CloseDialog();
            else if (_Screen != G10_TowerDefenseScreen.Map && _Screen != G10_TowerDefenseScreen.MainMenu) GoBack();
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
            if (_Screen == G10_TowerDefenseScreen.MainMenu)
                _MenuSelection = (_MenuSelection + direction + 3) % 3;
            else if (_Screen == G10_TowerDefenseScreen.SlotSelection)
                _SlotSelection = (_SlotSelection + direction + G10_TowerDefenseSaves.SlotCount) % G10_TowerDefenseSaves.SlotCount;
            else if (_Screen == G10_TowerDefenseScreen.DifficultySelection)
                _DifficultySelection = (_DifficultySelection + direction + DifficultyNames.Length) % DifficultyNames.Length;
            else if (_Screen == G10_TowerDefenseScreen.MapSelection)
            {
                _MapSelection = (_MapSelection + direction + G10_TowerDefenseMaps.Maps.Count) % G10_TowerDefenseMaps.Maps.Count;
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
            if (_Screen == G10_TowerDefenseScreen.TowerPlacement)
                _TowerSelection = (_TowerSelection + direction + TowerNames.Length) % TowerNames.Length;
            else
                _UpgradeSelection = (_UpgradeSelection + direction + 5) % 5;
        }

        void ConfirmSelection()
        {
            _RangeSpot = -1;
            if (_Screen == G10_TowerDefenseScreen.MainMenu)
            {
                if (_MenuSelection == 0)
                {
                    _SlotSelection = Math.Max(0, _SlotIndex);
                    _Screen = G10_TowerDefenseScreen.SlotSelection;
                }
                else if (_MenuSelection == 1) OpenHelp();
                else _Exit = true;
            }
            else if (_Screen == G10_TowerDefenseScreen.SlotSelection)
            {
                SelectSlot(_SlotSelection);
            }
            else if (_Screen == G10_TowerDefenseScreen.MapSelection)
            {
                if (!CurrentSlot.IsUnlocked(_MapSelection))
                {
                    _MapSelectionMessage = "ERST " + G10_TowerDefenseMaps.Maps[_MapSelection - 1].Name.ToUpperInvariant() + " AB NORMAL SCHAFFEN";
                    return;
                }
                if (CurrentSlot.GetRun(_MapSelection) != null)
                {
                    _Dialog = G10_TowerDefenseDialog.ResumeRun;
                    _DialogSelection = 0;
                    return;
                }
                _Screen = G10_TowerDefenseScreen.DifficultySelection;
            }
            else if (_Screen == G10_TowerDefenseScreen.DifficultySelection)
            {
                _Difficulty = (G10_TowerDefenseDifficulty)_DifficultySelection;
                StartMap(_MapSelection);
            }
            else if (_Screen == G10_TowerDefenseScreen.Map)
            {
                _TowerSelection = 0;
                _UpgradeSelection = 0;
                _Screen = _Towers[_SelectedSpot] == null
                    ? G10_TowerDefenseScreen.TowerPlacement
                    : G10_TowerDefenseScreen.TowerUpgrade;
            }
            else if (_Screen == G10_TowerDefenseScreen.TowerPlacement)
            {
                BuildTower();
            }
            else if (_Screen == G10_TowerDefenseScreen.TowerUpgrade)
            {
                if (_UpgradeSelection < 4) UpgradeTower(RowPath(_UpgradeSelection, _Upgrades[_SelectedSpot]));
                else SellTower();
            }
        }

        void StartMap(int mapIndex)
        {
            _MapIndex = mapIndex;
            _Map = G10_TowerDefenseMaps.Maps[mapIndex];
            _Towers = new G10_Tower[_Map.TowerSpots.Count];
            _Upgrades = new G10_TowerUpgrades[_Map.TowerSpots.Count];
            _Gold = 500;
            _Wave = 1;
            _Endless = false;
            _MainBuildingHealth = 100;
            _SelectedSpot = 0;
            _StatusMessage = "Bereit";
            _MapSelectionMessage = null;
            _Screen = G10_TowerDefenseScreen.Map;
            CreateBattle();
            SaveRun();
        }

        // Continues a saved map. The saved wave starts again from its beginning.
        void ResumeMap(int mapIndex)
        {
            G10_TowerDefenseRunState run = CurrentSlot.GetRun(mapIndex);
            _MapIndex = mapIndex;
            _Map = G10_TowerDefenseMaps.Maps[mapIndex];
            _Difficulty = (G10_TowerDefenseDifficulty)run.Difficulty;
            _Gold = run.Gold;
            _Wave = run.Wave;
            _Endless = run.Endless;
            _EndlessSeed = run.EndlessSeed;
            _EndlessRecordBefore = CurrentSlot.EndlessRecord(mapIndex);
            _MainBuildingHealth = run.MainBuildingHealth;
            _Towers = new G10_Tower[_Map.TowerSpots.Count];
            _Upgrades = new G10_TowerUpgrades[_Map.TowerSpots.Count];
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
            _Screen = G10_TowerDefenseScreen.Map;
        }

        void GoBack()
        {
            if (_Screen == G10_TowerDefenseScreen.SlotSelection) _Screen = G10_TowerDefenseScreen.MainMenu;
            else if (_Screen == G10_TowerDefenseScreen.DifficultySelection) _Screen = G10_TowerDefenseScreen.MapSelection;
            else if (_Screen == G10_TowerDefenseScreen.MapSelection) _Screen = G10_TowerDefenseScreen.SlotSelection;
            else if (_Screen == G10_TowerDefenseScreen.Map)
            {
                _Map = null;
                _Battle = null;
                _Screen = G10_TowerDefenseScreen.MapSelection;
            }
            else if (_Screen == G10_TowerDefenseScreen.TowerPlacement || _Screen == G10_TowerDefenseScreen.TowerUpgrade)
                _Screen = G10_TowerDefenseScreen.Map;
        }

        void BuildTower()
        {
            G10_Tower tower = TowerCreators[_TowerSelection]();
            if (_Gold < tower.Cost)
            {
                _StatusMessage = "Zu wenig Gold";
                return;
            }

            _Gold -= tower.Cost;
            _Towers[_SelectedSpot] = tower;
            _Upgrades[_SelectedSpot] = new G10_TowerUpgrades();
            _StatusMessage = tower.Name + " gebaut";
            _Screen = G10_TowerDefenseScreen.Map;
        }

        // The upgrade menu stays open, so several upgrades can be bought in a row.
        void UpgradeTower(G10_TowerUpgradePath path)
        {
            G10_Tower tower = _Towers[_SelectedSpot];
            G10_TowerUpgrades upgrades = _Upgrades[_SelectedSpot];
            if (!upgrades.CanBuy(path))
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
            _StatusMessage = path == G10_TowerUpgradePath.Mastery ? tower.Name + " Meister " + upgrades.Mastery
                : path == G10_TowerUpgradePath.Tier
                ? tower.Name + " Stufe " + TierName(upgrades.Tier)
                : UpgradeNames[(int)path] + " verbessert";
        }

        static readonly string[] UpgradeNames = { "Schaden", "Reichweite", "Tempo", "Turmstufe", "Meisterschaft" };

        // Row 3 of the upgrade menu is the tier upgrade and turns into the mastery once everything is bought.
        static G10_TowerUpgradePath RowPath(int row, G10_TowerUpgrades upgrades)
        {
            return row == 3 && upgrades.IsComplete ? G10_TowerUpgradePath.Mastery : (G10_TowerUpgradePath)row;
        }

        static string TierName(int tier)
        {
            return tier == 3 ? "III" : tier == 2 ? "II" : "I";
        }

        void SellTower()
        {
            G10_Tower tower = _Towers[_SelectedSpot];
            _Gold += G10_TowerDefenseBattle.SellValue(tower, _Upgrades[_SelectedSpot]);
            _Towers[_SelectedSpot] = null;
            _Upgrades[_SelectedSpot] = null;
            _StatusMessage = "Turm verkauft";
            _Screen = G10_TowerDefenseScreen.Map;
        }

        // ---------------- menu screens ----------------

        void DrawMainMenu()
        {
            DrawMenuBackground();
            DrawPanel(70, 30, 180, 108);
            DrawTitle("TOWER DEFENSE", 40);
            ShadowText("BAUE TÜRME. HALTE DIE WELLEN AUF.", 86, 64, 6, TextDim);
            DrawMenuEntry("SPIELEN", G10_TowerDefenseIcon.Sword, _MenuSelection == 0, 76,
                () => _MenuSelection = 0, ConfirmSelection);
            DrawMenuEntry("ANLEITUNG", G10_TowerDefenseIcon.Range, _MenuSelection == 1, 95,
                () => _MenuSelection = 1, ConfirmSelection);
            DrawMenuEntry("SPIEL VERLASSEN", G10_TowerDefenseIcon.Wave, _MenuSelection == 2, 114,
                () => _MenuSelection = 2, ConfirmSelection);
            DrawParade();
        }

        void DrawSlotSelection()
        {
            DrawMenuBackground();
            DrawPanel(36, 16, 248, 146);
            DrawTitle("SPIELSTAND WÄHLEN", 24);
            for (int i = 0; i < G10_TowerDefenseSaves.SlotCount; i++)
                DrawSlotCard(i, 48, 50 + i * 35);
        }

        void DrawSlotCard(int slotIndex, double x, double y)
        {
            const double width = 224, height = 31;
            G10_TowerDefenseSaveSlot slot = G10_TowerDefenseSaves.Slots[slotIndex];
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

            string detail = "KARTEN " + slot.UnlockedMaps + "/" + G10_TowerDefenseMaps.Maps.Count + " FREI";
            if (slot.LastMap >= 0)
            {
                detail += "   ZULETZT " + G10_TowerDefenseMaps.Maps[slot.LastMap].Name.ToUpperInvariant();
                G10_TowerDefenseRunState run = slot.GetRun(slot.LastMap);
                if (run != null) detail += " - WELLE " + run.Wave;
            }
            ShadowText(detail, x + 26, y + 18, 5, TextLight);

            for (int map = 0; map < G10_TowerDefenseMaps.Maps.Count; map++)
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
                    Icon(G10_TowerDefenseIcon.Skull, 160 + skull * 10, y + 3, 8);
                ShadowText(GetMultiplierText((G10_TowerDefenseDifficulty)i), 210, y + 3, 7, TextGold);
                if (CurrentSlot.HasCompleted(_MapSelection, i))
                {
                    Surface.Rect(236, y + 3.5, 8, 8, Outline);
                    Surface.Rect(236.5, y + 4, 7, 7, Color.FromRgb(30, 70, 34));
                    DrawCheck(237, y + 4.5, 6, CheckGreen);
                }
            }
            const string note = "NUR AB NORMAL WIRD DIE NÄCHSTE KARTE FREIGESCHALTET";
            ShadowText(note, 72, 61, 5, TextGold);
            DrawParade();
        }

        void DrawMapSelection()
        {
            DrawMenuBackground();
            DrawPanel(56, 14, 208, 146);
            DrawTitle("KARTE AUSWÄHLEN", 21);
            G10_TowerDefenseMapDefinition map = G10_TowerDefenseMaps.Maps[_MapSelection];
            bool unlocked = CurrentSlot.IsUnlocked(_MapSelection);
            G10_TowerDefenseRunState savedRun = CurrentSlot.GetRun(_MapSelection);

            // Whole map at 7/16 size (140 x 77 units) with its castle.
            const double previewX = 90, previewY = 45, previewWidth = 140, scale = previewWidth / DesignWidth;
            double previewHeight = UX(G10_TowerDefenseMaps.MapHeight) * scale;
            AddClickArea(previewX - 2, previewY - 2, previewWidth + 4, previewHeight + 4, null, ConfirmSelection);
            Surface.Rect(previewX - 2, previewY - 2, previewWidth + 4, previewHeight + 4, Outline);
            Surface.Rect(previewX - 1, previewY - 1, previewWidth + 2, previewHeight + 2, unlocked ? StoneHighlight : StoneDark);
            Surface.Sprite(G10_TowerDefenseSprites.MapLayer(map.Id, "komplett"), previewX, previewY, previewWidth, previewHeight);
            DrawMainBuilding(map, 100, scale, previewX, previewY);
            if (!unlocked)
            {
                Surface.Rect(previewX, previewY, previewWidth, previewHeight, Color.FromArgb(175, 16, 14, 20));
                Icon(G10_TowerDefenseIcon.Lock, previewX + previewWidth / 2 - 16, previewY + 14, 32);
                ShadowText("GESPERRT", previewX + previewWidth / 2 - 22, previewY + 52, 8, TextLight);
                string need = G10_TowerDefenseMaps.Maps[_MapSelection - 1].Name.ToUpperInvariant() + " AB NORMAL SCHAFFEN";
                ShadowText(need, previewX + previewWidth / 2 - need.Length * 1.375, previewY + 64, 5, TextDim);
            }
            else if (savedRun != null)
            {
                string saved = "GESPEICHERT: " + (savedRun.Endless ? "ENDLOS " : "") + "WELLE " + savedRun.Wave;
                double badgeWidth = saved.Length * 2.75 + 18;
                Surface.Rect(previewX + 2, previewY + 2, badgeWidth, 11, Outline);
                Surface.Rect(previewX + 3, previewY + 3, badgeWidth - 2, 9, Selection);
                Icon(G10_TowerDefenseIcon.Wave, previewX + 4, previewY + 3, 8);
                Surface.Text(saved, previewX + 14, previewY + 3, 5, TitleGold);
            }

            if (unlocked) DrawCompletion(_MapSelection, previewX, previewY, previewWidth, previewHeight);

            DrawArrowButton("<", 60, 74, () => MoveSelection(-1));
            DrawArrowButton(">", 244, 74, () => MoveSelection(1));

            string title = (_MapSelection + 1) + "/" + G10_TowerDefenseMaps.Maps.Count + "  " + map.Name.ToUpperInvariant();
            ShadowText(title, 160 - title.Length * 2.75, 126, 10, unlocked ? TitleGold : TextDim);
            double lineWidth = (_MapSelection + 1) * 9 + 4 + map.Description.Length * 3.3;
            double lineX = 160 - lineWidth / 2;
            for (int skull = 0; skull <= _MapSelection; skull++)
                Icon(G10_TowerDefenseIcon.Skull, lineX + skull * 9, 140, 8);
            ShadowText(map.Description.ToUpperInvariant(), lineX + (_MapSelection + 1) * 9 + 4, 141, 6, TextLight);
            DrawMapProgressDots(150);

            if (_MapSelectionMessage != null)
            {
                double width = _MapSelectionMessage.Length * 4.4 + 12;
                Surface.Rect(160 - width / 2, 100, width, 14, Outline);
                Surface.Rect(160 - width / 2 + 1, 101, width - 2, 12, Selection);
                ShadowText(_MapSelectionMessage, 160 - width / 2 + 6, 102, 8, TitleGold);
            }
        }

        static readonly Color CheckGreen = Color.FromRgb(96, 214, 96);

        // Band at the bottom of the map preview: the four difficulties with a green check mark when the map
        // was beaten on them, plus a big star in the top right corner once all four are done.
        void DrawCompletion(int mapIndex, double x, double y, double width, double height)
        {
            const double bandHeight = 10;
            double bandY = y + height - bandHeight;
            Surface.Rect(x, bandY, width, bandHeight, Color.FromArgb(200, 20, 18, 16));
            double cell = width / DifficultyNames.Length;
            for (int d = 0; d < DifficultyNames.Length; d++)
            {
                bool done = CurrentSlot.HasCompleted(mapIndex, d);
                double cx = x + d * cell + 3;
                Surface.Rect(cx, bandY + 2, 6, 6, Outline);
                Surface.Rect(cx + 0.5, bandY + 2.5, 5, 5, done ? Color.FromRgb(30, 70, 34) : PanelInsideDark);
                if (done) DrawCheck(cx + 0.5, bandY + 2.5, 5, CheckGreen);
                ShadowText(DifficultyNames[d].ToUpperInvariant(), cx + 8, bandY + 2, 5, done ? TextLight : TextDim);
            }

            int record = CurrentSlot.EndlessRecord(mapIndex);
            if (record > 0)
            {
                string text = "REKORD: BIS WELLE " + record;
                double plateWidth = text.Length * 2.75 + 6;
                Surface.Rect(x, bandY - 9, plateWidth, 9, Color.FromArgb(200, 20, 18, 16));
                ShadowText(text, x + 3, bandY - 8, 5, TitleGold);
            }

            if (!CurrentSlot.HasCompletedAll(mapIndex)) return;
            double sx = x + width - 17, sy = y + 2;
            Surface.Rect(sx, sy, 15, 15, Outline);
            Surface.Rect(sx + 0.5, sy + 0.5, 14, 14, Selection);
            Icon(G10_TowerDefenseIcon.Star, sx + 1.5, sy + 1.5, 12);
        }

        // Pixel check mark in a square of the given size.
        static readonly string[] CheckPixels = { "......#", ".....##", "#...##.", "##.##..", ".###...", "..#...." };

        void DrawCheck(double x, double y, double size, Color color)
        {
            double px = size / 7;
            for (int row = 0; row < CheckPixels.Length; row++)
                for (int col = 0; col < 7; col++)
                    if (CheckPixels[row][col] == '#') Surface.Rect(x + col * px, y + 0.5 * px + row * px, px, px, color);
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
            int count = G10_TowerDefenseMaps.Maps.Count;
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

        // ---------------- coordinates and sprites ----------------

        // The map images are 1920x1056 pixels and start right below the 48 px HUD bar. The lowest 24 px (only
        // decoration, every path ends above y = 960) are cut off by the screen edge.
        const double MapTopUnits = HudHeight;
        const double Px = G10_TowerDefenseSprites.PixelsPerUnit;

        // Map pixels to logical UI units.
        static double UX(double mapX) { return mapX / Px; }
        static double UY(double mapY) { return MapTopUnits + mapY / Px; }

        // Animation clock for idle loops (castle, towers, parade), independent of the game speed.
        double ClockSeconds { get { return _Tick * TickSeconds; } }

        // Draws one frame with the sprite's pivot (foot point) at (x, y) in logical units.
        // scale 1 = native 1080p pixels.
        void DrawSprite(G10_TowerDefenseSprite sprite, BitmapSource frame, double x, double y, bool flip,
            double scale = 1, double flash = 0, double opacity = 1)
        {
            double width = sprite.FrameWidth / Px * scale, height = sprite.FrameHeight / Px * scale;
            double pivotX = sprite.Pivot.X / Px * scale, pivotY = sprite.Pivot.Y / Px * scale;
            double left = flip ? x - (width - pivotX) : x - pivotX;
            if (opacity < 1) Surface.PushOpacity(opacity);
            Surface.Sprite(frame, left, y - pivotY, width, height, flip, 0, flash);
            if (opacity < 1) Surface.PopOpacity();
        }

        // Draws a frame centred on (x, y), optionally rotated (degrees).
        void DrawCentered(G10_TowerDefenseSprite sprite, BitmapSource frame, double x, double y, double rotation, double scale = 1)
        {
            double width = sprite.FrameWidth / Px * scale, height = sprite.FrameHeight / Px * scale;
            Surface.Sprite(frame, x - width / 2, y - height / 2, width, height, false, rotation, 0);
        }

        // ---------------- menu background ----------------

        // Map behind the menus: in the map and difficulty selection the selected map (so after "next map"
        // its own landscape shows), otherwise the first map.
        void DrawMenuBackground()
        {
            bool choosing = _Screen == G10_TowerDefenseScreen.MapSelection || _Screen == G10_TowerDefenseScreen.DifficultySelection;
            int index = choosing ? Math.Max(0, Math.Min(G10_TowerDefenseMaps.Maps.Count - 1, _MapSelection)) : 0;
            G10_TowerDefenseMapDefinition map = G10_TowerDefenseMaps.Maps[index];
            Surface.Rect(0, 0, DesignWidth, DesignHeight, Color.FromRgb(30, 40, 26));
            Surface.Sprite(G10_TowerDefenseSprites.MapLayer(map.Id, "komplett"), 0, 0,
                UX(G10_TowerDefenseMaps.MapWidth), UX(G10_TowerDefenseMaps.MapHeight));
            DrawMainBuilding(map, 100, 1, 0, 0);
            Surface.Rect(0, 0, DesignWidth, DesignHeight, Color.FromArgb(150, 14, 18, 24));
        }

        // A small procession of animated enemies walking along the bottom edge.
        void DrawParade()
        {
            const double groundY = 166;
            Surface.Rect(0, groundY - 2, DesignWidth, 4, Color.FromArgb(120, 0, 0, 0));
            double offset = (_Tick * 0.6) % 36;
            for (int i = 0; i < 11; i++)
            {
                string name = MenuParade[(i + _Tick / 60) % MenuParade.Length];
                G10_TowerDefenseSprite sprite = G10_TowerDefenseSprites.ForName(name);
                double x = -36 + i * 36 + offset;
                double scale = sprite.FrameHeight > 160 ? 0.5 : 1;
                DrawSprite(sprite, sprite.FrameAt("walk", ClockSeconds + i * 0.13), x, groundY + 1, false, scale);
            }
        }

        // ---------------- map ----------------

        // Layers as described in karten/LIESMICH_Karten.txt: ground, decoration, enemies under a bridge,
        // bridge decks, then everything else sorted by its foot point.
        void DrawMap()
        {
            Surface.Rect(0, 0, DesignWidth, DesignHeight, Color.FromRgb(20, 18, 16));
            double mapWidth = UX(G10_TowerDefenseMaps.MapWidth), mapHeight = UX(G10_TowerDefenseMaps.MapHeight);
            Surface.Sprite(G10_TowerDefenseSprites.MapLayer(_Map.Id, "boden"), 0, MapTopUnits, mapWidth, mapHeight);
            Surface.Sprite(G10_TowerDefenseSprites.MapLayer(_Map.Id, "deko"), 0, MapTopUnits, mapWidth, mapHeight);

            List<KeyValuePair<double, Action>> sorted = new List<KeyValuePair<double, Action>>();
            if (_Battle != null)
            {
                for (int e = 0; e < _Battle.Enemies.Count; e++)
                {
                    G10_TowerDefenseBattleEnemy enemy = _Battle.Enemies[e];
                    if (IsUnderBridge(enemy.X, enemy.Y, enemy.DirectionX, enemy.DirectionY)) DrawEnemy(enemy);
                    else sorted.Add(new KeyValuePair<double, Action>(enemy.Y, () => DrawEnemy(enemy)));
                }
                foreach (G10_TowerDefenseEffect effect in _Battle.Effects)
                {
                    if (effect.Kind != G10_TowerDefenseEffectKind.Death) continue;
                    G10_TowerDefenseEffect death = effect;
                    sorted.Add(new KeyValuePair<double, Action>(death.Y - 0.5, () => DrawDeath(death)));
                }
            }
            Surface.Sprite(G10_TowerDefenseSprites.MapLayer(_Map.Id, "bruecken"), 0, MapTopUnits, mapWidth, mapHeight);

            if (_Screen == G10_TowerDefenseScreen.Map)
            {
                G10_Tower selected = _Towers[_SelectedSpot];
                double range = selected == null ? 0 : G10_TowerDefenseBattle.TowerRange(selected, _Upgrades[_SelectedSpot]);
                if (RangeVisible(range) && selected != null) DrawRangeCircle(_Map.TowerSpots[_SelectedSpot], range);
            }

            sorted.Add(new KeyValuePair<double, Action>(_Map.MainBuilding.Y, () => DrawMainBuilding(_Map, MainBuildingHealth)));
            for (int i = 0; i < _Map.TowerSpots.Count; i++)
            {
                int index = i;
                sorted.Add(new KeyValuePair<double, Action>(_Map.TowerSpots[i].Y + 0.25, () => DrawSpot(index)));
            }
            // Stable sort: equal heights keep their order.
            List<KeyValuePair<double, Action>> ordered = new List<KeyValuePair<double, Action>>(sorted);
            ordered.Sort((a, b) => a.Key != b.Key ? a.Key.CompareTo(b.Key) : sorted.IndexOf(a).CompareTo(sorted.IndexOf(b)));
            foreach (KeyValuePair<double, Action> entry in ordered) entry.Value();

            DrawProjectilesAndEffects();
            DrawTowerMarker();
            DrawHud();
            DrawBanner();
            if (_ShowWavePreview) DrawWavePreview();
        }

        // An enemy that walks across a crossing below the bridge deck.
        bool IsUnderBridge(double x, double y, double directionX, double directionY)
        {
            foreach (G10_TowerDefenseCrossing crossing in _Map.Crossings)
                if (crossing.Area.Contains(x, y) && !crossing.IsOnBridge(x, y, directionX, directionY)) return true;
            return false;
        }

        // Pedestal click area and, if built, the tower. Click areas follow the drawing order, so the frontmost
        // tower wins the hit test.
        void DrawSpot(int index)
        {
            Point spot = _Map.TowerSpots[index];
            double x = UX(spot.X), y = UY(spot.Y);
            Action select = () => _SelectedSpot = index;
            Action open = () => { _SelectedSpot = index; ConfirmSelection(); };
            if (_Towers[index] == null)
            {
                AddClickArea(x - 8, y - 8, 16, 11, select, open);
                return;
            }
            AddClickArea(x - 8, y - 26, 16, 28, select, open);
            DrawTowerSprite(_Towers[index], _Upgrades[index], x, y, HidesEnemy(spot) ? TowerSeeThroughOpacity : 1, index);
        }

        void DrawEnemy(G10_TowerDefenseBattleEnemy enemy)
        {
            G10_TowerDefenseSprite sprite = G10_TowerDefenseSprites.ForName(enemy.Name);
            double x = UX(enemy.X), y = UY(enemy.Y);
            DrawSprite(sprite, sprite.FrameAt("walk", enemy.Age), x, y, enemy.FacingLeft, 1, Math.Min(0.45, enemy.HitFlash / 0.08 * 0.45));

            // Health bar once damaged (always for bosses), plus a dot per active status effect.
            G10_Enemy stats = enemy.Enemy;
            double barWidth = enemy.IsBoss ? 28 : 12;
            double top = y - sprite.Pivot.Y / Px * 0.85 - 2;
            if (stats.CurrentHealth < stats.MaxHealth || enemy.IsBoss)
            {
                double fill = Math.Max(0.5, barWidth * stats.CurrentHealth / (double)stats.MaxHealth);
                Surface.Rect(x - barWidth / 2 - 0.5, top - 0.5, barWidth + 1, 2, Outline);
                Surface.Rect(x - barWidth / 2, top, fill, 1, enemy.IsBoss ? Color.FromRgb(230, 90, 220) : Color.FromRgb(120, 220, 90));
            }
            double dotX = x + barWidth / 2 + 1.5;
            if (stats.BurnDamagePerSecond > 0) { Surface.Rect(dotX, top - 0.5, 2, 2, Color.FromRgb(255, 140, 40)); dotX += 2.5; }
            if (stats.PoisonDamagePerSecond > 0) { Surface.Rect(dotX, top - 0.5, 2, 2, Color.FromRgb(120, 220, 80)); dotX += 2.5; }
            if (stats.CurrentMoveSpeed < stats.MoveSpeed) { Surface.Rect(dotX, top - 0.5, 2, 2, Color.FromRgb(150, 220, 255)); dotX += 2.5; }
            if (stats.CurrentArmor < stats.Armor) Surface.Rect(dotX, top - 0.5, 2, 2, Color.FromRgb(200, 150, 90));
        }

        void DrawDeath(G10_TowerDefenseEffect death)
        {
            G10_TowerDefenseSprite sprite = G10_TowerDefenseSprites.ForName(death.Name);
            DrawSprite(sprite, sprite.FrameAt("death", death.Elapsed), UX(death.X), UY(death.Y), death.FacingLeft);
        }

        // Projectile sprite per tower; the castle shoots arcane bolts, the flamethrower only shows its flame.
        static string ProjectileSprite(G10_Tower source)
        {
            if (source == null) return "arcane_bolt";
            switch (source.Name)
            {
                case "Physical Archer": return "arrow";
                case "Poison Archer": return "poison_arrow";
                case "Fire Mage": return "fireball";
                case "Ice Mage": return "ice_shard";
                case "Ice Area Tower": return "ice_shard";
                case "Earth Mage": return "rock";
                default: return null;
            }
        }

        // Arrows and shards point along their flight; balls and rocks keep their own animation.
        static bool RotatesWithFlight(string sprite)
        {
            return sprite == "arrow" || sprite == "poison_arrow" || sprite == "ice_shard";
        }

        void DrawProjectilesAndEffects()
        {
            if (_Battle == null) return;
            foreach (G10_TowerDefenseProjectile projectile in _Battle.Projectiles)
            {
                string name = ProjectileSprite(projectile.Source);
                if (name == null) continue;
                G10_TowerDefenseSprite sprite = G10_TowerDefenseSprites.Effect(name);
                double rotation = RotatesWithFlight(name) ? projectile.Angle * 180 / Math.PI : 0;
                DrawCentered(sprite, sprite.FrameAt("play", projectile.Age), UX(projectile.X), UY(projectile.Y), rotation);
            }

            foreach (G10_TowerDefenseEffect effect in _Battle.Effects)
            {
                switch (effect.Kind)
                {
                    case G10_TowerDefenseEffectKind.Text:
                        double rise = effect.Elapsed / effect.Duration * 8;
                        ShadowText(effect.Text, UX(effect.X) - effect.Text.Length * 1.4, UY(effect.Y) - rise, 5, TitleGold);
                        break;
                    case G10_TowerDefenseEffectKind.Impact:
                        DrawImpact(effect);
                        break;
                    case G10_TowerDefenseEffectKind.Muzzle:
                        if (effect.Name != "Flamethrower") break;
                        G10_TowerDefenseSprite flame = G10_TowerDefenseSprites.Effect("flamethrower_flame");
                        // The flame starts at the muzzle and points to the target.
                        double half = flame.FrameWidth / 2.0;
                        double cx = effect.X + Math.Cos(effect.Angle) * half, cy = effect.Y + Math.Sin(effect.Angle) * half;
                        DrawCentered(flame, flame.FrameAt("play", effect.Elapsed), UX(cx), UY(cy), effect.Angle * 180 / Math.PI);
                        break;
                }
            }
        }

        void DrawImpact(G10_TowerDefenseEffect impact)
        {
            double x = UX(impact.X), y = UY(impact.Y);
            if (impact.Name == "Strom Mage")
            {
                G10_TowerDefenseSprite strike = G10_TowerDefenseSprites.Effect("lightning_strike");
                // The bolt comes from above and ends at the enemy's feet.
                Surface.Sprite(strike.FrameAt("play", impact.Elapsed), x - strike.FrameWidth / Px / 2,
                    y - strike.FrameHeight / Px, strike.FrameWidth / Px, strike.FrameHeight / Px);
            }
            else if (impact.Name == "Ice Area Tower")
            {
                G10_TowerDefenseSprite nova = G10_TowerDefenseSprites.Effect("frost_nova");
                DrawCentered(nova, nova.FrameAt("play", impact.Elapsed), x, y - 3, 0);
            }
        }

        void DrawRangeCircle(Point spot, double range)
        {
            Point center = G10_TowerDefenseBattle.RangeCenter(spot);
            DrawRangeRing(UX(center.X), UY(center.Y), range / Px, MapTopUnits, DesignHeight);
        }

        static readonly TimeSpan RangeShowTime = TimeSpan.FromSeconds(3);
        int _RangeSpot = -1;
        G10_Tower _RangeTower;
        double _RangeValue;
        DateTime _RangeShownSince;

        // The range shows for a few seconds after a tower gets selected, built or upgraded.
        // Hovering the same tower again does not restart the timer; clicking or Enter does.
        bool RangeVisible(double range)
        {
            G10_Tower tower = _Towers[_SelectedSpot];
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
            int points = Math.Max(24, (int)(radius * 1.2));
            for (int i = 0; i < points; i++)
            {
                double angle = i * Math.PI * 2 / points;
                double x = cx + Math.Cos(angle) * radius, y = cy + Math.Sin(angle) * radius;
                if (y < minY || y > maxY) continue;
                Surface.Rect(x - 1, y - 1, 2, 2, RangeRingShadow);
                Surface.Rect(x - 0.5, y - 0.5, 1, 1, RangeRingColor);
            }
        }

        void DrawBanner()
        {
            if (_BannerTicks <= 0 || _Banner == null) return;
            double width = _Banner.Length * 4.4 + 16;
            double x = 160 - width / 2;
            Surface.Rect(x, 12, width, 15, Outline);
            Surface.Rect(x + 1, 13, width - 2, 13, Selection);
            Surface.Rect(x + 1, 13, width - 2, 1, SelectionEdge);
            ShadowText(_Banner, x + 8, 15, 8, TitleGold);
        }

        // Window below the enemy counter: which enemy types the current or next wave brings (no counts).
        void DrawWavePreview()
        {
            G10_TowerDefenseWave wave = WaveRunning ? _Battle.CurrentWave : G10_TowerDefenseWaves.Get(_MapIndex, _Wave, _EndlessSeed);
            IList<string> types = wave.EnemyTypes;
            const double y = HudHeight + 1;
            // Up to 8 enemy types in one column; more (endless mode) continue in a second column to the
            // right. With more than 16 types the rows get a little tighter so everything stays on screen.
            const int rowsPerColumn = 8;
            const double columnWidth = 120, maxListHeight = 150;
            int columns = types.Count > rowsPerColumn ? 2 : 1;
            // The first column is filled completely, the rest continues in the second one.
            int rows = columns == 1 ? types.Count : Math.Max(rowsPerColumn, (types.Count + 1) / 2);
            double rowHeight = Math.Min(17, Math.Floor(maxListHeight / Math.Max(1, rows)));
            double iconSize = Math.Min(16, rowHeight - 1);
            double width = 4 + columns * columnWidth;
            double x = Math.Min(DesignWidth - width - 2, HudEnemyX - 2);
            double height = 20 + rows * rowHeight;
            AddClickArea(x, y, width, height, null, () => _ShowWavePreview = false);
            Surface.Rect(x, y, width, height, Outline);
            Surface.Rect(x + 1, y + 1, width - 2, height - 2, StoneDark);
            Surface.Rect(x + 2, y + 2, width - 4, height - 4, PanelInside);
            string title = (WaveRunning ? "WELLE " : "NÄCHSTE WELLE ") + wave.Number;
            ShadowText(title, x + 6, y + 4, 6, TitleGold);
            ShadowText("x", x + width - 10, y + 3, 6, TextDim);
            if (columns > 1) Surface.Rect(x + 2 + columnWidth, y + 15, 0.5, rows * rowHeight, StoneDark);
            for (int i = 0; i < types.Count; i++)
            {
                string name = types[i];
                double cx = x + (i / rows) * columnWidth;
                double rowY = y + 16 + (i % rows) * rowHeight;
                bool boss = G10_TowerDefenseSprites.IsBoss(name);
                Surface.Rect(cx + 5, rowY, iconSize + 2, iconSize, PanelInsideDark);
                DrawSpriteIcon(name, cx + 6, rowY, iconSize);
                double textX = cx + 9 + iconSize;
                ShadowText(name.ToUpperInvariant(), textX, rowY + 1, 5, boss ? TitleGold : TextLight);
                if (boss)
                {
                    string label = wave.IsMiniBoss ? "MINIBOSS" : "BOSS";
                    Surface.Text(label, cx + columnWidth - 1 - label.Length * 2.75, rowY + 1, 5, Color.FromRgb(230, 110, 200));
                }
                DrawPreviewDamageTypes(PreviewEnemy(name), textX, rowY + Math.Min(9, rowHeight - 6));
            }
        }

        // First walk frame of an enemy or boss, fitted into a square box (logical units).
        void DrawSpriteIcon(string name, double x, double y, double size)
        {
            G10_TowerDefenseSprite sprite = G10_TowerDefenseSprites.ForName(name);
            double scale = size / (Math.Max(sprite.FrameWidth, sprite.FrameHeight) / Px);
            Surface.Sprite(sprite.Frame("walk", 0), x, y, sprite.FrameWidth / Px * scale, sprite.FrameHeight / Px * scale);
        }

        static readonly Color WeakColor = Color.FromRgb(120, 220, 90);
        static readonly Color ResistColor = Color.FromRgb(222, 96, 64);
        static readonly Dictionary<string, G10_Enemy> _PreviewEnemies = new Dictionary<string, G10_Enemy>();

        static G10_Enemy PreviewEnemy(string name)
        {
            G10_Enemy enemy;
            if (!_PreviewEnemies.TryGetValue(name, out enemy))
            {
                enemy = G10_TowerDefenseWaves.CreateEnemy(name, 1);
                _PreviewEnemies[name] = enemy;
            }
            return enemy;
        }

        // One line with weakness and resistance icons, same labels as in the help page.
        void DrawPreviewDamageTypes(G10_Enemy enemy, double x, double y)
        {
            ShadowText("SCHWACH", x, y, 5, WeakColor);
            x += 7 * 2.75 + 1;
            foreach (G10_DamageType type in enemy.WeakAgainst) { Icon(G10_TowerDefenseSprites.DamageIcon(type), x, y, 7); x += 8; }
            x += 3;
            ShadowText("RESIST.", x, y, 5, ResistColor);
            x += 7 * 2.75 + 1;
            foreach (G10_DamageType type in enemy.ResistantDamageTypes) { Icon(G10_TowerDefenseSprites.DamageIcon(type), x, y, 7); x += 8; }
        }

        // Animated castle; its state follows the health (intact, damaged, ruin).
        void DrawMainBuilding(G10_TowerDefenseMapDefinition map, int healthPercent, double scale = 1, double originX = 0, double originY = MapTopUnits)
        {
            G10_TowerDefenseSprite castle = G10_TowerDefenseSprites.Castle;
            BitmapSource frame = castle.FrameAt(G10_TowerDefenseSprites.CastleAnimation(healthPercent), ClockSeconds);
            DrawSprite(castle, frame, originX + UX(map.MainBuilding.X) * scale, originY + UX(map.MainBuilding.Y) * scale, false, scale);
        }

        static readonly Color DamageColor = Color.FromRgb(232, 92, 70);
        static readonly Color RangeColor = Color.FromRgb(110, 210, 110);
        static readonly Color SpeedColor = Color.FromRgb(250, 210, 80);

        const double TowerSeeThroughOpacity = 0.45;

        // True when an enemy walks behind the tower on this pedestal, so the tower would hide it (map pixels).
        bool HidesEnemy(Point spot)
        {
            if (_Battle == null) return false;
            foreach (G10_TowerDefenseBattleEnemy enemy in _Battle.Enemies)
            {
                if (enemy.Y >= spot.Y) continue;
                double half = enemy.IsBoss ? 80 : 36, height = enemy.IsBoss ? 200 : 80;
                bool overlapX = enemy.X + half > spot.X - 40 && enemy.X - half < spot.X + 40;
                bool overlapY = enemy.Y > spot.Y - 150 && enemy.Y - height < spot.Y;
                if (overlapX && overlapY) return true;
            }
            return false;
        }

        // Tower at its foot point (x, y) in logical units. spot >= 0 plays the battle's attack animation.
        void DrawTowerSprite(G10_Tower tower, G10_TowerUpgrades upgrades, double x, double y, double opacity = 1, int spot = -1, double scale = 1)
        {
            G10_TowerDefenseSprite sprite = G10_TowerDefenseSprites.ForName(tower.Name);
            BitmapSource frame;
            bool flip = false;
            G10_TowerDefenseAnimation attack = sprite.Animation("attack");
            if (spot >= 0 && _Battle != null && _Battle.TowerAttackAge[spot] < attack.Duration)
            {
                frame = sprite.FrameAt("attack", _Battle.TowerAttackAge[spot]);
                flip = _Battle.TowerFacingLeft[spot];
            }
            else
            {
                frame = sprite.FrameAt("idle", ClockSeconds + Math.Max(0, spot) * 0.37);
                flip = spot >= 0 && _Battle != null && _Battle.TowerFacingLeft[spot];
            }
            DrawSprite(sprite, frame, x, y, flip, scale, 0, opacity);
            if (scale != 1) return;
            DrawTierStars(x, y, upgrades.Tier, 1, upgrades.Mastery);
            if (upgrades.TotalLevels == 0) return;

            // One bar per upgrade path below the pedestal, 1 unit per level.
            int widest = Math.Max(upgrades.Damage, Math.Max(upgrades.Range, upgrades.Speed));
            double left = x - widest / 2.0;
            Surface.Rect(left - 0.5, y + 2, widest + 1, 4, Outline);
            Surface.Rect(left, y + 2.5, upgrades.Damage, 0.67, DamageColor);
            Surface.Rect(left, y + 3.67, upgrades.Range, 0.67, RangeColor);
            Surface.Rect(left, y + 4.83, upgrades.Speed, 0.67, SpeedColor);
        }

        // 5x5 pixel star used for the tower tier.
        static readonly string[] TierStar = { "..#..", "#####", ".###.", ".#.#.", "#...#" };

        // Tier II/III as a small row of stars on a dark plate, centred on the tower's foot point
        // (cx, footY) and lying on the pedestal front, so it never reaches a neighbouring tower.
        // Tier II = 2 silver stars, tier III = 3 gold stars. scale 1 = map size (one star pixel = 3 px).
        void DrawTierStars(double cx, double footY, int tier, double scale = 1, int mastery = 0)
        {
            if (tier < 2) return;
            double px = 0.5 * scale, star = 5 * px, gap = px;
            double width = tier * star + (tier - 1) * gap;
            double left = cx - width / 2, top = footY - 2.5 * scale;
            Surface.Rect(left - px, top - px, width + 2 * px, star + 2 * px, Color.FromArgb(220, 24, 20, 18));
            Color fill = mastery > 0 ? MasteryColor : tier >= 3 ? SelectionEdge : Color.FromRgb(214, 220, 228);
            for (int s = 0; s < tier; s++)
            {
                double sx = left + s * (star + gap);
                for (int row = 0; row < 5; row++)
                    for (int col = 0; col < 5; col++)
                        if (TierStar[row][col] == '#') Surface.Rect(sx + col * px, top + row * px, px, px, fill);
            }
        }

        void DrawTowerMarker()
        {
            Point spot = _Map.TowerSpots[_SelectedSpot];
            double x = UX(spot.X), y = UY(spot.Y);
            bool occupied = _Towers[_SelectedSpot] != null;
            Color marker = occupied ? SelectionEdge : Color.FromRgb(120, 220, 240);
            DrawSelectionFrame(x, y, (_Tick / 8) % 2, marker);

            double bounce = (_Tick / 6) % 4 < 2 ? 0 : 2;
            double cursorY = (occupied ? y - 38 : y - 20) - bounce;
            Icon(G10_TowerDefenseIcon.Cursor, x - 6, cursorY, 12);
        }

        // Four corners around the pedestal at the foot point (x, y); pulse (0 or 1) widens the frame a little
        // for the blinking animation.
        void DrawSelectionFrame(double x, double y, double pulse, Color color)
        {
            double left = x - 8 - pulse, right = x + 7 + pulse, top = y - 7 - pulse, bottom = y + 3 + pulse;
            DrawCorner(left, top, 1, 1, color);
            DrawCorner(right, top, -1, 1, color);
            DrawCorner(left, bottom, 1, -1, color);
            DrawCorner(right, bottom, -1, -1, color);
        }

        // L-shaped corner of the selection frame. (x, y) is the outer corner cell; both arms are equally long
        // and point in the direction (dx, dy), so all four corners are exact mirror images.
        void DrawCorner(double x, double y, int dx, int dy, Color color)
        {
            const double arm = 3;
            double hx = dx > 0 ? x : x - arm + 1;
            double vy = dy > 0 ? y : y - arm + 1;
            Surface.Rect(hx - 1, y - 1, arm + 2, 3, Outline);
            Surface.Rect(x - 1, vy - 1, 3, arm + 2, Outline);
            Surface.Rect(hx, y, arm, 1, color);
            Surface.Rect(x, vy, 1, arm, color);
        }

        // ---------------- HUD: values at the top, buttons at the bottom right ----------------

        // Height of the value bar (48 px). It covers the top 24 px of the map slightly transparent.
        const double HudHeight = 8;
        const int HudTextSize = 6;
        const double HudIconSize = 7;
        // Width of one character of the HUD font in logical units.
        const double HudCharWidth = 3.3;
        const double HudEnemyX = 120;
        // Bottom right button group.
        const double ButtonHeight = 8;

        void DrawHud()
        {
            Surface.Rect(0, 0, DesignWidth, HudHeight, Color.FromArgb(225, 30, 26, 22));
            Surface.Rect(0, HudHeight - 0.5, DesignWidth, 0.5, StoneDark);
            double x = DrawHudValue(1.5, G10_TowerDefenseIcon.Coin, Gold.ToString(), 6);
            x = DrawHudValue(x, G10_TowerDefenseIcon.Heart, MainBuildingHealth + "%", 4);
            DrawHudValue(x, G10_TowerDefenseIcon.Wave, _Endless ? CurrentWave + " ENDLOS" : CurrentWave + "/" + G10_TowerDefenseWaves.WaveCount, 5);

            // The enemy counter opens the wave preview (mouse or E).
            string enemies = (WaveRunning ? "FEINDE " : "NÄCHSTE ") + ExpectedEnemies + (_ShowWavePreview ? " ^" : " v");
            double enemyWidth = HudIconSize + 3 + enemies.Length * HudCharWidth;
            bool hovered = IsMouseOver(HudEnemyX, 0, enemyWidth, HudHeight);
            AddClickArea(HudEnemyX, 0, enemyWidth, HudHeight, null, () => _ShowWavePreview = !_ShowWavePreview);
            if (hovered || _ShowWavePreview) Surface.Rect(HudEnemyX, 0.5, enemyWidth, HudHeight - 1, Selection);
            Icon(G10_TowerDefenseIcon.Skull, HudEnemyX + 1, 0.5, HudIconSize);
            ShadowText(enemies, HudEnemyX + HudIconSize + 2, 0.3, HudTextSize, hovered ? TitleGold : TextLight);

            double infoX = HudEnemyX + enemyWidth + 4;
            for (int skull = 0; skull <= (int)_Difficulty; skull++)
                Icon(G10_TowerDefenseIcon.Skull, infoX + skull * 4.5, 2, 4);
            infoX += 4.5 * ((int)_Difficulty + 1) + 2;
            string status = Difficulty.ToUpperInvariant() + "  " + Status.ToUpperInvariant();
            int maxChars = (int)((DesignWidth - infoX - 2) / HudCharWidth);
            if (status.Length > maxChars) status = status.Substring(0, Math.Max(0, maxChars));
            Surface.Text(status, infoX, 0.3, HudTextSize, TextDim);

            DrawHudButtons();
        }

        // Icon and value; returns the x where the next value starts.
        double DrawHudValue(double x, G10_TowerDefenseIcon icon, string value, int minChars)
        {
            Icon(icon, x, 0.5, HudIconSize);
            ShadowText(value, x + HudIconSize + 1.5, 0.3, HudTextSize, TitleGold);
            double end = x + HudIconSize + 1.5 + Math.Max(minChars, value.Length) * HudCharWidth + 2;
            Surface.Rect(end, 1.5, 0.5, HudHeight - 3, StoneDark);
            return end + 2.5;
        }

        // WELLE STARTEN, AUTO, TEMPO and MENUE as a compact group in the bottom right corner of the map.
        void DrawHudButtons()
        {
            List<string> labels = new List<string>();
            List<Action> actions = new List<Action>();
            List<bool> active = new List<bool>();
            if (_Battle != null && _Battle.Phase == G10_TowerDefenseBattlePhase.Building)
            {
                // During the auto start countdown the button shows the seconds left; a click starts at once.
                labels.Add(_AutoStartCountdown > 0 ? "START IN " + Math.Ceiling(_AutoStartCountdown) : "WELLE STARTEN");
                actions.Add(() => { _AutoStartCountdown = 0; StartNextWave(); });
                active.Add(false);
            }
            labels.Add(AutoStart ? "AUTO AN" : "AUTO AUS");
            actions.Add(ToggleAutoStart);
            active.Add(AutoStart);
            labels.Add("TEMPO x" + _GameSpeed);
            actions.Add(ToggleSpeed);
            active.Add(false);
            labels.Add("MENÜ");
            actions.Add(() => { _Paused = true; _PauseSelection = 0; });
            active.Add(false);

            double total = 0;
            foreach (string label in labels) total += ButtonWidth(label) + 1.5;
            double y = DesignHeight - ButtonHeight - 1.5;
            double x = DesignWidth - total - 2;
            Surface.Rect(x - 1.5, y - 1.5, total + 2.5, ButtonHeight + 3, Color.FromArgb(200, 30, 26, 22));
            for (int i = 0; i < labels.Count; i++)
                x = DrawHudButton(labels[i], x, y, actions[i], active[i]) + 1.5;
        }

        static double ButtonWidth(string label) { return label.Length * HudCharWidth + 5; }

        // Button with its left edge at x; returns its right edge. An active (switched on) button is green.
        double DrawHudButton(string label, double x, double y, Action click, bool active = false)
        {
            double width = ButtonWidth(label);
            bool hovered = IsMouseOver(x, y, width, ButtonHeight);
            AddClickArea(x, y, width, ButtonHeight, null, click);
            Color face = hovered ? Selection : active ? Color.FromRgb(46, 92, 46) : StoneDark;
            Color edge = hovered ? SelectionEdge : active ? Color.FromRgb(96, 170, 80) : Stone;
            Surface.Rect(x, y, width, ButtonHeight, hovered ? SelectionEdge : Outline);
            Surface.Rect(x + 0.5, y + 0.5, width - 1, ButtonHeight - 1, face);
            Surface.Rect(x + 0.5, y + 0.5, width - 1, 0.5, edge);
            ShadowText(label, x + 2.5, y + 0.4, HudTextSize, hovered ? TitleGold : TextLight);
            return x + width;
        }

        // HUD cell as used by the help page's picture of the interface.
        void DrawHudCell(double x, G10_TowerDefenseIcon icon, string label, string value, double y = 3)
        {
            Surface.Rect(x, y, 60, 17, Outline);
            Surface.Rect(x + 1, y + 1, 58, 15, PanelInside);
            Surface.Rect(x + 1, y + 1, 58, 1, PanelInsideDark);
            Icon(icon, x + 2, y + 1, 16);
            Surface.Text(label, x + 20, y, 5, TextDim);
            ShadowText(value, x + 20, y + 6, 8, TitleGold);
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
                G10_Tower option = TowerCreators[i]();
                double y = 55 + i * 12;
                bool selected = i == _TowerSelection;
                bool affordable = _Gold >= option.Cost;
                int index = i;
                AddClickArea(24, y, 148, 12, () => _TowerSelection = index, BuildTower);
                DrawSelectionBar(24, y, 148, 12, selected);
                Icon(G10_TowerDefenseSprites.DamageIcon(option.DamageType), 28, y + 2, 8);
                ShadowText(option.Name.ToUpperInvariant(), 40, y + 2, 6, selected ? TitleGold : TextLight);
                Icon(G10_TowerDefenseIcon.Coin, 140, y + 2, 8);
                ShadowText(option.Cost.ToString(), 150, y + 2, 6, affordable ? TextGold : Color.FromRgb(222, 96, 64));
            }

            G10_Tower tower = TowerCreators[_TowerSelection]();
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
            G10_Tower tower = _Towers[_SelectedSpot];
            G10_TowerUpgrades upgrades = _Upgrades[_SelectedSpot];
            DrawTowerShowcase(tower, 26, 60);
            DrawTierStars(26 + 24, 60 + 68, upgrades.Tier, 2, upgrades.Mastery);

            ShadowText(tower.Name.ToUpperInvariant(), 84, 56, 7, TitleGold);
            if (upgrades.Mastery > 0)
                ShadowText("MEISTER " + upgrades.Mastery, 230, 57, 6, MasteryColor);
            else
                ShadowText("TURMSTUFE " + TierName(upgrades.Tier), 230, 57, 6, upgrades.Tier > 1 ? TitleGold : TextLight);

            // Current values; the selected upgrade shows its result in green.
            G10_TowerUpgrades preview = _UpgradeSelection < 4 ? upgrades.Preview(RowPath(_UpgradeSelection, upgrades)) : upgrades;
            DrawUpgradeStat(G10_TowerDefenseIcon.Sword, G10_TowerDefenseBattle.TowerDamage(tower, upgrades).ToString(),
                G10_TowerDefenseBattle.TowerDamage(tower, preview).ToString(), 84);
            DrawUpgradeStat(G10_TowerDefenseIcon.Range, (G10_TowerDefenseBattle.TowerRange(tower, upgrades) / G10_TowerDefenseBattle.RangeUnit).ToString("0.0"),
                (G10_TowerDefenseBattle.TowerRange(tower, preview) / G10_TowerDefenseBattle.RangeUnit).ToString("0.0"), 156);
            DrawUpgradeStat(G10_TowerDefenseIcon.Speed, G10_TowerDefenseBattle.TowerAttackSpeed(tower, upgrades).ToString("0.00"),
                G10_TowerDefenseBattle.TowerAttackSpeed(tower, preview).ToString("0.00"), 226);
            DrawEffectInfo(tower, upgrades, preview, 26, 134);

            string[] labels =
            {
                "SCHADEN  +" + (int)Math.Round(G10_TowerUpgrades.DamagePerLevel * 100) + "%",
                "REICHWEITE  +" + (int)Math.Round(G10_TowerUpgrades.RangePerLevel * 100) + "%",
                "TEMPO  +" + (int)Math.Round(G10_TowerUpgrades.SpeedPerLevel * 100) + "%",
                upgrades.IsComplete ? "MEISTER " + (upgrades.Mastery + 1)
                    : upgrades.IsMaxed(G10_TowerUpgradePath.Tier) ? "TURMSTUFE" : "TURMSTUFE " + TierName(upgrades.Tier + 1),
                "VERKAUFEN"
            };
            G10_TowerDefenseIcon[] icons = { G10_TowerDefenseIcon.Sword, G10_TowerDefenseIcon.Range, G10_TowerDefenseIcon.Speed, G10_TowerDefenseIcon.Star, G10_TowerDefenseIcon.Sell };
            Color[] colors = { DamageColor, RangeColor, SpeedColor, upgrades.IsComplete ? MasteryColor : TitleGold, TextGold };
            for (int i = 0; i < labels.Length; i++)
            {
                double y = 78 + i * 15;
                int index = i;
                bool selected = _UpgradeSelection == i;
                Action click = i < 4 ? (Action)(() => UpgradeTower(RowPath(index, upgrades))) : SellTower;
                AddClickArea(82, y, 214, 14, () => _UpgradeSelection = index, click);
                DrawSelectionBar(82, y, 214, 14, selected);
                Icon(icons[i], 85, y + 3, 8);
                ShadowText(labels[i], 97, y + 3, 6, selected ? TitleGold : i == 3 && upgrades.IsComplete ? MasteryColor : TextLight);

                if (i == 4)
                {
                    Icon(G10_TowerDefenseIcon.Coin, 246, y + 3, 8);
                    ShadowText("+" + G10_TowerDefenseBattle.SellValue(tower, upgrades), 256, y + 3, 6, TextGold);
                    continue;
                }

                G10_TowerUpgradePath path = RowPath(i, upgrades);
                int max = G10_TowerUpgrades.MaxLevel(path);
                int current = upgrades.Level(path);
                if (path == G10_TowerUpgradePath.Mastery)
                    ShadowText("+" + (int)Math.Round(G10_TowerUpgrades.MasteryDamagePerLevel * 100) + "% SCH. +" + (int)Math.Round(G10_TowerUpgrades.MasterySpeedPerLevel * 100) + "% TEMPO", 166, y + 3, 5, TextDim);
                else if (path == G10_TowerUpgradePath.Tier && upgrades.IsMaxed(path))
                    ShadowText("MEISTER NACH ALLEN UPGRADES", 166, y + 3, 5, TextDim);
                else if (path == G10_TowerUpgradePath.Tier)
                    ShadowText(TierName(current) + "/III", 198, y + 3, 5, TextDim);
                else
                {
                    // Pips: filled levels in the path colour.
                    for (int pip = 0; pip < max; pip++)
                        Surface.Rect(190 + pip * 5, y + 5, 4, 4, pip < current ? colors[i] : PanelInsideDark);
                }

                if (!upgrades.CanBuy(path))
                {
                    ShadowText("MAX", 256, y + 3, 6, TextDim);
                    continue;
                }
                int cost = upgrades.Cost(tower, path);
                Icon(G10_TowerDefenseIcon.Coin, 246, y + 3, 8);
                ShadowText(cost.ToString(), 256, y + 3, 6, _Gold >= cost ? TextGold : Color.FromRgb(222, 96, 64));
            }

            ShadowText("KLICK / ENTER  KAUFEN    RECHTSKLICK / BACKSPACE  ZURÜCK", 84, 155, 5, TextDim);
        }

        static readonly Color BurnColor = Color.FromRgb(255, 140, 40);
        // Stars and labels of mastered towers.
        static readonly Color MasteryColor = Color.FromRgb(222, 140, 255);
        static readonly Color PoisonColor = Color.FromRgb(120, 220, 80);
        static readonly Color SlowColor = Color.FromRgb(150, 220, 255);
        static readonly Color ArmorBreakColor = Color.FromRgb(200, 150, 90);

        // Small box below the tower picture: the tower effect, its strength and how long it lasts.
        // Burn and poison grow with the damage upgrade, so the selected upgrade shows its result in green.
        void DrawEffectInfo(G10_Tower tower, G10_TowerUpgrades upgrades, G10_TowerUpgrades preview, double x, double y)
        {
            string name, value, next = null;
            Color color;
            bool timed = true;
            switch (tower.Effect)
            {
                case G10_TowerEffect.Burn:
                case G10_TowerEffect.PoisonDamageOverTime:
                    bool burn = tower.Effect == G10_TowerEffect.Burn;
                    name = burn ? "BRAND" : "GIFT";
                    color = burn ? BurnColor : PoisonColor;
                    value = G10_TowerDefenseBattle.TowerEffectDamage(tower, upgrades) + "/S";
                    next = G10_TowerDefenseBattle.TowerEffectDamage(tower, preview) + "/S";
                    break;
                case G10_TowerEffect.Slow:
                    name = "LANGSAM";
                    color = SlowColor;
                    value = "-" + (int)Math.Round((1 - tower.SlowFactor) * 100) + "% TEMPO";
                    break;
                case G10_TowerEffect.ArmorBreak:
                    name = "RÜSTUNG";
                    color = ArmorBreakColor;
                    value = "-" + tower.EffectStrength;
                    break;
                case G10_TowerEffect.ChainLightning:
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
                string duration = G10_Tower.StatusEffectDuration.ToString("0") + "S";
                ShadowText(duration, x + 45 - duration.Length * 2.75, y + 2, 5, TextDim);
            }
            ShadowText(value, x + 3, y + 9, 5, TextLight);
            if (next != null && next != value) ShadowText(">" + next, x + 5 + value.Length * 2.75, y + 9, 5, RangeColor);
        }

        void DrawUpgradeStat(G10_TowerDefenseIcon icon, string value, string next, double x)
        {
            Icon(icon, x, 66, 8);
            ShadowText(value, x + 11, 67, 5, TextLight);
            if (next != value) ShadowText(">" + next, x + 13 + value.Length * 2.75, 67, 5, RangeColor);
        }

        // Tower standing on a pedestal inside a small framed window (48x72).
        void DrawTowerShowcase(G10_Tower tower, double x, double y)
        {
            Surface.Rect(x, y, 48, 72, Outline);
            Surface.Rect(x + 1, y + 1, 46, 70, StoneLight);
            Surface.Rect(x + 2, y + 2, 44, 68, Color.FromRgb(98, 148, 66));
            Surface.Rect(x + 2, y + 2, 44, 40, Color.FromRgb(118, 176, 196));
            Surface.Rect(x + 2, y + 38, 44, 4, Color.FromRgb(150, 196, 206));
            // The tower at double size with its idle animation.
            DrawTowerSprite(tower, new G10_TowerUpgrades(), x + 24, y + 68, 1, -1, 2);
        }

        void DrawTowerStats(G10_Tower tower, double x, double y)
        {
            DrawStat(G10_TowerDefenseSprites.DamageIcon(tower.DamageType), DamageTypeNames[(int)tower.DamageType], x, y);
            DrawStat(G10_TowerDefenseSprites.Icon(G10_TowerDefenseIcon.Sword), "SCHADEN " + tower.Damage, x, y + 14);
            DrawStat(G10_TowerDefenseSprites.Icon(G10_TowerDefenseIcon.Speed), "TEMPO " + tower.AttackSpeed.ToString("0.0"), x, y + 28);
            DrawStat(G10_TowerDefenseSprites.Icon(G10_TowerDefenseIcon.Range), "REICHW. " + tower.AttackRange.ToString("0.#"), x, y + 42);
            string mode = tower.Effect == G10_TowerEffect.ChainLightning ? "KETTE x" + tower.MaxChainTargets
                : tower.Effect == G10_TowerEffect.Burn ? "BRAND " + tower.EffectDamagePerSecond + "/S"
                : tower.Effect == G10_TowerEffect.PoisonDamageOverTime ? "GIFT " + tower.EffectDamagePerSecond + "/S"
                : tower.Effect == G10_TowerEffect.Slow ? "LANGSAM -" + (int)Math.Round((1 - tower.SlowFactor) * 100) + "%"
                : tower.Effect == G10_TowerEffect.ArmorBreak ? "RÜSTUNG -" + tower.EffectStrength
                : tower.AttackMode == G10_AttackMode.AreaDamage ? "FLÄCHE" : "EINZELZIEL";
            DrawStat(G10_TowerDefenseSprites.Icon(G10_TowerDefenseIcon.Physical), mode, x, y + 56);
        }

        void DrawStat(BitmapSource icon, string text, double x, double y)
        {
            Icon(icon, x, y, 8);
            ShadowText(text, x + 11, y + 1, 5, TextLight);
        }

        // ---------------- shared UI pieces ----------------

        G10_TowerDefensePixelSurface Surface { get { return _PixelSurface; } }

        void Icon(G10_TowerDefenseIcon icon, double x, double y, double size)
        {
            Surface.Sprite(G10_TowerDefenseSprites.Icon(icon), x, y, size, size);
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

        void DrawMenuEntry(string text, G10_TowerDefenseIcon icon, bool selected, double y, Action hover, Action click)
        {
            AddClickArea(92, y, Math.Max(136, 34 + text.Length * 5.5), 17, hover, click);
            DrawSelectionBar(92, y, Math.Max(136, 34 + text.Length * 5.5), 17, selected);
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
                case G10_TowerDefenseScreen.MainMenu:
                    hint = "MAUS / PFEILE  WÄHLEN   KLICK / ENTER  BESTÄTIGEN   ESC  MENÜ";
                    back = false;
                    menu = false;
                    break;
                case G10_TowerDefenseScreen.SlotSelection:
                    hint = "KLICK / ENTER  LADEN   ENTF  LÖSCHEN";
                    break;
                case G10_TowerDefenseScreen.DifficultySelection:
                    hint = "KLICK / ENTER  KARTE STARTEN   RECHTSKLICK  ZURÜCK";
                    break;
                case G10_TowerDefenseScreen.MapSelection:
                    hint = CurrentSlot.IsUnlocked(_MapSelection)
                        ? "KLICK AUF KARTE / ENTER  STARTEN   < >  WECHSELN"
                        : "GESPERRT - VORHERIGE KARTE AUF NORMAL, SCHWER ODER GOTT SCHAFFEN";
                    break;
                case G10_TowerDefenseScreen.Map:
                    hint = WaveRunning
                        ? "KLICK / PFEILE  PODEST   T  TEMPO   E  GEGNER   ESC  MENÜ"
                        : "LEERTASTE  WELLE STARTEN   ENTER  BAUEN   E  GEGNER";
                    back = false;
                    break;
                default:
                    hint = "KLICK  WÄHLEN   RECHTSKLICK  SCHLIESSEN   ESC  MENÜ";
                    break;
            }
            bool delete = _Screen == G10_TowerDefenseScreen.SlotSelection && !G10_TowerDefenseSaves.Slots[_SlotSelection].IsEmpty;
            if (_Dialog != G10_TowerDefenseDialog.None)
            {
                hint = _Dialog == G10_TowerDefenseDialog.Defeat || _Dialog == G10_TowerDefenseDialog.MapComplete
                    ? "KLICK / ENTER  AUSWÄHLEN" : "KLICK / ENTER  AUSWÄHLEN   ESC  ABBRECHEN";
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
            // On the plain map the HUD bar above the map has all buttons; the map stays uncovered.
            if (_Screen == G10_TowerDefenseScreen.Map && _Dialog == G10_TowerDefenseDialog.None && !_Paused && !_ShowHelp) return;

            Surface.Rect(0, 170, DesignWidth, 10, Color.FromArgb(200, 20, 18, 16));
            Surface.Rect(0, 170, DesignWidth, 1, Color.FromArgb(200, 0, 0, 0));
            Surface.Text(hint, 4, 171, 5, TextDim);
            double right = DesignWidth - 2;
            if (menu) right = DrawFooterButton("MENÜ", right, () => { _Paused = true; _PauseSelection = 0; });
            if (back) right = DrawFooterButton("ZURÜCK", right, GoBack);
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

        static string GetMultiplierText(G10_TowerDefenseDifficulty difficulty)
        {
            return "x" + G10_TowerDefenseDifficultyRules.GetMultiplier(difficulty).ToString("0.00");
        }
    }
}
