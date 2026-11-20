using System;
using System.Collections.Generic;

namespace OOPGames
{
    // All sound effects, generated in code (own designs, no recordings of the original).
    // Each clip is built once and cached.
    internal static class G10_SoundSynth
    {
        static readonly Dictionary<G10_Sound, G10_SoundClip> _Clips = new Dictionary<G10_Sound, G10_SoundClip>();

        public static G10_SoundClip Clip(G10_Sound sound)
        {
            G10_SoundClip clip;
            if (!_Clips.TryGetValue(sound, out clip))
            {
                clip = new G10_SoundClip("pacman_" + sound, Create(sound).ToSamples());
                _Clips[sound] = clip;
            }
            return clip;
        }

        static G10_ToneWriter Create(G10_Sound sound)
        {
            switch (sound)
            {
                case G10_Sound.StartJingle: return StartJingle();
                case G10_Sound.ChompA: return Single(0.12, 600, 250, G10_Wave.Triangle, 0.35);
                case G10_Sound.ChompB: return Single(0.12, 250, 600, G10_Wave.Triangle, 0.35);
                case G10_Sound.GhostEaten: return GhostEaten();
                case G10_Sound.FruitEaten: return Arpeggio(0.1, G10_Wave.Triangle, 0.35, 880, 1320, 1760);
                case G10_Sound.ExtraLife: return ExtraLife();
                case G10_Sound.Death: return Death();
                case G10_Sound.Siren: return Siren();
                case G10_Sound.Frightened: return Frightened();
                case G10_Sound.Retreat: return Single(0.16, 1000, 2200, G10_Wave.Triangle, 0.2);
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

        // Own short tune: arpeggios over C, F, D minor and G with a triangle bass.
        static G10_ToneWriter StartJingle()
        {
            const double step = 0.12;
            int[] lead = { 72, 76, 79, 76, 84, 79, 76, 79, 74, 77, 81, 77, 71, 74, 79 };
            int[] bass = { 48, 45, 50, 43 };
            G10_ToneWriter writer = new G10_ToneWriter(step * (lead.Length + 1));
            for (int i = 0; i < lead.Length; i++)
            {
                double length = i == lead.Length - 1 ? step * 2 : step * 0.9;
                writer.Note(i * step, length, Midi(lead[i]), G10_Wave.Square, 0.16);
            }
            for (int i = 0; i < bass.Length; i++) writer.Note(i * step * 4, step * 4, Midi(bass[i]), G10_Wave.Triangle, 0.35);
            return writer;
        }

        static G10_ToneWriter GhostEaten()
        {
            G10_ToneWriter writer = new G10_ToneWriter(0.5);
            writer.Sweep(0, 0.5, 150, 1500, G10_Wave.Square, 0.2, 0.05);
            return writer;
        }

        static G10_ToneWriter ExtraLife()
        {
            G10_ToneWriter writer = new G10_ToneWriter(0.6);
            for (int i = 0; i < 3; i++) writer.Note(i * 0.2, 0.08, 1568, G10_Wave.Square, 0.15);
            return writer;
        }

        // Falling warble that gets lower and lower, then two short pops.
        static G10_ToneWriter Death()
        {
            G10_ToneWriter writer = new G10_ToneWriter(1.6);
            for (int i = 0; i < 10; i++)
                writer.Sweep(i * 0.12, 0.12, 950 - i * 60, 650 - i * 50, G10_Wave.Triangle, 0.35, 0.25);
            writer.Sweep(1.3, 0.08, 220, 60, G10_Wave.Square, 0.2, 0.05);
            writer.Sweep(1.45, 0.08, 220, 60, G10_Wave.Square, 0.2, 0.05);
            return writer;
        }

        // Loop: pitch rises and falls once, so the end joins the start.
        static G10_ToneWriter Siren()
        {
            G10_ToneWriter writer = new G10_ToneWriter(0.4);
            writer.Sweep(0, 0.2, 350, 700, G10_Wave.Sine, 0.25, 0.25);
            writer.Sweep(0.2, 0.2, 700, 350, G10_Wave.Sine, 0.25, 0.25);
            return writer;
        }

        static G10_ToneWriter Frightened()
        {
            G10_ToneWriter writer = new G10_ToneWriter(0.2);
            writer.Sweep(0, 0.1, 160, 320, G10_Wave.Square, 0.1, 0.1);
            writer.Sweep(0.1, 0.1, 160, 320, G10_Wave.Square, 0.1, 0.1);
            return writer;
        }

        static double Midi(int note)
        {
            return 440 * Math.Pow(2, (note - 69) / 12.0);
        }
    }
}
