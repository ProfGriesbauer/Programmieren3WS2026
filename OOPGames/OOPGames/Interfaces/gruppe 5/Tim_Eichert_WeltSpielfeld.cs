using System.Windows;

namespace OOPGames
{
    public enum G5_Gelaendeart
    {
        Begehbar,
        Hindernis
    }

    public enum G5_Rohstofftyp
    {
        Keiner,
        Holz,
        Eisen,
        Kupfer,
        Kohle,
        Kalkstein,
        Quarz,
        Schwefel,
        Uran,
        Wasser,
        Gas,
        Oel
    }

    public interface IG5_KachelInfo
    {
        G5_Gelaendeart Gelaende { get; }
        G5_Rohstofftyp Rohstoff { get; }
        int RohstoffMenge { get; }
        bool IstErkundet { get; }
        bool IstAktuellSichtbar { get; }
    }

    public interface IG5_WeltSpielfeld : IGameField
    {
        int Breite { get; }
        int Hoehe { get; }

        bool TryGetKachelInfo(int x, int y, out IG5_KachelInfo info);
        bool TryBildschirmZuRaster(int screenX, int screenY, out int x, out int y);
        bool TryRasterZuBildschirm(int x, int y, out Point position);
    }

    public interface IG5_KartenansichtSteuerung
    {
        void Verschieben(double deltaX, double deltaY);
        void Zoomieren(double faktor, double fokusX, double fokusY);
    }
}
