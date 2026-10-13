namespace TillMarkhof.HouseExample
{
    public class Zimmer
    {
        public string Funktion { get; set; }
        public decimal Große { get; set; }

        public Zimmer(string funktion, decimal große)
        {
            Funktion = funktion;
            Große = große;
        }
    }
}