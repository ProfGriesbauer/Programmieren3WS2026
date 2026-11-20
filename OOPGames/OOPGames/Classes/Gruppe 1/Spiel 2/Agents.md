# AGENTS.md – Verbindliche Projektanweisungen für das Casino-Spiel

## 1. Deine Rolle und dein Hauptziel

Du bist der KI-Entwicklungsassistent für unser C#- und WPF-Projekt im Rahmen von **Programmieren 3 – WS 2026/27**.

Deine Aufgabe ist es, das bestehende Spiel weiterzuentwickeln, Fehler zu beheben, neue Funktionen zu implementieren, die Grafikqualität zu verbessern und sämtliche dokumentierten Anforderungen und Ideen zu berücksichtigen.

Das Ziel ist ein umfangreiches, zusammenhängendes und funktionierendes 2D-Pixel-Art-Spiel, das sich optisch und atmosphärisch an Pokémon Rubin und Saphir für den Game Boy Advance orientiert.

Das Spiel soll ein begehbares Casino mit NPCs, Gebäuden, Geschäften, interaktiven Gegenständen und verschiedenen Casino-Minispielen werden.

**Wichtig:**
- Arbeite immer mit dem vorhandenen Projekt und dessen tatsächlicher Struktur.
- Untersuche zuerst den bestehenden Code, bevor du Änderungen vornimmst.
- Bestehende funktionierende Funktionen dürfen nicht unnötig entfernt oder verschlechtert werden.
- Setze Anforderungen vollständig um und nicht nur optisch.
- Erfinde keine nicht vorhandenen Framework-Funktionen oder Schnittstellen.
- Erstelle keine unnötigen doppelten Klassen oder Systeme.
- Arbeite modular, nachvollziehbar und wartbar.
- Dokumentiere, was du geändert hast und wie du es überprüft hast.
- Wenn eine Anforderung mit den offiziellen Projektvorgaben kollidiert, haben die offiziellen Projektvorgaben Vorrang.

Das langfristige Ziel ist ein vollständiges, spielbares und technisch sauberes Spiel.

---

# 2. Verbindliche technische Projektregeln

## 2.1 Technologien

Verwende ausschließlich die im Projekt vorgegebene technische Grundlage:

- C#
- .NET 6
- WPF
- Das vorgegebene OOPGames-Framework
- Die vorhandenen Projektbibliotheken und freigegebenen Schnittstellen

Verwende keine eigenständige Game-Engine.

Insbesondere darfst du nicht eigenständig auf Unity, Godot, MonoGame oder andere Game-Engines wechseln.

## 2.2 OOPGames-Framework

Das Spiel muss vollständig mit dem vorhandenen OOPGames-Framework funktionieren.

Untersuche zunächst die tatsächlichen Interfaces und deren Implementierungen.

Berücksichtige insbesondere die vorgesehenen Rollen:

- Painter über `IPaintGame`
- Rules über `IGameRules` und `IGameField`
- Player über `IHumanGamePlayer` und `IComputerGamePlayer`

Erweiterte Interfaces wie `IGameRules2` oder `IPaintGame2` dürfen nur verwendet werden, wenn sie im vorhandenen Framework tatsächlich existieren und für das Projekt freigegeben sind.

Die Kompatibilität muss anhand der tatsächlich vorhandenen Schnittstellen geprüft werden, insbesondere über:

- `CanBePaintedBy`
- `CanBeRuledBy`

Erfinde keine Methodensignaturen, Eigenschaften oder Framework-Funktionen.

## 2.3 Was nicht verändert werden darf

Es gelten die offiziellen Einschränkungen des Projekts.

Grundsätzlich dürfen nur folgende Bereiche verändert werden:

- `Classes/<Gruppe>/`
- `Interfaces/<Gruppe>/`

Einzige ausdrücklich vorgesehene Ausnahme:

Eigene Klassen dürfen ausschließlich im vorgesehenen Bereich

`//REGISTER YOUR CLASSES HERE`

in `MainWindow.xaml.cs` registriert werden.

Folgende Dateien und Bereiche sind tabu:

- Basis-Interfaces unter `Interfaces/`
- `Classes/OOPGames.cs`
- `MainWindow.xaml` und `MainWindow.xaml.cs`, außer der erlaubten Klassenregistrierung
- `App.xaml` und `App.xaml.cs`
- `.csproj`-Dateien
- `.sln`-Dateien
- `.vscode/`
- `X_Griesbauer`
- Ordner anderer Gruppen
- Sonstige Framework-Dateien

Ändere keine Projektdateien, um Abhängigkeiten oder neue Funktionen eigenmächtig einzuführen.

Wenn eine benötigte Funktion nur durch eine Änderung am Framework möglich wäre, dokumentiere das Problem und frage zuerst nach einer Freigabe.

## 2.4 Spiel 1 und Spiel 2

Das Projekt kann mehrere Aufgaben enthalten.

Spiel 1, insbesondere Vier gewinnt, ist eine separate Aufgabe.

Vermische dessen Regeln, Logik oder Implementierung nicht mit unserem Casino-Spiel.

Die Anforderungen an unser eigenes Spiel 2 müssen unabhängig davon umgesetzt werden.

---

# 3. Projektkonzept – Casino im Pokémon-Rubin-Stil

## 3.1 Grundidee

Entwickle ein 2D-Top-down-Pixel-Art-Spiel, dessen Stil an Pokémon Rubin und Saphir erinnert.

Der Spieler befindet sich in einer Spielwelt mit einem Casino als zentralem Spielbereich.

Das Casino soll nicht nur aus einer statischen Kulisse bestehen, sondern ein tatsächlich begehbarer und interaktiver Bereich sein.

Der Spieler soll:

- Sich frei bewegen können.
- Mit NPCs sprechen können.
- Spieltische benutzen können.
- Spielautomaten bedienen können.
- Gebäude und Gegenstände untersuchen können.
- Verschiedene Minispiele starten können.
- Gegenstände kaufen und verwenden können.
- Sein Inventar öffnen können.
- Mit Händlern und Mitarbeitern interagieren können.
- Die Spielwelt erkunden können.

Die gesamte Spielwelt soll wie aus einem zusammenhängenden Pokémon-Spiel wirken.

## 3.2 Ein zusammenhängender Grafikstil

Alle Grafiken müssen optisch zueinander passen.

Achte auf:

- Einheitliche Pixelauflösung.
- Einheitliche Perspektive.
- Passende Proportionen.
- Konsistente Licht- und Schattenrichtung.
- Einheitliche Farbpalette.
- Passende Konturen.
- Wiedererkennbare Charaktere.
- Harmonische Gebäude und Innenräume.

Vermeide es, beliebige Grafiken aus unterschiedlichen Stilen zu kombinieren.

Wenn mehrere Grafiken vorhanden sind, prüfe, ob sie tatsächlich zusammenpassen.

Grafiken dürfen nicht einfach überall wiederverwendet werden, wenn dadurch Gebäude, Charaktere oder Gegenstände identisch und unpassend aussehen.

---

# 4. Spielfigur und Bewegungssteuerung

## 4.1 Spielfigur

Die Spielfigur soll optisch an die Spielfiguren aus Pokémon Rubin und Saphir erinnern.

Sie benötigt unterschiedliche Grafiken für:

- Oben
- Unten
- Links
- Rechts

Zusätzlich müssen eigene Zustände vorhanden sein:

- Idle nach oben
- Idle nach unten
- Idle nach links
- Idle nach rechts
- Laufanimation nach oben
- Laufanimation nach unten
- Laufanimation nach links
- Laufanimation nach rechts

Die Figur muss beim Richtungswechsel korrekt dargestellt werden.

Die Laufanimation darf nicht weiterlaufen, wenn der Spieler steht.

## 4.2 Größe und Skalierung

Der vorgesehene logische Darstellungsbereich der Spielfigur beträgt ungefähr 90 × 90 Pixel.

Das ist eine Orientierung für die Darstellung und keine zwingende Größe jedes einzelnen Sprite-Frames.

Berücksichtige die tatsächliche Canvas-Größe und die vorhandene Rendertechnik.

Die Grafiken müssen passend skaliert werden, ohne:

- Verzerrungen.
- Unscharfe Pixel.
- Unterschiedliche Charaktergrößen.
- Übermäßige Vergrößerung.
- Ungewollte Überlappungen.

Sprite-Größen und Darstellungsgrößen müssen voneinander getrennt bleiben.

## 4.3 WASD-Steuerung

Die WASD-Steuerung funktioniert aktuell nicht zuverlässig.

Untersuche und behebe dieses Problem vollständig.

Prüfe:

- Ob Tastatureingaben zuverlässig erkannt werden.
- Ob der Tastaturfokus verloren geht.
- Ob Eingaben während Dialogen blockiert werden.
- Ob mehrere gleichzeitig gedrückte Tasten korrekt behandelt werden.
- Ob die Figur bei gedrückter Taste kontinuierlich läuft.
- Ob die Figur nach dem Loslassen sofort stoppt.
- Ob sich die Figur diagonal korrekt verhält.
- Ob die Bewegung durch Kollisionen begrenzt wird.
- Ob Dialoge und Minispiele die normale Bewegung korrekt deaktivieren.

Implementiere keine eigene Game-Loop, wenn das Framework bereits einen geeigneten Mechanismus bereitstellt.

Verwende ausschließlich die tatsächlich vorhandenen Möglichkeiten des Frameworks zur Verarbeitung von Eingaben.

## 4.4 Zusätzliche Verbesserungsideen

Wenn es technisch möglich ist:

- Flüssigere Laufanimationen.
- Kurze Übergänge zwischen Idle und Bewegung.
- Korrekte Blickrichtung nach dem Ansprechen eines NPCs.
- Keine Animation beim Blockieren durch eine Wand.
- Einheitliche Bewegungsgeschwindigkeit.
- Keine ungewollten Geschwindigkeitssprünge bei gleichzeitig gedrückten Tasten.

---

# 5. Spielwelt, Gebäude und Kollisionen

## 5.1 Begehbare Spielwelt

Die Spielwelt soll eine klare räumliche Struktur haben.

Es muss erkennbar sein, welche Bereiche begehbar und welche blockiert sind.

Nicht durchdringbar sind beispielsweise:

- Wände.
- Möbel.
- Spieltische.
- Spielautomaten.
- Schränke.
- Dekorationen.
- Gebäudeobjekte.
- Andere feste Hindernisse.

Der Spieler darf nicht durch diese Objekte laufen.

## 5.2 Kollisionssystem

Die Kollisionslogik muss unabhängig von der grafischen Darstellung funktionieren.

Eine Änderung der Grafik darf nicht automatisch die Kollisionslogik beschädigen.

Trenne:

- Grafische Darstellung.
- Kollisionsbereiche.
- Begehbare Flächen.
- Interaktionsbereiche.

Prüfe, ob Kollisionen bei verschiedenen Bewegungsrichtungen korrekt funktionieren.

Verhindere, dass der Spieler:

- Durch Wände läuft.
- In Möbeln stecken bleibt.
- Durch Tische läuft.
- Ungewollt hinter Gebäuden verschwindet.
- Durch geschlossene Türen läuft.
- An Ecken hängen bleibt.

## 5.3 Zusätzliche Ideen

Wenn es zur vorhandenen Architektur passt:

- Unsichtbare Kollisionsflächen für unregelmäßige Objekte.
- Separate Interaktionsbereiche vor NPCs und Möbeln.
- Korrekte Darstellung der Spielfigur vor und hinter Objekten.
- Saubere Übergänge zwischen Innen- und Außenbereichen.
- Einheitliche Koordinaten- und Größenverwaltung.

---

# 6. Austauschbares Grafiksystem

## 6.1 Grafiken und Spiellogik trennen

Alle Grafiken sollen möglichst unabhängig von der Spiellogik austauschbar sein.

Ein Austausch einer Grafik darf nicht dazu führen, dass Spielmechaniken angepasst werden müssen.

Verwende sprechende Grafik-IDs, beispielsweise:

- `player_idle_up`
- `player_idle_down`
- `player_walk_left`
- `player_walk_right`
- `casino_floor`
- `casino_wall`
- `roulette_table`
- `blackjack_table`
- `slot_machine`
- `claw_machine`
- `casino_npc`
- `shop_counter`

## 6.2 Zentraler Grafikmanager

Prüfe, ob ein zentraler Grafikmanager mit dem vorhandenen Framework sinnvoll umgesetzt werden kann.

Falls ja, soll er die Zuordnung von Grafik-IDs zu Grafikressourcen verwalten.

Grafiken sollen nicht an zahlreichen Stellen direkt und fest in der Spiellogik referenziert werden.

Eine mögliche Trennung:

- Grafikressourcen.
- Grafikzuordnung.
- Animationen.
- Spiellogik.
- Darstellung.
- Kollisionsdaten.

Verwende nur die im Projekt zulässigen Speicherorte.

Falls keine zusätzlichen Asset-Ordner erlaubt sind, müssen die Ressourcen innerhalb der freigegebenen Struktur organisiert werden. Ändere keine Projektdateien, um neue Ressourcen einzubinden.

## 6.3 Grafikqualität

Prüfe alle vorhandenen Grafiken auf:

- Richtige Größe.
- Richtige Ausrichtung.
- Transparenz.
- Passende Perspektive.
- Fehlerhafte Pixel.
- Falsche Animationen.
- Unpassende Farbgebung.
- Doppelte oder falsch zugeordnete Grafiken.

Insbesondere die Vogelgrafiken sind aktuell fehlerhaft und müssen überprüft und korrigiert werden.

Auch Bett, Teppiche, Möbel und andere Inneneinrichtungen sollen optisch verbessert werden.

---

# 7. NPCs und Charaktere

## 7.1 NPC-Grundsystem

NPCs sollen eigenständige Charaktere mit individuellen Eigenschaften sein.

Jeder NPC soll möglichst eigene:

- Grafiken.
- Namen.
- Dialoge.
- Dialogoptionen.
- Antworten.
- Interaktionsmöglichkeiten.
- Aufgaben oder Funktionen.

besitzen.

Nicht jeder NPC darf dieselben Dialoge oder dieselben Antwortmöglichkeiten verwenden.

## 7.2 Mutter und Oma

Die Mutter und die Oma des Spielers fehlen aktuell und müssen ergänzt werden.

### Mutter

Die Mutter soll:

- Optisch zum Pokémon-Rubin-Stil passen.
- Als eigenständige Figur erkennbar sein.
- Individuelle Dialoge besitzen.
- Eigene Antwortmöglichkeiten anbieten.
- Eine erkennbare Persönlichkeit haben.
- In die Spielwelt und die Hintergrundgeschichte integriert werden.

### Oma

Die Oma soll ein eigenständiger, wiedererkennbarer Charakter sein.

Als visuelle und charakterliche Inspiration dient die ältere Frau aus Deadpool.

Übernimm keine geschützte Figur eins zu eins und kopiere keine Originalgrafiken.

Die Oma soll beispielsweise:

- Graue oder weiße Haare haben.
- Einen älteren, markanten Charakterstil besitzen.
- Trocken und sarkastisch sein.
- Einen eigenen Humor haben.
- Ungewöhnliche und unterhaltsame Dialoge führen.
- Individuelle Antworten und Dialogoptionen anbieten.

Die Figur muss optisch weiterhin in die Pixel-Art-Welt passen.

## 7.3 Dialogsystem

Das Dialogsystem muss deutlich verbessert werden.

Jeder Charakter soll individuelle Dialogoptionen haben.

Beispielsweise:

- Begrüßung.
- Fragen zur Spielwelt.
- Fragen zu Geschäften.
- Fragen zum Casino.
- Persönliche Gespräche.
- Besondere Antworten.
- Wiederholte Gespräche mit veränderten Antworten.

Die Antworten müssen zur gewählten Option passen.

Vermeide allgemeine Standarddialoge, die für jeden NPC identisch sind.

## 7.4 Zusätzliche Dialogideen

Wenn sinnvoll:

- NPCs erinnern sich an vorherige Gespräche.
- Dialoge verändern sich abhängig vom Spielfortschritt.
- NPCs reagieren auf Tageszeiten.
- Mitarbeiter geben Informationen über ihre Geschäfte.
- Charaktere können kleine Hinweise und Geheimnisse verraten.
- Die Oma kann besondere humorvolle Kommentare machen.

Diese Funktionen müssen mit der vorhandenen Architektur umgesetzt werden und dürfen nicht unnötig kompliziert werden.

---

# 8. Casino und Minispiele

Das Casino ist der zentrale Spielbereich.

Es soll verschiedene interaktive Spielmöglichkeiten geben.

Jedes Minispiel muss modular und unabhängig implementiert werden.

## 8.1 Roulette

Erstelle ein eigenes Roulette-System mit:

- Roulette-Tisch.
- Einsatzmöglichkeiten.
- Zahlen und Farben.
- Gewinn- und Verlustberechnung.
- Auszahlung.
- Einsatzverwaltung.
- Passender grafischer Darstellung.

Die Spielregeln müssen korrekt sein.

## 8.2 Poker

Erstelle ein eigenständiges Pokersystem.

Je nach Umfang und vorhandener Zeit soll es Folgendes enthalten:

- Kartenverwaltung.
- Mischen.
- Austeilen.
- Spielerentscheidungen.
- Handbewertung.
- Gewinnermittlung.
- Gegnerlogik.
- Einsatzverwaltung.

Die Regeln müssen eindeutig und nachvollziehbar sein.

## 8.3 Blackjack

Implementiere ein eigenständiges Blackjack-Spiel.

Dazu gehören:

- Karten.
- Kartenwerte.
- Spielerhand.
- Dealerhand.
- Ziehen.
- Stehenbleiben.
- Bust.
- Blackjack.
- Gewinn- und Verlustberechnung.
- Einsätze und Auszahlungen.

Die Dealer-KI muss die vorgesehenen Regeln befolgen.

## 8.4 Spielautomaten

Implementiere einen oder mehrere Spielautomaten.

Mögliche Funktionen:

- Walzen.
- Symbole.
- Zufällige Ergebnisse.
- Gewinnkombinationen.
- Verschiedene Auszahlungen.
- Einsätze.
- Gewinnanimationen.

Die Zufallslogik darf nicht von der Grafik abhängig sein.

## 8.5 KI-Gegner

Falls ein Minispiel Gegner benötigt, soll deren Verhalten eigenständig und nachvollziehbar implementiert werden.

Die KI muss mit den tatsächlichen Regeln des jeweiligen Spiels arbeiten.

Keine bloße Zufallsauswahl, wenn eine sinnvolle Strategie möglich ist.

## 8.6 Zentrale Spielwährung

Alle Casino-Spiele sollen auf ein gemeinsames Guthabensystem zugreifen.

Verhindere:

- Negative Guthaben.
- Doppelte Abbuchungen.
- Doppelte Auszahlungen.
- Auszahlung ohne gültigen Gewinn.
- Manipulation des Guthabens durch bloße Grafikaktionen.

Die Berechnung muss in der Spiellogik stattfinden.

---

# 9. Inventar und Gegenstände

Erstelle beziehungsweise verbessere ein zentrales Inventarsystem, sofern es mit den bestehenden Framework-Vorgaben umsetzbar ist.

Der Spieler soll Gegenstände besitzen und verwenden können.

Beispielsweise:

- Fahrrad.
- Gekaufte Gegenstände.
- Besondere Items.
- Casino-Gegenstände.
- Eventuell Schlüssel oder andere Freischaltgegenstände.

Das Inventar soll übersichtlich dargestellt werden.

Grafische Darstellung und Inventarlogik müssen getrennt sein.

Ein Gegenstand darf nicht nur als Grafik angezeigt werden, sondern muss tatsächlich im Inventar gespeichert und verwendbar sein.

---

# 10. Gebäude, Geschäfte und Öffnungszeiten

## 10.1 Geschäfte

Die Öffnungszeiten werden aktuell teilweise für Verkäufe verwendet, aber die Geschäfte schließen nicht richtig.

Dieses Verhalten muss korrigiert werden.

Wenn ein Geschäft geschlossen ist:

- Dürfen keine normalen Verkäufe stattfinden.
- Muss die Interaktion entsprechend reagieren.
- Muss ein geschlossenes Geschäft visuell erkennbar sein.
- Muss ein CLOSED-Schild an der Tür angezeigt werden.

Wenn das Geschäft wieder öffnet, muss das Schild verschwinden beziehungsweise der normale Zustand wiederhergestellt werden.

Die Öffnungszeiten müssen tatsächlich mit der Spielzeit verbunden sein.

## 10.2 Arcade

Eine Arcade beziehungsweise Spielhalle fehlt und muss ergänzt werden.

Sie soll als eigener, erkennbarer Bereich gestaltet werden.

Die Claw Machine beziehungsweise der Greifautomat muss in die Arcade und nicht in ein falsches Gebäude.

Der Greifautomat soll:

- Eine eigene Grafik besitzen.
- Interaktiv sein.
- Einen verständlichen Ablauf haben.
- Einen tatsächlichen Spielmechanismus besitzen.
- Einen Gewinn oder Verlust ermöglichen.

## 10.3 Tankstelle

Die Tankstelle soll bewusst auch dann geöffnet bleiben können, wenn andere Geschäfte geschlossen sind.

Sie soll:

- Rot gestaltet sein.
- Zwei Zapfsäulen besitzen.
- Ein kleines Gebäude mit Verkäufer enthalten.
- Optisch als Tankstelle erkennbar sein.
- Eine eigene Innen- oder Interaktionsdarstellung besitzen.

Der Spieler soll durch ein Fenster mit dem Verkäufer sprechen und einkaufen können.

Die Interaktion muss tatsächlich funktionieren und darf nicht nur eine dekorative Grafik sein.

## 10.4 Innenwände und Dekorationen

Die linken und rechten Innenwände der Gebäude sehen aktuell schlecht aus.

Überarbeite die Wanddarstellung.

Vermeide, dass dieselbe Wandgrafik überall unpassend wiederholt wird.

Die Innenwände sollen:

- Zur jeweiligen Raumgestaltung passen.
- Korrekte Perspektiven besitzen.
- Einheitliche Übergänge haben.
- Nicht verzerrt sein.

Ergänze außerdem:

- Poster.
- Bilder.
- Wanddekoration.
- Kleine Details.
- Passende Beleuchtung und Gestaltungselemente, soweit technisch möglich.

Poster sollen eigene Grafiken und eine sinnvolle Positionierung besitzen.

## 10.5 Bett und Teppiche

Bett, Teppiche und weitere Einrichtungsgegenstände müssen optisch verbessert werden.

Achte auf:

- Richtige Größe.
- Passende Perspektive.
- Stimmige Farben.
- Einheitliche Pixel-Art.
- Korrekte Kollisionsbereiche.
- Sinnvolle Platzierung im Raum.

## 10.6 Pavillon

Der Pavillon ist aktuell schwer von einem Schild zu unterscheiden und sieht nicht korrekt aus.

Überarbeite seine Grafik und Darstellung.

Der Pavillon soll:

- Als eigenständiges Gebäude beziehungsweise Objekt erkennbar sein.
- Eine passende Dach- und Stützkonstruktion besitzen.
- Nicht mit einem Schild verwechselt werden können.
- Nur von unten begehbar sein, wenn das der vorgesehenen Gestaltung entspricht.
- Eine korrekte Kollisionsfläche besitzen.

## 10.7 Schilder und Poster

Schilder müssen anklickbar beziehungsweise interaktiv sein.

Der Spieler soll durch Interaktion Informationen erhalten.

Auch Poster sollen untersuchbar sein.

Folgende Texte sollen als kleine Details integriert werden:

- Größeres Poster: „Hier könnte Ihre Werbung stehen“
- Kleiner Sticker: „Nett hier, aber waren Sie schon mal in Baden-Württemberg“

Die Texte sollen zur Spielwelt passen und grafisch sinnvoll eingebunden werden.

---

# 11. Fahrrad und Fahrradgeschäft

Vor dem Haus des Spielers soll ein Fahrrad stehen.

Es soll optisch zum Spiel passen.

Zusätzlich soll es ein Geschäft geben, in dem verschiedene Fahrräder ausgestellt sind.

Mögliche Fahrradfarben:

- Rot.
- Blau.
- Grün.
- Gelb.
- Weitere passende Varianten.

Der Spieler soll andere Fahrradfarben kaufen können.

Das Fahrrad muss:

- Im Inventar gespeichert werden.
- Aus dem Inventar aufrufbar sein.
- Tatsächlich verwendbar sein, sofern die Framework-Mechanik dies zulässt.
- Eine passende Darstellung besitzen.
- Eine eigene Logik für den Besitz und die Auswahl haben.

Wenn ein Fahrrad verwendet wird, soll sich die Bewegung sinnvoll anfühlen.

Untersuche, ob sich eine höhere Bewegungsgeschwindigkeit umsetzen lässt, ohne die vorhandene Spielmechanik zu beschädigen.

---

# 12. Schlafanimation und Tagesablauf

Die Spielfigur soll eine Schlafanimation besitzen.

Wenn der Spieler schlafen geht:

- Soll die Figur optisch schlafen.
- Soll eine passende Animation angezeigt werden.
- Soll der Schlafzustand korrekt verwaltet werden.
- Soll der Tagesablauf weitergeschaltet werden, sofern das vorhandene Spielsystem dies unterstützt.

Prüfe, ob andere Systeme vom Tageswechsel abhängig sind.

Dazu gehören insbesondere:

- Öffnungszeiten.
- Bankzinsen.
- NPC-Verhalten.
- Tageszeit.
- Eventuelle Tagesereignisse.

Vermeide widersprüchliche Tages- und Zeitberechnungen.

---

# 13. Bank und Guthaben

Die Bank soll optisch und funktional verbessert werden.

Aktuell soll die Bank deutlich schöner gestaltet werden.

Sie soll:

- Ein eigenes Gebäude besitzen.
- Mitarbeiter beziehungsweise Bankangestellte enthalten.
- Eine passende Innenausstattung besitzen.
- Individuelle Dialoge anbieten.
- Tatsächlich mit dem Spielguthaben verbunden sein.

## 13.1 Bankfunktion

Die Bank soll das Guthaben des Spielers verwalten.

Das Bankguthaben soll pro Spieltag um 2 % erhöht werden.

Die Berechnung muss korrekt sein.

Beispiel:

Bei einem Bankguthaben von 1.000 Einheiten beträgt der Zuwachs nach einem Spieltag 20 Einheiten.

Nach der Verzinsung beträgt das Guthaben 1.020 Einheiten.

Verwende das tatsächliche Bankguthaben als Berechnungsgrundlage.

## 13.2 Zusätzliche Bankideen

Wenn es sinnvoll und technisch umsetzbar ist:

- Geld einzahlen.
- Geld abheben.
- Kontostand anzeigen.
- Zinsentwicklung anzeigen.
- Transaktionsübersicht.
- Verständliche Dialoge mit Bankmitarbeitern.

Verhindere doppelte Zinszahlungen am selben Spieltag.

Die Zinsberechnung muss unabhängig von der grafischen Darstellung funktionieren.

---

# 14. Spielzustände und Interaktionen

Das Spiel benötigt eine klare Verwaltung der verschiedenen Spielzustände.

Mögliche Zustände sind:

- `CasinoExploration`
- `Dialogue`
- `Roulette`
- `Poker`
- `Blackjack`
- `Slots`
- `Result`
- `Inventory`
- `Shop`
- `Bank`
- `Sleeping`

Verwende nur so viele Zustände, wie tatsächlich erforderlich sind.

Der Wechsel zwischen Zuständen muss eindeutig geregelt sein.

Beispiele:

- Beim Gespräch mit einem NPC wird die normale Bewegung deaktiviert.
- Beim Verlassen eines Dialogs wird die normale Bewegung wieder aktiviert.
- Beim Start eines Minispiels wird die Spielfigur nicht mehr frei bewegt.
- Nach dem Minispiel kehrt der Spieler in den Casino-Bereich zurück.
- Beim Öffnen des Inventars wird die normale Steuerung passend behandelt.

Verhindere, dass mehrere Spielzustände gleichzeitig ungewollt aktiv sind.

## 14.1 Interaktionssystem

Interaktionen sollen einheitlich aufgebaut sein.

Der Spieler soll beispielsweise vor einem NPC, einem Schild oder einem Spieltisch stehen und per vorgesehener Eingabe interagieren können.

Nutze die vorhandenen Möglichkeiten des Frameworks.

Achte auf:

- Erreichbarkeit.
- Interaktionsrichtung.
- Entfernung.
- Eindeutige Auswahl.
- Verhinderung mehrfacher unbeabsichtigter Interaktionen.

---

# 15. Zusätzliche Ideen zur Verbesserung des Spiels

Die folgenden Ideen sollen geprüft und bei sinnvoller Umsetzbarkeit in die Planung aufgenommen werden.

Sie dürfen nicht blind implementiert werden, wenn sie die offiziellen Projektregeln verletzen oder die vorhandene Architektur unnötig komplizieren.

## 15.1 Atmosphäre

- Unterschiedliche Beleuchtung für Casino, Arcade und Geschäfte.
- Dezente Animationen bei Spielautomaten.
- Kleine Animationen bei NPCs.
- Dekorationen, die die Gebäude lebendiger wirken lassen.
- Unterschiedliche Raumgestaltungen.
- Passende Farbgebung für verschiedene Bereiche.
- Kleine visuelle Effekte bei Gewinnen.

## 15.2 Spielwelt

- NPCs mit unterschiedlichen Tagesabläufen.
- Kleine versteckte Details.
- Untersuchbare Gegenstände.
- Kleine Nebenaufgaben.
- Besondere Dialoge.
- Dekorative Details, die zur Welt passen.

## 15.3 Casino

- Unterschiedliche Einsätze.
- Kleine Belohnungen.
- Gewinnanzeigen.
- Verlustanzeigen.
- Übersicht über vergangene Spielergebnisse.
- Einheitliche Darstellung des Guthabens.
- Kurze Animationen für Spielausgänge.

## 15.4 Benutzeroberfläche

Die Benutzeroberfläche soll verständlich und optisch einheitlich sein.

Mögliche Elemente:

- Guthabenanzeige.
- Inventarübersicht.
- Dialogfenster.
- Interaktionshinweise.
- Spielmenüs.
- Gewinn- und Verlustanzeigen.
- Statusanzeigen.
- Tageszeit und Spieltag.

Die Oberfläche darf das eigentliche Spielfeld nicht unnötig verdecken.

## 15.5 Fehlervermeidung

- Keine doppelten Interaktionen.
- Keine mehrfachen Auszahlungen.
- Keine verlorenen Eingaben.
- Keine festhängenden Animationen.
- Keine blockierten Spielzustände.
- Keine unerreichbaren NPCs.
- Keine ungewollten Kollisionen.
- Keine inkonsistenten Guthabenwerte.

---

# 16. Prüfung aller Issues im lokalen SprintPlanning-Ordner

Dieser Abschnitt ist besonders wichtig.

Die maßgebliche Aufgabenquelle sind die lokalen Dokumente im Projektordner:

`Classes/<Gruppe>/Scrum/SprintPlanning/`

**Es sollen keine GitHub-Issues als Grundlage verwendet werden.**

## 16.1 Vollständige Durchsicht

Untersuche alle vorhandenen Dokumente und Unterordner in `SprintPlanning`.

Berücksichtige:

- Offene Aufgaben.
- Abgeschlossene Aufgaben.
- Teilweise erledigte Aufgaben.
- Fehlerberichte.
- Verbesserungsvorschläge.
- Kommentare.
- Ergänzungen.
- Zusätzliche Ideen.
- Bereits geplante Funktionen.
- Aufgaben aus vergangenen Sprints.

Lies die vollständigen Inhalte und nicht nur Dateinamen oder Überschriften.

## 16.2 Tatsächlichen Implementierungsstand überprüfen

Vergleiche jede Aufgabe mit dem tatsächlichen Quellcode.

Eine Aufgabe gilt nicht automatisch als erledigt, nur weil sie im Dokument als abgeschlossen markiert wurde.

Prüfe:

1. Ist die Funktion vorhanden?
2. Ist sie vollständig implementiert?
3. Funktioniert sie tatsächlich?
4. Ist sie mit anderen Systemen verbunden?
5. Gibt es Fehler?
6. Fehlen Teilfunktionen?
7. Ist sie nur grafisch vorhanden?
8. Gibt es Tests?
9. Funktioniert sie nach anderen Änderungen weiterhin?

## 16.3 Statuskategorien

Verwende für jede Aufgabe einen dieser Status:

- `VOLLSTÄNDIG UMGESETZT`
- `TEILWEISE UMGESETZT`
- `FEHLERHAFT`
- `NUR GRAFISCH VORHANDEN`
- `NICHT IMPLEMENTIERT`
- `NICHT ÜBERPRÜFBAR`
- `ÜBERHOLT`

Begründe jeden Status anhand des tatsächlichen Codes oder eines nachvollziehbaren Testergebnisses.

## 16.4 Ideen aus den Issues

Alle sinnvollen Ideen aus den SprintPlanning-Dokumenten müssen berücksichtigt werden.

Verliere keine Vorschläge, nur weil sie nicht ausdrücklich in diesem Dokument aufgeführt sind.

Sortiere die Ideen in:

- Bereits umgesetzt.
- Noch offen.
- In Planung.
- Abhängig von anderen Funktionen.
- Nicht sinnvoll oder nicht mit den Projektvorgaben vereinbar.

Begründe, warum eine Idee nicht übernommen wird.

## 16.5 Zentrale Auswertung

Erstelle folgende Datei:

`Classes/<Gruppe>/Plans/SprintPlanning_Issue_Auswertung.md`

Dokumentiere darin für jede Aufgabe:

- Ursprüngliche Bezeichnung.
- Beschreibung.
- Quelldatei.
- Enthaltene Ideen.
- Ursprünglicher Status.
- Tatsächlicher Status.
- Betroffene Klassen.
- Fehlende Funktionen.
- Testergebnisse.
- Priorität.
- Verantwortliches Teammitglied.
- Nächste Schritte.

Verändere die ursprünglichen SprintPlanning-Dateien nicht unnötig.

Lösche keine alten Aufgaben oder deren Historie.

## 16.6 Product Backlog aktualisieren

Alle offenen und sinnvollen Aufgaben müssen in den SCRUM-Product-Backlog übernommen werden.

Formuliere sie als verständliche User Stories mit:

- Beschreibung.
- Akzeptanzkriterien.
- Priorität.
- Abhängigkeiten.
- Verantwortlichem.
- Umsetzungsstatus.

Doppelte Aufgaben sollen erkannt und sinnvoll zusammengeführt werden, ohne Informationen zu verlieren.

---

# 17. Automatisiertes Testsystem

Es soll ein automatisiertes Prüfprogramm beziehungsweise eine Testsammlung geben, mit der wichtige Funktionen des Spiels überprüft werden können.

## 17.1 Start nur auf ausdrücklichen Aufruf

Das Testsystem darf nicht automatisch bei jedem normalen Start des Spiels ausgeführt werden.

Es soll nur starten, wenn es ausdrücklich durch den Nutzer oder den KI-Agenten angefordert wird.

Ein normaler Spielstart darf dadurch nicht verlangsamt oder gestört werden.

## 17.2 Zu prüfende Funktionen

Das Testsystem soll, soweit technisch möglich, folgende Bereiche überprüfen:

**Bewegung**
- WASD-Eingaben.
- Richtungswechsel.
- Bewegungsgrenzen.
- Kollisionen.
- Stillstand nach Loslassen der Taste.

**Grafiken**
- Vorhandene Grafikzuordnungen.
- Fehlende Grafik-IDs.
- Fehlende Animationen.
- Ungültige Zuordnungen.
- Unterschiedliche Größen, sofern sie unzulässig sind.

**NPCs und Dialoge**
- Vorhandene NPC-Daten.
- Dialogzuordnungen.
- Dialogoptionen.
- Antwortzuordnungen.

**Gebäude und Interaktionen**
- Interaktionsmöglichkeiten.
- Kollisionsbereiche.
- Begehbare Flächen.
- Öffnungszeiten.
- Geschlossene Geschäfte.

**Casino**
- Roulette-Berechnungen.
- Blackjack-Regeln.
- Poker-Handbewertungen.
- Spielautomaten-Ergebnisse.
- Einsätze und Auszahlungen.

**Inventar und Bank**
- Einlagern und Entfernen von Gegenständen.
- Guthabenänderungen.
- Einzahlungen und Abhebungen.
- Zinsberechnung von 2 %.
- Verhinderung doppelter Tageszinsen.

**Spielzustände**
- Wechsel zwischen Dialogen und Spielwelt.
- Start und Ende von Minispielen.
- Öffnen und Schließen des Inventars.
- Vermeidung ungültiger Zustände.

## 17.3 Testergebnisse

Jeder Test soll eindeutig anzeigen:

- BESTANDEN
- FEHLGESCHLAGEN
- NICHT AUSFÜHRBAR
- NICHT VORHANDEN

Dokumentiere Fehler mit möglichst genauer Angabe der betroffenen Klasse oder Methode.

Unterscheide zwischen automatisierten Tests und manuellen Prüfungen.

Erfinde keine Testergebnisse.

## 17.4 Technische Grenzen

Erstelle Tests nur innerhalb der offiziell erlaubten Projektstruktur.

Falls ein separates Testprojekt oder Änderungen an Projektdateien notwendig wären, frage vorher nach einer Freigabe.

Verwende nach Möglichkeit vorhandene Testmöglichkeiten und reine Logiktests, die ohne eigene Game-Loop auskommen.

---

# 18. Modulare Klassenstruktur

Alle Klassen müssen modular und unabhängig aufgebaut sein.

Jede Klasse soll eine klar definierte Aufgabe besitzen.

Vermeide große Klassen, in denen sämtliche Spielmechaniken, Grafiken und Eingaben miteinander vermischt sind.

Trenne insbesondere:

- Spiellogik.
- Darstellung.
- Eingabeverarbeitung.
- Datenverwaltung.
- Animationen.
- Kollisionen.
- NPC-Verhalten.
- Dialoge.
- Minispiele.
- Inventar.
- Guthabenverwaltung.
- Spielzustände.

## 18.1 Minispiele

Roulette, Poker, Blackjack und Spielautomaten sollen jeweils eigene Klassen beziehungsweise klar abgegrenzte Module besitzen.

Ein Fehler in einem Minispiel soll nicht automatisch die anderen Minispiele beschädigen.

## 18.2 Gemeinsame Systeme

Gemeinsam verwendete Funktionen sollen zentral verwaltet werden.

Beispielsweise:

- Guthaben.
- Inventar.
- Grafikzuordnungen.
- Spielzustände.
- Dialogverwaltung.
- Zeit und Tageswechsel.

Vermeide doppelte Implementierungen derselben Funktion.

## 18.3 Keine unnötige Komplexität

Modularität bedeutet nicht, für jede einzelne Kleinigkeit eine eigene Klasse zu erstellen.

Wähle eine verständliche und sinnvolle Aufteilung.

Die Struktur muss zum vorhandenen OOPGames-Framework passen.

---

# 19. Entwicklerkennzeichnung in allen Änderungen

Jede neu erstellte oder wesentlich veränderte Klasse, Methode und jeder relevante Codeabschnitt muss mit dem Namen des verantwortlichen Entwicklers gekennzeichnet werden.

## 19.1 Name ermitteln

Wenn der Name des verantwortlichen Teammitglieds nicht bekannt ist, frage zuerst danach.

Erfinde keine Namen.

KI-generierter Code wird dem menschlichen Teammitglied zugeordnet, das den Code geprüft, verstanden und übernommen hat.

Die KI darf nicht als menschlicher Entwickler ausgegeben werden.

## 19.2 Klassenheader

Verwende beispielsweise:

```csharp
// Klasse: CAS_Roulette
// Entwickler: Max Mustermann
// Erstellt: 12.11.2026
// Beschreibung: Verwaltet die Spiellogik des Roulette-Minispiels.
```

## 19.3 Methodenkennzeichnung

Neue oder wesentlich geänderte Methoden sollen beispielsweise so gekennzeichnet werden:

```csharp
// Entwickler: Max Mustermann
// Aufgabe: Berechnet das Spielergebnis des Roulette-Spiels.
```

## 19.4 Änderungen

Bei späteren Änderungen muss ein Kommentar mit folgenden Angaben ergänzt werden:

- Entwickler.
- Datum.
- Beschreibung der Änderung.

Beispiel:

```csharp
// Änderung durch: Max Mustermann
// Datum: 15.11.2026
// Änderung: Fehlerhafte Berechnung der Auszahlung korrigiert.
```

## 19.5 Zusammenarbeit

Wenn mehrere Teammitglieder an einer Klasse arbeiten:

- Bestehende Kennzeichnungen erhalten.
- Eigene Methoden kennzeichnen.
- Eigene Änderungen kennzeichnen.
- Keine fremden Beiträge als eigene ausgeben.
- Keine fremden Kennzeichnungen entfernen.

Die Kommentare ersetzen nicht die Git-Historie.

---

# 20. Dateinamen und Namenskonventionen

Verwende die offizielle Dateinamenskonvention:

`Vorname_Nachname_<Inhalt>.cs`

Beispiel:

`Max_Mustermann_Roulette.cs`

Klassen und Interfaces müssen das Gruppenpräfix verwenden.

Beispiel:

`CAS_Roulette`

Achte darauf, dass:

- Klassennamen eindeutig sind.
- Interfaces eindeutig sind.
- `Name`-Properties nicht ungewollt mit anderen Klassen kollidieren.
- Keine doppelten Klassen erstellt werden.
- Die vorhandenen Namenskonventionen eingehalten werden.

---

# 21. Dokumentation der KI-Prompts

Jedes Teammitglied muss seine verwendeten KI-Prompts dokumentieren.

Die Datei soll innerhalb von `Classes/<Gruppe>/` liegen.

Namensschema:

`Vorname_Nachname_Prompts.md`

Dokumentiere:

- Verwendeten Prompt im Originalwortlaut.
- Datum.
- Verwendetes Werkzeug beziehungsweise KI-Modell.
- Ergebnis der Anfrage.
- Welche Teile übernommen wurden.
- Welche Teile angepasst oder verworfen wurden.
- Wer den Code geprüft und übernommen hat.

Die Dokumentation muss nachvollziehbar bleiben.

---

# 22. Planung und SCRUM

## 22.1 Planung

Pläne gehören in:

`Classes/<Gruppe>/Plans/`

Dort sollen insbesondere stehen:

- Projektkonzept.
- Architekturüberlegungen.
- Technische Entscheidungen.
- SprintPlanning-Auswertungen.
- Testberichte.
- Weitere relevante Projektdokumentation.

## 22.2 SCRUM

Die SCRUM-Dokumentation gehört in:

`Classes/<Gruppe>/Scrum/`

Berücksichtige:

- Product Owner.
- Scrum Master.
- Product Backlog.
- User Stories.
- Sprintplanung.
- Sprintziele.
- Aufgabenverantwortliche.
- Abstimmungsprotokolle.
- Sprint Reviews.
- Retrospektiven.

Sprints sollen eine Dauer von ein bis zwei Wochen haben.

Jeder Sprint benötigt ein klares Ziel und konkrete Aufgaben.

## 22.3 Aufgabenplanung

Bevor größere Änderungen vorgenommen werden:

1. Bestehende Aufgaben untersuchen.
2. Abhängigkeiten erkennen.
3. Prioritäten festlegen.
4. Änderungen in sinnvolle Arbeitsschritte aufteilen.
5. Verantwortlichkeiten berücksichtigen.
6. Erst danach implementieren.

---

# 23. Git-Regeln

Alle Teammitglieder verwenden ihren eigenen Git-Account.

Jedes Teammitglied muss eigene Änderungen selbst committen und pushen.

Beachte:

- Kleine und aussagekräftige Commits.
- Vor dem Push `git pull`.
- Vor dem Push `dotnet build`.
- Vor einem Commit `git status`.
- Vor einem Commit `git diff`.
- Keine fremden Änderungen überschreiben.
- Keine Änderungen anderer Teammitglieder als eigene ausgeben.
- Kein `git push --force`.
- Keine `bin/`- oder `obj/`-Ordner committen.
- Keine Änderungen an geschützten Dateien.

Wenn Konflikte auftreten, untersuche sie sorgfältig.

Löse Konflikte nicht durch blindes Überschreiben.

---

# 24. Vorgehensweise bei jeder neuen Aufgabe

Arbeite nach diesem verbindlichen Ablauf.

## Phase 1 – Bestand aufnehmen

- Projektstruktur untersuchen.
- Vorhandene Klassen untersuchen.
- Framework-Schnittstellen untersuchen.
- Bestehende Funktionen nachvollziehen.
- Vorhandene Grafiken und Ressourcen untersuchen.
- SprintPlanning-Dokumente berücksichtigen.
- Bereits vorhandene Lösungen erkennen.

## Phase 2 – Problem verstehen

- Ursache des Problems suchen.
- Betroffene Klassen feststellen.
- Abhängigkeiten untersuchen.
- Prüfen, ob die Funktion bereits teilweise vorhanden ist.
- Prüfen, welche Projektregeln gelten.

## Phase 3 – Planung

- Lösung festlegen.
- Änderungen modular aufteilen.
- Betroffene Dateien bestimmen.
- Risiken erkennen.
- Testmöglichkeiten festlegen.

Bei größeren Änderungen zuerst die geplante Lösung dokumentieren.

## Phase 4 – Implementierung

- Nur notwendige Dateien ändern.
- Bestehende Funktionen erhalten.
- Klassen modular gestalten.
- Entwicklerkennzeichnungen ergänzen.
- Fehlerbehandlung berücksichtigen.
- Grafik und Spiellogik trennen.
- Keine unzulässigen Framework-Änderungen vornehmen.

## Phase 5 – Überprüfung

- `dotnet build` ausführen.
- Vorhandene Tests ausführen, soweit erlaubt.
- Betroffene Funktionen gezielt überprüfen.
- Nebenwirkungen untersuchen.
- Fehler korrigieren.
- Nicht ausgeführte Prüfungen ausdrücklich kennzeichnen.

## Phase 6 – Dokumentation

- Änderungen dokumentieren.
- Status der Aufgabe aktualisieren.
- Testergebnisse dokumentieren.
- Offene Probleme festhalten.
- Product Backlog gegebenenfalls aktualisieren.

---

# 25. Abschlussprüfung des gesamten Spiels

Nach größeren Entwicklungsphasen muss das gesamte Spiel erneut überprüft werden.

Kontrolliere insbesondere:

### Steuerung
- Funktioniert WASD?
- Funktionieren alle Richtungen?
- Sind Animationen korrekt?
- Funktionieren Kollisionen?

### Grafiken
- Sind alle Grafiken vorhanden?
- Sind die Größen passend?
- Passen die Grafiken zusammen?
- Funktionieren alle Animationen?
- Sind Vogelgrafiken und Innenausstattung korrigiert?

### NPCs
- Sind Mutter und Oma vorhanden?
- Haben NPCs eigene Grafiken?
- Haben sie eigene Dialoge?
- Funktionieren die Dialogoptionen?

### Gebäude
- Sind alle Gebäude korrekt dargestellt?
- Sind Innenwände und Dekorationen verbessert?
- Ist der Pavillon erkennbar?
- Funktionieren Schilder und Poster?

### Geschäfte
- Funktionieren Öffnungszeiten?
- Werden geschlossene Geschäfte korrekt angezeigt?
- Ist die Tankstelle wie vorgesehen gestaltet?
- Ist die Arcade vorhanden?
- Befindet sich die Claw Machine im richtigen Gebäude?

### Fahrrad
- Steht ein Fahrrad vor dem Spielerhaus?
- Funktioniert der Fahrradshop?
- Können andere Farben gekauft werden?
- Ist das Fahrrad im Inventar vorhanden?

### Bank
- Sind Mitarbeiter vorhanden?
- Funktionieren Einzahlungen und Abhebungen?
- Werden 2 % pro Spieltag korrekt berechnet?
- Werden Zinsen nicht doppelt ausgezahlt?

### Casino
- Funktionieren Roulette, Poker, Blackjack und Spielautomaten?
- Werden Einsätze korrekt abgezogen?
- Werden Gewinne korrekt ausgezahlt?
- Funktioniert die Rückkehr in die Spielwelt?

### Dokumentation und Technik
- Sind Klassen modular?
- Sind Entwicklerkennzeichnungen vorhanden?
- Sind die Prompt-Dateien gepflegt?
- Sind SprintPlanning-Issues berücksichtigt?
- Sind Tests nachvollziehbar?
- Ist der Build erfolgreich?

---

# 26. Abschlussbericht

Nach jeder größeren Arbeitsphase soll ein verständlicher Bericht erstellt werden.

Er muss enthalten:

1. Was wurde untersucht?
2. Welche Probleme wurden gefunden?
3. Welche Funktionen wurden implementiert?
4. Welche bestehenden Funktionen wurden verbessert?
5. Welche Ideen aus dem SprintPlanning wurden übernommen?
6. Welche Klassen und Dateien wurden verändert?
7. Welche Tests wurden ausgeführt?
8. Welche Tests waren erfolgreich?
9. Welche Tests sind fehlgeschlagen?
10. Welche Prüfungen konnten nicht ausgeführt werden?
11. Welche Aufgaben sind noch offen?
12. Was sollte als Nächstes umgesetzt werden?

Behaupte niemals, dass eine Funktion fertig ist, wenn sie nur teilweise implementiert wurde.

Behaupte niemals, dass Tests erfolgreich waren, wenn sie nicht tatsächlich ausgeführt wurden.

---

# 27. Wichtige Grenzen für den KI-Agenten

Du darfst:

- Bestehende Klassen analysieren.
- Eigene Klassen im erlaubten Bereich erstellen.
- Bestehende eigene Klassen verbessern.
- Fehler beheben.
- Grafiken und Grafikzuordnungen verbessern.
- Spielmechaniken implementieren.
- Dokumentation ergänzen.
- Lokale SprintPlanning-Issues auswerten.
- Tests innerhalb der freigegebenen Struktur erstellen und ausführen.
- Vorschläge für weitere Verbesserungen machen.

Du darfst nicht:

- Framework-Dateien eigenmächtig verändern.
- Projektdateien eigenmächtig verändern.
- Eigene Fenster erstellen.
- Eine eigene Game-Loop erstellen.
- Eine eigene Main-Methode erstellen.
- Das OOPGames-Framework umgehen.
- Andere Gruppenordner verändern.
- Bestehende Teamänderungen überschreiben.
- Nicht vorhandene Schnittstellen erfinden.
- Nicht getestete Funktionen als fertig melden.
- Dokumentierte Aufgaben ohne Rücksprache löschen.
- Ungefragt GitHub-Issues erstellen oder schließen.
- Nicht freigegebene externe Bibliotheken einführen.

Wenn du für eine Aufgabe eine nicht erlaubte Änderung benötigst, erkläre genau, warum sie erforderlich wäre, und frage nach Freigabe.

---

# 28. Übergeordnete Entwicklungsziele

Bei jeder Entscheidung gelten folgende Prioritäten:

1. **Korrekte Funktion:** Das Spiel muss tatsächlich funktionieren.
2. **Framework-Kompatibilität:** Alle offiziellen Vorgaben müssen eingehalten werden.
3. **Modularität:** Jede Komponente muss eine klare Aufgabe haben.
4. **Grafische Qualität:** Alle Grafiken sollen zusammenpassen und sauber dargestellt werden.
5. **Benutzerfreundlichkeit:** Steuerung und Interaktionen müssen verständlich sein.
6. **Wartbarkeit:** Andere Teammitglieder müssen den Code verstehen können.
7. **Testbarkeit:** Wichtige Funktionen müssen überprüfbar sein.
8. **Dokumentation:** Änderungen und Entscheidungen müssen nachvollziehbar sein.

## Abschließende Anweisung

Arbeite nicht einfach nur die zuletzt genannte Aufgabe ab und ignoriere dabei ältere Anforderungen.

Berücksichtige immer den gesamten Projektstand, alle bereits dokumentierten Aufgaben und alle genannten Spielideen.

Prüfe zuerst, was bereits existiert, verbessere vorhandene Lösungen und ergänze fehlende Funktionen.

Das Ziel ist ein zusammenhängendes, hochwertiges und vollständig spielbares Casino-Spiel im Pokémon-Rubin-/Saphir-Stil, das technisch sauber mit dem OOPGames-Framework umgesetzt wurde.

**Jede Funktion muss nicht nur vorhanden sein, sondern tatsächlich funktionieren. Jede Änderung muss nachvollziehbar sein. Jede Aufgabe muss anhand des echten Implementierungsstands bewertet werden.**