using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    public enum G1C_RouletteBetType { Number, Red, Black, Even, Odd, Low, High, Dozen1, Dozen2, Dozen3 }

    /// <summary>Feste Daten des europäischen Roulettes (Radreihenfolge und rote Zahlen).</summary>
    public static class G1C_RouletteTable
    {
        public static readonly int[] WheelOrder =
        {
            0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10,
            5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
        };

        private static readonly HashSet<int> RedNumbers = new HashSet<int>
        {
            1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36
        };

        public static bool IsRed(int number) { return RedNumbers.Contains(number); }

        public static int WheelIndexOf(int number) { return Array.IndexOf(WheelOrder, number); }

        public static string ColorName(int number)
        {
            if (number == 0) return "Grün";
            return IsRed(number) ? "Rot" : "Schwarz";
        }
    }

    /// <summary>Ein Roulette-Einsatz mit Gewinnprüfung und Auszahlung.</summary>
    public class G1C_RouletteBet
    {
        public G1C_RouletteBet(G1C_RouletteBetType type, int number, int amount)
        {
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            if (type == G1C_RouletteBetType.Number && (number < 0 || number > 36)) throw new ArgumentOutOfRangeException(nameof(number));
            Type = type;
            Number = number;
            Amount = amount;
        }

        public G1C_RouletteBetType Type { get; }
        public int Number { get; }
        public int Amount { get; }

        /// <summary>Gewinnquote "x zu 1".</summary>
        public int PayoutFactor
        {
            get
            {
                if (Type == G1C_RouletteBetType.Number) return 35;
                if (Type == G1C_RouletteBetType.Dozen1 || Type == G1C_RouletteBetType.Dozen2 || Type == G1C_RouletteBetType.Dozen3) return 2;
                return 1;
            }
        }

        /// <summary>Gewinnwahrscheinlichkeit beim europäischen Rad (37 Felder).</summary>
        public double WinProbability
        {
            get
            {
                if (Type == G1C_RouletteBetType.Number) return 1 / 37.0;
                return PayoutFactor == 2 ? 12 / 37.0 : 18 / 37.0;
            }
        }

        public string Label
        {
            get
            {
                switch (Type)
                {
                    case G1C_RouletteBetType.Number: return "Zahl " + Number;
                    case G1C_RouletteBetType.Red: return "Rot";
                    case G1C_RouletteBetType.Black: return "Schwarz";
                    case G1C_RouletteBetType.Even: return "Gerade";
                    case G1C_RouletteBetType.Odd: return "Ungerade";
                    case G1C_RouletteBetType.Low: return "1-18";
                    case G1C_RouletteBetType.High: return "19-36";
                    case G1C_RouletteBetType.Dozen1: return "1. Dutzend";
                    case G1C_RouletteBetType.Dozen2: return "2. Dutzend";
                    default: return "3. Dutzend";
                }
            }
        }

        public bool Wins(int result)
        {
            if (Type == G1C_RouletteBetType.Number) return result == Number;
            if (result == 0) return false; // Die Null lässt alle Außenwetten verlieren

            switch (Type)
            {
                case G1C_RouletteBetType.Red: return G1C_RouletteTable.IsRed(result);
                case G1C_RouletteBetType.Black: return !G1C_RouletteTable.IsRed(result);
                case G1C_RouletteBetType.Even: return result % 2 == 0;
                case G1C_RouletteBetType.Odd: return result % 2 == 1;
                case G1C_RouletteBetType.Low: return result <= 18;
                case G1C_RouletteBetType.High: return result >= 19;
                case G1C_RouletteBetType.Dozen1: return result <= 12;
                case G1C_RouletteBetType.Dozen2: return result >= 13 && result <= 24;
                default: return result >= 25;
            }
        }

        /// <summary>Gesamte Auszahlung inkl. Einsatz (0 bei Verlust).</summary>
        public int TotalReturn(int result)
        {
            return Wins(result) ? Amount * (PayoutFactor + 1) : 0;
        }
    }

    /// <summary>
    /// Roulette gegen einen KI-Mitspieler. Ablauf: Feld und Einsatz wählen → Drehen →
    /// Animation von Rad und Kugel → Auszahlung.
    /// </summary>
    public class G1C_Roulette : G1C_CasinoGameBase
    {
        private enum Phase { Betting, Spinning, Result }

        private const int SpinTicks = 100;               // 100 x 40 ms = 4 Sekunden
        private const int TargetCount = 46;              // 37 Zahlen + 9 Außenwetten
        private const double WheelCenterX = 85;
        private const double WheelCenterY = 100;
        private const double WheelRadius = 60;

        // Zielindex 37..45 = Außenwetten
        private static readonly G1C_RouletteBetType[] OutsideTargets =
        {
            G1C_RouletteBetType.Red, G1C_RouletteBetType.Black, G1C_RouletteBetType.Even, G1C_RouletteBetType.Odd,
            G1C_RouletteBetType.Low, G1C_RouletteBetType.High,
            G1C_RouletteBetType.Dozen1, G1C_RouletteBetType.Dozen2, G1C_RouletteBetType.Dozen3
        };

        private readonly G1C_RouletteAI _ai;
        private readonly List<int> _history = new List<int>();
        private readonly List<KeyValuePair<Rect, int>> _cells = new List<KeyValuePair<Rect, int>>();

        private Phase _phase = Phase.Betting;
        private int _target = 37; // Start: Rot
        private G1C_RouletteBet _playerBet;
        private G1C_RouletteBet _aiBet;
        private int _result = -1;
        private int _spinTick;
        private double _wheelStartAngle;
        private double _wheelAngle;
        private double _ballAngle;

        public G1C_Roulette(G1C_MoneyManager money, Random random) : base(money, random)
        {
            _ai = new G1C_RouletteAI("Viktor", 1000, Random);
            Message = "Feld wählen: [A]/[D] oder Klick. Einsatz: [W]/[S]. [E] Drehen, [Q] Verlassen.";
        }

        public override string Name { get { return "Roulette"; } }

        public static G1C_RouletteBet CreateBet(int target, int amount)
        {
            if (target >= 0 && target <= 36) return new G1C_RouletteBet(G1C_RouletteBetType.Number, target, amount);
            return new G1C_RouletteBet(OutsideTargets[target - 37], 0, amount);
        }

        private static int TargetOf(G1C_RouletteBet bet)
        {
            if (bet.Type == G1C_RouletteBetType.Number) return bet.Number;
            return 37 + Array.IndexOf(OutsideTargets, bet.Type);
        }

        // ------------------------------------------------------------------ Eingabe

        protected override void OnKey(Key key)
        {
            if (_phase == Phase.Spinning) return; // Rien ne va plus

            if (_phase == Phase.Result)
            {
                if (G1C_InputManager.IsConfirm(key)) StartNewRound();
                else if (G1C_InputManager.IsBack(key)) RequestExit();
                return;
            }

            if (Bet.HandleKey(key)) return;
            if (G1C_InputManager.IsPrevious(key)) _target = (_target + TargetCount - 1) % TargetCount;
            else if (G1C_InputManager.IsNext(key)) _target = (_target + 1) % TargetCount;
            else if (G1C_InputManager.IsConfirm(key)) PlaceBetsAndSpin();
            else if (G1C_InputManager.IsBack(key)) RequestExit();
        }

        protected override void OnClick(double x, double y)
        {
            if (_phase == Phase.Spinning) return;
            foreach (KeyValuePair<Rect, int> cell in _cells)
            {
                if (cell.Key.Contains(x, y))
                {
                    _target = cell.Value;
                    if (_phase == Phase.Result) StartNewRound();
                    return;
                }
            }
        }

        private void StartNewRound()
        {
            _phase = Phase.Betting;
            _playerBet = null;
            _aiBet = null;
            Message = "Neue Runde! Feld und Einsatz wählen, dann [E] Drehen.";
        }

        private void PlaceBetsAndSpin()
        {
            if (!Money.TryWithdraw(Bet.Amount))
            {
                Message = "Nicht genug Chips für " + Bet.Amount + "! Senke den Einsatz mit [S].";
                return;
            }

            _playerBet = CreateBet(_target, Bet.Amount);
            _aiBet = _ai.DecideBet(_history);
            _result = Random.Next(37);
            _phase = Phase.Spinning;
            _spinTick = 0;
            _wheelStartAngle = _wheelAngle % 360;
            Message = "Rien ne va plus! Die Kugel rollt ...";
        }

        // ------------------------------------------------------------------ Animation

        public override void Update()
        {
            if (_phase != Phase.Spinning) return;

            _spinTick++;
            double t = Math.Min(1.0, _spinTick / (double)SpinTicks);
            double slowDown = (1 - t) * (1 - t);

            // Rad dreht im Uhrzeigersinn und wird langsamer
            _wheelAngle = _wheelStartAngle + 540 * (1 - slowDown);

            // Kugel läuft gegen den Uhrzeigersinn und landet am Ende genau in der Tasche der Ergebniszahl
            double pocketAngle = (G1C_RouletteTable.WheelIndexOf(_result) + 0.5) * 360.0 / 37.0;
            _ballAngle = _wheelAngle + pocketAngle + 1440 * slowDown;

            if (_spinTick >= SpinTicks) ResolveRound();
        }

        private void ResolveRound()
        {
            int playerReturn = _playerBet.TotalReturn(_result);
            if (playerReturn > 0) Money.Deposit(playerReturn);
            _ai.ApplyResult(_aiBet, _result);

            _history.Insert(0, _result);
            if (_history.Count > 12) _history.RemoveAt(_history.Count - 1);

            string resultText = "Die Kugel fällt auf " + _result + " (" + G1C_RouletteTable.ColorName(_result) + "). ";
            Message = playerReturn > 0
                ? resultText + "Gewonnen! +" + (playerReturn - _playerBet.Amount) + " Chips. [E] Weiter"
                : resultText + "Verloren: -" + _playerBet.Amount + " Chips. [E] Weiter";
            _phase = Phase.Result;
        }

        // ------------------------------------------------------------------ Darstellung

        protected override void DrawGame(G1C_Renderer r)
        {
            DrawWheel(r);
            DrawTable(r);
            DrawInfo(r);

            bool betting = _phase == Phase.Betting;
            if (_phase == Phase.Result)
            {
                AddButtonRow(Btn("[E] Weiter", Key.E), Btn("[Q] Verlassen", Key.Q));
            }
            else
            {
                AddButtonRow(
                    Btn("[A] ◄", Key.A, betting), Btn("[D] ►", Key.D, betting),
                    Btn("[W] +", Key.W, betting), Btn("[S] -", Key.S, betting),
                    Btn("[E] Drehen", Key.E, betting), Btn("[Q] Ende", Key.Q, betting));
            }
        }

        private void DrawWheel(G1C_Renderer r)
        {
            r.FillEllipse(WheelCenterX, WheelCenterY + 3, WheelRadius + 2, WheelRadius + 2, G1C_UiColors.FeltDark);
            r.Sprite("roulette_wheel", WheelCenterX - WheelRadius, WheelCenterY - WheelRadius,
                     WheelRadius * 2, WheelRadius * 2, _wheelAngle);

            if (_result >= 0)
            {
                // Radius: Kugel rollt außen und fällt in den letzten 30 % in die Tasche
                double t = _phase == Phase.Spinning ? _spinTick / (double)SpinTicks : 1.0;
                double outer = WheelRadius * 0.9;
                double inner = WheelRadius * 0.73;
                double radius = t < 0.7 ? outer : outer - (outer - inner) * ((t - 0.7) / 0.3);
                double rad = _ballAngle * Math.PI / 180.0;
                double bx = WheelCenterX + Math.Sin(rad) * radius;
                double by = WheelCenterY - Math.Cos(rad) * radius;
                r.FillEllipse(bx, by, 4, 4, Colors.White);
            }

            // Ergebnisanzeige unter dem Rad
            if (_result >= 0 && _phase != Phase.Spinning)
            {
                Color c = _result == 0 ? G1C_UiColors.RouletteGreen
                    : (G1C_RouletteTable.IsRed(_result) ? G1C_UiColors.RouletteRed : G1C_UiColors.RouletteBlack);
                r.FillEllipse(WheelCenterX, 188, 20, 20, Colors.White);
                r.FillEllipse(WheelCenterX, 188, 17, 17, c);
                r.Text(_result.ToString(), WheelCenterX - 20, 177, 18, Colors.White, 40);
            }
            else if (_phase == Phase.Spinning)
            {
                r.Text("...", WheelCenterX - 30, 178, 18, Colors.White, 60);
            }

            string history = _history.Count == 0 ? "-" : string.Join(" ", _history.Take(8));
            r.Text("Letzte Zahlen:", 6, 214, 10, G1C_UiColors.Gold);
            r.Text(history, 6, 228, 10, Colors.White);
        }

        private void DrawTable(G1C_Renderer r)
        {
            _cells.Clear();

            DrawCell(r, 0, 165, 36, 20, 66, "0", G1C_UiColors.RouletteGreen);
            for (int n = 1; n <= 36; n++)
            {
                int column = (n - 1) / 3;
                int row = 2 - (n - 1) % 3; // oberste Reihe: 3, 6, 9, ...
                Color c = G1C_RouletteTable.IsRed(n) ? G1C_UiColors.RouletteRed : G1C_UiColors.RouletteBlack;
                DrawCell(r, n, 185 + column * 24, 36 + row * 22, 24, 22, n.ToString(), c);
            }

            for (int d = 0; d < 3; d++)
            {
                DrawCell(r, 43 + d, 185 + d * 96, 104, 96, 20, (d + 1) + ". Dutzend", G1C_UiColors.FeltDark);
            }

            int[] evenMoneyTargets = { 41, 39, 37, 38, 40, 42 };
            string[] evenMoneyLabels = { "1-18", "Gerade", "Rot", "Schwarz", "Unger.", "19-36" };
            for (int i = 0; i < evenMoneyTargets.Length; i++)
            {
                Color c = evenMoneyTargets[i] == 37 ? G1C_UiColors.RouletteRed
                    : (evenMoneyTargets[i] == 38 ? G1C_UiColors.RouletteBlack : G1C_UiColors.FeltDark);
                DrawCell(r, evenMoneyTargets[i], 185 + i * 48, 126, 48, 20, evenMoneyLabels[i], c);
            }

            // Markierungen: Auswahl (gelb), Chip des Spielers, Chip der KI
            if (_phase == Phase.Betting) HighlightTarget(r, _target);
            if (_playerBet != null) DrawMarker(r, TargetOf(_playerBet), true);
            if (_aiBet != null) DrawMarker(r, TargetOf(_aiBet), false);
        }

        private void DrawCell(G1C_Renderer r, int target, double x, double y, double w, double h, string label, Color color)
        {
            r.DrawRect(x, y, w, h, color, Colors.White, 1);
            r.Text(label, x, y + h / 2 - 7, 10, Colors.White, w);
            _cells.Add(new KeyValuePair<Rect, int>(new Rect(x, y, w, h), target));
        }

        private Rect CellOf(int target)
        {
            foreach (KeyValuePair<Rect, int> cell in _cells)
            {
                if (cell.Value == target) return cell.Key;
            }
            return Rect.Empty;
        }

        private void HighlightTarget(G1C_Renderer r, int target)
        {
            Rect cell = CellOf(target);
            if (cell.IsEmpty) return;
            r.DrawRect(cell.X - 1, cell.Y - 1, cell.Width + 2, cell.Height + 2, Colors.Transparent, G1C_UiColors.Highlight, 2.5);
        }

        private void DrawMarker(G1C_Renderer r, int target, bool isPlayer)
        {
            Rect cell = CellOf(target);
            if (cell.IsEmpty) return;
            double x = isPlayer ? cell.X + 2 : cell.Right - 12;
            if (isPlayer)
            {
                r.Sprite("chip", x, cell.Y + 2, 10, 10);
            }
            else
            {
                r.FillEllipse(x + 5, cell.Y + 7, 5, 5, Color.FromRgb(0x30, 0x60, 0xE0));
                r.Text("K", x, cell.Y + 1, 8, Colors.White, 10);
            }
        }

        private void DrawInfo(G1C_Renderer r)
        {
            r.DrawPanel(165, 152, 308, 106);
            G1C_RouletteBet preview = _playerBet ?? CreateBet(_target, Bet.Amount);
            string betLine = _playerBet == null
                ? "Auswahl: " + preview.Label + "   Einsatz: " + Bet.Amount
                : "Dein Einsatz: " + _playerBet.Amount + " auf " + _playerBet.Label;
            r.Text(betLine, 172, 158, 11, G1C_UiColors.Text);
            r.Text("Quote " + preview.PayoutFactor + ":1   Chance " + preview.WinProbability.ToString("P1"), 172, 174, 10, G1C_UiColors.Text);

            r.Text("KI " + _ai.Name + "   Kasse: " + _ai.Bankroll + "   Strategie: " + _ai.Strategy, 172, 196, 10, G1C_UiColors.PanelBorder);
            r.Text(_ai.LastDecision, 172, 212, 9, G1C_UiColors.Text);
            r.Text("Chip = du, blaues K = KI", 172, 236, 9, G1C_UiColors.Disabled);
        }
    }
}
