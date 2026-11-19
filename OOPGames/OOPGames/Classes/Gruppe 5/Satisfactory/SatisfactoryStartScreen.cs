using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    internal sealed class SatisfactoryStartScreenSettings
    {
        internal int ResourceAmount { get; set; }
        internal int MinerCycleTicks { get; set; }
        internal int SmelterCycleTicks { get; set; }
    }

    internal sealed class SatisfactoryStartScreen
    {
        private enum Page
        {
            Home,
            Settings,
            Guide
        }

        private enum Action
        {
            Start,
            Settings,
            Guide,
            Home,
            ChangeResource,
            ChangeSpeed
        }

        private sealed class HitTarget
        {
            internal Rect Bounds { get; set; }
            internal Action Action { get; set; }
        }

        private static readonly Brush BackgroundBrush = BrushFromRgb(12, 19, 23);
        private static readonly Brush CardBrush = BrushFromRgb(23, 34, 39);
        private static readonly Brush MutedBrush = BrushFromRgb(162, 181, 181);
        private static readonly Brush AccentBrush = BrushFromRgb(96, 220, 151);
        private readonly List<HitTarget> _hitTargets = new List<HitTarget>();
        private Page _page = Page.Home;
        private int _resourceChoice = 1;
        private int _speedChoice = 1;

        internal SatisfactoryStartScreenSettings SelectedSettings
        {
            get
            {
                return new SatisfactoryStartScreenSettings
                {
                    ResourceAmount = _resourceChoice == 0 ? 150 : _resourceChoice == 2 ? 600 : 300,
                    MinerCycleTicks = _speedChoice == 0 ? 55 : _speedChoice == 2 ? 22 : 35,
                    SmelterCycleTicks = _speedChoice == 0 ? 80 : _speedChoice == 2 ? 32 : 55
                };
            }
        }

        internal void ShowHome()
        {
            _page = Page.Home;
        }

        internal void Paint(Canvas canvas)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            double width = canvas.ActualWidth;
            double height = canvas.ActualHeight;
            if (width <= 0 || height <= 0)
            {
                return;
            }

            canvas.Children.Clear();
            canvas.Background = BackgroundBrush;
            _hitTargets.Clear();
            DrawBackground(canvas, width, height);

            double contentWidth = Math.Min(1040, width - 48);
            double contentHeight = Math.Min(640, height - 40);
            double left = Math.Max(24, (width - contentWidth) / 2);
            double top = Math.Max(20, (height - contentHeight) / 2);
            DrawHeader(canvas, left, top, contentWidth);

            double panelTop = top + 64;
            double panelHeight = contentHeight - 80;
            double panelWidth = (contentWidth - 20) / 2;
            DrawHeroPanel(canvas, left, panelTop, panelWidth, panelHeight);
            DrawControlPanel(canvas, left + panelWidth + 20, panelTop, panelWidth, panelHeight);
            DrawFooter(canvas, left, top + contentHeight - 8, contentWidth);
        }

        internal bool HandleClick(int x, int y)
        {
            Point click = new Point(x, y);
            foreach (HitTarget target in _hitTargets)
            {
                if (!target.Bounds.Contains(click))
                {
                    continue;
                }

                switch (target.Action)
                {
                    case Action.Start:
                        return true;
                    case Action.Settings:
                        _page = Page.Settings;
                        break;
                    case Action.Guide:
                        _page = Page.Guide;
                        break;
                    case Action.Home:
                        _page = Page.Home;
                        break;
                    case Action.ChangeResource:
                        _resourceChoice = (_resourceChoice + 1) % 3;
                        break;
                    case Action.ChangeSpeed:
                        _speedChoice = (_speedChoice + 1) % 3;
                        break;
                }

                return false;
            }

            return false;
        }

        private void DrawBackground(Canvas canvas, double width, double height)
        {
            AddRectangle(canvas, BackgroundBrush, 0, 0, width, height);
            AddCircle(canvas, BrushFromRgb(22, 52, 45), width - 370, -240, 570, 0.45);
            AddCircle(canvas, BrushFromRgb(29, 52, 58), -260, height - 260, 440, 0.35);

            for (int i = 0; i < 9; i++)
            {
                double x = width * 0.57 + i * 44;
                double y = height * 0.18 + (i % 3) * 38;
                AddRectangle(canvas, BrushFromRgb(45, 70, 67), x, y, 2, 2, 0.65);
            }
        }

        private static void DrawHeader(Canvas canvas, double left, double top, double width)
        {
            AddRoundedRectangle(canvas, AccentBrush, left, top, 40, 40, 12);
            AddText(canvas, "S", Brushes.Black, left, top + 4, 40, 31, 24, FontWeights.Bold,
                TextAlignment.Center);
            AddText(canvas, "SATISFACTORY", Brushes.White, left + 54, top + 1, 260, 20, 16,
                FontWeights.Bold);
            AddText(canvas, "GRUPPE 5  /  FABRIK-SIMULATION", MutedBrush, left + 54, top + 23,
                300, 16, 10, FontWeights.SemiBold);
            AddText(canvas, "DEINE FABRIK BEGINNT HIER", AccentBrush,
                left + width - 260, top + 11, 260, 18, 10, FontWeights.Bold,
                TextAlignment.Right);
        }

        private void DrawHeroPanel(
            Canvas canvas,
            double left,
            double top,
            double width,
            double height)
        {
            AddRoundedRectangle(canvas, CardBrush, left, top, width, height, 20);
            AddRoundedRectangle(canvas, BrushFromRgb(37, 62, 54),
                left + 24, top + 24, 116, 24, 12);
            AddText(canvas, "PRODUKTION STARTEN", AccentBrush,
                left + 34, top + 29, 100, 14, 9, FontWeights.Bold);
            AddText(canvas, "Rohstoffe rein.\nProduktion an.", Brushes.White,
                left + 26, top + 70, width - 52, 100, 35, FontWeights.Bold);
            AddText(canvas,
                "Baue deine Fabrik auf der gemeinsamen Gruppe-5-Welt auf. Förderer, Schmelzen und Rezepte verwenden die Modelle deiner Gruppenmitglieder.",
                MutedBrush, left + 28, top + 185, width - 56, 68, 13);

            DrawStep(canvas, left + 28, top + 280, "01", "Förderer",
                "F drücken und auf ein Erzvorkommen klicken.");
            DrawStep(canvas, left + 28, top + 350, "02", "Schmelze",
                "S drücken und auf freies Gelände klicken.");
            DrawStep(canvas, left + 28, top + 420, "03", "Automatische Produktion",
                "Erz und Barren werden über das Hub transportiert.");
        }

        private void DrawControlPanel(
            Canvas canvas,
            double left,
            double top,
            double width,
            double height)
        {
            AddRoundedRectangle(canvas, BrushFromRgb(19, 30, 34), left, top, width, height, 20);

            if (_page == Page.Home)
            {
                DrawHome(canvas, left, top, width, height);
            }
            else if (_page == Page.Settings)
            {
                DrawSettings(canvas, left, top, width, height);
            }
            else
            {
                DrawGuide(canvas, left, top, width, height);
            }
        }

        private void DrawHome(Canvas canvas, double left, double top, double width, double height)
        {
            AddText(canvas, "Willkommen in der Fabrik", Brushes.White,
                left + 28, top + 30, width - 56, 34, 22, FontWeights.Bold);
            AddText(canvas, "Wie möchtest du beginnen?", MutedBrush,
                left + 30, top + 70, width - 60, 22, 13);

            DrawButton(canvas, left + 28, top + 128, width - 56, 54,
                "SPIEL STARTEN", "Welt öffnen und Produktion beginnen",
                AccentBrush, BrushFromRgb(13, 30, 24), Action.Start);
            DrawButton(canvas, left + 28, top + 199, width - 56, 54,
                "EINSTELLUNGEN", "Rohstoffvorrat und Tempo festlegen",
                BrushFromRgb(38, 55, 59), Brushes.White, Action.Settings);
            DrawButton(canvas, left + 28, top + 270, width - 56, 54,
                "ANLEITUNG", "Steuerung und Spielablauf ansehen",
                BrushFromRgb(38, 55, 59), Brushes.White, Action.Guide);

            AddRoundedRectangle(canvas, BrushFromRgb(29, 47, 43),
                left + 28, top + height - 118, width - 56, 82, 12);
            AddText(canvas, "AKTUELLE EINSTELLUNGEN", AccentBrush,
                left + 42, top + height - 103, width - 84, 15, 9, FontWeights.Bold);
            AddText(canvas,
                ResourceLabel() + "  ·  " + SpeedLabel(),
                Brushes.White, left + 42, top + height - 80, width - 84, 22, 13,
                FontWeights.SemiBold);
        }

        private void DrawSettings(Canvas canvas, double left, double top, double width, double height)
        {
            DrawBackButton(canvas, left + 28, top + 26);
            AddText(canvas, "Einstellungen", Brushes.White,
                left + 28, top + 72, width - 56, 36, 24, FontWeights.Bold);
            AddText(canvas, "Klicke auf eine Auswahl, um den Wert zu ändern.",
                MutedBrush, left + 30, top + 113, width - 60, 40, 12);

            AddText(canvas, "ROHSTOFFVORRAT", AccentBrush,
                left + 30, top + 190, width - 60, 18, 10, FontWeights.Bold);
            DrawOptionButton(canvas, left + 28, top + 216, width - 56, 54,
                ResourceLabel(), "Erz pro Vorkommen", Action.ChangeResource);

            AddText(canvas, "PRODUKTIONSTEMPO", AccentBrush,
                left + 30, top + 300, width - 60, 18, 10, FontWeights.Bold);
            DrawOptionButton(canvas, left + 28, top + 326, width - 56, 54,
                SpeedLabel(), "Förderer- und Schmelzzyklen", Action.ChangeSpeed);

            DrawButton(canvas, left + 28, top + height - 96, width - 56, 54,
                "SPIEL STARTEN", "Einstellungen übernehmen",
                AccentBrush, BrushFromRgb(13, 30, 24), Action.Start);
        }

        private void DrawGuide(Canvas canvas, double left, double top, double width, double height)
        {
            DrawBackButton(canvas, left + 28, top + 26);
            AddText(canvas, "Anleitung", Brushes.White,
                left + 28, top + 72, width - 56, 36, 24, FontWeights.Bold);
            AddText(canvas, "So bringst du deine Fabrik in Gang:",
                MutedBrush, left + 30, top + 113, width - 60, 22, 12);

            DrawStep(canvas, left + 30, top + 170, "01", "Förderer auswählen",
                "Drücke F und klicke direkt auf ein Eisen- oder Kupfervorkommen.");
            DrawStep(canvas, left + 30, top + 250, "02", "Schmelze platzieren",
                "Drücke S und klicke auf eine freie Fläche.");
            DrawStep(canvas, left + 30, top + 330, "03", "Produktion beobachten",
                "Förderer bringen Erz zum Hub. Passende Schmelzen erhalten Erz und lagern Barren im Hub.");
            DrawStep(canvas, left + 30, top + 425, "04", "Werkzeug wechseln",
                "Esc hebt das Bauwerkzeug auf. Mit F oder S wählst du ein neues.");

            DrawButton(canvas, left + 28, top + height - 78, width - 56, 48,
                "SPIEL STARTEN", " ", AccentBrush, BrushFromRgb(13, 30, 24), Action.Start);
        }

        private void DrawBackButton(Canvas canvas, double left, double top)
        {
            DrawButton(canvas, left, top, 96, 34, "← ZURÜCK", " ",
                BrushFromRgb(38, 55, 59), Brushes.White, Action.Home);
        }

        private void DrawOptionButton(
            Canvas canvas,
            double left,
            double top,
            double width,
            double height,
            string value,
            string hint,
            Action action)
        {
            AddRoundedRectangle(canvas, BrushFromRgb(34, 49, 53), left, top, width, height, 10);
            AddText(canvas, value, Brushes.White, left + 14, top + 8, width - 50, 20, 13,
                FontWeights.SemiBold);
            AddText(canvas, hint, MutedBrush, left + 14, top + 29, width - 50, 16, 10);
            AddText(canvas, "›", AccentBrush, left + width - 32, top + 9, 20, 26, 22,
                FontWeights.Bold, TextAlignment.Center);
            AddHitTarget(left, top, width, height, action);
        }

        private void DrawButton(
            Canvas canvas,
            double left,
            double top,
            double width,
            double height,
            string title,
            string subtitle,
            Brush background,
            Brush foreground,
            Action action)
        {
            AddRoundedRectangle(canvas, background, left, top, width, height, 11);
            AddText(canvas, title, foreground, left + 15, top + (subtitle == " " ? 14 : 9),
                width - 30, 20, 12, FontWeights.Bold);
            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                AddText(canvas, subtitle, foreground == Brushes.White ? MutedBrush : foreground,
                    left + 15, top + 30, width - 30, 17, 10);
            }

            AddHitTarget(left, top, width, height, action);
        }

        private void DrawStep(
            Canvas canvas,
            double left,
            double top,
            string number,
            string heading,
            string detail)
        {
            AddRoundedRectangle(canvas, BrushFromRgb(41, 58, 61), left, top, 36, 36, 11);
            AddText(canvas, number, AccentBrush, left, top + 9, 36, 18, 11,
                FontWeights.Bold, TextAlignment.Center);
            AddText(canvas, heading, Brushes.White, left + 50, top, 300, 21, 13,
                FontWeights.SemiBold);
            AddText(canvas, detail, MutedBrush, left + 50, top + 24, 390, 40, 11);
        }

        private void DrawFooter(Canvas canvas, double left, double top, double width)
        {
            AddText(canvas, "BEDIENUNG IM SPIEL:  F Förderer   ·   S Schmelze   ·   Esc Werkzeug abwählen",
                MutedBrush, left, top, width, 16, 10, FontWeights.Normal, TextAlignment.Center);
        }

        private string ResourceLabel()
        {
            return _resourceChoice == 0 ? "Knapp · 150 Erz" :
                _resourceChoice == 2 ? "Reich · 600 Erz" : "Standard · 300 Erz";
        }

        private string SpeedLabel()
        {
            return _speedChoice == 0 ? "Entspannt" :
                _speedChoice == 2 ? "Schnell" : "Standard";
        }

        private void AddHitTarget(double left, double top, double width, double height, Action action)
        {
            _hitTargets.Add(new HitTarget
            {
                Bounds = new Rect(left, top, width, height),
                Action = action
            });
        }

        private static void AddRectangle(
            Canvas canvas,
            Brush brush,
            double left,
            double top,
            double width,
            double height,
            double opacity = 1)
        {
            Rectangle rectangle = new Rectangle
            {
                Width = Math.Max(0, width),
                Height = Math.Max(0, height),
                Fill = brush,
                Opacity = opacity,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            canvas.Children.Add(rectangle);
        }

        private static void AddRoundedRectangle(
            Canvas canvas,
            Brush brush,
            double left,
            double top,
            double width,
            double height,
            double radius)
        {
            Rectangle rectangle = new Rectangle
            {
                Width = Math.Max(0, width),
                Height = Math.Max(0, height),
                Fill = brush,
                RadiusX = radius,
                RadiusY = radius,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            canvas.Children.Add(rectangle);
        }

        private static void AddCircle(
            Canvas canvas,
            Brush brush,
            double left,
            double top,
            double size,
            double opacity)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = brush,
                Opacity = opacity,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);
            canvas.Children.Add(ellipse);
        }

        private static void AddText(
            Canvas canvas,
            string text,
            Brush brush,
            double left,
            double top,
            double width,
            double height,
            double fontSize,
            FontWeight fontWeight = default(FontWeight),
            TextAlignment alignment = TextAlignment.Left)
        {
            TextBlock block = new TextBlock
            {
                Text = text,
                Foreground = brush,
                FontSize = fontSize,
                FontWeight = fontWeight == default(FontWeight) ? FontWeights.Normal : fontWeight,
                TextAlignment = alignment,
                TextWrapping = TextWrapping.Wrap,
                Width = Math.Max(0, width),
                Height = Math.Max(0, height),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(block, left);
            Canvas.SetTop(block, top);
            canvas.Children.Add(block);
        }

        private static Brush BrushFromRgb(byte red, byte green, byte blue)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
            brush.Freeze();
            return brush;
        }
    }

    internal sealed class SatisfactoryFieldView : IG5_FabrikSpielInfo
    {
        private readonly G5_FabrikSpielfeld _field;
        private readonly SatisfactoryStartScreen _startScreen;

        internal SatisfactoryFieldView(
            G5_FabrikSpielfeld field,
            SatisfactoryStartScreen startScreen,
            bool isGameStarted)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _startScreen = startScreen ?? throw new ArgumentNullException(nameof(startScreen));
            IsGameStarted = isGameStarted;
        }

        internal bool IsGameStarted { get; set; }
        internal SatisfactoryStartScreen StartScreen { get { return _startScreen; } }

        int IG5_WeltSpielfeld.Breite => ((IG5_WeltSpielfeld)_field).Breite;
        int IG5_WeltSpielfeld.Hoehe => ((IG5_WeltSpielfeld)_field).Hoehe;

        G5_Bauwerkzeug IG5_FabrikSpielInfo.AktuellesWerkzeug =>
            ((IG5_FabrikSpielInfo)_field).AktuellesWerkzeug;

        IReadOnlyList<IG5_GebaeudeInfo> IG5_FabrikSpielInfo.Gebaeude =>
            ((IG5_FabrikSpielInfo)_field).Gebaeude;

        string IG5_FabrikSpielInfo.StatusMeldung =>
            ((IG5_FabrikSpielInfo)_field).StatusMeldung;

        int IG5_FabrikSpielInfo.HubMenge(G5_Item item)
        {
            return ((IG5_FabrikSpielInfo)_field).HubMenge(item);
        }

        bool IG5_WeltSpielfeld.TryGetKachelInfo(int x, int y, out IG5_KachelInfo info)
        {
            return ((IG5_WeltSpielfeld)_field).TryGetKachelInfo(x, y, out info);
        }

        bool IG5_WeltSpielfeld.TryBildschirmZuRaster(int screenX, int screenY, out int x, out int y)
        {
            if (!IsGameStarted)
            {
                x = screenX;
                y = screenY;
                return x >= 0 && y >= 0;
            }

            return ((IG5_WeltSpielfeld)_field).TryBildschirmZuRaster(screenX, screenY, out x, out y);
        }

        bool IG5_WeltSpielfeld.TryRasterZuBildschirm(int x, int y, out Point position)
        {
            return ((IG5_WeltSpielfeld)_field).TryRasterZuBildschirm(x, y, out position);
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is IG5_FabrikPainter;
        }
    }

}
