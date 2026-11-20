# Pong fertigstellen (Retro-Kinderzimmer, Gruppe 10)

| | |
|---|---|
| **Status** | umgesetzt, im laufenden Programm noch nicht von Hand gespielt |
| **Autor** | Johannes Lacher |
| **Branch** | `gruppe10` |
| **Code** | `Classes/10_Gruppe/Kinderzimmer90er/Pong/`, `Interfaces/10_Gruppe/Kinderzimmer90er/Johannes_Lacher_I_Pong.cs`, `Classes/10_Gruppe/Shared/Johannes_Lacher_GameBoyColors.cs` |

## 1. Ausgangslage

Pong am Game Boy war ein Platzhalter: Der rechte Schläger stand still in der Mitte, ein verpasster
Ball kostete nichts, es gab weder Gegner noch Menü, Pause oder Spielende, der Abprallwinkel war immer
gleich, und der Schläger sprang pro Tastendruck 8 Pixel.

## 2. Umsetzung

Gleicher Aufbau wie Snake und Muenzen sammeln (siehe `2026-11-19_Johannes_Lacher_Snake_Muenzen.md`):

| Schicht | Klassen |
|---|---|
| Spiellogik ohne WPF und Tasten | `G10_PongSession`, `G10_PongBall`, `G10_PongPaddle`, `G10_PongConfig` |
| Steuerung eines Schlägers (Strategy) | `IG10_PaddleControl` mit `G10_HumanPaddleControl` und `G10_CpuPaddleControl` |
| nur lesende Sicht | `IG10_PongView` |
| Darstellung | `G10_PongRenderer` (Game-Boy-Grün, gemeinsam mit Snake über `G10_GameBoyColors`) |
| Adapter zum Hub (Tasten, Highscore) | `G10_PongGame`, `G10_PongInputReader` |

- **Menü**: 1 oder 2 Spieler (Pfeile/WS und Enter oder Tasten 1/2).
- **Steuerung**: allein W/S oder Pfeile, zu zweit W/S links und Pfeile rechts. Gehaltene Tasten
  bewegen weiter (Tastaturzustand wie bei Muenzen sammeln, Wiederholungen zählen nicht als Druck).
- **Ball**: Abprallwinkel nach Trefferpunkt (Mitte gerade, Rand bis 55°), jeder Treffer +0,15 px/Tick
  bis 4,6. Der Treffer wird über die Bahn des letzten Ticks geprüft, damit der schnelle Ball nicht
  durch den 4 px breiten Schläger springt.
- **Computer**: folgt dem Ball, solange er auf ihn zukommt, aber langsamer als ein Spieler (1,8 statt
  2,6 px/Tick), und zielt auf eine zufällige Stelle seines Schlägers; steile schnelle Bälle kommen durch.
- **Ablauf**: Aufschlag nach 1,5 s in Richtung der Seite, die den Punkt verloren hat; wer zuerst 7 Punkte
  hat, gewinnt; Pause (Enter/Z/Leertaste/P, auch während des Aufschlags); danach zurück ins Menü.
- **Highscore**: längster Ballwechsel gegen den Computer (zu zweit zählt er nicht, man könnte ihn
  beliebig verlängern). Der alte Eintrag „Pong“ zählte Treffer und passt dazu.

## 3. Stand (2026-11-19): umgesetzt

Build ohne Fehler und Warnungen. Testprogramm im Scratchpad (nicht committet), zusammen mit den Tests
von Snake und Muenzen sammeln 48.065 Prüfungen bestanden:

- Menü, Aufschlagzeit, Pause beim Aufschlag und im Spiel, Schläger stoppen an der Wand.
- Ball: gerader Treffer, Randtreffer (Winkel nach oben bzw. unten zwischen 35° und 55°),
  Vorbeiflug, Wände; 300 Wechsel zwischen zwei Schlägern bei Höchsttempo ohne Durchrutschen.
- Partien zu zweit ohne Eingabe enden bei 7 Punkten, Aufschlag geht zum Verlierer, Ballwechsel zählen
  zu zweit nicht; ein untätiger Spieler verliert gegen den Computer.
- 20 Partien eines Bots, der dem Ball perfekt folgt, gegen den Computer: Bot gewinnt alle, aber der
  Computer holt 49 von 189 Punkten und spielt 3.724 Bälle zurück. Ein Mensch reagiert langsamer;
  ob der Computer zu leicht oder zu schwer ist, muss das Spielen zeigen (`CpuSpeed` in `G10_PongConfig`).
- Eingabeleser mit nachgebauter Tastatur, Adapter, Hub mit echtem `B5RoomPaint` (Game Boy → Pong →
  spielen → Esc), Bildschirmfotos aller Phasen.

**Noch von Hand zu prüfen**: Spielgefühl und Schwierigkeit gegen den Computer, zwei Spieler an einer
Tastatur (gleichzeitig gehaltene Tasten), Lesbarkeit im Vollbild.
