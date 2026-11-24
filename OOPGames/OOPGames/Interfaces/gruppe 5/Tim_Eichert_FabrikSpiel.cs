using System.Collections.Generic;

namespace OOPGames
{
    // Bauwerkzeuge sind Befehlswerte fuer die Auswahl im Player und die Platzierung in den Rules.
    public enum G5_Bauwerkzeug
    {
        Keines,
        Foerderer,
        Schmelze,
        Foerderband,
        GebaeudeVerschieben,
        Konstruktor,
        Assembler,
        Verbrennungsgenerator,
        Strommast,
        Lager
    }

    public enum G5_FabrikGebaeudeTyp
    {
        Hub,
        Foerderer,
        Schmelze,
        Konstruktor,
        Assembler,
        Verbrennungsgenerator,
        Strommast,
        Lager,
        Laufband,
        Splitter,
        Merger
    }

    public enum G5_FabrikAktionstyp
    {
        WerkzeugWaehlen,
        GebaeudePlatzieren,
        GebaeudeBauen,
        SpielerBewegen,
        RohstoffAbbauen,
        GebaeudeAnsehen,
        RezeptWaehlen,
        GebaeudeUpgraden,
        GebaeudeVerkaufen,
        BaumenueUmschalten,
        TutorialUeberspringen,
        GebaeuderegisterUmschalten,
        GebaeuderegisterScrollen
    }

    public enum G5_SpielerBlickrichtung
    {
        Norden,
        Osten,
        Sueden,
        Westen
    }

    public interface IG5_GebaeudeInfo
    {
        G5_FabrikGebaeudeTyp Typ { get; }
        string Name { get; }
        string Beschreibung { get; }
        int X { get; }
        int Y { get; }
        int Breite { get; }
        int Hoehe { get; }
        int Level { get; }
        G5_BuildingStatus Status { get; }
        bool IstMitStromVersorgt { get; }
        string AusgewaehltesRezept { get; }
        IReadOnlyList<string> VerfuegbareRezepte { get; }
        IReadOnlyDictionary<G5_Item, int> UpgradeKosten { get; }
        bool KannVerbessertWerden { get; }
    }

    public interface IG5_FabrikSpielInfo : IG5_WeltSpielfeld
    {
        G5_Bauwerkzeug AktuellesWerkzeug { get; }
        IReadOnlyList<IG5_GebaeudeInfo> Gebaeude { get; }
        IG5_GebaeudeInfo AusgewaehltesGebaeude { get; }
        G5_GridPosition SpielerPosition { get; }
        G5_GridPosition ZielKachel { get; }
        G5_SpielerBlickrichtung SpielerBlickrichtung { get; }
        IReadOnlyDictionary<G5_Item, int> SpielerInventar { get; }
        int SpielerInventarKapazitaet { get; }
        int SpielerInventarBelegt { get; }
        bool BaumenueGeoeffnet { get; }
        int HubMenge(G5_Item item);
        string StatusMeldung { get; }
    }

    public interface IG5_FabrikPainter : IG5_WeltPainter
    {
    }

    public interface IG5_FabrikAktion : IPlayMove
    {
        G5_FabrikAktionstyp Aktionstyp { get; }
        G5_Bauwerkzeug Werkzeug { get; }
        int X { get; }
        int Y { get; }
    }
}
