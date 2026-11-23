using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // Which button outline of UiButtons.png is used: the full-width button or one half of a split row.
    internal enum ArenaUiButtonShape
    {
        Full,
        SplitLong,
        SplitShort
    }

    // Game state shown by the button in addition to hover/selection.
    internal enum ArenaUiButtonState
    {
        Normal,
        Locked,
        MaxedOut
    }

    // Menu button drawn from Assets/Overlay/UiButtons.png. The sprites are drawn as a nine-slice,
    // so the pixel-art corners keep their size no matter how wide or tall the button is.
    internal sealed class ArenaUiButton : Button
    {
        // Layout of UiButtons.png (frames of 256x256, see UiButtons.json).
        private const int FrameSize = 256;
        private const int SourceTop = 95;
        private const int SourceHeight = 33;
        private const int CornerSize = 5;
        private const int FrameNormal = 0;
        private const int FrameSelected = 1;
        private const int FrameLocked = 2;
        private const int FrameMaxedOut = 3;
        private const int FrameSplitLong = 4;
        private const int FrameSplitShort = 5;
        private const int FrameSplitLongSelected = 6;
        private const int FrameSplitShortSelected = 7;
        private static readonly Int32Rect FullBounds = new Int32Rect(47, SourceTop, 194, SourceHeight);
        private static readonly Int32Rect SplitLongBounds = new Int32Rect(47, SourceTop, 132, SourceHeight);
        private static readonly Int32Rect SplitShortBounds = new Int32Rect(183, SourceTop, 58, SourceHeight);
        // The padlock inside the locked frame, relative to the frame origin.
        private static readonly Int32Rect LockBounds = new Int32Rect(217, 104, 14, 16);
        private const int LockRightInset = 10;

        private static readonly Brush LightText = Freeze(new SolidColorBrush(Color.FromRgb(240, 240, 240)));
        private static readonly Brush DarkText = Freeze(new SolidColorBrush(Color.FromRgb(32, 32, 32)));
        private static readonly Brush LockedText = Freeze(new SolidColorBrush(Color.FromRgb(140, 140, 140)));
        private static readonly Brush MaxedText = Freeze(new SolidColorBrush(Color.FromRgb(215, 215, 215)));

        private static readonly Lazy<SpriteSet> Sprites = new Lazy<SpriteSet>(LoadSprites);
        private static ControlTemplate _template;

        private readonly ArenaUiButtonShape _shape;
        private ArenaUiButtonState _state;
        private bool _isChosen;

        public ArenaUiButton(
            string text, Action onClick,
            ArenaUiButtonShape shape = ArenaUiButtonShape.Full)
        {
            _shape = shape;
            Content = text;
            Template = GetTemplate();
            FocusVisualStyle = null;
            SnapsToDevicePixels = true;
            UseLayoutRounding = true;
            RenderOptions.SetBitmapScalingMode(this, BitmapScalingMode.NearestNeighbor);
            ToolTipService.SetShowOnDisabled(this, true);
            Cursor = Cursors.Hand;
            if (onClick != null)
            {
                Click += (sender, args) => onClick();
            }
            IsEnabledChanged += (sender, args) => UpdateAppearance();
            UpdateAppearance();
        }

        // Locked shows a padlock, MaxedOut the grey "fully upgraded" frame.
        public ArenaUiButtonState State
        {
            get { return _state; }
            set
            {
                _state = value;
                UpdateAppearance();
            }
        }

        // Permanently shows the selected frame, e.g. for the equipped weapon.
        public bool IsChosen
        {
            get { return _isChosen; }
            set
            {
                _isChosen = value;
                UpdateAppearance();
            }
        }

        // Places a long and a short split button next to each other in the sprite's proportions.
        public static Grid CreateSplitRow(
            ArenaUiButton longButton, ArenaUiButton shortButton, double width, double height)
        {
            Grid row = new Grid
            {
                Width = width,
                Height = height,
                Margin = new Thickness(0, 4, 0, 4)
            };
            row.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(SplitLongBounds.Width, GridUnitType.Star)
            });
            row.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(
                    SplitShortBounds.X - SplitLongBounds.X - SplitLongBounds.Width,
                    GridUnitType.Star)
            });
            row.ColumnDefinitions.Add(new ColumnDefinition
            {
                Width = new GridLength(SplitShortBounds.Width, GridUnitType.Star)
            });
            longButton.Margin = new Thickness(0);
            shortButton.Margin = new Thickness(0);
            Grid.SetColumn(longButton, 0);
            Grid.SetColumn(shortButton, 2);
            row.Children.Add(longButton);
            row.Children.Add(shortButton);
            return row;
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            UpdateAppearance();
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            UpdateAppearance();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            double width = Math.Round(ActualWidth);
            double height = Math.Round(ActualHeight);
            if (width < 2 * CornerSize || height < 2 * CornerSize)
            {
                return;
            }

            SpriteSet sprites = Sprites.Value;
            double scale = GetPixelScale(height);
            sprites.Frames[GetFrame()].Draw(
                drawingContext, width, height, CornerSize * scale, scale);

            if (IsLocked)
            {
                double lockWidth = LockBounds.Width * scale;
                double lockHeight = LockBounds.Height * scale;
                drawingContext.DrawImage(sprites.Lock, new Rect(
                    width - LockRightInset * scale - lockWidth,
                    Math.Round((height - lockHeight) / 2),
                    lockWidth, lockHeight));
            }
        }

        private bool IsLocked
        {
            get
            {
                return _state == ArenaUiButtonState.Locked
                    || (!IsEnabled && _state == ArenaUiButtonState.Normal);
            }
        }

        private bool IsHighlighted
        {
            get
            {
                return !IsLocked && _state != ArenaUiButtonState.MaxedOut
                    && (_isChosen || (IsEnabled && IsMouseOver));
            }
        }

        private int GetFrame()
        {
            switch (_shape)
            {
                case ArenaUiButtonShape.SplitLong:
                    return IsHighlighted ? FrameSplitLongSelected : FrameSplitLong;
                case ArenaUiButtonShape.SplitShort:
                    return IsHighlighted ? FrameSplitShortSelected : FrameSplitShort;
            }

            if (_state == ArenaUiButtonState.MaxedOut)
            {
                return FrameMaxedOut;
            }
            // The locked frame's padlock would be stretched by the nine-slice, so the normal
            // frame is drawn and the padlock is added unscaled on top.
            return IsHighlighted ? FrameSelected : FrameNormal;
        }

        private void UpdateAppearance()
        {
            Foreground = IsLocked
                ? LockedText
                : _state == ArenaUiButtonState.MaxedOut
                    ? MaxedText
                    : IsHighlighted ? DarkText : LightText;
            Cursor = IsEnabled ? Cursors.Hand : Cursors.Arrow;
            double sidePadding = _shape == ArenaUiButtonShape.SplitShort ? 6 : 12;
            Padding = new Thickness(
                sidePadding, 0,
                IsLocked ? sidePadding + LockRightInset + LockBounds.Width : sidePadding,
                0);
            InvalidateVisual();
        }

        // Whole-pixel scale so the 1px outline of the pixel art stays crisp.
        private static double GetPixelScale(double height)
        {
            return Math.Max(1, Math.Floor(height / SourceHeight));
        }

        private static ControlTemplate GetTemplate()
        {
            if (_template != null)
            {
                return _template;
            }

            // A transparent border makes the whole button clickable; the sprite itself is
            // drawn by OnRender underneath the template.
            FrameworkElementFactory root = new FrameworkElementFactory(typeof(Border));
            root.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            FrameworkElementFactory presenter = new FrameworkElementFactory(typeof(ContentPresenter));
            presenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            presenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            presenter.SetBinding(MarginProperty, new Binding("Padding")
            {
                RelativeSource = RelativeSource.TemplatedParent
            });
            // Labels stay plain strings; long ones are cut off with an ellipsis.
            FrameworkElementFactory label = new FrameworkElementFactory(typeof(TextBlock));
            label.SetBinding(TextBlock.TextProperty, new Binding());
            label.SetValue(TextBlock.TextTrimmingProperty, TextTrimming.CharacterEllipsis);
            label.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Center);
            DataTemplate labelTemplate = new DataTemplate { VisualTree = label };
            labelTemplate.Seal();
            presenter.SetValue(ContentPresenter.ContentTemplateProperty, labelTemplate);
            root.AppendChild(presenter);
            _template = new ControlTemplate(typeof(Button)) { VisualTree = root };
            _template.Seal();
            return _template;
        }

        private static SpriteSet LoadSprites()
        {
            BitmapImage sheet = new BitmapImage();
            sheet.BeginInit();
            sheet.UriSource = new Uri(
                PainterG9.FindAssetPath("Overlay", "UiButtons.png"), UriKind.Absolute);
            sheet.CacheOption = BitmapCacheOption.OnLoad;
            sheet.EndInit();
            sheet.Freeze();

            NineSlice[] frames = new NineSlice[8];
            for (int frame = 0; frame < frames.Length; frame++)
            {
                Int32Rect bounds = frame == FrameSplitLong || frame == FrameSplitLongSelected
                    ? SplitLongBounds
                    : frame == FrameSplitShort || frame == FrameSplitShortSelected
                        ? SplitShortBounds
                        : FullBounds;
                frames[frame] = new NineSlice(sheet, new Int32Rect(
                    frame * FrameSize + bounds.X, bounds.Y, bounds.Width, bounds.Height));
            }

            BitmapSource padlock = Crop(sheet, new Int32Rect(
                FrameLocked * FrameSize + LockBounds.X, LockBounds.Y,
                LockBounds.Width, LockBounds.Height));
            return new SpriteSet(frames, padlock);
        }

        private static BitmapSource Crop(BitmapSource source, Int32Rect rectangle)
        {
            CroppedBitmap crop = new CroppedBitmap(source, rectangle);
            crop.Freeze();
            return crop;
        }

        private static Brush Freeze(Brush brush)
        {
            brush.Freeze();
            return brush;
        }

        private sealed class SpriteSet
        {
            public NineSlice[] Frames { get; }
            public BitmapSource Lock { get; }

            public SpriteSet(NineSlice[] frames, BitmapSource padlock)
            {
                Frames = frames;
                Lock = padlock;
            }
        }

        // Corners are drawn at a fixed size, edges and centre are stretched.
        private sealed class NineSlice
        {
            private readonly BitmapSource[] _pieces = new BitmapSource[9];

            public NineSlice(BitmapSource sheet, Int32Rect bounds)
            {
                int[] xs =
                {
                    bounds.X, bounds.X + CornerSize,
                    bounds.X + bounds.Width - CornerSize, bounds.X + bounds.Width
                };
                int[] ys =
                {
                    bounds.Y, bounds.Y + CornerSize,
                    bounds.Y + bounds.Height - CornerSize, bounds.Y + bounds.Height
                };
                for (int row = 0; row < 3; row++)
                {
                    for (int column = 0; column < 3; column++)
                    {
                        _pieces[row * 3 + column] = Crop(sheet, new Int32Rect(
                            xs[column], ys[row],
                            xs[column + 1] - xs[column], ys[row + 1] - ys[row]));
                    }
                }
            }

            // Pieces that only touch would leave anti-aliased seams, so the centre and the edges
            // overlap their neighbours slightly. The overlapped pixels are identical in the
            // sprite, and the inset of the centre keeps the rounded corners transparent.
            public void Draw(
                DrawingContext context, double width, double height, double corner, double pixel)
            {
                double innerWidth = width - 2 * corner;
                double innerHeight = height - 2 * corner;
                context.DrawImage(_pieces[4], new Rect(
                    2 * pixel, 2 * pixel, width - 4 * pixel, height - 4 * pixel));
                context.DrawImage(_pieces[1], new Rect(
                    corner - pixel, 0, innerWidth + 2 * pixel, corner));
                context.DrawImage(_pieces[7], new Rect(
                    corner - pixel, height - corner, innerWidth + 2 * pixel, corner));
                context.DrawImage(_pieces[3], new Rect(
                    0, corner - pixel, corner, innerHeight + 2 * pixel));
                context.DrawImage(_pieces[5], new Rect(
                    width - corner, corner - pixel, corner, innerHeight + 2 * pixel));
                context.DrawImage(_pieces[0], new Rect(0, 0, corner, corner));
                context.DrawImage(_pieces[2], new Rect(width - corner, 0, corner, corner));
                context.DrawImage(_pieces[6], new Rect(0, height - corner, corner, corner));
                context.DrawImage(_pieces[8], new Rect(
                    width - corner, height - corner, corner, corner));
            }
        }
    }
}
