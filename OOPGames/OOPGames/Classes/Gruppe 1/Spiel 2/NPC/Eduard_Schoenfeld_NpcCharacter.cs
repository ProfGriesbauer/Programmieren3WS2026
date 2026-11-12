using System;
using System.Collections.Generic;

namespace OOPGames
{
    public enum G1C_NpcMood { Neutral, Happy, Sad, Angry, Nervous }

    /// <summary>Gesichtsausdruck im Gesprächsporträt (Sprite-ID: portrait_&lt;skin&gt;_&lt;ausdruck&gt;).</summary>
    public enum G1C_NpcExpression { Neutral, Happy, Angry, Sad, Surprised, Thoughtful }

    // =====================================================
    // Klasse: G1C_NpcPersonality
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Feste Charaktereigenschaften und Texte eines NPCs (reine Daten).
    // =====================================================
    public class G1C_NpcPersonality
    {
        public string Role { get; set; } = "";
        public string Traits { get; set; } = "";
        public double Generosity { get; set; }      // 0 = geizig, 1 = sehr großzügig
        public double Pride { get; set; }           // 0 = bescheiden, 1 = arrogant
        public double RiskAppetite { get; set; }    // 0 = vorsichtig, 1 = risikofreudig
        public bool Competitive { get; set; }       // kann durch Duelle zum Rivalen werden
        public bool RefusesMoneyByRule { get; set; } // z.B. Croupiers: Hausregel
        public G1C_NpcMood BaseMood { get; set; } = G1C_NpcMood.Neutral;

        public string FirstMeeting { get; set; } = "Hallo, wir kennen uns noch nicht.";
        public string GeneralInfo { get; set; } = "";
        public string PersonalStory { get; set; } = "";

        public Dictionary<G1C_RelationshipStatus, string> Greetings { get; } = new Dictionary<G1C_RelationshipStatus, string>();
        public string[] GeneralMonologues { get; set; } = new string[0];
        public string[] FriendMonologues { get; set; } = new string[0];
        public string[] RivalMonologues { get; set; } = new string[0];
        public string[] EnemyMonologues { get; set; } = new string[0];
        public Dictionary<G1C_NpcMood, string[]> MoodMonologues { get; } = new Dictionary<G1C_NpcMood, string[]>();

        /// <summary>Lena: Gesprächsaufgabe „zu einer Pause überreden“.</summary>
        public bool HasPauseTask { get; set; }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Wählt einen kurzen Monolog passend zu Beziehung und Stimmung.
        public string PickMonologue(G1C_RelationshipStatus status, G1C_NpcMood mood, Random random)
        {
            string[] relationLines = G1C_NpcRelationship.IsFriendly(status) ? FriendMonologues
                : G1C_NpcRelationship.IsRival(status) ? RivalMonologues
                : G1C_NpcRelationship.IsHostile(status) ? EnemyMonologues
                : null;
            string[] moodLines;
            MoodMonologues.TryGetValue(mood, out moodLines);

            double roll = random.NextDouble();
            if (relationLines != null && relationLines.Length > 0 && roll < 0.5) return Pick(relationLines, random);
            if (moodLines != null && moodLines.Length > 0 && roll < 0.8) return Pick(moodLines, random);
            return GeneralMonologues.Length > 0 ? Pick(GeneralMonologues, random) : null;
        }

        private static string Pick(string[] lines, Random random)
        {
            return lines[random.Next(lines.Length)];
        }
    }

    // =====================================================
    // Klasse: G1C_NpcCharacter
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Ein NPC mit Profil, Stimmung, Geldbeutel, Beziehung und Erinnerungen.
    //               Verknüpft über den Namen mit der vorhandenen Weltfigur (G1C_Npc).
    // =====================================================
    public class G1C_NpcCharacter
    {
        private const int MoodLockTicks = 750;   // 30 s: Stimmung wechselt nicht ständig
        private const int MoodResetTicks = 3000; // 2 min: danach zurück zur Grundstimmung

        private int _moodLockedUntil = G1C_NpcMemory.Never;
        private int _moodResetAt = int.MaxValue;
        private G1C_NpcMood _baseMood;

        // Entwickler: Eduard Schoenfeld
        public G1C_NpcCharacter(string name, string skin, G1C_NpcPersonality personality, int purse, int trust, int respect)
        {
            if (purse < 0) throw new ArgumentOutOfRangeException(nameof(purse));
            Name = name;
            Skin = skin;
            Personality = personality ?? throw new ArgumentNullException(nameof(personality));
            Purse = purse;
            Relationship = new G1C_NpcRelationship(trust, respect);
            _baseMood = personality.BaseMood;
            Mood = _baseMood;
        }

        public string Name { get; }
        public string Skin { get; }
        public G1C_NpcPersonality Personality { get; }
        public int Purse { get; private set; }
        public G1C_NpcMood Mood { get; private set; }
        public G1C_NpcRelationship Relationship { get; }
        public G1C_NpcMemory Memory { get; } = new G1C_NpcMemory();

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Stimmung setzen; ohne force nur, wenn die letzte Änderung lange genug her ist.
        public bool TrySetMood(G1C_NpcMood mood, int tick, bool force = false)
        {
            if (!force && tick < _moodLockedUntil) return false;
            Mood = mood;
            _moodLockedUntil = tick + MoodLockTicks;
            _moodResetAt = tick + MoodResetTicks;
            return true;
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Dauerhafte Grundstimmung ändern (z.B. Lena nach einer Pause).
        public void ChangeBaseMood(G1C_NpcMood mood)
        {
            _baseMood = mood;
        }

        // Entwickler: Eduard Schoenfeld
        public void UpdateMood(int tick)
        {
            if (Mood != _baseMood && tick >= _moodResetAt) Mood = _baseMood;
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Geld aus dem eigenen Geldbeutel nehmen – nie mehr als vorhanden.
        public bool TryPay(int amount)
        {
            if (amount <= 0 || amount > Purse) return false;
            Purse -= amount;
            return true;
        }

        public G1C_NpcExpression MoodExpression
        {
            get
            {
                switch (Mood)
                {
                    case G1C_NpcMood.Happy: return G1C_NpcExpression.Happy;
                    case G1C_NpcMood.Sad: return G1C_NpcExpression.Sad;
                    case G1C_NpcMood.Angry: return G1C_NpcExpression.Angry;
                    case G1C_NpcMood.Nervous: return G1C_NpcExpression.Surprised;
                    default: return G1C_NpcExpression.Neutral;
                }
            }
        }

        public static string MoodName(G1C_NpcMood mood)
        {
            switch (mood)
            {
                case G1C_NpcMood.Happy: return "glücklich";
                case G1C_NpcMood.Sad: return "traurig";
                case G1C_NpcMood.Angry: return "verärgert";
                case G1C_NpcMood.Nervous: return "nervös";
                default: return "neutral";
            }
        }
    }
}
