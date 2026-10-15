public class Raum : IRaum
{
    private string name = "";
    private bool lichtAn;
    private double temperatur = 20.0;

    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }

    public bool LichtAn
    {
        get
        {
            return lichtAn;
        }
        set
        {
            lichtAn = value;
        }
    }

    public double Temperatur
    {
        get
        {
            return temperatur;
        }
        set
        {
            temperatur = value;
        }
    }
}
