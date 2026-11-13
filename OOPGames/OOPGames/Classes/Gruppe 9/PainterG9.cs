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
    public sealed class PainterG9 : IPaintGame2
    {
        private Window _zoomInputWindow;
        private ArenaField _zoomInputField;

        // Reusable brushes avoid creating new colors for every frame.
        private static readonly Brush FloorBrush = new SolidColorBrush(Color.FromRgb(55, 93, 65));
        private static readonly Brush GridBrush = new SolidColorBrush(Color.FromRgb(68, 108, 78));
        private static readonly Brush WallBrush = new SolidColorBrush(Color.FromRgb(55, 57, 62));
        private static readonly object StartMenuTag = new object();
        private static readonly AnimationProfile BoxingKnightProfile = new AnimationProfile(
            "BoxingKnight",
            new[] { "Character", "BoxingKnight", "BoxingKnightSpritesheet.png" },
            new[] { "Character", "BoxingKnight", "BoxingKnightAnimation.json" },
            "IdleFront", "IdleBack", "IdleLeft", "IdleRight",
            "WalkFront", "WalkBack", "WalkLeft", "WalkRight");
        private static readonly AnimationProfile KurtProfile = new AnimationProfile(
            "Kurt",
            new[] { "Character", "Kurt", "Kurt.png" },
            new[] { "Character", "Kurt", "Kurt.json" },
            "Idle Front", "Idle Back", "Idle Left", "Idle Right",
            "Walk Forwards", "Walk Backwards", "Walk Left", "Walk Right");
        private AnimationProfile _selectedAnimationProfile = BoxingKnightProfile;
        private SpriteSheetAnimator _playerAnimator;
        private AnimationProfile _playerAnimatorProfile;
        private bool _showAnimationMenu;

        public string Name { get { return "Arena Survival"; } }

        internal void ResetAnimationProfile()
        {
            _selectedAnimationProfile = BoxingKnightProfile;
            _playerAnimator = null;
            _playerAnimatorProfile = null;
        }

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
            double left = field.Player.PositionX - width / 2;
            double top = field.Player.PositionY - height / 2;
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
                _selectedAnimationProfile, field.Player.Facing, field.Player.IsMoving);
            if (_playerAnimator == null
                || !ReferenceEquals(_playerAnimatorProfile, _selectedAnimationProfile))
            {
                _playerAnimator = CreateAnimator(_selectedAnimationProfile);
                _playerAnimatorProfile = _selectedAnimationProfile;
            }
            BitmapSource sprite = _playerAnimator.GetFrame(animationName);
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

        // Listen at the window level so arena controls work regardless of which control has focus.
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
                _zoomInputWindow.PreviewMouseWheel -= OnZoomMouseWheel;
                _zoomInputWindow.Closed -= OnZoomWindowClosed;
            }

            _zoomInputWindow = window;
            _zoomInputField = field;
            if (_zoomInputWindow != null)
            {
                _zoomInputWindow.PreviewKeyDown += OnZoomKeyDown;
                _zoomInputWindow.PreviewMouseWheel += OnZoomMouseWheel;
                _zoomInputWindow.Closed += OnZoomWindowClosed;
            }
        }

        private void OnZoomKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat)
            {
                if (e.Key == Key.Escape && _zoomInputField != null)
                {
                    _zoomInputField.ReturnToStartMenu();
                    e.Handled = true;
                    return;
                }

                _zoomInputField?.HandleZoomKey(e.Key);
            }
        }

        private void OnZoomMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
                && _zoomInputField != null)
            {
                _zoomInputField.HandleZoomWheel(e.Delta);
                e.Handled = true;
            }
        }

        private void OnZoomWindowClosed(object sender, EventArgs e)
        {
            if (_zoomInputWindow != null)
            {
                _zoomInputWindow.PreviewKeyDown -= OnZoomKeyDown;
                _zoomInputWindow.PreviewMouseWheel -= OnZoomMouseWheel;
                _zoomInputWindow.Closed -= OnZoomWindowClosed;
                _zoomInputWindow = null;
                _zoomInputField = null;
            }
        }

        // Keep Boxing Knight's game-state naming convention outside the generic animator.
        private static string GetPlayerAnimationName(
            AnimationProfile profile, PlayerFacing facing, bool isMoving)
        {
            string idleAnimation;
            string walkAnimation;
            switch (facing)
            {
                case PlayerFacing.Back:
                    idleAnimation = profile.IdleBack;
                    walkAnimation = profile.WalkBack;
                    break;
                case PlayerFacing.Left:
                    idleAnimation = profile.IdleLeft;
                    walkAnimation = profile.WalkLeft;
                    break;
                case PlayerFacing.Right:
                    idleAnimation = profile.IdleRight;
                    walkAnimation = profile.WalkRight;
                    break;
                default:
                    idleAnimation = profile.IdleFront;
                    walkAnimation = profile.WalkFront;
                    break;
            }

            return isMoving ? walkAnimation : idleAnimation;
        }

        private static SpriteSheetAnimator CreateAnimator(AnimationProfile profile)
        {
            return new SpriteSheetAnimator(
                FindAssetPath(profile.SpriteSheetPath),
                FindAssetPath(profile.AnimationMetadataPath));
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
                        string[] pathParts = new string[relativePath.Length + 4];
                        pathParts[0] = directory.FullName;
                        pathParts[1] = "Classes";
                        pathParts[2] = "Gruppe9";
                        pathParts[3] = "Assets";
                        Array.Copy(relativePath, 0, pathParts, 4, relativePath.Length);
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
                "Could not locate an animation asset under Classes\\Gruppe9\\Assets.",
                String.Join(System.IO.Path.DirectorySeparatorChar.ToString(), relativePath));
        }

        private void PaintStartMenu(Canvas canvas, ArenaField field, double width, double height)
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
            if (_showAnimationMenu)
            {
                contents.Children.Add(new TextBlock
                {
                    Text = "PLAYER ANIMATION",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 8)
                });
                contents.Children.Add(new TextBlock
                {
                    Text = "Selected: " + _selectedAnimationProfile.Name,
                    FontSize = 15,
                    Foreground = new SolidColorBrush(Color.FromRgb(183, 197, 212)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 14)
                });
                contents.Children.Add(CreateMenuButton("BoxingKnight (Default)",
                    GetAnimationButtonBrush(BoxingKnightProfile),
                    (sender, e) => SelectAnimationProfile(
                        canvas, BoxingKnightProfile)));
                contents.Children.Add(CreateMenuButton("Kurt",
                    GetAnimationButtonBrush(KurtProfile),
                    (sender, e) => SelectAnimationProfile(canvas, KurtProfile)));
                contents.Children.Add(CreateMenuButton("Back",
                    new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                    (sender, e) =>
                    {
                        _showAnimationMenu = false;
                        canvas.Tag = null;
                    }));
                panel.Child = contents;
                DrawStartMenuPanel(canvas, panel, width, height);
                return;
            }

            contents.Children.Add(new TextBlock
            {
                Text = "ARENA PAUSED",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });
            contents.Children.Add(new TextBlock
            {
                Text = "Your game is paused",
                FontSize = 15,
                Foreground = new SolidColorBrush(Color.FromRgb(183, 197, 212)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });
            contents.Children.Add(CreateMenuButton("Spiel fortsetzen",
                new SolidColorBrush(Color.FromRgb(43, 91, 125)),
                (sender, e) =>
                {
                    if (!field.ContinueGame())
                    {
                        MessageBox.Show("Kein aktives Spiel gefunden.",
                            "Spiel fortsetzen", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }));
            contents.Children.Add(CreateMenuButton("How to Play",
                new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                (sender, e) => ShowNotImplemented("How to Play")));
            contents.Children.Add(CreateMenuButton("Choose Animation",
                new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                (sender, e) =>
                {
                    _showAnimationMenu = true;
                    canvas.Tag = null;
                }));
            contents.Children.Add(CreateMenuButton("High Scores",
                new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                (sender, e) => ShowNotImplemented("High Scores")));
            panel.Child = contents;

            DrawStartMenuPanel(canvas, panel, width, height);
        }

        private static void DrawStartMenuPanel(Canvas canvas, Border panel, double width, double height)
        {
            panel.Measure(new Size(width, height));
            Canvas.SetLeft(panel, (width - panel.DesiredSize.Width) / 2);
            Canvas.SetTop(panel, (height - panel.DesiredSize.Height) / 2);
            canvas.Children.Add(panel);
            canvas.Tag = StartMenuTag;
        }

        private Brush GetAnimationButtonBrush(AnimationProfile profile)
        {
            return ReferenceEquals(profile, _selectedAnimationProfile)
                ? new SolidColorBrush(Color.FromRgb(43, 125, 82))
                : new SolidColorBrush(Color.FromRgb(55, 70, 88));
        }

        private void SelectAnimationProfile(Canvas canvas, AnimationProfile profile)
        {
            if (!ReferenceEquals(profile, _selectedAnimationProfile))
            {
                _selectedAnimationProfile = profile;
                _playerAnimator = null;
                _playerAnimatorProfile = null;
            }
            canvas.Tag = null;
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

        private sealed class AnimationProfile
        {
            public string Name { get; }
            public string[] SpriteSheetPath { get; }
            public string[] AnimationMetadataPath { get; }
            public string IdleFront { get; }
            public string IdleBack { get; }
            public string IdleLeft { get; }
            public string IdleRight { get; }
            public string WalkFront { get; }
            public string WalkBack { get; }
            public string WalkLeft { get; }
            public string WalkRight { get; }

            public AnimationProfile(
                string name,
                string[] spriteSheetPath,
                string[] animationMetadataPath,
                string idleFront,
                string idleBack,
                string idleLeft,
                string idleRight,
                string walkFront,
                string walkBack,
                string walkLeft,
                string walkRight)
            {
                Name = name;
                SpriteSheetPath = spriteSheetPath;
                AnimationMetadataPath = animationMetadataPath;
                IdleFront = idleFront;
                IdleBack = idleBack;
                IdleLeft = idleLeft;
                IdleRight = idleRight;
                WalkFront = walkFront;
                WalkBack = walkBack;
                WalkLeft = walkLeft;
                WalkRight = walkRight;
            }
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