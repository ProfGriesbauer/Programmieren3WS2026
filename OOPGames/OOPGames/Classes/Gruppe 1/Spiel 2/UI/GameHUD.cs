namespace OOPGames
{
    /// <summary>Kopfzeile mit Kontostand und Hinweisfeld für Interaktionen in der Casino-Welt.</summary>
    public class G1C_GameHUD
    {
        public const double Height = 32;

        public void Draw(G1C_Renderer r, G1C_GameManager game)
        {
            r.FillRect(0, 0, G1C_Renderer.ScreenWidth, Height, G1C_UiColors.HeaderBar);
            r.Sprite("chip", 8, 9, 14, 14);
            r.Text("Chips: " + game.Money.Balance, 28, 7, 15, G1C_UiColors.Gold);
            r.Text("★ Casino Gruppe 1 ★", 180, 8, 13, G1C_UiColors.TextLight);
            r.Text("[M] Menü", 400, 8, 13, G1C_UiColors.TextLight);

            if (game.State != G1C_GameState.MainGame) return;

            string hint = game.CurrentHint;
            if (string.IsNullOrEmpty(hint))
            {
                hint = "WASD/Pfeile: Laufen   E: Interagieren";
            }
            // Oben auf der Mauer platziert, damit kein begehbarer Bereich verdeckt wird
            r.DrawPanel(100, Height + 1, 280, 22);
            r.Text(hint, 100, Height + 5, 10, G1C_UiColors.Text, 280);
        }
    }
}
