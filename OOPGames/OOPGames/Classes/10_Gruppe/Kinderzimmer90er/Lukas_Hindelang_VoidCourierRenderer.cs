using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OOPGames
{
    internal sealed class G10_VoidCourierRenderer
    {
        const int Width = (int)G10_ShooterConfig.WorldWidth;
        const int Height = (int)G10_ShooterConfig.WorldHeight;
        const int HeaderHeight = (int)G10_ShooterConfig.HeaderHeight;

        static readonly Dictionary<uint, Brush> SolidBrushes = new Dictionary<uint, Brush>();
        static readonly Brush Background = Linear(
            Color.FromRgb(4, 8, 24), Color.FromRgb(15, 24, 55), new Point(0.5, 0), new Point(0.5, 1));
        static readonly Brush Panel = Linear(
            Color.FromArgb(244, 9, 17, 36), Color.FromArgb(228, 18, 30, 55), new Point(0, 0), new Point(0, 1));
        static readonly Brush Hull = Linear(
            Color.FromRgb(185, 218, 239), Color.FromRgb(43, 82, 124), new Point(0.35, 0), new Point(0.7, 1));
        static readonly Brush HullDark = Linear(
            Color.FromRgb(74, 122, 161), Color.FromRgb(19, 39, 70), new Point(0.3, 0), new Point(0.7, 1));
        static readonly Brush Gold = Linear(
            Color.FromRgb(255, 219, 137), Color.FromRgb(167, 87, 42), new Point(0, 0), new Point(0, 1));
        static readonly Brush Glass = Radial(
            Color.FromRgb(214, 250, 255), Color.FromRgb(38, 125, 192), Color.FromArgb(210, 9, 35, 78));
        static readonly Brush EngineGlow = Radial(
            Color.FromArgb(245, 220, 250, 255), Color.FromArgb(220, 52, 198, 255), Color.FromArgb(0, 24, 88, 255));
        static readonly Brush PlayerShotGlow = Radial(
            Color.FromArgb(235, 231, 254, 255), Color.FromArgb(210, 58, 217, 255), Color.FromArgb(0, 15, 93, 255));
        static readonly Brush PlayerFlameBrush = Linear(
            Color.FromRgb(255, 255, 224), Color.FromRgb(55, 180, 255), new Point(0, 0), new Point(0, 1));
        static readonly Brush EnemyShotGlow = Radial(
            Color.FromArgb(245, 255, 230, 177), Color.FromArgb(220, 255, 97, 54), Color.FromArgb(0, 140, 24, 42));
        static readonly Brush GreenNebula = Radial(
            Color.FromArgb(55, 62, 224, 172), Color.FromArgb(25, 16, 136, 156), Color.FromArgb(0, 6, 12, 30));
        static readonly Brush VioletNebula = Radial(
            Color.FromArgb(58, 169, 114, 255), Color.FromArgb(25, 73, 73, 193), Color.FromArgb(0, 10, 10, 40));
        static readonly Brush AmberNebula = Radial(
            Color.FromArgb(48, 255, 151, 70), Color.FromArgb(20, 171, 71, 58), Color.FromArgb(0, 20, 8, 24));
        static readonly Brush White = Solid(Color.FromRgb(231, 244, 255));
        static readonly Brush MutedText = Solid(Color.FromRgb(141, 174, 202));
        static readonly Brush Cyan = Solid(Color.FromRgb(77, 220, 255));
        static readonly Brush Amber = Solid(Color.FromRgb(255, 193, 91));
        static readonly Brush Red = Solid(Color.FromRgb(255, 90, 85));
        static readonly Brush PlanetSurface = Radial(Color.FromRgb(192, 220, 255),
            Color.FromRgb(43, 83, 130), Color.FromRgb(11, 18, 46));
        static readonly Brush PlanetCanopy = Radial(Color.FromRgb(192, 220, 255),
            Color.FromRgb(79, 105, 163), Color.FromRgb(27, 24, 79));
        static readonly Brush Atmosphere = Linear(Color.FromArgb(0, 58, 153, 222),
            Color.FromArgb(31, 32, 99, 164), new Point(0, 0), new Point(0, 1));
        static readonly Brush BurstGlow = Radial(Color.FromArgb(105, 255, 210, 126),
            Color.FromArgb(35, 255, 95, 62), Color.FromArgb(0, 255, 60, 42));
        static readonly Brush BurstFlash = Solid(Color.FromRgb(255, 198, 107));
        static readonly Pen GridPen = new Pen(Solid(Color.FromArgb(20, 90, 167, 208)), 0.35);
        static readonly Pen StarPen = new Pen(Solid(Color.FromArgb(64, 130, 206, 255)), 0.45);
        static readonly Pen FragmentPen = new Pen(Solid(Color.FromRgb(255, 224, 168)), 0.65);
        static readonly Geometry PlayerHullShape = Polygon(
            new Point(0, -9), new Point(8, -1), new Point(6, 5), new Point(3, 3),
            new Point(2, 8), new Point(-2, 8), new Point(-3, 3), new Point(-6, 5), new Point(-8, -1));
        static readonly Geometry PlayerBodyShape = Polygon(
            new Point(0, -8), new Point(3.2, -1), new Point(2, 5), new Point(-2, 5), new Point(-3.2, -1));
        static readonly Geometry PlayerLeftWing = Polygon(
            new Point(-6, 0), new Point(-2.2, 1), new Point(-2.8, 3), new Point(-7, 4));
        static readonly Geometry PlayerRightWing = Polygon(
            new Point(6, 0), new Point(2.2, 1), new Point(2.8, 3), new Point(7, 4));
        static readonly Geometry PlayerFlame = Polygon(
            new Point(0, 5), new Point(-3.3, 14), new Point(0, 12), new Point(3.3, 14));
        static readonly Geometry[] EnemyShapes =
        {
            Polygon(new Point(0, 5), new Point(-6, 2), new Point(-4, -1), new Point(-2, -3),
                new Point(2, -3), new Point(4, -1), new Point(6, 2)),
            Polygon(new Point(0, 5), new Point(-7, 1), new Point(-4, 0), new Point(-5, -3),
                new Point(-2, -2), new Point(0, -5), new Point(2, -2), new Point(5, -3),
                new Point(4, 0), new Point(7, 1)),
            Polygon(new Point(0, 5), new Point(-7, 2), new Point(-6, -2), new Point(-3, -4),
                new Point(3, -4), new Point(6, -2), new Point(7, 2))
        };
        static readonly Geometry[] BossShapes =
        {
            Polygon(new Point(0, -9), new Point(8, -6), new Point(16, -2), new Point(21, 1),
                new Point(16, 5), new Point(7, 7), new Point(0, 10), new Point(-7, 7),
                new Point(-16, 5), new Point(-21, 1), new Point(-16, -2), new Point(-8, -6)),
            Polygon(new Point(0, -10), new Point(5, -7), new Point(13, -7), new Point(19, -2),
                new Point(13, 0), new Point(19, 5), new Point(8, 6), new Point(0, 10),
                new Point(-8, 6), new Point(-19, 5), new Point(-13, 0), new Point(-19, -2), new Point(-13, -7), new Point(-5, -7)),
            Polygon(new Point(-19, -6), new Point(-11, -9), new Point(11, -9), new Point(19, -6),
                new Point(22, 0), new Point(16, 7), new Point(7, 6), new Point(0, 10),
                new Point(-7, 6), new Point(-16, 7), new Point(-22, 0))
        };
        static readonly Geometry PowerCellShape = Polygon(
            new Point(0, -4), new Point(3, -1), new Point(2, 3), new Point(0, 5),
            new Point(-3, 2), new Point(-2, -1));
        static readonly Brush EnemyHull = Linear(Color.FromRgb(255, 169, 151),
            Color.FromRgb(107, 42, 94), new Point(0, 0), new Point(0, 1));
        static readonly Brush BulwarkHull = Linear(Color.FromRgb(255, 210, 148),
            Color.FromRgb(142, 48, 60), new Point(0, 0), new Point(0, 1));
        static readonly Brush EnemyGlow = Radial(Color.FromArgb(75, 255, 94, 72),
            Color.FromArgb(28, 255, 58, 85), Color.FromArgb(0, 150, 25, 92));
        static readonly Brush BossHull = Linear(Color.FromRgb(255, 219, 152),
            Color.FromRgb(108, 61, 93), new Point(0.4, 0), new Point(0.6, 1));
        static readonly Geometry BossLeftPanel = Polygon(
            new Point(-17, 1), new Point(-10, -2), new Point(-5, 1), new Point(-10, 4));
        static readonly Geometry BossRightPanel = Polygon(
            new Point(17, 1), new Point(10, -2), new Point(5, 1), new Point(10, 4));
        static readonly Geometry BossCrown = Polygon(
            new Point(-8, -4), new Point(0, -7), new Point(8, -4), new Point(5, -1), new Point(-5, -1));

        readonly G10_EffectSystem _Effects = new G10_EffectSystem();
        float _BossHitFlash;
        G10_VoidCourierVisualHost _Host;
        IG10_ShooterView _ObservedSession;

        public void Draw(Canvas canvas, IG10_ShooterView session)
        {
            EnsureHost(canvas);
            ObserveSession(session);
            _Effects.Update(G10_ShooterConfig.FixedDeltaSeconds);
            _BossHitFlash = Math.Max(0, _BossHitFlash - G10_ShooterConfig.FixedDeltaSeconds);
            _Host.Render(delegate(DrawingContext drawing)
            {
                drawing.PushClip(new RectangleGeometry(new Rect(0, 0, Width, Height)));
                DrawBackdrop(drawing, session);
                DrawHud(drawing, session);

                foreach (G10_SolarShooterProjectile projectile in session.PlayerProjectiles)
                    DrawProjectile(drawing, projectile, true);
                foreach (G10_SolarShooterProjectile projectile in session.EnemyProjectiles)
                    DrawProjectile(drawing, projectile, false);
                foreach (G10_SolarShooterPowerCell cell in session.PowerCells)
                    DrawPowerCell(drawing, cell, session.TickNumber);
                foreach (G10_SolarShooterEnemy enemy in session.Enemies)
                    DrawEnemy(drawing, enemy, session.TickNumber);
                if (session.Boss != null)
                    DrawBoss(drawing, session.Boss, session.StageIndex, session.TickNumber);
                DrawBursts(drawing);
                DrawPlayer(drawing, session);
                DrawStateOverlay(drawing, session);
                drawing.Pop();
            });
        }

        void EnsureHost(Canvas canvas)
        {
            if (_Host != null && canvas.Children.Contains(_Host)) return;

            _Host = new G10_VoidCourierVisualHost { Width = Width, Height = Height };
            canvas.Children.Clear();
            canvas.Children.Add(_Host);
        }

        void ObserveSession(IG10_ShooterView session)
        {
            if (!ReferenceEquals(_ObservedSession, session))
            {
                _ObservedSession = session;
                _BossHitFlash = 0;
                _Effects.Attach(session);
                session.BossHit += OnBossHit;
            }
        }

        void OnBossHit(object sender, G10_BossHitEventArgs args)
        {
            _BossHitFlash = 0.12f;
        }

        void DrawBackdrop(DrawingContext drawing, IG10_ShooterView session)
        {
            drawing.DrawRectangle(Background, null, new Rect(0, 0, Width, Height));

            Brush nebula = session.Stage.Backdrop == G10_ShooterBackdrop.ShardField ? GreenNebula :
                session.Stage.Backdrop == G10_ShooterBackdrop.SporeGrove ? VioletNebula : AmberNebula;
            drawing.DrawEllipse(nebula, null,
                new Point(42 + (float)Math.Sin(session.TickNumber * 0.006) * 5, 67), 54, 39);
            drawing.DrawEllipse(session.Stage.Backdrop == G10_ShooterBackdrop.Ironworks ? VioletNebula : AmberNebula, null,
                new Point(132, 105 + (float)Math.Sin(session.TickNumber * 0.004) * 4), 37, 48);

            DrawDistantBody(drawing, session);
            DrawStarLayer(drawing, session.TickNumber, 34, 0.35, 1.2, 0.32);
            DrawStarLayer(drawing, session.TickNumber, 26, 0.75, 1.7, 0.62);
            DrawStarLayer(drawing, session.TickNumber, 16, 1.4, 2.2, 0.9);

            for (int i = 0; i < 5; i++)
            {
                double y = 33 + (i * 29 + session.TickNumber * 0.28) % 126;
                drawing.DrawLine(GridPen, new Point(0, y), new Point(Width, y));
            }

            drawing.DrawRectangle(Atmosphere, null,
                new Rect(0, HeaderHeight, Width, Height - HeaderHeight));
        }

        void DrawDistantBody(DrawingContext drawing, IG10_ShooterView session)
        {
            double x = session.Stage.Backdrop == G10_ShooterBackdrop.SporeGrove ? 126 : 28;
            double y = session.Stage.Backdrop == G10_ShooterBackdrop.ShardField ? 45 :
                session.Stage.Backdrop == G10_ShooterBackdrop.SporeGrove ? 56 : 67;
            double radius = session.Stage.Backdrop == G10_ShooterBackdrop.Ironworks ? 18 : 15;
            Brush planet = session.Stage.Backdrop == G10_ShooterBackdrop.SporeGrove ? PlanetCanopy : PlanetSurface;
            drawing.DrawEllipse(planet, null, new Point(x, y), radius, radius);
            drawing.DrawEllipse(null, new Pen(Solid(Color.FromArgb(80, 161, 206, 239)), 0.5),
                new Point(x - radius * 0.18, y - radius * 0.18), radius * 0.81, radius * 0.81);
            drawing.DrawEllipse(Solid(Color.FromArgb(18, 178, 215, 255)), null,
                new Point(x - radius * 0.35, y - radius * 0.35), radius * 0.28, radius * 0.19);
        }

        void DrawStarLayer(DrawingContext drawing, int tick, int count, double speed, double size, double opacity)
        {
            for (int i = 0; i < count; i++)
            {
                double x = (i * 71 + 13) % Width;
                double y = HeaderHeight + (i * 47 + tick * speed) % (Height - HeaderHeight);
                double twinkle = 0.62 + 0.38 * Math.Sin((tick + i * 19) * 0.055);
                byte alpha = (byte)(255 * opacity * twinkle);
                Brush star = Solid(Color.FromArgb(alpha, 193, 225, 255));
                drawing.DrawEllipse(star, null, new Point(x, y), size, size);
                if (size > 1.8)
                {
                    drawing.DrawLine(StarPen,
                        new Point(x - size * 2.2, y), new Point(x + size * 2.2, y));
                }
            }
        }

        void DrawHud(DrawingContext drawing, IG10_ShooterView session)
        {
            drawing.DrawRectangle(Panel, null, new Rect(0, 0, Width, HeaderHeight));
            drawing.DrawLine(new Pen(Linear(Color.FromArgb(30, 132, 206, 255),
                Color.FromArgb(220, 75, 201, 255), new Point(0, 0), new Point(1, 0)), 0.8),
                new Point(0, HeaderHeight - 0.5), new Point(Width, HeaderHeight - 0.5));

            DrawText(drawing, "VOID COURIER", 4, 2, 5.2, White);
            DrawText(drawing, "SCORE " + session.Score.ToString("D7"), 62, 2, 4.7, White);
            DrawText(drawing, "HULL", 4, 10, 3.8, MutedText);
            DrawText(drawing, session.Stage.Name, 33, 10, 3.8, Cyan);
            DrawText(drawing, "W" + session.Player.WeaponLevel, 111, 10, 3.8, Amber);
            DrawText(drawing, "L" + session.Player.Lives, 140, 2, 4.7, White);

            for (int i = 0; i < 3; i++)
            {
                Brush fill = i < session.Player.Lives ? Linear(
                    Color.FromRgb(255, 229, 164), Color.FromRgb(239, 97, 83),
                    new Point(0, 0), new Point(0, 1)) : Solid(Color.FromArgb(80, 110, 142, 165));
                drawing.DrawRoundedRectangle(fill, null, new Rect(17 + i * 5, 10, 3.5, 3), 0.8, 0.8);
            }

            if (session.Boss != null)
            {
                drawing.DrawRoundedRectangle(Panel,
                    new Pen(Solid(Color.FromArgb(190, 107, 195, 234)), 0.55),
                    new Rect(38, 20, 84, 8), 1.7, 1.7);
                DrawText(drawing, session.Stage.BossName, 41, 21, 3.6, White);
                Rect healthBar = new Rect(41, 25, 78, 1.7);
                drawing.DrawRoundedRectangle(Solid(Color.FromRgb(31, 42, 63)), null, healthBar, 0.7, 0.7);
                double ratio = session.Boss.MaxHealth <= 0 ? 0 :
                    Math.Max(0, Math.Min(1, (double)session.Boss.Health / session.Boss.MaxHealth));
                if (ratio > 0)
                    drawing.DrawRoundedRectangle(Linear(Color.FromRgb(255, 198, 99),
                        Color.FromRgb(255, 74, 95), new Point(0, 0), new Point(1, 0)), null,
                        new Rect(healthBar.X, healthBar.Y, healthBar.Width * ratio, healthBar.Height), 0.7, 0.7);
            }
        }

        void DrawPlayer(DrawingContext drawing, IG10_ShooterView session)
        {
            G10_SolarShooterPlayer player = session.Player;
            if (player.IsInvulnerable && session.TickNumber % 4 < 2) return;

            drawing.PushTransform(new TranslateTransform(player.X, player.Y));
            double flicker = 0.76 + 0.24 * Math.Sin(session.TickNumber * 0.7);
            drawing.DrawEllipse(EngineGlow, null, new Point(0, 9), 8 * flicker, 12 * flicker);
            drawing.DrawGeometry(PlayerFlameBrush, null, PlayerFlame);
            drawing.DrawGeometry(HullDark, new Pen(Solid(Color.FromRgb(164, 220, 248)), 0.8), PlayerHullShape);
            drawing.DrawGeometry(Hull, null, PlayerBodyShape);
            drawing.DrawGeometry(Gold, null, PlayerLeftWing);
            drawing.DrawGeometry(Gold, null, PlayerRightWing);
            drawing.DrawRoundedRectangle(Glass, new Pen(Solid(Color.FromArgb(210, 211, 248, 255)), 0.45),
                new Rect(-1.65, -5.2, 3.3, 4.2), 1.5, 1.5);
            drawing.DrawLine(new Pen(Solid(Color.FromArgb(210, 239, 249, 255)), 0.55),
                new Point(-0.5, -4.7), new Point(0.7, -3.1));
            drawing.DrawLine(new Pen(Solid(Color.FromArgb(175, 27, 55, 89)), 0.45),
                new Point(-4.2, 0.2), new Point(-2.4, 1.1));
            drawing.DrawLine(new Pen(Solid(Color.FromArgb(175, 27, 55, 89)), 0.45),
                new Point(4.2, 0.2), new Point(2.4, 1.1));
            drawing.DrawEllipse(Cyan, null, new Point(-5.4, 1.1), 0.65, 0.65);
            drawing.DrawEllipse(Cyan, null, new Point(5.4, 1.1), 0.65, 0.65);
            drawing.Pop();
        }

        void DrawEnemy(DrawingContext drawing, G10_SolarShooterEnemy enemy, int tick)
        {
            drawing.PushTransform(new TranslateTransform(enemy.X, enemy.Y));
            bool bulwark = enemy is G10_Bulwark;
            bool needle = enemy is G10_Needlewing;
            drawing.DrawEllipse(EnemyGlow, null, new Point(0, 0), bulwark ? 10 : 7, bulwark ? 8 : 6);
            Geometry hull = EnemyShapes[bulwark ? 2 : needle ? 1 : 0];
            Brush enemyHull = bulwark ? BulwarkHull : EnemyHull;
            drawing.DrawGeometry(enemyHull, new Pen(Solid(Color.FromRgb(255, 200, 190)), 0.55), hull);
            drawing.DrawGeometry(HullDark, null, Polygon(new Point(-5, 0), new Point(-8, 1), new Point(-5, 2)));
            drawing.DrawGeometry(HullDark, null, Polygon(new Point(5, 0), new Point(8, 1), new Point(5, 2)));
            drawing.DrawEllipse(Glass, null, new Point(0, -1.3), bulwark ? 1.6 : 1.2, 1.3);
            drawing.DrawEllipse(Amber, null, new Point(-2.4, 3.3), 0.65, 0.5);
            drawing.DrawEllipse(Amber, null, new Point(2.4, 3.3), 0.65, 0.5);
            if (bulwark)
            {
                drawing.DrawLine(new Pen(Gold, 0.7), new Point(-3.5, -1.5), new Point(-1.8, 1.2));
                drawing.DrawLine(new Pen(Gold, 0.7), new Point(3.5, -1.5), new Point(1.8, 1.2));
            }
            if (needle && tick % 16 < 5)
                drawing.DrawLine(new Pen(Solid(Color.FromArgb(130, 255, 130, 104)), 0.55),
                    new Point(-4, 5.5), new Point(4, 5.5));
            drawing.Pop();
        }

        void DrawBoss(DrawingContext drawing, G10_SolarShooterBoss boss, int stageIndex, int tick)
        {
            drawing.PushTransform(new TranslateTransform(boss.X, boss.Y));
            Brush glow = stageIndex == 1 ? VioletNebula : stageIndex == 2 ? AmberNebula : GreenNebula;
            drawing.DrawEllipse(glow, null, new Point(0, 0), 25, 15);
            if (_BossHitFlash > 0)
                drawing.DrawEllipse(Solid(Color.FromArgb(120, 255, 255, 255)), null, new Point(0, 0), 20, 10);

            Geometry body = BossShapes[Math.Max(0, Math.Min(BossShapes.Length - 1, stageIndex))];
            drawing.DrawGeometry(BossHull, new Pen(Solid(Color.FromRgb(255, 225, 197)), 0.85), body);
            drawing.DrawGeometry(HullDark, null, BossLeftPanel);
            drawing.DrawGeometry(HullDark, null, BossRightPanel);
            drawing.DrawGeometry(Gold, null, BossCrown);
            drawing.DrawEllipse(Glass, new Pen(Solid(Color.FromArgb(220, 231, 247, 255)), 0.65),
                new Point(0, 1), 4, 3.3);
            drawing.DrawEllipse(Cyan, null, new Point(-12, 2), 1.3, 1.1);
            drawing.DrawEllipse(Cyan, null, new Point(12, 2), 1.3, 1.1);
            drawing.DrawLine(new Pen(Solid(Color.FromArgb(210, 45, 54, 79)), 0.6),
                new Point(0, 5), new Point(0, 8));
            drawing.DrawLine(new Pen(Solid(Color.FromArgb(190, 255, 229, 179)), 0.55),
                new Point(-8, -5), new Point(-3, -2));
            drawing.DrawLine(new Pen(Solid(Color.FromArgb(190, 255, 229, 179)), 0.55),
                new Point(8, -5), new Point(3, -2));
            double pulse = 0.7 + 0.3 * Math.Sin(tick * 0.4);
            drawing.DrawEllipse(EngineGlow, null, new Point(-15, 4), 2 * pulse, 2 * pulse);
            drawing.DrawEllipse(EngineGlow, null, new Point(15, 4), 2 * pulse, 2 * pulse);
            drawing.Pop();
        }

        void DrawProjectile(DrawingContext drawing, G10_SolarShooterProjectile projectile, bool playerShot)
        {
            Brush glow = playerShot ? PlayerShotGlow : EnemyShotGlow;
            Brush core = playerShot ? Solid(Color.FromRgb(190, 249, 255)) : Solid(Color.FromRgb(255, 220, 154));
            drawing.DrawEllipse(glow, null, new Point(projectile.X, projectile.Y), playerShot ? 3.6 : 3.2, 5.4);
            drawing.DrawLine(new Pen(playerShot ? Cyan : Amber, playerShot ? 1.5 : 1.35),
                new Point(projectile.X, projectile.Y + (playerShot ? 5 : -5)),
                new Point(projectile.X, projectile.Y + (playerShot ? -2 : 2)));
            drawing.DrawLine(new Pen(core, 0.65),
                new Point(projectile.X, projectile.Y + (playerShot ? 4 : -4)),
                new Point(projectile.X, projectile.Y + (playerShot ? -1 : 1)));
        }

        void DrawPowerCell(DrawingContext drawing, G10_SolarShooterPowerCell cell, int tick)
        {
            double pulse = 0.78 + 0.22 * Math.Sin(tick * 0.3);
            drawing.DrawEllipse(EngineGlow, null, new Point(cell.X, cell.Y), 7 * pulse, 7 * pulse);
            drawing.PushTransform(new TranslateTransform(cell.X, cell.Y));
            drawing.DrawGeometry(Gold, new Pen(Solid(Color.FromRgb(255, 241, 188)), 0.65), PowerCellShape);
            drawing.Pop();
            drawing.DrawEllipse(White, null, new Point(cell.X, cell.Y), 1, 1.5);
        }

        void DrawBursts(DrawingContext drawing)
        {
            foreach (G10_SolarShooterBurst burst in _Effects.Bursts)
            {
                double progress = burst.Age / 0.72;
                double radius = (burst.Large ? 3 : 1.5) + progress * (burst.Large ? 17 : 10);
                double alpha = Math.Max(0, 1 - progress);
                drawing.PushOpacity(alpha);
                drawing.DrawEllipse(BurstGlow, null, new Point(burst.X, burst.Y), radius, radius);
                drawing.DrawEllipse(BurstFlash, null, new Point(burst.X, burst.Y), radius * 0.25, radius * 0.25);

                int fragments = burst.Large ? 10 : 6;
                for (int i = 0; i < fragments; i++)
                {
                    double angle = (i * 6.28318530718 / fragments) + burst.Seed * 0.31;
                    double distance = radius * (0.45 + (i % 3) * 0.1);
                    Point position = new Point(
                        burst.X + Math.Cos(angle) * distance,
                        burst.Y + Math.Sin(angle) * distance);
                    Point tail = new Point(
                        position.X - Math.Cos(angle) * 2.1,
                        position.Y - Math.Sin(angle) * 2.1);
                    drawing.DrawLine(FragmentPen, tail, position);
                }
                drawing.Pop();
            }
        }

        void DrawStateOverlay(DrawingContext drawing, IG10_ShooterView session)
        {
            if (session.IsGameOver)
                Overlay(drawing, "SHIP LOST", "PRESS ENTER", Red);
            else if (session.IsVictory)
                Overlay(drawing, "ROUTE SECURE", "PRESS ENTER", Cyan);
            else if (session.IsBossWarning)
            {
                drawing.DrawRoundedRectangle(Panel,
                    new Pen(Solid(Color.FromArgb(210, 123, 214, 255)), 0.8),
                    new Rect(23, 55, 114, 31), 3, 3);
                DrawText(drawing, session.Stage.BossName, 27, 61, 7.3, White, true);
                DrawText(drawing, "HOSTILE SIGNATURE APPROACHING", 31, 73, 4.4, Cyan);
            }
            else if (session.IsStageTransition)
            {
                drawing.DrawRoundedRectangle(Panel,
                    new Pen(Solid(Color.FromArgb(180, 123, 214, 255)), 0.7),
                    new Rect(35, 60, 90, 23), 3, 3);
                DrawText(drawing, "SECTOR CLEAR", 48, 66, 6.4, White, true);
            }
        }

        void Overlay(DrawingContext drawing, string title, string prompt, Brush accent)
        {
            drawing.DrawRoundedRectangle(Panel,
                new Pen(Solid(Color.FromArgb(230, 113, 197, 235)), 0.8),
                new Rect(12, 53, 136, 39), 3, 3);
            drawing.DrawRectangle(accent, null, new Rect(15, 56, 1, 33));
            DrawText(drawing, title, 0, 61, 8, accent, true, true);
            DrawText(drawing, prompt, 0, 77, 4.1, White, true, true);
        }

        static void DrawText(DrawingContext drawing, string text, double x, double y, double size,
            Brush brush, bool bold = false, bool centered = false)
        {
            FormattedText formatted = new FormattedText(
                text,
                CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"),
                size,
                brush,
                1.0);
            if (bold) formatted.SetFontWeight(FontWeights.Bold);
            if (centered) x = (Width - formatted.Width) / 2;
            drawing.DrawText(formatted, new Point(x, y));
        }

        static Geometry Polygon(params Point[] points)
        {
            StreamGeometry geometry = new StreamGeometry();
            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(points[0], true, true);
                for (int i = 1; i < points.Length; i++)
                    context.LineTo(points[i], true, false);
            }
            geometry.Freeze();
            return geometry;
        }

        static Brush Solid(Color color)
        {
            uint key = ((uint)color.A << 24) | ((uint)color.R << 16) | ((uint)color.G << 8) | color.B;
            Brush cached;
            if (SolidBrushes.TryGetValue(key, out cached)) return cached;

            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            SolidBrushes.Add(key, brush);
            return brush;
        }

        static Brush Linear(Color first, Color second, Point start, Point end)
        {
            LinearGradientBrush brush = new LinearGradientBrush(first, second, start, end);
            brush.Freeze();
            return brush;
        }

        static Brush Radial(Color center, Color middle, Color edge)
        {
            RadialGradientBrush brush = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.5),
                GradientOrigin = new Point(0.34, 0.3),
                RadiusX = 0.72,
                RadiusY = 0.72,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(center, 0),
                    new GradientStop(middle, 0.48),
                    new GradientStop(edge, 1)
                }
            };
            brush.Freeze();
            return brush;
        }

    }

    internal sealed class G10_SolarShooterBurst
    {
        public G10_SolarShooterBurst(float x, float y, bool large, int seed)
        {
            X = x;
            Y = y;
            Large = large;
            Seed = seed;
        }

        public float X { get; private set; }
        public float Y { get; private set; }
        public bool Large { get; private set; }
        public int Seed { get; private set; }
        public double Age { get; set; }
    }

    internal sealed class G10_VoidCourierVisualHost : FrameworkElement
    {
        readonly VisualCollection _Children;
        readonly DrawingVisual _Visual = new DrawingVisual();

        public G10_VoidCourierVisualHost()
        {
            _Children = new VisualCollection(this) { _Visual };
            SnapsToDevicePixels = true;
        }

        protected override int VisualChildrenCount { get { return _Children.Count; } }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _Children.Count) throw new ArgumentOutOfRangeException(nameof(index));
            return _Children[index];
        }

        public void Render(Action<DrawingContext> draw)
        {
            using (DrawingContext drawing = _Visual.RenderOpen())
                draw(drawing);
        }
    }
}
