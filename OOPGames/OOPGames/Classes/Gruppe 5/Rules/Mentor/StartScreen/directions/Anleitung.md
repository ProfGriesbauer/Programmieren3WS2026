# Satisfactory – Anleitung

## Spiel starten

Im Framework die Regeln **„G5 Fabrikregeln“**, den Painter
**„G5 Fabrik-Welt“** und den gemeinsamen menschlichen Spieler
**„G5 Fabrik-Spieler“** auswählen. Der Startbildschirm erscheint im
Framework-Spielfeld. Unter **Einstellungen** lassen sich Rohstoffvorrat,
Produktionstempo und Mentor ein- oder ausschalten. Ist der Mentor aktiv,
erklärt er die Steuerung, den Handabbau sowie Gebäude und Rezepte passend zu
den Aktionen im Spiel. Mit `T` kann er jederzeit übersprungen werden.

## Steuerung

| Aktion | Steuerung |
| --- | --- |
| Bewegen und Rohstofffeld anvisieren | `W`, `A`, `S`, `D` |
| Rohstoff am markierten Feld selbst abbauen | `E` |
| Baumenü öffnen oder schließen | `Tab`; mit Mausrad oder `↑` / `↓` scrollen |
| Gebäude auswählen | Taste aus dem Bauregister, danach Zielkachel anklicken |
| Gebäude platzieren | Zielkachel anklicken oder das Gebäude dorthin ziehen |
| Gebäudedetails und Rezeptwahl öffnen | Gebäude anklicken |
| Aktiven Mentor überspringen | `T` |
| Werkzeug abwählen | `Esc` |
| Inventar einklappen oder öffnen | Im Inventarbereich auf `‹ INVENTAR` oder `›` klicken |

Der Handabbau liefert ein Stück des Rohstoffs in den Spielervorrat. Er hat eine
kurze Abklingzeit. Ein Förderer muss direkt auf einem verfügbaren Vorkommen
stehen; alle anderen Gebäude benötigen ein freies Feld. Neue Karten verteilen
Vorkommen und Hindernisse zufällig.

## Gebäude und Fabriklogik

| Gebäude | Funktion |
| --- | --- |
| Förderer (`F`) | Baut das Vorkommen darunter automatisch ab und liefert ins gemeinsame Lager oder auf ein verbundenes Förderband. |
| Schmelze (`S`) | Verarbeitet Eisenerz zu Eisenbarren oder Kupfererz zu Kupferbarren. Das Rezept wird in den Gebäudedetails gewählt. |
| Förderband (`B`) | Transportiert Rohstoffe über die gesamte verbundene Linie zwischen passenden Gebäuden. |
| Konstruktor (`C`) | Fertigt Eisenplatten, Eisenstangen, Kupferdraht oder Schrauben aus Zutaten im gemeinsamen Lager; benötigt Strom. |
| Assembler (`A`) | Kombiniert eine Eisenstange und eine Schraube zu einem Rotor; benötigt Strom. |
| Kohlekraftwerk (`G`) | Verbraucht Kohle aus dem gemeinsamen Lager und erzeugt Strom. |
| Strommast (`P`) | Verbindet Kraftwerke und Maschinen innerhalb seiner Reichweite von acht Feldern. |
| Lager (`L`) | Erhöht die gemeinsame Lagerkapazität. |

Förderbänder müssen eine durchgehende Verbindung von einem Förderer zu einer
passenden Schmelze bilden. Ohne passende Verbindung liefern Förderer ihre
Produktion automatisch ins gemeinsame Lager. Konstruktor und Assembler werden
ebenfalls aus dem gemeinsamen Vorrat versorgt und legen ihre Produkte dort ab.
Splitter und Merger sind derzeit nicht im Bauregister verfügbar.

## Verfügbare Rezepte

- **Handwerk:** Eisenerz → Eisenbarren; Kupfererz → Kupferbarren;
  Eisenbarren → Eisenplatte oder Eisenstange; 2 Kalkstein → Beton.
- **Schmelze:** Eisenerz → Eisenbarren oder Kupfererz → Kupferbarren.
- **Konstruktor:** Eisenbarren → Eisenplatte oder Eisenstange;
  Kupferbarren → Kupferdraht; Eisenstange → 4 Schrauben.
- **Assembler:** Eisenstange + Schraube → Rotor.

Welche Rezepte für ein Gebäude verfügbar sind, steht in seinen Details. Handwerk
startet über die Rezeptauswahl im Inventar; Produktionsgebäude benötigen die
Zutaten im gemeinsamen Lager.

## Startwelt und Anzeige

Die Karte ist 120 × 80 Felder groß. Rohstoffvorkommen und Felsen werden für jede
neue Spielwelt zufällig verteilt. Die obere Anzeige zeigt Status, Werkzeug und
Bestände; das Inventar lässt sich einklappen. Im Vollbildmodus wird die Karte an
den verfügbaren Bereich angepasst.
