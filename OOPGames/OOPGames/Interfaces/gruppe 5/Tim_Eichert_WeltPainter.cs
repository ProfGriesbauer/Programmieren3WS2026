using System.Windows.Controls;

namespace OOPGames
{
    // Groupe-spezifischer Paintervertrag; das konkrete Zeichnen liegt in G5_FabrikPainter.
    public interface IG5_WeltPainter : IPaintGame2
    {
        void PaintWeltField(Canvas canvas, IG5_WeltSpielfeld currentField);
        void TickPaintWeltField(Canvas canvas, IG5_WeltSpielfeld currentField);
    }
}
