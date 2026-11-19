using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_AnimalSprites
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: Eingebaute Pixel-Art für Haustiere und das Angeln. Wie alle Grafiken über
    //               "<Sprite-ID>.png" in G1Casino_Assets austauschbar.
    //               pet_cat_<grey|calico|orange|black|white>_<l|r>_<0..2>   Katze (0 = Stand, 1/2 = Laufen)
    //               pet_dog_<brown|dalmatian|golden>_<l|r>_<0..2>          Hund
    //               fish_<art>_<l|r>                                       Fisch im Angelspiel (Seitenansicht)
    //               obj_fishing_rod, tile_city_plaza                       Angel am Geländer, Steinplatten
    // =====================================================
    public static class G1C_AnimalSprites
    {
        private static readonly Color Outline = Color.FromRgb(0x30, 0x24, 0x28);

        // Entwickler: Patrick Lupierz
        public static BitmapSource Create(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (id == "obj_fishing_rod") return FishingRod().ToBitmap();
            if (id == "tile_city_plaza") return Plaza().ToBitmap();
            // Änderung durch: Patrick Lupierz – 18.11.2026 – Flussfische: river_fish_<art>_<bild>, Grafik aus dem Angelspiel,
            // leicht wasserblau eingefärbt (unter Wasser); Bild 1 ist um ein Pixel versetzt (Schwimmbewegung).
            if (id.StartsWith("river_fish_", StringComparison.Ordinal))
            {
                int cut = id.LastIndexOf('_');
                int riverFrame;
                if (!int.TryParse(id.Substring(cut + 1), out riverFrame)) riverFrame = 0;
                G1C_PixelCanvas fish = G1C_FishSprites.Get(id.Substring("river_".Length, cut - "river_".Length));
                return fish == null ? null : RiverFish(fish, riverFrame).ToBitmap();
            }
            string[] parts = id.Split('_');
            if (parts.Length == 5 && parts[0] == "pet")
            {
                int frame;
                if (!int.TryParse(parts[4], out frame)) frame = 0;
                G1C_PixelCanvas c = parts[1] == "dog" ? Dog(parts[2], frame) : parts[1] == "cat" ? Cat(parts[2], frame) : null;
                if (c == null) return null;
                return (parts[3] == "l" ? c.MirrorX() : c).ToBitmap();
            }
            if (parts.Length == 3 && parts[0] == "fish")
            {
                G1C_PixelCanvas c = Catch("fish_" + parts[1]);
                return c == null ? null : (parts[2] == "l" ? c.MirrorX() : c).ToBitmap();
            }
            return null;
        }

        private static Color Rgb(uint rgb)
        {
            return Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private static Color Shade(Color c, double f)
        {
            return Color.FromRgb((byte)Math.Min(255, c.R * f), (byte)Math.Min(255, c.G * f), (byte)Math.Min(255, c.B * f));
        }

        // Entwickler: Patrick Lupierz – Datum: 18.11.2026 – Fisch im Wasser: 35 % Wasserblau, Bild 1 versetzt
        private static G1C_PixelCanvas RiverFish(G1C_PixelCanvas fish, int frame)
        {
            Color water = Rgb(0x3878C8);
            G1C_PixelCanvas c = new G1C_PixelCanvas(fish.Width, fish.Height);
            int dx = frame == 1 ? 1 : 0, dy = frame == 1 ? 1 : 0;
            for (int y = 0; y < fish.Height; y++)
                for (int x = 0; x < fish.Width; x++)
                {
                    Color p = fish.Get(x, y);
                    if (p.A == 0) continue;
                    c.Set(x + dx, y + dy, Color.FromRgb((byte)(p.R * 0.65 + water.R * 0.35), (byte)(p.G * 0.65 + water.G * 0.35),
                                                        (byte)(p.B * 0.65 + water.B * 0.35)));
                }
            return c;
        }

        // ------------------------------------------------------------------ Katzen

        // Entwickler: Patrick Lupierz – Seitenansicht nach rechts; Fellmuster je Variante
        public static G1C_PixelCanvas Cat(string variant, int frame)
        {
            Color fur, mark, eye = Rgb(0x283828), inner = Rgb(0xF0A0A8);
            switch (variant)
            {
                case "calico": fur = Rgb(0xF4F0E8); mark = Rgb(0xE08838); break;
                case "orange": fur = Rgb(0xEC9840); mark = Rgb(0xC06820); break;
                case "black": fur = Rgb(0x2C2C34); mark = Rgb(0x1C1C22); eye = Rgb(0xF0D040); break;
                case "white": fur = Rgb(0xF8F8F8); mark = Rgb(0xE0E0E8); eye = Rgb(0x4888E0); break;
                default: fur = Rgb(0x98A0A8); mark = Rgb(0x687078); break;   // grey (getigert)
            }
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 14);
            // Schwanz: im Stand nach oben gebogen, beim Laufen nach hinten gestreckt
            if (frame == 0) { c.Set(2, 8, fur); c.Set(1, 7, fur); c.Set(1, 6, fur); c.Set(1, 5, fur); c.Set(2, 4, fur); }
            else { c.Set(2, 8, fur); c.Set(1, 8, fur); c.Set(0, 7, fur); }
            // Beine (zwei Laufbilder)
            int[] legs = frame == 1 ? new[] { 3, 6, 8, 11 } : frame == 2 ? new[] { 5, 4, 10, 9 } : new[] { 4, 5, 9, 10 };
            foreach (int x in legs) c.FillRect(x, 10, 1, 3, Shade(fur, 0.85));
            c.FillEllipse(7, 8.5, 4.8, 2.6, fur);                 // Körper
            c.FillEllipse(12, 5.5, 2.6, 2.4, fur);                // Kopf
            c.Set(10, 2, fur); c.Set(10, 3, fur); c.Set(11, 3, fur); // Ohren
            c.Set(14, 2, fur); c.Set(14, 3, fur); c.Set(13, 3, fur);
            c.Set(10, 3, inner); c.Set(14, 3, inner);
            switch (variant)
            {
                case "calico":
                    c.FillEllipse(5, 7.5, 2.2, 1.5, mark);
                    c.FillRect(11, 3, 2, 2, Rgb(0x303030)); c.FillRect(8, 8, 2, 1, Rgb(0x303030));
                    break;
                case "white":
                    break;
                default:
                    foreach (int x in new[] { 5, 7, 9 }) c.FillRect(x, 6, 1, 2, mark);   // Tigerstreifen
                    c.Set(11, 4, mark); c.Set(12, 3, mark);
                    break;
            }
            c.FillRect(5, 10, 5, 1, Shade(fur, 1.08));            // heller Bauch
            c.Set(13, 5, eye);
            c.Set(15, 6, inner);                                  // Nase
            c.AddOutline(Outline);
            return c;
        }

        // ------------------------------------------------------------------ Hunde

        // Entwickler: Patrick Lupierz – Seitenansicht nach rechts mit Schnauze und Schlappohr
        public static G1C_PixelCanvas Dog(string variant, int frame)
        {
            Color fur, ear;
            switch (variant)
            {
                case "dalmatian": fur = Rgb(0xF8F8F4); ear = Rgb(0x282828); break;
                case "golden": fur = Rgb(0xE4B460); ear = Rgb(0xC08838); break;
                default: fur = Rgb(0x9A6030); ear = Rgb(0x6A3C18); break;   // brown
            }
            G1C_PixelCanvas c = new G1C_PixelCanvas(18, 15);
            // Schwanz wedelt (Stand: oben, Laufen: schräg)
            if (frame == 0) { c.Set(2, 7, fur); c.Set(1, 6, fur); c.Set(1, 5, fur); c.Set(1, 4, fur); }
            else { c.Set(2, 7, fur); c.Set(1, 6, fur); c.Set(0, 5, fur); }
            int[] legs = frame == 1 ? new[] { 3, 6, 10, 13 } : frame == 2 ? new[] { 5, 4, 12, 11 } : new[] { 4, 6, 10, 12 };
            foreach (int x in legs) c.FillRect(x, 10, 2, 4, Shade(fur, 0.88));
            c.FillEllipse(8, 8.5, 5.6, 3.0, fur);                 // Körper
            c.FillEllipse(13.5, 5, 2.8, 2.6, fur);                // Kopf
            c.FillRect(15, 5, 3, 3, fur);                         // Schnauze
            c.Set(17, 5, Rgb(0x181818));                          // Nase
            c.FillRect(11, 3, 2, 5, ear);                         // Schlappohr
            if (variant == "dalmatian")
                foreach (int[] spot in new[] { new[] { 5, 7 }, new[] { 8, 9 }, new[] { 10, 6 }, new[] { 4, 10 }, new[] { 14, 7 } })
                    c.Set(spot[0], spot[1], Rgb(0x202020));
            c.FillRect(6, 10, 5, 1, Shade(fur, 1.06));            // Bauch
            c.Set(14, 4, Rgb(0x201810));                          // Auge
            c.Set(15, 7, Rgb(0xE85060));                          // Zunge
            c.AddOutline(Outline);
            return c;
        }

        // ------------------------------------------------------------------ Angeln

        // Entwickler: Patrick Lupierz – Fische und Fundstücke (auch Inventar-Icons, siehe G1C_BuiltInSprites.ItemIcon)
        public static G1C_PixelCanvas Catch(string itemId)
        {
            // Änderung durch: Patrick Lupierz – 16.11.2026 – Fische aus dem frei nutzbaren Grafikpaket (G1C_FishSprites)
            G1C_PixelCanvas fish = G1C_FishSprites.Get(itemId);
            if (fish != null) return fish;
            switch (itemId)
            {
                case "boot": return Boot();
                case "treasure_ring": return Ring();
                default: return null;
            }
        }

        private static G1C_PixelCanvas Boot()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            Color leather = Rgb(0x6A4828);
            c.FillRect(4, 2, 5, 9, leather);
            c.FillRect(4, 10, 10, 4, leather);
            c.FillRect(4, 14, 11, 1, Rgb(0x302018));
            c.FillRect(5, 4, 3, 1, Rgb(0x8A6840)); c.FillRect(5, 7, 3, 1, Rgb(0x8A6840));
            c.Set(12, 11, Rgb(0x58B8F0));                                                           // Wassertropfen
            c.AddOutline(Outline);
            return c;
        }

        private static G1C_PixelCanvas Ring()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillEllipse(8, 10, 5, 4, Rgb(0xE8C040));
            c.ClearEllipse(8, 10, 3, 2.2);
            c.FillRect(6, 2, 4, 4, Rgb(0x60D8F8));                                                  // Edelstein
            c.Set(7, 3, Rgb(0xE8FFFF));
            c.AddOutline(Outline);
            return c;
        }

        // Entwickler: Patrick Lupierz – Angel lehnt am Geländer, daneben ein Ködereimer
        private static G1C_PixelCanvas FishingRod()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 32);
            Color wood = Rgb(0x8A5A30);
            for (int i = 0; i < 28; i++) c.Set(4 + i * 11 / 28, 30 - i, wood);                       // Rute
            for (int i = 0; i < 20; i++) c.Set(5 + i * 11 / 28, 30 - i, Shade(wood, 0.75));
            c.FillRect(6, 22, 3, 3, Rgb(0x404850));                                                  // Rolle
            for (int y = 3; y < 14; y++) c.Set(15, y, Rgb(0xE0E0E8));                                // Schnur
            c.FillRect(14, 14, 2, 2, Rgb(0xE83030));                                                 // Pose
            c.FillRect(0, 25, 6, 6, Rgb(0x5878A8));                                                  // Eimer
            c.FillRect(0, 25, 6, 1, Rgb(0x88A8D0));
            c.AddOutline(Outline);
            return c;
        }

        // Entwickler: Patrick Lupierz – graue Steinplatten (versetzte Fugen, leichte Sprenkel)
        private static G1C_PixelCanvas Plaza()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            Color stone = Rgb(0xB8B8B4), joint = Rgb(0x8C8C88), light = Rgb(0xCCCCC8);
            c.FillRect(0, 0, 16, 16, stone);
            c.FillRect(0, 7, 16, 1, joint); c.FillRect(0, 15, 16, 1, joint);
            c.FillRect(7, 0, 1, 7, joint); c.FillRect(15, 0, 1, 7, joint);
            c.FillRect(3, 8, 1, 7, joint); c.FillRect(11, 8, 1, 7, joint);
            c.FillRect(0, 0, 7, 1, light); c.FillRect(8, 0, 7, 1, light); c.FillRect(4, 8, 7, 1, light);
            c.Set(3, 3, joint); c.Set(12, 4, joint); c.Set(7, 11, joint); c.Set(14, 12, joint);
            return c;
        }
    }
}
