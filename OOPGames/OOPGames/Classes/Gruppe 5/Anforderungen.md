
# Anforderungen - FACTORIO/Satisfactory 2D

## Spielfeld
Raster Spielfeld
Verschiedene ressourcen wie holz, eisen, kupfer, kohle, kalkstein, quarz, schwefel, uran, wasser, gas, öl etc
Arten von Feldern: Ressourcen Felder, Leere Felder, Hinderniss felder
Hindernisse wie große Felsen, wo man nicht drüber gehen kann

zoomen möglich, und ansicht verschieben
Immer nur gewisser radius um einen sichtbar, wenn noch nicht in dem bereich gewesen

## Gebäude
Miner, Schmelze, Konstruktor, Verbrennungsgenerator, Kohlekraftwerk
Gebäude upgradebar
Laufband

## Game Logik
Hand abbauen und craften möglich
Inventar mit begrenzter Größe
Missionen wenn nicht geschafft, dann raus --> erstmal nicht(vielleicht mit zwei modi, competitive & cozy)

## Interaktion
wenn gebäude angeklickt, POP up mit rezept auswahl, feld für resourcen und feld für fertige produkte
Maussteuerung; Tastenkürzel, übersichtliche Benutzeroberfläche
Alles Zeitbasiert
Gebäude verschieben
Stromnetz quer drüber

## Rezepte
Schmelze: Aus rohstoff wird baren
Konstruktor: Eisenbaren wird zu stangen oder platten

# Struktur
Alles objektorientiert Aufbau
Oberklasse Gebäude (Name, Koordinate, LVL)
Unterklasse Fertigungsgebäude: mögliche Rezepte
Fertigungsband: mit welchen Koordinaten verbunden

Klasse Rezepte

## Design
vorgabe machen anhand beispiel
Sollte alles erkennbar und gut unterscheidbar sein

# Aufteilung
Plan machen für Klassen und Struktur:
Spielfeld: Tim
Game Logik und Interaktion: Maxi
Gebäude: Chris
