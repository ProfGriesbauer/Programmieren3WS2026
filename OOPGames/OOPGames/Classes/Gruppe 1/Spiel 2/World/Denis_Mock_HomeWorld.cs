using System.Collections.Generic;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_HomeWorld
    // Entwickler: Denis Mock
    // Beschreibung: Startszene und Einrichtung des Spielerhauses.
    // =====================================================
    public class G1C_HomeWorld : IG1C_WorldScene
    {
        private readonly List<G1C_WorldObject> _objects = new List<G1C_WorldObject>();

        public G1C_HomeMap Map { get; } = new G1C_HomeMap();
        public IReadOnlyList<G1C_WorldObject> Objects { get { return _objects; } }
        IG1C_WorldMap IG1C_WorldScene.Grid { get { return Map; } }

        public int PlayerStartX { get { return 5; } }
        public int PlayerStartY { get { return 8; } }
        public int TileSize { get { return 24; } }

        // Entwickler: Denis Mock
        public G1C_HomeWorld()
        {
            BuildHome();
        }

        // Entwickler: Denis Mock
        private void BuildHome()
        {
            // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – Bett, Kleiderschrank und Spiegel sind benutzbar
            _objects.Add(new G1C_WorldObject("Bett", "obj_home_bed", 2, 2, 2, 1, true, G1C_InteractionType.Bed));
            _objects.Add(new G1C_WorldObject("Kleiderschrank", "obj_wardrobe", 5, 1, 1, 1, true, G1C_InteractionType.Wardrobe));
            _objects.Add(new G1C_WorldObject("Spiegel", "obj_mirror", 7, 1, 1, 1, true, G1C_InteractionType.Wardrobe));
            _objects.Add(new G1C_WorldObject("Pflanze", "obj_plant", 10, 1, 1, 1, true, G1C_InteractionType.None));
            _objects.Add(new G1C_WorldObject("Tisch", "obj_home_table", 8, 3, 2, 1, true, G1C_InteractionType.None));
        }

        // Entwickler: Denis Mock
        public G1C_WorldObject ObjectAt(int x, int y)
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                if (_objects[i].Occupies(x, y)) return _objects[i];
            }
            return null;
        }
    }
}
