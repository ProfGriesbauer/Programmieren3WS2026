using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Decides what is heard: one-shot sounds for the cues of a tick, plus one background loop that
    // follows the game state (siren, frightened ghosts, eyes returning home). Loops are restarted
    // by the ticks themselves, so they fade out on their own when the game is no longer updated
    // (e.g. after Esc), without a timer of our own.
    public sealed class G10_PacManAudio
    {
        // The death sound starts when the death animation starts (see the renderer).
        const int DeathSoundDelayTicks = 15;
        // The sounds are loaded when the first game is shown; the start tune may wait this long
        // for its file, otherwise it is skipped.
        const int StartJingleWaitTicks = 12;
        const int NoTick = -1;

        readonly IG10_SoundOutput _Output;
        int _Tick;
        bool _HasLoop;
        G10_Sound _Loop;
        int _LoopEndsAt;
        int _ChompEndsAt;
        bool _NextChompIsB;
        int _DeathAt = NoTick;
        int _JingleWaitTicks;

        public G10_PacManAudio(IG10_SoundOutput output)
        {
            if (output == null) throw new ArgumentNullException(nameof(output));
            _Output = output;
        }

        public bool Muted { get; private set; }

        public void ToggleMute()
        {
            Muted = !Muted;
            if (!Muted) return;
            _Output.StopAll();
            _HasLoop = false;
            _DeathAt = NoTick;
            _JingleWaitTicks = 0;
        }

        public void Update(IG10_PacManView view)
        {
            _Tick++;
            if (Muted) return;
            foreach (G10_SoundCue cue in view.SoundCues) PlayCue(cue);
            PlayStartJingleWhenReady();
            if (_Tick == _DeathAt) _Output.Play(G10_Sound.Death);
            UpdateLoop(view);
        }

        void PlayStartJingleWhenReady()
        {
            if (_JingleWaitTicks == 0) return;
            if (_Output.IsReady(G10_Sound.StartJingle))
            {
                _Output.Play(G10_Sound.StartJingle);
                _JingleWaitTicks = 0;
                return;
            }
            _JingleWaitTicks--;
        }

        void PlayCue(G10_SoundCue cue)
        {
            switch (cue)
            {
                case G10_SoundCue.GameStart:
                    _JingleWaitTicks = StartJingleWaitTicks;
                    break;
                case G10_SoundCue.Chomp:
                    // While Pac-Man keeps eating, the two halves of the chomp alternate without overlapping.
                    if (_Tick < _ChompEndsAt) break;
                    G10_Sound chomp = _NextChompIsB ? G10_Sound.ChompB : G10_Sound.ChompA;
                    _NextChompIsB = !_NextChompIsB;
                    _Output.Play(chomp);
                    _ChompEndsAt = _Tick + G10_SoundSynth.Clip(chomp).DurationTicks;
                    break;
                case G10_SoundCue.GhostEaten:
                    _Output.Play(G10_Sound.GhostEaten);
                    break;
                case G10_SoundCue.FruitEaten:
                    _Output.Play(G10_Sound.FruitEaten);
                    break;
                case G10_SoundCue.ExtraLife:
                    _Output.Play(G10_Sound.ExtraLife);
                    break;
                case G10_SoundCue.Death:
                    _DeathAt = _Tick + DeathSoundDelayTicks;
                    break;
            }
        }

        void UpdateLoop(IG10_PacManView view)
        {
            G10_Sound wanted;
            bool hasWanted = WantedLoop(view, out wanted);
            if (_HasLoop && (!hasWanted || wanted != _Loop))
            {
                _Output.Stop(_Loop);
                _HasLoop = false;
            }
            if (!hasWanted) return;
            if (_HasLoop && _Tick < _LoopEndsAt) return;
            _Output.Play(wanted);
            _Loop = wanted;
            _HasLoop = true;
            _LoopEndsAt = _Tick + G10_SoundSynth.Clip(wanted).DurationTicks;
        }

        static bool WantedLoop(IG10_PacManView view, out G10_Sound loop)
        {
            loop = G10_Sound.Siren;
            if (view.Phase != G10_PacManPhaseKind.Playing) return false;
            bool frightened = false;
            foreach (IG10_GhostView ghost in view.Ghosts)
            {
                if (ghost.Mode == G10_GhostModeKind.Eaten && ghost.Visible)
                {
                    loop = G10_Sound.Retreat;
                    return true;
                }
                if (ghost.Mode == G10_GhostModeKind.Frightened) frightened = true;
            }
            if (frightened) loop = G10_Sound.Frightened;
            return true;
        }
    }

    // Pac-Man's sounds on the shared media output (Classes/10_Gruppe/Shared): maps each sound
    // to its generated clip.
    internal sealed class G10_MediaSoundOutput : IG10_SoundOutput
    {
        readonly IG10_ClipOutput _Clips;

        public G10_MediaSoundOutput()
        {
            List<G10_SoundClip> clips = new List<G10_SoundClip>();
            foreach (G10_Sound sound in Enum.GetValues(typeof(G10_Sound))) clips.Add(G10_SoundSynth.Clip(sound));
            _Clips = new G10_MediaClipOutput("PacManSounds", clips);
        }

        public bool IsReady(G10_Sound sound) { return _Clips.IsReady(G10_SoundSynth.Clip(sound)); }
        public void Play(G10_Sound sound) { _Clips.Play(G10_SoundSynth.Clip(sound)); }
        public void Stop(G10_Sound sound) { _Clips.Stop(G10_SoundSynth.Clip(sound)); }
        public void StopAll() { _Clips.StopAll(); }
    }
}
