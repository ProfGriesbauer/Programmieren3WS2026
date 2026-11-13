namespace OOPGames
{
    public sealed class ArenaRules : IGameRules2
    {
        private readonly ArenaField _field = new ArenaField();

        public string Name { get { return "Arena Survival"; } }
        public IGameField CurrentField { get { return _field; } }
        public bool MovesPossible { get { return _field.Player.IsAlive; } }

        public void DoMove(IPlayMove move)
        {
            // Arena movement is continuous and handled by the active player steering.
        }

        public void ClearField()
        {
            PainterG9 painter = OOPGamesManager.Singleton.ActivePainter as PainterG9;
            if (painter != null)
            {
                painter.ResetAnimationProfile();
            }

            _field.StartNewGame();
        }

        public int CheckIfPLayerWon()
        {
            return -1;
        }

        public void StartedGameCall()
        {
            ClearField();
        }

        public void TickGameCall()
        {
            if (!_field.IsGameStarted)
            {
                return;
            }

            _field.UpdatePlayerPosition();
            _field.UpdateEnemyPositions();
        }
    }
}
