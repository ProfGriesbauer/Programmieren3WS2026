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
                    default: throw new InvalidOperationException("Unbekannter Weltbereich: " + Location);
                }
            }
        }

        // Entwickler: Denis Mock
        public G1C_SceneTransitionManager(G1C_HomeWorld home, G1C_ExteriorWorld exterior, G1C_CasinoWorld casino,
                                          G1C_BuildingInteriorWorld hospital, G1C_BuildingInteriorWorld market)
        {
            _home = home ?? throw new ArgumentNullException(nameof(home));
            _exterior = exterior ?? throw new ArgumentNullException(nameof(exterior));
            _casino = casino ?? throw new ArgumentNullException(nameof(casino));
            _hospital = hospital ?? throw new ArgumentNullException(nameof(hospital));
            _market = market ?? throw new ArgumentNullException(nameof(market));
            Location = G1C_WorldLocation.Home;
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

            if (Location == G1C_WorldLocation.Exterior && destination.Value == G1C_WorldLocation.Home) return "[E] Zuhause betreten";
            if (Location == G1C_WorldLocation.Exterior && destination.Value == G1C_WorldLocation.Casino) return "[E] Casino betreten";
            if (Location == G1C_WorldLocation.Exterior && destination.Value == G1C_WorldLocation.Hospital) return "[E] Krankenhaus betreten";
            if (Location == G1C_WorldLocation.Exterior && destination.Value == G1C_WorldLocation.Market) return "[E] Supermarkt betreten";
            if (Location == G1C_WorldLocation.Home && destination.Value == G1C_WorldLocation.Exterior) return "[E] Haus verlassen";
            if (Location == G1C_WorldLocation.Casino && destination.Value == G1C_WorldLocation.Exterior) return "[E] Casino verlassen";
            if (Location == G1C_WorldLocation.Hospital && destination.Value == G1C_WorldLocation.Exterior) return "[E] Krankenhaus verlassen";
            if (Location == G1C_WorldLocation.Market && destination.Value == G1C_WorldLocation.Exterior) return "[E] Supermarkt verlassen";
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
            throw new InvalidOperationException("Kein Spawnpunkt für den Übergang " + source + " -> " + destination + ".");
        }

        // Entwickler: Denis Mock
        private G1C_Spawn ExteriorSpawn(G1C_WorldLocation building)
        {
            int x;
            int y;
            switch (building)
            {
                case G1C_WorldLocation.Home:
                    x = _exterior.Map.HomeDoorX;
                    y = _exterior.Map.HomeDoorY;
                    break;
                case G1C_WorldLocation.Casino:
                    x = _exterior.Map.CasinoDoorX;
                    y = _exterior.Map.CasinoDoorY;
                    break;
                case G1C_WorldLocation.Hospital:
                    x = _exterior.Map.HospitalDoorX;
                    y = _exterior.Map.HospitalDoorY;
                    break;
                case G1C_WorldLocation.Market:
                    x = _exterior.Map.MarketDoorX;
                    y = _exterior.Map.MarketDoorY;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(building));
            }
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
