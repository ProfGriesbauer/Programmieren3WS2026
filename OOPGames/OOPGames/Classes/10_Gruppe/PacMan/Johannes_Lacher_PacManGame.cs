using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    // Input of one tick, already free of keys.
    public struct G10_PacManInput
    {
        public G10_PacManInput(G10_Direction direction, bool confirm)
        {
            Direction = direction;
            Confirm = confirm;
        }

        public G10_Direction Direction { get; private set; }
        // Enter / Z / Space / P: pause, resume, new game after GAME OVER.
        public bool Confirm { get; private set; }
    }

    // Adapter between the Kinderzimmer hub and the game: translates keys, saves the highscore,
    // and hands the game state to the renderer and the audio. The only Pac-Man class that knows
    // about keys.
    public sealed class G10_PacManGame : IB5RoomMiniGame
    {
        // The hub creates a new game object for every start; all of them share one sound output.
        static G10_MediaSoundOutput _SharedOutput;

        readonly G10_PacManRenderer _Renderer = new G10_PacManRenderer();
        G10_PacManSession _Session;
        G10_PacManAudio _Audio;
        bool _HighscoreSaved;

        public string Title { get { return "Pac-Man"; } }
        public int Width { get { return G10_PacManRenderer.ScreenWidth; } }
        public int Height { get { return G10_PacManRenderer.ScreenHeight; } }
        public bool ExitRequested { get { return false; } }

        public void Start()
        {
            _Session = new G10_PacManSession(B5RoomHighscores.Get(Title), Environment.TickCount);
            _HighscoreSaved = false;
            // Loading the sounds takes a moment; the hub calls Start already during the cartridge
            // animation, so they are ready when the game begins. Nothing is played here.
            if (_SharedOutput == null) _SharedOutput = new G10_MediaSoundOutput();
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            if (_Session == null) Start();
            // Created on the first update, not in Start: the hub also starts a game for the
            // cartridge animation, which must stay silent.
            if (_Audio == null) _Audio = new G10_PacManAudio(_SharedOutput);
            G10_PacManInput input = ReadInput(keys);
            foreach (Key key in keys)
            {
                if (key == Key.M) _Audio.ToggleMute();
            }

            if (_Session.Phase == G10_PacManPhaseKind.GameOver)
            {
                if (!_HighscoreSaved)
                {
                    B5RoomHighscores.Save(Title, _Session.Score);
                    _HighscoreSaved = true;
                }
                if (!input.Confirm) return;
                Start();
            }
            _Session.Update(input);
            _Audio.Update(_Session);
        }

        public void Draw(Canvas canvas)
        {
            if (_Session == null) Start();
            _Renderer.Draw(canvas, _Session);
        }

        // The framework only reports key presses, so the last direction key of the tick wins
        // and Pac-Man keeps it until another one is pressed.
        static G10_PacManInput ReadInput(IReadOnlyCollection<Key> keys)
        {
            G10_Direction direction = G10_Direction.None;
            bool confirm = false;
            foreach (Key key in keys)
            {
                if (B5RoomKeys.IsLeft(key)) direction = G10_Direction.Left;
                else if (B5RoomKeys.IsRight(key)) direction = G10_Direction.Right;
                else if (B5RoomKeys.IsUp(key)) direction = G10_Direction.Up;
                else if (B5RoomKeys.IsDown(key)) direction = G10_Direction.Down;
                else if (key == Key.Enter || key == Key.Z || key == Key.Space || key == Key.P) confirm = true;
            }
            return new G10_PacManInput(direction, confirm);
        }
    }
}
