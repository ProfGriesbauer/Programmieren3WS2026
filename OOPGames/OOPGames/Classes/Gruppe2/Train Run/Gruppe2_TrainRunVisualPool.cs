using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    /// <summary>
    /// Keeps one ModelVisual3D per game object (obstacle, coin, runner ...) from frame to frame.
    /// Rebuilding the whole 3D scene every frame forced WPF to send all geometry to the render
    /// thread again and again, which made the game stutter. With the pool a frame only changes
    /// transforms (and contents that really changed); visuals are only added or removed when
    /// objects appear or disappear.
    /// </summary>
    internal sealed class Gruppe2_TrainRunVisualPool
    {
        private readonly Dictionary<object, ModelVisual3D> _visuals = new Dictionary<object, ModelVisual3D>();
        private readonly List<ModelVisual3D> _frame = new List<ModelVisual3D>();

        /// <summary>Visuals shown in the current frame, in drawing order.</summary>
        public IReadOnlyList<ModelVisual3D> FrameVisuals
        {
            get { return _frame; }
        }

        /// <summary>Shows the object identified by <paramref name="key"/> in this frame and returns its visual.</summary>
        public ModelVisual3D Show(object key, Model3D content, Transform3D transform)
        {
            ModelVisual3D visual;
            if (!_visuals.TryGetValue(key, out visual))
            {
                visual = new ModelVisual3D();
                _visuals[key] = visual;
            }

            if (!ReferenceEquals(visual.Content, content))
            {
                visual.Content = content;
            }

            visual.Transform = transform;
            _frame.Add(visual);
            return visual;
        }

        /// <summary>Like <see cref="Show"/>, but placed directly before <paramref name="anchor"/>.</summary>
        public ModelVisual3D ShowBefore(ModelVisual3D anchor, object key, Model3D content, Transform3D transform)
        {
            ModelVisual3D visual = Show(key, content, transform);
            _frame.RemoveAt(_frame.Count - 1);
            _frame.Insert(_frame.IndexOf(anchor), visual);
            return visual;
        }

        /// <summary>
        /// Makes the children of the viewport from <paramref name="firstIndex"/> on equal to the
        /// visuals of this frame, touching only what changed, and starts the next frame.
        /// </summary>
        public void Apply(Visual3DCollection children, int firstIndex)
        {
            HashSet<ModelVisual3D> shown = new HashSet<ModelVisual3D>(_frame);
            foreach (object key in _visuals.Where(entry => !shown.Contains(entry.Value)).Select(entry => entry.Key).ToList())
            {
                _visuals.Remove(key);
            }

            for (int index = children.Count - 1; index >= firstIndex; index--)
            {
                ModelVisual3D visual = children[index] as ModelVisual3D;
                if (visual == null || !shown.Contains(visual))
                {
                    children.RemoveAt(index);
                }
            }

            for (int position = 0; position < _frame.Count; position++)
            {
                int index = firstIndex + position;
                ModelVisual3D visual = _frame[position];
                if (index < children.Count && ReferenceEquals(children[index], visual))
                {
                    continue;
                }

                if (children.Contains(visual))
                {
                    children.Remove(visual);
                }

                children.Insert(index, visual);
            }

            _frame.Clear();
        }
    }
}
