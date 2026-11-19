using System.Collections.Generic;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_FishSprites
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: 20 Fischgrafiken (16x16) für das Angelspiel und das Inventar. Quelle: frei nutzbares
    //               Pixel-Art-Fischpaket (von Patrick Lupierz bereitgestellt, "free to use"). Die Bilddaten wurden
    //               1:1 als Palette + Pixelzeilen in Code übertragen, damit keine Datei ins Projekt (csproj) muss.
    //               '.' = transparent, Buchstaben = Index in der Palette. Blickrichtung: Kopf oben rechts.
    //               Wie alle Grafiken per "<Sprite-ID>.png" in G1Casino_Assets austauschbar.
    // =====================================================
    public static class G1C_FishSprites
    {
        private sealed class FishPixels
        {
            public readonly uint[] Palette;
            public readonly string[] Rows;
            public FishPixels(uint[] palette, string[] rows) { Palette = palette; Rows = rows; }
        }

        private static readonly Dictionary<string, FishPixels> Fish = new Dictionary<string, FishPixels>
        {
            { "fish_sunfish", new FishPixels(new uint[] { 0x37241A, 0x92867C, 0x866C5E, 0x684F46, 0x38281F, 0xD2E1E6, 0x3E3632, 0xB1BBC2, 0x98A5B3, 0x333A3F, 0x7E8CA3, 0x9B938E },
                new[] { "..........aa....", ".........aba....", "........abca....", ".....aaaaacaa...", "...aadccddcada..", "..adccccccccbca.", ".eccafbcccbbgca.", ".ebhffhhghhbgbg.", ".ehhhhhbcghhbbg.", ".gihhhbccjihgbg.", "..gihhijjkhhjhg.", "...jkihkkhhilhj.", "....jjkiijigkj..", "......jjjbigg...", ".........gbg....", "..........gg...." }) },
            { "fish_bluegill", new FishPixels(new uint[] { 0x274848, 0x333A3F, 0x3E675B, 0x539170, 0x94B575, 0x859F9B, 0x6C868F, 0xD0763C, 0x37241A, 0xDEC384, 0xCA895A },
                new[] { "...........aabb.", ".....bbb.aacddeb", "...bbfgaacdhieeb", "..bgfgacddddhejb", ".bffgacdcdeeejji", ".bgfacddbddejji.", "bffgadcebeebjki.", "bggacdddebbjkib.", ".bgadceeejjjigb.", "..acdedejjkibfb.", ".badedejjkib.b..", "bgcddjjkiib.....", "bgfejiiigfb.....", ".bbfjibgfb......", "..bgfb.bb.......", "...bb..........." }) },
            { "fish_bass", new FishPixels(new uint[] { 0x221E19, 0x524D2E, 0x413824, 0xE1B85C, 0xB1A063, 0x2E2A1E, 0x7C763E, 0xDEC384, 0x998D51, 0x272119 },
                new[] { "............aaa.", "......aa..aabbba", ".....acbaabdabea", "....acbfbefbdeea", "...acbfbbgeebeha", "...acfbbgebfeha.", "..acbfbgfbfehha.", "..abfbgggfehha..", "..acfggegehhfa..", "...abgegihhaba..", ".jaagegehha.a...", "acbbgghhfa......", ".aacbaffca......", "...abaacba......", "...aca.aa.......", "....a..........." }) },
            { "fish_perch", new FishPixels(new uint[] { 0x221E19, 0x7C763E, 0x524D2E, 0x2C2119, 0x2E2A1E, 0xD0763C, 0xB1A063, 0xDEC384, 0x37241A, 0xCA895A },
                new[] { "......aa.aaa....", ".....abcaccca...", "....acbacccbcad.", "....abcecbbfabgd", "...abcecbcgbfghd", "...accecbgcgbgd.", "....aecbecghghi.", "..aaecbceceghi..", ".abcebgbceghei..", ".dcecgbgghhefi..", "..dcbgghhii.i...", ".idcghheei......", "ifjjheefji......", ".iijeifji.......", "..ifi.ii........", "...i............" }) },
            { "fish_minnow", new FishPixels(new uint[] { 0x333A3F, 0x7E8CA3, 0x37241A, 0x3E3632, 0xB1BBC2, 0xDEC384, 0xB1A063, 0x98A5B3 },
                new[] { "................", "................", "...........aa...", "..........abba..", ".....ccddabcea..", ".....cfgabeeha..", "......dabhbea...", "......abheeha...", ".....abheeha....", "...dabhhhaad....", "..cgfhddadfc....", "...ccfd...c.....", "....cgc.........", ".....c..........", "................", "................" }) },
            { "fish_smelt", new FishPixels(new uint[] { 0x333A3F, 0x454F57, 0x7E8CA3, 0xB1BBC2, 0x859F9B, 0x6C868F, 0x98A5B3 },
                new[] { "................", "................", "................", "...........ab...", "..........accb..", ".....aaaaacadb..", ".....aefacgdgb..", ".....afbcgdgb...", "......abgddb....", "......bgdgba....", "....aadgabfa....", "...aefaa..a.....", "....aea.........", ".....a..........", "................", "................" }) },
            { "fish_trout", new FishPixels(new uint[] { 0x37241A, 0x684F46, 0x524D2E, 0xE1B85C, 0xC0625F, 0x866C5E, 0x422E24, 0x7C763E, 0xCD8983, 0xB2A590, 0xC2BDB0 },
                new[] { "...........aaaa.", ".........aabbca.", "....aaaaabbdaea.", "...affbgbfhedica", "...afbgbfceiihha", "...abgbfgbiihja.", "....agfcgbgkja..", "..aagfchegkjga..", "..abgcheikjgba..", "...afheikjg.a...", ".aagceikjga.....", "afbghikggba.....", "afbbggabffa.....", ".aabba.aaa......", "..affa..........", "...aa..........." }) },
            { "fish_catfish", new FishPixels(new uint[] { 0x1B201F, 0x57635D, 0x70786C, 0x475853, 0x2B3533 },
                new[] { "....aaa.........", ".....aba...aa...", "......acaaaba...", "......abacca....", ".....ada.abca...", "....aaa..adbca..", "..........adaa..", "....e..aa.aacea.", "...e..abaabceda.", "....e.aeabeedda.", "....aabceeddbeca", "...abcccccdbdeba", "..abecccaccbaaa.", "..adbeeccbaa....", ".e.aadddbae...e.", "e..e.aaaa..eee.." }) },
            { "fish_browntrout", new FishPixels(new uint[] { 0x37241A, 0x2C2119, 0x684F46, 0xE1B85C, 0x866C5E, 0x422E24, 0xB2A590, 0xB1A063, 0x7C763E, 0xC2BDB0, 0x221E19 },
                new[] { "............abb.", "..........aaccb.", ".........acdacbb", "........aceedceb", ".......acfcghib.", "....aaacjfcfib..", "...aecfcehfiba..", "...acfcjhgikca..", "....aceehikaea..", "....acjhgk..a...", "..aacehikca.....", ".aecfibkcea.....", "aececb..aa......", ".aaccb..........", "..aeb...........", "...a............" }) },
            { "fish_redfish", new FishPixels(new uint[] { 0x52242A, 0xCD8983, 0xC0625F, 0x70283E, 0xB14A57, 0xD0763C, 0xE0BBAF, 0xCE8C85 },
                new[] { "......aaaa.aaaa.", "....aabbcdaeccca", "...abbcddeefacca", "...abcdececcfbca", "..abcdecccecbbga", "..abcdecccedeca.", "..acdeccdedccga.", "...adeccbdcbgda.", "....dccbbbbgdca.", ".aaaecbbbggdaha.", "aeeecebggdda.a..", ".aaeccgadcha....", "...aeca.aaa.....", "....aea.........", "....aea.........", ".....a.........." }) },
            { "fish_flounder", new FishPixels(new uint[] { 0x37241A, 0x866C5E, 0x684F46, 0x422E24, 0x553F36, 0xE1B85C },
                new[] { ".....aa..aaaaaaa", "...aabcdaeeeecea", "..acbcdddcfaccca", "..abcdeedcceecca", ".acbdeecedeefaca", ".abcdeecebdcceea", ".abcdeccabbeedda", ".abdeeceabbacca.", ".acdecceeaaecea.", "..adecceceecedca", ".aadeeeccceedcba", "accdeeeeedddcba.", "abccdddddccbbca.", ".abccaacbbbcaa..", "..abca.aaaaa....", "...aa..........." }) },
            { "fish_zander", new FishPixels(new uint[] { 0x274848, 0x3E675B, 0x539170, 0x94B575, 0xE1B85C, 0x333A3F, 0xB1BBC2, 0x98A5B3, 0x454F57 },
                new[] { "..........aaaa..", "......aaaabcda..", ".....acabcefdaff", "....ababbccecfgf", "....aaaccbcchggf", "...acabcdcbagghf", "..acbabcabbaghf.", "..acabcdbaahghf.", "..ababddchgghif.", "...aacdcgghhfhf.", ".aaabcghhhif.f..", "abbcbbfiiihf....", ".aabcf.fhggf....", "...acf..fff.....", "...aba..........", "....a..........." }) },
            { "fish_carp", new FishPixels(new uint[] { 0x221E19, 0x7C763E, 0x2E2A1E, 0x524D2E, 0xD0763C, 0x000000, 0xB1A063, 0xC2BDB0, 0xB2A590, 0x684F46, 0x866C5E },
                new[] { "........aa.aaaa.", ".......abcadbba.", "......adcddefbaa", "....a.acdbddegha", "...adacdbdbdghia", "...abdcdcddghia.", "..abdcdbdcchia..", "..addcddbdiica..", "...acdbgggiaba..", "....abghica.a...", ".aaadhiccda.....", "adjkiicddba.....", ".aajkcadba......", "...aja.aa.......", "...ada..........", "....a..........." }) },
            { "fish_mackerel", new FishPixels(new uint[] { 0x313054, 0x32354A, 0xE1B85C, 0x5671A0, 0x3B5B97, 0xDEC384, 0x343D69, 0xB1BBC2, 0xCDD4D7, 0x98A5B3, 0x333A3F, 0x7E8CA3, 0x454F57, 0x738AAB },
                new[] { ".......aa..abbb.", "......acdaaeeddb", ".....afegeecadhb", ".....acgeeddchib", ".....aegeddjhiik", "....aageddcehihk", "...acegedfeeghk.", "....agedlgegjhk.", ".....adlfhgjhmjk", ".bbaadlciijmmjhk", "kijdencijmmk.kk.", ".kbbdekkkmjk....", "...bbdk..kck....", "....bjk...k.....", "....kik.........", ".....k.........." }) },
            { "fish_bream", new FishPixels(new uint[] { 0x333A3F, 0x2A2A29, 0x221E19, 0x6C868F, 0x859F9B, 0x684F46, 0x524D2E, 0x422E24, 0xE1B85C, 0x000000, 0xB2A590, 0x7C763E, 0xB1A063, 0xC2BDB0, 0x2E2A1E },
                new[] { "......abb..cccc.", ".....adedbcfgggc", "....adedhfgijggc", "....aedhfgkgiglc", "....bdhfgllkglc.", "...behfglglmnkc.", "..bedhggllmnkoc.", "...bcfglgmnkolc.", "....cgglmnkccmc.", ".ccccglmnkc..c..", "cnllglnkkoc.....", ".ckllgccclc.....", "..cclmc..c......", "...cmkc.........", "....cnc.........", ".....c.........." }) },
            { "fish_salmon", new FishPixels(new uint[] { 0x274848, 0x52242A, 0x3E675B, 0x539170, 0xB14A57, 0xC0625F, 0x70283E, 0x0C0903, 0xB1BBC2, 0x98A5B3, 0x34383B, 0xCD8983 },
                new[] { "............aaa.", ".....bbbb..acda.", "....befegbbchdia", "....bfegeeecdijk", "....begeefeccik.", ".....beeflfejjk.", ".....beflfcaek..", "...bbeflfdcaeb..", "..begefeacaeb...", "...beflfeaeb....", "...kelfeeeb.....", "aakefeeeeb......", "acdeeggggeb.....", ".acdkkeffb......", "..ack.bbb.......", "...aa..........." }) },
            { "fish_golden", new FishPixels(new uint[] { 0x52242A, 0xC2BDB0, 0xC0625F, 0xB2A590, 0xB14A57, 0x70283E, 0xD2E1E6 },
                new[] { "....aaaaa..aaaa.", "...abcdedaadbcca", "..aaaffffecgabea", ".abcdefdbcccgbda", ".aaafffcbdeecdda", "abcdefecefffeea.", "aaafffbeffbcfea.", ".abedfbdfbfeba..", "..affecfdfcfdca.", ".aaaabefcfebaeda", "abbbecbdfcfdaaa.", "adbdbeaaabffca..", ".adbdba..adaeda.", "..adbda..aefaa..", "...adda...ada...", "....aa.....a...." }) },
            { "fish_pike", new FishPixels(new uint[] { 0x274848, 0x333A3F, 0x94B575, 0xDEC384, 0x3E675B, 0x539170, 0xD2E1E6, 0x37241A, 0xCA895A, 0x422E24 },
                new[] { ".....aaaa..aabb.", "....acddcaaefcb.", "...acdcaaeghfcb.", "...adcaeeeegeccb", "...adaeefceefffh", "...acaefaceeefh.", "..adaeecaccaddh.", ".adcaefceaadih..", ".acaefcefcdijh..", "..aafcefcdijih..", "...acecddihhdh..", ".aaceddibh..h...", "aeeefabbcb......", ".aaea.acdb......", "..aea..ab.......", "...a............" }) },
            { "fish_marlin", new FishPixels(new uint[] { 0x313054, 0x3B5B97, 0x5671A0, 0x343D69, 0xE1B85C, 0xDEC384, 0xD2E1E6, 0xB1BBC2, 0xCDD4D7 },
                new[] { "aaaa......aaaaaa", ".abbaa...abcccca", "..aabbaaadddbca.", "....aabbbccddbca", ".....aefagbccdca", ".....ahigebbcdca", "......aabdiecdca", ".......abdhfcdba", "....a..aahebcdba", "...aca...adbbda.", "...aca..abdbfa..", "...abaaacbeia...", "....abdcehaa....", "....abaaaa......", "....aca.........", ".....a.........." }) },
            { "fish_chub", new FishPixels(new uint[] { 0x1B201F, 0x475853, 0x57635D, 0xB1BBC2, 0xD2E1E6, 0x70786C, 0x2B3533, 0x98A5B3 },
                new[] { "...........aaa..", "..........abcaa.", ".........abcdada", "........abeadfdg", "....aa.abcfddddg", "...afcabcddfddg.", "...afgbcdfbghg..", "...acgcfgbghg...", "....abcdfghg....", "....acfddhgg....", ".aaabddhggcg....", "abffhhggcffa....", ".acfcg..aaa.....", "..acbg..........", "...aba..........", "....a..........." }) },
        };

        public static IEnumerable<string> Ids { get { return Fish.Keys; } }

        // Entwickler: Patrick Lupierz – Fischgrafik als Pixel-Canvas (null = unbekannte ID)
        public static G1C_PixelCanvas Get(string id)
        {
            FishPixels fish;
            if (id == null || !Fish.TryGetValue(id, out fish)) return null;
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            for (int y = 0; y < fish.Rows.Length; y++)
                for (int x = 0; x < fish.Rows[y].Length; x++)
                {
                    char ch = fish.Rows[y][x];
                    if (ch == '.') continue;
                    uint rgb = fish.Palette[ch - 'a'];
                    c.Set(x, y, Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb));
                }
            return c;
        }
    }
}
