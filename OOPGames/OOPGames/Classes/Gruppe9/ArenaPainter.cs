//P06.11.2026
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OOPGames
{
    // Draws the arena every frame and keeps the player's world position at screen center.
    public sealed class ArenaPainter : IPaintGame2
    {
        private Window _zoomInputWindow;
        private ArenaField _zoomInputField;

        // Reusable brushes avoid creating new colors for every frame.
        private static readonly Brush FloorBrush = new SolidColorBrush(Color.FromRgb(55, 93, 65));
        private static readonly Brush GridBrush = new SolidColorBrush(Color.FromRgb(68, 108, 78));
        private static readonly Brush WallBrush = new SolidColorBrush(Color.FromRgb(55, 57, 62));
        private static readonly object StartMenuTag = new object();
        // The entity-specific setup supplies assets and maps movement to clip names.
        private static readonly Lazy<SpriteSheetAnimator> PlayerAnimator =
            new Lazy<SpriteSheetAnimator>(() => new SpriteSheetAnimator(
                FindAssetPath("Character", "BoxingKnight", "BoxingKnightSpritesheet.png"),
                FindAssetPath("Character", "BoxingKnight", "BoxingKnightAnimation.json")));

        public string Name { get { return "Arena Survival"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            // Use the same rendering path for both initial and tick-based painting.
            TickPaintGameField(canvas, currentField);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            // Ignore fields belonging to other games in the shared framework.
            if (!(currentField is ArenaField field))
            {
                return;
            }

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                // Canvas dimensions are not usable until layout has measured it.
                return;
            }

            if (!field.IsGameStarted)
            {
                if (!ReferenceEquals(canvas.Tag, StartMenuTag))
                {
                    PaintStartMenu(canvas, field, width, height);
                }
                return;
            }

            AttachZoomInput(canvas, field);

            // Remove the previous frame and clip drawings to the visible viewport.
            canvas.ClipToBounds = true;
            canvas.Children.Clear();
            canvas.Tag = null;
            canvas.Background = WallBrush;
            Canvas worldCanvas = new Canvas
            {
                Width = width,
                Height = height,
                RenderTransform = new ScaleTransform(
                    field.ZoomLevel, field.ZoomLevel, width / 2, height / 2)
            };

            // Offset map coordinates by the player's position to keep the camera centered.
            double left = field.Player.X - width / 2;
            double top = field.Player.Y - height / 2;
            double mapLeft = -left;
            double mapTop = -top;

            // Draw the walkable area first so grid lines and walls appear above it.
            AddRectangle(worldCanvas, FloorBrush,
                mapLeft + ArenaField.WallThickness,
                mapTop + ArenaField.WallThickness,
                ArenaField.MapWidth - 2 * ArenaField.WallThickness,
                ArenaField.MapHeight - 2 * ArenaField.WallThickness);

            double gridSpacing = 64;
            // Add a subtle grid as a visual reference for movement through the world.
            for (double x = ArenaField.WallThickness; x < ArenaField.MapWidth - ArenaField.WallThickness; x += gridSpacing)
            {
                AddLine(worldCanvas, GridBrush, mapLeft + x, mapTop + ArenaField.WallThickness,
                    mapLeft + x, mapTop + ArenaField.MapHeight - ArenaField.WallThickness);
            }
            for (double y = ArenaField.WallThickness; y < ArenaField.MapHeight - ArenaField.WallThickness; y += gridSpacing)
            {
                AddLine(worldCanvas, GridBrush, mapLeft + ArenaField.WallThickness, mapTop + y,
                    mapLeft + ArenaField.MapWidth - ArenaField.WallThickness, mapTop + y);
            }

            // Draw the four wall segments over the floor to frame the playable area.
            AddRectangle(worldCanvas, WallBrush, mapLeft, mapTop, ArenaField.MapWidth, ArenaField.WallThickness);
            AddRectangle(worldCanvas, WallBrush, mapLeft, mapTop + ArenaField.MapHeight - ArenaField.WallThickness,
                ArenaField.MapWidth, ArenaField.WallThickness);
            AddRectangle(worldCanvas, WallBrush, mapLeft, mapTop + ArenaField.WallThickness,
                ArenaField.WallThickness, ArenaField.MapHeight - 2 * ArenaField.WallThickness);
            AddRectangle(worldCanvas, WallBrush, mapLeft + ArenaField.MapWidth - ArenaField.WallThickness,
                mapTop + ArenaField.WallThickness, ArenaField.WallThickness,
                ArenaField.MapHeight - 2 * ArenaField.WallThickness);

            string animationName = GetPlayerAnimationName(
                field.Player.Facing, field.Player.IsMoving);
            BitmapSource sprite = PlayerAnimator.Value.GetFrame(animationName);
            double spriteHeight = field.Player.Size * 2;
            double spriteWidth = spriteHeight * sprite.PixelWidth / sprite.PixelHeight;
            Image player = new Image
            {
                Source = sprite,
                Width = spriteWidth,
                Height = spriteHeight,
                Stretch = Stretch.Uniform,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(player, width / 2 - spriteWidth / 2);
            Canvas.SetTop(player, height / 2 - spriteHeight / 2);
            worldCanvas.Children.Add(player);
            canvas.Children.Add(worldCanvas);
        }

        // Listen at the window level so zoom keys work regardless of which control has focus.
        private void AttachZoomInput(Canvas canvas, ArenaField field)
        {
            Window window = Window.GetWindow(canvas);
            if (ReferenceEquals(window, _zoomInputWindow))
            {
                _zoomInputField = field;
                return;
            }

            if (_zoomInputWindow != null)
            {
                _zoomInputWindow.PreviewKeyDown -= OnZoomKeyDown;
                _zoomInputWindow.Closed -= OnZoomWindowClosed;
            }

            _zoomInputWindow = window;
            _zoomInputField = field;
            if (_zoomInputWindow != null)
            {
                _zoomInputWindow.PreviewKeyDown += OnZoomKeyDown;
                _zoomInputWindow.Closed += OnZoomWindowClosed;
            }
        }

        private void OnZoomKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat)
            {
                _zoomInputField?.HandleZoomKey(e.Key);
            }
        }

        private void OnZoomWindowClosed(object sender, EventArgs e)
        {
            if (_zoomInputWindow != null)
            {
                _zoomInputWindow.PreviewKeyDown -= OnZoomKeyDown;
                _zoomInputWindow.Closed -= OnZoomWindowClosed;
                _zoomInputWindow = null;
                _zoomInputField = null;
            }
        }

        // Keep Boxing Knight's game-state naming convention outside the generic animator.
        private static string GetPlayerAnimationName(PlayerFacing facing, bool isMoving)
        {
            string direction;
            switch (facing)
            {
                case PlayerFacing.Back:
                    direction = "Back";
                    break;
                case PlayerFacing.Left:
                    direction = "Left";
                    break;
                case PlayerFacing.Right:
                    direction = "Right";
                    break;
                default:
                    direction = "Front";
                    break;
            }

            return (isMoving ? "Walk" : "Idle") + direction;
        }

        // Locate project assets both when launched from the project and from its build output.
        private static string FindAssetPath(params string[] relativePath)
        {
            HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            string[] startingDirectories =
            {
                AppContext.BaseDirectory,
                Environment.CurrentDirectory
            };

            foreach (string startingDirectory in startingDirectories)
            {
                DirectoryInfo directory = new DirectoryInfo(startingDirectory);
                while (directory != null)
                {
                    if (visited.Add(directory.FullName))
                    {
                        string[] pathParts = new string[relativePath.Length + 2];
                        pathParts[0] = directory.FullName;
                        pathParts[1] = "Assets";
                        Array.Copy(relativePath, 0, pathParts, 2, relativePath.Length);
                        string path = System.IO.Path.Combine(pathParts);
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                    directory = directory.Parent;
                }
            }

            throw new FileNotFoundException(
                "Could not locate an animation asset in an Assets directory.",
                String.Join(System.IO.Path.DirectorySeparatorChar.ToString(), relativePath));
        }

        private static void PaintStartMenu(Canvas canvas, ArenaField field, double width, double height)
        {
            canvas.Children.Clear();
            canvas.ClipToBounds = true;
            canvas.Background = new SolidColorBrush(Color.FromRgb(12, 17, 26));

            Border panel = new Border
            {
                Width = 380,
                Padding = new Thickness(30),
                CornerRadius = new CornerRadius(12),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(77, 94, 113)),
                Background = new SolidColorBrush(Color.FromRgb(27, 38, 52))
            };
            StackPanel contents = new StackPanel();
            contents.Children.Add(new TextBlock
            {
                Text = "ARENA SURVIVAL",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });
            contents.Children.Add(new TextBlock
            {
                Text = "Get ready to enter the arena",
                FontSize = 15,
                Foreground = new SolidColorBrush(Color.FromRgb(183, 197, 212)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });
            contents.Children.Add(CreateMenuButton("Start Game",
                new SolidColorBrush(Color.FromRgb(43, 125, 82)),
                (sender, e) => field.StartGame()));
            contents.Children.Add(CreateMenuButton("How to Play",
                new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                (sender, e) => ShowNotImplemented("How to Play")));
            contents.Children.Add(CreateMenuButton("Settings",
                new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                (sender, e) => ShowNotImplemented("Settings")));
            contents.Children.Add(CreateMenuButton("High Scores",
                new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                (sender, e) => ShowNotImplemented("High Scores")));
            panel.Child = contents;

            panel.Measure(new Size(width, height));
            Canvas.SetLeft(panel, (width - panel.DesiredSize.Width) / 2);
            Canvas.SetTop(panel, (height - panel.DesiredSize.Height) / 2);
            canvas.Children.Add(panel);
            canvas.Tag = StartMenuTag;
        }

        private static Button CreateMenuButton(string text, Brush background, RoutedEventHandler onClick)
        {
            Button button = new Button
            {
                Content = text,
                Width = 318,
                Height = 46,
                Margin = new Thickness(0, 5, 0, 5),
                Background = background,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Cursor = Cursors.Hand
            };
            button.Click += onClick;
            return button;
        }

        private static void ShowNotImplemented(string feature)
        {
            MessageBox.Show(feature + " is not implemented yet.",
                "Not implemented yet", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private static void AddRectangle(Canvas canvas, Brush brush, double x, double y, double width, double height)
        {
            // Place a filled rectangle in map/screen coordinates on the WPF canvas.
            Rectangle rectangle = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = brush
            };
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            canvas.Children.Add(rectangle);
        }

        private static void AddLine(Canvas canvas, Brush brush, double x1, double y1, double x2, double y2)
        {
            // Grid lines are drawn as separate canvas elements.
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = brush,
                StrokeThickness = 1
            };
            canvas.Children.Add(line);
        }
    }
}