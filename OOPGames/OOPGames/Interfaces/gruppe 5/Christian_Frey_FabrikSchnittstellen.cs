using System.Collections.Generic;

namespace OOPGames
{
    // Diese Schnittstellen trennen Gegenstandstransport und Stromverbrauch von konkreten Gebaeudeklassen.
    public interface IG5_ItemEndpoint
    {
        G5_GridPosition Position { get; }
        IReadOnlyDictionary<G5_Item, int> TransferableContents { get; }
        bool CanReceive(G5_Item item, int amount);
        bool TryReceive(G5_Item item, int amount);
        bool TryExtract(G5_Item item, int amount);
        bool TryRestoreExtracted(G5_Item item, int amount);
        int CountOf(G5_Item item);
    }

    public interface IG5_PowerConsumer
    {
        G5_GridPosition Position { get; }
        int PowerDemandPerTick { get; }
        int PowerPriority { get; }
        bool IsPowered { get; }
        void SetPowered(bool isPowered);
    }
}
