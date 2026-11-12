using System.Linq;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_PlayerProfile
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Name und Aussehen der Spielfigur. Aus Haar-, Hautfarbe und der angezogenen
    //               Kleidung entsteht eine Sprite-ID ("pc-..."), aus der G1C_BuiltInSprites alle
    //               vier Laufrichtungen und Schrittbilder in den passenden Farben erzeugt.
    // =====================================================
    public class G1C_PlayerProfile
    {
        public const int MaxNameLength = 10;

        public static readonly string[] HairNames = { "Rot", "Schwarz", "Braun", "Blond", "Grau", "Blau" };
        public static readonly uint[] HairColors = { 0xD83030, 0x282830, 0x8A5020, 0xF0D060, 0xB0B0B8, 0x3878D8 };
        public static readonly string[] SkinNames = { "Hell", "Mittel", "Gebräunt", "Dunkel" };
        public static readonly uint[] SkinColors = { 0xF8C898, 0xE8B080, 0xC89060, 0x8A5A3A };

        public string Name { get; set; } = "";
        public int Hair { get; set; }
        public int Skin { get; set; }
        public string Top { get; set; } = "top_blue";
        public string Pants { get; set; } = "pants_dark";
        public string Shoes { get; set; } = "shoes_red";
        public string Hat { get; set; } = "";

        public static bool IsValidName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) && name.Length <= MaxNameLength && name.All(char.IsLetterOrDigit);
        }

        public string Equipped(G1C_ClothingSlot slot)
        {
            switch (slot)
            {
                case G1C_ClothingSlot.Top: return Top;
                case G1C_ClothingSlot.Pants: return Pants;
                case G1C_ClothingSlot.Shoes: return Shoes;
                case G1C_ClothingSlot.Hat: return Hat;
                default: return "";
            }
        }

        // Entwickler: Patrick Lupierz – Kleidungsstück anziehen; eine getragene Mütze wird wieder abgesetzt
        public void Equip(G1C_Item item)
        {
            switch (item.Slot)
            {
                case G1C_ClothingSlot.Top: Top = item.Id; break;
                case G1C_ClothingSlot.Pants: Pants = item.Id; break;
                case G1C_ClothingSlot.Shoes: Shoes = item.Id; break;
                case G1C_ClothingSlot.Hat: Hat = Hat == item.Id ? "" : item.Id; break;
            }
        }

        /// <summary>Sprite-Skin: pc-Haar-Haut-Oberteil-Hose-Schuhe-Mütze (Farben hexadezimal, Mütze "x" = keine).</summary>
        public string SkinId
        {
            get
            {
                G1C_Item hat = G1C_ItemCatalog.Get(Hat);
                return "pc-" + HairColors[Index(Hair, HairColors.Length)].ToString("X6") +
                       "-" + SkinColors[Index(Skin, SkinColors.Length)].ToString("X6") +
                       "-" + ColorOf(Top, 0x3060C8) + "-" + ColorOf(Pants, 0x383848) + "-" + ColorOf(Shoes, 0xD83030) +
                       "-" + (hat != null ? hat.Color.ToString("X6") : "x");
            }
        }

        private static string ColorOf(string itemId, uint fallback)
        {
            G1C_Item item = G1C_ItemCatalog.Get(itemId);
            return (item != null ? item.Color : fallback).ToString("X6");
        }

        private static int Index(int value, int length)
        {
            return ((value % length) + length) % length;
        }
    }
}
