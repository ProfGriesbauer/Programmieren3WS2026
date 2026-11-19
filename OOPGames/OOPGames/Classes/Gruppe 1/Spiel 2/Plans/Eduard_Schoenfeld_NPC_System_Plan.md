# Plan – NPC-, Dialog- und Beziehungssystem (12.11.2026)

Verantwortlich: Eduard Schoenfeld (KI-unterstützt, siehe `Eduard_Schoenfeld_Prompts.md`)

## Analyse (Ist-Stand)

- 5 vorhandene NPCs im Casino (`G1C_Npc` in `G1C_CasinoWorld`): Croupier Rosa (`dealer_red`), Dealer Ben
  (`dealer_green`), Pokerprofi Max (`dealer_blue`, zugleich Poker-KI „Max“), Barkeeper Toni (`bartender`),
  Gast Lena (`guest`). Grafiken: 16x16-Pixel-Art aus `G1C_BuiltInSprites`, austauschbar über `G1C_SpriteLibrary`.
- Gespräche bisher: einfache Seiten-Dialoge (`G1C_DialogueBox`), Zustand `Dialogue`.
- Geld: `G1C_MoneyManager` (Spieler). Minispiele zahlen aus der Hauskasse. Barkeeper verschenkte 500 Chips
  „aus dem Nichts“, wenn der Spieler unter 50 Chips hatte.
- Ablauf: Eingaben → `G1C_CasinoRules.DoMove` → `G1C_GameManager`; Tick alle 40 ms → `Update`;
  Zeichnen → `G1C_CasinoPainter`. Keine neuen Framework-Schnittstellen nötig.

## Entscheidungen

1. **Keine neuen Figuren:** Die 5 vorhandenen NPCs erhalten Charakterprofile. Archetypen werden auf sie
   verteilt (Max = reicher, arroganter Rivale; Lena = arme, nervöse Spielerin; Toni = hilfsbereit;
   Rosa/Ben = professionelle Croupiers, die laut Hausregel kein Geld verschenken).
2. **Porträts:** `portrait_<skin>_<ausdruck>` (neutral, happy, angry, sad, surprised, thoughtful).
   Eingebaut: Kopf und Oberkörper aus dem vorhandenen Front-Sprite, verdoppelt und mit Gesichtsausdruck
   (Augenbrauen, Mund, Träne) ergänzt – dadurch bleiben Haare, Haut und Kleidung identisch.
   Echte, detailliertere Porträts können als PNG mit derselben ID eingelegt werden.
3. **Neuer Zustand `Conversation`** (Bewegung gesperrt). `Dialogue` bleibt für Automaten und Türen.
4. **Geld fließt nur zwischen Geldbeuteln:** NPC-Geschenke: NPC-Geldbeutel −x, Spieler +x.
   Aufgabenbelohnungen kommen aus der Hauskasse (wie Minispiel-Gewinne) – getrennt von NPC-Geldbeuteln.
   Der Gratis-Barkeeper wird durch einen risikofreien, wiederholbaren Job ersetzt (Gläser einsammeln).
5. **Ereignisse** nutzen nur das vorhandene Minispiel-Ergebnis (Kontostand vorher/nachher), keine eigene
   Ergebnisberechnung. Reaktionen nur von NPCs in Wahrnehmungsreichweite des Tisches.
6. **Nur Sitzungsspeicher:** Alles lebt im `G1C_GameManager`; „Start New Game“ setzt zurück.

## Dateien (neu)

| Datei | Aufgabe |
|---|---|
| `NPC/Eduard_Schoenfeld_NpcCharacter.cs` | Profil, Persönlichkeit, Stimmung, Geldbeutel |
| `NPC/Eduard_Schoenfeld_NpcRelationship.cs` | Vertrauen, Respekt, Beziehungsstatus (mit Hysterese) |
| `NPC/Eduard_Schoenfeld_NpcMemory.cs` | Erinnerungen |
| `NPC/Eduard_Schoenfeld_NpcRoster.cs` | Daten der 5 Charaktere (Texte, Werte) |
| `NPC/Eduard_Schoenfeld_Conversation.cs` | Mehrstufige Gespräche, Antwortoptionen, Wirkungen |
| `NPC/Eduard_Schoenfeld_NpcMoneyService.cs` | Geldanfragen mit Begrenzungen |
| `NPC/Eduard_Schoenfeld_NpcTasks.cs` | Sammelaufgaben (Gläser, verlorene Chips) |
| `NPC/Eduard_Schoenfeld_CasinoEventReactor.cs` | Reaktionen auf Minispiel-Ergebnisse, Duelle, Ruf |
| `NPC/Eduard_Schoenfeld_SpeechBubbles.cs` | Monologe/Sprechblasen-Steuerung |
| `NPC/Eduard_Schoenfeld_NpcSystem.cs` | Zentrale Anbindung an den GameManager |
| `UI/Eduard_Schoenfeld_ConversationView.cs` | Gesprächsfenster mit Porträt und Leisten |
| `UI/Eduard_Schoenfeld_NpcOverlayView.cs` | Sprechblasen, Aufgaben-Gegenstände, Aufgabenanzeige |
| `Graphics/Eduard_Schoenfeld_NpcSprites.cs` | Porträts und Aufgaben-Gegenstände |

Geändert (markiert mit „Änderung durch: Eduard Schoenfeld“): `GameState`, `GameManager`, `Painter`, `SpriteLibrary`.

## Bewusst nicht umgesetzt (Phase 10)

- NPC-Bewegungen/Tagesabläufe: hoher Aufwand (Wegfindung, Kollision mit Spieler), Priorität laut Auftrag
  liegt auf Dialogen und Beziehungen. NPCs drehen sich weiterhin zum Spieler.
- Kostenloses Pokerduell und „anspruchsvollere Duelle“: würden die Poker-Regeln ändern. Duelle sind
  normale Pokerrunden gegen Max.
- Dauerhafte Speicherung: nicht mit Framework/Vorgaben abgestimmt.
