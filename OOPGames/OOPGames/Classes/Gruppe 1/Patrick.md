# Patrick – Vier Gewinnt (Gruppe 1)

## Prompt-Dokumentation

- **Datum:** 29.10.2026
- **Werkzeug:** Copilot SDK in VS Code
- **Modell:** In dieser Sitzung nicht ausgewiesen
- **Prompt:** „ok indem Orderner Gruppe 1 mit den anforderungen dieser pdf erstelle mir das vier gewinnt spiel mit den regeln der pdf mach eine unschlagbare ki und erstelle mir eine .md mit dem namen Patrick.md unter classes mit einem neuen Folder Gruppe 1“
- **Ergebnis:** Gruppe-1-Implementierung für Vier Gewinnt mit Spielfeld, Regeln, Darstellung, menschlichem Spieler und Alpha-Beta-Computer-Spieler erstellt und im Hauptfenster registriert.

## Implementierung

Die Komponenten verwenden die vorhandenen gemeinsamen Schnittstellen `I4GWFeld` und `I4GWMove`. Das Spielfeld hat 6 Zeilen und 7 Spalten. Die Regeln prüfen gültige Spalten und Spielerreihenfolge und erkennen waagerechte, senkrechte und diagonale Vierer sowie ein volles Spielfeld.

Die Computer-KI gewinnt bei einem direkten Gewinnzug und sucht ansonsten mit iterativer Alpha-Beta-Suche, Zentrumsvorrang, Bewertungsfunktion und Transpositionstabelle. Die Suche ist auf Tiefe 12 und 250.000 Knoten begrenzt, damit ein Zug die Oberfläche nicht unbeschränkt blockiert. Sie ist dadurch deutlich stärker als eine reine Gewinn-/Block-KI, aber nicht mathematisch als unbesiegbar garantiert.

## Kompatibilitätshinweis

Die Gruppe-1-Komponenten nutzen die vorhandenen gemeinsamen Vier-gewinnt-Schnittstellen. Die projektweite Kurzfassung verlangt zusätzlich einen abgestimmten gemeinsamen Satz Rollen-Interfaces und Kompatibilitätstests mit anderen Gruppen. Diese Schnittstellenvereinbarung und die geforderten Tests sind nicht in diesem Einzelbeitrag enthalten.
