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
                    default: throw new InvalidOperationException("Unbekannter Weltbereich: " + Location);
                }
            }
        }

        // Entwickler: Denis Mock
        public G1C_SceneTransitionManager(G1C_HomeWorld home, G1C_ExteriorWorld exterior, G1C_CasinoWorld casino)
        {
            _home = home ?? throw new ArgumentNullException(nameof(home));
            _exterior = exterior ?? throw new ArgumentNullException(nameof(exterior));
            _casino = casino ?? throw new ArgumentNullException(nameof(casino));
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
            if (Location == G1C_WorldLocation.Home && destination.Value == G1C_WorldLocation.Exterior) return "[E] Haus verlassen";
            if (Location == G1C_WorldLocation.Casino && destination.Value == G1C_WorldLocation.Exterior) return "[E] Casino verlassen";
            return "";
        }

        // Entwickler: Denis Mock
        private static G1C_Spawn GetSpawn(G1C_WorldLocation source, G1C_WorldLocation destination)
        {
            if (source == G1C_WorldLocation.Home && destination == G1C_WorldLocation.Exterior)
                return new G1C_Spawn(4, 15, G1C_Direction.Down);
            if (source == G1C_WorldLocation.Exterior && destination == G1C_WorldLocation.Home)
                return new G1C_Spawn(5, 10, G1C_Direction.Up);
            if (source == G1C_WorldLocation.Exterior && destination == G1C_WorldLocation.Casino)
                return new G1C_Spawn(9, 10, G1C_Direction.Up);
            if (source == G1C_WorldLocation.Casino && destination == G1C_WorldLocation.Exterior)
                return new G1C_Spawn(14, 15, G1C_Direction.Down);
            throw new InvalidOperationException("Kein Spawnpunkt für den Übergang " + source + " -> " + destination + ".");
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
