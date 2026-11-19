using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    public static class Gruppe2_SubwayTests
    {
        public static void RunAllTests()
        {
            TestInitialState();
            TestGuardIntroVisibility();
            TestMovementAndLaneBounds();
            TestJumpAndRollTransitions();
            TestProgressionAndReset();
            TestJumpArc();
            TestHumanPlayerInput();
            TestPainterFocusesGameCanvas();
            TestThreeDimensionalSceneAndHud();
            TestFatalAndNonFatalObstacles();
            TestRewardCollectionAndUpgrades();
            TestWaveSpawningKeepsOneSafeLane();
            TestRampIsAlwaysFollowedByWalkableCars();
            TestRunnerStaysOnRoofUntilLastCarPassed();
            TestLaneChangeIntoTrainSideIsBlocked();
            TestSingleTrainsWithoutRampAreSpawned();
            TestRampCanBeWalkedUpWithoutJumping();
            TestBarriersNeedJumpOrRoll();
            TestTrainsWithoutRampNeedJumpStick();
            TestSpeedKeepsRisingAboveFormerLimit();
            TestJumpStickDoesNotJumpOnPickup();
            TestLaneChangeIsSmoothArc();
            TestJumpFromRoofToRoofOverGap();
            TestJetpackFlight();
            TestMagnetCollectsNeighbourLaneCoins();
        }

        private static void TestMagnetCollectsNeighbourLaneCoins()
        {
            foreach (bool magnet in new[] { true, false })
            {
                Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
                Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
                rules.ClearField();
                if (magnet)
                {
                    field.MutableUpgrades[Gruppe2_SubwayUpgradeKind.Magnet] = 200;
                }

                field.MutableRewards.Add(new Gruppe2_SubwayReward(Gruppe2_SubwayRewardKind.Coin, 0, 0.5));
                rules.TickGameCall();
                AssertTrue(field.Coins == (magnet ? 1 : 0),
                    magnet
                        ? "the magnet pulls in a coin from the neighbouring lane"
                        : "without the magnet a coin in the neighbouring lane stays where it is");
            }
        }

        private static void TestJetpackFlight()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            rules.ClearField();
            field.MutableRewards.Add(new Gruppe2_SubwayReward(
                Gruppe2_SubwayRewardKind.UpgradeBox, 1, 0.5, Gruppe2_SubwayUpgradeKind.Jetpack));
            rules.TickGameCall();
            AssertTrue(field.Level == Gruppe2_SubwayLevel.Air
                    && field.ActiveUpgrades.ContainsKey(Gruppe2_SubwayUpgradeKind.Jetpack),
                "picking up the jetpack starts a flight");
            AssertTrue(field.Rewards.Any(reward => reward.Elevated),
                "a coin trail appears at flight level");

            double previous = field.JetpackHeight;
            for (int tick = 0; tick < Gruppe2_SubwayTrainLayout.JetpackClimbTicks; tick++)
            {
                rules.TickGameCall();
                AssertTrue(field.JetpackHeight > previous || field.JetpackHeight == 1,
                    "the runner climbs steadily up to flight level");
                previous = field.JetpackHeight;
            }

            AssertTrue(Math.Abs(field.JetpackHeight - 1) < 0.001, "the runner reaches flight level");

            field.MutableRewards.Clear();
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(Gruppe2_SubwayObstacleKind.LargeBarrier, 1, 3));
            field.MutableRewards.Add(new Gruppe2_SubwayReward(Gruppe2_SubwayRewardKind.Coin, 1, 3, elevated: true));
            field.MutableRewards.Add(new Gruppe2_SubwayReward(Gruppe2_SubwayRewardKind.Coin, 1, 3));
            int coinsBefore = field.Coins;
            for (int tick = 0; tick < 6; tick++)
            {
                rules.TickGameCall();
            }

            AssertTrue(!field.GameOver, "obstacles on the track pass below the flying runner");
            AssertTrue(field.Coins == coinsBefore + 1,
                "while flying only the coin at flight level is collected, not the one on the ground");

            int remainingTicks = 0;
            bool sankBack = false;
            while (field.Level == Gruppe2_SubwayLevel.Air && remainingTicks < 400)
            {
                rules.TickGameCall();
                // Sinking may end early on a train roof (at half the flight level),
                // so any height between cruise level and the ground counts as sinking.
                sankBack |= field.JetpackHeight > 0 && field.JetpackHeight < 0.9;
                remainingTicks++;
            }

            AssertTrue(sankBack, "at the end the runner sinks back down in an arc");
            AssertTrue(field.Level != Gruppe2_SubwayLevel.Air && field.JetpackHeight == 0,
                "after the flight the runner is back on the track (or on a train roof)");
        }

        private static void TestJumpFromRoofToRoofOverGap()
        {
            AssertTrue(RoofJumpAcrossGap(true),
                "jumping on a roof carries the runner over a gap onto the next train");
            AssertTrue(!RoofJumpAcrossGap(false),
                "without jumping the runner falls into the gap between two trains");
        }

        /// <summary>Returns true if the runner ends up on the roof of the second train.</summary>
        private static bool RoofJumpAcrossGap(bool jump)
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            rules.ClearField();
            double spacing = Gruppe2_SubwayTrainLayout.CarSpacing;
            Gruppe2_SubwayObstacle firstWagon = new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.CargoWagon, 1, 1);
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.CargoRamp, 1, 1));
            field.MutableObstacles.Add(firstWagon);
            // Second train after a gap of 12 units.
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.CargoWagon, 1, 1 + spacing + 12));
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.Train, 1, 1 + 2 * spacing + 12));

            rules.TickGameCall();
            AssertTrue(field.Level == Gruppe2_SubwayLevel.TrainRoof, "the runner walks up onto the first train");

            while (firstWagon.Distance + spacing > 4)
            {
                rules.TickGameCall();
            }

            if (jump)
            {
                rules.DoMove(new Gruppe2_SubwayMove(Key.Up, 1));
                AssertTrue(field.Level == Gruppe2_SubwayLevel.TrainRoof && field.JumpTicks > 0,
                    "the runner can jump while standing on a train roof");
            }

            for (int tick = 0; tick < 35 && !field.GameOver; tick++)
            {
                rules.TickGameCall();
            }

            return !field.GameOver && field.Level == Gruppe2_SubwayLevel.TrainRoof;
        }

        private static void TestLaneChangeIsSmoothArc()
        {
            Gruppe2_SubwayLaneChange laneChange = new Gruppe2_SubwayLaneChange();
            laneChange.Reset(0);
            laneChange.Update(1.55);

            double previousX = 0;
            bool leftTheGround = false;
            for (int frame = 1; frame < Gruppe2_SubwayLaneChange.DurationFrames; frame++)
            {
                AssertTrue(laneChange.X > previousX && laneChange.X < 1.55,
                    "the runner slides step by step towards the new lane");
                leftTheGround |= laneChange.Hop > 0.1;
                previousX = laneChange.X;
                laneChange.Update(1.55);
            }

            AssertTrue(leftTheGround, "the lane change follows a small arc");
            AssertTrue(Math.Abs(laneChange.X - 1.55) < 0.001 && laneChange.Hop < 0.001,
                "after the lane change the runner stands on the new lane");
        }

        private static void TestJumpStickDoesNotJumpOnPickup()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            rules.ClearField();
            field.MutableRewards.Add(new Gruppe2_SubwayReward(
                Gruppe2_SubwayRewardKind.UpgradeBox, 1, 0.5, Gruppe2_SubwayUpgradeKind.JumpStick));
            rules.TickGameCall();

            AssertTrue(field.ActiveUpgrades.ContainsKey(Gruppe2_SubwayUpgradeKind.JumpStick),
                "the jump stick is active after picking it up");
            AssertTrue(field.Level == Gruppe2_SubwayLevel.Track && field.JumpHeight == 0,
                "picking up the jump stick does not start a jump by itself");

            rules.DoMove(new Gruppe2_SubwayMove(Key.Up, 1));
            int jumpTicks = 0;
            while (field.Level == Gruppe2_SubwayLevel.Air && jumpTicks < 100)
            {
                rules.TickGameCall();
                jumpTicks++;
            }

            AssertTrue(jumpTicks > 35,
                "the next jump with the jump stick is the long, high jump");
        }

        private static void TestSpeedKeepsRisingAboveFormerLimit()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            rules.ClearField();
            field.Distance = 3000;
            rules.TickGameCall();
            double speedAt3000 = field.Speed;
            field.Distance = 6000;
            rules.TickGameCall();

            AssertTrue(speedAt3000 > 2.1 && field.Speed > speedAt3000,
                "the speed keeps rising continuously beyond 2.1");
        }

        private static void TestTrainsWithoutRampNeedJumpStick()
        {
            foreach (Gruppe2_SubwayObstacleKind car in new[]
                { Gruppe2_SubwayObstacleKind.Train, Gruppe2_SubwayObstacleKind.CargoWagon })
            {
                AssertTrue(!JumpOntoCar(car, false),
                    "a normal jump onto a " + car + " without ramp ends the run");
                AssertTrue(JumpOntoCar(car, true),
                    "with the jump stick the runner can jump onto a " + car + " without ramp");
            }
        }

        /// <summary>Returns true if the runner reaches the roof of a lone car by jumping.</summary>
        private static bool JumpOntoCar(Gruppe2_SubwayObstacleKind kind, bool withJumpStick)
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            rules.ClearField();
            if (withJumpStick)
            {
                field.MutableUpgrades[Gruppe2_SubwayUpgradeKind.JumpStick] = 200;
            }

            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(kind, 1, 3));
            rules.DoMove(new Gruppe2_SubwayMove(Key.Up, 1));
            bool reachedRoof = false;
            for (int tick = 0; tick < 30 && !field.GameOver; tick++)
            {
                rules.TickGameCall();
                reachedRoof |= field.Level == Gruppe2_SubwayLevel.TrainRoof;
            }

            return reachedRoof && !field.GameOver;
        }

        private static void TestRampCanBeWalkedUpWithoutJumping()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            rules.ClearField();
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.CargoRamp, 1, 10));
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.CargoWagon, 1, 10));

            double halfway = Gruppe2_SubwayTrainLayout.RampProgressUnderRunner(field.Obstacles, 1);
            AssertTrue(halfway > 0 && halfway < 1,
                "the runner is halfway up the ramp before reaching its top");

            for (int tick = 0; tick < 12; tick++)
            {
                rules.TickGameCall();
            }

            AssertTrue(field.Level == Gruppe2_SubwayLevel.TrainRoof && !field.GameOver,
                "walking up the ramp without jumping leads onto the train roof");
        }

        private static void TestBarriersNeedJumpOrRoll()
        {
            AssertTrue(RunIntoObstacle(Gruppe2_SubwayObstacleKind.SmallBarrier, Key.Up),
                "jumping clears the small barrier");
            AssertTrue(!RunIntoObstacle(Gruppe2_SubwayObstacleKind.SmallBarrier, Key.None),
                "running into the small barrier ends the run");
            AssertTrue(RunIntoObstacle(Gruppe2_SubwayObstacleKind.LargeBarrier, Key.Down),
                "rolling passes under the large barrier");
            AssertTrue(!RunIntoObstacle(Gruppe2_SubwayObstacleKind.LargeBarrier, Key.Up),
                "jumping does not clear the large barrier");
        }

        /// <summary>Returns true if the runner survives the obstacle after pressing the key.</summary>
        private static bool RunIntoObstacle(Gruppe2_SubwayObstacleKind kind, Key key)
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            rules.ClearField();
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(kind, 1, 3));
            if (key != Key.None)
            {
                rules.DoMove(new Gruppe2_SubwayMove(key, 1));
            }

            for (int tick = 0; tick < 5; tick++)
            {
                rules.TickGameCall();
            }

            return !field.GameOver;
        }

        private static void TestSingleTrainsWithoutRampAreSpawned()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            bool singleTrainFound = false;

            for (int run = 0; run < 200 && !singleTrainFound; run++)
            {
                rules.ClearField();
                for (int tick = 0; tick < 400 && !field.GameOver && !singleTrainFound; tick++)
                {
                    rules.TickGameCall();
                    singleTrainFound = field.MutableObstacles.Any(train =>
                        train.Kind == Gruppe2_SubwayObstacleKind.Train
                        && !field.MutableObstacles.Any(ramp =>
                            ramp.Kind == Gruppe2_SubwayObstacleKind.CargoRamp && ramp.Lane == train.Lane));
                }
            }

            AssertTrue(singleTrainFound, "single trains without a ramp still appear and must be dodged");
        }

        private static void TestRampIsAlwaysFollowedByWalkableCars()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            int checkedRamps = 0;

            for (int run = 0; run < 200 && checkedRamps < 5; run++)
            {
                rules.ClearField();
                for (int tick = 0; tick < 400 && !field.GameOver; tick++)
                {
                    rules.TickGameCall();
                    foreach (Gruppe2_SubwayObstacle ramp in field.MutableObstacles
                        .Where(item => item.Kind == Gruppe2_SubwayObstacleKind.CargoRamp && item.Distance > 100))
                    {
                        // Follow the cars that are coupled directly behind the ramp
                        // (another train may stand further back in the same lane).
                        List<Gruppe2_SubwayObstacle> convoy = new List<Gruppe2_SubwayObstacle>();
                        Gruppe2_SubwayObstacle next = CarStartingAt(field, ramp.Lane, ramp.Distance);
                        while (next != null)
                        {
                            convoy.Add(next);
                            next = next.Kind == Gruppe2_SubwayObstacleKind.Train
                                ? null
                                : CarStartingAt(field, ramp.Lane, next.Distance + Gruppe2_SubwayTrainLayout.CarSpacing);
                        }

                        AssertTrue(convoy.Count >= 2
                                && convoy.Last().Kind == Gruppe2_SubwayObstacleKind.Train
                                && convoy.Take(convoy.Count - 1).All(car => car.Kind == Gruppe2_SubwayObstacleKind.CargoWagon),
                            "a ramp is followed by a continuous train of wagons ending with a locomotive");
                        checkedRamps++;
                    }
                }
            }

            AssertTrue(checkedRamps > 0, "train convoys with ramps are spawned");
        }

        private static Gruppe2_SubwayObstacle CarStartingAt(Gruppe2_SubwayField field, int lane, double distance)
        {
            return field.MutableObstacles.FirstOrDefault(item => item.Lane == lane
                && Gruppe2_SubwayTrainLayout.IsCar(item.Kind)
                && Math.Abs(item.Distance - distance) < 0.001);
        }

        private static void TestRunnerStaysOnRoofUntilLastCarPassed()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            rules.ClearField();
            double spacing = Gruppe2_SubwayTrainLayout.CarSpacing;
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.CargoRamp, 1, 1));
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.CargoWagon, 1, 1));
            Gruppe2_SubwayObstacle locomotive = new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.Train, 1, 1 + spacing);
            field.MutableObstacles.Add(locomotive);

            rules.DoMove(new Gruppe2_SubwayMove(Key.Up, 1));
            rules.TickGameCall();
            AssertTrue(field.Level == Gruppe2_SubwayLevel.TrainRoof,
                "jumping onto the ramp puts the runner on the roof");

            while (locomotive.Distance + spacing > 2 * field.Speed)
            {
                rules.TickGameCall();
                AssertTrue(field.Level == Gruppe2_SubwayLevel.TrainRoof && !field.GameOver,
                    "the runner keeps walking on wagon and locomotive roofs");
            }

            for (int tick = 0; tick < 5; tick++)
            {
                rules.TickGameCall();
            }

            AssertTrue(field.Level == Gruppe2_SubwayLevel.Air && field.DropHeight > 0.5,
                "after the last car the runner drops off the roof in an arc");

            double highestPoint = field.DropHeight;
            int dropTicks = 0;
            while (field.Level == Gruppe2_SubwayLevel.Air && dropTicks < 40)
            {
                rules.TickGameCall();
                highestPoint = Math.Max(highestPoint, field.DropHeight);
                dropTicks++;
            }

            AssertTrue(highestPoint > 1,
                "the drop starts with a small hop above the roof");
            AssertTrue(field.Level == Gruppe2_SubwayLevel.Track && field.DropHeight == 0,
                "the runner lands on the track after the drop");
            AssertTrue(!field.MutableObstacles.Contains(locomotive),
                "the left car is gone before the camera is back down on the track");
        }

        private static void TestLaneChangeIntoTrainSideIsBlocked()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            rules.ClearField();
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.CargoWagon, 0, -5));

            rules.DoMove(new Gruppe2_SubwayMove(Key.Left, 1));
            AssertTrue(field.Lane == 1,
                "the runner cannot move sideways into a train");
            AssertTrue(field.GameOver,
                "running sideways into a train ends the run");

            rules.ClearField();
            field.MutableUpgrades[Gruppe2_SubwayUpgradeKind.Surfboard] = 200;
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.CargoWagon, 0, -5));
            rules.DoMove(new Gruppe2_SubwayMove(Key.Left, 1));
            AssertTrue(!field.GameOver
                    && !field.ActiveUpgrades.ContainsKey(Gruppe2_SubwayUpgradeKind.Surfboard),
                "the surfboard saves the runner once from a side crash");
        }

        private static void TestInitialState()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            rules.ClearField();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            AssertTrue(rules.MovesPossible,
                "new Subway Surfer game accepts input");
            AssertTrue(field.Lane == 1,
                "starting lane is the middle lane");
            AssertTrue(field.Level == Gruppe2_SubwayLevel.Track,
                "runner starts on the track");
            AssertTrue(field.Speed == 1,
                "starting speed matches the design sheet");
            AssertTrue(field.GuardGap == 100,
                "guard starts at a full distance");
            AssertTrue(field.GuardVisible
                && field.GuardIntroTicks == Gruppe2_SubwayField.GuardIntroDurationTicks,
                "the Bahnrichter appears during the opening animation");
        }

        private static void TestGuardIntroVisibility()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            rules.ClearField();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            for (int tick = 0; tick < Gruppe2_SubwayField.GuardIntroDurationTicks - 1; tick++)
            {
                rules.TickGameCall();
            }

            AssertTrue(field.GuardVisible,
                "the Bahnrichter remains visible through the one-second follow and three-second fall-behind phases");
            rules.TickGameCall();
            AssertTrue(!field.GuardVisible,
                "the Bahnrichter hides after the four-second intro");

            field.GuardGap = 90;
            AssertTrue(field.GuardVisible,
                "the Bahnrichter appears again when the chase gets close");
        }

        private static void TestMovementAndLaneBounds()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            rules.ClearField();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            rules.DoMove(new Gruppe2_SubwayMove(Key.Left, 1));
            AssertTrue(field.Lane == 0,
                "left key moves runner to the left lane");

            rules.DoMove(new Gruppe2_SubwayMove(Key.Right, 1));
            rules.DoMove(new Gruppe2_SubwayMove(Key.Right, 1));
            AssertTrue(field.Lane == 2,
                "right key moves runner to the right lane");

            rules.DoMove(new Gruppe2_SubwayMove(Key.Right, 1));
            AssertTrue(field.Lane == 2,
                "lane movement stays capped at the rightmost lane");
        }

        private static void TestJumpAndRollTransitions()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            rules.ClearField();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            rules.DoMove(new Gruppe2_SubwayMove(Key.Up, 1));
            AssertTrue(field.Level == Gruppe2_SubwayLevel.Air,
                "up key triggers a jump into the air state");

            rules.DoMove(new Gruppe2_SubwayMove(Key.Down, 1));
            AssertTrue(field.Level == Gruppe2_SubwayLevel.Track,
                "down key returns the runner to the track");
        }

        private static void TestProgressionAndReset()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            rules.ClearField();
            IGruppe2_SubwayField field = (IGruppe2_SubwayField)rules.CurrentField;

            for (int tick = 0; tick < 24; tick++)
            {
                rules.TickGameCall();
            }

            AssertTrue(field.Distance > 0 && field.Score > 0,
                "distance and score advance while running");
            AssertTrue(((Gruppe2_SubwayField)field).AnimationTicks == 24,
                "the animation clock advances once per game tick");
            AssertTrue(field.Speed > 1,
                "speed increases as the score grows");

            rules.DoMove(new Gruppe2_SubwayMove(Key.Right, 1));
            rules.ClearField();

            AssertTrue(field.Lane == 1 && field.Level == Gruppe2_SubwayLevel.Track,
                "starting a new run resets the runner position");
            AssertTrue(field.Distance == 0 && field.Score == 0 && field.Coins == 0 && field.Speed == 1,
                "starting a new run resets progression");
            AssertTrue(((Gruppe2_SubwayField)field).AnimationTicks == 0,
                "starting a new run resets the animation clock");
            AssertTrue(field.GuardGap == 100 && !field.GameOver
                && field.Obstacles.Count == 0 && field.Rewards.Count == 0
                && field.ActiveUpgrades.Count == 0,
                "starting a new run clears hazards, rewards, upgrades, and guard state");
        }

        private static void TestJumpArc()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            rules.ClearField();
            IGruppe2_SubwayField field = (IGruppe2_SubwayField)rules.CurrentField;
            rules.DoMove(new Gruppe2_SubwayMove(Key.Up, 1));
            rules.TickGameCall();

            AssertTrue(field.Level == Gruppe2_SubwayLevel.Air && field.JumpHeight > 0,
                "jumping moves the runner through a visible air arc");
        }

        private static void TestHumanPlayerInput()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            rules.ClearField();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            Gruppe2_SubwayHumanPlayer player = new Gruppe2_SubwayHumanPlayer();
            player.SetPlayerNumber(1);

            AssertTrue(player.CanBeRuledBy(rules),
                "Subway Surfer human player is valid for the Subway Surfer rules");

            IPlayMove validMove = player.GetMove(new KeySelection(Key.Left), field);
            AssertTrue(validMove is IGruppe2_SubwayMove && ((IGruppe2_SubwayMove)validMove).Key == Key.Left,
                "valid keyboard input produces a Subway Surfer move");

            AssertTrue(player.GetMove(new KeySelection(Key.Space), field) == null,
                "unsupported keys are rejected");
        }

        private static void TestPainterFocusesGameCanvas()
        {
            Window window = new Window
            {
                Width = 320,
                Height = 260,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.ToolWindow
            };
            Canvas canvas = new Canvas();
            window.Content = canvas;

            try
            {
                window.Show();
                Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
                rules.ClearField();

                new Gruppe2_SubwayPainter().PaintGameField(canvas, rules.CurrentField);

                AssertTrue(canvas.Focusable,
                    "game canvas accepts keyboard focus");
                AssertTrue(Keyboard.FocusedElement == canvas,
                    "painting a new game focuses the canvas instead of the start button");
            }
            finally
            {
                window.Close();
            }
        }

        private static void TestThreeDimensionalSceneAndHud()
        {
            Window window = new Window
            {
                Width = 640,
                Height = 480,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.ToolWindow
            };
            Canvas canvas = new Canvas();
            window.Content = canvas;

            try
            {
                window.Show();
                Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
                rules.ClearField();
                Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
                field.GuardIntroTicks = 0;
                Gruppe2_SubwayPainter painter = new Gruppe2_SubwayPainter();
                painter.PaintGameField(canvas, field);

                AssertTrue(canvas.Background is LinearGradientBrush,
                    "the scene paints a sky gradient background");

                Viewport3D viewport = canvas.Children.OfType<Viewport3D>().FirstOrDefault();
                AssertTrue(viewport != null && viewport.Camera is PerspectiveCamera,
                    "the scene uses a perspective 3D camera");
                AssertTrue(viewport.Children.OfType<ModelVisual3D>()
                    .Any(visual => visual.Content is Model3DGroup),
                    "the 3D scene contains rendered world models");

                field.MutableRewards.Add(new Gruppe2_SubwayReward(
                    Gruppe2_SubwayRewardKind.UpgradeBox, 1, 25,
                    Gruppe2_SubwayUpgradeKind.Surfboard));
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                ModelVisual3D upgradeCoin = GetPinkUpgradeCoinVisual(viewport);
                AssertTrue(upgradeCoin != null,
                    "upgrade rewards render as large pink coins");
                Transform3DGroup upgradeCoinTransform = upgradeCoin.Transform as Transform3DGroup;
                AssertTrue(upgradeCoinTransform != null
                    && !upgradeCoinTransform.Children.OfType<RotateTransform3D>().Any(),
                    "upgrade coins do not spin");

                field.MutableRewards.Add(new Gruppe2_SubwayReward(
                    Gruppe2_SubwayRewardKind.Coin, 1, 25));
                field.AnimationTicks = 10;
                field.Speed = 1;
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                ModelVisual3D coinVisual = GetSpinningCoinVisual(viewport);
                double initialCoinAngle = GetCoinRotationAngle(coinVisual);

                field.Speed = 5;
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                coinVisual = GetSpinningCoinVisual(viewport);
                AssertTrue(Math.Abs(GetCoinRotationAngle(coinVisual) - initialCoinAngle) < 0.001,
                    "increasing player speed does not change coin spin without advancing time");

                field.AnimationTicks++;
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                coinVisual = GetSpinningCoinVisual(viewport);
                AssertTrue(Math.Abs(GetCoinRotationAngle(coinVisual) - initialCoinAngle - 11) < 0.001,
                    "coins spin at a fixed rate per game tick");
                field.Speed = 1;
                field.MutableRewards.Clear();
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();

                ModelVisual3D skaterVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                Model3DGroup skaterModel = skaterVisual.Content as Model3DGroup;
                AssertRearHair(skaterModel, true);
                Vector3D forward = GetRunnerTransform(skaterVisual).Transform(new Vector3D(0, 0, 1));
                forward.Normalize();
                AssertTrue(forward.Z < -0.99,
                    "the skater faces away from the camera toward the track");
                AssertTrue(Math.Abs(GetRunnerX(skaterVisual)) < 0.001,
                    "the skater starts centered in the middle track");
                AssertTrue(Math.Abs(GetRunnerScale(skaterVisual) - 0.55) < 0.001,
                    "the skater and skateboard are scaled down");
                AssertTrue(GetSkateboardModel(skaterModel) == null,
                    "the player starts without a skateboard");

                Rect3D runnerBounds = skaterModel.Bounds;
                AssertTrue(runnerBounds.X >= -0.55
                    && runnerBounds.X + runnerBounds.SizeX <= 0.55,
                    "the skater and skateboard fit between the track rails");

                string[] labels = canvas.Children.OfType<TextBlock>()
                    .Select(block => block.Text).ToArray();
                // HUD design: label above value, coins next to a coin icon.
                AssertTrue(labels.Contains("TEMPO")
                    && (labels.Contains("x1.0") || labels.Contains("x1,0"))
                    && labels.Contains("PUNKTE")
                    && labels.Count(text => text == "0") >= 2,
                    "the HUD displays speed, score, and coins");
                AssertTrue(labels.Any(text => text.Contains("A/D Spur wechseln", StringComparison.Ordinal))
                    && labels.Any(text => text == "TRAIN RUN"),
                    "the HUD displays controls and the game title");

                int paintedChildCount = canvas.Children.Count;
                painter.TickPaintGameField(canvas, rules.CurrentField);
                AssertTrue(canvas.Children.Count == paintedChildCount,
                    "redrawing the scene does not accumulate canvas elements");
                viewport = canvas.Children.OfType<Viewport3D>().First();
                skaterVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                AssertTrue(GetSkateboardModel(skaterVisual.Content as Model3DGroup) == null,
                    "redrawing keeps the skateboard hidden before unlocking it");

                field.Distance = Math.PI / (2 * 0.48);
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                skaterVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                skaterModel = skaterVisual.Content as Model3DGroup;
                AssertRunnerHasRunningAnimation(skaterModel);
                AssertTrue(GetRunnerHeight(skaterVisual) < 0,
                    "the runner's feet stay on the track without a skateboard");

                rules.DoMove(new Gruppe2_SubwayMove(Key.Right, 1));
                field.Distance = 0;
                // The lane change is animated smoothly; check the position once it has finished.
                for (int frame = 0; frame < Gruppe2_SubwayLaneChange.DurationFrames; frame++)
                {
                    painter.TickPaintGameField(canvas, field);
                }
                viewport = canvas.Children.OfType<Viewport3D>().First();
                skaterVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                AssertTrue(Math.Abs(GetRunnerX(skaterVisual) - 1.55) < 0.001,
                    "the skater starts centered when moving to the right track");
                AssertTrue(GetSkateboardModel(skaterVisual.Content as Model3DGroup) == null,
                    "lane changes do not unlock the skateboard");

                field.MutableUpgrades[Gruppe2_SubwayUpgradeKind.Surfboard] = 50;
                field.Distance = Math.PI / (2 * 0.32);
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                skaterVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                skaterModel = skaterVisual.Content as Model3DGroup;
                Model3DGroup skateboardModel = GetSkateboardModel(skaterModel);
                GeometryModel3D[] skateboardParts = skateboardModel == null
                    ? new GeometryModel3D[0]
                    : skateboardModel.Children.OfType<GeometryModel3D>().ToArray();
                AssertTrue(skateboardParts.Length == 8
                    && skateboardParts.Take(4)
                        .All(part => ReferenceEquals(part.Geometry, skateboardParts[0].Geometry))
                    && skateboardParts.Skip(4)
                        .All(part => ReferenceEquals(part.Geometry, skateboardParts[4].Geometry)),
                    "the skateboard has four wheels and reuses its box and sphere meshes");
                AssertTrue(skateboardModel.Bounds.SizeZ > skateboardModel.Bounds.SizeX,
                    "the skateboard's long axis follows the track direction");
                AssertTrue(ReferenceEquals(skateboardModel, GetSkateboardModel(skaterModel)),
                    "the unlocked skateboard uses the cached model");
                AssertTrue(GetRunnerX(skaterVisual) > 1.62
                    && GetRunnerX(skaterVisual) < 1.64,
                    "the skater carves to one side while riding");
                AssertTrue(Math.Abs(GetRunnerHeight(skaterVisual)) < 0.001,
                    "side-to-side skating does not add a vertical bob");
                AssertRunnerFitsTrack(skaterVisual, skaterModel, 1.55);
                AssertFeetStayPlanted(skaterModel);

                field.Distance = 3 * Math.PI / (2 * 0.32);
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                skaterVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                skaterModel = skaterVisual.Content as Model3DGroup;
                AssertTrue(ReferenceEquals(skateboardModel, GetSkateboardModel(skaterModel)),
                    "redrawing the unlocked skateboard reuses its cached model");
                AssertTrue(GetRunnerX(skaterVisual) > 1.46
                    && GetRunnerX(skaterVisual) < 1.48,
                    "the skater carves back across the lane");
                AssertRunnerFitsTrack(skaterVisual, skaterModel, 1.55);
                AssertFeetStayPlanted(skaterModel);

                field.Level = Gruppe2_SubwayLevel.TrainRoof;
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                skaterVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                AssertTrue(GetRunnerHeight(skaterVisual) > 2.4,
                    "the skater is elevated onto train roofs");

                field.MutableUpgrades.Remove(Gruppe2_SubwayUpgradeKind.Surfboard);
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                skaterVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                AssertTrue(GetSkateboardModel(skaterVisual.Content as Model3DGroup) == null,
                    "the skateboard disappears when the unlock ends");

                field.GuardGap = 90;
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                ModelVisual3D guardVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                Model3DGroup guardModel = guardVisual.Content as Model3DGroup;
                Vector3D guardForward = GetRunnerTransform(guardVisual).Transform(new Vector3D(0, 0, 1));
                guardForward.Normalize();
                AssertTrue(guardForward.Z < -0.99,
                    "the Bahnrichter faces away from the camera down the tracks");
                AssertRearHair(guardModel, false);

                field.GuardGap = 100;
                field.GuardIntroTicks = Gruppe2_SubwayField.GuardIntroDurationTicks;
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                guardVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                double introStartZ = GetRunnerZ(guardVisual);
                double introStartScale = GetRunnerScale(guardVisual);
                AssertTrue(introStartZ > 0.55,
                    "the Bahnrichter starts behind the player");
                double introStartMovement = GetRunnerAnimationMagnitude(guardVisual);
                AssertTrue(introStartMovement > 0,
                    "the Bahnrichter has an active articulated running pose on the first intro frame");

                int followTicks = Gruppe2_SubwayField.GuardIntroFollowDurationTicks;
                field.GuardIntroTicks = Gruppe2_SubwayField.GuardIntroDurationTicks - followTicks;
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                guardVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                double followEndZ = GetRunnerZ(guardVisual);
                double followEndMovement = GetRunnerAnimationMagnitude(guardVisual);
                AssertTrue(Math.Abs(followEndZ - introStartZ) < 0.001
                    && Math.Abs(followEndMovement - introStartMovement) > 0.001,
                    "the Bahnrichter keeps running behind the player during the follow phase");

                int exitElapsedTicks = Gruppe2_SubwayField.GuardIntroFallBehindDurationTicks / 2;
                field.GuardIntroTicks = Gruppe2_SubwayField.GuardIntroDurationTicks
                    - followTicks - exitElapsedTicks;
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                guardVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                double exitMiddleZ = GetRunnerZ(guardVisual);
                double exitMiddleMovement = GetRunnerAnimationMagnitude(guardVisual);
                AssertTrue(exitMiddleZ > followEndZ
                    && exitMiddleMovement > 0,
                    "the Bahnrichter starts to slow down and leave after following the player");

                field.GuardIntroTicks = 1;
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                guardVisual = viewport.Children.OfType<ModelVisual3D>().Last();
                int visualsBeforeIntroEnds = viewport.Children.OfType<ModelVisual3D>().Count();
                AssertTrue(GetRunnerZ(guardVisual) > exitMiddleZ
                    && GetRunnerZ(guardVisual) > 7
                    && Math.Abs(GetRunnerScale(guardVisual) - introStartScale) < 0.001
                    && GetRunnerAnimationMagnitude(guardVisual) < exitMiddleMovement,
                    "the Bahnrichter slows and passes the camera like an obstacle");

                field.GuardIntroTicks = 0;
                painter.TickPaintGameField(canvas, field);
                viewport = canvas.Children.OfType<Viewport3D>().First();
                AssertTrue(viewport.Children.OfType<ModelVisual3D>().Count()
                    == visualsBeforeIntroEnds - 1,
                    "the Bahnrichter leaves the scene when the intro ends");
            }
            finally
            {
                window.Close();
            }
        }

        private static Model3DGroup GetSkateboardModel(Model3DGroup skaterModel)
        {
            if (skaterModel == null)
            {
                return null;
            }

            return skaterModel.Children.OfType<Model3DGroup>()
                .FirstOrDefault(component =>
                    component.Children.OfType<GeometryModel3D>().Count() == 8);
        }

        private static Model3DGroup GetRiderModel(Model3DGroup skaterModel)
        {
            Model3DGroup skateboard = GetSkateboardModel(skaterModel);
            return skaterModel.Children.OfType<Model3DGroup>()
                .FirstOrDefault(component => !ReferenceEquals(component, skateboard));
        }

        private static void AssertRearHair(Model3DGroup character, bool hasPonytail)
        {
            Model3DGroup rider = GetRiderModel(character);
            int minimumHairParts = hasPonytail ? 3 : 2;
            double rearExtent = hasPonytail ? -0.34 : -0.30;
            AssertTrue(rider.Children.OfType<Model3DGroup>().Any(hair =>
                    hair.Children.OfType<GeometryModel3D>().Count() >= minimumHairParts
                    && hair.Bounds.Z < rearExtent),
                "hair clearly covers the back of the " + (hasPonytail ? "runner's" : "Bahnrichter's")
                + " head");
        }

        private static ModelVisual3D GetPinkUpgradeCoinVisual(Viewport3D viewport)
        {
            Color pink = Color.FromRgb(255, 72, 170);
            return viewport.Children.OfType<ModelVisual3D>().FirstOrDefault(visual =>
            {
                Model3DGroup model = visual.Content as Model3DGroup;
                Transform3DGroup transform = visual.Transform as Transform3DGroup;
                ScaleTransform3D scale = transform == null
                    ? null
                    : transform.Children.OfType<ScaleTransform3D>().FirstOrDefault();
                return scale != null && scale.ScaleX > 0.5
                    && model != null && model.Children.OfType<GeometryModel3D>().Any(part =>
                    {
                        DiffuseMaterial diffuse = part.Material as DiffuseMaterial;
                        SolidColorBrush brush = diffuse == null ? null : diffuse.Brush as SolidColorBrush;
                        return brush != null && brush.Color == pink;
                    });
            });
        }

        private static ModelVisual3D GetSpinningCoinVisual(Viewport3D viewport)
        {
            return viewport.Children.OfType<ModelVisual3D>().FirstOrDefault(visual =>
            {
                Transform3DGroup transform = visual.Transform as Transform3DGroup;
                ScaleTransform3D scale = transform == null
                    ? null
                    : transform.Children.OfType<ScaleTransform3D>().FirstOrDefault();
                return scale != null && Math.Abs(scale.ScaleX - 0.34) < 0.001
                    && transform.Children.OfType<RotateTransform3D>().Any();
            });
        }

        private static double GetCoinRotationAngle(ModelVisual3D coinVisual)
        {
            Transform3DGroup transform = coinVisual.Transform as Transform3DGroup;
            RotateTransform3D rotation = transform.Children.OfType<RotateTransform3D>().Single();
            return ((AxisAngleRotation3D)rotation.Rotation).Angle;
        }

        private static void AssertRunnerHasRunningAnimation(Model3DGroup skaterModel)
        {
            Model3DGroup rider = GetRiderModel(skaterModel);
            AssertTrue(rider != null,
                "the player model contains the runner");

            Rect3D[] feet = rider.Children.OfType<Model3DGroup>()
                .Where(part => part.Children.OfType<GeometryModel3D>().Count() == 2
                    && part.Bounds.Y < 1)
                .OrderBy(part => part.Bounds.X)
                .Select(part => part.Bounds)
                .ToArray();
            AssertTrue(feet.Length == 2,
                "the running pose contains two articulated feet");
            AssertTrue(feet[0].Z > 0 && feet[1].Z + feet[1].SizeZ < 0,
                "the feet stride in opposite directions");
            AssertTrue(feet[0].Y > feet[1].Y + 0.15,
                "one foot lifts while the other supports the run");

            Rect3D[] legSegments = rider.Children.OfType<Model3DGroup>()
                .Where(part => part.Children.OfType<GeometryModel3D>().Count() == 1
                    && part.Bounds.Y < 0.95)
                .Select(part => part.Bounds)
                .ToArray();
            AssertTrue(legSegments.Length == 4
                && legSegments.All(segment => segment.Y + segment.SizeY <= 0.91),
                "running legs stay below the shirt hem");
        }

        private static void AssertFeetStayPlanted(Model3DGroup skaterModel)
        {
            Model3DGroup rider = GetRiderModel(skaterModel);
            AssertTrue(rider != null && rider.Transform is RotateTransform3D,
                "the rider has a controlled carving lean");

            RotateTransform3D lean = (RotateTransform3D)rider.Transform;
            Point3D[] footAnchors =
            {
                new Point3D(-0.07, 0.22, 0.245),
                new Point3D(0.07, 0.22, -0.245)
            };
            AssertTrue(footAnchors.All(foot =>
                    Math.Abs(lean.Transform(foot).X - foot.X) < 0.001),
                "the feet stay planted on the skateboard while carving");
        }

        private static double GetRunnerHeight(ModelVisual3D skaterVisual)
        {
            Transform3DGroup transform = GetRunnerTransform(skaterVisual);
            TranslateTransform3D translation = transform.Children
                .OfType<TranslateTransform3D>().Single();
            return translation.OffsetY;
        }

        private static double GetRunnerX(ModelVisual3D skaterVisual)
        {
            Transform3DGroup transform = GetRunnerTransform(skaterVisual);
            return transform.Children.OfType<TranslateTransform3D>().Single().OffsetX;
        }

        private static double GetRunnerScale(ModelVisual3D skaterVisual)
        {
            Transform3DGroup transform = GetRunnerTransform(skaterVisual);
            return transform.Children.OfType<ScaleTransform3D>().First().ScaleX;
        }

        private static double GetRunnerZ(ModelVisual3D skaterVisual)
        {
            Transform3DGroup transform = GetRunnerTransform(skaterVisual);
            return transform.Children.OfType<TranslateTransform3D>().Single().OffsetZ;
        }

        private static double GetRunnerAnimationMagnitude(ModelVisual3D runnerVisual)
        {
            Model3DGroup runner = runnerVisual.Content as Model3DGroup;
            Model3DGroup rider = runner.Children.OfType<Model3DGroup>().First();
            return rider.Children.OfType<Model3DGroup>()
                .Where(part => part.Transform is RotateTransform3D)
                .Select(part => Math.Abs(((AxisAngleRotation3D)
                    ((RotateTransform3D)part.Transform).Rotation).Angle))
                .DefaultIfEmpty(0)
                .Max();
        }

        private static void AssertRunnerFitsTrack(
            ModelVisual3D skaterVisual, Model3DGroup skaterModel, double laneCenter)
        {
            Rect3D bounds = skaterModel.Bounds;
            double halfWidth = Math.Max(Math.Abs(bounds.X), Math.Abs(bounds.X + bounds.SizeX))
                * GetRunnerScale(skaterVisual);
            double centerOffset = GetRunnerX(skaterVisual) - laneCenter;
            double leftEdge = centerOffset - halfWidth;
            double rightEdge = centerOffset + halfWidth;
            AssertTrue(leftEdge >= -0.55 && rightEdge <= 0.55,
                "the skater stays within the selected track rails while carving ("
                + leftEdge + " to " + rightEdge + ")");
        }

        private static Transform3DGroup GetRunnerTransform(ModelVisual3D skaterVisual)
        {
            Transform3DGroup transform = skaterVisual.Transform as Transform3DGroup;
            AssertTrue(transform != null,
                "the skater has a scene transform");
            return transform;
        }

        private static void TestFatalAndNonFatalObstacles()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            rules.ClearField();
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.Curb, 1, 0.5));
            rules.TickGameCall();
            AssertTrue(field.GuardGap < 100 && field.GuardGap > 0,
                "light collision reduces the guard distance");
            AssertTrue(!field.GameOver,
                "light collision keeps the run alive");

            rules.ClearField();
            field = (Gruppe2_SubwayField)rules.CurrentField;
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.Train, 1, 0.5));
            rules.TickGameCall();
            AssertTrue(field.GameOver,
                "fatal collision ends the run");
            AssertTrue(rules.CheckIfPLayerWon() == -1,
                "game over is not reported as a win");
            AssertTrue(!rules.MovesPossible,
                "moves are blocked once the run ends");
        }

        private static void TestRewardCollectionAndUpgrades()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            rules.ClearField();
            field.MutableRewards.Add(new Gruppe2_SubwayReward(
                Gruppe2_SubwayRewardKind.Coin, 1, 0.5));
            rules.TickGameCall();
            AssertTrue(field.Coins == 1,
                "coin in the current lane is collected");

            rules.ClearField();
            field = (Gruppe2_SubwayField)rules.CurrentField;
            field.MutableRewards.Add(new Gruppe2_SubwayReward(
                Gruppe2_SubwayRewardKind.UpgradeBox, 1, 0.5,
                Gruppe2_SubwayUpgradeKind.Magnet));
            rules.TickGameCall();
            AssertTrue(field.ActiveUpgrades.ContainsKey(Gruppe2_SubwayUpgradeKind.Magnet),
                "upgrade box grants the expected upgrade");

            rules.ClearField();
            field = (Gruppe2_SubwayField)rules.CurrentField;
            field.MutableRewards.Add(new Gruppe2_SubwayReward(
                Gruppe2_SubwayRewardKind.UpgradeBox, 1, 0.5,
                Gruppe2_SubwayUpgradeKind.Surfboard));
            rules.TickGameCall();
            AssertTrue(field.ActiveUpgrades.TryGetValue(
                    Gruppe2_SubwayUpgradeKind.Surfboard, out int surfboardTicks)
                && surfboardTicks == 500,
                "the skateboard item unlocks the surfboard for 500 ticks");

            rules.ClearField();
            field = (Gruppe2_SubwayField)rules.CurrentField;
            field.MutableUpgrades[Gruppe2_SubwayUpgradeKind.Surfboard] = 1;
            rules.TickGameCall();
            AssertTrue(!field.ActiveUpgrades.ContainsKey(Gruppe2_SubwayUpgradeKind.Surfboard),
                "the skateboard unlock expires when its timer runs out");
        }

        private static void TestWaveSpawningKeepsOneSafeLane()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            for (int run = 0; run < 30; run++)
            {
                rules.ClearField();
                for (int tick = 0; tick < 300 && !field.GameOver; tick++)
                {
                    rules.TickGameCall();
                    AssertTrue(field.MutableObstacles.Count > 0,
                        "the track is filled with waves from the start");
                    AssertOneLaneOpenEverywhere(field);
                    AssertReactionGapBehindTrains(field);
                }
            }
        }

        /// <summary>
        /// Behind the end of a train the next object in the same lane keeps enough distance,
        /// so after dropping off the roof the runner still has time to react.
        /// Cars coupled to the same train (gap below 1) are part of the train itself.
        /// </summary>
        private static void AssertReactionGapBehindTrains(Gruppe2_SubwayField field)
        {
            foreach (Gruppe2_SubwayObstacle car in field.MutableObstacles
                .Where(item => Gruppe2_SubwayTrainLayout.IsCar(item.Kind)))
            {
                if (CarStartingAt(field, car.Lane, car.Distance + Gruppe2_SubwayTrainLayout.CarSpacing) != null)
                {
                    continue; // not the last car of its train
                }

                double trainEnd = Gruppe2_SubwayTrainLayout.FarEnd(car);
                foreach (Gruppe2_SubwayObstacle next in field.MutableObstacles
                    .Where(item => item.Lane == car.Lane && item.Kind != Gruppe2_SubwayObstacleKind.Signal))
                {
                    double gap = Gruppe2_SubwayTrainLayout.NearEnd(next) - trainEnd;
                    AssertTrue(gap <= 1 || gap >= 59,
                        "objects behind a train leave room to react after dropping off (gap " + gap + ")");
                }
            }
        }

        /// <summary>
        /// At every point of the track at most two lanes may be blocked.
        /// Signals stand between two tracks and block no lane, so they are not counted.
        /// </summary>
        private static void AssertOneLaneOpenEverywhere(Gruppe2_SubwayField field)
        {
            List<Gruppe2_SubwayObstacle> blocking = field.MutableObstacles
                .Where(item => item.Kind != Gruppe2_SubwayObstacleKind.Signal && item.Distance > 0)
                .ToList();
            for (double distance = 0; distance < 600; distance += 1)
            {
                int blockedLanes = blocking
                    .Where(item => Gruppe2_SubwayTrainLayout.NearEnd(item) <= distance + 0.5
                        && Math.Max(Gruppe2_SubwayTrainLayout.FarEnd(item), item.Distance) >= distance - 0.5)
                    .Select(item => item.Lane)
                    .Distinct()
                    .Count();
                AssertTrue(blockedLanes < 3,
                    "spawn logic always leaves at least one lane open (distance " + distance + ")");
            }
        }

        private static void AssertTrue(bool condition, string description)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Assertion failed: " + description);
            }
        }
    }
}
