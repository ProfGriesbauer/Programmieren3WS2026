# Snake und Muenzen sammeln fertigstellen (Retro-Kinderzimmer, Gruppe 10)

| | |
|---|---|
| **Status** | umgesetzt, im laufenden Programm noch nicht von Hand gespielt |
| **Autor** | Johannes Lacher |
| **Branch** | `gruppe10` (entwickelt im Worktree `gruppe10-minigames`) |
| **Code** | `Classes/10_Gruppe/Kinderzimmer90er/Snake/`, `Classes/10_Gruppe/Kinderzimmer90er/Muenzen/`, `Interfaces/10_Gruppe/Kinderzimmer90er/Johannes_Lacher_I_Snake.cs`, `…_I_Muenzen.cs` |

## 1. Ziel

Snake (Game Boy) und Muenzen sammeln waren Platzhalter von je ca. 60 Zeilen: kein Start, keine
Pause, kein Game Over, Snake setzte sich beim Tod still zurück, Muenzen sammeln hatte kein Spielziel.
Beide werden zu vollständigen kleinen Spielen ausgebaut. Muenzen sammeln stand doppelt im Zimmer
(Atari und Commodore) und bleibt nur noch am **Commodore**: Am Atari steht schon Pac-Man, das selbst ein
Sammelspiel ist, und der Commodore hatte außer Tower Defense nichts.

## 2. Aufbau (wie Pac-Man)

| Schicht | Snake | Muenzen sammeln |
|---|---|---|
| Spiellogik ohne WPF und Tasten | `G10_SnakeSession`, `G10_SnakeBody`, `G10_TurnBuffer`, `G10_SnakeConfig` | `G10_CoinSession`, `G10_CoinArena`, `G10_CoinLevel`, `G10_CoinLayouts`, `G10_CoinPlayer`, `G10_DustBunny`, `G10_CoinPickup`, `G10_CoinConfig` |
| nur lesende Sicht | `IG10_SnakeView` | `IG10_CoinView`, `IG10_CoinPlayerView`, `IG10_PickupView` |
| Darstellung | `G10_SnakeRenderer` (Game-Boy-Grün) | `G10_CoinRenderer` (C64-Farben) |
| Adapter zum Hub (Tasten, Highscore) | `G10_SnakeGame` | `G10_CoinGame`, `G10_CoinInputReader` |

Wiederverwendet: `G10_Direction` und `G10_Tile` aus Pac-Man, `B5RoomDraw`/`B5RoomKeys`/`B5RoomHighscores`
aus dem Hub. Neu in `B5RoomDraw`: `TextCentered`. Die alten Klassen `B5RoomCoinGame` und `B5RoomSnakeGame`
sind entfernt. Der Highscore wird gespeichert, sobald er übertroffen ist, weil Esc das Spiel jederzeit
verlassen kann.

## 3. Regeln

**Snake**: 19 × 15 Felder mit Rand. Start wartet auf die erste Richtungstaste. Apfel = 10 × Level Punkte
und ein Glied mehr; alle 5 Äpfel ein Level schneller (5 bis ca. 12 Felder pro Sekunde, bis Level 9).
Nach jedem 4. Apfel erscheint für 6 s ein Käfer (Level × (20 + Restticks / 5) Punkte, kein Wachstum, mit
Zeitbalken). Wand oder eigener Körper → kurze Blinkanimation → GAME OVER; volles Feld → YOU WIN. Bis zu
zwei Richtungstasten zwischen zwei Schritten werden gemerkt; eine Umkehr in den eigenen Hals wird nie
angenommen (der Platzhalter ließ das mit zwei schnellen Tasten zu).

**Muenzen sammeln**: 19 × 13 Felder, vier Raumaufteilungen im Wechsel. Je Level 10 bis 20 Münzen in
45 s; immer bis zu drei Münzen gleichzeitig; Restsekunden × 10 als Bonus. Wollmäuse springen diagonal und
prallen an Wänden ab (Level 1: eine, bis fünf; jedes zweite Level schneller). Berührung oder abgelaufene
Zeit kostet ein Leben (drei), danach zwei Sekunden Schutz am Start. Alle 10 s Spielzeit liegt für 5 s ein
Diamant (100 Punkte) aus. Eine Taste bewegt sofort um ein Feld, gehalten läuft das Kind weiter. Das
Framework meldet nur KeyDown (mit den Wiederholungen des Betriebssystems); deshalb liest der Adapter
zusätzlich den Tastaturzustand und erkennt daran Halten, Loslassen und Wiederholungen.

**Beide**: Enter, Z, Leertaste oder P pausiert, nach GAME OVER startet Enter neu, Esc/X geht über den Hub
zurück.

## 4. Stand (2026-11-19): umgesetzt

Build von `OOPGames.csproj` ohne Fehler und Warnungen. Testprogramm im Scratchpad (nicht committet)
gegen die echten Klassen, 47.960 Prüfungen bestanden:

- Snake: Körper/Wachstum, Puffer (Umkehr, zwei Wendungen, voll), Start erst nach Richtung, Tempo,
  Pause, Tod an Wand, Sterbephase, Adapter (WASD/Pfeile, P, Neustart). Bot (Breitensuche) spielt
  6 Partien: Level 9 und Länge 80 erreicht, 86 Käfer gesehen, 81 gefressen, jede Punktänderung ist
  genau ein Apfel oder ein Käfer.
- Muenzen sammeln: alle Aufteilungen 19 × 13, alles erreichbar, keine Gänge der Breite 1; Wände und
  Rand blockieren; Tastenwiederholung (sofort, nach 6 Ticks, dann alle 3); Eingabeleser mit
  nachgebauter Tastatur (Wiederholung ist kein neuer Druck, letzte Taste gewinnt, kurzes Antippen,
  WASD); Pause; Bot spielt 5 Partien bis Level 7 mit Zeitbonus, Nachfüllen der Münzen und
  Levelwechsel; 30 Partien ohne Eingabe enden mit GAME OVER (56 Treffer durch Wollmäuse,
  34 × Zeit abgelaufen, Diamant genau nach 250 Ticks für 125 Ticks).
- Hub mit echtem `B5RoomPaint`: Atari hat nur noch Pac-Man, Muenzen sammeln steht genau einmal im Zimmer;
  Snake am Game Boy und Muenzen sammeln am Commodore einlegen, spielen und mit Esc verlassen; auch
  direkt nach Tower Defense (Fehler aus Prompt 107 tritt nicht wieder auf).
- Bildschirmfotos aller Phasen beider Spiele und im Hub geprüft.

Beim Testen gefunden und behoben: Wollmäuse, die rein diagonal hüpfen, (a) bleiben immer auf Feldern
einer Schachbrettfarbe, das Kind wäre auf der anderen Hälfte sicher, (b) pendelten an Blockecken ewig
zwischen zwei Feldern, (c) kamen nie in Gänge der Breite 1, z. B. den äußeren Ring der alten vierten
Aufteilung. Jetzt: gelegentlicher Seitschritt mit neuer Richtung, an Ecken zufälliges Abprallen,
Aufteilungen 2 und 4 ohne Gänge der Breite 1. Außerdem bei Snake Nahtlinien zwischen Gliedern beim
Skalieren (Verbindungsstücke überlappen jetzt) und beim Zeitbonus die Rundung (jetzt wie die Uhr).

**Noch von Hand zu prüfen** (im Test nicht möglich): echtes Halten und Loslassen der Tasten bei
Muenzen sammeln, Spielgefühl/Tempo beider Spiele, Lesbarkeit der kleinen Schrift im Vollbild.
