using System;

namespace OOPGames
{
    public enum G1C_RelationshipStatus
    {
        Unknown, Acquaintance, Friend, GoodFriend, Rival, RespectedRival, Opponent, ArchEnemy
    }

    // =====================================================
    // Klasse: G1C_NpcRelationship
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Vertrauen und Respekt (je 0–100) sowie der daraus und aus den Erinnerungen
    //               abgeleitete Beziehungsstatus. Ein Statuswechsel braucht einen Abstand von
    //               5 Punkten zur Grenze, damit kleine Schwankungen nicht hin und her schalten.
    // =====================================================
    public class G1C_NpcRelationship
    {
        public const int MinValue = 0;
        public const int MaxValue = 100;
        private const int Hysteresis = 5;

        public int Trust { get; private set; }
        public int Respect { get; private set; }
        public G1C_RelationshipStatus Status { get; private set; } = G1C_RelationshipStatus.Unknown;

        // Entwickler: Eduard Schoenfeld
        public G1C_NpcRelationship(int trust, int respect)
        {
            Trust = Clamp(trust);
            Respect = Clamp(respect);
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Ändert das Vertrauen begrenzt auf 0–100 und gibt die tatsächliche Änderung zurück.
        public int ChangeTrust(int delta)
        {
            int old = Trust;
            Trust = Clamp(Trust + delta);
            return Trust - old;
        }

        // Entwickler: Eduard Schoenfeld
        public int ChangeRespect(int delta)
        {
            int old = Respect;
            Respect = Clamp(Respect + delta);
            return Respect - old;
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Neuen Status bestimmen. Gibt true zurück, wenn er sich geändert hat.
        public bool UpdateStatus(G1C_NpcMemory memory, G1C_NpcPersonality personality)
        {
            G1C_RelationshipStatus target = Classify(Trust, Respect, memory, personality);
            if (target == Status) return false;

            // Hysterese: Der neue Status muss auch mit ±5 Punkten Abstand noch gelten
            bool stable = Classify(Trust + Hysteresis, Respect + Hysteresis, memory, personality) == target &&
                          Classify(Trust - Hysteresis, Respect - Hysteresis, memory, personality) == target;
            if (Status != G1C_RelationshipStatus.Unknown && !stable) return false;

            Status = target;
            return true;
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Status aus Werten, Erinnerungen und Persönlichkeit ableiten (Reihenfolge = Priorität).
        public static G1C_RelationshipStatus Classify(int trust, int respect, G1C_NpcMemory memory, G1C_NpcPersonality personality)
        {
            if (memory.Conversations == 0 && memory.DuelsTotal == 0) return G1C_RelationshipStatus.Unknown;
            if (trust < 12 && memory.Insults >= 3) return G1C_RelationshipStatus.ArchEnemy;
            if (trust < 25) return G1C_RelationshipStatus.Opponent;

            // Rivalität entsteht nur bei ehrgeizigen NPCs durch tatsächliche Duelle
            bool rivalry = personality.Competitive && memory.DuelsTotal >= 2;
            if (rivalry && respect >= 65) return G1C_RelationshipStatus.RespectedRival;
            if (rivalry) return G1C_RelationshipStatus.Rival;

            if (trust >= 80) return G1C_RelationshipStatus.GoodFriend;
            if (trust >= 60) return G1C_RelationshipStatus.Friend;
            return G1C_RelationshipStatus.Acquaintance;
        }

        public static string DisplayName(G1C_RelationshipStatus status)
        {
            switch (status)
            {
                case G1C_RelationshipStatus.Acquaintance: return "Bekannt";
                case G1C_RelationshipStatus.Friend: return "Freund";
                case G1C_RelationshipStatus.GoodFriend: return "Guter Freund";
                case G1C_RelationshipStatus.Rival: return "Rivale";
                case G1C_RelationshipStatus.RespectedRival: return "Respektierter Rivale";
                case G1C_RelationshipStatus.Opponent: return "Gegner";
                case G1C_RelationshipStatus.ArchEnemy: return "Erzfeind";
                default: return "Unbekannt";
            }
        }

        public static bool IsHostile(G1C_RelationshipStatus s)
        {
            return s == G1C_RelationshipStatus.Opponent || s == G1C_RelationshipStatus.ArchEnemy;
        }

        public static bool IsFriendly(G1C_RelationshipStatus s)
        {
            return s == G1C_RelationshipStatus.Friend || s == G1C_RelationshipStatus.GoodFriend;
        }

        public static bool IsRival(G1C_RelationshipStatus s)
        {
            return s == G1C_RelationshipStatus.Rival || s == G1C_RelationshipStatus.RespectedRival;
        }

        private static int Clamp(int value)
        {
            return Math.Max(MinValue, Math.Min(MaxValue, value));
        }
    }
}
