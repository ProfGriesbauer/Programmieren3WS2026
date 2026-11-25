#if GROUP5_TESTS
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

    [Theory]
    [InlineData(G5_FabrikGebaeudeTyp.Foerderer)]
    [InlineData(G5_FabrikGebaeudeTyp.Schmelze)]
    [InlineData(G5_FabrikGebaeudeTyp.Konstruktor)]
    [InlineData(G5_FabrikGebaeudeTyp.Assembler)]
    [InlineData(G5_FabrikGebaeudeTyp.Verbrennungsgenerator)]
    [InlineData(G5_FabrikGebaeudeTyp.Strommast)]
    [InlineData(G5_FabrikGebaeudeTyp.Lager)]
    [InlineData(G5_FabrikGebaeudeTyp.Rakete)]
    public void BuildingArtworkFillsTheTile(G5_FabrikGebaeudeTyp type)
    {
        RunOnSta(() =>
        {
            const int size = 128;
            foreach (int level in new[] { 1, 3 })
            {
                Canvas artwork = G5_FabrikGebaeudeZeichner.Create(type, level, size, size, kachelFuellen: true);
                artwork.Measure(new Size(size, size));
                artwork.Arrange(new Rect(0, 0, size, size));
                artwork.UpdateLayout();
                RenderTargetBitmap bitmap = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(artwork);
                byte[] pixels = new byte[size * size * 4];
                bitmap.CopyPixels(pixels, size * 4, 0);

                int left = size, top = size, right = -1, bottom = -1;
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        if (pixels[(y * size + x) * 4 + 3] < 16) continue;
                        left = Math.Min(left, x);
                        top = Math.Min(top, y);
                        right = Math.Max(right, x);
                        bottom = Math.Max(bottom, y);
                    }
                }

                Assert.True(right - left + 1 >= size * 0.9,
                    $"{type} MK {level}: artwork width {right - left + 1}/{size}");
                Assert.True(bottom - top + 1 >= size * 0.9,
                    $"{type} MK {level}: artwork height {bottom - top + 1}/{size}");
            }
        });
    }

    [Fact]
    public void LevelIsShownInBuildingMenuInsteadOfOnTheTile()
    {
        RunOnSta(() =>
        {
            G5_FabrikSpielfeld field = CreateField();
            G5_Storage storage = new G5_Storage(new G5_GridPosition(5, 6), 100);
            field.AddGebaeude(storage);
            Assert.True(field.SelectBuildingAt(5, 6));
            SatisfactoryFieldView view = new SatisfactoryFieldView(
                field, new SatisfactoryStartScreen(), new SatisfactoryGameMentor(), true);
            Canvas canvas = new Canvas { Width = 800, Height = 600 };
            canvas.Measure(new Size(800, 600));
            canvas.Arrange(new Rect(0, 0, 800, 600));
            new G5_FabrikPainter().PaintGameField(canvas, view);
            canvas.UpdateLayout();
            string[] labels = VisualDescendants(canvas).OfType<TextBlock>()
                .Select(block => block.Text).ToArray();

            Assert.DoesNotContain("MK1", labels);
            Assert.Contains(labels, label => label.Contains("Lager  ·  MK 1"));
        });
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

    [Theory]
    [InlineData(G5_Item.Eisenbarren, G5_Item.Eisenerz, 1, 11)]
    [InlineData(G5_Item.Kupferbarren, G5_Item.Kupfererz, 1, 12)]
    [InlineData(G5_Item.Eisenplatte, G5_Item.Eisenbarren, 1, 13)]
    [InlineData(G5_Item.Eisenstange, G5_Item.Eisenbarren, 1, 14)]
    [InlineData(G5_Item.Beton, G5_Item.Kalkstein, 2, 15)]
    public void InventoryCraftButtonProducesTheSelectedBuildingMaterial(
        G5_Item product, G5_Item ingredient, int ingredientCount, int row)
    {
        RunOnSta(() =>
        {
            (SatisfactoryGameRules rules, G5_FabrikPainter painter, Canvas canvas) = StartGame();
            SatisfactoryFieldView view = (SatisfactoryFieldView)rules.CurrentField;
            G5_FabrikSpielfeld field = view.Spielfeld;
            Assert.True(field.TryAddInventar(ingredient, ingredientCount));
            painter.PaintGameField(canvas, view);
            canvas.UpdateLayout();
            G5_FabrikHumanPlayer player = new G5_FabrikHumanPlayer();
            player.SetPlayerNumber(1);

            // 1. Inventarzeile anklicken (kombiniertes Inventar: Zeilen ab y = 61, je 18 px).
            double panelLeft = canvas.ActualWidth - 268 - 10;
            int rowY = 66 + 9 + 61 + row * 18 + 8;
            IPlayMove selectMove = player.GetMove(
                new ClickSelection((int)(panelLeft + 60), rowY, (int)MouseButton.Left), view);
            Assert.NotNull(selectMove);
            Assert.Equal(G5_FabrikAktionstyp.InventarGegenstandWaehlen, ((G5_FabrikAktion)selectMove).Aktionstyp);
            rules.DoMove(selectMove);
            painter.PaintGameField(canvas, view);
            canvas.UpdateLayout();

            // 2. Knopf "HERSTELLEN ×1" im Detailbereich anklicken.
            TextBlock craftLabel = Assert.Single(VisualDescendants(canvas).OfType<TextBlock>(),
                text => text.Text == "HERSTELLEN ×1");
            Point craftPosition = craftLabel.TranslatePoint(new Point(0, 0), canvas);
            int x = (int)(craftPosition.X + 10);
            int y = (int)(craftPosition.Y + 5);
            IPlayMove move = player.GetMove(new ClickSelection(x, y, (int)MouseButton.Left), view);
            Assert.NotNull(move);
            Assert.Equal(G5_FabrikAktionstyp.ManuellHerstellen, ((G5_FabrikAktion)move).Aktionstyp);
            rules.DoMove(move);
            Assert.True(field.ManuellesHandwerkLaeuft);
            Assert.Equal(0, field.Menge(product));
            for (int tick = 0; tick < G5_FabrikSpielfeld.ManuellesHandwerkDauerTicks; tick++)
            {
                rules.TickGameCall();
            }

            Assert.Equal(1, field.Menge(product));
            Assert.Equal(0, field.Menge(ingredient));
        });
    }

    private static IEnumerable<DependencyObject> VisualDescendants(DependencyObject parent)
    {
        for (int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, index);
            yield return child;
            foreach (DependencyObject descendant in VisualDescendants(child))
            {
                yield return descendant;
            }
        }
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
    public void CraftingAndBuildingRecipesAreAvailableFromRawResources()
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

        IEnumerable<G5_Recipe> productionRecipes =
            G5_StarterRecipes.CreateSmeltingRecipes(1, 0)
                .Concat(G5_StarterRecipes.CreateConstructionRecipes(1, 0))
                .Concat(G5_StarterRecipes.CreateAssemblyRecipes(1, 0));
        foreach (G5_Recipe recipe in productionRecipes)
        {
            Assert.All(recipe.Ingredients.Keys, ingredient => Assert.Contains(ingredient, available));
            available.Add(recipe.Output);
        }

        foreach (G5_Bauwerkzeug tool in Enum.GetValues(typeof(G5_Bauwerkzeug)).Cast<G5_Bauwerkzeug>())
        {
            IReadOnlyDictionary<G5_Item, int> cost = G5_FabrikKosten.Baupreis(tool);
            Assert.All(cost.Keys, ingredient => Assert.Contains(ingredient, available));
        }

        Assert.Contains(G5_Item.Rotor, available);
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
            Assert.Contains("Eisenerz", mentor.Hint);
            string instruction = mentor.Message;

            mentor.ObserveHandMining(G5_Item.Holz);
            Assert.Equal(instruction, mentor.Message);
            Assert.Contains("Holz", mentor.Hint);
        });
    }

    [Fact]
    public void MentorKeepsGuidingThroughTheFullFactoryMilestonePath()
    {
        SatisfactoryGameMentor mentor = new SatisfactoryGameMentor();
        mentor.Begin();
        mentor.ObservePlayerMoved();
        mentor.ObserveHandMining(G5_Item.Eisenerz);
        mentor.ObserveBuildingPlaced(
            G5_Bauwerkzeug.Foerderer, "Förderer", "Fördert Eisenerz.", G5_Item.Eisenerz);
        mentor.ObserveBuildingPlaced(G5_Bauwerkzeug.Schmelze, "Schmelze", "Schmilzt Erz.");

        Assert.True(mentor.IsActive);
        Assert.Equal("SCHRITT 5/12", mentor.Progress);
        Assert.Contains("Förderbandlinie", mentor.Message);

        mentor.ObserveConveyorLinePlaced();
        Assert.Contains("Kohlevorkommen", mentor.Message);
        mentor.ObserveBuildingPlaced(
            G5_Bauwerkzeug.Foerderer, "Förderer", "Fördert Kohle.", G5_Item.Kohle);
        Assert.Contains("Kohlekraftwerk", mentor.Message);
        mentor.ObserveBuildingPlaced(
            G5_Bauwerkzeug.Verbrennungsgenerator, "Kohlekraftwerk", "Erzeugt Strom.");
        Assert.Contains("Strommast", mentor.Message);
        mentor.ObserveBuildingPlaced(G5_Bauwerkzeug.Strommast, "Strommast", "Leitet Strom.");
        Assert.Contains("Konstruktor", mentor.Message);
        mentor.ObserveBuildingPlaced(G5_Bauwerkzeug.Konstruktor, "Konstruktor", "Stellt Teile her.");
        Assert.Contains("Assembler", mentor.Message);
        mentor.ObserveBuildingPlaced(G5_Bauwerkzeug.Assembler, "Assembler", "Montiert Teile.");
        Assert.Contains("Lager", mentor.Message);
        mentor.ObserveBuildingPlaced(G5_Bauwerkzeug.Lager, "Lager", "Lagert Teile.");
        Assert.Contains("Rakete", mentor.Message);
        mentor.ObserveBuildingPlaced(G5_Bauwerkzeug.Rakete, "Rakete", "Spielziel erreicht.");

        Assert.True(mentor.IsActive);
        Assert.Equal("TUTORIAL ABGESCHLOSSEN", mentor.Progress);
        Assert.Contains("Rakete gebaut", mentor.Hint);
    }

    [Fact]
    public void MentorRemainsVisibleWhileStatisticsAreOpen()
    {
        RunOnSta(() =>
        {
            G5_FabrikSpielfeld field = CreateField();
            field.ToggleStatistik();
            SatisfactoryGameMentor mentor = new SatisfactoryGameMentor();
            mentor.Begin();
            SatisfactoryFieldView view = new SatisfactoryFieldView(
                field, new SatisfactoryStartScreen(), mentor, true);
            Canvas canvas = new Canvas { Width = 800, Height = 600 };
            canvas.Measure(new Size(800, 600));
            canvas.Arrange(new Rect(0, 0, 800, 600));

            new G5_FabrikPainter().PaintGameField(canvas, view);

            Assert.Contains(view.MentorOverlay, canvas.Children.OfType<Canvas>());
            Assert.Contains(view.MentorOverlay.Children.OfType<TextBlock>(),
                block => block.Text.Contains("SCHRITT 1/12"));
        });
    }

    [Fact]
    public void CombinedInventoryShowsCraftButtonForSelectedItem()
    {
        RunOnSta(() =>
        {
            G5_FabrikSpielfeld field = CreateField();
            field.WaehleAbwurfGegenstand(G5_Item.Eisenbarren);
            SatisfactoryFieldView view = new SatisfactoryFieldView(
                field, new SatisfactoryStartScreen(), new SatisfactoryGameMentor(), true);
            Canvas canvas = new Canvas { Width = 1000, Height = 800 };
            canvas.Measure(new Size(1000, 800));
            canvas.Arrange(new Rect(0, 0, 1000, 800));

            new G5_FabrikPainter().PaintGameField(canvas, view);

            // Panel: links 1000 - 268 - 10 = 722, oben HUD 56 + 10 = 66;
            // Herstellen-Knopf liegt bei Innenversatz 9 und y = 478 im Panelinhalt.
            Assert.True(view.TryGetMenuAktion(
                780,
                66 + 9 + 478 + 15,
                out G5_FabrikAktionstyp type,
                out _,
                out string output));
            Assert.Equal(G5_FabrikAktionstyp.ManuellHerstellen, type);
            Assert.Equal(nameof(G5_Item.Eisenbarren), output);
        });
    }

    [Fact]
    public void RocketBuildFuelLaunchVictoryAndResetWorkThroughGameActions()
    {
        RunOnSta(() =>
        {
            (SatisfactoryGameRules rules, G5_FabrikPainter painter, Canvas canvas) = StartGame();
            SatisfactoryFieldView view = (SatisfactoryFieldView)rules.CurrentField;
            G5_FabrikSpielfeld field = view.Spielfeld;
            IG5_WeltSpielfeld world = view;
            Assert.True(world.TryGetKachelInfo(65, 45, out IG5_KachelInfo uranium));
            Assert.Equal(G5_Rohstofftyp.Uran, uranium.Rohstoff);

            rules.DoMove(G5_FabrikAktion.GebaeudeBauen(1, G5_Bauwerkzeug.Rakete));
            Assert.Empty(field.InterneGebaeude);
            Assert.Equal(0, field.GebauteGebaeude);
            G5_Storage warehouse = new G5_Storage(new G5_GridPosition(58, 38), 500);
            field.AddGebaeude(warehouse);
            foreach (KeyValuePair<G5_Item, int> cost in G5_FabrikKosten.Baupreis(G5_Bauwerkzeug.Rakete))
            {
                Assert.True(warehouse.TryReceive(cost.Key, cost.Value));
            }

            painter.PaintGameField(canvas, view);
            Assert.True(view.TryGetMenuAktion(30, 375, out G5_FabrikAktionstyp rowAction,
                out G5_Bauwerkzeug tool, out _));
            Assert.Equal(G5_FabrikAktionstyp.WerkzeugWaehlen, rowAction);
            Assert.Equal(G5_Bauwerkzeug.Rakete, tool);
            G5_FabrikHumanPlayer player = new G5_FabrikHumanPlayer();
            player.SetPlayerNumber(1);
            IPlayMove selectMove = player.GetMove(new ClickSelection(30, 375, (int)MouseButton.Left), view);
            Assert.NotNull(selectMove);
            rules.DoMove(selectMove);
            painter.PaintGameField(canvas, view);
            IPlayMove buildMove = player.GetMove(new ClickSelection(100, 445, (int)MouseButton.Left), view);
            Assert.NotNull(buildMove);
            Assert.Equal(G5_FabrikAktionstyp.GebaeudeBauen, ((G5_FabrikAktion)buildMove).Aktionstyp);
            rules.DoMove(buildMove);
            G5_Rakete rocket = Assert.Single(field.InterneGebaeude.OfType<G5_Rakete>());
            Assert.Equal(1, field.GebauteGebaeude);
            Assert.Equal(0, warehouse.UsedCapacity);
            rules.DoMove(G5_FabrikAktion.GebaeudeBauen(1, G5_Bauwerkzeug.Rakete));
            Assert.Single(field.InterneGebaeude.OfType<G5_Rakete>());
            Assert.Equal(1, field.GebauteGebaeude);

            rules.DoMove(G5_FabrikAktion.GebaeudeAnsehen(1, rocket.Position.X, rocket.Position.Y));
            painter.PaintGameField(canvas, view);
            Assert.True(view.TryGetMenuAktion(550, 199, out G5_FabrikAktionstyp fuelAction,
                out _, out _));
            Assert.Equal(G5_FabrikAktionstyp.RaketeBetanken, fuelAction);
            rules.DoMove(G5_FabrikAktion.RaketeStarten(1));
            IPlayMove fuelMove = player.GetMove(new ClickSelection(550, 199, (int)MouseButton.Left), view);
            Assert.NotNull(fuelMove);
            rules.DoMove(fuelMove);
            Assert.Equal(G5_Spielphase.Fabrik, field.Spielphase);
            Assert.Equal(0, rocket.UranVorrat);

            Assert.True(field.TryAddInventar(G5_Item.Uran, 17));
            rules.DoMove(G5_FabrikAktion.RaketeBetanken(1));
            Assert.Equal(17, rocket.UranVorrat);
            rules.DoMove(G5_FabrikAktion.RaketeStarten(1));
            Assert.Equal(G5_Spielphase.Fabrik, field.Spielphase);
            Assert.True(warehouse.TryReceive(G5_Item.Uran, 50));
            Assert.True(field.MoveSpieler(1, 0));
            rules.DoMove(G5_FabrikAktion.RaketeBetanken(1));
            Assert.Equal(17, rocket.UranVorrat);
            Assert.True(field.MoveSpieler(-1, 0));
            rules.DoMove(G5_FabrikAktion.RaketeBetanken(1));
            Assert.Equal(50, rocket.UranVorrat);
            Assert.Equal(17, field.Gesamtmenge(G5_Item.Uran));
            rules.DoMove(G5_FabrikAktion.RaketeBetanken(1));
            Assert.Equal(17, field.Gesamtmenge(G5_Item.Uran));

            rules.DoMove(G5_FabrikAktion.RaketeStarten(1));
            Assert.Equal(G5_Spielphase.Raketenstart, field.Spielphase);
            G5_GridPosition position = field.SpielerPosition;
            rules.DoMove(G5_FabrikAktion.SpielerBewegen(1, 1, 0));
            Assert.Equal(position, field.SpielerPosition);
            painter.PaintGameField(canvas, view);
            for (int tick = 0; tick < G5_FabrikSpielfeld.RaketenstartDauerTicks - 1; tick++)
            {
                rules.TickGameCall();
            }

            Assert.Equal(G5_Spielphase.Raketenstart, field.Spielphase);
            rules.TickGameCall();
            Assert.Equal(G5_Spielphase.Gewonnen, field.Spielphase);
            Assert.Equal(-1, rules.CheckIfPLayerWon());
            painter.PaintGameField(canvas, view);
            Assert.True(view.TryGetMenuAktion(400, 320, out G5_FabrikAktionstyp statsAction,
                out _, out _));
            Assert.Equal(G5_FabrikAktionstyp.StatistikUmschalten, statsAction);
            IPlayMove statsMove = player.GetMove(new KeySelection(Key.F2), view);
            Assert.NotNull(statsMove);
            rules.DoMove(statsMove);
            Assert.True(field.StatistikGeoeffnet);
            painter.PaintGameField(canvas, view);
            Assert.True(view.TryGetMenuAktion(620, 130, out statsAction, out _, out _));
            Assert.Equal(G5_FabrikAktionstyp.StatistikUmschalten, statsAction);

            rules.ClearField();
            G5_FabrikSpielfeld restarted = ((SatisfactoryFieldView)rules.CurrentField).Spielfeld;
            Assert.Equal(G5_Spielphase.Fabrik, restarted.Spielphase);
            Assert.False(restarted.StatistikGeoeffnet);
            Assert.Equal(0, restarted.GebauteGebaeude);
            Assert.Empty(restarted.AbgebauteRohstoffe);
            Assert.Empty(restarted.HergestellteProdukte);
        });
    }

    [Fact]
    public void SellingFuelledRocketReturnsAllUranAndHalfOfBuildingCost()
    {
        RunOnSta(() =>
        {
            (SatisfactoryGameRules rules, _, _) = StartGame();
            G5_FabrikSpielfeld field = ((SatisfactoryFieldView)rules.CurrentField).Spielfeld;
            G5_Storage warehouse = new G5_Storage(new G5_GridPosition(58, 38), 500);
            field.AddGebaeude(warehouse);
            foreach (KeyValuePair<G5_Item, int> cost in G5_FabrikKosten.Baupreis(G5_Bauwerkzeug.Rakete))
            {
                Assert.True(warehouse.TryReceive(cost.Key, cost.Value));
            }
            rules.DoMove(G5_FabrikAktion.GebaeudeBauen(1, G5_Bauwerkzeug.Rakete));
            G5_Rakete rocket = Assert.Single(field.InterneGebaeude.OfType<G5_Rakete>());
            rules.DoMove(G5_FabrikAktion.GebaeudeAnsehen(1, rocket.Position.X, rocket.Position.Y));
            Assert.True(field.TryAddInventar(G5_Item.Uran, 23));
            rules.DoMove(G5_FabrikAktion.RaketeBetanken(1));
            Assert.True(warehouse.TryReceive(G5_Item.Holz, 500));
            rules.DoMove(G5_FabrikAktion.GebaeudeVerkaufen(1));
            Assert.Contains(rocket, field.InterneGebaeude);
            Assert.Equal(23, rocket.UranVorrat);
            Assert.True(warehouse.TryExtract(G5_Item.Holz, 500));
            rules.DoMove(G5_FabrikAktion.GebaeudeVerkaufen(1));

            Assert.Empty(field.InterneGebaeude.OfType<G5_Rakete>());
            Assert.Equal(23, field.Gesamtmenge(G5_Item.Uran));
            foreach (KeyValuePair<G5_Item, int> cost in G5_FabrikKosten.Baupreis(G5_Bauwerkzeug.Rakete))
            {
                Assert.Equal(cost.Value / 2, field.Gesamtmenge(cost.Key));
            }
            Assert.Equal(1, field.GebauteGebaeude);
        });
    }

    [Fact]
    public void StatisticsCountMiningAndCompletedProductsButNotTransfers()
    {
        RunOnSta(() =>
        {
            (SatisfactoryGameRules rules, _, _) = StartGame();
            G5_FabrikSpielfeld field = ((SatisfactoryFieldView)rules.CurrentField).Spielfeld;
            rules.DoMove(G5_FabrikAktion.SpielerBewegen(1, 1, 0));
            rules.DoMove(G5_FabrikAktion.SpielerBewegen(1, 1, 0));
            rules.DoMove(G5_FabrikAktion.RohstoffAbbauen(1));
            Assert.Equal(1, field.AbgebauteRohstoffe[G5_Item.Eisenerz]);
            rules.DoMove(G5_FabrikAktion.RohstoffAbbauen(1));
            Assert.Equal(1, field.AbgebauteRohstoffe[G5_Item.Eisenerz]);

            G5_Miner miner = new G5_Miner(new G5_GridPosition(1, 1),
                new G5_ResourceDeposit(G5_Item.Uran, 1), 2, 1, 1, 0);
            G5_Smelter smelter = new G5_Smelter(new G5_GridPosition(2, 1),
                G5_StarterRecipes.CreateSmeltingRecipes(1, 0), 5, 5);
            Assert.True(smelter.SelectRecipe("Eisenerz schmelzen"));
            Assert.True(smelter.TryReceive(G5_Item.Eisenerz, 1));
            field.AddGebaeude(miner);
            field.AddGebaeude(smelter);
            field.Simulation.Tick();
            Assert.Equal(1, field.AbgebauteRohstoffe[G5_Item.Uran]);
            Assert.Equal(1, field.HergestellteProdukte[G5_Item.Eisenbarren]);
            Assert.True(field.TryCollectMinerOutput(miner, 1, out int collected));
            Assert.Equal(1, collected);
            field.Simulation.Tick();
            Assert.Equal(1, field.AbgebauteRohstoffe[G5_Item.Uran]);
            Assert.Equal(1, field.HergestellteProdukte[G5_Item.Eisenbarren]);

            G5_Recipe recipe = G5_StarterRecipes.CreateManualCraftingRecipes()
                .Single(candidate => candidate.Output == G5_Item.Eisenbarren);
            Assert.True(field.TryStarteManuellesHandwerk(recipe));
            for (int tick = 0; tick < G5_FabrikSpielfeld.ManuellesHandwerkDauerTicks; tick++)
            {
                field.TickManuellesHandwerk();
            }
            Assert.Equal(2, field.HergestellteProdukte[G5_Item.Eisenbarren]);
        });
    }

    private static (SatisfactoryGameRules Rules, G5_FabrikPainter Painter, Canvas Canvas) StartGame()
    {
        SatisfactoryGameRules rules = new SatisfactoryGameRules();
        G5_FabrikPainter painter = new G5_FabrikPainter();
        Canvas canvas = new Canvas { Width = 800, Height = 600 };
        canvas.Measure(new Size(800, 600));
        canvas.Arrange(new Rect(0, 0, 800, 600));
        painter.PaintGameField(canvas, rules.CurrentField);
        rules.DoMove(G5_FabrikAktion.GebaeudePlatzieren(1, 500, 230));
        Assert.True(((SatisfactoryFieldView)rules.CurrentField).IsGameStarted);
        return (rules, painter, canvas);
    }

    private static G5_FabrikSpielfeld CreateField()
    {
        return new G5_FabrikSpielfeld(new G5_WeltSpielfeld(10, 10), 5, 5);
    }
}
#endif
