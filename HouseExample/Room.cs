public class Room
{
    public string Name { get; set; }
    public int Floor { get; set; }

    public Room(string name, int floor)
    {
        Name = name;
        Floor = floor;
    }
}
