# Implementierungsplan – Casino Version 1 (10.11.2026)

## Analyse des Frameworks

- `MainWindow` ruft alle 40 ms `IPaintGame2.TickPaintGameField` und `IGameRules2.TickGameCall` auf.
- Tasten und Klicks gehen nur an `IHumanGamePlayer.GetMove`; das Ergebnis landet in `IGameRules.DoMove`.
- Eingaben kommen nur an, wenn `MovesPossible == true` und `CheckIfPLayerWon() <= 0`.
- Nach jedem Zug wechselt das Fenster den Spieler → Player 1 und Player 2 müssen gesetzt sein.
- Leertaste/Enter lösen den fokussierten Start-Button aus → werden nicht als Spieltasten benutzt.

## Entscheidungen

1. Ein "Zug" ist eine Eingabe (`G1C_CasinoMove`). Die Rules übersetzen sie und geben sie an den `G1C_GameManager`.
2. Virtueller Bildschirm 480x320, wird auf das Canvas skaliert. Karte: 20x12 Kacheln, komplett sichtbar.
3. Bewegung kachelweise mit Interpolation (5 Ticks pro Kachel), Tastendrücke werden gepuffert.
4. Grafiken über Sprite-IDs; eingebaute Pixel-Art, optional PNG-Dateien (siehe `Assets/README.md`).
5. Jedes Minispiel ist eine eigene Klasse (`G1C_CasinoGameBase`), Geld nur über `G1C_MoneyManager`.
6. KI: Roulette (Strategiewahl nach Kasse/Verlustserie), Blackjack (Basic Strategy),
   Poker (Monte-Carlo-Gewinnchance, Pot-Odds, Gegner-Aggression, Bluffs).

## Umgesetzt in Version 1

- Casino-Welt mit Kollision, NPCs, Dialogen, Menü, Ergebnisanzeige
- Roulette, Blackjack, Poker (Heads-Up Limit Hold'em), Spielautomat – jeweils vollständig spielbar
- Zustände: MainGame, Dialogue, Menu, Roulette, Poker, Blackjack, Slots, Result

## Getestet

- `dotnet build`: 0 Fehler, 0 Warnungen
- Automatischer Testlauf (separates Testprojekt, nicht im Repo), 651 Prüfungen bestanden:
  Pokerhand-Bewertung, Blackjack-Werte, Roulette-Auszahlungen, Automaten-Quote (ca. 79 %),
  Chip-Erhaltung über 300 Pokerhände, 3000 Blackjack-Runden, Laufen/Kollision/Dialog/Minispiel-Start
  über die Framework-Schnittstellen, Klicks auf Knöpfe und Roulette-Felder, Zeichnen aller Zustände.
- Nicht automatisch getestet: echte Tastatur-/Mausbedienung im Hauptfenster.

## Offen / Ideen für spätere Sprints

- Kamera und größere Karte, laufende NPCs
- Mehrere Einsätze pro Roulette-Runde, Verdoppeln/Teilen beim Blackjack
- Spielstand speichern, Sound (nur falls mit dem Framework abgestimmt)

## Erweiterung: Zuhause und Außenwelt

Die aktuelle Version ergänzt die Casino-Innenwelt um ein Zuhause als Startszene und eine quadratische 180x180-Stadtkarte mit scrollender Kamera. Straßen, Wege und Brücken überqueren einen gewundenen Fluss; eine Baum-/Wassergrenze schließt die Karte ab. Zuhause, Casino, Krankenhaus und Supermarkt besitzen Türübergänge, und Krankenhaus sowie Supermarkt haben möblierte Innenräume. Nicht kollidierende Enten, Fische, Katzen und Vögel beleben die Außenwelt. Karten, Weltobjekte, Kamera, Kollisionen, Tiere und Übergänge sind getrennt modelliert und verwenden weiterhin die Framework-Eingaben, den 40-ms-Tick und den übergebenen Canvas.
