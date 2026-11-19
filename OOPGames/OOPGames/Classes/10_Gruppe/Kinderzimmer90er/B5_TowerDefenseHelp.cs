using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    // In-game manual: explains the game, the interface and the controls with pictures built
    // from the real map and sprite sheets. Opened from the Esc menu or the main menu.
    public sealed partial class B5RoomTowerDefenseGame
    {
        static readonly string[] HelpTitles =
        {
            "SPIELZIEL", "SPIELOBERFLÄCHE", "TÜRME BAUEN", "UPGRADES",
            "GEGNER UND WELLEN", "SCHADENSARTEN", "STEUERUNG", "SPIELSTÄNDE"
        };

        // Picture area on the left side of a help page.
        const double ImageX = 15, ImageY = 35, ImageWidth = 150, ImageHeight = 110;
        const double TextX = 176;

        bool _ShowHelp;
        int _HelpPage;

        public bool HelpVisible { get { return _ShowHelp; } }

        void OpenHelp()
        {
            _ShowHelp = true;
            _HelpPage = 0;
        }

        // Back to where the manual was opened from (the Esc menu stays open underneath).
        void CloseHelp()
        {
            _ShowHelp = false;
        }

        void TurnHelpPage(int direction)
        {
            _HelpPage = Math.Max(0, Math.Min(HelpTitles.Length - 1, _HelpPage + direction));
        }

        void HandleHelpKey(Key key)
        {
            if (key == Key.Escape || key == Key.X || key == Key.Back) CloseHelp();
            else if (B5RoomKeys.IsLeft(key) || B5RoomKeys.IsUp(key)) TurnHelpPage(-1);
            else if (B5RoomKeys.IsRight(key) || B5RoomKeys.IsDown(key)) TurnHelpPage(1);
            else if (key == Key.Enter || key == Key.Z)
            {
                if (_HelpPage == HelpTitles.Length - 1) CloseHelp();
                else TurnHelpPage(1);
            }
        }

        void DrawHelp()
        {
            Surface.Rect(0, 0, DesignWidth, DesignHeight, Color.FromArgb(170, 0, 0, 0));
            DrawPanel(4, 4, 312, 164);
            DrawTitle(HelpTitles[_HelpPage], 9);
            Surface.Text((_HelpPage + 1) + "/" + HelpTitles.Length, 14, 12, 6, TextDim);

            _ClickAreas.Clear();
            switch (_HelpPage)
            {
                case 0: DrawHelpGoal(); break;
                case 1: DrawHelpInterface(); break;
                case 2: DrawHelpBuilding(); break;
                case 3: DrawHelpUpgrades(); break;
                case 4: DrawHelpEnemies(); break;
                case 5: DrawHelpDamageTypes(); break;
                case 6: DrawHelpControls(); break;
                default: DrawHelpSaves(); break;
            }

            // Only the manual's own buttons react while it is open.
            _ClickAreas.Clear();
            AddClickArea(0, 0, DesignWidth, DesignHeight, null, null);
            DrawHelpButton("X", 296, 9, 13, CloseHelp);
            if (_HelpPage > 0) DrawHelpButton("< ZURÜCK", 12, 152, 46, () => TurnHelpPage(-1));
            if (_HelpPage < HelpTitles.Length - 1) DrawHelpButton("WEITER >", 262, 152, 46, () => TurnHelpPage(1));
            else DrawHelpButton("FERTIG", 262, 152, 46, CloseHelp);

            double dotsLeft = 160 - HelpTitles.Length * 5;
            for (int i = 0; i < HelpTitles.Length; i++)
            {
                double x = dotsLeft + i * 10;
                int page = i;
                AddClickArea(x - 2, 152, 10, 10, null, () => _HelpPage = page);
                Surface.Rect(x, 154, 6, 6, Outline);
                Surface.Rect(x + 1, 155, 4, 4, i == _HelpPage ? TitleGold : StoneDark);
            }
        }

        void DrawHelpButton(string label, double x, double y, double width, Action click)
        {
            bool hovered = IsMouseOver(x, y, width, 11);
            AddClickArea(x, y, width, 11, null, click);
            Surface.Rect(x, y, width, 11, Outline);
            Surface.Rect(x + 1, y + 1, width - 2, 9, hovered ? Selection : StoneDark);
            Surface.Rect(x + 1, y + 1, width - 2, 1, hovered ? SelectionEdge : Stone);
            Surface.Text(label, x + (width - label.Length * 2.75) / 2, y + 2, 5, hovered ? TitleGold : TextLight);
        }

        // ---------------- building blocks ----------------

        void BeginHelpImage()
        {
            Surface.Rect(ImageX - 2, ImageY - 2, ImageWidth + 4, ImageHeight + 4, Outline);
            Surface.Rect(ImageX - 1, ImageY - 1, ImageWidth + 2, ImageHeight + 2, StoneLight);
            Surface.PushClip(ImageX, ImageY, ImageWidth, ImageHeight);
        }

        void EndHelpImage()
        {
            Surface.PopClip();
        }

        // Numbered gold marker that links a spot in the picture to a line of the text.
        void Callout(int number, double x, double y)
        {
            Surface.Rect(x + 1, y - 1, 7, 11, Outline);
            Surface.Rect(x - 1, y + 1, 11, 7, Outline);
            Surface.Rect(x + 1, y, 7, 9, SelectionEdge);
            Surface.Rect(x, y + 1, 9, 7, SelectionEdge);
            Surface.Rect(x + 2, y, 5, 1, TitleGold);
            Surface.Text(number.ToString(), x + 3, y + 1, 5, Outline);
        }

        // "#..." = heading, "1  ..." = line with callout number, "   ..." = indented continuation.
        void HelpLines(double x, double y, params string[] lines)
        {
            foreach (string line in lines)
            {
                if (line.StartsWith("#"))
                {
                    ShadowText(line.Substring(1), x, y, 7, TitleGold);
                    y += 11;
                    continue;
                }
                if (line.Length > 2 && char.IsDigit(line[0]) && line[1] == ' ')
                {
                    Callout(line[0] - '0', x, y - 1);
                    ShadowText(line.Substring(1).TrimStart(), x + 13, y, 5, TextLight);
                }
                else if (line.StartsWith("   "))
                    ShadowText(line.TrimStart(), x + 13, y, 5, TextLight);
                else
                    ShadowText(line, x, y, 5, TextLight);
                y += 8;
            }
        }

        void HelpEnemy(string name, double x, double y, double health, bool flip = false)
        {
            bool boss = TowerDefenseSprites.IsBoss(name);
            double barWidth = boss ? 24 : 12;
            double top = boss ? y - 31 : y - 15;
            if (boss)
                Surface.Sprite(TowerDefenseSprites.Boss(name, (_Tick / 6) % 2), x - 16, y - 28, 32, 32, flip);
            else
                Surface.Sprite(TowerDefenseSprites.Enemy(name, (_Tick / 6) % 2), x - 8, y - 13, 16, 16, flip);
            if (health >= 1 && !boss) return;
            Surface.Rect(x - barWidth / 2 - 1, top - 1, barWidth + 2, 3, Outline);
            Surface.Rect(x - barWidth / 2, top, Math.Max(1, Math.Round(barWidth * health)), 1,
                boss ? Color.FromRgb(230, 90, 220) : Color.FromRgb(120, 220, 90));
        }

        void HelpTower(string name, double x, double y, TowerUpgrades upgrades)
        {
            DrawTowerSprite(CreateTower(name), upgrades ?? new TowerUpgrades(), x, y);
        }

        void HelpCircle(double cx, double cy, double radius)
        {
            DrawRangeRing(cx, cy, radius, double.MinValue, double.MaxValue);
        }

        void KeyCap(string label, double x, double y)
        {
            double width = label.Length * 2.75 + 6;
            Surface.Rect(x, y, width, 9, Outline);
            Surface.Rect(x + 1, y, width - 2, 8, StoneHighlight);
            Surface.Rect(x + 1, y + 7, width - 2, 1, Stone);
            Surface.Text(label, x + 3, y, 5, Outline);
        }

        // ---------------- pages ----------------

        // Map 1 near the castle: enemies on the path, towers on pedestals, the castle.
        void DrawHelpGoal()
        {
            TowerDefenseMapDefinition map = TowerDefenseMaps.Maps[0];
            const int cropX = 160, cropY = 8;
            Func<double, double> hx = mx => ImageX + mx - cropX;
            Func<double, double> hy = my => ImageY + my + 2 - cropY;

            BeginHelpImage();
            Surface.Sprite(TowerDefenseSprites.MapRegion(map.BackgroundFile, cropX, cropY, 150, 110), ImageX, ImageY, 150, 110);
            // Pedestals 3 and 5 of map 1 lie next to the last bends before the castle.
            Point fire = map.TowerSpots[2], archer = map.TowerSpots[4];
            Surface.Sprite(TowerDefenseSprites.MainBuilding(0), hx(map.MainBuilding.X) - 32, hy(map.MainBuilding.Y) - 44, 64, 48);
            HelpTower("Fire Mage", hx(fire.X), hy(fire.Y), null);
            HelpEnemy("Goblin", hx(228), hy(36), 1);
            HelpEnemy("Orc", hx(228), hy(58), 0.6);
            HelpEnemy("Goblin", hx(244), hy(90), 0.3);
            HelpTower("Physical Archer", hx(archer.X), hy(archer.Y), null);
            Surface.Sprite(TowerDefenseSprites.DamageIcon(DamageType.Fire), hx(fire.X + 18) - 3, hy(fire.Y - 14) - 3, 6, 6);
            Callout(1, hx(234), hy(26));
            Callout(2, hx(fire.X - 30), hy(fire.Y - 22));
            Callout(3, hx(268), hy(38));
            EndHelpImage();

            HelpLines(TextX, 36,
                "Gegner laufen in Wellen den Weg",
                "entlang zur Burg.",
                "1  Gegner folgen immer dem Erdweg.",
                "2  Türme stehen auf Steinpodesten",
                "   und schießen automatisch auf den",
                "   vordersten Gegner in Reichweite.",
                "3  Die Burg schießt mit. Erreicht ein",
                "   Gegner sie, verliert sie Leben.",
                "   Bei 0 % ist die Welle verloren.",
                "",
                "Jede Karte hat 50 Wellen. Wer alle",
                "schafft, schaltet die nächste",
                "Karte frei.");
        }

        // The real HUD cells and the footer buttons with numbered explanations.
        void DrawHelpInterface()
        {
            const double hudY = 36;
            Surface.Rect(10, hudY - 3, 300, 23, StoneDark);
            DrawHudCell(12, TowerDefenseIcon.Coin, "GOLD", "860", hudY);
            DrawHudCell(71, TowerDefenseIcon.Heart, "LEBEN", "94%", hudY);
            DrawHudCell(130, TowerDefenseIcon.Wave, "WELLE", "12/50", hudY);
            DrawHudCell(189, TowerDefenseIcon.Skull, "FEINDE", "14", hudY);
            Surface.Rect(248, hudY, 60, 17, Outline);
            Surface.Rect(249, hudY + 1, 58, 15, PanelInside);
            for (int skull = 0; skull < 2; skull++) Icon(TowerDefenseIcon.Skull, 251 + skull * 7, hudY + 2, 6);
            Surface.Text("NORMAL", 274, hudY + 1, 5, TextDim);
            Surface.Text("WELLE 12 LÄUFT", 251, hudY + 9, 5, TextLight);
            for (int i = 0; i < 5; i++) Callout(i + 1, 36 + i * 59, hudY + 20);

            HelpLines(14, 72,
                "1  GOLD: zum Bauen und Verbessern",
                "2  LEBEN: Zustand der Burg",
                "3  WELLE: aktuelle von 50 Wellen");
            HelpLines(162, 72,
                "4  FEINDE: Klick oder E zeigt die",
                "   Gegner der Welle",
                "5  Schwierigkeit und letzte Meldung");

            // Footer replica.
            const double footerY = 108;
            Surface.Rect(10, footerY, 300, 11, Color.FromArgb(200, 20, 18, 16));
            Surface.Text("LEERTASTE  WELLE STARTEN   ENTER  BAUEN", 14, footerY + 2, 5, TextDim);
            double right = 308;
            right = FakeButton("MENÜ", right, footerY + 1);
            double menuX = right;
            right = FakeButton("TEMPO x1", right, footerY + 1);
            double speedX = right;
            right = FakeButton("WELLE STARTEN", right, footerY + 1);
            Callout(6, right + 22, footerY + 13);
            Callout(7, speedX + 13, footerY + 13);
            Callout(8, menuX + 10, footerY + 13);

            HelpLines(14, 128,
                "6  WELLE STARTEN (Leertaste): nächste Welle beginnt",
                "7  TEMPO (T): Spielgeschwindigkeit x1, x2 oder x3",
                "8  MENÜ (Esc): Pause, Anleitung, Hauptmenü, Verlassen");
        }

        double FakeButton(string label, double right, double y)
        {
            double width = label.Length * 3 + 8, x = right - width;
            Surface.Rect(x, y, width, 9, Outline);
            Surface.Rect(x + 1, y + 1, width - 2, 7, StoneDark);
            Surface.Rect(x + 1, y + 1, width - 2, 1, Stone);
            Surface.Text(label, x + 4, y + 1, 5, TextLight);
            return x - 3;
        }

        // Map 1 left part: an empty selected pedestal, a tower with its range circle.
        void DrawHelpBuilding()
        {
            TowerDefenseMapDefinition map = TowerDefenseMaps.Maps[0];
            const int cropX = 40, cropY = 2;
            Func<double, double> hx = mx => ImageX + mx - cropX;
            Func<double, double> hy = my => ImageY + my + 2 - cropY;

            BeginHelpImage();
            Surface.Sprite(TowerDefenseSprites.MapRegion(map.BackgroundFile, cropX, cropY, 150, 110), ImageX, ImageY, 150, 110);
            // Pedestal 2 (ice mage with range), 7 (poison archer) and 4 (free, selected) of map 1.
            Point ice = map.TowerSpots[1], poison = map.TowerSpots[6], free = map.TowerSpots[3];
            HelpCircle(hx(ice.X), hy(ice.Y) - 6, TowerDefenseBattle.TowerRange(new IceMage(), new TowerUpgrades()));
            HelpTower("Ice Mage", hx(ice.X), hy(ice.Y), null);
            HelpTower("Poison Archer", hx(poison.X), hy(poison.Y), null);
            double px = hx(free.X), py = hy(free.Y);
            DrawCorner(px - 16, py - 8, 1, 1, Color.FromRgb(120, 220, 240));
            DrawCorner(px + 13, py - 8, -1, 1, Color.FromRgb(120, 220, 240));
            DrawCorner(px - 16, py + 6, 1, -1, Color.FromRgb(120, 220, 240));
            DrawCorner(px + 13, py + 6, -1, -1, Color.FromRgb(120, 220, 240));
            Icon(TowerDefenseIcon.Cursor, px - 8, py - 24, 16);
            Callout(1, px - 22, py + 8);
            Callout(2, hx(ice.X + 18), hy(ice.Y - 32));
            Callout(3, hx(ice.X + 38), hy(ice.Y));
            EndHelpImage();

            HelpLines(TextX, 36,
                "1  Freies Podest: mit Pfeilen oder",
                "   Maus auswählen, Enter/Klick",
                "   öffnet das Baumenü.",
                "2  Gebauter Turm: Enter/Klick öffnet",
                "   Upgrades und Verkaufen.",
                "3  Der Kreis zeigt die Reichweite",
                "   des ausgewählten Turms.",
                "",
                "Acht Turmtypen, jeder mit eigener",
                "Schadensart (siehe Seite 6).",
                "Gebaut wird auch während einer",
                "Welle - solange ein Menü offen",
                "ist, pausiert das Spiel.");
        }

        // A tower at double size with tier badge and upgrade bars.
        void DrawHelpUpgrades()
        {
            BeginHelpImage();
            Surface.Rect(ImageX, ImageY, ImageWidth, ImageHeight, Color.FromRgb(98, 148, 66));
            Surface.Rect(ImageX, ImageY, ImageWidth, 62, Color.FromRgb(118, 176, 196));
            Surface.Rect(ImageX, ImageY + 58, ImageWidth, 4, Color.FromRgb(150, 196, 206));
            double spotX = ImageX + 62, spotY = ImageY + 92;
            Surface.Sprite(TowerDefenseSprites.Pedestal(), spotX - 32, spotY - 48, 64, 64);
            Surface.Sprite(TowerDefenseSprites.Tower("Fire Mage"), spotX - 32, spotY - 86, 64, 96);
            DrawTierBadge(spotX + 20, spotY - 50, 3);

            // Upgrade bars at double size.
            double barX = spotX - 16, barY = spotY + 8;
            Surface.Rect(barX - 2, barY - 2, 36, 14, Outline);
            Surface.Rect(barX, barY, 4 * 4, 2, DamageColor);
            Surface.Rect(barX, barY + 4, 2 * 4, 2, RangeColor);
            Surface.Rect(barX, barY + 8, 3 * 4, 2, SpeedColor);
            Callout(1, spotX + 33, spotY - 52);
            Callout(2, barX + 38, barY);
            EndHelpImage();

            HelpLines(TextX, 36, "Jeder Turm hat vier Upgrade-Wege:");
            Icon(TowerDefenseIcon.Sword, TextX, 47, 8);
            ShadowText("SCHADEN  +25 % je Stufe, max. 8", TextX + 11, 48, 5, TextLight);
            Icon(TowerDefenseIcon.Range, TextX, 57, 8);
            ShadowText("REICHWEITE  +8 % je Stufe, max. 5", TextX + 11, 58, 5, TextLight);
            Icon(TowerDefenseIcon.Speed, TextX, 67, 8);
            ShadowText("TEMPO  +12 % je Stufe, max. 6", TextX + 11, 68, 5, TextLight);
            Icon(TowerDefenseIcon.Star, TextX, 77, 8);
            ShadowText("TURMSTUFE II / III: hebt alle", TextX + 11, 78, 5, TextLight);
            ShadowText("Basiswerte an", TextX + 11, 86, 5, TextLight);
            HelpLines(TextX, 98,
                "1  Wappen: Turmstufe (Silber II,",
                "   Gold III)",
                "2  Balken: Schaden rot, Reichweite",
                "   grün, Tempo gelb",
                "Verkaufen gibt die Hälfte aller",
                "Ausgaben zurück.");
        }

        // Map 2 path start with walking enemies, a boss and the wave preview.
        void DrawHelpEnemies()
        {
            TowerDefenseMapDefinition map = TowerDefenseMaps.Maps[1];
            const int cropX = 0, cropY = 12;
            Func<double, double> hx = mx => ImageX + mx - cropX;
            Func<double, double> hy = my => ImageY + my + 2 - cropY;

            BeginHelpImage();
            Surface.Sprite(TowerDefenseSprites.MapRegion(map.BackgroundFile, cropX, cropY, 150, 110), ImageX, ImageY, 150, 110);
            HelpEnemy("Goblin", hx(14), hy(40), 1);
            HelpEnemy("Goblin Archer", hx(40), hy(40), 0.5);
            ShadowText("+5", hx(56), hy(22), 5, TitleGold);
            HelpEnemy("Orc", hx(100), hy(66), 0.7);
            double dotX = hx(100) + 8, dotY = hy(66) - 16;
            Surface.Rect(dotX, dotY, 2, 2, Color.FromRgb(255, 140, 40));
            Surface.Rect(dotX + 3, dotY, 2, 2, Color.FromRgb(150, 220, 255));
            HelpEnemy("Orc Lord", hx(100), hy(110), 0.9);

            // Small preview window.
            double wx = ImageX + 3, wy = ImageY + 49;
            Surface.Rect(wx, wy, 44, 58, Outline);
            Surface.Rect(wx + 1, wy + 1, 42, 56, PanelInside);
            Surface.Text("WELLE 10", wx + 3, wy + 2, 5, TitleGold);
            Surface.Sprite(TowerDefenseSprites.Enemy("Orc", 0), wx + 3, wy + 11, 14, 14);
            Surface.Sprite(TowerDefenseSprites.Enemy("Goblin", 0), wx + 3, wy + 25, 14, 14);
            Surface.Sprite(TowerDefenseSprites.Boss("Orc Lord", 0), wx + 3, wy + 39, 16, 16);
            Surface.Text("ORC", wx + 20, wy + 15, 5, TextLight);
            Surface.Text("GOBLIN", wx + 18, wy + 29, 5, TextLight);
            Surface.Text("BOSS", wx + 22, wy + 44, 5, Color.FromRgb(230, 110, 200));
            Callout(1, hx(40) - 16, hy(40) - 22);
            Callout(2, dotX + 6, dotY - 4);
            Callout(3, hx(56) + 10, hy(22) - 2);
            Callout(4, hx(100) + 15, hy(110) - 30);
            Callout(5, wx + 46, wy + 2);
            EndHelpImage();

            HelpLines(TextX, 36,
                "Zwischen den Wellen ist Bauphase.",
                "1  Lebensbalken nach dem 1. Treffer",
                "2  Punkte: Brand, Gift, verlangsamt,",
                "   Rüstungsbruch",
                "3  Jeder besiegte Gegner gibt Gold,",
                "   jede geschaffte Welle einen Bonus",
                "4  Welle 5, 15 ... 45: Miniboss",
                "   Welle 10, 20 ... 50: Boss",
                "   - immer als letzter Gegner",
                "5  Vorschau (E / Klick auf FEINDE):",
                "   welche Gegner kommen",
                "Gegner werden jede Welle stärker.");
        }

        // The six damage types with their towers, and two example enemies with weaknesses.
        void DrawHelpDamageTypes()
        {
            string[] names = { "PHYSISCH", "FEUER", "EIS", "ERDE", "STURM", "GIFT" };
            string[] towers =
            {
                "Physical Archer", "Fire Mage, Flamethrower: Brand", "Ice Mage, Ice Area: langsamer",
                "Earth Mage: Rüstungsbruch", "Strom Mage: Kettenblitz", "Poison Archer: Gift"
            };
            for (int i = 0; i < names.Length; i++)
            {
                double x = 14 + (i % 2) * 150, y = 36 + (i / 2) * 17;
                Icon(TowerDefenseSprites.DamageIcon((DamageType)i), x, y, 16);
                ShadowText(names[i], x + 19, y + 1, 6, TitleGold);
                ShadowText(towers[i].ToUpperInvariant(), x + 19, y + 9, 5, TextLight);
            }

            DrawWeaknessCard("Skeleton", 14, 90);
            DrawWeaknessCard("Stone Golem", 14, 116);
            HelpLines(TextX, 92,
                "SCHWÄCHE: +50 % Schaden",
                "RESISTENZ: -50 % Schaden",
                "RÜSTUNG senkt Treffer prozentual;",
                "Brand und Gift ignorieren sie.",
                "Gemischte Türme lohnen sich!");
        }

        void DrawWeaknessCard(string enemyName, double x, double y)
        {
            Enemy enemy = TowerDefenseWaves.CreateEnemy(enemyName, 1);
            Surface.Rect(x, y, 156, 24, Outline);
            Surface.Rect(x + 1, y + 1, 154, 22, PanelInsideDark);
            Surface.Sprite(TowerDefenseSprites.Enemy(enemyName, 0), x + 3, y + 4, 16, 16);
            ShadowText(enemyName.ToUpperInvariant(), x + 22, y + 2, 5, TitleGold);
            ShadowText("SCHWACH", x + 22, y + 12, 5, Color.FromRgb(120, 220, 90));
            double ix = x + 52;
            foreach (DamageType type in enemy.WeakAgainst) { Icon(TowerDefenseSprites.DamageIcon(type), ix, y + 11, 9); ix += 10; }
            ShadowText("RESIST.", x + 92, y + 12, 5, Color.FromRgb(222, 96, 64));
            ix = x + 122;
            foreach (DamageType type in enemy.ResistantDamageTypes) { Icon(TowerDefenseSprites.DamageIcon(type), ix, y + 11, 9); ix += 10; }
        }

        // Keyboard and mouse side by side.
        void DrawHelpControls()
        {
            ShadowText("TASTATUR", 14, 34, 7, TitleGold);
            string[][] keys =
            {
                new[] { "PFEILE / WASD", "auswählen, Podest wechseln" },
                new[] { "ENTER / Z", "bestätigen, bauen, kaufen" },
                new[] { "BACKSPACE", "zurück, Menü schließen" },
                new[] { "LEERTASTE", "Welle starten" },
                new[] { "T", "Tempo x1 / x2 / x3" },
                new[] { "E", "Gegnervorschau" },
                new[] { "ENTF", "Spielstand löschen" },
                new[] { "ESC / X", "Menü mit Pause" }
            };
            for (int i = 0; i < keys.Length; i++)
            {
                double y = 47 + i * 12;
                KeyCap(keys[i][0], 14, y);
                ShadowText(keys[i][1].ToUpperInvariant(), 66, y + 1, 5, TextLight);
            }

            ShadowText("MAUS", 176, 34, 7, TitleGold);
            DrawMouseIcon(178, 48);
            string[] mouse =
            {
                "ÜBERFAHREN: auswählen",
                "LINKSKLICK: bestätigen, bauen,",
                "  kaufen, Karte starten",
                "RECHTSKLICK: oberstes Menü",
                "  schließen",
                "KLICK AUF FEINDE: Vorschau",
                "BUTTONS UNTEN: Welle starten,",
                "  Tempo, Menü, Zurück",
                "KLICK NEBEN EIN MENÜ: schließen"
            };
            for (int i = 0; i < mouse.Length; i++)
                ShadowText(mouse[i].ToUpperInvariant(), 200, 48 + i * 10, 5, TextLight);
        }

        void DrawMouseIcon(double x, double y)
        {
            Surface.Rect(x, y, 14, 20, Outline);
            Surface.Rect(x + 1, y + 1, 12, 18, StoneHighlight);
            Surface.Rect(x + 1, y + 1, 5, 7, SelectionEdge);
            Surface.Rect(x + 6, y + 1, 1, 8, Outline);
            Surface.Rect(x + 1, y + 8, 12, 1, Outline);
            Surface.Rect(x + 6, y + 3, 2, 3, StoneDark);
            Surface.Rect(x + 2, y + 15, 10, 3, Stone);
        }

        // Slot card, saved badge and wave banner as they appear in the game.
        void DrawHelpSaves()
        {
            BeginHelpImage();
            Surface.Rect(ImageX, ImageY, ImageWidth, ImageHeight, PanelInside);
            double cx = ImageX + 4, cy = ImageY + 6;
            Surface.Rect(cx, cy, 142, 28, SelectionEdge);
            Surface.Rect(cx + 1, cy + 1, 140, 26, Selection);
            ShadowText("1", cx + 5, cy + 6, 14, TitleGold);
            ShadowText("SPIELSTAND 1", cx + 22, cy + 4, 7, TitleGold);
            Surface.Text("KARTEN 2/4 FREI", cx + 22, cy + 14, 5, TextLight);
            Surface.Text("ZULETZT HERBSTWALD - WELLE 12", cx + 22, cy + 20, 5, TextLight);

            double by = ImageY + 42;
            Surface.Rect(cx, by, 104, 11, Outline);
            Surface.Rect(cx + 1, by + 1, 102, 9, Selection);
            Icon(TowerDefenseIcon.Wave, cx + 2, by + 1, 8);
            Surface.Text("GESPEICHERT: WELLE 12", cx + 12, by + 1, 5, TitleGold);

            double wy = ImageY + 62;
            Surface.Rect(cx, wy, 142, 13, Outline);
            Surface.Rect(cx + 1, wy + 1, 140, 11, Selection);
            Surface.Rect(cx + 1, wy + 1, 140, 1, SelectionEdge);
            ShadowText("WELLE 11 GESCHAFFT +150 GOLD", cx + 6, wy + 3, 5, TitleGold);

            double dy = ImageY + 82;
            Surface.Rect(cx, dy, 142, 22, Outline);
            Surface.Rect(cx + 1, dy + 1, 140, 20, PanelInsideDark);
            ShadowText("BURG ZERSTÖRT", cx + 6, dy + 3, 6, TitleGold);
            Surface.Text("WELLE NEU   /   KARTENWAHL", cx + 6, dy + 12, 5, TextLight);
            Callout(1, cx + 128, cy + 2);
            Callout(2, cx + 108, by + 1);
            Callout(3, cx + 128, wy + 2);
            Callout(4, cx + 128, dy + 2);
            EndHelpImage();

            HelpLines(TextX, 36,
                "1  3 unabhängige Spielstände mit",
                "   eigenen freigeschalteten Karten",
                "2  Angefangene Karten lassen sich",
                "   fortsetzen (mit Rückfrage)",
                "3  Gespeichert wird nach jeder",
                "   geschafften Welle: Gold, Türme",
                "   und Upgrades von diesem Moment.",
                "   Wer mitten in einer Welle geht,",
                "   startet sie später neu.",
                "4  Fällt die Burg, startet die",
                "   Welle ab dem letzten Speicherpunkt",
                "   oder es geht zur Kartenwahl.");
        }
    }
}
