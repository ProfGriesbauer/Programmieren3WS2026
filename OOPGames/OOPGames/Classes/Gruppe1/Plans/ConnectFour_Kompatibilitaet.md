# Kompatibilitätstests Gruppe 1

Am 29.10.2026 wurde ein temporärer STA-Test gegen die Komponenten im Repository ausgeführt. Er prüfte Feld- und Zugverträge, Human-/Computer-Player, Rendering auf dem WPF-Canvas sowie die Annahme gruppenfremder `I4GWMove`-Züge.

| Gruppe | G1-Player auf deren Regeln/Feld | G1-Painter auf deren Feld | Deren Painter auf G1-Feld | Deren Zug von G1-Regeln angenommen |
|---|---|---|---|---|
| Gruppe 2 | Bestanden | Bestanden | Bestanden | Bestanden |
| Gruppe 8 | Bestanden | Bestanden | Bestanden | Bestanden |
| Gruppe 10 | Bestanden | Bestanden | Bestanden | Bestanden |

Der Test ist ein automatisierter Komponenten-/Schnittstellentest; die vollständige interaktive WPF-Auswahl im Hauptfenster wurde nicht manuell geprüft. Insbesondere unterscheiden sich die Klick-Geometrien vorhandener Painter. Die Gruppe-1-Eingabe verwendet die vereinbarte Standardgeometrie (linker Rand 20, oberer Rand 80, Zellgröße 50); die Gruppen müssen diese Geometrie für gruppenübergreifende Mausklicks einhalten oder eine gemeinsame Geometrieschnittstelle vereinbaren.
