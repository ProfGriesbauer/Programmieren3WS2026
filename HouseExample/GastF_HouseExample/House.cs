public abstract class House
{
    private readonly List<HouseExample.Interfaces.IRaum> rooms;
    private readonly double electricityConsumptionWhenOn;
    private readonly double waterConsumptionWhenOn;
    private readonly double heatingPowerWhenOn;
    private readonly double restWaterConsumption;
    private bool heatingOn;
    private bool mainPowerOn;

    public House(
        double electricityConsumption,
        double waterConsumption,
        double heatingPower,
        IReadOnlyList<RoomConfiguration> roomConfigurations)
    {
        if (electricityConsumption < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(electricityConsumption));
        }

        if (waterConsumption < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(waterConsumption));
        }

        if (heatingPower < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heatingPower));
        }

        if (roomConfigurations is null)
        {
            throw new ArgumentNullException(nameof(roomConfigurations));
        }

        if (roomConfigurations.Any(configuration =>
                configuration.WaterConsumptionPercentage < 0 ||
                configuration.WaterConsumptionPercentage > 100))
        {
            throw new ArgumentOutOfRangeException(
                nameof(roomConfigurations),
                "Der Wasserverbrauchsanteil muss zwischen 0 und 100 Prozent liegen.");
        }

        electricityConsumptionWhenOn = electricityConsumption;
        waterConsumptionWhenOn = waterConsumption;
        heatingPowerWhenOn = heatingPower;
        rooms = [];
        var assignedWaterPercentage = roomConfigurations
            .Where(configuration => configuration.IsWet)
            .Sum(configuration => configuration.WaterConsumptionPercentage);

        if (assignedWaterPercentage > 100)
        {
            throw new ArgumentException(
                "Der gesamte Wasserverbrauch der Nassräume darf 100 Prozent nicht überschreiten.");
        }

        restWaterConsumption = waterConsumption *
            Math.Max(0, 100 - assignedWaterPercentage) / 100;

        for (var roomNumber = 0; roomNumber < roomConfigurations.Count; roomNumber++)
        {
            var roomConfiguration = roomConfigurations[roomNumber];
            var roomWaterConsumption = roomConfiguration.IsWet
                ? waterConsumption * roomConfiguration.WaterConsumptionPercentage / 100
                : 0;

            var room = CreateRoom(roomConfiguration, roomWaterConsumption);
            ((Raum)room).RoomNumber = roomNumber + 1;
            rooms.Add(room);
        }
    }

    private static HouseExample.Interfaces.IRaum CreateRoom(
        RoomConfiguration configuration,
        double waterConsumption)
    {
        return (configuration.IsWet, configuration.HasBlinds) switch
        {
            (true, true) => new RaumNassJalousie(waterConsumption),
            (true, false) => new RaumNass(waterConsumption),
            (false, true) => new RaumJalousie(),
            (false, false) => new RaumStandard(),
        };
    }

    public IReadOnlyList<HouseExample.Interfaces.IRaum> Rooms => rooms;

    public double ElectricityConsumption =>
        mainPowerOn ? electricityConsumptionWhenOn : 0;

    public double WaterConsumption => waterConsumptionWhenOn;

    public double RestWaterConsumption => restWaterConsumption;

    public double HeatingPower =>
        heatingOn ? heatingPowerWhenOn : 0;

    public bool HeatingOn
    {
        get => heatingOn;
        set => heatingOn = value && mainPowerOn;
    }

    public bool MainPowerOn
    {
        get => mainPowerOn;
        set
        {
            mainPowerOn = value;

            if (!mainPowerOn)
            {
                heatingOn = false;
            }
        }
    }

    public double CalculateWaterConsumption(TimeSpan duration)
    {
        if (duration < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        return waterConsumptionWhenOn * duration.TotalHours;
    }

    public abstract void Shutdown();
}
