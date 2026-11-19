using System.Windows.Input;

namespace OOPGames
{
    /// <summary>Menü mit Steuerung und Spielinformationen (öffnen/schließen mit [B]).</summary>
    public class G1C_CasinoMenu
    {
        /// <summary>Gibt true zurück, wenn das Menü geschlossen werden soll.</summary>
        public bool ShouldClose(Key key)
        {
            return key == Key.B || G1C_InputManager.IsBack(key) || G1C_InputManager.IsConfirm(key);
        }

        public void Draw(G1C_Renderer r, G1C_GameManager game)
        {
            double x = (G1C_Renderer.ScreenWidth - 360) / 2;
            double y = (G1C_Renderer.ScreenHeight - 240) / 2;
            r.DrawPanel(x, y, 360, 240);
            r.Text("MENÜ", x, y + 10, 16, G1C_UiColors.PanelBorder, 360);
            r.Text("Chips: " + game.Money.Balance + "    Gespielte Runden: " + game.GamesPlayed,
                   x + 16, y + 36, 11, G1C_UiColors.Text);

            string[] lines =
            {
                "Steuerung in der Welt:", // Änderung durch Denis Mock
                "  W A S D / Pfeiltasten  Laufen",
                "  E                      Interagieren / Weiter",
                "  Q / Esc                Abbrechen / Zurück",
                "  M                      Stadtkarte (im Freien)",
                "  B                      Menü (pausiert die Uhr)",
                "  P / I                  Handy / Inventar",
                "  T / F11 (im Menü)      Ton an-aus / Handy-Mixer",   // Patrick Lupierz – 16.11.2026/19.11.2026
                "In den Spielen: Tasten oder Knöpfe anklicken.",
                "Wenig Chips? Jobs an der Bar, Pfand im Laden.",
                "Hinweis: Leertaste/Enter nicht benutzen",
                "(startet das Spiel im Framework neu)."
            };
            for (int i = 0; i < lines.Length; i++)
            {
                r.Text(lines[i], x + 16, y + 60 + i * 14, 10, G1C_UiColors.Text);
            }
            r.Text("[B] / [Q] Schließen", x, y + 218, 11, G1C_UiColors.PanelBorder, 360);
        }
    }
}
