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

## Conversation summary
- User requested a review of the Subway Surfer game code.
- User then requested that the large game implementation be split into more logical files.
- The split was applied by responsibility.
- The game-over condition bug was identified and fixed.
- The project remains buildable after the refactor.
- The prompt-and-answer history is recorded in this file.

## Notes
This document should be updated as the project continues evolving.
