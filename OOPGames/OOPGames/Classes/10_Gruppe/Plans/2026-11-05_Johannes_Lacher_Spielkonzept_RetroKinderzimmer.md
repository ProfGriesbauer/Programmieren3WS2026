# Spielkonzept & Spezifikation: Retro-Kinderzimmer (Gruppe 10)

| | |
|---|---|
| **Status** | Entwurf – noch nicht mit dem Dozenten abgestimmt |
| **Version** | 0.5 |
| **Team** | Lukas Hindelang, Johannes Gabriel, Johannes Lacher |
| **Branch** | `gruppe10` |
| **Code** | `Classes/10_Gruppe/Kinderzimmer90er/`, `Interfaces/10_Gruppe/Kinderzimmer90er/` |

Dieses Dokument ist die verbindliche Beschreibung unseres zweiten Spiels. Jede Änderung an Umfang oder
Verhalten wird hier eingetragen (siehe [Änderungshistorie](#8-änderungshistorie)), bevor sie umgesetzt wird.

---

## 1. Idee

Der Spieler steht in einem **3D-Kinderzimmer aus den 90ern**. Im Zimmer stehen mehrere **Retro-Spielgeräte**
(Commodore-Heimcomputer, Atari, Gameboy). Wählt er ein Gerät aus, bekommt er die **Spiele dieses Geräts** zur Auswahl. Ein gewähltes
Spiel startet **im Vollbild** (siehe F5). Nach dem Spiel kehrt er zur Spielauswahl bzw. ins Zimmer zurück.

Welche Spiele es konkret gibt, wird später festgelegt (siehe [Offene Fragen](#7-offene-fragen)).

## 2. Ablauf

```
 Zimmer (3D)  ──Gerät wählen──▶  Spielauswahl  ──Spiel starten──▶  Spiel (Vollbild)
     ▲                             │      ▲                            │
     └──────────zurück─────────────┘      └───────────zurück───────────┘
```

## 3. Funktionale Anforderungen

| ID | Anforderung | Status |
|---|---|---|
| F1 | Das Zimmer wird als 3D-Szene im Stil eines 90er-Kinderzimmers dargestellt. | festgelegt |
| F2 | Im Zimmer stehen die Geräte **Commodore** (Heimcomputer), **Atari** und **Gameboy**; jedes ist visuell erkennbar. | festgelegt |
| F3 | Der Spieler kann im Zimmer ein Gerät auswählen; das aktuell gewählte Gerät ist hervorgehoben. | festgelegt |
| F4 | Jedes Gerät bietet **mehrere Spiele** zur Auswahl an. | festgelegt |
| F5 | Ein gestartetes Spiel füllt die **gesamte Zeichenfläche** (Canvas) des Frameworks aus, unter Beibehaltung seines Seitenverhältnisses. Ausnahme: der Platz für das Steuerkreuz (F8). | festgelegt |
| F6 | Aus einem Spiel kommt man per Taste zurück zur Spielauswahl, von dort zurück ins Zimmer. | festgelegt |
| F7 | Die konkreten Spiele je Gerät | offen |
| F8 | Sobald der Gameboy gewählt ist (Spielauswahl und im Spiel), werden **unterhalb des Spielbilds** die **Gameboy-Bedienelemente** gezeichnet: Steuerkreuz, **A**, **B**, **Start** und **Select**. Ein Mausklick auf ein Element wirkt wie die zugehörige Taste (siehe Tabelle unten), sodass der Gameboy komplett mit der Maus bedienbar ist. Die Tastatur funktioniert weiterhin. | festgelegt |
| F9 | Maussteuerung auch im Zimmer (Gerät anklicken) und bei Atari/Commodore-Spielen. | optional (wünschenswert, keine Pflicht) |

**Tastenbelegung der Gameboy-Bedienelemente (F8)**

| Element | Wirkt wie Taste | Bedeutung |
|---|---|---|
| Steuerkreuz | Pfeiltasten | Richtung / Auswahl im Menü |
| A | Z bzw. Enter | Bestätigen, Aktion im Spiel |
| B | X bzw. Esc | Zurück, zweite Aktion im Spiel |
| Start | Enter | Spiel starten / pausieren |
| Select | Backspace (Vorschlag) | je Spiel festzulegen, sonst ohne Funktion |

## 4. Rahmenbedingungen (aus den Projektanforderungen)

- Das Spiel läuft **vollständig im OOPGames-Framework**: Painter, Rules und Player werden im Hauptfenster gewählt
  und über „Start Game“ gestartet.
- **Kein eigenes Fenster, keine eigene Hauptschleife**, keine eigenen Event-Handler am `MainWindow`.
  Echtzeit läuft über `IGameRules2.TickGameCall()` / `IPaintGame2.TickPaintGameField()` (40-ms-Takt).
- Eingaben **nur** über die Klick-/Tastenereignisse, die das Framework an den Player liefert.
- Gezeichnet wird **nur** auf das übergebene Canvas.
- ⇒ Die Gameboy-Bedienelemente (F8) liegen **im unteren Teil des Canvas**, nicht unter dem Canvas im Fenster.
- Das Framework meldet nur **Maus-Klick (MouseDown)** und **Taste gedrückt (KeyDown)**, kein Loslassen.
  Ein Klick auf ein Bedienelement ist daher ein einzelner Tastendruck; „gedrückt halten“ per Maus ist nicht
  erkennbar. Spiele müssen mit einzelnen Richtungsimpulsen spielbar sein (z. B. Snake: Richtung wechseln).
- ⇒ **„Vollbild“ bedeutet: Das Spiel füllt das Canvas.** Das Canvas belegt im Hauptfenster die linke Hälfte;
  das Fenster selbst zu maximieren oder die Auswahllisten auszublenden, ist mit den Regeln nicht vereinbar.
- Namen: Klassen/Interfaces mit Gruppenpräfix (`G10…` / `IG10…`), Dateien `Vorname_Nachname_<Inhalt>.cs`,
  `Name`-Eigenschaften eindeutig mit Gruppe.
- Das Spiel muss „komplex/innovativ“ sein; ein einfacher Brettspiel-Klon reicht nicht. Das Konzept wird dem
  Dozenten **vor der Implementierung** vorgestellt.

## 5. Architektur (Vorschlag)

- **Hub** (Zimmer + Gerätewahl + Spielauswahl) und **Minispiele** sind getrennt.
- Jedes Minispiel implementiert ein gemeinsames Gruppen-Interface (Start, Update pro Tick mit gedrückten Tasten,
  Zeichnen, „Beenden angefordert“). Der Hub kennt nur dieses Interface.
- Ein Gerät ist eine Liste von Spiel-Einträgen. Neue Spiele werden nur in dieser Liste registriert; der Hub
  wird dafür nicht geändert.
- So können Teammitglieder parallel an verschiedenen Spielen arbeiten.
- Gameboy-Bedienelemente (F8): Der Player rechnet einen Klick (Canvas-Koordinaten) mit **derselben Layout-Berechnung wie
  der Painter** in eine Taste um und reiht sie in denselben Eingabepuffer ein wie echte Tastendrücke. Die
  Minispiele merken davon nichts.

## 6. Aktueller Stand (Prototyp, Commit `41cd1d9`, 2026-11-05)

Vorhanden:

- 3D-Zimmer über WPF `Viewport3D` mit Kamera und Geräte-Hervorhebung.
- Phasen Zimmer → Gerät → Kassette einlegen/auswerfen → Spielen; Steuerung per Tastatur.
- Minispiel-Interface `IB5RoomMiniGame`; Spiele: Münzen sammeln, Snake, Pong.
- Highscores als JSON unter `%LOCALAPPDATA%`.

Abweichungen von dieser Spezifikation:

| Thema | Prototyp | Spezifikation |
|---|---|---|
| Spieldarstellung | 160×144-Rahmen neben dem Zimmer | füllt das ganze Canvas (F5) |
| Maussteuerung | nur Tastatur | Gameboy komplett per Maus bedienbar (F8) |
| Namen | Präfix `B5`, Dateien `B5_Room*.cs` | Präfix `G10`, Dateien `Vorname_Nachname_*.cs` |

## 7. Offene Fragen

1. **Welche Spiele** gibt es je Gerät, und wer setzt welches um?
2. **Komplexität:** Reicht „Hub + mehrere kleine Spiele“ für die Bewertung, oder braucht mindestens ein Spiel
   mehr Tiefe (Levels, KI-Gegner, Physik)? → mit dem Dozenten klären.
3. **Bewegung im Zimmer:** Feste Kamera mit Auswahl, oder frei umsehen/bewegen?
4. **Highscores:** Speichern auf der Festplatte nötig/gewünscht?
5. **Abgabe:** Muss das Spiel am Ende nach `master` gemergt werden, oder wird der Branch `gruppe10` abgenommen?

## 8. Änderungshistorie

Neue Einträge oben. Jede Änderung an Abschnitt 1–5 bekommt eine Zeile und erhöht die Version
(0.x = Entwurf, 1.0 = vom Dozenten abgenommen).

| Version | Datum | Autor | Änderung |
|---|---|---|---|
| 0.5 | 2026-11-05 | Johannes Lacher | Neu F9 (optional): Maussteuerung im Zimmer und bei Atari/Commodore; offene Frage dazu entfällt. |
| 0.4 | 2026-11-05 | Johannes Lacher | F8 erweitert: auch A, B, Start und Select klickbar, sichtbar ab Gerätewahl; Tastenbelegung als Tabelle; offene Frage 3 eingegrenzt. |
| 0.3 | 2026-11-05 | Johannes Lacher | Neu F8: klickbares Gameboy-Steuerkreuz unter dem Spielbild; F5 entsprechend eingeschränkt; Rahmenbedingung „nur MouseDown/KeyDown“ ergänzt. |
| 0.2 | 2026-11-05 | Johannes Lacher | F2: PC durch Commodore-Heimcomputer ersetzt (wie im Prototyp); offene Frage zur PC-Ära entfällt. |
| 0.1 | 2026-11-05 | Johannes Lacher | Erste Fassung: Grundidee, Anforderungen F1–F7, Rahmenbedingungen, Stand des Prototyps, offene Fragen. |
