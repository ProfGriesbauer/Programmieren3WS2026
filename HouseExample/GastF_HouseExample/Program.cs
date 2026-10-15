var haus = new ExampleHouse(
    electricityConsumption: 16,
    waterConsumption: 0.2,
    heatingPower: 8,
    roomConfigurations:
    [
        new RoomConfiguration(IsWet: false, HasBlinds: true),
        new RoomConfiguration(IsWet: true, HasBlinds: false, WaterConsumptionPercentage: 60),
        new RoomConfiguration(IsWet: true, HasBlinds: true, WaterConsumptionPercentage: 30),
        new RoomConfiguration(IsWet: false, HasBlinds: false),
    ]);

haus.Rooms[0].LightOn = true;
haus.Rooms[0].Temperature = 21;
haus.Rooms[1].Temperature = 20;
haus.Rooms[2].LightOn = true;
haus.Rooms[2].Temperature = 19;
haus.Rooms[3].Temperature = 18;
haus.Rooms[1].BlindDown = true;
haus.Rooms[2].BlindDown = true;

haus.MainPowerOn = true;
haus.HeatingOn = true;

Console.WriteLine($"Stromverbrauch: {haus.ElectricityConsumption} A");
Console.WriteLine($"Wasserverbrauch: {haus.WaterConsumption} m³/h");
if (haus.RestWaterConsumption > 0)
{
    Console.WriteLine($"Wasserleck: {haus.RestWaterConsumption:F2} m³/h");
}
Console.WriteLine($"Heizungsleistung: {haus.HeatingPower} kW");
Console.WriteLine($"Anzahl Räume: {haus.Rooms.Count}");

for (var roomNumber = 0; roomNumber < haus.Rooms.Count; roomNumber++)
{
    var room = haus.Rooms[roomNumber];
    var lightState = room.LightOn ? "An" : "Aus";
    var waterConsumption = room is HouseExample.Interfaces.IRaumNass wetRoom
        ? $", Wasserverbrauch {wetRoom.WaterConsumption:F2} m³/h"
        : string.Empty;
    var blindState = room is HouseExample.Interfaces.IRaumJalousie roomWithBlinds
        ? $", Jalousie {(roomWithBlinds.BlindDown ? "Unten" : "Oben")}"
        : string.Empty;

    Console.WriteLine(
        $"Raum {roomNumber + 1}: Licht {lightState}, " +
        $"Temperatur {room.Temperature} °C" +
        $"{blindState}" +
        $"{waterConsumption}");
}