# Gruppe 1 – Casino Adventure (Spiel 2)

2D-Pixel-Art-Abenteuer im OOPGames-Framework. Die verbindlichen Entwicklungsregeln stehen in `Agents.md`.

Das Spiel startet im Zuhause. Über die quadratische Stadtkarte mit geschlossener Außengrenze erreicht man das Casino. Krankenhaus und Supermarkt sind erkennbare, kollidierbare Gebäude; Zuhause und Casino besitzen betretbare Innenräume.

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
- `World/` – Stadt-, Haus- und Casinokarten, Objekte/NPCs, Kollision, Szenenübergänge
- `Player/` – Spielfigur, Bewegung, Animation
- `CasinoGames/` – Roulette, Blackjack, Poker, Slots, gemeinsame Basis, Karten
- `AI/` – KI-Gegner (Roulette, Blackjack, Poker)
- `UI/` – Renderer, HUD, Dialog, Menü, Ergebnis, Karten
- `Graphics/` – Sprite-Verwaltung und eingebaute Pixel-Art
- `Assets/` – Anleitung zum Austauschen der Grafiken
- `Plans/`, `Scrum/` – Planung und Dokumentation
