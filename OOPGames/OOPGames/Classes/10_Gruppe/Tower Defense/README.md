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
