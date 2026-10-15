public class Pasture
{
    public AnimalGroup Animals { get; set; }
    public AnimalShelter Shelter { get; set; }

    public Pasture(AnimalGroup animals, AnimalShelter shelter)
    {
        Animals = animals;
        Shelter = shelter;
    }
}
