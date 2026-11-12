//Prompt 09.11.2026 Dev2_2E
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // Loads a tagged sprite-sheet atlas and plays any named clip from its JSON metadata.
    public sealed class SpriteSheetAnimator
    {
        private readonly Dictionary<string, AnimationClip> _clips =
            new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
        private string _currentClipName;
        private long _clipStartTimestamp;

        // The caller supplies the atlas and Aseprite/LibreSprite JSON paths, so the animator
        // can be reused with any entity's sprite assets and clip names.
        public SpriteSheetAnimator(string spriteSheetPath, string animationMetadataPath)
        {
            if (String.IsNullOrWhiteSpace(spriteSheetPath))
            {
                throw new ArgumentException("A sprite-sheet path is required.", nameof(spriteSheetPath));
            }
            if (String.IsNullOrWhiteSpace(animationMetadataPath))
            {
                throw new ArgumentException("An animation metadata path is required.", nameof(animationMetadataPath));
            }

            BitmapImage sheet = LoadSheet(spriteSheetPath);

            // Frame tags provide clip names and ranges; the caller decides which clip to play.
            using (JsonDocument metadata = JsonDocument.Parse(File.ReadAllText(animationMetadataPath)))
            {
                List<AnimationFrame> frames = ReadFrames(
                    metadata.RootElement.GetProperty("frames"), sheet);
                ReadClips(metadata.RootElement.GetProperty("meta").GetProperty("frameTags"), frames);
            }
        }

        // Returns the current frame for a named clip, restarting the clip when it changes.
        public BitmapSource GetFrame(string animationName)
        {
            if (String.IsNullOrWhiteSpace(animationName))
            {
                throw new ArgumentException("An animation name is required.", nameof(animationName));
            }
            if (!_clips.TryGetValue(animationName, out AnimationClip clip))
            {
                throw new KeyNotFoundException(
                    "The sprite-sheet metadata does not contain an animation named '"
                    + animationName + "'.");
            }

            // A new clip starts at frame zero; continued requests advance its loop.
            if (!String.Equals(_currentClipName, animationName, StringComparison.OrdinalIgnoreCase))
            {
                _currentClipName = animationName;
                _clipStartTimestamp = Stopwatch.GetTimestamp();
            }

            long elapsedMilliseconds = (long)Stopwatch.GetElapsedTime(
                _clipStartTimestamp).TotalMilliseconds;
            int frameTime = (int)(elapsedMilliseconds % clip.DurationMilliseconds);
            foreach (AnimationFrame frame in clip.Frames)
            {
                if (frameTime < frame.DurationMilliseconds)
                {
                    return frame.Image;
                }
                frameTime -= frame.DurationMilliseconds;
            }

            // Positive frame durations and a non-empty clip make this unreachable.
            throw new InvalidDataException("The sprite-sheet animation has no playable frames.");
        }

        private static BitmapImage LoadSheet(string path)
        {
            BitmapImage sheet = new BitmapImage();
            sheet.BeginInit();
            sheet.UriSource = new Uri(path, UriKind.Absolute);
            sheet.CacheOption = BitmapCacheOption.OnLoad;
            sheet.EndInit();
            sheet.Freeze();
            return sheet;
        }

        private static List<AnimationFrame> ReadFrames(
            JsonElement frameMetadata, BitmapImage sheet)
        {
            List<AnimationFrame> frames = new List<AnimationFrame>();

            foreach (JsonProperty property in frameMetadata.EnumerateObject())
            {
                JsonElement frameData = property.Value;
                JsonElement bounds = frameData.GetProperty("frame");
                int x = bounds.GetProperty("x").GetInt32();
                int y = bounds.GetProperty("y").GetInt32();
                int width = bounds.GetProperty("w").GetInt32();
                int height = bounds.GetProperty("h").GetInt32();
                int duration = frameData.GetProperty("duration").GetInt32();

                // Fail clearly for malformed metadata rather than displaying a wrong crop.
                if (frameData.GetProperty("rotated").GetBoolean()
                    || x < 0 || y < 0 || width <= 0 || height <= 0
                    || x > sheet.PixelWidth - width || y > sheet.PixelHeight - height
                    || duration <= 0)
                {
                    throw new InvalidDataException(
                        "The sprite-sheet metadata contains an invalid frame.");
                }

                CroppedBitmap image = new CroppedBitmap(
                    sheet, new System.Windows.Int32Rect(x, y, width, height));
                image.Freeze();
                frames.Add(new AnimationFrame(image, duration));
            }

            return frames;
        }

        private void ReadClips(
            JsonElement tagMetadata, IReadOnlyList<AnimationFrame> frames)
        {
            Dictionary<string, Tuple<int, int>> clipRanges =
                new Dictionary<string, Tuple<int, int>>(StringComparer.OrdinalIgnoreCase);
            foreach (JsonElement tag in tagMetadata.EnumerateArray())
            {
                string name = tag.GetProperty("name").GetString();
                int firstFrame = tag.GetProperty("from").GetInt32();
                int lastFrame = tag.GetProperty("to").GetInt32();
                if (String.IsNullOrWhiteSpace(name) || firstFrame < 0
                    || lastFrame < firstFrame || lastFrame >= frames.Count)
                {
                    throw new InvalidDataException(
                        "The sprite-sheet metadata contains an invalid animation tag.");
                }

                if (clipRanges.TryGetValue(name, out Tuple<int, int> existingRange))
                {
                    if (existingRange.Item1 == firstFrame && existingRange.Item2 == lastFrame)
                    {
                        continue;
                    }

                    throw new InvalidDataException(
                        "The sprite-sheet metadata contains conflicting animation tag names.");
                }

                List<AnimationFrame> clipFrames = new List<AnimationFrame>();
                for (int index = firstFrame; index <= lastFrame; index++)
                {
                    clipFrames.Add(frames[index]);
                }

                _clips.Add(name, new AnimationClip(clipFrames));
                clipRanges.Add(name, Tuple.Create(firstFrame, lastFrame));
            }

            if (_clips.Count == 0)
            {
                throw new InvalidDataException(
                    "The sprite-sheet metadata does not define any animation clips.");
            }
        }

        private sealed class AnimationClip
        {
            public IReadOnlyList<AnimationFrame> Frames { get; }
            public int DurationMilliseconds { get; }

            public AnimationClip(IReadOnlyList<AnimationFrame> frames)
            {
                Frames = frames;
                int duration = 0;
                foreach (AnimationFrame frame in frames)
                {
                    duration = checked(duration + frame.DurationMilliseconds);
                }
                DurationMilliseconds = duration;
            }
        }

        private sealed class AnimationFrame
        {
            public BitmapSource Image { get; }
            public int DurationMilliseconds { get; }

            public AnimationFrame(BitmapSource image, int durationMilliseconds)
            {
                Image = image;
                DurationMilliseconds = durationMilliseconds;
            }
        }
    }
}