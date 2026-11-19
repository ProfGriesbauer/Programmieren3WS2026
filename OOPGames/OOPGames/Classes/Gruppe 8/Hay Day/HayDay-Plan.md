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
- Es gibt Weizen, Mais, Karotten und Soja. Karotten- und Sojasaatgut ist günstig am Markt erhältlich; alle Feldfrüchte werden im Silo gelagert und als Mühlenzutaten verwendet.
- Futterrezepte: Hühnerfutter = 1 Weizen + 1 Mais, Kuhfutter = 2 Weizen + 1 Mais, Schaffutter = 1 Weizen + 2 Mais, Schweinefutter = 1 Mais + 1 Karotte, Ziegenfutter = 1 Weizen + 1 Soja.
- Jeder neu gebaute Tierstall beherbergt zwei Tiere; ein zusätzlicher Hühnerstall beherbergt fünf Hühner. Gebäude derselben Art und Felder werden bei wiederholtem Kauf teurer.
- Eine Futtereinheit versorgt den jeweiligen Tierbestand fünf Minuten lang. Pro Tier entsteht bei ausreichendem Futter alle 60 Sekunden ein Ei, alle 300 Sekunden Milch, alle 360 Sekunden Fleisch, alle 600 Sekunden Wolle oder alle 300 Sekunden Ziegenmilch. Feldfrüchte wachsen zwei Minuten und ergeben drei Einheiten.
- Zum Füttern wird genau so viel artgerechtes Futter verbraucht wie Tiere in einem Stall vorhanden sind. Die Tieranzeige zeigt Countdown oder fertige Produkte.
- Nach der Fütterung sind Tiere fünf Minuten lang versorgt. Erst danach werden sie wieder als hungrig angezeigt und benötigen eine erneute Fütterung, bevor die Produktion weiterläuft.
- Marktpreise ändern sich täglich. Feldfrüchte und Tierprodukte können einzeln verkauft werden; fünf Weizen/Mais kosten den doppelten Verkaufspreis, fünf Karotten-/Sojasamen den normalen Marktpreis. Ein Lieferauftrag kann nur einmal erfüllt werden und wird am nächsten Markttag erneuert.
- Pfeiltasten bewegen die Kamera, Plus/Minus verändern den Zoom, Links-/Rechtsklick dienen der Interaktion. Der Spielstand speichert Bestände, Fortschritt und Zeitstempel; beim Laden wird die vergangene Zeit berücksichtigt.

## Arbeitspakete

1. **Bestehende Hay-Day-Implementierung prüfen:** Framework-Nutzung, Klickabläufe, Shop, Preisstaffelung, Startzustand und Save-/Load-Verhalten anhand der aktuellen Klassen abgleichen. Bereits passende Funktionen beibehalten; nur Lücken und widersprüchliche Regeln ändern.
2. **Grundspiel- und Shopablauf vervollständigen:** Kaufbare Felder und Gebäude, inklusive weiterer Futtermühlen und Tierställe, über den bestehenden Shop auswählen und auf gültigen Rasterplätzen platzieren. Kosten mit jedem Kauf derselben Art erhöhen, unzureichende Münzen und belegte Plätze sauber behandeln.
3. **Felder und Saatgut ergänzen:** Rechtsklick-Saatmenü pro Feld, günstige Saatgutkäufe im Markt und zusätzliche Pflanzenarten ergänzen. Pflanzen, Wachstumsdauer, Ernteertrag und Verwendung der Ernte als Futterzutaten konsistent halten.
7. **Wachstumsanzeige ergänzen:** Jedes bepflanzte Feld erhält einen Countdown bis zur Ernte. Nach Ablauf wird `Ernten!` angezeigt und die Pflanzenköpfe werden goldfarben dargestellt.
4. **Tiere, Gehege und Futterkreislauf ausbauen:** Ställe mit umzäunten Gehegen und sichtbaren Tieren für Hühner, Kühe, Schweine, Schafe und Ziegen ergänzen. Tierzahlen und passende Futterarten speichern; gefütterte Tiere erzeugen ihre Produkte fortlaufend, ohne zu sterben oder entfernt zu werden. Mehrere Mühlen ermöglichen passende Futterrezepte aus geernteten Pflanzen.
5. **Markt und Speicherung integrieren:** Verkaufsangebote, Preisentwicklung, Bestellanfragen/Lieferwagen sowie Kauf von Saatgut und Weizen/Mais in die Gebäudemenüs integrieren. Alle neuen Bestände, Tiere, Gebäudetypen und Produktionszeiten rückwärtskompatibel speichern und beim Laden die verstrichene Zeit kontrolliert anwenden.
6. **Validieren:** Projekt im vorhandenen Ziel-Framework bauen. Kauf-/Preisstaffelung, Platzierung, Rechtsklick-Saatwahl, Kapazitätsgrenzen, Futterverbrauch, nicht-tödliche Tierproduktion und Save-/Load-Fortsetzung gezielt prüfen. Änderungen auf Hay-Day-Klassen beschränken.

## Betroffene Komponenten

- `OOPGames/Classes/Gruppe 8/Hay Day/HayDayGame.cs`: Regeln, Rasterzustand, Eingaben, Preise, Shop, Produktion und Save-Daten.
- `OOPGames/Classes/Gruppe 8/Hay Day/HayDayPainter.cs`: isometrische Darstellung, Menüs, Gehege, Gebäude, Pflanzen und Tiere.
- `OOPGames/Classes/Gruppe 8/Hay Day/HayDayHumanPlayer.cs`: Zuordnung der Maus- und Tastatureingaben zu Hay-Day-Zügen.
- `OOPGames/Gruppe8HayDayTests`: Automatisierte xUnit-Tests für Startzustand, Wachstum, Züge und Speicher.

## Hinweise

- Der im Projekt eingestellte .NET-Framework-Zielwert und die Framework-Registrierung bleiben bestehen; kein Wechsel oder Umbau des OOPGames-Frameworks.
- Die genaue Pflanzenliste und Preise werden bei der Umsetzung anhand der vorhandenen Erträge, Münzquellen und Startbestände balanciert.
- Save-Daten älterer Spielstände müssen weiterhin ladbar bleiben; fehlende neue Felder erhalten sichere Standardwerte.
