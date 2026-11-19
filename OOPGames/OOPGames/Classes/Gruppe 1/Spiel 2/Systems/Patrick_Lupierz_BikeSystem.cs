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

        /// <summary>Änderung durch: Patrick Lupierz – 16.11.2026 – Rad wartet vor der Tür des betretenen Gebäudes.</summary>
        public bool ParkedAtDoor { get; private set; }
        public bool BlockedByShark { get; private set; }
        public void SetBlockedByShark(bool blocked)
        {
            BlockedByShark = blocked;
            if (blocked && Riding) Dismount();
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026 – beim Betreten eines Gebäudes abstellen (force: beim Laden)
        public void ParkAtDoor(bool force = false)
        {
            if (!Riding && !force) return;
            Riding = false;
            ParkedAtDoor = true;
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026 – beim Hinausgehen wieder aufsteigen
        public void ResumeFromDoor()
        {
            if (!ParkedAtDoor) return;
            ParkedAtDoor = false;
            Riding = true;
        }

        // Entwickler: Patrick Lupierz
        public string TryMount(G1C_Inventory inventory, string bikeId, G1C_WorldLocation location)
        {
            if (BlockedByShark) return "Viktor hat dir das Fahrradfahren nach dem Knochenbruch verboten.";
            G1C_Item bike = G1C_ItemCatalog.Get(bikeId);
            if (bike == null || bike.Category != G1C_ItemCategory.Fahrzeug) return "Das ist kein Fahrrad.";
            if (inventory == null || !inventory.Has(bikeId)) return "Dieses Fahrrad besitzt du nicht.";
            if (location != G1C_WorldLocation.Exterior) return "Fahrrad fahren geht nur draußen in der Stadt.";
            Riding = true;
            ParkedAtDoor = false;
            ActiveBikeId = bikeId;
            return "Du fährst jetzt mit dem " + bike.Name + ". Schneller unterwegs!";
        }

        public string Dismount()
        {
            if (!Riding) return "Du fährst gerade nicht Fahrrad.";
            Riding = false;
            ParkedAtDoor = false;
            return "Du bist abgestiegen.";
        }

        // Entwickler: Patrick Lupierz – Tempo: Fahrrad beschleunigt, langsamere Effekte (z. B. Crash) bleiben wirksam
        public int StepTicks(int walkingTicks)
        {
            return Riding ? Math.Min(walkingTicks, RidingStepTicks) : walkingTicks;
        }

        // Änderung durch: Patrick Lupierz – Datum: 16.11.2026 – Fehler "Fahrrad zeigt immer in eine Richtung"
        // behoben: eigene Grafik für links (gespiegelt), rechts, oben (Rückansicht) und unten (Frontansicht).
        public string RideSpriteId(G1C_Direction facing)
        {
            switch (facing)
            {
                case G1C_Direction.Left: return "ride_" + ActiveBikeId + "_left";
                case G1C_Direction.Right: return "ride_" + ActiveBikeId + "_right";
                case G1C_Direction.Up: return "ride_" + ActiveBikeId + "_back";
                default: return "ride_" + ActiveBikeId + "_front";
            }
        }

        public void Restore(bool riding, string bikeId)
        {
            ActiveBikeId = G1C_ItemCatalog.Get(bikeId) != null ? bikeId : StarterBike;
            Riding = riding;
            ParkedAtDoor = false;
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
