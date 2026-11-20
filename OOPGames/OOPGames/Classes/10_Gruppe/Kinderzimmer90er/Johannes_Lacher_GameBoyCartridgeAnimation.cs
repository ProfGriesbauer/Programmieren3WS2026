using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // Game Boy cartridge: it comes in from the left with the label towards the player, turns round over the top
    // of the Game Boy (the label faces the back, as on the real device) and slides down into the slot at the back.
    // Then the power switch on top slides to ON and the red battery light comes on.
    public sealed class B5GameBoyCartridgeAnimation : B5MediaAnimation
    {
        // Poses of the cartridge's bottom edge in Game Boy millimetres (B5GameBoyModel.MediaPlacement):
        // origin in the middle of the top edge of the front face, y up, z out of the front.
        const double SlotZ = -B5GameBoyModel.Depth + B5GameBoyCartridgeModel.Thickness / 2 - 1;
        static readonly B5MediaPose StartPose = new B5MediaPose(-150, -45, 60, -25, 0);
        static readonly B5MediaPose HoverPose = new B5MediaPose(0, 8, SlotZ, 0, 180);
        static readonly B5MediaPose SeatedPose = new B5MediaPose(0, 5 - B5GameBoyCartridgeModel.Height, SlotZ, 0, 180);
        const double SwitchOff = -32, SwitchOn = -23;

        static readonly Material LightOff = B5AtariMesh.Plastic(Color.FromRgb(120, 22, 28));
        static readonly Material LightOn = CreateLightOn();

        readonly TranslateTransform3D _Switch = new TranslateTransform3D();
        readonly GeometryModel3D _Light;

        public B5GameBoyCartridgeAnimation() : base(B5RoomDeviceKind.Gameboy, B5GameBoyModel.MediaPlacement)
        {
            Matrix3D place = B5GameBoyModel.MediaPlacement;
            Material dark = B5AtariMesh.Plastic(Color.FromRgb(62, 61, 58));

            // Opening of the cartridge slot on top at the back, and the groove of the power switch at the top left.
            B5AtariMesh.Box(Parts, place, 0, 0.15, SlotZ, B5GameBoyCartridgeModel.Width + 2, 0.3, B5GameBoyCartridgeModel.Thickness + 1, dark);
            B5AtariMesh.Box(Parts, place, (SwitchOff + SwitchOn) / 2, 0.15, -9, SwitchOn - SwitchOff + 8, 0.3, 5, dark);

            Model3DGroup slider = new Model3DGroup();
            B5AtariMesh.Box(slider, Matrix3D.Identity, 0, 1.4, -9, 6, 2.8, 4, B5AtariMesh.Plastic(Color.FromRgb(150, 148, 140)));
            Transform3DGroup sliderTransform = new Transform3DGroup();
            sliderTransform.Children.Add(_Switch);
            sliderTransform.Children.Add(new MatrixTransform3D(place));
            slider.Transform = sliderTransform;
            Parts.Children.Add(slider);

            // Battery light on the front, left of the screen (it sits on the model's dark red dot).
            Model3DGroup light = new Model3DGroup();
            _Light = B5AtariMesh.CylinderZ(light, place, -32, -37, 1.7, 1.9, 2.3, LightOff);
            Parts.Children.Add(light);
            _Switch.OffsetX = SwitchOff;
        }

        // From the front right and above, so the cartridge can be seen turning round and going in at the back.
        // The target lies right of the Game Boy, which keeps it clear of the sidebar.
        public override B5CameraView InsertView
        {
            get
            {
                Point3D top = B5GameBoyModel.MediaPlacement.Transform(new Point3D(0, 10, -B5GameBoyModel.Depth / 2));
                return new B5CameraView(top + new Vector3D(0.17, 0, -0.08), 1.25, 0.45, 1.15);
            }
        }

        protected override B5MediaPose Start { get { return StartPose; } }
        protected override B5MediaPose Hover { get { return HoverPose; } }
        protected override B5MediaPose Seated { get { return SeatedPose; } }
        protected override double FlightArc { get { return 20; } }
        protected override double ClickDepth { get { return 1.5; } }

        protected override Model3DGroup CreateMedium(B5RoomGameEntry game)
        {
            return B5GameBoyCartridgeModel.Create(game);
        }

        protected override void SetPower(double on)
        {
            double x = SwitchOff + (SwitchOn - SwitchOff) * on;
            if (_Switch.OffsetX != x) _Switch.OffsetX = x;
            Material light = on >= 1 ? LightOn : LightOff;
            if (_Light.Material != light)
            {
                _Light.Material = light;
                _Light.BackMaterial = light;
            }
        }

        static Material CreateLightOn()
        {
            MaterialGroup material = new MaterialGroup();
            material.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 40, 30))));
            material.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color.FromRgb(255, 60, 40))));
            material.Freeze();
            return material;
        }
    }
}
