using System;
using System.Collections.Generic;

namespace OOPGames
{
    /// <summary>
    /// Datenmodell des JSON-Spielstands; enthält Hof, Lager, Tiere und Marktfortschritt.
    /// </summary>
    public sealed class HayDaySaveData
    {
        public int SaveVersion { get; set; }
        public int Coins { get; set; }
        public int BarnCapacity { get; set; }
        public int SiloCapacity { get; set; }
        public int ChickenCount { get; set; }
        public int CowCount { get; set; }
        public int PigCount { get; set; }
        public int SheepCount { get; set; }
        public int GoatCount { get; set; }
        public int MillLevel { get; set; }
        public int SelectedCropIndex { get; set; }
        public int SelectedBuildingIndex { get; set; }
        public int CameraRow { get; set; }
        public int CameraColumn { get; set; }
        public double Zoom { get; set; }
        public Dictionary<string, int> Inventory { get; set; }
        public List<FarmPlotSaveData> Plots { get; set; }
        public List<FarmBuildingSaveData> Buildings { get; set; }
        public List<MarketOfferSaveData> MarketOffers { get; set; }
        public MarketOrderSaveData Order { get; set; }
        public int EggProgressSeconds { get; set; }
        public int FeedProgressSeconds { get; set; }
        public int CowProgressSeconds { get; set; }
        public int CowFeedProgressSeconds { get; set; }
        public int SheepProgressSeconds { get; set; }
        public int SheepFeedProgressSeconds { get; set; }
        public int PigProgressSeconds { get; set; }
        public int PigFeedProgressSeconds { get; set; }
        public int GoatProgressSeconds { get; set; }
        public int GoatFeedProgressSeconds { get; set; }
        public int EggReady { get; set; }
        public int MilkReady { get; set; }
        public int PorkReady { get; set; }
        public int WoolReady { get; set; }
        public int GoatMilkReady { get; set; }
        public int OrderSequence { get; set; }
        public int MarketDay { get; set; } = int.MinValue;
        public DateTime LastSavedAt { get; set; }
    }

    public sealed class FarmPlotSaveData
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Crop { get; set; }
        public DateTime PlantedAtUtc { get; set; }
    }

    public sealed class FarmBuildingSaveData
    {
        public string Kind { get; set; }
        public string Name { get; set; }
        public int Row { get; set; }
        public int Column { get; set; }
    }

    public sealed class MarketOfferSaveData
    {
        public string Item { get; set; }
        public int Price { get; set; }
    }

    public sealed class MarketOrderSaveData
    {
        public string Item { get; set; }
        public int Quantity { get; set; }
        public int Reward { get; set; }
    }
}
