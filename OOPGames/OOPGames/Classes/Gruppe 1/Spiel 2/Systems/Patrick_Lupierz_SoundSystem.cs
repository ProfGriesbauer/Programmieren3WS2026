using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_SoundSystem
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: Hintergrundmusik je Ort (Stadt, Zuhause, Läden, Casino, Spielhalle, Angeln, Titel) und
    //               Geräusche (Gewinn, Jackpot, Verlust, Münze, Tür, Miau, Wuff, Platschen, Fang).
    //               Keine Audiodateien im Projekt und keine externe Bibliothek: Die Stücke werden beim ersten
    //               Gebrauch als 8-Bit-Chiptune (G1C_ChipTune) erzeugt, als WAV in den Temp-Ordner geschrieben und
    //               mit WPF-MediaPlayer abgespielt. Ohne Enable() (z. B. im Selbsttest) bleibt alles stumm; die
    //               Logik (welche Musik, welches Geräusch) ist trotzdem prüfbar. Fehler schalten den Ton ab.
    // =====================================================
    public sealed class G1C_SoundSystem
    {
        public const double MusicVolume = 0.30;
        public const double EffectVolume = 0.55;

        private readonly Dictionary<string, string> _files = new Dictionary<string, string>();
        private readonly List<MediaPlayer> _effects = new List<MediaPlayer>();
        private MediaPlayer _music;
        private DateTime _lastHeartbeat = DateTime.MinValue;

        public bool Enabled { get; private set; }
        public bool Muted { get; private set; }
        public string CurrentMusic { get; private set; }
        public string LastEffect { get; private set; }
        public int EffectCount { get; private set; }
        public string LastError { get; private set; }

        /// <summary>Nur das echte Spiel (G1C_CasinoRules) schaltet den Ton ein; Tests bleiben stumm.</summary>
        public void Enable()
        {
            Enabled = true;
        }

        public string ToggleMute()
        {
            Muted = !Muted;
            try
            {
                if (_music != null) _music.Volume = Muted ? 0 : MusicVolume;
            }
            catch (Exception ex) { Fail(ex); }
            return Muted ? "Ton aus." : "Ton an.";
        }

        /// <summary>Jeder Spiel-Tick meldet sich hier. Kommt kein Tick mehr (anderes Spiel im Hauptfenster gewählt),
        /// wird die Musik am Ende des Stücks nicht wiederholt – sie läuft also nicht ewig weiter.</summary>
        public void Heartbeat()
        {
            _lastHeartbeat = DateTime.Now;
        }

        // Entwickler: Patrick Lupierz – Musik wechseln; gleiche Musik läuft ohne Unterbrechung weiter
        public void SetMusic(string theme)
        {
            Heartbeat();
            if (theme == CurrentMusic) return;
            CurrentMusic = theme;
            if (!Enabled) return;
            try
            {
                if (_music != null) { _music.Stop(); _music.Close(); _music = null; }
                if (theme == null) return;
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(Resolve(FolderFor(theme), theme) ?? FileFor("music_" + theme), UriKind.Absolute));
                player.Volume = Muted ? 0 : MusicVolume;
                player.MediaEnded += (s, e) =>
                {
                    if ((DateTime.Now - _lastHeartbeat).TotalSeconds > 2) { player.Close(); _music = null; CurrentMusic = null; return; }
                    player.Position = TimeSpan.Zero;
                    player.Play();
                };
                player.Play();
                _music = player;
            }
            catch (Exception ex) { Fail(ex); }
        }

        // Entwickler: Patrick Lupierz – Geräusch abspielen (mehrere gleichzeitig möglich, max. 6)
        public void Play(string effect)
        {
            if (string.IsNullOrEmpty(effect)) return;
            LastEffect = effect;
            EffectCount++;
            if (!Enabled || Muted) return;
            try
            {
                _effects.RemoveAll(p => p.NaturalDuration.HasTimeSpan && p.Position >= p.NaturalDuration.TimeSpan);
                if (_effects.Count >= 6) { _effects[0].Close(); _effects.RemoveAt(0); }
                MediaPlayer player = new MediaPlayer();
                player.Open(new Uri(Resolve("Soundeffekte", effect) ?? FileFor("sfx_" + effect), UriKind.Absolute));
                player.Volume = EffectVolume;
                player.MediaEnded += (s, e) => player.Close();
                player.Play();
                _effects.Add(player);
            }
            catch (Exception ex) { Fail(ex); }
        }

        private void Fail(Exception ex)
        {
            LastError = ex.Message;
            Enabled = false; // ohne Audiogerät o. Ä. läuft das Spiel stumm weiter
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026
        // Aufgabe: Eigene Audiodateien aus G1Casino_Assets\Sounds\<Ordner> (wav oder mp3) bevorzugen.
        public const string SoundFolderName = "Sounds";

        /// <summary>Unterordner für Musik: Außen (Stadt, Angeln) oder Gebäude (alles andere).</summary>
        public static string FolderFor(string theme)
        {
            return theme == "town" || theme == "fishing" ? "Musik_Aussen" : "Musik_Gebaeude";
        }

        /// <summary>Sucht eine eigene Datei; null, wenn keine vorhanden ist (dann greift die eingebaute Version).</summary>
        public static string Resolve(string folder, string name)
        {
            try
            {
                string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "G1Casino_Assets", SoundFolderName, folder);
                foreach (string ext in new[] { ".wav", ".mp3" })
                {
                    string file = Path.Combine(dir, name + ext);
                    if (File.Exists(file)) return file;
                }
            }
            catch (Exception)
            {
                // Zugriffsfehler: eingebaute Version verwenden
            }
            return null;
        }

        private string FileFor(string name)
        {
            string file;
            if (_files.TryGetValue(name, out file)) return file;
            string folder = Path.Combine(Path.GetTempPath(), "G1Casino_Sounds");
            Directory.CreateDirectory(folder);
            file = Path.Combine(folder, name + ".wav");
            File.WriteAllBytes(file, G1C_ChipTune.Render(name));
            _files[name] = file;
            return file;
        }

        // Entwickler: Patrick Lupierz – welche Musik zu welchem Ort/Zustand gehört (reine Logik, testbar)
        public static string MusicFor(G1C_GameState state, G1C_WorldLocation location, G1C_CasinoGameBase activeGame)
        {
            if (state == G1C_GameState.Title) return "title";
            if (activeGame is G1C_FishingGame) return "fishing";
            if (state == G1C_GameState.Arcade || location == G1C_WorldLocation.Arcade) return "arcade";
            if (activeGame != null || location == G1C_WorldLocation.Casino) return "casino";
            switch (location)
            {
                case G1C_WorldLocation.Home:
                case G1C_WorldLocation.GrandmaHouse: return "home";
                case G1C_WorldLocation.Exterior: return "town";
                default: return "shop";
            }
        }
    }

    // =====================================================
    // Klasse: G1C_ChipTune
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: Kleiner Synthesizer: erzeugt WAV-Daten (22050 Hz, 16 Bit, mono) aus Notenfolgen mit
    //               Rechteck-Melodie und Dreieck-Bass sowie Rauschen für Platschen/Bellen. Noten als Text:
    //               "C5:2 E5:2 R:4" = Ton C5 zwei Sechzehntel, E5 zwei Sechzehntel, vier Sechzehntel Pause.
    // =====================================================
    public static class G1C_ChipTune
    {
        public const int SampleRate = 22050;

        private static readonly Dictionary<string, string[]> Songs = new Dictionary<string, string[]>
        {
            // { Melodie, Bass, Tempo }
            { "music_town", new[] { "E5:2 G5:2 C6:4 B5:2 G5:2 A5:4 G5:2 E5:2 F5:2 D5:2 E5:4 R:4 C5:2 E5:2 G5:4 A5:2 G5:2 F5:4 E5:2 D5:2 C5:2 D5:2 C5:4 R:4",
                                    "C3:8 G2:8 F2:8 C3:8 A2:8 F2:8 G2:8 C3:8", "132" } },
            { "music_home", new[] { "C5:4 E5:4 G5:8 F5:4 E5:4 D5:8 E5:4 G5:4 C6:8 B5:4 G5:4 C5:8",
                                    "C3:16 F2:16 C3:16 G2:8 C3:8", "84" } },
            { "music_shop", new[] { "G5:2 R:1 G5:1 E5:2 C5:2 D5:3 E5:1 R:4 A5:2 R:1 A5:1 F5:2 D5:2 G5:4 R:4 E5:2 G5:2 C6:2 G5:2 A5:2 F5:2 D5:4 E5:2 D5:2 C5:4 R:4",
                                    "C3:4 G2:4 C3:4 G2:4 F2:4 C3:4 G2:4 D3:4 C3:4 E3:4 F2:4 D3:4 G2:4 G2:4 C3:8", "112" } },
            { "music_casino", new[] { "A4:3 C5:1 E5:2 A5:2 G5:3 E5:1 D5:4 F5:3 E5:1 D5:2 C5:2 B4:4 E5:4 A4:3 C5:1 E5:2 A5:2 C6:3 B5:1 A5:2 G5:2 E5:4 D5:2 B4:2 A4:8",
                                      "A2:4 E3:4 A2:4 E3:4 D3:4 A2:4 E2:4 G#2:4 A2:4 E3:4 A2:4 C3:4 E3:4 E2:4 A2:8", "116" } },
            { "music_arcade", new[] { "C6:1 G5:1 E5:1 G5:1 C6:1 G5:1 E5:1 G5:1 D6:1 A5:1 F5:1 A5:1 D6:1 A5:1 F5:1 A5:1 E6:2 C6:2 G5:2 E5:2 F5:2 A5:2 C6:2 F6:2 E6:2 D6:2 C6:2 B5:2 C6:4 R:4",
                                      "C3:2 C4:2 C3:2 C4:2 D3:2 D4:2 D3:2 D4:2 E3:2 E4:2 C3:2 C4:2 F3:2 F4:2 F3:2 F4:2 G3:2 G4:2 G3:2 G4:2 C3:4 R:4", "160" } },
            { "music_fishing", new[] { "D5:4 F#5:4 A5:8 G5:4 F#5:4 E5:8 D5:4 E5:4 F#5:4 A5:4 B5:8 A5:8",
                                       "D3:16 A2:16 G2:16 D3:16", "76" } },
            { "music_title", new[] { "C5:2 C5:1 C5:1 G5:4 E5:2 C5:2 G5:4 A5:2 B5:2 C6:4 G5:4 R:4 F5:2 A5:2 C6:4 B5:2 G5:2 C6:8 R:8",
                                     "C3:8 C3:8 F2:8 G2:8 F2:8 G2:4 C3:12", "120" } },
            { "sfx_win", new[] { "C5:1 E5:1 G5:1 C6:3", "", "300" } },
            { "sfx_jackpot", new[] { "C5:1 E5:1 G5:1 C6:1 E6:1 G6:1 C6:1 E6:1 G6:1 C7:6", "C3:4 G3:4 C4:4", "320" } },
            { "sfx_lose", new[] { "G4:2 E4:2 C4:4", "", "220" } },
            { "sfx_coin", new[] { "B5:1 E6:3", "", "420" } },
            { "sfx_catch", new[] { "G5:1 B5:1 D6:1 G6:4", "", "300" } },
            { "sfx_door", new[] { "C3:2", "", "240" } },
            { "sfx_meow", new[] { "A5:1 F5:1 D5:3", "", "360" } },
            { "sfx_click", new[] { "C6:1", "", "600" } },
        };

        // Entwickler: Patrick Lupierz – fertige WAV-Datei (Header + PCM) für ein Stück oder Geräusch
        public static byte[] Render(string name)
        {
            float[] samples;
            if (name == "sfx_splash") samples = Noise(0.35, 0.9, false);
            else if (name == "sfx_woof") samples = Noise(0.18, 0.6, true);
            else
            {
                string[] song;
                if (!Songs.TryGetValue(name, out song)) song = Songs["sfx_click"];
                int tempo = int.Parse(song[2]);
                bool music = name.StartsWith("music_", StringComparison.Ordinal);
                float[] lead = Voice(song[0], tempo, true, music ? 0.22f : 0.35f);
                float[] bass = song[1].Length > 0 ? Voice(song[1], tempo, false, 0.25f) : new float[0];
                samples = new float[Math.Max(lead.Length, bass.Length)];
                for (int i = 0; i < samples.Length; i++)
                    samples[i] = (i < lead.Length ? lead[i] : 0) + (i < bass.Length ? bass[i] : 0);
            }
            return ToWav(samples);
        }

        private static float[] Voice(string notes, int tempo, bool square, float volume)
        {
            double sixteenth = 60.0 / tempo / 4;
            List<float> output = new List<float>();
            foreach (string token in notes.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string[] parts = token.Split(':');
                int length = parts.Length > 1 ? int.Parse(parts[1]) : 1;
                int count = (int)(SampleRate * sixteenth * length);
                double freq = Frequency(parts[0]);
                for (int i = 0; i < count; i++)
                {
                    if (freq <= 0) { output.Add(0); continue; }
                    double t = i / (double)SampleRate;
                    double phase = (t * freq) % 1.0;
                    double wave = square ? (phase < 0.25 ? 1 : -1) : (phase < 0.5 ? 4 * phase - 1 : 3 - 4 * phase);
                    double env = Math.Min(1, i / (SampleRate * 0.005)) * Math.Min(1, (count - i) / (SampleRate * 0.02));
                    env *= 0.75 + 0.25 * Math.Exp(-t * 6);
                    output.Add((float)(wave * env * volume));
                }
            }
            return output.ToArray();
        }

        // Entwickler: Patrick Lupierz – Notenname (z. B. "C#5") in Hz; "R" = Pause
        public static double Frequency(string note)
        {
            if (string.IsNullOrEmpty(note) || note[0] == 'R') return 0;
            int[] offsets = { 9, 11, 0, 2, 4, 5, 7 };   // A B C D E F G relativ zu C
            int semitone = offsets[note[0] - 'A'];
            int index = 1;
            if (note.Length > 2 && note[1] == '#') { semitone++; index = 2; }
            int octave = int.Parse(note.Substring(index));
            int midi = 12 * (octave + 1) + semitone;
            return 440.0 * Math.Pow(2, (midi - 69) / 12.0);
        }

        private static float[] Noise(double seconds, double volume, bool bark)
        {
            Random random = new Random(7);
            int count = (int)(SampleRate * seconds);
            float[] samples = new float[count];
            double last = 0;
            for (int i = 0; i < count; i++)
            {
                double t = i / (double)SampleRate;
                double raw = random.NextDouble() * 2 - 1;
                last = last * (bark ? 0.85 : 0.5) + raw * (bark ? 0.15 : 0.5);   // einfacher Tiefpass
                double env = Math.Exp(-t * (bark ? 14 : 9));
                double tone = bark ? Math.Sin(2 * Math.PI * (320 - t * 600) * t) * 0.6 : 0;
                samples[i] = (float)((last + tone) * env * volume);
            }
            return samples;
        }

        private static byte[] ToWav(float[] samples)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter w = new BinaryWriter(stream))
            {
                int dataBytes = samples.Length * 2;
                w.Write(new[] { 'R', 'I', 'F', 'F' }); w.Write(36 + dataBytes); w.Write(new[] { 'W', 'A', 'V', 'E' });
                w.Write(new[] { 'f', 'm', 't', ' ' }); w.Write(16); w.Write((short)1); w.Write((short)1);
                w.Write(SampleRate); w.Write(SampleRate * 2); w.Write((short)2); w.Write((short)16);
                w.Write(new[] { 'd', 'a', 't', 'a' }); w.Write(dataBytes);
                foreach (float s in samples) w.Write((short)(Math.Max(-1, Math.Min(1, s)) * 30000));
                w.Flush();
                return stream.ToArray();
            }
        }
    }
}
