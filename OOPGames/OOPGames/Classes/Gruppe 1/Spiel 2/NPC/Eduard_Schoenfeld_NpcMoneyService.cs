using System;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_MoneyRequestResult
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Ergebnis einer Geldanfrage an einen NPC.
    // =====================================================
    public class G1C_MoneyRequestResult
    {
        public bool Granted { get; set; }
        public int Amount { get; set; }
        public string Reply { get; set; } = "";
        public G1C_NpcExpression Expression { get; set; }
        public int TrustDelta { get; set; }
    }

    // =====================================================
    // Klasse: G1C_NpcMoneyService
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Entscheidet, ob ein NPC dem Spieler Chips schenkt, und bucht das Geld vollständig
    //               vom NPC-Geldbeutel auf das Spielerkonto. Geld entsteht dabei nie neu.
    //               Schutz gegen Ausnutzen: Wartezeit zwischen Anfragen, sinkende Bereitschaft
    //               mit jeder Anfrage und eine Obergrenze pro NPC.
    // =====================================================
    public class G1C_NpcMoneyService
    {
        public const int RequestCooldownTicks = 2250; // 90 s
        public const int MaxGiftPerNpc = 600;
        public const int MaxSingleGift = 300;
        public const int MinPurseToGive = 200;        // wer weniger hat, braucht es selbst
        private const double GiveThreshold = 0.55;

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Geldanfrage auswerten und bei Zusage die Chips übertragen.
        public G1C_MoneyRequestResult Handle(G1C_NpcCharacter npc, G1C_MoneyManager player, int tick, int reputation)
        {
            if (npc == null) throw new ArgumentNullException(nameof(npc));
            if (player == null) throw new ArgumentNullException(nameof(player));

            G1C_NpcMemory memory = npc.Memory;
            bool tooSoon = memory.MoneyRequests > 0 && tick - memory.LastMoneyRequestTick < RequestCooldownTicks;
            memory.MoneyRequests++;
            memory.LastMoneyRequestTick = tick;

            G1C_RelationshipStatus status = npc.Relationship.Status;

            if (npc.Personality.RefusesMoneyByRule)
                return Refuse("Tut mir leid. Angestellte dürfen Gästen kein Geld geben – Hausregel.", G1C_NpcExpression.Neutral, 0);
            if (tooSoon)
            {
                npc.TrySetMood(G1C_NpcMood.Angry, tick);
                return Refuse("Du hast doch gerade erst gefragt!", G1C_NpcExpression.Angry, -4);
            }
            if (G1C_NpcRelationship.IsHostile(status))
                return Refuse("Von mir bekommst du gar nichts.", G1C_NpcExpression.Angry, -2);
            if (npc.Purse < MinPurseToGive)
                return Refuse("Ich habe selbst kaum noch etwas ... tut mir leid.", G1C_NpcExpression.Sad, 0);
            if (memory.MoneyGivenToPlayer >= MaxGiftPerNpc)
                return Refuse("Ich habe dir schon genug gegeben. Jetzt bist du dran.", G1C_NpcExpression.Thoughtful, -1);

            bool playerBroke = player.Balance < 50;
            double willingness = npc.Relationship.Trust / 100.0 * (0.5 + npc.Personality.Generosity)
                                 + StatusBonus(status) + MoodBonus(npc.Mood)
                                 + (reputation - 50) / 250.0
                                 - 0.08 * (memory.MoneyRequests - 1)
                                 + (playerBroke ? 0.1 : 0.0);

            // Stolze Rivalen geben aus Überheblichkeit einen kleinen Betrag
            bool arrogantGift = G1C_NpcRelationship.IsRival(status) && npc.Personality.Pride >= 0.6;
            if (willingness < GiveThreshold && !arrogantGift)
            {
                string reply = status == G1C_RelationshipStatus.Unknown || status == G1C_RelationshipStatus.Acquaintance
                    ? "Ich kenne dich doch kaum. Nein."
                    : "Nein, diesmal nicht.";
                return Refuse(reply, G1C_NpcExpression.Thoughtful, 0);
            }

            int amount = willingness < GiveThreshold ? 30 : RoundTo10(npc.Purse * 0.02 * (0.5 + willingness));
            int limit = Math.Min(MaxSingleGift, Math.Min(npc.Purse / 4, MaxGiftPerNpc - memory.MoneyGivenToPlayer));
            amount = Math.Max(20, Math.Min(amount, limit));
            if (amount > limit || !npc.TryPay(amount))
                return Refuse("Gerade kann ich nichts entbehren.", G1C_NpcExpression.Sad, 0);

            player.Deposit(amount);
            memory.MoneyGivenToPlayer += amount;

            string text = arrogantGift && willingness < GiveThreshold
                ? "Hier, " + amount + " Chips. Damit du mir am Tisch nicht ausgehst."
                : "Na gut, hier sind " + amount + " Chips. Gib gut darauf acht.";
            return new G1C_MoneyRequestResult
            {
                Granted = true, Amount = amount, Reply = text,
                Expression = arrogantGift ? G1C_NpcExpression.Thoughtful : G1C_NpcExpression.Happy,
                TrustDelta = -1 // Betteln kostet immer etwas Ansehen
            };
        }

        private static G1C_MoneyRequestResult Refuse(string reply, G1C_NpcExpression expression, int trustDelta)
        {
            return new G1C_MoneyRequestResult { Granted = false, Reply = reply, Expression = expression, TrustDelta = trustDelta };
        }

        private static double StatusBonus(G1C_RelationshipStatus status)
        {
            switch (status)
            {
                case G1C_RelationshipStatus.GoodFriend: return 0.3;
                case G1C_RelationshipStatus.Friend: return 0.15;
                case G1C_RelationshipStatus.Unknown: return -0.2;
                default: return 0.0;
            }
        }

        private static double MoodBonus(G1C_NpcMood mood)
        {
            switch (mood)
            {
                case G1C_NpcMood.Happy: return 0.1;
                case G1C_NpcMood.Angry: return -0.3;
                case G1C_NpcMood.Sad: return -0.1;
                default: return 0.0;
            }
        }

        private static int RoundTo10(double value)
        {
            return (int)Math.Round(value / 10.0) * 10;
        }
    }
}
