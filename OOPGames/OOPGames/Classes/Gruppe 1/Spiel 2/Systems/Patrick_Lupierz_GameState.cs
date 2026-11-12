namespace OOPGames
{
    /// <summary>Klar getrennte Spielzustände (siehe Rules_Framework, Abschnitt 8).</summary>
    public enum G1C_GameState
    {
        MainGame,   // normale Casino-Erkundung
        Dialogue,   // Gespräch mit einem NPC
        Menu,       // Hilfe-/Pausenmenü
        Roulette,
        Poker,
        Blackjack,
        Slots,
        Result      // Ergebnis nach einem Minispiel
    }
}
