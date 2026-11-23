# Satisfactory – Anleitung

## Spiel starten

Im Framework die Regeln **„G5 Satisfactory Rules“**, den Painter
**„G5 Fabrik-Welt“** und den gemeinsamen menschlichen Spieler
**„G5 Fabrik-Spieler“** auswählen. Der Startbildschirm erscheint im
Framework-Spielfeld, nicht in einem zusätzlichen Fenster. Dort stehen
**„Spiel starten“**, **„Einstellungen“** und **„Anleitung“** zur Auswahl.
Rohstoffvorrat und Produktionstempo lassen sich unter Einstellungen wählen.
Dort lässt sich auch wählen, ob das Tutorial beim Start aktiv ist. Während des
Spiels kann das aktive Tutorial jederzeit mit `T` übersprungen werden.

## Einstellungen

- **Rohstoffvorrat:** Knapp (150), Standard (300) oder Reich (600) Erz pro Vorkommen.
- **Produktionstempo:** Entspannt, Standard oder Schnell für Förderer und Schmelzen.
- **Tutorial:** Aktiviert oder beim Spielstart übersprungen.

## Steuerung

| Aktion | Steuerung |
| --- | --- |
| Förderer auswählen | `F`, danach ein Eisenerz- oder Kupfervorkommen anklicken |
| Schmelze auswählen | `S`, danach eine freie Fläche anklicken |
| Förderband auswählen | `B`, danach freie Nachbarfelder anklicken |
| Konstruktor / Assembler / Generator / Strommast / Lager | `C` / `A` / `G` / `P` / `L`, danach freie Fläche anklicken |
| Gebäude-Register öffnen oder schließen | `Tab`; mit Mausrad oder `↑` / `↓` darin scrollen |
| Gebäude verschieben | `M`, Gebäude anklicken und danach das Zielfeld anklicken |
| Inventar einklappen oder öffnen | Im rechten Inventarbereich auf `‹ INVENTAR` oder `›` klicken |
| Aktives Tutorial überspringen | `T` |
| Bauwerkzeug abwählen oder Verschieben abbrechen | `Esc` |

Der Mentor erklärt die benötigte Taste und zeigt mit einem leuchtenden Pfeil
auf jedes Gebäude und Zielfeld. Baue zuerst einen Förderer auf dem markierten
Rohstofffeld und warte, bis er Erz abbaut. Errichte und verschiebe danach eine
Schmelze. Wähle anschließend mit `B` das Förderband und platziere die drei
markierten Segmente vom Förderer bis zur Schmelze. Sobald Erz
über das Band transportiert wurde, klicke auf das markierte Hub. Danach erklärt
der Mentor Konstruktor und Assembler, zeigt das Abbauen von Kohle für den
Generator und verbindet Generator, Konstruktor und Assembler über einen
Strommast. Zum Abschluss erklärt er das zusätzliche Lager. Mit `T` kannst du
das Tutorial jederzeit überspringen.

Das Bau-Register zeigt die verfügbaren Gebäudetypen und ihre Tasten. Konstruktor
und Assembler werden automatisch aus dem gemeinsamen Lager beliefert. Der
Konstruktor fertigt Bauteile, der Assembler kombiniert mehrere Bauteile. Beide
benötigen Strom. Der Generator verbrennt Kohle aus dem Lager, und Strommasten
verbinden ihn mit Maschinen in Reichweite. Ein zusätzliches Lager erhöht den
gemeinsam angezeigten Vorrat.

Im Vollbildmodus wird die gesamte Karte an den verfügbaren Bereich angepasst.
Die oberen zwei Zeilen zeigen Steuerung, aktives Werkzeug, Hub-Bestand und Status.
Im normalen Fenstermodus bleibt die Karte bei ihrer Standardgröße; der sichtbare
Ausschnitt beginnt oben links unterhalb der Steuerungszeilen und wird am
Spielfeldbereich begrenzt.

Jede neue Spielwelt verteilt die Rohstofffelder und Felsen zufällig neu. Die
Karte verwendet den Wiesenhintergrund mit Grasstruktur und Raster; Rohstoffe,
Gebäude und HUD werden separat aktualisiert.

Förderer bauen auf Rohstoffvorkommen und liefern Erz automatisch zum Hub, sofern
keine passende Förderbandverbindung besteht. Eine durchgehende Förderbandlinie
zwischen genau einem Förderer und einer passenden Schmelze transportiert das
Erz direkt. Das Rohstoffsymbol wandert dabei nacheinander über jedes Segment
der gesamten verbundenen Förderbandlinie. Verzweigte Leitungen oder Leitungen
mit mehr als zwei angeschlossenen Gebäuden werden nicht verbunden. Schmelzen liefern
Barren weiterhin zum Hub. Gebäude können mit `M` versetzt werden; Förderer
müssen auf ein freies Rohstoffvorkommen gesetzt werden. Auch das Hub kann
verschoben werden.

Splitter und Merger sind derzeit nur als Zeichnungen vorhanden und werden nicht
im Bau-Register angeboten.

## Startwelt

Die Karte enthält zufällig verteilte Vorkommen verschiedener Rohstoffarten sowie
eine Eisen- und eine Kupferschmelze. Für Kupfer wird die vorhandene
Kupferschmelze verwendet; neu platzierte Schmelzen starten mit dem Eisenrezept.
