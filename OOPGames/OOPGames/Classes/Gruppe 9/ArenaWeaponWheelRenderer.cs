using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    internal sealed class ArenaWeaponWheelRenderer
    {
        private static readonly Brush OverlayBrush =
            new SolidColorBrush(Color.FromArgb(105, 0, 0, 0));
        private static readonly Brush SegmentBrush =
            new SolidColorBrush(Color.FromArgb(225, 28, 35, 44));
        private static readonly Brush SelectedSegmentBrush =
            new SolidColorBrush(Color.FromArgb(238, 166, 105, 48));
        private static readonly Brush SegmentStrokeBrush =
            new SolidColorBrush(Color.FromArgb(245, 220, 226, 232));

        public void Paint(Canvas canvas, ArenaField field, double width, double height)
        {
            Rectangle overlay = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = OverlayBrush,
                IsHitTestVisible = false
            };
            canvas.Children.Add(overlay);

            double scale = Math.Max(0.55, Math.Min(1, Math.Min(width, height) / 700));
            double outerRadius = 210 * scale;
            double innerRadius = 76 * scale;
            double segmentAngle = Math.PI * 2 / ArenaWeaponWheel.Weapons.Count;
            double startAngle = -Math.PI / 2 - segmentAngle / 2;
            int selectedIndex = field.WeaponWheelSelectionIndex;

            for (int index = 0; index < ArenaWeaponWheel.Weapons.Count; index++)
            {
                double start = startAngle + segmentAngle * index;
                double end = start + segmentAngle;
                bool selected = index == selectedIndex;
                Path segment = new Path
                {
                    Data = CreateSegmentGeometry(
                        width / 2, height / 2,
                        innerRadius, outerRadius, start, end),
                    Fill = selected ? SelectedSegmentBrush : SegmentBrush,
                    Stroke = SegmentStrokeBrush,
                    StrokeThickness = selected ? 2.5 * scale : 1.25 * scale,
                    StrokeLineJoin = PenLineJoin.Round,
                    IsHitTestVisible = false
                };
                canvas.Children.Add(segment);

                double middle = (start + end) / 2;
                double labelRadius = (innerRadius + outerRadius) / 2;
                double labelWidth = 116 * scale;
                double labelHeight = 42 * scale;
                TextBlock label = new TextBlock
                {
                    Width = labelWidth,
                    Height = labelHeight,
                    Text = ArenaWeaponWheel.Weapons[index].Name,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 15 * scale,
                    FontWeight = selected ? FontWeights.Bold : FontWeights.SemiBold,
                    Foreground = Brushes.White,
                    TextWrapping = TextWrapping.Wrap,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(label,
                    width / 2 + Math.Cos(middle) * labelRadius - labelWidth / 2);
                Canvas.SetTop(label,
                    height / 2 + Math.Sin(middle) * labelRadius - labelHeight / 2);
                canvas.Children.Add(label);
            }

            TextBlock hint = new TextBlock
            {
                Width = width,
                Text = "WAFFE WÄHLEN  ·  1 LOSLASSEN ZUM AUSWÄHLEN",
                TextAlignment = TextAlignment.Center,
                FontSize = 14 * scale,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                IsHitTestVisible = false
            };
            Canvas.SetTop(hint, Math.Max(12, height / 2 - outerRadius - 34 * scale));
            canvas.Children.Add(hint);
        }

        private static Geometry CreateSegmentGeometry(
            double centerX, double centerY, double innerRadius,
            double outerRadius, double startAngle, double endAngle)
        {
            Point outerStart = GetPoint(
                centerX, centerY, outerRadius, startAngle);
            Point outerEnd = GetPoint(
                centerX, centerY, outerRadius, endAngle);
            Point innerEnd = GetPoint(
                centerX, centerY, innerRadius, endAngle);
            Point innerStart = GetPoint(
                centerX, centerY, innerRadius, startAngle);

            PathFigure figure = new PathFigure
            {
                StartPoint = outerStart,
                IsClosed = true
            };
            figure.Segments.Add(new ArcSegment(
                outerEnd, new Size(outerRadius, outerRadius), 0,
                endAngle - startAngle > Math.PI,
                SweepDirection.Clockwise, true));
            figure.Segments.Add(new LineSegment(innerEnd, true));
            figure.Segments.Add(new ArcSegment(
                innerStart, new Size(innerRadius, innerRadius), 0,
                endAngle - startAngle > Math.PI,
                SweepDirection.Counterclockwise, true));
            return new PathGeometry(new[] { figure });
        }

        private static Point GetPoint(
            double centerX, double centerY, double radius, double angle)
        {
            return new Point(
                centerX + Math.Cos(angle) * radius,
                centerY + Math.Sin(angle) * radius);
        }
    }
}
