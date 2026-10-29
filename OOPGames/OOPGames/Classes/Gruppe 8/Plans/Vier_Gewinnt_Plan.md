# Vier Gewinnt – Gruppe 8

## Umsetzung

- Ausschließlich die vorhandenen Verträge verwenden: `I4GWFeld` und
  `I4GWMove` aus `Interfaces/4GWInterfaces.cs` sowie die Framework-Verträge
  `IGameRules2`, `IPaintGame2`, `IHumanGamePlayer` und `IComputerGamePlayer`.
- Alle Spielklassen und Hilfslogik mit dem Präfix `G8_` in
  `Varena_Trabert_VierGewinnt.cs` definieren.
- Spielfeld mit 6 Zeilen und 7 Spalten, wobei Zeile 0 oben und Zeile 5 unten
  ist. Züge wählen eine Spalte; die Regeln bestimmen die tiefste freie Zeile.
- Ungültige, volle oder nach Spielende abgegebene Züge ablehnen. Horizontale,
  vertikale und beide diagonalen Gewinnrichtungen prüfen; bei vollem Feld
  ohne Sieger Unentschieden anzeigen.
- Menschliche Eingabe per Spaltenklick oder Taste 1–7; ungültige Auswahl
  liefert `null`. Computer gewinnt sofort, wenn möglich, blockiert sonst
  einen direkten gegnerischen Gewinn und wählt danach bevorzugt die Mitte.
- Painter: schwarzes Spielfeld, blaue Steine für Spieler 1 und pinke Steine
  für Spieler 2.

## Grenzen und Validierung

- Keine Framework-Dateien und keine fremden Gruppenordner ändern.
- Ausschließlich den vorgesehenen Klassenregistrierungsblock in
  `MainWindow.xaml.cs` ergänzen; dabei die vorhandene Gruppe-10-Registrierung
  beibehalten.
- Projektbuild erfolgreich; WPF-Laufzeittest noch nicht durchgeführt.
- Anschließend Kombinationen mit mindestens drei anderen Gruppen testen und
  die Resultate dokumentieren.
