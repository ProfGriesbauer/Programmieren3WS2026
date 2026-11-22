using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace OOPGames
{
    public sealed class B5RoomPaint : IPaintGame2
    {
        Grid _Root;
        Viewport3D _Viewport;
        PerspectiveCamera _Camera;
        Model3DGroup _Scene;
        Canvas _GameCanvas;
        Border _GameFrame;
        Border _ConsoleShell;
        TextBlock _ConsoleShellTitle;
        Border _Sidebar;
        StackPanel _Cards;
        TextBlock _DeviceTitle;
        B5ControlBar _ControlBar;
        B5MediaAnimation[] _Media;
        readonly List<GeometryModel3D> _Highlights = new List<GeometryModel3D>();
        readonly List<GeometryModel3D> _Screens = new List<GeometryModel3D>();
        readonly List<Border> _CardBorders = new List<Border>();
        readonly List<TextBlock> _CardTitles = new List<TextBlock>();
        readonly Model3DGroup[] _DeviceControls = { new Model3DGroup(), new Model3DGroup(), new Model3DGroup() };
        // Mouse picking: per device the boxes around its 3D parts (furniture included), in room coordinates.
        readonly List<Rect3D>[] _PickBoxes = { new List<Rect3D>(), new List<Rect3D>(), new List<Rect3D>() };
        static readonly Dictionary<Color, Brush> SolidBrushCache = new Dictionary<Color, Brush>();
        static readonly Dictionary<Color, Material> BoxMaterialCache = new Dictionary<Color, Material>();
        static readonly Dictionary<(Color Color, Color Highlight), Material> EllipsoidMaterialCache =
            new Dictionary<(Color Color, Color Highlight), Material>();
        static readonly Dictionary<Brush, Material> TexturedMaterialCache = new Dictionary<Brush, Material>();
        // Meshes made by AddBox are closed boxes with outward faces, so their back faces can never be seen.
        static readonly ConditionalWeakTable<MeshGeometry3D, object> ClosedMeshes = new ConditionalWeakTable<MeshGeometry3D, object>();
        static readonly Material[,] ScreenMaterials = CreateScreenMaterials();
        static readonly Material[] HighlightMaterials = CreateHighlightMaterials();
        static readonly Brush FloorShadowBrush = CreateFloorShadowBrush();
        static readonly Material FloorShadowMaterial = CreateFloorShadowMaterial();
        B5RoomField _LastField;
        int _LastDevice = -1;
        int _LastSelected = -1;
        int _LastHighlightDevice = -1;
        int _LastScreenDevice = -1;
        B5RoomPhase _LastDrawnPhase = (B5RoomPhase)(-1);
        int _LastDrawnDevice = -1;
        int _LastDrawnGame = -1;
        B5RoomPhase _LastCardPhase = (B5RoomPhase)(-1);
        int _LastCardPending = -1;
        int _LastCardInserted = -1;
        int _LastOverlayDevice = -1;
        bool _LastOverlayMonitor;
        Grid _ScreenEffect;
        bool _CameraInitialized;
        Point3D _CameraPosition;
        Vector3D _CameraLook;
        // Close to the old feel: the camera covered about 9 % of the remaining way per 40 ms tick.
        static readonly Duration CameraMove = new Duration(TimeSpan.FromSeconds(1.2));
        static readonly IEasingFunction CameraEase = CreateCameraEase();
        double _LastWidth = -1;

        static readonly Brush WoodTexture = CreateWoodTexture();
        static readonly Brush WallpaperTexture = CreateWallpaperTexture();
        static readonly Brush CarpetTexture = CreateCarpetTexture();
        static readonly Brush CurtainTexture = CreateCurtainTexture();
        static readonly Brush ArcadePoster = B5WallArt.CreateArcadePoster();
        static readonly Brush WindowView = B5WallArt.CreateWindowView();
        static readonly Brush DinoPoster = B5WallArt.CreateDinoPoster();

        public string Name { get { return "Kinderzimmer (90er) 3D"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            B5RoomField field = currentField as B5RoomField;
            if (field == null) return;
            EnsureRoot(canvas, field);
            DrawGame(field);
            UpdateInterface(field);
            UpdateCamera(field, 1);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            B5RoomField field = currentField as B5RoomField;
            if (field == null) return;
            EnsureRoot(canvas, field);
            DrawGame(field);
            UpdateInterface(field);
            UpdateCamera(field, 0.09);
        }

        void EnsureRoot(Canvas canvas, B5RoomField field)
        {
            if (_Root == null || !canvas.Children.Contains(_Root))
            {
                _Root = new Grid { ClipToBounds = true, Background = new SolidColorBrush(Color.FromRgb(29, 24, 21)) };
                _Viewport = new Viewport3D();
                _Camera = new PerspectiveCamera { FieldOfView = 70, NearPlaneDistance = 0.05, FarPlaneDistance = 100 };
                _Viewport.Camera = _Camera;
                _Scene = new Model3DGroup();
                _Scene.Children.Add(new AmbientLight(Color.FromRgb(153, 139, 122)));
                _Scene.Children.Add(new DirectionalLight(Color.FromRgb(255, 239, 211), new Vector3D(-1, -2, 1)));
                _Scene.Children.Add(new DirectionalLight(Color.FromRgb(177, 195, 220), new Vector3D(1, -0.5, -1)));
                _Scene.Children.Add(new PointLight(Color.FromRgb(255, 224, 172), new Point3D(-3.5, 2.45, -2.45))
                {
                    Range = 3.8,
                    ConstantAttenuation = 0.5,
                    LinearAttenuation = 0.25,
                    QuadraticAttenuation = 0.18
                });
                _Scene.Children.Add(new PointLight(Color.FromRgb(255, 239, 207), new Point3D(0.4, 3.5, -0.1))
                {
                    Range = 6.5,
                    ConstantAttenuation = 0.4,
                    LinearAttenuation = 0.15,
                    QuadraticAttenuation = 0.08
                });
                _Viewport.Children.Add(new ModelVisual3D { Content = _Scene });
                _Root.Children.Add(_Viewport);
                BuildInterface();
                BuildRoom();
                OptimizeStaticScene();
                HookMouse();
                // Cartridges and disks with their device parts, in device order. Added after the optimisation,
                // which would freeze the moving parts.
                _Media = new B5MediaAnimation[3];
                _Media[(int)B5RoomDeviceKind.Atari] = new B5AtariConsoleAnimation();
                _Media[(int)B5RoomDeviceKind.Gameboy] = new B5GameBoyCartridgeAnimation();
                _Media[(int)B5RoomDeviceKind.Commodore] = new B5CommodoreDiskAnimation();
                foreach (B5MediaAnimation media in _Media) _Scene.Children.Add(media.Model);
                AddDisplay(B5AtariTelevisionModel.CreateScreenMesh());
                Point3D gameBoyScreen = B5GameBoyModel.ScreenCenter;
                AddDisplayQuad(gameBoyScreen.X, gameBoyScreen.Y, gameBoyScreen.Z, B5GameBoyModel.ScreenWidth, B5GameBoyModel.ScreenHeight);
                Point3D commodoreScreen = B5CommodoreModel.ScreenCenter;
                AddDisplayQuad(commodoreScreen.X, commodoreScreen.Y, commodoreScreen.Z, B5CommodoreModel.ScreenWidth, B5CommodoreModel.ScreenHeight);
                canvas.Children.Clear();
                canvas.Children.Add(_Root);
                _LastField = field;
                _LastDevice = -1;
                _LastSelected = -1;
                _LastScreenDevice = -1;
                _LastHighlightDevice = -1;
                _LastDrawnPhase = (B5RoomPhase)(-1);
                _CameraInitialized = false;
                _LastWidth = -1;
                _LastOverlayDevice = -1;
            }

            double width = Math.Max(1, canvas.ActualWidth);
            double height = Math.Max(1, canvas.ActualHeight);
            if (Math.Abs(_LastWidth - width) > 1)
            {
                _Root.Width = width;
                _Root.Height = height;
                _Sidebar.Width = Math.Max(220, width * 0.29);
                _LastWidth = width;
            }
            if (Math.Abs(_Root.Height - height) > 1) _Root.Height = height;

            if (_LastField != field)
            {
                _LastField = field;
                _LastDevice = -1;
            }
        }

        // Game canvas plus a CRT effect layer (scanlines and dark corners) that is only shown in monitor view.
        UIElement BuildScreenContent()
        {
            Grid screen = new Grid();
            screen.Children.Add(new Viewbox { Stretch = Stretch.Uniform, Child = _GameCanvas });

            GeometryDrawing line = new GeometryDrawing(new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)), null,
                new RectangleGeometry(new Rect(0, 2, 4, 1)));
            GeometryDrawing gap = new GeometryDrawing(Brushes.Transparent, null, new RectangleGeometry(new Rect(0, 0, 4, 3)));
            DrawingGroup tile = new DrawingGroup();
            tile.Children.Add(gap);
            tile.Children.Add(line);
            DrawingBrush scanlines = new DrawingBrush(tile)
            {
                TileMode = TileMode.Tile,
                Viewport = new Rect(0, 0, 4, 3),
                ViewportUnits = BrushMappingMode.Absolute,
                Viewbox = new Rect(0, 0, 4, 3),
                ViewboxUnits = BrushMappingMode.Absolute,
                Stretch = Stretch.None
            };
            scanlines.Freeze();

            RadialGradientBrush vignette = new RadialGradientBrush { RadiusX = 0.75, RadiusY = 0.75 };
            vignette.GradientStops.Add(new GradientStop(Color.FromArgb(0, 0, 0, 0), 0.6));
            vignette.GradientStops.Add(new GradientStop(Color.FromArgb(110, 0, 0, 0), 1.0));
            vignette.Freeze();

            _ScreenEffect = new Grid { IsHitTestVisible = false, Visibility = Visibility.Collapsed };
            _ScreenEffect.Children.Add(new Rectangle { Fill = scanlines, Opacity = 0.14 });
            _ScreenEffect.Children.Add(new Rectangle { Fill = vignette });
            screen.Children.Add(_ScreenEffect);
            return screen;
        }

        // Tower Defense is shown on a full-size old monitor instead of inside the small console shell.
        static bool IsMonitorView(B5RoomField field)
        {
            return field.Phase == B5RoomPhase.Playing && field.ActiveGame is B5RoomTowerDefenseGame;
        }

        void BuildInterface()
        {
            _GameCanvas = new Canvas { Width = 160, Height = 144, Background = Brushes.Black, ClipToBounds = true };
            _GameFrame = new Border
            {
                Width = 160,
                Height = 144,
                BorderThickness = new Thickness(8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(181, 159, 119)),
                Background = new SolidColorBrush(Color.FromRgb(24, 25, 28)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Child = BuildScreenContent(),
                Visibility = Visibility.Collapsed
            };
            _GameFrame.SetValue(Panel.ZIndexProperty, 2);
            _Root.Children.Add(_GameFrame);

            Grid shellContent = new Grid();
            _ConsoleShellTitle = new TextBlock
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 9, 0, 0)
            };
            shellContent.Children.Add(_ConsoleShellTitle);
            _ConsoleShell = new Border
            {
                BorderThickness = new Thickness(4),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Child = shellContent,
                Visibility = Visibility.Collapsed
            };
            _ConsoleShell.SetValue(Panel.ZIndexProperty, 1);
            _Root.Children.Add(_ConsoleShell);

            StackPanel panel = new StackPanel();
            panel.Children.Add(B5RetroStyle.CreateText("Spiele waehlen:", 19, B5RetroStyle.Text));
            panel.Children.Add(new Border { Height = 2, Background = B5RetroStyle.Frame, Margin = new Thickness(-12, 8, -12, 10) });
            _DeviceTitle = B5RetroStyle.CreateText("", 17, B5RetroStyle.Text);
            _DeviceTitle.Margin = new Thickness(0, 0, 0, 8);
            panel.Children.Add(_DeviceTitle);
            _Cards = new StackPanel();
            panel.Children.Add(_Cards);
            _Sidebar = B5RetroStyle.CreatePanel(panel);
            _Sidebar.Width = 250;
            _Sidebar.HorizontalAlignment = HorizontalAlignment.Right;
            _Sidebar.VerticalAlignment = VerticalAlignment.Stretch;
            _Sidebar.Visibility = Visibility.Collapsed;
            _Sidebar.SetValue(Panel.ZIndexProperty, 3);
            _Root.Children.Add(_Sidebar);

            _ControlBar = new B5ControlBar();
            _ControlBar.Element.SetValue(Panel.ZIndexProperty, 4);
            _Root.Children.Add(_ControlBar.Element);
        }

        void BuildRoom()
        {
            AddBox(0, -0.05, 0, 10, 0.1, 6, Color.FromRgb(94, 64, 43));
            AddTexturedBox(0, 2, -3, 10, 4, 0.1, WallpaperTexture);
            AddBox(-5, 2, 0, 0.1, 4, 6, Color.FromRgb(192, 166, 132));

            for (int row = 0; row < 8; row++)
            {
                double z = -2.625 + row * 0.75;
                double x = -5;
                int plank = 0;
                if (row % 2 != 0)
                {
                    AddBox(x + 0.6125, 0.01, z, 1.2, 0.018, 0.72, Color.FromRgb(111, 73, 48));
                    x += 1.225;
                    plank++;
                }

                while (x < 5)
                {
                    double width = Math.Min(2.5, 5 - x);
                    Color wood = (row + plank) % 3 == 0 ? Color.FromRgb(132, 87, 55) :
                        (row + plank) % 3 == 1 ? Color.FromRgb(112, 72, 47) : Color.FromRgb(122, 79, 51);
                    AddBox(x + width / 2, 0.01, z, Math.Max(0.1, width - 0.025), 0.018, 0.72, wood);
                    if (width > 0.5)
                    {
                        Color grain = Color.FromRgb(
                            (byte)Math.Min(255, wood.R + 12),
                            (byte)Math.Min(255, wood.G + 8),
                            (byte)Math.Min(255, wood.B + 4));
                        AddBox(x + width * 0.48, 0.020, z - 0.18, width * 0.7, 0.0015, 0.008, grain);
                        AddBox(x + width * 0.58, 0.020, z + 0.16, width * 0.35, 0.0015, 0.006, grain);
                    }
                    x += width;
                    plank++;
                }
            }

            AddTexturedBox(0, 0.015, 0.8, 3.2, 0.02, 3.2, CarpetTexture);
            AddBox(0, 0.029, -0.62, 2.9, 0.006, 0.1, Color.FromRgb(218, 174, 124));
            AddBox(0, 0.029, 2.22, 2.9, 0.006, 0.1, Color.FromRgb(218, 174, 124));
            AddBox(-1.46, 0.029, 0.8, 0.1, 0.006, 2.7, Color.FromRgb(218, 174, 124));
            AddBox(1.46, 0.029, 0.8, 0.1, 0.006, 2.7, Color.FromRgb(218, 174, 124));
            AddBox(0, 0.029, -0.05, 2.2, 0.006, 0.12, Color.FromRgb(223, 190, 147));
            AddBox(0, 0.029, 0.8, 2.2, 0.006, 0.12, Color.FromRgb(198, 150, 103));
            AddBox(0, 0.029, 1.65, 2.2, 0.006, 0.12, Color.FromRgb(223, 190, 147));
            AddBox(-0.72, 0.029, 0.38, 0.14, 0.006, 0.62, Color.FromRgb(198, 150, 103));
            AddBox(0.72, 0.029, 1.22, 0.14, 0.006, 0.62, Color.FromRgb(198, 150, 103));

            AddTexturedBox(1.8, 1.55, -2.35, 5, 0.1, 1.3, WoodTexture);
            AddWoodBox(-0.2, 0.75, -2.35, 0.9, 1.5, 1.2);
            AddWoodBox(4.2, 0.75, -2.35, 0.15, 1.5, 1.2);
            AddBox(1.8, 0.96, -0.9, 0.9, 0.12, 0.9, Color.FromRgb(139, 134, 128));
            AddBox(1.8, 1.5, -0.4, 0.9, 0.85, 0.1, Color.FromRgb(139, 134, 128));
            AddBox(1.8, 0.52, -0.9, 0.12, 0.8, 0.12, Color.FromRgb(90, 91, 94));
            AddBox(1.8, 0.12, -0.9, 0.75, 0.08, 0.75, Color.FromRgb(90, 91, 94));
            BuildTelevisionDetails();

            foreach (List<Rect3D> boxes in _PickBoxes) boxes.Clear();
            int atariStart = _Scene.Children.Count;
            AddWoodBox(-3.6, 1.0, -1.8, 2, 0.08, 1.1);
            // Lower shelf sits low, so a cartridge fits upright into the console under the table top.
            AddWoodBox(-3.6, B5AtariConsoleModel.ShelfTop - 0.035, -1.8, 1.8, 0.07, 0.95);
            AddBox(-4.45, 0.48, -2.25, 0.08, 0.96, 0.08, Color.FromRgb(100, 67, 45));
            AddBox(-2.75, 0.48, -2.25, 0.08, 0.96, 0.08, Color.FromRgb(100, 67, 45));
            AddBox(-4.45, 0.48, -1.35, 0.08, 0.96, 0.08, Color.FromRgb(100, 67, 45));
            AddBox(-2.75, 0.48, -1.35, 0.08, 0.96, 0.08, Color.FromRgb(100, 67, 45));
            B5AtariTelevisionModel.Build(_Scene);
            B5AtariConsoleModel.Build(_Scene);
            _PickBoxes[(int)B5RoomDeviceKind.Atari].Add(BoundsSince(atariStart));
            AddBox(-2.4, 0.15, -0.8, 0.12, 0.3, 0.12, Color.FromRgb(35, 35, 39));

            int gameBoyStart = _Scene.Children.Count;
            B5GameBoyModel.Build(_Scene);
            _PickBoxes[(int)B5RoomDeviceKind.Gameboy].Add(BoundsSince(gameBoyStart));

            int commodoreStart = _Scene.Children.Count;
            B5CommodoreModel.Build(_Scene);
            _PickBoxes[(int)B5RoomDeviceKind.Commodore].Add(BoundsSince(commodoreStart));
            int controllerStart = _Scene.Children.Count;
            B5AtariControllerModel.Build(_Scene);
            _PickBoxes[(int)B5RoomDeviceKind.Atari].Add(BoundsSince(controllerStart));
            BuildComputerCaseDetails();
            BuildDesktopWear();

            AddBox(1.1, 2.6, -2.9, 1.1, 0.7, 0.04, Color.FromRgb(222, 194, 151));
            AddBox(0.72, 2.6, -2.872, 0.1, 0.5, 0.012, Color.FromRgb(38, 57, 78));
            AddBox(1.18, 2.42, -2.872, 0.6, 0.08, 0.012, Color.FromRgb(196, 76, 55));
            AddBox(0.98, 2.68, -2.872, 0.18, 0.18, 0.012, Color.FromRgb(221, 133, 63));
            AddBox(1.42, 2.77, -2.872, 0.18, 0.18, 0.012, Color.FromRgb(38, 57, 78));
            BuildPosterFrame(-1.5, 2.7, -2.875, 1.3, 0.8);
            BuildPosterFrame(-3.6, 2.77, -2.835, 2.25, 1.05);
            BuildPosterFrame(1.1, 2.6, -2.845, 1.1, 0.7);
            AddBox(0.6, 2.3, -2.75, 3.2, 0.08, 0.35, Color.FromRgb(107, 74, 54));
            AddBox(0.6, 3.0, -2.75, 3.2, 0.08, 0.35, Color.FromRgb(107, 74, 54));
            BuildShelfDetails();
            BuildBooks();

            AddBox(-4.35, 0.13, 1.55, 0.42, 0.24, 0.42, Color.FromRgb(174, 91, 59));
            AddBox(-4.35, 0.31, 1.55, 0.34, 0.08, 0.34, Color.FromRgb(103, 65, 44));
            AddBox(-4.35, 0.52, 1.55, 0.07, 0.42, 0.07, Color.FromRgb(68, 104, 57));
            AddBox(-4.52, 0.58, 1.55, 0.3, 0.1, 0.12, Color.FromRgb(76, 122, 64));
            AddBox(-4.18, 0.68, 1.55, 0.12, 0.1, 0.3, Color.FromRgb(89, 136, 70));
            BuildPlantPotDetails();

            BuildRoomDecor();

            _Highlights.Clear();
            // Selection frames lie flat around each device and are made visible for the selected one.
            _Highlights.Add(AddFrame(-3.6, 0.03, -1.8, 1.9, 1.2, 0.03));
            _Highlights.Add(AddFrame(0.8, 1.608, B5GameBoyModel.Center.Z, 0.5, 0.28, 0.02));
            _Highlights.Add(AddFrame(2.815, 1.608, -2.3225, 2.87, 1.235, 0.03));
        }

        // Bounding box of all scene parts added since the given child index.
        Rect3D BoundsSince(int start)
        {
            Rect3D bounds = Rect3D.Empty;
            for (int i = start; i < _Scene.Children.Count; i++) bounds.Union(_Scene.Children[i].Bounds);
            return bounds;
        }

        // Device under the mouse in the 3D view, or -1. The nearest surface under the mouse is looked up and
        // assigned to the smallest device box containing it (a little larger, so edges count too).
        int PickDevice(Point position)
        {
            RayHitTestResult hit = VisualTreeHelper.HitTest(_Viewport, position) as RayHitTestResult;
            if (hit == null) return -1;
            Point3D point = hit.PointHit;
            const double margin = 0.03;
            int best = -1;
            double bestVolume = double.MaxValue;
            for (int device = 0; device < _PickBoxes.Length; device++)
            {
                foreach (Rect3D box in _PickBoxes[device])
                {
                    if (box.IsEmpty) continue;
                    bool inside = point.X >= box.X - margin && point.X <= box.X + box.SizeX + margin &&
                        point.Y >= box.Y - margin && point.Y <= box.Y + box.SizeY + margin &&
                        point.Z >= box.Z - margin && point.Z <= box.Z + box.SizeZ + margin;
                    double volume = box.SizeX * box.SizeY * box.SizeZ;
                    if (inside && volume < bestVolume)
                    {
                        best = device;
                        bestVolume = volume;
                    }
                }
            }
            return best;
        }

        void HookMouse()
        {
            _Viewport.MouseMove += (sender, e) =>
            {
                B5RoomField field = _LastField;
                if (field == null) return;
                int device = PickDevice(e.GetPosition(_Viewport));
                bool clickable = device >= 0 && (field.Phase == B5RoomPhase.Room || field.Phase == B5RoomPhase.Device);
                Cursor cursor = clickable ? Cursors.Hand : null;
                if (_Viewport.Cursor != cursor) _Viewport.Cursor = cursor;
                if (device >= 0) field.HoverDevice(device);
            };
            _Viewport.MouseLeave += (sender, e) => _Viewport.Cursor = null;
            // Not marked as handled: the framework's canvas handler still has to get the click (keyboard focus).
            _Viewport.MouseLeftButtonDown += (sender, e) =>
            {
                B5RoomField field = _LastField;
                if (field == null) return;
                int device = PickDevice(e.GetPosition(_Viewport));
                if (device < 0) return;
                // A click on the open device inserts or starts the selected game, like Enter.
                if (field.Phase == B5RoomPhase.Device && device == field.SelectedDevice) field.LatchKey(Key.Enter);
                else field.OpenDevice(device);
            };
            // Right click = back. Not while a game runs: games use the right button themselves (Tower Defense).
            _Root.MouseRightButtonDown += (sender, e) =>
            {
                B5RoomField field = _LastField;
                if (field != null && field.Phase != B5RoomPhase.Playing) field.LatchKey(Key.Escape);
            };
            _ControlBar.TabClicked += device =>
            {
                if (_LastField != null) _LastField.OpenDevice(device);
            };
            _ControlBar.HintClicked += key =>
            {
                if (_LastField != null) _LastField.LatchKey(key);
            };
        }

        void BuildRoomDecor()
        {
            Color trim = Color.FromRgb(147, 105, 67);

            AddBox(0, 0.22, -2.91, 9.7, 0.16, 0.12, Color.FromRgb(157, 121, 82));
            AddBox(0, 0.31, -2.84, 9.6, 0.018, 0.02, Color.FromRgb(201, 168, 126));
            AddBox(-4.88, 2, -2.91, 0.16, 3.55, 0.12, trim);
            AddBox(-4.79, 2, -2.84, 0.018, 3.48, 0.02, Color.FromRgb(207, 176, 132));
            AddBox(0, 3.82, -2.91, 9.7, 0.16, 0.12, Color.FromRgb(157, 121, 82));

            AddFloorShadow(-3.6, -1.78, 2.1, 1.45);
            AddFloorShadow(1.8, -2.2, 5.2, 1.7);
            AddFloorShadow(0.8, -2.45, 1.5, 1.2);
            AddFloorShadow(-4.35, 1.55, 0.75, 0.75);

            AddWoodBox(3.58, 3.48, -2.91, 1.62, 1.02, 0.1);
            AddBox(3.58, 3.48, -2.85, 1.42, 0.84, 0.035, Color.FromRgb(72, 93, 105));
            // Painted evening view behind the glass (z -2.81), between the inner frame (-2.8325) and the
            // window cross (-2.794). It glows a little, like daylight coming in.
            AddWindowView(3.58, 3.48, -2.81, 1.3, 0.72);
            AddBox(3.58, 3.48, -2.8, 1.27, 0.012, 0.012, Color.FromRgb(192, 210, 208));
            AddBox(3.58, 3.48, -2.8, 0.012, 0.69, 0.012, Color.FromRgb(192, 210, 208));
            AddTexturedBox(2.75, 3.45, -2.8, 0.2, 1.12, 0.07, CurtainTexture);
            AddTexturedBox(4.4, 3.45, -2.8, 0.2, 1.12, 0.07, CurtainTexture);
            for (int fold = 0; fold < 4; fold++)
            {
                double x = fold < 2 ? 2.72 + fold * 0.08 : 4.36 + (fold - 2) * 0.08;
                AddBox(x, 3.45, -2.755, 0.018, 1.0, 0.012, Color.FromRgb(177, 104, 77));
            }
            AddWoodBox(3.58, 4.02, -2.79, 1.95, 0.07, 0.08);

            AddBox(-3.6, 2.77, -2.91, 2.25, 1.05, 0.12, Color.FromRgb(116, 87, 61));
            // Arcade poster on the backing board (front at z -2.85), framed by BuildPosterFrame.
            AddTexturedQuad(_Scene, -3.6, 2.77, -2.846, 2.06, 0.86, ArcadePoster, false);

            // Dino poster: backing board (front at z -2.89), picture just in front, frame around it.
            AddBox(-1.5, 2.7, -2.91, 1.3, 0.8, 0.04, Color.FromRgb(115, 93, 71));
            AddTexturedQuad(_Scene, -1.5, 2.7, -2.886, 1.15, 0.65, DinoPoster, false);

            AddTexturedBox(1.8, 1.46, -2.26, 4.68, 0.045, 0.06, WoodTexture);
            AddBox(1.8, 1.38, -1.77, 0.035, 1.02, 0.035, trim);
            AddBox(3.9, 1.38, -1.77, 0.035, 1.02, 0.035, trim);

            AddWoodBox(-3.6, 1.05, -2.31, 1.55, 0.045, 0.035);
            AddWoodBox(-3.6, 0.89, -2.306, 1.4, 0.16, 0.025);
            AddBox(-3.6, 0.9, -2.285, 0.22, 0.025, 0.018, Color.FromRgb(192, 159, 111));

            AddSphere(-4.35, 0.48, 1.55, 0.15, Color.FromRgb(179, 102, 62), Color.FromRgb(224, 170, 114));
            AddEllipsoid(-4.48, 0.7, 1.54, 0.18, 0.12, 0.09, Color.FromRgb(76, 122, 64));
            AddEllipsoid(-4.2, 0.75, 1.55, 0.12, 0.17, 0.1, Color.FromRgb(92, 143, 75));
            AddEllipsoid(-4.38, 0.82, 1.37, 0.1, 0.14, 0.18, Color.FromRgb(69, 113, 62));
        }

        void BuildTelevisionDetails()
        {
            double front = -0.35;
            Color casing = Color.FromRgb(100, 96, 89);
            Color trim = Color.FromRgb(163, 153, 135);
            AddBox(1.69, 1.54, front + 0.014, 0.68, 0.57, 0.024, trim);
            AddBox(1.69, 1.54, front + 0.03, 0.59, 0.49, 0.012, Color.FromRgb(19, 23, 28));
            AddEllipsoid(1.69, 1.54, front + 0.047, 0.285, 0.235, 0.025,
                Color.FromRgb(23, 31, 39), Color.FromRgb(135, 157, 165));
            AddBox(1.535, 1.715, front + 0.074, 0.17, 0.009, 0.003,
                Color.FromArgb(110, 201, 218, 222));

            AddBox(2.105, 1.69, front + 0.02, 0.16, 0.018, 0.012, casing);
            AddSphere(2.095, 1.59, front + 0.035, 0.035, Color.FromRgb(80, 77, 72), Color.FromRgb(210, 198, 173));
            AddSphere(2.095, 1.49, front + 0.035, 0.026, Color.FromRgb(74, 72, 68), Color.FromRgb(190, 181, 164));
            AddSphere(2.095, 1.40, front + 0.035, 0.018, Color.FromRgb(82, 77, 68), Color.FromRgb(197, 182, 152));
            AddSphere(2.19, 1.69, front + 0.034, 0.014, Color.FromRgb(65, 156, 88), Color.FromRgb(183, 255, 166));
            for (int vent = 0; vent < 7; vent++)
            {
                AddBox(2.105, 1.32 + vent * 0.022, front + 0.027, 0.13, 0.006, 0.006,
                    Color.FromRgb(55, 53, 50));
            }
            for (int vent = 0; vent < 6; vent++)
            {
                AddBox(2.255, 1.35 + vent * 0.045, -0.4, 0.006, 0.009, 0.15,
                    Color.FromRgb(75, 72, 66));
            }
            AddBox(1.77, 1.095, -0.346, 0.78, 0.012, 0.008, Color.FromRgb(172, 164, 150));
        }

        void BuildMonitorDetails()
        {
            double x = 3.3;
            double y = 2.03;
            double front = -1.945;
            Color bezel = Color.FromRgb(186, 182, 171);
            AddBox(x, y + 0.25, front + 0.012, 0.69, 0.045, 0.025, bezel);
            AddBox(x, y - 0.25, front + 0.012, 0.69, 0.045, 0.025, bezel);
            AddBox(x - 0.32, y, front + 0.012, 0.045, 0.46, 0.025, bezel);
            AddBox(x + 0.32, y, front + 0.012, 0.045, 0.46, 0.025, bezel);
            AddBox(x, y - 0.293, front + 0.024, 0.26, 0.014, 0.006, Color.FromRgb(135, 131, 122));
            AddSphere(x + 0.23, y - 0.292, front + 0.027, 0.012,
                Color.FromRgb(70, 141, 87), Color.FromRgb(193, 239, 154));
            AddBox(x - 0.475, y + 0.05, -2.4, 0.008, 0.19, 0.28, Color.FromRgb(179, 175, 164));
            AddBox(x + 0.475, y + 0.05, -2.4, 0.008, 0.19, 0.28, Color.FromRgb(179, 175, 164));
            for (int side = -1; side <= 1; side += 2)
            {
                double sideX = x + side * 0.481;
                for (int slit = 0; slit < 6; slit++)
                    AddBox(sideX, y + 0.045 + slit * 0.023, -2.4, 0.003, 0.008, 0.19,
                        Color.FromRgb(125, 121, 112));
            }
            AddBox(x - 0.1, 1.72, -1.94, 0.48, 0.012, 0.018, Color.FromRgb(167, 162, 151));
        }

        void BuildShelfDetails()
        {
            AddWoodBox(-0.96, 2.67, -2.75, 0.08, 0.58, 0.33);
            AddWoodBox(2.16, 2.67, -2.75, 0.08, 0.58, 0.33);
            AddWoodBox(0.6, 2.3, -2.56, 3.18, 0.045, 0.025);
            AddWoodBox(0.6, 3.0, -2.56, 3.18, 0.045, 0.025);

            foreach (double x in new[] { -0.91, 2.11 })
            {
                foreach (double y in new[] { 2.3, 3.0 })
                {
                    AddSphere(x, y, -2.541, 0.012, Color.FromRgb(104, 78, 48), Color.FromRgb(199, 157, 98));
                }
            }
            for (int joint = 0; joint < 4; joint++)
            {
                AddBox(-0.7 + joint * 0.85, 2.34, -2.58, 0.008, 0.028, 0.004,
                    Color.FromRgb(83, 58, 40));
                AddBox(-0.7 + joint * 0.85, 3.0, -2.58, 0.008, 0.028, 0.004,
                    Color.FromRgb(83, 58, 40));
            }
        }

        void BuildComputerCaseDetails()
        {
            for (int vent = 0; vent < 8; vent++)
            {
                AddBox(1.75 + vent * 0.09, 1.744, -2.26, 0.004, 0.003, 0.11,
                    Color.FromRgb(143, 136, 117));
            }
            AddSphere(2.66, 1.747, -2.14, 0.012, Color.FromRgb(63, 135, 85), Color.FromRgb(187, 240, 150));
            AddBox(2.49, 1.744, -1.79, 0.12, 0.003, 0.012, Color.FromRgb(163, 154, 132));
        }

        void BuildComputerDetails(Model3DGroup controls)
        {
            AddBox(controls, 2.1, 1.785, -2.245, 0.48, 0.012, 0.018, Color.FromRgb(74, 67, 57));
            AddBox(controls, 2.1, 1.793, -2.245, 0.25, 0.003, 0.004, Color.FromRgb(167, 151, 117));
            AddBox(controls, 2.66, 1.78, -2.15, 0.024, 0.008, 0.024, Color.FromRgb(65, 145, 83));
        }

        void BuildAtariControllerDetails(Model3DGroup controls)
        {
            AddBox(controls, -3.6, 0.732, -1.15, 0.13, 0.018, 0.13, Color.FromRgb(50, 49, 48));
            AddBox(controls, -3.32, 0.768, -1.15, 0.105, 0.008, 0.105, Color.FromRgb(122, 34, 30));
            AddBox(controls, -3.32, 0.773, -1.15, 0.068, 0.006, 0.068, Color.FromRgb(223, 67, 53));
            for (int rib = 0; rib < 4; rib++)
            {
                AddBox(controls, -3.78 + rib * 0.12, 0.721, -1.303, 0.045, 0.004, 0.003,
                    Color.FromRgb(76, 73, 70));
            }
        }

        void BuildDesktopWear()
        {
            Color wornWood = Color.FromRgb(177, 137, 95);
            Color darkScratch = Color.FromRgb(111, 77, 52);
            AddBox(0.25, 1.601, -1.74, 0.58, 0.002, 0.008, wornWood);
            AddBox(-0.15, 1.601, -1.82, 0.19, 0.0015, 0.003, darkScratch);
            AddBox(-0.02, 1.601, -1.88, 0.11, 0.0015, 0.003, wornWood);
            AddBox(0.34, 1.601, -1.93, 0.16, 0.0015, 0.003, darkScratch);
            AddBox(0.18, 1.601, -2.03, 0.08, 0.0015, 0.003, wornWood);
            AddBox(-0.34, 1.601, -2.1, 0.12, 0.0015, 0.003, darkScratch);
        }

        void BuildPlantPotDetails()
        {
            Color clay = Color.FromRgb(193, 111, 71);
            AddBox(-4.35, 0.08, 1.55, 0.43, 0.018, 0.43, Color.FromRgb(153, 75, 51));
            AddBox(-4.35, 0.25, 1.55, 0.37, 0.025, 0.37, clay);
            AddBox(-4.35, 0.348, 1.55, 0.29, 0.008, 0.29, Color.FromRgb(69, 52, 37));
            for (int rib = 0; rib < 5; rib++)
            {
                double x = -4.49 + rib * 0.07;
                AddBox(x, 0.145, 1.335, 0.008, 0.1, 0.006, Color.FromRgb(214, 137, 88));
                AddBox(x, 0.145, 1.765, 0.008, 0.1, 0.006, Color.FromRgb(214, 137, 88));
            }
        }

        void BuildPosterFrame(double x, double y, double z, double width, double height)
        {
            Color wood = Color.FromRgb(117, 80, 52);
            double rail = Math.Min(0.055, width * 0.06);
            AddBox(x, y + height / 2 - rail / 2, z, width, rail, 0.028, wood);
            AddBox(x, y - height / 2 + rail / 2, z, width, rail, 0.028, wood);
            AddBox(x - width / 2 + rail / 2, y, z, rail, height - rail * 2, 0.028, wood);
            AddBox(x + width / 2 - rail / 2, y, z, rail, height - rail * 2, 0.028, wood);
            AddBox(x, y + height / 2 - rail - 0.008, z + 0.016, width - rail * 2, 0.006, 0.004,
                Color.FromRgb(202, 166, 112));
            AddBox(x, y - height / 2 + rail + 0.008, z + 0.016, width - rail * 2, 0.006, 0.004,
                Color.FromRgb(202, 166, 112));
        }

        // Books stand on the top surface of the lower shelf (y = 2.34), flush with the back of the shelf.
        void BuildBooks()
        {
            const double shelfTop = 2.34;
            const double backZ = -2.90;
            Color[] colors =
            {
                Color.FromRgb(158, 58, 52), Color.FromRgb(52, 86, 140), Color.FromRgb(76, 122, 74),
                Color.FromRgb(214, 168, 70), Color.FromRgb(96, 66, 120), Color.FromRgb(196, 112, 62),
                Color.FromRgb(60, 110, 116)
            };
            string[] titles =
            {
                "PLANETEN UND STERNE", "JUNGE FORSCHER", "DAS GROSSE LEXIKON",
                "MAERCHENWELT", "DINOSAURIER", "RAUMFAHRT 2000", "BAUEN UND SPIELEN",
                "TIERATLAS", "ABENTEUER ERDE", "ERFINDUNGEN"
            };
            string[] authors =
            {
                "M. KELLER", "T. BRANDT", "H. WEBER", "L. FISCHER", "A. BERGER",
                "P. NEUMANN", "S. WOLF", "K. RICHTER", "J. BAUER", "R. HOFFMANN"
            };
            double[] widths = { 0.14, 0.13, 0.16, 0.12, 0.13, 0.14, 0.15, 0.13, 0.14, 0.12 };
            double[] heights = { 0.30, 0.34, 0.27, 0.32, 0.36, 0.29, 0.33, 0.26, 0.35, 0.30 };
            double[] depths = { 0.22, 0.24, 0.2, 0.23, 0.25, 0.21, 0.24, 0.2, 0.24, 0.22 };

            double x = -0.88;
            for (int i = 0; i < titles.Length; i++)
            {
                double width = widths[i];
                double height = heights[i];
                double depth = depths[i];
                double centerX = x + width / 2;
                double centerY = shelfTop + height / 2;
                double centerZ = backZ + depth / 2;
                AddBox(centerX, centerY, centerZ, width, height, depth, colors[i % colors.Length]);
                AddTexturedQuad(_Scene, centerX, centerY, backZ + depth + 0.001,
                    width * 0.9, height * 0.9, CreateBookSpineBrush(titles[i], authors[i], colors[i % colors.Length]), false);
                AddBox(centerX, shelfTop + 0.014, backZ + depth + 0.002, width * 0.76, 0.006, 0.003,
                    Color.FromRgb(220, 196, 137));
                x += width + 0.006;
            }

            // A short stack lying flat at the right end of the shelf.
            double stackY = shelfTop;
            double[] stackHeights = { 0.05, 0.04, 0.06 };
            double[] stackWidths = { 0.30, 0.26, 0.28 };
            for (int i = 0; i < stackHeights.Length; i++)
            {
                AddBox(1.7 + (i % 2) * 0.015, stackY + stackHeights[i] / 2, backZ + 0.12, stackWidths[i], stackHeights[i], 0.22, colors[(i * 2 + 1) % colors.Length]);
                AddBox(1.7 + (i % 2) * 0.015, stackY + stackHeights[i] / 2, backZ + 0.232,
                    stackWidths[i] * 0.82, 0.004, 0.003, Color.FromRgb(222, 198, 145));
                stackY += stackHeights[i];
            }
        }

        static Brush CreateBookSpineBrush(string title, string author, Color color)
        {
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                context.DrawRectangle(new SolidColorBrush(color), null, new Rect(0, 0, 128, 512));
                context.DrawRectangle(null, new Pen(new SolidColorBrush(Color.FromRgb(225, 197, 139)), 5),
                    new Rect(5, 5, 118, 502));
                context.DrawRectangle(new SolidColorBrush(Color.FromRgb(225, 197, 139)), null, new Rect(13, 24, 102, 9));
                context.DrawRectangle(new SolidColorBrush(Color.FromRgb(225, 197, 139)), null, new Rect(13, 479, 102, 9));

                FormattedText titleText = new FormattedText(title,
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Arial"), 25, new SolidColorBrush(Color.FromRgb(247, 235, 208)), 1.0);
                context.PushTransform(new RotateTransform(-90, 64, 256));
                context.DrawText(titleText, new Point((128 - titleText.Width) / 2, (512 - titleText.Height) / 2));
                context.Pop();

                FormattedText authorText = new FormattedText(author,
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Arial"), 15, new SolidColorBrush(Color.FromRgb(246, 227, 187)), 1.0)
                {
                    MaxTextWidth = 120,
                    TextAlignment = TextAlignment.Center
                };
                context.DrawText(authorText, new Point(4, 438));
            }
            DrawingBrush brush = new DrawingBrush(drawing) { Stretch = Stretch.Fill };
            Brush texture = B5RoomTextures.ToBitmap(brush);
            return texture;
        }

        static string GetKeyboardLabel(int row, int column)
        {
            string[] rows = { "1234567890", "QWERTYUIOP", "ASDFGHJKL;", "ZXCVBNM,./" };
            return rows[row][column].ToString();
        }

        static Brush CreateKeyLabelBrush(string label)
        {
            DrawingGroup drawing = new DrawingGroup();
            using (DrawingContext context = drawing.Open())
            {
                context.DrawRectangle(new SolidColorBrush(Color.FromRgb(78, 70, 59)), null,
                    new Rect(0, 0, 64, 64));
                FormattedText text = new FormattedText(label,
                    System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
                    new Typeface("Arial"), 38, new SolidColorBrush(Color.FromRgb(238, 229, 207)), 1.0);
                context.DrawText(text, new Point((64 - text.Width) / 2, (64 - text.Height) / 2));
            }
            DrawingBrush brush = new DrawingBrush(drawing) { Stretch = Stretch.Fill };
            Brush texture = B5RoomTextures.ToBitmap(brush);
            return texture;
        }

        void AddTexturedQuad(Model3DGroup parent, double x, double y, double z, double width, double height,
            Brush texture, bool horizontal)
        {
            Point3DCollection positions;
            Vector3D normal;
            PointCollection textureCoordinates;
            Int32Collection indices;
            if (horizontal)
            {
                positions = new Point3DCollection
                {
                    new Point3D(x - width / 2, y, z - height / 2),
                    new Point3D(x + width / 2, y, z - height / 2),
                    new Point3D(x + width / 2, y, z + height / 2),
                    new Point3D(x - width / 2, y, z + height / 2)
                };
                normal = new Vector3D(0, 1, 0);
                textureCoordinates = new PointCollection
                {
                    new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1)
                };
                indices = new Int32Collection { 0, 2, 1, 0, 3, 2 };
            }
            else
            {
                positions = new Point3DCollection
                {
                    new Point3D(x - width / 2, y - height / 2, z),
                    new Point3D(x + width / 2, y - height / 2, z),
                    new Point3D(x + width / 2, y + height / 2, z),
                    new Point3D(x - width / 2, y + height / 2, z)
                };
                normal = new Vector3D(0, 0, 1);
                textureCoordinates = new PointCollection
                {
                    new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0)
                };
                indices = new Int32Collection { 0, 1, 2, 0, 2, 3 };
            }

            MeshGeometry3D mesh = new MeshGeometry3D
            {
                Positions = positions,
                TextureCoordinates = textureCoordinates,
                Normals = new Vector3DCollection { normal, normal, normal, normal },
                TriangleIndices = indices
            };
            mesh.Freeze();
            DiffuseMaterial material = new DiffuseMaterial(texture);
            parent.Children.Add(new GeometryModel3D(mesh, material) { BackMaterial = material });
        }

        // Window view: lit by the room like a picture, plus some emissive light of its own.
        void AddWindowView(double x, double y, double z, double width, double height)
        {
            AddTexturedQuad(_Scene, x, y, z, width, height, WindowView, false);
            GeometryModel3D view = (GeometryModel3D)_Scene.Children[_Scene.Children.Count - 1];
            Brush glow = WindowView.Clone();
            glow.Opacity = 0.55;
            glow.Freeze();
            MaterialGroup material = new MaterialGroup();
            material.Children.Add(new DiffuseMaterial(WindowView));
            material.Children.Add(new EmissiveMaterial(glow));
            material.Freeze();
            view.Material = material;
            view.BackMaterial = null;
        }

        GeometryModel3D AddBox(double x, double y, double z, double width, double height, double depth, Color color)
        {
            return AddBox(_Scene, x, y, z, width, height, depth, color);
        }

        // Flat rectangular frame (four bars) around a footprint, as one model so it can be lit up as a whole.
        GeometryModel3D AddFrame(double x, double y, double z, double width, double depth, double thickness)
        {
            const double height = 0.012;
            double innerDepth = depth - 2 * thickness;
            MeshGeometry3D mesh = new MeshGeometry3D();
            AppendBox(mesh, x, y, z - depth / 2 + thickness / 2, width, height, thickness);
            AppendBox(mesh, x, y, z + depth / 2 - thickness / 2, width, height, thickness);
            AppendBox(mesh, x - width / 2 + thickness / 2, y, z, thickness, height, innerDepth);
            AppendBox(mesh, x + width / 2 - thickness / 2, y, z, thickness, height, innerDepth);
            DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(Color.FromArgb(0, 255, 213, 92)));
            GeometryModel3D model = new GeometryModel3D(mesh, material) { BackMaterial = material };
            _Scene.Children.Add(model);
            return model;
        }

        internal static GeometryModel3D AddBox(Model3DGroup parent, double x, double y, double z, double width, double height, double depth, Color color)
        {
            MeshGeometry3D mesh = new MeshGeometry3D();
            AppendBox(mesh, x, y, z, width, height, depth);
            mesh.Freeze();
            ClosedMeshes.Add(mesh, null);
            Material material = GetBoxMaterial(color);
            GeometryModel3D model = new GeometryModel3D(mesh, material) { BackMaterial = material };
            parent.Children.Add(model);
            return model;
        }

        static void AppendBox(MeshGeometry3D mesh, double x, double y, double z, double width, double height, double depth)
        {
            double x0 = x - width / 2, x1 = x + width / 2;
            double y0 = y - height / 2, y1 = y + height / 2;
            double z0 = z - depth / 2, z1 = z + depth / 2;
            Point3D[][] faces =
            {
                new[] { new Point3D(x0,y0,z1), new Point3D(x1,y0,z1), new Point3D(x1,y1,z1), new Point3D(x0,y1,z1) },
                new[] { new Point3D(x1,y0,z0), new Point3D(x0,y0,z0), new Point3D(x0,y1,z0), new Point3D(x1,y1,z0) },
                new[] { new Point3D(x0,y1,z1), new Point3D(x1,y1,z1), new Point3D(x1,y1,z0), new Point3D(x0,y1,z0) },
                new[] { new Point3D(x0,y0,z0), new Point3D(x1,y0,z0), new Point3D(x1,y0,z1), new Point3D(x0,y0,z1) },
                new[] { new Point3D(x1,y0,z1), new Point3D(x1,y0,z0), new Point3D(x1,y1,z0), new Point3D(x1,y1,z1) },
                new[] { new Point3D(x0,y0,z0), new Point3D(x0,y0,z1), new Point3D(x0,y1,z1), new Point3D(x0,y1,z0) }
            };
            Vector3D[] normals = { new Vector3D(0,0,1), new Vector3D(0,0,-1), new Vector3D(0,1,0), new Vector3D(0,-1,0), new Vector3D(1,0,0), new Vector3D(-1,0,0) };
            for (int face = 0; face < faces.Length; face++)
            {
                int start = mesh.Positions.Count;
                foreach (Point3D point in faces[face])
                {
                    mesh.Positions.Add(point);
                    mesh.Normals.Add(normals[face]);
                }
                mesh.TextureCoordinates.Add(new Point(0, 1));
                mesh.TextureCoordinates.Add(new Point(1, 1));
                mesh.TextureCoordinates.Add(new Point(1, 0));
                mesh.TextureCoordinates.Add(new Point(0, 0));
                mesh.TriangleIndices.Add(start); mesh.TriangleIndices.Add(start + 1); mesh.TriangleIndices.Add(start + 2);
                mesh.TriangleIndices.Add(start); mesh.TriangleIndices.Add(start + 2); mesh.TriangleIndices.Add(start + 3);
            }
        }

        GeometryModel3D AddWoodBox(double x, double y, double z, double width, double height, double depth)
        {
            return AddTexturedBox(x, y, z, width, height, depth, WoodTexture);
        }

        GeometryModel3D AddTexturedBox(double x, double y, double z, double width, double height, double depth,
            Brush texture)
        {
            GeometryModel3D model = AddBox(x, y, z, width, height, depth, Colors.White);
            Material material = GetTexturedMaterial(texture);
            model.Material = material;
            model.BackMaterial = material;
            return model;
        }

        internal static Material GetBoxMaterial(Color color)
        {
            Material material;
            if (BoxMaterialCache.TryGetValue(color, out material)) return material;

            // Diffuse only: every material in a MaterialGroup is drawn as an extra pass over the whole mesh,
            // and the old dark specular layer was hardly visible but doubled the drawing work.
            DiffuseMaterial diffuse = new DiffuseMaterial(GetFrozenBrush(color));
            diffuse.Freeze();
            BoxMaterialCache.Add(color, diffuse);
            return diffuse;
        }

        static Material GetEllipsoidMaterial(Color color, Color highlight)
        {
            (Color Color, Color Highlight) key = (color, highlight);
            Material material;
            if (EllipsoidMaterialCache.TryGetValue(key, out material)) return material;

            MaterialGroup group = new MaterialGroup();
            group.Children.Add(new DiffuseMaterial(GetFrozenBrush(color)));
            group.Children.Add(new SpecularMaterial(GetFrozenBrush(highlight), 42));
            group.Freeze();
            EllipsoidMaterialCache.Add(key, group);
            return group;
        }

        static Material GetTexturedMaterial(Brush texture)
        {
            Material material;
            if (TexturedMaterialCache.TryGetValue(texture, out material)) return material;

            DiffuseMaterial diffuse = new DiffuseMaterial(texture);
            diffuse.Freeze();
            TexturedMaterialCache.Add(texture, diffuse);
            return diffuse;
        }

        static Brush GetFrozenBrush(Color color)
        {
            Brush brush;
            if (SolidBrushCache.TryGetValue(color, out brush)) return brush;

            SolidColorBrush solidBrush = new SolidColorBrush(color);
            solidBrush.Freeze();
            SolidBrushCache.Add(color, solidBrush);
            return solidBrush;
        }

        static Brush CreateFloorShadowBrush()
        {
            RadialGradientBrush gradient = new RadialGradientBrush
            {
                Center = new Point(0.5, 0.5),
                GradientOrigin = new Point(0.5, 0.5),
                RadiusX = 0.5,
                RadiusY = 0.5,
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromArgb(78, 20, 15, 11), 0),
                    new GradientStop(Color.FromArgb(36, 20, 15, 11), 0.5),
                    new GradientStop(Color.FromArgb(0, 20, 15, 11), 1)
                }
            };
            DrawingGroup drawing = new DrawingGroup();
            drawing.Children.Add(new GeometryDrawing(gradient, null, new EllipseGeometry(new Rect(0, 0, 1, 1))));
            DrawingBrush brush = new DrawingBrush(drawing) { Stretch = Stretch.Fill };
            Brush texture = B5RoomTextures.ToBitmap(brush);
            return texture;
        }

        static Material CreateFloorShadowMaterial()
        {
            DiffuseMaterial material = new DiffuseMaterial(FloorShadowBrush);
            material.Freeze();
            return material;
        }

        static Material[,] CreateScreenMaterials()
        {
            Material[,] materials = new Material[3, 2];
            for (int i = 0; i < 3; i++)
            {
                Color dark = i == (int)B5RoomDeviceKind.Gameboy ? Color.FromRgb(32, 43, 24) :
                    i == (int)B5RoomDeviceKind.Commodore ? Color.FromRgb(22, 18, 35) : Color.FromRgb(16, 15, 15);
                Color lit = i == (int)B5RoomDeviceKind.Gameboy ? Color.FromRgb(118, 139, 54) :
                    i == (int)B5RoomDeviceKind.Commodore ? Color.FromRgb(62, 48, 123) : Color.FromRgb(174, 87, 43);
                materials[i, 0] = CreateFrozenMaterial(dark, null);
                materials[i, 1] = CreateFrozenMaterial(lit, lit);
            }
            // The Atari TV has a glass tube: reflective when off, an attract screen when on.
            int atari = (int)B5RoomDeviceKind.Atari;
            materials[atari, 0] = B5AtariTelevisionModel.CreateScreenOffMaterial();
            materials[atari, 1] = B5AtariTelevisionModel.CreateScreenOnMaterial(B5RoomRegistry.Devices[atari].ScreenColor);
            return materials;
        }

        static Material[] CreateHighlightMaterials()
        {
            // Unselected frames get no material at all: an invisible (alpha 0) material would still be drawn.
            return new[]
            {
                null,
                CreateFrozenMaterial(Color.FromArgb(145, 239, 190, 94), Color.FromArgb(75, 190, 127, 51))
            };
        }

        static Material CreateFrozenMaterial(Color diffuse, Color? emissive)
        {
            MaterialGroup material = new MaterialGroup();
            material.Children.Add(new DiffuseMaterial(GetFrozenBrush(diffuse)));
            if (emissive.HasValue) material.Children.Add(new EmissiveMaterial(GetFrozenBrush(emissive.Value)));
            material.Freeze();
            return material;
        }

        void OptimizeStaticScene()
        {
            HashSet<GeometryModel3D> mutableHighlights = new HashSet<GeometryModel3D>(_Highlights);
            OptimizeModelGroup(_Scene, mutableHighlights);
            foreach (Model3DGroup controls in _DeviceControls)
            {
                OptimizeModelGroup(controls, null);
                if (controls.CanFreeze) controls.Freeze();
            }
        }

        static void OptimizeModelGroup(Model3DGroup group, HashSet<GeometryModel3D> excluded)
        {
            List<Model3D> retained = new List<Model3D>();
            // Printed labels (single-sided, partly transparent) must be drawn after the surfaces they lie on,
            // otherwise their transparent parts hide those surfaces.
            List<Model3D> decals = new List<Model3D>();
            // Closed boxes and open meshes are merged separately: only open meshes need their back faces drawn.
            Dictionary<(Material Material, bool Closed), MeshGeometry3D> batches =
                new Dictionary<(Material Material, bool Closed), MeshGeometry3D>();
            foreach (Model3D child in group.Children)
            {
                GeometryModel3D model = child as GeometryModel3D;
                MeshGeometry3D mesh = model == null ? null : model.Geometry as MeshGeometry3D;
                if (model == null || mesh == null || model.Material == null ||
                    !ReferenceEquals(model.Material, model.BackMaterial) ||
                    (excluded != null && excluded.Contains(model)) ||
                    ReferenceEquals(model.Material, FloorShadowMaterial))
                {
                    if (mesh != null && mesh.CanFreeze) mesh.Freeze();
                    if (model != null && (excluded == null || !excluded.Contains(model)) && model.CanFreeze)
                        model.Freeze();
                    else if (model == null && child.CanFreeze)
                        child.Freeze();
                    if (model != null && model.Material != null && model.BackMaterial == null) decals.Add(child);
                    else retained.Add(child);
                    continue;
                }

                object unused;
                (Material Material, bool Closed) key = (model.Material, ClosedMeshes.TryGetValue(mesh, out unused));
                MeshGeometry3D batch;
                if (!batches.TryGetValue(key, out batch))
                {
                    batch = new MeshGeometry3D();
                    batches.Add(key, batch);
                }
                AppendMesh(batch, mesh);
            }

            group.Children.Clear();
            foreach (Model3D model in retained) group.Children.Add(model);
            foreach (KeyValuePair<(Material Material, bool Closed), MeshGeometry3D> batch in batches)
            {
                batch.Value.Freeze();
                GeometryModel3D merged = new GeometryModel3D(batch.Value, batch.Key.Material)
                {
                    BackMaterial = batch.Key.Closed ? null : batch.Key.Material
                };
                merged.Freeze();
                group.Children.Add(merged);
            }
            foreach (Model3D decal in decals) group.Children.Add(decal);
        }

        static void AppendMesh(MeshGeometry3D destination, MeshGeometry3D source)
        {
            int offset = destination.Positions.Count;
            foreach (Point3D point in source.Positions) destination.Positions.Add(point);
            foreach (Vector3D normal in source.Normals) destination.Normals.Add(normal);
            foreach (Point coordinate in source.TextureCoordinates) destination.TextureCoordinates.Add(coordinate);
            foreach (int index in source.TriangleIndices) destination.TriangleIndices.Add(offset + index);
        }

        static Brush CreateWoodTexture()
        {
            DrawingGroup drawing = new DrawingGroup();
            drawing.Children.Add(new GeometryDrawing(
                new SolidColorBrush(Color.FromRgb(172, 119, 76)), null, new RectangleGeometry(new Rect(0, 0, 128, 128))));

            GeometryGroup grain = new GeometryGroup();
            for (int i = 0; i < 13; i++)
            {
                double y = 7 + i * 9;
                grain.Children.Add(new RectangleGeometry(new Rect(0, y, 128, i % 3 == 0 ? 1.1 : 0.55)));
                grain.Children.Add(new RectangleGeometry(new Rect((i * 29) % 80, y + 2, 35 + (i % 4) * 9, 0.4)));
            }
            grain.Children.Add(new EllipseGeometry(new Point(85, 52), 13, 4));
            grain.Children.Add(new EllipseGeometry(new Point(85, 52), 8, 2));
            drawing.Children.Add(new GeometryDrawing(null,
                new Pen(new SolidColorBrush(Color.FromArgb(52, 72, 39, 22)), 1.0), grain));

            DrawingBrush brush = new DrawingBrush(drawing)
            {
                Stretch = Stretch.Fill,
                TileMode = TileMode.Tile,
                Viewbox = new Rect(0, 0, 128, 128),
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, 0.38, 0.62),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox
            };
            Brush texture = B5RoomTextures.ToBitmap(brush);
            return texture;
        }

        static Brush CreateWallpaperTexture()
        {
            DrawingGroup drawing = new DrawingGroup();
            drawing.Children.Add(new GeometryDrawing(
                new SolidColorBrush(Color.FromRgb(228, 213, 184)), null, new RectangleGeometry(new Rect(0, 0, 64, 64))));
            GeometryGroup pattern = new GeometryGroup();
            for (int x = 5; x < 64; x += 16)
            {
                pattern.Children.Add(new RectangleGeometry(new Rect(x, 0, 0.7, 64)));
                for (int y = 8; y < 64; y += 16)
                {
                    pattern.Children.Add(new EllipseGeometry(new Point(x + 6, y), 1.3, 2.2));
                    pattern.Children.Add(new EllipseGeometry(new Point(x + 9, y + 3), 1.3, 2.2));
                }
            }
            drawing.Children.Add(new GeometryDrawing(null,
                new Pen(new SolidColorBrush(Color.FromArgb(32, 127, 105, 78)), 0.7), pattern));

            DrawingBrush brush = new DrawingBrush(drawing)
            {
                Stretch = Stretch.Fill,
                TileMode = TileMode.Tile,
                Viewbox = new Rect(0, 0, 64, 64),
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, 0.06, 0.12),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox
            };
            Brush texture = B5RoomTextures.ToBitmap(brush);
            return texture;
        }

        static Brush CreateCarpetTexture()
        {
            DrawingGroup drawing = new DrawingGroup();
            drawing.Children.Add(new GeometryDrawing(
                new SolidColorBrush(Color.FromRgb(129, 58, 60)), null, new RectangleGeometry(new Rect(0, 0, 64, 64))));
            GeometryGroup weave = new GeometryGroup();
            for (int i = 0; i < 64; i += 4)
            {
                weave.Children.Add(new RectangleGeometry(new Rect(i, 0, 0.6, 64)));
                weave.Children.Add(new RectangleGeometry(new Rect(0, i, 64, 0.6)));
            }
            for (int i = 7; i < 64; i += 14)
                weave.Children.Add(new RectangleGeometry(new Rect(i, i, 2, 2)));
            drawing.Children.Add(new GeometryDrawing(null,
                new Pen(new SolidColorBrush(Color.FromArgb(55, 231, 183, 132)), 0.8), weave));

            DrawingBrush brush = new DrawingBrush(drawing)
            {
                Stretch = Stretch.Fill,
                TileMode = TileMode.Tile,
                Viewbox = new Rect(0, 0, 64, 64),
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, 0.18, 0.18),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox
            };
            Brush texture = B5RoomTextures.ToBitmap(brush);
            return texture;
        }

        static Brush CreateCurtainTexture()
        {
            DrawingGroup drawing = new DrawingGroup();
            drawing.Children.Add(new GeometryDrawing(
                new SolidColorBrush(Color.FromRgb(160, 83, 62)), null, new RectangleGeometry(new Rect(0, 0, 64, 64))));
            GeometryGroup folds = new GeometryGroup();
            for (int x = 5; x < 64; x += 10)
                folds.Children.Add(new RectangleGeometry(new Rect(x, 0, 1.3, 64)));
            drawing.Children.Add(new GeometryDrawing(null,
                new Pen(new SolidColorBrush(Color.FromArgb(72, 240, 178, 132)), 1.2), folds));

            DrawingBrush brush = new DrawingBrush(drawing)
            {
                Stretch = Stretch.Fill,
                TileMode = TileMode.Tile,
                Viewbox = new Rect(0, 0, 64, 64),
                ViewboxUnits = BrushMappingMode.Absolute,
                Viewport = new Rect(0, 0, 0.7, 0.45),
                ViewportUnits = BrushMappingMode.RelativeToBoundingBox
            };
            Brush texture = B5RoomTextures.ToBitmap(brush);
            return texture;
        }

        GeometryModel3D AddSphere(double x, double y, double z, double radius, Color color, Color highlight)
        {
            return AddEllipsoid(x, y, z, radius, radius, radius, color, highlight);
        }

        GeometryModel3D AddEllipsoid(double x, double y, double z, double radiusX, double radiusY,
            double radiusZ, Color color)
        {
            return AddEllipsoid(x, y, z, radiusX, radiusY, radiusZ, color,
                Color.FromRgb(175, 164, 146));
        }

        GeometryModel3D AddEllipsoid(double x, double y, double z, double radiusX, double radiusY,
            double radiusZ, Color color, Color highlight)
        {
            const int slices = 20;
            const int stacks = 12;
            MeshGeometry3D mesh = new MeshGeometry3D();
            for (int stack = 0; stack <= stacks; stack++)
            {
                double latitude = Math.PI * stack / stacks;
                double sinLatitude = Math.Sin(latitude);
                double cosLatitude = Math.Cos(latitude);
                for (int slice = 0; slice <= slices; slice++)
                {
                    double longitude = 2 * Math.PI * slice / slices;
                    double nx = sinLatitude * Math.Cos(longitude);
                    double ny = cosLatitude;
                    double nz = sinLatitude * Math.Sin(longitude);
                    mesh.Positions.Add(new Point3D(x + nx * radiusX, y + ny * radiusY, z + nz * radiusZ));
                    Vector3D normal = new Vector3D(nx / radiusX, ny / radiusY, nz / radiusZ);
                    normal.Normalize();
                    mesh.Normals.Add(normal);
                }
            }

            for (int stack = 0; stack < stacks; stack++)
            {
                for (int slice = 0; slice < slices; slice++)
                {
                    int topLeft = stack * (slices + 1) + slice;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + slices + 1;
                    int bottomRight = bottomLeft + 1;
                    mesh.TriangleIndices.Add(topLeft);
                    mesh.TriangleIndices.Add(bottomLeft);
                    mesh.TriangleIndices.Add(topRight);
                    mesh.TriangleIndices.Add(topRight);
                    mesh.TriangleIndices.Add(bottomLeft);
                    mesh.TriangleIndices.Add(bottomRight);
                }
            }

            mesh.Freeze();
            Material material = GetEllipsoidMaterial(color, highlight);
            GeometryModel3D sphere = new GeometryModel3D(mesh, material) { BackMaterial = material };
            _Scene.Children.Add(sphere);
            return sphere;
        }

        void AddFloorShadow(double x, double z, double width, double depth)
        {
            MeshGeometry3D mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection
                {
                    new Point3D(x - width / 2, 0.024, z - depth / 2),
                    new Point3D(x + width / 2, 0.024, z - depth / 2),
                    new Point3D(x + width / 2, 0.024, z + depth / 2),
                    new Point3D(x - width / 2, 0.024, z + depth / 2)
                },
                TextureCoordinates = new PointCollection
                {
                    new Point(0, 0), new Point(1, 0), new Point(1, 1), new Point(0, 1)
                },
                Normals = new Vector3DCollection
                {
                    new Vector3D(0, 1, 0), new Vector3D(0, 1, 0),
                    new Vector3D(0, 1, 0), new Vector3D(0, 1, 0)
                },
                TriangleIndices = new Int32Collection { 0, 2, 1, 0, 3, 2 }
            };
            mesh.Freeze();
            _Scene.Children.Add(new GeometryModel3D(mesh, FloorShadowMaterial) { BackMaterial = FloorShadowMaterial });
        }

        void AddDisplayQuad(double x, double y, double z, double width, double height)
        {
            MeshGeometry3D mesh = new MeshGeometry3D
            {
                Positions = new Point3DCollection
                {
                    new Point3D(x - width / 2, y - height / 2, z),
                    new Point3D(x + width / 2, y - height / 2, z),
                    new Point3D(x + width / 2, y + height / 2, z),
                    new Point3D(x - width / 2, y + height / 2, z)
                },
                TextureCoordinates = new PointCollection { new Point(0, 1), new Point(1, 1), new Point(1, 0), new Point(0, 0) },
                TriangleIndices = new Int32Collection { 0, 1, 2, 0, 2, 3 },
                Normals = new Vector3DCollection { new Vector3D(0, 0, 1), new Vector3D(0, 0, 1), new Vector3D(0, 0, 1), new Vector3D(0, 0, 1) }
            };
            mesh.Freeze();
            AddDisplay(mesh);
        }

        // Screens are added in device order; UpdateScreenLighting gives each its material.
        void AddDisplay(MeshGeometry3D mesh)
        {
            GeometryModel3D model = new GeometryModel3D(mesh, null);
            _Scene.Children.Add(model);
            _Screens.Add(model);
        }

        void UpdateInterface(B5RoomField field)
        {
            bool deviceView = field.Phase != B5RoomPhase.Room;
            bool focusing = field.Phase == B5RoomPhase.Focusing;
            bool playing = field.Phase == B5RoomPhase.Playing;
            _ControlBar.Update(field);
            Visibility sidebarVisibility = deviceView && !focusing && !playing ? Visibility.Visible : Visibility.Collapsed;
            Visibility frameVisibility = playing ? Visibility.Visible : Visibility.Collapsed;
            Visibility roomVisibility = playing ? Visibility.Collapsed : Visibility.Visible;
            Visibility controlBarVisibility = focusing || playing ? Visibility.Collapsed : Visibility.Visible;
            int gameWidth = playing && field.ActiveGame != null ? field.ActiveGame.Width : field.Device.DisplayWidth;
            int gameHeight = playing && field.ActiveGame != null ? field.ActiveGame.Height : field.Device.DisplayHeight;
            if (_Sidebar.Visibility != sidebarVisibility) _Sidebar.Visibility = sidebarVisibility;
            Thickness sidebarMargin = new Thickness(0, 8, 8, _ControlBar.OccupiedHeight);
            if (_Sidebar.Margin != sidebarMargin) _Sidebar.Margin = sidebarMargin;
            if (_GameFrame.Visibility != frameVisibility) _GameFrame.Visibility = frameVisibility;
            if (_ConsoleShell.Visibility != frameVisibility) _ConsoleShell.Visibility = frameVisibility;
            if (_Viewport.Visibility != roomVisibility) _Viewport.Visibility = roomVisibility;
            if (_ControlBar.Element.Visibility != controlBarVisibility) _ControlBar.Element.Visibility = controlBarVisibility;
            ConfigureGameOverlay(field);
            SizeGameFrame(field, gameWidth, gameHeight);
            if (_GameCanvas.Width != gameWidth) _GameCanvas.Width = gameWidth;
            if (_GameCanvas.Height != gameHeight) _GameCanvas.Height = gameHeight;
            UpdateMedia(field);
            UpdateScreenLighting(field);

            bool rebuildCards = _LastDevice != field.SelectedDevice;
            if (rebuildCards)
            {
                _LastDevice = field.SelectedDevice;
                B5RoomDevice device = field.Device;
                _DeviceTitle.Text = device.Name;
                _Cards.Children.Clear();
                _CardBorders.Clear();
                _CardTitles.Clear();
                for (int i = 0; i < device.Games.Count; i++)
                {
                    B5RoomGameEntry game = device.Games[i];
                    int index = i;
                    TextBlock title = B5RetroStyle.CreateText(game.Title, 16, B5RetroStyle.Text);
                    title.TextWrapping = TextWrapping.Wrap;
                    title.VerticalAlignment = VerticalAlignment.Center;
                    DockPanel row = new DockPanel();
                    Border colorMark = new Border { Width = 10, Height = 10, Margin = new Thickness(0, 0, 8, 0), Background = new SolidColorBrush(game.CartridgeColor) };
                    DockPanel.SetDock(colorMark, Dock.Left);
                    row.Children.Add(colorMark);
                    row.Children.Add(title);
                    Border card = B5RetroStyle.CreateSelectable(row);
                    card.Margin = new Thickness(0, 2, 0, 2);
                    // Mouse: pointing selects the game, a click inserts or starts it (like Enter).
                    card.Cursor = Cursors.Hand;
                    card.MouseEnter += (sender, e) =>
                    {
                        if (_LastField != null) _LastField.HoverGame(index);
                    };
                    card.MouseLeftButtonDown += (sender, e) =>
                    {
                        if (_LastField == null) return;
                        _LastField.HoverGame(index);
                        _LastField.LatchKey(Key.Enter);
                    };
                    _Cards.Children.Add(card);
                    _CardBorders.Add(card);
                    _CardTitles.Add(title);
                }
            }

            bool cardStateChanged = rebuildCards || _LastSelected != field.SelectedGame ||
                _LastCardPhase != field.Phase || _LastCardPending != field.PendingGame ||
                _LastCardInserted != field.InsertedGame;
            if (cardStateChanged)
            {
                for (int i = 0; i < _CardBorders.Count; i++)
                {
                    B5RetroStyle.SetSelected(_CardBorders[i], i == field.SelectedGame);
                    if (field.Phase == B5RoomPhase.Inserting && i == field.PendingGame)
                    {
                        _CardBorders[i].Background = Brushes.Transparent;
                        _CardBorders[i].BorderBrush = B5RetroStyle.Frame;
                        _CardTitles[i].Text = "  . . .";
                    }
                    else
                    {
                        _CardTitles[i].Text = B5RoomRegistry.Devices[field.SelectedDevice].Games[i].Title +
                            (i == field.InsertedGame ? "  [EINGELEGT]" : "");
                    }
                }
                _LastSelected = field.SelectedGame;
                _LastCardPhase = field.Phase;
                _LastCardPending = field.PendingGame;
                _LastCardInserted = field.InsertedGame;
            }

            if (_LastHighlightDevice != field.SelectedDevice)
            {
                _LastHighlightDevice = field.SelectedDevice;
                for (int i = 0; i < _Highlights.Count; i++)
                {
                    Material material = HighlightMaterials[i == field.SelectedDevice ? 1 : 0];
                    _Highlights[i].Material = material;
                    _Highlights[i].BackMaterial = material;
                }
            }
        }

        void ConfigureGameOverlay(B5RoomField field)
        {
            bool monitor = IsMonitorView(field);
            if (_LastOverlayDevice == field.SelectedDevice && _LastOverlayMonitor == monitor) return;
            _LastOverlayDevice = field.SelectedDevice;
            _LastOverlayMonitor = monitor;

            _GameFrame.BorderThickness = new Thickness(8);
            _ConsoleShell.BorderThickness = new Thickness(4);
            _ConsoleShellTitle.FontSize = 14;
            _ConsoleShellTitle.VerticalAlignment = VerticalAlignment.Top;
            _ConsoleShellTitle.Margin = new Thickness(0, 9, 0, 0);
            _ScreenEffect.Visibility = monitor ? Visibility.Visible : Visibility.Collapsed;

            if (monitor)
            {
                // Beige CRT monitor: thin plastic bezel, dark glass edge, label at the bottom.
                _GameFrame.BorderThickness = new Thickness(MonitorGlass);
                _GameFrame.BorderBrush = GetFrozenBrush(Color.FromRgb(22, 22, 24));
                _GameFrame.Background = GetFrozenBrush(Color.FromRgb(10, 10, 12));
                _GameFrame.CornerRadius = new CornerRadius(14);
                _ConsoleShell.BorderThickness = new Thickness(2);
                _ConsoleShell.BorderBrush = GetFrozenBrush(Color.FromRgb(120, 112, 96));
                _ConsoleShell.Background = GetFrozenBrush(Color.FromRgb(200, 191, 166));
                _ConsoleShell.CornerRadius = new CornerRadius(18);
                _ConsoleShellTitle.Text = "COMMODORE";
                _ConsoleShellTitle.FontSize = 11;
                _ConsoleShellTitle.Foreground = GetFrozenBrush(Color.FromRgb(92, 86, 74));
                _ConsoleShellTitle.VerticalAlignment = VerticalAlignment.Bottom;
                _ConsoleShellTitle.Margin = new Thickness(0, 0, 0, 3);
                _Root.Background = GetFrozenBrush(Color.FromRgb(28, 27, 31));
                return;
            }

            // Back to the normal console view: the shell sizes itself again like before the monitor view.
            _ConsoleShell.Width = double.NaN;
            _ConsoleShell.Height = double.NaN;

            switch (field.Device.Kind)
            {
                case B5RoomDeviceKind.Atari:
                    _GameFrame.BorderBrush = GetFrozenBrush(Color.FromRgb(151, 92, 54));
                    _GameFrame.Background = GetFrozenBrush(Color.FromRgb(43, 30, 25));
                    _GameFrame.CornerRadius = new CornerRadius(2);
                    _ConsoleShell.BorderBrush = GetFrozenBrush(Color.FromRgb(93, 54, 38));
                    _ConsoleShell.Background = GetFrozenBrush(Color.FromRgb(111, 73, 52));
                    _ConsoleShell.CornerRadius = new CornerRadius(10);
                    _ConsoleShellTitle.Text = "ATARI";
                    _ConsoleShellTitle.Foreground = GetFrozenBrush(Color.FromRgb(244, 208, 145));
                    _Root.Background = GetFrozenBrush(Color.FromRgb(47, 35, 31));
                    break;
                case B5RoomDeviceKind.Gameboy:
                    _GameFrame.BorderBrush = GetFrozenBrush(Color.FromRgb(146, 160, 104));
                    _GameFrame.Background = GetFrozenBrush(Color.FromRgb(42, 52, 33));
                    _GameFrame.CornerRadius = new CornerRadius(10);
                    _ConsoleShell.BorderBrush = GetFrozenBrush(Color.FromRgb(78, 88, 55));
                    _ConsoleShell.Background = GetFrozenBrush(Color.FromRgb(157, 166, 112));
                    _ConsoleShell.CornerRadius = new CornerRadius(24);
                    _ConsoleShellTitle.Text = "GAME BOY";
                    _ConsoleShellTitle.Foreground = GetFrozenBrush(Color.FromRgb(45, 59, 35));
                    _Root.Background = GetFrozenBrush(Color.FromRgb(34, 45, 31));
                    break;
                case B5RoomDeviceKind.Commodore:
                    _GameFrame.BorderBrush = GetFrozenBrush(Color.FromRgb(181, 159, 119));
                    _GameFrame.Background = GetFrozenBrush(Color.FromRgb(24, 25, 28));
                    _GameFrame.CornerRadius = new CornerRadius(3);
                    _ConsoleShell.BorderBrush = GetFrozenBrush(Color.FromRgb(107, 101, 88));
                    _ConsoleShell.Background = GetFrozenBrush(Color.FromRgb(190, 185, 169));
                    _ConsoleShell.CornerRadius = new CornerRadius(7);
                    _ConsoleShellTitle.Text = "COMMODORE 64";
                    _ConsoleShellTitle.Foreground = GetFrozenBrush(Color.FromRgb(54, 52, 48));
                    _Root.Background = GetFrozenBrush(Color.FromRgb(45, 47, 55));
                    break;
            }
        }

        const double MonitorGlass = 5;
        const double MonitorOuterMargin = 6;
        const double MonitorBezelSide = 12;
        const double MonitorBezelTop = 12;
        const double MonitorBezelBottom = 22;

        // Width/Height start as NaN (auto size); a plain difference check would never set them.
        static void SetWidth(FrameworkElement element, double value)
        {
            if (double.IsNaN(element.Width) || Math.Abs(element.Width - value) > 0.1) element.Width = value;
        }

        static void SetHeight(FrameworkElement element, double value)
        {
            if (double.IsNaN(element.Height) || Math.Abs(element.Height - value) > 0.1) element.Height = value;
        }

        // Monitor view: the screen fills the canvas except for a thin bezel. The scale is not rounded,
        // so the game grows with the window (1920x1080 when there is enough room).
        void SizeMonitorFrame(int gameWidth, int gameHeight)
        {
            double rootWidth = Math.Max(1, _Root.ActualWidth);
            double rootHeight = Math.Max(1, _Root.ActualHeight);
            double glass = MonitorGlass * 2;
            double availableWidth = rootWidth - MonitorOuterMargin * 2 - MonitorBezelSide * 2 - glass;
            double availableHeight = rootHeight - MonitorOuterMargin * 2 - MonitorBezelTop - MonitorBezelBottom - glass;
            double scale = Math.Max(0.05, Math.Min(1.0, Math.Min(availableWidth / gameWidth, availableHeight / gameHeight)));
            double frameWidth = gameWidth * scale + glass;
            double frameHeight = gameHeight * scale + glass;
            double shellWidth = frameWidth + MonitorBezelSide * 2;
            double shellHeight = frameHeight + MonitorBezelTop + MonitorBezelBottom;
            double shellLeft = Math.Max(0, (rootWidth - shellWidth) / 2);
            double shellTop = Math.Max(0, (rootHeight - shellHeight) / 2);

            SetWidth(_GameFrame, frameWidth);
            SetHeight(_GameFrame, frameHeight);
            SetWidth(_ConsoleShell, shellWidth);
            SetHeight(_ConsoleShell, shellHeight);
            if (_GameFrame.VerticalAlignment != VerticalAlignment.Top) _GameFrame.VerticalAlignment = VerticalAlignment.Top;
            if (_ConsoleShell.VerticalAlignment != VerticalAlignment.Top) _ConsoleShell.VerticalAlignment = VerticalAlignment.Top;
            Thickness frameMargin = new Thickness(shellLeft + MonitorBezelSide, shellTop + MonitorBezelTop, 0, 0);
            Thickness shellMargin = new Thickness(shellLeft, shellTop, 0, 0);
            if (_GameFrame.Margin != frameMargin) _GameFrame.Margin = frameMargin;
            if (_ConsoleShell.Margin != shellMargin) _ConsoleShell.Margin = shellMargin;
        }

        void SizeGameFrame(B5RoomField field, int gameWidth, int gameHeight)
        {
            if (IsMonitorView(field))
            {
                SizeMonitorFrame(gameWidth, gameHeight);
                return;
            }
            if (_GameFrame.VerticalAlignment != VerticalAlignment.Center) _GameFrame.VerticalAlignment = VerticalAlignment.Center;
            if (_ConsoleShell.VerticalAlignment != VerticalAlignment.Center) _ConsoleShell.VerticalAlignment = VerticalAlignment.Center;
            double rootWidth = Math.Max(1, _Root.ActualWidth);
            double rootHeight = Math.Max(1, _Root.ActualHeight);
            bool sidebarVisible = _Sidebar.Visibility == Visibility.Visible;
            double usableWidth = Math.Max(1, rootWidth - (sidebarVisible ? _Sidebar.Width : 0) - 32);
            double barHeight = _ControlBar.OccupiedHeight;
            double usableHeight = Math.Max(1, rootHeight - barHeight - 32);
            double border = _GameFrame.BorderThickness.Left + _GameFrame.BorderThickness.Right;
            double verticalBorder = _GameFrame.BorderThickness.Top + _GameFrame.BorderThickness.Bottom;
            double scale = Math.Min(
                (usableWidth - border) / gameWidth,
                (usableHeight - verticalBorder) / gameHeight);
            if (field.ActiveGame is B5RoomTowerDefenseGame && scale >= 1)
                scale = Math.Floor(scale);
            else
                scale = Math.Max(0.1, scale);
            double frameWidth = gameWidth * scale + border;
            double frameHeight = gameHeight * scale + verticalBorder;
            double marginLeft = Math.Max(0, (usableWidth - frameWidth) / 2);
            double shellHorizontalPadding = field.Device.Kind == B5RoomDeviceKind.Atari ? 52 : 38;
            double shellVerticalPadding = 32;
            SetWidth(_GameFrame, frameWidth);
            SetHeight(_GameFrame, frameHeight);
            Thickness frameMargin = new Thickness(marginLeft, 0, 0, barHeight);
            if (_GameFrame.Margin != frameMargin) _GameFrame.Margin = frameMargin;
            double shellWidth = frameWidth + shellHorizontalPadding * 2;
            double shellHeight = frameHeight + shellVerticalPadding * 2;
            Thickness shellMargin = new Thickness(Math.Max(0, marginLeft - shellHorizontalPadding), 0, 0, barHeight);
            if (Math.Abs(_ConsoleShell.Width - shellWidth) > 0.1) _ConsoleShell.Width = shellWidth;
            if (Math.Abs(_ConsoleShell.Height - shellHeight) > 0.1) _ConsoleShell.Height = shellHeight;
            if (_ConsoleShell.Margin != shellMargin) _ConsoleShell.Margin = shellMargin;
        }

        void UpdateScreenLighting(B5RoomField field)
        {
            if (_LastScreenDevice == field.SelectedDevice) return;
            _LastScreenDevice = field.SelectedDevice;
            for (int i = 0; i < _Screens.Count; i++)
            {
                Material material = ScreenMaterials[i, i == field.SelectedDevice ? 1 : 0];
                _Screens[i].Material = material;
                _Screens[i].BackMaterial = material;
            }
        }

        // Every device animates its own 3D medium (cartridge or disk) and its moving parts.
        void UpdateMedia(B5RoomField field)
        {
            foreach (B5MediaAnimation media in _Media) media.Update(field);
        }

        void DrawGame(B5RoomField field)
        {
            if (field.Phase == B5RoomPhase.Playing && field.ActiveGame != null)
            {
                if (!(field.ActiveGame is B5RoomTowerDefenseGame) && _GameCanvas.Tag != null)
                    _GameCanvas.Tag = null;
                field.ActiveGame.Draw(_GameCanvas);
                _LastDrawnPhase = field.Phase;
                _LastDrawnDevice = field.SelectedDevice;
                _LastDrawnGame = field.SelectedGame;
            }
            else if (field.ShowTitle)
            {
                if (_LastDrawnPhase == field.Phase && _LastDrawnDevice == field.SelectedDevice &&
                    _LastDrawnGame == field.PendingGame) return;
                ClearGameCanvas();
                Color background = field.Device.ScreenColor;
                _GameCanvas.Background = GetFrozenBrush(background);
                AddScreenText(field.Device.Kind == B5RoomDeviceKind.Commodore ? "READY." : field.Device.Kind == B5RoomDeviceKind.Atari ? "INSERT COIN" : "POCKET PLAY", 8, 16, 15, Colors.White);
                AddScreenText(field.Device.Games[field.PendingGame].Title, 8, 45, 13, Color.FromRgb(255, 221, 110));
                if (field.Device.Kind == B5RoomDeviceKind.Atari)
                {
                    B5RoomDraw.Rect(_GameCanvas, 8, 78, 42, 8, Color.FromRgb(232, 78, 60));
                    B5RoomDraw.Rect(_GameCanvas, 58, 78, 42, 8, Color.FromRgb(74, 145, 221));
                    B5RoomDraw.Rect(_GameCanvas, 108, 78, 42, 8, Color.FromRgb(240, 178, 72));
                }
                else AddScreenText("PRESS START", 8, 88, 10, Colors.White);
                _LastDrawnPhase = field.Phase;
                _LastDrawnDevice = field.SelectedDevice;
                _LastDrawnGame = field.PendingGame;
            }
            else
            {
                if (_LastDrawnPhase == field.Phase && _LastDrawnDevice == field.SelectedDevice) return;
                ClearGameCanvas();
                _GameCanvas.Background = GetFrozenBrush(field.Device.ScreenColor);
                _LastDrawnPhase = field.Phase;
                _LastDrawnDevice = field.SelectedDevice;
                _LastDrawnGame = -1;
            }
        }

        // Tower Defense keeps its pixel surface in the canvas Tag, and B5RoomDraw draws into that surface while it is
        // set. Once its image is removed the surface must go too, or the title screen would draw into a surface that
        // is not drawing (crash). Tower Defense creates a new surface when it is shown again.
        void ClearGameCanvas()
        {
            _GameCanvas.Children.Clear();
            _GameCanvas.Tag = null;
        }

        void AddScreenText(string text, double x, double y, int size, Color color)
        {
            B5RoomDraw.Text(_GameCanvas, text, x, y, size, color);
        }

        void UpdateCamera(B5RoomField field, double amount)
        {
            if (field.Phase == B5RoomPhase.Playing) return;

            bool focusScreen = field.Phase == B5RoomPhase.Focusing;
            Point3D target;
            double distance, azimuth, polar;
            if (field.Phase == B5RoomPhase.Room)
            {
                target = new Point3D(0, 1.7, -1);
                distance = 10.5; azimuth = 0.6; polar = 1.2;
            }
            else if (field.Phase == B5RoomPhase.Inserting || field.Phase == B5RoomPhase.Ejecting)
            {
                // Each device has its own close view of the slot while a medium goes in or comes out.
                B5CameraView view = _Media[(int)field.Device.Kind].InsertView;
                target = view.Target;
                distance = view.Distance; azimuth = view.Azimuth; polar = view.Polar;
            }
            else if (field.Device.Kind == B5RoomDeviceKind.Atari)
            {
                target = focusScreen ? B5AtariTelevisionModel.ScreenCenter : new Point3D(-3.6, 1.1, -1.8);
                distance = focusScreen ? 1.9 : 4.6; azimuth = 0.5; polar = 1.3;
            }
            else if (field.Device.Kind == B5RoomDeviceKind.Gameboy)
            {
                target = focusScreen ? B5GameBoyModel.ScreenCenter : B5GameBoyModel.Center;
                distance = focusScreen ? 0.8 : 1.05; azimuth = 0.2; polar = 1.4;
            }
            else
            {
                target = focusScreen ? new Point3D(3.3, 2.05, -1.96) : new Point3D(2.9, 1.95, -2.2);
                distance = focusScreen ? 1.8 : 4.3; azimuth = 0.35; polar = 1.35;
            }

            Point3D desiredPosition = new Point3D(
                target.X + distance * Math.Sin(azimuth) * Math.Sin(polar),
                target.Y + distance * Math.Cos(polar),
                target.Z + distance * Math.Cos(azimuth) * Math.Sin(polar));
            Vector3D desiredLook = target - desiredPosition;
            if (!_CameraInitialized || amount >= 1)
            {
                SetCamera(desiredPosition, desiredLook);
                _Camera.UpDirection = new Vector3D(0, 1, 0);
                _CameraInitialized = true;
                return;
            }

            // Already moving there (or there): the running animation continues on its own.
            if (IsClose(_CameraPosition, desiredPosition) && (_CameraLook - desiredLook).Length <= 0.00001) return;
            AnimateCamera(desiredPosition, desiredLook);
        }

        // The framework only ticks every 40 ms, so moving the camera a step per tick looks jerky.
        // Instead, one WPF animation is started whenever the camera gets a new goal; WPF then
        // moves the camera on every screen frame. _CameraPosition / _CameraLook hold the goal.
        void AnimateCamera(Point3D position, Vector3D look)
        {
            _CameraPosition = position;
            _CameraLook = look;
            _Camera.BeginAnimation(ProjectionCamera.PositionProperty,
                new Point3DAnimation(position, CameraMove) { EasingFunction = CameraEase });
            _Camera.BeginAnimation(ProjectionCamera.LookDirectionProperty,
                new Vector3DAnimation(look, CameraMove) { EasingFunction = CameraEase });
        }

        static IEasingFunction CreateCameraEase()
        {
            ExponentialEase ease = new ExponentialEase { EasingMode = EasingMode.EaseOut, Exponent = 5 };
            ease.Freeze();
            return ease;
        }

        void SetCamera(Point3D position, Vector3D look)
        {
            _CameraPosition = position;
            _CameraLook = look;
            _Camera.BeginAnimation(ProjectionCamera.PositionProperty, null);
            _Camera.BeginAnimation(ProjectionCamera.LookDirectionProperty, null);
            _Camera.Position = position;
            _Camera.LookDirection = look;
        }

        static bool IsClose(Point3D first, Point3D second, double tolerance = 0.00001)
        {
            double dx = first.X - second.X;
            double dy = first.Y - second.Y;
            double dz = first.Z - second.Z;
            return dx * dx + dy * dy + dz * dz <= tolerance * tolerance;
        }
    }

    static class B5RoomTextures
    {
        const int TextureSize = 512;

        // Vector textures (DrawingBrush) on 3D surfaces are rasterized again by WPF in many frames while the
        // camera moves, which made the room stutter (measured: ~45 instead of 90 fps on a 2880x1800 laptop).
        // So every texture is drawn once into a bitmap (longest side TextureSize pixels) and used as an
        // ImageBrush with the same tiling. Returns the frozen bitmap brush.
        public static Brush ToBitmap(DrawingBrush brush, int size = TextureSize)
        {
            Rect bounds = brush.Drawing.Bounds;
            Rect area = brush.ViewboxUnits == BrushMappingMode.Absolute ? brush.Viewbox : new Rect(
                bounds.X + brush.Viewbox.X * bounds.Width, bounds.Y + brush.Viewbox.Y * bounds.Height,
                brush.Viewbox.Width * bounds.Width, brush.Viewbox.Height * bounds.Height);
            if (area.IsEmpty || area.Width <= 0 || area.Height <= 0)
            {
                brush.Freeze();
                return brush;
            }

            double scale = size / Math.Max(area.Width, area.Height);
            int width = Math.Max(16, (int)Math.Round(area.Width * scale));
            int height = Math.Max(16, (int)Math.Round(area.Height * scale));
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext context = visual.RenderOpen())
            {
                context.PushTransform(new ScaleTransform(width / area.Width, height / area.Height));
                context.PushTransform(new TranslateTransform(-area.X, -area.Y));
                context.DrawDrawing(brush.Drawing);
            }
            RenderTargetBitmap bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();

            ImageBrush image = new ImageBrush(bitmap)
            {
                Stretch = brush.Stretch,
                AlignmentX = brush.AlignmentX,
                AlignmentY = brush.AlignmentY,
                TileMode = brush.TileMode,
                Viewport = brush.Viewport,
                ViewportUnits = brush.ViewportUnits,
                Opacity = brush.Opacity,
                Transform = brush.Transform,
                RelativeTransform = brush.RelativeTransform
            };
            image.Freeze();
            return image;
        }
    }
}
