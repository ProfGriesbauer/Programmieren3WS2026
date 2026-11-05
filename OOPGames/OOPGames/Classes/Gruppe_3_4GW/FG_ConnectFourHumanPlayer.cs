using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public sealed class FG_ConnectFourHumanPlayer : IFG_HumanConnectFourPlayer
    {
        private int _playerNumber;
    
        public string Name { get { return "FG Mensch Vier gewinnt"; } }
        public int PlayerNumber { get { return _playerNumber; } }
    
        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }
    
        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }
    
        public IGamePlayer Clone()
        {
            FG_ConnectFourHumanPlayer clone = new FG_ConnectFourHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }
    
        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (FG_ConnectFourPaint.IsAnyAnimating)
            {
                return null;
            }
    
            I4GWFeld gameField = field as I4GWFeld;
            if (gameField == null)
            {
                return null;
            }
    
            int column = GetSelectedColumn(selection);
            if (column < 0 ||
                column >= FG_ConnectFourField.Columns ||
                gameField[0, column] != 0)
            {
                return null;
            }
    
            return new FG_ConnectFourMove(
                FG_ConnectFourBoardLogic.GetLandingRow(gameField, column),
                column,
                _playerNumber);
        }
    
        private static int GetSelectedColumn(IMoveSelection selection)
        {
            IClickSelection click = selection as IClickSelection;
            if (click != null)
            {
                IConnectFourPainter painter =
                    OOPGamesManager.Singleton.ActivePainter as IConnectFourPainter;
                int fallbackColumn =
                    (int)((click.XClickPos - FG_ConnectFourPaint.Left) /
                        FG_ConnectFourPaint.CellSize);
                int column = painter == null
                    ? fallbackColumn
                    : painter.ColumnAt(click.XClickPos, click.YClickPos);
                return column < 0 &&
                    fallbackColumn >= 0 &&
                    fallbackColumn < FG_ConnectFourField.Columns
                    ? fallbackColumn
                    : column;
            }
    
            IKeySelection keySelection = selection as IKeySelection;
            if (keySelection == null)
            {
                return -1;
            }
    
            if (keySelection.Key >= Key.D1 && keySelection.Key <= Key.D7)
            {
                return (int)keySelection.Key - (int)Key.D1;
            }
    
            if (keySelection.Key >= Key.NumPad1 && keySelection.Key <= Key.NumPad7)
            {
                return (int)keySelection.Key - (int)Key.NumPad1;
            }
    
            return -1;
        }
    
        public IFG_ConnectFourMove GetMove(
            IMoveSelection selection,
            IFG_ConnectFourField field)
        {
            return GetMove(selection, (IGameField)field) as IFG_ConnectFourMove;
        }
    
    }
}
