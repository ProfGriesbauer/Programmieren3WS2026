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
                default: return null;
            }
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
