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
