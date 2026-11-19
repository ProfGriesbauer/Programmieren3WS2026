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
- **Produktionstempo:** Entspannt, Standard oder Schnell für die Produktionszyklen.
- **Gebäude:** Schmelzen und Konstruktoren können einzeln aktiviert werden. Deaktivierte Gebäude lassen sich im Spiel nicht auswählen oder platzieren.
- **Förderbänder:** Aus bedeutet automatische Übergabe zwischen Hub und Gebäuden. An bedeutet, dass Items nur über selbst verbundene Förderbänder transportiert werden.
- **Einführung:** Für den verpflichtenden Mentor muss eine Schmelze aktiviert sein. Der Startbildschirm führt andernfalls zu den Einstellungen zurück.

## Steuerung

| Aktion | Steuerung |
| --- | --- |
| Förderer auswählen | `F`, danach ein Eisenerz- oder Kupfervorkommen anklicken |
| Eisen-Schmelze auswählen | Schmelzen in den Einstellungen aktivieren; `S`, danach freie Fläche anklicken |
| Kupfer-Schmelze auswählen | Schmelzen in den Einstellungen aktivieren; `K`, danach freie Fläche anklicken |
| Konstruktor auswählen | In den Einstellungen aktivieren; `C`, danach eine freie Fläche anklicken |
| Bauwerkzeug abwählen | `Esc` |
| Förderband verbinden | In den Einstellungen aktivieren, `B`, roten Ausgang auf dem Gebäude anklicken, danach blauen Eingang auf dem Zielgebäude anklicken |
| Gebäude-Details öffnen | Ein Gebäude anklicken |
| Rezept ändern | Im Gebäudefenster das gewünschte Rezept anklicken |
| Gebäudefenster schließen | `X` anklicken oder außerhalb des Fensters auf die Karte klicken |

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

Förderer bauen auf Rohstoffvorkommen. Schmelzen verarbeiten Eisen- oder
Kupfererz, Konstruktoren fertigen Eisenplatten aus Eisenbarren. Bei
deaktivierten Förderbändern werden Items automatisch zwischen den Gebäuden
und dem Hub übergeben. Im manuellen Modus `B` drücken und den roten Ausgang
sowie den blauen Eingang direkt auf den Gebäuden anklicken; das Spiel verbindet
beide mit einem Weg aus Förderbandsegmenten.
Förderer, Schmelzen, Konstruktoren, Förderbänder und Darstellung verwenden die gemeinsamen Gebäudemodelle und
Schnittstellen von Gruppe 5. Herkunft und Verträge der Bausteine sind in
[`../Plans/Gemeinsame_Architektur_und_Schnittstellen.md`](../Plans/Gemeinsame_Architektur_und_Schnittstellen.md)
dokumentiert.

Das Gebäudefenster zeigt den Gebäudenamen, die Stufe, verfügbare Rezepte und
den aktuellen Inhalt der Eingänge beziehungsweise des Lagers. Es wird per Klick
auf ein Gebäude geöffnet; Produktionsrezepte lassen sich direkt im Fenster ändern.
Gebäude können in dieser Version platziert, aber nicht verschoben oder manuell
abgebaut werden.

## Startwelt

Die Karte enthält Eisen- und Kupfervorkommen sowie das Hub. Zu Beginn sind
keine Schmelzen oder Konstruktoren platziert. Baue Förderer auf Vorkommen;
aktiviere Schmelzen und Konstruktoren in den Einstellungen, wenn du sie
platzieren möchtest. `S` platziert Eisen-Schmelzen, `K` Kupfer-Schmelzen.
