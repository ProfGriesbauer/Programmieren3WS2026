using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // Kleines Startbildschirm-Modell mit Home-, Einstellungs- und Anleitung-Seite.
    // Die Klasse zeichnet WPF-Canvas und verwaltet ihre Klickbereiche objektorientiert.
    internal sealed class SatisfactoryStartScreenSettings
    {
        internal int ResourceAmount { get; set; }
        internal int MinerCycleTicks { get; set; }
        internal int SmelterCycleTicks { get; set; }
        internal bool TutorialEnabled { get; set; }
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
            ChangeSpeed,
            ToggleTutorial
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
        private bool _tutorialEnabled = true;

        internal SatisfactoryStartScreenSettings SelectedSettings
        {
            get
            {
                return new SatisfactoryStartScreenSettings
                {
                    ResourceAmount = _resourceChoice == 0 ? 150 : _resourceChoice == 2 ? 600 : 300,
                    MinerCycleTicks = _speedChoice == 0 ? 55 : _speedChoice == 2 ? 22 : 35,
                    SmelterCycleTicks = _speedChoice == 0 ? 80 : _speedChoice == 2 ? 32 : 55,
                    TutorialEnabled = _tutorialEnabled
                };
            }
        }

        internal void ShowHome()
        {
            _page = Page.Home;
        }

        internal void Paint(Canvas canvas)
        {
            // Zeichnet die aktuelle Startbildschirmseite als WPF-Canvas.
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
            // Uebersetzt Mausklickkoordinaten in Startbildschirmaktionen; Start liefert true zurueck.
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
                    case Action.ToggleTutorial:
                        _tutorialEnabled = !_tutorialEnabled;
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
            AddText(canvas, "BAUEN  /  ERWEITERN  /  AUTOMATISIEREN", MutedBrush, left + 54, top + 23,
                340, 16, 10, FontWeights.SemiBold);
            AddText(canvas, "DEINE WELT. DEINE FABRIK.", AccentBrush,
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
            AddText(canvas, "Sammeln.\nBauen. Automatisieren.", Brushes.White,
                left + 26, top + 70, width - 52, 100, 35, FontWeights.Bold);
            AddText(canvas,
                "Du startest ohne Gebäude. Laufe mit WASD, baue Rohstoffe mit E ab und entwickle deine Fabrik Schritt für Schritt.",
                MutedBrush, left + 28, top + 185, width - 206, 68, 13);
            DrawFactoryArt(canvas, left + width - 166, top + 170);

            DrawStep(canvas, left + 28, top + 280, "01", "Rohstoffe sammeln",
                "Schaue auf ein Rohstofffeld und drücke E, um selbst abzubauen.");
            DrawStep(canvas, left + 28, top + 350, "02", "Gebäude bauen",
                "Förderer stehen auf Vorkommen; andere Gebäude auf freien Feldern.");
            DrawStep(canvas, left + 28, top + 420, "03", "Fabrik verwalten",
                "Verbinde Förderer und Schmelze mit Förderbändern; wähle passende Rezepte.");
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
                ResourceLabel() + "  ·  " + SpeedLabel() + "  ·  Mentor " + TutorialLabel(),
                Brushes.White, left + 42, top + height - 80, width - 84, 22, 13,
                FontWeights.SemiBold);
            AddText(canvas, "Du beginnst ohne Gebäude und baust dein erstes Werk aus selbst gesammelten Rohstoffen.",
                MutedBrush, left + 30, top + 340, width - 60, 38, 11);
        }

        private void DrawSettings(Canvas canvas, double left, double top, double width, double height)
        {
            DrawBackButton(canvas, left + 28, top + 26);
            AddText(canvas, "Einstellungen", Brushes.White,
                left + 28, top + 72, width - 56, 36, 24, FontWeights.Bold);
            AddText(canvas, "Klicke auf eine Auswahl, um den Wert zu ändern.",
                MutedBrush, left + 30, top + 113, width - 60, 22, 12);

            AddText(canvas, "ROHSTOFFVORRAT", AccentBrush,
                left + 30, top + 148, width - 60, 18, 10, FontWeights.Bold);
            DrawOptionButton(canvas, left + 28, top + 170, width - 56, 54,
                ResourceLabel(), "Erz pro Vorkommen", Action.ChangeResource);

            AddText(canvas, "PRODUKTIONSTEMPO", AccentBrush,
                left + 30, top + 238, width - 60, 18, 10, FontWeights.Bold);
            DrawOptionButton(canvas, left + 28, top + 260, width - 56, 54,
                SpeedLabel(), "Förderer- und Schmelzzyklen", Action.ChangeSpeed);

            AddText(canvas, "TUTORIAL", AccentBrush,
                left + 30, top + 322, width - 60, 18, 10, FontWeights.Bold);
            DrawOptionButton(canvas, left + 28, top + 344, width - 56, 54,
                TutorialLabel(), "Mentorhinweise während des Spiels", Action.ToggleTutorial);

            AddText(canvas, "WASD bewegen · E baut Rohstoffe ab · Mentorhinweise: " + TutorialLabel(),
                MutedBrush, left + 30, top + Math.Max(404, height - 150), width - 60, 40, 11);

            DrawButton(canvas, left + 28, top + height - 96, width - 56, 54,
                "SPIEL STARTEN", "Einstellungen übernehmen",
                AccentBrush, BrushFromRgb(13, 30, 24), Action.Start);
        }

        private void DrawGuide(Canvas canvas, double left, double top, double width, double height)
        {
            DrawBackButton(canvas, left + 28, top + 26);
            AddText(canvas, "Anleitung", Brushes.White,
                left + 28, top + 72, width - 56, 36, 24, FontWeights.Bold);
            AddText(canvas, "Baue Rohstoffe selbst ab und automatisiere ihre Verarbeitung:",
                MutedBrush, left + 30, top + 113, width - 60, 22, 12);

            double stepsTop = top + Math.Max(136, (height - 330) / 2);
            DrawStep(canvas, left + 30, stepsTop, "01", "Laufen und abbauen",
                "WASD bewegt dich. Laufe neben ein Vorkommen, richte den Blick darauf und drücke E.");
            DrawStep(canvas, left + 30, stepsTop + 70, "02", "Gebäude kaufen",
                "Der erste Förderer braucht nur 8 Eisenerz und 5 Holz. Verarbeite weitere Zutaten im Inventar unter HANDWERK.");
            DrawStep(canvas, left + 30, stepsTop + 140, "03", "Produktion einstellen",
                "HANDWERK ordnet Rezepte nach Rohstoffen, Barren und Bauteilen. F Förderer, S Schmelze, B Förderband.");
            DrawStep(canvas, left + 30, stepsTop + 210, "04", "Strom und Handwerk",
                "Generator verbrennt Kohle; Strommasten verbinden Maschinen. Rezepte stehen in der Anleitung.");
            AddText(canvas, "Handwerk: Erz → Barren, Kalkstein → Beton, Barren → Platten/Stangen. T überspringt den Mentor.",
                MutedBrush, left + 30, stepsTop + 285, width - 60, 34, 10);

            DrawButton(canvas, left + 28, top + height - 60, width - 56, 44,
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

        private static void DrawFactoryArt(Canvas canvas, double left, double top)
        {
            AddRoundedRectangle(canvas, BrushFromRgb(48, 96, 84), left, top + 38, 146, 78, 20);
            AddRoundedRectangle(canvas, BrushFromRgb(71, 130, 102), left + 14, top + 50, 56, 66, 8);
            AddRoundedRectangle(canvas, BrushFromRgb(208, 126, 62), left + 76, top + 30, 55, 86, 8);
            AddRoundedRectangle(canvas, BrushFromRgb(87, 107, 111), left + 89, top + 10, 14, 30, 4);
            AddRoundedRectangle(canvas, BrushFromRgb(87, 107, 111), left + 115, top + 18, 12, 24, 4);
            AddCircle(canvas, BrushFromRgb(183, 199, 183), left + 82, top - 8, 24, 0.6);
            AddCircle(canvas, BrushFromRgb(183, 199, 183), left + 99, top - 15, 30, 0.48);
            AddCircle(canvas, BrushFromRgb(183, 199, 183), left + 119, top - 5, 23, 0.4);
            AddRoundedRectangle(canvas, BrushFromRgb(226, 181, 73), left + 23, top + 72, 15, 18, 3);
            AddRoundedRectangle(canvas, BrushFromRgb(226, 181, 73), left + 46, top + 72, 15, 18, 3);
            AddRoundedRectangle(canvas, BrushFromRgb(242, 197, 91), left + 87, top + 52, 14, 18, 3);
            AddRoundedRectangle(canvas, BrushFromRgb(242, 197, 91), left + 107, top + 52, 14, 18, 3);
            AddRoundedRectangle(canvas, BrushFromRgb(50, 78, 63), left - 4, top + 112, 156, 9, 5);
        }

        private void DrawFooter(Canvas canvas, double left, double top, double width)
        {
            AddText(canvas, "BEDIENUNG IM SPIEL:  WASD laufen   ·   E abbauen   ·   Gebäude über das Baumenü kaufen",
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

        private string TutorialLabel()
        {
            return _tutorialEnabled ? "Aktiv" : "Aus";
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

    internal sealed class SatisfactoryFieldView : IG5_FabrikSpielInfo, IG5_KartenansichtSteuerung
    {
        private readonly G5_FabrikSpielfeld _field;
        private readonly SatisfactoryStartScreen _startScreen;
        private readonly SatisfactoryGameMentor _mentor;
        private Rect _inventarBereich;
        private Rect _baumenueBereich;
        private Rect _gebaeudeMenueBereich;
        private readonly List<(Rect Bereich, G5_FabrikAktionstyp Typ, G5_Bauwerkzeug Werkzeug, string Wert)> _menuAktionen =
            new List<(Rect, G5_FabrikAktionstyp, G5_Bauwerkzeug, string)>();
        private (G5_Bauwerkzeug Werkzeug, G5_GridPosition Position)? _pendingBuild;
        private G5_Bauwerkzeug? _pendingToolSelection;
        private (G5_GridPosition Start, G5_GridPosition Ende)? _pendingConveyorLine;
        private bool _inventarGeoeffnet = true;
        private bool _handwerkGeoeffnet;

        internal SatisfactoryFieldView(
            G5_FabrikSpielfeld field,
            SatisfactoryStartScreen startScreen,
            SatisfactoryGameMentor mentor,
            bool isGameStarted)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _startScreen = startScreen ?? throw new ArgumentNullException(nameof(startScreen));
            _mentor = mentor ?? throw new ArgumentNullException(nameof(mentor));
            MentorOverlay = new Canvas { IsHitTestVisible = false };
            IsGameStarted = isGameStarted;
        }

        internal bool IsGameStarted { get; set; }
        internal SatisfactoryStartScreen StartScreen { get { return _startScreen; } }
        internal SatisfactoryGameMentor Mentor { get { return _mentor; } }
        internal Canvas MentorOverlay { get; }
        internal bool GebaeuderegisterGeoeffnet => _field.GebaeuderegisterGeoeffnet;
        internal bool InventarGeoeffnet => _inventarGeoeffnet;
        internal bool HandwerkGeoeffnet => _handwerkGeoeffnet;
        internal IG5_GebaeudeInfo AusgewaehltesGebaeude =>
            ((IG5_FabrikSpielInfo)_field).AusgewaehltesGebaeude;
        internal IReadOnlyDictionary<G5_Item, int> SpielerInventar =>
            ((IG5_FabrikSpielInfo)_field).SpielerInventar;
        internal G5_Item? AusgewaehlterAbwurfGegenstand => _field.AusgewaehlterAbwurfGegenstand;
        internal string MengeEingabe => _field.MengeEingabe;
        internal int HubMenge(G5_Item item) =>
            ((IG5_FabrikSpielInfo)_field).HubMenge(item);

        internal void ToggleGebaeuderegister()
        {
            _field.ToggleGebaeuderegister();
        }

        internal void ToggleInventar()
        {
            _inventarGeoeffnet = !_inventarGeoeffnet;
        }

        internal void ToggleHandwerk()
        {
            _handwerkGeoeffnet = !_handwerkGeoeffnet;
        }

        internal void SetInventarBereich(Rect bereich)
        {
            _inventarBereich = bereich;
        }

        internal void SetBaumenueBereich(Rect bereich)
        {
            _baumenueBereich = bereich;
        }

        internal void SetGebaeudeMenueBereich(Rect bereich)
        {
            _gebaeudeMenueBereich = bereich;
        }

        internal void BeginMenuAktionen()
        {
            _menuAktionen.Clear();
        }

        internal void RegisterMenuAktion(
            Rect bereich,
            G5_FabrikAktionstyp typ,
            G5_Bauwerkzeug werkzeug = G5_Bauwerkzeug.Keines,
            string wert = null)
        {
            _menuAktionen.Add((bereich, typ, werkzeug, wert ?? string.Empty));
        }

        internal bool TryGetMenuAktion(
            int screenX,
            int screenY,
            out G5_FabrikAktionstyp typ,
            out G5_Bauwerkzeug werkzeug,
            out string wert)
        {
            Point click = new Point(screenX, screenY);
            foreach (var aktion in _menuAktionen)
            {
                if (aktion.Bereich.Contains(click))
                {
                    typ = aktion.Typ;
                    werkzeug = aktion.Werkzeug;
                    wert = aktion.Wert;
                    return true;
                }
            }

            typ = default;
            werkzeug = G5_Bauwerkzeug.Keines;
            wert = string.Empty;
            return false;
        }

        internal void QueuePendingBuild(G5_Bauwerkzeug werkzeug, G5_GridPosition position)
        {
            _pendingBuild = (werkzeug, position);
        }

        internal void QueuePendingToolSelection(G5_Bauwerkzeug werkzeug)
        {
            _pendingToolSelection = werkzeug;
        }

        internal bool TryConsumePendingToolSelection(out G5_Bauwerkzeug werkzeug)
        {
            if (!_pendingToolSelection.HasValue)
            {
                werkzeug = G5_Bauwerkzeug.Keines;
                return false;
            }

            werkzeug = _pendingToolSelection.Value;
            _pendingToolSelection = null;
            return true;
        }

        internal void QueuePendingConveyorLine(G5_GridPosition start, G5_GridPosition end)
        {
            _pendingConveyorLine = (start, end);
        }

        internal bool TryConsumePendingConveyorLine(
            out G5_GridPosition start,
            out G5_GridPosition end)
        {
            if (!_pendingConveyorLine.HasValue)
            {
                start = default;
                end = default;
                return false;
            }

            start = _pendingConveyorLine.Value.Start;
            end = _pendingConveyorLine.Value.Ende;
            _pendingConveyorLine = null;
            return true;
        }

        internal bool TryConsumePendingBuild(
            out G5_Bauwerkzeug werkzeug,
            out G5_GridPosition position)
        {
            if (!_pendingBuild.HasValue)
            {
                werkzeug = G5_Bauwerkzeug.Keines;
                position = default;
                return false;
            }

            werkzeug = _pendingBuild.Value.Werkzeug;
            position = _pendingBuild.Value.Position;
            _pendingBuild = null;
            return true;
        }

        internal void CenterCamera(double viewportWidth, double viewportHeight)
        {
            if (viewportWidth <= 0 || viewportHeight <= 66)
            {
                return;
            }

            const double zoom = G5_Kartenansicht.StandardZoom;
            const double tileSize = 32;
            double cellSize = tileSize * zoom;
            double mapWidth = ((IG5_WeltSpielfeld)_field).Breite * cellSize;
            double mapHeight = ((IG5_WeltSpielfeld)_field).Hoehe * cellSize;
            G5_GridPosition player = ((IG5_FabrikSpielInfo)_field).SpielerPosition;
            double offsetX = viewportWidth / 2 - (player.X + 0.5) * cellSize;
            double offsetY = 66 + (viewportHeight - 66) / 2 - (player.Y + 0.5) * cellSize;
            offsetX = Math.Max(Math.Min(0, viewportWidth - mapWidth), Math.Min(0, offsetX));
            offsetY = Math.Max(Math.Min(66, viewportHeight - mapHeight), Math.Min(66, offsetY));
            ((IG5_KartenansichtSteuerung)_field).SetzeAnsicht(zoom, offsetX, offsetY);
        }

        internal bool IsInventarBereich(int screenX, int screenY)
        {
            return _inventarBereich.Contains(new Point(screenX, screenY));
        }

        internal bool IsMenueBereich(int screenX, int screenY)
        {
            Point position = new Point(screenX, screenY);
            return _baumenueBereich.Contains(position) || _gebaeudeMenueBereich.Contains(position);
        }

        internal bool TryGetConveyorRoute(
            int x,
            int y,
            out G5_ConveyorDirection inputDirection,
            out G5_ConveyorDirection outputDirection)
        {
            G5_Conveyor conveyor = null;
            foreach (G5_Building building in _field.InterneGebaeude)
            {
                if (building is G5_Conveyor candidate &&
                    candidate.Position.X == x &&
                    candidate.Position.Y == y)
                {
                    conveyor = candidate;
                    break;
                }
            }

            if (conveyor == null)
            {
                inputDirection = default;
                outputDirection = default;
                return false;
            }

            if (conveyor.TryGetFlowDirections(out inputDirection, out outputDirection))
            {
                return true;
            }

            int adjacentConveyorCount = 0;
            G5_ConveyorDirection firstConveyorDirection = default;
            G5_ConveyorDirection secondConveyorDirection = default;
            int adjacentBuildingCount = 0;
            G5_Building firstAdjacentBuilding = null;
            G5_ConveyorDirection firstBuildingDirection = default;
            G5_ConveyorDirection secondBuildingDirection = default;
            foreach (G5_Building building in _field.InterneGebaeude)
            {
                if (ReferenceEquals(building, conveyor) ||
                    !conveyor.TryGetDirectionTo(building, out G5_ConveyorDirection direction))
                {
                    continue;
                }

                if (building is G5_Conveyor)
                {
                    if (adjacentConveyorCount == 0)
                    {
                        firstConveyorDirection = direction;
                    }
                    else if (adjacentConveyorCount == 1)
                    {
                        secondConveyorDirection = direction;
                    }

                    adjacentConveyorCount++;
                }
                else
                {
                    if (adjacentBuildingCount == 0)
                    {
                        firstAdjacentBuilding = building;
                        firstBuildingDirection = direction;
                    }
                    else if (adjacentBuildingCount == 1)
                    {
                        secondBuildingDirection = direction;
                    }

                    adjacentBuildingCount++;
                }
            }

            if (adjacentConveyorCount == 0 && adjacentBuildingCount == 0)
            {
                inputDirection = default;
                outputDirection = default;
                return false;
            }

            if (adjacentBuildingCount > 0 && adjacentConveyorCount > 0)
            {
                if (firstAdjacentBuilding is G5_Miner ||
                    firstAdjacentBuilding is G5_ProductionBuilding)
                {
                    inputDirection = firstBuildingDirection;
                    outputDirection = firstConveyorDirection;
                }
                else
                {
                    inputDirection = firstConveyorDirection;
                    outputDirection = firstBuildingDirection;
                }

                return true;
            }

            if (adjacentConveyorCount >= 2)
            {
                inputDirection = firstConveyorDirection;
                outputDirection = secondConveyorDirection;
                return true;
            }

            if (adjacentBuildingCount >= 2)
            {
                inputDirection = firstBuildingDirection;
                outputDirection = secondBuildingDirection;
                return true;
            }

            if (adjacentBuildingCount == 1)
            {
                if (firstAdjacentBuilding is G5_Miner ||
                    firstAdjacentBuilding is G5_ProductionBuilding)
                {
                    inputDirection = firstBuildingDirection;
                    outputDirection = OppositeConveyorDirection(firstBuildingDirection);
                }
                else
                {
                    inputDirection = OppositeConveyorDirection(firstBuildingDirection);
                    outputDirection = firstBuildingDirection;
                }

                return true;
            }

            inputDirection = firstConveyorDirection;
            outputDirection = OppositeConveyorDirection(firstConveyorDirection);
            return true;
        }

        private static G5_ConveyorDirection OppositeConveyorDirection(G5_ConveyorDirection direction)
        {
            switch (direction)
            {
                case G5_ConveyorDirection.North:
                    return G5_ConveyorDirection.South;
                case G5_ConveyorDirection.East:
                    return G5_ConveyorDirection.West;
                case G5_ConveyorDirection.South:
                    return G5_ConveyorDirection.North;
                case G5_ConveyorDirection.West:
                    return G5_ConveyorDirection.East;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction));
            }
        }

        internal bool TryGetConveyorTransportProgress(int x, int y, out double progress)
        {
            foreach (G5_Conveyor conveyor in _field.InterneGebaeude.OfType<G5_Conveyor>())
            {
                if (conveyor.Position.X == x && conveyor.Position.Y == y)
                {
                    if (conveyor.Contents.Values.Sum() > 0)
                    {
                        progress = conveyor.TransportProgress;
                        return true;
                    }
                }
            }

            progress = 0;
            return false;
        }

        int IG5_WeltSpielfeld.Breite => ((IG5_WeltSpielfeld)_field).Breite;
        int IG5_WeltSpielfeld.Hoehe => ((IG5_WeltSpielfeld)_field).Hoehe;

        G5_Bauwerkzeug IG5_FabrikSpielInfo.AktuellesWerkzeug =>
            ((IG5_FabrikSpielInfo)_field).AktuellesWerkzeug;

        IReadOnlyList<IG5_GebaeudeInfo> IG5_FabrikSpielInfo.Gebaeude =>
            ((IG5_FabrikSpielInfo)_field).Gebaeude;

        IG5_GebaeudeInfo IG5_FabrikSpielInfo.AusgewaehltesGebaeude =>
            ((IG5_FabrikSpielInfo)_field).AusgewaehltesGebaeude;

        G5_GridPosition IG5_FabrikSpielInfo.SpielerPosition =>
            ((IG5_FabrikSpielInfo)_field).SpielerPosition;

        G5_GridPosition IG5_FabrikSpielInfo.ZielKachel =>
            ((IG5_FabrikSpielInfo)_field).ZielKachel;

        G5_SpielerBlickrichtung IG5_FabrikSpielInfo.SpielerBlickrichtung =>
            ((IG5_FabrikSpielInfo)_field).SpielerBlickrichtung;

        IReadOnlyDictionary<G5_Item, int> IG5_FabrikSpielInfo.SpielerInventar =>
            ((IG5_FabrikSpielInfo)_field).SpielerInventar;

        int IG5_FabrikSpielInfo.SpielerInventarKapazitaet =>
            ((IG5_FabrikSpielInfo)_field).SpielerInventarKapazitaet;

        int IG5_FabrikSpielInfo.SpielerInventarBelegt =>
            ((IG5_FabrikSpielInfo)_field).SpielerInventarBelegt;

        IReadOnlyList<G5_AbgelegterGegenstandInfo> IG5_FabrikSpielInfo.AbgelegteGegenstaende =>
            ((IG5_FabrikSpielInfo)_field).AbgelegteGegenstaende;

        bool IG5_FabrikSpielInfo.BaumenueGeoeffnet =>
            ((IG5_FabrikSpielInfo)_field).BaumenueGeoeffnet;

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

            if (screenY < 56)
            {
                x = -1;
                y = -1;
                return false;
            }

            double registerWidth = _field.GebaeuderegisterGeoeffnet ? 220 : 38;
            double registerHeight = _field.GebaeuderegisterGeoeffnet ? 253 : 42;
            if (screenX >= 10 && screenX <= 10 + registerWidth &&
                screenY >= 66 && screenY <= 66 + registerHeight)
            {
                x = -1;
                y = -1;
                return false;
            }

            if (IsInventarBereich(screenX, screenY))
            {
                x = -1;
                y = -1;
                return false;
            }

            if (IsMenueBereich(screenX, screenY))
            {
                x = -1;
                y = -1;
                return false;
            }

            return ((IG5_WeltSpielfeld)_field).TryBildschirmZuRaster(screenX, screenY, out x, out y);
        }

        bool IG5_WeltSpielfeld.TryRasterZuBildschirm(int x, int y, out Point position)
        {
            return ((IG5_WeltSpielfeld)_field).TryRasterZuBildschirm(x, y, out position);
        }

        void IG5_KartenansichtSteuerung.Verschieben(double deltaX, double deltaY)
        {
            ((IG5_KartenansichtSteuerung)_field).Verschieben(deltaX, deltaY);
        }

        void IG5_KartenansichtSteuerung.Zoomieren(double faktor, double fokusX, double fokusY)
        {
            ((IG5_KartenansichtSteuerung)_field).Zoomieren(faktor, fokusX, fokusY);
        }

        void IG5_KartenansichtSteuerung.SetzeAnsicht(double zoom, double verschiebungX, double verschiebungY)
        {
            ((IG5_KartenansichtSteuerung)_field).SetzeAnsicht(zoom, verschiebungX, verschiebungY);
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is IG5_FabrikPainter;
        }
    }

}
