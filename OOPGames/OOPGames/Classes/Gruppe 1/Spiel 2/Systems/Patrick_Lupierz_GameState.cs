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
        Result,     // Ergebnis nach einem Minispiel
        // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – Gespräch mit Porträt und Antwortoptionen
        Conversation,
        Title,      // Hauptmenü, Speicherplätze, Charaktererstellung
        ListMenu,   // Laden, Inventar, Kleiderschrank
        Phone       // Klapphandy
    }
}
