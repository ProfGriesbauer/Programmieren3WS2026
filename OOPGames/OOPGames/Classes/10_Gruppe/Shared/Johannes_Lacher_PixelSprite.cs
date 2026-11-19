using System;
using System.Collections.Generic;

namespace OOPGames
{
    // A small pixel image. Pixels are ARGB values (0xAARRGGBB); 0 means transparent.
    // Only fully opaque or fully transparent pixels are used.
    internal sealed class G10_Sprite
    {
        public G10_Sprite(int width, int height, uint[] pixels)
        {
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));
            if (pixels == null || pixels.Length != width * height) throw new ArgumentException("Pixel count does not match the size.", nameof(pixels));
            Width = width;
            Height = height;
            Pixels = pixels;
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public uint[] Pixels { get; private set; }

        // Builds a sprite from text rows. Each character is looked up in the palette;
        // '.' and ' ' are transparent. All rows must have the same length.
        public static G10_Sprite FromPattern(string[] rows, IDictionary<char, uint> palette)
        {
            if (rows == null || rows.Length == 0) throw new ArgumentException("A sprite needs at least one row.", nameof(rows));
            int width = rows[0].Length;
            uint[] pixels = new uint[width * rows.Length];
            for (int y = 0; y < rows.Length; y++)
            {
                if (rows[y].Length != width) throw new ArgumentException("Row " + y + " has a different length.", nameof(rows));
                for (int x = 0; x < width; x++)
                {
                    char c = rows[y][x];
                    if (c == '.' || c == ' ') continue;
                    uint color;
                    if (!palette.TryGetValue(c, out color)) throw new ArgumentException("No palette colour for '" + c + "'.", nameof(palette));
                    pixels[y * width + x] = color;
                }
            }
            return new G10_Sprite(width, rows.Length, pixels);
        }
    }

    // A small pixel image that is built by code before it becomes a G10_Sprite.
    internal sealed class G10_PixelCanvas
    {
        readonly uint[] _Pixels;

        public G10_PixelCanvas(int width, int height)
        {
            Width = width;
            Height = height;
            _Pixels = new uint[width * height];
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public uint Get(int x, int y)
        {
            return x < 0 || y < 0 || x >= Width || y >= Height ? 0 : _Pixels[y * Width + x];
        }

        public void Set(int x, int y, uint color)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return;
            _Pixels[y * Width + x] = color;
        }

        public void Rect(int x, int y, int w, int h, uint color)
        {
            for (int py = y; py < y + h; py++)
                for (int px = x; px < x + w; px++)
                    Set(px, py, color);
        }

        // Ellipse around (cx, cy) measured from pixel centres.
        public void Ellipse(double cx, double cy, double rx, double ry, uint color)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    double dx = (x + 0.5 - cx) / rx;
                    double dy = (y + 0.5 - cy) / ry;
                    if (dx * dx + dy * dy <= 1) Set(x, y, color);
                }
            }
        }

        public void Disc(double cx, double cy, double r, uint color)
        {
            Ellipse(cx, cy, r, r, color);
        }

        public void Pattern(int x, int y, string[] rows, IDictionary<char, uint> palette)
        {
            for (int row = 0; row < rows.Length; row++)
            {
                for (int col = 0; col < rows[row].Length; col++)
                {
                    uint color;
                    if (palette.TryGetValue(rows[row][col], out color)) Set(x + col, y + row, color);
                }
            }
        }

        public G10_Sprite ToSprite()
        {
            return new G10_Sprite(Width, Height, (uint[])_Pixels.Clone());
        }

        public G10_Sprite ToMirroredSprite()
        {
            uint[] mirrored = new uint[_Pixels.Length];
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    mirrored[y * Width + x] = _Pixels[y * Width + Width - 1 - x];
            return new G10_Sprite(Width, Height, mirrored);
        }
    }
}
