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

Build und automatisierte Schnittstellentests decken Gewinn in allen vier Richtungen, Zugablehnung, Human-/Computer-Züge, Rendering auf dem WPF-Canvas und Komponenten-Kombinationen mit Gruppe 2, Gruppe 8 und Gruppe 10 ab. Die Ergebnisse stehen in `ConnectFour_Kompatibilitaet.md`. Ein manueller interaktiver Lauf des Hauptfensters ist ergänzend sinnvoll.
