using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OOPGames
{
    internal sealed class ArenaHudRenderer
    {
        private static readonly Brush HealthBackgroundBrush =
            new SolidColorBrush(Color.FromRgb(40, 45, 50));
        private static readonly Brush EnemyHealthBackgroundBrush =
            new SolidColorBrush(Color.FromRgb(40, 20, 20));
        private static readonly Brush LabelBackgroundBrush =
            new SolidColorBrush(Color.FromArgb(190, 30, 34, 38));

        private readonly BitmapSource _overlay = LoadOverlayAsset("Overlay1.png");
        private readonly BitmapSource _healthBarTexture =
            CropAsset(LoadOverlayAsset("Healthbar.png"), new Int32Rect(0, 32, 128, 32));
        private readonly BitmapSource _abilityIndicator =
            CropAsset(LoadOverlayAsset("AbilityIndikator.png"), new Int32Rect(28, 30, 72, 66));

        public void PaintPlayerHud(
            Canvas canvas, ArenaField field, BitmapSource portraitSource)
        {
            PlayerCharacter player = field.Player;
            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;
            double scaleX = width / 1800;
            double scaleY = height / 1200;

            canvas.Children.Add(new Image
            {
                Source = _overlay,
                Width = width,
                Height = height,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            });
            PaintPortrait(canvas, portraitSource, scaleX, scaleY);

            AddHudText(canvas, "Kills: " + field.Score, 16, 16);
            double cooldownSeconds = field.GetAttackCooldownRemaining(player);
            AddHudText(canvas,
                "Waffe: " + player.EquippedWeapon.Name
                    + (cooldownSeconds > 0
                        ? "  |  Abklingzeit: " + cooldownSeconds.ToString("0.0") + " s"
                        : "  |  bereit"),
                16, 44);
            AddHudText(canvas, "Effekte: " + player.StatusSummary, 16, 72);
            AddHudText(canvas, field.Progression.IsGodMode
                ? "Skillpunkte: unbegrenzt  |  Waffenpunkte: unbegrenzt"
                : "Skillpunkte: " + field.Progression.UniversalPoints
                    + "  |  Waffenpunkte: "
                    + field.Progression.GetWeaponPoints(player.EquippedWeapon.Id), 16, 100);
            AddHudText(canvas, "Sprint: Strg halten", 16, 128);
            if (player.IsInvulnerable)
            {
                AddHudText(canvas, "GODMODE AKTIV", 16, 156);
            }

            PaintHealthBar(canvas, player, scaleX, scaleY);
            PaintAbilityIndicators(canvas, field, scaleX, scaleY);
        }

        public void PaintEntityHealth(
            Canvas canvas, Entity entity, double centerX, double top)
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
                Fill = EnemyHealthBackgroundBrush
            });
            healthBar.Children.Add(new Rectangle
            {
                Width = width * entity.CurrentHitPoints / entity.MaxHitPoints,
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

        public void PaintStatusPlaceholder(
            Canvas canvas, Entity entity, double centerX, double centerY)
        {
            if (entity.ActiveStatusEffects.Count == 0)
            {
                return;
            }

            bool isFrozen = false;
            bool isBurning = false;
            bool isPoisoned = false;
            foreach (ActiveStatusEffect effect in entity.ActiveStatusEffects)
            {
                if (effect.SpeedMultiplier == 0)
                {
                    isFrozen = true;
                }
            }
            foreach (ActiveDamageOverTimeEffect effect
                in entity.ActiveDamageOverTimeEffects)
            {
                if (effect.Id == "burning")
                {
                    isBurning = true;
                }
                else if (effect.Id == "poison")
                {
                    isPoisoned = true;
                }
            }

            double diameter = entity.Size + 12;
            Ellipse indicator = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                Fill = Brushes.Transparent,
                Stroke = isFrozen ? Brushes.LightCyan
                    : isBurning ? Brushes.OrangeRed
                    : isPoisoned ? Brushes.GreenYellow : Brushes.Plum,
                StrokeThickness = 3,
                StrokeDashArray = isFrozen ? null : new DoubleCollection { 2, 2 },
                IsHitTestVisible = false
            };
            Canvas.SetLeft(indicator, centerX - diameter / 2);
            Canvas.SetTop(indicator, centerY - diameter / 2);
            canvas.Children.Add(indicator);
        }

        private void PaintPortrait(
            Canvas canvas, BitmapSource portraitSource, double scaleX, double scaleY)
        {
            const double diameter = 104;
            Image portrait = new Image
            {
                Source = portraitSource,
                Width = diameter * scaleX,
                Height = diameter * scaleY,
                Stretch = Stretch.UniformToFill,
                Clip = new EllipseGeometry(new Rect(
                    0, 0, diameter * scaleX, diameter * scaleY)),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(portrait, (98 - diameter / 2) * scaleX);
            Canvas.SetTop(portrait, (1101 - diameter / 2) * scaleY);
            canvas.Children.Add(portrait);
        }

        private void PaintHealthBar(
            Canvas canvas, PlayerCharacter player, double scaleX, double scaleY)
        {
            const double left = 165;
            const double top = 1110;
            const double width = 320;
            const double height = 32;
            double healthRatio = Math.Max(0, Math.Min(
                1, (double)player.CurrentHitPoints / player.MaxHitPoints));
            double scaledWidth = width * scaleX;
            double scaledHeight = height * scaleY;

            Grid healthBar = new Grid
            {
                Width = scaledWidth,
                Height = scaledHeight,
                ClipToBounds = true
            };
            healthBar.Children.Add(new Rectangle
            {
                Fill = HealthBackgroundBrush
            });
            healthBar.Children.Add(new Rectangle
            {
                Width = scaledWidth * healthRatio,
                HorizontalAlignment = HorizontalAlignment.Left,
                Fill = Brushes.IndianRed
            });
            healthBar.Children.Add(new Image
            {
                Source = _healthBarTexture,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            });
            healthBar.Children.Add(new TextBlock
            {
                Text = "HP: " + player.CurrentHitPoints + " / " + player.MaxHitPoints,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = Math.Max(9, 14 * Math.Min(scaleX, scaleY)),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                IsHitTestVisible = false
            });
            Canvas.SetLeft(healthBar, left * scaleX);
            Canvas.SetTop(healthBar, top * scaleY);
            canvas.Children.Add(healthBar);
        }

        private void PaintAbilityIndicators(
            Canvas canvas, ArenaField field, double scaleX, double scaleY)
        {
            List<AbilityIndicatorData> abilities = new List<AbilityIndicatorData>();

            for (int i = 0; i < ArenaProgression.AbilitySlotCount; i++)
            {
                Weapon weapon = field.Progression.GetWeaponAbilityWeapon(
                    field.Player.EquippedWeapon.Id, i);
                if (weapon != null)
                {
                    ArenaAction action =
                        (ArenaAction)((int)ArenaAction.WeaponAbility1 + i);
                    abilities.Add(new AbilityIndicatorData(
                        weapon, action, weapon.CooldownSeconds,
                        field.GetAttackCooldownRemaining(field.Player, weapon)));
                }
            }

            for (int i = 0; i < ArenaProgression.AbilitySlotCount; i++)
            {
                Weapon weapon = field.Progression.GetEntityAbilityWeapon(i);
                if (weapon != null)
                {
                    ArenaAction action =
                        (ArenaAction)((int)ArenaAction.EntityAbility1 + i);
                    abilities.Add(new AbilityIndicatorData(
                        weapon, action, weapon.CooldownSeconds,
                        field.GetAttackCooldownRemaining(field.Player, weapon)));
                }
            }

            if (abilities.Count == 0)
            {
                return;
            }

            double left = 520 * scaleX;
            double availableWidth = Math.Max(0, canvas.ActualWidth - left - 20 * scaleX);
            double cellWidth = Math.Min(88 * scaleX, availableWidth / abilities.Count);
            double iconSize = Math.Min(52 * Math.Min(scaleX, scaleY), cellWidth * 0.68);
            double iconTop = 1104 * scaleY;
            double fontSize = Math.Max(8, 12 * Math.Min(scaleX, scaleY));

            for (int i = 0; i < abilities.Count; i++)
            {
                AbilityIndicatorData ability = abilities[i];
                double iconLeft = left + i * cellWidth + (cellWidth - iconSize) / 2;
                TextBlock keyLabel = new TextBlock
                {
                    Text = field.KeyBindings.GetDisplayName(ability.Action),
                    Width = cellWidth,
                    FontSize = fontSize,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(keyLabel, left + i * cellWidth);
                Canvas.SetTop(keyLabel, 1086 * scaleY);
                canvas.Children.Add(keyLabel);

                Image indicator = new Image
                {
                    Source = _abilityIndicator,
                    Width = iconSize,
                    Height = iconSize,
                    Stretch = Stretch.Fill,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(indicator, iconLeft);
                Canvas.SetTop(indicator, iconTop);
                canvas.Children.Add(indicator);

                double fillProgress = ability.CooldownRemaining <= 0
                    ? 1
                    : ability.CooldownDuration <= 0
                        ? 0
                        : Math.Max(0, Math.Min(1,
                            1 - ability.CooldownRemaining / ability.CooldownDuration));
                if (fillProgress > 0)
                {
                    double inset = iconSize * 0.18;
                    double fillSize = iconSize - 2 * inset;
                    double fillHeight = fillSize * fillProgress;
                    Canvas cooldownFill = new Canvas
                    {
                        Width = fillSize,
                        Height = fillSize,
                        Clip = new EllipseGeometry(new Rect(0, 0, fillSize, fillSize)),
                        IsHitTestVisible = false
                    };
                    Rectangle fill = new Rectangle
                    {
                        Width = fillSize,
                        Height = fillHeight,
                        Fill = new SolidColorBrush(Color.FromArgb(190, 55, 150, 225)),
                        IsHitTestVisible = false
                    };
                    Canvas.SetTop(fill, fillSize - fillHeight);
                    cooldownFill.Children.Add(fill);
                    Canvas.SetLeft(cooldownFill, iconLeft + inset);
                    Canvas.SetTop(cooldownFill, iconTop + inset);
                    canvas.Children.Add(cooldownFill);
                }

                TextBlock abilityLabel = new TextBlock
                {
                    Text = ability.Weapon.Name,
                    Width = cellWidth,
                    FontSize = fontSize,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis,
                    ToolTip = ability.Weapon.Name,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(abilityLabel, left + i * cellWidth);
                Canvas.SetTop(abilityLabel, 1160 * scaleY);
                canvas.Children.Add(abilityLabel);
            }
        }

        private static BitmapSource LoadOverlayAsset(string fileName)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(
                PainterG9.FindAssetPath("Overlay", fileName), UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();
            return image;
        }

        private static BitmapSource CropAsset(BitmapSource source, Int32Rect rectangle)
        {
            CroppedBitmap crop = new CroppedBitmap(source, rectangle);
            crop.Freeze();
            return crop;
        }

        private static void AddHudText(Canvas canvas, string text, double left, double top)
        {
            Border label = new Border
            {
                Padding = new Thickness(6, 2, 6, 2),
                Background = LabelBackgroundBrush,
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontSize = 12
                },
                IsHitTestVisible = false
            };
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            canvas.Children.Add(label);
        }

        private sealed class AbilityIndicatorData
        {
            public AbilityIndicatorData(
                Weapon weapon, ArenaAction action,
                double cooldownDuration, double cooldownRemaining)
            {
                Weapon = weapon;
                Action = action;
                CooldownDuration = cooldownDuration;
                CooldownRemaining = cooldownRemaining;
            }

            public Weapon Weapon { get; }
            public ArenaAction Action { get; }
            public double CooldownDuration { get; }
            public double CooldownRemaining { get; }
        }
    }
}
