# Pac-Man (Atari-Kassette im Kinderzimmer)

Pac-Man läuft als Minispiel `G10_PacManGame` (`IB5RoomMiniGame`) auf der Atari-Konsole des
Kinderzimmers. Plan und Entwurfsentscheidungen:
`Plans/2026-11-19_Johannes_Lacher_PacMan.md` (englisch: `..._PacMan_EN.md`).

## Steuerung

- Pfeiltasten oder WASD: Richtung. Pac-Man merkt sich die Richtung und biegt an der nächsten
  möglichen Stelle ab.
- Enter, Z, Leertaste oder P: Pause / weiter; nach GAME OVER neues Spiel.
- M: Ton an / aus.
- Esc oder X: zurück zur Geräteansicht (das Spiel bleibt erhalten).

## Aufbau

| Schicht | Klassen |
|---|---|
| Adapter | `G10_PacManGame` (Tasten → `G10_PacManInput`, Highscore speichern) |
| Ansicht | `G10_PacManRenderer` (eigene `WriteableBitmap`, liest nur `IG10_PacManView`), `G10_PacManSprites`, `G10_PixelFont` |
| Spielablauf | `G10_PacManSession`, Phasen `Ready`/`Playing`/`Paused`/`Dying`/`LevelCleared`/`GameOver` (State) |
| Labyrinth | `G10_MazeLayout` (Textdaten), `G10_Maze` (Raster mit `G10_Dot`/`G10_PowerPellet`) |
| Figuren | `G10_MovingEntity` → `G10_PacMan`, `G10_Ghost` |
| Geister | Strategien `G10_DirectChase`, `G10_Ambush`, `G10_Flank`, `G10_Shy`; Modi `InHouse`, `LeavingHouse`, `Scatter`, `Chase`, `Frightened`, `Eaten` (State); `G10_GhostContext`, `G10_GhostHouse`, `G10_ModeScheduler` |
| Daten | `G10_LevelTable` / `G10_LevelSettings`, `G10_ScoreKeeper`, `G10_BonusFruit` |
| Ton | `G10_PacManAudio` (Meldungen `G10_SoundCue` → Sounds, Hintergrundschleife), `IG10_SoundOutput` / `G10_MediaSoundOutput`, `G10_SoundSynth` (Klänge im Code erzeugt) |

Das Modell hängt nicht von WPF ab, kennt keine Tasten und zeichnet nicht. Ein Tick dauert 40 ms
(25 Hz); Positionen sind ganze Zahlen in Subpixeln, das Spiel ist bei gleichem Seed reproduzierbar.
Alle Grafiken sind eigene Pixelmuster im Code, alle Sounds werden im Code erzeugt (eigene
Entwürfe, keine Original-Sprites oder -Sounds). Die Sounds liegen zur Wiedergabe als WAV-Dateien
unter `%LOCALAPPDATA%\Programmieren3WS2026\Kinderzimmer90er\PacManSounds`.
