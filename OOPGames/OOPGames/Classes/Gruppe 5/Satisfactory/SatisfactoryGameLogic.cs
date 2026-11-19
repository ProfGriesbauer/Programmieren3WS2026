using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public enum SatisfactoryItem
    {
        IronOre,
        CopperOre,
        Coal,
        IronIngot,
        CopperIngot,
        IronPlate,
        IronRod,
        Wire
    }

    public enum SatisfactoryMachineType
    {
        Smelter,
        Constructor
    }

    public enum SatisfactoryGameMode
    {
        Cozy,
        Competitive
    }

    public sealed class SatisfactoryInventory
    {
        public const int StackSize = 100;
        private readonly Dictionary<SatisfactoryItem, int> _items =
            new Dictionary<SatisfactoryItem, int>();

        public SatisfactoryInventory(int slotCapacity)
        {
            if (slotCapacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(slotCapacity));
            }

            SlotCapacity = slotCapacity;
        }

        public int SlotCapacity { get; private set; }
        public int UsedSlots { get { return _items.Values.Sum(CountSlots); } }
        public int FreeSlots { get { return SlotCapacity - UsedSlots; } }

        public int Count(SatisfactoryItem item)
        {
            int amount;
            return _items.TryGetValue(item, out amount) ? amount : 0;
        }

        public IReadOnlyDictionary<SatisfactoryItem, int> Items
        {
            get { return new Dictionary<SatisfactoryItem, int>(_items); }
        }

        public bool CanAdd(SatisfactoryItem item, int amount)
        {
            if (amount <= 0)
            {
                return true;
            }

            int current = Count(item);
            int additionalCapacity = FreeSlots * StackSize;
            if (current > 0 && current % StackSize != 0)
            {
                additionalCapacity += StackSize - current % StackSize;
            }

            return amount <= additionalCapacity;
        }

        public int Add(SatisfactoryItem item, int amount)
        {
            if (amount <= 0)
            {
                return 0;
            }

            int current = Count(item);
            int accepted = Math.Min(amount, AvailableCapacity(item));
            if (accepted > 0)
            {
                _items[item] = current + accepted;
            }

            return accepted;
        }

        public bool Remove(SatisfactoryItem item, int amount)
        {
            int current = Count(item);
            if (amount <= 0 || current < amount)
            {
                return false;
            }

            int remaining = current - amount;
            if (remaining == 0)
            {
                _items.Remove(item);
            }
            else
            {
                _items[item] = remaining;
            }

            return true;
        }

        public int TransferTo(SatisfactoryInventory destination, SatisfactoryItem item, int amount)
        {
            if (destination == null || amount <= 0)
            {
                return 0;
            }

            int accepted = destination.Add(item, Math.Min(amount, Count(item)));
            if (accepted > 0)
            {
                Remove(item, accepted);
            }

            return accepted;
        }

        private static int CountSlots(int amount)
        {
            return (amount + StackSize - 1) / StackSize;
        }

        private int AvailableCapacity(SatisfactoryItem item)
        {
            int capacity = FreeSlots * StackSize;
            int current = Count(item);
            if (current > 0 && current % StackSize != 0)
            {
                capacity += StackSize - current % StackSize;
            }

            return capacity;
        }
    }

    public sealed class SatisfactoryRecipe
    {
        private readonly Dictionary<SatisfactoryItem, int> _inputs;
        private readonly Dictionary<SatisfactoryItem, int> _outputs;

        public SatisfactoryRecipe(
            string name,
            double durationSeconds,
            IDictionary<SatisfactoryItem, int> inputs,
            IDictionary<SatisfactoryItem, int> outputs)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Ein Rezept braucht einen Namen.", nameof(name));
            }

            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            Name = name;
            DurationSeconds = durationSeconds;
            _inputs = new Dictionary<SatisfactoryItem, int>(inputs);
            _outputs = new Dictionary<SatisfactoryItem, int>(outputs);
        }

        public string Name { get; private set; }
        public double DurationSeconds { get; private set; }
        public IReadOnlyDictionary<SatisfactoryItem, int> Inputs
        {
            get { return new Dictionary<SatisfactoryItem, int>(_inputs); }
        }
        public IReadOnlyDictionary<SatisfactoryItem, int> Outputs
        {
            get { return new Dictionary<SatisfactoryItem, int>(_outputs); }
        }

        public static IReadOnlyList<SatisfactoryRecipe> ForMachine(SatisfactoryMachineType machine)
        {
            if (machine == SatisfactoryMachineType.Smelter)
            {
                return new[]
                {
                    Create("Eisenbarren", 2, SatisfactoryItem.IronOre, 1, SatisfactoryItem.IronIngot, 1),
                    Create("Kupferbarren", 2, SatisfactoryItem.CopperOre, 1, SatisfactoryItem.CopperIngot, 1)
                };
            }

            return new[]
            {
                Create("Eisenplatten", 3, SatisfactoryItem.IronIngot, 2, SatisfactoryItem.IronPlate, 1),
                Create("Eisenstangen", 2, SatisfactoryItem.IronIngot, 1, SatisfactoryItem.IronRod, 1),
                Create("Kabel", 2, SatisfactoryItem.CopperIngot, 1, SatisfactoryItem.Wire, 2)
            };
        }

        public static IReadOnlyList<SatisfactoryRecipe> HandRecipes
        {
            get
            {
                return new[]
                {
                    Create("Eisenerz schmelzen", 4, SatisfactoryItem.IronOre, 1, SatisfactoryItem.IronIngot, 1),
                    Create("Kupfererz schmelzen", 4, SatisfactoryItem.CopperOre, 1, SatisfactoryItem.CopperIngot, 1),
                    Create("Eisenplatten", 3, SatisfactoryItem.IronIngot, 2, SatisfactoryItem.IronPlate, 1),
                    Create("Eisenstangen", 2, SatisfactoryItem.IronIngot, 1, SatisfactoryItem.IronRod, 1)
                };
            }
        }

        private static SatisfactoryRecipe Create(
            string name,
            double seconds,
            SatisfactoryItem input,
            int inputAmount,
            SatisfactoryItem output,
            int outputAmount)
        {
            return new SatisfactoryRecipe(name, seconds,
                new Dictionary<SatisfactoryItem, int> { { input, inputAmount } },
                new Dictionary<SatisfactoryItem, int> { { output, outputAmount } });
        }
    }

    public sealed class SatisfactoryResourceNode
    {
        public SatisfactoryResourceNode(SatisfactoryItem resource, int column, int row, int reserve)
        {
            if (reserve < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(reserve));
            }

            Resource = resource;
            Column = column;
            Row = row;
            Reserve = reserve;
        }

        public SatisfactoryItem Resource { get; private set; }
        public int Column { get; internal set; }
        public int Row { get; internal set; }
        public int Reserve { get; private set; }
        public bool IsDepleted { get { return Reserve == 0; } }

        internal bool MineOne()
        {
            if (Reserve == 0)
            {
                return false;
            }

            Reserve--;
            return true;
        }
    }

    public sealed class SatisfactoryProductionBuilding
    {
        private const double FrameworkTickSeconds = 0.1;
        private double _tickAccumulator;
        private double _workProgress;
        private bool _cycleInProgress;
        private G5_ProductionBuilding _frameworkBuilding;

        public SatisfactoryProductionBuilding(
            SatisfactoryMachineType machine,
            int column,
            int row,
            string name)
        {
            Machine = machine;
            Column = column;
            Row = row;
            Name = name;
            Input = new SatisfactoryInventory(8);
            Output = new SatisfactoryInventory(8);
            Recipes = SatisfactoryRecipe.ForMachine(machine);
            SelectedRecipe = Recipes[0];
            _frameworkBuilding = CreateFrameworkBuilding();
        }

        public SatisfactoryMachineType Machine { get; private set; }
        public int Column { get; internal set; }
        public int Row { get; internal set; }
        public int Width { get { return _frameworkBuilding.Width; } }
        public int Height { get { return _frameworkBuilding.Height; } }
        public string Name { get; private set; }
        public SatisfactoryInventory Input { get; private set; }
        public SatisfactoryInventory Output { get; private set; }
        public IReadOnlyList<SatisfactoryRecipe> Recipes { get; private set; }
        public SatisfactoryRecipe SelectedRecipe { get; private set; }
        public double Progress { get; private set; }
        public bool IsPowered
        {
            get { return _frameworkBuilding.IsPowered; }
            internal set { _frameworkBuilding.SetPowered(value); }
        }
        public G5_BuildingStatus Status { get { return _frameworkBuilding.Status; } }
        internal G5_ProductionBuilding FrameworkBuilding { get { return _frameworkBuilding; } }

        internal bool Contains(int column, int row)
        {
            return column >= Column && column < Column + Width &&
                row >= Row && row < Row + Height;
        }

        internal bool SelectRecipe(int index)
        {
            if (index < 0 || index >= Recipes.Count ||
                _cycleInProgress || _frameworkBuilding.InputContents.Count > 0)
            {
                return false;
            }

            SatisfactoryRecipe recipe = Recipes[index];
            if (!_frameworkBuilding.SelectRecipe(recipe.Name))
            {
                throw new InvalidOperationException("Das Rezept ist im Fabrikgebäude nicht verfügbar.");
            }

            SelectedRecipe = recipe;
            _workProgress = 0;
            Progress = 0;
            return true;
        }

        internal void Tick(double elapsedSeconds)
        {
            if (elapsedSeconds <= 0 || SelectedRecipe == null)
            {
                return;
            }

            _tickAccumulator += elapsedSeconds;
            while (_tickAccumulator >= FrameworkTickSeconds)
            {
                _tickAccumulator -= FrameworkTickSeconds;
                PrepareFrameworkInputs();

                bool produced = _frameworkBuilding.Tick();
                if (produced)
                {
                    _cycleInProgress = false;
                    _workProgress = 0;
                    Progress = 0;
                    TransferFrameworkOutput();
                }
                else if (_frameworkBuilding.Status == G5_BuildingStatus.Working)
                {
                    _cycleInProgress = true;
                    _workProgress += FrameworkTickSeconds;
                    Progress = Math.Min(1, _workProgress / SelectedRecipe.DurationSeconds);
                }
            }
        }

        internal bool MoveTo(int column, int row)
        {
            if (_cycleInProgress || _frameworkBuilding.InputContents.Count > 0)
            {
                return false;
            }

            TransferFrameworkOutput();
            if (_frameworkBuilding.OutputContents.Count > 0)
            {
                return false;
            }

            Column = column;
            Row = row;
            _frameworkBuilding = CreateFrameworkBuilding();
            _workProgress = 0;
            Progress = 0;
            _cycleInProgress = false;
            return true;
        }

        private G5_ProductionBuilding CreateFrameworkBuilding()
        {
            G5_RecipeType recipeType = Machine == SatisfactoryMachineType.Smelter
                ? G5_RecipeType.Smelting
                : G5_RecipeType.Construction;
            G5_Recipe[] recipes = Recipes.Select(recipe => CreateFrameworkRecipe(recipe, recipeType)).ToArray();
            G5_GridPosition position = new G5_GridPosition(Column, Row);
            G5_ProductionBuilding building = Machine == SatisfactoryMachineType.Smelter
                ? (G5_ProductionBuilding)new G5_Smelter(position, recipes, inputCapacity: 8, outputCapacity: 8)
                : new G5_Constructor(position, recipes, inputCapacity: 8, outputCapacity: 8);

            if (!building.SelectRecipe(SelectedRecipe.Name))
            {
                throw new InvalidOperationException("Das Standardrezept ist im Fabrikgebäude nicht verfügbar.");
            }

            return building;
        }

        private static G5_Recipe CreateFrameworkRecipe(SatisfactoryRecipe recipe, G5_RecipeType recipeType)
        {
            KeyValuePair<SatisfactoryItem, int>[] outputs = recipe.Outputs.ToArray();
            if (outputs.Length != 1)
            {
                throw new InvalidOperationException(
                    "Die gemeinsame Fabrik-API unterstützt pro Rezept genau ein Produkt.");
            }

            KeyValuePair<G5_Item, int>[] ingredients = recipe.Inputs
                .Select(input => new KeyValuePair<G5_Item, int>(
                    SatisfactoryGameField.ToG5Item(input.Key),
                    input.Value))
                .ToArray();
            int cycleTicks = Math.Max(1, (int)Math.Ceiling(recipe.DurationSeconds / FrameworkTickSeconds));

            return new G5_Recipe(
                recipe.Name,
                recipeType,
                ingredients,
                SatisfactoryGameField.ToG5Item(outputs[0].Key),
                outputs[0].Value,
                cycleTicks,
                powerDemandPerTick: 1);
        }

        private void PrepareFrameworkInputs()
        {
            KeyValuePair<SatisfactoryItem, int>[] inputs = SelectedRecipe.Inputs.ToArray();
            foreach (KeyValuePair<SatisfactoryItem, int> input in inputs)
            {
                G5_Item item = SatisfactoryGameField.ToG5Item(input.Key);
                int inBuilding = _frameworkBuilding.InputContents.TryGetValue(item, out int count)
                    ? count
                    : 0;
                int needed = input.Value - inBuilding;
                if (needed > 0 && Input.Count(input.Key) < needed)
                {
                    return;
                }
            }

            foreach (KeyValuePair<SatisfactoryItem, int> input in inputs)
            {
                G5_Item item = SatisfactoryGameField.ToG5Item(input.Key);
                int inBuilding = _frameworkBuilding.InputContents.TryGetValue(item, out int count)
                    ? count
                    : 0;
                int needed = input.Value - inBuilding;
                if (needed <= 0)
                {
                    continue;
                }

                if (!_frameworkBuilding.CanReceive(item, needed) ||
                    !Input.Remove(input.Key, needed) ||
                    !_frameworkBuilding.TryReceive(item, needed))
                {
                    throw new InvalidOperationException("Einsatzstoffe konnten nicht ins Fabrikgebäude übertragen werden.");
                }
            }
        }

        private void TransferFrameworkOutput()
        {
            KeyValuePair<G5_Item, int>[] outputs = _frameworkBuilding.OutputContents.ToArray();
            foreach (KeyValuePair<G5_Item, int> output in outputs)
            {
                SatisfactoryItem item = SatisfactoryGameField.FromG5Item(output.Key);
                int accepted = Output.Add(item, output.Value);
                if (accepted > 0 && !_frameworkBuilding.TryExtract(output.Key, accepted))
                {
                    throw new InvalidOperationException("Fertige Produkte konnten nicht aus dem Fabrikgebäude entnommen werden.");
                }
            }
        }
    }

    public sealed class SatisfactoryPowerNetwork
    {
        private readonly List<Point> _poles = new List<Point>();
        private readonly List<Point> _generators = new List<Point>();

        public SatisfactoryPowerNetwork()
        {
            _generators.Add(new Point(7, 7));
            _poles.Add(new Point(13, 7));
            _poles.Add(new Point(19, 7));
        }

        public IReadOnlyList<Point> Poles { get { return _poles.AsReadOnly(); } }
        public IReadOnlyList<Point> Generators { get { return _generators.AsReadOnly(); } }

        public bool AddPole(int column, int row)
        {
            if (_poles.Any(pole => pole.X == column && pole.Y == row))
            {
                return false;
            }

            _poles.Add(new Point(column, row));
            return true;
        }

        public bool IsConnected(int column, int row)
        {
            List<Point> connected = new List<Point>();
            foreach (Point source in _generators)
            {
                connected.Add(source);
            }

            bool changed;
            do
            {
                changed = false;
                foreach (Point pole in _poles)
                {
                    if (!connected.Contains(pole) &&
                        connected.Any(node => Distance(node, pole) <= 8))
                    {
                        connected.Add(pole);
                        changed = true;
                    }
                }
            }
            while (changed);

            Point target = new Point(column, row);
            return connected.Any(node => Distance(node, target) <= 8);
        }

        private static double Distance(Point first, Point second)
        {
            double columnDistance = first.X - second.X;
            double rowDistance = first.Y - second.Y;
            return Math.Sqrt(columnDistance * columnDistance + rowDistance * rowDistance);
        }
    }

    public sealed class SatisfactoryGameField : IG5_FabrikSpielInfo
    {
        public const int Columns = 40;
        public const int Rows = 28;
        public const double MissionDurationSeconds = 600;
        public const int MissionTargetPlates = 10;

        private readonly List<SatisfactoryResourceNode> _resources =
            new List<SatisfactoryResourceNode>();
        private readonly List<SatisfactoryProductionBuilding> _buildings =
            new List<SatisfactoryProductionBuilding>();
        private G5_WeltSpielfeld _welt;
        private SatisfactoryRecipe _activeHandRecipe;
        private double _handCraftProgress;
        private SatisfactoryResourceNode _activeMiningResource;
        private double _miningProgress;
        private G5_Bauwerkzeug _aktuellesWerkzeug = G5_Bauwerkzeug.Keines;

        public SatisfactoryGameField()
        {
            PlayerInventory = new SatisfactoryInventory(24);
            Power = new SatisfactoryPowerNetwork();
            HandRecipeIndex = 0;
            Reset();
        }

        public int PlayerColumn { get; private set; }
        public int PlayerRow { get; private set; }
        public SatisfactoryInventory PlayerInventory { get; private set; }
        public SatisfactoryPowerNetwork Power { get; private set; }
        public IReadOnlyList<SatisfactoryResourceNode> Resources { get { return _resources.AsReadOnly(); } }
        public IReadOnlyList<SatisfactoryProductionBuilding> Buildings { get { return _buildings.AsReadOnly(); } }
        public SatisfactoryProductionBuilding SelectedBuilding { get; private set; }
        public SatisfactoryResourceNode SelectedResource { get; private set; }
        public SatisfactoryGameMode Mode { get; private set; }
        public int HandRecipeIndex { get; private set; }
        public bool MoveBuildingMode { get; private set; }
        public bool IsGameOver { get; private set; }
        public double ElapsedSeconds { get; private set; }
        public bool MissionComplete { get; private set; }
        public bool MissionFailed { get; private set; }
        public double HandCraftProgress { get; private set; }
        public bool IsHandCrafting { get { return _activeHandRecipe != null; } }
        public bool IsMining { get { return _activeMiningResource != null; } }
        public double MiningProgress { get; private set; }
        public string HandCraftRecipeName
        {
            get { return _activeHandRecipe == null ? string.Empty : _activeHandRecipe.Name; }
        }
        public string Status { get; private set; }
        internal double ViewLeft { get; set; }
        internal double ViewTop { get; set; }
        internal double ViewTileSize { get; set; }
        internal double WorldViewWidth { get; set; }
        internal double WorldViewHeight { get; set; }

        int IG5_WeltSpielfeld.Breite => ((IG5_WeltSpielfeld)_welt).Breite;
        int IG5_WeltSpielfeld.Hoehe => ((IG5_WeltSpielfeld)_welt).Hoehe;
        G5_Bauwerkzeug IG5_FabrikSpielInfo.AktuellesWerkzeug => _aktuellesWerkzeug;
        string IG5_FabrikSpielInfo.StatusMeldung => Status;

        IReadOnlyList<IG5_GebaeudeInfo> IG5_FabrikSpielInfo.Gebaeude
        {
            get
            {
                return _buildings
                    .Select(building => (IG5_GebaeudeInfo)new SatisfactoryGebaeudeInfo(building))
                    .ToArray();
            }
        }

        int IG5_FabrikSpielInfo.HubMenge(G5_Item item)
        {
            SatisfactoryItem? satisfactoryItem = SatisfactoryItemZuG5Item(item);
            return satisfactoryItem.HasValue ? PlayerInventory.Count(satisfactoryItem.Value) : 0;
        }

        bool IG5_WeltSpielfeld.TryGetKachelInfo(int x, int y, out IG5_KachelInfo info)
        {
            return ((IG5_WeltSpielfeld)_welt).TryGetKachelInfo(x, y, out info);
        }

        bool IG5_WeltSpielfeld.TryBildschirmZuRaster(int screenX, int screenY, out int x, out int y)
        {
            x = 0;
            y = 0;
            if (ViewTileSize <= 0 || screenX < 0 || screenY < 0 ||
                screenX >= WorldViewWidth)
            {
                return false;
            }

            x = (int)Math.Floor(ViewLeft + screenX / ViewTileSize);
            y = (int)Math.Floor(ViewTop + screenY / ViewTileSize);
            return IsInside(x, y);
        }

        bool IG5_WeltSpielfeld.TryRasterZuBildschirm(int x, int y, out Point position)
        {
            position = new Point(
                (x - ViewLeft) * ViewTileSize,
                (y - ViewTop) * ViewTileSize);
            return IsInside(x, y);
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is SatisfactoryPainter || painter is IG5_FabrikPainter;
        }

        public void Reset()
        {
            PlayerColumn = 10;
            PlayerRow = 10;
            PlayerInventory = new SatisfactoryInventory(24);
            _welt = new G5_WeltSpielfeld(Columns, Rows);
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    if (!_welt.TrySetSichtbarkeit(column, row, true))
                    {
                        throw new InvalidOperationException("Eine Startkachel konnte nicht sichtbar gemacht werden.");
                    }
                }
            }

            _resources.Clear();
            _resources.Add(new SatisfactoryResourceNode(SatisfactoryItem.IronOre, 8, 10, 300));
            _resources.Add(new SatisfactoryResourceNode(SatisfactoryItem.IronOre, 9, 10, 300));
            _resources.Add(new SatisfactoryResourceNode(SatisfactoryItem.CopperOre, 11, 12, 300));
            _resources.Add(new SatisfactoryResourceNode(SatisfactoryItem.CopperOre, 12, 12, 300));
            _resources.Add(new SatisfactoryResourceNode(SatisfactoryItem.Coal, 6, 8, 200));
            foreach (SatisfactoryResourceNode resource in _resources)
            {
                _welt.TrySetRohstoff(
                    resource.Column,
                    resource.Row,
                    ToG5Rohstoff(resource.Resource),
                    resource.Reserve);
            }

            _buildings.Clear();
            _buildings.Add(new SatisfactoryProductionBuilding(
                SatisfactoryMachineType.Smelter, 17, 7, "Schmelzofen"));
            _buildings.Add(new SatisfactoryProductionBuilding(
                SatisfactoryMachineType.Constructor, 21, 7, "Konstruktor"));
            Power = new SatisfactoryPowerNetwork();
            SelectedBuilding = null;
            SelectedResource = null;
            Mode = SatisfactoryGameMode.Cozy;
            HandRecipeIndex = 0;
            MoveBuildingMode = false;
            IsGameOver = false;
            ElapsedSeconds = 0;
            MissionComplete = false;
            MissionFailed = false;
            _activeHandRecipe = null;
            _handCraftProgress = 0;
            HandCraftProgress = 0;
            _activeMiningResource = null;
            _miningProgress = 0;
            MiningProgress = 0;
            _aktuellesWerkzeug = G5_Bauwerkzeug.Keines;
            Status = "WASD/Pfeile: bewegen | E: abbauen | C: craften | Tab: Modus";
            UpdatePower();
        }

        internal void SetWerkzeug(G5_Bauwerkzeug werkzeug)
        {
            if (!Enum.IsDefined(typeof(G5_Bauwerkzeug), werkzeug))
            {
                Status = "Unbekanntes G5-Bauwerkzeug.";
                return;
            }

            _aktuellesWerkzeug = werkzeug;
            Status = werkzeug == G5_Bauwerkzeug.Keines
                ? "Bauwerkzeug abgewählt."
                : werkzeug + " ausgewählt.";
        }

        internal void Tick(double elapsedSeconds)
        {
            if (IsGameOver || elapsedSeconds <= 0)
            {
                return;
            }

            ElapsedSeconds += elapsedSeconds;
            UpdatePower();
            foreach (SatisfactoryProductionBuilding building in _buildings)
            {
                building.Tick(elapsedSeconds);
            }

            TickHandCraft(elapsedSeconds);
            TickMining(elapsedSeconds);

            if (!MissionComplete && PlayerInventory.Count(SatisfactoryItem.IronPlate) >= MissionTargetPlates)
            {
                MissionComplete = true;
                Status = "Auftrag geschafft: 10 Eisenplatten hergestellt!";
            }

            if (Mode == SatisfactoryGameMode.Competitive &&
                !MissionComplete &&
                ElapsedSeconds >= MissionDurationSeconds)
            {
                MissionFailed = true;
                Status = "Zeit abgelaufen. Du kannst im Spiel weiterbauen.";
            }
        }

        internal void HandleKey(Key key)
        {
            switch (key)
            {
                case Key.W:
                case Key.Up:
                    MovePlayer(0, -1);
                    break;
                case Key.S:
                case Key.Down:
                    MovePlayer(0, 1);
                    break;
                case Key.A:
                case Key.Left:
                    MovePlayer(-1, 0);
                    break;
                case Key.D:
                case Key.Right:
                    MovePlayer(1, 0);
                    break;
                case Key.E:
                    MineSelectedResource();
                    break;
                case Key.C:
                    CraftByHand();
                    break;
                case Key.M:
                    if (SelectedBuilding != null)
                    {
                        MoveBuildingMode = !MoveBuildingMode;
                        Status = MoveBuildingMode
                            ? "Klicke ein freies Feld, um das Gebäude zu verschieben."
                            : "Verschieben abgebrochen.";
                    }
                    break;
                case Key.P:
                    PlacePowerPole();
                    break;
                case Key.Tab:
                    ToggleMode();
                    break;
                case Key.Escape:
                    SelectedBuilding = null;
                    SelectedResource = null;
                    MoveBuildingMode = false;
                    break;
                case Key.D1:
                case Key.NumPad1:
                    HandRecipeIndex = 0;
                    break;
                case Key.D2:
                case Key.NumPad2:
                    HandRecipeIndex = 1;
                    break;
                case Key.D3:
                case Key.NumPad3:
                    HandRecipeIndex = 2;
                    break;
                case Key.D4:
                case Key.NumPad4:
                    HandRecipeIndex = 3;
                    break;
                case Key.Q:
                    TakeBuildingOutput();
                    break;
            }
        }

        internal void HandleClick(double x, double y)
        {
            double worldWidth = WorldViewWidth;
            if (x >= worldWidth)
            {
                HandlePanelClick(y);
                return;
            }

            int column = (int)Math.Floor(ViewLeft + x / ViewTileSize);
            int row = (int)Math.Floor(ViewTop + y / ViewTileSize);
            if (!IsInside(column, row))
            {
                return;
            }

            if (MoveBuildingMode && SelectedBuilding != null)
            {
                MoveSelectedBuilding(column, row);
                return;
            }

            SatisfactoryProductionBuilding building =
                _buildings.FirstOrDefault(value => value.Contains(column, row));
            if (building != null)
            {
                StopMining();
                SelectedBuilding = building;
                SelectedResource = null;
                Status = building.Name + " ausgewaehlt.";
                return;
            }

            SatisfactoryResourceNode resource =
                _resources.FirstOrDefault(value =>
                    value.Column == column && value.Row == row && !value.IsDepleted);
            if (resource != null)
            {
                StopMining();
                SelectedResource = resource;
                SelectedBuilding = null;
                Status = ItemName(resource.Resource) + " ausgewaehlt. Mit E abbauen.";
                return;
            }

            StopMining();
            SelectedBuilding = null;
            SelectedResource = null;
            int deltaColumn = column - PlayerColumn;
            int deltaRow = row - PlayerRow;
            if (Math.Abs(deltaColumn) > Math.Abs(deltaRow))
            {
                MovePlayer(Math.Sign(deltaColumn), 0);
            }
            else if (deltaRow != 0)
            {
                MovePlayer(0, Math.Sign(deltaRow));
            }
        }

        private void HandlePanelClick(double y)
        {
            if (SelectedBuilding == null)
            {
                if (y >= 267 && y < 339)
                {
                    int recipeIndex = (int)((y - 267) / 18);
                    if (recipeIndex >= 0 && recipeIndex < SatisfactoryRecipe.HandRecipes.Count)
                    {
                        HandRecipeIndex = recipeIndex;
                        Status = "Handrezept: " + SatisfactoryRecipe.HandRecipes[recipeIndex].Name;
                    }
                }
                else if (y >= 339 && y < 357)
                {
                    CraftByHand();
                }

                return;
            }

            if (y >= 270 && y < 324)
            {
                int recipeIndex = (int)((y - 270) / 18);
                if (SelectedBuilding.SelectRecipe(recipeIndex))
                {
                    Status = "Rezept: " + SelectedBuilding.SelectedRecipe.Name;
                }
                else
                {
                    Status = "Rezeptwechsel erst nach Abschluss des laufenden Produktionszyklus.";
                }
            }
            else if (y >= 374 && y < 392)
            {
                DepositInputs();
            }
            else if (y >= 392 && y < 410)
            {
                TakeBuildingOutput();
            }
            else if (y >= 410 && y < 428)
            {
                MoveBuildingMode = true;
                Status = "Klicke ein freies Feld, um das Gebäude zu verschieben.";
            }
        }

        private void MineSelectedResource()
        {
            if (_activeMiningResource != null)
            {
                StopMining();
                Status = "Abbau pausiert.";
                return;
            }

            if (SelectedResource == null || SelectedResource.IsDepleted)
            {
                Status = "Waehle zuerst ein Ressourcenfeld aus.";
                return;
            }

            if (Distance(PlayerColumn, PlayerRow, SelectedResource.Column, SelectedResource.Row) > 1)
            {
                Status = "Du musst zum Ressourcenfeld gehen, bevor du abbaust.";
                return;
            }

            if (!PlayerInventory.CanAdd(SelectedResource.Resource, 1))
            {
                Status = "Inventar voll. Uebertrage Ressourcen an ein Gebaeude.";
                return;
            }

            _activeMiningResource = SelectedResource;
            _miningProgress = 0;
            MiningProgress = 0;
            Status = "Abbau laeuft. E pausiert.";
        }

        private void CraftByHand()
        {
            if (_activeHandRecipe != null)
            {
                Status = "Handwerk laeuft bereits.";
                return;
            }

            IReadOnlyList<SatisfactoryRecipe> recipes = SatisfactoryRecipe.HandRecipes;
            if (HandRecipeIndex < 0 || HandRecipeIndex >= recipes.Count)
            {
                return;
            }

            SatisfactoryRecipe recipe = recipes[HandRecipeIndex];
            foreach (KeyValuePair<SatisfactoryItem, int> input in recipe.Inputs)
            {
                if (PlayerInventory.Count(input.Key) < input.Value)
                {
                    Status = "Nicht genug " + ItemName(input.Key) + " fuer " + recipe.Name + ".";
                    return;
                }
            }

            foreach (KeyValuePair<SatisfactoryItem, int> output in recipe.Outputs)
            {
                if (!PlayerInventory.CanAdd(output.Key, output.Value))
                {
                    Status = "Inventar voll.";
                    return;
                }
            }

            foreach (KeyValuePair<SatisfactoryItem, int> input in recipe.Inputs)
            {
                PlayerInventory.Remove(input.Key, input.Value);
            }

            StopMining();
            _activeHandRecipe = recipe;
            _handCraftProgress = 0;
            HandCraftProgress = 0;
            Status = recipe.Name + " wird hergestellt.";
        }

        private void DepositInputs()
        {
            if (SelectedBuilding == null)
            {
                return;
            }

            int moved = 0;
            foreach (KeyValuePair<SatisfactoryItem, int> input in SelectedBuilding.SelectedRecipe.Inputs)
            {
                moved += PlayerInventory.TransferTo(
                    SelectedBuilding.Input, input.Key, input.Value);
            }

            Status = moved > 0 ? moved + " Gegenstaende uebertragen." : "Nichts uebertragen.";
        }

        private void TakeBuildingOutput()
        {
            if (SelectedBuilding == null)
            {
                return;
            }

            int moved = 0;
            foreach (KeyValuePair<SatisfactoryItem, int> item in SelectedBuilding.Output.Items)
            {
                moved += SelectedBuilding.Output.TransferTo(PlayerInventory, item.Key, item.Value);
            }

            Status = moved > 0 ? moved + " Produkte eingesammelt." : "Kein fertiges Produkt vorhanden.";
        }

        private void MoveSelectedBuilding(int column, int row)
        {
            if (!IsBuildingFootprintAvailable(
                    column,
                    row,
                    SelectedBuilding.Width,
                    SelectedBuilding.Height,
                    SelectedBuilding))
            {
                Status = "Dort kann kein Gebaeude platziert werden.";
                return;
            }

            if (!SelectedBuilding.MoveTo(column, row))
            {
                Status = "Gebaeude kann waehrend eines Produktionszyklus nicht verschoben werden.";
                return;
            }

            MoveBuildingMode = false;
            UpdatePower();
            Status = "Gebaeude verschoben.";
        }

        private void PlacePowerPole()
        {
            if (PlayerInventory.Count(SatisfactoryItem.IronRod) < 2 ||
                PlayerInventory.Count(SatisfactoryItem.Wire) < 1)
            {
                Status = "Ein Strommast braucht 2 Eisenstangen und 1 Kabel.";
                return;
            }

            if (IsCellOccupiedByBuildingOrGenerator(PlayerColumn, PlayerRow) ||
                _resources.Any(resource =>
                    !resource.IsDepleted &&
                    resource.Column == PlayerColumn &&
                    resource.Row == PlayerRow) ||
                !Power.AddPole(PlayerColumn, PlayerRow))
            {
                Status = "Hier steht bereits ein Strommast oder Gebaeude.";
                return;
            }

            PlayerInventory.Remove(SatisfactoryItem.IronRod, 2);
            PlayerInventory.Remove(SatisfactoryItem.Wire, 1);
            UpdatePower();
            Status = "Strommast gesetzt. Masten verbinden sich innerhalb ihrer Reichweite.";
        }

        private void ToggleMode()
        {
            Mode = Mode == SatisfactoryGameMode.Cozy
                ? SatisfactoryGameMode.Competitive
                : SatisfactoryGameMode.Cozy;
            ElapsedSeconds = 0;
            MissionComplete = false;
            MissionFailed = false;
            Status = Mode == SatisfactoryGameMode.Cozy
                ? "Cozy-Modus: ohne Zeitlimit."
                : "Competitive: 10 Eisenplatten in 10 Minuten. Bei Ablauf geht es weiter.";
        }

        private void MovePlayer(int deltaColumn, int deltaRow)
        {
            int column = PlayerColumn + deltaColumn;
            int row = PlayerRow + deltaRow;
            if (!IsInside(column, row) ||
                _buildings.Any(building => building.Contains(column, row)))
            {
                return;
            }

            StopMining();
            PlayerColumn = column;
            PlayerRow = row;
        }

        private bool IsBuildingFootprintAvailable(
            int column,
            int row,
            int width,
            int height,
            SatisfactoryProductionBuilding ignoredBuilding)
        {
            if (column < 0 || row < 0 ||
                column + width > Columns || row + height > Rows ||
                (PlayerColumn >= column && PlayerColumn < column + width &&
                 PlayerRow >= row && PlayerRow < row + height))
            {
                return false;
            }

            if (_buildings.Any(building =>
                    building != ignoredBuilding &&
                    column < building.Column + building.Width &&
                    building.Column < column + width &&
                    row < building.Row + building.Height &&
                    building.Row < row + height))
            {
                return false;
            }

            if (_resources.Any(resource =>
                    !resource.IsDepleted &&
                    resource.Column >= column && resource.Column < column + width &&
                    resource.Row >= row && resource.Row < row + height))
            {
                return false;
            }

            if (Power.Poles.Any(pole =>
                    pole.X >= column && pole.X < column + width &&
                    pole.Y >= row && pole.Y < row + height))
            {
                return false;
            }

            return !Power.Generators.Any(generator =>
                generator.X < column + width &&
                generator.X + 2 > column &&
                generator.Y < row + height &&
                generator.Y + 2 > row);
        }

        private bool IsCellOccupiedByBuildingOrGenerator(int column, int row)
        {
            return _buildings.Any(building => building.Contains(column, row)) ||
                Power.Generators.Any(generator =>
                    column >= generator.X && column < generator.X + 2 &&
                    row >= generator.Y && row < generator.Y + 2);
        }

        private void UpdatePower()
        {
            foreach (SatisfactoryProductionBuilding building in _buildings)
            {
                building.IsPowered = Power.IsConnected(building.Column, building.Row);
            }
        }

        private void TickHandCraft(double elapsedSeconds)
        {
            if (_activeHandRecipe == null)
            {
                return;
            }

            _handCraftProgress += elapsedSeconds;
            HandCraftProgress = Math.Min(
                1, _handCraftProgress / _activeHandRecipe.DurationSeconds);
            if (_handCraftProgress < _activeHandRecipe.DurationSeconds)
            {
                return;
            }

            foreach (KeyValuePair<SatisfactoryItem, int> output in _activeHandRecipe.Outputs)
            {
                if (!PlayerInventory.CanAdd(output.Key, output.Value))
                {
                    Status = "Inventar voll. Handwerk wartet auf freien Platz.";
                    return;
                }
            }

            foreach (KeyValuePair<SatisfactoryItem, int> output in _activeHandRecipe.Outputs)
            {
                int accepted = PlayerInventory.Add(output.Key, output.Value);
                if (accepted != output.Value)
                {
                    throw new InvalidOperationException("Das Inventar hat unerwartet keinen Platz.");
                }
            }

            Status = _activeHandRecipe.Name + " hergestellt.";
            _activeHandRecipe = null;
            _handCraftProgress = 0;
            HandCraftProgress = 0;
        }

        private void TickMining(double elapsedSeconds)
        {
            if (_activeMiningResource == null)
            {
                return;
            }

            if (_activeMiningResource.IsDepleted)
            {
                StopMining();
                Status = "Ressourcenfeld erschoepft.";
                return;
            }

            if (Distance(PlayerColumn, PlayerRow,
                _activeMiningResource.Column, _activeMiningResource.Row) > 1)
            {
                StopMining();
                Status = "Abbau beendet: zu weit vom Ressourcenfeld entfernt.";
                return;
            }

            if (!PlayerInventory.CanAdd(_activeMiningResource.Resource, 1))
            {
                StopMining();
                Status = "Inventar voll. Abbau gestoppt.";
                return;
            }

            _miningProgress += elapsedSeconds;
            MiningProgress = Math.Min(1, _miningProgress);
            if (_miningProgress < 1)
            {
                return;
            }

            if (_activeMiningResource.MineOne())
            {
                if (!_welt.TryReduceRohstoff(
                    _activeMiningResource.Column,
                    _activeMiningResource.Row,
                    1))
                {
                    throw new InvalidOperationException("Der Ressourcenbestand auf dem Spielfeld stimmt nicht mit dem Abbau überein.");
                }

                PlayerInventory.Add(_activeMiningResource.Resource, 1);
            }
            _miningProgress -= 1;
            MiningProgress = _miningProgress;
            Status = "1 " + ItemName(_activeMiningResource.Resource) + " abgebaut.";
            if (_activeMiningResource.IsDepleted)
            {
                StopMining();
                Status = "Ressourcenfeld erschoepft.";
            }
        }

        private void StopMining()
        {
            _activeMiningResource = null;
            _miningProgress = 0;
            MiningProgress = 0;
        }

        private static G5_Rohstofftyp ToG5Rohstoff(SatisfactoryItem item)
        {
            switch (item)
            {
                case SatisfactoryItem.IronOre: return G5_Rohstofftyp.Eisen;
                case SatisfactoryItem.CopperOre: return G5_Rohstofftyp.Kupfer;
                case SatisfactoryItem.Coal: return G5_Rohstofftyp.Kohle;
                default: return G5_Rohstofftyp.Keiner;
            }
        }

        private static SatisfactoryItem? SatisfactoryItemZuG5Item(G5_Item item)
        {
            switch (item)
            {
                case G5_Item.Eisenerz: return SatisfactoryItem.IronOre;
                case G5_Item.Kupfererz: return SatisfactoryItem.CopperOre;
                case G5_Item.Kohle: return SatisfactoryItem.Coal;
                case G5_Item.Eisenbarren: return SatisfactoryItem.IronIngot;
                case G5_Item.Kupferbarren: return SatisfactoryItem.CopperIngot;
                case G5_Item.Eisenplatte: return SatisfactoryItem.IronPlate;
                case G5_Item.Eisenstange: return SatisfactoryItem.IronRod;
                case G5_Item.Kupferdraht: return SatisfactoryItem.Wire;
                default: return null;
            }
        }

        internal static G5_Item ToG5Item(SatisfactoryItem item)
        {
            switch (item)
            {
                case SatisfactoryItem.IronOre: return G5_Item.Eisenerz;
                case SatisfactoryItem.CopperOre: return G5_Item.Kupfererz;
                case SatisfactoryItem.Coal: return G5_Item.Kohle;
                case SatisfactoryItem.IronIngot: return G5_Item.Eisenbarren;
                case SatisfactoryItem.CopperIngot: return G5_Item.Kupferbarren;
                case SatisfactoryItem.IronPlate: return G5_Item.Eisenplatte;
                case SatisfactoryItem.IronRod: return G5_Item.Eisenstange;
                case SatisfactoryItem.Wire: return G5_Item.Kupferdraht;
                default: throw new ArgumentOutOfRangeException(nameof(item), item, "Unbekannter Satisfactory-Gegenstand.");
            }
        }

        internal static SatisfactoryItem FromG5Item(G5_Item item)
        {
            SatisfactoryItem? satisfactoryItem = SatisfactoryItemZuG5Item(item);
            if (!satisfactoryItem.HasValue)
            {
                throw new InvalidOperationException(
                    "Der gemeinsame Gegenstand " + item + " wird von Satisfactory nicht unterstützt.");
            }

            return satisfactoryItem.Value;
        }

        private static bool IsInside(int column, int row)
        {
            return column >= 0 && column < Columns && row >= 0 && row < Rows;
        }

        private static double Distance(int firstColumn, int firstRow, int secondColumn, int secondRow)
        {
            int columnDistance = firstColumn - secondColumn;
            int rowDistance = firstRow - secondRow;
            return Math.Sqrt(columnDistance * columnDistance + rowDistance * rowDistance);
        }

        internal static string ItemName(SatisfactoryItem item)
        {
            switch (item)
            {
                case SatisfactoryItem.IronOre: return "Eisenerz";
                case SatisfactoryItem.CopperOre: return "Kupfererz";
                case SatisfactoryItem.Coal: return "Kohle";
                case SatisfactoryItem.IronIngot: return "Eisenbarren";
                case SatisfactoryItem.CopperIngot: return "Kupferbarren";
                case SatisfactoryItem.IronPlate: return "Eisenplatte";
                case SatisfactoryItem.IronRod: return "Eisenstange";
                case SatisfactoryItem.Wire: return "Kabel";
                default: return item.ToString();
            }
        }
    }

    public sealed class SatisfactoryMove : IPlayMove
    {
        public SatisfactoryMove(int playerNumber, Key key)
        {
            PlayerNumber = playerNumber;
            Key = key;
        }

        public SatisfactoryMove(int playerNumber, double x, double y)
        {
            PlayerNumber = playerNumber;
            X = x;
            Y = y;
            IsClick = true;
        }

        public int PlayerNumber { get; private set; }
        public Key Key { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }
        public bool IsClick { get; private set; }
    }

    internal sealed class SatisfactoryEmptyTurnMove : IPlayMove
    {
        public SatisfactoryEmptyTurnMove(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        public int PlayerNumber { get; private set; }
    }

    public sealed class SatisfactoryRules : IGameRules2
    {
        private readonly SatisfactoryGameField _field = new SatisfactoryGameField();
        private DateTime _lastTick;

        public string Name { get { return "G5 Satisfactory Rules"; } }
        public IGameField CurrentField { get { return _field; } }
        public bool MovesPossible { get { return !_field.IsGameOver; } }

        public void DoMove(IPlayMove move)
        {
            if (move is SatisfactoryEmptyTurnMove)
            {
                return;
            }

            IG5_FabrikAktion fabrikAktion = move as IG5_FabrikAktion;
            if (fabrikAktion != null)
            {
                if (fabrikAktion.Aktionstyp == G5_FabrikAktionstyp.WerkzeugWaehlen)
                {
                    _field.SetWerkzeug(fabrikAktion.Werkzeug);
                }
                else if (fabrikAktion.Aktionstyp == G5_FabrikAktionstyp.GebaeudePlatzieren)
                {
                    double tileSize = _field.ViewTileSize > 0
                        ? _field.ViewTileSize
                        : 40;
                    double x = (_field.ViewLeft + fabrikAktion.X) * tileSize;
                    double y = (_field.ViewTop + fabrikAktion.Y) * tileSize;
                    _field.HandleClick(x, y);
                }
                return;
            }

            SatisfactoryMove satisfactoryMove = move as SatisfactoryMove;
            if (satisfactoryMove == null || !MovesPossible)
            {
                return;
            }

            if (satisfactoryMove.PlayerNumber != 1)
            {
                return;
            }

            if (satisfactoryMove.IsClick)
            {
                _field.HandleClick(satisfactoryMove.X, satisfactoryMove.Y);
            }
            else
            {
                _field.HandleKey(satisfactoryMove.Key);
            }
        }

        public void ClearField()
        {
            _field.Reset();
        }

        public int CheckIfPLayerWon()
        {
            return -1;
        }

        public void StartedGameCall()
        {
            ClearField();
            _lastTick = DateTime.UtcNow;
        }

        public void TickGameCall()
        {
            DateTime now = DateTime.UtcNow;
            if (_lastTick == default(DateTime))
            {
                _lastTick = now;
                return;
            }

            double elapsed = Math.Min(1, (now - _lastTick).TotalSeconds);
            _lastTick = now;
            _field.Tick(elapsed);
        }
    }

    public sealed class SatisfactoryHumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber;

        public string Name { get { return "Satisfactory - Mensch (du)"; } }
        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is SatisfactoryRules;
        }

        public IGamePlayer Clone()
        {
            return new SatisfactoryHumanPlayer { _playerNumber = _playerNumber };
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (!(field is SatisfactoryGameField) || selection == null)
            {
                return null;
            }

            IKeySelection keySelection = selection as IKeySelection;
            if (keySelection != null)
            {
                return new SatisfactoryMove(PlayerNumber, keySelection.Key);
            }

            IClickSelection clickSelection = selection as IClickSelection;
            return clickSelection == null
                ? null
                : new SatisfactoryMove(PlayerNumber, clickSelection.XClickPos, clickSelection.YClickPos);
        }
    }

    public sealed class SatisfactoryEmptyTurnComputerPlayer : IComputerGamePlayer
    {
        private int _playerNumber;

        public string Name { get { return "Satisfactory - KI (leerer Zug)"; } }
        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is SatisfactoryRules;
        }

        public IGamePlayer Clone()
        {
            return new SatisfactoryEmptyTurnComputerPlayer { _playerNumber = _playerNumber };
        }

        public IPlayMove GetMove(IGameField field)
        {
            return new SatisfactoryEmptyTurnMove(PlayerNumber);
        }
    }

    public sealed class SatisfactoryPainter : IG5_FabrikPainter
    {
        private const double PanelWidth = 250;
        private const double TileSize = 40;
        private static readonly Brush BackgroundBrush = BrushFromRgb(24, 31, 35);
        private static readonly Brush GridBrush = BrushFromRgb(47, 57, 61);
        private static readonly Brush PanelBrush = BrushFromRgb(34, 43, 47);
        private static readonly Brush TextBrush = BrushFromRgb(232, 236, 232);
        private static readonly Brush AccentBrush = BrushFromRgb(64, 190, 130);

        public string Name { get { return "Satisfactory - Gruppe 5"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            TickPaintGameField(canvas, currentField);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            SatisfactoryGameField field = currentField as SatisfactoryGameField;
            if (field == null || canvas.ActualWidth <= 0 || canvas.ActualHeight <= 0)
            {
                return;
            }

            canvas.Children.Clear();
            canvas.Background = BackgroundBrush;
            double panelWidth = Math.Min(PanelWidth, canvas.ActualWidth * 0.42);
            double worldWidth = canvas.ActualWidth - panelWidth;
            double tileSize = TileSize;
            double visibleColumns = worldWidth / tileSize;
            double visibleRows = canvas.ActualHeight / tileSize;
            field.ViewLeft = Math.Max(0, Math.Min(
                SatisfactoryGameField.Columns - visibleColumns,
                field.PlayerColumn + 0.5 - visibleColumns / 2));
            field.ViewTop = Math.Max(0, Math.Min(
                SatisfactoryGameField.Rows - visibleRows,
                field.PlayerRow + 0.5 - visibleRows / 2));
            field.ViewTileSize = tileSize;
            field.WorldViewWidth = worldWidth;
            field.WorldViewHeight = canvas.ActualHeight;

            DrawWorld(canvas, field, worldWidth, tileSize);
            DrawPanel(canvas, field, worldWidth, panelWidth);
        }

        public void PaintWeltField(Canvas canvas, IG5_WeltSpielfeld currentField)
        {
            PaintGameField(canvas, currentField);
        }

        public void TickPaintWeltField(Canvas canvas, IG5_WeltSpielfeld currentField)
        {
            TickPaintGameField(canvas, currentField);
        }

        private static void DrawWorld(Canvas canvas, SatisfactoryGameField field, double worldWidth, double tileSize)
        {
            AddRectangle(canvas, BackgroundBrush, 0, 0, worldWidth, canvas.ActualHeight);

            int firstColumn = Math.Max(0, (int)field.ViewLeft);
            int lastColumn = Math.Min(SatisfactoryGameField.Columns - 1,
                (int)Math.Ceiling(field.ViewLeft + worldWidth / tileSize));
            int firstRow = Math.Max(0, (int)field.ViewTop);
            int lastRow = Math.Min(SatisfactoryGameField.Rows - 1,
                (int)Math.Ceiling(field.ViewTop + canvas.ActualHeight / tileSize));

            for (int column = firstColumn; column <= lastColumn; column++)
            {
                for (int row = firstRow; row <= lastRow; row++)
                {
                    double x = (column - field.ViewLeft) * tileSize;
                    double y = (row - field.ViewTop) * tileSize;
                    AddRectangle(canvas, GridBrush, x, y, tileSize - 1, tileSize - 1);
                }
            }

            DrawPowerNetwork(canvas, field);

            foreach (SatisfactoryResourceNode resource in field.Resources)
            {
                if (!resource.IsDepleted)
                {
                    Brush color = resource.Resource == SatisfactoryItem.IronOre
                        ? BrushFromRgb(164, 120, 83)
                        : resource.Resource == SatisfactoryItem.CopperOre
                            ? BrushFromRgb(190, 117, 65)
                            : BrushFromRgb(72, 76, 80);
                    AddCircle(canvas, color, TileX(field, resource.Column) + 8,
                        TileY(field, resource.Row) + 8, tileSize - 16);
                }
            }

            foreach (SatisfactoryProductionBuilding building in field.Buildings)
            {
                Brush color = building.IsPowered
                    ? BrushFromRgb(71, 115, 91)
                    : BrushFromRgb(102, 59, 52);
                double buildingWidth = building.Width * tileSize - 4;
                double buildingHeight = building.Height * tileSize - 4;
                AddRectangle(canvas, color, TileX(field, building.Column) + 2,
                    TileY(field, building.Row) + 2, buildingWidth, buildingHeight);
                AddText(canvas, building.Machine == SatisfactoryMachineType.Smelter ? "S" : "C",
                    TextBrush,
                    TileX(field, building.Column) + building.Width * tileSize / 2 - 7,
                    TileY(field, building.Row) + building.Height * tileSize / 2 - 9, 18);
                AddRectangle(canvas, BrushFromRgb(25, 30, 30),
                    TileX(field, building.Column) + 4, TileY(field, building.Row) + tileSize - 7,
                    building.Width * tileSize - 8, 3);
                AddRectangle(canvas, AccentBrush,
                    TileX(field, building.Column) + 4, TileY(field, building.Row) + tileSize - 7,
                    (building.Width * tileSize - 8) * building.Progress, 3);
            }

            AddCircle(canvas, BrushFromRgb(85, 172, 238),
                TileX(field, field.PlayerColumn) + 9, TileY(field, field.PlayerRow) + 9, tileSize - 18);
            AddText(canvas, field.Status, TextBrush, 12, 10, 13);
            if (field.MoveBuildingMode)
            {
                AddText(canvas, "Gebaeude verschieben: Zielfeld anklicken",
                    BrushFromRgb(255, 215, 100), 12, 31, 13);
            }
        }

        private static void DrawPowerNetwork(Canvas canvas, SatisfactoryGameField field)
        {
            foreach (Point generator in field.Power.Generators)
            {
                foreach (Point pole in field.Power.Poles)
                {
                    if (Distance(generator, pole) <= 8)
                    {
                        AddLine(canvas, BrushFromRgb(220, 193, 81),
                            TileX(field, (int)generator.X) + TileSize / 2,
                            TileY(field, (int)generator.Y) + TileSize / 2,
                            TileX(field, (int)pole.X) + TileSize / 2,
                            TileY(field, (int)pole.Y) + TileSize / 2, 2);
                    }
                }

                foreach (SatisfactoryProductionBuilding building in field.Buildings)
                {
                    if (field.Power.IsConnected(building.Column, building.Row) &&
                        Distance(generator, new Point(building.Column, building.Row)) <= 8)
                    {
                        AddLine(canvas, BrushFromRgb(220, 193, 81),
                            TileX(field, (int)generator.X) + TileSize / 2,
                            TileY(field, (int)generator.Y) + TileSize / 2,
                            TileX(field, building.Column) + building.Width * TileSize / 2,
                            TileY(field, building.Row) + building.Height * TileSize / 2, 2);
                    }
                }

                AddCircle(canvas, BrushFromRgb(230, 190, 64),
                    TileX(field, (int)generator.X) + 12,
                    TileY(field, (int)generator.Y) + 12, 16);
            }

            foreach (Point pole in field.Power.Poles)
            {
                for (int firstIndex = 0; firstIndex < field.Power.Poles.Count; firstIndex++)
                {
                    Point firstPole = field.Power.Poles[firstIndex];
                    for (int secondIndex = firstIndex + 1;
                        secondIndex < field.Power.Poles.Count;
                        secondIndex++)
                    {
                        Point secondPole = field.Power.Poles[secondIndex];
                        if (Distance(firstPole, secondPole) <= 8)
                        {
                            AddLine(canvas, BrushFromRgb(220, 193, 81),
                                TileX(field, (int)firstPole.X) + TileSize / 2,
                                TileY(field, (int)firstPole.Y) + TileSize / 2,
                                TileX(field, (int)secondPole.X) + TileSize / 2,
                                TileY(field, (int)secondPole.Y) + TileSize / 2, 2);
                        }
                    }
                }

                foreach (SatisfactoryProductionBuilding building in field.Buildings)
                {
                    if (field.Power.IsConnected(building.Column, building.Row) &&
                        Distance(pole, new Point(building.Column, building.Row)) <= 8)
                    {
                        AddLine(canvas, BrushFromRgb(220, 193, 81),
                            TileX(field, (int)pole.X) + TileSize / 2,
                            TileY(field, (int)pole.Y) + TileSize / 2,
                            TileX(field, building.Column) + building.Width * TileSize / 2,
                            TileY(field, building.Row) + building.Height * TileSize / 2, 2);
                    }
                }

                AddCircle(canvas, BrushFromRgb(246, 220, 111),
                    TileX(field, (int)pole.X) + 15,
                    TileY(field, (int)pole.Y) + 15, 10);
            }
        }

        private static void DrawPanel(Canvas canvas, SatisfactoryGameField field, double left, double width)
        {
            AddRectangle(canvas, PanelBrush, left, 0, width, canvas.ActualHeight);
            AddText(canvas, "SATISFACTORY", TextBrush, left + 14, 14, 18);
            AddText(canvas, "Modus: " + field.Mode, AccentBrush, left + 14, 42, 13);
            AddText(canvas, "Zeit: " + FormatTime(field.ElapsedSeconds), TextBrush, left + 14, 64, 13);

            if (field.Mode == SatisfactoryGameMode.Competitive)
            {
                AddText(canvas, "Auftrag: 10 Eisenplatten", TextBrush, left + 14, 86, 12);
                double remaining = Math.Max(0,
                    SatisfactoryGameField.MissionDurationSeconds - field.ElapsedSeconds);
                AddText(canvas, field.MissionComplete
                    ? "Auftrag geschafft!"
                    : field.MissionFailed
                        ? "Zeit vorbei - weiterspielen"
                        : "Restzeit: " + FormatTime(remaining), BrushFromRgb(244, 205, 91),
                    left + 14, 105, 12);
            }

            AddText(canvas, "Inventar (" + field.PlayerInventory.UsedSlots + "/"
                + field.PlayerInventory.SlotCapacity + " Slots)", TextBrush, left + 14, 132, 13);
            int inventoryY = 153;
            foreach (KeyValuePair<SatisfactoryItem, int> item in field.PlayerInventory.Items)
            {
                if (inventoryY >= 207)
                {
                    AddText(canvas, "+ weitere Gegenstaende", TextBrush, left + 14, inventoryY, 12);
                    break;
                }

                AddText(canvas, ItemLine(item.Key, item.Value), TextBrush, left + 14, inventoryY, 12);
                inventoryY += 18;
            }

            if (field.SelectedBuilding != null)
            {
                DrawBuildingPanel(canvas, field, left, 230);
            }
            else
            {
                AddText(canvas, "Handwerk (1-4 waehlen, C craften)", TextBrush,
                    left + 14, 245, 12);
                IReadOnlyList<SatisfactoryRecipe> handRecipes = SatisfactoryRecipe.HandRecipes;
                for (int i = 0; i < handRecipes.Count; i++)
                {
                    AddText(canvas, (i == field.HandRecipeIndex ? "> " : "  ")
                        + (i + 1) + ": " + handRecipes[i].Name,
                        i == field.HandRecipeIndex ? AccentBrush : TextBrush,
                        left + 14, 267 + i * 18, 11);
                }

                AddText(canvas, field.IsHandCrafting
                    ? "Crafting: " + field.HandCraftRecipeName + " " + (int)(field.HandCraftProgress * 100) + "%"
                    : "[Klick] Rezept craften / C",
                    field.IsHandCrafting ? AccentBrush : TextBrush, left + 14, 339, 11);
                if (field.IsMining)
                {
                    AddText(canvas, "Abbau: " + (int)(field.MiningProgress * 100) + "% (E pausiert)",
                        AccentBrush, left + 14, 359, 11);
                }
                AddText(canvas, "E: Erz abbauen | P: Strommast", TextBrush,
                    left + 14, canvas.ActualHeight - 42, 11);
            }
        }

        private static void DrawBuildingPanel(
            Canvas canvas,
            SatisfactoryGameField field,
            double left,
            double top)
        {
            SatisfactoryProductionBuilding building = field.SelectedBuilding;
            AddText(canvas, building.Name + (building.IsPowered ? " (Strom)" : " (ohne Strom)"),
                building.IsPowered ? AccentBrush : BrushFromRgb(242, 120, 91),
                left + 14, top, 13);
            AddText(canvas, "Rezept anklicken:", TextBrush, left + 14, top + 22, 11);
            int recipeY = 270;
            for (int i = 0; i < building.Recipes.Count; i++)
            {
                bool selected = building.SelectedRecipe == building.Recipes[i];
                AddText(canvas, (selected ? "> " : "  ") + building.Recipes[i].Name,
                    selected ? AccentBrush : TextBrush, left + 14, recipeY + i * 18, 11);
            }

            AddText(canvas, "Einsatz: " + InventorySummary(building.Input), TextBrush,
                left + 14, 332, 10);
            AddText(canvas, "Produkt: " + InventorySummary(building.Output), TextBrush,
                left + 14, 350, 10);
            AddText(canvas, "[Klick] Einsatz uebertragen", AccentBrush,
                left + 14, 374, 10);
            AddText(canvas, "[Klick] Produkte einsammeln / Q", AccentBrush,
                left + 14, 392, 10);
            AddText(canvas, "[Klick] Gebaeude verschieben / M", TextBrush,
                left + 14, 410, 10);
            AddText(canvas, "P: Strommast setzen (2 Stangen + Kabel)", TextBrush,
                left + 14, canvas.ActualHeight - 42, 10);
        }

        private static double TileX(SatisfactoryGameField field, int column)
        {
            return (column - field.ViewLeft) * TileSize;
        }

        private static double TileY(SatisfactoryGameField field, int row)
        {
            return (row - field.ViewTop) * TileSize;
        }

        private static string ItemLine(SatisfactoryItem item, int count)
        {
            return ItemName(item) + ": " + count;
        }

        private static string InventorySummary(SatisfactoryInventory inventory)
        {
            return inventory.Items.Count == 0
                ? "leer"
                : string.Join(", ", inventory.Items.Select(item => ItemName(item.Key) + " " + item.Value));
        }

        private static string ItemName(SatisfactoryItem item)
        {
            switch (item)
            {
                case SatisfactoryItem.IronOre: return "Eisenerz";
                case SatisfactoryItem.CopperOre: return "Kupfererz";
                case SatisfactoryItem.Coal: return "Kohle";
                case SatisfactoryItem.IronIngot: return "Eisenbarren";
                case SatisfactoryItem.CopperIngot: return "Kupferbarren";
                case SatisfactoryItem.IronPlate: return "Eisenplatten";
                case SatisfactoryItem.IronRod: return "Eisenstangen";
                case SatisfactoryItem.Wire: return "Kabel";
                default: return item.ToString();
            }
        }

        private static string FormatTime(double seconds)
        {
            TimeSpan time = TimeSpan.FromSeconds(Math.Max(0, seconds));
            return ((int)time.TotalMinutes).ToString("00") + ":" + time.Seconds.ToString("00");
        }

        private static double Distance(Point first, Point second)
        {
            double dx = first.X - second.X;
            double dy = first.Y - second.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private static Brush BrushFromRgb(byte red, byte green, byte blue)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromRgb(red, green, blue));
            brush.Freeze();
            return brush;
        }

        private static void AddRectangle(Canvas canvas, Brush brush, double left, double top, double width, double height)
        {
            Rectangle rectangle = new Rectangle
            {
                Width = Math.Max(0, width),
                Height = Math.Max(0, height),
                Fill = brush,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(rectangle, left);
            Canvas.SetTop(rectangle, top);
            canvas.Children.Add(rectangle);
        }

        private static void AddCircle(Canvas canvas, Brush brush, double left, double top, double size)
        {
            Ellipse ellipse = new Ellipse
            {
                Width = size,
                Height = size,
                Fill = brush,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(ellipse, left);
            Canvas.SetTop(ellipse, top);
            canvas.Children.Add(ellipse);
        }

        private static void AddLine(
            Canvas canvas,
            Brush brush,
            double x1,
            double y1,
            double x2,
            double y2,
            double thickness)
        {
            Line line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = brush,
                StrokeThickness = thickness,
                IsHitTestVisible = false
            };
            canvas.Children.Add(line);
        }

        private static void AddText(Canvas canvas, string text, Brush brush, double left, double top, double fontSize)
        {
            TextBlock block = new TextBlock
            {
                Text = text,
                Foreground = brush,
                FontSize = fontSize,
                TextWrapping = TextWrapping.Wrap,
                Width = Math.Max(50, Math.Min(PanelWidth - 24, canvas.ActualWidth - left - 8)),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(block, left);
            Canvas.SetTop(block, top);
            canvas.Children.Add(block);
        }
    }

    internal sealed class SatisfactoryGebaeudeInfo : IG5_GebaeudeInfo
    {
        private readonly SatisfactoryProductionBuilding _gebaeude;

        internal SatisfactoryGebaeudeInfo(SatisfactoryProductionBuilding gebaeude)
        {
            _gebaeude = gebaeude ?? throw new ArgumentNullException(nameof(gebaeude));
        }

        public G5_FabrikGebaeudeTyp Typ =>
            _gebaeude.Machine == SatisfactoryMachineType.Smelter
                ? G5_FabrikGebaeudeTyp.Schmelze
                : G5_FabrikGebaeudeTyp.Foerderer;
        public string Name => _gebaeude.Name;
        public int X => _gebaeude.Column;
        public int Y => _gebaeude.Row;
        public int Breite => _gebaeude.Width;
        public int Hoehe => _gebaeude.Height;
        public int Level => _gebaeude.FrameworkBuilding.Level;
        public G5_BuildingStatus Status => _gebaeude.Status;
        public bool IstMitStromVersorgt => _gebaeude.IsPowered;
    }

    internal static class SatisfactoryRegistration
    {
        [ModuleInitializer]
        internal static void RegisterGame()
        {
            OOPGamesManager.Singleton.RegisterRules(new SatisfactoryRules());
            OOPGamesManager.Singleton.RegisterPainter(new SatisfactoryPainter());
            OOPGamesManager.Singleton.RegisterPlayer(new SatisfactoryHumanPlayer());
            OOPGamesManager.Singleton.RegisterPlayer(new SatisfactoryEmptyTurnComputerPlayer());
        }
    }
}
