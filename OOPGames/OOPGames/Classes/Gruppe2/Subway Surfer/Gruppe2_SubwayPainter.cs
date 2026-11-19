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
    public sealed class Gruppe2_SubwayPainter : IPaintGame2
    {
        private static readonly Brush SkyBrush = CreateSkyBrush();
        private static readonly Model3DGroup SceneLights = Gruppe2_SubwaySceneLighting.CreateLights();
        private static readonly Brush TrackBrush = Brush(Color.FromRgb(77, 74, 69));
        private static readonly Brush RailBrush = Brush(Color.FromRgb(203, 210, 216));
        private static readonly Brush PinkBrush = Brush(Color.FromRgb(231, 49, 135));
        private static readonly MeshGeometry3D BoxMesh = CreateBoxMesh();
        private static readonly MeshGeometry3D SphereMesh = CreateSphereMesh();
        private static readonly Material GuardOutfitMaterial = Material(Color.FromRgb(43, 74, 112));
        private static readonly Material RunnerOutfitMaterial = Material(Color.FromRgb(231, 49, 135));
        private static readonly Material SkinMaterial = Material(Color.FromRgb(245, 194, 157));
        private static readonly Material GuardHairMaterial = Material(Color.FromRgb(105, 67, 38));
        private static readonly Material RunnerHairMaterial = Material(Color.FromRgb(111, 0, 110));
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
        private static readonly Model3DGroup CargoWagonModel = CreateCargoWagonModel();
        private static readonly Model3DGroup TrainModel = CreateTrainModel();
        private static readonly Model3DGroup CargoRampModel = CreateCargoRampModel();
        private static readonly Model3DGroup CoinModel = CreateCoinModel(
            Color.FromRgb(255, 203, 32), Color.FromRgb(179, 126, 16), glow: true);
        private static readonly Model3DGroup UpgradeCoinModel = CreateCoinModel(
            Color.FromRgb(255, 72, 170), Color.FromRgb(160, 18, 92));
        private static readonly Model3DGroup TrackModel = CreateTrackModel();
        private static readonly Model3DGroup SleeperModel = CreateSleeperModel();
        private static readonly Dictionary<Gruppe2_SubwayObstacleKind, Model3DGroup> ObstacleModels =
            CreateObstacleModels();
        private static readonly Model3DGroup CityModel = CreateCityModel();
        private static readonly Model3DGroup BushModel = CreateBushModel();
        private static readonly Model3DGroup SignalModel = CreateSignalModel();
        private const double CityModelScale = 0.72;
        private const double CityRepeatDistance = 42;
        // Extra distance (model units) between the houses and the outer tracks,
        // so the houses never cut into the track bed and sleepers.
        private const double CityTrackClearance = 1.6;
        private const double TrackHorizonRatio = 0.40;
        private const double TrackBottomRatio = 0.94;
        private const double ObjectTrackDepthRatio = 0.82;
        private const double ObstaclePreviewDistance = 520;
        private const double RunnerWorldZ = 0.55;
        private const double CameraWorldZ = 7;
        private const double GuardChaseZ = 2.5;
        private const double GuardIntroEndZ = CameraWorldZ + 0.25;
        private const double GuardIntroFollowTicks = Gruppe2_SubwayField.GuardIntroFollowDurationTicks;
        private const double WorldUnitsPerDistance = 0.1;
        // Narrower than the lane spacing (1.55), so cars in neighbouring lanes never touch.
        private const double CarWorldWidth = 1.34;
        private const double CarRoofHeight = 2.42;
        // Runner and guard are drawn smaller and slimmer so they do not hide the track.
        private const double RunnerHeightScale = 0.68;
        private const double RunnerWidthScale = 0.55;
        // Body angle while flying with the jetpack (almost lying flat in the air).
        private const double JetpackLyingDegrees = 75;

        public string Name { get { return "Gruppe 2 - Subway Surfer Painter"; } }

        // Smooth, arc-shaped lane change of the runner; the camera follows it.
        private readonly Gruppe2_SubwayLaneChange _laneChange = new Gruppe2_SubwayLaneChange();

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            canvas.Focusable = true;
            _laneChange.Reset(RunnerLaneX(currentField));
            Draw(canvas, currentField, _laneChange);
            Keyboard.Focus(canvas);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            _laneChange.Update(RunnerLaneX(currentField));
            Draw(canvas, currentField, _laneChange);
        }

        /// <summary>X position of the lane the rules put the runner on.</summary>
        private static double RunnerLaneX(IGameField currentField)
        {
            IGruppe2_SubwayField field = currentField as IGruppe2_SubwayField;
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

        private static void Draw(Canvas canvas, IGameField currentField, Gruppe2_SubwayLaneChange laneChange)
        {
            IGruppe2_SubwayField field = currentField as IGruppe2_SubwayField;
            canvas.Children.Clear();
            if (field == null)
            {
                return;
            }

            double width = Math.Max(320, canvas.ActualWidth);
            double height = Math.Max(260, canvas.ActualHeight);
            canvas.Background = SkyBrush;

            DrawThreeDimensionalScene(canvas, width, height, field, laneChange);
            DrawHud(canvas, width, height, field);
            if (field.GameOver)
            {
                DrawGameOver(canvas, width, height);
            }
        }

        private static void DrawThreeDimensionalScene(
            Canvas canvas, double width, double height, IGruppe2_SubwayField field, Gruppe2_SubwayLaneChange laneChange)
        {
            Viewport3D viewport = new Viewport3D
            {
                Width = width,
                Height = height,
                ClipToBounds = true,
                IsHitTestVisible = false,
                Camera = new PerspectiveCamera(
                    new Point3D(laneChange.X, 2.2 + RunnerGroundHeight(field), CameraWorldZ),
                    new Vector3D(0, -0.10, -1),
                    new Vector3D(0, 1, 0),
                    55)
            };

            Model3DGroup scene = new Model3DGroup();
            scene.Children.Add(SceneLights);
            AddBox(scene, -18, -0.45, -70, 18, -0.08, 12,
                Material(Color.FromRgb(140, 112, 84)));
            viewport.Children.Add(new ModelVisual3D { Content = scene });
            AddThreeDimensionalTrack(viewport, field.Distance);

            double cityScroll = (field.Distance * 0.1 % CityRepeatDistance) * CityModelScale;
            viewport.Children.Add(CreateCityVisual(cityScroll));
            viewport.Children.Add(CreateCityVisual(cityScroll - CityRepeatDistance * CityModelScale));
            viewport.Children.Add(CreateCityVisual(cityScroll + CityRepeatDistance * CityModelScale));

            // Everything added from here on casts a shadow.
            int firstShadowCaster = viewport.Children.Count;
            Gruppe2_SubwayField animationField = field as Gruppe2_SubwayField;
            int animationTicks = animationField == null ? 0 : animationField.AnimationTicks;
            foreach (Gruppe2_SubwayObstacle obstacle in field.Obstacles)
            {
                AddThreeDimensionalObstacle(viewport, obstacle);
            }

            foreach (Gruppe2_SubwayReward reward in field.Rewards)
            {
                if (reward.Kind == Gruppe2_SubwayRewardKind.Coin)
                {
                    AddThreeDimensionalCoin(viewport, reward, field, laneChange.X, animationTicks);
                }
                else
                {
                    AddThreeDimensionalUpgradeCoin(viewport, reward);
                }
            }

            ModelVisual3D runner = AddThreeDimensionalRunner(viewport, field, laneChange, false, 0);
            if (field.GuardVisible)
            {
                Gruppe2_SubwayField subwayField = field as Gruppe2_SubwayField;
                int guardIntroTicks = subwayField == null ? 0 : subwayField.GuardIntroTicks;
                AddThreeDimensionalRunner(viewport, field, laneChange, true, guardIntroTicks);
            }

            // The runner's shadow falls onto the ramp or roof she stands on; all others onto the track.
            // Shadows are inserted after all world objects but before the runner (and guard):
            // they stand on top of the shadows, never behind them, so transparency stays correct
            // and the runner remains the last visual of the scene.
            double runnerGround = CarRoofHeight * RunnerSupportLevel(field);
            ModelVisual3D shadows = Gruppe2_SubwaySceneLighting.CreateShadows(
                viewport.Children.Skip(firstShadowCaster).OfType<ModelVisual3D>().ToList(),
                caster => caster == runner ? runnerGround : 0);
            viewport.Children.Insert(viewport.Children.IndexOf(runner), shadows);
            canvas.Children.Add(viewport);
        }

        private static void AddThreeDimensionalTrack(Viewport3D viewport, double distance)
        {
            viewport.Children.Add(new ModelVisual3D { Content = TrackModel });
            double sleeperOffset = distance * 0.1 % 1.6;
            for (double z = -64 + sleeperOffset; z < 5; z += 1.6)
            {
                AddWorldModel(viewport, SleeperModel, 0, 0, z, 1);
            }
        }

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
            Viewport3D viewport, Gruppe2_SubwayObstacle obstacle)
        {
            double x = LaneWorldX(obstacle.Lane);
            double z = WorldZ(obstacle.Distance);
            switch (obstacle.Kind)
            {
                case Gruppe2_SubwayObstacleKind.Train:
                    AddTrainPart(viewport, TrainModel, obstacle, false);
                    break;
                case Gruppe2_SubwayObstacleKind.CargoWagon:
                    AddTrainPart(viewport, CargoWagonModel, obstacle, false);
                    break;
                case Gruppe2_SubwayObstacleKind.CargoRamp:
                    // The ramp model rises towards +z; mirrored it rises away from the runner.
                    AddTrainPart(viewport, CargoRampModel, obstacle, true);
                    break;
                case Gruppe2_SubwayObstacleKind.Bush:
                    AddWorldModel(viewport, BushModel, x, 0.78, z, 1.05);
                    break;
                case Gruppe2_SubwayObstacleKind.Signal:
                    AddWorldModel(viewport, SignalModel,
                        (LaneWorldX(Math.Min(obstacle.Lane, 1))
                            + LaneWorldX(Math.Min(obstacle.Lane, 1) + 1)) / 2,
                        0, z, 0.75);
                    break;
                default:
                    AddWorldModel(viewport, ObstacleModels[obstacle.Kind],
                        x, 0, z, 1);
                    break;
            }
        }

        /// <summary>
        /// Fits a train model exactly into the space the rules reserve for it:
        /// centred between the rails of its lane, standing on the track, from its near end
        /// to its far end (Gruppe2_SubwayTrainLayout). All cars share one roof height,
        /// so the runner can walk from wagon to locomotive.
        /// </summary>
        private static void AddTrainPart(
            Viewport3D viewport, Model3D model, Gruppe2_SubwayObstacle obstacle, bool mirrorDepth)
        {
            double nearZ = WorldZ(Gruppe2_SubwayTrainLayout.NearEnd(obstacle));
            double farZ = WorldZ(Gruppe2_SubwayTrainLayout.FarEnd(obstacle));
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
            viewport.Children.Add(new ModelVisual3D { Content = model, Transform = transform });
        }

        private static double WorldZ(double distance)
        {
            return RunnerWorldZ - distance * WorldUnitsPerDistance;
        }

        /// <summary>
        /// Height the runner stands on: track, rising ramp, train roof or the arc
        /// when dropping off a roof. The camera follows this height.
        /// </summary>
        private static double RunnerGroundHeight(IGruppe2_SubwayField field)
        {
            double jetpackHeight = field.JetpackHeight * Gruppe2_SubwayTrainLayout.JetpackAltitudeInRoofHeights;
            return CarRoofHeight * Math.Max(Math.Max(RunnerSupportLevel(field), field.DropHeight), jetpackHeight);
        }

        /// <summary>
        /// Height of the surface below the runner in roof heights: 1 on a train roof,
        /// rising on a ramp, otherwise 0. The runner's shadow falls onto this surface.
        /// </summary>
        private static double RunnerSupportLevel(IGruppe2_SubwayField field)
        {
            return field.Level == Gruppe2_SubwayLevel.TrainRoof
                ? 1
                : Gruppe2_SubwayTrainLayout.RampProgressUnderRunner(field.Obstacles, field.Lane);
        }

        private static Dictionary<Gruppe2_SubwayObstacleKind, Model3DGroup> CreateObstacleModels()
        {
            return new Dictionary<Gruppe2_SubwayObstacleKind, Model3DGroup>
            {
                { Gruppe2_SubwayObstacleKind.SmallBarrier, CreateObstacleModel(Gruppe2_SubwayObstacleKind.SmallBarrier) },
                { Gruppe2_SubwayObstacleKind.LargeBarrier, CreateObstacleModel(Gruppe2_SubwayObstacleKind.LargeBarrier) },
                { Gruppe2_SubwayObstacleKind.PlantPot, CreateObstacleModel(Gruppe2_SubwayObstacleKind.PlantPot) },
                { Gruppe2_SubwayObstacleKind.Curb, CreateObstacleModel(Gruppe2_SubwayObstacleKind.Curb) },
                { Gruppe2_SubwayObstacleKind.CenterPillar, CreateObstacleModel(Gruppe2_SubwayObstacleKind.CenterPillar) }
            };
        }

        private static void AddThreeDimensionalCoin(
            Viewport3D viewport,
            Gruppe2_SubwayReward reward,
            IGruppe2_SubwayField field,
            double runnerX,
            int animationTicks)
        {
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new ScaleTransform3D(0.34, 0.34, 0.10));
            transform.Children.Add(new RotateTransform3D(
                new AxisAngleRotation3D(new Vector3D(0, 1, 0), animationTicks * 11)));
            // Coins of the jetpack trail float at flight level, just above the flying runner.
            double x = LaneWorldX(reward.Lane);
            double y = reward.Elevated
                ? CarRoofHeight * Gruppe2_SubwayTrainLayout.JetpackAltitudeInRoofHeights + 0.6
                : 0.82;

            double pull = MagnetPull(reward, field);
            if (pull > 0)
            {
                // The magnet pulls the coin sideways and up/down towards the runner's chest.
                double runnerChest = RunnerGroundHeight(field) + 0.8;
                x += (runnerX - x) * pull;
                y += (runnerChest - y) * pull;
            }

            transform.Children.Add(new TranslateTransform3D(x, y, WorldZ(reward.Distance)));
            viewport.Children.Add(new ModelVisual3D
            {
                Content = CoinModel,
                Transform = transform
            });
        }

        private static void AddThreeDimensionalUpgradeCoin(
            Viewport3D viewport, Gruppe2_SubwayReward reward)
        {
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new ScaleTransform3D(0.62, 0.62, 0.16));
            transform.Children.Add(new TranslateTransform3D(
                LaneWorldX(reward.Lane), 0.82, WorldZ(reward.Distance)));
            viewport.Children.Add(new ModelVisual3D
            {
                Content = UpgradeCoinModel,
                Transform = transform
            });
        }

        /// <summary>
        /// How far a coin has been pulled towards the runner by the magnet (0 = not at all,
        /// 1 = at the runner). Uses the same conditions as the rules for collecting it.
        /// </summary>
        private static double MagnetPull(Gruppe2_SubwayReward reward, IGruppe2_SubwayField field)
        {
            bool sameLevel = reward.Elevated == (field.JetpackHeight > 0.5);
            if (!field.ActiveUpgrades.ContainsKey(Gruppe2_SubwayUpgradeKind.Magnet) || !sameLevel
                || Math.Abs(reward.Lane - field.Lane) > 1
                || reward.Distance <= 0 || reward.Distance >= Gruppe2_SubwayTrainLayout.MagnetRange)
            {
                return 0;
            }

            double progress = 1 - reward.Distance / Gruppe2_SubwayTrainLayout.MagnetRange;
            return progress * progress * (3 - 2 * progress);
        }

        private static ModelVisual3D AddThreeDimensionalRunner(
            Viewport3D viewport,
            IGruppe2_SubwayField field,
            Gruppe2_SubwayLaneChange laneChange,
            bool guard,
            int guardIntroTicks)
        {
            double x = laneChange.X - (guard ? 0.35 : 0);
            bool guardIntro = guard && guardIntroTicks > 0;
            double introElapsedTicks = guardIntro
                ? Gruppe2_SubwayField.GuardIntroDurationTicks - guardIntroTicks
                : 0;
            double exitElapsedTicks = Math.Max(0, introElapsedTicks - GuardIntroFollowTicks);
            double exitProgress = Math.Min(1, exitElapsedTicks
                / (Gruppe2_SubwayField.GuardIntroDurationTicks - GuardIntroFollowTicks));
            double exitRemaining = 1 - exitProgress;
            double retreatProgress = 1 - exitRemaining * exitRemaining;
            double z = guardIntro
                ? GuardChaseZ + (GuardIntroEndZ - GuardChaseZ) * retreatProgress
                : guard ? GuardChaseZ : RunnerWorldZ;
            double jumpPeak = field.ActiveUpgrades.ContainsKey(Gruppe2_SubwayUpgradeKind.JumpStick)
                ? CarRoofHeight * 1.25
                : 1.15;
            // JumpHeight is 0 unless a jump is running, on the track as well as on a train roof.
            double jump = guard ? 0
                : RunnerGroundHeight(field) + laneChange.Hop + jumpPeak * field.JumpHeight;
            bool hasSkateboard = !guard
                && field.ActiveUpgrades.ContainsKey(Gruppe2_SubwayUpgradeKind.Surfboard);
            bool skating = hasSkateboard && field.Level != Gruppe2_SubwayLevel.Air && field.JumpHeight == 0;
            double introRunPhase = introElapsedTicks + 1 - exitElapsedTicks * exitProgress * 0.5;
            double run = guardIntro
                ? Math.Sin(introRunPhase * 0.48) * exitRemaining
                : Math.Sin(field.Distance * 0.48);
            double movement = skating
                ? Math.Sin(field.Distance * 0.32)
                : run;
            double sideToSide = skating ? movement * 0.08 : 0;
            double animation = guard || !hasSkateboard ? run : skating ? movement : 0;
            bool wearsJetpack = !guard && field.ActiveUpgrades.ContainsKey(Gruppe2_SubwayUpgradeKind.Jetpack);
            double flight = guard ? 0 : field.JetpackHeight;
            Model3DGroup runnerModel = CreateRunnerModel(guard, animation, hasSkateboard,
                wearsJetpack, flight > 0);
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
            ModelVisual3D visual = new ModelVisual3D { Content = runnerModel, Transform = transform };
            viewport.Children.Add(visual);
            return visual;
        }

        private static double LaneWorldX(int lane)
        {
            return (lane - 1) * 1.55;
        }

        private static void AddWorldModel(
            Viewport3D viewport, Model3D model, double x, double y, double z, double scale)
        {
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new ScaleTransform3D(scale, scale, scale));
            transform.Children.Add(new TranslateTransform3D(x, y, z));
            viewport.Children.Add(new ModelVisual3D { Content = model, Transform = transform });
        }

        private static void DrawBackground(Canvas canvas, double width, double height, double distance)
        {
            double center = width / 2;
            AddRectangle(canvas, 0, 0, width, height * TrackHorizonRatio,
                Brush(Color.FromRgb(104, 190, 224)));
            Viewport3D city = new Viewport3D
            {
                Width = width,
                Height = height,
                ClipToBounds = true,
                IsHitTestVisible = false,
                Camera = CreateCityCamera()
            };
            double cityScroll = (distance * 0.25 % CityRepeatDistance) * CityModelScale;
            city.Children.Add(CreateCityVisual(cityScroll));
            city.Children.Add(CreateCityVisual(cityScroll - CityRepeatDistance * CityModelScale));
            city.Children.Add(CreateCityVisual(cityScroll + CityRepeatDistance * CityModelScale));
            canvas.Children.Add(city);

            AddLine(canvas, center - width * 0.38, height * 0.28, center + width * 0.38, height * 0.28, Brushes.DimGray, 3);
            AddLine(canvas, center - width * 0.27, height * 0.20,
                center + width * 0.27, height * 0.20, Brushes.DimGray, 2);
            for (int cable = -2; cable <= 2; cable++)
            {
                double x = center + cable * width * 0.13;
                AddLine(canvas, x, height * 0.05, center + cable * width * 0.045,
                    height * 0.32, Brush(Color.FromRgb(61, 69, 73)), 1.2);
            }
        }

        private static ModelVisual3D CreateCityVisual(double zOffset)
        {
            ModelVisual3D visual = new ModelVisual3D
            {
                Transform = new TranslateTransform3D(0, 0, zOffset)
            };
            visual.Children.Add(new ModelVisual3D
            {
                Content = CityModel,
                Transform = new ScaleTransform3D(
                    CityModelScale, CityModelScale, CityModelScale)
            });
            return visual;
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

        private static Model3DGroup CreateObstacleModel(Gruppe2_SubwayObstacleKind kind)
        {
            Model3DGroup model = new Model3DGroup();
            Material dark = Material(Color.FromRgb(70, 68, 62));
            switch (kind)
            {
                case Gruppe2_SubwayObstacleKind.LargeBarrier:
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
                case Gruppe2_SubwayObstacleKind.SmallBarrier:
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
                case Gruppe2_SubwayObstacleKind.PlantPot:
                    AddBox(model, -0.32, 0, -0.32, 0.32, 0.42, 0.32,
                        Material(Color.FromRgb(166, 83, 48)));
                    AddScaledSphere(model, 0, 0.70, 0, 0.54, 0.48, 0.50,
                        Material(Color.FromRgb(58, 143, 62)));
                    break;
                case Gruppe2_SubwayObstacleKind.Curb:
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
            bool guard, double run, bool hasSkateboard, bool jetpack, bool flying)
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
            AddBox(rider, -0.34, 0.91, -0.18, 0.34, 1.70, 0.18, outfit);
            AddScaledSphere(rider, 0, 1.98, 0, 0.25, 0.31, 0.24, SkinMaterial);
            AddScaledSphere(rider, 0, 2.20, 0, 0.30, 0.12, 0.29, hair);

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

        private static PerspectiveCamera CreateCityCamera()
        {
            Vector3D direction = new Vector3D(0, -0.17, -1);
            direction.Normalize();
            return new PerspectiveCamera(
                new Point3D(0, 8.5, 25),
                direction,
                new Vector3D(0, 1, 0),
                42);
        }

        private static void DrawTrack(Canvas canvas, double width, double height, IGruppe2_SubwayField field)
        {
            double center = width / 2;
            double horizon = height * TrackHorizonRatio;
            double bottom = height * TrackBottomRatio;
            AddPolygon(canvas, new[]
            {
                new Point(0, height * 0.91),
                new Point(center - width * 0.39, height * 0.91),
                new Point(center - width * 0.075, horizon),
                new Point(center + width * 0.075, horizon),
                new Point(center + width * 0.39, height * 0.91),
                new Point(width, height * 0.91),
                new Point(width, height),
                new Point(0, height)
            }, Brush(Color.FromRgb(148, 112, 78)));

            double scroll = field.Distance % 22;
            for (int lane = 0; lane < 3; lane++)
            {
                double centerNear = LaneCenter(width, lane, 1);
                double centerFar = LaneCenter(width, lane, 0);
                double nearHalfWidth = width * 0.075;
                double farHalfWidth = width * 0.012;

                AddPolygon(canvas, new[]
                {
                    new Point(centerFar - farHalfWidth, horizon),
                    new Point(centerFar + farHalfWidth, horizon),
                    new Point(centerNear + nearHalfWidth, bottom),
                    new Point(centerNear - nearHalfWidth, bottom)
                }, TrackBrush, Brush(Color.FromRgb(112, 105, 97)), 1);

                for (int rail = -1; rail <= 1; rail += 2)
                {
                    double farX = centerFar + rail * farHalfWidth * 0.55;
                    double nearX = centerNear + rail * nearHalfWidth * 0.55;
                    AddLine(canvas, farX, horizon, nearX, bottom, RailBrush, 3);
                    AddLine(canvas, farX + width * 0.006, horizon, nearX + width * 0.012,
                        bottom, Brush(Color.FromRgb(111, 77, 50)), 1);
                }

                for (int sleeper = 0; sleeper < 11; sleeper++)
                {
                    double progress = (sleeper + scroll / 22) / 11;
                    double y = horizon + progress * (bottom - horizon);
                    double halfWidth = farHalfWidth + (nearHalfWidth - farHalfWidth) * progress;
                    double laneCenter = LaneCenter(width, lane, progress);
                    AddLine(canvas, laneCenter - halfWidth * 0.82, y,
                        laneCenter + halfWidth * 0.82, y,
                        Brush(Color.FromRgb(123, 80, 52)), 1 + progress * 5);
                }
            }

            AddLine(canvas, center - width * 0.29, height * 0.88,
                center - width * 0.075, horizon, Brush(Color.FromRgb(218, 191, 147)), 2);
            AddLine(canvas, center + width * 0.29, height * 0.88,
                center + width * 0.075, horizon, Brush(Color.FromRgb(218, 191, 147)), 2);
        }

        private static double LaneCenter(double width, int lane, double progress)
        {
            return width / 2 + (lane - 1) * width * (0.018 + 0.212 * progress);
        }

        private static double TrackProgressAtY(double height, double y)
        {
            double horizon = height * TrackHorizonRatio;
            double bottom = height * TrackBottomRatio;
            return Math.Max(0, Math.Min(1, (y - horizon) / (bottom - horizon)));
        }

        private static double TrackHalfWidth(double width, double progress)
        {
            return width * (0.012 + (0.075 - 0.012) * progress);
        }

        private static void DrawWorldObjects(Canvas canvas, double width, double height, IGruppe2_SubwayField field)
        {
            foreach (Gruppe2_SubwayObstacle obstacle in field.Obstacles.OrderByDescending(item => item.Distance))
            {
                bool trainObject = obstacle.Kind == Gruppe2_SubwayObstacleKind.Train
                    || obstacle.Kind == Gruppe2_SubwayObstacleKind.CargoRamp
                    || obstacle.Kind == Gruppe2_SubwayObstacleKind.CargoWagon;
                bool signal = obstacle.Kind == Gruppe2_SubwayObstacleKind.Signal;
                double previewDistance = trainObject ? ObstaclePreviewDistance : signal ? 180 : 100;
                double approach = 1 - Math.Max(0, Math.Min(previewDistance, obstacle.Distance)) / previewDistance;
                double horizon = height * TrackHorizonRatio;
                double trackBottom = height * TrackBottomRatio;
                double y = horizon + approach * (trackBottom - horizon) * ObjectTrackDepthRatio;
                double progress = TrackProgressAtY(height, y);
                double laneHalfWidth = TrackHalfWidth(width, progress);
                double perspectiveScale = laneHalfWidth / (width * 0.075);
                double scale = trainObject
                    ? Math.Max(0.08, laneHalfWidth * 0.98 / 35)
                    : perspectiveScale * 1.1;
                if (signal)
                {
                    int boundary = Math.Min(obstacle.Lane, 1);
                    double leftTrack = LaneCenter(width, boundary, progress);
                    double rightTrack = LaneCenter(width, boundary + 1, progress);
                    DrawSignal(canvas, (leftTrack + rightTrack) / 2, y, scale);
                    continue;
                }

                double centerOffset = obstacle.Kind == Gruppe2_SubwayObstacleKind.CargoWagon
                    ? 1.3
                    : obstacle.Kind == Gruppe2_SubwayObstacleKind.CargoRamp
                        ? 0.8
                        : obstacle.Kind == Gruppe2_SubwayObstacleKind.Train ? 0.74 : 0;
                double objectCenterY = y - 42 * scale * centerOffset;
                double objectProgress = TrackProgressAtY(height, objectCenterY);
                double x = LaneCenter(width, obstacle.Lane, objectProgress);
                double trackAngle = Math.Atan2((1 - obstacle.Lane) * width * 0.212,
                    height * (TrackBottomRatio - TrackHorizonRatio)) * 180 / Math.PI;
                if (obstacle.Moving)
                {
                    x += Math.Sin(obstacle.Distance / 9) * laneHalfWidth * 0.08;
                }

                DrawObstacle(canvas, obstacle.Kind, x, y, scale, trackAngle);
            }

            foreach (Gruppe2_SubwayReward reward in field.Rewards.OrderByDescending(item => item.Distance))
            {
                double approach = 1 - Math.Max(0, Math.Min(100, reward.Distance)) / 100;
                double horizon = height * TrackHorizonRatio;
                double trackBottom = height * TrackBottomRatio;
                double rewardY = horizon
                    + approach * (trackBottom - horizon) * ObjectTrackDepthRatio;
                double progress = TrackProgressAtY(height, rewardY);
                double scale = TrackHalfWidth(width, progress) / (width * 0.075) * 1.05;
                double y = rewardY;
                double x = LaneCenter(width, reward.Lane, progress);
                if (reward.Kind == Gruppe2_SubwayRewardKind.Coin)
                {
                    AddEllipse(canvas, x - 8 * scale, y - 25 * scale, 16 * scale, 16 * scale,
                        Brushes.Gold, Brushes.DarkGoldenrod, 2 * scale);
                    AddEllipse(canvas, x - 4 * scale, y - 21 * scale, 8 * scale, 8 * scale,
                        Brushes.LightGoldenrodYellow, null, 0);
                }
                else
                {
                    AddEllipse(canvas, x - 16 * scale, y - 39 * scale, 32 * scale, 32 * scale,
                        PinkBrush, Brush(Color.FromRgb(160, 18, 92)), 2 * scale);
                    DrawUpgradeIcon(canvas, reward.Upgrade, x, y - 23 * scale, 0.75 * scale);
                }
            }
        }

        private static void DrawUpgradeIcon(
            Canvas canvas,
            Gruppe2_SubwayUpgradeKind upgrade,
            double x,
            double y,
            double scale)
        {
            double size = scale;
            switch (upgrade)
            {
                case Gruppe2_SubwayUpgradeKind.Magnet:
                    AddPolygon(canvas, new[]
                    {
                        new Point(x - 10 * size, y - 11 * size),
                        new Point(x - 5 * size, y - 11 * size),
                        new Point(x - 5 * size, y + size),
                        new Point(x - 4 * size, y + 5 * size),
                        new Point(x - 2 * size, y + 7 * size),
                        new Point(x + 2 * size, y + 7 * size),
                        new Point(x + 4 * size, y + 5 * size),
                        new Point(x + 5 * size, y + size),
                        new Point(x + 5 * size, y - 11 * size),
                        new Point(x + 10 * size, y - 11 * size),
                        new Point(x + 10 * size, y + 2 * size),
                        new Point(x + 8 * size, y + 7 * size),
                        new Point(x + 4 * size, y + 11 * size),
                        new Point(x, y + 12 * size),
                        new Point(x - 4 * size, y + 11 * size),
                        new Point(x - 8 * size, y + 7 * size),
                        new Point(x - 10 * size, y + 2 * size)
                    }, Brush(Color.FromRgb(205, 30, 12)), Brush(Color.FromRgb(150, 25, 8)), 1.2 * size);
                    AddPolygon(canvas, new[]
                    {
                        new Point(x - 10 * size, y - 11 * size),
                        new Point(x - 5 * size, y - 11 * size),
                        new Point(x - 5 * size, y - 3 * size),
                        new Point(x - 9 * size, y - 3 * size)
                    }, Brush(Color.FromRgb(255, 220, 0)), Brush(Color.FromRgb(203, 160, 0)), size);
                    AddPolygon(canvas, new[]
                    {
                        new Point(x + 5 * size, y - 11 * size),
                        new Point(x + 10 * size, y - 11 * size),
                        new Point(x + 9 * size, y - 3 * size),
                        new Point(x + 5 * size, y - 3 * size)
                    }, Brush(Color.FromRgb(255, 220, 0)), Brush(Color.FromRgb(203, 160, 0)), size);
                    break;
                case Gruppe2_SubwayUpgradeKind.DoubleCoins:
                    AddEllipse(canvas, x - 9 * size, y - 5 * size, 12 * size, 12 * size,
                        Brushes.Gold, Brushes.DarkGoldenrod, size);
                    AddEllipse(canvas, x - 2 * size, y - 5 * size, 12 * size, 12 * size,
                        Brushes.Gold, Brushes.DarkGoldenrod, size);
                    AddLine(canvas, x - 3 * size, y - 2 * size, x - 3 * size, y + 4 * size,
                        Brushes.DarkGoldenrod, size);
                    AddLine(canvas, x + 4 * size, y - 2 * size, x + 4 * size, y + 4 * size,
                        Brushes.DarkGoldenrod, size);
                    break;
                case Gruppe2_SubwayUpgradeKind.Surfboard:
                    AddPolygon(canvas, new[]
                    {
                        new Point(x - 4 * size, y - 9 * size),
                        new Point(x + 3 * size, y - 7 * size),
                        new Point(x + 5 * size, y + 7 * size),
                        new Point(x, y + 10 * size),
                        new Point(x - 5 * size, y + 7 * size)
                    }, Brushes.LightCyan, Brushes.White, size);
                    AddLine(canvas, x - 3 * size, y - 2 * size, x + 4 * size, y,
                        Brushes.Teal, 2 * size);
                    break;
                case Gruppe2_SubwayUpgradeKind.JumpStick:
                    AddLine(canvas, x, y - 9 * size, x, y + 3 * size,
                        Brushes.LightGray, 2 * size);
                    AddLine(canvas, x, y + 3 * size, x - 4 * size, y + 5 * size,
                        Brushes.Gold, 2 * size);
                    AddLine(canvas, x - 4 * size, y + 5 * size, x + 4 * size, y + 7 * size,
                        Brushes.Gold, 2 * size);
                    AddLine(canvas, x + 4 * size, y + 7 * size, x - 4 * size, y + 9 * size,
                        Brushes.Gold, 2 * size);
                    AddLine(canvas, x - 4 * size, y + 9 * size, x + 4 * size, y + 11 * size,
                        Brushes.Gold, 2 * size);
                    break;
                case Gruppe2_SubwayUpgradeKind.Jetpack:
                    AddRectangle(canvas, x - 5 * size, y - 6 * size, 10 * size, 12 * size,
                        Brushes.Silver, Brushes.White, size);
                    AddPolygon(canvas, new[]
                    {
                        new Point(x - 4 * size, y + 6 * size),
                        new Point(x, y + 6 * size),
                        new Point(x - 2 * size, y + 11 * size)
                    }, Brushes.OrangeRed);
                    AddPolygon(canvas, new[]
                    {
                        new Point(x + size, y + 6 * size),
                        new Point(x + 5 * size, y + 6 * size),
                        new Point(x + 3 * size, y + 11 * size)
                    }, Brushes.Gold);
                    break;
            }
        }

        private static void DrawObstacle(
            Canvas canvas, Gruppe2_SubwayObstacleKind kind, double x, double y, double scale, double trackAngle)
        {
            double w = 35 * scale;
            double h = 42 * scale;
            switch (kind)
            {
                case Gruppe2_SubwayObstacleKind.Train:
                    DrawTrain(canvas, x, y, w, h, trackAngle);
                    break;
                case Gruppe2_SubwayObstacleKind.CargoRamp:
                    DrawCargoWagon(canvas, x, y, w * 1.12, h * 1.08, trackAngle);
                    break;
                case Gruppe2_SubwayObstacleKind.CargoWagon:
                    DrawClosedCargoWagon(canvas, x, y, w, h, trackAngle);
                    break;
                case Gruppe2_SubwayObstacleKind.LargeBarrier:
                    DrawLargeBarrier(canvas, x, y, w, h, scale);
                    break;
                case Gruppe2_SubwayObstacleKind.Bush:
                    DrawBush(canvas, x, y, w, h, trackAngle);
                    break;
                case Gruppe2_SubwayObstacleKind.PlantPot:
                    AddRectangle(canvas, x - w * 0.65, y - h * 0.65, w * 1.3, h * 0.65,
                        Brush(Color.FromRgb(183, 91, 48)), Brushes.SaddleBrown, 2 * scale);
                    AddEllipse(canvas, x - w * 0.55, y - h * 1.35, w * 0.75, h * 0.75,
                        Brush(Color.FromRgb(67, 155, 72)), null, 0);
                    AddEllipse(canvas, x - w * 0.1, y - h * 1.55, w * 0.65, h * 0.75,
                        Brush(Color.FromRgb(48, 132, 61)), null, 0);
                    break;
                case Gruppe2_SubwayObstacleKind.Curb:
                    AddRectangle(canvas, x - w, y - h * 0.45, w * 2, h * 0.45,
                        Brush(Color.FromRgb(172, 172, 165)), Brushes.DimGray, scale);
                    break;
            }
        }

        private static void DrawLargeBarrier(Canvas canvas, double x, double y, double halfWidth, double height, double scale)
        {
            Brush red = Brush(Color.FromRgb(196, 43, 42));
            Brush white = Brush(Color.FromRgb(238, 236, 220));
            double signTop = y - height * 2.15;
            double signBottom = y - height * 1.05;
            double postWidth = halfWidth * 0.16;

            AddRectangle(canvas, x - halfWidth * 0.92, signTop, halfWidth * 1.84,
                signBottom - signTop, red, Brush(Color.FromRgb(104, 35, 36)), 2 * scale);

            AddPolygon(canvas, new[]
            {
                new Point(x - halfWidth * 0.78, signTop + (signBottom - signTop) * 0.1),
                new Point(x - halfWidth * 0.49, signTop + (signBottom - signTop) * 0.1),
                new Point(x, signTop + (signBottom - signTop) * 0.61),
                new Point(x + halfWidth * 0.49, signTop + (signBottom - signTop) * 0.1),
                new Point(x + halfWidth * 0.78, signTop + (signBottom - signTop) * 0.1),
                new Point(x, signTop + (signBottom - signTop) * 0.86)
            }, white);
            AddPolygon(canvas, new[]
            {
                new Point(x - halfWidth * 0.92, signTop + (signBottom - signTop) * 0.1),
                new Point(x - halfWidth * 0.79, signTop + (signBottom - signTop) * 0.1),
                new Point(x - halfWidth * 0.79, signTop + (signBottom - signTop) * 0.37),
                new Point(x - halfWidth * 0.92, signTop + (signBottom - signTop) * 0.5)
            }, white);
            AddPolygon(canvas, new[]
            {
                new Point(x + halfWidth * 0.79, signTop + (signBottom - signTop) * 0.1),
                new Point(x + halfWidth * 0.92, signTop + (signBottom - signTop) * 0.1),
                new Point(x + halfWidth * 0.92, signTop + (signBottom - signTop) * 0.5),
                new Point(x + halfWidth * 0.79, signTop + (signBottom - signTop) * 0.37)
            }, white);

            AddRectangle(canvas, x - halfWidth * 0.9, signBottom, postWidth, y - signBottom,
                Brush(Color.FromRgb(105, 67, 55)), Brush(Color.FromRgb(59, 48, 44)), scale);
            AddRectangle(canvas, x + halfWidth * 0.74, signBottom, postWidth, y - signBottom,
                Brush(Color.FromRgb(105, 67, 55)), Brush(Color.FromRgb(59, 48, 44)), scale);
            AddRectangle(canvas, x - halfWidth * 0.96, signBottom - height * 0.09,
                halfWidth * 1.92, height * 0.12, white, Brush(Color.FromRgb(104, 35, 36)), scale);

            AddEllipse(canvas, x - halfWidth * 0.8, signTop - height * 0.05,
                halfWidth * 0.22, halfWidth * 0.22, Brush(Color.FromRgb(55, 59, 62)),
                Brush(Color.FromRgb(190, 197, 194)), scale);
            AddEllipse(canvas, x + halfWidth * 0.58, signTop - height * 0.05,
                halfWidth * 0.22, halfWidth * 0.22, Brush(Color.FromRgb(55, 59, 62)),
                Brush(Color.FromRgb(190, 197, 194)), scale);
            AddEllipse(canvas, x - halfWidth * 0.82, signBottom - height * 0.02,
                halfWidth * 0.19, halfWidth * 0.19, Brush(Color.FromRgb(55, 59, 62)),
                Brush(Color.FromRgb(190, 197, 194)), scale);
            AddEllipse(canvas, x + halfWidth * 0.63, signBottom - height * 0.02,
                halfWidth * 0.19, halfWidth * 0.19, Brush(Color.FromRgb(55, 59, 62)),
                Brush(Color.FromRgb(190, 197, 194)), scale);
        }

        private static void DrawCargoWagon(
            Canvas canvas, double x, double y, double halfWidth, double height, double trackAngle)
        {
            double detailWidth = halfWidth * 1.9;
            double detailHeight = height * 2.25;
            Canvas details = new Canvas { Width = detailWidth, Height = detailHeight };
            DrawCargoWagonDetails(details, detailWidth / 2, detailHeight - height * 0.08,
                halfWidth, height);
            DrawTexturedTrainPart(canvas, details, x, y - height * 2.05,
                detailWidth, detailHeight, Color.FromRgb(116, 35, 31),
                detailHeight * 0.45, detailHeight * 0.91, 0.34, trackAngle);
        }

        private static void DrawCargoWagonDetails(Canvas canvas, double x, double y, double halfWidth, double height)
        {
            double rearY = y - height * 2.05;
            double frontY = y - height * 0.12;
            double rearHalfWidth = halfWidth * 0.64;
            double frontHalfWidth = halfWidth * 0.88;
            double wagonScale = halfWidth / 35;
            Brush wagonRed = Brush(Color.FromRgb(194, 55, 42));
            Brush rimRed = Brush(Color.FromRgb(224, 75, 58));
            Brush wagonLight = Brush(Color.FromRgb(211, 69, 53));
            Brush wagonShade = Brush(Color.FromRgb(157, 43, 36));
            Brush inside = Brush(Color.FromRgb(77, 78, 70));

            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth, rearY),
                new Point(x + rearHalfWidth, rearY),
                new Point(x + frontHalfWidth, frontY),
                new Point(x - frontHalfWidth, frontY)
            }, Brush(Color.FromRgb(47, 54, 53)));

            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth, rearY + height * 0.12),
                new Point(x - rearHalfWidth * 0.78, rearY + height * 0.12),
                new Point(x - frontHalfWidth * 0.76, frontY - height * 0.12),
                new Point(x - frontHalfWidth, frontY - height * 0.12)
            }, wagonLight);
            AddPolygon(canvas, new[]
            {
                new Point(x + rearHalfWidth * 0.78, rearY + height * 0.12),
                new Point(x + rearHalfWidth, rearY + height * 0.12),
                new Point(x + frontHalfWidth, frontY - height * 0.12),
                new Point(x + frontHalfWidth * 0.76, frontY - height * 0.12)
            }, wagonShade);
            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth, rearY + height * 0.12),
                new Point(x + rearHalfWidth, rearY + height * 0.12),
                new Point(x + rearHalfWidth * 0.78, rearY + height * 0.34),
                new Point(x - rearHalfWidth * 0.78, rearY + height * 0.34)
            }, rimRed);

            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth * 0.78, rearY + height * 0.18),
                new Point(x + rearHalfWidth * 0.78, rearY + height * 0.18),
                new Point(x + frontHalfWidth * 0.76, frontY - height * 0.18),
                new Point(x - frontHalfWidth * 0.76, frontY - height * 0.18)
            }, inside);
            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth * 0.68, rearY + height * 0.35),
                new Point(x + rearHalfWidth * 0.68, rearY + height * 0.35),
                new Point(x + frontHalfWidth * 0.65, frontY - height * 0.31),
                new Point(x - frontHalfWidth * 0.65, frontY - height * 0.31)
            }, Brush(Color.FromRgb(102, 102, 91)));

            AddPolygon(canvas, new[]
            {
                new Point(x - frontHalfWidth, frontY - height * 0.12),
                new Point(x + frontHalfWidth, frontY - height * 0.12),
                new Point(x + frontHalfWidth * 0.94, frontY + height * 0.20),
                new Point(x - frontHalfWidth * 0.94, frontY + height * 0.20)
            }, wagonRed, Brush(Color.FromRgb(132, 41, 33)), wagonScale * 1.5);

            AddLine(canvas, x - rearHalfWidth, rearY + height * 0.12,
                x - frontHalfWidth, frontY - height * 0.12, rimRed, 3 * wagonScale);
            AddLine(canvas, x + rearHalfWidth, rearY + height * 0.12,
                x + frontHalfWidth, frontY - height * 0.12, rimRed, 3 * wagonScale);
            AddLine(canvas, x - frontHalfWidth * 0.68, frontY + height * 0.08,
                x + frontHalfWidth * 0.68, frontY + height * 0.08,
                Brush(Color.FromRgb(112, 43, 36)), 2 * wagonScale);

            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth * 0.55, rearY + height * 0.56),
                new Point(x - rearHalfWidth * 0.13, rearY + height * 0.42),
                new Point(x + rearHalfWidth * 0.22, rearY + height * 0.54),
                new Point(x + rearHalfWidth * 0.55, rearY + height * 0.45),
                new Point(x + frontHalfWidth * 0.53, frontY - height * 0.48),
                new Point(x + frontHalfWidth * 0.18, frontY - height * 0.34),
                new Point(x - frontHalfWidth * 0.2, frontY - height * 0.48),
                new Point(x - frontHalfWidth * 0.55, frontY - height * 0.36)
            }, Brush(Color.FromRgb(124, 123, 108)));
            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth * 0.45, rearY + height * 0.88),
                new Point(x - rearHalfWidth * 0.08, rearY + height * 0.72),
                new Point(x + rearHalfWidth * 0.27, rearY + height * 0.82),
                new Point(x + frontHalfWidth * 0.4, frontY - height * 0.66),
                new Point(x + frontHalfWidth * 0.05, frontY - height * 0.52),
                new Point(x - frontHalfWidth * 0.3, frontY - height * 0.63)
            }, Brush(Color.FromRgb(145, 143, 125)));
            AddEllipse(canvas, x - halfWidth * 0.16, y - height * 1.1,
                halfWidth * 0.35, height * 0.16, Brush(Color.FromRgb(157, 154, 136)), null, 0);
            AddEllipse(canvas, x + halfWidth * 0.06, y - height * 1.46,
                halfWidth * 0.29, height * 0.13, Brush(Color.FromRgb(91, 94, 86)), null, 0);
        }

        private static void DrawClosedCargoWagon(
            Canvas canvas, double x, double y, double halfWidth, double height, double trackAngle)
        {
            double wagonWidth = halfWidth * 3.25;
            double wagonHeight = height * 3.75;
            double viewportSize = Math.Sqrt(wagonWidth * wagonWidth + wagonHeight * wagonHeight) * 1.05;
            Viewport3D viewport = new Viewport3D
            {
                Width = viewportSize,
                Height = viewportSize,
                ClipToBounds = true,
                IsHitTestVisible = false,
                Camera = new PerspectiveCamera(
                    new Point3D(1.2, 2.6, 8),
                    new Vector3D(-1.2, -2.6, -8),
                    new Vector3D(0, 1, 0),
                    42)
            };
            viewport.Children.Add(new ModelVisual3D { Content = CargoWagonModel });
            viewport.RenderTransformOrigin = new Point(0.5, 0.5);
            viewport.RenderTransform = new RotateTransform(trackAngle);
            Canvas.SetLeft(viewport, x - viewportSize / 2);
            Canvas.SetTop(viewport, y - height * 1.3 - viewportSize / 2);
            canvas.Children.Add(viewport);
        }

        private static Model3DGroup CreateCargoWagonModel()
        {
            Model3DGroup model = new Model3DGroup();
            Material body = Material(Color.FromRgb(95, 31, 70));
            Material roofHighlight = Material(Color.FromRgb(190, 43, 116));
            Material roofMidtone = Material(Color.FromRgb(151, 34, 96));
            Material roofShadow = Material(Color.FromRgb(112, 28, 79));
            Material gold = Material(Color.FromRgb(244, 181, 35));
            Material front = Material(Color.FromRgb(179, 104, 35));
            Material window = Material(Color.FromRgb(35, 83, 133));
            Material glass = Material(Color.FromRgb(91, 174, 218));
            Material dark = Material(Color.FromRgb(36, 38, 48));
            Material lamp = Material(Color.FromRgb(255, 224, 89));

            AddBox(model, -0.78, -0.62, -1.55, 0.78, 0.30, 1.55, body);
            AddBox(model, -0.80, -0.66, 1.55, 0.80, 0.27, 1.59, front);
            AddBox(model, -0.82, -0.24, 1.595, 0.82, -0.12, 1.62, gold);
            AddBox(model, -0.82, -0.58, 1.62, -0.40, -0.32, 1.65, window);
            AddBox(model, -0.33, -0.58, 1.62, 0.09, -0.32, 1.65, window);
            AddBox(model, 0.16, -0.58, 1.62, 0.58, -0.32, 1.65, window);
            AddBox(model, -0.80, -0.28, 1.625, -0.40, -0.24, 1.66, glass);
            AddBox(model, -0.31, -0.28, 1.625, 0.11, -0.24, 1.66, glass);
            AddBox(model, 0.18, -0.28, 1.625, 0.58, -0.24, 1.66, glass);
            AddBox(model, -0.56, -0.72, 1.63, -0.34, -0.61, 1.67, lamp);
            AddBox(model, 0.34, -0.72, 1.63, 0.56, -0.61, 1.67, lamp);
            AddBox(model, -0.82, -0.18, -1.58, 0.82, -0.08, -1.54, gold);
            AddBox(model, -0.84, -0.18, 1.22, 0.84, -0.08, 1.33, gold);

            for (int segment = 0; segment < 12; segment++)
            {
                double start = Math.PI * segment / 12;
                double end = Math.PI * (segment + 1) / 12;
                double startX = 0.80 * Math.Cos(start);
                double startY = 0.27 + 0.53 * Math.Sin(start);
                double endX = 0.80 * Math.Cos(end);
                double endY = 0.27 + 0.53 * Math.Sin(end);
                Material roofMaterial = segment < 4
                    ? roofHighlight
                    : segment < 8 ? roofMidtone : roofShadow;
                AddQuad(model, new Point3D(startX, startY, -1.56),
                    new Point3D(endX, endY, -1.56),
                    new Point3D(endX, endY, 1.56),
                    new Point3D(startX, startY, 1.56), roofMaterial);
                AddTriangle(model, new Point3D(startX, startY, 1.565),
                    new Point3D(endX, endY, 1.565),
                    new Point3D(0, 0.27, 1.565), front);
            }

            AddWheel(model, 0.80, -0.68, -1.0);
            AddWheel(model, 0.80, -0.68, 1.0);
            AddWheel(model, -0.80, -0.68, -1.0);
            AddWheel(model, -0.80, -0.68, 1.0);

            model.Freeze();
            return model;
        }

        private static Model3DGroup CreateBushModel()
        {
            Model3DGroup bush = new Model3DGroup();
            AddBox(bush, -0.15, -0.78, -0.15, 0.15, -0.18, 0.15,
                Material(Color.FromRgb(91, 62, 38)));
            AddBushCluster(bush, -0.43, -0.28, 0.00, 0.48, 0.48, 0.44,
                Color.FromRgb(36, 112, 47));
            AddBushCluster(bush, 0.00, -0.18, 0.00, 0.56, 0.57, 0.50,
                Color.FromRgb(53, 144, 58));
            AddBushCluster(bush, 0.42, -0.30, 0.02, 0.43, 0.45, 0.42,
                Color.FromRgb(41, 124, 49));
            AddBushCluster(bush, -0.20, 0.10, 0.08, 0.37, 0.40, 0.36,
                Color.FromRgb(73, 164, 67));
            AddBushCluster(bush, 0.24, 0.13, 0.10, 0.34, 0.36, 0.34,
                Color.FromRgb(61, 151, 59));
            bush.Freeze();
            return bush;
        }

        private static Model3DGroup CreateSignalModel()
        {
            Model3DGroup signal = new Model3DGroup();
            Material pole = Material(Color.FromRgb(71, 77, 79));
            Material housing = Material(Color.FromRgb(39, 43, 46));
            Material rim = Material(Color.FromRgb(17, 19, 21));

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

            signal.Children.Add(new GeometryModel3D(SphereMesh, backing)
            {
                BackMaterial = backing,
                Transform = new Transform3DGroup
                {
                    Children = new Transform3DCollection
                    {
                        new ScaleTransform3D(0.21, 0.21, 0.10),
                        new TranslateTransform3D(0, y, 0.24)
                    }
                }
            });
            signal.Children.Add(new GeometryModel3D(SphereMesh, lens)
            {
                BackMaterial = lens,
                Transform = new Transform3DGroup
                {
                    Children = new Transform3DCollection
                    {
                        new ScaleTransform3D(0.15, 0.15, 0.12),
                        new TranslateTransform3D(0, y, 0.31)
                    }
                }
            });
        }

        private static void DrawSignal(Canvas canvas, double x, double y, double scale)
        {
            double viewportWidth = 58 * scale;
            double viewportHeight = 126 * scale;
            Viewport3D viewport = new Viewport3D
            {
                Width = viewportWidth,
                Height = viewportHeight,
                ClipToBounds = true,
                IsHitTestVisible = false,
                Camera = new PerspectiveCamera(
                    new Point3D(1.7, 2.1, 7),
                    new Vector3D(-1.7, -1.6, -7),
                    new Vector3D(0, 1, 0),
                    36)
            };
            viewport.Children.Add(new ModelVisual3D { Content = SignalModel });
            Canvas.SetLeft(viewport, x - viewportWidth / 2);
            Canvas.SetTop(viewport, y - viewportHeight);
            canvas.Children.Add(viewport);
        }

        private static void AddBushCluster(
            Model3DGroup bush, double x, double y, double z, double radiusX, double radiusY,
            double radiusZ, Color color)
        {
            Material leaves = Material(color);
            GeometryModel3D cluster = new GeometryModel3D(SphereMesh, leaves)
            {
                BackMaterial = leaves,
                Transform = new Transform3DGroup
                {
                    Children = new Transform3DCollection
                    {
                        new ScaleTransform3D(radiusX, radiusY, radiusZ),
                        new TranslateTransform3D(x, y, z)
                    }
                }
            };
            bush.Children.Add(cluster);
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

        private static void DrawBush(Canvas canvas, double x, double y, double halfWidth, double height, double trackAngle)
        {
            double viewportSize = Math.Max(halfWidth * 3.2, height * 2.8);
            Viewport3D viewport = new Viewport3D
            {
                Width = viewportSize,
                Height = viewportSize,
                ClipToBounds = true,
                IsHitTestVisible = false,
                Camera = new PerspectiveCamera(
                    new Point3D(2.4, 2.8, 5.4),
                    new Vector3D(-2.4, -2.6, -5.4),
                    new Vector3D(0, 1, 0),
                    42)
            };
            viewport.Children.Add(new ModelVisual3D { Content = BushModel });
            viewport.RenderTransformOrigin = new Point(0.5, 0.5);
            viewport.RenderTransform = new RotateTransform(trackAngle);
            Canvas.SetLeft(viewport, x - viewportSize / 2);
            Canvas.SetTop(viewport, y - viewportSize * 0.72);
            canvas.Children.Add(viewport);
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

        private static void DrawTexturedTrainPart(
            Canvas canvas,
            Canvas details,
            double x,
            double top,
            double width,
            double height,
            Color bodyColor,
            double bodyCenterFromTop,
            double bodyHeight,
            double cameraElevation,
            double trackAngle)
        {
            double modelHeight = height / width * 2;
            double bodyCenterY = (0.5 - bodyCenterFromTop / height) * modelHeight;
            double modelBodyHeight = bodyHeight / height * modelHeight;
            Material body = Material(bodyColor);
            Model3DGroup model = new Model3DGroup();
            AddBox(model, -0.86, bodyCenterY - modelBodyHeight / 2, -0.26,
                0.86, bodyCenterY + modelBodyHeight / 2, 0, body);

            VisualBrush faceBrush = new VisualBrush(details)
            {
                Stretch = Stretch.Fill,
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, width, height)
            };
            EmissiveMaterial faceMaterial = new EmissiveMaterial(faceBrush);
            MeshGeometry3D faceMesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection
                {
                    new Point3D(-1, modelHeight / 2, 0.015),
                    new Point3D(1, modelHeight / 2, 0.015),
                    new Point3D(1, -modelHeight / 2, 0.015),
                    new Point3D(-1, -modelHeight / 2, 0.015)
                },
                TextureCoordinates = new PointCollection
                {
                    new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1)
                },
                TriangleIndices = new Int32Collection { 0, 1, 2, 0, 2, 3 }
            };
            faceMesh.Freeze();
            GeometryModel3D face = new GeometryModel3D(faceMesh, faceMaterial)
            {
                BackMaterial = faceMaterial
            };
            model.Children.Add(face);
            model.Children.Add(new AmbientLight(Colors.White));
            model.Children.Add(new DirectionalLight(Colors.White, new Vector3D(-1, -2, -3)));

            Viewport3D viewport = new Viewport3D
            {
                Width = width * 1.3,
                Height = height * 1.3,
                ClipToBounds = true,
                IsHitTestVisible = false,
                Camera = CreateTrainCamera(
                    width * 1.3, height * 1.3, modelHeight * 1.3, cameraElevation)
            };
            viewport.Children.Add(new ModelVisual3D { Content = model });
            viewport.RenderTransformOrigin = new Point(0.5, 0.5);
            viewport.RenderTransform = new RotateTransform(trackAngle);
            Canvas.SetLeft(viewport, x - width * 1.3 / 2);
            Canvas.SetTop(viewport, top - height * 0.15);
            canvas.Children.Add(viewport);
        }

        private static OrthographicCamera CreateTrainCamera(
            double viewportWidth, double viewportHeight, double modelHeight, double elevation)
        {
            Vector3D direction = new Vector3D(-0.15, -elevation, -1);
            direction.Normalize();
            Vector3D right = Vector3D.CrossProduct(direction, new Vector3D(0, 1, 0));
            right.Normalize();
            Vector3D cameraUp = Vector3D.CrossProduct(right, direction);
            cameraUp.Normalize();

            const double modelWidth = 2;
            const double bodyDepth = 0.28;
            double horizontalSpan = Math.Abs(right.X) * modelWidth
                + Math.Abs(right.Y) * modelHeight + Math.Abs(right.Z) * bodyDepth;
            double verticalSpan = Math.Abs(cameraUp.X) * modelWidth
                + Math.Abs(cameraUp.Y) * modelHeight + Math.Abs(cameraUp.Z) * bodyDepth;
            double aspect = viewportWidth / viewportHeight;
            double cameraWidth = Math.Max(horizontalSpan, verticalSpan * aspect) * 1.02;
            Point3D position = new Point3D(-direction.X * 10, -direction.Y * 10, -direction.Z * 10);
            return new OrthographicCamera(position, direction, new Vector3D(0, 1, 0), cameraWidth);
        }

        private static void AddWheel(Model3DGroup model, double x, double y, double z)
        {
            Material wheel = Material(Color.FromRgb(34, 35, 40));
            const int segments = 10;
            for (int segment = 0; segment < segments; segment++)
            {
                double a = 2 * Math.PI * segment / segments;
                double b = 2 * Math.PI * (segment + 1) / segments;
                Point3D a0 = new Point3D(x, y + 0.18 * Math.Sin(a), z + 0.18 * Math.Cos(a));
                Point3D a1 = new Point3D(x, y + 0.18 * Math.Sin(b), z + 0.18 * Math.Cos(b));
                Point3D b0 = new Point3D(x + (x > 0 ? 0.10 : -0.10), a0.Y, a0.Z);
                Point3D b1 = new Point3D(x + (x > 0 ? 0.10 : -0.10), a1.Y, a1.Z);
                AddQuad(model, a0, a1, b1, b0, wheel);
            }
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

        private static void DrawTrain(
            Canvas canvas, double x, double bottom, double halfWidth, double bodyHeight, double trackAngle)
        {
            double sx = 2 * halfWidth / 229;
            double sy = 2.2 * bodyHeight / 281;
            double detailWidth = 2 * halfWidth;
            double detailHeight = 419 * sy;
            Canvas details = new Canvas { Width = detailWidth, Height = detailHeight };
            DrawTrainDetails(details, detailWidth / 2, detailHeight, sx, sy);
            DrawTexturedTrainPart(canvas, details, x, bottom - detailHeight,
                detailWidth, detailHeight, Color.FromRgb(151, 42, 37),
                detailHeight * 0.66, detailHeight * 0.67, 0.18, trackAngle);
        }

        private static void DrawTrainDetails(Canvas canvas, double x, double bottom, double sx, double sy)
        {
            Brush red = Brush(Color.FromRgb(218, 75, 62));
            Brush trim = Brush(Color.FromRgb(48, 69, 72));
            Brush fascia = Brush(Color.FromRgb(190, 203, 200));

            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(86, 50), new Point(153, 20), new Point(169, -118),
                new Point(133, -103), new Point(104, -62)
            }, Brush(Color.FromRgb(178, 52, 45)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(195, 20), new Point(267, 48), new Point(245, -58),
                new Point(214, -99), new Point(180, -118)
            }, Brush(Color.FromRgb(235, 88, 70)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(153, 20), new Point(195, 20), new Point(180, -118),
                new Point(169, -118)
            }, Brush(Color.FromRgb(203, 65, 54)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(116, -44), new Point(139, -71), new Point(145, -15), new Point(128, -7)
            }, Brush(Color.FromRgb(53, 91, 94)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(204, -72), new Point(228, -43), new Point(216, -7), new Point(198, -15)
            }, Brush(Color.FromRgb(53, 91, 94)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(110, -49), new Point(134, -74), new Point(138, -65), new Point(116, -40)
            }, Brush(Color.FromRgb(165, 211, 210)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(207, -65), new Point(229, -40), new Point(225, -31), new Point(203, -56)
            }, Brush(Color.FromRgb(165, 211, 210)));

            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(153, 20), new Point(195, 20), new Point(267, 48), new Point(280, 58),
                new Point(287, 71), new Point(289, 85), new Point(289, 248), new Point(285, 265),
                new Point(276, 280), new Point(259, 291), new Point(239, 299), new Point(111, 299),
                new Point(91, 294), new Point(74, 283), new Point(64, 270), new Point(60, 252),
                new Point(60, 85), new Point(63, 71), new Point(72, 59), new Point(86, 50)
            }, red, Brush(Color.FromRgb(174, 54, 46)), 1.5 * sx);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 153, 20, 42, 34,
                Brush(Color.FromRgb(67, 80, 83)), Brush(Color.FromRgb(45, 54, 57)), sx, 2);
            AddTrainEllipse(canvas, x, bottom, sx, sy, 159, 33, 13, 13,
                Brush(Color.FromRgb(174, 207, 212)));
            AddTrainEllipse(canvas, x, bottom, sx, sy, 176, 33, 13, 13,
                Brush(Color.FromRgb(174, 207, 212)));

            AddTrainRectangle(canvas, x, bottom, sx, sy, 80, 61, 189, 94,
                trim, Brush(Color.FromRgb(35, 54, 57)), 2 * sx, 10);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 88, 69, 80, 79,
                Brush(Color.FromRgb(119, 190, 194)), null, 0, 6);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 180, 69, 80, 79,
                Brush(Color.FromRgb(119, 190, 194)), null, 0, 6);
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(89, 135), new Point(154, 69), new Point(167, 69), new Point(97, 143)
            }, Brush(Color.FromRgb(181, 222, 220)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(181, 135), new Point(246, 69), new Point(259, 69), new Point(189, 143)
            }, Brush(Color.FromRgb(181, 222, 220)));

            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(60, 154), new Point(121, 177), new Point(175, 201), new Point(229, 177),
                new Point(289, 154), new Point(289, 174), new Point(231, 195), new Point(210, 300),
                new Point(140, 300), new Point(119, 195), new Point(60, 174)
            }, fascia);
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(60, 225), new Point(119, 225), new Point(124, 248), new Point(60, 248)
            }, Brush(Color.FromRgb(184, 44, 35)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(231, 225), new Point(289, 225), new Point(289, 248), new Point(226, 248)
            }, Brush(Color.FromRgb(184, 44, 35)));
            AddTrainEllipse(canvas, x, bottom, sx, sy, 76, 198, 17, 17,
                Brush(Color.FromRgb(191, 34, 24)));
            AddTrainEllipse(canvas, x, bottom, sx, sy, 100, 203, 18, 18,
                Brush(Color.FromRgb(255, 202, 47)));
            AddTrainEllipse(canvas, x, bottom, sx, sy, 232, 203, 18, 18,
                Brush(Color.FromRgb(255, 202, 47)));
            AddTrainEllipse(canvas, x, bottom, sx, sy, 256, 198, 17, 17,
                Brush(Color.FromRgb(191, 34, 24)));
            AddTrainRectangle(canvas, x, bottom, sx, sy, 145, 211, 59, 7, trim, null, 0, 0);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 145, 222, 59, 7, trim, null, 0, 0);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 145, 233, 59, 7, trim, null, 0, 0);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 151, 264, 48, 28,
                Brush(Color.FromRgb(24, 58, 61)), Brush(Color.FromRgb(106, 134, 133)), sx, 0);
        }

        private static Point TrainPoint(
            double x,
            double bottom,
            double sx,
            double sy,
            double sourceX,
            double sourceY)
        {
            double upperBodySpread = 1 + 0.10 * Math.Max(0, Math.Min(1, (301 - sourceY) / 281));
            return new Point(x + (sourceX - 174.5) * sx * upperBodySpread, bottom + (sourceY - 301) * sy);
        }

        private static void AddTrainPolygon(
            Canvas canvas,
            double x,
            double bottom,
            double sx,
            double sy,
            IEnumerable<Point> points,
            Brush fill,
            Brush stroke = null,
            double strokeWidth = 0)
        {
            AddPolygon(canvas, points.Select(point =>
                TrainPoint(x, bottom, sx, sy, point.X, point.Y)), fill, stroke, strokeWidth);
        }

        private static void AddTrainRectangle(
            Canvas canvas,
            double x,
            double bottom,
            double sx,
            double sy,
            double sourceX,
            double sourceY,
            double sourceWidth,
            double sourceHeight,
            Brush fill,
            Brush stroke,
            double strokeWidth,
            double radius)
        {
            Point topLeft = TrainPoint(x, bottom, sx, sy, sourceX, sourceY);
            Rectangle shape = new Rectangle
            {
                Width = sourceWidth * sx,
                Height = sourceHeight * sy,
                RadiusX = radius * sx,
                RadiusY = radius * sy,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeWidth
            };
            Canvas.SetLeft(shape, topLeft.X);
            Canvas.SetTop(shape, topLeft.Y);
            canvas.Children.Add(shape);
        }

        private static void AddTrainEllipse(
            Canvas canvas,
            double x,
            double bottom,
            double sx,
            double sy,
            double sourceX,
            double sourceY,
            double sourceWidth,
            double sourceHeight,
            Brush fill)
        {
            Point topLeft = TrainPoint(x, bottom, sx, sy, sourceX, sourceY);
            Ellipse shape = new Ellipse
            {
                Width = sourceWidth * sx,
                Height = sourceHeight * sy,
                Fill = fill
            };
            Canvas.SetLeft(shape, topLeft.X);
            Canvas.SetTop(shape, topLeft.Y);
            canvas.Children.Add(shape);
        }

        private static void DrawRunner(Canvas canvas, double width, double height, IGruppe2_SubwayField field, bool guard)
        {
            double x = LaneCenter(width, field.Lane, 0.82);
            double baseY = height * 0.84;
            double scale = guard ? 0.79 : 1.08;

            if (field.Level == Gruppe2_SubwayLevel.Air && field.JumpHeight > 0)
            {
                baseY -= height * 0.20 * field.JumpHeight;
            }
            else if (field.Level == Gruppe2_SubwayLevel.Air)
            {
                baseY -= height * 0.16;
            }
            else if (field.Level == Gruppe2_SubwayLevel.TrainRoof)
            {
                baseY -= height * 0.06;
            }

            if (guard)
            {
                x -= width * 0.07;
                baseY += height * (0.03 + Math.Abs(Math.Sin(field.Distance / 8)) * 0.035);
            }

            double runCycle = Math.Sin(field.Distance * 0.48);
            double runSwing = runCycle * 13 * scale;
            if (field.Level == Gruppe2_SubwayLevel.Track)
            {
                baseY -= Math.Abs(runCycle) * 2.5 * scale;
            }

            if (!guard && field.Level == Gruppe2_SubwayLevel.Track && field.RollTicks > 0)
            {
                AddEllipse(canvas, x - 23, baseY - 33, 46, 19, PinkBrush, Brushes.DarkMagenta, 2);
                AddEllipse(canvas, x + 4, baseY - 51, 18, 18,
                    Brush(Color.FromRgb(245, 194, 157)), Brushes.Black, 1);
                AddEllipse(canvas, x + 8, baseY - 54, 15, 8, Brushes.DarkMagenta, null, 0);
                AddLine(canvas, x - 20, baseY - 29, x - 34, baseY - 19, PinkBrush, 4);
                return;
            }

            Brush outfit = guard ? Brush(Color.FromRgb(43, 74, 112)) : PinkBrush;
            double leftKneeX = x - 9 * scale + runSwing * 0.45;
            double rightKneeX = x + 9 * scale - runSwing * 0.45;
            double leftFootY = baseY - Math.Max(0, -runCycle) * 7 * scale;
            double rightFootY = baseY - Math.Max(0, runCycle) * 7 * scale;
            AddLine(canvas, x - 5 * scale, baseY - 24 * scale,
                leftKneeX, baseY - 12 * scale, Brushes.Black, 5 * scale);
            AddLine(canvas, leftKneeX, baseY - 12 * scale,
                x - 12 * scale + runSwing, leftFootY, Brushes.Black, 5 * scale);
            AddLine(canvas, x + 5 * scale, baseY - 24 * scale,
                rightKneeX, baseY - 12 * scale, Brushes.Black, 5 * scale);
            AddLine(canvas, rightKneeX, baseY - 12 * scale,
                x + 12 * scale - runSwing, rightFootY, Brushes.Black, 5 * scale);
            AddRectangle(canvas, x - 15 * scale, baseY - 67 * scale, 30 * scale, 42 * scale,
                outfit, Brushes.Black, 1.5 * scale);
            AddLine(canvas, x - 13 * scale, baseY - 58 * scale,
                x - 21 * scale - runSwing * 0.55, baseY - 46 * scale, outfit, 5 * scale);
            AddLine(canvas, x - 21 * scale - runSwing * 0.55, baseY - 46 * scale,
                x - 25 * scale - runSwing, baseY - 34 * scale, outfit, 5 * scale);
            AddLine(canvas, x + 13 * scale, baseY - 58 * scale,
                x + 21 * scale + runSwing * 0.55, baseY - 46 * scale, outfit, 5 * scale);
            AddLine(canvas, x + 21 * scale + runSwing * 0.55, baseY - 46 * scale,
                x + 25 * scale + runSwing, baseY - 34 * scale, outfit, 5 * scale);
            AddEllipse(canvas, x - 12 * scale, baseY - 94 * scale, 24 * scale, 27 * scale,
                Brush(Color.FromRgb(245, 194, 157)), Brushes.Black, scale);
            AddEllipse(canvas, x - 14 * scale, baseY - 99 * scale, 28 * scale, 15 * scale,
                guard ? Brushes.SaddleBrown : Brushes.DarkMagenta, null, 0);
            if (!guard)
            {
                AddLine(canvas, x + 8 * scale, baseY - 88 * scale, x + 16 * scale, baseY - 56 * scale,
                    Brushes.DarkMagenta, 4 * scale);
            }
        }

        private static void DrawHud(Canvas canvas, double width, double height, IGruppe2_SubwayField field)
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

        private static string UpgradeLabel(Gruppe2_SubwayUpgradeKind upgrade)
        {
            switch (upgrade)
            {
                case Gruppe2_SubwayUpgradeKind.Magnet: return "Magnet";
                case Gruppe2_SubwayUpgradeKind.DoubleCoins: return "Stern (Muenzen x2)";
                case Gruppe2_SubwayUpgradeKind.Surfboard: return "Surfboard";
                case Gruppe2_SubwayUpgradeKind.JumpStick: return "Huepfstab";
                case Gruppe2_SubwayUpgradeKind.Jetpack: return "Jetpack";
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

        private static void AddLine(Canvas canvas, double x1, double y1, double x2, double y2,
            Brush brush, double thickness)
        {
            Line line = new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = thickness };
            canvas.Children.Add(line);
        }

        private static void AddPolygon(Canvas canvas, IEnumerable<Point> points, Brush fill,
            Brush stroke = null, double strokeWidth = 0)
        {
            Polygon polygon = new Polygon
            {
                Points = new PointCollection(points),
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeWidth
            };
            canvas.Children.Add(polygon);
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
