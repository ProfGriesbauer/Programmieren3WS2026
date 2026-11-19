using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // 3D model of a grey Game Boy cartridge (57 x 65 x 8 mm) standing upright with the label towards +z.
    // Millimetres, origin in the middle of the bottom edge (the contact end that goes into the slot).
    public static class B5GameBoyCartridgeModel
    {
        internal const double Width = 57, Height = 65, Thickness = 8;

        static readonly Color _Shell = Color.FromRgb(176, 174, 168);
        static readonly Color _Ridge = Color.FromRgb(158, 156, 150);
        static readonly Color _Emboss = Color.FromRgb(128, 126, 120);
        static readonly Color _Contacts = Color.FromRgb(205, 166, 72);

        public static Model3DGroup Create(B5RoomGameEntry game)
        {
            Model3DGroup cartridge = new Model3DGroup();
            Matrix3D place = Matrix3D.Identity;
            Material shell = B5AtariMesh.Plastic(_Shell);
            Material ridge = B5AtariMesh.Plastic(_Ridge);
            double front = Thickness / 2, back = -Thickness / 2;
            B5AtariMesh.Extrude(cartridge, place, B5AtariMesh.RoundedRect(Width, Height, 2.5, 1), back, front, false, shell);

            // Grip ridges along the top on both sides, the embossed logo line and the arrow pointing into the slot.
            for (double y = 58.5; y <= 63; y += 1.5)
            {
                B5AtariMesh.Box(cartridge, place, 0, y, front + 0.2, Width - 8, 0.6, 0.4, ridge);
                B5AtariMesh.Box(cartridge, place, 0, y, back - 0.2, Width - 8, 0.6, 0.4, ridge);
            }
            Vector3D right = new Vector3D(1, 0, 0), up = new Vector3D(0, 1, 0);
            B5AtariMesh.Text(cartridge, place, "GAME BOY", _Emboss, new Point3D(0, 54, front + 0.1), right, up, 2.4);
            List<Point> arrow = new List<Point> { new Point(-3, 9), new Point(3, 9), new Point(0, 4.5) };
            B5AtariMesh.Extrude(cartridge, place, arrow, front, front + 0.4, false, B5AtariMesh.Plastic(_Emboss));

            // Recessed frame and picture label on the front.
            B5AtariMesh.Box(cartridge, place, 0, 32, front + 0.05, Width - 7, 41, 0.1, ridge);
            B5AtariMesh.Quad(cartridge, place, new Point3D(0, 32, front + 0.15), right, up, Width - 9, 39,
                B5AtariMesh.Textured(CreateLabel(game)));

            // Gold contacts on the back of the bottom end and the screw in the middle of the back.
            B5AtariMesh.Box(cartridge, place, 0, 3.5, back - 0.2, Width - 12, 5, 0.4, B5AtariMesh.Metal(_Contacts));
            B5AtariMesh.CylinderZ(cartridge, place, 0, 34, 2.2, back - 0.3, back, B5AtariMesh.Plastic(_Emboss));
            return cartridge;
        }

        // Label with a white frame: title on top, artwork in the game's colour, small maker line at the bottom.
        static Brush CreateLabel(B5RoomGameEntry game)
        {
            Color color = game.CartridgeColor;
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                context.DrawRectangle(new SolidColorBrush(Color.FromRgb(240, 238, 230)), null, new Rect(0, 0, 192, 156));
                B5MediaArt.DrawFitted(context, game.Title.ToUpperInvariant(), B5MediaArt.TitleFont, B5MediaArt.Mix(color, Colors.Black, 0.35),
                    new Rect(10, 6, 172, 26));
                Rect picture = new Rect(10, 36, 172, 100);
                B5MediaArt.DrawArtwork(context, picture, color, game.Title.Length * 17 + color.G);
                context.DrawRectangle(null, new Pen(new SolidColorBrush(Color.FromRgb(40, 40, 40)), 1.5), picture);
                B5MediaArt.DrawFitted(context, "DMG-" + game.Title.Length.ToString("00") + " GAME PAK", B5MediaArt.PrintFont,
                    Color.FromRgb(70, 70, 70), new Rect(10, 140, 120, 11));
            }
            return B5MediaArt.Finish(drawing);
        }
    }
}
