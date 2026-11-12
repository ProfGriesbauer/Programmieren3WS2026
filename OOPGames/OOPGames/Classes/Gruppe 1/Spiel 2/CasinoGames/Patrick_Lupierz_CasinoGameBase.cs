using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    /// <summary>Ein anklickbarer Knopf in einem Minispiel. Ein Klick löst dieselbe Aktion wie die Taste aus.</summary>
    public class G1C_Button
    {
        public string Label { get; }
        public Key Key { get; }
        public double X { get; }
        public double Y { get; }
        public double Width { get; }
        public double Height { get; }
        public bool Enabled { get; }

        public G1C_Button(string label, Key key, double x, double y, double width, double height, bool enabled)
        {
            Label = label;
            Key = key;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Enabled = enabled;
        }

        public bool Contains(double px, double py)
        {
            return px >= X && px <= X + Width && py >= Y && py <= Y + Height;
        }
    }

    /// <summary>Einsatzauswahl in festen Stufen; von allen Minispielen gemeinsam genutzt.</summary>
    public class G1C_BetControl
    {
        private static readonly int[] Steps = { 10, 25, 50, 100, 250, 500 };
        private int _index = 1;

        public int Amount { get { return Steps[_index]; } }

        public void Increase() { if (_index < Steps.Length - 1) _index++; }

        public void Decrease() { if (_index > 0) _index--; }

        /// <summary>W/Hoch = mehr, S/Runter = weniger. Gibt true zurück, wenn die Taste verarbeitet wurde.</summary>
        public bool HandleKey(Key key)
        {
            if (G1C_InputManager.IsIncrease(key)) { Increase(); return true; }
            if (G1C_InputManager.IsDecrease(key)) { Decrease(); return true; }
            return false;
        }
    }

    /// <summary>
    /// Gemeinsame Basis aller Casino-Minispiele.
    /// Jedes Spiel hat eigene Logik, Darstellung und Eingabeverarbeitung,
    /// bucht Geld aber nur über den gemeinsamen G1C_MoneyManager.
    /// </summary>
    public abstract class G1C_CasinoGameBase
    {
        private readonly List<G1C_Button> _buttons = new List<G1C_Button>();

        protected G1C_CasinoGameBase(G1C_MoneyManager money, Random random)
        {
            Money = money ?? throw new ArgumentNullException(nameof(money));
            Random = random ?? new Random();
        }

        protected G1C_MoneyManager Money { get; }
        protected Random Random { get; }
        protected G1C_BetControl Bet { get; } = new G1C_BetControl();
        protected string Message { get; set; } = "";

        public abstract string Name { get; }

        /// <summary>Wird true, wenn der Spieler das Minispiel verlassen möchte.</summary>
        public bool WantsExit { get; private set; }

        protected void RequestExit()
        {
            WantsExit = true;
        }

        public void HandleInput(G1C_InputEvent input)
        {
            if (input == null) return;

            if (input.Kind == G1C_InputKind.Key)
            {
                OnKey(input.Key);
                return;
            }

            // Klick: zuerst Knöpfe prüfen, danach spielspezifische Klickflächen
            foreach (G1C_Button button in _buttons)
            {
                if (button.Enabled && button.Contains(input.X, input.Y))
                {
                    OnKey(button.Key);
                    return;
                }
            }
            OnClick(input.X, input.Y);
        }

        protected abstract void OnKey(Key key);

        protected virtual void OnClick(double x, double y)
        {
        }

        /// <summary>Wird vom Framework-Tick (alle 40 ms) aufgerufen, z.B. für Animationen.</summary>
        public virtual void Update()
        {
        }

        public void Draw(G1C_Renderer r)
        {
            _buttons.Clear();
            r.FillRect(0, 0, G1C_Renderer.ScreenWidth, G1C_Renderer.ScreenHeight, BackgroundColor);
            DrawHeader(r);
            DrawGame(r);
            DrawMessage(r);
            foreach (G1C_Button button in _buttons)
            {
                DrawButton(r, button);
            }
        }

        protected virtual Color BackgroundColor { get { return G1C_UiColors.Felt; } }

        protected abstract void DrawGame(G1C_Renderer r);

        protected void AddButton(string label, Key key, double x, double y, double width, bool enabled = true)
        {
            _buttons.Add(new G1C_Button(label, key, x, y, width, 22, enabled));
        }

        private void DrawHeader(G1C_Renderer r)
        {
            r.FillRect(0, 0, G1C_Renderer.ScreenWidth, 28, G1C_UiColors.HeaderBar);
            r.Text(Name, 8, 5, 15, G1C_UiColors.Gold);
            r.Sprite("chip", 340, 8, 12, 12);
            r.Text("Chips: " + Money.Balance, 356, 5, 15, G1C_UiColors.TextLight);
        }

        private void DrawMessage(G1C_Renderer r)
        {
            r.DrawPanel(4, 262, 472, 28);
            r.Text(Message, 10, 268, 11, G1C_UiColors.Text);
        }

        private static void DrawButton(G1C_Renderer r, G1C_Button b)
        {
            Color fill = b.Enabled ? G1C_UiColors.PanelFill : G1C_UiColors.Disabled;
            r.DrawRect(b.X, b.Y, b.Width, b.Height, fill, G1C_UiColors.PanelBorder, 2);
            r.Text(b.Label, b.X, b.Y + 4, 11, G1C_UiColors.Text, b.Width);
        }

        /// <summary>Hilfsfunktion: Knopfreihe gleichmäßig über die Bildschirmbreite verteilen.</summary>
        protected void AddButtonRow(params G1C_Button[] buttons)
        {
            const double y = 294;
            double width = (G1C_Renderer.ScreenWidth - 8 - (buttons.Length - 1) * 4) / buttons.Length;
            for (int i = 0; i < buttons.Length; i++)
            {
                AddButton(buttons[i].Label, buttons[i].Key, 4 + i * (width + 4), y, width, buttons[i].Enabled);
            }
        }

        /// <summary>Kurzform zum Erzeugen eines Knopfes für AddButtonRow.</summary>
        protected static G1C_Button Btn(string label, Key key, bool enabled = true)
        {
            return new G1C_Button(label, key, 0, 0, 0, 0, enabled);
        }
    }
}
