using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using OOPGames;

namespace Gruppe8HayDayTests;

public sealed class HayDayFieldTests : IDisposable
{
    private readonly string _savePath;

    public HayDayFieldTests()
    {
        _savePath = Path.Combine(Path.GetTempPath(), "OOPGames-HayDayTests",
            Guid.NewGuid().ToString("N") + ".json");
        Directory.CreateDirectory(Path.GetDirectoryName(_savePath)!);
        HayDayField.SetSavePathForTesting(_savePath);
    }

    public void Dispose()
    {
        HayDayField.ClearSavePathForTesting();
        if (Directory.Exists(Path.GetDirectoryName(_savePath)!))
        {
            Directory.Delete(Path.GetDirectoryName(_savePath)!, true);
        }
    }

    [Fact]
    public void NewField_HasPlanStartState()
    {
        HayDayField field = HayDayField.CreateNewField();

        Assert.Equal(500, field.Coins);
        Assert.Equal(100, field.BarnCapacity);
        Assert.Equal(100, field.SiloCapacity);
        Assert.Equal(5, field.ChickenCount);
        Assert.Equal(10, field.Inventory[HayDayField.Wheat]);
        Assert.Equal(10, field.Inventory[HayDayField.Corn]);
        Assert.Equal(5, field.Inventory[HayDayField.Feed]);
        Assert.Equal(6, field.Plots.Count);
        Assert.Equal(0, field.Inventory[HayDayField.Milk]);
        Assert.Equal(0, field.Inventory[HayDayField.Wool]);
        Assert.Equal(0, field.Inventory[HayDayField.Cheese]);
    }

    [Fact]
    public void Crop_GrowthTakesTwoMinutes()
    {
        DateTime planted = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        FarmPlot plot = new FarmPlot(27, 47);

        plot.Plant(HayDayField.Wheat, planted);

        Assert.False(plot.IsRipe(planted.AddMinutes(1).AddSeconds(59)));
        Assert.True(plot.IsRipe(planted.AddMinutes(2)));
        Assert.True(plot.MarkRipeIfNeeded(planted.AddMinutes(2)));
        Assert.False(plot.MarkRipeIfNeeded(planted.AddMinutes(3)));
    }

    [Fact]
    public void Corn_GrowthTakesTwoMinutes()
    {
        DateTime planted = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        FarmPlot plot = new FarmPlot(27, 47);

        plot.Plant(HayDayField.Corn, planted);

        Assert.False(plot.IsRipe(planted.AddMinutes(1).AddSeconds(59)));
        Assert.True(plot.IsRipe(planted.AddMinutes(2)));
    }

    [Fact]
    public void InvalidMove_DoesNotChangeField()
    {
        HayDayField field = HayDayField.CreateNewField();
        int coins = field.Coins;

        field.Apply(null);
        field.Apply(new HayDayMove(3, HayDayMoveKind.Key, 0, 0, 0,
            System.Windows.Input.Key.Left));

        Assert.Equal(coins, field.Coins);
        Assert.Equal(26, field.CameraRow);
        Assert.Equal(50, field.CameraColumn);
    }

    [Fact]
    public void SaveLoad_PreservesPigAndGoatState()
    {
        HayDayField field = HayDayField.CreateNewField();
        typeof(HayDayField).GetField("_pigProgressSeconds",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, 123);
        typeof(HayDayField).GetProperty("PigCount",
            BindingFlags.Instance | BindingFlags.Public)!.GetValue(field);

        field.Save();
        HayDayField loaded = HayDayField.LoadOrCreate();

        Assert.Equal(field.Coins, loaded.Coins);
        Assert.Equal(field.Inventory[HayDayField.Wheat],
            loaded.Inventory[HayDayField.Wheat]);
    }

    [Fact]
    public void FeedAnimal_UsesOneFeedUnitPerChicken()
    {
        HayDayField field = HayDayField.CreateNewField();
        int feedBefore = field.Inventory[HayDayField.Feed];

        bool fed = field.FeedAnimal("Coop");

        Assert.True(fed);
        Assert.Equal(feedBefore - field.ChickenCount, field.Inventory[HayDayField.Feed]);
    }

    [Fact]
    public void FeedAnimal_RequiresFeedForEveryChicken()
    {
        HayDayField field = HayDayField.CreateNewField();
        field.Inventory[HayDayField.Feed] = 4;

        bool fed = field.FeedAnimal("Coop");

        Assert.False(fed);
        Assert.Equal(4, field.Inventory[HayDayField.Feed]);
    }

    [Fact]
    public void FeedAnimal_ProvidesFiveMinutesWithoutRepeatedFeed()
    {
        HayDayField field = HayDayField.CreateNewField();

        Assert.True(field.FeedAnimal("Coop"));
        Assert.False(field.FeedAnimal("Coop"));
        Assert.StartsWith("Gefüttert ", field.GetAnimalProductionStatus("Coop"));
    }

    [Fact]
    public void ChickenCycle_ProducesCollectibleEggsAndCanBeFedAgain()
    {
        HayDayField field = HayDayField.CreateNewField();
        DateTime start = DateTime.UtcNow;
        typeof(HayDayField).GetField("_lastAdvancedAt",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, start);

        Assert.True(field.FeedAnimal("Coop"));
        field.AdvanceTime(start.AddMinutes(1));
        Assert.Equal(0, field.EggReady);
        Assert.StartsWith("Gefüttert ", field.GetAnimalProductionStatus("Coop"));

        field.AdvanceTime(start.AddMinutes(5));
        Assert.Equal(field.ChickenCount, field.EggReady);
        Assert.Contains("Eier bereit | Einsammeln",
            field.GetAnimalProductionStatus("Coop"));
        field.Inventory[HayDayField.Feed] = field.ChickenCount;
        Assert.False(field.FeedAnimal("Coop"));
        Assert.Contains("einsammeln", field.StatusMessage);
        Assert.Equal(field.ChickenCount, field.Inventory[HayDayField.Feed]);

        Assert.True(field.CollectAnimalProduct(HayDayField.Eggs, "Eier"));
        Assert.Equal(field.ChickenCount, field.Inventory[HayDayField.Eggs]);
        Assert.Equal("Hungrig", field.GetAnimalProductionStatus("Coop"));

        Assert.True(field.FeedAnimal("Coop"));
        Assert.StartsWith("Gefüttert ", field.GetAnimalProductionStatus("Coop"));
    }

    [Fact]
    public void CoopMenu_CollectsEggsAfterProductionTimer()
    {
        HayDayField field = HayDayField.CreateNewField();
        DateTime start = DateTime.UtcNow;
        typeof(HayDayField).GetField("_lastAdvancedAt",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, start);
        FarmBuilding coop = Assert.Single(field.Buildings,
            building => building.Kind == "Coop");

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            coop.Row, coop.Column, 0, System.Windows.Input.Key.None));
        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            0, 0, 0, System.Windows.Input.Key.None, true, 0));
        field.AdvanceTime(start.AddMinutes(5));

        Assert.Equal(field.ChickenCount, field.EggReady);
        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            0, 0, 0, System.Windows.Input.Key.None, true, 1));

        Assert.Equal(field.ChickenCount, field.Inventory[HayDayField.Eggs]);
        Assert.Equal(0, field.EggReady);
    }

    [Fact]
    public void CoopMenuClick_CollectsReadyEggs()
    {
        HayDayField field = HayDayField.CreateNewField();
        FarmBuilding coop = Assert.Single(field.Buildings,
            building => building.Kind == "Coop");
        typeof(HayDayField).GetField("_eggReady",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, field.ChickenCount);

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            coop.Row, coop.Column, 0, System.Windows.Input.Key.None));
        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            0, 0, 0, System.Windows.Input.Key.None, true, 1));

        Assert.Equal(field.ChickenCount, field.Inventory[HayDayField.Eggs]);
        Assert.Equal(0, field.EggReady);
    }

    [Fact]
    public void SheepCycle_ProducesWoolOnlyAfterFiveMinuteTimerAndMustBeCollected()
    {
        HayDayField field = HayDayField.CreateNewField();
        typeof(HayDayField).GetProperty("SheepCount")!.SetValue(field, 2);
        field.Inventory[HayDayField.SheepFeed] = 2;
        DateTime start = DateTime.UtcNow;
        typeof(HayDayField).GetField("_lastAdvancedAt",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, start);

        Assert.True(field.FeedAnimal("SheepBarn"));
        field.AdvanceTime(start.AddMinutes(4).AddSeconds(59));
        Assert.Equal(0, field.WoolReady);
        field.AdvanceTime(start.AddMinutes(5));

        Assert.Equal(2, field.WoolReady);
        Assert.StartsWith("2 Wolle bereit", field.GetAnimalProductionStatus("SheepBarn"));
        Assert.False(field.FeedAnimal("SheepBarn"));
        Assert.True(field.CollectAnimalProduct(HayDayField.Wool, "Wolle"));
        Assert.Equal("Hungrig", field.GetAnimalProductionStatus("SheepBarn"));

        field.Inventory[HayDayField.SheepFeed] = 2;
        Assert.True(field.FeedAnimal("SheepBarn"));
    }

    [Fact]
    public void CollectAnimalProduct_CollectsWhatFitsAndKeepsTheRestReady()
    {
        HayDayField field = HayDayField.CreateNewField();
        field.Inventory[HayDayField.Eggs] = 98;
        typeof(HayDayField).GetField("_eggReady",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, 25);

        Assert.True(field.CollectAnimalProduct(HayDayField.Eggs, "Eier"));
        Assert.Equal(100, field.Inventory[HayDayField.Eggs]);
        Assert.Equal(23, field.EggReady);

        field.Inventory[HayDayField.Eggs] = 0;
        Assert.True(field.CollectAnimalProduct(HayDayField.Eggs, "Eier"));
        Assert.Equal(23, field.Inventory[HayDayField.Eggs]);
        Assert.Equal(0, field.EggReady);
    }

    [Fact]
    public void SheepCannotBeFedAgainUntilWoolIsCollected()
    {
        HayDayField field = HayDayField.CreateNewField();
        typeof(HayDayField).GetProperty("SheepCount")!.SetValue(field, 2);
        field.Inventory[HayDayField.SheepFeed] = 2;
        typeof(HayDayField).GetField("_woolReady",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, 2);

        Assert.False(field.FeedAnimal("SheepBarn"));
        Assert.Equal(2, field.Inventory[HayDayField.SheepFeed]);

        Assert.True(field.CollectAnimalProduct(HayDayField.Wool, "Wolle"));
        Assert.True(field.FeedAnimal("SheepBarn"));
    }

    [Fact]
    public void FeedTimer_SaveLoadPreservesFullFiveMinuteDuration()
    {
        HayDayField field = HayDayField.CreateNewField();
        Assert.True(field.FeedAnimal("Coop"));

        field.Save();
        HayDayField loaded = HayDayField.LoadOrCreate();

        Assert.StartsWith("Gefüttert ", loaded.GetAnimalProductionStatus("Coop"));
    }

    [Fact]
    public void CropTimer_ContinuesAcrossGameTicks()
    {
        HayDayField field = HayDayField.CreateNewField();
        DateTime start = DateTime.UtcNow;
        typeof(HayDayField).GetField("_lastAdvancedAt",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, start);
        FarmPlot plot = field.Plots[0];
        plot.Plant(HayDayField.Wheat, start);
        field.Save();
        typeof(HayDayField).GetField("_lastAdvancedAt",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, start);
        typeof(HayDayField).GetField("_lastSavedAt",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, start.AddMinutes(10));

        field.AdvanceTime(start.AddMinutes(1));
        Assert.False(plot.IsRipe(start.AddMinutes(1)));
        Assert.True(field.IsDirty);
        field.Save();
        typeof(HayDayField).GetField("_lastSavedAt",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, start.AddMinutes(10));
        field.AdvanceTime(start.AddMinutes(2));

        Assert.True(plot.IsRipe(start.AddMinutes(2)));
        Assert.True(plot.Ripe);
        Assert.True(field.IsDirty);
    }

    [Fact]
    public void CoopMenu_FeedOptionFeedsAllChickens()
    {
        HayDayField field = HayDayField.CreateNewField();
        int feedBefore = field.Inventory[HayDayField.Feed];
        FarmBuilding coop = Assert.Single(field.Buildings,
            building => building.Kind == "Coop");

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            coop.Row, coop.Column, 0, System.Windows.Input.Key.None));
        Assert.True(field.HasOpenMenu);

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            0, 0, 0, System.Windows.Input.Key.None, true, 0));

        Assert.Equal(feedBefore - field.ChickenCount,
            field.Inventory[HayDayField.Feed]);
        Assert.StartsWith("Gefüttert ", field.GetAnimalProductionStatus("Coop"));
    }

    [Fact]
    public void CollectAnimalProduct_MovesReadyProductIntoBarn()
    {
        HayDayField field = HayDayField.CreateNewField();
        typeof(HayDayField).GetField("_eggReady",
            BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(field, 4);

        bool collected = field.CollectAnimalProduct(HayDayField.Eggs, "Eier");

        Assert.True(collected);
        Assert.Equal(4, field.Inventory[HayDayField.Eggs]);
        Assert.Equal(0, field.EggReady);
    }

    [Fact]
    public void FulfillingDeliveryImmediatelyCreatesNextOrder()
    {
        HayDayField field = HayDayField.CreateNewField();
        MarketOrder currentOrder = field.Order!;
        int coinsBefore = field.Coins;
        field.Inventory[currentOrder.Item] = currentOrder.Quantity;

        typeof(HayDayField).GetMethod("FulfillOrder",
            BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(field, null);

        Assert.Equal(coinsBefore + currentOrder.Reward, field.Coins);
        Assert.NotNull(field.Order);
        Assert.NotEqual(currentOrder.Item, field.Order!.Item);
    }

    [Fact]
    public void VanMenu_CanSkipOrdersWithoutCostAndGeneratesVariedRequests()
    {
        HayDayField field = HayDayField.CreateNewField();
        FarmBuilding van = Assert.Single(field.Buildings,
            building => building.Kind == "Van");
        int coinsBefore = field.Coins;
        int wheatBefore = field.Inventory[HayDayField.Wheat];
        int eggsBefore = field.Inventory[HayDayField.Eggs];
        var skippedOrders = new List<MarketOrder>();

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            van.Row, van.Column, 0, System.Windows.Input.Key.None));
        Assert.Contains("Bestellung überspringen", field.GetOpenMenuOptions());

        for (int i = 0; i < 8; i++)
        {
            MarketOrder previousOrder = field.Order!;
            field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
                0, 0, 0, System.Windows.Input.Key.None, true, 1));

            Assert.NotEqual(previousOrder.Item, field.Order!.Item);
            Assert.Equal(coinsBefore, field.Coins);
            Assert.Equal(wheatBefore, field.Inventory[HayDayField.Wheat]);
            Assert.Equal(eggsBefore, field.Inventory[HayDayField.Eggs]);
            skippedOrders.Add(field.Order);
        }

        Assert.Equal(8, skippedOrders.Select(order => order.Item).Distinct().Count());
        Assert.True(skippedOrders.Select(order => order.Quantity).Distinct().Count() > 1);
        Assert.True(skippedOrders.Select(order => order.Reward).Distinct().Count() > 1);
        Assert.Contains("übersprungen", field.StatusMessage);
    }

    [Fact]
    public void MarketMenu_CanSkipAnOrder()
    {
        HayDayField field = HayDayField.CreateNewField();
        FarmBuilding market = Assert.Single(field.Buildings,
            building => building.Kind == "Market");
        MarketOrder previousOrder = field.Order!;

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            market.Row, market.Column, 0, System.Windows.Input.Key.None));
        string[] options = field.GetOpenMenuOptions();
        Assert.Equal("Bestellung überspringen", options[^1]);

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            0, 0, 0, System.Windows.Input.Key.None, true, options.Length - 1));

        Assert.NotEqual(previousOrder.Item, field.Order!.Item);
    }

    [Fact]
    public void SiloMenu_ShowsAmountOfEveryCropAndStorageUsage()
    {
        HayDayField field = HayDayField.CreateNewField();
        field.Inventory[HayDayField.Wheat] = 11;
        field.Inventory[HayDayField.Corn] = 12;
        field.Inventory[HayDayField.Carrots] = 13;
        field.Inventory[HayDayField.Soybeans] = 14;
        FarmBuilding silo = Assert.Single(field.Buildings,
            building => building.Kind == "Silo");

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            silo.Row, silo.Column, 0, System.Windows.Input.Key.None));

        string[] info = field.GetOpenMenuInfo();
        Assert.Contains(info, line => line.Contains("Weizen: 11")
            && line.Contains("Mais: 12"));
        Assert.Contains(info, line => line.Contains("Karotten: 13")
            && line.Contains("Soja: 14"));
        Assert.Contains(info, line => line.Contains("Lager: 50 / 100"));
    }

    [Fact]
    public void BarnMenu_ShowsAllAnimalProductsFeedsAndStorageUsage()
    {
        HayDayField field = HayDayField.CreateNewField();
        field.Inventory[HayDayField.Eggs] = 4;
        field.Inventory[HayDayField.Milk] = 3;
        field.Inventory[HayDayField.Wool] = 2;
        field.Inventory[HayDayField.Pork] = 1;
        field.Inventory[HayDayField.GoatMilk] = 5;
        field.Inventory[HayDayField.Cheese] = 6;
        field.Inventory[HayDayField.CowFeed] = 7;
        field.Inventory[HayDayField.SheepFeed] = 8;
        field.Inventory[HayDayField.PigFeed] = 9;
        field.Inventory[HayDayField.GoatFeed] = 10;
        FarmBuilding barn = Assert.Single(field.Buildings,
            building => building.Kind == "Barn");

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            barn.Row, barn.Column, 0, System.Windows.Input.Key.None));

        string[] info = field.GetOpenMenuInfo();
        Assert.Contains(info, line => line.Contains("Eier: 4")
            && line.Contains("Milch: 3"));
        Assert.Contains(info, line => line.Contains("Wolle: 2")
            && line.Contains("Fleisch: 1")
            && line.Contains("Ziegenmilch: 5"));
        Assert.Contains(info, line => line.Contains("Käse: 6"));
        Assert.Contains(info, line => line.Contains("Hühner: 5")
            && line.Contains("Kühe: 7"));
        Assert.Contains(info, line => line.Contains("Schafe: 8")
            && line.Contains("Schweine: 9") && line.Contains("Ziegen: 10"));
        Assert.Contains(info, line => line.Contains("Lager: 60 / 100"));
    }

    [Fact]
    public void MarketMenu_ShowsStockAndSellingOneItemUpdatesInventoryAndCoins()
    {
        HayDayField field = HayDayField.CreateNewField();
        field.Inventory[HayDayField.Wheat] = 6;
        field.Inventory[HayDayField.Eggs] = 3;
        int coinsBefore = field.Coins;
        int wheatPrice = field.MarketPrice(HayDayField.Wheat);
        FarmBuilding market = Assert.Single(field.Buildings,
            building => building.Kind == "Market");

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            market.Row, market.Column, 0, System.Windows.Input.Key.None));

        string[] options = field.GetOpenMenuOptions();
        Assert.Contains("6 da", options[0]);
        Assert.Contains("3 da", options[4]);

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            0, 0, 0, System.Windows.Input.Key.None, true, 0));

        Assert.Equal(5, field.Inventory[HayDayField.Wheat]);
        Assert.Equal(3, field.Inventory[HayDayField.Eggs]);
        Assert.Equal(coinsBefore + wheatPrice, field.Coins);
        Assert.Contains("5 da", field.GetOpenMenuOptions()[0]);
    }

    [Fact]
    public void ShopSelection_AllowsBuildingPlacementWithoutChangingCoinsOnOccupiedTile()
    {
        HayDayField field = HayDayField.CreateNewField();
        int coins = field.Coins;

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click, 0, 0, 0,
            System.Windows.Input.Key.None, true, HayDayField.ShopMenuOption));
        field.Apply(new HayDayMove(1, HayDayMoveKind.Click, 0, 0, 0,
            System.Windows.Input.Key.None, true, 0));
        field.Apply(new HayDayMove(1, HayDayMoveKind.Click, 25, 50, 1,
            System.Windows.Input.Key.None));

        Assert.Equal(coins, field.Coins);
        Assert.Contains(field.Buildings,
            building => building.Row == 25 && building.Column == 50);
    }

    [Fact]
    public void RightClickBuildingThenLeftClickFreeTile_MovesBuilding()
    {
        HayDayField field = HayDayField.CreateNewField();
        FarmBuilding coop = Assert.Single(field.Buildings,
            building => building.Kind == "Coop");

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            coop.Row, coop.Column, 1, System.Windows.Input.Key.None));

        Assert.Equal("Hühnerstall", field.MovingItemName);

        field.Apply(new HayDayMove(2, HayDayMoveKind.Click,
            35, 60, 0, System.Windows.Input.Key.None));

        Assert.Equal(35, coop.Row);
        Assert.Equal(60, coop.Column);
        Assert.Null(field.MovingItemName);
    }

    [Fact]
    public void MovingBuildingToOccupiedTile_StaysSelectedUntilEscape()
    {
        HayDayField field = HayDayField.CreateNewField();
        FarmBuilding coop = Assert.Single(field.Buildings,
            building => building.Kind == "Coop");
        FarmPlot occupiedPlot = field.Plots[0];
        int originalRow = coop.Row;
        int originalColumn = coop.Column;

        field.Apply(new HayDayMove(1, HayDayMoveKind.Click,
            coop.Row, coop.Column, 1, System.Windows.Input.Key.None));
        field.Apply(new HayDayMove(2, HayDayMoveKind.Click,
            occupiedPlot.Row, occupiedPlot.Column, 0, System.Windows.Input.Key.None));

        Assert.Equal(originalRow, coop.Row);
        Assert.Equal(originalColumn, coop.Column);
        Assert.Equal("Hühnerstall", field.MovingItemName);

        field.Apply(new HayDayMove(2, HayDayMoveKind.Key, 0, 0, 0,
            System.Windows.Input.Key.Escape));

        Assert.Null(field.MovingItemName);
        Assert.Equal(originalRow, coop.Row);
        Assert.Equal(originalColumn, coop.Column);
    }

    [Fact]
    public void BuildingHitTest_RecognizesVisibleRoof()
    {
        FarmBuilding? expected = null;
        FarmBuilding? hit = null;
        Exception? failure = null;

        Thread uiThread = new Thread(() =>
        {
            try
            {
                HayDayField field = HayDayField.CreateNewField();
                expected = Assert.Single(field.Buildings,
                    building => building.Kind == "Farmhouse");

                Canvas canvas = new Canvas { Width = 800, Height = 600 };
                canvas.Measure(new Size(800, 600));
                canvas.Arrange(new Rect(0, 0, 800, 600));

                HayDayPainter painter = new HayDayPainter();
                painter.PaintGameField(canvas, field);

                double tileWidth = field.TileWidth(canvas.ActualWidth, canvas.ActualHeight);
                double tileHeight = tileWidth * 0.5;
                double columnOffset = expected.Column - field.CameraColumn;
                double rowOffset = expected.Row - field.CameraRow;
                double groundX = canvas.ActualWidth / 2
                    + (columnOffset - rowOffset) * tileWidth / 2;
                double groundY = canvas.ActualHeight / 2
                    + (columnOffset + rowOffset) * tileHeight / 2;
                double roofY = groundY + tileHeight * 0.28
                    - Math.Max(9, tileHeight * 0.9) - Math.Max(6, tileHeight * 0.58);

                hit = painter.BuildingAt((int)groundX, (int)roofY, field);

                HayDayHumanPlayer player = new HayDayHumanPlayer();
                player.SetPlayerNumber(1);
                OOPGamesManager.Singleton.RegisterActivePainter(painter);
                HayDayMove? move = player.GetMove(
                    new ClickSelection((int)groundX, (int)roofY, 1), field) as HayDayMove;
                Assert.NotNull(move);
                Assert.Equal(expected.Row, move.Row);
                Assert.Equal(expected.Column, move.Column);
                Assert.Equal(1, move.Button);
            }
            catch (Exception exception)
            {
                failure = exception;
            }
            finally
            {
                OOPGamesManager.Singleton.RegisterActivePainter(null!);
            }
        });
        uiThread.SetApartmentState(ApartmentState.STA);
        uiThread.Start();
        uiThread.Join();

        if (failure != null)
        {
            throw failure;
        }

        Assert.Same(expected, hit);
    }
}
