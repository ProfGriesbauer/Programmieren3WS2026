using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_GuestGameCatalog
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Findet zur Laufzeit Spiele anderer Gruppen über die im Framework registrierten Rules,
    //               Painter und Player (OOPGamesManager.Singleton). Kompatibilität wird ausschließlich mit den
    //               Framework-Methoden CanBePaintedBy und CanBeRuledBy geprüft. Dateien anderer Gruppen werden
    //               weder gelesen noch verändert; das eigene Casino wird ausgelassen. Höchstens neun Spiele.
    // Änderung durch: Patrick Lupierz – Datum: 18.11.2026 – Änderung (SprintPlanning, Arcade-Gast-Automat):
    //               Zu jeden Rules werden automatisch Painter, menschlicher Spieler und KI DERSELBEN Gruppe gewählt
    //               (Gruppe = Präfix des Klassennamens vor dem ersten "_", z. B. G10_, FG_, Gruppe2_). Bei mehreren
    //               Spielen einer Gruppe entscheidet der längste gemeinsame Klassennamen-Anfang (G10_VierGewinnt...).
    //               Nur wenn eine Gruppe ein Teil nicht hat, wird ein passendes Teil einer anderen Gruppe genommen.
    // =====================================================
    public sealed class G1C_GuestGameEntry
    {
        public string Name { get; set; }
        public string Group { get; set; }
        public Func<IGameRules> CreateRules { get; set; }
        public Func<IPaintGame> CreatePainter { get; set; }
        public Func<IGamePlayer> CreateHuman { get; set; }
        public Func<IGamePlayer> CreateOpponent { get; set; }
        public bool HasComputerOpponent { get; set; }
        public string PainterType { get; set; }
        public string HumanType { get; set; }
        public string ComputerType { get; set; }

        /// <summary>true, wenn Painter, Spieler und KI aus derselben Gruppe wie die Rules stammen.</summary>
        public bool AllFromOwnGroup { get; set; }
    }

    public static class G1C_GuestGameCatalog
    {
        // Entwickler: Patrick Lupierz – Spiele aus dem laufenden Framework (Gast-Automat)
        public static List<G1C_GuestGameEntry> DiscoverRegistered()
        {
            OOPGamesManager m = OOPGamesManager.Singleton;
            return Discover(m.Rules, m.Painters, m.Players);
        }

        // Entwickler: Patrick Lupierz – Datum: 18.11.2026 – Gruppe aus dem Klassennamen (Text vor dem ersten "_")
        public static string GroupOf(object part)
        {
            if (part == null) return "";
            string name = part.GetType().Name;
            int cut = name.IndexOf('_');
            return cut > 0 ? name.Substring(0, cut) : name;
        }

        // Entwickler: Patrick Lupierz – Datum: 18.11.2026
        // Aufgabe: Wie gut passt ein Teil zu den Rules? Gleiche Gruppe zählt am meisten, danach die Länge des
        //          gemeinsamen Klassennamen-Anfangs (trennt mehrere Spiele derselben Gruppe).
        public static int Affinity(object rules, object part)
        {
            string a = rules.GetType().Name, b = part.GetType().Name;
            int common = 0;
            while (common < a.Length && common < b.Length && a[common] == b[common]) common++;
            return (GroupOf(rules) == GroupOf(part) ? 1000 : 0) + common;
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: passende Painter/Spieler je Rules bestimmen. Fehler fremder Klassen überspringen das Spiel.
        // Änderung durch: Patrick Lupierz – 18.11.2026 – Auswahl nach Gruppe (Affinity) statt "erstes passendes".
        public static List<G1C_GuestGameEntry> Discover(IEnumerable<IGameRules> rules, IEnumerable<IPaintGame> painters,
                                                        IEnumerable<IGamePlayer> players)
        {
            List<G1C_GuestGameEntry> result = new List<G1C_GuestGameEntry>();
            List<IPaintGame> painterList = (painters ?? Enumerable.Empty<IPaintGame>()).Where(p => p != null && !(p is IG1C_CasinoPainter)).ToList();
            List<IGamePlayer> playerList = (players ?? Enumerable.Empty<IGamePlayer>()).Where(p => p != null).ToList();
            foreach (IGameRules registered in rules ?? Enumerable.Empty<IGameRules>())
            {
                if (registered == null || registered is IG1C_CasinoRules || result.Count >= G1C_ArcadeCatalog.MaxGamesPerCabinet) continue;
                try
                {
                    IGameRules probe = Fresh(registered);
                    probe.ClearField();
                    IPaintGame painter = Best(registered, painterList.Where(p => probe.CurrentField != null && Safe(() => probe.CurrentField.CanBePaintedBy(p))));
                    IGamePlayer human = Best(registered, playerList.Where(p => p is IHumanGamePlayer && Safe(() => p.CanBeRuledBy(probe))));
                    if (painter == null || human == null) continue;
                    IGamePlayer computer = Best(registered, playerList.Where(p => p is IComputerGamePlayer && Safe(() => p.CanBeRuledBy(probe))));
                    IGameRules template = registered;
                    string group = GroupOf(registered);
                    result.Add(new G1C_GuestGameEntry
                    {
                        Name = registered.Name,
                        Group = group,
                        CreateRules = () => Fresh(template),
                        CreatePainter = () => Fresh(painter),
                        CreateHuman = () => human.Clone(),
                        CreateOpponent = () => (computer ?? human).Clone(),
                        HasComputerOpponent = computer != null,
                        PainterType = painter.GetType().Name,
                        HumanType = human.GetType().Name,
                        ComputerType = computer != null ? computer.GetType().Name : null,
                        AllFromOwnGroup = GroupOf(painter) == group && GroupOf(human) == group && (computer == null || GroupOf(computer) == group)
                    });
                }
                catch (Exception)
                {
                    // fremdes Spiel nicht startbar – im Gast-Automaten einfach nicht anbieten
                }
            }
            return result;
        }

        private static T Best<T>(IGameRules rules, IEnumerable<T> candidates) where T : class
        {
            return candidates.OrderByDescending(c => Affinity(rules, c)).FirstOrDefault();
        }

        private static bool Safe(Func<bool> check)
        {
            try { return check(); }
            catch (Exception) { return false; }
        }

        // Neue Instanz über den parameterlosen Konstruktor, sonst das registrierte Objekt selbst
        private static T Fresh<T>(T template) where T : class
        {
            try
            {
                if (template.GetType().GetConstructor(Type.EmptyTypes) != null) return (T)Activator.CreateInstance(template.GetType());
            }
            catch (Exception) { }
            return template;
        }
    }

    // =====================================================
    // Klasse: G1C_GuestGameHost
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Spielt ein Framework-Spiel einer anderen Gruppe im "Bildschirm" des Gast-Automaten.
    //               Ablauf wie im Hauptfenster: StartedGameCall, ClearField, Züge über IHumanGamePlayer.GetMove,
    //               danach Computerzüge, Gewinnerprüfung mit CheckIfPLayerWon. Gezeichnet wird mit dem fremden
    //               Painter auf ein eigenes, nicht angezeigtes Canvas; das Bild erscheint im Automatenfenster.
    //               [Esc] verlässt den Automaten jederzeit. Sieg = 20 Chips (einmal pro Runde), Einsatz 5 Chips.
    // Änderung durch: Patrick Lupierz – Datum: 18.11.2026 – Änderung: Fehler "fremde Spiele laufen nicht durch"
    //               behoben. Mehrere Gruppen lesen OOPGamesManager.Singleton.ActivePainter/ActiveRules/ActivePlayers
    //               (z. B. um einen Klick in eine Spalte umzurechnen). Dort stand bisher das Casino. Jetzt werden
    //               während der Gast-Partie die fremden Teile als aktiv angemeldet – genau wie StartGame_Click im
    //               Hauptfenster – und beim Spielende bzw. [Esc] wieder auf den vorherigen Stand zurückgesetzt.
    //               Das Ergebnis bestimmen allein die Regeln der Gruppe (CheckIfPLayerWon / MovesPossible).
    // =====================================================
    public sealed class G1C_GuestGameHost : G1C_CasinoGameBase
    {
        public const int EntryFee = 5;
        public const int WinReward = 20;
        public const int CanvasW = 480, CanvasH = 320;
        private const double ViewW = 330, ViewH = 220;

        private readonly G1C_GuestGameEntry _entry;
        private readonly Canvas _canvas = new Canvas { Width = CanvasW, Height = CanvasH, Background = Brushes.White };
        private IGameRules _rules;
        private IPaintGame _painter;
        private IGamePlayer _player1, _player2, _current;
        private BitmapSource _frame;
        private bool _rewardPaid;
        private int _ticks;
        private bool _computerWaiting; // KI hat zuletzt (noch) keinen Zug geliefert – nur alle 3 Ticks neu fragen
        private bool _busy; // Patrick Lupierz – 18.11.2026: manche KIs warten per Dispatcher-Frame (Tick läuft dann verschachtelt)

        // vorheriger Framework-Stand (Casino), wird nach der Gast-Partie wiederhergestellt
        private bool _activated;
        private IGameRules _previousRules;
        private IPaintGame _previousPainter;
        private List<IGamePlayer> _previousPlayers;

        public G1C_GuestGameHost(G1C_MoneyManager money, Random random, G1C_GuestGameEntry entry) : base(money, random)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
            Message = "[E] Starten (" + EntryFee + " Chips). Sieg: " + WinReward + " Chips. [Esc] zurück zur Spielauswahl.";
        }

        public override string Name { get { return _entry.Name.Length > 28 ? _entry.Name.Substring(0, 27) + "…" : _entry.Name; } }
        public G1C_GuestGameEntry Entry { get { return _entry; } }
        public bool Running { get; private set; }
        public int? Winner { get; private set; }
        public bool Finished { get; private set; }
        public int MovesMade { get; private set; }
        public string LastError { get; private set; }
        public IGameRules Rules { get { return _rules; } }
        public IPaintGame Painter { get { return _painter; } }
        public IGamePlayer CurrentPlayer { get { return _current; } }
        public BitmapSource Frame { get { return _frame; } }
        public bool FrameworkActivated { get { return _activated; } }

        protected override Color BackgroundColor { get { return Color.FromRgb(0x12, 0x0E, 0x1C); } }

        // Änderung durch: Patrick Lupierz – 18.11.2026 – [Esc] verlässt immer (auch mitten in der Partie) und stellt
        // den Framework-Stand wieder her; der GameManager kehrt danach zur Spielauswahl zurück.
        protected override void OnKey(Key key)
        {
            if (key == Key.Escape || (!Running && key == Key.Q)) { Leave(); return; }
            if (!Running)
            {
                if (G1C_InputManager.IsConfirm(key)) Start();
                return;
            }
            Forward(new KeySelection(key));
        }

        public void Leave()
        {
            Running = false;
            Restore();
            RequestExit();
        }

        protected override void OnClick(double x, double y)
        {
            double vx = ViewX, vy = ViewY;
            if (!Running || x < vx || y < vy || x > vx + ViewW || y > vy + ViewH) return;
            Forward(new ClickSelection((int)((x - vx) / ViewW * CanvasW), (int)((y - vy) / ViewH * CanvasH), (int)MouseButton.Left));
        }

        private static double ViewX { get { return (G1C_Renderer.ScreenWidth - ViewW) / 2; } }
        private const double ViewY = 34;

        // Entwickler: Patrick Lupierz – neue Partie wie im Hauptfenster (StartGame_Click) starten
        public bool Start()
        {
            if (!Money.TryWithdraw(EntryFee)) { Message = "Ein Spiel kostet " + EntryFee + " Chips."; return false; }
            try
            {
                _rules = _entry.CreateRules();
                _painter = _entry.CreatePainter();
                _player1 = _entry.CreateHuman();
                _player2 = _entry.CreateOpponent();
                _player1.SetPlayerNumber(1);
                _player2.SetPlayerNumber(2);
                Activate();
                if (_rules is IGameRules2) ((IGameRules2)_rules).StartedGameCall();
                _rules.ClearField();
                _current = _player1;
                Running = true;
                Finished = false;
                Winner = null;
                MovesMade = 0;
                _rewardPaid = false;
                Message = (_entry.HasComputerOpponent ? "Du gegen die KI von " + _entry.Group + "." : "Zwei Spieler abwechselnd.")
                          + " [Esc] zur Spielauswahl.";
                Render(false);
                RunComputerMoves();
                CheckEnd();
                Render(false);
                return true;
            }
            catch (Exception e) { Fail(e); return false; }
        }

        // Entwickler: Patrick Lupierz – Datum: 18.11.2026
        // Aufgabe: Fremde Teile beim Framework als aktiv anmelden (wie StartGame_Click), alten Stand merken.
        private void Activate()
        {
            OOPGamesManager m = OOPGamesManager.Singleton;
            if (!_activated)
            {
                _previousRules = m.ActiveRules;
                _previousPainter = m.ActivePainter;
                _previousPlayers = (m.ActivePlayers ?? Enumerable.Empty<IGamePlayer>()).ToList();
                _activated = true;
            }
            m.RegisterActivePlayers(new List<IGamePlayer> { _player1, _player2 });
            m.RegisterActivePainter(_painter);
            m.RegisterActiveRules(_rules);
        }

        // Entwickler: Patrick Lupierz – Datum: 18.11.2026 – vorherigen Framework-Stand (Casino) wiederherstellen
        private void Restore()
        {
            if (!_activated) return;
            OOPGamesManager m = OOPGamesManager.Singleton;
            m.RegisterActivePlayers(_previousPlayers ?? new List<IGamePlayer>());
            m.RegisterActivePainter(_previousPainter);
            m.RegisterActiveRules(_previousRules);
            _activated = false;
        }

        // Entwickler: Patrick Lupierz – Eingabe an den aktuellen menschlichen Spieler weitergeben
        public void Forward(IMoveSelection selection)
        {
            if (!Running || _busy) return;
            _busy = true;
            try
            {
                IHumanGamePlayer human = _current as IHumanGamePlayer;
                if (human != null && _rules.MovesPossible && _rules.CheckIfPLayerWon() <= 0)
                {
                    IPlayMove move = human.GetMove(selection, _rules.CurrentField);
                    if (move != null)
                    {
                        _rules.DoMove(move);
                        MovesMade++;
                        Render(false);
                        Swap();
                        RunComputerMoves();
                        CheckEnd();
                        Render(false);
                    }
                }
            }
            catch (Exception e) { Fail(e); }
            finally { _busy = false; }
        }

        private void Swap() { _current = _current == _player1 ? _player2 : _player1; }

        private void RunComputerMoves()
        {
            for (int guard = 0; guard < 200 && Running && _rules.MovesPossible && _rules.CheckIfPLayerWon() <= 0; guard++)
            {
                IComputerGamePlayer computer = _current as IComputerGamePlayer;
                if (computer == null) return;
                IPlayMove move = computer.GetMove(_rules.CurrentField);
                _computerWaiting = move == null;
                if (move == null) return;
                _rules.DoMove(move);
                MovesMade++;
                Render(false);
                Swap();
            }
        }

        // Entwickler: Patrick Lupierz – Spielende erkennen; Belohnung genau einmal bei Sieg von Spieler 1
        // Änderung durch: Patrick Lupierz – 18.11.2026 – Ergebnis nur aus den Regeln der Gruppe, kein erzwungenes Ende
        private void CheckEnd()
        {
            int winner = _rules.CheckIfPLayerWon();
            if (winner <= 0 && _rules.MovesPossible) return;
            Running = false;
            Finished = true;
            Winner = winner > 0 ? winner : (int?)null;
            bool won = winner == _player1.PlayerNumber && _entry.HasComputerOpponent;
            if (won && !_rewardPaid) { _rewardPaid = true; Money.Deposit(WinReward); }
            Message = winner <= 0 ? "Unentschieden. [E] Nochmal, [Esc] zurück zur Spielauswahl."
                : won ? "Gewonnen! +" + WinReward + " Chips. [E] Nochmal, [Esc] zurück zur Spielauswahl."
                : "Spieler " + winner + " hat gewonnen. [E] Nochmal, [Esc] zurück zur Spielauswahl.";
        }

        public override void Update()
        {
            if (!Running || _busy) return;
            _ticks++;
            try
            {
                if (_rules is IGameRules2) ((IGameRules2)_rules).TickGameCall();
                // Änderung durch: Patrick Lupierz – 18.11.2026 – KI erneut fragen: manche Spiele (z. B. mit fallenden
                // Steinen) liefern während einer Animation noch keinen Zug; sonst bliebe die Partie stehen.
                if (_current is IComputerGamePlayer && (!_computerWaiting || _ticks % 3 == 0))
                {
                    _busy = true;
                    try { RunComputerMoves(); } finally { _busy = false; }
                    CheckEnd();
                }
                if (_ticks % 5 == 0) CheckEnd();
                if (_ticks % 2 == 0) Render(true);
            }
            catch (Exception e) { Fail(e); }
        }

        // Entwickler: Patrick Lupierz – fremden Painter auf das eigene Canvas zeichnen und als Bild übernehmen
        private void Render(bool tick)
        {
            if (_painter == null || _rules == null || _rules.CurrentField == null) return;
            if (tick && _painter is IPaintGame2) ((IPaintGame2)_painter).TickPaintGameField(_canvas, _rules.CurrentField);
            else _painter.PaintGameField(_canvas, _rules.CurrentField);
            _canvas.Measure(new Size(CanvasW, CanvasH));
            _canvas.Arrange(new Rect(0, 0, CanvasW, CanvasH));
            _canvas.UpdateLayout();
            RenderTargetBitmap bitmap = new RenderTargetBitmap(CanvasW, CanvasH, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(_canvas);
            bitmap.Freeze();
            _frame = bitmap;
        }

        private void Fail(Exception e)
        {
            Running = false;
            Restore();
            LastError = e.GetType().Name + ": " + e.Message;
            Message = "Das Gastspiel ist abgestürzt (" + e.GetType().Name + "). Einsatz verloren, [Esc] zurück zur Spielauswahl.";
        }

        protected override void DrawGame(G1C_Renderer r)
        {
            double vx = ViewX, vy = ViewY;
            r.DrawRect(vx - 10, vy - 4, ViewW + 20, ViewH + 8, Color.FromRgb(0x18, 0x18, 0x20), Color.FromRgb(0xF8, 0x60, 0xD0), 2);
            if (_frame != null) r.DrawBitmap(_frame, vx, vy, ViewW, ViewH);
            else
            {
                r.FillRect(vx, vy, ViewW, ViewH, Color.FromRgb(0x04, 0x04, 0x0C));
                r.Text(_entry.Name, vx, vy + ViewH / 2 - 20, 14, Color.FromRgb(0x50, 0xE0, 0xF8), ViewW);
                r.Text("[E] Start – " + EntryFee + " Chips", vx, vy + ViewH / 2 + 6, 11, Color.FromRgb(0xF8, 0xD0, 0x40), ViewW);
                // Änderung durch: Patrick Lupierz – 18.11.2026 – welche Teile benutzt werden (Gruppe des Spiels)
                r.Text("Regeln, Painter und KI: " + _entry.Group + (_entry.AllFromOwnGroup ? "" : " (teilweise fremde Teile)"),
                       vx, vy + ViewH / 2 + 28, 9, Color.FromRgb(0xB0, 0xA8, 0xC8), ViewW);
            }
            if (!Running) AddButtonRow(Btn("Start [E]", Key.E), Btn("Zurück zur Auswahl [Esc]", Key.Escape));
        }
    }
}
