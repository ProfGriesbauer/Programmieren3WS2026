using System;
using System.Windows.Media;

namespace OOPGames
{
    /// <summary>Kopfzeile in der Welt: Kontostand, Uhrzeit, aktive Effekte und der Hinweis zur möglichen Aktion.</summary>
    public class G1C_GameHUD
    {
        public const double Height = 32;

        public void Draw(G1C_Renderer r, G1C_GameManager game)
        {
            double width = G1C_Renderer.ScreenWidth;
            double rightWidth = Math.Min(220, Math.Max(136, width * 0.30));
            double rightX = width - rightWidth - 8;
            double centerX = 145;
            double centerWidth = Math.Max(80, rightX - centerX - 6);
            r.FillRect(0, 0, width, Height, G1C_UiColors.HeaderBar);
            r.Sprite("chip", 8, 9, 14, 14);
            r.Text("Chips: " + game.Money.Balance, 28, 7, 15, G1C_UiColors.Gold);
            // Änderung durch Denis Mock: Die Anzeige gilt jetzt für Haus, Stadt und Casino.
            // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – zentrale Uhr, Handy-Hinweis und Effekte
            r.Text("Tag " + game.Clock.Day + "  " + game.Clock.TimeText + "  " + game.Clock.PhaseName,
                   centerX, 4, 11, G1C_UiColors.TextLight, centerWidth);
            string phone = game.Phone.Unread > 0 ? "[P] Handy ✉" + game.Phone.Unread : "[P] Handy";
            string mapMenu = game.CurrentLocation == G1C_WorldLocation.Exterior ? " [M] Karte [B] Menü" : " [B] Menü";
            r.Text(phone + mapMenu, rightX, 4, 8, G1C_UiColors.TextLight, rightWidth);
            r.Text(game.Buffs.Describe(game.Clock.TotalTicks), rightX, 19, 8, G1C_UiColors.Gold, rightWidth);

            if (game.State != G1C_GameState.MainGame) return;

            int noticeTicks = game.SystemMessageEndTick - game.TickCount;
            if (noticeTicks > 0 && !string.IsNullOrEmpty(game.SystemMessage))
            {
                byte alpha = (byte)(220 * Math.Min(1.0, noticeTicks / 25.0));
                double noticeWidth = Math.Min(350, width - 20);
                double noticeX = (width - noticeWidth) / 2;
                r.DrawRect(noticeX, 42, noticeWidth, 36, Color.FromArgb(alpha, 0x18, 0x20, 0x30),
                           Color.FromArgb(alpha, 0xF8, 0xD0, 0x48), 1.5);
                r.WrappedText(game.SystemMessage, noticeX + 5, 46, 8,
                              Color.FromArgb(alpha, 0xF8, 0xF8, 0xF8), noticeWidth - 10);
            }

            string hint = game.CurrentHint;
            if (string.IsNullOrEmpty(hint))
            {
                hint = "WASD/Pfeile: Laufen   E: Interagieren";
            }
            // Änderung durch Denis Mock: Hinweise stehen in der Kopfzeile und verdecken keine Kartengrenze.
            r.Text(hint, centerX, 19, 9, G1C_UiColors.TextLight, centerWidth);
        }
    }
}
