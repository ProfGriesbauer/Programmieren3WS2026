using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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
        Border _Cartridge;
        Canvas _AnimationLayer;
        Border _Sidebar;
        StackPanel _Cards;
        TextBlock _DeviceTitle;
        TextBlock _Hint;
        TextBlock _RoomSelection;
        readonly List<GeometryModel3D> _Highlights = new List<GeometryModel3D>();
        readonly List<GeometryModel3D> _Screens = new List<GeometryModel3D>();
        readonly List<Border> _CardBorders = new List<Border>();
        readonly List<TextBlock> _CardTitles = new List<TextBlock>();
        readonly Model3DGroup[] _DeviceControls = { new Model3DGroup(), new Model3DGroup(), new Model3DGroup() };
        B5RoomField _LastField;
        Model3DGroup _VisibleControls;
        int _LastDevice = -1;
        int _LastSelected = -1;
        double _LastWidth = -1;

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
                _Root = new Grid { ClipToBounds = true, Background = new SolidColorBrush(Color.FromRgb(16, 22, 32)) };
                _Viewport = new Viewport3D();
                _Camera = new PerspectiveCamera { FieldOfView = 70, NearPlaneDistance = 0.05, FarPlaneDistance = 100 };
                _Viewport.Camera = _Camera;
                _Scene = new Model3DGroup();
                _Scene.Children.Add(new AmbientLight(Color.FromRgb(205, 190, 169)));
                _Scene.Children.Add(new DirectionalLight(Color.FromRgb(255, 235, 207), new Vector3D(-1, -2, 1)));
                _Viewport.Children.Add(new ModelVisual3D { Content = _Scene });
                _Root.Children.Add(_Viewport);
                _AnimationLayer = new Canvas { IsHitTestVisible = false };
                _AnimationLayer.SetValue(Panel.ZIndexProperty, 2);
                _Root.Children.Add(_AnimationLayer);
                BuildInterface();
                BuildRoom();
                AddDisplayQuad(-3.6, 1.62, -1.268, 1.1, 0.82);
                Point3D gameBoyScreen = B5GameBoyModel.ScreenCenter;
                AddDisplayQuad(gameBoyScreen.X, gameBoyScreen.Y, gameBoyScreen.Z, B5GameBoyModel.ScreenWidth, B5GameBoyModel.ScreenHeight);
                AddDisplayQuad(3.3, 2.03, -1.945, 0.6, 0.46);
                canvas.Children.Clear();
                canvas.Children.Add(_Root);
                _LastField = field;
                _LastDevice = -1;
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

        void BuildInterface()
        {
            _GameCanvas = new Canvas { Width = 160, Height = 144, Background = Brushes.Black, ClipToBounds = true };
            _GameFrame = new Border
            {
                Width = 160,
                Height = 144,
                BorderThickness = new Thickness(8),
                BorderBrush = new SolidColorBrush(Color.FromRgb(198, 185, 137)),
                Background = new SolidColorBrush(Color.FromRgb(20, 24, 26)),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                Child = new Viewbox { Stretch = Stretch.Uniform, Child = _GameCanvas },
                Visibility = Visibility.Collapsed
            };
            _GameFrame.SetValue(Panel.ZIndexProperty, 2);
            _Root.Children.Add(_GameFrame);

            _Cartridge = new Border
            {
                Width = 82,
                Height = 40,
                BorderThickness = new Thickness(3),
                BorderBrush = new SolidColorBrush(Color.FromRgb(237, 211, 140)),
                CornerRadius = new CornerRadius(4),
                Child = new TextBlock
                {
                    Foreground = Brushes.White,
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center
                },
                Visibility = Visibility.Collapsed
            };
            _AnimationLayer.Children.Add(_Cartridge);

            _Sidebar = new Border
            {
                Width = 250,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Color.FromArgb(238, 19, 24, 35)),
                Padding = new Thickness(18),
                Visibility = Visibility.Collapsed
            };
            StackPanel panel = new StackPanel();
            _DeviceTitle = new TextBlock { Foreground = Brushes.White, FontSize = 19, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 14) };
            panel.Children.Add(new TextBlock { Text = "SPIELE", Foreground = new SolidColorBrush(Color.FromRgb(239, 191, 83)), FontSize = 14, FontWeight = FontWeights.Bold });
            panel.Children.Add(_DeviceTitle);
            _Cards = new StackPanel();
            panel.Children.Add(_Cards);
            _Sidebar.Child = panel;
            _Sidebar.SetValue(Panel.ZIndexProperty, 3);
            _Root.Children.Add(_Sidebar);

            _Hint = new TextBlock
            {
                Foreground = Brushes.White,
                Background = new SolidColorBrush(Color.FromArgb(205, 18, 22, 32)),
                FontSize = 14,
                Padding = new Thickness(12, 7, 12, 7),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Bottom,
                Margin = new Thickness(8),
                TextAlignment = TextAlignment.Center
            };
            _Hint.SetValue(Panel.ZIndexProperty, 4);
            _Root.Children.Add(_Hint);

            _RoomSelection = new TextBlock
            {
                Foreground = new SolidColorBrush(Color.FromRgb(255, 220, 125)),
                Background = new SolidColorBrush(Color.FromArgb(190, 17, 22, 31)),
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                Padding = new Thickness(16, 8, 16, 8),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = new Thickness(0, 18, 0, 0),
                TextAlignment = TextAlignment.Center
            };
            _RoomSelection.SetValue(Panel.ZIndexProperty, 2);
            _Root.Children.Add(_RoomSelection);
        }

        void BuildRoom()
        {
            AddBox(0, -0.05, 0, 10, 0.1, 6, Color.FromRgb(72, 47, 35));
            AddBox(0, 2, -3, 10, 4, 0.1, Color.FromRgb(221, 202, 174));
            AddBox(-5, 2, 0, 0.1, 4, 6, Color.FromRgb(196, 169, 139));

            for (int row = 0; row < 8; row++)
            {
                double z = -2.625 + row * 0.75;
                double x = -5;
                int plank = 0;
                if (row % 2 != 0)
                {
                    AddBox(x + 0.6125, 0.01, z, 1.2, 0.018, 0.72, Color.FromRgb(103, 65, 44));
                    x += 1.225;
                    plank++;
                }

                while (x < 5)
                {
                    double width = Math.Min(2.5, 5 - x);
                    Color wood = (row + plank) % 3 == 0 ? Color.FromRgb(119, 76, 51) :
                        (row + plank) % 3 == 1 ? Color.FromRgb(99, 61, 43) : Color.FromRgb(110, 68, 46);
                    AddBox(x + width / 2, 0.01, z, Math.Max(0.1, width - 0.025), 0.018, 0.72, wood);
                    x += width;
                    plank++;
                }
            }

            AddBox(0, 0.015, 0.8, 3.2, 0.02, 3.2, Color.FromRgb(31, 45, 63));
            AddBox(0, 0.029, -0.62, 2.9, 0.006, 0.1, Color.FromRgb(221, 133, 63));
            AddBox(0, 0.029, 2.22, 2.9, 0.006, 0.1, Color.FromRgb(221, 133, 63));
            AddBox(-1.46, 0.029, 0.8, 0.1, 0.006, 2.7, Color.FromRgb(221, 133, 63));
            AddBox(1.46, 0.029, 0.8, 0.1, 0.006, 2.7, Color.FromRgb(221, 133, 63));
            AddBox(0, 0.029, -0.05, 2.2, 0.006, 0.12, Color.FromRgb(196, 76, 55));
            AddBox(0, 0.029, 0.8, 2.2, 0.006, 0.12, Color.FromRgb(225, 199, 157));
            AddBox(0, 0.029, 1.65, 2.2, 0.006, 0.12, Color.FromRgb(196, 76, 55));
            AddBox(-0.72, 0.029, 0.38, 0.14, 0.006, 0.62, Color.FromRgb(225, 199, 157));
            AddBox(0.72, 0.029, 1.22, 0.14, 0.006, 0.62, Color.FromRgb(225, 199, 157));

            AddBox(1.8, 1.55, -2.35, 5, 0.1, 1.3, Color.FromRgb(185, 119, 62));
            AddBox(-0.2, 0.75, -2.35, 0.9, 1.5, 1.2, Color.FromRgb(168, 103, 47));
            AddBox(4.2, 0.75, -2.35, 0.15, 1.5, 1.2, Color.FromRgb(168, 103, 47));
            AddBox(1.8, 0.96, -0.9, 0.9, 0.12, 0.9, Color.FromRgb(139, 134, 128));
            AddBox(1.8, 1.5, -0.4, 0.9, 0.85, 0.1, Color.FromRgb(139, 134, 128));
            AddBox(1.8, 0.52, -0.9, 0.12, 0.8, 0.12, Color.FromRgb(90, 91, 94));
            AddBox(1.8, 0.12, -0.9, 0.75, 0.08, 0.75, Color.FromRgb(90, 91, 94));

            AddBox(-3.6, 1.0, -1.8, 2, 0.08, 1.1, Color.FromRgb(145, 91, 49));
            AddBox(-3.6, 0.45, -1.8, 1.8, 0.07, 0.95, Color.FromRgb(145, 91, 49));
            AddBox(-4.45, 0.7, -2.25, 0.08, 0.6, 0.08, Color.FromRgb(100, 67, 45));
            AddBox(-2.75, 0.7, -2.25, 0.08, 0.6, 0.08, Color.FromRgb(100, 67, 45));
            AddBox(-4.45, 0.7, -1.35, 0.08, 0.6, 0.08, Color.FromRgb(100, 67, 45));
            AddBox(-2.75, 0.7, -1.35, 0.08, 0.6, 0.08, Color.FromRgb(100, 67, 45));
            AddBox(-3.6, 1.59, -1.8, 1.5, 1.1, 1, Color.FromRgb(45, 45, 51));
            AddBox(-3.6, 1.62, -1.285, 1.1, 0.82, 0.025, Color.FromRgb(13, 26, 36));
            AddBox(-3.6, 0.56, -1.7, 1.1, 0.16, 0.7, Color.FromRgb(43, 43, 43));
            AddBox(-3.6, 0.65, -1.9, 0.38, 0.04, 0.22, Color.FromRgb(154, 106, 60));
            AddBox(-2.4, 0.15, -0.8, 0.12, 0.3, 0.12, Color.FromRgb(35, 35, 39));

            B5GameBoyModel.Build(_Scene);

            AddBox(3.3, 2.03, -2.4, 0.95, 0.85, 0.85, Color.FromRgb(235, 232, 224));
            AddBox(3.3, 2.03, -1.96, 0.61, 0.47, 0.02, Color.FromRgb(26, 26, 31));
            AddBox(3.3, 1.62, -2.4, 0.6, 0.2, 0.5, Color.FromRgb(198, 195, 188));
            AddBox(2.1, 1.67, -2.0, 1.25, 0.14, 0.6, Color.FromRgb(217, 210, 187));
            Model3DGroup commodoreControls = _DeviceControls[(int)B5RoomDeviceKind.Commodore];
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 10; col++)
                {
                    if (row == 3 && col >= 4 && col <= 6) continue;
                    AddBox(commodoreControls, 1.65 + col * 0.082, 1.755, -2.21 + row * 0.115, 0.055, 0.018, 0.06, Color.FromRgb(64, 57, 48));
                }
            }
            AddBox(commodoreControls, 2.06, 1.755, -1.865, 0.27, 0.018, 0.06, Color.FromRgb(64, 57, 48));
            AddBox(commodoreControls, 2.58, 1.755, -2.095, 0.055, 0.018, 0.06, Color.FromRgb(64, 57, 48));
            AddBox(commodoreControls, 2.52, 1.755, -1.98, 0.055, 0.018, 0.06, Color.FromRgb(64, 57, 48));
            AddBox(commodoreControls, 2.64, 1.755, -1.98, 0.055, 0.018, 0.06, Color.FromRgb(64, 57, 48));
            AddBox(commodoreControls, 2.58, 1.755, -1.865, 0.055, 0.018, 0.06, Color.FromRgb(64, 57, 48));

            Model3DGroup atariControls = _DeviceControls[(int)B5RoomDeviceKind.Atari];
            AddBox(atariControls, -3.6, 0.68, -1.15, 0.48, 0.08, 0.34, Color.FromRgb(32, 31, 31));
            AddBox(atariControls, -3.6, 0.85, -1.15, 0.075, 0.28, 0.075, Color.FromRgb(19, 19, 20));
            AddBox(atariControls, -3.6, 1.005, -1.15, 0.16, 0.09, 0.16, Color.FromRgb(12, 12, 13));
            AddBox(atariControls, -3.32, 0.745, -1.15, 0.09, 0.04, 0.09, Color.FromRgb(201, 49, 39));

            AddBox(-1.5, 2.7, -2.91, 1.3, 0.8, 0.04, Color.FromRgb(38, 57, 78));
            AddBox(-1.5, 2.48, -2.884, 1.04, 0.07, 0.012, Color.FromRgb(221, 117, 67));
            AddBox(-1.5, 2.57, -2.884, 1.04, 0.07, 0.012, Color.FromRgb(196, 76, 55));
            AddBox(-1.5, 2.66, -2.884, 1.04, 0.07, 0.012, Color.FromRgb(225, 182, 111));
            AddBox(-1.5, 2.91, -2.884, 0.18, 0.18, 0.012, Color.FromRgb(239, 191, 95));
            AddBox(-1.86, 2.76, -2.884, 0.28, 0.1, 0.012, Color.FromRgb(72, 91, 105));
            AddBox(-1.53, 2.78, -2.884, 0.34, 0.12, 0.012, Color.FromRgb(53, 75, 90));
            AddBox(-1.18, 2.76, -2.884, 0.3, 0.1, 0.012, Color.FromRgb(72, 91, 105));

            AddBox(1.1, 2.6, -2.9, 1.1, 0.7, 0.04, Color.FromRgb(222, 194, 151));
            AddBox(0.72, 2.6, -2.872, 0.1, 0.5, 0.012, Color.FromRgb(38, 57, 78));
            AddBox(1.18, 2.42, -2.872, 0.6, 0.08, 0.012, Color.FromRgb(196, 76, 55));
            AddBox(0.98, 2.68, -2.872, 0.18, 0.18, 0.012, Color.FromRgb(221, 133, 63));
            AddBox(1.42, 2.77, -2.872, 0.18, 0.18, 0.012, Color.FromRgb(38, 57, 78));
            AddBox(0.6, 2.3, -2.75, 3.2, 0.08, 0.35, Color.FromRgb(107, 74, 54));
            AddBox(0.6, 3.0, -2.75, 3.2, 0.08, 0.35, Color.FromRgb(107, 74, 54));
            BuildBooks();

            AddBox(-4.35, 0.13, 1.55, 0.42, 0.24, 0.42, Color.FromRgb(174, 91, 59));
            AddBox(-4.35, 0.31, 1.55, 0.34, 0.08, 0.34, Color.FromRgb(103, 65, 44));
            AddBox(-4.35, 0.52, 1.55, 0.07, 0.42, 0.07, Color.FromRgb(68, 104, 57));
            AddBox(-4.52, 0.58, 1.55, 0.3, 0.1, 0.12, Color.FromRgb(76, 122, 64));
            AddBox(-4.18, 0.68, 1.55, 0.12, 0.1, 0.3, Color.FromRgb(89, 136, 70));

            _Highlights.Clear();
            _Highlights.Add(AddBox(-3.6, 0.14, -1.8, 1.4, 0.025, 0.9, Color.FromArgb(0, 255, 213, 92)));
            _Highlights.Add(AddBox(0.8, 1.625, -2.5, 0.55, 0.02, 0.34, Color.FromArgb(0, 255, 213, 92)));
            _Highlights.Add(AddBox(2.8, 1.62, -2.1, 1.5, 0.02, 0.85, Color.FromArgb(0, 255, 213, 92)));
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
            double[] widths = { 0.05, 0.07, 0.045, 0.06, 0.08, 0.05, 0.065, 0.045, 0.07, 0.055, 0.06, 0.05 };
            double[] heights = { 0.30, 0.34, 0.27, 0.32, 0.36, 0.29, 0.33, 0.26, 0.35, 0.30, 0.31, 0.28 };
            double[] depths = { 0.22, 0.24, 0.2, 0.23, 0.25, 0.21, 0.24, 0.2, 0.24, 0.22, 0.23, 0.21 };

            double x = -0.85;
            for (int i = 0; i < widths.Length; i++)
            {
                AddBox(x + widths[i] / 2, shelfTop + heights[i] / 2, backZ + depths[i] / 2, widths[i], heights[i], depths[i], colors[i % colors.Length]);
                x += widths[i] + 0.004;
            }

            // A short stack lying flat at the right end of the shelf.
            double stackY = shelfTop;
            double[] stackHeights = { 0.05, 0.04, 0.06 };
            double[] stackWidths = { 0.30, 0.26, 0.28 };
            for (int i = 0; i < stackHeights.Length; i++)
            {
                AddBox(1.7 + (i % 2) * 0.015, stackY + stackHeights[i] / 2, backZ + 0.12, stackWidths[i], stackHeights[i], 0.22, colors[(i * 2 + 1) % colors.Length]);
                stackY += stackHeights[i];
            }
        }

        GeometryModel3D AddBox(double x, double y, double z, double width, double height, double depth, Color color)
        {
            return AddBox(_Scene, x, y, z, width, height, depth, color);
        }

        GeometryModel3D AddBox(Model3DGroup parent, double x, double y, double z, double width, double height, double depth, Color color)
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
            MeshGeometry3D mesh = new MeshGeometry3D();
            for (int face = 0; face < faces.Length; face++)
            {
                int start = mesh.Positions.Count;
                foreach (Point3D point in faces[face])
                {
                    mesh.Positions.Add(point);
                    mesh.Normals.Add(normals[face]);
                }
                mesh.TriangleIndices.Add(start); mesh.TriangleIndices.Add(start + 1); mesh.TriangleIndices.Add(start + 2);
                mesh.TriangleIndices.Add(start); mesh.TriangleIndices.Add(start + 2); mesh.TriangleIndices.Add(start + 3);
            }
            DiffuseMaterial material = new DiffuseMaterial(new SolidColorBrush(color));
            GeometryModel3D model = new GeometryModel3D(mesh, material) { BackMaterial = material };
            parent.Children.Add(model);
            return model;
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
            GeometryModel3D model = new GeometryModel3D(mesh, null);
            _Scene.Children.Add(model);
            _Screens.Add(model);
        }

        void UpdateInterface(B5RoomField field)
        {
            bool deviceView = field.Phase != B5RoomPhase.Room;
            UpdateDeviceControls(field);
            _Sidebar.Visibility = deviceView ? Visibility.Visible : Visibility.Collapsed;
            _RoomSelection.Visibility = field.Phase == B5RoomPhase.Room ? Visibility.Visible : Visibility.Collapsed;
            _GameFrame.Visibility = field.Phase == B5RoomPhase.Playing || field.ShowTitle ? Visibility.Visible : Visibility.Collapsed;
            SizeGameFrame(field);
            _GameCanvas.Width = field.Device.DisplayWidth;
            _GameCanvas.Height = field.Device.DisplayHeight;
            _RoomSelection.Text = "Geraet: " + field.Device.Name + "    <  >   |   ENTER";
            _Hint.Text =             field.Phase == B5RoomPhase.Room ? "A / D: Geraet waehlen     ENTER / Z: oeffnen" :
                field.Phase == B5RoomPhase.Device ? "1 / 2: Spiel waehlen     ENTER / Z: Modul     ESC / X: zurueck" :
                field.Phase == B5RoomPhase.Playing ? "WASD: steuern     ESC / X: zurueck zur Geraeteauswahl" :
                "Modul wird " + (field.Phase == B5RoomPhase.Ejecting ? "ausgeworfen" : "eingesteckt") + " ...     ESC: abbrechen";
            UpdateCartridgeAnimation(field);
            UpdateScreenLighting(field);

            if (_LastDevice != field.SelectedDevice || _LastSelected != field.SelectedGame)
            {
                _LastDevice = field.SelectedDevice;
                _LastSelected = field.SelectedGame;
                B5RoomDevice device = field.Device;
                _DeviceTitle.Text = device.Name;
                _Cards.Children.Clear();
                _CardBorders.Clear();
                _CardTitles.Clear();
                foreach (B5RoomGameEntry game in device.Games)
                {
                    TextBlock title = new TextBlock
                    {
                        Text = game.Title,
                        Foreground = Brushes.White,
                        FontSize = 15,
                        TextWrapping = TextWrapping.Wrap
                    };
                    Border card = new Border
                    {
                        Margin = new Thickness(0, 4, 0, 5),
                        Padding = new Thickness(10),
                        BorderThickness = new Thickness(2),
                        BorderBrush = Brushes.Transparent,
                        Background = new SolidColorBrush(Color.FromRgb(35, 40, 54)),
                        Child = new StackPanel
                        {
                            Children =
                            {
                                new Border { Height = 7, Background = new SolidColorBrush(game.CartridgeColor), Margin = new Thickness(0,0,0,7) },
                                title
                            }
                        }
                    };
                    _Cards.Children.Add(card);
                    _CardBorders.Add(card);
                    _CardTitles.Add(title);
                }
            }

            for (int i = 0; i < _CardBorders.Count; i++)
            {
                bool selected = i == field.SelectedGame;
                _CardBorders[i].BorderBrush = selected ? new SolidColorBrush(Color.FromRgb(255, 213, 92)) : Brushes.Transparent;
                if (field.Phase == B5RoomPhase.Inserting && i == field.PendingGame)
                {
                    _CardBorders[i].Background = Brushes.Transparent;
                    _CardBorders[i].BorderBrush = new SolidColorBrush(Color.FromRgb(170, 170, 180));
                    _CardTitles[i].Text = "  . . .";
                }
                else
                {
                    _CardBorders[i].Background = new SolidColorBrush(Color.FromRgb(35, 40, 54));
                    _CardTitles[i].Text = B5RoomRegistry.Devices[field.SelectedDevice].Games[i].Title +
                        (i == field.InsertedGame ? "  [EINGELEGT]" : "");
                }
            }

            for (int i = 0; i < _Highlights.Count; i++)
            {
                Color color = i == field.SelectedDevice
                    ? Color.FromArgb(145, 255, 213, 92) : Color.FromArgb(0, 255, 213, 92);
                MaterialGroup material = new MaterialGroup();
                material.Children.Add(new DiffuseMaterial(new SolidColorBrush(color)));
                if (i == field.SelectedDevice)
                    material.Children.Add(new EmissiveMaterial(new SolidColorBrush(Color.FromArgb(80, 255, 185, 55))));
                _Highlights[i].Material = material;
                _Highlights[i].BackMaterial = material;
            }
        }

        void UpdateDeviceControls(B5RoomField field)
        {
            Model3DGroup controls = field.Phase == B5RoomPhase.Playing
                ? _DeviceControls[(int)field.Device.Kind]
                : null;
            if (_VisibleControls == controls) return;

            if (_VisibleControls != null) _Scene.Children.Remove(_VisibleControls);
            _VisibleControls = controls;
            if (_VisibleControls != null) _Scene.Children.Add(_VisibleControls);
        }

        void SizeGameFrame(B5RoomField field)
        {
            double rootWidth = Math.Max(1, _Root.ActualWidth);
            double rootHeight = Math.Max(1, _Root.ActualHeight);
            bool sidebarVisible = _Sidebar.Visibility == Visibility.Visible;
            double usableWidth = Math.Max(1, rootWidth - (sidebarVisible ? _Sidebar.Width : 0) - 32);
            double usableHeight = Math.Max(1, rootHeight - 80);
            double border = _GameFrame.BorderThickness.Left + _GameFrame.BorderThickness.Right;
            double verticalBorder = _GameFrame.BorderThickness.Top + _GameFrame.BorderThickness.Bottom;
            double scale = Math.Min(
                (usableWidth - border) / field.Device.DisplayWidth,
                (usableHeight - verticalBorder) / field.Device.DisplayHeight);
            scale = Math.Max(0.1, scale);
            _GameFrame.Width = field.Device.DisplayWidth * scale + border;
            _GameFrame.Height = field.Device.DisplayHeight * scale + verticalBorder;
            _GameFrame.Margin = new Thickness(Math.Max(0, (usableWidth - _GameFrame.Width) / 2), 0, 0, 0);
        }

        void UpdateScreenLighting(B5RoomField field)
        {
            for (int i = 0; i < _Screens.Count; i++)
            {
                B5RoomDeviceKind kind = (B5RoomDeviceKind)i;
                bool selected = i == field.SelectedDevice;
                Color dark = kind == B5RoomDeviceKind.Gameboy ? Color.FromRgb(32, 46, 13) :
                    kind == B5RoomDeviceKind.Commodore ? Color.FromRgb(17, 14, 43) : Color.FromRgb(7, 8, 10);
                Color lit = kind == B5RoomDeviceKind.Gameboy ? Color.FromRgb(107, 138, 18) :
                    kind == B5RoomDeviceKind.Commodore ? Color.FromRgb(53, 40, 121) : Color.FromRgb(151, 77, 35);
                MaterialGroup material = new MaterialGroup();
                material.Children.Add(new DiffuseMaterial(new SolidColorBrush(selected ? lit : dark)));
                if (selected) material.Children.Add(new EmissiveMaterial(new SolidColorBrush(lit)));
                _Screens[i].Material = material;
                _Screens[i].BackMaterial = material;
            }
        }

        void UpdateCartridgeAnimation(B5RoomField field)
        {
            bool animating = field.Phase == B5RoomPhase.Inserting || field.Phase == B5RoomPhase.Ejecting;
            _Cartridge.Visibility = animating ? Visibility.Visible : Visibility.Collapsed;
            if (!animating) return;

            int gameIndex = field.Phase == B5RoomPhase.Ejecting ? field.InsertedGame : field.PendingGame;
            if (gameIndex < 0 || gameIndex >= field.Device.Games.Count) return;
            B5RoomGameEntry game = field.Device.Games[gameIndex];
            _Cartridge.Background = new SolidColorBrush(game.CartridgeColor);
            ((TextBlock)_Cartridge.Child).Text = game.Title;

            double width = Math.Max(1, _Root.ActualWidth);
            double height = Math.Max(1, _Root.ActualHeight);
            double deviceX = width * (field.Device.Kind == B5RoomDeviceKind.Atari ? 0.24 :
                field.Device.Kind == B5RoomDeviceKind.Gameboy ? 0.43 : 0.62);
            double deviceY = height * 0.57;
            double progress = field.TransitionProgress;
            double x;
            double y;
            if (field.Phase == B5RoomPhase.Inserting)
            {
                if (progress < 0.55)
                {
                    double t = progress / 0.55;
                    double ease = t * t * (3 - 2 * t);
                    x = width + 85 + (deviceX - width - 85) * ease;
                    y = height * 0.48 + (deviceY - 38 - height * 0.48) * ease - Math.Sin(Math.PI * t) * 55;
                }
                else
                {
                    double t = (progress - 0.55) / 0.45;
                    double ease = 1 - (1 - t) * (1 - t);
                    x = deviceX;
                    y = deviceY - 38 + 38 * ease;
                    _Cartridge.Opacity = 1 - ease * 0.7;
                }
            }
            else
            {
                double ease = progress * progress * (3 - 2 * progress);
                x = deviceX + (width + 85 - deviceX) * ease;
                y = deviceY + (height * 0.48 - deviceY) * ease - Math.Sin(Math.PI * progress) * 45;
                _Cartridge.Opacity = 1 - progress * 0.45;
            }
            if (field.Phase == B5RoomPhase.Inserting && progress < 0.55) _Cartridge.Opacity = 1;
            Canvas.SetLeft(_Cartridge, x);
            Canvas.SetTop(_Cartridge, y);
        }

        void DrawGame(B5RoomField field)
        {
            if (field.Phase == B5RoomPhase.Playing && field.ActiveGame != null)
                field.ActiveGame.Draw(_GameCanvas);
            else if (field.ShowTitle)
            {
                _GameCanvas.Children.Clear();
                Color background = field.Device.ScreenColor;
                _GameCanvas.Background = new SolidColorBrush(background);
                AddScreenText(field.Device.Kind == B5RoomDeviceKind.Commodore ? "READY." : field.Device.Kind == B5RoomDeviceKind.Atari ? "INSERT COIN" : "POCKET PLAY", 8, 16, 15, Colors.White);
                AddScreenText(field.Device.Games[field.PendingGame].Title, 8, 45, 13, Color.FromRgb(255, 221, 110));
                if (field.Device.Kind == B5RoomDeviceKind.Atari)
                {
                    B5RoomDraw.Rect(_GameCanvas, 8, 78, 42, 8, Color.FromRgb(232, 78, 60));
                    B5RoomDraw.Rect(_GameCanvas, 58, 78, 42, 8, Color.FromRgb(74, 145, 221));
                    B5RoomDraw.Rect(_GameCanvas, 108, 78, 42, 8, Color.FromRgb(240, 178, 72));
                }
                else AddScreenText("PRESS START", 8, 88, 10, Colors.White);
            }
            else
            {
                _GameCanvas.Children.Clear();
                _GameCanvas.Background = new SolidColorBrush(field.Device.ScreenColor);
            }
        }

        void AddScreenText(string text, double x, double y, int size, Color color)
        {
            B5RoomDraw.Text(_GameCanvas, text, x, y, size, color);
        }

        void UpdateCamera(B5RoomField field, double amount)
        {
            Point3D target;
            double distance, azimuth, polar;
            if (field.Phase == B5RoomPhase.Room)
            {
                target = new Point3D(0, 1.7, -1);
                distance = 10.5; azimuth = 0.6; polar = 1.2;
            }
            else if (field.Device.Kind == B5RoomDeviceKind.Atari)
            {
                target = field.Phase == B5RoomPhase.Playing ? new Point3D(-3.6, 1.62, -1.3) : new Point3D(-3.6, 1.1, -1.8);
                distance = field.Phase == B5RoomPhase.Playing ? 2.5 : 4.6; azimuth = 0.5; polar = 1.3;
            }
            else if (field.Device.Kind == B5RoomDeviceKind.Gameboy)
            {
                target = field.Phase == B5RoomPhase.Playing ? B5GameBoyModel.ScreenCenter : B5GameBoyModel.Center;
                distance = field.Phase == B5RoomPhase.Playing ? 1.3 : 1.05; azimuth = 0.2; polar = 1.4;
            }
            else
            {
                target = field.Phase == B5RoomPhase.Playing ? new Point3D(3.3, 2.05, -1.96) : new Point3D(2.9, 1.95, -2.2);
                distance = field.Phase == B5RoomPhase.Playing ? 2.4 : 4.3; azimuth = 0.35; polar = 1.35;
            }

            Point3D desiredPosition = new Point3D(
                target.X + distance * Math.Sin(azimuth) * Math.Sin(polar),
                target.Y + distance * Math.Cos(polar),
                target.Z + distance * Math.Cos(azimuth) * Math.Sin(polar));
            Point3D current = _Camera.Position;
            Vector3D look = _Camera.LookDirection;
            if (look.Length < 0.01)
            {
                _Camera.Position = desiredPosition;
                _Camera.LookDirection = target - desiredPosition;
                _Camera.UpDirection = new Vector3D(0, 1, 0);
                return;
            }

            Point3D currentTarget = current + look;
            Point3D nextPosition = new Point3D(
                current.X + (desiredPosition.X - current.X) * amount,
                current.Y + (desiredPosition.Y - current.Y) * amount,
                current.Z + (desiredPosition.Z - current.Z) * amount);
            Point3D desiredTarget = target;
            Point3D nextTarget = new Point3D(
                currentTarget.X + (desiredTarget.X - currentTarget.X) * amount,
                currentTarget.Y + (desiredTarget.Y - currentTarget.Y) * amount,
                currentTarget.Z + (desiredTarget.Z - currentTarget.Z) * amount);
            _Camera.Position = nextPosition;
            _Camera.LookDirection = nextTarget - nextPosition;
            _Camera.UpDirection = new Vector3D(0, 1, 0);
        }
    }
}
