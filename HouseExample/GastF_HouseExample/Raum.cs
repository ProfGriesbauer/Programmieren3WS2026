public abstract class Raum : HouseExample.Interfaces.IRaum
{
    private bool lightOn;
    private double temperature;

    public bool LightOn
    {
        get => lightOn;
        set => lightOn = value;
    }

    public double Temperature
    {
        get => temperature;
        set => temperature = value;
    }

    public bool BlindDown { get; set; }
}
