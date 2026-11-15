using System;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_TileMask
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Nachbarschafts-Maske einer Kachel (Nord 1, Ost 2, Süd 4, West 8) – wie bei Straßen und
    //               Wasser. Wird für zusammengesetzte Bodengrafiken (Teppiche) verwendet.
    // =====================================================
    public static class G1C_TileMask
    {
        public static int Of(int x, int y, Func<int, int, bool> same)
        {
            int mask = 0;
            if (same(x, y - 1)) mask |= 1;
            if (same(x + 1, y)) mask |= 2;
            if (same(x, y + 1)) mask |= 4;
            if (same(x - 1, y)) mask |= 8;
            return mask;
        }
    }

    // =====================================================
    // Klasse: G1C_ShopWindow
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Verkaufsfenster an einer Außenwand (Tankstelle). Der Name ist der Verkäufer, damit sein
    //               Gesprächsprofil greift; ShopLocation sagt, welches Sortiment und welche Öffnungszeiten gelten.
    // =====================================================
    public sealed class G1C_ShopWindow : G1C_WorldObject
    {
        public G1C_ShopWindow(string sellerName, string spriteId, int x, int y, G1C_WorldLocation shopLocation)
            : base(sellerName, spriteId, x, y, 1, 1, true, G1C_InteractionType.Shop)
        {
            ShopLocation = shopLocation;
        }

        public G1C_WorldLocation ShopLocation { get; }
    }
}
