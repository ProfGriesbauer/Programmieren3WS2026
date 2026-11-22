using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    // Shared look of the 2D overlays: dark navy panels with a light frame, a monospace "terminal" font
    // and selected entries in a blue box.
    static class B5RetroStyle
    {
        public static readonly FontFamily Font = new FontFamily("Consolas");
        public static readonly Brush Panel = Frozen(Color.FromArgb(238, 14, 20, 48));
        public static readonly Brush Frame = Frozen(Color.FromRgb(150, 170, 230));
        public static readonly Brush Text = Frozen(Color.FromRgb(228, 234, 255));
        public static readonly Brush Dim = Frozen(Color.FromRgb(140, 152, 196));
        public static readonly Brush SelectedFill = Frozen(Color.FromRgb(38, 78, 200));
        public static readonly Brush SelectedFrame = Frozen(Color.FromRgb(130, 165, 255));

        public static Border CreatePanel(UIElement child)
        {
            return new Border
            {
                Background = Panel,
                BorderBrush = Frame,
                BorderThickness = new Thickness(2),
                Padding = new Thickness(12, 8, 12, 8),
                Child = child
            };
        }

        public static TextBlock CreateText(string text, double size, Brush brush)
        {
            return new TextBlock { Text = text, FontFamily = Font, FontSize = size, FontWeight = FontWeights.Bold, Foreground = brush };
        }

        // An entry that can be shown as selected (blue box) or not (no box).
        public static Border CreateSelectable(UIElement child)
        {
            return new Border { BorderThickness = new Thickness(2), Padding = new Thickness(10, 3, 10, 3), Child = child };
        }

        public static void SetSelected(Border entry, bool selected)
        {
            entry.Background = selected ? SelectedFill : Brushes.Transparent;
            entry.BorderBrush = selected ? SelectedFrame : Brushes.Transparent;
        }

        static Brush Frozen(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }

    // One entry of the control bar: the keys, what they do and, if the entry can be clicked, the key a click sends.
    sealed class B5ControlHint
    {
        public B5ControlHint(string keys, string label, Key? action = null)
        {
            Keys = keys;
            Label = label;
            Action = action;
        }

        public string Keys { get; private set; }
        public string Label { get; private set; }
        public Key? Action { get; private set; }
    }

    // Control bar at the bottom of the canvas: device tabs and the keys that work in the current phase.
    // Tabs and hints with an action can also be clicked.
    sealed class B5ControlBar
    {
        static readonly Brush HoverFill = Frozen(Color.FromRgb(38, 58, 130));

        readonly Border _Element;
        readonly StackPanel _Hints = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
        readonly List<Border> _Tabs = new List<Border>();
        B5RoomPhase? _HintPhase;

        // A device tab was clicked (device index).
        public event Action<int> TabClicked;

        // A hint with an action was clicked (the key it stands for).
        public event Action<Key> HintClicked;

        public B5ControlBar()
        {
            StackPanel tabs = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center };
            TextBlock caption = B5RetroStyle.CreateText("GERAET:", 15, B5RetroStyle.Dim);
            caption.VerticalAlignment = VerticalAlignment.Center;
            tabs.Children.Add(caption);
            for (int i = 0; i < B5RoomRegistry.Devices.Count; i++)
            {
                int device = i;
                Border tab = B5RetroStyle.CreateSelectable(B5RetroStyle.CreateText(B5RoomRegistry.Devices[i].Name, 15, B5RetroStyle.Text));
                tab.Margin = new Thickness(8, 0, 0, 0);
                tab.Cursor = Cursors.Hand;
                tab.MouseLeftButtonDown += (sender, e) =>
                {
                    if (TabClicked != null) TabClicked(device);
                };
                tabs.Children.Add(tab);
                _Tabs.Add(tab);
            }
            _Hints.Margin = new Thickness(0, 7, 0, 0);
            StackPanel rows = new StackPanel();
            rows.Children.Add(tabs);
            rows.Children.Add(_Hints);

            // Shrinks the bar contents on narrow canvases instead of cutting them off.
            _Element = B5RetroStyle.CreatePanel(new Viewbox { Stretch = Stretch.Uniform, StretchDirection = StretchDirection.DownOnly, Child = rows });
            _Element.HorizontalAlignment = HorizontalAlignment.Stretch;
            _Element.VerticalAlignment = VerticalAlignment.Bottom;
            _Element.Margin = new Thickness(8);
        }

        public FrameworkElement Element { get { return _Element; } }

        // Space the bar takes at the bottom of the canvas, including its margins.
        public double OccupiedHeight { get { return _Element.ActualHeight + _Element.Margin.Top + _Element.Margin.Bottom; } }

        public void Update(B5RoomField field)
        {
            for (int i = 0; i < _Tabs.Count; i++)
                B5RetroStyle.SetSelected(_Tabs[i], i == field.SelectedDevice);

            if (_HintPhase == field.Phase) return;
            _HintPhase = field.Phase;
            _Hints.Children.Clear();
            foreach (B5ControlHint hint in HintsFor(field.Phase))
            {
                Border key = new Border
                {
                    BorderBrush = B5RetroStyle.Frame,
                    BorderThickness = new Thickness(1.5),
                    Padding = new Thickness(6, 0, 6, 0),
                    Margin = new Thickness(14, 0, 6, 0),
                    Child = B5RetroStyle.CreateText(hint.Keys, 13, B5RetroStyle.Text)
                };
                TextBlock label = B5RetroStyle.CreateText(hint.Label, 13, B5RetroStyle.Dim);
                label.VerticalAlignment = VerticalAlignment.Center;
                if (hint.Action.HasValue)
                {
                    // Key box and label together form one button.
                    StackPanel content = new StackPanel { Orientation = Orientation.Horizontal };
                    content.Children.Add(key);
                    content.Children.Add(label);
                    label.Margin = new Thickness(0, 0, 6, 0);
                    Border button = new Border { Background = Brushes.Transparent, Cursor = Cursors.Hand, Child = content };
                    Key action = hint.Action.Value;
                    button.MouseEnter += (sender, e) => button.Background = HoverFill;
                    button.MouseLeave += (sender, e) => button.Background = Brushes.Transparent;
                    button.MouseLeftButtonDown += (sender, e) =>
                    {
                        if (HintClicked != null) HintClicked(action);
                    };
                    _Hints.Children.Add(button);
                }
                else
                {
                    _Hints.Children.Add(key);
                    _Hints.Children.Add(label);
                }
            }
        }

        static B5ControlHint[] HintsFor(B5RoomPhase phase)
        {
            switch (phase)
            {
                case B5RoomPhase.Room:
                    return new[]
                    {
                        new B5ControlHint("← → / A D / MAUS", "Geraet waehlen"),
                        new B5ControlHint("ENTER / Z / KLICK", "oeffnen", Key.Enter)
                    };
                case B5RoomPhase.Device:
                    return new[]
                    {
                        new B5ControlHint("↑ ↓ / W S / 1-3 / MAUS", "Spiel waehlen"),
                        new B5ControlHint("ENTER / Z / KLICK", "Modul", Key.Enter),
                        new B5ControlHint("ESC / X / RECHTSKLICK", "zurueck", Key.Escape)
                    };
                case B5RoomPhase.Playing:
                    return new[]
                    {
                        new B5ControlHint("PFEILE / WASD", "steuern"),
                        new B5ControlHint("ESC / X", "zurueck zur Geraeteauswahl", Key.Escape)
                    };
                case B5RoomPhase.Ejecting:
                    return new[] { new B5ControlHint("ESC", "abbrechen", Key.Escape), new B5ControlHint("...", "Modul wird ausgeworfen") };
                case B5RoomPhase.Focusing:
                    return new[] { new B5ControlHint("ESC / X", "zurueck", Key.Escape), new B5ControlHint("...", "Bildschirm wird fokussiert") };
                default:
                    return new[] { new B5ControlHint("ESC", "abbrechen", Key.Escape), new B5ControlHint("...", "Modul wird eingesteckt") };
            }
        }

        static Brush Frozen(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
