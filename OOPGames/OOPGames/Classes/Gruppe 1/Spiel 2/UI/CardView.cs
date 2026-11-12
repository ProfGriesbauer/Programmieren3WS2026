using System.Collections.Generic;
using System.Windows.Media;

namespace OOPGames
{
    /// <summary>Zeichnet Spielkarten (Vorder- und Rückseite sind austauschbare Sprites).</summary>
    public static class G1C_CardView
    {
        public const double CardWidth = 40;
        public const double CardHeight = 56;

        private static readonly Color RedSuit = Color.FromRgb(0xD0, 0x20, 0x20);
        private static readonly Color BlackSuit = Color.FromRgb(0x20, 0x20, 0x28);

        public static void Draw(G1C_Renderer r, G1C_Card card, double x, double y, bool faceDown)
        {
            if (faceDown || card == null)
            {
                r.Sprite("card_back", x, y, CardWidth, CardHeight);
                return;
            }

            r.Sprite("card_front", x, y, CardWidth, CardHeight);
            Color color = card.IsRed ? RedSuit : BlackSuit;
            r.Text(card.RankText, x + 4, y + 2, 13, color);
            r.Text(card.SuitSymbol, x, y + 20, 22, color, CardWidth);
        }

        /// <summary>Zeichnet eine Hand. Karten ab Index hideFrom werden verdeckt gezeigt.</summary>
        public static void DrawHand(G1C_Renderer r, IReadOnlyList<G1C_Card> cards, double x, double y,
                                    double spacing, int hideFrom = int.MaxValue)
        {
            for (int i = 0; i < cards.Count; i++)
            {
                Draw(r, cards[i], x + i * spacing, y, i >= hideFrom);
            }
        }
    }
}
