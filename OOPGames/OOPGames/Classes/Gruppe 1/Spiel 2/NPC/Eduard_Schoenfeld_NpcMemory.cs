namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_NpcMemory
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Merkt sich nur die für spätere Gespräche relevanten Ereignisse eines NPCs
    //               (keine vollständigen Gesprächsprotokolle). Gilt für die laufende Spielsitzung.
    // =====================================================
    public class G1C_NpcMemory
    {
        public const int Never = int.MinValue / 2;

        public int Conversations { get; set; }
        public int Insults { get; set; }           // offener Groll – sinkt durch Entschuldigungen
        public bool WasEverInsulted { get; set; }  // bleibt in Erinnerung
        public int Compliments { get; set; }
        public int MoneyRequests { get; set; }
        public int MoneyGivenToPlayer { get; set; }
        public int DuelsWonByPlayer { get; set; }
        public int DuelsLostByPlayer { get; set; }
        public int TasksCompleted { get; set; }
        public bool HelpedByPlayer { get; set; }
        public bool ReputationApplied { get; set; }

        public int LastMoneyRequestTick { get; set; } = Never;
        public int LastSocialRewardTick { get; set; } = Never;
        public int LastComplimentTick { get; set; } = Never;
        public int LastApologyTick { get; set; } = Never;
        public int SocialTrustGained { get; set; }

        /// <summary>Eine Bemerkung, die der NPC bei der nächsten Begrüßung einmalig anbringt.</summary>
        public string PendingRemark { get; set; }

        public int DuelsTotal { get { return DuelsWonByPlayer + DuelsLostByPlayer; } }
    }
}
