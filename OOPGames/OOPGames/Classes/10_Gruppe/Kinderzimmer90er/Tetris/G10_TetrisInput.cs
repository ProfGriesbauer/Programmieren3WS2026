using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace OOPGames
{
    internal readonly struct G10_TetrisInput
    {
        public G10_TetrisInput(
            int horizontal,
            int horizontalPressed,
            bool softDrop,
            bool rotateClockwise,
            bool rotateCounterClockwise,
            bool hardDrop,
            bool hold,
            bool start)
        {
            Horizontal = horizontal;
            HorizontalPressed = horizontalPressed;
            SoftDrop = softDrop;
            RotateClockwise = rotateClockwise;
            RotateCounterClockwise = rotateCounterClockwise;
            HardDrop = hardDrop;
            Hold = hold;
            Start = start;
        }

        // Held direction (-1, 0, 1) and the direction of a fresh key press this tick.
        public int Horizontal { get; }
        public int HorizontalPressed { get; }
        public bool SoftDrop { get; }
        public bool RotateClockwise { get; }
        public bool RotateCounterClockwise { get; }
        public bool HardDrop { get; }
        public bool Hold { get; }
        public bool Start { get; }
    }

    // X and Escape are taken by the room (leave the game), so they are not used here.
    internal sealed class G10_TetrisInputMapper
    {
        static readonly Key[] LeftKeys = { Key.Left, Key.A };
        static readonly Key[] RightKeys = { Key.Right, Key.D };
        static readonly Key[] SoftDropKeys = { Key.Down, Key.S };
        static readonly Key[] ClockwiseKeys = { Key.Up, Key.W };
        static readonly Key[] CounterClockwiseKeys = { Key.Z, Key.Q };
        static readonly Key[] HardDropKeys = { Key.Space };
        static readonly Key[] HoldKeys = { Key.C, Key.LeftShift, Key.RightShift };
        static readonly Key[] StartKeys = { Key.Enter, Key.P };
        static readonly Key[][] AllKeys =
        {
            LeftKeys, RightKeys, SoftDropKeys, ClockwiseKeys,
            CounterClockwiseKeys, HardDropKeys, HoldKeys, StartKeys
        };

        readonly IG10_KeyStateSource _KeyState;
        readonly G10_KeyAxis _Horizontal = new G10_KeyAxis(Key.Left, Key.A, Key.Right, Key.D);
        readonly HashSet<Key> _HeldLastTick = new HashSet<Key>();

        public G10_TetrisInputMapper(IG10_KeyStateSource keyState)
        {
            if (keyState == null) throw new ArgumentNullException("keyState");
            _KeyState = keyState;
        }

        // Keys still held from the cartridge menu (e.g. Enter) do not count as a press.
        public void Reset()
        {
            RememberHeldKeys();
        }

        // A key counts as pressed if its KeyDown arrived this tick and it was not held
        // last tick, so the keyboard's auto repeat does not rotate or drop again.
        public G10_TetrisInput Read(IReadOnlyCollection<Key> pressedKeys)
        {
            if (pressedKeys == null) throw new ArgumentNullException("pressedKeys");

            bool left = WasPressed(pressedKeys, LeftKeys);
            bool right = WasPressed(pressedKeys, RightKeys);
            G10_TetrisInput input = new G10_TetrisInput(
                (int)_Horizontal.Read(_KeyState),
                left == right ? 0 : left ? -1 : 1,
                IsDown(SoftDropKeys),
                WasPressed(pressedKeys, ClockwiseKeys),
                WasPressed(pressedKeys, CounterClockwiseKeys),
                WasPressed(pressedKeys, HardDropKeys),
                WasPressed(pressedKeys, HoldKeys),
                WasPressed(pressedKeys, StartKeys));
            RememberHeldKeys();
            return input;
        }

        bool WasPressed(IReadOnlyCollection<Key> pressedKeys, Key[] keys)
        {
            foreach (Key pressedKey in pressedKeys)
                foreach (Key key in keys)
                    if (pressedKey == key && !_HeldLastTick.Contains(key))
                        return true;
            return false;
        }

        bool IsDown(Key[] keys)
        {
            foreach (Key key in keys)
                if (_KeyState.IsDown(key)) return true;
            return false;
        }

        void RememberHeldKeys()
        {
            _HeldLastTick.Clear();
            foreach (Key[] keys in AllKeys)
                foreach (Key key in keys)
                    if (_KeyState.IsDown(key)) _HeldLastTick.Add(key);
        }
    }
}
