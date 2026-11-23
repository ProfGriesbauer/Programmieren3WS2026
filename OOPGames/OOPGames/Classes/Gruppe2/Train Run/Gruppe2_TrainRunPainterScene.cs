using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    /// <summary>
    /// Retained 3D scene of the Train Run painter: the viewport, camera, lights, ground,
    /// tracks, sleepers and city are created once and only moved from frame to frame.
    /// Game objects are kept in a <see cref="Gruppe2_TrainRunVisualPool"/>.
    /// </summary>
    public sealed partial class Gruppe2_TrainRunPainter
    {
        private const double SleeperSpacing = 1.6;

        // Runner models per animation phase, so they are not rebuilt in every frame.
        private const int RunnerAnimationSteps = 8;
        private static readonly Dictionary<RunnerModelKey, Model3DGroup> RunnerModelCache =
            new Dictionary<RunnerModelKey, Model3DGroup>();

        private readonly RetainedScene _scene = new RetainedScene();

        private struct RunnerModelKey : IEquatable<RunnerModelKey>
        {
            public bool Guard;
            public int Phase;
            public bool HasSkateboard;
            public bool Jetpack;
            public bool Flying;
            public JumpStickPose JumpStick;

            public bool Equals(RunnerModelKey other)
            {
                return Guard == other.Guard && Phase == other.Phase && HasSkateboard == other.HasSkateboard
                    && Jetpack == other.Jetpack && Flying == other.Flying && JumpStick == other.JumpStick;
            }

            public override bool Equals(object obj)
            {
                return obj is RunnerModelKey && Equals((RunnerModelKey)obj);
            }

            public override int GetHashCode()
            {
                return ((((Phase * 31 + (Guard ? 1 : 0)) * 31 + (HasSkateboard ? 1 : 0)) * 31
                    + (Jetpack ? 1 : 0)) * 31 + (Flying ? 1 : 0)) * 31 + (int)JumpStick;
            }
        }

        /// <summary>
        /// Runner model for the given pose. The animation value (-1 .. 1) is rounded to a few
        /// steps, so the same frozen model can be reused instead of building a new one each frame.
        /// </summary>
        private static Model3DGroup GetRunnerModel(
            bool guard, double run, bool hasSkateboard, bool jetpack, bool flying, JumpStickPose jumpStick)
        {
            RunnerModelKey key = new RunnerModelKey
            {
                Guard = guard,
                Phase = (int)Math.Round(run * RunnerAnimationSteps),
                HasSkateboard = hasSkateboard,
                Jetpack = jetpack,
                Flying = flying,
                JumpStick = jumpStick
            };

            Model3DGroup model;
            if (!RunnerModelCache.TryGetValue(key, out model))
            {
                model = CreateRunnerModel(guard, key.Phase / (double)RunnerAnimationSteps,
                    hasSkateboard, jetpack, flying, jumpStick);
                RunnerModelCache[key] = model;
            }

            return model;
        }

        private sealed class RetainedScene
        {
            private readonly PerspectiveCamera _camera = new PerspectiveCamera(
                new Point3D(0, 2.2, CameraWorldZ), new Vector3D(0, -0.10, -1), new Vector3D(0, 1, 0), 55);
            private readonly TranslateTransform3D _sleeperOffset = new TranslateTransform3D();
            private readonly TranslateTransform3D[] _cityOffsets =
            {
                new TranslateTransform3D(), new TranslateTransform3D(), new TranslateTransform3D()
            };

            public RetainedScene()
            {
                Viewport = new Viewport3D
                {
                    ClipToBounds = true,
                    IsHitTestVisible = false,
                    Camera = _camera
                };

                Model3DGroup ground = new Model3DGroup();
                ground.Children.Add(SceneLights);
                AddBox(ground, -18, -0.45, -70, 18, -0.08, 12, Material(Color.FromRgb(140, 112, 84)));
                ground.Children.Add(TrackModel);
                ground.Freeze();
                Viewport.Children.Add(new ModelVisual3D { Content = ground });

                // All sleepers in one model that moves as a whole with the running distance.
                Model3DGroup sleepers = new Model3DGroup();
                for (double z = -64 - SleeperSpacing; z < 5; z += SleeperSpacing)
                {
                    sleepers.Children.Add(new Model3DGroup
                    {
                        Children = { SleeperModel },
                        Transform = new TranslateTransform3D(0, 0, z)
                    });
                }

                sleepers.Freeze();
                Viewport.Children.Add(new ModelVisual3D { Content = sleepers, Transform = _sleeperOffset });

                foreach (TranslateTransform3D cityOffset in _cityOffsets)
                {
                    ModelVisual3D city = new ModelVisual3D { Transform = cityOffset };
                    city.Children.Add(new ModelVisual3D
                    {
                        Content = CityModel,
                        Transform = new ScaleTransform3D(CityModelScale, CityModelScale, CityModelScale)
                    });
                    Viewport.Children.Add(city);
                }

                FirstDynamicIndex = Viewport.Children.Count;
            }

            public Viewport3D Viewport { get; }

            public Gruppe2_TrainRunVisualPool Pool { get; } = new Gruppe2_TrainRunVisualPool();

            /// <summary>Index of the first game object in the viewport (everything before is static).</summary>
            public int FirstDynamicIndex { get; }

            /// <summary>Moves camera, sleepers and city to the current state; nothing is rebuilt.</summary>
            public void Update(double width, double height, IGruppe2_TrainRunField field, Gruppe2_TrainRunLaneChange laneChange)
            {
                Viewport.Width = width;
                Viewport.Height = height;
                _camera.Position = new Point3D(laneChange.X, CameraHeight(field), CameraWorldZ);
                _sleeperOffset.OffsetZ = field.Distance * WorldUnitsPerDistance % SleeperSpacing;

                double cityScroll = (field.Distance * 0.1 % CityRepeatDistance) * CityModelScale;
                double repeat = CityRepeatDistance * CityModelScale;
                _cityOffsets[0].OffsetZ = cityScroll;
                _cityOffsets[1].OffsetZ = cityScroll - repeat;
                _cityOffsets[2].OffsetZ = cityScroll + repeat;
            }

            /// <summary>Puts the viewport into the canvas (once) and removes the old overlay elements.</summary>
            public void AttachTo(Canvas canvas)
            {
                Panel currentParent = VisualTreeHelper.GetParent(Viewport) as Panel;
                if (currentParent != null && currentParent != canvas)
                {
                    currentParent.Children.Remove(Viewport);
                }

                for (int index = canvas.Children.Count - 1; index >= 0; index--)
                {
                    if (canvas.Children[index] != Viewport)
                    {
                        canvas.Children.RemoveAt(index);
                    }
                }

                if (!canvas.Children.Contains(Viewport))
                {
                    canvas.Children.Insert(0, Viewport);
                }
            }
        }
    }
}
