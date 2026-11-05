namespace OOPGames
{
    public sealed class Gruppe1_ConnectFourRules : IGameRules
    {
        private Gruppe1_ConnectFourField _field = new Gruppe1_ConnectFourField();
        private int _nextPlayer = 1;

        public string Name { get { return "Gruppe 1 Connect Four Rules"; } }

        public IGameField CurrentField { get { return _field; } }

        public bool MovesPossible
        {
            get
            {
                return G1_ConnectFourLogic.Winner(_field) < 0
                    && !G1_ConnectFourLogic.IsFull(_field);
            }
        }

        public void ClearField()
        {
            _field = new Gruppe1_ConnectFourField();
            _nextPlayer = 1;
        }

        public void DoMove(IPlayMove move)
        {
            I4GWMove connectFourMove = move as I4GWMove;
            if (connectFourMove == null
                || connectFourMove.PlayerNumber != _nextPlayer
                || connectFourMove.PlayerNumber < 1
                || connectFourMove.PlayerNumber > 2
                || !MovesPossible)
            {
                return;
            }

            int row = G1_ConnectFourLogic.FreeRow(_field, connectFourMove.Column);
            if (row < 0)
            {
                return;
            }

            _field[row, connectFourMove.Column] = connectFourMove.PlayerNumber;
            _nextPlayer = _nextPlayer == 1 ? 2 : 1;
        }

        public int CheckIfPLayerWon()
        {
            return G1_ConnectFourLogic.Winner(_field);
        }
    }
}
