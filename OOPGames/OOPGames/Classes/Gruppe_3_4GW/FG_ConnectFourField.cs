using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    internal static class FG_ConnectFourBoardLogic
    {
        internal static int GetLandingRow(I4GWFeld field, int column)
        {
            if (column < 0 || column >= FG_ConnectFourField.Columns)
            {
                return -1;
            }
    
            for (int row = FG_ConnectFourField.Rows - 1; row >= 0; row--)
            {
                if (field[row, column] == 0)
                {
                    return row;
                }
            }
    
            return -1;
        }
    }
    
    public sealed class FG_ConnectFourField : IFG_ConnectFourField
    {
        public const int Rows = 6;
        public const int Columns = 7;
        private readonly int[,] _board = new int[Rows, Columns];
    
        public int this[int row, int column]
        {
            get
            {
                if (row < 0 || row >= Rows || column < 0 || column >= Columns)
                {
                    return -1;
                }
    
                return _board[row, column];
            }
            set
            {
                if (row >= 0 && row < Rows && column >= 0 && column < Columns)
                {
                    _board[row, column] = value;
                }
            }
        }
    
        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }
    
    }
}
