using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    /// <summary>The three height levels coins can lie on and the runner can move on.</summary>
    internal enum Gruppe2_TrainRunHeightLevel
    {
        Ground,
        Roof,
        Flight
    }

    /// <summary>
    /// Shared geometry of a train convoy (ramp, cargo wagons, locomotive).
    /// Rules and painter use the same values, so what is drawn is exactly what collides.
    /// A car obstacle stores the distance of its near end; the car reaches CarSpacing further away.
    /// A ramp stores the distance of its top edge, where the first car starts.
    /// </summary>
    internal static class Gruppe2_TrainRunLayout
    {
        public const double CollisionDistance = 0.5;
        public const double CarSpacing = 25;
        public const double CarLength = 24.2;
        public const double RampLength = 30;
        public const double RemoveBehindDistance = -10;

        /// <summary>
        /// Train roofs are higher than the ground, so they stay visible longer below the runner.
        /// Behind this distance a train part is off screen and gets removed.
        /// </summary>
        public const double TrainRemoveBehindDistance = -20;

        public static bool IsTrainPart(Gruppe2_TrainRunObstacleKind kind)
        {
            return kind == Gruppe2_TrainRunObstacleKind.CargoRamp || IsCar(kind);
        }

        public static bool IsCar(Gruppe2_TrainRunObstacleKind kind)
        {
            return kind == Gruppe2_TrainRunObstacleKind.Train
                || kind == Gruppe2_TrainRunObstacleKind.CargoWagon;
        }

        public static double NearEnd(Gruppe2_TrainRunObstacle obstacle)
        {
            return obstacle.Kind == Gruppe2_TrainRunObstacleKind.CargoRamp
                ? obstacle.Distance - RampLength
                : obstacle.Distance;
        }

        public static double FarEnd(Gruppe2_TrainRunObstacle obstacle)
        {
            return IsCar(obstacle.Kind) ? obstacle.Distance + CarLength : obstacle.Distance;
        }

        /// <summary>
        /// Distance from which the magnet pulls coins of the own and the neighbouring lanes
        /// towards the runner (used by the rules for collecting and by the painter for the pull).
        /// </summary>
        public const double MagnetRange = 30;

        /// <summary>Height level a coin lies on.</summary>
        public static Gruppe2_TrainRunHeightLevel LevelOf(Gruppe2_TrainRunReward reward)
        {
            return reward.Elevated ? Gruppe2_TrainRunHeightLevel.Flight
                : reward.OnRoof ? Gruppe2_TrainRunHeightLevel.Roof
                : Gruppe2_TrainRunHeightLevel.Ground;
        }

        /// <summary>Height level the runner is on: flying high with the jetpack, on a train roof or below.</summary>
        public static Gruppe2_TrainRunHeightLevel LevelOf(IGruppe2_TrainRunField field)
        {
            return field.JetpackHeight > 0.5 ? Gruppe2_TrainRunHeightLevel.Flight
                : field.Level == Gruppe2_TrainRunLevel.TrainRoof ? Gruppe2_TrainRunHeightLevel.Roof
                : Gruppe2_TrainRunHeightLevel.Ground;
        }

        /// <summary>Jetpack flight level, measured in train roof heights.</summary>
        public const double JetpackAltitudeInRoofHeights = 2.0;

        /// <summary>Ticks to climb up to the flight level (and to sink back down at the end).</summary>
        public const int JetpackClimbTicks = 30;

        public static bool IsBehindRunner(Gruppe2_TrainRunObstacle obstacle)
        {
            double limit = IsTrainPart(obstacle.Kind) ? TrainRemoveBehindDistance : RemoveBehindDistance;
            return FarEnd(obstacle) < limit;
        }

        /// <summary>True while a car of the given lane is directly below the runner.</summary>
        public static bool IsCarUnderRunner(IEnumerable<Gruppe2_TrainRunObstacle> obstacles, int lane)
        {
            return obstacles.Any(obstacle => obstacle.Lane == lane
                && IsCar(obstacle.Kind)
                && obstacle.Distance <= CollisionDistance
                && obstacle.Distance + CarSpacing > 0);
        }

        /// <summary>
        /// How far the runner has walked up a ramp in the given lane:
        /// 0 at the foot of the ramp (or no ramp), 1 at its top edge.
        /// </summary>
        public static double RampProgressUnderRunner(IEnumerable<Gruppe2_TrainRunObstacle> obstacles, int lane)
        {
            Gruppe2_TrainRunObstacle ramp = obstacles.FirstOrDefault(obstacle => obstacle.Lane == lane
                && obstacle.Kind == Gruppe2_TrainRunObstacleKind.CargoRamp
                && NearEnd(obstacle) <= 0
                && obstacle.Distance > CollisionDistance);
            return ramp == null ? 0 : Math.Min(1, -NearEnd(ramp) / RampLength);
        }

        /// <summary>True if a train part (ramp, wagon, locomotive) of the lane reaches into [from, to].</summary>
        public static bool IsLaneOccupiedByTrain(
            IEnumerable<Gruppe2_TrainRunObstacle> obstacles, int lane, double from, double to)
        {
            return obstacles.Any(obstacle => obstacle.Lane == lane
                && IsTrainPart(obstacle.Kind)
                && NearEnd(obstacle) < to
                && FarEnd(obstacle) > from);
        }

        /// <summary>True while any part of a convoy in the given lane has not yet passed the runner.</summary>
        public static bool HasTrainAhead(IEnumerable<Gruppe2_TrainRunObstacle> obstacles, int lane)
        {
            return obstacles.Any(obstacle => obstacle.Lane == lane
                && IsTrainPart(obstacle.Kind)
                && obstacle.Distance + CarSpacing > 0);
        }
    }
}
