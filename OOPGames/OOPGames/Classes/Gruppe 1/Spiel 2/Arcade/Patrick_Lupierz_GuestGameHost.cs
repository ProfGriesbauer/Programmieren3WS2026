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
    // =====================================================
    public sealed class G1C_GuestGameEntry
    {
        public string Name { get; set; }
        public Func<IGameRules> CreateRules { get; set; }
        public Func<IPaintGame> CreatePainter { get; set; }
        public Func<IGamePlayer> CreateHuman { get; set; }
        public Func<IGamePlayer> CreateOpponent { get; set; }
        public bool HasComputerOpponent { get; set; }
    }

    public static class G1C_GuestGameCatalog
    {
        // Entwickler: Patrick Lupierz – Spiele aus dem laufenden Framework (Gast-Automat)
        public static List<G1C_GuestGameEntry> DiscoverRegistered()
        {
            OOPGamesManager m = OOPGamesManager.Singleton;
            return Discover(m.Rules, m.Painters, m.Players);
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: passende Painter/Spieler je Rules bestimmen. Fehler fremder Klassen überspringen das Spiel.
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
                    IPaintGame painter = painterList.FirstOrDefault(p => probe.CurrentField != null && probe.CurrentField.CanBePaintedBy(p));
                    IGamePlayer human = playerList.FirstOrDefault(p => p is IHumanGamePlayer && p.CanBeRuledBy(probe));
                    if (painter == null || human == null) continue;
                    IGamePlayer computer = playerList.FirstOrDefault(p => p is IComputerGamePlayer && p.CanBeRuledBy(probe));
                    IGameRules template = registered;
                    result.Add(new G1C_GuestGameEntry
                    {
                        Name = registered.Name,
                        CreateRules = () => Fresh(template),
                        CreatePainter = () => Fresh(painter),
                        CreateHuman = () => human.Clone(),
                        CreateOpponent = () => (computer ?? human).Clone(),
                        HasComputerOpponent = computer != null
                    });
                }
                catch (Exception)
                {
                    // fremdes Spiel nicht startbar – im Gast-Automaten einfach nicht anbieten
                }
            }
            return result;
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
    // =====================================================
    public sealed class G1C_GuestGameHost : G1C_CasinoGameBase
    {
        public const int EntryFee = 5;
        public const int WinReward = 20;
        private const int CanvasW = 480, CanvasH = 320;
        private const double ViewW = 330, ViewH = 220;

        private readonly G1C_GuestGameEntry _entry;
        private readonly Canvas _canvas = new Canvas { Width = CanvasW, Height = CanvasH, Background = Brushes.White };
        private IGameRules _rules;
        private IPaintGame _painter;
        private IGamePlayer _player1, _player2, _current;
        private BitmapSource _frame;
        private bool _rewardPaid;
        private int _ticks;

        public G1C_GuestGameHost(G1C_MoneyManager money, Random random, G1C_GuestGameEntry entry) : base(money, random)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
            Message = "[E] Starten (" + EntryFee + " Chips). Sieg: " + WinReward + " Chips. [Esc] verlässt den Automaten.";
        }

        public override string Name { get { return _entry.Name.Length > 28 ? _entry.Name.Substring(0, 27) + "…" : _entry.Name; } }
        public bool Running { get; private set; }
        public int? Winner { get; private set; }
        public string LastError { get; private set; }
        public IGameRules Rules { get { return _rules; } }
        public BitmapSource Frame { get { return _frame; } }

        protected override Color BackgroundColor { get { return Color.FromRgb(0x12, 0x0E, 0x1C); } }

        protected override void OnKey(Key key)
        {
            if (key == Key.Escape || (!Running && key == Key.Q)) { RequestExit(); return; }
            if (!Running)
            {
                if (G1C_InputManager.IsConfirm(key)) Start();
                return;
            }
            Forward(new KeySelection(key));
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
                if (_rules is IGameRules2) ((IGameRules2)_rules).StartedGameCall();
                _rules.ClearField();
                _current = _player1;
                Running = true;
                Winner = null;
                _rewardPaid = false;
                Message = _entry.HasComputerOpponent
                    ? "Du gegen den Computer – Tasten/Klicks wie im Original. [Esc] Ende."
                    : "Zwei Spieler abwechselnd am Automaten. [Esc] Ende.";
                RunComputerMoves();
                CheckEnd();
                Render(false);
                return true;
            }
            catch (Exception e) { Fail(e); return false; }
        }

        // Entwickler: Patrick Lupierz – Eingabe an den aktuellen menschlichen Spieler weitergeben
        public void Forward(IMoveSelection selection)
        {
            if (!Running) return;
            try
            {
                IHumanGamePlayer human = _current as IHumanGamePlayer;
                if (human != null && _rules.MovesPossible)
                {
                    IPlayMove move = human.GetMove(selection, _rules.CurrentField);
                    if (move != null)
                    {
                        _rules.DoMove(move);
                        Swap();
                        RunComputerMoves();
                    }
                }
                CheckEnd();
                Render(false);
            }
            catch (Exception e) { Fail(e); }
        }

        private void Swap() { _current = _current == _player1 ? _player2 : _player1; }

        private void RunComputerMoves()
        {
            for (int guard = 0; guard < 200 && Running && _rules.MovesPossible && _rules.CheckIfPLayerWon() <= 0; guard++)
            {
                IComputerGamePlayer computer = _current as IComputerGamePlayer;
                if (computer == null) return;
                IPlayMove move = computer.GetMove(_rules.CurrentField);
                if (move == null) return;
                _rules.DoMove(move);
                Swap();
            }
        }

        // Entwickler: Patrick Lupierz – Spielende erkennen; Belohnung genau einmal bei Sieg von Spieler 1
        private void CheckEnd()
        {
            int winner = _rules.CheckIfPLayerWon();
            if (winner <= 0 && _rules.MovesPossible) return;
            Running = false;
            Winner = winner > 0 ? winner : (int?)null;
            bool won = winner == _player1.PlayerNumber && _entry.HasComputerOpponent;
            if (won && !_rewardPaid) { _rewardPaid = true; Money.Deposit(WinReward); }
            Message = winner <= 0 ? "Unentschieden. [E] Nochmal, [Esc] verlassen."
                : won ? "Gewonnen! +" + WinReward + " Chips. [E] Nochmal, [Esc] verlassen."
                : "Spieler " + winner + " hat gewonnen. [E] Nochmal, [Esc] verlassen.";
        }

        public override void Update()
        {
            if (!Running) return;
            _ticks++;
            try
            {
                if (_rules is IGameRules2) ((IGameRules2)_rules).TickGameCall();
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
            LastError = e.GetType().Name + ": " + e.Message;
            Message = "Das Gastspiel ist abgestürzt (" + e.GetType().Name + "). Einsatz verloren, [Esc] verlassen.";
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
            }
            if (!Running) AddButtonRow(Btn("Start [E]", Key.E), Btn("Verlassen [Esc]", Key.Escape));
        }
    }
}
