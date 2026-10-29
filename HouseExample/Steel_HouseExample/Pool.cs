public class Pool : IWasserverbrauch
{
    public decimal FlächeQuadratmeter { get; } = 30m;
    public decimal TiefeMeter { get; }
    public decimal FassungsvermögenKubikmeter => FlächeQuadratmeter * TiefeMeter;
    public decimal FassungsvermögenLiter => FassungsvermögenKubikmeter * 1000m;
    public decimal Wasserverbrauch { get; private set; }

    public Pool(decimal tiefeMeter = 1.5m)
    {
        if (tiefeMeter < 1.5m || tiefeMeter > 2m)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tiefeMeter),
                "Die Pooltiefe muss zwischen 1,5 m und 2 m liegen.");
        }

        TiefeMeter = tiefeMeter;
    }

    public void VerbraucheWasser(decimal menge)
    {
        if (menge <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(menge), "Die Wassermenge muss größer als 0 sein.");
        }

        Wasserverbrauch += menge;
    }
}
