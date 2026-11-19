using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    // The hub creates and starts the game at the end of the cartridge animation, exactly when its
    // camera zoom begins. Start therefore stays cheap: it only begins a background warm-up of
    // everything that needs no WPF. The game, the bitmap and the media players are created when
    // the game is first shown and the 3D room is already hidden.
    public sealed class G10_PacManGame : IB5RoomMiniGame
    {
        // One per process: all game objects share the sound output and the warm-up.
        static G10_MediaSoundOutput _SharedOutput;
        static Task _WarmUp;

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
            _Session = null;
            if (_WarmUp == null) _WarmUp = Task.Run(new Action(WarmUp));
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            EnsureSession();
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
                NewSession();
            }
            _Session.Update(input);
            _Audio.Update(_Session);
        }

        public void Draw(Canvas canvas)
        {
            EnsureSession();
            _Renderer.Draw(canvas, _Session);
        }

        void EnsureSession()
        {
            if (_Session != null) return;
            // Usually long finished; waiting makes sure no shared cache is built twice at once.
            if (_WarmUp != null) _WarmUp.Wait();
            NewSession();
            if (_SharedOutput == null) _SharedOutput = new G10_MediaSoundOutput();
            if (_Audio == null) _Audio = new G10_PacManAudio(_SharedOutput);
        }

        void NewSession()
        {
            _Session = new G10_PacManSession(B5RoomHighscores.Get(Title), Environment.TickCount);
            _HighscoreSaved = false;
        }

        // Runs everything once that needs no WPF, so it is compiled and its caches are filled:
        // game logic, sprites, maze background, HUD, sounds and the highscore file.
        static void WarmUp()
        {
            G10_PacManSession session = new G10_PacManSession(B5RoomHighscores.Get("Pac-Man"), 0);
            G10_PacManInput none = new G10_PacManInput(G10_Direction.None, false);
            for (int i = 0; i <= G10_PacManSession.ReadyTicks; i++) session.Update(none);
            new G10_PacManRenderer().Compose(session);
            foreach (G10_Sound sound in Enum.GetValues(typeof(G10_Sound))) G10_SoundSynth.Clip(sound);
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
