namespace OOPGames
{
    // Entwickler: Denis Mock
    // Gemeinsame Verträge für die begehbaren Szenen der Außenwelt, des Hauses und des Casinos.
    public enum G1C_WorldLocation
    {
        Home,
        Exterior,
        Casino,
        Hospital,
        Market,
        // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – weitere Geschäfte
        GasStation,
        ClothingStore
    }

    public interface IG1C_WorldMap
    {
        int Width { get; }
        int Height { get; }
        // Entwickler: Denis Mock
        bool IsWalkable(int x, int y);
        // Entwickler: Denis Mock
        G1C_WorldLocation? GetTransitionAt(int x, int y);
        // Entwickler: Denis Mock
        string GetTileSpriteId(int x, int y);
    }

    public interface IG1C_WorldScene
    {
        IG1C_WorldMap Grid { get; }
        System.Collections.Generic.IReadOnlyList<G1C_WorldObject> Objects { get; }
        int PlayerStartX { get; }
        int PlayerStartY { get; }
        int TileSize { get; }
        // Entwickler: Denis Mock
        G1C_WorldObject ObjectAt(int x, int y);
    }
}
