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
        public G1C_ParkedBike ParkedBike { get; private set; } // Patrick Lupierz – 15.11.2026
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
            // Änderung durch: Patrick Lupierz – 15.11.2026 – Wohnhaus neben dem Zuhause ist Omas Haus (betretbar)
            AddBuilding("Omas Haus", "building_grandma", columns[2], 14, G1C_WorldLocation.GrandmaHouse);
            AddBuilding("Café", "building_cafe", columns[3], 14);
            AddBuilding("Krankenhaus", "building_hospital", columns[4], 14, G1C_WorldLocation.Hospital);
            AddBuilding("Blumenladen", "building_flower_shop", columns[5], 14);

            AddBuilding("Feuerwache", "building_fire_station", columns[0], 32);
            // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Bürgerhaus wird Fahrradladen (Fahrrad auf dem Dach),
            // Kino wird Spielhalle (Arcade). Die Positionen bleiben gleich, damit Straßen und Wege unverändert bleiben.
            AddBuilding("Fahrradladen", "building_bike_shop", columns[1], 32, G1C_WorldLocation.BikeShop);
            AddBuilding("Bekleidungsgeschäft", "building_clothing", columns[2], 32, G1C_WorldLocation.ClothingStore);
            AddBuilding("Spielhalle", "building_arcade", columns[3], 32, G1C_WorldLocation.Arcade);
            AddBuilding("Tankstelle", "building_gas_station", columns[4], 32, G1C_WorldLocation.GasStation);
            AddBuilding("Wohnhaus", "building_townhouse", columns[5], 32);

            AddBuilding("Hafenlager", "building_warehouse", columns[0], 50);
            AddBuilding("Supermarkt", "building_market", columns[1], 50, G1C_WorldLocation.Market);
            AddBuilding("Bäckerei", "building_cafe", columns[2], 50);
            AddBuilding("Post", "building_post", columns[3], 50);
            AddBuilding("Casino", "building_casino", columns[4], 50, G1C_WorldLocation.Casino);
            AddBuilding("Wohnhaus", "building_townhouse", columns[5], 50);

            AddBuilding("Bank", "building_bank", 40, 14, G1C_WorldLocation.Bank);
            AddBuilding("Möbelhaus", "building_furniture", 74, 32, G1C_WorldLocation.FurnitureStore);

            Map.AddPark(22, 18, 14, 5);
            Map.AddPark(58, 36, 14, 5);
            AddPavilion(23, 18);
            AddPavilion(67, 36);

            foreach (G1C_WorldLocation building in new[] { G1C_WorldLocation.Home, G1C_WorldLocation.Hospital,
                         G1C_WorldLocation.ClothingStore, G1C_WorldLocation.GasStation, G1C_WorldLocation.Market,
                         G1C_WorldLocation.Casino, G1C_WorldLocation.Bank, G1C_WorldLocation.FurnitureStore,
                         G1C_WorldLocation.Arcade, G1C_WorldLocation.BikeShop, G1C_WorldLocation.GrandmaHouse })
            {
                int doorX = Map.DoorX(building);
                int doorY = Map.DoorY(building);
                if (building == G1C_WorldLocation.GasStation) continue; // Patrick Lupierz – 16.11.2026: eigener Vorplatz
                AddProp("Laterne", "obj_street_lamp", doorX - 2, doorY + 1, G1C_InteractionType.None);
                if (building != G1C_WorldLocation.Home && building != G1C_WorldLocation.Hospital &&
                    building != G1C_WorldLocation.GrandmaHouse)
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
            AddTownNpc("Geldhai Viktor", "dealer_red", G1C_WorldLocation.Bank, G1C_InteractionType.LoanShark);

            // Änderung durch: Patrick Lupierz – Datum: 16.11.2026 – Enten sind nicht mehr begehbar
            AddAnimal("Ente", "animal_duck", RiverBank(8, -3), 8, 2, 8, true);
            AddAnimal("Ente", "animal_duck", RiverBank(24, 3), 24, 2, 10, true);
            AddAnimal("Ente", "animal_duck", RiverBank(40, -3), 40, 2, 8, true);
            AddAnimal("Ente", "animal_duck", RiverBank(58, 3), 58, 2, 10, true);
            foreach (int y in new[] { 6, 20, 26, 38, 44, 56 }) AddFish(y);

            // Änderung durch: Patrick Lupierz – Datum: 16.11.2026 – Katzen in 5 Fellvarianten und Hunde laufen in
            // ihrem Revier umher, sind nicht begehbar und lassen sich streicheln (G1C_PetAnimal).
            AddPet("Katze Grisu", G1C_PetKind.Cat, "grey", 12, 20);
            AddPet("Katze Lotti", G1C_PetKind.Cat, "calico", 37, 26);
            AddPet("Katze Kürbis", G1C_PetKind.Cat, "orange", 62, 24);
            AddPet("Katze Nacht", G1C_PetKind.Cat, "black", 26, 40);
            AddPet("Katze Schneeflocke", G1C_PetKind.Cat, "white", 88, 40);
            AddPet("Hund Bello", G1C_PetKind.Dog, "brown", 30, 22);
            AddPet("Hund Tupfen", G1C_PetKind.Dog, "dalmatian", 64, 40);
            AddPet("Hund Goldie", G1C_PetKind.Dog, "golden", 10, 44);

            AddGasStationForecourt(); // Änderung durch: Patrick Lupierz – 15.11.2026 – Oma wohnt jetzt in ihrem Haus
            AddFishingSpots();        // Änderung durch: Patrick Lupierz – 16.11.2026 – Angeln auf den Brücken
            // Änderung durch: Patrick Lupierz – 15.11.2026 – Fahrrad vor dem Spielerhaus (links neben dem Eingang)
            ParkedBike = new G1C_ParkedBike(Map.DoorX(G1C_WorldLocation.Home) - 1, Map.DoorY(G1C_WorldLocation.Home) + 1);
            _objects.Add(ParkedBike);

            // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – lesbare Schilder in der Stadt
            AddSign("Ortsschild", Map.DoorX(G1C_WorldLocation.Home) + 2, Map.DoorY(G1C_WorldLocation.Home) + 1,
                "Willkommen in Glücksstadt!\nCasino im Süden, Bank im Norden. [M] öffnet die Stadtkarte.",
                "Kleingedruckt: Die Stadtverwaltung haftet nicht für Spielschulden.");
            AddSign("Casinoschild", Map.DoorX(G1C_WorldLocation.Casino) - 3, Map.DoorY(G1C_WorldLocation.Casino) + 1,
                "Casino Glücksstadt – Roulette, Blackjack, Poker, Spielautomaten.\nSpiel verantwortungsvoll. Das Haus gewinnt trotzdem.");
            AddSign("Parkschild", 22, 23, "Stadtpark mit Pavillon.\nBitte die Enten nicht mit Chips füttern.");
            AddSign("Oma Gerdas Briefkasten", 29, Map.DoorY(G1C_WorldLocation.Home) + 1,
                "Auf dem Briefkasten steht: \"G. – Keine Werbung, keine Vertreter,\nkeine Fragen zum Handy.\"");
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Untersuchbares Schild auf eine freie, begehbare Kachel stellen (sonst die nächste freie rechts).
        private void AddSign(string name, int x, int y, params string[] lines)
        {
            for (int i = 0; i < 4; i++, x++)
            {
                if (!Map.IsWalkable(x, y) || Map.IsDoorApproach(x, y) || ObjectAt(x, y) != null) continue;
                _objects.Add(new G1C_WorldObject(name, "obj_sign_post", x, y, 1, 1, true, G1C_InteractionType.Inspect, lines));
                return;
            }
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Vorplatz der Tankstelle: zwei rote Zapfsäulen und ein Verkaufsfenster (Nachtschalter) an der
        //          Fassade, durch das man mit Tankwart Olli spricht und einkauft – ohne das Gebäude zu betreten.
        private void AddGasStationForecourt()
        {
            int doorX = Map.DoorX(G1C_WorldLocation.GasStation);
            int doorY = Map.DoorY(G1C_WorldLocation.GasStation);
            // Änderung durch: Patrick Lupierz – Datum: 16.11.2026 – grauer Steinplatten-Gehweg vor der Tankstelle;
            // das breite Verkaufsfenster mit Markise ist Teil der Gebäudegrafik (zwei Kacheln rechts der Tür).
            Map.AddPlaza(doorX - 4, doorY + 1, 9, 1);
            _objects.Add(new G1C_ShopWindow("Tankwart Olli", "obj_sales_window", doorX + 1, doorY, G1C_WorldLocation.GasStation, 2));
            string[] pumpLines =
            {
                "Zapfsäule: Super 1,89 Chips pro Liter, Diesel 1,79.",
                "Leider hast du kein Auto. Nur ein Fahrrad. Das tankt man mit Snacks."
            };
            // Änderung durch: Patrick Lupierz – 16.11.2026 – beide Zapfsäulen auf dem Steinplatten-Gehweg links der Tür
            AddForecourtObject("Zapfsäule 1", "obj_fuel_pump", doorX - 3, doorY + 1, pumpLines);
            AddForecourtObject("Zapfsäule 2", "obj_fuel_pump", doorX - 2, doorY + 1, pumpLines);
            AddProp("Mülleimer", "obj_trash", doorX + 4, doorY + 1, G1C_InteractionType.Trash);
        }

        private void AddForecourtObject(string name, string sprite, int x, int y, string[] lines)
        {
            for (int i = 0; i < 4; i++)
            {
                int tx = x + (x < Map.DoorX(G1C_WorldLocation.GasStation) ? -i : i);
                if (!Map.IsWalkable(tx, y) || Map.IsDoorApproach(tx, y) || ObjectAt(tx, y) != null) continue;
                _objects.Add(new G1C_WorldObject(name, sprite, tx, y, 1, 1, true, G1C_InteractionType.Inspect, lines));
                return;
            }
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
        // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Änderung: Pavillon ist unterbegehbar.
        // Änderung durch: Patrick Lupierz – Datum: 16.11.2026 – Änderung: von jeder Seite betretbar. Massiv sind nur
        // noch die vier Ecksäulen; die beiden mittleren Kacheln jeder Seite sind Eingänge.
        // Das Objekt selbst ist nicht massiv; das Dach wird über die Figur gezeichnet (Sortierung nach Unterkante).
        private void AddPavilion(int x, int y)
        {
            foreach (int[] corner in new[] { new[] { x, y }, new[] { x + 3, y }, new[] { x, y + 3 }, new[] { x + 3, y + 3 } })
                Map.AddSolidFootprint(corner[0], corner[1], 1, 1);
            _objects.Add(new G1C_WorldObject("Parkpavillon", "building_pavilion", x, y, 4, 4, false, G1C_InteractionType.None));
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026
        // Aufgabe: Es gibt genau einen Angelplatz: auf der untersten (südlichsten) Flussbrücke (Flussmitte, obere
        //          Brückenreihe). Änderung 16.11.2026 (Rückmeldung Patrick): Die Angel ist nicht massiv und blockiert
        //          die Brücke nicht; man steht daneben/davor und drückt [E].
        private void AddFishingSpots()
        {
            for (int y = G1C_ExteriorMap.MapHeight - 4; y >= 2; y--)
            {
                int x = Map.RiverCenterXAt(y);
                if (!Map.IsBridge(x, y) || !Map.IsBridge(x, y + 1) || Map.IsBridge(x, y - 1) || ObjectAt(x, y) != null) continue;
                _objects.Add(new G1C_WorldObject("Angel", "obj_fishing_rod", x, y, 1, 1, false, G1C_InteractionType.Fishing,
                    "Eine Angel lehnt am Brückengeländer. Daneben ein Eimer mit Ködern."));
                return;
            }
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026 – Haustier auf die nächste freie Kachel setzen
        private void AddPet(string name, G1C_PetKind kind, string variant, int x, int y)
        {
            for (int i = 0; i < 6 && (!Map.IsWalkable(x, y) || Map.IsDoorApproach(x, y) || ObjectAt(x, y) != null); i++) x++;
            if (!Map.IsWalkable(x, y) || ObjectAt(x, y) != null) return;
            _objects.Add(new G1C_PetAnimal(name, kind, variant, x, y, 4));
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
        private void AddTownNpc(string name, string skin, G1C_WorldLocation near,
                                G1C_InteractionType interaction = G1C_InteractionType.Talk)
        {
            int x = Map.DoorX(near) + 3;
            int y = Map.DoorY(near) + 2;
            while (!Map.IsWalkable(x, y) || ObjectAt(x, y) != null) x++;
            G1C_Npc npc = new G1C_Npc(name, skin, x, y, G1C_Direction.Down, interaction);
            _townNpcs.Add(npc);
            _objects.Add(npc);
        }

        // Entwickler: Denis Mock
        private void AddFish(int y)
        {
            int x = Map.RiverCenterXAt(y);
            // Änderung durch: Patrick Lupierz – 18.11.2026 – Flussfische mit denselben Grafiken wie im Angelspiel
            if (Map.IsWater(x, y)) AddAnimal("Fisch", RiverFishSprite(y), x, y, 2, 8);
            int secondX = Map.RiverCenterXAt(y + 3) + 1;
            if (y % 2 == 0 && Map.IsWater(secondX, y + 3))
                AddAnimal("Fisch", RiverFishSprite(y + 3), secondX, y + 3, 2, 10);
        }

        // Entwickler: Patrick Lupierz – Datum: 18.11.2026 – Fischart je Flussstelle (fest, damit sie nicht wechselt)
        private static string RiverFishSprite(int y)
        {
            List<string> ids = new List<string>(G1C_FishSprites.Ids);
            return "river_" + ids[(y * 7) % ids.Count];
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Datum: 16.11.2026 – optional massiv
        private void AddAnimal(string name, string sprite, int x, int y, int frames, int animationTicks, bool solid = false)
        {
            G1C_Animal animal = new G1C_Animal(name, sprite, x, y, frames, animationTicks, solid);
            _animals.Add(animal);
            _objects.Add(animal);
        }

        // Entwickler: Denis Mock
        private int RiverBank(int y, int side)
        {
            return Map.RiverCenterXAt(y) + side;
        }

        // Entwickler: Patrick Lupierz – Datum: 18.11.2026 – Tagesablauf (G1C_NpcSchedule): Stadtbewohner kommen und gehen.
        // Abwesende bleiben in TownNpcs (Position bleibt erhalten), stehen aber nicht in der Objektliste.
        public bool IsPresent(G1C_WorldObject obj)
        {
            return _objects.Contains(obj);
        }

        public void SetNpcPresent(G1C_Npc npc, bool present)
        {
            if (npc == null || !_townNpcs.Contains(npc) || IsPresent(npc) == present) return;
            if (present) _objects.Add(npc);
            else _objects.Remove(npc);
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
