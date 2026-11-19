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

        // Änderung durch: Patrick Lupierz – Datum: 16.11.2026 – optionale Skalierung (Poker-Hand 5 % größer)
        public static void Draw(G1C_Renderer r, G1C_Card card, double x, double y, bool faceDown, double scale = 1.0)
        {
            double w = CardWidth * scale, h = CardHeight * scale;
            if (faceDown || card == null)
            {
                r.Sprite("card_back", x, y, w, h);
                return;
            }

            r.Sprite("card_front", x, y, w, h);
            Color color = card.IsRed ? RedSuit : BlackSuit;
            r.Text(card.RankText, x + 4 * scale, y + 2 * scale, 13 * scale, color);
            r.Text(card.SuitSymbol, x, y + 20 * scale, 22 * scale, color, w);
        }

        public const double FanStepDegrees = 12;
        public const double FanRadius = 170;

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026
        // Aufgabe: Eigene Hand wie in der Hand gehalten: Karten im Halbkreis gefächert (jede Karte um einen gemeinsamen
        //          Punkt unterhalb der Hand gedreht), leicht überlappend, mittig um centerX.
        public static void DrawFan(G1C_Renderer r, IReadOnlyList<G1C_Card> cards, double centerX, double top, double scale)
        {
            double w = CardWidth * scale, h = CardHeight * scale;
            for (int i = 0; i < cards.Count; i++)
            {
                r.BeginRotation(FanAngle(i, cards.Count), centerX, top + h / 2 + FanRadius);
                Draw(r, cards[i], centerX - w / 2, top, false, scale);
                r.EndRotation();
            }
        }

        public static double FanAngle(int index, int count)
        {
            return (index - (count - 1) / 2.0) * FanStepDegrees;
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
