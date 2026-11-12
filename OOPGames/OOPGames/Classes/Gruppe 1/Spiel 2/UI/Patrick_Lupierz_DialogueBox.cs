using System;
using System.Windows.Input;

namespace OOPGames
{
    /// <summary>
    /// Dialogfenster im Pokémon-Stil. Zeigt Seite für Seite an; auf der letzten Seite
    /// kann eine Ja/Nein-Frage stehen ([E] = Ja, [Q] = Nein).
    /// </summary>
    public class G1C_DialogueBox
    {
        private string[] _pages = new string[0];
        private int _index;
        private Action _onAccept;

        public string Speaker { get; private set; } = "";
        public bool IsOpen { get; private set; }

        public bool IsChoicePage { get { return IsOpen && _onAccept != null && _index == _pages.Length - 1; } }

        public string CurrentText { get { return IsOpen ? _pages[_index] : ""; } }

        public void Open(string speaker, string[] pages, Action onAccept)
        {
            Speaker = speaker ?? "";
            _pages = (pages == null || pages.Length == 0) ? new[] { "..." } : pages;
            _index = 0;
            _onAccept = onAccept;
            IsOpen = true;
        }

        public void Close()
        {
            IsOpen = false;
            _onAccept = null;
        }

        public void HandleKey(Key key)
        {
            if (!IsOpen) return;

            if (G1C_InputManager.IsConfirm(key))
            {
                if (_index < _pages.Length - 1)
                {
                    _index++;
                    return;
                }
                Action accept = _onAccept;
                Close();
                if (accept != null) accept(); // erst schließen, dann z.B. das Minispiel starten
            }
            else if (G1C_InputManager.IsBack(key))
            {
                Close();
            }
        }

        public void Draw(G1C_Renderer r, int tick)
        {
            if (!IsOpen) return;

            r.DrawPanel(6, 236, 468, 80);
            if (Speaker.Length > 0)
            {
                r.DrawPanel(14, 220, 8 + Speaker.Length * 8, 22);
                r.Text(Speaker, 20, 224, 11, G1C_UiColors.PanelBorder);
            }
            r.Text(CurrentText, 18, 248, 12, G1C_UiColors.Text);

            if (IsChoicePage)
            {
                r.Text("[E] Ja     [Q] Nein", 300, 294, 11, G1C_UiColors.PanelBorder);
            }
            else if ((tick / 8) % 2 == 0)
            {
                r.Text("▼ [E]", 420, 294, 11, G1C_UiColors.Loss); // blinkender Weiter-Pfeil
            }
        }
    }
}
