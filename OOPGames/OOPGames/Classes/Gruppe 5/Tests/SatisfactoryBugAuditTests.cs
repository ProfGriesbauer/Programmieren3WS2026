#if GROUP5_TESTS
using System;
using System.Windows;
using OOPGames;
using Xunit.Abstractions;

namespace Gruppe5Tests;

public sealed class SatisfactoryBugAuditTests
{
    private readonly ITestOutputHelper _output;

    public SatisfactoryBugAuditTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Theory]
    [InlineData(0, 0, -1, 0)]
    [InlineData(0, 0, 0, -1)]
    [InlineData(3, 0, 1, 0)]
    [InlineData(0, 3, -1, 0)]
    [InlineData(3, 3, 1, 0)]
    [InlineData(3, 3, 0, 1)]
    public void PlayerCannotMoveBeyondMapEdges(int x, int y, int dx, int dy)
    {
        G5_FabrikSpielfeld field = CreateField(4, 4, x, y);

        bool moved = field.MoveSpieler(dx, dy);

        ReportCheck(
            !moved && field.SpielerPosition == new G5_GridPosition(x, y),
            "Spieler bleibt innerhalb des Kartenrasters.",
            $"Spieler konnte von ({x},{y}) aus der Karte laufen.");
    }

    [Fact]
    public void PlayerCannotWalkThroughObstacleOrBuilding()
    {
        G5_WeltSpielfeld world = new G5_WeltSpielfeld(4, 4);
        Assert.True(world.TrySetGelaende(1, 0, G5_Gelaendeart.Hindernis));
        G5_FabrikSpielfeld field = new G5_FabrikSpielfeld(world, 0, 0);

        bool walkedThroughObstacle = field.MoveSpieler(1, 0);
        ReportCheck(
            !walkedThroughObstacle && field.SpielerPosition == new G5_GridPosition(0, 0),
            "Hinderniskacheln blockieren die Bewegung.",
            "Spieler konnte durch ein Hindernis laufen.");

        Assert.True(world.TrySetGelaende(1, 0, G5_Gelaendeart.Begehbar));
        field.AddGebaeude(new G5_Storage(new G5_GridPosition(1, 0), capacity: 2));
        bool walkedThroughBuilding = field.MoveSpieler(1, 0);
        ReportCheck(
            !walkedThroughBuilding && field.SpielerPosition == new G5_GridPosition(0, 0),
            "Gebäude-Footprints blockieren die Bewegung.",
            "Spieler konnte durch ein Gebäude laufen.");
    }

    [Fact]
    public void ScreenAndGridConversionRejectCoordinatesOutsideMap()
    {
        G5_WeltSpielfeld world = new G5_WeltSpielfeld(4, 4);
        IG5_WeltSpielfeld view = world;

        bool negativeScreenCoordinate = view.TryBildschirmZuRaster(-1, 10, out _, out _);
        bool beyondRightEdge = view.TryBildschirmZuRaster(154, 10, out _, out _);
        bool beyondBottomEdge = view.TryBildschirmZuRaster(10, 154, out _, out _);
        bool negativeGridCoordinate = view.TryRasterZuBildschirm(-1, 0, out Point _);
        bool beyondGridWidth = view.TryRasterZuBildschirm(4, 0, out Point _);
        bool beyondGridHeight = view.TryRasterZuBildschirm(0, 4, out Point _);

        ReportCheck(
            !negativeScreenCoordinate &&
            !beyondRightEdge &&
            !beyondBottomEdge &&
            !negativeGridCoordinate &&
            !beyondGridWidth &&
            !beyondGridHeight,
            "Bildschirm- und Rasterumrechnung weist Außenkoordinaten zurück.",
            "Eine Außenkoordinate wurde als gültige Kartenposition akzeptiert.");
    }

    [Fact]
    public void FactoryRejectsOverlappingBuildingsWithoutAddingSecondOne()
    {
        G5_FactorySimulation simulation = new G5_FactorySimulation();
        G5_Storage first = new G5_Storage(new G5_GridPosition(1, 1), capacity: 2);
        G5_Storage overlapping = new G5_Storage(new G5_GridPosition(1, 1), capacity: 2);
        simulation.AddBuilding(first);

        InvalidOperationException error = Assert.Throws<InvalidOperationException>(
            () => simulation.AddBuilding(overlapping));

        ReportCheck(
            error.Message.Contains("same grid cells", StringComparison.Ordinal) &&
            simulation.Buildings.Count == 1,
            "Gebäude dürfen keine belegten Rasterfelder überlappen.",
            "Überlappende Gebäude wurden angenommen oder teilweise gespeichert.");
    }

    [Fact]
    public void PlayerMovementMustBeExactlyOneCardinalTile()
    {
        G5_FabrikSpielfeld field = CreateField(4, 4, 1, 1);

        Assert.Throws<ArgumentException>(() => field.MoveSpieler(1, 1));
        Assert.Throws<ArgumentException>(() => field.MoveSpieler(0, 0));
        ReportCheck(
            field.SpielerPosition == new G5_GridPosition(1, 1),
            "Bewegungsbefehle ändern keine Position, wenn sie ungültig sind.",
            "Ein ungültiger Bewegungsbefehl hat die Spielerposition verändert.");
    }

    private static G5_FabrikSpielfeld CreateField(int width, int height, int playerX, int playerY)
    {
        return new G5_FabrikSpielfeld(new G5_WeltSpielfeld(width, height), playerX, playerY);
    }

    private void ReportCheck(bool passed, string check, string bug)
    {
        _output.WriteLine(passed ? "OK: " + check : "BUG: " + bug);
        Assert.True(passed, "BUG: " + bug);
    }
}
#endif
