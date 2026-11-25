namespace OOPGames
{
    /// <summary>
    /// Beschreibt ein Gebäude mit Typ, Anzeigename und Rasterposition.
    /// </summary>
    public sealed class FarmBuilding
    {
        public FarmBuilding(string kind, string name, int row, int column)
        {
            Kind = kind;
            Name = name;
            Row = row;
            Column = column;
        }

        public string Kind { get; private set; }
        public string Name { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }

        // Die Feldlogik prüft zuerst den Zielplatz und ändert dann nur die Rasterposition.
        internal void MoveTo(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
}
