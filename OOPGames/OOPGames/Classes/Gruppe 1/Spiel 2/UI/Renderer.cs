using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OOPGames
{
    /// <summary>
    /// Umrechnung zwischen dem virtuellen Bildschirm (480x320) und dem echten Canvas.
    /// Wird vom Painter gesetzt und von den Rules genutzt, um Mausklicks umzurechnen.
    /// </summary>
    public class G1C_Viewport
    {
        public double Scale { get; set; } = 1.0;
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }

        public double ToVirtualX(double canvasX) { return (canvasX - OffsetX) / Scale; }
        public double ToVirtualY(double canvasY) { return (canvasY - OffsetY) / Scale; }
    }

    /// <summary>Feste Farben der Benutzeroberfläche (Pokémon-ähnlicher Stil).</summary>
    public static class G1C_UiColors
    {
        public static readonly Color PanelFill = Color.FromRgb(0xF8, 0xF8, 0xF8);
        public static readonly Color PanelBorder = Color.FromRgb(0x30, 0x38, 0x58);
        public static readonly Color PanelInner = Color.FromRgb(0x68, 0x90, 0xD0);
        public static readonly Color Text = Color.FromRgb(0x30, 0x30, 0x38);
        public static readonly Color TextLight = Color.FromRgb(0xF8, 0xF8, 0xF8);
        public static readonly Color Gold = Color.FromRgb(0xF8, 0xD0, 0x48);
        public static readonly Color Win = Color.FromRgb(0x28, 0x98, 0x38);
        public static readonly Color Loss = Color.FromRgb(0xD0, 0x30, 0x30);
        public static readonly Color Felt = Color.FromRgb(0x1E, 0x6A, 0x3A);
        public static readonly Color FeltDark = Color.FromRgb(0x14, 0x4A, 0x28);
        public static readonly Color Highlight = Color.FromRgb(0xF8, 0xE0, 0x30);
        public static readonly Color HeaderBar = Color.FromRgb(0x20, 0x18, 0x30);
        public static readonly Color Disabled = Color.FromRgb(0x90, 0x90, 0x98);
        public static readonly Color RouletteRed = Color.FromRgb(0xC8, 0x20, 0x28);
        public static readonly Color RouletteBlack = Color.FromRgb(0x20, 0x20, 0x24);
        public static readonly Color RouletteGreen = Color.FromRgb(0x18, 0x90, 0x40);
    }

    /// <summary>
    /// Zeichnet ausschließlich auf das vom Framework übergebene Canvas.
    /// Alle Koordinaten sind virtuell (480x320) und werden auf die Canvas-Größe skaliert,
    /// damit das Spiel bei jeder Fenstergröße passt.
    /// </summary>
    public class G1C_Renderer
    {
        public const double ScreenWidth = 480;
        public const double ScreenHeight = 320;

        private static readonly FontFamily UiFont = new FontFamily("Consolas, Segoe UI Symbol");
        private readonly Dictionary<Color, SolidColorBrush> _brushes = new Dictionary<Color, SolidColorBrush>();
        private Canvas _canvas;

        public G1C_SpriteLibrary Sprites { get; }
        public G1C_Viewport Viewport { get; private set; } = new G1C_Viewport();

        public G1C_Renderer(G1C_SpriteLibrary sprites)
        {
            Sprites = sprites ?? throw new ArgumentNullException(nameof(sprites));
        }

        /// <summary>Startet ein neues Bild: Canvas leeren und Skalierung berechnen.</summary>
        public void BeginFrame(Canvas canvas)
        {
            _canvas = canvas ?? throw new ArgumentNullException(nameof(canvas));

            double cw = canvas.ActualWidth;
            double ch = canvas.ActualHeight;
            if (cw < 1 || ch < 1)
            {
                cw = ScreenWidth;
                ch = ScreenHeight;
            }

            double scale = Math.Min(cw / ScreenWidth, ch / ScreenHeight);
            Viewport = new G1C_Viewport
            {
                Scale = scale,
                OffsetX = (cw - ScreenWidth * scale) / 2,
                OffsetY = (ch - ScreenHeight * scale) / 2
            };

            canvas.Children.Clear();
            canvas.Background = Brushes.Black;
        }

        private double S { get { return Viewport.Scale; } }

        private SolidColorBrush Brush(Color color)
        {
            SolidColorBrush brush;
            if (!_brushes.TryGetValue(color, out brush))
            {
                brush = new SolidColorBrush(color);
                brush.Freeze();
                _brushes[color] = brush;
            }
            return brush;
        }

        private void Place(UIElement element, double x, double y)
        {
            Canvas.SetLeft(element, Viewport.OffsetX + x * S);
            Canvas.SetTop(element, Viewport.OffsetY + y * S);
            _canvas.Children.Add(element);
        }

        public void FillRect(double x, double y, double w, double h, Color fill)
        {
            DrawRect(x, y, w, h, fill, Colors.Transparent, 0);
        }

        public void DrawRect(double x, double y, double w, double h, Color fill, Color stroke, double strokeWidth)
        {
            Rectangle rect = new Rectangle
            {
                Width = Math.Max(0, w * S),
                Height = Math.Max(0, h * S),
                Fill = Brush(fill)
            };
            if (strokeWidth > 0)
            {
                rect.Stroke = Brush(stroke);
                rect.StrokeThickness = strokeWidth * S;
            }
            Place(rect, x, y);
        }

        public void FillEllipse(double cx, double cy, double rx, double ry, Color fill)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = Math.Max(0, rx * 2 * S),
                Height = Math.Max(0, ry * 2 * S),
                Fill = Brush(fill)
            };
            Place(ellipse, cx - rx, cy - ry);
        }

        /// <summary>Text zeichnen. Ist width > 0, wird der Text in dieser Breite zentriert.</summary>
        public void Text(string text, double x, double y, double size, Color color, double width = 0)
        {
            TextBlock block = new TextBlock
            {
                Text = text ?? "",
                FontFamily = UiFont,
                FontSize = Math.Max(1, size * S),
                FontWeight = FontWeights.Bold,
                Foreground = Brush(color)
            };
            if (width > 0)
            {
                block.Width = width * S;
                block.TextAlignment = TextAlignment.Center;
            }
            Place(block, x, y);
        }

        public void Sprite(string id, double x, double y, double w, double h, double rotationDegrees = 0)
        {
            DrawBitmap(Sprites.Get(id), x, y, w, h, rotationDegrees);
        }

        public void DrawBitmap(BitmapSource bitmap, double x, double y, double w, double h, double rotationDegrees = 0)
        {
            Image image = new Image
            {
                Source = bitmap,
                Width = Math.Max(0, w * S),
                Height = Math.Max(0, h * S),
                Stretch = Stretch.Fill
            };
            // Pixel-Art scharf skalieren (keine Weichzeichnung)
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            if (rotationDegrees != 0)
            {
                image.RenderTransformOrigin = new Point(0.5, 0.5);
                image.RenderTransform = new RotateTransform(rotationDegrees);
            }
            Place(image, x, y);
        }

        /// <summary>Fensterrahmen im Stil der GBA-Pokémon-Spiele.</summary>
        public void DrawPanel(double x, double y, double w, double h)
        {
            FillRect(x, y, w, h, G1C_UiColors.PanelBorder);
            DrawRect(x + 2, y + 2, w - 4, h - 4, G1C_UiColors.PanelFill, G1C_UiColors.PanelInner, 1.5);
        }
    }
}
