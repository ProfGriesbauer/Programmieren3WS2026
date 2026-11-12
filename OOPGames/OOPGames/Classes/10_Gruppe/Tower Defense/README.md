# Tower Defense

Hier werden die Programmdateien für das Tower-Defense-Spiel der Commodore-Konsole abgelegt.

Die Klasse `Tower` bildet die gemeinsame Grundlage für verschiedene Tower-Typen.

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
