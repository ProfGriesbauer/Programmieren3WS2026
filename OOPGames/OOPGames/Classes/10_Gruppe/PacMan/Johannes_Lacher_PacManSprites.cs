using System;
using System.Collections.Generic;

namespace OOPGames
{
    // All Pac-Man graphics, drawn by us as pixel patterns. Sprites are created once and cached.
    internal static class G10_PacManSprites
    {
        // Actor sprites (Pac-Man, ghosts, fruit) are ActorSize x ActorSize and drawn centred on the actor.
        public const int ActorSize = 16;
        public const int PacManFrameCount = 4;
        public const int GhostFrameCount = 2;

        private const int _DeathFrames = 11;
        private const int _DirectionCount = 5;
        private const int _GhostKindCount = 4;
        private const int _FruitKindCount = 7;
        private const double _DiscCentre = 7.5;
        private const double _PacManRadiusSquared = 6.9 * 6.9;

        // Half opening angle of the mouth in degrees: wide open, half open, closed, half open.
        private static readonly double[] _ChompAngles = { 42.0, 22.0, 0.0, 22.0 };
        private static readonly double[] _DeathAngles = { 12.0, 28.0, 44.0, 60.0, 76.0, 92.0, 110.0, 128.0, 146.0, 164.0 };
        // Widths of the 14 rows (y = 2..15) of a round fruit.
        private static readonly int[] _DiscWidths = { 6, 10, 12, 14, 14, 14, 14, 14, 14, 14, 14, 12, 10, 6 };

        private static readonly Dictionary<char, uint> _PacManPalette = new Dictionary<char, uint>
        {
            { 'Y', 0xFFFFFF00 }
        };

        private static readonly uint[] _GhostColors = { 0xFFFF0000, 0xFFFFB8FF, 0xFF00FFFF, 0xFFFFB852 };

        private static readonly Dictionary<char, uint> _FrightenedPalette = new Dictionary<char, uint>
        {
            { 'B', 0xFF2121FF }, { 'F', 0xFFFFB8AE }
        };

        private static readonly Dictionary<char, uint> _FlashPalette = new Dictionary<char, uint>
        {
            { 'B', 0xFFDEDEFF }, { 'F', 0xFFFF0000 }
        };

        private static readonly Dictionary<char, uint> _FruitPalette = new Dictionary<char, uint>
        {
            { 'R', 0xFFFF0000 }, { 'H', 0xFFFFFFFF }, { 'S', 0xFFDE9751 }, { 'G', 0xFF00C800 },
            { 'Y', 0xFFFFFF00 }, { 'O', 0xFFFF9800 }, { 'L', 0xFFFFD070 }, { 'M', 0xFF00A800 },
            { 'N', 0xFFB8FFB8 }, { 'B', 0xFF00FFFF }, { 'W', 0xFFDEDEFF }, { 'C', 0xFFDEDEDE },
            { 'D', 0xFF9C0000 }
        };

        private static readonly Dictionary<char, uint> _CyanPointsPalette = new Dictionary<char, uint>
        {
            { '#', 0xFF00FFFF }
        };

        private static readonly Dictionary<char, uint> _PinkPointsPalette = new Dictionary<char, uint>
        {
            { '#', 0xFFFFB8FF }
        };

        // 3 x 5 digits for the score numbers.
        private static readonly string[][] _Digits =
        {
            new string[] { "###", "#.#", "#.#", "#.#", "###" },
            new string[] { ".#.", "##.", ".#.", ".#.", "###" },
            new string[] { "###", "..#", "###", "#..", "###" },
            new string[] { "###", "..#", "###", "..#", "###" },
            new string[] { "#.#", "#.#", "###", "..#", "..#" },
            new string[] { "###", "#..", "###", "..#", "###" },
            new string[] { "###", "#..", "###", "#.#", "###" },
            new string[] { "###", "..#", ".#.", ".#.", ".#." },
            new string[] { "###", "#.#", "###", "#.#", "###" },
            new string[] { "###", "#.#", "###", "..#", "###" }
        };

        private static readonly string[] _CherryRows =
        {
            "...........SS...",
            "..........S..S..",
            ".........S...S..",
            "........S....S..",
            ".......S.....S..",
            "......S......S..",
            ".....S.......S..",
            "....S.......S...",
            "...RRR......S...",
            "..RRRRR...RRR...",
            ".RHRRRRR.RRRRR..",
            ".RRRRRRRRRHRRRR.",
            ".RRRRRRRRRRRRRR.",
            "..RRRRR.RRRRRRR.",
            "...RRR...RRRRR..",
            "..........RRR..."
        };

        private static readonly string[] _KeyRows =
        {
            "................",
            ".....BBBBBB.....",
            "....BBBBBBBB....",
            "....BB....BB....",
            "....BB....BB....",
            "....BBBBBBBB....",
            ".....BBBBBB.....",
            ".......WW.......",
            ".......WW.......",
            ".......WW.......",
            ".......WWWW.....",
            ".......WW.......",
            ".......WWWW.....",
            ".......WW.......",
            ".......WW.......",
            "................"
        };

        private static readonly G10_Sprite[][] _PacMan = new G10_Sprite[_DirectionCount][];
        private static readonly G10_Sprite[] _Death = new G10_Sprite[_DeathFrames];
        private static readonly G10_Sprite[][][] _Ghosts = new G10_Sprite[_GhostKindCount][][];
        private static readonly G10_Sprite[][] _Frightened = new G10_Sprite[2][];
        private static readonly G10_Sprite[] _Eyes = new G10_Sprite[_DirectionCount];
        private static readonly G10_Sprite[] _Fruits = new G10_Sprite[_FruitKindCount];
        private static readonly Dictionary<int, G10_Sprite> _Points = new Dictionary<int, G10_Sprite>();

        static G10_PacManSprites()
        {
            BuildPacMan();
            BuildDeath();
            BuildGhosts();
            BuildFruits();
            int[] known = { 100, 200, 300, 400, 500, 700, 800, 1000, 1600, 2000, 3000, 5000 };
            for (int i = 0; i < known.Length; i++)
            {
                _Points[known[i]] = BuildPoints(known[i]);
            }
        }

        // frame: 0..PacManFrameCount-1, one full chomp cycle (e.g. wide open, half open, closed, half open).
        // Direction.None draws the closed (round) Pac-Man.
        public static G10_Sprite PacMan(G10_Direction direction, int frame)
        {
            return _PacMan[(int)direction][WrapFrame(frame, PacManFrameCount)];
        }

        public static int PacManDeathFrameCount
        {
            get
            {
                return _DeathFrames;
            }
        }

        // Death animation: the mouth opens until Pac-Man disappears; the last frame may be a small "pop".
        public static G10_Sprite PacManDeath(int frame)
        {
            if (frame < 0) frame = 0;
            if (frame >= _DeathFrames) frame = _DeathFrames - 1;
            return _Death[frame];
        }

        // Ghost body in its colour with eyes looking in the given direction. frame: 0..GhostFrameCount-1 (wavy skirt).
        public static G10_Sprite Ghost(G10_GhostKind kind, G10_Direction direction, int frame)
        {
            return _Ghosts[(int)kind][(int)direction][WrapFrame(frame, GhostFrameCount)];
        }

        // Frightened ghost: dark blue with a pale face, or white with a red face when flashWhite.
        public static G10_Sprite FrightenedGhost(int frame, bool flashWhite)
        {
            return _Frightened[flashWhite ? 1 : 0][WrapFrame(frame, GhostFrameCount)];
        }

        // Only the eyes, for a ghost that was eaten and returns to the house.
        public static G10_Sprite GhostEyes(G10_Direction direction)
        {
            return _Eyes[(int)direction];
        }

        public static G10_Sprite Fruit(G10_FruitKind kind)
        {
            return _Fruits[(int)kind];
        }

        // Small score numbers shown after eating a ghost or a fruit
        // (100, 200, 300, 400, 500, 700, 800, 1000, 1600, 2000, 3000, 5000).
        public static G10_Sprite Points(int points)
        {
            G10_Sprite sprite;
            if (!_Points.TryGetValue(points, out sprite))
            {
                sprite = BuildPoints(points);
                _Points[points] = sprite;
            }
            return sprite;
        }

        private static int WrapFrame(int frame, int count)
        {
            return ((frame % count) + count) % count;
        }

        // ---- Pac-Man ----

        private static void BuildPacMan()
        {
            // One set of base patterns facing right; the other directions are rotated copies.
            string[][] baseRows = new string[PacManFrameCount][];
            for (int f = 0; f < PacManFrameCount; f++)
            {
                baseRows[f] = BuildPacManRows(_ChompAngles[f]);
            }
            G10_Direction[] directions = { G10_Direction.None, G10_Direction.Up, G10_Direction.Left, G10_Direction.Down, G10_Direction.Right };
            for (int d = 0; d < directions.Length; d++)
            {
                G10_Direction direction = directions[d];
                _PacMan[(int)direction] = new G10_Sprite[PacManFrameCount];
                for (int f = 0; f < PacManFrameCount; f++)
                {
                    string[] rows = direction == G10_Direction.None ? baseRows[2] : Rotate(baseRows[f], direction);
                    _PacMan[(int)direction][f] = G10_Sprite.FromPattern(rows, _PacManPalette);
                }
            }
        }

        private static string[] BuildPacManRows(double halfAngleDegrees)
        {
            char[][] canvas = NewCanvas();
            double limit = halfAngleDegrees * Math.PI / 180.0;
            for (int y = 0; y < ActorSize; y++)
            {
                for (int x = 0; x < ActorSize; x++)
                {
                    double dx = x - _DiscCentre;
                    double dy = y - _DiscCentre;
                    if (dx * dx + dy * dy > _PacManRadiusSquared) continue;
                    if (halfAngleDegrees > 0.0 && dx > 0.0 && Math.Abs(Math.Atan2(dy, dx)) < limit) continue;
                    canvas[y][x] = 'Y';
                }
            }
            return ToRows(canvas);
        }

        // Rotates a right-facing pattern so that it faces the given direction.
        private static string[] Rotate(string[] rows, G10_Direction direction)
        {
            int size = rows.Length;
            string[] result = new string[size];
            for (int y = 0; y < size; y++)
            {
                char[] line = new char[size];
                for (int x = 0; x < size; x++)
                {
                    switch (direction)
                    {
                        case G10_Direction.Left:
                            line[x] = rows[y][size - 1 - x];
                            break;
                        case G10_Direction.Up:
                            line[x] = rows[x][size - 1 - y];
                            break;
                        case G10_Direction.Down:
                            line[x] = rows[size - 1 - x][y];
                            break;
                        default:
                            line[x] = rows[y][x];
                            break;
                    }
                }
                result[y] = new string(line);
            }
            return result;
        }

        private static void BuildDeath()
        {
            for (int f = 0; f < _DeathAngles.Length; f++)
            {
                char[][] canvas = NewCanvas();
                double limit = _DeathAngles[f] * Math.PI / 180.0;
                for (int y = 0; y < ActorSize; y++)
                {
                    for (int x = 0; x < ActorSize; x++)
                    {
                        double dx = x - _DiscCentre;
                        double dy = y - _DiscCentre;
                        if (dx * dx + dy * dy > _PacManRadiusSquared) continue;
                        // Angle between the pixel and the "up" direction.
                        if (Math.Atan2(Math.Abs(dx), -dy) < limit) continue;
                        canvas[y][x] = 'Y';
                    }
                }
                _Death[f] = G10_Sprite.FromPattern(ToRows(canvas), _PacManPalette);
            }

            // Final "pop": a small burst of dots, mirrored into all four quadrants.
            char[][] pop = NewCanvas();
            int[][] offsets = { new int[] { 0, 3 }, new int[] { 0, 6 }, new int[] { 3, 0 }, new int[] { 6, 0 }, new int[] { 2, 2 }, new int[] { 4, 4 } };
            for (int i = 0; i < offsets.Length; i++)
            {
                int a = offsets[i][0];
                int b = offsets[i][1];
                Put(pop, 7 - a, 7 - b, 'Y');
                Put(pop, 8 + a, 7 - b, 'Y');
                Put(pop, 7 - a, 8 + b, 'Y');
                Put(pop, 8 + a, 8 + b, 'Y');
            }
            _Death[_DeathFrames - 1] = G10_Sprite.FromPattern(ToRows(pop), _PacManPalette);
        }

        // ---- Ghosts ----

        private static void BuildGhosts()
        {
            G10_Direction[] directions = { G10_Direction.None, G10_Direction.Up, G10_Direction.Left, G10_Direction.Down, G10_Direction.Right };
            for (int k = 0; k < _GhostKindCount; k++)
            {
                Dictionary<char, uint> palette = new Dictionary<char, uint>
                {
                    { 'B', _GhostColors[k] }, { 'W', 0xFFFFFFFF }, { 'P', 0xFF2121DE }
                };
                _Ghosts[k] = new G10_Sprite[_DirectionCount][];
                for (int d = 0; d < directions.Length; d++)
                {
                    G10_Direction direction = directions[d];
                    _Ghosts[k][(int)direction] = new G10_Sprite[GhostFrameCount];
                    for (int f = 0; f < GhostFrameCount; f++)
                    {
                        char[][] canvas = GhostBody(f);
                        DrawEyes(canvas, direction);
                        _Ghosts[k][(int)direction][f] = G10_Sprite.FromPattern(ToRows(canvas), palette);
                    }
                }
            }

            Dictionary<char, uint> eyePalette = new Dictionary<char, uint>
            {
                { 'W', 0xFFFFFFFF }, { 'P', 0xFF2121DE }
            };
            for (int d = 0; d < directions.Length; d++)
            {
                char[][] canvas = NewCanvas();
                DrawEyes(canvas, directions[d]);
                _Eyes[(int)directions[d]] = G10_Sprite.FromPattern(ToRows(canvas), eyePalette);
            }

            for (int flash = 0; flash < 2; flash++)
            {
                _Frightened[flash] = new G10_Sprite[GhostFrameCount];
                for (int f = 0; f < GhostFrameCount; f++)
                {
                    char[][] canvas = GhostBody(f);
                    DrawFrightenedFace(canvas);
                    _Frightened[flash][f] = G10_Sprite.FromPattern(ToRows(canvas), flash == 1 ? _FlashPalette : _FrightenedPalette);
                }
            }
        }

        // Round head and wavy skirt; the two frames differ in the skirt only.
        private static char[][] GhostBody(int frame)
        {
            string[] head =
            {
                "................",
                ".....BBBBBB.....",
                "...BBBBBBBBBB...",
                "..BBBBBBBBBBBB..",
                "..BBBBBBBBBBBB..",
                ".BBBBBBBBBBBBBB.",
                ".BBBBBBBBBBBBBB.",
                ".BBBBBBBBBBBBBB.",
                ".BBBBBBBBBBBBBB.",
                ".BBBBBBBBBBBBBB.",
                ".BBBBBBBBBBBBBB.",
                ".BBBBBBBBBBBBBB.",
                ".BBBBBBBBBBBBBB.",
                ".BBBBBBBBBBBBBB."
            };
            string[] skirt0 =
            {
                ".BBBB.BBBB.BBBB.",
                ".BBB...BB...BBB."
            };
            string[] skirt1 =
            {
                ".BB..BBBBBB..BB.",
                ".B....BBBB....B."
            };
            string[] skirt = frame == 0 ? skirt0 : skirt1;
            char[][] canvas = NewCanvas();
            for (int y = 0; y < head.Length; y++)
            {
                canvas[y] = head[y].ToCharArray();
            }
            for (int y = 0; y < skirt.Length; y++)
            {
                canvas[head.Length + y] = skirt[y].ToCharArray();
            }
            return canvas;
        }

        // Eyeballs are 4 x 4 pixels, pupils 2 x 2. Both move towards the direction.
        private static void DrawEyes(char[][] canvas, G10_Direction direction)
        {
            int sx = 0;
            int sy = 0;
            switch (direction)
            {
                case G10_Direction.Right:
                    sx = 1;
                    break;
                case G10_Direction.Up:
                    sy = -1;
                    break;
                case G10_Direction.Down:
                    sy = 1;
                    break;
                default:
                    sx = -1;
                    break;
            }
            int[] eyeX = { 3, 9 };
            for (int i = 0; i < eyeX.Length; i++)
            {
                int ex = eyeX[i] + sx;
                int ey = 4 + sy;
                FillRect(canvas, ex, ey, 4, 4, 'W');
                FillRect(canvas, ex + 1 + sx, ey + 1 + sy, 2, 2, 'P');
            }
        }

        private static void DrawFrightenedFace(char[][] canvas)
        {
            FillRect(canvas, 4, 5, 2, 2, 'F');
            FillRect(canvas, 10, 5, 2, 2, 'F');
            string[] mouth = { ".##..##..##.", "#..##..##..#" };
            for (int y = 0; y < mouth.Length; y++)
            {
                for (int x = 0; x < mouth[y].Length; x++)
                {
                    if (mouth[y][x] == '#') Put(canvas, 2 + x, 10 + y, 'F');
                }
            }
        }

        // ---- Fruit ----

        private static void BuildFruits()
        {
            _Fruits[(int)G10_FruitKind.Cherry] = G10_Sprite.FromPattern(_CherryRows, _FruitPalette);
            _Fruits[(int)G10_FruitKind.Strawberry] = G10_Sprite.FromPattern(ToRows(StrawberryCanvas()), _FruitPalette);
            _Fruits[(int)G10_FruitKind.Orange] = G10_Sprite.FromPattern(ToRows(OrangeCanvas()), _FruitPalette);
            _Fruits[(int)G10_FruitKind.Apple] = G10_Sprite.FromPattern(ToRows(AppleCanvas()), _FruitPalette);
            _Fruits[(int)G10_FruitKind.Melon] = G10_Sprite.FromPattern(ToRows(MelonCanvas()), _FruitPalette);
            _Fruits[(int)G10_FruitKind.Bell] = G10_Sprite.FromPattern(ToRows(BellCanvas()), _FruitPalette);
            _Fruits[(int)G10_FruitKind.Key] = G10_Sprite.FromPattern(_KeyRows, _FruitPalette);
        }

        private static char[][] StrawberryCanvas()
        {
            char[][] canvas = NewCanvas();
            CenteredRow(canvas, 1, 2, 'G');
            CenteredRow(canvas, 2, 8, 'G');
            int[] widths = { 12, 14, 14, 14, 14, 12, 10, 8, 6, 4, 2 };
            for (int i = 0; i < widths.Length; i++)
            {
                CenteredRow(canvas, 3 + i, widths[i], 'R');
            }
            FillRect(canvas, 6, 3, 4, 1, 'G');
            int[][] seeds = { new int[] { 4, 4 }, new int[] { 11, 4 }, new int[] { 7, 6 }, new int[] { 8, 6 },
                              new int[] { 4, 8 }, new int[] { 11, 8 }, new int[] { 7, 10 }, new int[] { 8, 10 } };
            for (int i = 0; i < seeds.Length; i++)
            {
                Put(canvas, seeds[i][0], seeds[i][1], 'Y');
            }
            return canvas;
        }

        private static char[][] DiscCanvas(char fill)
        {
            char[][] canvas = NewCanvas();
            for (int i = 0; i < _DiscWidths.Length; i++)
            {
                CenteredRow(canvas, 2 + i, _DiscWidths[i], fill);
            }
            return canvas;
        }

        private static char[][] OrangeCanvas()
        {
            char[][] canvas = DiscCanvas('O');
            FillRect(canvas, 8, 0, 3, 1, 'G');
            FillRect(canvas, 7, 1, 2, 1, 'G');
            Put(canvas, 4, 4, 'L');
            Put(canvas, 5, 4, 'L');
            Put(canvas, 3, 5, 'L');
            Put(canvas, 3, 6, 'L');
            return canvas;
        }

        private static char[][] MelonCanvas()
        {
            char[][] canvas = DiscCanvas('M');
            for (int y = 2; y < ActorSize; y++)
            {
                for (int x = 0; x < ActorSize; x++)
                {
                    if (canvas[y][x] == 'M' && ((x + y) % 4 == 0 || ((x - y) % 4 + 4) % 4 == 0))
                    {
                        canvas[y][x] = 'N';
                    }
                }
            }
            FillRect(canvas, 7, 0, 2, 2, 'S');
            return canvas;
        }

        private static char[][] AppleCanvas()
        {
            char[][] canvas = NewCanvas();
            Put(canvas, 8, 0, 'S');
            FillRect(canvas, 9, 0, 3, 1, 'G');
            Put(canvas, 8, 1, 'S');
            FillRect(canvas, 3, 2, 4, 1, 'R');
            FillRect(canvas, 9, 2, 4, 1, 'R');
            CenteredRow(canvas, 3, 12, 'R');
            for (int y = 4; y <= 10; y++)
            {
                CenteredRow(canvas, y, 14, 'R');
            }
            CenteredRow(canvas, 11, 12, 'R');
            CenteredRow(canvas, 12, 12, 'R');
            CenteredRow(canvas, 13, 10, 'R');
            CenteredRow(canvas, 14, 8, 'R');
            Put(canvas, 3, 5, 'H');
            Put(canvas, 3, 6, 'H');
            return canvas;
        }

        private static char[][] BellCanvas()
        {
            char[][] canvas = NewCanvas();
            CenteredRow(canvas, 0, 2, 'Y');
            CenteredRow(canvas, 1, 6, 'Y');
            CenteredRow(canvas, 2, 8, 'Y');
            for (int y = 3; y <= 8; y++)
            {
                CenteredRow(canvas, y, 10, 'Y');
            }
            CenteredRow(canvas, 9, 12, 'Y');
            CenteredRow(canvas, 10, 12, 'Y');
            CenteredRow(canvas, 11, 14, 'Y');
            CenteredRow(canvas, 12, 14, 'Y');
            // Shade on the right edge, highlight on the left.
            for (int y = 3; y <= 12; y++)
            {
                int last = ActorSize - 1;
                while (last >= 0 && canvas[y][last] == '.') last--;
                canvas[y][last] = 'O';
            }
            Put(canvas, 4, 4, 'H');
            Put(canvas, 4, 5, 'H');
            Put(canvas, 4, 6, 'H');
            CenteredRow(canvas, 13, 4, 'C');
            CenteredRow(canvas, 14, 2, 'C');
            return canvas;
        }

        // ---- Points ----

        private static G10_Sprite BuildPoints(int points)
        {
            string text = Math.Abs(points).ToString();
            string[] rows = new string[5];
            for (int y = 0; y < rows.Length; y++)
            {
                rows[y] = "";
            }
            for (int i = 0; i < text.Length; i++)
            {
                string[] digit = _Digits[text[i] - '0'];
                for (int y = 0; y < rows.Length; y++)
                {
                    rows[y] += (i > 0 ? "." : "") + digit[y];
                }
            }
            bool ghostScore = points == 200 || points == 400 || points == 800 || points == 1600;
            return G10_Sprite.FromPattern(rows, ghostScore ? _CyanPointsPalette : _PinkPointsPalette);
        }

        // ---- Pattern helpers ----

        private static char[][] NewCanvas()
        {
            char[][] canvas = new char[ActorSize][];
            for (int y = 0; y < ActorSize; y++)
            {
                canvas[y] = new string('.', ActorSize).ToCharArray();
            }
            return canvas;
        }

        private static string[] ToRows(char[][] canvas)
        {
            string[] rows = new string[canvas.Length];
            for (int y = 0; y < canvas.Length; y++)
            {
                rows[y] = new string(canvas[y]);
            }
            return rows;
        }

        private static void Put(char[][] canvas, int x, int y, char c)
        {
            if (y < 0 || y >= canvas.Length || x < 0 || x >= canvas[y].Length) return;
            canvas[y][x] = c;
        }

        private static void FillRect(char[][] canvas, int x, int y, int width, int height, char c)
        {
            for (int yy = y; yy < y + height; yy++)
            {
                for (int xx = x; xx < x + width; xx++)
                {
                    Put(canvas, xx, yy, c);
                }
            }
        }

        private static void CenteredRow(char[][] canvas, int y, int width, char c)
        {
            FillRect(canvas, (ActorSize - width) / 2, y, width, 1, c);
        }
    }
}
