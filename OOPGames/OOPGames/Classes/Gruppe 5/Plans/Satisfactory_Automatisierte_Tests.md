# Automatisierte Tests für Satisfactory

Die Tests liegen in [`../Tests/SatisfactoryGameplayTests.cs`](../Tests/SatisfactoryGameplayTests.cs) und verwenden xUnit. Das separate Testprojekt greift über `InternalsVisibleTo` auf interne Gruppe-5-Spielmodelle zu. Die Testdatei wird im Hauptprojekt durch ein Präprozessorsymbol deaktiviert und nur im Testprojekt ausgeführt.

## Aktuell abgedeckte Funktionen

- **Kamera-Zoom:** Prüft, dass die anfängliche Rasterkachel durch den vergrößerten Zoom 38,4 statt 32 Bildschirmpixel breit ist.
- **Rucksack und Lager:** Prüft die Rucksackgrenze von 30 Gegenständen, dass Überfüllung ohne Lager abgewiesen wird, dass ein Lager zusätzliche Kapazität bietet und dass ein nicht vollständig möglicher Einlagerungsversuch keine Teilmenge verschiebt.
- **Handabbau:** Prüft, dass nach dem Abbau eines Rohstoffs erst nach 50 Spielticks wieder abgebaut werden kann. Bei 40 ms pro Tick entspricht das zwei Sekunden.

Die runde Darstellung der Steine und die Inventar-Texte sind visuelle WPF-Ausgaben und werden von diesen Logiktests nicht per Screenshot geprüft.

## Ausführen

Im Repository-Stammverzeichnis:

```powershell
dotnet test "OOPGames\OOPGames\Classes\Gruppe 5\Tests\Gruppe5Tests.csproj" --configuration Tests
```
