# Tower Defense Spritesheets

Alle Grafiken liegen in `OOPGames/Assets/TowerDefense` und werden ueber
`<Resource Include="Assets\TowerDefense\*.png" />` in das Spiel eingebunden. Neue
PNG-Dateien in diesem Ordner sind dadurch automatisch verfuegbar.

## Gemeinsames Format

- PNG mit Transparenz (32 Bit RGBA).
- **1 Kunstpixel = 4 x 4 Bildpixel** (wie `tower_spritesheet.png`). Alle Angaben unten
  sind in Kunstpixeln; die Datei ist also viermal so gross.
- Dunkler 1-Pixel-Umriss `#2B2520`, halbtransparente Schatten (`#000000`, Alpha 46-71).
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
| `map_gruene_wiese.png`, `map_herbstwald.png`, `map_wuestenpass.png`, `map_frostgipfel.png` | 320 x 155 | Fertig zusammengesetzte Karten inkl. Weg, 10 Podesten und Deko | Zeile 0 = Bildschirm-y 25 (Karten-y -2) |
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

Ein Sheet kann in einem Pixel-Art-Programm (z. B. Aseprite, Piskel, LibreSprite) in
Kunstpixel-Groesse gezeichnet und anschliessend mit 400 % und *Nearest Neighbor*
exportiert werden. Solange Zellgroesse und Reihenfolge gleich bleiben, ist keine
Code-Aenderung noetig.

## Generator

`Generate-TowerDefenseSprites.ps1` erzeugt alle Sheets ausser `tower_spritesheet.png`
neu (PowerShell mit eingebettetem C#, keine zusaetzliche Installation noetig):

```powershell
powershell -ExecutionPolicy Bypass -File ".\Generate-TowerDefenseSprites.ps1"
# mit vergroesserten Vorschaubildern:
powershell -ExecutionPolicy Bypass -File ".\Generate-TowerDefenseSprites.ps1" -PreviewDir "$env:TEMP\td-preview"
```

**Achtung:** Der Generator ueberschreibt von Hand bearbeitete Sheets.

## Karten

Die Karten sind in `Maps.All` im Generator definiert und muessen mit `TowerDefenseMaps`
in `Kinderzimmer90er/B5_TowerDefense.cs` uebereinstimmen (Weg, 10 Podeste, Burgposition).
Jede Karte hat ein Theme (`meadow`, `autumn`, `desert`, `snow`), das Boden-, Weg- und
Deko-Farben umfaerbt und festlegt, welche Deko wie oft vorkommt. Die Deko wird mit festem
Zufalls-Seed automatisch so verteilt, dass sie weder Weg, Podeste, Tuerme noch die Burg
verdeckt.

Vor dem Zeichnen prueft der Generator jede Karte und bricht mit einer Fehlermeldung ab,
wenn ein Podest den Weg beruehrt, mit der Burg oder einem anderen Podest ueberlappt oder
ein Turm das Podest dahinter verdeckt. Neue Karte: Eintrag in `Maps.All` ergaenzen,
Skript ausfuehren, `TowerDefenseMapDefinition` mit denselben Werten anlegen.

| Nr. | Karte | Weg | Burg (Tor) |
| --- | --- | --- | --- |
| 1 | Gruene Wiese | langer Schlangenweg, ca. 730 px | (286, 70) |
| 2 | Herbstwald | ca. 430 px | (280, 60) |
| 3 | Wuestenpass | ca. 320 px | (272, 60) |
| 4 | Frostgipfel | ca. 290 px, kurze Schleife | (160, 90) |
