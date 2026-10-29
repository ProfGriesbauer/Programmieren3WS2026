namespace PatrickHouseExample;

public interface IRaum
{
    string Name { get; }
    bool LichtAn { get; set; }
    decimal Temperatur { get; set; }
}
