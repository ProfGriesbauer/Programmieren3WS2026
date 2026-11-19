using System;
using System.Windows;

namespace OOPGames
{
    public sealed class G5_WeltSpielfeld : IG5_WeltSpielfeld, IG5_KartenansichtSteuerung
    {
        private readonly int _breite;
        private readonly int _hoehe;
        private readonly G5_Kachel[,] _kacheln;
        private readonly G5_Kartenansicht _kartenansicht;

        public G5_WeltSpielfeld(int breite, int hoehe)
        {
            if (breite <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(breite));
            }

            if (hoehe <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(hoehe));
            }

            _breite = breite;
            _hoehe = hoehe;
            _kacheln = new G5_Kachel[hoehe, breite];
            _kartenansicht = new G5_Kartenansicht();

            for (int y = 0; y < hoehe; y++)
            {
                for (int x = 0; x < breite; x++)
                {
                    _kacheln[y, x] = new G5_Kachel();
                }
            }
        }

        int IG5_WeltSpielfeld.Breite => _breite;

        int IG5_WeltSpielfeld.Hoehe => _hoehe;

        bool IG5_WeltSpielfeld.TryGetKachelInfo(int x, int y, out IG5_KachelInfo info)
        {
            if (!TryGetKachel(x, y, out G5_Kachel kachel))
            {
                info = null;
                return false;
            }

            info = kachel;
            return true;
        }

        bool IG5_WeltSpielfeld.TryBildschirmZuRaster(int screenX, int screenY, out int x, out int y)
        {
            return _kartenansicht.TryBildschirmZuRaster(
                screenX,
                screenY,
                _breite,
                _hoehe,
                out x,
                out y);
        }

        bool IG5_WeltSpielfeld.TryRasterZuBildschirm(int x, int y, out Point position)
        {
            return _kartenansicht.TryRasterZuBildschirm(x, y, _breite, _hoehe, out position);
        }

        bool IGameField.CanBePaintedBy(IPaintGame painter)
        {
            return painter is IG5_WeltPainter;
        }

        void IG5_KartenansichtSteuerung.Verschieben(double deltaX, double deltaY)
        {
            _kartenansicht.Verschieben(deltaX, deltaY);
        }

        void IG5_KartenansichtSteuerung.Zoomieren(double faktor, double fokusX, double fokusY)
        {
            _kartenansicht.Zoomieren(faktor, fokusX, fokusY);
        }

        internal bool TryGetKachel(int x, int y, out G5_Kachel kachel)
        {
            if (!IstInnerhalb(x, y))
            {
                kachel = null;
                return false;
            }

            kachel = _kacheln[y, x];
            return true;
        }

        internal bool TrySetGelaende(int x, int y, G5_Gelaendeart gelaende)
        {
            if (!TryGetKachel(x, y, out G5_Kachel kachel))
            {
                return false;
            }

            kachel.SetGelaende(gelaende);
            return true;
        }

        internal bool TrySetRohstoff(int x, int y, G5_Rohstofftyp rohstoff, int menge)
        {
            if (!TryGetKachel(x, y, out G5_Kachel kachel))
            {
                return false;
            }

            kachel.SetRohstoff(rohstoff, menge);
            return true;
        }

        internal bool TryReduceRohstoff(int x, int y, int menge)
        {
            return TryGetKachel(x, y, out G5_Kachel kachel) && kachel.TryReduceRohstoff(menge);
        }

        internal bool TrySetSichtbarkeit(int x, int y, bool istSichtbar)
        {
            if (!TryGetKachel(x, y, out G5_Kachel kachel))
            {
                return false;
            }

            kachel.SetSichtbarkeit(istSichtbar);
            return true;
        }

        internal void VerschiebeAnsicht(double deltaX, double deltaY)
        {
            ((IG5_KartenansichtSteuerung)this).Verschieben(deltaX, deltaY);
        }

        internal void AendereZoom(double faktor, double fokusX, double fokusY)
        {
            ((IG5_KartenansichtSteuerung)this).Zoomieren(faktor, fokusX, fokusY);
        }

        private bool IstInnerhalb(int x, int y)
        {
            return x >= 0 && x < _breite && y >= 0 && y < _hoehe;
        }
    }
}
