using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public sealed class HayDayPainter : IPaintGame2
    {
        private double _canvasWidth;
        private double _canvasHeight;

        public string Name { get { return "Hay Day - Isometrischer Bauernhof"; } }

        public PointOnFarm FarmPosition(int x, int y, HayDayField field)
        {
            return field.ScreenToFarm(x, y, _canvasWidth, _canvasHeight);
        }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            HayDayField field = currentField as HayDayField;
            canvas.Children.Clear();
            canvas.Background = new LinearGradientBrush(
                Color.FromRgb(157, 213, 237), Color.FromRgb(214, 235, 197), 90);
            if (field == null)
            {
                return;
            }

            _canvasWidth = canvas.ActualWidth;
            _canvasHeight = canvas.ActualHeight;
            if (_canvasWidth <= 0 || _canvasHeight <= 0)
            {
                return;
            }

            canvas.ClipToBounds = true;
            double tileWidth = field.TileWidth(_canvasWidth, _canvasHeight);
            double tileHeight = tileWidth * 0.5;
            StreamGeometry grass = new StreamGeometry();
            StreamGeometry darkGrass = new StreamGeometry();
            StreamGeometry road = new StreamGeometry();
            StreamGeometry grid = new StreamGeometry();

            using (StreamGeometryContext grassContext = grass.Open())
            using (StreamGeometryContext darkGrassContext = darkGrass.Open())
            using (StreamGeometryContext roadContext = road.Open())
            using (StreamGeometryContext gridContext = grid.Open())
            {
                for (int row = 0; row < HayDayField.Rows; row++)
                {
                    for (int column = 0; column < HayDayField.Columns; column++)
                    {
                        Point center = ToScreen(row, column, field, tileWidth, tileHeight);
                        StreamGeometryContext target = field.IsRoad(row, column)
                            ? roadContext
                            : (row * 31 + column * 17) % 4 == 0 ? darkGrassContext : grassContext;
                        AddDiamond(target, center, tileWidth, tileHeight, true);
                        AddDiamond(gridContext, center, tileWidth, tileHeight, false);
                    }
                }
            }

            AddPath(canvas, grass, new SolidColorBrush(Color.FromRgb(113, 183, 91)), null, 0);
            AddPath(canvas, darkGrass, new SolidColorBrush(Color.FromRgb(103, 169, 82)), null, 0);
            AddPath(canvas, road, new SolidColorBrush(Color.FromRgb(209, 183, 122)), null, 0);
            AddPath(canvas, grid, new SolidColorBrush(Color.FromArgb(110, 53, 102, 50)),
                new SolidColorBrush(Color.FromArgb(110, 53, 102, 50)),
                Math.Max(0.25, tileWidth * 0.012));

            foreach (FarmPlot plot in field.Plots)
            {
                Point center = ToScreen(plot.Row, plot.Column, field, tileWidth, tileHeight);
                AddDiamondShape(canvas, center, tileWidth * 0.94, tileHeight * 0.9,
                    new SolidColorBrush(Color.FromRgb(126, 78, 42)),
                    new SolidColorBrush(Color.FromRgb(170, 120, 71)), Math.Max(0.5, tileWidth * 0.025));
                if (plot.Crop != null)
                {
                    DrawCrop(canvas, center, tileWidth, tileHeight, plot);
                }
            }

            DrawCoopPens(canvas, field, tileWidth, tileHeight);
            DrawBuildings(canvas, field, tileWidth, tileHeight);
            DrawLivestock(canvas, field, tileWidth, tileHeight);
            DrawCoopForeground(canvas, field, tileWidth, tileHeight);
            DrawHud(canvas, field);
            DrawBuildingMenu(canvas, field);
            field.IsDirty = false;
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            HayDayField field = currentField as HayDayField;
            bool resized = canvas.ActualWidth != _canvasWidth || canvas.ActualHeight != _canvasHeight;
            if (field != null && (field.IsDirty || resized))
            {
                PaintGameField(canvas, currentField);
            }
        }

        public int MenuHitTest(int x, int y, HayDayField field)
        {
            if (x >= 145 && x <= 245 && y >= 0 && y <= 34)
            {
                return HayDayField.ShopMenuOption;
            }

            if (!field.HasOpenMenu)
            {
                return -1;
            }

            GetMenuBounds(field, out double left, out double top, out double width, out double height);
            if (x < left || x > left + width || y < top || y > top + height)
            {
                return -1;
            }

            string[] info = field.GetOpenMenuInfo();
            string[] options = field.GetOpenMenuOptions();
            double optionsTop = top + 44 + info.Length * 19 + 8;
            for (int i = 0; i < options.Length; i++)
            {
                double optionTop = optionsTop + i * 29;
                if (y >= optionTop && y <= optionTop + 25)
                {
                    return i;
                }
            }

            return -2;
        }

        private void DrawBuildings(Canvas canvas, HayDayField field, double tileWidth, double tileHeight)
        {
            foreach (FarmBuilding building in field.Buildings.OrderBy(building => building.Row + building.Column))
            {
                Point center = ToScreen(building.Row, building.Column, field, tileWidth, tileHeight);
                DrawHouse(canvas, center, tileWidth, tileHeight, building);
            }
        }

        private static void DrawHouse(Canvas canvas, Point ground, double tileWidth,
            double tileHeight, FarmBuilding building)
        {
            double width = Math.Max(12, tileWidth * 0.84);
            double wallHeight = Math.Max(9, tileHeight * 0.9);
            double roofHeight = Math.Max(6, tileHeight * 0.58);
            double left = ground.X - width * 0.46;
            double frontRight = ground.X + width * 0.25;
            double sideRight = ground.X + width * 0.46;
            double baseY = ground.Y + tileHeight * 0.28;
            double wallTop = baseY - wallHeight;
            double peakX = ground.X - width * 0.08;
            double peakY = wallTop - roofHeight;
            HousePalette palette = PaletteFor(building.Kind);

            Ellipse shadow = new Ellipse
            {
                Width = width * 1.15,
                Height = tileHeight * 0.34,
                Fill = new SolidColorBrush(Color.FromArgb(65, 34, 55, 27)),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(shadow, ground.X - shadow.Width / 2);
            Canvas.SetTop(shadow, baseY - shadow.Height * 0.45);
            canvas.Children.Add(shadow);

            AddPolygon(canvas, new[]
            {
                new Point(left, wallTop), new Point(frontRight, wallTop),
                new Point(frontRight, baseY), new Point(left, baseY)
            }, palette.Front, palette.Outline, 1);
            AddPolygon(canvas, new[]
            {
                new Point(frontRight, wallTop), new Point(sideRight, wallTop + tileHeight * 0.12),
                new Point(sideRight, baseY + tileHeight * 0.08), new Point(frontRight, baseY)
            }, palette.Side, palette.Outline, 1);

            AddPolygon(canvas, new[]
            {
                new Point(left - width * 0.06, wallTop + 1),
                new Point(peakX, peakY),
                new Point(frontRight + width * 0.05, wallTop + 1),
                new Point(frontRight, wallTop + roofHeight * 0.25),
                new Point(left, wallTop + roofHeight * 0.25)
            }, palette.RoofFront, palette.Outline, 1);
            AddPolygon(canvas, new[]
            {
                new Point(peakX, peakY),
                new Point(sideRight + width * 0.08, wallTop + roofHeight * 0.3),
                new Point(sideRight, wallTop + roofHeight * 0.48),
                new Point(frontRight, wallTop + roofHeight * 0.25)
            }, palette.RoofSide, palette.Outline, 1);

            double windowSize = Math.Max(3, tileWidth * 0.16);
            DrawWindow(canvas, left + width * 0.11, wallTop + wallHeight * 0.34, windowSize);
            DrawWindow(canvas, frontRight - width * 0.21, wallTop + wallHeight * 0.34, windowSize);
            DrawDoor(canvas, ground.X - width * 0.04, baseY - wallHeight * 0.53,
                Math.Max(3, tileWidth * 0.2), wallHeight * 0.53, palette.Door);

            double signWidth = width * 0.78;
            double signHeight = Math.Max(5, tileHeight * 0.28);
            Rectangle sign = new Rectangle
            {
                Width = signWidth,
                Height = signHeight,
                Fill = palette.Sign,
                Stroke = palette.Outline,
                StrokeThickness = 0.7,
                RadiusX = 2,
                RadiusY = 2,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(sign, ground.X - signWidth / 2);
            Canvas.SetTop(sign, wallTop + wallHeight * 0.08);
            canvas.Children.Add(sign);
            AddText(canvas, building.Name, ground.X - signWidth / 2 + 2,
                wallTop + wallHeight * 0.08 + 1,                 Math.Max(4, tileWidth * 0.14),
                palette.SignText, true, signWidth - 4);

            if (building.Kind == "Market")
            {
                DrawAwning(canvas, ground.X - width * 0.42, wallTop + wallHeight * 0.63,
                    width * 0.28, Math.Max(3, tileHeight * 0.25));
            }
            else if (building.Kind == "Van")
            {
                DrawAwning(canvas, ground.X + width * 0.17, baseY - wallHeight * 0.28,
                    width * 0.26, Math.Max(4, tileHeight * 0.3));
            }
        }

        private static void DrawWindow(Canvas canvas, double left, double top, double size)
        {
            Rectangle window = new Rectangle
            {
                Width = size,
                Height = size * 0.82,
                Fill = new SolidColorBrush(Color.FromRgb(182, 224, 232)),
                Stroke = new SolidColorBrush(Color.FromRgb(104, 72, 49)),
                StrokeThickness = 1,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(window, left);
            Canvas.SetTop(window, top);
            canvas.Children.Add(window);
            AddLine(canvas, new Point(left + size / 2, top), new Point(left + size / 2, top + size * 0.82),
                Brushes.WhiteSmoke, 0.8);
            AddLine(canvas, new Point(left, top + size * 0.41), new Point(left + size, top + size * 0.41),
                Brushes.WhiteSmoke, 0.8);
        }

        private static void DrawDoor(Canvas canvas, double left, double top, double width,
            double height, Brush doorBrush)
        {
            Rectangle door = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = doorBrush,
                Stroke = new SolidColorBrush(Color.FromRgb(82, 48, 34)),
                StrokeThickness = 1,
                RadiusX = 2,
                RadiusY = 2,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(door, left);
            Canvas.SetTop(door, top);
            canvas.Children.Add(door);
            Ellipse knob = new Ellipse
            {
                Width = Math.Max(1.5, width * 0.09),
                Height = Math.Max(1.5, width * 0.09),
                Fill = Brushes.Gold,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(knob, left + width * 0.76);
            Canvas.SetTop(knob, top + height * 0.56);
            canvas.Children.Add(knob);
        }

        private static void DrawAwning(Canvas canvas, double left, double top,
            double width, double height)
        {
            AddPolygon(canvas, new[]
            {
                new Point(left, top), new Point(left + width, top),
                new Point(left + width * 0.88, top + height), new Point(left + width * 0.12, top + height)
            }, new SolidColorBrush(Color.FromRgb(214, 82, 55)), Brushes.WhiteSmoke, 1);
            for (int i = 1; i < 4; i++)
            {
                double x = left + width * i / 4;
                AddLine(canvas, new Point(x, top + 1), new Point(left + width * (i * 0.88 + 0.12) / 4,
                    top + height), Brushes.WhiteSmoke, 1);
            }
        }

        private static void DrawCrop(Canvas canvas, Point center, double tileWidth,
            double tileHeight, FarmPlot plot)
        {
            bool wheat = plot.Crop == HayDayField.Wheat;
            Brush stem = wheat ? new SolidColorBrush(Color.FromRgb(213, 174, 64))
                : new SolidColorBrush(Color.FromRgb(42, 133, 57));
            Brush leaf = wheat ? new SolidColorBrush(Color.FromRgb(244, 208, 91))
                : new SolidColorBrush(Color.FromRgb(71, 173, 68));
            double spread = tileWidth * 0.24;
            double stemHeight = tileHeight * 0.7;
            for (int i = -1; i <= 1; i++)
            {
                double x = center.X + i * spread * 0.55;
                AddLine(canvas, new Point(x, center.Y + tileHeight * 0.1),
                    new Point(x + i * spread * 0.25, center.Y - stemHeight * 0.45),
                    stem, Math.Max(1, tileWidth * 0.035));
                Ellipse cropHead = new Ellipse
                {
                    Width = Math.Max(3, tileWidth * 0.12),
                    Height = Math.Max(3, tileWidth * 0.12),
                    Fill = plot.Ripe ? Brushes.Gold : leaf,
                    Stroke = stem,
                    StrokeThickness = 0.6,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(cropHead, x - cropHead.Width / 2);
                Canvas.SetTop(cropHead, center.Y - stemHeight * 0.52);
                canvas.Children.Add(cropHead);
            }
        }

        private void DrawCoopPens(Canvas canvas, HayDayField field, double tileWidth, double tileHeight)
        {
            foreach (FarmBuilding building in field.Buildings.Where(value => value.Kind == "Coop"))
            {
                Point center = ToScreen(building.Row, building.Column, field, tileWidth, tileHeight);
                Point top = new Point(center.X, center.Y - tileHeight * 1.6);
                Point left = new Point(center.X - tileWidth * 1.6, center.Y);
                Point right = new Point(center.X + tileWidth * 1.6, center.Y);
                Point bottom = new Point(center.X, center.Y + tileHeight * 1.6);

                AddPolygon(canvas, new[] { top, right, bottom, left },
                    new SolidColorBrush(Color.FromArgb(95, 145, 190, 94)),
                    Brushes.Transparent, 0);
                DrawFenceSegment(canvas, left, top, tileWidth, tileHeight);
                DrawFenceSegment(canvas, top, right, tileWidth, tileHeight);
            }
        }

        private void DrawCoopForeground(Canvas canvas, HayDayField field,
            double tileWidth, double tileHeight)
        {
            double[][] chickenOffsets =
            {
                new[] { -1.0, 0.35 },
                new[] { -0.5, 0.75 },
                new[] { 0.0, 0.35 },
                new[] { 0.5, 0.75 },
                new[] { 1.0, 0.35 }
            };

            foreach (FarmBuilding building in field.Buildings.Where(value => value.Kind == "Coop"))
            {
                Point center = ToScreen(building.Row, building.Column, field, tileWidth, tileHeight);
                foreach (double[] offset in chickenOffsets)
                {
                    DrawChicken(canvas,
                        new Point(center.X + offset[0] * tileWidth,
                            center.Y + offset[1] * tileHeight),
                        tileWidth);
                }

                Point left = new Point(center.X - tileWidth * 1.6, center.Y);
                Point right = new Point(center.X + tileWidth * 1.6, center.Y);
                Point bottom = new Point(center.X, center.Y + tileHeight * 1.6);
                DrawFenceSegment(canvas, left, bottom, tileWidth, tileHeight);
                DrawFenceSegment(canvas, bottom, right, tileWidth, tileHeight);
            }
        }

        private void DrawLivestock(Canvas canvas, HayDayField field,
            double tileWidth, double tileHeight)
        {
            foreach (FarmBuilding building in field.Buildings.Where(value =>
                value.Kind == "CowBarn" || value.Kind == "SheepBarn"))
            {
                Point center = ToScreen(building.Row, building.Column, field, tileWidth, tileHeight);
                bool sheep = building.Kind == "SheepBarn";
                for (int i = 0; i < 2; i++)
                {
                    DrawFarmAnimal(canvas,
                        new Point(center.X + (i == 0 ? -0.55 : 0.55) * tileWidth,
                            center.Y + (0.55 + i * 0.25) * tileHeight),
                        tileWidth, sheep);
                }
            }
        }

        private static void DrawFarmAnimal(Canvas canvas, Point ground, double tileWidth, bool sheep)
        {
            Brush coat = sheep
                ? new SolidColorBrush(Color.FromRgb(250, 244, 224))
                : new SolidColorBrush(Color.FromRgb(240, 235, 218));
            Brush outline = new SolidColorBrush(Color.FromRgb(95, 78, 60));
            double bodyWidth = Math.Max(8, tileWidth * 0.42);
            double bodyHeight = Math.Max(6, tileWidth * 0.23);
            Ellipse body = new Ellipse
            {
                Width = bodyWidth,
                Height = bodyHeight,
                Fill = coat,
                Stroke = outline,
                StrokeThickness = 0.8,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(body, ground.X - bodyWidth / 2);
            Canvas.SetTop(body, ground.Y - bodyHeight);
            canvas.Children.Add(body);

            if (sheep)
            {
                for (int i = -1; i <= 1; i++)
                {
                    Ellipse wool = new Ellipse
                    {
                        Width = bodyHeight * 0.75,
                        Height = bodyHeight * 0.75,
                        Fill = Brushes.WhiteSmoke,
                        IsHitTestVisible = false
                    };
                    Canvas.SetLeft(wool, ground.X + i * bodyWidth * 0.22 - wool.Width / 2);
                    Canvas.SetTop(wool, ground.Y - bodyHeight * 1.12);
                    canvas.Children.Add(wool);
                }
            }
            else
            {
                Ellipse patch = new Ellipse
                {
                    Width = bodyWidth * 0.23,
                    Height = bodyHeight * 0.45,
                    Fill = new SolidColorBrush(Color.FromRgb(100, 89, 77)),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(patch, ground.X - bodyWidth * 0.15);
                Canvas.SetTop(patch, ground.Y - bodyHeight * 0.75);
                canvas.Children.Add(patch);
            }

            Ellipse head = new Ellipse
            {
                Width = Math.Max(4, bodyHeight * 0.8),
                Height = Math.Max(4, bodyHeight * 0.8),
                Fill = coat,
                Stroke = outline,
                StrokeThickness = 0.7,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(head, ground.X + bodyWidth * 0.32);
            Canvas.SetTop(head, ground.Y - bodyHeight * 1.15);
            canvas.Children.Add(head);

            for (int i = -1; i <= 1; i += 2)
            {
                AddLine(canvas,
                    new Point(ground.X + i * bodyWidth * 0.28, ground.Y - bodyHeight * 0.28),
                    new Point(ground.X + i * bodyWidth * 0.3, ground.Y),
                    outline, Math.Max(1, tileWidth * 0.025));
            }
        }

        private static void DrawFenceSegment(Canvas canvas, Point start, Point end,
            double tileWidth, double tileHeight)
        {
            Brush fence = new SolidColorBrush(Color.FromRgb(112, 77, 45));
            Brush rail = new SolidColorBrush(Color.FromRgb(171, 126, 76));
            double postHeight = Math.Max(5, tileHeight * 0.48);
            int postCount = 5;

            for (int i = 0; i < postCount; i++)
            {
                double fraction = i / (double)(postCount - 1);
                double x = start.X + (end.X - start.X) * fraction;
                double y = start.Y + (end.Y - start.Y) * fraction;
                AddLine(canvas, new Point(x, y), new Point(x, y - postHeight), fence,
                    Math.Max(1, tileWidth * 0.035));
            }

            for (int i = 0; i < postCount - 1; i++)
            {
                double startFraction = i / (double)(postCount - 1);
                double endFraction = (i + 1) / (double)(postCount - 1);
                Point railStart = new Point(
                    start.X + (end.X - start.X) * startFraction,
                    start.Y + (end.Y - start.Y) * startFraction);
                Point railEnd = new Point(
                    start.X + (end.X - start.X) * endFraction,
                    start.Y + (end.Y - start.Y) * endFraction);
                foreach (double heightFraction in new[] { 0.25, 0.7 })
                {
                    AddLine(canvas,
                        new Point(railStart.X, railStart.Y - postHeight * heightFraction),
                        new Point(railEnd.X, railEnd.Y - postHeight * heightFraction),
                        rail, Math.Max(1, tileWidth * 0.045));
                }
            }
        }

        private static void DrawChicken(Canvas canvas, Point ground, double tileWidth)
        {
            double bodyWidth = Math.Max(6, tileWidth * 0.25);
            double bodyHeight = Math.Max(5, tileWidth * 0.17);
            double headSize = Math.Max(3, tileWidth * 0.12);
            Brush feathers = new SolidColorBrush(Color.FromRgb(255, 247, 218));
            Brush outline = new SolidColorBrush(Color.FromRgb(125, 91, 56));

            Ellipse body = new Ellipse
            {
                Width = bodyWidth,
                Height = bodyHeight,
                Fill = feathers,
                Stroke = outline,
                StrokeThickness = 0.8,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(body, ground.X - bodyWidth / 2);
            Canvas.SetTop(body, ground.Y - bodyHeight);
            canvas.Children.Add(body);

            Ellipse wing = new Ellipse
            {
                Width = bodyWidth * 0.48,
                Height = bodyHeight * 0.62,
                Fill = new SolidColorBrush(Color.FromRgb(239, 222, 183)),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(wing, ground.X - bodyWidth * 0.18);
            Canvas.SetTop(wing, ground.Y - bodyHeight * 0.82);
            canvas.Children.Add(wing);

            Ellipse head = new Ellipse
            {
                Width = headSize,
                Height = headSize,
                Fill = feathers,
                Stroke = outline,
                StrokeThickness = 0.7,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(head, ground.X + bodyWidth * 0.22);
            Canvas.SetTop(head, ground.Y - bodyHeight - headSize * 0.62);
            canvas.Children.Add(head);

            AddPolygon(canvas, new[]
            {
                new Point(ground.X + bodyWidth * 0.22 + headSize * 0.82,
                    ground.Y - bodyHeight - headSize * 0.24),
                new Point(ground.X + bodyWidth * 0.22 + headSize * 1.2,
                    ground.Y - bodyHeight - headSize * 0.12),
                new Point(ground.X + bodyWidth * 0.22 + headSize * 0.82,
                    ground.Y - bodyHeight)
            }, new SolidColorBrush(Color.FromRgb(224, 139, 43)), Brushes.Transparent, 0);

            AddLine(canvas, new Point(ground.X - bodyWidth * 0.18, ground.Y - 1),
                new Point(ground.X - bodyWidth * 0.22, ground.Y + tileWidth * 0.035),
                new SolidColorBrush(Color.FromRgb(194, 117, 41)), Math.Max(0.8, tileWidth * 0.025));
            AddLine(canvas, new Point(ground.X + bodyWidth * 0.12, ground.Y - 1),
                new Point(ground.X + bodyWidth * 0.16, ground.Y + tileWidth * 0.035),
                new SolidColorBrush(Color.FromRgb(194, 117, 41)), Math.Max(0.8, tileWidth * 0.025));
        }

        private static void DrawHud(Canvas canvas, HayDayField field)
        {
            Rectangle background = new Rectangle
            {
                Width = 136,
                Height = 34,
                Fill = new SolidColorBrush(Color.FromArgb(235, 255, 250, 226)),
                Stroke = new SolidColorBrush(Color.FromRgb(153, 117, 65)),
                StrokeThickness = 1,
                RadiusX = 7,
                RadiusY = 7,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(background, 0);
            Canvas.SetTop(background, 0);
            canvas.Children.Add(background);

            Ellipse coin = new Ellipse
            {
                Width = 20,
                Height = 20,
                Fill = new SolidColorBrush(Color.FromRgb(244, 190, 53)),
                Stroke = new SolidColorBrush(Color.FromRgb(153, 105, 35)),
                StrokeThickness = 1,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(coin, 7);
            Canvas.SetTop(coin, 7);
            canvas.Children.Add(coin);
            AddText(canvas, "Münzen: " + field.Coins, 33, 8, 13,
                Brushes.DarkSlateGray, true, 98);

            Rectangle shopButton = new Rectangle
            {
                Width = 100,
                Height = 34,
                Fill = new SolidColorBrush(Color.FromRgb(222, 190, 125)),
                Stroke = new SolidColorBrush(Color.FromRgb(145, 100, 54)),
                StrokeThickness = 1,
                RadiusX = 7,
                RadiusY = 7,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(shopButton, 145);
            Canvas.SetTop(shopButton, 0);
            canvas.Children.Add(shopButton);
            AddText(canvas, "Shop", 145, 8, 13, Brushes.SaddleBrown, true, 100);
        }

        private void DrawBuildingMenu(Canvas canvas, HayDayField field)
        {
            if (!field.HasOpenMenu)
            {
                return;
            }

            GetMenuBounds(field, out double left, out double top, out double width, out double height);
            Rectangle panel = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = new SolidColorBrush(Color.FromArgb(245, 255, 249, 225)),
                Stroke = new SolidColorBrush(Color.FromRgb(117, 78, 42)),
                StrokeThickness = 2,
                RadiusX = 8,
                RadiusY = 8,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(panel, left);
            Canvas.SetTop(panel, top);
            canvas.Children.Add(panel);

            AddText(canvas, field.GetOpenMenuTitle(), left + 10, top + 8, 16,
                Brushes.SaddleBrown, true, width - 20);
            string[] info = field.GetOpenMenuInfo();
            for (int i = 0; i < info.Length; i++)
            {
                AddText(canvas, info[i], left + 11, top + 37 + i * 19, 11,
                    Brushes.DarkSlateGray, false, width - 22);
            }

            string[] options = field.GetOpenMenuOptions();
            double optionsTop = top + 44 + info.Length * 19 + 8;
            for (int i = 0; i < options.Length; i++)
            {
                double optionTop = optionsTop + i * 29;
                Rectangle button = new Rectangle
                {
                    Width = width - 16,
                    Height = 25,
                    Fill = new SolidColorBrush(Color.FromRgb(222, 190, 125)),
                    Stroke = new SolidColorBrush(Color.FromRgb(145, 100, 54)),
                    StrokeThickness = 1,
                    RadiusX = 4,
                    RadiusY = 4,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(button, left + 8);
                Canvas.SetTop(button, optionTop);
                canvas.Children.Add(button);
                AddText(canvas, options[i], left + 14, optionTop + 4, 11,
                    Brushes.SaddleBrown, true, width - 28);
            }
        }

        private void GetMenuBounds(HayDayField field, out double left, out double top,
            out double width, out double height)
        {
            width = Math.Min(260, Math.Max(140, _canvasWidth - 20));
            left = Math.Max(8, _canvasWidth - width - 10);
            top = 78;
            height = 44 + field.GetOpenMenuInfo().Length * 19
                + 8 + field.GetOpenMenuOptions().Length * 29 + 10;
        }

        private static HousePalette PaletteFor(string kind)
        {
            switch (kind)
            {
                case "Farmhouse":
                    return new HousePalette(Color.FromRgb(242, 211, 157), Color.FromRgb(205, 164, 112),
                        Color.FromRgb(177, 69, 53), Color.FromRgb(137, 47, 42), Color.FromRgb(110, 67, 45),
                        Color.FromRgb(112, 69, 43), Color.FromRgb(255, 236, 188), Color.FromRgb(91, 56, 38));
                case "Coop":
                    return new HousePalette(Color.FromRgb(241, 220, 169), Color.FromRgb(210, 178, 120),
                        Color.FromRgb(177, 102, 54), Color.FromRgb(132, 72, 40), Color.FromRgb(103, 67, 43),
                        Color.FromRgb(118, 79, 45), Color.FromRgb(255, 239, 191), Color.FromRgb(86, 59, 39));
                case "CowBarn":
                    return new HousePalette(Color.FromRgb(237, 228, 199), Color.FromRgb(199, 186, 152),
                        Color.FromRgb(119, 88, 65), Color.FromRgb(84, 62, 51), Color.FromRgb(107, 73, 52),
                        Color.FromRgb(125, 91, 61), Color.FromRgb(255, 244, 211), Color.FromRgb(82, 61, 43));
                case "SheepBarn":
                    return new HousePalette(Color.FromRgb(232, 222, 194), Color.FromRgb(198, 185, 157),
                        Color.FromRgb(133, 111, 81), Color.FromRgb(94, 78, 64), Color.FromRgb(107, 73, 52),
                        Color.FromRgb(125, 91, 61), Color.FromRgb(255, 244, 211), Color.FromRgb(82, 61, 43));
                case "Dairy":
                    return new HousePalette(Color.FromRgb(231, 238, 215), Color.FromRgb(187, 207, 165),
                        Color.FromRgb(105, 154, 117), Color.FromRgb(67, 117, 84), Color.FromRgb(107, 73, 52),
                        Color.FromRgb(124, 151, 101), Color.FromRgb(255, 244, 211), Color.FromRgb(82, 61, 43));
                case "Barn":
                    return new HousePalette(Color.FromRgb(203, 91, 67), Color.FromRgb(165, 64, 53),
                        Color.FromRgb(123, 51, 49), Color.FromRgb(91, 41, 46), Color.FromRgb(95, 55, 43),
                        Color.FromRgb(109, 57, 43), Color.FromRgb(255, 222, 169), Color.FromRgb(86, 50, 37));
                case "Silo":
                    return new HousePalette(Color.FromRgb(218, 201, 145), Color.FromRgb(177, 157, 105),
                        Color.FromRgb(91, 139, 103), Color.FromRgb(60, 106, 81), Color.FromRgb(94, 75, 46),
                        Color.FromRgb(112, 89, 49), Color.FromRgb(255, 238, 181), Color.FromRgb(80, 68, 43));
                case "Mill":
                    return new HousePalette(Color.FromRgb(226, 190, 133), Color.FromRgb(190, 148, 101),
                        Color.FromRgb(103, 133, 150), Color.FromRgb(68, 97, 124), Color.FromRgb(99, 69, 45),
                        Color.FromRgb(117, 80, 48), Color.FromRgb(255, 234, 184), Color.FromRgb(83, 59, 40));
                case "Market":
                    return new HousePalette(Color.FromRgb(244, 215, 161), Color.FromRgb(211, 175, 117),
                        Color.FromRgb(198, 104, 58), Color.FromRgb(150, 70, 49), Color.FromRgb(101, 69, 47),
                        Color.FromRgb(129, 83, 47), Color.FromRgb(255, 243, 198), Color.FromRgb(88, 59, 39));
                default:
                    return new HousePalette(Color.FromRgb(196, 209, 217), Color.FromRgb(148, 174, 190),
                        Color.FromRgb(82, 127, 160), Color.FromRgb(57, 91, 127), Color.FromRgb(71, 76, 80),
                        Color.FromRgb(105, 75, 50), Color.FromRgb(255, 240, 195), Color.FromRgb(76, 57, 42));
            }
        }

        private static void AddPolygon(Canvas canvas, Point[] points, Brush fill, Brush stroke,
            double strokeThickness)
        {
            Polygon polygon = new Polygon
            {
                Points = new PointCollection(points),
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeThickness,
                StrokeLineJoin = PenLineJoin.Round,
                IsHitTestVisible = false
            };
            canvas.Children.Add(polygon);
        }

        private static void AddLine(Canvas canvas, Point start, Point end, Brush brush, double thickness)
        {
            Line line = new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = end.X,
                Y2 = end.Y,
                Stroke = brush,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false
            };
            canvas.Children.Add(line);
        }

        private sealed class HousePalette
        {
            public HousePalette(Color front, Color side, Color roofFront, Color roofSide,
                Color door, Color sign, Color signText, Color outline)
            {
                Front = new SolidColorBrush(front);
                Side = new SolidColorBrush(side);
                RoofFront = new SolidColorBrush(roofFront);
                RoofSide = new SolidColorBrush(roofSide);
                Door = new SolidColorBrush(door);
                Sign = new SolidColorBrush(sign);
                SignText = new SolidColorBrush(signText);
                Outline = new SolidColorBrush(outline);
            }

            public Brush Front { get; private set; }
            public Brush Side { get; private set; }
            public Brush RoofFront { get; private set; }
            public Brush RoofSide { get; private set; }
            public Brush Door { get; private set; }
            public Brush Sign { get; private set; }
            public Brush SignText { get; private set; }
            public Brush Outline { get; private set; }
        }

        private static void AddDiamond(StreamGeometryContext context, Point center,
            double width, double height, bool filled)
        {
            Point top = new Point(center.X, center.Y - height / 2);
            context.BeginFigure(top, filled, true);
            context.PolyLineTo(new[]
            {
                new Point(center.X + width / 2, center.Y),
                new Point(center.X, center.Y + height / 2),
                new Point(center.X - width / 2, center.Y)
            }, true, true);
        }

        private static void AddDiamondShape(Canvas canvas, Point center, double width, double height,
            Brush fill, Brush stroke, double strokeThickness)
        {
            Polygon diamond = new Polygon
            {
                Points = new PointCollection
                {
                    new Point(center.X, center.Y - height / 2),
                    new Point(center.X + width / 2, center.Y),
                    new Point(center.X, center.Y + height / 2),
                    new Point(center.X - width / 2, center.Y)
                },
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeThickness
            };
            canvas.Children.Add(diamond);
        }

        private static void AddPath(Canvas canvas, Geometry geometry, Brush fill, Brush stroke,
            double strokeThickness)
        {
            Path path = new Path
            {
                Data = geometry,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeThickness,
                IsHitTestVisible = false
            };
            canvas.Children.Add(path);
        }

        private static void AddText(Canvas canvas, string text, double left, double top,
            double fontSize, Brush foreground, bool bold, double width = double.NaN)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                Foreground = foreground,
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                Width = width,
                TextTrimming = TextTrimming.CharacterEllipsis,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            canvas.Children.Add(label);
        }

        private Point ToScreen(int row, int column, HayDayField field,
            double tileWidth, double tileHeight)
        {
            double columnOffset = column - field.CameraColumn;
            double rowOffset = row - field.CameraRow;
            return new Point(
                _canvasWidth / 2 + (columnOffset - rowOffset) * tileWidth / 2,
                _canvasHeight / 2 + (columnOffset + rowOffset) * tileHeight / 2);
        }
    }
}
