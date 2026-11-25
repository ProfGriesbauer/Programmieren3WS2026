namespace OOPGames
{
    public sealed class ArenaRules : IGameRules2
    {
        private readonly ArenaField _field = new ArenaField
        {
            SaveGames = new ArenaSaveGameRepository(ArenaSaveGameRepository.DefaultFolder),
            Scoreboard = ArenaScoreboard.Load(ArenaScoreboard.DefaultFilePath)
        };

        public string Name { get { return "Arena Survival"; } }
        public IGameField CurrentField { get { return _field; } }
        public bool MovesPossible { get { return _field.Player.IsAlive; } }

        public void DoMove(IPlayMove move)
        {
            // Arena movement is continuous and handled by the active player steering.
        }

        // Program start continues the run saved last (automatic saving keeps it current);
        // a new run is started from the save game menu.
        public void ClearField()
        {
            _field.ContinueLatestOrStartNew();
        }

        public int CheckIfPLayerWon()
        {
            return -1;
        }

        public void StartedGameCall()
        {
            PainterG9 painter = OOPGamesManager.Singleton.ActivePainter as PainterG9;
            if (painter != null)
            {
                // A continued save keeps its character.
                painter.ResetAnimationProfile(_field.Player.AnimationProfileName);
            }
        }

        public void TickGameCall()
        {
            _field.Tick();
        }
    }
}
