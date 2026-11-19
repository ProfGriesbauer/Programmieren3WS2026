using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Schnittstellen: IG1C_AudioVoice, IG1C_AudioBackend
    // Entwickler: Patrick Lupierz
    // Erstellt: 19.11.2026
    // Beschreibung: Trennung zwischen Klanglogik (G1C_SoundSystem: was, wann, wie laut, wie viele gleichzeitig) und dem
    //               eigentlichen Abspielen. Das Spiel benutzt WPF-MediaPlayer (G1C_MediaBackend); der Selbsttest ein
    //               Testgerät ohne Ton. So lässt sich die Logik prüfen, ohne dass etwas zu hören sein muss.
    // =====================================================
    public interface IG1C_AudioVoice
    {
        void Stop();
        void SetVolume(double volume);
    }

    public interface IG1C_AudioBackend
    {
        /// <summary>Startet eine Datei. Schleife: am Ende wird keepGoing gefragt, sonst endet sie und ended wird gerufen.</summary>
        IG1C_AudioVoice Start(string file, double volume, bool loop, Func<bool> keepGoing, Action ended);
    }

    // =====================================================
    // Klasse: G1C_MediaBackend
    // Entwickler: Patrick Lupierz
    // Erstellt: 19.11.2026
    // Beschreibung: Spielt Dateien mit WPF-MediaPlayer ab (wav, mp3; ogg kann WPF ohne Zusatzbibliothek nicht).
    //               Muss auf dem UI-Thread benutzt werden – das Spiel ruft es nur aus dem Framework-Tick auf.
    // =====================================================
    public sealed class G1C_MediaBackend : IG1C_AudioBackend
    {
        public IG1C_AudioVoice Start(string file, double volume, bool loop, Func<bool> keepGoing, Action ended)
        {
            MediaVoice voice = new MediaVoice(new MediaPlayer());
            MediaPlayer player = voice.Player;
            player.Open(new Uri(file, UriKind.Absolute));
            player.Volume = Math.Max(0, Math.Min(1, volume));
            player.MediaEnded += (s, e) =>
            {
                if (voice.Stopped) return;
                if (loop && (keepGoing == null || keepGoing()))
                {
                    player.Position = TimeSpan.Zero;
                    player.Play();
                    return;
                }
                voice.Stop();
                if (ended != null) ended();
            };
            player.MediaFailed += (s, e) =>
            {
                if (voice.Stopped) return;
                voice.Stop();
                if (ended != null) ended();
            };
            player.Play();
            return voice;
        }

        private sealed class MediaVoice : IG1C_AudioVoice
        {
            public MediaVoice(MediaPlayer player) { Player = player; }
            public MediaPlayer Player { get; }
            public bool Stopped { get; private set; }

            public void Stop()
            {
                if (Stopped) return;
                Stopped = true;
                try { Player.Stop(); Player.Close(); } catch (Exception) { }
            }

            public void SetVolume(double volume)
            {
                if (!Stopped) Player.Volume = Math.Max(0, Math.Min(1, volume));
            }
        }
    }

    // =====================================================
    // Klasse: G1C_StepSurface
    // Entwickler: Patrick Lupierz
    // Erstellt: 19.11.2026
    // Beschreibung: Untergrund für die Schrittgeräusche. Der Untergrund folgt der Kachelgrafik der Stadt
    //               (Brücke, Weg/Straße/Steinplatten, sonst Gras); in jedem Gebäude ist es der Innenboden.
    // =====================================================
    public static class G1C_StepSurface
    {
        public static string SoundIdFor(string tileSpriteId, G1C_WorldLocation location)
        {
            if (location != G1C_WorldLocation.Exterior) return "step_indoor";
            string id = tileSpriteId ?? "";
            if (id.Contains("bridge")) return "step_bridge";
            if (id.Contains("_road") || id.Contains("_path") || id.Contains("plaza")) return "step_path";
            return "step_grass";
        }

        /// <summary>Liegt ein Tier innerhalb des Radius (Kacheln, Luftlinie)? Für die Vogelstimmen.</summary>
        public static bool InRadius(int dx, int dy, int radius)
        {
            return dx * dx + dy * dy <= radius * radius;
        }
    }

    // =====================================================
    // Klasse: G1C_SoundSystem
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: Hintergrundmusik je Ort und Geräusche (Katalog: G1C_SoundCatalog, Dateien: Assets\Sounds).
    //               Ohne Audiogerät (Backend == null, z. B. im Selbsttest) bleibt alles stumm; die Logik läuft trotzdem.
    //               Regeln: Musik läuft immer nur als ein Stück. Effekte unterliegen drei Grenzen aus den Einstellungen:
    //               Wahrscheinlichkeit (Chance), Mindestabstand (Cooldown) und "höchstens N gleichzeitig" – ist die
    //               Grenze erreicht, verdrängt ein wichtigerer Effekt den unwichtigsten, sonst entfällt der neue.
    //               Derselbe Sound überlagert sich nie selbst (Schritte!). Fehler beim Abspielen schalten den Ton ab.
    // Änderung durch: Patrick Lupierz – Datum: 19.11.2026 – Neufassung: Klangpaket, Mixer-Einstellungen,
    //               Lautstärke-/Häufigkeitsregeln, Schleifen, Backend-Trennung.
    // =====================================================
    public sealed class G1C_SoundSystem
    {
        private sealed class Voice
        {
            public G1C_SoundDef Def;
            public IG1C_AudioVoice Handle;
            public long StartedMs;
            public bool Ended;
        }

        private readonly Dictionary<string, string> _generated = new Dictionary<string, string>();
        private readonly Dictionary<string, long> _lastPlay = new Dictionary<string, long>();
        private readonly Dictionary<string, int> _variant = new Dictionary<string, int>();
        private readonly List<Voice> _voices = new List<Voice>();
        private readonly ConcurrentQueue<Action> _commands = new ConcurrentQueue<Action>();
        private readonly Dictionary<string, long> _stopAt = new Dictionary<string, long>();
        private Voice _music;
        private long _lastHeartbeatMs;
        private long _previewMusicUntilMs;
        private int _savedRevision;
        private int _appliedRevision;
        private long _savedAtMs;

        public G1C_SoundSystem()
        {
            Settings = new G1C_SoundSettings();
            Settings.Sanitize();
            NowMs = () => Environment.TickCount64;
            Random = new Random();
        }

        public G1C_SoundSettings Settings { get; }
        public IG1C_AudioBackend Backend { get; set; }
        public Func<long> NowMs { get; set; }
        public Random Random { get; set; }
        public string PersistPath { get; set; }
        public G1C_SoundMixerServer Mixer { get; set; }
        public bool MixerLoopbackOnly { get; set; } // Tests: Handy-Server nur für diesen Rechner (keine Firewall-Frage)

        public bool Enabled { get { return Backend != null; } }
        public bool Muted { get; private set; }
        public string CurrentMusic { get; private set; }
        public string LastEffect { get; private set; }
        public int EffectCount { get; private set; }
        public string LastError { get; private set; }
        public string CurrentMusicFile { get; private set; }

        /// <summary>Ids der gerade laufenden Effekte (ohne Musik).</summary>
        public IList<string> ActiveEffects
        {
            get { lock (_voices) { return _voices.Select(v => v.Def.Id).ToList(); } }
        }

        /// <summary>Nur das echte Spiel (G1C_CasinoRules) schaltet den Ton ein; Tests bleiben stumm.</summary>
        public void Enable()
        {
            Backend = new G1C_MediaBackend();
            PersistPath = G1C_SoundSettings.DefaultPath;
            Settings.LoadFrom(PersistPath);
            SetMuted(Settings.Muted);
            _savedRevision = Settings.Revision;
            _appliedRevision = Settings.Revision;
        }

        // ------------------------------------------------------------------ Lautstärke

        // Entwickler: Patrick Lupierz – Endlautstärke 0..1 = Gesamt x (Musik|Effekte) x Einzelwert; stumm = 0
        public double VolumeFor(G1C_SoundDef def)
        {
            if (Muted || def == null) return 0;
            G1C_SoundEntrySettings entry = Settings.Get(def.Id);
            double group = def.IsMusic ? Settings.Music : Settings.Effects;
            return Settings.Master / 100.0 * group / 100.0 * entry.Volume / 100.0;
        }

        /// <summary>Wendet geänderte Lautstärken sofort auf laufende Töne an.</summary>
        public void RefreshVolumes()
        {
            try
            {
                lock (_voices)
                {
                    foreach (Voice v in _voices) v.Handle.SetVolume(VolumeFor(v.Def));
                }
                if (_music != null) _music.Handle.SetVolume(VolumeFor(_music.Def));
            }
            catch (Exception ex) { Fail(ex); }
        }

        public string ToggleMute()
        {
            SetMuted(!Muted);
            return Muted ? "Ton aus." : "Ton an.";
        }

        public void SetMuted(bool muted)
        {
            Muted = muted;
            Settings.Muted = muted;
            if (PersistPath != null) Settings.Save(PersistPath);
            RefreshVolumes();
        }

        // ------------------------------------------------------------------ Takt (UI-Thread)

        /// <summary>Befehle vom Handy-Mixer ausführen, laufende Töne anpassen, Einstellungen speichern.
        /// Wird bei jedem Spiel-Tick aufgerufen.</summary>
        public void Pump()
        {
            Action command;
            while (_commands.TryDequeue(out command))
            {
                try { command(); } catch (Exception ex) { LastError = ex.Message; }
            }
            long now = NowMs();
            if (_stopAt.Count > 0)
            {
                foreach (string id in _stopAt.Where(p => now >= p.Value).Select(p => p.Key).ToList())
                {
                    _stopAt.Remove(id);
                    StopLoop(id);
                }
            }
            if (Settings.Revision != _appliedRevision)
            {
                _appliedRevision = Settings.Revision;
                RefreshVolumes();
            }
            if (PersistPath != null && Settings.Revision != _savedRevision && now - _savedAtMs > 1000)
            {
                _savedRevision = Settings.Revision;
                _savedAtMs = now;
                Settings.Save(PersistPath);
            }
        }

        /// <summary>Vom Server-Thread aus: Befehl für den nächsten Spiel-Tick vormerken.</summary>
        public void PostCommand(Action command)
        {
            if (command != null) _commands.Enqueue(command);
        }

        /// <summary>Jeder Spiel-Tick meldet sich hier. Kommt kein Tick mehr (anderes Spiel im Hauptfenster gewählt),
        /// wird die Musik am Ende des Stücks nicht wiederholt – sie läuft also nicht ewig weiter.</summary>
        public void Heartbeat()
        {
            _lastHeartbeatMs = NowMs();
        }

        private bool HeartbeatAlive()
        {
            return NowMs() - _lastHeartbeatMs <= 2000;
        }

        // ------------------------------------------------------------------ Musik

        // Entwickler: Patrick Lupierz – Musik wechseln; gleiche Musik läuft ohne Unterbrechung weiter
        public void SetMusic(string theme)
        {
            Heartbeat();
            if (NowMs() < _previewMusicUntilMs && theme != null) return;   // Vorhören im Mixer läuft noch
            if (theme == CurrentMusic) return;
            CurrentMusic = theme;
            StopMusicVoice();
            if (theme == null || Backend == null) return;
            StartMusic();
        }

        private void StopMusicVoice()
        {
            if (_music != null)
            {
                Voice old = _music;
                _music = null;
                old.Ended = true;
                try { old.Handle.Stop(); } catch (Exception) { }
            }
            CurrentMusicFile = null;
        }

        private void StartMusic()
        {
            G1C_SoundDef def = G1C_SoundCatalog.Find("music_" + CurrentMusic);
            if (def == null || !Settings.Get(def.Id).Enabled) return;
            string[] files = G1C_SoundFiles.For(def);
            string file = Pick(def, files);
            if (file == null) return;
            CurrentMusicFile = file;
            Voice voice = new Voice { Def = def, StartedMs = NowMs() };
            string startedTheme = CurrentMusic;
            try
            {
                // Einzelne Datei: der Player wiederholt sie selbst; mehrere Dateien: nach dem Ende kommt die nächste
                bool single = files.Length <= 1;
                voice.Handle = Backend.Start(file, VolumeFor(def), single, HeartbeatAlive, () =>
                {
                    if (voice.Ended) return;
                    voice.Ended = true;
                    if (_music == voice) _music = null;
                    if (!HeartbeatAlive()) { CurrentMusic = null; return; }
                    if (CurrentMusic == startedTheme && _music == null) StartMusic();
                });
                _music = voice;
            }
            catch (Exception ex) { Fail(ex); }
        }

        // Entwickler: Patrick Lupierz – Datei auswählen: reihum (Schritte) oder zufällig ohne Wiederholung; sonst Chiptune
        private string Pick(G1C_SoundDef def, string[] files)
        {
            if (files.Length == 0)
                return def.Fallback == null ? null : GeneratedFile(def.Fallback);
            int index;
            if (!_variant.TryGetValue(def.Id, out index)) index = -1;
            if (def.Sequential || files.Length == 1) index = (index + 1) % files.Length;
            else index = (index + 1 + Random.Next(files.Length - 1)) % files.Length;   // nie dieselbe Datei zweimal hintereinander
            _variant[def.Id] = index;
            return files[index];
        }

        // ------------------------------------------------------------------ Effekte

        // Entwickler: Patrick Lupierz
        // Aufgabe: Auslöser für einen Sound. "<id>:stop" beendet einen laufenden Schleifen-Sound. Gibt true zurück,
        //          wenn tatsächlich etwas gestartet wurde. LastEffect/EffectCount zählen jede Anfrage (auch bei Stumm).
        public bool Play(string id)
        {
            return Play(id, false);
        }

        public bool Play(string id, bool force)
        {
            if (string.IsNullOrEmpty(id)) return false;
            if (id.EndsWith(":stop", StringComparison.Ordinal))
            {
                StopLoop(id.Substring(0, id.Length - 5));
                return false;
            }
            LastEffect = id;
            EffectCount++;
            G1C_SoundDef def = G1C_SoundCatalog.Find(id);
            if (def == null || Backend == null || Muted) return false;
            if (def.IsMusic) return false;
            G1C_SoundEntrySettings entry = Settings.Get(id);
            long now = NowMs();
            if (!force)
            {
                if (!entry.Enabled) return false;
                if (entry.Chance < 100 && Random.Next(100) >= entry.Chance) return false;
                long last;
                if (entry.CooldownMs > 0 && _lastPlay.TryGetValue(id, out last) && now - last < entry.CooldownMs) return false;
            }
            if (def.Loop) return StartLoop(id, true);
            string file = Pick(def, G1C_SoundFiles.For(def));
            if (file == null) return false;
            if (!MakeRoom(def)) return false;
            return Launch(def, file, false, now);
        }

        /// <summary>Schleifen-Sound starten (tut nichts, wenn er schon läuft).</summary>
        public bool StartLoop(string id)
        {
            return StartLoop(id, false);
        }

        private bool StartLoop(string id, bool counted)
        {
            G1C_SoundDef def = G1C_SoundCatalog.Find(id);
            if (def == null || Backend == null || Muted || !Settings.Get(id).Enabled) return false;
            lock (_voices) { if (_voices.Any(v => v.Def.Id == id)) return true; }
            if (!counted) { LastEffect = id; EffectCount++; }
            string file = Pick(def, G1C_SoundFiles.For(def));
            if (file == null || !MakeRoom(def)) return false;
            return Launch(def, file, true, NowMs());
        }

        public void StopLoop(string id)
        {
            List<Voice> stop;
            lock (_voices) { stop = _voices.Where(v => v.Def.Id == id).ToList(); _voices.RemoveAll(v => v.Def.Id == id); }
            foreach (Voice v in stop) { v.Ended = true; try { v.Handle.Stop(); } catch (Exception) { } }
        }

        public void StopAllEffects()
        {
            List<Voice> stop;
            lock (_voices) { stop = _voices.ToList(); _voices.Clear(); }
            foreach (Voice v in stop) { v.Ended = true; try { v.Handle.Stop(); } catch (Exception) { } }
        }

        /// <summary>Alles anhalten (Musik, Effekte, Handy-Server) – beim Neustart des Spiels.</summary>
        public void StopAll()
        {
            StopAllEffects();
            CurrentMusic = null;
            StopMusicVoice();
            if (Mixer != null) { Mixer.Stop(); Mixer = null; }
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Platz für einen neuen Effekt schaffen. Derselbe Sound überlagert sich nie; ist die Höchstzahl
        //          gleichzeitiger Effekte erreicht, weicht der unwichtigste (niedrigste Priorität, bei Gleichstand der
        //          älteste), sofern er nicht wichtiger ist als der neue – sonst entfällt der neue.
        private bool MakeRoom(G1C_SoundDef def)
        {
            List<Voice> stop = new List<Voice>();
            bool allowed = true;
            lock (_voices)
            {
                foreach (Voice same in _voices.Where(v => v.Def.Id == def.Id).ToList()) { stop.Add(same); _voices.Remove(same); }
                int max = Settings.MaxSimultaneous;
                while (_voices.Count >= max)
                {
                    Voice weakest = _voices.OrderBy(v => v.Def.Priority).ThenBy(v => v.StartedMs).First();
                    if (weakest.Def.Priority > def.Priority) { allowed = false; break; }
                    stop.Add(weakest);
                    _voices.Remove(weakest);
                }
            }
            foreach (Voice v in stop) { v.Ended = true; try { v.Handle.Stop(); } catch (Exception) { } }
            return allowed;
        }

        private bool Launch(G1C_SoundDef def, string file, bool loop, long now)
        {
            Voice voice = new Voice { Def = def, StartedMs = now };
            try
            {
                voice.Handle = Backend.Start(file, VolumeFor(def), loop, () => !voice.Ended, () =>
                {
                    voice.Ended = true;
                    lock (_voices) { _voices.Remove(voice); }
                });
            }
            catch (Exception ex) { Fail(ex); return false; }
            lock (_voices) { _voices.Add(voice); }
            _lastPlay[def.Id] = now;
            return true;
        }

        // ------------------------------------------------------------------ Vorhören (Handy-Mixer)

        /// <summary>Spielt einen Sound zum Probehören, ohne Wahrscheinlichkeit und Mindestabstand. Musik läuft 10 s,
        /// Schleifen 3 s. Nur auf dem UI-Thread aufrufen (der Mixer benutzt PostCommand).</summary>
        public void Preview(string id)
        {
            G1C_SoundDef def = G1C_SoundCatalog.Find(id);
            if (def == null || Backend == null) return;
            long now = NowMs();
            if (def.IsMusic)
            {
                _previewMusicUntilMs = 0;
                CurrentMusic = null;
                SetMusic(id.Substring("music_".Length));
                _previewMusicUntilMs = now + 10000;
                return;
            }
            Play(id, true);
            if (def.Loop) _stopAt[id] = now + 3000;
        }

        // ------------------------------------------------------------------ Hilfen

        private void Fail(Exception ex)
        {
            LastError = ex.Message;
            Backend = null; // ohne Audiogerät o. Ä. läuft das Spiel stumm weiter
        }

        private string GeneratedFile(string name)
        {
            string file;
            if (_generated.TryGetValue(name, out file)) return file;
            string folder = Path.Combine(Path.GetTempPath(), "G1Casino_Sounds");
            Directory.CreateDirectory(folder);
            file = Path.Combine(folder, name + ".wav");
            File.WriteAllBytes(file, G1C_ChipTune.Render(name));
            _generated[name] = file;
            return file;
        }

        // Kompatibilität (Tests und ältere Aufrufer): Musikordner je Thema
        public static string FolderFor(string theme)
        {
            G1C_SoundDef def = G1C_SoundCatalog.Find("music_" + theme);
            return def != null && def.Folder != null ? def.Folder : "Musik_Gebaeude";
        }

        /// <summary>Sucht eine eigene Datei; null, wenn keine vorhanden ist (dann greift die eingebaute Version).</summary>
        public static string Resolve(string folder, string name)
        {
            try
            {
                foreach (string root in G1C_SoundFiles.Roots())
                    foreach (string ext in G1C_SoundFiles.Extensions)
                    {
                        string file = Path.Combine(root, folder.Replace('/', Path.DirectorySeparatorChar), name + ext);
                        if (File.Exists(file)) return file;
                    }
            }
            catch (Exception) { }
            return null;
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
