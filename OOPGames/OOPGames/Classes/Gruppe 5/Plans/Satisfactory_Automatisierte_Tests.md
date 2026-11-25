# Automatisierte Tests für Satisfactory

Die Tests liegen in [`../Tests/SatisfactoryGameplayTests.cs`](../Tests/SatisfactoryGameplayTests.cs) und verwenden xUnit. Das separate Testprojekt greift über `InternalsVisibleTo` auf interne Gruppe-5-Spielmodelle zu. Die Testdatei wird im Hauptprojekt durch ein Präprozessorsymbol deaktiviert und nur im Testprojekt ausgeführt.

## Aktuell abgedeckte Funktionen

- **Kamera-Zoom:** Prüft, dass die anfängliche Rasterkachel durch den vergrößerten Zoom 38,4 statt 32 Bildschirmpixel breit ist.
- **Bauposition:** Prüft, dass ein per Drag-and-drop vorgemerkter Bau die ursprüngliche Zielposition bis zur Verarbeitung behält und nur einmal verarbeitet wird.
- **Spielerblickrichtung:** Prüft für alle vier Richtungen, dass die Vorderseite der gezeichneten Figur zur Aktionskachel zeigt.
- **Gebäudedarstellung:** Prüft anhand gerenderter Pixel, dass Gebäude mindestens
  90 Prozent der Kachelbreite und -höhe ausfüllen. Die Stufenanzeige steht im
  Gebäudemenü statt auf dem Gebäude.
- **Rucksack und Lager:** Prüft die Rucksackgrenze von 50 Gegenständen, dass Überfüllung ohne Lager abgewiesen wird, dass ein Lager zusätzliche Kapazität bietet und dass ein nicht vollständig möglicher Einlagerungsversuch keine Teilmenge verschiebt.
- **Abgelegte Gegenstände:** Prüft die Mengenentnahme aus dem Rucksack, das Wiederaufnehmen am Ablageort und den Ablauf nach zwei Minuten.
- **Fördererausgabe:** Prüft, dass beim Abholen die eingegebene Anzahl entnommen wird und der Rest im Förderer bleibt.
- **Handabbau:** Prüft, dass nach dem Abbau eines Rohstoffs erst nach 50 Spielticks wieder abgebaut werden kann. Bei 40 ms pro Tick entspricht das zwei Sekunden.
- **Manuelles Handwerk:** Prüft, dass eine Herstellung genau ein Produkt startet, Zutaten sofort verbraucht, den Produktplatz reserviert, das Ergebnis erst nach 50 Ticks liefert und nicht automatisch erneut herstellt.
- **Handwerk im Inventar:** Prüft für Eisen- und Kupferbarren, Eisenplatten,
  Eisenstangen und Beton, dass die sichtbare Schaltfläche die richtige
  Herstellung startet und das Ergebnis nach 50 Ticks im Rucksack liegt.
- **Rakete und Spielende:** Prüft die garantierte Uranquelle, fehlende Baukosten,
  erfolgreichen und einmaligen Raketenbau, die Baumenü- und Tank-Klickbereiche,
  schrittweises Betanken aus Rucksack und Lager sowie Nähe- und
  Treibstoffvoraussetzungen. Kontrolliert außerdem die 100 Startticks, gesperrte
  Bewegungen während des Abflugs, den Sieg, den Zugriff auf die Statistik und
  den Reset bei einem neuen Spielfeld.
- **Verkauf der betankten Rakete:** Prüft, dass bei unzureichendem Platz kein
  Uran verloren geht und dass ein erfolgreicher Verkauf das gesamte Uran und
  50 Prozent der Baukosten zurückgibt.
- **Statistik:** Prüft Handabbau, Förderer, automatische Produktion und
  Handwerk sowie dass gescheiterter Abbau und das Abholen von Rohstoffen nicht
  doppelt zählen. Historische Gebäudezahlen bleiben beim Verkauf bestehen.

Die runde Darstellung der Steine, Inventar-Texte und die Raketenanimation sind
visuelle WPF-Ausgaben und werden von diesen Tests nicht per Screenshot geprüft.
Die Raketen-, Abflug-, Sieg- und Statistikansichten werden jedoch im Test
gezeichnet und die relevanten Klickbereiche geprüft.

## Ausführen

Im Repository-Stammverzeichnis:

```powershell
dotnet test "OOPGames\OOPGames\Classes\Gruppe 5\Tests\Gruppe5Tests.csproj" --configuration Tests
```
