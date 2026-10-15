try
{
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
    haus.Rooms[2].BlindDown = true;

    haus.MainPowerOn = true;
    haus.HeatingOn = true;

    AusgabeHelper.Ausgabe(haus);
}
catch (Exception exception)
{
    Console.WriteLine($"Fehlermeldung: {exception.Message}");
}