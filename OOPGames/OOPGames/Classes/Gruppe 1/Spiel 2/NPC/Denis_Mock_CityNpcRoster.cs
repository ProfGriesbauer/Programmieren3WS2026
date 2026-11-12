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
                }, 80, 50, 25)
            };
        }
    }
}
