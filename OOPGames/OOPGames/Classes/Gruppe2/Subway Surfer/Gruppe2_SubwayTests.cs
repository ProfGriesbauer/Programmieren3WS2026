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
                Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
                Gruppe2_SubwayPainter painter = new Gruppe2_SubwayPainter();
                painter.PaintGameField(canvas, field);

                AssertTrue(canvas.Background is SolidColorBrush
                    && ((SolidColorBrush)canvas.Background).Color == Colors.LightSkyBlue,
                    "the scene paints a sky background");

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
                painter.TickPaintGameField(canvas, field);
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
