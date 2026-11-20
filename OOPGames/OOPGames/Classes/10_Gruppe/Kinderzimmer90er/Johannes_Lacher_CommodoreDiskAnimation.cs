using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // Floppy disk going into the disk drive: it comes in from the front left, lies down flat with the label up,
    // slides into the slot, then the latch lever turns shut and the drive light comes on.
    public sealed class B5CommodoreDiskAnimation : B5MediaAnimation
    {
        // Poses of the disk's leading edge in drive millimetres (B5CommodoreModel.MediaPlacement): origin in the
        // middle of the slot, z out of the front. Tilt -90 and turn 180 lay the disk flat with the label up,
        // reaching from the leading edge towards the viewer.
        static readonly B5MediaPose StartPose = new B5MediaPose(-200, 60, 330, -40, 160);
        static readonly B5MediaPose HoverPose = new B5MediaPose(0, 0, 12, -90, 180);
        static readonly B5MediaPose SeatedPose = new B5MediaPose(0, 0, -B5CommodoreDiskModel.Size - 5, -90, 180);
        const double LatchClosed = 90;

        static readonly Material LightOff = B5AtariMesh.Plastic(Color.FromRgb(96, 26, 22));
        static readonly Material LightOn = CreateLightOn();

        readonly AxisAngleRotation3D _Latch = new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0);
        readonly GeometryModel3D _Light;

        public B5CommodoreDiskAnimation() : base(B5RoomDeviceKind.Commodore, B5CommodoreModel.MediaPlacement)
        {
            Matrix3D place = B5CommodoreModel.MediaPlacement;

            // Latch lever above the middle of the slot: lying along the slot when open (the disk passes below it),
            // turned upright across the slot when the disk is locked in.
            Model3DGroup latch = new Model3DGroup();
            B5AtariMesh.Box(latch, Matrix3D.Identity, 0, 0, 5, 24, 7, 5, B5AtariMesh.Plastic(Color.FromRgb(150, 138, 112)));
            B5AtariMesh.CylinderZ(latch, Matrix3D.Identity, 0, 0, 4.5, 7.5, 9, B5AtariMesh.Plastic(Color.FromRgb(120, 110, 90)));
            Transform3DGroup latchTransform = new Transform3DGroup();
            latchTransform.Children.Add(new RotateTransform3D(_Latch));
            latchTransform.Children.Add(new TranslateTransform3D(0, 10, 0));
            latchTransform.Children.Add(new MatrixTransform3D(place));
            latch.Transform = latchTransform;
            Parts.Children.Add(latch);

            // Drive light, on top of the model's red LED.
            Model3DGroup light = new Model3DGroup();
            _Light = B5AtariMesh.Box(light, place, 64.3, -32.1, 2.6, 9.5, 9.5, 0.8, LightOff);
            Parts.Children.Add(light);
        }

        // From the front right (the post at the desk corner is in the way from the left), close to the drive;
        // the target lies right of the slot to keep it clear of the sidebar.
        public override B5CameraView InsertView
        {
            get
            {
                Point3D slot = B5CommodoreModel.MediaPlacement.Transform(new Point3D(0, 0, 0));
                return new B5CameraView(slot + new Vector3D(0.13, 0.03, 0.0), 0.9, 0.25, 1.2);
            }
        }

        protected override B5MediaPose Start { get { return StartPose; } }
        protected override B5MediaPose Hover { get { return HoverPose; } }
        protected override B5MediaPose Seated { get { return SeatedPose; } }
        protected override double FlightArc { get { return 40; } }
        protected override double ClickDepth { get { return 3; } }

        protected override Model3DGroup CreateMedium(B5RoomGameEntry game)
        {
            return B5CommodoreDiskModel.Create(game);
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
