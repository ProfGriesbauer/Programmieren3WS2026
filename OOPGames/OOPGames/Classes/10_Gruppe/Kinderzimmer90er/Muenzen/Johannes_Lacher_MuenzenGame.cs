using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    // Input of one tick, already free of keys.
    public struct G10_CoinInput
    {
        public G10_CoinInput(G10_Direction pressed, G10_Direction held, bool confirm)
        {
            Pressed = pressed;
            Held = held;
            Confirm = confirm;
        }

        // Direction key pressed during this tick; None if there was none.
        public G10_Direction Pressed { get; private set; }
        // Direction key held down right now; the player keeps walking while it is held.
        public G10_Direction Held { get; private set; }
        // Enter / Z / Space / P: pause, resume, new game after GAME OVER.
        public bool Confirm { get; private set; }
    }

    // Turns the keys of a tick into G10_CoinInput. The framework reports key presses only,
    // including the repeats of a held key, so the keyboard state is read as well: it tells
    // which keys are still held, and a press of a key that was already held is a repeat.
    internal sealed class G10_CoinInputReader
    {
        static readonly G10_Direction[] Directions =
        {
            G10_Direction.Up, G10_Direction.Left, G10_Direction.Down, G10_Direction.Right
        };

        readonly IG10_KeyStateSource _KeyState;
        readonly List<G10_Direction> _HeldBefore = new List<G10_Direction>();
        G10_Direction _LastPressed;

        public G10_CoinInputReader(IG10_KeyStateSource keyState)
        {
            if (keyState == null) throw new ArgumentNullException("keyState");
            _KeyState = keyState;
        }

        public void Reset()
        {
            _HeldBefore.Clear();
            _LastPressed = G10_Direction.None;
        }

        public G10_CoinInput Read(IReadOnlyCollection<Key> keys)
        {
            G10_Direction pressed = G10_Direction.None;
            bool confirm = false;
            foreach (Key key in keys)
            {
                G10_Direction direction = DirectionOf(key);
                if (direction != G10_Direction.None)
                {
                    if (!_HeldBefore.Contains(direction)) pressed = direction;
                }
                else if (key == Key.Enter || key == Key.Z || key == Key.Space || key == Key.P) confirm = true;
            }

            _HeldBefore.Clear();
            foreach (G10_Direction direction in Directions)
                if (IsHeld(direction)) _HeldBefore.Add(direction);
            if (pressed != G10_Direction.None) _LastPressed = pressed;

            // With several keys held, the one pressed last wins.
            G10_Direction held = G10_Direction.None;
            if (_HeldBefore.Contains(_LastPressed)) held = _LastPressed;
            else if (_HeldBefore.Count > 0) held = _HeldBefore[0];
            return new G10_CoinInput(pressed, held, confirm);
        }

        bool IsHeld(G10_Direction direction)
        {
            switch (direction)
            {
                case G10_Direction.Up: return _KeyState.IsDown(Key.Up) || _KeyState.IsDown(Key.W);
                case G10_Direction.Left: return _KeyState.IsDown(Key.Left) || _KeyState.IsDown(Key.A);
                case G10_Direction.Down: return _KeyState.IsDown(Key.Down) || _KeyState.IsDown(Key.S);
                case G10_Direction.Right: return _KeyState.IsDown(Key.Right) || _KeyState.IsDown(Key.D);
                default: return false;
            }
        }

        static G10_Direction DirectionOf(Key key)
        {
            if (B5RoomKeys.IsUp(key)) return G10_Direction.Up;
            if (B5RoomKeys.IsLeft(key)) return G10_Direction.Left;
            if (B5RoomKeys.IsDown(key)) return G10_Direction.Down;
            if (B5RoomKeys.IsRight(key)) return G10_Direction.Right;
            return G10_Direction.None;
        }
    }

    // Adapter between the Kinderzimmer hub and the game: reads the keys, saves the highscore
    // and hands the game state to the renderer. The only "Muenzen sammeln" class that knows
    // about keys.
    public sealed class G10_CoinGame : IB5RoomMiniGame
    {
        readonly G10_CoinRenderer _Renderer = new G10_CoinRenderer();
        readonly G10_CoinInputReader _Input = new G10_CoinInputReader(new G10_WpfKeyStateSource());
        G10_CoinSession _Session;

        public string Title { get { return "Muenzen sammeln"; } }
        public int Width { get { return G10_CoinRenderer.ScreenWidth; } }
        public int Height { get { return G10_CoinRenderer.ScreenHeight; } }
        public bool ExitRequested { get { return false; } }

        public void Start()
        {
            _Input.Reset();
            NewSession();
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            G10_CoinInput input = _Input.Read(keys);
            if (_Session.Phase == G10_CoinPhase.GameOver && input.Confirm)
            {
                NewSession();
                return;
            }
            _Session.Update(input);
            // Saved at once, because Esc can leave the game at any time.
            if (_Session.IsNewHighScore) B5RoomHighscores.Save(Title, _Session.Score);
        }

        public void Draw(Canvas canvas)
        {
            _Renderer.Draw(canvas, _Session);
        }

        void NewSession()
        {
            _Session = new G10_CoinSession(B5RoomHighscores.Get(Title), Environment.TickCount);
        }
    }
}
