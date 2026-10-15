public class House
{
    private const double DefaultElectricityConsumption = 1.5;
    private const double DefaultWaterConsumption = 0.2;
    private const double DefaultHeatingPower = 8.0;

    private double electricityConsumption = DefaultElectricityConsumption;
    private double waterConsumption = DefaultWaterConsumption;
    private double heatingPower = DefaultHeatingPower;
    private bool heatingState;
    private bool mainPowerState = true;

    public House(string name, int floors, List<IRoom> rooms)
    {
        Name = name;
        Floors = floors;
        Rooms = rooms;
    }

    public string Name { get; }

    public int Floors { get; }

    public List<IRoom> Rooms { get; }

    public double ElectricityConsumption => electricityConsumption;

    public double WaterConsumption => waterConsumption;

    public double HeatingPower => heatingPower;

    public bool HeatingState
    {
        get => heatingState;
        set
        {
            if (value && !MainPowerState)
            {
                throw new InvalidOperationException(
                    "Die Heizung kann nicht eingeschaltet werden, wenn der Hauptstrom ausgeschaltet ist.");
            }

            heatingState = value;
            if (heatingState)
            {
                heatingPower = DefaultHeatingPower;
            }
            else
            {
                heatingPower = 0;
            }
        }
    }

    public bool MainPowerState
    {
        get => mainPowerState;
        set
        {
            mainPowerState = value;
            if (!mainPowerState)
            {
                electricityConsumption = 0;
                HeatingState = false;
            }
            else
            {
                electricityConsumption = DefaultElectricityConsumption;
            }
        }
    }

    public double CalculateWaterConsumption(double hours)
    {
        if (hours < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hours), "Die Zeit darf nicht negativ sein.");
        }

        return WaterConsumption * hours;
    }

    public string Describe()
    {
        var roomNames = string.Join(", ", Rooms.Select(room => room.Name));
        return $"{Name} hat {Floors} Stockwerke und folgende Zimmer: {roomNames}.";
    }
}
