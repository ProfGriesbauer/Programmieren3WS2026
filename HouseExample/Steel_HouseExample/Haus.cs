public abstract class Haus
{
    public List<IRaum> Räume { get; } = new();
    public List<Pool> Pools { get; } = new();
    public List<Garage> Garagen { get; } = new();
    public List<Garten> Gärten { get; } = new();
    public List<Scheune> Scheunen { get; } = new();
    public decimal WasserverbrauchGesamt =>
        Räume.OfType<IWasserverbrauch>().Sum(verbrauch => verbrauch.Wasserverbrauch)
        + Pools.Sum(pool => pool.Wasserverbrauch)
        + Garagen.Sum(garage => garage.Wasserverbrauch)
        + Gärten.Sum(garten => garten.Wasserverbrauch)
        + Scheunen.Sum(scheune => scheune.Wasserverbrauch);
    public decimal FutterverbrauchGesamt => Scheunen.Sum(scheune => scheune.Futterverbrauch);

    public void AddRaum(IRaum raum)
    {
        Räume.Add(raum);
    }

    public void AddPool(Pool pool)
    {
        Pools.Add(pool);
    }

    public void AddGarage(Garage garage)
    {
        Garagen.Add(garage);
    }

    public void AddGarten(Garten garten)
    {
        Gärten.Add(garten);
    }

    public void AddScheune(Scheune scheune)
    {
        Scheunen.Add(scheune);
    }

    public abstract void AusschaltenOderHerunterfahren();
}
