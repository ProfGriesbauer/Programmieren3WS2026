using System;

namespace OOPGames
{
    // Rectangle (right and bottom exclusive) that limits where a sprite is drawn.
    internal struct G10_ClipRect
    {
        public G10_ClipRect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // A plain ARGB pixel array with the few drawing operations the pixel renderers need.
    internal sealed class G10_PixelBuffer
    {
        public G10_PixelBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new uint[width * height];
            Bounds = new G10_ClipRect(0, 0, width, height);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public uint[] Pixels { get; private set; }
        public G10_ClipRect Bounds { get; private set; }

        public void CopyFrom(uint[] source)
        {
            Array.Copy(source, Pixels, Pixels.Length);
        }

        public void FillRect(int x, int y, int w, int h, uint color)
        {
            int x0 = Math.Max(x, 0);
            int y0 = Math.Max(y, 0);
            int x1 = Math.Min(x + w, Width);
            int y1 = Math.Min(y + h, Height);
            for (int py = y0; py < y1; py++)
            {
                int row = py * Width;
                for (int px = x0; px < x1; px++)
                {
                    Pixels[row + px] = color;
                }
            }
        }

        // Copies the non-transparent sprite pixels; tint 0 keeps the sprite colours.
        public void Blit(G10_Sprite sprite, int x, int y, G10_ClipRect clip, uint tint)
        {
            int x0 = Math.Max(Math.Max(clip.Left, 0), x);
            int y0 = Math.Max(Math.Max(clip.Top, 0), y);
            int x1 = Math.Min(Math.Min(clip.Right, Width), x + sprite.Width);
            int y1 = Math.Min(Math.Min(clip.Bottom, Height), y + sprite.Height);
            uint[] source = sprite.Pixels;
            for (int py = y0; py < y1; py++)
            {
                int src = (py - y) * sprite.Width - x;
                int dst = py * Width;
                for (int px = x0; px < x1; px++)
                {
                    uint color = source[src + px];
                    if (color != 0)
                    {
                        Pixels[dst + px] = tint != 0 ? tint : color;
                    }
                }
            }
        }

        public void DrawText(string text, int x, int y, uint color)
        {
            for (int i = 0; i < text.Length; i++)
            {
                Blit(G10_PixelFont.Glyph(text[i]), x + i * G10_PixelFont.Advance, y, Bounds, color);
            }
        }

        public static int TextWidth(string text)
        {
            return text.Length * G10_PixelFont.Advance - 1;
        }
    }
}
