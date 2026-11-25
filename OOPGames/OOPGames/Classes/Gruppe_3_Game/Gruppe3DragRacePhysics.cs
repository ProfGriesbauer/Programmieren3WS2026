using System;

namespace OOPGames
{
    internal static class Gruppe3DragRacePhysics
    {
            private const double RollOutDeceleration = Gruppe3DragRaceConstants.RollOutDeceleration;
            private const double BrakeDeceleration = Gruppe3DragRaceConstants.BrakeDeceleration;
            private const double CoastDeceleration = Gruppe3DragRaceConstants.CoastDeceleration;
            private const double DownshiftDeceleration = Gruppe3DragRaceConstants.DownshiftDeceleration;
            private const double NeutralRpmStepFraction = Gruppe3DragRaceConstants.NeutralRpmStepFraction;

        public static double Advance(
            double distance,
            ref double speed,
            bool accelerating,
            bool braking,
            int gear,
            int carIndex)
        {
            Cars car = Gruppe3DragRaceCars.Get(carIndex);
            if (braking)
            {
                speed = Math.Max(0, speed - BrakeDeceleration);
            }
            else if (accelerating)
            {
                if (gear <= 0)
                {
                    return Math.Max(
                        0,
                        distance + speed *
                            Gruppe3DragRaceField.TrackDistanceScale);
                }

                double rpm = car.GetRpm(speed, gear);
                if (rpm < car.OverRevMaxRpm)
                {
                    speed = Math.Min(
                        car.GetGearOverRevSpeedLimit(gear),
                        speed +
                            car.AccelerationByGear[car.ClampGear(gear) - 1] *
                            car.AccelerationScale *
                            car.GetPowerFactor(rpm));
                }
            }
            else
            {
                speed = Math.Max(0, speed - CoastDeceleration);
            }

            return Math.Max(
                0,
                distance + speed * Gruppe3DragRaceField.TrackDistanceScale);
        }

        public static double RollOut(double distance, ref double speed)
        {
            speed = Math.Max(0, speed - RollOutDeceleration);
            return distance +
                speed * Gruppe3DragRaceField.TrackDistanceScale;
        }

        public static double UpdateNeutralRpm(
            double currentRpm,
            Cars car,
            bool accelerating)
        {
            double idleRpm = car.IdleRpm;
            double rpmChange = (car.MaxRpm - idleRpm) * NeutralRpmStepFraction;
            double rpm = Math.Max(idleRpm, currentRpm);
            return accelerating
                ? Math.Min(car.MaxRpm, rpm + rpmChange)
                : Math.Max(idleRpm, rpm - rpmChange);
        }

        public static double GetCurrentRpm(
            double speed,
            int gear,
            int carIndex,
            DateTime downshiftUntil,
            DateTime now)
        {
            Cars car = Gruppe3DragRaceCars.Get(carIndex);
            double normalRpm = car.GetRpm(speed, gear);
            if (now < downshiftUntil)
            {
                return Math.Min(
                    car.MaxRpm,
                    normalRpm + car.GetGearRpmDrop(gear));
            }

            return normalRpm;
        }

        public static void ApplyDownshiftLimit(
            ref double speed,
            double limit,
            DateTime downshiftUntil,
            DateTime now)
        {
            if (now >= downshiftUntil || speed <= limit)
            {
                return;
            }

            speed = Math.Max(limit, speed - DownshiftDeceleration);
        }

        public static double GetSpeedKmh(double speed, int carIndex)
        {
            Cars car = Gruppe3DragRaceCars.Get(carIndex);
            return car.MaxSpeed <= 0
                ? 0
                : Math.Max(0, speed) / car.MaxSpeed * 300;
        }
    }
}
