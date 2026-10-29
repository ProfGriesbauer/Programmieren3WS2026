using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OOPGames
{
    public interface IA5_ConnectFourPaint : IPaintGame
    {
        void PaintConnectFourField(Canvas canvas, IA5_ConnectFourField currentField);
    }

    public interface IA5_ConnectFourField : IGameField
    {
        int this[int row, int column] { get; set; }
    }

    public interface IA5_ConnectFourRules : IGameRules
    {
        IA5_ConnectFourField ConnectFourField { get; }
        void DoConnectFourMove(IA5_ConnectFourMove move);
    }

    public interface IA5_ConnectFourMove : IRowMove, IColumnMove
    {
    }

    public interface IA5_HumanConnectFourPlayer : IHumanGamePlayer
    {
        IA5_ConnectFourMove GetMove(IMoveSelection selection, IA5_ConnectFourField field);
    }

    public interface IA5_ComputerConnectFourPlayer : IComputerGamePlayer
    {
        IA5_ConnectFourMove GetMove(IA5_ConnectFourField field);
    }
}
