using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    // 3D model of the home computer on the desk: CRT monitor, keyboard, disk drive and the cable between them.
    // The desk surface is at y = 1.6; every part is placed so that it rests exactly on its support.
    public static class B5CommodoreModel
    {
        const double DeskTop = 1.6;
        const double MonitorX = 3.3, MonitorY = 2.06, FrontZ = -1.975;
        const double KeyboardX = 2.1, KeyboardZ = -2.025, KeyboardTop = 1.70;
        const double KeyHeight = 0.014;
        const double DriveX = 4.04, DriveZ = -2.5;

        static readonly Color _Monitor = Color.FromRgb(235, 232, 224);
        static readonly Color _MonitorRear = Color.FromRgb(222, 219, 208);
        static readonly Color _Bezel = Color.FromRgb(212, 208, 195);
        static readonly Color _Foot = Color.FromRgb(150, 146, 136);
        static readonly Color _Glass = Color.FromRgb(26, 26, 31);
        static readonly Color _Vent = Color.FromRgb(120, 116, 106);
        static readonly Color _Badge = Color.FromRgb(58, 56, 54);
        static readonly Color _Case = Color.FromRgb(217, 210, 187);
        static readonly Color _CaseDark = Color.FromRgb(196, 189, 168);
        static readonly Color _KeyDark = Color.FromRgb(66, 58, 50);
        static readonly Color _KeyLight = Color.FromRgb(98, 88, 76);
        static readonly Color _KeyFunction = Color.FromRgb(176, 150, 114);
        static readonly Color _Cable = Color.FromRgb(30, 30, 32);
        static readonly Color _Red = Color.FromRgb(201, 49, 39);
        static readonly Color _Navy = Color.FromRgb(38, 57, 78);

        public static Point3D ScreenCenter { get { return new Point3D(MonitorX, MonitorY, FrontZ + 0.022); } }
        public static double ScreenWidth { get { return 0.6; } }
        public static double ScreenHeight { get { return 0.46; } }

        public static void Build(Model3DGroup parent)
        {
            BuildMonitor(parent);
            BuildKeyboard(parent);
            BuildDiskDrive(parent);
            BuildCable(parent);
        }

        static void Box(Model3DGroup parent, double x, double y, double z, double width, double height, double depth, Color color)
        {
            B5RoomPaint.AddBox(parent, x, y, z, width, height, depth, color);
        }

        static void BuildMonitor(Model3DGroup p)
        {
            // Swivel foot, then the housing in three tapering steps like a real CRT.
            Box(p, MonitorX, DeskTop + 0.0175, -2.4, 0.6, 0.035, 0.5, _Foot);
            Box(p, MonitorX, MonitorY, FrontZ - 0.15, 0.95, 0.85, 0.30, _Monitor);
            Box(p, MonitorX, MonitorY, -2.425, 0.78, 0.70, 0.30, _MonitorRear);
            Box(p, MonitorX, MonitorY, -2.70, 0.54, 0.50, 0.25, _MonitorRear);

            // Raised bezel with the dark frame around the glass. The screen quad sits just in front of the frame.
            Box(p, MonitorX, MonitorY, FrontZ + 0.006, 0.80, 0.64, 0.012, _Bezel);
            Box(p, MonitorX, MonitorY, FrontZ + 0.0135, 0.68, 0.54, 0.013, _Glass);

            // Control strip below the screen: badge with colour stripes, power LED, two knobs and a switch.
            double stripY = 1.69;
            Box(p, 2.99, stripY, FrontZ + 0.004, 0.22, 0.04, 0.008, _Badge);
            Color[] stripes = { _Red, Color.FromRgb(221, 133, 63), Color.FromRgb(239, 191, 95), Color.FromRgb(76, 122, 74) };
            for (int i = 0; i < stripes.Length; i++)
                Box(p, 2.93 + i * 0.04, stripY, FrontZ + 0.0095, 0.03, 0.012, 0.003, stripes[i]);
            Box(p, 3.52, stripY, FrontZ + 0.006, 0.014, 0.014, 0.012, Color.FromRgb(70, 220, 70));
            Box(p, 3.60, stripY, FrontZ + 0.0125, 0.05, 0.05, 0.025, _Foot);
            Box(p, 3.68, stripY, FrontZ + 0.0125, 0.05, 0.05, 0.025, _Foot);
            Box(p, 3.745, stripY, FrontZ + 0.01, 0.03, 0.05, 0.02, _Badge);

            // Ventilation slits on top and on both sides of the front housing.
            for (int i = 0; i < 9; i++)
                Box(p, 3.14 + i * 0.04, MonitorY + 0.4265, -2.12, 0.014, 0.003, 0.12, _Vent);
            for (int i = 0; i < 7; i++)
            {
                Box(p, MonitorX - 0.4755, 2.12 + i * 0.035, -2.15, 0.003, 0.012, 0.14, _Vent);
                Box(p, MonitorX + 0.4755, 2.12 + i * 0.035, -2.15, 0.003, 0.012, 0.14, _Vent);
            }
        }

        static void BuildKeyboard(Model3DGroup p)
        {
            Box(p, KeyboardX, DeskTop + 0.05, KeyboardZ, 1.25, 0.10, 0.55, _Case);

            // Raised rear strip with a colour-striped badge and a power LED.
            Box(p, KeyboardX, KeyboardTop + 0.025, -2.21, 1.25, 0.05, 0.18, _CaseDark);
            Box(p, 1.78, KeyboardTop + 0.0515, -2.21, 0.5, 0.003, 0.07, _Badge);
            Color[] stripes = { _Red, Color.FromRgb(221, 133, 63), Color.FromRgb(239, 191, 95), Color.FromRgb(76, 122, 74) };
            for (int i = 0; i < stripes.Length; i++)
                Box(p, 1.60 + i * 0.04, KeyboardTop + 0.0545, -2.21, 0.03, 0.003, 0.05, stripes[i]);
            Box(p, 2.62, KeyboardTop + 0.028, -2.117, 0.014, 0.014, 0.006, _Red);

            BuildKeys(p);
        }

        static void BuildKeys(Model3DGroup p)
        {
            const double pitch = 0.055, size = 0.046;
            double[] rowZ = { -2.055, -2.0, -1.945, -1.89, -1.835 };

            for (int i = 0; i < 14; i++) Key(p, 1.55 + i * pitch, rowZ[0], size, size, _KeyDark);
            for (int i = 0; i < 13; i++) Key(p, 1.58 + i * pitch, rowZ[1], size, size, _KeyDark);
            for (int i = 0; i < 12; i++) Key(p, 1.595 + i * pitch, rowZ[2], size, size, _KeyDark);
            Key(p, 2.282, rowZ[2], 0.10, size, _KeyLight);                 // RETURN
            Key(p, 1.572, rowZ[3], 0.09, size, _KeyLight);                 // left SHIFT
            for (int i = 0; i < 10; i++) Key(p, 1.649 + i * pitch, rowZ[3], size, size, _KeyDark);
            Key(p, 2.226, rowZ[3], 0.10, size, _KeyLight);                 // right SHIFT
            Key(p, 1.58, rowZ[4], 0.07, size, _KeyLight);                  // C= key
            Key(p, 1.90, rowZ[4], 0.42, size, _KeyDark);                   // space bar
            Key(p, 2.20, rowZ[4], 0.07, size, _KeyLight);

            // Function keys in their own column on the right.
            for (int i = 0; i < 4; i++) Key(p, 2.55, -2.055 + i * 0.065, 0.08, 0.052, _KeyFunction);
        }

        static void Key(Model3DGroup p, double x, double z, double width, double depth, Color color)
        {
            Box(p, x, KeyboardTop + KeyHeight / 2, z, width, KeyHeight, depth, color);
        }

        static void BuildDiskDrive(Model3DGroup p)
        {
            Box(p, DriveX, DeskTop + 0.05, DriveZ, 0.28, 0.10, 0.40, _Case);
            double front = DriveZ + 0.2 + 0.001;
            Box(p, DriveX, 1.67, front, 0.20, 0.012, 0.004, _Badge);        // disk slot
            Box(p, DriveX + 0.09, 1.625, front, 0.012, 0.012, 0.004, _Red);   // activity LED
            Box(p, DriveX - 0.04, 1.625, front, 0.16, 0.012, 0.004, _Navy);   // brand stripe

            // Two floppy disks lying on top.
            Box(p, DriveX, 1.703, DriveZ, 0.09, 0.006, 0.09, _Navy);
            Box(p, DriveX + 0.01, 1.709, DriveZ, 0.09, 0.006, 0.09, _Red);
        }

        static void BuildCable(Model3DGroup p)
        {
            // Keyboard cable: out of the back of the keyboard, then along the desk to the monitor foot.
            Box(p, 2.60, DeskTop + 0.007, -2.375, 0.014, 0.014, 0.15, _Cable);
            Box(p, 2.80, DeskTop + 0.007, -2.45, 0.40, 0.014, 0.014, _Cable);
        }
    }
}
