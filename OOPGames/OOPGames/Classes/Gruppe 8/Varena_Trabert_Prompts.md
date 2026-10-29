# Varena Trabert – Prompt-Protokoll

Werkzeug: Copilot SDK in VS Code
Modell: im Chat nicht angezeigt

## 2026-10-29

### Prompt 1

```text
Ich muss ein 4 Gewinnt machen. Das sind die Anforderungen dafür. Gleib immer im Framework brich da nicht aus, Sonst brich ab Generelle Anforderungen
Eigene Verzeichnisse: Jede Gruppe arbeitet ausschließlich in Classes/<Gruppe>/ und Interfaces/<Gruppe>/
(analog X_Griesbauer ). Dateien: Vorname_Nachname_<Inhalt>.cs ; Klassen/Interfaces mit Gruppenpräfix;
eindeutige Name -Properties. Außerhalb davon ist nur das Registrieren der eigenen Klassen im Block
//REGISTER YOUR CLASSES HERE in MainWindow.xaml.cs erlaubt.
Jedes Teammitglied committet und pusht selbst (eigener GitHub-Account) – die Historie zeigt die
Aufteilung im Team. Kleine Commits mit aussagekräftigen Messages; vor dem Push git pull und dotnet
build ; kein --force , keine fremden Dateien, kein bin/ / obj/ .
Prompts dokumentieren: Für jeden Studierenden eine eigene Datei Vorname_Nachname_Prompts.md im
Hauptverzeichnis der Gruppe ( Classes/<Gruppe>/ ) mit jedem Prompt im Wortlaut, Datum, Werkzeug/Modell
und kurzem Ergebnisvermerk.
Pläne dokumentieren: Alle Pläne (vom Agenten erzeugt oder eigene) als Markdown in
Classes/<Gruppe>/Plans/ . Prompts und Pläne werden zusammen mit dem zugehörigen Code committet.
Framework-Dateien sind tabu – auch für Agenten. Die Basis-Interfaces in Interfaces/ ,
Classes/OOPGames.cs , MainWindow.* (außer Registrierung), App.* , .csproj / .sln , .vscode/ ,
X_Griesbauer und fremde Gruppenordner werden nicht verändert. Ist eine Framework-Erweiterung nötig,
MUSS sie vorab mit dem Dozenten abgeklärt werden. Der Agent ist entsprechend anzuweisen („Ändere nur
Dateien in Classes/<Gruppe>/ und Interfaces/<Gruppe>/; ist eine Framework-Änderung nötig, brich ab und
beschreibe sie.“); vor jedem Commit mit git status / git diff prüfen.
Anforderung 1: Vier gewinnt – kompatibel über alle Gruppen
Jede Gruppe implementiert Painter, Rules (mit Spielfeld), Human-Player und Computer-Player. Jede
Kombination aus Komponenten aller Gruppen muss im Hauptfenster funktionieren.
Grundlage ist ein gemeinsamer Satz Vier-gewinnt-Interfaces (z. B. Interfaces/C4_Gemeinsam/ : IC4_Field ,
IC4_Move : IColumnMove , IC4_Rules , IC4_Painter , IC4_HumanPlayer , IC4_ComputerPlayer ), von allen
Gruppen gemeinsam erarbeitet (je eine Vertretung), vom Dozenten freigegeben und danach eingefroren. Teil
der Vereinbarung: Feldgröße, Indexkonventionen, Zeichengeometrie (für die Klick-Umrechnung).
Programmiert wird nur gegen die gemeinsamen Interfaces – nie gegen konkrete Klassen (auch nicht die
eigenen).
Rules weisen ungültige Züge ab und erkennen Sieg (waagerecht, senkrecht, diagonal) und Unentschieden;
Human-Player liefert bei ungültiger Eingabe null ; Computer-Player spielt immer gültig und mindestens
„gewinnen, wenn möglich – sonst blockieren“.
Nachweis: Test mit mindestens drei anderen Gruppen, dokumentiert als Kompatibilitätsmatrix; Fehler in
fremden Komponenten als Issue melden, nicht selbst ändern.
```

Ergebnis: Rahmenbedingungen und Projektstruktur geprüft. Zu diesem Zeitpunkt
waren die C4-Verträge noch nicht im lokalen Projekt vorhanden; keine
Framework-Datei geändert.

### Prompt 2

```text
mach weiter. Du musst alle meine Prompts speichern so das ich diese an einem Ort gespeicht habe. z.B in einem Dokument
```

Ergebnis: Name erfragt und Prompt-Protokoll angelegt.

### Prompt 3

```text
Varena Trabert
```

Ergebnis: Name für die Prompt-Datei erhalten.

### Prompt 4

```text
Interfaces/C4_Gemeinsam/ : IC4_Field ,
IC4_Move : IColumnMove , IC4_Rules , IC4_Painter , IC4_HumanPlayer , IC4_ComputerPlayer
```

Ergebnis: Der angegebene Pfad wurde geprüft; die Schnittstellen lagen damals
nicht vor.

### Prompt 5

```text
Die vorgegebenen Interfaces ligen unter Interfaces ab nutze sie
```

Ergebnis: Das Interface-Verzeichnis wurde geprüft. Die gemeinsamen
`I4GWFeld`- und `I4GWMove`-Verträge wurden später im Projekt gefunden.

### Prompt 6

```text
Nutze diese Klassen wie für das Tick Tack Toe und mache daraus ein 4 Gewinnt
```

Ergebnis: Das vorhandene Tic-Tac-Toe-Muster und die aktuelle Gruppen-10-
Vier-gewinnt-Implementierung wurden zur Orientierung geprüft.

### Prompt 7

```text
Mach einen neuen Ordner für mich in Classes und Interfaces mit dem Namen Gruppe 8
```

Ergebnis: `Classes/Gruppe 8` und `Interfaces/Gruppe 8` wurden angelegt.

### Prompt 8

```text
pushe das
```

Ergebnis: Die Ordner wurden in Commit `8a5dc2b` gepusht.

### Prompt 9

```text
Erstelle mir in Classes im Ordner Gruppe 8 ein eigenes 4 Gewinnt nach den Anforderungen. Verändere das Framework nicht. benutze die Interfaces. 6 Zeilen und 7 Spalten solles haben. Die steine sollen blau und Pink sein und das Spielfeld schwarz
```

Ergebnis: Spielklassen, Rules, Painter, Human- und Computer-Player in
`Classes/Gruppe 8` erstellt und im erlaubten MainWindow-Registrierungsblock
registriert. Verwendet werden `I4GWFeld`, `I4GWMove` und die Framework-
Interfaces. Der Build lief erfolgreich; Framework-Dateien wurden nicht
geändert.
