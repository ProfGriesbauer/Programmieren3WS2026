using System;
using System.IO;
using System.Reflection;
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
        Assert.Equal(3, field.Inventory[HayDayField.Feed]);
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
    public void Corn_GrowthUsesThreeMinuteRule()
    {
        DateTime planted = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        FarmPlot plot = new FarmPlot(27, 47);

        plot.Plant(HayDayField.Corn, planted);

        Assert.False(plot.IsRipe(planted.AddMinutes(2).AddSeconds(59)));
        Assert.True(plot.IsRipe(planted.AddMinutes(3)));
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
}
