//P06.11.2026
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private const double PlayerAttackFlashDurationMilliseconds = 80;
        private const double PlayerAttackFlashGapMilliseconds = 40;
        private readonly Queue<Point> _playerAttackFlashQueue = new Queue<Point>();
        private Window _zoomInputWindow;
        private ArenaField _zoomInputField;
        private Canvas _zoomInputCanvas;
        private PlayerCharacter _attackFlashPlayer;
        private Point _activePlayerAttackDirection;
        private long _activePlayerAttackTimestamp;
        private long _nextPlayerAttackFlashTimestamp;
        private bool _isPlayerAttackFlashActive;

        // Reusable brushes avoid creating new colors for every frame.
        private static readonly Brush FloorBrush = new SolidColorBrush(Color.FromRgb(55, 93, 65));
        private static readonly Brush GridBrush = new SolidColorBrush(Color.FromRgb(68, 108, 78));
        private static readonly Brush WallBrush = new SolidColorBrush(Color.FromRgb(55, 57, 62));
        private static readonly Brush PlayerDamageRangeBrush =
            new SolidColorBrush(Color.FromArgb(65, 110, 190, 255));
        private static readonly Brush PlayerAttackFlashBrush =
            new SolidColorBrush(Color.FromArgb(120, 110, 190, 255));
        private static readonly Brush PlayerAttackFlashStrokeBrush =
            new SolidColorBrush(Color.FromArgb(240, 220, 245, 255));
        private static readonly Brush EnemyDamageRangeBrush =
            new SolidColorBrush(Color.FromArgb(60, 245, 70, 70));
        private static readonly Brush EntityHitboxFillBrush =
            new SolidColorBrush(Color.FromArgb(35, 144, 238, 144));
        private static readonly Brush EntityHitboxStrokeBrush =
            new SolidColorBrush(Color.FromArgb(240, 144, 238, 144));
        private static readonly Brush EnemySightRangeFillBrush =
            new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));
        private static readonly Brush EnemySightRangeStrokeBrush =
            new SolidColorBrush(Color.FromArgb(210, 255, 255, 255));
        private static readonly Brush EnemyPathBrush =
            new SolidColorBrush(Color.FromArgb(230, 255, 215, 0));
        private static readonly Brush EnemyAttackFlashBrush =
            new SolidColorBrush(Color.FromArgb(40, 255, 40, 40));
        private static readonly Brush EnemyAttackFlashStroke =
            new SolidColorBrush(Color.FromArgb(220, 255, 70, 50));
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
        private bool _showAnimationMenu;
        private double _pauseMenuCanvasWidth;
        private double _pauseMenuCanvasHeight;

        public string Name { get { return "Arena Survival"; } }

        internal void ResetAnimationProfile()
        {
            _selectedAnimationProfile = BoxingKnightProfile;
            _playerAnimator = null;
            _playerAnimatorProfile = null;
            _enemyAnimator = null;
            _showAnimationMenu = false;
            ClearPlayerAttackFlashes();
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

            if (!field.IsGameStarted && !field.HasActiveGame)
            {
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

            if (field.ShowDamageRanges)
            {
                foreach (PursuerEnemy enemy in field.Enemies)
                {
                    if (enemy.IsAlive)
                    {
                        AddPlannedEnemyPath(worldCanvas, enemy, field, left, top);
                        AddSightRange(worldCanvas,
                            enemy.PositionX - left, enemy.PositionY - top,
                            PursuerEnemy.SightRange, false);
                        if (enemy.HasSeenPlayer)
                        {
                            AddSightRange(worldCanvas,
                                enemy.PositionX - left, enemy.PositionY - top,
                                enemy.CurrentSightRange, true);
                        }
                    }
                }

                AddDamageCone(worldCanvas, PlayerDamageRangeBrush,
                    field.Player.PositionX - left, field.Player.PositionY - top,
                    ArenaField.PlayerAttackRange,
                    field.AimDirectionX, field.AimDirectionY,
                    ArenaField.PlayerAttackHalfAngleRadians);
                if (TryGetPlayerAttackFlash(field.Player, out Point attackDirection))
                {
                    AddDamageCone(worldCanvas, PlayerAttackFlashBrush,
                        field.Player.PositionX - left, field.Player.PositionY - top,
                        ArenaField.PlayerAttackRange,
                        attackDirection.X, attackDirection.Y,
                        ArenaField.PlayerAttackHalfAngleRadians,
                        PlayerAttackFlashStrokeBrush);
                }
            }

            string animationName = GetPlayerAnimationName(
                field.Player.Facing, field.Player.IsMoving,
                field.Player.IsAttacking, field.Player.AttackFacing);
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

            foreach (PursuerEnemy enemy in field.Enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                if (field.ShowDamageRanges)
                {
                    AddDamageRange(worldCanvas, EnemyDamageRangeBrush,
                        enemy.PositionX - left, enemy.PositionY - top,
                        PursuerEnemy.AttackRange);
                }

                if (_enemyAnimator == null)
                {
                    _enemyAnimator = new SpriteSheetAnimator(
                        FindAssetPath(EnemySpriteSheetPath),
                        FindAssetPath(EnemyAnimationMetadataPath));
                }

                BitmapSource enemyFrame = _enemyAnimator.GetFrame(
                    GetEnemyAnimationName(enemy));
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
                            ? (enemy.IsAttacking ? -1.2 : -1)
                            : (enemy.IsAttacking ? 1.2 : 1),
                        enemy.IsAttacking ? 1.2 : 1),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(enemyImage,
                    enemy.PositionX - left - enemyWidth / 2);
                Canvas.SetTop(enemyImage,
                    enemy.PositionY - top - enemyHeight / 2);
                worldCanvas.Children.Add(enemyImage);

                if (enemy.IsAttacking)
                {
                    PaintEnemyAttackEffect(worldCanvas, enemy,
                        enemy.PositionX - left, enemy.PositionY - top);
                }

                PaintEnemyHealth(worldCanvas, enemy,
                    enemy.PositionX - left, enemy.PositionY - top - enemyHeight / 2 - 10);
            }

            if (field.ShowDamageRanges)
            {
                AddEntityHitbox(worldCanvas,
                    field.Player.PositionX - left, field.Player.PositionY - top,
                    field.Player.Size);
                foreach (PursuerEnemy enemy in field.Enemies)
                {
                    if (enemy.IsAlive)
                    {
                        AddEntityHitbox(worldCanvas,
                            enemy.PositionX - left, enemy.PositionY - top, enemy.Size);
                    }
                }
            }

            canvas.Children.Add(worldCanvas);
            PaintPlayerHealth(canvas, field.Player, field.Score);
            if (field.ShowDamageRanges)
            {
                PaintLegend(canvas, width, height);
            }
            if (field.ShowDebugHud)
            {
                PaintDebugHud(canvas, field, width, height);
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

        private static void PaintPlayerHealth(Canvas canvas, PlayerCharacter player, int score)
        {
            const double width = 220;
            const double height = 26;
            Grid healthBar = new Grid
            {
                Width = width,
                Height = height,
                ClipToBounds = true
            };
            healthBar.Children.Add(new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromRgb(40, 45, 50))
            });
            healthBar.Children.Add(new Rectangle
            {
                Width = width * player.CurrentHitPoints / player.MaxHitPoints,
                HorizontalAlignment = HorizontalAlignment.Left,
                Fill = player.CurrentHitPoints > player.MaxHitPoints / 3
                    ? Brushes.LimeGreen
                    : Brushes.IndianRed
            });
            healthBar.Children.Add(new TextBlock
            {
                Text = "HP: " + player.CurrentHitPoints + " / " + player.MaxHitPoints,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            });

            Border border = new Border
            {
                Width = width,
                Height = height,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.White,
                Child = healthBar,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(border, 16);
            Canvas.SetTop(border, 16);
            canvas.Children.Add(border);

            Border scoreLabel = new Border
            {
                Padding = new Thickness(8, 3, 8, 3),
                Background = new SolidColorBrush(Color.FromArgb(210, 40, 45, 50)),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1),
                Child = new TextBlock
                {
                    Text = "Kills: " + score,
                    Foreground = Brushes.White,
                    FontWeight = FontWeights.Bold
                },
                IsHitTestVisible = false
            };
            Canvas.SetLeft(scoreLabel, width + 28);
            Canvas.SetTop(scoreLabel, 16);
            canvas.Children.Add(scoreLabel);
        }

        private static void AddDamageRange(
            Canvas canvas, Brush fill, double centerX, double centerY, double radius)
        {
            double diameter = radius * 2;
            Ellipse range = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                Fill = fill,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(range, centerX - radius);
            Canvas.SetTop(range, centerY - radius);
            canvas.Children.Add(range);
        }

        private static void AddEntityHitbox(
            Canvas canvas, double centerX, double centerY, double size)
        {
            double diameter = size;
            Ellipse hitbox = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                Fill = EntityHitboxFillBrush,
                Stroke = EntityHitboxStrokeBrush,
                StrokeThickness = 2,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(hitbox, centerX - diameter / 2);
            Canvas.SetTop(hitbox, centerY - diameter / 2);
            canvas.Children.Add(hitbox);
        }

        private static void AddSightRange(
            Canvas canvas, double centerX, double centerY, double radius, bool dashed)
        {
            double diameter = radius * 2;
            Ellipse range = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                Fill = dashed ? Brushes.Transparent : EnemySightRangeFillBrush,
                Stroke = EnemySightRangeStrokeBrush,
                StrokeThickness = 2,
                IsHitTestVisible = false
            };
            if (dashed)
            {
                range.StrokeDashArray = new DoubleCollection { 5, 3 };
            }
            Canvas.SetLeft(range, centerX - radius);
            Canvas.SetTop(range, centerY - radius);
            canvas.Children.Add(range);
        }

        private static void AddPlannedEnemyPath(
            Canvas canvas, PursuerEnemy enemy, ArenaField field, double left, double top)
        {
            Point start = new Point(enemy.PositionX - left, enemy.PositionY - top);
            Point destination = enemy.GetPlannedDestination(field);
            Point end = new Point(destination.X - left, destination.Y - top);
            if (start == end)
            {
                return;
            }

            canvas.Children.Add(new Polyline
            {
                Points = new PointCollection { start, end },
                Stroke = EnemyPathBrush,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 5, 3 },
                IsHitTestVisible = false
            });

            const double markerSize = 6;
            Ellipse destinationMarker = new Ellipse
            {
                Width = markerSize,
                Height = markerSize,
                Fill = EnemyPathBrush,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(destinationMarker, end.X - markerSize / 2);
            Canvas.SetTop(destinationMarker, end.Y - markerSize / 2);
            canvas.Children.Add(destinationMarker);
        }

        private static void PaintDebugHud(Canvas canvas, ArenaField field,
            double canvasWidth, double canvasHeight)
        {
            StackPanel content = new StackPanel();
            AddDebugText(content, "DEBUG-STATISTIKEN", true);
            AddDebugText(content, string.Format(
                "Position: X {0:0}, Y {1:0}",
                field.Player.PositionX, field.Player.PositionY));
            AddDebugText(content, string.Format(
                "Spielgeschwindigkeit: {0:0.0} / {1:0.0}",
                field.Player.currentSpeed, field.Player.MaxSpeed));
            AddDebugText(content, "Blickrichtung: " + GetFacingLabel(field.Player.Facing));
            double aimAngle = Math.Atan2(field.AimDirectionY, field.AimDirectionX)
                * 180 / Math.PI;
            AddDebugText(content, string.Format("Zielwinkel: {0:0}°", aimAngle));
            AddDebugText(content, string.Format(
                "Spielerschaden: {0} je Schlag", ArenaField.PlayerAttackDamage));
            AddDebugText(content, string.Format(
                "Gegnerschaden: {0} je {1:0} s in Reichweite",
                PursuerEnemy.AttackDamagePerHit,
                PursuerEnemy.AttackIntervalMilliseconds / 1000.0));
            AddDebugText(content, string.Format(
                "Gegner: {0}   Kills: {1}", field.Enemies.Count, field.Score));
            AddDebugText(content, string.Format(
                "Angriffsreichweite: {0:0}", ArenaField.PlayerAttackRange));
            AddDebugText(content, string.Format(
                "Gegner-Angriffsreichweite: {0:0}", PursuerEnemy.AttackRange));
            AddDebugText(content, string.Format(
                "Sichtweite: {0:0} (+{1:0} nach Erkennen)",
                PursuerEnemy.SightRange, PursuerEnemy.SightRangeAfterDetectionBonus));

            Border panel = CreateDebugPanel(content, canvasWidth, canvasHeight);
            Canvas.SetLeft(panel, 16);
            Canvas.SetTop(panel, 52);
            canvas.Children.Add(panel);
        }

        private static void PaintLegend(Canvas canvas, double canvasWidth, double canvasHeight)
        {
            StackPanel content = new StackPanel();
            AddDebugText(content, "LEGENDE", true);
            AddLegendItem(content, PlayerDamageRangeBrush, "Spieler-Angriffsbereich");
            AddLegendItem(content, EnemyDamageRangeBrush, "Gegner-Angriffsbereich");
            AddLegendItem(content, EntityHitboxStrokeBrush, "Hitboxen");
            AddLegendItem(content, EnemySightRangeStrokeBrush, "Gegner-Sichtweite");
            AddLegendItem(content, EnemyPathBrush, "Geplanter Gegnerweg");

            Border panel = CreateDebugPanel(content, canvasWidth, canvasHeight);
            Canvas.SetLeft(panel, Math.Max(16, canvasWidth - panel.DesiredSize.Width - 16));
            Canvas.SetTop(panel, 52);
            canvas.Children.Add(panel);
        }

        private static Border CreateDebugPanel(StackPanel content,
            double canvasWidth, double canvasHeight)
        {
            Border panel = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(220, 30, 34, 38)),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1),
                Padding = new Thickness(8),
                Child = content,
                IsHitTestVisible = false
            };
            panel.Measure(new Size(Math.Max(0, canvasWidth - 32),
                Math.Max(0, canvasHeight - 60)));
            return panel;
        }

        private static void AddDebugText(StackPanel panel, string text, bool isHeader = false)
        {
            panel.Children.Add(new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontWeight = isHeader ? FontWeights.Bold : FontWeights.Normal,
                Margin = new Thickness(0, 1, 0, 1)
            });
        }

        private static void AddLegendItem(StackPanel panel, Brush color, string label)
        {
            DockPanel row = new DockPanel { LastChildFill = true };
            Rectangle swatch = new Rectangle
            {
                Width = 12,
                Height = 12,
                Fill = color,
                Stroke = Brushes.White,
                StrokeThickness = 0.5,
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            DockPanel.SetDock(swatch, Dock.Left);
            row.Children.Add(swatch);
            row.Children.Add(new TextBlock
            {
                Text = label,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center
            });
            panel.Children.Add(row);
        }

        private static string GetFacingLabel(PlayerFacing facing)
        {
            switch (facing)
            {
                case PlayerFacing.Back:
                    return "Hinten";
                case PlayerFacing.Left:
                    return "Links";
                case PlayerFacing.Right:
                    return "Rechts";
                default:
                    return "Vorne";
            }
        }

        private static void AddDamageCone(
            Canvas canvas, Brush fill, double centerX, double centerY, double radius,
            double directionX, double directionY, double halfAngle, Brush stroke = null)
        {
            double direction = Math.Atan2(directionY, directionX);
            Point center = new Point(centerX, centerY);
            Point start = new Point(
                centerX + radius * Math.Cos(direction - halfAngle),
                centerY + radius * Math.Sin(direction - halfAngle));
            Point end = new Point(
                centerX + radius * Math.Cos(direction + halfAngle),
                centerY + radius * Math.Sin(direction + halfAngle));

            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(center, true, true);
                context.LineTo(start, true, false);
                context.ArcTo(end, new Size(radius, radius), 0, false,
                    SweepDirection.Clockwise, true, false);
            }
            geometry.Freeze();

            System.Windows.Shapes.Path cone = new System.Windows.Shapes.Path
            {
                Data = geometry,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = stroke == null ? 0 : 2,
                IsHitTestVisible = false
            };
            canvas.Children.Add(cone);
        }

        private static void PaintEnemyAttackEffect(
            Canvas canvas, PursuerEnemy enemy, double centerX, double centerY)
        {
            double radius = PursuerEnemy.AttackRange;
            Ellipse flash = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = EnemyAttackFlashBrush,
                Stroke = EnemyAttackFlashStroke,
                StrokeThickness = 3,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(flash, centerX - radius);
            Canvas.SetTop(flash, centerY - radius);
            canvas.Children.Add(flash);
        }

        private static void PaintEnemyHealth(Canvas canvas, PursuerEnemy enemy, double centerX, double top)
        {
            const double width = 44;
            const double height = 6;
            Grid healthBar = new Grid
            {
                Width = width,
                Height = height,
                ClipToBounds = true
            };
            healthBar.Children.Add(new Rectangle
            {
                Fill = new SolidColorBrush(Color.FromRgb(40, 20, 20))
            });
            healthBar.Children.Add(new Rectangle
            {
                Width = width * enemy.CurrentHitPoints / enemy.MaxHitPoints,
                HorizontalAlignment = HorizontalAlignment.Left,
                Fill = Brushes.IndianRed
            });

            Border border = new Border
            {
                Width = width,
                Height = height,
                BorderThickness = new Thickness(1),
                BorderBrush = Brushes.Black,
                Child = healthBar,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(border, centerX - width / 2);
            Canvas.SetTop(border, top);
            canvas.Children.Add(border);
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
                        _zoomInputCanvas.MouseDown -= OnAttackMouseDown;
                    }
                    canvas.MouseDown += OnAttackMouseDown;
                }
                _zoomInputCanvas = canvas;
                return;
            }

            if (_zoomInputCanvas != null)
            {
                _zoomInputCanvas.MouseDown -= OnAttackMouseDown;
            }
            if (_zoomInputWindow != null)
            {
                _zoomInputWindow.PreviewKeyDown -= OnZoomKeyDown;
                _zoomInputWindow.PreviewMouseWheel -= OnZoomMouseWheel;
                _zoomInputWindow.PreviewMouseMove -= OnAimMouseMove;
                _zoomInputWindow.Closed -= OnZoomWindowClosed;
            }

            _zoomInputWindow = window;
            _zoomInputField = field;
            _zoomInputCanvas = canvas;
            if (_zoomInputWindow != null)
            {
                _zoomInputCanvas.MouseDown += OnAttackMouseDown;
                _zoomInputWindow.PreviewKeyDown += OnZoomKeyDown;
                _zoomInputWindow.PreviewMouseWheel += OnZoomMouseWheel;
                _zoomInputWindow.PreviewMouseMove += OnAimMouseMove;
                _zoomInputWindow.Closed += OnZoomWindowClosed;
            }
        }

        private void OnAttackMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            if (_zoomInputField == null || !_zoomInputField.IsGameStarted
                || !_zoomInputField.Player.IsAlive || _zoomInputCanvas == null)
            {
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
            _zoomInputField.AttackWithPlayer(aimX, aimY);
            if (_zoomInputField.ShowDamageRanges)
            {
                QueuePlayerAttackFlash(_zoomInputField.Player,
                    new Point(_zoomInputField.AimDirectionX, _zoomInputField.AimDirectionY));
            }
        }

        private void QueuePlayerAttackFlash(PlayerCharacter player, Point direction)
        {
            EnsureAttackFlashPlayer(player);
            _playerAttackFlashQueue.Enqueue(direction);
        }

        private bool TryGetPlayerAttackFlash(PlayerCharacter player, out Point direction)
        {
            EnsureAttackFlashPlayer(player);
            long now = Stopwatch.GetTimestamp();

            if (_isPlayerAttackFlashActive)
            {
                if (Stopwatch.GetElapsedTime(_activePlayerAttackTimestamp, now)
                    .TotalMilliseconds < PlayerAttackFlashDurationMilliseconds)
                {
                    direction = _activePlayerAttackDirection;
                    return true;
                }

                _isPlayerAttackFlashActive = false;
                _nextPlayerAttackFlashTimestamp = now
                    + (long)(PlayerAttackFlashGapMilliseconds * Stopwatch.Frequency / 1000);
            }

            if (_playerAttackFlashQueue.Count > 0
                && now >= _nextPlayerAttackFlashTimestamp)
            {
                _activePlayerAttackDirection = _playerAttackFlashQueue.Dequeue();
                _activePlayerAttackTimestamp = now;
                _isPlayerAttackFlashActive = true;
                direction = _activePlayerAttackDirection;
                return true;
            }

            direction = default(Point);
            return false;
        }

        private void EnsureAttackFlashPlayer(PlayerCharacter player)
        {
            if (ReferenceEquals(_attackFlashPlayer, player))
            {
                return;
            }

            ClearPlayerAttackFlashes();
            _attackFlashPlayer = player;
        }

        private void ClearPlayerAttackFlashes()
        {
            _playerAttackFlashQueue.Clear();
            _isPlayerAttackFlashActive = false;
            _activePlayerAttackTimestamp = 0;
            _nextPlayerAttackFlashTimestamp = 0;
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
                if (e.Key == Key.Escape && _zoomInputField != null)
                {
                    _zoomInputField.ReturnToStartMenu();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.N && _zoomInputField != null
                    && _zoomInputField.IsGameStarted)
                {
                    _zoomInputField.SpawnEnemy();
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.B && _zoomInputField != null
                    && _zoomInputField.IsGameStarted)
                {
                    _zoomInputField.ToggleDamageRanges();
                    if (!_zoomInputField.ShowDamageRanges)
                    {
                        ClearPlayerAttackFlashes();
                    }
                    e.Handled = true;
                    return;
                }

                if (e.Key == Key.H && _zoomInputField != null
                    && _zoomInputField.IsGameStarted)
                {
                    _zoomInputField.ToggleDebugHud();
                    e.Handled = true;
                    return;
                }

                if (_zoomInputField != null)
                {
                    _zoomInputField.HandleZoomKey(e.Key);
                    if (!_zoomInputField.IsGameStarted
                        && _zoomInputCanvas != null
                        && (e.Key == Key.K || e.Key == Key.L))
                    {
                        _zoomInputCanvas.Tag = null;
                    }
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

        private void OnZoomWindowClosed(object sender, EventArgs e)
        {
            if (_zoomInputCanvas != null)
            {
                _zoomInputCanvas.MouseDown -= OnAttackMouseDown;
            }
            if (_zoomInputWindow != null)
            {
                _zoomInputWindow.PreviewKeyDown -= OnZoomKeyDown;
                _zoomInputWindow.PreviewMouseWheel -= OnZoomMouseWheel;
                _zoomInputWindow.PreviewMouseMove -= OnAimMouseMove;
                _zoomInputWindow.Closed -= OnZoomWindowClosed;
                _zoomInputWindow = null;
                _zoomInputField = null;
                _zoomInputCanvas = null;
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
                        _showAnimationMenu = false;
                        canvas.Tag = null;
                    }));
                panel.Child = contents;
                DrawStartMenuPanel(canvas, panel, width, height);
                return;
            }

            contents.Children.Add(new TextBlock
            {
                Text = isGameOver ? "GAME OVER" : "ARENA PAUSED",
                FontSize = 28,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 8)
            });
            contents.Children.Add(new TextBlock
            {
                Text = isGameOver ? "Your run has ended" : "Your game is paused",
                FontSize = 15,
                Foreground = new SolidColorBrush(Color.FromRgb(183, 197, 212)),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            });
            contents.Children.Add(CreateMenuButton(
                isGameOver ? "Erneut spielen" : "Spiel fortsetzen",
                new SolidColorBrush(Color.FromRgb(43, 91, 125)),
                (sender, e) =>
                {
                    if (isGameOver)
                    {
                        field.RestartGame();
                    }
                    else if (!field.ContinueGame())
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