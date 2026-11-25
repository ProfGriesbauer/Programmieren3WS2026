namespace OOPGames
{
    public interface IG8_VierGewinntField : I4GWFeld
    {
    }

    public interface IG8_VierGewinntMove : I4GWMove
    {
    }

    public interface IG8_VierGewinntRules : IGameRules2
    {
    }

    public interface IG8_VierGewinntPainter : IPaintGame2, IConnectFourPainter
    {
    }

    public interface IG8_VierGewinntHumanPlayer : IHumanGamePlayer
    {
    }

    public interface IG8_VierGewinntComputerPlayer : IComputerGamePlayer
    {
    }
}
