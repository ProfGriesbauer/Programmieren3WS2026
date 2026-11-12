# Plan: Spielwelt, Uhr, Charakter, Läden, Handy, Speichern

Entwickler: Patrick Lupierz – Stand 12.11.2026 – Auftrag: siehe `Patrick_Lupierz_Prompts.md`, Prompt 4

## 1. Bestandsaufnahme

Schon vorhanden und weiterverwendet:

| Anforderung | Vorhandene Lösung |
|---|---|
| Getrennte Karten mit Türen | `IG1C_WorldScene`, `G1C_SceneTransitionManager` (Denis Mock) |
| Einheitliche Währung | `G1C_MoneyManager` – einzige Geldverwaltung, überall "Chips" |
| NPC-Persönlichkeit, Dialoge, Vertrauen/Respekt, Status, Erinnerungen, Geldbeutel, Betteln, Aufgaben, Stimmung, Monologe, Porträts, Ruf | Ordner `NPC/` (Eduard Schoenfeld) |
| Austauschbare Grafiken | `G1C_SpriteLibrary` (PNG neben der exe ersetzt eingebaute Grafik) |
| Eingaben, Tick, Zeichnen | nur über `IGameRules2.TickGameCall`, `DoMove`, `IPaintGame2` des Frameworks |

Fehler im Bestand: In der Stadt wurde die Spielfigur außerhalb des Bildes gezeichnet. Der Painter gab den Figuren
die Ecke des Kartenausschnitts statt der Kartenecke mit; sobald die Kamera scrollte, fehlte der Kamera-Versatz.

## 2. Konflikte mit den Projektregeln

| Punkt | Bewertung | Umgang |
|---|---|---|
| `Agents.md` sieht nur das Casino als Spielbereich vor, das Konzept mehrere Karten | **genehmigungspflichtig** (Team/Dozent). Die Außenwelt ist bereits seit dem Commit „Außenwelt hinzugefügt“ im Branch. | Auf dem Bestand aufgebaut, `Agents.md` nicht verändert. Regelkonforme Alternative, falls abgelehnt: Läden als Räume im Casino – die Shop-, Inventar- und Uhrklassen hängen nicht an der Stadtkarte. |
| Speicherdateien | zulässig, solange nichts im Quellcode-Ordner landet | Ordner `G1Casino_Saves` neben der `OOPGames.exe`, wie der Grafikordner |
| „Beenden“ im Hauptmenü | nicht zulässig: das Spiel darf das Framework-Fenster nicht schließen | weggelassen |
| Enter/Leertaste | lösen den Start-Knopf des Frameworks aus | Namenseingabe bestätigt mit Pfeil ↓ oder Klick |
| Änderungen in Dateien von Denis Mock und Eduard Schoenfeld | nötig für Karte, Türen, Speichern | jede Stelle mit „Änderung durch: Patrick Lupierz – Datum“ markiert, bestehende Kennzeichnungen erhalten |

Außerhalb von `Spiel 2` wurde nichts geändert.

## 3. Umsetzung (Klasse je Aufgabe)

| Funktion | Klasse(n) |
|---|---|
| Reparatur Figur + Stadtbild in 16 px/Kachel, vergrößert gezeichnet | `G1C_CasinoPainter.DrawWorld` |
| Stadtkarte 96x64, geschwungene Straßen, Brücken, Türen in der Hausfront | `G1C_ExteriorMap`, `G1C_ExteriorWorld` |
| Neue Stadtgrafik (Randkacheln, Gebäude 96x80, Baum, Laterne, Mülleimer, Möbel) | `G1C_TownSprites` |
| Tankstelle, Bekleidungsgeschäft, Supermarkt-Kasse | `G1C_BuildingInteriorWorld`, `G1C_WorldLocation`, `G1C_SceneTransitionManager` |
| Zentrale Uhr, Tag-Nacht-Licht | `G1C_GameClock`, `G1C_CasinoPainter.DrawDaylight` |
| Hauptmenü, Speicherplätze, Charaktererstellung | `G1C_StartFlow`, `G1C_StartView` |
| Name und Aussehen, sichtbare Kleidung | `G1C_PlayerProfile`, `G1C_BuiltInSprites` (Skin `pc-...`) |
| Gegenstände, Inventar, Effekte, Läden | `G1C_Item`, `G1C_ItemCatalog`, `G1C_Inventory`, `G1C_BuffSystem`, `G1C_Shop` |
| Listenmenü für Laden/Inventar/Kleiderschrank | `G1C_ListMenu`, `G1C_ListMenuView` |
| Handy: Kontakte, Anrufe, SMS, Uhr, Aufgaben, Speichern | `G1C_Phone`, `G1C_PhoneView`, `G1C_Conversation.OpenCall` |
| Telefonnummer ab Freundschaft | `G1C_Conversation.TryGivePhoneNumber`, `G1C_NpcMemory.PhoneNumberGiven` |
| Mülleimer, lokale Wahrnehmung | `G1C_TrashSystem` |
| Speichern/Laden (JSON, Version, 3 Plätze) | `G1C_SaveSystem`, `G1C_GameManager.CreateSave/ApplySave` |

Entscheidungen:

- **Größen:** Stadt-Kacheln werden exakt verdoppelt (32 px, 15x9 Kacheln sichtbar). Dafür ist die Karte kleiner
  (96x64 statt 180x180): Zuhause → Casino dauert etwa 15 Sekunden statt über einer Minute.
- **Kleidung:** keine Ebenen-Grafiken, sondern Farbpalette pro Figur. Das nutzt den vorhandenen Figuren-Zeichner und
  funktioniert automatisch für alle 4 Richtungen und 3 Schrittbilder.
- **Effekte** wirken auf Lauftempo, Steuerung und Gespräche, nie auf Gewinnchancen. Alkohol und Zigaretten haben
  einen Folgenachteil (Kater, Husten).
- **Geldquellen:** Mülleimer höchstens einmal pro Ingame-Tag je Eimer (im Schnitt rund 2 Chips), Pfand 8 Chips.

## 4. Tasten

`P` Handy · `I` Inventar · `M` Hilfe/Pause (Uhr steht) · sonst wie bisher.

## 5. Tests

Testprojekt außerhalb des Repos, 149 Prüfungen, alle bestanden. Abgedeckt aus Abschnitt 38 des Auftrags:
1–5, 8–21, 23–25. Dazu Bildkontrolle gerenderter Szenen (Stadt bei Tag/Abend/Nacht, Läden, Handy, Menüs).
Nicht getestet: Bedienung mit echter Tastatur und Maus im Hauptfenster.

## 6. Offen

| Punkt | Grund |
|---|---|
| NPC-Tagesabläufe über mehrere Karten (Testfall 22) | Phase 10; Gast Lena steht deshalb weiterhin im Casino **und** in der Stadt |
| Friseur, Frisuren-Auswahl | braucht neue Kopf-Grafiken je Frisur |
| Möbelkauf/Hausgestaltung, Wetter | im Auftrag ausdrücklich nachrangig |
| Handy-Apps Karte und Einstellungen, Hauptmenü „Einstellungen“ | es gibt noch nichts einzustellen |
| 20–30 Dialogzeilen je NPC | Texte fehlen; Verkäufer haben noch kein Charakterprofil |
| Gegenstands-Icons, detaillierte Porträts | siehe `Assets/README.md` |
| Kostenloses Casino-Duell | würde die Spielregeln der Minispiele ändern |
