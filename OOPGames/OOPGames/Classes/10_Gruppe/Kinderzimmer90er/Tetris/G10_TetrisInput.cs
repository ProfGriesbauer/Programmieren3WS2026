using System.Windows.Input;

namespace OOPGames
{
    internal readonly struct G10_TetrisInput
    {
        public G10_TetrisInput(
            float horizontal,
            bool softDrop,
            bool rotate,
            bool hardDrop,
            bool confirm)
        {
            Horizontal = horizontal;
            SoftDrop = softDrop;
            Rotate = rotate;
            HardDrop = hardDrop;
            Confirm = confirm;
        }

        public static G10_TetrisInput Neutral { get; } =
            new G10_TetrisInput(0, false, false, false, false);

        public float Horizontal { get; }
        public bool SoftDrop { get; }
        public bool Rotate { get; }
        public bool HardDrop { get; }
        public bool Confirm { get; }
    }

    internal sealed class G10_TetrisInputMapper
    {
        readonly IG10_KeyStateSource _KeyState;
        readonly G10_KeyAxis _Horizontal =
            new G10_KeyAxis(Key.Left, Key.A, Key.Right, Key.D);
        readonly G10_RisingEdge _RotateEdge = new G10_RisingEdge();
        readonly G10_RisingEdge _HardDropEdge = new G10_RisingEdge();
        readonly G10_RisingEdge _ConfirmEdge = new G10_RisingEdge();
        bool _SuppressUntilRelease;

        public G10_TetrisInputMapper(IG10_KeyStateSource keyState)
        {
            _KeyState = keyState;
        }

        public G10_TetrisInput Read()
        {
            bool rotateDown = _KeyState.IsDown(Key.Up) || _KeyState.IsDown(Key.Z);
            bool hardDropDown = _KeyState.IsDown(Key.Space);
            bool confirmDown = _KeyState.IsDown(Key.Enter);
            bool rotate = _RotateEdge.Update(rotateDown);
            bool hardDrop = _HardDropEdge.Update(hardDropDown);
            bool confirm = _ConfirmEdge.Update(confirmDown);

            if (_SuppressUntilRelease)
            {
                if (!AreControlsDown())
                    _SuppressUntilRelease = false;
                return G10_TetrisInput.Neutral;
            }

            return new G10_TetrisInput(
                _Horizontal.Read(_KeyState),
                _KeyState.IsDown(Key.Down) || _KeyState.IsDown(Key.S),
                rotate,
                hardDrop,
                confirm);
        }

        public void Reset()
        {
            _RotateEdge.Reset(_KeyState.IsDown(Key.Up) || _KeyState.IsDown(Key.Z));
            _HardDropEdge.Reset(_KeyState.IsDown(Key.Space));
            _ConfirmEdge.Reset(_KeyState.IsDown(Key.Enter));
            _SuppressUntilRelease = AreControlsDown();
        }

        bool AreControlsDown()
        {
            return _Horizontal.IsAnyKeyDown(_KeyState) ||
                   _KeyState.IsDown(Key.Down) ||
                   _KeyState.IsDown(Key.S) ||
                   _KeyState.IsDown(Key.Up) ||
                   _KeyState.IsDown(Key.Z) ||
                   _KeyState.IsDown(Key.Space) ||
                   _KeyState.IsDown(Key.Enter);
        }
    }
}
