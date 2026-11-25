
# Anforderungen - FACTORIO/Satisfactory 2D

## Spielfeld
Raster Spielfeld
Verschiedene ressourcen wie holz, eisen, kupfer, kohle, kalkstein, quarz, schwefel, uran, wasser, gas, öl etc
Arten von Feldern: Ressourcen Felder, Leere Felder, Hinderniss felder
Hindernisse wie große Felsen, wo man nicht drüber gehen kann

zoomen möglich, und ansicht verschieben
Immer nur gewisser radius um einen sichtbar, wenn noch nicht in dem bereich gewesen

## Gebäude (gestalten, Funktionen festlegen)
Förderer, Schmelze, Konstruktor, Montagewerk, Kohlekraftwerk, Strommast, Lager
Gebäude verbesserbar
Förderband (tickbasiert)

## Game Logik
Hand abbauen und craften möglich
Inventar mit begrenzter Größe
-- Missionen wenn nicht geschafft, dann raus --> erstmal nicht(vielleicht mit zwei modi, competitive & cozy) --

## Interaktion
wenn gebäude angeklickt, POP up mit rezept auswahl, feld für resourcen und feld für fertige produkte
Maussteuerung; Tastenkürzel, übersichtliche Benutzeroberfläche
Alles Zeitbasiert
Gebäude verschieben
Stromnetz quer drüber

## Rezepte
Schmelze: Aus rohstoff wird barren
Konstruktor: Eisenbarren wird zu stangen oder platten

# Struktur
Alles objektorientiert Aufbau
Oberklasse Gebäude (Name, Koordinate, LVL)
Unterklasse Fertigungsgebäude: mögliche Rezepte
Förderband: mit welchen Koordinaten verbunden

Klasse Rezepte

## Design
vorgabe machen anhand beispiel
Sollte alles erkennbar und gut unterscheidbar sein

# Aufteilung
Plan machen für Klassen und Struktur:
Spielfeld, design: Tim
Game Logik und Interaktion: Maxi
Gebäude: Chris

## Erste spielbare Version

- Eine feste, übersichtliche Kachelkarte mit Eisenfeldern und einem festen Lager als Ziel.
- Mit `F` den Förderer oder mit `S` die Schmelze auswählen und anschließend per Klick auf der Karte platzieren; `Esc` bricht die Auswahl ab.
- Förderer werden auf Eisenfeldern platziert und produzieren automatisch Eisenerz.
- Das Lager nimmt Erz auf, versorgt Schmelzen automatisch und lagert erzeugte Eisenbarren ein. Für diesen ersten Prototyp sind keine Förderbänder nötig.
- Gebäude sind zunächst kostenlos; Stromversorgung, Handabbau und -herstellung, Verbesserungen und Missionsziele sind Erweiterungen.

## Endziel: Rakete und Spielabschluss

- Die Rakete ist das letzte und teuerste Bauwerk. Sie wird wie andere Gebäude
  über das Baumenü auf einem freien Feld gebaut; die Kosten betragen 40
  Eisenplatten, 30 Beton und 20 Rotoren. Es kann nur eine Rakete gleichzeitig
  gebaut werden.
- Die Rakete ist als helles Modell mit roter Spitze und roten Flossen sowie
  blauem Fenster erkennbar. Ihr Gebäudemenü zeigt den Füllstand an.
- Zum Start benötigt die Rakete 50 Uran. Betanken ist in mehreren Schritten aus
  Rucksack und Lagern möglich. Betanken und Einsteigen erfordern, dass der
  Spieler direkt neben der Rakete steht. Ohne vollen Tank ist kein Start möglich.
- Beim Verkauf werden 50 Prozent der Baukosten und das gesamte eingefüllte Uran
  zurückgegeben. Reicht der Lagerplatz nicht, bleibt die Rakete erhalten.
- Nach dem Einsteigen hebt die Rakete in einer kurzen Animation ab. Anschließend
  zeigt eine Gewinnansicht, dass der Planet verlassen und das Spiel geschafft
  wurde. Während des Abflugs und nach dem Sieg sind normale Spielaktionen gesperrt.
- Eine Statistik ist während des Spiels und nach dem Sieg per Schaltfläche oder
  `F2` einsehbar. Sie zeigt Spielzeit, die Anzahl gebauter Gebäude sowie je
  Rohstoff die tatsächlich abgebaute und je Produkt die tatsächlich hergestellte
  Menge. Transporte und Verkäufe verändern diese historischen Zähler nicht.
- Ein neues Spiel setzt Spielphase und Statistiken zurück. Mindestens ein
  Uranvorkommen ist in jeder neu erzeugten Welt vorhanden.
