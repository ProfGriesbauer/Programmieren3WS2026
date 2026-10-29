public static class AusgabeHelper
{
    public static void Ausgabe(ExampleHouse haus)
    {
        if (haus.HasErrors)
        {
            return;
        }
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
    }
}
