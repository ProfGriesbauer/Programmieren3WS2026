# Plan: Vollbild und normale Ansicht im Framework (Gruppe 10)

| | |
|---|---|
| **Status** | Entwurf – Tastensteuerung folgt in einem zweiten Schritt |
| **Version** | 0.6 |
| **Branch** | `Framework` |
| **Betroffene Dateien** | `MainWindow.xaml`, `MainWindow.xaml.cs` |

Gruppe 10 erweitert das gemeinsame Framework um eine Vollbild-Ansicht. Das Hauptfenster wird von allen
Gruppen genutzt, deshalb sind die Änderungen **rein additiv**: keine Änderung an Interfaces, an der
Registrierung oder am Spielablauf (Start, Züge, Ticks).

---

## 1. Verhalten

| Ansicht | Fenster | Inhalt |
|---|---|---|
| **Normale Ansicht** (wie heute) | normales Fenster mit Rahmen | Canvas links, Listen rechts, untere Zeile mit `Start New Game`, **`Vollbild`**, **`Normale Ansicht`**, Statusfeld |
| **Vollbild** | randlos, maximiert, verdeckt auch die Taskleiste | nur das Canvas; Listen und untere Zeile ausgeblendet; kleiner **„✕“-Button** oben rechts über dem Spiel; **Status als Einblendung** oben links, nur wenn er Text enthält |

- Die zwei Buttons stehen neben `Start New Game` und bilden einen **zweiteiligen Umschalter**
  `[ Normale Ansicht | Vollbild ]`: Der Teil der aktiven Ansicht ist hervorgehoben (gedrückt dargestellt),
  ein Klick darauf bewirkt nichts. Ein Klick auf den anderen Teil wechselt die Ansicht. In der normalen
  Ansicht zeigt der Umschalter damit an, dass sie aktiv ist; im Vollbild ist die untere Zeile ausgeblendet,
  dort übernimmt der ✕-Button den Weg zurück.
- Zurück aus dem Vollbild: „✕“-Button (Maus) und – im Tasten-Schritt – **F11**. **Esc** bleibt beim Spiel
  (siehe Abschnitt 6).
- Beim Wechsel ins Vollbild erscheint für einige Sekunden der Hinweis „F11 oder ✕: Vollbild beenden“.
- Beim Zurückwechseln werden Größe, Position und Zustand (z. B. maximiert) des Fensters von vorher wiederhergestellt.
- Der „✕“-Button ist halbtransparent und wird beim Überfahren mit der Maus deutlich sichtbar, damit er das Spiel möglichst wenig verdeckt.
- **Status im Vollbild:** Der Text des Statusfelds („Player 1's turn!“, „Player 2 Won!“) erscheint als kleine,
  halbtransparente Einblendung oben links. Ist das Statusfeld leer, ist die Einblendung unsichtbar. Sie zeigt
  den Text nur an (nicht editierbar) und lässt Mausklicks zum Spiel durch.
- Aufteilung der Ecken, damit sich nichts überlagert: Status oben links, Hinweis „F11 oder ✕ …“ oben mittig,
  „✕“ oben rechts. Unten bleibt frei, weil viele Spiele dort eigene Leisten zeichnen (z. B. Kinderzimmer).
- Im Vollbild reicht das Spiel bis an den Bildschirmrand (keine Ränder).

## 2. Umsetzung

### `MainWindow.xaml`
- Namen vergeben: rechte Spalte (`SideColumn`), Listenbereich (`SidePanel`), untere Zeile (`ControlRow`),
  äußeres Grid (`RootGrid`).
- In der unteren Zeile neben `StartGame` der Umschalter aus zwei `RadioButton`s `NormalViewButton`
  („Normale Ansicht“) und `FullscreenButton` („Vollbild“) mit gemeinsamer `GroupName` und dem Aussehen eines
  `ToggleButton` (`Style="{StaticResource {x:Type ToggleButton}}"`). So ist immer genau ein Teil gedrückt,
  und WPF verhindert, dass der aktive Teil durch einen Klick „ausgeschaltet“ wird. Beide mit
  `Focusable = False` (siehe Randfall R4); kein eigenes Styling nötig, sie passen damit zum Button
  `Start New Game`.
- Den „✕“-Button **neben** das Canvas in dieselbe Grid-Zelle legen (oben rechts, über dem Canvas), ebenfalls
  `Focusable = False`. Er darf **kein Kind des Canvas** sein, weil viele Painter `canvas.Children.Clear()`
  aufrufen und ihn sonst löschen würden.
- Ebenso die Status-Einblendung `StatusOverlay` (oben links): ein `TextBlock` in einem halbtransparenten
  `Border`, dessen Text per Binding vom Statusfeld übernommen wird (`{Binding Text, ElementName=Status}`).
  Ein Style-Trigger setzt `Visibility = Collapsed`, wenn der Text leer ist. `IsHitTestVisible = False`, damit
  Klicks auf das Spiel nicht abgefangen werden. Maximale Breite mit `TextTrimming` („…“). Da der Text
  gebunden ist, muss an den Stellen, die `Status.Text` setzen, nichts geändert werden.
- Hinweis-Einblendung `FullscreenHint` (oben mittig), ebenfalls `IsHitTestVisible = False`.
- „✕“, Hinweis und `StatusOverlay` liegen gemeinsam in einem Container `FullscreenLayer` **ohne Hintergrund**
  (siehe Randfall R5). Der Code blendet nur diesen Container ein und aus. Würde der Code die Sichtbarkeit von
  `StatusOverlay` direkt setzen, würde dieser Wert den Style-Trigger überschreiben und die Einblendung bliebe
  auch bei leerem Status sichtbar.

### `MainWindow.xaml.cs`
- Feld `_IsFullscreen`; beide Methoden tun nichts, wenn der gewünschte Zustand schon aktiv ist.
- `EnterFullscreen()`:
  1. Fensterzustand merken: `WindowState`, `WindowStyle`, `ResizeMode` und **`RestoreBounds`** (Position und
     Größe des nicht maximierten Fensters – bei einem maximierten Fenster liefern `Left/Top/Width/Height`
     die maximierten Werte).
  2. `WindowState = Normal`, dann `WindowStyle = None`, `ResizeMode = NoResize`, dann `WindowState = Maximized`
     (diese Reihenfolge ist nötig, damit das Fenster auch die Taskleiste verdeckt; `NoResize` verhindert
     außerdem, dass der Größengriff unten rechts sichtbar bleibt).
  3. `SidePanel` und `ControlRow` ausblenden, Breite von `SideColumn` auf 0 setzen, Ränder von `RootGrid`
     und `PaintCanvas` auf 0 setzen, `FullscreenLayer` einblenden (die Status-Einblendung darin bleibt
     trotzdem unsichtbar, solange der Status leer ist).
  4. Hinweis einblenden und Timer für das Ausblenden (neu) starten.
- `ExitFullscreen()`: Hinweis sofort ausblenden und Timer stoppen, dann alles in umgekehrter Reihenfolge
  zurücksetzen; Position und Größe aus den gemerkten `RestoreBounds`, danach den gemerkten `WindowState`.
- `StateChanged` des Fensters: Wird das Fenster im Vollbild auf `Normal` gesetzt (z. B. Windows-Snap mit
  Win+←), `ExitFullscreen()` aufrufen (siehe Randfall R2).
- Die `Checked`-Ereignisse des Umschalters und der ✕-Button rufen nur diese beiden Methoden auf; der
  Tasten-Schritt nutzt später dieselben Methoden.
- Die Methoden setzen am Ende den passenden Teil des Umschalters auf `IsChecked = true`, damit er auch nach
  ✕, F11 oder Windows-Snap den richtigen Zustand zeigt. Das löst erneut `Checked` aus; der zweite Aufruf
  endet sofort am Flag `_IsFullscreen` (R3).
- **Neu zeichnen nach Größenänderung:** Painter ohne `IPaintGame2` zeichnen nur nach einem Zug neu. Nach dem
  Wechsel stünde ihr Bild sonst in alter Größe in der Ecke. Deshalb bei `PaintCanvas.SizeChanged` einmal
  `PaintGameField` aufrufen – **nur** wenn ein Spiel läuft, `CurrentField` nicht `null` ist,
  `CanBePaintedBy` passt und der Painter **kein** `IPaintGame2` ist (siehe Randfall R6). Das behebt nebenbei
  dasselbe Problem beim normalen Vergrößern des Fensters.

## 3. Randfälle

| # | Randfall | Behandlung |
|---|---|---|
| R1 | Fenster war vor dem Vollbild maximiert, verkleinert oder verschoben | Größe und Position aus `RestoreBounds` merken, beim Zurückwechseln zuerst diese setzen, dann den gemerkten `WindowState`. |
| R2 | Im Vollbild wird das Fenster per Windows-Snap (Win+←/→) oder Win+↓ verändert | Ohne Behandlung bliebe ein kleines, randloses Fenster im Vollbild-Layout ohne Weg zurück. `StateChanged`: wechselt das Fenster im Vollbild auf `Normal`, wird `ExitFullscreen()` aufgerufen. Minimieren und Wiederherstellen über die Taskleiste bleibt im Vollbild (Zustand `Maximized` wird von Windows wiederhergestellt). |
| R3 | Doppelte Aufrufe (✕ und F11 fast gleichzeitig, schnelles Umschalten) | Flag `_IsFullscreen`; `EnterFullscreen()`/`ExitFullscreen()` tun nichts, wenn der Zustand schon aktiv ist. Sonst würde z. B. der Vollbild-Zustand als „vorheriger Zustand“ gemerkt. |
| R4 | Neue Buttons behalten den Tastaturfokus | ✕ und beide Teile des Umschalters sind `Focusable = False`. Sonst würde nach einem Klick Enter oder Leertaste die Ansicht umschalten, statt beim Spiel anzukommen (gleiches Problem wie bei „Start New Game“). |
| R5 | Overlay-Container fängt Klicks ab | `FullscreenLayer` hat **keinen** Hintergrund (auch nicht `Transparent`), sonst würde er jeden Klick auf das Spiel abfangen. Status- und Hinweis-Einblendung haben `IsHitTestVisible = False`; nur der ✕-Button nimmt Klicks an. |
| R6 | Neuzeichnen bei Painter mit `IPaintGame2` | Wird ausgelassen: Diese Painter zeichnen ohnehin alle 40 ms, und ein zusätzlicher `PaintGameField`-Aufruf kann Nebenwirkungen haben (z. B. springt die Kamera im Kinderzimmer). |
| R7 | Fremde Painter beim Neuzeichnen | Bisher wurde `PaintGameField` nur nach Zügen aufgerufen. Ein Painter, der bei ungewohnter Größe eine Exception wirft, würde jetzt beim Größenändern abstürzen. → Testfall: jeder registrierte Painter einmal im Vollbild und beim Ziehen der Fenstergröße. Fehler werden der jeweiligen Gruppe als Issue gemeldet. |
| R8 | Ränder im Vollbild | `RootGrid` und `PaintCanvas` haben je 5 px Rand. Im Vollbild auf 0, beim Zurückwechseln wieder auf die gemerkten Werte. Klickkoordinaten bleiben korrekt, da sie relativ zum Canvas gerechnet werden. |
| R9 | Langer Statustext | Die Einblendung hat eine maximale Breite und kürzt mit „…“, damit sie nicht über das ganze Spiel reicht. |
| R10 | Hinweis-Timer | Beim Verlassen des Vollbilds wird der Hinweis sofort ausgeblendet und der Timer gestoppt; beim erneuten Wechsel startet er neu. Sonst würde ein alter Timer den nächsten Hinweis zu früh ausblenden. |
| R11 | ✕ verdeckt Anzeigen des Spiels oben rechts | Bekannte Einschränkung (z. B. Überschrift der Spieleliste im Kinderzimmer). Akzeptiert, da der Button klein und halbtransparent ist. |
| R12 | Vollbild ohne laufendes Spiel | Erlaubt; das Canvas ist leer. Spielstart nur über die normale Ansicht (Entscheidung zu offener Frage 3). Ein Klick ins leere Canvas stürzt heute ab – wird als eigene Aufgabe behoben (Abschnitt 8). |
| R13 | Fenster wird im Vollbild geschlossen (Alt+F4) | Nichts zu tun; der Zustand wird nicht gespeichert, beim nächsten Start öffnet die normale Ansicht. |
| R14 | Alt+Tab im Vollbild | Funktioniert, da das Fenster bewusst **nicht** `Topmost` ist. |

## 4. Auswirkungen auf andere Gruppen

- **Klicks** bleiben korrekt: `ClickSelection` rechnet schon heute relativ zum Canvas.
- **Painter mit fester Größe** (zeichnen z. B. immer 300 × 300 px) bleiben im Vollbild so groß wie bisher und
  werden nicht skaliert. Mitwachsen kann nur ein Painter, der `canvas.ActualWidth/ActualHeight` nutzt – das
  liegt bei der jeweiligen Gruppe.
- **Neu:** `PaintGameField` wird zusätzlich bei Größenänderungen aufgerufen (nur Painter ohne `IPaintGame2`,
  siehe R6/R7).
- Keine Änderung nötig, damit bestehende Spiele weiter funktionieren.

## 5. Testplan

| Test | Spiele |
|---|---|
| Normal → Vollbild → Normal, vor und während eines laufenden Spiels | TicTacToe (`X_`), Vier gewinnt einer Gruppe, Kinderzimmer (Gruppe 10, `IPaintGame2`) |
| Klicks treffen im Vollbild die richtige Spalte bzw. das richtige Feld | Vier gewinnt, TicTacToe |
| Fenster vorher maximiert / verkleinert / verschoben → wird genau so wiederhergestellt (R1) | – |
| Im Vollbild Win+←, Win+↓, Minimieren über die Taskleiste und Wiederherstellen (R2) | – |
| Mehrfaches schnelles Umschalten, „✕“ und Umschalter abwechselnd (R3) | – |
| Umschalter zeigt nach jedem Weg zurück (✕, Snap, später F11) wieder „Normale Ansicht“ als aktiv; Klick auf den aktiven Teil bewirkt nichts | – |
| Nach Klick auf „Vollbild“ bzw. „✕“: Enter und Leertaste kommen beim Spiel an (R4) | Kinderzimmer |
| Fenster auf zweitem Monitor → Vollbild auf diesem Monitor | – |
| Status-Einblendung: vor dem Start (leer) unsichtbar, nach dem Start sichtbar, zeigt „Won!“-Text, Klicks darunter erreichen das Spiel (R5) | TicTacToe, Vier gewinnt |
| Sehr langer Statustext wird gekürzt (R9) | – (Text von Hand ins Statusfeld tippen) |
| Hinweis: Vollbild kurz verlassen und wieder betreten → Hinweis bleibt die volle Zeit sichtbar (R10) | – |
| **Jeder registrierte Painter** einmal im Vollbild und beim Ziehen der Fenstergröße, ohne Absturz (R7) | alle |

## 6. Später: Tasten und Fokus (eigener Schritt)

- **F11** schaltet zwischen normaler Ansicht und Vollbild um. Das Framework behält F11 für sich und gibt die
  Taste nicht an das Spiel weiter. Die Prüfung auf F11 muss in `Window_KeyDown` **vor** der Zeile
  `if (_CurrentRules == null) return;` stehen, sonst funktioniert F11 nur während eines Spiels.
- **Esc wird vom Framework nicht genutzt** und geht wie bisher an das Spiel. Grund: Viele Spiele nutzen Esc
  selbst (z. B. Kinderzimmer: „zurück“). Würde das Framework Esc abfangen oder zusätzlich auswerten, würden
  diese Spiele nicht mehr oder nicht mehr richtig funktionieren. Geprüfte Alternativen und warum sie verworfen
  wurden:

  | Alternative | Verworfen, weil |
  |---|---|
  | Esc beendet immer das Vollbild | Spiele bekommen Esc nicht mehr. |
  | Esc beendet das Vollbild und geht zusätzlich ans Spiel | Spiel reagiert und Vollbild endet gleichzeitig. |
  | Esc nur, wenn das Spiel die Taste nicht braucht | Nicht feststellbar: Echtzeitspiele puffern Tasten und reagieren erst im nächsten Tick. |
  | Checkbox „Esc beendet Vollbild“ | Spieler müsste wissen, welches Spiel Esc nutzt. |
  | Neues optionales Interface, mit dem ein Spiel Esc für sich beansprucht | Neues Framework-Interface für alle Gruppen; kann bei Bedarf später ergänzt werden, ohne die F11-Lösung zu ändern. |
- **Fokus:** Nach `Start New Game` bleibt der Fokus heute auf dem Button bzw. in den Listen. Dann wertet der
  Button Enter/Leertaste als Klick, und die Pfeiltasten wechseln die Auswahl in den Listen, statt beim Spiel
  anzukommen. Geplant: Nach dem Start den Fokus auf die Spielfläche legen.

## 7. Offene Fragen

1. ~~**Esc-Konflikt**~~ – entschieden (Version 0.2): nur F11 schaltet um, Esc bleibt beim Spiel (Abschnitt 6).
2. ~~**Status im Vollbild**~~ – entschieden (Version 0.3): Einblendung oben links, nur wenn Text vorhanden.
   Hinweis: Das Framework setzt ab dem Spielstart nach jedem Zug einen Text („Player 1's turn!“), die
   Einblendung ist während eines Spiels also praktisch immer sichtbar. Falls sie stört, wäre eine Variante,
   sie nach jeder Änderung einige Sekunden zu zeigen und dann auszublenden.
3. ~~**„Start New Game“ im Vollbild**~~ – entschieden (Version 0.4): Ein Neustart geht nur über die normale Ansicht; im Vollbild gibt es keinen eigenen Button dafür.
4. **Abstimmung:** Wann und von wem wird `Framework` nach `master` gemergt? Ggf. mit dem Dozenten klären, da
   alle Gruppen betroffen sind.
5. ~~**Button „Normale Ansicht“ ist nie klickbar**~~ – entschieden (Version 0.6): zweiteiliger Umschalter,
   der aktive Teil ist hervorgehoben (Abschnitte 1 und 2).
6. ~~**Absturz bei Klick ins Canvas vor dem Spielstart**~~ – entschieden (Version 0.6): wird als eigene
   Aufgabe behoben, nicht zusammen mit dem Vollbild (Abschnitt 8).

## 8. Weitere Aufgaben (getrennt vom Vollbild)

| # | Aufgabe | Beschreibung | Status |
|---|---|---|---|
| A1 | Null-Prüfung bei Klick ins Canvas | `PaintCanvas_MouseDown` (`MainWindow.xaml.cs`) greift auf `_CurrentRules` zu, ohne auf `null` zu prüfen. Ein Klick ins Canvas vor dem ersten Spielstart wirft eine `NullReferenceException` und beendet das Programm. Der Fehler besteht schon heute, wird im Vollbild aber wahrscheinlicher, weil der ganze Bildschirm Klickfläche ist. Lösung: am Anfang der Methode `if (_CurrentRules == null) return;`, wie in `Window_KeyDown`. Test: Programm starten, ohne Spielstart ins Canvas klicken (normal und im Vollbild) → kein Absturz. Eigener Commit, damit die Fehlerbehebung getrennt nachvollziehbar ist. | offen |

## 9. Änderungshistorie

| Version | Datum | Autor | Änderung |
|---|---|---|---|
| 0.6 | 2026-11-12 | Johannes Lacher | Entschieden: zweiteiliger Umschalter „Normale Ansicht / Vollbild“ (zwei `RadioButton`s im `ToggleButton`-Stil, aktiver Teil hervorgehoben, Zustand nach jedem Wechsel synchron); Null-Prüfung beim Klick ins Canvas als eigene Aufgabe A1 in neuem Abschnitt 8. |
| 0.5 | 2026-11-12 | Johannes Lacher | Randfälle-Prüfung: neuer Abschnitt 3 (R1–R14), Umsetzung ergänzt (`RestoreBounds`, `_IsFullscreen`, `StateChanged`, `Focusable = False`, Container ohne Hintergrund, Ränder auf 0, Neuzeichnen nur ohne `IPaintGame2`, Hinweis-Timer), Testplan erweitert, F11 vor der Null-Prüfung, offene Fragen 5 und 6. Abschnitte ab 3 neu nummeriert. |
| 0.4 | 2026-11-12 | Johannes Lacher | Entschieden: „Start New Game“ nur in der normalen Ansicht, kein Neustart-Button im Vollbild. |
| 0.3 | 2026-11-12 | Johannes Lacher | Status im Vollbild als Einblendung oben links, nur sichtbar mit Inhalt (Binding an das Statusfeld, Klicks gehen durch); gemeinsamer Container `FullscreenLayer`; Ecken-Aufteilung und Testfall ergänzt. |
| 0.2 | 2026-11-12 | Johannes Lacher | Esc-Konflikt entschieden: nur F11 (und „✕“) beendet das Vollbild, Esc bleibt beim Spiel; verworfene Alternativen dokumentiert; Hinweis beim Wechsel ins Vollbild ergänzt. |
| 0.1 | 2026-11-12 | Johannes Lacher | Erste Fassung: Vollbild randlos mit nur Canvas, zwei Buttons neben „Start New Game“, „✕“ + Esc/F11 zum Zurückkehren. |
