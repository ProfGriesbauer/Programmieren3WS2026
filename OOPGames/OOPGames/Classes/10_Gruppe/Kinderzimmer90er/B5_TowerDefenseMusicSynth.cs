using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OOPGames
{
    public enum TowerDefenseWaveform
    {
        Pulse,
        Triangle,
        Sine,
        Saw,
        // Sine with a few harmonics, like a soft organ.
        Organ,
        // Sine with fast decaying inharmonic partials, like a glockenspiel.
        Bell
    }

    // Sound of one voice: wave form, ADSR envelope (seconds, sustain 0..1), delayed vibrato and a
    // low-pass filter that softens the raw wave (the "16-bit" warmth compared with raw 8-bit tones).
    public sealed class TowerDefenseInstrument
    {
        public TowerDefenseWaveform Wave = TowerDefenseWaveform.Pulse;
        public double Duty = 0.5;
        public double Attack = 0.005, Decay = 0.1, Sustain = 0.7, Release = 0.05;
        public double VibratoDepth, VibratoRate = 5.5, VibratoDelay = 0.2;
        // Low-pass cut-off in Hz, 0 = unfiltered.
        public double Cutoff;
    }

    // One loop of music in a small tracker-like text notation, rendered to 16-bit mono samples.
    //
    // Melodic patterns: tokens separated by spaces, "|" is ignored (bar lines for reading).
    //   c5, f#4, bb3      note with octave (c4 = middle C)
    //   c4+e4+g4          chord
    //   r                 rest
    //   :n                length in sixteenths, e.g. "g4:8"; the length sticks until the next ":n"
    // Drum patterns use k (kick), s (snare), h (closed hat), o (open hat), t (low tom), T (high tom),
    // p (timpani), c (crash), r or - (rest); several at once with "+", e.g. "k+c:4".
    //
    // Every voice must fill exactly Bars x 16 sixteenths. Notes and echoes that ring past the end of
    // the loop continue at its start, so the loop repeats without a seam.
    public sealed class TowerDefenseSong
    {
        public const int SampleRate = G10_SoundClip.SampleRate;

        sealed class Voice
        {
            public TowerDefenseInstrument Instrument;
            public string Pattern;
            public double Gain;
            public bool Drums;
            public int EchoSixteenths;
            public double EchoFeedback;
        }

        readonly List<Voice> _Voices = new List<Voice>();

        public TowerDefenseSong(string name, double bpm, int bars)
        {
            Name = name;
            Bpm = bpm;
            Bars = bars;
        }

        public string Name { get; private set; }
        public double Bpm { get; private set; }
        public int Bars { get; private set; }
        public double SixteenthSeconds { get { return 60.0 / Bpm / 4; } }
        public double Seconds { get { return Bars * 16 * SixteenthSeconds; } }

        public TowerDefenseSong Add(TowerDefenseInstrument instrument, double gain, string pattern, int echoSixteenths = 0, double echoFeedback = 0)
        {
            _Voices.Add(new Voice { Instrument = instrument, Gain = gain, Pattern = pattern, EchoSixteenths = echoSixteenths, EchoFeedback = echoFeedback });
            return this;
        }

        public TowerDefenseSong AddDrums(double gain, string pattern)
        {
            _Voices.Add(new Voice { Gain = gain, Pattern = pattern, Drums = true });
            return this;
        }

        // Throws if a voice does not fill exactly the length of the song.
        public void Validate()
        {
            for (int i = 0; i < _Voices.Count; i++)
            {
                int total = TotalSixteenths(_Voices[i].Pattern);
                if (total != Bars * 16)
                    throw new InvalidOperationException(Name + ": voice " + i + " has " + total + " sixteenths, expected " + Bars * 16);
            }
        }

        // Renders the loop and scales it to a common loudness so all songs sound equally loud.
        public short[] Render()
        {
            Validate();
            int total = (int)Math.Round(Seconds * SampleRate);
            float[] mix = new float[total];
            foreach (Voice voice in _Voices)
            {
                float[] buffer = new float[total];
                foreach (Event e in Parse(voice.Pattern))
                {
                    double start = e.Start * SixteenthSeconds, length = e.Length * SixteenthSeconds;
                    foreach (string token in e.Sounds)
                    {
                        if (voice.Drums) Drum(buffer, token, start);
                        else Tone(buffer, voice.Instrument, MidiToHz(NoteToMidi(token)), start, length);
                    }
                }
                if (!voice.Drums && voice.Instrument.Cutoff > 0) LowPass(buffer, voice.Instrument.Cutoff);
                if (voice.EchoSixteenths > 0) Echo(buffer, (int)Math.Round(voice.EchoSixteenths * SixteenthSeconds * SampleRate), voice.EchoFeedback);
                for (int i = 0; i < total; i++) mix[i] += (float)(buffer[i] * voice.Gain);
            }
            return Normalize(mix);
        }

        // ---------------- pattern parsing ----------------

        struct Event
        {
            public int Start, Length;
            public string[] Sounds;
        }

        static IEnumerable<Event> Parse(string pattern)
        {
            int position = 0, length = 4;
            foreach (string raw in pattern.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (raw == "|") continue;
                string token = raw;
                int colon = token.IndexOf(':');
                if (colon >= 0)
                {
                    length = int.Parse(token.Substring(colon + 1), CultureInfo.InvariantCulture);
                    token = token.Substring(0, colon);
                }
                if (token != "r" && token != "-")
                    yield return new Event { Start = position, Length = length, Sounds = token.Split('+') };
                position += length;
            }
        }

        static int TotalSixteenths(string pattern)
        {
            int position = 0, length = 4;
            foreach (string raw in pattern.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (raw == "|") continue;
                int colon = raw.IndexOf(':');
                if (colon >= 0) length = int.Parse(raw.Substring(colon + 1), CultureInfo.InvariantCulture);
                position += length;
            }
            return position;
        }

        static readonly int[] PitchClass = { 9, 11, 0, 2, 4, 5, 7 }; // a b c d e f g

        public static int NoteToMidi(string note)
        {
            int pc = PitchClass[char.ToLowerInvariant(note[0]) - 'a'];
            int i = 1;
            while (i < note.Length && (note[i] == '#' || note[i] == 'b'))
            {
                pc += note[i] == '#' ? 1 : -1;
                i++;
            }
            int octave = int.Parse(note.Substring(i), CultureInfo.InvariantCulture);
            return 12 * (octave + 1) + pc;
        }

        static double MidiToHz(int midi) { return 440.0 * Math.Pow(2, (midi - 69) / 12.0); }

        // ---------------- synthesis ----------------

        static void Tone(float[] buffer, TowerDefenseInstrument ins, double hz, double start, double length)
        {
            int first = (int)Math.Round(start * SampleRate);
            double ring = length + ins.Release;
            int count = (int)(ring * SampleRate);
            double phase = 0;
            double levelAtEnd = Envelope(ins, length);
            for (int i = 0; i < count; i++)
            {
                double t = (double)i / SampleRate;
                double env = t < length ? Envelope(ins, t) : levelAtEnd * (1 - (t - length) / Math.Max(1e-4, ins.Release));
                if (env <= 0) continue;
                double f = hz;
                if (ins.VibratoDepth > 0 && t > ins.VibratoDelay)
                {
                    double depth = ins.VibratoDepth * Math.Min(1, (t - ins.VibratoDelay) / 0.3);
                    f *= Math.Pow(2, depth * Math.Sin(2 * Math.PI * ins.VibratoRate * t) / 12);
                }
                phase += f / SampleRate;
                double value = Oscillator(ins, phase, t);
                int index = (first + i) % buffer.Length;
                buffer[index] += (float)(value * env);
            }
        }

        static double Envelope(TowerDefenseInstrument ins, double t)
        {
            if (t < ins.Attack) return t / ins.Attack;
            t -= ins.Attack;
            if (t < ins.Decay) return 1 - (1 - ins.Sustain) * t / ins.Decay;
            return ins.Sustain;
        }

        static double Oscillator(TowerDefenseInstrument ins, double phase, double t)
        {
            double cycle = phase - Math.Floor(phase);
            double angle = 2 * Math.PI * phase;
            switch (ins.Wave)
            {
                case TowerDefenseWaveform.Pulse: return (cycle < ins.Duty ? 1 : -1) - (2 * ins.Duty - 1);
                case TowerDefenseWaveform.Triangle: return cycle < 0.5 ? 4 * cycle - 1 : 3 - 4 * cycle;
                case TowerDefenseWaveform.Saw: return 2 * cycle - 1;
                case TowerDefenseWaveform.Organ:
                    return 0.6 * Math.Sin(angle) + 0.3 * Math.Sin(2 * angle) + 0.15 * Math.Sin(3 * angle) + 0.08 * Math.Sin(4 * angle);
                case TowerDefenseWaveform.Bell:
                    return Math.Sin(angle) + 0.45 * Math.Sin(2.76 * angle) * Math.Exp(-t * 7) + 0.25 * Math.Sin(5.4 * angle) * Math.Exp(-t * 12);
                default: return Math.Sin(angle);
            }
        }

        static void Drum(float[] buffer, string kind, double start)
        {
            int first = (int)Math.Round(start * SampleRate);
            uint seed = (uint)(first * 2654435761u + kind[0]);
            switch (kind)
            {
                case "k": Thump(buffer, first, 150, 45, 0.18, 1.0, 0); break;
                case "t": Thump(buffer, first, 190, 90, 0.25, 0.7, 0.1); break;
                case "T": Thump(buffer, first, 280, 150, 0.2, 0.6, 0.1); break;
                case "p": Thump(buffer, first, 75, 62, 0.9, 0.9, 0.08); break;
                case "s": Noise(buffer, first, 0.16, 0.55, seed, 0.35); Thump(buffer, first, 200, 170, 0.08, 0.35, 0); break;
                case "h": Noise(buffer, first, 0.04, 0.3, seed, 0.9); break;
                case "o": Noise(buffer, first, 0.22, 0.25, seed, 0.9); break;
                case "c": Noise(buffer, first, 1.1, 0.35, seed, 0.75); break;
                default: throw new FormatException("Unknown drum: " + kind);
            }
        }

        // Pitched drum: a sine that glides down while it fades out; noise adds a little attack.
        static void Thump(float[] buffer, int first, double fromHz, double toHz, double seconds, double volume, double noise)
        {
            int count = (int)(seconds * SampleRate);
            double phase = 0;
            uint state = (uint)first | 1;
            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                phase += (toHz + (fromHz - toHz) * Math.Exp(-t * 6)) / SampleRate;
                double env = Math.Exp(-t * 5) * Math.Min(1, i / 40.0);
                double value = Math.Sin(2 * Math.PI * phase) + noise * NextNoise(ref state);
                buffer[(first + i) % buffer.Length] += (float)(value * env * volume);
            }
        }

        // Noise burst; brightness 0..1 removes the low part (higher = more hiss, like hi-hats).
        static void Noise(float[] buffer, int first, double seconds, double volume, uint seed, double brightness)
        {
            int count = (int)(seconds * SampleRate);
            uint state = seed | 1;
            double low = 0;
            for (int i = 0; i < count; i++)
            {
                double t = (double)i / count;
                double n = NextNoise(ref state);
                low += 0.2 * (n - low);
                double value = n - brightness * low;
                double env = Math.Exp(-t * 6) * Math.Min(1, i / 20.0);
                buffer[(first + i) % buffer.Length] += (float)(value * env * volume);
            }
        }

        static double NextNoise(ref uint state)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return (state & 0xFFFF) / 32767.5 - 1;
        }

        // One-pole low-pass, run twice around the loop so its start continues smoothly from its end.
        static void LowPass(float[] buffer, double cutoff)
        {
            double a = 1 - Math.Exp(-2 * Math.PI * cutoff / SampleRate);
            double y = 0;
            float[] source = (float[])buffer.Clone();
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i < buffer.Length; i++)
                {
                    y += a * (source[i] - y);
                    buffer[i] = (float)y;
                }
        }

        // Feedback echo around the loop (two passes so the tail of the end reaches the start).
        static void Echo(float[] buffer, int delay, double feedback)
        {
            if (delay <= 0 || delay >= buffer.Length) return;
            float[] dry = (float[])buffer.Clone();
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] = (float)(dry[i] + feedback * buffer[(i - delay + buffer.Length) % buffer.Length]);
        }

        // Same loudness for every song (RMS), but never above 0.9 at the loudest peak.
        static short[] Normalize(float[] mix)
        {
            double peak = 1e-6, sum = 0;
            foreach (float v in mix)
            {
                peak = Math.Max(peak, Math.Abs(v));
                sum += v * v;
            }
            double rms = Math.Sqrt(sum / Math.Max(1, mix.Length));
            double scale = Math.Min(0.9 / peak, 0.16 / Math.Max(1e-6, rms));
            short[] samples = new short[mix.Length];
            for (int i = 0; i < mix.Length; i++) samples[i] = (short)Math.Round(Math.Max(-1, Math.Min(1, mix[i] * scale)) * short.MaxValue);
            return samples;
        }
    }

    // Builds pattern strings from chord symbols, so accompaniments do not have to be written note by note.
    // Chords are given per bar, separated by spaces ("G Em C D"); "C,D" splits a bar into two halves.
    // Chord symbols: root (C, F#, Bb ...) plus "", m, 7, maj7, m7, dim, sus2, sus4, 5.
    public static class TowerDefenseChords
    {
        static readonly Dictionary<string, int[]> Qualities = new Dictionary<string, int[]>
        {
            { "", new[] { 0, 4, 7 } }, { "m", new[] { 0, 3, 7 } }, { "7", new[] { 0, 4, 7, 10 } },
            { "maj7", new[] { 0, 4, 7, 11 } }, { "m7", new[] { 0, 3, 7, 10 } }, { "dim", new[] { 0, 3, 6 } },
            { "sus2", new[] { 0, 2, 7 } }, { "sus4", new[] { 0, 5, 7 } }, { "5", new[] { 0, 7 } }
        };

        static readonly string[] Names = { "c", "c#", "d", "eb", "e", "f", "f#", "g", "ab", "a", "bb", "b" };

        public static string Repeat(string pattern, int times)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < times; i++) builder.Append(pattern).Append(' ');
            return builder.ToString();
        }

        // Chord tones from the root in the given octave; index n >= count continues an octave higher.
        public static int[] Tones(string chord, int octave)
        {
            int length = chord.Length > 1 && (chord[1] == '#' || chord[1] == 'b') ? 2 : 1;
            int root = TowerDefenseSong.NoteToMidi(chord.Substring(0, length).ToLowerInvariant() + octave);
            int[] intervals = Qualities[chord.Substring(length)];
            int[] tones = new int[intervals.Length];
            for (int i = 0; i < intervals.Length; i++) tones[i] = root + intervals[i];
            return tones;
        }

        static int Tone(int[] tones, int index)
        {
            return tones[index % tones.Length] + 12 * (index / tones.Length);
        }

        public static string Note(int midi)
        {
            return Names[midi % 12] + (midi / 12 - 1).ToString(CultureInfo.InvariantCulture);
        }

        // Splits "G Em C,D" into (chord, sixteenths) segments.
        static IEnumerable<KeyValuePair<string, int>> Segments(string chords)
        {
            foreach (string bar in chords.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (bar == "|") continue;
                string[] parts = bar.Split(',');
                foreach (string part in parts) yield return new KeyValuePair<string, int>(part, 16 / parts.Length);
            }
        }

        // Broken chords: order gives the chord-tone indices that are cycled in steps of `step` sixteenths.
        public static string Arpeggio(string chords, int octave, int[] order, int step)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, int> segment in Segments(chords))
            {
                int[] tones = Tones(segment.Key, octave);
                for (int i = 0; i < segment.Value / step; i++)
                    builder.Append(Note(Tone(tones, order[i % order.Length]))).Append(':').Append(step).Append(' ');
            }
            return builder.ToString();
        }

        // Bass line: rhythm tokens R (root), T (third), F (fifth), O (octave), L (fifth below), "-" (rest),
        // each with ":n". The full rhythm is used for whole bars, the half rhythm for half-bar chords.
        public static string Bass(string chords, int octave, string fullBar, string halfBar)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, int> segment in Segments(chords))
            {
                int[] tones = Tones(segment.Key, octave);
                foreach (string raw in (segment.Value == 16 ? fullBar : halfBar).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parts = raw.Split(':');
                    string length = ":" + parts[1];
                    switch (parts[0])
                    {
                        case "R": builder.Append(Note(tones[0])); break;
                        case "T": builder.Append(Note(tones[1 % tones.Length])); break;
                        case "F": builder.Append(Note(tones[0] + 7)); break;
                        case "O": builder.Append(Note(tones[0] + 12)); break;
                        case "L": builder.Append(Note(tones[0] - 5)); break;
                        default: builder.Append('r'); break;
                    }
                    builder.Append(length).Append(' ');
                }
            }
            return builder.ToString();
        }

        // Held chords, one per segment (or repeated every `step` sixteenths).
        public static string Pad(string chords, int octave, int step = 0)
        {
            StringBuilder builder = new StringBuilder();
            foreach (KeyValuePair<string, int> segment in Segments(chords))
            {
                int[] tones = Tones(segment.Key, octave);
                string chord = string.Join("+", Array.ConvertAll(tones, Note));
                int hit = step > 0 ? step : segment.Value;
                for (int i = 0; i < segment.Value / hit; i++) builder.Append(chord).Append(':').Append(hit).Append(' ');
            }
            return builder.ToString();
        }
    }
}
