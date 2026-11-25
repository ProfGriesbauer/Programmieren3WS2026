using System;

namespace OOPGames
{
    // =====================================================
    // Schnittstelle: IG1C_NpcDuelGame / Klasse: G1C_NpcDuel
    // Entwickler: Patrick Lupierz
    // Erstellt: 23.11.2026
    // Beschreibung: Duell gegen einen angesprochenen NPC. Im Gespräch wählt der Spieler das Spiel:
    //               - Pong:      Einsatz je 20 Chips, der Sieger bekommt beide Einsätze.
    //               - Poker:     Heads-up gegen den NPC; seine Kasse gewinnt/verliert, was der Spieler verliert/gewinnt.
    //               - Blackjack: beide spielen gegen den Dealer; wer am Ende mehr Gewinn hat, gewinnt das Duell.
    //               Die Spiele melden über diese Schnittstelle, ob das Duell entschieden ist, wer gewonnen hat und
    //               wie sich der Geldbeutel des NPCs ändert. Ausgewertet wird in G1C_GameManager.EndMinigame.
    // =====================================================
    public interface IG1C_NpcDuelGame
    {
        string DuelOpponent { get; }
        void ConfigureDuel(string opponent);
        /// <summary>Mindestens ein Ergebnis steht fest (sonst zählt das Duell nicht).</summary>
        bool DuelDecided { get; }
        bool PlayerWonDuel { get; }
        /// <summary>Chips, die der NPC durch das Duell gewinnt (+) oder verliert (−).</summary>
        int NpcPurseChange { get; }
    }

    public enum G1C_DuelGame { Pong, Poker, Blackjack }

    public static class G1C_NpcDuel
    {
        public const int MinimumPurse = 20;   // so viel muss der NPC mindestens haben, um anzunehmen

        public static string Title(G1C_DuelGame game)
        {
            switch (game)
            {
                case G1C_DuelGame.Poker: return "Poker gegen mich";
                case G1C_DuelGame.Blackjack: return "Blackjack: wer schlägt den Dealer?";
                default: return "Pong-Duell (je " + G1C_PongDuel.DuelStake + " Chips)";
            }
        }

        // Entwickler: Patrick Lupierz – passendes Minispiel für das gewählte Duell erzeugen und einrichten
        public static G1C_CasinoGameBase Create(G1C_DuelGame game, string opponent, G1C_MoneyManager money, Random random)
        {
            G1C_CasinoGameBase created;
            switch (game)
            {
                case G1C_DuelGame.Poker: created = new G1C_Poker(money, random); break;
                case G1C_DuelGame.Blackjack: created = new G1C_Blackjack(money, random); break;
                default: created = new G1C_PongDuel(money, random); break;
            }
            ((IG1C_NpcDuelGame)created).ConfigureDuel(opponent);
            return created;
        }

        public static G1C_GameState StateFor(G1C_DuelGame game)
        {
            switch (game)
            {
                case G1C_DuelGame.Poker: return G1C_GameState.Poker;
                case G1C_DuelGame.Blackjack: return G1C_GameState.Blackjack;
                default: return G1C_GameState.Arcade;
            }
        }
    }
}
