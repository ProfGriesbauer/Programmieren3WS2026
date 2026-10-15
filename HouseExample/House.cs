public class House
{
    public string Street { get; set; }
    public int NumberOfFloors { get; set; }
    public List<Room> Rooms { get; } = [];
    public int NumberOfRooms => Rooms.Count;
    public Garden? Garden { get; set; }
    public Garage? Garage { get; set; }
    public Attic? Attic { get; set; }

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
            Console.WriteLine($"- {room.Name} (Etage {room.Floor})");
        }

        if (Garden is not null)
        {
            Console.WriteLine($"Garten: {Garden.AreaInSquareMeters} m²");
        }

        if (Garage is not null)
        {
            Console.WriteLine($"Garage: {Garage.NumberOfParkingSpaces} Stellplätze");
        }

        if (Attic is not null)
        {
            Console.WriteLine($"Dachboden: {(Attic.IsFinished ? "ausgebaut" : "nicht ausgebaut")}");
        }
    }
}
