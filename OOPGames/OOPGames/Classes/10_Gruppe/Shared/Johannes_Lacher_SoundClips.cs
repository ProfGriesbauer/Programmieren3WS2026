using System;
using System.IO;

namespace OOPGames
{
    internal enum G10_Wave
    {
        Square,
        Triangle,
        Sine,
        // Pitched noise like the C64 sound chip: a new random level every period.
        Noise
    }

    // A generated sound as 16-bit mono samples. The name must be unique among all clips of a game;
    // it also names the sound file.
    public sealed class G10_SoundClip
    {
        public const int SampleRate = 22050;
        // The hub updates the games every 40 ms.
        const int TicksPerSecond = 25;

        public G10_SoundClip(string name, short[] samples)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("A clip needs a name.", nameof(name));
            if (samples == null) throw new ArgumentNullException(nameof(samples));
            Name = name;
            Samples = samples;
        }

        public string Name { get; private set; }
        public short[] Samples { get; private set; }

        public int DurationTicks
        {
            get
            {
                int samplesPerTick = SampleRate / TicksPerSecond;
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
                writer.Write(SampleRate);
                writer.Write(SampleRate * 2);
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
            _Buffer = new float[(int)(seconds * G10_SoundClip.SampleRate)];
        }

        public void Note(double start, double duration, double hz, G10_Wave wave, double volume)
        {
            Sweep(start, duration, hz, hz, wave, volume, volume);
        }

        // Tone whose pitch glides linearly from fromHz to toHz and whose volume goes from
        // startVolume to endVolume.
        public void Sweep(double start, double duration, double fromHz, double toHz, G10_Wave wave, double startVolume, double endVolume)
        {
            int first = (int)(start * G10_SoundClip.SampleRate);
            int count = (int)(duration * G10_SoundClip.SampleRate);
            int fade = Math.Max(1, (int)(FadeSeconds * G10_SoundClip.SampleRate));
            double phase = 0;
            for (int i = 0; i < count && first + i < _Buffer.Length; i++)
            {
                double t = (double)i / count;
                phase += (fromHz + (toHz - fromHz) * t) / G10_SoundClip.SampleRate;
                double envelope = Math.Min(1.0, Math.Min((double)i / fade, (double)(count - i) / fade));
                double volume = startVolume + (endVolume - startVolume) * t;
                double level = wave == G10_Wave.Noise ? NoiseLevel((long)phase) : Shape(wave, phase - Math.Floor(phase));
                _Buffer[first + i] += (float)(level * volume * envelope);
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

        // Random level between -1 and 1 for one period; the same period always gives the same level,
        // so the clips do not change between runs.
        static double NoiseLevel(long period)
        {
            uint x = (uint)period * 2654435761u;
            x ^= x >> 15;
            x *= 2246822519u;
            x ^= x >> 13;
            return (x & 0xFFFF) / 32767.5 - 1;
        }
    }
}
