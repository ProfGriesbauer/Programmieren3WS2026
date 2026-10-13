using System;
using System.Collections.Generic;

namespace TillMarkhof.HouseExample
{
    public class Haus
    {
        public string Straßenname { get; set; }
        public int Hausnummer { get; set; }
        public decimal Große { get; set; }
        public List<Zimmer> Zimmer { get; set; }

        public Haus(string straßenname, int hausnummer, decimal große)
        {
            Straßenname = straßenname;
            Hausnummer = hausnummer;
            Große = große;
            Zimmer = new List<Zimmer>();
        }

        public void AddZimmer(Zimmer zimmer)
        {
            Zimmer.Add(zimmer);
        }

        public void Klingel()
        {
            Console.WriteLine("DING DONG!");
        }
    }
}