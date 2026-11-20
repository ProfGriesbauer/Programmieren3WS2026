using System;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    public class G1C_ClawMachine : G1C_CasinoGameBase
    {
        private enum Phase { Ready, Align, Lower, Grab, Raise, RoundOver }

        private static readonly string[] PrizeIds = { "coffee", "water", "sandwich", "bottle", "lucky_coin", "chips" };
        private const int AnimationStepTicks = 12;

        private readonly G1C_Inventory _inventory;
        private Phase _phase = Phase.Ready;
        private int _selected;
        private int _timer;
        private int _clawX = 240;
        private int _clawY = 76;
        private bool _won;
        private bool _dropped;

        public G1C_ClawMachine(G1C_MoneyManager money, Random random, G1C_Inventory inventory)
            : base(money, random)
        {
            _inventory = inventory ?? throw new ArgumentNullException(nameof(inventory));
            Message = "W/S Einsatz, A/D Item wählen, E Greifer starten, Q zurück.";
        }

        public override string Name { get { return "Greifautomat"; } }

        protected override void OnKey(Key key)
        {
            if (_phase == Phase.Ready)
            {
                if (Bet.HandleKey(key)) return;
                if (key == Key.A || key == Key.Left) _selected = (_selected + PrizeIds.Length - 1) % PrizeIds.Length;
                else if (key == Key.D || key == Key.Right) _selected = (_selected + 1) % PrizeIds.Length;
                else if (G1C_InputManager.IsConfirm(key)) StartAttempt();
                else if (G1C_InputManager.IsBack(key)) RequestExit();
            }
            else if (_phase == Phase.RoundOver)
            {
                if (G1C_InputManager.IsConfirm(key))
                {
                    _phase = Phase.Ready;
                    Message = "Nächster Versuch? W/S Einsatz, A/D Item, E Greifer.";
                }
                else if (G1C_InputManager.IsBack(key)) RequestExit();
            }
            else if (_phase == Phase.Align)
            {
                if (key == Key.A || key == Key.Left) _clawX = Math.Max(65, _clawX - 12);
                else if (key == Key.D || key == Key.Right) _clawX = Math.Min(380, _clawX + 12);
                else if (G1C_InputManager.IsConfirm(key)) DropClaw();
                else if (G1C_InputManager.IsBack(key)) RequestExit();
            }
            else if (G1C_InputManager.IsBack(key))
            {
                RequestExit();
            }
        }

        private void StartAttempt()
        {
            if (!Money.TryWithdraw(Bet.Amount))
            {
                Message = "Nicht genug Chips für den Versuch.";
                return;
            }
            _won = false;
            _clawX = 240;
            _clawY = 76;
            _timer = 0;
            _dropped = false;
            _phase = Phase.Align;
            Message = "A/D bewegt den Greifer. E senkt ihn über dem markierten Preis ab.";
        }

        private void DropClaw()
        {
            _dropped = true;
            _timer = 0;
            _phase = Phase.Lower;
            Message = "Der Greifer senkt sich ...";
        }

        public override void Update()
        {
            if (_phase == Phase.Ready || _phase == Phase.Align || _phase == Phase.RoundOver) return;
            _timer++;
            if (_phase == Phase.Lower)
            {
                _clawY = 76 + (int)(70 * Math.Min(1.0, _timer / (double)AnimationStepTicks));
                if (_timer >= AnimationStepTicks) Advance(Phase.Grab);
            }
            else if (_phase == Phase.Grab && _timer >= 8)
            {
                int targetX = PrizeCenter(_selected);
                _won = _dropped && Math.Abs(_clawX - targetX) <= 16 && Random.NextDouble() < 0.45;
                if (_won)
                {
                    if (PrizeIds[_selected] == "chips") Money.Deposit(50);
                    else _inventory.Add(PrizeIds[_selected]);
                }
                Advance(Phase.Raise);
            }
            else if (_phase == Phase.Raise)
            {
                _clawY = 146 - (int)(70 * Math.Min(1.0, _timer / (double)AnimationStepTicks));
                if (_timer >= AnimationStepTicks)
                {
                    _phase = Phase.RoundOver;
                    Message = _won
                        ? (PrizeIds[_selected] == "chips" ? "50 Chips gewonnen!" : G1C_ItemCatalog.Get(PrizeIds[_selected]).Name + " gewonnen!")
                        : "Der Greifer hat das Item fallen lassen. Noch ein Versuch?";
                }
            }
        }

        private void Advance(Phase next)
        {
            _phase = next;
            _timer = 0;
        }

        protected override void DrawGame(G1C_Renderer r)
        {
            if (_phase == Phase.Ready)
            {
                DrawMachine(r);
                AddButtonRow(Btn("[A] Zurück", Key.A), Btn("[D] Weiter", Key.D),
                             Btn("[E] Versuch", Key.E), Btn("[Q] Verlassen", Key.Q));
            }
            else if (_phase == Phase.RoundOver)
            {
                DrawMachine(r);
                AddButtonRow(Btn("[E] Noch einmal", Key.E), Btn("[Q] Verlassen", Key.Q));
            }
            else if (_phase == Phase.Align)
            {
                DrawMachine(r);
                AddButtonRow(Btn("[A] Links", Key.A), Btn("[D] Rechts", Key.D),
                             Btn("[E] Absenken", Key.E), Btn("[Q] Verlassen", Key.Q));
            }
            else
            {
                DrawMachine(r);
                AddButtonRow(Btn("Greifer läuft ...", Key.None, false));
            }
        }

        private void DrawMachine(G1C_Renderer r)
        {
            string instruction = _phase == Phase.Ready ? "W/S Einsatz, A/D Preis wählen, E Versuch starten"
                : _phase == Phase.Align ? "A/D Greifer bewegen   E absenken   Q abbrechen"
                : _phase == Phase.RoundOver ? "Versuch beendet. Du kannst es erneut versuchen."
                : "Der Greifer bewegt sich. Q beendet den Automaten.";
            r.Text(instruction, 0, 40, 10, Colors.White, G1C_Renderer.ScreenWidth);
            r.DrawRect(38, 63, 404, 152, Color.FromRgb(0x9C, 0xD8, 0xE8), G1C_UiColors.PanelBorder, 3);
            r.FillRect(42, 67, 396, 134, Color.FromRgb(0x48, 0x70, 0x88));
            r.FillRect(42, 189, 396, 12, Color.FromRgb(0x78, 0x50, 0x30));
            DrawPrizes(r);
            DrawClaw(r);
            r.Text("Einsatz: " + Bet.Amount + " Chips", 0, 229, 11, G1C_UiColors.Gold, G1C_Renderer.ScreenWidth);
        }

        private void DrawPrizes(G1C_Renderer r)
        {
            for (int i = 0; i < PrizeIds.Length; i++)
            {
                double x = 48 + i * 63;
                if (i == _selected) r.DrawRect(x, 145, 34, 36, Colors.Transparent, G1C_UiColors.Gold, 2);
                bool chips = PrizeIds[i] == "chips";
                r.Sprite(chips ? "chip" : G1C_ItemCatalog.Get(PrizeIds[i]).IconId, x + 5, 151, 24, 24);
                r.Text(chips ? "50 Chips" : G1C_ItemCatalog.Get(PrizeIds[i]).Name, x - 12, 183, 8, Colors.White, 58);
            }
        }

        private void DrawClaw(G1C_Renderer r)
        {
            r.FillRect(_clawX - 1, 68, 2, Math.Max(0, _clawY - 68), Color.FromRgb(0xD8, 0xD8, 0xE0));
            r.FillRect(_clawX - 9, _clawY, 18, 3, Color.FromRgb(0xA8, 0xB0, 0xC0));
            r.FillRect(_clawX - 9, _clawY + 2, 2, 8, Color.FromRgb(0xA8, 0xB0, 0xC0));
            r.FillRect(_clawX + 7, _clawY + 2, 2, 8, Color.FromRgb(0xA8, 0xB0, 0xC0));
            if (_won && _phase == Phase.Raise)
            {
                bool chips = PrizeIds[_selected] == "chips";
                r.Sprite(chips ? "chip" : G1C_ItemCatalog.Get(PrizeIds[_selected]).IconId,
                         _clawX - 10, _clawY + 10, 20, 20);
            }
        }

        private static int PrizeCenter(int index) { return 65 + index * 63; }
    }
}
