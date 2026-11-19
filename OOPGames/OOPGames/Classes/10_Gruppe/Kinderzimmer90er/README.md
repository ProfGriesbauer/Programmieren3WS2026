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
Die gesamte Tower-Defense-Grafik stammt aus Spritesheets in
`Tower Defense/Assets` (Tower, Kartenhintergrund, Hauptgebaeude, Gegner, Bosse,
UI-Icons). `B5_TowerDefenseSprites.cs` laedt die Sheets einmalig und schneidet
die einzelnen Frames zu; die Pixel-Oberflaeche zeichnet sie mit
Nearest-Neighbor-Skalierung. Layouts und Generator sind in
`Tower Defense/SpriteGenerator/README.md` beschrieben.

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
- In der Geraeteansicht: 1-9 oder die Pfeiltasten waehlen ein Spiel, Enter oder Z startet es.
- Im Spiel: Pfeiltasten oder WASD steuern; Esc oder X geht zur Geraeteansicht zurueck.
  Ausnahme: Spiele mit `IB5RoomOwnBackKey` (Tower Defense) erhalten Esc/X selbst und
  oeffnen damit ein eigenes Menue; sie werden ueber `ExitRequested` verlassen.
- Void Courier: Pfeiltasten oder WASD bewegen das Schiff; Z oder Leertaste feuert.
  Nach Game Over oder dem Sieg startet Enter oder Z eine neue Runde. Dauerfeuer
  und Bewegung puffern Tastatureingaben kurz, da das Framework kein KeyUp liefert.
- Snake (Game Boy): Eine Pfeiltaste oder WASD startet und lenkt; Enter, Z, Leertaste
  oder P pausiert. Zwei schnell hintereinander gedrueckte Richtungen werden fuer die
  naechsten zwei Schritte gemerkt. Nach Game Over startet Enter eine neue Runde.
- Muenzen sammeln (Commodore): Pfeiltasten oder WASD bewegen das Kind um ein Feld;
  gehaltene Tasten laufen weiter. Enter, Z, Leertaste oder P pausiert, nach Game Over
  startet Enter eine neue Runde.
- Pong (Game Boy): Im Menue waehlen Pfeiltasten/WS oder 1/2 einen oder zwei Spieler,
  Enter startet. Allein bewegen W/S oder die Pfeiltasten den linken Schlaeger, zu zweit
  W/S den linken und die Pfeiltasten den rechten. Gehaltene Tasten bewegen weiter.
  Enter, Z, Leertaste oder P pausiert, nach dem Spiel geht Enter zurueck ins Menue.
- Esc oder X in der Geraeteansicht geht zurueck ins Zimmer.

Die Gameboy- und Atari-Quellen aus der Aufgabenbeschreibung waren in diesem
Repository nicht vorhanden. Alle Spiele sind daher eigenstaendige Minispiele.
Highscores liegen unter
`%LOCALAPPDATA%\\Programmieren3WS2026\\Kinderzimmer90er\\highscores.json`.

Snake (`Snake/`), Muenzen sammeln (`Muenzen/`) und Pong (`Pong/`) sind wie
Pac-Man aufgebaut: Spiellogik ohne WPF und ohne Tasten (`G10_SnakeSession`,
`G10_CoinSession`, `G10_PongSession`), nur lesende Sicht fuer den Renderer
(`IG10_SnakeView`, `IG10_CoinView`, `IG10_PongView` in
`Interfaces/10_Gruppe/Kinderzimmer90er/`) und ein Adapter als einzige Klasse
mit Tasten und Highscore-Datei (`G10_SnakeGame`, `G10_CoinGame`, `G10_PongGame`).
Die vier Game-Boy-Gruentoene liegen gemeinsam in `Shared/` (`G10_GameBoyColors`).

- **Snake**: 19 x 15 Felder, alle fuenf Aepfel ein Level schneller (bis Level 9).
  Nach jedem vierten Apfel krabbelt fuer sechs Sekunden ein Kaefer herum; je
  schneller er gefressen wird, desto mehr Punkte. Grafik in den vier Gruentoenen
  des ersten Game Boy.
- **Muenzen sammeln**: In jedem Level muessen innerhalb von 45 Sekunden 10 bis 20
  Muenzen eingesammelt werden; uebrige Sekunden gibt es als Bonus. Wollmaeuse
  springen diagonal durchs Zimmer und kosten bei Beruehrung ein Leben (drei Leben),
  ebenso das Ablaufen der Zeit. Ab und zu liegt fuer fuenf Sekunden ein Diamant
  (100 Punkte) herum. Vier Raumaufteilungen wechseln sich ab; mit jedem Level
  kommen mehr und schnellere Wollmaeuse. Grafik in den Farben des Commodore 64.
- **Pong**: Allein gegen den Computer oder zu zweit an einer Tastatur, wer zuerst
  sieben Punkte hat, gewinnt. Wo der Ball den Schlaeger trifft, bestimmt den
  Abprallwinkel (bis 55 Grad); jeder Treffer macht ihn etwas schneller. Der
  Computer (`G10_CpuPaddleControl`) folgt dem Ball langsamer als ein Spieler und
  zielt auf zufaellige Stellen seines Schlaegers; Spieler und Computer steuern ueber
  dasselbe Interface `IG10_PaddleControl` (Strategy). Als Highscore zaehlt der
  laengste Ballwechsel gegen den Computer.

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
