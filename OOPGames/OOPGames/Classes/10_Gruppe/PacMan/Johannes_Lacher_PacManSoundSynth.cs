using System;
using System.Collections.Generic;
using System.IO;

namespace OOPGames
{
    internal enum G10_Wave
    {
        Square,
        Triangle,
        Sine
    }

    // A generated sound as 16-bit mono samples.
    internal sealed class G10_SoundClip
    {
        public G10_SoundClip(G10_Sound sound, short[] samples)
        {
            Sound = sound;
            Samples = samples;
        }

        public G10_Sound Sound { get; private set; }
        public short[] Samples { get; private set; }

        public int DurationTicks
        {
            get
            {
                int samplesPerTick = G10_SoundSynth.SampleRate / G10_PacManConstants.TicksPerSecond;
                return (Samples.Length + samplesPerTick - 1) / samplesPerTick;
            }
        }

        public byte[] ToWav()
        {
            int dataBytes = Samples.Length * 2;
            using (MemoryStream stream = new MemoryStream(44 + dataBytes))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(new[] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + dataBytes);
                writer.Write(new[] { 'W', 'A', 'V', 'E', 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((short)1);
                writer.Write((short)1);
                writer.Write(G10_SoundSynth.SampleRate);
                writer.Write(G10_SoundSynth.SampleRate * 2);
                writer.Write((short)2);
                writer.Write((short)16);
                writer.Write(new[] { 'd', 'a', 't', 'a' });
                writer.Write(dataBytes);
                foreach (short sample in Samples) writer.Write(sample);
                writer.Flush();
                return stream.ToArray();
            }
        }
    }

    // Mixes tones into a buffer. Every tone fades in and out over a few milliseconds to avoid clicks.
    internal sealed class G10_ToneWriter
    {
        const double FadeSeconds = 0.003;

        readonly float[] _Buffer;

        public G10_ToneWriter(double seconds)
        {
            _Buffer = new float[(int)(seconds * G10_SoundSynth.SampleRate)];
        }

        public void Note(double start, double duration, double hz, G10_Wave wave, double volume)
        {
            Sweep(start, duration, hz, hz, wave, volume, volume);
        }

        // Tone whose pitch glides linearly from fromHz to toHz and whose volume goes from
        // startVolume to endVolume.
        public void Sweep(double start, double duration, double fromHz, double toHz, G10_Wave wave, double startVolume, double endVolume)
        {
            int first = (int)(start * G10_SoundSynth.SampleRate);
            int count = (int)(duration * G10_SoundSynth.SampleRate);
            int fade = Math.Max(1, (int)(FadeSeconds * G10_SoundSynth.SampleRate));
            double phase = 0;
            for (int i = 0; i < count && first + i < _Buffer.Length; i++)
            {
                double t = (double)i / count;
                phase += (fromHz + (toHz - fromHz) * t) / G10_SoundSynth.SampleRate;
                double envelope = Math.Min(1.0, Math.Min((double)i / fade, (double)(count - i) / fade));
                double volume = startVolume + (endVolume - startVolume) * t;
                _Buffer[first + i] += (float)(Shape(wave, phase - Math.Floor(phase)) * volume * envelope);
            }
        }

        public short[] ToSamples()
        {
            short[] samples = new short[_Buffer.Length];
            for (int i = 0; i < _Buffer.Length; i++)
                samples[i] = (short)(Math.Max(-1f, Math.Min(1f, _Buffer[i])) * short.MaxValue);
            return samples;
        }

        static double Shape(G10_Wave wave, double cycle)
        {
            switch (wave)
            {
                case G10_Wave.Square: return cycle < 0.5 ? 1 : -1;
                case G10_Wave.Triangle: return cycle < 0.5 ? 4 * cycle - 1 : 3 - 4 * cycle;
                default: return Math.Sin(cycle * 2 * Math.PI);
            }
        }
    }

    // All sound effects, generated in code (own designs, no recordings of the original).
    // Each clip is built once and cached.
    internal static class G10_SoundSynth
    {
        public const int SampleRate = 22050;

        static readonly Dictionary<G10_Sound, G10_SoundClip> _Clips = new Dictionary<G10_Sound, G10_SoundClip>();

        public static G10_SoundClip Clip(G10_Sound sound)
        {
            G10_SoundClip clip;
            if (!_Clips.TryGetValue(sound, out clip))
            {
                clip = new G10_SoundClip(sound, Create(sound).ToSamples());
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
