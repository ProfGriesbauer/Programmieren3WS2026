public interface IRaum : ISerialisierbar
{
    string Name { get; set; }
    bool LichtAn { get; set; }
    double Temperatur { get; set; } // eingestellte Temperatur [°C]
}
