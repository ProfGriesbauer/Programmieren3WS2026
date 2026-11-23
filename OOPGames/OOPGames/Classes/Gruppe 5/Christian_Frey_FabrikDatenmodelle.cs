using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OOPGames
{

    // Gemeinsame Gegenstands-IDs fuer Rezepte, Lager, Hub, Bander und Inventar.
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
}
