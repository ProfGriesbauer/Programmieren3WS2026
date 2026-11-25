# Gruppe 3: Retro Drag Race - Spielkonzept und Projektplan

Dieses Dokument beschreibt das Spielkonzept, die Architektur, die Regeln und die Features des im Rahmen des Projekts "OOPGames" entwickelten 2D Retro Drag Racing Spiels von Gruppe 3.

---

## 1. Spielkonzept & Übersicht

"Gruppe 3 Retro Drag Race" ist ein klassisches 2D-Beschleunigungsrennen (Drag Race) aus der Seitenansicht, angereichert mit Retro-Elementen, detaillierten Cockpits (Kombiinstrumenten) und realistischer Gangschaltung.

- **Ziel des Spiels:** Als Erster die Ziellinie des Drag-Strips zu überqueren, ohne einen Fehlstart zu begehen und durch optimales Timing beim Schalten die maximale Leistung aus dem gewählten Fahrzeug herauszuholen.
- **Spieler:** Zwei Spieler (Mensch gegen Mensch, Mensch gegen Computer oder Computer gegen Computer).
- **Steuerung:**
  - **Player 1:** 
    - Auto-Auswahl: `1`, `2`, `3` (bzw. entsprechende Klicks/Tasten)
    - Rennen: `W` (Beschleunigen), `S` (Bremsen), `Q` / `E` (Herunterschalten / Hochschalten)
  - **Player 2:** 
    - Auto-Auswahl: Entsprechende Tasten
    - Rennen: `Numpad 8` / `Pfeil nach oben` (Beschleunigen), `Numpad 5` / `Pfeil nach unten` (Bremsen), `7` / `9` bzw. `Bild Auf` / `Bild Ab` (Gangschaltung)

---

## 2. Spielablauf & Phasen

1. **Fahrzeugauswahl (Car Selection):**
   - Zu Beginn wählen nacheinander Player 1 und Player 2 ihr Fahrzeug aus drei verfügbaren Retro-Boliden aus.
   - Jedes Auto besitzt individuelle physikalische Eigenschaften (Drehzahlbereich, Getriebeübersetzung, Beschleunigung, Höchstgeschwindigkeit) und ein eigenes, einzigartiges Kombiinstrument (Tacho).
   
2. **Countdown & Start:**
   - Ein visueller Countdown (3, 2, 1, GO!) startet.
   - **Fehlstart-Erkennung:** Wenn ein Spieler vor dem "GO!"-Signal beschleunigt, wird ein Fehlstart gewertet und der Gegner gewinnt automatisch das Rennen.

3. **Das Rennen (Drag Strip):**
   - Die Fahrzeuge starten am linken Bildschirmrand und fahren bis zur Streckenmitte; ab dort scrollt die Kamera/Strecke dynamisch mit, sodass eine lange, flüssige Rennstrecke entsteht.
   - Beide Spieler müssen durch gezieltes Gasgeben und präzises Hochschalten ("Shift-Points") im optimalen Drehzahlbereich bleiben.
   - Schaltet ein Spieler bei roter Drehzahl nicht rechtzeitig hoch, begrenzt der Drehzahlbegrenzer die Leistung und die Nadel beginnt zu zittern.
   - Zu frühes oder abruptes Herunterschalten lässt die Drehzahl nach oben schnellen und bremst das Fahrzeug ab.

4. **Ziel & Auswertung:**
   - Wer zuerst die Ziellinie überquert, gewinnt. Bei exakt gleicher Zeit gibt es ein Unentschieden.

---

## 3. Technische Architektur & Komponenten

Das Spiel ist modular nach dem OOPGames-Framework aufgebaut:

- **`Gruppe3DragRaceRules`** (`IGruppe3DragRaceRules`): Verwaltet den gesamten Spielzustand, den Rundenticker (`TickGameCall`), den Countdown, die Fehlstart-Logik und die Siegbedingung.
- **`Gruppe3DragRaceField`** (`IGruppe3DragRaceField`): Hält alle darstellungsrelevanten Daten (Positionen der Autos, Geschwindigkeiten, Gänge, Drehzahlen, Tacho-Metriken, Timer und Statusmeldungen).
- **`Gruppe3DragRacePainter`** (`IGruppe3DragRacePainter`): Verantwortlich für das Rendering in WPF. Zeichnet den Drag Strip, die Autos, die skalierten Kombiinstrumente (Tachos mit dynamisch positionierten und skalierten Tachonadeln anhand der lila Referenzpunkte `#e100ff`), Fortschrittsbalken und Steuerungshinweise.
- **`Gruppe3DragRaceCars` & `Cars`:** Datenklasse für die verschiedenen Fahrzeuge mit individuellen Parametern (Maximaldrehzahl, Gänge, Tachokalibrierung und Asset-Pfaden).
- **`Gruppe3DragRaceRacerState` & `Physics`:** Kapseln die Fahrzeugphysik, Kinematik, Gangschaltungs-Mechanik und die KI-Entscheidungen (`Gruppe3DragRaceAi`).
- **`Gruppe3DragRaceInput` & `Move`:** Verarbeiten Tastatureingaben und wandeln sie in Spielzüge (`IPlayMove`) um.

---

## 4. Design-Besonderheiten & UI/UX

- **Dynamische Kombiinstrumente:** Jedes Auto lädt sein eigenes Cockpit-Hintergrundbild. Die Drehzahlmessernadeln werden mithilfe von automatischer Farberkennung (lila Referenzpunkt `#e100ff` mit 2x2 Pixeln) präzise zentriert und proportional zur Fenstergröße skaliert.
- **Responsive Layouts:** Skalierende Fortschrittsbalken, Tachos und Steuerungsanzeigen verhindern Überlappungen und passen sich flexibel an verschiedene Fenstergrößen an.

---

## 5. Zukünftige Erweiterungen & Ideen

- Erweiterung um Tuning-Optionen (Reifenmischung, Lachgaseinspritzung / Nitro).
- Zusätzliche Fahrzeuge und Soundeffekte (Motorsound, Gangwechsel-Klicken).
- Highscore-Liste für Bestzeiten.
