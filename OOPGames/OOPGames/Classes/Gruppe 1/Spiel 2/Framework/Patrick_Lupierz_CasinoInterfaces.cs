namespace OOPGames
{
    // Marker-Interfaces für CanBePaintedBy / CanBeRuledBy: Die Kompatibilität wird gegen
    // Interfaces geprüft, nicht gegen konkrete Klassen (Muster: IX_PaintTicTacToe in X_Griesbauer).
    // Bewusst in Spiel 2 statt in Interfaces/Gruppe 1/, weil nur dieser Ordner geändert werden darf.

    public interface IG1C_CasinoPainter : IPaintGame2
    {
    }

    public interface IG1C_CasinoRules : IGameRules2
    {
    }
}
