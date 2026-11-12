using System.Collections.Generic;

namespace OOPGames
{
    // Entwickler: Denis Mock
    // Gebäude, Wege, dekorative Bäume, Tiere und Startposition der großen Stadt.
    public class G1C_ExteriorWorld : IG1C_WorldScene
    {
        private readonly List<G1C_WorldObject> _objects = new List<G1C_WorldObject>();
        private readonly List<G1C_Npc> _townNpcs = new List<G1C_Npc>();
        private readonly List<G1C_Animal> _animals = new List<G1C_Animal>();

        public G1C_ExteriorMap Map { get; } = new G1C_ExteriorMap();
        public IReadOnlyList<G1C_WorldObject> Objects { get { return _objects; } }
        public IReadOnlyList<G1C_Npc> TownNpcs { get { return _townNpcs; } }
        public IReadOnlyList<G1C_Animal> Animals { get { return _animals; } }
        IG1C_WorldMap IG1C_WorldScene.Grid { get { return Map; } }

        public int PlayerStartX { get { return Map.HomeDoorX; } }
        public int PlayerStartY { get { return Map.HomeDoorY + 1; } }
        // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – 16-Pixel-Kacheln exakt verdoppelt
        // (15 x 9 Kacheln sichtbar), damit Figur und Gebäude im richtigen Größenverhältnis stehen.
        public int TileSize { get { return 32; } }

        // Entwickler: Denis Mock
        public G1C_ExteriorWorld()
        {
            BuildCity();
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – Gebäude stehen in drei Reihen jeweils
        // oberhalb einer geschwungenen Straße; neu sind Tankstelle, Bekleidungsgeschäft, Laternen, Mülleimer.
        private void BuildCity()
        {
            int[] columns = { 5, 22, 30, 58, 66, 84 };

            AddBuilding("Bibliothek", "building_library", columns[0], 14);
            AddBuilding("Zuhause", "building_home", columns[1], 14, G1C_WorldLocation.Home);
            AddBuilding("Wohnhaus", "building_townhouse", columns[2], 14);
            AddBuilding("Café", "building_cafe", columns[3], 14);
            AddBuilding("Krankenhaus", "building_hospital", columns[4], 14, G1C_WorldLocation.Hospital);
            AddBuilding("Blumenladen", "building_flower_shop", columns[5], 14);

            AddBuilding("Feuerwache", "building_fire_station", columns[0], 32);
            AddBuilding("Bürgerhaus", "building_townhouse", columns[1], 32);
            AddBuilding("Bekleidungsgeschäft", "building_clothing", columns[2], 32, G1C_WorldLocation.ClothingStore);
            AddBuilding("Kino", "building_cinema", columns[3], 32);
            AddBuilding("Tankstelle", "building_gas_station", columns[4], 32, G1C_WorldLocation.GasStation);
            AddBuilding("Wohnhaus", "building_townhouse", columns[5], 32);

            AddBuilding("Hafenlager", "building_warehouse", columns[0], 50);
            AddBuilding("Supermarkt", "building_market", columns[1], 50, G1C_WorldLocation.Market);
            AddBuilding("Bäckerei", "building_cafe", columns[2], 50);
            AddBuilding("Post", "building_post", columns[3], 50);
            AddBuilding("Casino", "building_casino", columns[4], 50, G1C_WorldLocation.Casino);
            AddBuilding("Wohnhaus", "building_townhouse", columns[5], 50);

            Map.AddPark(22, 18, 14, 5);
            Map.AddPark(58, 36, 14, 5);
            AddPavilion(23, 18);
            AddPavilion(67, 36);

            foreach (G1C_WorldLocation building in new[] { G1C_WorldLocation.Home, G1C_WorldLocation.Hospital,
                         G1C_WorldLocation.ClothingStore, G1C_WorldLocation.GasStation, G1C_WorldLocation.Market,
                         G1C_WorldLocation.Casino })
            {
                int doorX = Map.DoorX(building);
                int doorY = Map.DoorY(building);
                AddProp("Laterne", "obj_street_lamp", doorX - 2, doorY + 1, G1C_InteractionType.None);
                if (building != G1C_WorldLocation.Home && building != G1C_WorldLocation.Hospital)
                    AddProp("Mülleimer", "obj_trash", doorX + 2, doorY + 1, G1C_InteractionType.Trash);
            }
            AddProp("Laterne", "obj_street_lamp", 29, 20, G1C_InteractionType.None);
            AddProp("Mülleimer", "obj_trash", 30, 20, G1C_InteractionType.Trash);
            AddProp("Laterne", "obj_street_lamp", 62, 38, G1C_InteractionType.None);
            AddProp("Mülleimer", "obj_trash", 63, 38, G1C_InteractionType.Trash);

            AddNaturalTrees();
            foreach (int[] spot in new[] { new[] { 10, 20 }, new[] { 34, 21 }, new[] { 60, 39 }, new[] { 70, 20 }, new[] { 88, 56 } })
            {
                AddPerchingTree(spot[0], spot[1]);
                AddAnimal("Vogel", "animal_bird", spot[0], spot[1], 2, 6);
            }

            AddTownNpc("Gast Lena", "guest", G1C_WorldLocation.Home);
            AddTownNpc("Kurier Paul", "dealer_green", G1C_WorldLocation.Hospital);
            AddTownNpc("Gärtner Sam", "bartender", G1C_WorldLocation.Market);
            AddTownNpc("Studentin Lea", "dealer_blue", G1C_WorldLocation.Casino);

            AddAnimal("Ente", "animal_duck", RiverBank(8, -3), 8, 2, 8);
            AddAnimal("Ente", "animal_duck", RiverBank(24, 3), 24, 2, 10);
            AddAnimal("Ente", "animal_duck", RiverBank(40, -3), 40, 2, 8);
            AddAnimal("Ente", "animal_duck", RiverBank(58, 3), 58, 2, 10);
            foreach (int y in new[] { 6, 20, 26, 38, 44, 56 }) AddFish(y);

            AddAnimal("Katze", "animal_cat", 12, 20, 2, 8);
            AddAnimal("Katze", "animal_cat", 37, 26, 2, 9);
            AddAnimal("Katze", "animal_cat", 62, 24, 2, 8);
            AddAnimal("Katze", "animal_cat", 26, 40, 2, 9);
            AddAnimal("Katze", "animal_cat", 88, 40, 2, 8);
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Die Höhe ergibt sich aus der Straße unter dem Gebäude.
        private void AddBuilding(string name, string sprite, int x, int roadY, G1C_WorldLocation? destination = null)
        {
            const int width = 6;
            const int height = 5;
            int y = Map.FitAboveRoad(x, width, height, roadY);
            if (destination.HasValue) Map.AddBuildingFootprint(x, y, width, height, destination.Value);
            else Map.AddSolidFootprint(x, y, width, height);
            _objects.Add(new G1C_WorldObject(name, sprite, x, y, width, height, true, G1C_InteractionType.None));
        }

        // Entwickler: Patrick Lupierz
        private void AddPavilion(int x, int y)
        {
            Map.AddSolidFootprint(x, y, 4, 4);
            _objects.Add(new G1C_WorldObject("Parkpavillon", "building_pavilion", x, y, 4, 4, true, G1C_InteractionType.None));
        }

        // Entwickler: Patrick Lupierz – Laternen und Mülleimer nur auf freie, begehbare Kacheln stellen
        private void AddProp(string name, string sprite, int x, int y, G1C_InteractionType interaction)
        {
            if (!Map.IsWalkable(x, y) || ObjectAt(x, y) != null) return;
            _objects.Add(new G1C_WorldObject(name, sprite, x, y, 1, 1, true, interaction));
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Bäume stehen in kleinen Gruppen statt einzeln verstreut.
        private void AddNaturalTrees()
        {
            for (int y = 2; y < G1C_ExteriorMap.MapHeight - 2; y++)
            {
                for (int x = 2; x < G1C_ExteriorMap.MapWidth - 2; x++)
                {
                    bool grove = ((x / 3) * 37 + (y / 3) * 61) % 17 == 0 && (x + y) % 2 == 0;
                    if (!grove || !Map.IsOpenGround(x, y) || Map.IsDoorApproach(x, y) || ObjectAt(x, y) != null)
                        continue;
                    _objects.Add(new G1C_WorldObject("Baum", "obj_city_tree", x, y, 1, 1, true, G1C_InteractionType.None));
                }
            }
        }

        // Entwickler: Denis Mock
        private void AddPerchingTree(int x, int y)
        {
            G1C_WorldObject existing = ObjectAt(x, y);
            if (existing == null && Map.IsWalkable(x, y))
                _objects.Add(new G1C_WorldObject("Baum", "obj_city_tree", x, y, 1, 1, true, G1C_InteractionType.None));
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Stadtbewohner starten neben dem Eingang ihres Viertels.
        private void AddTownNpc(string name, string skin, G1C_WorldLocation near)
        {
            int x = Map.DoorX(near) + 3;
            int y = Map.DoorY(near) + 2;
            while (!Map.IsWalkable(x, y) || ObjectAt(x, y) != null) x++;
            G1C_Npc npc = new G1C_Npc(name, skin, x, y, G1C_Direction.Down, G1C_InteractionType.Talk);
            _townNpcs.Add(npc);
            _objects.Add(npc);
        }

        // Entwickler: Denis Mock
        private void AddFish(int y)
        {
            int x = Map.RiverCenterXAt(y);
            if (Map.IsWater(x, y)) AddAnimal("Fisch", "animal_fish", x, y, 2, 8);
            int secondX = Map.RiverCenterXAt(y + 3) + 1;
            if (y % 2 == 0 && Map.IsWater(secondX, y + 3))
                AddAnimal("Fisch", "animal_fish", secondX, y + 3, 2, 10);
        }

        // Entwickler: Denis Mock
        private void AddAnimal(string name, string sprite, int x, int y, int frames, int animationTicks)
        {
            G1C_Animal animal = new G1C_Animal(name, sprite, x, y, frames, animationTicks);
            _animals.Add(animal);
            _objects.Add(animal);
        }

        // Entwickler: Denis Mock
        private int RiverBank(int y, int side)
        {
            return Map.RiverCenterXAt(y) + side;
        }

        // Entwickler: Denis Mock
        public G1C_WorldObject ObjectAt(int x, int y)
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i].Occupies(x, y)) return _objects[i];
            return null;
        }
    }
}
