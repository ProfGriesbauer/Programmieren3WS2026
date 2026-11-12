using System.Collections.Generic;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_ExteriorWorld
    // Entwickler: Denis Mock
    // Beschreibung: Gebäude, Dekoration und Startposition der begehbaren Stadt.
    // =====================================================
    public class G1C_ExteriorWorld : IG1C_WorldScene
    {
        private readonly List<G1C_WorldObject> _objects = new List<G1C_WorldObject>();

        public G1C_ExteriorMap Map { get; } = new G1C_ExteriorMap();
        public IReadOnlyList<G1C_WorldObject> Objects { get { return _objects; } }
        IG1C_WorldMap IG1C_WorldScene.Grid { get { return Map; } }

        public int PlayerStartX { get { return 8; } }
        public int PlayerStartY { get { return 8; } }
        public int TileSize { get { return 16; } }

        // Entwickler: Denis Mock
        public G1C_ExteriorWorld()
        {
            BuildCity();
        }

        // Entwickler: Denis Mock
        // Gebäude sind als solide Weltobjekte unabhängig von ihrer Grafik kollidierbar.
        private void BuildCity()
        {
            _objects.Add(new G1C_WorldObject("Zuhause", "building_home", 2, 12, 4, 3, true, G1C_InteractionType.None));
            _objects.Add(new G1C_WorldObject("Krankenhaus", "building_hospital", 2, 2, 4, 3, true, G1C_InteractionType.None));
            _objects.Add(new G1C_WorldObject("Supermarkt", "building_market", 12, 2, 4, 3, true, G1C_InteractionType.None));
            _objects.Add(new G1C_WorldObject("Casino", "building_casino", 12, 12, 4, 3, true, G1C_InteractionType.None));

            _objects.Add(new G1C_WorldObject("Baum", "obj_city_tree", 1, 5, 1, 1, true, G1C_InteractionType.None));
            _objects.Add(new G1C_WorldObject("Baum", "obj_city_tree", 16, 5, 1, 1, true, G1C_InteractionType.None));
            _objects.Add(new G1C_WorldObject("Baum", "obj_city_tree", 1, 13, 1, 1, true, G1C_InteractionType.None));
            _objects.Add(new G1C_WorldObject("Baum", "obj_city_tree", 16, 13, 1, 1, true, G1C_InteractionType.None));
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
