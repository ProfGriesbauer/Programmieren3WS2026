using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;

namespace OOPGames
{
    internal enum ArenaSkillTreeTileState
    {
        Start,
        Fork,
        Locked,
        Available,
        Unlocked
    }

    internal static class ArenaSkillTreeVisuals
    {
        public const double NodeWidth = 232;
        public const double NodeHeight = 144;

        private static readonly Brush WireOutline =
            new SolidColorBrush(Color.FromRgb(5, 11, 18));

        public static Border CreateNodeTile(
            string title, string subtitle, ArenaSkillTreeTileState state)
        {
            Color accent = GetAccentColor(state);
            Border tile = new Border
            {
                Width = NodeWidth,
                Height = NodeHeight,
                Padding = new Thickness(14, 12, 14, 12),
                CornerRadius = new CornerRadius(12),
                BorderThickness = new Thickness(1.5),
                BorderBrush = new SolidColorBrush(Color.FromArgb(
                    235, accent.R, accent.G, accent.B)),
                Background = new LinearGradientBrush(
                    Color.FromRgb(31, 43, 57),
                    Color.FromRgb(15, 22, 31),
                    90),
                Effect = new DropShadowEffect
                {
                    BlurRadius = 12,
                    ShadowDepth = 2,
                    Opacity = 0.45,
                    Color = accent
                }
            };

            StackPanel content = new StackPanel
            {
                VerticalAlignment = VerticalAlignment.Center
            };
            content.Children.Add(new Border
            {
                Height = 3,
                Width = 42,
                Background = new SolidColorBrush(accent),
                CornerRadius = new CornerRadius(2),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });
            content.Children.Add(new TextBlock
            {
                Text = title,
                FontSize = 17,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Stretch
            });
            if (!String.IsNullOrWhiteSpace(subtitle))
            {
                content.Children.Add(new TextBlock
                {
                    Text = subtitle,
                    FontSize = 12,
                    Foreground = new SolidColorBrush(Color.FromRgb(185, 199, 215)),
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    Margin = new Thickness(0, 6, 0, 0)
                });
            }
            tile.Child = content;
            return tile;
        }

        public static Border CreateForkTile(bool forceDecision)
        {
            return CreateNodeTile(
                "GABELUNG",
                forceDecision ? "Entscheidung erzwingen" : "Freie Wahl",
                ArenaSkillTreeTileState.Fork);
        }

        public static void DrawConnections(
            Canvas canvas,
            ArenaSkillTreeDocument tree,
            Func<ArenaSkillTreeConnection, Brush> getBrush)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }
            if (tree == null)
            {
                throw new ArgumentNullException(nameof(tree));
            }
            if (getBrush == null)
            {
                throw new ArgumentNullException(nameof(getBrush));
            }

            foreach (ArenaSkillTreeConnection connection in tree.Connections)
            {
                ArenaSkillTreeNode from = tree.Nodes.Find(
                    node => node.SkillId == connection.FromSkillId);
                ArenaSkillTreeNode to = tree.Nodes.Find(
                    node => node.SkillId == connection.ToSkillId);
                if (from == null || to == null)
                {
                    throw new InvalidOperationException(
                        "A skill tree connection refers to a missing node.");
                }
                DrawConnection(canvas, from, to, getBrush(connection));
            }
        }

        private static void DrawConnection(
            Canvas canvas,
            ArenaSkillTreeNode from,
            ArenaSkillTreeNode to,
            Brush brush)
        {
            bool horizontal = Math.Abs(to.X - from.X) >= Math.Abs(to.Y - from.Y);
            Point start;
            Point end;
            Point firstBend;
            Point secondBend;
            if (horizontal)
            {
                bool goesRight = to.X >= from.X;
                start = new Point(
                    from.X + (goesRight ? NodeWidth : 0),
                    from.Y + NodeHeight / 2);
                end = new Point(
                    to.X + (goesRight ? 0 : NodeWidth),
                    to.Y + NodeHeight / 2);
                double middleX = (start.X + end.X) / 2;
                firstBend = new Point(middleX, start.Y);
                secondBend = new Point(middleX, end.Y);
            }
            else
            {
                bool goesDown = to.Y >= from.Y;
                start = new Point(
                    from.X + NodeWidth / 2,
                    from.Y + (goesDown ? NodeHeight : 0));
                end = new Point(
                    to.X + NodeWidth / 2,
                    to.Y + (goesDown ? 0 : NodeHeight));
                double middleY = (start.Y + end.Y) / 2;
                firstBend = new Point(start.X, middleY);
                secondBend = new Point(end.X, middleY);
            }

            Point[] route = { start, firstBend, secondBend, end };
            Polyline outline = CreatePolyline(route, WireOutline, 9);
            Polyline wire = CreatePolyline(route, brush, 4);
            canvas.Children.Add(outline);
            canvas.Children.Add(wire);

            Vector direction = end - secondBend;
            if (direction.Length < 1)
            {
                direction = end - firstBend;
            }
            if (direction.Length >= 1)
            {
                direction.Normalize();
                Vector perpendicular = new Vector(-direction.Y, direction.X);
                Point basePoint = end - direction * 13;
                Polygon arrowOutline = CreateArrow(
                    end, basePoint, perpendicular, 8, WireOutline);
                Polygon arrow = CreateArrow(
                    end, end - direction * 10, perpendicular, 5, brush);
                canvas.Children.Add(arrowOutline);
                canvas.Children.Add(arrow);
            }
        }

        private static Polyline CreatePolyline(
            Point[] points, Brush stroke, double thickness)
        {
            Polyline polyline = new Polyline
            {
                Stroke = stroke,
                StrokeThickness = thickness,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false
            };
            foreach (Point point in points)
            {
                polyline.Points.Add(point);
            }
            return polyline;
        }

        private static Polygon CreateArrow(
            Point tip, Point basePoint, Vector perpendicular,
            double halfWidth, Brush fill)
        {
            return new Polygon
            {
                Points = new PointCollection
                {
                    tip,
                    basePoint + perpendicular * halfWidth,
                    basePoint - perpendicular * halfWidth
                },
                Fill = fill,
                IsHitTestVisible = false
            };
        }

        private static Color GetAccentColor(ArenaSkillTreeTileState state)
        {
            switch (state)
            {
                case ArenaSkillTreeTileState.Start:
                    return Color.FromRgb(255, 190, 75);
                case ArenaSkillTreeTileState.Fork:
                    return Color.FromRgb(186, 128, 255);
                case ArenaSkillTreeTileState.Available:
                    return Color.FromRgb(78, 196, 255);
                case ArenaSkillTreeTileState.Unlocked:
                    return Color.FromRgb(83, 218, 164);
                default:
                    return Color.FromRgb(112, 132, 154);
            }
        }
    }
}
