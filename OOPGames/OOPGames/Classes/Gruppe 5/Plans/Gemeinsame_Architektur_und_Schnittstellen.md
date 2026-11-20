# Gemeinsame Architektur und Schnittstellen – Gruppe 5

## Ziel

Das Fabrikspiel verwendet ein gemeinsames Welt-, Gebäude-, Gegenstands- und Regelmodell. Die Teilbereiche werden nicht durch Kopien oder eigene Item-Enums verbunden, sondern greifen auf dieselben öffentlichen Typen und Verträge im Namespace `OOPGames` zu.

Der einzige aktive Regelbaustein für das Fabrikspiel ist `SatisfactoryGameRules`. Das ältere, kleinere `G5_FabrikRules`-Prototypregelwerk wurde entfernt. Painter, Rules und Player werden einmal im bestehenden Registrierungsblock in `MainWindow.xaml.cs` registriert; eine zusätzliche Registrierung beim Laden des Moduls findet nicht statt.

## Verwendete Programmteile und Herkunft

| Teil | Herkunft / Datei | Verwendung im gemeinsamen Spiel |
|---|---|---|
| Kartenraster, Kacheln und Kartenansicht | Tim – `Tim_Eichert_WeltSpielfeld.cs`, `Tim_Eichert_Kachel.cs`, `Tim_Eichert_Kartenansicht.cs` | Einzige Quelle für Raster, Gelände, Rohstoffdarstellung und Bildschirm-/Rasterumrechnung |
| Fabrik-Feldprojektion | Tim – `Tim_Eichert_FabrikSpielfeld.cs` | Aggregiert Welt und Simulation; veröffentlicht Anzeigezustand read-only |
| Tastatur-/Maussteuerung | Tim – `Tim_Eichert_FabrikPlayer.cs` | Erzeugt `IG5_FabrikAktion`; enthält keine Kopie des Welt- oder Gebäudemodells |
| Darstellung | Tim – `Tim_Eichert_FabrikPainter.cs`, `Tim_Eichert_FabrikGebaeudeZeichner.cs` | Liest Welt und Fabrik ausschließlich über die Gruppe-5-Leseinterfaces; zeichnet die zehn nativen WPF-Gebäudedesigns |
| Spielregeln und Startkarte | Maxi – `Satisfactory/SatisfactoryGameRules.cs` | Einziges Regelwerk; initialisiert die Karte, validiert Bauaktionen und koordiniert Transfer und Simulation |
| Gegenstände, Positionen, Rezepte und Upgrades | Chris – `Christian_Frey_FabrikDatenmodelle.cs` | Gemeinsame Definition von `G5_Item`, `G5_GridPosition`, `G5_Recipe` und zugehörigen Fabrikdaten |
| Gebäude und Rohstoffvorkommen | Chris – `Christian_Frey_FabrikGebaeude.cs` | Gemeinsame Gebäudebasis sowie Miner, Schmelze, Konstruktor, Lager und Ressourcenmodell |
| Transport, Strom und Itemtransfer | Chris – `Christian_Frey_FabrikTransportEnergie.cs` | Gemeinsame Förderband-/Stromkomponenten und atomarer Transfer zwischen Item-Endpunkten |
| Simulation und Startrezepte | Chris – `Christian_Frey_FabrikSimulation.cs` | Gemeinsamer Tick für Gebäude und Definition der initialen Rezepte |
| Gemeinsame Verträge | Gruppe 5 – Dateien unter `Interfaces/gruppe 5/` | Verbindliche Grenzen zwischen Karte, UI, Spielregeln, Gebäuden, Transport und Strom |

Die eingebaute Startkarte verwendet dieselben Gebäude-, Rezept-, Item- und Simulationsobjekte wie die platzierbaren Gebäude. Es gibt keine zweite Item- oder Produktionsklassenfamilie für die Startkarte.

## Einzige Gegenstandsdefinition

`G5_Item` aus `Christian_Frey_FabrikDatenmodelle.cs` ist die kanonische Item-ID für Gebäudepuffer, Rezepte, Lager, Förderbänder, Strombrennstoff und HUD-Abfragen. Neue Spielteile sollen keinen parallelen Gegenstands-Enum oder eine zweite Itemklasse anlegen; sie verwenden `G5_Item` direkt.

`G5_Rohstofftyp` beschreibt dagegen, was auf einer Kartenkachel sichtbar ist. Es ist kein zweites Inventar-Item-Modell: Ein Kartenfeld mit `G5_Rohstofftyp.Eisen` erzeugt beispielsweise `G5_Item.Eisenerz`. Diese Zuordnung erfolgt in den Spielregeln.

Die Gruppe-8-Anwendung Hay Day bleibt ein eigenständiges, separat auswählbares Spiel. Ihre Ernte- und Hofdaten sind kein Teil des Fabrikspiels und werden deshalb nicht in dessen Gegenstandsmodell vermischt.

## Gemeinsame Schnittstellen

| Vertrag | Wer implementiert / verwendet ihn | Zweck und Regeln |
|---|---|---|
| `IG5_WeltSpielfeld` | `G5_WeltSpielfeld`; Player, Rules und Painter lesen | Kartengröße, read-only Kachelzugriff und Bildschirm-/Rasterumrechnung |
| `IG5_KachelInfo` | interne `G5_Kachel`; Rules und Painter lesen | Gelände, sichtbarer Rohstoffbestand und Sichtstatus; keine Setter |
| `IG5_FabrikSpielInfo` | `G5_FabrikSpielfeld`; Player und Painter lesen | Aktuelles Bauwerkzeug, Gebäudeprojektionen, HUB-Mengen und Statusmeldung |
| `IG5_GebaeudeInfo` | read-only Projektion in `G5_FabrikSpielfeld`; Painter liest | Gebäudetyp, Name, Position, Footprint, Stufe und Zustand; keine veränderbaren Gebäudereferenzen |
| `IG5_FabrikAktion` | Player erzeugt; `SatisfactoryGameRules` verarbeitet | Unveränderlicher Befehl für Werkzeugwahl oder Gebäudeplatzierung |
| `IG5_ItemEndpoint` | Gebäude und Lager implementieren; Transfer-/Förderlogik verwendet | Lesbarer Bestand und kontrollierte Aufnahme, Entnahme oder Wiederherstellung eines `G5_Item` |
| `IG5_PowerConsumer` | Verbraucher implementieren; Stromnetz verwendet | Position, Leistungsbedarf/-priorität und Stromstatus |
| `IG5_KartenansichtSteuerung` | Welt-/Fabrikfeld implementiert; Eingabesteuerung verwendet | Zoom und Verschiebung als getrennte Befehle; nicht Teil der read-only-Weltsicht |

### Datenfluss

1. `G5_FabrikHumanPlayer` liest `IG5_WeltSpielfeld` und `IG5_FabrikSpielInfo` und erzeugt eine `IG5_FabrikAktion`.
2. `SatisfactoryGameRules` prüft die Aktion und ändert Welt bzw. Simulation. Nur dieses Regelwerk koordiniert die konkreten Komponenten.
3. `G5_FabrikSpielfeld` stellt Kacheln und Fabrikzustand über read-only-Interfaces bereit.
4. `G5_FabrikPainter` liest diese Interfaces und zeichnet; er ändert keine Spielzustände.
5. Gebäude und Lager verwenden `G5_Item` durchgängig. `G5_ItemTransfer` kommuniziert zwischen ihnen über `IG5_ItemEndpoint`.
6. `G5_FactorySimulation` aktualisiert Maschinen; `IG5_PowerConsumer` verbindet Verbraucher mit der Stromverteilung.

Für neue Teambeiträge gilt: erst den passenden gemeinsamen Vertrag verwenden; neue Schnittstellen nur ergänzen, wenn keine vorhandene den benötigten Zugriff abdeckt. Änderungen an diesen öffentlichen Verträgen müssen mit allen davon abhängigen Teilbereichen abgestimmt werden.

## Aktueller Nutzungsstand

„Im Spiel verwendet“ bedeutet, dass die aktuelle registrierte Satisfactory-Spielrunde den Typ direkt erzeugt, aufruft oder ausliest. „Vorhanden, aber noch nicht im Spielablauf“ bedeutet, dass der Baustein kompiliert und für Erweiterungen vorgesehen ist, aber derzeit kein Werkzeug, keine Karte oder Regelaktion ihn aktiviert.

| Dateien / Klassen | Status im aktuellen Fabrikspiel |
|---|---|
| `Satisfactory/SatisfactoryGameRules.cs` – `SatisfactoryGameRules` | **Aktiv.** Einziges Fabrik-Regelwerk; bei Start registriert, erzeugt die Karte und Startschmelzen, platziert Miner/Schmelzen und tickt/koordiniert Transfer. |
| `Tim_Eichert_FabrikPlayer.cs` – `G5_FabrikHumanPlayer`, `G5_FabrikAktion` | **Aktiv.** Der Player ist registriert und liefert die Aktionen, die das Regelwerk verarbeitet. |
| `Tim_Eichert_FabrikPainter.cs` – `G5_FabrikPainter` | **Aktiv.** Registrierter Painter; zeichnet Welt-, Gebäude- und Hub-Anzeige. |
| `Tim_Eichert_FabrikSpielfeld.cs` – `G5_FabrikSpielfeld`, `G5_GebaeudeInfo` | **Aktiv.** Hält Welt, Hub und Simulation zusammen und stellt die read-only Anzeige bereit. |
| `Tim_Eichert_WeltSpielfeld.cs`, `Tim_Eichert_Kachel.cs`, `Tim_Eichert_Kartenansicht.cs` | **Aktiv.** Erzeugen die Rasterwelt, speichern Kachelzustand und rechnen Bildschirm-/Rasterkoordinaten um. Zoom-/Verschiebeeingaben sind derzeit nicht angeschlossen. |
| `Christian_Frey_FabrikDatenmodelle.cs` – `G5_Item`, `G5_GridPosition`, `G5_Recipe`, Status-/Rezepttypen | **Aktiv.** Gemeinsame Identitäten und Daten, die Regeln, Puffer, Rezepte und Gebäude verwenden. `G5_BuildingUpgrade` ist dagegen aktuell nur ein Erweiterungsbaustein. |
| `Christian_Frey_FabrikGebaeude.cs` – `G5_ItemBuffer`, `G5_Building`, `G5_ResourceDeposit`, `G5_Miner`, `G5_ProductionBuilding`, `G5_Smelter`, `G5_Storage` | **Aktiv.** Werden in Startwelt, Platzierung, Tick, Transfer oder HUD verwendet. |
| `Christian_Frey_FabrikSimulation.cs` – `G5_FactorySimulation`, `G5_StarterRecipes` | **Aktiv, teilweise.** Simulation und Schmelzrezepte werden verwendet. Bau- und Montage-Rezeptlisten werden noch nicht aufgerufen. |
| `Christian_Frey_FabrikTransportEnergie.cs` – `G5_ItemTransfer` | **Aktiv.** Wird für die vereinfachten Transfers über den Hub benutzt. |
| Dieselbe Transport-/Energiedatei – `G5_Conveyor`, `G5_BurningGenerator`, `G5_PowerPole` | **Noch nicht im Spielablauf.** Die Klassen sind für Erweiterungen vorhanden, aber das aktuelle Regelwerk platziert sie nicht. |
| `G5_PowerGrid`, `IG5_PowerConsumer` | **Simulations-Hook aktiv, Spielfunktion nicht aktiv.** Das Grid wird pro Simulations-Tick aufgerufen; aktuell werden keine Generatoren oder Masten gebaut und die platzierten Miner/Schmelzen erhalten Leistungsbedarf 0. |
| `G5_Constructor`, `G5_Assembler` | **Noch nicht im Spielablauf.** Es gibt weder Bauwerkzeuge noch Produktionsaktionen, die diese Gebäude erzeugen. |
| `IG5_KartenansichtSteuerung` | **Derzeit ohne Eingabe-Aufrufer.** Welt und Fabrikfeld implementieren/weiterleiten den Vertrag; es gibt aber noch kein angeschlossenes Maus-Rad-/Drag-Steuerelement. |
| `IG5_WeltPainter` | **Aktiver Basiskontrakt.** `IG5_FabrikPainter` erweitert ihn; eine zusätzliche, separate Welt-Painterklasse ist nicht vorhanden und nicht nötig für das Fabrikspiel. |
| Vier-gewinnt-Dateien in `Classes/Gruppe 5` und `Interfaces/gruppe 5` | **Aktiv als separates Vier-gewinnt-Spiel**, nicht als Teil des Fabrikspiels. |
| `Plans/*.md`, `Satisfactory/Anleitung.md`, Promptdateien und HTML-Designvorschau | **Dokumentation/Entwurf**, kein Laufzeitcode. |

Die frühere Klasse `G5_FabrikRules` ist im aktuellen lokalen Arbeitsstand entfernt; `SatisfactoryGameRules` ersetzt sie. Der Löschschritt ist noch nicht committed/gepusht. Der Vier-gewinnt-Code bleibt ein anderes, separat registriertes Gruppe-5-Spiel und ist kein doppeltes Fabrikmodell.

### Chris' Gebäudedesigns

`G5_FabrikGebaeudeZeichner` enthält WPF-Vektorzeichnungen für alle zehn Designs aus der HTML-Vorschau. Die read-only Projektion ordnet vorhandene Gebäude den Designs Miner, Schmelze, Konstruktor, Assembler, Generator, Strommast und Laufband zu; das `G5_Storage`-Objekt des aktuellen HUB wird als Lager-Silo gezeichnet. Die Stufen 1–3 werden durch MK-Kennzeichnung und passende Farbakzente dargestellt.

Die Zeichenoptionen für Splitter und Merger sind vorbereitet, aber im aktuellen Modell nicht erreichbar: Es gibt dafür noch keine Gebäudeklassen. Auch die übrigen noch nicht freigeschalteten Gebäude werden durch die Darstellung allein nicht platzierbar. Werkzeuge, Regeln, Produktionssimulation und `MainWindow.xaml.cs` wurden dafür nicht erweitert.
