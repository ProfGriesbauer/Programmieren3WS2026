using System;
using System.Collections.Generic;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_TrashSystem
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Mülleimer durchsuchen. Jeder Eimer liefert höchstens einmal pro Ingame-Tag etwas.
    //               Nur NPCs auf derselben Karte innerhalb des Wahrnehmungsradius bemerken es; jeder
    //               von ihnen bewertet es höchstens einmal pro Tag und reagiert je nach Persönlichkeit.
    //               Der globale Ruf bleibt unberührt.
    // =====================================================
    public class G1C_TrashSystem
    {
        public const int PerceptionRadius = 4;   // Kacheln in jede Richtung
        private const int MaxRespectLosses = 4;  // danach ist ein NPC "daran gewöhnt"

        /// <summary>Schlüssel "Ort:x:y" -> Ingame-Tag der letzten Suche.</summary>
        public Dictionary<string, int> SearchedOnDay { get; set; } = new Dictionary<string, int>();

        public static string KeyOf(G1C_WorldLocation location, G1C_WorldObject can)
        {
            return location + ":" + can.X + ":" + can.Y;
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Eimer durchsuchen. Gibt null zurück, wenn er heute schon durchsucht wurde.
        public string Search(string key, int day, Random random, G1C_MoneyManager money, G1C_Inventory inventory)
        {
            int last;
            if (SearchedOnDay.TryGetValue(key, out last) && last >= day) return null;
            SearchedOnDay[key] = day;

            int roll = random.Next(100);
            if (roll < 35) return "Du wühlst im Müll ... nichts Brauchbares.";
            if (roll < 65)
            {
                int coins = random.Next(3, 13);
                money.Deposit(coins);
                return "Du findest " + coins + " Chips zwischen dem Müll!";
            }
            string item = roll < 90 ? "bottle" : (roll < 97 ? "snack" : "lucky_coin");
            inventory.Add(item);
            return "Du findest: " + G1C_ItemCatalog.Get(item).Name + "!";
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Anwesende NPCs in der Nähe reagieren mit einer Sprechblase und merken sich die Szene.
        public int NotifyWitnesses(IG1C_WorldScene scene, G1C_Player player, G1C_NpcSystem npcs, int day)
        {
            int witnesses = 0;
            foreach (G1C_WorldObject obj in scene.Objects)
            {
                G1C_Npc npc = obj as G1C_Npc;
                if (npc == null || Math.Max(Math.Abs(npc.X - player.TileX), Math.Abs(npc.Y - player.TileY)) > PerceptionRadius)
                    continue;

                witnesses++;
                G1C_NpcCharacter c = npcs.Find(npc.Name);
                if (c == null)
                {
                    npcs.Bubbles.ShowReaction(npc, "Äh ... alles in Ordnung?", npcs.Tick);
                    continue;
                }

                bool firstToday = c.Memory.LastTrashSeenDay != day;
                c.Memory.LastTrashSeenDay = day;
                int loss;
                string text = Reaction(c, out loss);
                if (firstToday)
                {
                    c.Memory.TrashSeenCount++;
                    if (c.Memory.TrashSeenCount > MaxRespectLosses) loss = 0;
                    if (loss != 0) c.Relationship.ChangeRespect(-loss);
                    c.Relationship.UpdateStatus(c.Memory, c.Personality);
                    if (string.IsNullOrEmpty(c.Memory.PendingRemark))
                        c.Memory.PendingRemark = "Ich habe dich neulich am Mülleimer gesehen ...";
                }
                npcs.Bubbles.ShowReaction(npc, firstToday && loss != 0 ? text + " (Respekt -" + loss + ")" : text, npcs.Tick);
            }
            return witnesses;
        }

        private static string Reaction(G1C_NpcCharacter c, out int respectLoss)
        {
            G1C_RelationshipStatus status = c.Relationship.Status;
            respectLoss = 0;
            if (G1C_NpcRelationship.IsFriendly(status)) return "Alles okay bei dir?";
            if (c.Purse < 300) return "Kenn ich. Harte Zeiten.";
            if (G1C_NpcRelationship.IsRival(status)) { respectLoss = 3; return "Suchst du da dein Talent?"; }
            if (c.Personality.Pride >= 0.7) { respectLoss = 5; return "Im Müll? Wie tief kann man sinken."; }
            respectLoss = 3;
            return "Suchst du im Müll nach Chips?";
        }
    }
}
