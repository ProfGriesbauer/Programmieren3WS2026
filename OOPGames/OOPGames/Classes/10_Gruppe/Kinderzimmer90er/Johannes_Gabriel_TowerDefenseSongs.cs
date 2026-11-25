using System;
using System.Collections.Generic;
using System.Text;

namespace OOPGames
{
    public enum G10_TowerDefenseTrack
    {
        None = -1,
        Menu,
        GreenValley,
        SwampForest,
        WinterPass,
        RiverGorge,
        CursedWasteland,
        MiniBoss,
        Boss
    }

    // The soundtrack: one loop of about a minute per map, plus menu, mini boss and boss music.
    // Style: 16-bit retro, quiet background music (see README "Musik").
    public static class G10_TowerDefenseSongs
    {
        // Map index (0-4) -> its track.
        public static G10_TowerDefenseTrack ForMap(int mapIndex)
        {
            return mapIndex >= 0 && mapIndex < 5 ? G10_TowerDefenseTrack.GreenValley + mapIndex : G10_TowerDefenseTrack.None;
        }

        public static G10_TowerDefenseSong Create(G10_TowerDefenseTrack track)
        {
            switch (track)
            {
                case G10_TowerDefenseTrack.Menu: return Menu();
                case G10_TowerDefenseTrack.GreenValley: return GreenValley();
                case G10_TowerDefenseTrack.SwampForest: return SwampForest();
                case G10_TowerDefenseTrack.WinterPass: return WinterPass();
                case G10_TowerDefenseTrack.RiverGorge: return RiverGorge();
                case G10_TowerDefenseTrack.CursedWasteland: return CursedWasteland();
                case G10_TowerDefenseTrack.MiniBoss: return G10_MiniBoss();
                case G10_TowerDefenseTrack.Boss: return G10_Boss();
                default: return null;
            }
        }

        // ---------------- instruments ----------------

        static G10_TowerDefenseInstrument Flute()
        {
            return new G10_TowerDefenseInstrument
            {
                Wave = G10_TowerDefenseWaveform.Triangle, Attack = 0.04, Decay = 0.2, Sustain = 0.75, Release = 0.15,
                VibratoDepth = 0.18, VibratoRate = 5, VibratoDelay = 0.25
            };
        }

        static G10_TowerDefenseInstrument SoftPluck()
        {
            return new G10_TowerDefenseInstrument
            {
                Wave = G10_TowerDefenseWaveform.Pulse, Duty = 0.25, Attack = 0.004, Decay = 0.25, Sustain = 0.15, Release = 0.1, Cutoff = 2200
            };
        }

        static G10_TowerDefenseInstrument SineBass()
        {
            return new G10_TowerDefenseInstrument { Wave = G10_TowerDefenseWaveform.Sine, Attack = 0.01, Decay = 0.3, Sustain = 0.8, Release = 0.1 };
        }

        static G10_TowerDefenseInstrument OrganPad(double attack)
        {
            return new G10_TowerDefenseInstrument
            {
                Wave = G10_TowerDefenseWaveform.Organ, Attack = attack, Decay = 0.5, Sustain = 0.8, Release = 0.8, Cutoff = 3000
            };
        }

        static G10_TowerDefenseInstrument Bell(double decay = 0.6)
        {
            return new G10_TowerDefenseInstrument
            {
                Wave = G10_TowerDefenseWaveform.Bell, Attack = 0.002, Decay = decay, Sustain = 0.25, Release = 0.4
            };
        }

        static G10_TowerDefenseInstrument Strings()
        {
            return new G10_TowerDefenseInstrument
            {
                Wave = G10_TowerDefenseWaveform.Saw, Attack = 0.35, Decay = 0.5, Sustain = 0.85, Release = 0.6,
                VibratoDepth = 0.08, VibratoRate = 4.5, VibratoDelay = 0.4, Cutoff = 1400
            };
        }

        static G10_TowerDefenseInstrument Horn()
        {
            return new G10_TowerDefenseInstrument
            {
                Wave = G10_TowerDefenseWaveform.Pulse, Duty = 0.5, Attack = 0.03, Decay = 0.2, Sustain = 0.8, Release = 0.12,
                VibratoDepth = 0.15, VibratoRate = 5, VibratoDelay = 0.3, Cutoff = 1800
            };
        }

        static G10_TowerDefenseInstrument Brass()
        {
            return new G10_TowerDefenseInstrument
            {
                Wave = G10_TowerDefenseWaveform.Saw, Attack = 0.02, Decay = 0.15, Sustain = 0.75, Release = 0.1,
                VibratoDepth = 0.12, VibratoRate = 5.5, VibratoDelay = 0.25, Cutoff = 2600
            };
        }

        static G10_TowerDefenseInstrument SharpLead()
        {
            return new G10_TowerDefenseInstrument
            {
                Wave = G10_TowerDefenseWaveform.Pulse, Duty = 0.125, Attack = 0.005, Decay = 0.1, Sustain = 0.55, Release = 0.05,
                VibratoDepth = 0.1, VibratoRate = 6, VibratoDelay = 0.2, Cutoff = 5000
            };
        }

        static G10_TowerDefenseInstrument Lead(double duty = 0.25)
        {
            return new G10_TowerDefenseInstrument
            {
                Wave = G10_TowerDefenseWaveform.Pulse, Duty = duty, Attack = 0.01, Decay = 0.15, Sustain = 0.6, Release = 0.08,
                VibratoDepth = 0.15, VibratoRate = 5.5, VibratoDelay = 0.15, Cutoff = 4500
            };
        }

        static G10_TowerDefenseInstrument Pluck(double duty = 0.125)
        {
            return new G10_TowerDefenseInstrument
            {
                Wave = G10_TowerDefenseWaveform.Pulse, Duty = duty, Attack = 0.004, Decay = 0.12, Sustain = 0.2, Release = 0.05, Cutoff = 3500
            };
        }

        static G10_TowerDefenseInstrument TriangleBass()
        {
            return new G10_TowerDefenseInstrument { Wave = G10_TowerDefenseWaveform.Triangle, Attack = 0.005, Decay = 0.2, Sustain = 0.7, Release = 0.05 };
        }

        // Lowpassed saw: a bass with audible overtones, so it carries even on small speakers.
        static G10_TowerDefenseInstrument SawBass()
        {
            return new G10_TowerDefenseInstrument { Wave = G10_TowerDefenseWaveform.Saw, Attack = 0.005, Decay = 0.15, Sustain = 0.7, Release = 0.05, Cutoff = 650 };
        }

        // Deep sine one octave below the bass line: weight for the "battle" feeling.
        static G10_TowerDefenseInstrument SubBass()
        {
            return new G10_TowerDefenseInstrument { Wave = G10_TowerDefenseWaveform.Sine, Attack = 0.01, Decay = 0.3, Sustain = 0.9, Release = 0.1 };
        }

        // Slow organ with vibrato, used like a choir.
        static G10_TowerDefenseInstrument Choir()
        {
            return new G10_TowerDefenseInstrument
            {
                Wave = G10_TowerDefenseWaveform.Organ, Attack = 0.3, Decay = 0.4, Sustain = 0.85, Release = 0.7,
                VibratoDepth = 0.1, VibratoRate = 4.5, VibratoDelay = 0.3, Cutoff = 2400
            };
        }

        // Drums in blocks of 8 bars: first bar (usually with crash), 6 x beat, fill bar.
        static string EightBars(string first, string beat, string fill, int blocks)
        {
            return G10_TowerDefenseChords.Repeat(first + G10_TowerDefenseChords.Repeat(beat, 6) + fill, blocks);
        }

        // Sub bass: the chord roots one octave below the bass line.
        static string Sub(string chords)
        {
            return G10_TowerDefenseChords.Bass(chords, 1, "R:16", "R:8");
        }

        // The same melody shifted by whole octaves (for doublings).
        static string Octave(string pattern, int octaves)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string raw in pattern.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (raw == "|" || raw.StartsWith("r")) { builder.Append(raw).Append(' '); continue; }
                int colon = raw.IndexOf(':');
                string note = colon >= 0 ? raw.Substring(0, colon) : raw;
                string length = colon >= 0 ? raw.Substring(colon) : "";
                builder.Append(G10_TowerDefenseChords.Note(G10_TowerDefenseSong.NoteToMidi(note) + 12 * octaves)).Append(length).Append(' ');
            }
            return builder.ToString();
        }

        // ---------------- Menue ----------------

        // Calm but already a little heroic: F major, 90 BPM, A B C (20 bars, 53 s). Flute melody over a
        // picked guitar-like arpeggio, strings, deep bass and a soft drum pulse.
        static G10_TowerDefenseSong Menu()
        {
            const string chords = "F C Dm Bb F C Bb,C F | Bb F Gm Dm Bb F Gm C | Dm Bb Gm C";
            const string melody =
                "a4:6 c5:2 f5:4 e5:2 d5:2 | e5:8 g4:4 c5:4 | d5:6 e5:2 f5:4 a5:4 | g5:4 f5:4 d5:8 | " +
                "c5:4 a4:4 c5:4 f5:4 | g5:6 f5:2 e5:4 c5:4 | d5:4 f5:4 e5:4 g5:4 | f5:12 r:4 | " +
                "d5:4 f5:4 bb5:6 a5:2 | a5:8 f5:4 c5:4 | bb4:4 d5:4 g5:6 f5:2 | a5:12 f5:2 e5:2 | " +
                "d5:4 bb4:4 d5:4 f5:4 | c6:6 a5:2 f5:8 | g5:4 a5:4 bb5:4 g5:4 | e5:8 g5:4 c5:4 | " +
                "f5:4 e5:2 d5:2 a4:8 | bb4:4 d5:4 f5:8 | g5:4 f5:2 d5:2 bb4:8 | c5:4 e5:4 g5:6 bb4:2 | ";
            string drums = G10_TowerDefenseChords.Repeat("k:4 h:4 k:2 k:2 h:4 ", 19) + "k:4 h:4 T:2 T:2 t:4 ";

            return new G10_TowerDefenseSong("menue", 90, 20)
                .Add(Flute(), 0.30, melody)
                .Add(SoftPluck(), 0.13, G10_TowerDefenseChords.Arpeggio(chords, 3, new[] { 0, 2, 3, 2, 1, 2, 3, 2 }, 2), 3, 0.25)
                .Add(Strings(), 0.08, G10_TowerDefenseChords.Pad(chords, 4))
                .Add(TriangleBass(), 0.40, G10_TowerDefenseChords.Bass(chords, 2, "R:4 R:4 F:4 R:4", "R:4 F:4"))
                .Add(SubBass(), 0.22, Sub(chords))
                .AddDrums(0.13, drums);
        }

        // ---------------- Gruenes Tal ----------------

        // Cheerful start of the adventure: G major, 120 BPM, form A A2 B A (32 bars, 64 s).
        static G10_TowerDefenseSong GreenValley()
        {
            const string chordsA = "G D Em C G D C,D G";
            const string chordsA2 = "G D Em C Am D G,Em Am,D";
            const string chordsB = "C D Bm Em C D Em,C D";
            string chords = chordsA + " " + chordsA2 + " " + chordsB + " " + chordsA;

            const string melodyA =
                "d5:4 b4:2 d5:2 g5:4 f#5:2 e5:2 | f#5:4 e5:2 d5:2 a4:8 | e5:4 g5:2 e5:2 b4:4 c5:2 d5:2 | e5:6 d5:2 c5:4 b4:2 a4:2 | " +
                "b4:4 d5:2 g5:2 b5:4 a5:2 g5:2 | f#5:4 a5:4 d5:8 | e5:2 f#5:2 g5:2 e5:2 d5:2 c5:2 b4:2 a4:2 | g4:8 r:4 b4:2 c5:2 | ";
            const string melodyA2 =
                "d5:4 b4:2 d5:2 g5:4 a5:2 b5:2 | a5:4 f#5:2 d5:2 a4:8 | g5:4 f#5:2 e5:2 b4:4 e5:4 | g5:6 e5:2 c5:8 | " +
                "c5:4 e5:2 a5:2 g5:4 e5:4 | f#5:4 e5:2 f#5:2 a5:8 | b5:4 a5:2 g5:2 e5:4 g5:4 | a5:4 g5:2 e5:2 f#5:4 d5:4 | ";
            const string melodyB =
                "e5:8 g5:4 e5:4 | f#5:8 a5:4 f#5:4 | d5:4 f#5:4 b5:6 a5:2 | g5:12 f#5:2 e5:2 | " +
                "c5:4 e5:4 g5:4 c6:4 | b5:6 a5:2 f#5:4 d5:4 | e5:4 g5:4 c6:4 b5:2 a5:2 | a5:8 f#5:4 d5:4 | ";
            string melody = melodyA + melodyA2 + melodyB + melodyA;

            string drums = EightBars("k+c:2 h:2 s:2 h:2 k:2 k:2 s:2 h:2 ", "k:2 h:2 s:2 h:2 k:2 k:2 s:2 h:2 ",
                "k:2 h:2 s:2 h:2 s:2 s:2 s:1 s:1 s:2 ", 4);

            return new G10_TowerDefenseSong("gruenes_tal", 120, 32)
                .Add(Lead(), 0.30, melody)
                .Add(Pluck(), 0.11, G10_TowerDefenseChords.Arpeggio(chords, 4, new[] { 0, 1, 2, 3, 2, 1, 0, 1 }, 2), 3, 0.3)
                .Add(Strings(), 0.06, G10_TowerDefenseChords.Pad(chords, 3))
                .Add(TriangleBass(), 0.42, G10_TowerDefenseChords.Bass(chords, 2, "R:4 F:2 R:2 O:4 F:4", "R:4 F:4"))
                .Add(SubBass(), 0.22, Sub(chords))
                .AddDrums(0.27, drums);
        }

        // ---------------- Sumpfwald ----------------

        // Mystical, but with a pulse now: D dorian, 88 BPM, A B C (20 bars, 55 s). Choir, bells with echo,
        // flute, a tom groove and a moving bass.
        static G10_TowerDefenseSong SwampForest()
        {
            const string chords = "Dm G Dm G Am Em F G | F C G Dm F C Em,G Am | F G Dm Dm";
            const string melody =
                "a4:8 d5:4 e5:4 | f5:4 e5:4 d5:4 b4:4 | a4:12 c5:4 | b4:8 d5:8 | " +
                "e5:8 c5:4 a4:4 | b4:8 e5:4 g5:4 | a5:6 g5:2 f5:4 c5:4 | d5:12 r:4 | " +
                "c5:4 f5:4 a5:8 | g5:8 e5:4 c5:4 | d5:4 g5:4 b5:6 a5:2 | a5:12 r:4 | " +
                "f5:4 a5:4 c6:6 b5:2 | g5:8 e5:8 | e5:4 g5:4 d5:4 b4:4 | a4:16 | " +
                "c5:4 a4:4 f4:8 | d5:4 b4:4 g4:8 | f4:4 a4:4 d5:8 | r:16 | ";
            const string groove = "k:4 t:2 t:2 k:4 T:2 t:2 ";
            const string roll = "p:4 t:2 t:2 T:2 T:2 t:2 t:2 ";
            string drums = G10_TowerDefenseChords.Repeat(G10_TowerDefenseChords.Repeat(groove, 3) + roll, 5);

            return new G10_TowerDefenseSong("sumpfwald", 88, 20)
                .Add(Flute(), 0.26, melody, 4, 0.25)
                .Add(Bell(0.8), 0.07, G10_TowerDefenseChords.Arpeggio(chords, 5, new[] { 0, 2, 1, 3 }, 4), 3, 0.4)
                .Add(Choir(), 0.10, G10_TowerDefenseChords.Pad(chords, 3))
                .Add(SineBass(), 0.40, G10_TowerDefenseChords.Bass(chords, 2, "R:4 R:4 F:4 R:4", "R:4 F:4"))
                .Add(SubBass(), 0.20, Sub(chords))
                .AddDrums(0.18, drums);
        }

        // ---------------- Winterpass ----------------

        // Cold and glittering, now driving: E minor with bright major moments, 104 BPM, A B A (24 bars, 55 s).
        static G10_TowerDefenseSong WinterPass()
        {
            const string chordsA = "Em C G D Em C Am B7";
            const string chordsB = "C G D Em C G Am,B7 Em";
            string chords = chordsA + " " + chordsB + " " + chordsA;
            const string melodyA =
                "b4:4 g4:2 e4:2 b4:4 e5:4 | e5:6 d5:2 c5:4 g4:4 | d5:4 b4:2 g4:2 d5:4 g5:4 | f#5:8 e5:4 d5:4 | " +
                "e5:4 g5:4 b5:6 a5:2 | g5:4 e5:4 c5:8 | c5:4 e5:4 a5:4 g5:2 e5:2 | d#5:8 f#5:4 b4:4 | ";
            const string melodyB =
                "g5:8 e5:4 g5:4 | d5:8 b4:4 d5:4 | a5:6 f#5:2 d5:8 | e5:4 f#5:4 g5:4 b5:4 | " +
                "c6:8 b5:4 g5:4 | b5:6 a5:2 g5:4 d5:4 | c5:4 e5:4 d#5:4 f#5:4 | e5:16 | ";
            string melody = melodyA + melodyB + melodyA;
            string drums = EightBars("k+c:2 h:2 s:2 h:2 k:2 h:2 s:2 o:2 ", "k:2 h:2 s:2 h:2 k:2 h:2 s:2 o:2 ",
                "k:2 h:2 s:2 h:2 T:2 T:2 t:2 t:2 ", 3);

            return new G10_TowerDefenseSong("winterpass", 104, 24)
                .Add(Bell(0.5), 0.30, melody, 3, 0.3)
                .Add(Bell(0.35), 0.07, G10_TowerDefenseChords.Arpeggio(chords, 5, new[] { 0, 1, 2, 3, 2, 1 }, 2), 3, 0.45)
                .Add(Strings(), 0.10, G10_TowerDefenseChords.Pad(chords, 3))
                .Add(TriangleBass(), 0.42, G10_TowerDefenseChords.Bass(chords, 2, "R:2 R:2 F:2 R:2 O:2 R:2 F:2 R:2", "R:2 R:2 F:2 R:2"))
                .Add(SubBass(), 0.22, Sub(chords))
                .AddDrums(0.22, drums);
        }

        // ---------------- Flussschlucht ----------------

        // Wide and epic: C minor / E flat major, 100 BPM, A B C (24 bars, 58 s). Horn melody doubled by
        // brass an octave lower, strings, galloping bass, timpani and snare.
        static G10_TowerDefenseSong RiverGorge()
        {
            const string chords = "Cm Ab Eb Bb Cm Ab Fm G | Eb Bb Cm Ab Eb Bb Ab,Bb Eb | Ab Bb Gm Cm Fm Bb Ab G";
            const string melody =
                "c5:8 g5:8 | ab5:6 g5:2 eb5:8 | bb4:4 eb5:4 g5:8 | f5:12 d5:4 | " +
                "c5:4 eb5:4 g5:4 c6:4 | c6:6 bb5:2 ab5:8 | f5:6 g5:2 ab5:4 c6:4 | b5:12 d5:4 | " +
                "g5:8 bb5:8 | f5:8 d5:8 | eb5:4 g5:4 c6:8 | c6:6 bb5:2 ab5:4 eb5:4 | " +
                "bb4:4 eb5:4 g5:4 bb5:4 | d6:8 bb5:4 f5:4 | ab5:4 c6:4 bb5:4 d6:4 | eb6:12 r:4 | " +
                "c5:8 eb5:8 | d5:8 f5:8 | g5:8 bb5:4 d5:4 | c5:12 eb5:4 | " +
                "f5:8 ab5:4 c6:4 | bb5:8 f5:8 | c6:6 bb5:2 ab5:8 | g5:8 b4:4 d5:4 | ";
            string drums = EightBars("p+k+c:4 h:2 h:2 s:4 k:2 h:2 ", "p+k:4 h:2 h:2 s:4 k:2 h:2 ",
                "p:2 p:2 s:2 s:2 s:1 s:1 s:1 s:1 p:2 p:2 ", 3);

            return new G10_TowerDefenseSong("flussschlucht", 100, 24)
                .Add(Horn(), 0.28, melody, 6, 0.2)
                .Add(Brass(), 0.10, Octave(melody, -1))
                .Add(Strings(), 0.11, G10_TowerDefenseChords.Pad(chords, 3))
                .Add(SoftPluck(), 0.07, G10_TowerDefenseChords.Arpeggio(chords, 4, new[] { 0, 1, 2, 3 }, 4), 3, 0.35)
                .Add(SawBass(), 0.30, G10_TowerDefenseChords.Bass(chords, 2, "R:3 R:1 R:2 R:2 F:3 F:1 F:2 F:2", "R:3 R:1 R:2 R:2"))
                .Add(SubBass(), 0.24, Sub(chords))
                .AddDrums(0.26, drums);
        }

        // ---------------- Verfluchtes Oedland ----------------

        // The finale, "now everything is at stake": A minor, 112 BPM, A B C (24 bars, 51 s). Brass melody,
        // choir and organ, saw and sub bass in eighths, a full marching drum kit.
        static G10_TowerDefenseSong CursedWasteland()
        {
            const string chords = "Am F G Am Am F Dm E | F G Em Am Dm Bb E E | Am Am F F Dm Dm E E";
            const string melody =
                "a4:4 c5:4 e5:6 d5:2 | c5:4 a4:4 f5:8 | g5:4 f5:2 e5:2 d5:4 b4:4 | c5:4 b4:2 a4:2 e5:8 | " +
                "a5:6 g5:2 e5:4 c5:4 | f5:4 e5:2 d5:2 c5:4 a4:4 | d5:4 f5:4 a5:4 g5:2 f5:2 | e5:8 g#4:4 b4:4 | " +
                "c6:8 a5:4 f5:4 | d6:8 b5:4 g5:4 | e6:6 d6:2 b5:4 g5:4 | a5:12 e5:4 | " +
                "f5:4 a5:4 d6:6 c6:2 | bb5:8 f5:4 d5:4 | e5:4 g#5:4 b5:4 d6:4 | e6:12 r:4 | " +
                "e5:4 a5:4 c6:6 b5:2 | a5:4 e5:4 c5:8 | f5:4 a5:4 c6:6 a5:2 | f5:12 e5:2 f5:2 | " +
                "d5:4 f5:4 a5:4 d6:4 | c6:6 a5:2 f5:8 | g#5:4 b5:4 e6:4 d6:4 | b5:8 g#5:4 e5:4 | ";
            string drums = EightBars("k+c:2 k:2 s:2 k:1 s:1 k:2 k:2 s:2 s:2 ", "k:2 k:2 s:2 k:1 s:1 k:2 k:2 s:2 s:2 ",
                "s:2 s:2 s:2 s:2 s:1 s:1 s:1 s:1 T:2 t:2 ", 3);

            return new G10_TowerDefenseSong("oedland", 112, 24)
                .Add(Brass(), 0.26, melody)
                .Add(Lead(0.5), 0.08, Octave(melody, 1))
                .Add(Choir(), 0.08, G10_TowerDefenseChords.Pad(chords, 4))
                .Add(OrganPad(0.05), 0.07, G10_TowerDefenseChords.Pad(chords, 3, 8))
                .Add(SawBass(), 0.32, G10_TowerDefenseChords.Bass(chords, 2, G10_TowerDefenseChords.Repeat("R:2", 8), G10_TowerDefenseChords.Repeat("R:2", 4)))
                .Add(SubBass(), 0.26, Sub(chords))
                .AddDrums(0.28, drums);
        }

        // ---------------- Mini-Boss ----------------

        // Tense battle: E minor, 144 BPM, A B A B (32 bars, 53 s). Sharp lead doubled by brass, hard
        // sixteenth saw-bass ostinato with sub bass, string stabs and crashes every four bars.
        static G10_TowerDefenseSong G10_MiniBoss()
        {
            const string chordsA = "Em Em C D Em Em C B7";
            const string chordsB = "Am Am Em Em C D B7 B7";
            string chords = chordsA + " " + chordsB + " " + chordsA + " " + chordsB;
            const string melodyA =
                "e5:2 r:2 e5:2 g5:2 b5:4 a5:2 g5:2 | f#5:4 g5:2 f#5:2 e5:8 | e5:2 r:2 e5:2 g5:2 c6:4 b5:2 a5:2 | a5:4 f#5:4 d5:8 | " +
                "e5:2 r:2 e5:2 g5:2 b5:4 e6:4 | d6:4 b5:2 g5:2 e5:8 | c6:4 b5:2 a5:2 g5:4 e5:4 | d#5:4 f#5:4 a5:4 b5:4 | ";
            const string melodyB =
                "a5:6 g5:2 e5:4 c5:4 | e5:4 a5:4 c6:8 | b5:6 a5:2 g5:4 e5:4 | g5:4 b5:4 e6:8 | " +
                "e6:4 d6:2 c6:2 g5:4 e5:4 | f#5:4 a5:4 d6:8 | d#6:6 b5:2 f#5:4 a5:4 | b5:8 r:8 | ";
            string melody = melodyA + melodyB + melodyA + melodyB;
            const string beat = "k:2 h:2 s:2 k:2 k:2 h:2 s:2 h:2 ";
            const string crash = "k+c:2 h:2 s:2 k:2 k:2 h:2 s:2 h:2 ";
            const string fill = "k:2 s:2 s:2 k:2 s:1 s:1 s:2 T:2 t:2 ";
            string drums = G10_TowerDefenseChords.Repeat(crash + beat + beat + beat + crash + beat + beat + fill, 4);

            return new G10_TowerDefenseSong("miniboss", 144, 32)
                .Add(SharpLead(), 0.22, melody)
                .Add(Brass(), 0.10, Octave(melody, -1))
                .Add(Strings(), 0.08, G10_TowerDefenseChords.Pad(chords, 4, 4))
                .Add(SawBass(), 0.32, G10_TowerDefenseChords.Bass(chords, 2,
                    "R:2 R:2 O:2 R:2 F:2 R:2 O:2 R:2", "R:2 R:2 O:2 R:2"))
                .Add(SubBass(), 0.26, Sub(chords))
                .AddDrums(0.30, drums);
        }

        // ---------------- Boss ----------------

        // Big and threatening: D minor with phrygian E flat, 138 BPM, A B C A (32 bars, 56 s). Fanfare in two
        // octaves (brass + lead), organ and choir, double kick, timpani and the heaviest bass of the soundtrack.
        static G10_TowerDefenseSong G10_Boss()
        {
            const string chordsA = "Dm Eb Dm Eb Dm Bb C,A A";
            const string chordsB = "Gm Dm Gm Dm Bb Eb A A";
            const string chordsC = "Dm C Bb A Dm C Eb A";
            string chords = chordsA + " " + chordsB + " " + chordsC + " " + chordsA;
            const string melodyA =
                "d5:3 d5:1 d5:4 a5:8 | bb5:4 g5:4 eb5:8 | d5:3 d5:1 f5:4 a5:4 d6:4 | eb6:8 bb5:4 g5:4 | " +
                "a5:4 f5:4 d5:4 a4:4 | bb4:4 d5:4 f5:8 | e5:4 g5:4 e5:4 c#5:4 | a4:8 r:8 | ";
            const string melodyB =
                "g5:6 f5:2 d5:4 bb4:4 | a4:8 d5:4 f5:4 | g5:4 bb5:4 d6:8 | d6:4 c6:2 a5:2 f5:8 | " +
                "f5:4 bb5:4 d6:6 c6:2 | bb5:4 g5:4 eb5:8 | c#5:4 e5:4 a5:4 c#6:4 | e6:8 c#6:4 a5:4 | ";
            const string melodyC =
                "f5:4 a5:4 d6:8 | e6:6 d6:2 c6:4 g5:4 | d6:4 bb5:4 f5:8 | e5:4 c#5:4 a4:8 | " +
                "d5:3 d5:1 f5:4 a5:4 d6:4 | c6:4 g5:4 e5:8 | eb5:4 g5:4 bb5:8 | a5:8 e5:4 c#5:4 | ";
            string melody = melodyA + melodyB + melodyC + melodyA;
            const string beat = "k:1 k:1 h:2 s:2 k:1 k:1 k:2 h:2 s:2 k:2 ";
            const string crash = "p+k+c:1 k:1 h:2 s:2 k:1 k:1 k:2 h:2 s:2 k:2 ";
            const string fill = "k:2 s:2 k:2 s:2 T:2 T:2 p:2 p:2 ";
            string drums = EightBars(crash, beat, fill, 4);

            return new G10_TowerDefenseSong("boss", 138, 32)
                .Add(Brass(), 0.24, melody)
                .Add(Lead(0.5), 0.09, Octave(melody, 1))
                .Add(OrganPad(0.03), 0.08, G10_TowerDefenseChords.Pad(chords, 3, 8))
                .Add(Choir(), 0.06, G10_TowerDefenseChords.Pad(chords, 4))
                .Add(SawBass(), 0.34, G10_TowerDefenseChords.Bass(chords, 2,
                    "R:2 R:2 R:2 R:2 L:2 L:2 R:2 R:2", "R:2 R:2 R:2 R:2"))
                .Add(SubBass(), 0.28, Sub(chords))
                .AddDrums(0.32, drums);
        }
    }
}
