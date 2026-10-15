public class HerbBed
{
    public List<string> Herbs { get; } = [];

    public void AddHerb(string herb)
    {
        Herbs.Add(herb);
    }
}
