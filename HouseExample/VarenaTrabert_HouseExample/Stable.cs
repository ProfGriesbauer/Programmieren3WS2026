public class Stable
{
    public double AreaInSquareMeters { get; set; }
    public List<AnimalGroup> Animals { get; } = [];

    public Stable(double areaInSquareMeters)
    {
        AreaInSquareMeters = areaInSquareMeters;
    }

    public void AddAnimals(AnimalGroup animalGroup)
    {
        Animals.Add(animalGroup);
    }
}
