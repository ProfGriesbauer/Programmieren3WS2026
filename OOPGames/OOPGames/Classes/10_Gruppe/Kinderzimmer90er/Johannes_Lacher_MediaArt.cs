using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    // Drawing helpers for the printed labels of cartridges and disks. Everything is drawn in code
    // from the game's title and colour, so new games get a matching label automatically.
    internal static class B5MediaArt
    {
        internal static readonly Typeface TitleFont = new Typeface(new FontFamily("Arial Black"), FontStyles.Normal, FontWeights.Black, FontStretches.Normal);
        internal static readonly Typeface PrintFont = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        internal static readonly Typeface HandFont = new Typeface(new FontFamily("Segoe Print"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

        // Evening sky with stars and a striped sun going down behind a grid landscape, in the game's colour.
        internal static void DrawArtwork(DrawingContext context, Rect picture, Color color, int seed)
        {
            Color night = Mix(color, Color.FromRgb(16, 18, 52), 0.82);
            Color glow = Mix(color, Colors.White, 0.35);
            double horizon = picture.Top + picture.Height * 0.76;
            double centerX = picture.Left + picture.Width / 2;
            double sunRadius = picture.Height * 0.36;

            LinearGradientBrush sky = new LinearGradientBrush(night, Mix(color, night, 0.35), new Point(0, picture.Top), new Point(0, horizon))
            {
                MappingMode = BrushMappingMode.Absolute
            };
            context.PushClip(new RectangleGeometry(picture));
            context.DrawRectangle(sky, null, picture);
            Random random = new Random(seed);
            for (int i = 0; i < 26; i++)
            {
                double size = (0.8 + random.NextDouble() * 1.6) * picture.Width / 340;
                context.DrawEllipse(new SolidColorBrush(Color.FromArgb(200, 255, 247, 222)), null,
                    new Point(picture.Left + random.NextDouble() * picture.Width, picture.Top + random.NextDouble() * picture.Height * 0.4), size, size);
            }
            RadialGradientBrush sun = new RadialGradientBrush(Mix(glow, Colors.White, 0.4), color);
            context.DrawEllipse(sun, null, new Point(centerX, horizon - sunRadius * 0.38), sunRadius, sunRadius);
            for (int i = 0; i < 4; i++)
            {
                double y = horizon - sunRadius * 0.7 + i * sunRadius * 0.17;
                context.DrawRectangle(sky, null, new Rect(centerX - sunRadius * 1.3, y, sunRadius * 2.6, sunRadius * (0.03 + i * 0.025)));
            }

            context.DrawRectangle(new SolidColorBrush(Mix(color, Colors.Black, 0.78)), null,
                new Rect(picture.Left, horizon, picture.Width, picture.Bottom - horizon));
            double line = Math.Max(0.8, picture.Width / 210);
            Pen grid = new Pen(new SolidColorBrush(Color.FromArgb(190, glow.R, glow.G, glow.B)), line);
            for (int i = -7; i <= 7; i++)
                context.DrawLine(grid, new Point(centerX + i * picture.Width * 0.026, horizon), new Point(centerX + i * picture.Width * 0.2, picture.Bottom));
            for (double step = picture.Height * 0.017; horizon + step < picture.Bottom; step *= 1.7)
                context.DrawLine(grid, new Point(picture.Left, horizon + step), new Point(picture.Right, horizon + step));
            context.DrawLine(new Pen(new SolidColorBrush(glow), line * 1.5), new Point(picture.Left, horizon), new Point(picture.Right, horizon));
            context.Pop();
        }

        // Draws text left-aligned and vertically centred in area, shrunk until it fits.
        internal static void DrawFitted(DrawingContext context, string text, Typeface font, Color color, Rect area)
        {
            FormattedText formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                font, area.Height, new SolidColorBrush(color), 1.0);
            double scale = Math.Min(1, Math.Min(area.Width / formatted.WidthIncludingTrailingWhitespace, area.Height / formatted.Height));
            formatted.SetFontSize(area.Height * scale);
            context.DrawText(formatted, new Point(area.Left, area.Top + (area.Height - formatted.Height) / 2));
        }

        internal static Brush Finish(DrawingGroup drawing)
        {
            DrawingBrush brush = new DrawingBrush(drawing) { Stretch = Stretch.Fill };
            Brush texture = B5RoomTextures.ToBitmap(brush);
            return texture;
        }

        internal static Color Mix(Color first, Color second, double amount)
        {
            return Color.FromRgb(
                (byte)(first.R + (second.R - first.R) * amount),
                (byte)(first.G + (second.G - first.G) * amount),
                (byte)(first.B + (second.B - first.B) * amount));
        }
    }
}
