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
            SelectMoveTool,
            SelectMoveSource,
            MoveSmelter,
            SelectConveyorTool,
            PlaceConveyor,
            WaitForTransport,
            InspectHub,
            Complete
        }

        private static readonly Brush PanelBrush = BrushFromRgb(24, 35, 38, 244);
        private static readonly Brush AccentBrush = BrushFromRgb(130, 229, 107);
        private static readonly Brush TextBrush = BrushFromRgb(248, 246, 224);
        private static readonly Brush MutedBrush = BrushFromRgb(195, 207, 183);

        private TutorialStep _step = TutorialStep.Inactive;
        private long _initialDeposit;
        private int _conveyorIndex;
        private int _minerX;
        private int _minerY;
        private const int SmelterStartX = 20;
        private const int SmelterStartY = 14;
        private int _smelterDestinationX;
        private int _smelterDestinationY;

        internal bool IsActive
        {
            get
            {
                return _step != TutorialStep.Inactive && _step != TutorialStep.Complete;
            }
        }

        internal string Message { get; private set; }

        internal void Begin(long initialDeposit, G5_GridPosition minerPosition)
        {
            _initialDeposit = initialDeposit;
            _conveyorIndex = 0;
            _minerX = minerPosition.X;
            _minerY = minerPosition.Y;
            _smelterDestinationX = _minerX + 5;
            _smelterDestinationY = _minerY;
            _step = TutorialStep.SelectMinerTool;
            Message = "Willkommen! Ich begleite dich durch deine ersten Fabrikschritte.";
        }

        internal void Skip()
        {
            _step = TutorialStep.Complete;
            Message = "Tutorial übersprungen. Du kannst jetzt frei spielen.";
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

            if (_step == TutorialStep.SelectMoveTool &&
                tool == G5_Bauwerkzeug.GebaeudeVerschieben)
            {
                _step = TutorialStep.SelectMoveSource;
                Message = "Klicke jetzt auf die Schmelze, die du verschieben möchtest.";
                message = Message;
                return true;
            }

            if (_step == TutorialStep.SelectConveyorTool &&
                tool == G5_Bauwerkzeug.Foerderband)
            {
                _conveyorIndex = 0;
                _step = TutorialStep.PlaceConveyor;
                Message = "Platziere das erste Förderband auf dem markierten Feld neben dem Förderer.";
                message = Message;
                return true;
            }

            message = _step == TutorialStep.SelectMinerTool
                ? "Wähle zuerst mit F den Förderer aus."
                : _step == TutorialStep.WaitForMining
                    ? "Lass den Förderer zuerst ein Erz abbauen. Danach geht es weiter."
                    : _step == TutorialStep.SelectSmelterTool
                        ? "Wähle jetzt mit S eine Schmelze aus."
                        : _step == TutorialStep.SelectMoveTool
                            ? "Drücke M, um das Verschieben zu lernen."
                            : _step == TutorialStep.SelectConveyorTool
                                ? "Drücke B, um ein Förderband auszuwählen."
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
                x == _minerX && y == _minerY)
            {
                return true;
            }

            if (_step == TutorialStep.PlaceSmelter &&
                tool == G5_Bauwerkzeug.Schmelze &&
                x == SmelterStartX && y == SmelterStartY)
            {
                return true;
            }

            if (_step == TutorialStep.PlaceConveyor &&
                tool == G5_Bauwerkzeug.Foerderband &&
                _conveyorIndex < 3 &&
                x == ConveyorX() && y == _minerY)
            {
                return true;
            }

            message = _step == TutorialStep.PlaceMiner
                ? "Platziere den Förderer auf dem markierten Rohstofffeld."
                : _step == TutorialStep.PlaceSmelter
                    ? "Platziere die Schmelze auf dem markierten freien Feld."
                    : _step == TutorialStep.PlaceConveyor
                        ? "Setze das Förderband auf das vom Mentor markierte Feld."
                        : "Folge dem markierten Lernschritt oder drücke T, um das Tutorial zu überspringen.";
            return false;
        }

        internal void BuildingPlaced(G5_Bauwerkzeug tool, int x, int y, long depositAmount)
        {
            if (_step == TutorialStep.PlaceMiner &&
                tool == G5_Bauwerkzeug.Foerderer &&
                x == _minerX && y == _minerY)
            {
                _initialDeposit = depositAmount;
                _step = TutorialStep.WaitForMining;
                Message = "Förderer gebaut! Er baut Erz automatisch ab. Warte, bis das erste Erz abgebaut ist.";
            }
            else if (_step == TutorialStep.PlaceSmelter &&
                tool == G5_Bauwerkzeug.Schmelze &&
                x == SmelterStartX && y == SmelterStartY)
            {
                _step = TutorialStep.SelectMoveTool;
                Message = "Schmelze gebaut! Drücke M und klicke sie an, damit du sie versetzen kannst.";
            }
            else if (_step == TutorialStep.PlaceConveyor &&
                tool == G5_Bauwerkzeug.Foerderband &&
                _conveyorIndex < 3 &&
                x == ConveyorX() && y == _minerY)
            {
                _conveyorIndex++;
                if (_conveyorIndex == 3)
                {
                    _step = TutorialStep.WaitForTransport;
                    Message = "Förderbänder verbunden! Beobachte, wie Erz auf dem Band zur Schmelze transportiert wird.";
                }
                else
                {
                    Message = "Gut! Setze das nächste Förderband direkt an das vorherige, auf das markierte Feld.";
                }
            }
        }

        internal bool TrySelectBuildingToMove(int x, int y, out string message)
        {
            message = Message;
            if (!IsActive)
            {
                return true;
            }

            if (_step == TutorialStep.SelectMoveSource &&
                x == SmelterStartX && y == SmelterStartY)
            {
                _step = TutorialStep.MoveSmelter;
                Message = "Jetzt klicke auf das markierte freie Feld, um die Schmelze dorthin zu versetzen.";
                message = Message;
                return true;
            }

            message = _step == TutorialStep.SelectMoveSource
                ? "Klicke auf die markierte Schmelze, nicht auf ein anderes Gebäude."
                : "Folge zuerst der aktuellen Anleitung.";
            return false;
        }

        internal bool CanMoveBuilding(int x, int y, out string message)
        {
            message = Message;
            if (!IsActive)
            {
                return true;
            }

            if (_step == TutorialStep.MoveSmelter &&
                x == _smelterDestinationX && y == _smelterDestinationY)
            {
                return true;
            }

            message = _step == TutorialStep.MoveSmelter
                ? "Versetze die Schmelze auf das markierte Zielfeld."
                : "Folge zuerst der aktuellen Anleitung.";
            return false;
        }

        internal void BuildingMoved()
        {
            if (_step != TutorialStep.MoveSmelter)
            {
                return;
            }

            _step = TutorialStep.SelectConveyorTool;
            Message = "Schmelze verschoben! Drücke B, um jetzt Förderbänder auszuwählen.";
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

        internal bool ObserveConveyorTransport(bool transportOccurred)
        {
            if (_step != TutorialStep.WaitForTransport || !transportOccurred)
            {
                return false;
            }

            _step = TutorialStep.InspectHub;
            Message = "Prima! Erz wird über die Förderbänder transportiert. Klicke jetzt auf das markierte Hub.";
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
            Message = "Geschafft! Du hast gebaut, ein Gebäude versetzt und Rohstoffe über Förderbänder transportiert. Ab jetzt kannst du frei spielen.";
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
                panelLeft + 68, panelTop + 80, panelWidth - 210, 34, 12, FontWeights.Normal);
            AddText(canvas, "T: Tutorial überspringen", AccentBrush,
                panelLeft + panelWidth - 142, panelTop + 99, 126, 16, 10, FontWeights.Bold);
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
                    targetX = _minerX;
                    targetY = _minerY;
                    break;
                case TutorialStep.SelectSmelterTool:
                case TutorialStep.PlaceSmelter:
                    targetX = SmelterStartX;
                    targetY = SmelterStartY;
                    break;
                case TutorialStep.SelectMoveTool:
                case TutorialStep.SelectMoveSource:
                    targetX = SmelterStartX;
                    targetY = SmelterStartY;
                    break;
                case TutorialStep.MoveSmelter:
                    targetX = _smelterDestinationX;
                    targetY = _smelterDestinationY;
                    break;
                case TutorialStep.SelectConveyorTool:
                case TutorialStep.PlaceConveyor:
                    targetX = ConveyorX();
                    targetY = _minerY;
                    break;
                case TutorialStep.WaitForTransport:
                    targetX = _minerX + 3;
                    targetY = _minerY;
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

            Point start = new Point(panelX, panelY - 5);
            target = new Point(target.X + tileWidth / 2, target.Y + tileHeight / 2);
            Vector direction = target - start;
            if (direction.Length == 0)
            {
                return;
            }
            direction.Normalize();
            Vector curve = new Vector(-direction.Y, direction.X);
            Vector distance = target - start;
            Point control1 = start + distance * 0.34 + curve * 46;
            Point control2 = start + distance * 0.72 + curve * 25;
            PathGeometry arrowPath = new PathGeometry(new[]
            {
                new PathFigure(start, new PathSegment[]
                {
                    new BezierSegment(control1, control2, target, true)
                }, false)
            });
            Path glow = new Path
            {
                Data = arrowPath,
                Stroke = BrushFromRgb(130, 229, 107, 90),
                StrokeThickness = 10,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false
            };
            canvas.Children.Add(glow);

            Path pointer = new Path
            {
                Data = arrowPath,
                Stroke = AccentBrush,
                StrokeThickness = 4,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 9,
                    ShadowDepth = 2,
                    Opacity = 0.45
                },
                IsHitTestVisible = false
            };
            canvas.Children.Add(pointer);

            Point headBase = target - direction * 19;
            Vector headSide = curve * 9;
            Polygon arrowHead = new Polygon
            {
                Points = new PointCollection
                {
                    target,
                    headBase + headSide,
                    headBase - headSide
                },
                Fill = AccentBrush,
                Stroke = Brushes.White,
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            };
            canvas.Children.Add(arrowHead);

            Ellipse targetRing = new Ellipse
            {
                Width = 38,
                Height = 38,
                Fill = BrushFromRgb(130, 229, 107, 42),
                Stroke = AccentBrush,
                StrokeThickness = 2,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(targetRing, target.X - targetRing.Width / 2);
            Canvas.SetTop(targetRing, target.Y - targetRing.Height / 2);
            canvas.Children.Add(targetRing);
            AddCircle(canvas, Brushes.White, target.X - 5, target.Y - 5, 10, 0.95);
        }

        private string StepLabel()
        {
            int number;
            switch (_step)
            {
                case TutorialStep.SelectMinerTool: number = 1; break;
                case TutorialStep.PlaceMiner: number = 2; break;
                case TutorialStep.WaitForMining: number = 3; break;
                case TutorialStep.SelectSmelterTool: number = 4; break;
                case TutorialStep.PlaceSmelter: number = 5; break;
                case TutorialStep.SelectMoveTool: number = 6; break;
                case TutorialStep.SelectMoveSource: number = 7; break;
                case TutorialStep.MoveSmelter: number = 8; break;
                case TutorialStep.SelectConveyorTool: number = 9; break;
                case TutorialStep.PlaceConveyor: number = 10 + _conveyorIndex; break;
                case TutorialStep.WaitForTransport: number = 13; break;
                case TutorialStep.InspectHub: number = 14; break;
                default: return string.Empty;
            }

            return number + " / 14";
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
                case TutorialStep.SelectMoveTool: return "Lerne Gebäude zu versetzen";
                case TutorialStep.SelectMoveSource: return "Wähle die Schmelze aus";
                case TutorialStep.MoveSmelter: return "Versetze die Schmelze";
                case TutorialStep.SelectConveyorTool: return "Wähle ein Förderband";
                case TutorialStep.PlaceConveyor: return "Förderband " + (_conveyorIndex + 1) + " von 3 platzieren";
                case TutorialStep.WaitForTransport: return "Rohstoffe auf dem Band";
                case TutorialStep.InspectHub: return "Sieh dir das Hub an";
                default: return string.Empty;
            }
        }

        private int ConveyorX()
        {
            return _minerX + 2 + Math.Min(_conveyorIndex, 2);
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
