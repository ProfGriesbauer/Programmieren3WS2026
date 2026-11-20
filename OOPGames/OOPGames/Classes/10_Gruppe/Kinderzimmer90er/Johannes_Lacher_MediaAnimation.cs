using System;
using System.Collections.Generic;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // Position and rotation of a game medium (cartridge or disk) in the millimetre space of its device.
    // Tilt turns around x, Turn around y (degrees); Position is the origin of the medium model.
    public struct B5MediaPose
    {
        public B5MediaPose(double x, double y, double z, double tilt, double turn)
        {
            Position = new Vector3D(x, y, z);
            Tilt = tilt;
            Turn = turn;
        }

        public Vector3D Position { get; private set; }
        public double Tilt { get; private set; }
        public double Turn { get; private set; }
    }

    // Orbit camera around a target, in the same terms as B5RoomPaint.UpdateCamera.
    public struct B5CameraView
    {
        public B5CameraView(Point3D target, double distance, double azimuth, double polar)
        {
            Target = target;
            Distance = distance;
            Azimuth = azimuth;
            Polar = polar;
        }

        public Point3D Target { get; private set; }
        public double Distance { get; private set; }
        public double Azimuth { get; private set; }
        public double Polar { get; private set; }
    }

    // 3D game medium of one device with its insert and eject animation. The timeline is the same for every
    // device: the medium flies in from Start (hidden until the camera has arrived), turns into the Hover pose
    // in front of the slot, is pushed in to Seated, clicks, then the device switches on. Ejecting plays this
    // backwards and faster. While a game is inserted its medium stays in the slot.
    // Subclasses provide the poses, the medium model, the camera view and the device's moving parts.
    public abstract class B5MediaAnimation
    {
        readonly int _Device;
        readonly Model3DGroup _Root = new Model3DGroup();
        readonly Model3DGroup _Medium = new Model3DGroup();
        readonly AxisAngleRotation3D _Tilt = new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0);
        readonly AxisAngleRotation3D _Turn = new AxisAngleRotation3D(new Vector3D(0, 1, 0), 0);
        readonly TranslateTransform3D _Position = new TranslateTransform3D();
        readonly Dictionary<B5RoomGameEntry, Model3DGroup> _Models = new Dictionary<B5RoomGameEntry, Model3DGroup>();
        B5RoomGameEntry _Shown;

        protected B5MediaAnimation(B5RoomDeviceKind device, Matrix3D placement)
        {
            _Device = (int)device;
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new RotateTransform3D(_Tilt));
            transform.Children.Add(new RotateTransform3D(_Turn));
            transform.Children.Add(_Position);
            transform.Children.Add(new MatrixTransform3D(placement));
            _Medium.Transform = transform;
            _Root.Children.Add(_Medium);
        }

        public Model3D Model { get { return _Root; } }

        // Camera while the medium goes in or comes out.
        public abstract B5CameraView InsertView { get; }

        // Moving parts of the device (switches, lights, flaps) are added here by the subclasses.
        protected Model3DGroup Parts { get { return _Root; } }

        protected abstract B5MediaPose Start { get; }
        protected abstract B5MediaPose Hover { get; }
        protected abstract B5MediaPose Seated { get; }
        protected virtual double FlightArc { get { return 18; } }
        protected virtual double ClickDepth { get { return 2.5; } }

        protected abstract Model3DGroup CreateMedium(B5RoomGameEntry game);

        // 0 = device off, 1 = on (power switch, latch, lights).
        protected abstract void SetPower(double on);

        // 0 = medium in front of the slot or away, 1 = fully inserted (flaps and similar parts follow it).
        protected virtual void SetInsertDepth(double depth) { }

        public void Update(B5RoomField field)
        {
            IList<B5RoomGameEntry> games = B5RoomRegistry.Devices[_Device].Games;
            bool selected = field.SelectedDevice == _Device;
            int inserted = field.GetInsertedGame(_Device);
            double progress = field.TransitionProgress;

            if (selected && field.Phase == B5RoomPhase.Inserting && field.PendingGame >= 0)
            {
                // The flight starts once the camera has mostly arrived; until then the medium is not shown.
                double fly = Ease(Section(progress, 0.18, 0.55));
                double push = Ease(Section(progress, 0.58, 0.76));
                Show(fly > 0 ? games[field.PendingGame] : null);
                if (push <= 0) Fly(fly);
                else Push(push, Math.Sin(Math.PI * Section(progress, 0.76, 0.84)));
                SetPower(Ease(Section(progress, 0.88, 0.95)));
            }
            else if (selected && field.Phase == B5RoomPhase.Ejecting && inserted >= 0)
            {
                double fly = 1 - Ease(Section(progress, 0.5, 1));
                Show(fly > 0 ? games[inserted] : null);
                SetPower(1 - Ease(Section(progress, 0, 0.18)));
                if (progress < 0.5) Push(1 - Ease(Section(progress, 0.15, 0.5)), 0);
                else Fly(fly);
            }
            else if (inserted >= 0)
            {
                Show(games[inserted]);
                Push(1, 0);
                SetPower(1);
            }
            else
            {
                Show(null);
                SetInsertDepth(0);
                SetPower(0);
            }
        }

        // Flight from Start (0) to Hover (1) along an arc. The rotation finishes a bit before the movement,
        // so the medium is already aligned with the slot when it arrives.
        void Fly(double amount)
        {
            double turn = 1 - Math.Pow(1 - Math.Min(1, amount * 1.2), 2);
            Vector3D position = Lerp(Start.Position, Hover.Position, amount) + new Vector3D(0, FlightArc * Math.Sin(Math.PI * amount), 0);
            SetPose(position, Lerp(Start.Tilt, Hover.Tilt, turn), Lerp(Start.Turn, Hover.Turn, turn));
            SetInsertDepth(0);
        }

        // Straight from Hover (0) to Seated (1); click pushes a little further in and lets it spring back.
        void Push(double depth, double click)
        {
            Vector3D way = Seated.Position - Hover.Position;
            Vector3D position = Hover.Position + way * depth;
            if (click > 0)
            {
                way.Normalize();
                position += way * (ClickDepth * click);
            }
            SetPose(position, Hover.Tilt, Hover.Turn);
            SetInsertDepth(depth);
        }

        void SetPose(Vector3D position, double tilt, double turn)
        {
            if (_Position.OffsetX != position.X) _Position.OffsetX = position.X;
            if (_Position.OffsetY != position.Y) _Position.OffsetY = position.Y;
            if (_Position.OffsetZ != position.Z) _Position.OffsetZ = position.Z;
            if (_Tilt.Angle != tilt) _Tilt.Angle = tilt;
            if (_Turn.Angle != turn) _Turn.Angle = turn;
        }

        void Show(B5RoomGameEntry game)
        {
            if (_Shown == game) return;
            _Shown = game;
            _Medium.Children.Clear();
            if (game == null) return;

            Model3DGroup model;
            if (!_Models.TryGetValue(game, out model))
            {
                model = CreateMedium(game);
                model.Freeze();
                _Models.Add(game, model);
            }
            _Medium.Children.Add(model);
        }

        // Position of progress inside [start, end] as 0..1.
        protected static double Section(double progress, double start, double end)
        {
            return Math.Max(0, Math.Min(1, (progress - start) / (end - start)));
        }

        protected static double Ease(double t)
        {
            return t * t * (3 - 2 * t);
        }

        static Vector3D Lerp(Vector3D from, Vector3D to, double amount)
        {
            return from + (to - from) * amount;
        }

        static double Lerp(double from, double to, double amount)
        {
            return from + (to - from) * amount;
        }
    }
}
