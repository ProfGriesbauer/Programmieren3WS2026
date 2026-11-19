using System;
using System.Windows;
using System.Windows.Media;

namespace OOPGames
{
    // Hosts one DrawingVisual on a Canvas. A mini game redraws it every frame through a
    // DrawingContext instead of adding hundreds of Rectangle/TextBlock elements.
    internal sealed class G10_DrawingVisualHost : FrameworkElement
    {
        readonly VisualCollection _Children;
        readonly DrawingVisual _Visual = new DrawingVisual();

        public G10_DrawingVisualHost()
        {
            _Children = new VisualCollection(this) { _Visual };
            SnapsToDevicePixels = true;
        }

        protected override int VisualChildrenCount { get { return _Children.Count; } }

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _Children.Count) throw new ArgumentOutOfRangeException(nameof(index));
            return _Children[index];
        }

        public void Render(Action<DrawingContext> draw)
        {
            using (DrawingContext drawing = _Visual.RenderOpen())
                draw(drawing);
        }
    }
}
