using System;
using System.Collections.Generic;
using System.Windows;

namespace OOPGames
{
    public enum TowerDefenseBattlePhase
    {
        Building,
        Running,
        Lost
    }

    public sealed class TowerDefenseBattleEnemy
    {
        public Enemy Enemy;
        public string Name;
        public bool IsBoss;
        public double Distance;
        public double X;
        public double Y;
        public bool FacingLeft;
        public double HitFlash;
    }

    public sealed class TowerDefenseProjectile
    {
        public double X;
        public double Y;
        public TowerDefenseBattleEnemy Target;
        public int Damage;
        public DamageType DamageType;
        public Tower Source;
        public bool Splash;
    }

    // Short-lived visual: lightning chain or floating gold text.
    public sealed class TowerDefenseEffect
    {
        public List<Point> Points = new List<Point>();
        public string Text;
        public double TimeLeft;
        public double Duration;
    }

    // Simulation of one map: spawning, movement along the path, tower attacks and rewards.
    // Coordinates are map coordinates (the same as the tower spots).
    public sealed class TowerDefenseBattle
    {
        // Pixels per range unit of Tower.AttackRange.
        public const double RangeUnit = 13;
        // Pixels per second per unit of Enemy.MoveSpeed.
        public const double SpeedUnit = 17;
        // Share of Enemy.Reward that is paid out as gold (Reward grows quickly with the enemy level).
        public const double RewardFactor = 0.6;
        public const int MainBuildingMaxHealth = 1000;
        const double ProjectileSpeed = 240;

        readonly IList<Point> _Path;
        readonly double[] _SegmentStart;
        readonly double _PathLength;
        readonly IList<Point> _Spots;
        readonly Tower[] _Towers;
        readonly TowerUpgrades[] _Upgrades;
        readonly double[] _Cooldowns;
        readonly Action<int> _AddGold;
        readonly int _MapIndex;
        readonly double _IncomingDamageFactor;
        readonly MainBuilding _Castle;
        readonly Point _CastleGate;
        double _CastleCooldown;
        readonly Queue<string> _SpawnQueue = new Queue<string>();
        double _SpawnTimer;
        bool _WaveCompleted;

        public TowerDefenseBattle(int mapIndex, IList<Point> path, Point castleGate, IList<Point> spots,
            Tower[] towers, TowerUpgrades[] upgrades, double difficultyMultiplier, int healthPercent, Action<int> addGold)
        {
            if (path == null || path.Count < 2) throw new ArgumentException("A path needs at least two points.", nameof(path));
            if (towers.Length != spots.Count || upgrades.Length != spots.Count) throw new ArgumentException("One tower slot per spot is required.");
            if (difficultyMultiplier <= 0) throw new ArgumentOutOfRangeException(nameof(difficultyMultiplier));

            _MapIndex = mapIndex;
            _Path = path;
            _SegmentStart = new double[path.Count];
            for (int i = 1; i < path.Count; i++)
                _SegmentStart[i] = _SegmentStart[i - 1] + Distance(path[i - 1], path[i]);
            _PathLength = _SegmentStart[path.Count - 1];
            _Spots = spots;
            _Towers = towers;
            _Upgrades = upgrades;
            _Cooldowns = new double[spots.Count];
            _AddGold = addGold;
            _IncomingDamageFactor = 1.0 / difficultyMultiplier;
            _CastleGate = castleGate;
            _Castle = new MainBuilding("Burg", 30, 1.0, 5, MainBuildingMaxHealth);
            int missing = MainBuildingMaxHealth - MainBuildingMaxHealth * Math.Max(1, Math.Min(100, healthPercent)) / 100;
            if (missing > 0) _Castle.TakeDamage(missing);
            Phase = TowerDefenseBattlePhase.Building;
            Enemies = new List<TowerDefenseBattleEnemy>();
            Projectiles = new List<TowerDefenseProjectile>();
            Effects = new List<TowerDefenseEffect>();
        }

        public TowerDefenseBattlePhase Phase { get; private set; }
        public TowerDefenseWave CurrentWave { get; private set; }
        public List<TowerDefenseBattleEnemy> Enemies { get; private set; }
        public List<TowerDefenseProjectile> Projectiles { get; private set; }
        public List<TowerDefenseEffect> Effects { get; private set; }
        public int HealthPercent { get { return (int)Math.Ceiling(_Castle.CurrentHealth * 100.0 / _Castle.MaxHealth); } }
        public int RemainingEnemies { get { return Enemies.Count + _SpawnQueue.Count; } }
        public double PathLength { get { return _PathLength; } }

        // Tower values including tier and the percentage upgrades (see TowerUpgrades).
        public static int TowerDamage(Tower tower, TowerUpgrades upgrades) { return (int)Math.Round(tower.Damage * upgrades.DamageMultiplier); }
        public static double TowerAttackSpeed(Tower tower, TowerUpgrades upgrades) { return tower.AttackSpeed * upgrades.SpeedMultiplier; }
        public static double TowerRange(Tower tower, TowerUpgrades upgrades) { return tower.AttackRange * RangeUnit * upgrades.RangeMultiplier; }
        public static int TowerEffectDamage(Tower tower, TowerUpgrades upgrades) { return (int)Math.Round(tower.EffectDamagePerSecond * upgrades.DamageMultiplier); }

        // Half of everything spent on this tower.
        public static int SellValue(Tower tower, TowerUpgrades upgrades)
        {
            return upgrades.Invested(tower) / 2;
        }

        public void StartWave(int wave)
        {
            if (Phase != TowerDefenseBattlePhase.Building) return;
            CurrentWave = TowerDefenseWaves.Get(_MapIndex, wave);
            _SpawnQueue.Clear();
            foreach (string name in CurrentWave.Spawns) _SpawnQueue.Enqueue(name);
            _SpawnTimer = 0.3;
            _WaveCompleted = false;
            Phase = TowerDefenseBattlePhase.Running;
        }

        // True once after the running wave has been beaten.
        public bool ConsumeWaveCompleted()
        {
            bool completed = _WaveCompleted;
            _WaveCompleted = false;
            return completed;
        }

        public void Step(double seconds)
        {
            UpdateEffects(seconds);
            if (Phase != TowerDefenseBattlePhase.Running) return;

            SpawnEnemies(seconds);
            MoveEnemies(seconds);
            if (Phase == TowerDefenseBattlePhase.Lost) return;
            FireTowers(seconds);
            FireCastle(seconds);
            MoveProjectiles(seconds);
            RemoveDefeated();

            if (_SpawnQueue.Count == 0 && Enemies.Count == 0)
            {
                Projectiles.Clear();
                Phase = TowerDefenseBattlePhase.Building;
                _AddGold(CurrentWave.CompletionBonus);
                _WaveCompleted = true;
            }
        }

        public void Heal(int percent)
        {
            _Castle.Repair(_Castle.MaxHealth * percent / 100);
        }

        void SpawnEnemies(double seconds)
        {
            if (_SpawnQueue.Count == 0) return;
            _SpawnTimer -= seconds;
            if (_SpawnTimer > 0) return;

            string name = _SpawnQueue.Dequeue();
            Enemy enemy = TowerDefenseWaves.CreateEnemy(name, CurrentWave.Number);
            enemy.IncomingDamageFactor = _IncomingDamageFactor;
            TowerDefenseBattleEnemy spawned = new TowerDefenseBattleEnemy
            {
                Enemy = enemy,
                Name = name,
                IsBoss = enemy is Boss || enemy is MiniBoss
            };
            PlaceOnPath(spawned);
            Enemies.Add(spawned);
            double interval = TowerDefenseWaves.SpawnInterval(CurrentWave.Number);
            // Bosses enter after a short pause.
            _SpawnTimer = _SpawnQueue.Count == 1 && CurrentWave.Boss != null ? interval * 3 : interval;
        }

        void MoveEnemies(double seconds)
        {
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                TowerDefenseBattleEnemy e = Enemies[i];
                int dotDamage = e.Enemy.TickStatusEffects(seconds);
                if (dotDamage > 0) e.HitFlash = Math.Max(e.HitFlash, 0.05);
                e.HitFlash = Math.Max(0, e.HitFlash - seconds);
                if (e.Enemy.IsDefeated) continue;

                e.Distance += e.Enemy.CurrentMoveSpeed * SpeedUnit * seconds;
                if (e.Distance >= _PathLength)
                {
                    _Castle.TakeDamage(e.Enemy.DamageOnMainBuilding);
                    Enemies.RemoveAt(i);
                    if (_Castle.IsDestroyed)
                    {
                        Phase = TowerDefenseBattlePhase.Lost;
                        return;
                    }
                    continue;
                }
                PlaceOnPath(e);
            }
        }

        void FireTowers(double seconds)
        {
            for (int i = 0; i < _Towers.Length; i++)
            {
                Tower tower = _Towers[i];
                if (tower == null) continue;
                _Cooldowns[i] = Math.Max(0, _Cooldowns[i] - seconds);
                if (_Cooldowns[i] > 0) continue;

                TowerUpgrades upgrades = _Upgrades[i] ?? new TowerUpgrades();
                Point origin = new Point(_Spots[i].X, _Spots[i].Y - 6);
                TowerDefenseBattleEnemy target = FindTarget(origin, TowerRange(tower, upgrades));
                if (target == null) continue;

                _Cooldowns[i] = 1.0 / TowerAttackSpeed(tower, upgrades);
                int damage = TowerDamage(tower, upgrades);
                Point muzzle = new Point(_Spots[i].X, _Spots[i].Y - 34);
                if (tower.Effect == TowerEffect.ChainLightning)
                {
                    ChainLightning(tower, upgrades, target, damage, muzzle);
                    continue;
                }

                Projectiles.Add(new TowerDefenseProjectile
                {
                    X = muzzle.X,
                    Y = muzzle.Y,
                    Target = target,
                    Damage = damage,
                    DamageType = tower.DamageType,
                    Source = tower,
                    Splash = tower.AttackMode == AttackMode.AreaDamage
                });
            }
        }

        void FireCastle(double seconds)
        {
            _CastleCooldown = Math.Max(0, _CastleCooldown - seconds);
            if (_CastleCooldown > 0) return;
            Point origin = new Point(_CastleGate.X, _CastleGate.Y - 20);
            TowerDefenseBattleEnemy target = FindTarget(origin, _Castle.AttackRange * RangeUnit);
            if (target == null) return;
            _CastleCooldown = 1.0 / _Castle.AttackSpeed;
            Projectiles.Add(new TowerDefenseProjectile
            {
                X = origin.X,
                Y = _CastleGate.Y - 38,
                Target = target,
                Damage = _Castle.Damage,
                DamageType = DamageType.Physical
            });
        }

        // The enemy that is furthest along the path inside the range.
        TowerDefenseBattleEnemy FindTarget(Point origin, double range)
        {
            TowerDefenseBattleEnemy best = null;
            foreach (TowerDefenseBattleEnemy e in Enemies)
            {
                if (e.Enemy.IsDefeated || e.X < -4 || e.X > 324) continue;
                double dx = e.X - origin.X, dy = e.Y - origin.Y;
                if (dx * dx + dy * dy > range * range) continue;
                if (best == null || e.Distance > best.Distance) best = e;
            }
            return best;
        }

        void ChainLightning(Tower tower, TowerUpgrades upgrades, TowerDefenseBattleEnemy first, int damage, Point muzzle)
        {
            // Enemies in path order, then the tower's own chain rule picks the neighbours.
            List<TowerDefenseBattleEnemy> ordered = new List<TowerDefenseBattleEnemy>();
            foreach (TowerDefenseBattleEnemy e in Enemies) if (!e.Enemy.IsDefeated) ordered.Add(e);
            ordered.Sort((a, b) => b.Distance.CompareTo(a.Distance));
            List<Enemy> enemies = new List<Enemy>();
            foreach (TowerDefenseBattleEnemy e in ordered) enemies.Add(e.Enemy);

            StromMage mage = tower as StromMage;
            IList<Enemy> hits = mage != null
                ? mage.GetChainTargets(enemies, ordered.IndexOf(first))
                : new List<Enemy> { first.Enemy };

            TowerDefenseEffect bolt = new TowerDefenseEffect { TimeLeft = 0.18, Duration = 0.18 };
            bolt.Points.Add(muzzle);
            foreach (Enemy hit in hits)
            {
                TowerDefenseBattleEnemy battleEnemy = ordered[enemies.IndexOf(hit)];
                // Chained hits lose a bit of power along the way.
                int chainDamage = Math.Max(1, (int)Math.Round(damage * Math.Pow(0.85, bolt.Points.Count - 1)));
                Hit(battleEnemy, chainDamage, tower.DamageType, tower, upgrades);
                bolt.Points.Add(new Point(battleEnemy.X, battleEnemy.Y - 6));
            }
            Effects.Add(bolt);
        }

        void MoveProjectiles(double seconds)
        {
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                TowerDefenseProjectile p = Projectiles[i];
                double tx = p.Target.X, ty = p.Target.Y - 6;
                double dx = tx - p.X, dy = ty - p.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                double step = ProjectileSpeed * seconds;
                if (distance > step)
                {
                    p.X += dx / distance * step;
                    p.Y += dy / distance * step;
                    continue;
                }

                Projectiles.RemoveAt(i);
                if (!Enemies.Contains(p.Target) || p.Target.Enemy.IsDefeated) continue;
                TowerUpgrades upgrades = new TowerUpgrades();
                for (int s = 0; s < _Towers.Length; s++) if (ReferenceEquals(_Towers[s], p.Source) && _Upgrades[s] != null) upgrades = _Upgrades[s];
                Hit(p.Target, p.Damage, p.DamageType, p.Source, upgrades);
                if (!p.Splash) continue;

                // Area towers also hit nearby enemies, up to MaxTargets in total.
                double radius = p.Source.SplashRadius * RangeUnit * 1.6;
                int extra = p.Source.MaxTargets - 1;
                foreach (TowerDefenseBattleEnemy other in Enemies)
                {
                    if (extra <= 0) break;
                    if (other == p.Target || other.Enemy.IsDefeated) continue;
                    double ox = other.X - p.Target.X, oy = other.Y - p.Target.Y;
                    if (ox * ox + oy * oy > radius * radius) continue;
                    Hit(other, p.Damage, p.DamageType, p.Source, upgrades);
                    extra--;
                }
            }
        }

        void Hit(TowerDefenseBattleEnemy target, int damage, DamageType type, Tower source, TowerUpgrades upgrades)
        {
            target.Enemy.TakeDamage(damage, type);
            target.HitFlash = 0.08;
            if (source == null || source.Effect == TowerEffect.None || source.Effect == TowerEffect.ChainLightning) return;
            if (TowerEffectDamage(source, upgrades) != source.EffectDamagePerSecond)
                target.Enemy.ApplyEffect(UpgradedEffectTower(source, upgrades));
            else
                target.Enemy.ApplyEffect(source);
        }

        // Effects scale with the damage upgrades, the Tower objects themselves stay unchanged.
        static Tower UpgradedEffectTower(Tower tower, TowerUpgrades upgrades)
        {
            return new Tower(tower.Name, tower.Damage, tower.AttackSpeed, tower.DamageType, tower.AttackRange,
                tower.AttackMode, tower.Cost, tower.Effect, TowerEffectDamage(tower, upgrades), tower.EffectDuration,
                tower.SlowFactor, tower.MaxChainTargets, tower.EffectStrength);
        }

        void RemoveDefeated()
        {
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                TowerDefenseBattleEnemy e = Enemies[i];
                if (!e.Enemy.IsDefeated) continue;
                Enemies.RemoveAt(i);
                int gold = Math.Max(1, (int)Math.Round(e.Enemy.Reward * RewardFactor));
                _AddGold(gold);
                Effects.Add(new TowerDefenseEffect
                {
                    Text = "+" + gold,
                    Points = { new Point(e.X, e.Y - (e.IsBoss ? 30 : 16)) },
                    TimeLeft = 0.9,
                    Duration = 0.9
                });
            }
        }

        void UpdateEffects(double seconds)
        {
            for (int i = Effects.Count - 1; i >= 0; i--)
            {
                Effects[i].TimeLeft -= seconds;
                if (Effects[i].TimeLeft <= 0) Effects.RemoveAt(i);
            }
        }

        void PlaceOnPath(TowerDefenseBattleEnemy e)
        {
            double d = Math.Max(0, Math.Min(_PathLength, e.Distance));
            int segment = 1;
            while (segment < _Path.Count - 1 && _SegmentStart[segment] < d) segment++;
            Point a = _Path[segment - 1], b = _Path[segment];
            double length = _SegmentStart[segment] - _SegmentStart[segment - 1];
            double t = length <= 0 ? 0 : (d - _SegmentStart[segment - 1]) / length;
            e.X = a.X + (b.X - a.X) * t;
            e.Y = a.Y + (b.Y - a.Y) * t;
            if (Math.Abs(b.X - a.X) > 0.1) e.FacingLeft = b.X < a.X;
        }

        static double Distance(Point a, Point b)
        {
            double dx = b.X - a.X, dy = b.Y - a.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
