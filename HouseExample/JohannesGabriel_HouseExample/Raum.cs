public class Raum : IRaum
{
    private bool lichtAn;
    private double temperatur = 20;

    public bool LichtAn
    {
        get => lichtAn;
        set => lichtAn = value;
    }

    public double Temperatur
    {
        get => temperatur;
        set => temperatur = value;
    }
}
