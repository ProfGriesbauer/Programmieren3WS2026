using System.Windows.Controls;

namespace OOPGames
{
    public interface IGruppe3DragRaceField : IGameField
    {
        double PlayerOneDistance { get; }
        double PlayerTwoDistance { get; }
        bool IsRaceStarted { get; }
        string CountdownText { get; }
        int Winner { get; }
        int PlayerOneCar { get; }
        int PlayerTwoCar { get; }
        bool CarSelectionComplete { get; }
    }

    public interface IGruppe3DragRaceMove : IPlayMove
    {
        bool Accelerate { get; }
        bool Brake { get; }
        int CarIndex { get; }
    }

    public interface IGruppe3DragRaceRules : IGameRules2
    {
        IGruppe3DragRaceField DragRaceField { get; }
    }

    public interface IGruppe3DragRacePainter : IPaintGame2
    {
        void PaintDragRaceField(Canvas canvas, IGruppe3DragRaceField field);
        int CarAt(int x, int y);
    }

    public interface IGruppe3DragRaceHumanPlayer : IHumanGamePlayer
    {
        IGruppe3DragRaceMove GetMove(IMoveSelection selection, IGruppe3DragRaceField field);
    }

    public interface IGruppe3DragRaceComputerPlayer : IComputerGamePlayer
    {
        IGruppe3DragRaceMove GetMove(IGruppe3DragRaceField field);
    }
}
