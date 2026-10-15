public class Raum : IRaum // Raum erfüllt den Vetrag aus dem Interface.
{
    private bool _lichtAn;
    private double _temperatur;

    public bool LichtAn
    {
        get => _lichtAn; // get liefert den aktuellen Wert der Variable zurück, set setzt den Wert der Variable.
        set => _lichtAn = value;
    }

    public double Temperatur
    {
        get => _temperatur;
        set => _temperatur = value;
    }
}