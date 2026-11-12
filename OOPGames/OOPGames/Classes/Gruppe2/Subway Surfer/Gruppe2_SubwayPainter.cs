using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public sealed class Gruppe2_SubwayPainter : IPaintGame2
    {
        private static readonly Brush SkyBrush = Brush(Colors.LightSkyBlue);
        private static readonly Brush TrackBrush = Brush(Color.FromRgb(77, 74, 69));
        private static readonly Brush RailBrush = Brush(Color.FromRgb(203, 210, 216));
        private static readonly Brush PinkBrush = Brush(Color.FromRgb(231, 49, 135));

        public string Name { get { return "Gruppe 2 - Subway Surfer Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            Draw(canvas, currentField);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            Draw(canvas, currentField);
        }

        private static Brush Brush(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private static void Draw(Canvas canvas, IGameField currentField)
        {
            IGruppe2_SubwayField field = currentField as IGruppe2_SubwayField;
            canvas.Children.Clear();
            if (field == null)
            {
                return;
            }

            double width = Math.Max(320, canvas.ActualWidth);
            double height = Math.Max(260, canvas.ActualHeight);
            canvas.Background = SkyBrush;

            DrawBackground(canvas, width, height);
            DrawTrack(canvas, width, height, field);
            DrawWorldObjects(canvas, width, height, field);
            DrawRunner(canvas, width, height, field, false);
            if (field.GuardVisible)
            {
                DrawRunner(canvas, width, height, field, true);
            }

            DrawHud(canvas, width, height, field);
            if (field.GameOver)
            {
                DrawGameOver(canvas, width, height);
            }
        }

        private static void DrawBackground(Canvas canvas, double width, double height)
        {
            double center = width / 2;
            AddRectangle(canvas, 0, 0, width, height * 0.39, Brush(Color.FromRgb(104, 190, 224)));
            AddRectangle(canvas, 0, height * 0.30, width, height * 0.13, Brush(Color.FromRgb(209, 157, 104)));
            AddPolygon(canvas, new[]
            {
                new Point(0, height * 0.03),
                new Point(center - width * 0.075, height * 0.32),
                new Point(center - width * 0.39, height * 0.91),
                new Point(0, height * 0.91)
            }, Brush(Color.FromRgb(174, 76, 48)));
            AddPolygon(canvas, new[]
            {
                new Point(width, height * 0.02),
                new Point(center + width * 0.075, height * 0.32),
                new Point(center + width * 0.39, height * 0.91),
                new Point(width, height * 0.91)
            }, Brush(Color.FromRgb(195, 91, 48)));

            for (int side = 0; side < 2; side++)
            {
                for (int floor = 0; floor < 4; floor++)
                {
                    double progress = 0.18 + floor * 0.19;
                    double y = height * (0.24 + progress * 0.45);
                    double facadeInset = width * (0.045 + progress * 0.30);
                    double edge = side == 0 ? facadeInset : width - facadeInset;
                    double windowWidth = width * (0.035 + progress * 0.025);
                    double windowHeight = height * (0.035 + progress * 0.025);
                    Brush windowBrush = floor % 2 == 0
                        ? Brush(Color.FromRgb(124, 197, 218))
                        : Brush(Color.FromRgb(255, 208, 112));

                    AddRectangle(canvas, side == 0 ? edge : edge - windowWidth, y,
                        windowWidth, windowHeight, windowBrush, Brushes.SaddleBrown, 1);
                    AddRectangle(canvas, side == 0 ? edge : edge - windowWidth,
                        y + windowHeight * 1.5, windowWidth, windowHeight * 0.55,
                        Brush(Color.FromRgb(237, 167, 66)), Brushes.SaddleBrown, 1);
                }
            }

            AddLine(canvas, center - width * 0.38, height * 0.28, center + width * 0.38, height * 0.28, Brushes.DimGray, 3);
            AddLine(canvas, center - width * 0.27, height * 0.20,
                center + width * 0.27, height * 0.20, Brushes.DimGray, 2);
            for (int cable = -2; cable <= 2; cable++)
            {
                double x = center + cable * width * 0.13;
                AddLine(canvas, x, height * 0.05, center + cable * width * 0.045,
                    height * 0.32, Brush(Color.FromRgb(61, 69, 73)), 1.2);
            }
        }

        private static void DrawTrack(Canvas canvas, double width, double height, IGruppe2_SubwayField field)
        {
            double center = width / 2;
            double horizon = height * 0.34;
            double bottom = height * 0.94;
            AddPolygon(canvas, new[]
            {
                new Point(0, height * 0.91),
                new Point(center - width * 0.39, height * 0.91),
                new Point(center - width * 0.075, horizon),
                new Point(center + width * 0.075, horizon),
                new Point(center + width * 0.39, height * 0.91),
                new Point(width, height * 0.91),
                new Point(width, height),
                new Point(0, height)
            }, Brush(Color.FromRgb(148, 112, 78)));

            double scroll = field.Distance % 22;
            for (int lane = 0; lane < 3; lane++)
            {
                double centerNear = LaneCenter(width, lane, 1);
                double centerFar = LaneCenter(width, lane, 0);
                double nearHalfWidth = width * 0.075;
                double farHalfWidth = width * 0.012;

                AddPolygon(canvas, new[]
                {
                    new Point(centerFar - farHalfWidth, horizon),
                    new Point(centerFar + farHalfWidth, horizon),
                    new Point(centerNear + nearHalfWidth, bottom),
                    new Point(centerNear - nearHalfWidth, bottom)
                }, TrackBrush, Brush(Color.FromRgb(112, 105, 97)), 1);

                for (int rail = -1; rail <= 1; rail += 2)
                {
                    double farX = centerFar + rail * farHalfWidth * 0.55;
                    double nearX = centerNear + rail * nearHalfWidth * 0.55;
                    AddLine(canvas, farX, horizon, nearX, bottom, RailBrush, 3);
                    AddLine(canvas, farX + width * 0.006, horizon, nearX + width * 0.012,
                        bottom, Brush(Color.FromRgb(111, 77, 50)), 1);
                }

                for (int sleeper = 0; sleeper < 11; sleeper++)
                {
                    double progress = (sleeper + scroll / 22) / 11;
                    double y = horizon + progress * (bottom - horizon);
                    double halfWidth = farHalfWidth + (nearHalfWidth - farHalfWidth) * progress;
                    double laneCenter = LaneCenter(width, lane, progress);
                    AddLine(canvas, laneCenter - halfWidth * 0.82, y,
                        laneCenter + halfWidth * 0.82, y,
                        Brush(Color.FromRgb(123, 80, 52)), 1 + progress * 5);
                }
            }

            AddLine(canvas, center - width * 0.29, height * 0.88,
                center - width * 0.075, horizon, Brush(Color.FromRgb(218, 191, 147)), 2);
            AddLine(canvas, center + width * 0.29, height * 0.88,
                center + width * 0.075, horizon, Brush(Color.FromRgb(218, 191, 147)), 2);
        }

        private static double LaneCenter(double width, int lane, double progress)
        {
            return width / 2 + (lane - 1) * width * (0.018 + 0.212 * progress);
        }

        private static void DrawWorldObjects(Canvas canvas, double width, double height, IGruppe2_SubwayField field)
        {
            foreach (Gruppe2_SubwayObstacle obstacle in field.Obstacles.OrderByDescending(item => item.Distance))
            {
                double approach = 1 - Math.Max(0, Math.Min(100, obstacle.Distance)) / 100;
                double progress = approach * 0.82;

                double scale = 0.32 + approach * 0.9;
                double y = height * 0.34 + approach * height * 0.50;
                double x = LaneCenter(width, obstacle.Lane, progress);
                if (obstacle.Moving)
                {
                    x += Math.Sin(obstacle.Distance / 9) * width * 0.012;
                }

                DrawObstacle(canvas, obstacle.Kind, x, y, scale);
            }

            foreach (Gruppe2_SubwayReward reward in field.Rewards.OrderByDescending(item => item.Distance))
            {
                double approach = 1 - Math.Max(0, Math.Min(100, reward.Distance)) / 100;
                double progress = approach * 0.82;
                double scale = 0.25 + approach * 0.8;
                double y = height * 0.34 + approach * height * 0.50;
                double x = LaneCenter(width, reward.Lane, progress);
                if (reward.Kind == Gruppe2_SubwayRewardKind.Coin)
                {
                    AddEllipse(canvas, x - 8 * scale, y - 25 * scale, 16 * scale, 16 * scale,
                        Brushes.Gold, Brushes.DarkGoldenrod, 2 * scale);
                    AddEllipse(canvas, x - 4 * scale, y - 21 * scale, 8 * scale, 8 * scale,
                        Brushes.LightGoldenrodYellow, null, 0);
                }
                else
                {
                    AddRectangle(canvas, x - 16 * scale, y - 38 * scale, 32 * scale, 30 * scale,
                        UpgradeColor(reward.Upgrade), Brushes.White, 2 * scale);
                    DrawUpgradeIcon(canvas, reward.Upgrade, x, y - 23 * scale, scale);
                }
            }
        }

        private static Brush UpgradeColor(Gruppe2_SubwayUpgradeKind upgrade)
        {
            switch (upgrade)
            {
                case Gruppe2_SubwayUpgradeKind.Magnet: return Brush(Color.FromRgb(190, 65, 77));
                case Gruppe2_SubwayUpgradeKind.DoubleCoins: return Brush(Color.FromRgb(196, 137, 27));
                case Gruppe2_SubwayUpgradeKind.Surfboard: return Brush(Color.FromRgb(25, 143, 157));
                case Gruppe2_SubwayUpgradeKind.JumpStick: return Brush(Color.FromRgb(78, 140, 72));
                case Gruppe2_SubwayUpgradeKind.Jetpack: return Brush(Color.FromRgb(65, 105, 176));
                default: return Brush(Color.FromRgb(147, 63, 209));
            }
        }

        private static void DrawUpgradeIcon(
            Canvas canvas,
            Gruppe2_SubwayUpgradeKind upgrade,
            double x,
            double y,
            double scale)
        {
            double size = scale;
            switch (upgrade)
            {
                case Gruppe2_SubwayUpgradeKind.Magnet:
                    AddPolygon(canvas, new[]
                    {
                        new Point(x - 10 * size, y - 11 * size),
                        new Point(x - 5 * size, y - 11 * size),
                        new Point(x - 5 * size, y + size),
                        new Point(x - 4 * size, y + 5 * size),
                        new Point(x - 2 * size, y + 7 * size),
                        new Point(x + 2 * size, y + 7 * size),
                        new Point(x + 4 * size, y + 5 * size),
                        new Point(x + 5 * size, y + size),
                        new Point(x + 5 * size, y - 11 * size),
                        new Point(x + 10 * size, y - 11 * size),
                        new Point(x + 10 * size, y + 2 * size),
                        new Point(x + 8 * size, y + 7 * size),
                        new Point(x + 4 * size, y + 11 * size),
                        new Point(x, y + 12 * size),
                        new Point(x - 4 * size, y + 11 * size),
                        new Point(x - 8 * size, y + 7 * size),
                        new Point(x - 10 * size, y + 2 * size)
                    }, Brush(Color.FromRgb(205, 30, 12)), Brush(Color.FromRgb(150, 25, 8)), 1.2 * size);
                    AddPolygon(canvas, new[]
                    {
                        new Point(x - 10 * size, y - 11 * size),
                        new Point(x - 5 * size, y - 11 * size),
                        new Point(x - 5 * size, y - 3 * size),
                        new Point(x - 9 * size, y - 3 * size)
                    }, Brush(Color.FromRgb(255, 220, 0)), Brush(Color.FromRgb(203, 160, 0)), size);
                    AddPolygon(canvas, new[]
                    {
                        new Point(x + 5 * size, y - 11 * size),
                        new Point(x + 10 * size, y - 11 * size),
                        new Point(x + 9 * size, y - 3 * size),
                        new Point(x + 5 * size, y - 3 * size)
                    }, Brush(Color.FromRgb(255, 220, 0)), Brush(Color.FromRgb(203, 160, 0)), size);
                    break;
                case Gruppe2_SubwayUpgradeKind.DoubleCoins:
                    AddEllipse(canvas, x - 9 * size, y - 5 * size, 12 * size, 12 * size,
                        Brushes.Gold, Brushes.DarkGoldenrod, size);
                    AddEllipse(canvas, x - 2 * size, y - 5 * size, 12 * size, 12 * size,
                        Brushes.Gold, Brushes.DarkGoldenrod, size);
                    AddLine(canvas, x - 3 * size, y - 2 * size, x - 3 * size, y + 4 * size,
                        Brushes.DarkGoldenrod, size);
                    AddLine(canvas, x + 4 * size, y - 2 * size, x + 4 * size, y + 4 * size,
                        Brushes.DarkGoldenrod, size);
                    break;
                case Gruppe2_SubwayUpgradeKind.Surfboard:
                    AddPolygon(canvas, new[]
                    {
                        new Point(x - 4 * size, y - 9 * size),
                        new Point(x + 3 * size, y - 7 * size),
                        new Point(x + 5 * size, y + 7 * size),
                        new Point(x, y + 10 * size),
                        new Point(x - 5 * size, y + 7 * size)
                    }, Brushes.LightCyan, Brushes.White, size);
                    AddLine(canvas, x - 3 * size, y - 2 * size, x + 4 * size, y,
                        Brushes.Teal, 2 * size);
                    break;
                case Gruppe2_SubwayUpgradeKind.JumpStick:
                    AddLine(canvas, x, y - 9 * size, x, y + 3 * size,
                        Brushes.LightGray, 2 * size);
                    AddLine(canvas, x, y + 3 * size, x - 4 * size, y + 5 * size,
                        Brushes.Gold, 2 * size);
                    AddLine(canvas, x - 4 * size, y + 5 * size, x + 4 * size, y + 7 * size,
                        Brushes.Gold, 2 * size);
                    AddLine(canvas, x + 4 * size, y + 7 * size, x - 4 * size, y + 9 * size,
                        Brushes.Gold, 2 * size);
                    AddLine(canvas, x - 4 * size, y + 9 * size, x + 4 * size, y + 11 * size,
                        Brushes.Gold, 2 * size);
                    break;
                case Gruppe2_SubwayUpgradeKind.Jetpack:
                    AddRectangle(canvas, x - 5 * size, y - 6 * size, 10 * size, 12 * size,
                        Brushes.Silver, Brushes.White, size);
                    AddPolygon(canvas, new[]
                    {
                        new Point(x - 4 * size, y + 6 * size),
                        new Point(x, y + 6 * size),
                        new Point(x - 2 * size, y + 11 * size)
                    }, Brushes.OrangeRed);
                    AddPolygon(canvas, new[]
                    {
                        new Point(x + size, y + 6 * size),
                        new Point(x + 5 * size, y + 6 * size),
                        new Point(x + 3 * size, y + 11 * size)
                    }, Brushes.Gold);
                    break;
            }
        }

        private static void DrawObstacle(Canvas canvas, Gruppe2_SubwayObstacleKind kind, double x, double y, double scale)
        {
            double w = 35 * scale;
            double h = 42 * scale;
            switch (kind)
            {
                case Gruppe2_SubwayObstacleKind.Train:
                    DrawTrain(canvas, x, y, w, h);
                    break;
                case Gruppe2_SubwayObstacleKind.CargoRamp:
                    DrawCargoWagon(canvas, x, y, w, h);
                    break;
                case Gruppe2_SubwayObstacleKind.CargoWagon:
                    DrawCargoTrailer(canvas, x, y, w, h);
                    break;
                case Gruppe2_SubwayObstacleKind.LargeBarrier:
                    DrawLargeBarrier(canvas, x, y, w, h, scale);
                    break;
                case Gruppe2_SubwayObstacleKind.SmallBarrier:
                    AddRectangle(canvas, x - w, y - h * 0.8, w * 2, h * 0.8,
                        Brush(Color.FromRgb(235, 180, 51)), Brushes.Black, 2 * scale);
                    break;
                case Gruppe2_SubwayObstacleKind.Bush:
                    AddEllipse(canvas, x - w, y - h, w * 2, h, Brush(Color.FromRgb(45, 136, 62)),
                        Brushes.DarkGreen, scale);
                    AddEllipse(canvas, x - w * 0.55, y - h * 1.3, w, h, Brush(Color.FromRgb(67, 165, 75)),
                        null, 0);
                    break;
                case Gruppe2_SubwayObstacleKind.PlantPot:
                    AddRectangle(canvas, x - w * 0.65, y - h * 0.65, w * 1.3, h * 0.65,
                        Brush(Color.FromRgb(183, 91, 48)), Brushes.SaddleBrown, 2 * scale);
                    AddEllipse(canvas, x - w * 0.55, y - h * 1.35, w * 0.75, h * 0.75,
                        Brush(Color.FromRgb(67, 155, 72)), null, 0);
                    AddEllipse(canvas, x - w * 0.1, y - h * 1.55, w * 0.65, h * 0.75,
                        Brush(Color.FromRgb(48, 132, 61)), null, 0);
                    break;
                case Gruppe2_SubwayObstacleKind.Curb:
                    AddRectangle(canvas, x - w, y - h * 0.45, w * 2, h * 0.45,
                        Brush(Color.FromRgb(172, 172, 165)), Brushes.DimGray, scale);
                    break;
                case Gruppe2_SubwayObstacleKind.CenterPillar:
                    AddRectangle(canvas, x - w * 0.45, y - h * 2, w * 0.9, h * 2,
                        Brush(Color.FromRgb(112, 105, 99)), Brushes.Black, 2 * scale);
                    break;
                case Gruppe2_SubwayObstacleKind.Signal:
                    AddRectangle(canvas, x - w * 0.12, y - h * 2, w * 0.24, h * 2,
                        Brushes.DimGray, Brushes.Black, scale);
                    AddEllipse(canvas, x - w * 0.4, y - h * 2, w * 0.8, h * 0.8, Brushes.Red, null, 0);
                    AddEllipse(canvas, x - w * 0.4, y - h * 1.1, w * 0.8, h * 0.8, Brushes.LimeGreen, null, 0);
                    break;
            }
        }

        private static void DrawLargeBarrier(Canvas canvas, double x, double y, double halfWidth, double height, double scale)
        {
            Brush red = Brush(Color.FromRgb(196, 43, 42));
            Brush white = Brush(Color.FromRgb(238, 236, 220));
            double signTop = y - height * 2.15;
            double signBottom = y - height * 1.05;
            double postWidth = halfWidth * 0.16;

            AddRectangle(canvas, x - halfWidth * 0.92, signTop, halfWidth * 1.84,
                signBottom - signTop, red, Brush(Color.FromRgb(104, 35, 36)), 2 * scale);

            AddPolygon(canvas, new[]
            {
                new Point(x - halfWidth * 0.78, signTop + (signBottom - signTop) * 0.1),
                new Point(x - halfWidth * 0.49, signTop + (signBottom - signTop) * 0.1),
                new Point(x, signTop + (signBottom - signTop) * 0.61),
                new Point(x + halfWidth * 0.49, signTop + (signBottom - signTop) * 0.1),
                new Point(x + halfWidth * 0.78, signTop + (signBottom - signTop) * 0.1),
                new Point(x, signTop + (signBottom - signTop) * 0.86)
            }, white);
            AddPolygon(canvas, new[]
            {
                new Point(x - halfWidth * 0.92, signTop + (signBottom - signTop) * 0.1),
                new Point(x - halfWidth * 0.79, signTop + (signBottom - signTop) * 0.1),
                new Point(x - halfWidth * 0.79, signTop + (signBottom - signTop) * 0.37),
                new Point(x - halfWidth * 0.92, signTop + (signBottom - signTop) * 0.5)
            }, white);
            AddPolygon(canvas, new[]
            {
                new Point(x + halfWidth * 0.79, signTop + (signBottom - signTop) * 0.1),
                new Point(x + halfWidth * 0.92, signTop + (signBottom - signTop) * 0.1),
                new Point(x + halfWidth * 0.92, signTop + (signBottom - signTop) * 0.5),
                new Point(x + halfWidth * 0.79, signTop + (signBottom - signTop) * 0.37)
            }, white);

            AddRectangle(canvas, x - halfWidth * 0.9, signBottom, postWidth, y - signBottom,
                Brush(Color.FromRgb(105, 67, 55)), Brush(Color.FromRgb(59, 48, 44)), scale);
            AddRectangle(canvas, x + halfWidth * 0.74, signBottom, postWidth, y - signBottom,
                Brush(Color.FromRgb(105, 67, 55)), Brush(Color.FromRgb(59, 48, 44)), scale);
            AddRectangle(canvas, x - halfWidth * 0.96, signBottom - height * 0.09,
                halfWidth * 1.92, height * 0.12, white, Brush(Color.FromRgb(104, 35, 36)), scale);

            AddEllipse(canvas, x - halfWidth * 0.8, signTop - height * 0.05,
                halfWidth * 0.22, halfWidth * 0.22, Brush(Color.FromRgb(55, 59, 62)),
                Brush(Color.FromRgb(190, 197, 194)), scale);
            AddEllipse(canvas, x + halfWidth * 0.58, signTop - height * 0.05,
                halfWidth * 0.22, halfWidth * 0.22, Brush(Color.FromRgb(55, 59, 62)),
                Brush(Color.FromRgb(190, 197, 194)), scale);
            AddEllipse(canvas, x - halfWidth * 0.82, signBottom - height * 0.02,
                halfWidth * 0.19, halfWidth * 0.19, Brush(Color.FromRgb(55, 59, 62)),
                Brush(Color.FromRgb(190, 197, 194)), scale);
            AddEllipse(canvas, x + halfWidth * 0.63, signBottom - height * 0.02,
                halfWidth * 0.19, halfWidth * 0.19, Brush(Color.FromRgb(55, 59, 62)),
                Brush(Color.FromRgb(190, 197, 194)), scale);
        }

        private static void DrawCargoWagon(Canvas canvas, double x, double y, double halfWidth, double height)
        {
            double rearY = y - height * 2.05;
            double frontY = y - height * 0.12;
            double rearHalfWidth = halfWidth * 0.64;
            double frontHalfWidth = halfWidth * 0.88;
            double wagonScale = halfWidth / 35;
            Brush wagonRed = Brush(Color.FromRgb(194, 55, 42));
            Brush rimRed = Brush(Color.FromRgb(224, 75, 58));
            Brush wagonLight = Brush(Color.FromRgb(211, 69, 53));
            Brush wagonShade = Brush(Color.FromRgb(157, 43, 36));
            Brush inside = Brush(Color.FromRgb(77, 78, 70));

            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth, rearY),
                new Point(x + rearHalfWidth, rearY),
                new Point(x + frontHalfWidth, frontY),
                new Point(x - frontHalfWidth, frontY)
            }, Brush(Color.FromRgb(47, 54, 53)));

            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth, rearY + height * 0.12),
                new Point(x - rearHalfWidth * 0.78, rearY + height * 0.12),
                new Point(x - frontHalfWidth * 0.76, frontY - height * 0.12),
                new Point(x - frontHalfWidth, frontY - height * 0.12)
            }, wagonLight);
            AddPolygon(canvas, new[]
            {
                new Point(x + rearHalfWidth * 0.78, rearY + height * 0.12),
                new Point(x + rearHalfWidth, rearY + height * 0.12),
                new Point(x + frontHalfWidth, frontY - height * 0.12),
                new Point(x + frontHalfWidth * 0.76, frontY - height * 0.12)
            }, wagonShade);
            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth, rearY + height * 0.12),
                new Point(x + rearHalfWidth, rearY + height * 0.12),
                new Point(x + rearHalfWidth * 0.78, rearY + height * 0.34),
                new Point(x - rearHalfWidth * 0.78, rearY + height * 0.34)
            }, rimRed);

            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth * 0.78, rearY + height * 0.18),
                new Point(x + rearHalfWidth * 0.78, rearY + height * 0.18),
                new Point(x + frontHalfWidth * 0.76, frontY - height * 0.18),
                new Point(x - frontHalfWidth * 0.76, frontY - height * 0.18)
            }, inside);
            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth * 0.68, rearY + height * 0.35),
                new Point(x + rearHalfWidth * 0.68, rearY + height * 0.35),
                new Point(x + frontHalfWidth * 0.65, frontY - height * 0.31),
                new Point(x - frontHalfWidth * 0.65, frontY - height * 0.31)
            }, Brush(Color.FromRgb(102, 102, 91)));

            AddPolygon(canvas, new[]
            {
                new Point(x - frontHalfWidth, frontY - height * 0.12),
                new Point(x + frontHalfWidth, frontY - height * 0.12),
                new Point(x + frontHalfWidth * 0.94, frontY + height * 0.20),
                new Point(x - frontHalfWidth * 0.94, frontY + height * 0.20)
            }, wagonRed, Brush(Color.FromRgb(132, 41, 33)), wagonScale * 1.5);

            AddLine(canvas, x - rearHalfWidth, rearY + height * 0.12,
                x - frontHalfWidth, frontY - height * 0.12, rimRed, 3 * wagonScale);
            AddLine(canvas, x + rearHalfWidth, rearY + height * 0.12,
                x + frontHalfWidth, frontY - height * 0.12, rimRed, 3 * wagonScale);
            AddLine(canvas, x - frontHalfWidth * 0.68, frontY + height * 0.08,
                x + frontHalfWidth * 0.68, frontY + height * 0.08,
                Brush(Color.FromRgb(112, 43, 36)), 2 * wagonScale);

            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth * 0.55, rearY + height * 0.56),
                new Point(x - rearHalfWidth * 0.13, rearY + height * 0.42),
                new Point(x + rearHalfWidth * 0.22, rearY + height * 0.54),
                new Point(x + rearHalfWidth * 0.55, rearY + height * 0.45),
                new Point(x + frontHalfWidth * 0.53, frontY - height * 0.48),
                new Point(x + frontHalfWidth * 0.18, frontY - height * 0.34),
                new Point(x - frontHalfWidth * 0.2, frontY - height * 0.48),
                new Point(x - frontHalfWidth * 0.55, frontY - height * 0.36)
            }, Brush(Color.FromRgb(124, 123, 108)));
            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth * 0.45, rearY + height * 0.88),
                new Point(x - rearHalfWidth * 0.08, rearY + height * 0.72),
                new Point(x + rearHalfWidth * 0.27, rearY + height * 0.82),
                new Point(x + frontHalfWidth * 0.4, frontY - height * 0.66),
                new Point(x + frontHalfWidth * 0.05, frontY - height * 0.52),
                new Point(x - frontHalfWidth * 0.3, frontY - height * 0.63)
            }, Brush(Color.FromRgb(145, 143, 125)));
            AddEllipse(canvas, x - halfWidth * 0.16, y - height * 1.1,
                halfWidth * 0.35, height * 0.16, Brush(Color.FromRgb(157, 154, 136)), null, 0);
            AddEllipse(canvas, x + halfWidth * 0.06, y - height * 1.46,
                halfWidth * 0.29, height * 0.13, Brush(Color.FromRgb(91, 94, 86)), null, 0);
        }

        private static void DrawCargoTrailer(Canvas canvas, double x, double y, double halfWidth, double height)
        {
            double rearY = y - height * 1.72;
            double frontY = y - height * 0.12;
            double rearHalfWidth = halfWidth * 0.58;
            double frontHalfWidth = halfWidth * 0.92;
            double scale = halfWidth / 35;
            Brush body = Brush(Color.FromRgb(159, 48, 39));
            Brush side = Brush(Color.FromRgb(200, 59, 44));
            Brush top = Brush(Color.FromRgb(75, 78, 70));

            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth, rearY),
                new Point(x + rearHalfWidth, rearY),
                new Point(x + frontHalfWidth, frontY),
                new Point(x - frontHalfWidth, frontY)
            }, body, Brush(Color.FromRgb(102, 39, 34)), scale);
            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth, rearY),
                new Point(x - rearHalfWidth * 0.78, rearY + height * 0.13),
                new Point(x - frontHalfWidth * 0.78, frontY + height * 0.08),
                new Point(x - frontHalfWidth, frontY)
            }, side);
            AddPolygon(canvas, new[]
            {
                new Point(x + rearHalfWidth * 0.78, rearY + height * 0.13),
                new Point(x + rearHalfWidth, rearY),
                new Point(x + frontHalfWidth, frontY),
                new Point(x + frontHalfWidth * 0.78, frontY + height * 0.08)
            }, Brush(Color.FromRgb(129, 39, 34)));
            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth * 0.78, rearY + height * 0.13),
                new Point(x + rearHalfWidth * 0.78, rearY + height * 0.13),
                new Point(x + frontHalfWidth * 0.78, frontY + height * 0.08),
                new Point(x - frontHalfWidth * 0.78, frontY + height * 0.08)
            }, top);

            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth * 0.42, rearY + height * 0.20),
                new Point(x + rearHalfWidth * 0.34, rearY + height * 0.20),
                new Point(x + frontHalfWidth * 0.48, frontY - height * 0.20),
                new Point(x + frontHalfWidth * 0.05, frontY - height * 0.20),
                new Point(x - frontHalfWidth * 0.43, frontY - height * 0.20)
            }, Brush(Color.FromRgb(139, 137, 117)));
            AddPolygon(canvas, new[]
            {
                new Point(x - rearHalfWidth * 0.20, rearY + height * 0.18),
                new Point(x + rearHalfWidth * 0.48, rearY + height * 0.18),
                new Point(x + frontHalfWidth * 0.32, frontY - height * 0.43),
                new Point(x - frontHalfWidth * 0.12, frontY - height * 0.35)
            }, Brush(Color.FromRgb(111, 111, 98)));

            AddLine(canvas, x - halfWidth * 0.28, frontY + height * 0.12,
                x + halfWidth * 0.28, frontY + height * 0.12,
                Brush(Color.FromRgb(48, 50, 47)), 2 * scale);
            AddEllipse(canvas, x - halfWidth * 0.06, frontY + height * 0.08,
                halfWidth * 0.12, height * 0.10, Brush(Color.FromRgb(77, 78, 70)), null, 0);
        }

        private static void DrawTrain(Canvas canvas, double x, double bottom, double halfWidth, double bodyHeight)
        {
            double sx = 2 * halfWidth / 229;
            double sy = 2.2 * bodyHeight / 281;
            Brush red = Brush(Color.FromRgb(218, 75, 62));
            Brush trim = Brush(Color.FromRgb(48, 69, 72));
            Brush fascia = Brush(Color.FromRgb(190, 203, 200));

            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(86, 50), new Point(153, 20), new Point(169, -118),
                new Point(133, -103), new Point(104, -62)
            }, Brush(Color.FromRgb(178, 52, 45)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(195, 20), new Point(267, 48), new Point(245, -58),
                new Point(214, -99), new Point(180, -118)
            }, Brush(Color.FromRgb(235, 88, 70)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(153, 20), new Point(195, 20), new Point(180, -118),
                new Point(169, -118)
            }, Brush(Color.FromRgb(203, 65, 54)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(116, -44), new Point(139, -71), new Point(145, -15), new Point(128, -7)
            }, Brush(Color.FromRgb(53, 91, 94)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(204, -72), new Point(228, -43), new Point(216, -7), new Point(198, -15)
            }, Brush(Color.FromRgb(53, 91, 94)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(110, -49), new Point(134, -74), new Point(138, -65), new Point(116, -40)
            }, Brush(Color.FromRgb(165, 211, 210)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(207, -65), new Point(229, -40), new Point(225, -31), new Point(203, -56)
            }, Brush(Color.FromRgb(165, 211, 210)));

            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(153, 20), new Point(195, 20), new Point(267, 48), new Point(280, 58),
                new Point(287, 71), new Point(289, 85), new Point(289, 248), new Point(285, 265),
                new Point(276, 280), new Point(259, 291), new Point(239, 299), new Point(111, 299),
                new Point(91, 294), new Point(74, 283), new Point(64, 270), new Point(60, 252),
                new Point(60, 85), new Point(63, 71), new Point(72, 59), new Point(86, 50)
            }, red, Brush(Color.FromRgb(174, 54, 46)), 1.5 * sx);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 153, 20, 42, 34,
                Brush(Color.FromRgb(67, 80, 83)), Brush(Color.FromRgb(45, 54, 57)), sx, 2);
            AddTrainEllipse(canvas, x, bottom, sx, sy, 159, 33, 13, 13,
                Brush(Color.FromRgb(174, 207, 212)));
            AddTrainEllipse(canvas, x, bottom, sx, sy, 176, 33, 13, 13,
                Brush(Color.FromRgb(174, 207, 212)));

            AddTrainRectangle(canvas, x, bottom, sx, sy, 80, 61, 189, 94,
                trim, Brush(Color.FromRgb(35, 54, 57)), 2 * sx, 10);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 88, 69, 80, 79,
                Brush(Color.FromRgb(119, 190, 194)), null, 0, 6);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 180, 69, 80, 79,
                Brush(Color.FromRgb(119, 190, 194)), null, 0, 6);
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(89, 135), new Point(154, 69), new Point(167, 69), new Point(97, 143)
            }, Brush(Color.FromRgb(181, 222, 220)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(181, 135), new Point(246, 69), new Point(259, 69), new Point(189, 143)
            }, Brush(Color.FromRgb(181, 222, 220)));

            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(60, 154), new Point(121, 177), new Point(175, 201), new Point(229, 177),
                new Point(289, 154), new Point(289, 174), new Point(231, 195), new Point(210, 300),
                new Point(140, 300), new Point(119, 195), new Point(60, 174)
            }, fascia);
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(60, 225), new Point(119, 225), new Point(124, 248), new Point(60, 248)
            }, Brush(Color.FromRgb(184, 44, 35)));
            AddTrainPolygon(canvas, x, bottom, sx, sy, new[]
            {
                new Point(231, 225), new Point(289, 225), new Point(289, 248), new Point(226, 248)
            }, Brush(Color.FromRgb(184, 44, 35)));
            AddTrainEllipse(canvas, x, bottom, sx, sy, 76, 198, 17, 17,
                Brush(Color.FromRgb(191, 34, 24)));
            AddTrainEllipse(canvas, x, bottom, sx, sy, 100, 203, 18, 18,
                Brush(Color.FromRgb(255, 202, 47)));
            AddTrainEllipse(canvas, x, bottom, sx, sy, 232, 203, 18, 18,
                Brush(Color.FromRgb(255, 202, 47)));
            AddTrainEllipse(canvas, x, bottom, sx, sy, 256, 198, 17, 17,
                Brush(Color.FromRgb(191, 34, 24)));
            AddTrainRectangle(canvas, x, bottom, sx, sy, 145, 211, 59, 7, trim, null, 0, 0);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 145, 222, 59, 7, trim, null, 0, 0);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 145, 233, 59, 7, trim, null, 0, 0);
            AddTrainRectangle(canvas, x, bottom, sx, sy, 151, 264, 48, 28,
                Brush(Color.FromRgb(24, 58, 61)), Brush(Color.FromRgb(106, 134, 133)), sx, 0);
        }

        private static Point TrainPoint(
            double x,
            double bottom,
            double sx,
            double sy,
            double sourceX,
            double sourceY)
        {
            double upperBodySpread = 1 + 0.10 * Math.Max(0, Math.Min(1, (301 - sourceY) / 281));
            return new Point(x + (sourceX - 174.5) * sx * upperBodySpread, bottom + (sourceY - 301) * sy);
        }

        private static void AddTrainPolygon(
            Canvas canvas,
            double x,
            double bottom,
            double sx,
            double sy,
            IEnumerable<Point> points,
            Brush fill,
            Brush stroke = null,
            double strokeWidth = 0)
        {
            AddPolygon(canvas, points.Select(point =>
                TrainPoint(x, bottom, sx, sy, point.X, point.Y)), fill, stroke, strokeWidth);
        }

        private static void AddTrainRectangle(
            Canvas canvas,
            double x,
            double bottom,
            double sx,
            double sy,
            double sourceX,
            double sourceY,
            double sourceWidth,
            double sourceHeight,
            Brush fill,
            Brush stroke,
            double strokeWidth,
            double radius)
        {
            Point topLeft = TrainPoint(x, bottom, sx, sy, sourceX, sourceY);
            Rectangle shape = new Rectangle
            {
                Width = sourceWidth * sx,
                Height = sourceHeight * sy,
                RadiusX = radius * sx,
                RadiusY = radius * sy,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeWidth
            };
            Canvas.SetLeft(shape, topLeft.X);
            Canvas.SetTop(shape, topLeft.Y);
            canvas.Children.Add(shape);
        }

        private static void AddTrainEllipse(
            Canvas canvas,
            double x,
            double bottom,
            double sx,
            double sy,
            double sourceX,
            double sourceY,
            double sourceWidth,
            double sourceHeight,
            Brush fill)
        {
            Point topLeft = TrainPoint(x, bottom, sx, sy, sourceX, sourceY);
            Ellipse shape = new Ellipse
            {
                Width = sourceWidth * sx,
                Height = sourceHeight * sy,
                Fill = fill
            };
            Canvas.SetLeft(shape, topLeft.X);
            Canvas.SetTop(shape, topLeft.Y);
            canvas.Children.Add(shape);
        }

        private static void DrawRunner(Canvas canvas, double width, double height, IGruppe2_SubwayField field, bool guard)
        {
            double x = LaneCenter(width, field.Lane, 0.82);
            double baseY = height * 0.84;
            double scale = guard ? 0.72 : 1;

            if (field.Level == Gruppe2_SubwayLevel.Air && field.JumpHeight > 0)
            {
                baseY -= height * 0.20 * field.JumpHeight;
            }
            else if (field.Level == Gruppe2_SubwayLevel.Air)
            {
                baseY -= height * 0.16;
            }
            else if (field.Level == Gruppe2_SubwayLevel.TrainRoof)
            {
                baseY -= height * 0.06;
            }

            if (guard)
            {
                x -= width * 0.07;
                baseY += height * (0.03 + Math.Abs(Math.Sin(field.Distance / 8)) * 0.035);
            }

            double runCycle = Math.Sin(field.Distance * 0.48);
            double runSwing = runCycle * 13 * scale;
            if (field.Level == Gruppe2_SubwayLevel.Track)
            {
                baseY -= Math.Abs(runCycle) * 2.5 * scale;
            }

            if (!guard && field.Level == Gruppe2_SubwayLevel.Track && field.RollTicks > 0)
            {
                AddEllipse(canvas, x - 23, baseY - 33, 46, 19, PinkBrush, Brushes.DarkMagenta, 2);
                AddEllipse(canvas, x + 4, baseY - 51, 18, 18,
                    Brush(Color.FromRgb(245, 194, 157)), Brushes.Black, 1);
                AddEllipse(canvas, x + 8, baseY - 54, 15, 8, Brushes.DarkMagenta, null, 0);
                AddLine(canvas, x - 20, baseY - 29, x - 34, baseY - 19, PinkBrush, 4);
                return;
            }

            Brush outfit = guard ? Brush(Color.FromRgb(43, 74, 112)) : PinkBrush;
            double leftKneeX = x - 9 * scale + runSwing * 0.45;
            double rightKneeX = x + 9 * scale - runSwing * 0.45;
            double leftFootY = baseY - Math.Max(0, -runCycle) * 7 * scale;
            double rightFootY = baseY - Math.Max(0, runCycle) * 7 * scale;
            AddLine(canvas, x - 5 * scale, baseY - 24 * scale,
                leftKneeX, baseY - 12 * scale, Brushes.Black, 5 * scale);
            AddLine(canvas, leftKneeX, baseY - 12 * scale,
                x - 12 * scale + runSwing, leftFootY, Brushes.Black, 5 * scale);
            AddLine(canvas, x + 5 * scale, baseY - 24 * scale,
                rightKneeX, baseY - 12 * scale, Brushes.Black, 5 * scale);
            AddLine(canvas, rightKneeX, baseY - 12 * scale,
                x + 12 * scale - runSwing, rightFootY, Brushes.Black, 5 * scale);
            AddRectangle(canvas, x - 15 * scale, baseY - 67 * scale, 30 * scale, 42 * scale,
                outfit, Brushes.Black, 1.5 * scale);
            AddLine(canvas, x - 13 * scale, baseY - 58 * scale,
                x - 21 * scale - runSwing * 0.55, baseY - 46 * scale, outfit, 5 * scale);
            AddLine(canvas, x - 21 * scale - runSwing * 0.55, baseY - 46 * scale,
                x - 25 * scale - runSwing, baseY - 34 * scale, outfit, 5 * scale);
            AddLine(canvas, x + 13 * scale, baseY - 58 * scale,
                x + 21 * scale + runSwing * 0.55, baseY - 46 * scale, outfit, 5 * scale);
            AddLine(canvas, x + 21 * scale + runSwing * 0.55, baseY - 46 * scale,
                x + 25 * scale + runSwing, baseY - 34 * scale, outfit, 5 * scale);
            AddEllipse(canvas, x - 12 * scale, baseY - 94 * scale, 24 * scale, 27 * scale,
                Brush(Color.FromRgb(245, 194, 157)), Brushes.Black, scale);
            AddEllipse(canvas, x - 14 * scale, baseY - 99 * scale, 28 * scale, 15 * scale,
                guard ? Brushes.SaddleBrown : Brushes.DarkMagenta, null, 0);
            if (!guard)
            {
                AddLine(canvas, x + 8 * scale, baseY - 88 * scale, x + 16 * scale, baseY - 56 * scale,
                    Brushes.DarkMagenta, 4 * scale);
            }
        }

        private static void DrawHud(Canvas canvas, double width, double height, IGruppe2_SubwayField field)
        {
            AddRectangle(canvas, 10, 10, Math.Min(width * 0.48, 190), 78,
                new SolidColorBrush(Color.FromArgb(210, 20, 27, 38)));
            AddText(canvas, "TEMPO  x" + field.Speed.ToString("0.0"), 20, 16, 15,
                Brushes.Gold, FontWeights.Bold);
            string upgrades = field.ActiveUpgrades.Count == 0
                ? "Upgrades: keine"
                : "Upgrades: " + string.Join(", ", field.ActiveUpgrades.Keys.Select(UpgradeLabel));
            AddText(canvas, upgrades, 20, 40, 11, Brushes.White, FontWeights.Normal);

            if (field.MessageTicks > 0 || field.GameOver)
            {
                AddText(canvas, field.Message, width * 0.2, height - 61, 13,
                    field.GameOver ? Brushes.OrangeRed : Brushes.White, FontWeights.Bold);
            }

            if (field.GuardVisible)
            {
                AddText(canvas, "Bahnwaerter  " + field.GuardGap.ToString("0") + " m",
                    14, height - 58, 12, Brushes.White, FontWeights.Bold);
            }

            AddRectangle(canvas, width - 142, 12, 132, 58,
                new SolidColorBrush(Color.FromArgb(220, 22, 30, 39)));
            AddText(canvas, "SCORE  " + field.Score, width - 132, 16, 16,
                Brushes.White, FontWeights.Bold);
            AddText(canvas, "MÜNZEN  " + field.Coins, width - 132, 41, 13,
                Brushes.Gold, FontWeights.SemiBold);

            AddRectangle(canvas, 0, height - 37, width, 37,
                new SolidColorBrush(Color.FromArgb(195, 26, 34, 41)));
            AddText(canvas, "A/D Spur wechseln     W Springen     S Rollen",
                14, height - 31, 13, Brushes.White, FontWeights.SemiBold);

            AddRectangle(canvas, width / 2 - 62, 10, 124, 30,
                Brush(Color.FromRgb(25, 82, 105)), Brushes.White, 1);
            AddText(canvas, "TRAIN RUN", width / 2 - 53, 14, 17,
                Brushes.Gold, FontWeights.Black);
        }

        private static string UpgradeLabel(Gruppe2_SubwayUpgradeKind upgrade)
        {
            switch (upgrade)
            {
                case Gruppe2_SubwayUpgradeKind.Magnet: return "Magnet";
                case Gruppe2_SubwayUpgradeKind.DoubleCoins: return "Stern (Muenzen x2)";
                case Gruppe2_SubwayUpgradeKind.Surfboard: return "Surfboard";
                case Gruppe2_SubwayUpgradeKind.JumpStick: return "Huepfstab";
                case Gruppe2_SubwayUpgradeKind.Jetpack: return "Jetpack";
                default: return "Upgrade";
            }
        }

        private static void DrawGameOver(Canvas canvas, double width, double height)
        {
            AddRectangle(canvas, width * 0.18, height * 0.36, width * 0.64, height * 0.24,
                new SolidColorBrush(Color.FromArgb(225, 15, 18, 24)));
            AddText(canvas, "SPIEL VORBEI", width * 0.29, height * 0.39, 30,
                Brushes.OrangeRed, FontWeights.Bold);
            AddText(canvas, "Starte ein neues Spiel fuer eine weitere Runde.", width * 0.23,
                height * 0.49, 15, Brushes.White, FontWeights.SemiBold);
        }

        private static void AddRectangle(Canvas canvas, double x, double y, double width, double height,
            Brush fill, Brush stroke = null, double strokeWidth = 0)
        {
            Rectangle shape = new Rectangle { Width = width, Height = height, Fill = fill, Stroke = stroke, StrokeThickness = strokeWidth };
            Canvas.SetLeft(shape, x);
            Canvas.SetTop(shape, y);
            canvas.Children.Add(shape);
        }

        private static void AddEllipse(Canvas canvas, double x, double y, double width, double height,
            Brush fill, Brush stroke, double strokeWidth)
        {
            Ellipse shape = new Ellipse { Width = width, Height = height, Fill = fill, Stroke = stroke, StrokeThickness = strokeWidth };
            Canvas.SetLeft(shape, x);
            Canvas.SetTop(shape, y);
            canvas.Children.Add(shape);
        }

        private static void AddLine(Canvas canvas, double x1, double y1, double x2, double y2,
            Brush brush, double thickness)
        {
            Line line = new Line { X1 = x1, Y1 = y1, X2 = x2, Y2 = y2, Stroke = brush, StrokeThickness = thickness };
            canvas.Children.Add(line);
        }

        private static void AddPolygon(Canvas canvas, IEnumerable<Point> points, Brush fill,
            Brush stroke = null, double strokeWidth = 0)
        {
            Polygon polygon = new Polygon
            {
                Points = new PointCollection(points),
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = strokeWidth
            };
            canvas.Children.Add(polygon);
        }

        private static void AddText(Canvas canvas, string text, double x, double y, double size,
            Brush brush, FontWeight weight)
        {
            TextBlock block = new TextBlock
            {
                Text = text,
                FontSize = size,
                Foreground = brush,
                FontWeight = weight,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = Math.Max(100, canvas.ActualWidth - x - 8)
            };
            Canvas.SetLeft(block, x);
            Canvas.SetTop(block, y);
            canvas.Children.Add(block);
        }
    }
}
