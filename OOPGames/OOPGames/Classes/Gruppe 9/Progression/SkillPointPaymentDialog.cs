using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace OOPGames
{
    // Asks whether missing weapon points may be paid with skill points. Unlike a MessageBox it
    // offers "Nicht erneut fragen": confirming with the box ticked pays without asking from
    // then on.
    internal sealed class SkillPointPaymentDialog : Window
    {
        private readonly CheckBox _dontAskAgain;

        private SkillPointPaymentDialog(string message, Action playClick)
        {
            // The dialog is its own window, so it plays the menu click itself.
            if (playClick != null)
            {
                AddHandler(ButtonBase.ClickEvent,
                    new RoutedEventHandler((sender, e) => playClick()), true);
            }
            Title = "Mit Skillpunkten bezahlen?";
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = Application.Current != null
                && Application.Current.MainWindow != null
                && Application.Current.MainWindow.IsVisible
                    ? WindowStartupLocation.CenterOwner
                    : WindowStartupLocation.CenterScreen;
            if (WindowStartupLocation == WindowStartupLocation.CenterOwner)
            {
                Owner = Application.Current.MainWindow;
            }

            StackPanel content = new StackPanel { Margin = new Thickness(16), MaxWidth = 420 };
            content.Children.Add(new TextBlock
            {
                Text = message,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 12)
            });
            _dontAskAgain = new CheckBox
            {
                Content = "Nicht erneut fragen (fehlende Waffenpunkte immer mit Skillpunkten bezahlen)",
                Margin = new Thickness(0, 0, 0, 12)
            };
            content.Children.Add(_dontAskAgain);

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Button yes = new Button
            {
                Content = "Ja", Width = 80, Margin = new Thickness(0, 0, 8, 0), IsDefault = true
            };
            yes.Click += (sender, e) => DialogResult = true;
            Button no = new Button { Content = "Nein", Width = 80, IsCancel = true };
            buttons.Children.Add(yes);
            buttons.Children.Add(no);
            content.Children.Add(buttons);
            Content = content;
        }

        // True when the player agreed; a ticked box on "Ja" stops future questions.
        public static bool Confirm(
            string message, ArenaPreferences preferences, Action playClick = null)
        {
            SkillPointPaymentDialog dialog = new SkillPointPaymentDialog(message, playClick);
            bool confirmed = dialog.ShowDialog() == true;
            if (confirmed && dialog._dontAskAgain.IsChecked == true)
            {
                preferences.AskBeforeSkillPointPayment = false;
            }
            return confirmed;
        }
    }
}
