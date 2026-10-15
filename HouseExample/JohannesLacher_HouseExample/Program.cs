var rooms = new List<IRoom>
{
    new Room("Wohnzimmer", 25.5),
    new Room("Küche", 12.0),
    new Room("Schlafzimmer", 16.0),
    new Room("Badezimmer", 8.5)
};

rooms[0].Light = true;
rooms[0].Temperature = 21.5;

var house = new House("Familienhaus", 2, rooms);
Console.WriteLine(house.Describe());
Console.WriteLine(
    $"{rooms[0].Name}: Licht an = {rooms[0].Light}, Temperatur = {rooms[0].Temperature} °C");

Console.WriteLine($"Wasserverbrauch in 5 Stunden: {house.CalculateWaterConsumption(5)} m³");

house.HeatingState = true;
Console.WriteLine($"Heizung: {house.HeatingState}, Leistung: {house.HeatingPower} kW");

house.MainPowerState = true;
Console.WriteLine($"Hauptstrom: {house.MainPowerState}, Stromverbrauch: {house.ElectricityConsumption} A");

house.HeatingState = true;
Console.WriteLine($"Heizung: {house.HeatingState}, Leistung: {house.HeatingPower} kW");