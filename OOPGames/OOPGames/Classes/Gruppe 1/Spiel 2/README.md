# Gruppe 1 – Casino Adventure (Spiel 2)

2D-Pixel-Art-Abenteuer im OOPGames-Framework. Die verbindlichen Entwicklungsregeln stehen in `Agents.md`.

Nach **Start New Game** erscheint das Hauptmenü: *Neues Spiel* (Name eingeben, Haare, Haut und Kleidung wählen) oder
*Spiel laden* (drei Speicherplätze). Das Spiel beginnt im Zuhause. Die Stadt (96x64 Kacheln, scrollende Kamera) hat
geschwungene Straßen, einen Fluss mit Brücken, Parks, Laternen und Mülleimer.

Betretbar sind Zuhause, Casino, Krankenhaus, Supermarkt, Tankstelle und Bekleidungsgeschäft. In den Läden kauft man mit
Chips ein; Kleidung zieht man zu Hause am Kleiderschrank an und sieht sie an der Figur. Getränke und Snacks haben zeitlich
begrenzte Effekte. Eine zentrale Uhr (ein Tag = 30 Minuten) steuert Tageslicht, Öffnungszeiten, SMS und die Erreichbarkeit
der NPCs am Handy. Befreundete NPCs geben ihre Telefonnummer heraus. Gespeichert wird über das Handy.

Enten und Fische leben am Fluss, Katzen und Vögel beleben die Plätze. Vier Stadtbewohner patrouillieren in ihren Vierteln
und lassen sich mit **E** ansprechen; Casino-NPCs und Minispiele bleiben erhalten.

## Starten

Im Hauptfenster auswählen und dann **Start New Game** klicken:

| Liste    | Auswahl                               |
|----------|---------------------------------------|
| Painter  | Gruppe 1 - Casino Painter (Spiel 2)   |
| Player 1 | Gruppe 1 - Casino Spieler             |
| Player 2 | Gruppe 1 - Casino Spieler             |
| Rules    | Gruppe 1 - Casino (Spiel 2)           |

**Beide** Spieler müssen "Casino Spieler" sein: Das Hauptfenster wechselt nach jeder Eingabe
den Spieler. Fehlt Player 2, stürzt das Framework bei der ersten Eingabe ab.

## Steuerung

| Taste                 | Funktion                                  |
|-----------------------|-------------------------------------------|
| W A S D / Pfeiltasten | Laufen (in Spielen: Einsatz / Auswahl)    |
| E                     | Interagieren, Bestätigen, Weiter          |
| Q / Esc               | Abbrechen, Minispiel verlassen            |
| M                     | Stadtkarte (draußen)                      |
| B                     | Menü                                      |
| P                     | Handy (Kontakte, SMS, Uhr, Aufgaben, Speichern) |
| I                     | Inventar                                  |
| Maus                  | Knöpfe und Roulette-Felder anklicken      |

In der Stadt zeigt die Minimap Straßen und Gebäude in der Nähe. **M** öffnet die Stadtkarte;
ein Klick auf ein Gebäude zeigt dessen Namen. Am Bett kann die Aufwachzeit gewählt werden.
Im Casino bietet der Greifautomat eine animierte Item-Chance. Verkaufsautomaten verkaufen
Kaffee, Wasser und Snacks; **F3** kauft dort Kaffee mit 12 % Chance auf ein zweites Item.
Ladenpreise enthalten einen Aufschlag von 5 %. Beim Blackjack sind **3** = Split und **4** = Double.
In der Bank kannst du Chips einzahlen und abheben; der Geldhai verleiht einmalig 250 Chips,
die mit 300 Chips zurückzuzahlen sind. Möbel kaufst du im Möbelhaus und platzierst sie zuhause
über den Einrichtungsplan.

Zuhause wartet **Mama**, im Haus rechts daneben wohnt **Oma Gerda** (betretbar) mit ihren drei Katzen; beide stehen von Anfang an im
Handy. Über "Ich hab da eine Frage ..." haben sie (und die Verkäufer) eigene Themen. Beide geben einmal pro Spieltag
Taschengeld (Mama 25, Oma 20 Chips) und sagen danach einfach Nein; zum Gegner werden sie nie.

**Spielhalle** (mittlere Straße): vier Automaten mit Sternenjäger, Schlange, Ziegelbrecher und Pong-Duell (5 Chips pro
Spiel, gute Punkte bringen bis zu 30 Chips), in der Mitte der Gast-Automat mit den Spielen der anderen Gruppen, dazu
Greifautomat und Highscore-Tafel. **Fahrrad:** steht vor dem Haus ([E] auf-/absteigen, auch über das Inventar), fährt
fast doppelt so schnell; weitere Farben gibt es im Fahrradladen (Fahrrad auf dem Dach). Unter den Pavillon im Park kann
man von unten hineinlaufen. **Oma Gerda** wohnt im Haus rechts neben deinem, zusammen mit ihren drei Katzen.
Die rote **Tankstelle** hat zwei Zapfsäulen und ein Verkaufsfenster: dort mit [E] bei Olli einkaufen, ohne hineinzugehen. Läden mit Öffnungszeiten sind danach abgeschlossen und zeigen ein
CLOSED-Schild (die Tankstelle hat immer offen). Das Bankguthaben wächst pro Spieltag um 2 %. Schilder, Poster und Sticker
lassen sich mit **E** ansehen.

**Neu (16.11.2026):** Katzen und Hunde laufen herum und lassen sich mit **E** streicheln. Auf jeder Flussbrücke steht eine
**Angel**: [E] auswerfen, beim Zucken [E] anhauen, dann den Fisch mit **A**/**D** im grünen Bereich halten; 20 Fischarten und
Fundstücke verkauft man in jedem Laden. Admin-Cheat: im Spielautomaten das Wort ADMIN tippen (Jackpot-Garantie und dauerhaftes Turbo-Tempo an/aus). Am **Einrichtungsplan** stellt man Möbel per Mausklick in der Hausansicht auf und
verschiebt sie (höchstens 3 Stück je Möbel). Musik und Geräusche: im Menü **B** mit **T** an/aus. Wer mit dem Fahrrad in ein
Haus geht, fährt beim Hinausgehen automatisch weiter.

**Selbsttest (nur auf Aufruf):** Menü **B** öffnen, dann **F12**. Die Ergebnisse erscheinen als Liste und in
`G1Casino_SelfTest.txt` neben der `OOPGames.exe`.

Leertaste und Enter **nicht** benutzen: Nach dem Start hat der Button "Start New Game" den Fokus,
diese Tasten würden das Spiel neu starten. Falls die Pfeiltasten nicht reagieren, WASD verwenden.

## Framework-Anbindung

| Framework-Schnittstelle | Klasse                  | Aufgabe                                             |
|-------------------------|-------------------------|-----------------------------------------------------|
| `IPaintGame2`           | `G1C_CasinoPainter`     | zeichnet alle 40 ms auf das übergebene Canvas       |
| `IGameRules2`           | `G1C_CasinoRules`       | `DoMove` = Eingabe, `TickGameCall` = Echtzeit-Update |
| `IGameField`            | `G1C_CasinoField`       | hält den `G1C_GameManager` (Spielzustand)           |
| `IHumanGamePlayer`      | `G1C_CasinoHumanPlayer` | macht aus Taste/Klick einen `G1C_CasinoMove`        |

Eingaben erreichen das Spiel im Framework nur über Player → Rules. Deshalb leiten die Rules sie an den
`G1C_GameManager` weiter; der Painter zeichnet den Zustand und delegiert an das aktive Minispiel.
Außerhalb dieses Ordners wurden nur drei Registrierungszeilen im Block `//REGISTER YOUR CLASSES HERE` der MainWindow ergänzt.

## Spielaufbau

- `Painter.cs` – zeichnet die aktive Szene und Minispiele auf dem vom Framework übergebenen Canvas
- `Framework/` – Rules, Field, Move, Player
- `Systems/` – GameManager, GameState, MoneyManager, InputManager
- `World/` – Karten, Stadt-/Innenraumszenen, Kamera, Kollisionen und Übergänge
- `NPC/` – Casino-Dialoge, Stadtbewohner-Patrouillen und nicht kollidierende Tierdarstellung
- `Player/` – Spielfigur, Bewegung, Animation
- `CasinoGames/` – Roulette, Blackjack, Poker, Slots, gemeinsame Basis, Karten
- `AI/` – KI-Gegner (Roulette, Blackjack, Poker)
- `UI/` – Renderer, HUD, Dialog, Menü, Ergebnis, Karten
- `Graphics/` – Sprite-Verwaltung und eingebaute Pixel-Art
- `Assets/` – Anleitung zum Austauschen der Grafiken
- `Plans/`, `Scrum/` – Planung und Dokumentation
