using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    internal sealed class SatisfactoryGameMentor
    {
        private static readonly Brush PanelBrush = BrushFromRgb(24, 35, 38, 244);
        private static readonly Brush AccentBrush = BrushFromRgb(130, 229, 107);
        private static readonly Brush TextBrush = BrushFromRgb(248, 246, 224);
        private static readonly Brush MutedBrush = BrushFromRgb(195, 207, 183);

        internal bool IsActive { get; private set; }
        internal string Message { get; private set; }

        internal void Begin()
        {
            IsActive = true;
            Explain("Willkommen! Bewege dich mit WASD. Schaue ein Feld an und drücke E, um dort selbst Rohstoffe abzubauen.");
        }

        internal void Explain(string message)
        {
            if (IsActive && !string.IsNullOrWhiteSpace(message))
            {
                Message = message;
            }
        }

        internal void Skip()
        {
            IsActive = false;
            Message = "Tutorial übersprungen. Du kannst jederzeit frei spielen.";
        }

        internal void Reset()
        {
            IsActive = false;
            Message = string.Empty;
        }

        internal void Paint(
            Canvas canvas,
            double canvasWidth,
            double canvasHeight,
            IG5_WeltSpielfeld world)
        {
            if (!IsActive || canvas == null || world == null ||
                canvasWidth <= 0 || canvasHeight <= 0)
            {
                return;
            }

            canvas.Width = canvasWidth;
            canvas.Height = canvasHeight;
            canvas.Children.Clear();

            double panelWidth = Math.Min(460, canvasWidth - 32);
            double panelHeight = Math.Min(132, canvasHeight - 32);
            double left = 16;
            double top = Math.Max(68, canvasHeight - panelHeight - 16);

            AddRoundedRectangle(canvas, PanelBrush, AccentBrush, left, top, panelWidth, panelHeight);
            AddText(canvas, "FABRIK-MENTOR", AccentBrush,
                left + 18, top + 12, panelWidth - 36, 18, 11, FontWeights.Bold);
            AddText(canvas, Message, TextBrush,
                left + 18, top + 36, panelWidth - 36, 58, 12, FontWeights.SemiBold);
            AddText(canvas, "T: Tutorial überspringen", MutedBrush,
                left + 18, top + 104, panelWidth - 36, 16, 9, FontWeights.Normal);
        }

        private static void AddRoundedRectangle(
            Canvas canvas,
            Brush fill,
            Brush stroke,
            double left,
            double top,
            double width,
            double height)
        {
            Rectangle rectangle = new Rectangle
            {
                Width = width,
                Height = height,
                RadiusX = 12,
                RadiusY = 12,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            canvas.Children.Add(rectangle);
        }

        private static void AddText(
            Canvas canvas,
            string text,
            Brush foreground,
            double left,
            double top,
            double width,
            double height,
            double fontSize,
            FontWeight fontWeight)
        {
            TextBlock block = new TextBlock
            {
                Text = text ?? string.Empty,
                Foreground = foreground,
                FontSize = fontSize,
                FontWeight = fontWeight,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            Canvas.SetLeft(block, left);
            Canvas.SetTop(block, top);
            block.Width = width;
            block.Height = height;
            canvas.Children.Add(block);
        }

        private static Brush BrushFromRgb(byte red, byte green, byte blue, byte alpha = 255)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(alpha, red, green, blue));
            brush.Freeze();
            return brush;
        }
    }
}
