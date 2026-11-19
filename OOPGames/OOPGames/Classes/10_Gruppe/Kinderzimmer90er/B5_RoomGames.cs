using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    static class B5RoomDraw
    {
        public static void Rect(Canvas canvas, double x, double y, double width, double height, Color color)
        {
            TowerDefensePixelSurface surface = canvas.Tag as TowerDefensePixelSurface;
            if (surface != null)
            {
                surface.Rect(x, y, width, height, color);
                return;
            }

            Rectangle shape = new Rectangle { Width = width, Height = height, Fill = new SolidColorBrush(color) };
            Canvas.SetLeft(shape, x);
            Canvas.SetTop(shape, y);
            canvas.Children.Add(shape);
        }

        public static void Text(Canvas canvas, string text, double x, double y, int size, Color color)
        {
            TowerDefensePixelSurface surface = canvas.Tag as TowerDefensePixelSurface;
            if (surface != null)
            {
                surface.Text(text, x, y, size, color);
                return;
            }

            TextBlock block = new TextBlock
            {
                Text = text,
                FontFamily = new FontFamily("Consolas"),
                FontSize = size,
                Foreground = new SolidColorBrush(color)
            };
            Canvas.SetLeft(block, x);
            Canvas.SetTop(block, y);
            canvas.Children.Add(block);
        }

        // Consolas is monospaced: every character is 0.55 em wide.
        public static void TextCentered(Canvas canvas, string text, double centerX, double y, int size, Color color)
        {
            Text(canvas, text, centerX - text.Length * size * 0.55 / 2, y, size, color);
        }
    }

    // Directions can be steered with the arrow keys or with WASD.
    static class B5RoomKeys
    {
        public static bool IsLeft(Key key) { return key == Key.Left || key == Key.A; }
        public static bool IsRight(Key key) { return key == Key.Right || key == Key.D; }
        public static bool IsUp(Key key) { return key == Key.Up || key == Key.W; }
        public static bool IsDown(Key key) { return key == Key.Down || key == Key.S; }
    }

    static class B5RoomHighscores
    {
        static readonly string _Path = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Programmieren3WS2026", "Kinderzimmer90er", "highscores.json");
        static readonly Dictionary<string, int> _Scores = Load();

        static Dictionary<string, int> Load()
        {
            if (!File.Exists(_Path)) return new Dictionary<string, int>();
            Dictionary<string, int> scores = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(_Path));
            if (scores == null) throw new InvalidDataException("Die Kinderzimmer-Highscores sind ungueltig.");
            return scores;
        }

        public static int Get(string game)
        {
            int score;
            return _Scores.TryGetValue(game, out score) ? score : 0;
        }

        public static void Save(string game, int score)
        {
            if (score <= Get(game)) return;
            _Scores[game] = score;
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_Path));
            File.WriteAllText(_Path, JsonSerializer.Serialize(_Scores));
        }
    }
}
