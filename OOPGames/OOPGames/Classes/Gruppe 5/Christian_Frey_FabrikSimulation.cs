using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OOPGames
{

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

        public void MoveBuilding(G5_Building building, G5_GridPosition destination)
        {
            if (building == null)
            {
                throw new ArgumentNullException(nameof(building));
            }

            if (!_buildings.Contains(building))
            {
                throw new InvalidOperationException("The building is not part of this factory.");
            }

            if (_buildings.Any(existing =>
                !ReferenceEquals(existing, building) &&
                Overlaps(existing.Position, existing.Width, existing.Height,
                    destination, building.Width, building.Height)))
            {
                throw new InvalidOperationException("Buildings cannot occupy the same grid cells.");
            }

            building.MoveTo(destination);
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
            return Overlaps(first.Position, first.Width, first.Height,
                second.Position, second.Width, second.Height);
        }

        private static bool Overlaps(
            G5_GridPosition firstPosition,
            int firstWidth,
            int firstHeight,
            G5_GridPosition secondPosition,
            int secondWidth,
            int secondHeight)
        {
            return firstPosition.X < (long)secondPosition.X + secondWidth
                && secondPosition.X < (long)firstPosition.X + firstWidth
                && firstPosition.Y < (long)secondPosition.Y + secondHeight
                && secondPosition.Y < (long)firstPosition.Y + firstHeight;
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
                    G5_Item.Kupferdraht, 1, cycleTicks, powerDemandPerTick),
                new G5_Recipe("Schraube herstellen", G5_RecipeType.Construction,
                    new[] { new KeyValuePair<G5_Item, int>(G5_Item.Eisenstange, 1) },
                    G5_Item.Schraube, 4, cycleTicks, powerDemandPerTick)
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
