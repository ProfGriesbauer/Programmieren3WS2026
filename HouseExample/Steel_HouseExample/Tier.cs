public abstract class Tier : IWasserverbrauch, IFutterverbrauch
{
    public decimal Wasserverbrauch { get; private set; }
    public decimal Futterverbrauch { get; private set; }

    public void VerbraucheWasser(decimal menge)
    {
        if (menge <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(menge), "Die Wassermenge muss größer als 0 sein.");
        }

        Wasserverbrauch += menge;
    }

    public void VerbraucheFutter(decimal menge)
    {
        if (menge <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(menge), "Die Futtermenge muss größer als 0 sein.");
        }

        Futterverbrauch += menge;
    }
}
