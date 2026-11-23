# SprintPlanning – Issue-Auswertung (Casino, Spiel 2)

Erstellt: 15.11.2026 – Patrick Lupierz (KI-gestützt mit Claude Code, geprüft und übernommen von Patrick Lupierz)

**Quelle:** ausschließlich `Scrum/SprintPlanning.md` (einzige Datei der SprintPlanning-Dokumentation; ein Unterordner
`Scrum/SprintPlanning/` existiert nicht). GitHub-Issues wurden nicht verwendet. Die Originaldatei bleibt unverändert.

**Prüfgrundlage:** Quellcode-Durchsicht und der automatisierte Selbsttest `G1C_SelfTest` (Stand 15.11.2026, 2. Arbeitsschritt:
43/43 BESTANDEN, siehe `Plans/Testbericht_2026-11-15.md`). Manuelle Bedienung im echten Hauptfenster konnte der
KI-Agent nicht durchführen; solche Punkte sind ausdrücklich als nicht geprüft gekennzeichnet.

Statuswerte: VOLLSTÄNDIG UMGESETZT · TEILWEISE UMGESETZT · FEHLERHAFT · NUR GRAFISCH VORHANDEN · NICHT IMPLEMENTIERT ·
NICHT ÜBERPRÜFBAR · ÜBERHOLT. Priorität: H = hoch, M = mittel, N = niedrig.

## Zusammenfassung

| Status | Anzahl |
|--------|-------:|
| VOLLSTÄNDIG UMGESETZT | 31 (Stand 16.11.2026, inkl. 11 neuer Punkte aus Abschnitt H) |
| TEILWEISE UMGESETZT | 12 |
| FEHLERHAFT | 0 (Issue 6 war fehlerhaft und wurde am 15.11.2026 behoben) |
| NUR GRAFISCH VORHANDEN | 0 |
| NICHT IMPLEMENTIERT | 0 (Arcade C11 am 15.11.2026, 2. Arbeitsschritt, umgesetzt) |
| NICHT ÜBERPRÜFBAR | 2 (B4, B7: Entscheidung Team/Dozent) |
| ÜBERHOLT | 0 |
| **Summe** | **45** |

## A. Sprint 1 – Aufgaben

| # | Aufgabe | urspr. Status | tatsächlicher Status | Begründung / betroffene Klassen | Fehlend / nächste Schritte | Tests | Prio | Verantw. |
|---|---------|---------------|----------------------|----------------------------------|----------------------------|-------|------|----------|
| A1 | Framework-Anbindung (Painter, Rules, Player) | erledigt | VOLLSTÄNDIG UMGESETZT | `G1C_CasinoPainter` (IPaintGame2), `G1C_CasinoRules` (IGameRules2), `G1C_CasinoField.CanBePaintedBy`, `G1C_CasinoHumanPlayer.CanBeRuledBy`; nur 3 Registrierungszeilen in MainWindow | – | Build 0/0; Selbsttests laufen über `G1C_GameManager` | – | _eintragen_ |
| A2 | Welt, Kollision, Spielfigur, Animation | erledigt | TEILWEISE UMGESETZT | WASD-Logik repariert (`G1C_PlayerMovement`, `G1C_InputManager.HeldDirection`), Canvas-Fokus (`G1C_CasinoPainter.FocusCanvas`), Kollisionsfehler behoben (`G1C_CollisionManager`) | Tastaturfokus und Laufgefühl im echten Fenster manuell prüfen | 7 Bewegungstests BESTANDEN | H | Patrick Lupierz |
| A3 | Dialoge, HUD, Menü, Ergebnisanzeige | erledigt | VOLLSTÄNDIG UMGESETZT | `G1C_DialogueBox`, `G1C_GameHUD`, `G1C_CasinoMenu`, `G1C_ResultScreen`; Ergebnis wird in `EndMinigame` aus echtem Kontostand gebildet | – | Zustandstests (Dialog, Menü) BESTANDEN; Ergebnisanzeige nur Code-Durchsicht | – | _eintragen_ |
| A4 | Roulette + KI | erledigt | VOLLSTÄNDIG UMGESETZT | `G1C_Roulette`, `G1C_RouletteBet` (35:1, 2:1, 1:1, Null), `G1C_RouletteAI` | KI-Verhalten nur per Code-Durchsicht | Roulette-Test BESTANDEN | – | _eintragen_ |
| A5 | Blackjack + KI | erledigt | VOLLSTÄNDIG UMGESETZT | `G1C_BlackjackHand`, `DealerShouldHit` (zieht bis 16) | – | Blackjack-Test BESTANDEN | – | _eintragen_ |
| A6 | Poker + KI | erledigt | VOLLSTÄNDIG UMGESETZT | `G1C_PokerHandEvaluator`, `G1C_PokerAI` | KI nur Code-Durchsicht | Poker-Test BESTANDEN | – | _eintragen_ |
| A7 | Spielautomat | erledigt | VOLLSTÄNDIG UMGESETZT | `G1C_Slots.GetMultiplier`, Zufall unabhängig von Grafik | – | Slots-Test BESTANDEN | – | _eintragen_ |
| A8 | Austauschbare Grafiken | erledigt | VOLLSTÄNDIG UMGESETZT | `G1C_SpriteLibrary`: PNG `<ID>.png` überschreibt eingebaute Grafik; neue IDs (`sign_closed`, `obj_poster_ad`, `obj_sticker_bw`, `obj_sign_post`) ebenso | – | Grafiktests (alle Objekt-IDs auflösbar) BESTANDEN | – | _eintragen_ |
| A9 | Manueller Test im Hauptfenster | erledigt (Team, 16.11.) | VOLLSTÄNDIG UMGESETZT (laut SprintPlanning vom Team erledigt; vom Agenten selbst nicht bedienbar) | KI-Agent kann das WPF-Fenster nicht bedienen. Ersatzweise: Szenen offscreen mit dem echten Painter gerendert (Zuhause, CLOSED-Schild, Oma, Tankstelle) | Teammitglied spielt Checkliste aus `Plans/Testbericht_2026-11-15.md` durch | – | H | _eintragen_ |

## B. Sprint 1 – Nachtrag: Erweiterungen in Agents.md

| # | Inhalt | urspr. | tatsächlicher Status | Begründung | Nächste Schritte | Prio | Verantw. |
|---|--------|--------|----------------------|------------|------------------|------|----------|
| B1 | Abschnitt 0: harte Regeln / erweiterbare Inhalte | erledigt (Team, 16.11.) | VOLLSTÄNDIG UMGESETZT (Freigabe laut SprintPlanning erteilt) | Abschnitt steht in `Agents.md`; offen ist nur die Freigabe durch Team/Dozent | Freigabe einholen | M | Team |
| B2 | Stadt, Innenräume, Gebäude erweiterbar (Tankstelle, Bekleidung) | offen | VOLLSTÄNDIG UMGESETZT | `G1C_BuildingInteriorWorld` (GasStation, ClothingStore, Bank, FurnitureStore) | – | – | Patrick Lupierz |
| B3 | Weitere getrennte Karten erlaubt | offen | VOLLSTÄNDIG UMGESETZT | Regel steht in Agents.md; `IG1C_WorldScene` + `G1C_SceneTransitionManager` tragen weitere Karten | – | – | – |
| B4 | Stadtgröße 96x64 statt 180x180 | offen | NICHT ÜBERPRÜFBAR | Code: 96x64 (`G1C_ExteriorMap`). Entscheidung liegt bei Team/Dozent | Entscheidung protokollieren | M | Team |
| B5 | Innenräume Krankenhaus, Supermarkt, weitere | offen | VOLLSTÄNDIG UMGESETZT | sechs Innenräume vorhanden | Arcade, Fahrradladen, Omas Haus fehlen (siehe C11, D-Liste) | – | – |
| B6 | Weitere Spielzustände zentral verwaltet | offen | VOLLSTÄNDIG UMGESETZT | `G1C_GameState` (Title, Conversation, ListMenu, Phone, Map …) nur im `G1C_GameManager` gesetzt | – | – | – |
| B7 | 180x180-Vorgabe in Phase 3 entfernt | offen | NICHT ÜBERPRÜFBAR | Dokumentänderung, Freigabe offen | mit B4 klären | N | Team |
| B8 | Regeln 22.1 (Währung, Uhr, Licht, Läden, Handy …) | offen | VOLLSTÄNDIG UMGESETZT | Systeme existieren (`G1C_MoneyManager`, `G1C_GameClock`, `G1C_Phone`, `G1C_SaveSystem`) | – | – | – |

## C. Sprint 1 – Kommentare aus Agents.md (Issues 1–17)

| # | Issue (gekürzt) | urspr. Status | tatsächlicher Status | Begründung / Klassen | Fehlend / nächste Schritte | Tests | Prio | Verantw. |
|---|-----------------|---------------|----------------------|----------------------|----------------------------|-------|------|----------|
| C1 | Türen sehen nicht gleich aus | teilweise | TEILWEISE UMGESETZT | gemeinsame Türgrafik; Render-Stichprobe Bekleidung/Zuhause einheitlich | übrige Gebäude visuell prüfen | Render-Stichprobe | N | _eintragen_ |
| C2 | Charaktere modernisieren, detailliertere Porträts (Bild 4/5) | teilweise | TEILWEISE UMGESETZT | Porträts aus 24x24-Sprite (`G1C_NpcSprites`); Referenzbilder 4/5 liegen nicht im Repo | Porträt-PNGs (Backlog 20) | – | M | Eduard Schoenfeld |
| C3 | Welt-Grafik ragt über Fenster | teilweise | TEILWEISE UMGESETZT | `ClipToVirtualRect` im Painter | Seitenverhältnisse 16:10 / 4:3 prüfen | nicht geprüft | M | _eintragen_ |
| C4 | Greifautomat mit Animation und Items | teilweise | TEILWEISE UMGESETZT | `G1C_ClawMachine` im Casino | laut neuer Vorgabe in die Arcade verschieben (C11); Gewinnpfad manuell prüfen | – | M | _eintragen_ |
| C5 | Komplexere Gespräche, Tagesablauf, unterschiedliche Dialoge je Charakter | offen | TEILWEISE UMGESETZT | NEU: `G1C_DialogueTopic` – eigene Fragen/Antworten je NPC, Antworten ändern sich bei Wiederholung und nach Tageszeit; genutzt von Mama, Oma, Jana, Olli, Mia | Casino-NPCs und Stadtbewohner mit Themen ausstatten; Tagesabläufe (Backlog 19); "echte KI" nicht vorgesehen (externe Dienste nicht freigegeben) | NPC-Tests BESTANDEN | H | Patrick Lupierz / Eduard Schoenfeld |
| C6 | Bäume: man kann durchlaufen | teilweise | VOLLSTÄNDIG UMGESETZT (war FEHLERHAFT) | Ursache gefunden: `ObjectAt` liefert nur das oberste Objekt – ein nicht massiver Vogel auf einem Baum machte die Kachel begehbar. `G1C_CollisionManager.IsBlocked` prüft jetzt alle massiven Objekte | – | "Bäume, Gebäude, Pavillons und NPCs blockieren" BESTANDEN (vorher FEHLGESCHLAGEN) | – | Patrick Lupierz |
| C7 | NPC-Herausforderung zum Kartenspielen | teilweise | TEILWEISE UMGESETZT | Pokerduell über Gespräch (Rivalen-Optionen in `G1C_Conversation`) | Blackjack-Herausforderung, Ablauf manuell prüfen | – | M | Eduard Schoenfeld |
| C8 | Item-Icons, Verkaufsautomat, +5 %, F3 mit 12 %, Systemnachricht 5 s | teilweise | TEILWEISE UMGESETZT | `G1C_Shop.PriceWithMarkup`, `BuyVendingItem`, `NotifySystemMessage` (125 Ticks = 5 s) | Ausblenden der Nachricht und F3-Pfad manuell prüfen | Kauf/Aufschlag-Test BESTANDEN | N | _eintragen_ |
| C9 | Karte M / Menü B; Mama und Oma mit Nummer, eigenem Zuhause, 3 Katzen; Taschengeld-Chance | offen | VOLLSTÄNDIG UMGESETZT (3. Schritt: Omas Haus) | NEU: `G1C_FamilyNpcRoster` (Mama im Spielerhaus, Oma Gerda vor ihrem Wohnhaus mit 3 Katzen, beide von Beginn an im Handy), `G1C_FamilyPocketMoney` (nach Gespräch mit beiden, 1x pro Tag, 60 % → 25 Chips) | Innenraum für Omas Haus; Taschengeld-Tag wird nicht gespeichert | 4 NPC-Tests BESTANDEN | H | Patrick Lupierz |
| C10 | Startbildschirm | erledigt | VOLLSTÄNDIG UMGESETZT | `G1C_StartFlow`, `G1C_StartView` | nicht erneut manuell geprüft | – | – | _eintragen_ |
| C11 | Arcade mit 5 Automaten, Spiele anderer Gruppen | offen | VOLLSTÄNDIG UMGESETZT (2. Schritt) | Spielhalle (ehem. Kino) mit dunklem Neon-Boden/-Wänden; 4 Automaten mit eigenen Spielen (Sternenjäger im Galaga-Genre, Schlange, Ziegelbrecher, Pong-Duell gegen KI), Gast-Automat in der Mitte lädt Framework-Spiele anderer Gruppen dynamisch (`G1C_GuestGameCatalog`, seit 23.11.2026 ohne Obergrenze; eigene Automaten max. 9), Bild im Automatenfenster, Chips als Belohnung, Greifautomat umgezogen, Highscore-Tafel (gespeichert) | Bedienung im echten Fenster manuell prüfen | 9 Spielhallen-Tests BESTANDEN; 9 echte Gruppenspiele starten und zeichnen fehlerfrei | – | Patrick Lupierz |
| C12 | Alle Gebäude auf der Karte; dynamisch speicherbar | offen | TEILWEISE UMGESETZT | alle betretbaren Gebäude stehen auf der Karte; `G1C_SaveData` speichert NPCs, Aufgaben, Bank inkl. Zinstag (NEU) | laufende Minispiele und Familien-Taschengeld werden nicht gespeichert | Speichern/Laden im Zinstest BESTANDEN | M | _eintragen_ |
| C13 | Blackjack Split und Double | teilweise | TEILWEISE UMGESETZT | `PlayerSplit`, `PlayerDouble` vorhanden | Ablauf automatisiert testen | – | M | _eintragen_ |
| C14 | Minimap, Stadtkarte, Ortsname beim Darüberfahren | teilweise | TEILWEISE UMGESETZT | Karte + Klickauswahl; Hover nicht möglich: Framework liefert keine Mausbewegung (nur Klick) | – (Framework-Grenze, dokumentiert) | – | N | _eintragen_ |
| C15 | Aufwachzeit beim Schlafen wählen | teilweise | VOLLSTÄNDIG UMGESETZT (3. Schritt: Figur liegt im Bett) | Auswahl vorhanden; NEU: Schlafanimation (Abdunkeln + "Zzz", Steuerung gesperrt) und Tageswechsel mit Zinsen | Figur liegend im Bett zeigen; Animation manuell prüfen | Zinstest über Tageswechsel BESTANDEN | M | Patrick Lupierz |
| C16 | Bank und Geldhai | teilweise | TEILWEISE UMGESETZT | NEU: 2 % Zinsen/Tag ohne Doppelzahlung, Kontostand- und Kontoauszugszeile (`G1C_FinanceManager`) | Bank optisch verbessern; Ansprechen durch den Geldhai beim Vorbeilaufen prüfen | 3 Banktests BESTANDEN | H | Patrick Lupierz |
| C17 | Möbelgeschäft und Einrichten | teilweise | VOLLSTÄNDIG UMGESETZT (16.11.: Einrichtungsmodus, Limit 3, Tests – siehe H1) | `OpenFurnishMenu`, `PlaceFurniture`, Speicherung | Kauf/Platzierung manuell prüfen, Test ergänzen | – | N | _eintragen_ |

## D. Zusätzliche Anforderungen aus AGENTS.md (Arbeitsauftrag 15.11.2026) – Stand

Nicht Teil der 34 SprintPlanning-Einträge, aber für den Backlog ausgewertet:

| Anforderung | Status | Nachweis / Grund |
|-------------|--------|------------------|
| WASD zuverlässig, Stopp nach Loslassen, keine Bewegung in Dialogen | TEILWEISE UMGESETZT | Logik getestet; Fokus im echten Fenster manuell offen |
| Öffnungszeiten, CLOSED-Schild, Verkauf nach Ladenschluss gesperrt | VOLLSTÄNDIG UMGESETZT | Tests + Render; Tankstelle bleibt 24 h offen |
| Mutter und Oma (sarkastisch, eigene Dialoge) | VOLLSTÄNDIG UMGESETZT | Tests; Omas Innenraum offen (C9) |
| Individuelle Verkäufer-Dialoge | VOLLSTÄNDIG UMGESETZT | `G1C_ShopNpcRoster`, Test "Verkäuferin: Gespräch, dann Warenliste" |
| Schilder interaktiv, Poster/Sticker mit den vorgegebenen Texten | VOLLSTÄNDIG UMGESETZT | `G1C_InteractionType.Inspect`, Test BESTANDEN |
| Bank 2 % pro Tag, kein Doppelzins | VOLLSTÄNDIG UMGESETZT | Tests (1000 → 1020 → 1040) |
| Automatisches Testsystem nur auf Aufruf | VOLLSTÄNDIG UMGESETZT | `G1C_SelfTest`, Menü [B] + [F12] |
| Arcade, Claw Machine verschieben | NICHT IMPLEMENTIERT | siehe C11 |
| Tankstelle rot, 2 Zapfsäulen, Fensterverkauf | NICHT IMPLEMENTIERT | Gebäude existiert, Gestaltung fehlt |
| Fahrrad vor dem Haus, Fahrradladen, Farben, schnelleres Laufen | NICHT IMPLEMENTIERT | `G1C_Player.StepTicks` erlaubt höhere Geschwindigkeit (Basis vorhanden) |
| Vogelgrafik korrigieren | NICHT IMPLEMENTIERT | Fehlerbild nicht dokumentiert; Grafik nicht geändert |
| Bett, Teppiche, Innenwände links/rechts | NICHT IMPLEMENTIERT | – |
| Pavillon erkennbar, nur von unten begehbar | NICHT IMPLEMENTIERT | derzeit massiver 4x4-Block |
| Schlafanimation | TEILWEISE UMGESETZT | Overlay-Animation, keine liegende Figur |

## E. Ideen-Sortierung

- **Bereits umgesetzt:** Taschengeld-Chance (C9), CLOSED-Schilder, tageszeitabhängige Antworten, Kontoauszug, Wiederholungsantworten.
- **Noch offen:** Arcade (C11), Fahrrad, Tankstellen-Neugestaltung, Pavillon, Innenwände/Bett/Teppiche, Omas Innenraum, NPC-Tagesabläufe (Backlog 19), Wetter (30), Friseur (29).
- **Abhängig von anderen Funktionen:** Claw Machine verschieben → braucht Arcade; Fahrrad-Geschwindigkeit → braucht Inventar-Item "Fahrrad"; Spiele anderer Gruppen in der Arcade → braucht deren Freigabe.
- **Nicht übernommen:** "echte KI als Option" (C5) – würde externe Dienste/Bibliotheken erfordern, die laut Agents.md nicht freigegeben sind; Hover-Anzeige auf der Karte (C14) – Framework liefert keine Mausbewegung, Änderung am Framework wäre nötig.


## F. Nachtrag 15.11.2026 – 2. Arbeitsschritt (Wünsche von Patrick Lupierz)

| Wunsch | Status | Nachweis |
|--------|--------|----------|
| Mama und Oma werden nie Gegner | VOLLSTÄNDIG UMGESETZT | `G1C_NpcPersonality.IsFamily`, `G1C_NpcRelationship.Classify`; Test "werden auch nach Beleidigungen nie Gegner" |
| Taschengeld fragen; wer schon gegeben hat, sagt Nein | VOLLSTÄNDIG UMGESETZT | einmal pro Spieltag je Person (Mama 25, Oma 20 Chips), danach Nein; Tag in `G1C_NpcMemory.PocketMoneyDay` gespeichert; Test inkl. Speichern/Laden. Ersetzt die frühere Regel aus C9 ("mit beiden gesprochen, 60 %") |
| Bank: Mitarbeiter | VOLLSTÄNDIG UMGESETZT | Bankberater Karl (Konto + eigene Themen), Wachmann Bodo, Bankierin Frida (jetzt sichtbar: Schalter mit Durchreiche) |
| Stühle, Tische, Betten realistischer | VOLLSTÄNDIG UMGESETZT (Grafik) | neue Pixel-Art für Stuhl, Tisch, Bett, Krankenbett, Theke; gestreckte Sitzbänke durch Stühle ersetzt; Render geprüft |
| Fahrradladen mit Fahrrad auf dem Dach | VOLLSTÄNDIG UMGESETZT | `building_bike_shop`, Innenraum mit 5 ausgestellten Rädern, Rudi; Räder kaufbar (einmalig), im Inventar, fahrbar (3 statt 5 Ticks/Feld), Ständer vor dem Haus |
| Pavillon unterbegehbar | VOLLSTÄNDIG UMGESETZT | Rückseite + Säulen massiv, Mitte begehbar, Dach wird über der Figur gezeichnet; Test + Render |
| Arcade mit allen Implementierungen | VOLLSTÄNDIG UMGESETZT | siehe C11 |

Weiterhin offen: Omas Innenraum, Innenwände links/rechts, Teppiche, Vogelgrafik, rote Tankstelle mit Zapfsäulen,
liegende Schlaffigur, manuelle Prüfung im Hauptfenster.

## G. Nachtrag 15.11.2026 – 3. Arbeitsschritt ("alles Offene bearbeiten")

| Offener Punkt | Status | Nachweis |
|---------------|--------|----------|
| Omas Haus (C9) | VOLLSTÄNDIG UMGESETZT | Wohnhaus neben dem Zuhause ist "Omas Haus" (lila Dach, betretbar). Innen: Oma, ihre drei Katzen Schnurrbert, Mausi, Herr Doktor Krallmann (animiert), Sessel, Teetisch, Bett, Bücherregal, Katzenbild, Blümchentapete, Dielen. Oma steht nicht mehr draußen. Tests "Omas Haus ist betretbar …", "Mama und Oma existieren …" |
| Innenwände links/rechts | VOLLSTÄNDIG UMGESETZT | Seiten- und Unterwände zeigen jetzt die Mauerkrone in Raumfarbe (`tile_wallside_<raum>_<l/r/b>`) statt der gedrehten Rückwand – in allen Innenräumen und im Zuhause. Test + Render |
| Teppiche | VOLLSTÄNDIG UMGESETZT | Teppiche aus Rand-/Eck-/Mittelteilen mit Bordüre, Fransen und Rautenmuster (`G1C_TileMask`), Farbe je Raum; neuer Teppich im Zuhause |
| Poster und Wanddekoration | VOLLSTÄNDIG UMGESETZT | Jeder Innenraum hat ein eigenes, untersuchbares Wandbild (Gesundheit, Angebote, Urlaub, Mode, Zinsen, Berge, Radrennen, Katzen); Zuhause zusätzlich Urlaubsfoto. Test: keine Wiederholung |
| Vogelgrafik | VOLLSTÄNDIG UMGESETZT | Fehlerbild: Vogel lag als Fleck ohne Kontur auf dem Baumstamm am Boden. Jetzt Rotkehlchen mit Kontur, sitzt auf der Kronenspitze. Test + Render |
| Rote Tankstelle, 2 Zapfsäulen, Fensterverkauf | VOLLSTÄNDIG UMGESETZT | Gebäude und Innenwände rot, zwei Zapfsäulen (untersuchbar, massiv), Verkaufsfenster (`G1C_ShopWindow`) an der Fassade: Gespräch mit Tankwart Olli und Einkauf ohne Betreten, rund um die Uhr. Test (Kauf um 3 Uhr) |
| Liegende Schlaffigur | VOLLSTÄNDIG UMGESETZT | Während der Schlafanimation liegt die Figur gedreht im Bett unter der Decke; Steuerung gesperrt, danach frei. Test + Render |
| Manuelle Prüfung im Hauptfenster (A9) | NICHT ÜBERPRÜFBAR | Der KI-Agent kann das WPF-Fenster nicht bedienen. Ersatz: Logiktests (49/49), Offscreen-Renderings mit dem echten Painter, 9 echte Gruppenspiele im Gast-Automaten. Checkliste: `Plans/Testbericht_2026-11-15.md` |

Damit sind alle bekannten offenen Implementierungsaufgaben umgesetzt. Offen bleiben nur Punkte, die außerhalb des Codes
liegen: Freigaben durch Team/Dozent (B1, B4, B7) und der manuelle Durchlauf im Hauptfenster (A9).

## H. Neue SprintPlanning-Punkte vom 16.11.2026 (Ideen: Patrick)

Quelle: Ende von `Scrum/SprintPlanning.md` (Stand 16.11.2026). Prüfung: Code-Durchsicht, Selbsttest **64/64 BESTANDEN**
(15 neue Tests) und Offscreen-Renderings mit dem echten Painter. Bedienung im Hauptfenster: siehe Checkliste im Testbericht.

| # | Punkt | Status | Umsetzung / Nachweis |
|---|-------|--------|----------------------|
| H1 | Möbel über klickbare, sich aktualisierende Hausansicht platzieren; max. 3 je Möbel; im Platzierungsmodus verschiebbar | VOLLSTÄNDIG UMGESETZT | `G1C_FurnitureEditor` + Zustand `FurnitureEditor`: echtes Zuhause mit Raster, Vorschau grün/rot, Klick oder [E] setzt, eigenes Möbel anklicken = aufheben/verschieben, [R] einlagern, [N] nächstes, [Q] fertig (aufgehobenes Möbel kehrt zurück). `G1C_Shop.MaxFurniturePerItem = 3` (Inventar + aufgestellt). Tests "Kauflimit", "Einrichtungsmodus" |
| H2 | Tiere mit Sprechblasen (typische Laute), nicht begehbar, laufen draußen umher; Katzen streicheln; 5 Katzenvarianten (u. a. Calico, Orange, Schwarz); Hunde, streichelbar | VOLLSTÄNDIG UMGESETZT | `G1C_PetAnimal` (Katzen grey/calico/orange/black/white, Hunde brown/dalmatian/golden), laufen im Revier (Radius 4), massiv; `G1C_AnimalVoices` (Miau, Wuff, Quak, Tschilp, Blubb); [E] streicheln = Herz-Blase. Enten massiv. Omas Katzen sind ebenfalls Haustiere. Tests "Katzen und Hunde", "Streicheln" |
| H3 | Sounds: Hintergrundmusik (Läden, Casino …), Gewinn-Sounds, allgemeine Geräusche | VOLLSTÄNDIG UMGESETZT (Hörprobe im Fenster offen) | `G1C_SoundSystem` + `G1C_ChipTune`: 7 Musikstücke (Stadt, Zuhause, Laden, Casino, Spielhalle, Angeln, Titel) und 10 Geräusche, im Code erzeugt (keine Dateien, keine Bibliothek), WPF-`MediaPlayer`. [T] im Menü = Ton an/aus. Im Selbsttest stumm. Test "Sound" prüft Zuordnung und WAV-Daten; ob es gut klingt, kann der Agent nicht hören |
| H4 | Fahrrad zeigt immer in eine Richtung; nach Haus betreten/verlassen muss zweimal geklickt werden | VOLLSTÄNDIG UMGESETZT (Fehler behoben) | Grafik je Richtung (links gespiegelt, rechts, Rück-, Frontansicht), Figur ohne Laufbeine auf dem Rad. Ursache Doppelklick: beim Betreten wurde abgestiegen, das Rad aber weiter als ausgerüstet geführt. Jetzt `ParkedAtDoor`: Rad wartet vor der Tür, beim Hinausgehen automatisch wieder aufsteigen; Inventar zeigt "fährst du" / "steht vor der Tür". Tests "Fahrrad …" |
| H5 | Standardsätze "Das machst du echt gut hier" / "Du kannst nicht spielen" nicht bei jedem NPC; mehr Themen | VOLLSTÄNDIG UMGESETZT | `G1C_SmallTalk`: Lob und Spitze passend zur Rolle (17 Rollen), Familie ohne Beleidigung. `G1C_ExtraTopics`: eigene Fragen für alle 11 Casino- und Stadt-NPCs, für alle (außer Familie) täglich wechselnder Stadtklatsch und Tageszeit-Frage; "Erzähl mir mehr …" im Plaudern. Test "Gespräche" |
| H6 | Angel-Minispiel auf der Brücke, Fische verkaufen, Fluss, Fischanimation, Platschen, Glücksfunde, graue Leiste mit grünem Abschnitt, der entgegen dem Fisch läuft | VOLLSTÄNDIG UMGESETZT | Angel auf jeder Flussbrücke (`AddFishingSpots`). `G1C_FishingGame`: Auswerfen, Biss, Anhauen, Drill mit schwimmendem Fisch (Ziele, Pfeilschüsse, Sprünge, Platschen), graue Leiste, grüner Bereich treibt gegen die Fischbewegung, [A]/[D] gegensteuern. 6 Fischarten + Stiefel, Flasche, Chipsbeutel, Ring, Glücksmünze; alles im Laden verkaufbar. Nachtrag: 20 Fischarten mit den von Patrick bereitgestellten, frei nutzbaren Fischgrafiken (`G1C_FishSprites`, als Pixeldaten im Code) |
| H7 | Tankstelle: Retro-Look rot/weiß, braune Holztür, breites Verkaufsfenster mit Markise und Theke, zwei rote Zapfsäulen auf grauen Steinplatten | VOLLSTÄNDIG UMGESETZT | `GasStationBuilding` (TownSprites), Verkaufsfenster 2 Kacheln mit Markise/Theke, Steinplatten `tile_city_plaza`, Zapfsäulen darauf. Test "Tankstelle: Retro-Look" + Render |
| H8 | Poker: eigene Hand tiefer, im Halbkreis, 5 % größer | VOLLSTÄNDIG UMGESETZT | `G1C_CardView.DrawFan` (Drehung um gemeinsamen Punkt, `G1C_Renderer.BeginRotation`), Skalierung 1,05. Test + Render |
| H9 | Admin-Cheat: 100 % Jackpot; Münzregen abhängig vom Einsatz | VOLLSTÄNDIG UMGESETZT | Tastenfolge geändert (Rückmeldung Patrick: A B A B scheitert an [B] = Menü): jetzt ADMIN. `G1C_CheatCode` (an/aus), `G1C_Slots.GuaranteedJackpot`, Münzregen `CoinsForBet` (20 + Einsatz/2, max. 240), Jackpot-Schriftzug und -Sound. Test "Admin-Cheat" + Render |
| H10 | Umzugsschild des Greifautomaten entfernen | VOLLSTÄNDIG UMGESETZT | Schild aus `G1C_CasinoWorld` entfernt. Test |
| H11 | Pavillon von jeder Seite betretbar | VOLLSTÄNDIG UMGESETZT | Nur noch die vier Ecksäulen massiv. Test angepasst ("von jeder Seite betretbar") |

Hinweise: Die Statusänderungen von A9 und B1 stammen aus der SprintPlanning (vom Team auf "erledigt" gesetzt).
Offen bleiben B4/B7 (Stadtgröße, Entscheidung Team/Dozent) sowie eine Hör- und Spielprobe der neuen Punkte im Hauptfenster.

## I. Nachtrag 18.11.2026 – offene Issues

Prüfung: Selbsttest **70/70 BESTANDEN** (6 neue Tests), Offscreen-Renderings mit dem echten Painter.

| Punkt | Status | Umsetzung / Nachweis |
|-------|--------|----------------------|
| Gast-Automat: Spiele anderer Gruppen mit deren Regeln, Painter und KI | VOLLSTÄNDIG UMGESETZT | `G1C_GuestGameCatalog`: Auswahl nach Gruppen-Präfix des Klassennamens (G10_, FG_, Gruppe2_ …), bei mehreren Spielen einer Gruppe nach gemeinsamem Klassennamen. Ursache für hängende Spiele: mehrere Gruppen lesen `OOPGamesManager.Singleton.ActivePainter/ActiveRules/ActivePlayers`; der Host meldet die fremden Teile jetzt wie das Hauptfenster als aktiv an und stellt danach den alten Stand wieder her. Die KI wird bei Animationen (Gruppe 10) erneut gefragt. Ergebnis kommt nur aus den Regeln der Gruppe. [Esc] führt immer zur Spielauswahl zurück. Test: alle 9 registrierten Spiele (8x 4 gewinnt + TicTacToe) komplett bis zum Ende gespielt |
| Fisch-Skins im Fluss wie im Angelspiel | VOLLSTÄNDIG UMGESETZT | `river_fish_<art>_<bild>` aus `G1C_FishSprites`. Test + Render |
| Issue 5 Tagesablauf | TEILWEISE UMGESETZT | `G1C_NpcSchedule`: Paul, Sam, Lea nachts zu Hause, Lena abends im Casino, Viktor 10–4 Uhr; Wechsel nur außer Sichtweite. "Echte KI" nicht umgesetzt (externe Dienste nicht freigegeben). Test |
| Issue 16 Geldhai spricht an | VOLLSTÄNDIG UMGESETZT | Sprechblase + Hinweis bei Abstand ≤ 2, einmal pro Vorbeigehen. Test |
| Issue 3 Seitenverhältnisse | VOLLSTÄNDIG UMGESETZT | 16:10 und 4:3 gerendert, kein Überlauf |
| Nachtrag B3, B5, B6, B8 | VOLLSTÄNDIG UMGESETZT | Status in SprintPlanning nachgezogen (war bereits umgesetzt) |

Weiter offen: Stadtgröße (B4/B7, Team/Dozent), Hörprobe Sound, sonstige "manuell prüfen"-Punkte im Hauptfenster.

## J. Nachtrag 19.11.2026 – Klangpaket und Handy-Mixer

Prüfung: Selbsttest **80/80 BESTANDEN** (10 neue Klangtests), alle 49 Dateien mit dem echten WPF-MediaPlayer geöffnet (stumm),
Handy-Seite mit Headless-Chrome gerendert. Hören und Bedienen auf einem echten Handy konnte der Agent nicht.

| Punkt | Status | Umsetzung / Nachweis |
|-------|--------|----------------------|
| Alle Sounds des Pakets implementieren (Musik, Ambiente, Tiere, Casino, Schritte, Allgemein) | VOLLSTÄNDIG UMGESETZT | 49 Dateien in `Assets/Sounds` (OGG → WAV, lange Aufnahmen → MP3, 291 MB → 28 MB), Katalog mit 38 Sounds. Test: jede Datei gehört zu einem Sound; jeder Sound hat Datei oder eingebaute Melodie. Doppelte Datei „Oustide 3“ und OGG/MP3-Doppelung bei `slot_win_small` nicht doppelt abgelegt |
| Handy-App für alle Einstellungen | VOLLSTÄNDIG UMGESETZT (als Web-App) | Menü B → F11: Server + PIN, Seite im Handy-Browser (Startbildschirm-fähig). Gesamt/Musik/Effekte, je Sound Lautstärke, Häufigkeit (Chance, Mindestabstand), Ein/Aus, Probehören, Stumm, Zurücksetzen, Vogel-Radius, Höchstzahl gleichzeitig. Keine native App (APK/IPA) |
| Lautstärke anpassen | VOLLSTÄNDIG UMGESETZT | Endlautstärke = Gesamt × Musik/Effekte × Einzelwert, live änderbar, gespeichert; Effekte auf −3 dBFS Spitzenpegel angeglichen (Gras-Schritte im Original unhörbar) |
| Häufigkeit | VOLLSTÄNDIG UMGESETZT | je Sound Wahrscheinlichkeit und Mindestabstand |
| Mehrere Sounds nicht gleichzeitig | VOLLSTÄNDIG UMGESETZT | einstellbar 1–8; Prioritäten verdrängen; derselbe Sound nie doppelt; Musik zählt nicht |
| Tiere nur beim Streicheln, Vögel nur im 4-Kacheln-Radius | VOLLSTÄNDIG UMGESETZT | Test: 1500 Ticks ohne Streicheln = kein Tiergeräusch; Vogel hörbar im Radius, nicht außerhalb; Radius einstellbar. Frosch-Sound liegt bereit, aber es gibt noch keinen Frosch im Spiel |
| Schritte nach Dateinamen und Boden, in Schleife | VOLLSTÄNDIG UMGESETZT | Untergrund aus der Kachelgrafik (Gras, Weg/Straße/Steinplatten, Brücke, innen); Varianten (1)(2)(3) reihum; Fahrrad: Schleife statt Schritte |
| Dateien in Ordnern unter Assets | VOLLSTÄNDIG UMGESETZT | Sounds: `Assets/Sounds/*`, Code: `Assets/SoundMixer/*` |

Grenzen: Schleifen mit dem WPF-MediaPlayer haben eine kurze Pause beim Neustart (bei 0,7-s-Schleifen wie dem Fahrrad hörbar
möglich). Die Handy-App braucht WLAN im selben Netz und eine Firewall-Freigabe. Die Lautstärke-Standardwerte sind nach Pegel
geschätzt – der Feinschliff nach Gehör steht noch aus.
