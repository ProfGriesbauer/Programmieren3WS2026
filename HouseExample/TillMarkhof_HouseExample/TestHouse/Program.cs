using System;

namespace TillMarkhof.HouseExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var haus = new Haus(10m, 0.1m, 5m);
            haus.HauptstromAn = true;
            haus.HeizungAn = true;

            Console.WriteLine($"Hauptstrom: {(haus.HauptstromAn ? "An" : "Aus")}");
            Console.WriteLine($"Stromverbrauch: {haus.Stromverbrauch} A");
            Console.WriteLine($"Heizung: {(haus.HeizungAn ? "An" : "Aus")}");
            Console.WriteLine($"Heizungsleistung: {haus.Heizungsleistung} kW");
            Console.WriteLine($"Wasserverbrauch nach 2 Stunden: {haus.BerechneWassermenge(2m)} m³");
        }
    }
}