using System;
using System.Collections.Generic;

namespace OOPGames
{
    // One wave: enemies in spawn order. A mini boss or boss is always the last enemy.
    public sealed class TowerDefenseWave
    {
        public TowerDefenseWave(int number, IList<string> spawns, string boss, bool isMiniBoss, int completionBonus)
        {
            Number = number;
            Spawns = spawns;
            Boss = boss;
            IsMiniBoss = isMiniBoss;
            CompletionBonus = completionBonus;
        }

        public int Number { get; private set; }
        public IList<string> Spawns { get; private set; }
        public string Boss { get; private set; }
        public bool IsMiniBoss { get; private set; }
        public bool IsBossWave { get { return Boss != null && !IsMiniBoss; } }
        public int CompletionBonus { get; private set; }

        // Enemy types of this wave in order of appearance, without counts (for the preview window).
        public IList<string> EnemyTypes
        {
            get
            {
                List<string> types = new List<string>();
                foreach (string name in Spawns)
                    if (!types.Contains(name)) types.Add(name);
                return types;
            }
        }
    }

    // 50 waves per map (plus random endless waves after that). Regular waves draw from enemy tiers that get stronger;
    // waves 5, 15, 25, 35, 45 end with a mini boss, waves 10, 20, 30, 40, 50 with a boss.
    public static class TowerDefenseWaves
    {
        public const int WaveCount = 50;
        // Upper limit for the enemy slots of a wave, so very late endless waves stay playable; they still
        // get harder because the enemy level keeps rising.
        const int MaxSlots = 60;

        // Endless mode (waves after 50): which enemies come is random. The number of enemy slots keeps growing
        // like in waves 1-50, every 5th wave ends with a random mini boss, every 10th with a random boss.
        // The seed is drawn once per endless run, so the preview matches the battle and a wave restarted
        // from its checkpoint is the same, but every new endless run is different.
        static TowerDefenseWave Endless(int mapIndex, int wave, int seed)
        {
            Random random = new Random(unchecked(seed * 31 + mapIndex * 7919 + wave * 131));
            int slots = Math.Min(MaxSlots, 6 + (int)Math.Round(wave * 0.62));
            string boss = null;
            bool miniBoss = wave % 10 == 5;
            if (wave % 5 == 0)
            {
                List<string> bosses = new List<string>();
                foreach (KeyValuePair<int, KeyValuePair<string, string[]>> entry in _BossWaves)
                    if ((entry.Key % 10 == 5) == miniBoss) bosses.Add(entry.Value.Key);
                boss = bosses[random.Next(bosses.Count)];
                slots = (int)Math.Round(slots * 0.7);
            }

            List<string> pool = new List<string>();
            foreach (string name in _Factory.Keys)
                if (!IsBoss(name)) pool.Add(name);
            List<string> spawns = new List<string>();
            while (slots > 0)
            {
                string name = pool[random.Next(pool.Count)];
                spawns.Add(name);
                slots -= SlotCost(name);
            }
            spawns.Sort((a, b) => SlotCost(a).CompareTo(SlotCost(b)));
            if (boss != null) spawns.Add(boss);

            int bonus = 20 + wave * 5;
            if (boss != null) bonus *= 2;
            return new TowerDefenseWave(wave, spawns, boss, boss != null && miniBoss, bonus);
        }

        static readonly Dictionary<string, Func<int, Enemy>> _Factory = new Dictionary<string, Func<int, Enemy>>
        {
            { "Goblin", l => new Goblin(l) },
            { "Goblin Archer", l => new GoblinArcher(l) },
            { "Goblin Shaman", l => new GoblinShaman(l) },
            { "Orc", l => new Orc(l) },
            { "Orc Berserker", l => new OrcBerserker(l) },
            { "Orc Warlord", l => new OrcWarlord(l) },
            { "Skeleton", l => new Skeleton(l) },
            { "Skeleton Knight", l => new SkeletonKnight(l) },
            { "Troll", l => new Troll(l) },
            { "Stone Golem", l => new StoneGolem(l) },
            { "Fire Elemental", l => new FireElemental(l) },
            { "Ice Elemental", l => new IceElemental(l) },
            { "Storm Elemental", l => new StormElemental(l) },
            { "Earth Elemental", l => new EarthElemental(l) },
            { "Dark Elf", l => new DarkElf(l) },
            { "Vampire", l => new Vampire(l) },
            { "Wyvern", l => new Wyvern(l) },
            { "Fire Golem", l => new FireGolem(l) },
            { "Dragon", l => new Dragon(l) },
            { "Arcane Titan", l => new ArcaneTitan(l) },
            { "Goblin Lord", l => new GoblinLord(l) },
            { "Skeleton Mage", l => new SkeletonMage(l) },
            { "Dark Elf King", l => new DarkElfKing(l) },
            { "All Elements Golem", l => new AllElementsGolem(l) },
            { "Fire Dragon", l => new FireDragon(l) },
            { "Orc Lord", l => new OrcLord(l) },
            { "Vampire Lord", l => new VampireLord(l) },
            { "Elemental God", l => new ElementalGod(l) },
            { "Elder Dragon", l => new ElderDragon(l) },
            { "Skeleton King", l => new SkeletonKing(l) }
        };

        // Regular waves: the tier with the highest start wave <= wave is used, mixed with the tier before.
        static readonly KeyValuePair<int, string[]>[] _Tiers =
        {
            new KeyValuePair<int, string[]>(1, new[] { "Goblin", "Goblin Archer", "Skeleton" }),
            new KeyValuePair<int, string[]>(6, new[] { "Goblin", "Goblin Shaman", "Skeleton", "Orc" }),
            new KeyValuePair<int, string[]>(11, new[] { "Orc", "Orc Berserker", "Skeleton Knight", "Goblin Shaman" }),
            new KeyValuePair<int, string[]>(16, new[] { "Orc Berserker", "Skeleton Knight", "Dark Elf", "Orc Warlord", "Troll" }),
            new KeyValuePair<int, string[]>(21, new[] { "Dark Elf", "Vampire", "Troll", "Fire Elemental", "Ice Elemental" }),
            new KeyValuePair<int, string[]>(26, new[] { "Fire Elemental", "Ice Elemental", "Storm Elemental", "Vampire", "Earth Elemental" }),
            new KeyValuePair<int, string[]>(31, new[] { "Storm Elemental", "Earth Elemental", "Stone Golem", "Wyvern", "Vampire" }),
            new KeyValuePair<int, string[]>(36, new[] { "Stone Golem", "Wyvern", "Fire Golem", "Earth Elemental", "Dragon" }),
            new KeyValuePair<int, string[]>(41, new[] { "Wyvern", "Fire Golem", "Dragon", "Stone Golem", "Arcane Titan" }),
            new KeyValuePair<int, string[]>(46, new[] { "Dragon", "Arcane Titan", "Fire Golem", "Wyvern", "Vampire" })
        };

        // Boss waves: boss name and the matching escort.
        static readonly Dictionary<int, KeyValuePair<string, string[]>> _BossWaves = new Dictionary<int, KeyValuePair<string, string[]>>
        {
            { 5, new KeyValuePair<string, string[]>("Goblin Lord", new[] { "Goblin", "Goblin Archer", "Goblin Shaman" }) },
            { 10, new KeyValuePair<string, string[]>("Orc Lord", new[] { "Orc", "Orc Berserker", "Goblin" }) },
            { 15, new KeyValuePair<string, string[]>("Skeleton Mage", new[] { "Skeleton", "Skeleton Knight" }) },
            { 20, new KeyValuePair<string, string[]>("Vampire Lord", new[] { "Vampire", "Dark Elf", "Skeleton Knight" }) },
            { 25, new KeyValuePair<string, string[]>("Dark Elf King", new[] { "Dark Elf", "Vampire", "Troll" }) },
            { 30, new KeyValuePair<string, string[]>("Elemental God", new[] { "Fire Elemental", "Ice Elemental", "Storm Elemental", "Earth Elemental" }) },
            { 35, new KeyValuePair<string, string[]>("All Elements Golem", new[] { "Stone Golem", "Earth Elemental", "Fire Golem" }) },
            { 40, new KeyValuePair<string, string[]>("Elder Dragon", new[] { "Wyvern", "Dragon", "Fire Elemental" }) },
            { 45, new KeyValuePair<string, string[]>("Fire Dragon", new[] { "Fire Elemental", "Fire Golem", "Wyvern" }) },
            { 50, new KeyValuePair<string, string[]>("Skeleton King", new[] { "Skeleton Knight", "Vampire", "Dark Elf", "Orc Warlord" }) }
        };

        // Heavy enemies use more of a wave's slots, so late waves are not flooded with tanks.
        static readonly Dictionary<string, int> _SlotCost = new Dictionary<string, int>
        {
            { "Troll", 2 }, { "Stone Golem", 2 }, { "Earth Elemental", 2 }, { "Fire Golem", 2 },
            { "Dragon", 3 }, { "Arcane Titan", 4 }, { "Vampire", 2 }, { "Wyvern", 2 }
        };

        public static bool IsBoss(string name)
        {
            foreach (KeyValuePair<string, string[]> wave in _BossWaves.Values)
                if (wave.Key == name) return true;
            return false;
        }

        // Level passed to the enemy constructors. Enemy stats grow by 20 % of their base value per level.
        public static int EnemyLevel(int wave)
        {
            int level = 1 + (wave - 1) / 2;
            // Endless waves mix in weak enemies (goblins, skeletons) at random. A higher level makes the
            // average random wave as strong as the late fixed waves instead of suddenly easier.
            return wave > WaveCount ? (int)Math.Round(level * EndlessLevelFactor + EndlessLevelBonus) : level;
        }

        const double EndlessLevelFactor = 1.5, EndlessLevelBonus = 2;

        public static Enemy CreateEnemy(string name, int wave)
        {
            Func<int, Enemy> create;
            if (!_Factory.TryGetValue(name, out create)) throw new ArgumentException("Unknown enemy: " + name, nameof(name));
            return create(EnemyLevel(wave));
        }

        // Spawn delay after an enemy of this wave in seconds.
        public static double SpawnInterval(int wave)
        {
            return Math.Max(0.45, 1.0 - wave * 0.011);
        }

        // endlessSeed is only used for waves after 50 (see Endless).
        public static TowerDefenseWave Get(int mapIndex, int wave, int endlessSeed = 0)
        {
            if (wave < 1) throw new ArgumentOutOfRangeException(nameof(wave));
            if (wave > WaveCount) return Endless(mapIndex, wave, endlessSeed);
            Random random = new Random((mapIndex + 1) * 7919 + wave * 131);
            int slots = Math.Min(MaxSlots, 6 + (int)Math.Round(wave * 0.62));
            List<string> spawns = new List<string>();
            string boss = null;
            bool miniBoss = false;

            KeyValuePair<string, string[]> bossWave;
            List<string> pool = new List<string>();
            if (_BossWaves.TryGetValue(wave, out bossWave))
            {
                boss = bossWave.Key;
                miniBoss = wave % 10 == 5;
                pool.AddRange(bossWave.Value);
                slots = (int)Math.Round(slots * 0.7);
            }
            else
            {
                int tier = 0;
                for (int i = 0; i < _Tiers.Length; i++)
                    if (_Tiers[i].Key <= wave) tier = i;
                for (int weight = 0; weight < 3; weight++) pool.AddRange(_Tiers[tier].Value);
                if (tier > 0) pool.AddRange(_Tiers[tier - 1].Value);
            }

            while (slots > 0)
            {
                string name = pool[random.Next(pool.Count)];
                int cost;
                if (!_SlotCost.TryGetValue(name, out cost)) cost = 1;
                spawns.Add(name);
                slots -= cost;
            }
            // Easier enemies first, so a wave builds up towards its strongest enemies.
            spawns.Sort((a, b) => SlotCost(a).CompareTo(SlotCost(b)));
            if (boss != null) spawns.Add(boss);

            int bonus = 20 + wave * 5;
            if (boss != null) bonus *= 2;
            return new TowerDefenseWave(wave, spawns, boss, miniBoss, bonus);
        }

        static int SlotCost(string name)
        {
            int cost;
            return _SlotCost.TryGetValue(name, out cost) ? cost : 1;
        }
    }
}
