public sealed class ExampleHouse : House
{
    public ExampleHouse(
        double electricityConsumption,
        double waterConsumption,
        double heatingPower,
        IReadOnlyList<RoomConfiguration> roomConfigurations)
        : base(electricityConsumption, waterConsumption, heatingPower, roomConfigurations)
    {
    }

    public override void Shutdown()
    {
        MainPowerOn = false;

        foreach (var room in Rooms)
        {
            room.LightOn = false;

            if (room is HouseExample.Interfaces.IRaumJalousie roomWithBlinds)
            {
                roomWithBlinds.BlindDown = true;
            }
        }
    }
}
