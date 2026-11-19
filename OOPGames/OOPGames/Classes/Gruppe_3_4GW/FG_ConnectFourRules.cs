namespace OOPGames
{
    public sealed class FG_ConnectFourRules : IFG_ConnectFourRules
    {
        private readonly FG_ConnectFourField _field = new FG_ConnectFourField();

        public string Name { get { return "FG Vier gewinnt Regeln"; } }
        public IGameField CurrentField { get { return _field; } }
        public IFG_ConnectFourField ConnectFourField { get { return _field; } }

        public bool MovesPossible
        {
            get
            {
                if (CheckIfPLayerWon() > 0)
                {
                    return false;
                }

                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    if (_field[0, column] == 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void ClearField()
        {
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    _field[row, column] = 0;
                }
            }
        }

        public void DoMove(IPlayMove move)
        {
            I4GWMove gameMove = move as I4GWMove;
            if (gameMove == null)
            {
                return;
            }

            if (gameMove.PlayerNumber < 1 ||
                gameMove.PlayerNumber > FG_ConnectFourBoardLogic.PlayerCount ||
                gameMove.Column < 0 ||
                gameMove.Column >= FG_ConnectFourField.Columns ||
                CheckIfPLayerWon() > 0)
            {
                return;
            }

            for (int row = FG_ConnectFourField.Rows - 1; row >= 0; row--)
            {
                if (_field[row, gameMove.Column] == 0)
                {
                    _field[row, gameMove.Column] = gameMove.PlayerNumber;
                    return;
                }
            }
        }

        public void DoConnectFourMove(IFG_ConnectFourMove move)
        {
            DoMove(move);
        }

        public int CheckIfPLayerWon()
        {
            return FG_ConnectFourBoardLogic.FindWinner(_field);
        }
    }
}
