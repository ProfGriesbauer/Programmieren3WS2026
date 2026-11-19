using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OOPGames
{

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
    public abstract class G5_Building : IG5_ItemEndpoint, IG5_PowerConsumer
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
}
