using System;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_TopicContext
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Spielzustand, auf den eine Gesprächsantwort reagieren darf (Name, Tag, Uhrzeit,
    //               Kontostand, wie oft die Frage schon gestellt wurde). Reine Daten, keine Darstellung.
    // =====================================================
    public sealed class G1C_TopicContext
    {
        public string PlayerName { get; set; } = "";
        public int Day { get; set; } = 1;
        public int Hour { get; set; } = 12;
        public G1C_MoneyManager Money { get; set; }
        public Random Random { get; set; } = new Random();
        public bool ViaPhone { get; set; }
        public int TimesAsked { get; set; }
    }

    // =====================================================
    // Klasse: G1C_DialogueTopic
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Individuelle Frage eines NPCs mit eigener Antwort. Die Antwort ist eine Funktion,
    //               damit sie sich nach Wiederholung, Tageszeit oder Spielfortschritt ändern kann.
    //               Wird von G1C_Conversation als Antwortoption angeboten (Personality.Topics).
    // =====================================================
    public sealed class G1C_DialogueTopic
    {
        private readonly Func<G1C_TopicContext, string> _answer;

        public G1C_DialogueTopic(string question, Func<G1C_TopicContext, string> answer,
                                 G1C_NpcExpression expression = G1C_NpcExpression.Neutral)
        {
            if (string.IsNullOrEmpty(question)) throw new ArgumentException("Frage fehlt.", nameof(question));
            Question = question;
            _answer = answer ?? throw new ArgumentNullException(nameof(answer));
            Expression = expression;
        }

        public string Question { get; }
        public G1C_NpcExpression Expression { get; }
        public int TimesAsked { get; private set; }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Antwort für den aktuellen Spielzustand bilden und die Wiederholung zählen.
        public string Ask(G1C_TopicContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            context.TimesAsked = TimesAsked;
            TimesAsked++;
            return _answer(context) ?? "";
        }

        // Entwickler: Patrick Lupierz – Antworten der Reihe nach; die letzte bleibt bei weiteren Wiederholungen.
        public static Func<G1C_TopicContext, string> Cycle(params string[] answers)
        {
            if (answers == null || answers.Length == 0) throw new ArgumentException("Antworten fehlen.", nameof(answers));
            return context => answers[Math.Min(context.TimesAsked, answers.Length - 1)];
        }

        // Entwickler: Patrick Lupierz – Antwort abhängig von der Tageszeit (Morgen, Tag, Abend, Nacht).
        public static Func<G1C_TopicContext, string> ByTime(string morning, string day, string evening, string night)
        {
            return context =>
            {
                if (context.Hour >= 6 && context.Hour < 10) return morning;
                if (context.Hour >= 10 && context.Hour < 18) return day;
                if (context.Hour >= 18 && context.Hour < 22) return evening;
                return night;
            };
        }
    }
}
