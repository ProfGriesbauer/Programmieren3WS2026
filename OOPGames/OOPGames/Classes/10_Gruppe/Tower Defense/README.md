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

Alle Grafiken kommen aus Spritesheets in `Assets/TowerDefense` im Stil des
Tower-Spritesheets (Pixel-Art, 4-fach skaliert, dunkler Umriss). Die Karte
`Gruene Wiese` ist ein vorgerendertes Hintergrundbild mit Weg, Pflasterstein-Podesten
und Deko; Tower und Hauptgebaeude (intakt, beschaedigt, zerstoert je nach Leben) werden
darueber gezeichnet. Fuer alle 20 Gegner sowie 5 Bosse und 5 Mini-Bosse gibt es
Sprites mit zwei Lauf-Frames, die ueber den Gegnernamen abgerufen werden. Layouts,
Ankerpunkte und der Sprite-Generator sind in
[SpriteGenerator/README.md](./SpriteGenerator/README.md) beschrieben.

## Karten und Freischaltung

Es gibt vier Karten mit je zehn Podesten: `Gruene Wiese`, `Herbstwald`, `Wuestenpass`
und `Frostgipfel`. Der Weg wird von Karte zu Karte kuerzer, die Gegner erreichen die
Burg also schneller. Am Anfang ist nur die erste Karte spielbar; gesperrte Karten sind in
der Kartenauswahl mit einem Schloss markiert. Wer alle 50 Wellen einer Karte besiegt,
schaltet die naechste frei. Der Fortschritt wird pro Spielstand gespeichert
(siehe Abschnitt Spielstaende).

Auf der Karte springen die Pfeiltasten zum naechsten Podest in der gedrueckten Richtung.

## Menue und Maussteuerung

`Esc` (oder `X`) schliesst Tower Defense nicht mehr, sondern oeffnet ein Menue mit
`FORTSETZEN`, `HAUPTMENUE` und `SPIEL VERLASSEN`. Im Hauptbildschirm gibt es zusaetzlich den
Eintrag `SPIEL VERLASSEN`. Technisch implementiert das Spiel `IB5RoomOwnBackKey`; der
Kinderzimmer-Kern reicht Esc/X dann an das Spiel weiter, statt es zu beenden.

Alles ist auch mit der Maus bedienbar: Ueberfahren waehlt Menueeintraege, Podeste und Tower
aus, Linksklick bestaetigt (Turm bauen, Upgrade, Verkaufen, Karte starten), Rechtsklick
schliesst das oberste Menue. Ein Klick neben ein Bau- oder Upgrade-Menue schliesst es. In der
Fusszeile gibt es die Buttons `MENUE` und `ZURUECK`, in der Kartenauswahl die Pfeile `<` `>`
und die Fortschrittspunkte zum Wechseln der Karte.

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
Karte verloren, bis die Welle geschafft ist. Einer geschafften Karte wird ihr Zwischenstand
entfernt. Spielstaende lassen sich in der Auswahl mit `Entf` oder dem Button `LOESCHEN` (mit
Rueckfrage) leeren.

Alles liegt in `%LOCALAPPDATA%\Programmieren3WS2026\Kinderzimmer90er\towerdefense_saves.json`
(`B5_TowerDefenseSaves.cs`). Ein Freischaltstand aus der aelteren Datei
`towerdefense_progress.json` wird beim ersten Start in Spielstand 1 uebernommen.

## Wellen und Kampf

Jede Karte hat 50 Wellen (`Waves.cs`). Zwischen den Wellen ist Bauphase; mit `Leertaste`
oder dem Button `WELLE STARTEN` beginnt die naechste. `T` bzw. `TEMPO` schaltet 1x/2x/3x.
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
  der laufenden bzw. naechsten Welle (Icon und Name, ohne Anzahl; Bosse markiert).

Kampf (`Battle.cs`): Gegner laufen den Kartenweg entlang (`MoveSpeed` x 17 px/s). Tower
schiessen auf den vordersten Gegner in Reichweite (`AttackRange` x 13 px). Flaechen-Tower
treffen bis zu `MaxTargets` Gegner in der Naehe, der Strom Mage verkettet ueber
`GetChainTargets`, Effekte (Brand, Gift, Verlangsamung, Ruestungsbruch) kommen aus `Enemy`.
Die Burg (`MainBuilding`, 1000 Leben) schiesst selbst mit und wird nach jeder Welle um 3 %
repariert. Erreicht ein Gegner die Burg, zieht er `DamageOnMainBuilding` ab; bei 0 % ist die
Welle verloren und kann ab dem letzten Checkpoint neu gestartet werden.

Gold: Jeder besiegte Gegner gibt 60 % seines `Reward` direkt aufs Konto (als `+n` ueber dem
Gegner sichtbar), jede geschaffte Welle zusaetzlich 20 + 5 x Welle Gold (Boss-Wellen doppelt).
Verkaufen erstattet die Haelfte aller Ausgaben fuer den Tower.

## Upgrades

Jeder Tower hat vier unabhaengige Upgrade-Wege (`TowerUpgrades.cs`). Die Prozentwerte beziehen
sich auf die Basiswerte des Towers; die Turmstufe erhoeht diese Basiswerte selbst, die
Prozent-Upgrades wirken also darauf.

| Upgrade | pro Stufe | Stufen | Kosten der naechsten Stufe |
| --- | --- | --- | --- |
| Schaden (auch Brand/Gift) | +25 % | 8 | 30 % Baupreis x Stufe |
| Reichweite | +8 % | 5 | 25 % Baupreis x Stufe |
| Tempo | +12 % | 6 | 30 % Baupreis x Stufe |
| Turmstufe II / III | Basis x1,5 / x2,25 Schaden, x1,15 / x1,3 Tempo, x1,1 / x1,2 Reichweite | 3 | 1,5x / 3x Baupreis |

Im Menue `TURM VERWALTEN` zeigt die Werte-Zeile den Effekt des ausgewaehlten Upgrades in Gruen;
das Menue bleibt nach einem Kauf offen. Auf der Karte zeigt ein Wappen die Turmstufe (Silber II,
Gold III) und drei Balken unter dem Podest die Stufen von Schaden (rot), Reichweite (gruen) und
Tempo (gelb). Gespeichert wird pro Podest `TowerTiers`, `DamageLevels`, `RangeLevels` und
`SpeedLevels`; aeltere Spielstaende mit nur einer Stufe werden als Schaden-Upgrades uebernommen.

Schadensmodell (`Enemy.TakeDamage`): Schwaeche +50 %, Resistenz -50 % (nicht mehr immun),
Ruestung reduziert prozentual um `150 / (150 + Ruestung)`. Brand und Gift zaehlen als Feuer-
bzw. Giftschaden, ignorieren Ruestung und sammeln Bruchteile ueber die Ticks. Die
Schwierigkeit teilt den eingehenden Schaden (`IncomingDamageFactor`), wirkt also wie mehr Leben.

Die Werte wurden mit einem Simulations-Bot abgestimmt, der Tower reihum baut und Gold jeweils
in das Upgrade mit dem besten Nutzen pro Gold steckt (wie ein guter Spieler, aber ohne auf die
Schwaechen der Gegner zu achten). Er gewinnt alle Karten auf Easy und Normal, Karte 1 auch auf
God und Karte 2 auf Hard. Karte 3 und 4 auf Hard sowie Karte 2-4 auf God verliert er in den
spaeten Wellen (Karte 4 auf God schon um Welle 20); dort ist gezielter Einsatz der
Schadensarten noetig.

## Anleitung im Spiel

Ueber `ANLEITUNG` im Esc-Menue (und im Hauptmenue) oeffnet sich eine bebilderte Anleitung mit
acht Seiten (`Kinderzimmer90er/B5_TowerDefenseHelp.cs`): Spielziel, Spieloberflaeche,
Tuerme bauen, Upgrades, Gegner und Wellen, Schadensarten, Steuerung (Tastatur und Maus) und
Spielstaende. Die Bilder werden aus den echten Kartenausschnitten und Sprites zusammengesetzt;
nummerierte Markierungen verbinden Bildstellen mit dem Text. Blaettern mit Pfeiltasten, Enter
oder den Buttons `< ZURUECK` / `WEITER >` bzw. den Seitenpunkten; `Esc`, `Backspace`,
Rechtsklick oder `X` schliessen sie wieder (zurueck ins Esc-Menue). Waehrend die Anleitung
offen ist, ist das Spiel pausiert.
