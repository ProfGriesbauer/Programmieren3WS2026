# Prompts – Johannes Lacher (Gruppe 10)

Werkzeug: Claude Code (VS-Code-Erweiterung), Modell: Claude Opus 5.5 (`claude-opus-5-5`).

---

## 2026-10-22 – Claude Code (Modell: Claude Opus 5.5)

### 1
**Prompt:**
> Erkläre das Framework OOP games

**Ergebnis:** übernommen – Erklärung von Rollen (Painter, Rules, Player, Field, Move), `OOPGamesManager`, Spielablauf in `MainWindow` und dem TicTacToe-Beispiel; keine Codeänderung.

### 2
**Prompt:**
> erstelle grundlegendes 4 gewinnt

**Ergebnis:** übernommen - „Vier gewinnt“ mit Feld, Rules, Painter, Human- und Computer-Player (gewinnen, blockieren, Mitte bevorzugen) erstellt; später nach `10_Gruppe` verschoben und an die gemeinsamen Interfaces angepasst (Prompt 22).

### 3
**Prompt:**
> how does the animation work?

**Ergebnis:** übernommen – Erklärung der Animation (Zug sofort im Feld, Warteschlange fallender Steine, Physik pro Tick, Zeichenreihenfolge mit EvenOdd-Löchern); keine Codeänderung.

---

## 2026-10-29 – Claude Code (Modell: Claude Opus 5.5)

### 9
**Prompt:**
> So only IVierGewinnt.cs is the shared interface all other groups would have to use?

**Ergebnis:** übernommen – Erklärung, was zum gemeinsamen Vertrag gehört und welche Konventionen nur in Kommentaren stehen; keine Codeänderung.

### 10
**Prompt:**
> okay simplify it

**Ergebnis:** verworfen – ungenutzte Member aus dem Entwurf entfernt; mit Prompt 13 nach den Projektanforderungen wieder ergänzt.

### 11
**Prompt:**
> pull the current state and read the projektanforderungen recently added. then create a memory or skill file with the summary to remember it across sessions

**Ergebnis:** übernommen – Pull durchgeführt, Projektanforderungen gelesen und als Memory-Dateien für Claude Code zusammengefasst (außerhalb des Repos); Abweichungen des Entwurfs von den Anforderungen aufgelistet.

### 12
**Prompt:**
> rename my draft to IC4_

**Ergebnis:** verworfen – Präfix `IVG_` → `IC4_`, Ordner `Interfaces/C4_Gemeinsam/`; Entwurf später entfernt (Prompt 22).

### 13
**Prompt:**
> align the draft to the project rules

**Ergebnis:** verworfen – Entwurf um `IC4_Rules`/`IC4_Painter`/Player-Interfaces und Konventionen ergänzt; Entwurf später entfernt (Prompt 22). Die Prüfung auf den falschen Spieler in den Rules wurde übernommen.

### 14
**Prompt:**
> whats the advantage of using namespace instead of a public interface?

**Ergebnis:** übernommen – Erklärung Namespace vs. Zugriffsmodifizierer und Namespaces vs. Präfixe; keine Codeänderung.

### 15
**Prompt:**
> adjust the shared interface to use the prefixes

**Ergebnis:** verworfen – keine Änderung nötig, alle Interfaces hatten bereits das Präfix.

### 16
**Prompt:**
> but why is this line needed then
> "namespace OOPGames"

**Ergebnis:** übernommen – Erklärung, warum die Interfaces im gemeinsamen Namespace liegen; keine Codeänderung.

### 17
**Prompt:**
> what is this line doing?
>         int this[int r, int c] { get; }

**Ergebnis:** übernommen – Erklärung des Indexers; keine Codeänderung.

### 18
**Prompt:**
> why is this empty
>     public interface IC4_Move : IColumnMove
>     {
>     }

**Ergebnis:** übernommen – Erklärung von Marker-Interfaces und geerbten Membern; keine Codeänderung.

### 19
**Prompt:**
> what would be the clearer more easy to read approach for the indexer

**Ergebnis:** übernommen – Vorschläge: Parameternamen `row`/`column`, XML-Doc-Kommentar, benannte Konstanten für Zellwerte; Umsetzung in Prompt 20.

### 20
**Prompt:**
> add your three suggested changes

**Ergebnis:** verworfen – Vorschläge im Entwurf umgesetzt; Entwurf später entfernt (Prompt 22).

### 21
**Prompt:**
> do we need an equivalent for a "Spielzug" interface?

**Ergebnis:** übernommen – Erklärung, dass das Zug-Interface bereits der Spielzug ist und jede Gruppe ihre eigene Zug-Klasse schreibt; keine Codeänderung.

### 22
**Prompt:**
> Pull the current changes. the interface was defined. remove my draft interface and move the current implementation to a group folder named "10_Gruppe". note to save to memory: the second project will be developed on our own branch. the first one all on the same branch

**Ergebnis:** übernommen – Pull durchgeführt, eigener Entwurf und Testgruppe entfernt, Implementierung nach `Classes/10_Gruppe/` und `Interfaces/10_Gruppe/` verschoben und an `I4GWFeld`/`I4GWMove` angepasst (Präfix `G10_`/`IG10_`, da C#-Namen nicht mit einer Ziffer beginnen dürfen); Branch-Strategie als Memory gespeichert.

### 23
**Prompt:**
> why is obj/…AssemblyReference.cache tracked?

**Ergebnis:** übernommen – Erklärung: Datei wurde trotz `.gitignore` committet, `.gitignore` wirkt nur auf nicht getrackte Dateien; keine Codeänderung.

### 24
**Prompt:**
> who commited it? can the same happen to me? or does the .gitignore block it from now on?

**Ergebnis:** übernommen – Erklärung, wer die Datei committet hat und wie man das selbst vermeidet (gezielt stagen, vor jedem Commit `git status` prüfen); keine Codeänderung.

### 25
**Prompt:**
> remove the Johannes_Lacher_prefix as it will be developed by our group

**Ergebnis:** verworfen – Dateien ohne Namenspräfix umbenannt; widerspricht der Namenskonvention (Abschnitt 2.1), mit Prompt 26 rückgängig gemacht.

### 26
**Prompt:**
> revert the rename when its against the projects conventions

**Ergebnis:** übernommen – Dateinamen `Johannes_Lacher_…` wiederhergestellt.

### 27
**Prompt:**
> can we detect an invalid player-field combination? e.g. tic tac toe player with 4-gewinnt-rules

**Ergebnis:** übernommen – Analyse: `MainWindow` ruft `CanBeRuledBy` nie auf, ein unpassender Computer-Player friert das Fenster ein; Vorschlag für eine Prüfung auf unserer Seite (Umsetzung in Prompt 30).

### 28
**Prompt:**
> review and test the recent changes

**Ergebnis:** übernommen – Code-Review und Testprogramm (außerhalb des Repos); zwei Fehler gefunden und behoben: Züge nach einem Sieg wurden angenommen, und eine abweichende Zeilenkonvention anderer Gruppen ließ das Spiel einfrieren. Anzeige von Sieg/Unentschieden erst nach der Animation.

### 29
**Prompt:**
> pull the current changes and apply my commit on top

**Ergebnis:** übernommen – Commit „Add Vier Gewinnt (Gruppe 10)“ per Rebase auf den aktuellen Stand gesetzt, Build geprüft.

### 30
**Prompt:**
> add a check on our side to catch invalid players/rules and fields with functions like CanBeRuledBy

**Ergebnis:** übernommen – `G10_VierGewinntCompatibility` prüft die gewählten Player mit `CanBeRuledBy`; die Rules sperren bei unpassenden Playern alle Züge, der Painter zeigt den Grund an (Commit „Add compatibility checks for player and rules in Vier Gewinnt“).

### 31
**Prompt:**
> pull the current changes

**Ergebnis:** übernommen – Pull durchgeführt, Build geprüft.

### 32
**Prompt:**
> create a readme like group 5

**Ergebnis:** nachgebessert – `README.md` nach dem Muster von Gruppe 5 erstellt; Namen der Gruppenmitglieder selbst ergänzt (Commit „Create README.md (Gruppe 10)“).

### 33
**Prompt:**
> the newly added rules and players are not registered in the mainwindow.xaml
> add them

**Ergebnis:** verworfen – Klassen von Gruppe 3 lokal registriert; Gruppe 3 hat ihre Klassen danach selbst registriert, die lokale Änderung wurde nicht committet.

### 34
**Prompt:**
> scale our field to the window size

**Ergebnis:** übernommen – Brett skaliert mit der Canvas-Größe, Klicks werden mit demselben Layout in Spalten umgerechnet (Commit „Allow field to scale dynamically to window size“).

### 35
**Prompt:**
> whats going on in the git history? can i push my changes (allow field to scale dynmaically)?

**Ergebnis:** übernommen – Erklärung der parallelen Pushes und Merge-Commits; Vorgehen `git pull --rebase`, Build, `git push`.

### 36
**Prompt:**
> rename our players, fields and so on to follow gruppe 8s naming scheme (no last name just group number)

**Ergebnis:** übernommen – `Name`-Texte auf „Gruppe 10 Vier Gewinnt …“ geändert (Commit „Update player and game names (Gruppe 10)“).