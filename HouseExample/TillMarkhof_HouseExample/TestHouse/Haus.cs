using System.Collections.Generic;

namespace TillMarkhof.HouseExample
{
    public class Haus
    {
        private readonly decimal _stromverbrauchBeiBetrieb;
        private readonly decimal _wasserverbrauchProStunde;
        private readonly decimal _heizungsleistungBeiBetrieb;
        private decimal _stromverbrauch;
        private decimal _wasserverbrauch;
        private decimal _heizungsleistung;
        private bool _heizungAn;
        private bool _hauptstromAn;

        public decimal Stromverbrauch => _stromverbrauch;
        public decimal Wasserverbrauch => _wasserverbrauch;
        public decimal Heizungsleistung => _heizungsleistung;
        public List<IRaum> Räume { get; }

        public bool HeizungAn
        {
            get => _heizungAn;
            set
            {
                _heizungAn = value && _hauptstromAn;
                _heizungsleistung = _heizungAn ? _heizungsleistungBeiBetrieb : 0m;
            }
        }

        public bool HauptstromAn
        {
            get => _hauptstromAn;
            set
            {
                _hauptstromAn = value;
                _stromverbrauch = _hauptstromAn ? _stromverbrauchBeiBetrieb : 0m;

                if (!_hauptstromAn)
                {
                    HeizungAn = false;
                }
            }
        }

        public Haus(
            decimal stromverbrauchBeiBetrieb,
            decimal wasserverbrauchProStunde,
            decimal heizungsleistungBeiBetrieb)
        {
            if (stromverbrauchBeiBetrieb < 0m)
                throw new ArgumentOutOfRangeException(nameof(stromverbrauchBeiBetrieb));
            if (wasserverbrauchProStunde < 0m)
                throw new ArgumentOutOfRangeException(nameof(wasserverbrauchProStunde));
            if (heizungsleistungBeiBetrieb < 0m)
                throw new ArgumentOutOfRangeException(nameof(heizungsleistungBeiBetrieb));

            _stromverbrauchBeiBetrieb = stromverbrauchBeiBetrieb;
            _wasserverbrauchProStunde = wasserverbrauchProStunde;
            _heizungsleistungBeiBetrieb = heizungsleistungBeiBetrieb;
            _wasserverbrauch = _wasserverbrauchProStunde;

            Räume = new List<IRaum>
            {
                new Raum(),
                new Raum(),
                new Raum(),
                new Raum()
            };
        }

        public decimal BerechneWassermenge(decimal stunden)
        {
            if (stunden < 0m)
                throw new ArgumentOutOfRangeException(nameof(stunden));

            return _wasserverbrauch * stunden;
        }
    }
}