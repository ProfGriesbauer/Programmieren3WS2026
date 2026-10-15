public class GrieslsHaus
{
    public string name;
    public int hausnummer;

    private const double GrundStromA = 2.0;
    private const double WasserM3proH = 0.5;
    private const double HeizungKW = 5.0;

    private double stromverbrauch;   // [A]
    private double wasserverbrauch;  // [m³/h]
    private double heizungsleistung; // [kW]
    private bool heizungAn;
    private bool hauptstromAn;

    public double Stromverbrauch
    {
        get
        {
            return GrundStromA + stromverbrauch;
        }
        set
        {
            stromverbrauch = value;
            if (value > 0)
            {
                hauptstromAn = true;
            }
            else
            {
                hauptstromAn = false;
                HeizungAn = false; // ohne Strom keine Heizung
            }
        }
    }

    public double Wasserverbrauch
    {
        get
        {
            return wasserverbrauch;
        }
    }

    public double Heizungsleistung
    {
        get
        {
            return heizungsleistung;
        }
    }

    public bool HauptstromAn
    {
        get
        {
            return hauptstromAn;
        }
        set
        {
            hauptstromAn = value;
            if (value)
            {
                stromverbrauch = GrundStromA;
            }
            else
            {
                stromverbrauch = 0;
                HeizungAn = false; // ohne Strom keine Heizung
            }
        }
    }

    public bool HeizungAn
    {
        get
        {
            return heizungAn;
        }
        set
        {
            if (value && !hauptstromAn)
            {
                return; // Heizung nur mit Strom einschaltbar
            }
            heizungAn = value;
            heizungsleistung = value ? HeizungKW : 0;
        }
    }

    public void WasserAufdrehen(bool an)
    {
        wasserverbrauch = an ? WasserM3proH : 0;
    }

    // Verbrauchte Wassermenge [m³] über die angegebene Dauer in Stunden
    public double BerechneWasserverbrauch(double stunden)
    {
        return wasserverbrauch * stunden;
    }
}
