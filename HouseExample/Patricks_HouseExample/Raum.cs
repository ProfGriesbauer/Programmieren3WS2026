namespace PatrickHouseExample;

public class Raum
{
    public string Name { get; }
    public Licht Licht { get; }
    public Heizung Heizung { get; }
    public Fenster Fenster { get; }
    public decimal Temperatur { get; private set; }

    public Raum(string name, decimal temperatur)
    {
        Name = name;
        Temperatur = temperatur;
        Licht = new Licht { VerbrauchProStunde = 0.25m };
        Heizung = new Heizung { LeistungKw = 2.5m, ZielTemperatur = temperatur };
        Fenster = new Fenster();
    }

    public void SetzeTemperatur(decimal temperatur)
    {
        Temperatur = temperatur;
        Heizung.ZielTemperatur = temperatur;
    }

    public void EnergieSparen()
    {
        Licht.Ausschalten();
        Heizung.Ausschalten();
        Fenster.Schliessen();
    }

    public void PartyModus()
    {
        Licht.SetzeHelligkeit(100);
        Heizung.Einschalten();
        Heizung.ZielTemperatur = 23m;
        Fenster.Oeffnen();
        Temperatur = 23m;
    }

    public override string ToString()
    {
        return $"{Name}: Licht {Licht} | Heizung {Heizung} | Fenster {Fenster} | Temperatur {Temperatur}°C";
    }
}
