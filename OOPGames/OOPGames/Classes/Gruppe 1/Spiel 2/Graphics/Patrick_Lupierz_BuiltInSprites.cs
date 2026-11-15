using System;
using System.Collections.Generic;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    /// <summary>
    /// Erzeugt die eingebauten Pixel-Art-Grafiken (16x16 Pixel pro Kachel, 24x24 pro Figur).
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
                case "obj_claw_machine": return ClawMachine();
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
            if (id.StartsWith("item_", StringComparison.Ordinal)) return ItemIcon(id.Substring(5));
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

        private static G1C_PixelCanvas ClawMachine()
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(32, 40);
            c.FillRect(2, 1, 28, 38, Hex(0x684898));
            c.FillRect(4, 3, 24, 5, Hex(0xF0C848));
            c.FillRect(5, 4, 22, 3, Hex(0x382050));
            c.FillRect(5, 10, 22, 20, Hex(0xA8D8E8));
            c.DrawRectBorder(5, 10, 22, 20, Hex(0x302840));
            c.FillRect(6, 27, 20, 2, Hex(0x805838));
            c.FillRect(15, 11, 2, 6, Hex(0x606878));
            c.FillRect(12, 16, 8, 2, Hex(0x606878));
            c.FillRect(12, 17, 2, 4, Hex(0xB8C0C8));
            c.FillRect(18, 17, 2, 4, Hex(0xB8C0C8));
            c.FillEllipse(9, 25, 3, 2, Hex(0xD85848));
            c.FillEllipse(16, 24, 3, 2, Hex(0xE8C848));
            c.FillEllipse(23, 25, 3, 2, Hex(0x58A868));
            c.FillRect(9, 32, 14, 4, Hex(0x302840));
            c.FillRect(13, 33, 6, 2, Hex(0x98A0A8));
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

        private static G1C_PixelCanvas ItemIcon(string id)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(16, 16);
            G1C_Item item = G1C_ItemCatalog.Get(id);
            // Änderung durch: Patrick Lupierz – 15.11.2026 – Fahrräder nutzen dieselbe Grafik wie in der Welt
            if (item != null && item.Category == G1C_ItemCategory.Fahrzeug) return G1C_DecorSprites.BikeSide(id);
            if (item != null && item.Slot != G1C_ClothingSlot.None)
            {
                Color clothing = Color.FromRgb((byte)(item.Color >> 16), (byte)(item.Color >> 8), (byte)item.Color);
                c.FillRect(4, 4, 8, 10, clothing);
                c.FillRect(6, 2, 4, 3, clothing);
                c.FillRect(2, 5, 3, 5, clothing);
                c.FillRect(11, 5, 3, 5, clothing);
            }
            else if (id == "coffee")
            {
                c.FillRect(4, 4, 8, 9, White);
                c.FillRect(5, 5, 6, 4, Hex(0x704020));
                c.FillRect(12, 6, 2, 4, Hex(0xC8D0D8));
                c.FillRect(6, 2, 1, 2, Hex(0xA8E0F0));
                c.FillRect(9, 1, 1, 3, Hex(0xA8E0F0));
            }
            else if (id == "water")
            {
                c.FillRect(5, 3, 6, 2, Hex(0x78C0F0));
                c.FillRect(4, 5, 8, 9, Hex(0x58A8E0));
                c.FillRect(6, 6, 2, 6, Hex(0xC8F0FF));
                c.FillRect(5, 8, 6, 2, White);
            }
            else if (id == "sandwich")
            {
                c.FillRect(3, 5, 10, 7, Hex(0xD8A858));
                c.FillRect(4, 4, 8, 2, Hex(0xF0D090));
                c.FillRect(4, 7, 8, 2, Hex(0x50A848));
                c.FillRect(4, 9, 8, 2, Hex(0xC85038));
            }
            else if (id == "apple")
            {
                c.FillEllipse(8, 9, 6, 5, Hex(0xD83030));
                c.FillRect(8, 2, 1, 3, Hex(0x704820));
                c.FillEllipse(11, 4, 3, 1.5, Hex(0x48A848));
                c.Set(5, 8, White);
            }
            else if (id == "bottle")
            {
                c.FillRect(6, 2, 4, 2, Hex(0xA0D8C8));
                c.FillRect(5, 4, 6, 10, Hex(0x58A888));
                c.FillRect(6, 5, 2, 7, Hex(0xC8F0E0));
            }
            else if (id == "lucky_coin")
            {
                c.FillEllipse(8, 8, 6, 6, GoldColor);
                c.FillEllipse(8, 8, 4, 4, Hex(0xF8E080));
                c.Set(8, 4, Hex(0xD8A848));
                c.Set(8, 11, Hex(0xD8A848));
            }
            else if (id == "furniture_plant")
            {
                c.FillRect(5, 10, 6, 4, Hex(0xA86838));
                c.FillRect(7, 5, 2, 6, Hex(0x388840));
                c.FillRect(4, 5, 4, 2, Hex(0x58B858));
                c.FillRect(8, 3, 4, 2, Hex(0x78D068));
            }
            else if (id == "furniture_chair")
            {
                c.FillRect(4, 4, 8, 2, Hex(0xA06838));
                c.FillRect(4, 6, 2, 4, Hex(0xA06838));
                c.FillRect(4, 10, 8, 2, Hex(0xC08850));
                c.FillRect(5, 12, 2, 3, Hex(0x704820));
                c.FillRect(10, 12, 2, 3, Hex(0x704820));
            }
            else if (id == "furniture_table")
            {
                c.FillRect(3, 6, 10, 3, Hex(0xB07840));
                c.FillRect(4, 9, 2, 5, Hex(0x704820));
                c.FillRect(10, 9, 2, 5, Hex(0x704820));
            }
            else if (id == "furniture_shelf")
            {
                c.FillRect(3, 2, 10, 12, Hex(0x805030));
                c.FillRect(5, 4, 2, 3, Hex(0xD84838));
                c.FillRect(8, 4, 3, 3, Hex(0x3868B8));
                c.FillRect(4, 8, 3, 4, Hex(0x48A050));
                c.FillRect(8, 8, 4, 4, Hex(0xD8B848));
            }
            else if (id == "furniture_bed")
            {
                c.FillRect(2, 5, 12, 7, Hex(0x6088C8));
                c.FillRect(3, 4, 4, 3, White);
                c.FillRect(2, 12, 2, 2, Hex(0x805030));
                c.FillRect(12, 12, 2, 2, Hex(0x805030));
            }
            else
            {
                Color tint = item != null && item.Category == G1C_ItemCategory.Getraenk
                    ? Hex(0x5098D0) : Hex(0xD8A848);
                c.FillRect(4, 4, 8, 9, tint);
                c.FillRect(6, 2, 4, 2, tint);
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
            G1C_PixelCanvas c = new G1C_PixelCanvas(24, 24);
            Color eye = Hex(0x202028);

            // Kopf
            c.FillRect(7, 2, 10, 8, p.Skin);
            c.FillRect(5, 5, 2, 3, p.Skin);
            c.FillRect(17, 5, 2, 3, p.Skin);
            if (back)
            {
                c.FillRect(7, 0, 10, 8, p.Hair);
                c.FillRect(5, 2, 2, 5, p.Hair);
                c.FillRect(17, 2, 2, 5, p.Hair);
            }
            else
            {
                c.FillRect(7, 0, 10, 3, p.Hair);
                c.FillRect(5, 1, 3, 5, p.Hair);
                c.FillRect(16, 1, 3, 4, p.Hair);
                c.FillRect(8, 4, 3, 3, White);
                c.FillRect(14, 4, 3, 3, White);
                c.FillRect(9, 5, 1, 2, eye);
                c.FillRect(15, 5, 1, 2, eye);
                c.Set(12, 7, Darken(p.Skin, 0.8));
                c.FillRect(11, 8, 3, 1, Darken(p.Skin, 0.7));
            }

            if (p.Hat.HasValue)
            {
                c.FillRect(7, 0, 10, back ? 4 : 3, p.Hat.Value);
                if (!back) c.FillRect(5, 3, 14, 1, Darken(p.Hat.Value, 0.7));
            }

            // Oberkörper mit Weste
            c.FillRect(7, 10, 10, 8, p.Shirt);
            if (!back)
            {
                c.FillRect(7, 10, 2, 8, p.Vest);
                c.FillRect(15, 10, 2, 8, p.Vest);
                c.FillRect(10, 10, 4, 2, White);
                c.FillRect(11, 12, 2, 5, GoldColor);
                c.Set(9, 15, Darken(p.Vest, 0.75));
                c.Set(15, 15, Darken(p.Vest, 0.75));
            }
            else
            {
                c.FillRect(7, 10, 10, 8, p.Vest == p.Shirt ? p.Shirt : p.Vest);
            }

            // Arme schwingen beim Laufen leicht mit
            int leftHand = frame == 1 ? 18 : 17;
            int rightHand = frame == 2 ? 18 : 17;
            c.FillRect(4, 11, 3, 5, Darken(p.Shirt, 0.85));
            c.FillRect(17, 11, 3, 5, Darken(p.Shirt, 0.85));
            c.FillRect(5, leftHand, 2, 2, p.Skin);
            c.FillRect(17, rightHand, 2, 2, p.Skin);

            // Beine: Frame 1/2 heben jeweils einen Fuß an
            int leftLeg = frame == 2 ? 3 : 4;
            int rightLeg = frame == 1 ? 3 : 4;
            c.FillRect(8, 18, 3, leftLeg, p.Pants);
            c.FillRect(13, 18, 3, rightLeg, p.Pants);
            c.FillRect(7, 18 + leftLeg, 4, 2, p.Shoes);
            c.FillRect(13, 18 + rightLeg, 4, 2, p.Shoes);

            c.AddOutline(OutlineColor);
            return c;
        }

        // Seitenansicht nach rechts (links = gespiegelt)
        private static G1C_PixelCanvas CharacterSide(CharacterPalette p, int frame)
        {
            G1C_PixelCanvas c = new G1C_PixelCanvas(24, 24);

            // Kopf
            c.FillRect(9, 2, 9, 8, p.Skin);
            c.FillRect(7, 5, 2, 3, p.Skin);
            c.FillRect(7, 0, 11, 3, p.Hair);
            c.FillRect(7, 2, 4, 5, p.Hair);
            c.FillRect(14, 4, 2, 3, White);
            c.Set(15, 5, Hex(0x202028));
            c.Set(12, 8, Darken(p.Skin, 0.7));

            if (p.Hat.HasValue)
            {
                c.FillRect(7, 0, 11, 3, p.Hat.Value);
                c.FillRect(14, 3, 5, 1, Darken(p.Hat.Value, 0.7));
            }

            // Körper
            c.FillRect(9, 10, 8, 8, p.Shirt);
            c.FillRect(15, 10, 2, 8, p.Vest);
            c.FillRect(11, 10, 4, 2, White);
            c.FillRect(12, 12, 2, 5, GoldColor);

            // Arm
            int handX = frame == 1 ? 10 : (frame == 2 ? 13 : 12);
            c.FillRect(8, 11, 2, 5, Darken(p.Shirt, 0.85));
            c.FillRect(handX, 16, 2, 2, p.Skin);

            // Beine
            if (frame == 0)
            {
                c.FillRect(10, 18, 6, 4, p.Pants);
                c.FillRect(10, 22, 7, 2, p.Shoes);
            }
            else if (frame == 1)
            {
                c.FillRect(9, 18, 3, 4, p.Pants);
                c.FillRect(14, 18, 3, 4, p.Pants);
                c.FillRect(7, 22, 5, 2, p.Shoes);
                c.FillRect(15, 22, 4, 2, p.Shoes);
            }
            else
            {
                c.FillRect(10, 18, 3, 4, p.Pants);
                c.FillRect(14, 18, 3, 3, p.Pants);
                c.FillRect(9, 22, 4, 2, p.Shoes);
                c.FillRect(14, 21, 5, 2, p.Shoes);
            }

            c.AddOutline(OutlineColor);
            return c;
        }
    }
}
