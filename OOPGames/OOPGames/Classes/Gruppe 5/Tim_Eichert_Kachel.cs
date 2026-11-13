using System;

namespace OOPGames
{
    internal sealed class G5_Kachel : IG5_KachelInfo
    {
        private G5_Gelaendeart _gelaende;
        private G5_Rohstofftyp _rohstoff;
        private int _rohstoffMenge;
        private bool _istErkundet;
        private bool _istAktuellSichtbar;

        internal G5_Kachel()
        {
            _gelaende = G5_Gelaendeart.Begehbar;
            _rohstoff = G5_Rohstofftyp.Keiner;
        }

        G5_Gelaendeart IG5_KachelInfo.Gelaende => _gelaende;

        G5_Rohstofftyp IG5_KachelInfo.Rohstoff => _rohstoff;

        int IG5_KachelInfo.RohstoffMenge => _rohstoffMenge;

        bool IG5_KachelInfo.IstErkundet => _istErkundet;

        bool IG5_KachelInfo.IstAktuellSichtbar => _istAktuellSichtbar;

        internal void SetGelaende(G5_Gelaendeart gelaende)
        {
            if (!Enum.IsDefined(typeof(G5_Gelaendeart), gelaende))
            {
                throw new ArgumentOutOfRangeException(nameof(gelaende));
            }

            _gelaende = gelaende;
        }

        internal void SetRohstoff(G5_Rohstofftyp rohstoff, int menge)
        {
            if (!Enum.IsDefined(typeof(G5_Rohstofftyp), rohstoff))
            {
                throw new ArgumentOutOfRangeException(nameof(rohstoff));
            }

            if (menge < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(menge));
            }

            if (rohstoff == G5_Rohstofftyp.Keiner && menge != 0)
            {
                throw new ArgumentException("Ohne Rohstofftyp darf die Menge nur null sein.", nameof(menge));
            }

            _rohstoff = rohstoff;
            _rohstoffMenge = menge;
        }

        internal bool TryReduceRohstoff(int menge)
        {
            if (menge <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(menge));
            }

            if (_rohstoff == G5_Rohstofftyp.Keiner || menge > _rohstoffMenge)
            {
                return false;
            }

            _rohstoffMenge -= menge;
            if (_rohstoffMenge == 0)
            {
                _rohstoff = G5_Rohstofftyp.Keiner;
            }

            return true;
        }

        internal void SetSichtbarkeit(bool istSichtbar)
        {
            _istAktuellSichtbar = istSichtbar;
            if (istSichtbar)
            {
                _istErkundet = true;
            }
        }
    }
}
