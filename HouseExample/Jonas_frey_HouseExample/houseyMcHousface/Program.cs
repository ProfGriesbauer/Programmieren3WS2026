var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.Run();

class houseyMcHousface
{
    public string Address { get; set; }
    public int NumberOfRooms { get; set; }
    public bool HasGarage { get; set; }

    public houseyMcHousface(string address, int numberOfRooms, bool hasGarage)
    {
        Address = address;
        NumberOfRooms = numberOfRooms;
        HasGarage = hasGarage;
    }

    public void DisplayInfo()
    {
        Console.WriteLine($"Address: {Address}");
        Console.WriteLine($"Number of Rooms: {NumberOfRooms}");
        Console.WriteLine($"Has Garage: {HasGarage}");
    }

}