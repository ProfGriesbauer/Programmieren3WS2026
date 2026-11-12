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
| M                     | Menü                                      |
| P                     | Handy (Kontakte, SMS, Uhr, Aufgaben, Speichern) |
| I                     | Inventar                                  |
| Maus                  | Knöpfe und Roulette-Felder anklicken      |

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
