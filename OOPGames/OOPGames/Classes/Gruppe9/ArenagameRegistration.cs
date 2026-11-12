namespace OOPGames
{
    internal static class ArenaGameRegistration
    {
        internal static void RegisterArenaGame()
        {
            OOPGamesManager.Singleton.RegisterPainter(new PainterG9());
            OOPGamesManager.Singleton.RegisterRules(new ArenaRules());
            OOPGamesManager.Singleton.RegisterPlayer(new HumanPlayerSteering());
        }
    }
}