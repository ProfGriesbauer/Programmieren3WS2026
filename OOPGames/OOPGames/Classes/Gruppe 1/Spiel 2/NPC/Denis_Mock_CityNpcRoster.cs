using System.Collections.Generic;

namespace OOPGames
{
    // Entwickler: Denis Mock
    // Gesprächsprofile zusätzlicher Stadtbewohner; vorhandene Casino-Profile werden wiederverwendet.
    public static class G1C_CityNpcRoster
    {
        // Entwickler: Denis Mock
        public static List<G1C_NpcCharacter> Create()
        {
            return new List<G1C_NpcCharacter>
            {
                new G1C_NpcCharacter("Kurier Paul", "dealer_green", new G1C_NpcPersonality
                {
                    Role = "Kurier",
                    Traits = "lebhaft, hilfsbereit",
                    Generosity = 0.5,
                    Pride = 0.3,
                    RiskAppetite = 0.4,
                    FirstMeeting = "Vorsicht, ich bin auf Lieferfahrt! Ich bin Paul.",
                    GeneralInfo = "Die Hauptstraße führt direkt zum Casino.",
                    PersonalStory = "Ich fahre jeden Tag alle Wege der Stadt ab.",
                    GeneralMonologues = new[] { "Noch eine Lieferung, dann ist Pause.", "Die Wege sind heute frei." }
                }, 90, 45, 25),
                new G1C_NpcCharacter("Gärtner Sam", "bartender", new G1C_NpcPersonality
                {
                    Role = "Gärtner",
                    Traits = "gelassen, naturverbunden",
                    Generosity = 0.7,
                    Pride = 0.2,
                    RiskAppetite = 0.1,
                    FirstMeeting = "Schönes Wetter für einen Spaziergang, findest du nicht?",
                    GeneralInfo = "Die Bäume am Stadtrand spenden an heißen Tagen Schatten.",
                    PersonalStory = "Ich kümmere mich um die Grünflächen zwischen den Häusern.",
                    GeneralMonologues = new[] { "Die Blumen brauchen heute Wasser.", "In der Stadt ist es erstaunlich grün." }
                }, 55, 55, 30),
                new G1C_NpcCharacter("Studentin Lea", "dealer_blue", new G1C_NpcPersonality
                {
                    Role = "Studentin",
                    Traits = "neugierig, ruhig",
                    Generosity = 0.4,
                    Pride = 0.4,
                    RiskAppetite = 0.3,
                    FirstMeeting = "Hi! Ich mache gerade eine Pause vom Lernen.",
                    GeneralInfo = "Vom südlichen Weg kommst du schnell wieder auf die Hauptstraße.",
                    PersonalStory = "Ich studiere Stadtplanung und zeichne gern Karten.",
                    GeneralMonologues = new[] { "Ein kurzer Spaziergang hilft beim Nachdenken.", "Ich sollte später noch in die Bibliothek." }
                }, 80, 50, 25),
                CreateAngler(),
                new G1C_NpcCharacter("Geldhai Viktor", "dealer_red", new G1C_NpcPersonality
                {
                    Role = "Kreditgeber",
                    Traits = "aufdringlich, geschäftstüchtig",
                    Generosity = 0.1,
                    Pride = 0.7,
                    RiskAppetite = 0.9,
                    FirstMeeting = "Psst. Du brauchst Chips? Ich kann dir sofort welche leihen.",
                    GeneralInfo = "Ich verleihe bis zu 5000 Chips. Jeden Tag wachsen offene Schulden um 50 Prozent.",
                    PersonalStory = "Ich kenne jeden, der hier auf Pump spielt. Zahlen ist besser als Weglaufen.",
                    GeneralMonologues = new[] { "Ein Kredit ist kein Geschenk.", "Ich bin gleich wieder an der Bank." }
                }, 35, 45, 600),
                new G1C_NpcCharacter("Bankierin Frida", "dealer_blue", new G1C_NpcPersonality
                {
                    Role = "Bankierin",
                    Traits = "sorgfältig, geduldig",
                    Generosity = 0.4,
                    Pride = 0.3,
                    RiskAppetite = 0.1,
                    FirstMeeting = "Willkommen. Ich bin Frida und helfe dir gern mit deinem Konto.",
                    GeneralInfo = "Einzahlungen bleiben sicher auf deinem Konto und sind jederzeit verfügbar.",
                    PersonalStory = "Ich prüfe jede Buchung sorgfältig, damit dein Kontostand stimmt.",
                    GeneralMonologues = new[] { "Ein guter Überblick hilft beim Sparen.", "Alle Buchungen sind ordentlich erfasst." }
                }, 60, 65, 0),
                new G1C_NpcCharacter("Möbelhändlerin Lotte", "guest", new G1C_NpcPersonality
                {
                    Role = "Möbelhändlerin",
                    Traits = "kreativ, freundlich",
                    Generosity = 0.6,
                    Pride = 0.4,
                    RiskAppetite = 0.2,
                    FirstMeeting = "Hallo! Suchst du etwas Schönes für dein Zuhause?",
                    GeneralInfo = "Pflanzen, Stühle und größere Möbel passen in verschiedene freie Bereiche.",
                    PersonalStory = "Ich richte kleine Räume so ein, dass sie trotzdem gemütlich wirken.",
                    GeneralMonologues = new[] { "Heute ist ein guter Tag zum Umdekorieren.", "Ein Zimmer wirkt mit Pflanzen gleich freundlicher." }
                }, 65, 60, 0)
            };
        }

        // Änderung durch: Patrick Lupierz – Datum: 19.11.2026 – Angler-Tutorial aus vorhandenen Fangdaten
        private static G1C_NpcCharacter CreateAngler()
        {
            G1C_NpcPersonality p = new G1C_NpcPersonality
            {
                Role = "Angler",
                Traits = "geduldig, sachkundig",
                Generosity = 0.6,
                Pride = 0.3,
                RiskAppetite = 0.2,
                FirstMeeting = "Ich kenne jeden guten Angelplatz. Dieser hier ist einer davon.",
                GeneralInfo = "Am Brückengeländer steht eine Angel.",
                PersonalStory = "Ich beobachte die Pose, bevor ich selbst die Schnur werfe.",
                GeneralMonologues = new[] { "Warte auf das Zucken.", "Nicht zu früh anhauen." }
            };
            p.Topics.Add(new G1C_DialogueTopic("Wie funktioniert das Angelspiel?", context =>
                "[E] wirft die Angel aus. Die Pose treibt, und der Fisch beißt nach kurzer Wartezeit.\n" +
                "Bei Zucken schnell [E] drücken. Mit [A]/[D] haeltst du den Fisch im grünen Bereich. " +
                "[Q] beendet das Minispiel. Zu frueh beissen lassen bedeutet: nichts angebissen; aus der Fangleiste heraus kommt der Fisch frei.", G1C_NpcExpression.Thoughtful));
            p.Topics.Add(new G1C_DialogueTopic("Welche Fische gibt es?", context =>
            {
                System.Text.StringBuilder text = new System.Text.StringBuilder();
                foreach (G1C_CatchInfo catchInfo in G1C_FishingGame.Catches)
                {
                    if (!catchInfo.IsFish) continue;
                    G1C_Item item = G1C_ItemCatalog.Get(catchInfo.ItemId);
                    if (item == null) continue;
                    string rarity = catchInfo.Weight <= 2 ? "sehr selten" : catchInfo.Weight <= 4 ? "selten" : catchInfo.Weight <= 7 ? "ungewoehnlich" : "haeufig";
                    string property = "Tempo " + catchInfo.Speed.ToString("0.0") + ", Sprunghaftigkeit " + catchInfo.Erratic.ToString("0.0");
                    text.Append(item.Name).Append(" | ").Append(rarity).Append(" | ").Append(item.SellValue).Append(" Chips | ")
                        .Append(property).Append(" | Verkauf in jedem Laden\n");
                }
                return text.ToString();
            }, G1C_NpcExpression.Thoughtful));
            p.Topics.Add(new G1C_DialogueTopic("Wofuer sind die Fische?", context =>
                "Gefangene Fische werden ins Inventar gelegt und koennen in jedem Laden verkauft werden.\n" +
                "Der verkaufte Wert steht in der Fischliste. Weitere Quests oder Rezepte koennen spaeter dieselben Fischdaten verwenden.", G1C_NpcExpression.Thoughtful));
            return new G1C_NpcCharacter("Angler Fabian", "dealer_green", p, 120, 55, 40);
        }
    }
}
