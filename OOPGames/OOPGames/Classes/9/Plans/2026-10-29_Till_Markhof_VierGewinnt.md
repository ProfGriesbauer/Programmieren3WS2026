# Umsetzungsplan: Vier gewinnt

1. Die vorhandenen Framework- und 4-Gewinnt-Interfaces unverändert verwenden.
2. Feld, Zug, Rules, Painter, menschlichen Spieler und Bot getrennt implementieren.
3. Regeln validieren: gültiger Spieler, Spaltengrenzen, freie Zelle am Spaltenende, vier Gewinnrichtungen und volles Spielfeld.
4. Menschliche Eingaben über `IClickSelection` auf die gemeinsame Brettgeometrie abbilden; bei ungültiger Eingabe `null` liefern.
5. Den Bot zuerst einen eigenen Gewinnzug wählen lassen, andernfalls einen unmittelbaren gegnerischen Gewinn blockieren; sonst die Mitte bevorzugen.
6. Komponenten ausschließlich im Gruppenordner erstellen und nur im freigegebenen Registrierungsblock des Framework-Hauptfensters anmelden.
7. Mit `dotnet build` prüfen. Fremdgruppen-Kompatibilität erst nach Verfügbarkeit des freigegebenen gemeinsamen Interfaces und Testkomponenten bestätigen.

## Offene Rahmenbedingung

Das Repository enthält derzeit `I4GWFeld` und `I4GWMove`, aber keinen gemeinsamen `IC4_*`-Vertrag. Insbesondere gibt es kein gruppenübergreifendes Painter-Interface oder eine vereinbarte Painter-Geometrie. Daher ist die vollständige Austauschbarkeit mit Komponenten anderer Gruppen noch nicht belegbar.
