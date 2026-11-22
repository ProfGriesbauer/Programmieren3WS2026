using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    internal static class G5_FabrikGebaeudeZeichner
    {
        private const double ZeichenBreite = 160;
        private const double ZeichenHoehe = 130;
        private static readonly Dictionary<string, Geometry> Geometrien =
            new Dictionary<string, Geometry>();

        private static readonly Brush Dunkel = Brush(31, 38, 36);
        private static readonly Brush Stahl = Gradient(137, 145, 138, 64, 73, 68);
        private static readonly Brush StahlSeite = Brush(53, 62, 57);
        private static readonly Brush StahlHell = Brush(169, 176, 166);
        private static readonly Brush Messing = Gradient(228, 202, 121, 112, 82, 35);
        private static readonly Brush Glut = Gradient(255, 240, 161, 164, 61, 33);
        private static readonly Brush Schatten = Brush(23, 27, 26, 150);
        private static readonly Brush Band = Brush(8, 12, 11);
        private static readonly Brush Rahmen = Brush(70, 80, 75);

        internal static Canvas Create(
            G5_FabrikGebaeudeTyp typ,
            int level,
            double width,
            double height)
        {
            Canvas container = new Canvas
            {
                Width = width,
                Height = height,
                ClipToBounds = true,
                IsHitTestVisible = false
            };
            Canvas zeichnung = new Canvas
            {
                Width = ZeichenBreite,
                Height = ZeichenHoehe,
                IsHitTestVisible = false,
                RenderTransform = new ScaleTransform(width / ZeichenBreite, height / ZeichenHoehe),
                RenderTransformOrigin = new Point(0, 0)
            };
            container.Children.Add(zeichnung);

            switch (typ)
            {
                case G5_FabrikGebaeudeTyp.Hub:
                case G5_FabrikGebaeudeTyp.Lager:
                    ZeichneLager(zeichnung);
                    break;
                case G5_FabrikGebaeudeTyp.Foerderer:
                    ZeichneMiner(zeichnung);
                    break;
                case G5_FabrikGebaeudeTyp.Schmelze:
                    ZeichneSchmelze(zeichnung);
                    break;
                case G5_FabrikGebaeudeTyp.Konstruktor:
                    ZeichneKonstruktor(zeichnung);
                    break;
                case G5_FabrikGebaeudeTyp.Assembler:
                    ZeichneAssembler(zeichnung);
                    break;
                case G5_FabrikGebaeudeTyp.Verbrennungsgenerator:
                    ZeichneGenerator(zeichnung);
                    break;
                case G5_FabrikGebaeudeTyp.Strommast:
                    ZeichneStrommast(zeichnung, level);
                    break;
                case G5_FabrikGebaeudeTyp.Laufband:
                    ZeichneLaufband(zeichnung, level);
                    break;
                case G5_FabrikGebaeudeTyp.Splitter:
                    ZeichneWeiche(zeichnung, istSplitter: true, level);
                    break;
                case G5_FabrikGebaeudeTyp.Merger:
                    ZeichneWeiche(zeichnung, istSplitter: false, level);
                    break;
                default:
                    throw new InvalidOperationException("Unbekannter Gebäudetyp in der Darstellung.");
            }

            if (typ != G5_FabrikGebaeudeTyp.Strommast &&
                typ != G5_FabrikGebaeudeTyp.Laufband &&
                typ != G5_FabrikGebaeudeTyp.Splitter &&
                typ != G5_FabrikGebaeudeTyp.Merger)
            {
                ZeichneAusbaustufenAkzent(zeichnung, level);
            }

            return container;
        }

        private static void ZeichneMiner(Canvas canvas)
        {
            EllipseShape(canvas, 29, 82, 102, 35, Schatten);

            PathShape(canvas, "M53 59 39 91l10 6 20-28M108 59l16 32-10 6-21-26",
                null, Dunkel, 15);
            PathShape(canvas, "M53 59 39 91l10 6 20-28M108 59l16 32-10 6-21-26",
                null, Stahl, 8);
            PathShape(canvas, "M36 90l14 6-6 12-15-6zM114 96l14-6 6 12-15 7z",
                Brush(52, 60, 57), Dunkel, 2);
            Line(canvas, "M34 96l12 5M119 101l12-5", Brush(209, 173, 90), 2);

            PathShape(canvas, "M49 48l12-7h38l12 8v33l-12 8H61l-12-8z",
                Dunkel, Brush(26, 32, 31), 3);
            PathShape(canvas, "M49 48l12-7h38l12 8-12 7H61z",
                Stahl, Brush(53, 61, 57), 2);
            PathShape(canvas, "M49 48l12 8v25l-12-7z", Brush(81, 90, 85));
            PathShape(canvas, "M99 56l12-7v30l-12 7z", Brush(37, 44, 42));
            RectangleShape(canvas, 62, 59, 36, 17, Dunkel, Brush(37, 44, 42), 2);
            for (int i = 0; i < 3; i++)
            {
                Line(canvas, "M66 " + (62 + i * 4) + "h28", Brush(179, 170, 139), 1.5);
            }

            PathShape(canvas, "M66 41v-9h31v9", Brush(66, 74, 70), Dunkel, 2);
            RectangleShape(canvas, 72, 33, 19, 6, Brush(119, 127, 119));
            RectangleShape(canvas, 76, 34, 11, 4, Messing);
            Line(canvas, "M68 43v14M95 43v14", Dunkel, 5);
            Line(canvas, "M68 43v12M95 43v12", StahlHell, 2);

            Line(canvas, "M82 57v52", Dunkel, 9);
            Line(canvas, "M82 57v52", Brush(119, 127, 120), 3);
            const string bohrer = "M82 57c-9 2-9 5 0 7s9 5 0 7-9 5 0 7 9 5 0 7-9 5 0 7 9 5 0 7-9 5 0 8";
            PathShape(canvas, bohrer, null, Dunkel, 9);
            PathShape(canvas, bohrer, null, Brush(208, 208, 195), 4);
            Line(canvas, "M82 105v7", Brush(65, 73, 70), 8);
            PathShape(canvas, "M75 109l7 15 7-15z", StahlHell, Dunkel, 2);
            PathShape(canvas, "M79 119l3 7 3-7z", Messing);
            Line(canvas, "M70 56h24M73 54v-5M91 54v-5", Dunkel, 3);
            Bolzen(canvas, 56, 51);
            Bolzen(canvas, 108, 51);
        }

        private static void ZeichneSchmelze(Canvas canvas)
        {
            EllipseShape(canvas, 24, 94, 114, 28, Schatten);
            PathShape(canvas, "M29 53c0-17 23-30 52-30s52 13 52 30v31c0 18-23 32-52 32S29 102 29 84z",
                Brush(60, 69, 65), Dunkel, 4);
            PathShape(canvas, "M37 64c12 10 27 15 44 15s32-5 44-15v18c0 13-19 24-44 24S37 95 37 82z",
                Brush(37, 44, 42));
            PathShape(canvas, "M35 78c13 10 29 15 46 15s33-5 46-15", null, Brush(133, 142, 132), 4);
            PathShape(canvas, "M39 91c12 8 26 12 42 12s30-4 42-12", null, Brush(168, 121, 53), 3);

            EllipseShape(canvas, 29, 24, 104, 58, Stahl, Dunkel, 4);
            EllipseShape(canvas, 38, 30, 86, 45, Messing, Brush(59, 52, 39), 3);
            EllipseShape(canvas, 46, 34, 70, 35, Brush(37, 43, 41), Messing, 2);
            EllipseShape(canvas, 56, 38, 50, 23, Brush(213, 91, 36));
            EllipseShape(canvas, 64, 40, 34, 16, Glut);

            PathShape(canvas, "M30 61h-8v22h9M132 61h8v22h-9",
                null, StahlHell, 7);
            Line(canvas, "M47 111v7M115 111v7", Dunkel, 7);
        }

        private static void ZeichneKonstruktor(Canvas canvas)
        {
            EllipseShape(canvas, 28, 102, 106, 28, Schatten);
            PathShape(canvas,
                "M32 71l25-8 13-11h34l15 9 18 5v10l-18 5-12 10-3 11H68l-5-11-14-6-17-4z",
                Brush(65, 74, 69), Dunkel, 4);
            PathShape(canvas,
                "M32 71l25-8 13-11h34l15 9 18 5-21 5-14-4H75l-15 8z",
                Brush(169, 175, 164), Brush(48, 56, 52), 2.5);
            PathShape(canvas, "M60 72l15-8h27l13 4 21-2",
                null, Brush(224, 224, 211), 2.5);
            PathShape(canvas, "M60 72l9 7 14 5 4 9 10 1 3-9 9-5 13-3",
                null, Brush(127, 137, 129), 3);
            PathShape(canvas, "M66 100l35 1 9 7-53-1z",
                Brush(48, 56, 52), Dunkel, 3);
        }

        private static void ZeichneAssembler(Canvas canvas)
        {
            EllipseShape(canvas, 28, 99, 110, 31, Schatten);
            PathShape(canvas, "M45 83c0-8 17-15 38-15s38 7 38 15v17c0 7-17 13-38 13s-38-6-38-13z",
                Brush(48, 56, 53), Dunkel, 3);
            EllipseShape(canvas, 45, 66, 76, 32, Brush(98, 107, 101), Dunkel, 3);
            EllipseShape(canvas, 54, 68, 58, 23, Brush(150, 152, 139), Brush(52, 59, 55), 2);
            EllipseShape(canvas, 65, 61, 31, 31, Brush(37, 44, 42), Messing, 3);
            EllipseShape(canvas, 72, 67, 17, 18, Brush(99, 107, 101), Brush(216, 187, 111), 2);
            PathShape(canvas, "M74 72c8-3 16 2 17 10", null, Brush(215, 187, 111), 2);

            Line(canvas, "M79 68l13-21", Dunkel, 18);
            Line(canvas, "M79 68l13-21", Stahl, 12);
            Line(canvas, "M80 66l11-18", StahlHell, 3);
            EllipseShape(canvas, 82, 34, 24, 24, Brush(52, 60, 57), Dunkel, 3);
            EllipseShape(canvas, 88, 39, 13, 13, Messing, Brush(41, 47, 44), 2);

            Line(canvas, "M101 42l18-11", Dunkel, 17);
            Line(canvas, "M101 42l18-11", Brush(120, 130, 122), 11);
            Line(canvas, "M103 39l15-9", StahlHell, 2.5);
            EllipseShape(canvas, 113, 22, 16, 16, Brush(52, 60, 57), Dunkel, 3);
            PathShape(canvas, "M125 28c6-5 11-5 15-2l5 4-6 10-10 1-6-7z",
                Brush(102, 113, 106), Dunkel, 3);
            PathShape(canvas, "M138 30l9-7c4-3 8-1 8 3l-2 5-8 5M138 36l9 7c4 3 8 1 8-3l-2-5-8-4",
                null, Dunkel, 8);
            Line(canvas, "M139 30l8-6M139 36l8 6", StahlHell, 3);
        }

        private static void ZeichneLager(Canvas canvas)
        {
            EllipseShape(canvas, 20, 98, 122, 33, Schatten);
            PathShape(canvas, "M36 50c0-14 19-25 43-25s43 11 43 25v37c0 12-19 22-43 22S36 99 36 87z",
                Brush(113, 99, 74), Brush(48, 42, 33), 3);
            EllipseShape(canvas, 36, 24, 86, 50, Brush(179, 154, 103), Brush(69, 61, 47), 3);
            EllipseShape(canvas, 47, 31, 64, 34, Brush(208, 183, 121), Brush(104, 88, 58), 2);
            EllipseShape(canvas, 58, 37, 42, 21, Brush(136, 116, 77), Brush(93, 80, 54), 2);

            PathShape(canvas, "M44 64c12 9 25 13 36 13s24-4 35-13M43 78c11 9 24 13 37 13s26-4 37-13",
                null, Brush(211, 187, 125), 3);
            Line(canvas, "M52 37v53M64 29v70M77 26v77M90 29v70M103 36v54",
                Brush(141, 119, 76), 2);
            PathShape(canvas, "M47 91l11 15h42l13-15",
                Brush(81, 71, 53), Brush(48, 42, 33), 3);
            Line(canvas, "M58 106v7M100 106v7M64 112h30", Brush(155, 133, 87), 4);

            PathShape(canvas, "M108 68v24c0 9 9 16 19 16s19-7 19-16V68",
                Brush(81, 75, 62), Brush(48, 42, 33), 3);
            EllipseShape(canvas, 108, 55, 38, 26, Brush(114, 96, 69), Brush(48, 42, 33), 3);
            EllipseShape(canvas, 111, 58, 32, 19, Brush(179, 154, 103), Brush(93, 80, 54), 2);
            PathShape(canvas, "M112 81c5 4 10 6 15 6s10-2 15-6",
                null, Brush(208, 183, 121), 2);
            Line(canvas, "M118 57v34M127 55v38M136 58v33", Brush(128, 106, 67), 2);
        }

        private static void ZeichneGenerator(Canvas canvas)
        {
            EllipseShape(canvas, 24, 98, 112, 31, Schatten);
            PathShape(canvas, "M35 64c0-15 17-26 42-26s42 11 42 26v28c0 12-17 21-42 21S35 104 35 92z",
                Brush(65, 73, 70), Dunkel, 3);
            PathShape(canvas, "M43 69c0-11 14-19 34-19s34 8 34 19v20c0 9-14 16-34 16S43 98 43 89z",
                Brush(74, 81, 73), Brush(119, 125, 112), 2);
            PathShape(canvas, "M53 73c0-7 10-12 24-12s24 5 24 12v13c0 7-10 12-24 12s-24-5-24-12z",
                Brush(37, 43, 41), Messing, 3);
            PathShape(canvas, "M62 83c-5-7 4-10 7-16 5 5 5 10 10 12 1-6 7-8 10-2 3 6-4 15-14 15-7 0-11-3-13-9z",
                Glut);

            PathShape(canvas, "M47 51c-5-5-5-14 0-18l8 3v15M96 48c7-5 13-4 19 1",
                null, Dunkel, 11);
            PathShape(canvas, "M47 51c-5-5-5-14 0-18l8 3v15M96 48c7-5 13-4 19 1",
                null, Brush(146, 154, 143), 5);
            PathShape(canvas, "M111 49c5-3 9-8 9-14V23c0-6 4-10 9-10h8",
                null, Dunkel, 13);
            PathShape(canvas, "M111 49c5-3 9-8 9-14V23c0-6 4-10 9-10h8",
                null, Stahl, 8);
            EllipseShape(canvas, 131, 7, 20, 12, Brush(52, 58, 55), Dunkel, 3);
            EllipseShape(canvas, 135, 9, 12, 6, Brush(34, 40, 39));
            Line(canvas, "M44 90c8 7 20 11 33 11", Brush(198, 158, 67), 3);
            Line(canvas, "M60 106v7M94 106v7M55 114h44", StahlHell, 4);
        }

        private static void ZeichneStrommast(Canvas canvas, int level)
        {
            EllipseShape(canvas, 30, 100, 102, 26, Schatten);
            EllipseShape(canvas, 43, 86, 74, 28, Brush(53, 60, 57), Dunkel, 3);
            EllipseShape(canvas, 53, 87, 54, 18, Brush(120, 128, 120), Brush(52, 59, 55), 2);
            PathShape(canvas, "M77 91l2-43c0-7 4-11 7-11s6 4 6 11l-2 43z",
                Brush(133, 139, 128), Brush(48, 55, 52), 2);
            PathShape(canvas, "M79 84l11-5 5 3-11 6z", Brush(168, 170, 160));
            Line(canvas, "M83 82V35M83 47L54 33M83 47l29-14M83 60L60 49M83 60l23-11",
                Brush(112, 121, 112), 4);
            Insulator(canvas, 51, 26);
            Insulator(canvas, 105, 26);
            Insulator(canvas, 56, 42);
            Insulator(canvas, 100, 42);
            PathShape(canvas, "M81 37l-4 11h8z", Brush(197, 166, 80));

            if (level >= 2)
            {
                PathShape(canvas, "M44 23c18-15 55-15 74 0",
                    null, LevelAccent(level), level >= 3 ? 3 : 2);
            }

            if (level >= 3)
            {
                PathShape(canvas, "M44 63c20 17 54 17 75 0",
                    null, LevelAccent(level), 3);
            }
        }

        private static void ZeichneLaufband(Canvas canvas, int level)
        {
            EllipseShape(canvas, 10, 87, 140, 23, Schatten);
            RectangleShape(canvas, 15, 43, 130, 43, Dunkel, Brush(17, 22, 21), 3);
            RectangleShape(canvas, 19, 48, 122, 34, Rahmen, Brush(53, 61, 57), 2);
            RectangleShape(canvas, 22, 53, 116, 24, Band);
            RectangleShape(canvas, 22, 47, 116, 5, StahlSeite);
            RectangleShape(canvas, 22, 78, 116, 5, StahlSeite);
            for (int x = 28; x <= 132; x += 16)
            {
                Line(canvas, "M" + x + " 53v24", Brush(48, 57, 53), 3);
            }
            Line(canvas, "M22 49v32M138 49v32", Brush(107, 116, 108), 4);
            Line(canvas, "M38 86v8M80 86v8M122 86v8M31 96h98", Brush(69, 78, 73), 5);
            if (level >= 2)
            {
                Line(canvas, "M22 49v32M138 49v32", LevelAccent(level), 3);
            }
        }

        private static void ZeichneWeiche(Canvas canvas, bool istSplitter, int level)
        {
            EllipseShape(canvas, 14, 94, 132, 25, Schatten);
            string weg = istSplitter
                ? "M80 8V65H8M80 65h72"
                : "M8 65h72V8m0 57h72";
            PathShape(canvas, weg, null, Brush(17, 22, 21), 42);
            PathShape(canvas, weg, null, Rahmen, 34);
            PathShape(canvas, weg, null, Band, 26);
            if (istSplitter)
            {
                Line(canvas, "M67 18h26M67 36h26M67 53h26M20 52v26M40 52v26M120 52v26M140 52v26",
                    Brush(48, 57, 53), 3);
            }
            else
            {
                Line(canvas, "M20 52v26M40 52v26M120 52v26M140 52v26M67 53h26M67 35h26M67 17h26",
                    Brush(48, 57, 53), 3);
            }

            RectangleShape(canvas, 58, 48, 44, 34, Brush(37, 44, 42), Brush(101, 112, 105), 3);
            if (istSplitter)
            {
                PathShape(canvas, "M80 53l15 22H85l-5-7-5 7H65z",
                    Messing, Band, 3);
            }
            else
            {
                PathShape(canvas, "M65 73l15-21 15 21H85l-5-8-5 8z",
                    StahlHell, Band, 3);
            }

            if (level >= 2)
            {
                RectangleShape(canvas, 58, 48, 44, 34, null, LevelAccent(level), 3);
            }
        }

        private static void ZeichneAusbaustufenAkzent(Canvas canvas, int level)
        {
            if (level < 2)
            {
                return;
            }

            Brush akzent = LevelAccent(level);
            PathShape(canvas, "M60 38h40", null, akzent, level >= 3 ? 4 : 3);
            if (level >= 3)
            {
                PathShape(canvas, "M53 44l7-6M107 38l7 6", null, akzent, 3);
            }
        }

        private static void Insulator(Canvas canvas, double x, double y)
        {
            PathShape(canvas,
                "M" + x + " " + y + "c0-3 3-5 7-5s7 2 7 5v4c0 3-3 5-7 5s-7-2-7-5z",
                Messing, Brush(52, 58, 55), 2);
            EllipseShape(canvas, x + 3, y + 1, 4, 4, Brush(216, 200, 120));
        }

        private static void Bolzen(Canvas canvas, double x, double y)
        {
            EllipseShape(canvas, x, y, 5, 5, Brush(198, 196, 172), Brush(43, 50, 49), 1);
            Line(canvas, "M" + (x + 1) + " " + (y + 2) + "h3", Brush(69, 76, 72), 1);
        }

        private static void RectangleShape(
            Canvas canvas,
            double x,
            double y,
            double width,
            double height,
            Brush fill,
            Brush stroke = null,
            double strokeThickness = 1)
        {
            Rectangle rectangle = new Rectangle
            {
                Width = width,
                Height = height,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeThickness
            };
            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            canvas.Children.Add(rectangle);
        }

        private static void EllipseShape(
            Canvas canvas,
            double x,
            double y,
            double width,
            double height,
            Brush fill,
            Brush stroke = null,
            double strokeThickness = 1)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = width,
                Height = height,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeThickness
            };
            Canvas.SetLeft(ellipse, x);
            Canvas.SetTop(ellipse, y);
            canvas.Children.Add(ellipse);
        }

        private static void Line(Canvas canvas, string geometry, Brush stroke, double thickness)
        {
            PathShape(canvas, geometry, null, stroke, thickness);
        }

        private static void PathShape(
            Canvas canvas,
            string geometry,
            Brush fill,
            Brush stroke = null,
            double strokeThickness = 1)
        {
            if (!Geometrien.TryGetValue(geometry, out Geometry parsed))
            {
                parsed = Geometry.Parse(geometry);
                parsed.Freeze();
                Geometrien.Add(geometry, parsed);
            }

            Path path = new Path
            {
                Data = parsed,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeThickness,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false
            };
            canvas.Children.Add(path);
        }

        private static Brush LevelAccent(int level)
        {
            if (level >= 3)
            {
                return Brush(191, 89, 64);
            }

            return Brush(74, 165, 197);
        }

        private static Brush Brush(byte red, byte green, byte blue, byte alpha = 255)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(alpha, red, green, blue));
            brush.Freeze();
            return brush;
        }

        private static Brush Gradient(
            byte topRed,
            byte topGreen,
            byte topBlue,
            byte bottomRed,
            byte bottomGreen,
            byte bottomBlue)
        {
            LinearGradientBrush brush = new LinearGradientBrush(
                Color.FromRgb(topRed, topGreen, topBlue),
                Color.FromRgb(bottomRed, bottomGreen, bottomBlue),
                90);
            brush.Freeze();
            return brush;
        }
    }

}
