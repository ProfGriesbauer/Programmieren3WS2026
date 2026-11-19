using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // Mesh helpers for the Atari console and its cartridges. Shapes are given in millimetres and baked
    // into room coordinates with a placement matrix, so static parts can still be merged by B5RoomPaint.
    internal static class B5AtariMesh
    {
        static readonly Dictionary<Color, Material> MetalCache = new Dictionary<Color, Material>();
        static readonly Typeface LabelFont = new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

        internal static Material Plastic(Color color)
        {
            return B5RoomPaint.GetBoxMaterial(color);
        }

        internal static Material Metal(Color color)
        {
            Material material;
            if (MetalCache.TryGetValue(color, out material)) return material;

            MaterialGroup group = new MaterialGroup();
            group.Children.Add(new DiffuseMaterial(new SolidColorBrush(color)));
            group.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromRgb(235, 235, 235)), 45));
            group.Freeze();
            MetalCache.Add(color, group);
            return group;
        }

        internal static Material Textured(Brush brush)
        {
            MaterialGroup group = new MaterialGroup();
            group.Children.Add(new DiffuseMaterial(brush));
            group.Children.Add(new SpecularMaterial(new SolidColorBrush(Color.FromRgb(40, 36, 32)), 25));
            group.Freeze();
            return group;
        }

        internal static GeometryModel3D Box(Model3DGroup parent, Matrix3D place, double x, double y, double z,
            double width, double height, double depth, Material material)
        {
            List<Point> outline = new List<Point>
            {
                new Point(x - width / 2, y - height / 2), new Point(x + width / 2, y - height / 2),
                new Point(x + width / 2, y + height / 2), new Point(x - width / 2, y + height / 2)
            };
            return Extrude(parent, place, outline, z - depth / 2, z + depth / 2, false, material);
        }

        internal static GeometryModel3D CylinderZ(Model3DGroup parent, Matrix3D place, double x, double y,
            double radius, double zBack, double zFront, Material material)
        {
            const int segments = 24;
            List<Point> outline = new List<Point>();
            for (int i = 0; i < segments; i++)
            {
                double angle = 2 * Math.PI * i / segments;
                outline.Add(new Point(x + radius * Math.Cos(angle), y + radius * Math.Sin(angle)));
            }
            return Extrude(parent, place, outline, zBack, zFront, false, material);
        }

        // Extrudes a convex outline. alongX = false: outline is (x, y), extruded from z0 to z1.
        // alongX = true: outline is a side profile (z, y), extruded from x0 to x1.
        // Each face gets the whole texture, so textured materials (wood) can be used too.
        internal static GeometryModel3D Extrude(Model3DGroup parent, Matrix3D place, IList<Point> outline,
            double from, double to, bool alongX, Material material)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            Point center = new Point();
            Rect bounds = Rect.Empty;
            foreach (Point point in outline)
            {
                center = new Point(center.X + point.X / outline.Count, center.Y + point.Y / outline.Count);
                bounds.Union(point);
            }

            foreach (double w in new[] { from, to })
            {
                Vector3D normal = Map(0, 0, w < (from + to) / 2 ? -1 : 1, alongX);
                int start = mesh.Positions.Count;
                AddVertex(mesh, place, MapPoint(center.X, center.Y, w, alongX), normal);
                mesh.TextureCoordinates.Add(CapCoordinate(center, bounds));
                foreach (Point point in outline)
                {
                    AddVertex(mesh, place, MapPoint(point.X, point.Y, w, alongX), normal);
                    mesh.TextureCoordinates.Add(CapCoordinate(point, bounds));
                }
                for (int i = 0; i < outline.Count; i++)
                    AddTriangle(mesh, start, start + 1 + i, start + 1 + (i + 1) % outline.Count, normal);
            }

            for (int i = 0; i < outline.Count; i++)
            {
                Point a = outline[i];
                Point b = outline[(i + 1) % outline.Count];
                Vector edge = b - a;
                Vector side = new Vector(edge.Y, -edge.X);
                if (side * (a - center) < 0) side = -side;
                side.Normalize();
                Vector3D normal = Map(side.X, side.Y, 0, alongX);
                int start = mesh.Positions.Count;
                AddVertex(mesh, place, MapPoint(a.X, a.Y, from, alongX), normal);
                AddVertex(mesh, place, MapPoint(b.X, b.Y, from, alongX), normal);
                AddVertex(mesh, place, MapPoint(b.X, b.Y, to, alongX), normal);
                AddVertex(mesh, place, MapPoint(a.X, a.Y, to, alongX), normal);
                mesh.TextureCoordinates.Add(new Point(0, 1));
                mesh.TextureCoordinates.Add(new Point(1, 1));
                mesh.TextureCoordinates.Add(new Point(1, 0));
                mesh.TextureCoordinates.Add(new Point(0, 0));
                AddTriangle(mesh, start, start + 1, start + 2, normal);
                AddTriangle(mesh, start, start + 2, start + 3, normal);
            }
            return AddModel(parent, mesh, material);
        }

        static Point CapCoordinate(Point point, Rect bounds)
        {
            return new Point((point.X - bounds.Left) / Math.Max(bounds.Width, 1e-9), (bounds.Bottom - point.Y) / Math.Max(bounds.Height, 1e-9));
        }

        // Flat rectangle around center, spanned by the unit vectors right and up; texture (0,0) is its top-left corner.
        internal static GeometryModel3D Quad(Model3DGroup parent, Matrix3D place, Point3D center, Vector3D right, Vector3D up,
            double width, double height, Material material)
        {
            Vector3D normal = Vector3D.CrossProduct(right, up);
            normal.Normalize();
            Vector3D halfRight = right * (width / 2);
            Vector3D halfUp = up * (height / 2);
            MeshGeometry3D mesh = new MeshGeometry3D();
            AddVertex(mesh, place, center - halfRight - halfUp, normal);
            AddVertex(mesh, place, center + halfRight - halfUp, normal);
            AddVertex(mesh, place, center + halfRight + halfUp, normal);
            AddVertex(mesh, place, center - halfRight + halfUp, normal);
            mesh.TextureCoordinates = new PointCollection { new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0) };
            AddTriangle(mesh, 0, 1, 2, place.Transform(normal));
            AddTriangle(mesh, 0, 2, 3, place.Transform(normal));
            mesh.Freeze();
            GeometryModel3D model = new GeometryModel3D(mesh, material);
            parent.Children.Add(model);
            return model;
        }

        // Printed text: a quad of the given letter height whose width follows the text.
        internal static void Text(Model3DGroup parent, Matrix3D place, string text, Color color, Point3D center,
            Vector3D right, Vector3D up, double height)
        {
            FormattedText formatted = new FormattedText(text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                LabelFont, 100, Brushes.Black, 1.0);
            Geometry geometry = formatted.BuildGeometry(new Point(0, 0));
            Rect bounds = geometry.Bounds;
            DrawingBrush brush = new DrawingBrush(new GeometryDrawing(new SolidColorBrush(color), null, geometry));
            B5RoomTextures.CacheAsBitmap(brush);
            brush.Freeze();
            DiffuseMaterial material = new DiffuseMaterial(brush);
            material.Freeze();
            Quad(parent, place, center, right, up, height * bounds.Width / bounds.Height, height, material);
        }

        // Rounded rectangle outline (x, y) with its bottom edge on y = 0.
        internal static List<Point> RoundedRect(double width, double height, double topRadius, double bottomRadius)
        {
            List<Point> outline = new List<Point>();
            AddCorner(outline, width / 2 - bottomRadius, bottomRadius, bottomRadius, 270);
            AddCorner(outline, width / 2 - topRadius, height - topRadius, topRadius, 0);
            AddCorner(outline, -width / 2 + topRadius, height - topRadius, topRadius, 90);
            AddCorner(outline, -width / 2 + bottomRadius, bottomRadius, bottomRadius, 180);
            return outline;
        }

        static void AddCorner(List<Point> outline, double cx, double cy, double radius, double startAngle)
        {
            const int segments = 4;
            for (int i = 0; i <= segments; i++)
            {
                double angle = (startAngle + 90.0 * i / segments) * Math.PI / 180;
                outline.Add(new Point(cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle)));
            }
        }

        static Point3D MapPoint(double u, double v, double w, bool alongX)
        {
            return alongX ? new Point3D(w, v, u) : new Point3D(u, v, w);
        }

        static Vector3D Map(double u, double v, double w, bool alongX)
        {
            return alongX ? new Vector3D(w, v, u) : new Vector3D(u, v, w);
        }

        internal static void AddVertex(MeshGeometry3D mesh, Matrix3D place, Point3D point, Vector3D normal)
        {
            mesh.Positions.Add(place.Transform(point));
            Vector3D placed = place.Transform(normal);
            placed.Normalize();
            mesh.Normals.Add(placed);
        }

        // Orders the triangle so that its front side faces along normal; WPF lights back faces differently.
        internal static void AddTriangle(MeshGeometry3D mesh, int a, int b, int c, Vector3D normal)
        {
            Vector3D face = Vector3D.CrossProduct(mesh.Positions[b] - mesh.Positions[a], mesh.Positions[c] - mesh.Positions[a]);
            bool flip = Vector3D.DotProduct(face, normal) < 0;
            mesh.TriangleIndices.Add(a);
            mesh.TriangleIndices.Add(flip ? c : b);
            mesh.TriangleIndices.Add(flip ? b : c);
        }

        internal static GeometryModel3D AddModel(Model3DGroup parent, MeshGeometry3D mesh, Material material)
        {
            mesh.Freeze();
            GeometryModel3D model = new GeometryModel3D(mesh, material) { BackMaterial = material };
            parent.Children.Add(model);
            return model;
        }
    }
}
