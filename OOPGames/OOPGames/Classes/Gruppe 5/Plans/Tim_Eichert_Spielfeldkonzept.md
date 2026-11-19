# Konzept: Spielfeld für das 2D-Fabrikspiel

## Ziel und Rahmen

Das Spielfeld ist eine endliche, zweidimensionale Kachelkarte. Es speichert den Zustand der Welt, zeichnet ihn aber nicht selbst und entscheidet nicht eigenständig über Spielregeln. So bleiben Spielfeld, Painter und Rules austauschbar und die Framework-Grenzen werden eingehalten.

Das Konzept orientiert sich am vorhandenen WPF-Framework OOPGames: Das Spielfeld implementiert `IGameField`; der Painter implementiert `IPaintGame` beziehungsweise bei laufender Aktualisierung `IPaintGame2`. Die Framework-Dateien und `MainWindow`-Dateien außerhalb des Registrierungsblocks bleiben unverändert. Gruppenspezifische Verträge für teamübergreifende Komponenten müssen vor ihrer Verwendung mit dem Team und den Projektvorgaben abgeglichen werden.

Das Klassendiagramm dazu liegt in [Tim_Eichert_Spielfeld_UML.md](./Tim_Eichert_Spielfeld_UML.md).

## Vorgeschlagene Klassen

| Klasse | Aufgabe | Zuständigkeit |
|---|---|---|
| `G5_WeltSpielfeld` | Implementierter Weltzustand; hält Kartengröße, Kacheln und die Kartenansicht in privaten Feldern. Implementiert `IGameField` und `IG5_WeltSpielfeld`. | Tim |
| `G5_Kachel` | Interne Kachel-Repräsentation mit Gelände, optionalem Rohstoffvorkommen und Erkundungszustand. Implementiert `IG5_KachelInfo`, aber gibt keine Setter oder WPF-Zeichenlogik frei. | Tim |
| `IG5_KachelInfo` | Schlanker Lesevertrag für Gelände, Rohstoff, Menge sowie erkundeten/aktuell sichtbaren Zustand. Gibt weder Arrays noch Setter frei. | Tim |
| `IG5_WeltPainter` | Gruppenspezifisches Painter-Interface mit typisierten Zeichenmethoden; erweitert `IPaintGame2` für zeitbasierte Darstellung. | Abstimmung mit Team |
| `G5_WeltPainter` | Konkrete Darstellung ist noch nicht implementiert. Die typisierte Schnittstelle `IG5_WeltPainter` ist vorbereitet; die Framework-Adapter und Zeichenlogik fehlen. | Maxi |
| `G5_Kartenansicht` | Hält Zoom und Verschiebung privat und rechnet zwischen Bildschirm- und Rasterkoordinaten um. Die Leseumrechnung ist Teil von `IG5_WeltSpielfeld`; die separate `IG5_KartenansichtSteuerung` ist für Painter-Eingaben vorgesehen. | Tim/Painter gemeinsam |
| `G5_WeltRules` | Noch nicht implementiert. Soll Feldaktionen, Sichtweite und zeitbasierte Simulation kontrollieren und das Spielfeld erzeugen. | Maxi |
| `G5_PlatzierbaresObjekt` | Noch nicht implementiert. Der gemeinsame Vertrag mit Chris für Gebäude und Laufbänder muss vor einer Belegung im Feld abgestimmt werden. | Chris/Tim |

Die Dateinamen richten sich nach den Projektvorgaben, zum Beispiel `Tim_Eichert_WeltSpielfeld.cs` und `Tim_Eichert_Kachel.cs`. Klassennamen behalten das Gruppenpräfix `G5_`.

## Umsetzungsstand

Implementiert sind die read-only Kachel-/Spielfeld-Interfaces, das leere Raster, Terrain-/Rohstoffdaten, Rohstoffabbau, Erkundungsstatus, Sichtbarkeit sowie Bildschirm-/Rasterumrechnung mit Zoom- und Verschiebeoperationen. Zoom und Verschieben sind über den getrennten Steuerungsvertrag `IG5_KartenansichtSteuerung` zugänglich, nicht über die lesende Spielfeldschnittstelle.

Noch nicht implementiert sind der Kartenaufbau/Spawnpunkt, die Berechnung des Sicht-Radius, die Gebäude- und Laufbandbelegung, das Stromnetz-Overlay, der konkrete Painter, die Spieler-/Rules-Klassen und ihre Registrierung im erlaubten MainWindow-Block.

Die Kartenansicht nutzt vorläufig 32 WPF-Layout-Einheiten pro Kachel und Zoomfaktor 0,25–4. Die Ansicht kann noch nicht an die Grenzen des Canvas geklemmt werden, da die sichtbare Canvas-Größe und die gewünschte UX mit Maxi abzustimmen sind.

## Schnittstellen und Sichtbarkeit

Eine separate `G5_KachelAnsicht`-Snapshot-Klasse ist für den ersten Entwurf nicht nötig. Stattdessen implementiert die interne `G5_Kachel` ein schmales, nur lesendes Interface:

- `IG5_WeltSpielfeld : IGameField` zeigt Kartenbreite/-höhe, `TryGetKachelInfo(x, y, out info)` sowie `TryBildschirmZuRaster(...)` und `TryRasterZuBildschirm(...)`.
- `IG5_KachelInfo` zeigt nur Kacheldaten, die andere Komponenten benötigen, zum Beispiel Gelände, Rohstoff, Restmenge und Erkundungsstatus. Der Rückgabewert ist der Interface-Typ, nicht `G5_Kachel`.
- `IG5_KartenansichtSteuerung` ist ein separater Befehlsvertrag mit nur `Verschieben(...)` und `Zoomieren(...)`. Der Painter kann ihn für Canvas-Eingaben verwenden; die Spielereingabe arbeitet weiterhin mit dem lesenden `IG5_WeltSpielfeld`.
- Kachel-Array und `G5_Kartenansicht` bleiben intern. Es gibt keine öffentlichen Setter oder öffentlichen Array-Zugriffe. Belegungs- und Stromnetz-Layer sind erst nach Festlegung der Teamverträge einzubauen.
- Platzieren und Abbauen laufen über die Rules; dafür nötige Änderungsmethoden am Spielfeld sind `internal` und kontrollieren die Änderung.

Das Framework ruft beim menschlichen Spieler `GetMove(IMoveSelection selection, IGameField field)` auf. Der Spieler bekommt also weder den Painter noch das Ansichtsobjekt. Er braucht aber die aktuelle Umrechnung eines Klicks in Rasterkoordinaten. Deshalb stellt `IG5_WeltSpielfeld` nur die nötigen Umrechnungsfunktionen bereit: `TryBildschirmZuRaster(...)` für den Spieler und `TryRasterZuBildschirm(...)` für den Painter. Die Steuerbefehle liegen separat in `IG5_KartenansichtSteuerung`, damit die lesende Datenansicht keine Änderungsmethoden enthält.

Das gruppenspezifische `IG5_WeltPainter : IPaintGame2` bietet typisierte Zeichenmethoden mit `IG5_WeltSpielfeld`; das Interface ist angelegt, die konkrete `G5_WeltPainter`-Implementierung fehlt noch. `CanBePaintedBy` prüft `painter is IG5_WeltPainter`, nicht den konkreten Painter-Typ.

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

1. `G5_WeltRules` erstellt oder erhält ein `G5_WeltSpielfeld` und veröffentlicht es als `CurrentField`.
2. OOPGames prüft die Kompatibilität der Regeln und des Painters über die vorgesehenen Interfaces.
3. `G5_WeltPainter` liest den Zustand aus dem Spielfeld und zeichnet ihn. Der Painter verändert keine Ressourcen und platziert keine Objekte.
4. Die Eingabe wird in eine Spielaktion beziehungsweise einen Zug übersetzt. Die Rules prüfen, ob diese Aktion zulässig ist, und ändern erst dann den Weltzustand.
5. Für zeitbasierte Abläufe aktualisiert `TickGameCall()` die Regeln; `TickPaintGameField()` zeichnet den aktuellen Stand. Der Spieler wandelt Klicks über `TryBildschirmZuRaster(...)` in Rasterkoordinaten um, der Painter verwendet `TryRasterZuBildschirm(...)` zum Zeichnen und kann Zoom/Verschiebung über den separaten Steuerungsvertrag anpassen.

`IGameField` bietet im Framework nur die allgemeine Kompatibilitätsprüfung. Die benötigten Kartendaten werden über ein schmales gruppenspezifisches Interface bereitgestellt, statt den Painter von der konkreten Klasse abhängig zu machen. Dieses Interface gehört in den erlaubten Gruppenordner unter `Interfaces\gruppe 5` und sollte mit allen Teammitgliedern abgestimmt werden.

## Schrittweiser Umfang

### Erste lauffähige Version

1. Feste Kartengröße und eine kleine, fest definierte Testkarte.
2. Kacheln mit begehbarem Gelände, Hindernissen und höchstens einem Rohstofftyp pro Vorkommen.
3. Painter stellt jede Kachelart klar unterscheidbar dar.
4. Kameraverschiebung und Zoom, zunächst ohne Nebeneffekte auf die Weltkoordinaten.
5. Tests für Kartenränder, Kachelzugriff, Hindernisse und die Rückrechnung von Bildschirm- auf Rasterkoordinaten.

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
