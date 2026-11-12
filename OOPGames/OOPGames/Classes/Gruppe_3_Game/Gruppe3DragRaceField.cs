using System;

namespace OOPGames
{
    public sealed class Gruppe3DragRaceField : IGruppe3DragRaceField
    {
        internal const double FinishDistance = 1600;

        public double PlayerOneDistance { get; internal set; }
        public double PlayerTwoDistance { get; internal set; }
        public bool IsRaceStarted { get; internal set; }
        public string CountdownText { get; internal set; }
        public int Winner { get; internal set; }
        public int PlayerOneCar { get; internal set; }
        public int PlayerTwoCar { get; internal set; }
        public bool CarSelectionComplete { get; internal set; }
        public string SelectionFeedbackText { get; internal set; }
        public DateTime SelectionFeedbackUntil { get; internal set; }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is IGruppe3DragRacePainter;
        }
    }
}
