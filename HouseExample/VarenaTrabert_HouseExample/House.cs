public class House
{
    public string Street { get; set; }
    public int NumberOfFloors { get; set; }
    public List<Room> Rooms { get; } = [];
    public int NumberOfRooms => Rooms.Count;
    public Garden? Garden { get; set; }
    public Garage? Garage { get; set; }
    public Attic? Attic { get; set; }
    public Stable? Stable { get; set; }
    public FlowerGarden? FlowerGarden { get; set; }
    public HerbBed? HerbBed { get; set; }
    public Dog? Dog { get; set; }
    public Pasture? Pasture { get; set; }

    public House(string street, int numberOfFloors)
    {
        Street = street;
        NumberOfFloors = numberOfFloors;
    }

    public void AddRoom(Room room)
    {
        Rooms.Add(room);
    }

    public void PrintInfo()
    {
        Console.WriteLine($"Haus in {Street}: {NumberOfFloors} Etagen, {NumberOfRooms} Räume.");

        foreach (Room room in Rooms)
        {
            string adjacentInfo = room.AdjacentTo is null ? "" : $", neben {room.AdjacentTo.Name}";
            Console.WriteLine($"- {room.Name} (Etage {room.Floor}{adjacentInfo})");
        }

        if (Garden is not null)
        {
            Console.WriteLine($"Garten: {Garden.AreaInSquareMeters} m²");

            if (Garden.Pool is not null)
            {
                Console.WriteLine($"- Pool: {Garden.Pool.AreaInSquareMeters} m²");
            }

            foreach (Tree tree in Garden.Trees)
            {
                Console.WriteLine($"- Baum: {tree.Name}");
            }
        }

        if (Garage is not null)
        {
            Console.WriteLine($"Garage: {Garage.NumberOfParkingSpaces} Stellplätze");
        }

        if (Attic is not null)
        {
            Console.WriteLine($"Dachboden: {(Attic.IsFinished ? "ausgebaut" : "nicht ausgebaut")}");
        }

        if (Stable is not null)
        {
            Console.WriteLine($"Großer Stall: {Stable.AreaInSquareMeters} m²");

            foreach (AnimalGroup animalGroup in Stable.Animals)
            {
                Console.WriteLine($"- {animalGroup.Count} {animalGroup.Species}");
            }
        }

        if (FlowerGarden is not null)
        {
            Console.WriteLine($"Blumengarten: {FlowerGarden.Flowers.Count} Blumenarten ({string.Join(", ", FlowerGarden.Flowers)})");
        }

        if (HerbBed is not null)
        {
            Console.WriteLine($"Kräuterbeet: {string.Join(", ", HerbBed.Herbs)}");
        }

        if (Dog is not null)
        {
            Console.WriteLine($"Hund: {Dog.Name}");
        }

        if (Pasture is not null)
        {
            Console.WriteLine($"Koppel: {Pasture.Animals.Count} {Pasture.Animals.Species}");
            Console.WriteLine($"- Unterstand für {Pasture.Shelter.Capacity} Tiere");
        }
    }
}
