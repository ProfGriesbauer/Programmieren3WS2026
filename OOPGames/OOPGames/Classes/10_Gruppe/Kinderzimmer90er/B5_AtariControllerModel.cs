using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // 3D model of the Atari joystick (CX40 style): black base, red fire button, stick with a round handle.
    // It lies on the shelf next to the console and is connected to it by a short cable.
    public static class B5AtariControllerModel
    {
        const double ShelfTop = 0.485;
        const double X = -2.88, Z = -1.65;
        const double BaseHeight = 0.045, PlateHeight = 0.01;

        static readonly Color _Base = Color.FromRgb(24, 24, 26);
        static readonly Color _Plate = Color.FromRgb(46, 46, 50);
        static readonly Color _Stick = Color.FromRgb(70, 70, 74);
        static readonly Color _Handle = Color.FromRgb(12, 12, 13);
        static readonly Color _Fire = Color.FromRgb(201, 49, 39);
        static readonly Color _Cable = Color.FromRgb(30, 30, 32);

        public static void Build(Model3DGroup parent)
        {
            double top = ShelfTop + BaseHeight + PlateHeight;
            B5RoomPaint.AddBox(parent, X, ShelfTop + BaseHeight / 2, Z, 0.25, BaseHeight, 0.22, _Base);
            B5RoomPaint.AddBox(parent, X, ShelfTop + BaseHeight + PlateHeight / 2, Z, 0.23, PlateHeight, 0.20, _Plate);

            // Fire button in the rear left corner of the base.
            AddCylinder(parent, X - 0.07, top, Z - 0.055, 0.026, 0.008, _Base);
            AddCylinder(parent, X - 0.07, top + 0.008, Z - 0.055, 0.021, 0.012, _Fire);

            // Stick: collar, thin shaft, round handle with a smaller cap on top.
            double stickX = X + 0.025, stickZ = Z + 0.02;
            AddCylinder(parent, stickX, top, stickZ, 0.04, 0.012, _Base);
            AddCylinder(parent, stickX, top + 0.012, stickZ, 0.012, 0.10, _Stick);
            AddCylinder(parent, stickX, top + 0.112, stickZ, 0.028, 0.07, _Handle);
            AddCylinder(parent, stickX, top + 0.182, stickZ, 0.021, 0.012, _Handle);

            // Cable from the left side of the base to the console.
            B5RoomPaint.AddBox(parent, -3.03, ShelfTop + BaseHeight / 2, -1.7, 0.06, 0.014, 0.014, _Cable);
        }

        // Upright cylinder resting on y = bottom.
        static void AddCylinder(Model3DGroup parent, double x, double bottom, double z, double radius, double height, Color color)
        {
            const int segments = 20;
            double top = bottom + height;
            MeshGeometry3D mesh = new MeshGeometry3D();
            for (int i = 0; i < segments; i++)
            {
                double a0 = 2 * Math.PI * i / segments;
                double a1 = 2 * Math.PI * (i + 1) / segments;
                Vector3D n0 = new Vector3D(Math.Cos(a0), 0, Math.Sin(a0));
                Vector3D n1 = new Vector3D(Math.Cos(a1), 0, Math.Sin(a1));
                Point3D p0 = new Point3D(x + radius * n0.X, bottom, z + radius * n0.Z);
                Point3D p1 = new Point3D(x + radius * n1.X, bottom, z + radius * n1.Z);

                int start = mesh.Positions.Count;
                AddVertex(mesh, p0, n0);
                AddVertex(mesh, p1, n1);
                AddVertex(mesh, new Point3D(p1.X, top, p1.Z), n1);
                AddVertex(mesh, new Point3D(p0.X, top, p0.Z), n0);
                AddTriangle(mesh, start, start + 2, start + 1);
                AddTriangle(mesh, start, start + 3, start + 2);

                start = mesh.Positions.Count;
                Vector3D up = new Vector3D(0, 1, 0);
                AddVertex(mesh, new Point3D(x, top, z), up);
                AddVertex(mesh, new Point3D(p1.X, top, p1.Z), up);
                AddVertex(mesh, new Point3D(p0.X, top, p0.Z), up);
                AddTriangle(mesh, start, start + 1, start + 2);
            }

            DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(color));
            parent.Children.Add(new GeometryModel3D(mesh, material) { BackMaterial = material });
        }

        static void AddVertex(MeshGeometry3D mesh, Point3D position, Vector3D normal)
        {
            mesh.Positions.Add(position);
            mesh.Normals.Add(normal);
        }

        static void AddTriangle(MeshGeometry3D mesh, int a, int b, int c)
        {
            mesh.TriangleIndices.Add(a);
            mesh.TriangleIndices.Add(b);
            mesh.TriangleIndices.Add(c);
        }
    }
}
