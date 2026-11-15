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
            return new List<G1C_NpcCharacter> { Jana(), Olli(), Mia() };
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
