using System;

namespace OOPGames
{
    public sealed class HayDayRules : IGameRules2
    {
        private HayDayField _field = HayDayField.LoadOrCreate();
        private int _nextPlayer = 1;
        private bool _prepared;

        public string Name { get { return "Hay Day - Bauernhof"; } }
        public IGameField CurrentField { get { return _field; } }
        public bool MovesPossible { get { return true; } }

        public void StartedGameCall()
        {
            _field = HayDayField.LoadOrCreate();
            _prepared = true;
        }

        public void TickGameCall()
        {
            _field.AdvanceTime(DateTime.UtcNow);
        }

        public void ClearField()
        {
            if (!_prepared)
            {
                _field = HayDayField.LoadOrCreate();
            }

            _prepared = false;
            _field.ResetCamera();
            _nextPlayer = 1;
        }

        public void DoMove(IPlayMove move)
        {
            HayDayMove farmMove = move as HayDayMove;
            if (farmMove == null || farmMove.PlayerNumber != _nextPlayer || !MovesPossible)
            {
                return;
            }

            _field.Apply(farmMove);
            _field.Save();
            _nextPlayer = _nextPlayer == 1 ? 2 : 1;
        }

        public int CheckIfPLayerWon()
        {
            return -1;
        }
    }
}
