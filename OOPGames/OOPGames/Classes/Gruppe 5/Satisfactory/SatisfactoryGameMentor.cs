using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    internal sealed class SatisfactoryGameMentor
    {
        private enum TutorialStep
        {
            Inactive,
            SelectMinerTool,
            PlaceMiner,
            WaitForMining,
            SelectSmelterTool,
            PlaceSmelter,
            InspectHub,
            Complete
        }

        private static readonly Brush PanelBrush = BrushFromRgb(24, 35, 38, 244);
        private static readonly Brush AccentBrush = BrushFromRgb(130, 229, 107);
        private static readonly Brush TextBrush = BrushFromRgb(248, 246, 224);
        private static readonly Brush MutedBrush = BrushFromRgb(195, 207, 183);

        private TutorialStep _step = TutorialStep.Inactive;
        private long _initialDeposit;

        internal bool IsActive
        {
            get
            {
                return _step != TutorialStep.Inactive && _step != TutorialStep.Complete;
            }
        }

        internal string Message { get; private set; }

        internal void Begin(long initialDeposit)
        {
            _initialDeposit = initialDeposit;
            _step = TutorialStep.SelectMinerTool;
            Message = "Willkommen! Ich begleite dich durch deine ersten Fabrikschritte.";
        }

        internal void Reset()
        {
            _step = TutorialStep.Inactive;
            Message = string.Empty;
        }

        internal bool TrySelectTool(G5_Bauwerkzeug tool, out string message)
        {
            message = Message;
            if (!IsActive)
            {
                return true;
            }

            if (_step == TutorialStep.SelectMinerTool && tool == G5_Bauwerkzeug.Foerderer)
            {
                _step = TutorialStep.PlaceMiner;
                Message = "Gut! Klicke jetzt auf das markierte Eisenerzfeld, um den Förderer zu bauen.";
                message = Message;
                return true;
            }

            if (_step == TutorialStep.SelectSmelterTool && tool == G5_Bauwerkzeug.Schmelze)
            {
                _step = TutorialStep.PlaceSmelter;
                Message = "Jetzt klicke auf das markierte freie Feld, um eine Schmelze zu platzieren.";
                message = Message;
                return true;
            }

            message = _step == TutorialStep.SelectMinerTool
                ? "Wähle zuerst mit F den Förderer aus."
                : _step == TutorialStep.WaitForMining
                    ? "Lass den Förderer zuerst ein Erz abbauen. Danach geht es weiter."
                    : _step == TutorialStep.SelectSmelterTool
                        ? "Wähle jetzt mit S eine Schmelze aus."
                        : "Folge dem markierten Lernschritt, bevor du fortfährst.";
            return false;
        }

        internal bool CanPlaceBuilding(
            G5_Bauwerkzeug tool,
            int x,
            int y,
            out string message)
        {
            message = Message;
            if (!IsActive)
            {
                return true;
            }

            if (_step == TutorialStep.PlaceMiner &&
                tool == G5_Bauwerkzeug.Foerderer &&
                x == 5 && y == 2)
            {
                return true;
            }

            if (_step == TutorialStep.PlaceSmelter &&
                tool == G5_Bauwerkzeug.Schmelze &&
                x == 20 && y == 14)
            {
                return true;
            }

            message = _step == TutorialStep.PlaceMiner
                ? "Platziere den Förderer auf dem markierten Eisenerzfeld."
                : _step == TutorialStep.PlaceSmelter
                    ? "Platziere die Schmelze auf dem markierten freien Feld."
                    : "Folge dem markierten Lernschritt. Die Anleitung lässt sich anfangs nicht überspringen.";
            return false;
        }

        internal void BuildingPlaced(G5_Bauwerkzeug tool, int x, int y, long depositAmount)
        {
            if (_step == TutorialStep.PlaceMiner &&
                tool == G5_Bauwerkzeug.Foerderer &&
                x == 5 && y == 2)
            {
                _initialDeposit = depositAmount;
                _step = TutorialStep.WaitForMining;
                Message = "Förderer gebaut! Er baut Erz automatisch ab. Warte, bis das erste Erz abgebaut ist.";
            }
            else if (_step == TutorialStep.PlaceSmelter &&
                tool == G5_Bauwerkzeug.Schmelze &&
                x == 20 && y == 14)
            {
                _step = TutorialStep.InspectHub;
                Message = "Schmelze gebaut! Klicke jetzt auf das markierte Hub, um Lager und Transport zu sehen.";
            }
        }

        internal bool ObserveMining(long remainingDeposit)
        {
            if (_step != TutorialStep.WaitForMining ||
                remainingDeposit >= _initialDeposit)
            {
                return false;
            }

            _step = TutorialStep.SelectSmelterTool;
            Message = "Prima, dein Förderer hat Erz abgebaut! Drücke S, um eine Schmelze auszuwählen.";
            return true;
        }

        internal bool TryInspectHub(int x, int y)
        {
            if (_step != TutorialStep.InspectHub ||
                x < 1 || x >= 3 || y < 1 || y >= 3)
            {
                return false;
            }

            _step = TutorialStep.Complete;
            Message = "Geschafft! Du hast einen Förderer gebaut, Erz abgebaut und die Fabrik erweitert. Ab jetzt kannst du frei spielen.";
            return true;
        }

        internal void Paint(
            Canvas canvas,
            double canvasWidth,
            double canvasHeight,
            IG5_WeltSpielfeld world)
        {
            if (!IsActive || canvas == null || world == null ||
                canvasWidth <= 0 || canvasHeight <= 0)
            {
                return;
            }

            canvas.Width = canvasWidth;
            canvas.Height = canvasHeight;
            canvas.Children.Clear();
            double panelWidth = Math.Min(590, canvasWidth - 32);
            const double panelHeight = 126;
            double panelLeft = 16;
            double panelTop = Math.Max(12, canvasHeight - panelHeight - 16);

            DrawTarget(canvas, world, panelLeft + panelWidth * 0.62, panelTop);
            AddRoundedRectangle(canvas, PanelBrush, panelLeft, panelTop, panelWidth, panelHeight, 18);
            DrawAvatar(canvas, panelLeft + 18, panelTop + 45);
            AddRoundedRectangle(canvas, AccentBrush, panelLeft + 68, panelTop + 17, 90, 24, 12);
            AddText(canvas, "MENTOR  " + StepLabel(), Brushes.Black,
                panelLeft + 76, panelTop + 22, 78, 14, 9, FontWeights.Bold);
            AddText(canvas, StepTitle(), TextBrush,
                panelLeft + 68, panelTop + 51, panelWidth - 88, 24, 16, FontWeights.Bold);
            AddText(canvas, Message ?? string.Empty, MutedBrush,
                panelLeft + 68, panelTop + 80, panelWidth - 88, 34, 12, FontWeights.Normal);
        }

        private static void DrawAvatar(Canvas canvas, double left, double top)
        {
            AddCircle(canvas, BrushFromRgb(70, 125, 75), left, top, 44, 1);
            AddCircle(canvas, BrushFromRgb(240, 191, 137), left + 9, top + 8, 26, 1);
            AddRoundedRectangle(canvas, BrushFromRgb(238, 190, 75), left + 6, top + 3, 32, 11, 6);
            AddCircle(canvas, Brushes.Black, left + 16, top + 19, 3, 1);
            AddCircle(canvas, Brushes.Black, left + 27, top + 19, 3, 1);
            AddRoundedRectangle(canvas, BrushFromRgb(153, 79, 58), left + 18, top + 29, 10, 3, 2);
        }

        private void DrawTarget(Canvas canvas, IG5_WeltSpielfeld world, double panelX, double panelY)
        {
            int targetX;
            int targetY;
            switch (_step)
            {
                case TutorialStep.SelectMinerTool:
                case TutorialStep.PlaceMiner:
                case TutorialStep.WaitForMining:
                    targetX = 5;
                    targetY = 2;
                    break;
                case TutorialStep.SelectSmelterTool:
                    targetX = 20;
                    targetY = 14;
                    break;
                case TutorialStep.PlaceSmelter:
                    targetX = 20;
                    targetY = 14;
                    break;
                case TutorialStep.InspectHub:
                    targetX = 1;
                    targetY = 1;
                    break;
                default:
                    return;
            }

            if (!world.TryRasterZuBildschirm(targetX, targetY, out Point target))
            {
                return;
            }

            double tileWidth = 32;
            double tileHeight = 32;
            if (targetX + 1 < world.Breite &&
                world.TryRasterZuBildschirm(targetX + 1, targetY, out Point nextColumn))
            {
                tileWidth = nextColumn.X - target.X;
            }
            if (targetY + 1 < world.Hoehe &&
                world.TryRasterZuBildschirm(targetX, targetY + 1, out Point nextRow))
            {
                tileHeight = nextRow.Y - target.Y;
            }

            target = new Point(target.X + tileWidth / 2, target.Y + tileHeight / 2);
            Line pointer = new Line
            {
                X1 = panelX,
                Y1 = panelY,
                X2 = target.X,
                Y2 = target.Y,
                Stroke = AccentBrush,
                StrokeThickness = 3,
                Opacity = 0.85,
                IsHitTestVisible = false
            };
            canvas.Children.Add(pointer);
            AddCircle(canvas, AccentBrush, target.X - 18, target.Y - 18, 36, 0.22);
            AddCircle(canvas, AccentBrush, target.X - 5, target.Y - 5, 10, 0.95);
        }

        private string StepLabel()
        {
            switch (_step)
            {
                case TutorialStep.SelectMinerTool: return "1 / 6";
                case TutorialStep.PlaceMiner: return "2 / 6";
                case TutorialStep.WaitForMining: return "3 / 6";
                case TutorialStep.SelectSmelterTool: return "4 / 6";
                case TutorialStep.PlaceSmelter: return "5 / 6";
                case TutorialStep.InspectHub: return "6 / 6";
                default: return string.Empty;
            }
        }

        private string StepTitle()
        {
            switch (_step)
            {
                case TutorialStep.SelectMinerTool: return "Wähle deinen ersten Förderer";
                case TutorialStep.PlaceMiner: return "Platziere den Förderer auf Erz";
                case TutorialStep.WaitForMining: return "Dein Förderer arbeitet";
                case TutorialStep.SelectSmelterTool: return "Erweitere die Produktion";
                case TutorialStep.PlaceSmelter: return "Baue eine Schmelze";
                case TutorialStep.InspectHub: return "Sieh dir das Hub an";
                default: return string.Empty;
            }
        }

        private static void AddRoundedRectangle(
            Canvas canvas,
            Brush brush,
            double left,
            double top,
            double width,
            double height,
            double radius)
        {
            Rectangle rectangle = new Rectangle
            {
                Width = Math.Max(0, width),
                Height = Math.Max(0, height),
                Fill = brush,
                RadiusX = radius,
                RadiusY = radius,
                Stroke = BrushFromRgb(255, 255, 255, 45),
                StrokeThickness = 1,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            canvas.Children.Add(rectangle);
        }

        private static void AddCircle(
            Canvas canvas,
            Brush brush,
            double left,
            double top,
            double size,
            double opacity)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = brush,
                Opacity = opacity,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);
            canvas.Children.Add(ellipse);
        }

        private static void AddText(
            Canvas canvas,
            string text,
            Brush brush,
            double left,
            double top,
            double width,
            double height,
            double fontSize,
            FontWeight fontWeight)
        {
            TextBlock block = new TextBlock
            {
                Text = text,
                Foreground = brush,
                FontSize = fontSize,
                FontWeight = fontWeight,
                TextWrapping = TextWrapping.Wrap,
                Width = Math.Max(0, width),
                Height = Math.Max(0, height),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(block, left);
            Canvas.SetTop(block, top);
            canvas.Children.Add(block);
        }

        private static Brush BrushFromRgb(byte red, byte green, byte blue, byte alpha = 255)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(alpha, red, green, blue));
            brush.Freeze();
            return brush;
        }
    }
}
