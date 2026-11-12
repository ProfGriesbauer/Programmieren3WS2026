# Plan: Vollbild, normale Ansicht und Tastaturfokus im Framework (Gruppe 10)

| | |
|---|---|
| **Status** | Bereit zur Umsetzung |
| **Version** | 0.9 |
| **Branch** | `Framework` |
| **Betroffene Dateien** | `MainWindow.xaml`, `MainWindow.xaml.cs` |

Gruppe 10 erweitert das gemeinsame Framework um eine Vollbild-Ansicht und behebt, dass Tasten nicht
zuverlässig beim Spiel ankommen. Das Hauptfenster wird von allen Gruppen genutzt, deshalb sind die Änderungen
**rein additiv**: keine Änderung an Interfaces, an der Registrierung oder am Spielablauf (Start, Züge, Ticks).

Die Umsetzung ist in **sechs Blöcke** aufgeteilt. Jeder Block ist ein eigener Commit und hinterlässt ein
lauffähiges Programm.

---

## 1. Zielbild

| Ansicht | Fenster | Inhalt |
|---|---|---|
| **Normale Ansicht** (wie heute) | normales Fenster mit Rahmen | Canvas links, Listen rechts, untere Zeile: `Start New Game`, Umschalter `[ Normale Ansicht \| Vollbild ]`, Statusfeld |
| **Vollbild** | randlos, maximiert, verdeckt auch die Taskleiste | nur das Spiel bis an den Bildschirmrand; darüber Status oben links (nur mit Text), Hinweis oben mittig (einige Sekunden), ✕ oben rechts |

- **Wechsel:** Umschalter (Maus), ✕ (zurück), **F11** (beide Richtungen). **Esc** bleibt beim Spiel.
- **Zurück** stellt Größe, Position und Zustand des Fensters von vorher wieder her.
- **Neustart** nur über `Start New Game` in der normalen Ansicht.
- **Tasten** kommen nach dem Spielstart zuverlässig beim Spiel an (Pfeile, Enter, Leertaste, Tab), unabhängig
  davon, was vorher angeklickt wurde.

## 2. Reihenfolge und Abhängigkeiten

| Block | Inhalt | Hängt ab von | Commit-Nachricht (Vorschlag) |
|---|---|---|---|
| **B1** | Absturz bei Klick ins leere Canvas beheben | – | `G10: Fix crash when clicking the canvas before a game is started` |
| **B2** | Vollbild-Grundfunktion: Umschalter, Fensterwechsel, ✕ | – | `G10: Add fullscreen view with normal/fullscreen switch` |
| **B3** | F11 schaltet die Ansicht um | B2 | `G10: Toggle fullscreen with F11` |
| **B4** | Einblendungen im Vollbild: Status und Hinweis | B2, B3 (Hinweistext nennt F11) | `G10: Show status and hint overlays in fullscreen` |
| **B5** | Neu zeichnen nach Größenänderung | – (wirkt mit B2 zusammen) | `G10: Repaint the game when the canvas is resized` |
| **B6** | Tastaturfokus auf die Spielfläche | B1 (gleiche Methode `PaintCanvas_MouseDown`) | `G10: Keep keyboard focus on the game after start` |

Jeder Commit enthält den Plan bzw. die Prompt-Datei, soweit sie sich dafür geändert haben.

---

## B1 – Absturz bei Klick ins leere Canvas

**Ziel:** Ein Klick ins Canvas vor dem ersten Spielstart beendet das Programm nicht mehr.

**Ursache:** `PaintCanvas_MouseDown` greift auf `_CurrentRules` zu, ohne auf `null` zu prüfen. Der Fehler
besteht schon heute, wird im Vollbild aber wahrscheinlicher, weil der ganze Bildschirm Klickfläche ist.

**Änderung (`MainWindow.xaml.cs`):** Am Anfang von `PaintCanvas_MouseDown`
`if (_CurrentRules == null) return;` – wie bereits in `Window_KeyDown`.

**Tests:**
- Programm starten, ohne Spielstart ins Canvas klicken → kein Absturz.
- Danach Spiel starten und ins Canvas klicken → Züge funktionieren wie bisher.

---

## B2 – Vollbild-Grundfunktion

**Ziel:** Umschalten zwischen normaler Ansicht und Vollbild per Umschalter, zurück per ✕.

### Änderungen `MainWindow.xaml`
- Namen vergeben: äußeres Grid `RootGrid`, rechte Spalte `SideColumn`, Listenbereich `SidePanel`,
  untere Zeile `ControlRow`.
- **Umschalter** in der unteren Zeile neben `StartGame`: zwei `RadioButton`s `NormalViewButton`
  („Normale Ansicht“) und `FullscreenButton` („Vollbild“) mit gemeinsamer `GroupName` und dem Aussehen eines
  `ToggleButton` (`Style="{StaticResource {x:Type ToggleButton}}"`). So ist immer genau ein Teil gedrückt,
  ein Klick auf den aktiven Teil bewirkt nichts. Kein eigenes Styling nötig, sie passen zu `Start New Game`.
  Beide `Focusable = False` (R4).
- **Container `FullscreenLayer`** in derselben Grid-Zelle wie das Canvas, darüber gelegt, **ohne Hintergrund**
  (R5), im Normalfall ausgeblendet. Er darf **kein Kind des Canvas** sein, weil viele Painter
  `canvas.Children.Clear()` aufrufen und ihn sonst löschen würden.
- **✕-Button** im `FullscreenLayer` oben rechts: klein, halbtransparent, beim Überfahren deutlich sichtbar,
  `Focusable = False` (R4).

### Änderungen `MainWindow.xaml.cs`
- Feld `_IsFullscreen`. `EnterFullscreen()` und `ExitFullscreen()` tun nichts, wenn der Zustand schon aktiv
  ist (R3).
- `EnterFullscreen()`:
  1. Fensterzustand merken: `WindowState`, `WindowStyle`, `ResizeMode` und `RestoreBounds` (R1).
  2. `WindowState = Normal`, dann `WindowStyle = None`, `ResizeMode = NoResize`, dann `WindowState = Maximized`.
     Diese Reihenfolge ist nötig, damit das Fenster auch die Taskleiste verdeckt; `NoResize` blendet außerdem
     den Größengriff unten rechts aus.
  3. `SidePanel` und `ControlRow` ausblenden, Breite von `SideColumn` auf 0, Ränder von `RootGrid` und
     `PaintCanvas` auf 0 (R8), `FullscreenLayer` einblenden.
  4. `FullscreenButton.IsChecked = true` (Umschalter synchron halten).
- `ExitFullscreen()`: in umgekehrter Reihenfolge zurücksetzen; zuerst Position und Größe aus den gemerkten
  `RestoreBounds`, dann den gemerkten `WindowState`. Am Ende `NormalViewButton.IsChecked = true`.
- `Checked`-Ereignisse des Umschalters → `EnterFullscreen()` bzw. `ExitFullscreen()`; ✕-Klick →
  `ExitFullscreen()`. Das erneute `Checked` durch das Synchronisieren endet sofort am Flag (R3).
- `StateChanged` des Fensters: wechselt das Fenster im Vollbild auf `Normal` (z. B. Windows-Snap), wird
  `ExitFullscreen()` aufgerufen (R2).

### Randfälle in diesem Block
R1, R2, R3, R4, R5, R8, R11, R12, R13, R14 (siehe Abschnitt 3).

### Tests
- Normal → Vollbild → Normal, vor und während eines laufenden Spiels (TicTacToe, Vier gewinnt, Kinderzimmer).
- Klicks treffen im Vollbild die richtige Spalte bzw. das richtige Feld (Vier gewinnt, TicTacToe).
- Fenster vorher maximiert / verkleinert / verschoben → wird genau so wiederhergestellt (R1).
- Im Vollbild Win+←, Win+↓, Minimieren über die Taskleiste und Wiederherstellen (R2).
- Mehrfaches schnelles Umschalten, ✕ und Umschalter abwechselnd (R3).
- Umschalter zeigt nach jedem Weg zurück wieder „Normale Ansicht“ als aktiv; Klick auf den aktiven Teil
  bewirkt nichts.
- Nach Klick auf „Vollbild“ bzw. ✕: Enter und Leertaste kommen beim Spiel an (R4, Kinderzimmer).
- Fenster auf zweitem Monitor → Vollbild auf diesem Monitor.

---

## B3 – F11 schaltet um

**Ziel:** F11 wechselt in beide Richtungen, auch bevor ein Spiel gestartet wurde.

**Änderung (`MainWindow.xaml.cs`, `Window_KeyDown`):** Als **erste** Prüfung, noch vor
`if (_CurrentRules == null) return;`:
`if (e.Key == Key.F11)` → je nach `_IsFullscreen` `EnterFullscreen()` oder `ExitFullscreen()`,
`e.Handled = true`, `return`. F11 wird damit nie an das Spiel weitergegeben.

**Esc wird bewusst nicht genutzt** und geht wie bisher an das Spiel (Begründung in Anhang A).

**Tests:**
- F11 vor dem ersten Spielstart → Vollbild; erneut F11 → zurück.
- F11 während eines Spiels → Wechsel; das Spiel bekommt F11 nicht.
- Esc im Vollbild → kommt beim Spiel an (Kinderzimmer: „zurück“), Vollbild bleibt.
- Nach F11 zeigt der Umschalter den richtigen Zustand.

---

## B4 – Einblendungen im Vollbild

**Ziel:** Im Vollbild sind Status und Bedienhinweis sichtbar, ohne Klicks auf das Spiel abzufangen.

### Änderungen `MainWindow.xaml` (im `FullscreenLayer`)
- **`StatusOverlay`** oben links: `TextBlock` in halbtransparentem `Border`, Text per Binding vom Statusfeld
  (`{Binding Text, ElementName=Status}`). Style-Trigger: `Visibility = Collapsed`, wenn der Text leer ist.
  `IsHitTestVisible = False` (R5), maximale Breite mit `TextTrimming` („…“) (R9). Die Stellen, die
  `Status.Text` setzen, bleiben unverändert.
- **`FullscreenHint`** oben mittig: „F11 oder ✕: Vollbild beenden“, `IsHitTestVisible = False` (R5).
- Die Sichtbarkeit von `StatusOverlay` wird **nur** über den Style-Trigger gesteuert, nie direkt im Code –
  ein direkt gesetzter Wert würde den Trigger überschreiben. Der Code blendet nur den ganzen
  `FullscreenLayer` ein und aus.
- Ecken: Status links, Hinweis mittig, ✕ rechts. Unten bleibt frei, weil viele Spiele dort eigene Leisten
  zeichnen (z. B. Kinderzimmer).

### Änderungen `MainWindow.xaml.cs`
- `DispatcherTimer` für den Hinweis (einige Sekunden).
- `EnterFullscreen()`: Hinweis einblenden, Timer neu starten (R10).
- `ExitFullscreen()`: Hinweis sofort ausblenden, Timer stoppen (R10).

### Tests
- Status: vor dem Start (leer) unsichtbar, nach dem Start sichtbar, zeigt den „Won!“-Text; Klicks darunter
  erreichen das Spiel (TicTacToe, Vier gewinnt).
- Sehr langer Statustext wird mit „…“ gekürzt (Text von Hand ins Statusfeld tippen) (R9).
- Hinweis erscheint beim Wechsel ins Vollbild und verschwindet nach einigen Sekunden.
- Vollbild kurz verlassen und wieder betreten → Hinweis bleibt die volle Zeit sichtbar (R10).

**Bekannt:** Das Framework setzt ab dem Spielstart nach jedem Zug einen Text („Player 1's turn!“), die
Status-Einblendung ist während eines Spiels also praktisch immer sichtbar. Falls sie stört, könnte sie nach
jeder Änderung einige Sekunden gezeigt und dann ausgeblendet werden.

---

## B5 – Neu zeichnen nach Größenänderung

**Ziel:** Nach dem Wechsel der Ansicht (und beim Ziehen der Fenstergröße) steht das Spiel nicht mehr im alten
Zustand in der Ecke.

**Ursache:** Painter ohne `IPaintGame2` zeichnen nur nach einem Zug neu.

**Änderung (`MainWindow.xaml.cs`):** Handler für `PaintCanvas.SizeChanged`, der einmal `PaintGameField`
aufruft – **nur** wenn ein Spiel läuft, `CurrentField` nicht `null` ist, `CanBePaintedBy` passt und der Painter
**kein** `IPaintGame2` ist (R6).

**Tests:**
- Vollbild und Zurück mit TicTacToe und Vier gewinnt → Spielfeld wird sofort neu gezeichnet.
- Fenstergröße in der normalen Ansicht ziehen → ebenso.
- Kinderzimmer (`IPaintGame2`): Kamera springt beim Wechsel nicht (R6).
- **Jeder registrierte Painter** einmal im Vollbild und beim Ziehen der Fenstergröße, ohne Absturz (R7).
  Fehler fremder Painter werden der jeweiligen Gruppe als Issue gemeldet.

---

## B6 – Tastaturfokus auf die Spielfläche

**Ziel:** Nach dem Spielstart kommen alle Tasten beim Spiel an; Listen und Buttons reagieren nicht mehr
versehentlich.

**Ursache:** Das Framework liest Tasten in `Window_KeyDown` (Ereignis `KeyDown` des Fensters). Eine Taste geht
in WPF zuerst an das Steuerelement mit Tastaturfokus und steigt dann zum Fenster auf; behandelt das
Steuerelement sie selbst, kommt sie nie bei `Window_KeyDown` an. Folgen heute:
- Hat eine Liste rechts den Fokus, wechseln ↑/↓ deren Auswahl (←/→ kommen an, da eine senkrechte Liste sie
  nicht nutzt). Nebenbei ändert sich still die Auswahl für den nächsten Spielstart.
- Nach `Start New Game` behält der Button den Fokus und wertet Enter und Leertaste als erneuten Klick – das
  Spiel startet neu.
- Hat das Statusfeld den Fokus, werden Buchstaben hineingetippt; Tab wechselt immer den Fokus.

### Änderungen
1. `MainWindow.xaml`: `PaintCanvas` fokussierbar (`Focusable = True`, `FocusVisualStyle = {x:Null}`, damit kein
   gestrichelter Rahmen erscheint).
2. `StartGame_Click`: am Ende den Fokus auf `PaintCanvas` legen.
3. `PaintCanvas_MouseDown`: als Erstes den Fokus auf `PaintCanvas` legen – **vor** der Null-Prüfung aus B1,
   damit ein Klick ins Spiel den Fokus auch zurückholt, wenn noch kein Spiel läuft. Klicks auf Elemente, die
   ein Painter in das Canvas legt, steigen ebenfalls zum Canvas auf.
4. `Window_KeyDown`: wurde die Taste an ein laufendes Spiel weitergegeben, `e.Handled = true` setzen. Sonst
   verschiebt WPF bei Pfeiltasten und Tab anschließend den Fokus von der Spielfläche weg, und das nächste Enter
   startet das Spiel neu.
   **Per Test bestätigt** (2026-11-12, nachgebautes Layout des Hauptfensters, Tasten durch die
   WPF-Eingabeverarbeitung): Ohne `e.Handled` wandert der Fokus bei ↑/↓/Tab auf `Start New Game` und bei ←/→ in
   die Liste; mit `e.Handled = true` bleibt er bei allen getesteten Tasten (Pfeile, Tab, Enter, Leertaste,
   Buchstaben) auf dem Canvas. Das Spiel bekommt die Taste in beiden Fällen.

**Bewusst so gelassen:** Klickt der Spieler in eine Liste oder ins Statusfeld, bekommt dieses den Fokus und die
Tasten wirken dort. Ein Klick ins Spiel holt den Fokus zurück. Der Umschalter und ✕ sind nicht fokussierbar
(R4) und nehmen den Fokus nie weg.

**Danach in Gruppe 10:** Das Kinderzimmer verschiebt den Fokus bisher selbst (`TakeKeyboardFocus` im Branch
`gruppe10`); das wird überflüssig und kann entfernt werden.

### Tests
- Spieler wählen (Fokus in der Liste), Spiel starten → ↑/↓/←/→, Enter und Leertaste kommen beim Spiel an, das
  Spiel startet nicht neu, die Listenauswahl ändert sich nicht (Kinderzimmer, ein Spiel mit Pfeiltasten).
- Mehrfach Pfeiltasten und Tab drücken → Fokus bleibt auf der Spielfläche.
- In eine Liste klicken (↑/↓ wirken dort), dann ins Spiel klicken → Tasten wirken wieder im Spiel.
- F11 funktioniert weiterhin (B3).

---

## 3. Randfälle (Übersicht)

| # | Randfall | Behandlung | Block |
|---|---|---|---|
| R1 | Fenster war vor dem Vollbild maximiert, verkleinert oder verschoben | Größe und Position aus `RestoreBounds` merken (bei maximiertem Fenster liefern `Left/Top/Width/Height` die maximierten Werte); beim Zurückwechseln zuerst diese setzen, dann den gemerkten `WindowState`. | B2 |
| R2 | Im Vollbild Windows-Snap (Win+←/→) oder Win+↓ | Sonst bliebe ein kleines, randloses Fenster im Vollbild-Layout ohne Weg zurück. `StateChanged`: wechselt das Fenster im Vollbild auf `Normal`, wird `ExitFullscreen()` aufgerufen. Minimieren und Wiederherstellen über die Taskleiste bleibt im Vollbild. | B2 |
| R3 | Doppelte Aufrufe (✕ und F11 fast gleichzeitig, schnelles Umschalten, `Checked` durch Synchronisieren) | Flag `_IsFullscreen`; die Methoden tun nichts, wenn der Zustand schon aktiv ist. Sonst würde z. B. der Vollbild-Zustand als „vorheriger Zustand“ gemerkt. | B2 |
| R4 | Neue Buttons behalten den Tastaturfokus | ✕ und beide Teile des Umschalters `Focusable = False`. Sonst würde nach einem Klick Enter oder Leertaste die Ansicht umschalten, statt beim Spiel anzukommen. | B2 |
| R5 | Einblendungen fangen Klicks ab | `FullscreenLayer` ohne Hintergrund (auch nicht `Transparent`); Status und Hinweis `IsHitTestVisible = False`; nur ✕ nimmt Klicks an. | B2, B4 |
| R6 | Neuzeichnen bei Painter mit `IPaintGame2` | Wird ausgelassen: diese Painter zeichnen ohnehin alle 40 ms, ein zusätzlicher `PaintGameField`-Aufruf kann Nebenwirkungen haben (Kamera im Kinderzimmer springt). | B5 |
| R7 | Fremde Painter beim Neuzeichnen | Bisher wurde `PaintGameField` nur nach Zügen aufgerufen; ein Painter, der bei ungewohnter Größe eine Exception wirft, würde jetzt beim Größenändern abstürzen. Testfall mit allen registrierten Paintern; Fehler als Issue an die Gruppe. | B5 |
| R8 | Ränder im Vollbild | `RootGrid` und `PaintCanvas` haben je 5 px Rand; im Vollbild 0, danach die gemerkten Werte. Klickkoordinaten bleiben korrekt (relativ zum Canvas). | B2 |
| R9 | Langer Statustext | Maximale Breite, Kürzen mit „…“. | B4 |
| R10 | Hinweis-Timer | Beim Verlassen sofort ausblenden und Timer stoppen, beim erneuten Wechsel neu starten. Sonst blendet ein alter Timer den nächsten Hinweis zu früh aus. | B4 |
| R11 | ✕ verdeckt Anzeigen des Spiels oben rechts | Bekannte Einschränkung (z. B. Überschrift der Spieleliste im Kinderzimmer); akzeptiert, da klein und halbtransparent. | B2 |
| R12 | Vollbild ohne laufendes Spiel | Erlaubt, Canvas leer; Spielstart nur in der normalen Ansicht. Klick ins leere Canvas: B1. | B1, B2 |
| R13 | Fenster wird im Vollbild geschlossen (Alt+F4) | Nichts zu tun; beim nächsten Start öffnet die normale Ansicht. | B2 |
| R14 | Alt+Tab im Vollbild | Funktioniert, da das Fenster bewusst **nicht** `Topmost` ist. | B2 |

## 4. Auswirkungen auf andere Gruppen

- **Klicks** bleiben korrekt: `ClickSelection` rechnet schon heute relativ zum Canvas.
- **Painter mit fester Größe** (z. B. immer 300 × 300 px) bleiben im Vollbild so groß wie bisher und werden
  nicht skaliert. Mitwachsen kann nur ein Painter, der `canvas.ActualWidth/ActualHeight` nutzt – das liegt bei
  der jeweiligen Gruppe.
- **Neu:** `PaintGameField` wird zusätzlich bei Größenänderungen aufgerufen (nur Painter ohne `IPaintGame2`,
  R6/R7).
- **Neu:** Nach dem Spielstart liegt der Fokus auf der Spielfläche; an das Spiel weitergegebene Tasten werden als
  behandelt markiert. Für Spiele heißt das nur, dass Pfeile, Enter, Leertaste und Tab zuverlässig ankommen.
- Keine Änderung nötig, damit bestehende Spiele weiter funktionieren.

## 5. Offene Frage

- **Abstimmung:** Wann und von wem wird `Framework` nach `master` gemergt? Mit dem Dozenten klären, da alle
  Gruppen betroffen sind.

---

## Anhang A – Entscheidungen und verworfene Alternativen

| Thema | Entscheidung (Version) | Verworfene Alternativen |
|---|---|---|
| Was ist Vollbild? | Randlos, maximiert, nur das Spiel (0.1) | Nur Listen ausblenden; nur Fenster vergrößern |
| Zwei Buttons | Zweiteiliger Umschalter, aktiver Teil hervorgehoben (0.6) | Nur ein Button „Vollbild“ (der Button „Normale Ansicht“ wäre sonst nie klickbar) |
| Tasten zum Wechseln | Nur F11, Esc bleibt beim Spiel (0.2) | siehe Tabelle unten |
| Status im Vollbild | Einblendung oben links, nur mit Text (0.3) | Unten links: intuitiver, aber verdeckt Leisten der Spiele |
| Neustart im Vollbild | Nur über die normale Ansicht (0.4) | Eigener Button im Vollbild |
| Klick ins leere Canvas | Eigene Aufgabe B1 (0.6) | Zusammen mit dem Vollbild umsetzen |
| Tastaturfokus | Variante A: Fokus auf die Spielfläche + `e.Handled` (0.7, 0.8) | siehe Tabelle unten |

**Esc zum Verlassen des Vollbilds – verworfen, weil viele Spiele Esc selbst nutzen** (z. B. Kinderzimmer:
„zurück“):

| Alternative | Verworfen, weil |
|---|---|
| Esc beendet immer das Vollbild | Spiele bekommen Esc nicht mehr. |
| Esc beendet das Vollbild und geht zusätzlich ans Spiel | Spiel reagiert und Vollbild endet gleichzeitig. |
| Esc nur, wenn das Spiel die Taste nicht braucht | Nicht feststellbar: Echtzeitspiele puffern Tasten und reagieren erst im nächsten Tick. |
| Checkbox „Esc beendet Vollbild“ | Spieler müsste wissen, welches Spiel Esc nutzt. |
| Neues optionales Interface, mit dem ein Spiel Esc für sich beansprucht | Neues Framework-Interface für alle Gruppen; kann später ergänzt werden, ohne die F11-Lösung zu ändern. |

**Tastaturfokus:**

| Alternative | Verworfen, weil |
|---|---|
| B: Tasten per `PreviewKeyDown` vor allen Steuerelementen abfangen und bei laufendem Spiel immer ans Spiel geben | Unabhängig vom Fokus, aber deutlich stärkerer Eingriff: ändert den Tastenweg für alle Gruppen, und nach dem ersten Spielstart sind die Listen bis zum Programmende nicht mehr per Tastatur bedienbar. |
| C: Listen, Buttons und Statusfeld nicht fokussierbar machen | Tastatur für die Oberfläche auch vor dem Spielstart unbrauchbar; unklar, ob Listen dann noch per Maus auswählen; viele Einzeländerungen. |

## Anhang B – Änderungshistorie

| Version | Datum | Autor | Änderung |
|---|---|---|---|
| 0.9 | 2026-11-12 | Johannes Lacher | Umstrukturiert für die Umsetzung: sechs Blöcke B1–B6 mit Ziel, Änderungen, Randfällen, Tests und Commit-Nachricht; F11 (B3) vor die Einblendungen (B4) gezogen, weil der Hinweistext F11 nennt; Aufgabe A1 wird B1; Randfälle mit Block-Zuordnung; entschiedene Fragen und verworfene Alternativen in Anhang A. Inhaltlich keine Änderung. |
| 0.8 | 2026-11-12 | Johannes Lacher | Entschieden: `e.Handled = true` für an das Spiel weitergegebene Tasten (Schritt 4); Testergebnis zur Fokusverschiebung bei Pfeiltasten und Tab ergänzt. |
| 0.7 | 2026-11-12 | Johannes Lacher | Tastaturfokus: Ursachen beschrieben; Lösung Variante A (Fokus nach Spielstart und bei Klick ins Spiel auf das Canvas, an das Spiel weitergegebene Tasten als behandelt markieren); Alternativen B (`PreviewKeyDown`) und C (Steuerelemente nicht fokussierbar) mit Begründung verworfen. |
| 0.6 | 2026-11-12 | Johannes Lacher | Entschieden: zweiteiliger Umschalter „Normale Ansicht / Vollbild“ (zwei `RadioButton`s im `ToggleButton`-Stil, aktiver Teil hervorgehoben, Zustand nach jedem Wechsel synchron); Null-Prüfung beim Klick ins Canvas als eigene Aufgabe A1. |
| 0.5 | 2026-11-12 | Johannes Lacher | Randfälle-Prüfung: Abschnitt Randfälle (R1–R14), Umsetzung ergänzt (`RestoreBounds`, `_IsFullscreen`, `StateChanged`, `Focusable = False`, Container ohne Hintergrund, Ränder auf 0, Neuzeichnen nur ohne `IPaintGame2`, Hinweis-Timer), Testplan erweitert, F11 vor der Null-Prüfung. |
| 0.4 | 2026-11-12 | Johannes Lacher | Entschieden: „Start New Game“ nur in der normalen Ansicht, kein Neustart-Button im Vollbild. |
| 0.3 | 2026-11-12 | Johannes Lacher | Status im Vollbild als Einblendung oben links, nur sichtbar mit Inhalt (Binding an das Statusfeld, Klicks gehen durch); gemeinsamer Container `FullscreenLayer`; Ecken-Aufteilung und Testfall ergänzt. |
| 0.2 | 2026-11-12 | Johannes Lacher | Esc-Konflikt entschieden: nur F11 (und „✕“) beendet das Vollbild, Esc bleibt beim Spiel; verworfene Alternativen dokumentiert; Hinweis beim Wechsel ins Vollbild ergänzt. |
| 0.1 | 2026-11-12 | Johannes Lacher | Erste Fassung: Vollbild randlos mit nur Canvas, zwei Buttons neben „Start New Game“, „✕“ + Esc/F11 zum Zurückkehren. |
