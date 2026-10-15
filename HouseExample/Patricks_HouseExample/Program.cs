using System;

namespace PatrickHouseExample;

public class Program
{
    public static void Main(string[] args)
    {
        var haus = new Haus(0.8m);
        haus.RaumHinzufügen(new Raum("Wohnzimmer", 21m));
        haus.RaumHinzufügen(new Raum("Schlafzimmer", 19m));
        haus.RaumHinzufügen(new Raum("Küche", 22m));
        haus.RaumHinzufügen(new Raum("Badezimmer", 24m));
        haus.RaumHinzufügen(new Raum("Arbeitszimmer", 20m));
        haus.RaumHinzufügen(new Raum("Gartenhaus", 18m));

        haus.HauptstromAn = true;
        haus.Räume[0].Licht.SetzeHelligkeit(80);
        haus.Räume[1].Licht.SetzeHelligkeit(30);
        haus.Räume[2].Licht.Einschalten();
        haus.Räume[2].Heizung.Einschalten();
        haus.Räume[2].Heizung.ZielTemperatur = 23m;
        haus.Räume[3].Fenster.Oeffnen();
        haus.Räume[4].Licht.SetzeHelligkeit(60);

        Console.WriteLine("Vor dem Modus:");
        Console.WriteLine(haus.Statusbericht());
        Console.WriteLine();

        haus.ToggleLicht("Arbeitszimmer");
        Console.WriteLine("Nach Lichtwechsel:");
        Console.WriteLine(haus.Statusbericht());
        Console.WriteLine();

        haus.PartyModus();
        Console.WriteLine("Party-Modus:");
        Console.WriteLine(haus.Statusbericht());
        Console.WriteLine($"Wasser für 3 Stunden: {haus.BerechneWassermenge(3m)} m³");
        Console.WriteLine();

        haus.Energiesparmodus();
        Console.WriteLine("Energiesparmodus:");
        Console.WriteLine(haus.Statusbericht());
    }
}
