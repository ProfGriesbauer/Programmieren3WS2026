namespace PatrickHouseExample;

public class Heizung
{
    private bool _istAn;

    public bool IstAn => _istAn;
    public decimal LeistungKw { get; set; }
    public decimal ZielTemperatur { get; set; }

    public void Einschalten()
    {
        _istAn = true;
    }

    public void Ausschalten()
    {
        _istAn = false;
    }

    public override string ToString()
    {
        return $"{(IstAn ? "An" : "Aus")}, Ziel {ZielTemperatur}°C, Leistung {LeistungKw} kW";
    }
}
