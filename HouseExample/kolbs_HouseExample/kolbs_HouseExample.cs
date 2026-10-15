public class SteffisHaus
{
public string Name = string.Empty;
public string Vorname = string.Empty;
public string Straße = string.Empty;
public int Hausnummer;

    private double GrundStromA = 2.0;
    private double WasserM3proH = 0.5;
    private double HeizungsleistungKW = 5.0;

    private double stromverbrauch;
    private double wasserverbrauch;
    private double heizungsleistung;
    private bool heizungAn;
    private bool hauptstromAn;

    public double Stromverbrauch
{
    get
    {
        return stromverbrauch;
    }
}
    public double Wasserverbrauch => wasserverbrauch;

    public double Heizungsleistung => heizungsleistung;

    public bool HauptstromAn
    {
        get => hauptstromAn;
        set
        {
            hauptstromAn = value;
            stromverbrauch = value ? GrundStromA : 0;

            if (!value)
            {
                HeizungAn = false;
            }
        }
    }

    public bool HeizungAn
    {
        get => heizungAn;
        set
        {
            if (value && !hauptstromAn)
            {
                return;
            }

            heizungAn = value;
            heizungsleistung = value ? HeizungsleistungKW : 0;
        }
    }

    public void WasserAufdrehen(bool an)
    {
        wasserverbrauch = an ? WasserM3proH : 0;
    }

    public double BerechneWasserverbrauch(double stunden)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(stunden);
        return wasserverbrauch * stunden;
    }
}
