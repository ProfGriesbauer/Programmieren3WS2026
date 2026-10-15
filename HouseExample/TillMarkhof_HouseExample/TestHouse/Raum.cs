namespace TillMarkhof.HouseExample
{
    public class Raum : IRaum
    {
        private bool _lichtAn;
        private decimal _temperatur;

        public bool LichtAn
        {
            get => _lichtAn;
            set => _lichtAn = value;
        }

        public decimal Temperatur
        {
            get => _temperatur;
            set => _temperatur = value;
        }
    }
}
