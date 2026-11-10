public class RaumMitWasser : Raum, IRaumNass
{
    public decimal Wasserverbrauch { get; }

    public RaumMitWasser(bool lichtAn, decimal temperatur, decimal wasserverbrauch)
        : base(lichtAn, temperatur)
    {
        Wasserverbrauch = wasserverbrauch;
    }
}
