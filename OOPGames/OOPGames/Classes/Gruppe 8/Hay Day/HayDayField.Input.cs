using System;
using System.Linq;
using System.Windows.Input;

namespace OOPGames
{
    // Verarbeitet Klicks und Tasten; ein aktiver Verschiebevorgang hat Vorrang vor normalen Aktionen.
    public sealed partial class HayDayField
    {
        private void ApplyClick(int row, int column, int button)
        {
            if (HasOpenMenu)
            {
                CloseOpenMenu();
            }

            bool rightClick = button == 1;
            bool isMoving = _movingPlot != null || _movingBuilding != null;
            if (isMoving)
            {
                if (rightClick)
                {
                    CancelItemMove();
                    SetStatus("Verschieben abgebrochen.");
                    return;
                }

                if (!IsInside(row, column))
                {
                    SetStatus("Wähle einen freien Platz innerhalb des Hofs.");
                    return;
                }

                MoveSelectedItem(row, column);
                return;
            }

            // Rechtsklick auf ein Feld oder Gebäude startet erst die Auswahl, nicht die Bewegung.
            if (!IsInside(row, column))
            {
                return;
            }

            FarmPlot plot = Plots.FirstOrDefault(value => value.Row == row && value.Column == column);
            FarmBuilding building = Buildings.FirstOrDefault(value => value.Row == row && value.Column == column);
            if (rightClick && plot != null)
            {
                BeginMoving(plot);
                return;
            }

            if (rightClick && building != null)
            {
                BeginMoving(building);
                return;
            }

            if (plot != null)
            {
                if (plot.Crop == null)
                {
                    SelectedPlotRow = plot.Row;
                    SelectedPlotColumn = plot.Column;
                    SetStatus("Saatgut auswählen.");
                    return;
                }

                if (plot.IsRipe(DateTime.UtcNow))
                {
                    Harvest(plot);
                }
                else
                {
                    SetStatus(plot.Crop + " wächst noch - reif in " +
                        plot.RemainingGrowthText(DateTime.UtcNow) + ".");
                }

                return;
            }

            if (building != null)
            {
                SelectedBuildingRow = building.Row;
                SelectedBuildingColumn = building.Column;
                SetStatus(building.Name + " geöffnet.");
                return;
            }

            if (rightClick)
            {
                if (!_pendingBuildingPlacement)
                {
                    SetStatus("Wähle zuerst im Shop ein Gebäude oder einen Acker.");
                    return;
                }

                PlaceBuilding(row, column);
            }
            else
            {
                SetStatus("B wählt ein Gebäude; Rechtsklick auf ein Objekt startet das Verschieben.");
            }
        }

        private void BeginMoving(FarmPlot plot)
        {
            _pendingBuildingPlacement = false;
            _movingPlot = plot;
            _movingBuilding = null;
            SetStatus("Acker ausgewählt. Linksklick auf einen freien Platz; Rechtsklick oder Esc bricht ab.");
        }

        private void BeginMoving(FarmBuilding building)
        {
            _pendingBuildingPlacement = false;
            _movingPlot = null;
            _movingBuilding = building;
            SetStatus(building.Name + " ausgewählt. Linksklick auf einen freien Platz; " +
                "Rechtsklick oder Esc bricht ab.");
        }

        private void MoveSelectedItem(int row, int column)
        {
            // Bei einem ungültigen Ziel bleibt die Auswahl aktiv, damit ein anderes Feld gewählt wird.
            if (!IsAvailableLocation(row, column, _movingPlot, _movingBuilding))
            {
                SetStatus("Dort ist kein freier Platz. Wähle ein freies Feld ohne Straße.");
                return;
            }

            if (_movingPlot != null)
            {
                _movingPlot.MoveTo(row, column);
                _movingPlot = null;
                SetStatus("Acker verschoben.");
            }
            else if (_movingBuilding != null)
            {
                string buildingName = _movingBuilding.Name;
                _movingBuilding.MoveTo(row, column);
                _movingBuilding = null;
                SetStatus(buildingName + " verschoben.");
            }
        }

        private bool CancelItemMove()
        {
            bool wasMoving = _movingPlot != null || _movingBuilding != null;
            _movingPlot = null;
            _movingBuilding = null;
            return wasMoving;
        }

        private void ApplyKey(Key key)
        {
            // Escape beendet ausschließlich die Objektbewegung; andere Tastaturaktionen bleiben unverändert.
            switch (key)
            {
                case Key.Escape:
                    if (!CancelItemMove())
                    {
                        return;
                    }
                    SetStatus("Verschieben abgebrochen.");
                    break;
                case Key.Left:
                    CameraColumn = Math.Max(0, CameraColumn - 2);
                    SetStatus("Kamera bewegt.");
                    break;
                case Key.Right:
                    CameraColumn = Math.Min(Columns - 1, CameraColumn + 2);
                    SetStatus("Kamera bewegt.");
                    break;
                case Key.Up:
                    CameraRow = Math.Max(0, CameraRow - 2);
                    SetStatus("Kamera bewegt.");
                    break;
                case Key.Down:
                    CameraRow = Math.Min(Rows - 1, CameraRow + 2);
                    SetStatus("Kamera bewegt.");
                    break;
                case Key.Add:
                case Key.OemPlus:
                    Zoom = Math.Min(3.0, Zoom + 0.2);
                    SetStatus("Zoom: " + Zoom.ToString("0.0") + "x");
                    break;
                case Key.Subtract:
                case Key.OemMinus:
                    Zoom = Math.Max(0.5, Zoom - 0.2);
                    SetStatus("Zoom: " + Zoom.ToString("0.0") + "x");
                    break;
                case Key.B:
                    SelectedBuildingIndex = (SelectedBuildingIndex + 1) % BuildableKinds.Length;
                    SetStatus("Bauauswahl: " + BuildingLabel(BuildableKinds[SelectedBuildingIndex]) + ".");
                    break;
                case Key.D1:
                case Key.NumPad1:
                    SelectedCropIndex = 0;
                    SetStatus("Ausgewählt: Weizen.");
                    break;
                case Key.D2:
                case Key.NumPad2:
                    SelectedCropIndex = 1;
                    SetStatus("Ausgewählt: Mais.");
                    break;
                case Key.D3:
                case Key.NumPad3:
                    SelectedCropIndex = 2;
                    SetStatus("Ausgewählt: Karotten.");
                    break;
                case Key.D4:
                case Key.NumPad4:
                    SelectedCropIndex = 3;
                    SetStatus("Ausgewählt: Soja.");
                    break;
                case Key.D5:
                case Key.NumPad5:
                    Sell(Eggs, 1);
                    break;
                default:
                    return;
            }

            IsDirty = true;
        }
    }
}
