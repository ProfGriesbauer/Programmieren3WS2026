public class Room
{
    public string Name { get; set; }
    public int Floor { get; set; }
    public Room? AdjacentTo { get; set; }

    public Room(string name, int floor, Room? adjacentTo = null)
    {
        Name = name;
        Floor = floor;
        AdjacentTo = adjacentTo;
    }
}
