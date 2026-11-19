using System;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // Atari cartridge going into the console: it comes in from the front left under the table top, turns upright
    // above the slot, is pushed in (the two flaps swing open), clicks into place, then the power switch is flipped on.
    public sealed class B5AtariConsoleAnimation : B5MediaAnimation
    {
        // Poses of the cartridge's bottom edge in console millimetres (see B5AtariConsoleModel, z = -d).
        static readonly B5MediaPose StartPose = new B5MediaPose(-280, 90, 150, -65, 35);
        static readonly B5MediaPose HoverPose = new B5MediaPose(0, B5AtariConsoleModel.SlotTop + 14, -B5AtariConsoleModel.SlotCenter, 0, 0);
        static readonly B5MediaPose SeatedPose = new B5MediaPose(0, B5AtariConsoleModel.SlotTop - 46, -B5AtariConsoleModel.SlotCenter, 0, 0);
        const double FlapOpenAngle = 84, PowerOn = 28, PowerOff = -28;

        readonly AxisAngleRotation3D _FrontFlap;
        readonly AxisAngleRotation3D _BackFlap;
        readonly AxisAngleRotation3D _Power;

        public B5AtariConsoleAnimation() : base(B5RoomDeviceKind.Atari, B5AtariConsoleModel.Placement)
        {
            Parts.Children.Add(B5AtariConsoleModel.CreateFlap(true, out _FrontFlap));
            Parts.Children.Add(B5AtariConsoleModel.CreateFlap(false, out _BackFlap));
            Parts.Children.Add(B5AtariConsoleModel.CreatePowerLever(out _Power));
            _Power.Angle = PowerOff;
        }

        // Low and close under the table top; the target lies right of the slot, which keeps the console clear of the sidebar.
        public override B5CameraView InsertView
        {
            get { return new B5CameraView(B5AtariConsoleModel.SlotCenterInRoom + new Vector3D(0.28, 0.03, -0.07), 1.6, 0.42, 1.27); }
        }

        protected override B5MediaPose Start { get { return StartPose; } }
        protected override B5MediaPose Hover { get { return HoverPose; } }
        protected override B5MediaPose Seated { get { return SeatedPose; } }

        protected override Model3DGroup CreateMedium(B5RoomGameEntry game)
        {
            return B5AtariCartridgeModel.Create(game);
        }

        protected override void SetPower(double on)
        {
            double angle = PowerOff + (PowerOn - PowerOff) * on;
            if (_Power.Angle != angle) _Power.Angle = angle;
        }

        // The flaps are pushed open by the bottom edge of the cartridge once it reaches the slot.
        protected override void SetInsertDepth(double depth)
        {
            double travel = HoverPose.Position.Y - SeatedPose.Position.Y;
            double gap = HoverPose.Position.Y - B5AtariConsoleModel.SlotTop - 1;
            double open = Math.Max(0, Math.Min(1, (depth * travel - gap) / 14));
            double angle = FlapOpenAngle * open;
            if (_FrontFlap.Angle != -angle) _FrontFlap.Angle = -angle;
            if (_BackFlap.Angle != angle) _BackFlap.Angle = angle;
        }
    }
}
