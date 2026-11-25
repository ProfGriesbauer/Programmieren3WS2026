using System;

namespace OOPGames
{
    public static class Gruppe3DragRaceConstants
    {
        // Countdown & Timing
        public const int CountdownSeconds = 3;
        public const double SelectionFeedbackDurationSeconds = 1.2;
        public const double TargetRaceDurationSeconds = 7.5;
        public const double StandardTrackLengthMeters = 400.0;

        // AI Parameters
        public const double AiReactionTimeMeanSeconds = 0.25;
        public const double AiFalseStartProbability = 0.01;

        // Physics & Deceleration
        public const double RollOutDeceleration = 0.1;
        public const double BrakeDeceleration = 0.45;
        public const double CoastDeceleration = 0.05;
        public const double DownshiftDeceleration = 0.9;
        public const double NeutralRpmStepFraction = 0.08;
    }
}