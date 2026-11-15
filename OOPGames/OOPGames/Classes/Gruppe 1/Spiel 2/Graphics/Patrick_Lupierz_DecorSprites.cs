using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_DecorSprites
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Eingebaute Pixel-Art für Schilder und Wanddekoration. Alle IDs sind wie jede andere
    //               Grafik über "<Sprite-ID>.png" im Ordner G1Casino_Assets austauschbar:
    //               sign_closed (CLOSED-Schild an Ladentüren), obj_poster_ad (großes Werbeposter),
    //               obj_sticker_bw (kleiner Sticker), obj_sign_post (Holzschild in der Stadt).
    // =====================================================
    public static class G1C_DecorSprites
    {
        // 3x5-Pixelschrift für die Schildbeschriftung (nur benötigte Buchstaben)
        private static readonly Dictionary<char, string[]> Font = new Dictionary<char, string[]>
        {
            { 'C', new[] { "111", "100", "100", "100", "111" } },
            { 'L', new[] { "100", "100", "100", "100", "111" } },
            { 'O', new[] { "111", "101", "101", "101", "111" } },
            { 'S', new[] { "111", "100", "111", "001", "111" } },
            { 'E', new[] { "111", "100", "110", "100", "111" } },
            { 'D', new[] { "110", "101", "101", "101", "110" } },
        };

        private static readonly Color Outline = Color.FromRgb(0x30, 0x20, 0x28);

        // Entwickler: Patrick Lupierz
        public static BitmapSource Create(string id)
        {
            switch (id)
            {
                case "sign_closed": return ClosedSign().ToBitmap();
                case "obj_poster_ad": return Poster().ToBitmap();
                case "obj_sticker_bw": return Sticker().ToBitmap();
                case "obj_sign_post": return SignPost().ToBitmap();
                // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Fahrrad, Spielhalle, Innenräume
                case "obj_bike_stand": return BikeStand().ToBitmap();
                case "obj_highscore_board": return HighscoreBoard().ToBitmap();
                case "obj_neon_sign": return NeonSign().ToBitmap();
                case "tile_arcade_floor": return ArcadeFloor().ToBitmap();
                case "tile_arcade_wall": return ArcadeWall().ToBitmap();
                case "tile_bike_floor": return PlainFloor(Color.FromRgb(0xC8, 0xC8, 0xC0), Color.FromRgb(0xB0, 0xB0, 0xA8)).ToBitmap();
                case "tile_bike_wall": return TwoToneWall(Color.FromRgb(0x38, 0x98, 0x58)).ToBitmap();
            }
            // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Seitenwände, Teppichteile, Bilder, Tankstelle, Omas Haus
            switch (id)
            {
                case "obj_fuel_pump": return FuelPump().ToBitmap();
                case "obj_sales_window": return SalesWindow().ToBitmap();
                case "tile_grandma_floor": return GrandmaFloor().ToBitmap();
                case "tile_grandma_wall": return GrandmaWall().ToBitmap();
            }
            if (id.StartsWith("tile_wallside_", System.StringComparison.Ordinal))
            {
                string[] parts = id.Substring("tile_wallside_".Length).Split('_');   // <set>_<l|r|b>
                return WallSide(parts[0], parts.Length > 1 ? parts[1] : "l").ToBitmap();
            }
            if (id.StartsWith("tile_rug_", System.StringComparison.Ordinal))
            {
                string[] parts = id.Substring("tile_rug_".Length).Split('_');        // <set>_<maske>
                int mask;
                return RugPiece(parts[0], parts.Length > 1 && int.TryParse(parts[1], out mask) ? mask : 15).ToBitmap();
            }
            if (id.StartsWith("obj_picture_", System.StringComparison.Ordinal)) return Picture(id.Substring("obj_picture_".Length)).ToBitmap();
            if (id.StartsWith("obj_bike_", System.StringComparison.Ordinal)) return BikeSide(id.Substring(4)).ToBitmap();
            if (id.StartsWith("ride_", System.StringComparison.Ordinal) && id.EndsWith("_side", System.StringComparison.Ordinal))
                return BikeSide(id.Substring(5, id.Length - 10)).ToBitmap();
            if (id.StartsWith("ride_", System.StringComparison.Ordinal) && id.EndsWith("_front", System.StringComparison.Ordinal))
                return BikeFront(id.Substring(5, id.Length - 11)).ToBitmap();
            if (id.StartsWith("obj_arcade_cabinet_", System.StringComparison.Ordinal))
                return ArcadeCabinet(id.Substring("obj_arcade_cabinet_".Length)).ToBitmap();
            return null;
        }

        // ------------------------------------------------------------------ Fahrräder

        // Entwickler: Patrick Lupierz – Rahmenfarbe aus dem Gegenstandskatalog (eine Quelle für Shop, Icon und Grafik)
        private static Color BikeColor(string bikeId)
        {
            G1C_Item item = G1C_ItemCatalog.Get(bikeId);
            uint rgb = item != null ? item.Color : 0xD83030;
            return Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private static Color Darker(Color c, double f)
        {
            return Color.FromRgb((byte)(c.R * f), (byte)(c.G * f), (byte)(c.B * f));
        }

        // Entwickler: Patrick Lupierz – Seitenansicht: zwei Speichenräder, Rahmen, Sattel, Lenker
        public static G1C_PixelCanvas BikeSide(string bikeId)
        {
            Color frame = BikeColor(bikeId), tire = Color.FromRgb(0x28, 0x28, 0x30), metal = Color.FromRgb(0xB8, 0xC0, 0xC8);
            G1C_PixelCanvas c = new G1C_PixelCanvas(22, 14);
            foreach (int cx in new[] { 5, 16 })
            {
                c.FillEllipse(cx, 9, 4.5, 4.5, tire);
                c.ClearEllipse(cx, 9, 3, 3);
                c.Set(cx, 9, metal); c.Set(cx - 1, 9, metal); c.Set(cx + 1, 9, metal); c.Set(cx, 8, metal); c.Set(cx, 10, metal);
            }
            for (int i = 0; i < 6; i++) { c.Set(6 + i, 8 - i / 2, frame); c.Set(6 + i, 9 - i / 2, frame); }     // Unterrohr
            c.FillRect(9, 5, 6, 1, frame);                                                                   // Oberrohr
            for (int i = 0; i < 4; i++) c.Set(15 + i / 3, 5 + i, frame);                                    // Gabel
            c.FillRect(8, 3, 4, 2, Color.FromRgb(0x30, 0x28, 0x28));                                        // Sattel
            c.FillRect(9, 5, 1, 4, Darker(frame, 0.7));                                                      // Sattelrohr
            c.FillRect(15, 2, 4, 1, metal); c.Set(15, 3, metal);                                            // Lenker
            c.AddOutline(Outline);
            return c;
        }

        // Entwickler: Patrick Lupierz – Ansicht von vorn/hinten (beim Fahren nach oben/unten)
        public static G1C_PixelCanvas BikeFront(string bikeId)
        {
            Color frame = BikeColor(bikeId);
            G1C_PixelCanvas c = new G1C_PixelCanvas(12, 14);
            c.FillRect(5, 6, 2, 8, Color.FromRgb(0x28, 0x28, 0x30));     // Reifen
            c.FillRect(5, 3, 2, 4, frame);                               // Gabel/Rahmen
            c.FillRect(1, 3, 10, 1, Color.FromRgb(0xB8, 0xC0, 0xC8));    // Lenker
            c.FillRect(0, 3, 2, 2, Color.FromRgb(0x30, 0x28, 0x28)); c.FillRect(10, 3, 2, 2, Color.FromRgb(0x30, 0x28, 0x28));
            c.FillRect(5, 1, 2, 2, Color.FromRgb(0xF8, 0xE8, 0x80));     // Lampe
            c.AddOutline(Outline);
            return c;
        }

        private static G1C_PixelCanvas BikeStand()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 12);
            Color metal = Color.FromRgb(0x90, 0x98, 0xA0);
            c.FillRect(2, 4, 12, 1, metal);
            foreach (int x in new[] { 3, 7, 11 }) c.FillRect(x, 4, 1, 7, metal);
            c.FillRect(1, 11, 14, 1, Color.FromRgb(0x60, 0x60, 0x68));
            c.AddOutline(Outline);
            return c;
        }

        // ------------------------------------------------------------------ Spielhalle

        // Entwickler: Patrick Lupierz – Automat mit Marquee und kleinem Bildschirm; jedes Motiv zeigt "sein" Spiel
        private static G1C_PixelCanvas ArcadeCabinet(string motif)
        {
            Color body, accent;
            switch (motif)
            {
                case "stars": body = Color.FromRgb(0x30, 0x38, 0x98); accent = Color.FromRgb(0x50, 0xE0, 0xF8); break;
                case "retro": body = Color.FromRgb(0x98, 0x28, 0x38); accent = Color.FromRgb(0xF8, 0xD0, 0x40); break;
                case "duel": body = Color.FromRgb(0x28, 0x78, 0x48); accent = Color.FromRgb(0xA0, 0xF8, 0x80); break;
                case "guest": body = Color.FromRgb(0x18, 0x18, 0x20); accent = Color.FromRgb(0xF8, 0x60, 0xD0); break;
                default: body = Color.FromRgb(0x78, 0x30, 0x98); accent = Color.FromRgb(0xF8, 0x80, 0xF0); break;
            }
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 30);
            c.FillRect(1, 1, 14, 28, body);
            c.FillRect(1, 1, 14, 4, accent);                                   // Marquee (leuchtet)
            c.FillRect(2, 2, 12, 1, Color.FromRgb(0xFF, 0xFF, 0xFF));
            c.FillRect(2, 6, 12, 10, Color.FromRgb(0x08, 0x08, 0x10));         // Bildschirm
            Color s = accent;
            switch (motif)
            {
                case "stars": c.Set(5, 8, s); c.Set(9, 8, s); c.Set(7, 9, s); c.FillRect(6, 13, 3, 2, Color.FromRgb(0xF8, 0xF8, 0xF8)); break;
                case "retro": c.FillRect(3, 8, 10, 2, Color.FromRgb(0xE8, 0x50, 0x40)); c.FillRect(6, 14, 4, 1, s); c.Set(8, 12, s); break;
                case "duel": c.FillRect(3, 9, 1, 4, s); c.FillRect(12, 8, 1, 4, s); c.Set(7, 10, Color.FromRgb(0xF8, 0xF8, 0xF8)); break;
                case "guest": c.FillRect(4, 8, 8, 6, Color.FromRgb(0x30, 0x60, 0xC8)); c.Set(6, 10, s); c.Set(9, 11, Color.FromRgb(0xF8, 0xD0, 0x40)); break;
                default: c.FillRect(3, 9, 4, 1, s); c.FillRect(9, 12, 4, 1, Color.FromRgb(0x50, 0xE0, 0xF8)); break;
            }
            c.FillRect(1, 17, 14, 4, Darker(body, 0.7));                       // Bedienpult
            c.FillRect(4, 18, 2, 2, Color.FromRgb(0x20, 0x20, 0x20));          // Joystick
            c.Set(10, 18, Color.FromRgb(0xE8, 0x40, 0x40)); c.Set(12, 19, Color.FromRgb(0x40, 0x90, 0xE8));
            c.FillRect(6, 23, 4, 2, Color.FromRgb(0xC0, 0xA0, 0x40));          // Münzschlitz
            c.FillRect(2, 29, 12, 1, Color.FromArgb(0x60, 0, 0, 0));
            c.AddOutline(Outline);
            return c;
        }

        private static G1C_PixelCanvas HighscoreBoard()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 14);
            c.FillRect(1, 1, 14, 12, Color.FromRgb(0x10, 0x10, 0x18));
            c.DrawRectBorder(0, 0, 16, 14, Color.FromRgb(0xF8, 0x60, 0xD0));
            for (int y = 3; y <= 10; y += 3) c.FillRect(3, y, y == 3 ? 10 : 7, 1, Color.FromRgb(0x50, 0xE0, 0xF8));
            return c;
        }

        private static G1C_PixelCanvas NeonSign()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 10);
            c.FillRect(1, 2, 14, 6, Color.FromRgb(0x20, 0x10, 0x28));
            c.FillRect(2, 3, 5, 1, Color.FromRgb(0xF8, 0x60, 0xD0)); c.FillRect(2, 6, 5, 1, Color.FromRgb(0xF8, 0x60, 0xD0));
            c.FillRect(9, 3, 1, 4, Color.FromRgb(0x50, 0xE0, 0xF8)); c.FillRect(12, 3, 1, 4, Color.FromRgb(0x50, 0xE0, 0xF8));
            c.AddOutline(Outline);
            return c;
        }

        private static G1C_PixelCanvas ArcadeFloor()
        {
            G1C_PixelCanvas c = PlainFloor(Color.FromRgb(0x20, 0x18, 0x30), Color.FromRgb(0x18, 0x10, 0x24));
            c.Set(4, 5, Color.FromRgb(0xF8, 0x60, 0xD0)); c.Set(11, 12, Color.FromRgb(0x50, 0xE0, 0xF8)); c.Set(13, 3, Color.FromRgb(0xF8, 0xD0, 0x40));
            return c;
        }

        private static G1C_PixelCanvas ArcadeWall()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, Color.FromRgb(0x18, 0x14, 0x28));
            c.FillRect(0, 6, 16, 1, Color.FromRgb(0xF8, 0x60, 0xD0));         // Neonband
            c.FillRect(0, 9, 16, 1, Color.FromRgb(0x50, 0xE0, 0xF8));
            c.FillRect(0, 13, 16, 3, Color.FromRgb(0x10, 0x0C, 0x18));
            return c;
        }

        // ------------------------------------------------------------------ Innenwände, Teppiche, Bilder

        // Entwickler: Patrick Lupierz – Akzentfarbe eines Raums = Farbe des oberen Wandbands (eine Quelle für alles)
        private static Color WallAccent(string set)
        {
            if (set == "home") return Color.FromRgb(0x98, 0x68, 0x48);
            if (set == "arcade") return Color.FromRgb(0x48, 0x30, 0x68);
            if (set == "grandma") return Color.FromRgb(0x98, 0x58, 0x70);
            if (set == "bike") return Color.FromRgb(0x38, 0x98, 0x58);
            BitmapSource wall = G1C_TownSprites.Create("tile_" + set + "_wall") ?? G1C_WorldSprites.Create("tile_" + set + "_wall");
            if (wall == null) return Color.FromRgb(0x78, 0x78, 0x88);
            FormatConvertedBitmap bgra = new FormatConvertedBitmap(wall, PixelFormats.Bgra32, null, 0);
            byte[] px = new byte[4];
            bgra.CopyPixels(new System.Windows.Int32Rect(bgra.PixelWidth / 2, 1, 1, 1), px, 4, 0);
            return Color.FromRgb(px[2], px[1], px[0]);
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Linke/rechte/untere Innenwand in Draufsicht (nur die Mauerkrone ist sichtbar) statt der
        //          bisher gedrehten Oberwand-Grafik. Innenkante hell, Mauerkrone dunkler mit Fugen.
        private static G1C_PixelCanvas WallSide(string set, string side)
        {
            Color accent = WallAccent(set), cap = Darker(accent, 0.55), edge = accent, light = Lighter(accent, 1.35);
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, cap);
            for (int y = 3; y < 16; y += 6) c.FillRect(0, y, 16, 1, Darker(cap, 0.8));
            if (side == "l") { c.FillRect(12, 0, 3, 16, edge); c.FillRect(15, 0, 1, 16, light); }
            else if (side == "r") { c.FillRect(1, 0, 3, 16, edge); c.FillRect(0, 0, 1, 16, light); }
            else { c.FillRect(0, 1, 16, 3, edge); c.FillRect(0, 0, 16, 1, light); }
            return c;
        }

        private static Color Lighter(Color c, double f)
        {
            return Color.FromRgb((byte)Math.Min(255, c.R * f + 20), (byte)Math.Min(255, c.G * f + 20), (byte)Math.Min(255, c.B * f + 20));
        }

        private static Color RugColor(string set)
        {
            switch (set)
            {
                case "hospital": return Color.FromRgb(0x48, 0x98, 0xA0);
                case "market": return Color.FromRgb(0x58, 0x90, 0x48);
                case "gas": return Color.FromRgb(0x70, 0x78, 0x88);
                case "clothing": return Color.FromRgb(0xB0, 0x50, 0x90);
                case "bank": return Color.FromRgb(0x98, 0x28, 0x30);
                case "furniture": return Color.FromRgb(0x98, 0x68, 0x38);
                case "bike": return Color.FromRgb(0x30, 0x70, 0x48);
                case "grandma": return Color.FromRgb(0xA8, 0x38, 0x48);
                default: return Color.FromRgb(0x38, 0x58, 0xA0);   // home
            }
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Ein Teppichteil. Maske = Seiten mit Teppich-Nachbar (N 1, O 2, S 4, W 8): an freien Seiten
        //          liegt die Bordüre mit Fransen, innen ein Rautenmuster – so wirkt der Teppich wie ein Stück.
        private static G1C_PixelCanvas RugPiece(string set, int mask)
        {
            Color main = RugColor(set), border = Color.FromRgb(0xE8, 0xD8, 0xA8), dark = Darker(main, 0.7);
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, main);
            for (int y = 0; y < 16; y++)
                for (int x = 0; x < 16; x++)
                    if ((x + y) % 8 == 0 || (x - y + 16) % 8 == 0) c.Set(x, y, dark);       // Rautenmuster
            if ((mask & 1) == 0) { c.FillRect(0, 2, 16, 2, border); for (int x = 1; x < 16; x += 2) c.Set(x, 0, border); }
            if ((mask & 4) == 0) { c.FillRect(0, 12, 16, 2, border); for (int x = 1; x < 16; x += 2) c.Set(x, 15, border); }
            if ((mask & 8) == 0) c.FillRect(1, 0, 2, 16, border);
            if ((mask & 2) == 0) c.FillRect(13, 0, 2, 16, border);
            return c;
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026 – gerahmte Bilder/Poster je Raum (Motiv im Namen)
        private static G1C_PixelCanvas Picture(string motif)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 14);
            Color frame = motif == "health" || motif == "interest" ? Color.FromRgb(0xE8, 0xE8, 0xE8) : Color.FromRgb(0x90, 0x60, 0x30);
            c.FillRect(0, 0, 16, 14, frame);
            c.FillRect(2, 2, 12, 10, Color.FromRgb(0xF0, 0xEC, 0xE0));
            switch (motif)
            {
                case "sea":
                    c.FillRect(2, 2, 12, 5, Color.FromRgb(0x88, 0xC8, 0xF0)); c.FillRect(2, 7, 12, 5, Color.FromRgb(0x30, 0x70, 0xC0));
                    c.FillEllipse(11, 4, 1.5, 1.5, Color.FromRgb(0xF8, 0xD8, 0x40)); break;
                case "mountain":
                    c.FillRect(2, 2, 12, 10, Color.FromRgb(0x98, 0xD0, 0xF0));
                    for (int i = 0; i < 6; i++) c.FillRect(4 + i / 2, 11 - i, 8 - i, 1, Color.FromRgb(0x58, 0x70, 0x60));
                    c.FillRect(6, 6, 3, 1, Color.FromRgb(0xF8, 0xF8, 0xF8)); break;
                case "cat":
                    c.FillRect(2, 2, 12, 10, Color.FromRgb(0xF0, 0xD8, 0xD0));
                    c.FillRect(5, 6, 6, 5, Color.FromRgb(0xA8, 0x88, 0x68)); c.FillRect(5, 4, 2, 2, Color.FromRgb(0xA8, 0x88, 0x68));
                    c.FillRect(9, 4, 2, 2, Color.FromRgb(0xA8, 0x88, 0x68)); c.Set(6, 7, Color.FromRgb(0x20, 0x20, 0x20)); c.Set(9, 7, Color.FromRgb(0x20, 0x20, 0x20)); break;
                case "health":
                    c.FillRect(6, 4, 4, 7, Color.FromRgb(0xE0, 0x30, 0x40)); c.FillRect(4, 6, 8, 3, Color.FromRgb(0xE0, 0x30, 0x40)); break;
                case "sale":
                    c.FillRect(2, 2, 12, 4, Color.FromRgb(0xE8, 0x40, 0x30)); c.FillEllipse(8, 9, 3, 2, Color.FromRgb(0xE8, 0xC0, 0x30)); break;
                case "fashion":
                    c.FillRect(5, 4, 6, 7, Color.FromRgb(0xE8, 0x70, 0xA0)); c.FillRect(3, 5, 2, 3, Color.FromRgb(0xE8, 0x70, 0xA0));
                    c.FillRect(11, 5, 2, 3, Color.FromRgb(0xE8, 0x70, 0xA0)); break;
                case "interest":
                    for (int i = 0; i < 4; i++) c.FillRect(3 + i * 3, 10 - i * 2, 2, 2 + i * 2, Color.FromRgb(0x38, 0xA0, 0x50)); break;
                case "bike":
                    c.FillRect(2, 2, 12, 10, Color.FromRgb(0xF8, 0xE8, 0x90));
                    c.FillEllipse(5, 9, 2, 2, Color.FromRgb(0x30, 0x30, 0x30)); c.FillEllipse(11, 9, 2, 2, Color.FromRgb(0x30, 0x30, 0x30));
                    c.FillRect(5, 6, 6, 1, Color.FromRgb(0x38, 0x98, 0x58)); break;
                default:
                    c.FillRect(2, 2, 12, 10, Color.FromRgb(0xC0, 0xD8, 0x98)); c.FillEllipse(8, 7, 3, 3, Color.FromRgb(0xE8, 0x80, 0x40)); break;
            }
            c.AddOutline(Outline);
            return c;
        }

        // ------------------------------------------------------------------ Tankstelle

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026 – rote Zapfsäule mit Anzeige, Schlauch und Zapfpistole
        private static G1C_PixelCanvas FuelPump()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 28);
            Color red = Color.FromRgb(0xD0, 0x28, 0x30);
            c.FillRect(3, 2, 10, 24, red);
            c.FillRect(3, 2, 10, 3, Color.FromRgb(0xF0, 0xF0, 0xF0));            // Kopf
            c.FillRect(5, 7, 6, 5, Color.FromRgb(0x18, 0x28, 0x20));              // Anzeige
            c.FillRect(6, 8, 4, 1, Color.FromRgb(0x60, 0xF0, 0x80)); c.FillRect(6, 10, 3, 1, Color.FromRgb(0x60, 0xF0, 0x80));
            c.FillRect(12, 14, 2, 6, Color.FromRgb(0x30, 0x30, 0x30));            // Zapfpistole
            c.FillRect(13, 6, 1, 8, Color.FromRgb(0x30, 0x30, 0x30));             // Schlauch
            c.FillRect(2, 26, 12, 2, Color.FromRgb(0x90, 0x90, 0x98));            // Sockel
            c.AddOutline(Outline);
            return c;
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026 – Verkaufsfenster (Nachtschalter) mit Ablage und Schild
        private static G1C_PixelCanvas SalesWindow()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(1, 1, 14, 10, Color.FromRgb(0x30, 0x30, 0x38));
            c.FillRect(2, 2, 12, 8, Color.FromRgb(0xA8, 0xD8, 0xF0));
            c.FillRect(6, 4, 4, 6, Color.FromRgb(0x38, 0xA0, 0x50));              // Verkäufer im Fenster
            c.FillRect(6, 3, 4, 2, Color.FromRgb(0xE8, 0xB8, 0x90));
            c.FillRect(0, 11, 16, 3, Color.FromRgb(0xC0, 0xC0, 0xC8));            // Ablage
            c.FillRect(4, 14, 8, 2, Color.FromRgb(0xD0, 0x28, 0x30));             // "24h"-Schild
            c.AddOutline(Outline);
            return c;
        }

        // ------------------------------------------------------------------ Omas Haus

        private static G1C_PixelCanvas GrandmaFloor()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, Color.FromRgb(0xB0, 0x80, 0x58));             // Dielen
            for (int y = 3; y < 16; y += 4) c.FillRect(0, y, 16, 1, Color.FromRgb(0x88, 0x60, 0x40));
            c.FillRect(5, 0, 1, 3, Color.FromRgb(0x88, 0x60, 0x40)); c.FillRect(11, 4, 1, 3, Color.FromRgb(0x88, 0x60, 0x40));
            c.FillRect(3, 8, 1, 3, Color.FromRgb(0x88, 0x60, 0x40)); c.FillRect(9, 12, 1, 3, Color.FromRgb(0x88, 0x60, 0x40));
            return c;
        }

        private static G1C_PixelCanvas GrandmaWall()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, Color.FromRgb(0xF0, 0xD8, 0xD8));             // Blümchentapete
            for (int x = 2; x < 16; x += 6) { c.Set(x, 3, Color.FromRgb(0xC8, 0x60, 0x80)); c.Set(x + 3, 8, Color.FromRgb(0xC8, 0x60, 0x80)); }
            c.FillRect(0, 0, 16, 2, Color.FromRgb(0x98, 0x58, 0x70));
            c.FillRect(0, 13, 16, 3, Color.FromRgb(0x78, 0x50, 0x38));            // Holzleiste
            return c;
        }

        private static G1C_PixelCanvas PlainFloor(Color main, Color seam)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, main);
            c.FillRect(0, 15, 16, 1, seam); c.FillRect(15, 0, 1, 16, seam);
            return c;
        }

        private static G1C_PixelCanvas TwoToneWall(Color color)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, Color.FromRgb(0xF0, 0xEC, 0xE0));
            c.FillRect(0, 0, 16, 5, color);
            c.FillRect(0, 5, 16, 1, Darker(color, 0.7));
            c.FillRect(0, 13, 16, 3, Darker(color, 0.6));
            return c;
        }

        private static G1C_PixelCanvas ClosedSign()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(29, 11);
            c.FillRect(1, 1, 27, 9, Color.FromRgb(0xC8, 0x20, 0x28));
            c.FillRect(1, 1, 27, 1, Color.FromRgb(0xE8, 0x58, 0x50)); // Lichtkante oben
            c.DrawRectBorder(0, 0, 29, 11, Outline);
            DrawWord(c, "CLOSED", 3, 3, Color.FromRgb(0xF8, 0xF8, 0xF8));
            return c;
        }

        private static G1C_PixelCanvas Poster()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 20);
            c.FillRect(1, 1, 14, 18, Color.FromRgb(0xF0, 0xE8, 0xC8));
            c.FillRect(2, 2, 12, 6, Color.FromRgb(0x38, 0x70, 0xC8));
            c.FillEllipse(8, 5, 2.5, 2.5, Color.FromRgb(0xF8, 0xD0, 0x48));
            for (int y = 10; y <= 16; y += 2) c.FillRect(3, y, y == 16 ? 6 : 10, 1, Color.FromRgb(0x60, 0x58, 0x60));
            c.DrawRectBorder(0, 0, 16, 20, Outline);
            return c;
        }

        private static G1C_PixelCanvas Sticker()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(10, 8);
            c.FillRect(1, 1, 8, 6, Color.FromRgb(0xF8, 0xD8, 0x30));
            c.FillRect(2, 3, 6, 1, Color.FromRgb(0x20, 0x20, 0x20));
            c.FillRect(2, 5, 4, 1, Color.FromRgb(0x20, 0x20, 0x20));
            c.DrawRectBorder(0, 0, 10, 8, Outline);
            return c;
        }

        // Holzschild auf Pfosten (Draufsicht wie bei den Stadtschildern im GBA-Stil)
        private static G1C_PixelCanvas SignPost()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(7, 9, 2, 6, Color.FromRgb(0x70, 0x48, 0x28));          // Pfosten
            c.FillRect(2, 2, 12, 8, Color.FromRgb(0xC8, 0x90, 0x50));         // Tafel
            c.FillRect(2, 2, 12, 1, Color.FromRgb(0xE0, 0xB0, 0x70));         // Lichtkante
            c.FillRect(2, 9, 12, 1, Color.FromRgb(0x90, 0x60, 0x30));         // Schattenkante
            for (int y = 4; y <= 7; y += 3) c.FillRect(4, y, 8, 1, Color.FromRgb(0x58, 0x38, 0x20)); // Schriftzeilen
            c.FillRect(6, 15, 4, 1, Color.FromArgb(0x60, 0, 0, 0));            // Schatten am Boden
            c.AddOutline(Outline);
            return c;
        }

        private static void DrawWord(G1C_PixelCanvas c, string word, int x, int y, Color color)
        {
            foreach (char ch in word)
            {
                string[] glyph;
                if (Font.TryGetValue(ch, out glyph))
                    for (int row = 0; row < glyph.Length; row++)
                        for (int col = 0; col < glyph[row].Length; col++)
                            if (glyph[row][col] == '1') c.Set(x + col, y + row, color);
                x += 4;
            }
        }
    }
}
