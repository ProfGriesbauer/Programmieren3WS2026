# Sounds – Gruppe 9

All files: WAV, mono, 44.1 kHz, 16-bit. Silence at the start is already trimmed, so play them immediately when the event fires.
Where a group has several numbered variants, play a **random** one each time so it doesn't sound repetitive.

## Player

| File(s) | When to play | How |
|---|---|---|
| `damage_01.wav` – `damage_18.wav` | Player takes damage | One-shot, random variant |
| `player_death.wav` | Player dies ("Emotional Damage", 3.2 s) | One-shot. Stop the sprint loop first |
| `playerFootstep_01.wav` – `playerFootstep_08.wav` | Each footstep while the player **walks** (dirt/gravel, 0.2–0.33 s) | One-shot, random variant (avoid the same one twice in a row). Trigger in sync with the walk animation, or on a timer (~0.35–0.45 s between steps while walking). Not while sprinting (the engine loop replaces them). Optionally vary volume ±10 % |
| `playerHeal.wav` | Player **heals** (picks up / uses a heal) | One-shot (a long, swelling "huuuu", 2.7 s). Don't restart it if it's already playing; if health goes back above the low-health threshold, also reset the low-health warning |
| `playerLowHealth.wav` | Player's health drops **below a low-health threshold** (e.g. 25 %) | One-shot (3.1 s), play once when crossing the threshold – not every frame. Optionally repeat every ~8–10 s while health stays low; reset when the player heals above the threshold or dies |

## Combat

| File(s) | When to play | How |
|---|---|---|
| `sword_swing_01.wav` – `sword_swing_06.wav` | Normal sword attack | One-shot, random variant |
| `sword_swing_light.wav` | Fast / light attack (optional) | One-shot |
| `sword_swing_heavy.wav` | Heavy / charged attack (optional) | One-shot |
| `electricDamage_01.wav` – `electricDamage_04.wav` | Someone takes **electric / shock damage** (player or enemy) | One-shot, random variant (01–03 ≈ 1 s, 04 = short 0.55 s version for rapid or repeated hits). Can play together with the normal damage sound |
| `hit_impact_01.wav` – `hit_impact_06.wav` | An attack hits something | One-shot, random variant. Can play together with a damage sound |

## Enemies / non-player entities

| File(s) | When to play | How |
|---|---|---|
| `entity_death_01.wav` – `entity_death_05.wav` | Enemy / NPC dies | One-shot, random variant (04 = small enemy, 05 = big enemy) |
| `entity_death_06.wav` | Enemy dies while many die at once | One-shot, short version |

## Sprinting (shrill F1 race-car style)

The sprint sounds work as a small state machine:

| File(s) | When to play | How |
|---|---|---|
| `sprint_loop_01.wav` – `sprint_loop_03.wav` | **While** the player is sprinting | Pick one variant at random when the sprint starts and **loop** it seamlessly (1.2–1.6 s each, F1 engine at constant revs) until the sprint ends. Fade it in over ~0.1 s at the start (there is no separate start sound). Only one instance at a time |
| `sprint_stop_brake.wav` | Player **stops** sprinting | Stop the running sprint loop (short ~0.05 s fade), then play this one-shot (shrill brake squeal, 1.2 s) |
| `sprint_turn_neeow_01.wav` – `sprint_turn_neeow_03.wav` | Player **changes direction while sprinting** | One-shot, random variant – the classic "neeeoowww" pass-by (1.2–1.6 s). Keep the sprint loop running underneath (optionally duck it a bit). Add a cooldown (~0.4 s) so rapid direction changes don't stack |

Sequence: `idle → sprint_loop_XX (looping) → [sprint_stop_brake] → idle`, with `sprint_turn_neeow_XX` on top of the loop when the direction changes.

## UI

| File(s) | When to play | How |
|---|---|---|
| `ui_menu_click.wav` | Clicking a button / menu entry | One-shot |

## Notes for implementation

- Several one-shots can overlap (e.g. `hit_impact` + `damage`), so use a player that supports overlapping playback (e.g. a new `MediaPlayer` per one-shot in WPF, or a small pool), and a separate, dedicated player for `sprint_loop`.
- Set every WAV to **Copy to Output Directory: Copy if newer** (or `<Content ... CopyToOutputDirectory="PreserveNewest" />` in the .csproj) so the files are found at runtime.
- Obsolete files that can be deleted: `hit_impact.wav` (without a number) and `sprint_turn_skid_01.wav` – `sprint_turn_skid_03.wav` (replaced by `sprint_turn_neeow_XX`).
