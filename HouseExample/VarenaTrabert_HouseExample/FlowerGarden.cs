public class FlowerGarden
{
    public List<string> Flowers { get; } = [];

    public void AddFlower(string flower)
    {
        Flowers.Add(flower);
    }
}
