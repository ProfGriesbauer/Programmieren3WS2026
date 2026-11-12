namespace OOPGames
{
    /// <summary>Minispiel- und UI-Zustände; die aktive Welt-Szene wird separat verwaltet.</summary>
    public enum G1C_GameState
    {
        MainGame,   // Erkundung der aktiven Welt-Szene; Änderung durch Denis Mock
        Dialogue,   // Gespräch mit einem NPC
        Menu,       // Hilfe-/Pausenmenü
        Roulette,
        Poker,
        Blackjack,
        Slots,
        Result      // Ergebnis nach einem Minispiel
    }
}
