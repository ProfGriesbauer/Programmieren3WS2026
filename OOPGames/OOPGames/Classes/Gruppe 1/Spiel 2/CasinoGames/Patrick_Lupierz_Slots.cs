using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    public enum G1C_SlotSymbol { Cherry, Lemon, Bell, Bar, Diamond, Seven }

    /// <summary>
    /// Spielautomat mit drei Walzen. Jede Walze ist ein gemischter Streifen aus 20 Symbolen;
    /// seltene Symbole kommen seltener vor und zahlen dafür mehr.
    /// </summary>
    public class G1C_Slots : G1C_CasinoGameBase
    {
        private const int ReelCount = 3;
        private const int FirstStopTick = 24;
        private const int StopInterval = 12;

        // Häufigkeit der Symbole pro Walze (Summe 20)
        private static readonly KeyValuePair<G1C_SlotSymbol, int>[] SymbolWeights =
        {
            new KeyValuePair<G1C_SlotSymbol, int>(G1C_SlotSymbol.Cherry, 5),
            new KeyValuePair<G1C_SlotSymbol, int>(G1C_SlotSymbol.Lemon, 5),
            new KeyValuePair<G1C_SlotSymbol, int>(G1C_SlotSymbol.Bell, 4),
            new KeyValuePair<G1C_SlotSymbol, int>(G1C_SlotSymbol.Bar, 3),
            new KeyValuePair<G1C_SlotSymbol, int>(G1C_SlotSymbol.Diamond, 2),
            new KeyValuePair<G1C_SlotSymbol, int>(G1C_SlotSymbol.Seven, 1),
        };

        private readonly G1C_SlotSymbol[][] _reels = new G1C_SlotSymbol[ReelCount][];
        private readonly int[] _positions = new int[ReelCount];
        private readonly int[] _targets = new int[ReelCount];

        private bool _spinning;
        private int _tick;
        private int _spinBet;
        private int _lastWin = -1;

        public G1C_Slots(G1C_MoneyManager money, Random random) : base(money, random)
        {
            for (int i = 0; i < ReelCount; i++)
            {
                _reels[i] = BuildReel();
                _positions[i] = Random.Next(_reels[i].Length);
            }
            Message = "Einsatz mit [W]/[S], [E] Drehen, [Q] Verlassen.";
        }

        public override string Name { get { return "Spielautomat"; } }

        private G1C_SlotSymbol[] BuildReel()
        {
            List<G1C_SlotSymbol> strip = new List<G1C_SlotSymbol>();
            foreach (KeyValuePair<G1C_SlotSymbol, int> entry in SymbolWeights)
            {
                for (int i = 0; i < entry.Value; i++) strip.Add(entry.Key);
            }
            return strip.OrderBy(s => Random.Next()).ToArray();
        }

        /// <summary>Auszahlungsfaktor (Vielfaches des Einsatzes) für die Symbole auf der Gewinnlinie.</summary>
        public static int GetMultiplier(IList<G1C_SlotSymbol> line)
        {
            if (line == null || line.Count != ReelCount) return 0;

            if (line.All(s => s == line[0]))
            {
                switch (line[0])
                {
                    case G1C_SlotSymbol.Seven: return 100;
                    case G1C_SlotSymbol.Diamond: return 40;
                    case G1C_SlotSymbol.Bar: return 20;
                    case G1C_SlotSymbol.Bell: return 12;
                    case G1C_SlotSymbol.Cherry: return 10;
                    default: return 8; // Zitrone
                }
            }
            if (line.Count(s => s == G1C_SlotSymbol.Seven) == 2) return 5;
            if (line.Count(s => s == G1C_SlotSymbol.Cherry) == 2) return 2;
            return 0;
        }

        private static string SpriteOf(G1C_SlotSymbol symbol)
        {
            switch (symbol)
            {
                case G1C_SlotSymbol.Cherry: return "slot_cherry";
                case G1C_SlotSymbol.Lemon: return "slot_lemon";
                case G1C_SlotSymbol.Bell: return "slot_bell";
                case G1C_SlotSymbol.Bar: return "slot_bar";
                case G1C_SlotSymbol.Diamond: return "slot_diamond";
                default: return "slot_seven";
            }
        }

        private G1C_SlotSymbol SymbolAt(int reel, int offset)
        {
            G1C_SlotSymbol[] strip = _reels[reel];
            int index = ((_positions[reel] + offset) % strip.Length + strip.Length) % strip.Length;
            return strip[index];
        }

        // ------------------------------------------------------------------ Eingabe und Ablauf

        protected override void OnKey(Key key)
        {
            if (_spinning) return;
            if (Bet.HandleKey(key)) return;
            if (G1C_InputManager.IsConfirm(key)) Spin();
            else if (G1C_InputManager.IsBack(key)) RequestExit();
        }

        private void Spin()
        {
            if (!Money.TryWithdraw(Bet.Amount))
            {
                Message = "Nicht genug Chips für " + Bet.Amount + "! Senke den Einsatz mit [S].";
                return;
            }

            _spinBet = Bet.Amount;
            for (int i = 0; i < ReelCount; i++)
            {
                _targets[i] = Random.Next(_reels[i].Length); // Ergebnis steht beim Start fest
            }
            _spinning = true;
            _tick = 0;
            _lastWin = -1;
            Message = "Die Walzen drehen sich ...";
        }

        public override void Update()
        {
            if (!_spinning) return;

            _tick++;
            for (int i = 0; i < ReelCount; i++)
            {
                // Die Walzen stoppen nacheinander von links nach rechts
                if (_tick < FirstStopTick + i * StopInterval)
                    _positions[i] = (_positions[i] + 1) % _reels[i].Length;
                else
                    _positions[i] = _targets[i];
            }

            if (_tick >= FirstStopTick + (ReelCount - 1) * StopInterval)
            {
                _spinning = false;
                EvaluateLine();
            }
        }

        private void EvaluateLine()
        {
            G1C_SlotSymbol[] line = new G1C_SlotSymbol[ReelCount];
            for (int i = 0; i < ReelCount; i++) line[i] = SymbolAt(i, 0);

            int multiplier = GetMultiplier(line);
            _lastWin = _spinBet * multiplier;
            if (_lastWin > 0)
            {
                Money.Deposit(_lastWin);
                Message = "Gewinn! x" + multiplier + " = " + _lastWin + " Chips (Netto +" + (_lastWin - _spinBet) + "). [E] Nochmal";
            }
            else
            {
                Message = "Leider nichts. -" + _spinBet + " Chips. [E] Nochmal, [Q] Verlassen";
            }
        }

        // ------------------------------------------------------------------ Darstellung

        protected override Color BackgroundColor { get { return Color.FromRgb(0x30, 0x18, 0x38); } }

        protected override void DrawGame(G1C_Renderer r)
        {
            // Gehäuse
            r.DrawRect(80, 34, 320, 176, Color.FromRgb(0xC8, 0x30, 0x30), G1C_UiColors.Gold, 3);
            r.Text("★ LUCKY SLOTS ★", 80, 38, 14, G1C_UiColors.Gold, 320);

            for (int i = 0; i < ReelCount; i++)
            {
                double x = 100 + i * 96;
                r.DrawRect(x, 60, 88, 140, Colors.White, Color.FromRgb(0x40, 0x40, 0x40), 2);
                for (int row = -1; row <= 1; row++)
                {
                    double y = 106 + row * 44;
                    r.Sprite(SpriteOf(SymbolAt(i, row)), x + 24, y + 2, 40, 40);
                }
            }
            // Gewinnlinie
            r.FillRect(92, 129, 296, 2, Color.FromArgb(0xC0, 0xE0, 0x20, 0x20));
            if (_lastWin > 0) r.DrawRect(98, 104, 284, 48, Colors.Transparent, G1C_UiColors.Highlight, 3);

            // Einsatz und Gewinntabelle
            r.Text("Einsatz", 0, 100, 11, G1C_UiColors.Gold, 80);
            r.Text(Bet.Amount.ToString(), 0, 116, 16, Colors.White, 80);
            r.Text("Gewinne:", 6, 216, 10, G1C_UiColors.Gold);
            r.Text("7 7 7 = x100   ◆◆◆ = x40   BAR x3 = x20   Glocke x3 = x12", 6, 230, 10, Colors.White);
            r.Text("Kirsche x3 = x10   Zitrone x3 = x8   zwei 7 = x5   zwei Kirschen = x2", 6, 244, 10, Colors.White);

            AddButtonRow(Btn("[W] Einsatz +", Key.W, !_spinning), Btn("[S] Einsatz -", Key.S, !_spinning),
                         Btn("[E] Drehen", Key.E, !_spinning), Btn("[Q] Verlassen", Key.Q, !_spinning));
        }
    }
}
