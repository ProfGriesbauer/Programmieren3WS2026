using System;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_CasinoEventReactor
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Lässt NPCs auf tatsächliche Casino-Ereignisse reagieren. Grundlage ist nur das
    //               vorhandene Minispiel-Ergebnis (Kontostand vorher/nachher) – keine eigene
    //               Ergebnisberechnung. Reagieren dürfen nur NPCs in Sichtweite des Tisches.
    // =====================================================
    public class G1C_CasinoEventReactor
    {
        public const int BigResult = 300;     // ab diesem Gewinn/Verlust reden die Leute darüber
        private const int PerceptionRange = 7; // Kacheln (Manhattan-Distanz)

        private readonly G1C_NpcSystem _system;

        public G1C_CasinoEventReactor(G1C_NpcSystem system)
        {
            _system = system ?? throw new ArgumentNullException(nameof(system));
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Auswertung, nachdem der Spieler ein Minispiel verlassen hat.
        public void OnMinigameFinished(G1C_InteractionType game, int balanceDiff)
        {
            G1C_NpcCharacter dealer = _system.Find(DealerOf(game));
            G1C_Npc origin = dealer != null ? _system.WorldNpcOf(dealer) : null;
            int originX = origin != null ? origin.X : 3; // Automaten stehen links unten
            int originY = origin != null ? origin.Y : 8;

            // Pokerrunden gegen Max sind Duelle
            if (game == G1C_InteractionType.Poker && dealer != null && balanceDiff != 0)
            {
                RecordDuel(dealer, balanceDiff > 0);
            }

            if (Math.Abs(balanceDiff) < BigResult) return;
            bool win = balanceDiff > 0;
            if (win) _system.ChangeReputation(2); // ein großer Gewinn macht bekannt

            foreach (var pair in _system.PlacedNpcs)
            {
                if (pair.Value == dealer && game == G1C_InteractionType.Poker) continue; // hat schon reagiert
                int distance = Math.Abs(pair.Key.X - originX) + Math.Abs(pair.Key.Y - originY);
                if (distance > PerceptionRange) continue;

                G1C_NpcCharacter c = pair.Value;
                if (win && G1C_NpcRelationship.IsFriendly(c.Relationship.Status)) c.TrySetMood(G1C_NpcMood.Happy, _system.Tick);
                _system.Bubbles.ShowReaction(pair.Key, ReactionTo(c, win), _system.Tick);
            }
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Duell-Ergebnis merken; Respekt, Stimmung und Rivalität entwickeln sich je nach Stolz.
        public void RecordDuel(G1C_NpcCharacter npc, bool playerWon)
        {
            G1C_NpcMemory memory = npc.Memory;
            int tick = _system.Tick;
            string remark;

            if (playerWon)
            {
                memory.DuelsWonByPlayer++;
                npc.Relationship.ChangeRespect(6);
                if (npc.Personality.Pride >= 0.6) npc.Relationship.ChangeTrust(-2); // ein stolzer Verlierer ist gekränkt

                int wins = memory.DuelsWonByPlayer;
                if (npc.Relationship.Status == G1C_RelationshipStatus.RespectedRival) remark = "Du bist besser geworden. Das muss ich zugeben.";
                else if (wins == 1) remark = "Gutes Spiel.";
                else if (wins == 2) remark = "Das war bestimmt nur Glück.";
                else remark = "Jetzt reicht's! Revanche!";

                if (wins >= 2 && npc.Personality.Pride >= 0.6) npc.TrySetMood(G1C_NpcMood.Angry, tick, force: true);
                memory.PendingRemark = "Du hast mich letztes Mal überrascht. Das passiert mir nicht noch einmal.";
            }
            else
            {
                memory.DuelsLostByPlayer++;
                npc.TrySetMood(G1C_NpcMood.Happy, tick);
                remark = "Na, bereit für die nächste Niederlage?";
                memory.PendingRemark = "Letztes Mal hast du ganz schön Lehrgeld bezahlt.";
            }

            npc.Relationship.UpdateStatus(memory, npc.Personality);
            _system.Bubbles.ShowReaction(_system.WorldNpcOf(npc), remark, tick);
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Nach erledigter Aufgabe bedankt sich der Auftraggeber öffentlich; der Ruf steigt.
        public void OnTaskCompleted(G1C_NpcCharacter giver)
        {
            _system.ChangeReputation(3);
            _system.Bubbles.ShowReaction(_system.WorldNpcOf(giver), "Danke für deine Hilfe!", _system.Tick);
        }

        private static string ReactionTo(G1C_NpcCharacter c, bool win)
        {
            G1C_RelationshipStatus s = c.Relationship.Status;
            if (G1C_NpcRelationship.IsFriendly(s)) return win ? "Unglaublich! Glückwunsch!" : "Kopf hoch, das wird wieder.";
            if (G1C_NpcRelationship.IsRival(s)) return win ? "Genieß es, solange es anhält." : "Ha! Das Glück ist weg.";
            if (G1C_NpcRelationship.IsHostile(s)) return win ? "Pures Glück. Mehr nicht." : "Geschieht ihm recht.";
            if (c.Relationship.Trust < 35) return win ? "Das ging aber schnell ..." : "Autsch ...";
            return win ? "Nicht schlecht!" : "Das tat weh ...";
        }

        private static string DealerOf(G1C_InteractionType game)
        {
            switch (game)
            {
                case G1C_InteractionType.Roulette: return "Croupier Rosa";
                case G1C_InteractionType.Blackjack: return "Dealer Ben";
                case G1C_InteractionType.Poker: return "Pokerprofi Max";
                default: return null;
            }
        }
    }
}
