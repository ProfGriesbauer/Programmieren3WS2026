public class Koppel
{
    public List<Tier> Tiere { get; } = new();

    public void AddTier(Tier tier)
    {
        ArgumentNullException.ThrowIfNull(tier);
        Tiere.Add(tier);
    }
}
