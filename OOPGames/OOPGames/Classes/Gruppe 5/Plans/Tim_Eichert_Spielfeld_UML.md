# UML-Klassendiagramm: Spielfeld des 2D-Fabrikspiels

Das Diagramm zeigt die Trennung zwischen Weltzustand, Regeln, Painter und Eingabe. `G5_KachelAnsicht` entfällt: Die interne Kachel implementiert direkt ein nur lesendes Interface. Der Spieler bekommt vom Framework nur Eingabe und Feld, daher stellt das Feld eine minimale Bildschirm-zu-Raster-Umrechnung bereit. Die gruppenspezifischen Interfaces sind mit dem Team abzustimmen.

```mermaid
classDiagram
    direction LR

    class IGameField {
        <<interface>>
        +CanBePaintedBy(IPaintGame painter) bool
    }

    class IPaintGame {
        <<interface>>
        +PaintGameField(Canvas canvas, IGameField field) void
    }

    class IPaintGame2 {
        <<interface>>
        +TickPaintGameField(Canvas canvas, IGameField field) void
    }

    class IGameRules2 {
        <<interface>>
        +TickGameCall() void
    }

    class IGameRules {
        <<interface>>
        +CurrentField IGameField
        +MovesPossible bool
        +DoMove(IPlayMove move) void
        +ClearField() void
        +CheckIfPLayerWon() int
    }

    class IG5_WeltSpielfeld {
        <<interface>>
        +Breite: int
        +Hoehe: int
        +TryGetKachelInfo(x, y, out info): bool
        +TryBildschirmZuRaster(x, y, out rasterX, out rasterY): bool
        +RasterZuBildschirm(rasterX, rasterY): Punkt
    }

    class IG5_KachelInfo {
        <<interface>>
        +Gelaende: G5_Gelaendeart
        +Rohstoff: G5_Rohstofftyp
        +RohstoffMenge: int
        +IstErkundet: bool
        +IstAktuellSichtbar: bool
    }

    class IG5_WeltPainter {
        <<interface>>
        +PaintWeltField(Canvas, IG5_WeltSpielfeld): void
        +TickPaintWeltField(Canvas, IG5_WeltSpielfeld): void
    }

    class G5_WeltSpielfeld {
        -_breite: int
        -_hoehe: int
        -_kacheln: G5_Kachel[,]
        -_gebaeudeBelegung: G5_PlatzierbaresObjekt[,]
        -_laufbandBelegung: G5_PlatzierbaresObjekt[,]
        -_stromnetz: G5_Stromnetz
        -_kartenansicht: G5_Kartenansicht
        +IG5_WeltSpielfeld.Breite: int
        +IG5_WeltSpielfeld.Hoehe: int
        +IG5_WeltSpielfeld.TryGetKachelInfo(x, y, out info): bool
        +IG5_WeltSpielfeld.TryBildschirmZuRaster(x, y, out rasterX, out rasterY): bool
        +IG5_WeltSpielfeld.RasterZuBildschirm(rasterX, rasterY): Punkt
        +IGameField.CanBePaintedBy(IPaintGame painter): bool
        ~VerringereRohstoff(x, y, menge): bool
        ~VerschiebeAnsicht(dx, dy): void
        ~AendereZoom(faktor, fokus): void
    }

    class G5_Kachel {
        <<internal>>
        -_gelaende: G5_Gelaendeart
        -_rohstoff: G5_Rohstofftyp
        -_rohstoffMenge: int
        -_istErkundet: bool
        -_istAktuellSichtbar: bool
    }

    class G5_WeltHumanPlayer {
        +GetMove(IMoveSelection, IGameField): IPlayMove
    }

    class G5_PlatzierbaresObjekt {
        <<abstract>>
        -_x: int
        -_y: int
    }

    class G5_Stromnetz {
        -_verbindungen: Sammlung~Verbindung~
    }

    class G5_WeltRules {
        -_feld: G5_WeltSpielfeld
        +CurrentField: IGameField
        +TickGameCall(): void
        +AktionAusfuehren(aktion): void
        ~AnsichtAktionAusfuehren(aktion): void
    }

    class G5_WeltPainter {
        +PaintGameField(Canvas canvas, IGameField field): void
        +PaintWeltField(Canvas, IG5_WeltSpielfeld): void
        +TickPaintGameField(Canvas, IGameField): void
        +TickPaintWeltField(Canvas, IG5_WeltSpielfeld): void
    }

    class G5_Kartenansicht {
        -_zoom: double
        -_verschiebungX: double
        -_verschiebungY: double
        ~BildschirmZuRaster(x, y): Koordinate
        ~RasterZuBildschirm(x, y): Koordinate
    }

    class G5_Gelaendeart {
        <<enumeration>>
        Begehbar
        Hindernis
    }

    class G5_Rohstofftyp {
        <<enumeration>>
        Holz
        Eisen
        Kupfer
        Kohle
        Kalkstein
        Quarz
        Schwefel
        Uran
        Wasser
        Gas
        Oel
        Keiner
    }

    IGameRules <|-- IGameRules2
    IGameRules2 <|.. G5_WeltRules
    IPaintGame <|-- IPaintGame2
    IGameField <|-- IG5_WeltSpielfeld
    IG5_WeltSpielfeld <|.. G5_WeltSpielfeld
    IG5_KachelInfo <|.. G5_Kachel
    IPaintGame2 <|-- IG5_WeltPainter
    IG5_WeltPainter <|.. G5_WeltPainter
    G5_WeltSpielfeld "1" *-- "1..*" G5_Kachel : enthält
    G5_WeltSpielfeld "1" o-- "0..*" G5_PlatzierbaresObjekt : Gebäude-Layer
    G5_WeltSpielfeld "1" o-- "0..*" G5_PlatzierbaresObjekt : Laufband-Layer
    G5_WeltSpielfeld "1" *-- "1" G5_Stromnetz : Overlay
    G5_WeltSpielfeld "1" *-- "1" G5_Kartenansicht : gemeinsame Ansichtsberechnung
    G5_WeltSpielfeld ..> IG5_KachelInfo : gibt lesend zurück
    G5_WeltRules --> G5_WeltSpielfeld : verwaltet
    G5_WeltPainter --> IG5_WeltSpielfeld : liest Feld und Zeichenkoordinaten
    G5_WeltHumanPlayer --> IG5_WeltSpielfeld : erhält Feld und rechnet Klick um
    G5_Kachel ..> G5_Gelaendeart : speichert
    G5_Kachel ..> G5_Rohstofftyp : speichert
```

## Hinweise zur Umsetzung

- `IG5_WeltSpielfeld` zeigt nur Breite, Höhe, lesenden Kachelzugriff sowie Bildschirm-/Rasterumrechnung.
- `RasterZuBildschirm` ist die entsprechende lesende Umrechnung für den Painter; Zoom und Verschiebung selbst bleiben privat und werden kontrolliert über die Rules geändert.
- `G5_Kachel` ist intern und implementiert `IG5_KachelInfo`. Der Painter erhält nur das Interface und keine Arrays oder Setter; ein zusätzlicher Snapshot-Typ wäre unnötig.
- Gelände/Hindernis, Rohstoffvorkommen, Gebäude, Laufbänder und Stromnetz sind getrennte Ebenen. Das Stromnetz wird als Overlay modelliert und überschreibt keine Geländedaten.
- Der Rohstoff-Enum enthält die Typen aus `Anforderungen.md`; die erste Karte muss nicht alle Typen direkt verwenden.
- `IstErkundet` bleibt dauerhaft erhalten, `IstAktuellSichtbar` wird von den Rules anhand des Sicht-Radius aktualisiert.
- `CanBePaintedBy` wird am Feld aufgerufen und prüft auf `IG5_WeltPainter`. Die Richtung bedeutet: „Dieses Feld kann von diesem Painter gezeichnet werden.“
- Das Framework reicht dem Spieler in `GetMove` nur `IMoveSelection` und `IGameField` weiter. Deshalb liest der Spieler die gemeinsame Umrechnung über `IG5_WeltSpielfeld`; er bekommt nicht die Kartenansicht selbst.
- Die Belegung durch Gebäude oder Laufbänder ist vorläufig als internes `G5_PlatzierbaresObjekt` dargestellt. Der Painter benötigt bei Bedarf später eine separate, minimale Lesesicht auf deren Bilddaten.
- `~` bezeichnet im Diagramm interne Mitglieder, die nur innerhalb der Assembly verfügbar sind. Die Interface-Mitglieder sind als explizite Implementierungen dargestellt und werden über das jeweilige Interface aufgerufen.
- Das Raster ist im Diagramm als Sammlung von Kacheln dargestellt. Getrennte Gebäude- und Laufbandbelegungen verhindern, dass diese Ebenen oder Gelände/Rohstoffe einander überschreiben.
- `IGameRules2.TickGameCall()` ist optional und nur nötig, wenn die zeitbasierte Simulation den Tick-Aufruf des Frameworks nutzt.
