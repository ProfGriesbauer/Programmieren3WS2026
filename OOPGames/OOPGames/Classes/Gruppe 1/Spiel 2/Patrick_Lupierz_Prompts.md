# Prompt-Protokoll – Casino (Spiel 2)

## 10-11-2026

### Prompt 1
befolge die regeln Rules_framework im Ordner Gruppe 1 wir wollen ein 2d Videospiel bauen in dem man ein casino spiel hat erstelle mir eine lauffähige erste version

Werkzeug: Claude Code (VS-Code-Erweiterung)
Modell: Claude Opus 5.5, im Verlauf der Sitzung Claude Fable 5.2

Ergebnisvermerk: Erste Version des Casino-Spiels unter `Classes/Gruppe 1/Spiel 2/` erstellt (Casino-Welt, Roulette, Blackjack, Poker, Spielautomat, KI-Gegner, austauschbare Grafiken). `dotnet build`: 0 Fehler, 0 Warnungen. Automatischer Testlauf mit 651 Prüfungen erfolgreich; echte Tastatur-/Mausbedienung im Hauptfenster nicht getestet. Kein Commit.

### Prompt 2
es darf nichts in der mainwidow geändert werden ausschlieslich nur in dem ordner spiel 2

Werkzeug: Claude Code (VS-Code-Erweiterung)
Modell: Claude Fable 5.2

Ergebnisvermerk: Alle Änderungen liegen in `Spiel 2`; der Eintrag in `Classes/Gruppe 1/Patrick_lupierz_prompts.md` wurde zurückgenommen und das Prompt-Protokoll hierher verlegt. Die Registrierung wurde zunächst ebenfalls entfernt (siehe Prompt 3).

### Prompt 3
es darf unter der registrierung eingefügt werden was anderes nicht

Werkzeug: Claude Code (VS-Code-Erweiterung)
Modell: Claude Fable 5.2

Ergebnisvermerk: Painter, Rules und Player wurden mit je einer Zeile im Block `//REGISTER YOUR CLASSES HERE` der MainWindow registriert. Sonst wurde außerhalb von `Spiel 2` nichts geändert. `dotnet build`: 0 Fehler, 0 Warnungen.

## 12-11-2026

### Prompt 4

Werkzeug: Claude Code (VS-Code-Erweiterung)
Modell: Claude Fable 5.2

```text
# CASINO ADVENTURE – Vollständiger Entwicklungsauftrag

## Dynamische Spielwelt, NPC-System, Flipphone, Charakteranpassung, Geschäfte, Tag-Nacht-Zyklus und Speichersystem

# 1. Projektbeschreibung und Ziel

Erweitere unser bestehendes Casino Adventure zu einem zusammenhängenden 2D-Pixel-Art-RPG, das sich optisch und spielerisch an Pokémon Rubin/Saphir für den Game Boy Advance orientiert.

Das ursprüngliche Casino bleibt ein zentraler Bestandteil des Spiels. Die Spielwelt soll jedoch um weitere Gebäude, NPCs, Geschäfte, Aufgaben und soziale Interaktionen erweitert werden.

**Das Ziel ist eine lebendige Spielwelt, in der Spieler eigene Entscheidungen treffen können, die dauerhaft Auswirkungen auf die Welt und ihre Bewohner haben.**

Wichtige Grundprinzipien:

1. Bereits vorhandene Funktionen und Grafiken erhalten.
2. Den bestehenden Pixel-Art-Stil konsequent fortführen.
3. NPCs sollen unterschiedliche Persönlichkeiten und Erinnerungen besitzen.
4. Entscheidungen beeinflussen Freundschaften, Rivalitäten und Feindschaften.
5. Die Spielwelt besitzt eine zentrale Ingame-Uhr.
6. Der Spieler kann sein Aussehen individuell gestalten.
7. NPCs können angerufen werden und auf Ereignisse reagieren.
8. Mehrere getrennte Kartenbereiche bilden die Spielwelt.
9. Der Fortschritt kann gespeichert und geladen werden.
10. Das gesamte Spiel verwendet ausschließlich Casino-Chips als Währung.

# 2. Verbindliche Projektregeln und AGENTS.md

Lies vor Beginn die vollständige AGENTS.md.

Untersuche anschließend die bestehende Projektstruktur, die vorhandenen NPCs, das Rendering, die Spielzustände und alle relevanten OOPGames-Interfaces.

Die AGENTS.md und die offiziellen Vorgaben des Dozenten haben Vorrang vor diesem Entwicklungsauftrag.

Besonders wichtig:

- Keine eigenen WPF- oder Windows-Forms-Fenster erstellen.
- Keine eigene Main-Methode erstellen.
- Keine unabhängige Game-Loop implementieren.
- Keine zusätzlichen Render-Canvas erstellen.
- Keine Framework-Dateien verändern.
- Keine Framework-Interfaces eigenständig erweitern.
- Keine eigenen Eingabe-Handler außerhalb der vorgesehenen Framework-Schnittstellen erstellen.
- Ausschließlich die tatsächlich vorhandenen Framework-Eingabemöglichkeiten verwenden.
- Sämtliche grafischen Elemente über den vom Framework bereitgestellten Canvas darstellen.
- Klassen modular halten.
- Spiellogik von Darstellung und Eingabeverarbeitung trennen.
- Nur erlaubte Projektverzeichnisse verändern.
- Bestehende Entwicklerkennzeichnungen erhalten.
- Neue oder veränderte Implementierungen entsprechend den Projektregeln kennzeichnen.
- KI-Unterstützung in den vorgesehenen Prompt-Dateien dokumentieren.
- Vorhandene Funktionen nicht unnötig ersetzen.

**Wichtiger Konzeptkonflikt:**

Die bisherige AGENTS.md sieht ursprünglich ausschließlich ein Casino-Gebäude als Spielbereich vor.

Unser neues Konzept sieht mehrere getrennte Kartenbereiche vor.

Analysiere deshalb zunächst, welche Anforderungen mit den bisherigen Projektregeln vereinbar sind und für welche Erweiterungen eine Freigabe des Teams beziehungsweise Dozenten erforderlich ist.

Ändere die AGENTS.md oder andere geschützte Dateien nicht eigenmächtig.

Falls eine Erweiterung die aktuellen Regeln verletzt, kennzeichne sie im Implementierungsplan als genehmigungspflichtig. Schlage eine regelkonforme Alternative vor und implementiere sie erst nach der erforderlichen Freigabe.

# 3. Die Spielwelt – mehrere getrennte Kartenbereiche

Die Spielwelt soll nicht aus einer einzigen großen, dauerhaft geladenen Karte bestehen.

Stattdessen besteht sie aus verschiedenen voneinander getrennten Karten.

Geplante Kartenbereiche:

1. Casino.
2. Außenbereich beziehungsweise Straßen.
3. Spielerhaus.
4. Tankstelle.
5. Bekleidungsgeschäft.
6. Friseursalon.
7. Weitere Geschäfte.
8. Zusätzliche Innenräume und Aufenthaltsbereiche.

Jede Karte soll ein eigenes Layout, eigene Kollisionen, Objekte, NPC-Positionen und Interaktionsbereiche besitzen.

## 3.1 Kartenwechsel

Der Spieler kann beispielsweise das Casino verlassen und auf die Straße gelangen.

Betritt er die Tankstelle, wird die entsprechende Innenraumkarte aktiviert.

Beim Verlassen kehrt er an die passende Position der vorherigen Karte zurück.

Anforderungen:

- Definierte Karteneingänge und Ausgänge.
- Korrekte Spawnpositionen.
- Keine Platzierung innerhalb von Hindernissen.
- Keine unnötigen Ladeunterbrechungen.
- Einheitliches Bewegungssystem.
- Funktionierende Kollisionen.
- Erhalt des globalen Spielzustands beim Kartenwechsel.

NPC-Beziehungen, Geldbeutel, Uhrzeit, Inventar und Aufgaben dürfen beim Kartenwechsel nicht zurückgesetzt werden.

## 3.2 Darstellung

Alle Karten müssen denselben zusammenhängenden Pixel-Art-Stil verwenden.

Verwende:

- Klare Pixelkonturen.
- Passende Gebäudegrößen.
- Einheitliche Tiles und Sprites.
- Zusammenpassende Farbschemata.
- Wiedererkennbare Möbel.
- Einen konsistenten Maßstab zwischen Spieler und Umgebung.

Erhalte die vorhandene logische Darstellungsgröße der Spielfigur sowie ihre vier Laufrichtungen und Animationen, soweit die bestehende Implementierung diese vorgibt.

# 4. Einheitliches Coins-System

**Das gesamte Spiel verwendet ausschließlich eine einzige Währung: Casino-Chips, im Spiel auch Coins genannt.**

Es gibt ausdrücklich:

- Keine Euro.
- Kein Bargeld.
- Keine zweite Währung.
- Kein separates Cash-Konto.
- Keinen Umtausch zwischen Echtgeld, Bargeld und Chips.
- Kein paralleles Geldsystem.

Coins werden für sämtliche finanziellen Transaktionen verwendet.

Dazu gehören:

- Roulette.
- Poker.
- Blackjack.
- Spielautomaten.
- Kleidung.
- Lebensmittel.
- Getränke.
- Verbrauchsgegenstände.
- Möbel.
- Dienstleistungen.
- Friseurbesuche.
- Sonstige Einkäufe.

Coins können durch Casinospiele, Aufgaben, Nebenjobs, Fundstücke und Unterstützung durch NPCs erhalten werden.

Alle Systeme müssen denselben zentralen Spieler-Geldbeutel verwenden.

## 4.1 NPC-Geldbeutel

Jeder NPC besitzt einen eigenen individuellen Coins-Bestand.

Dieser soll zur Persönlichkeit und wirtschaftlichen Situation des NPCs passen.

Beispielwerte:

Reicher Geschäftsmann: 20.000 Coins.

Wohlhabende Dame: 12.000 Coins.

Normaler Besucher: 1.500 Coins.

Armer Spieler: 120 Coins.

Croupier: 800 private Coins.

Diese Werte dienen nur als Ausgangspunkt und sollen an die tatsächliche Spielökonomie angepasst werden.

Wenn ein NPC 100 Coins verschenkt, verliert sein persönlicher Geldbeutel exakt 100 Coins und der Spieler erhält exakt 100 Coins.

NPCs dürfen keine negativen Geldbestände erhalten.

Unterscheide das private Guthaben eines NPCs vom möglichen Budget eines Geschäfts oder des Casinos.

Questbelohnungen und Casino-Auszahlungen sollen aus klar definierten Quellen stammen.

Vermeide unbegrenzte oder versehentlich doppelte Gutschriften.

# 5. Zentrale Ingame-Uhr

Ein vollständiger Ingame-Tag dauert exakt 30 Echtzeitminuten, solange die Spielzeit aktiv läuft.

Das bedeutet:

- 24 Ingame-Stunden entsprechen 30 Echtzeitminuten.
- Eine Ingame-Stunde entspricht 75 Echtzeitsekunden.
- Eine Ingame-Minute entspricht 1,25 Echtzeitsekunden.

Die Uhrzeit wird zentral verwaltet und darf nicht für jede Karte separat existieren.

Die Zeit läuft über Kartenwechsel hinweg weiter.

Während eines pausierten Spiels oder im Hauptmenü soll die Ingame-Zeit nicht unkontrolliert weiterlaufen.

Nutze die vom OOPGames-Framework vorgesehenen Aktualisierungsmöglichkeiten. Implementiere keine unabhängige Game-Loop.

## 5.1 Tag-Nacht-Zyklus

Die Welt soll sich optisch an die Uhrzeit anpassen.

Vorgesehene Zeitabschnitte:

06:00–10:00 Uhr: Morgen.

10:00–18:00 Uhr: Tag.

18:00–22:00 Uhr: Abend.

22:00–06:00 Uhr: Nacht.

Die Farben sollen sich möglichst fließend verändern.

Beispiele:

Morgens warme, leicht goldene Beleuchtung.

Tagsüber normale helle Farben.

Abends orange-violette Farbtöne.

Nachts dunkelblaue Umgebung mit eingeschalteten Straßenlaternen.

Innenräume sollen eigene Lichtregeln besitzen.

Beispielsweise darf das beleuchtete Casino nachts nicht genauso dunkel werden wie eine unbeleuchtete Straße.

Der Farbwechsel soll die vorhandenen Grafiken visuell beeinflussen, ohne deren originale Farben oder Dateien dauerhaft zu verändern.

## 5.2 Zeitabhängige Spielereignisse

Die Uhrzeit soll später Einfluss nehmen können auf:

- Ladenöffnungszeiten.
- NPC-Aufenthaltsorte.
- NPC-Verfügbarkeit.
- Telefonanrufe.
- Bestimmte Aufgaben.
- Öffentliche Aktivitäten.
- Beleuchtung.

# 6. Charaktererstellung beim Start

Wenn der Spieler ein neues Spiel startet, muss zuerst eine Charaktererstellung erscheinen.

Die Charaktererstellung soll im vorhandenen Pixel-Art-Stil gestaltet sein.

## 6.1 Spielername

Der Spieler kann seinem Charakter einen eigenen Namen geben.

Anforderungen:

- Freie Namenseingabe.
- Keine leeren Namen.
- Sinnvolle maximale Namenslänge.
- Prüfung ungültiger Eingaben.
- Anzeige des Namens im Charaktermenü.
- Speicherung des Namens im Spielstand.

Der Name muss dynamisch verwendet werden.

Beispielsweise:

NPC: „Hey Alex, schön dich wiederzusehen!“

Dabei steht Alex stellvertretend für den tatsächlich vom Spieler gewählten Namen.

Verwende keine fest einprogrammierten Spielernamen.

## 6.2 Aussehen

Bei der Charaktererstellung soll der Spieler nach Möglichkeit folgende Merkmale auswählen können:

- Haarfarbe.
- Frisur.
- Hautfarbe.
- Anfangskleidung.
- Weitere passende optische Merkmale.

Die verfügbaren Möglichkeiten sollen sich an den vorhandenen Charakter-Sprites und dem bestehenden Grafiksystem orientieren.

Erstelle keine visuell unpassenden Figuren.

Alle Anpassungen müssen mit den Laufanimationen in vier Richtungen kompatibel sein.

# 7. Spielerhaus

Der Spieler besitzt ein eigenes Haus mit separater Innenraumkarte.

Das Haus dient als persönlicher Rückzugsort.

Vorgesehene Bestandteile:

- Bett.
- Kleiderschrank.
- Spiegel.
- Dekorationen.
- Platz für spätere Möbel.

## 7.1 Charakteranpassung im Haus

Über den Spiegel oder Kleiderschrank kann der Spieler sein Aussehen erneut bearbeiten.

Er kann beispielsweise:

- Kleidung wechseln.
- Gekaufte Mützen aufsetzen.
- Oberteile ändern.
- Hosen wechseln.
- Passende Accessoires ausrüsten.

Frisuren können zusätzlich über einen Friseursalon geändert werden.

Der beim Spielstart gewählte Name bleibt zunächst erhalten, sofern keine ausdrücklich vorgesehene Umbenennungsfunktion existiert.

## 7.2 Hausgestaltung

Als spätere Erweiterung soll der Spieler Möbel und Dekoration mit Coins kaufen können.

Die Möbel sollen anschließend sichtbar im Haus erscheinen.

Das vorhandene Kollisionssystem muss dabei berücksichtigt werden.

# 8. Bekleidungsgeschäft und sichtbare Kleidung

Implementiere ein Bekleidungsgeschäft als eigene Karte.

Das Geschäft besitzt:

- Verkaufstheke.
- Verkäufer-NPC.
- Ausgestellte Kleidung.
- Interaktionsbereiche.
- Einkaufsmöglichkeiten.

Der Spieler kann Kleidung mit Coins kaufen.

Mögliche Kategorien:

- Mützen.
- Oberteile.
- Jacken.
- Hosen.
- Schuhe.
- Accessoires.

## 8.1 Sichtbare Ausrüstung

**Die Kleidung muss tatsächlich auf der Spielfigur sichtbar sein.**

Es reicht nicht, die gekaufte Kleidung nur im Inventar anzuzeigen.

Wenn der Spieler beispielsweise eine rote Jacke kauft und ausrüstet, muss sein Charakter im Spiel eine rote Jacke tragen.

Dies muss für folgende Animationen funktionieren:

- Laufen nach oben.
- Laufen nach unten.
- Laufen nach links.
- Laufen nach rechts.
- Idle-Animationen.

Die Kleidung muss auch nach Kartenwechseln und nach dem Laden eines Speicherstands erhalten bleiben.

## 8.2 Modulares Sprite-System

Prüfe, ob sich das bestehende Grafiksystem um austauschbare Kleidungsebenen erweitern lässt.

Mögliche Ebenen:

- Grundkörper.
- Haare.
- Oberteil.
- Hose.
- Schuhe.
- Kopfbedeckung.
- Accessoires.

Die Darstellung darf nicht von einer bestimmten Kleidungsdatei abhängig sein.

Falls diese Lösung mit den vorhandenen Sprites nicht sinnvoll möglich ist, entwickle eine kompatible Alternative.

Behalte die bestehenden Charakterproportionen und Animationen bei.

# 9. Tankstelle und weitere Geschäfte

Die Tankstelle erhält eine eigene Karte.

Sie besitzt mindestens:

- Einen Verkaufsbereich.
- Einen Verkäufer-NPC.
- Regale.
- Kaufbare Gegenstände.
- Eine Verkaufstheke.

Alle Preise werden ausschließlich in Coins angegeben.

## 9.1 Sortiment

Geplante Gegenstände:

- Wasser.
- Kaffee.
- Energydrinks.
- Sandwiches.
- Snacks.
- Wodka.
- Zigaretten.
- Weitere passende Alltagsgegenstände.

Diese Gegenstände sollen Teil eines fiktiven Spielsystems sein.

## 9.2 Verbrauchsgegenstände und Buffs

Einige Gegenstände können zeitlich begrenzte Effekte besitzen.

Beispiele:

Kaffee:
Kurzzeitiger Ausdauerbonus.

Energydrink:
Kurzfristiger Energieschub mit anschließendem Nachteil.

Sandwich:
Stellt Ausdauer wieder her.

Zigaretten:
Möglicherweise kurzfristige Stimmungsveränderung mit anschließendem Nachteil.

Wodka:
Vorübergehende Veränderung von Stimmung und Dialogverhalten mit Einschränkungen der Kontrolle oder Wahrnehmung.

Alkohol und Zigaretten sollen nicht ausschließlich als vorteilhaft dargestellt werden. Mögliche kurzfristige Effekte sollen durch nachvollziehbare Nachteile ausgeglichen werden.

Alle Buffs benötigen:

- Eine klar definierte Wirkung.
- Eine begrenzte Dauer.
- Einen eindeutigen Startzeitpunkt.
- Ein automatisches Ende.
- Regeln für mehrfaches Verwenden.
- Eine korrekte Speicherung des verbleibenden Effekts.

Buffs dürfen keine unfairen oder mathematisch falschen Gewinnchancen bei Casinospielen erzeugen.

## 9.3 Weitere Geschäfte

Das System soll später erweiterbar sein.

Mögliche weitere Geschäfte:

- Friseursalon.
- Möbelgeschäft.
- Kiosk.
- Weitere thematisch passende Läden.

Jedes Geschäft soll dieselbe grundlegende Kauf- und Inventarlogik wiederverwenden.

# 10. Inventarsystem

Der Spieler erhält ein zentrales Inventar.

Es enthält alle gekauften oder gefundenen Gegenstände.

Vorgesehene Kategorien:

- Kleidung.
- Lebensmittel.
- Getränke.
- Verbrauchsgegenstände.
- Questgegenstände.
- Sonstige Gegenstände.

Inventargegenstände benötigen geeignete Eigenschaften.

Beispiele:

- Eindeutige Kennung.
- Name.
- Beschreibung.
- Kategorie.
- Icon.
- Wert in Coins.
- Anzahl.
- Verwendbarkeit.
- Effekt.
- Gegebenenfalls Effektdauer.

Der Spieler soll Gegenstände verwenden oder Kleidung ausrüsten können.

Das Inventar darf keine getrennte Geldverwaltung besitzen.

Alle Kaufvorgänge verwenden die zentrale Coins-Verwaltung.

# 11. Flipphone als zentrales Spielmenü

Implementiere ein aufklappbares Retro-Handy als wesentliches Element des Spiels.

Das Handy soll optisch zu einem klassischen Mobiltelefon aus den 2000er-Jahren passen.

Es muss im vorhandenen Pixel-Art-Stil gestaltet werden.

Der Spieler kann das Telefon über eine vom OOPGames-Framework unterstützte Eingabe öffnen.

Das Handy soll visuell wie ein Flipphone aufgebaut sein:

- Oberer Bildschirm.
- Unterer Tastenbereich.
- Retro-Gehäuse.
- Pixel-Art-Menüs.
- Klar erkennbare Auswahlmarkierung.

Wenn eine Klappanimation mit dem Framework möglich ist, kann sie ergänzt werden.

Keine externen Fenster verwenden.

## 11.1 Geplante Handy-Apps

Das Handy soll folgende Anwendungen besitzen:

1. Telefonbuch.
2. Anrufe.
3. SMS.
4. Uhr.
5. Speichern.
6. Aufgaben.
7. Karte.
8. Einstellungen.

Die Apps sollen modular sein, damit später weitere Anwendungen ergänzt werden können.

# 12. Telefonbuch

Das Telefonbuch enthält die Telefonnummern bekannter NPCs.

Jeder Kontakt zeigt:

- Name.
- Telefonnummer.
- Individuelles Charakterporträt.
- Gegebenenfalls Beziehungsstatus.
- Anrufoption.

**Besonders wichtig:**

Neben jedem Kontaktnamen soll das zum NPC passende Pixel-Art-Porträt angezeigt werden.

Das Porträt muss eindeutig dem tatsächlichen Aussehen des NPCs entsprechen.

## 12.1 Telefonnummern freischalten

Nicht alle Telefonnummern stehen von Anfang an zur Verfügung.

Der Spieler kann Nummern im Verlauf des Spiels erhalten.

Beispiel:

Ein NPC vertraut dem Spieler zunehmend.

Ab einem passenden Beziehungsstand bietet er seine Telefonnummer an.

Dialog:

„Weißt du was? Hier ist meine Nummer. Ruf mich ruhig mal an.“

Anschließend erscheint der Kontakt im Telefonbuch.

Freundschaften sollen eine wichtige Möglichkeit sein, Telefonnummern freizuschalten.

Einige NPCs können ihre Nummer auch als Belohnung oder nach besonderen Ereignissen herausgeben.

NPCs sollen individuelle, eindeutig zugeordnete fiktive Telefonnummern besitzen.

# 13. NPCs anrufen

Der Spieler kann freigeschaltete Kontakte anrufen.

Ein Telefonat öffnet ein Dialogfenster.

Es enthält:

- Charaktername.
- Charakterporträt.
- Gesprächsantwort.
- Antwortmöglichkeiten.
- Gegebenenfalls Vertrauens- und Respektanzeige.

Anrufe funktionieren ähnlich wie direkte NPC-Gespräche.

Allerdings können die Dialoge anders ausfallen, weil sich der Spieler nicht persönlich vor dem NPC befindet.

## 13.1 Erreichbarkeit

Die Erreichbarkeit soll abhängig sein von:

- Ingame-Uhrzeit.
- Tagesablauf.
- Beziehung zum Spieler.
- Aktueller Beschäftigung des NPCs.
- Gegebenenfalls dessen Stimmung.

Beispiele:

Ein guter Freund geht spätabends möglicherweise noch ans Telefon.

Ein unbekannter NPC beantwortet einen Anruf nachts eventuell nicht.

Ein Gegner kann den Anruf ablehnen.

Ein NPC während eines Casino-Duells kann vorübergehend nicht erreichbar sein.

# 14. SMS-System

Der Spieler kann über das Flipphone Nachrichten erhalten.

Beispiele:

Ein Freund schreibt:
„Hey, komm heute Abend ins Casino!“

Ein Rivale schreibt:
„Zeit für eine Revanche!“

Ein NPC bittet um Hilfe:
„Kannst du kurz vorbeikommen?“

SMS sollen:

- Mit den NPC-Beziehungen verbunden sein.
- Zeitabhängig ausgelöst werden können.
- Aufgaben freischalten können.
- Eine übersichtliche Nachrichtenliste besitzen.
- Im Spielstand erhalten bleiben.

Vermeide unkontrollierte Nachrichtenfluten.

# 15. Startbildschirm und Speicherstände

Beim Start des Spiels erscheint zunächst ein Hauptmenü.

Vorgesehene Optionen:

- Neues Spiel.
- Spiel laden.
- Einstellungen.
- Beenden.

Die Darstellung muss im vorhandenen Pixel-Art-Stil erfolgen.

Das Hauptmenü soll innerhalb der vom Framework erlaubten Darstellung funktionieren.

## 15.1 Neues Spiel

Beim Start eines neuen Spiels wird zuerst die Charaktererstellung geöffnet.

Der Spieler:

1. Gibt seinen Namen ein.
2. Wählt sein Anfangsaussehen.
3. Bestätigt seine Auswahl.
4. Beginnt an einer festgelegten Startposition.

## 15.2 Spiel laden

Der Spieler kann zwischen mehreren Speicherplätzen wählen.

Vorgesehen sind mindestens drei Speicherplätze, sofern technisch und gemäß Projektvorgaben zulässig.

Jeder Speicherplatz zeigt:

- Spielernamen.
- Speicherplatznummer.
- Letzten Aufenthaltsort.
- Ingame-Tag.
- Ingame-Uhrzeit.
- Gesamte Spielzeit.

Leere Speicherplätze sollen klar erkennbar sein.

## 15.3 Speichern über das Flipphone

Das Spiel soll über eine App im Flipphone gespeichert werden können.

Beim Speichern müssen alle wesentlichen Spielzustände erhalten bleiben.

Dazu gehören:

- Spielername.
- Aussehen.
- Kleidung.
- Aktuelle Karte.
- Spielerposition.
- Coins.
- Inventar.
- Aktive Buffs.
- Ingame-Uhrzeit.
- Ingame-Tag.
- NPC-Beziehungen.
- Vertrauen und Respekt.
- NPC-Geldbeutel.
- NPC-Erinnerungen.
- Freigeschaltete Telefonnummern.
- SMS.
- Aufgaben.
- Erledigte Quests.
- Wichtige Ereignisse.
- Gekaufte Gegenstände.
- Hausausstattung, soweit implementiert.
- Spielzeit.

Beim Laden müssen diese Zustände korrekt wiederhergestellt werden.

Es darf nicht vorkommen, dass Geld, Kleidung oder Beziehungen beim Laden zurückgesetzt werden.

Das Speicherformat soll nachvollziehbar, versionierbar und gegen beschädigte Daten angemessen abgesichert sein.

Die Speicherung muss mit den zulässigen Dateipfaden und Projektregeln vereinbar sein.

# 16. Bereits vorhandene NPCs beibehalten

**Alle vorhandenen Charaktere sollen ihr bisheriges Aussehen behalten.**

Dies betrifft:

- Frisur.
- Haarfarbe.
- Kleidung.
- Hautfarbe.
- Körperproportionen.
- Accessoires.
- Animationen.
- Individuelle visuelle Merkmale.

Ersetze vorhandene NPCs nicht grundlos durch neue Charaktere.

Stattdessen sollen die bestehenden Charaktere individuelle Persönlichkeiten und zusätzliche Funktionen erhalten.

Jeder NPC soll im Spiel deutlich wiedererkennbar bleiben.

# 17. Detaillierte NPC-Porträts bei Gesprächen

Beim Ansprechen eines NPCs soll neben dem Text ein größeres, detailliertes Charakterporträt erscheinen.

Das Porträt zeigt vorzugsweise den Kopf und Oberkörper.

Es muss dasselbe Aussehen wie die bereits vorhandene NPC-Spielfigur besitzen.

Beispiel:

Ein NPC mit schwarzem Anzug und blonden Haaren darf im Gespräch nicht plötzlich braune Haare und ein anderes Outfit besitzen.

Die Porträts sollen deutlich detaillierter als die normalen Top-Down-Sprites sein, aber weiterhin dem bestehenden Pixel-Art-Stil entsprechen.

Verwende möglichst klassische 16-Bit-RPG-Porträtgestaltung.

## 17.1 Emotionale Porträts

Vorgesehene Emotionen:

- Neutral.
- Glücklich.
- Wütend.
- Traurig.
- Überrascht.
- Nachdenklich.

Die Emotionen sollen sich passend zur Situation ändern.

Ein beleidigter NPC zeigt beispielsweise ein wütendes Porträt.

Ein glücklicher NPC zeigt ein freundliches Gesicht.

Die Porträts müssen austauschbare Grafikressourcen sein.

Falls passende Porträtdateien fehlen, verwende zunächst vorhandene Charaktergrafiken als visuell konsistenten Ersatz und dokumentiere die fehlenden Assets.

# 18. Automatische NPC-Monologe

NPCs sollen gelegentlich eigenständig kurze Aussagen äußern.

Diese erscheinen als kleine Sprechblasen über den Köpfen der Charaktere.

Anforderungen:

- Kurze Texte, idealerweise 2–8 Wörter.
- Individuelle Aussagen.
- Charakterabhängige Inhalte.
- Zeitliche Abstände.
- Automatisches Verschwinden.
- Keine Überlappung mit wichtigen UI-Elementen.
- Höchstens zwei bis drei spontane Sprechblasen gleichzeitig.
- Priorität für NPCs in Spielernähe.

Beispiele:

Reicher Spieler:
„Heute läuft es hervorragend!“

Armer Spieler:
„Nur noch eine Runde ...“

Rivale:
„Den schlage ich diesmal!“

Freund:
„Da ist er ja wieder!“

Verärgerter NPC:
„Mit dem rede ich nicht.“

Die Monologe sollen auch durch aktuelle Ereignisse ausgelöst werden können.

# 19. Umfangreiches NPC-Dialogsystem

Jeder wichtige NPC soll eine große Auswahl unterschiedlicher Dialoge erhalten.

Vorgesehen sind möglichst mindestens 20–30 unterschiedliche, charaktertypische Dialogzeilen pro wichtigem NPC als Ausgangsbasis.

Die Dialoge sollen nicht nur zufällig gewählt werden, sondern zur Situation passen.

Mögliche Gesprächskategorien:

- Begrüßung.
- Smalltalk.
- Persönliche Geschichten.
- Casino-Gespräche.
- Komplimente.
- Beleidigungen.
- Provokationen.
- Entschuldigungen.
- Geldanfragen.
- Aufgaben.
- Freundschaft.
- Rivalität.
- Feindschaft.
- Reaktionen auf Ereignisse.

## 19.1 Dialogauswahl

Der Spieler erhält normalerweise zwei bis vier Antwortmöglichkeiten pro Gesprächsstufe.

Beispiel:

NPC:
„Du siehst nicht gerade erfolgreich aus.“

Antworten:

1. „Heute läuft es nicht besonders gut.“
2. „Kümmer dich um deinen eigenen Kram.“
3. „Du bist selbst ein Versager.“
4. „Hast du vielleicht ein paar Coins übrig?“

Die Antworten beeinflussen die Beziehung abhängig von der Persönlichkeit des NPCs.

## 19.2 Beleidigungen

Der Spieler soll NPCs bewusst beleidigen oder provozieren können.

Beleidigungen können:

- Vertrauen reduzieren.
- Respekt reduzieren.
- Die Stimmung verschlechtern.
- Rivalitäten verstärken.
- Feindschaften auslösen.
- Spätere Dialoge verändern.

Die Konsequenzen müssen zur Person passen.

Ein arroganter Geschäftsmann reagiert anders als ein ruhiger oder schüchterner NPC.

Wiederholte Beleidigungen können dazu führen, dass NPCs Gespräche verweigern.

## 19.3 Entschuldigungen

Der Spieler soll sich auch entschuldigen können.

Eine Entschuldigung kann Vertrauen zurückgewinnen.

Allerdings dürfen schwere oder wiederholte Beleidigungen nicht sofort vollständig vergessen werden.

Die NPCs sollen entsprechend ihrer Persönlichkeit reagieren.

# 20. NPC-Vertrauen und Respekt

Jeder NPC besitzt zwei voneinander unabhängige Werte.

## 20.1 Vertrauen

Wertebereich: 0–100.

Vertrauen beeinflusst:

- Hilfsbereitschaft.
- Persönliche Gespräche.
- Geldgeschenke.
- Telefonnummern.
- Freundschaften.
- Aufgaben.
- Geheimnisse.
- Unterstützung.

## 20.2 Respekt

Wertebereich: 0–100.

Respekt beeinflusst:

- Ernsthaftigkeit der NPC-Reaktionen.
- Rivalitäten.
- Herausforderungen.
- Anerkennung.
- Verhalten nach Casinoduellen.

Vertrauen und Respekt dürfen nicht gleichgesetzt werden.

Ein NPC kann den Spieler respektieren, ihn aber nicht mögen.

## 20.3 Anzeige

Während eines Gesprächs sollen zwei kompakte Leisten sichtbar sein.

Vertrauen: 0–100.

Respekt: 0–100.

Wertänderungen sollen kurz angezeigt werden.

Beispiel:

„Vertrauen -10“

„Respekt +5“

Die Werte dürfen die Grenzen 0 und 100 nicht überschreiten.

Wiederholbare Dialoge dürfen nicht zum unbegrenzten Erhöhen dieser Werte missbraucht werden.

# 21. Beziehungsstatus

Jeder NPC besitzt einen individuellen Beziehungsstatus gegenüber dem Spieler.

Mögliche Zustände:

- Unbekannt.
- Bekannt.
- Freund.
- Guter Freund.
- Rivale.
- Respektierter Rivale.
- Gegner.
- Erzfeind.

Der Status soll anhand verschiedener Informationen bestimmt werden.

Berücksichtige:

- Vertrauen.
- Respekt.
- Charaktereigenschaften.
- Vergangene Beleidigungen.
- Freundliche Handlungen.
- Gemeinsame Ereignisse.
- Casino-Duelle.
- Erfüllte Aufgaben.

Der Beziehungsstatus darf sich nicht allein durch einen einzelnen Zahlenwert ergeben.

Beispielsweise kann ein guter Freund gleichzeitig ein freundschaftlicher Konkurrent sein.

Vermeide abrupte, unlogische Statuswechsel.

# 22. NPC-Verhalten abhängig von Beziehungen

**Der Beziehungsstatus muss das tatsächliche Verhalten verändern.**

Es reicht nicht, nur einen anderen Statustext anzuzeigen.

## 22.1 Freunde

Freunde sollen:

- Den Spieler freundlich begrüßen.
- Sich an ihn erinnern.
- Zusätzliche Gespräche anbieten.
- Eher Hilfe leisten.
- Gegebenenfalls ihre Telefonnummer geben.
- Besondere Aufgaben anbieten.
- Sich über Erfolge des Spielers freuen.

Beispiel:

„Hey, schön dich wiederzusehen!“

## 22.2 Gute Freunde

Gute Freunde können:

- Persönliche Geschichten erzählen.
- Besondere Aufträge anbieten.
- Bei Geldmangel helfen.
- Den Spieler auch außerhalb des Casinos unterstützen.
- Bei Problemen besorgt reagieren.

## 22.3 Rivalen

Rivalen können:

- Den Spieler herausfordern.
- Provokante Kommentare machen.
- Revanchen verlangen.
- Auf Casino-Gewinne reagieren.
- Dem Spieler bei Erfolgen Respekt entgegenbringen.

Beispiel:

„Na, bereit für die nächste Niederlage?“

## 22.4 Respektierte Rivalen

Respektierte Rivalen sollen:

- Den Spieler ernst nehmen.
- Schwierige Herausforderungen anbieten.
- Seine Leistungen anerkennen.
- Besondere Duelle ermöglichen.

## 22.5 Gegner

Gegner sollen:

- Abweisend reagieren.
- Hilfe verweigern.
- Gespräche beenden können.
- Den Spieler verspotten.
- Bestimmte Aufgaben nicht anbieten.
- Geldanfragen ablehnen können.

## 22.6 Erzfeinde

Erzfeinde können:

- Besonders feindselig reagieren.
- Persönliche Gespräche verweigern.
- Den Spieler öffentlich provozieren.
- Besondere Konfliktdialoge auslösen.

Beziehungen müssen durch nachvollziehbare Handlungen grundsätzlich wieder veränderbar bleiben.

# 23. Persönliche Casino-Duelle

Duelle sind persönliche Wettkämpfe zwischen dem Spieler und NPCs.

Es handelt sich nicht um körperliche Kämpfe.

Verwende dafür bestehende Casinospiele wie Poker oder andere passende Wettbewerbsformen.

Beispiel:

Ein NPC hat mehrfach beim Poker gegen den Spieler verloren.

Er sagt:

„Dieses Mal besiege ich dich!“

Antwortmöglichkeiten:

1. „Na gut, spielen wir.“
2. „Ich mache dich wieder fertig.“
3. „Du bist keine Herausforderung.“
4. „Vielleicht später.“

Ein Duell kann folgende Werte beeinflussen:

- Respekt.
- Vertrauen.
- Rivalität.
- Stimmung.
- Coins.

Die normalen Regeln der Casinospiele müssen unverändert gelten.

NPCs dürfen nicht künstlich gewinnen oder verlieren, nur damit eine bestimmte Geschichte entsteht.

# 24. NPC-Erinnerungen

NPCs sollen sich wichtige Spielerhandlungen merken.

Beispiele:

- Erstes Kennenlernen.
- Frühere Gespräche.
- Wiederholte Beleidigungen.
- Entschuldigungen.
- Geldgeschenke.
- Geldanfragen.
- Casinoduelle.
- Gewonnene oder verlorene Herausforderungen.
- Erledigte Aufgaben.
- Beobachtete Aktionen in der Spielwelt.

Es ist nicht notwendig, jede einzelne Gesprächszeile zu speichern.

Nutze kompakte, nachvollziehbare Erinnerungsdaten.

Diese Erinnerungen sollen spätere Gespräche verändern.

Beispiel:

Vor einem Duell:
„Viel Glück!“

Nach drei Niederlagen:
„Schon wieder du? Diesmal gewinne ich!“

Nach einer Beleidigung:
„Ich habe nicht vergessen, was du gesagt hast.“

Die Erinnerungen müssen mit dem Spielstand gespeichert werden.

# 25. Mülleimer durchsuchen

Der Spieler soll in dafür vorgesehenen Mülleimern nach Gegenständen und Coins suchen können.

Mögliche Funde:

- Kleine Coinbeträge.
- Pfandähnliche Gegenstände mit Coin-Verkaufswert.
- Gewöhnliche Fundstücke.
- Seltene Gegenstände.
- Gar nichts.

**Wichtig: Sämtliche gefundenen Geldbeträge sind Coins. Es gibt keine Euro-Funde.**

Die Fundwahrscheinlichkeiten sollen nachvollziehbar und ausgewogen sein.

Ein Mülleimer darf nicht unbegrenzt sofort neue Coins erzeugen.

Verwende beispielsweise eine erneute Freigabe nach einem Ingame-Tag.

# 26. NPCs bemerken das Durchsuchen von Mülleimern

**Nur NPCs, die sich zum Zeitpunkt der Handlung in der unmittelbaren Umgebung des Spielers befinden, bemerken das Durchsuchen eines Mülleimers.**

Implementiere einen einstellbaren Wahrnehmungsradius.

Ein NPC darf nur reagieren, wenn:

1. Er sich auf derselben Karte befindet.
2. Er sich innerhalb des Wahrnehmungsradius befindet.
3. Er die konkrete Handlung noch nicht bewertet hat.

Für diese Mechanik ist zunächst keine Blickrichtungsprüfung notwendig.

NPCs auf anderen Karten oder außerhalb des Radius bekommen nicht automatisch mit, was passiert.

Mehrere anwesende NPCs dürfen dieselbe Handlung bemerken.

## 26.1 Respektverlust

Das Durchsuchen von Mülleimern kann zu einem leichten Respektverlust führen.

Die Reaktion hängt von der Persönlichkeit ab.

Beispiele:

Normaler Besucher:
Respekt -3.

Arroganter Geschäftsmann:
Respekt -5.

Guter Freund:
Kein Respektverlust, stattdessen besorgte Reaktion.

Armer Spieler:
Möglicherweise Verständnis.

Rivale:
Spöttischer Kommentar und gegebenenfalls Respektverlust.

Beispiel:

NPC:
„Suchst du ernsthaft im Müll nach Coins?“

Die Reaktionen sollen als kurze Sprechblasen über den NPCs erscheinen können.

## 26.2 Erinnerung

Ein NPC kann sich merken, dass er den Spieler beim Durchsuchen eines Mülleimers beobachtet hat.

In späteren Gesprächen kann er darauf Bezug nehmen.

Der Respektverlust darf nicht durch wiederholte Bewertungen desselben Ereignisses mehrfach ausgelöst werden.

Auch bei mehreren Mülleimern sollen sinnvolle Begrenzungen gegen übermäßige Statusverluste vorgesehen werden.

# 27. NPC-Stimmungssystem

Jeder NPC besitzt eine aktuelle Stimmung.

Mögliche Stimmungen:

- Glücklich.
- Neutral.
- Traurig.
- Verärgert.
- Nervös.

Die Stimmung beeinflusst:

- Dialoge.
- Monologe.
- Porträtausdrücke.
- Geldanfragen.
- Hilfsbereitschaft.
- Verhalten nach Casinoduellen.

Beispiele:

Ein großer Casinogewinn verbessert die Stimmung.

Eine Niederlage kann einen NPC verärgern.

Eine freundliche Unterhaltung kann seine Stimmung verbessern.

Eine Beleidigung kann sie verschlechtern.

Die Änderungen sollen nicht unkontrolliert oder zu häufig stattfinden.

# 28. NPCs um Coins bitten

Der Spieler soll geeignete NPCs um Coins bitten können.

Dialogoption:

„Kannst du mir ein paar Coins geben?“

Die Entscheidung hängt ab von:

- Vertrauen.
- Beziehungsstatus.
- Großzügigkeit.
- Persönlichem Coinbestand.
- Stimmung.
- Früheren Geldanfragen.
- Persönlichkeit.

Ein reicher Freund kann großzügiger sein als ein armer Freund.

Ein Gegner kann die Bitte ablehnen.

Ein NPC darf niemals mehr Coins geben, als er besitzt.

Wiederholtes Betteln soll nicht unbegrenzt funktionieren.

Es kann Vertrauen und Respekt verschlechtern.

Alle Coins werden direkt zwischen den vorhandenen Geldbeuteln übertragen.

# 29. Aufgaben und Nebenjobs

Wenn der Spieler keine Coins mehr besitzt, muss er trotzdem neue Coins verdienen können.

Dafür sollen Aufgaben und kleine Nebenjobs vorgesehen werden.

Beispiele:

- Dem Tankstellenverkäufer helfen.
- Gegenstände im Casino einsammeln.
- Einen verlorenen Gegenstand suchen.
- Eine Lieferung erledigen.
- Ein kostenloses Casino-Duell gewinnen.
- Einem NPC bei einem Problem helfen.

Aufgaben können folgende Auswirkungen besitzen:

- Coins verdienen.
- Vertrauen erhöhen.
- Respekt erhöhen.
- Telefonnummern freischalten.
- Freundschaften vertiefen.
- Weitere Aufgaben öffnen.

Es soll mindestens eine faire, risikofreie und wiederholbare Möglichkeit geben, ohne vorhandene Coins neue Coins zu verdienen.

Damit wird verhindert, dass der Spieler dauerhaft feststeckt.

Questbelohnungen dürfen nicht mehrfach durch denselben Abschluss ausgezahlt werden.

# 30. NPCs reagieren auf Casino-Ereignisse

NPCs sollen nicht unabhängig von der Spielwelt handeln.

Sie sollen auf tatsächlich stattfindende Ereignisse reagieren.

Beispiele:

- Der Spieler gewinnt viel.
- Der Spieler verliert viel.
- Ein Freund gewinnt.
- Ein Rivale verliert.
- Eine Herausforderung wird abgeschlossen.
- Eine Freundschaft entsteht.
- Ein NPC wird beleidigt.
- Der Spieler hilft einem NPC.

Die Reaktionen sollen zur Persönlichkeit und Beziehung passen.

Beispiel:

Der Spieler gewinnt 2.000 Coins.

Freund:
„Unglaublich! Glückwunsch!“

Rivale:
„Das war bestimmt nur Glück.“

Gegner:
„Freu dich nicht zu früh.“

Nicht alle NPCs sollen gleichzeitig reagieren.

Bevorzuge Charaktere, die das Ereignis tatsächlich mitbekommen haben.

# 31. NPC-Tagesabläufe

NPCs sollen sich abhängig von der Ingame-Uhrzeit an unterschiedlichen Orten aufhalten können.

Beispiele:

Ein NPC besucht tagsüber die Tankstelle.

Abends spielt er im Casino.

Nachts befindet er sich zu Hause oder ist nicht erreichbar.

Die Aufenthaltsorte sollen mit dem Kartensystem verbunden sein.

Wenn ein NPC gerade auf einer anderen Karte ist, darf er nicht gleichzeitig auf der aktuellen Karte stehen.

Telefonanrufe sollen seine aktuelle Aktivität berücksichtigen.

Bewegungen müssen die vorhandenen Kollisionen beachten.

Nutze einfache, vordefinierte Tagesabläufe, bevor komplexe Wegfindung implementiert wird.

# 32. Globaler Ruf und lokale Wahrnehmung

Zusätzlich zu individuellen NPC-Beziehungen kann ein einfaches globales Rufsystem vorgesehen werden.

Der Ruf beschreibt, wie der Spieler innerhalb der Casino-Gemeinschaft wahrgenommen wird.

Beispiele für öffentlich wahrnehmbare Handlungen:

- Jemandem helfen.
- Wiederholt NPCs beleidigen.
- Besonders erfolgreich spielen.
- Sich respektlos verhalten.

**Wichtig:**

Persönliches Vertrauen und globaler Ruf sind unterschiedliche Werte.

Ein NPC kann den Spieler trotz schlechtem Ruf mögen.

Nicht jede private Handlung darf automatisch den globalen Ruf verändern.

Insbesondere darf das Durchsuchen eines Mülleimers nicht sofort allen NPCs bekannt werden.

Zunächst reagieren ausschließlich NPCs im unmittelbaren Wahrnehmungsradius.

Als spätere Erweiterung können NPCs Informationen durch persönliche Gespräche weitergeben.

Ein globales Rufsystem darf die lokale Ereigniswahrnehmung nicht umgehen.

# 33. Grafische Benutzeroberfläche

Alle grafischen Benutzeroberflächen müssen zum bestehenden Spielstil passen.

Dazu gehören:

- Hauptmenü.
- Charaktererstellung.
- Dialogfenster.
- NPC-Porträts.
- Sprechblasen.
- Vertrauensleisten.
- Respektleisten.
- Flipphone.
- Telefonbuch.
- Anrufansicht.
- SMS-Ansicht.
- Inventar.
- Shop-Menüs.
- Kleidungsmenü.
- Questübersicht.
- Speicherstandauswahl.

Verwende passende Pixel-Art-Rahmen, Farben, Symbole und Schriften.

Die Benutzeroberfläche muss zur tatsächlichen Canvas-Größe passen.

Alle Inhalte müssen lesbar bleiben.

Menüs dürfen wichtige Spielinformationen nicht unkontrolliert überdecken.

Grafiken müssen austauschbar sein, ohne die Spiellogik verändern zu müssen.

# 34. Dynamisches Wetter – spätere Erweiterung

Falls die übrigen Systeme zuverlässig funktionieren, soll die Architektur auf Wettereffekte vorbereitet werden.

Mögliche Wetterarten:

- Sonnig.
- Bewölkt.
- Regen.
- Nebel.

Wetter kann die Darstellung der Außenkarten beeinflussen.

Später können auch NPC-Tagesabläufe auf Wetter reagieren.

Beispielsweise halten sich bei Regen mehr NPCs in Gebäuden auf.

Diese Funktion hat geringere Priorität als die grundlegenden Spielmechaniken.

# 35. Optionale Hausgestaltung

Die Architektur soll zukünftige Möbelkäufe unterstützen.

Beispiele:

- Betten.
- Schränke.
- Fernseher.
- Tische.
- Dekoration.

Gekaufte Möbel sollen im Haus sichtbar sein.

Alle Käufe erfolgen mit Coins.

Möbelplatzierungen dürfen keine Türen oder wichtige Laufwege blockieren.

Diese Erweiterung soll erst umgesetzt werden, wenn Inventar, Kaufen und Speichern zuverlässig funktionieren.

# 36. Technische Architektur

Erweitere die bestehende Architektur sauber objektorientiert.

Trenne die Verantwortlichkeiten insbesondere für:

- Kartenverwaltung.
- Spielzustände.
- Zeitverwaltung.
- NPC-Daten.
- NPC-Tagesabläufe.
- Persönlichkeiten.
- Dialoge.
- Vertrauen und Respekt.
- Beziehungssystem.
- NPC-Erinnerungen.
- Ereignisverwaltung.
- Persönliche Geldbeutel.
- Zentrale Coins-Verwaltung.
- Inventar.
- Shops.
- Gegenstandseffekte.
- Kleidung.
- Charakteranpassung.
- Telefon.
- Kontakte.
- SMS.
- Quests.
- Speichern und Laden.
- Grafische Darstellung.

Nutze vorhandene Klassen weiter, wenn sie die notwendige Verantwortung bereits übernehmen.

Erstelle zusätzliche Klassen nur, wenn dies fachlich sinnvoll ist.

Vermeide riesige Klassen mit vielen unabhängigen Aufgaben.

Der Painter darf nicht die vollständige Spielmechanik enthalten.

Nutze gemeinsame Zustände und Ereignisse, damit Systeme miteinander kommunizieren können, ohne unnötig voneinander abhängig zu sein.

Es darf keine doppelte Geld-, Zeit- oder Dialogverwaltung entstehen.

# 37. Prioritäten und Entwicklungsphasen

Das Projekt ist umfangreich. Die Umsetzung soll deshalb schrittweise erfolgen.

## Phase 1 – Vollständige Bestandsaufnahme

Untersuche:

- AGENTS.md.
- Framework-Interfaces.
- Existierende Klassen.
- Vorhandene NPCs.
- Vorhandene Grafiken.
- Spielsteuerung.
- Casino-Minispiele.
- Bestehende Geldverwaltung.
- Bereits funktionierende Systeme.

Dokumentiere Konflikte mit den Projektregeln und stelle einen konkreten Implementierungsplan auf.

## Phase 2 – Globale Spielstruktur

Entwickle beziehungsweise erweitere:

- Zentrale Spielzustände.
- Kartenverwaltung.
- Kartenwechsel.
- Globale Uhrzeit.
- Coins-Verwaltung.

Bestehende Funktionen müssen weiterhin funktionieren.

## Phase 3 – Spielstart und Charakter

Implementiere:

- Startbildschirm.
- Neues Spiel.
- Charaktererstellung.
- Spielernamen.
- Anfangsaussehen.
- Charakterdaten.

## Phase 4 – NPC-Grundsystem

Implementiere zunächst einen vollständigen Beispiel-NPC mit:

- Persönlichkeit.
- Dialogen.
- Vertrauen.
- Respekt.
- Beziehungen.
- Erinnerungen.
- Monologen.
- Detailliertem Porträt.

Teste den NPC vollständig, bevor das System auf sämtliche vorhandenen NPCs übertragen wird.

## Phase 5 – Erweiterte NPC-Interaktionen

Ergänze:

- Beleidigungen.
- Entschuldigungen.
- Freundschaften.
- Rivalitäten.
- Gegner.
- NPC-Geldbeutel.
- Coin-Anfragen.
- NPC-Reaktionen auf Casino-Ereignisse.
- Mülleimer-Interaktionen.
- Lokale Wahrnehmung.

## Phase 6 – Flipphone

Implementiere:

- Handy-Oberfläche.
- Uhr.
- Telefonbuch.
- Charakterporträts.
- Telefonnummern.
- Anrufe.
- Freischaltbedingungen.
- Erste SMS-Funktionen.

## Phase 7 – Speichersystem

Implementiere:

- Mehrere Speicherplätze.
- Speichern über das Handy.
- Laden über das Hauptmenü.
- Wiederherstellung aller wichtigen Spielzustände.

## Phase 8 – Spielerhaus und Kleidung

Implementiere:

- Spielerhaus.
- Charaktermenü.
- Kleidungswechsel.
- Sichtbare Kleidung.
- Bekleidungsgeschäft.

## Phase 9 – Geschäfte und Inventar

Implementiere:

- Tankstelle.
- Verkäufer.
- Einkaufssystem.
- Inventar.
- Verbrauchsgegenstände.
- Zeitlich begrenzte Buffs.

## Phase 10 – Lebendige Spielwelt

Ergänze:

- NPC-Tagesabläufe.
- Zeitabhängige Aufenthaltsorte.
- Ladenöffnungszeiten.
- Dynamische NPC-Reaktionen.
- Weitere Aufgaben.
- Nebenjobs.

## Phase 11 – Optionale Erweiterungen

Falls Zeit und Framework dies zulassen:

- Wetter.
- Friseursalon.
- Möbelgeschäft.
- Erweiterte Hausgestaltung.
- Weitere Telefon-Apps.
- Zusätzliche NPC-Geschichten.

## Phase 12 – Qualitätssicherung

Überprüfe:

- Kompilierbarkeit.
- Framework-Kompatibilität.
- Kartenwechsel.
- Kollisionen.
- Spieleranimationen.
- Kleidungsdarstellung.
- Uhrzeit.
- Farbwechsel.
- NPC-Verhalten.
- Dialoge.
- Beziehungen.
- Coins-Verwaltung.
- Shop-Transaktionen.
- Inventar.
- Gegenstandseffekte.
- Telefon.
- Speichern.
- Laden.
- Vorhandene Casino-Minispiele.

# 38. Besondere technische Testfälle

Teste ausdrücklich folgende Situationen:

1. Der Spieler wechselt eine Karte und seine Coins bleiben erhalten.
2. Der Spieler kauft Kleidung und sein Outfit verändert sich sichtbar.
3. Der Spieler lädt einen Spielstand und trägt weiterhin die gekaufte Kleidung.
4. Der Spieler beleidigt einen NPC und dessen Vertrauen sinkt.
5. Derselbe NPC erinnert sich beim nächsten Gespräch an die Beleidigung.
6. Ein NPC wird durch mehrere Ereignisse zum Rivalen.
7. Ein Rivale reagiert anders als ein Freund.
8. Ein NPC erhält nach ausreichender Freundschaft die Möglichkeit, seine Telefonnummer zu übergeben.
9. Der freigeschaltete Kontakt erscheint mit passendem Porträt im Telefonbuch.
10. Ein NPC reagiert beim Anruf abhängig von der Uhrzeit.
11. Ein NPC auf einer anderen Karte bemerkt nicht, dass der Spieler einen Mülleimer durchsucht.
12. Ein NPC in unmittelbarer Nähe bemerkt dieselbe Handlung und reagiert passend.
13. Mehrere nahe NPCs können unterschiedlich auf dieselbe Handlung reagieren.
14. Kein NPC verliert mehrfach Respekt durch dieselbe einzelne Mülleimeraktion.
15. Ein bereits durchsuchter Mülleimer liefert nicht sofort unbegrenzt neue Coins.
16. Der Spieler erhält eine Questbelohnung nicht mehrfach durch denselben Abschluss.
17. Ein NPC verschenkt niemals mehr Coins, als er besitzt.
18. Einkäufe, Gewinne, Verluste und Belohnungen verändern ausschließlich die Coins-Bestände.
19. Es existiert keine zweite Währung und kein Umtauschsystem.
20. Ein vollständiger Ingame-Tag dauert 30 aktive Echtzeitminuten.
21. Die Beleuchtung verändert sich passend zur Ingame-Zeit.
22. NPCs können sich abhängig von der Uhrzeit auf unterschiedlichen Karten befinden.
23. Speichern und Laden erhalten die NPC-Beziehungen.
24. Speichern und Laden erhalten das Inventar und die Kontakte.
25. Kein vorhandenes Casino-Minispiel wird durch die neuen Systeme beschädigt.

# 39. Umgang mit bestehenden Grafiken

Die bereits vorhandenen Grafiken und Charakterdesigns haben Vorrang.

Verändere sie nicht ohne Notwendigkeit.

Wenn zusätzliche Grafiken benötigt werden:

1. Untersuche den bisherigen Stil.
2. Bestimme passende Größen und Proportionen.
3. Verwende dieselben visuellen Merkmale wie die bestehenden Charaktere.
4. Halte die Pixel-Art-Darstellung einheitlich.
5. Organisiere die Grafikressourcen austauschbar.
6. Verwende nur zulässige Projektverzeichnisse.
7. Dokumentiere fehlende oder noch zu erstellende Assets.

Für NPC-Gesprächsporträts gilt besonders:

**Die detaillierten Porträts müssen eindeutig zu den tatsächlichen vorhandenen Spielfiguren passen.**

# 40. Dokumentation und Zusammenarbeit

Berücksichtige die bestehende SCRUM-Struktur des Projekts.

Erstelle beziehungsweise ergänze die Dokumentation entsprechend den erlaubten Projektbereichen.

Beachte:

- Product Backlog.
- User Stories.
- Sprintplanung.
- Zuständigkeiten.
- Entwicklungsfortschritt.
- Prompt-Dokumentation.
- Entwicklerkennzeichnungen.
- Git-Historie.

Die Implementierung soll in kleinen, nachvollziehbaren Schritten erfolgen.

Jede größere Änderung muss überprüft werden.

Bestehende Entwicklerkennzeichnungen dürfen nicht entfernt oder fremden Personen zugeschrieben werden.

Falls der verantwortliche Entwickler unbekannt ist, frage vor kennzeichnungspflichtigen Änderungen nach dessen vollständigem Namen.

Keine eigenmächtigen Commits oder Pushes im Namen anderer Teammitglieder.

# 41. Abschließende Vorgaben

Das fertige Casino Adventure soll eine lebendige, zusammenhängende Spielwelt besitzen.

Dabei gelten folgende verbindliche Festlegungen:

**Währung:** Ausschließlich Casino-Chips (Coins) für sämtliche Spiele, Einkäufe und Transaktionen.

**Spielwelt:** Mehrere getrennte Kartenbereiche statt einer einzigen durchgehend geladenen Open World.

**Zeit:** Ein vollständiger Ingame-Tag dauert 30 aktive Echtzeitminuten.

**Charakter:** Eigener Spielername und anpassbares Aussehen.

**Kleidung:** Gekaufte Kleidungsstücke müssen sichtbar auf der Spielfigur erscheinen.

**NPCs:** Individuelle Persönlichkeiten, umfangreiche Dialoge, Erinnerungen, Vertrauen, Respekt und dynamische Beziehungen.

**Beziehungen:** Freundschaften, Rivalitäten und Feindschaften müssen sichtbare spielerische Konsequenzen haben.

**NPC-Porträts:** Detaillierte Pixel-Art-Porträts, die zum bestehenden Charakterdesign passen.

**Telefon:** Aufklappbares Flipphone mit Apps, Kontakten, Anrufen, SMS, Uhr und Speicherfunktion.

**Telefonbuch:** NPC-Namen mit passenden Charakterporträts und freischaltbaren Telefonnummern.

**Mülleimer:** Coins und Gegenstände können gefunden werden. Ausschließlich NPCs in unmittelbarer Nähe auf derselben Karte bemerken das Durchsuchen und können darauf reagieren.

**Geschäfte:** Tankstelle, Bekleidungsgeschäft und weitere Läden, in denen ausschließlich mit Coins bezahlt wird.

**Inventar:** Zentrale Gegenstandsverwaltung mit Kleidung und zeitlich begrenzten Verbrauchseffekten.

**Speichern:** Mehrere Speicherplätze und vollständige Wiederherstellung des Spielfortschritts.

**Grafik:** Einheitlicher Pokémon-Rubin-/Saphir-inspirierter Pixel-Art-Stil unter Beibehaltung aller vorhandenen Charaktermerkmale.

**Framework:** Vollständige Kompatibilität mit den bestehenden OOPGames-Schnittstellen und den verbindlichen Projektregeln.

# 42. Konkreter Arbeitsauftrag an Codex

Beginne nicht blind mit dem Schreiben neuer Klassen.

Gehe folgendermaßen vor:

1. Lies AGENTS.md vollständig.
2. Analysiere die bestehende Projektstruktur.
3. Ermittle, welche der beschriebenen Funktionen bereits vorhanden sind.
4. Prüfe die aktuellen Framework-Schnittstellen.
5. Identifiziere mögliche Konflikte mit den Projektregeln.
6. Erstelle einen konkreten, priorisierten Implementierungsplan.
7. Ordne jeder Funktion passende vorhandene oder neue Klassen zu.
8. Nenne die tatsächlich benötigten Änderungen.
9. Kennzeichne genehmigungspflichtige Erweiterungen.
10. Beginne anschließend mit der schrittweisen Umsetzung aller zulässigen Funktionen.
11. Teste nach jeder größeren Änderung.
12. Erhalte sämtliche bereits funktionierenden Spielmechaniken.
13. Dokumentiere erledigte Funktionen, offene Punkte und benötigte Grafikressourcen.
14. Kontrolliere am Ende die Kompilierbarkeit und den Git-Diff.

Verwende keine vorgetäuschten Implementierungen, keine erfundenen Framework-Schnittstellen und keine nicht funktionierenden Platzhalter, die als fertig ausgegeben werden.

Baue die Funktionen modular, verständlich und erweiterbar auf.

**Das wichtigste Ziel ist nicht die größtmögliche Anzahl an Features, sondern eine technisch funktionierende, grafisch einheitliche und glaubwürdige Spielwelt, in der die verschiedenen Systeme sinnvoll miteinander interagieren.**  füge diese funktionen hinzu und wenn du was in anderen dateien änderst dann kommentiere das ich die geändert habe speichere den prompt wie vorhin in meinem ordner comitte und pushe es gleich auf den branch repariere dann auch noch die map weil der charakter ist unsichtbar und mache die straßen schöner lass sie natülicher verlaufen passe die größen an ändere die grafik auch gleich ab orientiere dich an pokemon omega rubin von der grafik
```

Ergebnisvermerk: Umgesetzt und mit „Änderung durch: Patrick Lupierz“ bzw. „Entwickler: Patrick Lupierz“ markiert:
Reparatur der unsichtbaren Spielfigur in der Stadt (Kamera-Versatz im Painter), neue kompakte Stadtkarte mit geschwungenen
Straßen und überarbeiteter Grafik, Tankstelle und Bekleidungsgeschäft als eigene Karten, zentrale Uhr (30 Minuten pro Tag)
mit Tag-Nacht-Licht und Laternen, Hauptmenü mit Charaktererstellung, sichtbare Kleidung, Inventar, Läden, zeitlich begrenzte
Effekte, Klapphandy (Kontakte, Anrufe, SMS, Uhr, Aufgaben, Speichern), drei Speicherplätze, Mülleimer mit lokaler Wahrnehmung.
Plan, Regelkonflikte und offene Punkte: `Plans/Patrick_Lupierz_Welt_Plan.md`.
`dotnet build`: 0 Fehler, 0 Warnungen. Automatischer Testlauf (Testprojekt außerhalb des Repos): 149 neue Prüfungen bestanden,
dazu die bisherigen NPC-Prüfungen. Bedienung mit echter Tastatur/Maus im Hauptfenster nicht getestet.
Nicht umgesetzt: NPC-Tagesabläufe, Friseur, Möbel/Hausgestaltung, Wetter, Karten- und Einstellungs-App, Frisuren-Auswahl.
