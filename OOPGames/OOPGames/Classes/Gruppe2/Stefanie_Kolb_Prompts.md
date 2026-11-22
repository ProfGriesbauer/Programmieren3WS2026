# Prompts - Stefanie Kolb

## 19.11.2026

**Werkzeug:** Copilot SDK in VS Code
**Modell:** gpt-6-luna

### Prompt 1 - Wagon-Darstellung auf den Gleisen

> in diesem Bild kannst du sehen, dass die Münzen gut animiert sind. Die Wagons passen noch nicht. bitte passe mir die Darstellung auf den 3 verschiedenen Gleisen an

**Ergebnis:** Wagons werden entsprechend der Perspektive auf den drei Gleisen positioniert.

### Prompt 2 - Zug- und Wagon-Bewegung und 3D-Darstellung

> Die Züge und die Wagon erscheinen zu plötzlich im Spielfeld. Bitte lasse diese Hindernisse vom Horizont bis nach vorne wandern lassen so wie die büsche.
>
> Bitte mache die visualisierung von 2D in 3D über direct3d

**Ergebnis:** Zug- und Wagon-Hindernisse starten weiter entfernt am Horizont und wandern über die Distanz nach vorne. Nach Klärung der technischen Umsetzung wurde WPF Viewport3D gewählt; die Szene wird innerhalb des vorhandenen Canvas-Painters in 3D dargestellt.

### Prompt 3 - Unterschied der Renderer

> was ist der unterschied von diesen zwei?

**Ergebnis:** WPF Viewport3D und native Direct3D 11 wurden hinsichtlich Framework-Integration, Aufwand und Kontrolle verglichen.

### Prompt 4 - Projektvorgaben und Empfehlung

> Generelle Anforderungen
> Eigene Verzeichnisse: Jede Gruppe arbeitet ausschließlich in
> (analog
> Classes/<Gruppe>/ und
> X_Griesbauer ). Dateien:
> Interfaces/<Gruppe>/
> Vorname_Nachname_<Inhalt>.cs ; Klassen/Interfaces mit Gruppenpräfix;
> eindeutige
> Name-Properties. Außerhalb davon ist nur das Registrieren der eigenen Klassen im Block
> //REGISTER YOUR CLASSES HERE in
> MainWindow.xaml.cs erlaubt.
> Jedes Teammitglied committet und pusht selbst (eigener GitHub-Account) – die Historie zeigt die
> Aufteilung im Team. Kleine Commits mit aussagekräftigen Messages; vor dem Push
> build ; kein --force , keine fremden Dateien, kein
> bin/ /
> git pull und
> obj/ .
> dotnet
> Prompts dokumentieren: Für jeden Studierenden eine eigene Datei
> Vorname_Nachname_Prompts.md im
> Hauptverzeichnis der Gruppe (
> Classes/<Gruppe>/ ) mit jedem Prompt im Wortlaut, Datum, Werkzeug/Modell
> und kurzem Ergebnisvermerk.
> Pläne dokumentieren: Alle Pläne (vom Agenten erzeugt oder eigene) als Markdown in
> Classes/<Gruppe>/Plans/ . Prompts und Pläne werden zusammen mit dem zugehörigen Code committet.
> Framework-Dateien sind tabu – auch für Agenten. Die Basis-Interfaces in
> Interfaces/ ,
> Classes/OOPGames.cs ,
> MainWindow.* (außer Registrierung),
> App.* ,
> .csproj /
> .sln ,
> .vscode/ ,
> X_Griesbauer und fremde Gruppenordner werden nicht verändert. Ist eine Framework-Erweiterung nötig,
> MUSS sie vorab mit dem Dozenten abgeklärt werden. Der Agent ist entsprechend anzuweisen („Ändere nur
> Dateien in Classes/<Gruppe>/ und Interfaces/<Gruppe>/; ist eine Framework-Änderung nötig, brich ab und
> beschreibe sie.“); vor jedem Commit mit
> git status /
> git diff prüfen
>
> Anforderung 2: Eigenes komplexes Spiel
> Jede Gruppe entwickelt mit Hilfe von Agenten ein eigenes, komplexes bzw. innovatives Spiel; Kompatibilität
> mit anderen Gruppen ist nicht nötig. Erwartet wird deutlich mehr als ein einfacher Brettspielklon (z. B. Echtzeit
> über
> IGameRules2 /
> IPaintGame2 , starker KI-Gegner, Levels/Phasen).
> Spielkonzept (eine Seite in
> Plans/ ) vor Beginn dem Dozenten vorstellen.
> Harte Vorgabe: Das Spiel läuft vollständig im vorgegebenen Framework – Auswahl und Start über das
> Hauptfenster, Eingaben nur über Klick-/Tasten-Selections, Zeichnen nur auf das übergebene Canvas, kein
> eigenes Fenster oder eigene Hauptschleife. Framework-Erweiterungen nur nach Absprache (siehe oben).
> Sauberer OO-Entwurf; das Team muss den gesamten Code erklären können, auch den agentisch erzeugten.
> Teamarbeit nach SCRUM, dokumentiert in
> Classes/<Gruppe>/Scrum/ (oder GitHub Issues/Projects): Product
> Owner und Scrum Master benannt, Product Backlog mit User Stories, Sprints (1–2 Wochen) mit Sprintziel und
> Verantwortlichen je Aufgabe, Kurzprotokolle der Abstimmungen, Review und Retrospektive je Sprint. Backlog,
> Prompt-Dateien, Pläne und Commits müssen zusammenpassen; jedes Mitglied hat einen substanziellen,
> sichtbaren Anteil
>
> Bitte halte dich an diese Vorgaben und empfehle mir welche 3d ansicht besser wäre

**Ergebnis:** WPF Viewport3D empfohlen, weil die 3D-Szene im bestehenden Canvas-Painter bleibt und keine Framework- oder Projektdateien geändert werden müssen.

### Prompt 5 - Auswahl der 3D-Umsetzung

> WPF Viewport3D (empfohlen, passend zum Framework)

**Ergebnis:** Umsetzung der 3D-Spielszene mit WPF Viewport3D bestätigt.

## 22.11.2026

**Werkzeug:** Copilot SDK in VS Code
**Modell:** gpt-6-luna

### Prompt 6 - Spielkonzept aktualisieren und alten Ordner entfernen

> kannst du bitte das Spielkonzept nach dem aktuellen Stand von Tain Run bitte anpassen und das in den Gruppe2 Ordner bei Gruppe2_TrainRun abspeichern und die alte Datei mit dem alten Ordner löschen

**Ergebnis:** Das aktuelle Spielkonzept wurde auf Grundlage des Train-Run-Standes in `Classes/Gruppe2/Train Run/Spielkonzept.md` erstellt. Die veraltete Datei und der leere Ordner `Classes/Gruppe_2_Train_Run` wurden entfernt.

### Prompt 7 - Leeren Subway-Surfer-Ordner löschen

> kannst du bitte den leeren Ordner Subway Surfer in Gruppe2 löschen

**Ergebnis:** Der leere Ordner `Classes/Gruppe2/Subway Surfer` wurde entfernt.

### Prompt 8 - Leeren Gruppe_2-Ordner löschen

> kannst du mir bitte den Ordner Gruppe_2 löschen, in dem .gitkeep gespeichert ist, da dieser Ordner leer ist

**Ergebnis:** Der leere Ordner `Classes/Gruppe_2` einschließlich `.gitkeep` wurde entfernt.

### Prompt 9 - Leeren Train-Run-Plans-Ordner löschen

> kannst du mir bitte den Ordner Gruppe_2_Train_Run\Plans auch löschen, da dieser auch leer ist

**Ergebnis:** Der leere Unterordner `Classes/Gruppe_2_Train_Run/Plans` wurde entfernt.

### Prompt 10 - Verbleibenden Train-Run-Duplikatordner löschen

> kannst du mir bitte den leeren Ordner Gruppe_2_Train_Run auch löschen

**Ergebnis:** Der leere restliche Ordner `Classes/Gruppe_2_Train_Run` wurde entfernt.

### Prompt 11 - Veraltete Gruppe2-Plans-Dateien löschen

> kannst du bitte den unterordner Plans bei Gruppe2 mit den beiden Dateien Stefanie_Kolb_... löschen

**Ergebnis:** Der Unterordner `Classes/Gruppe2/Plans` mit den beiden Stefanie-Kolb-Train-Run-Plänen wurde gelöscht.

### Prompt 12 - Prompts dokumentieren

> Projektanforderungen OOPGames – Kurzfassung
> Programmieren 3 · Wintersemester 2026/27 · Josef Griesbauer · ausführliche Fassung:
> Projektanforderungen_OOPGames.pdf
> Alle Gruppen arbeiten am WPF-Framework
> OOPGames (C#, .NET 6). Jedes Spiel besteht aus den austauschbaren
> IPaintGame ), Rules (
> IGameRules , 
> IGameField ) und Player (
> IComputerGamePlayer ). Die Kompatibilität wird über 
> IHumanGamePlayer ,
> CanBePaintedBy / 
> CanBeRuledBy immer gegen Interfaces
> geprüft – Muster: Beispiel 
> X_Griesbauer .
> Generelle Anforderungen
> Eigene Verzeichnisse: Jede Gruppe arbeitet ausschließlich in 
> (analog 
> Classes/<Gruppe>/ und 
> X_Griesbauer ). Dateien: 
> Interfaces/<Gruppe>/
> Vorname_Nachname_<Inhalt>.cs ; Klassen/Interfaces mit Gruppenpräfix;
> eindeutige 
> Name-Properties. Außerhalb davon ist nur das Registrieren der eigenen Klassen im Block
> //REGISTER YOUR CLASSES HERE in 
> MainWindow.xaml.cs erlaubt.
> Jedes Teammitglied committet und pusht selbst (eigener GitHub-Account) – die Historie zeigt die
> Aufteilung im Team. Kleine Commits mit aussagekräftigen Messages; vor dem Push 
> build ; kein --force , keine fremden Dateien, kein 
> bin/ /
> git pull und 
> obj/ .
> dotnet
> Prompts dokumentieren: Für jeden Studierenden eine eigene Datei 
> Vorname_Nachname_Prompts.md im
> Hauptverzeichnis der Gruppe (
> Classes/<Gruppe>/ ) mit jedem Prompt im Wortlaut, Datum, Werkzeug/Modell
> und kurzem Ergebnisvermerk.
> Pläne dokumentieren: Alle Pläne (vom Agenten erzeugt oder eigene) als Markdown in
> Classes/<Gruppe>/Plans/ . Prompts und Pläne werden zusammen mit dem zugehörigen Code committet.
> Framework-Dateien sind tabu – auch für Agenten. Die Basis-Interfaces in 
> Interfaces/ ,
> Classes/OOPGames.cs , 
> MainWindow.* (außer Registrierung), 
> App.* , 
> .csproj /
> .sln , 
> .vscode/ ,
> X_Griesbauer und fremde Gruppenordner werden nicht verändert. Ist eine Framework-Erweiterung nötig,
> MUSS sie vorab mit dem Dozenten abgeklärt werden. Der Agent ist entsprechend anzuweisen („Ändere nur
> Dateien in Classes/<Gruppe>/ und Interfaces/<Gruppe>/; ist eine Framework-Änderung nötig, brich ab und
> beschreibe sie.“); vor jedem Commit mit 
> git status / 
> git diff prüfen
>
> Jede Gruppe entwickelt mit Hilfe von Agenten ein eigenes, komplexes bzw. innovatives Spiel; Kompatibilität
> mit anderen Gruppen ist nicht nötig. Erwartet wird deutlich mehr als ein einfacher Brettspielklon (z. B. Echtzeit
> über 
> IGameRules2 /
> IPaintGame2 , starker KI-Gegner, Levels/Phasen).
> Spielkonzept (eine Seite in 
> Plans/ ) vor Beginn dem Dozenten vorstellen.
> Harte Vorgabe: Das Spiel läuft vollständig im vorgegebenen Framework – Auswahl und Start über das
> Hauptfenster, Eingaben nur über Klick-/Tasten-Selections, Zeichnen nur auf das übergebene Canvas, kein
> eigenes Fenster oder eigene Hauptschleife. Framework-Erweiterungen nur nach Absprache (siehe oben).
> Sauberer OO-Entwurf; das Team muss den gesamten Code erklären können, auch den agentisch erzeugten.
>
> bitte erweitere mir meine Promts datei mit den heutigen erstellten Prompts

**Ergebnis:** Diese Prompts-Datei wurde um die heutigen Prompts vom 22.11.2026, die verwendeten Werkzeuge und kurze Ergebnisvermerke ergänzt.
