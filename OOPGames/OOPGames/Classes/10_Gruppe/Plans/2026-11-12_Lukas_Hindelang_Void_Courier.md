# Void Courier: eigenstaendige Retro-Alternative

## Ziel

Ein eigenstaendiges vertikales Shoot-'em-up als zusaetzliches Minispiel in der
Game-Boy-Geraeteansicht des Kinderzimmers. Die Spielszene verwendet ein
160x144-Koordinatensystem und wird mit WPF-Vektorgrafik farbig dargestellt.
Eigene Formen, Farbverlaeufe, parallax bewegte Sterne, dezente Nebel und
animierte Effekte sorgen fuer einen stilisierten 2.5D-Science-Fiction-Look.
Sie uebernimmt keine spezifischen SolarStriker-Sprites, Bossformen, Level oder
deren Ablaufe.

## Einbindung und Entwurf

- `G10_SolarShooterSession` verwaltet Spielzustand, Spawn-Takt, Punkte,
  Power-ups, Waffenstufen, Bossphasen und Kollisionen.
- Spieler, Gegner, Bosse, Projektile, Energiekapseln und Kollisionsgrenzen sind
  getrennte Objekte.
- `G10_SolarShooterGame` implementiert `IB5RoomMiniGame` und uebersetzt
  Tastatureingaben.
- `G10_VoidCourierRenderer` zeichnet die 160x144-Spielszene als skalierbare
  WPF-Vektorgrafik mit eigener Farbpalette und drei selbst gestalteten
  Umgebungen.
- Die Game-Boy-Auswahl erzeugt das Minispiel ueber `B5RoomRegistry`; Framework-
  Interfaces und Fenster bleiben unveraendert.
- Updates verwenden das vorhandene feste Intervall von 40 ms.

## Prototypumfang

- Bewegung in acht Richtungen, Dauerfeuer und drei Schusskonfigurationen
- Drei eigene Sektoren mit unterschiedlichen Hintergruenden und Gegnerwellen
- Drei Gegnertypen mit unterschiedlichen Bewegungen und Angriffstakten
- Sektor-Bosse mit angepasster Bewegung, Gesundheit und Angriffsmuster
- Energiekapseln, Waffenstufen, Punkte, Leben und Treffer-Unverwundbarkeit
- Game-Over-, Sektorwechsel- und Siegzustand; Neustart nach Spielende
- Farbige Raumschiff- und Gegnerformen mit Metallverlaeufen, Glasdetails und
  Triebwerksgluehen
- Mehrschichtiger Weltraumhintergrund, farbige Waffen und begrenzte
  Treffer-/Explosionsanimationen

Die Darstellung ist bewusst keine fotorealistische Echtzeit-3D-Szene: Das
Minispiel wird als WPF-Overlay gerendert und nutzt keine 3D-Raumschiffmodelle.
Das 160x144-Koordinatensystem bleibt fuer die vorhandene Minispiel- und
Bildschirm-Integration erhalten; WPF skaliert die Vektoren fuer die Anzeige.

Die Namen, Gegner, Bossmuster und Umgebungen sind eigene Designentscheidungen,
keine Behauptungen ueber Inhalte des Originals. Das Framework liefert kein
`KeyUp`; Richtung und Feuer werden deshalb kurz gepuffert und laufen nach dem
letzten `KeyDown`-Ereignis aus.

## Pruefung

Der Build wird mit `dotnet build` geprueft. Beim manuellen Start sind die
Game-Boy-Auswahl, Pixel-Skalierung, Steuerung, Waffenstufen, Energiekapseln,
Gegnerbewegungen, Bosskaempfe, Sektorwechsel, Game Over und Neustart zu
pruefen.

Vor einer Erweiterung zu einem groesseren Spielkonzept ist die Vorstellung beim
Dozenten gemaess Projektanforderungen zu klaeren.
