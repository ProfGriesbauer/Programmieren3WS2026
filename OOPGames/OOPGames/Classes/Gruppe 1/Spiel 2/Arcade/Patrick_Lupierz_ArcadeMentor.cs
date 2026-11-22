using System.Collections.Generic;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_ArcadeMentor
    // Entwickler: Patrick Lupierz
    // Erstellt: 22.11.2026
    // Beschreibung: Logik des Spielhallen-NPCs "Patrick" (blond, schwarzes T-Shirt, blaue Hose, weiße Schuhe;
    //               steht am mittleren Automaten, optimistisch und freundlich):
    //               - jeden Spieltag ein anderer Spieltipp (gleicher Tag = gleicher Tipp),
    //               - einmalig 35 Chips, sobald der Spieler seinen Schlangen-Rekord (220 Punkte = 22 Früchte,
    //                 kurz bevor die Schlange ihr Höchsttempo erreicht) übertrifft.
    //               Ob schon ausgezahlt wurde, steht im gespeicherten NPC-Gedächtnis (RecordRewardPaid).
    //               Reine Logik; das Gespräch läuft über G1C_Conversation (Personality.Topics).
    // =====================================================
    public static class G1C_ArcadeMentor
    {
        public const string NpcName = "Patrick";
        public const string Skin = "pc-F0D060-F0C8A0-181818-2848A8-F8F8F8-x";   // Haare, Haut, T-Shirt, Hose, Schuhe, keine Mütze
        public const string SnakeGameName = "Schlange";
        public const int SnakeRecord = 220;
        public const int RecordReward = 35;

        public static readonly string[] Tips =
        {
            "Schlange: Fahr am Rand entlang in Bahnen hin und her. So beißt du dich fast nie selbst!",
            "Sternenjäger: Bleib in Bewegung. Die Sturzflieger zielen dahin, wo du gerade WARST.",
            "Ziegelbrecher: Triff den Ball mit der Schlägerkante, dann fliegt er schräg in die Ecken.",
            "Pong-Duell: Die KI reagiert spät. Steile Bälle kurz vor ihrem Schläger bringen Punkte!",
            "Schlange: Ab 180 Punkten wird sie schneller. Plan zwei Züge voraus, nicht nur einen.",
            "Greifautomat: Lass die Kralle genau über der Mitte los. Die Ränder rutschen gern weg.",
            "Gast-Automat: Da laufen Spiele anderer Gruppen. Erst die Regeln lesen, dann loslegen!",
            "Jede Runde kostet 5 Chips, gute Runden bringen bis zu 30 zurück. Üben lohnt sich!",
            "Pause machen ist auch ein Tipp. Mit frischem Kopf spielt man doppelt so gut."
        };

        // Entwickler: Patrick Lupierz – Tipp des Tages (jeder Spieltag ein anderer, reihum)
        public static string TipFor(int day)
        {
            int index = ((day - 1) % Tips.Length + Tips.Length) % Tips.Length;
            return Tips[index];
        }

        public static int BestSnake(IReadOnlyDictionary<string, int> highscores)
        {
            int best;
            return highscores != null && highscores.TryGetValue(SnakeGameName, out best) ? best : 0;
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Antwort auf die Rekord-Frage; zahlt die Belohnung genau einmal aus, sobald der Rekord übertroffen ist.
        public static string RecordAnswer(G1C_TopicContext c)
        {
            int best = BestSnake(c.Highscores);
            if (c.Memory != null && c.Memory.RecordRewardPaid)
                return "Du hast meinen Rekord geknackt! Unglaublich. Ich trainiere schon für meine Revanche!";
            if (best > SnakeRecord)
            {
                if (c.Memory != null) c.Memory.RecordRewardPaid = true;
                if (c.Money != null) c.Money.Deposit(RecordReward);
                if (c.Notice != null) c.Notice("+" + RecordReward + " Chips");
                return best + " Punkte bei der Schlange? WOW! Mein Rekord ist Geschichte. Versprochen ist versprochen: "
                       + RecordReward + " Chips für dich!";
            }
            if (best == 0)
                return "Mein Schlangen-Rekord: " + SnakeRecord + " Punkte. Schlag ihn, dann gibt's " + RecordReward
                       + " Chips von mir. Du schaffst das!";
            return "Mein Rekord: " + SnakeRecord + " Punkte, deiner: " + best + ". Nur noch " + (SnakeRecord - best + 10)
                   + " Punkte bis zu " + RecordReward + " Chips. Du bist nah dran!";
        }

        // Entwickler: Patrick Lupierz – Gesprächsprofil (in G1C_ShopNpcRoster eingetragen)
        public static G1C_NpcCharacter Create()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Spielhallen-Stammgast", Traits = "optimistisch, freundlich, hilfsbereit, ehrgeizig",
                Generosity = 0.6, Pride = 0.3, RiskAppetite = 0.3,
                BaseMood = G1C_NpcMood.Happy,
                FirstMeeting = "Hey, ich bin Patrick! Ich hab jeden Tag einen Tipp für dich. Und wenn du meinen Schlangen-Rekord knackst, gibt's "
                               + RecordReward + " Chips!",
                GeneralInfo = "Die Spielhalle ist der beste Ort der Stadt. Hier verliert man höchstens 5 Chips, aber gewinnt immer Spaß!",
                PersonalStory = "Ich hab mit der Schlange angefangen, als ich so alt war wie mein Joystick. " + SnakeRecord + " Punkte, ganz ehrlich erspielt!",
                GeneralMonologues = new[]
                {
                    "Heute ist ein guter Tag für einen neuen Rekord!",
                    "Noch eine Runde? Na klar!",
                    "Verlieren gehört dazu. Beim nächsten Mal klappt's!",
                    "Hast du schon den Gast-Automaten probiert? Super Spiele!",
                    "Jeder Highscore fängt mit null Punkten an."
                }
            };
            p.MoodMonologues[G1C_NpcMood.Happy] = new[] { "Ich liebe dieses Piepen, wenn die Schlange frisst!" };
            p.Topics.Add(new G1C_DialogueTopic("Hast du einen Tipp für heute?", c =>
                "Tipp des Tages: " + TipFor(c.Day) + (c.TimesAsked > 0 ? " Morgen hab ich einen neuen für dich!" : ""),
                G1C_NpcExpression.Happy));
            p.Topics.Add(new G1C_DialogueTopic("Wie hoch ist dein Schlangen-Rekord?", RecordAnswer, G1C_NpcExpression.Happy));
            p.Topics.Add(new G1C_DialogueTopic("Bist du immer so gut gelaunt?", G1C_DialogueTopic.Cycle(
                "Klar! Wer lacht, spielt besser. Das ist wissenschaftlich fast bewiesen.",
                "Sogar wenn ich verliere. Dann weiß ich wenigstens, was ich üben muss!")));
            return new G1C_NpcCharacter(NpcName, Skin, p, 90, 55, 50);
        }
    }
}
