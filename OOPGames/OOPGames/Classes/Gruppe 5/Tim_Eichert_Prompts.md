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

## 2026-11-05

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> erstelle für den gesamten code kommentare die erklären was in dem teil gerade passiert

- **Ergebnisvermerk:** Die Spiel- und Suchlogik sowie Regeln, Darstellung, Eingabe und Computer-Spieler wurden mit erklärenden deutschen Kommentaren versehen.

## 2026-11-05

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> ich habe das problem, wenn ich gegen den computer spiele, liest der glaube ich so schnell ein, dass wenn ich eine taste drücke macht er 2 kugeln da rein. löse das problem

- **Ergebnisvermerk:** Wiederholte Tastaturereignisse beim Gedrückthalten einer Zahlentaste werden jetzt bis zum Loslassen ignoriert, sodass ein Tasten-Repeat nicht als weiterer Zug zählt.

## 2026-11-05

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> ich habe immernoch das problenm, wenn ich eine taste kurz drücke, dass er einen zug macht, dann die ki einen zug macht und ich dann automatisch noch einen zug an der selben stelle mache.

- **Ergebnisvermerk:** Zusätzlich zur Spielereingabe fängt das WPF-Preview-Keyboard-Event automatische Tastenwiederholungen vor dem `MainWindow`-Handler ab. Dadurch kann ein nach dem Computerzug eintreffendes Repeat-Ereignis keinen zweiten Spielzug mehr auslösen.

## Projekt: 2D-Fabrikspiel (Satisfactory-inspiriert)

Für neue Prompts zu diesem Spiel werden Datum, lokale Uhrzeit (Europe/Berlin), Wortlaut, Werkzeug/Modell und Ergebnisvermerk ergänzt.

### 2026-11-12, 08:52 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> Ich möchte in der Uni für unser programmieren 3 Projekt im 3 semster Mechatronik bachelor studium. Wir wollen in einer vorgegebenen entwicklungsumgebung ein spiel programmieren. Dabei haben wir uns für eine abgespeckte version von satisfactory in 2D entschieden. Ich habe gehört, dass wäre dann wie factorio das spiel kenne ich selber aber nicht. Ich bin zuständig für das spielfeld. Kannst du mir bitte ein Konzept für den aufbau des programms erstellen. Also welche Klassen ich brauche und was da rein soll. Im anhang sind die anforderungen an das orojekt.Ich habe schonmal angefangen die anforderungen und ideen in der anforderungen.md erstellt

- **Ergebnisvermerk:** Ein Spielfeldkonzept mit Klassenaufteilung, Verantwortlichkeiten, Framework-Anbindung und schrittweisem MVP wurde in `Plans/Tim_Eichert_Spielfeldkonzept.md` erstellt.

### 2026-11-12, 09:10 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> bitte fügen meine prompts in der TIm Eichert prompts md hinzu. Mache für dieses spiel einen seperaten bereich in der datei. Erstelle mir bitte ebenfalls ein UML diagramm um die struktur besser zu verstehen

- **Ergebnisvermerk:** Die Prompts zum Fabrikspiel wurden in diesem separaten Abschnitt dokumentiert. Ein UML-Klassendiagramm wurde in `Plans/Tim_Eichert_Spielfeld_UML.md` ergänzt.

### 2026-11-12, 09:34 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> ich glaube die interfaces machen aktuell wenig sinn so wie du die erstellen möchtest, weil das interface einfach die komplette klasse verwendet. ICh dachte ein interface soll nur das notwendige lesend zeigen. Warum ist außerdem so viel öffentlich? kann man nicht mehr private machen und mit get funktionen arbeiten

- **Ergebnisvermerk:** Das Konzept und UML wurden auf schmale, lesende Interfaces, private Spielfeldzustände, kontrollierte interne Änderungen und reduzierte Objektansichten umgestellt.

### 2026-11-12, 09:38 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> bitte trage jeden prompt immer automatisch mit dem ergebnis in die markdown ein. kannst du bitte auch die uhrzeit hnzufügen. und den letzten prompt nachtragen

- **Ergebnisvermerk:** Die Spiel-Prompt-Einträge erhielten Uhrzeiten; der vorherige Prompt zur Überarbeitung der Interfaces wurde nachgetragen. Ab jetzt wird bei weiteren Aufgaben zu diesem Spiel der Prompt mit Uhrzeit und Ergebnis in diesem Abschnitt dokumentiert.

### 2026-11-12, 09:55 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> ich verstehe nicht ganz wofür g5kachelansicht benötigt wird, das wirkt irgendwie überflüssig. Kann es sein, dass de spieler nicht an die kartenansicht ran kommt? Ist can be painted by möglicherweise falsch herum beschrieben. Kann es sein dass noch ein IG5 welt painter fehlt?

- **Ergebnisvermerk:** Die zusätzliche Kachelansicht wurde durch das schlanke `IG5_KachelInfo` ersetzt. Der fehlende Zugriff des Spielers auf die Kartenumrechnung wurde berücksichtigt; `CanBePaintedBy` und seine Aufrufrichtung wurden am Framework verifiziert; `IG5_WeltPainter` wurde im Konzept und UML ergänzt.

### 2026-11-12, 10:19 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> biite pull den aktuellen stand, füge meine änderungen in spielfeldkonzept prompts und das uml hi nzu und pushe das bitte

- **Ergebnisvermerk:** Der aktuelle Remote-Stand wurde per Fast-Forward eingezogen. Die Anforderungen zu Ressourcen, Sichtweite und getrennten Gebäude-/Laufband-/Stromnetzebenen wurden in Spielfeldkonzept und UML übernommen; der Prompt ist in diesem Dokument protokolliert.

### 2026-11-12, 10:31 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> Setze das konzept nun um und sag bescheid was dir fehlt.

- **Ergebnisvermerk:** Spielfeld- und Kachel-Interfaces, Kacheldatenmodell, Raster sowie Kartenkoordinaten-/Zoomlogik wurden umgesetzt. Für Zoom und Verschieben gibt es zusätzlich `IG5_KartenansichtSteuerung`, getrennt vom lesenden Spielfeldinterface. Konzept und UML wurden auf den tatsächlichen Code abgeglichen; der OOPGames-Build lief mit 0 Warnungen und 0 Fehlern. Noch offen sind die mit Chris abzustimmenden Objekt-/Laufbandverträge, Rules und Sicht-Radius, konkrete Painter-/Eingabeverdrahtung sowie Kartenlayout/Spawnpunkt.

### 2026-11-12, 10:47 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> pushe miene datein

- **Ergebnisvermerk:** Die Spielfeldimplementierung und die dazugehörigen Konzept-/UML-Dateien wurden geprüft und auf den Remote-Branch gepusht. Nicht zugehörige generierte `obj`-Dateien blieben außen vor. `MainWindow.xaml.cs` war unverändert und wurde nicht mitgepusht.

### 2026-11-12, 10:53 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> erstelle mir ein konzept für das gesamte spiel

- **Ergebnisvermerk:** Ein übergeordnetes Spielkonzept mit OOPGames-Anbindung, Klassenverantwortlichkeiten, Datenfluss, Simulationstick, MVP-Stufen, Teamzuständigkeiten und offenen Entscheidungen wurde in `Plans/Tim_Eichert_Gesamtkonzept.md` erstellt. Das bestehende Spielfeldkonzept verweist auf das Gesamtkonzept.

### 2026-11-12, 11:08 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> zunächst würde ich mit einem workaround arbeiten. erstelle nun ein erstes einfaches spiel dabei sein soll:Karte, Gebäude per Klick platzieren (Förderer, Schmelze), automatische Produktion, ein einfaches Lager oder HUB als Ziel.

- **Ergebnisvermerk:** Ein fester Karten-/Fabrikprototyp mit Förderer- und Schmelzenplatzierung, automatischer Erz- und Barrenproduktion sowie HUB-Transfer wurde implementiert. Die Produktion wurde mit einem gezielten Laufzeittest verifiziert; Build erfolgreich. `MainWindow.xaml.cs` bleibt unverändert, daher sind die Rollen noch nicht über die Framework-Auswahllisten startbar.

### 2026-11-12, 11:21 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> bedenke es darf nichts an der main window.xaml.cs geändert werden

- **Ergebnisvermerk:** Alle Fabrikspiel-Registrierungseinträge wurden aus `MainWindow.xaml.cs` entfernt; die Datei ist unverändert. Konzept und UML halten fest, dass der Spielstart über die bestehende Auswahloberfläche dadurch noch nicht möglich ist und ein erlaubter Integrationsweg fehlt.

### 2026-11-12, 11:27 Uhr (Europe/Berlin, UTC+01:00)

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> achso klarden painter die rules und players dürfen hinzugefügt werden

- **Ergebnisvermerk:** `G5_FabrikPainter`, `G5_FabrikRules` und `G5_FabrikHumanPlayer` wurden ausschließlich im bestehenden Registrierungsblock von `MainWindow.xaml.cs` eingetragen. Konzept und UML wurden entsprechend korrigiert.
