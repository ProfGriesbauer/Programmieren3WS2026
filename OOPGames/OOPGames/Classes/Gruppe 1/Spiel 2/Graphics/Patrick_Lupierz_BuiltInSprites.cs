using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    /// <summary>
    /// Erzeugt die eingebauten Pixel-Art-Grafiken (Stil: GBA, 16x16 Pixel pro Kachel).
    /// Jede Grafik kann über eine PNG-Datei mit gleicher ID ersetzt werden (siehe G1C_SpriteLibrary).
    ///
    /// Figuren-IDs: "<skin>_<richtung>_<frame>", z.B. "player_left_1".
    ///   skin:     player, dealer_red, dealer_green, dealer_blue, bartender, guest
    ///   richtung: down, up, left, right
    ///   frame:    0 = stehen, 1 = Schritt links, 2 = Schritt rechts
    /// </summary>
    public static class G1C_BuiltInSprites
    {
        private static readonly Color OutlineColor = Hex(0x181820);
        private static readonly Color White = Hex(0xF8F8F8);
        private static readonly Color Felt = Hex(0x1E7A3C);
        private static readonly Color FeltLight = Hex(0x2A9048);
        private static readonly Color Wood = Hex(0x7A4A24);
        private static readonly Color WoodDark = Hex(0x4A2A10);
        private static readonly Color WoodLight = Hex(0xA06838);
        private static readonly Color GoldColor = Hex(0xD8A848);

        private class CharacterPalette
        {
            public Color Hair, Skin, Shirt, Vest, Pants, Shoes;
            public Color? Hat; // nur Spielfigur
        }

        private static readonly Dictionary<string, CharacterPalette> Palettes = new Dictionary<string, CharacterPalette>
        {
            { "player",       new CharacterPalette { Hair = Hex(0xD83030), Skin = Hex(0xF8C898), Shirt = Hex(0x3060C8), Vest = Hex(0x3060C8), Pants = Hex(0x383848), Shoes = Hex(0xD83030) } },
            { "dealer_red",   new CharacterPalette { Hair = Hex(0x2A2A2A), Skin = Hex(0xF0C090), Shirt = White,          Vest = Hex(0xB02030), Pants = Hex(0x202020), Shoes = Hex(0x101010) } },
            { "dealer_green", new CharacterPalette { Hair = Hex(0x8A5020), Skin = Hex(0xE8B080), Shirt = White,          Vest = Hex(0x208040), Pants = Hex(0x202020), Shoes = Hex(0x101010) } },
            { "dealer_blue",  new CharacterPalette { Hair = Hex(0x404040), Skin = Hex(0xC89060), Shirt = White,          Vest = Hex(0x2848A8), Pants = Hex(0x202020), Shoes = Hex(0x101010) } },
            { "bartender",    new CharacterPalette { Hair = Hex(0x7A4A24), Skin = Hex(0xF8C898), Shirt = White,          Vest = Hex(0x202020), Pants = Hex(0x303030), Shoes = Hex(0x101010) } },
            { "guest",        new CharacterPalette { Hair = Hex(0xF0D060), Skin = Hex(0xF8D0A8), Shirt = Hex(0x40A060), Vest = Hex(0x40A060), Pants = Hex(0x6A4A2A), Shoes = Hex(0x402010) } },
        };

        public static BitmapSource Create(string id)
        {
            G1C_PixelCanvas canvas = CreateCanvas(id);
            return canvas == null ? null : canvas.ToBitmap();
        }

        private static G1C_PixelCanvas CreateCanvas(string id)
        {
            switch (id)
            {
                case "tile_floor": return Floor(false);
                case "tile_floor_alt": return Floor(true);
                case "tile_wall": return WallTop();
                case "tile_wall_face": return WallFace();
                case "tile_exit": return ExitMat();
                case "obj_plant": return Plant();
                case "obj_slot_machine": return SlotMachine();
                case "obj_bar": return BarCounter();
                case "obj_roulette_table": return RouletteTable();
                case "obj_blackjack_table": return BlackjackTable();
                case "obj_poker_table": return PokerTable();
                case "roulette_wheel": return RouletteWheel();
                case "card_front": return CardFront();
                case "card_back": return CardBack();
                case "chip": return Chip();
                case "slot_cherry": return SlotCherry();
                case "slot_lemon": return SlotLemon();
                case "slot_bell": return SlotBell();
                case "slot_bar": return SlotBar();
                case "slot_diamond": return SlotDiamond();
                case "slot_seven": return SlotSeven();
            }
            return TryCreateCharacter(id);
        }

        private static Color Hex(uint rgb)
        {
            return Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
        }

        private static Color Darken(Color c, double factor)
        {
            return Color.FromRgb((byte)(c.R * factor), (byte)(c.G * factor), (byte)(c.B * factor));
        }

        // ------------------------------------------------------------------ Kacheln

        private static G1C_PixelCanvas Floor(bool alternate)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            Color baseColor = alternate ? Hex(0x9A2236) : Hex(0xA8283C);
            c.FillRect(0, 0, 16, 16, baseColor);
            // Rautenmuster des Casino-Teppichs
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    int d = Math.Abs(x - 8) + Math.Abs(y - 8);
                    if (d == 7) c.Set(x, y, Darken(baseColor, 0.8));
                }
            }
            c.Set(8, 8, GoldColor);
            c.Set(0, 0, GoldColor);
            return c;
        }

        private static G1C_PixelCanvas WallTop()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, Hex(0x3A2A4A));
            for (int y = 3; y < 16; y += 4)
            {
                c.FillRect(0, y, 16, 1, Hex(0x2A1E38));
                int offset = (y / 4) % 2 == 0 ? 3 : 11;
                c.FillRect(offset, y - 3, 1, 3, Hex(0x2A1E38));
            }
            c.FillRect(0, 0, 16, 1, Hex(0x5A4470));
            return c;
        }

        private static G1C_PixelCanvas WallFace()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, Hex(0xD8B070));
            for (int x = 1; x < 16; x += 4)
            {
                c.FillRect(x, 2, 1, 11, Hex(0xC09858));
            }
            c.FillRect(0, 0, 16, 2, Hex(0x6A4A2A));
            c.FillRect(0, 13, 16, 3, Hex(0x5A3A20));
            c.FillRect(0, 13, 16, 1, Hex(0x7A5A30));
            return c;
        }

        private static G1C_PixelCanvas ExitMat()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 16, Hex(0x2A1E38));
            c.FillRect(1, 2, 14, 12, Hex(0x6A3A1A));
            c.DrawRectBorder(1, 2, 14, 12, GoldColor);
            // Pfeil nach unten
            c.FillRect(7, 4, 2, 5, GoldColor);
            c.FillRect(5, 8, 6, 1, GoldColor);
            c.FillRect(6, 9, 4, 1, GoldColor);
            c.FillRect(7, 10, 2, 1, GoldColor);
            return c;
        }

        // ------------------------------------------------------------------ Möbel

        private static G1C_PixelCanvas Plant()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillEllipse(8, 6, 6, 5.5, Hex(0x38A048));
            c.FillEllipse(5.5, 4.5, 2.5, 2, Hex(0x60C870));
            c.FillEllipse(10.5, 7, 2, 1.5, Hex(0x60C870));
            c.FillRect(4, 10, 8, 6, Hex(0xB06030));
            c.FillRect(3, 10, 10, 2, Hex(0xC87840));
            c.AddOutline(OutlineColor);
            return c;
        }

        private static G1C_PixelCanvas SlotMachine()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 24);
            c.FillRect(1, 3, 14, 21, Hex(0xC83030));
            c.FillEllipse(8, 3.5, 7, 3, Hex(0xE8C040));
            c.FillRect(3, 6, 10, 7, White);
            c.FillRect(4, 8, 2, 3, Hex(0xD02020));
            c.FillRect(7, 8, 2, 3, Hex(0xE8C040));
            c.FillRect(10, 8, 2, 3, Hex(0x2848C8));
            c.FillRect(3, 15, 10, 3, Hex(0x303030));
            c.Set(5, 16, Hex(0x40E040));
            c.Set(10, 16, Hex(0xE04040));
            c.FillRect(1, 20, 14, 4, Hex(0x802020));
            c.FillRect(14, 7, 1, 6, Hex(0xA0A0A0));
            c.FillRect(13, 6, 2, 2, Hex(0xE02020));
            c.AddOutline(OutlineColor);
            return c;
        }

        private static G1C_PixelCanvas BarCounter()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(0, 0, 16, 5, Hex(0x8A5A30));
            c.FillRect(0, 0, 16, 1, Hex(0xB07840));
            c.FillRect(0, 5, 16, 11, Hex(0x5A3418));
            c.FillRect(0, 5, 1, 11, Hex(0x4A2810));
            c.FillRect(8, 5, 1, 11, Hex(0x4A2810));
            c.FillRect(0, 5, 16, 1, Hex(0x3A2008));
            c.FillRect(5, 0, 2, 3, Hex(0xA8E0F0));
            c.Set(5, 0, White);
            return c;
        }

        private static G1C_PixelCanvas RouletteTable()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(48, 32);
            c.FillRect(1, 2, 46, 28, Wood);
            c.FillRect(1, 2, 46, 1, WoodLight);
            c.FillRect(3, 4, 42, 24, Felt);
            // kleines Rad links
            c.FillEllipse(12, 16, 9, 9, WoodDark);
            c.FillEllipse(12, 16, 7, 7, Hex(0xC82028));
            for (int i = 0; i < 360; i += 30)
            {
                double a = i * Math.PI / 180.0;
                c.FillRect((int)(12 + Math.Cos(a) * 5), (int)(16 + Math.Sin(a) * 5), 2, 2, Hex(0x202020));
            }
            c.FillEllipse(12, 16, 3, 3, GoldColor);
            // Zahlenfeld rechts
            for (int i = 0; i < 18; i++)
            {
                int x = 24 + (i % 6) * 3;
                int y = 8 + (i / 6) * 5;
                c.FillRect(x, y, 3, 5, i % 2 == 0 ? Hex(0xC82028) : Hex(0x202020));
            }
            c.DrawRectBorder(23, 7, 20, 17, White);
            c.AddOutline(OutlineColor);
            return c;
        }

        private static G1C_PixelCanvas BlackjackTable()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(48, 32);
            c.FillRect(1, 2, 46, 12, Wood);
            c.FillEllipse(24, 13, 23, 18, Wood);
            c.FillRect(3, 4, 42, 10, Felt);
            c.FillEllipse(24, 13, 20.5, 15.5, Felt);
            c.FillRect(1, 2, 46, 1, WoodLight);
            // Chip-Ablage des Dealers
            c.FillRect(17, 4, 14, 3, WoodDark);
            for (int i = 0; i < 6; i++)
            {
                c.FillRect(18 + i * 2, 5, 1, 1, i % 2 == 0 ? Hex(0xE04040) : Hex(0x4040E0));
            }
            // Kartenfelder im Bogen
            for (int i = 0; i < 5; i++)
            {
                double a = (200 + i * 35) * Math.PI / 180.0;
                int x = (int)(24 + Math.Cos(a) * 13);
                int y = (int)(13 - Math.Sin(a) * 9);
                c.DrawRectBorder(x - 1, y, 4, 5, FeltLight);
            }
            c.AddOutline(OutlineColor);
            return c;
        }

        private static G1C_PixelCanvas PokerTable()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(48, 32);
            c.FillEllipse(24, 16, 23.5, 15, Wood);
            c.FillEllipse(24, 16, 20.5, 12, Hex(0x1E6A5A));
            c.FillEllipse(24, 16, 17, 9, Hex(0x247868));
            for (int i = 0; i < 5; i++)
            {
                c.FillRect(14 + i * 4, 14, 3, 5, White);
            }
            c.FillEllipse(9, 16, 2, 2, Hex(0xE04040));
            c.FillEllipse(39, 16, 2, 2, Hex(0x4040E0));
            c.AddOutline(OutlineColor);
            return c;
        }

        // ------------------------------------------------------------------ Minispiel-Grafiken

        /// <summary>
        /// Europäisches Roulette-Rad: 37 Taschen in der echten Reihenfolge.
        /// Tasche 0 liegt oben, die Reihenfolge läuft im Uhrzeigersinn.
        /// </summary>
        private static G1C_PixelCanvas RouletteWheel()
        {
            const int size = 64;
            G1C_PixelCanvas c = new G1C_PixelCanvas(size, size);
            double center = size / 2.0;
            int pockets = G1C_RouletteTable.WheelOrder.Length;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    double dx = x + 0.5 - center;
                    double dy = y + 0.5 - center;
                    double r = Math.Sqrt(dx * dx + dy * dy);
                    if (r > 31.5) continue;

                    Color color;
                    if (r > 28.5)
                    {
                        color = WoodDark;
                    }
                    else if (r > 19)
                    {
                        // Winkel ab "oben" im Uhrzeigersinn in [0, 1)
                        double angle = Math.Atan2(dx, -dy);
                        if (angle < 0) angle += 2 * Math.PI;
                        double pos = angle / (2 * Math.PI) * pockets;
                        int index = (int)pos % pockets;
                        double fraction = pos - Math.Floor(pos);
                        int number = G1C_RouletteTable.WheelOrder[index];

                        if (fraction < 0.1 || r > 27.5)
                            color = GoldColor;
                        else if (number == 0)
                            color = Hex(0x189040);
                        else if (G1C_RouletteTable.IsRed(number))
                            color = Hex(0xC82028);
                        else
                            color = Hex(0x202024);
                    }
                    else if (r > 17)
                    {
                        color = GoldColor;
                    }
                    else if (r > 5)
                    {
                        color = (Math.Abs(dx) < 1 || Math.Abs(dy) < 1) ? GoldColor : Wood;
                    }
                    else
                    {
                        color = Hex(0xF0D070);
                    }
                    c.Set(x, y, color);
                }
            }
            return c;
        }

        private static G1C_PixelCanvas CardFront()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(20, 28);
            c.FillRect(0, 0, 20, 28, White);
            c.DrawRectBorder(0, 0, 20, 28, Hex(0x404048));
            return c;
        }

        private static G1C_PixelCanvas CardBack()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(20, 28);
            c.FillRect(0, 0, 20, 28, White);
            c.FillRect(2, 2, 16, 24, Hex(0x2848A8));
            for (int y = 2; y < 26; y++)
            {
                for (int x = 2; x < 18; x++)
                {
                    if ((x + y) % 4 == 0 || (x - y + 40) % 4 == 0) c.Set(x, y, Hex(0x4870D0));
                }
            }
            c.DrawRectBorder(0, 0, 20, 28, Hex(0x404048));
            return c;
        }

        private static G1C_PixelCanvas Chip()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(10, 10);
            c.FillEllipse(5, 5, 5, 5, Hex(0xD03030));
            c.FillRect(4, 0, 2, 2, White);
            c.FillRect(4, 8, 2, 2, White);
            c.FillRect(0, 4, 2, 2, White);
            c.FillRect(8, 4, 2, 2, White);
            c.FillEllipse(5, 5, 2.5, 2.5, White);
            c.FillEllipse(5, 5, 1.5, 1.5, Hex(0xD03030));
            return c;
        }

        private static G1C_PixelCanvas SlotCherry()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            Color stem = Hex(0x308030);
            for (int i = 0; i <= 7; i++)
            {
                c.Set(8 - i * 3 / 7, 2 + i, stem);
                c.Set(8 + i * 3 / 7, 2 + i, stem);
            }
            c.FillEllipse(10.5, 3, 2.5, 1.5, Hex(0x40B040));
            c.FillEllipse(5, 11.5, 3.5, 3.5, Hex(0xD82020));
            c.FillEllipse(11, 11.5, 3.5, 3.5, Hex(0xD82020));
            c.Set(4, 10, White);
            c.Set(10, 10, White);
            c.AddOutline(OutlineColor);
            return c;
        }

        private static G1C_PixelCanvas SlotLemon()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillEllipse(8, 8, 6.5, 5, Hex(0xF0D830));
            c.FillRect(1, 7, 2, 2, Hex(0xF0D830));
            c.FillRect(13, 7, 2, 2, Hex(0xF0D830));
            c.FillEllipse(6, 6, 2, 1.2, Hex(0xFFF8A0));
            c.AddOutline(OutlineColor);
            return c;
        }

        private static G1C_PixelCanvas SlotBell()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillEllipse(8, 6.5, 5, 5, Hex(0xE8B830));
            c.FillRect(3, 7, 10, 5, Hex(0xE8B830));
            c.FillRect(2, 11, 12, 2, Hex(0xC08820));
            c.FillEllipse(8, 13.5, 1.6, 1.6, Hex(0x604010));
            c.FillRect(5, 4, 1, 5, Hex(0xFFE890));
            c.AddOutline(OutlineColor);
            return c;
        }

        private static G1C_PixelCanvas SlotBar()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            c.FillRect(1, 4, 14, 8, Hex(0x202020));
            c.FillRect(3, 6, 10, 1, White);
            c.FillRect(3, 9, 10, 1, White);
            c.AddOutline(GoldColor);
            return c;
        }

        private static G1C_PixelCanvas SlotDiamond()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    double d = Math.Abs(x - 7.5) + Math.Abs(y - 7.5) * 1.1;
                    if (d <= 7)
                    {
                        c.Set(x, y, (x < 8 && y < 8) ? Hex(0xA0F0FF) : Hex(0x40C0E8));
                    }
                }
            }
            c.AddOutline(OutlineColor);
            return c;
        }

        private static G1C_PixelCanvas SlotSeven()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            Color red = Hex(0xE02020);
            c.FillRect(3, 2, 10, 3, red);
            for (int y = 5; y < 15; y++)
            {
                int x = (int)(12 - (y - 5) * 0.6);
                c.FillRect(x - 1, y, 3, 1, red);
            }
            c.AddOutline(OutlineColor);
            return c;
        }

        // ------------------------------------------------------------------ Figuren

        private static G1C_PixelCanvas TryCreateCharacter(string id)
        {
            // Format: <skin>_<richtung>_<frame>; der Skin darf selbst Unterstriche enthalten
            string[] parts = id.Split('_');
            if (parts.Length < 3) return null;

            int frame;
            if (!int.TryParse(parts[parts.Length - 1], out frame) || frame < 0 || frame > 2) return null;

            string direction = parts[parts.Length - 2];
            string skin = string.Join("_", parts, 0, parts.Length - 2);

            CharacterPalette palette = ParseCustomPalette(skin);
            if (palette == null && !Palettes.TryGetValue(skin, out palette)) return null;

            G1C_PixelCanvas c;
            switch (direction)
            {
                case "down": c = CharacterFront(palette, frame, false); break;
                case "up": c = CharacterFront(palette, frame, true); break;
                case "right": c = CharacterSide(palette, frame); break;
                case "left": c = CharacterSide(palette, frame).MirrorX(); break;
                default: return null;
            }
            return c;
        }

        // Skin der anpassbaren Spielfigur: pc-Haar-Haut-Oberteil-Hose-Schuhe-Mütze (siehe G1C_PlayerProfile.SkinId)
        private static CharacterPalette ParseCustomPalette(string skin)
        {
            string[] parts = skin.Split('-');
            if (parts.Length != 7 || parts[0] != "pc") return null;

            uint[] colors = new uint[6];
            for (int i = 0; i < 6; i++)
            {
                bool noHat = i == 5 && parts[6] == "x";
                if (!noHat && !uint.TryParse(parts[i + 1], System.Globalization.NumberStyles.HexNumber, null, out colors[i])) return null;
            }
            return new CharacterPalette
            {
                Hair = Hex(colors[0]), Skin = Hex(colors[1]), Shirt = Hex(colors[2]), Vest = Hex(colors[2]),
                Pants = Hex(colors[3]), Shoes = Hex(colors[4]), Hat = parts[6] == "x" ? (Color?)null : Hex(colors[5])
            };
        }

        // Ansicht von vorne (down) oder von hinten (up)
        private static G1C_PixelCanvas CharacterFront(CharacterPalette p, int frame, bool back)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            Color eye = Hex(0x202028);

            // Kopf
            c.FillRect(4, 1, 8, 7, p.Skin);
            if (back)
            {
                c.FillRect(4, 0, 8, 7, p.Hair);
                c.FillRect(3, 1, 1, 5, p.Hair);
                c.FillRect(12, 1, 1, 5, p.Hair);
            }
            else
            {
                c.FillRect(4, 0, 8, 3, p.Hair);
                c.FillRect(3, 1, 1, 4, p.Hair);
                c.FillRect(12, 1, 1, 4, p.Hair);
                c.FillRect(6, 4, 1, 2, eye);
                c.FillRect(9, 4, 1, 2, eye);
            }

            if (p.Hat.HasValue)
            {
                c.FillRect(4, 0, 8, back ? 3 : 2, p.Hat.Value);
                if (!back) c.FillRect(3, 2, 10, 1, Darken(p.Hat.Value, 0.7)); // Schirm
            }

            // Oberkörper mit Weste
            c.FillRect(4, 8, 8, 5, p.Shirt);
            if (!back)
            {
                c.FillRect(4, 8, 2, 5, p.Vest);
                c.FillRect(10, 8, 2, 5, p.Vest);
            }
            else
            {
                c.FillRect(4, 8, 8, 5, p.Vest == p.Shirt ? p.Shirt : p.Vest);
            }

            // Arme schwingen beim Laufen leicht mit
            int leftHand = frame == 1 ? 13 : 12;
            int rightHand = frame == 2 ? 13 : 12;
            c.FillRect(3, 8, 1, leftHand - 8, Darken(p.Shirt, 0.85));
            c.FillRect(12, 8, 1, rightHand - 8, Darken(p.Shirt, 0.85));
            c.Set(3, leftHand, p.Skin);
            c.Set(12, rightHand, p.Skin);

            // Beine: Frame 1/2 heben jeweils einen Fuß an
            int leftLeg = frame == 2 ? 1 : 2;
            int rightLeg = frame == 1 ? 1 : 2;
            c.FillRect(5, 13, 2, leftLeg, p.Pants);
            c.FillRect(9, 13, 2, rightLeg, p.Pants);
            c.FillRect(5, 13 + leftLeg, 2, 1, p.Shoes);
            c.FillRect(9, 13 + rightLeg, 2, 1, p.Shoes);

            c.AddOutline(OutlineColor);
            return c;
        }

        // Seitenansicht nach rechts (links = gespiegelt)
        private static G1C_PixelCanvas CharacterSide(CharacterPalette p, int frame)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);

            // Kopf
            c.FillRect(5, 1, 7, 7, p.Skin);
            c.FillRect(5, 0, 7, 3, p.Hair);
            c.FillRect(5, 3, 3, 4, p.Hair);
            c.FillRect(10, 4, 1, 2, Hex(0x202028));

            if (p.Hat.HasValue)
            {
                c.FillRect(5, 0, 7, 2, p.Hat.Value);
                c.FillRect(10, 2, 3, 1, Darken(p.Hat.Value, 0.7));
            }

            // Körper
            c.FillRect(6, 8, 5, 5, p.Shirt);
            c.FillRect(10, 8, 1, 5, p.Vest);

            // Arm
            int handX = frame == 1 ? 7 : (frame == 2 ? 9 : 8);
            c.FillRect(8, 9, 2, 3, Darken(p.Shirt, 0.85));
            c.Set(handX, 12, p.Skin);

            // Beine
            if (frame == 0)
            {
                c.FillRect(7, 13, 3, 2, p.Pants);
                c.FillRect(7, 15, 4, 1, p.Shoes);
            }
            else if (frame == 1)
            {
                c.FillRect(6, 13, 2, 2, p.Pants);
                c.FillRect(9, 13, 2, 2, p.Pants);
                c.FillRect(5, 15, 3, 1, p.Shoes);
                c.FillRect(10, 15, 2, 1, p.Shoes);
            }
            else
            {
                c.FillRect(7, 13, 2, 2, p.Pants);
                c.FillRect(9, 13, 2, 1, p.Pants);
                c.FillRect(7, 15, 2, 1, p.Shoes);
                c.FillRect(10, 14, 2, 1, p.Shoes);
            }

            c.AddOutline(OutlineColor);
            return c;
        }
    }
}
