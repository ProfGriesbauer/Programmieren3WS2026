using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_CatchInfo
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: Ein möglicher Fang im Angelspiel: Gegenstand (oder Chips), Häufigkeit und wie stark der
    //               Fisch zieht (Tempo, Sprunghaftigkeit). Fische und Fundstücke kann man im Laden verkaufen.
    // =====================================================
    public sealed class G1C_CatchInfo
    {
        public string ItemId { get; set; }        // null = Chips
        public string Name { get; set; }
        public int Weight { get; set; }
        public double Speed { get; set; }         // 0.3 (träge) .. 1.6 (sehr schnell)
        public double Erratic { get; set; }       // 0 .. 1: wie oft der Fisch plötzlich die Richtung wechselt
        public int MinCoins { get; set; }
        public int MaxCoins { get; set; }
        public bool IsFish { get { return ItemId != null && ItemId.StartsWith("fish_", StringComparison.Ordinal); } }
    }

    // =====================================================
    // Klasse: G1C_FishingGame
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: Angel-Minispiel auf der Brücke (SprintPlanning "fishing mini game").
    //               Ablauf: [E] Auswerfen -> warten, bis die Pose zuckt -> [E] Anhauen (kurzes Zeitfenster) ->
    //               Drill: Der Fisch schwimmt im Fluss hin und her (Pfeile, Haken, Sprünge mit Platschen). Unten ist
    //               eine graue Leiste mit grünem Abschnitt; der grüne Abschnitt treibt immer entgegengesetzt zur
    //               Bewegung des Fisches weg. Mit [A]/[D] (bzw. Pfeiltasten, gehalten) folgt man dem Fisch.
    //               Liegt der Fisch im grünen Bereich, füllt sich die Fangleiste, sonst sinkt sie.
    //               Mit Glück hängen statt Fischen Chips, Glücksmünzen, ein Ring – oder ein alter Stiefel am Haken.
    //               Gespielt wird nur über ActiveGame im G1C_GameManager (kein eigener Loop, keine eigene Eingabe).
    // =====================================================
    public sealed class G1C_FishingGame : G1C_CasinoGameBase
    {
        public enum Phase { Ready, Waiting, Bite, Fight, Caught, Escaped }

        public const double ZoneWidth = 0.26;
        public const int BiteWindowTicks = 32;

        // Änderung durch: Patrick Lupierz – 16.11.2026 – 20 Fischarten (Grafikpaket), seltene Fische ziehen stärker
        public static readonly G1C_CatchInfo[] Catches =
        {
            new G1C_CatchInfo { ItemId = "fish_minnow", Name = "Elritze", Weight = 20, Speed = 0.55, Erratic = 0.15 },
            new G1C_CatchInfo { ItemId = "fish_smelt", Name = "Stint", Weight = 18, Speed = 0.5, Erratic = 0.15 },
            new G1C_CatchInfo { ItemId = "fish_bluegill", Name = "Sonnenbarsch", Weight = 14, Speed = 0.7, Erratic = 0.3 },
            new G1C_CatchInfo { ItemId = "fish_perch", Name = "Barsch", Weight = 14, Speed = 0.75, Erratic = 0.3 },
            new G1C_CatchInfo { ItemId = "fish_bream", Name = "Brasse", Weight = 12, Speed = 0.6, Erratic = 0.2 },
            new G1C_CatchInfo { ItemId = "fish_chub", Name = "Döbel", Weight = 10, Speed = 0.8, Erratic = 0.35 },
            new G1C_CatchInfo { ItemId = "fish_trout", Name = "Regenbogenforelle", Weight = 9, Speed = 0.95, Erratic = 0.45 },
            new G1C_CatchInfo { ItemId = "fish_browntrout", Name = "Bachforelle", Weight = 8, Speed = 1.0, Erratic = 0.45 },
            new G1C_CatchInfo { ItemId = "fish_bass", Name = "Schwarzbarsch", Weight = 7, Speed = 0.95, Erratic = 0.4 },
            new G1C_CatchInfo { ItemId = "fish_carp", Name = "Karpfen", Weight = 7, Speed = 0.8, Erratic = 0.25 },
            new G1C_CatchInfo { ItemId = "fish_flounder", Name = "Flunder", Weight = 5, Speed = 0.6, Erratic = 0.3 },
            new G1C_CatchInfo { ItemId = "fish_zander", Name = "Zander", Weight = 5, Speed = 1.05, Erratic = 0.5 },
            new G1C_CatchInfo { ItemId = "fish_mackerel", Name = "Makrele", Weight = 4, Speed = 1.2, Erratic = 0.55 },
            new G1C_CatchInfo { ItemId = "fish_redfish", Name = "Rotbarsch", Weight = 3, Speed = 0.9, Erratic = 0.4 },
            new G1C_CatchInfo { ItemId = "fish_catfish", Name = "Wels", Weight = 3, Speed = 0.85, Erratic = 0.3 },
            new G1C_CatchInfo { ItemId = "fish_pike", Name = "Hecht", Weight = 3, Speed = 1.25, Erratic = 0.65 },
            new G1C_CatchInfo { ItemId = "fish_salmon", Name = "Lachs", Weight = 2, Speed = 1.3, Erratic = 0.6 },
            new G1C_CatchInfo { ItemId = "fish_sunfish", Name = "Mondfisch", Weight = 1, Speed = 0.5, Erratic = 0.2 },
            new G1C_CatchInfo { ItemId = "fish_marlin", Name = "Blauer Marlin", Weight = 1, Speed = 1.55, Erratic = 0.85 },
            new G1C_CatchInfo { ItemId = "fish_golden", Name = "Glücks-Koi", Weight = 1, Speed = 1.5, Erratic = 0.85 },
            new G1C_CatchInfo { ItemId = "boot", Name = "Alter Stiefel", Weight = 6, Speed = 0.3, Erratic = 0.05 },
            new G1C_CatchInfo { ItemId = "bottle", Name = "Pfandflasche", Weight = 5, Speed = 0.3, Erratic = 0.05 },
            new G1C_CatchInfo { ItemId = null, Name = "Chipsbeutel", Weight = 5, Speed = 0.45, Erratic = 0.2, MinCoins = 10, MaxCoins = 60 },
            new G1C_CatchInfo { ItemId = "treasure_ring", Name = "Verlorener Ring", Weight = 2, Speed = 0.5, Erratic = 0.3 },
            new G1C_CatchInfo { ItemId = "lucky_coin", Name = "Glücksmünze", Weight = 1, Speed = 0.6, Erratic = 0.4 },
        };

        private readonly G1C_Inventory _inventory;
        private readonly List<double[]> _splashes = new List<double[]>();   // x, y, Alter
        private int _tick;
        private int _phaseTick;
        private int _waitTicks;
        private double _fishTarget;
        private int _retargetIn;

        public G1C_FishingGame(G1C_MoneyManager money, Random random, G1C_Inventory inventory) : base(money, random)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Message = "[E] Angel auswerfen.  [Q] Aufhören.";
        }

        public override string Name { get { return "Angeln"; } }

        public Phase State { get; private set; } = Phase.Ready;
        public G1C_CatchInfo Hooked { get; private set; }
        public double FishPosition { get; private set; } = 0.5;   // 0..1 auf der Leiste
        public double FishVelocity { get; private set; }
        public double ZoneCenter { get; private set; } = 0.5;
        public double Progress { get; private set; }
        public List<string> Caught { get; } = new List<string>();
        public int CoinsFound { get; private set; }

        /// <summary>Gehaltene Steuerrichtung (-1 links, 0, +1 rechts). Standard: Tastaturzustand; Tests setzen eigene Werte.</summary>
        public Func<int> Steering { get; set; } = DefaultSteering;

        private static int DefaultSteering()
        {
            int dir = 0;
            if (G1C_InputManager.IsKeyHeld(Key.A) || G1C_InputManager.IsKeyHeld(Key.Left)) dir--;
            if (G1C_InputManager.IsKeyHeld(Key.D) || G1C_InputManager.IsKeyHeld(Key.Right)) dir++;
            return dir;
        }

        public bool InZone { get { return Math.Abs(FishPosition - ZoneCenter) <= ZoneWidth / 2; } }

        // ------------------------------------------------------------------ Eingabe

        protected override void OnKey(Key key)
        {
            if (G1C_InputManager.IsBack(key)) { if (State != Phase.Fight) RequestExit(); return; }
            switch (State)
            {
                case Phase.Ready:
                case Phase.Caught:
                case Phase.Escaped:
                    if (G1C_InputManager.IsConfirm(key)) Cast();
                    break;
                case Phase.Waiting:
                    if (G1C_InputManager.IsConfirm(key)) { State = Phase.Ready; Message = "Zu früh! Nichts dran. [E] Neu auswerfen."; }
                    break;
                case Phase.Bite:
                    if (G1C_InputManager.IsConfirm(key)) StrikeHook();
                    break;
                case Phase.Fight:
                    // Einzelner Tastendruck = kleiner Ruck (zusätzlich zum Halten der Taste)
                    if (key == Key.A || key == Key.Left) ZoneCenter = Clamp(ZoneCenter - 0.05, ZoneWidth / 2, 1 - ZoneWidth / 2);
                    if (key == Key.D || key == Key.Right) ZoneCenter = Clamp(ZoneCenter + 0.05, ZoneWidth / 2, 1 - ZoneWidth / 2);
                    break;
            }
        }

        // Entwickler: Patrick Lupierz
        public void Cast()
        {
            State = Phase.Waiting;
            _phaseTick = 0;
            _waitTicks = 40 + Random.Next(90);
            Hooked = null;
            AddSplash(0.5, 0.45);
            Sfx("splash");
            Message = "Die Pose treibt auf dem Wasser ... warte, bis sie zuckt.";
        }

        // Entwickler: Patrick Lupierz – Fang auswürfeln und Drill starten
        public void StrikeHook()
        {
            int total = Catches.Sum(c => c.Weight);
            int roll = Random.Next(total);
            Hooked = Catches.First(c => (roll -= c.Weight) < 0);
            State = Phase.Fight;
            _phaseTick = 0;
            FishPosition = 0.5;
            FishVelocity = 0;
            ZoneCenter = 0.5;
            Progress = 0.3;
            _retargetIn = 0;
            Sfx("splash");
            Message = "Angebissen! Halte den Fisch mit [A]/[D] im grünen Bereich!";
        }

        // ------------------------------------------------------------------ Ablauf (Tick)

        public override void Update()
        {
            _tick++;
            _phaseTick++;
            foreach (double[] s in _splashes) s[2]++;
            _splashes.RemoveAll(s => s[2] > 18);

            switch (State)
            {
                case Phase.Waiting:
                    if (_phaseTick >= _waitTicks) { State = Phase.Bite; _phaseTick = 0; Message = "Die Pose zuckt! Schnell [E] zum Anhauen!"; AddSplash(0.5, 0.45); }
                    break;
                case Phase.Bite:
                    if (_phaseTick > BiteWindowTicks) { State = Phase.Ready; Message = "Zu spät – der Köder ist weg. [E] Neu auswerfen."; }
                    break;
                case Phase.Fight:
                    UpdateFight(Steering != null ? Steering() : 0);
                    break;
            }
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Ein Drill-Tick. Der Fisch steuert ein zufälliges Ziel an (sprunghafte Fische wechseln öfter und
        //          schneller); der grüne Bereich treibt entgegengesetzt zur Fischbewegung, der Spieler steuert dagegen.
        public void UpdateFight(int steering)
        {
            if (State != Phase.Fight || Hooked == null) return;
            if (--_retargetIn <= 0)
            {
                bool dart = Random.NextDouble() < Hooked.Erratic * 0.5;
                _fishTarget = dart ? (FishPosition < 0.5 ? 0.75 + Random.NextDouble() * 0.2 : 0.05 + Random.NextDouble() * 0.2)
                                   : 0.1 + Random.NextDouble() * 0.8;
                _retargetIn = (int)((dart ? 12 : 30) + Random.Next(30) * (1 - Hooked.Erratic));
                if (dart || Random.NextDouble() < 0.25) { AddSplash(FishPosition, 0.6); Sfx("splash"); }
            }
            double accel = (_fishTarget - FishPosition) * 0.02 * Hooked.Speed;
            FishVelocity = Clamp(FishVelocity * 0.9 + accel, -0.03 * Hooked.Speed, 0.03 * Hooked.Speed);
            FishPosition = Clamp(FishPosition + FishVelocity, 0.02, 0.98);

            double zoneVelocity = -FishVelocity * 0.6 + Math.Sign(steering) * 0.022;
            ZoneCenter = Clamp(ZoneCenter + zoneVelocity, ZoneWidth / 2, 1 - ZoneWidth / 2);

            Progress += InZone ? 0.011 : -0.007 * (1 + Hooked.Speed * 0.5);
            if (Progress >= 1) Land();
            else if (Progress <= 0) { State = Phase.Escaped; Message = "Der Fang hat sich losgerissen! [E] Neu auswerfen."; Sfx("lose"); }
        }

        private void Land()
        {
            State = Phase.Caught;
            Progress = 1;
            AddSplash(FishPosition, 0.5);
            if (Hooked.ItemId == null)
            {
                int coins = Hooked.MinCoins + Random.Next(Hooked.MaxCoins - Hooked.MinCoins + 1);
                Money.Deposit(coins);
                CoinsFound += coins;
                Caught.Add(coins + " Chips");
                Message = "Ein Beutel voller Chips! +" + coins + " Chips. [E] Weiter angeln.";
            }
            else
            {
                _inventory.Add(Hooked.ItemId);
                Caught.Add(Hooked.Name);
                G1C_Item item = G1C_ItemCatalog.Get(Hooked.ItemId);
                int value = item != null ? item.SellValue : 0;
                Message = (Hooked.IsFish ? "Gefangen: " : "Am Haken: ") + Hooked.Name + " (im Laden " + value + " Chips wert). [E] Weiter.";
            }
            Sfx("catch");
        }

        private void AddSplash(double barX, double depth)
        {
            _splashes.Add(new[] { barX, depth, 0 });
        }

        private static double Clamp(double v, double min, double max)
        {
            return v < min ? min : v > max ? max : v;
        }

        // ------------------------------------------------------------------ Darstellung

        private const double WaterTop = 58;
        private const double WaterBottom = 196;
        private const double WaterLeft = 36;
        private const double WaterRight = 444;

        protected override Color BackgroundColor { get { return Color.FromRgb(0x58, 0xA8, 0x48); } }

        private double BarToScreenX(double barX) { return WaterLeft + 20 + barX * (WaterRight - WaterLeft - 40); }

        protected override void DrawGame(G1C_Renderer r)
        {
            // Ufer, Fluss mit Strömungslinien, Brücke oben
            r.FillRect(0, 28, 480, 176, Color.FromRgb(0x68, 0xB8, 0x50));
            r.FillRect(WaterLeft, WaterTop - 6, WaterRight - WaterLeft, WaterBottom - WaterTop + 12, Color.FromRgb(0xC8, 0xB8, 0x80));
            for (int band = 0; band < 6; band++)
            {
                double y = WaterTop + band * (WaterBottom - WaterTop) / 6;
                byte shade = (byte)(0x40 + band * 6);
                r.FillRect(WaterLeft + 4, y, WaterRight - WaterLeft - 8, (WaterBottom - WaterTop) / 6 + 1, Color.FromRgb(0x30, shade, (byte)(0xB8 - band * 8)));
            }
            for (int i = 0; i < 14; i++)
            {
                double x = WaterLeft + 10 + ((i * 53 + _tick * (1 + i % 3)) % (WaterRight - WaterLeft - 40));
                double y = WaterTop + 10 + (i * 37) % (WaterBottom - WaterTop - 20);
                r.FillRect(x, y, 14 + i % 3 * 4, 1.5, Color.FromArgb(120, 0xD8, 0xF0, 0xFF));
            }
            r.FillRect(WaterLeft - 4, 28, WaterRight - WaterLeft + 8, 24, Color.FromRgb(0x9A, 0x68, 0x38));          // Brücke
            for (double x = WaterLeft; x < WaterRight; x += 12) r.FillRect(x, 28, 1.5, 24, Color.FromRgb(0x6A, 0x44, 0x20));
            r.FillRect(WaterLeft - 4, 48, WaterRight - WaterLeft + 8, 4, Color.FromRgb(0x5A, 0x38, 0x18));            // Geländer

            double tipX = 240, tipY = 44;
            r.Sprite("obj_fishing_rod", 222, 6, 18, 36);
            switch (State)
            {
                case Phase.Waiting:
                case Phase.Bite:
                case Phase.Ready:
                    if (State == Phase.Ready) break;
                    double bob = State == Phase.Bite ? 5 + Math.Sin(_tick * 1.3) * 4 : Math.Sin(_tick * 0.15) * 1.5;
                    double px = BarToScreenX(0.5), py = WaterTop + 0.45 * (WaterBottom - WaterTop) + bob;
                    DrawLine(r, tipX, tipY, px, py);
                    r.FillEllipse(px, py, 5, 5, Color.FromRgb(0xF0, 0x30, 0x30));
                    r.FillEllipse(px, py - 2, 5, 2.5, Color.FromRgb(0xF8, 0xF8, 0xF8));
                    if (State == Phase.Bite) r.Text("!", px + 8, py - 30, 22, G1C_UiColors.Gold);
                    break;
                case Phase.Fight:
                case Phase.Caught:
                case Phase.Escaped:
                    DrawFish(r, tipX, tipY);
                    break;
            }
            foreach (double[] s in _splashes)
            {
                double age = s[2];
                double sx = BarToScreenX(s[0]), sy = WaterTop + s[1] * (WaterBottom - WaterTop);
                byte alpha = (byte)Math.Max(0, 220 - age * 12);
                r.FillEllipse(sx, sy, 6 + age * 1.6, 2 + age * 0.6, Color.FromArgb(alpha, 0xE8, 0xF8, 0xFF));
                for (int d = -1; d <= 1; d++) r.FillEllipse(sx + d * (4 + age), sy - 6 - age * (1.5 - Math.Abs(d) * 0.5) + age * age * 0.08, 1.8, 1.8, Color.FromArgb(alpha, 0xC8, 0xE8, 0xFF));
            }

            if (State == Phase.Fight) DrawSlideBar(r);
            r.Text("Fang heute: " + (Caught.Count == 0 ? "noch nichts" : string.Join(", ", Caught.Skip(Math.Max(0, Caught.Count - 3)))), 8, 240, 10, Colors.White);

            bool fighting = State == Phase.Fight;
            AddButtonRow(Btn("[A] Links", Key.A, fighting), Btn("[D] Rechts", Key.D, fighting),
                         Btn(State == Phase.Bite ? "[E] Anhauen!" : "[E] Auswerfen", Key.E, !fighting && State != Phase.Waiting),
                         Btn("[Q] Aufhören", Key.Q, !fighting));
        }

        private void DrawFish(G1C_Renderer r, double tipX, double tipY)
        {
            if (Hooked == null) return;
            double fx = BarToScreenX(FishPosition);
            double fy = WaterTop + (0.55 + Math.Sin(_tick * 0.09) * 0.18 + Math.Sin(_tick * 0.23) * 0.06) * (WaterBottom - WaterTop);
            if (State == Phase.Caught) fy = WaterTop - 10 - Math.Min(30, _phaseTick * 2);
            if (State != Phase.Escaped) DrawLine(r, tipX, tipY, fx, fy);
            if (State == Phase.Escaped) return;
            string sprite = Hooked.IsFish ? Hooked.ItemId + "_" + (FishVelocity < 0 ? "l" : "r")
                          : Hooked.ItemId == null ? "chip" : "item_" + Hooked.ItemId;
            double w = Hooked.IsFish ? 34 : 24, h = Hooked.IsFish ? 34 : 24;
            r.Sprite(sprite, fx - w / 2, fy - h / 2, w, h, State == Phase.Fight ? Math.Sin(_tick * 0.5) * 10 : 0);
        }

        private void DrawSlideBar(G1C_Renderer r)
        {
            double left = WaterLeft + 20, width = WaterRight - WaterLeft - 40, y = 214;
            r.DrawRect(left, y, width, 18, Color.FromRgb(0x90, 0x90, 0x98), Color.FromRgb(0x30, 0x30, 0x38), 2);   // grau
            r.FillRect(left + (ZoneCenter - ZoneWidth / 2) * width, y + 2, ZoneWidth * width, 14,
                       InZone ? Color.FromRgb(0x48, 0xE0, 0x58) : Color.FromRgb(0x38, 0xB0, 0x48));               // grün
            if (Hooked != null && Hooked.IsFish) r.Sprite(Hooked.ItemId + "_" + (FishVelocity < 0 ? "l" : "r"), left + FishPosition * width - 9, y - 1, 18, 18);
            else r.FillEllipse(left + FishPosition * width, y + 9, 5, 5, G1C_UiColors.Gold);
            r.FillRect(left, y - 8, width, 5, Color.FromRgb(0x30, 0x30, 0x38));                                    // Fangfortschritt
            r.FillRect(left, y - 8, width * Math.Max(0, Progress), 5, Color.FromRgb(0xF8, 0xC8, 0x30));
        }

        // Angelschnur als Folge kleiner Punkte (der Renderer kennt keine Linien)
        private static void DrawLine(G1C_Renderer r, double x1, double y1, double x2, double y2)
        {
            int steps = (int)Math.Max(4, Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1)) / 3);
            for (int i = 0; i <= steps; i++)
            {
                double t = i / (double)steps;
                double sag = Math.Sin(t * Math.PI) * 6;
                r.FillRect(x1 + (x2 - x1) * t, y1 + (y2 - y1) * t + sag, 1.2, 1.2, Color.FromRgb(0xF0, 0xF0, 0xF0));
            }
        }
    }
}
