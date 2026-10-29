using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace OOPGames
{
    public interface IFG_ConnectFourPaint : IPaintGame
    {
        void PaintConnectFourField(Canvas canvas, IFG_ConnectFourField currentField);
    }

    public interface IFG_ConnectFourField : I4GWFeld
    {
    }

    public interface IFG_ConnectFourRules : IGameRules
    {
        IFG_ConnectFourField ConnectFourField { get; }
        void DoConnectFourMove(IFG_ConnectFourMove move);
    }

    public interface IFG_ConnectFourMove : I4GWMove
    {
    }

    public interface IFG_HumanConnectFourPlayer : IHumanGamePlayer
    {
        IFG_ConnectFourMove GetMove(IMoveSelection selection, IFG_ConnectFourField field);
    }

    public interface IFG_ComputerConnectFourPlayer : IComputerGamePlayer
    {
        IFG_ConnectFourMove GetMove(IFG_ConnectFourField field);
    }
}
