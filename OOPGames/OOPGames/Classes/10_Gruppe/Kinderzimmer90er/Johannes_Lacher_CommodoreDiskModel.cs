using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // 3D model of a 5.25" floppy disk (133 x 133 mm) standing upright with the label side towards +z.
    // Millimetres, origin in the middle of the bottom edge, which is the edge that goes into the drive first
    // (head window there, label sticker at the opposite edge).
    public static class B5CommodoreDiskModel
    {
        internal const double Size = 133, Thickness = 1.8;

        static readonly Color _Sleeve = Color.FromRgb(30, 30, 33);
        static readonly Color _Media = Color.FromRgb(86, 62, 42);
        static readonly Color _HubRing = Color.FromRgb(168, 160, 140);
        static readonly Color _Hole = Color.FromRgb(10, 10, 10);

        public static Model3DGroup Create(B5RoomGameEntry game)
        {
            Model3DGroup disk = new Model3DGroup();
            Matrix3D place = Matrix3D.Identity;
            double front = Thickness / 2, back = -Thickness / 2;
            B5AtariMesh.Extrude(disk, place, B5AtariMesh.RoundedRect(Size, Size, 2, 2), back, front, false, B5AtariMesh.Plastic(_Sleeve));

            // Hub opening, index hole and head window show the brown magnetic disk; they go through on both sides.
            double hubY = Size / 2;
            foreach (int side in new[] { -1, 1 })
            {
                double z = side * front;
                B5AtariMesh.CylinderZ(disk, place, 0, hubY, 19, z, z + side * 0.1, B5AtariMesh.Plastic(_Media));
                B5AtariMesh.CylinderZ(disk, place, 0, hubY, 15, z + side * 0.1, z + side * 0.2, B5AtariMesh.Plastic(_HubRing));
                B5AtariMesh.CylinderZ(disk, place, 0, hubY, 13, z + side * 0.2, z + side * 0.3, B5AtariMesh.Plastic(_Hole));
                B5AtariMesh.CylinderZ(disk, place, 26, hubY + 6, 2.5, z, z + side * 0.1, B5AtariMesh.Plastic(_Media));
                List<Point> window = new List<Point>();
                foreach (Point point in B5AtariMesh.RoundedRect(13, 36, 6.5, 6.5)) window.Add(new Point(point.X, point.Y + 7));
                B5AtariMesh.Extrude(disk, place, window, z, z + side * 0.1, false, B5AtariMesh.Plastic(_Media));
            }

            // Write-protect notch on the side edge and the label sticker near the top edge. The writing on the sticker
            // points towards the head window, so it reads the right way round when the disk lies in front of the drive.
            B5AtariMesh.Box(disk, place, Size / 2 - 2, 100, 0, 4.2, 6, Thickness + 0.4, B5AtariMesh.Plastic(_Hole));
            B5AtariMesh.Quad(disk, place, new Point3D(10, 116, front + 0.15), new Vector3D(-1, 0, 0), new Vector3D(0, -1, 0),
                96, 24, B5AtariMesh.Textured(CreateLabel(game)));
            return disk;
        }

        // White sticker with a coloured stripe at the top and the title written by hand in blue ink.
        static Brush CreateLabel(B5RoomGameEntry game)
        {
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                context.DrawRectangle(new SolidColorBrush(Color.FromRgb(242, 240, 232)), null, new Rect(0, 0, 384, 96));
                context.DrawRectangle(new SolidColorBrush(game.CartridgeColor), null, new Rect(0, 0, 384, 14));
                Pen line = new Pen(new SolidColorBrush(Color.FromRgb(170, 190, 215)), 1.2);
                for (double y = 40; y < 96; y += 22) context.DrawLine(line, new Point(8, y), new Point(376, y));
                B5MediaArt.DrawFitted(context, game.Title, B5MediaArt.HandFont, Color.FromRgb(36, 52, 140), new Rect(14, 20, 356, 40));
                B5MediaArt.DrawFitted(context, "LOAD \"*\",8,1", B5MediaArt.HandFont, Color.FromRgb(36, 52, 140), new Rect(14, 64, 200, 24));
            }
            return B5MediaArt.Finish(drawing);
        }
    }
}
