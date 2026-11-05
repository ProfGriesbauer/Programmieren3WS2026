using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
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
            Rectangle shape = new Rectangle { Width = width, Height = height, Fill = new SolidColorBrush(color) };
            Canvas.SetLeft(shape, x);
            Canvas.SetTop(shape, y);
            canvas.Children.Add(shape);
        }

        public static void Text(Canvas canvas, string text, double x, double y, int size, Color color)
        {
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

    public sealed class B5RoomCoinGame : IB5RoomMiniGame
    {
        readonly string _Title;
        readonly Color _Background;
        readonly Color _Accent;
        readonly Random _Random = new Random(17);
        int _X = 8;
        int _Y = 7;
        int _CoinX = 14;
        int _CoinY = 5;
        int _Score;
        bool _Exit;

        public B5RoomCoinGame(string title, Color background, Color accent)
        {
            _Title = title;
            _Background = background;
            _Accent = accent;
        }

        public string Title { get { return _Title; } }
        public int Width { get { return 160; } }
        public int Height { get { return 120; } }
        public bool ExitRequested { get { return _Exit; } }
        public void Start() { _X = 8; _Y = 7; _Score = 0; _Exit = false; }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            foreach (Key key in keys)
            {
                if (key == Key.Escape || key == Key.X) { _Exit = true; continue; }
                if (key == Key.Left) _X = Math.Max(0, _X - 1);
                if (key == Key.Right) _X = Math.Min(19, _X + 1);
                if (key == Key.Up) _Y = Math.Max(0, _Y - 1);
                if (key == Key.Down) _Y = Math.Min(11, _Y + 1);
            }

            if (_X == _CoinX && _Y == _CoinY)
            {
                _Score++;
                _CoinX = _Random.Next(20);
                _CoinY = _Random.Next(12);
                B5RoomHighscores.Save(_Title, _Score);
            }
        }

        public void Draw(Canvas canvas)
        {
            canvas.Children.Clear();
            B5RoomDraw.Rect(canvas, 0, 0, Width, Height, _Background);
            B5RoomDraw.Text(canvas, "SCORE " + _Score + "  BEST " + B5RoomHighscores.Get(_Title), 4, 3, 10, Colors.White);
            B5RoomDraw.Rect(canvas, _CoinX * 8 + 1, _CoinY * 7 + 22, 5, 5, Color.FromRgb(255, 220, 68));
            B5RoomDraw.Rect(canvas, _X * 8 + 1, _Y * 7 + 22, 6, 6, _Accent);
            B5RoomDraw.Text(canvas, "ARROWS MOVE", 4, 105, 8, Colors.White);
        }
    }

    public sealed class B5RoomSnakeGame : IB5RoomMiniGame
    {
        readonly List<Point> _Snake = new List<Point>();
        readonly Random _Random = new Random(9);
        int _Direction = 1;
        int _Ticks;
        int _Score;
        Point _Food;
        bool _Exit;

        public string Title { get { return "Snake"; } }
        public int Width { get { return 160; } }
        public int Height { get { return 144; } }
        public bool ExitRequested { get { return _Exit; } }

        public void Start()
        {
            _Snake.Clear();
            _Snake.Add(new Point(8, 8));
            _Snake.Add(new Point(7, 8));
            _Snake.Add(new Point(6, 8));
            _Direction = 1;
            _Ticks = 0;
            _Score = 0;
            _Exit = false;
            PlaceFood();
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            foreach (Key key in keys)
            {
                if (key == Key.Escape || key == Key.X) _Exit = true;
                if (key == Key.Up && _Direction != 2) _Direction = 0;
                if (key == Key.Right && _Direction != 3) _Direction = 1;
                if (key == Key.Down && _Direction != 0) _Direction = 2;
                if (key == Key.Left && _Direction != 1) _Direction = 3;
            }

            if (++_Ticks % 5 != 0 || _Exit) return;
            Point head = _Snake[0];
            if (_Direction == 0) head.Y--;
            if (_Direction == 1) head.X++;
            if (_Direction == 2) head.Y++;
            if (_Direction == 3) head.X--;

            if (head.X < 0 || head.X >= 20 || head.Y < 0 || head.Y >= 16 || _Snake.Contains(head))
            {
                _Score = 0;
                _Snake.Clear();
                _Snake.Add(new Point(8, 8));
                _Snake.Add(new Point(7, 8));
                _Snake.Add(new Point(6, 8));
                return;
            }

            _Snake.Insert(0, head);
            if (head == _Food)
            {
                _Score++;
                B5RoomHighscores.Save(Title, _Score);
                PlaceFood();
            }
            else _Snake.RemoveAt(_Snake.Count - 1);
        }

        void PlaceFood()
        {
            do { _Food = new Point(_Random.Next(20), _Random.Next(16)); }
            while (_Snake.Contains(_Food));
        }

        public void Draw(Canvas canvas)
        {
            canvas.Children.Clear();
            B5RoomDraw.Rect(canvas, 0, 0, Width, Height, Color.FromRgb(184, 204, 121));
            B5RoomDraw.Text(canvas, "SCORE " + _Score + "  BEST " + B5RoomHighscores.Get(Title), 4, 3, 10, Color.FromRgb(40, 59, 29));
            B5RoomDraw.Rect(canvas, _Food.X * 8, _Food.Y * 8 + 16, 7, 7, Color.FromRgb(44, 71, 37));
            foreach (Point segment in _Snake)
                B5RoomDraw.Rect(canvas, segment.X * 8, segment.Y * 8 + 16, 7, 7, Color.FromRgb(54, 82, 40));
        }
    }

    public sealed class B5RoomPongGame : IB5RoomMiniGame
    {
        double _BallX;
        double _BallY;
        double _VelocityX;
        double _VelocityY;
        double _PaddleY;
        int _Score;
        bool _Exit;

        public string Title { get { return "Pong"; } }
        public int Width { get { return 160; } }
        public int Height { get { return 144; } }
        public bool ExitRequested { get { return _Exit; } }

        public void Start()
        {
            _BallX = 80;
            _BallY = 72;
            _VelocityX = 2.2;
            _VelocityY = 1.4;
            _PaddleY = 65;
            _Score = 0;
            _Exit = false;
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            foreach (Key key in keys)
            {
                if (key == Key.Escape || key == Key.X) _Exit = true;
                if (key == Key.Up) _PaddleY = Math.Max(18, _PaddleY - 8);
                if (key == Key.Down) _PaddleY = Math.Min(118, _PaddleY + 8);
            }

            if (_Exit) return;
            _BallX += _VelocityX;
            _BallY += _VelocityY;
            if (_BallY < 18 || _BallY > 136) _VelocityY = -_VelocityY;
            if (_VelocityX < 0 && _BallX < 14 && _BallY >= _PaddleY && _BallY <= _PaddleY + 27)
            {
                _VelocityX = Math.Abs(_VelocityX) + 0.1;
                _Score++;
                B5RoomHighscores.Save(Title, _Score);
            }
            else if (_BallX < 4) ResetBall(1);

            if (_VelocityX > 0 && _BallX > 146 && Math.Abs(_BallY - 72) < 20) _VelocityX = -_VelocityX;
            else if (_BallX > 158) ResetBall(-1);
        }

        void ResetBall(int direction)
        {
            _BallX = 80;
            _BallY = 72;
            _VelocityX = direction * 2.2;
            _VelocityY = 1.4;
        }

        public void Draw(Canvas canvas)
        {
            canvas.Children.Clear();
            B5RoomDraw.Rect(canvas, 0, 0, Width, Height, Color.FromRgb(25, 39, 43));
            B5RoomDraw.Text(canvas, "PONG   SCORE " + _Score, 6, 3, 10, Color.FromRgb(203, 222, 147));
            for (int y = 18; y < 144; y += 14) B5RoomDraw.Rect(canvas, 79, y, 2, 7, Color.FromRgb(110, 125, 95));
            B5RoomDraw.Rect(canvas, 7, _PaddleY, 5, 27, Color.FromRgb(203, 222, 147));
            B5RoomDraw.Rect(canvas, 148, 58, 5, 27, Color.FromRgb(142, 165, 99));
            B5RoomDraw.Rect(canvas, _BallX, _BallY, 5, 5, Colors.White);
        }
    }
}
