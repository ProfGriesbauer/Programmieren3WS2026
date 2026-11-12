using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OOPGames
{
    public enum G5_Item
    {
        Holz,
        Eisenerz,
        Kupfererz,
        Kohle,
        Kalkstein,
        Quarz,
        Schwefel,
        Uran,
        Wasser,
        Gas,
        Oel,
        Eisenbarren,
        Kupferbarren,
        Eisenplatte,
        Eisenstange,
        Kupferdraht,
        Schraube,
        Rotor
    }

    public enum G5_BuildingStatus
    {
        Idle,
        Working,
        WaitingForRecipe,
        WaitingForInput,
        WaitingForPower,
        OutputBlocked,
        DepositDepleted,
        WaitingForConnection
    }

    public enum G5_RecipeType
    {
        Smelting,
        Construction,
        Assembly
    }

    public readonly record struct G5_GridPosition(int X, int Y)
    {
        public int ManhattanDistanceTo(G5_GridPosition other)
        {
            return (int)(Math.Abs((long)X - other.X) + Math.Abs((long)Y - other.Y));
        }
    }

    public interface G5_IItemEndpoint
    {
        G5_GridPosition Position { get; }
        IReadOnlyDictionary<G5_Item, int> TransferableContents { get; }
        bool CanReceive(G5_Item item, int amount);
        bool TryReceive(G5_Item item, int amount);
        bool TryExtract(G5_Item item, int amount);
        bool TryRestoreExtracted(G5_Item item, int amount);
        int CountOf(G5_Item item);
    }

    public interface G5_IPowerConsumer
    {
        G5_GridPosition Position { get; }
        int PowerDemandPerTick { get; }
        int PowerPriority { get; }
        bool IsPowered { get; }
        void SetPowered(bool isPowered);
    }

    public sealed class G5_Recipe
    {
        private readonly IReadOnlyDictionary<G5_Item, int> _ingredients;

        public G5_Recipe(
            string name,
            G5_RecipeType type,
            IEnumerable<KeyValuePair<G5_Item, int>> ingredients,
            G5_Item output,
            int outputAmount,
            int cycleTicks,
            int powerDemandPerTick)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("A recipe must have a name.", nameof(name));
            }

            if (ingredients == null)
            {
                throw new ArgumentNullException(nameof(ingredients));
            }

            Dictionary<G5_Item, int> ingredientMap = new Dictionary<G5_Item, int>();
            foreach (KeyValuePair<G5_Item, int> ingredient in ingredients)
            {
                if (ingredient.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(ingredients), "Ingredient amounts must be positive.");
                }

                ingredientMap.Add(ingredient.Key, ingredient.Value);
            }

            if (ingredientMap.Count == 0)
            {
                throw new ArgumentException("A recipe must require at least one ingredient.", nameof(ingredients));
            }

            if (outputAmount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(outputAmount));
            }

            if (cycleTicks <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cycleTicks));
            }

            if (powerDemandPerTick < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(powerDemandPerTick));
            }

            Name = name;
            Type = type;
            _ingredients = new ReadOnlyDictionary<G5_Item, int>(ingredientMap);
            Output = output;
            OutputAmount = outputAmount;
            CycleTicks = cycleTicks;
            PowerDemandPerTick = powerDemandPerTick;
        }

        public string Name { get; }
        public G5_RecipeType Type { get; }
        public IReadOnlyDictionary<G5_Item, int> Ingredients => _ingredients;
        public G5_Item Output { get; }
        public int OutputAmount { get; }
        public int CycleTicks { get; }
        public int PowerDemandPerTick { get; }
    }

    public sealed class G5_BuildingUpgrade
    {
        public G5_BuildingUpgrade(
            int productionTicksReduction = 0,
            int inputCapacityIncrease = 0,
            int outputCapacityIncrease = 0,
            int powerDemandReduction = 0,
            int outputAmountIncrease = 0,
            int storageCapacityIncrease = 0,
            int powerOutputIncrease = 0,
            int transportCapacityIncrease = 0)
        {
            ProductionTicksReduction = NonNegative(productionTicksReduction, nameof(productionTicksReduction));
            InputCapacityIncrease = NonNegative(inputCapacityIncrease, nameof(inputCapacityIncrease));
            OutputCapacityIncrease = NonNegative(outputCapacityIncrease, nameof(outputCapacityIncrease));
            PowerDemandReduction = NonNegative(powerDemandReduction, nameof(powerDemandReduction));
            OutputAmountIncrease = NonNegative(outputAmountIncrease, nameof(outputAmountIncrease));
            StorageCapacityIncrease = NonNegative(storageCapacityIncrease, nameof(storageCapacityIncrease));
            PowerOutputIncrease = NonNegative(powerOutputIncrease, nameof(powerOutputIncrease));
            TransportCapacityIncrease = NonNegative(transportCapacityIncrease, nameof(transportCapacityIncrease));
        }

        public int ProductionTicksReduction { get; }
        public int InputCapacityIncrease { get; }
        public int OutputCapacityIncrease { get; }
        public int PowerDemandReduction { get; }
        public int OutputAmountIncrease { get; }
        public int StorageCapacityIncrease { get; }
        public int PowerOutputIncrease { get; }
        public int TransportCapacityIncrease { get; }

        private static int NonNegative(int value, string parameterName)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }

            return value;
        }
    }

    internal sealed class G5_ItemBuffer
    {
        private readonly Dictionary<G5_Item, int> _items = new Dictionary<G5_Item, int>();
        private int _capacity;

        internal G5_ItemBuffer(int capacity)
        {
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity));
            }

            _capacity = capacity;
        }

        internal int Capacity => _capacity;
        internal int UsedCapacity => _items.Values.Sum();
        internal int FreeCapacity => _capacity - UsedCapacity;

        internal IReadOnlyDictionary<G5_Item, int> Contents =>
            new ReadOnlyDictionary<G5_Item, int>(new Dictionary<G5_Item, int>(_items));

        internal int CountOf(G5_Item item)
        {
            return _items.TryGetValue(item, out int amount) ? amount : 0;
        }

        internal bool CanAdd(G5_Item item, int amount)
        {
            return amount > 0 && amount <= FreeCapacity;
        }

        internal bool TryAdd(G5_Item item, int amount)
        {
            if (!CanAdd(item, amount))
            {
                return false;
            }

            _items[item] = checked(CountOf(item) + amount);
            return true;
        }

        internal bool CanRemove(G5_Item item, int amount)
        {
            return amount > 0 && CountOf(item) >= amount;
        }

        internal bool TryRemove(G5_Item item, int amount)
        {
            if (!CanRemove(item, amount))
            {
                return false;
            }

            int remaining = CountOf(item) - amount;
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

        internal bool TryConsume(IReadOnlyDictionary<G5_Item, int> ingredients)
        {
            if (ingredients.Any(ingredient => !CanRemove(ingredient.Key, ingredient.Value)))
            {
                return false;
            }

            foreach (KeyValuePair<G5_Item, int> ingredient in ingredients)
            {
                TryRemove(ingredient.Key, ingredient.Value);
            }

            return true;
        }

        internal void IncreaseCapacity(int amount)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            _capacity = checked(_capacity + amount);
        }
    }

    public abstract class G5_Building : G5_IItemEndpoint, G5_IPowerConsumer
    {
        private int _powerDemandPerTick;

        protected G5_Building(
            G5_GridPosition position,
            int width,
            int height,
            int powerDemandPerTick = 0,
            int powerPriority = 0)
        {
            if (width <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(width));
            }

            if (height <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(height));
            }

            if (powerDemandPerTick < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(powerDemandPerTick));
            }

            Position = position;
            Width = width;
            Height = height;
            PowerPriority = powerPriority;
            _powerDemandPerTick = powerDemandPerTick;
            IsPowered = powerDemandPerTick == 0;
        }

        public abstract string Name { get; }
        public G5_GridPosition Position { get; }
        public int Width { get; }
        public int Height { get; }
        public int Level { get; private set; } = 1;
        public G5_BuildingStatus Status { get; protected set; } = G5_BuildingStatus.Idle;
        public virtual int PowerDemandPerTick => _powerDemandPerTick;
        public int PowerPriority { get; }
        public bool IsPowered { get; private set; }
        public virtual IReadOnlyDictionary<G5_Item, int> TransferableContents =>
            new ReadOnlyDictionary<G5_Item, int>(new Dictionary<G5_Item, int>());

        public virtual bool TryReceive(G5_Item item, int amount)
        {
            return false;
        }

        public virtual bool CanReceive(G5_Item item, int amount)
        {
            return false;
        }

        public virtual bool TryExtract(G5_Item item, int amount)
        {
            return false;
        }

        public virtual bool TryRestoreExtracted(G5_Item item, int amount)
        {
            return false;
        }

        public virtual int CountOf(G5_Item item)
        {
            return 0;
        }

        public void Upgrade(G5_BuildingUpgrade upgrade)
        {
            if (upgrade == null)
            {
                throw new ArgumentNullException(nameof(upgrade));
            }

            int nextLevel = checked(Level + 1);
            ApplyUpgrade(upgrade);
            Level = nextLevel;
        }

        public void SetPowered(bool isPowered)
        {
            IsPowered = PowerDemandPerTick == 0 || isPowered;
        }

        protected virtual void ApplyUpgrade(G5_BuildingUpgrade upgrade)
        {
            ReducePowerDemand(upgrade.PowerDemandReduction);
        }

        protected void ReducePowerDemand(int amount)
        {
            _powerDemandPerTick = Math.Max(0, _powerDemandPerTick - amount);
            if (_powerDemandPerTick == 0)
            {
                IsPowered = true;
            }
        }

        protected void SetStatus(G5_BuildingStatus status)
        {
            Status = status;
        }
    }

    public sealed class G5_ResourceDeposit
    {
        public G5_ResourceDeposit(G5_Item item, long? amount)
        {
            if (!IsMineableSolid(item))
            {
                throw new ArgumentException("This resource requires a different extraction method.", nameof(item));
            }

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            Item = item;
            RemainingAmount = amount;
        }

        public G5_Item Item { get; }
        public long? RemainingAmount { get; private set; }

        internal int ExtractUpTo(int requestedAmount)
        {
            if (requestedAmount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(requestedAmount));
            }

            if (RemainingAmount == 0)
            {
                return 0;
            }

            if (RemainingAmount == null)
            {
                return requestedAmount;
            }

            int extracted = (int)Math.Min(RemainingAmount.Value, requestedAmount);
            RemainingAmount -= extracted;
            return extracted;
        }

        private static bool IsMineableSolid(G5_Item item)
        {
            return item == G5_Item.Eisenerz
                || item == G5_Item.Kupfererz
                || item == G5_Item.Kohle
                || item == G5_Item.Kalkstein
                || item == G5_Item.Quarz
                || item == G5_Item.Schwefel
                || item == G5_Item.Uran;
        }
    }

    public sealed class G5_Miner : G5_Building
    {
        private readonly G5_ResourceDeposit _deposit;
        private readonly G5_ItemBuffer _output;
        private int _cycleTicks;
        private int _ticksRemaining;
        private int _amountPerCycle;

        public G5_Miner(
            G5_GridPosition position,
            G5_ResourceDeposit deposit,
            int outputCapacity,
            int cycleTicks,
            int amountPerCycle,
            int powerDemandPerTick)
            : base(position, 2, 2, powerDemandPerTick)
        {
            _deposit = deposit ?? throw new ArgumentNullException(nameof(deposit));
            _output = new G5_ItemBuffer(outputCapacity);
            _cycleTicks = Positive(cycleTicks, nameof(cycleTicks));
            _amountPerCycle = Positive(amountPerCycle, nameof(amountPerCycle));
            if (outputCapacity < amountPerCycle)
            {
                throw new ArgumentException("Output capacity must fit at least one mining cycle.", nameof(outputCapacity));
            }
        }

        public override string Name => "Miner";
        public G5_Item Resource => _deposit.Item;
        public long? RemainingDeposit => _deposit.RemainingAmount;
        public IReadOnlyDictionary<G5_Item, int> OutputContents => _output.Contents;
        public override IReadOnlyDictionary<G5_Item, int> TransferableContents => _output.Contents;

        public void Tick()
        {
            if (!IsPowered)
            {
                SetStatus(G5_BuildingStatus.WaitingForPower);
                return;
            }

            if (_deposit.RemainingAmount == 0)
            {
                SetStatus(G5_BuildingStatus.DepositDepleted);
                return;
            }

            if (_output.FreeCapacity < _amountPerCycle)
            {
                SetStatus(G5_BuildingStatus.OutputBlocked);
                return;
            }

            _ticksRemaining++;
            if (_ticksRemaining < _cycleTicks)
            {
                SetStatus(G5_BuildingStatus.Working);
                return;
            }

            _ticksRemaining = 0;
            int amount = _deposit.ExtractUpTo(_amountPerCycle);
            if (amount == 0)
            {
                SetStatus(G5_BuildingStatus.DepositDepleted);
                return;
            }

            if (!_output.TryAdd(_deposit.Item, amount))
            {
                throw new InvalidOperationException("The miner output buffer changed during a mining tick.");
            }

            SetStatus(G5_BuildingStatus.Working);
        }

        public override bool TryExtract(G5_Item item, int amount)
        {
            return _output.TryRemove(item, amount);
        }

        public override bool TryRestoreExtracted(G5_Item item, int amount)
        {
            return _output.TryAdd(item, amount);
        }

        public override int CountOf(G5_Item item)
        {
            return _output.CountOf(item);
        }

        protected override void ApplyUpgrade(G5_BuildingUpgrade upgrade)
        {
            int nextAmountPerCycle = checked(_amountPerCycle + upgrade.OutputAmountIncrease);
            int nextOutputCapacity = checked(_output.Capacity + upgrade.OutputCapacityIncrease);
            if (nextOutputCapacity < nextAmountPerCycle)
            {
                throw new ArgumentException(
                    "The upgraded output buffer must fit at least one mining cycle.",
                    nameof(upgrade));
            }

            base.ApplyUpgrade(upgrade);
            _cycleTicks = Math.Max(1, _cycleTicks - upgrade.ProductionTicksReduction);
            _amountPerCycle = nextAmountPerCycle;
            _output.IncreaseCapacity(upgrade.OutputCapacityIncrease);
        }

        private static int Positive(int value, string parameterName)
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(parameterName);
            }

            return value;
        }
    }

    public abstract class G5_ProductionBuilding : G5_Building
    {
        private readonly IReadOnlyList<G5_Recipe> _recipes;
        private readonly G5_ItemBuffer _input;
        private readonly G5_ItemBuffer _output;
        private int _productionTicksReduction;
        private int _powerDemandReduction;
        private int _ticksRemaining;
        private G5_Recipe _selectedRecipe;

        protected G5_ProductionBuilding(
            G5_GridPosition position,
            string name,
            G5_RecipeType recipeType,
            IEnumerable<G5_Recipe> recipes,
            int inputCapacity,
            int outputCapacity,
            int powerPriority = 0)
            : base(position, 2, 2, 0, powerPriority)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("A building must have a name.", nameof(name));
            }

            if (recipes == null)
            {
                throw new ArgumentNullException(nameof(recipes));
            }

            List<G5_Recipe> recipeList = recipes.ToList();
            if (recipeList.Count == 0 || recipeList.Any(recipe => recipe == null || recipe.Type != recipeType))
            {
                throw new ArgumentException("Provide at least one recipe of the supported type.", nameof(recipes));
            }

            _recipes = recipeList.AsReadOnly();
            _input = new G5_ItemBuffer(inputCapacity);
            _output = new G5_ItemBuffer(outputCapacity);
            Name = name;
        }

        public override string Name { get; }
        public IReadOnlyList<G5_Recipe> Recipes => _recipes;
        public G5_Recipe SelectedRecipe => _selectedRecipe;
        public IReadOnlyDictionary<G5_Item, int> InputContents => _input.Contents;
        public IReadOnlyDictionary<G5_Item, int> OutputContents => _output.Contents;
        public override IReadOnlyDictionary<G5_Item, int> TransferableContents => _output.Contents;
        public override int PowerDemandPerTick =>
            _selectedRecipe == null ? 0 : Math.Max(0, _selectedRecipe.PowerDemandPerTick - _powerDemandReduction);

        public bool SelectRecipe(string recipeName)
        {
            if (string.IsNullOrWhiteSpace(recipeName))
            {
                throw new ArgumentException("A recipe name is required.", nameof(recipeName));
            }

            G5_Recipe recipe = _recipes.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, recipeName, StringComparison.Ordinal));
            if (recipe == null)
            {
                return false;
            }

            _selectedRecipe = recipe;
            _ticksRemaining = 0;
            SetStatus(G5_BuildingStatus.Idle);
            SetPowered(PowerDemandPerTick == 0);
            return true;
        }

        public bool Tick()
        {
            if (_selectedRecipe == null)
            {
                SetStatus(G5_BuildingStatus.WaitingForRecipe);
                return false;
            }

            if (!IsPowered)
            {
                SetStatus(G5_BuildingStatus.WaitingForPower);
                return false;
            }

            if (_ticksRemaining == 0)
            {
                if (_output.FreeCapacity < _selectedRecipe.OutputAmount)
                {
                    SetStatus(G5_BuildingStatus.OutputBlocked);
                    return false;
                }

                if (!_input.TryConsume(_selectedRecipe.Ingredients))
                {
                    SetStatus(G5_BuildingStatus.WaitingForInput);
                    return false;
                }

                _ticksRemaining = Math.Max(1, _selectedRecipe.CycleTicks - _productionTicksReduction);
            }

            _ticksRemaining--;
            if (_ticksRemaining > 0)
            {
                SetStatus(G5_BuildingStatus.Working);
                return false;
            }

            if (!_output.TryAdd(_selectedRecipe.Output, _selectedRecipe.OutputAmount))
            {
                throw new InvalidOperationException("The production output buffer changed during a production tick.");
            }

            SetStatus(G5_BuildingStatus.Working);
            return true;
        }

        public override bool TryReceive(G5_Item item, int amount)
        {
            return _input.TryAdd(item, amount);
        }

        public override bool CanReceive(G5_Item item, int amount)
        {
            return _input.CanAdd(item, amount);
        }

        public override bool TryExtract(G5_Item item, int amount)
        {
            return _output.TryRemove(item, amount);
        }

        public override bool TryRestoreExtracted(G5_Item item, int amount)
        {
            return _output.TryAdd(item, amount);
        }

        public override int CountOf(G5_Item item)
        {
            return checked(_input.CountOf(item) + _output.CountOf(item));
        }

        protected override void ApplyUpgrade(G5_BuildingUpgrade upgrade)
        {
            _productionTicksReduction = checked(_productionTicksReduction + upgrade.ProductionTicksReduction);
            _powerDemandReduction = checked(_powerDemandReduction + upgrade.PowerDemandReduction);
            _input.IncreaseCapacity(upgrade.InputCapacityIncrease);
            _output.IncreaseCapacity(upgrade.OutputCapacityIncrease);
        }
    }

    public sealed class G5_Smelter : G5_ProductionBuilding
    {
        public G5_Smelter(
            G5_GridPosition position,
            IEnumerable<G5_Recipe> recipes,
            int inputCapacity,
            int outputCapacity,
            int powerPriority = 0)
            : base(position, "Schmelze", G5_RecipeType.Smelting, recipes, inputCapacity, outputCapacity, powerPriority)
        {
        }
    }

    public sealed class G5_Constructor : G5_ProductionBuilding
    {
        public G5_Constructor(
            G5_GridPosition position,
            IEnumerable<G5_Recipe> recipes,
            int inputCapacity,
            int outputCapacity,
            int powerPriority = 0)
            : base(position, "Konstruktor", G5_RecipeType.Construction, recipes, inputCapacity, outputCapacity, powerPriority)
        {
        }
    }

    public sealed class G5_Assembler : G5_ProductionBuilding
    {
        public G5_Assembler(
            G5_GridPosition position,
            IEnumerable<G5_Recipe> recipes,
            int inputCapacity,
            int outputCapacity,
            int powerPriority = 0)
            : base(position, "Assembler", G5_RecipeType.Assembly, recipes, inputCapacity, outputCapacity, powerPriority)
        {
        }
    }

    public sealed class G5_Storage : G5_Building
    {
        private readonly G5_ItemBuffer _items;

        public G5_Storage(G5_GridPosition position, int capacity)
            : base(position, 2, 2)
        {
            _items = new G5_ItemBuffer(capacity);
        }

        public override string Name => "Lager";
        public int Capacity => _items.Capacity;
        public int UsedCapacity => _items.UsedCapacity;
        public IReadOnlyDictionary<G5_Item, int> Contents => _items.Contents;
        public override IReadOnlyDictionary<G5_Item, int> TransferableContents => _items.Contents;

        public override bool TryReceive(G5_Item item, int amount)
        {
            return _items.TryAdd(item, amount);
        }

        public override bool CanReceive(G5_Item item, int amount)
        {
            return _items.CanAdd(item, amount);
        }

        public override bool TryExtract(G5_Item item, int amount)
        {
            return _items.TryRemove(item, amount);
        }

        public override bool TryRestoreExtracted(G5_Item item, int amount)
        {
            return _items.TryAdd(item, amount);
        }

        public override int CountOf(G5_Item item)
        {
            return _items.CountOf(item);
        }

        protected override void ApplyUpgrade(G5_BuildingUpgrade upgrade)
        {
            _items.IncreaseCapacity(upgrade.StorageCapacityIncrease);
        }
    }

    public sealed class G5_Conveyor : G5_Building
    {
        private readonly G5_ItemBuffer _items;
        private int _itemsPerTick;
        private G5_IItemEndpoint _source;
        private G5_IItemEndpoint _destination;

        public G5_Conveyor(
            G5_GridPosition position,
            int bufferCapacity,
            int itemsPerTick,
            int powerDemandPerTick = 0)
            : base(position, 1, 1, powerDemandPerTick)
        {
            _items = new G5_ItemBuffer(bufferCapacity);
            if (itemsPerTick <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(itemsPerTick));
            }

            _itemsPerTick = itemsPerTick;
        }

        public override string Name => "Laufband";
        public IReadOnlyDictionary<G5_Item, int> Contents => _items.Contents;
        public override IReadOnlyDictionary<G5_Item, int> TransferableContents => _items.Contents;

        public void Connect(G5_IItemEndpoint source, G5_IItemEndpoint destination)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (ReferenceEquals(source, this) || ReferenceEquals(destination, this) ||
                ReferenceEquals(source, destination))
            {
                throw new ArgumentException("A conveyor needs distinct source and destination endpoints.");
            }

            _source = source;
            _destination = destination;
        }

        public void Tick()
        {
            if (!IsPowered)
            {
                SetStatus(G5_BuildingStatus.WaitingForPower);
                return;
            }

            if (_source == null || _destination == null)
            {
                SetStatus(G5_BuildingStatus.WaitingForConnection);
                return;
            }

            int moved = 0;
            foreach (KeyValuePair<G5_Item, int> stack in _items.Contents)
            {
                int amount = Math.Min(stack.Value, _itemsPerTick - moved);
                if (amount <= 0)
                {
                    break;
                }

                int transferred = G5_ItemTransfer.Transfer(this, _destination, stack.Key, amount);
                moved += transferred;
                if (moved == _itemsPerTick)
                {
                    break;
                }
            }

            int remainingThroughput = _itemsPerTick - moved;
            if (remainingThroughput > 0 && _items.FreeCapacity > 0)
            {
                foreach (KeyValuePair<G5_Item, int> stack in _source.TransferableContents)
                {
                    int amount = Math.Min(Math.Min(stack.Value, remainingThroughput), _items.FreeCapacity);
                    if (amount <= 0)
                    {
                        break;
                    }

                    int transferred = G5_ItemTransfer.Transfer(_source, this, stack.Key, amount);
                    moved += transferred;
                    remainingThroughput -= transferred;
                    if (remainingThroughput == 0 || _items.FreeCapacity == 0)
                    {
                        break;
                    }
                }
            }

            SetStatus(moved > 0 ? G5_BuildingStatus.Working : G5_BuildingStatus.Idle);
        }

        public override bool TryReceive(G5_Item item, int amount)
        {
            return _items.TryAdd(item, amount);
        }

        public override bool CanReceive(G5_Item item, int amount)
        {
            return _items.CanAdd(item, amount);
        }

        public override bool TryExtract(G5_Item item, int amount)
        {
            return _items.TryRemove(item, amount);
        }

        public override bool TryRestoreExtracted(G5_Item item, int amount)
        {
            return _items.TryAdd(item, amount);
        }

        public override int CountOf(G5_Item item)
        {
            return _items.CountOf(item);
        }

        protected override void ApplyUpgrade(G5_BuildingUpgrade upgrade)
        {
            base.ApplyUpgrade(upgrade);
            _itemsPerTick = checked(_itemsPerTick + upgrade.TransportCapacityIncrease);
            _items.IncreaseCapacity(upgrade.OutputCapacityIncrease);
        }
    }

    public sealed class G5_BurningGenerator : G5_Building
    {
        private readonly G5_ItemBuffer _fuel;
        private readonly IReadOnlyDictionary<G5_Item, int> _energyPerFuel;
        private int _maxOutputPerTick;
        private int _storedEnergy;

        public G5_BurningGenerator(
            G5_GridPosition position,
            int fuelCapacity,
            int maxOutputPerTick,
            IEnumerable<KeyValuePair<G5_Item, int>> energyPerFuel)
            : base(position, 2, 2)
        {
            _fuel = new G5_ItemBuffer(fuelCapacity);
            if (maxOutputPerTick <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxOutputPerTick));
            }

            if (energyPerFuel == null)
            {
                throw new ArgumentNullException(nameof(energyPerFuel));
            }

            Dictionary<G5_Item, int> fuelValues = new Dictionary<G5_Item, int>();
            foreach (KeyValuePair<G5_Item, int> fuel in energyPerFuel)
            {
                if (fuel.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(energyPerFuel), "Fuel energy values must be positive.");
                }

                fuelValues.Add(fuel.Key, fuel.Value);
            }

            if (fuelValues.Count == 0)
            {
                throw new ArgumentException("At least one fuel type is required.", nameof(energyPerFuel));
            }

            _energyPerFuel = new ReadOnlyDictionary<G5_Item, int>(fuelValues);
            _maxOutputPerTick = maxOutputPerTick;
        }

        public override string Name => "Verbrennungsgenerator";
        public int MaxOutputPerTick => _maxOutputPerTick;
        public IReadOnlyDictionary<G5_Item, int> FuelContents => _fuel.Contents;

        public bool TryAddFuel(G5_Item item, int amount)
        {
            return _energyPerFuel.ContainsKey(item) && _fuel.TryAdd(item, amount);
        }

        internal int AvailablePower
        {
            get
            {
                long available = _storedEnergy;
                foreach (KeyValuePair<G5_Item, int> fuel in _energyPerFuel)
                {
                    available += (long)_fuel.CountOf(fuel.Key) * fuel.Value;
                }

                return (int)Math.Min(_maxOutputPerTick, available);
            }
        }

        internal int SupplyPower(int requested)
        {
            if (requested < 0 || requested > _maxOutputPerTick)
            {
                throw new ArgumentOutOfRangeException(nameof(requested));
            }

            int supplied = Math.Min(requested, AvailablePower);
            while (_storedEnergy < supplied)
            {
                KeyValuePair<G5_Item, int>? availableFuel = _energyPerFuel
                    .FirstOrDefault(fuel => _fuel.CountOf(fuel.Key) > 0);
                if (availableFuel == null || availableFuel.Value.Value <= 0)
                {
                    throw new InvalidOperationException("Generator fuel changed while distributing power.");
                }

                KeyValuePair<G5_Item, int> selectedFuel = availableFuel.Value;
                if (!_fuel.TryRemove(selectedFuel.Key, 1))
                {
                    throw new InvalidOperationException("Generator fuel changed while distributing power.");
                }

                _storedEnergy = checked(_storedEnergy + selectedFuel.Value);
            }

            _storedEnergy -= supplied;
            return supplied;
        }

        protected override void ApplyUpgrade(G5_BuildingUpgrade upgrade)
        {
            _maxOutputPerTick = checked(_maxOutputPerTick + upgrade.PowerOutputIncrease);
        }
    }

    public sealed class G5_PowerPole : G5_Building
    {
        public G5_PowerPole(G5_GridPosition position, int connectionRange)
            : base(position, 1, 1)
        {
            if (connectionRange < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(connectionRange));
            }

            ConnectionRange = connectionRange;
        }

        public override string Name => "Strommast";
        public int ConnectionRange { get; }
    }

    public sealed class G5_PowerGrid
    {
        private readonly List<G5_PowerPole> _poles = new List<G5_PowerPole>();
        private readonly List<G5_BurningGenerator> _generators = new List<G5_BurningGenerator>();
        private readonly List<G5_IPowerConsumer> _consumers = new List<G5_IPowerConsumer>();

        public void AddPole(G5_PowerPole pole)
        {
            if (pole == null)
            {
                throw new ArgumentNullException(nameof(pole));
            }

            if (_poles.Contains(pole))
            {
                throw new InvalidOperationException("This power pole is already registered.");
            }

            _poles.Add(pole);
        }

        public void AddGenerator(G5_BurningGenerator generator)
        {
            if (generator == null)
            {
                throw new ArgumentNullException(nameof(generator));
            }

            if (_generators.Contains(generator))
            {
                throw new InvalidOperationException("This generator is already registered.");
            }

            _generators.Add(generator);
        }

        public void AddConsumer(G5_IPowerConsumer consumer)
        {
            if (consumer == null)
            {
                throw new ArgumentNullException(nameof(consumer));
            }

            if (_consumers.Contains(consumer))
            {
                throw new InvalidOperationException("This power consumer is already registered.");
            }

            _consumers.Add(consumer);
        }

        public void DistributePower()
        {
            foreach (G5_IPowerConsumer consumer in _consumers)
            {
                consumer.SetPowered(consumer.PowerDemandPerTick == 0);
            }

            foreach (List<G5_PowerPole> component in GetPoleComponents())
            {
                HashSet<G5_PowerPole> componentPoles = new HashSet<G5_PowerPole>(component);
                List<G5_BurningGenerator> connectedGenerators = _generators
                    .Where(generator => IsConnected(generator.Position, componentPoles))
                    .ToList();
                List<G5_IPowerConsumer> connectedConsumers = _consumers
                    .Where(consumer => consumer.PowerDemandPerTick > 0 &&
                        IsConnected(consumer.Position, componentPoles))
                    .OrderBy(consumer => consumer.PowerPriority)
                    .ThenBy(consumer => consumer.Position.Y)
                    .ThenBy(consumer => consumer.Position.X)
                    .ToList();

                long availablePower = connectedGenerators.Sum(generator => (long)generator.AvailablePower);
                int powerToGenerate = 0;
                foreach (G5_IPowerConsumer consumer in connectedConsumers)
                {
                    if (consumer.PowerDemandPerTick <= availablePower)
                    {
                        consumer.SetPowered(true);
                        availablePower -= consumer.PowerDemandPerTick;
                        powerToGenerate = checked(powerToGenerate + consumer.PowerDemandPerTick);
                    }
                }

                int remainingPower = powerToGenerate;
                foreach (G5_BurningGenerator generator in connectedGenerators)
                {
                    if (remainingPower == 0)
                    {
                        break;
                    }

                    int generated = generator.SupplyPower(Math.Min(remainingPower, generator.AvailablePower));
                    remainingPower -= generated;
                }

                if (remainingPower != 0)
                {
                    throw new InvalidOperationException("The power grid allocated more energy than its generators supplied.");
                }
            }
        }

        private List<List<G5_PowerPole>> GetPoleComponents()
        {
            List<List<G5_PowerPole>> components = new List<List<G5_PowerPole>>();
            HashSet<G5_PowerPole> unvisited = new HashSet<G5_PowerPole>(_poles);

            while (unvisited.Count > 0)
            {
                G5_PowerPole start = unvisited.First();
                unvisited.Remove(start);
                List<G5_PowerPole> component = new List<G5_PowerPole> { start };
                Queue<G5_PowerPole> queue = new Queue<G5_PowerPole>();
                queue.Enqueue(start);

                while (queue.Count > 0)
                {
                    G5_PowerPole current = queue.Dequeue();
                    foreach (G5_PowerPole candidate in unvisited.ToList())
                    {
                        int distance = current.Position.ManhattanDistanceTo(candidate.Position);
                        if (distance <= current.ConnectionRange && distance <= candidate.ConnectionRange)
                        {
                            unvisited.Remove(candidate);
                            component.Add(candidate);
                            queue.Enqueue(candidate);
                        }
                    }
                }

                components.Add(component);
            }

            return components;
        }

        private static bool IsConnected(G5_GridPosition position, HashSet<G5_PowerPole> component)
        {
            return component.Any(pole =>
                position.ManhattanDistanceTo(pole.Position) <= pole.ConnectionRange);
        }
    }

    public static class G5_ItemTransfer
    {
        public static int Transfer(
            G5_IItemEndpoint source,
            G5_IItemEndpoint destination,
            G5_Item item,
            int amount)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (amount == 0 || ReferenceEquals(source, destination))
            {
                return 0;
            }

            int available = source.TransferableContents.TryGetValue(item, out int count) ? count : 0;
            int transferAmount = Math.Min(amount, available);
            if (transferAmount == 0 || !destination.CanReceive(item, transferAmount))
            {
                return 0;
            }

            if (!source.TryExtract(item, transferAmount))
            {
                return 0;
            }

            if (destination.TryReceive(item, transferAmount))
            {
                return transferAmount;
            }

            if (!source.TryRestoreExtracted(item, transferAmount))
            {
                throw new InvalidOperationException("A failed transfer could not be rolled back to its source.");
            }

            return 0;
        }
    }

    public sealed class G5_FactorySimulation
    {
        private readonly List<G5_Building> _buildings = new List<G5_Building>();

        public G5_FactorySimulation()
        {
            PowerGrid = new G5_PowerGrid();
        }

        public G5_PowerGrid PowerGrid { get; }
        public IReadOnlyList<G5_Building> Buildings => _buildings.AsReadOnly();
        public long TickNumber { get; private set; }

        public void AddBuilding(G5_Building building)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            if (_buildings.Contains(building))
            {
                throw new InvalidOperationException("This building is already in the factory.");
            }

            if (_buildings.Any(existing => Overlaps(existing, building)))
            {
                throw new InvalidOperationException("Buildings cannot occupy the same grid cells.");
            }

            _buildings.Add(building);

            if (building is G5_PowerPole pole)
            {
                PowerGrid.AddPole(pole);
            }
            else if (building is G5_BurningGenerator generator)
            {
                PowerGrid.AddGenerator(generator);
            }
            else if (building.PowerDemandPerTick > 0 || building is G5_ProductionBuilding)
            {
                PowerGrid.AddConsumer(building);
            }
        }

        public void Tick()
        {
            PowerGrid.DistributePower();

            foreach (G5_Building building in _buildings)
            {
                if (building is G5_Miner miner)
                {
                    miner.Tick();
                }
                else if (building is G5_ProductionBuilding productionBuilding)
                {
                    productionBuilding.Tick();
                }
            }

            foreach (G5_Conveyor conveyor in _buildings.OfType<G5_Conveyor>())
            {
                conveyor.Tick();
            }

            TickNumber = checked(TickNumber + 1);
        }

        private static bool Overlaps(G5_Building first, G5_Building second)
        {
            return first.Position.X < (long)second.Position.X + second.Width
                && second.Position.X < (long)first.Position.X + first.Width
                && first.Position.Y < (long)second.Position.Y + second.Height
                && second.Position.Y < (long)first.Position.Y + first.Height;
        }
    }

    public static class G5_StarterRecipes
    {
        public static IReadOnlyList<G5_Recipe> CreateSmeltingRecipes(int cycleTicks, int powerDemandPerTick)
        {
            return new[]
            {
                new G5_Recipe("Eisenerz schmelzen", G5_RecipeType.Smelting,
                    new[] { new KeyValuePair<G5_Item, int>(G5_Item.Eisenerz, 1) },
                    G5_Item.Eisenbarren, 1, cycleTicks, powerDemandPerTick),
                new G5_Recipe("Kupfererz schmelzen", G5_RecipeType.Smelting,
                    new[] { new KeyValuePair<G5_Item, int>(G5_Item.Kupfererz, 1) },
                    G5_Item.Kupferbarren, 1, cycleTicks, powerDemandPerTick)
            };
        }

        public static IReadOnlyList<G5_Recipe> CreateConstructionRecipes(int cycleTicks, int powerDemandPerTick)
        {
            return new[]
            {
                new G5_Recipe("Eisenplatte herstellen", G5_RecipeType.Construction,
                    new[] { new KeyValuePair<G5_Item, int>(G5_Item.Eisenbarren, 1) },
                    G5_Item.Eisenplatte, 1, cycleTicks, powerDemandPerTick),
                new G5_Recipe("Eisenstange herstellen", G5_RecipeType.Construction,
                    new[] { new KeyValuePair<G5_Item, int>(G5_Item.Eisenbarren, 1) },
                    G5_Item.Eisenstange, 1, cycleTicks, powerDemandPerTick),
                new G5_Recipe("Kupferdraht herstellen", G5_RecipeType.Construction,
                    new[] { new KeyValuePair<G5_Item, int>(G5_Item.Kupferbarren, 1) },
                    G5_Item.Kupferdraht, 1, cycleTicks, powerDemandPerTick)
            };
        }

        public static IReadOnlyList<G5_Recipe> CreateAssemblyRecipes(int cycleTicks, int powerDemandPerTick)
        {
            return new[]
            {
                new G5_Recipe("Rotor herstellen", G5_RecipeType.Assembly,
                    new[]
                    {
                        new KeyValuePair<G5_Item, int>(G5_Item.Eisenstange, 1),
                        new KeyValuePair<G5_Item, int>(G5_Item.Schraube, 1)
                    },
                    G5_Item.Rotor, 1, cycleTicks, powerDemandPerTick)
            };
        }
    }
}
