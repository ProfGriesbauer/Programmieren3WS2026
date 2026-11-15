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

            double width = G1C_Renderer.ScreenWidth - 12;
            double panelHeight = 84;
            double x = 6;
            double y = G1C_Renderer.ScreenHeight - panelHeight - 4;
            r.DrawPanel(x, y, width, panelHeight);
            if (Speaker.Length > 0)
            {
                double speakerWidth = Math.Min(width - 16, 16 + Speaker.Length * 8);
                r.DrawPanel(x + 8, y - 16, speakerWidth, 22);
                r.Text(Speaker, x + 14, y - 12, 11, G1C_UiColors.PanelBorder);
            }
            r.WrappedText(CurrentText, x + 12, y + 12, 11, G1C_UiColors.Text, width - 24);

            if (IsChoicePage)
            {
                r.Text("[E] Ja     [Q] Nein", G1C_Renderer.ScreenWidth - 170, y + panelHeight - 20,
                       10, G1C_UiColors.PanelBorder, 156);
            }
            else if ((tick / 8) % 2 == 0)
            {
                r.Text("▼ [E]", G1C_Renderer.ScreenWidth - 64, y + panelHeight - 20, 10, G1C_UiColors.Loss);
            }
        }
    }
}
