using System;
using System.Collections.Generic;
using System.IO;
using System.Media;

namespace OOPGames
{
    // A continuous sound such as the engine loop. Windows' SoundPlayer loops a WAV without
    // the gap MediaPlayer leaves on restart; only one loop can run, which is all a loop needs.
    // One-shot effects use MediaPlayer, so they are not interrupted by this player.
    // SoundPlayer has no volume control, so the volume is applied to the samples in memory.
    public sealed class ArenaLoopingSound
    {
        // Stays null off Windows, where SoundPlayer does not exist.
        private readonly List<SoundPlayer> _variants;
        private readonly Random _random = new Random();

        public ArenaLoopingSound(IEnumerable<string> paths, double volume)
        {
            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }
            if (double.IsNaN(volume) || volume < 0 || volume > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(volume));
            }
            if (!OperatingSystem.IsWindows())
            {
                return;
            }

            _variants = new List<SoundPlayer>();
            foreach (string path in paths)
            {
                try
                {
                    byte[] wave = ScalePcm16Wave(File.ReadAllBytes(path), volume);
                    SoundPlayer player = new SoundPlayer(new MemoryStream(wave, false));
                    player.Load();
                    _variants.Add(player);
                }
                catch (Exception exception) when (exception is InvalidOperationException
                    || exception is IOException || exception is TimeoutException
                    || exception is UnauthorizedAccessException)
                {
                    // An unreadable variant is skipped; the others still loop.
                }
            }
        }

        public bool IsPlaying { get; private set; }

        // Starts a random variant; does nothing while this loop is already running.
        public void Start()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            if (IsPlaying || _variants == null || _variants.Count == 0)
            {
                return;
            }

            try
            {
                _variants[_random.Next(_variants.Count)].PlayLooping();
                IsPlaying = true;
            }
            catch (InvalidOperationException)
            {
                // Missing audio devices must not interrupt the game.
            }
        }

        public void Stop()
        {
            if (!OperatingSystem.IsWindows())
            {
                return;
            }
            if (!IsPlaying || _variants == null)
            {
                return;
            }

            IsPlaying = false;
            foreach (SoundPlayer player in _variants)
            {
                player.Stop();
            }
        }

        // Returns a copy whose 16-bit PCM samples are multiplied by volume (0..1). Other
        // formats are returned unchanged and play at their recorded level.
        public static byte[] ScalePcm16Wave(byte[] wave, double volume)
        {
            if (wave == null)
            {
                throw new ArgumentNullException(nameof(wave));
            }

            byte[] result = (byte[])wave.Clone();
            if (volume == 1 || !TryFindPcm16Data(result, out int dataOffset, out int dataLength))
            {
                return result;
            }

            for (int offset = dataOffset; offset + 1 < dataOffset + dataLength; offset += 2)
            {
                short sample = (short)Math.Round(BitConverter.ToInt16(result, offset) * volume);
                result[offset] = (byte)(sample & 0xFF);
                result[offset + 1] = (byte)((sample >> 8) & 0xFF);
            }
            return result;
        }

        private static bool TryFindPcm16Data(byte[] wave, out int dataOffset, out int dataLength)
        {
            dataOffset = 0;
            dataLength = 0;
            if (wave.Length < 12 || ReadChunkId(wave, 0) != "RIFF" || ReadChunkId(wave, 8) != "WAVE")
            {
                return false;
            }

            bool isPcm16 = false;
            int offset = 12;
            while (offset + 8 <= wave.Length)
            {
                string chunkId = ReadChunkId(wave, offset);
                int chunkLength = BitConverter.ToInt32(wave, offset + 4);
                int chunkData = offset + 8;
                if (chunkLength < 0 || chunkLength > wave.Length - chunkData)
                {
                    return false;
                }

                if (chunkId == "fmt " && chunkLength >= 16)
                {
                    isPcm16 = BitConverter.ToInt16(wave, chunkData) == 1
                        && BitConverter.ToInt16(wave, chunkData + 14) == 16;
                }
                else if (chunkId == "data")
                {
                    dataOffset = chunkData;
                    dataLength = chunkLength;
                    return isPcm16;
                }

                // RIFF chunks are padded to an even length.
                offset = chunkData + chunkLength + (chunkLength & 1);
            }
            return false;
        }

        private static string ReadChunkId(byte[] wave, int offset)
        {
            return System.Text.Encoding.ASCII.GetString(wave, offset, 4);
        }
    }
}
