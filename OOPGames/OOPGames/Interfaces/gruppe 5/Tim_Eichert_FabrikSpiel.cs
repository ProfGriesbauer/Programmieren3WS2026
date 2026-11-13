using System.Collections.Generic;

namespace OOPGames
{
    public enum G5_Bauwerkzeug
    {
        Keines,
        Foerderer,
        Schmelze
    }

    public enum G5_FabrikGebaeudeTyp
    {
        Hub,
        Foerderer,
        Schmelze
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
    }

    public interface IG5_FabrikSpielInfo : IG5_WeltSpielfeld
    {
        G5_Bauwerkzeug AktuellesWerkzeug { get; }
        IReadOnlyList<IG5_GebaeudeInfo> Gebaeude { get; }
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
