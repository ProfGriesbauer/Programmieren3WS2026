using System;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // Atari cartridge going into the console: it comes in from the front left under the table top, turns upright
    // above the slot, is pushed in (the two flaps swing open), clicks into place, then the power switch is flipped on.
    public sealed class G10_AtariConsoleAnimation : G10_MediaAnimation
    {
        // Poses of the cartridge's bottom edge in console millimetres (see G10_AtariConsoleModel, z = -d).
        static readonly G10_MediaPose StartPose = new G10_MediaPose(-280, 90, 150, -65, 35);
        static readonly G10_MediaPose HoverPose = new G10_MediaPose(0, G10_AtariConsoleModel.SlotTop + 14, -G10_AtariConsoleModel.SlotCenter, 0, 0);
        static readonly G10_MediaPose SeatedPose = new G10_MediaPose(0, G10_AtariConsoleModel.SlotTop - 46, -G10_AtariConsoleModel.SlotCenter, 0, 0);
        const double FlapOpenAngle = 84, PowerOn = 28, PowerOff = -28;

        readonly AxisAngleRotation3D _FrontFlap;
        readonly AxisAngleRotation3D _BackFlap;
        readonly AxisAngleRotation3D _Power;

        public G10_AtariConsoleAnimation() : base(G10_RoomDeviceKind.Atari, G10_AtariConsoleModel.Placement)
        {
            Parts.Children.Add(G10_AtariConsoleModel.CreateFlap(true, out _FrontFlap));
            Parts.Children.Add(G10_AtariConsoleModel.CreateFlap(false, out _BackFlap));
            Parts.Children.Add(G10_AtariConsoleModel.CreatePowerLever(out _Power));
            _Power.Angle = PowerOff;
        }

        // Low and close under the table top; the target lies right of the slot, which keeps the console clear of the sidebar.
        public override G10_CameraView InsertView
        {
            get { return new G10_CameraView(G10_AtariConsoleModel.SlotCenterInRoom + new Vector3D(0.28, 0.03, -0.07), 1.6, 0.42, 1.27); }
        }

        protected override G10_MediaPose Start { get { return StartPose; } }
        protected override G10_MediaPose Hover { get { return HoverPose; } }
        protected override G10_MediaPose Seated { get { return SeatedPose; } }

        protected override Model3DGroup CreateMedium(G10_RoomGameEntry game)
        {
            return G10_AtariCartridgeModel.Create(game);
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
            double gap = HoverPose.Position.Y - G10_AtariConsoleModel.SlotTop - 1;
            double open = Math.Max(0, Math.Min(1, (depth * travel - gap) / 14));
            double angle = FlapOpenAngle * open;
            if (_FrontFlap.Angle != -angle) _FrontFlap.Angle = -angle;
            if (_BackFlap.Angle != angle) _BackFlap.Angle = angle;
        }
    }
}
