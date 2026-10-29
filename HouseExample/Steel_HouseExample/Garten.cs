public class Garten : IWasserverbrauch
{
    public List<Obstbaum> Obstbäume { get; } = new();
    public List<Zierpflanze> Zierpflanzen { get; } = new();
    public Kräutergarten Kräutergarten { get; } = new();
    public decimal Wasserverbrauch =>
        Obstbäume.Sum(obstbaum => obstbaum.Wasserverbrauch)
        + Zierpflanzen.Sum(zierpflanze => zierpflanze.Wasserverbrauch)
        + Kräutergarten.Wasserverbrauch;

    public void AddObstbaum(Obstbaum obstbaum)
    {
        Obstbäume.Add(obstbaum);
    }

    public void AddZierpflanze(Zierpflanze zierpflanze)
    {
        Zierpflanzen.Add(zierpflanze);
    }

    public void BewässereAlle(decimal mengeProPflanze)
    {
        foreach (var pflanze in Obstbäume.Cast<Pflanze>()
            .Concat(Zierpflanzen)
            .Concat(Kräutergarten.Kräuter))
        {
            pflanze.Bewässere(mengeProPflanze);
        }
    }
}
