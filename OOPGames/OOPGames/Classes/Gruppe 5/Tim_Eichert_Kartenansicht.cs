using System;
using System.Windows;

namespace OOPGames
{
    internal sealed class G5_Kartenansicht
    {
        private const double KachelGroesse = 32.0;
        private const double MinZoom = 0.25;
        private const double MaxZoom = 4.0;

        private double _zoom = 1.0;
        private double _verschiebungX;
        private double _verschiebungY;

        internal bool TryBildschirmZuRaster(
            int screenX,
            int screenY,
            int breite,
            int hoehe,
            out int x,
            out int y)
        {
            double kachelGroesse = KachelGroesse * _zoom;
            x = (int)Math.Floor((screenX - _verschiebungX) / kachelGroesse);
            y = (int)Math.Floor((screenY - _verschiebungY) / kachelGroesse);

            return x >= 0 && x < breite && y >= 0 && y < hoehe;
        }

        internal bool TryRasterZuBildschirm(int x, int y, int breite, int hoehe, out Point position)
        {
            if (x < 0 || x >= breite || y < 0 || y >= hoehe)
            {
                position = default(Point);
                return false;
            }

            double kachelGroesse = KachelGroesse * _zoom;
            position = new Point(
                _verschiebungX + x * kachelGroesse,
                _verschiebungY + y * kachelGroesse);
            return true;
        }

        internal void Verschieben(double deltaX, double deltaY)
        {
            if (double.IsNaN(deltaX) || double.IsInfinity(deltaX) ||
                double.IsNaN(deltaY) || double.IsInfinity(deltaY))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaX), "Die Verschiebung muss endlich sein.");
            }

            _verschiebungX += deltaX;
            _verschiebungY += deltaY;
        }

        internal void Zoomieren(double faktor, double fokusX, double fokusY)
        {
            if (faktor <= 0 || double.IsNaN(faktor) || double.IsInfinity(faktor))
            {
                throw new ArgumentOutOfRangeException(nameof(faktor));
            }

            if (double.IsNaN(fokusX) || double.IsInfinity(fokusX) ||
                double.IsNaN(fokusY) || double.IsInfinity(fokusY))
            {
                throw new ArgumentOutOfRangeException(nameof(fokusX), "Der Fokus muss endlich sein.");
            }

            double neuerZoom = Math.Max(MinZoom, Math.Min(MaxZoom, _zoom * faktor));
            double zoomFaktor = neuerZoom / _zoom;
            _verschiebungX = fokusX - (fokusX - _verschiebungX) * zoomFaktor;
            _verschiebungY = fokusY - (fokusY - _verschiebungY) * zoomFaktor;
            _zoom = neuerZoom;
        }
    }
}
