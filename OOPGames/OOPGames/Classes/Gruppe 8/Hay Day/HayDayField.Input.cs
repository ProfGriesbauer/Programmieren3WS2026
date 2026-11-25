using System;
using System.Linq;
using System.Windows.Input;

namespace OOPGames
{
    public sealed partial class HayDayField
    {
        private void ApplyClick(int row, int column, int button)
        {
            if (HasOpenMenu)
            {
                CloseOpenMenu();
            }

            if (!IsInside(row, column))
            {
                return;
            }

            FarmPlot plot = Plots.FirstOrDefault(value => value.Row == row && value.Column == column);
            FarmBuilding building = Buildings.FirstOrDefault(value => value.Row == row && value.Column == column);
            bool rightClick = button == 1;
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
                if (!rightClick)
                {
                    SelectedBuildingRow = building.Row;
                    SelectedBuildingColumn = building.Column;
                    SetStatus(building.Name + " geöffnet.");
                }
                else
                {
                    InteractWithBuilding(building, true);
                }
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
                SetStatus("B wählt ein Gebäude; Rechtsklick baut es auf einem freien Feld.");
            }
        }

        private void ApplyKey(Key key)
        {
            switch (key)
            {
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

        private void InteractWithBuilding(FarmBuilding building, bool rightClick)
        {
            switch (building.Kind)
            {
                case "Mill":
                    ProduceFeed(0);
                    break;
                case "Coop":
                    if (rightClick)
                    {
                        CollectAnimalProduct(Eggs, "Eier");
                    }
                    else
                    {
                        FeedAnimal("Coop");
                    }
                    break;
                case "Market":
                    if (rightClick)
                    {
                        FulfillOrder();
                    }
                    else
                    {
                        Sell(SelectedCrop, 1);
                    }
                    break;
                case "Van":
                    if (rightClick)
                    {
                        FulfillOrder();
                    }
                    else
                    {
                        SetStatus(Order == null ? "Lieferwagen ist bereit." : Order.Description);
                    }
                    break;
                case "Barn":
                    SetStatus("Scheune: " + Inventory[Eggs] + " Eier, " + Inventory[Feed] +
                        " Futter. Kapazität " + BarnCapacity + ".");
                    break;
                case "CowBarn":
                    if (rightClick)
                        SetStatus("Kühe: " + CowCount + ". Futter: " + Inventory[CowFeed]
                            + ". Bereit: " + MilkReady + " Milch.");
                    else
                        FeedAnimal("CowBarn");
                    break;
                case "PigBarn":
                    if (rightClick)
                        SetStatus("Schweine: " + PigCount + ". Futter: " + Inventory[PigFeed]
                            + ". Bereit: " + PorkReady + " Fleisch.");
                    else
                        FeedAnimal("PigBarn");
                    break;
                case "SheepBarn":
                    if (rightClick)
                        SetStatus("Schafe: " + SheepCount + ". Futter: " + Inventory[SheepFeed]
                            + ". Bereit: " + WoolReady + " Wolle.");
                    else
                        FeedAnimal("SheepBarn");
                    break;
                case "GoatBarn":
                    if (rightClick)
                        SetStatus("Ziegen: " + GoatCount + ". Futter: " + Inventory[GoatFeed]
                            + ". Bereit: " + GoatMilkReady + " Ziegenmilch.");
                    else
                        FeedAnimal("GoatBarn");
                    break;
                case "Silo":
                    SetStatus("Silo: " + Inventory[Wheat] + " Weizen, " + Inventory[Corn] +
                        " Mais, " + Inventory[Carrots] + " Karotten und " + Inventory[Soybeans] +
                        " Soja. Kapazität " + SiloCapacity + ". Tasten 1-4 wählen Saatgut.");
                    break;
                default:
                    SetStatus("Willkommen auf deinem Hof!");
                    break;
            }
        }
    }
}
