using System;

namespace OOPGames
{
    // Entwickler: Denis Mock
    // Berechnet eine auf die Kartenränder begrenzte Kamera in Weltkachel-Koordinaten.
    public sealed class G1C_WorldCamera
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        // Entwickler: Denis Mock
        public void Follow(G1C_Player player, IG1C_WorldScene world, double viewWidthPixels,
                           double viewHeightPixels)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (world == null) throw new ArgumentNullException(nameof(world));
            if (world.TileSize <= 0) throw new ArgumentOutOfRangeException(nameof(world));

            double viewWidth = viewWidthPixels / world.TileSize;
            double viewHeight = viewHeightPixels / world.TileSize;
            X = Clamp(player.VisualX + 0.5 - viewWidth / 2, 0, Math.Max(0, world.Grid.Width - viewWidth));
            Y = Clamp(player.VisualY + 0.5 - viewHeight / 2, 0, Math.Max(0, world.Grid.Height - viewHeight));
        }

        // Entwickler: Denis Mock
        private static double Clamp(double value, double minimum, double maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }
}
