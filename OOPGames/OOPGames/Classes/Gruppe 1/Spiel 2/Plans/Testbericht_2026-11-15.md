# Testbericht – 15.11.2026

Verantwortlich: Patrick Lupierz (KI-gestützt mit Claude Code; Ergebnisse vom Agenten tatsächlich ausgeführt)

## 1. Build

`dotnet build OOPGames.csproj` → **0 Fehler, 0 Warnungen**.

Hinweis: Eine liegengebliebene WPF-Zwischendatei `OOPGames_*_wpftmp.csproj` (entsteht bei überlappenden Builds,
z. B. Agent + IDE gleichzeitig) ließ `dotnet build` ohne Projektangabe mit MSB1011 abbrechen. Sie wurde gelöscht
(nicht versioniert). Bei erneutem Auftreten: Datei löschen oder `dotnet build OOPGames.csproj` verwenden.

## 2. Automatisierte Tests (`G1C_SelfTest`)

- **Start im Spiel (nur auf ausdrücklichen Aufruf):** Menü mit **[B]** öffnen, dann **[F12]**. Ergebnisse erscheinen als
  Liste; zusätzlich entsteht `G1Casino_SelfTest.txt` neben der `OOPGames.exe`. Ein normaler Spielstart führt keine Tests aus.
- **Start ohne Fenster (Agent):** Hilfsprogramm außerhalb des Repositorys (Scratchpad), das die gebaute `OOPGames.dll`
  referenziert und `G1C_SelfTest.RunAll()` aufruft. Am Projekt wurde dafür nichts geändert.

Ergebnis: **30/30 BESTANDEN**

| Bereich | Test | Ergebnis |
|---------|------|----------|
| Bewegung | Ein Tastendruck = genau ein Schritt, danach Stillstand | BESTANDEN |
| Bewegung | Gehaltene Taste läuft ohne Pause weiter | BESTANDEN |
| Bewegung | Wiederholte Taste läuft nach dem Loslassen nicht nach | BESTANDEN |
| Bewegung | Richtungswechsel während eines Schritts | BESTANDEN |
| Bewegung | Wand und Bett blockieren, Figur dreht sich trotzdem | BESTANDEN |
| Bewegung | Keine Bewegung während eines Dialogs | BESTANDEN |
| Bewegung | Bäume, Gebäude, Pavillons und NPCs blockieren in der Stadt | BESTANDEN (erster Lauf FEHLGESCHLAGEN → Fehler in `G1C_CollisionManager` behoben) |
| Grafik | Spielfigur: 4 Richtungen x 3 Frames vorhanden | BESTANDEN |
| Grafik | Alle Objekte aller Szenen haben eine Grafik | BESTANDEN |
| Grafik | Neue IDs: sign_closed, Poster, Sticker, Mama, Oma | BESTANDEN |
| NPC | Mama und Oma existieren, stehen in der Welt und sind im Handy | BESTANDEN |
| NPC | Mama und Oma haben eigene Fragen und Antworten | BESTANDEN |
| NPC | Jede Weltfigur hat ein Gesprächsprofil | BESTANDEN (erster Lauf FEHLGESCHLAGEN: Jana, Olli, Mia ohne Profil → `G1C_ShopNpcRoster`) |
| NPC | Gespräch mit Mama bietet Fragen an, Wiederholung ändert Antwort | BESTANDEN |
| NPC | Taschengeld nur nach Gespräch mit beiden, einmal pro Tag | BESTANDEN |
| Geschäfte | Öffnungszeiten der Läden, Tankstelle durchgehend | BESTANDEN |
| Geschäfte | Geschlossener Laden: Tür zu, Hinweis, CLOSED-Liste | BESTANDEN |
| Geschäfte | Kauf bucht Preis + 5 % ab und legt Item ins Inventar | BESTANDEN |
| Geschäfte | Verkäuferin: Gespräch, dann Warenliste; nach Ladenschluss kein Verkauf | BESTANDEN |
| Gebäude | Schilder, Poster und Sticker sind untersuchbar | BESTANDEN |
| Casino | Roulette: Zahl 35:1, Rot 1:1, Null verliert Außenwette | BESTANDEN |
| Casino | Blackjack: Werte, Blackjack, Bust, Dealer zieht bis 16 | BESTANDEN |
| Casino | Poker: Handbewertung und Vergleich | BESTANDEN |
| Casino | Spielautomat: Gewinnkombinationen | BESTANDEN |
| Casino | Guthaben: keine negativen Werte, keine Überziehung | BESTANDEN |
| Inventar | Einlagern und Entfernen | BESTANDEN |
| Bank | Einzahlen und Abheben | BESTANDEN |
| Bank | 2 % Zinsen pro Spieltag, kein Doppelzins | BESTANDEN |
| Bank | Zinsen beim Tageswechsel im laufenden Spiel genau einmal (inkl. Speichern/Laden) | BESTANDEN |
| Zustände | Inventar, Handy und Menü öffnen und schließen | BESTANDEN |

## 3. Visuelle Stichprobe (automatisiert gerendert, nicht interaktiv)

Mit dem echten `G1C_CasinoPainter` offscreen auf ein Canvas gerendert und angesehen:
Zuhause mit Mama und Poster · Bekleidungsgeschäft um 23 Uhr mit CLOSED-Schild und Hinweis · Oma Gerda mit Katze und
Schildern vor ihrem Haus · Tankstelle mit Sticker. Nacht-Abdunkelung gemessen (mittleres RGB 84/98/98 nachts gegenüber
161/181/119 mittags).

## 4. Nicht ausgeführt / manuell offen (NICHT AUSFÜHRBAR für den Agenten)

- Bedienung mit echter Tastatur im Hauptfenster: Fokus nach "Start New Game", Pfeiltasten, Leertaste/Enter, Halten von WASD.
- Schlafanimation, Gewinnanimationen, Claw-Machine-Ablauf, Blackjack Split/Double, Möbel kaufen/platzieren, Geldhai-Ansprache.
- Fenster-Seitenverhältnisse 16:10 und 4:3.

### Manuelle Checkliste für das Team

1. Start New Game → sofort **W/A/S/D** und **Pfeiltasten** halten: Figur läuft ohne Anfangspause, stoppt beim Loslassen sofort.
2. **Leertaste** drücken: Spiel darf **nicht** neu starten.
3. In eine Auswahlliste des Hauptfensters klicken, dann ins Spielfeld klicken: Pfeiltasten steuern wieder die Figur.
4. Mit Mama (Zuhause) und Oma (Haus rechts neben dem Zuhause) sprechen → "Ich hab da eine Frage ..." → Taschengeld.
5. Nach 19 Uhr vor das Bekleidungsgeschäft: CLOSED-Schild, Tür bleibt zu. Tankstelle nachts betretbar.
6. Geld einzahlen, schlafen (Bett) → Abdunkeln mit "Zzz", Meldung mit Zinsen; Bankzeile "Kontostand" zeigt Zinsen.
7. Menü [B] → [F12] → Selbsttest-Liste erscheint.


## 5. Nachtrag – 2. Arbeitsschritt (Familie, Bank, Möbel, Fahrrad, Pavillon, Spielhalle)

Build: `dotnet build OOPGames.csproj` → 0 Fehler, 0 Warnungen.
Selbsttest (ohne Fenster ausgeführt): **43/43 BESTANDEN**. Der frühere Test "Taschengeld nur nach Gespräch mit beiden"
wurde durch die neue Regel ersetzt (einmal pro Tag, danach Nein).

Während der Arbeit fehlgeschlagen und behoben: zwei Testaufbau-Fehler (Türposition aus einer leeren Karte gelesen;
Ball im Ziegelbrecher-Test nicht weit genug unter dem Feld) – keine Spielfehler.

Vollständige Liste:

- [BESTANDEN] Bewegung - Ein Tastendruck = genau ein Schritt, danach Stillstand
- [BESTANDEN] Bewegung - Gehaltene Taste läuft ohne Pause weiter
- [BESTANDEN] Bewegung - Wiederholte Taste läuft nach dem Loslassen nicht nach
- [BESTANDEN] Bewegung - Richtungswechsel während eines Schritts
- [BESTANDEN] Bewegung - Wand und Bett blockieren, Figur dreht sich trotzdem
- [BESTANDEN] Bewegung - Keine Bewegung während eines Dialogs
- [BESTANDEN] Bewegung - Bäume, Gebäude, Pavillons und NPCs blockieren in der Stadt
- [BESTANDEN] Grafik - Spielfigur: 4 Richtungen x 3 Frames vorhanden
- [BESTANDEN] Grafik - Alle Objekte aller Szenen haben eine Grafik
- [BESTANDEN] Grafik - Neue IDs: sign_closed, Poster, Sticker, Mama, Oma
- [BESTANDEN] NPC - Mama und Oma existieren, stehen in der Welt und sind im Handy
- [BESTANDEN] NPC - Mama und Oma haben eigene Fragen und Antworten
- [BESTANDEN] NPC - Jede Weltfigur hat ein Gesprächsprofil
- [BESTANDEN] NPC - Gespräch mit Mama bietet Fragen an, Wiederholung ändert Antwort
- [BESTANDEN] NPC - Taschengeld: einmal pro Tag, danach Nein; am nächsten Tag wieder
- [BESTANDEN] NPC - Mama und Oma werden auch nach Beleidigungen nie Gegner
- [BESTANDEN] Geschäfte - Öffnungszeiten der Läden, Tankstelle durchgehend
- [BESTANDEN] Geschäfte - Geschlossener Laden: Tür zu, Hinweis, CLOSED-Liste
- [BESTANDEN] Geschäfte - Kauf bucht Preis + 5 % ab und legt Item ins Inventar
- [BESTANDEN] Geschäfte - Verkäuferin: Gespräch, dann Warenliste; nach Ladenschluss kein Verkauf
- [BESTANDEN] Gebäude - Schilder, Poster und Sticker sind untersuchbar
- [BESTANDEN] Casino - Roulette: Zahl 35:1, Rot 1:1, Null verliert Außenwette
- [BESTANDEN] Casino - Blackjack: Werte, Blackjack, Bust, Dealer zieht bis 16
- [BESTANDEN] Casino - Poker: Handbewertung und Vergleich
- [BESTANDEN] Casino - Spielautomat: Gewinnkombinationen
- [BESTANDEN] Casino - Guthaben: keine negativen Werte, keine Überziehung
- [BESTANDEN] Inventar - Einlagern und Entfernen
- [BESTANDEN] Bank - Einzahlen und Abheben
- [BESTANDEN] Bank - 2 % Zinsen pro Spieltag, kein Doppelzins
- [BESTANDEN] Bank - Zinsen beim Tageswechsel im laufenden Spiel genau einmal
- [BESTANDEN] Zustände - Inventar und Handy öffnen und schließen
- [BESTANDEN] Gebäude - Pavillon: von unten begehbar, Rückseite und Säulen massiv
- [BESTANDEN] Bank - Bank hat Bankierin, Berater und Wachmann mit eigenen Themen
- [BESTANDEN] Fahrrad - Startfahrrad, Auf-/Absteigen nur draußen, schneller, Absteigen im Gebäude
- [BESTANDEN] Fahrrad - Fahrradladen: alle Farben ausgestellt, Kauf nur einmal, Ladenschluss
- [BESTANDEN] Spielhalle - 5 Automaten, max. 9 Spiele, Gast-Automat, Greifautomat umgezogen
- [BESTANDEN] Spielhalle - Einsatz 5 Chips, Belohnung genau einmal (Pong-Sieg gegen KI)
- [BESTANDEN] Spielhalle - Sternenjäger: Treffer zählt, Schiff verliert Leben
- [BESTANDEN] Spielhalle - Schlange: Frucht verlängert, Wand beendet
- [BESTANDEN] Spielhalle - Ziegelbrecher: Stein zerstört, 3 verlorene Bälle beenden
- [BESTANDEN] Spielhalle - Pong-KI berechnet Auftreffpunkt mit Wandabpraller
- [BESTANDEN] Spielhalle - Gast-Automat: fremdes Framework-Spiel finden, spielen, zeichnen, belohnen
- [BESTANDEN] Spielhalle - Automat im Spiel starten, Ergebnis, Rekord gespeichert

**Gast-Automat mit echten Spielen anderer Gruppen** (Hilfsprogramm außerhalb des Repos instanziiert alle Rules/Painter/Player
der gebauten Assembly – wie die Registrierung im Hauptfenster): 9 Spiele gefunden (Obergrenze 9), alle starten, nehmen
Taste/Klick an und werden im Automatenfenster gezeichnet, keine Ausnahme: Gruppe 10, G9, Gruppe 1 (2x), Gruppe 5, Gruppe 8,
Gruppe 2, FG Vier gewinnt, Griesbauer TicTacToe. Sichtprüfung der Bilder: Vier gewinnt (Gruppe 10) und TicTacToe korrekt.

**Gerenderte Szenen (angesehen):** Zuhause (neues Bett/Tisch), Bank (Frida sichtbar, Karl, Bodo, Stühle), Fahrradladen innen
und außen (Rad auf dem Dach), Spielhalle innen und außen, Figur unter dem Pavillon, Figur auf dem Fahrrad (leerer Ständer),
Sternenjäger im Spiel.

**Nicht ausgeführt:** Spielen der Arcade-Spiele mit echter Tastatur (Laufgefühl, Schwierigkeit), Greifautomat in der Spielhalle,
Gast-Spiele mit echter Maus im Hauptfenster.

## 6. Nachtrag – 3. Arbeitsschritt (Omas Haus, Tankstelle, Innenräume, Vögel, Schlafen)

Build: 0 Fehler, 0 Warnungen. Selbsttest: **49/49 BESTANDEN**.
Erster Lauf: 48/49 – der Vogeltest verlangte fälschlich, dass der Vogel nicht höher als der Baum ist; ein Vogel auf der
Kronenspitze muss aber darüber hinausragen. Test korrigiert (Überstand 6–12 Pixel), Grafik auf 16x40 angepasst.

Neue Tests: Omas Haus betretbar und bewohnt · Tankstelle mit Zapfsäulen und Fensterkauf um 3 Uhr · Seitenwände und
Teppichteile · eigenes Wandbild je Innenraum ohne Wiederholung · Vogel auf der Krone, unten transparent · Schlafzustand
sperrt und gibt die Steuerung wieder frei.

Gerendert und angesehen: Omas Haus außen und innen, Tankstelle mit Fenster und Zapfsäulen, Vogel auf dem Baum, Zuhause mit
Teppich, Wänden und Bildern, schlafende Figur im Bett, Bank mit neuen Wänden, Teppich und Zinsplakat.

Weiterhin nicht ausführbar: Bedienung im echten Hauptfenster (siehe Checkliste in Abschnitt 4).

## 7. Nachtrag 16.11.2026 – neue SprintPlanning-Punkte

Build: 0 Fehler, 0 Warnungen. Selbsttest: **64/64 BESTANDEN** (15 neue Tests: Möbel-Limit, Einrichtungsmodus, Haustiere,
Streicheln/Laute, Sound, Fahrrad-Richtungen, Fahrrad vor der Tür, Gespräche, Angelplätze, Drill/Fang/Verkauf, Entkommen,
Admin-Cheat/Münzregen, Poker-Fächer, Casino-Schild, Tankstellen-Look).

Erster Lauf: 60/64. Ursachen: (1) Tiertest lief im Zuhause – Haustiere bewegen sich bewusst nur in der aktiven Szene; Test in
die Stadt verlegt. (2)/(3) Testposition 30,20 war nicht begehbar; außerdem rief `Select(...).Last()` wegen einer
.NET-Optimierung nur das letzte Element auf (Cheat-Test). Tests korrigiert. (4) **Echter Fehler:** Die Zapfsäulen standen auf
der Straße statt auf den Steinplatten – Vorplatz und Säulen korrigiert. Pavillon- und Katzen-Test an das neue, gewünschte
Verhalten angepasst.

Gerendert und angesehen: Tankstelle (Dach, Wände, Holztür, Fenster mit Markise, Säulen auf Steinplatten), Hund mit
"Wuff! ♥", Katzen mit Lautblasen, Pavillon, Angel auf der Brücke, Angelspiel (Warten, Drill mit Leiste), Poker-Fächer,
Jackpot mit Münzregen, Einrichtungsmodus, Fahrrad in allen vier Richtungen, Omas Haus mit laufenden Katzen.

Nachtrag: Cheat-Tastenfolge auf ADMIN geändert ([B] öffnet das Menü); 20 Fischarten mit dem bereitgestellten
Grafikpaket. Selbsttest danach erneut ausgeführt (siehe Prompt 12 im Prompt-Protokoll).

Nicht ausführbar für den Agenten: Hörprobe der Musik/Geräusche und Bedienung im Hauptfenster. Ergänzung der Checkliste:
- [ ] Musik wechselt beim Betreten von Laden/Casino/Spielhalle; [B] dann [T] schaltet stumm; nach Wechsel zu einem anderen
      Framework-Spiel endet die Musik spätestens am Ende des Stücks
- [ ] Einrichtungsplan: Möbel per Mausklick aufstellen, anklicken und verschieben, [R] einlagern
- [ ] Angeln: [A]/[D] gedrückt halten steuert den grünen Bereich flüssig
- [ ] Fahrrad: in ein Haus fahren, wieder hinausgehen – man fährt direkt weiter
- [ ] Im Spielautomaten ADMIN tippen: Hinweis erscheint, nächste Drehung 7-7-7 mit Münzregen
