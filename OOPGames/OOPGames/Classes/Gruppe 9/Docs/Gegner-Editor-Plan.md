# Plan: Gegner-Editor (Dev Tool)

Stand: 25.11.2026 · Autor: Till Markhof (mit Claude) · Status: **Phase 1–4 umgesetzt** (siehe Abschnitte 10–13)

Ziel: Neue Gegnerarten schnell im Dev Tool anlegen und dabei Werte, Waffen, Verhalten,
Spawning und Aussehen einstellen. **Jedes Feld ist optional.** Ein leeres Feld übernimmt den
Wert des heutigen Standardgegners, eine neue Art funktioniert also schon, wenn nur ein Name
eingetragen ist.

---

## 1. Ausgangslage (heutiger Code)

Heute gibt es eine einzige Gegnerart, deren Werte fest im Code stehen:

| Bereich | Heute | Datei |
|---|---|---|
| Lebenspunkte, Tempo | 30 LP, 75 % des Spielertempos | `PursuerEnemy.cs` |
| Hitbox | 34, aus dem Sprite gemessen, schiebbar | `PursuerEnemy.cs`, `EntityHitboxCatalog.cs` |
| Sicht | 200, nach Entdeckung +50 | `PursuerEnemy.cs` |
| Alarm nach Treffer | 20 s, Radius 100, Kettenreaktion, Tote warnen nicht | `PursuerEnemy.cs`, `ArenaField.cs` |
| Streunen | halbes Tempo, Richtungswechsel alle 1,5–4 s | `PursuerEnemy.cs` |
| Waffe | `EnemyClaws`: Kegel 60 / 45°, 10 Schaden, 1 s, Rückstoß 16 | `Combat/WeaponCatalog.cs` |
| Spawning | ein Gegner alle 7 s, höchstens 15, mindestens 250 vom Spieler entfernt | `ArenaField.cs` |
| Aussehen | `Assets/Enemies/Enemie1.png` + `.json` | `PainterG9.cs` |
| Spielstand | Gegner ohne Art (Position, LP, Status, Alarm) | `SaveGames/ArenaSaveGame.cs` |

Das Kampfsystem ist schon baukastenartig aufgebaut. Darauf baut der Editor auf:

- **Angriffsmuster:** Kegel, Bogenhieb, Drehwirbel, Projektil, Puls um sich selbst, Welle
- **Effekte:** Schaden, Gift, Feuer, Frost, Betäubung, Verlangsamung, Schwäche, Lebensraub, beliebig kombinierbar

## 2. Getroffene Entscheidungen

| Thema | Entscheidung |
|---|---|
| Waffen | Baukasten (Muster + Werte + Effekte), mehrere Angriffe pro Gegner. Außerdem kann man eine **fertige Waffe** wählen (Klauen, Schwert, Zauberstab, Bogen) und einzelne Werte überschreiben. |
| Verhalten | Grundtyp wählen, alle Zahlen frei einstellbar |
| Spawning | **eigener Takt pro Art** plus Spawnbedingungen; **Gesamt-Cap fest im Code** (heute 15); **Maximum pro Art** einstellbar |
| Regionen | Benannte Rechtecke im **Map-Editor**, gespeichert pro Map; jede Gegnerart wählt Regionsnamen aus |
| Speicherort | JSON im Repo unter `Assets/Enemies/Types/`, also im Git und fürs Team sichtbar |
| Aussehen | Spritesheet aus `Assets/Enemies/` wählbar, dazu Größe und Farbtönung; Hitbox automatisch aus dem Sprite |
| Vorgehen | In Phasen (Abschnitt 8), nach jeder Phase Test und Feedback |

## 3. Datenmodell: Gegnerart (`Assets/Enemies/Types/<Id>.json`)

Alle Felder außer `Id` sind optional. Leere oder fehlende Felder übernehmen den Standardwert
in der rechten Spalte. Der heutige Gegner wird als `Standard.json` mitgeliefert. Wenn diese
Datei fehlt, gelten die Werte aus dem Code.

### 3.1 Grunddaten

| Feld | Bedeutung | Standard |
|---|---|---|
| `Id` | eindeutiger Dateiname, nur Buchstaben, Ziffern, `-`, `_` | – (Pflicht) |
| `Name` | Anzeigename im Editor | = Id |
| `Description` | Notiz für das Team | leer |
| `Enabled` | Art spawnt im Spiel | ja |

### 3.2 Werte

| Feld | Bedeutung | Standard |
|---|---|---|
| `MaxHitPoints` | Lebenspunkte | 30 |
| `SpeedFactor` | Tempo relativ zum Spieler-Grundtempo | 0,75 |
| `Size` | Größe; Sprite und Hitbox skalieren mit | 34 |
| `CanBePushed` | lässt sich durch Rückstoß und Kontakt schieben | ja |
| `KnockbackResistance` | 0 = voller Rückstoß, 1 = keiner | 0 |
| `DamageTakenMultiplier` | Panzerung (<1) oder Verwundbarkeit (>1) | 1 |
| `StatusImmunities` | Liste aus Gift, Feuer, Frost, Betäubung, Verlangsamung, Schwäche | keine |
| `RegenerationPerSecond` | Heilung pro Sekunde | 0 |

### 3.3 Belohnung

| Feld | Bedeutung | Standard |
|---|---|---|
| `ScoreValue` | zählt als so viele Kills (Score/Bestenliste) | 1 |
| `WeaponPointsPerKill` | Waffenpunkte für die ausgerüstete Waffe | 1 |
| `KillsTowardSkillPoint` | Beitrag zum „+1 Skillpunkt je 5 Kills“ | 1 |

### 3.4 Waffen und Angriffe (`Attacks`, Liste)

Jeder Eintrag beschreibt einen Angriff. Es gibt zwei Arten:

- **Fertige Waffe:** `BaseWeapon` = `enemy-claws` | `sword` | `magic-wand` | `bow`. Alle Werte unten sind dann Überschreibungen.
- **Baukasten:** `Pattern` mit eigenen Werten.

| Feld | Bedeutung | Standard |
|---|---|---|
| `BaseWeapon` | fertige Waffe als Vorlage | `enemy-claws` |
| `Pattern` | Kegel, Bogenhieb, Drehwirbel, Projektil, Puls, Welle | aus der Vorlage |
| `Range` | Reichweite (Kegel/Bogen/Projektil) oder Radius (Puls/Welle) | aus der Vorlage (60) |
| `AngleDegrees` | Öffnungswinkel (Kegel/Bogen) | 45 |
| `DurationSeconds` | Dauer des Angriffs | 0,2 |
| `ProjectileSpeed`, `ProjectileSize` | nur beim Projektil | aus der Vorlage |
| `Damage` | Schaden pro Treffer | 10 |
| `CooldownSeconds` | Abklingzeit | 1,0 |
| `Knockback` | Rückstoß auf das Ziel | 16 |
| `MaxHitsPerTarget` | Treffer pro Ziel und Angriff | 1 |
| `Effects` | Zusatzeffekte, je mit Stärke und Dauer: Gift, Feuer, Frost, Betäubung, Verlangsamung, Schwäche, Lebensraub | keine |
| `UseFromDistance` / `UseUpToDistance` | Abstand zum Ziel, in dem der Angriff gewählt wird | 0 / Reichweite |
| `Weight` | Häufigkeit, wenn mehrere Angriffe passen | 1 |
| `Sprite` | Aussehen bei fertigen Waffen | aus der Vorlage |

Auswahlregel bei mehreren Angriffen: Von den Angriffen, die gerade bereit sind und zum Abstand
passen, wird einer gewichtet zufällig gewählt.

### 3.5 Verhalten

`BehaviorType` legt den Grundtyp fest:

| Typ | Verhalten |
|---|---|
| `Chaser` (Standard) | verfolgt das Ziel bis zum Hitbox-Kontakt, wie heute |
| `Ranged` | hält `PreferredDistance` zum Ziel, weicht bei Annäherung zurück |
| `Turret` | bewegt sich nicht, dreht sich zum Ziel und greift in Reichweite an |
| `Coward` | wie `Chaser`, flieht aber unter `FleeBelowHealthPercent` |

| Feld | Bedeutung | Standard |
|---|---|---|
| `SightRange` | Sichtweite | 200 |
| `SightBonusAfterDetection` | zusätzliche Sicht, nachdem das Ziel entdeckt wurde | 50 |
| `PreferredDistance` | Wunschabstand (`Ranged`) | 150 |
| `FleeBelowHealthPercent` | Fluchtschwelle (`Coward`) | 30 |
| `TargetsMirrorImages` | greift Spiegelbilder an | ja |
| `AlertDurationSeconds` | Dauer der Jagd nach einem Treffer | 20 |
| `AlertRadius` | Radius, in dem Artgenossen gewarnt werden | 100 |
| `WarnsOthers` / `CanBeWarned` | nimmt an der Alarmkette teil | ja / ja |
| `WanderSpeedFactor` | Streuntempo relativ zum eigenen Tempo | 0,5 |
| `WanderMinSeconds` / `WanderMaxSeconds` | Zeit bis zum nächsten Richtungswechsel | 1,5 / 4 |

Für den Alarm zählt jetzt „gleiche Gegnerart“ (`Id`) statt „gleiche Klasse“.

### 3.6 Spawning (eigener Takt pro Art)

| Feld | Bedeutung | Standard |
|---|---|---|
| `SpawnIntervalSeconds` | Takt dieser Art (Spielzeit) | 7 |
| `FirstSpawnDelaySeconds` | Wartezeit bis zum ersten Spawn im Run | = Intervall |
| `SpawnChancePercent` | Wahrscheinlichkeit, dass ein Spawn-Versuch des Takts wirklich spawnt (gilt für die ganze Gruppe, nicht für die Start-Anzahl) | 100 |
| `MaxAlive` | höchstens so viele Gegner dieser Art gleichzeitig | 15 |
| `GroupSizeMin` / `GroupSizeMax` | Gegner pro Spawn, nahe beieinander | 1 / 1 |
| `Regions` | Regionsnamen der Map; leer = überall | leer |
| `PlayerCondition` | `Always` · `PlayerInsideRegion` (nur wenn der Spieler in einer der Regionen ist) · `PlayerOutsideRegion` | `Always` |
| `MinDistanceToPlayer` | Mindestabstand vom Spieler beim Spawn | Sicht + 50 |
| `MinKills` / `MinRunSeconds` | erst ab so vielen Kills bzw. so viel Spielzeit | 0 / 0 |
| `MaxKills` | ab so vielen Kills nicht mehr spawnen | unbegrenzt |

Weitere Regeln:

- Der Gesamt-Cap aller Gegner bleibt im Code (`ArenaField.MaximumEnemyCount = 15`). Ist er (oder eine andere Bedingung) beim Takt nicht erfüllt, fällt dieser Spawn aus und der Takt läuft weiter, wie bisher beim einzelnen Timer. Ein Gegner erscheint also nicht sofort, wenn wieder Platz ist.
- Jede Art hat einen eigenen Zähler, alle Takte laufen parallel.
- Die Zähler werden im Spielstand gespeichert.
- Findet sich in den Regionen kein freier Platz, fällt der Spawn dieses Takts aus und es gibt keinen Absturz. Heute wirft der Code in diesem Fall eine Exception.

### 3.7 Aussehen

| Feld | Bedeutung | Standard |
|---|---|---|
| `SpriteSheet` | PNG aus `Assets/Enemies/` mit gleichnamiger LibreSprite-JSON | `Enemie1.png` |
| `Tint` | Farbtönung (#RRGGBB), leer = keine | leer |
| `Opacity` | Deckkraft | 1 |

Die Hitbox wird automatisch aus den sichtbaren Pixeln des Sprites gemessen, wie heute im
`EntityHitboxCatalog`. Ein Spritesheet muss dieselben Animations-Tags haben wie `Enemie1.json`:
IdleFront/Back/Left, WalkForward/Back/Right/Left, TakeDamage, Dead. Fehlende Tags fallen auf
`AnimtationNotFound` bzw. Idle zurück.

Sounds pro Art folgen später. Das Feld `Sounds` ist reserviert.

## 4. Spawn-Regionen im Map-Editor

- Im Map-Editor gibt es einen neuen Modus „Regionen“: Rechtecke aufziehen, benennen, verschieben und löschen.
- Gespeichert wird als Begleitdatei `Assets/Background/<Map>.regions.json`, mit Name und Rechteck in Weltkoordinaten. Die Map-PNG bleibt unverändert.
- Gespeicherte Maps sind fertige PNG-Bilder. Damit man auch bei bestehenden Maps Regionen setzen kann, lädt der Regionen-Modus eine vorhandene Map als Hintergrund.
- Der Gegner-Editor bietet die Regionsnamen aller Maps zur Auswahl an. Gibt es eine Region auf der gespielten Map nicht, gilt für diese Art „überall“.
- Mit F4+B (Debugbereiche) werden die Regionen im Spiel angezeigt.

## 5. Gegner-Editor (neuer Tab im Dev Tool)

- **Links:** Liste der Gegnerarten mit „Neu“, „Duplizieren“, „Löschen“ und „Speichern“.
- **Mitte:** Formular in Abschnitten (Grunddaten, Werte, Belohnung, Angriffe, Verhalten, Spawning, Aussehen).
  - Jedes Feld zeigt seinen Standardwert als Platzhalter, „Zurücksetzen“ leert es.
  - Angriffe sind eine Liste: „Fertige Waffe“ oder „Baukasten“, „+“ fügt einen Angriff hinzu.
  - Es werden nur die Felder angezeigt, die zum gewählten Muster bzw. Verhaltenstyp passen.
- **Rechts:** Vorschau mit animiertem Sprite, Tönung, Größe, Hitbox, Sicht- und Angriffsradien.
- **Unten:**
  - „Gegner neben mir spawnen“: spawnt die aktuelle Fassung zum Testen, ohne zu speichern.
  - „Alle Gegner entfernen“
  - Fehleranzeige, z. B. „Sprite nicht gefunden“ oder „Intervall muss > 0 sein“
- Gespeicherte Arten gelten ab dem nächsten Spawn, laufende Gegner behalten ihre Werte.
- Esc fragt vorher nach, wie bei den anderen Editoren.

## 6. Spielstände und Kompatibilität

- `ArenaEntitySave` bekommt `EnemyTypeId`. Fehlt die Angabe (alte Spielstände) oder wurde die Art gelöscht, wird der Gegner als `Standard` geladen.
- `ArenaSaveGame` bekommt die Spawn-Zähler pro Art. Das bisherige `EnemySpawnElapsedSeconds` wird für `Standard` übernommen.
- Ungültige Werte in einer JSON-Datei, z. B. negative Lebenspunkte, werden beim Laden auf den Standardwert gesetzt. Das Dev Tool meldet sie, das Spiel stürzt dadurch nicht ab.

## 7. Technischer Aufbau (Vorschlag)

| Neu/geändert | Aufgabe |
|---|---|
| `Enemies/EnemyTypeDefinition.cs` | JSON-Modell, alle Felder nullable |
| `Enemies/EnemyType.cs` | aufgelöste Art: Definition + Standardwerte, validiert, mit fertigen `Weapon`-Objekten |
| `Enemies/EnemyTypeRepository.cs` | Laden, Speichern, Auflisten, Löschen unter `Assets/Enemies/Types/` (wie `ArenaSkillTreeRepository`) |
| `Enemies/EnemySpawnDirector.cs` | Takte pro Art, Bedingungen, Regionen, Gesamt-Cap; aus `ArenaField` herausgelöst |
| `Enemies/ArenaMapRegions.cs` | Regionen pro Map laden und speichern |
| `PursuerEnemy.cs` | bekommt seine `EnemyType` statt Konstanten; Verhaltenstypen als kleine Strategie-Klassen |
| `PainterG9.cs` | ein Animator pro Spritesheet statt eines einzigen `_enemyAnimator` |
| `ArenaDevToolsControl.cs` | neuer Tab `ArenaEnemyEditorControl` |

Die Framework-Interfaces bleiben unverändert.

## 8. Phasen

| Phase | Inhalt | Fertig, wenn … |
|---|---|---|
| **1 – Datenmodell und Spawning** | Definition, Repository, `Standard.json`, Gegner mit Art, Spawn-Takt pro Art mit Max. pro Art und Bedingungen (ohne Regionen), Spielstände mit Art | das Spiel spielt sich mit `Standard.json` wie heute, eine zweite Art per JSON-Datei spawnt mit eigenem Takt, alle Tests grün |
| **2 – Editor-Tab** | Formular mit allen Werten, Liste, Speichern, Validierung, Test-Spawn, Vorschau | eine neue Art lässt sich ohne Text-Editor anlegen und im Spiel testen |
| **3 – Verhalten und Waffen-Baukasten** | Verhaltenstypen, mehrere Angriffe, Effekte, fertige Waffen als Vorlage | Fernkämpfer und Turm funktionieren, Angriffe sind frei kombinierbar |
| **4 – Regionen** | Regionen-Modus im Map-Editor, Begleitdatei, Bedingungen „Spieler in/außerhalb Region“, Debug-Anzeige | Arten spawnen nur in ihren Regionen bzw. nur, wenn der Spieler dort ist |

## 9. Offene Fragen

1. **Spawnbedingungen:** Reichen „Spieler in Region“, „Spieler außerhalb“, „ab Kills/Spielzeit“ und „Mindestabstand“, oder sind weitere gewünscht (z. B. nur bei wenig Spieler-LP, nur nachts)?
2. **Regionen:** Sollen Regionen nur Rechtecke sein, oder auch Kreise oder Vielecke?
3. **Sprite-Auswahl:** Gibt es weitere Gegner-Sprites, oder sollen anfangs alle Arten `Enemie1` mit Tönung nutzen?
4. **Gegner gegen Gegner:** Sollen Flächenangriffe von Gegnern andere Gegner treffen können? Heute treffen sie nur die Spielerseite.
5. **Gesamt-Cap:** Bleiben 15 Gegner im Code, oder soll der Wert später in die Einstellungen?

## 10. Stand nach Phase 1

Umgesetzt:

- `Enemies/EnemyTypeDefinition.cs`: JSON-Modell, alle Felder nullable
- `Enemies/EnemyType.cs`: aufgelöste Art mit Prüfung. Ungültige Werte fallen auf den Standardwert zurück und werden gemeldet.
- `Enemies/EnemyTypeRepository.cs`: Laden, Speichern und Löschen unter `Assets/Enemies/Types/`. Geschrieben werden nur gesetzte Werte; defekte Dateien werden übersprungen.
- `Enemies/EnemySpawnDirector.cs`: Takt pro Art und Spawnbedingungen; Abschnitt 3.6 ohne Regionen und Spielerbedingung
- `Assets/Enemies/Types/Standard.json`: der bisherige Gegner, ohne geänderte Werte

Abweichungen vom Plan:

- **Konfiguration in `NonPlayerCharacter`** (Entscheidung Till): Gegnerart, Sicht, Alarm, Streunen, aktuelles Ziel und der Angriffs-Einstieg liegen in der gemeinsamen NPC-Basisklasse. `PursuerEnemy` und `MirrorImage` nutzen das gemeinsame Streunen; `MirrorImage` hat keine eigene Kopie mehr. Das Spielfeld führt Gegner als `NonPlayerCharacter`.
- Neues Feld `StartCount` (Gegner zu Run-Beginn, Standard 1). Der bisherige Startgegner wird dadurch zum Standardwert.
- Die Standardwerte stehen als `EnemyType.Default…`-Konstanten im Code. `PursuerEnemy.SightRange` usw. entfallen, weil sie jetzt Eigenschaften jedes NPCs sind.
- Der Spielstand speichert pro Gegner `EnemyTypeId`, außerdem `EnemySpawnTimers` und `RunSeconds`. Alte Spielstände laden mit dem bisherigen Einzeltimer für `Standard`.
- Findet sich keine freie Spawn-Position, wird der Spawn übersprungen. Vorher hat der Code eine Exception geworfen.

Noch nicht verwendet (folgen in Phase 2/3): Belohnung (3.3), Waffen (3.4), Verhaltenstypen (3.5: `BehaviorType`, `PreferredDistance`, `FleeBelowHealthPercent`), Aussehen (3.7: `SpriteSheet`, `Tint`) sowie aus 3.2 `KnockbackResistance`, `DamageTakenMultiplier`, `StatusImmunities` und `RegenerationPerSecond`.

Tests: Die Testharness nutzt einen leeren temporären Ordner für Gegnerarten (`EnemyTypeRepository.DirectoryOverride`) und prüft die mitgelieferten Arten nur lesend.

## 11. Stand nach Phase 2

Umgesetzt:

- **Gegner-Editor** als dritter Tab im Dev Tool (`Enemies/ArenaEnemyEditorControl.cs`):
  - Links die Liste der Arten.
  - In der Mitte das Formular mit allen Werten aus Phase 1. Neben jedem Feld steht der Standardwert; Ja/Nein-Felder haben „Standard“, „ja“ und „nein“.
  - Rechts die Vorschau: Gegner vergrößert mit Hitbox, Sicht, Sicht mit Bonus, Alarmradius, eine Kurzübersicht und die Probleme der aktuellen Werte, live.
- **Knöpfe:** Neu, Duplizieren, Löschen (beim Standard: zurücksetzen), Speichern, „Gegner neben mir spawnen“ und „Alle Gegner entfernen“.
  - Gespeichert wird nur, wenn alle Werte gültig sind. Die Arten des Spiels werden danach neu geladen; neue Werte gelten ab dem nächsten Spawn.
  - Der Test-Spawn nimmt die aktuelle, auch ungespeicherte Fassung und setzt den Gegner direkt neben den Spieler, auch im pausierten Run.
  - Beim Wechsel der Auswahl, bei Neu und beim Überschreiben einer vorhandenen Art fragt der Editor nach.
- **Neues Feld `SpawnChancePercent`** (Spawn-Wahrscheinlichkeit pro Versuch, Standard 100 %).
- **Kommazahlen:** `ArenaIntegerTextBox` nimmt mit `AllowDecimalSeparator` ein Dezimaltrennzeichen an; Punkt oder Komma, angezeigt als Komma.
- **Esc im Dev Tool:** Die Rückfrage nennt jetzt auch den Gegner-Editor.

Noch offen für Phase 3: Belohnung, Waffen, Verhaltenstypen, Aussehen (Sprite, Tönung), Widerstände und Regeneration. Danach folgt Phase 4 mit den Regionen.

## 12. Stand nach Phase 3

Umgesetzt (alle Felder aus 3.2–3.5 und 3.7, im Editor und im Spiel):

- **Widerstände** in `Entity`: `DamageTakenMultiplier` (Panzerung bzw. Verwundbarkeit), `KnockbackResistance` und `StatusImmunities` (Gift, Feuer, Frost, Betäubung, Verlangsamung, Schwäche). Ohne Gegnerart wirken sie nicht; für den Spieler ändert sich nichts.
- **Regeneration** in `NonPlayerCharacter`: heilt still, also ohne Heil-Ereignis, Partikel oder Sound.
- **Belohnung:**
  - `ScoreValue` geht in Score, Bestenliste und die Spawnbedingung „ab Kills“ ein.
  - `KillsTowardSkillPoint` zählt für die Skillpunkt-Meilensteine; mehrere Meilensteine auf einmal sind möglich.
  - `WeaponPointsPerKill` gibt Waffenpunkte; dafür gibt es eine neue Überladung `ArenaProgression.AwardKill(weapon, kills, weaponPoints)`.
- **Angriffe** (`Enemies/EnemyAttack.cs`):
  - Jede Art hat eine Liste von Angriffen. Ein Angriff nimmt entweder eine fertige Waffe als Vorlage (Klauen, Schwert, Stab, Bogen) oder ein Muster aus dem Baukasten.
  - Einstellbar sind Reichweite, Winkel, Dauer, Projektiltempo und -größe, Schaden, Abklingzeit, Rückstoß und Treffer pro Ziel.
  - Zusatzeffekte: Gift, Feuer, Frost, Betäubung, Verlangsamung, Schwäche und Lebensraub.
  - Zur Auswahl gehören der Einsatzbereich (ab/bis Abstand) und eine Gewichtung.
  - Von den bereiten Angriffen, die zum Abstand passen, wird einer nach Gewichtung gewählt. Ein neuer Angriff startet erst, wenn der laufende vorbei ist.
- **Verhaltenstypen** (`Enemies/EnemyBehavior.cs`, umgesetzt in `PursuerEnemy`):
  - Verfolger: wie bisher.
  - Fernkämpfer: hält `PreferredDistance` mit ±12 Spielraum und weicht zurück.
  - Turm: bewegt und streunt nicht.
  - Feigling: flieht unter `FleeBelowHealthPercent` und greift dann nicht an.
- **Aussehen:** Spritesheet aus `Assets/Enemies`. Der Painter hat einen Animator pro Sheet; fehlt das Sheet, nutzt er den Standardgegner. Dazu kommen Tönung (`#RRGGBB`) als eingefärbte Kopie des Sprites und Deckkraft. Die Hitbox wird pro Sheet aus den Pixeln gemessen (`EntityHitboxCatalog.GetEnemyHitboxOffset`).
- **Editor:**
  - Neue Abschnitte Belohnung, Verhalten und Aussehen.
  - Immunitäten als Ankreuzfelder, Auswahllisten für Verhalten, Spritesheet, Waffe und Muster.
  - Angriffsliste mit „+ Angriff“ und „Angriff entfernen“. Pro Angriff erscheinen nur die Felder, die zum Muster passen.
  - Die Vorschau zeigt zusätzlich Reichweite, Wunschabstand, Tönung, Verhalten und eine Zeile je Angriff.
  - Ein fehlendes Spritesheet blockiert das Speichern.
- **Aufgeräumt:**
  - Die Prüfhilfe für Werte ist eine eigene Klasse (`Enemies/EnemyValueChecker.cs`), die Gegnerart und Angriffe gemeinsam nutzen.
  - Die Felder im Editor kommen aus generischen Feldbeschreibungen für Gegnerart und Angriff, nicht aus Einzelcode.

Offen: Phase 4 (Regionen und Spielerbedingung), Sounds pro Art.

## 13. Stand nach Phase 4

Umgesetzt:

- **Eigener Dev-Tool-Tab „Spawn-Regionen“** (Entscheidung Till) statt eines Modus im Map-Editor, weil der Map-Editor vorhandene Maps nicht wieder öffnen kann (`Enemies/ArenaRegionEditorControl.cs`). Jonas' Map-Editor bleibt unverändert.
  - Vorhandene Map wählen; Regionen als **Rechtecke und Kreise** (Entscheidung Till) aufziehen.
  - Ziehen auf einer Form verschiebt sie, Rechtsklick löscht sie. Dazu kommen Umbenennen, Löschen, Speichern, Verwerfen und Zoom.
  - Formen mit gleichem Namen bilden zusammen eine Region.
- **Speicherung** als `Assets/Background/<Map>.regions.json` (`Enemies/ArenaMapRegions.cs`). Eine leere Liste löscht die Datei; defekte Dateien werden gemeldet und gelten als „keine Regionen“.
- **Gegnerart:** neue Felder `Regions` (Namen) und `PlayerCondition` (`Always`, `PlayerInsideRegion`, `PlayerOutsideRegion`). Eine Spielerbedingung ohne Regionen wird gemeldet.
- **Spawning:**
  - Arten mit Regionen spawnen in einer ihrer Formen; größere Formen bekommen anteilig mehr Spawn-Punkte. Mindestabstand und Überlappungsprüfung gelten weiter.
  - Regionsnamen, die die gespielte Map nicht hat, werden ignoriert. Bleibt keine übrig, spawnt die Art überall und die Spielerbedingung entfällt.
  - Die Spielerbedingung gilt auch für die Anzahl zu Run-Beginn.
- **Spielfeld:** lädt die Regionen der gespielten Map beim Start, beim Map-Wechsel, beim Laden eines Spielstands und nach dem Speichern im Regionen-Tab.
- **Anzeige:** F4+B (Debugbereiche) zeigt die Regionen gestrichelt mit Namen.
- **Gegner-Editor:**
  - „Spawn-Regionen“ als Ankreuzfelder mit den Namen aller Maps, nach dem Speichern im Regionen-Tab aktualisiert. Namen, die es nicht mehr gibt, bleiben erhalten.
  - „Spielerbedingung“ als Auswahl.

Damit sind die offenen Fragen 1 (Spawnbedingungen) und 2 (Formen) aus Abschnitt 9 beantwortet. Offen bleiben Sounds pro Art sowie die Fragen 3–5.

## 14. Nachtrag: Spawn-Positionssuche ausgelagert

Die Suche nach freien Gegner-Positionen steckt jetzt in `Enemies/EnemySpawnPositionFinder.cs` statt in `ArenaField` (dort rund 100 Zeilen weniger). Sie kennt drei Fälle:

- irgendwo in der Arena mit Mindestabstand zum Spieler
- in Spawn-Regionen, flächengewichtet
- direkt neben dem Spieler für den Test-Spawn

Das Spielfeld übergibt ihr die begehbare Fläche, den Spieler und die Überlappungsprüfung. Die Spawn-Positionen der Spiegelbilder bleiben im Spielfeld, weil sie andere Regeln haben (Kreise um den Spieler, Spiegelbilder blockieren niemanden).

## 15. Nachtrag: Frosch und neue Fähigkeiten

**Gegnerart `Frog`** (`Assets/Enemies/Types/Frog.json`):

- 1 Lebenspunkt, stirbt also durch einen Schlag.
- Genau einer gleichzeitig (Anzahl zu Run-Beginn 1, Max. 1). Nach seinem Tod kommt er beim nächsten Spawn-Versuch zurück, spätestens nach 5 s.
- Hüpft langsam auf eine zufällige Entity zu und verschlingt mit einem Zungenkegel (Reichweite 70, 60°) die nächste Entity in Reichweite, auch den Spieler und andere Gegner.

**Neue, allgemein nutzbare Fähigkeiten** (auch im Gegner-Editor):

- **Zielwahl `RandomEntity`**: Die Art verfolgt eine zufällige lebende Entity (Spieler, Spiegelbild oder anderer Gegner) ohne Sichtgrenze. Ein neues Ziel gibt es, wenn das alte stirbt oder nach `RetargetSeconds`.
- **Bewegungsart `Hop`**: Die Art bewegt sich in Sprüngen (`HopSeconds`) mit Pausen dazwischen (`HopPauseSeconds`).
- **Angriffsziele `Everyone`**: Der Angriff trifft alle Entities außer dem Angreifer, und zwar die nächste in Reichweite statt nur das aktuelle Ziel (neuer `WeaponTargetingMode.Everyone`).
- **Effekt „Verschlingen“** (`Devour`, `DevourPullSeconds`, `DevourEffect`):
  - Das Opfer wird festgehalten und in der eingestellten Zeit herangezogen.
  - Am Ende stirbt es sofort, ohne Panzerung (`Entity.TakeFatalDamage`).
  - Unverwundbare überleben (Godmode, Respawn-Schutz).
  - Gefressene Gegner und Spiegelbilder verlassen die Arena und zählen nicht als Kills. Ein gefressener Spieler stirbt normal, mit der üblichen Todesstrafe.

**Animation:**

- Der `SpriteSheetAnimator` spiegelt fehlende Clips einer Seite: Fehlt „… Right …“, nimmt er „… Left …“ waagerecht gespiegelt, und umgekehrt.
- Für Arten mit eigenem Sheet wählt der Painter „Idle/Walk <Seite>“ und beim Angriff „[Walk ]Attack <Richtung>“ in acht Richtungen. „Attack Right Down“ beim Frosch kommt gespiegelt aus „Attack Left Down“.
- Der Standardgegner bleibt bei seinen bisherigen Clips.

**Assets korrigiert** (mit Zustimmung):

- `Frog.json`: Tag-Namen vereinheitlicht und Tippfehler behoben; „Right Attack Down“ heißt jetzt richtig „Attack Left Down“.
- `Frog.png`: Frame 44 und 45 getauscht, damit „Walk Attack Left Up“ keine „NotFound“-Grafik mehr enthält.

**Fehler behoben:** Das Muster „Puls um sich“ trifft nur den Angreifer selbst (Heilimpuls). Gegner hätten sich damit selbst getroffen. Es steht deshalb nicht mehr zur Auswahl; vorhandene Einträge werden gemeldet.

## 16. Nachtrag: Mine Turtle und neue Fähigkeiten

**Gegnerart `MineTurtle`** (`Assets/Enemies/Types/MineTurtle.json`, Sheet `Mine Turtel.png`):

- Bewegt sich nicht (Turm, Tempo 0) und lässt sich nicht verschieben. Sie blockiert niemanden, damit der Spieler auf sie treten kann.
- Tritt der Spieler auf ihre Hitbox, geht der Knopf runter (Clip „Aktive“, 0,3 s). Dann sagt sie „Hello“ (Clip „Hello“ mit `mineTurtleHello.wav`, 0,7 s) und explodiert.
- Die Explosion ist eine Welle (Radius 160) mit dem Power-Blast-Ton. Sie tötet alles im Radius, auch andere Gegner; Godmode schützt.
- Danach stirbt die Schildkröte selbst, ohne Punkte für den Spieler.
- Spawning: 2 zu Run-Beginn, höchstens 3, alle 20 s ein Versuch.

**Neue, allgemein nutzbare Fähigkeiten** (auch im Gegner-Editor):

- **Blockiert Bewegung** (`BlocksMovement`): auch an `Entity`. Spiegelbilder nutzen das jetzt statt einer Typprüfung.
- **Angriffs-Auslöser** `Trigger`: `Distance` (wie bisher) oder `Contact` (Hitboxen berühren sich).
- **Auslösen dürfen** `TriggeredBy`: `Targets`, `Player`, `PlayerSide` oder `Anyone`.
- **Vorlauf** `Windup`: Schritte mit Clip, Dauer und optionalem Sound, die vor dem Angriff laufen. Im Editor als Text „Clip:Sekunden[:Sound]; …“.
- **Sound beim Angriff** `Sound` und **Sounds für Gegnerarten** (`ArenaAudio.EnemySoundNames`): PowerBlast, MineTurtleHello, SwordSwing, ElectricDamage, EntityDeath.
- **Tödlich** `Lethal` (`LethalEffect`) und **Stirbt nach dem Angriff** `KillsAttacker`.
- **Painter:** zeigt den Vorlauf-Clip. Sheets ohne Richtungs-Clips fallen auf „Idle“ zurück; dafür braucht es keinen „NotFound“-Clip.

**Behoben:** Ein Angriff auf ein Ziel genau im Mittelpunkt des Gegners hatte keine Richtung, und das Kampfsystem lehnte ihn ab. Die Mine hätte dann ihren Vorlauf endlos wiederholt.

## 17. Nachtrag: Scharfe Pixel-Art und Froschsounds

- **Schärfe:** Pixel-Art wird jetzt ohne Weichzeichnen skaliert (`BitmapScalingMode.NearestNeighbor`). Das gilt für die ganze Spielwelt (Map, Spieler, Gegner, Spiegelbilder, Waffen, Partikel), die HUD-Bilder, die Vorschauen im Gegner-Editor, den Regionen-Tab und die Map-Fläche im Map-Editor. Das gemeinsame Framework-Canvas bleibt unverändert.
- **Froschsounds:** neue Soundgruppe `frogCroak_*.wav`. Sie hat eine Stimme und 0,7 s Mindestabstand, damit sich Quaker nicht überlagern.
- **Gelegentlicher Sound pro Gegnerart:** neue Felder `IdleSound`, `IdleSoundMinSeconds` und `IdleSoundMaxSeconds`, auch im Gegner-Editor. Der Frosch quakt damit alle 2–6 s Spielzeit und zusätzlich bei jedem Zungenangriff.
- **Ein Ort für Soundmuster:** `ArenaAudio.SoundFilePatterns` stellt die Dateimuster bereit. Der Soundtest nutzt sie, statt eine eigene Kopie zu pflegen.
