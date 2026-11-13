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
| 1 | Türen sehen nicht gleich aus | Phase 4 (Spielfigur), Zeile 1011 | _eintragen_ | _eintragen_ | offen |
| 2 | Charaktere etwas modernisieren; in Gesprächen detaillierter als in der Spielwelt (wie Interaktionen in Pokémon) | Phase 4 (Spielfigur), Zeile 1012 | _eintragen_ | _eintragen_ | offen |
| 3 | Welt-Grafik reparieren: Häuser tauchen vollständig auf und ragen über das Fenster; Bäume ebenso; beim Laufen wird das Fenster durch Grafiken verschoben und ragt über den vorgesehenen Bereich hinaus | Phase 5 (Grafiksystem), Zeile 1021 | _eintragen_ | _eintragen_ | offen |
| 4 | Funktionsfähiger Greifer (Claw-Machine) als Minispiel, mit dem man Chips/Items greifen kann; richtige Animation und richtiges Item-Aussehen | Phase 7 (Minispiele), Zeile 1036 | _eintragen_ | _eintragen_ | offen |
| 5 | Komplexere Gespräche, die den Beziehungsstatus mit den KIs verbessern und verschlechtern können; echte KI als Option; KIs sollen einen Tagesablauf haben (z. B. abends nach Hause oder ins Casino) | Phase 8 (KI), Zeile 1043 | _eintragen_ | _eintragen_ | offen |
| 6 | Kollision durch Bäume nicht vollständig behandelt | Abschnitt 21, Zeile 1076 | _eintragen_ | _eintragen_ | offen |
| 7 | Herausforderungsfunktion im Casino zum Kartenspielen (Poker / Blackjack) | Abschnitt 21, Zeile 1080 | _eintragen_ | _eintragen_ | offen |
| 8 | Items haben kein Aussehen im Inventar und im Laden; weitere Implementation: Verkaufsautomaten (z. B. Kaffee), Laden-Items für 5 % Aufpreis; Taste F3 für das Produkt mit 12 % Chance auf ein doppeltes Item; Systemnachricht („Du hast Glück beim Runterfallen wurde ein weiteres Item runtergestoßen“) 5 s sichtbar, dann verblassen | Abschnitt 22.1, Zeile 1111 | _eintragen_ | _eintragen_ | offen |
| 9 | Fehlende App Karte (Taste K besser wäre M; Menü unter z. B. B = Bedienungen); Telefonnummern sollten schon existieren: Mama und Oma als eigene Charaktere mit eigenem Zuhause, das sie nicht verlässt, passendem Aussehen, z. B. 3 Katzen; Häuser mit Einrichtung und festen Wänden; eigenes Haus mit Mutter (gleiche Funktion, ohne Haustiere); Telefon-Charaktere mit dynamischeren Antworten, z. B. mit einer Chance Taschengeld, wenn man mit beiden spricht | Abschnitt 22.1, Zeilen 1113–1114 | _eintragen_ | _eintragen_ | offen |
| 10 | Startbildschirm nicht vollständig ausgereift | Abschnitt 22.1, Zeile 1116 | _eintragen_ | _eintragen_ | offen |

| 11 | Arcade gebäude mit spielen wie galaga und 90er spiele max 9 stück pro arcade Spielautomat 5 spiele automaten mit einem der in der mitte ist in dem man die spiele der anderen gruppen spielen kann, spiel automat sichtbar als fenster und innnenrdrin Spiele der anderen gruppen momentan noch dynamisch da gruppen datein noch nicht offen sind mit belohnung von paar chips fürs spielen. Arcade aussehen dunkel gehalten boden und wände, wände mit deko passend zu arcade. 


|12| alle gebäude müssen auf der karte existieren alle spiele und spielstände sollten dynamisch speicherbar sein wenn ein spiel oder eine Ki nie angesprochen wurde einfach standard werte behalten und erst beim speichern zum fortschritt hinzufügen.