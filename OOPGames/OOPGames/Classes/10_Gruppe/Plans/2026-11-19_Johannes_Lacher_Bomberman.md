# Bomberman for the Commodore (Retro Kinderzimmer, group 10)

| | |
|---|---|
| **Status** | Implemented and tested in a test program (see section 7). Not yet discussed with the team / lecturer |
| **Author** | Johannes Lacher |
| **Branch** | `gruppe10` |
| **Code** | `Classes/10_Gruppe/Bomberman/`, `Interfaces/10_Gruppe/Bomberman/`; shared with Pac-Man: `Classes/10_Gruppe/Shared/`, `Interfaces/10_Gruppe/Shared/` |

## 1. Goal

A Bomberman-style game as a new cartridge for the **Commodore home computer** in the Kinderzimmer.
The Commodore had only two games so far (Tower Defense, Muenzen sammeln), and the genre started on
home computers, so it fits there. Like Pac-Man, the game should show clean object-oriented design:
small classes with one job each, behaviour you can swap through interfaces (Strategy, State), and game
logic that is independent of WPF and can be tested on its own.

**Scope**

- Arena of 15 × 13 cells: outer wall, a grid of pillars, and random bricks.
- Bombs with a fuse, flames in four directions, chain reactions; a flame destroys the first brick in
  its way.
- Hidden under the bricks: one exit and one power-up per stage (more bombs, longer flames, speed,
  detonator).
- Five enemy types with different speed, points and behaviour (wander, chase, pass through bricks).
- 8 stages, a 200-second timer (when it runs out, phantoms appear), lives, time bonus, highscore.
- Phases: STAGE intro → playing (with pause) → death / stage clear → GAME OVER or victory.
- Sound in the style of the C64 sound chip: background loop, jingles, effects (see section 6).

**Original content only.** The hero (a small demolition robot with a hard hat), the enemies, the
graphics, the tunes and the stage data are our own designs. There are no original sprites, sounds,
music, names or level data. Colours follow the 16-colour palette of the C64.

**Not included**: multiplayer (battle mode), kicking bombs.

## 2. Constraints

- Runs as `IB5RoomMiniGame` inside the existing hub. No dedicated window or main loop, no framework
  change. Updates come in the hub's **40 ms tick (25 Hz)**.
- The framework only reports **KeyDown**. Walking needs held keys, so the input mapper also reads the
  keyboard state (`IG10_KeyStateSource`, the same approach as Tetris and Void Courier).
- Names: classes `G10_Bomber…`, interfaces `IG10_Bomber…`, files `Johannes_Lacher_Bomberman<Topic>.cs`.
- Changes only inside `Classes/10_Gruppe/` and `Interfaces/10_Gruppe/`. Outside the `Bomberman`
  folders: the cartridge entry in `B5RoomRegistry` (`Kinderzimmer90er/B5_RoomCore.cs`), and the move
  of the pixel and sound classes from Pac-Man into `Shared/` (see section 4).

## 3. Display

- Logical resolution **320 × 240** (4:3, like the C64 screen with its border), scaled by the hub with
  nearest-neighbour filtering.
- Top bar: score, highscore, time. Left panel: stage and the player's upgrades. Right panel: lives,
  enemies left, "EXIT" when the exit is open, key hints.
- Arena 240 × 208 px (cells of 16 × 16 px). Floor and pillars are drawn once per arena and cached.
  Each frame draws only bricks, items, bombs, flames, actors and the HUD.
- The stage intro looks like the C64 start screen (light-blue border, "READY." with a blinking cursor).

## 4. Architecture

Same layering as Pac-Man. Dependencies only go downwards:

```
 Hub (B5RoomRegistry, painter)        — knows only IB5RoomMiniGame
   │
 Adapter:  G10_BombermanGame          — keys → G10_BomberInput, owns session, renderer, audio; highscore
   │
 View:     G10_BombermanRenderer      — reads only IG10_BomberView (read-only)
           G10_BombermanAudio         — reads only IG10_BomberView, plays through IG10_ClipOutput
   │
 Model:    G10_BomberSession & co.    — no WPF, no keys, no drawing; deterministic for a given seed
```

| Class | Responsibility |
|---|---|
| `G10_BomberSession` | One whole game: score, lives, stage progress, current phase. Implements `IG10_BomberView`. |
| `G10_BomberPhases` + `IG10_BomberPhase` | **State pattern**: Intro, Playing, Paused, Dying, StageClear, GameOver/Victory. Each phase returns the next one. |
| `G10_BomberRound` | One attempt at one stage: arena, player, enemies, bombs, timer, collisions, sound cues. Returns `Running` / `PlayerDied` / `StageCleared`. A lost life throws the round away. |
| `G10_BomberSpawner` | Where and when enemies enter: at the start (away from the player), door guards after a flame hit the exit (only once the player is 2 cells away), phantoms at time up. |
| `G10_BomberArena` + `G10_BomberArenaGenerator` | Cells, bricks, hidden items, burning bricks. Knows nothing about bombs or actors. |
| `G10_BomberBlastSystem` | Bombs and flames: fuse, remote detonation, spreading, chain reaction, burning items, exit hit. |
| `G10_BomberActor` (abstract) | Position in sub-pixels, lane movement, collision. **Template method**: subclasses say whether they pass bricks. |
| `G10_BomberPlayer` + `G10_BomberPlayerStats` | Steering with corner assist (slides into the next lane), upgrades. |
| `G10_BomberEnemy` + `G10_BomberEnemyType` | Moves from cell centre to cell centre; the brain chooses at each centre. Type = data (speed, points, passes bricks, brain). |
| `IG10_BomberEnemyBrain` | **Strategy pattern**: `G10_BomberWanderBrain` (random at crossings), `G10_BomberChaseBrain` (breadth-first search to the player within a range, otherwise a fallback brain). |
| `G10_BomberStages` | Data of the 8 stages (brick density, power-up, enemies, door guard). |
| `G10_BomberInputMapper` | Held direction (last pressed wins), single presses without key repeat. |
| `G10_BombermanRenderer`, `G10_BombermanHud`, `G10_BomberFlamePainter`, `G10_BombermanSprites` | Drawing only, with the shared pixel classes. |
| `G10_BombermanAudio` + `G10_BombermanSoundSynth` | Which sound plays when (cues, phase jingles, steps, loop, mute); the synth generates the clips. |

Actors only see the field through `IG10_BomberObstacles` (implemented by the round), so player and
enemies can be tested with any field.

**Shared with Pac-Man** (`Classes/10_Gruppe/Shared/`). These classes used to sit inside the Pac-Man
files, which made Bomberman depend on Pac-Man without it being visible. They were moved without
changes in behaviour: `G10_Direction`, `G10_PixelBuffer`, `G10_ClipRect`, `G10_Sprite`,
`G10_PixelCanvas`, `G10_PixelFont`, `G10_C64Colors`, and for sound `G10_SoundClip`, `G10_ToneWriter`
(new: a noise wave for explosions) and `G10_MediaClipOutput : IG10_ClipOutput`. Pac-Man's
`G10_MediaSoundOutput` is now a small adapter onto the shared output; its sound files keep their
names.

## 5. Rules in detail

| Topic | Rule |
|---|---|
| Movement | Player 1.25 px/tick, +0.25 per speed level (max 3). Off the lane, the player slides into a lane that leads on (corner assist). |
| Bombs | Fuse 2.5 s. Capacity and range start at 1 (max 8). The player can walk out of their own bomb, but not back in. Bombs block enemies. |
| Flames | Last 0.5 s. Stop at pillars, destroy the first brick (no flame on it), set off other bombs, burn uncovered power-ups. |
| Detonator | Bombs wait for **C**; the oldest one goes off. Lost when a life is lost; bombs, fire and speed stay. |
| Enemies | Puff (slow, wanders) 100 · Spark (chases within 4 cells) 200 · Drift (passes bricks) 400 · Hunter (fast, chases within 10) 800 · Phantom (fastest, passes bricks, always chases) 1000. |
| Exit | Opens when all enemies are gone. A flame on the exit lets 3 door guards out, after the flame and only once the player is at least 2 cells away. |
| Time | 200 s per stage. At 0: 4 phantoms. Time bonus at the exit: 10 points per second left. Power-up: 1000 points. |
| Lives | 2 in reserve. Death → the stage restarts with a new arena. |

## 6. Controls

| Key | Action |
|---|---|
| Arrow keys / WASD (hold) | Walk |
| Z / Space | Drop a bomb |
| C | Detonate (with the detonator) |
| Enter / P | Pause, resume, new game after GAME OVER |
| M | Sound on / off |
| Esc / X | Back to the device view (hub) |

**Sound.** All clips are generated in code with square, triangle and noise voices, like the C64
sound chip. The model only reports cues (`G10_BomberSoundCue`: bomb placed, explosion, power-up,
enemy defeated, player died, time up, exit opened, door guards); `G10_BombermanAudio` decides what
plays. On top of that: jingles for stage start, stage clear, game over and victory; footsteps; and a
3.2-second background loop (bass, arpeggio, noise drums) that switches to a faster version in the
last 30 seconds. The loop is restarted by the updates, so it ends on its own after Esc.

## 7. Status (2026-11-19): implemented

Verified with a throwaway test program in the session scratchpad that references `OOPGames.csproj`
and drives the real classes (not committed). **98 of 98 checks pass**:

- Arena: layout, free start zone, exactly one exit and one power-up under bricks (800 generated arenas).
- Player: walking, walls, corner assist in both directions, walking out of own bomb, speed cap.
- Blasts: fuse timing, cross shape and end pieces, pillar and brick stop, chain reaction, capacity,
  remote bombs, items uncovered after burning, power-up burnt by flame, exit hit reported once.
- Enemies: never inside blocked cells and always on a lane (3000 ticks per type), chase finds the
  player, Drift passes bricks, turning around at a fresh bomb.
- Session: intro, pause, death by own bomb, escape, enemy killed (points + popup), power-up pickup,
  exit closed while enemies live, door guards, door guards waiting while the player is on the exit,
  time up, stage clear, victory after stage 8, game over, detonator lost on death, 20 random games
  without exceptions.
- Audio (with a fake output): stage jingle, loop start and restart, faster loop in the last 30 s,
  pause stops the loop, bomb/explosion/step sounds, mute, jingle waiting for its file, all 16 clips
  audible; Pac-Man's clip names unchanged after the move to `Shared/`.
- Input mapper: held keys, last pressed wins, tap, key repeat ignored, Enter from the hub ignored.
- Rendering: screenshots of all phases checked by eye; the real hub selects the Commodore, inserts
  the cartridge and shows the game in the CRT frame.

Fixed during testing: the power-up blink turned the whole cell white (now only the border blinks), the
dead player was drawn again under GAME OVER, and a pixel error in the Puff sprite.

**Still to check by hand in the running program**: feel of the controls with a real keyboard (held
keys, two directions at once), the balance of the 8 stages, the speed on the lecture PCs, how the
sounds and the loop actually sound (volume, gaps when the loop restarts), and that Pac-Man's sound
still plays after the move to `Shared/`.
