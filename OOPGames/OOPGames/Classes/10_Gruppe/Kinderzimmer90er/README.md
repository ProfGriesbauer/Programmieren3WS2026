# Kinderzimmer (90er)

Das Spiel ist ueber `B5RoomPaint`, `B5RoomRules` und `B5RoomHumanPlayer` in
`MainWindow.xaml.cs` registriert. Die unveraenderten Framework-Interfaces laufen
ueber `IGameRules2`; Eingaben werden im Spieler gepuffert und im 40-ms-Tick verarbeitet.

## Visueller Stand

Das Zimmer behaelt seine 90er-Jahre-Einrichtung und wird innerhalb der
vorhandenen WPF-3D-Szene materiell und raeumlich detaillierter gestaltet:
warme Holzboeden mit Maserung, dezente Tapeten- und Teppichmuster,
texturierte Moebel- und Vorhangflaechen, ein gerahmtes Fenster mit Vorhaengen,
Pflanzendetails sowie weiche prozedurale Kontakt-Schatten. Warmes Raumlicht,
gerichtete Lichtquellen und dezente Glanzlichter modellieren die Oberflaechen.
Die Objekte bestehen nun aus mehr Einzelteilen: Buecher tragen individuell
gestaltete Ruecken mit Titeln und Autorennamen, Regale sichtbare Kanten und
Verbindungen, CRT-Bildschirme Einfassungen, Bedienelemente und Lueftungsschlitze.
Tastenkappen besitzen einzelne Beschriftungen; Poster und Pflanzentopf haben
zusaetzliche Rahmen- und Oberflaechendetails. Raumabmessungen,
Geraetepositionen und Spielablauf bleiben erhalten. Die Einlegeanimation laeuft
weiterhin in der 3D-Raumszene. Sobald ein Spiel aktiv ist, werden 3D-Viewport,
Geraeteauswahl und Steuerleiste ausgeblendet und nur das konsolenspezifische
2D-Spieloverlay angezeigt. Beim Zurueckkehren zur Geraeteauswahl wird die
bestehende Szene wieder eingeblendet.
Zwischen Einlegeanimation und Spieloverlay liegt eine kurze Fokusphase, in der
die Kamera auf den jeweiligen Konsolenbildschirm zoomt.
Das Overlay liegt dabei in einem konsolenspezifischen Gehaeuse: Gameboy mit
gruenem Handheld-Rahmen, Atari mit braunem Fernseher-Rahmen und Commodore mit
beigem CRT-Monitor-Rahmen.
Das Tower-Defense-Spiel verwendet darin eine Ausgabeaufloesung von
`640 x 360` im 16:9-Format. Die Zeichenlogik basiert weiterhin auf einer
320-x-180-Pixelbasis mit ganzzahligem Faktor 2. Die bestehende Tower-Defense-
Darstellung kann dadurch auf 720p mit Faktor 1 und bei groesseren Fenstern
weiterhin ohne weiche Zwischenpixel dargestellt werden. Die bestehenden Spiele
behalten ihre bisherigen Aufloesungen.
Tower Defense zeichnet seine Pixel-Art seitdem in eine einzelne gerenderte
Oberflaeche statt viele WPF-Rechtecke pro Frame zu erzeugen. Die bestehenden
Minispiele verwenden weiterhin ihre unveraenderten Canvas-Zeichenroutinen.
Wiederverwendete Farbpinsel und Nearest-Neighbor-Skalierung reduzieren dabei
unnoetige Frame-Allocations.

Die Szene verwendet prozedural erzeugte WPF-Materialien und Geometrie, keine
externen Texturen oder Modelle. Das WPF-3D-Rendering bietet keine physikalisch
korrekten Echtzeit-Schatten oder PBR-Reflexionen; Kontakt-Schatten und
Oberflaechenstrukturen sind deshalb angenaeherte Darstellungen und keine
Fotorealismus-Garantie.

## Rendering- und Performanceoptimierung

Der Zimmeraufbau war bereits auf die einmalige Erstellung beim Anlegen der
Viewport-Szene begrenzt; die Kamera hat keine statischen Objekte neu aufgebaut.
Im 40-ms-Tick wurden jedoch Bildschirm- und Auswahlmaterialien wiederholt neu
erzeugt, waehrend Kameraeigenschaften und unveraenderte UI-Zustaende laufend
gesetzt wurden. Zudem bestanden viele statische Einzelteile als separate
Geometrieobjekte.

Wiederkehrende Materialien, Bildschirmzustaende und Schatten-Brushes werden
jetzt eingefroren und wiederverwendet. Kompatible statische Meshes werden beim
Szenenaufbau nach Material zusammengefasst; dynamische Auswahl-Highlights und
Bildschirmflaechen bleiben getrennt. Auch die Geraetesteuerungs-Meshes werden
einmalig optimiert. Unveraenderte Titel-, Interface- und Materialzustaende
werden nicht erneut gezeichnet beziehungsweise gesetzt. Die Kamera aktualisiert
ihre Zielwerte weiter waehrend der Bewegung, stoppt aber Property-Updates nach
dem Einrasten und schreibt ihre feste Aufwaertsrichtung nur einmal. Im aktiven
Spiel wird der 3D-Viewport zudem ausgeblendet und die Kamera nicht weiter
aktualisiert. Die Details, Materialien, Positionen und Spielregeln bleiben
erhalten.

## Steuerung

- Im Zimmer: Pfeiltasten links/rechts oder A/D waehlen ein Geraet, Enter oder Z oeffnet es.
- In der Geraeteansicht: 1/2 waehlt ein Spiel, Enter oder Z startet es.
- Im Spiel: Pfeiltasten oder WASD steuern; Esc oder X geht zur Geraeteansicht zurueck.
- Void Courier: Pfeiltasten oder WASD bewegen das Schiff; Z oder Leertaste feuert.
  Nach Game Over oder dem Sieg startet Enter oder Z eine neue Runde. Dauerfeuer
  und Bewegung puffern Tastatureingaben kurz, da das Framework kein KeyUp liefert.
- Esc oder X in der Geraeteansicht geht zurueck ins Zimmer.

Die Gameboy- und Atari-Quellen aus der Aufgabenbeschreibung waren in diesem
Repository nicht vorhanden. Snake und Pong sowie das Atari-Spiel sind daher
eigenstaendige Minispiele; Atari und Commodore verwenden eine Variante des
Muenzen-Sammelspiels. Highscores liegen unter
`%LOCALAPPDATA%\\Programmieren3WS2026\\Kinderzimmer90er\\highscores.json`.

Void Courier ist ein eigenstaendiger Shoot-'em-up mit einer farbigen,
vektorbasierten 2.5D-Darstellung. WPF zeichnet die Szene in einem 160x144-
Koordinatensystem und skaliert die Vektorgrafik in den vorhandenen Spielrahmen.
Farbverlaeufe und Formen werden dadurch bei der vergroesserten Anzeige weich
und detailreich dargestellt, ohne eine hochaufgeloeste Bitmap pro Frame
anzulegen. Mehrschichtige Sternfelder, dezente Nebel, ferne Planeten,
metallisch schattierte Schiffe, Cockpitglas, Triebwerksgluehen, farbige
Projektile und begrenzte Treffer-/Explosionsanimationen bilden den neuen Look.

Das Minispiel bleibt ein 2D-Overlay in der WPF-Geraeteansicht; die Raumschiffe
sind keine echten 3D-Modelle und werden nicht als Textur auf die Game-Boy-
Displaygeometrie projiziert. Der Renderer verwendet ausschliesslich eigens
gezeichnete Formen und keine externen Spielgrafiken. Die drei selbst
entworfenen Sektoren RIFT, CANOPY und IRONWORKS behalten ihre eigenen
Hintergruende und Bossbegegnungen. Gegner, drei Waffenstufen, sammelbare
Energiekapseln, Leben, Punkte, Game Over und Sieg bleiben enthalten. Die
Spielinhalte sind eigenstaendige Entwuerfe und keine Rekonstruktion bestimmter
SolarStriker-Sprites, -Level oder -Bossmuster.
