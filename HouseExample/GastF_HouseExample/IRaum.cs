namespace HouseExample.Interfaces;

public interface IRaum
{
    bool LightOn { get; set; }

    double Temperature { get; set; }

    bool BlindDown { get; set; }
}

public interface IRaumJalousie : IRaum
{
}

public interface IRaumNass : IRaum
{
    double WaterConsumption { get; }
}
