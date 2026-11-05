using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // 3D model of the Game Boy (DMG-01) standing on the shelf.
    // All measurements are millimetres of the real device (90 x 148 x 32 mm), taken from the top-left
    // corner of the front face: x to the right, y downwards, z out of the front face.
    public static class B5GameBoyModel
    {
        const double Scale = 0.0042;
        const double BodyWidth = 90, BodyHeight = 148, BodyDepth = 32;
        const double LcdLeft = 21.5, LcdTop = 22, LcdWidth = 47, LcdHeight = 43, LcdLayer = 2;
        const double ButtonAngle = 26.57;

        static readonly Point3D _FrontTopLeft = new Point3D(0.8 - BodyWidth * Scale / 2, 1.6 + BodyHeight * Scale, -2.44);

        static readonly Color _Body = Color.FromRgb(201, 198, 184);
        static readonly Color _Recess = Color.FromRgb(184, 181, 168);
        static readonly Color _Groove = Color.FromRgb(158, 155, 143);
        static readonly Color _Bezel = Color.FromRgb(86, 88, 102);
        static readonly Color _Maroon = Color.FromRgb(143, 35, 75);
        static readonly Color _Navy = Color.FromRgb(38, 47, 105);
        static readonly Color _DPad = Color.FromRgb(38, 40, 39);
        static readonly Color _Rubber = Color.FromRgb(105, 103, 99);
        static readonly Color _Speaker = Color.FromRgb(72, 72, 68);

        static readonly Typeface _LogoFont = new Typeface(new FontFamily("Arial"), FontStyles.Italic, FontWeights.Bold, FontStretches.Normal);
        static readonly Typeface _LabelFont = new Typeface(new FontFamily("Arial"), FontStyles.Italic, FontWeights.Normal, FontStretches.Normal);

        public static Point3D Center { get { return ToRoom(BodyWidth / 2, BodyHeight / 2, -BodyDepth / 2); } }
        public static Point3D ScreenCenter { get { return ToRoom(LcdLeft + LcdWidth / 2, LcdTop + LcdHeight / 2, LcdLayer); } }
        public static double ScreenWidth { get { return LcdWidth * Scale; } }
        public static double ScreenHeight { get { return LcdHeight * Scale; } }

        public static void Build(Model3DGroup parent)
        {
            BuildBody(parent);
            BuildScreenArea(parent);
            BuildControls(parent);
        }

        static void BuildBody(Model3DGroup parent)
        {
            // The large rounded bottom-right corner is the typical Game Boy silhouette.
            AddPlate(parent, 45, 74, BodyWidth, BodyHeight, 4, 4, 22, 4, 0, -BodyDepth, 0, _Body);
            AddPlate(parent, 45, 6.5, 82, 0.8, 0.4, 0, -0.5, 1, _Groove);
            AddPlate(parent, 22, -0.8, 10, 2.5, 0.8, 0, -22, -10, _Groove);
            AddLabel(parent, "Nintendo GAME BOY", _LogoFont, _Navy, 8, 76.5, 4.2, 0, 0, 1);

            // Speaker grille in the bottom-right corner.
            for (int i = 0; i < 6; i++)
                AddPlate(parent, 66 + i * 3.4, 131, 13, 1.8, 0.9, 60, -0.5, 1, _Speaker);
        }

        static void BuildScreenArea(Model3DGroup parent)
        {
            AddPlate(parent, 45, 41, 76, 58, 3, 3, 12, 3, 0, -0.5, 1, _Bezel);
            AddPlate(parent, 19, 16, 16, 0.9, 0.45, 0, 0.5, 2, _Maroon);
            AddPlate(parent, 71, 16, 16, 0.9, 0.45, 0, 0.5, 2, _Maroon);
            AddPlate(parent, 19, 17.6, 16, 0.9, 0.45, 0, 0.5, 2, _Navy);
            AddPlate(parent, 71, 17.6, 16, 0.9, 0.45, 0, 0.5, 2, _Navy);
            AddLabel(parent, "DOT MATRIX WITH STEREO SOUND", _LabelFont, Color.FromRgb(176, 176, 188), 45, 16.8, 1.3, 0.5, 0, 2);
            AddDisc(parent, 13, 37, 1.6, 0.5, 2, Color.FromRgb(120, 22, 28));
            AddLabel(parent, "BATTERY", _LabelFont, Color.FromRgb(176, 176, 188), 13, 41.5, 1.2, 0.5, 0, 2);
        }

        static void BuildControls(Model3DGroup parent)
        {
            // Direction pad in a round recess.
            AddDisc(parent, 20, 100, 13, -0.5, 1, _Recess);
            AddPlate(parent, 20, 100, 23, 7.5, 1.2, 0, -0.5, 4, _DPad);
            AddPlate(parent, 20, 100, 7.5, 23, 1.2, 0, -0.5, 4, _DPad);
            AddDisc(parent, 20, 100, 2.4, 3.5, 5, Color.FromRgb(60, 62, 61));

            // A and B sit in a slanted, capsule-shaped recess; B is the lower left one.
            AddPlate(parent, 64, 99.5, 33, 13, 6.5, ButtonAngle, -0.5, 1, _Recess);
            AddDisc(parent, 73, 95, 5, -0.5, 3.5, _Maroon);
            AddDisc(parent, 55, 104, 5, -0.5, 3.5, _Maroon);
            AddLabel(parent, "A", _LogoFont, _Navy, 73, 103.5, 2.4, 0.5, 0, 1.5);
            AddLabel(parent, "B", _LogoFont, _Navy, 55, 112.5, 2.4, 0.5, 0, 1.5);

            AddStartSelectButton(parent, 38, 125, "SELECT");
            AddStartSelectButton(parent, 52, 125, "START");
        }

        static void AddStartSelectButton(Model3DGroup parent, double cx, double cy, string label)
        {
            AddPlate(parent, cx, cy, 15, 6.5, 3.25, ButtonAngle, -0.5, 1, _Recess);
            AddPlate(parent, cx, cy, 11, 3.6, 1.8, ButtonAngle, -0.5, 2.5, _Rubber);
            AddLabel(parent, label, _LabelFont, _Navy, cx + 2.4, cy + 4.8, 1.6, 0.5, ButtonAngle, 1.5);
        }

        static Point3D ToRoom(double x, double y, double z)
        {
            return new Point3D(_FrontTopLeft.X + x * Scale, _FrontTopLeft.Y - y * Scale, _FrontTopLeft.Z + z * Scale);
        }

        static void AddDisc(Model3DGroup parent, double cx, double cy, double radius, double zBack, double zFront, Color color)
        {
            AddPlate(parent, cx, cy, radius * 2, radius * 2, radius, 0, zBack, zFront, color);
        }

        static void AddPlate(Model3DGroup parent, double cx, double cy, double width, double height,
            double radius, double angle, double zBack, double zFront, Color color)
        {
            AddPlate(parent, cx, cy, width, height, radius, radius, radius, radius, angle, zBack, zFront, color);
        }

        // Extrudes a rounded rectangle along z. angle rotates it counter-clockwise around its centre (degrees).
        static void AddPlate(Model3DGroup parent, double cx, double cy, double width, double height,
            double radiusTopLeft, double radiusTopRight, double radiusBottomRight, double radiusBottomLeft,
            double angle, double zBack, double zFront, Color color)
        {
            // Outline counter-clockwise in a y-up frame around the centre, with the outward normal of each point.
            List<Point> outline = new List<Point>();
            List<Vector> normals = new List<Vector>();
            AddCorner(outline, normals, width / 2 - radiusTopRight, height / 2 - radiusTopRight, radiusTopRight, 0);
            AddCorner(outline, normals, -width / 2 + radiusTopLeft, height / 2 - radiusTopLeft, radiusTopLeft, 90);
            AddCorner(outline, normals, -width / 2 + radiusBottomLeft, -height / 2 + radiusBottomLeft, radiusBottomLeft, 180);
            AddCorner(outline, normals, width / 2 - radiusBottomRight, -height / 2 + radiusBottomRight, radiusBottomRight, 270);
            Rotate(outline, normals, angle);

            Point3D front = ToRoom(cx, cy, zFront);
            Point3D back = ToRoom(cx, cy, zBack);
            MeshGeometry3D mesh = new MeshGeometry3D();
            AddCap(mesh, outline, front, new Vector3D(0, 0, 1));
            AddCap(mesh, outline, back, new Vector3D(0, 0, -1));
            for (int i = 0; i < outline.Count; i++)
            {
                int next = (i + 1) % outline.Count;
                int start = mesh.Positions.Count;
                AddVertex(mesh, outline[i], front, new Vector3D(normals[i].X, normals[i].Y, 0));
                AddVertex(mesh, outline[next], front, new Vector3D(normals[next].X, normals[next].Y, 0));
                AddVertex(mesh, outline[i], back, new Vector3D(normals[i].X, normals[i].Y, 0));
                AddVertex(mesh, outline[next], back, new Vector3D(normals[next].X, normals[next].Y, 0));
                AddTriangle(mesh, start, start + 2, start + 3);
                AddTriangle(mesh, start, start + 3, start + 1);
            }

            DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(color));
            parent.Children.Add(new GeometryModel3D(mesh, material) { BackMaterial = material });
        }

        static void AddCorner(List<Point> outline, List<Vector> normals, double cx, double cy, double radius, double startAngle)
        {
            int segments = radius > 6 ? 8 : 3;
            for (int i = 0; i <= segments; i++)
            {
                double angle = (startAngle + 90.0 * i / segments) * Math.PI / 180;
                outline.Add(new Point(cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle)));
                normals.Add(new Vector(Math.Cos(angle), Math.Sin(angle)));
            }
        }

        static void Rotate(List<Point> points, List<Vector> normals, double angle)
        {
            double cos = Math.Cos(angle * Math.PI / 180), sin = Math.Sin(angle * Math.PI / 180);
            for (int i = 0; i < points.Count; i++)
            {
                points[i] = new Point(points[i].X * cos - points[i].Y * sin, points[i].X * sin + points[i].Y * cos);
                normals[i] = new Vector(normals[i].X * cos - normals[i].Y * sin, normals[i].X * sin + normals[i].Y * cos);
            }
        }

        // Triangle fan around the centre; every outline used here is convex or star-shaped around it.
        static void AddCap(MeshGeometry3D mesh, List<Point> outline, Point3D center, Vector3D normal)
        {
            int start = mesh.Positions.Count;
            mesh.Positions.Add(center);
            mesh.Normals.Add(normal);
            foreach (Point point in outline) AddVertex(mesh, point, center, normal);
            for (int i = 0; i < outline.Count; i++)
            {
                int a = start + 1 + i;
                int b = start + 1 + (i + 1) % outline.Count;
                if (normal.Z > 0) AddTriangle(mesh, start, a, b);
                else AddTriangle(mesh, start, b, a);
            }
        }

        static void AddVertex(MeshGeometry3D mesh, Point offsetMillimetres, Point3D center, Vector3D normal)
        {
            mesh.Positions.Add(new Point3D(center.X + offsetMillimetres.X * Scale, center.Y + offsetMillimetres.Y * Scale, center.Z));
            mesh.Normals.Add(normal);
        }

        static void AddTriangle(MeshGeometry3D mesh, int a, int b, int c)
        {
            mesh.TriangleIndices.Add(a);
            mesh.TriangleIndices.Add(b);
            mesh.TriangleIndices.Add(c);
        }

        // Flat text printed on the front face. anchorX: 0 = x is the left edge, 0.5 = x is the centre.
        static void AddLabel(Model3DGroup parent, string text, Typeface font, Color color,
            double x, double cy, double height, double anchorX, double angle, double z)
        {
            FormattedText formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                font, 100, Brushes.Black, 1.0);
            Geometry geometry = formatted.BuildGeometry(new Point(0, 0));
            Rect bounds = geometry.Bounds;
            double width = height * bounds.Width / bounds.Height;
            double left = -anchorX * width;

            List<Point> corners = new List<Point>
            {
                new Point(left, -height / 2), new Point(left + width, -height / 2),
                new Point(left + width, height / 2), new Point(left, height / 2)
            };
            List<Vector> unused = new List<Vector> { new Vector(), new Vector(), new Vector(), new Vector() };
            Rotate(corners, unused, angle);

            Point3D anchor = ToRoom(x, cy, z);
            MeshGeometry3D mesh = new MeshGeometry3D
            {
                TextureCoordinates = new PointCollection { new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0) },
                TriangleIndices = new Int32Collection { 0, 1, 2, 0, 2, 3 }
            };
            foreach (Point corner in corners) AddVertex(mesh, corner, anchor, new Vector3D(0, 0, 1));

            DrawingBrush brush = new DrawingBrush(new GeometryDrawing(new SolidColorBrush(color), null, geometry));
            brush.Freeze();
            parent.Children.Add(new GeometryModel3D(mesh, new DiffuseMaterial(brush)));
        }
    }
}
