public abstract class Pflanze : IWasserverbrauch
{
    public string Name { get; set; }
    public decimal Wasserverbrauch { get; private set; }

    protected Pflanze(string name)
    {
        Name = name;
    }

    public void Bewässere(decimal menge)
    {
        if (menge <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(menge), "Die Wassermenge muss größer als 0 sein.");
        }

        Wasserverbrauch += menge;
    }
}
