public class Garage : IWasserverbrauch
{
    public int Stellplätze { get; } = 3;
    public bool HatWasseranschluss { get; } = true;
    public decimal Wasserverbrauch { get; private set; }

    public void VerbraucheWasser(decimal menge)
    {
        if (menge <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(menge), "Die Wassermenge muss größer als 0 sein.");
        }

        Wasserverbrauch += menge;
    }
}
