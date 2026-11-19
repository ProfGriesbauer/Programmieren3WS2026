using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class HayDayField : IGameField                                                        
    {
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
        private bool _pendingBuildingPlacement;

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

        public static HayDayField CreateNewField()
        {
            return CreateNew();
        }

        public static void SetSavePathForTesting(string savePath)
        {
            _savePathOverride = savePath;
        }

        public static void ClearSavePathForTesting()
        {
            _savePathOverride = null;
        }

        public static HayDayField LoadOrCreate()
        {
            HayDayField field;
            if (File.Exists(SavePath))
            {
                HayDaySaveData data;
                try
                {
                    data = JsonSerializer.Deserialize<HayDaySaveData>(File.ReadAllText(SavePath));
                }
                catch (JsonException exception)
                {
                    throw new InvalidDataException("Der Hay-Day-Spielstand ist beschädigt: " + SavePath, exception);
                }

                if (data == null)
                {
                    throw new InvalidDataException("Der Hay-Day-Spielstand ist leer: " + SavePath);
                }

                field = FromSaveData(data);
            }
            else
            {
                field = CreateNew();
            }

            field._lastAdvancedAt = field.LastSavedAt == default(DateTime)
                ? DateTime.UtcNow
                : field.LastSavedAt;
            field.AdvanceTime(DateTime.UtcNow);
            field.RefreshMarket(DateTime.UtcNow);
            field.SetStatus("Spielstand geladen. Letzter Speicherzeitpunkt: " +
                (field.LastSavedAt == default(DateTime)
                    ? "Neuer Hof"
                    : field.LastSavedAt.ToLocalTime().ToString("dd.MM.yyyy HH:mm:ss")));
            field.Save();
            return field;
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is HayDayPainter;
        }

        public void Apply(HayDayMove move)
        {
            if (move == null || move.PlayerNumber < 1 || move.PlayerNumber > 2)
            {
                return;
            }

            if (move.MoveKind == HayDayMoveKind.Key)
            {
                ApplyKey(move.Key);
            }
            else if (move.IsMenuClick)
            {
                if (move.MenuOption == ShopMenuOption)
                {
                    OpenShop();
                }
                else if (HasOpenMenu)
                {
                    ExecuteMenuOption(move.MenuOption);
                }
            }
            else
            {
                ApplyClick(move.Row, move.Column, move.Button);
            }
        }

        public string[] GetOpenMenuInfo()
        {
            if (IsShopOpen)
            {
                return new[]
                {
                    "Auswählen, dann rechtsklicken zum Platzieren.",
                    "Münzen: " + Coins
                };
            }

            if (SelectedPlotRow >= 0)
            {
                return new[] { "Saatgut für das nächste Pflanzen wählen.",
                    "Bestand: " + string.Join(" | ", CropKinds.Select(crop =>
                        crop + " " + Inventory[crop])) };
            }

            FarmBuilding building = OpenBuilding;
            if (building == null)
            {
                return new string[0];
            }

            switch (building.Kind)
            {
                case "Barn":
                    return new[]
                    {
                        "Eier: " + Inventory[Eggs],
                        "Futter: " + Inventory[Feed],
                        "Kapazität: " + BarnCapacity
                    };
                case "Silo":
                    return new[]
                    {
                        "Weizen: " + Inventory[Wheat],
                        "Mais: " + Inventory[Corn],
                        "Kapazität: " + SiloCapacity
                    };
                case "Mill":
                    return new[]
                    {
                        "Mühlen verarbeiten Feldfrüchte zu Tierfutter.",
                        "Hühner: " + Inventory[Feed] + " | Kühe: " + Inventory[CowFeed],
                        "Schweine: " + Inventory[PigFeed] + " | Schafe: " + Inventory[SheepFeed],
                        "Ziegen: " + Inventory[GoatFeed]
                    };
                case "Coop":
                    return new[]
                    {
                        "Produziert Eier mit Futter",
                        "Hühner: " + ChickenCount,
                        "Futter: " + Inventory[Feed],
                        "Bereit zum Einsammeln: " + EggReady + " Eier"
                    };
                case "CowBarn":
                    return new[] { "Kühe: " + CowCount, "Milch: " + Inventory[Milk],
                        "Futter: " + Inventory[CowFeed],
                        "Bereit zum Einsammeln: " + MilkReady };
                case "PigBarn":
                    return new[] { "Schweine: " + PigCount, "Fleisch: " + Inventory[Pork],
                        "Futter: " + Inventory[PigFeed],
                        "Bereit zum Einsammeln: " + PorkReady };
                case "SheepBarn":
                    return new[] { "Schafe: " + SheepCount, "Wolle: " + Inventory[Wool],
                        "Futter: " + Inventory[SheepFeed],
                        "Bereit zum Einsammeln: " + WoolReady };
                case "GoatBarn":
                    return new[] { "Ziegen: " + GoatCount, "Ziegenmilch: " + Inventory[GoatMilk],
                        "Futter: " + Inventory[GoatFeed],
                        "Bereit zum Einsammeln: " + GoatMilkReady };
                case "Dairy":
                    return new[] { "Käse aus 2 Milch herstellen.",
                        "Milch: " + Inventory[Milk], "Käse: " + Inventory[Cheese] };
                case "Market":
                    return new[]
                    {
                        "Verkaufsangebote:",
                        "Feldfrüchte: Weizen " + MarketPrice(Wheat) + ", Mais " + MarketPrice(Corn)
                            + ", Karotten " + MarketPrice(Carrots) + ", Soja " + MarketPrice(Soybeans) + " M",
                        "Produkte: Eier " + MarketPrice(Eggs) + ", Milch " + MarketPrice(Milk)
                            + ", Wolle " + MarketPrice(Wool) + ", Käse " + MarketPrice(Cheese) + " M",
                        "Fleisch " + MarketPrice(Pork) + " | Ziegenmilch " + MarketPrice(GoatMilk) + " M",
                        "Lieferwagen: " + (Order == null ? "keine offene Anfrage" : Order.Description)
                    };
                case "Van":
                    return Order == null
                        ? new[] { "Keine offene Lieferung" }
                        : new[] { "Lieferauftrag:", Order.Quantity + " " + Order.Item,
                            "Belohnung: " + Order.Reward + " Münzen" };
                default:
                    return new[] { "Münzen: " + Coins, "Dein Bauernhof" };
            }
        }

        public string GetOpenMenuTitle()
        {
            if (IsShopOpen)
            {
                return "Hofladen";
            }

            if (SelectedPlotRow >= 0)
            {
                return "Saatgut auswählen";
            }

            return OpenBuilding == null ? "" : OpenBuilding.Name;
        }

        public string[] GetOpenMenuOptions()
        {
            if (IsShopOpen)
            {
                return ShopKinds.Select(kind => BuildingLabel(kind) + " kaufen (" +
                    BuildingCost(kind) + " Münzen)").ToArray();
            }

            if (SelectedPlotRow >= 0)
            {
                return CropKinds.Select((crop, index) =>
                    crop + (SelectedCropIndex == index ? " (ausgewählt)" : "")).ToArray();
            }

            FarmBuilding building = OpenBuilding;
            if (building == null)
            {
                return new string[0];
            }

            switch (building.Kind)
            {
                case "Mill":
                    return new[]
                    {
                        "Hühnerfutter: 1 Weizen + 1 Mais",
                        "Kuhfutter: 2 Weizen + 1 Mais",
                        "Schaffutter: 1 Weizen + 2 Mais",
                        "Schweinefutter: 1 Mais + 1 Karotte",
                        "Ziegenfutter: 1 Weizen + 1 Soja"
                    };
                case "Coop":
                    return new[] { "Mit Hühnerfutter füttern", "Eier einsammeln", "Eier verkaufen" };
                case "CowBarn":
                    return new[] { "Mit Kuhfutter füttern", "Milch einsammeln", "Milch verkaufen" };
                case "PigBarn":
                    return new[] { "Mit Schweinefutter füttern", "Fleisch einsammeln", "Fleisch verkaufen" };
                case "SheepBarn":
                    return new[] { "Mit Schaffutter füttern", "Wolle einsammeln", "Wolle verkaufen" };
                case "GoatBarn":
                    return new[] { "Mit Ziegenfutter füttern", "Ziegenmilch einsammeln", "Ziegenmilch verkaufen" };
                case "Market":
                    return MarketItems.Select(item => item + " verkaufen (" +
                        MarketPrice(item) + " Münzen)").Concat(new[]
                    {
                        "5 Weizen kaufen (" + PurchasePrice(Wheat, 5) + " Münzen)",
                        "5 Mais kaufen (" + PurchasePrice(Corn, 5) + " Münzen)",
                        "5 Karottensamen kaufen (" + SeedPurchasePrice(Carrots, 5) + " Münzen)",
                        "5 Sojasamen kaufen (" + SeedPurchasePrice(Soybeans, 5) + " Münzen)",
                        "Lieferung abgeben"
                    }).ToArray();
                case "Van":
                    return new[] { "Lieferung abgeben" };
                case "Dairy":
                    return new[] { "Käse herstellen (2 Milch)" };
                default:
                    return new[] { "Schließen" };
            }
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
                _eggProgressSeconds = AdvanceAnimalProduction(elapsed, ChickenCount, Feed, Eggs,
                    60, ref _feedProgressSeconds, _eggProgressSeconds);
                _cowProgressSeconds = AdvanceAnimalProduction(elapsed, CowCount, CowFeed, Milk,
                    300, ref _cowFeedProgressSeconds, _cowProgressSeconds);
                _sheepProgressSeconds = AdvanceAnimalProduction(elapsed, SheepCount, SheepFeed, Wool,
                    600, ref _sheepFeedProgressSeconds, _sheepProgressSeconds);
                _pigProgressSeconds = AdvanceAnimalProduction(elapsed, PigCount, PigFeed, Pork,
                    360, ref _pigFeedProgressSeconds, _pigProgressSeconds);
                _goatProgressSeconds = AdvanceAnimalProduction(elapsed, GoatCount, GoatFeed, GoatMilk,
                    300, ref _goatFeedProgressSeconds, _goatProgressSeconds);
                _lastAdvancedAt = now;
            }

            foreach (FarmPlot plot in Plots)
            {
                if (plot.MarkRipeIfNeeded(now))
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

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            LastSavedAt = DateTime.UtcNow;
            File.WriteAllText(SavePath, JsonSerializer.Serialize(ToSaveData(),
                new JsonSerializerOptions { WriteIndented = true }));
            _lastSavedAt = LastSavedAt;
            IsDirty = false;
        }

        public void ResetCamera()
        {
            CameraRow = 26;
            CameraColumn = 50;
            Zoom = 1.0;
            IsDirty = true;
        }

        private void ExecuteMenuOption(int option)
        {
            if (IsShopOpen)
            {
                if (option < 0 || option >= ShopKinds.Length)
                {
                    return;
                }

                SelectedBuildingIndex = option;
                string kind = ShopKinds[option];
                CloseOpenMenu();
                _pendingBuildingPlacement = true;
                SetStatus(BuildingLabel(kind) + " ausgewählt. Rechtsklick auf einen freien Platz zum " +
                    "Kaufen und Platzieren (" + BuildingCost(kind) + " Münzen).");
                return;
            }

            if (SelectedPlotRow >= 0)
            {
                if (option >= 0 && option < CropKinds.Length)
                {
                    SelectedCropIndex = option;
                    FarmPlot plot = Plots.FirstOrDefault(value =>
                        value.Row == SelectedPlotRow && value.Column == SelectedPlotColumn);
                    CloseOpenMenu();
                    if (plot == null || plot.Crop != null)
                    {
                        SetStatus("Das Feld ist nicht mehr zum Pflanzen frei.");
                        return;
                    }

                    if (RemoveInventory(SelectedCrop, 1))
                    {
                        plot.Plant(SelectedCrop, DateTime.UtcNow);
                        SetStatus(SelectedCrop + " gepflanzt. Reif nach " +
                            FarmPlot.GrowthDuration(SelectedCrop).TotalMinutes.ToString("0") + " Minuten.");
                    }
                    else
                    {
                        SetStatus("Nicht genug " + SelectedCrop + " im Silo.");
                    }
                }
                return;
            }

            FarmBuilding building = OpenBuilding;
            if (building == null)
            {
                return;
            }

            string[] options = GetOpenMenuOptions();
            if (option < 0 || option >= options.Length)
            {
                return;
            }

            switch (building.Kind)
            {
                case "Mill":
                    ProduceFeed(option);
                    break;
                case "Coop":
                    if (option == 0) FeedAnimal("Coop");
                    else if (option == 1) CollectAnimalProduct(Eggs, "Eier");
                    else Sell(Eggs, 1);
                    break;
                case "CowBarn":
                    if (option == 0) FeedAnimal("CowBarn");
                    else if (option == 1) CollectAnimalProduct(Milk, "Milch");
                    else Sell(Milk, 1);
                    break;
                case "PigBarn":
                    if (option == 0) FeedAnimal("PigBarn");
                    else if (option == 1) CollectAnimalProduct(Pork, "Fleisch");
                    else Sell(Pork, 1);
                    break;
                case "SheepBarn":
                    if (option == 0) FeedAnimal("SheepBarn");
                    else if (option == 1) CollectAnimalProduct(Wool, "Wolle");
                    else Sell(Wool, 1);
                    break;
                case "GoatBarn":
                    if (option == 0) FeedAnimal("GoatBarn");
                    else if (option == 1) CollectAnimalProduct(GoatMilk, "Ziegenmilch");
                    else Sell(GoatMilk, 1);
                    break;
                case "Market":
                    if (option < MarketItems.Length)
                    {
                        Sell(MarketItems[option], 1);
                    }
                    else
                    {
                        switch (option - MarketItems.Length)
                        {
                            case 0: Buy(Wheat, 5); break;
                            case 1: Buy(Corn, 5); break;
                            case 2: Buy(Carrots, 5, true); break;
                            case 3: Buy(Soybeans, 5, true); break;
                            case 4: FulfillOrder(); break;
                        }
                    }
                    break;
                case "Van":
                    FulfillOrder();
                    break;
                case "Dairy":
                    ProduceCheese();
                    break;
                default:
                    SelectedBuildingRow = -1;
                    SelectedBuildingColumn = -1;
                    SetStatus("Gebäudemenü geschlossen.");
                    break;
            }

            IsDirty = true;
        }

        private void OpenShop()
        {
            if (IsShopOpen)
            {
                CloseOpenMenu();
                SetStatus("Hofladen geschlossen.");
                return;
            }

            CloseOpenMenu();
            IsShopOpen = true;
            SetStatus("Hofladen geöffnet.");
        }

        private void CloseOpenMenu()
        {
            IsShopOpen = false;
            SelectedPlotRow = -1;
            SelectedPlotColumn = -1;
            SelectedBuildingRow = -1;
            SelectedBuildingColumn = -1;
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

        public int MarketPrice(string item)
        {
            MarketOffer offer = MarketOffers.FirstOrDefault(value => value.Item == item);
            if (offer != null)
            {
                return offer.Price;
            }

            switch (item)
            {
                case Wheat: return 3;
                case Corn: return 4;
                case Carrots: return 2;
                case Soybeans: return 3;
                case Eggs: return 6;
                case Milk: return 8;
                case Wool: return 7;
                case Cheese: return 12;
                case Pork: return 10;
                case GoatMilk: return 9;
                default: return 1;
            }
        }

        private static HayDayField CreateNew()
        {
            HayDayField field = new HayDayField
            {
                Coins = 500,
                BarnCapacity = 100,
                SiloCapacity = 100,
                ChickenCount = 5,
                CowCount = 0,
                PigCount = 0,
                SheepCount = 0,
                GoatCount = 0,
                MillLevel = 1,
                SelectedCropIndex = 0,
                SelectedBuildingIndex = 0,
                CameraRow = 26,
                CameraColumn = 50,
                Zoom = 1.0,
                LastSavedAt = DateTime.UtcNow,
                _lastAdvancedAt = DateTime.UtcNow,
                _lastSavedAt = DateTime.UtcNow
            };
            field.Inventory[Wheat] = 10;
            field.Inventory[Corn] = 10;
            field.Inventory[Carrots] = 0;
            field.Inventory[Soybeans] = 0;
            field.Inventory[Eggs] = 0;
            field.Inventory[Feed] = 3;
            field.Inventory[CowFeed] = 0;
            field.Inventory[SheepFeed] = 0;
            field.Inventory[PigFeed] = 0;
            field.Inventory[GoatFeed] = 0;
            field.Inventory[Milk] = 0;
            field.Inventory[Wool] = 0;
            field.Inventory[Cheese] = 0;
            field.Inventory[Pork] = 0;
            field.Inventory[GoatMilk] = 0;

            for (int row = 27; row <= 28; row++)
            {
                for (int column = 47; column <= 49; column++)
                {
                    field.Plots.Add(new FarmPlot(row, column));
                }
            }

            field.Buildings.Add(new FarmBuilding("Farmhouse", "Farmhaus", 25, 50));
            field.Buildings.Add(new FarmBuilding("Barn", "Scheune", 22, 46));
            field.Buildings.Add(new FarmBuilding("Silo", "Silo", 22, 54));
            field.Buildings.Add(new FarmBuilding("Coop", "Hühnerstall", 31, 57));
            field.Buildings.Add(new FarmBuilding("Mill", "Futtermühle", 31, 44));
            field.Buildings.Add(new FarmBuilding("Market", "Marktstand", 26, 61));
            field.Buildings.Add(new FarmBuilding("Van", "Lieferwagen", 19, 50));
            field.RefreshMarket(DateTime.UtcNow);
            return field;
        }

        private static HayDayField FromSaveData(HayDaySaveData data)
        {
            if (data.Inventory == null || data.Plots == null || data.Buildings == null)
            {
                throw new InvalidDataException("Der Hay-Day-Spielstand enthält unvollständige Daten.");
            }

            HayDayField field = new HayDayField
            {
                Coins = Math.Max(0, data.Coins),
                BarnCapacity = Math.Max(20, data.BarnCapacity),
                SiloCapacity = Math.Max(20, data.SiloCapacity),
                ChickenCount = Math.Max(5, data.ChickenCount),
                CowCount = Math.Max(0, data.CowCount),
                PigCount = Math.Max(0, data.PigCount),
                SheepCount = Math.Max(0, data.SheepCount),
                GoatCount = Math.Max(0, data.GoatCount),
                MillLevel = Math.Max(1, data.MillLevel),
                SelectedCropIndex = Math.Max(0, Math.Min(CropKinds.Length - 1, data.SelectedCropIndex)),
                SelectedBuildingIndex = Math.Max(0, data.SelectedBuildingIndex) % BuildableKinds.Length,
                CameraRow = Math.Max(0, Math.Min(Rows - 1, data.CameraRow)),
                CameraColumn = Math.Max(0, Math.Min(Columns - 1, data.CameraColumn)),
                Zoom = Math.Max(0.5, Math.Min(3.0, data.Zoom)),
                LastSavedAt = data.LastSavedAt,
                _eggProgressSeconds = Math.Max(0, data.EggProgressSeconds) % 60,
                _feedProgressSeconds = Math.Max(0, data.FeedProgressSeconds) % 300,
                _cowProgressSeconds = Math.Max(0, data.CowProgressSeconds) % 300,
                _cowFeedProgressSeconds = Math.Max(0, data.CowFeedProgressSeconds) % 300,
                _sheepProgressSeconds = Math.Max(0, data.SheepProgressSeconds) % 600,
                _sheepFeedProgressSeconds = Math.Max(0, data.SheepFeedProgressSeconds) % 300,
                _pigProgressSeconds = Math.Max(0, data.PigProgressSeconds) % 360,
                _pigFeedProgressSeconds = Math.Max(0, data.PigFeedProgressSeconds) % 300,
                _goatProgressSeconds = Math.Max(0, data.GoatProgressSeconds) % 300,
                _goatFeedProgressSeconds = Math.Max(0, data.GoatFeedProgressSeconds) % 300,
                _eggReady = Math.Max(0, data.EggReady),
                _milkReady = Math.Max(0, data.MilkReady),
                _porkReady = Math.Max(0, data.PorkReady),
                _woolReady = Math.Max(0, data.WoolReady),
                _goatMilkReady = Math.Max(0, data.GoatMilkReady),
                _marketDay = data.MarketDay
            };

            foreach (string item in InventoryItems)
            {
                int amount;
                int capacity = CropKinds.Contains(item) ? field.SiloCapacity : field.BarnCapacity;
                field.Inventory[item] = data.Inventory.TryGetValue(item, out amount)
                    ? Math.Min(capacity, Math.Max(0, amount))
                    : 0;
            }

            foreach (FarmPlotSaveData plot in data.Plots)
            {
                if (IsInside(plot.Row, plot.Column))
                {
                    field.Plots.Add(new FarmPlot(plot.Row, plot.Column, plot.Crop, plot.PlantedAtUtc));
                }
            }

            foreach (FarmBuildingSaveData building in data.Buildings)
            {
                if (IsInside(building.Row, building.Column) && !string.IsNullOrWhiteSpace(building.Kind))
                {
                    field.Buildings.Add(new FarmBuilding(building.Kind, building.Name, building.Row, building.Column));
                }
            }

            if (field.Plots.Count == 0 || field.Buildings.Count == 0)
            {
                throw new InvalidDataException("Der Hay-Day-Spielstand besitzt kein gültiges Spielfeld.");
            }

            field.Order = data.Order == null
                ? null
                : new MarketOrder(data.Order.Item, data.Order.Quantity, data.Order.Reward);
            if (data.MarketOffers != null)
            {
                field.MarketOffers.AddRange(data.MarketOffers
                    .Where(offer => offer != null && !string.IsNullOrWhiteSpace(offer.Item))
                    .Select(offer => new MarketOffer(offer.Item, offer.Price)));
            }

            return field;
        }

        private void ApplyClick(int row, int column, int button)
        {
            if (HasOpenMenu)
            {
                CloseOpenMenu();
            }

            if (!IsInside(row, column))
            {
                return;
            }

            FarmPlot plot = Plots.FirstOrDefault(value => value.Row == row && value.Column == column);
            FarmBuilding building = Buildings.FirstOrDefault(value => value.Row == row && value.Column == column);
            bool rightClick = button == 1;
            if (plot != null)
            {
                if (rightClick)
                {
                    if (plot.Crop != null)
                    {
                        SetStatus("Reife Pflanzen werden geerntet. Nur leere Felder wählen Saatgut.");
                        return;
                    }

                    SelectedPlotRow = plot.Row;
                    SelectedPlotColumn = plot.Column;
                    SetStatus("Saatgut auswählen.");
                    return;
                }

                if (plot.Crop == null)
                {
                    if (RemoveInventory(SelectedCrop, 1))
                    {
                        plot.Plant(SelectedCrop, DateTime.UtcNow);
                        SetStatus(SelectedCrop + " gepflanzt. Reif nach " +
                            FarmPlot.GrowthDuration(SelectedCrop).TotalMinutes.ToString("0") + " Minuten.");
                    }
                    else
                    {
                        SetStatus("Nicht genug " + SelectedCrop + " im Silo.");
                    }
                }
                else if (plot.IsRipe(DateTime.UtcNow))
                {
                    Harvest(plot);
                }
                else
                {
                    SetStatus(plot.Crop + " wächst noch - reif in " +
                        plot.RemainingGrowthText(DateTime.UtcNow) + ".");
                }

                return;
            }

            if (building != null)
            {
                if (!rightClick)
                {
                    SelectedBuildingRow = building.Row;
                    SelectedBuildingColumn = building.Column;
                    SetStatus(building.Name + " geöffnet.");
                }
                else
                {
                    InteractWithBuilding(building, true);
                }
                return;
            }

            if (rightClick)
            {
                if (!_pendingBuildingPlacement)
                {
                    SetStatus("Wähle zuerst im Shop ein Gebäude oder einen Acker.");
                    return;
                }

                PlaceBuilding(row, column);
            }
            else
            {
                SetStatus("B wählt ein Gebäude; Rechtsklick baut es auf einem freien Feld.");
            }
        }

        private void ApplyKey(Key key)
        {
            switch (key)
            {
                case Key.Left:
                    CameraColumn = Math.Max(0, CameraColumn - 2);
                    SetStatus("Kamera bewegt.");
                    break;
                case Key.Right:
                    CameraColumn = Math.Min(Columns - 1, CameraColumn + 2);
                    SetStatus("Kamera bewegt.");
                    break;
                case Key.Up:
                    CameraRow = Math.Max(0, CameraRow - 2);
                    SetStatus("Kamera bewegt.");
                    break;
                case Key.Down:
                    CameraRow = Math.Min(Rows - 1, CameraRow + 2);
                    SetStatus("Kamera bewegt.");
                    break;
                case Key.Add:
                case Key.OemPlus:
                    Zoom = Math.Min(3.0, Zoom + 0.2);
                    SetStatus("Zoom: " + Zoom.ToString("0.0") + "x");
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    Zoom = Math.Max(0.5, Zoom - 0.2);
                    SetStatus("Zoom: " + Zoom.ToString("0.0") + "x");
                    break;
                case Key.B:
                    SelectedBuildingIndex = (SelectedBuildingIndex + 1) % BuildableKinds.Length;
                    SetStatus("Bauauswahl: " + BuildingLabel(BuildableKinds[SelectedBuildingIndex]) + ".");
                    break;
                case Key.D1:
                case Key.NumPad1:
                    SelectedCropIndex = 0;
                    SetStatus("Ausgewählt: Weizen.");
                    break;
                case Key.D2:
                case Key.NumPad2:
                    SelectedCropIndex = 1;
                    SetStatus("Ausgewählt: Mais.");
                    break;
                case Key.D3:
                case Key.NumPad3:
                    SelectedCropIndex = 2;
                    SetStatus("Ausgewählt: Karotten.");
                    break;
                case Key.D4:
                case Key.NumPad4:
                    SelectedCropIndex = 3;
                    SetStatus("Ausgewählt: Soja.");
                    break;
                case Key.D5:
                case Key.NumPad5:
                    Sell(Eggs, 1);
                    break;
                default:
                    return;
            }

            IsDirty = true;
        }

        private void InteractWithBuilding(FarmBuilding building, bool rightClick)
        {
            switch (building.Kind)
            {
                case "Mill":
                    ProduceFeed(0);
                    break;
                case "Coop":
                    if (rightClick)
                    {
                        SetStatus("Hühner: " + ChickenCount + ". Futter: " + Inventory[Feed]
                            + ". Bereit: " + EggReady + " Eier.");
                    }
                    else
                    {
                        FeedAnimal("Coop");
                    }
                    break;
                case "Market":
                    if (rightClick)
                    {
                        FulfillOrder();
                    }
                    else
                    {
                        Sell(SelectedCrop, 1);
                    }
                    break;
                case "Van":
                    if (rightClick)
                    {
                        FulfillOrder();
                    }
                    else
                    {
                        SetStatus(Order == null ? "Lieferwagen ist bereit." : Order.Description);
                    }
                    break;
                case "Barn":
                    SetStatus("Scheune: " + Inventory[Eggs] + " Eier, " + Inventory[Feed] +
                        " Futter. Kapazität " + BarnCapacity + ".");
                    break;
                case "CowBarn":
                    if (rightClick)
                        SetStatus("Kühe: " + CowCount + ". Futter: " + Inventory[CowFeed]
                            + ". Bereit: " + MilkReady + " Milch.");
                    else
                        FeedAnimal("CowBarn");
                    break;
                case "PigBarn":
                    if (rightClick)
                        SetStatus("Schweine: " + PigCount + ". Futter: " + Inventory[PigFeed]
                            + ". Bereit: " + PorkReady + " Fleisch.");
                    else
                        FeedAnimal("PigBarn");
                    break;
                case "SheepBarn":
                    if (rightClick)
                        SetStatus("Schafe: " + SheepCount + ". Futter: " + Inventory[SheepFeed]
                            + ". Bereit: " + WoolReady + " Wolle.");
                    else
                        FeedAnimal("SheepBarn");
                    break;
                case "GoatBarn":
                    if (rightClick)
                        SetStatus("Ziegen: " + GoatCount + ". Futter: " + Inventory[GoatFeed]
                            + ". Bereit: " + GoatMilkReady + " Ziegenmilch.");
                    else
                        FeedAnimal("GoatBarn");
                    break;
                case "Silo":
                    SetStatus("Silo: " + Inventory[Wheat] + " Weizen, " + Inventory[Corn] +
                        " Mais, " + Inventory[Carrots] + " Karotten und " + Inventory[Soybeans] +
                        " Soja. Kapazität " + SiloCapacity + ". Tasten 1-4 wählen Saatgut.");
                    break;
                default:
                    SetStatus("Willkommen auf deinem Hof!");
                    break;
            }
        }

        private void ProduceFeed(int recipe)
        {
            string product;
            int wheat;
            int corn;
            int carrots = 0;
            int soybeans = 0;
            if (recipe == 0)
            {
                product = Feed;
                wheat = 1;
                corn = 1;
            }
            else if (recipe == 1)
            {
                product = CowFeed;
                wheat = 2;
                corn = 1;
            }
            else if (recipe == 2)
            {
                product = SheepFeed;
                wheat = 1;
                corn = 2;
            }
            else if (recipe == 3)
            {
                product = PigFeed;
                wheat = 0;
                corn = 1;
                carrots = 1;
            }
            else
            {
                product = GoatFeed;
                wheat = 1;
                corn = 0;
                soybeans = 1;
            }

            if (Inventory[product] >= BarnCapacity)
            {
                SetStatus("Der Lagerplatz für " + product + " ist voll.");
            }
            else if (Inventory[Wheat] >= wheat && Inventory[Corn] >= corn
                && Inventory[Carrots] >= carrots && Inventory[Soybeans] >= soybeans)
            {
                if (wheat > 0) RemoveInventory(Wheat, wheat);
                if (corn > 0) RemoveInventory(Corn, corn);
                if (carrots > 0) RemoveInventory(Carrots, carrots);
                if (soybeans > 0) RemoveInventory(Soybeans, soybeans);
                Inventory[product]++;
                string ingredients = string.Join(" und ", new[]
                {
                    wheat > 0 ? wheat + " Weizen" : null,
                    corn > 0 ? corn + " Mais" : null,
                    carrots > 0 ? carrots + " Karotten" : null,
                    soybeans > 0 ? soybeans + " Soja" : null
                }.Where(value => value != null));
                SetStatus(ingredients + " zu " + product + " verarbeitet.");
            }
            else
            {
                string missing = string.Join(" und ", new[]
                {
                    Inventory[Wheat] < wheat ? wheat + " Weizen" : null,
                    Inventory[Corn] < corn ? corn + " Mais" : null,
                    Inventory[Carrots] < carrots ? carrots + " Karotten" : null,
                    Inventory[Soybeans] < soybeans ? soybeans + " Soja" : null
                }.Where(value => value != null));
                SetStatus("Für " + product + " fehlen " + missing + ".");
            }
        }

        private void ProduceCheese()
        {
            if (Inventory[Cheese] >= BarnCapacity)
            {
                SetStatus("Der Lagerplatz für Käse ist voll.");
            }
            else if (RemoveInventory(Milk, 2))
            {
                Inventory[Cheese]++;
                SetStatus("2 Milch zu 1 Käse verarbeitet.");
            }
            else
            {
                SetStatus("Die Molkerei braucht 2 Milch.");
            }
        }

        public bool FeedAnimal(string kind)
        {
            string feedItem;
            int animalCount;
            switch (kind)
            {
                case "Coop":
                    feedItem = Feed;
                    animalCount = ChickenCount;
                    break;
                case "CowBarn":
                    feedItem = CowFeed;
                    animalCount = CowCount;
                    break;
                case "PigBarn":
                    feedItem = PigFeed;
                    animalCount = PigCount;
                    break;
                case "SheepBarn":
                    feedItem = SheepFeed;
                    animalCount = SheepCount;
                    break;
                case "GoatBarn":
                    feedItem = GoatFeed;
                    animalCount = GoatCount;
                    break;
                default:
                    return false;
            }

            if (animalCount <= 0)
            {
                SetStatus("In diesem Stall sind keine Tiere.");
                return false;
            }

            if (Inventory[feedItem] <= 0)
            {
                SetStatus("Zuerst in der Futtermühle " + feedItem + " herstellen.");
                return false;
            }

            RemoveInventory(feedItem, 1);
            SetStatus(feedItem + " verteilt. Die Tiere produzieren für 5 Minuten.");
            IsDirty = true;
            return true;
        }

        public bool CollectAnimalProduct(string productItem, string label)
        {
            int ready;
            switch (productItem)
            {
                case Eggs: ready = _eggReady; break;
                case Milk: ready = _milkReady; break;
                case Pork: ready = _porkReady; break;
                case Wool: ready = _woolReady; break;
                case GoatMilk: ready = _goatMilkReady; break;
                default: return false;
            }

            if (ready <= 0)
            {
                SetStatus("Noch keine " + label + " bereit.");
                return false;
            }

            if (!AddInventory(productItem, ready))
            {
                SetStatus("Kein Platz für " + label + " in der Scheune.");
                return false;
            }

            switch (productItem)
            {
                case Eggs: _eggReady = 0; break;
                case Milk: _milkReady = 0; break;
                case Pork: _porkReady = 0; break;
                case Wool: _woolReady = 0; break;
                case GoatMilk: _goatMilkReady = 0; break;
            }

            SetStatus(ready + " " + label + " eingesammelt.");
            IsDirty = true;
            return true;
        }

        private int AdvanceAnimalProduction(int elapsed, int animalCount, string feedItem,
            string productItem, int productIntervalSeconds, ref int feedProgressSeconds,
            int productProgressSeconds)
        {
            if (animalCount == 0)
            {
                return productProgressSeconds;
            }

            int capacity = CropKinds.Contains(productItem) ? SiloCapacity : BarnCapacity;
            int space = Math.Max(0, capacity - Inventory[productItem]);
            if (space == 0)
            {
                return productProgressSeconds;
            }

            long availableFeedSeconds = Math.Max(0L,
                (long)Inventory[feedItem] * 300 - feedProgressSeconds);
            long productCapacitySeconds = (long)Math.Ceiling(space / (double)animalCount) * productIntervalSeconds
                - productProgressSeconds;
            productCapacitySeconds = Math.Max(0L, productCapacitySeconds);
            int activeSeconds = (int)Math.Min((long)elapsed,
                Math.Min(availableFeedSeconds, productCapacitySeconds));
            long totalProductSeconds = productProgressSeconds + (long)activeSeconds;
            long produced = totalProductSeconds / productIntervalSeconds * animalCount;
            int stored = (int)Math.Min(produced, space);
            if (stored > 0)
            {
                switch (productItem)
                {
                    case Eggs: _eggReady += stored; break;
                    case Milk: _milkReady += stored; break;
                    case Pork: _porkReady += stored; break;
                    case Wool: _woolReady += stored; break;
                    case GoatMilk: _goatMilkReady += stored; break;
                }
                IsDirty = true;
            }

            long totalFeedSeconds = feedProgressSeconds + (long)activeSeconds;
            int feedUsed = (int)Math.Min(Inventory[feedItem], totalFeedSeconds / 300);
            feedProgressSeconds = (int)(totalFeedSeconds % 300);
            if (feedUsed > 0)
            {
                Inventory[feedItem] -= feedUsed;
                IsDirty = true;
            }

            return (int)(totalProductSeconds % productIntervalSeconds);
        }

        private void Buy(string item, int quantity, bool discountedSeed = false)
        {
            int price = discountedSeed ? SeedPurchasePrice(item, quantity) : PurchasePrice(item, quantity);
            int capacity = CropKinds.Contains(item) ? SiloCapacity : BarnCapacity;
            if (quantity <= 0 || Inventory[item] > capacity - quantity)
            {
                SetStatus("Nicht genug Platz im Lager für " + item + ".");
            }
            else if (Coins < price)
            {
                SetStatus("Dafür fehlen " + (price - Coins) + " Münzen.");
            }
            else
            {
                Coins -= price;
                Inventory[item] += quantity;
                SetStatus(quantity + " " + item + " für " + price + " Münzen gekauft.");
            }
        }

        public int PurchasePrice(string item, int quantity)
        {
            if (quantity <= 0 || !MarketItems.Contains(item))
            {
                return int.MaxValue;
            }

            return MarketPrice(item) * 2 * quantity;
        }

        public int SeedPurchasePrice(string item, int quantity)
        {
            if (quantity <= 0 || !CropKinds.Contains(item))
            {
                return int.MaxValue;
            }

            return MarketPrice(item) * quantity;
        }

        private void Harvest(FarmPlot plot)
        {
            string crop = plot.Crop;
            if (AddInventory(crop, 3))
            {
                plot.Clear();
                SetStatus("3 " + crop + " geerntet.");
            }
            else
            {
                SetStatus("Silo voll. Verkaufe erst Pflanzen am Marktstand.");
            }
        }

        private void PlaceBuilding(int row, int column)
        {
            if (IsRoad(row, column) || Plots.Any(plot => plot.Row == row && plot.Column == column) ||
                Buildings.Any(value => value.Row == row && value.Column == column))
            {
                SetStatus("Hier ist kein Bauplatz frei.");
                return;
            }

            string kind = BuildableKinds[SelectedBuildingIndex];
            int cost = BuildingCost(kind);
            if (Coins < cost)
            {
                SetStatus(BuildingLabel(kind) + " kostet " + cost + " Münzen.");
                return;
            }

            Coins -= cost;
            _pendingBuildingPlacement = false;
            if (kind == "Field")
            {
                Plots.Add(new FarmPlot(row, column));
            }
            else
            {
                Buildings.Add(new FarmBuilding(kind, BuildingLabel(kind), row, column));
                if (kind == "Coop")
                {
                    ChickenCount += 5;
                }
                else if (kind == "CowBarn")
                {
                    CowCount += 2;
                }
                else if (kind == "PigBarn")
                {
                    PigCount += 2;
                }
                else if (kind == "SheepBarn")
                {
                    SheepCount += 2;
                }
                else if (kind == "GoatBarn")
                {
                    GoatCount += 2;
                }
                else if (kind == "Barn")
                {
                    BarnCapacity += 50;
                }
                else if (kind == "Silo")
                {
                    SiloCapacity += 50;
                }
                else if (kind == "Mill")
                {
                    MillLevel++;
                }
            }

            SetStatus(BuildingLabel(kind) + " gebaut für " + cost + " Münzen.");
        }

        private void Sell(string item, int amount)
        {
            int available = Inventory.ContainsKey(item) ? Inventory[item] : 0;
            int quantity = Math.Min(amount, available);
            if (quantity == 0)
            {
                SetStatus("Keine " + item + " zum Verkaufen.");
                return;
            }

            RemoveInventory(item, quantity);
            int earnings = MarketPrice(item) * quantity;
            Coins += earnings;
            SetStatus(quantity + " " + item + " für " + earnings + " Münzen verkauft.");
        }

        private void FulfillOrder()
        {
            if (Order == null)
            {
                SetStatus("Aktuell keine Bestellanfrage.");
                return;
            }

            if (!RemoveInventory(Order.Item, Order.Quantity))
            {
                SetStatus(Order.Description);
                return;
            }

            Coins += Order.Reward;
            SetStatus("Bestellung geliefert: +" + Order.Reward + " Münzen.");
            Order = null;
        }

        private void RefreshMarket(DateTime now)
        {
            int day = (int)(now.Date - new DateTime(2020, 1, 1)).TotalDays;
            if (day != _marketDay)
            {
                MarketOffers.Clear();
                int[] basePrices = { 3, 4, 2, 3, 6, 8, 7, 12, 10, 9 };
                for (int i = 0; i < MarketItems.Length; i++)
                {
                    int price = Math.Max(1, basePrices[i] + ((day * 7 + i * 11) % 5) - 2);
                    MarketOffers.Add(new MarketOffer(MarketItems[i], price));
                }

                Order = CreateOrder(now);
                _marketDay = day;
                IsDirty = true;
            }
        }

        private static MarketOrder CreateOrder(DateTime now)
        {
            int day = (int)(now.Date - new DateTime(2020, 1, 1)).TotalDays;
            int[] basePrices = { 3, 4, 2, 3, 6, 8, 7, 12, 10, 9 };
            int index = Math.Abs(day % MarketItems.Length);
            int quantity = 3 + day % 3;
            int price = Math.Max(1, basePrices[index] + ((day * 7 + index * 11) % 5) - 2);
            return new MarketOrder(MarketItems[index], quantity, quantity * price + 10);
        }

        private bool AddInventory(string item, int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            int capacity = CropKinds.Contains(item) ? SiloCapacity : BarnCapacity;
            int current = Inventory.ContainsKey(item) ? Inventory[item] : 0;
            if (current > capacity - amount)
            {
                return false;
            }

            Inventory[item] = current + amount;
            return true;
        }

        private bool RemoveInventory(string item, int amount)
        {
            int current;
            if (amount <= 0 || !Inventory.TryGetValue(item, out current) || current < amount)
            {
                return false;
            }

            Inventory[item] = current - amount;
            IsDirty = true;
            return true;
        }

        private void SetStatus(string message)
        {
            StatusMessage = message;
            IsDirty = true;
        }

        private HayDaySaveData ToSaveData()
        {
            return new HayDaySaveData
            {
                Coins = Coins,
                BarnCapacity = BarnCapacity,
                SiloCapacity = SiloCapacity,
                ChickenCount = ChickenCount,
                MillLevel = MillLevel,
                SelectedCropIndex = SelectedCropIndex,
                SelectedBuildingIndex = SelectedBuildingIndex,
                CameraRow = CameraRow,
                CameraColumn = CameraColumn,
                Zoom = Zoom,
                Inventory = new Dictionary<string, int>(Inventory),
                Plots = Plots.Select(plot => new FarmPlotSaveData
                {
                    Row = plot.Row,
                    Column = plot.Column,
                    Crop = plot.Crop,
                    PlantedAtUtc = plot.PlantedAtUtc
                }).ToList(),
                Buildings = Buildings.Select(building => new FarmBuildingSaveData
                {
                    Kind = building.Kind,
                    Name = building.Name,
                    Row = building.Row,
                    Column = building.Column
                }).ToList(),
                MarketOffers = MarketOffers.Select(offer => new MarketOfferSaveData
                {
                    Item = offer.Item,
                    Price = offer.Price
                }).ToList(),
                Order = Order == null ? null : new MarketOrderSaveData
                {
                    Item = Order.Item,
                    Quantity = Order.Quantity,
                    Reward = Order.Reward
                },
                EggProgressSeconds = _eggProgressSeconds,
                FeedProgressSeconds = _feedProgressSeconds,
                CowProgressSeconds = _cowProgressSeconds,
                CowFeedProgressSeconds = _cowFeedProgressSeconds,
                SheepProgressSeconds = _sheepProgressSeconds,
                SheepFeedProgressSeconds = _sheepFeedProgressSeconds,
                PigProgressSeconds = _pigProgressSeconds,
                PigFeedProgressSeconds = _pigFeedProgressSeconds,
                GoatProgressSeconds = _goatProgressSeconds,
                GoatFeedProgressSeconds = _goatFeedProgressSeconds,
                EggReady = _eggReady,
                MilkReady = _milkReady,
                PorkReady = _porkReady,
                WoolReady = _woolReady,
                GoatMilkReady = _goatMilkReady,
                CowCount = CowCount,
                PigCount = PigCount,
                SheepCount = SheepCount,
                GoatCount = GoatCount,
                MarketDay = _marketDay,
                LastSavedAt = DateTime.UtcNow
            };
        }

        private static bool IsInside(int row, int column)
        {
            return row >= 0 && row < Rows && column >= 0 && column < Columns;
        }

        public static string BuildingLabel(string kind)
        {
            switch (kind)
            {
                case "Field": return "Acker";
                case "Coop": return "Hühnerstall";
                case "CowBarn": return "Kuhstall";
                case "PigBarn": return "Schweinestall";
                case "SheepBarn": return "Schafstall";
                case "GoatBarn": return "Ziegenstall";
                case "Barn": return "Scheune";
                case "Silo": return "Silo";
                case "Mill": return "Futtermühle";
                case "Dairy": return "Molkerei";
                case "Market": return "Marktstand";
                default: return kind;
            }
        }

        public int BuildingCost(string kind)
        {
            int existing = kind == "Field"
                ? Math.Max(0, Plots.Count - InitialPlotCount)
                : Buildings.Count(value => value.Kind == kind);
            return BuildCost(kind) * (existing + 1);
        }

        private static int BuildCost(string kind)
        {
            switch (kind)
            {
                case "Field": return 100;
                case "Coop": return 250;
                case "CowBarn": return 450;
                case "PigBarn": return 450;
                case "SheepBarn": return 400;
                case "GoatBarn": return 400;
                case "Barn": return 200;
                case "Silo": return 200;
                case "Mill": return 300;
                case "Dairy": return 400;
                case "Market": return 350;
                default: return int.MaxValue;
            }
        }
    }

    public sealed class FarmPlot
    {
        public FarmPlot(int row, int column) : this(row, column, null, default(DateTime))
        {
        }

        public FarmPlot(int row, int column, string crop, DateTime plantedAtUtc)
        {
            Row = row;
            Column = column;
            Crop = crop;
            PlantedAtUtc = plantedAtUtc;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
        public string Crop { get; private set; }
        public DateTime PlantedAtUtc { get; private set; }
        public bool Ripe { get; private set; }

        public void Plant(string crop, DateTime plantedAt)
        {
            Crop = crop;
            PlantedAtUtc = plantedAt;
            Ripe = false;
        }

        public void Clear()
        {
            Crop = null;
            PlantedAtUtc = default(DateTime);
            Ripe = false;
        }

        public bool IsRipe(DateTime now)
        {
            return Crop != null && now - PlantedAtUtc >= GrowthDuration(Crop);
        }

        public static TimeSpan GrowthDuration(string crop)
        {
            return TimeSpan.FromMinutes(2);
        }

        public string RemainingGrowthText(DateTime now)
        {
            if (IsRipe(now))
            {
                return "Reife";
            }

            TimeSpan remaining = GrowthDuration(Crop) - (now - PlantedAtUtc);
            int seconds = Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds));
            return (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
        }

        public bool MarkRipeIfNeeded(DateTime now)
        {
            if (!Ripe && IsRipe(now))
            {
                Ripe = true;
                return true;
            }

            return false;
        }
    }

    public sealed class FarmBuilding
    {
        public FarmBuilding(string kind, string name, int row, int column)
        {
            Kind = kind;
            Name = name;
            Row = row;
            Column = column;
        }

        public string Kind { get; private set; }
        public string Name { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }
    }

    public sealed class MarketOffer
    {
        public MarketOffer(string item, int price)
        {
            Item = item;
            Price = price;
        }

        public string Item { get; private set; }
        public int Price { get; private set; }
    }

    public sealed class MarketOrder
    {
        public MarketOrder(string item, int quantity, int reward)
        {
            Item = item;
            Quantity = quantity;
            Reward = reward;
        }

        public string Item { get; private set; }
        public int Quantity { get; private set; }
        public int Reward { get; private set; }
        public string Description
        {
            get { return "Lieferanfrage: " + Quantity + " " + Item + " für " + Reward + " Münzen."; }
        }
    }

    public sealed class PointOnFarm
    {
        public PointOnFarm(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
    }

    public sealed class HayDayRules : IGameRules2
    {
        private HayDayField _field = HayDayField.LoadOrCreate();
        private int _nextPlayer = 1;
        private bool _prepared;

        public string Name { get { return "Hay Day - Bauernhof"; } }
        public IGameField CurrentField { get { return _field; } }
        public bool MovesPossible { get { return true; } }

        public void StartedGameCall()
        {
            _field = HayDayField.LoadOrCreate();
            _prepared = true;
        }

        public void TickGameCall()
        {
            _field.AdvanceTime(DateTime.UtcNow);
        }

        public void ClearField()
        {
            if (!_prepared)
            {
                _field = HayDayField.LoadOrCreate();
            }

            _prepared = false;
            _field.ResetCamera();
            _nextPlayer = 1;
        }

        public void DoMove(IPlayMove move)
        {
            HayDayMove farmMove = move as HayDayMove;
            if (farmMove == null || farmMove.PlayerNumber != _nextPlayer || !MovesPossible)
            {
                return;
            }

            _field.Apply(farmMove);
            _field.Save();
            _nextPlayer = _nextPlayer == 1 ? 2 : 1;
        }

        public int CheckIfPLayerWon()
        {
            return -1;
        }
    }

    public sealed class HayDayMove : IPlayMove
    {
        public HayDayMove(int playerNumber, HayDayMoveKind moveKind, int row, int column,
            int button, Key key, bool isMenuClick = false, int menuOption = -1)
        {
            PlayerNumber = playerNumber;
            MoveKind = moveKind;
            Row = row;
            Column = column;
            Button = button;
            Key = key;
            IsMenuClick = isMenuClick;
            MenuOption = menuOption;
        }

        public int PlayerNumber { get; private set; }
        public HayDayMoveKind MoveKind { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }
        public int Button { get; private set; }
        public Key Key { get; private set; }
        public bool IsMenuClick { get; private set; }
        public int MenuOption { get; private set; }
    }

    public enum HayDayMoveKind
    {
        Click,
        Key
    }

    public sealed class HayDaySaveData
    {
        public int Coins { get; set; }
        public int BarnCapacity { get; set; }
        public int SiloCapacity { get; set; }
        public int ChickenCount { get; set; }
        public int CowCount { get; set; }
        public int PigCount { get; set; }
        public int SheepCount { get; set; }
        public int GoatCount { get; set; }
        public int MillLevel { get; set; }
        public int SelectedCropIndex { get; set; }
        public int SelectedBuildingIndex { get; set; }
        public int CameraRow { get; set; }
        public int CameraColumn { get; set; }
        public double Zoom { get; set; }
        public Dictionary<string, int> Inventory { get; set; }
        public List<FarmPlotSaveData> Plots { get; set; }
        public List<FarmBuildingSaveData> Buildings { get; set; }
        public List<MarketOfferSaveData> MarketOffers { get; set; }
        public MarketOrderSaveData Order { get; set; }
        public int EggProgressSeconds { get; set; }
        public int FeedProgressSeconds { get; set; }
        public int CowProgressSeconds { get; set; }
        public int CowFeedProgressSeconds { get; set; }
        public int SheepProgressSeconds { get; set; }
        public int SheepFeedProgressSeconds { get; set; }
        public int PigProgressSeconds { get; set; }
        public int PigFeedProgressSeconds { get; set; }
        public int GoatProgressSeconds { get; set; }
        public int GoatFeedProgressSeconds { get; set; }
        public int EggReady { get; set; }
        public int MilkReady { get; set; }
        public int PorkReady { get; set; }
        public int WoolReady { get; set; }
        public int GoatMilkReady { get; set; }
        public int MarketDay { get; set; } = int.MinValue;
        public DateTime LastSavedAt { get; set; }
    }

    public sealed class FarmPlotSaveData
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Crop { get; set; }
        public DateTime PlantedAtUtc { get; set; }
    }

    public sealed class FarmBuildingSaveData
    {
        public string Kind { get; set; }
        public string Name { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public sealed class MarketOfferSaveData
    {
        public string Item { get; set; }
        public int Price { get; set; }
    }

    public sealed class MarketOrderSaveData
    {
        public string Item { get; set; }
        public int Quantity { get; set; }
        public int Reward { get; set; }
    }
}
