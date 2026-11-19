using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Media;

namespace OOPGames
{
    // Decides what is heard: one-shot sounds for the cues of a tick, plus one background loop that
    // follows the game state (siren, frightened ghosts, eyes returning home). Loops are restarted
    // by the ticks themselves, so they fade out on their own when the game is no longer updated
    // (e.g. after Esc), without a timer of our own.
    public sealed class G10_PacManAudio
    {
        // The death sound starts when the death animation starts (see the renderer).
        const int DeathSoundDelayTicks = 15;
        const int NoTick = -1;

        readonly IG10_SoundOutput _Output;
        int _Tick;
        bool _HasLoop;
        G10_Sound _Loop;
        int _LoopEndsAt;
        int _ChompEndsAt;
        bool _NextChompIsB;
        int _DeathAt = NoTick;

        public G10_PacManAudio(IG10_SoundOutput output)
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
            _HasLoop = false;
            _DeathAt = NoTick;
        }

        public void Update(IG10_PacManView view)
        {
            _Tick++;
            if (Muted) return;
            foreach (G10_SoundCue cue in view.SoundCues) PlayCue(cue);
            if (_Tick == _DeathAt) _Output.Play(G10_Sound.Death);
            UpdateLoop(view);
        }

        void PlayCue(G10_SoundCue cue)
        {
            switch (cue)
            {
                case G10_SoundCue.GameStart:
                    _Output.Play(G10_Sound.StartJingle);
                    break;
                case G10_SoundCue.Chomp:
                    // While Pac-Man keeps eating, the two halves of the chomp alternate without overlapping.
                    if (_Tick < _ChompEndsAt) break;
                    G10_Sound chomp = _NextChompIsB ? G10_Sound.ChompB : G10_Sound.ChompA;
                    _NextChompIsB = !_NextChompIsB;
                    _Output.Play(chomp);
                    _ChompEndsAt = _Tick + G10_SoundSynth.Clip(chomp).DurationTicks;
                    break;
                case G10_SoundCue.GhostEaten:
                    _Output.Play(G10_Sound.GhostEaten);
                    break;
                case G10_SoundCue.FruitEaten:
                    _Output.Play(G10_Sound.FruitEaten);
                    break;
                case G10_SoundCue.ExtraLife:
                    _Output.Play(G10_Sound.ExtraLife);
                    break;
                case G10_SoundCue.Death:
                    _DeathAt = _Tick + DeathSoundDelayTicks;
                    break;
            }
        }

        void UpdateLoop(IG10_PacManView view)
        {
            G10_Sound wanted;
            bool hasWanted = WantedLoop(view, out wanted);
            if (_HasLoop && (!hasWanted || wanted != _Loop))
            {
                _Output.Stop(_Loop);
                _HasLoop = false;
            }
            if (!hasWanted) return;
            if (_HasLoop && _Tick < _LoopEndsAt) return;
            _Output.Play(wanted);
            _Loop = wanted;
            _HasLoop = true;
            _LoopEndsAt = _Tick + G10_SoundSynth.Clip(wanted).DurationTicks;
        }

        static bool WantedLoop(IG10_PacManView view, out G10_Sound loop)
        {
            loop = G10_Sound.Siren;
            if (view.Phase != G10_PacManPhaseKind.Playing) return false;
            bool frightened = false;
            foreach (IG10_GhostView ghost in view.Ghosts)
            {
                if (ghost.Mode == G10_GhostModeKind.Eaten && ghost.Visible)
                {
                    loop = G10_Sound.Retreat;
                    return true;
                }
                if (ghost.Mode == G10_GhostModeKind.Frightened) frightened = true;
            }
            if (frightened) loop = G10_Sound.Frightened;
            return true;
        }
    }

    // Plays the generated clips with WPF MediaPlayer (one player per sound, so sounds can overlap).
    // MediaPlayer needs files, so the clips are written once to the local app data folder.
    // If that is not possible, the game simply stays silent.
    // Opening is asynchronous and the very first open in a process can take seconds; a sound that
    // is not open yet is skipped instead of being played late.
    internal sealed class G10_MediaSoundOutput : IG10_SoundOutput
    {
        const double Volume = 0.6;

        static readonly string Folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programmieren3WS2026", "Kinderzimmer90er", "PacManSounds");

        readonly Dictionary<G10_Sound, MediaPlayer> _Players = new Dictionary<G10_Sound, MediaPlayer>();
        readonly HashSet<G10_Sound> _Opened = new HashSet<G10_Sound>();

        public G10_MediaSoundOutput()
        {
            try
            {
                Directory.CreateDirectory(Folder);
                foreach (G10_Sound sound in Enum.GetValues(typeof(G10_Sound)))
                {
                    MediaPlayer player = new MediaPlayer { Volume = Volume };
                    player.MediaOpened += (sender, e) => _Opened.Add(sound);
                    player.Open(new Uri(WriteClip(G10_SoundSynth.Clip(sound))));
                    _Players[sound] = player;
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
        }

        public void Play(G10_Sound sound)
        {
            MediaPlayer player;
            if (!_Opened.Contains(sound) || !_Players.TryGetValue(sound, out player)) return;
            player.Position = TimeSpan.Zero;
            player.Play();
        }

        public void Stop(G10_Sound sound)
        {
            MediaPlayer player;
            if (_Players.TryGetValue(sound, out player)) player.Stop();
        }

        public void StopAll()
        {
            foreach (MediaPlayer player in _Players.Values) player.Stop();
        }

        // The file name contains a hash of the content: a changed sound gets a new file, and a file
        // that another player still has open is never overwritten.
        static string WriteClip(G10_SoundClip clip)
        {
            byte[] wav = clip.ToWav();
            uint hash = 2166136261;
            foreach (byte b in wav) hash = (hash ^ b) * 16777619;
            string path = Path.Combine(Folder, "pacman_" + clip.Sound + "_" + hash.ToString("x8") + ".wav");
            if (!File.Exists(path)) File.WriteAllBytes(path, wav);
            return path;
        }
    }
}
