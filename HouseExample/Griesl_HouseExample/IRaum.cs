public interface IRaum
{
    string Name { get; set; }
    bool LichtAn { get; set; }
    double Temperatur { get; set; } // eingestellte Temperatur [°C]
}
