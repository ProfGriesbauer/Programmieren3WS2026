public class Raum : IRaum
{
    private bool lichtAn;
    private double temperatur;

    public bool LichtAn
    {
        get { return lichtAn; }
        set { lichtAn = value; }
    }

    public double Temperatur
    {
        get { return temperatur; }
        set { temperatur = value; }
    }
}