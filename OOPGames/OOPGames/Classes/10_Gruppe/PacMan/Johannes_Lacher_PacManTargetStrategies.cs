namespace OOPGames
{
    // Chase targets of the four ghosts (Strategy pattern). Named after the behaviour, so a
    // strategy could be given to any ghost.

    // Blinky: straight at Pac-Man.
    public sealed class G10_DirectChase : IG10_TargetStrategy
    {
        public G10_Tile TargetTile(G10_Ghost ghost, G10_GhostContext context)
        {
            return context.PacMan.Tile;
        }
    }

    // Pinky: 4 tiles in front of Pac-Man.
    public sealed class G10_Ambush : IG10_TargetStrategy
    {
        const int TilesAhead = 4;

        public G10_Tile TargetTile(G10_Ghost ghost, G10_GhostContext context)
        {
            G10_Direction direction = context.PacMan.Direction;
            return context.PacMan.Tile.Offset(direction.DeltaX() * TilesAhead, direction.DeltaY() * TilesAhead);
        }
    }

    // Inky: take the point 2 tiles in front of Pac-Man and double the vector from Blinky to it.
    public sealed class G10_Flank : IG10_TargetStrategy
    {
        const int TilesAhead = 2;

        public G10_Tile TargetTile(G10_Ghost ghost, G10_GhostContext context)
        {
            G10_Direction direction = context.PacMan.Direction;
            G10_Tile pivot = context.PacMan.Tile.Offset(direction.DeltaX() * TilesAhead, direction.DeltaY() * TilesAhead);
            G10_Tile blinky = context.Blinky.Tile;
            return pivot.Offset(pivot.Col - blinky.Col, pivot.Row - blinky.Row);
        }
    }

    // Clyde: chases Pac-Man from afar, but retreats to its scatter corner within 8 tiles.
    public sealed class G10_Shy : IG10_TargetStrategy
    {
        const int ShyRadius = 8;

        public G10_Tile TargetTile(G10_Ghost ghost, G10_GhostContext context)
        {
            G10_Tile pacMan = context.PacMan.Tile;
            return ghost.Tile.DistanceSquaredTo(pacMan) > ShyRadius * ShyRadius ? pacMan : ghost.ScatterCorner;
        }
    }
}
