using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace OOPGames
{

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
}
