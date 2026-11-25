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

        /// <summary>Änderung durch: Patrick Lupierz – 19.11.2026 – wird gerufen, wenn sich der Einsatz wirklich ändert (Klick-Ton).</summary>
        public Action Changed { get; set; }

        public void Increase() { if (_index < Steps.Length - 1) { _index++; if (Changed != null) Changed(); } }

        public void Decrease() { if (_index > 0) { _index--; if (Changed != null) Changed(); } }

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
            Bet.Changed = () => Sfx("bet"); // Patrick Lupierz – 19.11.2026
        }

        protected G1C_MoneyManager Money { get; }
        protected Random Random { get; }
        protected G1C_BetControl Bet { get; } = new G1C_BetControl();
        protected string Message { get; set; } = "";
        /// <summary>Änderung durch: Patrick Lupierz – 23.11.2026 – aktuelle Meldung (lesend, für den Selbsttest)</summary>
        public string StatusMessage { get { return Message; } }

        public abstract string Name { get; }

        /// <summary>Wird true, wenn der Spieler das Minispiel verlassen möchte.</summary>
        public bool WantsExit { get; private set; }

        protected void RequestExit()
        {
            WantsExit = true;
        }

        // Änderung durch: Patrick Lupierz – Datum: 16.11.2026 – Spiele melden Geräusche (Gewinn, Platschen, ...);
        // abgespielt werden sie vom G1C_SoundSystem des GameManagers. Ohne Empfänger passiert nichts.
        public Action<string> SoundRequested { get; set; }

        protected void Sfx(string effect)
        {
            if (SoundRequested != null) SoundRequested(effect);
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
            // Änderung durch: Patrick Lupierz – Datum: 22.11.2026 – Hintergrund füllt das ganze Fenster, nicht nur den Rahmen
            r.FillRect(G1C_Renderer.WindowLeft, G1C_Renderer.WindowTop, G1C_Renderer.WindowRight - G1C_Renderer.WindowLeft,
                       G1C_Renderer.WindowBottom - G1C_Renderer.WindowTop, BackgroundColor);
            DrawHeader(r);
            DrawGame(r);
            if (ShowMessage) DrawMessage(r);   // Änderung durch: Patrick Lupierz – 25.11.2026 – Spiele können die Meldungsleiste ausblenden
            foreach (G1C_Button button in _buttons)
            {
                DrawButton(r, button);
            }
        }

        protected virtual Color BackgroundColor { get { return G1C_UiColors.Felt; } }

        /// <summary>Änderung durch: Patrick Lupierz – 25.11.2026 – false blendet die Meldungsleiste unten aus.</summary>
        protected virtual bool ShowMessage { get { return true; } }

        protected abstract void DrawGame(G1C_Renderer r);

        protected void AddButton(string label, Key key, double x, double y, double width, bool enabled = true)
        {
            _buttons.Add(new G1C_Button(label, key, x, y, width, 22, enabled));
        }

        private void DrawHeader(G1C_Renderer r)
        {
            r.FillRect(G1C_Renderer.WindowLeft, 0, G1C_Renderer.WindowRight - G1C_Renderer.WindowLeft, 28, G1C_UiColors.HeaderBar); // Patrick Lupierz – 22.11.2026: ganze Fensterbreite
            r.Text(Name, 8, 5, 15, G1C_UiColors.Gold);
            double balanceX = G1C_Renderer.ScreenWidth - 124;
            r.Sprite("chip", balanceX, 8, 12, 12);
            r.Text("Chips: " + Money.Balance, balanceX + 16, 5, 15, G1C_UiColors.TextLight);
        }

        private void DrawMessage(G1C_Renderer r)
        {
            double y = G1C_Renderer.ScreenHeight - 58;
            r.DrawPanel(4, y, G1C_Renderer.ScreenWidth - 8, 28);
            r.WrappedText(Message, 10, y + 6, 10, G1C_UiColors.Text, G1C_Renderer.ScreenWidth - 20);
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
            double y = G1C_Renderer.ScreenHeight - 26;
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
