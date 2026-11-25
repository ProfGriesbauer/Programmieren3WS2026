using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // Floppy disk going into the disk drive: it comes in from the front left, lies down flat with the label up,
    // slides into the slot, then the latch lever turns shut and the drive light comes on.
    public sealed class G10_CommodoreDiskAnimation : G10_MediaAnimation
    {
        // Poses of the disk's leading edge in drive millimetres (G10_CommodoreModel.MediaPlacement): origin in the
        // middle of the slot, z out of the front. Tilt -90 and turn 180 lay the disk flat with the label up,
        // reaching from the leading edge towards the viewer.
        static readonly G10_MediaPose StartPose = new G10_MediaPose(-200, 60, 330, -40, 160);
        static readonly G10_MediaPose HoverPose = new G10_MediaPose(0, 0, 12, -90, 180);
        static readonly G10_MediaPose SeatedPose = new G10_MediaPose(0, 0, -G10_CommodoreDiskModel.Size - 5, -90, 180);
        const double LatchClosed = 90;

        static readonly Material LightOff = G10_AtariMesh.Plastic(Color.FromRgb(96, 26, 22));
        static readonly Material LightOn = CreateLightOn();

        readonly AxisAngleRotation3D _Latch = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0);
        readonly GeometryModel3D _Light;

        public G10_CommodoreDiskAnimation() : base(G10_RoomDeviceKind.Commodore, G10_CommodoreModel.MediaPlacement)
        {
            Matrix3D place = G10_CommodoreModel.MediaPlacement;

            // Latch lever above the middle of the slot: lying along the slot when open (the disk passes below it),
            // turned upright across the slot when the disk is locked in.
            Model3DGroup latch = new Model3DGroup();
            G10_AtariMesh.Box(latch, Matrix3D.Identity, 0, 0, 5, 24, 7, 5, G10_AtariMesh.Plastic(Color.FromRgb(150, 138, 112)));
            G10_AtariMesh.CylinderZ(latch, Matrix3D.Identity, 0, 0, 4.5, 7.5, 9, G10_AtariMesh.Plastic(Color.FromRgb(120, 110, 90)));
            Transform3DGroup latchTransform = new Transform3DGroup();
            latchTransform.Children.Add(new RotateTransform3D(_Latch));
            latchTransform.Children.Add(new TranslateTransform3D(0, 10, 0));
            latchTransform.Children.Add(new MatrixTransform3D(place));
            latch.Transform = latchTransform;
            Parts.Children.Add(latch);

            // Drive light, on top of the model's red LED.
            Model3DGroup light = new Model3DGroup();
            _Light = G10_AtariMesh.Box(light, place, 64.3, -32.1, 2.6, 9.5, 9.5, 0.8, LightOff);
            Parts.Children.Add(light);
        }

        // From the front right (the post at the desk corner is in the way from the left), close to the drive;
        // the target lies right of the slot to keep it clear of the sidebar.
        public override G10_CameraView InsertView
        {
            get
            {
                Point3D slot = G10_CommodoreModel.MediaPlacement.Transform(new Point3D(0, 0, 0));
                return new G10_CameraView(slot + new Vector3D(0.13, 0.03, 0.0), 0.9, 0.25, 1.2);
            }
        }

        protected override G10_MediaPose Start { get { return StartPose; } }
        protected override G10_MediaPose Hover { get { return HoverPose; } }
        protected override G10_MediaPose Seated { get { return SeatedPose; } }
        protected override double FlightArc { get { return 40; } }
        protected override double ClickDepth { get { return 3; } }

        protected override Model3DGroup CreateMedium(G10_RoomGameEntry game)
        {
            return G10_CommodoreDiskModel.Create(game);
        }

        protected override void SetPower(double on)
        {
            double angle = LatchClosed * on;
            if (_Latch.Angle != angle) _Latch.Angle = angle;
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
            material.Children.Add(new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 50, 35))));
            material.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color.FromRgb(255, 70, 45))));
            material.Freeze();
            return material;
        }
    }
}
