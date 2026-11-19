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
        private readonly ArenaHudRenderer _hudRenderer = new ArenaHudRenderer();
        private readonly ArenaDebugRenderer _debugRenderer = new ArenaDebugRenderer();
        private readonly ArenaProgressionMenuRenderer _progressionMenuRenderer =
            new ArenaProgressionMenuRenderer();
        private readonly ArenaWeaponWheelRenderer _weaponWheelRenderer =
            new ArenaWeaponWheelRenderer();
        private Window _zoomInputWindow;
        private ArenaField _zoomInputField;
        private Canvas _zoomInputCanvas;

        // Reusable brushes avoid creating new colors for every frame.
        private static readonly Brush FloorBrush = new SolidColorBrush(Color.FromRgb(55, 93, 65));
        private static readonly Brush GridBrush = new SolidColorBrush(Color.FromRgb(68, 108, 78));
        private static readonly Brush WallBrush = new SolidColorBrush(Color.FromRgb(55, 57, 62));
        private static readonly object StartMenuTag = new object();
        private static readonly object PauseMenuTag = new object();
        private static readonly object GameOverMenuTag = new object();
        private static readonly AnimationProfile BoxingKnightProfile = new AnimationProfile(
            "BoxingKnight",
            new[] { "Character", "BoxingKnight", "BoxingKnightSpritesheet.png" },
            new[] { "Character", "BoxingKnight", "BoxingKnightAnimation.json" });
        private static readonly AnimationProfile KurtProfile = new AnimationProfile(
            "Kurt",
            new[] { "Character", "Kurt", "Kurt.png" },
            new[] { "Character", "Kurt", "Kurt.json" });
        private static readonly string[] EnemySpriteSheetPath =
            { "Enemies", "Enemie1.png" };
        private static readonly string[] EnemyAnimationMetadataPath =
            { "Enemies", "Enemie1.json" };
        private AnimationProfile _selectedAnimationProfile = BoxingKnightProfile;
        private SpriteSheetAnimator _playerAnimator;
        private AnimationProfile _playerAnimatorProfile;
        private SpriteSheetAnimator _enemyAnimator;
        private ArenaField _pauseMenuField;
        private ArenaField _menuStateField;
        private MenuPage _menuPage;
        private MenuPage _menuPageBeforeWeaponUpgrades = MenuPage.Main;
        private ArenaAction? _keyBindingCaptureAction;
        private bool _lastGameStarted;
        private double _pauseMenuCanvasWidth;
        private double _pauseMenuCanvasHeight;

        public string Name { get { return "Arena Survival"; } }

        internal void ResetAnimationProfile()
        {
            _selectedAnimationProfile = BoxingKnightProfile;
            _playerAnimator = null;
            _playerAnimatorProfile = null;
            _enemyAnimator = null;
            _menuPage = MenuPage.Main;
            _menuPageBeforeWeaponUpgrades = MenuPage.Main;
            _keyBindingCaptureAction = null;
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
            SynchronizeMenuState(field);

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                // Canvas dimensions are not usable until layout has measured it.
                return;
            }

            if (!field.IsGameStarted && !field.HasActiveGame)
            {
                AttachZoomInput(canvas, field);
                if (!ReferenceEquals(canvas.Tag, StartMenuTag))
                {
                    PaintStartMenu(canvas, field, width, height);
                }
                return;
            }

            if (!field.IsGameStarted)
            {
                object menuTag = field.IsGameOver ? GameOverMenuTag : PauseMenuTag;
                if (ReferenceEquals(canvas.Tag, menuTag)
                    && ReferenceEquals(_pauseMenuField, field)
                    && _pauseMenuCanvasWidth == width
                    && _pauseMenuCanvasHeight == height)
                {
                    return;
                }
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

            _debugRenderer.PaintWorldOverlays(worldCanvas, field, left, top);

            string animationName = field.IsPlayerSpinning
                ? GetSpinAnimationName(field.PlayerSpinFacing)
                : GetPlayerAnimationName(
                    field.Player.Facing, field.Player.IsMoving,
                    field.Player.IsAttacking, field.Player.AttackFacing);
            if (_playerAnimator == null
                || !ReferenceEquals(_playerAnimatorProfile, _selectedAnimationProfile))
            {
                _playerAnimator = CreateAnimator(_selectedAnimationProfile);
                _playerAnimatorProfile = _selectedAnimationProfile;
            }
            BitmapSource sprite = _playerAnimator.GetFrame(
                animationName, field.GameTimeScale);
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
            _hudRenderer.PaintStatusPlaceholder(
                worldCanvas, field.Player, width / 2, height / 2);

            foreach (PursuerEnemy enemy in field.Enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (_enemyAnimator == null)
                {
                    _enemyAnimator = new SpriteSheetAnimator(
                        FindAssetPath(EnemySpriteSheetPath),
                        FindAssetPath(EnemyAnimationMetadataPath));
                }

                BitmapSource enemyFrame = _enemyAnimator.GetFrame(
                    GetEnemyAnimationName(enemy), field.GameTimeScale);
                double enemyHeight = enemy.Size * 2;
                double enemyWidth = enemyHeight * enemyFrame.PixelWidth / enemyFrame.PixelHeight;
                Image enemyImage = new Image
                {
                    Source = enemyFrame,
                    Width = enemyWidth,
                    Height = enemyHeight,
                    Stretch = Stretch.Uniform,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new ScaleTransform(
                        enemy.Facing == PlayerFacing.Right && !enemy.IsMoving
                            ? (enemy.HasActiveAttack ? -1.2 : -1)
                            : (enemy.HasActiveAttack ? 1.2 : 1),
                        enemy.HasActiveAttack ? 1.2 : 1),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(enemyImage,
                    enemy.PositionX - left - enemyWidth / 2);
                Canvas.SetTop(enemyImage,
                    enemy.PositionY - top - enemyHeight / 2);
                worldCanvas.Children.Add(enemyImage);

                _hudRenderer.PaintStatusPlaceholder(
                    worldCanvas, enemy, enemy.PositionX - left, enemy.PositionY - top);
                _hudRenderer.PaintEntityHealth(worldCanvas, enemy,
                    enemy.PositionX - left, enemy.PositionY - top - enemyHeight / 2 - 10);
            }

            canvas.Children.Add(worldCanvas);
            _hudRenderer.PaintPlayerHud(canvas, field.Player, field.Score,
                field.GetAttackCooldownRemaining(field.Player),
                field.Progression, field.KeyBindings);
            _debugRenderer.PaintPanels(canvas, field, width, height);
            if (field.IsWeaponWheelOpen)
            {
                _weaponWheelRenderer.Paint(canvas, field, width, height);
            }
            if (!field.IsGameStarted)
            {
                PaintStartMenu(canvas, field, width, height, true, field.IsGameOver);
                canvas.Tag = field.IsGameOver ? GameOverMenuTag : PauseMenuTag;
                _pauseMenuField = field;
                _pauseMenuCanvasWidth = width;
                _pauseMenuCanvasHeight = height;
            }
        }

        private void SynchronizeMenuState(ArenaField field)
        {
            if (!ReferenceEquals(_menuStateField, field)
                || _lastGameStarted != field.IsGameStarted)
            {
                _menuPage = MenuPage.Main;
                _menuPageBeforeWeaponUpgrades = MenuPage.Main;
                _keyBindingCaptureAction = null;
                _menuStateField = field;
            }
            _lastGameStarted = field.IsGameStarted;
        }

        private static string GetEnemyAnimationName(PursuerEnemy enemy)
        {
            switch (enemy.Facing)
            {
                case PlayerFacing.Back:
                    return enemy.IsMoving ? "WalkForward" : "IdleBack";
                case PlayerFacing.Left:
                    return enemy.IsMoving ? "WalkLeft" : "idleLeft";
                case PlayerFacing.Right:
                    return enemy.IsMoving ? "WalkRight" : "idleLeft";
                default:
                    return enemy.IsMoving ? "WalkForward" : "IdleFront";
            }
        }

        // Listen at the window level so arena controls work regardless of which control has focus.
        private void AttachZoomInput(Canvas canvas, ArenaField field)
        {
            Window window = Window.GetWindow(canvas);
            if (ReferenceEquals(window, _zoomInputWindow))
            {
                _zoomInputField = field;
                if (!ReferenceEquals(canvas, _zoomInputCanvas))
                {
                    if (_zoomInputCanvas != null)
                    {
                        _zoomInputCanvas.PreviewMouseDown -= OnAttackMouseDown;
                    }
                    canvas.PreviewMouseDown += OnAttackMouseDown;
                }
                _zoomInputCanvas = canvas;
                return;
            }

            if (_zoomInputCanvas != null)
            {
                _zoomInputCanvas.PreviewMouseDown -= OnAttackMouseDown;
            }
            if (_zoomInputWindow != null)
            {
                _zoomInputWindow.PreviewKeyDown -= OnZoomKeyDown;
                _zoomInputWindow.PreviewKeyUp -= OnZoomKeyUp;
                _zoomInputWindow.PreviewMouseWheel -= OnZoomMouseWheel;
                _zoomInputWindow.PreviewMouseMove -= OnAimMouseMove;
                _zoomInputWindow.Deactivated -= OnZoomWindowDeactivated;
                _zoomInputWindow.Closed -= OnZoomWindowClosed;
            }

            _zoomInputWindow = window;
            _zoomInputField = field;
            _zoomInputCanvas = canvas;
            if (_zoomInputWindow != null)
            {
                _zoomInputCanvas.PreviewMouseDown += OnAttackMouseDown;
                _zoomInputWindow.PreviewKeyDown += OnZoomKeyDown;
                _zoomInputWindow.PreviewKeyUp += OnZoomKeyUp;
                _zoomInputWindow.PreviewMouseWheel += OnZoomMouseWheel;
                _zoomInputWindow.PreviewMouseMove += OnAimMouseMove;
                _zoomInputWindow.Deactivated += OnZoomWindowDeactivated;
                _zoomInputWindow.Closed += OnZoomWindowClosed;
            }
        }

        private void OnAttackMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_keyBindingCaptureAction.HasValue
                && _menuPage == MenuPage.KeyBindings
                && _zoomInputField != null)
            {
                try
                {
                    _zoomInputField.KeyBindings.SetMouseButton(
                        _keyBindingCaptureAction.Value, e.ChangedButton);
                    _zoomInputField.KeyBindings.Save();
                    _keyBindingCaptureAction = null;
                }
                catch (InvalidOperationException exception)
                {
                    MessageBox.Show(exception.Message,
                        "Tastenbelegung", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                catch (ArgumentOutOfRangeException exception)
                {
                    MessageBox.Show(exception.Message,
                        "Tastenbelegung", MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                if (_zoomInputCanvas != null)
                {
                    _zoomInputCanvas.Tag = null;
                }
                e.Handled = true;
                return;
            }

            if (_zoomInputField == null || !_zoomInputField.IsGameStarted
                || !_zoomInputField.Player.IsAlive || _zoomInputCanvas == null)
            {
                return;
            }
            if (_zoomInputField.IsWeaponWheelOpen)
            {
                e.Handled = true;
                return;
            }

            Point clickPosition = e.GetPosition(_zoomInputCanvas);
            if (clickPosition.X < 0 || clickPosition.Y < 0
                || clickPosition.X > _zoomInputCanvas.ActualWidth
                || clickPosition.Y > _zoomInputCanvas.ActualHeight)
            {
                return;
            }

            double aimX = clickPosition.X - _zoomInputCanvas.ActualWidth / 2;
            double aimY = clickPosition.Y - _zoomInputCanvas.ActualHeight / 2;
            _zoomInputField.SetAimDirection(aimX, aimY);
            _zoomInputField.HandleMouseDown(e.ChangedButton);
        }

        private void OnAimMouseMove(object sender, MouseEventArgs e)
        {
            if (_zoomInputField == null || !_zoomInputField.IsGameStarted
                || _zoomInputCanvas == null)
            {
                return;
            }

            Point mousePosition = e.GetPosition(_zoomInputCanvas);
            if (mousePosition.X < 0 || mousePosition.Y < 0
                || mousePosition.X > _zoomInputCanvas.ActualWidth
                || mousePosition.Y > _zoomInputCanvas.ActualHeight)
            {
                return;
            }

            _zoomInputField.SetAimDirection(
                mousePosition.X - _zoomInputCanvas.ActualWidth / 2,
                mousePosition.Y - _zoomInputCanvas.ActualHeight / 2);
        }

        private void OnZoomKeyDown(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat)
            {
                if (_keyBindingCaptureAction.HasValue
                    && _menuPage == MenuPage.KeyBindings
                    && _zoomInputField != null)
                {
                    if (e.Key == Key.Escape)
                    {
                        _keyBindingCaptureAction = null;
                    }
                    else
                    {
                        try
                        {
                            _zoomInputField.KeyBindings.SetKey(
                                _keyBindingCaptureAction.Value, e.Key);
                            _zoomInputField.KeyBindings.Save();
                            _keyBindingCaptureAction = null;
                        }
                        catch (InvalidOperationException exception)
                        {
                            MessageBox.Show(exception.Message,
                                "Tastenbelegung", MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                        catch (ArgumentOutOfRangeException exception)
                        {
                            MessageBox.Show(exception.Message,
                                "Tastenbelegung", MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }
                    }
                    if (_zoomInputCanvas != null)
                    {
                        _zoomInputCanvas.Tag = null;
                    }
                    e.Handled = true;
                    return;
                }

                if (_zoomInputField != null
                    && _zoomInputField.HandleKeyDown(e.Key))
                {
                    if (_zoomInputCanvas != null)
                    {
                        _zoomInputCanvas.Tag = null;
                    }
                    e.Handled = true;
                }
            }
        }

        private void OnZoomMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
                && _zoomInputField != null)
            {
                _zoomInputField.HandleZoomWheel(e.Delta);
                if (!_zoomInputField.IsGameStarted && _zoomInputCanvas != null)
                {
                    _zoomInputCanvas.Tag = null;
                }
                e.Handled = true;
            }
        }

        private void OnZoomKeyUp(object sender, KeyEventArgs e)
        {
            if (_zoomInputField != null
                && _zoomInputField.HandleKeyUp(e.Key))
            {
                if (_zoomInputCanvas != null)
                {
                    _zoomInputCanvas.Tag = null;
                }
                e.Handled = true;
            }
        }

        private void OnZoomWindowClosed(object sender, EventArgs e)
        {
            if (_zoomInputCanvas != null)
            {
                _zoomInputCanvas.PreviewMouseDown -= OnAttackMouseDown;
            }
            if (_zoomInputWindow != null)
            {
                _zoomInputWindow.PreviewKeyDown -= OnZoomKeyDown;
                _zoomInputWindow.PreviewKeyUp -= OnZoomKeyUp;
                _zoomInputWindow.PreviewMouseWheel -= OnZoomMouseWheel;
                _zoomInputWindow.PreviewMouseMove -= OnAimMouseMove;
                _zoomInputWindow.Deactivated -= OnZoomWindowDeactivated;
                _zoomInputWindow.Closed -= OnZoomWindowClosed;
                _zoomInputWindow = null;
                _zoomInputField = null;
                _zoomInputCanvas = null;
            }
        }

        private void OnZoomWindowDeactivated(object sender, EventArgs e)
        {
            if (_zoomInputField != null && _zoomInputField.IsWeaponWheelOpen)
            {
                _zoomInputField.CancelWeaponWheel();
                if (_zoomInputCanvas != null)
                {
                    _zoomInputCanvas.Tag = null;
                }
            }
        }

        private static string GetPlayerAnimationName(
            PlayerFacing facing, bool isMoving,
            bool isAttacking, PlayerFacing attackFacing)
        {
            if (isAttacking)
            {
                switch (attackFacing)
                {
                    case PlayerFacing.Back:
                        return isMoving ? "WalkAttackBackwards" : "IdleAttackBackwards";
                    case PlayerFacing.Left:
                        return "IdleAttackLeft";
                    case PlayerFacing.Right:
                        return "IdleAttackRight";
                    default:
                        return "IdleAttackFront";
                }
            }

            string idleAnimation;
            string walkAnimation;
            switch (facing)
            {
                case PlayerFacing.Back:
                    idleAnimation = "Idle Back";
                    walkAnimation = "Walk Backwards";
                    break;
                case PlayerFacing.Left:
                    idleAnimation = "Idle Left";
                    walkAnimation = "Walk Left";
                    break;
                case PlayerFacing.Right:
                    idleAnimation = "Idle Right";
                    walkAnimation = "Walk Right";
                    break;
                default:
                    idleAnimation = "Idle Front";
                    walkAnimation = "Walk Forwards";
                    break;
            }

            return isMoving ? walkAnimation : idleAnimation;
        }

        private static string GetSpinAnimationName(PlayerFacing facing)
        {
            switch (facing)
            {
                case PlayerFacing.Back:
                    return "Idle Back";
                case PlayerFacing.Left:
                    return "Idle Left";
                case PlayerFacing.Right:
                    return "Idle Right";
                default:
                    return "Idle Front";
            }
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
                        pathParts[2] = "Gruppe 9";
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
                "Could not locate an animation asset under Classes\\Gruppe 9\\Assets.",
                String.Join(System.IO.Path.DirectorySeparatorChar.ToString(), relativePath));
        }

        private void PaintStartMenu(
            Canvas canvas, ArenaField field, double width, double height,
            bool overlayGame = false, bool isGameOver = false)
        {
            if (overlayGame)
            {
                canvas.Children.Add(new Rectangle
                {
                    Width = width,
                    Height = height,
                    Fill = new SolidColorBrush(Color.FromArgb(160, 0, 0, 0))
                });
            }
            else
            {
                canvas.Children.Clear();
                canvas.ClipToBounds = true;
                canvas.Background = new SolidColorBrush(Color.FromRgb(12, 17, 26));
            }

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
            if (_menuPage == MenuPage.SkillTree)
            {
                contents.Children.Add(_progressionMenuRenderer.CreateSkillTree(
                    field, () => canvas.Tag = null));
                contents.Children.Add(CreateMenuButton("Back",
                    new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                    (sender, e) =>
                    {
                        _menuPage = MenuPage.Main;
                        canvas.Tag = null;
                    }));
                panel.Child = contents;
                DrawStartMenuPanel(canvas, panel, width, height);
                return;
            }

            if (_menuPage == MenuPage.WeaponUpgrades)
            {
                contents.Children.Add(_progressionMenuRenderer.CreateWeaponUpgrades(
                    field, () => canvas.Tag = null));
                contents.Children.Add(CreateMenuButton("Back",
                    new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                    (sender, e) =>
                    {
                        _menuPage = _menuPageBeforeWeaponUpgrades;
                        canvas.Tag = null;
                    }));
                panel.Child = contents;
                DrawStartMenuPanel(canvas, panel, width, height);
                return;
            }

            if (_menuPage == MenuPage.KeyBindings)
            {
                contents.Children.Add(_progressionMenuRenderer.CreateKeyBindings(
                    field, _keyBindingCaptureAction,
                    action =>
                    {
                        _keyBindingCaptureAction = action;
                        canvas.Tag = null;
                    },
                    () => canvas.Tag = null));
                contents.Children.Add(CreateMenuButton("Back",
                    new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                    (sender, e) =>
                    {
                        _menuPage = MenuPage.Main;
                        _keyBindingCaptureAction = null;
                        canvas.Tag = null;
                    }));
                panel.Child = contents;
                DrawStartMenuPanel(canvas, panel, width, height);
                return;
            }

            if (_menuPage == MenuPage.Equipment)
            {
                contents.Children.Add(new TextBlock
                {
                    Text = "AUSRÜSTUNG",
                    FontSize = 24,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 8)
                });
                contents.Children.Add(new TextBlock
                {
                    Text = "Ausgewählt: " + field.SelectedPlayerWeapon.Name
                        + " — " + field.SelectedPlayerWeapon.Effect.Description,
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(183, 197, 212)),
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 8)
                });
                foreach (Weapon weapon in new[]
                {
                    WeaponCatalog.Sword,
                    WeaponCatalog.MagicWand,
                    WeaponCatalog.Bow
                })
                {
                    contents.Children.Add(CreateMenuButton(
                        weapon.Name + " — " + weapon.AttackPattern.Name
                            + " (" + weapon.Effect.Description + ")",
                        GetWeaponButtonBrush(field, weapon),
                        (sender, e) =>
                        {
                            field.SelectPlayerWeapon(weapon);
                            canvas.Tag = null;
                        }));
                    contents.Children.Add(CreateMenuButton(
                        weapon.Name + " — Waffenbaum",
                        new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                        (sender, e) => OpenWeaponUpgrades(
                            canvas, field, weapon, MenuPage.Equipment),
                        field.HasActiveGame && !field.IsGameOver));
                }
                contents.Children.Add(CreateMenuButton("Back",
                    new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                    (sender, e) =>
                    {
                        _menuPage = MenuPage.Main;
                        canvas.Tag = null;
                    }));
                panel.Child = contents;
                DrawStartMenuPanel(canvas, panel, width, height);
                return;
            }

            if (_menuPage == MenuPage.Animation)
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
                contents.Children.Add(CreateMenuButton("BoxingKnight (ohne Schlag)",
                    GetAnimationButtonBrush(BoxingKnightProfile),
                    (sender, e) => SelectAnimationProfile(
                        canvas, BoxingKnightProfile)));
                contents.Children.Add(CreateMenuButton("Kurt (mit Schlaganimation)",
                    GetAnimationButtonBrush(KurtProfile),
                    (sender, e) => SelectAnimationProfile(canvas, KurtProfile)));
                contents.Children.Add(CreateMenuButton("Back",
                    new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                    (sender, e) =>
                    {
                        _menuPage = MenuPage.Main;
                        canvas.Tag = null;
                    }));
                panel.Child = contents;
                DrawStartMenuPanel(canvas, panel, width, height);
                return;
            }

            contents.Children.Add(new TextBlock
            {
                Text = isGameOver
                    ? "GAME OVER"
                    : field.HasActiveGame ? "ARENA PAUSED" : "ARENA SURVIVAL",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });
            contents.Children.Add(new TextBlock
            {
                Text = isGameOver
                    ? "Your run has ended"
                    : field.HasActiveGame ? "Your game is paused" : "Choose equipment and start",
                FontSize = 15,
                Foreground = new SolidColorBrush(Color.FromRgb(183, 197, 212)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });
            contents.Children.Add(CreateMenuButton(
                isGameOver
                    ? "Erneut spielen"
                    : field.HasActiveGame ? "Spiel fortsetzen" : "Spiel starten",
                new SolidColorBrush(Color.FromRgb(43, 91, 125)),
                (sender, e) =>
                {
                    if (isGameOver)
                    {
                        field.RestartGame();
                    }
                    else if (field.HasActiveGame && !field.ContinueGame())
                    {
                        MessageBox.Show("Kein aktives Spiel gefunden.",
                            "Spiel fortsetzen", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else if (!field.HasActiveGame)
                    {
                        field.StartNewGame();
                    }
                }));
            contents.Children.Add(CreateMenuButton("How to Play",
                new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                (sender, e) => ShowNotImplemented("How to Play")));
            contents.Children.Add(CreateMenuButton("Ausrüstung",
                new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                (sender, e) =>
                {
                    _menuPage = MenuPage.Equipment;
                    canvas.Tag = null;
                }));
            contents.Children.Add(CreateMenuButton("Choose Animation",
                new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                (sender, e) =>
                {
                    _menuPage = MenuPage.Animation;
                    canvas.Tag = null;
                }));
            if (field.HasActiveGame && !field.IsGameOver)
            {
                contents.Children.Add(CreateMenuButton("Skilltree",
                    new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                    (sender, e) =>
                    {
                        _menuPage = MenuPage.SkillTree;
                        canvas.Tag = null;
                    }));
                contents.Children.Add(CreateMenuButton("Waffen verbessern",
                    new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                    (sender, e) =>
                    {
                        _menuPageBeforeWeaponUpgrades = MenuPage.Main;
                        _menuPage = MenuPage.WeaponUpgrades;
                        canvas.Tag = null;
                    }));
            }
            contents.Children.Add(CreateMenuButton("Tastenbelegung",
                new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                (sender, e) =>
                {
                    _menuPage = MenuPage.KeyBindings;
                    canvas.Tag = null;
                }));
            contents.Children.Add(CreateMenuButton("High Scores",
                new SolidColorBrush(Color.FromRgb(55, 70, 88)),
                (sender, e) => ShowNotImplemented("High Scores")));
            panel.Child = contents;

            DrawStartMenuPanel(canvas, panel, width, height);
        }

        private static Brush GetWeaponButtonBrush(ArenaField field, Weapon weapon)
        {
            return field.SelectedPlayerWeapon.Id == weapon.Id
                ? new SolidColorBrush(Color.FromRgb(43, 125, 82))
                : new SolidColorBrush(Color.FromRgb(55, 70, 88));
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

        private static Button CreateMenuButton(
            string text, Brush background, RoutedEventHandler onClick,
            bool isEnabled = true)
        {
            Button button = new Button
            {
                Content = text,
                Width = 318,
                Height = 46,
                Margin = new Thickness(0, 5, 0, 5),
                Background = isEnabled
                    ? background
                    : new SolidColorBrush(Color.FromRgb(65, 68, 73)),
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Cursor = isEnabled ? Cursors.Hand : Cursors.Arrow,
                IsEnabled = isEnabled
            };
            if (!isEnabled)
            {
                button.ToolTip = "Waffen-Upgrades sind erst während eines aktiven Runs verfügbar.";
            }
            button.Click += onClick;
            return button;
        }

        private void OpenWeaponUpgrades(
            Canvas canvas, ArenaField field, Weapon weapon, MenuPage returnPage)
        {
            field.SelectPlayerWeapon(weapon);
            _menuPageBeforeWeaponUpgrades = returnPage;
            _menuPage = MenuPage.WeaponUpgrades;
            canvas.Tag = null;
        }

        private enum MenuPage
        {
            Main,
            Equipment,
            Animation,
            SkillTree,
            WeaponUpgrades,
            KeyBindings
        }

        private sealed class AnimationProfile
        {
            public string Name { get; }
            public string[] SpriteSheetPath { get; }
            public string[] AnimationMetadataPath { get; }

            public AnimationProfile(
                string name,
                string[] spriteSheetPath,
                string[] animationMetadataPath)
            {
                Name = name;
                SpriteSheetPath = spriteSheetPath;
                AnimationMetadataPath = animationMetadataPath;
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