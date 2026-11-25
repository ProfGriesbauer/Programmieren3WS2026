using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    struct G10_TowerDefenseClick
    {
        public G10_TowerDefenseClick(Point position, bool right)
        {
            Position = position;
            Right = right;
        }

        public Point Position { get; private set; }
        public bool Right { get; private set; }
    }

    sealed class G10_TowerDefensePixelSurface
    {
        readonly DrawingGroup _Drawing = new DrawingGroup();
        readonly Dictionary<int, SolidColorBrush> _Brushes = new Dictionary<int, SolidColorBrush>();
        readonly int _PixelScale;
        DrawingContext _Context;

        public G10_TowerDefensePixelSurface(int logicalWidth, int logicalHeight, int pixelScale)
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
            RenderOptions.SetBitmapScalingMode(_Drawing, BitmapScalingMode.NearestNeighbor);
            // Fixed bounds: sprites that move partly off-screen must not shift the image origin.
            _Bounds = new Rect(0, 0, logicalWidth * pixelScale, logicalHeight * pixelScale);
            RectangleGeometry clip = new RectangleGeometry(_Bounds);
            clip.Freeze();
            _Drawing.ClipGeometry = clip;

            Image.MouseMove += (sender, e) => MousePosition = ToLogical(e.GetPosition(Image));
            Image.MouseLeave += (sender, e) => MousePosition = null;
            Image.MouseDown += (sender, e) =>
            {
                Point position = ToLogical(e.GetPosition(Image));
                MousePosition = position;
                if (e.ChangedButton == System.Windows.Input.MouseButton.Left || e.ChangedButton == System.Windows.Input.MouseButton.Right)
                    _Clicks.Enqueue(new G10_TowerDefenseClick(position, e.ChangedButton == System.Windows.Input.MouseButton.Right));
            };
        }

        readonly Rect _Bounds;
        readonly Queue<G10_TowerDefenseClick> _Clicks = new Queue<G10_TowerDefenseClick>();

        // Mouse position in logical pixels, null while the mouse is outside of the game.
        public Point? MousePosition { get; private set; }

        public List<G10_TowerDefenseClick> DrainClicks()
        {
            List<G10_TowerDefenseClick> clicks = new List<G10_TowerDefenseClick>(_Clicks);
            _Clicks.Clear();
            return clicks;
        }

        Point ToLogical(Point imagePosition)
        {
            return new Point(imagePosition.X / _PixelScale, imagePosition.Y / _PixelScale);
        }

        public Image Image { get; private set; }

        public void Begin()
        {
            _Context = _Drawing.Open();
            _Context.DrawRectangle(Brushes.Transparent, null, _Bounds);
        }

        public void End()
        {
            if (_Context == null) return;
            _Context.Close();
            _Context = null;
        }

        public void Rect(double x, double y, double width, double height, Color color)
        {
            EnsureDrawing();
            _Context.DrawRectangle(GetBrush(color), null,
                new Rect(x * _PixelScale, y * _PixelScale, width * _PixelScale, height * _PixelScale));
        }

        public void Text(string text, double x, double y, int size, Color color)
        {
            EnsureDrawing();
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

        // Draws a sprite with the given opacity (0..1).
        public void Sprite(BitmapSource sprite, double x, double y, double width, double height, double opacity)
        {
            EnsureDrawing();
            _Context.PushOpacity(opacity);
            Sprite(sprite, x, y, width, height);
            _Context.Pop();
        }

        // Draws a sprite in logical units. The position is rounded to whole device pixels so pixel art stays sharp.
        public void Sprite(BitmapSource sprite, double x, double y, double width, double height, bool flipX = false)
        {
            Sprite(sprite, x, y, width, height, flipX, 0, 0);
        }

        // rotation: degrees around the sprite's centre. flash: 0..1, paints the sprite's shape white (hit feedback).
        public void Sprite(BitmapSource sprite, double x, double y, double width, double height, bool flipX, double rotation, double flash)
        {
            EnsureDrawing();
            Rect target = new Rect(Math.Round(x * _PixelScale), Math.Round(y * _PixelScale),
                Math.Round(width * _PixelScale), Math.Round(height * _PixelScale));
            int pushed = 0;
            if (rotation != 0)
            {
                _Context.PushTransform(new RotateTransform(rotation, target.X + target.Width / 2, target.Y + target.Height / 2));
                pushed++;
            }
            if (flipX)
            {
                _Context.PushTransform(new ScaleTransform(-1, 1, target.X + target.Width / 2, 0));
                pushed++;
            }
            _Context.DrawImage(sprite, target);
            if (flash > 0)
            {
                _Context.PushOpacityMask(GetMask(sprite));
                _Context.DrawRectangle(GetBrush(Color.FromArgb((byte)(Math.Min(1, flash) * 255), 255, 255, 255)), null, target);
                _Context.Pop();
            }
            for (int i = 0; i < pushed; i++) _Context.Pop();
        }

        readonly Dictionary<BitmapSource, ImageBrush> _Masks = new Dictionary<BitmapSource, ImageBrush>();

        ImageBrush GetMask(BitmapSource sprite)
        {
            ImageBrush mask;
            if (!_Masks.TryGetValue(sprite, out mask))
            {
                mask = new ImageBrush(sprite) { Stretch = Stretch.Fill };
                RenderOptions.SetBitmapScalingMode(mask, BitmapScalingMode.NearestNeighbor);
                mask.Freeze();
                _Masks.Add(sprite, mask);
            }
            return mask;
        }

        // Everything drawn until PopOpacity is drawn with this opacity (0..1).
        public void PushOpacity(double opacity)
        {
            EnsureDrawing();
            _Context.PushOpacity(opacity);
        }

        public void PopOpacity()
        {
            EnsureDrawing();
            _Context.Pop();
        }

        // Everything drawn until PopClip is cut to this rectangle (logical pixels).
        public void PushClip(double x, double y, double width, double height)
        {
            EnsureDrawing();
            _Context.PushClip(new RectangleGeometry(new Rect(x * _PixelScale, y * _PixelScale, width * _PixelScale, height * _PixelScale)));
        }

        public void PopClip()
        {
            EnsureDrawing();
            _Context.Pop();
        }

        void EnsureDrawing()
        {
            if (_Context == null) throw new InvalidOperationException("The pixel surface is not drawing.");
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
