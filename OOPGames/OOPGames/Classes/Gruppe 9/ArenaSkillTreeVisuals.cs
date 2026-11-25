using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
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
        public const double EditorBoardWidth = 4096;
        public const double EditorBoardHeight = 3072;
        private const double ForkMarkerDiameter = 32;

        private static readonly Brush WireOutline =
            new SolidColorBrush(Color.FromRgb(5, 11, 18));
        private static readonly Brush FreeChoiceForkBrush = Brushes.DeepSkyBlue;
        private static readonly Brush UndecidedForcedForkBrush = Brushes.MediumPurple;
        private static readonly Brush DecidedForcedForkBrush = Brushes.Red;
        private static readonly Brush WaitingForkBrush = Brushes.Gray;

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

        public static Border CreateStartNodeTile(BitmapSource characterSprite)
        {
            if (characterSprite == null)
            {
                throw new ArgumentNullException(nameof(characterSprite));
            }
            Border tile = CreateNodeTile(
                null, null, ArenaSkillTreeTileState.Start);
            tile.Padding = new Thickness(8, 4, 8, 4);
            tile.Child = new Image
            {
                Source = characterSprite,
                Width = 96,
                Height = 128,
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            return tile;
        }

        public static Ellipse CreateForkMarker(
            bool forceDecision, bool hasChosenBranch)
        {
            return new Ellipse
            {
                Width = ForkMarkerDiameter,
                Height = ForkMarkerDiameter,
                Fill = !forceDecision
                    ? FreeChoiceForkBrush
                    : hasChosenBranch
                        ? DecidedForcedForkBrush
                        : UndecidedForcedForkBrush,
                IsHitTestVisible = false
            };
        }

        public static FrameworkElement CreateForkMarkerVisual(
            bool forceDecision, bool hasChosenBranch, int inputsRemaining)
        {
            Grid marker = new Grid
            {
                Width = ForkMarkerDiameter,
                Height = ForkMarkerDiameter,
                IsHitTestVisible = false
            };
            Ellipse sphere = CreateForkMarker(
                forceDecision, hasChosenBranch);
            if (inputsRemaining > 0)
            {
                sphere.Fill = WaitingForkBrush;
            }
            marker.Children.Add(sphere);
            if (inputsRemaining > 0)
            {
                marker.Children.Add(new TextBlock
                {
                    Text = inputsRemaining.ToString(),
                    Foreground = Brushes.White,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                });
            }
            return marker;
        }

        public static void DrawConnections(
            Canvas canvas,
            ArenaSkillTreeDocument tree,
            Func<ArenaSkillTreeConnection, Brush> getBrush,
            bool forksAsMarkers = false)
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
                    node => ArenaSkillTreeRepository.GetNodeId(node)
                        == connection.FromSkillId);
                ArenaSkillTreeNode to = tree.Nodes.Find(
                    node => ArenaSkillTreeRepository.GetNodeId(node)
                        == connection.ToSkillId);
                if (from == null || to == null)
                {
                    throw new InvalidOperationException(
                        "A skill tree connection refers to a missing node.");
                }
                DrawConnection(
                    canvas, from, to, getBrush(connection), forksAsMarkers);
            }
        }

        private static void DrawConnection(
            Canvas canvas,
            ArenaSkillTreeNode from,
            ArenaSkillTreeNode to,
            Brush brush,
            bool forksAsMarkers)
        {
            bool horizontal = Math.Abs(to.X - from.X) >= Math.Abs(to.Y - from.Y);
            Point start;
            Point end;
            Point firstBend;
            Point secondBend;
            if (horizontal)
            {
                bool goesRight = to.X >= from.X;
                double directionX = goesRight ? 1 : -1;
                start = GetConnectionPoint(
                    from, directionX, 0, forksAsMarkers);
                end = GetConnectionPoint(
                    to, -directionX, 0, forksAsMarkers);
                double middleX = (start.X + end.X) / 2;
                firstBend = new Point(middleX, start.Y);
                secondBend = new Point(middleX, end.Y);
            }
            else
            {
                bool goesDown = to.Y >= from.Y;
                double directionY = goesDown ? 1 : -1;
                start = GetConnectionPoint(
                    from, 0, directionY, forksAsMarkers);
                end = GetConnectionPoint(
                    to, 0, -directionY, forksAsMarkers);
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

        private static Point GetConnectionPoint(
            ArenaSkillTreeNode node, double directionX,
            double directionY, bool forksAsMarkers)
        {
            double centerX = node.X + NodeWidth / 2;
            double centerY = node.Y + NodeHeight / 2;
            if (forksAsMarkers && node.IsFork)
            {
                return new Point(
                    centerX + directionX * ForkMarkerDiameter / 2,
                    centerY + directionY * ForkMarkerDiameter / 2);
            }

            if (Math.Abs(directionX) > 0)
            {
                return new Point(
                    centerX + directionX * NodeWidth / 2, centerY);
            }
            return new Point(
                centerX, centerY + directionY * NodeHeight / 2);
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
                case ArenaSkillTreeTileState.Locked:
                    return Color.FromRgb(235, 76, 76);
                default:
                    return Color.FromRgb(112, 132, 154);
            }
        }
    }
}
