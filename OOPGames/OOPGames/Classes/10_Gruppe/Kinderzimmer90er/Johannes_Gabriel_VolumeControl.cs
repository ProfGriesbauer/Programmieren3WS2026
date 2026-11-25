using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // Volume panel in the top left corner of the 3D room: speaker icon, ten clickable segments,
    // - / + buttons and the mouse wheel. It controls G10_SoundVolume, i.e. all games of group 10.
    // While a game runs, the panel is hidden and only appears briefly after a change (+ / - keys).
    sealed class G10_VolumeControl
    {
        const double ShowAfterChangeSeconds = 1.6;
        static readonly Brush EmptySegment = Frozen(Color.FromRgb(36, 44, 78));
        static readonly Brush ButtonHover = Frozen(Color.FromRgb(38, 78, 200));
        static readonly Brush MuteBrush = Frozen(Color.FromRgb(232, 84, 72));

        readonly Border _Element;
        readonly Border[] _Segments = new Border[G10_SoundVolume.MaxLevel];
        readonly TextBlock _Value;
        readonly Path[] _Waves = new Path[3];
        readonly Canvas _MuteCross;

        public G10_VolumeControl()
        {
            StackPanel row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(CreateSpeakerIcon(out _MuteCross));

            StackPanel column = new StackPanel { Margin = new Thickness(8, 0, 0, 0) };
            DockPanel header = new DockPanel { LastChildFill = false };
            header.Children.Add(G10_RetroStyle.CreateText("LAUTSTAERKE", 13, G10_RetroStyle.Dim));
            TextBlock keys = G10_RetroStyle.CreateText("+ / -", 12, G10_RetroStyle.Dim);
            keys.Margin = new Thickness(14, 0, 0, 0);
            DockPanel.SetDock(keys, Dock.Right);
            header.Children.Add(keys);
            column.Children.Add(header);

            StackPanel bar = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 4, 0, 0) };
            bar.Children.Add(CreateButton("-", G10_SoundVolume.Decrease));
            for (int i = 0; i < _Segments.Length; i++)
            {
                int level = i + 1;
                Border segment = new Border
                {
                    Width = 5,
                    Height = 18,
                    Margin = new Thickness(0.5, 0, 0.5, 0),
                    BorderBrush = G10_RetroStyle.Frame,
                    BorderThickness = new Thickness(1),
                    Cursor = Cursors.Hand,
                    VerticalAlignment = VerticalAlignment.Center
                };
                // Clicking the only lit segment again mutes.
                segment.MouseLeftButtonDown += (sender, e) =>
                {
                    G10_SoundVolume.Level = level == 1 && G10_SoundVolume.Level == 1 ? 0 : level;
                    e.Handled = true;
                };
                _Segments[i] = segment;
                bar.Children.Add(segment);
            }
            bar.Children.Add(CreateButton("+", G10_SoundVolume.Increase));
            _Value = G10_RetroStyle.CreateText("", 14, G10_RetroStyle.Text);
            _Value.Width = 52;
            _Value.TextAlignment = TextAlignment.Right;
            _Value.VerticalAlignment = VerticalAlignment.Center;
            bar.Children.Add(_Value);
            column.Children.Add(bar);
            row.Children.Add(column);

            _Element = G10_RetroStyle.CreatePanel(row);
            _Element.HorizontalAlignment = HorizontalAlignment.Left;
            _Element.VerticalAlignment = VerticalAlignment.Top;
            _Element.Margin = new Thickness(8);
            _Element.ToolTip = "Lautstaerke aller Spiele (Mausrad, Klick oder + / -)";
            _Element.MouseWheel += (sender, e) =>
            {
                if (e.Delta > 0) G10_SoundVolume.Increase();
                else G10_SoundVolume.Decrease();
                e.Handled = true;
            };

            G10_SoundVolume.Changed += Refresh;
            Refresh();
        }

        public FrameworkElement Element { get { return _Element; } }

        // roomVisible: the 3D room (room or device view) is on screen.
        public void Update(bool roomVisible)
        {
            bool recentlyChanged = (DateTime.Now - G10_SoundVolume.LastChanged).TotalSeconds < ShowAfterChangeSeconds;
            Visibility visibility = roomVisible || recentlyChanged ? Visibility.Visible : Visibility.Collapsed;
            if (_Element.Visibility != visibility) _Element.Visibility = visibility;
        }

        void Refresh()
        {
            int level = G10_SoundVolume.Level;
            for (int i = 0; i < _Segments.Length; i++)
                _Segments[i].Background = i < level ? SegmentBrush(i) : EmptySegment;
            _Value.Text = level == 0 ? "STUMM" : G10_SoundVolume.Percent + "%";
            _Value.Foreground = level == 0 ? MuteBrush : G10_RetroStyle.Text;

            int percent = G10_SoundVolume.Percent;
            int waves = level == 0 ? 0 : percent <= 30 ? 1 : percent <= 70 ? 2 : 3;
            for (int i = 0; i < _Waves.Length; i++)
                _Waves[i].Visibility = i < waves ? Visibility.Visible : Visibility.Hidden;
            _MuteCross.Visibility = level == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        // Green for quiet, yellow in the middle, red for the loudest segments.
        static readonly Brush QuietSegment = Frozen(Color.FromRgb(110, 210, 110));
        static readonly Brush MediumSegment = Frozen(Color.FromRgb(240, 200, 70));
        static readonly Brush LoudSegment = Frozen(Color.FromRgb(232, 84, 72));

        // Colour by the percentage a segment stands for: up to 50 % green, up to 80 % yellow, above red.
        static Brush SegmentBrush(int index)
        {
            int percent = (index + 1) * G10_SoundVolume.PercentPerLevel;
            if (percent > 80) return LoudSegment;
            if (percent > 50) return MediumSegment;
            return QuietSegment;
        }

        Border CreateButton(string label, Action click)
        {
            TextBlock text = G10_RetroStyle.CreateText(label, 15, G10_RetroStyle.Text);
            text.HorizontalAlignment = HorizontalAlignment.Center;
            text.VerticalAlignment = VerticalAlignment.Center;
            Border button = new Border
            {
                Width = 22,
                Height = 20,
                Margin = new Thickness(3, 0, 3, 0),
                BorderBrush = G10_RetroStyle.Frame,
                BorderThickness = new Thickness(1.5),
                Background = Brushes.Transparent,
                Cursor = Cursors.Hand,
                Child = text
            };
            button.MouseEnter += (sender, e) => button.Background = ButtonHover;
            button.MouseLeave += (sender, e) => button.Background = Brushes.Transparent;
            button.MouseLeftButtonDown += (sender, e) =>
            {
                click();
                e.Handled = true;
            };
            return button;
        }

        Canvas CreateSpeakerIcon(out Canvas muteCross)
        {
            Canvas icon = new Canvas { Width = 34, Height = 30, VerticalAlignment = VerticalAlignment.Center };
            icon.Children.Add(new Rectangle { Width = 7, Height = 10, Fill = G10_RetroStyle.Text, Margin = new Thickness(2, 10, 0, 0) });
            icon.Children.Add(new Polygon
            {
                Points = new PointCollection { new Point(9, 10), new Point(17, 3), new Point(17, 27), new Point(9, 20) },
                Fill = G10_RetroStyle.Text
            });
            for (int i = 0; i < _Waves.Length; i++)
            {
                double radius = 5 + i * 5;
                PathFigure arc = new PathFigure { StartPoint = new Point(19, 15 - radius * 0.8) };
                arc.Segments.Add(new ArcSegment(new Point(19, 15 + radius * 0.8), new Size(radius, radius), 0, false, SweepDirection.Clockwise, true));
                _Waves[i] = new Path
                {
                    Data = new PathGeometry(new[] { arc }),
                    Stroke = G10_RetroStyle.Frame,
                    StrokeThickness = 2
                };
                icon.Children.Add(_Waves[i]);
            }

            muteCross = new Canvas();
            muteCross.Children.Add(new Line { X1 = 21, Y1 = 9, X2 = 31, Y2 = 21, Stroke = MuteBrush, StrokeThickness = 2.5 });
            muteCross.Children.Add(new Line { X1 = 31, Y1 = 9, X2 = 21, Y2 = 21, Stroke = MuteBrush, StrokeThickness = 2.5 });
            icon.Children.Add(muteCross);
            return icon;
        }

        static Brush Frozen(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
