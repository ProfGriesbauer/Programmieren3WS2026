using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace OOPGames
{
    // Turns keys into G10_BomberInput. Walking needs held keys, which the framework does not report
    // (it only sends key presses), so the mapper also asks the keyboard state.
    internal sealed class G10_BomberInputMapper
    {
        static readonly Key[] _BombKeys = { Key.Z, Key.Space };
        static readonly Key[] _DetonateKeys = { Key.C };
        static readonly Key[] _ConfirmKeys = { Key.Enter, Key.P };

        readonly IG10_KeyStateSource _KeyState;
        G10_Direction _LastPressed = G10_Direction.None;
        bool _BombWasDown;
        bool _DetonateWasDown;
        bool _ConfirmWasDown;

        public G10_BomberInputMapper(IG10_KeyStateSource keyState)
        {
            if (keyState == null) throw new ArgumentNullException(nameof(keyState));
            _KeyState = keyState;
        }

        // Keys still held from the hub (e.g. the Enter that started the game) must not act at once.
        public void Reset()
        {
            _LastPressed = G10_Direction.None;
            _BombWasDown = AnyDown(_BombKeys);
            _DetonateWasDown = AnyDown(_DetonateKeys);
            _ConfirmWasDown = AnyDown(_ConfirmKeys);
        }

        public G10_BomberInput Read(IReadOnlyCollection<Key> pressedKeys)
        {
            if (pressedKeys == null) throw new ArgumentNullException(nameof(pressedKeys));
            G10_Direction tapped = G10_Direction.None;
            foreach (Key key in pressedKeys)
            {
                G10_Direction direction = DirectionOf(key);
                if (direction != G10_Direction.None) tapped = direction;
            }
            if (tapped != G10_Direction.None) _LastPressed = tapped;

            G10_BomberInput input = new G10_BomberInput(
                CurrentDirection(tapped),
                IsFreshPress(pressedKeys, _BombKeys, _BombWasDown),
                IsFreshPress(pressedKeys, _DetonateKeys, _DetonateWasDown),
                IsFreshPress(pressedKeys, _ConfirmKeys, _ConfirmWasDown));
            _BombWasDown = AnyDown(_BombKeys);
            _DetonateWasDown = AnyDown(_DetonateKeys);
            _ConfirmWasDown = AnyDown(_ConfirmKeys);
            return input;
        }

        // The direction pressed last wins while it is held. A short tap still moves for one tick.
        G10_Direction CurrentDirection(G10_Direction tapped)
        {
            if (_LastPressed != G10_Direction.None && IsHeld(_LastPressed)) return _LastPressed;
            foreach (G10_Direction direction in G10_DirectionExtensions.TieBreakOrder)
            {
                if (IsHeld(direction)) return direction;
            }
            return tapped;
        }

        bool IsHeld(G10_Direction direction)
        {
            switch (direction)
            {
                case G10_Direction.Up: return _KeyState.IsDown(Key.Up) || _KeyState.IsDown(Key.W);
                case G10_Direction.Down: return _KeyState.IsDown(Key.Down) || _KeyState.IsDown(Key.S);
                case G10_Direction.Left: return _KeyState.IsDown(Key.Left) || _KeyState.IsDown(Key.A);
                case G10_Direction.Right: return _KeyState.IsDown(Key.Right) || _KeyState.IsDown(Key.D);
                default: return false;
            }
        }

        // A press counts once: the key repeat of a held key is ignored.
        static bool IsFreshPress(IReadOnlyCollection<Key> pressedKeys, Key[] keys, bool wasDown)
        {
            if (wasDown) return false;
            foreach (Key key in pressedKeys)
            {
                if (Array.IndexOf(keys, key) >= 0) return true;
            }
            return false;
        }

        bool AnyDown(Key[] keys)
        {
            foreach (Key key in keys)
            {
                if (_KeyState.IsDown(key)) return true;
            }
            return false;
        }

        static G10_Direction DirectionOf(Key key)
        {
            if (B5RoomKeys.IsUp(key)) return G10_Direction.Up;
            if (B5RoomKeys.IsDown(key)) return G10_Direction.Down;
            if (B5RoomKeys.IsLeft(key)) return G10_Direction.Left;
            if (B5RoomKeys.IsRight(key)) return G10_Direction.Right;
            return G10_Direction.None;
        }
    }
}
