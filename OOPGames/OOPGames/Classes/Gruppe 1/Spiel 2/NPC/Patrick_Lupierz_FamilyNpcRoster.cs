using System;
using System.Collections.Generic;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_FamilyNpcRoster
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Mutter und Oma des Spielers (reine Daten + individuelle Gesprächsthemen).
    //               Beide sind von Anfang an im Handy gespeichert. Das Aussehen entsteht aus der
    //               eingebauten Figur (pc-Skin), Porträts daher automatisch passend.
    //               Die Oma ist eine eigene Figur: trocken, sarkastisch, alt – frei erfunden.
    // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Änderung: Familie wird nie Gegner (IsFamily).
    //               Taschengeld: jede gibt einmal pro Spieltag (Mama 25, Oma 20 Chips), danach einfach Nein.
    //               Die frühere Zufalls-Regel ("mit beiden gesprochen, 60 %") wurde ersetzt.
    // =====================================================
    public static class G1C_FamilyNpcRoster
    {
        public const string MotherName = "Mama";
        public const string GrandmaName = "Oma Gerda";
        // pc-Haar-Haut-Oberteil-Hose-Schuhe-Mütze
        public const string MotherSkin = "pc-6A3A1C-F0C8A0-D86890-404868-803030-x";
        public const string GrandmaSkin = "pc-E8E8E8-E8C0A0-7050A0-505058-302020-x";

        // Entwickler: Patrick Lupierz
        public static List<G1C_NpcCharacter> Create()
        {
            List<G1C_NpcCharacter> family = new List<G1C_NpcCharacter> { Mother(), Grandma() };
            foreach (G1C_NpcCharacter member in family) member.Memory.PhoneNumberGiven = true;
            return family;
        }

        private static G1C_NpcCharacter Mother()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Mutter",
                Traits = "fürsorglich, ordentlich, ein bisschen besorgt",
                Generosity = 0.6, Pride = 0.2, RiskAppetite = 0.1,
                BaseMood = G1C_NpcMood.Happy,
                IsFamily = true,
                PocketMoney = 25,
                PocketMoneyLine = "Na gut, hier sind 25 Chips Taschengeld. Nicht alles am Automaten verspielen!",
                MoneyRefusal = new[]
                {
                    "Nein, mein Schatz, du hast heute schon was bekommen. Geld wächst nicht auf Bäumen.",
                    "Nein. Und guck nicht so, das zieht bei mir nicht mehr.",
                    "Immer noch nein. Aber es gibt Abendessen, das ist doch auch was."
                },
                FirstMeeting = "Na, ausgeschlafen? Frühstück steht auf dem Tisch.",
                GeneralInfo = "Denk dran: Die Läden haben Öffnungszeiten, nur die Tankstelle hat immer offen.",
                PersonalStory = "Als ich so alt war wie du, war hier noch kein Casino. Nur ein Teich mit sehr frechen Enten.",
                GeneralMonologues = new[] { "Wer hat schon wieder die Milch leer getrunken?", "Räum bitte dein Zimmer auf." }
            };
            p.Greetings[G1C_RelationshipStatus.Friend] = "Da ist ja mein Liebling!";
            p.Greetings[G1C_RelationshipStatus.GoodFriend] = "Da ist ja mein Liebling! Hast du was gegessen?";

            p.Topics.Add(new G1C_DialogueTopic("Wie geht's dir, Mama?", G1C_DialogueTopic.Cycle(
                "Gut, mein Schatz! Ich habe Wäsche gemacht. Deine Socken sind übrigens immer noch einzeln.",
                "Immer noch gut. Du fragst heute auffällig oft – brauchst du etwas?",
                "Ich kenne dich. Raus damit, was willst du?"), G1C_NpcExpression.Happy));
            p.Topics.Add(new G1C_DialogueTopic("Was hältst du vom Casino?", c =>
                c.Money != null && c.Money.Balance >= 1500
                    ? "Du hast ja gewonnen! Bring die Hälfte zur Bank, die zahlt 2 % pro Tag. Das Casino zahlt gar nichts zurück."
                    : c.Money != null && c.Money.Balance < 200
                        ? "Oh je ... Spiel nie mit Chips, die du nicht hast. Und vom Geldhai hältst du dich fern!"
                        : "Ein bisschen Spaß ist in Ordnung. Aber das Haus gewinnt immer.", G1C_NpcExpression.Thoughtful));
            p.Topics.Add(new G1C_DialogueTopic("Was soll ich heute machen?", G1C_DialogueTopic.ByTime(
                "Erst frühstücken! Der Supermarkt öffnet um 7 Uhr, falls du etwas brauchst.",
                "Geh raus an die frische Luft. Nimm dein Fahrrad, es steht vor der Tür. Und schau bei Oma vorbei!",
                "Die Läden schließen bald. Die Tankstelle hat aber die ganze Nacht offen.",
                "Es ist mitten in der Nacht! Ab ins Bett mit dir.")));
            p.Topics.Add(new G1C_DialogueTopic("Erzähl mir von Oma.", G1C_DialogueTopic.Cycle(
                "Deine Oma hat drei Katzen und mehr Meinungen als die ganze Stadt zusammen.",
                "Sie behauptet, sie hätte früher fast das Casino ruiniert. Ich glaube ihr kein Wort. Fast keins."),
                G1C_NpcExpression.Thoughtful));
            return new G1C_NpcCharacter(MotherName, MotherSkin, p, 200, 85, 50);
        }

        private static G1C_NpcCharacter Grandma()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Oma",
                Traits = "trocken, sarkastisch, schlagfertig, heimlich herzlich",
                Generosity = 0.5, Pride = 0.7, RiskAppetite = 0.7,
                BaseMood = G1C_NpcMood.Neutral,
                IsFamily = true,
                PocketMoney = 20,
                PocketMoneyLine = "Hier, 20 Chips. Sag deiner Mutter nichts. Und kauf dir was Vernünftiges. Oder Schokolade.",
                MoneyRefusal = new[]
                {
                    "Nein. Du hast heute schon was bekommen. Ich bin Rentnerin, keine Bank., keine Bank. Die Bank ist da drüben.",
                    "Nein. Frag den Geldhai, der freut sich. Ich nicht.",
                    "Kind. Die Antwort ist Nein. Sie war Nein und sie bleibt Nein. Wie mein Rücken."
                },
                FirstMeeting = "Ach, du lebst noch. Schön. Steh nicht so rum, die Katzen gucken schon.",
                GeneralInfo = "Ich gehe nicht in die Stadt. Die Stadt kommt zu mir. Meistens mit Problemen.",
                PersonalStory = "Ich hatte mal einen Führerschein. Dann hatte ich keinen mehr. Die Laterne an der Ecke weiß, warum.",
                GeneralMonologues = new[] { "Früher war mehr Lametta.", "Wenn ich noch einmal 'Oma, wie geht das Handy' höre ..." }
            };
            p.Greetings[G1C_RelationshipStatus.Friend] = "Na, Kind. Hast du mir was mitgebracht? Nein? Typisch.";
            p.Greetings[G1C_RelationshipStatus.GoodFriend] = "Mein Lieblingsenkel. Sag's nicht den anderen. Es gibt keine anderen.";

            p.Topics.Add(new G1C_DialogueTopic("Hallo Oma, wie geht's?", G1C_DialogueTopic.Cycle(
                "Prächtig. Meine Hüfte ist aus Titan, meine Geduld aus Papier.",
                "Immer noch gut. Frag nicht dauernd, sonst denke ich, du willst erben.",
                "Kind. Wir hatten das Thema."), G1C_NpcExpression.Happy));
            p.Topics.Add(new G1C_DialogueTopic("Was hältst du vom Casino?", G1C_DialogueTopic.Cycle(
                "Ich habe dort 1983 Hausverbot bekommen. Angeblich darf man beim Blackjack nicht laut mitzählen.",
                "Mein Tipp: Beim Blackjack bei 17 aufhören. Im Leben nicht."), G1C_NpcExpression.Thoughtful));
            p.Topics.Add(new G1C_DialogueTopic("Wie heißen deine Katzen?", G1C_DialogueTopic.Cycle(
                "Schnurrbert, Mausi und Herr Doktor Krallmann. Der Doktor hat keinen Abschluss, aber Selbstbewusstsein.",
                "Immer noch Schnurrbert, Mausi und der Doktor. Die heißen nicht jeden Tag anders, Kind."),
                G1C_NpcExpression.Happy));
            p.Topics.Add(new G1C_DialogueTopic("Hast du einen Tipp für mich?", c =>
            {
                if (c.Hour >= 22 || c.Hour < 6) return "Um diese Uhrzeit? Schlafen. Oder Tankstelle. Die hat immer offen. So wie ich früher.";
                string[] tips =
                {
                    "Die Bank zahlt 2 % am Tag. Das Casino zahlt Tränen.",
                    "In der Arcade gibt's ein Gerät mit Spielen von anderen Leuten. Ich hab da den Rekord. Behaupte ich.",
                    "Der Geldhai vor der Bank schuldet mir noch 20 Mark. Sag ihm, ich hab's nicht vergessen."
                };
                return tips[c.TimesAsked % tips.Length];
            }, G1C_NpcExpression.Thoughtful));
            p.Topics.Add(new G1C_DialogueTopic("Du siehst heute gut aus, Oma.", G1C_DialogueTopic.Cycle(
                "Ich weiß. Danke trotzdem. Du übrigens nicht – schlaf mal mehr.",
                "Zweimal am Tag Komplimente? Was hast du angestellt?"), G1C_NpcExpression.Surprised));
            return new G1C_NpcCharacter(GrandmaName, GrandmaSkin, p, 300, 75, 40);
        }
    }
}
