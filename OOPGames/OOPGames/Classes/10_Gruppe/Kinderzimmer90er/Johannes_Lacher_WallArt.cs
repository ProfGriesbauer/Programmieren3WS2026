using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    // Painted pictures for the room wall: the arcade poster behind the Atari TV and the evening view
    // through the window. Both are drawn once as vector art and turned into bitmaps (B5RoomTextures).
    static class B5WallArt
    {
        // Poster drawing size; the poster in the room is 2.06 x 0.86 m, so the same 2.4 : 1 ratio.
        const double PosterWidth = 960;
        const double PosterHeight = 400;
        const double PosterHorizon = 250;

        // Window view drawing size; the glass in the room is 1.3 x 0.72 m.
        const double ViewWidth = 650;
        const double ViewHeight = 360;

        // Higher resolution than the other textures: both pictures fill much of the screen in close views.
        const int PictureTextureSize = 1024;

        public static Brush CreateArcadePoster()
        {
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                // Clip to the picture, so glows at the edges do not widen the drawing (it is stretched to its bounds).
                context.PushClip(new RectangleGeometry(new Rect(0, 0, PosterWidth, PosterHeight)));
                Rect sky = new Rect(0, 0, PosterWidth, PosterHorizon);
                Rect ground = new Rect(0, PosterHorizon, PosterWidth, PosterHeight - PosterHorizon);
                context.DrawRectangle(Vertical(Rgb(20, 10, 51), Rgb(75, 21, 99), Rgb(194, 54, 107)), null, sky);
                context.DrawRectangle(Vertical(Rgb(38, 9, 58), Rgb(14, 5, 32)), null, ground);
                DrawStars(context, new Random(7), 80, new Rect(0, 0, PosterWidth, PosterHorizon - 60));
                DrawStripedSun(context, new Point(300, PosterHorizon), 118);
                DrawMountains(context);
                DrawGrid(context);
                DrawRacer(context, new Point(470, 345));
                DrawTitle(context);
                // Thin printed border.
                context.DrawRectangle(null, new Pen(Brush(Color.FromArgb(150, 255, 236, 210)), 3),
                    new Rect(10, 10, PosterWidth - 20, PosterHeight - 20));
            }
            return ToTexture(drawing);
        }

        public static Brush CreateWindowView()
        {
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                context.PushClip(new RectangleGeometry(new Rect(0, 0, ViewWidth, ViewHeight)));
                context.DrawRectangle(Vertical(Rgb(56, 88, 146), Rgb(150, 140, 170), Rgb(236, 166, 108), Rgb(247, 205, 140)),
                    null, new Rect(0, 0, ViewWidth, ViewHeight));

                // Low evening sun with a soft glow.
                Point sun = new Point(455, 222);
                RadialGradientBrush glow = new RadialGradientBrush(Color.FromArgb(170, 255, 236, 180), Color.FromArgb(0, 255, 200, 120));
                context.DrawEllipse(glow, null, sun, 120, 120);
                context.DrawEllipse(Brush(Rgb(255, 243, 200)), null, sun, 30, 30);

                DrawCloud(context, 120, 70, 1.0);
                DrawCloud(context, 380, 110, 0.75);
                DrawCloud(context, 560, 55, 0.6);
                DrawBirds(context);

                // Hills: far hazy, then green.
                context.DrawGeometry(Brush(Rgb(150, 125, 160)), null, Hill(232, new[] { 0, 18, 6, 30, 12, 24, 2, 20 }));
                context.DrawGeometry(Brush(Rgb(108, 132, 100)), null, Hill(258, new[] { 10, 0, 22, 8, 26, 4, 16, 10 }));

                DrawHouses(context);
                DrawTree(context, 78, 300);

                // Lawn and fence in front.
                context.DrawRectangle(Vertical(Rgb(86, 128, 70), Rgb(60, 98, 52)), null, new Rect(0, 315, ViewWidth, ViewHeight - 315));
                DrawFence(context, 330);
            }
            return ToTexture(drawing);
        }

        // Dino poster drawing size; the picture in the room is 1.15 x 0.65 m.
        const double DinoWidth = 800;
        const double DinoHeight = 452;

        // Dinosaur poster: silhouettes of a long-neck dinosaur with its young in front of a smoking volcano
        // at sunset, palm trees and flying pterosaurs.
        public static Brush CreateDinoPoster()
        {
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                context.PushClip(new RectangleGeometry(new Rect(0, 0, DinoWidth, DinoHeight)));
                context.DrawRectangle(Vertical(Rgb(42, 27, 61), Rgb(120, 40, 74), Rgb(222, 92, 58), Rgb(252, 196, 104)),
                    null, new Rect(0, 0, DinoWidth, 340));
                RadialGradientBrush sunGlow = new RadialGradientBrush(Color.FromArgb(150, 255, 230, 150), Color.FromArgb(0, 255, 160, 80));
                context.DrawEllipse(sunGlow, null, new Point(170, 255), 170, 110);
                context.DrawEllipse(Brush(Rgb(255, 226, 140)), null, new Point(170, 255), 50, 50);

                DrawVolcano(context);
                // Distant hills, then the dark jungle floor.
                context.DrawGeometry(Brush(Rgb(122, 52, 76)), null, Wave(300, DinoWidth, new[] { 8, 22, 4, 26, 10, 18 }));
                context.DrawGeometry(Brush(Rgb(78, 32, 62)), null, Wave(330, DinoWidth, new[] { 14, 4, 20, 8, 16, 6 }));
                Brush shadow = Brush(Rgb(30, 18, 38));
                context.DrawRectangle(shadow, null, new Rect(0, 360, DinoWidth, DinoHeight - 360));

                DrawSauropod(context, shadow, new Point(340, 372), 1.0);
                DrawSauropod(context, shadow, new Point(150, 378), 0.45);
                DrawPalm(context, shadow, new Point(600, 380), 1.0);
                DrawPalm(context, shadow, new Point(660, 385), 0.7);
                DrawPalm(context, shadow, new Point(40, 380), 0.85);
                DrawFerns(context, shadow);
                DrawPterosaur(context, shadow, new Point(520, 92), 1.0);
                DrawPterosaur(context, shadow, new Point(600, 130), 0.65);
                DrawPterosaur(context, shadow, new Point(330, 185), 0.5);

                Typeface heavy = new Typeface(new FontFamily("Arial Black, Arial"), FontStyles.Normal, FontWeights.Black, FontStretches.Normal);
                Typeface plain = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
                FormattedText title = Text("DINO-EXPEDITION", heavy, 52, Brushes.White);
                context.DrawGeometry(Brush(Rgb(40, 16, 40)), null, title.BuildGeometry(new Point(36, 30)));
                context.DrawGeometry(Vertical(Rgb(255, 240, 200), Rgb(255, 196, 90)),
                    new Pen(Brush(Rgb(60, 22, 50)), 2.5) { LineJoin = PenLineJoin.Round }, title.BuildGeometry(new Point(32, 26)));
                context.DrawText(Text("ABENTEUER IN DER URZEIT", plain, 20, Brush(Rgb(255, 214, 160))), new Point(36, 96));
                context.DrawText(Text("FILMPOSTER  •  AB 6 JAHREN", plain, 12, Brush(Color.FromArgb(200, 240, 220, 210))),
                    new Point(600, 425));
                context.DrawRectangle(null, new Pen(Brush(Color.FromArgb(140, 255, 230, 200)), 3),
                    new Rect(10, 10, DinoWidth - 20, DinoHeight - 20));
            }
            return ToTexture(drawing);
        }

        static void DrawVolcano(DrawingContext context)
        {
            // Smoke first, so the mountain covers its foot.
            Brush smoke = Brush(Color.FromArgb(150, 90, 60, 80));
            double[,] puffs = { { 655, 150, 30 }, { 640, 115, 38 }, { 675, 80, 44 }, { 650, 40, 50 }, { 700, 20, 40 } };
            for (int i = 0; i < puffs.GetLength(0); i++)
                context.DrawEllipse(smoke, null, new Point(puffs[i, 0], puffs[i, 1]), puffs[i, 2], puffs[i, 2] * 0.8);
            RadialGradientBrush glow = new RadialGradientBrush(Color.FromArgb(190, 255, 150, 60), Color.FromArgb(0, 255, 90, 40));
            context.DrawEllipse(glow, null, new Point(655, 175), 70, 40);
            context.DrawGeometry(Vertical(Rgb(92, 40, 68), Rgb(60, 26, 52)), null,
                Polygon(new Point(0, 0), new[] { 480.0, 340, 620, 178, 690, 178, 820, 340 }));
            // Glowing lava streams.
            Pen lava = new Pen(Vertical(Rgb(255, 214, 90), Rgb(240, 80, 40)), 5) { LineJoin = PenLineJoin.Round, EndLineCap = PenLineCap.Round };
            context.DrawGeometry(null, lava, Polyline(new[] { 640.0, 180, 632, 215, 645, 250, 628, 300 }));
            context.DrawGeometry(null, new Pen(lava.Brush, 3.5) { EndLineCap = PenLineCap.Round },
                Polyline(new[] { 672.0, 180, 684, 222, 676, 262, 696, 310 }));
        }

        // Long-neck dinosaur standing at its feet line (origin), facing right.
        static void DrawSauropod(DrawingContext context, Brush fill, Point feet, double scale)
        {
            context.PushTransform(new TranslateTransform(feet.X, feet.Y));
            context.PushTransform(new ScaleTransform(scale, scale));
            StreamGeometry body = new StreamGeometry();
            using (StreamGeometryContext s = body.Open())
            {
                // Tail tip, along the back, up the neck to the head, then down the throat and belly.
                s.BeginFigure(new Point(-230, -30), true, true);
                s.BezierTo(new Point(-160, -50), new Point(-110, -110), new Point(-30, -112), true, true);
                s.BezierTo(new Point(40, -114), new Point(70, -110), new Point(90, -140), true, true);
                s.BezierTo(new Point(110, -180), new Point(120, -235), new Point(150, -250), true, true);
                s.BezierTo(new Point(165, -258), new Point(190, -256), new Point(196, -244), true, true);
                s.BezierTo(new Point(198, -236), new Point(180, -232), new Point(166, -232), true, true);
                s.BezierTo(new Point(148, -220), new Point(136, -160), new Point(118, -110), true, true);
                s.BezierTo(new Point(104, -72), new Point(80, -50), new Point(60, -44), true, true);
                s.BezierTo(new Point(10, -30), new Point(-60, -30), new Point(-110, -44), true, true);
                s.BezierTo(new Point(-150, -40), new Point(-200, -26), new Point(-230, -30), true, true);
            }
            body.Freeze();
            context.DrawGeometry(fill, null, body);
            // Legs.
            foreach (double x in new[] { -90.0, -60, 30, 56 })
                context.DrawRectangle(fill, null, new Rect(x - 11, -60, 22, 62));
            context.Pop();
            context.Pop();
        }

        static void DrawPalm(DrawingContext context, Brush fill, Point root, double scale)
        {
            context.PushTransform(new TranslateTransform(root.X, root.Y));
            context.PushTransform(new ScaleTransform(scale, scale));
            Point top = new Point(-24, -170);
            context.DrawGeometry(null, new Pen(fill, 9) { StartLineCap = PenLineCap.Round },
                Curve(new Point(0, 0), new Point(10, -70), new Point(-20, -120), top));
            double[] angles = { -170, -135, -95, -55, -15, 20 };
            foreach (double angle in angles)
            {
                double radians = angle * Math.PI / 180;
                Point tip = new Point(top.X + Math.Cos(radians) * 80, top.Y + Math.Sin(radians) * 40 + 34);
                Point middle = new Point(top.X + Math.Cos(radians) * 45, top.Y + Math.Sin(radians) * 40 - 6);
                StreamGeometry leaf = new StreamGeometry();
                using (StreamGeometryContext s = leaf.Open())
                {
                    s.BeginFigure(top, true, true);
                    s.QuadraticBezierTo(new Point(middle.X, middle.Y - 10), tip, true, true);
                    s.QuadraticBezierTo(new Point(middle.X, middle.Y + 8), top, true, true);
                }
                leaf.Freeze();
                context.DrawGeometry(fill, null, leaf);
            }
            context.Pop();
            context.Pop();
        }

        static void DrawFerns(DrawingContext context, Brush fill)
        {
            for (int i = 0; i < 14; i++)
            {
                double x = 20 + i * 57 + (i % 3) * 9;
                double height = 22 + (i * 7) % 18;
                context.DrawGeometry(fill, null, Polygon(new Point(x, 366), new[]
                {
                    -18.0, 0, -12, -height * 0.6, -4, -height * 0.2, 0, -height, 4, -height * 0.2, 12, -height * 0.6, 18, 0
                }));
            }
        }

        static void DrawPterosaur(DrawingContext context, Brush fill, Point center, double scale)
        {
            context.DrawGeometry(fill, null, Polygon(center, new[]
            {
                -46 * scale, -6 * scale, -14 * scale, -2 * scale, -4 * scale, -8 * scale, 12 * scale, -10 * scale,
                22 * scale, -6 * scale, 8 * scale, -2 * scale, 46 * scale, -12 * scale, 10 * scale, 6 * scale, -10 * scale, 4 * scale
            }));
        }

        // Soft wave line over the full width at baseY, closed down to the bottom.
        static Geometry Wave(double baseY, double width, int[] heights)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext stream = geometry.Open())
            {
                double step = width / (heights.Length - 1);
                stream.BeginFigure(new Point(0, baseY + 200), true, true);
                stream.LineTo(new Point(0, baseY - heights[0]), true, true);
                for (int i = 1; i < heights.Length; i++)
                {
                    Point previous = new Point((i - 1) * step, baseY - heights[i - 1]);
                    Point next = new Point(i * step, baseY - heights[i]);
                    stream.BezierTo(new Point(previous.X + step / 2, previous.Y), new Point(next.X - step / 2, next.Y), next, true, true);
                }
                stream.LineTo(new Point(width, baseY + 200), true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        static Geometry Polyline(double[] points)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext stream = geometry.Open())
            {
                stream.BeginFigure(new Point(points[0], points[1]), false, false);
                for (int i = 2; i < points.Length; i += 2)
                    stream.LineTo(new Point(points[i], points[i + 1]), true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        static Geometry Curve(Point start, Point control1, Point control2, Point end)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext stream = geometry.Open())
            {
                stream.BeginFigure(start, false, false);
                stream.BezierTo(control1, control2, end, true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        static void DrawStars(DrawingContext context, Random random, int count, Rect area)
        {
            for (int i = 0; i < count; i++)
            {
                double x = area.X + random.NextDouble() * area.Width;
                double y = area.Y + random.NextDouble() * area.Height;
                double size = 0.7 + random.NextDouble() * 1.6;
                byte alpha = (byte)(120 + random.Next(135));
                context.DrawEllipse(Brush(Color.FromArgb(alpha, 255, 246, 230)), null, new Point(x, y), size, size);
            }
            // A few brighter sparkle stars.
            Pen sparkle = new Pen(Brush(Color.FromArgb(220, 255, 250, 235)), 1.2);
            foreach (Point star in new[] { new Point(90, 50), new Point(610, 38), new Point(860, 120), new Point(200, 150) })
            {
                context.DrawLine(sparkle, new Point(star.X - 6, star.Y), new Point(star.X + 6, star.Y));
                context.DrawLine(sparkle, new Point(star.X, star.Y - 6), new Point(star.X, star.Y + 6));
            }
        }

        // Sunset sun cut by horizontal gaps that grow towards the horizon.
        static void DrawStripedSun(DrawingContext context, Point center, double radius)
        {
            GeometryGroup gaps = new GeometryGroup();
            double y = center.Y - radius * 0.55;
            for (int i = 0; y < center.Y; i++)
            {
                double gap = 2 + i * 1.6;
                gaps.Children.Add(new RectangleGeometry(new Rect(center.X - radius, y, radius * 2, gap)));
                y += gap + 13 - i;
            }
            Geometry disc = new CombinedGeometry(GeometryCombineMode.Intersect,
                new EllipseGeometry(center, radius, radius),
                new RectangleGeometry(new Rect(center.X - radius, center.Y - radius, radius * 2, radius)));
            Geometry sun = new CombinedGeometry(GeometryCombineMode.Exclude, disc, gaps);
            LinearGradientBrush fill = new LinearGradientBrush(Rgb(255, 222, 89), Rgb(255, 64, 129), 90)
            {
                MappingMode = BrushMappingMode.Absolute,
                StartPoint = new Point(0, center.Y - radius),
                EndPoint = new Point(0, center.Y)
            };
            RadialGradientBrush halo = new RadialGradientBrush(Color.FromArgb(110, 255, 120, 150), Color.FromArgb(0, 255, 80, 140));
            context.DrawEllipse(halo, null, new Point(center.X, center.Y - 20), radius * 1.6, radius * 1.3);
            context.DrawGeometry(fill, null, sun);
        }

        static void DrawMountains(DrawingContext context)
        {
            context.DrawGeometry(Brush(Rgb(58, 20, 92)), null, Ridge(PosterHorizon,
                new[] { 0.0, 40, 60, 25, 130, 70, 210, 30, 520, 30, 600, 85, 690, 40, 780, 95, 880, 50, 960, 70 }));
            context.DrawGeometry(Brush(Rgb(33, 10, 60)), null, Ridge(PosterHorizon,
                new[] { 0.0, 15, 90, 45, 160, 12, 560, 10, 650, 50, 730, 20, 830, 55, 960, 25 }));
            // Neon edge along the horizon.
            context.DrawLine(new Pen(Brush(Rgb(255, 90, 200)), 2.5), new Point(0, PosterHorizon), new Point(PosterWidth, PosterHorizon));
        }

        // Ridge from (x, height above the horizon) pairs.
        static Geometry Ridge(double horizon, double[] points)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext stream = geometry.Open())
            {
                stream.BeginFigure(new Point(0, horizon), true, true);
                for (int i = 0; i < points.Length; i += 2)
                    stream.LineTo(new Point(points[i], horizon - points[i + 1]), true, true);
                stream.LineTo(new Point(PosterWidth, horizon), true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        // Perspective neon grid on the ground, vanishing at the middle of the horizon.
        static void DrawGrid(DrawingContext context)
        {
            double vanishX = PosterWidth / 2;
            double depth = PosterHeight - PosterHorizon;
            context.PushClip(new RectangleGeometry(new Rect(0, PosterHorizon, PosterWidth, depth)));
            for (int k = -14; k <= 14; k++)
            {
                Pen pen = new Pen(Brush(Color.FromArgb(170, 255, 80, 215)), 1.6);
                context.DrawLine(pen, new Point(vanishX + k * 22, PosterHorizon), new Point(vanishX + k * 150, PosterHeight));
            }
            for (int row = 1; row <= 9; row++)
            {
                double t = row / 9.0;
                double y = PosterHorizon + depth * t * t;
                Pen pen = new Pen(Brush(Color.FromArgb((byte)(70 + 150 * t), 255, 80, 215)), 0.8 + 1.4 * t);
                context.DrawLine(pen, new Point(0, y), new Point(PosterWidth, y));
            }
            context.Pop();
        }

        // Small racer ship seen from behind, flying along the grid.
        static void DrawRacer(DrawingContext context, Point position)
        {
            RadialGradientBrush exhaust = new RadialGradientBrush(Color.FromArgb(200, 120, 240, 255), Color.FromArgb(0, 60, 160, 255));
            context.DrawEllipse(exhaust, null, new Point(position.X - 20, position.Y + 14), 16, 10);
            context.DrawEllipse(exhaust, null, new Point(position.X + 20, position.Y + 14), 16, 10);
            context.DrawGeometry(Vertical(Rgb(210, 220, 235), Rgb(110, 120, 150)), new Pen(Brush(Rgb(30, 12, 50)), 1.5),
                Polygon(position, new[] { 0.0, -34, 16, -6, 58, 10, 18, 16, -18, 16, -58, 10, -16, -6 }));
            context.DrawGeometry(Brush(Rgb(60, 200, 255)), null,
                Polygon(position, new[] { 0.0, -26, 7, -10, -7, -10 }));
            context.DrawRectangle(Brush(Rgb(255, 70, 120)), null, new Rect(position.X - 52, position.Y + 6, 12, 3));
            context.DrawRectangle(Brush(Rgb(255, 70, 120)), null, new Rect(position.X + 40, position.Y + 6, 12, 3));
        }

        static void DrawTitle(DrawingContext context)
        {
            Typeface heavy = new Typeface(new FontFamily("Arial Black, Arial"), FontStyles.Italic, FontWeights.Black, FontStretches.Normal);
            Typeface plain = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
            DrawOutlinedText(context, "TURBO", heavy, 78, new Point(560, 34));
            DrawOutlinedText(context, "GALAXY", heavy, 78, new Point(585, 118));
            FormattedText subtitle = Text("DAS WELTRAUM-RENNEN", plain, 21, Brush(Rgb(120, 240, 255)));
            context.DrawText(subtitle, new Point(600, 218));
            FormattedText year = Text("1  -  2   SPIELER   •   1989", plain, 13, Brush(Color.FromArgb(220, 255, 220, 240)));
            context.DrawText(year, new Point(640, 356));
        }

        // Chrome-style title: magenta drop shadow, dark outline, yellow to orange fill.
        static void DrawOutlinedText(DrawingContext context, string text, Typeface typeface, double size, Point origin)
        {
            FormattedText formatted = Text(text, typeface, size, Brushes.White);
            Geometry shadow = formatted.BuildGeometry(new Point(origin.X + 6, origin.Y + 6));
            Geometry letters = formatted.BuildGeometry(origin);
            context.DrawGeometry(Brush(Rgb(255, 40, 160)), null, shadow);
            LinearGradientBrush fill = new LinearGradientBrush
            {
                MappingMode = BrushMappingMode.Absolute,
                StartPoint = new Point(0, origin.Y + size * 0.25),
                EndPoint = new Point(0, origin.Y + size * 1.15)
            };
            fill.GradientStops.Add(new GradientStop(Rgb(255, 250, 200), 0));
            fill.GradientStops.Add(new GradientStop(Rgb(255, 214, 70), 0.45));
            fill.GradientStops.Add(new GradientStop(Rgb(255, 120, 40), 0.5));
            fill.GradientStops.Add(new GradientStop(Rgb(255, 70, 90), 1));
            context.DrawGeometry(fill, new Pen(Brush(Rgb(40, 8, 60)), 3) { LineJoin = PenLineJoin.Round }, letters);
        }

        static void DrawCloud(DrawingContext context, double x, double y, double scale)
        {
            Brush shade = Brush(Color.FromArgb(200, 214, 150, 140));
            Brush light = Brush(Color.FromArgb(235, 255, 222, 190));
            double[,] puffs = { { 0, 8, 34 }, { 34, 0, 30 }, { 66, 8, 26 }, { -30, 12, 22 }, { 92, 14, 18 } };
            for (int i = 0; i < puffs.GetLength(0); i++)
                context.DrawEllipse(shade, null, new Point(x + puffs[i, 0] * scale, y + (puffs[i, 1] + 6) * scale),
                    puffs[i, 2] * scale, puffs[i, 2] * 0.55 * scale);
            for (int i = 0; i < puffs.GetLength(0); i++)
                context.DrawEllipse(light, null, new Point(x + puffs[i, 0] * scale, y + puffs[i, 1] * scale),
                    puffs[i, 2] * scale, puffs[i, 2] * 0.5 * scale);
        }

        static void DrawBirds(DrawingContext context)
        {
            Pen pen = new Pen(Brush(Rgb(70, 60, 80)), 1.6) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
            foreach (Point bird in new[] { new Point(250, 140), new Point(268, 128), new Point(290, 146) })
            {
                context.DrawLine(pen, new Point(bird.X - 7, bird.Y - 4), bird);
                context.DrawLine(pen, bird, new Point(bird.X + 7, bird.Y - 4));
            }
        }

        // Rolling hill line through the given heights above the base, spread over the full width.
        static Geometry Hill(double baseY, int[] heights)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext stream = geometry.Open())
            {
                double step = ViewWidth / (heights.Length - 1);
                stream.BeginFigure(new Point(0, ViewHeight), true, true);
                stream.LineTo(new Point(0, baseY - heights[0]), true, true);
                for (int i = 1; i < heights.Length; i++)
                {
                    Point previous = new Point((i - 1) * step, baseY - heights[i - 1]);
                    Point next = new Point(i * step, baseY - heights[i]);
                    stream.BezierTo(new Point(previous.X + step / 2, previous.Y), new Point(next.X - step / 2, next.Y), next, true, true);
                }
                stream.LineTo(new Point(ViewWidth, ViewHeight), true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        static void DrawHouses(DrawingContext context)
        {
            Color[] walls = { Rgb(232, 214, 186), Rgb(196, 120, 96), Rgb(226, 196, 132), Rgb(168, 180, 196), Rgb(236, 226, 210) };
            Color[] roofs = { Rgb(150, 66, 52), Rgb(90, 70, 70), Rgb(122, 58, 48), Rgb(70, 80, 96), Rgb(140, 80, 56) };
            double[] xs = { 150, 250, 352, 450, 548 };
            double[] widths = { 84, 78, 88, 74, 86 };
            double[] heights = { 46, 54, 42, 50, 44 };
            Brush lit = Brush(Rgb(255, 214, 110));
            Brush dark = Brush(Rgb(90, 100, 120));
            for (int i = 0; i < xs.Length; i++)
            {
                double bottom = 318;
                Rect wall = new Rect(xs[i], bottom - heights[i], widths[i], heights[i]);
                context.DrawRectangle(Brush(walls[i]), null, wall);
                context.DrawGeometry(Brush(roofs[i]), null, Polygon(new Point(0, 0), new[]
                {
                    wall.Left - 6, wall.Top, wall.Left + wall.Width / 2, wall.Top - 26 - i % 2 * 6, wall.Right + 6, wall.Top
                }));
                for (int w = 0; w < 2; w++)
                {
                    Rect window = new Rect(wall.Left + 12 + w * (wall.Width - 38), wall.Top + 10, 14, 12);
                    context.DrawRectangle((i + w) % 3 == 0 ? dark : lit, new Pen(Brush(Rgb(250, 245, 235)), 1.2), window);
                }
                context.DrawRectangle(Brush(Rgb(110, 72, 52)), null, new Rect(wall.Left + wall.Width / 2 - 6, bottom - 18, 12, 18));
            }
        }

        static void DrawTree(DrawingContext context, double x, double bottom)
        {
            context.DrawRectangle(Brush(Rgb(92, 64, 44)), null, new Rect(x - 6, bottom - 50, 12, 60));
            Brush back = Brush(Rgb(46, 84, 50));
            Brush front = Brush(Rgb(66, 112, 62));
            context.DrawEllipse(back, null, new Point(x - 22, bottom - 70), 34, 30);
            context.DrawEllipse(back, null, new Point(x + 24, bottom - 66), 32, 28);
            context.DrawEllipse(front, null, new Point(x, bottom - 92), 38, 34);
            context.DrawEllipse(front, null, new Point(x - 10, bottom - 58), 26, 20);
        }

        static void DrawFence(DrawingContext context, double top)
        {
            Brush paint = Brush(Rgb(240, 232, 218));
            Pen edge = new Pen(Brush(Rgb(170, 150, 130)), 0.8);
            context.DrawRectangle(paint, edge, new Rect(0, top + 8, ViewWidth, 4));
            context.DrawRectangle(paint, edge, new Rect(0, top + 20, ViewWidth, 4));
            for (double x = 6; x < ViewWidth; x += 18)
                context.DrawGeometry(paint, edge, Polygon(new Point(x, top), new[] { 0.0, 0, 4, -5, 8, 0, 8, 30, 0, 30 }));
        }

        static Geometry Polygon(Point origin, double[] points)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext stream = geometry.Open())
            {
                stream.BeginFigure(new Point(origin.X + points[0], origin.Y + points[1]), true, true);
                for (int i = 2; i < points.Length; i += 2)
                    stream.LineTo(new Point(origin.X + points[i], origin.Y + points[i + 1]), true, true);
            }
            geometry.Freeze();
            return geometry;
        }

        static FormattedText Text(string text, Typeface typeface, double size, Brush brush)
        {
            return new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface, size, brush, 1.0);
        }

        static LinearGradientBrush Vertical(params Color[] colors)
        {
            LinearGradientBrush brush = new LinearGradientBrush { StartPoint = new Point(0, 0), EndPoint = new Point(0, 1) };
            for (int i = 0; i < colors.Length; i++)
                brush.GradientStops.Add(new GradientStop(colors[i], colors.Length == 1 ? 0 : (double)i / (colors.Length - 1)));
            return brush;
        }

        static Brush Brush(Color color)
        {
            return new SolidColorBrush(color);
        }

        static Color Rgb(byte r, byte g, byte b)
        {
            return Color.FromRgb(r, g, b);
        }

        static Brush ToTexture(DrawingGroup drawing)
        {
            DrawingBrush brush = new DrawingBrush(drawing) { Stretch = Stretch.Fill };
            return B5RoomTextures.ToBitmap(brush, PictureTextureSize);
        }
    }
}
