# Subway Surfer – project notes

## Context
This file documents the work on the Subway Surfer implementation in the Gruppe2 folder, including the review, refactor, and game-logic fixes.

## Current status
- The code was reviewed and the largest gameplay class was split into smaller files by responsibility.
- The refactor is organized into:
  - `Gruppe2_SubwayField.cs` for the game state and field model
  - `Gruppe2_SubwayPlayer.cs` for player input and move creation
  - `Gruppe2_SubwayRules.cs` for gameplay rules, collisions, upgrades, and wave logic
  - `Gruppe2_SubwayPainter.cs` for drawing/rendering
- The original monolithic `SubwaySurfer.cs` is kept as a lightweight compatibility stub to avoid duplicate class definitions while the split implementation is the active logic.
- The game-over logic was corrected so a run ending in loss is no longer reported as a win.
- The guard now catches the runner when its distance reaches zero.

## Important finding
The main issue in the original rules logic was that `CheckIfPLayerWon()` returned `1` when `GameOver` was true. That meant the UI interpreted a loss as a win and showed `Player 1 Won!`.

The corrected behavior follows the project contract: return `-1` when no player has won and only return a positive player number for an actual win.

## Validation
The solution was built successfully with:

```bash
dotnet build .\OOPGames.sln --nologo
```

Result: build succeeded with 0 errors.

## Prompt history and responses

### Prompt 1
User prompt:
> hi, can you review the code for the subway surfer game in the folde Gruppe2

Assistant answer:
- I reviewed the main Subway Surfer implementation in `SubwaySurfer.cs`.
- I also checked the related interfaces in `Stefanie_Kolb_Gruppe2_SubwaySurferInterfaces.cs`.
- I reviewed the game loop integration in `MainWindow.xaml.cs`.
- I confirmed the project still builds after the review.
- I identified the real game flow issue: `CheckIfPLayerWon()` returned `1` on game over, which made a loss appear as a win in the UI.

### Prompt 2
User prompt:
> can you split the files in a logical way

Assistant answer:
- I split the large game logic into smaller, responsibility-based files:
  - `Gruppe2_SubwayField.cs`
  - `Gruppe2_SubwayPlayer.cs`
  - `Gruppe2_SubwayRules.cs`
  - `Gruppe2_SubwayPainter.cs`
- The original `SubwaySurfer.cs` was reduced to a lightweight compatibility stub so the refactored files are the active implementation and duplicate type definitions are avoided.
- I verified the solution still compiles successfully after the refactor.

### Prompt 3
User prompt:
> in the markdown include my prompts and your answers to them

Assistant answer:
- I updated this file to include the prompt history and the corresponding assistant responses so the documentation reflects the actual conversation progress.
- This markdown now records the review, the refactor, and the fix work in a traceable way.

### Prompt 4
User prompt:
> can you check the subfiles for function, clarity of code and repetitions?

Assistant answer:
- I reviewed the split field, player, rules, and painter files, along with their shared interfaces.
- The responsibility split is understandable, and the public field interface keeps the renderer separated from most gameplay internals.
- I found a gameplay gap: collisions reduce `GuardGap`, but reaching zero does not end the run or otherwise affect gameplay. The chase meter currently communicates a threat that is not enforced.
- `Gruppe2_SubwayPainter.cs` is still very large (849 lines). Its responsibilities could be separated into scene/track, objects and characters, HUD, and drawing helpers.
- The painter redraws the entire canvas and recreates many shapes and brushes on each timer tick; this may cause unnecessary allocations and rendering work.
- Upgrade labels are maintained in both the rules and painter files. A shared label helper could prevent the strings from drifting.
- The split player and field files are small and focused. `StartedGameCall()` is an empty required-interface implementation, and `PaintGameField()`/`TickPaintGameField()` intentionally share the same drawing routine.
- I did not change game code as part of this review.

### Prompt 5
User prompt:
> dont change the painter. can you change the guard behaviour to better reflect that of the vision of the game and rename variables where appropriate for clarity

Assistant answer:
- I left `Gruppe2_SubwayPainter.cs` unchanged.
- Non-fatal obstacle hits reduce the guard distance. When it reaches zero, the guard catches the runner and the game ends with a specific message.
- The guard distance no longer recovers after game over.
- I renamed `HitOrEnd` to `HandleObstacleHit`, clarified its parameter names, and renamed the local obstacle `kind` variable to `obstacleKind`.
- I kept the `GuardGap` property name because the painter and shared field interface use it.
- The solution built successfully with 0 warnings and 0 errors.

## Conversation summary
- User requested a review of the Subway Surfer game code.
- User then requested that the large game implementation be split into more logical files.
- The split was applied by responsibility.
- The game-over condition bug was identified and fixed.
- The guard now ends the run if the chase distance reaches zero.
- The project remains buildable after the refactor.
- The prompt-and-answer history is recorded in this file.
- A follow-up review checked the split files for functionality, clarity, and repetition.

## Notes
This document should be updated as the project continues evolving.
