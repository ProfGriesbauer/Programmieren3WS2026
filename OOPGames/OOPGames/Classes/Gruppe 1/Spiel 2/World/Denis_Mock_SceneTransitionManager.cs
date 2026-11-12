using System;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_SceneTransitionManager
    // Entwickler: Denis Mock
    // Beschreibung: Verbindet Haus, Stadt und vorhandenes Casino über Türfelder.
    // =====================================================
    public class G1C_SceneTransitionManager
    {
        private readonly G1C_HomeWorld _home;
        private readonly G1C_ExteriorWorld _exterior;
        private readonly G1C_CasinoWorld _casino;
        private readonly G1C_BuildingInteriorWorld _hospital;
        private readonly G1C_BuildingInteriorWorld _market;
        // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – weitere Geschäfte ohne eigene Felder
        private readonly System.Collections.Generic.Dictionary<G1C_WorldLocation, G1C_BuildingInteriorWorld> _shops =
            new System.Collections.Generic.Dictionary<G1C_WorldLocation, G1C_BuildingInteriorWorld>();

        public G1C_WorldLocation Location { get; private set; }

        public IG1C_WorldScene CurrentWorld
        {
            get
            {
                switch (Location)
                {
                    case G1C_WorldLocation.Home: return _home;
                    case G1C_WorldLocation.Exterior: return _exterior;
                    case G1C_WorldLocation.Casino: return _casino;
                    case G1C_WorldLocation.Hospital: return _hospital;
                    case G1C_WorldLocation.Market: return _market;
                    default:
                        G1C_BuildingInteriorWorld shop;
                        if (_shops.TryGetValue(Location, out shop)) return shop;
                        throw new InvalidOperationException("Unbekannter Weltbereich: " + Location);
                }
            }
        }

        // Entwickler: Denis Mock
        public G1C_SceneTransitionManager(G1C_HomeWorld home, G1C_ExteriorWorld exterior, G1C_CasinoWorld casino,
                                          G1C_BuildingInteriorWorld hospital, G1C_BuildingInteriorWorld market,
                                          params G1C_BuildingInteriorWorld[] shops)
        {
            _home = home ?? throw new ArgumentNullException(nameof(home));
            _exterior = exterior ?? throw new ArgumentNullException(nameof(exterior));
            _casino = casino ?? throw new ArgumentNullException(nameof(casino));
            _hospital = hospital ?? throw new ArgumentNullException(nameof(hospital));
            _market = market ?? throw new ArgumentNullException(nameof(market));
            foreach (G1C_BuildingInteriorWorld shop in shops) _shops[shop.Location] = shop;
            Location = G1C_WorldLocation.Home;
        }

        // Entwickler: Patrick Lupierz – Ort beim Laden eines Spielstands wiederherstellen
        public void Restore(G1C_WorldLocation location)
        {
            Location = location;
        }

        // Entwickler: Patrick Lupierz
        public static string DisplayName(G1C_WorldLocation location)
        {
            switch (location)
            {
                case G1C_WorldLocation.Home: return "Zuhause";
                case G1C_WorldLocation.Casino: return "Casino";
                case G1C_WorldLocation.Hospital: return "Krankenhaus";
                case G1C_WorldLocation.Market: return "Supermarkt";
                case G1C_WorldLocation.GasStation: return "Tankstelle";
                case G1C_WorldLocation.ClothingStore: return "Bekleidungsgeschäft";
                default: return "Stadt";
            }
        }

        // Entwickler: Denis Mock
        // Ein Übergang wird nur ausgelöst, wenn die Figur stillsteht und direkt auf eine Tür schaut.
        public bool TryTransition(G1C_Player player)
        {
            if (player == null || player.IsMoving) return false;

            G1C_WorldLocation? destination = CurrentWorld.Grid.GetTransitionAt(player.FacingTileX, player.FacingTileY);
            if (!destination.HasValue) return false;

            G1C_Spawn spawn = GetSpawn(Location, destination.Value);
            Location = destination.Value;
            player.Teleport(spawn.X, spawn.Y, spawn.Facing);
            return true;
        }

        // Entwickler: Denis Mock
        public string GetHint(G1C_Player player)
        {
            if (player == null || player.IsMoving) return "";

            G1C_WorldLocation? destination = CurrentWorld.Grid.GetTransitionAt(player.FacingTileX, player.FacingTileY);
            if (!destination.HasValue) return "";

            // Änderung durch: Patrick Lupierz – Hinweise für alle Gebäude aus einer Namenstabelle
            if (Location == G1C_WorldLocation.Exterior) return "[E] " + DisplayName(destination.Value) + " betreten";
            if (destination.Value == G1C_WorldLocation.Exterior) return "[E] " + DisplayName(Location) + " verlassen";
            return "";
        }

        // Entwickler: Denis Mock
        private G1C_Spawn GetSpawn(G1C_WorldLocation source, G1C_WorldLocation destination)
        {
            if (source == G1C_WorldLocation.Home && destination == G1C_WorldLocation.Exterior)
                return ExteriorSpawn(G1C_WorldLocation.Home);
            if (source == G1C_WorldLocation.Exterior && destination == G1C_WorldLocation.Home)
                return new G1C_Spawn(5, 10, G1C_Direction.Up);
            if (source == G1C_WorldLocation.Exterior && destination == G1C_WorldLocation.Casino)
                return new G1C_Spawn(9, 10, G1C_Direction.Up);
            if (source == G1C_WorldLocation.Casino && destination == G1C_WorldLocation.Exterior)
                return ExteriorSpawn(G1C_WorldLocation.Casino);
            if (source == G1C_WorldLocation.Exterior && destination == G1C_WorldLocation.Hospital)
                return new G1C_Spawn(_hospital.PlayerStartX, _hospital.PlayerStartY, G1C_Direction.Down);
            if (source == G1C_WorldLocation.Hospital && destination == G1C_WorldLocation.Exterior)
                return ExteriorSpawn(G1C_WorldLocation.Hospital);
            if (source == G1C_WorldLocation.Exterior && destination == G1C_WorldLocation.Market)
                return new G1C_Spawn(_market.PlayerStartX, _market.PlayerStartY, G1C_Direction.Down);
            if (source == G1C_WorldLocation.Market && destination == G1C_WorldLocation.Exterior)
                return ExteriorSpawn(G1C_WorldLocation.Market);
            // Änderung durch: Patrick Lupierz – weitere Geschäfte
            G1C_BuildingInteriorWorld shop;
            if (source == G1C_WorldLocation.Exterior && _shops.TryGetValue(destination, out shop))
                return new G1C_Spawn(shop.PlayerStartX, shop.PlayerStartY, G1C_Direction.Up);
            if (destination == G1C_WorldLocation.Exterior && _shops.ContainsKey(source))
                return ExteriorSpawn(source);
            throw new InvalidOperationException("Kein Spawnpunkt für den Übergang " + source + " -> " + destination + ".");
        }

        // Entwickler: Denis Mock
        private G1C_Spawn ExteriorSpawn(G1C_WorldLocation building)
        {
            // Änderung durch: Patrick Lupierz – Türpositionen kommen aus der Karte
            int x = _exterior.Map.DoorX(building);
            int y = _exterior.Map.DoorY(building);
            if (x == 0) throw new ArgumentOutOfRangeException(nameof(building));
            return new G1C_Spawn(x, y + 1, G1C_Direction.Up);
        }

        private struct G1C_Spawn
        {
            public readonly int X;
            public readonly int Y;
            public readonly G1C_Direction Facing;

            public G1C_Spawn(int x, int y, G1C_Direction facing)
            {
                X = x;
                Y = y;
                Facing = facing;
            }
        }
    }
}
