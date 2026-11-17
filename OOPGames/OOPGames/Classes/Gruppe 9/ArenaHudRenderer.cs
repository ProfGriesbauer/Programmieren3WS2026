using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        public void PaintPlayerHud(
            Canvas canvas, PlayerCharacter player, int score, double cooldownSeconds)
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
                Fill = HealthBackgroundBrush
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
                Background = LabelBackgroundBrush,
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

            AddHudText(canvas,
                "Waffe: " + player.EquippedWeapon.Name
                    + (cooldownSeconds > 0
                        ? "  |  Abklingzeit: " + cooldownSeconds.ToString("0.0") + " s"
                        : "  |  bereit"),
                16, 50);
            AddHudText(canvas, "Effekte: " + player.StatusSummary, 16, 73);
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
            foreach (ActiveStatusEffect effect in entity.ActiveStatusEffects)
            {
                if (effect.SpeedMultiplier == 0)
                {
                    isFrozen = true;
                    break;
                }
            }

            double diameter = entity.Size + 12;
            Ellipse indicator = new Ellipse
            {
                Width = diameter,
                Height = diameter,
                Fill = Brushes.Transparent,
                Stroke = isFrozen ? Brushes.LightCyan : Brushes.Plum,
                StrokeThickness = 3,
                StrokeDashArray = isFrozen ? null : new DoubleCollection { 2, 2 },
                IsHitTestVisible = false
            };
            Canvas.SetLeft(indicator, centerX - diameter / 2);
            Canvas.SetTop(indicator, centerY - diameter / 2);
            canvas.Children.Add(indicator);
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
    }
}
