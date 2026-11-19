using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_SmallTalk
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: SprintPlanning "alle Personen haben die Standardfragen 'Das machst du echt gut hier' und
    //               'Du kannst nicht spielen'". Beim Plaudern passen Lob und Spitze jetzt zur Rolle des NPCs
    //               (Croupier, Verkäuferin, Gärtner ...). Familie bekommt keine Beleidigungs-Option (wird nie
    //               zum Gegner). Die Wirkung (Vertrauen, Ruf) bleibt Eduards Compliment/Insult-Logik.
    // =====================================================
    public static class G1C_SmallTalk
    {
        // Entwickler: Patrick Lupierz – Lob passend zur Rolle
        public static string ComplimentFor(G1C_NpcPersonality p)
        {
            switch (p != null ? p.Role : "")
            {
                case "Croupier":
                case "Blackjack-Dealer": return "Du machst das echt gut hier.";
                case "Pokerprofi":
                case "Stammgast": return "Du spielst richtig stark.";
                case "Barkeeper": return "Deine Drinks sind die besten der Stadt.";
                case "Kassiererin":
                case "Modeverkäuferin":
                case "Tankwart":
                case "Radhändler":
                case "Möbelhändlerin": return "Toller Laden hast du hier.";
                case "Bankierin":
                case "Bankberater": return "Sie wirken sehr kompetent.";
                case "Wachmann": return "Bei Ihnen fühlt man sich sicher.";
                case "Kurier": return "Du bist echt schnell unterwegs.";
                case "Gärtner": return "Die Beete sehen wunderschön aus.";
                case "Studentin": return "Du wirkst richtig schlau.";
                case "Kreditgeber": return "Schicker Anzug.";
                case "Spielhallen-Aufsicht": return "Du hast hier alles im Griff.";
                case "Mutter": return "Ich hab dich lieb, Mama.";
                case "Oma": return "Du backst den besten Kuchen, Oma.";
                default: return "Schön, mit dir zu reden.";
            }
        }

        // Entwickler: Patrick Lupierz – Spitze passend zur Rolle; null = keine solche Option (Familie)
        public static string InsultFor(G1C_NpcPersonality p)
        {
            if (p == null) return "Du redest ganz schön viel.";
            if (p.IsFamily) return null;
            switch (p.Role)
            {
                case "Croupier":
                case "Blackjack-Dealer": return "Du schiebst die Chips viel zu langsam.";
                case "Pokerprofi":
                case "Stammgast": return p.Pride >= 0.6 ? "Du kannst wohl nichts außer angeben." : "Du kannst wohl nicht spielen.";
                case "Barkeeper": return "Die Gläser hier sind ganz schön fleckig.";
                case "Kassiererin":
                case "Modeverkäuferin":
                case "Tankwart":
                case "Radhändler":
                case "Möbelhändlerin": return "Deine Preise sind reiner Wucher.";
                case "Bankierin":
                case "Bankberater": return "Banken sind doch alle gleich.";
                case "Wachmann": return "Sie würden keinen Dieb fangen.";
                case "Kurier": return "Mein Paket kam letztens zu spät.";
                case "Gärtner": return "Da drüben wächst Unkraut.";
                case "Studentin": return "Studieren ist doch Zeitverschwendung.";
                case "Kreditgeber": return "Du bist ein Halsabschneider.";
                case "Spielhallen-Aufsicht": return "Deine Automaten sind doch alle kaputt.";
                default: return "Du redest ganz schön viel.";
            }
        }
    }

    // =====================================================
    // Klasse: G1C_ExtraTopics
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: Mehr Gesprächsthemen (SprintPlanning). Casino- und Stadtbewohner (Profile von Eduard und Denis)
    //               bekommen eigene Fragen; alle NPCs außer der Familie zusätzlich Stadtklatsch, der jeden Spieltag
    //               wechselt und auf neue Orte hinweist (Angeln, Spielhalle, Tankstelle, Hunde ...).
    //               Wird einmal im G1C_NpcSystem angewendet; doppelte Fragen werden nicht hinzugefügt.
    // =====================================================
    public static class G1C_ExtraTopics
    {
        private static readonly string[] Gossip =
        {
            "Auf den Brücken stehen jetzt Angeln. Gärtner Sam schwört, er hat da einen goldenen Karpfen gesehen.",
            "In der Spielhalle hält angeblich jemand den Schlangen-Rekord seit drei Tagen. Paula ist sauer.",
            "Die Tankstelle hat ein neues Verkaufsfenster. Olli verkauft da sogar nachts Snacks.",
            "Im Park läuft ein Hund namens Bello herum. Wer ihn streichelt, hat angeblich Glück beim Roulette.",
            "Man munkelt, es gibt einen geheimen Trick am Spielautomaten. Irgendwas mit den Pfeiltasten ... Quatsch, oder?",
            "Bei der Bank gibt es jeden Tag 2 % Zinsen. Karl erzählt das jedem, ob er will oder nicht.",
            "Oma Gerdas Katzen haben neulich den Briefträger verjagt. Alle drei. Gleichzeitig."
        };

        // Entwickler: Patrick Lupierz
        public static void Apply(IEnumerable<G1C_NpcCharacter> characters)
        {
            foreach (G1C_NpcCharacter c in characters)
            {
                foreach (G1C_DialogueTopic topic in TopicsFor(c.Name)) Add(c, topic);
                if (c.Personality.IsFamily) continue;
                Add(c, new G1C_DialogueTopic("Was gibt's Neues in der Stadt?", ctx => Gossip[(ctx.Day + c.Name.Length) % Gossip.Length]));
                Add(c, new G1C_DialogueTopic("Wie ist der Tag so?", G1C_DialogueTopic.ByTime(
                    "Früh. Zu früh. Frag mich nach dem ersten Kaffee nochmal.",
                    "Ganz gut. Viel los heute.",
                    "Der Abend ist die schönste Zeit. Die Laternen gehen an.",
                    "Es ist mitten in der Nacht! Was machst du überhaupt noch draußen?")));
            }
        }

        private static void Add(G1C_NpcCharacter c, G1C_DialogueTopic topic)
        {
            if (c.Personality.Topics.All(t => t.Question != topic.Question)) c.Personality.Topics.Add(topic);
        }

        private static IEnumerable<G1C_DialogueTopic> TopicsFor(string name)
        {
            switch (name)
            {
                case "Croupier Rosa":
                    yield return new G1C_DialogueTopic("Wie oft kommt die Null?", G1C_DialogueTopic.Cycle(
                        "Statistisch einmal in 37 Drehungen. Gefühlt immer, wenn du alles auf Rot gesetzt hast.",
                        "Immer noch einmal in 37. Die Kugel hat kein Gedächtnis – ich schon."));
                    yield return new G1C_DialogueTopic("Wie lange arbeitest du schon hier?", G1C_DialogueTopic.Cycle(
                        "Zwölf Jahre. Ich habe hier schon Millionäre kommen und Millionäre gehen sehen. Meistens ohne Million."));
                    break;
                case "Dealer Ben":
                    yield return new G1C_DialogueTopic("Wann soll ich beim Blackjack ziehen?", G1C_DialogueTopic.Cycle(
                        "Bis 11 immer. Ab 17 nie. Dazwischen kommt es auf meine offene Karte an.",
                        "Zeige ich eine 7 oder mehr, zieh bis 17. Zeige ich 2 bis 6, lass mich lieber selbst überkaufen."));
                    yield return new G1C_DialogueTopic("Zählst du Karten?", G1C_DialogueTopic.Cycle(
                        "Ich? Nein. Ich zähle nur bis 21. Das reicht für den Job.", "Psst. Nicht so laut."));
                    break;
                case "Pokerprofi Max":
                    yield return new G1C_DialogueTopic("Wie bluffe ich richtig?", G1C_DialogueTopic.Cycle(
                        "Selten. Ein Bluff, der jedes Mal kommt, ist keiner mehr – das ist eine Spende.",
                        "Und schau nicht auf deine Karten, wenn der Flop kommt. Schau auf mich. Ich schau auch auf dich."));
                    yield return new G1C_DialogueTopic("Was war dein größter Pot?", G1C_DialogueTopic.Cycle(
                        "20 000 Chips. Mit Zwei-Sieben. Frag nicht.", "Ich sagte: Frag nicht."));
                    break;
                case "Barkeeper Toni":
                    yield return new G1C_DialogueTopic("Was ist dein bester Drink?", G1C_DialogueTopic.ByTime(
                        "Morgens? Kaffee schwarz. Alles andere wäre unverantwortlich.",
                        "Der \"Glückspilz\": Zitrone, Minze, ein Hauch Hoffnung.",
                        "Der \"Letzte Einsatz\". Schmeckt nach Mut.",
                        "Um diese Uhrzeit? Wasser. Und dann nach Hause."));
                    yield return new G1C_DialogueTopic("Hörst du hier viel Klatsch?", c =>
                        "Mehr als mir lieb ist. " + Gossip[(c.Day + 3) % Gossip.Length]);
                    break;
                case "Gast Lena":
                    yield return new G1C_DialogueTopic("Glaubst du an Glückssträhnen?", G1C_DialogueTopic.Cycle(
                        "Natürlich! Ich hatte letzte Woche drei Gewinne in Folge. Danach elf Verluste, aber egal.",
                        "Mein Glücksbringer ist eine Socke. Frag nicht, welche."));
                    yield return new G1C_DialogueTopic("Was machst du, wenn du nicht spielst?", G1C_DialogueTopic.Cycle(
                        "Ich füttere die Enten am Fluss. Mit Brot! Nicht mit Chips, das hat der Park verboten."));
                    break;
                case "Kurier Paul":
                    yield return new G1C_DialogueTopic("Was lieferst du so?", G1C_DialogueTopic.Cycle(
                        "Pakete, Briefe, einmal einen Kühlschrank. Mit dem Fahrrad. Nie wieder.",
                        "Heute nur Briefe. Einer davon riecht nach Parfüm. Geht mich nichts an."));
                    yield return new G1C_DialogueTopic("Kennst du eine Abkürzung?", G1C_DialogueTopic.Cycle(
                        "Über die Brücken! Und mit dem Fahrrad bist du fast doppelt so schnell. Im Haus aber absteigen."));
                    break;
                case "Gärtner Sam":
                    yield return new G1C_DialogueTopic("Gibt es Fische im Fluss?", G1C_DialogueTopic.Cycle(
                        "Massenhaft! Auf der Brücke lehnt eine Angel. Halt den Fisch im grünen Bereich, dann klappt's.",
                        "Barsche, Forellen, Hechte – und einmal hab ich einen goldenen Karpfen gesehen. Ehrlich!"));
                    yield return new G1C_DialogueTopic("Welche Blumen magst du am liebsten?", G1C_DialogueTopic.Cycle(
                        "Sonnenblumen. Sie schauen immer nach vorn. Das sollten mehr Leute tun."));
                    break;
                case "Studentin Lea":
                    yield return new G1C_DialogueTopic("Was studierst du?", G1C_DialogueTopic.Cycle(
                        "Mathematik. Darum weiß ich auch, dass das Casino immer gewinnt. Ich spiele trotzdem. Forschung!",
                        "Meine Abschlussarbeit: \"Warum Menschen trotz negativem Erwartungswert spielen\". Du bist Teil davon."));
                    yield return new G1C_DialogueTopic("Lernst du gerade?", G1C_DialogueTopic.ByTime(
                        "Ja, mit drei Kaffee.", "Gleich. Nach dieser Runde.", "Ich lerne abends am besten. Sagt man.", "Nein. Ich schlafe. Im Stehen."));
                    break;
                case "Geldhai Viktor":
                    yield return new G1C_DialogueTopic("Wovon lebst du eigentlich?", G1C_DialogueTopic.Cycle(
                        "Von Zinsen. Deinen, bald.", "Und von Leuten, die zu viele Fragen stellen. Also: Kredit oder nicht?"));
                    yield return new G1C_DialogueTopic("Was passiert, wenn ich nicht zahle?", G1C_DialogueTopic.Cycle(
                        "Dann reden wir. Lange. Sehr lange. Ohne Kaffee."));
                    break;
                case "Bankierin Frida":
                    yield return new G1C_DialogueTopic("Sind meine Chips bei euch sicher?", G1C_DialogueTopic.Cycle(
                        "Sicherer als in deiner Hosentasche. Bodo passt auf. Und Bodo verpasst nichts.",
                        "Und sie werden jeden Tag mehr: 2 % Zinsen auf dein Guthaben."));
                    yield return new G1C_DialogueTopic("Wie werde ich reich?", G1C_DialogueTopic.Cycle(
                        "Einzahlen und warten. Langweilig, aber es funktioniert. Das Casino ist das Gegenteil davon."));
                    break;
                case "Möbelhändlerin Lotte":
                    yield return new G1C_DialogueTopic("Wie viele Möbel darf ich kaufen?", G1C_DialogueTopic.Cycle(
                        "Von jedem Stück höchstens " + G1C_Shop.MaxFurniturePerItem + ". Sonst wird dein Zuhause ein Lager.",
                        "Am Einrichtungsplan zuhause kannst du alles anklicken und verschieben. Oder zurück ins Inventar legen."));
                    yield return new G1C_DialogueTopic("Was ist gerade beliebt?", G1C_DialogueTopic.Cycle(
                        "Topfpflanzen. Sie widersprechen nie.", "Bücherregale. Auch wenn keiner mehr liest."));
                    break;
            }
        }
    }
}
