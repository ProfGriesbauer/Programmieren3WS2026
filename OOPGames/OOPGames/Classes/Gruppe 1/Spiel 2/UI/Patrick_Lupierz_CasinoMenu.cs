using System.Windows.Input;

namespace OOPGames
{
    /// <summary>Menü mit Steuerung und Spielinformationen (öffnen/schließen mit [M]).</summary>
    public class G1C_CasinoMenu
    {
        /// <summary>Gibt true zurück, wenn das Menü geschlossen werden soll.</summary>
        public bool ShouldClose(Key key)
        {
            return key == Key.M || G1C_InputManager.IsBack(key) || G1C_InputManager.IsConfirm(key);
        }

        public void Draw(G1C_Renderer r, G1C_GameManager game)
        {
            r.DrawPanel(60, 40, 360, 240);
            r.Text("MENÜ", 60, 50, 16, G1C_UiColors.PanelBorder, 360);
            r.Text("Chips: " + game.Money.Balance + "    Gespielte Runden: " + game.GamesPlayed, 76, 76, 11, G1C_UiColors.Text);

            string[] lines =
            {
                "Steuerung in der Welt:", // Änderung durch Denis Mock
                "  W A S D / Pfeiltasten  Laufen",
                "  E                      Interagieren / Weiter",
                "  Q / Esc                Abbrechen / Zurück",
                "  M                      Menü",
                "",
                "In den Spielen: Tasten oder Knöpfe anklicken.",
                "Wenig Chips? Der Barkeeper hilft dir aus.",
                "",
                "Hinweis: Leertaste/Enter nicht benutzen",
                "(startet das Spiel im Framework neu)."
            };
            for (int i = 0; i < lines.Length; i++)
            {
                r.Text(lines[i], 76, 100 + i * 14, 10, G1C_UiColors.Text);
            }
            r.Text("[M] / [Q] Schließen", 60, 258, 11, G1C_UiColors.PanelBorder, 360);
        }
    }
}
