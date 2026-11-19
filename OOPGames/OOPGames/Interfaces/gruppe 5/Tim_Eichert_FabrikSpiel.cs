using System.Collections.Generic;

namespace OOPGames
{
    public enum G5_Bauwerkzeug
    {
        Keines,
        Foerderer,
        Schmelze,
        Konstruktor,
        Kupferschmelze,
        Foerderband
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
        Laufband,
        Splitter,
        Merger
    }

    public enum G5_FabrikAktionstyp
    {
        WerkzeugWaehlen,
        GebaeudePlatzieren
    }

    public interface IG5_GebaeudeInfo
    {
        G5_FabrikGebaeudeTyp Typ { get; }
        string Name { get; }
        int X { get; }
        int Y { get; }
        int Breite { get; }
        int Hoehe { get; }
        int Level { get; }
        G5_BuildingStatus Status { get; }
        bool IstMitStromVersorgt { get; }
        bool HatEingang { get; }
        bool HatAusgang { get; }
        G5_GridPosition EingangsPosition { get; }
        G5_GridPosition AusgangsPosition { get; }
        bool AusgangAusgewaehlt { get; }
        bool IstAusgewaehlt { get; }
        IReadOnlyDictionary<G5_Item, int> EingangsInhalt { get; }
        IReadOnlyList<string> VerfuegbareRezepte { get; }
        string AktuellesRezept { get; }
    }

    public interface IG5_FabrikSpielInfo : IG5_WeltSpielfeld
    {
        G5_Bauwerkzeug AktuellesWerkzeug { get; }
        IReadOnlyList<IG5_GebaeudeInfo> Gebaeude { get; }
        bool FoerderbaenderAktiv { get; }
        IReadOnlyList<IReadOnlyList<G5_GridPosition>> FoerderbandRouten { get; }
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
        int BildschirmX { get; }
        int BildschirmY { get; }
    }
}
