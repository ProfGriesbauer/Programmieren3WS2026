# Kinderzimmer (90er)

Das Spiel ist ueber `B5RoomPaint`, `B5RoomRules` und `B5RoomHumanPlayer` in
`MainWindow.xaml.cs` registriert. Die unveraenderten Framework-Interfaces laufen
ueber `IGameRules2`; Eingaben werden im Spieler gepuffert und im 40-ms-Tick verarbeitet.

## Visueller Stand

Phase 1 gestaltet die Umgebung mit warmen Waenden und Licht, einem Dielenboden,
einem geometrischen Teppich, abstrakten Postern und einer kleinen Pflanze neu.
Raumabmessungen, Geraetepositionen, Kameras und Spielablauf bleiben erhalten;
die Konsolengehaeuse sind ansonsten noch nicht ueberarbeitet. Die Atari-,
Game-Boy- und Commodore-Bedienelemente werden nur waehrend eines laufenden
Spiels in der jeweiligen Geraeteansicht eingeblendet.

## Steuerung

- Im Zimmer: Pfeiltasten links/rechts oder A/D waehlen ein Geraet, Enter oder Z oeffnet es.
- In der Geraeteansicht: 1/2 waehlt ein Spiel, Enter oder Z startet es.
- Im Spiel: Pfeiltasten oder WASD steuern; Esc oder X geht zur Geraeteansicht zurueck.
- Esc oder X in der Geraeteansicht geht zurueck ins Zimmer.

Die Gameboy- und Atari-Quellen aus der Aufgabenbeschreibung waren in diesem
Repository nicht vorhanden. Snake und Pong sowie das Atari-Spiel sind daher
eigenstaendige Minispiele; Atari und Commodore verwenden eine Variante des
Muenzen-Sammelspiels. Highscores liegen unter
`%LOCALAPPDATA%\\Programmieren3WS2026\\Kinderzimmer90er\\highscores.json`.
