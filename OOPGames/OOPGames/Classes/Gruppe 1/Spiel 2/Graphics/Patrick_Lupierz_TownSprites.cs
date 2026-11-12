using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_TownSprites
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Überarbeitete Stadtgrafik im hellen, freundlichen Stil moderner Monster-RPGs:
    //   kräftiges Gras, sandige Wege und helle Pflasterstraßen mit weichen Rändern
    //   (Randgrafik je nach Nachbarkacheln, ID-Endung = Bitmaske N=1, O=2, S=4, W=8),
    //   Fluss mit Uferkante, größere Gebäude (96x80 Pixel für 6x5 Kacheln) mit Tür in der Front,
    //   sowie Laterne, Mülleimer, Kleiderschrank, Spiegel und Kleiderständer.
    //   Steht in der Sprite-Kette vor G1C_WorldSprites und ersetzt dort gleichnamige IDs;
    //   jede ID bleibt weiterhin über eine PNG-Datei austauschbar.
    // =====================================================
    public static class G1C_TownSprites
    {
        private static readonly Color Grass = Rgb(0x78C850);
        private static readonly Color GrassLight = Rgb(0x98E068);
        private static readonly Color GrassDark = Rgb(0x58A838);
        private static readonly Color Outline = Rgb(0x303040);
        private static readonly Color Gold = Rgb(0xF8D048);

        // Entwickler: Patrick Lupierz
        public static BitmapSource Create(string id)
        {
            int mask;
            if (TryMask(id, "tile_city_road_", out mask)) return Surface(mask, Rgb(0xDCD4C4), Rgb(0xC0B8A8), Rgb(0x988F80), true);
            if (TryMask(id, "tile_city_path_", out mask)) return Surface(mask, Rgb(0xE8D8A0), Rgb(0xD4C088), Rgb(0xB89C60), false);
            if (TryMask(id, "tile_city_water_", out mask)) return Water(mask);

            switch (id)
            {
                case "tile_city_grass": return GrassTile(0);
                case "tile_city_grass_alt": return GrassTile(1);
                case "tile_city_park": return GrassTile(2);
                case "tile_city_flowers": return GrassTile(3);
                case "tile_city_forest": return Forest();
                case "tile_city_bridge": return Bridge();
                case "tile_gas_floor": return Floor(Rgb(0xD8DCE0));
                case "tile_gas_wall": return Wall(Rgb(0xF08030));
                case "tile_clothing_floor": return Floor(Rgb(0xF0D8C0));
                case "tile_clothing_wall": return Wall(Rgb(0xE870A0));
                case "building_home": return Building(Rgb(0xE05040), Rgb(0xF8F0D8), Rgb(0xE05040), "");
                case "building_hospital": return Building(Rgb(0x58B8D8), Rgb(0xFFFFFF), Rgb(0xFFFFFF), "cross");
                case "building_market": return Building(Rgb(0x3890E0), Rgb(0xF8F8F8), Rgb(0x3890E0), "bag");
                case "building_casino": return Building(Rgb(0x8040B0), Rgb(0xF8E8B0), Rgb(0x402060), "chip");
                case "building_gas_station": return Building(Rgb(0xF08030), Rgb(0xE8E8E8), Rgb(0x303848), "drop");
                case "building_clothing": return Building(Rgb(0xE870A0), Rgb(0xFFF4E0), Rgb(0x804060), "shirt");
                case "building_library": return Building(Rgb(0x5868A8), Rgb(0xE8DCC0), Rgb(0x5868A8), "");
                case "building_townhouse": return Building(Rgb(0xC86848), Rgb(0xF0E0B8), Rgb(0xC86848), "");
                case "building_cafe": return Building(Rgb(0xA06840), Rgb(0xF8E8C0), Rgb(0x603820), "bag");
                case "building_flower_shop": return Building(Rgb(0x78B858), Rgb(0xFFF0F4), Rgb(0xE870A0), "");
                case "building_fire_station": return Building(Rgb(0xD03838), Rgb(0xE8E0D0), Rgb(0xD03838), "drop");
                case "building_cinema": return Building(Rgb(0x484868), Rgb(0xD8C8B0), Rgb(0xD03848), "chip");
                case "building_warehouse": return Building(Rgb(0x788888), Rgb(0xC8C0A8), Rgb(0x788888), "");
                case "building_post": return Building(Rgb(0xE8B838), Rgb(0xF8F0D8), Rgb(0x3060A8), "bag");
                case "obj_city_tree": return Tree();
                case "obj_street_lamp": return Lamp();
                case "obj_trash": return TrashCan();
                case "obj_wardrobe": return Wardrobe();
                case "obj_mirror": return Mirror();
                case "obj_clothes_rack": return ClothesRack();
                default: return null;
            }
        }

        private static bool TryMask(string id, string prefix, out int mask)
        {
            mask = 0;
            return id.StartsWith(prefix, StringComparison.Ordinal) &&
                   int.TryParse(id.Substring(prefix.Length), out mask) && mask >= 0 && mask <= 15;
        }

        // ------------------------------------------------------------------ Bodenkacheln

        // variant: 0 = Gras, 1 = Gras mit Büscheln, 2 = Parkwiese, 3 = Blumen
        private static BitmapSource GrassTile(int variant)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, variant == 2 ? Rgb(0x88D860) : Grass);
            c.Set(3, 4, GrassLight); c.Set(11, 10, GrassLight); c.Set(7, 13, GrassDark);
            if (variant == 1)
            {
                foreach (int[] tuft in new[] { new[] { 4, 9 }, new[] { 11, 4 } })
                {
                    c.Set(tuft[0], tuft[1], GrassDark); c.Set(tuft[0] + 1, tuft[1] - 1, GrassDark); c.Set(tuft[0] + 2, tuft[1], GrassDark);
                }
            }
            if (variant == 3)
            {
                Flower(c, 3, 3, Rgb(0xF85870)); Flower(c, 10, 6, Rgb(0xF8F8F8)); Flower(c, 5, 11, Rgb(0xF8D048));
            }
            return c.ToBitmap();
        }

        private static void Flower(G1C_PixelCanvas c, int x, int y, Color petal)
        {
            c.Set(x, y - 1, petal); c.Set(x - 1, y, petal); c.Set(x + 1, y, petal); c.Set(x, y + 1, petal);
            c.Set(x, y, Rgb(0xF8A030));
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Weg/Straße; an Seiten ohne Anschluss wächst Gras herein, Außenecken werden abgerundet.
        private static BitmapSource Surface(int mask, Color baseColor, Color detail, Color edge, bool paved)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, baseColor);
            if (paved)
            {
                // Pflastersteine im Versatz
                c.FillRect(0, 7, 16, 1, detail); c.FillRect(0, 15, 16, 1, detail);
                c.FillRect(4, 0, 1, 7, detail); c.FillRect(12, 8, 1, 7, detail);
            }
            else
            {
                c.Set(4, 5, detail); c.Set(11, 3, detail); c.Set(8, 11, detail); c.Set(13, 13, detail);
            }
            Edges(c, mask, Grass, edge);
            return c.ToBitmap();
        }

        private static BitmapSource Water(int mask)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, Rgb(0x4898E8));
            c.FillRect(2, 5, 5, 1, Rgb(0x78C0F8)); c.FillRect(9, 11, 5, 1, Rgb(0x78C0F8));
            c.FillRect(10, 3, 3, 1, Rgb(0x3078C8)); c.FillRect(3, 13, 3, 1, Rgb(0x3078C8));
            Edges(c, mask, Grass, Rgb(0x488830));
            return c.ToBitmap();
        }

        private static void Edges(G1C_PixelCanvas c, int mask, Color outside, Color edge)
        {
            bool n = (mask & 1) == 0, e = (mask & 2) == 0, s = (mask & 4) == 0, w = (mask & 8) == 0;
            if (n) { c.FillRect(0, 0, 16, 2, outside); c.FillRect(0, 2, 16, 1, edge); }
            if (s) { c.FillRect(0, 14, 16, 2, outside); c.FillRect(0, 13, 16, 1, edge); }
            if (w) { c.FillRect(0, 0, 2, 16, outside); c.FillRect(2, n ? 2 : 0, 1, 16, edge); }
            if (e) { c.FillRect(14, 0, 2, 16, outside); c.FillRect(13, n ? 2 : 0, 1, 16, edge); }
            if (n) { c.FillRect(0, 0, 16, 2, outside); }
            if (s) { c.FillRect(0, 14, 16, 2, outside); if (w) c.Set(2, 13, edge); if (e) c.Set(13, 13, edge); }
            // abgerundete Außenecken
            if (n && w) { c.FillRect(0, 0, 4, 3, outside); c.FillRect(0, 0, 3, 4, outside); c.Set(3, 3, edge); }
            if (n && e) { c.FillRect(12, 0, 4, 3, outside); c.FillRect(13, 0, 3, 4, outside); c.Set(12, 3, edge); }
            if (s && w) { c.FillRect(0, 13, 4, 3, outside); c.FillRect(0, 12, 3, 4, outside); c.Set(3, 12, edge); }
            if (s && e) { c.FillRect(12, 13, 4, 3, outside); c.FillRect(13, 12, 3, 4, outside); c.Set(12, 12, edge); }
        }

        private static BitmapSource Forest()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, Rgb(0x2C6E3C));
            c.FillEllipse(4, 5, 5, 5, Rgb(0x3C8C48)); c.FillEllipse(12, 4, 5, 4.5, Rgb(0x3C8C48));
            c.FillEllipse(8, 12, 6, 4.5, Rgb(0x348040));
            c.FillEllipse(3, 3, 2, 1.5, Rgb(0x58B058)); c.FillEllipse(11, 2.5, 2, 1.5, Rgb(0x58B058));
            c.FillEllipse(7, 10, 2.5, 1.5, Rgb(0x58B058));
            return c.ToBitmap();
        }

        private static BitmapSource Bridge()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, Rgb(0xC89858));
            for (int x = 3; x < 16; x += 4) c.FillRect(x, 0, 1, 16, Rgb(0x9C7038));
            c.FillRect(0, 0, 16, 1, Rgb(0xE0B878));
            c.Set(1, 4, Rgb(0x805828)); c.Set(9, 11, Rgb(0x805828));
            return c.ToBitmap();
        }

        private static BitmapSource Floor(Color color)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, color);
            c.FillRect(0, 15, 16, 1, Shade(color, 0.88)); c.FillRect(15, 0, 1, 16, Shade(color, 0.88));
            c.Set(3, 3, Shade(color, 1.06));
            return c.ToBitmap();
        }

        private static BitmapSource Wall(Color color)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, Rgb(0xF4F0E8));
            c.FillRect(0, 0, 16, 5, color);
            c.FillRect(0, 5, 16, 1, Shade(color, 0.7));
            c.FillRect(0, 13, 16, 3, Shade(color, 0.6));
            return c.ToBitmap();
        }

        // ------------------------------------------------------------------ Gebäude

        // Entwickler: Patrick Lupierz
        // Aufgabe: Haus mit schrägem Dach, Fenstern, Tür in Kachelspalte 3 (dort betritt der Spieler es) und Schild.
        private static BitmapSource Building(Color roof, Color wall, Color sign, string icon)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(96, 80);
            for (int y = 2; y < 38; y++)
            {
                int inset = y < 12 ? 12 - y : 0;
                Color row = y < 6 ? Shade(roof, 1.25) : (y % 6 == 3 ? Shade(roof, 0.85) : roof);
                c.FillRect(1 + inset, y, 94 - 2 * inset, 1, row);
            }
            c.FillRect(1, 36, 94, 1, Shade(roof, 1.2));
            c.FillRect(1, 37, 94, 3, Shade(roof, 0.65));

            c.FillRect(4, 40, 88, 40, wall);
            c.FillRect(4, 40, 88, 3, Shade(wall, 0.8));
            c.FillRect(4, 74, 88, 6, Shade(wall, 0.7));

            Window(c, 10, 50); Window(c, 28, 50); Window(c, 70, 50);

            c.FillRect(47, 54, 18, 26, Outline);
            c.FillRect(49, 56, 14, 24, Rgb(0x8A5A30));
            c.FillRect(50, 57, 12, 8, Rgb(0x98D0F0));
            c.FillRect(50, 57, 12, 1, Rgb(0xD8F0FF));
            c.FillRect(60, 70, 2, 2, Gold);

            if (!string.IsNullOrEmpty(icon))
            {
                c.FillRect(44, 42, 24, 11, Outline);
                c.FillRect(45, 43, 22, 9, sign);
                Icon(c, icon, 52, 44, sign);
            }
            if (icon == "chip")
            {
                // Lichterkette an der Traufe
                for (int x = 5; x < 92; x += 6) c.FillRect(x, 37, 2, 2, Gold);
            }
            c.AddOutline(Outline);
            return c.ToBitmap();
        }

        private static void Window(G1C_PixelCanvas c, int x, int y)
        {
            c.FillRect(x, y, 14, 14, Outline);
            c.FillRect(x + 1, y + 1, 12, 12, Rgb(0x88C8F0));
            c.FillRect(x + 1, y + 1, 12, 2, Rgb(0xC8E8FF));
            c.FillRect(x + 6, y + 1, 2, 12, Rgb(0xF8F8F8));
            c.FillRect(x - 1, y + 14, 16, 2, Rgb(0xB8B0A0));
        }

        private static void Icon(G1C_PixelCanvas c, string icon, int x, int y, Color sign)
        {
            Color white = Rgb(0xF8F8F8);
            switch (icon)
            {
                case "cross":
                    c.FillRect(x + 3, y, 2, 7, Rgb(0xE03040)); c.FillRect(x, y + 2, 8, 3, Rgb(0xE03040));
                    break;
                case "bag":
                    c.FillRect(x + 1, y + 2, 6, 5, white); c.FillRect(x + 2, y, 4, 1, white);
                    c.Set(x + 2, y + 1, white); c.Set(x + 5, y + 1, white);
                    break;
                case "chip":
                    c.FillEllipse(x + 4, y + 3.5, 4, 3.5, Gold); c.FillRect(x + 3, y + 2, 2, 3, sign);
                    break;
                case "drop":
                    c.FillRect(x + 3, y, 2, 2, white); c.FillRect(x + 2, y + 2, 4, 2, white); c.FillRect(x + 1, y + 4, 6, 3, white);
                    break;
                case "shirt":
                    c.FillRect(x + 2, y + 1, 4, 6, white); c.FillRect(x, y + 1, 2, 2, white); c.FillRect(x + 6, y + 1, 2, 2, white);
                    break;
            }
        }

        // ------------------------------------------------------------------ Objekte

        private static BitmapSource Tree()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 30);
            c.FillRect(6, 20, 4, 9, Rgb(0x8A5A30));
            c.FillRect(6, 20, 1, 9, Rgb(0x6A4020));
            c.FillEllipse(8, 11, 7.5, 10, Rgb(0x2C8040));
            c.FillEllipse(8, 9, 6.5, 8, Rgb(0x48A850));
            c.FillEllipse(6, 6, 3.5, 3.5, Rgb(0x78D068));
            c.FillEllipse(11, 13, 2.5, 2, Rgb(0x2C8040));
            c.AddOutline(Rgb(0x204830));
            return c.ToBitmap();
        }

        private static BitmapSource Lamp()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 32);
            c.FillRect(7, 10, 2, 19, Rgb(0x505868));
            c.FillRect(5, 29, 6, 2, Rgb(0x404858));
            c.FillRect(4, 3, 8, 7, Rgb(0x404858));
            c.FillRect(5, 4, 6, 5, Rgb(0xF8E890));
            c.FillRect(3, 2, 10, 1, Rgb(0x404858));
            c.AddOutline(Outline);
            return c.ToBitmap();
        }

        private static BitmapSource TrashCan()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 20);
            c.FillRect(3, 7, 10, 12, Rgb(0x6A9A80));
            c.FillRect(3, 7, 2, 12, Rgb(0x88B898));
            c.FillRect(6, 9, 1, 8, Rgb(0x4A7860)); c.FillRect(9, 9, 1, 8, Rgb(0x4A7860));
            c.FillRect(2, 4, 12, 3, Rgb(0x4A7860));
            c.FillRect(6, 2, 4, 2, Rgb(0x4A7860));
            c.AddOutline(Outline);
            return c.ToBitmap();
        }

        private static BitmapSource Wardrobe()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 28);
            c.FillRect(1, 1, 14, 26, Rgb(0xA06838));
            c.FillRect(1, 1, 14, 2, Rgb(0xC08850));
            c.FillRect(7, 3, 2, 22, Rgb(0x704820));
            c.FillRect(5, 13, 2, 2, Gold); c.FillRect(9, 13, 2, 2, Gold);
            c.FillRect(1, 25, 14, 2, Rgb(0x704820));
            c.AddOutline(Outline);
            return c.ToBitmap();
        }

        private static BitmapSource Mirror()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 28);
            c.FillRect(3, 1, 10, 20, Gold);
            c.FillRect(4, 2, 8, 18, Rgb(0xB8E0F8));
            c.FillRect(5, 3, 2, 6, Rgb(0xF8F8F8)); c.Set(8, 4, Rgb(0xF8F8F8));
            c.FillRect(7, 21, 2, 4, Rgb(0x8A5A30));
            c.FillRect(4, 25, 8, 2, Rgb(0x8A5A30));
            c.AddOutline(Outline);
            return c.ToBitmap();
        }

        private static BitmapSource ClothesRack()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(32, 24);
            c.FillRect(2, 3, 28, 1, Rgb(0x909098));
            c.FillRect(2, 3, 1, 20, Rgb(0x909098)); c.FillRect(29, 3, 1, 20, Rgb(0x909098));
            Color[] shirts = { Rgb(0xD83030), Rgb(0x3060C8), Rgb(0x40A060), Rgb(0xF8D048) };
            for (int i = 0; i < shirts.Length; i++)
            {
                int x = 5 + i * 6;
                c.FillRect(x, 5, 5, 11, shirts[i]);
                c.FillRect(x, 5, 5, 2, Shade(shirts[i], 1.2));
                c.Set(x + 2, 4, Rgb(0x505058));
            }
            c.AddOutline(Outline);
            return c.ToBitmap();
        }

        // ------------------------------------------------------------------ Farben

        private static Color Rgb(uint rgb)
        {
            return Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private static Color Shade(Color c, double factor)
        {
            return Color.FromRgb((byte)Math.Min(255, c.R * factor), (byte)Math.Min(255, c.G * factor), (byte)Math.Min(255, c.B * factor));
        }
    }
}
