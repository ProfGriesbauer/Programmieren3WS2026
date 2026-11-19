using System;
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
        private static readonly BitmapImage[] _carImages =
            new BitmapImage[3];
        private static readonly BitmapImage[] _dashboardImages =
            new BitmapImage[3];
        private static readonly BitmapImage[] _rpmNeedleImages =
            new BitmapImage[3];
        private static readonly BitmapImage[] _speedNeedleImages =
            new BitmapImage[3];
        private double _selectionCardWidth = 220;
        private double _selectionCardHeight = 120;
        private double _selectionGap = 15;
        private double _selectionLeft = 20;
        private double _selectionTop = 70;
        private int _lastPlayerOneCar = -1;
        private int _lastPlayerTwoCar = -1;
        private DateTime _selectionFeedbackUntil;
        private string _selectionFeedbackText;

        public Gruppe3DragRacePainter()
        {
            string basePath = FindAssetDirectory();
            if (basePath == null)
            {
                return;
            }

            _carImages[0] = LoadBitmap(
                System.IO.Path.Combine(basePath, "Golf", "Golf.png"));
            _carImages[1] = LoadBitmap(
                System.IO.Path.Combine(basePath, "Ford Mustang", "FordMustang.png"));
            _carImages[2] = LoadBitmap(
                System.IO.Path.Combine(basePath, "Audi", "AudiRS4.png"));

            _dashboardImages[0] = LoadBitmap(
                System.IO.Path.Combine(basePath, "Golf", "Golf_Tacho_Hintergrund.png"));
            _dashboardImages[1] = LoadBitmap(
                System.IO.Path.Combine(basePath, "Ford Mustang", "FordMustang_Tacho_Hintergrund.png"));
            _dashboardImages[2] = LoadBitmap(
                System.IO.Path.Combine(basePath, "Audi", "AudiRS4_Tacho_Hintergrund.png"));

            _rpmNeedleImages[0] = LoadBitmap(
                System.IO.Path.Combine(basePath, "Golf", "Golf_Tacho_Drehzahlnadel.png"));
            _rpmNeedleImages[1] = LoadBitmap(
                System.IO.Path.Combine(basePath, "Ford Mustang", "FordMustang_Tacho_Drehzahlnadel.png"));
            _rpmNeedleImages[2] = null;

            _speedNeedleImages[0] = LoadBitmap(
                System.IO.Path.Combine(basePath, "Golf", "Golf_Tacho_Tachonadel.png"));
            _speedNeedleImages[1] = LoadBitmap(
                System.IO.Path.Combine(basePath, "Ford Mustang", "FordMustang_Tacho_Tachonadel.png"));
            _speedNeedleImages[2] = LoadBitmap(
                System.IO.Path.Combine(basePath, "Audi", "AudiRS4_Tacho_Nadel.png"));
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
            UpdateSelectionFeedback(field);
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
                    Foreground = Brushes.White,
                    FontSize = 32 * scale,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    Width = width
                };
                Canvas.SetLeft(status, 0);
                Canvas.SetTop(status, Math.Max(5, trackTop - 65 * scale));
                canvas.Children.Add(status);
            }

            if (field.IsRaceStarted || field.Winner != 0)
            {
                PaintRaceProgress(canvas, field, width, height, scale);
                PaintSpeedometers(canvas, field, width, height, scale);
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
            if (column < 0 || column >= 3 || row != 0 ||
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

            UpdateSelectionFeedback(field);
            TextBlock title = new TextBlock
            {
                Text = DateTime.UtcNow < _selectionFeedbackUntil
                    ? _selectionFeedbackText
                    : field.CountdownText,
                Foreground = Brushes.White,
                FontSize = 30 * scale,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(title, _selectionLeft);
            Canvas.SetTop(title, Math.Max(5, _selectionTop - 45 * scale));
            canvas.Children.Add(title);

            for (int carIndex = 0; carIndex < 3; carIndex++)
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
                15 * scale, textWidth / 48));
            TextBlock controls = new TextBlock
            {
                Text = "PLAYER 1:  W = GAS    S = BREMSE       " +
                    "PLAYER 2:  NUMPAD 8 / ↑ = GAS    NUMPAD 5 / ↓ = BREMSE\n" +
                    "SCHALTEN:  P1  Q = HOCH  E = RUNTER       " +
                    "P2  7 / Numpad 7 = HOCH  9 / Numpad 9 / Bild↑ / Bild↓",
                Foreground = Brushes.White,
                FontSize = fontSize,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Width = textWidth,
                TextWrapping = TextWrapping.Wrap,
                MaxHeight = Math.Max(30, 42 * scale)
            };
            Canvas.SetLeft(controls, (width - textWidth) / 2);
            Canvas.SetTop(controls, Math.Max(0, height - 45 * scale));
            canvas.Children.Add(controls);
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
                    dashboardHeight);
            }

            double speedRatio = Math.Min(1, engineSpeed / 16);
            double kmh = speedRatio * 320;

        }

        private static void AddDashboardNeedles(
            Canvas canvas,
            int carIndex,
            double rpm,
            double engineSpeed,
            double dashboardLeft,
            double dashboardTop,
            double dashboardWidth,
            double dashboardHeight)
        {
            double rpmPivotX = dashboardLeft + dashboardWidth * 0.23;
            double rpmPivotY = dashboardTop + dashboardHeight * 0.56;
            double speedPivotX = dashboardLeft + dashboardWidth * 0.78;
            double speedPivotY = dashboardTop + dashboardHeight * 0.56;

            double rpmRatio = Math.Max(0, Math.Min(1, rpm / 8000));
            double speedRatio = Math.Max(0, Math.Min(1, engineSpeed / 16));
            double rpmAngle = -135 + rpmRatio * 270;
            if (rpm >= 7800)
            {
                long jitterStep =
                    DateTime.UtcNow.Ticks / TimeSpan.TicksPerMillisecond / 35;
                rpmAngle += jitterStep % 2 == 0 ? 1.2 : -1.2;
            }
            double speedAngle = -135 + speedRatio * 270;

            BitmapImage rpmImage = carIndex >= 0 &&
                carIndex < _rpmNeedleImages.Length
                ? _rpmNeedleImages[carIndex]
                : null;
            BitmapImage speedImage = carIndex >= 0 &&
                carIndex < _speedNeedleImages.Length
                ? _speedNeedleImages[carIndex]
                : null;

            if (rpmImage != null)
            {
                AddRotatedNeedle(
                    canvas, rpmImage, rpmPivotX, rpmPivotY,
                    dashboardWidth * 0.16, rpmAngle);
            }
            if (speedImage != null)
            {
                AddRotatedNeedle(
                    canvas, speedImage, speedPivotX, speedPivotY,
                    dashboardWidth * 0.16, speedAngle);
            }
        }

        private static void AddRotatedNeedle(
            Canvas canvas,
            BitmapImage needleImage,
            double pivotX,
            double pivotY,
            double needleWidth,
            double angle)
        {
            double needleHeight =
                needleWidth * needleImage.PixelHeight /
                needleImage.PixelWidth;
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

        private void UpdateSelectionFeedback(IGruppe3DragRaceField field)
        {
            if (field.PlayerOneCar != _lastPlayerOneCar)
            {
                if (field.PlayerOneCar >= 0)
                {
                    _selectionFeedbackText =
                        "PLAYER 1: AUTO " + (field.PlayerOneCar + 1) + " GEWÄHLT!";
                    _selectionFeedbackUntil =
                        DateTime.UtcNow.AddSeconds(1.2);
                }

                _lastPlayerOneCar = field.PlayerOneCar;
            }

            if (field.PlayerTwoCar != _lastPlayerTwoCar)
            {
                if (field.PlayerTwoCar >= 0)
                {
                    _selectionFeedbackText =
                        "PLAYER 2: AUTO " + (field.PlayerTwoCar + 1) + " GEWÄHLT!";
                    _selectionFeedbackUntil =
                        DateTime.UtcNow.AddSeconds(1.2);
                }

                _lastPlayerTwoCar = field.PlayerTwoCar;
            }
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
