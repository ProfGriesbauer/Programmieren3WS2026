# Tim Eichert – Prompts

## 2026-10-29

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> ich möchte ein vier gewinnt erstellen. als vorlage dient das tictactoe vom prof griesbauer. es müssen die selben klassen verwendet werden, damit es über die mainwindow. xaml cs gestarted werde kann. Speichere die datein in gruppe 5. Der name der datei soll tim_eichert_4GW sien. Die bekommen anforderungen sind folgende: Projektanforderungen OOPGames – Kurzfassung
> Programmieren 3 · Wintersemester 2026/27 · Josef Griesbauer · ausführliche Fassung:
> Projektanforderungen_OOPGames.pdf
> Alle Gruppen arbeiten am WPF-Framework OOPGames (C#, .NET 6). Jedes Spiel besteht aus den austauschbaren
> Rollen Painter ( IPaintGame), Rules ( IGameRules, IGameField) und Player ( IHumanGamePlayer,
> IComputerGamePlayer). Die Kompatibilität wird über CanBePaintedBy / CanBeRuledBy immer gegen Interfaces
> geprüft – Muster: Beispiel X_Griesbauer.
> Generelle Anforderungen
> Eigene Verzeichnisse: Jede Gruppe arbeitet ausschließlich in Classes/<Gruppe>/ und Interfaces/<Gruppe>/
> (analog X_Griesbauer). Dateien: Vorname_Nachname_<Inhalt>.cs; Klassen/Interfaces mit Gruppenpräfix;
> eindeutige Name-Properties. Außerhalb davon ist nur das Registrieren der eigenen Klassen im Block
> //REGISTER YOUR CLASSES HERE in MainWindow.xaml.cs erlaubt.
> Jedes Teammitglied committet und pusht selbst (eigener GitHub-Account) – die Historie zeigt die
> Aufteilung im Team. Kleine Commits mit aussagekräftigen Messages; vor dem Push git pull und dotnet
> build; kein --force, keine fremden Dateien, kein bin// obj/.
> Prompts dokumentieren: Für jeden Studierenden eine eigene Datei Vorname_Nachname_Prompts.md im
> Hauptverzeichnis der Gruppe ( Classes/<Gruppe>/) mit jedem Prompt im Wortlaut, Datum, Werkzeug/Modell
> und kurzem Ergebnisvermerk.
> Pläne dokumentieren: Alle Pläne (vom Agenten erzeugt oder eigene) als Markdown in
> Classes/<Gruppe>/Plans/. Prompts und Pläne werden zusammen mit dem zugehörigen Code committet.
> Framework-Dateien sind tabu – auch für Agenten. Die Basis-Interfaces in Interfaces/,
> Classes/OOPGames.cs, MainWindow.* (außer Registrierung), App.*, .csproj/ .sln, .vscode/,
> X_Griesbauer und fremde Gruppenordner werden nicht verändert. Ist eine Framework-Erweiterung nötig,
> MUSS sie vorab mit dem Dozenten abgeklärt werden. Der Agent ist entsprechend anzuweisen („Ändere nur
> Dateien in Classes/<Gruppe>/ und Interfaces/<Gruppe>/; ist eine Framework-Änderung nötig, brich ab und
> beschreibe sie.“); vor jedem Commit mit git status/ git diff prüfen.
> Anforderung 1: Vier gewinnt – kompatibel über alle Gruppen
> Jede Gruppe implementiert Painter, Rules (mit Spielfeld), Human-Player und Computer-Player. Jede
> Kombination aus Komponenten aller Gruppen muss im Hauptfenster funktionieren.
> Grundlage ist ein gemeinsamer Satz Vier-gewinnt-Interfaces (z. B. Interfaces/C4_Gemeinsam/: IC4_Field,
> IC4_Move : IColumnMove, IC4_Rules, IC4_Painter, IC4_HumanPlayer, IC4_ComputerPlayer), von allen
> Gruppen gemeinsam erarbeitet (je eine Vertretung), vom Dozenten freigegeben und danach eingefroren. Teil
> der Vereinbarung: Feldgröße, Indexkonventionen, Zeichengeometrie (für die Klick-Umrechnung).
> Programmiert wird nur gegen die gemeinsamen Interfaces – nie gegen konkrete Klassen (auch nicht die
> eigenen).
> Rules weisen ungültige Züge ab und erkennen Sieg (waagerecht, senkrecht, diagonal) und Unentschieden;
> Human-Player liefert bei ungültiger Eingabe null; Computer-Player spielt immer gültig und mindestens
> „gewinnen, wenn möglich – sonst blockieren“.
> Nachweis: Test mit mindestens drei anderen Gruppen, dokumentiert als Kompatibilitätsmatrix; Fehler in
> fremden Komponenten als Issue melden, nicht selbst ändern.

- **Ergebnisvermerk:** Tic-Tac-Toe-kompatible Vier-gewinnt-Komponenten für Gruppe 5 erstellt und im Hauptfenster registriert. Die gemeinsame Basis im Repository enthält derzeit `I4GWFeld` und `I4GWMove`; gemeinsame Painter-/Rules-/Player-Interfaces und eine abgestimmte Klickgeometrie fehlen noch. Der Austausch mit allen Gruppen ist daher erst nach gemeinsamer Festlegung und Kompatibilitätstests belegbar.

## 2026-10-29

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> kannst du noch eine Makierung am ende erstellen, wo die 4 steine in einer reihe sind. Füge hinzu, dass man auch mit den tasten 1 bis 7 die kugeln platzieren kann. Kannst du den Computer verbessern, sodass er besser spielt. Warum ist das ganze nicht mit anderen kompatibel?

- **Ergebnisvermerk:** Die Gewinnsteine werden hervorgehoben, der Computer sucht mehrere Züge voraus und die bereits vorhandene Tastatureingabe für 1–7 wird überprüft. Die Grenzen der gruppenübergreifenden Kompatibilität werden erklärt.

## 2026-10-29

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> füge unter das raster bitte die spalten nummer hinzu und bei computer gegen computer spielen. Mache das so das man einstellen kann wie schnell die steine gesetzt werden, sodass nicht alles auf einmal auftaucht. Prüfe bitte welche kombinationen noch nicht kompatibel sind.

- **Ergebnisvermerk:** Spaltennummern und ein Verzögerungsregler (0–1500 ms) wurden in der Gruppe-5-Spielfläche ergänzt; Computerzüge warten entsprechend der Einstellung. Geprüft: Gruppen 5/8/10 tauschen Felder und Züge zwischen Regeln und Paintern aus, Gruppe 3/FG ist wegen konkreter Klassenprüfungen nur mit den eigenen Rollen kompatibel. Die Mausgeometrien von Gruppe 5 und Gruppen 8/10 weichen voneinander ab; daher sind gruppenübergreifende Klickzüge trotz gemeinsamer Feld-/Zugtypen nicht zuverlässig.
