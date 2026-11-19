<#
.SYNOPSIS
    Erzeugt die Pixel-Art-Spritesheets fuer das Tower-Defense-Spiel (Gruppe 10).

.DESCRIPTION
    Zeichnet Gelaende, Deko, Kartenhintergrund, Hauptgebaeude, Gegner, Bosse und UI-Icons
    im Stil von tower_spritesheet.png (1 Kunstpixel = 4x4 Bildpixel, dunkler Umriss).
    tower_spritesheet.png wird NICHT veraendert, nur fuer die Vorschau gelesen.

    Achtung: Alle anderen Sheets in Assets/TowerDefense werden ueberschrieben. Wer ein Sheet
    von Hand nachbearbeitet, sollte es danach nicht erneut generieren.

.PARAMETER AssetDir
    Zielordner der PNG-Dateien. Standard: OOPGames/Assets/TowerDefense.

.PARAMETER PreviewDir
    Optionaler Ordner fuer vergroesserte Vorschaubilder (Szene, Gegner, Bosse, Icons).

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File .\Generate-TowerDefenseSprites.ps1
#>
param(
    [string]$AssetDir = (Join-Path $PSScriptRoot "..\..\..\..\Assets\TowerDefense"),
    [string]$PreviewDir = $null
)

$ErrorActionPreference = "Stop"
$AssetDir = [System.IO.Path]::GetFullPath($AssetDir)
if ($PreviewDir) { $PreviewDir = [System.IO.Path]::GetFullPath($PreviewDir) }

$source = @'
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System;


namespace TowerDefenseSprites
{
    // One sprite in art pixels. Sheets are saved with a 4x scale like tower_spritesheet.png.
    public class Spr
    {
        public readonly int W;
        public readonly int H;
        readonly Color[,] _P;

        public Spr(int w, int h)
        {
            W = w; H = h; _P = new Color[w, h];
            for (int y = 0; y < h; y++) for (int x = 0; x < w; x++) _P[x, y] = Color.Transparent;
        }

        public Color Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return Color.Transparent;
            return _P[x, y];
        }

        public bool Solid(int x, int y) { return Get(x, y).A == 255; }
        public bool Empty(int x, int y) { return Get(x, y).A == 0; }

        public void Set(int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return;
            _P[x, y] = c;
        }

        public void Clear(int x, int y) { Set(x, y, Color.Transparent); }

        public void Px(int x, int y, Color c)
        {
            if (x < 0 || y < 0 || x >= W || y >= H || c.A == 0) return;
            Color d = _P[x, y];
            if (c.A == 255 || d.A == 0) { _P[x, y] = c; return; }
            double a = c.A / 255.0, da = d.A / 255.0;
            double oa = a + da * (1 - a);
            int r = (int)Math.Round((c.R * a + d.R * da * (1 - a)) / oa);
            int g = (int)Math.Round((c.G * a + d.G * da * (1 - a)) / oa);
            int b = (int)Math.Round((c.B * a + d.B * da * (1 - a)) / oa);
            _P[x, y] = Color.FromArgb((int)Math.Round(oa * 255), r, g, b);
        }

        public void Behind(int x, int y, Color c) { if (Empty(x, y)) Px(x, y, c); }

        public void Rect(int x, int y, int w, int h, Color c)
        {
            for (int yy = y; yy < y + h; yy++) for (int xx = x; xx < x + w; xx++) Px(xx, yy, c);
        }

        public void RectBehind(int x, int y, int w, int h, Color c)
        {
            for (int yy = y; yy < y + h; yy++) for (int xx = x; xx < x + w; xx++) Behind(xx, yy, c);
        }

        public static bool InEllipse(int x, int y, double cx, double cy, double rx, double ry)
        {
            double dx = (x + 0.5 - cx) / rx, dy = (y + 0.5 - cy) / ry;
            return dx * dx + dy * dy <= 1.0;
        }

        public void Ellipse(double cx, double cy, double rx, double ry, Color c)
        {
            for (int y = (int)Math.Floor(cy - ry - 1); y <= (int)Math.Ceiling(cy + ry + 1); y++)
                for (int x = (int)Math.Floor(cx - rx - 1); x <= (int)Math.Ceiling(cx + rx + 1); x++)
                    if (InEllipse(x, y, cx, cy, rx, ry)) Px(x, y, c);
        }

        public void EllipseBehind(double cx, double cy, double rx, double ry, Color c)
        {
            for (int y = (int)Math.Floor(cy - ry - 1); y <= (int)Math.Ceiling(cy + ry + 1); y++)
                for (int x = (int)Math.Floor(cx - rx - 1); x <= (int)Math.Ceiling(cx + rx + 1); x++)
                    if (InEllipse(x, y, cx, cy, rx, ry)) Behind(x, y, c);
        }

        public void Line(int x0, int y0, int x1, int y1, Color c)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            while (true)
            {
                Px(x0, y0, c);
                if (x0 == x1 && y0 == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x0 += sx; }
                if (e2 <= dx) { err += dx; y0 += sy; }
            }
        }

        public void Poly(double[] pts, Color c)
        {
            int n = pts.Length / 2;
            double minX = 1e9, maxX = -1e9, minY = 1e9, maxY = -1e9;
            for (int i = 0; i < n; i++)
            {
                minX = Math.Min(minX, pts[i * 2]); maxX = Math.Max(maxX, pts[i * 2]);
                minY = Math.Min(minY, pts[i * 2 + 1]); maxY = Math.Max(maxY, pts[i * 2 + 1]);
            }
            for (int y = (int)Math.Floor(minY); y <= (int)Math.Ceiling(maxY); y++)
                for (int x = (int)Math.Floor(minX); x <= (int)Math.Ceiling(maxX); x++)
                {
                    double px = x + 0.5, py = y + 0.5;
                    bool inside = false;
                    for (int i = 0, j = n - 1; i < n; j = i++)
                    {
                        double xi = pts[i * 2], yi = pts[i * 2 + 1], xj = pts[j * 2], yj = pts[j * 2 + 1];
                        if (((yi > py) != (yj > py)) && (px < (xj - xi) * (py - yi) / (yj - yi) + xi)) inside = !inside;
                    }
                    if (inside) Px(x, y, c);
                }
        }

        public void Grid(string[] rows, int ox, int oy, Dictionary<char, Color> pal)
        {
            for (int y = 0; y < rows.Length; y++)
                for (int x = 0; x < rows[y].Length; x++)
                {
                    Color c;
                    if (pal.TryGetValue(rows[y][x], out c)) Px(ox + x, oy + y, c);
                }
        }

        // Adds the dark 1px outline used by the tower sprites around every opaque pixel.
        public void Outline(Color c)
        {
            List<Point> add = new List<Point>();
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (Empty(x, y) && (Solid(x - 1, y) || Solid(x + 1, y) || Solid(x, y - 1) || Solid(x, y + 1)))
                        add.Add(new Point(x, y));
            foreach (Point p in add) Set(p.X, p.Y, c);
        }

        public void Stamp(Spr s, int ox, int oy)
        {
            for (int y = 0; y < s.H; y++) for (int x = 0; x < s.W; x++) Px(ox + x, oy + y, s.Get(x, y));
        }

        public Spr Clone()
        {
            Spr s = new Spr(W, H);
            for (int y = 0; y < H; y++) for (int x = 0; x < W; x++) s.Set(x, y, _P[x, y]);
            return s;
        }
    }

    static class C
    {
        public static Color Rgb(int r, int g, int b) { return Color.FromArgb(255, r, g, b); }
        public static Color Argb(int a, int r, int g, int b) { return Color.FromArgb(a, r, g, b); }
        public static Color Mix(Color a, Color b, double t)
        {
            return Color.FromArgb(255, (int)(a.R + (b.R - a.R) * t), (int)(a.G + (b.G - a.G) * t), (int)(a.B + (b.B - a.B) * t));
        }
        public static Color Light(Color c, double t) { return Mix(c, Color.White, t); }
        public static Color Dark(Color c, double t) { return Mix(c, Color.Black, t); }

        // Palette taken from tower_spritesheet.png
        public static readonly Color Out = Rgb(43, 37, 32);
        public static readonly Color Out2 = Rgb(27, 24, 22);
        public static readonly Color S0 = Rgb(94, 87, 76), S1 = Rgb(138, 130, 116), S2 = Rgb(170, 162, 147), S3 = Rgb(207, 199, 180), S4 = Rgb(232, 226, 208);
        public static readonly Color W0 = Rgb(74, 50, 32), W1 = Rgb(107, 74, 43), W2 = Rgb(150, 104, 62), W3 = Rgb(186, 138, 86);
        public static readonly Color Skin = Rgb(232, 184, 138), SkinD = Rgb(196, 144, 104);
        public static readonly Color Shadow = Argb(71, 0, 0, 0), ShadowL = Argb(46, 0, 0, 0);
        public static readonly Color GR0 = Rgb(58, 98, 52), GR1 = Rgb(78, 124, 58), GR2 = Rgb(98, 148, 66), GR3 = Rgb(126, 172, 78), GR4 = Rgb(166, 200, 98);
        public static readonly Color D0 = Rgb(116, 82, 54), D1 = Rgb(146, 106, 70), D2 = Rgb(178, 134, 88), D3 = Rgb(204, 166, 114);
        public static readonly Color WA0 = Rgb(44, 92, 132), WA1 = Rgb(62, 126, 168), WA2 = Rgb(96, 164, 200), WA3 = Rgb(178, 220, 234);
        public static readonly Color R0 = Rgb(120, 36, 32), R1 = Rgb(176, 58, 46), R2 = Rgb(222, 96, 64);
        public static readonly Color Y0 = Rgb(170, 112, 30), Y1 = Rgb(234, 178, 52), Y2 = Rgb(255, 224, 108);
        public static readonly Color M0 = Rgb(84, 92, 104), M1 = Rgb(134, 144, 158), M2 = Rgb(196, 204, 214);
        public static readonly Color B0 = Rgb(150, 142, 120), B1 = Rgb(208, 202, 182), B2 = Rgb(242, 238, 222);
        public static readonly Color Sand = Rgb(212, 190, 136), SandD = Rgb(178, 154, 104);
        public static readonly Color TG0 = Rgb(40, 86, 50), TG1 = Rgb(62, 120, 58), TG2 = Rgb(98, 154, 68), TG3 = Rgb(146, 190, 88);
        public static readonly Color Fire0 = Rgb(178, 48, 28), Fire1 = Rgb(236, 112, 36), Fire2 = Rgb(255, 184, 60), Fire3 = Rgb(255, 238, 150);
        public static readonly Color Ice0 = Rgb(64, 132, 196), Ice1 = Rgb(120, 196, 240), Ice2 = Rgb(196, 238, 255);
        public static readonly Color Storm0 = Rgb(80, 72, 126), Storm1 = Rgb(122, 112, 170), Storm2 = Rgb(180, 170, 222);
        public static readonly Color Poison0 = Rgb(54, 120, 46), Poison1 = Rgb(110, 190, 70), Poison2 = Rgb(184, 236, 120);
        public static readonly Color Purple0 = Rgb(72, 46, 104), Purple1 = Rgb(112, 72, 150), Purple2 = Rgb(160, 118, 200);
    }

    public class SheetWriter
    {
        public static void Save(string path, int artW, int artH, List<KeyValuePair<Point, Spr>> parts, int scale)
        {
            using (Bitmap bmp = new Bitmap(artW * scale, artH * scale, PixelFormat.Format32bppArgb))
            {
                int[] data = new int[bmp.Width * bmp.Height];
                foreach (KeyValuePair<Point, Spr> part in parts)
                {
                    Spr s = part.Value;
                    for (int y = 0; y < s.H; y++)
                        for (int x = 0; x < s.W; x++)
                        {
                            Color c = s.Get(x, y);
                            int ax = part.Key.X + x, ay = part.Key.Y + y;
                            if (ax < 0 || ay < 0 || ax >= artW || ay >= artH) continue;
                            int argb = c.ToArgb();
                            for (int sy = 0; sy < scale; sy++)
                                for (int sx = 0; sx < scale; sx++)
                                    data[(ay * scale + sy) * bmp.Width + ax * scale + sx] = argb;
                        }
                }
                BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                for (int row = 0; row < bmp.Height; row++)
                    Marshal.Copy(data, row * bmp.Width, IntPtr.Add(bd.Scan0, row * bd.Stride), bmp.Width);
                bmp.UnlockBits(bd);
                bmp.Save(path, ImageFormat.Png);
            }
        }

        public static void SaveRow(string path, List<Spr> frames, int cellW, int cellH, int columns, int scale)
        {
            int rows = (frames.Count + columns - 1) / columns;
            List<KeyValuePair<Point, Spr>> parts = new List<KeyValuePair<Point, Spr>>();
            for (int i = 0; i < frames.Count; i++)
                parts.Add(new KeyValuePair<Point, Spr>(new Point((i % columns) * cellW, (i / columns) * cellH), frames[i]));
            Save(path, cellW * columns, cellH * rows, parts, scale);
        }

        public static Spr Load(string path, int scale)
        {
            using (Bitmap bmp = new Bitmap(path))
            {
                Spr s = new Spr(bmp.Width / scale, bmp.Height / scale);
                for (int y = 0; y < s.H; y++)
                    for (int x = 0; x < s.W; x++)
                        s.Set(x, y, bmp.GetPixel(x * scale + scale / 2, y * scale + scale / 2));
                return s;
            }
        }

        public static Spr Crop(Spr src, int x, int y, int w, int h)
        {
            Spr s = new Spr(w, h);
            for (int yy = 0; yy < h; yy++) for (int xx = 0; xx < w; xx++) s.Set(xx, yy, src.Get(x + xx, y + yy));
            return s;
        }
    }
}


namespace TowerDefenseSprites
{
    public static class World
    {
        // ---------- terrain tiles (16x16) ----------
        public static List<Spr> TerrainTiles()
        {
            List<Spr> t = new List<Spr>();
            for (int v = 0; v < 4; v++) t.Add(Grass(v));
            t.Add(Dirt(0)); t.Add(Dirt(1));
            t.Add(Water(0)); t.Add(Water(1));
            return t;
        }

        static Spr Grass(int variant)
        {
            Spr s = new Spr(16, 16);
            s.Rect(0, 0, 16, 16, C.GR2);
            Random r = new Random(100 + variant);
            for (int i = 0; i < 12; i++) s.Px(r.Next(16), r.Next(16), C.GR1);
            for (int i = 0; i < 7; i++) s.Px(r.Next(16), r.Next(16), C.GR3);
            if (variant == 1)
                for (int i = 0; i < 3; i++) Tuft(s, r.Next(1, 13), r.Next(3, 15));
            if (variant == 2)
            {
                Color[] fl = { C.B2, C.Y2, C.Rgb(236, 140, 172) };
                for (int i = 0; i < 3; i++)
                {
                    int x = r.Next(1, 15), y = r.Next(1, 14);
                    s.Px(x, y + 1, C.GR0); s.Px(x, y, fl[i % 3]);
                }
                Tuft(s, r.Next(1, 13), r.Next(3, 15));
            }
            if (variant == 3)
                for (int i = 0; i < 3; i++)
                {
                    int x = r.Next(0, 14), y = r.Next(0, 15);
                    s.Rect(x, y, 3, 2, C.GR1); s.Px(x + 1, y + 1, C.GR0); s.Px(x, y, C.GR3);
                }
            return s;
        }

        static void Tuft(Spr s, int x, int y)
        {
            s.Rect(x, y, 3, 1, C.GR1);
            s.Px(x, y - 1, C.GR3); s.Px(x + 2, y - 1, C.GR3); s.Px(x + 1, y - 1, C.GR3); s.Px(x + 1, y - 2, C.GR4);
        }

        static Spr Dirt(int variant)
        {
            Spr s = new Spr(16, 16);
            s.Rect(0, 0, 16, 16, C.D2);
            Random r = new Random(200 + variant);
            for (int i = 0; i < 16; i++) s.Px(r.Next(16), r.Next(16), C.D1);
            for (int i = 0; i < 9; i++) s.Px(r.Next(16), r.Next(16), C.D3);
            for (int i = 0; i < 2 + variant; i++)
            {
                int x = r.Next(1, 14), y = r.Next(1, 14);
                s.Px(x, y, C.S2); s.Px(x + 1, y, C.S3); s.Px(x, y + 1, C.D0); s.Px(x + 1, y + 1, C.D1);
            }
            return s;
        }

        static Spr Water(int variant)
        {
            Spr s = new Spr(16, 16);
            s.Rect(0, 0, 16, 16, C.WA1);
            Random r = new Random(300 + variant);
            for (int i = 0; i < 6; i++) s.Px(r.Next(16), r.Next(16), C.WA0);
            for (int i = 0; i < 3; i++)
            {
                int x = r.Next(0, 12), y = r.Next(1, 15), len = r.Next(3, 5);
                s.Rect(x, y, len, 1, C.WA2); s.Px(x + 1, y, C.WA3);
            }
            return s;
        }

        // ---------- scenery ----------
        public static Spr Oak()
        {
            Spr s = new Spr(32, 32);
            s.Rect(14, 19, 4, 9, C.W1); s.Rect(17, 19, 1, 9, C.W0); s.Px(13, 27, C.W1); s.Px(18, 27, C.W0); s.Px(15, 21, C.W2);
            s.Ellipse(16, 13, 11, 9, C.TG0); s.Ellipse(8, 16, 5, 4, C.TG0); s.Ellipse(24, 16, 5, 4, C.TG0);
            s.Ellipse(15, 11.5, 9.5, 7.5, C.TG1); s.Ellipse(9, 13, 4, 3.5, C.TG1); s.Ellipse(22, 12, 4.5, 4, C.TG1);
            s.Ellipse(13, 8.5, 6, 4.5, C.TG2); s.Ellipse(21, 9.5, 3, 2.5, C.TG2); s.Ellipse(8, 12, 2, 1.5, C.TG2);
            s.Px(11, 6, C.TG3); s.Px(12, 5, C.TG3); s.Px(13, 6, C.TG3); s.Px(20, 8, C.TG3); s.Px(7, 11, C.TG3);
            Random r = new Random(7);
            for (int i = 0; i < 14; i++)
            {
                int x = r.Next(7, 25), y = r.Next(6, 20);
                if (s.Get(x, y) == C.TG1) s.Px(x, y, C.TG0);
            }
            s.Outline(C.Out);
            s.EllipseBehind(16, 28.5, 10, 2.5, C.Shadow);
            return s;
        }

        public static Spr Pine()
        {
            Spr s = new Spr(32, 32);
            Color p0 = C.Rgb(30, 78, 62), p1 = C.Rgb(46, 108, 74), p2 = C.Rgb(78, 142, 86);
            s.Rect(15, 24, 3, 4, C.W1); s.Rect(17, 24, 1, 4, C.W0);
            for (int i = 3; i >= 0; i--)
            {
                int apex = 2 + i * 5;
                double hw = 4 + i * 2.4;
                for (int y = apex; y <= apex + 8; y++)
                {
                    double w = (y - apex) / 8.0 * hw + 0.6;
                    for (int x = (int)Math.Round(16 - w); x <= (int)Math.Round(16 + w); x++)
                    {
                        Color c = x < 16 - w * 0.35 ? p2 : (x < 16 + w * 0.35 ? p1 : p0);
                        if (y == apex + 8) c = p0;
                        s.Px(x, y, c);
                    }
                }
            }
            s.Px(16, 1, p2);
            s.Outline(C.Out);
            s.EllipseBehind(16.5, 28.5, 8, 2.2, C.Shadow);
            return s;
        }

        public static Spr Bush()
        {
            Spr s = new Spr(32, 32);
            s.Ellipse(16, 24, 8, 5, C.TG0); s.Ellipse(15, 23, 6.5, 4, C.TG1); s.Ellipse(13, 21.5, 3.5, 2, C.TG2);
            s.Px(12, 21, C.TG3);
            s.Px(11, 24, C.R2); s.Px(18, 22, C.R2); s.Px(20, 25, C.R1); s.Px(15, 26, C.R2);
            s.Outline(C.Out);
            s.EllipseBehind(16, 29, 8, 1.8, C.Shadow);
            return s;
        }

        public static Spr Rock()
        {
            Spr s = new Spr(32, 32);
            s.Ellipse(16, 24, 9, 5.5, C.S0); s.Ellipse(15.5, 23, 8, 4.5, C.S1); s.Ellipse(14, 21.5, 5, 2.8, C.S2);
            s.Px(12, 20, C.S3); s.Px(13, 20, C.S3);
            s.Line(18, 21, 19, 24, C.S0);
            s.Px(20, 20, C.GR3); s.Px(21, 20, C.GR2); s.Px(19, 20, C.GR3);
            s.Outline(C.Out);
            s.EllipseBehind(16, 29, 10, 1.8, C.Shadow);
            return s;
        }

        public static Spr Pebbles()
        {
            Spr s = new Spr(32, 32);
            s.Ellipse(12, 26, 4, 2.5, C.S1); s.Ellipse(11.5, 25.5, 2.5, 1.5, C.S2);
            s.Ellipse(20, 27, 3, 2, C.S1); s.Px(19, 26, C.S3);
            s.Outline(C.Out);
            s.EllipseBehind(16, 29.5, 9, 1.4, C.ShadowL);
            return s;
        }

        public static Spr Flowers()
        {
            Spr s = new Spr(32, 32);
            Color[] heads = { C.Y2, C.Rgb(236, 120, 90), C.Rgb(226, 120, 180), C.B2, C.Rgb(150, 160, 240) };
            int[] xs = { 9, 13, 17, 21, 24 };
            int[] ys = { 24, 22, 25, 23, 26 };
            for (int i = 0; i < xs.Length; i++)
            {
                s.Rect(xs[i], ys[i], 1, 28 - ys[i], C.GR0);
                s.Px(xs[i] + 1, ys[i] + 2, C.GR1);
                Color h = heads[i];
                s.Px(xs[i], ys[i] - 1, h); s.Px(xs[i] - 1, ys[i], h); s.Px(xs[i] + 1, ys[i], h); s.Px(xs[i], ys[i] + 1, C.Dark(h, 0.3));
                s.Px(xs[i], ys[i], C.Y1);
            }
            return s;
        }

        static Spr House(Color r0, Color r1, Color r2)
        {
            Spr s = new Spr(32, 32);
            Color plaster = C.Rgb(214, 196, 160), plasterD = C.Rgb(178, 160, 128);
            s.Rect(5, 16, 22, 12, plaster); s.Rect(26, 16, 1, 12, plasterD); s.Rect(5, 27, 22, 1, plasterD);
            s.Rect(5, 16, 1, 12, C.W1); s.Rect(15, 16, 1, 5, C.W1); s.Rect(26, 16, 1, 12, C.W1); s.Rect(5, 16, 22, 1, C.W1);
            s.Rect(21, 6, 3, 6, C.S1); s.Rect(23, 6, 1, 6, C.S0); s.Rect(21, 5, 3, 1, C.S2);
            for (int y = 5; y <= 16; y++)
            {
                double half = Math.Min(13.5, 1.5 + (y - 5) * 1.25);
                for (int x = (int)Math.Round(16 - half); x < (int)Math.Round(16 + half); x++)
                {
                    Color c = r1;
                    if (x < 16 - half + 2) c = r2;
                    if ((y - 5) % 3 == 2) c = r0;
                    if (y == 16) c = r0;
                    s.Px(x, y, c);
                }
            }
            s.Rect(14, 20, 4, 8, C.W0); s.Rect(15, 21, 2, 7, C.W1); s.Px(17, 24, C.Y1);
            Color glass = C.Rgb(96, 154, 172);
            s.Rect(8, 19, 4, 4, glass); s.Px(8, 19, C.WA3); s.Rect(9, 19, 1, 4, C.W1); s.Rect(8, 20, 4, 1, C.W1);
            s.Rect(20, 19, 4, 4, glass); s.Px(20, 19, C.WA3); s.Rect(21, 19, 1, 4, C.W1); s.Rect(20, 20, 4, 1, C.W1);
            s.Rect(7, 23, 6, 1, C.W2); s.Rect(19, 23, 6, 1, C.W2);
            s.Outline(C.Out);
            s.EllipseBehind(16, 29, 13, 2, C.Shadow);
            return s;
        }

        public static Spr HouseRed() { return House(C.R0, C.R1, C.R2); }
        public static Spr HouseBrown() { return House(C.Rgb(92, 60, 44), C.Rgb(132, 88, 58), C.Rgb(170, 120, 78)); }

        // Flat cobblestone pedestal. Anchor: cell (16, 24) is the tower spot.
        public static Spr Pedestal()
        {
            Spr s = new Spr(32, 32);
            Color mortar = C.Rgb(104, 97, 86);
            s.Ellipse(16, 25.5, 13, 5, C.S0);
            for (int y = 18; y < 31; y++)
                for (int x = 2; x < 30; x++)
                {
                    if (!Spr.InEllipse(x, y, 16, 24, 13, 4.5)) continue;
                    int row = (y - 19) / 3;
                    int off = (row % 2) * 3;
                    bool mortarPx = (y - 19) % 3 == 2 || (x + off) % 6 == 5;
                    Color c = mortarPx ? mortar : (((x + off) / 6 + row) % 3 == 0 ? C.S1 : C.S2);
                    if (!mortarPx && (y - 19) % 3 == 0 && (x + off) % 6 == 0) c = C.S3;
                    bool rim = !Spr.InEllipse(x, y - 1, 16, 24, 13, 4.5) || !Spr.InEllipse(x - 1, y, 16, 24, 13, 4.5) || !Spr.InEllipse(x + 1, y, 16, 24, 13, 4.5);
                    if (rim) c = C.S3;
                    s.Px(x, y, c);
                }
            s.Outline(C.Out);
            s.EllipseBehind(16, 28.5, 14.5, 2.8, C.Shadow);
            return s;
        }

        public static Spr Fence()
        {
            Spr s = new Spr(32, 32);
            s.Rect(1, 21, 30, 2, C.W2); s.Rect(1, 22, 30, 1, C.W1);
            s.Rect(1, 25, 30, 2, C.W2); s.Rect(1, 26, 30, 1, C.W1);
            int[] posts = { 2, 15, 27 };
            foreach (int x in posts) { s.Rect(x, 19, 3, 10, C.W1); s.Rect(x + 2, 19, 1, 10, C.W0); s.Px(x, 19, C.W3); }
            s.Outline(C.Out);
            s.RectBehind(1, 29, 30, 1, C.ShadowL);
            return s;
        }

        public static Spr Barrel()
        {
            Spr s = new Spr(32, 32);
            s.Rect(11, 18, 10, 11, C.W2); s.Rect(10, 20, 12, 7, C.W2);
            s.Rect(18, 18, 3, 11, C.W1); s.Rect(20, 20, 2, 7, C.W1); s.Rect(11, 18, 2, 11, C.W3);
            s.Rect(10, 21, 12, 1, C.M0); s.Rect(10, 25, 12, 1, C.M0);
            s.Rect(12, 17, 8, 2, C.W1); s.Rect(13, 17, 6, 1, C.W0);
            s.Outline(C.Out);
            s.EllipseBehind(16, 29.5, 8, 1.6, C.Shadow);
            return s;
        }

        public static Spr Crate()
        {
            Spr s = new Spr(32, 32);
            s.Rect(9, 18, 14, 11, C.W2);
            s.Rect(9, 18, 14, 1, C.W3); s.Rect(9, 28, 14, 1, C.W1); s.Rect(9, 18, 1, 11, C.W1); s.Rect(22, 18, 1, 11, C.W1);
            s.Line(10, 27, 21, 19, C.W1); s.Line(10, 26, 20, 19, C.W1);
            s.Rect(23, 22, 4, 7, C.Rgb(196, 176, 130)); s.Rect(23, 21, 3, 1, C.Rgb(196, 176, 130)); s.Rect(26, 23, 1, 6, C.Rgb(160, 140, 100));
            s.Outline(C.Out);
            s.EllipseBehind(17, 29.5, 10, 1.6, C.Shadow);
            return s;
        }

        // 64x32 pond.
        public static Spr Pond()
        {
            Spr s = new Spr(64, 32);
            s.Ellipse(32, 16, 29, 12.5, C.SandD);
            s.Ellipse(31.5, 15.5, 28, 11.5, C.Sand);
            s.Ellipse(32, 16.5, 25, 9.5, C.WA0);
            s.Ellipse(32, 17.5, 24, 8.5, C.WA1);
            Random r = new Random(9);
            for (int i = 0; i < 9; i++)
            {
                int x = r.Next(14, 48), y = r.Next(12, 23);
                if (Spr.InEllipse(x, y, 32, 17.5, 22, 7)) { s.Rect(x, y, 4, 1, C.WA2); s.Px(x + 1, y, C.WA3); }
            }
            int[,] pads = { { 18, 18 }, { 40, 21 }, { 45, 14 } };
            for (int i = 0; i < 3; i++)
            {
                int x = pads[i, 0], y = pads[i, 1];
                s.Rect(x, y, 4, 2, C.TG1); s.Rect(x + 1, y - 1, 2, 1, C.TG2); s.Px(x + 3, y + 1, C.TG0);
                if (i == 1) { s.Px(x + 1, y - 1, C.Rgb(236, 150, 186)); s.Px(x + 2, y - 2, C.Rgb(250, 200, 220)); }
            }
            int[] reeds = { 6, 8, 9, 55, 57 };
            foreach (int x in reeds)
            {
                int h = 5 + x % 3;
                s.Rect(x, 18 - h, 1, h, C.GR0); s.Px(x, 18 - h, C.W1); s.Px(x, 17 - h, C.W2);
            }
            s.Outline(C.GR0);
            return s;
        }

        // ---------- main building (64x48), anchor: cell (32, 44) = gate threshold ----------
        public static Spr Castle(int stage)
        {
            Spr s = new Spr(64, 48);
            Random r = new Random(40);
            Bricks(s, 14, 10, 36, 35, r);
            Bricks(s, 2, 16, 14, 29, r);
            Bricks(s, 48, 16, 14, 29, r);
            // keep crenellations
            for (int x = 14; x < 50; x += 5) { s.Rect(x, 6, 3, 4, C.S1); s.Px(x, 6, C.S3); s.Rect(x + 2, 6, 1, 4, C.S0); }
            s.Rect(14, 10, 36, 1, C.S0);
            // side tower roofs
            Cone(s, 9, 2, 16);
            Cone(s, 55, 2, 16);
            s.Rect(2, 16, 14, 1, C.S0); s.Rect(48, 16, 14, 1, C.S0);
            // banners
            Banner(s, 18); Banner(s, 43);
            // gate
            Color dark = C.Rgb(40, 32, 28);
            s.Rect(25, 30, 14, 15, C.S3);
            s.Rect(27, 28, 10, 2, C.S3);
            s.Rect(26, 31, 12, 14, dark); s.Rect(28, 29, 8, 2, dark);
            for (int x = 27; x < 37; x += 3) s.Rect(x, 32, 2, 13, C.W1);
            s.Rect(26, 35, 12, 1, C.W0); s.Rect(26, 40, 12, 1, C.W0);
            for (int x = 27; x < 38; x += 2) s.Rect(x, 29, 1, 3, C.M0);
            s.Px(35, 38, C.Y1);
            // windows on keep and towers
            Window(s, 23, 15); Window(s, 39, 15);
            Window(s, 8, 24); Window(s, 54, 24);
            Window(s, 8, 34); Window(s, 54, 34);
            // steps
            s.Rect(23, 44, 18, 1, C.S2); s.Rect(24, 43, 16, 1, C.S3);
            // flag
            s.Rect(31, 0, 1, 7, C.W1);
            s.Rect(32, 1, 7, 4, C.R1); s.Rect(32, 4, 7, 1, C.R0); s.Px(39, 2, C.R1); s.Px(35, 2, C.Y1); s.Px(34, 2, C.Y2);

            if (stage >= 1)
            {
                Random d = new Random(77 + stage);
                Crack(s, 20, 20, 8, d); Crack(s, 44, 26, 7, d); Crack(s, 6, 30, 6, d); Crack(s, 52, 38, 5, d);
                for (int x = 39; x < 50; x++) for (int y = 6; y < 10; y++) s.Clear(x, y);
                for (int x = 48; x < 62; x++) for (int y = 0; y < 16; y++) if (y < 8 + (x * 7) % 5) s.Clear(x, y);
                Scorch(s, 46, 30); Scorch(s, 16, 38);
                Flame(s, 52, 12); Flame(s, 22, 6);
            }
            if (stage >= 2)
            {
                Random d = new Random(99);
                for (int x = 0; x < 64; x++)
                {
                    int cut = 14 + d.Next(0, 10) + (Math.Abs(x - 32) < 7 ? 6 : 0);
                    for (int y = 0; y < cut; y++) s.Clear(x, y);
                }
                Rubble(s, 6, 44, d); Rubble(s, 18, 45, d); Rubble(s, 46, 45, d); Rubble(s, 57, 44, d);
                Flame(s, 40, 22); Flame(s, 12, 24); Flame(s, 50, 20);
            }
            s.Outline(C.Out);
            s.EllipseBehind(32, 45.5, 31, 2.5, C.Shadow);
            if (stage >= 1) Smoke(s, stage == 1 ? 52 : 40, stage == 1 ? 10 : 18);
            if (stage >= 2) Smoke(s, 14, 20);
            return s;
        }

        static void Bricks(Spr s, int x0, int y0, int w, int h, Random r)
        {
            Color mortar = C.Rgb(112, 104, 92);
            for (int y = y0; y < y0 + h; y++)
                for (int x = x0; x < x0 + w; x++)
                {
                    int row = (y - y0) / 3, off = (row % 2) * 3;
                    bool m = (y - y0) % 3 == 2 || (x - x0 + off) % 6 == 5;
                    Color c = m ? mortar : (((x - x0 + off) / 6 * 7 + row * 3) % 5 == 0 ? C.S2 : C.S1);
                    if (!m && (y - y0) % 3 == 0 && (x - x0 + off) % 6 == 0) c = C.S3;
                    if (x == x0) c = C.S2;
                    if (x == x0 + w - 1) c = C.S0;
                    s.Px(x, y, c);
                }
        }

        static void Cone(Spr s, int cx, int top, int bottom)
        {
            for (int y = top; y < bottom; y++)
            {
                double half = 1 + (y - top) * 7.5 / (bottom - top);
                for (int x = (int)Math.Round(cx - half); x < (int)Math.Round(cx + half); x++)
                {
                    Color c = x < cx - half * 0.4 ? C.R2 : (x < cx + half * 0.35 ? C.R1 : C.R0);
                    if ((y - top) % 4 == 3) c = C.Dark(c, 0.2);
                    s.Px(x, y, c);
                }
            }
            s.Px(cx - 1, top - 1, C.Y1); s.Px(cx - 1, top - 2, C.Y2);
        }

        static void Banner(Spr s, int x)
        {
            s.Rect(x, 12, 4, 10, C.R1); s.Rect(x + 3, 12, 1, 10, C.R0);
            s.Px(x, 22, C.R1); s.Px(x + 3, 22, C.R0); s.Px(x + 1, 21, C.R0); s.Px(x + 2, 21, C.R0);
            s.Rect(x + 1, 15, 2, 3, C.Y1); s.Px(x + 1, 15, C.Y2);
            s.Rect(x - 1, 11, 6, 1, C.W1);
        }

        static void Window(Spr s, int x, int y)
        {
            s.Rect(x, y + 1, 2, 4, C.Rgb(40, 32, 28)); s.Px(x, y + 1, C.Rgb(60, 50, 40)); s.Px(x + 1, y + 3, C.Y1);
            s.Rect(x - 1, y + 5, 4, 1, C.S3);
        }

        static void Crack(Spr s, int x, int y, int len, Random r)
        {
            for (int i = 0; i < len; i++)
            {
                if (s.Solid(x, y)) s.Px(x, y, C.Out);
                y++; x += r.Next(-1, 2);
            }
        }

        static void Scorch(Spr s, int x, int y)
        {
            for (int yy = -2; yy <= 2; yy++)
                for (int xx = -3; xx <= 3; xx++)
                    if (Spr.InEllipse(x + xx, y + yy, x + 0.5, y + 0.5, 3.5, 2.5) && s.Solid(x + xx, y + yy))
                        s.Px(x + xx, y + yy, C.Argb(140, 40, 30, 26));
        }

        static void Flame(Spr s, int x, int y)
        {
            s.Rect(x - 1, y, 3, 3, C.Fire1); s.Px(x, y - 1, C.Fire1); s.Px(x, y - 2, C.Fire2); s.Px(x - 1, y - 1, C.Fire2);
            s.Px(x, y + 1, C.Fire3); s.Px(x, y, C.Fire2); s.Px(x + 1, y + 2, C.Fire0); s.Px(x - 1, y + 2, C.Fire0);
        }

        static void Smoke(Spr s, int x, int y)
        {
            Color sm = C.Argb(150, 96, 96, 100), sl = C.Argb(110, 150, 150, 156);
            s.Ellipse(x + 0.5, y - 3, 2.5, 2, sm); s.Ellipse(x + 2.5, y - 7, 3, 2.2, sl); s.Ellipse(x + 1.5, y - 11, 2.2, 1.8, C.Argb(80, 170, 170, 176));
        }

        static void Rubble(Spr s, int x, int y, Random r)
        {
            s.Ellipse(x, y - 1, 4, 2.5, C.S0); s.Ellipse(x - 0.5, y - 1.5, 3, 1.8, C.S1); s.Px(x - 2, y - 3, C.S3); s.Px(x + 1, y - 2, C.S2);
        }

    }
}


namespace TowerDefenseSprites
{
    public class Hum
    {
        public int Cx = 8, Ground = 13;
        public int LegH = 3, LegW = 2, LegGap = 1;
        public int TorsoW = 6, TorsoH = 4;
        public int HeadW = 5, HeadH = 4;
        public int ArmW = 1, ArmH = 3;
        public Color Skin = C.Skin, SkinD = C.SkinD, Body = C.W2, BodyD = C.W1, Legs = C.W1, LegsD = C.W0, Feet = C.W0, Eye = C.Out2;
        public Color Hair = Color.Empty;
        public bool Ears, TwoEyes, BigEyes, Sleeves = true;
        // computed
        public int HeadX, HeadY, TorsoX, TorsoY, FrontX, FrontY, BackX, BackY;
    }

    public static class Creatures
    {
        public static void DrawHum(Spr s, Hum h, int f)
        {
            int bob = f == 1 ? 1 : 0, stride = f == 1 ? 1 : 0;
            int legTop = h.Ground - h.LegH + 1;
            int lx = h.Cx - h.LegGap / 2 - h.LegW;
            int rx = h.Cx + (h.LegGap - h.LegGap / 2);
            s.Rect(lx - stride, legTop - bob, h.LegW, h.LegH + bob, h.LegsD);
            s.Rect(lx - stride, h.Ground, h.LegW, 1, h.Feet);
            s.Rect(rx + stride, legTop - bob, h.LegW, h.LegH + bob, h.Legs);
            s.Rect(rx + stride, h.Ground, h.LegW + 1, 1, h.Feet);

            int ty = legTop - h.TorsoH - bob;
            int tx = h.Cx - h.TorsoW / 2;
            h.TorsoX = tx; h.TorsoY = ty;
            // back arm
            Color armBack = h.Sleeves ? h.BodyD : h.SkinD;
            s.Rect(tx - h.ArmW, ty + 1, h.ArmW, h.ArmH - 1, armBack);
            s.Rect(tx - h.ArmW, ty + h.ArmH, h.ArmW, 1, h.SkinD);
            h.BackX = tx - h.ArmW; h.BackY = ty + h.ArmH;
            // torso
            s.Rect(tx, ty, h.TorsoW, h.TorsoH, h.Body);
            if (h.TorsoW >= 8) s.Rect(tx, ty, 1, h.TorsoH, C.Light(h.Body, 0.18));
            s.Rect(tx + h.TorsoW - 1, ty, 1, h.TorsoH, h.BodyD);
            s.Rect(tx, ty + h.TorsoH - 1, h.TorsoW, 1, h.BodyD);
            // front arm
            int swing = f == 1 ? 1 : 0;
            Color armFront = h.Sleeves ? h.Body : h.Skin;
            s.Rect(tx + h.TorsoW, ty + 1 + swing, h.ArmW, h.ArmH - 1, armFront);
            s.Rect(tx + h.TorsoW, ty + h.ArmH + swing, h.ArmW, 1, h.Skin);
            h.FrontX = tx + h.TorsoW + h.ArmW - 1; h.FrontY = ty + h.ArmH + swing;
            // head
            int hx = h.Cx - h.HeadW / 2 + 1, hy = ty - h.HeadH;
            h.HeadX = hx; h.HeadY = hy;
            s.Rect(hx, hy, h.HeadW, h.HeadH, h.Skin);
            s.Rect(hx, hy, 1, h.HeadH, h.SkinD);
            if (h.Hair != Color.Empty)
            {
                s.Rect(hx, hy, h.HeadW, Math.Max(1, h.HeadH / 3), h.Hair);
                s.Rect(hx, hy, Math.Max(1, h.HeadW / 3), h.HeadH - 1, h.Hair);
            }
            int ey = hy + h.HeadH / 2;
            if (h.BigEyes)
            {
                s.Rect(hx + h.HeadW - 3, ey - 1, 2, 2, h.Eye);
                if (h.TwoEyes) s.Rect(hx + h.HeadW - 6, ey - 1, 2, 2, h.Eye);
            }
            else
            {
                s.Px(hx + h.HeadW - 2, ey, h.Eye);
                if (h.TwoEyes) s.Px(hx + h.HeadW - 4, ey, h.Eye);
            }
            if (h.Ears)
            {
                s.Px(hx - 1, hy + 1, h.Skin); s.Px(hx - 2, hy, h.Skin);
                if (h.HeadW >= 8) { s.Px(hx - 1, hy + 2, h.Skin); s.Px(hx - 3, hy - 1, h.Skin); s.Px(hx - 2, hy + 1, h.SkinD); }
            }
        }

        static void Finish(Spr s, int ground, bool flying)
        {
            s.Outline(C.Out);
            int cx = s.W / 2;
            double k = s.W / 16.0;
            if (flying) s.EllipseBehind(cx, s.H - 1, 3.5 * k, 1 * k, C.ShadowL);
            else s.EllipseBehind(cx, ground + 1.2, 5 * k, 1.2 * k, C.Shadow);
        }

        // ---------------- standard enemies (16x16, 2 frames) ----------------
        public static readonly string[] EnemyNames =
        {
            "Goblin", "Goblin Archer", "Goblin Shaman", "Orc", "Orc Berserker", "Orc Warlord",
            "Skeleton", "Skeleton Knight", "Troll", "Stone Golem", "Fire Elemental", "Ice Elemental",
            "Storm Elemental", "Earth Elemental", "Dark Elf", "Vampire", "Wyvern", "Fire Golem",
            "Dragon", "Arcane Titan"
        };

        static Hum Goblin()
        {
            Hum h = new Hum();
            h.LegH = 2; h.LegW = 2; h.LegGap = 0; h.TorsoW = 4; h.TorsoH = 3; h.HeadW = 5; h.HeadH = 4; h.ArmH = 2;
            h.Skin = C.Rgb(118, 170, 74); h.SkinD = C.Rgb(84, 128, 56); h.Ears = true;
            h.Body = C.W2; h.BodyD = C.W1;
            return h;
        }

        static Hum Orc()
        {
            Hum h = new Hum();
            h.ArmW = 2; h.HeadW = 5; h.HeadH = 4;
            h.Skin = C.Rgb(102, 146, 72); h.SkinD = C.Rgb(74, 110, 52);
            h.Body = C.W1; h.BodyD = C.W0; h.Legs = C.Rgb(76, 64, 54); h.LegsD = C.Rgb(56, 46, 40); h.Feet = C.Out2;
            return h;
        }

        static Hum Skeleton()
        {
            Hum h = new Hum();
            h.LegW = 1; h.LegGap = 2; h.TorsoW = 5; h.HeadW = 5; h.HeadH = 4;
            h.Skin = C.B2; h.SkinD = C.B1; h.Body = C.B1; h.BodyD = C.B0; h.Legs = C.B1; h.LegsD = C.B0; h.Feet = C.B0;
            h.Sleeves = false;
            return h;
        }

        static Hum Big()
        {
            Hum h = new Hum();
            h.LegH = 3; h.LegW = 3; h.LegGap = 1; h.TorsoW = 9; h.TorsoH = 5; h.HeadW = 5; h.HeadH = 4; h.ArmW = 2; h.ArmH = 5;
            return h;
        }

        static void Ribs(Spr s, Hum h)
        {
            for (int y = h.TorsoY + 1; y < h.TorsoY + h.TorsoH - 1; y += 2)
                s.Rect(h.TorsoX + 1, y, h.TorsoW - 2, 1, C.Rgb(70, 62, 54));
            s.Px(h.TorsoX + h.TorsoW / 2, h.TorsoY + 1, C.B1);
            s.Px(h.HeadX + h.HeadW - 2, h.HeadY + h.HeadH - 1, C.Out2);
        }

        public static Spr Enemy(int index, int f)
        {
            Spr s = new Spr(16, 16);
            Hum h;
            bool flying = false;
            switch (index)
            {
                case 0: // Goblin
                    h = Goblin(); DrawHum(s, h, f);
                    s.Rect(h.FrontX + 1, h.FrontY - 3, 1, 3, C.M2); s.Px(h.FrontX + 1, h.FrontY, C.W1); s.Px(h.FrontX, h.FrontY - 1, C.M1);
                    break;
                case 1: // Goblin Archer
                    h = Goblin(); h.Hair = C.Rgb(56, 96, 48); h.Body = C.Rgb(92, 120, 62); h.BodyD = C.Rgb(68, 92, 48);
                    DrawHum(s, h, f);
                    { int bx = h.FrontX + 1, by = h.FrontY; s.Rect(bx, by - 4, 1, 6, C.B2); s.Px(bx + 1, by - 5, C.W2); s.Rect(bx + 2, by - 4, 1, 6, C.W2); s.Px(bx + 1, by + 2, C.W2); s.Px(bx - 2, by - 6, C.W3); s.Px(bx - 3, by - 7, C.R2); }
                    break;
                case 2: // Goblin Shaman
                    h = Goblin(); h.Body = C.Purple1; h.BodyD = C.Purple0; h.Legs = C.Purple1; h.LegsD = C.Purple0;
                    DrawHum(s, h, f);
                    s.Rect(h.FrontX + 1, h.FrontY - 6, 1, 7, C.W1);
                    s.Rect(h.FrontX, h.FrontY - 8, 3, 2, C.B2); s.Px(h.FrontX + 1, h.FrontY - 7, C.Out2); s.Px(h.FrontX + 3, h.FrontY - 9, C.Poison2);
                    s.Px(h.HeadX + 1, h.HeadY - 1, C.R2); s.Px(h.HeadX + 2, h.HeadY - 2, C.Y2); s.Px(h.HeadX + 3, h.HeadY - 1, C.Rgb(80, 170, 190));
                    break;
                case 3: // Orc
                    h = Orc(); DrawHum(s, h, f);
                    s.Px(h.HeadX + h.HeadW - 1, h.HeadY + h.HeadH - 1, C.B2); s.Px(h.HeadX + 1, h.HeadY - 1, C.Out2);
                    s.Rect(h.TorsoX, h.TorsoY + h.TorsoH - 2, h.TorsoW, 1, C.W3);
                    s.Rect(h.FrontX + 1, h.FrontY - 5, 1, 6, C.W1); s.Rect(h.FrontX + 2, h.FrontY - 6, 2, 3, C.M1); s.Px(h.FrontX + 3, h.FrontY - 6, C.M2);
                    break;
                case 4: // Orc Berserker
                    h = Orc(); h.Skin = C.Rgb(116, 156, 74); h.Body = h.Skin; h.BodyD = h.SkinD; h.Sleeves = false;
                    DrawHum(s, h, f);
                    s.Rect(h.HeadX, h.HeadY + 1, h.HeadW, 1, C.R1); s.Px(h.HeadX - 1, h.HeadY + 2, C.R1); s.Px(h.HeadX - 2, h.HeadY + 3, C.R0);
                    s.Px(h.HeadX + h.HeadW - 1, h.HeadY + h.HeadH - 1, C.B2);
                    s.Rect(h.TorsoX + 1, h.TorsoY + 1, 1, 2, C.R0);
                    s.Rect(h.FrontX + 1, h.FrontY - 4, 1, 5, C.W1); s.Rect(h.FrontX + 2, h.FrontY - 5, 2, 3, C.M2);
                    s.Rect(h.BackX - 1, h.BackY - 3, 1, 4, C.W1); s.Rect(h.BackX - 3, h.BackY - 4, 2, 2, C.M1);
                    break;
                case 5: // Orc Warlord
                    h = Orc(); h.TorsoW = 7; h.Body = C.M1; h.BodyD = C.M0; h.Legs = C.M0; h.LegsD = C.Rgb(60, 66, 76);
                    DrawHum(s, h, f);
                    s.RectBehind(h.TorsoX - 2, h.TorsoY + 1, 2, 7, C.R1);
                    s.Rect(h.HeadX, h.HeadY, h.HeadW, 2, C.M1); s.Px(h.HeadX - 1, h.HeadY - 1, C.B2); s.Px(h.HeadX + h.HeadW, h.HeadY - 1, C.B2); s.Px(h.HeadX + h.HeadW, h.HeadY - 2, C.B2);
                    s.Px(h.HeadX + h.HeadW - 1, h.HeadY + h.HeadH - 1, C.B2);
                    s.Rect(h.TorsoX + 2, h.TorsoY + 1, 2, 1, C.Y1);
                    s.Rect(h.FrontX + 1, h.FrontY - 6, 2, 6, C.M2); s.Rect(h.FrontX + 2, h.FrontY - 6, 1, 6, C.M1); s.Px(h.FrontX + 1, h.FrontY, C.Y1);
                    break;
                case 6: // Skeleton
                    h = Skeleton(); DrawHum(s, h, f); Ribs(s, h);
                    s.Rect(h.FrontX + 1, h.FrontY - 5, 1, 5, C.Rgb(168, 128, 96)); s.Px(h.FrontX, h.FrontY - 1, C.W1); s.Px(h.FrontX + 2, h.FrontY - 1, C.W1);
                    break;
                case 7: // Skeleton Knight
                    h = Skeleton(); DrawHum(s, h, f); Ribs(s, h);
                    s.Rect(h.HeadX, h.HeadY, h.HeadW, 2, C.M1); s.Px(h.HeadX + h.HeadW - 1, h.HeadY + 2, C.M1); s.Px(h.HeadX + 1, h.HeadY - 1, C.R1); s.Px(h.HeadX + 2, h.HeadY - 1, C.R1);
                    s.Rect(h.FrontX, h.FrontY - 4, 4, 6, C.Rgb(60, 82, 146)); s.Rect(h.FrontX + 3, h.FrontY - 4, 1, 6, C.Rgb(42, 58, 108));
                    s.Rect(h.FrontX + 1, h.FrontY - 2, 2, 2, C.Y1); s.Rect(h.FrontX, h.FrontY - 4, 4, 1, C.M2);
                    break;
                case 8: // Troll
                    h = Big(); h.Skin = C.Rgb(124, 142, 132); h.SkinD = C.Rgb(92, 106, 100); h.Body = h.Skin; h.BodyD = h.SkinD; h.Sleeves = false;
                    h.Legs = h.Skin; h.LegsD = h.SkinD; h.Feet = h.SkinD; h.HeadW = 5; h.HeadH = 4;
                    DrawHum(s, h, f);
                    s.Rect(h.TorsoX, h.TorsoY + h.TorsoH - 2, h.TorsoW, 2, C.W1); s.Px(h.TorsoX + 3, h.TorsoY + h.TorsoH - 1, C.W2);
                    s.Px(h.HeadX + h.HeadW - 1, h.HeadY + h.HeadH - 1, C.B2); s.Px(h.HeadX + h.HeadW, h.HeadY + 1, h.Skin);
                    s.Rect(h.FrontX + 1, h.FrontY - 6, 1, 7, C.W1); s.Rect(h.FrontX + 1, h.FrontY - 9, 2, 4, C.W2); s.Px(h.FrontX + 2, h.FrontY - 8, C.W3);
                    break;
                case 9: // Stone Golem
                    h = Big(); h.Skin = C.S2; h.SkinD = C.S1; h.Body = C.S1; h.BodyD = C.S0; h.Legs = C.S1; h.LegsD = C.S0; h.Feet = C.S0;
                    h.Sleeves = false; h.ArmW = 3; h.Eye = C.Rgb(120, 230, 240); h.TwoEyes = true; h.HeadW = 5;
                    DrawHum(s, h, f);
                    s.Px(h.TorsoX + 3, h.TorsoY + 1, C.S0); s.Px(h.TorsoX + 4, h.TorsoY + 2, C.S0); s.Px(h.TorsoX + 6, h.TorsoY + 3, C.S0);
                    s.Px(h.TorsoX, h.TorsoY - 1, C.GR3); s.Px(h.TorsoX + 1, h.TorsoY - 1, C.GR2); s.Px(h.TorsoX + h.TorsoW + 1, h.TorsoY, C.GR3);
                    break;
                case 10: // Fire Elemental
                    Elemental(s, f, 16, C.Fire0, C.Fire1, C.Fire2, C.Fire3, 0);
                    flying = true;
                    break;
                case 11: // Ice Elemental
                    Elemental(s, f, 16, C.Ice0, C.Ice1, C.Ice2, C.B2, 1);
                    flying = true;
                    break;
                case 12: // Storm Elemental
                    Elemental(s, f, 16, C.Storm0, C.Storm1, C.Storm2, C.Y2, 2);
                    flying = true;
                    break;
                case 13: // Earth Elemental
                    Elemental(s, f, 16, C.W0, C.Rgb(122, 92, 62), C.S2, C.GR3, 3);
                    break;
                case 14: // Dark Elf
                    h = new Hum(); h.TorsoW = 5; h.HeadW = 5; h.HeadH = 4; h.LegW = 1; h.LegGap = 2; h.Ears = true;
                    h.Skin = C.Rgb(130, 116, 172); h.SkinD = C.Rgb(98, 86, 136); h.Hair = C.Rgb(228, 228, 238);
                    h.Body = C.Rgb(62, 46, 86); h.BodyD = C.Rgb(42, 30, 60); h.Legs = C.Rgb(52, 40, 72); h.LegsD = C.Rgb(36, 28, 50); h.Feet = C.Out2;
                    h.Eye = C.R2;
                    DrawHum(s, h, f);
                    s.Rect(h.HeadX, h.HeadY + h.HeadH - 1, 1, 3, h.Hair);
                    s.Px(h.FrontX + 1, h.FrontY - 1, C.M2); s.Px(h.FrontX + 2, h.FrontY - 2, C.M2); s.Px(h.FrontX + 2, h.FrontY - 3, C.M2); s.Px(h.FrontX + 1, h.FrontY - 4, C.M1);
                    s.Px(h.TorsoX + 2, h.TorsoY + 1, C.Purple2);
                    break;
                case 15: // Vampire
                    h = new Hum(); h.TorsoW = 5; h.HeadW = 5; h.HeadH = 4;
                    h.Skin = C.Rgb(226, 214, 214); h.SkinD = C.Rgb(186, 172, 178); h.Hair = C.Rgb(34, 28, 36); h.Eye = C.R1;
                    h.Body = C.Rgb(44, 36, 50); h.BodyD = C.Rgb(30, 24, 34); h.Legs = C.Rgb(40, 32, 46); h.LegsD = C.Rgb(28, 22, 32); h.Feet = C.Out2;
                    DrawHum(s, h, f);
                    s.RectBehind(h.TorsoX - 3, h.TorsoY, 3, 9 - f, C.Rgb(34, 28, 40)); s.RectBehind(h.TorsoX - 1, h.TorsoY + 1, 1, 7 - f, C.R0);
                    s.Px(h.HeadX - 1, h.HeadY + h.HeadH - 1, C.R1); s.Px(h.HeadX - 1, h.HeadY + h.HeadH - 2, C.R1);
                    s.Px(h.TorsoX + 2, h.TorsoY, C.B2); s.Px(h.TorsoX + 2, h.TorsoY + 1, C.R1);
                    s.Px(h.HeadX + h.HeadW - 2, h.HeadY + h.HeadH - 1, C.B2);
                    break;
                case 16: // Wyvern
                    Dragon(s, f, 1, C.Rgb(52, 100, 80), C.Rgb(76, 136, 104), C.Rgb(184, 204, 140), C.Rgb(44, 82, 70), false, false);
                    flying = true;
                    break;
                case 17: // Fire Golem
                    h = Big(); Color r0 = C.Rgb(70, 54, 48), r1 = C.Rgb(98, 76, 66);
                    h.Skin = r1; h.SkinD = r0; h.Body = r1; h.BodyD = r0; h.Legs = r1; h.LegsD = r0; h.Feet = r0;
                    h.Sleeves = false; h.ArmW = 3; h.Eye = C.Y2; h.TwoEyes = true;
                    DrawHum(s, h, f);
                    s.Line(h.TorsoX + 2, h.TorsoY + 1, h.TorsoX + 4, h.TorsoY + 3, C.Fire1); s.Px(h.TorsoX + 3, h.TorsoY + 2, C.Fire2);
                    s.Line(h.TorsoX + 6, h.TorsoY, h.TorsoX + 5, h.TorsoY + 2, C.Fire1);
                    s.Px(h.FrontX, h.FrontY - 2, C.Fire1); s.Px(h.BackX, h.BackY - 2, C.Fire1);
                    s.Px(h.HeadX + 2, h.HeadY - 1, C.Fire1); s.Px(h.HeadX + 3, h.HeadY - 2 + f, C.Fire2);
                    break;
                case 18: // Dragon
                    Dragon(s, f, 1, C.R0, C.R1, C.Rgb(232, 184, 98), C.Rgb(96, 28, 26), true, true);
                    flying = true;
                    break;
                default: // Arcane Titan
                    h = Big(); h.Skin = C.Rgb(84, 96, 178); h.SkinD = C.Rgb(60, 68, 132); h.Body = C.Rgb(76, 88, 170); h.BodyD = C.Rgb(52, 60, 124);
                    h.Legs = h.Body; h.LegsD = h.BodyD; h.Feet = C.Y0; h.Sleeves = true; h.Eye = C.Rgb(130, 240, 250);
                    DrawHum(s, h, f);
                    s.Rect(h.HeadX + 1, h.HeadY + 1, h.HeadW - 1, 1, C.Rgb(130, 240, 250));
                    s.Px(h.HeadX + 2, h.HeadY - 1, C.Y1);
                    s.Rect(h.TorsoX, h.TorsoY + h.TorsoH - 2, h.TorsoW, 1, C.Y1);
                    s.Px(h.TorsoX - 1, h.TorsoY, C.Y1); s.Px(h.TorsoX + h.TorsoW, h.TorsoY, C.Y1);
                    s.Px(h.TorsoX + 3, h.TorsoY + 1, C.Rgb(130, 240, 250)); s.Px(h.TorsoX + 5, h.TorsoY + 2, C.Rgb(130, 240, 250));
                    s.Rect(h.FrontX + 1, h.FrontY - 2, 2, 2, C.Purple2); s.Px(h.FrontX + 1, h.FrontY - 2, C.B2);
                    break;
            }
            Finish(s, 13, flying);
            return s;
        }

        // shape: 0 fire, 1 ice, 2 storm, 3 earth. Size 16 or 32.
        static void Elemental(Spr s, int f, int size, Color c0, Color c1, Color c2, Color c3, int shape)
        {
            double k = size / 16.0;
            if (shape == 0)
            {
                s.Ellipse(8 * k, 10 * k, 4.5 * k, 4 * k, c0);
                int[] tipsX = { 5, 8, 11 };
                int[] tipsY0 = { 4, 1, 5 }, tipsY1 = { 3, 3, 3 };
                for (int i = 0; i < 3; i++)
                {
                    double tx = (tipsX[i] + (f == 1 && i == 1 ? -1 : 0)) * k, ty = (f == 0 ? tipsY0[i] : tipsY1[i]) * k;
                    s.Poly(new[] { tx, ty, 8 * k - 3 * k + i * 3 * k - 1.5 * k, 9 * k, 8 * k - 3 * k + i * 3 * k + 1.5 * k, 9 * k }, c0);
                    s.Poly(new[] { tx, ty + 1.5 * k, 8 * k - 3 * k + i * 3 * k - 0.7 * k, 9 * k, 8 * k - 3 * k + i * 3 * k + 0.7 * k, 9 * k }, c1);
                }
                s.Ellipse(8 * k, 10.5 * k, 3.6 * k, 3.2 * k, c1);
                s.Ellipse(8 * k, 11 * k, 2.4 * k, 2 * k, c2);
                s.Ellipse(8 * k, 11.5 * k, 1.2 * k, 1 * k, c3);
                EyePair(s, 9 * k, 9 * k, k, C.Out2);
                s.Px((int)(3 * k), (int)((6 + f) * k), c2); s.Px((int)(13 * k), (int)((8 - f) * k), c2);
            }
            else if (shape == 1)
            {
                double b = f == 1 ? -0.5 * k : 0;
                s.Poly(new[] { 8 * k, 1 * k + b, 12.5 * k, 7 * k + b, 8 * k, 13.5 * k + b, 3.5 * k, 7 * k + b }, c1);
                s.Poly(new[] { 8 * k, 1 * k + b, 8 * k, 13.5 * k + b, 3.5 * k, 7 * k + b }, c2);
                s.Line((int)(8 * k), (int)(2 * k + b), (int)(8 * k), (int)(12 * k + b), c0);
                s.Poly(new[] { 2 * k, 9 * k - b, 4.5 * k, 8 * k - b, 4 * k, 13 * k - b }, c1);
                s.Poly(new[] { 14 * k, 9 * k - b, 11.5 * k, 8 * k - b, 12 * k, 13 * k - b }, c0);
                s.Px((int)(6 * k), (int)(4 * k + b), c3);
                EyePair(s, 9.5 * k, 6.5 * k + b, k, C.Rgb(30, 60, 110));
            }
            else if (shape == 2)
            {
                s.Ellipse(6 * k, 7 * k, 3.5 * k, 3 * k, c1); s.Ellipse(10 * k, 6 * k, 4 * k, 3.5 * k, c1); s.Ellipse(8 * k, 9 * k, 5.5 * k, 3 * k, c1);
                s.Ellipse(9 * k, 10.5 * k, 4.5 * k, 1.5 * k, c0);
                s.Ellipse(8.5 * k, 4.5 * k, 2.5 * k, 1.5 * k, c2); s.Ellipse(4.5 * k, 6.5 * k, 1.5 * k, 1.2 * k, c2);
                EyePair(s, 10 * k, 7.5 * k, k, c3);
                int bx = (int)((f == 0 ? 7 : 10) * k);
                int by = (int)(11 * k);
                s.Line(bx, by, bx - (int)k, by + (int)(2 * k), c3); s.Line(bx - (int)k, by + (int)(2 * k), bx + (int)k, by + (int)(2 * k), c3); s.Line(bx + (int)k, by + (int)(2 * k), bx, by + (int)(4 * k), c3);
            }
            else
            {
                int g = (int)(13 * k);
                int st = f == 1 ? (int)k : 0;
                s.Rect((int)(5 * k) - st, g - (int)(2 * k), (int)(2 * k), (int)(3 * k), c0);
                s.Rect((int)(9 * k) + st, g - (int)(2 * k), (int)(2 * k), (int)(3 * k), c1);
                s.Ellipse(8 * k, 8.5 * k - st * 0.5, 6 * k, 4.5 * k, c1);
                s.Ellipse(7 * k, 7 * k - st * 0.5, 4 * k, 2.5 * k, C.Light(c1, 0.15));
                s.Ellipse(5 * k, 6 * k, 2 * k, 1.5 * k, c2); s.Ellipse(11 * k, 5.5 * k, 2 * k, 1.6 * k, C.S1); s.Px((int)(11 * k), (int)(5 * k), c2);
                s.Px((int)(7 * k), (int)(4 * k), c3); s.Px((int)(8 * k), (int)(3 * k), c3); s.Px((int)(6 * k), (int)(3 * k), C.GR2); s.Px((int)(9 * k), (int)(4 * k), C.GR2);
                EyePair(s, 10 * k, 8 * k, k, C.Y1);
                s.Px((int)(12 * k), (int)(10 * k), c0);
            }
        }

        static void EyePair(Spr s, double x, double y, double k, Color c)
        {
            int sz = Math.Max(1, (int)k);
            s.Rect((int)x, (int)y, sz, sz, c);
            s.Rect((int)(x + 2 * k), (int)y, sz, sz, c);
        }

        // k = 1 for 16x16, 2 for 32x32. Facing right.
        static void Dragon(Spr s, int f, int k, Color dark, Color body, Color belly, Color wing, bool horns, bool legs)
        {
            Func<double, double, double[]> P = (x, y) => new[] { x * k, y * k };
            // far wing
            if (f == 0) s.Poly(Concat(P(6, 8), P(9, 8), P(8, 1), P(5, 2), P(3, 4)), wing);
            else s.Poly(Concat(P(6, 9), P(9, 9), P(7, 14), P(4, 13)), wing);
            // tail
            s.Poly(Concat(P(4, 8), P(5, 11), P(1, 12), P(1, 11)), body);
            s.Poly(Concat(P(1, 11), P(1, 12.5), P(0.2, 10.5)), dark);
            // body
            s.Ellipse(7.5 * k, 9.5 * k, 4.2 * k, 2.6 * k, body);
            s.Ellipse(7.5 * k, 11 * k, 3.2 * k, 1.1 * k, belly);
            // neck & head
            s.Poly(Concat(P(9.5, 8), P(11, 5), P(12.5, 5), P(12, 9)), body);
            s.Rect(11 * k, 4 * k, 3 * k, 2 * k, body);
            s.Rect(13 * k, 5 * k, 2 * k, k, body);
            s.Rect(13 * k, 6 * k, 2 * k, k, belly);
            s.Rect(12 * k, 4 * k, k, k, C.Y2);
            if (k == 2) s.Px(13 * k, 5 * k - 1, C.Out2);
            if (horns) { s.Px(11 * k, 3 * k, C.B2); s.Px(10 * k, 2 * k, C.B2); if (k == 2) { s.Rect(10 * k, 2 * k, 2, 3, C.B2); s.Rect(19, 5, 2, 2, C.B1); } }
            // legs
            if (legs)
            {
                int st = f == 1 ? k : 0;
                s.Rect(5 * k - st, 11 * k, k, 2 * k, dark); s.Rect(9 * k + st, 11 * k, k, 2 * k, dark);
                s.Rect(9 * k + st, 13 * k - (k == 2 ? 1 : 0), k + 1, 1, C.B1);
            }
            else
            {
                s.Px(7 * k, 12 * k, dark); s.Px(9 * k, 12 * k, dark);
            }
            // near wing
            if (f == 0)
            {
                s.Poly(Concat(P(5, 8.5), P(9, 8.5), P(11, 2), P(9, 0.5), P(6, 2.5)), body);
                s.Line(9 * k, 8 * k, 10 * k, 2 * k, dark); s.Line(7 * k, 8 * k, 8 * k, 2 * k, dark);
            }
            else
            {
                s.Poly(Concat(P(5, 9), P(9, 9), P(9, 15), P(6, 14.5), P(4, 13)), body);
                s.Line(8 * k, 10 * k, 8 * k, 14 * k, dark); s.Line(6 * k, 10 * k, 5 * k, 13 * k, dark);
            }
            // spine
            for (int i = 0; i < 3; i++) s.Px((5 + i * 2) * k, 7 * k - (i == 1 ? 0 : 0), C.Light(body, 0.25));
        }

        static double[] Concat(params double[][] pts)
        {
            List<double> l = new List<double>();
            foreach (double[] p in pts) l.AddRange(p);
            return l.ToArray();
        }

        // ---------------- bosses (32x32, 2 frames) ----------------
        public static readonly string[] BossNames =
        {
            "Skeleton King", "Orc Lord", "Elemental God", "Vampire Lord", "Elder Dragon",
            "Fire Dragon", "Skeleton Mage", "Dark Elf King", "All Elements Golem", "Goblin Lord"
        };

        static Hum BigHum()
        {
            Hum h = new Hum();
            h.Cx = 16; h.Ground = 28; h.LegH = 6; h.LegW = 3; h.LegGap = 2; h.TorsoW = 11; h.TorsoH = 8;
            h.HeadW = 9; h.HeadH = 8; h.ArmW = 3; h.ArmH = 8; h.TwoEyes = true; h.BigEyes = true;
            return h;
        }

        static void Crown(Spr s, int x, int y, int w)
        {
            s.Rect(x, y, w, 2, C.Y1); s.Rect(x, y + 1, w, 1, C.Y0);
            for (int i = 0; i < w; i += 3) { s.Px(x + i, y - 1, C.Y1); s.Px(x + i, y - 2, C.Y2); }
            s.Px(x + w / 2, y, C.R2);
        }

        static void Cape(Spr s, Hum h, Color c, Color lining, int f)
        {
            int len = h.TorsoH + h.LegH - 1 - f;
            s.RectBehind(h.TorsoX - 4, h.TorsoY, 4, len, c);
            s.RectBehind(h.TorsoX - 5, h.TorsoY + 3, 1, len - 3, c);
            s.RectBehind(h.TorsoX - 1, h.TorsoY + 1, 1, len - 2, lining);
        }

        public static Spr Boss(int index, int f)
        {
            Spr s = new Spr(32, 32);
            Hum h;
            bool flying = false;
            switch (index)
            {
                case 0: // Skeleton King
                    h = BigHum(); h.LegW = 2; h.LegGap = 4; h.TorsoW = 9; h.Skin = C.B2; h.SkinD = C.B1; h.Body = C.B1; h.BodyD = C.B0; h.Legs = C.B1; h.LegsD = C.B0; h.Feet = C.B0; h.Sleeves = false; h.Eye = C.Rgb(120, 230, 240); h.ArmW = 2;
                    DrawHum(s, h, f);
                    for (int y = h.TorsoY + 1; y < h.TorsoY + h.TorsoH - 1; y += 2) s.Rect(h.TorsoX + 1, y, h.TorsoW - 2, 1, C.Rgb(70, 62, 54));
                    s.Rect(h.TorsoX + 4, h.TorsoY, 1, h.TorsoH, C.B1);
                    s.Rect(h.HeadX + 4, h.HeadY + h.HeadH - 2, 4, 1, C.Out2); s.Px(h.HeadX + 5, h.HeadY + h.HeadH - 1, C.B1); s.Px(h.HeadX + 7, h.HeadY + h.HeadH - 1, C.B1);
                    Cape(s, h, C.Purple1, C.Purple0, f);
                    Crown(s, h.HeadX, h.HeadY - 1, h.HeadW);
                    s.Rect(h.FrontX + 1, h.FrontY - 13, 2, 13, C.M2); s.Rect(h.FrontX + 2, h.FrontY - 13, 1, 13, C.M1); s.Rect(h.FrontX - 1, h.FrontY - 1, 6, 2, C.Y1); s.Rect(h.FrontX + 1, h.FrontY + 1, 2, 2, C.W1);
                    break;
                case 1: // Orc Lord
                    h = BigHum(); h.TorsoW = 13; h.Skin = C.Rgb(102, 146, 72); h.SkinD = C.Rgb(74, 110, 52); h.Body = C.M1; h.BodyD = C.M0; h.Legs = C.Rgb(76, 64, 54); h.LegsD = C.Rgb(56, 46, 40); h.Feet = C.Out2; h.LegW = 4;
                    DrawHum(s, h, f);
                    Cape(s, h, C.R1, C.R0, f);
                    s.Rect(h.HeadX, h.HeadY, h.HeadW, 3, C.M1); s.Rect(h.HeadX, h.HeadY + 2, h.HeadW, 1, C.M0);
                    s.Rect(h.HeadX - 2, h.HeadY - 2, 2, 2, C.B2); s.Px(h.HeadX - 3, h.HeadY - 4, C.B2); s.Px(h.HeadX - 2, h.HeadY - 3, C.B2);
                    s.Rect(h.HeadX + h.HeadW, h.HeadY - 2, 2, 2, C.B2); s.Px(h.HeadX + h.HeadW + 2, h.HeadY - 4, C.B2); s.Px(h.HeadX + h.HeadW + 1, h.HeadY - 3, C.B2);
                    s.Rect(h.HeadX + h.HeadW - 2, h.HeadY + h.HeadH - 2, 1, 2, C.B2); s.Rect(h.HeadX + h.HeadW - 5, h.HeadY + h.HeadH - 2, 1, 2, C.B2);
                    s.Rect(h.TorsoX, h.TorsoY, 3, 2, C.M2); s.Rect(h.TorsoX + h.TorsoW - 3, h.TorsoY, 3, 2, C.M2);
                    s.Rect(h.TorsoX, h.TorsoY + h.TorsoH - 3, h.TorsoW, 2, C.W1); s.Rect(h.TorsoX + 5, h.TorsoY + h.TorsoH - 3, 3, 2, C.Y1);
                    s.Rect(h.FrontX + 1, h.FrontY - 12, 2, 15, C.W1);
                    s.Poly(new double[] { h.FrontX + 3, h.FrontY - 13, h.FrontX + 9, h.FrontY - 15, h.FrontX + 9, h.FrontY - 5, h.FrontX + 3, h.FrontY - 7 }, C.M1);
                    s.Rect(h.FrontX + 7, h.FrontY - 14, 2, 9, C.M2);
                    break;
                case 2: // Elemental God
                    {
                        int bob = f;
                        Color robe = C.B2, robeD = C.B1, skin = C.Rgb(250, 230, 180);
                        s.Ellipse(16, 16 - bob, 13, 13, C.Argb(60, 255, 240, 170));
                        s.Poly(new double[] { 11, 12 - bob, 21, 12 - bob, 24, 27 - bob, 20, 25 - bob, 16, 28 - bob, 12, 25 - bob, 8, 27 - bob }, robe);
                        s.Poly(new double[] { 16, 12 - bob, 21, 12 - bob, 24, 27 - bob, 20, 25 - bob, 16, 28 - bob }, robeD);
                        s.Rect(11, 17 - bob, 10, 2, C.Y1);
                        s.Rect(12, 4 - bob, 8, 8, skin); s.Rect(12, 4 - bob, 1, 8, C.Skin);
                        s.Rect(17, 7 - bob, 2, 2, C.Rgb(120, 230, 250)); s.Rect(13, 7 - bob, 2, 2, C.Rgb(120, 230, 250));
                        Crown(s, 12, 3 - bob, 8);
                        s.Rect(8, 13 - bob, 3, 6, robeD); s.Rect(21, 13 - bob, 3, 6, robe);
                        int[,] orbs = { { 4, 9 }, { 28, 9 }, { 3, 22 }, { 29, 22 } };
                        Color[] oc = { C.Fire1, C.Ice1, C.Storm2, C.Rgb(150, 112, 70) };
                        Color[] ol = { C.Fire3, C.Ice2, C.Y2, C.GR3 };
                        for (int i = 0; i < 4; i++)
                        {
                            int oy = orbs[i, 1] + ((i + f) % 2 == 0 ? -1 : 1);
                            s.Ellipse(orbs[i, 0], oy, 2.6, 2.6, oc[i]); s.Px(orbs[i, 0] - 2, oy - 2, ol[i]); s.Px(orbs[i, 0] - 1, oy - 2, ol[i]);
                        }
                        flying = true;
                    }
                    break;
                case 3: // Vampire Lord
                    h = BigHum(); h.TorsoW = 9; h.LegW = 2; h.LegGap = 3;
                    h.Skin = C.Rgb(226, 214, 214); h.SkinD = C.Rgb(186, 172, 178); h.Hair = C.Rgb(34, 28, 36); h.Eye = C.R1;
                    h.Body = C.Rgb(44, 36, 50); h.BodyD = C.Rgb(30, 24, 34); h.Legs = C.Rgb(40, 32, 46); h.LegsD = C.Rgb(28, 22, 32); h.Feet = C.Out2;
                    DrawHum(s, h, f);
                    {
                        Color cape = C.Rgb(34, 28, 40);
                        int w = f == 0 ? 0 : 1;
                        s.Poly(new double[] { h.TorsoX, h.TorsoY - 3, h.TorsoX - 9 + w, h.TorsoY + 2, h.TorsoX - 8 + w, h.TorsoY + 16, h.TorsoX - 5, h.TorsoY + 13, h.TorsoX - 2, h.TorsoY + 16, h.TorsoX, h.TorsoY + 12 }, cape);
                        s.Poly(new double[] { h.TorsoX - 1, h.TorsoY, h.TorsoX - 6 + w, h.TorsoY + 4, h.TorsoX - 6 + w, h.TorsoY + 13, h.TorsoX - 1, h.TorsoY + 11 }, C.R0);
                        s.Rect(h.HeadX - 2, h.HeadY + 3, 2, 6, C.R1); s.Rect(h.HeadX - 1, h.HeadY + 2, 1, 1, C.R1);
                        s.Rect(h.TorsoX + 3, h.TorsoY, 3, 3, C.B2); s.Px(h.TorsoX + 4, h.TorsoY + 1, C.R1);
                        s.Px(h.HeadX + h.HeadW - 3, h.HeadY + h.HeadH - 1, C.B2); s.Px(h.HeadX + h.HeadW - 5, h.HeadY + h.HeadH - 1, C.B2);
                        s.Px(h.HeadX + 2, h.HeadY - 1, h.Hair); s.Px(h.HeadX + 6, h.HeadY - 1, h.Hair);
                    }
                    break;
                case 4: // Elder Dragon
                    Dragon(s, f, 2, C.Rgb(52, 34, 76), C.Rgb(88, 60, 122), C.Rgb(196, 170, 120), C.Rgb(42, 28, 60), true, true);
                    flying = true;
                    break;
                case 5: // Fire Dragon
                    Dragon(s, f, 2, C.Fire0, C.Rgb(214, 84, 40), C.Fire2, C.R0, true, true);
                    s.Px(29, 11, C.Fire2); s.Px(30, 10 + f, C.Fire1); s.Px(31, 11, C.Fire3);
                    flying = true;
                    break;
                case 6: // Skeleton Mage
                    h = BigHum(); h.LegW = 2; h.LegGap = 4; h.TorsoW = 9; h.Skin = C.B2; h.SkinD = C.B1; h.Eye = C.Poison2;
                    h.Body = C.Rgb(48, 58, 108); h.BodyD = C.Rgb(32, 40, 78); h.Legs = h.Body; h.LegsD = h.BodyD; h.Feet = h.BodyD; h.Sleeves = true;
                    DrawHum(s, h, f);
                    s.Poly(new double[] { h.TorsoX, h.TorsoY + h.TorsoH - 1, h.TorsoX + h.TorsoW, h.TorsoY + h.TorsoH - 1, h.TorsoX + h.TorsoW + 2, 28, h.TorsoX - 2, 28 }, h.Body);
                    s.Rect(h.TorsoX + h.TorsoW - 1, h.TorsoY + h.TorsoH - 1, 2, 28 - h.TorsoY - h.TorsoH + 1, h.BodyD);
                    s.Rect(h.HeadX, h.HeadY - 1, h.HeadW, 3, h.Body); s.Rect(h.HeadX - 1, h.HeadY + 1, 2, h.HeadH, h.BodyD);
                    s.Px(h.HeadX + 3, h.HeadY - 3, h.Body); s.Rect(h.HeadX + 2, h.HeadY - 2, 4, 1, h.Body);
                    s.Rect(h.HeadX + 4, h.HeadY + h.HeadH - 2, 4, 1, C.Out2);
                    s.Rect(h.TorsoX + 3, h.TorsoY + 1, 3, 1, C.Y1);
                    s.Rect(h.FrontX + 1, h.FrontY - 12, 1, 20, C.W1);
                    s.Ellipse(h.FrontX + 1.5, h.FrontY - 14, 2.5, 2.5, C.Poison1); s.Px(h.FrontX + 1, h.FrontY - 15, C.Poison2); s.Px(h.FrontX + 1, h.FrontY - 18 + f, C.Poison2);
                    break;
                case 7: // Dark Elf King
                    h = BigHum(); h.TorsoW = 9; h.LegW = 2; h.LegGap = 3; h.Ears = true;
                    h.Skin = C.Rgb(130, 116, 172); h.SkinD = C.Rgb(98, 86, 136); h.Hair = C.Rgb(228, 228, 238); h.Eye = C.R2;
                    h.Body = C.Rgb(62, 46, 86); h.BodyD = C.Rgb(42, 30, 60); h.Legs = C.Rgb(52, 40, 72); h.LegsD = C.Rgb(36, 28, 50); h.Feet = C.Out2;
                    DrawHum(s, h, f);
                    Cape(s, h, C.Rgb(40, 30, 58), C.Purple1, f);
                    s.Rect(h.HeadX, h.HeadY + h.HeadH - 1, 2, 6, h.Hair);
                    Crown(s, h.HeadX + 1, h.HeadY - 1, h.HeadW - 1);
                    s.Rect(h.TorsoX + 1, h.TorsoY + 2, h.TorsoW - 2, 1, C.Purple2); s.Px(h.TorsoX + 4, h.TorsoY + 4, C.Y1);
                    s.Line(h.FrontX + 1, h.FrontY, h.FrontX + 5, h.FrontY - 7, C.M2); s.Line(h.FrontX + 2, h.FrontY, h.FrontX + 6, h.FrontY - 7, C.M1); s.Rect(h.FrontX, h.FrontY, 3, 1, C.Y1);
                    s.Line(h.BackX - 1, h.BackY, h.BackX - 4, h.BackY - 6, C.M1);
                    break;
                case 8: // All Elements Golem
                    h = BigHum(); h.TorsoW = 14; h.LegW = 4; h.ArmW = 4; h.ArmH = 10; h.HeadW = 7; h.HeadH = 6; h.BigEyes = false;
                    h.Skin = C.S2; h.SkinD = C.S1; h.Body = C.S1; h.BodyD = C.S0; h.Legs = C.S1; h.LegsD = C.S0; h.Feet = C.S0; h.Sleeves = false; h.Eye = C.B2;
                    DrawHum(s, h, f);
                    {
                        int tx = h.TorsoX, ty = h.TorsoY, hw = h.TorsoW / 2, hh = h.TorsoH / 2;
                        Gem(s, tx + 1, ty + 1, hw - 2, hh - 1, C.Fire1, C.Fire3);
                        Gem(s, tx + hw + 1, ty + 1, hw - 2, hh - 1, C.Ice1, C.Ice2);
                        Gem(s, tx + 1, ty + hh + 1, hw - 2, hh - 2, C.Storm1, C.Y2);
                        Gem(s, tx + hw + 1, ty + hh + 1, hw - 2, hh - 2, C.Rgb(122, 92, 62), C.GR3);
                        s.Rect(h.HeadX + 2, h.HeadY + 2, h.HeadW - 2, 1, C.Rgb(250, 250, 220));
                        s.Rect(h.FrontX - 2, h.FrontY - 2, 3, 3, C.Fire1); s.Rect(h.BackX, h.BackY - 2, 3, 3, C.Ice1);
                    }
                    break;
                default: // Goblin Lord
                    h = BigHum(); h.LegH = 5; h.TorsoW = 11; h.TorsoH = 7; h.HeadW = 9; h.HeadH = 7; h.Ears = true;
                    h.Skin = C.Rgb(118, 170, 74); h.SkinD = C.Rgb(84, 128, 56); h.Body = C.W2; h.BodyD = C.W1; h.Eye = C.Y2;
                    DrawHum(s, h, f);
                    s.Rect(h.TorsoX - 1, h.TorsoY - 1, h.TorsoW + 2, 3, C.Rgb(196, 176, 140)); s.Rect(h.TorsoX - 1, h.TorsoY + 1, h.TorsoW + 2, 1, C.Rgb(150, 130, 100));
                    Crown(s, h.HeadX + 1, h.HeadY - 1, h.HeadW - 2);
                    s.Rect(h.HeadX + h.HeadW - 3, h.HeadY + h.HeadH - 2, 1, 2, C.B2); s.Rect(h.HeadX + h.HeadW - 6, h.HeadY + h.HeadH - 2, 1, 2, C.B2);
                    s.Rect(h.FrontX + 1, h.FrontY - 9, 2, 11, C.W1); s.Ellipse(h.FrontX + 2, h.FrontY - 11, 3, 3.5, C.W2);
                    s.Px(h.FrontX - 1, h.FrontY - 12, C.M2); s.Px(h.FrontX + 5, h.FrontY - 11, C.M2); s.Px(h.FrontX + 2, h.FrontY - 15, C.M2); s.Px(h.FrontX + 3, h.FrontY - 9, C.M2);
                    break;
            }
            Finish(s, 28, flying);
            return s;
        }

        static void Gem(Spr s, int x, int y, int w, int h, Color c, Color l)
        {
            s.Rect(x, y, w, h, c); s.Rect(x, y, w, 1, l); s.Px(x, y + 1, l); s.Rect(x + w - 1, y + 1, 1, h - 1, C.Dark(c, 0.3));
        }

        // ---------------- UI icons (16x16) ----------------
        public static readonly string[] IconNames =
        {
            "Coin", "Heart", "Wave", "Skull", "Physical", "Fire", "Ice", "Earth", "Storm", "Poison",
            "Upgrade", "Sell", "Cursor", "Star", "Sword", "Range", "Speed", "Lock"
        };

        public static Spr Icon(int i)
        {
            Spr s = new Spr(16, 16);
            Dictionary<char, Color> pal = new Dictionary<char, Color>();
            switch (i)
            {
                case 0:
                    s.Ellipse(8, 8.5, 6, 6, C.Y0); s.Ellipse(7.5, 8, 5.2, 5.2, C.Y1);
                    s.Ellipse(7.5, 8, 3.2, 3.2, C.Y0); s.Ellipse(7.5, 8, 2.4, 2.4, C.Y1);
                    s.Px(5, 4, C.Y2); s.Px(4, 5, C.Y2); s.Px(6, 4, C.Y2); s.Px(7, 7, C.Y2); s.Rect(7, 6, 1, 4, C.Y0);
                    break;
                case 1:
                    pal['r'] = C.R1; pal['h'] = C.Rgb(255, 156, 140); pal['d'] = C.R0;
                    s.Grid(new[] {
                        "  rr   rr  ",
                        " rhrr rrrr ",
                        "rhhrrrrrrrd",
                        "rhrrrrrrrrd",
                        "rrrrrrrrrdd",
                        " rrrrrrrdd ",
                        "  rrrrrdd  ",
                        "   rrrdd   ",
                        "    rdd    ",
                        "     d     " }, 2, 3, pal);
                    break;
                case 2:
                    s.Rect(4, 2, 1, 12, C.W2); s.Rect(5, 2, 1, 12, C.W1);
                    s.Rect(6, 3, 7, 5, C.Rgb(80, 170, 190)); s.Rect(6, 7, 7, 1, C.Rgb(52, 120, 140)); s.Px(12, 3, C.Rgb(52, 120, 140));
                    s.Rect(8, 4, 2, 2, C.B2); s.Px(8, 4, C.Rgb(52, 120, 140));
                    s.Rect(2, 13, 6, 1, C.W1);
                    break;
                case 3:
                    pal['b'] = C.B2; pal['s'] = C.B1; pal['e'] = C.Out2;
                    s.Grid(new[] {
                        "  bbbbbb  ",
                        " bbbbbbbbs",
                        "bbbbbbbbbs",
                        "beebbbeebs",
                        "beebbbeebs",
                        "bbbbebbbss",
                        " sbbbbbss ",
                        "  bsbsbs  ",
                        "  s s s   " }, 3, 3, pal);
                    break;
                case 4:
                    s.Line(3, 12, 11, 4, C.W2); s.Line(4, 12, 11, 5, C.W1);
                    s.Rect(11, 3, 2, 2, C.M2); s.Px(13, 3, C.M1); s.Px(11, 2, C.M1); s.Px(12, 5, C.M1);
                    s.Px(2, 12, C.R1); s.Px(3, 13, C.R1); s.Px(2, 13, C.R2); s.Px(4, 11, C.R2); s.Px(1, 12, C.R2);
                    break;
                case 5:
                    pal['a'] = C.Fire0; pal['b'] = C.Fire1; pal['c'] = C.Fire2; pal['d'] = C.Fire3;
                    s.Grid(new[] {
                        "   a     ",
                        "   aa    ",
                        "  aba  a ",
                        "  abba aa",
                        " abbbaaba",
                        " abcbbbba",
                        "abbccbcba",
                        "abccdccba",
                        "abcdddcba",
                        " abcdcba ",
                        "  aaaaa  " }, 3, 2, pal);
                    break;
                case 6:
                    s.Poly(new double[] { 8, 1, 12, 6, 8, 15, 4, 6 }, C.Ice1);
                    s.Poly(new double[] { 8, 1, 8, 15, 4, 6 }, C.Ice2);
                    s.Line(8, 2, 8, 13, C.Ice0); s.Px(6, 5, C.B2);
                    break;
                case 7:
                    s.Ellipse(8, 9.5, 6, 4.5, C.Rgb(122, 92, 62)); s.Ellipse(7, 8.5, 4.5, 3, C.Rgb(150, 112, 74));
                    s.Ellipse(10, 10, 2.5, 2, C.S1); s.Px(9, 9, C.S2); s.Px(5, 7, C.Rgb(176, 136, 92));
                    s.Px(6, 5, C.GR3); s.Px(7, 4, C.GR3); s.Px(7, 5, C.GR2); s.Px(8, 5, C.GR2);
                    break;
                case 8:
                    s.Poly(new double[] { 9, 1, 4, 9, 7.5, 9, 6, 15, 12, 6, 8.5, 6, 11, 1 }, C.Y1);
                    s.Poly(new double[] { 9, 1, 4, 9, 6.5, 9, 9.5, 2 }, C.Y2);
                    break;
                case 9:
                    s.Ellipse(8, 10, 4.5, 4.5, C.Poison1); s.Poly(new double[] { 8, 1.5, 11.8, 8, 4.2, 8 }, C.Poison1);
                    s.Ellipse(9, 11, 2.8, 2.8, C.Poison0); s.Ellipse(8.5, 10.5, 2.2, 2.2, C.Poison1);
                    s.Px(6, 8, C.Poison2); s.Px(6, 9, C.Poison2); s.Px(7, 6, C.Poison2);
                    break;
                case 10:
                    s.Poly(new double[] { 8, 2, 14, 8.5, 10.5, 8.5, 10.5, 14, 5.5, 14, 5.5, 8.5, 2, 8.5 }, C.Rgb(84, 180, 76));
                    s.Poly(new double[] { 8, 2, 8, 14, 5.5, 14, 5.5, 8.5, 2, 8.5 }, C.Rgb(140, 220, 110));
                    break;
                case 11:
                    s.Ellipse(8, 10, 5.5, 4.5, C.W3); s.Ellipse(7, 9, 3.5, 3, C.Rgb(214, 170, 110));
                    s.Rect(6, 3, 5, 3, C.W3); s.Rect(6, 5, 5, 1, C.W1); s.Px(5, 2, C.W3); s.Px(11, 2, C.W3);
                    s.Rect(8, 8, 1, 5, C.Y0); s.Rect(7, 9, 3, 1, C.Y0); s.Rect(7, 11, 3, 1, C.Y0);
                    break;
                case 12:
                    s.Poly(new double[] { 3, 3, 13, 3, 8, 12 }, C.Y1);
                    s.Poly(new double[] { 3, 3, 8, 3, 8, 12 }, C.Y2);
                    s.Rect(3, 3, 10, 1, C.Y2);
                    break;
                case 13:
                    s.Poly(new double[] { 8, 1, 10, 6, 15, 6.5, 11, 9.5, 12.5, 15, 8, 12, 3.5, 15, 5, 9.5, 1, 6.5, 6, 6 }, C.Y1);
                    s.Poly(new double[] { 8, 1, 8, 12, 3.5, 15, 5, 9.5, 1, 6.5, 6, 6 }, C.Y2);
                    break;
                case 14:
                    s.Line(4, 11, 12, 3, C.M2); s.Line(5, 11, 12, 4, C.M1); s.Px(13, 2, C.M2);
                    s.Line(2, 9, 6, 13, C.Y1); s.Line(3, 12, 1, 14, C.W1); s.Px(2, 13, C.W1);
                    break;
                case 15:
                    s.Ellipse(8, 8, 6.5, 6.5, C.R1); s.Ellipse(8, 8, 4.8, 4.8, C.B2); s.Ellipse(8, 8, 3.2, 3.2, C.R1); s.Ellipse(8, 8, 1.5, 1.5, C.B2);
                    break;
                case 16:
                    s.Rect(4, 2, 8, 2, C.W1); s.Rect(4, 13, 8, 2, C.W1);
                    s.Poly(new double[] { 5, 4, 11, 4, 8.5, 8.5, 11, 13, 5, 13, 7.5, 8.5 }, C.Ice2);
                    s.Poly(new double[] { 6, 5, 10, 5, 8, 7.5 }, C.Y2); s.Poly(new double[] { 8, 10, 10.5, 13, 5.5, 13 }, C.Y1);
                    break;
                default:
                    s.Rect(5, 2, 6, 1, C.M1); s.Rect(4, 3, 1, 4, C.M1); s.Rect(11, 3, 1, 4, C.M0); s.Px(5, 3, C.M2);
                    s.Rect(3, 7, 10, 7, C.Y1); s.Rect(3, 7, 10, 1, C.Y2); s.Rect(3, 13, 10, 1, C.Y0); s.Rect(12, 8, 1, 5, C.Y0);
                    s.Rect(7, 9, 2, 2, C.Out2); s.Rect(7, 11, 1, 2, C.Out2);
                    break;
            }
            s.Outline(C.Out);
            return s;
        }
    }
}


namespace TowerDefenseSprites
{
    // One playable map. Path, spots and main building must match TowerDefenseMaps in B5_TowerDefense.cs.
    public class MapDef
    {
        public string File;
        public string Theme;
        public int[] Path;
        public int[] Spots;
        public Point MainBuilding;
        public int Seed;
    }

    public class Theme
    {
        public string Name;
        public Color[] Ground;  // replaces GR0..GR4
        public Color[] Dirt;    // replaces D0..D3
        public Color[] Water;   // replaces WA0..WA3
        public int[] TileWeights; // grass variants 0..3
        public Dictionary<int, Color> SpriteColors = new Dictionary<int, Color>();
        public List<KeyValuePair<string, int>> Decos = new List<KeyValuePair<string, int>>();
    }

    public static class Maps
    {
        public const int MapW = 320, MapH = 155, RowOffset = 2;

        // Order = unlock order. From map to map the path gets shorter and there are fewer
        // pedestals (10, 10, 9, 8) that cover less of the path, so every map is a bit harder.
        // Every pedestal reaches at least 60 px of path with range 4 (52 px).
        public static readonly MapDef[] All =
        {
            new MapDef
            {
                File = "map_gruene_wiese.png", Theme = "meadow", Seed = 11, MainBuilding = new Point(286, 70),
                Path = new[] { -10, 128, 40, 128, 40, 14, 100, 14, 100, 128, 164, 128, 164, 14, 228, 14, 228, 90, 286, 90, 286, 70 },
                Spots = new[] { 16, 46, 132, 46, 198, 56, 70, 62, 196, 100, 16, 102, 132, 102, 236, 106, 66, 108, 264, 124 }
            },
            new MapDef
            {
                File = "map_herbstwald.png", Theme = "autumn", Seed = 22, MainBuilding = new Point(280, 60),
                Path = new[] { -10, 40, 100, 40, 100, 120, 200, 120, 200, 76, 280, 76, 280, 60 },
                Spots = new[] { 124, 54, 232, 58, 42, 74, 74, 76, 126, 98, 170, 102, 228, 110, 258, 110, 76, 120, 198, 136 }
            },
            new MapDef
            {
                File = "map_wuestenpass.png", Theme = "desert", Seed = 33, MainBuilding = new Point(272, 60),
                Path = new[] { -10, 100, 120, 100, 120, 76, 272, 76, 272, 60 },
                Spots = new[] { 160, 58, 190, 58, 224, 58, 46, 82, 90, 82, 144, 110, 232, 110, 44, 134, 80, 134 }
            },
            new MapDef
            {
                File = "map_frostgipfel.png", Theme = "snow", Seed = 44, MainBuilding = new Point(196, 80),
                Path = new[] { -10, 36, 120, 36, 120, 100, 196, 100, 196, 80 },
                Spots = new[] { 144, 38, 14, 52, 42, 70, 92, 70, 148, 82, 112, 116, 140, 134, 170, 134 }
            }
        };

        public static Theme GetTheme(string name)
        {
            Theme t = new Theme();
            t.Name = name;
            t.Ground = new[] { C.GR0, C.GR1, C.GR2, C.GR3, C.GR4 };
            t.Dirt = new[] { C.D0, C.D1, C.D2, C.D3 };
            t.Water = new[] { C.WA0, C.WA1, C.WA2, C.WA3 };
            t.TileWeights = new[] { 6, 2, 1, 1 };
            if (name == "meadow")
            {
                Add(t, "pond", 2); Add(t, "house_red", 1); Add(t, "house_brown", 1); Add(t, "oak", 5); Add(t, "pine", 3);
                Add(t, "bush", 5); Add(t, "rock", 3); Add(t, "fence", 1); Add(t, "barrel", 1); Add(t, "crate", 1);
                Add(t, "flowers", 8); Add(t, "pebbles", 5);
            }
            else if (name == "autumn")
            {
                t.Ground = new[] { C.Rgb(84, 92, 44), C.Rgb(110, 116, 52), C.Rgb(136, 138, 62), C.Rgb(164, 158, 76), C.Rgb(196, 178, 94) };
                Map(t, C.TG0, C.Rgb(122, 48, 30)); Map(t, C.TG1, C.Rgb(182, 82, 36)); Map(t, C.TG2, C.Rgb(224, 134, 50)); Map(t, C.TG3, C.Rgb(246, 192, 92));
                Add(t, "pond", 1); Add(t, "house_brown", 2); Add(t, "oak", 9); Add(t, "pine", 6); Add(t, "bush", 5);
                Add(t, "mushrooms", 6); Add(t, "rock", 3); Add(t, "fence", 1); Add(t, "crate", 1); Add(t, "pebbles", 4);
            }
            else if (name == "desert")
            {
                t.Ground = new[] { C.Rgb(178, 142, 88), C.Rgb(196, 160, 102), C.Rgb(216, 184, 124), C.Rgb(232, 204, 148), C.Rgb(244, 224, 176) };
                t.Dirt = new[] { C.Rgb(118, 88, 66), C.Rgb(144, 108, 80), C.Rgb(168, 128, 92), C.Rgb(192, 154, 112) };
                t.TileWeights = new[] { 7, 2, 0, 1 };
                Map(t, C.TG0, C.Rgb(92, 90, 46)); Map(t, C.TG1, C.Rgb(124, 118, 58)); Map(t, C.TG2, C.Rgb(156, 146, 72)); Map(t, C.TG3, C.Rgb(186, 176, 96));
                Map(t, C.R2, C.Rgb(196, 150, 96));
                Add(t, "pond", 1); Add(t, "house_brown", 1); Add(t, "cactus", 9); Add(t, "rock", 7); Add(t, "bush", 4);
                Add(t, "barrel", 2); Add(t, "crate", 2); Add(t, "pebbles", 8);
            }
            else
            {
                t.Ground = new[] { C.Rgb(150, 170, 192), C.Rgb(182, 198, 216), C.Rgb(214, 226, 238), C.Rgb(232, 240, 248), C.Rgb(250, 252, 255) };
                t.Dirt = new[] { C.Rgb(80, 76, 82), C.Rgb(104, 100, 106), C.Rgb(130, 126, 130), C.Rgb(160, 158, 164) };
                t.Water = new[] { C.Rgb(118, 168, 206), C.Rgb(160, 206, 232), C.Rgb(200, 232, 246), C.Rgb(240, 250, 255) };
                t.TileWeights = new[] { 7, 2, 0, 1 };
                Map(t, C.Rgb(78, 142, 86), C.Rgb(236, 244, 250));
                Map(t, C.TG2, C.Rgb(226, 236, 244)); Map(t, C.TG3, C.Rgb(250, 252, 255));
                Map(t, C.S3, C.Rgb(244, 248, 252)); Map(t, C.R2, C.Rgb(240, 246, 252)); Map(t, C.Sand, C.Rgb(200, 214, 228)); Map(t, C.SandD, C.Rgb(170, 186, 204));
                Map(t, C.WA0, C.Rgb(118, 168, 206)); Map(t, C.WA1, C.Rgb(160, 206, 232)); Map(t, C.WA2, C.Rgb(200, 232, 246)); Map(t, C.WA3, C.Rgb(240, 250, 255));
                Add(t, "pond", 2); Add(t, "house_red", 1); Add(t, "pine", 12); Add(t, "rock", 6); Add(t, "bush", 2);
                Add(t, "barrel", 1); Add(t, "fence", 1); Add(t, "pebbles", 5);
            }
            return t;
        }

        static void Add(Theme t, string name, int count) { t.Decos.Add(new KeyValuePair<string, int>(name, count)); }
        static void Map(Theme t, Color from, Color to) { t.SpriteColors[from.ToArgb()] = to; }

        // ---------- extra scenery ----------
        public static Spr Cactus()
        {
            Spr s = new Spr(32, 32);
            Color c0 = C.Rgb(48, 104, 62), c1 = C.Rgb(70, 140, 76), c2 = C.Rgb(112, 176, 96);
            s.Rect(14, 10, 5, 18, c1); s.Rect(14, 10, 1, 18, c2); s.Rect(18, 10, 1, 18, c0); s.Rect(15, 9, 3, 1, c1);
            s.Rect(9, 16, 5, 3, c1); s.Rect(9, 12, 3, 5, c1); s.Rect(9, 12, 1, 6, c2);
            s.Rect(19, 19, 4, 3, c1); s.Rect(21, 14, 3, 6, c1); s.Rect(23, 14, 1, 7, c0);
            for (int y = 12; y < 27; y += 3) { s.Px(16, y, c0); s.Px(16, y + 1, C.B2); }
            s.Px(15, 8, C.Rgb(236, 120, 160)); s.Px(16, 8, C.Rgb(250, 170, 196));
            s.Outline(C.Out);
            s.EllipseBehind(16.5, 28.5, 7, 1.8, C.Shadow);
            return s;
        }

        public static Spr Mushrooms()
        {
            Spr s = new Spr(32, 32);
            int[,] m = { { 12, 25, 3 }, { 18, 27, 2 }, { 21, 24, 2 } };
            for (int i = 0; i < 3; i++)
            {
                int x = m[i, 0], y = m[i, 1], r = m[i, 2];
                s.Rect(x - 1, y - r + 1, 2, r + 1, C.B1);
                s.Ellipse(x, y - r, r + 1, r * 0.8, C.R1);
                s.Px(x - 1, y - r - 1, C.B2); s.Px(x + 1, y - r, C.B2);
            }
            s.Outline(C.Out);
            s.EllipseBehind(16.5, 28.5, 7, 1.4, C.ShadowL);
            return s;
        }

        // ---------- composition ----------
        // Builds a map in twice the game resolution. sprites are the detailed scenery sprites of the
        // theme (64x64 cells), anchors are given in game pixels.
        public static Spr Compose(MapDef def, Theme theme, Dictionary<string, Spr> sprites, Dictionary<string, Point> anchors)
        {
            Validate(def);
            int k = HiRes.K;
            List<Rectangle> bands = Bands(def.Path);

            // occupancy for automatic decoration placement (game pixels)
            bool[,] blocked = new bool[MapW, MapH];
            foreach (Rectangle b in bands) Block(blocked, Rectangle.Inflate(b, 3, 3));
            for (int i = 0; i < def.Spots.Length; i += 2)
            {
                Block(blocked, ToImage(PedestalRect(def.Spots[i], def.Spots[i + 1], 2)));
                Block(blocked, ToImage(TowerRect(def.Spots[i], def.Spots[i + 1], 2)));
            }
            Block(blocked, ToImage(CastleRect(def.MainBuilding, 3)));
            Block(blocked, new Rectangle(0, 0, MapW, 2));
            Block(blocked, new Rectangle(0, MapH - 8, MapW, 8));

            List<KeyValuePair<Point, string>> placed = new List<KeyValuePair<Point, string>>();
            Random r = new Random(def.Seed);
            foreach (KeyValuePair<string, int> deco in theme.Decos)
            {
                Spr s = sprites[deco.Key];
                Point a = anchors[deco.Key];
                Rectangle hb = Bounds(s);
                Rectangle bbox = Rectangle.FromLTRB(hb.Left / k, hb.Top / k, (hb.Right + k - 1) / k, (hb.Bottom + k - 1) / k);
                int count = 0;
                for (int attempt = 0; attempt < 3000 && count < deco.Value; attempt++)
                {
                    int x = r.Next(0, MapW), y = r.Next(0, MapH);
                    Rectangle rect = new Rectangle(x - a.X + bbox.X, y - a.Y + bbox.Y, bbox.Width, bbox.Height);
                    if (rect.Left < -4 || rect.Right > MapW + 4 || rect.Top < 0 || rect.Bottom > MapH) continue;
                    if (IsBlocked(blocked, Rectangle.Inflate(rect, 1, 1))) continue;
                    Block(blocked, rect);
                    placed.Add(new KeyValuePair<Point, string>(new Point(x, y), deco.Key));
                    count++;
                }
            }

            Spr m = Art.Terrain(theme, bands, def.Seed);
            foreach (KeyValuePair<Point, string> p in placed)
                if (p.Value == "pond") StampHi(m, sprites, anchors, p.Value, p.Key.X, p.Key.Y);

            List<KeyValuePair<Point, string>> sorted = new List<KeyValuePair<Point, string>>();
            foreach (KeyValuePair<Point, string> p in placed) if (p.Value != "pond") sorted.Add(new KeyValuePair<Point, string>(new Point(p.Key.X, p.Key.Y - RowOffset), p.Value));
            for (int i = 0; i < def.Spots.Length; i += 2) sorted.Add(new KeyValuePair<Point, string>(new Point(def.Spots[i], def.Spots[i + 1]), "pedestal"));
            sorted.Sort((a, b) => a.Key.Y.CompareTo(b.Key.Y));
            foreach (KeyValuePair<Point, string> p in sorted) StampHi(m, sprites, anchors, p.Value, p.Key.X, p.Key.Y + RowOffset);
            return m;
        }

        static void StampHi(Spr m, Dictionary<string, Spr> sprites, Dictionary<string, Point> anchors, string name, int x, int imageY)
        {
            Point a = anchors[name];
            m.Stamp(sprites[name], (x - a.X) * HiRes.K, (imageY - a.Y) * HiRes.K);
        }

        static Dictionary<int, Color> TileColors(Theme t)
        {
            Dictionary<int, Color> map = new Dictionary<int, Color>();
            Color[] gr = { C.GR0, C.GR1, C.GR2, C.GR3, C.GR4 };
            Color[] di = { C.D0, C.D1, C.D2, C.D3 };
            Color[] wa = { C.WA0, C.WA1, C.WA2, C.WA3 };
            for (int i = 0; i < 5; i++) map[gr[i].ToArgb()] = t.Ground[i];
            for (int i = 0; i < 4; i++) map[di[i].ToArgb()] = t.Dirt[i];
            for (int i = 0; i < 4; i++) map[wa[i].ToArgb()] = t.Water[i];
            return map;
        }

        public static Spr Recolor(Spr s, Dictionary<int, Color> map)
        {
            Spr o = s.Clone();
            if (map.Count == 0) return o;
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    Color c;
                    if (map.TryGetValue(s.Get(x, y).ToArgb(), out c)) o.Set(x, y, c);
                }
            return o;
        }

        static void Stamp(Spr m, Dictionary<string, Spr> sprites, Dictionary<string, Point> anchors, string name, int x, int imageY)
        {
            Point a = anchors[name];
            m.Stamp(sprites[name], x - a.X, imageY - a.Y);
        }

        static Rectangle Bounds(Spr s)
        {
            int x0 = s.W, y0 = s.H, x1 = -1, y1 = -1;
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                    if (!s.Empty(x, y)) { x0 = Math.Min(x0, x); y0 = Math.Min(y0, y); x1 = Math.Max(x1, x); y1 = Math.Max(y1, y); }
            return new Rectangle(x0, y0, x1 - x0 + 1, y1 - y0 + 1);
        }

        static void Block(bool[,] b, Rectangle r)
        {
            for (int y = Math.Max(0, r.Top); y < Math.Min(MapH, r.Bottom); y++)
                for (int x = Math.Max(0, r.Left); x < Math.Min(MapW, r.Right); x++) b[x, y] = true;
        }

        static bool IsBlocked(bool[,] b, Rectangle r)
        {
            for (int y = Math.Max(0, r.Top); y < Math.Min(MapH, r.Bottom); y++)
                for (int x = Math.Max(0, r.Left); x < Math.Min(MapW, r.Right); x++) if (b[x, y]) return true;
            return false;
        }

        // ---------- geometry in map coordinates ----------
        public static List<Rectangle> Bands(int[] path)
        {
            List<Rectangle> l = new List<Rectangle>();
            for (int i = 0; i + 3 < path.Length; i += 2)
            {
                int x0 = Math.Min(path[i], path[i + 2]) - 8, x1 = Math.Max(path[i], path[i + 2]) + 8;
                int y0 = Math.Min(path[i + 1], path[i + 3]) - 8 + RowOffset, y1 = Math.Max(path[i + 1], path[i + 3]) + 8 + RowOffset;
                l.Add(Rectangle.FromLTRB(x0, y0, x1, y1));
            }
            return l;
        }

        static Rectangle ToImage(Rectangle r) { r.Offset(0, RowOffset); return r; }
        static Rectangle PedestalRect(int x, int y, int m) { return Rectangle.FromLTRB(x - 14 - m, y - 6 - m, x + 14 + m, y + 8 + m); }
        static Rectangle TowerRect(int x, int y, int m) { return Rectangle.FromLTRB(x - 16 - m, y - 37 - m, x + 16 + m, y + 5 + m); }
        static Rectangle CastleRect(Point p, int m) { return Rectangle.FromLTRB(p.X - 32 - m, p.Y - 44 - m, p.X + 32 + m, p.Y + 3 + m); }

        public static void Validate(MapDef def)
        {
            List<string> errors = new List<string>();
            int n = def.Spots.Length / 2;
            if (n < 6 || n > 10) errors.Add("expected 6 to 10 spots, found " + n);
            List<Rectangle> bands = Bands(def.Path);
            int[] p = def.Path;
            if (p[p.Length - 2] != def.MainBuilding.X || p[p.Length - 1] != def.MainBuilding.Y) errors.Add("path must end at the main building gate");
            Rectangle castle = ToImage(CastleRect(def.MainBuilding, 0));
            if (castle.Left < 0 || castle.Right > MapW || castle.Top < 0) errors.Add("main building outside of the map");
            for (int i = 0; i < n; i++)
            {
                int x = def.Spots[i * 2], y = def.Spots[i * 2 + 1];
                string id = "spot " + (i + 1) + " (" + x + "," + y + ")";
                Rectangle ped = ToImage(PedestalRect(x, y, 2)), tower = ToImage(TowerRect(x, y, 0));
                if (x < 14 || x > 306 || y < 38 || y > 136) errors.Add(id + " outside of the playable area");
                foreach (Rectangle b in bands) if (b.IntersectsWith(ped)) errors.Add(id + " touches the path");
                if (castle.IntersectsWith(ToImage(PedestalRect(x, y, 2))) || castle.IntersectsWith(tower)) errors.Add(id + " overlaps the main building");
                for (int j = 0; j < n; j++)
                {
                    if (j == i) continue;
                    int ox = def.Spots[j * 2], oy = def.Spots[j * 2 + 1];
                    if (j > i && PedestalRect(x, y, 1).IntersectsWith(PedestalRect(ox, oy, 1))) errors.Add(id + " overlaps spot " + (j + 1));
                    if (oy < y && Math.Abs(ox - x) < 28 && y - 37 < oy + 7) errors.Add(id + " hides the pedestal of spot " + (j + 1));
                }
            }
            if (errors.Count > 0) throw new InvalidOperationException(def.File + ": " + string.Join("; ", errors.ToArray()));
        }

        static int Hash(int x, int y)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263;
                h = (h ^ (h >> 13)) * 1274126177;
                return Math.Abs(h ^ (h >> 16));
            }
        }
    }
}


namespace TowerDefenseSprites
{
    public delegate bool Mask(int x, int y);

    // Drawing helpers for the detailed sheets (twice the game resolution). Every shape gets a flat
    // base colour, a shadow band on its right/bottom side and a 1px top highlight; where it overlaps
    // an earlier shape a dark separation line is drawn. Finish() adds the outer outline.
    public static class D
    {
        public static readonly Color Ink = C.Rgb(36, 28, 34);
        static readonly Color ShadowTint = C.Rgb(40, 24, 64);

        public static Color Shd(Color c) { return C.Mix(c, ShadowTint, 0.34); }
        public static Color Deep(Color c) { return C.Mix(c, ShadowTint, 0.58); }
        public static Color Hl(Color c) { return C.Mix(c, C.Rgb(255, 252, 236), 0.32); }

        static bool Other(Spr s, Mask m, int x, int y)
        {
            if (m(x, y)) return false;
            Color o = s.Get(x, y);
            return o.A == 255 && o.ToArgb() != Ink.ToArgb();
        }

        public static void Fill(Spr s, int x0, int y0, int x1, int y1, Mask m, Color c, int shade, bool line)
        {
            Color d = Shd(c), l = Hl(c);
            List<Point> pts = new List<Point>();
            for (int y = Math.Max(0, y0); y <= Math.Min(s.H - 1, y1); y++)
                for (int x = Math.Max(0, x0); x <= Math.Min(s.W - 1, x1); x++)
                    if (m(x, y)) pts.Add(new Point(x, y));
            List<Point> edge = new List<Point>();
            if (line)
                foreach (Point p in pts)
                    if (Other(s, m, p.X - 1, p.Y) || Other(s, m, p.X + 1, p.Y) || Other(s, m, p.X, p.Y - 1) || Other(s, m, p.X, p.Y + 1)) edge.Add(p);
            foreach (Point p in pts)
            {
                int x = p.X, y = p.Y;
                Color col = c;
                if (shade > 0 && (!m(x + shade, y) || !m(x, y + shade))) col = d;
                else if (shade > 1 && !m(x, y - 1) && m(x, y + 2)) col = l;
                s.Set(x, y, col);
            }
            foreach (Point p in edge) s.Set(p.X, p.Y, Ink);
        }

        public static void Ell(Spr s, double cx, double cy, double rx, double ry, Color c, int shade = 2, bool line = true)
        {
            Fill(s, (int)Math.Floor(cx - rx) - 1, (int)Math.Floor(cy - ry) - 1, (int)Math.Ceiling(cx + rx) + 1, (int)Math.Ceiling(cy + ry) + 1,
                (x, y) => Spr.InEllipse(x, y, cx, cy, rx, ry), c, shade, line);
        }

        public static void Box(Spr s, double x, double y, double w, double h, double r, Color c, int shade = 2, bool line = true)
        {
            int x0 = (int)Math.Round(x), y0 = (int)Math.Round(y), x1 = (int)Math.Round(x + w) - 1, y1 = (int)Math.Round(y + h) - 1;
            if (x1 < x0 || y1 < y0) return;
            Fill(s, x0, y0, x1, y1, (px, py) => InRound(px, py, x0, y0, x1, y1, r), c, shade, line);
        }

        public static bool InRound(int x, int y, int x0, int y0, int x1, int y1, double r)
        {
            if (x < x0 || x > x1 || y < y0 || y > y1) return false;
            if (r <= 0.5) return true;
            double cx = Math.Min(Math.Max(x + 0.5, x0 + r), x1 + 1 - r), cy = Math.Min(Math.Max(y + 0.5, y0 + r), y1 + 1 - r);
            double dx = x + 0.5 - cx, dy = y + 0.5 - cy;
            return dx * dx + dy * dy <= r * r;
        }

        public static void Poly(Spr s, double[] p, Color c, int shade = 2, bool line = true)
        {
            double minX = 1e9, maxX = -1e9, minY = 1e9, maxY = -1e9;
            for (int i = 0; i < p.Length; i += 2)
            {
                minX = Math.Min(minX, p[i]); maxX = Math.Max(maxX, p[i]);
                minY = Math.Min(minY, p[i + 1]); maxY = Math.Max(maxY, p[i + 1]);
            }
            Fill(s, (int)Math.Floor(minX) - 1, (int)Math.Floor(minY) - 1, (int)Math.Ceiling(maxX) + 1, (int)Math.Ceiling(maxY) + 1,
                (x, y) => InPoly(p, x + 0.5, y + 0.5), c, shade, line);
        }

        public static bool InPoly(double[] p, double px, double py)
        {
            bool inside = false;
            int n = p.Length / 2;
            for (int i = 0, j = n - 1; i < n; j = i++)
            {
                double xi = p[i * 2], yi = p[i * 2 + 1], xj = p[j * 2], yj = p[j * 2 + 1];
                if (((yi > py) != (yj > py)) && (px < (xj - xi) * (py - yi) / (yj - yi) + xi)) inside = !inside;
            }
            return inside;
        }

        static double SegDist(double px, double py, double x0, double y0, double x1, double y1)
        {
            double dx = x1 - x0, dy = y1 - y0, len = dx * dx + dy * dy;
            double t = len == 0 ? 0 : Math.Max(0, Math.Min(1, ((px - x0) * dx + (py - y0) * dy) / len));
            double qx = x0 + dx * t - px, qy = y0 + dy * t - py;
            return Math.Sqrt(qx * qx + qy * qy);
        }

        // Thick line with round ends.
        public static void Stick(Spr s, double x0, double y0, double x1, double y1, double w, Color c, int shade = 1, bool line = true)
        {
            double r = w / 2.0;
            Fill(s, (int)Math.Floor(Math.Min(x0, x1) - r) - 1, (int)Math.Floor(Math.Min(y0, y1) - r) - 1, (int)Math.Ceiling(Math.Max(x0, x1) + r) + 1, (int)Math.Ceiling(Math.Max(y0, y1) + r) + 1,
                (x, y) => SegDist(x + 0.5, y + 0.5, x0, y0, x1, y1) <= r, c, shade, line);
        }

        public static void Rect(Spr s, double x, double y, double w, double h, Color c)
        {
            int x0 = (int)Math.Round(x), y0 = (int)Math.Round(y), x1 = (int)Math.Round(x + w), y1 = (int)Math.Round(y + h);
            for (int yy = y0; yy < y1; yy++) for (int xx = x0; xx < x1; xx++) s.Set(xx, yy, c);
        }

        public static void Px(Spr s, double x, double y, Color c) { s.Set((int)Math.Floor(x), (int)Math.Floor(y), c); }

        // Two eyes like in the reference: dark rounded ovals with a small light glint.
        public static void Eyes(Spr s, double cx, double cy, double gap, double w, double h, Color c, bool glint)
        {
            for (int i = -1; i <= 1; i += 2)
            {
                double ex = cx + i * gap / 2.0;
                int x0 = (int)Math.Round(ex - w / 2), y0 = (int)Math.Round(cy - h / 2);
                int x1 = x0 + (int)Math.Round(w) - 1, y1 = y0 + (int)Math.Round(h) - 1;
                double r = Math.Min(w, h) * 0.45;
                Fill(s, x0, y0, x1, y1, (px, py) => InRound(px, py, x0, y0, x1, y1, r), c, 0, false);
                if (glint)
                {
                    int g = Math.Max(1, (int)Math.Round(w / 3.0));
                    Rect(s, x0 + (w >= 4 ? 1 : 0), y0 + (h >= 5 ? 1 : 0), g, g, C.Rgb(255, 255, 250));
                }
            }
        }

        // Clears pixels (used for broken walls).
        public static void Erase(Spr s, Mask m)
        {
            for (int y = 0; y < s.H; y++) for (int x = 0; x < s.W; x++) if (m(x, y)) s.Clear(x, y);
        }

        public static void Finish(Spr s, double cx, double ground, double rx, bool flying)
        {
            s.Outline(Ink);
            if (flying) s.EllipseBehind(cx, s.H - 2.5, rx * 0.7, Math.Max(1.5, rx * 0.22), C.ShadowL);
            else s.EllipseBehind(cx, ground + 1.2, rx, Math.Max(1.5, rx * 0.24), C.Shadow);
        }

        public static int Hash(int x, int y)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263;
                h = (h ^ (h >> 13)) * 1274126177;
                return Math.Abs(h ^ (h >> 16));
            }
        }
    }

    // Front facing chibi figure: big head, small body. All sizes are multiplied with K.
    public class Chibi
    {
        public double K = 1, Cx = 16, Ground = 27;
        public double HeadW = 14, HeadH = 12, BodyW = 10, BodyH = 7, LegW = 3, LegH = 3, ArmW = 3, ArmL = 6;
        public bool SquareHead, Robe, ThinLimbs;
        public Color Skin = C.Rgb(240, 196, 150), Body = C.Rgb(150, 100, 60), Legs = C.Rgb(110, 76, 50), Boots = C.Rgb(76, 54, 42);
        public Color Sleeve = Color.Empty, Hand = Color.Empty;
        public int Ears, Eyes, Mouth;
        public Color EyeColor = D.Ink;
        public double EyeW = 2, EyeH = 3, EyeGap = 0;
        public Action<Spr> AfterBody;
        // filled in by Art.DrawChibi
        public double HeadCx, HeadCy, HeadTop, BodyTop, BodyBottom, BodyX0, BodyX1, RHandX, RHandY, LHandX, LHandY;
    }

    public static class Art
    {
        public static readonly Color White = C.Rgb(250, 248, 240);
        public static readonly Color Metal = C.Rgb(178, 186, 198), MetalD = C.Rgb(116, 122, 138);
        public static readonly Color Wood = C.Rgb(150, 98, 56), WoodD = C.Rgb(112, 72, 44);
        public static readonly Color Gold = C.Rgb(244, 192, 60), Bone = C.Rgb(238, 234, 220);
        public static readonly Color GoblinSkin = C.Rgb(124, 182, 76), OrcSkin = C.Rgb(104, 150, 78);
        public static readonly Color Leather = C.Rgb(146, 96, 60), Boot = C.Rgb(86, 60, 46);
        public static readonly Color Red = C.Rgb(204, 62, 54), Purple = C.Rgb(136, 64, 150), Blue = C.Rgb(66, 100, 180);
        public static readonly Color FireA = C.Rgb(232, 96, 40), FireB = C.Rgb(252, 160, 52), FireC = C.Rgb(255, 228, 120);
        public static readonly Color IceA = C.Rgb(96, 170, 230), IceB = C.Rgb(176, 226, 250), PoisonA = C.Rgb(112, 196, 72);
        public static readonly Color StormA = C.Rgb(130, 120, 190), Bolt = C.Rgb(255, 224, 80);

        // ---------------- figure ----------------
        // Body measures for frame f (also needed before drawing, e.g. for a cape behind the figure).
        public static void Layout(Chibi c, int f)
        {
            double k = c.K;
            double legTop = c.Ground + 1 - c.LegH * k - (f == 1 ? k : 0);
            c.BodyTop = legTop - c.BodyH * k + k; c.BodyBottom = legTop + k;
            c.BodyX0 = c.Cx - c.BodyW * k / 2; c.BodyX1 = c.Cx + c.BodyW * k / 2;
        }

        public static void DrawChibi(Spr s, Chibi c, int f)
        {
            double k = c.K, cx = c.Cx, g = c.Ground;
            double bob = f == 1 ? k : 0;
            Color sleeve = c.Sleeve == Color.Empty ? c.Body : c.Sleeve;
            Layout(c, f);
            double legTop = g + 1 - c.LegH * k - bob;
            double bodyTop = c.BodyTop;
            double bw = c.BodyW * k;

            double lw = c.LegW * k, gap = (c.ThinLimbs ? 1.5 : 0.5) * k, lift = f == 1 ? k : 0;
            if (!c.Robe)
            {
                D.Box(s, cx - gap - lw, legTop, lw, g + 1 - legTop - lift, 0, c.Legs, 1, true);
                D.Box(s, cx + gap, legTop, lw, g + 1 - legTop, 0, c.Legs, 1, true);
                D.Box(s, cx - gap - lw - 0.5 * k, g + 1 - 2 * k - lift, lw + k, 2 * k, 0.8 * k, c.Boots, 1, true);
                D.Box(s, cx + gap - 0.5 * k, g + 1 - 2 * k, lw + k, 2 * k, 0.8 * k, c.Boots, 1, true);
                D.Box(s, c.BodyX0, bodyTop, bw, c.BodyH * k, 2 * k, c.Body, 2, true);
            }
            else
            {
                double hem = g + 1 - lift * 0.5;
                D.Poly(s, new[] { c.BodyX0 + k, bodyTop, c.BodyX1 - k, bodyTop, c.BodyX1 + 1.5 * k, hem, c.BodyX0 - 1.5 * k, hem }, c.Body, 2, true);
                D.Box(s, cx - 3 * k, hem - k, 2.5 * k, k + 0.4, 0, c.Boots, 0, false);
                D.Box(s, cx + 0.5 * k, hem - k, 2.5 * k, k + 0.4, 0, c.Boots, 0, false);
            }
            if (c.AfterBody != null) c.AfterBody(s);

            double swing = f == 1 ? k : 0;
            double aw = c.ArmW * k, al = c.ArmL * k;
            double lax = c.BodyX0 - aw + k, rax = c.BodyX1 - k;
            D.Box(s, lax, bodyTop + k + swing, aw, al, aw / 2, sleeve, 1, true);
            D.Box(s, rax, bodyTop + k - swing, aw, al, aw / 2, sleeve, 1, true);
            c.LHandX = lax + aw / 2; c.LHandY = bodyTop + k + swing + al - 0.5 * k;
            c.RHandX = rax + aw / 2; c.RHandY = bodyTop + k - swing + al - 0.5 * k;
            Hand(s, c, false); Hand(s, c, true);

            double hw = c.HeadW * k, hh = c.HeadH * k;
            double hcy = bodyTop - hh / 2 + 2 * k;
            c.HeadCx = cx; c.HeadCy = hcy; c.HeadTop = hcy - hh / 2;
            if (c.Ears > 0)
            {
                double len = (c.Ears == 2 ? 5 : 3) * k, up = (c.Ears == 2 ? 3 : 3.5) * k;
                D.Poly(s, new[] { cx - hw / 2 + 1.5 * k, hcy - 1.5 * k, cx - hw / 2 - len, hcy - up, cx - hw / 2 + 1.5 * k, hcy + 2 * k }, c.Skin, 1, true);
                D.Poly(s, new[] { cx + hw / 2 - 1.5 * k, hcy - 1.5 * k, cx + hw / 2 + len, hcy - up, cx + hw / 2 - 1.5 * k, hcy + 2 * k }, c.Skin, 1, true);
            }
            if (c.SquareHead) D.Box(s, cx - hw / 2, hcy - hh / 2, hw, hh, 3 * k, c.Skin, 2, true);
            else D.Ell(s, cx, hcy, hw / 2, hh / 2, c.Skin, 2, true);
            Face(s, c);
        }

        public static void Hand(Spr s, Chibi c, bool right)
        {
            double k = c.K, r = c.ArmW * k * 0.62;
            Color hand = c.Hand == Color.Empty ? c.Skin : c.Hand;
            if (right) D.Ell(s, c.RHandX, c.RHandY, r, r, hand, 1, true);
            else D.Ell(s, c.LHandX, c.LHandY, r, r, hand, 1, true);
        }

        static void Face(Spr s, Chibi c)
        {
            double k = c.K, cx = c.HeadCx, cy = c.HeadCy + 0.5 * k;
            double gap = c.EyeGap > 0 ? c.EyeGap * k : c.HeadW * k * 0.38;
            if (c.Eyes == 1) D.Eyes(s, cx, cy, gap * 1.05, 3.4 * k, 3.6 * k, D.Ink, false);
            else if (c.Eyes == 2) D.Eyes(s, cx, cy, gap, c.EyeW * k, c.EyeH * k, c.EyeColor, false);
            else D.Eyes(s, cx, cy, gap, c.EyeW * k, c.EyeH * k, c.EyeColor, c.EyeColor.ToArgb() == D.Ink.ToArgb());
            if (c.Eyes == 3)
            {
                D.Stick(s, cx - gap / 2 - 1.8 * k, cy - 2.6 * k, cx - gap / 2 + 1.2 * k, cy - 1.8 * k, 0.9 * k + 0.3, D.Ink, 0, false);
                D.Stick(s, cx + gap / 2 + 1.8 * k, cy - 2.6 * k, cx + gap / 2 - 1.2 * k, cy - 1.8 * k, 0.9 * k + 0.3, D.Ink, 0, false);
            }
            double my = cy + 3 * k;
            switch (c.Mouth)
            {
                case 1: D.Rect(s, cx - 1.5 * k, my, 3 * k, Math.Max(1, k), D.Ink); break;
                case 2:
                    D.Rect(s, cx - 2.5 * k, my, 5 * k, Math.Max(1, k), D.Ink);
                    D.Rect(s, cx - 2.5 * k, my - 1.5 * k, Math.Max(1, k), 1.5 * k, White);
                    D.Rect(s, cx + 1.5 * k, my - 1.5 * k, Math.Max(1, k), 1.5 * k, White);
                    break;
                case 3:
                    D.Rect(s, cx - 2 * k, my, 4 * k, Math.Max(1, k), D.Ink);
                    D.Rect(s, cx - 1.5 * k, my + k, Math.Max(1, k), k, White);
                    D.Rect(s, cx + 0.5 * k, my + k, Math.Max(1, k), k, White);
                    break;
                case 4:
                    D.Rect(s, cx - 3 * k, my - 0.5 * k, 6 * k, 2 * k, D.Ink);
                    for (double x = cx - 2.5 * k; x < cx + 2.5 * k; x += 2 * k) D.Rect(s, x, my - 0.5 * k, Math.Max(1, k), 2 * k, Bone);
                    D.Rect(s, cx - 0.5 * k, cy + 1.2 * k, Math.Max(1, k), Math.Max(1, k), D.Ink);
                    break;
                case 5:
                    D.Box(s, cx - 2 * k, my - 0.5 * k, 4 * k, 2.5 * k, k, D.Ink, 0, false);
                    D.Rect(s, cx - k, my + k, 2 * k, Math.Max(1, k), C.Rgb(220, 90, 100));
                    break;
            }
        }

        // ---------------- equipment ----------------
        static void Dagger(Spr s, Chibi c)
        {
            double k = c.K, x = c.RHandX, y = c.RHandY;
            D.Poly(s, new[] { x - k, y - k, x + k, y - k, x + 1.5 * k, y - 7 * k, x, y - 8 * k }, Metal, 1, true);
            D.Box(s, x - 2 * k, y - 1.8 * k, 4.5 * k, 1.6 * k, 0, Gold, 0, true);
            Hand(s, c, true);
        }

        static void Sword(Spr s, Chibi c, double len, bool left)
        {
            double k = c.K, x = left ? c.LHandX : c.RHandX, y = left ? c.LHandY : c.RHandY;
            D.Box(s, x - k, y - len * k, 2.2 * k, len * k - k, 0.8 * k, Metal, 1, true);
            D.Box(s, x - 3 * k, y - 2.3 * k, 6.2 * k, 1.8 * k, 0.6 * k, Gold, 0, true);
            D.Box(s, x - 0.6 * k, y + 0.5 * k, 1.4 * k, 2.5 * k, 0, WoodD, 0, true);
            Hand(s, c, !left);
        }

        static void Axe(Spr s, Chibi c, double size, bool left)
        {
            double k = c.K * size, x = left ? c.LHandX : c.RHandX, y = left ? c.LHandY : c.RHandY;
            double dir = left ? -1 : 1;
            D.Stick(s, x, y + 2 * k, x, y - 10 * k, 1.8 * k, Wood, 1, true);
            D.Poly(s, new[] { x, y - 10 * k, x + dir * 5 * k, y - 12 * k, x + dir * 6.5 * k, y - 7.5 * k, x + dir * 5 * k, y - 3.5 * k, x, y - 5.5 * k }, Metal, 2, true);
            D.Rect(s, Math.Min(x + dir * 5.5 * k, x + dir * 6.3 * k), y - 10 * k, Math.Max(1, k * 0.8), 5 * k, C.Rgb(230, 236, 244));
            Hand(s, c, !left);
        }

        static void Club(Spr s, Chibi c, Color col, double size)
        {
            double k = c.K * size, x = c.RHandX, y = c.RHandY;
            D.Stick(s, x, y + 2 * k, x + 2 * k, y - 7 * k, 2.2 * k, col, 1, true);
            D.Ell(s, x + 2.5 * k, y - 8.5 * k, 3 * k, 3.6 * k, col, 2, true);
            Hand(s, c, true);
        }

        static void BoneClub(Spr s, Chibi c)
        {
            double k = c.K, x = c.RHandX, y = c.RHandY;
            D.Stick(s, x - 4 * k, y + 1 * k, x + 5 * k, y - 1 * k, 1.8 * k, Bone, 1, true);
            D.Ell(s, x + 5.5 * k, y - 2 * k, 1.6 * k, 1.6 * k, Bone, 1, true);
            D.Ell(s, x + 6 * k, y + 0.2 * k, 1.6 * k, 1.6 * k, Bone, 1, true);
            Hand(s, c, true);
        }

        static void Bow(Spr s, Chibi c, Color arrowTip)
        {
            double k = c.K, x = c.RHandX + 1.5 * k, y = c.RHandY - 1 * k;
            double[] px = { -1.5, 1, 2.5, 2.5, 1, -1.5 }, py = { -8, -6, -2.5, 2.5, 6, 8 };
            for (int i = 0; i + 1 < px.Length; i++) D.Stick(s, x + px[i] * k, y + py[i] * k, x + px[i + 1] * k, y + py[i + 1] * k, 1.6 * k, Wood, 0, true);
            D.Stick(s, x - 1.5 * k, y - 8 * k, x - 1.5 * k, y + 8 * k, Math.Max(1, 0.6 * k), C.Rgb(230, 226, 210), 0, false);
            D.Stick(s, x - 6 * k, y, x + 5 * k, y, Math.Max(1, 0.8 * k), WoodD, 0, false);
            D.Poly(s, new[] { x + 4.5 * k, y - 1.5 * k, x + 7 * k, y, x + 4.5 * k, y + 1.5 * k }, arrowTip, 0, true);
            Hand(s, c, true);
        }

        static void Staff(Spr s, Chibi c, double len, Color orb)
        {
            double k = c.K, x = c.RHandX, y = c.RHandY;
            D.Stick(s, x, y + 3 * k, x, y - len * k, 1.8 * k, Wood, 1, true);
            D.Ell(s, x, y - len * k - 2 * k, 2.8 * k, 2.8 * k, orb, 2, true);
            D.Rect(s, x - 1.5 * k, y - len * k - 3.5 * k, Math.Max(1, k), Math.Max(1, k), White);
            Hand(s, c, true);
        }

        static void Shield(Spr s, Chibi c, Color col)
        {
            double k = c.K, x = c.LHandX, y = c.LHandY - 2 * k;
            D.Box(s, x - 4 * k, y - 4.5 * k, 8 * k, 9.5 * k, 3.5 * k, col, 2, true);
            D.Box(s, x - 1.5 * k, y - 2 * k, 3 * k, 4 * k, 1.5 * k, Gold, 0, true);
        }

        static void Hood(Spr s, Chibi c, Color col)
        {
            double k = c.K, hw = c.HeadW * k, hh = c.HeadH * k, cx = c.HeadCx, cy = c.HeadCy;
            double rx = hw / 2 + 1.5 * k, ry = hh / 2 + 1.3 * k, ocy = cy - k;
            double frx = hw / 2 - 1.2 * k, fry = hh / 2 - 1.2 * k, fcy = cy + 2.8 * k;
            Mask m = (x, y) => Spr.InEllipse(x, y, cx, ocy, rx, ry) && !Spr.InEllipse(x, y, cx, fcy, frx, fry);
            D.Fill(s, (int)(cx - rx - 2), (int)(ocy - ry - 2), (int)(cx + rx + 2), (int)(ocy + ry + 2), m, col, 2, true);
            D.Poly(s, new[] { cx - 2 * k, ocy - ry + 1.5 * k, cx + 3 * k, ocy - ry + 0.5 * k, cx - 3 * k, ocy - ry - 2.5 * k }, col, 1, true);
        }

        static void WizardHat(Spr s, Chibi c, Color col, Color band, double tilt)
        {
            double k = c.K, hw = c.HeadW * k, cx = c.HeadCx, by = c.HeadTop + 3 * k;
            D.Poly(s, new[] { cx - hw * 0.34, by, cx + hw * 0.36, by, cx + tilt * k, by - 14 * k }, col, 2, true);
            D.Box(s, cx - hw * 0.34, by - 3 * k, hw * 0.7, 2 * k, 0, band, 0, true);
            D.Ell(s, cx + 0.5 * k, by, hw / 2 + 2.5 * k, 1.8 * k, col, 1, true);
        }

        static void Helmet(Spr s, Chibi c, Color col, bool horns)
        {
            double k = c.K, hw = c.HeadW * k, hh = c.HeadH * k, cx = c.HeadCx, cy = c.HeadCy;
            if (horns)
            {
                D.Poly(s, new[] { cx - hw / 2 + 1 * k, cy - 2.5 * k, cx - hw / 2 - 3 * k, cy - 6 * k, cx - hw / 2 - 2 * k, cy - 10 * k, cx - hw / 2 + 2.5 * k, cy - 4.5 * k }, Bone, 1, true);
                D.Poly(s, new[] { cx + hw / 2 - 1 * k, cy - 2.5 * k, cx + hw / 2 + 3 * k, cy - 6 * k, cx + hw / 2 + 2 * k, cy - 10 * k, cx + hw / 2 - 2.5 * k, cy - 4.5 * k }, Bone, 1, true);
            }
            Mask m = (x, y) => Spr.InEllipse(x, y, cx, cy, hw / 2 + 0.8 * k, hh / 2 + 0.8 * k) && y < cy - 3 * k;
            D.Fill(s, (int)(cx - hw), (int)(cy - hh), (int)(cx + hw), (int)cy, m, col, 2, true);
            D.Box(s, cx - hw / 2 - 0.5 * k, cy - 4.2 * k, hw + k, 1.8 * k, k, col, 1, true);
            D.Box(s, cx - 0.7 * k, cy - 3 * k, 1.4 * k, 2.5 * k, 0, col, 1, true);
        }

        static void Crown(Spr s, double cx, double y, double w, double k)
        {
            D.Poly(s, new[] { cx - w / 2, y, cx - w / 2, y - 4 * k, cx - w / 4, y - 1.8 * k, cx, y - 5 * k, cx + w / 4, y - 1.8 * k, cx + w / 2, y - 4 * k, cx + w / 2, y }, Gold, 1, true);
            D.Box(s, cx - w / 2, y - k, w, 2.2 * k, 0, Gold, 1, true);
            D.Rect(s, cx - 0.8 * k, y - 0.6 * k, 1.6 * k, 1.4 * k, Red);
        }

        static void Hair(Spr s, Chibi c, Color col, bool longHair)
        {
            double k = c.K, hw = c.HeadW * k, hh = c.HeadH * k, cx = c.HeadCx, cy = c.HeadCy;
            if (longHair)
            {
                D.Box(s, cx - hw / 2 - 0.8 * k, cy - 2 * k, 3 * k, hh / 2 + 5 * k, k, col, 1, true);
                D.Box(s, cx + hw / 2 - 2.2 * k, cy - 2 * k, 3 * k, hh / 2 + 5 * k, k, col, 1, true);
            }
            Mask m = (x, y) => Spr.InEllipse(x, y, cx, cy - 0.5 * k, hw / 2 + 0.8 * k, hh / 2 + 0.8 * k)
                && (y < cy - 2.5 * k + Math.Abs(x + 0.5 - cx) * 0.25 || (Math.Abs(x + 0.5 - cx) > hw / 2 - 1.5 * k && y < cy));
            D.Fill(s, (int)(cx - hw), (int)(cy - hh), (int)(cx + hw), (int)(cy + 1), m, col, 2, true);
        }

        static void Cape(Spr s, Chibi c, Color col, Color lining, int f)
        {
            Layout(c, f);
            double k = c.K, w = f == 1 ? k : 0;
            D.Poly(s, new[] { c.BodyX0 - 1 * k, c.BodyTop, c.BodyX1 + 1 * k, c.BodyTop, c.BodyX1 + 4 * k + w, c.Ground - k, c.BodyX0 - 4 * k - w, c.Ground - k }, col, 2, true);
            D.Poly(s, new[] { c.BodyX0, c.BodyTop + 2 * k, c.BodyX1, c.BodyTop + 2 * k, c.BodyX1 + 2.5 * k, c.Ground - 2 * k, c.BodyX0 - 2.5 * k, c.Ground - 2 * k }, lining, 1, false);
        }

        static Action<Spr> Belt(Chibi c, Color col, double height)
        {
            return s => D.Box(s, c.BodyX0, c.BodyBottom - height * c.K - c.K, c.BodyW * c.K, height * c.K, 0, col, 1, true);
        }

        // ---------------- creature bases ----------------
        static Chibi Goblin(double k, double cx, double g)
        {
            Chibi c = new Chibi { K = k, Cx = cx, Ground = g, HeadW = 14, HeadH = 11, BodyW = 9, BodyH = 6, LegH = 3, ArmL = 5 };
            c.Skin = GoblinSkin; c.Body = GoblinSkin; c.Legs = GoblinSkin; c.Boots = Boot; c.Ears = 2; c.Mouth = 1;
            c.AfterBody = Belt(c, Leather, 3);
            return c;
        }

        static Chibi Orc(double k, double cx, double g)
        {
            Chibi c = new Chibi { K = k, Cx = cx, Ground = g, HeadW = 14, HeadH = 11, BodyW = 12, BodyH = 8, LegW = 3.5, LegH = 3, ArmW = 3.5, ArmL = 7 };
            c.Skin = OrcSkin; c.Body = Leather; c.Legs = C.Rgb(92, 76, 64); c.Boots = Boot; c.Sleeve = OrcSkin; c.Ears = 1; c.Eyes = 3; c.Mouth = 2;
            return c;
        }

        static Chibi Skeleton(double k, double cx, double g)
        {
            Chibi c = new Chibi { K = k, Cx = cx, Ground = g, HeadW = 14, HeadH = 12, BodyW = 8, BodyH = 7, LegW = 2, LegH = 4, ArmW = 2.2, ArmL = 7, ThinLimbs = true };
            c.Skin = Bone; c.Body = Bone; c.Legs = Bone; c.Boots = Bone; c.SquareHead = true; c.Eyes = 1; c.Mouth = 4;
            c.AfterBody = s =>
            {
                double kk = c.K;
                for (double y = c.BodyTop + 2 * kk; y < c.BodyBottom - kk; y += 2 * kk) D.Rect(s, c.BodyX0 + kk, y, c.BodyW * kk - 2 * kk, Math.Max(1, kk * 0.8), D.Ink);
                D.Rect(s, c.Cx - 0.5 * kk, c.BodyTop + kk, Math.Max(1, kk), c.BodyH * kk - 2 * kk, Bone);
            };
            return c;
        }

        static Chibi Golem(double k, double cx, double g, Color stone)
        {
            Chibi c = new Chibi { K = k, Cx = cx, Ground = g, HeadW = 10, HeadH = 8, BodyW = 15, BodyH = 9, LegW = 4.5, LegH = 4, ArmW = 5, ArmL = 9 };
            c.Skin = stone; c.Body = stone; c.Legs = stone; c.Boots = D.Shd(stone); c.SquareHead = true; c.Eyes = 2; c.EyeW = 2.6; c.EyeH = 2; c.EyeGap = 4.5;
            return c;
        }

        static Chibi Elf(double k, double cx, double g)
        {
            Chibi c = new Chibi { K = k, Cx = cx, Ground = g, HeadW = 13, HeadH = 12, BodyW = 9, BodyH = 7, LegW = 2.5, LegH = 4, ArmW = 2.6, ArmL = 6.5 };
            c.Skin = C.Rgb(156, 136, 200); c.Body = C.Rgb(70, 52, 96); c.Legs = C.Rgb(56, 44, 78); c.Boots = C.Rgb(40, 32, 52); c.Ears = 1;
            c.EyeColor = C.Rgb(230, 60, 60); c.Eyes = 2; c.EyeH = 2; c.EyeW = 2.4; c.Mouth = 1;
            return c;
        }

        static Chibi Vampire(double k, double cx, double g)
        {
            Chibi c = new Chibi { K = k, Cx = cx, Ground = g, HeadW = 13, HeadH = 12, BodyW = 9, BodyH = 8, LegW = 2.6, LegH = 4, ArmW = 2.6, ArmL = 6.5 };
            c.Skin = C.Rgb(232, 224, 226); c.Body = C.Rgb(48, 40, 56); c.Legs = C.Rgb(40, 34, 48); c.Boots = C.Rgb(30, 24, 34);
            c.EyeColor = C.Rgb(210, 40, 50); c.Eyes = 2; c.EyeH = 2; c.EyeW = 2.4; c.Mouth = 3;
            c.AfterBody = s => { double kk = c.K; D.Poly(s, new[] { c.Cx - 2.5 * kk, c.BodyTop, c.Cx + 2.5 * kk, c.BodyTop, c.Cx, c.BodyTop + 4 * kk }, White, 0, true); D.Rect(s, c.Cx - 0.5 * kk, c.BodyTop + kk, Math.Max(1, kk), 1.5 * kk, Red); };
            return c;
        }

        static void Flame(Spr s, int f, double k, double cx, double cy)
        {
            double t = f == 1 ? k : 0;
            D.Poly(s, new[] { cx - 7 * k, cy, cx - 6 * k, cy - 7 * k + t, cx - 3 * k, cy - 4 * k, cx - 1 * k, cy - 12 * k - t, cx + 2.5 * k, cy - 5 * k, cx + 5 * k, cy - 9 * k + t, cx + 7 * k, cy }, FireA, 2, true);
            D.Ell(s, cx, cy + 0.5 * k, 7 * k, 6.5 * k, FireA, 2, false);
            D.Poly(s, new[] { cx - 4 * k, cy + 2 * k, cx - 3 * k, cy - 3 * k, cx - 0.5 * k, cy - 7 * k - t, cx + 2 * k, cy - 2 * k, cx + 4 * k, cy + 2 * k }, FireB, 0, false);
            D.Ell(s, cx, cy + 2.5 * k, 4.5 * k, 3.5 * k, FireB, 0, false);
            D.Ell(s, cx, cy + 3.5 * k, 2.5 * k, 2 * k, FireC, 0, false);
            D.Eyes(s, cx, cy, 5 * k, 2 * k, 3 * k, D.Ink, true);
            D.Rect(s, cx - k, cy + 3 * k, 2 * k, Math.Max(1, k), D.Ink);
        }

        static void Crystal(Spr s, int f, double k, double cx, double cy)
        {
            double b = f == 1 ? -k : 0;
            D.Poly(s, new[] { cx - 9 * k, cy + 2 * k - b, cx - 6 * k, cy - 1 * k - b, cx - 5 * k, cy + 6 * k - b }, IceA, 1, true);
            D.Poly(s, new[] { cx + 9 * k, cy + 2 * k - b, cx + 6 * k, cy - 1 * k - b, cx + 5 * k, cy + 6 * k - b }, IceA, 1, true);
            D.Poly(s, new[] { cx, cy - 12 * k + b, cx + 6 * k, cy + b, cx, cy + 12 * k + b, cx - 6 * k, cy + b }, IceB, 3, true);
            D.Stick(s, cx, cy - 10 * k + b, cx, cy + 10 * k + b, Math.Max(1, 0.7 * k), C.Rgb(150, 210, 245), 0, false);
            D.Eyes(s, cx, cy + b, 6 * k, 2 * k, 3 * k, C.Rgb(30, 60, 120), true);
        }

        static void Cloud(Spr s, int f, double k, double cx, double cy)
        {
            int bx = f == 0 ? -1 : 2;
            D.Poly(s, new[] { cx + (bx - 1) * k, cy + 5 * k, cx + (bx + 3) * k, cy + 5 * k, cx + (bx + 1) * k, cy + 9 * k, cx + (bx + 3) * k, cy + 9 * k, cx + (bx - 1) * k, cy + 15 * k, cx + bx * k, cy + 10 * k, cx + (bx - 2) * k, cy + 10 * k }, Bolt, 1, true);
            D.Ell(s, cx - 5 * k, cy + 1 * k, 5 * k, 4.5 * k, StormA, 2, true);
            D.Ell(s, cx + 5 * k, cy + 1 * k, 5 * k, 4.5 * k, StormA, 2, true);
            D.Ell(s, cx, cy - 2 * k, 6.5 * k, 6 * k, StormA, 2, true);
            D.Ell(s, cx - 1 * k, cy - 4 * k, 3.5 * k, 2.5 * k, D.Hl(StormA), 0, false);
            D.Eyes(s, cx, cy + 0.5 * k, 5 * k, 2 * k, 3 * k, D.Ink, true);
        }

        static void Mound(Spr s, int f, double k, double cx, double g)
        {
            Color rock = C.Rgb(150, 112, 78), stone = C.Rgb(160, 156, 148), moss = C.Rgb(104, 170, 70);
            double st = f == 1 ? k : 0;
            D.Box(s, cx - 6 * k - st, g - 4 * k, 4 * k, 5 * k, k, D.Shd(rock), 1, true);
            D.Box(s, cx + 2 * k + st, g - 4 * k, 4 * k, 5 * k, k, rock, 1, true);
            D.Ell(s, cx, g - 9 * k, 10 * k, 7.5 * k, rock, 3, true);
            D.Ell(s, cx - 5 * k, g - 15 * k, 4 * k, 3 * k, stone, 2, true);
            D.Ell(s, cx + 4 * k, g - 16 * k, 3.5 * k, 3 * k, stone, 2, true);
            D.Ell(s, cx - 1 * k, g - 15.5 * k, 3 * k, 1.6 * k, moss, 0, true);
            D.Eyes(s, cx, g - 9 * k, 6 * k, 2.4 * k, 2 * k, Gold, false);
            D.Rect(s, cx - 2 * k, g - 5.5 * k, 4 * k, Math.Max(1, k), D.Ink);
        }

        // Side view, facing right; k = 1 for a 32 px cell.
        static void Dragon(Spr s, int f, double k, Color body, Color belly, Color wing, bool horns, bool fire)
        {
            Func<double[], double[]> S =p => { double[] o = new double[p.Length]; for (int i = 0; i < p.Length; i++) o[i] = p[i] * k; return o; };
            if (f == 0) D.Poly(s, S(new double[] { 12, 14, 18, 14, 17, 3, 12, 4, 8, 7 }), D.Shd(wing), 1, true);
            else D.Poly(s, S(new double[] { 12, 16, 18, 16, 16, 26, 11, 24 }), D.Shd(wing), 1, true);
            D.Poly(s, S(new double[] { 9, 16, 11, 21, 3, 22, 1, 20, 3, 19 }), body, 1, true);
            D.Poly(s, S(new double[] { 1, 20, 3, 18, 3, 23, 0, 23 }), wing, 0, true);
            D.Box(s, 10 * k, 20 * k, 3 * k, 6 * k + 0.4, k, D.Shd(body), 1, true);
            D.Box(s, 16 * k, 20 * k, 3 * k, 6 * k + 0.4, k, D.Shd(body), 1, true);
            D.Ell(s, 14.5 * k, 17.5 * k, 7.5 * k, 5 * k, body, 3, true);
            D.Ell(s, 14.5 * k, 20.5 * k, 5 * k, 2 * k, belly, 0, true);
            D.Poly(s, S(new double[] { 17, 15, 22, 8, 26, 10, 21, 18 }), body, 2, true);
            if (horns) D.Poly(s, S(new double[] { 22, 6, 18, 2, 24, 6.5 }), Bone, 0, true);
            D.Ell(s, 24.5 * k, 9 * k, 4.5 * k, 3.6 * k, body, 2, true);
            D.Box(s, 25 * k, 8.5 * k, 6 * k, 4 * k, 1.5 * k, body, 2, true);
            D.Rect(s, 26 * k, 11.2 * k, 5 * k, Math.Max(1, k), belly);
            D.Eyes(s, 24 * k, 8 * k, 0, 2 * k, 2.4 * k, D.Ink, true);
            D.Rect(s, 29.5 * k, 9.5 * k, Math.Max(1, k), Math.Max(1, k), D.Ink);
            if (fire)
            {
                D.Poly(s, S(new double[] { 31, 10, 34, 8.5, 35, 11, 34, 13.5 }), FireA, 0, true);
                D.Ell(s, 32.5 * k, 11 * k, 1.5 * k, 1.3 * k, FireC, 0, false);
            }
            if (f == 0) D.Poly(s, S(new double[] { 12, 15, 18, 15, 20, 5, 16, 3, 10, 7 }), wing, 2, true);
            else D.Poly(s, S(new double[] { 12, 16, 18, 16, 17, 26, 13, 25, 9, 22 }), wing, 2, true);
            if (f == 0) { D.Stick(s, 17 * k, 14 * k, 18 * k, 4 * k, Math.Max(1, 0.7 * k), D.Shd(wing), 0, false); D.Stick(s, 14 * k, 14 * k, 12 * k, 5 * k, Math.Max(1, 0.7 * k), D.Shd(wing), 0, false); }
            else { D.Stick(s, 16 * k, 17 * k, 16 * k, 27 * k, Math.Max(1, 0.7 * k), D.Shd(wing), 0, false); D.Stick(s, 13 * k, 17 * k, 11 * k, 25 * k, Math.Max(1, 0.7 * k), D.Shd(wing), 0, false); }
            for (int i = 0; i < 3; i++) D.Poly(s, S(new double[] { 10 + i * 3, 13, 11.5 + i * 3, 10.5, 13 + i * 3, 13 }), D.Hl(body), 0, true);
        }

        // ---------------- enemies (32x32 art pixels = 16x16 game pixels, 2 frames) ----------------
        public static Spr Enemy(int i, int f)
        {
            Spr s = new Spr(32, 32);
            double k = 1, cx = 16, g = 27, rx = 7;
            bool fly = false;
            Chibi c;
            switch (i)
            {
                case 0: // Goblin
                    c = Goblin(k, cx, g); DrawChibi(s, c, f); Dagger(s, c); break;
                case 1: // Goblin Archer
                    c = Goblin(k, cx, g); c.Body = C.Rgb(86, 130, 60); c.AfterBody = Belt(c, Leather, 2);
                    DrawChibi(s, c, f); Hood(s, c, C.Rgb(70, 118, 56)); Bow(s, c, Metal); break;
                case 2: // Goblin Shaman
                    c = Goblin(k, cx, g); c.Robe = true; c.Body = Purple; c.Sleeve = Purple; c.AfterBody = null;
                    DrawChibi(s, c, f);
                    D.Stick(s, cx - 3, c.HeadTop + 1, cx - 5, c.HeadTop - 4, 1.5, Red, 0, true);
                    D.Stick(s, cx, c.HeadTop, cx, c.HeadTop - 5, 1.5, Gold, 0, true);
                    D.Stick(s, cx + 3, c.HeadTop + 1, cx + 5, c.HeadTop - 4, 1.5, IceA, 0, true);
                    Staff(s, c, 11, PoisonA); break;
                case 3: // Orc
                    c = Orc(k, cx, g); c.AfterBody = Belt(c, Boot, 2); DrawChibi(s, c, f);
                    D.Box(s, c.BodyX1 - 3, c.BodyTop - 0.5, 5, 3, 1.5, Metal, 1, true);
                    Axe(s, c, 1, false); break;
                case 4: // Orc Berserker
                    c = Orc(k, cx, g); c.Body = OrcSkin; c.AfterBody = Belt(c, Leather, 2); DrawChibi(s, c, f);
                    D.Box(s, cx - 7, c.HeadCy - 4, 14, 2, 0, Red, 0, true);
                    D.Rect(s, cx - 6, c.HeadCy + 1, 2, 1, Red); D.Rect(s, cx + 4, c.HeadCy + 1, 2, 1, Red);
                    Axe(s, c, 0.85, false); Axe(s, c, 0.85, true); break;
                case 5: // Orc Warlord
                    c = Orc(k, cx, g); c.Body = Metal; c.Legs = MetalD; c.AfterBody = Belt(c, Red, 2);
                    Cape(s, c, Red, C.Rgb(150, 40, 40), f);
                    DrawChibi(s, c, f); Helmet(s, c, MetalD, true); Sword(s, c, 11, false); break;
                case 6: // Skeleton
                    c = Skeleton(k, cx, g); DrawChibi(s, c, f); BoneClub(s, c); break;
                case 7: // Skeleton Knight
                    c = Skeleton(k, cx, g); DrawChibi(s, c, f); Helmet(s, c, MetalD, false); Shield(s, c, Blue); Sword(s, c, 9, false); break;
                case 8: // Troll
                    c = new Chibi { K = k, Cx = cx, Ground = g, HeadW = 12, HeadH = 10, BodyW = 15, BodyH = 9, LegW = 4.5, LegH = 4, ArmW = 4.5, ArmL = 9 };
                    c.Skin = C.Rgb(140, 160, 126); c.Body = c.Skin; c.Legs = c.Skin; c.Boots = D.Shd(c.Skin); c.Ears = 1; c.Mouth = 2; c.EyeGap = 4;
                    c.AfterBody = Belt(c, Leather, 3);
                    DrawChibi(s, c, f); Club(s, c, Wood, 1); rx = 8; break;
                case 9: // Stone Golem
                    c = Golem(k, cx, g, C.Rgb(158, 154, 146)); c.EyeColor = C.Rgb(110, 236, 250); DrawChibi(s, c, f);
                    D.Ell(s, c.BodyX0, c.BodyTop + 1, 2.5, 1.5, C.Rgb(104, 170, 70), 0, true);
                    D.Ell(s, c.BodyX1, c.BodyTop + 1, 2, 1.3, C.Rgb(104, 170, 70), 0, true);
                    D.Stick(s, cx - 2, c.BodyTop + 2, cx, c.BodyTop + 6, 1, D.Ink, 0, false); rx = 9; break;
                case 10: Flame(s, f, k, cx, 16); fly = true; break;
                case 11: Crystal(s, f, k, cx, 14); fly = true; break;
                case 12: Cloud(s, f, k, cx, 11); fly = true; break;
                case 13: Mound(s, f, k, cx, g); rx = 9; break;
                case 14: // Dark Elf
                    c = Elf(k, cx, g); DrawChibi(s, c, f); Hair(s, c, C.Rgb(236, 236, 244), true); Dagger(s, c); break;
                case 15: // Vampire
                    c = Vampire(k, cx, g); Cape(s, c, C.Rgb(40, 32, 48), C.Rgb(170, 40, 50), f);
                    DrawChibi(s, c, f); Hair(s, c, C.Rgb(34, 28, 40), false); break;
                case 16: Dragon(s, f, 1, C.Rgb(76, 150, 104), C.Rgb(200, 220, 150), C.Rgb(60, 120, 96), false, false); fly = true; cx = 15; break;
                case 17: // Fire Golem
                    c = Golem(k, cx, g, C.Rgb(96, 76, 70)); c.EyeColor = FireC; DrawChibi(s, c, f);
                    D.Stick(s, cx - 3, c.BodyTop + 2, cx + 1, c.BodyTop + 6, 1.4, FireB, 0, false);
                    D.Stick(s, cx + 3, c.BodyTop + 1, cx + 2, c.BodyTop + 4, 1.2, FireB, 0, false);
                    D.Poly(s, new double[] { cx - 3, c.HeadTop + 1, cx - 1, c.HeadTop - 4 - f, cx + 1, c.HeadTop - 1, cx + 3, c.HeadTop - 3 + f, cx + 4, c.HeadTop + 1 }, FireA, 0, true);
                    rx = 9; break;
                case 18: Dragon(s, f, 1, C.Rgb(200, 62, 52), C.Rgb(244, 196, 110), C.Rgb(150, 40, 40), true, false); fly = true; cx = 15; break;
                default: // Arcane Titan
                    c = Golem(k, cx, g, C.Rgb(82, 100, 186)); c.SquareHead = true; c.EyeColor = C.Rgb(130, 244, 252); c.Body = C.Rgb(70, 86, 168);
                    c.AfterBody = Belt(c, Gold, 2);
                    DrawChibi(s, c, f);
                    D.Box(s, c.BodyX0 - 1, c.BodyTop - 1, 4, 3, 1, Gold, 1, true); D.Box(s, c.BodyX1 - 3, c.BodyTop - 1, 4, 3, 1, Gold, 1, true);
                    D.Rect(s, cx - 1, c.BodyTop + 2, 2, 2, C.Rgb(130, 244, 252)); D.Rect(s, cx - 3, c.BodyTop + 5, 6, 1, C.Rgb(130, 244, 252));
                    rx = 9; break;
            }
            D.Finish(s, cx, g, rx, fly);
            return s;
        }

        // ---------------- bosses (64x64 art pixels = 32x32 game pixels, 2 frames) ----------------
        public static Spr Boss(int i, int f)
        {
            Spr s = new Spr(64, 64);
            double k = 2, cx = 32, g = 57, rx = 14;
            bool fly = false;
            Chibi c;
            switch (i)
            {
                case 0: // Skeleton King
                    c = Skeleton(k, cx, g); Cape(s, c, Purple, C.Rgb(96, 40, 110), f);
                    DrawChibi(s, c, f); c.Eyes = 2; c.EyeColor = C.Rgb(120, 236, 250);
                    D.Eyes(s, c.HeadCx, c.HeadCy + k, 5.5 * k, 1.6 * k, 1.6 * k, c.EyeColor, false);
                    Crown(s, cx, c.HeadTop + 2 * k, 10 * k, k); Sword(s, c, 13, false); break;
                case 1: // Orc Lord
                    c = Orc(k, cx, g); c.Body = Metal; c.Legs = MetalD; c.AfterBody = Belt(c, Gold, 2);
                    Cape(s, c, Red, C.Rgb(150, 40, 40), f);
                    DrawChibi(s, c, f);
                    D.Box(s, c.BodyX0 - 2 * k, c.BodyTop - k, 5 * k, 3.5 * k, 1.5 * k, MetalD, 1, true); D.Box(s, c.BodyX1 - 3 * k, c.BodyTop - k, 5 * k, 3.5 * k, 1.5 * k, MetalD, 1, true);
                    Helmet(s, c, MetalD, true); Axe(s, c, 1.25, false); break;
                case 2: // Elemental God
                    {
                        double b = f == 1 ? -k : 0;
                        c = new Chibi { K = k, Cx = cx, Ground = g - 4 + b, HeadW = 12, HeadH = 12, BodyW = 11, BodyH = 9, ArmW = 3, ArmL = 7 };
                        c.Robe = true; c.Body = C.Rgb(240, 236, 220); c.Skin = C.Rgb(252, 226, 186); c.Boots = c.Body; c.Mouth = 1;
                        c.EyeColor = C.Rgb(90, 200, 240); c.Eyes = 2; c.EyeH = 2.4;
                        c.AfterBody = Belt(c, Gold, 2);
                        DrawChibi(s, c, f); Crown(s, cx, c.HeadTop + 2 * k, 9 * k, k);
                        double[] ox = { 8, 56, 6, 58 }, oy = { 18, 18, 42, 42 };
                        Color[] oc = { FireA, IceA, StormA, C.Rgb(150, 112, 70) };
                        for (int o = 0; o < 4; o++) { double yy = oy[o] + ((o + f) % 2 == 0 ? -2 : 2); D.Ell(s, ox[o], yy, 5, 5, oc[o], 2, true); D.Rect(s, ox[o] - 2, yy - 3, 2, 2, White); }
                        fly = true;
                    }
                    break;
                case 3: // Vampire Lord
                    c = Vampire(k, cx, g);
                    {
                        double w = f == 1 ? 2 : 0;
                        D.Poly(s, new double[] { cx - 6, 22, cx - 30 + w, 12, cx - 26 + w, 30, cx - 30 + w, 48, cx - 8, 52 }, C.Rgb(40, 32, 48), 2, true);
                        D.Poly(s, new double[] { cx + 6, 22, cx + 30 - w, 12, cx + 26 - w, 30, cx + 30 - w, 48, cx + 8, 52 }, C.Rgb(40, 32, 48), 2, true);
                        D.Poly(s, new double[] { cx - 8, 26, cx - 25 + w, 18, cx - 22 + w, 46, cx - 8, 48 }, C.Rgb(170, 40, 50), 1, false);
                        D.Poly(s, new double[] { cx + 8, 26, cx + 25 - w, 18, cx + 22 - w, 46, cx + 8, 48 }, C.Rgb(170, 40, 50), 1, false);
                    }
                    DrawChibi(s, c, f); Hair(s, c, C.Rgb(34, 28, 40), false); Crown(s, cx, c.HeadTop + 2 * k, 8 * k, k); break;
                case 4: Dragon(s, f, 1.8, C.Rgb(110, 76, 150), C.Rgb(214, 190, 140), C.Rgb(80, 56, 112), true, false); fly = true; cx = 28; rx = 16; break;
                case 5: Dragon(s, f, 1.8, C.Rgb(236, 110, 44), FireC, C.Rgb(190, 60, 36), true, true); fly = true; cx = 28; rx = 16; break;
                case 6: // Skeleton Mage (hooded like the reference)
                    c = Skeleton(k, cx, g); c.Robe = true; c.Body = C.Rgb(60, 70, 140); c.Sleeve = c.Body; c.AfterBody = null;
                    DrawChibi(s, c, f);
                    D.Eyes(s, c.HeadCx, c.HeadCy + k, 5.5 * k, 1.6 * k, 1.6 * k, PoisonA, false);
                    Hood(s, c, C.Rgb(60, 70, 140)); Staff(s, c, 16, PoisonA); break;
                case 7: // Dark Elf King
                    c = Elf(k, cx, g); Cape(s, c, C.Rgb(56, 40, 80), Purple, f); DrawChibi(s, c, f);
                    Hair(s, c, C.Rgb(236, 236, 244), true); Crown(s, cx, c.HeadTop + 2 * k, 9 * k, k);
                    Sword(s, c, 10, false); Sword(s, c, 10, true); break;
                case 8: // All Elements Golem
                    c = Golem(k, cx, g, C.Rgb(160, 156, 148)); c.EyeColor = White; DrawChibi(s, c, f);
                    {
                        double x0 = c.BodyX0 + 2 * k, y0 = c.BodyTop + 2 * k, w = 5 * k, h = 3 * k;
                        D.Box(s, x0, y0, w, h, k, FireA, 1, true); D.Box(s, x0 + w + 1 * k, y0, w, h, k, IceA, 1, true);
                        D.Box(s, x0, y0 + h + k, w, h, k, StormA, 1, true); D.Box(s, x0 + w + 1 * k, y0 + h + k, w, h, k, C.Rgb(130, 170, 70), 1, true);
                        D.Ell(s, c.LHandX, c.LHandY, 2 * k, 2 * k, IceA, 1, true); D.Ell(s, c.RHandX, c.RHandY, 2 * k, 2 * k, FireA, 1, true);
                    }
                    rx = 18; break;
                default: // Goblin Lord
                    c = Goblin(k, cx, g); c.Body = C.Rgb(150, 100, 60); c.Eyes = 3; c.Mouth = 2; DrawChibi(s, c, f);
                    D.Box(s, c.BodyX0 - 2 * k, c.BodyTop - 1.5 * k, c.BodyW * k + 4 * k, 3 * k, 1.5 * k, C.Rgb(214, 196, 160), 1, true);
                    Crown(s, cx, c.HeadTop + 2 * k, 9 * k, k); Club(s, c, Wood, 1);
                    for (int sp = 0; sp < 3; sp++) D.Rect(s, c.RHandX + (1 + sp * 2.5) * k, c.RHandY - (11 + (sp % 2) * 3) * k, 1.5 * k, 1.5 * k, Metal);
                    break;
            }
            D.Finish(s, cx, g, rx, fly);
            return s;
        }

        // ---------------- towers (64x96 art pixels, spot at (32, 86)) ----------------
        public static Spr Tower(int i)
        {
            Spr s = new Spr(64, 96);
            Color[] stones = { C.Rgb(170, 166, 158), C.Rgb(150, 168, 140), C.Rgb(184, 146, 128), C.Rgb(152, 174, 198), C.Rgb(186, 166, 130), C.Rgb(160, 150, 184), C.Rgb(130, 126, 126), C.Rgb(200, 224, 240) };
            Color[] accents = { Leather, PoisonA, FireA, IceA, C.Rgb(122, 154, 70), Bolt, FireA, IceA };
            Color stone = stones[i], acc = accents[i], mortar = D.Shd(stone);
            // body
            D.Box(s, 15, 44, 34, 43, 2, stone, 3, true);
            for (int row = 0; row < 6; row++)
            {
                int y = 50 + row * 6;
                D.Rect(s, 16, y, 31, 1, mortar);
                for (int x = 19 + (row % 2) * 5; x < 46; x += 10) D.Rect(s, x, y + 1, 1, 5, mortar);
            }
            D.Box(s, 15, 58, 34, 4, 0, acc, 1, true);
            D.Box(s, 26, 70, 12, 17, 6, C.Rgb(104, 70, 46), 1, true);
            D.Rect(s, 31, 72, 1, 15, C.Rgb(80, 54, 38));
            D.Rect(s, 34, 79, 1, 1, Gold);
            D.Box(s, 30, 47, 4, 8, 2, C.Rgb(44, 36, 46), 0, true);
            // platform floor behind the figure
            D.Ell(s, 32, 40, 22, 4.5, D.Hl(stone), 0, true);

            Chibi c, archer = null;
            double k = 1.15, g = 42;
            switch (i)
            {
                case 0:
                case 1:
                    c = new Chibi { K = k, Cx = 30, Ground = g, HeadW = 13, HeadH = 11 };
                    c.Body = i == 0 ? C.Rgb(84, 140, 70) : C.Rgb(110, 76, 140); c.Legs = Leather;
                    DrawChibi(s, c, 0); Hood(s, c, i == 0 ? C.Rgb(70, 124, 60) : C.Rgb(96, 62, 126));
                    archer = c;
                    break;
                case 6: // Flamethrower
                    D.Ell(s, 24, 30, 7, 7, Red, 2, true);
                    D.Rect(s, 20, 27, 8, 1, D.Hl(Red));
                    D.Box(s, 26, 26, 24, 7, 3, C.Rgb(96, 96, 104), 2, true);
                    D.Box(s, 46, 24, 6, 11, 2, C.Rgb(70, 70, 78), 1, true);
                    D.Poly(s, new double[] { 52, 26, 60, 22, 58, 29.5, 62, 33, 52, 33 }, FireA, 1, true);
                    D.Ell(s, 55, 29.5, 2.5, 2, FireC, 0, false);
                    break;
                case 7: // Ice Area Tower
                    D.Poly(s, new double[] { 32, 4, 41, 20, 32, 36, 23, 20 }, IceB, 3, true);
                    D.Stick(s, 32, 7, 32, 33, 1, C.Rgb(150, 210, 245), 0, false);
                    D.Poly(s, new double[] { 17, 22, 21, 16, 22, 28 }, IceA, 1, true);
                    D.Poly(s, new double[] { 47, 22, 43, 16, 42, 28 }, IceA, 1, true);
                    D.Rect(s, 12, 10, 2, 2, IceB); D.Rect(s, 50, 8, 2, 2, IceB); D.Rect(s, 54, 18, 1, 1, White); D.Rect(s, 9, 18, 1, 1, White);
                    break;
                default: // mages
                    Color[] robes = { Red, C.Rgb(70, 120, 200), C.Rgb(130, 104, 70), Purple };
                    Color[] orbs = { FireB, IceB, C.Rgb(150, 120, 80), Bolt };
                    int m = i - 2;
                    c = new Chibi { K = k, Cx = 30, Ground = g, HeadW = 13, HeadH = 11 };
                    c.Robe = true; c.Body = robes[m];
                    DrawChibi(s, c, 0); WizardHat(s, c, robes[m], Gold, m % 2 == 0 ? -2 : 2);
                    Staff(s, c, 14, orbs[m]);
                    break;
            }
            // parapet in front of the figure
            D.Box(s, 9, 38, 46, 9, 2, stone, 2, true);
            D.Rect(s, 10, 43, 44, 1, mortar);
            for (int x = 0; x < 4; x++)
            {
                double mx = x < 2 ? 9 + x * 8 : 40 + (x - 2) * 8;
                D.Box(s, mx, 33, 7, 6, 1, stone, 2, true);
            }
            // the archers hold their bow over the parapet
            if (archer != null)
            {
                archer.RHandY = 36;
                Bow(s, archer, i == 0 ? Metal : PoisonA);
                if (i == 1) D.Ell(s, archer.LHandX + 1, 38, 2, 2.6, PoisonA, 1, true);
            }
            D.Finish(s, 32, 86, 17, false);
            return s;
        }

        // ---------------- castle (128x96 art pixels, gate threshold at (64, 88)) ----------------
        public static Spr Castle(int stage)
        {
            Spr s = new Spr(128, 96);
            Color st = C.Rgb(176, 172, 164), mortar = D.Shd(st), roof = C.Rgb(206, 74, 58);
            Random r = new Random(5 + stage);
            D.Box(s, 18, 36, 92, 52, 0, st, 3, true);
            for (int x = 20; x < 108; x += 10) D.Box(s, x, 30, 6, 7, 1, st, 2, true);
            foreach (int tx in new[] { 2, 96 })
            {
                D.Box(s, tx, 24, 30, 64, 2, st, 3, true);
                for (int m = 0; m < 3; m++) D.Box(s, tx + m * 11, 18, 8, 7, 1, st, 2, true);
            }
            for (int row = 0; row < 10; row++)
            {
                int y = 40 + row * 5;
                foreach (int[] seg in new[] { new[] { 4, 30 }, new[] { 20, 94 }, new[] { 98, 124 } })
                {
                    if (y + 5 > 86) continue;
                    D.Rect(s, seg[0], y, seg[1] - seg[0], 1, mortar);
                    for (int x = seg[0] + 3 + (row % 2) * 4; x < seg[1]; x += 8) D.Rect(s, x, y + 1, 1, 4, mortar);
                }
            }
            if (stage < 2)
            {
                D.Poly(s, new double[] { -1, 20, 35, 20, 17, -2 }, roof, 3, true);
                D.Poly(s, new double[] { 93, 20, 129, 20, 111, -2 }, roof, 3, true);
                for (int y = 6; y < 20; y += 5) { D.Rect(s, 17 - (y + 2) * 0.8, y, (y + 2) * 1.6, 1, D.Shd(roof)); D.Rect(s, 111 - (y + 2) * 0.8, y, (y + 2) * 1.6, 1, D.Shd(roof)); }
            }
            foreach (int[] w in new[] { new[] { 13, 36 }, new[] { 13, 58 }, new[] { 107, 36 }, new[] { 107, 58 }, new[] { 34, 46 }, new[] { 88, 46 } })
            {
                D.Box(s, w[0] - 3, w[1], 6, 9, 3, C.Rgb(44, 36, 46), 0, true);
                if (stage < 2) D.Rect(s, w[0] - 1, w[1] + 3, 2, 5, C.Rgb(250, 206, 96));
            }
            if (stage < 2)
            {
                foreach (int bx in new[] { 44, 78 })
                {
                    D.Box(s, bx, 38, 7, 15, 0, Red, 1, true);
                    D.Poly(s, new double[] { bx, 52, bx + 7, 52, bx + 3.5, 56 }, Red, 0, true);
                    D.Ell(s, bx + 3.5, 44, 1.8, 1.8, Gold, 0, false);
                }
                D.Stick(s, 17, -1, 17, 8, 1, WoodD, 0, false);
            }
            D.Box(s, 48, 58, 32, 30, 15, stage < 2 ? C.Rgb(122, 80, 48) : C.Rgb(76, 52, 36), 2, true);
            for (int x = 52; x < 78; x += 6) D.Rect(s, x, 62, 1, 26, C.Rgb(90, 58, 38));
            D.Rect(s, 49, 70, 30, 2, C.Rgb(64, 60, 66)); D.Rect(s, 49, 80, 30, 2, C.Rgb(64, 60, 66));
            D.Box(s, 44, 86, 40, 3, 1, D.Shd(st), 0, true);
            if (stage == 0)
            {
                D.Poly(s, new double[] { 18, 0, 30, 3, 18, 7 }, Red, 0, true);
            }
            if (stage >= 1)
            {
                D.Stick(s, 26, 40, 22, 48, 1, D.Ink, 0, false); D.Stick(s, 22, 48, 27, 55, 1, D.Ink, 0, false);
                D.Stick(s, 100, 66, 104, 74, 1, D.Ink, 0, false); D.Stick(s, 104, 74, 99, 80, 1, D.Ink, 0, false);
                D.Box(s, 60, 40, 6, 4, 1, D.Deep(st), 0, true);
            }
            if (stage == 1)
            {
                D.Erase(s, (x, y) => y < 16 && x > 100 && y < (x - 100) * 1.2);
                Fire(s, 108, 16, 1); Fire(s, 30, 34, 0.8);
            }
            if (stage == 2)
            {
                D.Erase(s, (x, y) => y < 34 + (D.Hash(x / 5, 3) % 9) || (x > 20 && x < 106 && y < 42 + (D.Hash(x / 4, 7) % 10) && (x < 46 || x > 82)));
                for (int i = 0; i < 9; i++)
                {
                    int x = 6 + r.Next(116), y = 86 + r.Next(5);
                    if (x > 44 && x < 84) continue;
                    D.Ell(s, x, y, 3 + r.Next(3), 2 + r.Next(2), st, 1, true);
                }
                Fire(s, 22, 46, 1); Fire(s, 98, 48, 1.1); Fire(s, 60, 46, 0.8);
            }
            s.Outline(D.Ink);
            if (stage >= 1)
            {
                Smoke(s, stage == 1 ? 110 : 24, stage == 1 ? 6 : 34);
                if (stage == 2) Smoke(s, 100, 34);
            }
            s.EllipseBehind(64, 89, 62, 4, C.Shadow);
            return s;
        }

        static void Fire(Spr s, double x, double y, double k)
        {
            D.Poly(s, new[] { x - 4 * k, y, x - 3 * k, y - 5 * k, x - 1 * k, y - 3 * k, x, y - 9 * k, x + 2 * k, y - 4 * k, x + 4 * k, y - 6 * k, x + 4 * k, y }, FireA, 1, true);
            D.Poly(s, new[] { x - 2 * k, y, x - 1 * k, y - 3 * k, x, y - 6 * k, x + 2 * k, y - 2 * k, x + 2 * k, y }, FireB, 0, false);
            D.Ell(s, x, y - 1 * k, 1.3 * k, 1.2 * k, FireC, 0, false);
        }

        static void Smoke(Spr s, double x, double y)
        {
            Color a = C.Argb(170, 120, 116, 120), b = C.Argb(140, 150, 146, 150);
            s.Ellipse(x, y - 3, 4, 3.5, a); s.Ellipse(x + 3, y - 8, 3.5, 3, b); s.Ellipse(x + 1, y - 13, 3, 2.5, b);
        }

        // ---------------- scenery (64x64, ground at row 56; pedestal spot (32, 48); pond 128x64) ----------------
        public static Dictionary<string, Spr> Scenery(Theme t)
        {
            bool snow = t.Name == "snow", autumn = t.Name == "autumn", desert = t.Name == "desert";
            Color leaf = autumn ? C.Rgb(222, 126, 48) : desert ? C.Rgb(150, 150, 76) : C.Rgb(92, 168, 70);
            Color pine = C.Rgb(56, 128, 88);
            Dictionary<string, Spr> d = new Dictionary<string, Spr>();
            d["oak"] = Oak(leaf, snow);
            d["pine"] = Pine(pine, snow);
            d["bush"] = Bush(leaf, !autumn && !desert && !snow, snow);
            d["rock"] = Rock(snow);
            d["pebbles"] = Pebbles();
            d["flowers"] = Flowers();
            d["house_red"] = House(C.Rgb(204, 72, 58), snow);
            d["house_brown"] = House(C.Rgb(150, 96, 60), snow);
            d["pedestal"] = Pedestal();
            d["fence"] = Fence(snow);
            d["barrel"] = Barrel();
            d["crate"] = Crate();
            d["cactus"] = Cactus();
            d["mushrooms"] = Mushrooms();
            d["pond"] = Pond(t, snow);
            return d;
        }

        static Spr Done(Spr s, double rx)
        {
            s.Outline(D.Ink);
            s.EllipseBehind(32, 56.5, rx, Math.Max(2, rx * 0.22), C.Shadow);
            return s;
        }

        static Spr Oak(Color leaf, bool snow)
        {
            Spr s = new Spr(64, 64);
            D.Box(s, 28, 36, 8, 21, 2, Wood, 2, true);
            D.Ell(s, 22, 30, 11, 10, leaf, 3, true);
            D.Ell(s, 42, 30, 11, 10, leaf, 3, true);
            D.Ell(s, 32, 20, 14, 12, leaf, 3, true);
            D.Ell(s, 27, 15, 5, 3, D.Hl(leaf), 0, false);
            if (snow) D.Ell(s, 32, 11, 10, 3, White, 0, false);
            return Done(s, 14);
        }

        static Spr Pine(Color leaf, bool snow)
        {
            Spr s = new Spr(64, 64);
            D.Box(s, 29, 46, 6, 11, 1, Wood, 1, true);
            double[] w = { 20, 16, 11 };
            for (int i = 0; i < 3; i++)
            {
                double top = 6 + i * 10, bottom = 30 + i * 9;
                D.Poly(s, new[] { 32, top, 32 + w[2 - i], bottom, 32 - w[2 - i], bottom }, leaf, 3, true);
                if (snow) D.Poly(s, new[] { 32, top + 1, 32 + w[2 - i] * 0.45, top + 9, 32, top + 7, 32 - w[2 - i] * 0.45, top + 9 }, White, 0, false);
            }
            return Done(s, 14);
        }

        static Spr Bush(Color leaf, bool berries, bool snow)
        {
            Spr s = new Spr(64, 64);
            D.Ell(s, 24, 48, 9, 8, leaf, 2, true);
            D.Ell(s, 40, 48, 9, 8, leaf, 2, true);
            D.Ell(s, 32, 43, 11, 9, leaf, 3, true);
            if (berries) foreach (int[] b in new[] { new[] { 27, 42 }, new[] { 36, 46 }, new[] { 41, 41 }, new[] { 23, 49 } }) D.Box(s, b[0], b[1], 3, 3, 1, Red, 0, true);
            if (snow) D.Ell(s, 32, 36, 8, 2.5, White, 0, false);
            return Done(s, 15);
        }

        static Spr Rock(bool snow)
        {
            Spr s = new Spr(64, 64);
            Color c = C.Rgb(150, 148, 146);
            D.Ell(s, 32, 47, 15, 10, c, 3, true);
            D.Ell(s, 42, 51, 7, 5, c, 2, true);
            D.Stick(s, 26, 43, 30, 48, 1, D.Ink, 0, false);
            if (snow) D.Ell(s, 30, 39, 10, 3, White, 0, false);
            return Done(s, 16);
        }

        static Spr Pebbles()
        {
            Spr s = new Spr(64, 64);
            D.Ell(s, 26, 52, 6, 4, C.Rgb(156, 152, 148), 1, true);
            D.Ell(s, 37, 53, 4, 3, C.Rgb(140, 136, 132), 1, true);
            return Done(s, 11);
        }

        static Spr Flowers()
        {
            Spr s = new Spr(64, 64);
            Color[] cols = { C.Rgb(236, 90, 120), Gold, C.Rgb(150, 120, 230), White, C.Rgb(244, 140, 60) };
            int[,] p = { { 20, 46 }, { 28, 42 }, { 36, 47 }, { 44, 43 }, { 31, 51 } };
            for (int i = 0; i < 5; i++)
            {
                D.Stick(s, p[i, 0], p[i, 1], p[i, 0], 56, 1, C.Rgb(70, 140, 60), 0, false);
                D.Ell(s, p[i, 0] + 0.5, p[i, 1], 2.6, 2.6, cols[i], 0, true);
                D.Rect(s, p[i, 0], p[i, 1] - 0.5, 1, 1, Gold);
            }
            return Done(s, 13);
        }

        static Spr House(Color roof, bool snow)
        {
            Spr s = new Spr(64, 64);
            Color wall = C.Rgb(240, 226, 196);
            D.Box(s, 44, 18, 6, 12, 0, C.Rgb(150, 140, 136), 1, true);
            D.Box(s, 12, 34, 40, 23, 0, wall, 3, true);
            D.Poly(s, new double[] { 6, 36, 58, 36, 46, 16, 18, 16 }, roof, 3, true);
            for (int y = 21; y < 36; y += 5) D.Rect(s, 10, y, 44, 1, D.Shd(roof));
            if (snow) D.Poly(s, new double[] { 18, 16, 46, 16, 49, 22, 15, 22 }, White, 0, false);
            D.Box(s, 28, 44, 9, 13, 4, Wood, 1, true);
            D.Box(s, 16, 41, 8, 7, 1, C.Rgb(120, 180, 220), 0, true);
            D.Box(s, 41, 41, 8, 7, 1, C.Rgb(120, 180, 220), 0, true);
            D.Rect(s, 19, 41, 1, 7, D.Ink); D.Rect(s, 44, 41, 1, 7, D.Ink);
            return Done(s, 24);
        }

        public static Spr Pedestal()
        {
            Spr s = new Spr(64, 64);
            Color st = C.Rgb(170, 166, 158);
            D.Ell(s, 32, 51, 27, 10, D.Shd(st), 1, true);
            D.Box(s, 5, 46, 54, 6, 0, D.Shd(st), 1, false);
            D.Ell(s, 32, 46, 27, 9, st, 0, true);
            D.Ell(s, 32, 46, 21, 6, D.Hl(st), 0, true);
            foreach (int x in new[] { 12, 22, 32, 42, 52 }) D.Rect(s, x, 52, 1, 6, D.Ink);
            s.Outline(D.Ink);
            s.EllipseBehind(33, 59, 28, 4, C.Shadow);
            return s;
        }

        static Spr Fence(bool snow)
        {
            Spr s = new Spr(64, 64);
            D.Box(s, 6, 42, 52, 4, 1, Wood, 1, true);
            D.Box(s, 6, 49, 52, 4, 1, Wood, 1, true);
            foreach (int x in new[] { 8, 30, 52 }) D.Box(s, x - 2, 36, 5, 21, 1, Wood, 1, true);
            if (snow) D.Rect(s, 7, 41, 50, 1, White);
            return Done(s, 24);
        }

        static Spr Barrel()
        {
            Spr s = new Spr(64, 64);
            D.Box(s, 22, 36, 20, 21, 5, Wood, 3, true);
            D.Rect(s, 22, 41, 20, 2, MetalD); D.Rect(s, 22, 51, 20, 2, MetalD);
            D.Ell(s, 32, 37, 9, 2.5, D.Hl(Wood), 0, true);
            return Done(s, 11);
        }

        static Spr Crate()
        {
            Spr s = new Spr(64, 64);
            D.Box(s, 20, 36, 24, 21, 1, C.Rgb(186, 136, 82), 3, true);
            D.Stick(s, 22, 38, 42, 55, 2, C.Rgb(150, 104, 62), 0, true);
            D.Rect(s, 20, 36, 24, 2, C.Rgb(150, 104, 62));
            return Done(s, 13);
        }

        static Spr Cactus()
        {
            Spr s = new Spr(64, 64);
            Color g = C.Rgb(78, 160, 84);
            D.Box(s, 15, 26, 6, 14, 3, g, 1, true);
            D.Box(s, 15, 36, 14, 6, 3, g, 1, true);
            D.Box(s, 43, 22, 6, 16, 3, g, 1, true);
            D.Box(s, 36, 33, 13, 6, 3, g, 1, true);
            D.Box(s, 27, 14, 10, 43, 5, g, 2, true);
            for (int y = 20; y < 54; y += 6) { D.Rect(s, 30, y, 1, 1, White); D.Rect(s, 34, y + 3, 1, 1, White); }
            D.Ell(s, 32, 13, 3, 2.5, C.Rgb(240, 120, 170), 0, true);
            return Done(s, 12);
        }

        static Spr Mushrooms()
        {
            Spr s = new Spr(64, 64);
            int[,] m = { { 24, 52, 7 }, { 37, 54, 5 }, { 44, 49, 4 } };
            for (int i = 0; i < 3; i++)
            {
                int x = m[i, 0], y = m[i, 1], r = m[i, 2];
                D.Box(s, x - r * 0.35, y - r, r * 0.7, r + 3, 1, C.Rgb(240, 230, 214), 1, true);
                D.Ell(s, x, y - r, r + 1, r * 0.75, Red, 2, true);
                D.Rect(s, x - r * 0.5, y - r - 1, 2, 2, White); D.Rect(s, x + r * 0.35, y - r, 1, 1, White);
            }
            return Done(s, 13);
        }

        static Spr Pond(Theme t, bool snow)
        {
            Spr s = new Spr(128, 64);
            Color bank = snow ? C.Rgb(196, 210, 226) : C.Rgb(214, 196, 150);
            Color water = snow ? C.Rgb(176, 214, 238) : C.Rgb(84, 156, 214);
            D.Ell(s, 64, 33, 58, 25, bank, 2, true);
            D.Ell(s, 64, 33, 53, 21, water, 0, true);
            D.Ell(s, 64, 37, 50, 17, snow ? C.Rgb(160, 202, 230) : C.Rgb(70, 138, 200), 0, false);
            for (int i = 0; i < 6; i++) D.Rect(s, 32 + i * 11 + (i % 2) * 4, 26 + (i % 3) * 6, 6, 1, snow ? White : C.Rgb(170, 216, 240));
            if (!snow)
            {
                D.Ell(s, 44, 40, 4, 2.5, C.Rgb(90, 170, 72), 1, true); D.Ell(s, 84, 30, 3.5, 2, C.Rgb(90, 170, 72), 1, true);
                D.Rect(s, 44, 38, 2, 2, C.Rgb(240, 150, 190));
                foreach (int x in new[] { 14, 18, 110, 114 }) { D.Stick(s, x, 34, x, 22, 1.4, C.Rgb(80, 140, 60), 0, true); D.Box(s, x - 1, 20, 3, 5, 1, Wood, 0, true); }
            }
            else
            {
                D.Stick(s, 50, 30, 58, 36, 1, C.Rgb(130, 170, 210), 0, false); D.Stick(s, 58, 36, 72, 33, 1, C.Rgb(130, 170, 210), 0, false);
            }
            s.Outline(D.Ink);
            return s;
        }

        // ---------------- map ground (twice the map resolution) ----------------
        public static Spr Terrain(Theme t, List<Rectangle> bands, int seed)
        {
            int w = Maps.MapW * 2, h = Maps.MapH * 2;
            Spr m = new Spr(w, h);
            bool[,] path = new bool[w, h];
            foreach (Rectangle b in bands)
                for (int y = Math.Max(0, b.Top * 2); y < Math.Min(h, b.Bottom * 2); y++)
                    for (int x = Math.Max(0, b.Left * 2); x < Math.Min(w, b.Right * 2); x++) path[x, y] = true;
            Func<int, int, bool> P = (x, y) => path[Math.Max(0, Math.Min(w - 1, x)), Math.Max(0, Math.Min(h - 1, y))];
            Color g0 = t.Ground[0], g1 = t.Ground[1], g2 = t.Ground[2], g3 = t.Ground[3];
            Color d0 = t.Dirt[0], d1 = t.Dirt[1], d2 = t.Dirt[2], d3 = t.Dirt[3];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    if (path[x, y])
                    {
                        int dist = 4;
                        for (int dd = 1; dd <= 3 && dist == 4; dd++)
                            if (!P(x - dd, y) || !P(x + dd, y) || !P(x, y - dd) || !P(x, y + dd)) dist = dd;
                        bool top = !P(x, y - 1) || !P(x, y - 2) || !P(x, y - 3);
                        m.Set(x, y, dist == 1 ? D.Ink : dist == 2 ? d0 : (dist == 3 || top) ? d1 : d2);
                    }
                    else m.Set(x, y, g2);
                }
            // grass tufts and light specks, pebbles on the path
            for (int cy = 0; cy < h; cy += 9)
                for (int cx = 0; cx < w; cx += 11)
                {
                    int hh = D.Hash(cx + seed * 31, cy);
                    int x = cx + hh % 9, y = cy + (hh / 9) % 7;
                    if (x + 5 >= w || y + 3 >= h) continue;
                    bool onPath = path[x, y] && path[x + 4, y + 2];
                    bool nearPath = P(x, y) || P(x + 4, y) || P(x, y + 2) || P(x + 4, y + 2) || P(x - 2, y) || P(x + 6, y + 2);
                    if (onPath)
                    {
                        bool inner = P(x - 4, y) && P(x + 8, y) && P(x, y - 4) && P(x, y + 6);
                        if (inner && hh % 3 == 0) { D.Box(m, x, y, 4, 3, 1.4, d3, 0, false); D.Rect(m, x + 1, y + 3, 3, 1, d1); }
                    }
                    else if (!nearPath)
                    {
                        int kind = hh % 5;
                        if (kind < 2) { m.Set(x, y + 2, g1); m.Set(x + 1, y + 1, g1); m.Set(x + 2, y + 2, g1); m.Set(x + 3, y, g1); m.Set(x + 4, y + 2, g1); m.Set(x + 1, y + 2, g1); m.Set(x + 3, y + 2, g1); m.Set(x + 3, y + 1, g1); }
                        else if (kind == 2) { m.Set(x, y, g3); m.Set(x + 1, y, g3); }
                        else if (kind == 3 && hh % 7 == 0) { D.Rect(m, x, y, 3, 2, g0); D.Rect(m, x, y, 2, 1, g1); }
                    }
                }
            return m;
        }
    }

    public enum Mat { None, Brick, Rock, Wood, Metal, Leaf }

    // Turns a 1x sprite into a sprite with twice the resolution (1 game pixel = 2 art pixels):
    // Scale2x smooths the diagonals, the outline is drawn again with half the width, edges get a
    // light rim (top/left) and a shadow rim (bottom/right) and known palette colors get a material texture.
    public static class HiRes
    {
        public const int K = 2;

        static Dictionary<int, Mat> Set(Dictionary<int, Mat> d, Mat m, params Color[] colors)
        {
            foreach (Color c in colors) d[c.ToArgb()] = m;
            return d;
        }

        public static Dictionary<int, Mat> Building()
        {
            Dictionary<int, Mat> d = new Dictionary<int, Mat>();
            Set(d, Mat.Brick, C.S0, C.S1, C.S2, C.S3, C.S4);
            Set(d, Mat.Wood, C.W0, C.W1, C.W2, C.W3);
            Set(d, Mat.Metal, C.M0, C.M1, C.M2);
            Set(d, Mat.Leaf, C.TG0, C.TG1, C.TG2, C.TG3, C.GR0, C.GR1, C.GR2, C.GR3, C.GR4);
            return d;
        }

        public static Dictionary<int, Mat> Creature()
        {
            Dictionary<int, Mat> d = new Dictionary<int, Mat>();
            Set(d, Mat.Rock, C.S0, C.S1, C.S2, C.S3, C.S4);
            Set(d, Mat.Wood, C.W0, C.W1, C.W2, C.W3);
            Set(d, Mat.Metal, C.M0, C.M1, C.M2);
            Set(d, Mat.Leaf, C.TG0, C.TG1, C.TG2, C.TG3);
            return d;
        }

        public static Dictionary<int, Mat> Scenery()
        {
            Dictionary<int, Mat> d = new Dictionary<int, Mat>();
            Set(d, Mat.Rock, C.S0, C.S1, C.S2, C.S3, C.S4);
            Set(d, Mat.Wood, C.W0, C.W1, C.W2, C.W3);
            Set(d, Mat.Metal, C.M0, C.M1, C.M2);
            Set(d, Mat.Leaf, C.TG0, C.TG1, C.TG2, C.TG3, C.GR0, C.GR1, C.GR2, C.GR3, C.GR4);
            return d;
        }

        // Material map after a theme recolor: recolored palette entries keep their material.
        public static Dictionary<int, Mat> Recolored(Dictionary<int, Mat> mats, Dictionary<int, Color> recolor)
        {
            Dictionary<int, Mat> d = new Dictionary<int, Mat>(mats);
            foreach (KeyValuePair<int, Color> kv in recolor)
            {
                Mat m;
                if (mats.TryGetValue(kv.Key, out m)) d[kv.Value.ToArgb()] = m;
            }
            return d;
        }

        public static Spr Scale2x(Spr s)
        {
            Spr o = new Spr(s.W * 2, s.H * 2);
            for (int y = 0; y < s.H; y++)
                for (int x = 0; x < s.W; x++)
                {
                    Color p = s.Get(x, y);
                    int a = s.Get(x, y - 1).ToArgb(), b = s.Get(x + 1, y).ToArgb(), c = s.Get(x - 1, y).ToArgb(), d = s.Get(x, y + 1).ToArgb();
                    Color e0 = p, e1 = p, e2 = p, e3 = p;
                    if (c == a && c != d && a != b) e0 = s.Get(x, y - 1);
                    if (a == b && a != c && b != d) e1 = s.Get(x + 1, y);
                    if (d == c && d != b && c != a) e2 = s.Get(x - 1, y);
                    if (b == d && b != a && d != c) e3 = s.Get(x + 1, y);
                    o.Set(x * 2, y * 2, e0); o.Set(x * 2 + 1, y * 2, e1);
                    o.Set(x * 2, y * 2 + 1, e2); o.Set(x * 2 + 1, y * 2 + 1, e3);
                }
            return o;
        }

        public static Spr Nearest2x(Spr s)
        {
            Spr o = new Spr(s.W * 2, s.H * 2);
            for (int y = 0; y < o.H; y++) for (int x = 0; x < o.W; x++) o.Set(x, y, s.Get(x / 2, y / 2));
            return o;
        }

        static bool IsOut(Color c) { return c.A == 255 && c.ToArgb() == C.Out.ToArgb(); }

        public static Spr Refine(Spr src, Dictionary<int, Mat> mats)
        {
            // 1. split into body (opaque) and soft shadow (semi transparent); drop the outer outline
            Spr body = new Spr(src.W, src.H), shadow = new Spr(src.W, src.H);
            for (int y = 0; y < src.H; y++)
                for (int x = 0; x < src.W; x++)
                {
                    Color c = src.Get(x, y);
                    if (c.A == 0) continue;
                    if (c.A < 255) { shadow.Set(x, y, c); continue; }
                    bool outer = IsOut(c) && (src.Get(x - 1, y).A < 255 || src.Get(x + 1, y).A < 255 || src.Get(x, y - 1).A < 255 || src.Get(x, y + 1).A < 255);
                    if (!outer) body.Set(x, y, c);
                }

            // 2. upscale; remember the 1x colour of every pixel for the material lookup
            Spr hi = Scale2x(body);
            Spr baseColors = hi.Clone();

            // 3. texture by material, then light/shadow rims at the silhouette
            for (int y = 0; y < hi.H; y++)
                for (int x = 0; x < hi.W; x++)
                {
                    Color c = hi.Get(x, y);
                    if (c.A < 255 || IsOut(c)) continue;
                    Mat m;
                    if (mats != null && mats.TryGetValue(c.ToArgb(), out m)) hi.Set(x, y, Texture(m, c, x, y));
                }
            for (int y = 0; y < hi.H; y++)
                for (int x = 0; x < hi.W; x++)
                {
                    Color c = hi.Get(x, y);
                    if (c.A < 255 || IsOut(c)) continue;
                    bool lit = Edge(baseColors, x, y - 1) || Edge(baseColors, x - 1, y);
                    bool dark = Edge(baseColors, x, y + 1) || Edge(baseColors, x + 1, y);
                    if (lit && !dark) hi.Set(x, y, C.Light(c, 0.16));
                    else if (dark && !lit) hi.Set(x, y, C.Dark(c, 0.16));
                }

            // 4. thin outline around the new silhouette, soft shadow behind everything
            hi.Outline(C.Out);
            Spr sh = Nearest2x(shadow);
            for (int y = 0; y < hi.H; y++) for (int x = 0; x < hi.W; x++) hi.Behind(x, y, sh.Get(x, y));
            return hi;
        }

        static bool Edge(Spr s, int x, int y) { Color c = s.Get(x, y); return c.A < 255 || IsOut(c); }

        static int Hash(int x, int y)
        {
            unchecked
            {
                int h = x * 73856093 ^ y * 19349663;
                h = (h ^ (h >> 13)) * 1274126177;
                return Math.Abs(h ^ (h >> 16));
            }
        }

        public static Color Texture(Mat m, Color c, int x, int y)
        {
            int h = Hash(x, y);
            switch (m)
            {
                case Mat.Brick:
                {
                    int row = y / 4, off = (row % 2) * 5;
                    if (y % 4 == 3 || (x + off) % 10 == 9) return C.Dark(c, 0.17);
                    if (y % 4 == 0) return C.Light(c, 0.09);
                    return h % 11 == 0 ? C.Dark(c, 0.07) : c;
                }
                case Mat.Rock:
                    return h % 9 == 0 ? C.Light(c, 0.12) : h % 9 == 1 ? C.Dark(c, 0.14) : c;
                case Mat.Wood:
                    return (y + Hash(x / 3, 7) % 3) % 3 == 0 ? C.Dark(c, 0.11) : h % 13 == 0 ? C.Light(c, 0.1) : c;
                case Mat.Metal:
                    return (x + y) % 7 == 0 ? C.Light(c, 0.1) : c;
                case Mat.Leaf:
                    return h % 5 == 0 ? C.Light(c, 0.14) : h % 5 == 1 ? C.Dark(c, 0.15) : c;
            }
            return c;
        }

        // Fine grass, dirt and water detail for the upscaled map ground.
        public static void GroundDetail(Spr hi, Theme t, Spr mask)
        {
            Dictionary<int, int> ground = new Dictionary<int, int>(), dirt = new Dictionary<int, int>(), water = new Dictionary<int, int>();
            for (int i = 0; i < t.Ground.Length; i++) ground[t.Ground[i].ToArgb()] = i;
            for (int i = 0; i < t.Dirt.Length; i++) dirt[t.Dirt[i].ToArgb()] = i;
            for (int i = 0; i < t.Water.Length; i++) water[t.Water[i].ToArgb()] = i;
            for (int y = 0; y < hi.H; y++)
                for (int x = 0; x < hi.W; x++)
                {
                    if (mask != null && mask.Get(x, y).A > 0) continue;
                    Color c = hi.Get(x, y);
                    int k = c.ToArgb(), h = Hash(x + 1000, y);
                    if (ground.ContainsKey(k))
                    {
                        if (h % 17 == 0 && y > 0) { hi.Set(x, y, C.Light(c, 0.16)); hi.Set(x, y + 1, C.Dark(c, 0.1)); }
                        else if (h % 17 == 1) hi.Set(x, y, C.Dark(c, 0.1));
                    }
                    else if (dirt.ContainsKey(k) && dirt[k] >= 2)
                    {
                        if (h % 29 == 0) { hi.Set(x, y, C.Light(c, 0.22)); hi.Set(x, y + 1, C.Dark(c, 0.2)); }
                        else if (h % 29 == 1) hi.Set(x, y, C.Dark(c, 0.08));
                    }
                    else if (water.ContainsKey(k))
                    {
                        if ((x / 3 + y * 5) % 23 == 0) hi.Set(x, y, C.Light(c, 0.3));
                    }
                }
        }
    }

    public static class Program
    {
        public static void Run(string assetDir, string previewDir)
        {
            // 1x sheets (icons, terrain tiles) keep 4 image pixels per art pixel. The detailed sheets
            // have twice the art resolution and are saved with 2 image pixels per art pixel, so every
            // sheet still has 4 image pixels per game pixel and the game code needs no change.
            const int scale = 4;
            const int hiScale = scale / HiRes.K;
            const int k = HiRes.K;
            Directory.CreateDirectory(assetDir);

            List<Spr> tiles = World.TerrainTiles();
            SheetWriter.SaveRow(System.IO.Path.Combine(assetDir, "terrain_tiles.png"), tiles, 16, 16, tiles.Count, scale);

            // scenery sheet: row 0 = 8 cells of 32x32, row 1 = pond (64x32) + 6 cells (game pixels)
            Dictionary<string, Point> anchor = new Dictionary<string, Point>();
            string[] names = { "oak", "pine", "bush", "rock", "pebbles", "flowers", "house_red", "house_brown", "pedestal", "fence", "barrel", "crate", "cactus", "mushrooms" };
            for (int i = 0; i < names.Length; i++) anchor[names[i]] = new Point(16, 28);
            anchor["pedestal"] = new Point(16, 24);
            anchor["pond"] = new Point(32, 16);
            Dictionary<string, Spr> sc = Art.Scenery(Maps.GetTheme("meadow"));
            List<KeyValuePair<Point, Spr>> parts = new List<KeyValuePair<Point, Spr>>();
            for (int i = 0; i < 8; i++) parts.Add(new KeyValuePair<Point, Spr>(new Point(i * 32 * k, 0), sc[names[i]]));
            parts.Add(new KeyValuePair<Point, Spr>(new Point(0, 32 * k), sc["pond"]));
            for (int i = 8; i < names.Length; i++) parts.Add(new KeyValuePair<Point, Spr>(new Point((64 + (i - 8) * 32) * k, 32 * k), sc[names[i]]));
            SheetWriter.Save(System.IO.Path.Combine(assetDir, "scenery_sprites.png"), 256 * k, 64 * k, parts, hiScale);

            List<Spr> maps = new List<Spr>();
            foreach (MapDef def in Maps.All)
            {
                Theme theme = Maps.GetTheme(def.Theme);
                Spr map = Maps.Compose(def, theme, Art.Scenery(theme), anchor);
                maps.Add(map);
                SheetWriter.Save(System.IO.Path.Combine(assetDir, def.File), Maps.MapW * k, Maps.MapH * k,
                    new List<KeyValuePair<Point, Spr>> { new KeyValuePair<Point, Spr>(new Point(0, 0), map) }, hiScale);
            }

            List<Spr> castle = new List<Spr>();
            for (int i = 0; i < 3; i++) castle.Add(Art.Castle(i));
            SheetWriter.SaveRow(System.IO.Path.Combine(assetDir, "main_building.png"), castle, 64 * k, 48 * k, 3, hiScale);

            List<Spr> enemies = new List<Spr>();
            for (int f = 0; f < 2; f++) for (int i = 0; i < Creatures.EnemyNames.Length; i++) enemies.Add(Art.Enemy(i, f));
            SheetWriter.SaveRow(System.IO.Path.Combine(assetDir, "enemy_spritesheet.png"), enemies, 16 * k, 16 * k, Creatures.EnemyNames.Length, hiScale);

            List<Spr> bosses = new List<Spr>();
            for (int f = 0; f < 2; f++) for (int i = 0; i < Creatures.BossNames.Length; i++) bosses.Add(Art.Boss(i, f));
            SheetWriter.SaveRow(System.IO.Path.Combine(assetDir, "boss_spritesheet.png"), bosses, 32 * k, 32 * k, Creatures.BossNames.Length, hiScale);

            List<Spr> towers = new List<Spr>();
            for (int i = 0; i < 8; i++) towers.Add(Art.Tower(i));
            SheetWriter.SaveRow(System.IO.Path.Combine(assetDir, "tower_spritesheet.png"), towers, 32 * k, 48 * k, 8, hiScale);

            List<Spr> icons = new List<Spr>();
            for (int i = 0; i < Creatures.IconNames.Length; i++) icons.Add(Creatures.Icon(i));
            SheetWriter.SaveRow(System.IO.Path.Combine(assetDir, "ui_icons.png"), icons, 16, 16, icons.Count, scale);

            if (string.IsNullOrEmpty(previewDir)) return;
            Directory.CreateDirectory(previewDir);
            for (int m = 0; m < Maps.All.Length; m++)
            {
                MapDef def = Maps.All[m];
                Spr scene = new Spr(320 * k, 180 * k);
                scene.Rect(0, 0, 320 * k, 25 * k, C.Rgb(29, 47, 57));
                scene.Stamp(maps[m], 0, 25 * k);
                List<int> order = new List<int>();
                for (int i = 0; i < def.Spots.Length / 2; i++) order.Add(i);
                order.Sort((a, b) => def.Spots[a * 2 + 1].CompareTo(def.Spots[b * 2 + 1]));
                scene.Stamp(castle[0], (def.MainBuilding.X - 32) * k, (def.MainBuilding.Y + 27 - 44) * k);
                foreach (int i in order)
                    scene.Stamp(towers[i % 8], (def.Spots[i * 2] - 16) * k, (def.Spots[i * 2 + 1] + 27 - 43) * k);
                SheetWriter.Save(System.IO.Path.Combine(previewDir, "preview_" + System.IO.Path.GetFileNameWithoutExtension(def.File) + ".png"), 320 * k, 180 * k,
                    new List<KeyValuePair<Point, Spr>> { new KeyValuePair<Point, Spr>(new Point(0, 0), scene) }, 2);
            }
            SheetWriter.SaveRow(System.IO.Path.Combine(previewDir, "preview_enemies.png"), enemies, 16 * k, 16 * k, 10, 4);
            SheetWriter.SaveRow(System.IO.Path.Combine(previewDir, "preview_bosses.png"), bosses, 32 * k, 32 * k, 5, 3);
            SheetWriter.SaveRow(System.IO.Path.Combine(previewDir, "preview_icons.png"), icons, 16, 16, 9, 8);
            SheetWriter.SaveRow(System.IO.Path.Combine(previewDir, "preview_castle.png"), castle, 64 * k, 48 * k, 3, 3);
            SheetWriter.SaveRow(System.IO.Path.Combine(previewDir, "preview_towers.png"), towers, 32 * k, 48 * k, 8, 3);
        }
    }
}
'@

Add-Type -TypeDefinition $source -ReferencedAssemblies System.Drawing
[TowerDefenseSprites.Program]::Run($AssetDir, $PreviewDir)
Write-Host "Spritesheets geschrieben nach $AssetDir"
