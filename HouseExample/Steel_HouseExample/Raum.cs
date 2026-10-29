public class Raum : IRaum
{
    private bool _lichtAn;
    private decimal _temperatur;

    public bool LichtAn
    {
        get => _lichtAn;
        set => _lichtAn = value;
    }

    public decimal Temperatur
    {
        get => _temperatur;
        set => _temperatur = value;
    }

    public Raum(bool lichtAn, decimal temperatur)
    {
        _lichtAn = lichtAn;
        _temperatur = temperatur;
    }
}
