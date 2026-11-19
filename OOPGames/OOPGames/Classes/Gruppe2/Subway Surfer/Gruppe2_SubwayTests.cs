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
                        List<double> carStarts = field.MutableObstacles
                            .Where(item => item.Lane == ramp.Lane
                                && Gruppe2_SubwayTrainLayout.IsCar(item.Kind)
                                && item.Distance >= ramp.Distance - 0.001)
                            .Select(item => item.Distance)
                            .OrderBy(distance => distance)
                            .ToList();
                        AssertTrue(carStarts.Count >= 2,
                            "a ramp is followed by at least one wagon and the locomotive");
                        AssertTrue(Math.Abs(carStarts[0] - ramp.Distance) < 0.001,
                            "the first car starts exactly at the top of the ramp");
                        AssertTrue(carStarts.Zip(carStarts.Skip(1), (a, b) => b - a)
                                .All(gap => Math.Abs(gap - Gruppe2_SubwayTrainLayout.CarSpacing) < 0.001),
                            "the cars behind a ramp form one continuous walkable train");
                        checkedRamps++;
                    }
                }
            }

            AssertTrue(checkedRamps > 0, "train convoys with ramps are spawned");
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
            AssertTrue(field.GuardGap < 100 && !field.GameOver,
                "bumping into the train side only brings the guard closer");
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
            AssertTrue(field.Speed > 1,
                "speed increases as the score grows");

            rules.DoMove(new Gruppe2_SubwayMove(Key.Right, 1));
            rules.ClearField();

            AssertTrue(field.Lane == 1 && field.Level == Gruppe2_SubwayLevel.Track,
                "starting a new run resets the runner position");
            AssertTrue(field.Distance == 0 && field.Score == 0 && field.Coins == 0 && field.Speed == 1,
                "starting a new run resets progression");
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
                Gruppe2_SubwayPainter painter = new Gruppe2_SubwayPainter();
                painter.PaintGameField(canvas, rules.CurrentField);

                AssertTrue(canvas.Background is SolidColorBrush
                    && ((SolidColorBrush)canvas.Background).Color == Colors.LightSkyBlue,
                    "the scene paints a sky background");

                Viewport3D viewport = canvas.Children.OfType<Viewport3D>().FirstOrDefault();
                AssertTrue(viewport != null && viewport.Camera is PerspectiveCamera,
                    "the scene uses a perspective 3D camera");
                AssertTrue(viewport.Children.OfType<ModelVisual3D>()
                    .Any(visual => visual.Content is Model3DGroup),
                    "the 3D scene contains rendered world models");

                string[] labels = canvas.Children.OfType<TextBlock>()
                    .Select(block => block.Text).ToArray();
                AssertTrue(labels.Any(text => text.StartsWith("TEMPO  ", StringComparison.Ordinal))
                    && labels.Any(text => text.StartsWith("SCORE  0", StringComparison.Ordinal))
                    && labels.Any(text => text.Contains("M\u00dcNZEN  0", StringComparison.Ordinal)),
                    "the HUD displays speed, score, and coins");
                AssertTrue(labels.Any(text => text.Contains("A/D Spur wechseln", StringComparison.Ordinal))
                    && labels.Any(text => text == "TRAIN RUN"),
                    "the HUD displays controls and the game title");

                int paintedChildCount = canvas.Children.Count;
                painter.TickPaintGameField(canvas, rules.CurrentField);
                AssertTrue(canvas.Children.Count == paintedChildCount,
                    "redrawing the scene does not accumulate canvas elements");
            }
            finally
            {
                window.Close();
            }
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
        }

        private static void TestWaveSpawningKeepsOneSafeLane()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            rules.ClearField();
            for (int tick = 0; tick < 90; tick++)
            {
                rules.TickGameCall();
                if (field.MutableObstacles.Count > 0)
                {
                    break;
                }
            }

            AssertTrue(field.MutableObstacles.Count > 0,
                "waves spawn after the configured distance threshold");
            // Signals stand between two tracks and block no lane, so they are not counted.
            AssertTrue(field.MutableObstacles
                    .Where(item => item.Kind != Gruppe2_SubwayObstacleKind.Signal)
                    .Select(item => item.Lane).Distinct().Count() < 3,
                "spawn logic always leaves at least one lane open");
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
