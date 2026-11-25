using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // Game Boy cartridge: it comes in from the left with the label towards the player, turns round over the top
    // of the Game Boy (the label faces the back, as on the real device) and slides down into the slot at the back.
    // Then the power switch on top slides to ON and the red battery light comes on.
    public sealed class G10_GameBoyCartridgeAnimation : G10_MediaAnimation
    {
        // Poses of the cartridge's bottom edge in Game Boy millimetres (G10_GameBoyModel.MediaPlacement):
        // origin in the middle of the top edge of the front face, y up, z out of the front.
        const double SlotZ = -G10_GameBoyModel.Depth + G10_GameBoyCartridgeModel.Thickness / 2 - 1;
        static readonly G10_MediaPose StartPose = new G10_MediaPose(-150, -45, 60, -25, 0);
        static readonly G10_MediaPose HoverPose = new G10_MediaPose(0, 8, SlotZ, 0, 180);
        static readonly G10_MediaPose SeatedPose = new G10_MediaPose(0, 5 - G10_GameBoyCartridgeModel.Height, SlotZ, 0, 180);
        const double SwitchOff = -32, SwitchOn = -23;

        static readonly Material LightOff = G10_AtariMesh.Plastic(Color.FromRgb(120, 22, 28));
        static readonly Material LightOn = CreateLightOn();

        readonly TranslateTransform3D _Switch = new TranslateTransform3D();
        readonly GeometryModel3D _Light;

        public G10_GameBoyCartridgeAnimation() : base(G10_RoomDeviceKind.Gameboy, G10_GameBoyModel.MediaPlacement)
        {
            Matrix3D place = G10_GameBoyModel.MediaPlacement;
            Material dark = G10_AtariMesh.Plastic(Color.FromRgb(62, 61, 58));

            // Opening of the cartridge slot on top at the back, and the groove of the power switch at the top left.
            G10_AtariMesh.Box(Parts, place, 0, 0.15, SlotZ, G10_GameBoyCartridgeModel.Width + 2, 0.3, G10_GameBoyCartridgeModel.Thickness + 1, dark);
            G10_AtariMesh.Box(Parts, place, (SwitchOff + SwitchOn) / 2, 0.15, -9, SwitchOn - SwitchOff + 8, 0.3, 5, dark);

            Model3DGroup slider = new Model3DGroup();
            G10_AtariMesh.Box(slider, Matrix3D.Identity, 0, 1.4, -9, 6, 2.8, 4, G10_AtariMesh.Plastic(Color.FromRgb(150, 148, 140)));
            Transform3DGroup sliderTransform = new Transform3DGroup();
            sliderTransform.Children.Add(_Switch);
            sliderTransform.Children.Add(new MatrixTransform3D(place));
            slider.Transform = sliderTransform;
            Parts.Children.Add(slider);

            // Battery light on the front, left of the screen (it sits on the model's dark red dot).
            Model3DGroup light = new Model3DGroup();
            _Light = G10_AtariMesh.CylinderZ(light, place, -32, -37, 1.7, 1.9, 2.3, LightOff);
            Parts.Children.Add(light);
            _Switch.OffsetX = SwitchOff;
        }

        // From the front right and above, so the cartridge can be seen turning round and going in at the back.
        // The target lies right of the Game Boy, which keeps it clear of the sidebar.
        public override G10_CameraView InsertView
        {
            get
            {
                Point3D top = G10_GameBoyModel.MediaPlacement.Transform(new Point3D(0, 10, -G10_GameBoyModel.Depth / 2));
                return new G10_CameraView(top + new Vector3D(0.17, 0, -0.08), 1.25, 0.45, 1.15);
            }
        }

        protected override G10_MediaPose Start { get { return StartPose; } }
        protected override G10_MediaPose Hover { get { return HoverPose; } }
        protected override G10_MediaPose Seated { get { return SeatedPose; } }
        protected override double FlightArc { get { return 20; } }
        protected override double ClickDepth { get { return 1.5; } }

        protected override Model3DGroup CreateMedium(G10_RoomGameEntry game)
        {
            return G10_GameBoyCartridgeModel.Create(game);
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
