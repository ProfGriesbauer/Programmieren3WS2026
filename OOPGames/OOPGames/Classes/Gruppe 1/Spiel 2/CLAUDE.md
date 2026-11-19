# CLAUDE.md

Guidance for Claude Code in this folder. The binding project rules are in [Agents.md](Agents.md) (German, ~38 KB) – read it before making non-trivial changes. This file is the codebase map; it does not replace Agents.md.

## What this is

"Casino Adventure" (Gruppe 1, Spiel 2): a Pokémon-Ruby/Sapphire-style 2D pixel-art top-down game (home, a city, casino, shops, bank) built on the **OOPGames** teaching framework (WPF, C#). Course: Programmieren 3, WS 2026/27. Spiel 1 (`Classes/Gruppe 1/*VierGewinnt.cs`, `Classes/Gruppe1/`) is a separate Vier-gewinnt task – do not mix it with this game.

## Repository layout

Git root is `gitrepo/`; the relevant project is `OOPGames/OOPGames/` (`OOPGames.csproj`, `net10.0-windows`, `UseWPF`, no NuGet packages). Other top-level dirs (`HouseExample/`, `MLBattle/`, `Anforderungen/`) are unrelated.

Our code lives only in `OOPGames/OOPGames/Classes/Gruppe 1/Spiel 2/` (this folder). Never edit other groups' folders, `Interfaces/*` base interfaces, `Classes/OOPGames.cs`, `MainWindow.*` (except the `//REGISTER YOUR CLASSES HERE` block), `App.*`, `.csproj`/`.sln`, `.vscode/`, `X_Griesbauer`.

## Build / run

```
cd OOPGames/OOPGames
dotnet build            # Agents.md requires a green build before every push
dotnet run              # opens the framework's main window
```

Tests: `G1C_SelfTest` ([Systems/Patrick_Lupierz_SelfTest.cs](Systems/Patrick_Lupierz_SelfTest.cs)) holds pure-logic tests. It runs **only on explicit call**: in-game menu `B` then `F12`, or headless via a throwaway console project *outside the repo* that references `bin/Debug/net10.0-windows/OOPGames.dll` and calls `G1C_SelfTest.RunAll()` (WPF, `[STAThread]`). No test project may be added to the repo (csproj change needs approval). Add a test there for every fixed bug. Do not claim tests passed that were not run. If `dotnet build` fails with MSB1011, delete the stray `OOPGames_*_wpftmp.csproj` (leftover from overlapping builds).

Starting the game in the UI: choose Painter `Gruppe 1 - Casino Painter (Spiel 2)`, Rules `Gruppe 1 - Casino (Spiel 2)`, and **both** Player 1 and Player 2 = `Gruppe 1 - Casino Spieler` (the framework alternates players per input; a missing Player 2 crashes). Don't press Space/Enter in-game – the focused "Start New Game" button would restart. See [README.md](README.md) for controls.

## Framework contract (how the game plugs in)

Registered in `MainWindow.xaml.cs` (3 lines: `G1C_CasinoPainter`, `G1C_CasinoRules`, human player). Interfaces actually available: `IPaintGame`/`IPaintGame2` (`TickPaintGameField`, every 40 ms), `IGameRules`/`IGameRules2` (`StartedGameCall`, `TickGameCall`), `IGameField`, `IGamePlayer`/`IHumanGamePlayer`/`IComputerGamePlayer`, `IPlayMove`, `IKeySelection`/`IClickSelection`. Do not invent others.

Data flow: input → `G1C_CasinoHumanPlayer` builds `G1C_CasinoMove` → `G1C_CasinoRules.DoMove` → `G1C_GameManager.HandleInput`. Real-time updates come via `TickGameCall` → `G1C_GameManager.Update`. `G1C_CasinoPainter` ([Patrick_Lupierz_Painter.cs](Patrick_Lupierz_Painter.cs)) only draws the manager's state onto the given Canvas. No own window, game loop or `Main` is allowed.

## Code map (prefix `G1C_`, files `Vorname_Nachname_<Inhalt>.cs`)

| Folder | Contents |
|---|---|
| `Framework/` | `G1C_CasinoRules`, `G1C_CasinoField`, `G1C_CasinoMove`, human player, `CasinoInterfaces` |
| `Systems/` | `G1C_GameManager` (central hub, ~800 lines: owns all worlds, state, money, inventory, NPCs, clock, phone), `G1C_GameState` enum (MainGame, Dialogue, Menu, Roulette, Poker, Blackjack, Slots, Claw, Result, Conversation, Title, ListMenu, Phone, Map), `GameClock` (1 game day = 30 real min; drives daylight, shop hours), `MoneyManager`, `FinanceManager` (bank/loan), `Inventory`, `InputManager`, `Phone`, `SaveSystem` (3 slots), `StartFlow` (title/character creator), `TrashSystem` |
| `World/` | Scenes via `IG1C_WorldScene`: `HomeWorld`, `ExteriorWorld` (96×64 tile city), `CasinoWorld`, `BuildingInteriorWorld` (hospital, market, gas station, clothing, bank, furniture); `*Map`, `WorldCamera`, `SceneTransitionManager`, `CollisionManager`, `InteractionManager`, `WorldObject`, `Animal`, `WorldContracts` |
| `Player/` | Player, movement, animation, customizable `PlayerProfile` |
| `NPC/` | Eduard's system: roster, conversations, memory, relationships, tasks, money, speech bubbles, casino event reactions; Denis's city roster and exterior roaming |
| `CasinoGames/` | `CasinoGameBase` + Roulette, Blackjack (split/double), Poker (+`PokerHandEvaluator`), Slots, ClawMachine, Cards |
| `AI/` | Roulette/Blackjack/Poker/Pong opponents |
| `Arcade/` | `G1C_ArcadeGame` base (fee 5, reward once, max 30), StarHunter, Snake, BrickBreaker, PongDuel, `G1C_ArcadeCatalog` (5 cabinets, ≤9 games), `G1C_GuestGameHost`/`G1C_GuestGameCatalog` (plays other groups' registered framework games via `OOPGamesManager.Singleton`, CanBePaintedBy/CanBeRuledBy) |
| `Fishing/` | `G1C_FishingGame` (bridge fishing minigame, slide bar; catches are items with `SellValue`; 20 fish sprites from a free asset pack, stored as pixel data in `G1C_FishSprites`) |
| `UI/` | Renderer (`BeginRotation` for grouped rotation, e.g. poker fan), HUD, DialogueBox, CasinoMenu, ResultScreen, MapView, PhoneView, CardView, StartView, Conversation/NpcOverlay views |
| `Graphics/` | `SpriteLibrary` (sprite-ID → image, PNG override), `BuiltInSprites`, `TownSprites`, `NpcSprites`, `WorldSprites`, `PixelCanvas` |
| `Assets/` | README; `Sounds/` (music + effects, ~28 MB, see its README); `SoundMixer/` (sound catalog, settings, phone web-app server + page). Custom art: drop `<Sprite-ID>.png` into `G1Casino_Assets/` next to the exe; no csproj change needed |
| `Plans/`, `Scrum/` | Plans, ProductBacklog, SprintPlanning, Reviews, Retrospectives |

Keep logic, rendering and collision separate; sprites are referenced by ID only (see Assets/README.md for ID schema).

## Conventions that matter

- Class prefix `G1C_`; file names `Vorname_Nachname_<Inhalt>.cs`; no duplicate classes or colliding `Name` properties (they show up in the main window lists).
- **Attribution is mandatory** (Agents.md §15/§19): class header (`Klasse / Entwickler / Erstellt / Beschreibung`), method comment (`Entwickler / Aufgabe`), and for edits `Änderung durch / Datum / Änderung`. Never remove others' markers; never invent a developer name – ask. AI code is attributed to the human who reviewed and adopted it. Existing authors: Patrick Lupierz, Eduard Schoenfeld (NPC), Denis Mock (world/graphics).
- Each team member documents prompts in `<Vorname_Nachname>_Prompts.md` in this folder.
- Plans go in `Plans/`, user stories in `Scrum/ProductBacklog.md`. The authoritative task source is `Scrum/SprintPlanning.md` (local files, not GitHub issues); status evaluation goes to `Plans/SprintPlanning_Issue_Auswertung.md` (not yet created).
- Git: own commits only, `git pull` + `dotnet build` before push, no force-push, never commit `bin/`/`obj/` (note `HouseExample/.../obj` files show as modified in status – ignore, don't stage).
- Code is written in German comments/UI strings; keep that language.

## Known discrepancies / gotchas

- The pasted/original AGENTS text says ".NET 6", but the csproj targets `net10.0-windows` and Agents.md says .NET 10. The csproj is the truth and may not be changed.
- Status of every SprintPlanning item: [Plans/SprintPlanning_Issue_Auswertung.md](Plans/SprintPlanning_Issue_Auswertung.md) (section H = items added 16.11.2026); all code tasks are implemented as of 16.11.2026; open: city size decision (B4/B7) and a listening/manual check of the 16.11. features in the real main window.
- Interior floors/walls: back wall `tile_<set>_wall`, side/bottom walls `tile_wallside_<set>_<l|r|b>`, rugs `tile_rug_<set>_<mask>` (`G1C_TileMask`), all generated in `G1C_DecorSprites`. Exterior shop windows: `G1C_ShopWindow` (name = seller profile, `ShopLocation` = assortment/hours). Verify against code before reporting status (§16.2).
- Input: the framework only forwards `KeyDown`. Continuous walking polls `Keyboard.IsKeyDown` in the tick (`G1C_InputManager.HeldDirection`); the painter moves keyboard focus to the canvas (`FocusCanvas`) so arrow keys/Space don't hit framework controls.
- Collision must check *all* solid objects on a tile (`G1C_CollisionManager`) – `ObjectAt` returns only the topmost (e.g. a bird on a tree).
- Day change goes through `G1C_GameManager.ApplyNewDay()` (bank interest, guarded per day). Shop hours live only in `G1C_Shop`.
- New `G1C_WorldLocation` values must be appended at the end (save files store the int). Adding a location touches: enum, `G1C_ExteriorMap.DoorTiles`, `AddBuilding` in ExteriorWorld, `G1C_BuildingInteriorWorld` (switch + tile set), `SceneTransitionManager.DisplayName`, GameManager world property + transition ctor, self-test `AllScenes`.
- Family NPCs (`Personality.IsFamily`) never become hostile; pocket money once per game day (`NpcMemory.PocketMoneyDay`). Bikes: `G1C_BikeSystem` (ownership in inventory, category `Fahrzeug`).
- NPC individual dialogue: add `G1C_DialogueTopic`s to `Personality.Topics` (see `G1C_FamilyNpcRoster`, `G1C_ShopNpcRoster`, `G1C_ExtraTopics` for Eduard's/Denis' NPCs); max 4 options per screen. Small-talk compliment/insult lines come from `G1C_SmallTalk` by `Role`.
- Pets: `G1C_PetAnimal` (solid, roams only in the *current* scene via `GameManager.Update`, `PetCanEnter`); voices `G1C_AnimalVoices`. Sprite IDs `pet_<cat|dog>_<variant>_<l|r>_<frame>` in `G1C_AnimalSprites`.
- Sound: catalog `Assets/SoundMixer/Patrick_Lupierz_SoundCatalog.cs` (id → folder, filename regex, defaults, priority); files in `Assets/Sounds/{Musik_Aussen,Musik_Gebaeude,Soundeffekte/{Schritte,Tiere,Casino,Allgemein}}` (wav/mp3 only – WPF cannot play OGG; sources were converted, see its README). `G1C_SoundSystem` (Systems/) is logic only: chance, cooldown, max-simultaneous with priority, per-id exclusivity, loops (`"<id>:stop"`), music rotation; playing goes through `IG1C_AudioBackend` (`G1C_MediaBackend` in the game, a fake in tests). It is silent unless `Enable()` (only `G1C_CasinoRules.ClearField`). Search order for files: env `G1CASINO_SOUNDS`, `G1Casino_Assets/Sounds` next to the exe, the source `Assets/Sounds` found by walking up from the exe. Missing file → built-in chiptune (`G1C_ChipTune`, `%TEMP%/G1Casino_Sounds`).
- Triggers: `Sound.Play("<id>")` in GameManager (steps by tile via `G1C_StepSurface`, bicycle loop, birds only within `Settings.BirdRadius`, pets only on petting); minigames call `Sfx("<id>")`. Settings (`G1C_SoundSettings`, JSON in `%LocalAppData%/G1Casino/sound_settings.json`) are edited from the phone: Menu `B` → `F11` starts `G1C_SoundMixerServer` (web app, PIN, LAN). The server thread only queues commands (`PostCommand`); `Sound.Pump()` runs them on the UI tick because MediaPlayer is UI-thread only.
- Tests that need the real sound files set env `G1CASINO_SOUNDS` (the runner is not inside the source tree); without it the catalog-files test reports NICHT AUSFÜHRBAR.
- Minigames report sounds via `G1C_CasinoGameBase.SoundRequested`. Admin cheat `G1C_CheatCode` (the word ADMIN, only while a slot machine is active) toggles `G1C_Slots.GuaranteedJackpot`.
- Furniture: `G1C_FurnitureEditor` (state `FurnitureEditor`, click → tile via `TileAt`, same math as the painter for non-scrolling scenes); limit `G1C_Shop.MaxFurniturePerItem`.
- Bikes: entering a building sets `G1C_BikeSystem.ParkedAtDoor`; leaving resumes riding. `RideSpriteId` has 4 directions.
- Guest cabinet: `G1C_GuestGameCatalog` pairs rules/painter/players by class-name group prefix (`GroupOf`, `Affinity`); `G1C_GuestGameHost` registers them via `OOPGamesManager.RegisterActive*` while playing (other groups' players read `ActivePainter`) and restores afterwards; `GameManager.GuestGameSource` is injectable for tests. Esc returns to the guest list.
- NPC day schedule: `G1C_NpcSchedule` (town NPCs leave/return only out of the player's sight; `ExteriorWorld/CasinoWorld.SetNpcPresent`).
- The test runner references a *copy* of OOPGames.dll – rebuild the runner after rebuilding the game, or tests run stale code.
