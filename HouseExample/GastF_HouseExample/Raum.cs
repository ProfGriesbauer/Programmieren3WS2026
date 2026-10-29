public abstract class Raum : HouseExample.Interfaces.IRaum
{
    private bool lightOn;
    private double temperature;
    internal bool HasError { get; private set; }
    internal int RoomNumber { get; set; }

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

    public bool BlindDown
    {
        get => blindDown;
        set
        {
            if (value && this is not HouseExample.Interfaces.IRaumJalousie)
            {
                Console.WriteLine(
                    $"Raum {RoomNumber} hat keine Jalousie, die heruntergefahren werden kann.");
                HasError = true;
                return;
            }

            blindDown = value;
        }
    }

    private bool blindDown;
}
