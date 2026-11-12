using System.Collections.Generic;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_NpcRoster
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Charakterprofile der bereits vorhandenen Casino-NPCs (reine Daten).
    //               Die Namen entsprechen den Weltfiguren in G1C_CasinoWorld; der Skin ist der
    //               vorhandene Sprite, damit Aussehen und Porträt übereinstimmen.
    //               Neue Charaktere: hier ein weiteres Profil ergänzen.
    // =====================================================
    public static class G1C_NpcRoster
    {
        // Entwickler: Eduard Schoenfeld
        public static List<G1C_NpcCharacter> CreateCasinoNpcs()
        {
            return new List<G1C_NpcCharacter> { Rosa(), Ben(), Max(), Toni(), Lena() };
        }

        // Croupier: professionell, zurückhaltend, regelorientiert
        private static G1C_NpcCharacter Rosa()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Croupier",
                Traits = "professionell, höflich, regelorientiert",
                Generosity = 0.3, Pride = 0.3, RiskAppetite = 0.1,
                RefusesMoneyByRule = true,
                FirstMeeting = "Guten Abend. Wir kennen uns noch nicht, oder?",
                GeneralInfo = "Am Roulette zahlt eine Zahl 35:1. Die Null gehört dem Haus.",
                PersonalStory = "Ich habe früher selbst gespielt. Seit ich am Tisch stehe, weiß ich, warum das Haus gewinnt.",
                GeneralMonologues = new[] { "Bitte platzieren Sie Ihre Einsätze.", "Rien ne va plus.", "Die Kugel rollt gleich." },
                FriendMonologues = new[] { "Schön, ihn wiederzusehen!", "Mein Lieblingsgast ist da." },
                RivalMonologues = new[] { "Er spielt mutig heute." },
                EnemyMonologues = new[] { "Bitte Abstand vom Tisch.", "Mit dem rede ich nicht mehr." }
            };
            p.Greetings[G1C_RelationshipStatus.Acquaintance] = "Willkommen zurück am Roulette-Tisch.";
            p.Greetings[G1C_RelationshipStatus.Friend] = "Hey, schön dich wiederzusehen!";
            p.Greetings[G1C_RelationshipStatus.Opponent] = "Bitte benehmen Sie sich an meinem Tisch.";
            p.MoodMonologues[G1C_NpcMood.Happy] = new[] { "Heute läuft alles rund." };
            p.MoodMonologues[G1C_NpcMood.Angry] = new[] { "Manche Gäste haben keine Manieren." };
            return new G1C_NpcCharacter("Croupier Rosa", "dealer_red", p, 800, 40, 30);
        }

        // Dealer: freundlich, gesprächig, aber ebenfalls an die Hausregeln gebunden
        private static G1C_NpcCharacter Ben()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Blackjack-Dealer",
                Traits = "freundlich, gesprächig, fair",
                Generosity = 0.4, Pride = 0.2, RiskAppetite = 0.2,
                RefusesMoneyByRule = true,
                FirstMeeting = "Hallo! Neu hier? Ich bin Ben.",
                GeneralInfo = "Beim Blackjack ziehe ich bis 16 und halte ab 17. So steht's in den Hausregeln.",
                PersonalStory = "Ich zähle keine Karten. Ehrlich. Na gut ... manchmal im Kopf, aus Gewohnheit.",
                GeneralMonologues = new[] { "Karte oder halten?", "Der Dealer hält ab 17.", "Wer will Blackjack?" },
                FriendMonologues = new[] { "Da ist ja mein Kumpel!", "Schön, ihn wiederzusehen!" },
                RivalMonologues = new[] { "Heute schlage ich ihn." },
                EnemyMonologues = new[] { "Der schon wieder ...", "Mit dem rede ich nicht mehr." }
            };
            p.Greetings[G1C_RelationshipStatus.Acquaintance] = "Na, wieder da? Die Karten warten schon.";
            p.Greetings[G1C_RelationshipStatus.GoodFriend] = "Mein Lieblingsspieler! Setz dich!";
            p.MoodMonologues[G1C_NpcMood.Happy] = new[] { "Was für ein schöner Abend!" };
            p.MoodMonologues[G1C_NpcMood.Angry] = new[] { "Heute ist nicht mein Tag." };
            return new G1C_NpcCharacter("Dealer Ben", "dealer_green", p, 800, 45, 30);
        }

        // Reicher Pokerprofi: arrogant, vermögend, selbstbewusst, ehrgeizig (möglicher Rivale)
        private static G1C_NpcCharacter Max()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Pokerprofi",
                Traits = "arrogant, vermögend, ehrgeizig, nachtragend",
                Generosity = 0.35, Pride = 0.85, RiskAppetite = 0.8,
                Competitive = true,
                BaseMood = G1C_NpcMood.Happy,
                FirstMeeting = "Noch ein Neuer. Weißt du überhaupt, wer ich bin?",
                GeneralInfo = "Ich habe mehr Turniere gewonnen, als du Chips besitzt.",
                PersonalStory = "Mein erstes Turnier habe ich mit geliehenem Geld gespielt. Erzähl das niemandem.",
                GeneralMonologues = new[] { "Heute verdopple ich mein Vermögen!", "Wer traut sich an meinen Tisch?", "Poker ist kein Glücksspiel." },
                FriendMonologues = new[] { "Der Kleine ist gar nicht übel." },
                RivalMonologues = new[] { "Der hat doch nur Glück!", "Nächstes Mal gehört er mir.", "Revanche, und zwar bald." },
                EnemyMonologues = new[] { "Mit dem rede ich nicht mehr.", "Der soll bloß verschwinden." }
            };
            p.Greetings[G1C_RelationshipStatus.Acquaintance] = "Ach, du schon wieder.";
            p.Greetings[G1C_RelationshipStatus.Friend] = "Na, mein Lieblingsgegner. Setz dich.";
            p.Greetings[G1C_RelationshipStatus.Rival] = "Na, bereit für die nächste Niederlage?";
            p.Greetings[G1C_RelationshipStatus.RespectedRival] = "Du bist besser geworden. Das muss ich zugeben.";
            p.Greetings[G1C_RelationshipStatus.Opponent] = "Was willst du schon wieder?";
            p.Greetings[G1C_RelationshipStatus.ArchEnemy] = "Du hast Nerven, mich noch anzusprechen.";
            p.MoodMonologues[G1C_NpcMood.Angry] = new[] { "Jetzt reicht's! Revanche!", "Das war pures Glück." };
            p.MoodMonologues[G1C_NpcMood.Happy] = new[] { "Ich bin heute unschlagbar." };
            return new G1C_NpcCharacter("Pokerprofi Max", "dealer_blue", p, 20000, 30, 15);
        }

        // Barkeeper: hilfsbereit, großzügig, vergibt einen risikofreien Job
        private static G1C_NpcCharacter Toni()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Barkeeper",
                Traits = "hilfsbereit, gelassen, großzügig",
                Generosity = 0.6, Pride = 0.2, RiskAppetite = 0.1,
                FirstMeeting = "Willkommen an der Bar! Ich bin Toni.",
                GeneralInfo = "Wenn dir die Chips ausgehen: Ich zahle für ehrliche Arbeit.",
                PersonalStory = "Ich hatte mal ein eigenes Casino. Jetzt spüle ich hier Gläser – und schlafe besser.",
                GeneralMonologues = new[] { "Noch jemand Durst?", "Die Gläser spülen sich nicht allein.", "Ein ruhiger Abend heute." },
                FriendMonologues = new[] { "Schön, ihn wiederzusehen!", "Für ihn geht's aufs Haus." },
                EnemyMonologues = new[] { "Dem schenke ich nichts aus." }
            };
            p.Greetings[G1C_RelationshipStatus.Acquaintance] = "Na, wie läuft's? Was darf's sein?";
            p.Greetings[G1C_RelationshipStatus.Friend] = "Hey, schön dich wiederzusehen! Das Übliche?";
            p.MoodMonologues[G1C_NpcMood.Happy] = new[] { "Heute klingelt die Trinkgeldkasse." };
            return new G1C_NpcCharacter("Barkeeper Toni", "bartender", p, 1500, 50, 30);
        }

        // Arme, nervöse Spielerin: emotional, freundlich, kaum Geld
        private static G1C_NpcCharacter Lena()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Stammgast",
                Traits = "freundlich, emotional, verzweifelt",
                Generosity = 0.8, Pride = 0.2, RiskAppetite = 0.9,
                BaseMood = G1C_NpcMood.Nervous,
                HasPauseTask = true,
                FirstMeeting = "Oh, hallo. Entschuldige, ich bin etwas durcheinander.",
                GeneralInfo = "Beim Blackjack halte ich ab 17. Und wenn Max erhöht, hat er meistens etwas.",
                PersonalStory = "Eigentlich wollte ich nur einmal spielen. Das war vor drei Wochen.",
                GeneralMonologues = new[] { "Nur noch eine Runde ...", "Das waren meine letzten Chips ...", "Heute gewinne ich bestimmt." },
                FriendMonologues = new[] { "Schön, ihn wiederzusehen!", "Er hat mir echt geholfen." },
                EnemyMonologues = new[] { "Mit dem rede ich nicht mehr." }
            };
            p.Greetings[G1C_RelationshipStatus.Friend] = "Hey, schön dich wiederzusehen!";
            p.MoodMonologues[G1C_NpcMood.Happy] = new[] { "Mir geht's viel besser!", "Pause tut richtig gut." };
            p.MoodMonologues[G1C_NpcMood.Nervous] = new[] { "Wo sind nur meine Chips?", "Gleich habe ich es zurück." };
            return new G1C_NpcCharacter("Gast Lena", "guest", p, 120, 45, 20);
        }
    }
}
