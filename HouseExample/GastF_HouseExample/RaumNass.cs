public class RaumNass : Raum, HouseExample.Interfaces.IRaumNass
{
    public double WaterConsumption { get; }

    public RaumNass(double waterConsumption = 0)
    {
        WaterConsumption = waterConsumption;

        if (waterConsumption < 0)
        {
            Console.WriteLine("Fehlermeldung: Der Wasserverbrauch darf nicht negativ sein.");
            WaterConsumption = 0;
            return;
        }
    }
}
