using System;
using System.Windows.Media;

namespace OOPGames
{
    // Anything that can be triggered as a sound; lets sound groups be tested without audio.
    public interface IArenaSoundEffect
    {
        void Play();
    }

    // A sound file with several preloaded WPF MediaPlayer voices. Each Play uses the next
    // voice, so the same sound and different sounds can overlap instead of cutting each other off.
    public sealed class ArenaSoundEffect : IArenaSoundEffect
    {
        private readonly MediaPlayer[] _voices;
        private int _nextVoice;

        // Must be created on the UI thread because MediaPlayer belongs to its dispatcher.
        public ArenaSoundEffect(string path, double volume, int voiceCount)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("A sound path is required.", nameof(path));
            }
            if (double.IsNaN(volume) || volume < 0 || volume > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(volume));
            }
            if (voiceCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(voiceCount));
            }

            Uri source = new Uri(path, UriKind.Absolute);
            _voices = new MediaPlayer[voiceCount];
            for (int index = 0; index < voiceCount; index++)
            {
                // Opening ahead of time keeps the delay between hit and sound small.
                MediaPlayer voice = new MediaPlayer { Volume = volume };
                voice.Open(source);
                _voices[index] = voice;
            }
        }

        public void Play()
        {
            MediaPlayer voice = _voices[_nextVoice];
            _nextVoice = (_nextVoice + 1) % _voices.Length;
            voice.Stop();
            voice.Play();
        }
    }
}
