# Hay-Day-Spiel – Umsetzungsplan

## Ziel und Ansatz

Das Hay-Day-Spiel wird innerhalb des bestehenden OOPGames-Frameworks vervollständigt. Framework-Schnittstellen und allgemeine OOPGames-Klassen werden nicht verändert. Spielfeld, Eingaben, Regeln, Zeichnung und Spielstand bleiben in den vorhandenen Hay-Day-Klassen gebündelt.

Der vorhandene Stand enthält bereits das 50x100-Raster, isometrische Zeichnung, Kamera- und Zoomsteuerung, Grundgebäude, Münzen, Zeitstempel-Spielstand, einen Shop, Weizen/Mais, Hühner sowie erste Kuh-/Schaf- und Futter-/Produktionslogik. Diese Teile werden zuerst geprüft und weiterverwendet, statt parallel neu gebaut zu werden.

## Festgelegte Spielregeln

- Startansicht und Anfangsausstattung bleiben fest: sechs Felder, Farmhaus mit endender Straße, Scheune, Silo, Hühnerstall mit fünf Hühnern und Futtermühle.
- Ausschließlich Münzen dienen als Währung. Neue Gebäude, Tiere und Erweiterungen werden mit wiederholtem Kauf derselben Art teurer; Preise und Startkapital müssen zusammen spielbar bleiben.
- Tiere werden nicht geschlachtet, sterben nicht und werden beim Produzieren nicht entfernt. Mit passendem Futter erzeugen sie regelmäßig ein tierartspezifisches Produkt.
- Geplante Produktzuordnung: Hühner → Eier, Kühe → Milch, Schweine → Fleisch als abstraktes Spielprodukt ohne Tierverlust, Schafe → Wolle, Ziegen → Ziegenmilch.
- Rechtsklick auf ein Feld öffnet die Saatgutwahl; ein Feld wird nach der Auswahl mit Linksklick bepflanzt. Reife Pflanzen werden geerntet.
- Saatgut für zusätzliche Pflanzenarten soll im Markt günstig erhältlich sein. Pflanzen werden als Futterzutaten für die Futtermühle verwendet.
- Pfeiltasten bewegen die Kamera, Plus/Minus verändern den Zoom, Links-/Rechtsklick dienen der Interaktion. Der Spielstand speichert Bestände, Fortschritt und Zeitstempel; beim Laden wird die vergangene Zeit berücksichtigt.

## Arbeitspakete

1. **Bestehende Hay-Day-Implementierung prüfen:** Framework-Nutzung, Klickabläufe, Shop, Preisstaffelung, Startzustand und Save-/Load-Verhalten anhand der aktuellen Klassen abgleichen. Bereits passende Funktionen beibehalten; nur Lücken und widersprüchliche Regeln ändern.
2. **Grundspiel- und Shopablauf vervollständigen:** Kaufbare Felder und Gebäude, inklusive weiterer Futtermühlen und Tierställe, über den bestehenden Shop auswählen und auf gültigen Rasterplätzen platzieren. Kosten mit jedem Kauf derselben Art erhöhen, unzureichende Münzen und belegte Plätze sauber behandeln.
3. **Felder und Saatgut ergänzen:** Rechtsklick-Saatmenü pro Feld, günstige Saatgutkäufe im Markt und zusätzliche Pflanzenarten ergänzen. Pflanzen, Wachstumsdauer, Ernteertrag und Verwendung der Ernte als Futterzutaten konsistent halten.
4. **Tiere, Gehege und Futterkreislauf ausbauen:** Ställe mit umzäunten Gehegen und sichtbaren Tieren für Hühner, Kühe, Schweine, Schafe und Ziegen ergänzen. Tierzahlen und passende Futterarten speichern; gefütterte Tiere erzeugen ihre Produkte fortlaufend, ohne zu sterben oder entfernt zu werden. Mehrere Mühlen ermöglichen passende Futterrezepte aus geernteten Pflanzen.
5. **Markt und Speicherung integrieren:** Verkaufsangebote, Preisentwicklung, Bestellanfragen/Lieferwagen sowie Kauf von Saatgut und Weizen/Mais in die Gebäudemenüs integrieren. Alle neuen Bestände, Tiere, Gebäudetypen und Produktionszeiten rückwärtskompatibel speichern und beim Laden die verstrichene Zeit kontrolliert anwenden.
6. **Validieren:** Projekt im vorhandenen Ziel-Framework bauen. Kauf-/Preisstaffelung, Platzierung, Rechtsklick-Saatwahl, Kapazitätsgrenzen, Futterverbrauch, nicht-tödliche Tierproduktion und Save-/Load-Fortsetzung gezielt prüfen. Änderungen auf Hay-Day-Klassen beschränken.

## Betroffene Komponenten

- `OOPGames/Classes/Gruppe 8/Hay Day/HayDayGame.cs`: Regeln, Rasterzustand, Eingaben, Preise, Shop, Produktion und Save-Daten.
- `OOPGames/Classes/Gruppe 8/Hay Day/HayDayPainter.cs`: isometrische Darstellung, Menüs, Gehege, Gebäude, Pflanzen und Tiere.
- `OOPGames/Classes/Gruppe 8/Hay Day/HayDayHumanPlayer.cs`: Zuordnung der Maus- und Tastatureingaben zu Hay-Day-Zügen.

## Hinweise

- Der im Projekt eingestellte .NET-Framework-Zielwert und die Framework-Registrierung bleiben bestehen; kein Wechsel oder Umbau des OOPGames-Frameworks.
- Die genaue Pflanzenliste und Preise werden bei der Umsetzung anhand der vorhandenen Erträge, Münzquellen und Startbestände balanciert.
- Save-Daten älterer Spielstände müssen weiterhin ladbar bleiben; fehlende neue Felder erhalten sichere Standardwerte.
