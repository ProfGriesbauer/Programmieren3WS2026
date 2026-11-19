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

### Prompt 6
Timestamp: 2026-11-12T10:44:05+01:00

User prompt:
> hi, can you continuously log this conversation into this file

Assistant answer:
- I’m logging this conversation to this markdown file and will keep appending future prompt/response entries here so the discussion stays traceable.
- This file now serves as the ongoing project conversation log for the Subway Surfer work.

### Prompt 7
Timestamp: 2026-11-12T10:45:33+01:00

User prompt:
> can you create tests given the current code for the subway surfer game and the deisgn sheet that can be found in the gruppe2 folder.

Assistant answer:
- I created a dedicated Subway Surfer test suite in the game folder so the checks stay within the Subway Surfer area.
- The tests cover the current implementation against the design sheet: initial state, three-lane logic, jump/roll transitions, human input, obstacle damage versus fatal collisions, rewards and upgrades, and wave spawns that leave a safe lane.
- The suite passes in a temporary validation harness and the solution still builds successfully.

### Prompt 8
Timestamp: 2026-11-12T10:49:14+01:00

User prompt:
> where is the file you are trying to chnage

Assistant answer:
- The change target is the Subway Surfer implementation area in the Gruppe2 folder: [Classes/Gruppe2/Subway Surfer](/Users/lania/Downloads/Programmieren3WS2026/OOPGames/OOPGames/Classes/Gruppe2/Subway Surfer).
- I corrected course to keep all work limited to that directory and to avoid edits in the shared test harness outside the game folder.

### Prompt 9
Timestamp: 2026-11-12T10:50:24+01:00

User prompt:
> you should only change files in the subway surfer file

Assistant answer:
- I reverted the unrelated test harness change and kept all work inside the Subway Surfer area.
- I then created a dedicated Subway Surfer test suite in the game’s project folder instead of modifying the shared project test harness.
- The new test file covers initial state, movement and lane bounds, jump and roll transitions, human input handling, fatal and non-fatal obstacle behavior, reward collection and upgrades, and wave spawning with a guaranteed safe lane.

### Prompt 10
Timestamp: 2026-11-12T10:55:45+01:00

User prompt:
> how can i run your tests?

Assistant answer:
- I added [RunSubwayTests.ps1](./RunSubwayTests.ps1), a folder-local script that creates a temporary console runner and invokes `Gruppe2_SubwayTests.RunAllTests()`.
- Run it from PowerShell with:
  `powershell.exe -NoProfile -ExecutionPolicy Bypass -File "C:\Users\lania\Downloads\Programmieren3WS2026\OOPGames\OOPGames\Classes\Gruppe2\Subway Surfer\RunSubwayTests.ps1"`
- This is a custom test runner, not a `dotnet test` project. The current environment blocked execution of the generated runner because of its application-control policy; the test suite itself had passed in an earlier temporary validation harness.

### Prompt 11
Timestamp: 2026-11-12T11:00:53+01:00

User prompt:
> is there a way that you can run the tests automatically with every code change?

Assistant answer:
- A local file watcher could rerun the tests whenever a Subway Surfer C# file is saved, or the test command could be wired to a build.
- In this environment, Windows application control blocks the generated test runner, so I cannot currently run the suite automatically or bypass that restriction.
- The current test runner is manual; no automatic watcher or build hook has been configured.

### Prompt 12
Timestamp: 2026-11-12T11:01:46+01:00

User prompt:
> how do i let it run a generated test runner

Assistant answer:
- The error indicates Windows application control, not PowerShell execution policy. The `-ExecutionPolicy Bypass` option does not override that restriction.
- Check Event Viewer at the time of the block under `Microsoft-Windows-CodeIntegrity/Operational` or the AppLocker `EXE and DLL` log to identify which policy blocked it.
- If the device is managed, ask its administrator to approve a trusted test-runner workflow or a properly signed runner. Do not allow all of `%TEMP%` or disable the policy; the runner is regenerated there and a broad path exception would be unsafe.

### Prompt 13
Timestamp: 2026-11-12T11:03:07+01:00

User prompt:
> how do i alloww just this runner to run

Assistant answer:
- If the block is from AppLocker and you administer the device, an administrator can create an Allow rule for the exact runner file hash. Because the runner is regenerated, the hash may need updating on future runs.
- If Code Integrity/WDAC blocked the file, the managed policy administrator must authorize an approved runner.
- A broad allow rule for `%TEMP%` would be unsafe.

### Prompt 14
Timestamp: 2026-11-12T11:13:16+01:00

User prompt:
> there seems to be a bug where space resets the game state, it shouldnt do that

Assistant answer:
- I traced the likely cause to the Start New Game button retaining keyboard focus: Space activates the button, whose handler clears the field.
- Keeping the change within the Subway Surfer folder, I made the game canvas focusable and give it keyboard focus when the game is first painted. That keeps Space from activating the start button while the game is running.
- I did not edit the shared MainWindow files.

### Prompt 16
Timestamp: 2026-11-12T11:24:50+01:00

User prompt:
> can you check the subway surfer code for potential bugs?

Assistant answer:
- I reviewed the rules, field, player, painter, tests, interfaces, and the Gruppe-2 design sheet. I made no game-code changes.
- High-priority finding: `GuardGap` can be reduced to zero, but the tick loop contains no catch/game-over check for zero; it instead starts recovering the gap. This conflicts with the guard-catching behavior documented for the game.
- Design-sheet gaps: `CenterPillar` and a tempo-boost upgrade are declared/planned but are not generated or implemented in the current rules/painter. The existing collision test suite also does not test the guard reaching zero.
- The solution builds with 0 warnings and 0 errors. The test runner was blocked on this attempt by Windows application control.

### Prompt 15
Timestamp: 2026-11-12T11:17:01+01:00

User prompt:
> please create further tests when necessaryy

Assistant answer:
- I added a WPF regression test that checks painting a new Subway Surfer game makes the canvas keyboard-focusable and focused.
- I marked the temporary test runner's entry point as STA so the WPF focus test can run.
- Running the full Subway Surfer suite succeeded with all tests passing.

### Prompt 12
Timestamp: 2026-11-12T11:01:46+01:00

User prompt:
> how do i let it run a generated test runner

Assistant answer:
- The error indicates Windows application control, not PowerShell execution policy. The `-ExecutionPolicy Bypass` option does not override that restriction.
- Check Event Viewer at the time of the block under `Microsoft-Windows-CodeIntegrity/Operational` or the AppLocker `EXE and DLL` log to identify which policy blocked it.
- If the device is managed, ask its administrator to approve a trusted test-runner workflow or a properly signed runner. Do not allow all of `%TEMP%` or disable the policy; the runner is regenerated there and a broad path exception would be unsafe.

### Prompt 13
Timestamp: 2026-11-12T11:03:07+01:00

User prompt:
> how do i alloww just this runner to run

Assistant answer:
- If the block is from AppLocker and you administer the device, an administrator can create an Allow rule for the exact runner file hash. Because the runner is regenerated, the hash may need updating on future runs.
- If Code Integrity/WDAC blocked the file, the managed policy administrator must authorize an approved runner.
- A broad allow rule for `%TEMP%` would be unsafe.

### Prompt 14
Timestamp: 2026-11-12T11:13:16+01:00

User prompt:
> there seems to be a bug where space resets the game state, it shouldnt do that

Assistant answer:
- I traced the likely cause to the Start New Game button retaining keyboard focus: Space activates the button, whose handler clears the field.
- Keeping the change within the Subway Surfer folder, I made the game canvas focusable and give it keyboard focus when the game is first painted. That keeps Space from activating the start button while the game is running.
- I did not edit the shared MainWindow files.

### Prompt 17
Timestamp: 2026-11-19T08:55:26+01:00

User prompt:
> can you create tests for the relevant rules and important optics regarding the subway surfer game? write them into the powershell file so that it can be used as a test file. consider the markdown files in "plans" when you create the tests

Assistant answer:
- I reviewed the Gruppe2 plans and added checks for progression and reset behavior, the jump arc, and the 3D scene and HUD, in addition to the existing rules tests.
- I corrected the safe-lane test to exclude signals, which are boundary hazards rather than lane-blocking obstacles.
- The expanded test suite passed repeatedly.

### Prompt 18
Timestamp: 2026-11-19T09:00:19+01:00

User prompt:
> is it better to have the tests in the cs file?

Assistant answer:
- Yes. Keeping test logic in `Gruppe2_SubwayTests.cs` is clearer and easier to maintain than embedding C# checks in the PowerShell script.
- The PowerShell script can remain as a convenient launcher; a separate test project could be considered later.

### Prompt 19
Timestamp: 2026-11-19T09:01:09+01:00

User prompt:
> does it then make sense to have the tests in the powershell folder?

Assistant answer:
- I understood this as asking whether to keep `RunSubwayTests.ps1` alongside the tests. Keeping the launcher beside the C# test file is useful, but a separate PowerShell folder is unnecessary.
- The test logic should remain in C# and the script should only launch it.

### Prompt 20
Timestamp: 2026-11-19T09:01:37+01:00

User prompt:
> ok can you adjust the changes to reflect this

Assistant answer:
- I moved the added progression, reset, jump-arc, and 3D/HUD checks into `Gruppe2_SubwayTests.cs`.
- `RunSubwayTests.ps1` now only creates a temporary runner and invokes `RunAllTests()`.
- The full suite passed after the move.

### Prompt 21
Timestamp: 2026-11-19T09:03:16+01:00

User prompt:
> and now run them please

Assistant answer:
- I ran `RunSubwayTests.ps1`; all Subway Surfer tests passed.

### Prompt 22
Timestamp: 2026-11-19T09:05:53+01:00

User prompt:
> ok. for future changes please run the tests to prevent recession and write new tests where required. please document this conversation (prompts + answers) in the markdown file lani_sorensen_prompts_subwaysurfer that already exists in the subway surfer folder

Assistant answer:
- I appended this exchange to this conversation log.
- For future Subway Surfer changes, run `RunSubwayTests.ps1` and add focused tests when behavior changes or a regression needs coverage.

### Prompt 23
Timestamp: 2026-11-19T09:08:10+01:00

User prompt:
> in the game currently the character is very static. can you add some more detail in the style of the game graphics and make them ride a skateboard?

Assistant answer:
- I updated the 3D runner with a skateboard, riding pose, brighter clothing and face details, and subtle motion tied to run distance.
- I added a regression check for the skateboard model and the skater's movement.
- The Subway Surfer test suite passed.
