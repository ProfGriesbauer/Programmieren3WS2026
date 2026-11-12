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
        private readonly BitmapImage _spriteSheet;
        private double _selectionCardWidth = 220;
        private double _selectionCardHeight = 120;
        private double _selectionGap = 15;
        private double _selectionLeft = 20;
        private double _selectionTop = 70;

        public Gruppe3DragRacePainter()
        {
            string assetPath = FindAssetPath();
            if (assetPath != null)
            {
                BitmapImage spriteSheet = new BitmapImage();
                spriteSheet.BeginInit();
                spriteSheet.UriSource = new Uri(assetPath, UriKind.Absolute);
                spriteSheet.CacheOption = BitmapCacheOption.OnLoad;
                spriteSheet.EndInit();
                _spriteSheet = spriteSheet;
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

            if (!field.IsRaceStarted || field.Winner != 0)
            {
                TextBlock status = new TextBlock
                {
                    Text = field.CountdownText,
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
            if (column < 0 || column >= 3 || row < 0 || row >= 3 ||
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
            double scale = Math.Max(0.75, Math.Min(2.2,
                Math.Min(width / 700, height / 450)));
            _selectionCardWidth = 220 * scale;
            _selectionCardHeight = 120 * scale;
            _selectionGap = 15 * scale;
            _selectionLeft = Math.Max(10, (width -
                (3 * _selectionCardWidth + 2 * _selectionGap)) / 2);
            _selectionTop = Math.Max(55, (height -
                (3 * _selectionCardHeight + 2 * _selectionGap)) / 2);

            TextBlock title = new TextBlock
            {
                Text = field.CountdownText,
                Foreground = Brushes.White,
                FontSize = 30 * scale,
                FontWeight = FontWeights.Bold
            };
            Canvas.SetLeft(title, _selectionLeft);
            Canvas.SetTop(title, Math.Max(5, _selectionTop - 45 * scale));
            canvas.Children.Add(title);

            for (int carIndex = 0; carIndex < 9; carIndex++)
            {
                int column = carIndex % 3;
                int row = carIndex / 3;
                double left = _selectionLeft +
                    column * (_selectionCardWidth + _selectionGap);
                double top = _selectionTop +
                    row * (_selectionCardHeight + _selectionGap);
                Rectangle card = new Rectangle
                {
                    Width = _selectionCardWidth,
                    Height = _selectionCardHeight,
                    Fill = new SolidColorBrush(Color.FromRgb(55, 58, 68)),
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };
                Canvas.SetLeft(card, left);
                Canvas.SetTop(card, top);
                canvas.Children.Add(card);
                AddCarImage(
                    canvas, carIndex, left + 0.19 * _selectionCardWidth,
                    top + 0.1 * _selectionCardHeight,
                    0.62 * _selectionCardWidth,
                    0.8 * _selectionCardHeight);
            }
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
            if (_spriteSheet == null)
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
            if (_spriteSheet == null)
            {
                AddFallbackCar(canvas, left, top, carIndex, 1);
                return;
            }

            Image car = new Image
            {
                Source = new CroppedBitmap(_spriteSheet, new Int32Rect(
                    (carIndex % 3) * 208,
                    (carIndex / 3) * 125,
                    208,
                    125)),
                Width = width,
                Height = height,
                Stretch = Stretch.Uniform,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(car, left);
            Canvas.SetTop(car, top);
            canvas.Children.Add(car);
        }

        private static string FindAssetPath()
        {
            DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
            for (int level = 0; level < 8 && directory != null; level++)
            {
                string candidate = System.IO.Path.Combine(
                    directory.FullName,
                    "Assets",
                    "Gruppe_3_Game",
                    "autos_assets.png");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            string currentDirectoryCandidate = System.IO.Path.Combine(
                Environment.CurrentDirectory,
                "Assets",
                "Gruppe_3_Game",
                "autos_assets.png");
            return File.Exists(currentDirectoryCandidate)
                ? currentDirectoryCandidate
                : null;
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
