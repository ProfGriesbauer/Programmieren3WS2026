using System.Windows.Controls;

namespace OOPGames
{
    public interface IG9_4GewinntPainter : IPaintGame
    {
        void Paint4GewinntField(Canvas canvas, I4GWFeld currentField);
    }

    public interface IG9_4GewinntRules : IGameRules
    {
        I4GWFeld VierGewinntField { get; }
        void Do4GewinntMove(I4GWMove move);
    }

    public interface IG9_Human4GewinntPlayer : IHumanGamePlayer
    {
        I4GWMove GetMove(IMoveSelection selection, I4GWFeld field);
    }

    public interface IG9_Computer4GewinntPlayer : IComputerGamePlayer
    {
        I4GWMove GetMove(I4GWFeld field);
    }
}
