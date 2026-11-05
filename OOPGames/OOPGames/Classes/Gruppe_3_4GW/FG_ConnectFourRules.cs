using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

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
                gameMove.PlayerNumber > 2 ||
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
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    int player = _field[row, column];
                    if (player > 0 &&
                        (HasLine(row, column, 0, 1, player) ||
                         HasLine(row, column, 1, 0, player) ||
                         HasLine(row, column, 1, 1, player) ||
                         HasLine(row, column, 1, -1, player)))
                    {
                        return player;
                    }
                }
            }
    
            return -1;
        }
    
        private bool HasLine(int row, int column, int rowStep, int columnStep, int player)
        {
            for (int index = 1; index < 4; index++)
            {
                int nextRow = row + rowStep * index;
                int nextColumn = column + columnStep * index;
                if (nextRow < 0 ||
                    nextRow >= FG_ConnectFourField.Rows ||
                    nextColumn < 0 ||
                    nextColumn >= FG_ConnectFourField.Columns ||
                    _field[nextRow, nextColumn] != player)
                {
                    return false;
                }
            }
    
            return true;
        }
    }
}
