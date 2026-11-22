using System.Collections.Generic;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_ShopNpcRoster
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Gesprächsprofile der Verkäufer (Supermarkt, Tankstelle, Bekleidung). Bisher öffneten sie
    //               nur die Warenliste; jetzt haben sie eigene Persönlichkeit und Fragen, z. B. zu ihren
    //               Öffnungszeiten (aus G1C_Shop, damit Text und Logik nie auseinanderlaufen).
    //               Namen und Grafiken stimmen mit G1C_BuildingInteriorWorld.AddSeller überein.
    // =====================================================
    public static class G1C_ShopNpcRoster
    {
        // Entwickler: Patrick Lupierz
        public static List<G1C_NpcCharacter> Create()
        {
            return new List<G1C_NpcCharacter> { Jana(), Olli(), Mia(), Karl(), Bodo(), Rudi(), Paula(),
                                                G1C_ArcadeMentor.Create() }; // Patrick Lupierz – 22.11.2026: Patrick in der Spielhalle
        }

        // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Bankberater, Wachmann, Radhändler, Spielhallen-Aufsicht.
        // Skins entsprechen den Weltfiguren in G1C_BuildingInteriorWorld (BuildBank, BuildBikeShop, BuildArcade).
        public const string KarlSkin = "pc-303030-F0C8A0-2C3C68-282830-181818-x";
        public const string BodoSkin = "pc-201810-C89068-384858-282838-101010-202838";
        public const string RudiSkin = "pc-A06030-E8B890-38A050-404858-603020-F0C030";
        public const string PaulaSkin = "pc-F860D0-F0C8A0-282838-383848-F8F8F8-x";

        private static G1C_NpcCharacter Karl()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Bankberater", Traits = "korrekt, zahlenverliebt, trocken",
                Generosity = 0.1, Pride = 0.5, RiskAppetite = 0.0, RefusesMoneyByRule = true,
                FirstMeeting = "Guten Tag. Karl, Anlageberatung. Ich berate gern. Ich lache selten.",
                GeneralInfo = "Zinseszins ist das achte Weltwunder. Die anderen sieben verzinsen sich nicht.",
                PersonalStory = "Ich habe mein erstes Sparbuch mit sechs eröffnet. Mit acht hatte ich ein Portfolio."
            };
            p.Topics.Add(new G1C_DialogueTopic("Wie viel bringen die Zinsen?", c =>
                "Jeden Spieltag " + G1C_FinanceManager.InterestPercentPerDay + " % auf dein Guthaben, abgerundet auf ganze Chips. " +
                "Aus 1000 werden nach einem Tag 1020, danach rechnen wir mit 1020 weiter.", G1C_NpcExpression.Thoughtful));
            p.Topics.Add(new G1C_DialogueTopic("Lohnt sich das Casino?", G1C_DialogueTopic.Cycle(
                "Rechnerisch nie. Beim Roulette verlieren Sie im Schnitt 2,7 % pro Einsatz. Bei uns gewinnen Sie 2 % pro Tag.",
                "Ich wiederhole mich ungern: nein.")));
            p.Topics.Add(new G1C_DialogueTopic("Was ist mit dem Geldhai draußen?", G1C_DialogueTopic.Cycle(
                "Er verleiht bis zu 5000 Chips. Offene Schulden wachsen um 50 % pro Tag.",
                "Wir haben ihn mehrfach gebeten, nicht vor unserer Tür zu stehen. Er steht trotzdem da.")));
            return new G1C_NpcCharacter("Bankberater Karl", KarlSkin, p, 500, 45, 40);
        }

        private static G1C_NpcCharacter Bodo()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Wachmann", Traits = "wortkarg, aufmerksam, heimlicher Katzenfan",
                Generosity = 0.2, Pride = 0.4, RiskAppetite = 0.1, RefusesMoneyByRule = true,
                FirstMeeting = "Hm. Willkommen in der Bank. Keine Mützen. Ach, meine zählt nicht.",
                GeneralInfo = "Der Tresor ist sicher. Fragen Sie nicht, wie sicher.",
                PersonalStory = "Ich wollte eigentlich Tierpfleger werden. Jetzt bewache ich Geld. Geld miaut nicht."
            };
            p.Topics.Add(new G1C_DialogueTopic("Ist hier schon mal eingebrochen worden?", G1C_DialogueTopic.Cycle(
                "Einmal. Ein Waschbär. Er kam nicht weit.", "Der Waschbär hat Hausverbot. Wie alle anderen auch.")));
            p.Topics.Add(new G1C_DialogueTopic("Wie ist die Nachtschicht?", G1C_DialogueTopic.ByTime(
                "Ich habe gerade Feierabend gehabt. Sieht man mir an, oder?",
                "Tagsüber ist es ruhig. Nachts auch. Ich mag ruhig.",
                "Bald wird es dunkel. Dann zähle ich Laternen.",
                "Psst. Nachts flüstern wir hier.")));
            return new G1C_NpcCharacter("Wachmann Bodo", BodoSkin, p, 60, 40, 45);
        }

        private static G1C_NpcCharacter Rudi()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Radhändler", Traits = "begeistert, sportlich, redet gern über Gangschaltungen",
                Generosity = 0.4, Pride = 0.4, RiskAppetite = 0.5,
                FirstMeeting = "Servus! Rudi mein Name, Räder mein Spiel. Das große Rad auf dem Dach? Hab ich selbst raufgetragen.",
                GeneralInfo = "Mit dem Rad bist du fast doppelt so schnell. Nur draußen, drinnen bitte schieben.",
                PersonalStory = "Ich bin mal mit dem Rad bis ans Meer gefahren. Dann ist mir eingefallen, dass wir keins haben."
            };
            p.Topics.Add(new G1C_DialogueTopic("Wann habt ihr offen?", c => Hours(G1C_WorldLocation.BikeShop)));
            p.Topics.Add(new G1C_DialogueTopic("Welches Rad empfiehlst du?", c =>
                c.Money != null && c.Money.Balance >= 500
                    ? "Das schwarze Rennrad! Leicht, schnell, sieht aus wie Mitternacht."
                    : "Blau oder Grün – solide Räder für faire Chips. Dein rotes vor dem Haus tut's aber auch."));
            p.Topics.Add(new G1C_DialogueTopic("Wie fahre ich Fahrrad?", c =>
                "Im Inventar [I] dein Rad wählen und [E] drücken – oder am Ständer vor deinem Haus [E]. Absteigen genauso."));
            return new G1C_NpcCharacter("Radhändler Rudi", RudiSkin, p, 120, 45, 35);
        }

        private static G1C_NpcCharacter Paula()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Spielhallen-Aufsicht", Traits = "Highscore-Jägerin, schnell gelangweilt, hilfsbereit",
                Generosity = 0.3, Pride = 0.6, RiskAppetite = 0.6,
                FirstMeeting = "Hi! Paula. Ich passe auf die Automaten auf. Und auf meinen Rekord.",
                GeneralInfo = "Jedes Spiel kostet 5 Chips. Gute Punktzahlen bringen Chips zurück, bis zu 30 pro Runde.",
                PersonalStory = "Ich habe beim Sternenjäger mal Welle 12 erreicht. Seitdem nennen mich alle Captain."
            };
            p.Topics.Add(new G1C_DialogueTopic("Was ist der Gast-Automat?", c =>
                "Der in der Mitte! Da laufen Spiele aus anderen Gruppen, die beim Spielsystem angemeldet sind. Gewinnst du gegen den Computer, gibt's 20 Chips."));
            p.Topics.Add(new G1C_DialogueTopic("Hast du einen Tipp?", G1C_DialogueTopic.Cycle(
                "Sternenjäger: Bleib in Bewegung, die Sturzflieger zielen auf deine alte Position.",
                "Ziegelbrecher: Triff den Ball mit dem Schlägerrand, dann fliegt er schräg.",
                "Pong: Die KI ist langsamer als du. Spiel steil, dann kommt sie nicht hinterher.")));
            p.Topics.Add(new G1C_DialogueTopic("Wo ist der Greifautomat?", c =>
                "Rechts an der Wand. Der ist aus dem Casino zu uns umgezogen – hier passt er auch viel besser hin."));
            return new G1C_NpcCharacter("Aufsicht Paula", PaulaSkin, p, 80, 45, 35);
        }

        private static string Hours(G1C_WorldLocation location)
        {
            G1C_Shop shop = G1C_Shop.For(location);
            return shop.OpenUntil >= 24 && shop.OpenFrom == 0
                ? "Wir haben rund um die Uhr geöffnet."
                : "Wir haben von " + shop.OpenFrom + " bis " + shop.OpenUntil + " Uhr geöffnet.";
        }

        private static G1C_NpcCharacter Jana()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Kassiererin", Traits = "flink, freundlich, leicht gestresst",
                Generosity = 0.3, Pride = 0.3, RiskAppetite = 0.2,
                FirstMeeting = "Hallo! Brauchst du eine Tüte? Die kostet nichts, ist aber auch keine gute.",
                GeneralInfo = "Äpfel sind heute frisch. Gestern waren sie auch frisch. Vorgestern ... reden wir nicht drüber.",
                PersonalStory = "Ich spare auf eine Weltreise. Bisher reicht es bis zur Tankstelle."
            };
            p.Topics.Add(new G1C_DialogueTopic("Wann habt ihr offen?", c => Hours(G1C_WorldLocation.Market)));
            p.Topics.Add(new G1C_DialogueTopic("Was empfiehlst du?", G1C_DialogueTopic.ByTime(
                "Morgens? Kaffee. Ganz klar Kaffee.",
                "Ein Sandwich. Macht satt und hält dich vom Automaten fern.",
                "Wasser. Nach einem Abend im Casino braucht man das.",
                "Du solltest nicht wach sein. Ich übrigens auch nicht.")));
            p.Topics.Add(new G1C_DialogueTopic("Wie läuft das Geschäft?", G1C_DialogueTopic.Cycle(
                "Gut! Seit es das Casino gibt, kaufen alle Snacks für die Wartezeit.",
                "Immer noch gut. Du bist heute schon der Dritte, der fragt. Alle anderen warst auch du.")));
            return new G1C_NpcCharacter("Kassiererin Jana", "guest", p, 60, 45, 30);
        }

        private static G1C_NpcCharacter Olli()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Tankwart", Traits = "gemütlich, nachtaktiv, philosophisch",
                Generosity = 0.4, Pride = 0.2, RiskAppetite = 0.3,
                FirstMeeting = "Tach. Getankt wird draußen, gequatscht wird hier.",
                GeneralInfo = "Die Tankstelle schließt nie. Ich auch nicht. Also, die Augen schon manchmal.",
                PersonalStory = "Nachts kommen die interessantesten Leute. Gestern hat mir eine Oma Kaffee mit Wodka empfohlen."
            };
            p.Topics.Add(new G1C_DialogueTopic("Habt ihr wirklich immer offen?", c => Hours(G1C_WorldLocation.GasStation) +
                (c.Hour >= 22 || c.Hour < 6 ? " Gerade sind wir die Einzigen in der Stadt." : "")));
            p.Topics.Add(new G1C_DialogueTopic("Was ist hier beliebt?", G1C_DialogueTopic.ByTime(
                "Kaffee und Brötchen. Die Frühaufsteher sind treu.",
                "Energy-Drinks. Die Casino-Leute schwören drauf.",
                "Sandwiches. Abends hat sonst kaum noch was offen.",
                "Nachts? Alles. Ich frage nicht, warum.")));
            p.Topics.Add(new G1C_DialogueTopic("Langweilig hier, oder?", G1C_DialogueTopic.Cycle(
                "Langweilig? Ich zähle Autos. Heute: null. Rekord ist eins.",
                "Ich überlege, ein Buch zu schreiben. Titel: 'Benzin und Bekenntnisse'.")));
            return new G1C_NpcCharacter("Tankwart Olli", "dealer_green", p, 80, 45, 30);
        }

        private static G1C_NpcCharacter Mia()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Modeverkäuferin", Traits = "stilbewusst, ehrlich, direkt",
                Generosity = 0.3, Pride = 0.6, RiskAppetite = 0.4,
                FirstMeeting = "Willkommen! Oh ... wir arbeiten an deinem Outfit, versprochen.",
                GeneralInfo = "Gekaufte Kleidung ziehst du zu Hause am Kleiderschrank an.",
                PersonalStory = "Ich designe nebenbei Mützen. Die rote Kappe ist von mir. Bitte sag, dass sie dir gefällt."
            };
            p.Topics.Add(new G1C_DialogueTopic("Wann habt ihr offen?", c => Hours(G1C_WorldLocation.ClothingStore)));
            p.Topics.Add(new G1C_DialogueTopic("Was steht mir?", G1C_DialogueTopic.Cycle(
                "Die grüne Kapuzenjacke. Sie sagt: 'Ich habe Pläne.'",
                "Die schwarze Mütze. Sie sagt: 'Ich habe keine Pläne, aber Stil.'",
                "Ehrlich? Alles ist besser als das, was du gerade trägst. Nichts für ungut.")));
            p.Topics.Add(new G1C_DialogueTopic("Gibt es Rabatt?", G1C_DialogueTopic.Cycle(
                "Rabatt? Wir haben einen Aufschlag von 5 %. Nennt sich Qualitätszuschlag.",
                "Nein. Aber ich lächle beim Kassieren. Das ist fast dasselbe.")));
            return new G1C_NpcCharacter("Verkäuferin Mia", "dealer_red", p, 90, 45, 35);
        }
    }
}
