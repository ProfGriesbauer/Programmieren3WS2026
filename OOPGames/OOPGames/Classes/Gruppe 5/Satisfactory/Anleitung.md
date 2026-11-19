# Satisfactory – Anleitung

## Spiel starten

Im Framework die Regeln **„G5 Satisfactory Rules“**, den Painter
**„G5 Fabrik-Welt“** und den gemeinsamen menschlichen Spieler
**„G5 Fabrik-Spieler“** auswählen. Der Startbildschirm erscheint im
Framework-Spielfeld, nicht in einem zusätzlichen Fenster. Dort stehen
**„Spiel starten“**, **„Einstellungen“** und **„Anleitung“** zur Auswahl.
Rohstoffvorrat und Produktionstempo lassen sich unter Einstellungen wählen.
Nach dem Start führt dich ein verpflichtender Mentor Schritt für Schritt durch
Fördererbau, Erzförderung, Schmelzenbau, Gebäudebewegung und den Bau einer
Förderbandverbindung. Die Anleitung kann während dieser Schritte nicht
übersprungen werden.

## Einstellungen

- **Rohstoffvorrat:** Knapp (150), Standard (300) oder Reich (600) Erz pro Vorkommen.
- **Produktionstempo:** Entspannt, Standard oder Schnell für Förderer und Schmelzen.

## Steuerung

| Aktion | Steuerung |
| --- | --- |
| Förderer auswählen | `F`, danach ein Eisenerz- oder Kupfervorkommen anklicken |
| Schmelze auswählen | `S`, danach eine freie Fläche anklicken |
| Förderband auswählen | `B`, danach freie Nachbarfelder anklicken |
| Gebäude verschieben | `M`, Gebäude anklicken und danach das Zielfeld anklicken |
| Bauwerkzeug abwählen oder Verschieben abbrechen | `Esc` |

Der Mentor erklärt die benötigte Taste und zeigt mit einem leuchtenden Pfeil
auf jedes Gebäude und Zielfeld. Baue zuerst einen Förderer auf dem markierten
Eisenerzvorkommen und warte, bis er Erz abbaut. Errichte und verschiebe danach
eine Schmelze. Wähle anschließend mit `B` das Förderband und platziere die drei
markierten Segmente vom Förderer bis zur vorhandenen Eisenschmelze. Sobald Erz
über das Band transportiert wurde, klicke auf das markierte Hub. Danach ist das
freie Bauen und Verschieben möglich.

Im Vollbildmodus wird die gesamte Karte an den verfügbaren Bereich angepasst.
Die oberen zwei Zeilen zeigen Steuerung, aktives Werkzeug, Hub-Bestand und Status.
Im normalen Fenstermodus bleibt die Karte bei ihrer Standardgröße; der sichtbare
Ausschnitt beginnt oben links unterhalb der Steuerungszeilen und wird am
Spielfeldbereich begrenzt.

Die Karte verwendet den Wiesenhintergrund mit Grasstruktur, Raster und Felsen.
Dieser statische Hintergrund bleibt während der Simulation erhalten; Rohstoffe,
Gebäude und HUD werden separat aktualisiert. Bei einem neuen Spielfeld oder
einer geänderten Ansichtsgröße wird die Hintergrundebene neu aufgebaut.

Förderer bauen auf Rohstoffvorkommen und liefern Erz automatisch zum Hub, sofern
keine passende Förderbandverbindung besteht. Eine durchgehende Förderbandlinie
zwischen genau einem Förderer und einer passenden Schmelze transportiert das
Erz direkt. Verzweigte Leitungen oder Leitungen mit mehr als zwei
angeschlossenen Gebäuden werden nicht verbunden. Auf aktiven Förderbändern
werden transportierte Rohstoffe als bewegte Teile angezeigt. Schmelzen liefern
Barren weiterhin zum Hub. Gebäude können mit `M` versetzt werden; Förderer
müssen auf ein freies Rohstoffvorkommen gesetzt werden. Auch das Hub kann
verschoben werden.

## Startwelt

Die Karte enthält Eisen- und Kupfervorkommen sowie je eine passende Schmelze.
Für Kupfer wird die vorhandene Kupferschmelze verwendet; neu platzierte
Schmelzen starten mit dem Eisenrezept.
