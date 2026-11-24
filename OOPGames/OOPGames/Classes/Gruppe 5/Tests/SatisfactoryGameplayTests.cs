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

        Assert.True(field.TryAddInventar(G5_Item.Holz, G5_FabrikSpielfeld.SpielerInventarKapazitaetMax));
        Assert.False(field.TryAddInventar(G5_Item.Eisenerz, 1));
        Assert.Equal(G5_FabrikSpielfeld.SpielerInventarKapazitaetMax, field.InventarBelegt);

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

    private static G5_FabrikSpielfeld CreateField()
    {
        return new G5_FabrikSpielfeld(new G5_WeltSpielfeld(10, 10), 5, 5);
    }
}
#endif
