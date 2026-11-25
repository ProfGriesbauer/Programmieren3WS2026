using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;

namespace OOPGames
{
    // Background music of Tower Defense. The game calls Update every tick with the track it wants;
    // a different track starts at once (hard cut, like old consoles) and loops without end.
    //
    // - The songs are rendered once in the background and written as WAV files to the local app data
    //   folder (MediaPlayer needs files). Until a song is ready, it simply stays silent.
    // - Volume = master volume (G10_SoundVolume) x MusicLevel; menus and dialogs dim it further.
    // - If the game stops calling Update (device left, game closed), the music pauses by itself after
    //   a moment and continues when the game runs again.
    public static class G10_TowerDefenseMusic
    {
        // Quiet background music: share of the master volume.
        const double MusicLevel = 0.25;
        // Extra factor while the Esc menu, help or a dialog is open.
        const double DimFactor = 0.45;
        const int SilenceMilliseconds = 400;

        static readonly Dictionary<G10_TowerDefenseTrack, string> _Files = new Dictionary<G10_TowerDefenseTrack, string>();
        static readonly Dictionary<G10_TowerDefenseTrack, MediaPlayer> _Players = new Dictionary<G10_TowerDefenseTrack, MediaPlayer>();
        static G10_TowerDefenseTrack _Playing = G10_TowerDefenseTrack.None;
        static bool _Rendering, _Suspended, _Dimmed;
        static int _LastUpdate;
        static DispatcherTimer _Watchdog;

        // Called by the game every tick on the UI thread.
        public static void Update(G10_TowerDefenseTrack wanted, bool dimmed)
        {
            _LastUpdate = Environment.TickCount;
            _Dimmed = dimmed;
            if (!_Rendering) StartRendering();

            if (wanted != _Playing)
            {
                Stop();
                MediaPlayer player = PlayerFor(wanted);
                if (player == null) return;
                player.Position = TimeSpan.Zero;
                player.Play();
                _Playing = wanted;
            }
            else if (_Suspended && _Playing != G10_TowerDefenseTrack.None)
            {
                _Players[_Playing].Play();
            }
            _Suspended = false;
            ApplyVolume();
        }

        public static void Stop()
        {
            if (_Playing != G10_TowerDefenseTrack.None) _Players[_Playing].Stop();
            _Playing = G10_TowerDefenseTrack.None;
            _Suspended = false;
        }

        static MediaPlayer PlayerFor(G10_TowerDefenseTrack track)
        {
            MediaPlayer player;
            if (_Players.TryGetValue(track, out player)) return player;
            string file;
            lock (_Files)
                if (!_Files.TryGetValue(track, out file)) return null;
            player = new MediaPlayer();
            // Loop: start again from the beginning when the end is reached.
            player.MediaEnded += (sender, e) =>
            {
                player.Position = TimeSpan.Zero;
                player.Play();
            };
            player.Open(new Uri(file));
            _Players[track] = player;
            return player;
        }

        static void ApplyVolume()
        {
            if (_Playing == G10_TowerDefenseTrack.None) return;
            _Players[_Playing].Volume = G10_SoundVolume.Volume * MusicLevel * (_Dimmed ? DimFactor : 1);
        }

        // Renders all songs on a worker thread; the first tick of the game starts it.
        static void StartRendering()
        {
            _Rendering = true;
            _Watchdog = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _Watchdog.Tick += (sender, e) => CheckSilence();
            _Watchdog.Start();

            string folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programmieren3WS2026", "Kinderzimmer90er", "TowerDefenseMusic");
            Task.Run(() =>
            {
                try
                {
                    Directory.CreateDirectory(folder);
                    foreach (G10_TowerDefenseTrack track in Enum.GetValues(typeof(G10_TowerDefenseTrack)))
                    {
                        G10_TowerDefenseSong song = G10_TowerDefenseSongs.Create(track);
                        if (song == null) continue;
                        string path = WriteSong(folder, song);
                        lock (_Files) _Files[track] = path;
                    }
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            });
        }

        // The file name contains a hash of the content: a changed song gets a new file, and a file that
        // a player still has open is never overwritten.
        static string WriteSong(string folder, G10_TowerDefenseSong song)
        {
            byte[] wav = new G10_SoundClip(song.Name, song.Render()).ToWav();
            uint hash = 2166136261;
            foreach (byte b in wav) hash = (hash ^ b) * 16777619;
            string path = Path.Combine(folder, song.Name + "_" + hash.ToString("x8") + ".wav");
            if (!File.Exists(path)) File.WriteAllBytes(path, wav);
            return path;
        }

        // Pauses the music when the game has not been updated for a while (it is no longer on screen).
        static void CheckSilence()
        {
            if (_Suspended || _Playing == G10_TowerDefenseTrack.None) return;
            if (unchecked(Environment.TickCount - _LastUpdate) < SilenceMilliseconds) return;
            _Players[_Playing].Pause();
            _Suspended = true;
        }
    }
}
