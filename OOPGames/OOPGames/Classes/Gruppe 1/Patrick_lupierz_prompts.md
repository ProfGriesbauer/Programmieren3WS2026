# Patrick Lupierz – Prompt-Protokoll

Werkzeug: Copilot SDK in VS Code
Modell: im Chat nicht angezeigt

## 29-10-2026

### Prompt 1
Patrick – Arbeitsanweisungen Vier Gewinnt (Gruppe 1)

1. Allgemeine Projektregeln und Architektur

Werkzeug: Copilot SDK in VS Code

Du arbeitest ausschließlich an meinem Vier-Gewinnt-Projekt in Gruppe 1.

Halte dich strikt an die vorhandene Projektarchitektur und das vorgegebene Framework. Weiche nicht eigenständig davon ab.

Verzeichnisstruktur

Ausschließlich folgende Verzeichnisse verwenden:

Classes/Gruppe 1/

Interfaces/Gruppe 1/

Die vorhandenen gemeinsamen Interfaces und Framework-Schnittstellen unter Interfaces/ müssen verwendet werden.

Erstelle keine eigenen alternativen Schnittstellen, wenn bereits passende vorgegebene Interfaces existieren.

Dateibenennung und Klassenaufbau

Dateien müssen nach dem Schema Patrick_<Inhalt>.cs benannt werden.

Alle eigenen Klassen und Interfaces müssen ein eindeutiges Gruppenpräfix besitzen.

Verwende eindeutige Namen und Properties.

Halte dich an den vorhandenen objektorientierten Klassenaufbau.

Orientiere dich an den bereits vorhandenen funktionierenden Beispielen im Projekt, insbesondere an der bestehenden Tic-Tac-Toe-Implementierung.

Programmiere gegen die vorgegebenen Interfaces und nicht gegen konkrete Implementierungen anderer Gruppen.

Verwende die vorhandenen Vererbungs-, Interface- und Komponentenstrukturen.

Framework-Regeln

Framework-Dateien sind tabu.

Nicht verändern:

Basis-Interfaces unter Interfaces/.

Classes/OOPGames.cs

MainWindow.*, mit Ausnahme des ausdrücklich erlaubten Registrierungsblocks //REGISTER YOUR CLASSES HERE.

App.*

.csproj und .sln.

.vscode/

Andere Gruppenverzeichnisse.

Fremde Klassen und Implementierungen.

Die Registrierung eigener Klassen darf ausschließlich im vorgesehenen Registrierungsblock erfolgen, sofern sie notwendig ist.

Falls eine Änderung am Framework oder an gemeinsamen Interfaces notwendig sein sollte:

Sofort abbrechen, die notwendige Änderung erklären und auf meine Freigabe warten.

2. Arbeitsbereich – ausschließlich Gruppe 1

Du darfst ausschließlich die Implementierung von Gruppe 1 bearbeiten.

Keine anderen Gruppen verändern.

Keine fremden Klassen überschreiben.

Keine fremden Dateien löschen.

Keine gemeinsamen Interfaces verändern.

Keine Framework-Erweiterungen eigenständig durchführen.

Bestehende Änderungen anderer Entwickler nicht überschreiben oder zurücksetzen.

Alle Änderungen müssen sich auf Gruppe 1 beschränken.

3. Vier-Gewinnt-Spiel – Benutzeroberfläche verbessern

Das bestehende Vier-Gewinnt-Spiel soll optisch und technisch verbessert werden.

Die vorhandene Spiellogik und Architektur müssen dabei erhalten bleiben.

Anzeige des aktuellen Spielers

Im oberen rechten Bereich des Spielfensters soll eine gut sichtbare Anzeige eingebaut werden.

Anforderungen:

Anzeige der aktuellen Spielernummer.

Spieler 1 wird in seiner Spielfarbe dargestellt.

Spieler 2 wird in seiner Spielfarbe dargestellt.

Die Spielernummer soll deutlich und groß erkennbar sein.

Nach jedem gültigen Spielzug muss automatisch der nächste Spieler angezeigt werden.

Nach Spielende soll der Gewinner beziehungsweise ein Unentschieden angezeigt werden.

Die Anzeige muss zuverlässig mit der eigentlichen Spiellogik synchronisiert sein.

Spielfeld und Eingabemöglichkeiten

Das Spielfeld besitzt weiterhin:

7 Spalten.

6 Zeilen.

Die Bedienung soll verbessert werden:

Jede der sieben Spalten muss direkt mit der Maus anklickbar sein.

Touch-Eingaben sollen unterstützt werden, sofern das vorhandene Framework dies ermöglicht.

Beim Anklicken einer Spalte wird der Spielstein automatisch in die unterste freie Position gesetzt.

Die Spalten müssen eindeutig erkennbar sein.

Die bisherige Tastatursteuerung über die Tasten 1–7 muss erhalten bleiben.

Volle Spalten dürfen keine weiteren Spielsteine aufnehmen.

Ungültige Spielzüge müssen abgefangen werden.

Nach Spielende dürfen keine weiteren Spielzüge möglich sein.

Die bestehende Darstellung darf modernisiert und übersichtlicher gestaltet werden, ohne andere Gruppen oder Framework-Komponenten zu verändern.

4. KI-Systeme analysieren und verbessern

Untersuche sämtliche vorhandenen KI-Implementierungen innerhalb von Gruppe 1.

Vergleiche die vorhandenen Computer-Player und deren Spielstärke.

Ziel: Eine möglichst starke Vier-Gewinnt-KI

Die stärkste KI soll systematisch verbessert werden.

Untersuche insbesondere:

Minimax-Algorithmus.

Alpha-Beta-Pruning.

Optimierung der Suchtiefe.

Bewertung von Spielsituationen.

Erkennung eigener Gewinnmöglichkeiten.

Verhinderung gegnerischer Gewinnzüge.

Erkennung von Doppeldrohungen.

Kontrolle der mittleren Spalten.

Strategische Vorausplanung.

Effiziente Zugberechnung.

Anforderungen an die KI

Eigene direkte Gewinnzüge zuverlässig erkennen.

Gegnerische unmittelbare Gewinnzüge blockieren.

Mehrere Züge vorausdenken.

Strategische Spielsituationen erkennen.

Keine unnötigen oder offensichtlich schlechten Züge machen.

Rechenzeit möglichst gering halten.

Keine ungültigen Spielzüge ausführen.

Mit den vorhandenen Framework-Interfaces kompatibel bleiben.

KI-Vergleich und Benchmark

Vergleiche die vorhandenen KI-Versionen innerhalb von Gruppe 1.

Führe automatisierte Testspiele durch:

KI gegen KI.

Unterschiedliche Startpositionen.

Unterschiedliche Spielsituationen.

Gewinn- und Verlustquoten.

Unentschiedenquoten.

Durchschnittliche Berechnungszeit.

Erkennung taktischer Fehler.

Verbessere die stärkste KI anhand der Ergebnisse.

Ziel ist eine KI, die alle anderen vorhandenen Varianten möglichst deutlich übertrifft.

Behaupte keine Überlegenheit, die nicht durch nachvollziehbare Tests belegt wurde.

5. Fehleranalyse und Bugfixing

Untersuche die gesamte Implementierung von Gruppe 1 auf Fehler.

Prüfe insbesondere:

Fehlerhafte Spielzugberechnung.

Falsche Spielerwechsel.

Fehlerhafte Gewinnerkennung.

Fehlerhafte Unentschieden-Erkennung.

Probleme beim Neustarten des Spiels.

Fehlerhafte Darstellung der Spielsteine.

Probleme bei schnellen Eingaben.

Fehler in der KI-Berechnung.

Fehler bei vollen Spalten.

Probleme mit Maus- und Tastatursteuerung.

Fehler bei der Synchronisierung zwischen Spielfeld und Spiellogik.

Probleme bei der Zusammenarbeit zwischen Painter, Rules und Player.

Behebe alle gefundenen Fehler innerhalb von Gruppe 1.

Achte darauf, dass bestehende funktionierende Funktionen erhalten bleiben.

6. Tests und Qualitätssicherung

Nach den Änderungen müssen folgende Prüfungen durchgeführt werden:

dotnet build

Vorhandene Tests ausführen.

Spiellogik testen.

Gewinnerkennung testen.

Unentschieden testen.

Spielerwechsel testen.

Neustart testen.

KI gegen KI testen.

Benutzeroberfläche überprüfen.

Maus- und Tastatursteuerung überprüfen.

Kompatibilität mit den vorhandenen Interfaces prüfen.

Änderungen dürfen keine bestehenden Framework-Funktionen beschädigen.

7. Dokumentation

Dokumentiere deine Änderungen nachvollziehbar.

Erstelle bei Bedarf Pläne als Markdown-Dateien innerhalb von:

Classes/Gruppe 1/Plans/

Falls Prompts dokumentiert werden, speichere sie in:

Classes/Gruppe 1/Patrick_Prompts.md

Die Dokumentation soll enthalten:

Datum.

Vollständigen Prompt.

Verwendetes Werkzeug und Modell, sofern bekannt.

Kurzen Ergebnisvermerk.

Erfinde keine Angaben zu Werkzeug oder Modell.

8. Git-Regeln – besonders wichtig

Du bleibst auf dem aktuell ausgecheckten Branch.

Folgende Aktionen sind ausdrücklich verboten:

Keine Commits erstellen.

Nichts pushen.

Keinen Branch wechseln.

Keinen neuen Branch erstellen.

Keine fremden Änderungen zurücksetzen.

Keine fremden Dateien verändern.

Vor und nach deinen Änderungen:

git status prüfen.

Änderungen mit git diff kontrollieren.

Sicherstellen, dass ausschließlich erlaubte Dateien von Gruppe 1 betroffen sind.

Falls unerwartet Änderungen außerhalb von Gruppe 1 auftreten, diese nicht eigenständig zurücksetzen. Berichte darüber und warte auf Anweisung.

9. Abschließender Arbeitsauftrag

Arbeite selbstständig und gründlich.

Analysiere zuerst:

Die vorhandene Projektstruktur.

Den Aufbau von Gruppe 1.

Die vorhandenen Interfaces.

Die bestehenden Klassen.

Die vorhandenen KI-Implementierungen.

Bereits vorhandene Fehler.

Erstelle anschließend einen sinnvollen Verbesserungsplan und setze die Änderungen innerhalb der erlaubten Grenzen um.

Am Ende berichte:

Welche Dateien geändert wurden.

Welche Bugs gefunden und behoben wurden.

Welche UI-Verbesserungen umgesetzt wurden.

Welche KI-Verbesserungen umgesetzt wurden.

Welche KI-Vergleiche durchgeführt wurden.

Welche Tests erfolgreich waren.

Ob dotnet build erfolgreich war.

Ob noch Probleme bestehen.

Ob Dateien außerhalb von Gruppe 1 unverändert geblieben sind.

ABSOLUTE PRIORITÄT: Gruppe 1 verbessern, die vorhandene Architektur einhalten, keine Framework-Dateien verändern und niemals committen oder pushen.

Wenn eine Aufgabe nur durch eine unerlaubte Änderung außerhalb von Gruppe 1 lösbar ist, brich ab und erkläre das Problem, anstatt eigenmächtig Änderungen vorzunehmen.

Jeglichen Propmt in die Patrick_lupierz_prompts.md falls vorhanden einfügen.

## 06-11-2026

### Prompt 2
checke ab ob jede dieser anforderungen gefüllt wurde ändere nur die dateien von Gruppe1 niemals was anders

Werkzeug: Copilot SDK in VS Code
Modell: im Chat nicht angezeigt

Ergebnisvermerk: Die Gruppe-1-Implementierung wurde gegen die Anforderungen geprüft. Die Spielfeldanzeige skaliert jetzt mit der verfügbaren Canvas-Größe; die Spieleranzeige wird rechts in der Kopfzeile positioniert. Der Build und 24 Assertions des vorhandenen Testprogramms waren erfolgreich. Im vorhandenen Benchmark gewann die Gruppe-1-KI 6 von 6 Partien gegen die Vergleichs-KI (keine Remis); das ist kein Vergleich mehrerer Gruppe-1-KI-Varianten, da nur eine solche Variante vorhanden ist. Bereits vorhandene Änderungen außerhalb von Gruppe 1 wurden nicht verändert.

### Prompt 3 / 4 

## 10. Absturzsicherheit, Neustarts und KI-gegen-KI im bestehenden Spielfenster

### A. Stabilität und mehrfaches Starten

Das Vier-Gewinnt-Spiel muss auch bei wiederholter Benutzung zuverlässig funktionieren.

Teste insbesondere:

- Neues Spiel mehrfach hintereinander starten.
- Mehrfach schnell hintereinander auf „Start New Game“ klicken.
- Ein Spiel starten, abbrechen und direkt ein neues beginnen.
- Mehrere Spiele hintereinander vollständig durchspielen.
- Nach einem Sieg sofort ein neues Spiel starten.
- Nach einem Unentschieden sofort ein neues Spiel starten.
- Während einer KI-Berechnung einen Neustart auslösen.
- Mehrere Eingaben unmittelbar hintereinander durchführen.

**Anforderungen:**

- Das Programm darf durch wiederholtes Starten oder Neustarten niemals abstürzen.
- Alte Spielzustände dürfen nicht übernommen werden.
- Alte KI-Berechnungen dürfen keine Züge im neuen Spiel ausführen.
- Es dürfen keine doppelten Event-Handler entstehen.
- Es dürfen keine mehrfachen Timer oder Hintergrundprozesse gestartet werden.
- Spielerwechsel und Spielsteinpositionen müssen nach jedem Neustart korrekt zurückgesetzt werden.
- Laufende KI-Berechnungen müssen sicher abgebrochen oder ungültig gemacht werden.
- Ressourcen müssen ordnungsgemäß freigegeben werden.
- Race Conditions zwischen KI-Berechnungen, Benutzereingaben und Neustarts müssen verhindert werden.

### B. Zwei unterschiedliche Gruppen-KIs im bestehenden Spielfenster auswählen

Es soll keine separate Battle-Funktion, kein zusätzliches Fenster und kein eigenständiger Spielmodus entwickelt werden.

Die Möglichkeit, zwei unterschiedliche Gruppen-KIs gegeneinander spielen zu lassen, muss direkt in die bereits vorhandene Spielerauswahl des bestehenden Vier-Gewinnt-Fensters integriert werden.

**Spielerauswahl:**

- Spieler 1 kann eine verfügbare KI aus Gruppe 1 auswählen.
- Spieler 2 kann eine verfügbare KI aus Gruppe 2 auswählen.
- Ebenso sollen KIs weiterer Gruppen auswählbar sein, sofern sie über die vorhandenen gemeinsamen Interfaces kompatibel sind.
- Verwende die bereits vorhandenen Auswahlfelder für Spieler 1 und Spieler 2.
- Erstelle keine zusätzliche Battle-Oberfläche.

Beispiel: Spieler 1 = Gruppe 1 KI, Spieler 2 = Gruppe 2 KI.

### C. Automatischer Spielablauf

Wenn zwei KIs ausgewählt wurden und der Benutzer auf Start klickt, müssen beide KIs automatisch gegeneinander spielen.

Der Ablauf:

1. Die KI von Spieler 1 berechnet ihren Zug.
2. Der Spielstein wird auf dem Spielfeld dargestellt.
3. Die KI von Spieler 2 berechnet ihren Gegenzug.
4. Der Spielstein wird dargestellt.
5. Beide KIs spielen automatisch und abwechselnd weiter.
6. Das Spiel endet bei einem Sieg oder Unentschieden.

Der Spielablauf muss über die vorhandene Spiellogik, Rules, Painter und Player-Architektur laufen.

### D. Technische Anforderungen und Kompatibilität

- Verwende die vorhandenen gemeinsamen Framework- und Vier-Gewinnt-Interfaces.
- Die ausgewählten KIs müssen über ihre vorgesehenen Interfaces angesprochen werden.
- Die Spielerauswahl muss tatsächlich die ausgewählte KI verwenden.
- Der automatische Spielerwechsel muss zuverlässig funktionieren.
- Die Oberfläche darf während der KI-Berechnung nicht unnötig blockieren.
- Ungültige KI-Züge müssen sicher abgefangen werden.
- Endlosschleifen und blockierende Berechnungen müssen verhindert werden.
- Ein Neustart muss laufende Berechnungen sicher abbrechen oder ungültig machen.
- Mehrere KI-gegen-KI-Spiele hintereinander dürfen keinen Absturz verursachen.

Falls eine fremde Gruppen-KI nicht kompatibel ist, untersuche den Grund und dokumentiere ihn.

**Wichtig:**
- Ausschließlich Gruppe 1 bearbeiten.
- Keine Klassen anderer Gruppen verändern.
- Keine gemeinsamen Interfaces verändern.
- Keine Framework-Dateien verändern.
- Keine fremden Implementierungen überschreiben.
- Keine zusätzliche Battle-Oberfläche erstellen.
- Keine bestehende Spielerauswahl unnötig ersetzen.

Wenn die Einbindung einer fremden KI nur durch unerlaubte Änderungen möglich wäre, brich ab und erkläre das Problem.

### E. Abschließende Tests

Teste nach der Umsetzung:

- Gruppe 1 KI gegen Gruppe 1 KI.
- Gruppe 1 KI gegen Gruppe 2 KI.
- Gruppe 1 KI gegen weitere kompatible Gruppen-KIs.
- Mehrere Partien hintereinander.
- Neustart während eines KI-Spiels.
- Neustart direkt nach einem Sieg.
- Neustart direkt nach einem Unentschieden.
- Verhalten bei ungültigen KI-Zügen.
- Verhalten bei nicht reagierenden KI-Gegnern.

Dokumentiere die Testergebnisse nachvollziehbar.

**Endziel:** Ich möchte im normalen Vier-Gewinnt-Fenster die KIs für Spieler 1 und Spieler 2 auswählen können, beispielsweise Gruppe 1 gegen Gruppe 2. Anschließend klicke ich auf Start und beide KIs spielen selbstständig gegeneinander. Auch bei häufigen Neustarts und mehreren aufeinanderfolgenden Partien darf das Programm nicht abstürzen.

==>
 dann ändere nicht die mainwindow sondern erstelle in der .md eine Infoliste die in knappen sätzen sagt was probleme sind dun teste nur wenn eine ki nicht funktioniert dann darfst du daran auch nichts ändern

Werkzeug: Copilot SDK in VS Code
Modell: im Chat nicht angezeigt

### Infoliste: Probleme und Testergebnisse

- `MainWindow.DoComputerMoves()` berechnet KI-Züge synchron in einer Schleife; während KI-gegen-KI ist die Oberfläche blockiert.
- Ein Neustart kann während dieser Schleife nicht verarbeitet werden; ein alter KI-Zug kann daher nicht über einen Spielabbruch ungültig gemacht werden.
- Liefert eine KI `null` oder einen ungültigen Zug, prüft das Fenster den Spielzustand nach dem Aufruf nicht auf Fortschritt; dadurch kann die Schleife hängen bleiben.
- Die vorhandene Spieler-Auswahl und Registrierung enthalten Gruppe-1- und Gruppe-8-KIs; die Gruppe-2-Datei `Classes/Gruppe2/4Gewinnt.cs` ist leer.
- Der vorhandene Prüflauf besteht mit 24 Assertions; er prüft unter anderem direkte Gewinn- und Blockzüge der Gruppe-1-KI.
- Der dokumentierte KI-Benchmark gegen die Vergleichs-KI ergab 6 Siege aus 6 Partien; ein GUI-Test für Neustarts, Abbruch während der Berechnung oder nicht reagierende KIs wurde nicht durchgeführt.
- Keine MainWindow-Datei und keine KI-Implementierung wurde für diesen Auftrag geändert.