using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public sealed class FG_ConnectFourMove : IFG_ConnectFourMove
    {
        public FG_ConnectFourMove(int row, int column, int playerNumber)
        {
            Row = row;
            Column = column;
            PlayerNumber = playerNumber;
        }
    
        public int Row { get; private set; }
        public int Column { get; private set; }
        public int PlayerNumber { get; private set; }
    }
}
