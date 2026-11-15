# Sprint Planning – Casino (Spiel 2)

## Sprint 1 (Start 10.11.2026)

Sprintziel: Lauffähige erste Version – Casino erkunden und alle vier Minispiele im Framework spielen.

| Aufgabe | Backlog-ID | Verantwortlich | Status |
|---------|-----------|----------------|--------|
| Framework-Anbindung (Painter, Rules, Player) | 1 | _eintragen_ | erledigt |
| Welt, Kollision, Spielfigur, Animation | 1 | _eintragen_ | erledigt |
| Dialoge, HUD, Menü, Ergebnisanzeige | 2, 3, 8 | _eintragen_ | erledigt |
| Roulette + KI | 4 | _eintragen_ | erledigt |
| Blackjack + KI | 5 | _eintragen_ | erledigt |
| Poker + KI | 6 | _eintragen_ | erledigt |
| Spielautomat | 7 | _eintragen_ | erledigt |
| Austauschbare Grafiken | 9 | _eintragen_ | erledigt |
| Manueller Test im Hauptfenster | – | _eintragen_ | offen |

Kurzprotokolle der Abstimmungen: _noch eintragen_

## Sprint 1 – Nachtrag: Erweiterungen in Agents.md (12.11.2026)

Quelle: Änderungen von Patrick Lupierz in `Agents.md` (Abschnitte 0, 1, 8, 12, 20, 22). Harte Regeln bleiben unverändert.

| # | Kommentar / Inhalt | Agents.md-Stelle | Zugehörig | Bearbeiter | Status |
|---|--------------------|------------------|-----------|------------|--------|
| 1 | Neuer Abschnitt 0: Unterscheidung harte Regeln und erweiterbare Inhalte, Bedingungen für Erweiterungen | Abschnitt 0 | _eintragen_ | _eintragen_ | offen |
| 2 | Spielwelt-Einleitung: Stadt, Innenräume und Gebäude dürfen erweitert werden (Tankstelle, Bekleidungsgeschäft) | Abschnitt 1 | _eintragen_ | _eintragen_ | offen |
| 3 | Weitere getrennte Karten dürfen ergänzt werden | Abschnitt 8 | _eintragen_ | _eintragen_ | offen |
| 4 | Stadtgröße nicht festgelegt, muss zum Maßstab der Spielfigur passen (aktuell 96x64 statt 180x180) | Abschnitt 8.1 | _eintragen_ | _eintragen_ | offen – Entscheidung Team/Dozent |
| 5 | Weitere Innenräume für Krankenhaus, Supermarkt und weitere Gebäude | Abschnitt 8.3 | _eintragen_ | _eintragen_ | offen |
| 6 | Weitere Spielzustände (Hauptmenü, Gespräch, Listenmenü, Handy) erlaubt, zentral verwaltet | Abschnitt 12 | _eintragen_ | _eintragen_ | offen |
| 7 | 180x180-Vorgabe in Phase 3 entfernt | Abschnitt 20 | _eintragen_ | _eintragen_ | offen |
| 8 | Regeln für Erweiterungen: Währung, Uhr, Licht, Charakter, Läden, Handy, Speichern, lokale Wahrnehmung, Tasten | Abschnitt 22.1 | _eintragen_ | _eintragen_ | offen |

Offene Fragen an Team und Dozent: Freigabe der Lockerung in Abschnitt 0 und 8.1; Vorstellung des Spielkonzepts nach Abschnitt 18.

## Sprint 1 – Kommentare aus Agents.md als Issues

Quelle: Kommentare mit `Kommentar für issue` bzw. `//` in `Agents.md`. Sie wurden aus der Datei entfernt und hier übernommen.

| # | Issue-Inhalt | Fundstelle (vorher in Agents.md) | Zugehörig | Bearbeiter | Status |
|---|--------------|----------------------------------|-----------|------------|--------|
| 1 | Türen sehen nicht gleich aus | Phase 4 (Spielfigur), Zeile 1011 | _eintragen_ | _eintragen_ | teilweise – gemeinsame Türgrafik integriert, weitere Gebäude visuell prüfen |
| 2 | Charaktere graphisch modernisieren (siehe bild 4), sowohl allgemein, als auch auf den bildern in Gesprächen noch detaillierter (siehe bild 5) | Phase 4 (Spielfigur), Zeile 1012 | _eintragen_ | _eintragen_ | teilweise – Grafiken integriert, Gesprächsporträts noch visuell prüfen |
| 3 | Welt-Grafik reparieren: Häuser tauchen vollständig auf und ragen über das Fenster; Bäume ebenso; beim Laufen wird das Fenster durch Grafiken verschoben und ragt über den vorgesehenen Bereich hinaus | Phase 5 (Grafiksystem), Zeile 1021 | _eintragen_ | _eintragen_ | teilweise – bei kleinen und maximierten Fenstern ohne Überlauf geprüft; weitere Seitenverhältnisse offen |
| 4 | Funktionsfähiger Greifer (Claw-Machine) als Minispiel, mit dem man Chips/Items greifen kann; richtige Animation und richtiges Item-Aussehen | Phase 7 (Minispiele), Zeile 1036 | _eintragen_ | _eintragen_ | teilweise – Minispiel integriert, Gewinn-/Verlustpfad noch manuell prüfen |
| 5 | Komplexere Gespräche, die den Beziehungsstatus mit den KIs verbessern und verschlechtern können; echte KI als Option; KIs sollen einen Tagesablauf haben (z. B. abends nach Hause oder ins Casino), mehr auswahl an gesprächsoptionen und das die unterschiedlichen charaktere auch unterschiedliche dialoge führen können und nicht alle dieselben | Phase 8 (KI), Zeile 1043 | _eintragen_ | _eintragen_ | offen |
| 6 | Kollision durch Bäume nicht vollständig behandelt, man kann durchlaufen | Abschnitt 21, Zeile 1076 | _eintragen_ | _eintragen_ | teilweise – Kollisionslogik integriert, Durchlaufen noch manuell prüfen |
| 7 | NPC Herausforderungsfunktion im Casino zum Kartenspielen (Poker / Blackjack) | Abschnitt 21, Zeile 1080 | _eintragen_ | _eintragen_ | teilweise – Herausforderung integriert, Spielablauf noch manuell prüfen |
| 8 | Items haben kein Aussehen im Inventar und im Laden; weitere Implementation: Verkaufsautomaten (z. B. Kaffee), Laden-Items für 5 % Aufpreis; Taste F3 für das Produkt mit 12 % Chance auf ein doppeltes Item; Systemnachricht („Du hast Glück beim Runterfallen wurde ein weiteres Item runtergestoßen“) 5 s sichtbar, dann verblassen | Abschnitt 22.1, Zeile 1111 | _eintragen_ | _eintragen_ | teilweise – Icons und Automat integriert, Kauf-/Bonuspfad noch manuell prüfen |
| 9 | Fehlende App Karte (Taste K besser wäre M; Menü unter z. B. B = Bedienungen); Telefonnummern sollten schon existieren: Mama und Oma (siehe bild 1) als eigene Charaktere mit eigenem Zuhause, das sie nicht verlässt, passendem Aussehen, z. B. 3 Katzen; Häuser mit Einrichtung und festen Wänden; eigenes Haus mit Mutter (gleiche Funktion, ohne Haustiere); Telefon-Charaktere mit dynamischeren Antworten, z. B. mit einer Chance Taschengeld, wenn man mit beiden spricht | Abschnitt 22.1, Zeilen 1113–1114 | _eintragen_ | _eintragen_ | offen |
| 10 | Startbildschirm nicht vollständig ausgereift | Abschnitt 22.1, Zeile 1116 | _eintragen_ | _eintragen_ | erledigt |
| 11 | Arcade-Gebäude mit Spielen wie Galaga und 90er-Spielen; maximal neun Spiele pro Arcade-Spielautomat, fünf Automaten insgesamt und einer davon in der Mitte für dynamisch eingebundene Spiele anderer Gruppen (deren Dateien sind noch nicht offen). Die Spiele sollen im Automatenfenster sichtbar sein und ein paar Chips als Belohnung geben. Dunkler Arcade-Boden und dunkle Wände mit passender Dekoration. | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | offen |
| 12 | Alle Gebäude müssen auf der Karte existieren. Alle Spiele und Spielstände sollen dynamisch speicherbar sein; wenn ein Spiel oder eine KI noch nie angesprochen wurde, Standardwerte behalten und erst beim Speichern in den Fortschritt aufnehmen. | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | offen |
| 13 | Beim Blackjack im Casino Optionen wie Split und Double hinzufügen | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | teilweise – Optionen integriert, Spielablauf noch manuell prüfen |
| 14 | Minimap mit Straßen und nahegelegenen Gebäuden sowie große Stadtkarte; Ortsbezeichnung beim Darüberfahren | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | teilweise – Karten und Gebäudeauswahl per Klick umgesetzt; das Framework liefert keine Mausbewegungs-Eingabe für Hover |
| 15 | Im eigenen Zuhause beim Schlafengehen die Aufwach-Uhrzeit auswählen | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | teilweise – Auswahl integriert, Aufwachzeit noch manuell prüfen |
| 16 | Bank in der Welt; vor der Bank wartet ein Geldhai, spricht den Spieler beim Vorbeilaufen an und will ihm Geld andrehen | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | teilweise – Bank/Kredit integriert, NPC-Ansprache und Transaktionen noch manuell prüfen |
| 17 | Möbelgeschäft, in dem Möbel für das eigene Zuhause gekauft werden können, sowie die Möglichkeit, das Zuhause einzurichten | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | teilweise – Kauf/Platzierung/Speicherung integriert, noch manuell prüfen |

Build: `dotnet build OOPGames\OOPGames.csproj --no-restore` erfolgreich (0 Warnungen, 0 Fehler).
Manuell geprüft: Startansicht, Zuhause/Stadt-Übergang und Stadtkarte; zusätzlich kleine und maximierte Fenster.
Noch offen: vollständige Prüfung der Seitenverhältnisse 16:10 und 4:3 sowie die Funktionspfade für Greifer,
Bank/Kredit und Möbelkauf/-platzierung.