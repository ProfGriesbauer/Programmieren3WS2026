using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OOPGames
{
    internal sealed class SatisfactoryStartScreenSettings
    {
        internal int ResourceAmount { get; set; }
        internal int MinerCycleTicks { get; set; }
        internal int SmelterCycleTicks { get; set; }
    }

    internal sealed class SatisfactoryStartScreen : Window
    {
        private static readonly Brush BackgroundBrush = BrushFromRgb(12, 19, 23);
        private static readonly Brush CardBrush = BrushFromRgb(23, 34, 39);
        private static readonly Brush MutedBrush = BrushFromRgb(162, 181, 181);
        private static readonly Brush AccentBrush = BrushFromRgb(96, 220, 151);
        private readonly ComboBox _resourceAmount;
        private readonly ComboBox _productionSpeed;
        private readonly ContentControl _guideContent;
        private readonly Button _overviewButton;
        private readonly Button _guideButton;

        internal SatisfactoryStartScreenSettings SelectedSettings { get; private set; }

        internal SatisfactoryStartScreen()
        {
            Title = "Satisfactory | Start";
            Width = 1040;
            Height = 700;
            MinWidth = 900;
            MinHeight = 620;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = BackgroundBrush;
            Foreground = Brushes.White;
            FontFamily = new FontFamily("Segoe UI");

            if (Application.Current != null &&
                Application.Current.MainWindow != null &&
                Application.Current.MainWindow != this)
            {
                Owner = Application.Current.MainWindow;
            }

            _resourceAmount = CreateComboBox(new[]
            {
                new ComboBoxItem { Content = "Knapp - 150 Erz pro Vorkommen", Tag = 150 },
                new ComboBoxItem { Content = "Standard - 300 Erz pro Vorkommen", Tag = 300 },
                new ComboBoxItem { Content = "Reich - 600 Erz pro Vorkommen", Tag = 600 }
            });
            _resourceAmount.SelectedIndex = 1;

            _productionSpeed = CreateComboBox(new[]
            {
                new ComboBoxItem { Content = "Entspannt", Tag = 1 },
                new ComboBoxItem { Content = "Standard", Tag = 2 },
                new ComboBoxItem { Content = "Schnell", Tag = 3 }
            });
            _productionSpeed.SelectedIndex = 1;

            _guideContent = new ContentControl();
            _overviewButton = CreateButton("Übersicht", AccentBrush, BackgroundBrush);
            _guideButton = CreateButton("Anleitung", CardBrush, MutedBrush);
            _overviewButton.Click += (sender, args) => ShowOverview();
            _guideButton.Click += (sender, args) => ShowGuide();

            Content = BuildLayout();
            ShowOverview();
        }

        private UIElement BuildLayout()
        {
            Grid layout = new Grid { Margin = new Thickness(30) };
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            StackPanel header = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(2, 0, 0, 20)
            };
            Border logo = new Border
            {
                Width = 38,
                Height = 38,
                CornerRadius = new CornerRadius(12),
                Background = AccentBrush,
                Child = new TextBlock
                {
                    Text = "S",
                    FontSize = 23,
                    FontWeight = FontWeights.Bold,
                    Foreground = BackgroundBrush,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            header.Children.Add(logo);
            StackPanel brand = new StackPanel { Margin = new Thickness(12, 0, 0, 0) };
            brand.Children.Add(new TextBlock
            {
                Text = "SATISFACTORY",
                FontSize = 17,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            });
            brand.Children.Add(new TextBlock
            {
                Text = "GRUPPE 5  /  FABRIK-SIMULATION",
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = MutedBrush,
                Margin = new Thickness(0, 3, 0, 0)
            });
            header.Children.Add(brand);
            Grid.SetRow(header, 0);
            layout.Children.Add(header);

            Grid columns = new Grid();
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1.13, GridUnitType.Star) });
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(22) });
            columns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.87, GridUnitType.Star) });

            Border introduction = CreateCard();
            Grid.SetColumn(introduction, 0);
            columns.Children.Add(introduction);

            Grid introLayout = new Grid { Margin = new Thickness(26) };
            introLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            introLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            introLayout.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            introLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock eyebrow = new TextBlock
            {
                Text = "DEINE FABRIK BEGINNT HIER",
                Foreground = AccentBrush,
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(eyebrow, 0);
            introLayout.Children.Add(eyebrow);

            TextBlock title = new TextBlock
            {
                Text = "Rohstoffe rein.\nProduktion an.",
                Foreground = Brushes.White,
                FontSize = 36,
                FontWeight = FontWeights.Bold,
                LineHeight = 43,
                Margin = new Thickness(0, 0, 0, 16)
            };
            Grid.SetRow(title, 1);
            introLayout.Children.Add(title);

            Grid guideArea = new Grid();
            guideArea.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            guideArea.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            StackPanel navigation = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 4, 0, 18)
            };
            navigation.Children.Add(_overviewButton);
            navigation.Children.Add(_guideButton);
            Grid.SetRow(navigation, 0);
            guideArea.Children.Add(navigation);
            Grid.SetRow(_guideContent, 1);
            guideArea.Children.Add(_guideContent);
            Grid.SetRow(guideArea, 2);
            introLayout.Children.Add(guideArea);

            TextBlock compatibility = new TextBlock
            {
                Text = "Welt, Förderer, Schmelzen und Darstellung nutzen die gemeinsamen Gruppe-5-Schnittstellen.",
                Foreground = MutedBrush,
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 18, 0, 0)
            };
            Grid.SetRow(compatibility, 3);
            introLayout.Children.Add(compatibility);
            introduction.Child = introLayout;

            Border settings = CreateCard();
            Grid.SetColumn(settings, 2);
            columns.Children.Add(settings);

            StackPanel settingsLayout = new StackPanel { Margin = new Thickness(25) };
            settingsLayout.Children.Add(new TextBlock
            {
                Text = "Spiel einstellen",
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 5)
            });
            settingsLayout.Children.Add(new TextBlock
            {
                Text = "Passe die Ressourcen und das Tempo an.",
                FontSize = 12,
                Foreground = MutedBrush,
                Margin = new Thickness(0, 0, 0, 22)
            });

            AddSetting(settingsLayout, "ROHSTOFFVORRAT", _resourceAmount,
                "Bestimmt die Erzmenge jedes Vorkommens.");
            AddSetting(settingsLayout, "PRODUKTIONSTEMPO", _productionSpeed,
                "Beeinflusst Förderer und Schmelzzyklen.");

            Border objective = new Border
            {
                Background = BrushFromRgb(29, 47, 43),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(14),
                Margin = new Thickness(0, 10, 0, 0),
                Child = new TextBlock
                {
                    Text = "DEIN ZIEL\nBaue Förderer auf Erzvorkommen. Das Hub transportiert Erz automatisch zu passenden Schmelzen.",
                    Foreground = Brushes.White,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 19
                }
            };
            settingsLayout.Children.Add(objective);

            Button start = CreateButton("FABRIK STARTEN   →", AccentBrush, BackgroundBrush);
            start.FontSize = 14;
            start.FontWeight = FontWeights.Bold;
            start.HorizontalContentAlignment = HorizontalAlignment.Center;
            start.Height = 48;
            start.Margin = new Thickness(0, 20, 0, 0);
            start.Click += StartGame;
            settingsLayout.Children.Add(start);
            settings.Child = settingsLayout;

            Grid.SetColumn(settings, 1);
            Grid.SetRow(columns, 1);
            layout.Children.Add(columns);

            TextBlock footer = new TextBlock
            {
                Text = "Steuerung im Spiel: F Förderer   •   S Schmelze   •   Esc Auswahl aufheben",
                Foreground = MutedBrush,
                FontSize = 11,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 16, 0, 0)
            };
            Grid.SetRow(footer, 2);
            layout.Children.Add(footer);
            return layout;
        }

        private void ShowOverview()
        {
            _overviewButton.Background = AccentBrush;
            _overviewButton.Foreground = BackgroundBrush;
            _guideButton.Background = CardBrush;
            _guideButton.Foreground = MutedBrush;
            _guideContent.Content = BuildSteps();
        }

        private void ShowGuide()
        {
            _overviewButton.Background = CardBrush;
            _overviewButton.Foreground = MutedBrush;
            _guideButton.Background = AccentBrush;
            _guideButton.Foreground = BackgroundBrush;
            _guideContent.Content = BuildGuide();
        }

        private static UIElement BuildSteps()
        {
            StackPanel steps = new StackPanel();
            AddStep(steps, "01", "Förderer platzieren",
                "Drücke F und klicke auf ein Eisen- oder Kupfervorkommen.");
            AddStep(steps, "02", "Schmelze bauen",
                "Drücke S und wähle eine freie Fläche für eine Schmelze.");
            AddStep(steps, "03", "Produktion starten",
                "Erz und Barren werden automatisch zwischen Hub und Gebäuden transportiert.");
            return steps;
        }

        private static UIElement BuildGuide()
        {
            StackPanel guide = new StackPanel();
            guide.Children.Add(new TextBlock
            {
                Text = "ANLEITUNG",
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Foreground = AccentBrush,
                Margin = new Thickness(0, 0, 0, 12)
            });
            guide.Children.Add(new TextBlock
            {
                Text = "1. Wähle mit F den Förderer und setze ihn direkt auf ein Vorkommen.\\n\\n" +
                    "2. Wähle mit S eine Schmelze und platziere sie auf freiem Gelände.\\n\\n" +
                    "3. Förderer bringen Erz zum Hub. Das Hub versorgt passende Schmelzen und lagert ihre Produkte ein.\\n\\n" +
                    "4. Esc hebt das aktuelle Bauwerkzeug auf. Mit F oder S wählst du anschließend ein neues.",
                FontSize = 13,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 21
            });
            return guide;
        }

        private void StartGame(object sender, RoutedEventArgs e)
        {
            ComboBoxItem resource = (ComboBoxItem)_resourceAmount.SelectedItem;
            ComboBoxItem speed = (ComboBoxItem)_productionSpeed.SelectedItem;
            int speedChoice = (int)speed.Tag;
            SelectedSettings = new SatisfactoryStartScreenSettings
            {
                ResourceAmount = (int)resource.Tag,
                MinerCycleTicks = speedChoice == 1 ? 55 : speedChoice == 3 ? 22 : 35,
                SmelterCycleTicks = speedChoice == 1 ? 80 : speedChoice == 3 ? 32 : 55
            };
            DialogResult = true;
        }

        private static void AddSetting(
            Panel parent,
            string label,
            ComboBox options,
            string description)
        {
            parent.Children.Add(new TextBlock
            {
                Text = label,
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = AccentBrush,
                Margin = new Thickness(0, 0, 0, 7)
            });
            parent.Children.Add(options);
            parent.Children.Add(new TextBlock
            {
                Text = description,
                FontSize = 11,
                Foreground = MutedBrush,
                Margin = new Thickness(0, 5, 0, 18)
            });
        }

        private static void AddStep(Panel parent, string number, string heading, string detail)
        {
            Grid row = new Grid { Margin = new Thickness(0, 0, 0, 15) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(42) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            Border badge = new Border
            {
                Width = 32,
                Height = 32,
                CornerRadius = new CornerRadius(10),
                Background = BrushFromRgb(41, 58, 61),
                Child = new TextBlock
                {
                    Text = number,
                    Foreground = AccentBrush,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            row.Children.Add(badge);
            StackPanel text = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
            text.Children.Add(new TextBlock
            {
                Text = heading,
                FontSize = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.White
            });
            text.Children.Add(new TextBlock
            {
                Text = detail,
                FontSize = 11,
                Foreground = MutedBrush,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 3, 0, 0)
            });
            Grid.SetColumn(text, 1);
            row.Children.Add(text);
            parent.Children.Add(row);
        }

        private static Border CreateCard()
        {
            return new Border
            {
                Background = CardBrush,
                CornerRadius = new CornerRadius(18),
                BorderBrush = BrushFromRgb(40, 55, 60),
                BorderThickness = new Thickness(1)
            };
        }

        private static ComboBox CreateComboBox(ComboBoxItem[] items)
        {
            ComboBox comboBox = new ComboBox
            {
                Height = 38,
                FontSize = 12,
                Foreground = Brushes.White,
                Background = BrushFromRgb(31, 45, 50),
                BorderBrush = BrushFromRgb(58, 77, 80),
                Padding = new Thickness(9, 5, 7, 5)
            };
            Style itemStyle = new Style(typeof(ComboBoxItem));
            itemStyle.Setters.Add(new Setter(Control.ForegroundProperty, Brushes.White));
            itemStyle.Setters.Add(new Setter(Control.BackgroundProperty, BrushFromRgb(31, 45, 50)));
            itemStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(8, 6, 8, 6)));
            comboBox.ItemContainerStyle = itemStyle;
            foreach (ComboBoxItem item in items)
            {
                item.Foreground = Brushes.White;
                item.Background = BrushFromRgb(31, 45, 50);
                comboBox.Items.Add(item);
            }

            return comboBox;
        }

        private static Button CreateButton(string label, Brush background, Brush foreground)
        {
            return new Button
            {
                Content = label,
                Background = background,
                Foreground = foreground,
                BorderBrush = Brushes.Transparent,
                Padding = new Thickness(15, 8, 15, 8),
                Margin = new Thickness(0, 0, 8, 0),
                Cursor = System.Windows.Input.Cursors.Hand,
                FontSize = 11,
                FontWeight = FontWeights.SemiBold
            };
        }

        private static Brush BrushFromRgb(byte red, byte green, byte blue)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
            brush.Freeze();
            return brush;
        }
    }
}
