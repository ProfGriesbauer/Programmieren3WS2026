# Testplan für Hay Day

## Zweck

Dieser Plan legt fest, welche automatisierten Tests später für Hay Day erstellt werden und was jeder Test prüft. Sobald das Testprojekt umgesetzt ist, soll die Aufforderung **„Führe die Hay-Day-Tests durch“** bedeuten: alle unten aufgeführten automatisierten Tests ausführen, Ergebnisse zusammenfassen und bei Fehlern den Testnamen sowie die Ursache nennen.

Der Plan selbst führt noch keine Tests aus und ändert kein Spielverhalten.

## Geplanter Testaufbau

- Ein separates Projekt `Gruppe8HayDayTests` unter `OOPGames` mit xUnit; es referenziert das Spielprojekt, enthält aber keinen Spielcode.
- Tests verwenden pro Test einen frischen Feldzustand und verändern keine gemeinsam genutzten Fixtures.
- Zeit wird in Tests über feste Zeitwerte vorgegeben, nicht durch Warten oder Abhängigkeit von der echten Uhr.
- Das Spiel speichert derzeit unter einem festen AppData-Pfad. Vor automatisierten Tests, die `Save`, `LoadOrCreate` oder `HayDayRules.DoMove` ausführen, muss eine Testnaht für einen separaten temporären Speicherpfad bereitstehen. Die Tests dürfen niemals den echten Spielstand lesen oder überschreiben.
- Keine Tests parallel ausführen, solange sie nicht nachweislich getrennte Feld- und Speicherzustände verwenden. Nach Isolation kann parallele Ausführung aktiviert werden.
- Die Testnamen sollen das erwartete Verhalten ausdrücken, z. B. `Harvest_WhenCropIsRipe_AddsThreeItems`.

## Testkatalog

### A. Build- und Testlauf

1. **Spielprojekt baut**
   - Das OOPGames-Projekt wird für seine konfigurierte Zielplattform gebaut.
   - Prüft, dass Änderungen an Klassen, Interfaces und Typen kompilieren.
2. **Gesamte Hay-Day-Testsuite läuft**
   - Führt alle Tests dieses Testprojekts aus und liefert einen eindeutigen Pass-/Fail-Status.
   - Das ist der Standardlauf für die Aufforderung „Führe die Hay-Day-Tests durch“.

### B. Startzustand und Objektgrenzen

1. **Neuer Hof hat erwartete Startwerte**
   - Prüft 500 Münzen, 100 Plätze in Scheune und Silo, fünf Hühner, zehn Weizen, zehn Mais, drei Futter sowie sechs leere Felder.
   - Prüft, dass die Inventarwerte für Eier, Kuhfutter, Schaffutter, Milch, Wolle und Käse initial null sind.
2. **Startobjekte sind unabhängig**
   - Erstellt zwei frische Höfe und ändert im ersten Feld Inventar, Feldfrucht oder Gebäude.
   - Prüft, dass sich der zweite Hof nicht ändert; keine Dictionarys, Listen oder Farmobjekte dürfen versehentlich zwischen Spielinstanzen geteilt werden.
3. **Feldgrenzen werden eingehalten**
   - Prüft, dass akzeptierte Feldpositionen innerhalb von 50 Zeilen und 100 Spalten liegen.
   - Aktionen mit Positionen außerhalb des Spielfelds dürfen keine Ressourcen, Gebäude oder Felder verändern.

### C. Pflanzen und Ernten

1. **Feld kann Weizen oder Mais pflanzen**
   - Prüft, dass eine gültige Pflanzaktion genau ein Saatgut der ausgewählten Sorte verbraucht und Sorte sowie Pflanzzeit am Feld speichert.
2. **Pflanze reift erst nach zwei Minuten**
   - Prüft bei einer festen Pflanzzeit: vor Ablauf von 120 Sekunden nicht reif, ab 120 Sekunden reif.
   - Wiederholtes Fortschreiben der Zeit markiert ein Feld nur einmal als neu gereift.
3. **Reife Pflanze ernten**
   - Prüft, dass die Ernte drei Einheiten der Feldfrucht ins Inventar legt und das Feld anschließend leer und wieder bepflanzbar ist.
4. **Nicht reife Pflanze ernten**
   - Prüft, dass eine unreife Pflanze nicht geerntet oder aus dem Feld entfernt wird.
5. **Volles Lager verhindert Ernteverlust**
   - Prüft, dass bei unzureichendem Lagerplatz weder die Ernte dem Inventar hinzugefügt noch die Pflanze gelöscht wird.
6. **Leeres Feld ohne Saatgut**
   - Prüft, dass ungültige Pflanzversuche keine negativen Inventarwerte oder versehentlich bepflanzten Felder erzeugen.

### D. Ressourcen, Lager und Produktion

1. **Inventar bleibt innerhalb der Kapazität**
   - Prüft erfolgreiche Einlagerung bis zur Kapazität und Ablehnung von Einlagerung darüber hinaus.
   - Bestand darf weder negativ noch größer als die zugehörige Lagerkapazität werden.
2. **Futtermühle verarbeitet jedes Rezept korrekt**
   - Hühnerfutter: ein Weizen und ein Mais werden verbraucht, ein Hühnerfutter entsteht.
   - Kuhfutter: zwei Weizen und ein Mais werden verbraucht, ein Kuhfutter entsteht.
   - Schaffutter: ein Weizen und zwei Mais werden verbraucht, ein Schaffutter entsteht.
   - Bei fehlenden Zutaten oder vollem Ziellager ändern sich Zutaten und Bestand nicht.
3. **Molkerei verarbeitet Käse**
   - Prüft, dass zwei Milch zu genau einem Käse werden.
   - Bei weniger als zwei Milch oder vollem Käselager bleiben alle Bestände unverändert.
4. **Tierproduktion verwendet Futter und Zeit korrekt**
   - Mit fester Zeit wird geprüft, dass die vorhandenen Tiere nur bei vorhandenem Futter Produkte erzeugen.
   - Prüft die Produktionsintervalle: Eier pro 60 Sekunden, Milch pro 300 Sekunden und Wolle pro 600 Sekunden je Tier.
   - Prüft Futterverbrauch, angesammelten Teilfortschritt und Verhalten bei vollem Lager.
5. **Zeitfortschritt ist stabil**
   - Derselbe Zeitabschnitt darf nicht doppelt verarbeitet werden.
   - Eine rückwärts gesetzte Zeit darf keine zusätzlichen Produkte oder negativen Fortschritt erzeugen.

### E. Käufe, Verkäufe und Marktaufträge

1. **Kauf prüft Münzen und Lagerplatz**
   - Prüft erfolgreichen Kauf mit ausreichenden Münzen und Platz sowie korrekte Änderung von Münzen und Warenbestand.
   - Bei zu wenig Münzen oder Lagerplatz bleiben beide Werte unverändert.
2. **Gebäudekauf prüft Kosten und Platzierung**
   - Prüft den Kauf eines Gebäudes bei ausreichenden Münzen an einer gültigen freien Position.
   - Prüft, dass zu wenig Münzen, eine belegte Position oder eine Position außerhalb des Spielfelds weder Gebäude noch Münzbestand unzulässig verändert.
   - Prüft steigende Kosten bei weiteren Gebäuden derselben Art, sofern diese Spielregel beibehalten wird.
3. **Verkauf prüft Bestand und Erlös**
   - Prüft, dass nur vorhandene Waren verkauft werden und dass sich Bestand und Münzen gemäß dem festgelegten Marktpreis ändern.
   - Verkauf ohne ausreichenden Bestand darf keinen negativen Bestand erzeugen.
4. **Lieferauftrag benötigt die geforderte Menge**
   - Prüft, dass bei ausreichender Warenmenge die geforderte Menge abgezogen und die Belohnung gutgeschrieben wird.
   - Bei fehlender Menge bleiben Bestand und Münzen unverändert; ein Auftrag darf nicht mehrfach eingelöst werden.

### F. Spielregeln und Züge

1. **Nur gültige Hay-Day-Züge verändern den Hof**
   - Prüft, dass `null`, ein Zug eines falschen Typs und ein Zug des nicht aktiven Spielers den Zustand nicht verändern.
2. **Gültiger Spielerzug wird angewendet**
   - Prüft, dass ein Zug des erwarteten Spielers angewendet wird und danach der nächste Spieler an der Reihe ist.
3. **Spielziel bleibt korrekt**
   - Prüft das aktuell vorgesehene Ergebnis von `CheckIfPLayerWon` (kein Gewinner, solange Hay Day kein Gewinnziel verwendet).

## Ausführung und Zusammenarbeit

- Sobald das Testprojekt erstellt und in die Lösung aufgenommen wurde, soll der Standardlauf vom Repository-Ordner aus lauten:

  ```powershell
  dotnet test .\OOPGames\Gruppe8HayDayTests\Gruppe8HayDayTests.csproj
  ```

- Die Tests sollen auch über Test Explorer in VS Code auffindbar sein. „Führe die Hay-Day-Tests durch“ führt dieselbe vollständige Suite aus; auf Wunsch kann zusätzlich ein einzelner Test per Name ausgeführt werden.
- Vor einem gemeinsamen Commit: Standard-Testlauf ausführen und nur fortfahren, wenn Build und Tests erfolgreich sind.
- Tests müssen unabhängig voneinander und in beliebiger Reihenfolge bestehen. Gemeinsame globale Testdaten, echte Wartezeiten und produktive Save-Dateien sind verboten.

## Noch zu erledigen, bevor Tests implementiert werden

1. Testprojekt anlegen, xUnit und die passende .NET-Zielplattform einrichten und in die Lösung aufnehmen.
2. Einen frischen, deterministischen Feldzustand für Tests verfügbar machen.
3. Uhr und Speicherpfad so isolieren, dass Zeit- und Regeltests deterministisch sind und keine Nutzerdaten berühren.
4. Die Testfälle in der Reihenfolge A bis F umsetzen und nach jeder Gruppe die gesamte Suite ausführen.

## Abgrenzung

Dieser erste Testumfang prüft Spielregeln und Zustandsänderungen. Painter, Bildschirmdarstellung, Maus-Hit-Tests und manuelle UI-Abläufe sind zunächst ausgeschlossen. Save-/Load-Verhalten wird nur soweit benötigt isoliert, um die Tests sicher ausführen zu können; eine vollständige Prüfung der Speicherformat-Kompatibilität ist eine spätere Erweiterung.
