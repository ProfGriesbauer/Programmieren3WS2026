#if GROUP5_TESTS
using System.Windows;
using OOPGames;

namespace Gruppe5Tests;

public sealed class SatisfactoryGameplayTests
{
    [Fact]
    public void StartingCameraUsesLargerTiles()
    {
        G5_Kartenansicht view = new G5_Kartenansicht();

        Assert.True(view.TryRasterZuBildschirm(0, 0, 10, 10, out Point first));
        Assert.True(view.TryRasterZuBildschirm(1, 0, 10, 10, out Point second));

        Assert.Equal(38.4, second.X - first.X, 3);
    }

    [Fact]
    public void BackpackHasLimitedCapacityAndWarehouseExtendsIt()
    {
        G5_FabrikSpielfeld field = CreateField();

        Assert.True(field.TryAddInventar(G5_Item.Holz, 50));
        Assert.False(field.TryAddInventar(G5_Item.Eisenerz, 1));
        Assert.Equal(50, field.InventarBelegt);

        G5_Storage warehouse = new G5_Storage(new G5_GridPosition(1, 1), capacity: 2);
        field.AddGebaeude(warehouse);

        Assert.False(field.TryAddInventar(G5_Item.Eisenerz, 3));
        Assert.Equal(0, field.Menge(G5_Item.Eisenerz));
        Assert.Equal(0, warehouse.CountOf(G5_Item.Eisenerz));

        Assert.True(field.TryAddInventar(G5_Item.Eisenerz, 2));
        Assert.Equal(2, warehouse.CountOf(G5_Item.Eisenerz));
        Assert.Equal(G5_FabrikSpielfeld.SpielerInventarKapazitaetMax, field.InventarBelegt);
    }

    [Fact]
    public void DroppedItemsCanBePickedUpAtTheirDropLocation()
    {
        G5_FabrikSpielfeld field = CreateField();
        G5_GridPosition playerPosition = field.SpielerPosition;
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Assert.True(field.TryAddInventar(G5_Item.Eisenplatte, 8));

        Assert.True(field.TryAblegen(G5_Item.Eisenplatte, 5, now));
        Assert.Equal(3, field.Menge(G5_Item.Eisenplatte));
        Assert.Equal(5, field.AnzahlAbgelegterGegenstaende(playerPosition, now));

        Assert.Equal(5, field.NimmAbgelegteGegenstaendeAuf(playerPosition, now.AddSeconds(119)));
        Assert.Equal(8, field.Menge(G5_Item.Eisenplatte));
        Assert.Equal(0, field.AnzahlAbgelegterGegenstaende(playerPosition, now.AddSeconds(119)));
    }

    [Fact]
    public void DroppedItemsExpireAfterTwoMinutes()
    {
        G5_FabrikSpielfeld field = CreateField();
        G5_GridPosition playerPosition = field.SpielerPosition;
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Assert.True(field.TryAddInventar(G5_Item.Kupferdraht, 3));
        Assert.True(field.TryAblegen(G5_Item.Kupferdraht, 3, now));

        Assert.Equal(0, field.NimmAbgelegteGegenstaendeAuf(
            playerPosition,
            now + G5_FabrikSpielfeld.AbwurfLebensdauer));
        Assert.Equal(0, field.AnzahlAbgelegterGegenstaende(
            playerPosition,
            now + G5_FabrikSpielfeld.AbwurfLebensdauer));
        Assert.Equal(0, field.Menge(G5_Item.Kupferdraht));
    }

    [Fact]
    public void DropQuantityCanBeEnteredWithMultipleDigits()
    {
        G5_FabrikSpielfeld field = CreateField();
        field.WaehleAbwurfGegenstand(G5_Item.Eisenerz);

        field.AendereMengeEingabe("2");
        field.AendereMengeEingabe("5");

        Assert.Equal("25", field.MengeEingabe);
        field.AendereMengeEingabe("\b");
        Assert.Equal("2", field.MengeEingabe);
    }

    [Fact]
    public void HandMiningRequiresTwoSecondsBetweenResources()
    {
        G5_FabrikSpielfeld field = CreateField();

        Assert.True(field.TryStartHandabbau());
        Assert.False(field.TryStartHandabbau());

        for (int tick = 0; tick < G5_FabrikSpielfeld.HandabbauCooldownTicks - 1; tick++)
        {
            field.TickHandabbauCooldown();
        }

        Assert.False(field.TryStartHandabbau());
        field.TickHandabbauCooldown();
        Assert.True(field.TryStartHandabbau());
    }

    [Fact]
    public void ManualCraftingProducesOnlyOneItemAfterTwoSecondsAndDoesNotRepeat()
    {
        G5_FabrikSpielfeld field = CreateField();
        G5_Recipe recipe = G5_StarterRecipes.CreateManualCraftingRecipes()
            .Single(candidate => candidate.Output == G5_Item.Eisenbarren);
        Assert.True(field.TryAddInventar(G5_Item.Holz, 48));
        Assert.True(field.TryAddInventar(G5_Item.Eisenerz, 2));

        Assert.True(field.TryStarteManuellesHandwerk(recipe));
        Assert.False(field.TryStarteManuellesHandwerk(recipe));
        Assert.False(field.TryAddInventar(G5_Item.Kupfererz, 1));
        Assert.Equal(1, field.Menge(G5_Item.Eisenerz));
        Assert.Equal(0, field.Menge(G5_Item.Eisenbarren));

        for (int tick = 0; tick < G5_FabrikSpielfeld.ManuellesHandwerkDauerTicks - 1; tick++)
        {
            Assert.Null(field.TickManuellesHandwerk());
        }

        Assert.Equal(0, field.Menge(G5_Item.Eisenbarren));
        Assert.Same(recipe, field.TickManuellesHandwerk());
        Assert.Equal(1, field.Menge(G5_Item.Eisenbarren));
        Assert.Equal(50, field.InventarBelegt);
        Assert.Null(field.TickManuellesHandwerk());
        Assert.Equal(1, field.Menge(G5_Item.Eisenbarren));
    }

    [Fact]
    public void ManualCraftingProducesRequestedAmountOneAfterAnother()
    {
        G5_FabrikSpielfeld field = CreateField();
        G5_Recipe recipe = G5_StarterRecipes.CreateManualCraftingRecipes()
            .Single(candidate => candidate.Output == G5_Item.Eisenbarren);
        int erzProStueck = recipe.Ingredients[G5_Item.Eisenerz];
        Assert.True(field.TryAddInventar(G5_Item.Eisenerz, erzProStueck * 3));

        Assert.False(field.TryStarteManuellesHandwerk(recipe, 4));
        Assert.True(field.TryStarteManuellesHandwerk(recipe, 3));
        Assert.Equal(3, field.OffeneHandwerkAnzahl);

        for (int tick = 0; tick < G5_FabrikSpielfeld.ManuellesHandwerkDauerTicks * 3; tick++)
        {
            field.TickManuellesHandwerk();
        }

        Assert.Equal(3 * recipe.OutputAmount, field.Menge(G5_Item.Eisenbarren));
        Assert.Equal(0, field.Menge(G5_Item.Eisenerz));
        Assert.Equal(0, field.OffeneHandwerkAnzahl);
        Assert.False(field.ManuellesHandwerkLaeuft);
    }

    [Fact]
    public void ManualCraftingRequiresIngredientsInBackpack()
    {
        G5_FabrikSpielfeld field = CreateField();
        G5_Recipe recipe = G5_StarterRecipes.CreateManualCraftingRecipes()
            .Single(candidate => candidate.Output == G5_Item.Beton);
        Assert.True(field.TryAddInventar(G5_Item.Kalkstein, 1));

        Assert.False(field.TryStarteManuellesHandwerk(recipe));

        Assert.Equal(1, field.Menge(G5_Item.Kalkstein));
        Assert.Equal(0, field.Menge(G5_Item.Beton));
    }

    [Fact]
    public void MinerOutputMustBeCollectedIntoTheInventory()
    {
        G5_FabrikSpielfeld field = CreateField();
        G5_Miner miner = new G5_Miner(
            new G5_GridPosition(1, 1),
            new G5_ResourceDeposit(G5_Item.Eisenerz, 5),
            outputCapacity: 5,
            cycleTicks: 1,
            amountPerCycle: 1,
            powerDemandPerTick: 0);
        field.AddGebaeude(miner);
        miner.Tick();

        Assert.Equal(0, field.Menge(G5_Item.Eisenerz));
        Assert.Equal(1, miner.CountOf(G5_Item.Eisenerz));
        Assert.True(field.TryCollectMinerOutput(miner, 1, out int collected));

        Assert.Equal(1, collected);
        Assert.Equal(1, field.Menge(G5_Item.Eisenerz));
        Assert.Equal(0, miner.CountOf(G5_Item.Eisenerz));
    }

    [Fact]
    public void MinerOutputPickupUsesTheRequestedQuantity()
    {
        G5_FabrikSpielfeld field = CreateField();
        G5_Miner miner = new G5_Miner(
            new G5_GridPosition(1, 1),
            new G5_ResourceDeposit(G5_Item.Eisenerz, 5),
            outputCapacity: 5,
            cycleTicks: 1,
            amountPerCycle: 1,
            powerDemandPerTick: 0);
        field.AddGebaeude(miner);
        miner.Tick();
        miner.Tick();
        miner.Tick();

        Assert.True(field.TryCollectMinerOutput(miner, 2, out int collected));

        Assert.Equal(2, collected);
        Assert.Equal(2, field.Menge(G5_Item.Eisenerz));
        Assert.Equal(1, miner.CountOf(G5_Item.Eisenerz));
    }

    [Fact]
    public void ProductionBuildingOutputCanBeCollectedManually()
    {
        G5_FabrikSpielfeld field = CreateField();
        G5_Recipe recipe = G5_StarterRecipes.CreateSmeltingRecipes(1, 0)
            .Single(candidate => candidate.Output == G5_Item.Eisenbarren);
        G5_Smelter smelter = new G5_Smelter(
            new G5_GridPosition(1, 1),
            new[] { recipe },
            inputCapacity: 5,
            outputCapacity: 5);
        Assert.True(smelter.SelectRecipe(recipe.Name));
        Assert.True(smelter.TryReceive(G5_Item.Eisenerz, 1));
        field.AddGebaeude(smelter);
        smelter.Tick();

        Assert.Equal(1, smelter.OutputContents[G5_Item.Eisenbarren]);
        Assert.True(field.TryCollectBuildingOutput(
            smelter,
            G5_Item.Eisenbarren,
            1,
            out int collected));

        Assert.Equal(1, collected);
        Assert.Equal(1, field.Menge(G5_Item.Eisenbarren));
        Assert.Equal(0, smelter.OutputContents.GetValueOrDefault(G5_Item.Eisenbarren));
    }

    [Fact]
    public void ConveyorLinePathConnectsEndpointsWithAnOrthogonalRoute()
    {
        IReadOnlyList<G5_GridPosition> route = SatisfactoryGameRules.CreateConveyorLinePath(
            new G5_GridPosition(2, 3),
            new G5_GridPosition(4, 5));

        Assert.Equal(
            new[]
            {
                new G5_GridPosition(2, 3),
                new G5_GridPosition(3, 3),
                new G5_GridPosition(4, 3),
                new G5_GridPosition(4, 4),
                new G5_GridPosition(4, 5)
            },
            route);
    }

    [Fact]
    public void BuildingCostsIncludeProcessedMaterialsAndConcrete()
    {
        IReadOnlyDictionary<G5_Item, int> minerCost =
            G5_FabrikKosten.Baupreis(G5_Bauwerkzeug.Foerderer);
        IReadOnlyDictionary<G5_Item, int> smelterCost =
            G5_FabrikKosten.Baupreis(G5_Bauwerkzeug.Schmelze);

        Assert.True(minerCost.ContainsKey(G5_Item.Eisenplatte));
        Assert.True(smelterCost.ContainsKey(G5_Item.Eisenbarren));
        Assert.True(smelterCost.ContainsKey(G5_Item.Beton));
    }

    private static G5_FabrikSpielfeld CreateField()
    {
        return new G5_FabrikSpielfeld(new G5_WeltSpielfeld(10, 10), 5, 5);
    }
}
#endif
