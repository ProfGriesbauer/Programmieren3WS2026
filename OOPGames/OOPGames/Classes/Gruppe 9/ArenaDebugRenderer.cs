using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    internal sealed class ArenaDebugRenderer
    {
        private static readonly Brush PlayerAttackBrush =
            new SolidColorBrush(Color.FromArgb(55, 110, 190, 255));
        private static readonly Brush EnemyAttackBrush =
            new SolidColorBrush(Color.FromArgb(45, 245, 70, 70));
        private static readonly Brush HitboxFillBrush =
            new SolidColorBrush(Color.FromArgb(35, 144, 238, 144));
        private static readonly Brush HitboxStrokeBrush =
            new SolidColorBrush(Color.FromArgb(240, 144, 238, 144));
        private static readonly Brush SightFillBrush =
            new SolidColorBrush(Color.FromArgb(10, 255, 255, 255));
        private static readonly Brush SightStrokeBrush =
            new SolidColorBrush(Color.FromArgb(210, 255, 255, 255));
        private static readonly Brush EnemyPathBrush =
            new SolidColorBrush(Color.FromArgb(230, 255, 215, 0));
        private static readonly Brush HealingEffectBrush =
            new SolidColorBrush(Color.FromArgb(100, 50, 230, 100));
        private static readonly Brush FreezeEffectBrush =
            new SolidColorBrush(Color.FromArgb(150, 100, 220, 255));
        private static readonly Brush SlowEffectBrush =
            new SolidColorBrush(Color.FromArgb(120, 190, 120, 255));
        private static readonly Brush MixedEffectBrush =
            new SolidColorBrush(Color.FromArgb(130, 210, 150, 255));
        private static readonly Brush PoisonEffectBrush =
            new SolidColorBrush(Color.FromArgb(120, 110, 210, 80));
        private static readonly Brush FireEffectBrush =
            new SolidColorBrush(Color.FromArgb(130, 255, 110, 25));
        private static readonly Brush LightningEffectBrush =
            new SolidColorBrush(Color.FromArgb(145, 255, 245, 90));
        private static readonly Brush DamageEffectBrush =
            new SolidColorBrush(Color.FromArgb(100, 255, 100, 70));

        private static readonly Brush RegionStrokeBrush =
            new SolidColorBrush(Color.FromArgb(220, 120, 220, 255));
        private static readonly Brush RegionFillBrush =
            new SolidColorBrush(Color.FromArgb(28, 120, 220, 255));

        public void PaintWorldOverlays(
            Canvas canvas, ArenaField field, double left, double top)
        {
            if (field.ShowDamageRanges)
            {
                PaintSpawnRegions(canvas, field, left, top);
                foreach (NonPlayerCharacter enemy in field.Enemies)
                {
                    if (!enemy.IsAlive)
                    {
                        continue;
                    }

                    AddPlannedEnemyPath(canvas, enemy, field, left, top);
                    AddSightRange(canvas, enemy.PositionX - left,
                        enemy.PositionY - top, enemy.SightRange, false);
                    if (enemy.HasSeenPlayer)
                    {
                        AddSightRange(canvas, enemy.PositionX - left,
                            enemy.PositionY - top, enemy.CurrentSightRange, true);
                    }

                    AddDamageRange(canvas, EnemyAttackBrush,
                        enemy.PositionX - left, enemy.PositionY - top,
                        enemy.EquippedWeapon.AttackPattern.Range);
                }

                PaintWeaponRange(canvas, field.Player.EquippedWeapon,
                    field.Player.PositionX - left, field.Player.PositionY - top,
                    field.AimDirectionX, field.AimDirectionY);
                AddEntityHitbox(canvas, field.Player.Hitbox, left, top);
                foreach (NonPlayerCharacter enemy in field.Enemies)
                {
                    if (enemy.IsAlive)
                    {
                        AddEntityHitbox(canvas, enemy.Hitbox, left, top);
                    }
                }
            }

            // In normal play the weapons, projectiles and particles show the attacks; their
            // exact damage areas are only drawn in the debug view.
            if (!field.ShowDamageRanges)
            {
                return;
            }
            foreach (AttackVisualState visual in field.ActiveAttackVisuals)
            {
                if (visual.HasBlade)
                {
                    AddBladeHitArea(canvas, visual, left, top);
                    continue;
                }
                PaintActiveAttack(canvas, visual, left, top);
            }
            foreach (ArenaGroundEffect groundEffect in field.GroundEffects)
            {
                if (groundEffect.Effect != null)
                {
                    AddDamageRange(canvas, FireEffectBrush,
                        groundEffect.CenterX - left, groundEffect.CenterY - top,
                        groundEffect.Radius);
                }
            }
        }

        public void PaintPanels(Canvas canvas, ArenaField field, double width, double height)
        {
            if (field.ShowDamageRanges)
            {
                PaintLegend(canvas, width, height);
            }
            if (field.ShowDebugHud)
            {
                PaintDebugHud(canvas, field, width, height);
            }
        }

        private static void PaintWeaponRange(
            Canvas canvas, Weapon weapon, double centerX, double centerY,
            double directionX, double directionY)
        {
            IAttackPattern pattern = weapon.AttackPattern;
            switch (pattern.VisualKind)
            {
                case AttackVisualKind.Cone:
                case AttackVisualKind.Sweep:
                    AddDamageCone(canvas, PlayerAttackBrush, centerX, centerY,
                        pattern.Range, directionX, directionY,
                        pattern.DebugHalfAngleRadians);
                    break;
                case AttackVisualKind.Projectile:
                    AddLine(canvas, PlayerAttackBrush, centerX, centerY,
                        centerX + directionX * pattern.Range,
                        centerY + directionY * pattern.Range);
                    AddDamageRange(canvas, PlayerAttackBrush,
                        centerX + directionX * pattern.Range,
                        centerY + directionY * pattern.Range,
                        pattern.DebugRadius);
                    break;
                case AttackVisualKind.Burst:
                    AddDamageRange(canvas, PlayerAttackBrush,
                        centerX, centerY, pattern.DebugRadius);
                    break;
            }
        }

        private static void AddBladeHitArea(
            Canvas canvas, AttackVisualState visual, double left, double top)
        {
            canvas.Children.Add(new Line
            {
                X1 = visual.BladeStartX - left,
                Y1 = visual.BladeStartY - top,
                X2 = visual.BladeEndX - left,
                Y2 = visual.BladeEndY - top,
                Stroke = PlayerAttackBrush,
                StrokeThickness = visual.BladeHalfWidth * 2,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false
            });
        }

        private static void PaintActiveAttack(
            Canvas canvas, AttackVisualState visual, double left, double top)
        {
            Brush fill;
            Brush stroke;
            switch (visual.EffectKind)
            {
                case CombatEffectKind.Healing:
                    fill = HealingEffectBrush;
                    stroke = Brushes.LightGreen;
                    break;
                case CombatEffectKind.Freeze:
                    fill = FreezeEffectBrush;
                    stroke = Brushes.LightCyan;
                    break;
                case CombatEffectKind.Slow:
                    fill = SlowEffectBrush;
                    stroke = Brushes.Plum;
                    break;
                case CombatEffectKind.Mixed:
                    fill = MixedEffectBrush;
                    stroke = Brushes.White;
                    break;
                case CombatEffectKind.Poison:
                    fill = PoisonEffectBrush;
                    stroke = Brushes.GreenYellow;
                    break;
                case CombatEffectKind.Fire:
                    fill = FireEffectBrush;
                    stroke = Brushes.Orange;
                    break;
                case CombatEffectKind.Lightning:
                    fill = LightningEffectBrush;
                    stroke = Brushes.Yellow;
                    break;
                default:
                    fill = DamageEffectBrush;
                    stroke = Brushes.OrangeRed;
                    break;
            }

            double centerX = visual.CenterX - left;
            double centerY = visual.CenterY - top;
            if (visual.VisualKind == AttackVisualKind.Projectile)
            {
                AddDamageRange(canvas, fill,
                    visual.ProjectileX - left, visual.ProjectileY - top,
                    visual.Radius);
                return;
            }
            if (visual.VisualKind == AttackVisualKind.Burst)
            {
                // Radius already grows with the wave.
                AddOutlinedRange(canvas, fill, stroke,
                    centerX, centerY, visual.Radius, 1);
                return;
            }

            AddDamageCone(canvas, fill, centerX, centerY, visual.Range,
                visual.DirectionX, visual.DirectionY,
                visual.HalfAngleRadians, stroke);
        }

        private static void PaintDebugHud(
            Canvas canvas, ArenaField field, double canvasWidth, double canvasHeight)
        {
            StackPanel content = new StackPanel();
            AddDebugText(content, "ARENA DEBUG", true);
            AddDebugText(content, string.Format(
                "Spieler: {0:0}, {1:0} | HP {2}/{3}",
                field.Player.PositionX, field.Player.PositionY,
                field.Player.CurrentHitPoints, field.Player.MaxHitPoints));
            AddDebugText(content, string.Format(
                "Tempo: {0:0.0}/{1:0.0} | Blick: {2}",
                field.Player.EffectiveSpeed, field.Player.MaxSpeed,
                GetFacingLabel(field.Player.Facing)));
            AddDebugText(content, string.Format(
                "Waffe: {0} | Muster: {1}",
                field.SelectedPlayerWeapon.Name,
                field.SelectedPlayerWeapon.AttackPattern.Name));
            AddDebugText(content, "Effekt: "
                + field.SelectedPlayerWeapon.Effect.Description);
            AddDebugText(content, string.Format(
                "Reichweite: {0:0} | Abklingzeit: {1:0.00}s | Aktiv: {2}",
                field.SelectedPlayerWeapon.AttackPattern.Range,
                field.GetAttackCooldownRemaining(field.Player),
                field.ActiveAttackCount));
            AddDebugText(content, "Animation: "
                + field.SelectedPlayerWeapon.AnimationPlaceholder + " (Platzhalter)");
            AddDebugText(content, "Spielerstatus: " + field.Player.StatusSummary);
            AddDebugText(content, string.Format(
                "Gegner: {0} | Kills: {1}", field.Enemies.Count, field.Score));

            int shownEnemies = 0;
            foreach (NonPlayerCharacter enemy in field.Enemies)
            {
                if (!enemy.IsAlive || shownEnemies >= 5)
                {
                    continue;
                }

                double dx = enemy.PositionX - field.Player.PositionX;
                double dy = enemy.PositionY - field.Player.PositionY;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                AddDebugText(content, string.Format(
                    "Gegner {0}: {1:0} HP | {2:0} Einheiten | {3}",
                    shownEnemies + 1, enemy.CurrentHitPoints, distance,
                    enemy.StatusSummary));
                shownEnemies++;
            }

            Border panel = CreateDebugPanel(content, canvasWidth, canvasHeight);
            Canvas.SetLeft(panel, 16);
            Canvas.SetTop(panel, 150);
            canvas.Children.Add(panel);
        }

        private static void PaintLegend(Canvas canvas, double width, double height)
        {
            StackPanel content = new StackPanel();
            AddDebugText(content, "DEBUG-LEGENDE", true);
            AddLegendItem(content, PlayerAttackBrush, "Spieler-Waffe");
            AddLegendItem(content, EnemyAttackBrush, "Gegner-Waffe");
            AddLegendItem(content, HitboxStrokeBrush, "Hitboxen");
            AddLegendItem(content, SightStrokeBrush, "Gegner-Sichtweite");
            AddLegendItem(content, EnemyPathBrush, "Geplanter Gegnerweg");

            Border panel = CreateDebugPanel(content, width, height);
            Canvas.SetLeft(panel, Math.Max(16, width - panel.DesiredSize.Width - 16));
            Canvas.SetTop(panel, 102);
            canvas.Children.Add(panel);
        }

        private static Border CreateDebugPanel(
            StackPanel content, double canvasWidth, double canvasHeight)
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
                Math.Max(0, canvasHeight - 110)));
            return panel;
        }

        private static void AddDebugText(StackPanel panel, string text, bool header = false)
        {
            panel.Children.Add(new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontWeight = header ? FontWeights.Bold : FontWeights.Normal,
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

        private static void AddDamageRange(
            Canvas canvas, Brush fill, double centerX, double centerY, double radius)
        {
            AddOutlinedRange(canvas, fill, null, centerX, centerY, radius, 1);
        }

        private static void AddOutlinedRange(
            Canvas canvas, Brush fill, Brush stroke,
            double centerX, double centerY, double radius, double progress)
        {
            double currentRadius = Math.Max(2, radius * Math.Max(0.15, progress));
            Ellipse range = new Ellipse
            {
                Width = currentRadius * 2,
                Height = currentRadius * 2,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = stroke == null ? 0 : 2,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(range, centerX - currentRadius);
            Canvas.SetTop(range, centerY - currentRadius);
            canvas.Children.Add(range);
        }

        private static void AddEntityHitbox(
            Canvas canvas, Rect hitbox, double left, double top)
        {
            Rectangle outline = new Rectangle
            {
                Width = hitbox.Width,
                Height = hitbox.Height,
                Fill = HitboxFillBrush,
                Stroke = HitboxStrokeBrush,
                StrokeThickness = 2,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(outline, hitbox.X - left);
            Canvas.SetTop(outline, hitbox.Y - top);
            canvas.Children.Add(outline);
        }

        private static void AddSightRange(
            Canvas canvas, double centerX, double centerY, double radius, bool dashed)
        {
            Ellipse range = new Ellipse
            {
                Width = radius * 2,
                Height = radius * 2,
                Fill = dashed ? Brushes.Transparent : SightFillBrush,
                Stroke = SightStrokeBrush,
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

        // Spawn regions of the map: outlined shapes with their names (F4 + B).
        private static void PaintSpawnRegions(
            Canvas canvas, ArenaField field, double left, double top)
        {
            foreach (ArenaMapRegion region in field.MapRegions)
            {
                Rect bounds = region.Bounds;
                Shape shape = region.Shape == ArenaRegionShape.Circle
                    ? (Shape)new Ellipse()
                    : new Rectangle();
                shape.Width = bounds.Width;
                shape.Height = bounds.Height;
                shape.Stroke = RegionStrokeBrush;
                shape.StrokeThickness = 2;
                shape.StrokeDashArray = new DoubleCollection { 6, 4 };
                shape.Fill = RegionFillBrush;
                shape.IsHitTestVisible = false;
                Canvas.SetLeft(shape, bounds.X - left);
                Canvas.SetTop(shape, bounds.Y - top);
                canvas.Children.Add(shape);
                TextBlock label = new TextBlock
                {
                    Text = region.Name,
                    Foreground = RegionStrokeBrush,
                    FontWeight = FontWeights.Bold,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(label, bounds.X - left + 4);
                Canvas.SetTop(label, bounds.Y - top + 2);
                canvas.Children.Add(label);
            }
        }

        private static void AddPlannedEnemyPath(
            Canvas canvas, NonPlayerCharacter enemy, ArenaField field, double left, double top)
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

        private static void AddLine(
            Canvas canvas, Brush brush, double startX, double startY, double endX, double endY)
        {
            canvas.Children.Add(new Line
            {
                X1 = startX,
                Y1 = startY,
                X2 = endX,
                Y2 = endY,
                Stroke = brush,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 5, 3 },
                IsHitTestVisible = false
            });
        }
    }
}
