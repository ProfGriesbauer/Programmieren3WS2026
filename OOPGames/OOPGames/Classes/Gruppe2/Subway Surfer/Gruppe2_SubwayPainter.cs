using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace OOPGames
{
    public sealed partial class Gruppe2_TrainRunPainter : IPaintGame2
    {
        private static readonly Brush SkyBrush = CreateSkyBrush();
        private static readonly Model3DGroup SceneLights = Gruppe2_TrainRunSceneLighting.CreateLights();
        private static readonly MeshGeometry3D BoxMesh = CreateBoxMesh();
        private static readonly MeshGeometry3D SphereMesh = CreateSphereMesh();
        private static readonly Material GuardOutfitMaterial = Material(Color.FromRgb(43, 74, 112));
        private static readonly Material RunnerOutfitMaterial = Material(Color.FromRgb(231, 49, 135));
        private static readonly Material SkinMaterial = Material(Color.FromRgb(245, 194, 157));
        private static readonly Material GuardHairMaterial = Material(Color.FromRgb(105, 67, 38));
        private static readonly Material RunnerHairMaterial = Material(Color.FromRgb(104, 64, 36));
        private static readonly Material PantsMaterial = Material(Color.FromRgb(35, 37, 41));
        private static readonly Material RunnerAccentMaterial = Material(Color.FromRgb(39, 193, 194));
        private static readonly Material RunnerEyeMaterial = Material(Color.FromRgb(45, 37, 52));
        private static readonly Material RunnerShoeMaterial = Material(Color.FromRgb(255, 190, 62));
        private static readonly Material SkateboardDeckMaterial = Material(Color.FromRgb(39, 193, 194));
        private static readonly Material SkateboardGraphicMaterial = Material(Color.FromRgb(231, 49, 135));
        private static readonly Material SkateboardTruckMaterial = Material(Color.FromRgb(176, 190, 197));
        private static readonly Model3DGroup SkateboardModel = CreateSkateboardModel();
        private static readonly Model3DGroup JetpackModel = CreateJetpackModel();
        private static readonly Model3DGroup JetpackFlameModel = CreateJetpackFlameModel();
        private static readonly Model3DGroup JumpStickModel = CreateJumpStickModel();

        private enum JumpStickPose
        {
            None,
            Carried,
            Jumping
        }
        private static readonly Model3DGroup CargoWagonModel = CreateCargoWagonModel();
        private static readonly Model3DGroup TrainModel = CreateTrainModel();
        private static readonly Model3DGroup CargoRampModel = CreateCargoRampModel();
        private static readonly Model3DGroup CoinModel = CreateCoinModel(
            Color.FromRgb(255, 203, 32), Color.FromRgb(179, 126, 16), glow: true);
        // Every upgrade has its own coin colour, so it can be recognised from far away.
        private static readonly Dictionary<Gruppe2_TrainRunUpgradeKind, Model3DGroup> UpgradeCoinModels =
            new Dictionary<Gruppe2_TrainRunUpgradeKind, Model3DGroup>
            {
                { Gruppe2_TrainRunUpgradeKind.Magnet, CreateUpgradeCoinModel(Color.FromRgb(222, 44, 44)) },
                { Gruppe2_TrainRunUpgradeKind.DoubleCoins, CreateUpgradeCoinModel(Color.FromRgb(255, 140, 16)) },
                { Gruppe2_TrainRunUpgradeKind.Surfboard, CreateUpgradeCoinModel(Color.FromRgb(255, 72, 170)) },
                { Gruppe2_TrainRunUpgradeKind.JumpStick, CreateUpgradeCoinModel(Color.FromRgb(58, 190, 82)) },
                { Gruppe2_TrainRunUpgradeKind.Jetpack, CreateUpgradeCoinModel(Color.FromRgb(42, 120, 232)) }
            };
        private static readonly Model3DGroup TrackModel = CreateTrackModel();
        private static readonly Model3DGroup SleeperModel = CreateSleeperModel();
        private static readonly Dictionary<Gruppe2_TrainRunObstacleKind, Model3DGroup> ObstacleModels =
            CreateObstacleModels();
        private static readonly Model3DGroup CityModel = CreateCityModel();
        private static readonly Model3DGroup BushModel = CreateBushModel();
        private static readonly Model3DGroup SignalModel = CreateSignalModel();
        private const double CityModelScale = 0.72;
        private const double CityRepeatDistance = 42;
        // Extra distance (model units) between the houses and the outer tracks,
        // so the houses never cut into the track bed and sleepers.
        private const double CityTrackClearance = 1.6;
        private const double RunnerWorldZ = 0.55;
        private const double CameraWorldZ = 7;
        private const double GuardChaseZ = 2.5;
        private const double GuardIntroEndZ = CameraWorldZ + 0.25;
        private const double GuardIntroFollowTicks = Gruppe2_TrainRunField.GuardIntroFollowDurationTicks;
        private const double WorldUnitsPerDistance = 0.1;
        // Narrower than the lane spacing (1.55), so cars in neighbouring lanes never touch.
        private const double CarWorldWidth = 1.34;
        private const double CarRoofHeight = 2.42;
        // Runner and guard are drawn smaller and slimmer so they do not hide the track.
        private const double RunnerHeightScale = 0.68;
        private const double RunnerWidthScale = 0.55;
        // Body angle while flying with the jetpack (almost lying flat in the air).
        private const double JetpackLyingDegrees = 75;

        public string Name { get { return "Gruppe 2 - Train Run Painter"; } }

        // Smooth, arc-shaped lane change of the runner; the camera follows it.
        private readonly Gruppe2_TrainRunLaneChange _laneChange = new Gruppe2_TrainRunLaneChange();

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            canvas.Focusable = true;
            _laneChange.Reset(RunnerLaneX(currentField));
            Draw(canvas, currentField, _laneChange, _scene);
            Keyboard.Focus(canvas);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            _laneChange.Update(RunnerLaneX(currentField));
            Draw(canvas, currentField, _laneChange, _scene);
        }

        /// <summary>X position of the lane the rules put the runner on.</summary>
        private static double RunnerLaneX(IGameField currentField)
        {
            IGruppe2_TrainRunField field = currentField as IGruppe2_TrainRunField;
            return field == null ? 0 : LaneWorldX(field.Lane);
        }

        private static Brush CreateSkyBrush()
        {
            LinearGradientBrush sky = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1)
            };
            sky.GradientStops.Add(new GradientStop(Color.FromRgb(62, 128, 201), 0));
            sky.GradientStops.Add(new GradientStop(Color.FromRgb(150, 198, 236), 0.35));
            sky.GradientStops.Add(new GradientStop(Color.FromRgb(226, 236, 240), 0.5));
            sky.Freeze();
            return sky;
        }

        private static Brush Brush(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private static void Draw(Canvas canvas, IGameField currentField, Gruppe2_TrainRunLaneChange laneChange,
            RetainedScene scene)
        {
            IGruppe2_TrainRunField field = currentField as IGruppe2_TrainRunField;
            if (field == null)
            {
                canvas.Children.Clear();
                return;
            }

            double width = Math.Max(320, canvas.ActualWidth);
            double height = Math.Max(260, canvas.ActualHeight);
            canvas.Background = SkyBrush;

            // The 3D viewport stays in the canvas; only the 2D overlay (HUD) is rebuilt.
            scene.AttachTo(canvas);
            DrawThreeDimensionalScene(scene, width, height, field, laneChange);
            DrawHud(canvas, width, height, field);
            if (field.GameOver)
            {
                DrawGameOver(canvas, width, height);
            }
        }

        /// <summary>
        /// Updates the retained 3D scene: static parts are only moved, game objects keep their
        /// visuals from the previous frame (see <see cref="Gruppe2_TrainRunVisualPool"/>).
        /// </summary>
        private static void DrawThreeDimensionalScene(
            RetainedScene scene, double width, double height, IGruppe2_TrainRunField field, Gruppe2_TrainRunLaneChange laneChange)
        {
            scene.Update(width, height, field, laneChange);
            Gruppe2_TrainRunVisualPool pool = scene.Pool;

            Gruppe2_TrainRunField animationField = field as Gruppe2_TrainRunField;
            int animationTicks = animationField == null ? 0 : animationField.AnimationTicks;
            foreach (Gruppe2_TrainRunObstacle obstacle in field.Obstacles)
            {
                AddThreeDimensionalObstacle(pool, obstacle);
            }

            foreach (Gruppe2_TrainRunReward reward in field.Rewards)
            {
                if (reward.Kind == Gruppe2_TrainRunRewardKind.Coin)
                {
                    AddThreeDimensionalCoin(pool, reward, field, laneChange.X, animationTicks);
                }
                else
                {
                    AddThreeDimensionalUpgradeCoin(pool, reward);
                }
            }

            ModelVisual3D runner = AddThreeDimensionalRunner(pool, field, laneChange, false, 0);
            if (field.GuardVisible)
            {
                Gruppe2_TrainRunField trainRunField = field as Gruppe2_TrainRunField;
                int guardIntroTicks = trainRunField == null ? 0 : trainRunField.GuardIntroTicks;
                AddThreeDimensionalRunner(pool, field, laneChange, true, guardIntroTicks);
            }

            // The runner's shadow falls onto the ramp or roof she stands on; all others onto the track.
            // Shadows are placed after all world objects but before the runner (and guard):
            // they stand on top of the shadows, never behind them, so transparency stays correct
            // and the runner remains the last visual of the scene.
            double runnerGround = CarRoofHeight * RunnerSupportLevel(field);
            Model3DGroup shadows = Gruppe2_TrainRunSceneLighting.CreateShadows(
                // Coins cast no shadow: barely visible, but many of them cost time in every frame.
                pool.FrameVisuals.Where(visual => visual.Content != CoinModel).ToList(),
                caster => caster == runner ? runnerGround : 0);
            pool.ShowBefore(runner, ShadowsKey, shadows, null);
            pool.Apply(scene.Viewport.Children, scene.FirstDynamicIndex);
        }

        private static readonly object ShadowsKey = new object();
        private static readonly object RunnerKey = new object();
        private static readonly object GuardKey = new object();

        private static Model3DGroup CreateTrackModel()
        {
            Material ballast = Material(Color.FromRgb(82, 78, 72));
            Material rail = Material(Color.FromRgb(183, 190, 195));
            Model3DGroup track = new Model3DGroup();

            for (int lane = 0; lane < 3; lane++)
            {
                double laneX = LaneWorldX(lane);
                AddBox(track, laneX - 0.77, -0.09, -64, laneX + 0.77, 0.01, 5, ballast);
                for (int side = -1; side <= 1; side += 2)
                {
                    double railX = laneX + side * 0.55;
                    AddBox(track, railX - 0.055, 0.01, -64, railX + 0.055, 0.15, 5, rail);
                    AddBox(track, railX - 0.08, 0.0, -64, railX + 0.08, 0.035, 5,
                        Material(Color.FromRgb(111, 77, 50)));
                }
            }

            track.Freeze();
            return track;
        }

        private static Model3DGroup CreateSleeperModel()
        {
            Model3DGroup sleeper = new Model3DGroup();
            AddBox(sleeper, -2.45, -0.01, -0.10, 2.45, 0.04, 0.10,
                Material(Color.FromRgb(105, 69, 46)));
            sleeper.Freeze();
            return sleeper;
        }

        private static void AddThreeDimensionalObstacle(
            Gruppe2_TrainRunVisualPool pool, Gruppe2_TrainRunObstacle obstacle)
        {
            double x = LaneWorldX(obstacle.Lane);
            double z = WorldZ(obstacle.Distance);
            switch (obstacle.Kind)
            {
                case Gruppe2_TrainRunObstacleKind.Train:
                    AddTrainPart(pool, TrainModel, obstacle, false);
                    break;
                case Gruppe2_TrainRunObstacleKind.CargoWagon:
                    AddTrainPart(pool, CargoWagonModel, obstacle, false);
                    break;
                case Gruppe2_TrainRunObstacleKind.CargoRamp:
                    // The ramp model rises towards +z; mirrored it rises away from the runner.
                    AddTrainPart(pool, CargoRampModel, obstacle, true);
                    break;
                case Gruppe2_TrainRunObstacleKind.Bush:
                    AddWorldModel(pool, obstacle, BushModel, x, 0.78, z, 1.05);
                    break;
                case Gruppe2_TrainRunObstacleKind.Signal:
                    AddWorldModel(pool, obstacle, SignalModel,
                        (LaneWorldX(Math.Min(obstacle.Lane, 1))
                            + LaneWorldX(Math.Min(obstacle.Lane, 1) + 1)) / 2,
                        0, z, 0.75);
                    break;
                default:
                    AddWorldModel(pool, obstacle, ObstacleModels[obstacle.Kind],
                        x, 0, z, 1);
                    break;
            }
        }

        /// <summary>
        /// Fits a train model exactly into the space the rules reserve for it:
        /// centred between the rails of its lane, standing on the track, from its near end
        /// to its far end (Gruppe2_TrainRunLayout). All cars share one roof height,
        /// so the runner can walk from wagon to locomotive.
        /// </summary>
        private static void AddTrainPart(
            Gruppe2_TrainRunVisualPool pool, Model3D model, Gruppe2_TrainRunObstacle obstacle, bool mirrorDepth)
        {
            double nearZ = WorldZ(Gruppe2_TrainRunLayout.NearEnd(obstacle));
            double farZ = WorldZ(Gruppe2_TrainRunLayout.FarEnd(obstacle));
            Rect3D bounds = model.Bounds;
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new TranslateTransform3D(
                -(bounds.X + bounds.SizeX / 2), -bounds.Y, -(bounds.Z + bounds.SizeZ / 2)));
            transform.Children.Add(new ScaleTransform3D(
                CarWorldWidth / bounds.SizeX,
                CarRoofHeight / bounds.SizeY,
                (nearZ - farZ) / bounds.SizeZ * (mirrorDepth ? -1 : 1)));
            transform.Children.Add(new TranslateTransform3D(
                LaneWorldX(obstacle.Lane), 0, (nearZ + farZ) / 2));
            pool.Show(obstacle, model, transform);
        }

        private static double WorldZ(double distance)
        {
            return RunnerWorldZ - distance * WorldUnitsPerDistance;
        }

        /// <summary>
        /// Height the runner stands on: track, rising ramp, train roof or the arc
        /// when dropping off a roof. The camera follows this height.
        /// </summary>
        private static double RunnerGroundHeight(IGruppe2_TrainRunField field)
        {
            double jetpackHeight = field.JetpackHeight * Gruppe2_TrainRunLayout.JetpackAltitudeInRoofHeights;
            return CarRoofHeight * Math.Max(Math.Max(RunnerSupportLevel(field), field.DropHeight), jetpackHeight);
        }

        /// <summary>
        /// Height of the surface below the runner in roof heights: 1 on a train roof,
        /// rising on a ramp, otherwise 0. The runner's shadow falls onto this surface.
        /// </summary>
        private static double RunnerSupportLevel(IGruppe2_TrainRunField field)
        {
            return field.Level == Gruppe2_TrainRunLevel.TrainRoof
                ? 1
                : Gruppe2_TrainRunLayout.RampProgressUnderRunner(field.Obstacles, field.Lane);
        }

        private static Dictionary<Gruppe2_TrainRunObstacleKind, Model3DGroup> CreateObstacleModels()
        {
            return new Dictionary<Gruppe2_TrainRunObstacleKind, Model3DGroup>
            {
                { Gruppe2_TrainRunObstacleKind.SmallBarrier, CreateObstacleModel(Gruppe2_TrainRunObstacleKind.SmallBarrier) },
                { Gruppe2_TrainRunObstacleKind.LargeBarrier, CreateObstacleModel(Gruppe2_TrainRunObstacleKind.LargeBarrier) },
                { Gruppe2_TrainRunObstacleKind.PlantPot, CreateObstacleModel(Gruppe2_TrainRunObstacleKind.PlantPot) },
                { Gruppe2_TrainRunObstacleKind.Curb, CreateObstacleModel(Gruppe2_TrainRunObstacleKind.Curb) },
                { Gruppe2_TrainRunObstacleKind.CenterPillar, CreateObstacleModel(Gruppe2_TrainRunObstacleKind.CenterPillar) }
            };
        }

        private static void AddThreeDimensionalCoin(
            Gruppe2_TrainRunVisualPool pool,
            Gruppe2_TrainRunReward reward,
            IGruppe2_TrainRunField field,
            double runnerX,
            int animationTicks)
        {
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new ScaleTransform3D(0.34, 0.34, 0.10));
            transform.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 1, 0), animationTicks * 11)));
            // Coins of the jetpack trail float at flight level, roof coins on top of the train.
            double x = LaneWorldX(reward.Lane);
            double y;
            switch (Gruppe2_TrainRunLayout.LevelOf(reward))
            {
                case Gruppe2_TrainRunHeightLevel.Flight:
                    y = CarRoofHeight * Gruppe2_TrainRunLayout.JetpackAltitudeInRoofHeights + 0.6;
                    break;
                case Gruppe2_TrainRunHeightLevel.Roof:
                    y = CarRoofHeight + 0.82;
                    break;
                default:
                    y = 0.82;
                    break;
            }

            double pull = MagnetPull(reward, field);
            if (pull > 0)
            {
                // The magnet pulls the coin sideways and up/down towards the runner's chest.
                double runnerChest = RunnerGroundHeight(field) + 0.8;
                x += (runnerX - x) * pull;
                y += (runnerChest - y) * pull;
            }

            transform.Children.Add(new TranslateTransform3D(x, y, WorldZ(reward.Distance)));
            pool.Show(reward, CoinModel, transform);
        }

        private static void AddThreeDimensionalUpgradeCoin(
            Gruppe2_TrainRunVisualPool pool, Gruppe2_TrainRunReward reward)
        {
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new ScaleTransform3D(0.62, 0.62, 0.16));
            transform.Children.Add(new TranslateTransform3D(
                LaneWorldX(reward.Lane), 0.82, WorldZ(reward.Distance)));
            pool.Show(reward, UpgradeCoinModels[reward.Upgrade], transform);
        }

        /// <summary>
        /// How far a coin has been pulled towards the runner by the magnet (0 = not at all,
        /// 1 = at the runner). Uses the same conditions as the rules for collecting it.
        /// </summary>
        private static double MagnetPull(Gruppe2_TrainRunReward reward, IGruppe2_TrainRunField field)
        {
            // The magnet pulls coins from every height level (ground, roof, flight level).
            if (!field.ActiveUpgrades.ContainsKey(Gruppe2_TrainRunUpgradeKind.Magnet)
                || Math.Abs(reward.Lane - field.Lane) > 1
                || reward.Distance <= 0 || reward.Distance >= Gruppe2_TrainRunLayout.MagnetRange)
            {
                return 0;
            }

            double progress = 1 - reward.Distance / Gruppe2_TrainRunLayout.MagnetRange;
            return progress * progress * (3 - 2 * progress);
        }

        /// <summary>
        /// Current height of the jump arc above the surface (0 when not jumping, on the track
        /// as well as on a train roof). A normal jump stays below the train roof; with the jump
        /// stick the arc goes higher than the roof, because only then can a train be climbed.
        /// </summary>
        private static double RunnerJumpHeight(IGruppe2_TrainRunField field)
        {
            double jumpPeak = field.ActiveUpgrades.ContainsKey(Gruppe2_TrainRunUpgradeKind.JumpStick)
                ? CarRoofHeight * 1.25
                : 1.15;
            return jumpPeak * field.JumpHeight;
        }

        /// <summary>
        /// Height of the camera: it follows the surface the runner is on and, with the jump stick,
        /// most of the high jump arc, so the runner stays in view while still visibly rising.
        /// </summary>
        private static double CameraHeight(IGruppe2_TrainRunField field)
        {
            const double JumpStickCameraFollow = 0.8;
            double jumpFollow = field.ActiveUpgrades.ContainsKey(Gruppe2_TrainRunUpgradeKind.JumpStick)
                ? JumpStickCameraFollow * RunnerJumpHeight(field)
                : 0;
            return 2.2 + RunnerGroundHeight(field) + jumpFollow;
        }

        private static ModelVisual3D AddThreeDimensionalRunner(
            Gruppe2_TrainRunVisualPool pool,
            IGruppe2_TrainRunField field,
            Gruppe2_TrainRunLaneChange laneChange,
            bool guard,
            int guardIntroTicks)
        {
            double x = laneChange.X - (guard ? 0.35 : 0);
            bool guardIntro = guard && guardIntroTicks > 0;
            double introElapsedTicks = guardIntro
                ? Gruppe2_TrainRunField.GuardIntroDurationTicks - guardIntroTicks
                : 0;
            double exitElapsedTicks = Math.Max(0, introElapsedTicks - GuardIntroFollowTicks);
            double exitProgress = Math.Min(1, exitElapsedTicks
                / (Gruppe2_TrainRunField.GuardIntroDurationTicks - GuardIntroFollowTicks));
            double exitRemaining = 1 - exitProgress;
            double retreatProgress = 1 - exitRemaining * exitRemaining;
            double z = guardIntro
                ? GuardChaseZ + (GuardIntroEndZ - GuardChaseZ) * retreatProgress
                : guard ? GuardChaseZ : RunnerWorldZ;
            double jump = guard ? 0
                : RunnerGroundHeight(field) + laneChange.Hop + RunnerJumpHeight(field);
            bool hasSkateboard = !guard
                && field.ActiveUpgrades.ContainsKey(Gruppe2_TrainRunUpgradeKind.Surfboard);
            bool skating = hasSkateboard && field.Level != Gruppe2_TrainRunLevel.Air && field.JumpHeight == 0;
            double introRunPhase = introElapsedTicks + 1 - exitElapsedTicks * exitProgress * 0.5;
            double run = guardIntro
                ? Math.Sin(introRunPhase * 0.48) * exitRemaining
                : Math.Sin(field.Distance * 0.48);
            double movement = skating
                ? Math.Sin(field.Distance * 0.32)
                : run;
            double sideToSide = skating ? movement * 0.08 : 0;
            // In the air (jump, roof jump, drop, jetpack) the legs and arms stay still, like on the
            // skateboard; the running animation continues once the runner is back on the ground.
            bool inAir = !guard && (field.Level == Gruppe2_TrainRunLevel.Air || field.JumpHeight > 0);
            double animation = inAir ? 0
                : guard || !hasSkateboard ? run
                : skating ? movement : 0;
            bool wearsJetpack = !guard && field.ActiveUpgrades.ContainsKey(Gruppe2_TrainRunUpgradeKind.Jetpack);
            double flight = guard ? 0 : field.JetpackHeight;
            // The jump stick is carried in the hand and only put under the feet while jumping.
            bool hasJumpStick = !guard && field.ActiveUpgrades.ContainsKey(Gruppe2_TrainRunUpgradeKind.JumpStick);
            JumpStickPose jumpStick = !hasJumpStick ? JumpStickPose.None
                : field.JumpHeight > 0 ? JumpStickPose.Jumping
                : JumpStickPose.Carried;
            Model3DGroup runnerModel = GetRunnerModel(guard, animation, hasSkateboard,
                wearsJetpack, flight > 0, jumpStick);
            double heightScale = RunnerHeightScale * (!guard && field.RollTicks > 0 ? 0.55 : 1);
            double footOffset = !guard && !hasSkateboard ? -0.17 * heightScale : 0;
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new ScaleTransform3D(RunnerWidthScale, heightScale, RunnerWidthScale));
            transform.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 1, 0), 180)));

            if (flight > 0)
            {
                // While climbing the runner tilts forward until she lies flat in the air
                // (head in flight direction), and straightens up again while sinking.
                transform.Children.Add(new RotateTransform3D(
                    new AxisAngleRotation3D(new Vector3D(1, 0, 0), -JetpackLyingDegrees * flight),
                    new Point3D(0, 0.8 * heightScale, 0)));
            }

            transform.Children.Add(new TranslateTransform3D(x + sideToSide, jump + footOffset, z));
            return pool.Show(guard ? GuardKey : RunnerKey, runnerModel, transform);
        }

        private static double LaneWorldX(int lane)
        {
            return (lane - 1) * 1.55;
        }

        private static void AddWorldModel(
            Gruppe2_TrainRunVisualPool pool, object key, Model3D model, double x, double y, double z, double scale)
        {
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new ScaleTransform3D(scale, scale, scale));
            transform.Children.Add(new TranslateTransform3D(x, y, z));
            pool.Show(key, model, transform);
        }

        private static Model3DGroup CreateTrainModel()
        {
            Model3DGroup train = new Model3DGroup();
            Material red = Material(Color.FromRgb(188, 47, 39));
            Material brightRed = Material(Color.FromRgb(221, 68, 54));
            Material dark = Material(Color.FromRgb(41, 52, 57));
            Material glass = Material(Color.FromRgb(77, 161, 181));
            Material trim = Material(Color.FromRgb(208, 211, 199));
            Material lamp = Material(Color.FromRgb(255, 213, 82));

            AddBox(train, -0.88, 0.38, -2.6, 0.88, 2.25, 2.35, red);
            AddBox(train, -0.92, 0.35, 1.65, 0.92, 0.72, 2.42, brightRed);
            AddBox(train, -0.78, 2.22, -2.55, 0.78, 2.42, 2.08, dark);
            AddBox(train, -0.70, 1.16, 2.36, 0.70, 2.02, 2.40, glass);
            AddBox(train, -0.89, 0.98, 2.34, 0.89, 1.08, 2.42, trim);
            AddBox(train, -0.72, 0.65, 2.40, -0.40, 0.83, 2.46, lamp);
            AddBox(train, 0.40, 0.65, 2.40, 0.72, 0.83, 2.46, lamp);

            for (int window = 0; window < 5; window++)
            {
                double z = -1.95 + window * 0.77;
                AddBox(train, -0.91, 1.20, z, -0.88, 1.78, z + 0.48, glass);
                AddBox(train, 0.88, 1.20, z, 0.91, 1.78, z + 0.48, glass);
            }

            for (int wheel = 0; wheel < 4; wheel++)
            {
                double z = -1.95 + wheel * 1.25;
                AddScaledSphere(train, -0.82, 0.36, z, 0.20, 0.28, 0.30,
                    Material(Color.FromRgb(35, 37, 39)));
                AddScaledSphere(train, 0.82, 0.36, z, 0.20, 0.28, 0.30,
                    Material(Color.FromRgb(35, 37, 39)));
            }

            train.Freeze();
            return train;
        }

        /// <summary>
        /// Cargo ramp in model space: rises from the low end (z = Start) to roof height (z = End).
        /// Brown wooden driving surface with two black boards running up in the running
        /// direction, and dark wooden side walls.
        /// AddTrainPart mirrors it so the low end faces the runner.
        /// </summary>
        private static Model3DGroup CreateCargoRampModel()
        {
            const double Start = -0.75;
            const double End = 0.75;
            const double InnerX = 0.52;
            const double OuterX = 0.66;
            const double Top = 1.0;
            const double WallLip = 0.24;
            const double BoardInnerX = 0.10;
            const double BoardOuterX = 0.44;
            const double BoardThickness = 0.035;
            Model3DGroup ramp = new Model3DGroup();
            Material ground = Material(Color.FromRgb(132, 88, 52));
            Material board = Material(Color.FromRgb(30, 30, 32));
            Material wall = Material(Color.FromRgb(88, 56, 34));
            Material wallEdge = Material(Color.FromRgb(118, 78, 48));
            // The boards end exactly at roof height (Top), the surface below them.
            Func<double, double> surface = z => (Top - BoardThickness) * (z - Start) / (End - Start);

            AddQuad(ramp,
                new Point3D(-InnerX, surface(Start), Start), new Point3D(InnerX, surface(Start), Start),
                new Point3D(InnerX, surface(End), End), new Point3D(-InnerX, surface(End), End), ground);

            for (int side = -1; side <= 1; side += 2)
            {
                // A black board lying on the brown surface, running up the ramp.
                double left = side < 0 ? -BoardOuterX : BoardInnerX;
                double right = side < 0 ? -BoardInnerX : BoardOuterX;
                AddQuad(ramp,
                    new Point3D(left, surface(Start) + BoardThickness, Start),
                    new Point3D(right, surface(Start) + BoardThickness, Start),
                    new Point3D(right, surface(End) + BoardThickness, End),
                    new Point3D(left, surface(End) + BoardThickness, End), board);
                foreach (double x in new[] { left, right })
                {
                    AddQuad(ramp,
                        new Point3D(x, surface(Start), Start), new Point3D(x, surface(End), End),
                        new Point3D(x, surface(End) + BoardThickness, End),
                        new Point3D(x, surface(Start) + BoardThickness, Start), board);
                }
            }

            for (int side = -1; side <= 1; side += 2)
            {
                // The wall top runs from WallLip above the low end up to roof height,
                // so it always stays above the driving surface.
                foreach (double x in new[] { side * InnerX, side * OuterX })
                {
                    AddQuad(ramp,
                        new Point3D(x, 0, Start), new Point3D(x, 0, End),
                        new Point3D(x, Top, End), new Point3D(x, WallLip, Start), wall);
                }

                AddQuad(ramp,
                    new Point3D(side * InnerX, WallLip, Start), new Point3D(side * OuterX, WallLip, Start),
                    new Point3D(side * OuterX, Top, End), new Point3D(side * InnerX, Top, End), wallEdge);
                AddQuad(ramp,
                    new Point3D(side * InnerX, 0, Start), new Point3D(side * OuterX, 0, Start),
                    new Point3D(side * OuterX, WallLip, Start), new Point3D(side * InnerX, WallLip, Start), wall);
            }

            ramp.Freeze();
            return ramp;
        }

        private static Model3DGroup CreateObstacleModel(Gruppe2_TrainRunObstacleKind kind)
        {
            Model3DGroup model = new Model3DGroup();
            Material dark = Material(Color.FromRgb(70, 68, 62));
            switch (kind)
            {
                case Gruppe2_TrainRunObstacleKind.LargeBarrier:
                    // Tall gate: the sign hangs high, so only a rolling runner fits underneath.
                    AddBox(model, -0.74, 0, -0.10, -0.62, 2.05, 0.10, dark);
                    AddBox(model, 0.62, 0, -0.10, 0.74, 2.05, 0.10, dark);
                    AddBox(model, -0.80, 1.05, -0.14, 0.80, 1.85, 0.14,
                        Material(Color.FromRgb(211, 47, 39)));
                    for (int stripe = 0; stripe < 4; stripe++)
                    {
                        double left = -0.66 + stripe * 0.36;
                        AddBox(model, left, 1.20, 0.15, left + 0.18, 1.70, 0.18,
                            Material(Color.FromRgb(237, 232, 210)));
                    }
                    break;
                case Gruppe2_TrainRunObstacleKind.SmallBarrier:
                    // Low red/white striped barrier on two feet: jump over it.
                    AddBox(model, -0.50, 0, -0.08, -0.40, 0.20, 0.08, dark);
                    AddBox(model, 0.40, 0, -0.08, 0.50, 0.20, 0.08, dark);
                    AddBox(model, -0.55, 0.18, -0.12, 0.55, 0.50, 0.12,
                        Material(Color.FromRgb(211, 47, 39)));
                    for (int stripe = 0; stripe < 4; stripe++)
                    {
                        double left = -0.47 + stripe * 0.26;
                        AddBox(model, left, 0.20, -0.13, left + 0.13, 0.48, 0.13,
                            Material(Color.FromRgb(240, 238, 230)));
                    }
                    break;
                case Gruppe2_TrainRunObstacleKind.PlantPot:
                    AddBox(model, -0.32, 0, -0.32, 0.32, 0.42, 0.32,
                        Material(Color.FromRgb(166, 83, 48)));
                    AddScaledSphere(model, 0, 0.70, 0, 0.54, 0.48, 0.50,
                        Material(Color.FromRgb(58, 143, 62)));
                    break;
                case Gruppe2_TrainRunObstacleKind.Curb:
                    AddBox(model, -0.80, 0, -0.26, 0.80, 0.34, 0.26,
                        Material(Color.FromRgb(163, 165, 160)));
                    break;
                default:
                    AddBox(model, -0.34, 0, -0.34, 0.34, 1.12, 0.34, dark);
                    break;
            }

            model.Freeze();
            return model;
        }

        /// <summary>Diffuse colour plus a slight glow of the same colour (about 45 %).</summary>
        private static Material GlowingMaterial(Color color)
        {
            MaterialGroup material = new MaterialGroup();
            material.Children.Add(new DiffuseMaterial(Brush(color)));
            material.Children.Add(new EmissiveMaterial(Brush(Color.FromRgb(
                (byte)(color.R * 0.45), (byte)(color.G * 0.45), (byte)(color.B * 0.45)))));
            material.Freeze();
            return material;
        }

        /// <summary>Upgrade coin: face in the upgrade colour, edge in a darker shade of it.</summary>
        private static Model3DGroup CreateUpgradeCoinModel(Color color)
        {
            Color edge = Color.FromRgb((byte)(color.R * 0.62), (byte)(color.G * 0.62), (byte)(color.B * 0.62));
            return CreateCoinModel(color, edge);
        }

        private static Model3DGroup CreateCoinModel(Color faceColor, Color edgeColor, bool glow = false)
        {
            Model3DGroup coin = new Model3DGroup();
            // A slight glow keeps the gold coins bright even on the side facing away from the sun.
            Material face = glow ? GlowingMaterial(faceColor) : Material(faceColor);
            Material edge = glow ? GlowingMaterial(edgeColor) : Material(edgeColor);
            const int segments = 20;
            const double radius = 0.5;
            const double depth = 0.10;

            for (int segment = 0; segment < segments; segment++)
            {
                double first = 2 * Math.PI * segment / segments;
                double second = 2 * Math.PI * (segment + 1) / segments;
                Point3D a = new Point3D(radius * Math.Cos(first), radius * Math.Sin(first), -depth);
                Point3D b = new Point3D(radius * Math.Cos(second), radius * Math.Sin(second), -depth);
                Point3D c = new Point3D(radius * Math.Cos(second), radius * Math.Sin(second), depth);
                Point3D d = new Point3D(radius * Math.Cos(first), radius * Math.Sin(first), depth);
                AddQuad(coin, a, b, c, d, edge);
                AddTriangle(coin, new Point3D(0, 0, depth),
                    new Point3D(radius * Math.Cos(first), radius * Math.Sin(first), depth),
                    new Point3D(radius * Math.Cos(second), radius * Math.Sin(second), depth), face);
                AddTriangle(coin, new Point3D(0, 0, -depth),
                    new Point3D(radius * Math.Cos(second), radius * Math.Sin(second), -depth),
                    new Point3D(radius * Math.Cos(first), radius * Math.Sin(first), -depth), face);
            }

            coin.Freeze();
            return coin;
        }

        private static Model3DGroup CreateRunnerModel(
            bool guard, double run, bool hasSkateboard, bool jetpack, bool flying, JumpStickPose jumpStick)
        {
            Model3DGroup runner = new Model3DGroup();
            Model3DGroup rider = new Model3DGroup();
            Material outfit = guard ? GuardOutfitMaterial : RunnerOutfitMaterial;
            Material hair = guard ? GuardHairMaterial : RunnerHairMaterial;
            double characterMovement = guard || !hasSkateboard ? run : 0;

            // While flying the skateboard stays behind; the jetpack is worn as long as the upgrade lasts.
            if (!guard && hasSkateboard && !flying)
            {
                runner.Children.Add(SkateboardModel);
            }

            if (jetpack)
            {
                runner.Children.Add(JetpackModel);
                if (flying)
                {
                    runner.Children.Add(JetpackFlameModel);
                }
            }

            if (jumpStick != JumpStickPose.None)
            {
                runner.Children.Add(PlaceJumpStick(jumpStick, run));
            }
            AddBox(rider, -0.34, 0.91, -0.18, 0.34, 1.70, 0.18, outfit);
            AddScaledSphere(rider, 0, 1.98, 0, 0.25, 0.31, 0.24, SkinMaterial);
            if (guard)
            {
                // The guard's flat, wide top reads as a uniform cap.
                AddScaledSphere(rider, 0, 2.20, 0, 0.30, 0.12, 0.29, hair);
            }
            else
            {
                // The runner's hair lies close to the head (no brim, so it does not look like a hat).
                AddScaledSphere(rider, 0, 2.06, -0.05, 0.262, 0.27, 0.24, hair);
            }

            AddScaledSphere(rider, -0.085, 2.02, 0.218, 0.027, 0.035, 0.02, RunnerEyeMaterial);
            AddScaledSphere(rider, 0.085, 2.02, 0.218, 0.027, 0.035, 0.02, RunnerEyeMaterial);

            Model3DGroup rearHair = new Model3DGroup();
            AddScaledSphere(rearHair, 0, 2.04, -0.15, 0.27, 0.24, 0.16, hair);
            if (guard)
            {
                AddScaledSphere(rearHair, 0, 1.88, -0.15, 0.20, 0.16, 0.14, hair);
            }
            else
            {
                AddScaledSphere(rearHair, -0.10, 1.90, -0.20, 0.12, 0.19, 0.14, hair);
                AddScaledSphere(rearHair, -0.13, 1.73, -0.24, 0.09, 0.16, 0.11, hair);
            }
            rearHair.Transform = new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 0, 1), characterMovement * 3),
                new Point3D(0, 2.0, -0.15));
            rider.Children.Add(rearHair);

            if (guard)
            {
                AddRunningArm(rider, -1, -run, outfit);
                AddRunningArm(rider, 1, run, outfit);
                AddRunningLeg(rider, -0.16, run, RunnerShoeMaterial);
                AddRunningLeg(rider, 0.16, -run, RunnerShoeMaterial);
            }
            else
            {
                AddBox(rider, -0.035, 1.08, 0.181, 0.035, 1.57, 0.205, RunnerAccentMaterial);
                AddBox(rider, -0.27, 1.12, 0.181, -0.09, 1.30, 0.205, RunnerAccentMaterial);

                if (hasSkateboard)
                {
                    AddRunnerLimb(rider, -0.22, 1.57, -0.01, -0.31, 1.69, 0.03,
                        0, outfit);
                    AddRunnerLimb(rider, -0.31, 1.69, 0.03, -0.40, 1.55, 0.08,
                        0, outfit);
                    AddRunnerLimb(rider, 0.22, 1.57, -0.01, 0.31, 1.69, 0.03,
                        0, outfit);
                    AddRunnerLimb(rider, 0.31, 1.69, 0.03, 0.40, 1.55, 0.08,
                        0, outfit);
                    AddScaledSphere(rider, -0.40, 1.55, 0.08, 0.06, 0.06, 0.06, SkinMaterial);
                    AddScaledSphere(rider, 0.40, 1.55, 0.08, 0.06, 0.06, 0.06, SkinMaterial);

                    AddRunnerLimb(rider, -0.16, 0.97, 0, -0.12, 0.61, 0.12,
                        0, PantsMaterial);
                    AddRunnerLimb(rider, -0.12, 0.61, 0.12, -0.07, 0.27, 0.24,
                        0, PantsMaterial);
                    AddRunnerLimb(rider, 0.16, 0.97, 0, 0.12, 0.61, -0.12,
                        0, PantsMaterial);
                    AddRunnerLimb(rider, 0.12, 0.61, -0.12, 0.07, 0.27, -0.24,
                        0, PantsMaterial);

                    AddBox(rider, -0.18, 0.17, 0.13, 0.04, 0.22, 0.35, PantsMaterial);
                    AddBox(rider, -0.18, 0.22, 0.14, 0.04, 0.30, 0.35, RunnerShoeMaterial);
                    AddBox(rider, -0.04, 0.17, -0.35, 0.18, 0.22, -0.13, PantsMaterial);
                    AddBox(rider, -0.04, 0.22, -0.35, 0.18, 0.30, -0.14, RunnerShoeMaterial);
                    rider.Transform = new RotateTransform3D(
                        new AxisAngleRotation3D(new Vector3D(0, 0, 1), -run * 5),
                        new Point3D(0, 0.22, 0));
                }
                else
                {
                    AddRunningArm(rider, -1, -run, outfit);
                    AddRunningArm(rider, 1, run, outfit);
                    AddRunningLeg(rider, -0.16, run, RunnerShoeMaterial);
                    AddRunningLeg(rider, 0.16, -run, RunnerShoeMaterial);
                }
            }

            runner.Children.Add(rider);
            runner.Freeze();
            return runner;
        }

        /// <summary>
        /// Jetpack worn on the back (in the runner model the back is the -z side):
        /// two silver tanks with red caps on a dark frame, nozzles at the bottom.
        /// </summary>
        private static Model3DGroup CreateJetpackModel()
        {
            Model3DGroup jetpack = new Model3DGroup();
            Material frame = Material(Color.FromRgb(52, 56, 62));
            Material tank = Material(Color.FromRgb(196, 202, 210));
            Material cap = Material(Color.FromRgb(204, 48, 40));
            AddBox(jetpack, -0.22, 1.02, -0.24, 0.22, 1.58, -0.18, frame);
            for (int side = -1; side <= 1; side += 2)
            {
                AddScaledSphere(jetpack, side * 0.15, 1.30, -0.34, 0.12, 0.32, 0.12, tank);
                AddScaledSphere(jetpack, side * 0.15, 1.62, -0.34, 0.08, 0.06, 0.08, cap);
                AddBox(jetpack, side * 0.15 - 0.06, 0.90, -0.40, side * 0.15 + 0.06, 1.00, -0.28, frame);
            }

            jetpack.Freeze();
            return jetpack;
        }

        /// <summary>
        /// Pogo-style jump stick, standing on its rubber tip at y = 0:
        /// spring, foot pegs at <see cref="JumpStickPegHeight"/>, shaft and handlebar on top.
        /// </summary>
        private static Model3DGroup CreateJumpStickModel()
        {
            Model3DGroup stick = new Model3DGroup();
            Material rubber = Material(Color.FromRgb(34, 34, 38));
            Material spring = Material(Color.FromRgb(150, 156, 164));
            Material tube = Material(Color.FromRgb(196, 202, 210));
            Material grip = Material(Color.FromRgb(204, 48, 40));

            AddBox(stick, -0.05, 0, -0.05, 0.05, 0.08, 0.05, rubber);
            for (int coil = 0; coil < 6; coil++)
            {
                double y = 0.10 + coil * 0.06;
                AddBox(stick, -0.07, y, -0.07, 0.07, y + 0.03, 0.07, spring);
            }

            AddBox(stick, -0.025, 0.08, -0.025, 0.025, 0.46, 0.025, spring);
            AddBox(stick, -0.05, 0.46, -0.05, 0.05, JumpStickPegHeight, 0.05, tube);
            AddBox(stick, -0.20, JumpStickPegHeight - 0.03, -0.04, 0.20, JumpStickPegHeight + 0.01, 0.04, rubber);
            AddBox(stick, -0.03, JumpStickPegHeight, -0.03, 0.03, 1.78, 0.03, tube);
            AddBox(stick, -0.24, 1.76, -0.035, 0.24, 1.83, 0.035, grip);
            stick.Freeze();
            return stick;
        }

        private const double JumpStickPegHeight = 0.72;

        /// <summary>
        /// Carried: upright in the right hand, swinging with the arm.
        /// Jumping: in front of the runner (model front is +z), feet on the pegs,
        /// handlebar at hand height and the tip reaching below the feet.
        /// </summary>
        private static Model3DGroup PlaceJumpStick(JumpStickPose pose, double run)
        {
            Transform3D transform = pose == JumpStickPose.Jumping
                ? new TranslateTransform3D(0, 0.20 - JumpStickPegHeight, 0.30)
                : (Transform3D)new Transform3DGroup
                {
                    Children = new Transform3DCollection
                    {
                        new ScaleTransform3D(0.75, 0.75, 0.75),
                        new TranslateTransform3D(0.47, 0.25, -run * 0.18)
                    }
                };
            return new Model3DGroup { Children = { JumpStickModel }, Transform = transform };
        }

        /// <summary>Glowing exhaust flames below the two jetpack nozzles.</summary>
        private static Model3DGroup CreateJetpackFlameModel()
        {
            Model3DGroup flames = new Model3DGroup();
            Material outer = new EmissiveMaterial(Brush(Color.FromRgb(255, 120, 24)));
            Material inner = new EmissiveMaterial(Brush(Color.FromRgb(255, 226, 90)));
            outer.Freeze();
            inner.Freeze();
            for (int side = -1; side <= 1; side += 2)
            {
                AddScaledSphere(flames, side * 0.15, 0.72, -0.34, 0.08, 0.20, 0.08, outer);
                AddScaledSphere(flames, side * 0.15, 0.78, -0.34, 0.05, 0.12, 0.05, inner);
            }

            flames.Freeze();
            return flames;
        }

        private static Model3DGroup CreateSkateboardModel()
        {
            Model3DGroup skateboard = new Model3DGroup();
            AddBox(skateboard, -0.24, 0.12, -0.52, 0.24, 0.19, 0.52, SkateboardDeckMaterial);
            AddBox(skateboard, -0.12, 0.191, -0.40, 0.12, 0.202, 0.40, SkateboardGraphicMaterial);
            AddBox(skateboard, -0.18, 0.08, -0.36, 0.18, 0.13, -0.28, SkateboardTruckMaterial);
            AddBox(skateboard, -0.18, 0.08, 0.28, 0.18, 0.13, 0.36, SkateboardTruckMaterial);

            for (int axle = -1; axle <= 1; axle += 2)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    AddScaledSphere(skateboard, side * 0.16, 0.055, axle * 0.34,
                        0.08, 0.065, 0.06, PantsMaterial);
                }
            }

            skateboard.Freeze();
            return skateboard;
        }

        private static void AddRunnerLimb(
            Model3DGroup runner, double x1, double y1, double z1,
            double x2, double y2, double z2, double angle, Material material)
        {
            double thickness = 0.12;
            Model3DGroup limb = new Model3DGroup();
            AddBox(limb,
                Math.Min(x1, x2) - thickness, Math.Min(y1, y2),
                Math.Min(z1, z2) - 0.10,
                Math.Max(x1, x2) + thickness, Math.Max(y1, y2) + 0.08,
                Math.Max(z1, z2) + 0.10, material);
            limb.Transform = new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 0, 1), angle * 20),
                new Point3D(x1, y1, z1));
            runner.Children.Add(limb);
        }

        private static void AddRunningArm(
            Model3DGroup rider, int side, double stride, Material outfit)
        {
            double shoulderX = side * 0.22;
            Point3D elbow = new Point3D(side * 0.35, 1.30, stride * 0.13);
            Point3D hand = new Point3D(side * 0.43, 1.14, -stride * 0.18);
            AddRunnerLimb(rider, shoulderX, 1.57, -0.01,
                elbow.X, elbow.Y, elbow.Z, 0, outfit);
            AddRunnerLimb(rider, elbow.X, elbow.Y, elbow.Z,
                hand.X, hand.Y, hand.Z, 0, outfit);
            AddScaledSphere(rider, hand.X, hand.Y, hand.Z, 0.06, 0.06, 0.06, SkinMaterial);
        }

        private static void AddRunningLeg(
            Model3DGroup rider, double hipX, double stride, Material shoeMaterial)
        {
            const double HipHeight = 0.82;
            double footX = hipX * 0.75;
            double footZ = stride * 0.24;
            double footY = 0.17 + Math.Max(0, stride) * 0.20;
            double kneeY = 0.56 + Math.Max(0, stride) * 0.12;
            double kneeZ = footZ * 0.45;
            double kneeX = hipX * 0.75;
            double ankleY = footY + 0.10;

            AddRunnerLimb(rider, hipX, HipHeight, 0, kneeX, kneeY, kneeZ, 0, PantsMaterial);
            AddRunnerLimb(rider, kneeX, kneeY, kneeZ, footX, ankleY, footZ, 0, PantsMaterial);

            Model3DGroup foot = new Model3DGroup();
            AddBox(foot, footX - 0.11, footY - 0.04, footZ - 0.13,
                footX + 0.11, footY + 0.02, footZ + 0.13, PantsMaterial);
            AddBox(foot, footX - 0.11, footY, footZ - 0.14,
                footX + 0.11, footY + 0.09, footZ + 0.14, shoeMaterial);
            rider.Children.Add(foot);
        }

        private static void AddScaledSphere(
            Model3DGroup model, double x, double y, double z,
            double radiusX, double radiusY, double radiusZ, Material material)
        {
            GeometryModel3D sphere = new GeometryModel3D(SphereMesh, material)
            {
                BackMaterial = material,
                Transform = new Transform3DGroup
                {
                    Children = new Transform3DCollection
                    {
                        new ScaleTransform3D(radiusX, radiusY, radiusZ),
                        new TranslateTransform3D(x, y, z)
                    }
                }
            };
            sphere.Freeze();
            model.Children.Add(sphere);
        }

        private static Model3DGroup CreateCityModel()
        {
            Model3DGroup city = new Model3DGroup();
            Color[] facades =
            {
                Color.FromRgb(157, 73, 52),
                Color.FromRgb(185, 91, 56),
                Color.FromRgb(131, 77, 65),
                Color.FromRgb(178, 113, 66),
                Color.FromRgb(125, 83, 73),
                Color.FromRgb(191, 102, 62)
            };
            double[,] buildings =
            {
                { 5.2, 10, 4.4, 4.0, 9.0 },
                { 6.0, 2, 4.8, 4.2, 11.5 },
                { 7.3, -6, 5.2, 4.5, 10.0 },
                { 9.0, -15, 6.0, 5.0, 13.0 },
                { 5.4, -24, 5.0, 5.0, 8.5 },
                { 8.5, -32, 7.0, 5.5, 12.0 }
            };

            for (int side = -1; side <= 1; side += 2)
            {
                for (int index = 0; index < buildings.GetLength(0); index++)
                {
                    AddCityBuilding(city,
                        side * (buildings[index, 0] + CityTrackClearance),
                        buildings[index, 1],
                        buildings[index, 2],
                        buildings[index, 3],
                        buildings[index, 4],
                        facades[index],
                        index);
                }
            }

            city.Freeze();
            return city;
        }

        private static void AddCityBuilding(
            Model3DGroup city, double x, double z, double width, double depth, double height,
            Color facadeColor, int style)
        {
            Material facade = Material(facadeColor);
            Material sideFacade = Material(Color.FromRgb(
                (byte)(facadeColor.R * 0.72),
                (byte)(facadeColor.G * 0.72),
                (byte)(facadeColor.B * 0.72)));
            Material trim = Material(Color.FromRgb(91, 60, 49));
            Material glass = Material(Color.FromRgb(104, 178, 199));
            Material litGlass = Material(Color.FromRgb(255, 201, 111));
            double left = x - width / 2;
            double right = x + width / 2;
            double front = z + depth / 2;

            AddBox(city, left, 0, z - depth / 2, right, height, front, facade);
            AddBox(city, left - 0.16, height, z - depth / 2 - 0.12,
                right + 0.16, height + 0.24, front + 0.12, trim);
            AddBox(city, left - 0.12, 0, front + 0.08,
                right + 0.12, 0.36, front + 0.20, sideFacade);

            int columns = Math.Max(2, (int)(width / 1.15));
            int floors = Math.Max(2, (int)(height / 1.35));
            double windowWidth = width / (columns * 2.2);
            double windowHeight = Math.Min(0.55, height / (floors * 2.4));
            for (int floor = 0; floor < floors; floor++)
            {
                double windowY = 0.72 + floor * (height - 1.0) / floors;
                for (int column = 0; column < columns; column++)
                {
                    double windowX = left + (column + 0.5) * width / columns;
                    Material window = (floor + column + style) % 4 == 0 ? litGlass : glass;
                    AddBox(city,
                        windowX - windowWidth / 2, windowY,
                        front + 0.015,
                        windowX + windowWidth / 2, windowY + windowHeight,
                        front + 0.075,
                        window);
                }
            }

            double sideX = x < 0 ? right : left;
            double sideWindowLeft = x < 0 ? sideX + 0.015 : sideX - 0.07;
            double sideWindowRight = x < 0 ? sideX + 0.07 : sideX - 0.015;
            for (int floor = 0; floor < floors; floor++)
            {
                double windowY = 0.72 + floor * (height - 1.0) / floors;
                for (int column = 0; column < 2; column++)
                {
                    double windowZ = z - depth * 0.25 + column * depth * 0.5;
                    AddBox(city,
                        sideWindowLeft, windowY, windowZ - 0.22,
                        sideWindowRight, windowY + windowHeight,
                        windowZ + 0.22,
                        (floor + column + style) % 5 == 0 ? litGlass : glass);
                }
            }
        }

        /// <summary>
        /// Freight boxcar: rust-red box body with vertical ribs, a sliding door on each side,
        /// a low arched roof, plain ends with ribs, ladder and coupler (no windows),
        /// on a dark underframe with two bogies. Model space: front (+z) faces the runner,
        /// AddTrainPart fits the model into the space the rules reserve for a car.
        /// </summary>
        private static Model3DGroup CreateCargoWagonModel()
        {
            const double HalfWidth = 0.78;
            const double HalfLength = 1.55;
            const double BodyBottom = 0.36;
            const double BodyTop = 1.60;
            Model3DGroup model = new Model3DGroup();
            Material body = Material(Color.FromRgb(146, 60, 40));
            Material rib = Material(Color.FromRgb(118, 46, 30));
            Material door = Material(Color.FromRgb(128, 74, 48));
            Material roof = Material(Color.FromRgb(104, 96, 90));
            Material steel = Material(Color.FromRgb(46, 46, 50));
            Material wheel = Material(Color.FromRgb(30, 30, 34));

            // Underframe and bogies with wheels.
            AddBox(model, -HalfWidth + 0.08, 0.24, -HalfLength + 0.05, HalfWidth - 0.08, BodyBottom, HalfLength - 0.05, steel);
            for (int bogie = -1; bogie <= 1; bogie += 2)
            {
                double bogieZ = bogie * 1.0;
                AddBox(model, -0.62, 0.12, bogieZ - 0.36, 0.62, 0.26, bogieZ + 0.36, steel);
                for (int axle = -1; axle <= 1; axle += 2)
                {
                    for (int side = -1; side <= 1; side += 2)
                    {
                        AddScaledSphere(model, side * 0.64, 0.15, bogieZ + axle * 0.22, 0.06, 0.15, 0.15, wheel);
                    }
                }
            }

            // Box body with a low arched roof.
            AddBox(model, -HalfWidth, BodyBottom, -HalfLength, HalfWidth, BodyTop, HalfLength, body);
            AddBox(model, -HalfWidth - 0.02, BodyTop, -HalfLength - 0.02, HalfWidth + 0.02, BodyTop + 0.06, HalfLength + 0.02, roof);
            AddBox(model, -HalfWidth + 0.18, BodyTop + 0.06, -HalfLength, HalfWidth - 0.18, BodyTop + 0.13, HalfLength, roof);

            for (int side = -1; side <= 1; side += 2)
            {
                double outer = side * (HalfWidth + 0.03);
                double surface = side * HalfWidth;

                // Vertical ribs along the side, leaving room for the door in the middle.
                for (double z = -HalfLength + 0.12; z <= HalfLength - 0.1; z += 0.29)
                {
                    if (Math.Abs(z) < 0.55)
                    {
                        continue;
                    }

                    AddBox(model, Math.Min(surface, outer), BodyBottom + 0.04, z - 0.03,
                        Math.Max(surface, outer), BodyTop - 0.04, z + 0.03, rib);
                }

                // Sliding door with its rails above and below.
                double doorOuter = side * (HalfWidth + 0.05);
                AddBox(model, Math.Min(surface, doorOuter), BodyBottom + 0.08, -0.45,
                    Math.Max(surface, doorOuter), BodyTop - 0.10, 0.45, door);
                AddBox(model, Math.Min(surface, doorOuter), BodyTop - 0.10, -0.58,
                    Math.Max(surface, doorOuter), BodyTop - 0.05, 0.58, steel);
                AddBox(model, Math.Min(surface, doorOuter), BodyBottom + 0.03, -0.58,
                    Math.Max(surface, doorOuter), BodyBottom + 0.08, 0.58, steel);
                // Door handle on the outside of the door.
                AddBox(model, Math.Min(doorOuter, doorOuter + side * 0.03), 0.80, 0.30,
                    Math.Max(doorOuter, doorOuter + side * 0.03), 1.10, 0.36, steel);
            }

            for (int end = -1; end <= 1; end += 2)
            {
                double surface = end * HalfLength;
                double outer = end * (HalfLength + 0.03);
                double zMin = Math.Min(surface, outer);
                double zMax = Math.Max(surface, outer);

                // Plain end wall: horizontal ribs, a ladder on one side and the coupler below.
                for (double y = BodyBottom + 0.25; y < BodyTop; y += 0.35)
                {
                    AddBox(model, -HalfWidth + 0.04, y - 0.03, zMin, HalfWidth - 0.04, y + 0.03, zMax, rib);
                }

                AddBox(model, 0.48, BodyBottom, zMin - 0.01, 0.51, BodyTop, zMax + 0.01, steel);
                AddBox(model, 0.66, BodyBottom, zMin - 0.01, 0.69, BodyTop, zMax + 0.01, steel);
                for (double y = BodyBottom + 0.15; y < BodyTop; y += 0.2)
                {
                    AddBox(model, 0.48, y, zMin - 0.01, 0.69, y + 0.025, zMax + 0.01, steel);
                }

                AddBox(model, -0.10, 0.26, zMin, 0.10, 0.36, zMax, steel);
            }

            model.Freeze();
            return model;
        }

        private static Model3DGroup CreateBushModel()
        {
            Model3DGroup bush = new Model3DGroup();
            AddBox(bush, -0.15, -0.78, -0.15, 0.15, -0.18, 0.15,
                Material(Color.FromRgb(91, 62, 38)));
            AddScaledSphere(bush, -0.43, -0.28, 0.00, 0.48, 0.48, 0.44,
                Material(Color.FromRgb(36, 112, 47)));
            AddScaledSphere(bush, 0.00, -0.18, 0.00, 0.56, 0.57, 0.50,
                Material(Color.FromRgb(53, 144, 58)));
            AddScaledSphere(bush, 0.42, -0.30, 0.02, 0.43, 0.45, 0.42,
                Material(Color.FromRgb(41, 124, 49)));
            AddScaledSphere(bush, -0.20, 0.10, 0.08, 0.37, 0.40, 0.36,
                Material(Color.FromRgb(73, 164, 67)));
            AddScaledSphere(bush, 0.24, 0.13, 0.10, 0.34, 0.36, 0.34,
                Material(Color.FromRgb(61, 151, 59)));
            bush.Freeze();
            return bush;
        }

        private static Model3DGroup CreateSignalModel()
        {
            Model3DGroup signal = new Model3DGroup();
            Material pole = Material(Color.FromRgb(71, 77, 79));
            Material housing = Material(Color.FromRgb(39, 43, 46));

            AddBox(signal, -0.07, 0, -0.08, 0.07, 1.65, 0.08, pole);
            AddBox(signal, -0.22, 0.02, -0.16, 0.22, 0.12, 0.16,
                Material(Color.FromRgb(88, 85, 75)));
            // Slim housing, so the signal between two tracks does not look bulky.
            AddBox(signal, -0.28, 1.36, -0.22, 0.28, 2.84, 0.22, housing);
            AddBox(signal, -0.32, 2.82, -0.26, 0.32, 2.94, 0.26, pole);
            AddBox(signal, -0.24, 1.27, -0.20, 0.24, 1.37, 0.20, pole);

            AddSignalLens(signal, 2.51, Color.FromRgb(242, 35, 38));
            AddSignalLens(signal, 2.10, Color.FromRgb(255, 190, 28));
            AddSignalLens(signal, 1.69, Color.FromRgb(55, 222, 75));

            signal.Freeze();
            return signal;
        }

        private static void AddSignalLens(Model3DGroup signal, double y, Color color)
        {
            Material backing = Material(Color.FromRgb(12, 14, 16));
            Material lens = new EmissiveMaterial(Brush(color));
            lens.Freeze();
            AddScaledSphere(signal, 0, y, 0.24, 0.21, 0.21, 0.10, backing);
            AddScaledSphere(signal, 0, y, 0.31, 0.15, 0.15, 0.12, lens);
        }

        private static MeshGeometry3D CreateSphereMesh()
        {
            const int rings = 8;
            const int segments = 12;
            MeshGeometry3D mesh = new MeshGeometry3D();
            for (int ring = 0; ring <= rings; ring++)
            {
                double latitude = Math.PI * ring / rings;
                double y = Math.Cos(latitude);
                double ringRadius = Math.Sin(latitude);
                for (int segment = 0; segment <= segments; segment++)
                {
                    double longitude = 2 * Math.PI * segment / segments;
                    mesh.Positions.Add(new Point3D(
                        ringRadius * Math.Cos(longitude),
                        y,
                        ringRadius * Math.Sin(longitude)));
                }
            }

            for (int ring = 0; ring < rings; ring++)
            {
                for (int segment = 0; segment < segments; segment++)
                {
                    int current = ring * (segments + 1) + segment;
                    int next = current + segments + 1;
                    mesh.TriangleIndices.Add(current);
                    mesh.TriangleIndices.Add(next);
                    mesh.TriangleIndices.Add(current + 1);
                    mesh.TriangleIndices.Add(current + 1);
                    mesh.TriangleIndices.Add(next);
                    mesh.TriangleIndices.Add(next + 1);
                }
            }

            mesh.Freeze();
            return mesh;
        }

        private static Material Material(Color color)
        {
            DiffuseMaterial material = new DiffuseMaterial(Brush(color));
            material.Freeze();
            return material;
        }

        private static void AddBox(Model3DGroup model,
            double x1, double y1, double z1, double x2, double y2, double z2, Material material)
        {
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new ScaleTransform3D(x2 - x1, y2 - y1, z2 - z1));
            transform.Children.Add(new TranslateTransform3D(
                (x1 + x2) / 2, (y1 + y2) / 2, (z1 + z2) / 2));
            GeometryModel3D box = new GeometryModel3D(BoxMesh, material)
            {
                BackMaterial = material,
                Transform = transform
            };
            box.Freeze();
            model.Children.Add(box);
        }

        private static MeshGeometry3D CreateBoxMesh()
        {
            Point3D[][] faces =
            {
                new[]
                {
                    new Point3D(-0.5, -0.5, -0.5), new Point3D(-0.5, -0.5, 0.5),
                    new Point3D(-0.5, 0.5, 0.5), new Point3D(-0.5, 0.5, -0.5)
                },
                new[]
                {
                    new Point3D(0.5, -0.5, -0.5), new Point3D(0.5, 0.5, -0.5),
                    new Point3D(0.5, 0.5, 0.5), new Point3D(0.5, -0.5, 0.5)
                },
                new[]
                {
                    new Point3D(-0.5, -0.5, -0.5), new Point3D(0.5, -0.5, -0.5),
                    new Point3D(0.5, -0.5, 0.5), new Point3D(-0.5, -0.5, 0.5)
                },
                new[]
                {
                    new Point3D(-0.5, 0.5, -0.5), new Point3D(-0.5, 0.5, 0.5),
                    new Point3D(0.5, 0.5, 0.5), new Point3D(0.5, 0.5, -0.5)
                },
                new[]
                {
                    new Point3D(-0.5, -0.5, -0.5), new Point3D(-0.5, 0.5, -0.5),
                    new Point3D(0.5, 0.5, -0.5), new Point3D(0.5, -0.5, -0.5)
                },
                new[]
                {
                    new Point3D(-0.5, -0.5, 0.5), new Point3D(0.5, -0.5, 0.5),
                    new Point3D(0.5, 0.5, 0.5), new Point3D(-0.5, 0.5, 0.5)
                }
            };
            MeshGeometry3D mesh = new MeshGeometry3D();
            foreach (Point3D[] face in faces)
            {
                int firstIndex = mesh.Positions.Count;
                foreach (Point3D point in face)
                {
                    mesh.Positions.Add(point);
                }

                mesh.TriangleIndices.Add(firstIndex);
                mesh.TriangleIndices.Add(firstIndex + 1);
                mesh.TriangleIndices.Add(firstIndex + 2);
                mesh.TriangleIndices.Add(firstIndex);
                mesh.TriangleIndices.Add(firstIndex + 2);
                mesh.TriangleIndices.Add(firstIndex + 3);
            }

            mesh.Freeze();
            return mesh;
        }

        private static void AddQuad(Model3DGroup model, Point3D a, Point3D b, Point3D c, Point3D d,
            Material material)
        {
            MeshGeometry3D mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection { a, b, c, d },
                TriangleIndices = new Int32Collection { 0, 1, 2, 0, 2, 3 }
            };
            mesh.Freeze();
            GeometryModel3D face = new GeometryModel3D(mesh, material) { BackMaterial = material };
            face.Freeze();
            model.Children.Add(face);
        }

        private static void AddTriangle(Model3DGroup model, Point3D a, Point3D b, Point3D c,
            Material material)
        {
            MeshGeometry3D mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection { a, b, c },
                TriangleIndices = new Int32Collection { 0, 1, 2 }
            };
            mesh.Freeze();
            GeometryModel3D face = new GeometryModel3D(mesh, material) { BackMaterial = material };
            face.Freeze();
            model.Children.Add(face);
        }

        private static void DrawHud(Canvas canvas, double width, double height, IGruppe2_TrainRunField field)
        {
            // Left: speed and active upgrades.
            AddPanel(canvas, 12, 12, 188, 58);
            AddText(canvas, "TEMPO", 24, 18, 10, HudLabelBrush, FontWeights.SemiBold);
            AddText(canvas, "x" + field.Speed.ToString("0.0"), 24, 30, 20, Brushes.White, FontWeights.Bold);
            string upgrades = field.ActiveUpgrades.Count == 0
                ? "keine Upgrades"
                : string.Join(" · ", field.ActiveUpgrades.Keys.Select(UpgradeLabel));
            AddText(canvas, upgrades, 92, 35, 11, HudLabelBrush, FontWeights.Normal);

            // Right: score and coins.
            AddPanel(canvas, width - 172, 12, 160, 58);
            AddText(canvas, "PUNKTE", width - 160, 18, 10, HudLabelBrush, FontWeights.SemiBold);
            AddText(canvas, field.Score.ToString(), width - 160, 30, 20, Brushes.White, FontWeights.Bold);
            AddEllipse(canvas, width - 78, 37, 14, 14, HudGoldBrush, Brush(Color.FromRgb(176, 128, 20)), 1.5);
            AddText(canvas, field.Coins.ToString(), width - 58, 33, 15, HudGoldBrush, FontWeights.Bold);

            // Centre: title.
            AddText(canvas, "TRAIN RUN", width / 2 - 56, 16, 20, Brushes.White, FontWeights.Black);

            if (field.GuardVisible)
            {
                AddPanel(canvas, 12, 78, 188, 26);
                AddEllipse(canvas, 22, 87, 9, 9, Brush(Color.FromRgb(226, 68, 58)), null, 0);
                AddText(canvas, "Bahnwärter " + field.GuardGap.ToString("0") + " m", 38, 82, 12,
                    Brushes.White, FontWeights.SemiBold);
            }

            if (field.MessageTicks > 0 || field.GameOver)
            {
                double messageWidth = Math.Min(width - 40, 420);
                AddPanel(canvas, (width - messageWidth) / 2, height - 82, messageWidth, 30);
                AddCenteredText(canvas, field.Message, (width - messageWidth) / 2, height - 76, messageWidth, 13,
                    field.GameOver ? Brush(Color.FromRgb(255, 120, 96)) : Brushes.White, FontWeights.SemiBold);
            }

            double controlsWidth = Math.Min(width - 40, 380);
            AddPanel(canvas, (width - controlsWidth) / 2, height - 42, controlsWidth, 28);
            AddCenteredText(canvas, "A/D Spur wechseln     W Springen     S Rollen",
                (width - controlsWidth) / 2, height - 37, controlsWidth, 12, HudLabelBrush, FontWeights.SemiBold);
        }

        private static readonly Brush HudPanelBrush = Brush(Color.FromArgb(150, 14, 20, 32));
        private static readonly Brush HudLabelBrush = Brush(Color.FromRgb(196, 206, 220));
        private static readonly Brush HudGoldBrush = Brush(Color.FromRgb(250, 196, 46));

        private static void AddPanel(Canvas canvas, double x, double y, double width, double height)
        {
            Rectangle panel = new Rectangle
            {
                Width = width,
                Height = height,
                RadiusX = 10,
                RadiusY = 10,
                Fill = HudPanelBrush
            };
            Canvas.SetLeft(panel, x);
            Canvas.SetTop(panel, y);
            canvas.Children.Add(panel);
        }

        private static void AddCenteredText(Canvas canvas, string text, double x, double y, double width,
            double size, Brush brush, FontWeight weight)
        {
            TextBlock block = new TextBlock
            {
                Text = text,
                Width = width,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontSize = size,
                Foreground = brush,
                FontWeight = weight
            };
            Canvas.SetLeft(block, x);
            Canvas.SetTop(block, y);
            canvas.Children.Add(block);
        }

        private static string UpgradeLabel(Gruppe2_TrainRunUpgradeKind upgrade)
        {
            switch (upgrade)
            {
                case Gruppe2_TrainRunUpgradeKind.Magnet: return "Magnet";
                case Gruppe2_TrainRunUpgradeKind.DoubleCoins: return "Stern (Muenzen x2)";
                case Gruppe2_TrainRunUpgradeKind.Surfboard: return "Surfboard";
                case Gruppe2_TrainRunUpgradeKind.JumpStick: return "Huepfstab";
                case Gruppe2_TrainRunUpgradeKind.Jetpack: return "Jetpack";
                default: return "Upgrade";
            }
        }

        private static void DrawGameOver(Canvas canvas, double width, double height)
        {
            double panelWidth = Math.Min(width - 40, 420);
            double left = (width - panelWidth) / 2;
            AddRectangle(canvas, 0, 0, width, height, Brush(Color.FromArgb(90, 0, 0, 0)));
            AddPanel(canvas, left, height * 0.36, panelWidth, 110);
            AddCenteredText(canvas, "SPIEL VORBEI", left, height * 0.36 + 18, panelWidth, 30,
                Brush(Color.FromRgb(255, 120, 96)), FontWeights.Black);
            AddCenteredText(canvas, "Starte ein neues Spiel für eine weitere Runde.", left,
                height * 0.36 + 66, panelWidth, 14, HudLabelBrush, FontWeights.SemiBold);
        }

        private static void AddRectangle(Canvas canvas, double x, double y, double width, double height,
            Brush fill, Brush stroke = null, double strokeWidth = 0)
        {
            Rectangle shape = new Rectangle { Width = width, Height = height, Fill = fill, Stroke = stroke, StrokeThickness = strokeWidth };
            Canvas.SetLeft(shape, x);
            Canvas.SetTop(shape, y);
            canvas.Children.Add(shape);
        }

        private static void AddEllipse(Canvas canvas, double x, double y, double width, double height,
            Brush fill, Brush stroke, double strokeWidth)
        {
            Ellipse shape = new Ellipse { Width = width, Height = height, Fill = fill, Stroke = stroke, StrokeThickness = strokeWidth };
            Canvas.SetLeft(shape, x);
            Canvas.SetTop(shape, y);
            canvas.Children.Add(shape);
        }

        private static void AddText(Canvas canvas, string text, double x, double y, double size,
            Brush brush, FontWeight weight)
        {
            TextBlock block = new TextBlock
            {
                Text = text,
                FontSize = size,
                Foreground = brush,
                FontWeight = weight,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = Math.Max(100, canvas.ActualWidth - x - 8)
            };
            Canvas.SetLeft(block, x);
            Canvas.SetTop(block, y);
            canvas.Children.Add(block);
        }
    }
}
