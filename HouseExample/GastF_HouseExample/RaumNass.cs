public class RaumNass : Raum, HouseExample.Interfaces.IRaumNass
{
    public double WaterConsumption { get; }

    public RaumNass(double waterConsumption = 0)
    {
        if (waterConsumption < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(waterConsumption));
        }

        WaterConsumption = waterConsumption;
    }
}
