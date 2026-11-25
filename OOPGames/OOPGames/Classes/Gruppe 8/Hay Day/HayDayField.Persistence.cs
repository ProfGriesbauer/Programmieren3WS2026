using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OOPGames
{
    public sealed partial class HayDayField
    {
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

        public void Save()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath));
            LastSavedAt = DateTime.UtcNow;
            File.WriteAllText(SavePath, JsonSerializer.Serialize(ToSaveData(),
                new JsonSerializerOptions { WriteIndented = true }));
            _lastSavedAt = LastSavedAt;
            IsDirty = false;
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
            field.Inventory[Feed] = field.ChickenCount;
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
                _feedProgressSeconds = Math.Min(300, Math.Max(0, data.FeedProgressSeconds)),
                _cowProgressSeconds = Math.Max(0, data.CowProgressSeconds) % 300,
                _cowFeedProgressSeconds = Math.Min(300, Math.Max(0, data.CowFeedProgressSeconds)),
                _sheepProgressSeconds = Math.Max(0, data.SheepProgressSeconds) % 600,
                _sheepFeedProgressSeconds = Math.Min(300, Math.Max(0, data.SheepFeedProgressSeconds)),
                _pigProgressSeconds = Math.Max(0, data.PigProgressSeconds) % 360,
                _pigFeedProgressSeconds = Math.Min(300, Math.Max(0, data.PigFeedProgressSeconds)),
                _goatProgressSeconds = Math.Max(0, data.GoatProgressSeconds) % 300,
                _goatFeedProgressSeconds = Math.Min(300, Math.Max(0, data.GoatFeedProgressSeconds)),
                _eggReady = Math.Max(0, data.EggReady),
                _milkReady = Math.Max(0, data.MilkReady),
                _porkReady = Math.Max(0, data.PorkReady),
                _woolReady = Math.Max(0, data.WoolReady),
                _goatMilkReady = Math.Max(0, data.GoatMilkReady),
                _orderSequence = data.OrderSequence > 0 ? data.OrderSequence
                    : data.Order == null ? 0 : 1,
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

            if (data.SaveVersion < CurrentSaveVersion)
            {
                field.Inventory[Feed] = Math.Max(field.Inventory[Feed], field.ChickenCount);
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

        private HayDaySaveData ToSaveData()
        {
            return new HayDaySaveData
            {
                SaveVersion = CurrentSaveVersion,
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
                OrderSequence = _orderSequence,
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
    }
}
