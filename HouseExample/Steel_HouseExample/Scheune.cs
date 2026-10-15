public class Scheune : IWasserverbrauch, IFutterverbrauch
{
    public List<Schwarznasenschaf> Schwarznasenschafe { get; } = new();
    public List<Pony> Ponys { get; } = new();
    public List<Hochlandrind> Hochlandrinder { get; } = new();
    public Koppel Koppel { get; } = new();
    public decimal Wasserverbrauch =>
        Koppel.Tiere.Sum(tier => tier.Wasserverbrauch);
    public decimal Futterverbrauch =>
        Koppel.Tiere.Sum(tier => tier.Futterverbrauch);

    public Scheune()
    {
        for (var i = 0; i < 5; i++)
        {
            var schaf = new Schwarznasenschaf();
            Schwarznasenschafe.Add(schaf);
            Koppel.AddTier(schaf);
        }

        for (var i = 0; i < 3; i++)
        {
            var pony = new Pony();
            Ponys.Add(pony);
            Koppel.AddTier(pony);
        }

        for (var i = 0; i < 3; i++)
        {
            var rind = new Hochlandrind();
            Hochlandrinder.Add(rind);
            Koppel.AddTier(rind);
        }
    }

    public void TränkeAlle(decimal mengeProTier)
    {
        foreach (var tier in Koppel.Tiere)
        {
            tier.VerbraucheWasser(mengeProTier);
        }
    }

    public void FüttereAlle(decimal mengeProTier)
    {
        foreach (var tier in Koppel.Tiere)
        {
            tier.VerbraucheFutter(mengeProTier);
        }
    }
}
