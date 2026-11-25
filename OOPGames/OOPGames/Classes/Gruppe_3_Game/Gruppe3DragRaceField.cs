using System;

namespace OOPGames
{
    public sealed class Gruppe3DragRaceField : IGruppe3DragRaceField
    {
        internal const double TrackDistanceScale = 4;
        internal const double FinishDistance = 1850 * TrackDistanceScale;

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
        public double PlayerOneSpeed { get; internal set; }
        public double PlayerTwoSpeed { get; internal set; }
        public int PlayerOneGear { get; internal set; }
        public int PlayerTwoGear { get; internal set; }
        public double PlayerOneRpm { get; internal set; }
        public double PlayerTwoRpm { get; internal set; }
        public double? PlayerOneRaceTimeSeconds { get; internal set; }
        public double? PlayerTwoRaceTimeSeconds { get; internal set; }
        public double? PlayerOneFinishSpeedKmh { get; internal set; }
        public double? PlayerTwoFinishSpeedKmh { get; internal set; }
        public double? PlayerOneReactionTimeMs { get; internal set; }
        public double? PlayerTwoReactionTimeMs { get; internal set; }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is IGruppe3DragRacePainter;
        }
    }
}
