namespace OOPGames
{
    public interface IG5_4GWFeld : I4GWFeld
    {
    }

    public interface IG5_4GWMove : I4GWMove
    {
    }

    public interface IG5_4GWRules : IGameRules
    {
        IG5_4GWFeld VierGewinntFeld { get; }
        bool IsDraw { get; }
    }

    public interface IG5_4GWPainter : IPaintGame
    {
    }

    public interface IG5_4GWHumanPlayer : IHumanGamePlayer
    {
    }

    public interface IG5_4GWComputerPlayer : IComputerGamePlayer
    {
    }
}
