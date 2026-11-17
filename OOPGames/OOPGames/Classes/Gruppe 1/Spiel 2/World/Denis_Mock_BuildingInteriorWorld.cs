using System.Collections.Generic;

namespace OOPGames
{
    // Entwickler: Denis Mock
    // Frameworkkonforme, eingerichtete Innenr\u00e4ume f\u00fcr Krankenhaus und Supermarkt.
    public sealed class G1C_BuildingInteriorWorld : IG1C_WorldScene
    {
        private readonly List<G1C_WorldObject> _objects = new List<G1C_WorldObject>();

        public G1C_BuildingInteriorMap Map { get; }
        public IReadOnlyList<G1C_WorldObject> Objects { get { return _objects; } }
        IG1C_WorldMap IG1C_WorldScene.Grid { get { return Map; } }
        public int PlayerStartX { get { return 7; } }
        public int PlayerStartY { get { return 10; } }
        public int TileSize { get { return 24; } }
        public G1C_WorldLocation Location { get; }

        // Entwickler: Denis Mock
        public G1C_BuildingInteriorWorld(G1C_WorldLocation location)
        {
            // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – Tankstelle und Bekleidungsgeschäft
            Location = location;
            Map = new G1C_BuildingInteriorMap(location);
            switch (location)
            {
                case G1C_WorldLocation.Hospital: BuildHospital(); break;
                case G1C_WorldLocation.Market: BuildMarket(); break;
                case G1C_WorldLocation.GasStation: BuildGasStation(); break;
                case G1C_WorldLocation.ClothingStore: BuildClothingStore(); break;
                case G1C_WorldLocation.Bank: BuildBank(); break;
                case G1C_WorldLocation.FurnitureStore: BuildFurnitureStore(); break;
                case G1C_WorldLocation.Arcade: BuildArcade(); break;       // Patrick Lupierz – 15.11.2026
                case G1C_WorldLocation.BikeShop: BuildBikeShop(); break;   // Patrick Lupierz – 15.11.2026
                case G1C_WorldLocation.GrandmaHouse: BuildGrandmaHouse(); break; // Patrick Lupierz – 15.11.2026
                default: throw new System.ArgumentOutOfRangeException(nameof(location));
            }
            AddWallDecor(); // Änderung durch: Patrick Lupierz – 15.11.2026
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Je Raum ein eigenes, untersuchbares Bild/Poster an der Rückwand (keine identische Wiederverwendung).
        private void AddWallDecor()
        {
            switch (Location)
            {
                case G1C_WorldLocation.Hospital: AddPicture("Gesundheitsposter", "health", 6, "\"Hände waschen! 20 Sekunden – so lange wie ein Roulette-Dreh.\""); break;
                case G1C_WorldLocation.Market: AddPicture("Angebotsplakat", "sale", 7, "\"Diese Woche: Äpfel im Angebot.\"\nLetzte Woche auch. Nächste Woche vermutlich ebenfalls."); break;
                case G1C_WorldLocation.GasStation: AddPicture("Urlaubsposter", "sea", 4, "\"Fahr doch mal ans Meer!\"\nHinweis der Redaktion: Glücksstadt hat kein Meer."); break;
                case G1C_WorldLocation.ClothingStore: AddPicture("Modeplakat", "fashion", 9, "\"Herbstkollektion: Rot ist das neue Rot.\""); break;
                case G1C_WorldLocation.Bank: AddPicture("Zinsplakat", "interest", 6, "\"2 % Zinsen – jeden Tag! Ihr Geld arbeitet, während Sie schlafen.\""); break;
                case G1C_WorldLocation.FurnitureStore: AddPicture("Landschaftsbild", "mountain", 11, "Ein Bergpanorama. Preisschild: \"Unverkäuflich – nur Deko.\""); break;
                case G1C_WorldLocation.BikeShop: AddPicture("Rennplakat", "bike", 7, "\"Tour de Glücksstadt – Start am Rathaus.\"\nEin Rathaus gibt es noch nicht."); break;
            }
        }

        private void AddPicture(string name, string motif, int x, string text)
        {
            for (int i = 0; i < Map.Width; i++)
            {
                int tx = 1 + (x - 1 + i) % (Map.Width - 2);
                if (ObjectAt(tx, 0) != null || !Map.IsWalkable(tx, 1) || ObjectAt(tx, 1) != null) continue;
                _objects.Add(new G1C_WorldObject(name, "obj_picture_" + motif, tx, 0, 1, 1, true, G1C_InteractionType.Inspect, text));
                return;
            }
        }

        // Entwickler: Denis Mock
        private void BuildHospital()
        {
            AddFurniture("Empfangstresen", "obj_counter", 2, 2, 3, 1);
            AddFurniture("Behandlungsbett", "obj_medical_bed", 3, 4, 2, 1);
            AddFurniture("Behandlungsbett", "obj_medical_bed", 9, 4, 2, 1);
            AddFurniture("Medizinschrank", "obj_clinic_cabinet", 3, 7, 2, 2);
            AddFurniture("Pflanze", "obj_plant", 13, 2, 1, 1);
            AddFurniture("Stuhl", "obj_chair", 7, 6, 1, 1);
        }

        // Entwickler: Denis Mock
        private void BuildMarket()
        {
            AddFurniture("Regal", "obj_store_shelf", 2, 2, 2, 2);
            AddFurniture("Regal", "obj_store_shelf", 5, 2, 2, 2);
            AddFurniture("Regal", "obj_store_shelf", 9, 2, 2, 2);
            // Änderung durch: Patrick Lupierz – die Kasse ist jetzt ein echter Verkaufstresen
            AddFurniture("Kasse", "obj_counter", 10, 6, 3, 1, G1C_InteractionType.Shop);
            AddSeller("Kassiererin Jana", "guest", 11, 5);
            AddFurniture("K\u00fchlschrank", "obj_clinic_cabinet", 13, 6, 1, 2);
            AddFurniture("Pflanze", "obj_plant", 2, 8, 1, 1);
        }

        // Entwickler: Patrick Lupierz
        private void BuildGasStation()
        {
            AddFurniture("Regal", "obj_store_shelf", 2, 2, 2, 2);
            AddFurniture("Regal", "obj_store_shelf", 5, 2, 2, 2);
            AddFurniture("K\u00fchlregal", "obj_clinic_cabinet", 13, 2, 1, 2);
            AddFurniture("Regal", "obj_store_shelf", 2, 6, 2, 2);
            AddFurniture("Verkaufstheke", "obj_counter", 9, 4, 3, 1, G1C_InteractionType.Shop);
            AddSeller("Tankwart Olli", "dealer_green", 10, 3);
            AddFurniture("Verkaufsautomat", "obj_vending_machine", 13, 8, 1, 1, G1C_InteractionType.Vending);
            AddFurniture("M\u00fclleimer", "obj_trash", 13, 9, 1, 1, G1C_InteractionType.Trash);
            // \u00c4nderung durch: Patrick Lupierz \u2013 Datum: 15.11.2026 \u2013 kleiner Sticker an der Wand (untersuchbar)
            _objects.Add(new G1C_WorldObject("Sticker", "obj_sticker_bw", 8, 0, 1, 1, true, G1C_InteractionType.Inspect,
                "Ein kleiner gelber Sticker klebt an der Wand:\n\"Nett hier, aber waren Sie schon mal in Baden-W\u00fcrttemberg?\""));
        }

        // Entwickler: Patrick Lupierz
        private void BuildClothingStore()
        {
            AddFurniture("Kleiderst\u00e4nder", "obj_clothes_rack", 2, 2, 2, 1);
            AddFurniture("Kleiderst\u00e4nder", "obj_clothes_rack", 5, 2, 2, 1);
            AddFurniture("Kleiderst\u00e4nder", "obj_clothes_rack", 2, 5, 2, 1);
            AddFurniture("Kleiderst\u00e4nder", "obj_clothes_rack", 5, 5, 2, 1);
            AddFurniture("Spiegel", "obj_mirror", 13, 1, 1, 1);
            AddFurniture("Verkaufstheke", "obj_counter", 10, 4, 3, 1, G1C_InteractionType.Shop);
            AddSeller("Verk\u00e4uferin Mia", "dealer_red", 11, 3);
            AddFurniture("Pflanze", "obj_plant", 2, 9, 1, 1);
        }

        private void BuildBank()
        {
            // Änderung durch: Patrick Lupierz – 15.11.2026 – Schalter mit Durchreiche vor Frida (10,2)
            AddFurniture("Schalter", "obj_counter", 8, 3, 2, 1);
            AddFurniture("Schalter", "obj_counter", 11, 3, 2, 1);
            // Änderung durch: Patrick Lupierz – 15.11.2026 – statt auf zwei Kacheln gestreckter Stuhlgrafik je zwei Stühle
            foreach (int chairX in new[] { 3, 4, 11, 12 }) AddFurniture("Stuhl", "obj_chair", chairX, 5, 1, 1);
            AddFurniture("Tresor", "obj_clinic_cabinet", 2, 2, 2, 2);
            AddSeller("Bankierin Frida", "dealer_blue", 10, 2, G1C_InteractionType.Bank);
            AddFurniture("Pflanze", "obj_plant", 13, 2, 1, 1);
            // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – mehr Mitarbeiter und Einrichtung:
            // Beratungsplatz (Tisch, Stühle) mit Bankberater Karl, Wachmann Bodo am Eingang, Wartebereich.
            AddFurniture("Beratungstisch", "obj_home_table", 10, 8, 2, 1);
            AddFurniture("Stuhl", "obj_chair", 10, 9, 1, 1);
            AddFurniture("Stuhl", "obj_chair", 11, 9, 1, 1);
            AddSeller("Bankberater Karl", G1C_ShopNpcRoster.KarlSkin, 10, 7, G1C_InteractionType.Bank);
            AddFurniture("Wartetisch", "obj_home_table", 2, 8, 2, 1);
            AddFurniture("Stuhl", "obj_chair", 4, 8, 1, 1);
            AddFurniture("Pflanze", "obj_plant", 1, 10, 1, 1);
            _objects.Add(new G1C_Npc("Wachmann Bodo", G1C_ShopNpcRoster.BodoSkin, 9, 10,
                                     G1C_Direction.Left, G1C_InteractionType.Talk));
        }

        private void BuildFurnitureStore()
        {
            AddFurniture("Regal mit Pflanzen", "obj_store_shelf", 2, 2, 2, 2);
            AddFurniture("Regal mit Tischen", "obj_store_shelf", 5, 2, 2, 2);
            AddFurniture("Regal mit Stühlen", "obj_store_shelf", 8, 2, 2, 2);
            AddFurniture("Verkaufstheke", "obj_counter", 10, 6, 3, 1);
            AddSeller("Möbelhändlerin Lotte", "guest", 11, 5, G1C_InteractionType.FurnitureShop);
            AddFurniture("Pflanze", "obj_plant", 13, 2, 1, 1);
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Spielhalle (SprintPlanning C11): dunkler Boden und Wände mit Neon, vier Automaten an der
        //          Rückwand, Gast-Automat für Spiele anderer Gruppen in der Mitte, Greifautomat (aus dem Casino
        //          hierher verschoben), Highscore-Tafel, Aufsicht Paula. Die Automaten-Namen verweisen auf
        //          G1C_ArcadeCatalog (Spiele) – Grafik und Spiellogik bleiben getrennt.
        private void BuildArcade()
        {
            for (int i = 0; i < G1C_ArcadeCatalog.Cabinets.Count; i++)
            {
                G1C_ArcadeCabinet cabinet = G1C_ArcadeCatalog.Cabinets[i];
                _objects.Add(new G1C_WorldObject(cabinet.Name, cabinet.SpriteId, cabinet.X, cabinet.Y, 1, 1, true,
                                                 G1C_InteractionType.Arcade));
            }
            _objects.Add(new G1C_WorldObject("Greifautomat", "obj_claw_machine", 12, 6, 2, 2, true,
                G1C_InteractionType.Claw, "Im Greifautomaten liegen Gewinne für deine Sammlung.", "Willst du dein Glück versuchen?"));
            _objects.Add(new G1C_WorldObject(G1C_ArcadeCatalog.HighscoreBoardName, "obj_highscore_board", 7, 0, 1, 1, true,
                                             G1C_InteractionType.Arcade));
            _objects.Add(new G1C_WorldObject("Neonschild", "obj_neon_sign", 3, 0, 1, 1, true, G1C_InteractionType.Inspect,
                "Ein summendes Neonschild: \"INSERT COIN\".\nDarunter klein: \"Bitte keine Limo auf die Automaten.\""));
            AddFurniture("Theke", "obj_counter", 1, 8, 3, 1);
            _objects.Add(new G1C_Npc("Aufsicht Paula", G1C_ShopNpcRoster.PaulaSkin, 2, 7,
                                     G1C_Direction.Down, G1C_InteractionType.Talk));
            AddFurniture("Stuhl", "obj_chair", 10, 9, 1, 1);
            AddFurniture("Pflanze", "obj_plant", 14, 10, 1, 1);
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Fahrradladen mit ausgestellten Fahrrädern aller Farben (untersuchbar), Werkbank und Verkäufer.
        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Omas Haus (SprintPlanning C9): Oma wohnt hier mit ihren drei Katzen und verlässt es nicht.
        //          Sessel, Teetisch, Bett, Bücherregal, Katzenbild; Blümchentapete und Dielenboden.
        private void BuildGrandmaHouse()
        {
            _objects.Add(new G1C_Npc(G1C_FamilyNpcRoster.GrandmaName, G1C_FamilyNpcRoster.GrandmaSkin, 4, 4,
                                     G1C_Direction.Down, G1C_InteractionType.Talk));
            AddFurniture("Sessel", "obj_chair", 3, 4, 1, 1);
            AddFurniture("Teetisch", "obj_home_table", 4, 5, 2, 1);
            AddFurniture("Omas Bett", "obj_home_bed", 11, 2, 2, 1);
            AddFurniture("Bücherregal", "obj_store_shelf", 1, 1, 2, 2);
            AddFurniture("Pflanze", "obj_plant", 14, 1, 1, 1);
            AddFurniture("Pflanze", "obj_plant", 1, 10, 1, 1);
            _objects.Add(new G1C_WorldObject("Katzenbild", "obj_picture_cat", 7, 0, 1, 1, true, G1C_InteractionType.Inspect,
                "Ein Ölbild von Schnurrbert, Mausi und Herrn Doktor Krallmann.\nDarunter: \"Meine wahren Erben.\""));
            string[] cats = { "Schnurrbert", "Mausi", "Herr Doktor Krallmann" };
            int[][] spots = { new[] { 6, 4 }, new[] { 12, 6 }, new[] { 9, 9 } };
            // Änderung durch: Patrick Lupierz – Datum: 16.11.2026 – Katzen laufen umher, sind streichelbar und massiv
            string[] variants = { "orange", "calico", "black" };
            for (int i = 0; i < cats.Length; i++)
                _objects.Add(new G1C_PetAnimal(cats[i], G1C_PetKind.Cat, variants[i], spots[i][0], spots[i][1], 2));
        }

        private void BuildBikeShop()
        {
            string[] bikes = { "bike_red", "bike_blue", "bike_green", "bike_yellow", "bike_black" };
            for (int i = 0; i < bikes.Length; i++)
            {
                G1C_Item bike = G1C_ItemCatalog.Get(bikes[i]);
                _objects.Add(new G1C_WorldObject(bike.Name, "obj_" + bike.Id, 2 + i * 2, 3, 1, 1, true, G1C_InteractionType.Inspect,
                    bike.Name + " – " + G1C_Shop.PriceWithMarkup(bike.Price) + " Chips.\nKaufen kannst du es bei Rudi an der Theke."));
            }
            AddFurniture("Werkbank", "obj_home_table", 2, 8, 2, 1);
            AddFurniture("Verkaufstheke", "obj_counter", 10, 6, 3, 1, G1C_InteractionType.Shop);
            AddSeller("Radhändler Rudi", G1C_ShopNpcRoster.RudiSkin, 11, 5);
            AddFurniture("Pflanze", "obj_plant", 13, 2, 1, 1);
        }

        // Entwickler: Denis Mock
        private void AddFurniture(string name, string sprite, int x, int y, int width, int height,
                                  G1C_InteractionType interaction = G1C_InteractionType.None)
        {
            _objects.Add(new G1C_WorldObject(name, sprite, x, y, width, height, true, interaction));
        }

        // Entwickler: Patrick Lupierz – Verkäufer steht hinter der Theke und öffnet ebenfalls den Laden
        private void AddSeller(string name, string skin, int x, int y,
                               G1C_InteractionType interaction = G1C_InteractionType.Shop)
        {
            _objects.Add(new G1C_Npc(name, skin, x, y, G1C_Direction.Down, interaction));
        }

        // Entwickler: Denis Mock
        public G1C_WorldObject ObjectAt(int x, int y)
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
                if (_objects[i].Occupies(x, y)) return _objects[i];
            return null;
        }
    }

    // Entwickler: Denis Mock
    // Eigene Kollisions- und Bodendaten der beiden eingerichteten Gesch\u00e4ftsr\u00e4ume.
    public sealed class G1C_BuildingInteriorMap : IG1C_WorldMap
    {
        private const int MapWidth = 16;
        private const int MapHeight = 12;
        private readonly G1C_WorldLocation _location;

        // Entwickler: Denis Mock
        public G1C_BuildingInteriorMap(G1C_WorldLocation location)
        {
            _location = location;
        }

        public int Width { get { return MapWidth; } }
        public int Height { get { return MapHeight; } }

        // Entwickler: Denis Mock
        public bool IsWalkable(int x, int y)
        {
            return GetTile(x, y) == '.' || GetTile(x, y) == 'r';
        }

        // Entwickler: Denis Mock
        public G1C_WorldLocation? GetTransitionAt(int x, int y)
        {
            return GetTile(x, y) == 'E' ? G1C_WorldLocation.Exterior : (G1C_WorldLocation?)null;
        }

        // Entwickler: Denis Mock
        public string GetTileSpriteId(int x, int y)
        {
            char tile = GetTile(x, y);
            if (tile == 'E') return "tile_interior_exit";
            // Änderung durch: Patrick Lupierz – Kachelsatz je Gebäude (hospital, market, gas, clothing)
            string set = _location == G1C_WorldLocation.Hospital ? "hospital"
                       : _location == G1C_WorldLocation.GasStation ? "gas"
                       : _location == G1C_WorldLocation.ClothingStore ? "clothing"
                       : _location == G1C_WorldLocation.Bank ? "bank"
                       : _location == G1C_WorldLocation.FurnitureStore ? "furniture"
                       : _location == G1C_WorldLocation.Arcade ? "arcade"      // Patrick Lupierz – 15.11.2026
                       : _location == G1C_WorldLocation.BikeShop ? "bike"
                       : _location == G1C_WorldLocation.GrandmaHouse ? "grandma" : "market";
            if (tile == 'r' && _location == G1C_WorldLocation.Arcade) return "tile_arcade_floor";
            // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – linke/rechte/untere Wand zeigen die Mauerkrone
            // (eigene Grafik) statt der Oberwand; der Teppich setzt sich aus Rand- und Mittelteilen zusammen.
            if (tile == '#' && y > 0)
                return "tile_wallside_" + set + "_" + (x == 0 ? "l" : x == Width - 1 ? "r" : "b");
            if (tile == '#') return "tile_" + set + "_wall";
            if (tile == 'r') return "tile_rug_" + set + "_" + G1C_TileMask.Of(x, y, (tx, ty) => GetTile(tx, ty) == 'r');
            return "tile_" + set + "_floor";
        }

        // Entwickler: Denis Mock
        private char GetTile(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height) return '#';
            if (y == Height - 1) return x == 7 ? 'E' : '#';
            return x == 0 || x == Width - 1 || y == 0 ? '#' : (x >= 6 && x <= 8 && y >= 7 && y <= 8 ? 'r' : '.');
        }
    }
}
