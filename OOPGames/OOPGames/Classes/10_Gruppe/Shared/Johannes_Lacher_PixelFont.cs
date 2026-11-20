using System.Collections.Generic;

namespace OOPGames
{
    // Pixel font for HUDs and messages of the group-10 games. Glyphs are white;
    // the renderer tints them when drawing.
    internal static class G10_PixelFont
    {
        public const int GlyphWidth = 5;
        public const int GlyphHeight = 7;
        // Horizontal distance from one character to the next.
        public const int Advance = 6;

        // Each entry: character followed by seven rows separated by '|'.
        private static readonly string[] _Definitions =
        {
            "A.###.|#...#|#...#|#####|#...#|#...#|#...#",
            "B####.|#...#|#...#|####.|#...#|#...#|####.",
            "C.####|#....|#....|#....|#....|#....|.####",
            "D####.|#...#|#...#|#...#|#...#|#...#|####.",
            "E#####|#....|#....|####.|#....|#....|#####",
            "F#####|#....|#....|####.|#....|#....|#....",
            "G.####|#....|#....|#..##|#...#|#...#|.###.",
            "H#...#|#...#|#...#|#####|#...#|#...#|#...#",
            "I#####|..#..|..#..|..#..|..#..|..#..|#####",
            "J..###|...#.|...#.|...#.|...#.|#..#.|.##..",
            "K#...#|#..#.|#.#..|##...|#.#..|#..#.|#...#",
            "L#....|#....|#....|#....|#....|#....|#####",
            "M#...#|##.##|#.#.#|#.#.#|#...#|#...#|#...#",
            "N#...#|##..#|#.#.#|#.#.#|#..##|#...#|#...#",
            "O.###.|#...#|#...#|#...#|#...#|#...#|.###.",
            "P####.|#...#|#...#|####.|#....|#....|#....",
            "Q.###.|#...#|#...#|#...#|#.#.#|#..#.|.##.#",
            "R####.|#...#|#...#|####.|#.#..|#..#.|#...#",
            "S.####|#....|#....|.###.|....#|....#|####.",
            "T#####|..#..|..#..|..#..|..#..|..#..|..#..",
            "U#...#|#...#|#...#|#...#|#...#|#...#|.###.",
            "V#...#|#...#|#...#|#...#|#...#|.#.#.|..#..",
            "W#...#|#...#|#...#|#.#.#|#.#.#|##.##|#...#",
            "X#...#|#...#|.#.#.|..#..|.#.#.|#...#|#...#",
            "Y#...#|#...#|.#.#.|..#..|..#..|..#..|..#..",
            "Z#####|....#|...#.|..#..|.#...|#....|#####",
            "0.###.|#...#|#..##|#.#.#|##..#|#...#|.###.",
            "1..#..|.##..|..#..|..#..|..#..|..#..|.###.",
            "2.###.|#...#|....#|...#.|..#..|.#...|#####",
            "3.###.|#...#|....#|..##.|....#|#...#|.###.",
            "4...#.|..##.|.#.#.|#..#.|#####|...#.|...#.",
            "5#####|#....|####.|....#|....#|#...#|.###.",
            "6..##.|.#...|#....|####.|#...#|#...#|.###.",
            "7#####|....#|...#.|..#..|.#...|.#...|.#...",
            "8.###.|#...#|#...#|.###.|#...#|#...#|.###.",
            "9.###.|#...#|#...#|.####|....#|...#.|.##..",
            "!..#..|..#..|..#..|..#..|..#..|.....|..#..",
            "-.....|.....|.....|#####|.....|.....|.....",
            ":.....|..#..|.....|.....|.....|..#..|.....",
            "......|.....|.....|.....|.....|.##..|.##..",
            "/....#|....#|...#.|..#..|.#...|#....|#....",
            "'..#..|..#..|.#...|.....|.....|.....|....."
        };

        private static readonly Dictionary<char, uint> _Palette = new Dictionary<char, uint>
        {
            { '#', 0xFFFFFFFF }
        };

        private static readonly Dictionary<char, G10_Sprite> _Glyphs = new Dictionary<char, G10_Sprite>();
        private static readonly G10_Sprite _Blank;

        static G10_PixelFont()
        {
            for (int i = 0; i < _Definitions.Length; i++)
            {
                string definition = _Definitions[i];
                string[] rows = definition.Substring(1).Split('|');
                _Glyphs[definition[0]] = G10_Sprite.FromPattern(rows, _Palette);
            }
            string[] blankRows = new string[GlyphHeight];
            for (int y = 0; y < GlyphHeight; y++)
            {
                blankRows[y] = new string('.', GlyphWidth);
            }
            _Blank = G10_Sprite.FromPattern(blankRows, _Palette);
        }

        // Supports A-Z, 0-9, space and ! - : . / ' ; lower-case letters are drawn as upper case;
        // unknown characters are drawn blank.
        public static G10_Sprite Glyph(char c)
        {
            G10_Sprite sprite;
            if (_Glyphs.TryGetValue(char.ToUpperInvariant(c), out sprite))
            {
                return sprite;
            }
            return _Blank;
        }
    }
}
