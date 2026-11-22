# Projektplan Gruppe 2

**Stand:** 22.11.2026
**Teammitglieder:** Elena Stempfle, Lani Sorensen, Stefanie Kolb
**Projekt:** OOPGames – Gruppe 2

## Ausgewertete Unterlagen

Dieser Plan wurde aus den Prompt-Dokumentationen der drei Teammitglieder sowie dem aktuellen Spielkonzept und dem Train-Run-Code abgeleitet:

- `Classes/Gruppe2/Elena_Stempfle_Prompts.md`
- `Classes/Gruppe2/Stefanie_Kolb_Prompts.md`
- `Classes/Gruppe2/Train Run/lani_sorensen_prompts_trainrun.md`
- `Classes/Gruppe2/Train Run/Spielkonzept.md`
- `Classes/Gruppe2/4Gewinnt/`
- `Classes/Gruppe2/Train Run/`

## Projektziele

Gruppe 2 entwickelt zwei Bestandteile:

1. **Vier gewinnt**
   - kompatibel mit den gemeinsam festgelegten Vier-gewinnt-Interfaces;
   - Includes Human-Player, Computer-Player, Rules, Field und Painter;
   - Prüfung von gültigen Zügen, Sieg, Unentschieden und KI-Entscheidungen.

2. **Train Run**
   - eigenständiges, komplexes Echtzeitspiel über `IGameRules2` und `IPaintGame2`;
   - vollständig innerhalb des vorhandenen WPF-Hauptfensters;
   - Eingaben über die vorhandenen Key-Selections;
   - Zeichnung ausschließlich auf dem vom Framework übergebenen Canvas;
   - keine eigenen Fenster, keine eigene Hauptschleife und keine Framework-Änderungen.

## Aktueller Stand

### Vier gewinnt

- Das Spielfeld hat sieben Spalten und sechs Zeilen.
- Field, Move, Rules, Player und Painter sind in den Klassen von Gruppe 2 vorhanden.
- Human- und Computer-Spieler sind getrennt implementiert.
- Der Computer muss mindestens gewinnen oder einen möglichen Sieg des Gegners blockieren.
- Regeltests und ein Build-Nachweis werden vor der Abgabe benötigt.
- Die Kompatibilität mit mindestens drei anderen Gruppen sollte als Matrix dokumentiert werden.

### Train Run

- Das Spiel läuft unter dem Namen "Train Run".
- Die 3D-Szene verwendet WPF `Viewport3D` innerhalb des Canvas-Painters.
- Die drei Gleise bilden die Ausgangsebene für Spurwechsel.
- Spielerinnen können mit Pfeiltasten oder A/D, W und S auf den drei Ebenen bewegen.
- Züge, Güterwagen, Rampen, Schranken, Büsche, Pflanzentöpfe, Bordsteine und Signale sind als Hindernisse vorgesehen oder umgesetzt.
- Münzen und die Upgrades Magnet, doppelte Münzen, Surfboard, Hüpfstab und Jetpack sind Teil des Konzepts.
- Die gemeinsame Zug-Geometrie hält Darstellung und Kollision zusammen.
- Der Bahnwärter dient als sichtbarer Verfolger.
- Die Geschwindigkeit steigt mit Laufzeit und Punktestand.
- Eine lokale C#-Testklasse und ein PowerShell-Skript bilden die wiederholbare Testausführung.

## Umgesetzte Meilensteine

### 1. Framework und Vier gewinnt

- Projektanforderungen als verbindliche Leitplanken festgehalten.
- Vier-gewinnt-Regeln, Feld, Spieler und Darstellung in Gruppe 2 umgesetzt.
- Sprachbezeichnungen und relevante Klassen auf Englisch beziehungsweise Projektbegriffe vereinheitlicht.
- Pull-, Commit- und Push-Abläufe eingehalten.
- Framework-Dateien wurden nicht verändert.

### 2. Train-Run-Konzept

- Subway-Surfer-Idee in ein Train-Run-Konzept überführt.
- Umsetzung mit drei Spuren, drei Ebenen, Hinderniswellen und Upgrades festgelegt.
- Entscheidung für WPF `Viewport3D` getroffen, da diese Lösung innerhalb des vorhandenen Frameworks funktioniert.
- Native Direct3D und separate Fenster wurden verworfen beziehungsweise nicht umgesetzt.

### 3. 3D-Darstellung und Zugwelt

- Züge werden aus einer gemeinsamen Perspektivprojektion auf den Gleisen dargestellt.
- Konvois bestehen aus Rampe, Güterwagen und Lokomotive.
- Nach Rampen existiert mindestens ein begehbarer Zugteil.
- Einzelne Züge können ebenfalls erscheinen und müssen umgangen werden.
- Zugdächer, Rampen, Läufer- und Kamerahöhe stimmen mit der Kollisionslogik überein.
- Güterrampen sind farblich und mit Seitenwänden als begehbare Übergänge erkennbar.

### 4. Spielerfigur und Verfolger

- Die Läuferin wurde verkleinert, damit sie die gesamte Spur nicht verdeckt.
- Eine Laufanimation, ein Side-to-Side-Skateboard-Motion und ein Surfboard-Upgradespielstand wurden umgesetzt.
- Das Skateboard erscheint nur während des Surfboard-Upgrades.
- Der Bahnwärter besitzt eine eigene Laufanimation.
- Der Bahnwärter folgt zu Beginn, fällt danach zurück und verlässt das Sichtfeld.
- Beinahe-Kollisionen verkürzen den Verfolgerabstand.

### 5. Bewegungen und Upgrades

- Spurwechsel, Sprung, Rolle und Zugdach sind in der Rules- und Painter-Logik vorgesehen.
- Das Jetpack hebt die Läuferin in eine eigene Flughöhe.
- Der Magnet zieht Münzen aus mehreren Spuren und Höhenebenen.
- Upgrade-Münzen sind nicht rotierend.
- Normale Münzen drehen sich konstant und nicht schneller bei steigendem Tempo.

### 6. Tests und Wartung

- Eine Gruppe-2-Testklasse enthält Regeln-, Kollisions-, Upgrade-, Bewegung- und Optiktests.
- Das PowerShell-Skript dient nur als Testlauncher.
- Die Tests werden nach Änderungen wiederholt ausgeführt.
- Neue Tests werden bei Verhaltensänderungen und Regressionen ergänzt.
- Das Projekt wird mit `dotnet build` geprüft.

## Verantwortungsbereiche

Die konkrete Git-Historie ist für den endgültigen Nachweis maßgeblich. Die folgende Aufteilung ergibt sich aus den Prompt-Dokumentationen:

### Elena Stempfle

- Vier-gewinnt-Implementierung und Dokumentation.
- Zuglayout und gemeinsame Geometrie.
- Rampen, Zugbewegung, Zugdach und Spurwechsel.
- Train-Run-Umbenennungen und Integration.
- Fehler-, Merge- und Git-Pflege für die Gruppe.

### Stefanie Kolb

- 3D-Visualisierung mit WPF `Viewport3D`.
- Perspektivische Gleise, Beleuchtung, 3D-Szene und Darstellung.
- Wagen, Münzen, Läufer, Bahnwärter und optische Verbesserungen.
- Auswahl der technischen 3D-Lösung passend zum Framework.
- Aktualisierung des Spielkonzepts und der eigenen Prompt-Dokumentation.

### Lani Sorensen

- Code-Reviews und Refactoring.
- Teststrategie und Regressionstests.
- Darstellung der Läuferanimation, des Skateboards und des Bahnwächters.
- Optiktests, Coin-Verhalten und Verfolgeranimation.
- Dokumentation des promptbasierten Entwicklungsverlaufs.

## Produkt-Backlog

### Hohe Priorität

- Build und Start von Train Run im Hauptfenster prüfen.
- Alle drei Teammitglieder lassen das Spiel im Framework starten.
- Hartes Spielende durch schwere Kollisionen nachweisen.
- Zugrampen, Zugdächer und Fall am Zugende nachweisen.
- Münzsammlung und alle Upgrades im HUD prüfen.
- Vier-gewinnt-Kollisionen, Sieg und Unentschieden testen.
- Vier-gewinnt-KI auf gültige und optimale Züge prüfen.
- Kompatibilitätsmatrix mit mindestens drei anderen Gruppen erstellen.

### Mittlere Priorität

- Hinderniswellen auf garantierte freie Spur testen.
- Magnet, Jetpack und Surfboard auf Verdrängung voneinander prüfen.
- Sprung-, Rolle- und Lane-Change-Anweisungen dokumentieren.
- Geschwindigkeit bei höherem Score messen und testen.
- Ladezeit und Rendering-Wiederholungen prüfen.
- Double-Coin-, Jump-Stick- und Jetpack-Restdauern sichtbar machen.

### Niedrige Priorität

- Zusätzliche 3D-Details und Beleuchtungsvarianten.
- Zusätzliche Soundeffekte.
- Komfortoptionen für Tastatursteuerung.
- Weitere Visualisierungsvarianten für Hindernisse.

## Sprintplan

### Sprint 1 – Vier gewinnt und Framework

**Ziel:** Gemeinsame Schnittstellen verstehen und Vier gewinnt vollständig integrieren.

- Verbindliche Interface-Konventionen festlegen.
- Feld, Regeln, Human-Player und Computer-Player implementieren.
- Painter ergänzen und über das Hauptfenster registrieren.
- Build und Grundtests durchführen.

**Ergebnisse:** Implementiertes Vier-gewinnt-Spiel und erste Prompt-Dokumentation.

### Sprint 2 – Train-Run-Basis

**Ziel:** Echtzeitlauf und 3D-Szene innerhalb des Frameworks.

- Field, Rules, Painter und Human-Player trennen.
- Drei Gleise und Spurwechsel implementieren.
- Perspektivische Bahnwelt und erste 3D-Zugmodelle erstellen.
- Wellenstruktur und sichere Spur vorbereiten.

**Ergebnisse:** Laufbare, perspektivische Train-Run-Basis.

### Sprint 3 – Hindernisse und Upgrades

**Ziel:** Gameplay-Komplexität und Belohnungen ergänzen.

- Züge, Rampen, Hindernisse und Kollisionen verbinden.
- Zugdächer und Flughöhe implementieren.
- Münzen und Upgrades ergänzen.
- Bahnwärter und Verfolgerabstand integrieren.

**Ergebnisse:** Vollständige Spielregeln und spielbare Laufvariante.

### Sprint 4 – Stabilisierung

**Ziel:** Qualität, Tests und Abgabe vorbereiten.

- Prompts und Pläne vervollständigen.
- Merge-Konflikte und fremde Dateien vermeiden.
- Regressionstests erweitern.
- Build, Start und Abnahmekriterien prüfen.
- Vier-gewinnt-Kompatibilitätsmatrix erstellen.

**Ergebnisse:** Abgabefähige Version von Vier gewinnt und Train Run.

## Abnahmekriterien

1. Keine Änderungen an Framework-Dateien, gemeinsamen Interfaces oder fremden Gruppenordnern.
2. Alle eigenen Klassen und Dateien liegen in `Classes/Gruppe2/` oder den erlaubten Gruppen-Interfaces.
3. Vier gewinnt ist mit den gemeinsamen Vier-gewinnt-Interfaces kompatibel.
4. Train Run startet über die vorhandene Painter-, Rules- und Player-Auswahl.
5. Train Run zeichnet nur auf dem übergebenen Canvas.
6. Drei Gleise, Spurwechsel, Sprung, Rolle, Hindernisse und Tempo funktionieren.
7. Zugdach, Jetpack, Münzen und Upgrades sind nachvollziehbar.
8. Spielende und Neustart sind vorhanden und testbar.
9. Die Test-Suite läuft erfolgreich.
10. Der Build ist fehlerfrei.
11. Prompt-Dateien, diese Plandokumentation und der zugehörige Code sind committed.

## Offene Punkte

- Product Owner und Scrum Master für die Teamdokumentation benennen.
- Verantwortliche für die letzten offenen Sprintaufgaben festlegen.
- Vier-gewinnt-Kompatibilitätsmatrix erstellen.
- Finale Vier-gewinnt-Testabdeckung prüfen.
- Finale Abnahmetests für Train Run durchführen.
- Git-Status vor jedem Commit kontrollieren.
- Sicherstellen, dass keine fremden Build- oder Objektdateien committed werden.
