namespace OOPGames
{
    /// <summary>Kopfzeile in der Welt: Kontostand, Uhrzeit, aktive Effekte und der Hinweis zur möglichen Aktion.</summary>
    public class G1C_GameHUD
    {
        public const double Height = 32;

        public void Draw(G1C_Renderer r, G1C_GameManager game)
        {
            r.FillRect(0, 0, G1C_Renderer.ScreenWidth, Height, G1C_UiColors.HeaderBar);
            r.Sprite("chip", 8, 9, 14, 14);
            r.Text("Chips: " + game.Money.Balance, 28, 7, 15, G1C_UiColors.Gold);
            // Änderung durch Denis Mock: Die Anzeige gilt jetzt für Haus, Stadt und Casino.
            // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – zentrale Uhr, Handy-Hinweis und Effekte
            r.Text("Tag " + game.Clock.Day + "  " + game.Clock.TimeText + "  " + game.Clock.PhaseName, 150, 4, 11, G1C_UiColors.TextLight, 180);
            string phone = game.Phone.Unread > 0 ? "[P] Handy ✉" + game.Phone.Unread : "[P] Handy";
            r.Text(phone + "  [M] Menü", 340, 4, 10, G1C_UiColors.TextLight, 136);
            r.Text(game.Buffs.Describe(game.Clock.TotalTicks), 340, 19, 8, G1C_UiColors.Gold, 136);

            if (game.State != G1C_GameState.MainGame) return;

            string hint = game.CurrentHint;
            if (string.IsNullOrEmpty(hint))
            {
                hint = "WASD/Pfeile: Laufen   E: Interagieren";
            }
            // Änderung durch Denis Mock: Hinweise stehen in der Kopfzeile und verdecken keine Kartengrenze.
            r.Text(hint, 130, 19, 9, G1C_UiColors.TextLight, 220);
        }
    }
}
