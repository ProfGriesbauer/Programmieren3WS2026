# Kompatibilitätstests Gruppe 1

Stand: 05.11.2026. Die Peer-Komponenten wurden auf `b1adc94` geprüft; die Gruppe-1-Anpassung ist in `efcb74d` enthalten.

Ein temporärer Windows-STA-Test prüft die sieben im Hauptfenster registrierten Vier-Gewinnt-Implementierungen (Gruppe 1, 2, 3/FG, 5, 8, 9 und 10). Geprüft werden Player-gegen-Rules, Human-/Computer-Züge auf fremden Feldern, jede Zugklasse gegen jede Rules-Klasse sowie alle Feld-Painter-Paare auf Annahme durch `CanBePaintedBy` und tatsächliche Ausgabe auf einem WPF-Canvas.

| Prüfung | Ergebnis |
|---|---|
| Gruppe-1-Human- und Computer-Player mit allen 7 Rules | Bestanden |
| Gruppe-1-Human- und Computer-Zug auf allen 6 fremden Feldern | Bestanden |
| Alle 7 Zugklassen mit allen 7 Rules | Bestanden |
| Alle 49 Rules-/Painter-Paare zeichnen Inhalte | Bestanden |
| Feld akzeptiert den Painter bei allen 49 Rules-/Painter-Paaren | 43/49; 6 Ablehnungen durch Gruppe-9-Rules |
| Gruppe-1-Human-Player wertet die 5 Painter mit `IConnectFourPainter`-Geometrie korrekt aus | Bestanden |

## Bekannte gruppenübergreifende Einschränkungen

- `G9_VierGewinntField.CanBePaintedBy` akzeptiert ausschließlich `IG9_4GewinntPainter`. Damit verweigert Gruppe 9 ihr Feld gegenüber den sechs anderen Vier-Gewinnt-Paintern, obwohl die Painter es zeichnen können. Gruppe-9-Rules funktionieren aktuell nur mit Gruppe-9-Painter. Das muss in Gruppe 9 auf den gemeinsamen `I4GWFeld`-/`IConnectFourPainter`-Vertrag umgestellt werden; Gruppe 1 ändert keinen fremden Gruppenordner.
- Gruppe 5 und Gruppe 9 implementieren noch nicht `IConnectFourPainter`. Deshalb können Gruppen-1-Human-Züge bei Auswahl ihrer Painter nicht deren Spaltengeometrie abfragen; die kompatiblen Painter von Gruppen 1, 2, 3/FG, 8 und 10 implementieren den gemeinsamen Vertrag.

Der automatisierte Test ersetzt keinen manuellen Lauf des WPF-Hauptfensters.
