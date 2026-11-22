using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // 3D model of the Atari 2600 ("six switch" version) on the lower shelf of the TV stand.
    // Measurements are millimetres of the real console (340 x 230 mm): x to the right from the centre,
    // y up from the shelf, d backwards from the front edge. Wedge body with a wood-grain front, ribbed top,
    // six metal switches and a raised cartridge slot with two spring flaps.
    public static class B5AtariConsoleModel
    {
        internal const double Scale = 0.0032;
        internal const double ShelfTop = 0.235;
        const double CenterX = -3.6, FrontZ = -1.37;
        const double Width = 340, Depth = 230;

        // Cartridge slot on top of the raised housing.
        internal const double SlotTop = 76, SlotFront = 120, SlotBack = 144, SlotHalfWidth = 44;
        internal const double SlotCenter = (SlotFront + SlotBack) / 2;

        static readonly Color _Body = Color.FromRgb(32, 30, 29);
        static readonly Color _BodyEdge = Color.FromRgb(46, 44, 42);
        static readonly Color _Hole = Color.FromRgb(7, 7, 8);
        static readonly Color _Silver = Color.FromRgb(196, 196, 200);
        static readonly Color _Print = Color.FromRgb(214, 210, 200);
        static readonly Color _Rubber = Color.FromRgb(16, 16, 16);

        static readonly Matrix3D _Place = CreatePlacement();
        internal static readonly Brush WoodGrain = CreateWoodGrain();

        // Switch positions from left to right; the power switch is the leftmost one and is animated.
        internal static readonly double[] SwitchX = { -146, -116, -86, 86, 116, 146 };
        internal const double SwitchD = 47;
        static readonly string[] SwitchLabels = { "POWER", "TV TYPE", "LEFT DIFF.", "RIGHT DIFF.", "SELECT", "RESET" };
        static readonly double[] SwitchAngles = { 0, 28, -28, 28, -28, -28 };

        internal static Matrix3D Placement { get { return _Place; } }
        public static Point3D SlotCenterInRoom { get { return _Place.Transform(new Point3D(0, SlotTop, -SlotCenter)); } }

        public static void Build(Model3DGroup parent)
        {
            BuildBody(parent);
            BuildSlotHousing(parent);
            BuildTopRibs(parent);
            BuildSwitches(parent);
            BuildCables(parent);
        }

        // Top surface of the main body at depth d (it rises slightly towards the back).
        internal static double TopAt(double d)
        {
            return 54 + 8 * (d - 32) / (Depth - 32);
        }

        static Point P(double d, double y)
        {
            return new Point(-d, y);
        }

        static void BuildBody(Model3DGroup p)
        {
            Material body = B5AtariMesh.Plastic(_Body);
            B5AtariMesh.Extrude(p, _Place, new[] { P(0, 3), P(Depth, 3), P(Depth, TopAt(Depth)), P(32, TopAt(32)), P(0, 18) },
                -Width / 2, Width / 2, true, body);

            // Rubber feet lift the body a little off the shelf.
            foreach (double x in new[] { -140.0, 140.0 })
                foreach (double d in new[] { 22.0, 208.0 })
                    B5AtariMesh.Box(p, _Place, x, 1.5, -d, 22, 3, 14, B5AtariMesh.Plastic(_Rubber));

            // Wood-grain front panel on the slope, with a thin chrome strip along its upper edge.
            Vector3D up = new Vector3D(0, 36, -32);
            up.Normalize();
            Vector3D outward = new Vector3D(0, 32, 36);
            outward.Normalize();
            Point3D middle = new Point3D(0, 36, -16) + outward * 0.4;
            B5AtariMesh.Quad(p, _Place, middle, new Vector3D(1, 0, 0), up, Width - 2, Math.Sqrt(32 * 32 + 36 * 36) - 2,
                B5AtariMesh.Textured(WoodGrain));
            B5AtariMesh.Box(p, _Place, 0, TopAt(32) - 0.4, -32.5, Width - 4, 1.6, 2.2, B5AtariMesh.Metal(_Silver));
            B5AtariMesh.Box(p, _Place, 0, 17.5, 0.2, Width - 4, 1.2, 1.2, B5AtariMesh.Metal(_Silver));
        }

        // Raised housing in the middle of the top. It is built around the slot opening so the slot is a real hole.
        static void BuildSlotHousing(Model3DGroup p)
        {
            Material body = B5AtariMesh.Plastic(_Body);
            Material edge = B5AtariMesh.Plastic(_BodyEdge);
            const double halfWidth = 64;
            B5AtariMesh.Extrude(p, _Place, new[] { P(56, 50), P(SlotFront, 50), P(SlotFront, SlotTop), P(88, SlotTop), P(56, 60) },
                -halfWidth, halfWidth, true, body);
            B5AtariMesh.Extrude(p, _Place, new[] { P(SlotBack, 50), P(176, 50), P(176, SlotTop), P(SlotBack, SlotTop) },
                -halfWidth, halfWidth, true, body);
            foreach (int side in new[] { -1, 1 })
            {
                B5AtariMesh.Box(p, _Place, side * (SlotHalfWidth + halfWidth) / 2, (50 + SlotTop) / 2, -SlotCenter,
                    halfWidth - SlotHalfWidth, SlotTop - 50, SlotBack - SlotFront, body);
            }

            // Dark lining inside the slot and a slightly lighter rim around it.
            double floor = TopAt(SlotCenter) + 0.3;
            B5AtariMesh.Box(p, _Place, 0, floor, -SlotCenter, SlotHalfWidth * 2, 0.4, SlotBack - SlotFront, B5AtariMesh.Plastic(_Hole));
            B5AtariMesh.Box(p, _Place, 0, (floor + SlotTop) / 2, -SlotFront - 0.3, SlotHalfWidth * 2, SlotTop - floor, 0.4, B5AtariMesh.Plastic(_Hole));
            B5AtariMesh.Box(p, _Place, 0, (floor + SlotTop) / 2, -SlotBack + 0.3, SlotHalfWidth * 2, SlotTop - floor, 0.4, B5AtariMesh.Plastic(_Hole));
            const double rim = 3;
            B5AtariMesh.Box(p, _Place, 0, SlotTop + 0.4, -SlotFront + rim / 2, SlotHalfWidth * 2 + rim * 2, 0.8, rim, edge);
            B5AtariMesh.Box(p, _Place, 0, SlotTop + 0.4, -SlotBack - rim / 2, SlotHalfWidth * 2 + rim * 2, 0.8, rim, edge);
            B5AtariMesh.Box(p, _Place, -SlotHalfWidth - rim / 2, SlotTop + 0.4, -SlotCenter, rim, 0.8, SlotBack - SlotFront, edge);
            B5AtariMesh.Box(p, _Place, SlotHalfWidth + rim / 2, SlotTop + 0.4, -SlotCenter, rim, 0.8, SlotBack - SlotFront, edge);

            // Logo on the sloped front of the housing.
            Vector3D up = new Vector3D(0, 16, -32);
            up.Normalize();
            Vector3D outward = new Vector3D(0, 32, 16);
            outward.Normalize();
            Point3D logo = new Point3D(0, 60 + 16 * 0.62, -(56 + 32 * 0.62)) + outward * 0.3;
            B5AtariMesh.Text(p, _Place, "ATARI", _Silver, logo, new Vector3D(1, 0, 0), up, 9);
            Point3D subtitle = new Point3D(0, 60 + 16 * 0.22, -(56 + 32 * 0.22)) + outward * 0.3;
            B5AtariMesh.Text(p, _Place, "VIDEO COMPUTER SYSTEM", _Print, subtitle, new Vector3D(1, 0, 0), up, 3.2);
        }

        // Ridges across the top left and right of the housing, like the moulded ribs of the real case.
        static void BuildTopRibs(Model3DGroup p)
        {
            Material rib = B5AtariMesh.Plastic(_BodyEdge);
            for (double d = 66; d <= 222; d += 7)
            {
                foreach (int side in new[] { -1, 1 })
                    B5AtariMesh.Box(p, _Place, side * 116, TopAt(d) + 0.6, -d, 94, 1.4, 2.6, rib);
            }
        }

        static void BuildSwitches(Model3DGroup p)
        {
            Vector3D flatUp = new Vector3D(0, 8, -(Depth - 32));
            flatUp.Normalize();
            for (int i = 0; i < SwitchX.Length; i++)
            {
                double x = SwitchX[i];
                double top = TopAt(SwitchD);
                B5AtariMesh.Box(p, _Place, x, top + 0.5, -SwitchD - 1, 16, 1.0, 16, B5AtariMesh.Plastic(_BodyEdge));
                B5AtariMesh.Box(p, _Place, x, top + 1.05, -SwitchD - 1, 5, 0.2, 11, B5AtariMesh.Plastic(_Hole));
                B5AtariMesh.Text(p, _Place, SwitchLabels[i], _Print, new Point3D(x, TopAt(36.2) + 0.1, -36.2),
                    new Vector3D(1, 0, 0), flatUp, 3.0);
                if (i > 0) AddLever(p, _Place, x, SwitchAngles[i]);
            }
        }

        // Metal toggle lever standing in a switch, tilted forwards (positive angle) or backwards.
        internal static void AddLever(Model3DGroup parent, Matrix3D place, double x, double angle)
        {
            Matrix3D tilt = Matrix3D.Identity;
            tilt.Rotate(new Quaternion(new Vector3D(1, 0, 0), angle));
            tilt.Translate(new Vector3D(x, TopAt(SwitchD) + 1, -SwitchD));
            Matrix3D lever = tilt * place;
            Material metal = B5AtariMesh.Metal(_Silver);
            B5AtariMesh.Box(parent, lever, 0, 9, 0, 3.4, 18, 3.4, metal);
            B5AtariMesh.Box(parent, lever, 0, 19, 0, 6, 5, 6, metal);
        }

        // Lever of the power switch as its own model so it can be flipped; it pivots in its switch.
        internal static Model3DGroup CreatePowerLever(out AxisAngleRotation3D rotation)
        {
            Model3DGroup lever = new Model3DGroup();
            AddLever(lever, Matrix3D.Identity, 0, 0);
            rotation = new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0);
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new RotateTransform3D(rotation, new Point3D(0, TopAt(SwitchD) + 1, -SwitchD)));
            transform.Children.Add(new TranslateTransform3D(SwitchX[0], 0, 0));
            transform.Children.Add(new MatrixTransform3D(_Place));
            lever.Transform = transform;
            return lever;
        }

        // Spring flap of the slot, hinged along its outer edge. front = the flap at the front edge of the slot.
        internal static Model3DGroup CreateFlap(bool front, out AxisAngleRotation3D rotation)
        {
            Model3DGroup flap = new Model3DGroup();
            double hinge = front ? SlotFront : SlotBack;
            double length = (SlotBack - SlotFront) / 2 - 0.4;
            double centerD = front ? hinge + length / 2 : hinge - length / 2;
            B5AtariMesh.Box(flap, Matrix3D.Identity, 0, SlotTop - 1.2, -centerD, SlotHalfWidth * 2 - 1, 1.2, length,
                B5AtariMesh.Plastic(Color.FromRgb(38, 37, 36)));
            rotation = new AxisAngleRotation3D(new Vector3D(1, 0, 0), 0);
            Transform3DGroup transform = new Transform3DGroup();
            transform.Children.Add(new RotateTransform3D(rotation, new Point3D(0, SlotTop - 0.6, -hinge)));
            transform.Children.Add(new MatrixTransform3D(_Place));
            flap.Transform = transform;
            return flap;
        }

        static void BuildCables(Model3DGroup p)
        {
            // TV cable out of the back, over the rear edge of the shelf and down to the floor.
            Material cable = B5AtariMesh.Plastic(Color.FromRgb(26, 26, 28));
            B5AtariMesh.Box(p, _Place, 60, 12, -(Depth + 285) / 2, 5, 5, 285 - Depth, cable);
            B5AtariMesh.Box(p, _Place, 60, (12 - ShelfTop / Scale) / 2, -287, 5, 12 + ShelfTop / Scale, 5, cable);
        }

        static Matrix3D CreatePlacement()
        {
            Matrix3D place = Matrix3D.Identity;
            place.Scale(new Vector3D(Scale, Scale, Scale));
            place.Translate(new Vector3D(CenterX, ShelfTop, FrontZ));
            return place;
        }

        // Dark walnut veneer with fine horizontal grain.
        static Brush CreateWoodGrain()
        {
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                context.DrawRectangle(new SolidColorBrush(Color.FromRgb(104, 63, 35)), null, new Rect(0, 0, 512, 64));
                Random random = new Random(26);
                for (int i = 0; i < 46; i++)
                {
                    double y = random.NextDouble() * 64;
                    double wave = 0.6 + random.NextDouble() * 1.6;
                    StreamGeometry line = new StreamGeometry();
                    using (StreamGeometryContext path = line.Open())
                    {
                        path.BeginFigure(new Point(0, y), false, false);
                        for (int x = 64; x <= 512; x += 64)
                            path.BezierTo(new Point(x - 42, y - wave), new Point(x - 22, y + wave), new Point(x, y + (random.NextDouble() - 0.5) * 0.8), true, true);
                    }
                    Color color = i % 3 == 0 ? Color.FromArgb(150, 136, 88, 52) : Color.FromArgb(120, 58, 32, 17);
                    context.DrawGeometry(null, new Pen(new SolidColorBrush(color), 0.35 + random.NextDouble() * 0.9), line);
                }
            }
            DrawingBrush brush = new DrawingBrush(drawing) { Stretch = Stretch.Fill };
            Brush texture = B5RoomTextures.ToBitmap(brush);
            return texture;
        }
    }
}
