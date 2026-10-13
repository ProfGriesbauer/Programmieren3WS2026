using System;

namespace TillMarkhof.HouseExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Haus erstellen
            var meinHaus = new Haus("Hauptstraße", 42, 120.5m);

            // Zimmer hinzufügen
            meinHaus.AddZimmer(new Zimmer("Wohnzimmer", 30.0m));
            meinHaus.AddZimmer(new Zimmer("Schlafzimmer", 20.0m));
            meinHaus.AddZimmer(new Zimmer("Küche", 15.5m));

            // Klingel testen
            meinHaus.Klingel();
        }
    }
}