using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    // Adapter between the Kinderzimmer hub and the game: translates keys, saves the highscore and
    // hands the game state to the renderer and the audio. The only Bomberman class that knows the hub.
    public sealed class G10_BombermanGame : IG10_RoomMiniGame
    {
        // One per process: creating the media players is slow, and all game objects can share them.
        static IG10_ClipOutput _SharedOutput;

        readonly G10_BombermanRenderer _Renderer = new G10_BombermanRenderer();
        readonly G10_BomberInputMapper _Input = new G10_BomberInputMapper(new G10_WpfKeyStateSource());
        G10_BomberSession _Session;
        G10_BomberPhaseKind _LastPhase;
        G10_BombermanAudio _Audio;

        public string Title { get { return "Bomberman"; } }
        public int Width { get { return G10_BombermanRenderer.ScreenWidth; } }
        public int Height { get { return G10_BombermanRenderer.ScreenHeight; } }
        public bool ExitRequested { get { return false; } }

        public void Start()
        {
            _Input.Reset();
            NewSession();
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            EnsureAudio();
            G10_BomberInput input = _Input.Read(keys);
            foreach (Key key in keys)
            {
                if (key == Key.M) _Audio.ToggleMute();
            }

            if (IsOver(_Session.Phase))
            {
                if (input.Confirm) NewSession();
            }
            else
            {
                _Session.Update(input);
                SaveHighscoreOnPhaseChange();
            }
            _Audio.Update(_Session);
        }

        public void Draw(Canvas canvas)
        {
            _Renderer.Draw(canvas, _Session);
        }

        // Saved after each stage too, so leaving with Esc keeps the score of cleared stages.
        void SaveHighscoreOnPhaseChange()
        {
            if (_Session.Phase == _LastPhase) return;
            _LastPhase = _Session.Phase;
            if (_LastPhase == G10_BomberPhaseKind.StageClear || IsOver(_LastPhase))
                G10_RoomHighscores.Save(Title, _Session.Score);
        }

        // The hub calls Start while its camera still zooms in, so the media players are created on
        // the first update, when the 3D room is already hidden.
        void EnsureAudio()
        {
            if (_Audio != null) return;
            if (_SharedOutput == null) _SharedOutput = new G10_MediaClipOutput("BombermanSounds", G10_BombermanSoundSynth.All());
            _Audio = new G10_BombermanAudio(_SharedOutput);
        }

        void NewSession()
        {
            _Session = new G10_BomberSession(G10_RoomHighscores.Get(Title), Environment.TickCount);
            _LastPhase = _Session.Phase;
        }

        static bool IsOver(G10_BomberPhaseKind phase)
        {
            return phase == G10_BomberPhaseKind.GameOver || phase == G10_BomberPhaseKind.Victory;
        }
    }
}
