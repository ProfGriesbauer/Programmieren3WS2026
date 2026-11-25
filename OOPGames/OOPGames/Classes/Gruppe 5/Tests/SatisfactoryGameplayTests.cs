#if GROUP5_TESTS
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OOPGames;

namespace Gruppe5Tests;

public sealed class SatisfactoryGameplayTests
{
    [Fact]
    public void QueuedBuildKeepsItsDropPositionUntilConsumed()
    {
        RunOnSta(() =>
        {
            G5_FabrikSpielfeld field = CreateField();
            SatisfactoryFieldView view = new SatisfactoryFieldView(
                field, new SatisfactoryStartScreen(), new SatisfactoryGameMentor(), true);
            G5_GridPosition droppedAt = new G5_GridPosition(4, 5);

            view.QueuePendingBuild(G5_Bauwerkzeug.Lager, droppedAt);
            field.MoveSpieler(1, 0);

            Assert.True(view.TryConsumePendingBuild(out G5_Bauwerkzeug tool, out G5_GridPosition position));
            Assert.Equal(G5_Bauwerkzeug.Lager, tool);
            Assert.Equal(droppedAt, position);
            Assert.False(view.TryConsumePendingBuild(out _, out _));
        });
    }

    [Theory]
    [InlineData(0, -1)]
    [InlineData(1, 0)]
    [InlineData(0, 1)]
    [InlineData(-1, 0)]
    public void PlayerFacePointsTowardsActionTile(int dx, int dy)
    {
        RunOnSta(() =>
        {
            G5_FabrikSpielfeld field = CreateField();
            field.MoveSpieler(dx, dy);
            SatisfactoryFieldView view = new SatisfactoryFieldView(
                field, new SatisfactoryStartScreen(), new SatisfactoryGameMentor(), true);
            Canvas canvas = new Canvas { Width = 800, Height = 600 };
            canvas.Measure(new Size(800, 600));
            canvas.Arrange(new Rect(0, 0, 800, 600));

            new G5_FabrikPainter().PaintGameField(canvas, view);

            Image player = Assert.Single(canvas.Children.OfType<Canvas>()
                .SelectMany(layer => layer.Children.OfType<Image>()),
                image => image.RenderTransform is RotateTransform);
            Vector front = player.RenderTransform.Transform(new Point(0, 1)) -
                player.RenderTransform.Transform(new Point(0, 0));
            G5_GridPosition position = ((IG5_FabrikSpielInfo)view).SpielerPosition;
            G5_GridPosition target = ((IG5_FabrikSpielInfo)view).ZielKachel;
            Assert.Equal(target.X - position.X, front.X, 6);
            Assert.Equal(target.Y - position.Y, front.Y, 6);
        });
    }

    private static void RunOnSta(Action action)
    {
        Exception? failure = null;
        Thread thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                failure = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (failure != null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }
    }

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
    public void FirstMinerUsesOnlyResourcesThePlayerCanMine()
    {
        IReadOnlyDictionary<G5_Item, int> minerCost =
            G5_FabrikKosten.Baupreis(G5_Bauwerkzeug.Foerderer);

        Assert.Equal(8, minerCost[G5_Item.Eisenerz]);
        Assert.Equal(5, minerCost[G5_Item.Holz]);
        Assert.All(minerCost.Keys, item =>
            Assert.Contains(item, new[] { G5_Item.Eisenerz, G5_Item.Holz }));
    }

    [Fact]
    public void ManualRecipesAreOrderedFromRawResourcesToComponents()
    {
        IReadOnlyList<G5_Recipe> recipes = G5_StarterRecipes.CreateManualCraftingRecipes();
        G5_Item[] outputs = recipes.Select(recipe => recipe.Output).ToArray();

        Assert.Equal(
            new[]
            {
                G5_Item.Eisenbarren,
                G5_Item.Kupferbarren,
                G5_Item.Beton,
                G5_Item.Eisenplatte,
                G5_Item.Eisenstange
            },
            outputs);

        HashSet<G5_Item> available = new HashSet<G5_Item>
        {
            G5_Item.Eisenerz,
            G5_Item.Kupfererz,
            G5_Item.Kalkstein,
            G5_Item.Holz,
            G5_Item.Kohle
        };
        foreach (G5_Recipe recipe in recipes)
        {
            Assert.All(recipe.Ingredients.Keys, ingredient => Assert.Contains(ingredient, available));
            available.Add(recipe.Output);
        }

        foreach (G5_Bauwerkzeug tool in Enum.GetValues(typeof(G5_Bauwerkzeug)).Cast<G5_Bauwerkzeug>())
        {
            IReadOnlyDictionary<G5_Item, int> cost = G5_FabrikKosten.Baupreis(tool);
            Assert.All(cost.Keys, ingredient => Assert.Contains(ingredient, available));
        }
    }

    [Fact]
    public void MentorAdvancesWhenThePlayerMovesAndMines()
    {
        RunOnSta(() =>
        {
            SatisfactoryGameMentor mentor = new SatisfactoryGameMentor();
            mentor.Begin();
            string opening = mentor.Message;

            mentor.ObservePlayerMoved();
            Assert.NotEqual(opening, mentor.Message);
            Assert.Contains("drücke E", mentor.Message);

            mentor.ObserveHandMining(G5_Item.Eisenerz);
            Assert.Contains("8 Eisenerz", mentor.Message);
        });
    }

    [Fact]
    public void WorkshopDisplaysClickableManualRecipes()
    {
        RunOnSta(() =>
        {
            SatisfactoryFieldView view = new SatisfactoryFieldView(
                CreateField(), new SatisfactoryStartScreen(), new SatisfactoryGameMentor(), true);
            view.ToggleHandwerk();
            Canvas canvas = new Canvas { Width = 1000, Height = 800 };
            canvas.Measure(new Size(1000, 800));
            canvas.Arrange(new Rect(0, 0, 1000, 800));

            new G5_FabrikPainter().PaintGameField(canvas, view);

            Assert.True(view.TryGetMenuAktion(
                800,
                160,
                out G5_FabrikAktionstyp type,
                out _,
                out string output));
            Assert.Equal(G5_FabrikAktionstyp.ManuellHerstellen, type);
            Assert.Equal(nameof(G5_Item.Eisenbarren), output);
        });
    }

    private static G5_FabrikSpielfeld CreateField()
    {
        return new G5_FabrikSpielfeld(new G5_WeltSpielfeld(10, 10), 5, 5);
    }
}
#endif
