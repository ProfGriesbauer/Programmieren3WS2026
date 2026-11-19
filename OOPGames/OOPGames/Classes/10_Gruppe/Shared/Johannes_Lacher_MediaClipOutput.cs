using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace OOPGames
{
    // Plays generated clips with WPF MediaPlayer (one player per clip, so sounds can overlap).
    // MediaPlayer needs files, so the clips are written once to the local app data folder.
    // If that is not possible, the game simply stays silent.
    // Opening is asynchronous and the very first open in a process can take seconds; a clip that
    // is not open yet is skipped instead of being played late.
    // The volume follows the group's master volume (G10_SoundVolume), also while a clip is playing.
    internal sealed class G10_MediaClipOutput : IG10_ClipOutput
    {
        readonly string _Folder;
        readonly Dictionary<string, MediaPlayer> _Players = new Dictionary<string, MediaPlayer>();
        readonly HashSet<string> _Opened = new HashSet<string>();

        // folderName: sub folder of the Kinderzimmer data folder, one per game.
        public G10_MediaClipOutput(string folderName, IEnumerable<G10_SoundClip> clips)
        {
            if (clips == null) throw new ArgumentNullException(nameof(clips));
            _Folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programmieren3WS2026", "Kinderzimmer90er", folderName);
            try
            {
                Directory.CreateDirectory(_Folder);
                foreach (G10_SoundClip clip in clips)
                {
                    string name = clip.Name;
                    MediaPlayer player = new MediaPlayer { Volume = G10_SoundVolume.Volume };
                    player.MediaOpened += (sender, e) => _Opened.Add(name);
                    player.Open(new Uri(WriteClip(clip)));
                    _Players[name] = player;
                }
            }
            catch (IOException)
            {
                _Players.Clear();
            }
            catch (UnauthorizedAccessException)
            {
                _Players.Clear();
            }
            G10_SoundVolume.Changed += ApplyVolume;
        }

        void ApplyVolume()
        {
            foreach (MediaPlayer player in _Players.Values) player.Volume = G10_SoundVolume.Volume;
        }

        public bool IsReady(G10_SoundClip clip)
        {
            return _Opened.Contains(clip.Name);
        }

        public void Play(G10_SoundClip clip)
        {
            MediaPlayer player;
            if (!IsReady(clip) || !_Players.TryGetValue(clip.Name, out player)) return;
            player.Position = TimeSpan.Zero;
            player.Play();
        }

        public void Stop(G10_SoundClip clip)
        {
            MediaPlayer player;
            if (_Players.TryGetValue(clip.Name, out player)) player.Stop();
        }

        public void StopAll()
        {
            foreach (MediaPlayer player in _Players.Values) player.Stop();
        }

        // The file name contains a hash of the content: a changed sound gets a new file, and a file
        // that another player still has open is never overwritten.
        string WriteClip(G10_SoundClip clip)
        {
            byte[] wav = clip.ToWav();
            uint hash = 2166136261;
            foreach (byte b in wav) hash = (hash ^ b) * 16777619;
            string path = Path.Combine(_Folder, clip.Name + "_" + hash.ToString("x8") + ".wav");
            if (!File.Exists(path)) File.WriteAllBytes(path, wav);
            return path;
        }
    }
}
