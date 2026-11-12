# AGENTS.md – Verbindliche Projektregeln für unser Casino-Spiel

**Projekt:** Casino Adventure – Pokémon-Rubin-inspirierte Stadt- und Casino-Welt
**Studiengang:** Programmieren 3 – Wintersemester 2026/27  
**Framework:** OOPGames – WPF, C#, .NET 10 (Windows)

Diese Datei enthält die verbindlichen Regeln für alle Entwickler und KI-Agenten, die an diesem Projekt arbeiten.

Die offiziellen Projektanforderungen und das vorhandene Framework haben immer Vorrang vor zusätzlichen Projektwünschen.

---

# 1. Allgemeine Projektbeschreibung

Wir entwickeln ein umfangreiches 2D-Pixel-Art-Spiel, das sich optisch und spielerisch an Pokémon Rubin/Saphir für den Game Boy Advance orientiert.

Der Spieler startet in seinem Zuhause und erkundet von dort eine quadratische Stadt mit begehbaren Straßen und Gassen. Das Casino ist ein betretbares Gebäude in dieser Außenwelt; seine vorhandenen Spielbereiche und Minispiele bleiben erhalten.

Das Casino enthält verschiedene Spielbereiche, NPCs und interaktive Spieltische. An diesen Tischen kann der Spieler unterschiedliche Casino-Spiele gegen KI-Gegner spielen.

Das Spiel besteht aus verbundenen Bereichen:

1. Das Zuhause als Start- und Rückkehrort.
2. Die begehbare, 180x180 Kacheln große Stadt mit Casino, Krankenhaus, Supermarkt, weiteren Gebäuden, Fluss und Tierwelt.
3. Das Casino-Innere mit interaktiven Minispielen, eigenständiger Spiellogik und KI-Gegnern.
4. Ergänzende, eingerichtete Innenräume für Krankenhaus und Supermarkt.

Mindestens Zuhause und Casino benötigen betretbare Innenräume. Zusätzliche Gebäude dürfen als solide Fassaden oder mit klar abgegrenzten, eigenen Innenräumen umgesetzt werden.

Das Spiel soll einen hochwertigen, zusammenhängenden Eindruck vermitteln und deutlich umfangreicher als ein einfacher Brettspielklon sein.

Alle Bestandteile müssen vollständig innerhalb des vorgegebenen OOPGames-Frameworks funktionieren.

## 1.1 Grafischer Stil

Der gesamte visuelle Stil orientiert sich an Pokémon Rubin/Saphir für den Game Boy Advance.

Verbindliche Gestaltungsmerkmale:

- 2D-Pixel-Art.
- Top-Down-Perspektive.
- Klare Pixelkonturen.
- Einheitlicher Pixel-Art-Stil.
- Farblich abgestimmte Umgebungen.
- Animierte Spielfigur.
- Detailreiche Casino-Einrichtung.
- Wiedererkennbare NPCs.
- Einheitliche grafische Gestaltung aller Minispiele.

Es dürfen keine realistischen 3D-Grafiken oder unpassenden modernen Grafikelemente verwendet werden.

---

# 2. ABSOLUTE FRAMEWORK-REGELN

Diese Regeln haben höchste Priorität.

Das bestehende OOPGames-Framework darf niemals umgangen oder eigenständig ersetzt werden.

- Das Spiel muss vollständig innerhalb des vorgegebenen Frameworks laufen.
- Keine eigenen Windows-Forms-, WPF- oder sonstigen Fenster erstellen.
- Keine eigene Main-Methode erstellen.
- Keine eigene Hauptschleife oder Game-Loop implementieren.
- Keine eigene Framework-Initialisierung erstellen.
- Keine eigenen Eingabe-Handler außerhalb der vorgesehenen Schnittstellen erstellen.
- Eingaben ausschließlich über die vorgesehenen Framework-Klick- und Tastatur-Selections verarbeiten.
- Ausschließlich auf dem vom Framework übergebenen Canvas zeichnen.
- Keine zusätzlichen Render-Fenster erstellen.
- Keine zusätzlichen Zeichenflächen erstellen.
- Das Hauptfenster des Frameworks muss zur Auswahl und zum Start des Spiels verwendet werden.
- Keine Framework-Dateien verändern.
- Keine Framework-Erweiterungen ohne vorherige Zustimmung des Dozenten.

Es müssen die tatsächlich vorhandenen Framework-Schnittstellen verwendet werden.

Dazu gehören entsprechend ihrer tatsächlichen Verfügbarkeit:

- IPaintGame
- IGameRules
- IGameField
- IHumanGamePlayer
- IComputerGamePlayer
- IGameRules2
- IPaintGame2

Wichtig:

Vor jeder Implementierung müssen die vorhandenen Interface-Definitionen und Beispielimplementierungen untersucht werden.

Es dürfen keine Methoden, Properties, Events oder Parameter erfunden werden.

Die tatsächlichen Schnittstellen haben immer Vorrang vor den hier beschriebenen Architekturvorschlägen.

---

# 3. Erlaubte Verzeichnisse und Dateistruktur

Es dürfen ausschließlich Dateien innerhalb folgender Verzeichnisse erstellt oder verändert werden:

- Classes/<Gruppe>/
- Interfaces/<Gruppe>/

Die einzige Ausnahme ist die Registrierung der eigenen Klassen im vorgesehenen Bereich:

//REGISTER YOUR CLASSES HERE

in MainWindow.xaml.cs.

Diese Ausnahme erlaubt ausschließlich die Registrierung und keine sonstigen Änderungen.

Framework-Dateien und fremde Gruppenordner sind tabu.

Insbesondere dürfen niemals verändert werden:

- Basis-Interfaces in Interfaces/.
- Classes/OOPGames.cs.
- MainWindow.xaml und MainWindow.xaml.cs außerhalb des Registrierungsblocks.
- App.xaml und App.xaml.cs.
- .csproj und .sln.
- .vscode/.
- X_Griesbauer/.
- Fremde Gruppenverzeichnisse.

## 3.1 Vorgesehene Projektstruktur

Die folgende Struktur dient als Architekturvorschlag und muss an das tatsächlich vorhandene Projekt angepasst werden.

```text
Classes/
└── <Gruppe>/
    ├── AGENTS.md
    │
    ├── Plans/
    │   ├── Casino_Konzept.md
    │   ├── Architektur.md
    │   └── Grafiksystem.md
    │
    ├── Scrum/
    │   ├── ProductBacklog.md
    │   ├── SprintPlanning.md
    │   ├── SprintReviews.md
    │   └── Retrospectives.md
    │
    ├── <Vorname>_<Nachname>_Painter.cs
    ├── <Vorname>_<Nachname>_Rules.cs
    ├── <Vorname>_<Nachname>_Field.cs
    ├── <Vorname>_<Nachname>_HumanPlayer.cs
    ├── <Vorname>_<Nachname>_ComputerPlayer.cs
    │
    ├── <Vorname>_<Nachname>_CasinoWorld.cs
    ├── <Vorname>_<Nachname>_CasinoMap.cs
    ├── <Vorname>_<Nachname>_CollisionManager.cs
    ├── <Vorname>_<Nachname>_InteractionManager.cs
    │
    ├── <Vorname>_<Nachname>_Player.cs
    ├── <Vorname>_<Nachname>_PlayerMovement.cs
    ├── <Vorname>_<Nachname>_PlayerAnimation.cs
    │
    ├── <Vorname>_<Nachname>_Roulette.cs
    ├── <Vorname>_<Nachname>_Poker.cs
    ├── <Vorname>_<Nachname>_Blackjack.cs
    ├── <Vorname>_<Nachname>_Slots.cs
    │
    ├── <Vorname>_<Nachname>_RouletteAI.cs
    ├── <Vorname>_<Nachname>_PokerAI.cs
    ├── <Vorname>_<Nachname>_BlackjackAI.cs
    │
    ├── <Vorname>_<Nachname>_GameManager.cs
    ├── <Vorname>_<Nachname>_MoneyManager.cs
    ├── <Vorname>_<Nachname>_GameState.cs
    ├── <Vorname>_<Nachname>_AssetManager.cs
    │
    ├── <Vorname>_<Nachname>_GameHUD.cs
    ├── <Vorname>_<Nachname>_DialogueBox.cs
    └── <Vorname>_<Nachname>_CasinoMenu.cs
```

Jeder Studierende besitzt zusätzlich eine eigene Prompt-Datei:

`Classes/<Gruppe>/<Vorname>_<Nachname>_Prompts.md`

Diese liegt direkt im Hauptverzeichnis der Gruppe.

Die tatsächlichen Dateinamen müssen den offiziellen Namenskonventionen entsprechen.

Nicht benötigte Klassen dürfen entfallen. Zusätzliche Klassen dürfen nur erstellt werden, wenn sie eine klar definierte Aufgabe besitzen.

---

# 4. Architektur und objektorientierter Entwurf

Das Projekt muss sauber objektorientiert aufgebaut sein.

Jede Klasse erhält eine klar definierte Verantwortung.

Die Spiellogik muss von der Darstellung getrennt werden.

Die einzelnen Casino-Spiele müssen voneinander unabhängig sein.

Jedes Casino-Spiel wird in einer eigenen C#-Datei implementiert.

Beispiele:

- Roulette.cs
- Poker.cs
- Blackjack.cs
- Slots.cs

Jedes Spiel besitzt seine eigene Spiellogik, seine eigenen Regeln und seine eigenen KI-Entscheidungen.

Der Painter darf nicht die vollständige Spiellogik aller Minispiele enthalten.

Die Klassen müssen so aufgebaut sein, dass Änderungen an einem Casino-Spiel möglichst keine Änderungen an anderen Spielen erfordern.

## 4.1 Painter

Der Painter ist die zentrale Darstellungskomponente.

Er übernimmt:

- Zeichnen der Casino-Spielwelt.
- Zeichnen der Spielfigur.
- Zeichnen der NPCs.
- Zeichnen der Möbel und Spieltische.
- Zeichnen der Minispiele.
- Zeichnen der Benutzeroberfläche.
- Darstellung des aktuellen Spielzustands.

Der Painter verwendet ausschließlich das vom Framework übergebene Canvas.

Er darf nicht eigenständig eine zweite Zeichenumgebung erstellen.

## 4.2 Rules und Field

Die Rules verwalten den Spielablauf und die Spielregeln entsprechend dem OOPGames-Framework.

Das Field verwaltet die notwendigen Zustands- und Spielfeldinformationen.

Die genaue Aufteilung richtet sich nach den vorhandenen Framework-Interfaces.

Die Darstellung darf nicht die eigentliche Spielregelverwaltung ersetzen.

## 4.3 Player

Human-Player und Computer-Player werden entsprechend der vorgesehenen Framework-Interfaces implementiert.

Die Spielfigur in der Casino-Welt ist von der technischen Rolle des Human-Players zu unterscheiden.

Der Human-Player verarbeitet die vorgesehenen Benutzereingaben und stellt die erforderlichen Spielaktionen bereit.

Der Computer-Player übernimmt die KI-Entscheidungen der jeweiligen Spielkomponente.

---

# 5. Spielfigur und Bewegungssystem

Die Spielfigur ist ein zentraler Bestandteil des Spiels.

Sie soll sich ähnlich wie die Spielfigur in Pokémon Rubin/Saphir durch Zuhause, Außenwelt und Casino bewegen.

## 5.1 Größe und Darstellung

Die Spielfigur verwendet als vorgesehene logische Darstellungsgröße:

**90 × 90 Pixel.**

Diese Größe bezeichnet den vorgesehenen Darstellungsbereich der Figur und muss bei der tatsächlichen Canvas-Größe und Skalierung berücksichtigt werden.

Die Spielfigur darf nicht verzerrt dargestellt werden.

Das Seitenverhältnis muss erhalten bleiben.

Pixel-Art-Grafiken sollen möglichst ohne unscharfe Interpolation skaliert werden, sofern das Framework dies unterstützt.

## 5.2 Separate Grafiken für jede Laufrichtung

Für jede Laufrichtung muss eine eigene Sprite-Grafik existieren.

Mindestens folgende Richtungen sind erforderlich:

- Oben.
- Unten.
- Links.
- Rechts.

Für jede Richtung muss es eine eigene visuelle Darstellung geben.

Die Figur darf nicht einfach durch Spiegelung oder Rotation einer einzigen Grafik dargestellt werden, wenn dadurch die vorgesehene Charaktergestaltung verloren geht.

Die Ausrichtung muss eindeutig erkennbar sein.

## 5.3 Animationen

Jede Laufrichtung soll eigene Laufanimationen besitzen.

Vorgesehen sind:

- Idle-Animation nach oben.
- Idle-Animation nach unten.
- Idle-Animation nach links.
- Idle-Animation nach rechts.
- Laufanimation nach oben.
- Laufanimation nach unten.
- Laufanimation nach links.
- Laufanimation nach rechts.

Die Animationen bestehen aus mehreren Einzelbildern.

Der Wechsel der Einzelbilder muss an die vom Framework vorgesehenen Aktualisierungs- und Zeichenaufrufe angepasst werden.

Keine eigene Game-Loop zur Animation erstellen.

Die Animation darf nicht unabhängig vom Framework laufen.

## 5.4 Bewegungssteuerung

Die Bewegung erfolgt über die vorgesehenen Tastatur-Selections.

Vorgesehene Steuerung:

- W oder Pfeil nach oben: nach oben laufen.
- S oder Pfeil nach unten: nach unten laufen.
- A oder Pfeil nach links: nach links laufen.
- D oder Pfeil nach rechts: nach rechts laufen.
- Interaktionstaste: mit Objekten und NPCs interagieren.

Es dürfen nur tatsächlich vom Framework unterstützte Eingabemöglichkeiten verwendet werden.

Die Bewegung muss flüssig und nachvollziehbar sein.

Der Spieler darf nicht durch Wände, Möbel oder andere nicht begehbare Objekte laufen.

---

# 6. Kollisionssystem

Das Spiel benötigt ein zuverlässiges Kollisionssystem.

## 6.1 Nicht durchdringbare Objekte

Folgende Objekte müssen Kollisionen besitzen:

- Außenwände.
- Innenwände.
- Säulen.
- Tresen.
- Spieltische.
- Spielautomaten.
- Schränke.
- Dekorationen mit physischer Begrenzung.
- Andere nicht begehbare Objekte.

Die Spielfigur darf diese Objekte und die geschlossene Außengrenze der Stadtkarte nicht durchdringen.

## 6.2 Kollisionsabfrage

Das Kollisionssystem muss:

- Die aktuelle Position der Spielfigur berücksichtigen.
- Die Größe der Spielfigur berücksichtigen.
- Die Kollisionsbereiche der Umgebung berücksichtigen.
- Bewegungen vor einer Kollision verhindern.
- Bewegungen entlang einer Wand ermöglichen.
- Ungewolltes Feststecken vermeiden.
- Kollisionen unabhängig von der Bewegungsrichtung korrekt erkennen.

Wenn das Framework keine vorgefertigte Kollisionsfunktion besitzt, darf innerhalb des erlaubten Gruppenordners eine eigene Kollisionslogik implementiert werden.

Es darf dafür keine Framework-Datei verändert werden.

## 6.3 Begehbare Bereiche

Jede Szene muss klar definierte begehbare und nicht begehbare Bereiche besitzen. Die Außenwelt benötigt eine sichtbare, geschlossene und kollidierbare Kartengrenze.

Die Spielfigur darf sich ausschließlich innerhalb der vorgesehenen begehbaren Fläche bewegen.

Die Karte muss so gestaltet sein, dass alle wichtigen Bereiche erreichbar sind.

---

# 7. Austauschbares Grafiksystem

Eine der wichtigsten Anforderungen ist die vollständige Austauschbarkeit der Grafiken.

**Keine Grafik darf fest und untrennbar in die Spiellogik eingebaut werden.**

Alle grafischen Elemente müssen so organisiert sein, dass sie später ohne Änderung der Spielmechanik ausgetauscht werden können.

## 7.1 Austauschbare Grafikelemente

Dies gilt mindestens für:

- Spielfigur.
- Spielfigur-Sprites für jede Laufrichtung.
- Laufanimationen.
- Casino-Boden.
- Wände.
- Türen.
- Tische.
- Stühle.
- Spielautomaten.
- Roulette-Tische.
- Poker-Tische.
- Blackjack-Tische.
- NPCs.
- Dialogfenster.
- Karten.
- Spielchips.
- Symbole.
- Benutzeroberfläche.
- Hintergrundgrafiken.
- Stadtboden, Straßen, Wege, Stadtgrenze und Gebäudefassaden.

## 7.2 Trennung von Grafik und Spiellogik

Die Spielmechanik darf nicht von bestimmten Bilddateien abhängig sein.

Beispielsweise darf ein Roulette-Spiel nicht davon abhängig sein, wie die Roulette-Grafik aussieht.

Die Roulette-Klasse verwaltet die Regeln und Ergebnisse.

Die Darstellungskomponente verwendet die zugehörigen Grafiken.

Ein Austausch der Roulette-Grafik darf keine Änderung der Roulette-Spielregeln benötigen.

## 7.3 Grafikverwaltung

Es soll ein zentrales Grafikverwaltungssystem innerhalb des erlaubten Gruppenordners vorgesehen werden.

Dieses System soll:

- Grafiken zentral zuordnen.
- Grafiken anhand eindeutiger Namen identifizieren.
- Grafiken austauschbar machen.
- Sprites und Animationen verwalten.
- Grafische Ressourcen möglichst wiederverwenden.
- Fehlende Grafiken nachvollziehbar behandeln.

Beispielhafte Ressourcenbezeichnungen:

- player_idle_up
- player_idle_down
- player_idle_left
- player_idle_right
- player_walk_up
- player_walk_down
- player_walk_left
- player_walk_right
- casino_floor
- casino_wall
- roulette_table
- poker_table
- blackjack_table
- slot_machine

Die tatsächliche Lade- und Speichertechnik muss an das bestehende Framework angepasst werden.

Es dürfen keine zusätzlichen externen Fenster oder Rendering-Systeme erstellt werden.

Grafikdateien dürfen nur dort abgelegt werden, wo dies mit den offiziellen Projektvorgaben und der vorhandenen Ressourcenverwaltung vereinbar ist. Falls die Vorgaben keine zusätzlichen Asset-Verzeichnisse erlauben, muss vor einer Erweiterung Rücksprache mit dem Dozenten gehalten werden.

---

# 8. Stadt- und Casino-Spielwelt

Die begehbare Spielwelt besteht aus dem Zuhause, einer quadratischen Außenwelt und dem vorhandenen Casino-Innenraum. Alle Szenen werden in Top-Down-Perspektive und im einheitlichen 2D-Pixel-Art-Stil dargestellt.

## 8.1 Außenwelt

- Die Spielfigur startet im eigenen Zuhause und kann dieses betreten sowie verlassen.
- Die Stadtkarte ist quadratisch und 180x180 Kacheln groß (100-fache Fläche der ursprünglichen 18x18-Karte).
- Eine mit der Spielfigur scrollende Kamera zeigt nur einen viewportgroßen Ausschnitt, folgt flüssig und begrenzt sich an allen Kartenrändern.
- Dichte Baumreihen und Wasser bilden eine sichtbare, geschlossene, nicht begehbare Außengrenze.
- Straßen, Gassen, Parkwege und Brücken bilden ein verbundenes Wegenetz zwischen Zuhause, Casino und Stadtbereichen.
- Ein gewundener Fluss mit begehbaren Ufern verläuft durch die Stadt; Brücken überqueren ihn an den Straßen und Fußwegen.
- Mindestens Zuhause, Casino, Krankenhaus und Supermarkt müssen von außen erkennbar sein.
- Gebäude, Mauern, Bäume und andere Hindernisse besitzen unabhängig von ihrer Grafik Kollisionen.
- Zuhause und Casino sind betretbar und über eindeutige Türfelder wieder verlassbar.
- Krankenhaus und Supermarkt besitzen eingerichtete Innenräume mit eigenen Bodendaten, Möbelkollisionen und Türübergängen; weitere Gebäude dürfen solide Fassaden sein.
- Enten und Fische befinden sich im oder am Wasser; Katzen und Vögel beleben Wege, Bäume und Plätze, ohne Kollisionen oder Interaktionen auszulösen.
- Mindestens vier Stadt-NPCs sind über getrennte Bezirke der Außenwelt verteilt und bewegen sich eigenständig mit variierenden Pausen.
- NPC-Laufwege bleiben auf begehbaren Kacheln innerhalb ihrer Bereiche, prüfen Gebäude, Dekoration, andere NPCs und den Spieler als Hindernisse und meiden Türfelder.
- Die Stadt-NPCs sind über die Interaktionstaste ansprechbar und verwenden das vorhandene Gesprächssystem; Casino-NPC-Positionen und Casino-Interaktionen bleiben erhalten.
- Weltkarte, Kamera, Darstellung, Kollisionen, Szenenwechsel und Tieranimation bleiben getrennt; die Karte wird effizient im niedrigen Pixelmaßstab zwischengespeichert.

## 8.2 Casino-Innenraum

Das bestehende Casino bleibt als eigener Innenraum mit Eingangsbereich, Empfang, Roulette-, Poker-, Blackjack- und Spielautomatenbereichen, NPCs und Dekoration erhalten. Der Übergang aus der Stadt führt in diesen vorhandenen Innenraum; der Ausgang führt zurück an die Casino-Tür in der Außenwelt.

Spieltische und Möbel müssen sinnvoll positioniert sein. Die Umgebung muss ausreichend Platz bieten und darf nicht wie ein einfacher leerer Raum wirken.

## 8.3 Weitere Innenräume

Krankenhaus und Supermarkt dürfen eigene kleine, ausgestattete Szenen erhalten. Jede Szene benötigt eine passende Bodentextur, eine geschlossene Außenwand, ein Ausgangsfeld, kollidierbare Einrichtung und einen eindeutigen Übergang zurück zur zugehörigen Außentür.

---

# 9. Interaktionssystem

Der Spieler muss mit NPCs und Objekten interagieren können.

Interaktionen erfolgen über die vorgesehenen Framework-Eingaben.

Vorgesehene Interaktionen:

- Mit NPCs sprechen.
- Roulette starten.
- Poker starten.
- Blackjack starten.
- Spielautomaten bedienen.
- Mit der Kasse interagieren.
- Dialoge bestätigen.
- Minispiele verlassen.

Vor einer Interaktion muss geprüft werden, ob sich der Spieler im entsprechenden Interaktionsbereich befindet.

Interaktionsbereiche müssen unabhängig von der Grafik verwaltet werden.

Ein Austausch der Grafik eines Spieltisches darf nicht dazu führen, dass die Interaktion nicht mehr funktioniert.

---

# 10. Casino-Minispiele

Jedes Casino-Spiel muss in einer eigenen C#-Datei implementiert werden.

Jedes Spiel muss eine eigenständige Spiellogik besitzen.

Alle Minispiele müssen vollständig in das zentrale OOPGames-Spiel integriert werden.

Die Minispiele dürfen keine eigenen Fenster oder Game-Loops verwenden.

## 10.1 Roulette

Funktionen:

- Roulette-Tisch.
- Zahlen von 0 bis 36.
- Rote und schwarze Zahlen.
- Grüne Null.
- Einsatzmöglichkeiten auf einzelne Zahlen.
- Einsatzmöglichkeiten auf Farben.
- Einsatzmöglichkeiten auf Zahlenbereiche.
- Einsatzverwaltung.
- Zufällige Ermittlung des Ergebnisses.
- Korrekte Gewinnberechnung.
- Korrekte Verlustberechnung.
- KI-Gegner.
- Darstellung des Ergebnisses.
- Runde abschließen und zurückkehren.

## 10.2 Poker

Die verwendete Poker-Variante muss vor der Implementierung festgelegt werden.

Vorgesehene Funktionen:

- Kartenverteilung.
- Spielerhand.
- Gemeinschaftskarten, falls die gewählte Variante diese verwendet.
- Einsatzsystem.
- Fold.
- Call.
- Raise.
- KI-Gegner.
- Bewertung der Pokerhände.
- Ermittlung des Gewinners.
- Anzeige des Spielergebnisses.

Die KI muss abhängig von ihrer Hand und der Spielsituation Entscheidungen treffen.

## 10.3 Blackjack

Funktionen:

- Kartenausgabe.
- Spielerhand.
- Dealer-Hand.
- Hit.
- Stand.
- Blackjack-Erkennung.
- Bust-Erkennung.
- Berechnung der Kartenwerte.
- KI-Entscheidungslogik.
- Gewinnerermittlung.
- Einsatzabrechnung.

## 10.4 Spielautomaten

Funktionen:

- Mindestens drei Walzen.
- Unterschiedliche Symbole.
- Zufällige Ergebnisse.
- Unterschiedliche Gewinnkombinationen.
- Einsatzsystem.
- Auszahlungssystem.
- Gewinnanimation, sofern mit dem Framework möglich.

Alle Casino-Spiele müssen einheitlich gestaltet sein, dürfen aber ihre eigenen Regeln besitzen.

---

# 11. KI-Gegner

Die KI-Gegner müssen eine eigenständige und nachvollziehbare Entscheidungslogik besitzen.

Sie dürfen nicht einfach zufällig gewinnen oder verlieren.

Die KI soll spielabhängig Entscheidungen treffen.

## Roulette-KI

- Eigene Einsatzstrategie.
- Verschiedene Einsatzmöglichkeiten.
- Risikobewertung.
- Berücksichtigung des verfügbaren Spielgeldes.

Es darf keine Gewinnstrategie behauptet werden, die mathematisch nicht existiert.

## Poker-KI

- Bewertung der eigenen Karten.
- Bewertung der aktuellen Spielsituation.
- Berücksichtigung der Gemeinschaftskarten.
- Entscheidungen über Fold, Call und Raise.
- Berücksichtigung des verfügbaren Einsatzes.

## Blackjack-KI

- Bewertung der eigenen Karten.
- Berücksichtigung der sichtbaren Dealer-Karte.
- Strategische Entscheidungen über Hit und Stand.

Die KI-Logik muss modular aufgebaut sein und darf nicht vollständig im Painter implementiert werden.

---

# 12. Spielphasen und Spielzustände

Das Spiel muss über eindeutig definierte Zustände verfügen.

Mindestens folgende Zustände sind vorgesehen:

- WorldExploration (Zuhause, Außenwelt oder Casino-Innenraum; der aktuelle Ort ist getrennt vom UI-/Minispielzustand)
- Dialogue
- Roulette
- Poker
- Blackjack
- Slots
- Result

Beim Start eines Minispiels:

- Wird die normale Bewegung deaktiviert.
- Wird das ausgewählte Minispiel aktiviert.
- Werden die erforderlichen Daten übergeben.
- Wird die Darstellung auf das Minispiel umgestellt.

Nach Beendigung:

- Wird das Spielergebnis angezeigt.
- Wird das Spielgeld aktualisiert.
- Wird der Zustand zurückgesetzt.
- Kehrt der Spieler in die Szene zurück, aus der das Minispiel gestartet wurde.
- Wird die Bewegung wieder aktiviert.

Die Zustandsverwaltung muss zentral und nachvollziehbar erfolgen.

---

# 13. Benutzeroberfläche

Die Benutzeroberfläche muss zum Pokémon-Rubin-Pixel-Art-Stil passen.

Sie muss ausschließlich auf dem vom Framework übergebenen Canvas dargestellt werden.

Vorgesehene Elemente:

- Kontostand.
- Interaktionshinweise.
- Dialogfenster.
- NPC-Namen.
- Einsatzanzeige.
- Kartenanzeige.
- Spielinformationen.
- Gewinn- und Verlustanzeige.
- Spielmenüs.
- Ergebnisanzeige.

Die Benutzeroberfläche muss übersichtlich und einheitlich gestaltet sein.

Grafische UI-Elemente müssen ebenfalls austauschbar sein.

---

# 14. Modulare Klassenstruktur

Diese Regel gilt für den gesamten Quellcode des Projekts.

Alle Klassen müssen modular und unabhängig voneinander aufgebaut sein.

- Jede Klasse besitzt eine klar definierte Aufgabe.
- Klassen dürfen nicht unnötig voneinander abhängig sein.
- Spiellogik, Grafik, Eingabeverarbeitung und Datenverwaltung müssen getrennt werden.
- Wiederkehrende Funktionen sollen zentral und wiederverwendbar implementiert werden.
- Änderungen an einer Klasse sollen möglichst keine Anpassungen an anderen Klassen erfordern.
- Jede Casino-Spielklasse muss eigenständig funktionieren und darf nicht die interne Implementierung eines anderen Minispiels voraussetzen.
- Der Painter darf keine vollständige Spiellogik enthalten.
- Grafiken müssen unabhängig von der Spiellogik austauschbar bleiben.
- Abhängigkeiten zwischen Klassen müssen nachvollziehbar sein.
- Bestehende Klassen dürfen nicht ohne Grund zusammengeführt werden.
- Große Klassen müssen bei Bedarf in kleinere, fachlich sinnvolle Klassen aufgeteilt werden.

Jede Klasse soll nur die Daten und Methoden enthalten, die zu ihrer Aufgabe gehören.

Keine unnötigen Hilfsklassen oder überdimensionierten Manager erstellen.

Die Architektur muss für alle Teammitglieder verständlich bleiben.

---

# 15. Entwicklerkennzeichnung im Quellcode

Jeder neu erstellte oder wesentlich veränderte Codeabschnitt muss eindeutig dem verantwortlichen Teammitglied zugeordnet werden.

Dafür werden Kommentare mit dem vollständigen Vor- und Nachnamen verwendet.

## 15.1 Kennzeichnung neuer Klassen

Beispiel:

```csharp
// =====================================================
// Klasse: CAS_Roulette
// Entwickler: Max Mustermann
// Erstellt: 12.11.2026
// Beschreibung: Enthält die Spiellogik für Roulette.
// =====================================================
```

## 15.2 Kennzeichnung neuer Methoden

Beispiel:

```csharp
// Entwickler: Max Mustermann
// Aufgabe: Berechnet den Gewinn des Spielers.
private void CalculateWinnings()
{
    // Implementierung
}
```

## 15.3 Kennzeichnung veränderter Codeabschnitte

Beispiel:

```csharp
// Änderung durch: Max Mustermann
// Datum: 12.11.2026
// Änderung: Kollisionsprüfung für Casino-Wände erweitert.
```

## 15.4 Änderungen durch mehrere Entwickler

Wenn mehrere Teammitglieder an derselben Klasse arbeiten:

- Jeder Entwickler kennzeichnet seine eigenen neu erstellten oder wesentlich veränderten Methoden.
- Bei Änderungen innerhalb einer bestehenden Methode wird die Änderung an der entsprechenden Stelle dokumentiert.
- Der ursprüngliche Entwickler darf nicht nachträglich als Autor fremder Änderungen eingetragen werden.
- Gemeinsame Implementierungen müssen mit allen tatsächlich beteiligten Entwicklern gekennzeichnet werden.
- Bestehende Entwicklerkennzeichnungen müssen erhalten bleiben.

Nicht jede einzelne Codezeile benötigt einen Kommentar. Die Kennzeichnung soll auf Klassen-, Methoden- und relevanter Änderungsebene erfolgen.

## 15.5 KI-generierter Code

KI-generierter Code muss ebenfalls einem verantwortlichen Teammitglied zugeordnet werden.

Der eingetragene Name bezeichnet das Teammitglied, das den Code geprüft, verstanden und in das Projekt übernommen hat.

Eine KI darf niemals als menschlicher Entwickler ausgegeben werden.

KI-Unterstützung muss zusätzlich in der persönlichen Prompt-Datei des verantwortlichen Teammitglieds dokumentiert werden.

Ist der verantwortliche Entwickler nicht bekannt, muss der Agent vor der Implementierung nach dem vollständigen Namen fragen.

## 15.6 Git und Nachvollziehbarkeit

Die Entwicklerkennzeichnungen ersetzen nicht die Git-Historie.

Jedes Teammitglied muss weiterhin mit dem eigenen GitHub-Account committen und pushen.

Die Entwicklerkennzeichnungen, persönlichen Prompt-Dateien und Git-Commits müssen nachvollziehbar zusammenpassen.

Ziel ist, dass jederzeit erkennbar ist:

- Wer eine Klasse erstellt hat.
- Wer eine Methode implementiert hat.
- Wer eine bestehende Funktion verändert hat.
- Wer eine KI-generierte Implementierung geprüft und übernommen hat.

---

# 16. Codequalität und Wartbarkeit

- Jede Klasse hat eine klar definierte Aufgabe.
- Keine riesigen Klassen mit sämtlicher Spiellogik.
- Keine unnötigen globalen Variablen.
- Keine doppelten Implementierungen.
- Möglichst geringe Kopplung zwischen den Minispielen.
- Aussagekräftige Namen für Klassen und Methoden.
- Kommentare für komplexe Algorithmen.
- Fehlerbehandlung bei ungültigen Eingaben.
- Trennung von Darstellung und Logik.
- Wiederverwendbare Komponenten.
- Austauschbare Grafiken.
- Keine unnötigen Framework-Abhängigkeiten.
- Keine unnötigen Änderungen an bestehenden Klassen.

Jedes Teammitglied muss den gesamten eigenen Code erklären können, einschließlich agentisch erzeugtem Code.

---

# 17. Regeln für KI-Agenten und VS Code

Bevor neue Funktionen implementiert werden:

1. Bestehenden Code untersuchen.
2. Framework-Schnittstellen überprüfen.
3. Beispielprojekt X_Griesbauer analysieren.
4. Vorhandene Klassen untersuchen.
5. Abhängigkeiten verstehen.
6. Prüfen, welche Dateien verändert werden dürfen.
7. Verantwortlichen Entwickler feststellen.
8. Einen konkreten Implementierungsplan erstellen.

Bei Änderungen:

- Nur tatsächlich benötigte Dateien bearbeiten.
- Nur erlaubte Verzeichnisse verwenden.
- Keine Framework-Dateien verändern.
- Keine fremden Gruppenordner bearbeiten.
- Keine vorhandenen Funktionen unnötig ersetzen.
- Keine zusätzlichen Framework-Abhängigkeiten einführen.
- Keine Methoden oder Interfaces erfinden.
- Keine Platzhalter als fertige Funktionen deklarieren.
- Bestehende Entwicklerkennzeichnungen erhalten.
- Neue oder wesentlich veränderte Methoden kennzeichnen.
- Klassen modular halten.
- Kompilierbarkeit nach größeren Änderungen überprüfen.

Wenn eine Funktion eine Framework-Erweiterung benötigt, muss die Implementierung abgebrochen werden.

Der Agent muss beschreiben:

- Welche Erweiterung notwendig wäre.
- Warum sie benötigt wird.
- Welche Framework-Dateien betroffen wären.
- Welche Alternative ohne Framework-Änderung möglich ist.

Die Entscheidung liegt beim Team und beim Dozenten.

---

# 18. SCRUM und Dokumentation

Die Entwicklung erfolgt nach SCRUM.

Die Dokumentation liegt unter:

`Classes/<Gruppe>/Scrum/`

Sie enthält:

- Product Owner.
- Scrum Master.
- Product Backlog.
- User Stories.
- Sprintplanung.
- Sprintziele.
- Verantwortliche je Aufgabe.
- Kurzprotokolle der Abstimmungen.
- Sprint Reviews.
- Sprint Retrospektiven.

Die Sprints dauern ein bis zwei Wochen.

Jedes Teammitglied muss einen sichtbaren und substanziellen Beitrag leisten.

Alle Prompts müssen pro Studierendem dokumentiert werden.

Die Prompts werden im Wortlaut mit Datum, Werkzeug, Modell und Ergebnisvermerk gespeichert.

Die Pläne werden unter `Classes/<Gruppe>/Plans/` abgelegt.

Das Spielkonzept muss vor Beginn der Implementierung dem Dozenten vorgestellt werden.

Backlog, Prompts, Pläne und Git-Commits müssen zusammenpassen.

---

# 19. Git und Build-Regeln

Vor jedem Commit:

1. `git status` ausführen.
2. `git diff` kontrollieren.
3. Änderungen auf unerlaubte Dateien prüfen.
4. Sicherstellen, dass keine Framework-Dateien verändert wurden.
5. `dotnet build` ausführen.

Vor jedem Push:

1. `git pull` ausführen.
2. `dotnet build` ausführen.
3. Änderungen erneut prüfen.
4. Eigenen Commit erstellen.
5. Mit dem eigenen GitHub-Account pushen.

Keine Force-Pushes.

Keine bin/- oder obj/-Dateien committen.

Jedes Teammitglied committet selbst.

Commits müssen kleine, nachvollziehbare Änderungen enthalten.

Der KI-Agent darf keine Commits unter fremdem Namen erstellen oder pushen.

---

# 20. Verbindliche Entwicklungsreihenfolge

Die Implementierung soll in folgender Reihenfolge erfolgen.

## Phase 1: Framework-Analyse

- Vorhandene Interfaces untersuchen.
- Beispielprojekt untersuchen.
- Painter, Rules und Player verstehen.
- Registrierungsmöglichkeiten untersuchen.
- Erlaubte Architektur festlegen.

## Phase 2: Grundstruktur

- Painter implementieren.
- Rules implementieren.
- Field implementieren.
- Human-Player implementieren.
- Computer-Player implementieren.
- Registrierung vorbereiten.

## Phase 3: Zuhause, Außenwelt und Casino-Spielwelt

- Begehbare Startszene im Zuhause erstellen.
- Quadratische 180x180 Außenkarte mit geschlossener Baum-/Wassergrenze, Fluss, Brücken und verbundenen Straßen erstellen.
- Scrollende, begrenzte Kamera und sichtbaren Kartenausschnitt implementieren.
- Casino, Krankenhaus, Supermarkt und weitere Stadtbereiche erkennbar darstellen.
- Übergänge zwischen Zuhause, Stadt, Casino und optionalen Gebäudeinnenräumen implementieren.
- Begehbare Bereiche und Kollisionen für alle Szenen definieren.
- Nicht kollidierende Tierdarstellung und tickbasierte Animation ergänzen.

## Phase 4: Spielfigur

- Spielfigur implementieren.
- Vier Laufrichtungen integrieren.
- Idle-Sprites integrieren.
- Laufanimationen integrieren.
- Bewegung implementieren.
- Kollisionen testen.
- Kartengrenzen, Flussufer und Brücken testen.
- Kamerafahrt und Randbegrenzung testen.
- Tierdarstellung und zus\u00e4tzliche Innenraumübergänge testen.

## Phase 5: Grafiksystem

- Austauschbares Grafiksystem implementieren.
- Ressourcen zentral verwalten.
- Grafiken von der Spiellogik trennen.
- Austauschbarkeit testen.

## Phase 6: Interaktionen

- NPCs implementieren.
- Spieltische integrieren.
- Interaktionsbereiche erstellen.
- Dialogsystem implementieren.

## Phase 7: Casino-Minispiele

- Roulette implementieren.
- Blackjack implementieren.
- Poker implementieren.
- Spielautomaten implementieren.

## Phase 8: KI

- KI-Gegner integrieren.
- Entscheidungslogik implementieren.
- Spielstrategien testen.

## Phase 9: Gesamtsystem

- Spielzustände verbinden.
- Geldsystem integrieren.
- Benutzeroberfläche fertigstellen.
- Übergänge zwischen Zuhause, Außenwelt, Casino und Minispielen testen.

## Phase 10: Qualitätssicherung

- Build überprüfen.
- Kollisionen testen.
- Eingaben testen.
- Minispiele testen.
- KI testen.
- Grafikaustausch testen.
- Gesamte Framework-Integration überprüfen.

---

# 21. Abschließende Prioritäten

Die höchste Priorität hat immer die vollständige Kompatibilität mit dem vorgegebenen OOPGames-Framework.

Danach folgen:

1. Saubere objektorientierte Architektur.
2. Modulare Klassen mit klaren Verantwortlichkeiten.
3. Funktionierende, verbundene Zuhause-, Stadt- und Casino-Spielwelt.
4. Spielfigur mit vier Laufrichtungen und Animationen.
5. Zuverlässiges Kollisionssystem.
6. Vollständig austauschbare Grafiken.
7. Funktionierende Casino-Minispiele.
8. Anspruchsvolle KI-Gegner.
9. Hochwertige Pixel-Art-Darstellung.
10. Vollständige Entwickler- und SCRUM-Dokumentation.

Keine zusätzlichen Features dürfen die Framework-Kompatibilität gefährden.

## Verbindlicher Grundsatz

Das Spiel soll wie ein eigenständiges Pokémon-Rubin-inspiriertes Casino-Abenteuer wirken, technisch aber vollständig und ausschließlich als reguläre OOPGames-Komponente funktionieren.

Alle Klassen müssen modular, nachvollziehbar und austauschbar sein.

Jede wesentliche Codeänderung muss dem tatsächlich verantwortlichen Teammitglied zugeordnet werden können.

Alle diese Regeln sind bei jeder Implementierung verbindlich.

Bei Bedarf soll die Agents.md erweitern werden. Die Grundregeln dürfen nicht verändert werden. Pushen soll nur auf den jetzigen Branch sein, kein merge.