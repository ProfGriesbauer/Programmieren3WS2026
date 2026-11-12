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

        public int PlayerStartX { get { return 25; } }
        public int PlayerStartY { get { return 30; } }
        public int TileSize { get { return 16; } }

        // Entwickler: Denis Mock
        public G1C_ExteriorWorld()
        {
            BuildCity();
        }

        // Entwickler: Denis Mock
        private void BuildCity()
        {
            AddBuilding("Zuhause", "building_home", 22, 25, 6, 5, G1C_WorldLocation.Home);
            AddBuilding("Krankenhaus", "building_hospital", 146, 25, 6, 5, G1C_WorldLocation.Hospital);
            AddBuilding("Supermarkt", "building_market", 22, 145, 6, 5, G1C_WorldLocation.Market);
            AddBuilding("Casino", "building_casino", 146, 145, 6, 5, G1C_WorldLocation.Casino);

            AddBuilding("Bibliothek", "building_library", 4, 25, 6, 5);
            AddBuilding("Wohnhaus", "building_townhouse", 31, 25, 6, 5);
            AddBuilding("Caf\u00e9", "building_cafe", 140, 25, 6, 5);
            AddBuilding("Blumenladen", "building_flower_shop", 153, 25, 6, 5);
            AddBuilding("Feuerwache", "building_fire_station", 4, 97, 6, 5);
            AddBuilding("B\u00fcrgerhaus", "building_townhouse", 20, 97, 6, 5);
            AddBuilding("Kino", "building_cinema", 140, 97, 6, 5);
            AddBuilding("Wohnhaus", "building_townhouse", 153, 97, 6, 5);
            AddBuilding("Parkpavillon", "building_pavilion", 5, 72, 4, 4);
            AddBuilding("Parkpavillon", "building_pavilion", 120, 97, 4, 4);
            AddBuilding("Hafenlager", "building_warehouse", 4, 145, 6, 5);
            AddBuilding("B\u00e4ckerei", "building_cafe", 31, 145, 6, 5);
            AddBuilding("Post", "building_post", 140, 145, 6, 5);
            AddBuilding("Wohnhaus", "building_townhouse", 153, 145, 6, 5);

            AddNaturalTrees();
            AddPerchingTree(12, 32);
            AddPerchingTree(54, 74);
            AddPerchingTree(124, 101);
            AddPerchingTree(54, 145);
            AddPerchingTree(166, 154);
            AddTownNpc("Gast Lena", "guest", 34, 34);
            AddTownNpc("Kurier Paul", "dealer_green", 142, 35);
            AddTownNpc("G\u00e4rtner Sam", "bartender", 34, 154);
            AddTownNpc("Studentin Lea", "dealer_blue", 142, 154);

            AddAnimal("Ente", "animal_duck", RiverBank(21, -3), 21, 2, 8);
            AddAnimal("Ente", "animal_duck", RiverBank(58, 3), 58, 2, 10);
            AddAnimal("Ente", "animal_duck", RiverBank(122, -3), 122, 2, 8);
            AddAnimal("Ente", "animal_duck", RiverBank(154, 3), 154, 2, 10);

            AddFish(21);
            AddFish(33);
            AddFish(52);
            AddFish(77);
            AddFish(101);
            AddFish(125);
            AddFish(149);
            AddFish(169);

            AddAnimal("Katze", "animal_cat", 12, 57, 2, 8);
            AddAnimal("Katze", "animal_cat", 62, 80, 2, 9);
            AddAnimal("Katze", "animal_cat", 127, 74, 2, 8);
            AddAnimal("Katze", "animal_cat", 47, 133, 2, 9);
            AddAnimal("Katze", "animal_cat", 167, 126, 2, 8);

            AddAnimal("Vogel", "animal_bird", 12, 32, 2, 6);
            AddAnimal("Vogel", "animal_bird", 54, 74, 2, 7);
            AddAnimal("Vogel", "animal_bird", 124, 101, 2, 6);
            AddAnimal("Vogel", "animal_bird", 54, 145, 2, 7);
            AddAnimal("Vogel", "animal_bird", 166, 154, 2, 6);
        }

        // Entwickler: Denis Mock
        private void AddBuilding(string name, string sprite, int x, int y, int width, int height,
                                 G1C_WorldLocation? destination = null)
        {
            if (destination.HasValue) Map.AddBuildingFootprint(x, y, width, height, destination.Value);
            else Map.AddSolidFootprint(x, y, width, height);
            _objects.Add(new G1C_WorldObject(name, sprite, x, y, width, height, true, G1C_InteractionType.None));
        }

        // Entwickler: Denis Mock
        private void AddNaturalTrees()
        {
            for (int y = 2; y < G1C_ExteriorMap.MapSize - 2; y++)
            {
                for (int x = 2; x < G1C_ExteriorMap.MapSize - 2; x++)
                {
                    if ((x * 37 + y * 61) % 211 != 0 || !Map.IsOpenGround(x, y) || Map.IsDoorApproach(x, y))
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
        private void AddTownNpc(string name, string skin, int x, int y)
        {
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
