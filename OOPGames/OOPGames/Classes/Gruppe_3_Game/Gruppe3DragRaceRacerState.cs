using System;
using System.Collections.Generic;

namespace OOPGames
{
    internal sealed class Gruppe3DragRaceRacerState
    {
        public Gruppe3DragRaceRacerState(int playerNumber, Random random)
        {
            PlayerNumber = playerNumber;
            KeyboardInput = new Gruppe3DragRaceKeyboardInput();
            AiController = new Gruppe3DragRaceAiController(random);
            Reset();
        }

        public int PlayerNumber { get; }
        public int CarIndex { get; set; }
        private double _speed;
        private DateTime _downshiftUntil;
        private double _downshiftLimit;

        private DateTime _raceStartedAt;
        public double Distance { get; private set; }
        public double Speed { get { return _speed; } }
        public int Gear { get; private set; }
        public double Rpm { get; private set; }
        public bool Accelerating { get; set; }
        public bool Braking { get; set; }
        public bool Finished { get; private set; }
        public double? RaceTimeSeconds { get; private set; }
        public double? FinishSpeedKmh { get; private set; }
        public double? ReactionTimeMs { get; set; }
        public bool IsComputer { get; private set; }
        public bool IsHuman { get; private set; }
        public Gruppe3DragRaceKeyboardInput KeyboardInput { get; }
        public Gruppe3DragRaceAiController AiController { get; }

        public void Reset()
        {
            CarIndex = -1;
            Distance = 0;
            _speed = 0;
            Gear = 0;
            Rpm = 0;
            Accelerating = false;
            Braking = false;
            Finished = false;
            RaceTimeSeconds = null;
            FinishSpeedKmh = null;
            ReactionTimeMs = null;
            IsComputer = false;
            IsHuman = false;
            _downshiftUntil = DateTime.MinValue;
            _downshiftLimit = Gruppe3DragRaceCars.Get(0).MaxSpeed;
            KeyboardInput.Reset();
            AiController.Reset();
        }

        public void ConfigurePlayerType(IEnumerable<IGamePlayer> activePlayers)
        {
            IsComputer = false;
            IsHuman = false;
            foreach (IGamePlayer player in activePlayers)
            {
                if (player.PlayerNumber == PlayerNumber)
                {
                    IsComputer = player is IComputerGamePlayer;
                    IsHuman = player is IHumanGamePlayer;
                    break;
                }
            }
        }

        public void ConfigureForCountdown(
            IEnumerable<IGamePlayer> activePlayers,
            DateTime countdownStarted)
        {
            ConfigurePlayerType(activePlayers);
            if (IsComputer)
            {
                AiController.ConfigureForCountdown(countdownStarted, CarIndex);
            }
        }

        public void OnRaceStarted(DateTime raceStartedAt)
        {
            _raceStartedAt = raceStartedAt;
            if (IsComputer)
            {
                Gear = 0;
                AiController.ScheduleReaction(raceStartedAt);
            }
            else
            {
                Gear = Math.Max(1, Gear);
            }
        }

        public void ApplyCountdownInput(DateTime now)
        {
            if (IsComputer)
            {
                Gruppe3DragRaceControlCommand aiCommand;
                if (AiController.TryGetCountdownCommand(now, Gear, out aiCommand))
                {
                    ApplyCommand(aiCommand, now);
                }
            }

            if (IsHuman)
            {
                ApplyCommand(KeyboardInput.ReadCommand(PlayerNumber), now);
            }
        }

        public void UpdateCountdownRpm()
        {
            Cars car = Gruppe3DragRaceCars.Get(CarIndex);
            Rpm = Gear == 0
                ? Gruppe3DragRacePhysics.UpdateNeutralRpm(
                    Rpm, car, Accelerating)
                : car.GetRpm(Speed, Gear);
        }

        public bool HasFalseStart()
        {
            return Gear > 0 && Accelerating;
        }

        public void ApplyFalseStart(DateTime now, double falseStartReactionMs)
        {
            Distance = Gruppe3DragRacePhysics.Advance(
                Distance,
                ref _speed,
                true,
                false,
                Gear,
                CarIndex);
            Rpm = Gruppe3DragRacePhysics.GetCurrentRpm(
                _speed, Gear, CarIndex, DateTime.MinValue, now);
            ReactionTimeMs = falseStartReactionMs;
        }

        public void RecordCurrentFinishSpeed()
        {
            FinishSpeedKmh =
                Gruppe3DragRacePhysics.GetSpeedKmh(_speed, CarIndex);
        }

        public void ApplyRaceInput(DateTime now, DateTime raceStartedAt)
        {
            if (Finished)
            {
                return;
            }

            if (IsComputer)
            {
                bool launchedNow;
                Gruppe3DragRaceControlCommand aiCommand =
                    AiController.GetRaceCommand(
                        now, Gear, Rpm, CarIndex, out launchedNow);
                ApplyCommand(aiCommand, now);
                if (launchedNow && !ReactionTimeMs.HasValue)
                {
                    ReactionTimeMs = (now - _raceStartedAt).TotalMilliseconds;
                }
            }

            if (IsHuman)
            {
                ApplyCommand(KeyboardInput.ReadCommand(PlayerNumber), now);
            }
        }

        public void StepRace(DateTime now, DateTime raceStartedAt)
        {
            if (Finished)
            {
                Distance = Gruppe3DragRacePhysics.RollOut(
                    Distance, ref _speed);
            }
            else
            {
                Gruppe3DragRacePhysics.ApplyDownshiftLimit(
                    ref _speed, _downshiftLimit, _downshiftUntil, now);
                Distance = Gruppe3DragRacePhysics.Advance(
                    Distance,
                    ref _speed,
                    Accelerating,
                    Braking,
                    Gear,
                    CarIndex);
                if (Distance >= Gruppe3DragRaceField.FinishDistance)
                {
                    Finished = true;
                    RaceTimeSeconds =
                        Math.Max(0, (now - raceStartedAt).TotalSeconds);
                    FinishSpeedKmh =
                        Gruppe3DragRacePhysics.GetSpeedKmh(_speed, CarIndex);
                }
            }

            Rpm = Gear == 0
                ? Gruppe3DragRacePhysics.UpdateNeutralRpm(
                    Rpm, Gruppe3DragRaceCars.Get(CarIndex), Accelerating)
                : Gruppe3DragRacePhysics.GetCurrentRpm(
                    _speed, Gear, CarIndex, _downshiftUntil, now);
        }

        public void RollOut(DateTime now)
        {
            Distance = Gruppe3DragRacePhysics.RollOut(
                Distance, ref _speed);
            Rpm = Gruppe3DragRacePhysics.GetCurrentRpm(
                _speed, Gear, CarIndex, DateTime.MinValue, now);
        }

        public void ApplyCommand(
            Gruppe3DragRaceControlCommand command, DateTime now)
        {
            Accelerating = command.Accelerate;
            Braking = command.Brake;

            Cars car = Gruppe3DragRaceCars.Get(CarIndex);
            if (command.ShiftUp)
            {
                if (!ReactionTimeMs.HasValue && _raceStartedAt != default(DateTime))
                {
                    ReactionTimeMs = (now - _raceStartedAt).TotalMilliseconds;
                }
                Gear = Math.Min(car.GearEnds.Count, Gear + 1);
            }

            if (command.ShiftDown && Gear > 1)
            {
                int newGear = Math.Max(1, Gear - 1);
                if (newGear != Gear)
                {
                    Gear = newGear;
                    _downshiftLimit = car.GetGearMaximum(newGear);
                    _downshiftUntil = now.AddMilliseconds(650);
                }
            }
        }

        public void SyncToField(Gruppe3DragRaceField field)
        {
            if (PlayerNumber == 1)
            {
                field.PlayerOneCar = CarIndex;
                field.PlayerOneDistance = Distance;
                field.PlayerOneSpeed = Speed;
                field.PlayerOneGear = Gear;
                field.PlayerOneRpm = Rpm;
                field.PlayerOneRaceTimeSeconds = RaceTimeSeconds;
                field.PlayerOneFinishSpeedKmh = FinishSpeedKmh;
                field.PlayerOneReactionTimeMs = ReactionTimeMs;
            }
            else
            {
                field.PlayerTwoCar = CarIndex;
                field.PlayerTwoDistance = Distance;
                field.PlayerTwoSpeed = Speed;
                field.PlayerTwoGear = Gear;
                field.PlayerTwoRpm = Rpm;
                field.PlayerTwoRaceTimeSeconds = RaceTimeSeconds;
                field.PlayerTwoFinishSpeedKmh = FinishSpeedKmh;
                field.PlayerTwoReactionTimeMs = ReactionTimeMs;
            }
        }
    }
}
