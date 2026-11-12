using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    /// <summary>
    /// Pixel-Zeichenfläche im Arbeitsspeicher (keine Bildschirmausgabe).
    /// Damit werden die eingebauten Pixel-Art-Grafiken erzeugt und die Karte
    /// einmalig zu einem einzigen Bild zusammengesetzt.
    /// </summary>
    public class G1C_PixelCanvas
    {
        private readonly uint[] _pixels;

        public int Width { get; }
        public int Height { get; }

        public G1C_PixelCanvas(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width), "Breite und Höhe müssen größer als 0 sein.");
            }

            Width = width;
            Height = height;
            _pixels = new uint[width * height];
        }

        // Farbe im Format 0xAARRGGBB (entspricht Bgra32 im Speicher)
        private static uint Pack(Color c)
        {
            return ((uint)c.A << 24) | ((uint)c.R << 16) | ((uint)c.G << 8) | c.B;
        }

        private bool InBounds(int x, int y)
        {
            return x >= 0 && y >= 0 && x < Width && y < Height;
        }

        private bool IsOpaque(uint[] data, int x, int y)
        {
            return InBounds(x, y) && (data[y * Width + x] >> 24) != 0;
        }

        public void Set(int x, int y, Color c)
        {
            if (!InBounds(x, y) || c.A == 0) return;
            _pixels[y * Width + x] = Pack(c);
        }

        public void FillRect(int x, int y, int w, int h, Color c)
        {
            for (int yy = y; yy < y + h; yy++)
            {
                for (int xx = x; xx < x + w; xx++)
                {
                    Set(xx, yy, c);
                }
            }
        }

        public void DrawRectBorder(int x, int y, int w, int h, Color c)
        {
            FillRect(x, y, w, 1, c);
            FillRect(x, y + h - 1, w, 1, c);
            FillRect(x, y, 1, h, c);
            FillRect(x + w - 1, y, 1, h, c);
        }

        public void FillEllipse(double cx, double cy, double rx, double ry, Color c)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    double dx = (x + 0.5 - cx) / rx;
                    double dy = (y + 0.5 - cy) / ry;
                    if (dx * dx + dy * dy <= 1.0)
                    {
                        Set(x, y, c);
                    }
                }
            }
        }

        /// <summary>
        /// Zeichnet eine Kontur um alle sichtbaren Pixel (typischer GBA-Look).
        /// Ein transparenter Pixel wird zur Kontur, wenn ein direkter Nachbar sichtbar ist.
        /// </summary>
        public void AddOutline(Color c)
        {
            uint[] copy = (uint[])_pixels.Clone();
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (IsOpaque(copy, x, y)) continue;
                    if (IsOpaque(copy, x - 1, y) || IsOpaque(copy, x + 1, y) ||
                        IsOpaque(copy, x, y - 1) || IsOpaque(copy, x, y + 1))
                    {
                        Set(x, y, c);
                    }
                }
            }
        }

        /// <summary>Gibt eine horizontal gespiegelte Kopie zurück (z.B. "rechts" → "links").</summary>
        public G1C_PixelCanvas MirrorX()
        {
            G1C_PixelCanvas result = new G1C_PixelCanvas(Width, Height);
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    result._pixels[y * Width + (Width - 1 - x)] = _pixels[y * Width + x];
                }
            }
            return result;
        }

        /// <summary>
        /// Kopiert ein Bitmap skaliert (Nearest Neighbour) an die angegebene Stelle.
        /// Wird zum Zusammensetzen der Karte aus einzelnen Kacheln verwendet.
        /// </summary>
        public void DrawBitmap(BitmapSource source, int x, int y, int w, int h)
        {
            if (source == null || w <= 0 || h <= 0) return;

            BitmapSource converted = source.Format == PixelFormats.Bgra32
                ? source
                : new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);

            int sw = converted.PixelWidth;
            int sh = converted.PixelHeight;
            uint[] data = new uint[sw * sh];
            converted.CopyPixels(data, sw * 4, 0);

            for (int yy = 0; yy < h; yy++)
            {
                int sy = yy * sh / h;
                for (int xx = 0; xx < w; xx++)
                {
                    int sx = xx * sw / w;
                    uint p = data[sy * sw + sx];
                    int tx = x + xx;
                    int ty = y + yy;
                    if ((p >> 24) != 0 && InBounds(tx, ty))
                    {
                        _pixels[ty * Width + tx] = p;
                    }
                }
            }
        }

        public BitmapSource ToBitmap()
        {
            WriteableBitmap bitmap = new WriteableBitmap(Width, Height, 96, 96, PixelFormats.Bgra32, null);
            bitmap.WritePixels(new Int32Rect(0, 0, Width, Height), _pixels, Width * 4, 0);
            bitmap.Freeze();
            return bitmap;
        }
    }
}
