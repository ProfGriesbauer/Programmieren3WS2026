namespace OOPGames
{
    /// <summary>Kopfzeile mit Kontostand und Hinweisfeld für die aktive Welt-Szene.</summary>
    public class G1C_GameHUD
    {
        public const double Height = 32;

        public void Draw(G1C_Renderer r, G1C_GameManager game)
        {
            r.FillRect(0, 0, G1C_Renderer.ScreenWidth, Height, G1C_UiColors.HeaderBar);
            r.Sprite("chip", 8, 9, 14, 14);
            r.Text("Chips: " + game.Money.Balance, 28, 7, 15, G1C_UiColors.Gold);
            // Änderung durch Denis Mock: Die Anzeige gilt jetzt für Haus, Stadt und Casino.
            r.Text("★ Casino-Abenteuer ★", 180, 8, 13, G1C_UiColors.TextLight);
            r.Text("[M] Menü", 400, 8, 13, G1C_UiColors.TextLight);

            if (game.State != G1C_GameState.MainGame) return;

            string hint = game.CurrentHint;
            if (string.IsNullOrEmpty(hint))
            {
                hint = "WASD/Pfeile: Laufen   E: Interagieren";
            }
            // Änderung durch Denis Mock: Hinweise stehen in der Kopfzeile und verdecken keine Kartengrenze.
            r.Text(hint, 100, 20, 9, G1C_UiColors.TextLight, 280);
        }
    }
}
