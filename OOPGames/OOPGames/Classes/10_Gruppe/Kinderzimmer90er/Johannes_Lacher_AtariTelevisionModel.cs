using System;
using System.Globalization;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // 3D model of the colour TV on the Atari stand: an early-80s 17" set with a wood-grain cabinet,
    // curved picture tube, aluminium control panel with channel dials, tapered rear and rabbit-ear antenna.
    // Millimetres like the console: x to the right from the screen centre, y up from the table top,
    // z towards the viewer with the front of the cabinet at z = 0.
    public static class B5AtariTelevisionModel
    {
        const double Scale = B5AtariConsoleModel.Scale;
        const double TableTop = 1.04, CenterX = -3.6, FrontZ = -1.27;

        // Picture tube: visible area, height of its centre and how far the glass bulges forwards.
        const double ScreenWidth = 344, ScreenHeight = 258, ScreenY = 181, Bulge = 16, GlassEdgeZ = -12;
        const double Left = -214, Right = 300, Top = 360, Bottom = 18, Depth = 260;
        const double PanelLeft = 196, PanelRight = 286;

        static readonly Color _Mask = Color.FromRgb(46, 44, 42);
        static readonly Color _Bezel = Color.FromRgb(20, 20, 21);
        static readonly Color _Aluminium = Color.FromRgb(178, 176, 168);
        static readonly Color _Knob = Color.FromRgb(28, 27, 26);
        static readonly Color _Rear = Color.FromRgb(42, 37, 33);
        static readonly Color _Slot = Color.FromRgb(12, 12, 12);
        static readonly Color _Print = Color.FromRgb(40, 38, 36);
        static readonly Color _Chrome = Color.FromRgb(205, 205, 210);

        static readonly Matrix3D _Place = CreatePlacement();

        public static Point3D ScreenCenter { get { return _Place.Transform(new Point3D(0, ScreenY, GlassEdgeZ + Bulge)); } }

        public static void Build(Model3DGroup parent)
        {
            BuildCabinet(parent);
            BuildBezel(parent);
            BuildControlPanel(parent);
            BuildRear(parent);
            BuildAntenna(parent);
        }

        static void BuildCabinet(Model3DGroup p)
        {
            // Wood-grain shell around a dark front mask that sits a little behind the front edge of the wood.
            Material wood = B5AtariMesh.Textured(B5AtariConsoleModel.WoodGrain);
            const double wall = 14;
            B5AtariMesh.Box(p, _Place, (Left + Right) / 2, Top - wall / 2, -Depth / 2, Right - Left, wall, Depth, wood);
            B5AtariMesh.Box(p, _Place, (Left + Right) / 2, Bottom + wall / 2, -Depth / 2, Right - Left, wall, Depth, wood);
            B5AtariMesh.Box(p, _Place, Left + wall / 2, (Top + Bottom) / 2, -Depth / 2, wall, Top - Bottom - 2 * wall, Depth, wood);
            B5AtariMesh.Box(p, _Place, Right - wall / 2, (Top + Bottom) / 2, -Depth / 2, wall, Top - Bottom - 2 * wall, Depth, wood);
            // The mask is open where the bezel and the tube sit; behind that opening the cabinet is closed further back.
            Material mask = B5AtariMesh.Plastic(_Mask);
            double innerLeft = Left + wall, innerRight = Right - wall, innerTop = Top - wall, innerBottom = Bottom + wall;
            double bezelLeft = -ScreenWidth / 2 - 20, bezelRight = ScreenWidth / 2 + 20, bezelTop = ScreenY + ScreenHeight / 2 + 20;
            const double maskFront = -8, maskBack = -30;
            B5AtariMesh.Box(p, _Place, (innerLeft + innerRight) / 2, (innerTop + innerBottom) / 2, (maskBack - Depth) / 2,
                innerRight - innerLeft, innerTop - innerBottom, maskBack + Depth, mask);
            B5AtariMesh.Box(p, _Place, (innerLeft + bezelLeft) / 2, (innerTop + innerBottom) / 2, (maskFront + maskBack) / 2,
                bezelLeft - innerLeft, innerTop - innerBottom, maskFront - maskBack, mask);
            B5AtariMesh.Box(p, _Place, (bezelRight + innerRight) / 2, (innerTop + innerBottom) / 2, (maskFront + maskBack) / 2,
                innerRight - bezelRight, innerTop - innerBottom, maskFront - maskBack, mask);
            B5AtariMesh.Box(p, _Place, 0, (bezelTop + innerTop) / 2, (maskFront + maskBack) / 2,
                bezelRight - bezelLeft, innerTop - bezelTop, maskFront - maskBack, mask);

            // Chrome trim line along the top front edge and dark feet.
            B5AtariMesh.Box(p, _Place, (Left + Right) / 2, Top - wall - 3, -5, Right - Left - 2 * wall, 2, 2, B5AtariMesh.Metal(_Chrome));
            foreach (double x in new[] { Left + 40, Right - 40 })
                foreach (double z in new[] { -30.0, -Depth + 30 })
                    B5AtariMesh.Box(p, _Place, x, Bottom / 2, z, 44, Bottom, 30, B5AtariMesh.Plastic(_Knob));
        }

        // Raised black frame around the tube with rounded inner corners.
        static void BuildBezel(Model3DGroup p)
        {
            Material bezel = B5AtariMesh.Plastic(_Bezel);
            const double frame = 20, front = 6, back = -16, radius = 26;
            double halfW = ScreenWidth / 2, halfH = ScreenHeight / 2;
            B5AtariMesh.Box(p, _Place, 0, ScreenY + halfH + frame / 2, (front + back) / 2, ScreenWidth + 2 * frame, frame, front - back, bezel);
            B5AtariMesh.Box(p, _Place, 0, ScreenY - halfH - frame / 2, (front + back) / 2, ScreenWidth + 2 * frame, frame, front - back, bezel);
            B5AtariMesh.Box(p, _Place, -halfW - frame / 2, ScreenY, (front + back) / 2, frame, ScreenHeight, front - back, bezel);
            B5AtariMesh.Box(p, _Place, halfW + frame / 2, ScreenY, (front + back) / 2, frame, ScreenHeight, front - back, bezel);
            foreach (int sx in new[] { -1, 1 })
                foreach (int sy in new[] { -1, 1 })
                    AddCornerFill(p, new Point(sx * halfW, ScreenY + sy * halfH), sx, sy, radius, back, front, bezel);

            // Dark interior behind the glass, so nothing shows through at grazing angles.
            B5AtariMesh.Box(p, _Place, 0, ScreenY, GlassEdgeZ - 6, ScreenWidth, ScreenHeight, 4, B5AtariMesh.Plastic(_Slot));
        }

        // Fills the square corner of the screen opening outside a quarter circle, so the opening gets rounded corners.
        static void AddCornerFill(Model3DGroup p, Point corner, int sx, int sy, double radius, double back, double front, Material material)
        {
            const int segments = 6;
            Point center = new Point(corner.X - sx * radius, corner.Y - sy * radius);
            List<Point> arc = new List<Point>();
            for (int i = 0; i <= segments; i++)
            {
                double angle = Math.PI / 2 * i / segments;
                arc.Add(new Point(center.X + sx * radius * Math.Cos(angle), center.Y + sy * radius * Math.Sin(angle)));
            }

            MeshGeometry3D mesh = new MeshGeometry3D();
            Vector3D forward = new Vector3D(0, 0, 1);
            int hub = mesh.Positions.Count;
            B5AtariMesh.AddVertex(mesh, _Place, new Point3D(corner.X, corner.Y, front), forward);
            foreach (Point point in arc) B5AtariMesh.AddVertex(mesh, _Place, new Point3D(point.X, point.Y, front), forward);
            for (int i = 0; i < segments; i++) B5AtariMesh.AddTriangle(mesh, hub, hub + 1 + i, hub + 2 + i, forward);

            // Inner wall of the rounded corner, facing the screen centre.
            for (int i = 0; i < segments; i++)
            {
                Point a = arc[i], b = arc[i + 1];
                Vector3D normal = new Vector3D(center.X - (a.X + b.X) / 2, center.Y - (a.Y + b.Y) / 2, 0);
                normal.Normalize();
                int start = mesh.Positions.Count;
                B5AtariMesh.AddVertex(mesh, _Place, new Point3D(a.X, a.Y, back), normal);
                B5AtariMesh.AddVertex(mesh, _Place, new Point3D(b.X, b.Y, back), normal);
                B5AtariMesh.AddVertex(mesh, _Place, new Point3D(b.X, b.Y, front), normal);
                B5AtariMesh.AddVertex(mesh, _Place, new Point3D(a.X, a.Y, front), normal);
                B5AtariMesh.AddTriangle(mesh, start, start + 1, start + 2, normal);
                B5AtariMesh.AddTriangle(mesh, start, start + 2, start + 3, normal);
            }
            B5AtariMesh.AddModel(p, mesh, material);
        }

        static void BuildControlPanel(Model3DGroup p)
        {
            double centerX = (PanelLeft + PanelRight) / 2;
            double top = Top - 26, bottom = Bottom + 22;
            B5AtariMesh.Box(p, _Place, centerX, (top + bottom) / 2, -3, PanelRight - PanelLeft, top - bottom, 10, B5AtariMesh.Metal(_Aluminium));
            Vector3D right = new Vector3D(1, 0, 0), up = new Vector3D(0, 1, 0);

            B5AtariMesh.Text(p, _Place, "KOSMOS", _Print, new Point3D(centerX, top - 14, 2.2), right, up, 11);
            B5AtariMesh.Text(p, _Place, "COLOR 17", _Print, new Point3D(centerX, top - 28, 2.2), right, up, 6);
            B5AtariMesh.CylinderZ(p, _Place, PanelRight - 14, top - 44, 3, 2, 4, B5AtariMesh.Plastic(Color.FromRgb(210, 40, 30)));

            // Channel dials: a large VHF dial and a smaller UHF dial, both with gripping ribs and a pointer.
            AddDial(p, centerX, 238, 30, 22, "VHF");
            AddDial(p, centerX, 162, 23, 18, "UHF");

            // Volume and colour knobs.
            foreach (int side in new[] { -1, 1 })
            {
                double x = centerX + side * 22;
                B5AtariMesh.CylinderZ(p, _Place, x, 112, 11, 2, 16, B5AtariMesh.Plastic(_Knob));
                B5AtariMesh.CylinderZ(p, _Place, x, 112, 6, 16, 17, B5AtariMesh.Metal(_Chrome));
                B5AtariMesh.Text(p, _Place, side < 0 ? "VOL" : "COLOR", _Print, new Point3D(x, 93, 2.2), right, up, 5);
            }

            // Speaker grille.
            for (double y = bottom + 8; y <= bottom + 44; y += 6)
                B5AtariMesh.Box(p, _Place, centerX, y, 2.1, PanelRight - PanelLeft - 18, 2.6, 0.4, B5AtariMesh.Plastic(_Slot));
        }

        static void AddDial(Model3DGroup p, double x, double y, double radius, double height, string label)
        {
            Material knob = B5AtariMesh.Plastic(_Knob);
            B5AtariMesh.CylinderZ(p, _Place, x, y, radius + 4, 2, 5, B5AtariMesh.Metal(_Chrome));
            B5AtariMesh.CylinderZ(p, _Place, x, y, radius, 5, height, knob);
            B5AtariMesh.CylinderZ(p, _Place, x, y, radius * 0.55, height, height + 3, knob);
            for (int i = 0; i < 12; i++)
            {
                Matrix3D rib = Matrix3D.Identity;
                rib.Rotate(new Quaternion(new Vector3D(0, 0, 1), i * 30));
                rib.Translate(new Vector3D(x, y, 0));
                B5AtariMesh.Box(p, rib * _Place, radius, 0, (5 + height) / 2, 3, 5, height - 6, knob);
            }
            B5AtariMesh.Box(p, _Place, x, y + radius * 0.5, height + 3.2, 2.4, radius * 0.7, 0.4, B5AtariMesh.Plastic(Color.FromRgb(225, 222, 210)));
            B5AtariMesh.Text(p, _Place, label, _Print, new Point3D(x - radius - 4, y + radius + 3, 2.2), new Vector3D(1, 0, 0), new Vector3D(0, 1, 0), 5);
        }

        // Tapered housing of the picture tube behind the cabinet, with ventilation slits on the back.
        static void BuildRear(Model3DGroup p)
        {
            Material rear = B5AtariMesh.Plastic(_Rear);
            double middle = (Left + Right) / 2 - 20;
            List<Point> profile = new List<Point>
            {
                new Point(-Depth, Bottom + 20), new Point(-Depth, Top - 30), new Point(-Depth - 110, Top - 90), new Point(-Depth - 110, Bottom + 50)
            };
            B5AtariMesh.Extrude(p, _Place, profile, middle - 200, middle + 200, true, rear);
            B5AtariMesh.Box(p, _Place, middle, 180, -Depth - 118, 270, 140, 16, rear);
            for (double y = 130; y <= 230; y += 12)
                B5AtariMesh.Box(p, _Place, middle, y, -Depth - 126.3, 200, 4, 0.6, B5AtariMesh.Plastic(_Slot));
            B5AtariMesh.Box(p, _Place, middle + 150, 120, -Depth - 112, 30, 30, 4, B5AtariMesh.Metal(_Chrome));
        }

        // Rabbit-ear antenna: a round base on the cabinet and two telescopic rods in a V, leaning back a little.
        static void BuildAntenna(Model3DGroup p)
        {
            double x = (Left + Right) / 2 + 30, z = -150;
            Material dark = B5AtariMesh.Plastic(_Knob);
            Material chrome = B5AtariMesh.Metal(_Chrome);
            Matrix3D baseTurn = Matrix3D.Identity;
            baseTurn.Rotate(new Quaternion(new Vector3D(1, 0, 0), -90));
            baseTurn.Translate(new Vector3D(x, Top, z));
            B5AtariMesh.CylinderZ(p, baseTurn * _Place, 0, 0, 34, 0, 2, dark);
            B5AtariMesh.CylinderZ(p, baseTurn * _Place, 0, 0, 26, 2, 12, dark);
            B5AtariMesh.CylinderZ(p, baseTurn * _Place, 0, 0, 8, 12, 20, chrome);

            foreach (int side in new[] { -1, 1 })
            {
                Matrix3D rod = Matrix3D.Identity;
                rod.Rotate(new Quaternion(new Vector3D(0, 0, 1), -side * 32));
                rod.Rotate(new Quaternion(new Vector3D(1, 0, 0), -14));
                rod.Translate(new Vector3D(x, Top + 18, z));
                Matrix3D place = rod * _Place;
                double[] thickness = { 5, 4, 3, 2.4 };
                double length = 0;
                foreach (double t in thickness)
                {
                    B5AtariMesh.Box(p, place, 0, length + 45, 0, t, 92, t, chrome);
                    length += 88;
                }
                B5AtariMesh.Box(p, place, 0, length + 6, 0, 8, 8, 8, chrome);
            }
        }

        // Curved glass of the picture tube as a grid; the edges lie on the bezel, the centre bulges forward.
        internal static MeshGeometry3D CreateScreenMesh()
        {
            const int columns = 16, rows = 12;
            MeshGeometry3D mesh = new MeshGeometry3D();
            double halfW = ScreenWidth / 2, halfH = ScreenHeight / 2;
            for (int row = 0; row <= rows; row++)
            {
                for (int column = 0; column <= columns; column++)
                {
                    double u = (double)column / columns, v = (double)row / rows;
                    double x = -halfW + u * ScreenWidth, y = halfH - v * ScreenHeight;
                    double fx = 1 - (x / halfW) * (x / halfW), fy = 1 - (y / halfH) * (y / halfH);
                    Vector3D normal = new Vector3D(Bulge * 2 * x / (halfW * halfW) * fy, Bulge * 2 * y / (halfH * halfH) * fx, 1);
                    normal.Normalize();
                    B5AtariMesh.AddVertex(mesh, _Place, new Point3D(x, ScreenY + y, GlassEdgeZ + Bulge * fx * fy), normal);
                    mesh.TextureCoordinates.Add(new Point(u, v));
                }
            }
            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int a = row * (columns + 1) + column, b = a + 1, c = a + columns + 1, d = c + 1;
                    B5AtariMesh.AddTriangle(mesh, a, c, b, new Vector3D(0, 0, 1));
                    B5AtariMesh.AddTriangle(mesh, b, c, d, new Vector3D(0, 0, 1));
                }
            }
            mesh.Freeze();
            return mesh;
        }

        // Switched-off tube: dark grey-green glass with a strong highlight.
        internal static Material CreateScreenOffMaterial()
        {
            LinearGradientBrush glass = new LinearGradientBrush(Color.FromRgb(52, 58, 56), Color.FromRgb(16, 19, 18), new Point(0.15, 0), new Point(0.85, 1));
            MaterialGroup material = new MaterialGroup();
            material.Children.Add(new DiffuseMaterial(glass));
            material.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromRgb(200, 205, 205)), 70));
            material.Freeze();
            return material;
        }

        // Switched-on tube: a glowing attract screen like the console's title screen, with scanlines and dark corners.
        internal static Material CreateScreenOnMaterial(Color background)
        {
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                Rect area = new Rect(0, 0, 320, 240);
                RadialGradientBrush glow = new RadialGradientBrush(Mix(background, Colors.White, 0.22), background) { RadiusX = 0.7, RadiusY = 0.7 };
                context.DrawRectangle(glow, null, area);
                Typeface font = new Typeface(new FontFamily("Consolas"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
                DrawText(context, "INSERT COIN", font, 30, Colors.White, new Point(28, 34));
                DrawText(context, "ATARI VCS", font, 22, Color.FromRgb(255, 221, 110), new Point(28, 92));
                Color[] bars = { Color.FromRgb(232, 78, 60), Color.FromRgb(74, 145, 221), Color.FromRgb(240, 178, 72) };
                for (int i = 0; i < bars.Length; i++)
                    context.DrawRectangle(new SolidColorBrush(bars[i]), null, new Rect(28 + i * 92, 160, 80, 16));
                for (double y = 0; y < area.Height; y += 3)
                    context.DrawRectangle(new SolidColorBrush(Color.FromArgb(70, 0, 0, 0)), null, new Rect(0, y, area.Width, 1));
                RadialGradientBrush vignette = new RadialGradientBrush(Color.FromArgb(0, 0, 0, 0), Color.FromArgb(150, 0, 0, 0))
                {
                    RadiusX = 0.75,
                    RadiusY = 0.75
                };
                vignette.GradientStops[0].Offset = 0.55;
                context.DrawRectangle(vignette, null, area);
            }
            DrawingBrush brush = new DrawingBrush(drawing) { Stretch = Stretch.Fill };
            B5RoomTextures.CacheAsBitmap(brush);
            brush.Freeze();

            MaterialGroup material = new MaterialGroup();
            material.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.Black)));
            material.Children.Add(new EmissiveMaterial(brush));
            material.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromRgb(120, 125, 125)), 70));
            material.Freeze();
            return material;
        }

        static void DrawText(DrawingContext context, string text, Typeface font, double size, Color color, Point origin)
        {
            context.DrawText(new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, font, size,
                new SolidColorBrush(color), 1.0), origin);
        }

        static Color Mix(Color first, Color second, double amount)
        {
            return Color.FromRgb(
                (byte)(first.R + (second.R - first.R) * amount),
                (byte)(first.G + (second.G - first.G) * amount),
                (byte)(first.B + (second.B - first.B) * amount));
        }

        static Matrix3D CreatePlacement()
        {
            Matrix3D place = Matrix3D.Identity;
            place.Scale(new Vector3D(Scale, Scale, Scale));
            place.Translate(new Vector3D(CenterX, TableTop, FrontZ));
            return place;
        }
    }
}
