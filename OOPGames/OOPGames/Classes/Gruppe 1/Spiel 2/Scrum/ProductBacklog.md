# Product Backlog – Casino (Spiel 2)

Product Owner: _noch eintragen_
Scrum Master: _noch eintragen_

| ID | User Story | Status |
|----|------------|--------|
| 1 | Als Spieler möchte ich mich mit einer animierten Figur frei im Casino bewegen, ohne durch Wände und Möbel zu laufen. | erledigt (V1) |
| 2 | Als Spieler möchte ich NPCs ansprechen und über einen Dialog ein Casino-Spiel starten. | erledigt (V1) |
| 3 | Als Spieler möchte ich meinen Kontostand jederzeit sehen. | erledigt (V1) |
| 4 | Als Spieler möchte ich Roulette gegen einen KI-Mitspieler spielen (Zahlen, Farben, Bereiche). | erledigt (V1) |
| 5 | Als Spieler möchte ich Blackjack mit Hit/Stand gegen Dealer und KI spielen. | erledigt (V1) |
| 6 | Als Spieler möchte ich Poker gegen eine KI spielen, die foldet, callt und erhöht. | erledigt (V1) |
| 7 | Als Spieler möchte ich an einem Spielautomaten mit drei Walzen spielen. | erledigt (V1) |
| 8 | Als Spieler möchte ich nach einem Minispiel Gewinn/Verlust sehen und ins Casino zurückkehren. | erledigt (V1) |
| 9 | Als Team möchten wir alle Grafiken austauschen können, ohne Code zu ändern. | erledigt (V1) |
| 10 | Als Spieler möchte ich eine größere Karte mit Kamera und laufenden NPCs. | offen |
| 11 | Als Spieler möchte ich beim Roulette mehrere Einsätze pro Runde platzieren. | offen |
| 12 | Als Spieler möchte ich beim Blackjack verdoppeln und teilen. | offen |
| 13 | Als Team möchten wir eigene Pixel-Art-Grafiken (z.B. 90x90) für Figur und Möbel einbauen. | offen |
| 14 | Als Spieler möchte ich mit NPCs Gespräche mit Antwortoptionen führen und ihr Porträt mit Gesichtsausdruck sehen. | erledigt (Eduard Schoenfeld) |
| 15 | Als Spieler möchte ich, dass NPCs mir vertrauen, mich respektieren und zu Freunden, Rivalen oder Gegnern werden – und sich entsprechend verhalten. | erledigt (Eduard Schoenfeld) |
| 16 | Als Spieler möchte ich, dass NPCs sich an Beleidigungen, Hilfe und Pokerduelle erinnern. | erledigt (Eduard Schoenfeld) |
| 17 | Als Spieler ohne Chips möchte ich NPCs um Geld bitten oder durch kleine Aufgaben Geld verdienen. | erledigt (Eduard Schoenfeld) |
| 18 | Als Spieler möchte ich kurze Monologe und Reaktionen der NPCs auf Gewinne und Verluste sehen. | erledigt (Eduard Schoenfeld) |
| 19 | Als Spieler möchte ich, dass NPCs im Casino umhergehen (Tagesablauf). | offen |
| 20 | Als Team möchten wir 30 detaillierte Porträt-PNGs (5 NPCs × 6 Ausdrücke) einbauen. | offen |
| 21 | Als Spieler möchte ich meine Figur in der Stadt sehen und über natürlich verlaufende Straßen laufen. | erledigt (Patrick Lupierz) |
| 22 | Als Spieler möchte ich beim Start einen Namen und mein Aussehen wählen. | erledigt (Patrick Lupierz) |
| 23 | Als Spieler möchte ich eine Uhrzeit mit Tag-Nacht-Wechsel erleben (ein Tag = 30 Minuten). | erledigt (Patrick Lupierz) |
| 24 | Als Spieler möchte ich in Tankstelle, Supermarkt und Bekleidungsgeschäft mit Chips einkaufen und Gekauftes im Inventar finden. | erledigt (Patrick Lupierz) |
| 25 | Als Spieler möchte ich gekaufte Kleidung an meiner Figur sehen. | erledigt (Patrick Lupierz) |
| 26 | Als Spieler möchte ich ein Klapphandy mit Kontakten, Anrufen, SMS, Uhr, Aufgaben und Speichern benutzen. | erledigt (Patrick Lupierz) |
| 27 | Als Spieler möchte ich auf drei Speicherplätzen speichern und im Hauptmenü laden. | erledigt (Patrick Lupierz) |
| 28 | Als Spieler möchte ich Mülleimer durchsuchen; nur NPCs in der Nähe bemerken es. | erledigt (Patrick Lupierz) |
| 29 | Als Spieler möchte ich zum Friseur gehen und Möbel für mein Haus kaufen. | offen |
| 30 | Als Spieler möchte ich wechselndes Wetter erleben. | offen |

## Ergänzung 15.11.2026 (Patrick Lupierz, KI-gestützt)

Quelle: `Plans/SprintPlanning_Issue_Auswertung.md` und Arbeitsauftrag vom 15.11.2026. Bestehende Einträge oben bleiben
unverändert; Statusänderungen stehen hier. Doppelte Wünsche wurden zusammengeführt (Verweis in Klammern).

### Statusänderungen bestehender Stories

| ID | Neuer Stand |
|----|-------------|
| 1 | Kollisionsfehler "durch Bäume laufen" behoben; WASD-Logik repariert (Halten/Loslassen). Manuelle Fensterprüfung offen. |
| 19 | weiterhin offen (zusammengeführt mit SprintPlanning C5 "Tagesablauf"). |
| 29 | Möbelteil vorhanden (SprintPlanning C17); Friseur offen. |

### Neue User Stories

| ID | User Story | Akzeptanzkriterien | Prio | Abhängig von | Verantw. | Status |
|----|------------|--------------------|------|--------------|----------|--------|
| 31 | Als Spieler möchte ich, dass die Figur bei gehaltener WASD-Taste flüssig läuft und beim Loslassen sofort stehen bleibt. | kein Anfangsruckeln; kein Nachlaufen; keine Bewegung in Dialogen/Menüs; Pfeiltasten funktionieren nach Spielstart | H | – | Patrick Lupierz | umgesetzt, manuelle Prüfung offen |
| 32 | Als Spieler möchte ich, dass Läden nach Ladenschluss geschlossen sind und ein CLOSED-Schild zeigen. | Tür bleibt zu mit Hinweis; kein Verkauf bei offenem Menü nach Ladenschluss; Schild verschwindet bei Öffnung; Tankstelle 24 h | H | – | Patrick Lupierz | erledigt (Tests) |
| 33 | Als Spieler möchte ich, dass mein Bankguthaben pro Spieltag um 2 % wächst. | 1000 → 1020 nach einem Tag; kein Doppelzins am selben Tag; auch nach Schlafen und Laden korrekt; Kontoauszug sichtbar | H | – | Patrick Lupierz | erledigt (Tests) |
| 34 | Als Spieler möchte ich mit meiner Mutter und meiner sarkastischen Oma sprechen. | eigene Grafik, eigene Fragen/Antworten, wechselnde Antworten, Nummer im Handy, Taschengeld-Chance nach Gespräch mit beiden (SprintPlanning C9) | H | 35 | Patrick Lupierz | erledigt; Omas Innenraum offen (→ 44) |
| 35 | Als Team möchten wir jedem NPC eigene Fragen und Antworten geben können. | `G1C_DialogueTopic` je Profil; Antworten nach Wiederholung/Tageszeit; max. 4 Optionen | H | – | Patrick Lupierz | erledigt; Casino-NPCs und Stadtbewohner noch ohne Themen |
| 36 | Als Spieler möchte ich Schilder, Poster und Sticker untersuchen. | [E] zeigt Text; Poster "Hier könnte Ihre Werbung stehen"; Sticker "Nett hier, aber waren Sie schon mal in Baden-Württemberg" | M | – | Patrick Lupierz | erledigt (Test) |
| 37 | Als Team möchten wir wichtige Funktionen per Selbsttest prüfen, der nur auf Aufruf startet. | Menü [B] + [F12]; Status BESTANDEN/FEHLGESCHLAGEN/NICHT AUSFÜHRBAR; Bericht-Datei | H | – | Patrick Lupierz | erledigt (30 Tests) |
| 38 | Als Spieler möchte ich eine Arcade mit Spielautomaten und dem Greifautomaten. | eigenes Gebäude, dunkler Boden/Wände, 5 Automaten, Claw Machine aus dem Casino verschoben, Chips als Belohnung (SprintPlanning C4, C11) | H | – | _eintragen_ | offen |
| 39 | Als Spieler möchte ich eine rote Tankstelle mit zwei Zapfsäulen und Verkauf durchs Fenster. | Außengrafik rot, 2 Zapfsäulen (Kollision), Verkäufer am Fenster ansprechbar, 24 h offen | M | – | _eintragen_ | offen |
| 40 | Als Spieler möchte ich ein Fahrrad besitzen, Farben kaufen und damit schneller fahren. | Fahrrad vor dem Haus; Fahrradladen mit Rot/Blau/Grün/Gelb; Inventar-Item; schnellere Bewegung über `G1C_Player.StepTicks` | M | 38 (neues Gebäude-Muster) | _eintragen_ | offen |
| 41 | Als Spieler möchte ich einen erkennbaren Pavillon, der nur von unten betreten werden kann. | eigene Dach-/Stützengrafik; Kollision nur an Seiten/Rückwand | N | – | _eintragen_ | offen |
| 42 | Als Spieler möchte ich schönere Innenräume. | Innenwände links/rechts überarbeitet, Bett und Teppiche verbessert, Poster je Raum | N | – | _eintragen_ | offen |
| 43 | Als Spieler möchte ich korrekte Vogelgrafiken. | Fehlerbild dokumentieren; Frames korrigiert | N | – | _eintragen_ | offen |
| 44 | Als Spieler möchte ich Oma in ihrem Haus mit drei Katzen besuchen. | Innenraum Wohnhaus, Oma drinnen, 3 Katzen | M | 34 | _eintragen_ | offen |
| 45 | Als Spieler möchte ich, dass sich die Figur zum Schlafen ins Bett legt. | liegende Figur im Bett während der Schlafanimation | N | – | _eintragen_ | offen (Abdunkel-Animation vorhanden) |
| 46 | Als Spieler möchte ich, dass Familien-Taschengeld und Minispielstände gespeichert werden. | Speichern/Laden ohne doppeltes Taschengeld (SprintPlanning C12) | N | 34 | _eintragen_ | offen |

## Ergänzung 2 – 15.11.2026 (Patrick Lupierz, KI-gestützt)

| ID | Neuer Stand |
|----|-------------|
| 34 | geändert auf Wunsch: Familie wird nie Gegner; Taschengeld einmal pro Tag je Person, danach "Nein" (gespeichert). Erledigt (Tests). |
| 38 | Spielhalle erledigt: 4 eigene Spiele, Gast-Automat für Spiele anderer Gruppen (9 gefunden und lauffähig), Greifautomat umgezogen, Highscores. Manuelle Fensterprüfung offen. |
| 40 | Fahrrad erledigt: Ständer vor dem Haus, Fahrradladen mit Rad auf dem Dach, 5 Farben, Inventar, schneller fahren, Absteigen in Gebäuden. |
| 41 | Pavillon erledigt: neue Grafik, nur von unten begehbar. |
| 42 | teilweise: Bett, Tisch, Stuhl, Krankenbett, Theke neu; Innenwände und Teppiche offen. |
| 47 | NEU – Als Spieler möchte ich in der Bank mit mehreren Mitarbeitern sprechen. Erledigt (Karl, Bodo, Frida). |

## Ergänzung 3 – 15.11.2026 (Patrick Lupierz, KI-gestützt)

| ID | Neuer Stand |
|----|-------------|
| 39 | Tankstelle erledigt: rot, zwei Zapfsäulen, Verkaufsfenster mit Olli (24 h). |
| 42 | Innenräume erledigt: Seitenwände, Teppiche aus Teilstücken, Wandbilder je Raum, Möbelgrafiken. |
| 43 | Vogelgrafik erledigt: sitzt auf der Baumkrone, mit Kontur. |
| 44 | Omas Haus erledigt: betretbar, Oma und drei Katzen wohnen darin. |
| 45 | Schlafen erledigt: Figur liegt während der Animation im Bett. |
| 46 | teilweise: Taschengeld-Tag wird gespeichert (NPC-Erinnerung); laufende Minispiele werden bewusst nicht gespeichert (Runde endet beim Verlassen). |
| – | Offen außerhalb des Codes: manueller Test im Hauptfenster; Freigaben (Agents.md Abschnitt 0, Stadtgröße). |
