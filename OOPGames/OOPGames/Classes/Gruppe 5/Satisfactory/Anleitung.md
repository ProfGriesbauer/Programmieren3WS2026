# Satisfactory – Anleitung

## Spiel starten

Im Framework die Regeln **„G5 Satisfactory Rules“**, den Painter
**„G5 Fabrik-Welt“** und den gemeinsamen menschlichen Spieler
**„G5 Fabrik-Spieler“** auswählen. Der Startbildschirm erscheint im
Framework-Spielfeld, nicht in einem zusätzlichen Fenster. Dort stehen
**„Spiel starten“**, **„Einstellungen“** und **„Anleitung“** zur Auswahl.
Rohstoffvorrat und Produktionstempo lassen sich unter Einstellungen wählen.
Satisfactory registriert keine eigenen Mensch- oder KI-Spieler. Es ist das einzige Fabrik-
Regelwerk und verwendet die gemeinsame Welt, Gebäude, Items, Rezepte und
Simulation von Gruppe 5. Für den Zwei-Spieler-Workaround denselben
Fabrik-Spieler in beiden Spielerfeldern auswählen; das Framework selbst
alterniert weiterhin zwischen den beiden Spieler-Slots.

## Einstellungen

- **Rohstoffvorrat:** Knapp (150), Standard (300) oder Reich (600) Erz pro Vorkommen.
- **Produktionstempo:** Entspannt, Standard oder Schnell für Förderer und Schmelzen.

## Steuerung

| Aktion | Steuerung |
| --- | --- |
| Förderer auswählen | `F`, danach ein Eisenerz- oder Kupfervorkommen anklicken |
| Schmelze auswählen | `S`, danach eine freie Fläche anklicken |
| Bauwerkzeug abwählen | `Esc` |

Förderer bauen auf Rohstoffvorkommen und liefern Erz automatisch zum Hub.
Schmelzen beziehen passende Erze automatisch aus dem Hub und liefern Barren
wieder dorthin. Förderer, Schmelzen und Darstellung verwenden die gemeinsamen Gebäudemodelle und
Schnittstellen von Gruppe 5. Herkunft und Verträge der Bausteine sind in
[`../Plans/Gemeinsame_Architektur_und_Schnittstellen.md`](../Plans/Gemeinsame_Architektur_und_Schnittstellen.md)
dokumentiert.

## Startwelt

Die Karte enthält Eisen- und Kupfervorkommen sowie je eine passende Schmelze.
Für Kupfer wird die vorhandene Kupferschmelze verwendet; neu platzierte
Schmelzen starten mit dem Eisenrezept. Die Simulation, Rohstofffelder,
Förderer, Schmelzen und Darstellung verwenden die gemeinsamen Gruppe-5-
Schnittstellen und Modelle.
