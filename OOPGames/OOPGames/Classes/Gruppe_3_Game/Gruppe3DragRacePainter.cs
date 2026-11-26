using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OOPGames
{
    public sealed class Gruppe3DragRacePainter : IGruppe3DragRacePainter
    {
        private static readonly FontFamily GameFont = LoadGameFont();
        private static readonly BitmapImage[] _carImages =
            new BitmapImage[Gruppe3DragRaceCars.All.Count];
        private static readonly BitmapImage[] _dashboardImages =
            new BitmapImage[Gruppe3DragRaceCars.All.Count];
        private static readonly BitmapImage[] _rpmNeedleImages =
            new BitmapImage[Gruppe3DragRaceCars.All.Count];
        private static readonly BitmapImage[] _speedNeedleImages =
            new BitmapImage[Gruppe3DragRaceCars.All.Count];
        private static readonly BitmapImage[] _turboNeedleImages =
            new BitmapImage[Gruppe3DragRaceCars.All.Count];
        private double _selectionCardWidth = 220;
        private double _selectionCardHeight = 120;
        private double _selectionGap = 15;
        private double _selectionLeft = 20;
        private double _selectionTop = 70;

        public Gruppe3DragRacePainter()
        {
            string basePath = FindAssetDirectory();
            if (basePath == null)
            {
                return;
            }

            foreach (Cars car in Gruppe3DragRaceCars.All)
            {
                _carImages[car.Index] = LoadBitmap(
                    System.IO.Path.Combine(
                        basePath, car.FolderName, car.CarFileName));
                _dashboardImages[car.Index] = LoadBitmap(
                    System.IO.Path.Combine(
                        basePath, car.FolderName, car.DashboardFileName));
                if (car.RpmNeedleFileName != null)
                {
                    _rpmNeedleImages[car.Index] = LoadBitmap(
                        System.IO.Path.Combine(
                            basePath, car.FolderName, car.RpmNeedleFileName));
                }
                _speedNeedleImages[car.Index] = LoadBitmap(
                    System.IO.Path.Combine(
                        basePath, car.FolderName, car.SpeedNeedleFileName));
                if (car.TurboNeedleFileName != null)
                {
                    _turboNeedleImages[car.Index] = LoadBitmap(
                        System.IO.Path.Combine(
                            basePath, car.FolderName, car.TurboNeedleFileName));
                }
            }
        }

        public string Name { get { return "Gruppe 3 Retro Drag Race Darstellung"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintDragRaceField(canvas, currentField as IGruppe3DragRaceField);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }

        public void PaintDragRaceField(Canvas canvas, IGruppe3DragRaceField field)
        {
            if (canvas == null || field == null)
            {
                return;
            }

            canvas.Children.Clear();
            canvas.Background = new SolidColorBrush(Color.FromRgb(25, 28, 38));
            double width = canvas.ActualWidth > 10 ? canvas.ActualWidth : 800;
            if (!field.CarSelectionComplete)
            {
                PaintCarSelection(canvas, field, width);
                return;
            }

            double height = canvas.ActualHeight > 10 ? canvas.ActualHeight : 300;
            double scale = Math.Max(0.75, Math.Min(2.2,
                Math.Min(width / 700, height / 260)));
            double laneHeight = 110 * scale;
            double trackTop = Math.Max(0,
                (height - laneHeight * 2 - 40 * scale) / 2);
            double cameraPosition = GetCameraPosition(field, width, scale);
            DrawTrack(canvas, width, laneHeight, trackTop, cameraPosition, scale);
            AddCar(
                canvas, field.PlayerOneDistance, width,
                trackTop + 10 * scale, field.PlayerOneCar, cameraPosition, scale);
            AddCar(
                canvas, field.PlayerTwoDistance, width,
                trackTop + laneHeight + 10 * scale,
                field.PlayerTwoCar, cameraPosition, scale);

            bool showSelectionFeedback =
                DateTime.UtcNow < field.SelectionFeedbackUntil;
            if (!field.IsRaceStarted || field.Winner != 0 ||
                showSelectionFeedback)
            {
                TextBlock status = new TextBlock
                {
                    Text = showSelectionFeedback
                        ? field.SelectionFeedbackText
                        : field.CountdownText,
                    Foreground = Brushes.Black,
                    FontSize = 32 * scale,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Width = width,
                    FontFamily = GameFont
                };
                AddText(
                    canvas, status, 0,
                    Math.Max(5, trackTop - 65 * scale));
            }

            if (field.IsRaceStarted || field.Winner != 0)
            {
                PaintRaceProgress(canvas, field, width, height, scale);
            }
            if (field.CarSelectionComplete)
            {
                PaintSpeedometers(canvas, field, width, height, scale);
            }
            if (field.Winner != 0)
            {
                PaintRaceResults(canvas, field, width, height, scale);
            }
        }

        public int CarAt(int x, int y)
        {
            int column = (int)((x - _selectionLeft) /
                (_selectionCardWidth + _selectionGap));
            int row = (int)((y - _selectionTop) /
                (_selectionCardHeight + _selectionGap));
            double localX = x - _selectionLeft -
                column * (_selectionCardWidth + _selectionGap);
            double localY = y - _selectionTop -
                row * (_selectionCardHeight + _selectionGap);
            if (column < 0 || column >= Gruppe3DragRaceCars.All.Count ||
                row != 0 ||
                localX > _selectionCardWidth || localY > _selectionCardHeight)
            {
                return -1;
            }

            return row * 3 + column;
        }

        private void PaintCarSelection(
            Canvas canvas, IGruppe3DragRaceField field, double width)
        {
            double height = canvas.ActualHeight > 10 ? canvas.ActualHeight : 450;
            double scale = Math.Max(0.45, Math.Min(2.2,
                Math.Min(
                    (width - 20) / (3 * 220 + 2 * 15),
                    (height - 80) / 120)));
            _selectionCardWidth = 220 * scale;
            _selectionCardHeight = 120 * scale;
            _selectionGap = 15 * scale;
            _selectionLeft = (width -
                (3 * _selectionCardWidth + 2 * _selectionGap)) / 2;
            _selectionTop = Math.Max(55, (height - _selectionCardHeight) / 2);

            bool showSelectionFeedback =
                !string.IsNullOrEmpty(field.SelectionFeedbackText) &&
                DateTime.UtcNow < field.SelectionFeedbackUntil;
            TextBlock title = new TextBlock
            {
                Text = showSelectionFeedback
                    ? field.SelectionFeedbackText
                    : field.CountdownText,
                Foreground = Brushes.Black,
                FontSize = 30 * scale,
                FontWeight = FontWeights.Bold,
                FontFamily = GameFont
            };
            AddText(
                canvas, title, _selectionLeft,
                Math.Max(5, _selectionTop - 60 * scale));

            for (int carIndex = 0;
                carIndex < Gruppe3DragRaceCars.All.Count;
                carIndex++)
            {
                int column = carIndex % 3;
                int row = 0;
                double left = _selectionLeft +
                    column * (_selectionCardWidth + _selectionGap);
                double top = _selectionTop +
                    row * (_selectionCardHeight + _selectionGap);
                bool selectedByBoth = field.PlayerOneCar == carIndex &&
                    field.PlayerTwoCar == carIndex;
                Rectangle card = new Rectangle
                {
                    Width = _selectionCardWidth,
                    Height = _selectionCardHeight,
                    Fill = new SolidColorBrush(Color.FromRgb(55, 58, 68)),
                    Stroke = selectedByBoth
                        ? Brushes.Transparent
                        : IsSelected(field, carIndex)
                        ? GetSelectionBrush(field, carIndex)
                        : Brushes.White,
                    StrokeThickness = selectedByBoth
                        ? 0
                        : IsSelected(field, carIndex)
                        ? 5 * scale
                        : 1
                };
                Canvas.SetLeft(card, left);
                Canvas.SetTop(card, top);
                canvas.Children.Add(card);
                if (selectedByBoth)
                {
                    AddSharedSelectionBorder(
                        canvas, left, top, _selectionCardWidth,
                        _selectionCardHeight, 5 * scale);
                }
                AddCarImage(
                    canvas, carIndex, left + 0.19 * _selectionCardWidth,
                    top + 0.1 * _selectionCardHeight,
                    0.62 * _selectionCardWidth,
                    0.8 * _selectionCardHeight);
            }

            PaintControls(canvas, width, height, scale);
        }

        private static void PaintControls(
            Canvas canvas, double width, double height, double scale)
        {
            double textWidth = Math.Max(120, width - 20 * scale);
            double fontSize = Math.Max(8, Math.Min(
                11 * scale, textWidth / 50));
            TextBlock controls = new TextBlock
            {
                Text = "PLAYER 1:  W = GAS    S = BREMSE       " +
                    "PLAYER 2:  NUMPAD 8 / ↑ = GAS    NUMPAD 5 / ↓ = BREMSE\n" +
                    "SCHALTEN:  P1  Q = HOCH  E = RUNTER       " +
                    "P2  NUMPAD 7 / Bild↑ = HOCH  NUMPAD 9 / Bild↓ = RUNTER",
                Foreground = Brushes.Black,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Width = textWidth,
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = Math.Max(30, 42 * scale),
                FontFamily = GameFont
            };
            AddText(
                canvas, controls, (width - textWidth) / 2,
                Math.Max(0, height - 45 * scale));
        }

        private static void PaintRaceResults(
            Canvas canvas,
            IGruppe3DragRaceField field,
            double width,
            double height,
            double scale)
        {
            double panelWidth = Math.Min(width - 24 * scale, 620 * scale);
            double panelHeight = 170 * scale;
            double left = (width - panelWidth) / 2;
            double top = Math.Max(8 * scale, (height - panelHeight) / 2);
            Rectangle panel = new Rectangle
            {
                Width = panelWidth,
                Height = panelHeight,
                Fill = new SolidColorBrush(Color.FromArgb(238, 15, 17, 24)),
                Stroke = Brushes.White,
                StrokeThickness = 3 * scale,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(panel, left);
            Canvas.SetTop(panel, top);
            canvas.Children.Add(panel);

            TextBlock heading = new TextBlock
            {
                Text = "RENNERGEBNIS",
                Foreground = Brushes.Black,
                FontFamily = GameFont,
                FontSize = 20 * scale,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Width = panelWidth
            };
            AddText(canvas, heading, left, top + 8 * scale);

            double columnWidth = panelWidth / 2;
            double resultFontSize = Math.Max(
                9, Math.Min(15 * scale, columnWidth / 17));
            AddRaceResultColumn(
                canvas,
                "PLAYER 1",
                FormatTime(field.PlayerOneRaceTimeSeconds),
                FormatSpeed(field.PlayerOneFinishSpeedKmh),
                FormatReaction(field.PlayerOneReactionTimeMs),
                left,
                top + 42 * scale,
                columnWidth,
                resultFontSize,
                scale);
            AddRaceResultColumn(
                canvas,
                "PLAYER 2",
                FormatTime(field.PlayerTwoRaceTimeSeconds),
                FormatSpeed(field.PlayerTwoFinishSpeedKmh),
                FormatReaction(field.PlayerTwoReactionTimeMs),
                left + columnWidth,
                top + 42 * scale,
                columnWidth,
                resultFontSize,
                scale);
        }

        private static void AddRaceResultColumn(
            Canvas canvas,
            string player,
            string time,
            string speed,
            string reaction,
            double left,
            double top,
            double width,
            double fontSize,
            double scale)
        {
            TextBlock result = new TextBlock
            {
                Text = player + "\nZEIT: " + time +
                    "\nENDGESCHW.: " + speed +
                    "\nREAKTION: " + reaction,
                Foreground = Brushes.Black,
                FontFamily = GameFont,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Width = width,
                IsHitTestVisible = false
            };
            AddText(canvas, result, left, top);
        }

        private static string FormatTime(double? seconds)
        {
            return seconds.HasValue
                ? seconds.Value.ToString("F2", CultureInfo.CurrentCulture) + " s"
                : "--";
        }

        private static string FormatSpeed(double? speedKmh)
        {
            return speedKmh.HasValue
                ? speedKmh.Value.ToString("F0", CultureInfo.CurrentCulture) +
                    " km/h"
                : "--";
        }

        private static string FormatReaction(double? reactionMs)
        {
            return reactionMs.HasValue
                ? reactionMs.Value.ToString("F0", CultureInfo.CurrentCulture) +
                    " ms"
                : "--";
        }

        private static void AddText(
            Canvas canvas, TextBlock text, double left, double top)
        {
            Canvas.SetLeft(text, left);
            Canvas.SetTop(text, top);
            canvas.Children.Add(text);

            TextBlock whiteOverlay = new TextBlock
            {
                Text = text.Text,
                FontFamily = text.FontFamily,
                FontSize = text.FontSize,
                FontStretch = text.FontStretch,
                FontStyle = text.FontStyle,
                FontWeight = text.FontWeight,
                Foreground = Brushes.White,
                TextAlignment = text.TextAlignment,
                TextWrapping = text.TextWrapping,
                TextTrimming = text.TextTrimming,
                LineHeight = text.LineHeight,
                Width = text.Width,
                Height = text.Height,
                MaxWidth = text.MaxWidth,
                MaxHeight = text.MaxHeight,
                Padding = text.Padding,
                FlowDirection = text.FlowDirection,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(whiteOverlay, left - 5);
            Canvas.SetTop(whiteOverlay, top - 5);
            canvas.Children.Add(whiteOverlay);
        }

        private static void PaintRaceProgress(
            Canvas canvas,
            IGruppe3DragRaceField field,
            double width,
            double height,
            double scale)
        {
            double barWidth = Math.Min(width - 40 * scale, 620 * scale);
            double barHeight = 4;
            double left = (width - barWidth) / 2;
            double top = Math.Max(0, height - 18);
            AddProgressBar(
                canvas, field.PlayerOneDistance,
                left, top, barWidth, barHeight, Brushes.Gold);
            AddProgressBar(
                canvas, field.PlayerTwoDistance,
                left, top + barHeight + 2,
                barWidth, barHeight, Brushes.Cyan);
        }

        private static void AddProgressBar(
            Canvas canvas,
            double distance,
            double left,
            double top,
            double width,
            double height,
            Brush foreground)
        {
            ProgressBar progress = new ProgressBar
            {
                Minimum = 0,
                Maximum = Gruppe3DragRaceField.FinishDistance,
                Value = Math.Min(
                    Gruppe3DragRaceField.FinishDistance, Math.Max(0, distance)),
                Width = width,
                Height = height,
                Foreground = foreground,
                Background = new SolidColorBrush(Color.FromRgb(65, 68, 78))
            };
            Canvas.SetLeft(progress, left);
            Canvas.SetTop(progress, top);
            canvas.Children.Add(progress);
        }

        private static void PaintSpeedometers(
            Canvas canvas,
            IGruppe3DragRaceField field,
            double width,
            double height,
            double scale)
        {
            double gaugeSize = Math.Min(
                160,
                Math.Max(
                    50,
                    Math.Min(width / 5.2, height / 3.1)));
            double visibleDashboardWidth = gaugeSize * 2.2;
            double dashboardHeight = visibleDashboardWidth * 572.0 / 1024.0;
            double gaugeTop = Math.Max(
                5,
                height - dashboardHeight - 20);
            double centerOffset =
                Math.Min(width * 0.28, (width - visibleDashboardWidth) / 2);
            double centerOne = width / 2 - centerOffset;
            double centerTwo = width / 2 + centerOffset;

            AddSpeedometer(
                canvas, "P1", field.PlayerOneSpeed, field.PlayerOneRpm,
                centerOne - gaugeSize / 2, gaugeTop, gaugeSize,
                Brushes.Gold, field.PlayerOneGear, field.PlayerOneCar);
            AddSpeedometer(
                canvas, "P2", field.PlayerTwoSpeed, field.PlayerTwoRpm,
                centerTwo - gaugeSize / 2, gaugeTop, gaugeSize,
                Brushes.Cyan, field.PlayerTwoGear, field.PlayerTwoCar);
        }

        private static void AddSpeedometer(
            Canvas canvas,
            string player,
            double engineSpeed,
            double rpm,
            double left,
            double top,
            double size,
            Brush accent,
            int gear,
            int carIndex)
        {
            BitmapImage dashboard = carIndex >= 0 &&
                carIndex < _dashboardImages.Length
                ? _dashboardImages[carIndex]
                : null;
            if (dashboard != null)
            {
                double dashboardWidth = size * 2.2;
                double dashboardHeight =
                    dashboardWidth * dashboard.PixelHeight /
                    dashboard.PixelWidth;
                Image dashboardImage = new Image
                {
                    Source = dashboard,
                    Width = dashboardWidth,
                    Height = dashboardHeight,
                    Stretch = Stretch.Fill,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(dashboardImage, left - size * 0.6);
                Canvas.SetTop(dashboardImage, top - size * 0.05);
                canvas.Children.Add(dashboardImage);
                AddDashboardNeedles(
                    canvas, carIndex, rpm, engineSpeed,
                    left - size * 0.6,
                    top - size * 0.05,
                    dashboardWidth,
                    dashboardHeight,
                    dashboard);
            }

        }

        private static void AddDashboardNeedles(
            Canvas canvas,
            int carIndex,
            double rpm,
            double engineSpeed,
            double dashboardLeft,
            double dashboardTop,
            double dashboardWidth,
            double dashboardHeight,
            BitmapImage dashboard)
        {
            Cars car = Gruppe3DragRaceCars.Get(carIndex);
            double dashboardScale = dashboardWidth / dashboard.PixelWidth;
            double rpmPivotX =
                dashboardLeft + car.RpmPivotX * dashboardScale;
            double rpmPivotY =
                dashboardTop + car.RpmPivotY * dashboardScale;
            double speedPivotX =
                dashboardLeft + car.SpeedPivotX * dashboardScale;
            double speedPivotY =
                dashboardTop + car.SpeedPivotY * dashboardScale;

            double rpmAngle = car.GetRpmNeedleAngle(rpm);
            if (rpm >= car.MaxRpm - 200)
            {
                long jitterStep =
                    DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond / 35;
                rpmAngle += jitterStep % 2 == 0 ? 1.2 : -1.2;
            }
            double speedAngle = car.GetSpeedNeedleAngle(engineSpeed);
            double rpmRange = car.MaxRpm - car.IdleRpm;
            double turboProgress = rpmRange <= 0
                ? 0
                : (rpm - car.IdleRpm) / rpmRange;
            turboProgress = Math.Max(0, Math.Min(1, turboProgress));
            double turboValue = car.TurboAtMaxAngle * turboProgress;
            double turboAngle = car.GetTurboNeedleAngle(turboValue);

            BitmapImage rpmImage = carIndex >= 0 &&
                carIndex < _rpmNeedleImages.Length
                ? _rpmNeedleImages[carIndex]
                : null;
            BitmapImage speedImage = carIndex >= 0 &&
                carIndex < _speedNeedleImages.Length
                ? _speedNeedleImages[carIndex]
                : null;
            BitmapImage turboImage = carIndex >= 0 &&
                carIndex < _turboNeedleImages.Length
                ? _turboNeedleImages[carIndex]
                : null;

            if (rpmImage != null)
            {
                AddRotatedNeedle(
                    canvas, rpmImage, rpmPivotX, rpmPivotY,
                    dashboardScale, rpmAngle);
            }
            if (speedImage != null)
            {
                AddRotatedNeedle(
                    canvas, speedImage, speedPivotX, speedPivotY,
                    dashboardScale, speedAngle);
            }
            if (turboImage != null)
            {
                double turboPivotX =
                    dashboardLeft + car.TurboPivotX * dashboardScale;
                double turboPivotY =
                    dashboardTop + car.TurboPivotY * dashboardScale;
                AddRotatedNeedle(
                    canvas, turboImage, turboPivotX, turboPivotY,
                    dashboardScale, turboAngle);
            }
        }

        private static void AddRotatedNeedle(
            Canvas canvas,
            BitmapImage needleImage,
            double pivotX,
            double pivotY,
            double imageScale,
            double angle)
        {
            double needleWidth = needleImage.PixelWidth * imageScale;
            double needleHeight = needleImage.PixelHeight * imageScale;
            Image needle = new Image
            {
                Source = needleImage,
                Width = needleWidth,
                Height = needleHeight,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(angle)
            };
            Canvas.SetLeft(needle, pivotX - needleWidth / 2);
            Canvas.SetTop(needle, pivotY - needleHeight / 2);
            canvas.Children.Add(needle);
        }

        private static void AddSharedSelectionBorder(
            Canvas canvas,
            double left,
            double top,
            double width,
            double height,
            double thickness)
        {
            double right = left + width;
            double bottom = top + height;
            double corner = Math.Min(thickness, Math.Min(width, height) / 2);

            AddBorderPolygon(
                canvas,
                Brushes.Gold,
                new Point(left, top),
                new Point(right, top),
                new Point(right - corner, top + corner),
                new Point(left + corner, top + corner));
            AddBorderPolygon(
                canvas,
                Brushes.Gold,
                new Point(left, top),
                new Point(left + corner, top + corner),
                new Point(left + corner, bottom - corner),
                new Point(left, bottom));
            AddBorderPolygon(
                canvas,
                Brushes.Cyan,
                new Point(left, bottom),
                new Point(left + corner, bottom - corner),
                new Point(right - corner, bottom - corner),
                new Point(right, bottom));
            AddBorderPolygon(
                canvas,
                Brushes.Cyan,
                new Point(right - corner, top + corner),
                new Point(right, top),
                new Point(right, bottom),
                new Point(right - corner, bottom - corner));
        }

        private static void AddBorderPolygon(
            Canvas canvas, Brush brush, params Point[] points)
        {
            Polygon polygon = new Polygon
            {
                Fill = brush,
                Points = new PointCollection(points),
                IsHitTestVisible = false
            };
            canvas.Children.Add(polygon);
        }

        private static bool IsSelected(
            IGruppe3DragRaceField field, int carIndex)
        {
            return field.PlayerOneCar == carIndex ||
                field.PlayerTwoCar == carIndex;
        }

        private static Brush GetSelectionBrush(
            IGruppe3DragRaceField field, int carIndex)
        {
            if (field.PlayerOneCar == carIndex &&
                field.PlayerTwoCar == carIndex)
            {
                return Brushes.White;
            }

            if (field.PlayerOneCar == carIndex)
            {
                return Brushes.Gold;
            }

            if (field.PlayerTwoCar == carIndex)
            {
                return Brushes.Cyan;
            }

            return Brushes.White;
        }

        private static double GetCameraPosition(
            IGruppe3DragRaceField field, double width, double scale)
        {
            double leadingDistance = Math.Max(
                field.PlayerOneDistance, field.PlayerTwoDistance);
            double carStartPosition = 20 * scale;
            double centerPosition = width * 0.5 - 55 * scale;
            return Math.Max(
                0, leadingDistance * scale - (centerPosition - carStartPosition));
        }

        private static void DrawTrack(
            Canvas canvas,
            double width,
            double laneHeight,
            double trackTop,
            double cameraPosition,
            double scale)
        {
            for (int lane = 0; lane < 2; lane++)
            {
                Rectangle road = new Rectangle
                {
                    Width = width,
                    Height = laneHeight,
                    Fill = new SolidColorBrush(Color.FromRgb(55, 58, 68))
                };
                Canvas.SetTop(road, trackTop + lane * laneHeight);
                canvas.Children.Add(road);

                Line finish = new Line
                {
                    X1 = 20 * scale +
                        Gruppe3DragRaceField.FinishDistance * scale - cameraPosition,
                    X2 = 20 * scale +
                        Gruppe3DragRaceField.FinishDistance * scale - cameraPosition,
                    Y1 = trackTop + lane * laneHeight,
                    Y2 = trackTop + lane * laneHeight + laneHeight,
                    Stroke = Brushes.White,
                    StrokeThickness = 4 * scale,
                    StrokeDashArray = new DoubleCollection { 2, 2 }
                };
                canvas.Children.Add(finish);

                for (int marker = -1; marker < 20; marker++)
                {
                    double markerX = marker * 90 * scale -
                        cameraPosition % (90 * scale);
                    Line roadMarker = new Line
                    {
                        X1 = markerX,
                        X2 = markerX + 45 * scale,
                        Y1 = trackTop + lane * laneHeight + laneHeight / 2,
                        Y2 = trackTop + lane * laneHeight + laneHeight / 2,
                        Stroke = Brushes.White,
                        StrokeThickness = 2 * scale,
                        Opacity = 0.55
                    };
                    canvas.Children.Add(roadMarker);
                }
            }
        }

        private void AddCar(
            Canvas canvas,
            double distance,
            double width,
            double top,
            int carIndex,
            double cameraPosition,
            double scale)
        {
            double left = 20 * scale + distance * scale - cameraPosition;
            if (_carImages[carIndex] == null)
            {
                AddFallbackCar(canvas, left, top, carIndex, scale);
                return;
            }

            AddCarImage(canvas, carIndex, left, top, 105 * scale, 63 * scale);
        }

        private void AddCarImage(
            Canvas canvas, int carIndex, double left, double top,
            double width, double height)
        {
            if (carIndex < 0 || carIndex >= _carImages.Length ||
                _carImages[carIndex] == null)
            {
                AddFallbackCar(canvas, left, top, carIndex, 1);
                return;
            }

            Image car = new Image
            {
                Source = _carImages[carIndex],
                Width = width,
                Height = height,
                Stretch = Stretch.Uniform,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(car, left);
            Canvas.SetTop(car, top);
            canvas.Children.Add(car);
        }

        private static string FindAssetDirectory()
        {
            DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
            for (int level = 0; level < 8 && directory != null; level++)
            {
                string candidate = System.IO.Path.Combine(
                    directory.FullName,
                    "Assets",
                    "Gruppe_3_Game");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            string currentDirectoryCandidate = System.IO.Path.Combine(
                Environment.CurrentDirectory,
                "Assets",
                "Gruppe_3_Game");
            return Directory.Exists(currentDirectoryCandidate)
                ? currentDirectoryCandidate
                : null;
        }

        private static BitmapImage LoadBitmap(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(path, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return image;
        }

        private static FontFamily LoadGameFont()
        {
            string assetDirectory = FindAssetDirectory();
            if (assetDirectory == null)
            {
                throw new DirectoryNotFoundException(
                    "The Gruppe 3 game asset directory could not be found.");
            }

            string fontPath = System.IO.Path.Combine(
                assetDirectory, "Font", "SF Pixelate Bold.ttf");
            if (!File.Exists(fontPath))
            {
                throw new FileNotFoundException(
                    "The Gruppe 3 game font could not be found.", fontPath);
            }

            Uri fontUri = new Uri(fontPath, UriKind.Absolute);
            foreach (FontFamily family in Fonts.GetFontFamilies(fontUri))
            {
                foreach (string familyName in family.FamilyNames.Values)
                {
                    if (string.Equals(
                        familyName,
                        "SF Pixelate",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return family;
                    }
                }
            }

            throw new InvalidDataException(
                "The font file does not contain the SF Pixelate family.");
        }

        private static void AddFallbackCar(
            Canvas canvas, double left, double top, int carIndex, double scale)
        {
            Rectangle body = new Rectangle
            {
                Width = 105 * scale,
                Height = 28 * scale,
                RadiusX = 5 * scale,
                RadiusY = 5 * scale,
                Fill = carIndex % 2 == 0 ? Brushes.Red : Brushes.DeepSkyBlue
            };
            Canvas.SetLeft(body, left);
            Canvas.SetTop(body, top + 20 * scale);
            canvas.Children.Add(body);

            Rectangle cabin = new Rectangle
            {
                Width = 45 * scale,
                Height = 18 * scale,
                Fill = carIndex % 2 == 0 ? Brushes.DarkRed : Brushes.DarkBlue
            };
            Canvas.SetLeft(cabin, left + 27 * scale);
            Canvas.SetTop(cabin, top + 5 * scale);
            canvas.Children.Add(cabin);
        }
    }
}
