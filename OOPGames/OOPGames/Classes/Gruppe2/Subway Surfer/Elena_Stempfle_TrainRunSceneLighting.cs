using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace OOPGames
{
    /// <summary>
    /// Lighting and shadows of the 3D Train Run scene.
    /// The sun shines from the upper right, so shadows fall to the left.
    /// WPF 3D cannot compute shadows itself; every shadow caster gets a flat,
    /// semi-transparent shadow shape: its bounding box projected along the sun direction.
    /// </summary>
    internal static class Gruppe2_TrainRunSceneLighting
    {
        /// <summary>Direction the sunlight travels: from upper right (slightly behind the camera) to lower left.</summary>
        public static readonly Vector3D SunDirection = new Vector3D(-0.55, -1, -0.25);

        // Shadows lie slightly above the rail tops (0.15), so rails do not cut through them.
        private const double GroundShadowLift = 0.16;
        private const double RoofShadowLift = 0.02;

        private static readonly Material ShadowMaterial = CreateShadowMaterial();

        /// <summary>One light setup for the whole scene (lights in WPF 3D always affect the whole viewport).</summary>
        public static Model3DGroup CreateLights()
        {
            Model3DGroup lights = new Model3DGroup();
            lights.Children.Add(new AmbientLight(Color.FromRgb(92, 98, 112)));
            lights.Children.Add(new DirectionalLight(Color.FromRgb(255, 241, 218), SunDirection));
            // Weak cool fill light from the left, so shaded sides are not pitch black.
            lights.Children.Add(new DirectionalLight(Color.FromRgb(58, 70, 92), new Vector3D(0.6, -0.3, -0.4)));
            lights.Freeze();
            return lights;
        }

        /// <summary>
        /// Builds the shadows of the given visuals. Each visual casts its shadow onto
        /// the ground, or onto the height given by <paramref name="groundHeightOf"/>
        /// (e.g. the train roof the runner stands on).
        /// Must be added to the viewport after all opaque objects (transparency).
        /// </summary>
        public static Model3DGroup CreateShadows(
            IEnumerable<ModelVisual3D> casters, Func<ModelVisual3D, double> groundHeightOf)
        {
            Model3DGroup shadows = new Model3DGroup();
            foreach (ModelVisual3D caster in casters)
            {
                if (caster.Content == null)
                {
                    continue;
                }

                Rect3D bounds = caster.Content.Bounds;
                if (caster.Transform != null)
                {
                    bounds = caster.Transform.TransformBounds(bounds);
                }

                if (bounds.IsEmpty)
                {
                    continue;
                }

                double groundHeight = groundHeightOf(caster);
                GeometryModel3D shadow = CreateShadow(bounds, groundHeight);
                if (shadow != null)
                {
                    shadows.Children.Add(shadow);
                }
            }

            shadows.Freeze();
            return shadows;
        }

        private static GeometryModel3D CreateShadow(Rect3D bounds, double groundHeight)
        {
            double shadowY = groundHeight + (groundHeight > 0 ? RoofShadowLift : GroundShadowLift);
            double stepX = SunDirection.X / -SunDirection.Y;
            double stepZ = SunDirection.Z / -SunDirection.Y;

            List<Point> footprint = new List<Point>();
            foreach (double x in new[] { bounds.X, bounds.X + bounds.SizeX })
            {
                foreach (double y in new[] { bounds.Y, bounds.Y + bounds.SizeY })
                {
                    foreach (double z in new[] { bounds.Z, bounds.Z + bounds.SizeZ })
                    {
                        double heightAboveGround = Math.Max(0, y - groundHeight);
                        footprint.Add(new Point(x + stepX * heightAboveGround, z + stepZ * heightAboveGround));
                    }
                }
            }

            List<Point> hull = ConvexHull(footprint);
            if (hull.Count < 3)
            {
                return null;
            }

            MeshGeometry3D mesh = new MeshGeometry3D();
            foreach (Point point in hull)
            {
                mesh.Positions.Add(new Point3D(point.X, shadowY, point.Y));
            }

            for (int index = 1; index < hull.Count - 1; index++)
            {
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(index);
                mesh.TriangleIndices.Add(index + 1);
            }

            mesh.Freeze();
            GeometryModel3D shadow = new GeometryModel3D(mesh, ShadowMaterial) { BackMaterial = ShadowMaterial };
            shadow.Freeze();
            return shadow;
        }

        /// <summary>Convex hull (Andrew's monotone chain), points in X/Z.</summary>
        private static List<Point> ConvexHull(List<Point> points)
        {
            List<Point> sorted = points.Distinct().OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
            if (sorted.Count < 3)
            {
                return sorted;
            }

            List<Point> hull = new List<Point>();
            for (int pass = 0; pass < 2; pass++)
            {
                int start = hull.Count;
                foreach (Point point in sorted)
                {
                    while (hull.Count >= start + 2 && Cross(hull[hull.Count - 2], hull[hull.Count - 1], point) <= 0)
                    {
                        hull.RemoveAt(hull.Count - 1);
                    }

                    hull.Add(point);
                }

                hull.RemoveAt(hull.Count - 1);
                sorted.Reverse();
            }

            return hull;
        }

        private static double Cross(Point origin, Point a, Point b)
        {
            return (a.X - origin.X) * (b.Y - origin.Y) - (a.Y - origin.Y) * (b.X - origin.X);
        }

        private static Material CreateShadowMaterial()
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(135, 12, 16, 30));
            brush.Freeze();
            DiffuseMaterial material = new DiffuseMaterial(brush);
            material.Freeze();
            return material;
        }
    }
}
