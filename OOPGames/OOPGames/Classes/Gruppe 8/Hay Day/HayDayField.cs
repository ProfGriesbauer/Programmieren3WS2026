using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OOPGames
{
    /// <summary>
    /// Gemeinsamer Hofzustand. Die Spiellogik ist nach Aufgaben in weitere Partial-Dateien aufgeteilt.
    /// </summary>
    public sealed partial class HayDayField : IGameField
    {
        private const int CurrentSaveVersion = 1;
        public const int Rows = 50;
        public const int Columns = 100;
        public const int InitialPlotCount = 6;
        public const string Wheat = "Weizen";
        public const string Corn = "Mais";
        public const string Carrots = "Karotten";
        public const string Soybeans = "Soja";
        public const string Eggs = "Eier";
        public const string Feed = "Futter";
        public const string CowFeed = "Kuhfutter";
        public const string SheepFeed = "Schaffutter";
        public const string PigFeed = "Schweinefutter";
        public const string GoatFeed = "Ziegenfutter";
        public const string Milk = "Milch";
        public const string Wool = "Wolle";
        public const string Cheese = "Käse";
        public const string Pork = "Fleisch";
        public const string GoatMilk = "Ziegenmilch";

        public const int ShopMenuOption = -3;
        private static readonly string[] ShopKinds =
        {
            "Field", "Coop", "CowBarn", "SheepBarn", "Barn", "Silo", "Mill", "Dairy", "Market",
            "PigBarn", "GoatBarn"
        };
        private static readonly string[] CropKinds =
        {
            Wheat, Corn, Carrots, Soybeans
        };
        private static readonly string[] MarketItems =
        {
            Wheat, Corn, Carrots, Soybeans, Eggs, Milk, Wool, Cheese, Pork, GoatMilk
        };
        private static readonly string[] InventoryItems =
        {
            Wheat, Corn, Carrots, Soybeans, Eggs, Feed, CowFeed, SheepFeed, PigFeed,
            GoatFeed, Milk, Wool, Cheese, Pork, GoatMilk
        };

        private static string _savePathOverride;
        private static string SavePath
        {
            get
            {
                return _savePathOverride ?? Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "OOPGames", "HayDaySave.json");
            }
        }
        private DateTime _lastAdvancedAt;
        private DateTime _lastSavedAt;
        private int _eggProgressSeconds;
        private int _feedProgressSeconds;
        private int _cowProgressSeconds;
        private int _cowFeedProgressSeconds;
        private int _sheepProgressSeconds;
        private int _sheepFeedProgressSeconds;
        private int _pigProgressSeconds;
        private int _pigFeedProgressSeconds;
        private int _goatProgressSeconds;
        private int _goatFeedProgressSeconds;
        private int _eggReady;
        private int _milkReady;
        private int _porkReady;
        private int _woolReady;
        private int _goatMilkReady;
        private int _marketDay = int.MinValue;
        private int _orderSequence;
        private bool _pendingBuildingPlacement;
        // Diese Referenzen halten ein Objekt nur während des Verschiebevorgangs fest.
        private FarmPlot _movingPlot;
        private FarmBuilding _movingBuilding;

        private HayDayField()
        {
            Inventory = new Dictionary<string, int>();
            Plots = new List<FarmPlot>();
            Buildings = new List<FarmBuilding>();
            MarketOffers = new List<MarketOffer>();
            StatusMessage = "Willkommen auf deinem Hof!";
        }

        public int Coins { get; private set; }
        public int BarnCapacity { get; private set; }
        public int SiloCapacity { get; private set; }
        public int ChickenCount { get; private set; }
        public int CowCount { get; private set; }
        public int PigCount { get; private set; }
        public int SheepCount { get; private set; }
        public int GoatCount { get; private set; }
        public int EggReady { get { return _eggReady; } }
        public int MilkReady { get { return _milkReady; } }
        public int PorkReady { get { return _porkReady; } }
        public int WoolReady { get { return _woolReady; } }
        public int GoatMilkReady { get { return _goatMilkReady; } }
        public int MillLevel { get; private set; }
        public int SelectedCropIndex { get; private set; }
        public int SelectedBuildingIndex { get; private set; }
        public int CameraRow { get; private set; } = 26;
        public int CameraColumn { get; private set; } = 50;
        public double Zoom { get; private set; } = 1.0;
        public Dictionary<string, int> Inventory { get; private set; }
        public List<FarmPlot> Plots { get; private set; }
        public List<FarmBuilding> Buildings { get; private set; }
        public List<MarketOffer> MarketOffers { get; private set; }
        public MarketOrder Order { get; private set; }
        public string StatusMessage { get; private set; }
        public string MovingItemName
        {
            get
            {
                if (_movingBuilding != null)
                {
                    return _movingBuilding.Name;
                }

                return _movingPlot == null ? null : "Acker";
            }
        }
        public DateTime LastSavedAt { get; private set; }
        public bool IsDirty { get; internal set; }
        public int SelectedBuildingRow { get; private set; } = -1;
        public int SelectedBuildingColumn { get; private set; } = -1;
        public int SelectedPlotRow { get; private set; } = -1;
        public int SelectedPlotColumn { get; private set; } = -1;
        public bool IsShopOpen { get; private set; }
        public bool HasOpenMenu
        {
            get { return IsShopOpen || SelectedPlotRow >= 0 || OpenBuilding != null; }
        }
        public FarmBuilding OpenBuilding
        {
            get
            {
                return Buildings.FirstOrDefault(building =>
                    building.Row == SelectedBuildingRow && building.Column == SelectedBuildingColumn);
            }
        }

        public string SelectedCrop
        {
            get { return CropKinds[Math.Max(0, Math.Min(CropKinds.Length - 1, SelectedCropIndex))]; }
        }

        public static string[] BuildableKinds
        {
            get { return (string[])ShopKinds.Clone(); }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is HayDayPainter;
        }

        public void AdvanceTime(DateTime now)
        {
            if (now < _lastAdvancedAt)
            {
                return;
            }

            int elapsed = (int)Math.Min(int.MaxValue, (now - _lastAdvancedAt).TotalSeconds);
            if (elapsed > 0)
            {
                AdvanceAnimalProduction(elapsed, ChickenCount, Eggs, ref _feedProgressSeconds);
                AdvanceAnimalProduction(elapsed, CowCount, Milk, ref _cowFeedProgressSeconds);
                AdvanceAnimalProduction(elapsed, SheepCount, Wool, ref _sheepFeedProgressSeconds);
                AdvanceAnimalProduction(elapsed, PigCount, Pork, ref _pigFeedProgressSeconds);
                AdvanceAnimalProduction(elapsed, GoatCount, GoatMilk, ref _goatFeedProgressSeconds);
                _lastAdvancedAt = now;
            }

            foreach (FarmPlot plot in Plots)
            {
                if (plot.MarkRipeIfNeeded(now))
                {
                    IsDirty = true;
                }
                else if (elapsed > 0 && plot.Crop != null)
                {
                    IsDirty = true;
                }
            }

            RefreshMarket(now);
            if (now - _lastSavedAt >= TimeSpan.FromSeconds(30))
            {
                Save();
            }
        }

        public void ResetCamera()
        {
            CameraRow = 26;
            CameraColumn = 50;
            Zoom = 1.0;
            IsDirty = true;
        }

        public PointOnFarm ScreenToFarm(int x, int y, double canvasWidth, double canvasHeight)
        {
            double tileWidth = TileWidth(canvasWidth, canvasHeight);
            double tileHeight = tileWidth * 0.5;
            double dx = (x - canvasWidth / 2) / (tileWidth / 2);
            double dy = (y - canvasHeight / 2) / (tileHeight / 2);
            int column = (int)Math.Round(CameraColumn + (dx + dy) / 2);
            int row = (int)Math.Round(CameraRow + (dy - dx) / 2);
            return new PointOnFarm(row, column);
        }

        public double TileWidth(double canvasWidth, double canvasHeight)
        {
            double available = Math.Max(1, Math.Min(canvasWidth, canvasHeight));
            return Math.Max(8, available / 25 * Zoom);
        }

        public PointOnFarm BuildingPosition(FarmBuilding building)
        {
            return new PointOnFarm(building.Row, building.Column);
        }

        public bool IsRoad(int row, int column)
        {
            return column == 50 && row >= 0 && row <= 24;
        }
    }

}
