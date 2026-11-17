using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OOPGames
{
    sealed class TowerDefensePixelSurface
    {
        readonly DrawingGroup _Drawing = new DrawingGroup();
        readonly Dictionary<int, SolidColorBrush> _Brushes = new Dictionary<int, SolidColorBrush>();
        DrawingContext _Context;

        public TowerDefensePixelSurface(int logicalWidth, int logicalHeight, int pixelScale)
        {
            if (logicalWidth <= 0) throw new ArgumentOutOfRangeException(nameof(logicalWidth));
            if (logicalHeight <= 0) throw new ArgumentOutOfRangeException(nameof(logicalHeight));
            if (pixelScale <= 0) throw new ArgumentOutOfRangeException(nameof(pixelScale));

            _PixelScale = pixelScale;
            Image = new Image
            {
                Width = logicalWidth * pixelScale,
                Height = logicalHeight * pixelScale,
                Stretch = Stretch.None,
                SnapsToDevicePixels = true,
                UseLayoutRounding = true,
                Source = new DrawingImage(_Drawing)
            };
            RenderOptions.SetEdgeMode(Image, EdgeMode.Aliased);
            RenderOptions.SetBitmapScalingMode(Image, BitmapScalingMode.NearestNeighbor);
        }

        readonly int _PixelScale;

        public Image Image { get; private set; }

        public void Begin()
        {
            _Context = _Drawing.Open();
        }

        public void End()
        {
            if (_Context == null) return;
            _Context.Close();
            _Context = null;
        }

        public void Rect(double x, double y, double width, double height, Color color)
        {
            if (_Context == null) throw new InvalidOperationException("The pixel surface is not drawing.");
            _Context.DrawRectangle(GetBrush(color), null,
                new Rect(x * _PixelScale, y * _PixelScale, width * _PixelScale, height * _PixelScale));
        }

        public void Text(string text, double x, double y, int size, Color color)
        {
            if (_Context == null) throw new InvalidOperationException("The pixel surface is not drawing.");
            FormattedText formattedText = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Consolas"),
                size * _PixelScale,
                GetBrush(color),
                1.0);
            _Context.DrawText(formattedText, new Point(x * _PixelScale, y * _PixelScale));
        }

        SolidColorBrush GetBrush(Color color)
        {
            int key = color.A << 24 | color.R << 16 | color.G << 8 | color.B;
            SolidColorBrush brush;
            if (!_Brushes.TryGetValue(key, out brush))
            {
                brush = new SolidColorBrush(color);
                brush.Freeze();
                _Brushes.Add(key, brush);
            }

            return brush;
        }
    }
}
