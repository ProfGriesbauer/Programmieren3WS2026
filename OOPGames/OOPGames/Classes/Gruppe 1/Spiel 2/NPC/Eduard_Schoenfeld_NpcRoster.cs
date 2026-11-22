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
            return new List<G1C_NpcCharacter> { Rosa(), Ben(), Max(), Toni(), Lena(), Eduard() };
        }

        // Änderung durch: Denis Mock – Datum: 19.11.2026 – unzuverlässiger, spielsüchtiger Automatenspieler
        private static G1C_NpcCharacter Eduard()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Spielsüchtiger Stammgast",
                Traits = "großspurig, verspätet, unzuverlässig, nervig",
                Generosity = 0.1,
                Pride = 0.9,
                RiskAppetite = 1.0,
                BaseMood = G1C_NpcMood.Nervous,
                FirstMeeting = "Ich bin Eduard. Ich war pünktlich. Die Slotmaschine hat behauptet, vorher sei meine Runde.",
                GeneralInfo = "Eduard steht an den Spielautomaten, hält sich aber nie zuverlässig an Termine.",
                PersonalStory = "Ich habe fast immer schon gewonnen. Gestern war eine Ausnahme. Und vorgestern.",
                GeneralMonologues = new[]
                {
                    "Gleich gewinnt garantiert. Außer das Ding spinnt.",
                    "Ich wäre ja pünktlich gewesen, aber die Walzen waren wichtiger.",
                    "Ich habe ein System. Das funktioniert immer. Fast immer.",
                    "Die anderen sagen, ich sei unbeliebt. Das stimmt nicht. Die mögen mich nur nicht.",
                    // Änderung durch: Patrick Lupierz – Datum: 22.11.2026 – mehr Sprüche am Automaten
                    "*klack* *klack* ... Kirsche, Zitrone, Glocke. Knapp!",
                    "Der linke Automat ist meiner. Den hab ich warmgespielt.",
                    "Noch zehn Chips. Das reicht für mindestens drei Jackpots.",
                    "Hörst du das? So klingt ein Automat kurz vorm Auszahlen.",
                    "Zwei Siebenen! Die dritte war nur schüchtern.",
                    "Ich spiel nicht aus Spaß. Ich spiel aus Überzeugung.",
                    "Wer aufhört, verpasst den Jackpot. Das ist Mathematik.",
                    "Ich drück einfach fester. Das hilft. Glaub ich.",
                    "Meine Glückssocken sind in der Wäsche. Das erklärt einiges.",
                    "Heute Nacht schlafe ich hier. Der Automat braucht mich.",
                    "Nicht drängeln, ich bin gleich fertig. Seit drei Stunden.",
                    "Rosa sagt, ich soll mal Pause machen. Rosa hat keine Ahnung.",
                    "BAR, BAR, ... Banane? Ach nein, Zitrone.",
                    "Ich leih dir nichts. Ich brauch alles für die nächste Runde."
                }
            };
            p.MoodMonologues[G1C_NpcMood.Happy] = new[] { "Heute zahlt der Automat bestimmt alles zurück!",
                "JAAA! ... ach, nur zwei Kirschen.", "Siehst du? Mein System funktioniert!" };
            p.MoodMonologues[G1C_NpcMood.Nervous] = new[] { "Nur noch eine Runde. Dann wirklich Schluss.",
                "Nur noch eine. Und dann noch eine.", "Wo sind meine Chips hin? Die waren eben noch da." };
            p.MoodMonologues[G1C_NpcMood.Angry] = new[] { "Das war kein Verlust. Das war technisches Pech!",
                "Dieser Automat ist manipuliert! ... Ich spiel trotzdem weiter.", "Hör auf, mir über die Schulter zu schauen!" };
            p.Topics.Add(new G1C_DialogueTopic("Wie läuft dein Gewinnsystem?", G1C_DialogueTopic.Cycle(
                "Ich setze immer denselben Betrag. Das ist Wissenschaft.",
                "Gestern hatte ich fast einen Jackpot. Der Automat hat sich nur vertan.",
                "Heute ganz sicher. Außer jemand hat die Walzen blockiert."),
                G1C_NpcExpression.Thoughtful));
            p.Topics.Add(new G1C_DialogueTopic("Warum bist du so spät?", G1C_DialogueTopic.Cycle(
                "Ich habe den Termin nicht vergessen. Ich habe ihn verschoben.",
                "Die Slotmaschine wollte noch eine Runde. Ich konnte nicht einfach gehen.",
                "Pünktlichkeit ist für Leute, die keine angeblich garantierten Gewinnchancen haben."),
                G1C_NpcExpression.Surprised));
            p.Topics.Add(new G1C_DialogueTopic("Wie viele Chips hast du verloren?", G1C_DialogueTopic.Cycle(
                "Das zählt nicht. Ich habe investiert.",
                "Viele. Sehr viele. Aber der nächste Gewinn gleicht das bestimmt aus."),
                G1C_NpcExpression.Neutral));
            // Änderung durch: Patrick Lupierz – Datum: 22.11.2026 – eigenes Aussehen (gestreiftes Hemd, schwarze Mütze)
            return new G1C_NpcCharacter("Eduard", G1C_CasinoWorld.EduardSkin, p, 220, 25, 10);
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
