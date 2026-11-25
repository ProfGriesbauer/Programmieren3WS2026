using System;
using System.Linq;

namespace OOPGames
{
    // Enthält Preise, Handel, Ernten und das gemeinsame Platzieren von Hofobjekten.
    public sealed partial class HayDayField
    {
        // Marktangebote liefern den Tagespreis; für unbekannte Produkte gilt ein Grundpreis.
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
            if (!IsAvailableLocation(row, column))
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
            // Ein Acker wird als FarmPlot gespeichert; alle anderen Shop-Einträge sind Gebäude.
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

        // Neue und verschobene Objekte teilen dieselben Grenzen und Belegungsregeln.
        private bool IsAvailableLocation(int row, int column,
            FarmPlot movingPlot = null, FarmBuilding movingBuilding = null)
        {
            return IsInside(row, column)
                && !IsRoad(row, column)
                && !Plots.Any(plot => plot != movingPlot
                    && plot.Row == row && plot.Column == column)
                && !Buildings.Any(building => building != movingBuilding
                    && building.Row == row && building.Column == column);
        }

        private void Sell(string item, int amount)
        {
            if (amount <= 0 || !MarketItems.Contains(item))
            {
                SetStatus("Dieser Artikel kann nicht verkauft werden.");
                return;
            }

            int available = Inventory.ContainsKey(item) ? Inventory[item] : 0;
            int quantity = Math.Min(amount, available);
            if (quantity <= 0)
            {
                SetStatus("Keine " + item + " zum Verkaufen.");
                return;
            }

            if (!RemoveInventory(item, quantity))
            {
                SetStatus("Verkauf von " + item + " fehlgeschlagen. Bestand bitte prüfen.");
                return;
            }

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
            int reward = Order.Reward;
            CreateNextOrder(DateTime.UtcNow);
            SetStatus("Bestellung geliefert: +" + reward + " Münzen. Neue Anfrage: " +
                Order.Description);
        }

        private void SkipOrder()
        {
            if (Order == null)
            {
                SetStatus("Aktuell keine Bestellanfrage zum Überspringen.");
                return;
            }

            CreateNextOrder(DateTime.UtcNow);
            SetStatus("Bestellung übersprungen. Neue Anfrage: " + Order.Description);
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

                CreateNextOrder(now);
                _marketDay = day;
                IsDirty = true;
            }
            else if (Order == null)
            {
                CreateNextOrder(now);
                IsDirty = true;
            }
        }

        private void CreateNextOrder(DateTime now)
        {
            Order = CreateOrder(now, _orderSequence, Order == null ? null : Order.Item);
            _orderSequence++;
        }

        private static MarketOrder CreateOrder(DateTime now, int sequence, string previousItem)
        {
            int day = (int)(now.Date - new DateTime(2020, 1, 1)).TotalDays;
            int[] basePrices = { 3, 4, 2, 3, 6, 8, 7, 12, 10, 9 };
            int index = PositiveModulo(day * 3 + sequence * 7, MarketItems.Length);
            if (MarketItems[index] == previousItem)
            {
                index = (index + 1) % MarketItems.Length;
            }

            int quantity = 2 + PositiveModulo(day + sequence * 3, 4);
            int price = Math.Max(1, basePrices[index] +
                ((day * 7 + index * 11 + sequence * 13) % 5) - 2);
            int bonus = 8 + PositiveModulo(day * 3 + sequence * 5, 13);
            return new MarketOrder(MarketItems[index], quantity, quantity * price + bonus);
        }

        private static int PositiveModulo(int value, int modulus)
        {
            int result = value % modulus;
            return result < 0 ? result + modulus : result;
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
}
