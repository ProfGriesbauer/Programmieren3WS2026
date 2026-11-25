using System;

namespace OOPGames
{
    internal sealed class Gruppe3DragRaceAiController
    {
        private const double ComputerFalseStartChance = 0.01;
        private const double ComputerShiftVariation = 0.05;

        private readonly Random _random;
        private bool _willFalseStart;
        private DateTime _falseStartAt;
        private DateTime _startAt;
        private double _shiftRpm;

        public Gruppe3DragRaceAiController(Random random)
        {
            _random = random;
            Reset();
        }

        public void Reset()
        {
            _willFalseStart = false;
            _falseStartAt = DateTime.MaxValue;
            _startAt = DateTime.MaxValue;
            _shiftRpm = 0;
        }

        public void ConfigureForCountdown(
            DateTime countdownStarted, int carIndex)
        {
            _willFalseStart = _random.NextDouble() < ComputerFalseStartChance;
            _falseStartAt = _willFalseStart
                ? countdownStarted.AddMilliseconds(_random.Next(700, 2700))
                : DateTime.MaxValue;
            _startAt = DateTime.MaxValue;
            _shiftRpm = GetRandomShiftRpm(carIndex);
        }

        public void ScheduleReaction(DateTime raceStartedAt)
        {
            _startAt = raceStartedAt.AddMilliseconds(_random.Next(100, 401));
        }

        public bool TryGetCountdownCommand(
            DateTime now,
            int currentGear,
            out Gruppe3DragRaceControlCommand command)
        {
            if (_willFalseStart && now >= _falseStartAt)
            {
                command = new Gruppe3DragRaceControlCommand(
                    accelerate: true,
                    brake: false,
                    shiftUp: currentGear == 0,
                    shiftDown: false);
                return true;
            }

            command = default(Gruppe3DragRaceControlCommand);
            return false;
        }

        public Gruppe3DragRaceControlCommand GetRaceCommand(
            DateTime now,
            int currentGear,
            double currentRpm,
            int carIndex,
            out bool launchedNow)
        {
            launchedNow = false;
            if (now < _startAt)
            {
                return new Gruppe3DragRaceControlCommand(
                    accelerate: true,
                    brake: false,
                    shiftUp: false,
                    shiftDown: false);
            }

            bool shiftUp = false;
            if (currentGear == 0)
            {
                shiftUp = true;
                launchedNow = true;
            }
            else
            {
                Cars car = Gruppe3DragRaceCars.Get(carIndex);
                if (currentRpm >= _shiftRpm &&
                    currentGear < car.GearEnds.Count)
                {
                    shiftUp = true;
                    _shiftRpm = GetRandomShiftRpm(carIndex);
                }
            }

            return new Gruppe3DragRaceControlCommand(
                accelerate: true,
                brake: false,
                shiftUp: shiftUp,
                shiftDown: false);
        }

        private double GetRandomShiftRpm(int carIndex)
        {
            Cars car = Gruppe3DragRaceCars.Get(carIndex);
            double variation =
                (car.MaxRpm - car.IdleRpm) * ComputerShiftVariation;
            return car.OptimalShiftRpm +
                (_random.NextDouble() * 2 - 1) * variation;
        }
    }
}
