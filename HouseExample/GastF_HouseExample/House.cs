public abstract class House
{
    private readonly List<HouseExample.Interfaces.IRaum> rooms = [];
    private double electricityConsumptionWhenOn;
    private double waterConsumptionWhenOn;
    private double heatingPowerWhenOn;
    private double restWaterConsumption;
    private bool heatingOn;
    private bool mainPowerOn;
    private bool hasConfigurationError;

    public House(
        double electricityConsumption,
        double waterConsumption,
        double heatingPower,
        IReadOnlyList<RoomConfiguration> roomConfigurations)
    {
        electricityConsumptionWhenOn = electricityConsumption;
        waterConsumptionWhenOn = waterConsumption;
        heatingPowerWhenOn = heatingPower;

        if (electricityConsumption < 0)
        {
            Console.WriteLine("Fehlermeldung: Der Stromverbrauch darf nicht negativ sein.");
            electricityConsumptionWhenOn = 0;
            hasConfigurationError = true;
            return;
        }

        if (waterConsumption < 0)
        {
            Console.WriteLine("Fehlermeldung: Der Wasserverbrauch darf nicht negativ sein.");
            hasConfigurationError = true;
            return;
        }

        if (heatingPower < 0)
        {
            Console.WriteLine("Fehlermeldung: Die Heizleistung darf nicht negativ sein.");
            hasConfigurationError = true;
            return;
        }

        if (roomConfigurations is null)
        {
            Console.WriteLine("Fehlermeldung: Es wurden keine Raumkonfigurationen angegeben.");
            hasConfigurationError = true;
            return;
        }

        if (roomConfigurations.Any(configuration =>
                configuration.WaterConsumptionPercentage < 0 ||
                configuration.WaterConsumptionPercentage > 100))
        {
            Console.WriteLine(
                "Fehlermeldung: Der Wasserverbrauchsanteil muss zwischen 0 und 100 Prozent liegen.");
            hasConfigurationError = true;
            return;
        }

        electricityConsumptionWhenOn = electricityConsumption;
        waterConsumptionWhenOn = waterConsumption;
        heatingPowerWhenOn = heatingPower;
        var assignedWaterPercentage = roomConfigurations
            .Where(configuration => configuration.IsWet)
            .Sum(configuration => configuration.WaterConsumptionPercentage);

        if (assignedWaterPercentage > 100)
        {
            Console.WriteLine(
                "Fehlermeldung: Der gesamte Wasserverbrauch der Nassräume darf 100 Prozent nicht überschreiten.");
            hasConfigurationError = true;
            return;
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

    public bool HasErrors =>
        hasConfigurationError || rooms.Any(room => ((Raum)room).HasError);

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
            Console.WriteLine("Fehlermeldung: Die Dauer darf nicht negativ sein.");
            return 0;
        }

        return waterConsumptionWhenOn * duration.TotalHours;
    }

    public abstract void Shutdown();
}
