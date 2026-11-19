using System;
using System.Collections.Generic;
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
        bool _SuppressUntilRelease;

        public G10_TetrisInputMapper(IG10_KeyStateSource keyState)
        {
            if (keyState == null) throw new ArgumentNullException("keyState");
            _KeyState = keyState;
        }

        public G10_TetrisInput Read(IReadOnlyCollection<Key> pressedKeys)
        {
            if (pressedKeys == null) throw new ArgumentNullException("pressedKeys");

            if (_SuppressUntilRelease)
            {
                if (!AreControlsDown())
                    _SuppressUntilRelease = false;
                return G10_TetrisInput.Neutral;
            }

            return new G10_TetrisInput(
                _Horizontal.Read(_KeyState),
                _KeyState.IsDown(Key.Down) || _KeyState.IsDown(Key.S),
                WasPressed(pressedKeys, Key.Up) || WasPressed(pressedKeys, Key.Z),
                WasPressed(pressedKeys, Key.Space),
                WasPressed(pressedKeys, Key.Enter));
        }

        public void Reset()
        {
            _SuppressUntilRelease = AreControlsDown();
        }

        static bool WasPressed(IReadOnlyCollection<Key> pressedKeys, Key key)
        {
            foreach (Key pressedKey in pressedKeys)
                if (pressedKey == key)
                    return true;
            return false;
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
