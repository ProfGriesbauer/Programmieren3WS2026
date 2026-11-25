using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // 3D model of a grey Game Boy cartridge (57 x 65 x 8 mm) standing upright with the label towards +z.
    // Millimetres, origin in the middle of the bottom edge (the contact end that goes into the slot).
    public static class G10_GameBoyCartridgeModel
    {
        internal const double Width = 57, Height = 65, Thickness = 8;

        static readonly Color _Shell = Color.FromRgb(176, 174, 168);
        static readonly Color _Ridge = Color.FromRgb(158, 156, 150);
        static readonly Color _Emboss = Color.FromRgb(128, 126, 120);
        static readonly Color _Contacts = Color.FromRgb(205, 166, 72);

        public static Model3DGroup Create(G10_RoomGameEntry game)
        {
            Model3DGroup cartridge = new Model3DGroup();
            Matrix3D place = Matrix3D.Identity;
            Material shell = G10_AtariMesh.Plastic(_Shell);
            Material ridge = G10_AtariMesh.Plastic(_Ridge);
            double front = Thickness / 2, back = -Thickness / 2;
            G10_AtariMesh.Extrude(cartridge, place, G10_AtariMesh.RoundedRect(Width, Height, 2.5, 1), back, front, false, shell);

            // Grip ridges along the top on both sides, the embossed logo line and the arrow pointing into the slot.
            for (double y = 58.5; y <= 63; y += 1.5)
            {
                G10_AtariMesh.Box(cartridge, place, 0, y, front + 0.2, Width - 8, 0.6, 0.4, ridge);
                G10_AtariMesh.Box(cartridge, place, 0, y, back - 0.2, Width - 8, 0.6, 0.4, ridge);
            }
            Vector3D right = new Vector3D(1, 0, 0), up = new Vector3D(0, 1, 0);
            G10_AtariMesh.Text(cartridge, place, "GAME BOY", _Emboss, new Point3D(0, 54, front + 0.1), right, up, 2.4);
            List<Point> arrow = new List<Point> { new Point(-3, 9), new Point(3, 9), new Point(0, 4.5) };
            G10_AtariMesh.Extrude(cartridge, place, arrow, front, front + 0.4, false, G10_AtariMesh.Plastic(_Emboss));

            // Recessed frame and picture label on the front.
            G10_AtariMesh.Box(cartridge, place, 0, 32, front + 0.05, Width - 7, 41, 0.1, ridge);
            G10_AtariMesh.Quad(cartridge, place, new Point3D(0, 32, front + 0.15), right, up, Width - 9, 39,
                G10_AtariMesh.Textured(CreateLabel(game)));

            // Gold contacts on the back of the bottom end and the screw in the middle of the back.
            G10_AtariMesh.Box(cartridge, place, 0, 3.5, back - 0.2, Width - 12, 5, 0.4, G10_AtariMesh.Metal(_Contacts));
            G10_AtariMesh.CylinderZ(cartridge, place, 0, 34, 2.2, back - 0.3, back, G10_AtariMesh.Plastic(_Emboss));
            return cartridge;
        }

        // Label with a white frame: title on top, artwork in the game's colour, small maker line at the bottom.
        static Brush CreateLabel(G10_RoomGameEntry game)
        {
            Color color = game.CartridgeColor;
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                context.DrawRectangle(new SolidColorBrush(Color.FromRgb(240, 238, 230)), null, new Rect(0, 0, 192, 156));
                G10_MediaArt.DrawFitted(context, game.Title.ToUpperInvariant(), G10_MediaArt.TitleFont, G10_MediaArt.Mix(color, Colors.Black, 0.35),
                    new Rect(10, 6, 172, 26));
                Rect picture = new Rect(10, 36, 172, 100);
                G10_MediaArt.DrawArtwork(context, picture, color, game.Title.Length * 17 + color.G);
                context.DrawRectangle(null, new Pen(new SolidColorBrush(Color.FromRgb(40, 40, 40)), 1.5), picture);
                G10_MediaArt.DrawFitted(context, "DMG-" + game.Title.Length.ToString("00") + " GAME PAK", G10_MediaArt.PrintFont,
                    Color.FromRgb(70, 70, 70), new Rect(10, 140, 120, 11));
            }
            return G10_MediaArt.Finish(drawing);
        }
    }
}
