using System;
using System.Collections.Generic;

namespace OOPGames
{
    internal enum G10_BomberSound
    {
        StageStart,
        StageClear,
        GameOver,
        Victory,
        // Loops while playing; Hurry is the same tune, faster, for the last 30 seconds.
        Music,
        Hurry,
        StepA,
        StepB,
        BombPlaced,
        Explosion,
        PowerUp,
        EnemyDefeated,
        Death,
        TimeUp,
        ExitOpened,
        DoorGuards
    }

    // All Bomberman sounds, generated in code in the style of the C64 sound chip (square, triangle
    // and noise voices). Tunes and effects are our own; each clip is built once and cached.
    internal static class G10_BombermanSoundSynth
    {
        // Chords of the background loop (A minor, F, G, E minor) as MIDI root, third, fifth.
        static readonly int[][] _Chords =
        {
            new[] { 57, 60, 64 }, new[] { 53, 57, 60 }, new[] { 55, 59, 62 }, new[] { 52, 55, 59 }
        };
        // Which chord tone the arpeggio plays on each eighth of a bar (3 = root one octave up).
        static readonly int[] _ArpeggioSteps = { 0, 1, 2, 3, 2, 1, 2, 1 };

        static readonly Dictionary<G10_BomberSound, G10_SoundClip> _Clips = new Dictionary<G10_BomberSound, G10_SoundClip>();

        public static G10_SoundClip Clip(G10_BomberSound sound)
        {
            G10_SoundClip clip;
            if (!_Clips.TryGetValue(sound, out clip))
            {
                clip = new G10_SoundClip("bomber_" + sound, Create(sound).ToSamples());
                _Clips[sound] = clip;
            }
            return clip;
        }

        public static IEnumerable<G10_SoundClip> All()
        {
            foreach (G10_BomberSound sound in Enum.GetValues(typeof(G10_BomberSound))) yield return Clip(sound);
        }

        static G10_ToneWriter Create(G10_BomberSound sound)
        {
            switch (sound)
            {
                case G10_BomberSound.StageStart: return Tune(0.1, G10_Wave.Square, new[] { 60, 64, 67, 72, 71, 72 }, 76, 0.6, 36);
                case G10_BomberSound.StageClear: return Tune(0.08, G10_Wave.Square, new[] { 67, 72, 76, 79, 76, 79 }, 84, 0.7, 48);
                case G10_BomberSound.GameOver: return Tune(0.22, G10_Wave.Triangle, new[] { 69, 67, 64, 62 }, 57, 0.9, 33);
                case G10_BomberSound.Victory: return Tune(0.1, G10_Wave.Square, new[] { 72, 76, 79, 84, 79, 84, 86, 88 }, 91, 0.9, 48);
                case G10_BomberSound.Music: return Loop(0.1);
                case G10_BomberSound.Hurry: return Loop(0.07);
                case G10_BomberSound.StepA: return Single(0.03, 190, 150, G10_Wave.Triangle, 0.12);
                case G10_BomberSound.StepB: return Single(0.03, 160, 120, G10_Wave.Triangle, 0.12);
                case G10_BomberSound.BombPlaced: return Single(0.08, 240, 110, G10_Wave.Triangle, 0.4);
                case G10_BomberSound.Explosion: return Explosion();
                case G10_BomberSound.PowerUp: return Arpeggio(0.05, G10_Wave.Square, 0.18, 784, 988, 1175, 1568, 1976);
                case G10_BomberSound.EnemyDefeated: return EnemyDefeated();
                case G10_BomberSound.Death: return Death();
                case G10_BomberSound.TimeUp: return Arpeggio(0.12, G10_Wave.Square, 0.16, 880, 660, 880, 660, 880, 660);
                case G10_BomberSound.ExitOpened: return ExitOpened();
                case G10_BomberSound.DoorGuards: return DoorGuards();
                default: throw new ArgumentOutOfRangeException(nameof(sound));
            }
        }

        static G10_ToneWriter Single(double seconds, double fromHz, double toHz, G10_Wave wave, double volume)
        {
            G10_ToneWriter writer = new G10_ToneWriter(seconds);
            writer.Sweep(0, seconds, fromHz, toHz, wave, volume, volume);
            return writer;
        }

        static G10_ToneWriter Arpeggio(double noteSeconds, G10_Wave wave, double volume, params double[] notes)
        {
            G10_ToneWriter writer = new G10_ToneWriter(noteSeconds * notes.Length);
            for (int i = 0; i < notes.Length; i++) writer.Note(i * noteSeconds, noteSeconds, notes[i], wave, volume);
            return writer;
        }

        // Short tune: quick notes, one long final note, and a held bass note underneath.
        static G10_ToneWriter Tune(double step, G10_Wave wave, int[] notes, int finalNote, double finalSeconds, int bass)
        {
            double length = step * notes.Length + finalSeconds;
            G10_ToneWriter writer = new G10_ToneWriter(length);
            for (int i = 0; i < notes.Length; i++) writer.Note(i * step, step * 0.9, Midi(notes[i]), wave, 0.16);
            writer.Sweep(step * notes.Length, finalSeconds, Midi(finalNote), Midi(finalNote), wave, 0.18, 0.02);
            writer.Sweep(0, length, Midi(bass), Midi(bass), G10_Wave.Triangle, 0.3, 0.05);
            return writer;
        }

        // Four bars of eighths: triangle bass, square arpeggio, noise hi-hat and a short kick.
        static G10_ToneWriter Loop(double step)
        {
            const int StepsPerBar = 8;
            G10_ToneWriter writer = new G10_ToneWriter(step * StepsPerBar * _Chords.Length);
            for (int bar = 0; bar < _Chords.Length; bar++)
            {
                int[] chord = _Chords[bar];
                for (int s = 0; s < StepsPerBar; s++)
                {
                    double start = (bar * StepsPerBar + s) * step;
                    int tone = _ArpeggioSteps[s];
                    int note = tone == 3 ? chord[0] + 12 : chord[tone];
                    writer.Note(start, step * 0.8, Midi(note + 12), G10_Wave.Square, 0.07);
                    if (s % 2 == 0 || s % 4 == 3)
                    {
                        int bass = chord[0] - 12 + (s % 4 == 3 ? 12 : 0);
                        writer.Note(start, step * 0.9, Midi(bass), G10_Wave.Triangle, 0.3);
                    }
                    if (s % 2 == 1) writer.Note(start, 0.02, 7000, G10_Wave.Noise, 0.08);
                    if (s % 4 == 0) writer.Sweep(start, 0.06, 150, 50, G10_Wave.Square, 0.18, 0.02);
                }
            }
            return writer;
        }

        // A burst of noise that falls in pitch and fades, with a low thump underneath.
        static G10_ToneWriter Explosion()
        {
            G10_ToneWriter writer = new G10_ToneWriter(0.7);
            writer.Sweep(0, 0.7, 4000, 250, G10_Wave.Noise, 0.6, 0.0);
            writer.Sweep(0, 0.25, 120, 40, G10_Wave.Square, 0.3, 0.0);
            return writer;
        }

        static G10_ToneWriter EnemyDefeated()
        {
            G10_ToneWriter writer = new G10_ToneWriter(0.3);
            writer.Sweep(0, 0.25, 1200, 200, G10_Wave.Square, 0.16, 0.04);
            writer.Sweep(0, 0.08, 6000, 3000, G10_Wave.Noise, 0.15, 0.0);
            return writer;
        }

        // Falling wobble, then a puff of noise.
        static G10_ToneWriter Death()
        {
            G10_ToneWriter writer = new G10_ToneWriter(1.4);
            for (int i = 0; i < 8; i++)
                writer.Sweep(i * 0.12, 0.12, 900 - i * 90, 700 - i * 80, G10_Wave.Triangle, 0.35, 0.25);
            writer.Sweep(0.96, 0.4, 2000, 300, G10_Wave.Noise, 0.3, 0.0);
            return writer;
        }

        // Three bell-like notes that ring into each other.
        static G10_ToneWriter ExitOpened()
        {
            G10_ToneWriter writer = new G10_ToneWriter(0.7);
            double[] notes = { Midi(84), Midi(88), Midi(91) };
            for (int i = 0; i < notes.Length; i++) writer.Sweep(i * 0.1, 0.5, notes[i], notes[i], G10_Wave.Triangle, 0.25, 0.0);
            return writer;
        }

        static G10_ToneWriter DoorGuards()
        {
            G10_ToneWriter writer = new G10_ToneWriter(0.6);
            for (int i = 0; i < 3; i++) writer.Sweep(i * 0.2, 0.18, 220, 110, G10_Wave.Square, 0.16, 0.06);
            return writer;
        }

        static double Midi(int note)
        {
            return 440 * Math.Pow(2, (note - 69) / 12.0);
        }
    }

    // Decides what is heard: one-shot sounds for the cues of an update, jingles when the phase
    // changes, footsteps, and the background loop while playing. The loop is restarted by the
    // updates themselves, so it stops on its own when the game is no longer updated (e.g. after Esc).
    public sealed class G10_BombermanAudio
    {
        const int StepIntervalTicks = 5;
        // The sounds are loaded when the game is first shown; a jingle may wait this long for its
        // file, otherwise it is skipped.
        const int JingleWaitTicks = 12;
        const int HurrySeconds = 30;

        readonly IG10_ClipOutput _Output;
        int _Tick;
        bool _HasPhase;
        G10_BomberPhaseKind _LastPhase;
        G10_SoundClip _Loop;
        int _LoopEndsAt;
        G10_SoundClip _Jingle;
        int _JingleWait;
        int _NextStepAt;
        bool _NextStepIsB;

        public G10_BombermanAudio(IG10_ClipOutput output)
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
            _Loop = null;
            _Jingle = null;
        }

        public void Update(IG10_BomberView view)
        {
            _Tick++;
            bool phaseChanged = !_HasPhase || view.Phase != _LastPhase;
            _HasPhase = true;
            _LastPhase = view.Phase;
            if (Muted) return;

            if (phaseChanged) StartJingle(view.Phase);
            PlayJingleWhenReady();
            foreach (G10_BomberSoundCue cue in view.SoundCues) Play(CueSound(cue));
            PlaySteps(view);
            UpdateLoop(view);
        }

        void StartJingle(G10_BomberPhaseKind phase)
        {
            switch (phase)
            {
                case G10_BomberPhaseKind.StageIntro: _Jingle = G10_BombermanSoundSynth.Clip(G10_BomberSound.StageStart); break;
                case G10_BomberPhaseKind.StageClear: _Jingle = G10_BombermanSoundSynth.Clip(G10_BomberSound.StageClear); break;
                case G10_BomberPhaseKind.GameOver: _Jingle = G10_BombermanSoundSynth.Clip(G10_BomberSound.GameOver); break;
                case G10_BomberPhaseKind.Victory: _Jingle = G10_BombermanSoundSynth.Clip(G10_BomberSound.Victory); break;
                default: return;
            }
            _JingleWait = JingleWaitTicks;
        }

        void PlayJingleWhenReady()
        {
            if (_Jingle == null) return;
            if (_Output.IsReady(_Jingle))
            {
                _Output.Play(_Jingle);
                _Jingle = null;
                return;
            }
            if (--_JingleWait <= 0) _Jingle = null;
        }

        void PlaySteps(IG10_BomberView view)
        {
            if (view.Phase != G10_BomberPhaseKind.Playing || !view.Player.IsMoving || _Tick < _NextStepAt) return;
            Play(_NextStepIsB ? G10_BomberSound.StepB : G10_BomberSound.StepA);
            _NextStepIsB = !_NextStepIsB;
            _NextStepAt = _Tick + StepIntervalTicks;
        }

        void UpdateLoop(IG10_BomberView view)
        {
            G10_SoundClip wanted = null;
            if (view.Phase == G10_BomberPhaseKind.Playing)
                wanted = G10_BombermanSoundSynth.Clip(view.SecondsLeft <= HurrySeconds ? G10_BomberSound.Hurry : G10_BomberSound.Music);
            if (_Loop != null && wanted != _Loop)
            {
                _Output.Stop(_Loop);
                _Loop = null;
            }
            if (wanted == null || (_Loop != null && _Tick < _LoopEndsAt)) return;
            if (!_Output.IsReady(wanted)) return;
            _Output.Play(wanted);
            _Loop = wanted;
            _LoopEndsAt = _Tick + wanted.DurationTicks;
        }

        void Play(G10_BomberSound sound)
        {
            _Output.Play(G10_BombermanSoundSynth.Clip(sound));
        }

        static G10_BomberSound CueSound(G10_BomberSoundCue cue)
        {
            switch (cue)
            {
                case G10_BomberSoundCue.BombPlaced: return G10_BomberSound.BombPlaced;
                case G10_BomberSoundCue.Explosion: return G10_BomberSound.Explosion;
                case G10_BomberSoundCue.PowerUp: return G10_BomberSound.PowerUp;
                case G10_BomberSoundCue.EnemyDefeated: return G10_BomberSound.EnemyDefeated;
                case G10_BomberSoundCue.PlayerDied: return G10_BomberSound.Death;
                case G10_BomberSoundCue.TimeUp: return G10_BomberSound.TimeUp;
                case G10_BomberSoundCue.ExitOpened: return G10_BomberSound.ExitOpened;
                default: return G10_BomberSound.DoorGuards;
            }
        }
    }
}
