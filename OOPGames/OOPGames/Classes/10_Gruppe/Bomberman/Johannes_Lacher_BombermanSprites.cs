using System;
using System.Collections.Generic;

namespace OOPGames
{
    // All Bomberman graphics, drawn by us as patterns or by code. Sprites are built once and cached.
    internal static class G10_BombermanSprites
    {
        public const int Size = G10_BomberConstants.TileSize;
        public const int WalkFrameCount = 4;
        public const int EnemyFrameCount = 2;
        public const int BurnFrameCount = 3;
        public const int SmokeFrameCount = 4;

        static readonly Dictionary<char, uint> _Palette = new Dictionary<char, uint>
        {
            { 'K', G10_C64Colors.Black }, { 'W', G10_C64Colors.White }, { 'R', G10_C64Colors.Red },
            { 'C', G10_C64Colors.Cyan }, { 'P', G10_C64Colors.Purple }, { 'G', G10_C64Colors.Green },
            { 'B', G10_C64Colors.Blue }, { 'Y', G10_C64Colors.Yellow }, { 'O', G10_C64Colors.Orange },
            { 'N', G10_C64Colors.Brown }, { 'r', G10_C64Colors.LightRed }, { 'D', G10_C64Colors.DarkGrey },
            { 'g', G10_C64Colors.Grey }, { 'l', G10_C64Colors.LightGreen }, { 'b', G10_C64Colors.LightBlue },
            { 'L', G10_C64Colors.LightGrey }
        };

        // The hero: a little demolition robot with a hard hat. Rows 0-12 are head and body,
        // rows 13-15 the legs, which change while walking.
        static readonly string[] _BodyDown =
        {
            "................",
            "......YYYY......",
            "....YYYWYYYY....",
            "...YYYYWYYYYY...",
            "...YYYYYYYYYY...",
            "..OOOOOOOOOOOO..",
            "...LLLLLLLLLL...",
            "...LKKKKKKKKL...",
            "...LKCCKKCCKL...",
            "....LLLLLLLL....",
            "..WBBBBBBBBBBW..",
            "..WbBBBRRBBBbW..",
            "....BBBBBBBB...."
        };

        static readonly string[] _BodyUp =
        {
            "................",
            "......YYYY......",
            "....YYYYYYYY....",
            "...YYYYYYYYYY...",
            "...YYYYYYYYYY...",
            "..OOOOOOOOOOOO..",
            "...LLLLLLLLLL...",
            "...LLLDDDDLLL...",
            "...LLLDDDDLLL...",
            "....LLLLLLLL....",
            "..WBBBBBBBBBBW..",
            "..WBBbBBBBbBBW..",
            "....BBBBBBBB...."
        };

        static readonly string[] _BodyRight =
        {
            "................",
            ".....YYYY.......",
            "...YYYYWYYY.....",
            "..YYYYYYWYYY....",
            "..YYYYYYYYYY....",
            "..OOOOOOOOOOOOO.",
            "...LLLLLLLLL....",
            "...LLLLKKKKK....",
            "...LLLLKKCCK....",
            "....LLLLLLL.....",
            "....BBBBBBB.....",
            "....BBBWBBB.....",
            "....BBBBBBB....."
        };

        // Walk cycle: step, stand, other step, stand.
        static readonly string[][] _LegsFront =
        {
            new[] { "....BBB..BBB....", "....BBB..NNNN...", "...NNNN........." },
            new[] { "....BBB..BBB....", "...NNNN..NNNN...", "................" },
            new[] { "....BBB..BBB....", "...NNNN..BBB....", ".........NNNN..." },
            new[] { "....BBB..BBB....", "...NNNN..NNNN...", "................" }
        };

        static readonly string[][] _LegsSide =
        {
            new[] { "....BBB.BBB.....", "...NNN...NNNN...", "................" },
            new[] { ".....BBBBB......", ".....NNNNNN.....", "................" },
            new[] { ".....BBBBB......", "......NNNNN.....", "................" },
            new[] { ".....BBBBB......", ".....NNNNNN.....", "................" }
        };

        static readonly string[] _BombIcon =
        {
            "........Y...",
            ".......N.Y..",
            "......N.....",
            "...KKKNK....",
            "..KKKKKKK...",
            ".KWWKKKKKK..",
            ".KWKKKKKKK..",
            ".KKKKKKKKK..",
            ".KKKKKKKKK..",
            "..KKKKKKK...",
            "...KKKKK....",
            "............"
        };

        static readonly string[] _FireIcon =
        {
            ".....r......",
            "....rr......",
            "....rYr.....",
            "...rrYr..r..",
            "...rYYrr.rr.",
            "..rrYYYrrrr.",
            "..rYYWYYYrr.",
            ".rrYWWWYYr..",
            ".rYYWWWYYr..",
            ".rrYYWYYrr..",
            "..rrYYYrr...",
            "...rrrrr...."
        };

        static readonly string[] _SpeedIcon =
        {
            "............",
            "....rrr.....",
            "....rrr.....",
            "....rrr.....",
            "....rrrr....",
            "W...rrrrrr..",
            "....rrrrrrr.",
            "WW..rrrrrrr.",
            "...rrrrrrrr.",
            "....YY..YY..",
            "....YY..YY..",
            "............"
        };

        static readonly string[] _RemoteIcon =
        {
            ".........W..",
            "........W...",
            ".......W....",
            "......D.....",
            "...DDDDDDD..",
            "...DLLLLLD..",
            "...DLRRRLD..",
            "...DLRRRLD..",
            "...DLLLLLD..",
            "...DLKLKLD..",
            "...DDDDDDD..",
            "............"
        };

        static readonly string[] _Door =
        {
            "................",
            ".....DDDDDD.....",
            "...DDLLLLLLDD...",
            "..DLLIIIIIILLD..",
            "..DLIIIIIIIILD..",
            ".DLIIIIIIIIIILD.",
            ".DLIIIIIIIIIILD.",
            ".DLIIIIIIIIIILD.",
            ".DLIIIIIIIIIILD.",
            ".DLIIIIIIIIIILD.",
            ".DLIIIIIIIIIILD.",
            ".DLIIIIIIIIIILD.",
            ".DLIIIIIIIIIILD.",
            ".DLIIIIIIIIIILD.",
            ".DDDDDDDDDDDDDD.",
            "................"
        };

        static readonly string[] _LifeIcon =
        {
            "..YYYY..",
            ".YYYYYY.",
            "OOOOOOOO",
            ".LKKKKL.",
            ".LKCKCL.",
            "..LLLL..",
            ".WBBBBW.",
            "..N..N.."
        };

        static readonly G10_Sprite[][] _Player = new G10_Sprite[5][];
        static readonly G10_Sprite[][] _Enemies = new G10_Sprite[5][];
        static readonly G10_Sprite[] _Burn = new G10_Sprite[BurnFrameCount];
        static readonly G10_Sprite[] _Smoke = new G10_Sprite[SmokeFrameCount];
        static readonly Dictionary<G10_BomberItemKind, G10_Sprite> _Items = new Dictionary<G10_BomberItemKind, G10_Sprite>();
        static readonly G10_Sprite[] _Bombs = new G10_Sprite[4];

        static G10_BombermanSprites()
        {
            BuildPlayer();
            foreach (G10_BomberEnemyKind kind in Enum.GetValues(typeof(G10_BomberEnemyKind)))
            {
                _Enemies[(int)kind] = new G10_Sprite[EnemyFrameCount];
                for (int frame = 0; frame < EnemyFrameCount; frame++)
                    _Enemies[(int)kind][frame] = BuildEnemy(kind, frame);
            }
            Floor = BuildFloor();
            Pillar = BuildPillar();
            Brick = BuildBrick();
            for (int frame = 0; frame < BurnFrameCount; frame++) _Burn[frame] = BuildBurn(frame);
            for (int frame = 0; frame < SmokeFrameCount; frame++) _Smoke[frame] = BuildSmoke(frame);
            for (int frame = 0; frame < 4; frame++) _Bombs[frame] = BuildBomb(frame % 2 == 1, frame >= 2);
            _Items[G10_BomberItemKind.BombUp] = BuildItem(_BombIcon);
            _Items[G10_BomberItemKind.FireUp] = BuildItem(_FireIcon);
            _Items[G10_BomberItemKind.SpeedUp] = BuildItem(_SpeedIcon);
            _Items[G10_BomberItemKind.Remote] = BuildItem(_RemoteIcon);
            DoorClosed = BuildDoor(G10_C64Colors.Brown, true);
            DoorOpenDark = BuildDoor(G10_C64Colors.Black, false);
            DoorOpenBright = BuildDoor(G10_C64Colors.Yellow, false);
            LifeIcon = G10_Sprite.FromPattern(_LifeIcon, _Palette);
        }

        public static G10_Sprite Floor { get; private set; }
        public static G10_Sprite Pillar { get; private set; }
        public static G10_Sprite Brick { get; private set; }
        public static G10_Sprite DoorClosed { get; private set; }
        public static G10_Sprite DoorOpenDark { get; private set; }
        public static G10_Sprite DoorOpenBright { get; private set; }
        public static G10_Sprite LifeIcon { get; private set; }

        public static G10_Sprite Player(G10_Direction direction, int walkFrame)
        {
            return _Player[(int)direction][walkFrame % WalkFrameCount];
        }

        public static G10_Sprite Enemy(G10_BomberEnemyKind kind, int frame)
        {
            return _Enemies[(int)kind][frame % EnemyFrameCount];
        }

        public static G10_Sprite Bomb(bool swollen, bool remote)
        {
            return _Bombs[(swollen ? 1 : 0) + (remote ? 2 : 0)];
        }

        public static G10_Sprite Burn(int frame) { return _Burn[Math.Min(frame, BurnFrameCount - 1)]; }
        public static G10_Sprite Smoke(int frame) { return _Smoke[Math.Min(frame, SmokeFrameCount - 1)]; }
        public static G10_Sprite Item(G10_BomberItemKind kind) { return _Items[kind]; }

        static void BuildPlayer()
        {
            for (int d = 0; d < 5; d++) _Player[d] = new G10_Sprite[WalkFrameCount];
            for (int frame = 0; frame < WalkFrameCount; frame++)
            {
                G10_Sprite down = PlayerFrame(_BodyDown, _LegsFront[frame]).ToSprite();
                _Player[(int)G10_Direction.Down][frame] = down;
                _Player[(int)G10_Direction.None][frame] = down;
                _Player[(int)G10_Direction.Up][frame] = PlayerFrame(_BodyUp, _LegsFront[frame]).ToSprite();
                G10_PixelCanvas side = PlayerFrame(_BodyRight, _LegsSide[frame]);
                _Player[(int)G10_Direction.Right][frame] = side.ToSprite();
                _Player[(int)G10_Direction.Left][frame] = side.ToMirroredSprite();
            }
        }

        static G10_PixelCanvas PlayerFrame(string[] body, string[] legs)
        {
            G10_PixelCanvas canvas = new G10_PixelCanvas(Size, Size);
            canvas.Pattern(0, 0, body, _Palette);
            canvas.Pattern(0, body.Length, legs, _Palette);
            return canvas;
        }

        static G10_Sprite BuildEnemy(G10_BomberEnemyKind kind, int frame)
        {
            G10_PixelCanvas c = new G10_PixelCanvas(Size, Size);
            switch (kind)
            {
                case G10_BomberEnemyKind.Puff:
                    // A soft cloud creature with big eyes.
                    c.Disc(7.5, 7.0, 6.0, G10_C64Colors.LightRed);
                    for (int i = 0; i < 4; i++) c.Disc(3.5 + i * 2.8, 12.0 + (i + frame) % 2, 1.8, G10_C64Colors.LightRed);
                    for (int y = 11; y < Size; y++)
                        for (int x = 0; x < Size; x++)
                            if (c.Get(x, y) != 0) c.Set(x, y, G10_C64Colors.Red);
                    Eyes(c, 5, 6, 10, 6, frame);
                    break;
                case G10_BomberEnemyKind.Spark:
                    // An electric ball; the spikes turn by 45 degrees each frame.
                    for (int i = 1; i <= 7; i++)
                    {
                        if (frame == 0)
                        {
                            c.Set(7, 7 - i, G10_C64Colors.Yellow); c.Set(8, 8 + i, G10_C64Colors.Yellow);
                            c.Set(7 - i, 8, G10_C64Colors.Yellow); c.Set(8 + i, 7, G10_C64Colors.Yellow);
                        }
                        else if (i <= 6)
                        {
                            c.Set(7 - i, 7 - i, G10_C64Colors.Yellow); c.Set(8 + i, 8 + i, G10_C64Colors.Yellow);
                            c.Set(7 - i, 8 + i, G10_C64Colors.Yellow); c.Set(8 + i, 7 - i, G10_C64Colors.Yellow);
                        }
                    }
                    c.Disc(7.5, 7.5, 5.0, G10_C64Colors.Cyan);
                    c.Disc(7.5, 7.5, 2.6, G10_C64Colors.White);
                    c.Set(6, 7, G10_C64Colors.Black);
                    c.Set(9, 7, G10_C64Colors.Black);
                    break;
                case G10_BomberEnemyKind.Drift:
                    // A see-through jelly that floats through bricks.
                    c.Disc(7.5, 7.0, 6.0, G10_C64Colors.Purple);
                    c.Rect(2, 7, 12, 5, G10_C64Colors.Purple);
                    for (int x = 2; x < 14; x += 2) c.Rect(x + frame, 12, 1, 3 - (x / 2 + frame) % 2, G10_C64Colors.Purple);
                    for (int y = 0; y < 12; y++)
                        for (int x = 0; x < Size; x++)
                            if (c.Get(x, y) != 0 && (x + y + frame) % 3 == 0) c.Set(x, y, G10_C64Colors.LightBlue);
                    Eyes(c, 5, 6, 10, 6, frame);
                    break;
                case G10_BomberEnemyKind.Hunter:
                    // A fast beetle that snaps its jaws.
                    for (int i = 0; i < 3; i++)
                    {
                        int legY = 6 + i * 3 + (frame + i) % 2;
                        c.Rect(0, legY, 3, 1, G10_C64Colors.Black);
                        c.Rect(13, legY, 3, 1, G10_C64Colors.Black);
                    }
                    c.Ellipse(7.5, 8.5, 6.0, 6.0, G10_C64Colors.Orange);
                    c.Rect(3, 9, 10, 1, G10_C64Colors.Brown);
                    c.Rect(3, 12, 10, 1, G10_C64Colors.Brown);
                    if (frame == 0)
                    {
                        c.Set(4, 1, G10_C64Colors.Black); c.Set(5, 2, G10_C64Colors.Black);
                        c.Set(11, 1, G10_C64Colors.Black); c.Set(10, 2, G10_C64Colors.Black);
                    }
                    else
                    {
                        c.Set(6, 1, G10_C64Colors.Black); c.Set(6, 2, G10_C64Colors.Black);
                        c.Set(9, 1, G10_C64Colors.Black); c.Set(9, 2, G10_C64Colors.Black);
                    }
                    c.Rect(4, 5, 2, 2, G10_C64Colors.Red); c.Set(4, 5, G10_C64Colors.White);
                    c.Rect(10, 5, 2, 2, G10_C64Colors.Red); c.Set(10, 5, G10_C64Colors.White);
                    break;
                default:
                    // Phantom: a flickering wisp with red eyes.
                    c.Disc(7.5, 10.0, 5.5, G10_C64Colors.LightBlue);
                    for (int y = 1; y < 6; y++)
                    {
                        int half = (y + 1) / 2;
                        int shift = (y + frame) % 2 == 0 ? 1 : -1;
                        c.Rect(8 - half + shift, y, half * 2, 1, G10_C64Colors.LightBlue);
                    }
                    c.Disc(7.5, 10.5, 3.0, G10_C64Colors.White);
                    c.Rect(5, 9, 2, 2, G10_C64Colors.Red);
                    c.Rect(9, 9, 2, 2, G10_C64Colors.Red);
                    break;
            }
            return c.ToSprite();
        }

        static void Eyes(G10_PixelCanvas c, int leftX, int leftY, int rightX, int rightY, int frame)
        {
            c.Rect(leftX - 1, leftY - 1, 3, 3, G10_C64Colors.White);
            c.Rect(rightX - 1, rightY - 1, 3, 3, G10_C64Colors.White);
            c.Set(leftX + frame, leftY, G10_C64Colors.Black);
            c.Set(rightX + frame, rightY, G10_C64Colors.Black);
        }

        static G10_Sprite BuildFloor()
        {
            G10_PixelCanvas c = new G10_PixelCanvas(Size, Size);
            c.Rect(0, 0, Size, Size, G10_C64Colors.Green);
            // A few blades of grass in a fixed pattern.
            int[] blades = { 2, 3, 11, 5, 6, 10, 13, 12, 4, 14 };
            for (int i = 0; i < blades.Length; i += 2)
            {
                c.Set(blades[i], blades[i + 1], G10_C64Colors.LightGreen);
                c.Set(blades[i], blades[i + 1] + 1, G10_C64Colors.LightGreen);
            }
            return c.ToSprite();
        }

        static G10_Sprite BuildPillar()
        {
            G10_PixelCanvas c = new G10_PixelCanvas(Size, Size);
            c.Rect(0, 0, Size, Size, G10_C64Colors.Grey);
            c.Rect(0, 0, Size, 2, G10_C64Colors.LightGrey);
            c.Rect(0, 0, 2, Size, G10_C64Colors.LightGrey);
            c.Rect(0, Size - 2, Size, 2, G10_C64Colors.DarkGrey);
            c.Rect(Size - 2, 0, 2, Size, G10_C64Colors.DarkGrey);
            c.Rect(5, 5, 6, 6, G10_C64Colors.DarkGrey);
            c.Rect(5, 5, 5, 5, G10_C64Colors.LightGrey);
            c.Rect(6, 6, 4, 4, G10_C64Colors.Grey);
            return c.ToSprite();
        }

        static G10_Sprite BuildBrick()
        {
            G10_PixelCanvas c = new G10_PixelCanvas(Size, Size);
            c.Rect(0, 0, Size, Size, G10_C64Colors.Red);
            for (int course = 0; course < 4; course++)
            {
                int top = course * 4;
                c.Rect(0, top, Size, 1, G10_C64Colors.LightRed);
                c.Rect(0, top + 3, Size, 1, G10_C64Colors.Brown);
                int offset = course % 2 == 0 ? 7 : 3;
                for (int x = offset; x < Size; x += 8) c.Rect(x, top, 1, 3, G10_C64Colors.Brown);
            }
            return c.ToSprite();
        }

        // A destroyed brick: first glowing, then crumbling, then smoking rubble.
        static G10_Sprite BuildBurn(int frame)
        {
            uint[][] colors =
            {
                new[] { G10_C64Colors.White, G10_C64Colors.Yellow, G10_C64Colors.LightRed },
                new[] { G10_C64Colors.Yellow, G10_C64Colors.LightRed, G10_C64Colors.Red },
                new[] { G10_C64Colors.Red, G10_C64Colors.DarkGrey, 0u }
            };
            G10_PixelCanvas c = new G10_PixelCanvas(Size, Size);
            Random random = new Random(41 + frame);
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    uint color = colors[frame][random.Next(3)];
                    if (frame > 0 && random.Next(4) == 0) color = 0;
                    c.Set(x, y, color);
                }
            }
            return c.ToSprite();
        }

        static G10_Sprite BuildSmoke(int frame)
        {
            G10_PixelCanvas c = new G10_PixelCanvas(Size, Size);
            double radius = 3 + frame * 1.5;
            c.Disc(7.5, 7.5, radius, frame < 2 ? G10_C64Colors.LightGrey : G10_C64Colors.Grey);
            c.Disc(7.5, 7.5, Math.Max(0, radius - 1 - frame), 0);
            Random random = new Random(7 + frame);
            for (int i = 0; i < 6 + frame * 8; i++) c.Set(random.Next(Size), random.Next(Size), 0);
            return c.ToSprite();
        }

        static G10_Sprite BuildBomb(bool swollen, bool remote)
        {
            G10_PixelCanvas c = new G10_PixelCanvas(Size, Size);
            double r = swollen ? 6.3 : 5.6;
            c.Disc(7.5, 9.0, r, G10_C64Colors.DarkGrey);
            c.Disc(7.5, 9.0, r - 1, G10_C64Colors.Black);
            c.Rect(4, 6, 2, 2, G10_C64Colors.White);
            c.Set(4, 8, G10_C64Colors.White);
            c.Rect(8, 2, 2, 2, G10_C64Colors.Brown);
            c.Set(10, 1, G10_C64Colors.Brown);
            c.Set(11, 0, swollen ? G10_C64Colors.Yellow : G10_C64Colors.White);
            c.Set(12, 1, swollen ? G10_C64Colors.White : G10_C64Colors.Yellow);
            if (remote) c.Rect(7, 9, 2, 2, swollen ? G10_C64Colors.LightRed : G10_C64Colors.Red);
            return c.ToSprite();
        }

        static G10_Sprite BuildItem(string[] icon)
        {
            G10_PixelCanvas c = new G10_PixelCanvas(Size, Size);
            c.Rect(0, 0, Size, Size, G10_C64Colors.LightBlue);
            c.Rect(1, 1, Size - 2, Size - 2, G10_C64Colors.Blue);
            c.Pattern(2, 2, icon, _Palette);
            return c.ToSprite();
        }

        static G10_Sprite BuildDoor(uint inside, bool knob)
        {
            Dictionary<char, uint> palette = new Dictionary<char, uint>(_Palette);
            palette['I'] = inside;
            G10_PixelCanvas c = new G10_PixelCanvas(Size, Size);
            c.Pattern(0, 0, _Door, palette);
            if (knob)
            {
                for (int x = 4; x < 12; x += 3) c.Rect(x, 4, 1, 10, G10_C64Colors.Orange);
                c.Set(11, 9, G10_C64Colors.Yellow);
            }
            return c.ToSprite();
        }
    }
}
