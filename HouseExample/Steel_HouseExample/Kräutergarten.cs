public class Kräutergarten : IWasserverbrauch
{
    public List<Kraut> Kräuter { get; } = new();
    public decimal Wasserverbrauch => Kräuter.Sum(kraut => kraut.Wasserverbrauch);

    public void AddKraut(Kraut kraut)
    {
        Kräuter.Add(kraut);
    }

    public void BewässereAlle(decimal mengeProKraut)
    {
        foreach (var kraut in Kräuter)
        {
            kraut.Bewässere(mengeProKraut);
        }
    }
}
