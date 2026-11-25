using System;
using System.Collections.Generic;

namespace OOPGames
{
    // What the spawn director needs to know about the arena and how it spawns an enemy.
    internal interface IEnemySpawnHost
    {
        int LivingEnemyCount { get; }
        int CountLivingEnemies(string typeId);
        int Kills { get; }
        double RunSeconds { get; }
        // The kind's player condition (inside/outside its regions) is met.
        bool IsPlayerConditionMet(EnemyType type);
        // False when no free position was found; the spawn is then skipped.
        bool TrySpawnEnemy(EnemyType type);
    }

    // Spawns every enemy kind on its own timer (game time). Each time a timer fires, the
    // spawn happens with the kind's SpawnChancePercent; a failed roll or a blocking
    // condition (overall cap, the kind's maximum, kills or run time) skips this spawn, as
    // the single timer did before enemy kinds existed. Start enemies ignore the chance.
    public sealed class EnemySpawnDirector
    {
        private readonly Dictionary<string, double> _elapsedSeconds =
            new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        // Timers start so that the first spawn comes after FirstSpawnDelaySeconds.
        public void Reset(IEnumerable<EnemyType> types)
        {
            _elapsedSeconds.Clear();
            foreach (EnemyType type in types)
            {
                _elapsedSeconds[type.Id] = type.SpawnIntervalSeconds - type.FirstSpawnDelaySeconds;
            }
        }

        public IReadOnlyDictionary<string, double> GetTimers()
        {
            return new Dictionary<string, double>(_elapsedSeconds, StringComparer.OrdinalIgnoreCase);
        }

        public void RestoreTimer(string typeId, double elapsedSeconds)
        {
            if (!String.IsNullOrEmpty(typeId) && !Double.IsNaN(elapsedSeconds))
            {
                _elapsedSeconds[typeId] = elapsedSeconds;
            }
        }

        // Enemies present when a run begins; kills and run time count as zero here (the
        // field resets them only after the arena).
        internal void SpawnStartEnemies(IEnumerable<EnemyType> types, IEnemySpawnHost host)
        {
            foreach (EnemyType type in types)
            {
                for (int i = 0; type.Enabled && i < type.StartCount; i++)
                {
                    if (!CanSpawn(type, host, 0, 0) || !host.TrySpawnEnemy(type))
                    {
                        break;
                    }
                }
            }
        }

        internal void Update(
            double deltaSeconds, IEnumerable<EnemyType> types, IEnemySpawnHost host)
        {
            foreach (EnemyType type in types)
            {
                if (!type.Enabled)
                {
                    continue;
                }

                double elapsed = (_elapsedSeconds.TryGetValue(type.Id, out double stored)
                    ? stored
                    : type.SpawnIntervalSeconds - type.FirstSpawnDelaySeconds) + deltaSeconds;
                if (elapsed < type.SpawnIntervalSeconds)
                {
                    _elapsedSeconds[type.Id] = elapsed;
                    continue;
                }
                _elapsedSeconds[type.Id] = elapsed % type.SpawnIntervalSeconds;
                if (type.SpawnChancePercent < 100
                    && Random.Shared.NextDouble() * 100 >= type.SpawnChancePercent)
                {
                    continue;
                }

                int groupSize = type.GroupSizeMin
                    + Random.Shared.Next(type.GroupSizeMax - type.GroupSizeMin + 1);
                for (int i = 0; i < groupSize; i++)
                {
                    if (!CanSpawn(type, host) || !host.TrySpawnEnemy(type))
                    {
                        break;
                    }
                }
            }
        }

        internal static bool CanSpawn(EnemyType type, IEnemySpawnHost host)
        {
            return CanSpawn(type, host, host.Kills, host.RunSeconds);
        }

        private static bool CanSpawn(
            EnemyType type, IEnemySpawnHost host, int kills, double runSeconds)
        {
            return host.LivingEnemyCount < ArenaField.MaximumEnemyCount
                && host.CountLivingEnemies(type.Id) < type.MaxAlive
                && kills >= type.MinKills
                && (!type.MaxKills.HasValue || kills < type.MaxKills.Value)
                && runSeconds >= type.MinRunSeconds
                && host.IsPlayerConditionMet(type);
        }
    }
}
