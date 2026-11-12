using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_NpcSprites
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Eingebaute Grafiken des NPC-Systems:
    //   - portrait_<skin>_<ausdruck>: Kopf und Oberkörper aus dem vorhandenen Front-Sprite
    //     "<skin>_down_0", doppelt aufgelöst und um einen Gesichtsausdruck ergänzt. Haare, Haut
    //     und Kleidung kommen 1:1 aus dem Sprite, daher bleibt der Charakter wiedererkennbar.
    //   - item_glass, item_lost_chip: Gegenstände für Sammelaufgaben.
    // Alle IDs sind über PNG-Dateien austauschbar (G1C_SpriteLibrary). Ersetzt jemand das Front-Sprite
    // durch eine PNG anderer Größe, wird das Porträt ohne Gesichts-Overlay vergrößert.
    // =====================================================
    public static class G1C_NpcSprites
    {
        private const int Scale = 2;
        private const int BaseSize = 16;
        private const int CropRows = 13; // Kopf + Oberkörper des 16x16-Sprites

        // Entwickler: Eduard Schoenfeld
        public static BitmapSource Create(string id, G1C_SpriteLibrary library)
        {
            if (id == "item_glass") return Glass();
            if (id == "item_lost_chip") return LostChip();
            if (library == null || !id.StartsWith("portrait_", StringComparison.Ordinal)) return null;

            int last = id.LastIndexOf('_');
            if (last <= "portrait_".Length) return null;
            string skin = id.Substring("portrait_".Length, last - "portrait_".Length);
            G1C_NpcExpression expression;
            if (!Enum.TryParse(id.Substring(last + 1), true, out expression)) return null;

            return BuildPortrait(library.Get(skin + "_down_0"), expression);
        }

        public static string PortraitId(string skin, G1C_NpcExpression expression)
        {
            return "portrait_" + skin + "_" + expression.ToString().ToLowerInvariant();
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Porträt aus dem Front-Sprite erzeugen und Gesichtsausdruck einzeichnen.
        private static BitmapSource BuildPortrait(BitmapSource body, G1C_NpcExpression expression)
        {
            if (body.PixelWidth != BaseSize || body.PixelHeight != BaseSize)
            {
                // Fremde Grafik: nur oberen Teil ausschneiden, Gesicht nicht übermalen
                int rows = Math.Max(1, body.PixelHeight * CropRows / BaseSize);
                return new CroppedBitmap(body, new Int32Rect(0, 0, body.PixelWidth, rows));
            }

            BitmapSource source = body.Format == PixelFormats.Bgra32 ? body : new FormatConvertedBitmap(body, PixelFormats.Bgra32, null, 0);
            uint[] src = new uint[BaseSize * BaseSize];
            source.CopyPixels(src, BaseSize * 4, 0);

            int w = BaseSize * Scale;
            int h = CropRows * Scale;
            uint[] dst = new uint[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    dst[y * w + x] = src[(y / Scale) * BaseSize + x / Scale];

            // Farben aus dem Sprite übernehmen: Haut (Wange) und Augenfarbe
            uint skin = src[6 * BaseSize + 5];
            uint eye = src[4 * BaseSize + 6];
            uint mouth = Darken(skin, 0.6);
            DrawExpression(dst, w, h, expression, skin, eye, mouth);

            WriteableBitmap bitmap = new WriteableBitmap(w, h, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, w, h), dst, w * 4, 0);
            bitmap.Freeze();
            return bitmap;
        }

        // Koordinaten in doppelter Auflösung: Augen bei x 12–13 / 18–19, y 8–11; Brauen y 6–7; Mund y 12–14.
        private static void DrawExpression(uint[] p, int w, int h, G1C_NpcExpression expression, uint skin, uint eye, uint mouth)
        {
            Action<int, int, uint> set = (x, y, c) => { if (x >= 0 && y >= 0 && x < w && y < h) p[y * w + x] = c; };
            Action clearEyes = () =>
            {
                for (int y = 8; y <= 11; y++) { set(12, y, skin); set(13, y, skin); set(18, y, skin); set(19, y, skin); }
            };

            switch (expression)
            {
                case G1C_NpcExpression.Happy:
                    clearEyes();
                    foreach (int ox in new[] { 11, 17 }) { set(ox, 10, eye); set(ox + 1, 9, eye); set(ox + 2, 9, eye); set(ox + 3, 10, eye); }
                    set(13, 12, mouth); set(18, 12, mouth);
                    for (int x = 14; x <= 17; x++) set(x, 13, mouth);
                    break;

                case G1C_NpcExpression.Angry:
                    set(11, 6, eye); set(12, 6, eye); set(13, 7, eye); set(14, 7, eye); // Brauen fallen zur Mitte
                    set(17, 7, eye); set(18, 7, eye); set(19, 6, eye); set(20, 6, eye);
                    set(13, 14, mouth); set(18, 14, mouth);
                    for (int x = 14; x <= 17; x++) set(x, 13, mouth);
                    break;

                case G1C_NpcExpression.Sad:
                    set(11, 7, eye); set(12, 7, eye); set(13, 6, eye);                 // Brauen steigen zur Mitte
                    set(18, 6, eye); set(19, 7, eye); set(20, 7, eye);
                    set(12, 12, 0xFF70B8F0); set(12, 13, 0xFF70B8F0);                // Träne
                    set(14, 14, mouth); set(15, 13, mouth); set(16, 13, mouth); set(17, 14, mouth);
                    break;

                case G1C_NpcExpression.Surprised:
                    for (int y = 7; y <= 12; y++) { set(12, y, eye); set(13, y, eye); set(18, y, eye); set(19, y, eye); }
                    set(12, 8, 0xFFF8F8F8); set(18, 8, 0xFFF8F8F8);                  // Glanzpunkte
                    set(14, 13, mouth); set(15, 13, mouth); set(13, 14, mouth); set(16, 14, mouth);
                    set(14, 15, mouth); set(15, 15, mouth);
                    break;

                case G1C_NpcExpression.Thoughtful:
                    for (int y = 8; y <= 11; y++) { set(12, y, skin); set(18, y, skin); }  // Blick zur Seite
                    set(18, 7, eye); set(19, 6, eye); set(20, 7, eye);                // eine Braue hochgezogen (unter dem Haaransatz)
                    for (int x = 15; x <= 18; x++) set(x, 13, mouth);
                    break;

                default: // Neutral
                    for (int x = 14; x <= 17; x++) set(x, 13, mouth);
                    break;
            }
        }

        private static uint Darken(uint argb, double factor)
        {
            uint r = (uint)(((argb >> 16) & 0xFF) * factor);
            uint g = (uint)(((argb >> 8) & 0xFF) * factor);
            uint b = (uint)((argb & 0xFF) * factor);
            return 0xFF000000 | (r << 16) | (g << 8) | b;
        }

        // Entwickler: Eduard Schoenfeld
        private static BitmapSource Glass()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(5, 4, 6, 8, Color.FromRgb(0xA8, 0xE0, 0xF0));
            c.FillRect(6, 5, 1, 5, Color.FromRgb(0xF8, 0xF8, 0xF8));
            c.FillRect(7, 12, 2, 2, Color.FromRgb(0xA8, 0xE0, 0xF0));
            c.FillRect(5, 14, 6, 1, Color.FromRgb(0xA8, 0xE0, 0xF0));
            c.AddOutline(Color.FromRgb(0x30, 0x50, 0x68));
            return c.ToBitmap();
        }

        // Entwickler: Eduard Schoenfeld
        private static BitmapSource LostChip()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillEllipse(8, 10, 5, 3.5, Color.FromRgb(0xD0, 0x30, 0x30));
            c.FillEllipse(8, 10, 2.5, 1.5, Color.FromRgb(0xF8, 0xF8, 0xF8));
            c.FillRect(4, 9, 1, 2, Color.FromRgb(0xF8, 0xF8, 0xF8));
            c.FillRect(11, 9, 1, 2, Color.FromRgb(0xF8, 0xF8, 0xF8));
            c.AddOutline(Color.FromArgb(0xFF, 0x18, 0x18, 0x20));
            return c.ToBitmap();
        }
    }
}
