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
| Manueller Test im Hauptfenster | – | _eintragen_ | erledigt |

Kurzprotokolle der Abstimmungen: _noch eintragen_

## Sprint 1 – Nachtrag: Erweiterungen in Agents.md (12.11.2026)

Quelle: Änderungen von Patrick Lupierz in `Agents.md` (Abschnitte 0, 1, 8, 12, 20, 22). Harte Regeln bleiben unverändert.

| # | Kommentar / Inhalt | Agents.md-Stelle | Zugehörig | Bearbeiter | Status |
|---|--------------------|------------------|-----------|------------|--------|
| 1 | Neuer Abschnitt 0: Unterscheidung harte Regeln und erweiterbare Inhalte, Bedingungen für Erweiterungen | Abschnitt 0 | _eintragen_ | _eintragen_ | erledigt |
| 2 | Spielwelt-Einleitung: Stadt, Innenräume und Gebäude dürfen erweitert werden (Tankstelle, Bekleidungsgeschäft) | Abschnitt 1 | _eintragen_ | _eintragen_ | erledigt |
| 3 | Weitere getrennte Karten dürfen ergänzt werden | Abschnitt 8 | _eintragen_ | _eintragen_ | erledigt – getrennte Karten über IG1C_WorldScene/G1C_SceneTransitionManager (Zuhause, Stadt, Casino, 10 Innenräume) |
| 4 | Stadtgröße nicht festgelegt, muss zum Maßstab der Spielfigur passen (aktuell 96x64 statt 180x180) | Abschnitt 8.1 | _eintragen_ | _eintragen_ | offen – Entscheidung Team/Dozent |
| 5 | Weitere Innenräume für Krankenhaus, Supermarkt und weitere Gebäude | Abschnitt 8.3 | _eintragen_ | _eintragen_ | erledigt – Krankenhaus, Supermarkt, Tankstelle, Bekleidung, Bank, Möbelhaus, Spielhalle, Fahrradladen, Omas Haus |
| 6 | Weitere Spielzustände (Hauptmenü, Gespräch, Listenmenü, Handy) erlaubt, zentral verwaltet | Abschnitt 12 | _eintragen_ | _eintragen_ | erledigt – alle Zustände zentral in G1C_GameState/G1C_GameManager |
| 7 | 180x180-Vorgabe in Phase 3 entfernt | Abschnitt 20 | _eintragen_ | _eintragen_ | offen |
| 8 | Regeln für Erweiterungen: Währung, Uhr, Licht, Charakter, Läden, Handy, Speichern, lokale Wahrnehmung, Tasten | Abschnitt 22.1 | _eintragen_ | _eintragen_ | erledigt – Chips, Uhr, Tageslicht, Charakter, Läden, Handy, Speichern, Tasten umgesetzt |

Offene Fragen an Team und Dozent: Freigabe der Lockerung in Abschnitt 0 und 8.1; Vorstellung des Spielkonzepts nach Abschnitt 18.

## Sprint 1 – Kommentare aus Agents.md als Issues

Quelle: Kommentare mit `Kommentar für issue` bzw. `//` in `Agents.md`. Sie wurden aus der Datei entfernt und hier übernommen.

| # | Issue-Inhalt | Fundstelle (vorher in Agents.md) | Zugehörig | Bearbeiter | Status |
|---|--------------|----------------------------------|-----------|------------|--------|
| 1 | Türen sehen nicht gleich aus | Phase 4 (Spielfigur), Zeile 1011 | _eintragen_ | _eintragen_ | teilweise – gemeinsame Türgrafik integriert, weitere Gebäude visuell prüfen |
| 2 | Charaktere graphisch modernisieren (siehe bild 4), sowohl allgemein, als auch auf den bildern in Gesprächen noch detaillierter (siehe bild 5) | Phase 4 (Spielfigur), Zeile 1012 | _eintragen_ | _eintragen_ | teilweise – Grafiken integriert, Gesprächsporträts noch visuell prüfen |
| 3 | Welt-Grafik reparieren: Häuser tauchen vollständig auf und ragen über das Fenster; Bäume ebenso; beim Laufen wird das Fenster durch Grafiken verschoben und ragt über den vorgesehenen Bereich hinaus | Phase 5 (Grafiksystem), Zeile 1021 | _eintragen_ | _eintragen_ | erledigt – Welt auf das Fenster begrenzt; 16:10 (1280x800) und 4:3 (1024x768) am 18.11. per Rendering geprüft, kein Überlauf |
| 4 | Funktionsfähiger Greifer (Claw-Machine) als Minispiel, mit dem man Chips/Items greifen kann; richtige Animation und richtiges Item-Aussehen | Phase 7 (Minispiele), Zeile 1036 | _eintragen_ | _eintragen_ | teilweise – Minispiel integriert, Gewinn-/Verlustpfad noch manuell prüfen |
| 5 | Komplexere Gespräche, die den Beziehungsstatus mit den KIs verbessern und verschlechtern können; echte KI als Option; KIs sollen einen Tagesablauf haben (z. B. abends nach Hause oder ins Casino), mehr auswahl an gesprächsoptionen und das die unterschiedlichen charaktere auch unterschiedliche dialoge führen können und nicht alle dieselben | Phase 8 (KI), Zeile 1043 | _eintragen_ | _eintragen_ | teilweise – eigene Themen je NPC, Beziehung steigt/sinkt, Tagesablauf (nachts zu Hause, Lena abends im Casino); "echte KI" nicht umgesetzt (externe Dienste nicht freigegeben) |
| 6 | Kollision durch Bäume nicht vollständig behandelt, man kann durchlaufen | Abschnitt 21, Zeile 1076 | _eintragen_ | _eintragen_ | teilweise – Kollisionslogik integriert, Durchlaufen noch manuell prüfen |
| 7 | NPC Herausforderungsfunktion im Casino zum Kartenspielen (Poker / Blackjack) | Abschnitt 21, Zeile 1080 | _eintragen_ | _eintragen_ | teilweise – Herausforderung integriert, Spielablauf noch manuell prüfen |
| 8 | Items haben kein Aussehen im Inventar und im Laden; weitere Implementation: Verkaufsautomaten (z. B. Kaffee), Laden-Items für 5 % Aufpreis; Taste F3 für das Produkt mit 12 % Chance auf ein doppeltes Item; Systemnachricht („Du hast Glück beim Runterfallen wurde ein weiteres Item runtergestoßen“) 5 s sichtbar, dann verblassen | Abschnitt 22.1, Zeile 1111 | _eintragen_ | _eintragen_ | teilweise – Icons und Automat integriert, Kauf-/Bonuspfad noch manuell prüfen |
| 9 | Fehlende App Karte (Taste K besser wäre M; Menü unter z. B. B = Bedienungen); Telefonnummern sollten schon existieren: Mama und Oma (siehe bild 1) als eigene Charaktere mit eigenem Zuhause, das sie nicht verlässt, passendem Aussehen, z. B. 3 Katzen; Häuser mit Einrichtung und festen Wänden; eigenes Haus mit Mutter (gleiche Funktion, ohne Haustiere); Telefon-Charaktere mit dynamischeren Antworten, z. B. mit einer Chance Taschengeld, wenn man mit beiden spricht | Abschnitt 22.1, Zeilen 1113–1114 | _eintragen_ | _eintragen_ | teilweise – Mama und Oma mit Nummer, eigenem Zuhause (Omas Haus), Katzen, Taschengeld-Chance; Tür-/Haus-Details visuell prüfen |
| 10 | Startbildschirm nicht vollständig ausgereift | Abschnitt 22.1, Zeile 1116 | _eintragen_ | _eintragen_ | erledigt |
| 11 | Arcade-Gebäude mit Spielen wie Galaga und 90er-Spielen; maximal neun Spiele pro Arcade-Spielautomat, fünf Automaten insgesamt und einer davon in der Mitte für dynamisch eingebundene Spiele anderer Gruppen (deren Dateien sind noch nicht offen). Die Spiele sollen im Automatenfenster sichtbar sein und ein paar Chips als Belohnung geben. Dunkler Arcade-Boden und dunkle Wände mit passender Dekoration. | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | erledigt – 5 Automaten, je max. 9 Spiele (Stern-Jäger, Schlange, Ziegelbrecher, Pong), Gast-Automat mit Spielen anderer Gruppen, Rekorde, Chip-Belohnung; Boden und Wände dunkel |
| 12 | Alle Gebäude müssen auf der Karte existieren. Alle Spiele und Spielstände sollen dynamisch speicherbar sein; wenn ein Spiel oder eine KI noch nie angesprochen wurde, Standardwerte behalten und erst beim Speichern in den Fortschritt aufnehmen. | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | teilweise – alle Gebäude auf der Karte; Spielstand speichert NPCs, Aufgaben, Bank; laufende Minispiele bewusst nicht gespeichert |
| 13 | Beim Blackjack im Casino Optionen wie Split und Double hinzufügen | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | teilweise – Optionen integriert, Spielablauf noch manuell prüfen |
| 14 | Minimap mit Straßen und nahegelegenen Gebäuden sowie große Stadtkarte; Ortsbezeichnung beim Darüberfahren | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | teilweise – Karten und Gebäudeauswahl per Klick umgesetzt; das Framework liefert keine Mausbewegungs-Eingabe für Hover |
| 15 | Im eigenen Zuhause beim Schlafengehen die Aufwach-Uhrzeit auswählen | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | erledigt – Aufwachzeit wählbar (06/08/12/18 Uhr), Figur liegt während des Schlafens im Bett |
| 16 | Bank in der Welt; vor der Bank wartet ein Geldhai, spricht den Spieler beim Vorbeilaufen an und will ihm Geld andrehen | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | erledigt – Bank mit 2 % Zinsen pro Tag; Geldhai Viktor spricht beim Vorbeilaufen an (Sprechblase + Hinweis, einmal pro Vorbeigehen) |
| 17 | Möbelgeschäft, in dem Möbel für das eigene Zuhause gekauft werden können, sowie die Möglichkeit, das Zuhause einzurichten | Agents.md (Nachtrag) | _eintragen_ | _eintragen_ | erledigt – Möbelhaus; Einrichtungsmodus mit klickbarer Hausansicht, Verschieben, max. 3 je Möbel |

Build: `dotnet build OOPGames\OOPGames.csproj --no-restore` erfolgreich (0 Warnungen, 0 Fehler).
Manuell geprüft: Startansicht, Zuhause/Stadt-Übergang und Stadtkarte; zusätzlich kleine und maximierte Fenster.
Noch offen: vollständige Prüfung der Seitenverhältnisse 16:10 und 4:3 sowie die Funktionspfade für Greifer,
Bank/Kredit und Möbelkauf/-platzierung.

## Sprint 1 – Neue Issues aus der Ideen-Liste (15./16.11.2026)

Quelle: Ideen von Patrick Lupierz, bisher als Freitext am Ende dieser Datei. Jetzt im Schema der Issue-Tabelle; Status nach Stand 16.11.2026.

| # | Issue-Inhalt | Fundstelle | Zugehörig | Bearbeiter | Status |
|---|--------------|------------|-----------|------------|--------|
| 1 | Möbel über klickbare, sich aktualisierende Hausansicht platzieren; max. 3 je Möbel; im Platzierungsmodus bewegbar | Ideen-Liste (Ende Datei), Patrick | Zuhause / Möbelgeschäft | Patrick Lupierz | erledigt – Einrichtungsmodus (klicken, verschieben, einlagern), Limit 3 je Möbel |
| 2 | Tiere mit Sprechblasen (typische Laute), nicht begehbar, laufen draußen umher; Katzen streicheln; 5 Katzenvarianten (Calico, Orange, Schwarz u. a.); Hunde, streichelbar | Ideen-Liste (Ende Datei), Patrick | Außenwelt / Tiere | Patrick Lupierz | erledigt – 5 Katzen- und 3 Hundevarianten, Revier-Laufen, massiv, Streicheln, Lautblasen |
| 3 | Hintergrundmusik in Läden, Shops, Casino und Spielhalle; Geräusche beim Gewinnen und generell | Ideen-Liste (Ende Datei), Patrick | Audio | Patrick Lupierz | teilweise – 7 Musikstücke und 10 Geräusche integriert, Ton mit [T] aus/an; Hörprobe im Hauptfenster offen |
| 4 | Fahrrad zeigt immer in eine Richtung; Animation anpassen; ausgerüstetes Fahrrad wird beim Betreten/Verlassen eines Hauses doppelt angeklickt | Ideen-Liste (Ende Datei), Patrick | Fahrrad / Außenwelt | Patrick Lupierz | erledigt – Grafik je Richtung, Rad wartet vor der Tür, automatisches Weiterfahren |
| 5 | Standardfragen "Das machst du echt gut hier" / "Du kannst nicht spielen" nicht bei jedem NPC; mehr Gesprächsthemen | Ideen-Liste (Ende Datei), Patrick | NPC / Gespräche | Patrick Lupierz | erledigt – rollenabhängiges Lob und Spitze, Familie ohne Beleidigung, eigene Themen je NPC, Stadtklatsch |
| 6 | Angel-Minispiel auf der Brücke; Fische verkaufen; Fischanimation, Platschen, Schlupf-Leiste (grau mit grünem Abschnitt, entgegen dem Fisch); Glücksfunde (Coins, Items) | Ideen-Liste (Ende Datei), Patrick | Minispiel / Angeln | Patrick Lupierz | erledigt – eine Angel auf der untersten Brücke (begehbar), Drill mit Leiste, 20 Fischarten aus Grafikpaket, Funde, Verkauf im Laden; Hörprobe offen |
| 7 | Tankstelle: Retro-Gebäude in Rot/Weiß, Verkaufsfenster mit Markise und Theke, zwei Zapfsäulen auf Steinplatten | Ideen-Liste (Ende Datei), Patrick | Außenwelt / Tankstelle | Patrick Lupierz | erledigt – rotes Dach, Holztür, Verkaufsfenster (Olli, rund um die Uhr), Zapfsäulen auf Steinplatten |
| 8 | Poker: eigene Hand tiefer, im Halbkreis gehalten und 5 % größer | Ideen-Liste (Ende Datei), Patrick | Casino / Poker | Patrick Lupierz | erledigt – Kartenfächer, Skalierung 1,05 |
| 9 | Admin-Cheat per Tastenfolge (ADMIN im Spielautomaten): 100 % Jackpot, Münzregen abhängig vom Einsatz, dauerhaft erhöhtes Tempo | Ideen-Liste (Ende Datei), Patrick | Casino / Spielautomat | Patrick Lupierz | erledigt – an/aus per ADMIN, Münzregen nach Einsatz, Turbo-Tempo |
| 10 | Umzugsschild vom Greifautomaten entfernen | Ideen-Liste (Ende Datei), Patrick | Casino | Patrick Lupierz | erledigt |
| 11 | Pavillon von jeder Seite betretbar | Ideen-Liste (Ende Datei), Patrick | Außenwelt / Pavillon | Patrick Lupierz | erledigt – nur die Ecksäulen massiv |
| 12 | Fisch-Skins im Fluss wie im Angel-Minispiel | Ideen-Liste (Ende Datei), Patrick | Außenwelt / Tiere | Patrick Lupierz | erledigt – Flussfische nutzen die 20 Fischgrafiken des Angelspiels (wasserblau getönt) |
| 13 | Arcade-Gast-Automat: Regeln, Painter und KI derselben Gruppe automatisch wählen (z. B. 4 gewinnt), ganze Partie ohne erzwungenes Ergebnis, [Esc] immer zurück zur Spielauswahl; später auch die zweiten Spiele der Gruppen | Ideen-Liste (Ende Datei), Patrick | Spielhalle / Gast-Automat | Patrick Lupierz | erledigt – Auswahl nach Gruppen-Präfix, fremde Teile während der Partie beim Framework als aktiv angemeldet, KI wartet Animationen ab; alle 9 Spiele im Test komplett durchgespielt; neue Spiele (z. B. zweite Spiele) werden automatisch erkannt, max. 9 |
| 14 | Sound-Paket einbauen (alle Sounds), Handy-App zum Einstellen von Lautstärke, Häufigkeit und gleichzeitigen Tönen; Tiere nur beim Streicheln, Vögel nur im 4-Block-Radius, Schritte nach Dateiname und Boden; Dateien unter Assets | Ideen-Liste (Chat), Patrick | Audio / Handy-App | Patrick Lupierz | erledigt – 49 Dateien (umgewandelt, 28 MB) in Assets/Sounds, Web-App im Handy-Browser (Menü B → F11), 80/80 Tests; Hörprobe und Handy-Test offen |
