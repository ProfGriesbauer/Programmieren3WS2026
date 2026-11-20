using System;

namespace OOPGames
{
    /// <summary>
    /// Smooth lane change of the runner (purely visual; the rules switch lanes instantly).
    /// Over a few frames the runner slides to the new lane with ease-in/ease-out and
    /// lifts off in a small arc. The body stays upright the whole time.
    /// The camera follows <see cref="X"/>, so the runner is always seen from behind.
    /// </summary>
    internal sealed class Gruppe2_TrainRunLaneChange
    {
        /// <summary>Frames per lane change (the framework repaints every 40 ms, so about 0.3 s).</summary>
        public const int DurationFrames = 7;
        private const double ArcHeight = 0.3;

        private double _fromX;
        private double _toX;
        private int _frame = DurationFrames;

        /// <summary>Current x position of the runner.</summary>
        public double X
        {
            get { return _fromX + (_toX - _fromX) * Ease(Progress); }
        }

        /// <summary>Extra height of the small arc during the lane change.</summary>
        public double Hop
        {
            get { return ArcHeight * Math.Sin(Math.PI * Progress); }
        }

        private double Progress
        {
            get { return Math.Min(1, _frame / (double)DurationFrames); }
        }

        /// <summary>Places the runner on the lane without animation (new game).</summary>
        public void Reset(double laneX)
        {
            _fromX = laneX;
            _toX = laneX;
            _frame = DurationFrames;
        }

        /// <summary>Advances the animation by one frame; starts a new one if the target lane changed.</summary>
        public void Update(double targetLaneX)
        {
            if (targetLaneX != _toX)
            {
                // Start from the current position, so a quick double lane change stays smooth.
                _fromX = X;
                _toX = targetLaneX;
                _frame = 0;
            }

            if (_frame < DurationFrames)
            {
                _frame++;
            }
        }

        private static double Ease(double t)
        {
            return t * t * (3 - 2 * t);
        }
    }
}
