using System;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_BikeSystem
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Fahrradbesitz und -nutzung. Besitz liegt im G1C_Inventory (Kategorie Fahrzeug), hier nur:
    //               fährt der Spieler gerade, mit welchem Rad, und wie schnell. Fahren geht nur in der Stadt;
    //               beim Betreten eines Gebäudes wird automatisch abgestiegen. Fahrrad = 3 statt 5 Ticks pro Feld.
    // =====================================================
    public sealed class G1C_BikeSystem
    {
        public const string StarterBike = "bike_red";
        public const int RidingStepTicks = 3;

        public bool Riding { get; private set; }
        public string ActiveBikeId { get; private set; } = StarterBike;

        // Entwickler: Patrick Lupierz
        public string TryMount(G1C_Inventory inventory, string bikeId, G1C_WorldLocation location)
        {
            G1C_Item bike = G1C_ItemCatalog.Get(bikeId);
            if (bike == null || bike.Category != G1C_ItemCategory.Fahrzeug) return "Das ist kein Fahrrad.";
            if (inventory == null || !inventory.Has(bikeId)) return "Dieses Fahrrad besitzt du nicht.";
            if (location != G1C_WorldLocation.Exterior) return "Fahrrad fahren geht nur draußen in der Stadt.";
            Riding = true;
            ActiveBikeId = bikeId;
            return "Du fährst jetzt mit dem " + bike.Name + ". Schneller unterwegs!";
        }

        public string Dismount()
        {
            if (!Riding) return "Du fährst gerade nicht Fahrrad.";
            Riding = false;
            return "Du bist abgestiegen.";
        }

        // Entwickler: Patrick Lupierz – Tempo: Fahrrad beschleunigt, langsamere Effekte (z. B. Crash) bleiben wirksam
        public int StepTicks(int walkingTicks)
        {
            return Riding ? Math.Min(walkingTicks, RidingStepTicks) : walkingTicks;
        }

        public string RideSpriteId(G1C_Direction facing)
        {
            bool vertical = facing == G1C_Direction.Up || facing == G1C_Direction.Down;
            return "ride_" + ActiveBikeId + (vertical ? "_front" : "_side");
        }

        public void Restore(bool riding, string bikeId)
        {
            ActiveBikeId = G1C_ItemCatalog.Get(bikeId) != null ? bikeId : StarterBike;
            Riding = riding;
        }
    }

    // =====================================================
    // Klasse: G1C_ParkedBike
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Fahrradständer vor dem Spielerhaus. Zeigt das aktive Rad, solange der Spieler nicht fährt,
    //               sonst den leeren Ständer. Die Grafik-ID kommt vom GameManager (SpriteSelector).
    // =====================================================
    public sealed class G1C_ParkedBike : G1C_WorldObject
    {
        public G1C_ParkedBike(int x, int y)
            : base("Dein Fahrrad", "obj_bike_red", x, y, 1, 1, true, G1C_InteractionType.Bike)
        {
        }

        public Func<string> SpriteSelector { get; set; }

        public override string SpriteId { get { return SpriteSelector != null ? SpriteSelector() : base.SpriteId; } }
    }
}
