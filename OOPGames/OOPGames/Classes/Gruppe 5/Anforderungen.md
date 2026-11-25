
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
