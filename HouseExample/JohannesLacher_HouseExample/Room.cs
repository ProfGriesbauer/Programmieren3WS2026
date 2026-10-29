public class Room : IRoom
{
    public Room(string name, double areaInSquareMeters)
    {
        Name = name;
        AreaInSquareMeters = areaInSquareMeters;
    }

    public string Name { get; }

    public double AreaInSquareMeters { get; }

    public bool Light { get; set; }

    public double Temperature { get; set; } = 20;
}
