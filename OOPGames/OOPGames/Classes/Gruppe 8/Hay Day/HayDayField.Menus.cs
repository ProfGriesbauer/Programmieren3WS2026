using System;
using System.Linq;

namespace OOPGames
{
    public sealed partial class HayDayField
    {
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
                    int barnInventoryCount = Inventory
                        .Where(item => !CropKinds.Contains(item.Key))
                        .Sum(item => item.Value);
                    return new[]
                    {
                        "Tierprodukte: Eier: " + Inventory[Eggs] + " | Milch: " + Inventory[Milk],
                        "Wolle: " + Inventory[Wool] + " | Fleisch: " + Inventory[Pork]
                            + " | Ziegenmilch: " + Inventory[GoatMilk],
                        "Käse: " + Inventory[Cheese],
                        "Futter: Hühner: " + Inventory[Feed] + " | Kühe: " + Inventory[CowFeed],
                        "Schafe: " + Inventory[SheepFeed] + " | Schweine: " + Inventory[PigFeed]
                            + " | Ziegen: " + Inventory[GoatFeed],
                        "Lager: " + barnInventoryCount + " / " + BarnCapacity
                    };
                case "Silo":
                    int siloInventoryCount = CropKinds.Sum(crop => Inventory[crop]);
                    return new[]
                    {
                        "Weizen: " + Inventory[Wheat] + " | Mais: " + Inventory[Corn],
                        "Karotten: " + Inventory[Carrots] + " | Soja: " + Inventory[Soybeans],
                        "Lager: " + siloInventoryCount + " / " + SiloCapacity
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
                        "Feldfrüchte: Weizen " + Inventory[Wheat] + " | Mais " + Inventory[Corn],
                        "Karotten " + Inventory[Carrots] + " | Soja " + Inventory[Soybeans],
                        "Tierprodukte: Eier " + Inventory[Eggs] + " | Milch " + Inventory[Milk],
                        "Wolle " + Inventory[Wool] + " | Käse " + Inventory[Cheese]
                            + " | Fleisch " + Inventory[Pork],
                        "Ziegenmilch " + Inventory[GoatMilk],
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
                    return new[] { "Mit Hühnerfutter füttern",
                        "Eier einsammeln (" + EggReady + " bereit)", "Eier verkaufen" };
                case "CowBarn":
                    return new[] { "Mit Kuhfutter füttern", "Milch einsammeln", "Milch verkaufen" };
                case "PigBarn":
                    return new[] { "Mit Schweinefutter füttern", "Fleisch einsammeln", "Fleisch verkaufen" };
                case "SheepBarn":
                    return new[] { "Mit Schaffutter füttern", "Wolle einsammeln", "Wolle verkaufen" };
                case "GoatBarn":
                    return new[] { "Mit Ziegenfutter füttern", "Ziegenmilch einsammeln", "Ziegenmilch verkaufen" };
                case "Market":
                    return MarketItems.Select(item => "1 " + item + " verkaufen ("
                        + Inventory[item] + " da, " + MarketPrice(item) + " M/St.)")
                        .Concat(new[]
                    {
                        "5 Weizen kaufen (" + PurchasePrice(Wheat, 5) + " Münzen)",
                        "5 Mais kaufen (" + PurchasePrice(Corn, 5) + " Münzen)",
                        "5 Karottensamen kaufen (" + SeedPurchasePrice(Carrots, 5) + " Münzen)",
                        "5 Sojasamen kaufen (" + SeedPurchasePrice(Soybeans, 5) + " Münzen)",
                        "Bestellung abgeben",
                        "Bestellung überspringen"
                    }).ToArray();
                case "Van":
                    return new[] { "Bestellung abgeben", "Bestellung überspringen" };
                case "Dairy":
                    return new[] { "Käse herstellen (2 Milch)" };
                default:
                    return new[] { "Schließen" };
            }
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
                            case 5: SkipOrder(); break;
                        }
                    }
                    break;
                case "Van":
                    if (option == 0) FulfillOrder();
                    else SkipOrder();
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
    }
}
