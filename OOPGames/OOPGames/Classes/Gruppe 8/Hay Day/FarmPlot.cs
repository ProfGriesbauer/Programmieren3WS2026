using System;

namespace OOPGames
{
    public sealed class FarmPlot
    {
        public FarmPlot(int row, int column) : this(row, column, null, default(DateTime))
        {
        }

        public FarmPlot(int row, int column, string crop, DateTime plantedAtUtc)
        {
            Row = row;
            Column = column;
            Crop = crop;
            PlantedAtUtc = plantedAtUtc;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
        public string Crop { get; private set; }
        public DateTime PlantedAtUtc { get; private set; }
        public bool Ripe { get; private set; }

        public void Plant(string crop, DateTime plantedAt)
        {
            Crop = crop;
            PlantedAtUtc = plantedAt;
            Ripe = false;
        }

        public void Clear()
        {
            Crop = null;
            PlantedAtUtc = default(DateTime);
            Ripe = false;
        }

        public bool IsRipe(DateTime now)
        {
            return Crop != null && now - PlantedAtUtc >= GrowthDuration(Crop);
        }

        public static TimeSpan GrowthDuration(string crop)
        {
            return TimeSpan.FromMinutes(2);
        }

        public string RemainingGrowthText(DateTime now)
        {
            if (IsRipe(now))
            {
                return "Reife";
            }

            TimeSpan remaining = GrowthDuration(Crop) - (now - PlantedAtUtc);
            int seconds = Math.Max(1, (int)Math.Ceiling(remaining.TotalSeconds));
            return (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
        }

        public bool MarkRipeIfNeeded(DateTime now)
        {
            if (!Ripe && IsRipe(now))
            {
                Ripe = true;
                return true;
            }

            return false;
        }
    }
}
