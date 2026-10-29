public class BeesHouse
{
    public string name;
    public int Hausnummer;
    public List<IRaum> Raeume { get; } = new()
    {
        new Raum(),
        new Raum(),
        new Raum(),
        new Raum()
    };

    private const double GrundStromA = 2.0;
    private const double WasserM3proH = 0.5;
    private const double HeizungKW = 5.0;

    private double stromverbrauch;
    private double wasserverbrauch;
    private double heizungsleistung;
    private bool hauptstromAn;
    private bool heizungAn;
    public double Stromverbrauch
    {
        get { return stromverbrauch+GrundStromA ; }
    }
    public double Wasserverbrauch
    {
        get { return wasserverbrauch+WasserM3proH; }
    }
    public double Heizungsleistung
    {
        get { return heizungsleistung+HeizungKW; }
    }
    public bool HauptstromAn
    {
        get { return hauptstromAn; }
        set
        {
            hauptstromAn = value;
            if (value)
            {
                stromverbrauch=GrundStromA;

            }
            else
            {
                stromverbrauch=0;
                heizungAn = false;
            }
        }
    }
    public double BerechneWasserverbrauch(double stunden)
    {
        return Wasserverbrauch * stunden;
    }
    

}