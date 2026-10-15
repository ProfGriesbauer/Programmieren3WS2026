public class AnimalGroup
{
    public string Species { get; set; }
    public int Count { get; set; }

    public AnimalGroup(string species, int count)
    {
        Species = species;
        Count = count;
    }
}
