# Tower Defense Spritesheets

Alle Grafiken liegen in `Classes/10_Gruppe/Tower Defense/Assets` (also in unserem
Gruppenordner, ohne Eintrag in `OOPGames.csproj`). `B5_TowerDefenseSprites.cs` laedt
sie zur Laufzeit direkt aus diesem Ordner; den Ordner findet es ueber den Pfad der
eigenen Quelldatei (`CallerFilePath`). Neue PNG-Dateien in diesem Ordner sind dadurch
automatisch verfuegbar. Das Spiel muss dafuer aus dem Repository gebaut werden (wie
ueblich); eine kopierte exe ohne den Quellordner findet die Grafiken nicht.

## Gemeinsames Format

- PNG mit Transparenz (32 Bit RGBA).
- Jedes Sheet hat **4 x 4 Bildpixel pro Spielpixel**. Alle Zellgroessen und Ankerpunkte
  unten sind in Spielpixeln; die Datei ist also viermal so gross. Das Spiel zeichnet mit
  1920 x 1080 (6 Bildschirmpixel pro Spielpixel).
- **Detaillierte Sheets** (Tuerme, Gegner, Bosse, Burg, Deko, Karten) haben die doppelte
  Kunstaufloesung: 1 Spielpixel = 2 x 2 Kunstpixel, 1 Kunstpixel = 2 x 2 Bildpixel.
- **1x-Sheets** (`ui_icons.png`, `terrain_tiles.png`): 1 Spielpixel = 1 Kunstpixel = 4 x 4 Bildpixel.
- Dunkler 1-Kunstpixel-Umriss `#2B2520`, halbtransparente Schatten (`#000000`, Alpha 46-71).
- Stein-Palette: `#5E574C`, `#8A8274`, `#AAA293`, `#CFC7B4`.
- Figuren schauen nach rechts. Fuer Bewegungen nach links spiegelt das Spiel sie
  (`TowerDefensePixelSurface.Sprite(..., flipX: true)`).

## Sheets

| Datei | Zelle | Inhalt | Ankerpunkt |
| --- | --- | --- | --- |
| `tower_spritesheet.png` | 32 x 48, 8 Spalten | Physical Archer, Poison Archer, Fire Mage, Ice Mage, Earth Mage, Strom Mage, Flamethrower, Ice Area Tower | Spot bei (16, 43) |
| `enemy_spritesheet.png` | 16 x 16, 20 Spalten x 2 Zeilen | 20 Standard-Gegner, Zeile 0/1 = Lauf-Frames | Fuesse auf Zeile 13 |
| `boss_spritesheet.png` | 32 x 32, 10 Spalten x 2 Zeilen | 5 Bosse + 5 Mini-Bosse, Zeile 0/1 = Lauf-Frames | Fuesse auf Zeile 28 |
| `main_building.png` | 64 x 48, 3 Spalten | Burg intakt / beschaedigt / zerstoert | Tor-Schwelle bei (32, 44) |
| `ui_icons.png` | 16 x 16, 18 Spalten | Coin, Heart, Wave, Skull, Physical, Fire, Ice, Earth, Storm, Poison, Upgrade, Sell, Cursor, Star, Sword, Range, Speed, Lock | - |
| `map_gruene_wiese.png`, `map_herbstwald.png`, `map_wuestenpass.png`, `map_frostgipfel.png` | 320 x 155 | Fertig zusammengesetzte Karten inkl. Weg, Podesten und Deko | Zeile 0 = Bildschirm-y 25 (Karten-y -2) |
| `terrain_tiles.png` | 16 x 16, 8 Spalten | 4x Gras, 2x Erdweg, 2x Wasser | Baustein fuer Karten |
| `scenery_sprites.png` | 32 x 32 (Teich 64 x 32) | Zeile 0: Eiche, Tanne, Busch, Fels, Kiesel, Blumen, Haus rot, Haus braun. Zeile 1: Teich (2 Zellen), Podest, Zaun, Fass, Kiste, Kaktus, Pilze | Boden auf Zeile 28, Podest-Spot (16, 24) |

Gegner-Reihenfolge (Spalten): Goblin, Goblin Archer, Goblin Shaman, Orc, Orc Berserker,
Orc Warlord, Skeleton, Skeleton Knight, Troll, Stone Golem, Fire Elemental, Ice Elemental,
Storm Elemental, Earth Elemental, Dark Elf, Vampire, Wyvern, Fire Golem, Dragon, Arcane Titan.

Boss-Reihenfolge (Spalten): Skeleton King, Orc Lord, Elemental God, Vampire Lord,
Elder Dragon, Fire Dragon, Skeleton Mage, Dark Elf King, All Elements Golem, Goblin Lord.

Die Namen entsprechen den Klassennamen in `StandardEnemies.cs` und `Bosses.cs`. Im Code
liefert `TowerDefenseSprites.Enemy(enemy.Name, frame)` bzw. `TowerDefenseSprites.Boss(...)`
den passenden Frame.

## Eigene Sheets zeichnen

Ein detailliertes Sheet wird in doppelter Kunstaufloesung gezeichnet (ein Turm also 64 x 96
Kunstpixel) und mit 200 % und *Nearest Neighbor* exportiert. Ein 1x-Sheet wird in
Spielpixel-Groesse gezeichnet und mit 400 % exportiert. Solange Zellgroesse und Reihenfolge
gleich bleiben, ist keine Code-Aenderung noetig.

## Generator

`Generate-TowerDefenseSprites.ps1` erzeugt alle Sheets neu (PowerShell mit eingebettetem C#,
keine zusaetzliche Installation noetig):

```powershell
powershell -ExecutionPolicy Bypass -File ".\Generate-TowerDefenseSprites.ps1"
# mit vergroesserten Vorschaubildern:
powershell -ExecutionPolicy Bypass -File ".\Generate-TowerDefenseSprites.ps1" -PreviewDir "$env:TEMP\td-preview"
```

Alle detaillierten Sheets werden in der Klasse `Art` direkt in doppelter Aufloesung gezeichnet,
im Stil von einfacher Chibi-Pixel-Art: grosse Koepfe mit gut lesbaren Gesichtern, klare Formen,
ein duenner dunkler Umriss, dunkle Trennlinien zwischen Koerperteilen und je Form eine
Schattenseite rechts/unten plus eine helle Oberkante (`D.Fill`). Figuren entstehen aus dem
Baukasten `Chibi` (Kopf, Koerper, Arme, Beine, Augen, Mund, Ohren) plus Ausruestung wie
`Sword`, `Axe`, `Bow`, `Staff`, `Hood`, `WizardHat`, `Helmet`, `Crown`. Tuerme (`Art.Tower`),
Burg (`Art.Castle`), Deko je Karten-Theme (`Art.Scenery`) und der Boden (`Art.Terrain`) sind
im selben Stil gezeichnet.

**Achtung:** Der Generator ueberschreibt von Hand bearbeitete Sheets in `Tower Defense/Assets`.

## Karten

Die Karten sind in `Maps.All` im Generator definiert und muessen mit `TowerDefenseMaps`
in `Kinderzimmer90er/B5_TowerDefense.cs` uebereinstimmen (Weg, Podeste, Burgposition).
Jede Karte hat ein Theme (`meadow`, `autumn`, `desert`, `snow`), das Boden-, Weg- und
Deko-Farben umfaerbt und festlegt, welche Deko wie oft vorkommt. Die Deko wird mit festem
Zufalls-Seed automatisch so verteilt, dass sie weder Weg, Podeste, Tuerme noch die Burg
verdeckt.

Vor dem Zeichnen prueft der Generator jede Karte und bricht mit einer Fehlermeldung ab,
wenn ein Podest den Weg beruehrt, mit der Burg oder einem anderen Podest ueberlappt, ein
Turm das Podest dahinter verdeckt oder ein Turm vor einem waagerechten Wegstueck steht und
die Gegner dahinter verdecken wuerde (er muss mindestens 34 px unter dem Weg stehen).
Neue Karte: Eintrag in `Maps.All` ergaenzen, Skript ausfuehren, `TowerDefenseMapDefinition`
mit denselben Werten anlegen.

Die Podeste sind so gesetzt, dass jedes mit Reichweite 4 (52 px) mindestens 60 px Weg
erreicht (Frostgipfel: 50 px). Von Karte zu Karte wird der Weg kuerzer, es gibt weniger
Podeste und sie decken zusammen weniger Weg ab, so wird jede Karte etwas schwerer:

| Nr. | Karte | Weg | Podeste | Wegabdeckung gesamt (52 px) | Burg (Tor) |
| --- | --- | --- | --- | --- | --- |
| 1 | Gruene Wiese | langer Schlangenweg, ca. 724 px | 10 | 1496 | (286, 70) |
| 2 | Herbstwald | ca. 420 px | 10 | 1075 | (280, 60) |
| 3 | Wuestenpass | ca. 312 px | 9 | 894 | (272, 60) |
| 4 | Frostgipfel | kurzer Bergpfad, ca. 280 px | 8 | 758 | (196, 80) |
