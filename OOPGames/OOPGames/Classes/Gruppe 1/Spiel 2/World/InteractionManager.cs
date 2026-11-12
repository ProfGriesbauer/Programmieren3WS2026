namespace OOPGames
{
    /// <summary>
    /// Findet das Objekt, vor dem der Spieler steht und auf das er schaut,
    /// und liefert den passenden Hinweistext für die Benutzeroberfläche.
    /// </summary>
    public class G1C_InteractionManager
    {
        private readonly G1C_CasinoWorld _world;

        public G1C_InteractionManager(G1C_CasinoWorld world)
        {
            _world = world;
        }

        public G1C_WorldObject FindTarget(G1C_Player player)
        {
            if (player.IsMoving) return null;
            G1C_WorldObject obj = _world.ObjectAt(player.FacingTileX, player.FacingTileY);
            return obj != null && obj.Interaction != G1C_InteractionType.None ? obj : null;
        }

        public bool IsFacingExit(G1C_Player player)
        {
            return !player.IsMoving && _world.Map.GetTile(player.FacingTileX, player.FacingTileY) == G1C_TileType.Exit;
        }

        public string GetHint(G1C_Player player)
        {
            G1C_WorldObject target = FindTarget(player);
            if (target == null)
            {
                return IsFacingExit(player) ? "[E] Ausgang" : "";
            }

            switch (target.Interaction)
            {
                case G1C_InteractionType.Roulette: return "[E] Roulette spielen";
                case G1C_InteractionType.Blackjack: return "[E] Blackjack spielen";
                case G1C_InteractionType.Poker: return "[E] Poker spielen";
                case G1C_InteractionType.Slots: return "[E] Spielautomat";
                case G1C_InteractionType.Bartender: return "[E] Mit dem Barkeeper sprechen";
                default: return "[E] Sprechen";
            }
        }
    }
}
