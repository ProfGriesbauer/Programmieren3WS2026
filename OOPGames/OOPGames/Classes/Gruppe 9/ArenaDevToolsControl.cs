using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Path = System.IO.Path;

namespace OOPGames
{
    internal sealed class ArenaDevToolsControl : UserControl
    {
        private readonly ArenaField _field;
        private readonly Action _close;

        public ArenaDevToolsControl(ArenaField field, Action close)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _close = close ?? throw new ArgumentNullException(nameof(close));
            Content = BuildContent();
        }

        private UIElement BuildContent()
        {
            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            StackPanel heading = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 10)
            };
            heading.Children.Add(new TextBlock
            {
                Text = "ARENA DEV TOOL",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            });
            Button back = new Button
            {
                Content = "Zurück zum Startmenü",
                Margin = new Thickness(20, 0, 0, 0),
                Padding = new Thickness(12, 6, 12, 6),
                VerticalAlignment = VerticalAlignment.Center
            };
            back.Click += (sender, args) => _close();
            heading.Children.Add(back);
            root.Children.Add(heading);

            TabControl tabs = new TabControl
            {
                Background = new SolidColorBrush(Color.FromRgb(22, 30, 40)),
                Foreground = Brushes.White
            };
            tabs.Items.Add(new TabItem
            {
                Header = "Map Editor",
                Content = new ArenaMapEditorControl(_field)
            });
            tabs.Items.Add(new TabItem
            {
                Header = "Skilltree Editor",
                Content = new ArenaSkillTreeEditorControl(_field)
            });
            Grid.SetRow(tabs, 1);
            root.Children.Add(tabs);

            TextBlock hint = new TextBlock
            {
                Text = "Die Editoren nutzen den verfügbaren Spielbereich; F11 schaltet in die Vollbildansicht.",
                Foreground = new SolidColorBrush(Color.FromRgb(183, 197, 212)),
                Margin = new Thickness(0, 8, 0, 0)
            };
            Grid.SetRow(hint, 2);
            root.Children.Add(hint);
            return root;
        }
    }

    internal sealed class ArenaMapEditorControl : UserControl
    {
        private const string MapDragFormat = "OOPGames.ArenaMapElement";
        private const int MapGridSize = 8;
        private const int MajorMapGridSize = 256;
        private const int RandomBushFreeCheckRadius = 55;
        private const byte BackgroundColorTolerance = 12;
        private readonly ArenaField _field;
        private readonly List<ArenaMapElement> _elements;
        private readonly List<ArenaMapPlacement> _placements =
            new List<ArenaMapPlacement>();
        private readonly Canvas _mapCanvas;
        private readonly ListBox _palette;
        private readonly ComboBox _mapSelection;
        private readonly ComboBox _baseSelection;
        private readonly TextBox _widthInput;
        private readonly TextBox _heightInput;
        private readonly TextBox _mapNameInput;
        private readonly TextBlock _status;
        private readonly TextBlock _mapZoomLabel;
        private readonly ScaleTransform _mapZoomTransform = new ScaleTransform(1, 1);
        private readonly Button _rotatePendingButton;
        private readonly Button _flipPendingHorizontalButton;
        private readonly Button _flipPendingVerticalButton;
        private readonly CheckBox _mapGridCheckBox;
        private readonly Button _mapGridSnapButton;
        private BitmapSource _baseImage;
        private ArenaMapElement _draggedElement;
        private ArenaMapPlacement _pendingPlacement;
        private Image _mapDragPreview;
        private Point _dragStart;
        private double _mapZoom = 1.0;
        private bool _isMovingPendingPlacement;
        private bool _snapToGrid = true;

        public ArenaMapEditorControl(ArenaField field)
        {
            _field = field;
            PreviewKeyDown += OnMapEditorPreviewKeyDown;
            _elements = ArenaMapAssets.LoadMapElements().ToList();
            _baseImage = ArenaMapAssets.LoadMapBase();

            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            StackPanel toolbar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(4)
            };
            toolbar.Children.Add(new TextBlock
            {
                Text = "Breite (px)",
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            });
            _widthInput = CreateIntegerTextBox("1800", 90);
            toolbar.Children.Add(_widthInput);
            toolbar.Children.Add(new TextBlock
            {
                Text = "Höhe (px)",
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 5, 0)
            });
            _heightInput = CreateIntegerTextBox("1200", 90);
            toolbar.Children.Add(_heightInput);
            Button applySize = CreateButton("Größe anwenden");
            applySize.Click += (sender, args) => ApplyMapSize();
            toolbar.Children.Add(applySize);
            toolbar.Children.Add(new TextBlock
            {
                Text = "Kartenbasis",
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 5, 0)
            });
            _baseSelection = new ComboBox
            {
                MinWidth = 160,
                ItemsSource = ArenaMapAssets.GetMapBaseFileNames(),
                SelectedIndex = 0,
                Margin = new Thickness(0, 0, 5, 0)
            };
            _baseSelection.SelectionChanged += OnBaseSelectionChanged;
            toolbar.Children.Add(_baseSelection);
            toolbar.Children.Add(new TextBlock
            {
                Text = "Name",
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 5, 0)
            });
            _mapNameInput = CreateTextBox("NeueMap", 150);
            toolbar.Children.Add(_mapNameInput);
            Button saveMap = CreateButton("Karte speichern");
            saveMap.Click += (sender, args) => SaveMap();
            toolbar.Children.Add(saveMap);
            root.Children.Add(toolbar);

            Grid workspace = new Grid();
            workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(220) });
            workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _palette = new ListBox
            {
                ItemsSource = _elements,
                ItemTemplate = CreateElementTemplate(),
                Margin = new Thickness(4),
                Background = new SolidColorBrush(Color.FromRgb(30, 40, 52))
            };
            _palette.PreviewMouseLeftButtonDown += OnPaletteMouseDown;
            _palette.PreviewMouseMove += OnPaletteMouseMove;
            _palette.PreviewMouseLeftButtonUp += OnPaletteMouseLeftButtonUp;
            Grid palettePanel = new Grid();
            palettePanel.RowDefinitions.Add(
                new RowDefinition { Height = GridLength.Auto });
            palettePanel.RowDefinitions.Add(
                new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            Button randomBush = CreateButton("Busch zufällig platzieren");
            randomBush.Margin = new Thickness(4);
            randomBush.Click += (sender, args) => PlaceRandomGrassBush();
            palettePanel.Children.Add(randomBush);
            Grid.SetRow(_palette, 1);
            palettePanel.Children.Add(_palette);
            workspace.Children.Add(palettePanel);

            _mapCanvas = new Canvas
            {
                Width = 1800,
                Height = 1200,
                Background = Brushes.Black,
                AllowDrop = true,
                Focusable = true,
                LayoutTransform = _mapZoomTransform,
                ClipToBounds = true
            };
            _mapCanvas.DragEnter += OnMapDragOver;
            _mapCanvas.DragOver += OnMapDragOver;
            _mapCanvas.DragLeave += OnMapDragLeave;
            _mapCanvas.Drop += OnMapDrop;
            _mapCanvas.MouseMove += OnMapMouseMove;
            _mapCanvas.PreviewMouseLeftButtonUp += OnPendingPlacementMouseUp;
            _mapCanvas.LostMouseCapture += OnMapLostMouseCapture;
            _mapCanvas.MouseRightButtonDown += OnMapRightButtonDown;
            ScrollViewer mapScroll = new ScrollViewer
            {
                Content = _mapCanvas,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(4),
                Background = new SolidColorBrush(Color.FromRgb(17, 23, 31))
            };
            mapScroll.PreviewMouseWheel += OnMapPreviewMouseWheel;
            Grid mapWorkspace = new Grid();
            mapWorkspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mapWorkspace.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mapWorkspace.PreviewKeyDown += OnMapKeyDown;
            StackPanel mapTools = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(4, 2, 4, 0)
            };
            mapTools.Children.Add(new TextBlock
            {
                Text = "Zoom",
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 5, 0)
            });
            Button zoomOut = CreateButton("−");
            zoomOut.Click += (sender, args) => ChangeMapZoom(1.0 / 1.2);
            mapTools.Children.Add(zoomOut);
            _mapZoomLabel = new TextBlock
            {
                Text = "100%",
                Foreground = Brushes.White,
                MinWidth = 48,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            mapTools.Children.Add(_mapZoomLabel);
            Button zoomIn = CreateButton("+");
            zoomIn.Click += (sender, args) => ChangeMapZoom(1.2);
            mapTools.Children.Add(zoomIn);
            Button resetZoom = CreateButton("100%");
            resetZoom.Click += (sender, args) => SetMapZoom(1.0);
            mapTools.Children.Add(resetZoom);
            _mapGridCheckBox = new CheckBox
            {
                Content = "Raster anzeigen (8 px)",
                IsChecked = true,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 4, 0)
            };
            _mapGridCheckBox.Checked += OnMapGridChanged;
            _mapGridCheckBox.Unchecked += OnMapGridChanged;
            mapTools.Children.Add(_mapGridCheckBox);
            _mapGridSnapButton = CreateButton("Einrasten: Ein");
            _mapGridSnapButton.Click += OnMapGridSnapButtonClick;
            mapTools.Children.Add(_mapGridSnapButton);
            mapTools.Children.Add(new TextBlock
            {
                Text = "Vorschau:",
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 4, 0)
            });
            _rotatePendingButton = CreateButton("Drehen (R)");
            _rotatePendingButton.Click += (sender, args) => RotatePendingPlacement();
            mapTools.Children.Add(_rotatePendingButton);
            _flipPendingHorizontalButton = CreateButton("Horizontal spiegeln (H)");
            _flipPendingHorizontalButton.Click +=
                (sender, args) => FlipPendingPlacement(true);
            mapTools.Children.Add(_flipPendingHorizontalButton);
            _flipPendingVerticalButton = CreateButton("Vertikal spiegeln (V)");
            _flipPendingVerticalButton.Click +=
                (sender, args) => FlipPendingPlacement(false);
            mapTools.Children.Add(_flipPendingVerticalButton);
            Grid.SetRow(mapTools, 0);
            mapWorkspace.Children.Add(mapTools);
            Grid.SetRow(mapScroll, 1);
            mapWorkspace.Children.Add(mapScroll);
            Grid.SetColumn(mapWorkspace, 1);
            workspace.Children.Add(mapWorkspace);
            Grid.SetRow(workspace, 1);
            root.Children.Add(workspace);

            StackPanel footer = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(4, 2, 4, 0)
            };
            Button removeLast = CreateButton("Letztes Element entfernen");
            removeLast.Click += (sender, args) =>
            {
                if (_placements.Count > 0)
                {
                    _placements.RemoveAt(_placements.Count - 1);
                    RedrawMap();
                }
            };
            footer.Children.Add(removeLast);
            Button newMap = CreateButton("Neue Karte");
            newMap.Click += (sender, args) => StartNewMap();
            footer.Children.Add(newMap);
            _mapSelection = new ComboBox
            {
                MinWidth = 220,
                ItemsSource = ArenaMapAssets.GetMapFileNames(),
                SelectedItem = _field.MapFileName,
                Margin = new Thickness(14, 0, 5, 0)
            };
            footer.Children.Add(_mapSelection);
            Button selectMap = CreateButton("Als Spielhintergrund verwenden");
            selectMap.Click += (sender, args) => SelectMap();
            footer.Children.Add(selectMap);
            _status = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromRgb(183, 197, 212)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(12, 0, 0, 0)
            };
            footer.Children.Add(_status);
            Grid.SetRow(footer, 2);
            root.Children.Add(footer);

            Content = root;
            UpdatePendingPlacementControls();
            RedrawMap();
        }

        private void ApplyMapSize()
        {
            if (!Int32.TryParse(_widthInput.Text, out int width)
                || !Int32.TryParse(_heightInput.Text, out int height))
            {
                ShowError("Breite und Höhe müssen ganze Pixelwerte sein.");
                return;
            }
            try
            {
                ArenaMapAssets.ValidateDimensions(width, height);
                _mapCanvas.Width = width;
                _mapCanvas.Height = height;
                RedrawMap();
                SetStatus("Kartengröße: " + width + " × " + height + " px.");
            }
            catch (ArgumentOutOfRangeException exception)
            {
                ShowError(exception.Message);
            }
        }

        private void OnBaseSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_baseSelection.SelectedItem is string fileName)
            {
                _baseImage = ArenaMapAssets.LoadMapBase(fileName);
                RedrawMap();
            }
        }

        private void OnPaletteMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_pendingPlacement != null)
            {
                _draggedElement = null;
                SetStatus("Platziere oder verwirf zuerst die aktuelle Vorschau.");
                return;
            }

            _dragStart = e.GetPosition(_palette);
            ListBoxItem item = ItemsControl.ContainerFromElement(
                _palette, e.OriginalSource as DependencyObject) as ListBoxItem;
            _draggedElement = item == null ? null : item.DataContext as ArenaMapElement;
        }

        private void OnPaletteMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed
                || _draggedElement == null)
            {
                return;
            }
            Point current = e.GetPosition(_palette);
            if (Math.Abs(current.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(current.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }
            DataObject data = new DataObject(MapDragFormat, _draggedElement.Name);
            DragDrop.DoDragDrop(_palette, data, DragDropEffects.Copy);
            _draggedElement = null;
        }

        private void OnPaletteMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _draggedElement = null;
        }

        private void OnMapDragOver(object sender, DragEventArgs e)
        {
            ArenaMapElement element = GetDraggedMapElement(e.Data);
            if (element == null)
            {
                HideMapDragPreview();
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            Point point = e.GetPosition(_mapCanvas);
            Point position = GetMapPlacementPosition(point, element.Image);
            ShowMapDragPreview(element, position);
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void OnMapDragLeave(object sender, DragEventArgs e)
        {
            HideMapDragPreview();
        }

        private void OnMapDrop(object sender, DragEventArgs e)
        {
            ArenaMapElement element = GetDraggedMapElement(e.Data);
            if (element == null)
            {
                return;
            }

            HideMapDragPreview();
            Point position = GetMapPlacementPosition(
                e.GetPosition(_mapCanvas), element.Image);
            _pendingPlacement = new ArenaMapPlacement(
                element.Image, position.X, position.Y);
            UpdatePendingPlacementControls();
            RedrawMap();
            Keyboard.Focus(_mapCanvas);
            SetStatus("Zum Verschieben Vorschau mit gedrückter Maustaste ziehen. "
                + "Enter: platzieren · Esc: verwerfen · R: drehen · H/V: spiegeln.");
            e.Handled = true;
        }

        private void OnMapMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMovingPendingPlacement
                || e.LeftButton != MouseButtonState.Pressed
                || _pendingPlacement == null
                || _mapDragPreview == null)
            {
                return;
            }

            MovePendingPlacementTo(e.GetPosition(_mapCanvas));
        }

        private void OnPendingPlacementMouseDown(
            object sender, MouseButtonEventArgs e)
        {
            if (_pendingPlacement == null)
            {
                return;
            }

            _isMovingPendingPlacement = true;
            _mapCanvas.CaptureMouse();
            MovePendingPlacementTo(e.GetPosition(_mapCanvas));
            e.Handled = true;
        }

        private void OnPendingPlacementMouseUp(
            object sender, MouseButtonEventArgs e)
        {
            if (!_isMovingPendingPlacement)
            {
                return;
            }

            _isMovingPendingPlacement = false;
            if (_mapCanvas.IsMouseCaptured)
            {
                _mapCanvas.ReleaseMouseCapture();
            }
            e.Handled = true;
        }

        private void OnMapLostMouseCapture(object sender, MouseEventArgs e)
        {
            _isMovingPendingPlacement = false;
        }

        private void MovePendingPlacementTo(Point pointer)
        {
            if (_pendingPlacement == null || _mapDragPreview == null)
            {
                return;
            }

            Point position = GetMapPlacementPosition(
                pointer, _pendingPlacement.Image,
                _pendingPlacement.RotationDegrees);
            _pendingPlacement = new ArenaMapPlacement(
                _pendingPlacement.Image, position.X, position.Y,
                _pendingPlacement.RotationDegrees,
                _pendingPlacement.FlipHorizontal,
                _pendingPlacement.FlipVertical);
            SetPlacementImagePosition(_mapDragPreview, _pendingPlacement);
        }

        private ArenaMapElement GetDraggedMapElement(IDataObject data)
        {
            if (!data.GetDataPresent(MapDragFormat)
                || !(data.GetData(MapDragFormat) is string name))
            {
                return null;
            }
            return _elements.FirstOrDefault(
                item => String.Equals(item.Name, name, StringComparison.Ordinal));
        }

        private Point GetMapPlacementPosition(
            Point pointer, BitmapSource image, int rotationDegrees = 0)
        {
            double width = GetPlacementWidth(image, rotationDegrees);
            double height = GetPlacementHeight(image, rotationDegrees);
            double x = pointer.X - width / 2.0;
            double y = pointer.Y - height / 2.0;
            double wallThickness = ArenaField.WallThickness;
            double maximumX = _mapCanvas.Width - wallThickness - width;
            double maximumY = _mapCanvas.Height - wallThickness - height;
            if (_snapToGrid)
            {
                x = SnapGridCoordinate(
                    x, wallThickness, maximumX, MapGridSize);
                y = SnapGridCoordinate(
                    y, wallThickness, maximumY, MapGridSize);
            }
            else
            {
                x = ClampPlacementPosition(x, wallThickness, maximumX);
                y = ClampPlacementPosition(y, wallThickness, maximumY);
            }
            return new Point(x, y);
        }

        internal static double SnapGridCoordinate(double coordinate, double maximum)
        {
            return SnapGridCoordinate(coordinate, 0, maximum, MapGridSize);
        }

        internal static double SnapGridCoordinate(
            double coordinate, double minimum, double maximum, int gridSize)
        {
            if (gridSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gridSize));
            }

            double minimumGridCoordinate =
                Math.Ceiling(minimum / gridSize) * gridSize;
            double maximumGridCoordinate =
                Math.Floor(maximum / gridSize) * gridSize;
            if (maximumGridCoordinate < minimumGridCoordinate)
            {
                return minimum;
            }

            double snappedCoordinate =
                Math.Round(coordinate / gridSize) * gridSize;
            return ClampPlacementPosition(
                snappedCoordinate, minimumGridCoordinate, maximumGridCoordinate);
        }

        private void OnMapGridChanged(object sender, RoutedEventArgs e)
        {
            RedrawMap();
        }

        private void OnMapGridSnapButtonClick(object sender, RoutedEventArgs e)
        {
            _snapToGrid = !_snapToGrid;
            _mapGridSnapButton.Content = _snapToGrid
                ? "Einrasten: Ein"
                : "Einrasten: Aus";
        }

        private void PlaceRandomGrassBush()
        {
            ArenaMapElement bush = _elements.FirstOrDefault(
                element => String.Equals(
                    element.Name, "GrassBush", StringComparison.Ordinal));
            if (bush == null)
            {
                ShowError("Das Map-Element GrassBush wurde nicht gefunden.");
                return;
            }

            double width = bush.Image.PixelWidth;
            double height = bush.Image.PixelHeight;
            double minimumX = ArenaField.WallThickness;
            double minimumY = ArenaField.WallThickness;
            double maximumX = _mapCanvas.Width - ArenaField.WallThickness - width;
            double maximumY = _mapCanvas.Height - ArenaField.WallThickness - height;
            if (maximumX < minimumX || maximumY < minimumY)
            {
                SetStatus("Die Karte ist zu klein, um einen Busch zu platzieren.");
                return;
            }

            const int maximumAttempts = 200;
            for (int attempt = 0; attempt < maximumAttempts; attempt++)
            {
                double candidateX = minimumX
                    + Random.Shared.NextDouble() * (maximumX - minimumX);
                double candidateY = minimumY
                    + Random.Shared.NextDouble() * (maximumY - minimumY);
                Point position = GetMapPlacementPosition(
                    new Point(candidateX + width / 2.0, candidateY + height / 2.0),
                    bush.Image);
                Point candidateCenter = new Point(
                    position.X + width / 2.0, position.Y + height / 2.0);
                if (!IsBackgroundFreeCircle(candidateCenter))
                {
                    continue;
                }

                _placements.Add(new ArenaMapPlacement(
                    bush.Image, position.X, position.Y));
                RedrawMap();
                SetStatus("Busch zufällig auf freier Kartenfläche platziert.");
                return;
            }

            SetStatus("Kein freier Platz für einen Busch gefunden.");
        }

        private bool IsBackgroundFreeCircle(Point center)
        {
            int diameter = RandomBushFreeCheckRadius * 2 + 1;
            RenderTargetBitmap background = RenderMapRegion(center, false, diameter);
            RenderTargetBitmap currentMap = RenderMapRegion(center, true, diameter);
            int stride = diameter * 4;
            byte[] backgroundPixels = new byte[stride * diameter];
            byte[] mapPixels = new byte[stride * diameter];
            background.CopyPixels(backgroundPixels, stride, 0);
            currentMap.CopyPixels(mapPixels, stride, 0);

            int radiusSquared = RandomBushFreeCheckRadius
                * RandomBushFreeCheckRadius;
            for (int y = 0; y < diameter; y++)
            {
                int offsetY = y - RandomBushFreeCheckRadius;
                for (int x = 0; x < diameter; x++)
                {
                    int offsetX = x - RandomBushFreeCheckRadius;
                    if (offsetX * offsetX + offsetY * offsetY > radiusSquared)
                    {
                        continue;
                    }

                    int pixelOffset = (y * diameter + x) * 4;
                    Color backgroundColor = GetPixelColor(
                        backgroundPixels, pixelOffset);
                    Color mapColor = GetPixelColor(mapPixels, pixelOffset);
                    if (!PixelMatchesBackground(mapColor, backgroundColor))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private RenderTargetBitmap RenderMapRegion(
            Point center, bool includePlacements, int diameter)
        {
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext drawing = visual.RenderOpen())
            {
                drawing.PushTransform(new TranslateTransform(
                    RandomBushFreeCheckRadius - Math.Round(center.X),
                    RandomBushFreeCheckRadius - Math.Round(center.Y)));
                drawing.DrawImage(_baseImage,
                    new Rect(0, 0, _mapCanvas.Width, _mapCanvas.Height));
                if (includePlacements)
                {
                    foreach (ArenaMapPlacement placement in _placements)
                    {
                        DrawMapPlacement(drawing, placement, 1.0);
                    }
                    if (_pendingPlacement != null)
                    {
                        DrawMapPlacement(drawing, _pendingPlacement, 0.65);
                    }
                }
                drawing.Pop();
            }

            RenderTargetBitmap rendered = new RenderTargetBitmap(
                diameter, diameter, 96, 96, PixelFormats.Pbgra32);
            rendered.Render(visual);
            rendered.Freeze();
            return rendered;
        }

        private static void DrawMapPlacement(
            DrawingContext drawing, ArenaMapPlacement placement, double opacity)
        {
            double width = GetPlacementWidth(
                placement.Image, placement.RotationDegrees);
            double height = GetPlacementHeight(
                placement.Image, placement.RotationDegrees);
            drawing.PushTransform(new TranslateTransform(
                placement.X + width / 2.0,
                placement.Y + height / 2.0));
            drawing.PushTransform(new RotateTransform(placement.RotationDegrees));
            drawing.PushTransform(new ScaleTransform(
                placement.FlipHorizontal ? -1 : 1,
                placement.FlipVertical ? -1 : 1));
            if (opacity < 1.0)
            {
                drawing.PushOpacity(opacity);
            }
            drawing.DrawImage(placement.Image,
                new Rect(-placement.Image.PixelWidth / 2.0,
                    -placement.Image.PixelHeight / 2.0,
                    placement.Image.PixelWidth, placement.Image.PixelHeight));
            if (opacity < 1.0)
            {
                drawing.Pop();
            }
            drawing.Pop();
            drawing.Pop();
            drawing.Pop();
        }

        private static Color GetPixelColor(byte[] pixels, int offset)
        {
            return Color.FromArgb(
                pixels[offset + 3], pixels[offset + 2],
                pixels[offset + 1], pixels[offset]);
        }

        internal static bool PixelMatchesBackground(Color pixel, Color background)
        {
            return Math.Abs(pixel.A - background.A) <= BackgroundColorTolerance
                && Math.Abs(pixel.R - background.R) <= BackgroundColorTolerance
                && Math.Abs(pixel.G - background.G) <= BackgroundColorTolerance
                && Math.Abs(pixel.B - background.B) <= BackgroundColorTolerance;
        }

        private void ShowMapDragPreview(ArenaMapElement element, Point position)
        {
            if (_mapDragPreview == null
                || !ReferenceEquals(_mapDragPreview.Source, element.Image))
            {
                HideMapDragPreview();
                _mapDragPreview = new Image
                {
                    Source = element.Image,
                    Width = element.Image.PixelWidth,
                    Height = element.Image.PixelHeight,
                    Opacity = 0.6,
                    IsHitTestVisible = false,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };
                _mapCanvas.Children.Add(_mapDragPreview);
            }

            Canvas.SetLeft(_mapDragPreview, position.X);
            Canvas.SetTop(_mapDragPreview, position.Y);
            ApplyPlacementTransform(_mapDragPreview, 0, false, false);
        }

        private void OnMapKeyDown(object sender, KeyEventArgs e)
        {
            if (_pendingPlacement == null)
            {
                return;
            }

            switch (e.Key)
            {
                case Key.Enter:
                    CommitPendingPlacement(e);
                    e.Handled = true;
                    break;
                case Key.Escape:
                    _pendingPlacement = null;
                    UpdatePendingPlacementControls();
                    RedrawMap();
                    SetStatus("Vorschau verworfen.");
                    e.Handled = true;
                    break;
                case Key.R:
                    RotatePendingPlacement();
                    e.Handled = true;
                    break;
                case Key.H:
                    FlipPendingPlacement(true);
                    e.Handled = true;
                    break;
                case Key.V:
                    FlipPendingPlacement(false);
                    e.Handled = true;
                    break;
            }
        }

        private void OnMapEditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && _pendingPlacement != null)
            {
                CommitPendingPlacement(e);
            }
        }

        private void CommitPendingPlacement(KeyEventArgs e)
        {
            _placements.Add(_pendingPlacement);
            _pendingPlacement = null;
            UpdatePendingPlacementControls();
            RedrawMap();
            SetStatus("Element auf der Karte platziert.");
            e.Handled = true;
        }

        private void OnMapPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                return;
            }

            ChangeMapZoom(e.Delta > 0 ? 1.2 : 1.0 / 1.2);
            e.Handled = true;
        }

        private void ChangeMapZoom(double multiplier)
        {
            SetMapZoom(_mapZoom * multiplier);
        }

        private void SetMapZoom(double zoom)
        {
            _mapZoom = Math.Max(0.2, Math.Min(3.0, zoom));
            _mapZoomTransform.ScaleX = _mapZoom;
            _mapZoomTransform.ScaleY = _mapZoom;
            _mapZoomLabel.Text = Math.Round(_mapZoom * 100).ToString() + "%";
        }

        private void RotatePendingPlacement()
        {
            if (_pendingPlacement == null)
            {
                return;
            }

            double centerX = _pendingPlacement.X
                + GetPlacementWidth(_pendingPlacement.Image,
                    _pendingPlacement.RotationDegrees) / 2.0;
            double centerY = _pendingPlacement.Y
                + GetPlacementHeight(_pendingPlacement.Image,
                    _pendingPlacement.RotationDegrees) / 2.0;
            int rotation = (_pendingPlacement.RotationDegrees + 90) % 360;
            double width = GetPlacementWidth(_pendingPlacement.Image, rotation);
            double height = GetPlacementHeight(_pendingPlacement.Image, rotation);
            _pendingPlacement = new ArenaMapPlacement(
                _pendingPlacement.Image,
                ClampPlacementPosition(centerX - width / 2.0,
                    ArenaField.WallThickness,
                    _mapCanvas.Width - ArenaField.WallThickness - width),
                ClampPlacementPosition(centerY - height / 2.0,
                    ArenaField.WallThickness,
                    _mapCanvas.Height - ArenaField.WallThickness - height),
                rotation,
                _pendingPlacement.FlipHorizontal,
                _pendingPlacement.FlipVertical);
            RedrawMap();
            SetStatus("Vorschau gedreht. Enter: platzieren · Esc: verwerfen.");
        }

        private void FlipPendingPlacement(bool horizontal)
        {
            if (_pendingPlacement == null)
            {
                return;
            }

            _pendingPlacement = new ArenaMapPlacement(
                _pendingPlacement.Image, _pendingPlacement.X, _pendingPlacement.Y,
                _pendingPlacement.RotationDegrees,
                horizontal
                    ? !_pendingPlacement.FlipHorizontal
                    : _pendingPlacement.FlipHorizontal,
                horizontal
                    ? _pendingPlacement.FlipVertical
                    : !_pendingPlacement.FlipVertical);
            RedrawMap();
            SetStatus(horizontal
                ? "Vorschau horizontal gespiegelt."
                : "Vorschau vertikal gespiegelt.");
        }

        private void UpdatePendingPlacementControls()
        {
            bool hasPendingPlacement = _pendingPlacement != null;
            _rotatePendingButton.IsEnabled = hasPendingPlacement;
            _flipPendingHorizontalButton.IsEnabled = hasPendingPlacement;
            _flipPendingVerticalButton.IsEnabled = hasPendingPlacement;
        }

        private void ShowPendingPlacementPreview()
        {
            if (_pendingPlacement == null)
            {
                return;
            }

            Image image = CreatePlacementImage(_pendingPlacement, 0.65, true);
            image.PreviewMouseLeftButtonDown += OnPendingPlacementMouseDown;
            _mapCanvas.Children.Add(image);
            _mapDragPreview = image;
        }

        private static Image CreatePlacementImage(
            ArenaMapPlacement placement, double opacity,
            bool isPendingPlacement = false)
        {
            Image image = new Image
            {
                Source = placement.Image,
                Width = placement.Image.PixelWidth,
                Height = placement.Image.PixelHeight,
                Opacity = opacity,
                IsHitTestVisible = isPendingPlacement,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
            SetPlacementImagePosition(image, placement);
            ApplyPlacementTransform(image, placement.RotationDegrees,
                placement.FlipHorizontal, placement.FlipVertical);
            return image;
        }

        private static void SetPlacementImagePosition(
            Image image, ArenaMapPlacement placement)
        {
            Canvas.SetLeft(image, placement.X
                + GetPlacementWidth(placement.Image, placement.RotationDegrees) / 2.0
                - placement.Image.PixelWidth / 2.0);
            Canvas.SetTop(image, placement.Y
                + GetPlacementHeight(placement.Image, placement.RotationDegrees) / 2.0
                - placement.Image.PixelHeight / 2.0);
        }

        private static void ApplyPlacementTransform(
            Image image, int rotationDegrees, bool flipHorizontal,
            bool flipVertical)
        {
            image.RenderTransform = new TransformGroup
            {
                Children = new TransformCollection
                {
                    new ScaleTransform(
                        flipHorizontal ? -1 : 1, flipVertical ? -1 : 1),
                    new RotateTransform(rotationDegrees)
                }
            };
        }

        private static double GetPlacementWidth(
            BitmapSource image, int rotationDegrees)
        {
            return rotationDegrees % 180 == 0
                ? image.PixelWidth
                : image.PixelHeight;
        }

        private static double GetPlacementHeight(
            BitmapSource image, int rotationDegrees)
        {
            return rotationDegrees % 180 == 0
                ? image.PixelHeight
                : image.PixelWidth;
        }

        private static double ClampPlacementPosition(double position, double maximum)
        {
            return Math.Max(0, Math.Min(maximum, position));
        }

        private static double ClampPlacementPosition(
            double position, double minimum, double maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, position));
        }

        private void HideMapDragPreview()
        {
            if (_mapDragPreview != null)
            {
                _mapCanvas.Children.Remove(_mapDragPreview);
                _mapDragPreview = null;
            }
        }

        private void OnMapRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point point = e.GetPosition(_mapCanvas);
            for (int i = _placements.Count - 1; i >= 0; i--)
            {
                ArenaMapPlacement placement = _placements[i];
                if (point.X >= placement.X
                    && point.X < placement.X
                        + GetPlacementWidth(placement.Image, placement.RotationDegrees)
                    && point.Y >= placement.Y
                    && point.Y < placement.Y
                        + GetPlacementHeight(placement.Image, placement.RotationDegrees))
                {
                    _placements.RemoveAt(i);
                    RedrawMap();
                    e.Handled = true;
                    return;
                }
            }
        }

        private void RedrawMap()
        {
            HideMapDragPreview();
            _mapCanvas.Children.Clear();
            Image background = new Image
            {
                Source = _baseImage,
                Width = _mapCanvas.Width,
                Height = _mapCanvas.Height,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            };
            _mapCanvas.Children.Add(background);
            if (_mapGridCheckBox.IsChecked == true)
            {
                DrawMapGrid();
            }
            foreach (ArenaMapPlacement placement in _placements)
            {
                _mapCanvas.Children.Add(CreatePlacementImage(placement, 1.0));
            }
            ShowPendingPlacementPreview();
            DrawUnusableMapBorder();
        }

        private void DrawUnusableMapBorder()
        {
            double border = ArenaField.WallThickness;
            Brush mask = new SolidColorBrush(Color.FromRgb(16, 22, 30));
            AddMapMask(0, 0, _mapCanvas.Width, border, mask);
            AddMapMask(0, _mapCanvas.Height - border,
                _mapCanvas.Width, border, mask);
            AddMapMask(0, border, border,
                _mapCanvas.Height - 2 * border, mask);
            AddMapMask(_mapCanvas.Width - border, border, border,
                _mapCanvas.Height - 2 * border, mask);
            System.Windows.Shapes.Rectangle borderFrame =
                new System.Windows.Shapes.Rectangle
            {
                Width = _mapCanvas.Width - 2 * border,
                Height = _mapCanvas.Height - 2 * border,
                Stroke = new SolidColorBrush(Color.FromArgb(170, 220, 232, 244)),
                StrokeThickness = 1,
                IsHitTestVisible = false
            };
            _mapCanvas.Children.Add(borderFrame);
            Canvas.SetLeft(borderFrame, border);
            Canvas.SetTop(borderFrame, border);
        }

        private void AddMapMask(
            double x, double y, double width, double height, Brush mask)
        {
            _mapCanvas.Children.Add(new System.Windows.Shapes.Rectangle
            {
                Width = width,
                Height = height,
                Fill = mask,
                IsHitTestVisible = false
            });
            UIElement overlay = _mapCanvas.Children[_mapCanvas.Children.Count - 1];
            Canvas.SetLeft(overlay, x);
            Canvas.SetTop(overlay, y);
        }

        private void DrawMapGrid()
        {
            Brush minorGridBrush = new SolidColorBrush(
                Color.FromArgb(45, 220, 232, 244));
            Brush majorGridBrush = new SolidColorBrush(
                Color.FromArgb(100, 220, 232, 244));
            for (double x = MapGridSize; x < _mapCanvas.Width; x += MapGridSize)
            {
                bool isMajorLine = x % MajorMapGridSize == 0;
                _mapCanvas.Children.Add(new System.Windows.Shapes.Line
                {
                    X1 = x,
                    Y1 = 0,
                    X2 = x,
                    Y2 = _mapCanvas.Height,
                    Stroke = isMajorLine ? majorGridBrush : minorGridBrush,
                    StrokeThickness = isMajorLine ? 1.25 : 0.6,
                    IsHitTestVisible = false
                });
            }
            for (double y = MapGridSize; y < _mapCanvas.Height; y += MapGridSize)
            {
                bool isMajorLine = y % MajorMapGridSize == 0;
                _mapCanvas.Children.Add(new System.Windows.Shapes.Line
                {
                    X1 = 0,
                    Y1 = y,
                    X2 = _mapCanvas.Width,
                    Y2 = y,
                    Stroke = isMajorLine ? majorGridBrush : minorGridBrush,
                    StrokeThickness = isMajorLine ? 1.25 : 0.6,
                    IsHitTestVisible = false
                });
            }
        }

        private void SaveMap()
        {
            if (!Int32.TryParse(_widthInput.Text, out int width)
                || !Int32.TryParse(_heightInput.Text, out int height))
            {
                ShowError("Breite und Höhe müssen ganze Pixelwerte sein.");
                return;
            }
            try
            {
                ArenaMapAssets.ValidateDimensions(width, height);
                _mapCanvas.Width = width;
                _mapCanvas.Height = height;
                RedrawMap();
                string fileName = ArenaMapAssets.SaveMap(
                    _mapNameInput.Text, width, height, _baseImage, _placements);
                _mapSelection.ItemsSource = ArenaMapAssets.GetMapFileNames();
                _mapSelection.SelectedItem = fileName;
                SetStatus("Gespeichert: Assets\\Background\\" + fileName
                    + ". Wähle „Als Spielhintergrund verwenden“, um die Karte zu aktivieren."
                    + (_pendingPlacement == null
                        ? String.Empty
                        : " Die offene Vorschau wurde nicht gespeichert; Enter platziert sie."));
            }
            catch (ArgumentException exception)
            {
                ShowError(exception.Message);
            }
            catch (InvalidDataException exception)
            {
                ShowError(exception.Message);
            }
            catch (IOException exception)
            {
                ShowError(exception.Message);
            }
            catch (UnauthorizedAccessException exception)
            {
                ShowError(exception.Message);
            }
        }

        private void SelectMap()
        {
            if (!(_mapSelection.SelectedItem is string fileName))
            {
                ShowError("Wähle zuerst eine Karte aus.");
                return;
            }
            try
            {
                _field.SelectMap(fileName);
                _widthInput.Text = ((int)_field.WorldWidth).ToString();
                _heightInput.Text = ((int)_field.WorldHeight).ToString();
                SetStatus(_field.HasActiveGame
                    ? "Map übernommen. Der Arena-Lauf wurde neu gestartet und pausiert."
                    : "Aktiver Spielhintergrund: " + fileName);
            }
            catch (ArgumentException exception)
            {
                ShowError(exception.Message);
            }
            catch (IOException exception)
            {
                ShowError(exception.Message);
            }
            catch (InvalidOperationException exception)
            {
                ShowError(exception.Message);
            }
        }

        private void StartNewMap()
        {
            _placements.Clear();
            _pendingPlacement = null;
            UpdatePendingPlacementControls();
            _mapNameInput.Text = "NeueMap";
            _widthInput.Text = "1800";
            _heightInput.Text = "1200";
            _mapCanvas.Width = 1800;
            _mapCanvas.Height = 1200;
            RedrawMap();
            SetStatus("Leere Karte erstellt. Elemente aus der Palette per Drag-and-drop platzieren.");
        }

        private void SetStatus(string message)
        {
            _status.Text = message;
        }

        private void ShowError(string message)
        {
            _status.Text = message;
            MessageBox.Show(message, "Map Editor",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private static DataTemplate CreateElementTemplate()
        {
            DataTemplate template = new DataTemplate(typeof(ArenaMapElement));
            FrameworkElementFactory row =
                new FrameworkElementFactory(typeof(StackPanel));
            row.SetValue(StackPanel.OrientationProperty, Orientation.Horizontal);
            FrameworkElementFactory image = new FrameworkElementFactory(typeof(Image));
            image.SetBinding(Image.SourceProperty, new Binding(nameof(ArenaMapElement.Image)));
            image.SetValue(FrameworkElement.WidthProperty, 42.0);
            image.SetValue(FrameworkElement.HeightProperty, 42.0);
            image.SetValue(FrameworkElement.MarginProperty, new Thickness(2));
            row.AppendChild(image);
            FrameworkElementFactory label = new FrameworkElementFactory(typeof(TextBlock));
            label.SetBinding(TextBlock.TextProperty, new Binding(nameof(ArenaMapElement.Name)));
            label.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
            label.SetValue(TextBlock.MarginProperty, new Thickness(6, 0, 0, 0));
            label.SetValue(TextBlock.ForegroundProperty, Brushes.White);
            row.AppendChild(label);
            template.VisualTree = row;
            return template;
        }

        private static TextBox CreateTextBox(string text, double width)
        {
            return new TextBox
            {
                Text = text,
                Width = width,
                Margin = new Thickness(0, 0, 4, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
        }

        private static ArenaIntegerTextBox CreateIntegerTextBox(
            string text, double width)
        {
            return new ArenaIntegerTextBox
            {
                Text = text,
                Width = width,
                Margin = new Thickness(0, 0, 4, 0),
                VerticalContentAlignment = VerticalAlignment.Center
            };
        }

        private static Button CreateButton(string label)
        {
            return new Button
            {
                Content = label,
                Padding = new Thickness(8, 5, 8, 5),
                Margin = new Thickness(3),
                VerticalAlignment = VerticalAlignment.Center
            };
        }
    }

    internal sealed class ArenaIntegerTextBox : TextBox
    {
        private int? _selectionAnchor;

        public ArenaIntegerTextBox()
        {
            AddHandler(
                Keyboard.PreviewKeyDownEvent,
                new KeyEventHandler(OnPreviewKeyDown), true);
        }

        internal bool ProcessEditorKey(Key key, ModifierKeys modifiers)
        {
            bool control = (modifiers & ModifierKeys.Control) != 0;
            bool shift = (modifiers & ModifierKeys.Shift) != 0;
            if (control)
            {
                if (key == Key.A)
                {
                    SelectAll();
                    _selectionAnchor = null;
                    return true;
                }
                if (key == Key.C || key == Key.X)
                {
                    if (SelectionLength > 0)
                    {
                        Clipboard.SetText(SelectedText);
                        if (key == Key.X)
                        {
                            ReplaceSelection(String.Empty);
                        }
                    }
                    return true;
                }
                if (key == Key.V)
                {
                    string clipboardText = Clipboard.ContainsText()
                        ? Clipboard.GetText()
                        : String.Empty;
                    string digits = new string(
                        clipboardText.Where(character =>
                            character >= '0' && character <= '9').ToArray());
                    ReplaceSelection(digits);
                    return true;
                }
            }

            if (TryGetDigit(key, out char digit))
            {
                ReplaceSelection(digit.ToString());
                return true;
            }

            switch (key)
            {
                case Key.Back:
                    if (SelectionLength == 0 && SelectionStart > 0)
                    {
                        Select(SelectionStart - 1, 1);
                    }
                    ReplaceSelection(String.Empty);
                    return true;
                case Key.Delete:
                    if (SelectionLength == 0 && SelectionStart < Text.Length)
                    {
                        Select(SelectionStart, 1);
                    }
                    ReplaceSelection(String.Empty);
                    return true;
                case Key.Left:
                    MoveCaret(-1, shift);
                    return true;
                case Key.Right:
                    MoveCaret(1, shift);
                    return true;
                case Key.Home:
                    MoveCaretTo(0, shift);
                    return true;
                case Key.End:
                    MoveCaretTo(Text.Length, shift);
                    return true;
                default:
                    _selectionAnchor = null;
                    return false;
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ProcessEditorKey(e.Key, Keyboard.Modifiers))
            {
                e.Handled = true;
            }
        }

        private void ReplaceSelection(string value)
        {
            int insertionPoint = SelectionStart;
            SelectedText = value;
            CaretIndex = insertionPoint + value.Length;
            _selectionAnchor = null;
        }

        private void MoveCaret(int direction, bool extendSelection)
        {
            if (!extendSelection && SelectionLength > 0)
            {
                CaretIndex = direction < 0
                    ? SelectionStart
                    : SelectionStart + SelectionLength;
                _selectionAnchor = null;
                return;
            }

            int current = CaretIndex;
            int next = Math.Max(0, Math.Min(Text.Length, current + direction));
            MoveCaretTo(next, extendSelection);
        }

        private void MoveCaretTo(int position, bool extendSelection)
        {
            position = Math.Max(0, Math.Min(Text.Length, position));
            if (!extendSelection)
            {
                Select(position, 0);
                _selectionAnchor = null;
                return;
            }

            if (!_selectionAnchor.HasValue)
            {
                _selectionAnchor = CaretIndex;
            }
            Select(Math.Min(_selectionAnchor.Value, position),
                Math.Abs(_selectionAnchor.Value - position));
        }

        private static bool TryGetDigit(Key key, out char digit)
        {
            int keyValue = (int)key;
            if (keyValue >= (int)Key.D0 && keyValue <= (int)Key.D9)
            {
                digit = (char)('0' + keyValue - (int)Key.D0);
                return true;
            }
            if (keyValue >= (int)Key.NumPad0 && keyValue <= (int)Key.NumPad9)
            {
                digit = (char)('0' + keyValue - (int)Key.NumPad0);
                return true;
            }
            digit = '\0';
            return false;
        }
    }

    internal sealed class ArenaSkillTreeEditorControl : UserControl
    {
        private const double BoardWidth = ArenaSkillTreeVisuals.EditorBoardWidth;
        private const double BoardHeight = ArenaSkillTreeVisuals.EditorBoardHeight;
        private const string SkillDragFormat = "OOPGames.ArenaSkill";
        private const string SkillNodeDragFormat = "OOPGames.ArenaSkillNode";
        private readonly ArenaField _field;
        private readonly IReadOnlyList<ArenaSkillInfo> _skills;
        private readonly IReadOnlyList<ArenaSkillInfo> _paletteItems;
        private readonly ListBox _palette;
        private readonly Canvas _treeCanvas;
        private readonly ScrollViewer _treeScrollViewer;
        private readonly ComboBox _treeSelection;
        private readonly TextBox _treeNameInput;
        private readonly TextBlock _status;
        private readonly Button _connectionModeButton;
        private readonly Button _removeConnectionModeButton;
        private readonly CheckBox _forceForkDecisionCheckBox;
        private ArenaSkillTreeDocument _tree;
        private ArenaSkillInfo _draggedSkill;
        private Border _treeDragPreview;
        private string _treeDragPreviewSkillId;
        private string _treeDragPreviewNodeId;
        private string _treeName;
        private string _selectedSkillId;
        private string _draggedNodeId;
        private bool _nodeDragStarted;
        private bool _connectionMode;
        private bool _removeConnectionMode;
        private bool _suppressTreeSelectionChanged;
        private bool _suppressForkDecisionChanged;
        private Point _dragStart;

        public ArenaSkillTreeEditorControl(ArenaField field)
        {
            _field = field;
            _skills = ArenaSkillTreeRepository.GetImplementedSkills();
            _paletteItems = _skills.Concat(new[]
            {
                ArenaSkillTreeRepository.ForkPaletteInfo
            }).ToArray();
            _tree = ArenaSkillTreeRepository.LoadSelectedTree();
            _treeName = ArenaSkillTreeRepository.GetSelectedTreeName();

            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            StackPanel toolbar = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(4)
            };
            _treeSelection = new ComboBox
            {
                MinWidth = 180,
                ItemsSource = ArenaSkillTreeRepository.GetTreeNames(),
                SelectedItem = _treeName,
                Margin = new Thickness(0, 0, 8, 0)
            };
            _treeSelection.SelectionChanged += OnTreeSelected;
            toolbar.Children.Add(_treeSelection);
            _treeNameInput = new TextBox
            {
                Text = _treeName == ArenaSkillTreeRepository.DefaultTreeName
                    ? "MeinSkilltree" : _treeName,
                Width = 170,
                VerticalContentAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 4, 0)
            };
            toolbar.Children.Add(_treeNameInput);
            Button saveTree = CreateButton("Skilltree speichern");
            saveTree.Click += (sender, args) => SaveTree();
            toolbar.Children.Add(saveTree);
            _connectionModeButton = CreateButton("Verbindung hinzufügen");
            _connectionModeButton.Click += (sender, args) => SetConnectionMode(false);
            toolbar.Children.Add(_connectionModeButton);
            _removeConnectionModeButton = CreateButton("Verbindung entfernen");
            _removeConnectionModeButton.Click += (sender, args) => SetConnectionMode(true);
            toolbar.Children.Add(_removeConnectionModeButton);
            _forceForkDecisionCheckBox = new CheckBox
            {
                Content = "Abzweigung erzwingt Entscheidung",
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(8, 0, 8, 0),
                IsEnabled = false
            };
            _forceForkDecisionCheckBox.Checked += OnForkDecisionChanged;
            _forceForkDecisionCheckBox.Unchecked += OnForkDecisionChanged;
            toolbar.Children.Add(_forceForkDecisionCheckBox);
            Button removeNode = CreateButton("Ausgewählten Skill entfernen");
            removeNode.Click += (sender, args) => RemoveSelectedNode();
            toolbar.Children.Add(removeNode);
            Button newTree = CreateButton("Neuer Skilltree");
            newTree.Click += (sender, args) => StartNewTree();
            toolbar.Children.Add(newTree);
            root.Children.Add(toolbar);

            Grid workspace = new Grid();
            workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(240) });
            workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _palette = new ListBox
            {
                ItemsSource = _paletteItems,
                ItemTemplate = CreateSkillTemplate(),
                Margin = new Thickness(4),
                Background = new SolidColorBrush(Color.FromRgb(30, 40, 52))
            };
            _palette.PreviewMouseLeftButtonDown += OnPaletteMouseDown;
            _palette.PreviewMouseMove += OnPaletteMouseMove;
            _palette.PreviewMouseLeftButtonUp += OnPaletteMouseLeftButtonUp;
            workspace.Children.Add(_palette);

            _treeCanvas = new Canvas
            {
                Width = BoardWidth,
                Height = BoardHeight,
                Background = new SolidColorBrush(Color.FromRgb(13, 19, 27)),
                AllowDrop = true
            };
            _treeCanvas.DragEnter += OnCanvasDragOver;
            _treeCanvas.DragOver += OnCanvasDragOver;
            _treeCanvas.DragLeave += OnCanvasDragLeave;
            _treeCanvas.Drop += OnCanvasDrop;
            _treeScrollViewer = new ScrollViewer
            {
                Content = _treeCanvas,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(4),
                Background = new SolidColorBrush(Color.FromRgb(17, 23, 31))
            };
            _treeScrollViewer.Loaded += (sender, args) => CenterTreeBoard();
            Grid.SetColumn(_treeScrollViewer, 1);
            workspace.Children.Add(_treeScrollViewer);
            Grid.SetRow(workspace, 1);
            root.Children.Add(workspace);

            _status = new TextBlock
            {
                Text = "Kacheln per Drag-and-drop verschieben. Rechtsklick: Rangstufen und Abzweigungsoptionen.",
                Foreground = new SolidColorBrush(Color.FromRgb(183, 197, 212)),
                Margin = new Thickness(6, 3, 0, 0)
            };
            Grid.SetRow(_status, 2);
            root.Children.Add(_status);

            Content = root;
            RedrawTree();
        }

        private void OnTreeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressTreeSelectionChanged
                || !(_treeSelection.SelectedItem is string name))
            {
                return;
            }
            try
            {
                _tree = ArenaSkillTreeRepository.LoadTreeByName(name);
                _treeName = name;
                _selectedSkillId = null;
                _treeNameInput.Text = name == ArenaSkillTreeRepository.DefaultTreeName
                    ? "MeinSkilltree" : name;
                _field.SelectSkillTree(name);
                RedrawTree();
                SetStatus(_field.HasActiveGame
                    ? "Skilltree übernommen. Der Arena-Lauf wurde neu gestartet und pausiert."
                    : "Aktiver Skilltree: " + name);
            }
            catch (ArgumentException exception)
            {
                ShowError(exception.Message);
            }
            catch (InvalidDataException exception)
            {
                ShowError(exception.Message);
            }
            catch (IOException exception)
            {
                ShowError(exception.Message);
            }
        }

        private void OnPaletteMouseDown(object sender, MouseButtonEventArgs e)
        {
            _dragStart = e.GetPosition(_palette);
            ListBoxItem item = ItemsControl.ContainerFromElement(
                _palette, e.OriginalSource as DependencyObject) as ListBoxItem;
            _draggedSkill = item == null ? null : item.DataContext as ArenaSkillInfo;
        }

        private void OnPaletteMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed
                || _draggedSkill == null)
            {
                return;
            }
            Point current = e.GetPosition(_palette);
            if (Math.Abs(current.X - _dragStart.X) < SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(current.Y - _dragStart.Y) < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }
            DataObject data = new DataObject(SkillDragFormat, _draggedSkill.Id);
            DragDrop.DoDragDrop(_palette, data, DragDropEffects.Copy);
            _draggedSkill = null;
        }

        private void OnPaletteMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _draggedSkill = null;
        }

        private void OnCanvasDragOver(object sender, DragEventArgs e)
        {
            string nodeId = GetDraggedNodeId(e.Data);
            if (nodeId != null)
            {
                ArenaSkillTreeNode node = _tree.Nodes.FirstOrDefault(
                    item => ArenaSkillTreeRepository.GetNodeId(item) == nodeId);
                if (node == null)
                {
                    HideTreeDragPreview();
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
                }
                ShowTreeNodeDragPreview(
                    node, GetNodePosition(e.GetPosition(_treeCanvas)));
                e.Effects = DragDropEffects.Move;
                e.Handled = true;
                return;
            }

            ArenaSkillInfo skill = GetDraggedSkill(e.Data);
            if (skill == null || !CanPlaceSkill(skill))
            {
                HideTreeDragPreview();
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            Point position = GetNodePosition(e.GetPosition(_treeCanvas));
            ShowTreeDragPreview(skill, position);
            e.Effects = DragDropEffects.Copy;
            e.Handled = true;
        }

        private void OnCanvasDragLeave(object sender, DragEventArgs e)
        {
            HideTreeDragPreview();
        }

        private void OnCanvasDrop(object sender, DragEventArgs e)
        {
            string nodeId = GetDraggedNodeId(e.Data);
            if (nodeId != null)
            {
                ArenaSkillTreeNode node = _tree.Nodes.FirstOrDefault(
                    item => ArenaSkillTreeRepository.GetNodeId(item) == nodeId);
                if (node != null)
                {
                    Point nodePosition =
                        GetNodePosition(e.GetPosition(_treeCanvas));
                    node.X = nodePosition.X;
                    node.Y = nodePosition.Y;
                    _selectedSkillId = nodeId;
                    RedrawTree();
                    SetStatus("Kachel verschoben. Die Rangreihenfolge bleibt erhalten.");
                }
                HideTreeDragPreview();
                e.Handled = true;
                return;
            }

            ArenaSkillInfo skill = GetDraggedSkill(e.Data);
            if (skill == null)
            {
                return;
            }
            HideTreeDragPreview();
            if (!CanPlaceSkill(skill))
            {
                SetStatus("Für diesen Skill ist kein weiteres Rangbudget verfügbar.");
                return;
            }

            Point position = GetNodePosition(e.GetPosition(_treeCanvas));
            bool isFork = skill.Id == ArenaSkillTreeRepository.ForkPaletteId;
            int rankStages = isFork
                ? 0
                : ArenaSkillTreeRepository.GetMaximumRank(skill.Id)
                    - _tree.Nodes.Where(node => !node.IsFork
                        && node.SkillId == skill.Id)
                        .Sum(ArenaSkillTreeRepository.GetRankStages);
            ArenaSkillTreeNode newNode = new ArenaSkillTreeNode
            {
                SkillId = isFork
                    ? ArenaSkillTreeRepository.CreateForkNodeId()
                    : skill.Id,
                NodeId = isFork
                    ? null
                    : ArenaSkillTreeRepository.CreateSkillNodeId(),
                RankStages = rankStages,
                X = position.X,
                Y = position.Y,
                IsFork = isFork
            };
            _tree.Nodes.Add(newNode);
            if (isFork)
            {
                _selectedSkillId = ArenaSkillTreeRepository.GetNodeId(newNode);
                UpdateForkDecisionOption();
            }
            RedrawTree();
            SetStatus(isFork
                ? "Abzweigung platziert. Verbinde einen Vorgänger mit ihr und anschließend die Ausgänge mit Skills."
                : "Skill platziert. Klicke zwei Knoten nacheinander, um eine Verbindung zu zeichnen.");
            e.Handled = true;
        }

        private ArenaSkillInfo GetDraggedSkill(IDataObject data)
        {
            if (!data.GetDataPresent(SkillDragFormat)
                || !(data.GetData(SkillDragFormat) is string skillId))
            {
                return null;
            }
            return _paletteItems.FirstOrDefault(skill => skill.Id == skillId);
        }

        private string GetDraggedNodeId(IDataObject data)
        {
            return data.GetDataPresent(SkillNodeDragFormat)
                ? data.GetData(SkillNodeDragFormat) as string
                : null;
        }

        private bool CanPlaceSkill(ArenaSkillInfo skill)
        {
            if (skill.Id == ArenaSkillTreeRepository.ForkPaletteId)
            {
                return true;
            }
            List<ArenaSkillTreeNode> existingNodes = _tree.Nodes
                .Where(node => !node.IsFork && node.SkillId == skill.Id)
                .ToList();
            if (existingNodes.Count == 0)
            {
                return true;
            }
            int maximumRank = ArenaSkillTreeRepository.GetMaximumRank(skill.Id);
            int allocatedStages = existingNodes.Sum(
                ArenaSkillTreeRepository.GetRankStages);
            return maximumRank > 1 && allocatedStages < maximumRank;
        }

        private static Point GetNodePosition(Point pointer)
        {
            double x = Math.Max(0, Math.Min(
                BoardWidth - ArenaSkillTreeVisuals.NodeWidth,
                pointer.X - ArenaSkillTreeVisuals.NodeWidth / 2));
            double y = Math.Max(0, Math.Min(
                BoardHeight - ArenaSkillTreeVisuals.NodeHeight,
                pointer.Y - ArenaSkillTreeVisuals.NodeHeight / 2));
            return new Point(x, y);
        }

        private void ShowTreeDragPreview(ArenaSkillInfo skill, Point position)
        {
            if (_treeDragPreview == null
                || !String.Equals(
                    _treeDragPreviewSkillId, skill.Id, StringComparison.Ordinal))
            {
                HideTreeDragPreview();
                _treeDragPreview = skill.Id == ArenaSkillTreeRepository.ForkPaletteId
                    ? ArenaSkillTreeVisuals.CreateForkTile(false)
                    : ArenaSkillTreeVisuals.CreateNodeTile(
                        skill.Name, "Skill", ArenaSkillTreeTileState.Available);
                _treeDragPreview.Opacity = 0.6;
                _treeDragPreview.IsHitTestVisible = false;
                _treeDragPreviewSkillId = skill.Id;
                _treeCanvas.Children.Add(_treeDragPreview);
            }

            Canvas.SetLeft(_treeDragPreview, position.X);
            Canvas.SetTop(_treeDragPreview, position.Y);
        }

        private void ShowTreeNodeDragPreview(
            ArenaSkillTreeNode node, Point position)
        {
            string nodeId = ArenaSkillTreeRepository.GetNodeId(node);
            if (_treeDragPreview == null || _treeDragPreviewNodeId != nodeId)
            {
                HideTreeDragPreview();
                ArenaSkillInfo info = GetSkill(nodeId);
                _treeDragPreview = node.IsFork
                    ? ArenaSkillTreeVisuals.CreateForkTile(node.ForceDecision)
                    : ArenaSkillTreeVisuals.CreateNodeTile(
                        info.Name, "Verschieben", ArenaSkillTreeTileState.Available);
                _treeDragPreview.Opacity = 0.6;
                _treeDragPreview.IsHitTestVisible = false;
                _treeDragPreviewNodeId = nodeId;
                _treeCanvas.Children.Add(_treeDragPreview);
            }
            Canvas.SetLeft(_treeDragPreview, position.X);
            Canvas.SetTop(_treeDragPreview, position.Y);
        }

        private void HideTreeDragPreview()
        {
            if (_treeDragPreview != null)
            {
                _treeCanvas.Children.Remove(_treeDragPreview);
                _treeDragPreview = null;
                _treeDragPreviewSkillId = null;
                _treeDragPreviewNodeId = null;
            }
        }

        private void OnNodeClicked(string nodeId)
        {
            _selectedSkillId = nodeId;
            if (!_connectionMode)
            {
                UpdateForkDecisionOption();
                SetStatus("Ausgewählt: " + GetSkill(nodeId).Name);
                RedrawTree();
                return;
            }
            if (String.IsNullOrEmpty(_pendingConnectionStart))
            {
                _pendingConnectionStart = nodeId;
                SetStatus("Verbindungsquelle: " + GetSkill(nodeId).Name
                    + ". Klicke jetzt den Skill, der dadurch freigeschaltet werden soll.");
                RedrawTree();
                return;
            }
            if (String.Equals(_pendingConnectionStart, nodeId, StringComparison.Ordinal))
            {
                _pendingConnectionStart = null;
                SetStatus("Verbindung abgebrochen.");
                RedrawTree();
                return;
            }

            if (_removeConnectionMode)
            {
                ArenaSkillTreeConnection existing = _tree.Connections.FirstOrDefault(
                    connection => connection.FromSkillId == _pendingConnectionStart
                        && connection.ToSkillId == nodeId);
                if (existing != null)
                {
                    _tree.Connections.Remove(existing);
                    SetStatus("Freischalt-Verbindung entfernt.");
                }
                else
                {
                    SetStatus("Zwischen diesen Skills besteht keine Verbindung.");
                }
            }
            else
            {
                _tree.Connections.Add(new ArenaSkillTreeConnection
                {
                    FromSkillId = _pendingConnectionStart,
                    ToSkillId = nodeId
                });
                try
                {
                    ArenaSkillTreeRepository.ValidateTree(_tree);
                    SetStatus("Freischalt-Verbindung hinzugefügt.");
                }
                catch (InvalidDataException exception)
                {
                    _tree.Connections.RemoveAt(_tree.Connections.Count - 1);
                    ShowError(exception.Message);
                }
            }
            _pendingConnectionStart = null;
            RedrawTree();
        }

        private void OnForkDecisionChanged(object sender, RoutedEventArgs e)
        {
            if (_suppressForkDecisionChanged
                || String.IsNullOrEmpty(_selectedSkillId))
            {
                return;
            }

            ArenaSkillTreeNode fork = _tree.Nodes.FirstOrDefault(
                node => node.IsFork
                    && ArenaSkillTreeRepository.GetNodeId(node) == _selectedSkillId);
            if (fork == null)
            {
                return;
            }

            fork.ForceDecision = _forceForkDecisionCheckBox.IsChecked == true;
            RedrawTree();
            SetStatus(fork.ForceDecision
                ? "Entscheidung aktiv: Nach dem Kauf eines Ausgangs werden die anderen gesperrt."
                : "Freie Wahl: Mehrere Ausgänge dieser Abzweigung können freigeschaltet werden.");
        }

        private void UpdateForkDecisionOption()
        {
            ArenaSkillTreeNode fork = _tree.Nodes.FirstOrDefault(
                node => node.IsFork
                    && ArenaSkillTreeRepository.GetNodeId(node) == _selectedSkillId);
            _suppressForkDecisionChanged = true;
            try
            {
                _forceForkDecisionCheckBox.IsEnabled = fork != null;
                _forceForkDecisionCheckBox.IsChecked =
                    fork != null && fork.ForceDecision;
            }
            finally
            {
                _suppressForkDecisionChanged = false;
            }
        }

        private string _pendingConnectionStart;

        private void RedrawTree()
        {
            HideTreeDragPreview();
            _treeCanvas.Children.Clear();
            ArenaSkillTreeVisuals.DrawConnections(
                _treeCanvas, _tree,
                connection => ArenaSkillTreeRepository.IsStartNode(
                    connection.FromSkillId) ? Brushes.Gold : Brushes.DeepSkyBlue);

            foreach (ArenaSkillTreeNode node in _tree.Nodes)
            {
                string nodeId = ArenaSkillTreeRepository.GetNodeId(node);
                bool isStartNode = ArenaSkillTreeRepository.IsStartNode(node.SkillId);
                bool isFork = node.IsFork;
                ArenaSkillInfo skill = GetSkill(nodeId);
                Border tile = isStartNode
                    ? ArenaSkillTreeVisuals.CreateNodeTile(
                        skill.Name, "Baumstart", ArenaSkillTreeTileState.Start)
                    : isFork
                        ? ArenaSkillTreeVisuals.CreateForkTile(node.ForceDecision)
                        : ArenaSkillTreeVisuals.CreateNodeTile(
                            skill.Name, "Fähigkeit", ArenaSkillTreeTileState.Available);
                tile.Tag = nodeId;
                tile.ToolTip = skill.Description;
                tile.Cursor = Cursors.Hand;
                tile.PreviewMouseLeftButtonDown += (sender, args) =>
                {
                    _draggedNodeId = (string)((FrameworkElement)sender).Tag;
                    _dragStart = args.GetPosition(_treeCanvas);
                    _nodeDragStarted = false;
                    args.Handled = true;
                };
                tile.MouseMove += OnTreeNodeMouseMove;
                tile.PreviewMouseLeftButtonUp += (sender, args) =>
                {
                    if (_draggedNodeId == nodeId)
                    {
                        if (!_nodeDragStarted)
                        {
                            OnNodeClicked(nodeId);
                        }
                        _draggedNodeId = null;
                        _nodeDragStarted = false;
                        args.Handled = true;
                    }
                };
                if (!isStartNode)
                {
                    tile.ContextMenu = CreateNodeContextMenu(node);
                }
                Canvas.SetLeft(tile, node.X);
                Canvas.SetTop(tile, node.Y);
                _treeCanvas.Children.Add(tile);
            }
            UpdateForkDecisionOption();
        }

        private void OnTreeNodeMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed
                || String.IsNullOrEmpty(_draggedNodeId)
                || _nodeDragStarted)
            {
                return;
            }
            Point current = e.GetPosition(_treeCanvas);
            if (Math.Abs(current.X - _dragStart.X)
                    < SystemParameters.MinimumHorizontalDragDistance
                && Math.Abs(current.Y - _dragStart.Y)
                    < SystemParameters.MinimumVerticalDragDistance)
            {
                return;
            }
            _nodeDragStarted = true;
            DataObject data = new DataObject(
                SkillNodeDragFormat, _draggedNodeId);
            DragDrop.DoDragDrop(
                _treeCanvas, data, DragDropEffects.Move);
            _draggedNodeId = null;
            _nodeDragStarted = false;
            e.Handled = true;
        }

        private ContextMenu CreateNodeContextMenu(ArenaSkillTreeNode node)
        {
            ContextMenu menu = new ContextMenu();
            if (node.IsFork)
            {
                MenuItem forceDecision = new MenuItem
                {
                    Header = "Auswahl erzwingen",
                    IsCheckable = true,
                    IsChecked = node.ForceDecision
                };
                forceDecision.Click += (sender, args) =>
                {
                    node.ForceDecision = forceDecision.IsChecked;
                    _selectedSkillId =
                        ArenaSkillTreeRepository.GetNodeId(node);
                    UpdateForkDecisionOption();
                    RedrawTree();
                };
                menu.Items.Add(forceDecision);

                int inputCount = ArenaSkillTreeRepository.GetForkInputCount(
                    _tree, ArenaSkillTreeRepository.GetNodeId(node));
                MenuItem requiredInputs = new MenuItem
                {
                    Header = "Benötigte Eingänge"
                };
                MenuItem allInputs = new MenuItem
                {
                    Header = "Alle verbundenen Eingänge",
                    IsCheckable = true,
                    IsChecked = node.RequiredInputs == 0
                };
                allInputs.Click += (sender, args) =>
                    SetForkRequiredInputs(node, 0);
                requiredInputs.Items.Add(allInputs);

                int maximumInputRequirement =
                    Math.Max(1, Math.Max(inputCount, node.RequiredInputs));
                for (int count = 1; count <= maximumInputRequirement; count++)
                {
                    int selectedCount = count;
                    MenuItem option = new MenuItem
                    {
                        Header = count.ToString(),
                        IsCheckable = true,
                        IsChecked = node.RequiredInputs == count
                    };
                    option.Click += (sender, args) =>
                        SetForkRequiredInputs(node, selectedCount);
                    requiredInputs.Items.Add(option);
                }
                menu.Items.Add(requiredInputs);
            }
            else
            {
                int maximumRank =
                    ArenaSkillTreeRepository.GetMaximumRank(node.SkillId);
                int allocatedByOtherNodes = _tree.Nodes
                    .Where(other => !other.IsFork
                        && other.SkillId == node.SkillId
                        && !ReferenceEquals(other, node))
                    .Sum(ArenaSkillTreeRepository.GetRankStages);
                int maximumForThisNode = maximumRank - allocatedByOtherNodes;
                MenuItem rankStages = new MenuItem
                {
                    Header = "Rangstufen dieser Kachel"
                };
                for (int count = 1; count <= maximumForThisNode; count++)
                {
                    int selectedCount = count;
                    MenuItem option = new MenuItem
                    {
                        Header = count.ToString(),
                        IsCheckable = true,
                        IsChecked = ArenaSkillTreeRepository.GetRankStages(node)
                            == count
                    };
                    option.Click += (sender, args) =>
                    {
                        node.RankStages = selectedCount;
                        RedrawTree();
                        SetStatus("Kachel reserviert jetzt "
                            + selectedCount + " Rangstufe(n).");
                    };
                    rankStages.Items.Add(option);
                }
                menu.Items.Add(rankStages);
            }
            return menu;
        }

        private void SetForkRequiredInputs(
            ArenaSkillTreeNode node, int requiredInputs)
        {
            node.RequiredInputs = requiredInputs;
            RedrawTree();
            SetStatus(requiredInputs == 0
                ? "Die Abzweigung wartet auf alle verbundenen Eingänge."
                : "Die Abzweigung benötigt " + requiredInputs
                    + " aktive Eingänge.");
        }

        private void SetConnectionMode(bool removeConnection)
        {
            if (_connectionMode && _removeConnectionMode == removeConnection)
            {
                _connectionMode = false;
                _pendingConnectionStart = null;
                _connectionModeButton.Content = "Verbindung hinzufügen";
                _removeConnectionModeButton.Content = "Verbindung entfernen";
                SetStatus("Verbindungsmodus beendet.");
                RedrawTree();
                return;
            }
            _connectionMode = true;
            _removeConnectionMode = removeConnection;
            _pendingConnectionStart = null;
            _connectionModeButton.Content = removeConnection
                ? "Verbindung hinzufügen"
                : "Verbindung hinzufügen (aktiv)";
            _removeConnectionModeButton.Content = removeConnection
                ? "Verbindung entfernen (aktiv)"
                : "Verbindung entfernen";
            SetStatus("Klicke zuerst den Voraussetzung-Skill, dann den davon abhängigen Skill.");
        }

        private void RemoveSelectedNode()
        {
            if (String.IsNullOrEmpty(_selectedSkillId))
            {
                SetStatus("Wähle zuerst einen Skill auf der Fläche aus.");
                return;
            }
            if (ArenaSkillTreeRepository.IsStartNode(_selectedSkillId))
            {
                SetStatus("Die Startkachel kann nicht entfernt werden.");
                return;
            }
            string removedNodeName = GetSkill(_selectedSkillId).Name;
            _tree.Nodes.RemoveAll(node =>
                ArenaSkillTreeRepository.GetNodeId(node) == _selectedSkillId);
            _tree.Connections.RemoveAll(connection =>
                connection.FromSkillId == _selectedSkillId
                || connection.ToSkillId == _selectedSkillId);
            SetStatus(removedNodeName + " entfernt.");
            _selectedSkillId = null;
            RedrawTree();
        }

        private void StartNewTree()
        {
            _tree = new ArenaSkillTreeDocument
            {
                Name = "MeinSkilltree",
                Nodes = new List<ArenaSkillTreeNode>
                {
                    new ArenaSkillTreeNode
                    {
                        SkillId = ArenaSkillTreeRepository.StartNodeId,
                        X = (BoardWidth - ArenaSkillTreeVisuals.NodeWidth) / 2,
                        Y = (BoardHeight - ArenaSkillTreeVisuals.NodeHeight) / 2
                    }
                },
                Connections = new List<ArenaSkillTreeConnection>()
            };
            _treeName = _tree.Name;
            _treeNameInput.Text = _treeName;
            _selectedSkillId = null;
            _pendingConnectionStart = null;
            RedrawTree();
            CenterTreeBoard();
            SetStatus("Neuer Skilltree mit Startkachel. Verbinde den Start mit den zuerst freischaltbaren Skills.");
        }

        private void CenterTreeBoard()
        {
            _treeScrollViewer.ScrollToHorizontalOffset(
                Math.Max(0, (BoardWidth - _treeScrollViewer.ViewportWidth) / 2));
            _treeScrollViewer.ScrollToVerticalOffset(
                Math.Max(0, (BoardHeight - _treeScrollViewer.ViewportHeight) / 2));
        }

        private void SaveTree()
        {
            string name = _treeNameInput.Text.Trim();
            try
            {
                ArenaSkillTreeRepository.ValidateTree(_tree);
                if (ArenaSkillTreeRepository.GetTreeNames().Any(
                    existing => String.Equals(existing, name,
                        StringComparison.OrdinalIgnoreCase))
                    && !String.Equals(name, _treeName, StringComparison.OrdinalIgnoreCase)
                    && !ConfirmOverwrite(name))
                {
                    return;
                }
                ArenaSkillTreeRepository.SaveTree(name, _tree);
                _field.SelectSkillTree(name);
                _treeName = name;
                _suppressTreeSelectionChanged = true;
                try
                {
                    _treeSelection.ItemsSource = ArenaSkillTreeRepository.GetTreeNames();
                    _treeSelection.SelectedItem = name;
                }
                finally
                {
                    _suppressTreeSelectionChanged = false;
                }
                SetStatus(_field.HasActiveGame
                    ? "Gespeichert und übernommen. Der Arena-Lauf wurde neu gestartet und pausiert."
                    : "Gespeichert und im Spiel ausgewählt: " + name);
            }
            catch (ArgumentException exception)
            {
                ShowError(exception.Message);
            }
            catch (InvalidDataException exception)
            {
                ShowError(exception.Message);
            }
            catch (IOException exception)
            {
                ShowError(exception.Message);
            }
            catch (UnauthorizedAccessException exception)
            {
                ShowError(exception.Message);
            }
        }

        private bool ConfirmOverwrite(string name)
        {
            return MessageBox.Show(
                "Der Skilltree „" + name + "“ existiert bereits. Überschreiben?",
                "Skilltree speichern",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        private ArenaSkillInfo GetSkill(string skillId)
        {
            ArenaSkillTreeNode node = _tree.Nodes.FirstOrDefault(
                item => ArenaSkillTreeRepository.GetNodeId(item) == skillId);
            if (node == null)
            {
                throw new InvalidDataException(
                    "Der Skilltree enthält eine unbekannte Kachel.");
            }
            if (ArenaSkillTreeRepository.IsStartNode(node.SkillId))
            {
                return ArenaSkillTreeRepository.StartNodeInfo;
            }
            if (node.IsFork)
            {
                return ArenaSkillTreeRepository.GetForkNodeInfo(
                    node.ForceDecision,
                    node.RequiredInputs,
                    ArenaSkillTreeRepository.GetForkInputCount(
                        _tree, ArenaSkillTreeRepository.GetNodeId(node)));
            }
            ArenaSkillInfo skill =
                _skills.FirstOrDefault(item => item.Id == node.SkillId);
            if (skill == null)
            {
                throw new InvalidDataException("Der Skilltree enthält einen unbekannten Skill.");
            }
            return skill;
        }

        private void SetStatus(string message)
        {
            _status.Text = message;
        }

        private void ShowError(string message)
        {
            _status.Text = message;
            MessageBox.Show(message, "Skilltree Editor",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private static DataTemplate CreateSkillTemplate()
        {
            DataTemplate template = new DataTemplate(typeof(ArenaSkillInfo));
            FrameworkElementFactory stack =
                new FrameworkElementFactory(typeof(StackPanel));
            FrameworkElementFactory title = new FrameworkElementFactory(typeof(TextBlock));
            title.SetBinding(TextBlock.TextProperty, new Binding(nameof(ArenaSkillInfo.Name)));
            title.SetValue(TextBlock.FontWeightProperty, FontWeights.Bold);
            title.SetValue(TextBlock.ForegroundProperty, Brushes.White);
            stack.AppendChild(title);
            FrameworkElementFactory description =
                new FrameworkElementFactory(typeof(TextBlock));
            description.SetBinding(
                TextBlock.TextProperty, new Binding(nameof(ArenaSkillInfo.Description)));
            description.SetValue(TextBlock.ForegroundProperty,
                new SolidColorBrush(Color.FromRgb(183, 197, 212)));
            description.SetValue(TextBlock.TextWrappingProperty, TextWrapping.Wrap);
            stack.AppendChild(description);
            template.VisualTree = stack;
            return template;
        }

        private static Button CreateButton(string label)
        {
            return new Button
            {
                Content = label,
                Padding = new Thickness(8, 5, 8, 5),
                Margin = new Thickness(3),
                VerticalAlignment = VerticalAlignment.Center
            };
        }
    }
}
