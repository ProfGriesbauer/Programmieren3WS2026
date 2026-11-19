# Sounds – Musik und Soundeffekte

Quelle: Sound-Paket „CasinoAdventureSounds“ (von Patrick Lupierz bereitgestellt, frei nutzbar). Alle Sounds sind
im Spiel eingebaut (Katalog: `Assets/SoundMixer/Patrick_Lupierz_SoundCatalog.cs`). Lautstärke, Häufigkeit und wie
viele Töne gleichzeitig laufen dürfen, stellst du mit dem **Handy-Mixer** ein (siehe `Assets/SoundMixer/README.md`).

## Ordner

| Ordner | Inhalt | Wird gespielt |
|--------|--------|---------------|
| `Musik_Aussen/` | 5 Ambiente-Aufnahmen „Ambient Outside 1/2/4/5“, „Ambient Oustide“ | Stadt (draußen), im Wechsel |
| `Musik_Gebaeude/` | `CasinoBackgroundLoop`, 4× „Chippy music“, „Ambient Charakter“ | Casino · Spielhalle (4 Stücke im Wechsel) · Titelbild und Charaktererstellung |
| `Soundeffekte/Schritte/` | StepGrass, StepsPath, StepBridge, Steps_Indoor (je 3 Varianten) | je Schritt, Untergrund nach Kachel; Varianten reihum |
| `Soundeffekte/Tiere/` | catMeow1/2, dogBark, dog_bark_01, puppy_bark_01, songbird_chirp_01, frog_croak_01 | Katze/Hund nur beim Streicheln · Vogel nur im Radius (4 Kacheln) · Frosch liegt bereit (noch kein Frosch im Spiel) |
| `Soundeffekte/Casino/` | Münzen (3/4/5), ChipUp, GeneralVictory, jackpot, JackpotAlarm, PokerWinSound, shuffle_riffle, SlotsSpinSoundLoop, slot_lever, slot_win_big/small, ui_bet_up | Spielautomat, Roulette, Blackjack, Poker, Siege |
| `Soundeffekte/Allgemein/` | BicycleSoundLoop, EffectGainTypeAny, OpenFurnitureDesk, OpenTrashCan, purchase_coin_07 | Fahrrad (Schleife) · Gegenstand benutzt · Einrichtungsplan · Mülleimer · Einkauf bezahlt |

Home-, Laden- und Angelmusik gibt es im Paket nicht: Dafür bleibt die eingebaute Melodie. Eigene Dateien einfach als
`Home.mp3`, `Shop.mp3` (in `Musik_Gebaeude/`) bzw. `Fishing.mp3` (in `Musik_Aussen/`) ablegen.

## Umgewandelt (und warum)

WPF kann **OGG** ohne Zusatzbibliothek nicht abspielen, und das Paket ist mit 291 MB zu groß fürs Repository. Deshalb:

- Lange Aufnahmen (Ambiente, Arcade-Musik) → **MP3** (96 bzw. 128 kbps), zusammen ca. 25 MB
- Kurze Effekte (OGG/MP3/WAV) → **WAV** 16 Bit, **Spitzenpegel auf −3 dBFS** angehoben/abgesenkt. Die Original-Schritte
  auf Gras lagen bei −58 dB und wären unhörbar gewesen; nach dem Angleichen regelt der Mixer die Lautstärke.
- `CasinoBackgroundLoop.mp3` unverändert kopiert.
- „Ambient Oustide 3“ ist byte-gleich mit „Ambient Oustide“ und deshalb nur einmal abgelegt; `slot_win_small_01` gab es als
  OGG und MP3 (gleicher Ton) – es liegt nur eine WAV-Version vor.
- Die Originale bleiben im Download-Ordner des Teams; wer neu umwandeln will, nimmt für Effekte eine WAV (kein MP3, das
  bei Schleifen hörbare Pausen erzeugt).

## Wo sucht das Spiel?

In dieser Reihenfolge: (1) Umgebungsvariable `G1CASINO_SOUNDS`, (2) `G1Casino_Assets\Sounds\` neben der `OOPGames.exe`
(wie bei den Grafiken – kopieren, wenn das Spiel ohne Quellordner läuft), (3) **dieser Ordner im Quellcode**
(`Classes\Gruppe 1\Spiel 2\Assets\Sounds`, von `bin\Debug` aus nach oben gesucht). Beim Entwickeln muss also nichts kopiert
werden. Eine automatische Kopie über die csproj ist nicht vorgesehen, weil die Projektdatei nicht geändert werden darf.

## Neue Dateien

Datei in den passenden Ordner legen (wav oder mp3, **kein ogg**) und im Namen dem Muster des Katalogs folgen. Gehört eine
Datei zu keinem Sound, meldet es der Selbsttest („Dateien ohne Sound im Katalog“).

Lizenz: Nur frei nutzbare Sounds ablegen und die Quelle hier notieren.

Quellen:
- CasinoAdventureSounds (Team-Paket, frei nutzbar)
