using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // Dev tool tab "Spawn-Regionen": draws named rectangles and circles on an existing map
    // and saves them as <map>.regions.json next to the map. Enemy kinds choose region names
    // in the enemy editor. Left drag on free ground draws a new shape, left drag on a shape
    // moves it, right click deletes it.
    internal sealed class ArenaRegionEditorControl : UserControl
    {
        private static readonly CultureInfo GermanCulture = CultureInfo.GetCultureInfo("de-DE");
        private static readonly Brush TextBrush = new SolidColorBrush(Color.FromRgb(183, 197, 212));
        private static readonly Brush ErrorBrush = new SolidColorBrush(Color.FromRgb(255, 120, 120));
        private static readonly Brush ShapeStroke = new SolidColorBrush(Color.FromRgb(120, 220, 255));
        private static readonly Brush ShapeFill = new SolidColorBrush(Color.FromArgb(50, 120, 220, 255));
        private static readonly Brush SelectedStroke = new SolidColorBrush(Color.FromRgb(255, 214, 102));
        private static readonly Brush SelectedFill = new SolidColorBrush(Color.FromArgb(70, 255, 214, 102));
        // Shapes smaller than this (world units) are treated as an accidental click.
        private const double MinimumShapeSize = 8;
        private const double MinimumZoom = 0.15;
        private const double MaximumZoom = 2;

        private readonly ArenaField _field;
        private readonly Action _regionsSaved;
        private readonly ComboBox _mapSelection;
        private readonly RadioButton _circleTool;
        private readonly TextBox _nameInput;
        private readonly Canvas _mapCanvas;
        private readonly ScaleTransform _zoomTransform = new ScaleTransform(0.5, 0.5);
        private readonly TextBlock _zoomLabel;
        private readonly ListBox _regionList;
        private readonly TextBlock _status;
        private readonly List<ArenaMapRegion> _regions = new List<ArenaMapRegion>();
        private string _mapFileName;
        private int _selected = -1;
        private bool _dirty;
        private bool _suppressListSelection;
        private DragMode _dragMode;
        private Point _dragStart;
        private ArenaMapRegion _dragOriginal;
        private ArenaMapRegion _drawing;

        private enum DragMode
        {
            None,
            Create,
            Move
        }

        public ArenaRegionEditorControl(ArenaField field, Action regionsSaved)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _regionsSaved = regionsSaved;

            Grid root = new Grid();
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            WrapPanel toolbar = new WrapPanel { Margin = new Thickness(4) };
            toolbar.Children.Add(CreateLabel("Map:"));
            _mapSelection = new ComboBox
            {
                MinWidth = 200,
                ItemsSource = ArenaMapAssets.GetMapFileNames(),
                Margin = new Thickness(0, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            _mapSelection.SelectionChanged += OnMapSelected;
            toolbar.Children.Add(_mapSelection);
            toolbar.Children.Add(CreateLabel("Form:"));
            RadioButton rectangleTool = new RadioButton
            {
                Content = "Rechteck",
                IsChecked = true,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 8, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            toolbar.Children.Add(rectangleTool);
            _circleTool = new RadioButton
            {
                Content = "Kreis",
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 12, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            toolbar.Children.Add(_circleTool);
            toolbar.Children.Add(CreateLabel("Name:"));
            _nameInput = new TextBox
            {
                Text = "Region1",
                Width = 150,
                Margin = new Thickness(0, 0, 4, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
                ToolTip = "Name für neue Formen; gleiche Namen ergeben zusammen eine Region."
            };
            toolbar.Children.Add(_nameInput);
            AddButton(toolbar, "Auswahl umbenennen", RenameSelected);
            AddButton(toolbar, "Auswahl löschen", DeleteSelected);
            AddButton(toolbar, "Speichern", () => Save());
            AddButton(toolbar, "Verwerfen", Discard);
            AddButton(toolbar, "−", () => SetZoom(_zoomTransform.ScaleX / 1.25));
            _zoomLabel = CreateLabel("");
            toolbar.Children.Add(_zoomLabel);
            AddButton(toolbar, "+", () => SetZoom(_zoomTransform.ScaleX * 1.25));
            root.Children.Add(toolbar);

            Grid workspace = new Grid();
            workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            workspace.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(280) });
            _mapCanvas = new Canvas
            {
                LayoutTransform = _zoomTransform,
                ClipToBounds = true,
                Cursor = Cursors.Cross
            };
            RenderOptions.SetBitmapScalingMode(_mapCanvas, BitmapScalingMode.NearestNeighbor);
            _mapCanvas.MouseLeftButtonDown += (sender, e) =>
            {
                _mapCanvas.CaptureMouse();
                PointerDown(e.GetPosition(_mapCanvas));
                e.Handled = true;
            };
            _mapCanvas.MouseMove += (sender, e) =>
            {
                if (_dragMode != DragMode.None)
                {
                    PointerMove(e.GetPosition(_mapCanvas));
                }
            };
            _mapCanvas.MouseLeftButtonUp += (sender, e) =>
            {
                PointerUp(e.GetPosition(_mapCanvas));
                _mapCanvas.ReleaseMouseCapture();
                e.Handled = true;
            };
            _mapCanvas.MouseRightButtonDown += (sender, e) =>
            {
                DeleteAt(e.GetPosition(_mapCanvas));
                e.Handled = true;
            };
            ScrollViewer mapScroll = new ScrollViewer
            {
                Content = _mapCanvas,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(4),
                Background = new SolidColorBrush(Color.FromRgb(17, 23, 31))
            };
            workspace.Children.Add(mapScroll);

            StackPanel side = new StackPanel { Margin = new Thickness(4) };
            side.Children.Add(new TextBlock
            {
                Text = "Regionen dieser Map",
                FontSize = 15,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 4)
            });
            _regionList = new ListBox
            {
                Height = 360,
                Background = new SolidColorBrush(Color.FromRgb(30, 40, 52)),
                Foreground = Brushes.White
            };
            _regionList.SelectionChanged += (sender, e) =>
            {
                if (!_suppressListSelection)
                {
                    Select(_regionList.SelectedIndex);
                }
            };
            side.Children.Add(_regionList);
            side.Children.Add(new TextBlock
            {
                Text = "Ziehen auf freier Fläche: neue Form · Ziehen auf einer Form: "
                    + "verschieben · Rechtsklick: löschen. Formen mit gleichem Namen "
                    + "bilden zusammen eine Region. Gegnerarten wählen Regionen im "
                    + "Gegner-Editor; mit F4 + B sind sie im Spiel sichtbar.",
                Foreground = TextBrush,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 8, 0, 0)
            });
            Grid.SetColumn(side, 1);
            workspace.Children.Add(side);
            Grid.SetRow(workspace, 1);
            root.Children.Add(workspace);

            _status = new TextBlock
            {
                Foreground = TextBrush,
                Margin = new Thickness(6, 3, 0, 0),
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetRow(_status, 2);
            root.Children.Add(_status);

            Content = root;
            SetZoom(0.5);
            _mapSelection.SelectedItem = ((IEnumerable<string>)_mapSelection.ItemsSource)
                .FirstOrDefault(name => String.Equals(name, _field.MapFileName,
                    StringComparison.OrdinalIgnoreCase))
                ?? ((IEnumerable<string>)_mapSelection.ItemsSource).FirstOrDefault();
        }

        internal Func<string, bool> Confirm { get; set; } = message =>
            MessageBox.Show(message, "Spawn-Regionen",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

        internal Action<string> ShowError { get; set; } = message =>
            MessageBox.Show(message, "Spawn-Regionen",
                MessageBoxButton.OK, MessageBoxImage.Warning);

        internal string MapFileName { get { return _mapFileName; } }
        internal IReadOnlyList<ArenaMapRegion> Regions { get { return _regions; } }
        internal int SelectedIndex { get { return _selected; } }
        internal bool IsDirty { get { return _dirty; } }
        internal string StatusText { get { return _status.Text; } }

        internal bool DrawCircles
        {
            get { return _circleTool.IsChecked == true; }
            set { _circleTool.IsChecked = value; }
        }

        internal string NewShapeName
        {
            get { return _nameInput.Text; }
            set { _nameInput.Text = value; }
        }

        // ---- map ----

        internal void SelectMap(string mapFileName)
        {
            _mapSelection.SelectedItem = mapFileName;
        }

        private void OnMapSelected(object sender, SelectionChangedEventArgs e)
        {
            if (!(_mapSelection.SelectedItem is string map)
                || String.Equals(map, _mapFileName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            if (_dirty && !Confirm("Nicht gespeicherte Regionen verwerfen?"))
            {
                _mapSelection.SelectedItem = _mapFileName;
                return;
            }
            LoadMap(map);
        }

        private void LoadMap(string map)
        {
            try
            {
                ArenaMapDefinition definition = ArenaMapAssets.LoadMap(map);
                _mapCanvas.Width = definition.Width;
                _mapCanvas.Height = definition.Height;
                _mapCanvas.Background = new ImageBrush(definition.Image) { Stretch = Stretch.Fill };
            }
            catch (Exception exception) when (IsFileProblem(exception))
            {
                Fail("Map „" + map + "“ kann nicht geladen werden: " + exception.Message);
                return;
            }

            _mapFileName = map;
            _regions.Clear();
            try
            {
                _regions.AddRange(ArenaMapRegionRepository.Load(map).Select(region => region.Copy()));
                SetStatus(_regions.Count == 0
                    ? "„" + map + "“ hat noch keine Regionen."
                    : "„" + map + "“: " + _regions.Count + " Form(en) geladen.");
            }
            catch (Exception exception) when (IsFileProblem(exception))
            {
                Fail("Regionen von „" + map + "“ nicht lesbar: " + exception.Message
                    + " Speichern ersetzt die Datei.");
            }
            _selected = -1;
            _dirty = false;
            Refresh();
        }

        // ---- pointer (world coordinates of the map) ----

        internal void PointerDown(Point point)
        {
            int hit = FindShapeAt(point);
            _dragStart = point;
            if (hit >= 0)
            {
                Select(hit);
                _dragMode = DragMode.Move;
                _dragOriginal = _regions[hit].Copy();
                return;
            }

            _dragMode = DragMode.Create;
            _drawing = new ArenaMapRegion
            {
                Name = GetNewShapeName(),
                Shape = DrawCircles ? ArenaRegionShape.Circle : ArenaRegionShape.Rectangle,
                X = point.X,
                Y = point.Y
            };
            Refresh();
        }

        internal void PointerMove(Point point)
        {
            if (_dragMode == DragMode.Create && _drawing != null)
            {
                if (_drawing.Shape == ArenaRegionShape.Circle)
                {
                    _drawing.X = _dragStart.X;
                    _drawing.Y = _dragStart.Y;
                    _drawing.Radius = (point - _dragStart).Length;
                }
                else
                {
                    _drawing.X = Math.Min(_dragStart.X, point.X);
                    _drawing.Y = Math.Min(_dragStart.Y, point.Y);
                    _drawing.Width = Math.Abs(point.X - _dragStart.X);
                    _drawing.Height = Math.Abs(point.Y - _dragStart.Y);
                }
                Refresh();
            }
            else if (_dragMode == DragMode.Move && _selected >= 0)
            {
                ArenaMapRegion region = _regions[_selected];
                region.X = _dragOriginal.X + point.X - _dragStart.X;
                region.Y = _dragOriginal.Y + point.Y - _dragStart.Y;
                _dirty = true;
                Refresh();
            }
        }

        internal void PointerUp(Point point)
        {
            PointerMove(point);
            if (_dragMode == DragMode.Create && _drawing != null)
            {
                bool largeEnough = _drawing.Shape == ArenaRegionShape.Circle
                    ? _drawing.Radius * 2 >= MinimumShapeSize
                    : _drawing.Width >= MinimumShapeSize && _drawing.Height >= MinimumShapeSize;
                if (largeEnough)
                {
                    _regions.Add(_drawing);
                    _selected = _regions.Count - 1;
                    _dirty = true;
                    SetStatus("„" + _drawing.Name + "“ hinzugefügt – Speichern nicht vergessen.");
                }
                else
                {
                    Select(-1);
                }
            }
            _drawing = null;
            _dragMode = DragMode.None;
            Refresh();
        }

        private void DeleteAt(Point point)
        {
            int hit = FindShapeAt(point);
            if (hit >= 0)
            {
                Select(hit);
                DeleteSelected();
            }
        }

        // The topmost shape at the point (the last drawn), or -1.
        private int FindShapeAt(Point point)
        {
            for (int i = _regions.Count - 1; i >= 0; i--)
            {
                if (_regions[i].Contains(point.X, point.Y))
                {
                    return i;
                }
            }
            return -1;
        }

        // ---- actions ----

        private void Select(int index)
        {
            _selected = index >= 0 && index < _regions.Count ? index : -1;
            if (_selected >= 0)
            {
                _nameInput.Text = _regions[_selected].Name;
            }
            Refresh();
        }

        internal void RenameSelected()
        {
            string name = (_nameInput.Text ?? "").Trim();
            if (_selected < 0)
            {
                Fail("Zuerst eine Form auswählen.");
                return;
            }
            if (name.Length == 0)
            {
                Fail("Der Name darf nicht leer sein.");
                return;
            }
            _regions[_selected].Name = name;
            _dirty = true;
            Refresh();
        }

        internal void DeleteSelected()
        {
            if (_selected < 0)
            {
                return;
            }
            string name = _regions[_selected].Name;
            _regions.RemoveAt(_selected);
            _selected = -1;
            _dirty = true;
            SetStatus("Form von „" + name + "“ gelöscht – Speichern nicht vergessen.");
            Refresh();
        }

        internal bool Save()
        {
            if (_mapFileName == null)
            {
                Fail("Keine Map gewählt.");
                return false;
            }
            try
            {
                ArenaMapRegionRepository.Save(_mapFileName, _regions);
                if (String.Equals(_mapFileName, _field.MapFileName, StringComparison.OrdinalIgnoreCase))
                {
                    _field.ReloadMapRegions();
                }
                _dirty = false;
                _regionsSaved?.Invoke();
                SetStatus("Gespeichert: " + ArenaMapRegionRepository.GetPath(_mapFileName)
                    + " (" + _regions.Select(region => region.Name)
                        .Distinct(StringComparer.OrdinalIgnoreCase).Count() + " Region(en)).");
                return true;
            }
            catch (Exception exception) when (IsFileProblem(exception))
            {
                Fail(exception.Message);
                return false;
            }
        }

        private void Discard()
        {
            if (_mapFileName != null && (!_dirty || Confirm("Änderungen verwerfen?")))
            {
                LoadMap(_mapFileName);
            }
        }

        private string GetNewShapeName()
        {
            string name = (_nameInput.Text ?? "").Trim();
            return name.Length > 0 ? name : "Region1";
        }

        private void SetZoom(double zoom)
        {
            zoom = Math.Max(MinimumZoom, Math.Min(MaximumZoom, zoom));
            _zoomTransform.ScaleX = zoom;
            _zoomTransform.ScaleY = zoom;
            _zoomLabel.Text = Math.Round(zoom * 100) + " %";
        }

        // ---- drawing ----

        private void Refresh()
        {
            _mapCanvas.Children.Clear();
            for (int i = 0; i < _regions.Count; i++)
            {
                AddShape(_regions[i], i == _selected);
            }
            if (_drawing != null)
            {
                AddShape(_drawing, true);
            }

            _suppressListSelection = true;
            try
            {
                _regionList.ItemsSource = _regions.Select(Describe).ToArray();
                _regionList.SelectedIndex = _selected;
            }
            finally
            {
                _suppressListSelection = false;
            }
        }

        private void AddShape(ArenaMapRegion region, bool selected)
        {
            Rect bounds = region.Bounds;
            Shape shape = region.Shape == ArenaRegionShape.Circle
                ? (Shape)new Ellipse()
                : new Rectangle();
            shape.Width = Math.Max(1, bounds.Width);
            shape.Height = Math.Max(1, bounds.Height);
            shape.Stroke = selected ? SelectedStroke : ShapeStroke;
            shape.Fill = selected ? SelectedFill : ShapeFill;
            shape.StrokeThickness = 3;
            shape.IsHitTestVisible = false;
            Canvas.SetLeft(shape, bounds.X);
            Canvas.SetTop(shape, bounds.Y);
            _mapCanvas.Children.Add(shape);
            TextBlock label = new TextBlock
            {
                Text = region.Name,
                Foreground = selected ? SelectedStroke : ShapeStroke,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(label, bounds.X + 6);
            Canvas.SetTop(label, bounds.Y + 4);
            _mapCanvas.Children.Add(label);
        }

        private static string Describe(ArenaMapRegion region)
        {
            return region.Shape == ArenaRegionShape.Circle
                ? region.Name + " – Kreis, Radius " + Format(region.Radius)
                    + " bei " + Format(region.X) + "/" + Format(region.Y)
                : region.Name + " – Rechteck " + Format(region.Width) + " × "
                    + Format(region.Height) + " bei " + Format(region.X) + "/" + Format(region.Y);
        }

        // ---- helpers ----

        private static string Format(double value)
        {
            return Math.Round(value).ToString(GermanCulture);
        }

        private void SetStatus(string message)
        {
            _status.Foreground = TextBrush;
            _status.Text = message;
        }

        private void Fail(string message)
        {
            _status.Foreground = ErrorBrush;
            _status.Text = message;
            ShowError(message);
        }

        private static bool IsFileProblem(Exception exception)
        {
            return exception is IOException || exception is UnauthorizedAccessException
                || exception is JsonException || exception is InvalidDataException
                || exception is ArgumentException || exception is NotSupportedException
                || exception is InvalidOperationException;
        }

        private static TextBlock CreateLabel(string text)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0)
            };
        }

        private static void AddButton(Panel panel, string label, Action action)
        {
            Button button = new Button
            {
                Content = label,
                Padding = new Thickness(8, 4, 8, 4),
                Margin = new Thickness(3),
                VerticalAlignment = VerticalAlignment.Center
            };
            button.Click += (sender, args) => action();
            panel.Children.Add(button);
        }
    }
}
