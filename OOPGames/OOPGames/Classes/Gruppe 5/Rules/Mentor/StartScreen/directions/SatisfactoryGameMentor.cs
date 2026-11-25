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
            Inactive = -1,
            FindDeposit,
            MineResource,
            GatherStarterMaterials,
            BuildSmelter,
            ConnectConveyor,
            BuildCoalMiner,
            BuildGenerator,
            BuildPowerPole,
            BuildConstructor,
            BuildAssembler,
            BuildStorage,
            BuildRocket,
            Complete
        }

        private const int TutorialStepCount = 12;
        private static readonly Brush PanelBrush = BrushFromRgb(24, 35, 38, 244);
        private static readonly Brush AccentBrush = BrushFromRgb(130, 229, 107);
        private static readonly Brush TextBrush = BrushFromRgb(248, 246, 224);
        private static readonly Brush MutedBrush = BrushFromRgb(195, 207, 183);

        private TutorialStep _step;
        private string _hint;
        private bool _hasMoved;
        private bool _hasMined;
        private bool _hasStarterMiner;
        private bool _hasSmelter;
        private bool _hasConveyorLine;
        private bool _hasCoalMiner;
        private bool _hasGenerator;
        private bool _hasPowerPole;
        private bool _hasConstructor;
        private bool _hasAssembler;
        private bool _hasStorage;
        private bool _hasRocket;

        internal bool IsActive { get; private set; }
        internal string Message { get; private set; }
        internal string Hint => _hint;
        internal string Progress => _step == TutorialStep.Complete
            ? "TUTORIAL ABGESCHLOSSEN"
            : "SCHRITT " + ((int)_step + 1) + "/" + TutorialStepCount;

        internal void Begin()
        {
            IsActive = true;
            _hasMoved = false;
            _hasMined = false;
            _hasStarterMiner = false;
            _hasSmelter = false;
            _hasConveyorLine = false;
            _hasCoalMiner = false;
            _hasGenerator = false;
            _hasPowerPole = false;
            _hasConstructor = false;
            _hasAssembler = false;
            _hasStorage = false;
            _hasRocket = false;
            _hint = string.Empty;
            _step = TutorialStep.Inactive;
            SetNextStep();
        }

        internal void ObservePlayerMoved()
        {
            _hasMoved = true;
            SetNextStep();
        }

        internal void ObserveHandMining(G5_Item item)
        {
            _hasMined = true;
            SetNextStep();
            Explain("+" + G5_FabrikBezeichnungen.Gegenstand(item) +
                ". Rohstoffe aus Vorkommen kommen direkt in deinen Rucksack.");
        }

        internal void ObserveBuildingPlaced(
            G5_Bauwerkzeug tool,
            string buildingName,
            string explanation,
            G5_Item? minedResource = null)
        {
            if (tool == G5_Bauwerkzeug.Foerderer && minedResource == G5_Item.Eisenerz)
            {
                _hasStarterMiner = true;
            }
            else if (tool == G5_Bauwerkzeug.Foerderer && minedResource == G5_Item.Kohle)
            {
                _hasCoalMiner = true;
            }

            _hasSmelter |= tool == G5_Bauwerkzeug.Schmelze;
            _hasGenerator |= tool == G5_Bauwerkzeug.Verbrennungsgenerator;
            _hasPowerPole |= tool == G5_Bauwerkzeug.Strommast;
            _hasConstructor |= tool == G5_Bauwerkzeug.Konstruktor;
            _hasAssembler |= tool == G5_Bauwerkzeug.Assembler;
            _hasStorage |= tool == G5_Bauwerkzeug.Lager;
            _hasRocket |= tool == G5_Bauwerkzeug.Rakete;
            SetNextStep();
            Explain(buildingName + " gebaut. " + explanation);
        }

        internal void ObserveConveyorLinePlaced()
        {
            _hasConveyorLine = true;
            SetNextStep();
            Explain("Förderbandlinie verbunden: Rohstoffe werden über alle Segmente weitergegeben.");
        }

        internal void ObserveManualCraftStarted(G5_Recipe recipe)
        {
            Explain(recipe.Name + " gestartet. Die Zutaten kommen aus deinem Rucksack; " +
                "das Ergebnis wird nach kurzer Zeit dort abgelegt.");
        }

        internal void ObserveManualCraftCompleted(G5_Recipe recipe)
        {
            Explain(recipe.OutputAmount + " " + G5_FabrikBezeichnungen.Gegenstand(recipe.Output) +
                " hergestellt. Die ⚒-Gegenstände im Inventar führen von Rohstoffen über Barren " +
                "zu Bauteilen und Beton.");
        }

        internal void Explain(string message)
        {
            if (IsActive && !string.IsNullOrWhiteSpace(message))
            {
                _hint = message;
            }
        }

        private void SetNextStep()
        {
            TutorialStep nextStep =
                !_hasMoved ? TutorialStep.FindDeposit :
                !_hasMined ? TutorialStep.MineResource :
                !_hasStarterMiner ? TutorialStep.GatherStarterMaterials :
                !_hasSmelter ? TutorialStep.BuildSmelter :
                !_hasConveyorLine ? TutorialStep.ConnectConveyor :
                !_hasCoalMiner ? TutorialStep.BuildCoalMiner :
                !_hasGenerator ? TutorialStep.BuildGenerator :
                !_hasPowerPole ? TutorialStep.BuildPowerPole :
                !_hasConstructor ? TutorialStep.BuildConstructor :
                !_hasAssembler ? TutorialStep.BuildAssembler :
                !_hasStorage ? TutorialStep.BuildStorage :
                !_hasRocket ? TutorialStep.BuildRocket :
                TutorialStep.Complete;

            if (_step != nextStep)
            {
                _step = nextStep;
                Message = InstructionFor(nextStep);
                _hint = string.Empty;
            }
        }

        private static string InstructionFor(TutorialStep step)
        {
            switch (step)
            {
                case TutorialStep.FindDeposit:
                    return "Bewege dich mit WASD zu einem Rohstoffvorkommen. Der erste Förderer kostet nur abbaubares Eisenerz und Holz.";
                case TutorialStep.MineResource:
                    return "Laufe neben Erz oder Holz, richte den Blick auf das Vorkommen und drücke E zum Handabbau.";
                case TutorialStep.GatherStarterMaterials:
                    return "Sammle 8 Eisenerz und 5 Holz. Wähle danach im Bauregister den Förderer und setze ihn auf ein Eisenvorkommen.";
                case TutorialStep.BuildSmelter:
                    return "Baue als Nächstes eine Schmelze (3 Eisenbarren, 2 Beton, 3 Holz). Fehlende Bauteile kannst du im Inventar herstellen: Gegenstand wählen, Anzahl eingeben, HERSTELLEN.";
                case TutorialStep.ConnectConveyor:
                    return "Ziehe eine Förderbandlinie vom Förderer zur Schmelze (je neues Segment: 1 Eisenbarren, 1 Eisenplatte, 1 Beton). Wähle in der Schmelze das passende Erzrezept.";
                case TutorialStep.BuildCoalMiner:
                    return "Finde ein Kohlevorkommen und baue dort einen zweiten Förderer. Kohle wird für die Stromerzeugung gebraucht.";
                case TutorialStep.BuildGenerator:
                    return "Baue ein Kohlekraftwerk und versorge es mit Kohle aus dem Lager oder Rucksack.";
                case TutorialStep.BuildPowerPole:
                    return "Setze einen Strommast in Reichweite von Generator und Maschinen; seine Reichweite beträgt 8 Felder.";
                case TutorialStep.BuildConstructor:
                    return "Baue einen Konstruktor. Er verarbeitet Eisenbarren zu Platten/Stangen und kann Kupferdraht oder Schrauben herstellen.";
                case TutorialStep.BuildAssembler:
                    return "Baue einen Assembler und versorge ihn mit Strom. Er kombiniert Eisenstange und Schraube zu einem Rotor.";
                case TutorialStep.BuildStorage:
                    return "Baue ein Lager (2 Eisenplatten, 1 Eisenstange, 2 Beton, 4 Holz), um die gemeinsame Lagerkapazität zu erhöhen.";
                case TutorialStep.BuildRocket:
                    return "Endziel: Baue eine Rakete, stelle 40 Eisenplatten, 30 Beton und 20 Rotoren bereit und tanke 50 Uran.";
                default:
                    return "Die Fabrik ist startklar. Du kannst Rezepte, Upgrades und weitere Gebäude frei erkunden oder T drücken.";
            }
        }

        internal void Skip()
        {
            IsActive = false;
            _step = TutorialStep.Complete;
            Message = "Tutorial übersprungen. Du kannst jederzeit frei spielen.";
            _hint = string.Empty;
        }

        internal void Reset()
        {
            IsActive = false;
            _step = TutorialStep.Inactive;
            Message = string.Empty;
            _hint = string.Empty;
        }

        internal void Paint(
            Canvas canvas,
            double canvasWidth,
            double canvasHeight,
            IG5_WeltSpielfeld world,
            bool compact = false)
        {
            if (!IsActive || canvas == null || world == null ||
                canvasWidth <= 32 || canvasHeight <= 32)
            {
                return;
            }

            canvas.Width = canvasWidth;
            canvas.Height = canvasHeight;
            canvas.Children.Clear();

            double panelWidth = Math.Min(compact ? 760 : 520, canvasWidth - 32);
            double panelHeight = Math.Min(compact ? 56 : 180, canvasHeight - 24);
            double left = 16;
            double top = compact ? 58 : Math.Max(68, canvasHeight - panelHeight - 16);

            AddRoundedRectangle(canvas, PanelBrush, AccentBrush, left, top, panelWidth, panelHeight);
            if (compact)
            {
                AddText(canvas, Progress, AccentBrush,
                    left + 12, top + 3, 135, 16, 9, FontWeights.Bold);
                AddText(canvas, Message, TextBrush,
                    left + 12, top + 19, panelWidth - 125, 32, 9, FontWeights.SemiBold);
                AddText(canvas, "T: überspringen", MutedBrush,
                    left + panelWidth - 112, top + 4, 100, 14, 8, FontWeights.Normal);
                return;
            }

            AddText(canvas, "FABRIK-MENTOR  ·  " + Progress, AccentBrush,
                left + 18, top + 10, panelWidth - 36, 18, 10, FontWeights.Bold);
            AddText(canvas, Message, TextBrush,
                left + 18, top + 34, panelWidth - 36, 70, 11, FontWeights.SemiBold);
            AddText(canvas, Hint, MutedBrush,
                left + 18, top + 104, panelWidth - 36, 48, 9, FontWeights.Normal);
            AddText(canvas, "T: Tutorial überspringen", MutedBrush,
                left + 18, top + 158, panelWidth - 36, 14, 8, FontWeights.Normal);
        }

        private static void AddRoundedRectangle(
            Canvas canvas,
            Brush fill,
            Brush stroke,
            double left,
            double top,
            double width,
            double height)
        {
            Rectangle rectangle = new Rectangle
            {
                Width = width,
                Height = height,
                RadiusX = 12,
                RadiusY = 12,
                Fill = fill,
                Stroke = stroke,
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            canvas.Children.Add(rectangle);
        }

        private static void AddText(
            Canvas canvas,
            string text,
            Brush foreground,
            double left,
            double top,
            double width,
            double height,
            double fontSize,
            FontWeight fontWeight)
        {
            TextBlock block = new TextBlock
            {
                Text = text ?? string.Empty,
                Foreground = foreground,
                FontSize = fontSize,
                FontWeight = fontWeight,
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Center
            };
            Canvas.SetLeft(block, left);
            Canvas.SetTop(block, top);
            block.Width = width;
            block.Height = height;
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
