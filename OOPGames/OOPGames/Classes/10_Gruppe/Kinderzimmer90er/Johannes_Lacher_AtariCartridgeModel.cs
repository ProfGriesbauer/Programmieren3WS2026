using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // 3D model of an Atari 2600 game cartridge (80 x 108 x 20 mm), standing upright with the picture label
    // towards +z. Units are millimetres, the origin is the middle of the bottom edge (the end that goes into the slot).
    public static class G10_AtariCartridgeModel
    {
        internal const double Width = 80, Height = 108, Thickness = 20;

        static readonly Color _Shell = Color.FromRgb(24, 23, 22);
        static readonly Color _ShellEdge = Color.FromRgb(40, 39, 37);
        static readonly Color _Door = Color.FromRgb(33, 32, 31);

        public static Model3DGroup Create(G10_RoomGameEntry game)
        {
            Model3DGroup cartridge = new Model3DGroup();
            Matrix3D place = Matrix3D.Identity;
            Material shell = G10_AtariMesh.Plastic(_Shell);
            Material edge = G10_AtariMesh.Plastic(_ShellEdge);
            double front = Thickness / 2, back = -Thickness / 2;
            G10_AtariMesh.Extrude(cartridge, place, G10_AtariMesh.RoundedRect(Width, Height, 6, 1.5), back, front, false, shell);

            // Grip ribs on the back of the upper end and the sliding dust door over the contacts below.
            for (double y = 82; y <= 102; y += 4)
                G10_AtariMesh.Box(cartridge, place, 0, y, back - 0.5, Width - 12, 1.6, 1.0, edge);
            G10_AtariMesh.Box(cartridge, place, 0, 20, back - 0.3, Width - 14, 34, 0.6, G10_AtariMesh.Plastic(_Door));

            // Moulded step on the front where the part that goes into the console begins.
            G10_AtariMesh.Box(cartridge, place, 0, 44, front + 0.2, Width - 4, 1.0, 0.4, edge);
            G10_AtariMesh.Box(cartridge, place, 0, 4, front + 0.2, Width - 10, 1.0, 0.4, edge);

            G10_AtariMesh.Quad(cartridge, place, new Point3D(0, 75, front + 0.15), new Vector3D(1, 0, 0), new Vector3D(0, 1, 0),
                Width - 8, 56, G10_AtariMesh.Textured(CreateLabel(game)));
            G10_AtariMesh.Quad(cartridge, place, new Point3D(0, Height + 0.15, 0), new Vector3D(1, 0, 0), new Vector3D(0, 0, -1),
                Width - 16, 15, G10_AtariMesh.Textured(CreateEndLabel(game)));
            return cartridge;
        }

        // Picture label in the style of the late-70s Atari labels: title band, artwork, maker band.
        static Brush CreateLabel(G10_RoomGameEntry game)
        {
            Color color = game.CartridgeColor;
            Rect picture = new Rect(10, 58, 340, 176);
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                context.DrawRectangle(new SolidColorBrush(Color.FromRgb(14, 14, 14)), null, new Rect(0, 0, 360, 280));
                G10_MediaArt.DrawArtwork(context, picture, color, game.Title.Length * 31 + color.R);
                context.DrawRectangle(null, new Pen(new SolidColorBrush(color), 3), picture);
                G10_MediaArt.DrawFitted(context, game.Title.ToUpperInvariant(), G10_MediaArt.TitleFont, color, new Rect(14, 8, 332, 44));
                G10_MediaArt.DrawFitted(context, "ATARI", G10_MediaArt.TitleFont, Color.FromRgb(236, 232, 220), new Rect(14, 242, 120, 28));
                G10_MediaArt.DrawFitted(context, "GAME PROGRAM", G10_MediaArt.PrintFont, Color.FromRgb(200, 196, 186), new Rect(196, 248, 150, 18));
            }
            return G10_MediaArt.Finish(drawing);
        }

        // Narrow label on the top end of the cartridge, readable when it stands on a shelf.
        static Brush CreateEndLabel(G10_RoomGameEntry game)
        {
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                context.DrawRectangle(new SolidColorBrush(Color.FromRgb(14, 14, 14)), null, new Rect(0, 0, 256, 60));
                context.DrawRectangle(new SolidColorBrush(game.CartridgeColor), null, new Rect(0, 0, 22, 60));
                G10_MediaArt.DrawFitted(context, game.Title.ToUpperInvariant(), G10_MediaArt.TitleFont, game.CartridgeColor, new Rect(32, 12, 216, 36));
            }
            return G10_MediaArt.Finish(drawing);
        }
    }
}
