# Gruppe 1 – Connect Four

## Ziel

Implementiere ein austauschbares 6x7-Vier-Gewinnt-Spiel für das OOPGames-WPF-Framework. Die Komponenten verwenden die vorhandenen gemeinsamen Verträge `I4GWFeld` und `I4GWMove`; Framework-Dateien bleiben unverändert.

## Komponenten

- Spielfeld und Zug implementieren die gemeinsamen Vier-Gewinnt-Schnittstellen.
- Rules setzen abwechselnde Züge durch, lassen Steine in der gewählten Spalte fallen und prüfen Sieg in allen vier Richtungen sowie Unentschieden.
- Der Painter unterstützt jedes `I4GWFeld` und zeichnet auf das übergebene Canvas.
- Der Human Player verarbeitet Mausklicks im Spielfeld und die Tasten 1–7.
- Der Computer Player gewinnt sofort, blockiert einen direkten Gewinn und bevorzugt danach sichere, zentrale Spalten.
- Alle Komponenten werden ausschließlich im vorgesehenen Registrierungsslot von `MainWindow.xaml.cs` registriert.

## Validierung

Build und automatisierte Schnittstellentests decken Zugverträge, Player, Rendering und Kombinationen mit den aktuell registrierten Gruppen ab. Die Ergebnisse und gruppenfremden Kompatibilitätsblocker stehen in `ConnectFour_Kompatibilitaet.md`. Ein manueller interaktiver Lauf des Hauptfensters ist ergänzend sinnvoll.
