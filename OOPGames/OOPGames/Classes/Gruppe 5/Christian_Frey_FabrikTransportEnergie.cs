using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OOPGames
{

    public sealed class G5_Conveyor : G5_Building
    {
        private const int TransportIntervalTicks = 12;
        private readonly G5_ItemBuffer _items;
        private int _itemsPerTick;
        private int _ticksUntilTransport;
        private IG5_ItemEndpoint _source;
        private IG5_ItemEndpoint _destination;

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
        internal IG5_ItemEndpoint Source => _source;
        internal IG5_ItemEndpoint Destination => _destination;
        internal double TransportProgress => _items.UsedCapacity == 0
            ? 0
            : Math.Max(0, Math.Min(1,
                (TransportIntervalTicks - _ticksUntilTransport) / (double)TransportIntervalTicks));

        public void Connect(IG5_ItemEndpoint source, IG5_ItemEndpoint destination)
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

        internal void Disconnect()
        {
            _source = null;
            _destination = null;
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

            if (_ticksUntilTransport > 0)
            {
                _ticksUntilTransport--;
                SetStatus(_items.UsedCapacity > 0
                    ? G5_BuildingStatus.Working
                    : G5_BuildingStatus.Idle);
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

            if (moved > 0)
            {
                _ticksUntilTransport = TransportIntervalTicks;
            }

            SetStatus(moved > 0 ? G5_BuildingStatus.Working : G5_BuildingStatus.Idle);
        }

        public override bool TryReceive(G5_Item item, int amount)
        {
            bool wasEmpty = _items.UsedCapacity == 0;
            bool received = _items.TryAdd(item, amount);
            if (received && wasEmpty)
            {
                _ticksUntilTransport = TransportIntervalTicks;
            }

            return received;
        }

        public override bool CanReceive(G5_Item item, int amount)
        {
            return _items.CanAdd(item, amount);
        }

        public override bool TryExtract(G5_Item item, int amount)
        {
            if (_ticksUntilTransport > 0)
            {
                return false;
            }

            bool extracted = _items.TryRemove(item, amount);
            if (extracted && _items.UsedCapacity == 0)
            {
                _ticksUntilTransport = 0;
            }

            return extracted;
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
        private readonly List<IG5_PowerConsumer> _consumers = new List<IG5_PowerConsumer>();

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

        public void AddConsumer(IG5_PowerConsumer consumer)
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
            foreach (IG5_PowerConsumer consumer in _consumers)
            {
                consumer.SetPowered(consumer.PowerDemandPerTick == 0);
            }

            foreach (List<G5_PowerPole> component in GetPoleComponents())
            {
                HashSet<G5_PowerPole> componentPoles = new HashSet<G5_PowerPole>(component);
                List<G5_BurningGenerator> connectedGenerators = _generators
                    .Where(generator => IsConnected(generator.Position, componentPoles))
                    .ToList();
                List<IG5_PowerConsumer> connectedConsumers = _consumers
                    .Where(consumer => consumer.PowerDemandPerTick > 0 &&
                        IsConnected(consumer.Position, componentPoles))
                    .OrderBy(consumer => consumer.PowerPriority)
                    .ThenBy(consumer => consumer.Position.Y)
                    .ThenBy(consumer => consumer.Position.X)
                    .ToList();

                long availablePower = connectedGenerators.Sum(generator => (long)generator.AvailablePower);
                int powerToGenerate = 0;
                foreach (IG5_PowerConsumer consumer in connectedConsumers)
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
            IG5_ItemEndpoint source,
            IG5_ItemEndpoint destination,
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
}
