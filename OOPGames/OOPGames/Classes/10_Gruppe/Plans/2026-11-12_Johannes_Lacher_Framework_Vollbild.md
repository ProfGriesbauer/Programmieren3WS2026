# Plan: Vollbild, normale Ansicht und Tastaturfokus im Framework (Gruppe 10)

| | |
|---|---|
| **Status** | Bereit zur Umsetzung |
| **Version** | 0.11 |
| **Branch** | `Framework` |
| **Betroffene Dateien** | `MainWindow.xaml`, `MainWindow.xaml.cs` |

Gruppe 10 erweitert das gemeinsame Framework um eine Vollbild-Ansicht und behebt, dass Tasten nicht
zuverlässig beim Spiel ankommen. Das Hauptfenster wird von allen Gruppen genutzt, deshalb sind die Änderungen
**rein additiv**: keine Änderung an Interfaces, an der Registrierung oder am Spielablauf (Start, Züge, Ticks).

Die Umsetzung ist in **vier Blöcke** (B2, B3, B5, B6) aufgeteilt; B1 und B4 wurden gestrichen, die Nummern
bleiben zur Nachvollziehbarkeit erhalten. Jeder Block ist ein eigener Commit und hinterlässt ein lauffähiges Programm.

---

## 1. Zielbild

| Ansicht | Fenster | Inhalt |
|---|---|---|
| **Normale Ansicht** (wie heute) | normales Fenster mit Rahmen | Canvas links, Listen rechts, untere Zeile: `Start New Game`, Umschalter `[ Normale Ansicht \| Vollbild ]`, Statusfeld |
| **Vollbild** | randlos, maximiert, verdeckt auch die Taskleiste | Spiel über die volle Breite bis an den oberen Bildschirmrand; darunter eine **Leiste über die ganze Breite** mit `Start New Game`, Umschalter und Statusfeld; Listen ausgeblendet |

- **Bedienleiste im Vollbild:** Die untere Zeile der normalen Ansicht bleibt im Vollbild sichtbar und reicht
  über die ganze Breite. Sie liegt **nicht über dem Spiel**, sondern unter ihm: Das Canvas wird um die Höhe der
  Leiste kleiner. So bleiben `Start New Game`, der Umschalter (Weg zurück) und der Status ohne Einblendungen
  erreichbar.
- **Wechsel:** Umschalter in der Leiste (Maus), **F11** (beide Richtungen). **Esc** bleibt beim Spiel.
- **Zurück** stellt Größe, Position und Zustand des Fensters von vorher wieder her.
- **Neustart** über `Start New Game` in beiden Ansichten.
- **Tasten** kommen nach dem Spielstart zuverlässig beim Spiel an (Pfeile, Enter, Leertaste, Tab), unabhängig
  davon, was vorher angeklickt wurde.

## 2. Reihenfolge und Abhängigkeiten

| Block | Inhalt | Hängt ab von | Commit-Nachricht (Vorschlag) |
|---|---|---|---|
| ~~**B1**~~ | ~~Absturz bei Klick ins leere Canvas beheben~~ – gestrichen, per Klick nicht erreichbar | – | – |
| **B2** | Vollbild-Grundfunktion: Umschalter, Fensterwechsel, Bedienleiste unten | – | `G10: Add fullscreen view with normal/fullscreen switch` |
| **B3** | F11 schaltet die Ansicht um | B2 | `G10: Toggle fullscreen with F11` |
| ~~**B4**~~ | ~~Einblendungen im Vollbild: Status und Hinweis~~ – gestrichen, die Bedienleiste zeigt Status und Umschalter | – | – |
| **B5** | Neu zeichnen nach Größenänderung | – (wirkt mit B2 zusammen) | `G10: Repaint the game when the canvas is resized` |
| **B6** | Tastaturfokus auf die Spielfläche | – | `G10: Keep keyboard focus on the game after start` |

Jeder Commit enthält den Plan bzw. die Prompt-Datei, soweit sie sich dafür geändert haben.

---

## B1 – ~~Absturz bei Klick ins leere Canvas~~ (gestrichen)

**Gestrichen (Version 0.10).** Geplant war eine Null-Prüfung am Anfang von `PaintCanvas_MouseDown`, weil die
Methode ohne Prüfung auf `_CurrentRules` zugreift. Der Absturz ist mit einem echten Mausklick aber **nicht
erreichbar**:
- `PaintCanvas` hat keinen Hintergrund. In WPF trifft ein Mausklick ein solches Element nur dort, wo etwas
  gezeichnet ist. Vor dem ersten Spielstart ist das Canvas leer, `PaintCanvas_MouseDown` läuft also nie –
  auch nicht im Vollbild.
- Nach dem Spielstart ist `_CurrentRules` gesetzt; die Methode kann nicht mehr abstürzen.

Belegt per Hit-Test am echten `MainWindow` (2026-11-12): leeres Canvas ohne Hintergrund → Klick trifft nichts;
mit `Background = Transparent` → Klick trifft das Canvas. Ein erster Test hatte das Klick-Ereignis direkt
ausgelöst und so den Hit-Test umgangen; der dort gezeigte Absturz kommt im Programm nicht vor.

**Folge:** Das Framework bekommt keinen Hintergrund für das Canvas (siehe B6), damit bleibt der Fall
unerreichbar und keine Änderung nötig.

---

## B2 – Vollbild-Grundfunktion

**Ziel:** Umschalten zwischen normaler Ansicht und Vollbild per Umschalter. Im Vollbild bleibt die untere
Zeile als Bedienleiste über die ganze Breite sichtbar.

### Änderungen `MainWindow.xaml`
- Namen vergeben: äußeres Grid `RootGrid`, rechte Spalte `SideColumn`, Listenbereich `SidePanel`,
  untere Zeile `ControlRow`.
- **Umschalter** in der unteren Zeile neben `StartGame`: zwei `RadioButton`s `NormalViewButton`
  („Normale Ansicht“) und `FullscreenButton` („Vollbild“) mit gemeinsamer `GroupName` und dem Aussehen eines
  `ToggleButton` (`Style="{StaticResource {x:Type ToggleButton}}"`). So ist immer genau ein Teil gedrückt,
  ein Klick auf den aktiven Teil bewirkt nichts. Kein eigenes Styling nötig, sie passen zu `Start New Game`.
  Beide `Focusable = False` (R4).
- **Bedienleiste:** Die untere Zeile (`ControlRow`) steht in der linken Spalte direkt unter dem Canvas, in einer
  eigenen Grid-Zeile mit Höhe `Auto`. Wird im Vollbild nur die rechte Spalte ausgeblendet, reicht die linke
  Spalte – und damit die Leiste – automatisch über die ganze Breite. Das Statusfeld (Spalte `1*`) füllt den
  Rest der Leiste. Kein zusätzliches Element nötig, keine Einblendung über dem Spiel.

### Änderungen `MainWindow.xaml.cs`
- Feld `_IsFullscreen`. `EnterFullscreen()` und `ExitFullscreen()` tun nichts, wenn der Zustand schon aktiv
  ist (R3).
- `EnterFullscreen()`:
  1. Fensterzustand merken: `WindowState`, `WindowStyle`, `ResizeMode` und Größe/Position (R1): bei einem
     maximierten Fenster aus `RestoreBounds`, bei einem normalen Fenster so, wie es angezeigt wird
     (`Left/Top/ActualWidth/ActualHeight`) – `RestoreBounds` lieferte bei einem angedockten (Snap) Fenster die
     Größe von vor dem Andocken.
  2. `WindowState = Normal`, dann `WindowStyle = None`, `ResizeMode = NoResize`, dann `WindowState = Maximized`.
     Diese Reihenfolge ist nötig, damit das Fenster auch die Taskleiste verdeckt; `NoResize` blendet außerdem
     den Größengriff unten rechts aus. **Per Test bestätigt:** Ein bereits maximiertes Fenster, das nur randlos
     wird, bleibt so groß wie der Arbeitsbereich (1166 statt 1200 px hoch), die Taskleiste bleibt sichtbar.
     Der kurze Zwischenschritt über `Normal` (sichtbares Flackern bei vorher maximiertem Fenster) ist daher
     nötig.
  3. `SidePanel` ausblenden, Breite von `SideColumn` auf 0, Ränder von `RootGrid` und `PaintCanvas` auf 0
     (R8). `ControlRow` bleibt sichtbar.
  4. `FullscreenButton.IsChecked = true` (Umschalter synchron halten).
- `ExitFullscreen(restoreWindow)`: in umgekehrter Reihenfolge zurücksetzen; bei `restoreWindow = true` zuerst
  die gemerkte Position und Größe, dann den gemerkten `WindowState`. Am Ende `NormalViewButton.IsChecked = true`.
- `Checked`-Ereignisse des Umschalters → `EnterFullscreen()` bzw. `ExitFullscreen(true)`. Das erneute
  `Checked` durch das Synchronisieren endet sofort am Flag (R3).
- `StateChanged` des Fensters: wechselt das Fenster im Vollbild auf `Normal` (z. B. Windows-Snap), wird
  `ExitFullscreen(false)` aufgerufen: Layout und Fensterrahmen zurück, aber die Position und Größe, die Windows
  gerade gewählt hat, bleiben (R2).

### Randfälle in diesem Block
R1, R2, R3, R4, R8, R12, R13, R14, R15, R16 (siehe Abschnitt 3).

### Tests
- Normal → Vollbild → Normal, vor und während eines laufenden Spiels (TicTacToe, Vier gewinnt, Kinderzimmer).
- Im Vollbild: Leiste über die ganze Breite am unteren Rand, Canvas direkt darüber bis an den oberen und die
  seitlichen Bildschirmränder, Leiste überlappt das Canvas nicht (R15).
- `Start New Game` im Vollbild startet ein neues Spiel, das Fenster bleibt im Vollbild (R16).
- Klicks treffen im Vollbild die richtige Spalte bzw. das richtige Feld (Vier gewinnt, TicTacToe).
- Fenster vorher maximiert / verkleinert / verschoben → wird genau so wiederhergestellt (R1).
- Im Vollbild Win+←, Win+↓, Minimieren über die Taskleiste und Wiederherstellen (R2).
- Mehrfaches schnelles Umschalten (R3).
- Umschalter zeigt nach jedem Weg zurück wieder „Normale Ansicht“ als aktiv; Klick auf den aktiven Teil
  bewirkt nichts.
- Nach Klick auf den Umschalter: Enter und Leertaste kommen beim Spiel an (R4, Kinderzimmer).
- Fenster auf zweitem Monitor → Vollbild auf diesem Monitor.

**Stand (2026-11-12): umgesetzt nach Version 0.11.** Gegenüber der ersten Umsetzung (0.10): `ControlRow`
bleibt im Vollbild sichtbar, `FullscreenLayer` mit ✕-Button und Klick-Handler entfernt. Dazu drei Punkte aus
einem Code-Review: Größe eines normalen Fensters wird so gemerkt, wie es angezeigt wird (R1, angedockte
Fenster); bei Windows-Snap im Vollbild bleibt die neue Platzierung (R2); Kommentar zum nicht fokussierbaren
Umschalter berichtigt (er nimmt den Fokus nicht weg, gibt ihn aber auch nicht ans Spiel – das macht B6).
Testprogramm mit dem echten `MainWindow`, alle Prüfungen bestanden: randlos, maximiert, ohne Größengriff;
Leiste über die ganze Breite am unteren Rand (1920 px, 38 px hoch), Canvas darüber 1920 × 1162 ab der
Oberkante, keine Überlappung (R15); TicTacToe per `Start New Game` im Vollbild gestartet, Status
„Player 1's turn!“, Fenster bleibt im Vollbild (R16); Rückkehr mit gleicher Größe und Position, auch aus
maximiertem Fenster (R1) und für ein angedocktes Fenster (0, 0, 960 × 1100); Snap im Vollbild behält die von
Windows gesetzte Platzierung, Minimieren und Wiederherstellen bleibt im Vollbild (R2); doppelte Aufrufe (R3);
Umschalter nicht fokussierbar (R4); Ränder zurück auf 5 px (R8). Screenshots geprüft.
**Bekannt bis B5:** Beim Wechsel der Ansicht zeichnen Painter ohne `IPaintGame2` erst beim nächsten Zug neu;
bis dahin steht das alte Bild (z. B. zentriert für die alte Größe) im Canvas.
Noch von Hand im laufenden Programm zu prüfen: echte Klicks in einem Spiel im Vollbild, echtes Windows-Snap
(Win+←) und Win+↓, zweiter Monitor.

---

## B3 – F11 schaltet um

**Ziel:** F11 wechselt in beide Richtungen, auch bevor ein Spiel gestartet wurde.

**Änderung (`MainWindow.xaml.cs`, `Window_KeyDown`):** Als **erste** Prüfung, noch vor
`if (_CurrentRules == null) return;`:
`if (e.Key == Key.F11)` → je nach `_IsFullscreen` `EnterFullscreen()` oder `ExitFullscreen()`,
`e.Handled = true`, `return`. F11 wird damit nie an das Spiel weitergegeben.

**Änderung (`MainWindow.xaml`):** Tooltips an beiden Teilen des Umschalters nennen die Taste
(„Normale Ansicht (F11)“, „Vollbild (F11)“). Da es keinen eingeblendeten Hinweis mehr gibt (B4 gestrichen),
ist F11 so auffindbar.

**Esc wird bewusst nicht genutzt** und geht wie bisher an das Spiel (Begründung in Anhang A).

**Tests:**
- F11 vor dem ersten Spielstart → Vollbild; erneut F11 → zurück.
- F11 während eines Spiels → Wechsel; das Spiel bekommt F11 nicht.
- Esc im Vollbild → kommt beim Spiel an (Kinderzimmer: „zurück“), Vollbild bleibt.
- Nach F11 zeigt der Umschalter den richtigen Zustand.
- Tooltips am Umschalter nennen F11.

**Stand (2026-11-12): umgesetzt.** Testprogramm mit dem echten `MainWindow` und einem Testspieler, der
aufzeichnet, welche Tasten beim Spiel ankommen; Tasten durch die WPF-Eingabeverarbeitung. Vor der Änderung
9 von 16 Prüfungen fehlgeschlagen (F11 schaltete nicht um und kam beim Spiel an), danach alle bestanden:
F11 schaltet vor dem ersten Spielstart, mit Fokus in einer Liste und im Statusfeld sowie während eines Spiels
in beide Richtungen; das Spiel bekommt F11 nie; Esc kommt beim Spiel an und lässt das Vollbild bestehen;
der Umschalter zeigt nach F11 den richtigen Zustand; beide Tooltips nennen F11. Die B2-Tests laufen
weiterhin durch.
Noch von Hand zu prüfen: F11 auf einer echten Tastatur (manche Laptops brauchen dafür die Fn-Taste).

---

## B4 – ~~Einblendungen im Vollbild~~ (gestrichen)

**Gestrichen (Version 0.11).** Geplant waren eine Status-Einblendung oben links, ein Hinweis „F11 oder ✕:
Vollbild beenden“ oben mittig und ein ✕-Button oben rechts, jeweils über dem Spiel. Mit der Bedienleiste im
Vollbild (B2) sind sie überflüssig:
- Der **Status** steht im Statusfeld der Leiste.
- Der **Weg zurück** ist der Umschalter in der Leiste; der ✕-Button entfällt.
- **F11** nennen die Tooltips am Umschalter (B3).

Damit verdeckt im Vollbild nichts das Spiel, und die Randfälle der Einblendungen (Klicks durchlassen, langer
Statustext, Hinweis-Timer, ✕ verdeckt Spielanzeigen) entfallen.

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
3. `PaintCanvas_MouseDown`: als Erstes den Fokus auf `PaintCanvas` legen. Klicks auf Elemente, die ein
   Painter in das Canvas legt, steigen ebenfalls zum Canvas auf. Ein Klick kommt nur dort an, wo der Painter
   etwas gezeichnet oder einen Hintergrund gesetzt hat; fast alle Painter setzen `canvas.Background` (9 von
   10 geprüften, dazu das Kinderzimmer mit einer flächendeckenden Ebene). Das Framework setzt bewusst
   **keinen** eigenen Hintergrund für das Canvas: Klicks auf bisher leere Stellen kämen sonst bei den Spielern
   aller Gruppen an (geändertes Klickverhalten), und `PaintCanvas_MouseDown` würde vor dem Spielstart
   erreichbar und dort abstürzen (siehe B1).
4. `Window_KeyDown`: wurde die Taste an ein laufendes Spiel weitergegeben, `e.Handled = true` setzen. Sonst
   verschiebt WPF bei Pfeiltasten und Tab anschließend den Fokus von der Spielfläche weg, und das nächste Enter
   startet das Spiel neu.
   **Per Test bestätigt** (2026-11-12, nachgebautes Layout des Hauptfensters, Tasten durch die
   WPF-Eingabeverarbeitung): Ohne `e.Handled` wandert der Fokus bei ↑/↓/Tab auf `Start New Game` und bei ←/→ in
   die Liste; mit `e.Handled = true` bleibt er bei allen getesteten Tasten (Pfeile, Tab, Enter, Leertaste,
   Buchstaben) auf dem Canvas. Das Spiel bekommt die Taste in beiden Fällen.

**Bewusst so gelassen:** Klickt der Spieler in eine Liste oder ins Statusfeld, bekommt dieses den Fokus und die
Tasten wirken dort. Ein Klick ins Spiel holt den Fokus zurück. Der Umschalter ist nicht fokussierbar (R4) und
nimmt den Fokus nie weg. Da `Start New Game` jetzt auch im Vollbild sichtbar ist, ist Schritt 2 (Fokus nach dem
Start aufs Canvas) dort genauso wichtig: sonst würde Enter im Vollbild das Spiel neu starten.

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
| R1 | Fenster war vor dem Vollbild maximiert, verkleinert oder verschoben | Maximiert: Größe und Position aus `RestoreBounds` (`Left/Top/Width/Height` liefern die maximierten Werte). Normal, auch angedockt: so, wie es angezeigt wird (`RestoreBounds` lieferte die Größe von vor dem Andocken). Beim Zurückwechseln zuerst Größe und Position setzen, dann den gemerkten `WindowState`. | B2 |
| R2 | Im Vollbild Windows-Snap (Win+←/→) oder Win+↓ | Sonst bliebe ein kleines, randloses Fenster im Vollbild-Layout. `StateChanged`: wechselt das Fenster im Vollbild auf `Normal`, wird `ExitFullscreen(false)` aufgerufen – Layout und Rahmen zurück, die von Windows gewählte Platzierung bleibt (nicht mit der alten überschreiben). Minimieren und Wiederherstellen über die Taskleiste bleibt im Vollbild. | B2 |
| R3 | Doppelte Aufrufe (Umschalter und F11 fast gleichzeitig, schnelles Umschalten, `Checked` durch Synchronisieren) | Flag `_IsFullscreen`; die Methoden tun nichts, wenn der Zustand schon aktiv ist. Sonst würde z. B. der Vollbild-Zustand als „vorheriger Zustand“ gemerkt. | B2 |
| R4 | Neue Buttons behalten den Tastaturfokus | Beide Teile des Umschalters `Focusable = False`. Sonst würde nach einem Klick Enter oder Leertaste die Ansicht umschalten, statt beim Spiel anzukommen. | B2 |
| ~~R5~~ | ~~Einblendungen fangen Klicks ab~~ | Entfällt (0.11): keine Einblendungen über dem Spiel mehr. | – |
| R6 | Neuzeichnen bei Painter mit `IPaintGame2` | Wird ausgelassen: diese Painter zeichnen ohnehin alle 40 ms, ein zusätzlicher `PaintGameField`-Aufruf kann Nebenwirkungen haben (Kamera im Kinderzimmer springt). | B5 |
| R7 | Fremde Painter beim Neuzeichnen | Bisher wurde `PaintGameField` nur nach Zügen aufgerufen; ein Painter, der bei ungewohnter Größe eine Exception wirft, würde jetzt beim Größenändern abstürzen. Testfall mit allen registrierten Paintern; Fehler als Issue an die Gruppe. | B5 |
| R8 | Ränder im Vollbild | `RootGrid` und `PaintCanvas` haben je 5 px Rand; im Vollbild 0, danach die gemerkten Werte. Klickkoordinaten bleiben korrekt (relativ zum Canvas). | B2 |
| ~~R9~~ | ~~Langer Statustext~~ | Entfällt (0.11): das Statusfeld in der Leiste ist so breit wie bisher und scrollt wie bisher. | – |
| ~~R10~~ | ~~Hinweis-Timer~~ | Entfällt (0.11): kein eingeblendeter Hinweis mehr. | – |
| ~~R11~~ | ~~✕ verdeckt Anzeigen des Spiels oben rechts~~ | Entfällt (0.11): kein ✕-Button mehr. | – |
| R12 | Vollbild ohne laufendes Spiel | Erlaubt, Canvas leer; ein Spiel lässt sich direkt im Vollbild über `Start New Game` starten. Ein Klick ins leere Canvas trifft nichts (kein Hintergrund) und löst nichts aus (siehe B1). | B2 |
| R13 | Fenster wird im Vollbild geschlossen (Alt+F4) | Nichts zu tun; beim nächsten Start öffnet die normale Ansicht. | B2 |
| R14 | Alt+Tab im Vollbild | Funktioniert, da das Fenster bewusst **nicht** `Topmost` ist. | B2 |
| R15 | Leiste nimmt Platz vom Canvas | Gewollt: Das Canvas ist im Vollbild so hoch wie der Bildschirm abzüglich der Leiste (rund 45 px) und überlappt sie nicht. Painter, die `canvas.ActualHeight` nutzen, passen sich an; das Neuzeichnen übernimmt B5. Klickkoordinaten bleiben korrekt (relativ zum Canvas). | B2, B5 |
| R16 | `Start New Game` im Vollbild | Startet wie gewohnt ein neues Spiel; das Fenster bleibt im Vollbild. Die Spielauswahl (Listen) ist im Vollbild nicht sichtbar, es wird also die zuletzt gewählte Kombination gestartet. Danach liegt der Fokus nach B6 auf dem Spiel, sonst würde Enter das Spiel erneut starten. | B2, B6 |

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
| Was ist Vollbild? | Randlos, maximiert, Spiel plus Bedienleiste unten (0.1, Leiste seit 0.11) | Nur Listen ausblenden; nur Fenster vergrößern; nur das Spiel ohne Leiste (bis 0.10) |
| Bedienleiste im Vollbild | Untere Zeile bleibt sichtbar, reicht über die ganze Breite und macht das Canvas kleiner, statt darüber zu liegen (0.11) | Einblendungen über dem Spiel (Status, Hinweis, ✕; bis 0.10): verdecken Teile des Spiels und brauchen eigene Sonderfälle |
| Zwei Buttons | Zweiteiliger Umschalter, aktiver Teil hervorgehoben (0.6); im Vollbild in der Leiste (0.11) | Nur ein Button „Vollbild“ (der Button „Normale Ansicht“ wäre sonst nie klickbar); ✕-Button über dem Spiel (bis 0.10) |
| Tasten zum Wechseln | Nur F11, Esc bleibt beim Spiel (0.2); F11 in den Tooltips des Umschalters (0.11) | siehe Tabelle unten; eingeblendeter Hinweis (bis 0.10) |
| Status im Vollbild | Statusfeld in der Leiste (0.11) | Einblendung oben links (0.3–0.10); unten links über dem Spiel: verdeckt Leisten der Spiele |
| Neustart im Vollbild | `Start New Game` in der Leiste (0.11) | Nur über die normale Ansicht (0.4–0.10) |
| Klick ins leere Canvas | Keine Änderung; per Klick nicht erreichbar (0.10, vorher eigene Aufgabe seit 0.6) | Null-Prüfung als reine Absicherung behalten |
| Hintergrund für das Canvas | Kein eigener Hintergrund durch das Framework (0.10) | `Background = Transparent`, damit auch Klicks auf leere Stellen den Fokus holen: ändert das Klickverhalten für alle Gruppen |
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
| 0.11 | 2026-11-12 | Johannes Lacher | Bedienleiste im Vollbild: die untere Zeile (`Start New Game`, Umschalter, Statusfeld) bleibt sichtbar, reicht über die ganze Breite und verkleinert das Canvas statt darüber zu liegen. Neustart damit auch im Vollbild. ✕-Button und `FullscreenLayer` entfallen, B4 (Einblendungen) gestrichen, F11 über Tooltips am Umschalter (B3). R5, R9, R10, R11 entfallen, neu R15 (Leiste nimmt Platz) und R16 (`Start New Game` im Vollbild). B2-Umsetzung muss angepasst werden. |
| 0.10 | 2026-11-12 | Johannes Lacher | B1 gestrichen: Absturz per echtem Klick nicht erreichbar (Canvas ohne Hintergrund wird vor dem Spielstart nicht getroffen; per Hit-Test belegt), Änderung zurückgenommen. B6: Abhängigkeit zu B1 entfernt, Hinweis ergänzt, dass Klicks nur auf gezeichnete Flächen ankommen und das Framework bewusst keinen Canvas-Hintergrund setzt. R12 und Anhang A angepasst. |
| 0.9 | 2026-11-12 | Johannes Lacher | Umstrukturiert für die Umsetzung: sechs Blöcke B1–B6 mit Ziel, Änderungen, Randfällen, Tests und Commit-Nachricht; F11 (B3) vor die Einblendungen (B4) gezogen, weil der Hinweistext F11 nennt; Aufgabe A1 wird B1; Randfälle mit Block-Zuordnung; entschiedene Fragen und verworfene Alternativen in Anhang A. Inhaltlich keine Änderung. |
| 0.8 | 2026-11-12 | Johannes Lacher | Entschieden: `e.Handled = true` für an das Spiel weitergegebene Tasten (Schritt 4); Testergebnis zur Fokusverschiebung bei Pfeiltasten und Tab ergänzt. |
| 0.7 | 2026-11-12 | Johannes Lacher | Tastaturfokus: Ursachen beschrieben; Lösung Variante A (Fokus nach Spielstart und bei Klick ins Spiel auf das Canvas, an das Spiel weitergegebene Tasten als behandelt markieren); Alternativen B (`PreviewKeyDown`) und C (Steuerelemente nicht fokussierbar) mit Begründung verworfen. |
| 0.6 | 2026-11-12 | Johannes Lacher | Entschieden: zweiteiliger Umschalter „Normale Ansicht / Vollbild“ (zwei `RadioButton`s im `ToggleButton`-Stil, aktiver Teil hervorgehoben, Zustand nach jedem Wechsel synchron); Null-Prüfung beim Klick ins Canvas als eigene Aufgabe A1. |
| 0.5 | 2026-11-12 | Johannes Lacher | Randfälle-Prüfung: Abschnitt Randfälle (R1–R14), Umsetzung ergänzt (`RestoreBounds`, `_IsFullscreen`, `StateChanged`, `Focusable = False`, Container ohne Hintergrund, Ränder auf 0, Neuzeichnen nur ohne `IPaintGame2`, Hinweis-Timer), Testplan erweitert, F11 vor der Null-Prüfung. |
| 0.4 | 2026-11-12 | Johannes Lacher | Entschieden: „Start New Game“ nur in der normalen Ansicht, kein Neustart-Button im Vollbild. |
| 0.3 | 2026-11-12 | Johannes Lacher | Status im Vollbild als Einblendung oben links, nur sichtbar mit Inhalt (Binding an das Statusfeld, Klicks gehen durch); gemeinsamer Container `FullscreenLayer`; Ecken-Aufteilung und Testfall ergänzt. |
| 0.2 | 2026-11-12 | Johannes Lacher | Esc-Konflikt entschieden: nur F11 (und „✕“) beendet das Vollbild, Esc bleibt beim Spiel; verworfene Alternativen dokumentiert; Hinweis beim Wechsel ins Vollbild ergänzt. |
| 0.1 | 2026-11-12 | Johannes Lacher | Erste Fassung: Vollbild randlos mit nur Canvas, zwei Buttons neben „Start New Game“, „✕“ + Esc/F11 zum Zurückkehren. |
