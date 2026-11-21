# Tower Defense

Hier werden die Programmdateien für das Tower-Defense-Spiel der Commodore-Konsole abgelegt.

Die Klasse `Tower` bildet die gemeinsame Grundlage für verschiedene Tower-Typen.

Das Spiel ist im Commodore-Menue als `Tower Defense` auswaehlbar. Das eigentliche
Spielfeld und die Spieloberflaeche werden in einem spaeteren Schritt ergaenzt.
Das Modul zeigt bereits eine Informationsleiste mit Welle, erwarteten Gegnern,
Gold, Schwierigkeit, Kartenname, Hauptgebaeude-Lebenspunkten und Status. Die
angezeigten Werte sind bis zur Implementierung der Spiel- und Wellenlogik
Platzhalter.

Der Tower-Defense-Hauptbildschirm fuehrt ueber `NEUES SPIEL` zuerst zur
Schwierigkeitsauswahl und danach zur Kartenauswahl. Verfuegbar sind `Easy`,
`Normal`, `Hard` und `God` mit den Multiplikatoren `0.75`, `1.00`, `1.35` und
`1.80` fuer spaetere Gegner-Basiswerte. Die erste Karte `Gruene Wiese` besitzt
einen festen Gegnerpfad und fuenf Stein-Podeste als vorgesehene Tower-Spots.

`Enemy` beschreibt Gegner mit Bewegung, Leben, Rüstung, Schaden am Hauptgebäude,
Schadensresistenzen, Schwächen und einer Belohnung. Eine passende Schadensart aus
`WeakAgainst` verursacht 50 Prozent zusätzlichen Schaden. `MainBuilding` erbt die Angriffswerte von
`Tower` und besitzt zusätzlich Lebenspunkte. Das Spiel kann enden, sobald
`MainBuilding.IsDestroyed` den Wert `true` liefert.

Die verfügbaren Standard-Tower sind `PhysicalArcher`, `FireMage`, `IceMage`,
`EarthMage`, `PoisonArcher`, `StromMage`, `Flamethrower` und `IceAreaTower`. Das
`MainBuilding` verursacht immer physischen Einzelziel-Schaden.

Eigene Spielideen können in [TowerDefensePlan.md](./TowerDefensePlan.md)
gesammelt und weiterentwickelt werden.

`FireMage` und `Flamethrower` wenden Burn an, `PoisonArcher` verursacht
Schaden über Zeit, Ice-Tower verlangsamen Gegner und `EarthMage` verursacht
Rüstungsbruch. Der `StromMage` verursacht Storm-Schaden und kann vom zuerst
getroffenen Gegner aus in beide Richtungen bis zu sechs Gegner verketten. Dafür wird
`GetChainTargets(enemies, firstTargetIndex)` verwendet.

`MiniBoss` und `Boss` erben von `Enemy` und besitzen dadurch alle Eigenschaften
und das Verhalten eines normalen Gegners. Sie können später um spezielle
Boss-Fähigkeiten erweitert werden.

Die 20 Standard-Gegner in `StandardEnemies.cs` erhalten eine Wellenstufe im
Konstruktor, zum Beispiel `new Orc(wave: 5)`. Dadurch steigen ihre Lebenspunkte,
Rüstung, Gebäudeschaden und Belohnung mit jeder Welle.

Zusätzliche Bosse sind `SkeletonKing`, `OrcLord`, `ElementalGod`, `VampireLord`
und `ElderDragon`. Mini-Bosse sind `FireDragon`, `SkeletonMage`, `DarkElfKing`,
`AllElementsGolem` und `GoblinLord`. Der frühere `AncientDragon` heißt jetzt
`Dragon`.

## Grafik

Das Spiel rendert nativ in 1920x1080. Menues, HUD und Anleitung nutzen weiter ein Raster
von 320x180 Einheiten (1 Einheit = 6 Pixel); Sprites und Karten werden 1:1 in ihren
Originalpixeln gezeichnet (Nearest Neighbor, keine Skalierung).

Alle Grafiken liegen in `Tower Defense/Assets` (Sprite-Paket v4, Ordner 1080p, siehe
`LIESMICH_Sprites.txt` und `karten/LIESMICH_Karten.txt`):

| Ordner | Inhalt | Animationen |
| --- | --- | --- |
| `tuerme/` | 8 Tower, 96x160 | `idle` (Schleife), `attack` (bei jedem Schuss, in Schussrichtung gespiegelt) |
| `gegner/` | 20 Gegner, 96-160 px | `walk` (Schleife), `death` (einmalig nach dem Sieg ueber den Gegner) |
| `minibosse/`, `bosse/` | 5 + 5, 192 bzw. 256 px | `walk`, `death` |
| `hauptgebaeude/schloss.png` | Burg, 320x288 | `intakt` > 66 %, `beschaedigt` 33-66 %, `ruine` darunter |
| `effekte/` | Projektile und Flaecheneffekte | Pfeil, Giftpfeil, Feuerball, Eissplitter, Fels (Earth Mage), Blitz (Strom Mage), Flamme (Flamethrower), Frostnova (Ice Area Tower), Arkanbolzen (Burg) |
| `karten/` | 5 Karten je `boden`, `deko`, `bruecken`, `komplett` + JSON | - |
| `ui_icons.png` | HUD- und Menue-Icons | - |

`sprites.json` beschreibt jedes Sheet (Framegroesse, Animationen mit Zeile/Frames/fps/Schleife,
Pivot = Fusspunkt, Schussursprung der Tower) und wird zur Laufzeit gelesen
(`Kinderzimmer90er/B5_TowerDefenseSprites.cs`). Ein Sheet mit gleichem Format laesst sich daher
ohne Code-Aenderung austauschen. Figuren schauen nach rechts und werden fuer die Laufrichtung
links gespiegelt; Treffer lassen sie kurz weiss aufblitzen.

## Karten und Freischaltung

Die fuenf Karten werden aus `Assets/karten/map*.json` geladen (`B5_TowerDefenseMaps.cs`):
Routen (ein oder zwei Spawns), Podeste (Pivot fuer den Tower-Fuss), Burgposition und
Brueckenkreuzungen. Gezeichnet wird in den Ebenen Boden, Deko, Gegner unter einer Bruecke,
Brueckendeck, dann Tower, Burg und uebrige Gegner nach ihrer Hoehe sortiert. Bei zwei Spawns
kommen die Gegner abwechselnd aus beiden, der Boss immer ueber den laengeren Weg.

| Nr. | Karte | Wege | Gegnerstaerke |
| --- | --- | --- |
| 1 | Gruenes Tal | 1 Weg, 46 Kacheln, Steinbruecke | x1,25 |
| 2 | Sumpfwald | 1 Weg, 46 Kacheln, Holzbruecke | x1,45 |
| 3 | Winterpass | 2 Spawns (32 / 20 Kacheln) | x1,2 |
| 4 | Flussschlucht | 2 Spawns (22 / 21 Kacheln) | x0,92 |
| 5 | Verfluchtes Oedland | 2 Spawns (17 / 36 Kacheln), Schleife | x1,38 |

Die 10 Podeste jeder Karte stehen an den vom Spieler markierten Stellen, jeweils direkt neben dem
Weg (93-99 px vom Weg; Baeume oder Steine an diesen Stellen wurden entfernt).
Jedes Podest liegt so nah am Weg, dass jeder Tower ihn schon mit seiner Grundreichweite
erreicht (auch Flamethrower und Ice Area Tower mit 3 x 48 px). Die Reichweite wird von der
Podestmitte am Boden gemessen. Die Gegnerstaerke je Karte (`TowerDefenseMaps.EnemyStrength`)
gleicht die Karten mit dem Simulations-Bot aus, damit sie der Reihe nach schwerer werden.
Am Anfang ist nur die erste
Karte spielbar; gesperrte Karten sind in
der Kartenauswahl mit einem Schloss markiert. Wer alle 50 Wellen einer Karte auf Normal,
Hard oder God besiegt, schaltet die naechste frei. Ein Sieg auf Easy zaehlt nur als Uebung:
die Karte gilt als geschafft, die naechste bleibt aber gesperrt (Hinweis in der Kartenauswahl,
im Schwierigkeitsmenue und in der Anleitung). Der Fortschritt wird pro Spielstand gespeichert
(siehe Abschnitt Spielstaende).

Nach der 50. Welle erscheint das Menue `KARTE ABGESCHLOSSEN` mit Karte und Schwierigkeit, bei
einer neu freigeschalteten Karte zusaetzlich `NEUE KARTE FREIGESCHALTET: <Name>`.
`NAECHSTE KARTE` fuehrt zur Schwierigkeitsauswahl der naechsten Karte (bzw. zur
Weiterspielen-Frage, falls dort ein Zwischenstand liegt), `SCHWIERIGKEIT WAEHLEN` zur
Schwierigkeitsauswahl der gleichen Karte; vorausgewaehlt ist die erste noch nicht geschaffte
Schwierigkeit. Ist die naechste Karte noch gesperrt oder war es die letzte, steht statt
`NAECHSTE KARTE` der Button `KARTENAUSWAHL`. Das Menue liegt ueber der Karte und laesst sich
nicht mit Esc schliessen.

### Endlos-Modus

Der dritte Button `ENDLOS-MODUS` spielt die geschaffte Karte mit Welle 51 weiter, mit allen
Tuermen, Upgrades und dem Gold. Es geht so lange weiter, bis die Burg faellt. Die Wellen nach 50 sind
zufaellig (`TowerDefenseWaves.Endless`): Die Anzahl der Gegner waechst weiter wie in Welle 1-50
(hoechstens 60 Plaetze pro Welle), aber welche Gegner kommen, wird aus allen 20 normalen Gegnertypen
ausgewuerfelt. Jede 5. Welle endet mit einem zufaelligen Mini-Boss, jede 10. mit einem zufaelligen
Boss. Jeder Endlos-Lauf bekommt beim Start einen eigenen Zufallswert (`EndlessSeed`, im Spielstand
gespeichert): Die Wellen-Vorschau zeigt deshalb die echte naechste Welle, und eine vom Speicherpunkt
neu gestartete Welle ist dieselbe, aber jeder neue Endlos-Lauf verlaeuft anders.

Weil die Zufallsmischung auch schwache Gegner wie Goblins enthaelt, haben Endlos-Gegner eine hoehere
Stufe (`EnemyLevel`: normale Stufe x 1,5 + 2). Dadurch ist Welle 51 im Schnitt so stark wie Welle 49
(etwa 88.000 statt 59.000 Lebenspunkte), danach steigt es weiter. Gold gibt es weiter: 30 % der
Gegnerbelohnung plus Wellenbonus; uebriges Gold steckt man in die Meister-Stufen. Das HUD zeigt
`<Welle> ENDLOS`.

Der Endlos-Lauf wird wie ein normaler Lauf nach jeder Welle gespeichert (`TowerDefenseRunState.Endless`)
und laesst sich fortsetzen. Pro Karte merkt sich der Spielstand den Rekord: die hoechste geschaffte
Wellennummer inklusive der ersten 50 (`EndlessRecords`, mindestens 50, sobald der Endlos-Modus
beginnt). Die Kartenauswahl zeigt ihn als `REKORD: BIS WELLE n`, die Niederlage als
`REKORD: BIS WELLE n GESCHAFFT` bzw. `NEUER REKORD: BIS WELLE n GESCHAFFT!`.

### Burg zerstoert

Faellt die Burg (normal oder endlos), erscheint `BURG ZERSTOERT` mit Karte und verlorener Welle,
im Endlos-Modus zusaetzlich Rekord bzw. `NEUER REKORD`. Der Lauf ist damit vorbei, sein
Zwischenstand wird geloescht. Auswahl: `NEUSTARTEN` (gleiche Karte und Schwierigkeit ab Welle 1
mit 500 Gold), `ANDERE KARTE` (Kartenauswahl) oder `HAUPTMENUE`. Nach einem Endlos-Lauf gibt es
zusaetzlich `SCHWIERIGKEIT WAEHLEN` (wie nach Welle 50: Schwierigkeitsauswahl der gleichen Karte,
vorausgewaehlt ist die erste noch nicht geschaffte Schwierigkeit).

Jeder Spielstand merkt sich pro Karte, auf welchen Schwierigkeiten sie geschafft wurde
(`CompletedDifficulties`, Bitmaske). Die Kartenauswahl zeigt das in einer Leiste unten im
Vorschaubild (gruener Haken je Schwierigkeit), die Schwierigkeitsauswahl mit einem Haken pro
Zeile. Sind alle vier geschafft, erscheint oben rechts im Vorschaubild ein Stern. Siege aus
aelteren Versionen sind dort noch nicht erfasst.

Auf der Karte springen die Pfeiltasten zum naechsten Podest in der gedrueckten Richtung.
Das HUD sitzt in einer 48-px-Leiste ueber der Karte (Gold, Leben, Welle, Gegnerzaehler mit
Vorschau, Schwierigkeit/Status) und nutzt doppelt so grosse Schrift und Icons wie zuvor. Die
Buttons `WELLE STARTEN`, `AUTO`, `TEMPO` und `MENUE` liegen unten rechts ueber der Karte; dort ist auf
allen Karten nur Deko. Die Karte beginnt direkt unter der Leiste. Die untersten 24 px (nur
Deko, alle Wege enden darueber) liegen ausserhalb des Bildes.

## Menue und Maussteuerung

`Esc` (oder `X`) schliesst Tower Defense nicht mehr, sondern oeffnet ein Menue mit
`FORTSETZEN`, `HAUPTMENUE` und `SPIEL VERLASSEN`. Im Hauptbildschirm gibt es zusaetzlich den
Eintrag `SPIEL VERLASSEN`. Technisch implementiert das Spiel `IB5RoomOwnBackKey`; der
Kinderzimmer-Kern reicht Esc/X dann an das Spiel weiter, statt es zu beenden.

Alles ist auch mit der Maus bedienbar: Ueberfahren waehlt Menueeintraege, Podeste und Tower
aus, Linksklick bestaetigt (Turm bauen, Upgrade, Verkaufen, Karte starten), Rechtsklick
schliesst das oberste Menue. Ein Klick neben ein Bau- oder Upgrade-Menue schliesst es. In der
Kartenauswahl wechseln die Pfeile `<` `>` und die Fortschrittspunkte die Karte.

## Spielstaende

Nach `SPIELEN` waehlt man einen von drei unabhaengigen Spielstaenden. Jeder Spielstand hat
eigene freigeschaltete Karten und pro angefangener Karte einen Zwischenstand: Welle, Gold,
Leben der Burg, Schwierigkeit sowie Tower und deren Stufe auf jedem Podest. Gegner werden
nicht gespeichert; beim Fortsetzen beginnt die gespeicherte Welle von vorne.

Ablauf: Spielstand -> Karte -> bei angefangener Karte Abfrage `WEITERSPIELEN` /
`NEU BEGINNEN` / `ABBRECHEN` -> bei neuem Start Schwierigkeit -> Karte.

Gespeichert wird nur an Checkpoints: beim Start einer Karte (Anfang Welle 1) und nach jeder
abgeschlossenen Welle. Der Spielstand enthaelt damit Gold, Tower und Upgrades zum Anfang der
naechsten Welle. Was danach gebaut, verbessert oder verkauft wird, geht beim Verlassen der
Karte verloren, bis die Welle geschafft ist. Einer geschafften Karte (ausser im Endlos-Modus) und
nach einer Niederlage wird ihr Zwischenstand
entfernt. Spielstaende lassen sich in der Auswahl mit `Entf` oder dem Button `LOESCHEN` (mit
Rueckfrage) leeren.

Alles liegt in `%LOCALAPPDATA%\Programmieren3WS2026\Kinderzimmer90er\towerdefense_saves.json`
(`B5_TowerDefenseSaves.cs`). Ein Freischaltstand aus der aelteren Datei
`towerdefense_progress.json` wird beim ersten Start in Spielstand 1 uebernommen.

## Wellen und Kampf

Jede Karte hat 50 Wellen (`Waves.cs`). Zwischen den Wellen ist Bauphase; mit `Leertaste`
oder dem Button `WELLE STARTEN` beginnt die naechste. `T` bzw. `TEMPO` schaltet 1x/2x/3x.
`R` bzw. der Button `AUTO AN`/`AUTO AUS` schaltet den Autostart: 2 Spielsekunden nach einer
geschafften Welle startet die naechste von selbst (bei Tempo x2/x3 entsprechend schneller). Der
Start-Button zeigt dann `START IN 2`/`START IN 1` und startet bei Klick sofort. Bau- und
Upgrade-Menue sowie das Esc-Menue halten den Countdown an. Die Einstellung wird pro Spielstand
gespeichert (`AutoStart`).
Waehrend Bau-, Upgrade-, Esc-Menue und Dialogen ist der Kampf angehalten.

- Normale Wellen ziehen Gegner aus Stufen, die mit der Welle staerker werden (Goblins und
  Skelette -> Orks -> Dunkelelfen/Vampire -> Elementare -> Golems/Wyvern -> Drachen/Titanen).
  Starke Gegner belegen mehr Plaetze einer Welle, damit spaete Wellen nicht nur aus Tanks bestehen.
- Wellen 5, 15, 25, 35, 45 enden mit einem Mini-Boss, Wellen 10, 20, 30, 40, 50 mit einem Boss,
  jeweils mit passendem Gefolge: Goblin Lord + Goblins, Orc Lord + Orks, Skeleton Mage +
  Skelette, Vampire Lord + Vampire/Dunkelelfen, Dark Elf King, Elemental God + Elementare,
  All Elements Golem + Golems, Elder Dragon + Wyvern/Drachen, Fire Dragon + Feuergegner und in
  Welle 50 der Skeleton King mit Untoten- und Elite-Gefolge. Der Boss ist immer der letzte Gegner.
- Die Zusammensetzung jeder Welle ist pro Karte fest (Seed aus Karte und Welle), damit eine
  neu gestartete Welle wieder gleich aussieht.
- Klick auf das HUD-Feld `FEINDE`/`NAECHSTE` oder `E` oeffnet ein Fenster mit den Gegnertypen
  der laufenden bzw. naechsten Welle (Icon und Name, ohne Anzahl; Bosse markiert). Ab 9 Gegnertypen
  (Endlos-Modus) wird die erste Spalte mit 8 Typen gefuellt und der Rest steht in einer zweiten
  Spalte rechts daneben; bei mehr als 16 Typen werden die Zeilen etwas enger.
- Der Arcane Titan hat 75 statt 90 Grund-Ruestung, weil er im Endlos-Modus (hohe Stufe) sonst kaum
  noch zu knacken war.

Kampf (`Battle.cs`): Gegner laufen den Kartenweg entlang (`MoveSpeed` x 100 px/s). Tower
schiessen auf den vordersten Gegner in Reichweite (`AttackRange` x 48 px ab Podestmitte). Flaechen-Tower
treffen bis zu `MaxTargets` Gegner in der Naehe, der Strom Mage verkettet ueber
`GetChainTargets`, Effekte (Brand, Gift, Verlangsamung, Ruestungsbruch) kommen aus `Enemy`.
Die Burg (`MainBuilding`, 1000 Leben) schiesst selbst mit und wird nach jeder Welle um 3 %
repariert. Sie hilft nur gegen Gegner in ihrer Naehe: Reichweite 5 x 48 px, ein Arkanbolzen
pro Sekunde mit 30 Grundschaden. Der Schaden waechst pro Welle linear um einen Anteil, der von
der Schwierigkeit abhaengt (`TowerDefenseDifficultyRules.CastleGrowthPerWave`):

| Schwierigkeit | Zuwachs pro Welle | Schaden Welle 1 / 25 / 50 |
| --- | --- | --- |
| Einfach | +5 % | 30 / 66 / 104 |
| Normal | +3,5 % | 30 / 55 / 81 |
| Schwer | +2 % | 30 / 44 / 59 |
| Gott | +1 % | 30 / 37 / 45 |

Weil Gegner auf hoeheren Stufen ohnehin weniger Schaden nehmen, ist die Burg auf Gott deutlich
schwaecher als auf Einfach und bleibt immer schwaecher als ein ausgebauter Tower. Erreicht ein Gegner die Burg, zieht er `DamageOnMainBuilding` ab; bei 0 % ist der Lauf
verloren (siehe Burg zerstoert).

Gold: Jeder besiegte Gegner zahlt einen Anteil seines `Reward` direkt aufs Konto (als `+n` ueber
dem Gegner sichtbar): bis Welle 15 sind es 60 %, danach sinkt der Anteil gleichmaessig auf 30 % in
Welle 50 und bleibt dann dort (`TowerDefenseBattle.RewardFactorAt`, auch fuer einen spaeteren
Endlos-Modus). Jede geschaffte Welle gibt zusaetzlich 20 + 5 x Welle Gold (Boss-Wellen doppelt).
Verkaufen erstattet die Haelfte aller Ausgaben fuer den Tower.

Auf Karte 1 bringen alle 50 Wellen zusammen etwa 83.000 Gold (vorher 120.000). Zehn voll
ausgebaute Tower kosten etwa 55.000 Gold; alles ist also erst um Welle 42-45 gekauft, der Rest
fliesst in die Meisterschaft (bis Welle 50 etwa drei Meister-Stufen pro Tower).

## Upgrades

Jeder Tower hat vier unabhaengige Upgrade-Wege (`TowerUpgrades.cs`). Die Prozentwerte beziehen
sich auf die Basiswerte des Towers; die Turmstufe erhoeht diese Basiswerte selbst, die
Prozent-Upgrades wirken also darauf.

| Upgrade | pro Stufe | Stufen | Kosten der naechsten Stufe |
| --- | --- | --- |
| Schaden (auch Brand/Gift) | +30 % | 8 | 40 % Baupreis x Stufe |
| Reichweite | +10 % | 5 | 35 % Baupreis x Stufe |
| Tempo | +15 % | 6 | 40 % Baupreis x Stufe |
| Turmstufe II / III | Basis x1,5 / x2,25 Schaden, x1,15 / x1,3 Tempo, x1,1 / x1,2 Reichweite | 3 | 2x / 4x Baupreis |
| Meister (erst wenn alles oben gekauft ist) | +10 % Schaden, +5 % Tempo (multiplikativ, Reichweite bleibt) | ohne Limit | 3x Baupreis, jede weitere Stufe x1,6 |

Ein voll ausgebauter Tower kostet damit etwa das 35-fache seines Baupreises (vorher 26-fach).
Die Meisterschaft ist das Ziel fuer uebriges Gold: In `TURM VERWALTEN` wird die Zeile
`TURMSTUFE` zu `MEISTER n`, sobald Schaden, Reichweite, Tempo und Turmstufe voll sind
(vorher steht dort `MEISTER NACH ALLEN UPGRADES`).

Im Menue `TURM VERWALTEN` zeigt die Werte-Zeile den Effekt des ausgewaehlten Upgrades in Gruen;
das Menue bleibt nach einem Kauf offen. Auf der Karte zeigen kleine Sterne auf dem Podest die
Turmstufe (2 Silber = II, 3 Gold = III, 3 Lila = Meister; immer innerhalb der Turmbreite) und
drei Balken unter dem Podest die Stufen von Schaden (rot), Reichweite (gruen) und Tempo (gelb).
Gespeichert wird pro Podest `TowerTiers`, `DamageLevels`, `RangeLevels`, `SpeedLevels` und
`MasteryLevels`; aeltere Spielstaende mit nur einer Stufe werden als Schaden-Upgrades uebernommen.

Schadensmodell (`Enemy.TakeDamage`): Schwaeche +50 %, Resistenz -50 % (nicht mehr immun),
Ruestung reduziert prozentual um `150 / (150 + Ruestung)`. Brand und Gift zaehlen als Feuer-
bzw. Giftschaden, ignorieren Ruestung und sammeln Bruchteile ueber die Ticks. Die
Schwierigkeit teilt den eingehenden Schaden (`IncomingDamageFactor`), wirkt also wie mehr Leben.

Die Werte wurden mit einem Simulations-Bot abgestimmt, der Tower reihum baut und Gold jeweils
in das Upgrade mit dem besten Nutzen pro Gold steckt (wie ein guter Spieler, aber ohne auf die
Schwaechen der Gegner zu achten, Meister-Stufen eingeschlossen). Ergebnis mit den aktuellen Werten:

| Karte | Easy | Normal | Hard | God |
| --- | --- | --- | --- | --- |
| Gruenes Tal | gewonnen | gewonnen | gewonnen | verloren Welle 49 |
| Sumpfwald | gewonnen | gewonnen | gewonnen | verloren Welle 40 |
| Winterpass | gewonnen | gewonnen | knapp gewonnen | verloren Welle 49 |
| Flussschlucht | gewonnen | knapp gewonnen | verloren Welle 40 | verloren Welle 38 |
| Verfluchtes Oedland | gewonnen | verloren Welle 46 | verloren Welle 40 | verloren Welle 20 |

Wo der Bot verliert, ist gezielter Einsatz der Schadensarten noetig.

## Musik

Jede Karte bekommt einen eigenen Soundtrack im 16-Bit-Retro-Stil, dazu Menue-, Mini-Boss- und
Boss-Musik. Die Musik wird im Code komponiert und synthetisiert, es gibt keine fremden
Audiodateien:

- `Kinderzimmer90er/B5_TowerDefenseMusicSynth.cs`: Synthesizer (Pulswellen mit waehlbarer
  Breite, Dreieck, Sinus, Saegezahn, Orgel, Glocke, Rausch-Schlagzeug, ADSR-Huellkurve, Vibrato,
  Tiefpass, Echo) und eine kleine Tracker-Notation (`g4:8` = G4 als Halbe, `c4+e4+g4` = Akkord,
  `r` = Pause). `TowerDefenseChords` erzeugt Arpeggios, Basslinien und Akkordflaechen aus
  Akkordsymbolen. Toene und Echos, die ueber das Loop-Ende hinausklingen, laufen am Anfang weiter,
  der Loop hat also keine Naht.
- `Kinderzimmer90er/B5_TowerDefenseSongs.cs`: die Kompositionen.
- `Kinderzimmer90er/B5_TowerDefenseMusic.cs`: Wiedergabe. Die Stuecke werden beim ersten Start im
  Hintergrund gerendert und als WAV nach `%LOCALAPPDATA%\Programmieren3WS2026\Kinderzimmer90er\TowerDefenseMusic`
  geschrieben. Alle Stuecke werden auf die gleiche Lautheit gebracht. Die Lautstaerke betraegt 25 % des globalen Lautstaerkereglers (Hintergrund), im
  Esc-Menue, in der Anleitung und in Dialogen noch einmal 45 % davon. Wechsel sind harte Schnitte.
  Ruft das Spiel kein `Update` mehr auf (Geraet verlassen), pausiert die Musik nach 0,4 s.

Die Stuecke werden von Karte zu Karte epischer (mehr Tempo, mehr Bass, volleres Schlagzeug);
Mini-Boss und Boss sind die schnellsten und schwersten.

| Stueck | Stimmung | Umsetzung |
| --- | --- | --- |
| Menue (Hauptmenue, Kartenauswahl) | einladend, schon leicht heroisch | F-Dur, 90 BPM, 20 Takte (53 s): Floeten-Melodie, gezupfte Begleitung, Streicher, Sub-Bass, leise Trommel
| Gruenes Tal | froehlich-abenteuerlich | G-Dur, 120 BPM, 32 Takte A A2 B A (64 s): Pulswellen-Melodie, Arpeggio mit Echo, Streicher, Dreieck- und Sub-Bass, Schlagzeug mit Becken
| Sumpfwald | mystisch mit Puls | D-Dorisch, 88 BPM, 20 Takte (55 s): Chor-Flaeche, Glocken mit Echo, Floete, Tom-Groove, bewegter Bass
| Winterpass | kalt-glitzernd, treibend | E-Moll mit hellen Dur-Teilen, 104 BPM, 24 Takte (55 s): Glockenspiel-Melodie, Glocken-Arpeggios, Streicher, Achtel-Bass, Schlagzeug
| Flussschlucht | weit-episch | C-Moll/Es-Dur, 100 BPM, 24 Takte (58 s): Horn-Melodie mit Blech-Verdopplung, Streicher, galoppierender Saege-Bass, Pauken und Snare
| Verfluchtes Oedland | Finale | A-Moll, 112 BPM, 24 Takte (51 s): Blech-Melodie in zwei Oktaven, Chor und Orgel, Saege- und Sub-Bass in Achteln, volles Marsch-Schlagzeug
| Mini-Boss | angespannte Schlacht | E-Moll, 144 BPM, 32 Takte (53 s): scharfe Melodie mit Blech-Verdopplung, harter Sechzehntel-Saege-Bass, Streicher-Stoesse, Becken alle 4 Takte
| Boss (inkl. Skeleton King) | gross und bedrohlich | D-Moll mit phrygischem Es, 138 BPM, 32 Takte (56 s): Fanfare in zwei Oktaven, Orgel und Chor, Doppel-Kick, Pauken, schwerster Bass |

Die Boss-Musik startet, sobald der (Mini-)Boss als letzter Gegner der Welle erscheint, und endet,
wenn er besiegt ist.

## Anleitung im Spiel

Ueber `ANLEITUNG` im Esc-Menue (und im Hauptmenue) oeffnet sich eine bebilderte Anleitung mit
acht Seiten (`Kinderzimmer90er/B5_TowerDefenseHelp.cs`): Spielziel, Spieloberflaeche,
Tuerme bauen, Upgrades, Gegner und Wellen, Schadensarten, Steuerung (Tastatur und Maus) und
Spielstaende. Die Bilder werden aus den echten Kartenausschnitten und Sprites zusammengesetzt;
nummerierte Markierungen verbinden Bildstellen mit dem Text. Blaettern mit Pfeiltasten, Enter
oder den Buttons `< ZURUECK` / `WEITER >` bzw. den Seitenpunkten; `Esc`, `Backspace`,
Rechtsklick oder `X` schliessen sie wieder (zurueck ins Esc-Menue). Waehrend die Anleitung
offen ist, ist das Spiel pausiert.
