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
        private BitmapSource _baseImage;
        private ArenaMapElement _draggedElement;
        private Image _mapDragPreview;
        private Point _dragStart;

        public ArenaMapEditorControl(ArenaField field)
        {
            _field = field;
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
            workspace.Children.Add(_palette);

            _mapCanvas = new Canvas
            {
                Width = 1800,
                Height = 1200,
                Background = Brushes.Black,
                AllowDrop = true,
                ClipToBounds = true
            };
            _mapCanvas.DragEnter += OnMapDragOver;
            _mapCanvas.DragOver += OnMapDragOver;
            _mapCanvas.DragLeave += OnMapDragLeave;
            _mapCanvas.Drop += OnMapDrop;
            _mapCanvas.MouseRightButtonDown += OnMapRightButtonDown;
            ScrollViewer mapScroll = new ScrollViewer
            {
                Content = _mapCanvas,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(4),
                Background = new SolidColorBrush(Color.FromRgb(17, 23, 31))
            };
            Grid.SetColumn(mapScroll, 1);
            workspace.Children.Add(mapScroll);
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
            Point position = GetMapPlacementPosition(point, element);
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
                e.GetPosition(_mapCanvas), element);
            _placements.Add(new ArenaMapPlacement(
                element.Image, position.X, position.Y));
            RedrawMap();
            e.Handled = true;
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

        private Point GetMapPlacementPosition(Point pointer, ArenaMapElement element)
        {
            double x = Math.Max(0, Math.Min(
                _mapCanvas.Width - element.Image.PixelWidth,
                pointer.X - element.Image.PixelWidth / 2.0));
            double y = Math.Max(0, Math.Min(
                _mapCanvas.Height - element.Image.PixelHeight,
                pointer.Y - element.Image.PixelHeight / 2.0));
            return new Point(x, y);
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
                    IsHitTestVisible = false
                };
                _mapCanvas.Children.Add(_mapDragPreview);
            }

            Canvas.SetLeft(_mapDragPreview, position.X);
            Canvas.SetTop(_mapDragPreview, position.Y);
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
                if (point.X >= placement.X && point.X < placement.X + placement.Image.PixelWidth
                    && point.Y >= placement.Y
                    && point.Y < placement.Y + placement.Image.PixelHeight)
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
            foreach (ArenaMapPlacement placement in _placements)
            {
                Image image = new Image
                {
                    Source = placement.Image,
                    Width = placement.Image.PixelWidth,
                    Height = placement.Image.PixelHeight,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(image, placement.X);
                Canvas.SetTop(image, placement.Y);
                _mapCanvas.Children.Add(image);
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
                    + ". Wähle „Als Spielhintergrund verwenden“, um die Karte zu aktivieren.");
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
        private const double BoardWidth = 2048;
        private const double BoardHeight = 1280;
        private const string SkillDragFormat = "OOPGames.ArenaSkill";
        private readonly ArenaField _field;
        private readonly IReadOnlyList<ArenaSkillInfo> _skills;
        private readonly IReadOnlyList<ArenaSkillInfo> _paletteItems;
        private readonly ListBox _palette;
        private readonly Canvas _treeCanvas;
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
        private string _treeName;
        private string _selectedSkillId;
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
            ScrollViewer treeScroll = new ScrollViewer
            {
                Content = _treeCanvas,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(4),
                Background = new SolidColorBrush(Color.FromRgb(17, 23, 31))
            };
            Grid.SetColumn(treeScroll, 1);
            workspace.Children.Add(treeScroll);
            Grid.SetRow(workspace, 1);
            root.Children.Add(workspace);

            _status = new TextBlock
            {
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
            ArenaSkillInfo skill = GetDraggedSkill(e.Data);
            if (skill == null)
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
            ArenaSkillInfo skill = GetDraggedSkill(e.Data);
            if (skill == null)
            {
                return;
            }
            HideTreeDragPreview();
            if (_tree.Nodes.Any(node => node.SkillId == skill.Id))
            {
                SetStatus("Dieser Skill liegt bereits auf der Fläche.");
                return;
            }

            Point position = GetNodePosition(e.GetPosition(_treeCanvas));
            bool isFork = skill.Id == ArenaSkillTreeRepository.ForkPaletteId;
            ArenaSkillTreeNode newNode = new ArenaSkillTreeNode
            {
                SkillId = isFork
                    ? ArenaSkillTreeRepository.CreateForkNodeId()
                    : skill.Id,
                X = position.X,
                Y = position.Y,
                IsFork = isFork
            };
            _tree.Nodes.Add(newNode);
            if (isFork)
            {
                _selectedSkillId = newNode.SkillId;
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

        private void HideTreeDragPreview()
        {
            if (_treeDragPreview != null)
            {
                _treeCanvas.Children.Remove(_treeDragPreview);
                _treeDragPreview = null;
                _treeDragPreviewSkillId = null;
            }
        }

        private void OnNodeClicked(string skillId)
        {
            _selectedSkillId = skillId;
            if (!_connectionMode)
            {
                UpdateForkDecisionOption();
                SetStatus("Ausgewählt: " + GetSkill(skillId).Name);
                RedrawTree();
                return;
            }
            if (String.IsNullOrEmpty(_pendingConnectionStart))
            {
                _pendingConnectionStart = skillId;
                SetStatus("Verbindungsquelle: " + GetSkill(skillId).Name
                    + ". Klicke jetzt den Skill, der dadurch freigeschaltet werden soll.");
                RedrawTree();
                return;
            }
            if (String.Equals(_pendingConnectionStart, skillId, StringComparison.Ordinal))
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
                        && connection.ToSkillId == skillId);
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
                    ToSkillId = skillId
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
                node => node.IsFork && node.SkillId == _selectedSkillId);
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
                node => node.IsFork && node.SkillId == _selectedSkillId);
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
                bool isStartNode = ArenaSkillTreeRepository.IsStartNode(node.SkillId);
                bool isFork = node.IsFork;
                ArenaSkillInfo skill = GetSkill(node.SkillId);
                Border tile = isStartNode
                    ? ArenaSkillTreeVisuals.CreateNodeTile(
                        skill.Name, "Baumstart", ArenaSkillTreeTileState.Start)
                    : isFork
                        ? ArenaSkillTreeVisuals.CreateForkTile(node.ForceDecision)
                        : ArenaSkillTreeVisuals.CreateNodeTile(
                            skill.Name, "Fähigkeit", ArenaSkillTreeTileState.Available);
                tile.Tag = node.SkillId;
                tile.ToolTip = skill.Description;
                tile.Cursor = Cursors.Hand;
                tile.MouseLeftButtonDown += (sender, args) =>
                {
                    OnNodeClicked((string)((FrameworkElement)sender).Tag);
                    args.Handled = true;
                };
                Canvas.SetLeft(tile, node.X);
                Canvas.SetTop(tile, node.Y);
                _treeCanvas.Children.Add(tile);
            }
            UpdateForkDecisionOption();
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
            _tree.Nodes.RemoveAll(node => node.SkillId == _selectedSkillId);
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
                        X = 100,
                        Y = 300
                    }
                },
                Connections = new List<ArenaSkillTreeConnection>()
            };
            _treeName = _tree.Name;
            _treeNameInput.Text = _treeName;
            _selectedSkillId = null;
            _pendingConnectionStart = null;
            RedrawTree();
            SetStatus("Neuer Skilltree mit Startkachel. Verbinde den Start mit den zuerst freischaltbaren Skills.");
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
            if (ArenaSkillTreeRepository.IsStartNode(skillId))
            {
                return ArenaSkillTreeRepository.StartNodeInfo;
            }
            ArenaSkillTreeNode fork = _tree.Nodes.FirstOrDefault(
                node => node.IsFork && node.SkillId == skillId);
            if (fork != null)
            {
                return ArenaSkillTreeRepository.GetForkNodeInfo(
                    fork.ForceDecision);
            }
            ArenaSkillInfo skill = _skills.FirstOrDefault(item => item.Id == skillId);
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
