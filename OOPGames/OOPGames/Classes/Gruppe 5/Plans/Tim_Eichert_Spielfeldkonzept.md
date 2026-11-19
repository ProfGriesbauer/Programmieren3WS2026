# Konzept: Spielfeld für das 2D-Fabrikspiel

## Ziel und Rahmen

Das Spielfeld ist eine endliche, zweidimensionale Kachelkarte. Es speichert den Zustand der Welt, zeichnet ihn aber nicht selbst und entscheidet nicht eigenständig über Spielregeln. So bleiben Spielfeld, Painter und Rules austauschbar und die Framework-Grenzen werden eingehalten.

Das Konzept orientiert sich am vorhandenen WPF-Framework OOPGames: Das Spielfeld implementiert `IGameField`; der Painter implementiert `IPaintGame` beziehungsweise bei laufender Aktualisierung `IPaintGame2`. `MainWindow.xaml.cs` bleibt vollständig unverändert. Gruppenspezifische Verträge für teamübergreifende Komponenten müssen vor ihrer Verwendung mit dem Team und den Projektvorgaben abgeglichen werden.

Das Klassendiagramm dazu liegt in [Tim_Eichert_Spielfeld_UML.md](./Tim_Eichert_Spielfeld_UML.md).

Das übergeordnete Architekturkonzept des gesamten Spiels steht in [Tim_Eichert_Gesamtkonzept.md](./Tim_Eichert_Gesamtkonzept.md).

## Vorgeschlagene Klassen

| Klasse | Aufgabe | Zuständigkeit |
|---|---|---|
| `G5_WeltSpielfeld` | Implementierter Weltzustand; hält Kartengröße, Kacheln und die Kartenansicht in privaten Feldern. Implementiert `IGameField` und `IG5_WeltSpielfeld`. | Tim |
| `G5_Kachel` | Interne Kachel-Repräsentation mit Gelände, optionalem Rohstoffvorkommen und Erkundungszustand. Implementiert `IG5_KachelInfo`, aber gibt keine Setter oder WPF-Zeichenlogik frei. | Tim |
| `IG5_KachelInfo` | Schlanker Lesevertrag für Gelände, Rohstoff, Menge sowie erkundeten/aktuell sichtbaren Zustand. Gibt weder Arrays noch Setter frei. | Tim |
| `IG5_WeltPainter` | Gruppenspezifisches Painter-Interface mit typisierten Zeichenmethoden; erweitert `IPaintGame2` für zeitbasierte Darstellung. | Abstimmung mit Team |
| `IG5_FabrikSpielInfo` | Read-only Anzeigevertrag für Gebäude, HUB-Bestand, aktuelles Bauwerkzeug und Statusmeldungen; erweitert das Spielfeld für den ersten Prototyp. | Tim |
| `G5_FabrikSpielfeld` | Aggregiert das Spielfeld mit der vorhandenen Fabriksimulation und stellt deren Daten nur lesend für Player/Painter bereit. | Tim |
| `G5_FabrikPainter` | Zeichnet Karte, Rohstofffelder, HUB, Förderer, Schmelzen und Statusanzeige. Implementiert `IG5_FabrikPainter`. | Tim |
| `G5_Kartenansicht` | Hält Zoom und Verschiebung privat und rechnet zwischen Bildschirm- und Rasterkoordinaten um. Die Leseumrechnung ist Teil von `IG5_WeltSpielfeld`; die separate `IG5_KartenansichtSteuerung` ist für Painter-Eingaben vorgesehen. | Tim/Painter gemeinsam |
| `G5_FabrikRules` | Verarbeitet Werkzeugwahl und Platzierung, initialisiert die Testkarte und taktet die Fabriksimulation. | Tim; Ausbau mit Maxi |
| `G5_FabrikHumanPlayer` / `G5_FabrikAktion` | Übersetzt `F`, `S`, `Esc` und Kartenklicks in typisierte Spielaktionen. | Tim; Ausbau mit Maxi |
| `G5_Building`-Familie | Vorhandene Gebäude aus `Christian_Frey_FabrikGebaeude.cs`; Miner, Schmelze und Lager werden in dieser Version wiederverwendet. | Chris |

Die Dateinamen richten sich nach den Projektvorgaben, zum Beispiel `Tim_Eichert_WeltSpielfeld.cs` und `Tim_Eichert_Kachel.cs`. Klassennamen behalten das Gruppenpräfix `G5_`.

## Umsetzungsstand

Implementiert sind die read-only Kachel-/Spielfeld-Interfaces, das leere Raster, Terrain-/Rohstoffdaten, Rohstoffabbau, Erkundungsstatus, Sichtbarkeit sowie Bildschirm-/Rasterumrechnung mit Zoom- und Verschiebeoperationen. Zoom und Verschieben sind über den getrennten Steuerungsvertrag `IG5_KartenansichtSteuerung` zugänglich, nicht über die lesende Spielfeldschnittstelle.

Zusätzlich ist der erste Karten-/Fabrikprototyp in den Spielklassen implementiert: feste Karte mit zwei Eisenvorkommen und einem HUB, Auswahl von Förderer (`F`) oder Schmelze (`S`), Platzierung per Kartenklick sowie automatische Produktion. Der Förderer gibt Erz an das HUB ab; das HUB versorgt die Schmelze und lagert Eisenbarren ein. Dafür werden die vorhandenen Gebäude- und Simulationstypen von Chris wiederverwendet; ein gezielter Laufzeittest bestätigt die Erz-zu-Barren-Produktion. Painter, Rules und Player sind im vorhandenen Registrierungsblock von `MainWindow.xaml.cs` eingetragen.

Noch nicht umgesetzt sind Laufbänder, Stromversorgung, Baukosten, Handabbau/-crafting, Gebäudeauswahl per Mausklick/HUD, Sicht-Radius und Kartengenerierung. Im Prototyp sind Gebäude kostenlos und die Logistik zwischen Maschinen läuft vereinfacht automatisch über das HUB.

Die Kartenansicht nutzt vorläufig 32 WPF-Layout-Einheiten pro Kachel und Zoomfaktor 0,25–4. Die Ansicht kann noch nicht an die Grenzen des Canvas geklemmt werden, da die sichtbare Canvas-Größe und die gewünschte UX mit Maxi abzustimmen sind.

## Schnittstellen und Sichtbarkeit

Eine separate `G5_KachelAnsicht`-Snapshot-Klasse ist für den ersten Entwurf nicht nötig. Stattdessen implementiert die interne `G5_Kachel` ein schmales, nur lesendes Interface:

- `IG5_WeltSpielfeld : IGameField` zeigt Kartenbreite/-höhe, `TryGetKachelInfo(x, y, out info)` sowie `TryBildschirmZuRaster(...)` und `TryRasterZuBildschirm(...)`.
- `IG5_KachelInfo` zeigt nur Kacheldaten, die andere Komponenten benötigen, zum Beispiel Gelände, Rohstoff, Restmenge und Erkundungsstatus. Der Rückgabewert ist der Interface-Typ, nicht `G5_Kachel`.
- `IG5_KartenansichtSteuerung` ist ein separater Befehlsvertrag mit nur `Verschieben(...)` und `Zoomieren(...)`. Der Painter kann ihn für Canvas-Eingaben verwenden; die Spielereingabe arbeitet weiterhin mit dem lesenden `IG5_WeltSpielfeld`.
- `IG5_FabrikSpielInfo` ergänzt für den Prototyp eine read-only Sicht auf Gebäude, HUB-Bestand und UI-Status. `G5_FabrikSpielfeld` implementiert diese Sicht, ohne konkrete Gebäude oder veränderbare Sammlungen herauszugeben.
- Kachel-Array und `G5_Kartenansicht` bleiben intern. Es gibt keine öffentlichen Setter oder öffentlichen Array-Zugriffe. Belegungs- und Stromnetz-Layer sind erst nach Festlegung der Teamverträge einzubauen.
- Platzieren und Abbauen laufen über die Rules; dafür nötige Änderungsmethoden am Spielfeld sind `internal` und kontrollieren die Änderung.

Das Framework ruft beim menschlichen Spieler `GetMove(IMoveSelection selection, IGameField field)` auf. Der Spieler bekommt also weder den Painter noch das Ansichtsobjekt. Er braucht aber die aktuelle Umrechnung eines Klicks in Rasterkoordinaten. Deshalb stellt `IG5_WeltSpielfeld` nur die nötigen Umrechnungsfunktionen bereit: `TryBildschirmZuRaster(...)` für den Spieler und `TryRasterZuBildschirm(...)` für den Painter. Die Steuerbefehle liegen separat in `IG5_KartenansichtSteuerung`, damit die lesende Datenansicht keine Änderungsmethoden enthält.

Das gruppenspezifische `IG5_WeltPainter : IPaintGame2` bietet typisierte Zeichenmethoden mit `IG5_WeltSpielfeld`. Der konkrete `G5_FabrikPainter` implementiert zusätzlich `IG5_FabrikPainter`, weil er neben Kacheln auch die Fabrikanzeigeinformationen benötigt. Das Fabrikfeld prüft `CanBePaintedBy` gegen dieses kompatible Painter-Interface.

`CanBePaintedBy` ist aus Sicht des Felds benannt: „Kann dieses Feld von diesem Painter gezeichnet werden?“ Der Framework-Aufruf `CurrentField.CanBePaintedBy(_CurrentPainter)` bestätigt diese Richtung. Der Name ist nicht vertauscht und das Framework-Interface darf nicht geändert werden.

Dass `G5_WeltSpielfeld` intern die Kartenansicht mitträgt, ist ein pragmatischer Kompromiss: Das Framework übergibt dem Spieler nur das Feld, keinen gemeinsamen Ansichts-/Spielkontext. Die Ansicht bleibt intern; falls später eine gemeinsame Kontext-/Injection-Lösung hinzukommt, kann sie aus dem Feld herausgezogen werden.

Öffentlich ist nur, was ein anderer Baustein oder das Framework wirklich aufrufen muss. Die Spielfeldklasse implementiert die Leseverträge explizit. Get-only Properties eignen sich für einfache Werte; für Koordinatenabfragen passen Methoden wie `TryGetKachelInfo`. Damit sind Werte lesbar, aber nicht veränderbar. Die separat gehaltene Steuerungsschnittstelle enthält nur notwendige Ansichtsaktionen.

### Datenaufteilung

`G5_WeltSpielfeld` enthält intern beispielhaft:

- private Felder `_breite`, `_hoehe`
- privates Array `_kacheln`
- private Ansichtsreferenz `_kartenansicht`
- Rasterkoordinaten: `x` ist die Spalte und `y` die Zeile; intern wird mit `_kacheln[y, x]` indiziert
- öffentliche get-only Properties `Breite` und `Hoehe` nur über die Leseschnittstelle
- `TryGetKachelInfo(...)` für lesenden, geprüften Zugriff
- `TryBildschirmZuRaster(...)` als lesende Eingabeumrechnung für den Spieler
- `TryRasterZuBildschirm(...)` als lesende Zeichenumrechnung für den Painter
- interne kontrollierte Änderungsmethoden für die Rules
- explizite Implementierung von `CanBePaintedBy(IPaintGame painter)`

Die interne Klasse `G5_Kachel` enthält:

- `G5_Gelaendeart` (`Begehbar` oder `Hindernis`)
- optionalen `G5_Rohstofftyp` (z. B. Eisen, Kupfer, Kohle)
- `RohstoffMenge` (0 bedeutet kein abbaubarer Vorrat)
- `IstErkundet`
- `IstAktuellSichtbar` (von den Rules anhand des Sicht-Radius bestimmt)

Diese Werte werden für den Painter nur lesend über `IG5_KachelInfo` bereitgestellt. So erhält er keine Referenz, mit der er Kachelzustand ändern könnte. Ein belegendes Objekt wird, falls für die Darstellung nötig, ebenfalls nur als reduzierte Ansicht und nicht als konkrete Gebäude-/Laufbandklasse weitergegeben.

Gelände, Rohstoff und platzierte Objekte werden getrennt gespeichert. So kann etwa eine Ressource auf begehbarem Gelände liegen, ohne die Kachelart mit dem Gebäudezustand zu vermischen. Ob mehrere Kacheln von einem Gebäude belegt werden dürfen, wird zunächst nicht vorausgesetzt.

## Abgleich mit `Anforderungen.md`

- Gelände/Hindernis und Rohstoff sind getrennte Informationen. So können normale begehbare Kacheln, Hindernisse (z. B. Felsen) und Rohstoffvorkommen unabhängig dargestellt und geprüft werden.
- Der Rohstofftyp muss die dort genannten Vorkommen abbilden können: Holz, Eisen, Kupfer, Kohle, Kalkstein, Quarz, Schwefel, Uran, Wasser, Gas und Öl. In der ersten lauffähigen Karte können zunächst nur wenige davon vorkommen.
- Für Sichtweite werden `IstErkundet` (bleibt nach dem Besuch erhalten) und `IstAktuellSichtbar` (liegt gerade im Sicht-Radius) getrennt behandelt. Die Rules berechnen den aktuellen Radius; der Painter liest beide Zustände.
- Gebäude, Laufbänder und Stromnetz sollten keine einzige Belegungsmatrix teilen. Gebäude- und Laufbandbelegung müssen separat prüfbar sein; Strom kann als eigene Overlay-/Verbindungsebene über Kacheln dargestellt werden. Das ist noch nicht implementiert, weil Chris' Objektvertrag und Maxis Strom-/Regellogik fehlen.
- Zoom, Verschieben und Klick-Umrechnung gehören zur gemeinsamen Kartenansicht. Der Spieler fragt nur die Umrechnung ab; er erhält weder die Ansicht noch veränderbare Kamerawerte.

## Zusammenspiel der Komponenten

1. `G5_FabrikRules` erstellt eine `G5_FabrikSpielfeld`-Ansicht und veröffentlicht sie als `CurrentField`.
2. OOPGames prüft die Kompatibilität der Regeln und des Painters über die vorgesehenen Interfaces.
3. `G5_FabrikPainter` liest Kacheln und Fabrikanzeige über read-only Interfaces und zeichnet sie. Er verändert keine Ressourcen und platziert keine Objekte.
4. `G5_FabrikHumanPlayer` übersetzt Tastenkürzel und Kartenklicks in `G5_FabrikAktion`; die Rules prüfen die Aktion und ändern erst dann den Weltzustand.
5. `TickGameCall()` aktualisiert die vorhandene Fabriksimulation und verschiebt Items zwischen Förderer, HUB und Schmelze; `TickPaintGameField()` zeichnet den aktuellen Stand.

`IGameField` bietet im Framework nur die allgemeine Kompatibilitätsprüfung. Die benötigten Kartendaten werden über ein schmales gruppenspezifisches Interface bereitgestellt, statt den Painter von der konkreten Klasse abhängig zu machen. Dieses Interface gehört in den erlaubten Gruppenordner unter `Interfaces\gruppe 5` und sollte mit allen Teammitgliedern abgestimmt werden.

## Schrittweiser Umfang

### Erste lauffähige Version (umgesetzt)

1. Feste Karte mit Eisenfeldern und festem HUB/Lager.
2. Förderer und Schmelze auswählen und per Klick auf gültigen Flächen platzieren.
3. Automatische Förderung und Schmelze; Produkte werden vereinfacht über das HUB transportiert.
4. HUD zeigt aktuelles Werkzeug, HUB-Bestand und Statusmeldung; Gebäude und Gelände sind unterscheidbar gezeichnet.
5. Für diesen Prototyp noch keine Kosten, Laufbänder, Stromversorgung oder Siegbedingungen.

### Danach, falls Zeit bleibt

- Rohstoffmengen abbauen und auf null reduzieren.
- Erkundungsradius und dauerhaft erkundete Bereiche.
- Platzierung, Verschieben und Entfernen von Objekten über die Rules.
- Kartengenerierung, mehrere Rohstoffarten und größere Karten.
- Mehrkachel-Gebäude, Speicherstand und weitere Kartenansichten.

## Abgrenzung und Entscheidungen vor der Implementierung

- Die Spielfeldklasse verwaltet Weltzustand, nicht Rezepte, Inventare, Stromversorgung oder Produktionsfortschritt.
- Abbau, Platzierung und Bewegung erfolgen ausschließlich über die Rules, damit ungültige Aktionen an einer Stelle geprüft werden.
- Zoom, Schwenken und Zeichengeometrie gehören zur Ansicht beziehungsweise zum Painter, nicht in die Regeln für Rohstoffe.
- Vor dem Code müssen Team und Dozent klären, welche Spielfeld-/Painter-Interfaces für dieses Spiel verlangt sind und ob eine gruppenübergreifende Kombination der Komponenten vorgesehen ist.
- Gebäude- und Laufbandklassen brauchen eine gemeinsam vereinbarte Möglichkeit, ihre Position und belegten Kacheln an das Spielfeld zu melden. Für die erste Version sollte ein Objekt genau eine Kachel belegen, sofern die Anforderungen nichts anderes verlangen.
