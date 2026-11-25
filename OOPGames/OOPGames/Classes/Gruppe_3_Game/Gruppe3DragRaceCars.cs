using System;
using System.Collections.Generic;

namespace OOPGames
{
    public sealed class Cars
    {
        private const double PowerAtIdle = 0.35;
        private const double PowerCurvePeakFraction = 0.9;
        private const double OverRevRangeFraction = 0.1;
        private const double PowerAtOverRevLimit = 0.08;
        private const double OverRevSpeedFraction = 0.05;

        public Cars(
            int index,
            string name,
            string folderName,
            string carFileName,
            string dashboardFileName,
            string rpmNeedleFileName,
            string speedNeedleFileName,
            double[] gearStarts,
            double[] gearEnds,
            double[] gearRpmDrops,
            double[] accelerationByGear,
            double maxSpeed,
            double accelerationScale,
            double idleRpm,
            double maxRpm,
            double speedAtMaxAngleKmh,
            double rpmNeedleMaxAngle,
            double rpmAtMaxAngle,
            double speedNeedleMaxAngle,
            double rpmPivotX,
            double rpmPivotY,
            double speedPivotX,
            double speedPivotY,
            string turboNeedleFileName = null,
            double turboNeedleMaxAngle = 0,
            double turboAtMaxAngle = 0,
            double turboPivotX = 0,
            double turboPivotY = 0)
        {
            Index = index;
            Name = name;
            FolderName = folderName;
            CarFileName = carFileName;
            DashboardFileName = dashboardFileName;
            RpmNeedleFileName = rpmNeedleFileName;
            SpeedNeedleFileName = speedNeedleFileName;
            GearStarts = (double[])gearStarts.Clone();
            GearEnds = (double[])gearEnds.Clone();
            GearRpmDrops = (double[])gearRpmDrops.Clone();
            AccelerationByGear = (double[])accelerationByGear.Clone();
            MaxSpeed = maxSpeed;
            AccelerationScale = accelerationScale;
            IdleRpm = idleRpm;
            MaxRpm = maxRpm;
            SpeedAtMaxAngleKmh = speedAtMaxAngleKmh;
            RpmNeedleMaxAngle = rpmNeedleMaxAngle;
            RpmAtMaxAngle = rpmAtMaxAngle;
            SpeedNeedleMaxAngle = speedNeedleMaxAngle;
            RpmPivotX = rpmPivotX;
            RpmPivotY = rpmPivotY;
            SpeedPivotX = speedPivotX;
            SpeedPivotY = speedPivotY;
            TurboNeedleFileName = turboNeedleFileName;
            TurboNeedleMaxAngle = turboNeedleMaxAngle;
            TurboAtMaxAngle = turboAtMaxAngle;
            TurboPivotX = turboPivotX;
            TurboPivotY = turboPivotY;
        }

        public int Index { get; private set; }
        public string Name { get; private set; }
        public string FolderName { get; private set; }
        public string CarFileName { get; private set; }
        public string DashboardFileName { get; private set; }
        public string RpmNeedleFileName { get; private set; }
        public string SpeedNeedleFileName { get; private set; }
        public IReadOnlyList<double> GearStarts { get; private set; }
        public IReadOnlyList<double> GearEnds { get; private set; }
        public IReadOnlyList<double> GearRpmDrops { get; private set; }
        public IReadOnlyList<double> AccelerationByGear { get; private set; }
        public double MaxSpeed { get; private set; }
        public double OverRevMaxRpm
        {
            get
            {
                return MaxRpm + (MaxRpm - IdleRpm) * OverRevRangeFraction;
            }
        }
        public double OverRevMaxSpeed
        {
            get { return MaxSpeed * (1 + OverRevSpeedFraction); }
        }
        public double AccelerationScale { get; private set; }
        public double IdleRpm { get; private set; }
        public double MaxRpm { get; private set; }
        public double SpeedAtMaxAngleKmh { get; private set; }
        public double RpmNeedleMaxAngle { get; private set; }
        public double RpmAtMaxAngle { get; private set; }
        public double SpeedNeedleMaxAngle { get; private set; }
        public double RpmPivotX { get; private set; }
        public double RpmPivotY { get; private set; }
        public double SpeedPivotX { get; private set; }
        public double SpeedPivotY { get; private set; }
        public string TurboNeedleFileName { get; private set; }
        public double TurboNeedleMaxAngle { get; private set; }
        public double TurboAtMaxAngle { get; private set; }
        public double TurboPivotX { get; private set; }
        public double TurboPivotY { get; private set; }

        public int ClampGear(int gear)
        {
            return Math.Max(1, Math.Min(GearEnds.Count, gear));
        }

        public double GetGearMaximum(int gear)
        {
            return GearEnds[ClampGear(gear) - 1];
        }

        public double GetGearOverRevSpeedLimit(int gear)
        {
            return Math.Min(
                OverRevMaxSpeed,
                GetGearMaximum(gear) + (OverRevMaxSpeed - MaxSpeed));
        }

        public double GetGearRpmDrop(int lowerGear)
        {
            if (GearRpmDrops.Count == 0)
            {
                return 0;
            }

            int dropIndex = Math.Max(
                0, Math.Min(GearRpmDrops.Count - 1, lowerGear - 1));
            return GearRpmDrops[dropIndex];
        }

        public double GetRpm(double speed, int gear)
        {
            return GetRpmForGear(speed, ClampGear(gear) - 1);
        }

        public double OptimalShiftRpm
        {
            get
            {
                return IdleRpm +
                    (MaxRpm - IdleRpm) * PowerCurvePeakFraction;
            }
        }

        public double GetPowerFactor(double rpm)
        {
            if (rpm > MaxRpm)
            {
                double overRevRange = OverRevMaxRpm - MaxRpm;
                double overRevProgress = overRevRange <= 0
                    ? 1
                    : (rpm - MaxRpm) / overRevRange;
                overRevProgress = Math.Max(0, Math.Min(1, overRevProgress));
                double remainingPower = 1 - overRevProgress;
                return PowerAtOverRevLimit +
                    (1 - PowerAtOverRevLimit) *
                    remainingPower * remainingPower * remainingPower;
            }

            double rampRange = OptimalShiftRpm - IdleRpm;
            double progress = rampRange <= 0
                ? 1
                : (rpm - IdleRpm) / rampRange;
            progress = Math.Max(0, Math.Min(1, progress));
            return PowerAtIdle + (1 - PowerAtIdle) * progress;
        }

        private double GetRpmForGear(double speed, int gearIndex)
        {
            double rpmAtGearStart = gearIndex == 0
                ? IdleRpm
                : Math.Max(
                    IdleRpm,
                    MaxRpm - GearRpmDrops[gearIndex - 1]);
            if (speed > GearEnds[gearIndex])
            {
                double overRevRange = OverRevMaxSpeed - MaxSpeed;
                double overRevProgress = overRevRange <= 0
                    ? 1
                    : (speed - GearEnds[gearIndex]) / overRevRange;
                overRevProgress = Math.Max(0, Math.Min(1, overRevProgress));
                return MaxRpm +
                    overRevProgress * (OverRevMaxRpm - MaxRpm);
            }

            double gearRange = GearEnds[gearIndex] - GearStarts[gearIndex];
            double progress = gearRange <= 0
                ? 0
                : (speed - GearStarts[gearIndex]) / gearRange;
            progress = Math.Max(0, Math.Min(1, progress));
            return rpmAtGearStart + progress * (MaxRpm - rpmAtGearStart);
        }

        public double GetRpmNeedleAngle(double rpm)
        {
            double rpmRange = RpmAtMaxAngle - IdleRpm;
            double progress = rpmRange <= 0
                ? 0
                : (rpm - IdleRpm) / rpmRange;
            progress = Math.Max(0, Math.Min(1, progress));
            return progress * RpmNeedleMaxAngle;
        }

        public double GetSpeedNeedleAngle(double speed)
        {
            const double raceMaxSpeedKmh = 300;
            double displayedSpeedKmh = MaxSpeed <= 0
                ? 0
                : Math.Max(0, speed) / MaxSpeed * raceMaxSpeedKmh;
            double progress = SpeedAtMaxAngleKmh <= 0
                ? 0
                : displayedSpeedKmh / SpeedAtMaxAngleKmh;
            progress = Math.Max(0, Math.Min(1, progress));
            return progress * SpeedNeedleMaxAngle;
        }

        public double GetTurboNeedleAngle(double turboValue)
        {
            double progress = TurboAtMaxAngle <= 0
                ? 0
                : turboValue / TurboAtMaxAngle;
            progress = Math.Max(0, Math.Min(1, progress));
            return progress * TurboNeedleMaxAngle;
        }
    }

    public static class Gruppe3DragRaceCars
    {
        private const double SharedMaxSpeed = 17.0;

        private static readonly double[] GolfGearStarts =
            { 0, 4.5, 7.2, 9.8, 12.2, 14.5 };
        private static readonly double[] GolfGearEnds =
            { 4.5, 7.2, 9.8, 12.2, 14.5, 17.0 };
        private static readonly double[] GolfAcceleration =
            { 0.62, 0.50, 0.40, 0.32, 0.25, 0.20 };
        private static readonly double[] MustangGearStarts =
            { 0, 5.0, 7.8, 10.4, 13.0, 15.2 };
        private static readonly double[] MustangGearEnds =
            { 5.0, 7.8, 10.4, 13.0, 15.2, 17.0 };
        private static readonly double[] MustangAcceleration =
            { 0.72, 0.57, 0.45, 0.36, 0.28, 0.22 };
        private static readonly double[] AudiGearStarts =
            { 0, 4.8, 7.5, 10.1, 12.7, 15.0 };
        private static readonly double[] AudiGearEnds =
            { 4.8, 7.5, 10.1, 12.7, 15.0, 17.0 };
        private static readonly double[] AudiAcceleration =
            { 0.68, 0.54, 0.43, 0.34, 0.27, 0.21 };
        private static readonly double[] StandardGearRpmDrops =
            { 1500, 1250, 1000, 750, 500 };

        public static readonly IReadOnlyList<Cars> All =
            new[]
            {
                CreateCar(
                    index: 0,
                    name: "Golf",
                    folderName: "Golf",
                    carFileName: "Golf.png",
                    dashboardFileName: "Golf_Tacho_Hintergrund.png",
                    rpmNeedleFileName: "Golf_Tacho_Drehzahlnadel.png",
                    speedNeedleFileName: "Golf_Tacho_Tachonadel.png",
                    turboNeedleFileName: "Golf_Tacho_Turbonadel.png",
                    gearStarts: GolfGearStarts,
                    gearEnds: GolfGearEnds,
                    gearRpmDrops: StandardGearRpmDrops,
                    accelerationByGear: GolfAcceleration,
                    maxSpeed: SharedMaxSpeed,
                    accelerationScale: 0.340939,
                    idleRpm: 1000,
                    maxRpm: 8000,
                    rpmNeedleMaxAngle: 203,
                    rpmAtMaxAngle: 8000,
                    speedNeedleMaxAngle: 310,
                    speedAtMaxAngleKmh: 300,
                    turboNeedleMaxAngle: 405,
                    turboAtMaxAngle: 10.0,
                    rpmPivotX: 759,
                    rpmPivotY: 202,
                    speedPivotX: 263,
                    speedPivotY: 200,
                    turboPivotX: 871,
                    turboPivotY: 260),
                CreateCar(
                    index: 1,
                    name: "Ford Mustang",
                    folderName: "Ford Mustang",
                    carFileName: "FordMustang.png",
                    dashboardFileName: "FordMustang_Tacho_Hintergrund.png",
                    rpmNeedleFileName: "FordMustang_Tacho_Drehzahlnadel.png",
                    speedNeedleFileName: "FordMustang_Tacho_Tachonadel.png",
                    gearStarts: MustangGearStarts,
                    gearEnds: MustangGearEnds,
                    gearRpmDrops: StandardGearRpmDrops,
                    accelerationByGear: MustangAcceleration,
                    maxSpeed: SharedMaxSpeed,
                    accelerationScale: 0.292441,
                    idleRpm: 800,
                    maxRpm: 7600,
                    rpmNeedleMaxAngle: 180,
                    rpmAtMaxAngle: 7600,
                    speedNeedleMaxAngle: 220,
                    speedAtMaxAngleKmh: 270,
                    rpmPivotX: 212,
                    rpmPivotY: 276,
                    speedPivotX: 813,
                    speedPivotY: 277),
                CreateCar(
                    index: 2,
                    name: "Audi RS4",
                    folderName: "Audi",
                    carFileName: "AudiRS4.png",
                    dashboardFileName: "AudiRS4_Tacho_Hintergrund.png",
                    rpmNeedleFileName: "AudiRS4_Tacho_Nadel.png",
                    speedNeedleFileName: "AudiRS4_Tacho_Nadel.png",
                    gearStarts: AudiGearStarts,
                    gearEnds: AudiGearEnds,
                    gearRpmDrops: StandardGearRpmDrops,
                    accelerationByGear: AudiAcceleration,
                    maxSpeed: SharedMaxSpeed,
                    accelerationScale: 0.310554,
                    idleRpm: 900,
                    maxRpm: 7500,
                    rpmNeedleMaxAngle: 260,
                    rpmAtMaxAngle: 7500,
                    speedNeedleMaxAngle: 261,
                    speedAtMaxAngleKmh: 260,
                    rpmPivotX: 290,
                    rpmPivotY: 295,
                    speedPivotX: 737,
                    speedPivotY: 295)
            };

        public static Cars Get(int carIndex)
        {
            if (carIndex < 0 || carIndex >= All.Count)
            {
                return All[0];
            }

            return All[carIndex];
        }

        private static Cars CreateCar(
            int index,
            string name,
            string folderName,
            string carFileName,
            string dashboardFileName,
            string rpmNeedleFileName,
            string speedNeedleFileName,
            double[] gearStarts,
            double[] gearEnds,
            double[] gearRpmDrops,
            double[] accelerationByGear,
            double maxSpeed,
            double accelerationScale,
            double idleRpm,
            double maxRpm,
            double speedAtMaxAngleKmh,
            double rpmNeedleMaxAngle,
            double rpmAtMaxAngle,
            double speedNeedleMaxAngle,
            double rpmPivotX,
            double rpmPivotY,
            double speedPivotX,
            double speedPivotY,
            string turboNeedleFileName = null,
            double turboNeedleMaxAngle = 0,
            double turboAtMaxAngle = 0,
            double turboPivotX = 0,
            double turboPivotY = 0)
        {
            return new Cars(
                index: index,
                name: name,
                folderName: folderName,
                carFileName: carFileName,
                dashboardFileName: dashboardFileName,
                rpmNeedleFileName: rpmNeedleFileName,
                speedNeedleFileName: speedNeedleFileName,
                gearStarts: gearStarts,
                gearEnds: gearEnds,
                gearRpmDrops: gearRpmDrops,
                accelerationByGear: accelerationByGear,
                maxSpeed: maxSpeed,
                accelerationScale: accelerationScale,
                idleRpm: idleRpm,
                maxRpm: maxRpm,
                speedAtMaxAngleKmh: speedAtMaxAngleKmh,
                rpmNeedleMaxAngle: rpmNeedleMaxAngle,
                rpmAtMaxAngle: rpmAtMaxAngle,
                speedNeedleMaxAngle: speedNeedleMaxAngle,
                rpmPivotX: rpmPivotX,
                rpmPivotY: rpmPivotY,
                speedPivotX: speedPivotX,
                speedPivotY: speedPivotY,
                turboNeedleFileName: turboNeedleFileName,
                turboNeedleMaxAngle: turboNeedleMaxAngle,
                turboAtMaxAngle: turboAtMaxAngle,
                turboPivotX: turboPivotX,
                turboPivotY: turboPivotY);
        }
    }
}
