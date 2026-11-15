using System.Collections.Generic;

namespace OOPGames
{
    public sealed class G1C_PlacedFurniture
    {
        public string ItemId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
    }

    public sealed class G1C_FurnitureLocation
    {
        public int X { get; }
        public int Y { get; }

        public G1C_FurnitureLocation(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    // =====================================================
    // Klasse: G1C_HomeWorld
    // Entwickler: Denis Mock
    // Beschreibung: Startszene und Einrichtung des Spielerhauses.
    // =====================================================
    public class G1C_HomeWorld : IG1C_WorldScene
    {
        private readonly List<G1C_WorldObject> _objects = new List<G1C_WorldObject>();
        private readonly List<G1C_PlacedFurniture> _placedFurniture = new List<G1C_PlacedFurniture>();
        private readonly List<G1C_WorldObject> _placedObjects = new List<G1C_WorldObject>();

        public G1C_HomeMap Map { get; } = new G1C_HomeMap();
        public IReadOnlyList<G1C_WorldObject> Objects { get { return _objects; } }
        public IReadOnlyList<G1C_PlacedFurniture> PlacedFurniture { get { return _placedFurniture; } }
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
            _objects.Add(new G1C_WorldObject("Einrichtungsplan", "obj_home_table", 8, 3, 2, 1, true, G1C_InteractionType.Furnish));
            // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Mutter wohnt im Spielerhaus (Gesprächsprofil:
            // G1C_FamilyNpcRoster). Sie steht fest neben dem Tisch und verlässt das Haus nicht.
            _objects.Add(new G1C_Npc(G1C_FamilyNpcRoster.MotherName, G1C_FamilyNpcRoster.MotherSkin, 9, 5,
                                     G1C_Direction.Left, G1C_InteractionType.Talk));
            // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – großes Poster an der Wand (untersuchbar)
            _objects.Add(new G1C_WorldObject("Poster", "obj_poster_ad", 9, 0, 1, 1, true, G1C_InteractionType.Inspect,
                "Ein großes, leicht vergilbtes Poster:\n\"Hier könnte Ihre Werbung stehen.\"",
                "Darunter hat jemand mit Kuli geschrieben:\n\"Steht sie aber nicht.\" – Das war bestimmt Oma."));
        }

        public List<G1C_FurnitureLocation> AvailableFurnitureLocations(G1C_Item item, G1C_Player player)
        {
            List<G1C_FurnitureLocation> locations = new List<G1C_FurnitureLocation>();
            if (item == null || item.Category != G1C_ItemCategory.Moebel) return locations;
            for (int y = 1; y < Map.Height - 1; y++)
                for (int x = 1; x < Map.Width - 1; x++)
                    if (CanPlaceFurniture(item, x, y, player)) locations.Add(new G1C_FurnitureLocation(x, y));
            return locations;
        }

        public bool PlaceFurniture(G1C_Item item, int x, int y, G1C_Player player)
        {
            if (!CanPlaceFurniture(item, x, y, player)) return false;
            G1C_WorldObject placed = new G1C_WorldObject(item.Name, item.FurnitureSpriteId, x, y,
                item.FurnitureWidth, item.FurnitureHeight, true, G1C_InteractionType.None);
            _objects.Add(placed);
            _placedObjects.Add(placed);
            _placedFurniture.Add(new G1C_PlacedFurniture { ItemId = item.Id, X = x, Y = y });
            return true;
        }

        public void RestoreFurniture(IEnumerable<G1C_PlacedFurniture> furniture)
        {
            foreach (G1C_WorldObject placed in _placedObjects) _objects.Remove(placed);
            _placedObjects.Clear();
            _placedFurniture.Clear();
            if (furniture == null) return;
            foreach (G1C_PlacedFurniture saved in furniture)
            {
                G1C_Item item = saved == null ? null : G1C_ItemCatalog.Get(saved.ItemId);
                if (item != null) PlaceFurniture(item, saved.X, saved.Y, null);
            }
        }

        private bool CanPlaceFurniture(G1C_Item item, int x, int y, G1C_Player player)
        {
            if (item == null || item.Category != G1C_ItemCategory.Moebel ||
                string.IsNullOrEmpty(item.FurnitureSpriteId) || item.FurnitureWidth <= 0 ||
                item.FurnitureHeight <= 0 || x < 1 || y < 1 ||
                x + item.FurnitureWidth >= Map.Width - 1 || y + item.FurnitureHeight >= Map.Height - 1)
                return false;
            for (int tileY = y; tileY < y + item.FurnitureHeight; tileY++)
            {
                for (int tileX = x; tileX < x + item.FurnitureWidth; tileX++)
                {
                    if (!Map.IsWalkable(tileX, tileY) || ObjectAt(tileX, tileY) != null ||
                        (player != null && player.TileX == tileX && player.TileY == tileY)) return false;
                }
            }
            return true;
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
