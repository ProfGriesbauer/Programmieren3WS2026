public interface IRoom
{
    string Name { get; }

    double AreaInSquareMeters { get; }

    bool Light { get; set; }

    double Temperature { get; set; }
}
