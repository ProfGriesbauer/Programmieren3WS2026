# Gebäude und Funktionen

## Zweck und Grundlage

Dieses Dokument konkretisiert den Abschnitt "Gebäude" in [Anforderungen.md](../Anforderungen.md). Es ist ein Vorschlag für die Gruppe und beschreibt Aufgaben, Produktionsabläufe und visuelle Erkennbarkeit der Gebäude. Konkrete Produktionszeiten, Kapazitäten, Reichweiten und Baukosten bleiben offen, bis sie gemeinsam festgelegt werden.

Ziel ist ein eigenständiges 2D-Fabrikspiel aus der Vogelperspektive: Rohstoffe werden gewonnen, transportiert, verarbeitet und zu komplexeren Produkten kombiniert. Jedes Gebäude soll eine klar erkennbare Aufgabe haben.

## Empfohlener Umfang

### Erste spielbare Ausbaustufe

1. Handabbau und Handwerk im Inventar
2. Miner für feste Rohstoffvorkommen
3. Schmelze für Erz zu Barren
4. Konstruktor für einfache Produkte aus einem oder wenigen Eingangsmaterialien
5. Verbrennungsgenerator für die erste Stromversorgung
6. Laufbänder für den tickbasierten Transport
7. Strommasten und ein einfaches Stromnetz
8. Lager als Puffer für Gegenstände

### Spätere Erweiterungen

- Assembler für Rezepte mit mehreren verschiedenen Zutaten
- Kohlekraftwerk und weitere Kraftwerkstypen
- Hoehere Gebäude- und Laufbandstufen
- Splitter und Merger für verzweigte bzw. zusammengeführte Laufbandstrecken
- Pumpen und Foerderanlagen für flüssige oder gasfoermige Ressourcen

Diese Reihenfolge hält die erste Version überschaubar und ermoeglicht trotzdem spätere Erweiterungen.

## Gebäude

| Gebäude | Aufgabe | Eingaben | Ausgaben | Erste Ausbaustufe |
|---|---|---|---|---|
| Miner | Foerdert einen festen Rohstoff aus einem passenden Vorkommen | Rohstofffeld, Strom | Rohstoff | Ja |
| Schmelze | Verarbeitet ein geeignetes Erz zu einem Barren | Erz, Strom | Barren | Ja |
| Konstruktor | Stellt ein einfaches Produkt nach ausgewähltem Rezept her | Rezeptzutaten, Strom | Bauteil | Ja |
| Verbrennungsgenerator | Erzeugt Strom aus einem Brennstoff | Holz oder später Biomasse | Strom | Ja |
| Laufband | Transportiert Gegenstände zwischen benachbarten Feldern | Gegenstände am Eingang | Gegenstände am Ausgang | Ja |
| Strommast | Verbindet Gebäude mit dem Stromnetz | Verbindung zu einem Stromnetz | Stromverbindung | Ja |
| Lager | Speichert Gegenstände und puffert Produktionsketten | Gegenstände | Gespeicherte Gegenstände | Empfohlen |
| Assembler | Stellt komplexe Produkte aus mehreren Zutaten her | Mehrere Rezeptzutaten, Strom | Komplexes Bauteil | Später |
| Kohlekraftwerk | Erzeugt mehr Strom aus Kohle; benoetigt eine Brennstoffversorgung | Kohle, ggf. Wasser | Strom | Später |
| Splitter | Verteilt Gegenstände fair auf zwei Ausgänge | Ein Laufband | Zwei Laufbänder | Später |
| Merger | Vereinigt Gegenstände aus zwei Eingängen | Zwei Laufbänder | Ein Laufband | Später |

### Miner und Rohstoffgewinnung

Der Miner wird auf einem passenden Vorkommen platziert und foerdert im laufenden Spiel Rohstoffe. In der ersten Ausbaustufe sollte ein Miner jeweils nur eine Rohstoffart abbauen. Die Rohstoffart ergibt sich aus dem Feld und kann nicht frei im Gebäudemenü gewählt werden.

Feste Vorkommen wie Eisen, Kupfer, Kohle, Kalkstein, Quarz, Schwefel und Uran eignen sich für Miner. Holz wird zunächst von Hand an Bäumen gesammelt. Wasser, Gas und Oel sollten später eigene Pumpen oder Foerderanlagen erhalten, da sie sich anders als feste Erze verhalten und eigene Transport- oder Verarbeitungssysteme benoetigen koennen.

Ist das Vorkommen erschoepft, die Ausgabe voll oder der Miner nicht mit Strom versorgt, foerdert er nicht weiter und zeigt den Grund an.

### Schmelze

Die Schmelze verarbeitet nur dafür definierte Rohstoffe. Das erste Beispielrezept ist:

- Eisenerz -> Eisenbarren
- Kupfererz -> Kupferbarren

Weitere Rezepte für Kalkstein oder andere Rohstoffe sollten nur aufgenommen werden, wenn ein konkreter Produktionszweck festgelegt ist. Nicht jeder Rohstoff muss geschmolzen werden.

Die Schmelze bietet eine Rezeptauswahl. Sie startet einen Produktionsschritt nur, wenn das Rezept aktiv ist, alle Zutaten vorhanden sind, Strom verfügbar ist und genug Platz im Ausgabepuffer bleibt.

### Konstruktor

Der Konstruktor stellt einfache Bauteile aus einem ausgewählten Rezept her. Beispiele für den Start:

- Eisenbarren -> Eisenplatte
- Eisenbarren -> Eisenstange
- Kupferbarren -> Kupferdraht

Der Konstruktor ist für einfache oder einzelne Verarbeitungsschritte gedacht. Produkte, die mehrere unterschiedliche Zutaten gleichzeitig erfordern, werden später dem Assembler zugeordnet. Die genaue Zuordnung sollte in einer gemeinsamen Rezeptliste festgehalten werden, damit Rezepte und Gebäude nicht widersprüchlich sind.

### Assembler

Der Assembler ist eine spätere Produktionsstufe und benoetigt mehrere Zutaten zugleich. Ein moegliches Beispiel ist:

- Eisenstangen + Schrauben -> Rotor

Er sollte erst eingeführt werden, wenn die benoetigten Einzelteile und ihre Rezepte feststehen. Dadurch bleibt der Unterschied zum Konstruktor klar: Konstruktor für einfache Verarbeitung, Assembler für mehrteilige Produkte.

### Lager

Ein Lager nimmt Gegenstände auf und gibt sie wieder ab. Es entkoppelt Maschinen, die unterschiedlich schnell produzieren oder zeitweise stehen. Ein Lager besitzt eine begrenzte Kapazität; eine hoehere Stufe kann mehr speichern. Die Kapazitätswerte sind noch festzulegen.

Im Informationsfenster sollten Inhalt und freie Kapazität sichtbar sein. Ein Lager darf Gegenstände weder verschwinden lassen noch aufnehmen, wenn kein Platz mehr vorhanden ist.

### Stromversorgung

Der Verbrennungsgenerator ist die frühe Stromquelle und verbraucht einen festgelegten Brennstoff, zum Beispiel Holz. Ein späteres Kohlekraftwerk liefert mehr Leistung, setzt aber Kohle und eine verlässliche Brennstoffzufuhr voraus.

Strommasten verbinden Generatoren und Verbraucher zu einem Netz. für die erste Version kann ein zusammenhängendes Netz genügen. Reichweite, maximale Netzlast und moegliche Ausfälle sind Spielwerte, die die Gruppe noch festlegen muss.

Ein Verbraucher ohne Strom pausiert und behält seine Eingaben. Generatoren zeigen Brennstoffbestand und erzeugte Leistung an; das Netz sollte seinen Verbrauchern den Versorgungszustand mitteilen.

### Laufbänder und Transporterweiterungen

Laufbänder bewegen Gegenstände in einer festgelegten Richtung und aktualisieren ihre Bewegung in Spielticks. Für die erste Version sollten Anfang, Ende, Richtung, belegte Plätze und Transportgrenze eindeutig definiert sein.

Höhere Laufbandstufen können mehr Gegenstände pro Zeit transportieren. Splitter und Merger erweitern das Netz später um Abzweigungen und Zusammenführungen.

Der Splitter nimmt einen Eingang an und verteilt Gegenstände abwechselnd auf zwei Ausgänge. Ist ein Ausgang blockiert oder voll, darf er Gegenstände über den freien Ausgang weiterleiten. Sind beide blockiert, bleibt das Gut im Eingang; kein Gegenstand darf dupliziert oder verworfen werden.

Der Merger nimmt zwei Eingänge an und gibt beide auf ein Ausgangsband. Er bedient die Eingänge fair abwechselnd. Ist das Ausgangsband voll, wartet er, ohne Gegenstände aus den Eingängen zu entfernen. Ein Beispiel für eine Produktionskette ist ein Erzband, das per Splitter zwei Schmelzen versorgt; deren Ausgaben werden danach per Merger auf einem Barrenband gesammelt. Der zusätzliche Durchsatz entsteht nur, wenn beide Schmelzen und die anschließenden Bänder ausreichend Kapazität besitzen.

## Gemeinsamer Produktionsablauf

Jeder Produktionsschritt sollte in einem Spieltick nach denselben Regeln abgewickelt werden:

1. Prüfen, ob ein Rezept ausgewählt ist.
2. Prüfen, ob alle benoetigten Eingaben vorhanden sind.
3. Prüfen, ob Strom verfügbar ist.
4. Prüfen, ob der Ausgabepuffer genug Platz für das Ergebnis hat.
5. Zutaten verbrauchen und das Produkt ausgeben.

Wenn eine Prüfung fehlschlägt, wird der Schritt nicht teilweise ausgeführt. Das Gebäude zeigt einen nachvollziehbaren Status an, zum Beispiel "Kein Strom", "Eingang leer", "Ausgang voll" oder "Kein passendes Vorkommen". So kann der Spieler die Fabrik gezielt reparieren.

## Einheitliches Gebäudeverhalten

Jedes platzierte Gebäude benoetigt mindestens:

- eindeutigen Namen und Gebäudetyp
- Position und belegte Rasterfelder
- Ausbaustufe
- Eingangs- und Ausgabepuffer, soweit für den Typ erforderlich
- Betriebszustand (arbeitet, wartet, blockiert oder ohne Strom)
- klares Verhalten beim Platzieren, Anklicken, Verschieben und Entfernen

Produktionsgebäude zeigen beim Anklicken Rezept, Eingaben, Ausgaben, Fortschritt und Betriebsstatus. Der Spieler soll erkennen koennen, warum ein Gebäude gerade nicht arbeitet.

## Upgrades

Ein Upgrade kann mehrere Eigenschaften verbessern. Geeignete Werte sind:

- Produktionsmenge pro Zeit
- Groesse des Eingangs- oder Ausgabepuffers
- Energieverbrauch
- Foerdermenge pro Zeit
- Lagerkapazität

Welche Werte sich pro Stufe ändern und wie viele Stufen es gibt, ist noch offen. Ein Upgrade sollte nicht automatisch alle Eigenschaften gleichzeitig verbessern, wenn dadurch Kosten und Nachteile bedeutungslos werden.

## Visuelle Gestaltung

Eine interaktive Konzeptansicht mit eigenständigen SVG-Maschinenmodellen liegt in [Gebaeude_Designvorschau.html](./Gebaeude_Designvorschau.html). Sie zeigt die Gebäude aus schräger Draufsicht und macht die Förderwege von Laufband, Splitter und Merger anhand eines Produktionsbeispiels sichtbar.

- Gebäude erhalten aus der Vogelperspektive unterscheidbare Silhouetten und klar erkennbare Grundfarben.
- Der Miner hat eine reduzierte, eindeutige Silhouette: genau zwei deutlich getrennte Standfüße, einen kompakten Motorrahmen und einen großen zentralen Bohrer mit sichtbarer spiralförmiger Bohrschnecke und Spitze.
- Andere Modelle vermeiden strenge quaderförmige Blockaufbauten und folgen denselben Gestaltungsprinzipien wie der Miner: klare Silhouette, wenige gut platzierte Bauteile und sichtbare, funktionsgerechte Mechanik.
- Die Schmelze ist ein runder Ofenkörper mit glühender Öffnung und gebogenen Abzügen; der Konstruktor ein offener Drehtisch mit Werkzeug und Gelenkarmen; der Assembler kombiniert mehrere runde Fertigungstische um einen zentralen Antrieb.
- Das Lager verwendet zylindrische Silos mit gewölbten Deckeln, der Generator einen runden Kessel mit Brennkammer und gebogenen Abzügen. Strommast und Transportteile setzen ebenfalls auf abgerundete Formen und klare Funktionsführung statt dekorativer Blockfundamente.
- Die Form bleibt der Funktion untergeordnet: Förderstrecken dürfen für Richtungswechsel abknicken, während Maschinenkörper organische Rundungen und erkennbare Arbeitsmechanik zeigen.
- Der Assembler wirkt groesser und komplexer als der Konstruktor.
- Lager zeigen ihre Groesse oder Stufe durch zusätzliche Container oder Regale.
- Laufbandstufen koennen unterschiedliche, gut unterscheidbare Farben erhalten.
- Splitter und Merger zeigen ihre Ein- und Ausgänge durch klar sichtbare, abgewinkelte Bandführungen; Materialrichtung bleibt auch in der kompakten Draufsicht lesbar.
- Animationen wie Bohrbewegung, Ofenglow oder bewegte Gegenstände sollen den Betriebszustand unterstützen, nicht nur dekorativ sein.
- Einblendungen oder Symbole machen "arbeitet", "wartet" und "blockiert" auch ohne geoeffnetes Menü erkennbar.
- Gegenstands- und Stromanschlüsse sind im Prototyp farblich getrennt markiert: Eingänge grün, Ausgänge gelb, Strom blau. Die Symbole zeigen die Funktion, legen aber noch keine endgültigen Weltkoordinaten fest.

### Vorläufige Draufsicht, Raster und Anschlüsse

Die Designvorschau enthält ein schematisches Rasterbeispiel, in dem Gebäude-Footprints, Seitenanschlüsse und einzelne Laufbandfelder als 1x1-Kacheln gezeigt werden. Es ist eine Layout-Idee und noch keine verbindliche Platzierungsregel. Die Vorschau verwendet 32-Pixel-Kacheln als vorläufige Referenz, angelehnt an die vorhandene Kartenansicht; Rastermaß, Footprints und Drehung müssen mit der Spielumgebung abgestimmt werden.

- Gebäude behalten einen rechteckigen, kollisionsfreien Footprint; Details der Illustration dürfen nicht als zusätzliche belegte Felder interpretiert werden.
- Anschlüsse liegen an einer der vier Footprint-Seiten und sollen direkt an eine angrenzende Laufbandkachel anschließen. Richtung und erlaubte Drehung sind pro Gebäude in der Weltlogik festzulegen.
- Miner: Rohstoffvorkommen unter dem Footprint, ein Gegenstandsausgang und ein separater Stromanschluss.
- Schmelze und Konstruktor: je mindestens ein Gegenstandseingang, ein Ausgang und Stromanschluss. Mehrere Zutaten können beim Konstruktor je nach Rezept zusätzliche Eingänge erfordern.
- Assembler: mehrere getrennte Gegenstandseingänge, ein Ausgang und Stromanschluss.
- Lager: Gegenstandsein- und -ausgang; die konkrete Transferlogik muss Überschreiben bei vollem Lager verhindern.
- Verbrennungsgenerator: Brennstoffeingang und Stromabgabe. Strommast: Anschluss ans Stromnetz; kein Gegenstandstransport.
- Laufband: ein Eingangs- und ein Ausgangsanschluss; Splitter: ein Eingang und zwei Ausgänge; Merger: zwei Eingänge und ein Ausgang.
- Stromleitungen/Netzverbindungen sind visuell von Gegenstandsbändern zu trennen. Symbole in der Vorschau sind Orientierungshilfen und ersetzen keine festgelegte Port-API.

## Noch gemeinsam festzulegen

- Produktionszeiten, Tickdauer und Gegenstände pro Tick
- Puffer- und Lagerkapazitäten
- Stromverbrauch und Generatorleistung
- Bau- und Upgrade-Kosten
- Groesse und Platzierungsregeln jedes Gebäudes
- Ob Laufbänder Gegenstände intern pro Feld puffern
- Wie Rohstoffe auf der Karte endlich oder unendlich sind
- Wie Gebäude Eingaben und Ausgaben mit Laufbändern austauschen
- Welche Rezepte in der ersten spielbaren Version enthalten sind

## Code-Umsetzung

Die zentrale Gebäudesimulation ist in [Christian_Frey_FabrikGebaeude.cs](../Christian_Frey_FabrikGebaeude.cs) implementiert. Sie enthält Miner, Schmelze, Konstruktor, Assembler, Lager, Laufband, Verbrennungsgenerator, Strommasten und ein tickbasiertes Fabrikmodell. Rezepte, Puffer, Statusmeldungen, Stromverteilung, Itemtransfer und konfigurierbare Upgrades sind ebenfalls enthalten. Die Umsetzung ist eine eigenständige Gebäude- und Simulationsschicht innerhalb von Gruppe 5; sie bindet weder die parallel entwickelte Spielumgebung noch die Bedienoberfläche ein.

Produktionszeiten, Kapazitäten, Foerdermengen, Reichweiten und Stromwerte werden als Konstruktor- oder Upgradeparameter uebergeben, statt im Code als festgelegte Spielbalance vorgegeben zu werden. Die Beispielrezepte fuer Erzschmelze, Konstruktor und Assembler sind ueber `G5_StarterRecipes` verfuegbar.

Noch nicht Teil dieser Gebäudeschicht sind Handabbau und Spielerinventar, die Karte und Platzierungsoberfläche, eine grafische WPF-Darstellung, Pumpen fuer Fluessigkeiten/Gase sowie Splitter und Merger. Diese Bereiche benoetigen die dazugehoerige Spielwelt- und UI-Implementierung.

## Absprachen fuer die spätere Integration

Die folgenden Punkte werden benoetigt, sobald Gebäude mit den parallel entwickelten Teilen des Spiels verbunden werden. Es ist nicht erforderlich, diese Abhängigkeiten vorab in die Spielumgebung oder Bedienung einzubauen; die Schnittstellen sollen mit den zuständigen Entwicklern abgestimmt werden.

### Spielumgebung

- **Raster und Koordinaten:** Welche Klasse repräsentiert ein Kartenfeld, und welche Koordinaten- und Indexkonvention gilt? Die Gebäudeschicht verwendet derzeit `G5_GridPosition` mit ganzzahligen X-/Y-Koordinaten und Rechteck-Footprints.
- **Platzierung:** Wie fragt ein Gebäude ab, ob sein gesamter Footprint bebaubar ist, auf einem passenden Vorkommen steht oder mit Hindernissen bzw. anderen Gebäuden kollidiert?
- **Ressourcenvorkommen:** Wie meldet die Karte Rohstoffart und verbleibende Menge? Miner nehmen derzeit ein `G5_ResourceDeposit` entgegen; eine gemeinsame Vorkommensschnittstelle wuerde die direkte Kopplung vermeiden.
- **Spiel-Ticks:** Welcher Takt ruft die Simulation auf, und in welcher Reihenfolge werden Foerderung, Transport, Produktion und Stromversorgung aktualisiert?
- **Speichern und Laden:** Welche Datenform und welcher Lebenszyklus werden fuer platzierte Gebäude, Puffer, Rezeptwahl, Upgrades und Vorkommensstände verwendet?

### Bedienung und Darstellung

- **Gebäudeaktionen:** Wie meldet die Bedienung Platzieren, Auswählen, Verschieben, Entfernen und Upgrade eines Gebäudes an die Spielsimulation?
- **Rezeptauswahl:** Wie stellt die Bedienung die Rezepte bereit und wie werden Rezeptwechsel sowie Eingaben während eines laufenden Produktionszyklus behandelt?
- **Statusanzeige:** Welche Informationen erwartet die UI fuer Rezept, Fortschritt, Ein-/Ausgangspuffer, Stromversorgung und Blockierungsgrund? Die Gebäude stellen derzeit `Status`, Rezept und Pufferinhalte als Modellzustand bereit.
- **Visuelle Stände:** Wie erhält die Darstellung Gebäudetyp, Position, Groesse und Ausbaustufe, damit Upgrades auch optisch sichtbar werden?

### Gemeinsame Produktions- und Transportschnittstellen

- **Gegenstands-IDs:** Werden die `G5_Item`-Werte von der Gruppe als gemeinsame Item-IDs uebernommen oder durch ein Inventar-/Itemmodell der Spielumgebung ersetzt?
- **Itemtransfer:** Die aktuelle Gebäudeschicht nutzt `G5_IItemEndpoint` fuer typisierte Eingaben, Ausgaben und Bestand. Mit den Laufbändern ist zu klären, ob Transfer weiterhin in Mengen pro Tick oder ueber belegte Rasterfelder und Bandrichtung modelliert wird.
- **Stromnetz:** Die aktuelle Implementierung verbindet Verbraucher ueber die Reichweite von `G5_PowerPole` und verteilt Generatorleistung pro Tick. Falls die Spielumgebung bereits Stromnetze oder Leitungsverbindungen modelliert, soll diese Logik dort integriert statt parallel doppelt gefuehrt werden.
- **Gemeinsame Balance:** Tickdauer, Produktionszeiten, Kapazitäten, Baukosten, Stromwerte, Rohstoff-Endlichkeit und Rezeptliste werden mit allen drei Entwicklern abgestimmt, sobald Umgebung und Bedienung konkretere Vorgaben liefern.

### Splitter und Merger

- Ein Splitter erhält genau einen Bandzulauf und hat zwei Ausgänge. Er wechselt nach erfolgreicher Übergabe den bevorzugten Ausgang, damit beide Zweige fair bedient werden.
- Ist ein Zweig blockiert, darf der Splitter über den anderen freien Zweig liefern. Sind beide blockiert, bleibt das Gut am Zulauf; es wird weder dupliziert noch verworfen.
- Ein Merger hat zwei Bandzuläufe und einen Ausgang. Er wechselt zwischen den Zuläufen, sofern dort Gut anliegt und das Ausgangsband freie Kapazität hat.
- Bei vollem Ausgang wartet das Gut im jeweiligen Zulauf. Die Übergabe muss atomar sein, damit beim Zusammenführen keine Gegenstände verloren gehen oder überschrieben werden.
- Der Beispielaufbau in der Designvorschau führt einen Erzstrom über einen Splitter zu zwei Schmelzen und sammelt deren Barren mit einem Merger wieder auf einem Band.


### Noch hinzufügen

Verbrennungsgenerator für verschiedene Rohstoffe (Holz, Kohle, Gas, Uran) aus