//P06.11.2026
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OOPGames
{
    // Draws the arena every frame and keeps the player's world position at screen center.
    public sealed class PainterG9 : IPaintGame2
    {
        private const double PlayerSpriteScale = 2.0;
        private readonly ArenaHudRenderer _hudRenderer = new ArenaHudRenderer();
        private readonly ArenaDebugRenderer _debugRenderer = new ArenaDebugRenderer();
        private readonly ArenaProgressionMenuRenderer _progressionMenuRenderer =
            new ArenaProgressionMenuRenderer();
        private readonly ArenaWeaponWheelRenderer _weaponWheelRenderer =
            new ArenaWeaponWheelRenderer();
        private readonly ArenaWeaponRenderer _weaponRenderer = new ArenaWeaponRenderer();
        private readonly ArenaParticleRenderer _particleRenderer;

        public PainterG9()
        {
            _particleRenderer = new ArenaParticleRenderer(_weaponRenderer);
            _progressionMenuRenderer.PlayMenuClick = () =>
            {
                if (ReferenceEquals(OOPGamesManager.Singleton.ActivePainter, this))
                {
                    _audio.PlayMenuClick();
                }
            };
        }
        private Window _zoomInputWindow;
        private ArenaField _zoomInputField;
        private Canvas _zoomInputCanvas;

        // Reusable brushes avoid creating new colors for every frame.
        private static readonly Brush FloorBrush = new SolidColorBrush(Color.FromRgb(55, 93, 65));
        private static readonly Brush WallBrush = new SolidColorBrush(Color.FromRgb(55, 57, 62));
        private static readonly Brush SlowMotionTintBrush =
            new SolidColorBrush(Color.FromArgb(45, 70, 130, 220));
        private static readonly Brush BulletTimeTintBrush =
            new SolidColorBrush(Color.FromArgb(45, 220, 160, 60));
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
        private readonly ArenaAudio _audio = new ArenaAudio();
        private Canvas _menuClickCanvas;
        private ArenaField _pauseMenuField;
        private ArenaField _menuStateField;
        private MenuPage _menuPage;
        private MenuPage _menuPageBeforeWeaponUpgrades = MenuPage.Main;
        private ArenaAction? _keyBindingCaptureAction;
        private bool _lastGameStarted;
        private double _pauseMenuCanvasWidth;
        private double _pauseMenuCanvasHeight;
        private Canvas _fullScreenMenuCanvas;
        private Border _fullScreenMenuPanel;

        public string Name { get { return "Arena Survival"; } }

        internal void ResetAnimationProfile()
        {
            ResetAnimationProfile(null);
        }

        // Starts with the character of the loaded run (Kurt or BoxingKnight).
        internal void ResetAnimationProfile(string profileName)
        {
            _selectedAnimationProfile = GetAnimationProfile(profileName);
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
            _audio.Attach(field);
            if (field.IsGameStarted)
            {
                // Menus stay free of media players; sounds load once gameplay begins.
                _audio.PreloadAll();
            }
            _audio.Update();
            AttachMenuClickSound(canvas);

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

            DetachFullScreenMenu();
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

            // Draw the walkable area before the walls.
            AddRectangle(worldCanvas, FloorBrush,
                mapLeft + ArenaField.WallThickness,
                mapTop + ArenaField.WallThickness,
                field.WorldWidth - 2 * ArenaField.WallThickness,
                field.WorldHeight - 2 * ArenaField.WallThickness);
            Image backgroundImage = new Image
            {
                Source = field.MapImage,
                Width = field.WorldWidth,
                Height = field.WorldHeight,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(backgroundImage, mapLeft);
            Canvas.SetTop(backgroundImage, mapTop);
            worldCanvas.Children.Add(backgroundImage);

            // Draw the four wall segments over the floor to frame the playable area.
            AddRectangle(worldCanvas, WallBrush, mapLeft, mapTop,
                field.WorldWidth, ArenaField.WallThickness);
            AddRectangle(worldCanvas, WallBrush,
                mapLeft, mapTop + field.WorldHeight - ArenaField.WallThickness,
                field.WorldWidth, ArenaField.WallThickness);
            AddRectangle(worldCanvas, WallBrush, mapLeft, mapTop + ArenaField.WallThickness,
                ArenaField.WallThickness, field.WorldHeight - 2 * ArenaField.WallThickness);
            AddRectangle(worldCanvas, WallBrush,
                mapLeft + field.WorldWidth - ArenaField.WallThickness,
                mapTop + ArenaField.WallThickness, ArenaField.WallThickness,
                field.WorldHeight - 2 * ArenaField.WallThickness);

            _debugRenderer.PaintWorldOverlays(worldCanvas, field, left, top);
            // Particles run on game time; blood stains and ground traces lie under everyone.
            _particleRenderer.Attach(field);
            _particleRenderer.Update(field);
            _particleRenderer.PaintGround(worldCanvas, field, left, top);

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
            if (_playerAnimator.GetFrameCount(animationName) == 0)
            {
                // Characters without attack clips (BoxingKnight) show the idle or walk view of
                // the strike direction instead of the NotFound frame.
                animationName = GetPlayerAnimationName(
                    field.Player.AttackFacing, field.Player.IsMoving, false,
                    field.Player.AttackFacing);
            }
            BitmapSource sprite = _playerAnimator.GetFrame(
                animationName, field.PlayerTimeScale);
            double spriteHeight = field.Player.Size * 2 * PlayerSpriteScale;
            double spriteWidth = spriteHeight * sprite.PixelWidth / sprite.PixelHeight;
            // Weapon frames use the 128-pixel grid of the character frames.
            double unitsPerFramePixel = spriteHeight / ArenaWeaponRenderer.FramePixelSize;
            _weaponRenderer.PaintEntityWeapon(
                worldCanvas, field, field.Player, width / 2, height / 2,
                unitsPerFramePixel, true);
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
            AddDamageFlashOverlay(worldCanvas, field.Player, player);
            _weaponRenderer.PaintEntityWeapon(
                worldCanvas, field, field.Player, width / 2, height / 2,
                unitsPerFramePixel, false);
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
                AddDamageFlashOverlay(worldCanvas, enemy, enemyImage);

                _hudRenderer.PaintStatusPlaceholder(
                    worldCanvas, enemy, enemy.PositionX - left, enemy.PositionY - top);
                _hudRenderer.PaintEntityHealth(worldCanvas, enemy,
                    enemy.PositionX - left, enemy.PositionY - top - enemyHeight / 2 - 10);
            }

            _weaponRenderer.PaintProjectiles(worldCanvas, field, left, top);
            _particleRenderer.PaintAir(worldCanvas, field, left, top);

            canvas.Children.Add(worldCanvas);
            if (field.IsSlowMotionActive)
            {
                // Cool tint over the arena while the Zeitlupe ability slows the tick speed.
                canvas.Children.Add(new Rectangle
                {
                    Width = width,
                    Height = height,
                    Fill = field.IsBulletTimeActive
                        ? BulletTimeTintBrush
                        : SlowMotionTintBrush,
                    IsHitTestVisible = false
                });
            }
            _hudRenderer.PaintPlayerHud(canvas, field, sprite);
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
            
            // Update player's animation profile name for correct hand positioning
            if (field.Player != null)
            {
                field.Player.AnimationProfileName = _selectedAnimationProfile.Name;
            }
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

        // Menu buttons are rebuilt often, so one handler on the canvas catches every click as it
        // bubbles up. The canvas is shared with other games, hence the active-painter check.
        private void AttachMenuClickSound(Canvas canvas)
        {
            if (ReferenceEquals(_menuClickCanvas, canvas))
            {
                return;
            }
            if (_menuClickCanvas != null)
            {
                _menuClickCanvas.RemoveHandler(
                    System.Windows.Controls.Primitives.ButtonBase.ClickEvent,
                    new RoutedEventHandler(OnMenuButtonClick));
            }

            _menuClickCanvas = canvas;
            canvas.AddHandler(
                System.Windows.Controls.Primitives.ButtonBase.ClickEvent,
                new RoutedEventHandler(OnMenuButtonClick), true);
        }

        private void OnMenuButtonClick(object sender, RoutedEventArgs e)
        {
            if (ReferenceEquals(OOPGamesManager.Singleton.ActivePainter, this))
            {
                _audio.PlayMenuClick();
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
            _zoomInputField.SetAimTarget(
                aimX / _zoomInputField.ZoomLevel, aimY / _zoomInputField.ZoomLevel);
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

            // The camera centers the player; dividing by the zoom gives world units.
            _zoomInputField.SetAimTarget(
                (mousePosition.X - _zoomInputCanvas.ActualWidth / 2) / _zoomInputField.ZoomLevel,
                (mousePosition.Y - _zoomInputCanvas.ActualHeight / 2) / _zoomInputField.ZoomLevel);
        }

        private void OnZoomKeyDown(object sender, KeyEventArgs e)
        {
            if (_menuPage == MenuPage.DevTools)
            {
                return;
            }

            if (IsTextEntryFocused())
            {
                return;
            }

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
            if (_menuPage == MenuPage.DevTools)
            {
                return;
            }

            if (_zoomInputField != null && _zoomInputField.HandleGameSpeedWheel(e.Delta))
            {
                e.Handled = true;
                return;
            }
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
            if (_menuPage == MenuPage.DevTools)
            {
                return;
            }

            if (IsTextEntryFocused())
            {
                return;
            }
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

        private static bool IsTextEntryFocused()
        {
            DependencyObject focusedElement =
                Keyboard.FocusedElement as DependencyObject;
            while (focusedElement != null)
            {
                if (focusedElement
                    is System.Windows.Controls.Primitives.TextBoxBase)
                {
                    return true;
                }

                DependencyObject parent =
                    focusedElement is Visual || focusedElement is Visual3D
                        ? VisualTreeHelper.GetParent(focusedElement)
                        : null;
                focusedElement = parent ?? LogicalTreeHelper.GetParent(focusedElement);
            }
            return false;
        }

        private void OnZoomWindowClosed(object sender, EventArgs e)
        {
            // Closing the program saves the running game.
            _zoomInputField?.SaveNow();
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
                        return isMoving ? "WalkAttackBack" : "IdleAttackBack";
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
        internal static string FindAssetPath(params string[] relativePath)
        {
            string path = FindAssetLocation(relativePath, File.Exists);
            if (path != null)
            {
                return path;
            }

            throw new FileNotFoundException(
                "Could not locate an asset under Classes\\Gruppe 9\\Assets.",
                String.Join(System.IO.Path.DirectorySeparatorChar.ToString(), relativePath));
        }

        // Locates an asset folder such as Sounds the same way as single asset files.
        internal static string FindAssetDirectory(params string[] relativePath)
        {
            string path = FindAssetLocation(relativePath, Directory.Exists);
            if (path != null)
            {
                return path;
            }

            throw new DirectoryNotFoundException(
                "Could not locate the asset folder Classes\\Gruppe 9\\Assets\\"
                + String.Join("\\", relativePath) + ".");
        }

        private static string FindAssetLocation(
            string[] relativePath, Func<string, bool> exists)
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
                        if (exists(path))
                        {
                            return path;
                        }
                    }
                    directory = directory.Parent;
                }
            }

            return null;
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
            if (_menuPage == MenuPage.DevTools)
            {
                DrawStartMenuFullScreen(canvas,
                    new ArenaDevToolsControl(field, () =>
                    {
                        _menuPage = MenuPage.Main;
                        canvas.Tag = null;
                    }),
                    width, height);
                return;
            }

            if (_menuPage == MenuPage.SkillTree)
            {
                Grid skillTreePage = new Grid();
                skillTreePage.RowDefinitions.Add(
                    new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                skillTreePage.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                skillTreePage.Children.Add(_progressionMenuRenderer.CreateSkillTree(
                    field, () => canvas.Tag = null,
                    Math.Max(0, width - 48), Math.Max(0, height - 48)));
                Button backButton = CreateMenuButton("Back",
                    () =>
                    {
                        _menuPage = MenuPage.Main;
                        canvas.Tag = null;
                    });
                Grid.SetRow(backButton, 1);
                skillTreePage.Children.Add(backButton);
                DrawStartMenuFullScreen(canvas, skillTreePage, width, height);
                return;
            }

            if (_menuPage == MenuPage.WeaponUpgrades)
            {
                contents.Children.Add(_progressionMenuRenderer.CreateWeaponUpgrades(
                    field, () => canvas.Tag = null));
                contents.Children.Add(CreateMenuButton("Back",
                    () =>
                    {
                        _menuPage = _menuPageBeforeWeaponUpgrades;
                        canvas.Tag = null;
                    }));
                panel.Child = contents;
                DrawStartMenuPanel(canvas, panel, width, height);
                return;
            }

            if (_menuPage == MenuPage.SaveGames)
            {
                AddSaveGamePage(canvas, field, contents);
                panel.Child = contents;
                DrawStartMenuPanel(canvas, panel, width, height);
                return;
            }

            if (_menuPage == MenuPage.HighScores)
            {
                AddHighScorePage(canvas, field, contents);
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
                    () =>
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
                    // Split row: left selects the weapon, right opens its upgrade tree.
                    ArenaUiButton selectButton = new ArenaUiButton(
                        weapon.Name + " — " + weapon.AttackPattern.Name,
                        () =>
                        {
                            field.SelectPlayerWeapon(weapon);
                            canvas.Tag = null;
                        },
                        ArenaUiButtonShape.SplitLong)
                    {
                        IsChosen = field.SelectedPlayerWeapon.Id == weapon.Id,
                        ToolTip = weapon.Effect.Description,
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold
                    };
                    bool canUpgrade = field.HasActiveGame && !field.IsGameOver;
                    ArenaUiButton treeButton = new ArenaUiButton(
                        "Baum",
                        () => OpenWeaponUpgrades(
                            canvas, field, weapon, MenuPage.Equipment),
                        ArenaUiButtonShape.SplitShort)
                    {
                        IsEnabled = canUpgrade,
                        ToolTip = canUpgrade
                            ? weapon.Name + " — Waffenbaum"
                            : "Waffen-Upgrades sind erst während eines aktiven Runs verfügbar.",
                        FontSize = 14,
                        FontWeight = FontWeights.SemiBold
                    };
                    System.Windows.Automation.AutomationProperties.SetName(
                        treeButton, weapon.Name + " — Waffenbaum");
                    contents.Children.Add(ArenaUiButton.CreateSplitRow(
                        selectButton, treeButton, MenuButtonWidth, MenuButtonHeight));
                }
                contents.Children.Add(CreateMenuButton("Back",
                    () =>
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
                    () => SelectAnimationProfile(canvas, BoxingKnightProfile),
                    isChosen: IsSelectedAnimationProfile(BoxingKnightProfile)));
                contents.Children.Add(CreateMenuButton("Kurt (mit Schlaganimation)",
                    () => SelectAnimationProfile(canvas, KurtProfile),
                    isChosen: IsSelectedAnimationProfile(KurtProfile)));
                contents.Children.Add(CreateMenuButton("Back",
                    () =>
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
                    ? "Du bist gestorben (Tode: " + field.Deaths + ")"
                    : field.HasActiveGame ? "Your game is paused" : "Choose equipment and start",
                FontSize = 15,
                Foreground = new SolidColorBrush(Color.FromRgb(183, 197, 212)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });
            contents.Children.Add(CreateMenuButton(
                isGameOver
                    ? "Respawnen (−" + ArenaField.DeathSkillPointPenalty + " Skill-, −"
                        + ArenaField.DeathWeaponPointPenalty + " Waffenpunkte)"
                    : field.HasActiveGame ? "Spiel fortsetzen" : "Spiel starten",
                () =>
                {
                    if (isGameOver)
                    {
                        field.Respawn();
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
            if (isGameOver)
            {
                contents.Children.Add(CreateMenuButton("Neuer Run",
                    () => field.RestartGame()));
            }
            contents.Children.Add(CreateMenuButton("Spielstände",
                () =>
                {
                    _menuPage = MenuPage.SaveGames;
                    canvas.Tag = null;
                }));
            contents.Children.Add(CreateMenuButton("How to Play",
                () => ShowNotImplemented("How to Play")));
            contents.Children.Add(CreateMenuButton("Ausrüstung",
                () =>
                {
                    _menuPage = MenuPage.Equipment;
                    canvas.Tag = null;
                }));
            contents.Children.Add(CreateMenuButton("Choose Animation",
                () =>
                {
                    _menuPage = MenuPage.Animation;
                    canvas.Tag = null;
                }));
            contents.Children.Add(CreateMenuButton("Dev Tool",
                () =>
                {
                    _menuPage = MenuPage.DevTools;
                    canvas.Tag = null;
                }));
            if (field.HasActiveGame && !field.IsGameOver)
            {
                // Split row: left toggles godmode, right buys every upgrade (godmode only).
                ArenaUiButton godModeButton = new ArenaUiButton(
                    "Godmode: " + (field.IsGodMode ? "AN" : "AUS")
                        + " — " + (field.IsGodMode ? "deaktivieren" : "aktivieren"),
                    () =>
                    {
                        field.ToggleGodMode();
                        canvas.Tag = null;
                    },
                    ArenaUiButtonShape.SplitLong)
                {
                    IsChosen = field.IsGodMode,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    ToolTip = "Im Godmode: T + Mausrad ändert das Spieltempo."
                };
                ArenaUiButton buyAllButton = new ArenaUiButton(
                    "Alles",
                    () =>
                    {
                        field.PurchaseAllUpgrades();
                        canvas.Tag = null;
                    },
                    ArenaUiButtonShape.SplitShort)
                {
                    IsEnabled = field.IsGodMode,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    ToolTip = field.IsGodMode
                        ? "Alle Skills und Waffen-Upgrades vollständig kaufen."
                        : "Nur im Godmode verfügbar."
                };
                System.Windows.Automation.AutomationProperties.SetName(
                    buyAllButton, "Alle Upgrades kaufen");
                contents.Children.Add(ArenaUiButton.CreateSplitRow(
                    godModeButton, buyAllButton, MenuButtonWidth, MenuButtonHeight));
                contents.Children.Add(CreateMenuButton("Skilltree",
                    () =>
                    {
                        _menuPage = MenuPage.SkillTree;
                        canvas.Tag = null;
                    }));
                // Weapon trees are reached from the equipment selection (tree button per weapon).
            }
            contents.Children.Add(CreateMenuButton("Tastenbelegung",
                () =>
                {
                    _menuPage = MenuPage.KeyBindings;
                    canvas.Tag = null;
                }));
            contents.Children.Add(CreateMenuButton("High Scores",
                () =>
                {
                    _menuPage = MenuPage.HighScores;
                    canvas.Tag = null;
                }));
            panel.Child = contents;

            DrawStartMenuPanel(canvas, panel, width, height);
        }

        // A hit entity lights up itself: a white copy of its own sprite, shaped by the
        // sprite's alpha, lies on top of it and fades out with the damage flash.
        private const double MaximumDamageFlashOpacity = 0.85;

        private static void AddDamageFlashOverlay(Canvas canvas, Entity entity, Image sprite)
        {
            double intensity = entity.DamageFlashIntensity;
            if (intensity <= 0 || sprite.Source == null)
            {
                return;
            }

            Rectangle glow = new Rectangle
            {
                Width = sprite.Width,
                Height = sprite.Height,
                Fill = Brushes.White,
                OpacityMask = new ImageBrush(sprite.Source) { Stretch = sprite.Stretch },
                Opacity = intensity * MaximumDamageFlashOpacity,
                RenderTransformOrigin = sprite.RenderTransformOrigin,
                RenderTransform = sprite.RenderTransform,
                IsHitTestVisible = false
            };
            RenderOptions.SetBitmapScalingMode(glow, BitmapScalingMode.NearestNeighbor);
            Canvas.SetLeft(glow, Canvas.GetLeft(sprite));
            Canvas.SetTop(glow, Canvas.GetTop(sprite));
            canvas.Children.Add(glow);
        }

        private void DrawStartMenuPanel(Canvas canvas, Border panel, double width, double height)
        {
            DetachFullScreenMenu();
            panel.Measure(new Size(width, height));
            Canvas.SetLeft(panel, (width - panel.DesiredSize.Width) / 2);
            Canvas.SetTop(panel, (height - panel.DesiredSize.Height) / 2);
            canvas.Children.Add(panel);
            canvas.Tag = StartMenuTag;
        }

        private void DrawStartMenuFullScreen(
            Canvas canvas, UIElement content, double width, double height)
        {
            DetachFullScreenMenu();
            Border panel = new Border
            {
                Width = Math.Max(0, width - 24),
                Height = Math.Max(0, height - 24),
                Padding = new Thickness(12),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                BorderBrush = new SolidColorBrush(Color.FromRgb(77, 94, 113)),
                Background = new SolidColorBrush(Color.FromRgb(27, 38, 52)),
                Child = content
            };
            Canvas.SetLeft(panel, 12);
            Canvas.SetTop(panel, 12);
            canvas.Children.Add(panel);
            _fullScreenMenuCanvas = canvas;
            _fullScreenMenuPanel = panel;
            canvas.SizeChanged += OnFullScreenMenuCanvasSizeChanged;
            canvas.Tag = StartMenuTag;
        }

        private void OnFullScreenMenuCanvasSizeChanged(
            object sender, SizeChangedEventArgs e)
        {
            if (!ReferenceEquals(sender, _fullScreenMenuCanvas)
                || _fullScreenMenuPanel == null)
            {
                return;
            }
            _fullScreenMenuPanel.Width = Math.Max(0, e.NewSize.Width - 24);
            _fullScreenMenuPanel.Height = Math.Max(0, e.NewSize.Height - 24);
        }

        private void DetachFullScreenMenu()
        {
            if (_fullScreenMenuCanvas != null)
            {
                _fullScreenMenuCanvas.SizeChanged -= OnFullScreenMenuCanvasSizeChanged;
            }
            _fullScreenMenuCanvas = null;
            _fullScreenMenuPanel = null;
        }

        private bool IsSelectedAnimationProfile(AnimationProfile profile)
        {
            return ReferenceEquals(profile, _selectedAnimationProfile);
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

        private const double MenuButtonWidth = 318;
        private const double MenuButtonHeight = 46;

        // Menu buttons use the UiButtons sprite sheet; chosen entries show the selected frame.
        private static ArenaUiButton CreateMenuButton(
            string text, Action onClick, bool isEnabled = true, bool isChosen = false)
        {
            return new ArenaUiButton(text, onClick)
            {
                Width = MenuButtonWidth,
                Height = MenuButtonHeight,
                Margin = new Thickness(0, 5, 0, 5),
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                IsEnabled = isEnabled,
                IsChosen = isChosen
            };
        }

        private void OpenWeaponUpgrades(
            Canvas canvas, ArenaField field, Weapon weapon, MenuPage returnPage)
        {
            field.SelectPlayerWeapon(weapon);
            _menuPageBeforeWeaponUpgrades = returnPage;
            _menuPage = MenuPage.WeaponUpgrades;
            canvas.Tag = null;
        }

        private static readonly Brush MenuTextBrush =
            new SolidColorBrush(Color.FromRgb(183, 197, 212));
        private static readonly Brush HighlightTextBrush =
            new SolidColorBrush(Color.FromRgb(255, 214, 102));

        private static TextBlock CreateMenuText(
            string text, double fontSize, Brush foreground, bool bold = false)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                Foreground = foreground,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 6)
            };
        }

        // Five save slots: a used slot can be loaded or deleted, an empty one starts a new run.
        private void AddSaveGamePage(Canvas canvas, ArenaField field, StackPanel contents)
        {
            contents.Children.Add(CreateMenuText("SPIELSTÄNDE", 24, Brushes.White, true));
            contents.Children.Add(CreateMenuText(
                "Das laufende Spiel speichert sich automatisch (Pause, Menü, Beenden, alle "
                    + (int)ArenaField.AutosaveIntervalSeconds + " s).",
                13, MenuTextBrush));
            if (field.SaveGames == null)
            {
                contents.Children.Add(CreateMenuText(
                    "Speichern ist hier nicht verfügbar.", 14, MenuTextBrush));
            }
            else
            {
                if (field.IsRunUnsaved)
                {
                    contents.Children.Add(CreateMenuText(
                        "Alle Slots sind belegt: Der laufende Run wird erst gespeichert, "
                            + "wenn du einen Spielstand löschst.",
                        13, HighlightTextBrush));
                }
                for (int slot = 0; slot < ArenaSaveGameRepository.SlotCount; slot++)
                {
                    AddSaveSlotRow(canvas, field, contents, slot);
                }
            }
            contents.Children.Add(CreateMenuButton("Back",
                () =>
                {
                    _menuPage = MenuPage.Main;
                    canvas.Tag = null;
                }));
        }

        private void AddSaveSlotRow(
            Canvas canvas, ArenaField field, StackPanel contents, int slot)
        {
            ArenaSaveGame game = field.SaveGames.Load(slot);
            bool isActive = field.HasActiveGame && field.ActiveSaveSlot == slot;
            string title = ArenaSaveGameRepository.GetSlotName(slot)
                + (isActive ? " (aktuelles Spiel)" : string.Empty);
            string details = game == null
                ? "leer"
                : "Score " + ArenaScoreboard.GetScore(game.Score, game.Deaths)
                    + " | " + game.Score + " Kills | " + game.Deaths + " Tode | "
                    + GetWeaponName(game.SelectedWeaponId) + " | "
                    + game.SavedAt.ToString("dd.MM. HH:mm")
                    + (game.Player != null && game.Player.HitPoints <= 0 ? " | tot" : string.Empty);
            contents.Children.Add(CreateMenuText(
                title, 15, isActive ? HighlightTextBrush : Brushes.White, true));
            contents.Children.Add(CreateMenuText(details, 13, MenuTextBrush));

            if (game == null)
            {
                contents.Children.Add(CreateMenuButton("Neues Spiel in " + title,
                    () =>
                    {
                        // The running game is saved first, then the new run takes this slot.
                        field.SaveNow();
                        field.StartNewGameInSlot(slot);
                        _menuPage = MenuPage.Main;
                        canvas.Tag = null;
                    }));
                return;
            }

            ArenaUiButton load = new ArenaUiButton("Laden",
                () =>
                {
                    if (field.ActiveSaveSlot != slot)
                    {
                        field.SaveNow();
                    }
                    if (field.LoadSlot(slot))
                    {
                        ResetAnimationProfile(field.Player.AnimationProfileName);
                    }
                    _menuPage = MenuPage.Main;
                    canvas.Tag = null;
                },
                ArenaUiButtonShape.SplitLong)
            {
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            };
            System.Windows.Automation.AutomationProperties.SetName(
                load, "Laden " + ArenaSaveGameRepository.GetSlotName(slot));
            ArenaUiButton delete = new ArenaUiButton("Löschen",
                () =>
                {
                    if (MessageBox.Show(
                        ArenaSaveGameRepository.GetSlotName(slot) + " wirklich löschen?",
                        "Spielstand löschen", MessageBoxButton.YesNo,
                        MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    {
                        field.DeleteSaveSlot(slot);
                    }
                    canvas.Tag = null;
                },
                ArenaUiButtonShape.SplitShort)
            {
                FontSize = 14,
                FontWeight = FontWeights.SemiBold
            };
            System.Windows.Automation.AutomationProperties.SetName(
                delete, "Löschen " + ArenaSaveGameRepository.GetSlotName(slot));
            contents.Children.Add(ArenaUiButton.CreateSplitRow(
                load, delete, MenuButtonWidth, MenuButtonHeight));
        }

        // Best runs by score (kills minus a penalty per death); the running run is highlighted.
        private void AddHighScorePage(Canvas canvas, ArenaField field, StackPanel contents)
        {
            contents.Children.Add(CreateMenuText("HIGH SCORES", 24, Brushes.White, true));
            contents.Children.Add(CreateMenuText(
                "Score = Kills − " + ArenaScoreboard.DeathPenalty
                    + " je Tod. Runs im Godmode zählen nicht.",
                13, MenuTextBrush));
            IReadOnlyList<ArenaScoreEntry> entries = field.Scoreboard == null
                ? new List<ArenaScoreEntry>()
                : field.Scoreboard.Entries;
            if (entries.Count == 0)
            {
                contents.Children.Add(CreateMenuText("Noch keine Einträge.", 14, MenuTextBrush));
            }
            bool currentListed = false;
            for (int index = 0; index < entries.Count; index++)
            {
                ArenaScoreEntry entry = entries[index];
                bool isCurrent = field.HasActiveGame && entry.RunId == field.RunId;
                currentListed |= isCurrent;
                contents.Children.Add(CreateMenuText(
                    (index + 1) + ". " + entry.Score + " Punkte — " + entry.Label
                        + (isCurrent ? " (aktuell)" : string.Empty)
                        + "\n    " + entry.Kills + " Kills, " + entry.Deaths + " Tode, "
                        + entry.Date.ToString("dd.MM.yy HH:mm"),
                    14, isCurrent ? HighlightTextBrush : Brushes.White, isCurrent));
            }
            if (field.HasActiveGame && !currentListed)
            {
                contents.Children.Add(CreateMenuText(
                    "Aktueller Run: " + field.TotalScore + " Punkte ("
                        + field.Score + " Kills, " + field.Deaths + " Tode)"
                        + (field.IsGodMode ? " — Godmode, zählt nicht" : string.Empty),
                    14, HighlightTextBrush));
            }
            contents.Children.Add(CreateMenuButton("Back",
                () =>
                {
                    _menuPage = MenuPage.Main;
                    canvas.Tag = null;
                }));
        }

        private static string GetWeaponName(string weaponId)
        {
            if (weaponId == WeaponCatalog.MagicWand.Id) return WeaponCatalog.MagicWand.Name;
            if (weaponId == WeaponCatalog.Bow.Id) return WeaponCatalog.Bow.Name;
            return WeaponCatalog.Sword.Name;
        }

        private static AnimationProfile GetAnimationProfile(string profileName)
        {
            return profileName == KurtProfile.Name ? KurtProfile : BoxingKnightProfile;
        }

        private enum MenuPage
        {
            Main,
            Equipment,
            Animation,
            SkillTree,
            DevTools,
            WeaponUpgrades,
            KeyBindings,
            SaveGames,
            HighScores
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

    }
}