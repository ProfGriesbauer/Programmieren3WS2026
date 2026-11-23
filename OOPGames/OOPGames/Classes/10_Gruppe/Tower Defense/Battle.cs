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
        public int Route;
        public double Distance;
        public double X;
        public double Y;
        // Direction of the path segment the enemy walks on (for facing and bridge layering).
        public double DirectionX;
        public double DirectionY;
        public bool FacingLeft;
        public double HitFlash;
        // Seconds since spawning (drives the walk animation).
        public double Age;
    }

    public sealed class TowerDefenseProjectile
    {
        public double X;
        public double Y;
        public TowerDefenseBattleEnemy Target;
        public int Damage;
        public DamageType DamageType;
        // Null for the castle's own shots.
        public Tower Source;
        public bool Splash;
        // Flight direction in radians (for rotated projectile sprites).
        public double Angle;
        public double Age;
    }

    public enum TowerDefenseEffectKind
    {
        // Floating gold text.
        Text,
        // Death animation of an enemy (Name = enemy name).
        Death,
        // Impact of a tower on an enemy (Name = tower name), e.g. lightning strike or frost nova.
        Impact,
        // Muzzle effect at a tower (Name = tower name, Angle = shooting direction), e.g. the flamethrower flame.
        Muzzle
    }

    // Short-lived visual event created by the battle; the renderer decides how it looks.
    public sealed class TowerDefenseEffect
    {
        public TowerDefenseEffectKind Kind;
        public string Name;
        public string Text;
        public double X;
        public double Y;
        public double Angle;
        public bool FacingLeft;
        public double TimeLeft;
        public double Duration;

        public double Elapsed { get { return Duration - TimeLeft; } }
    }

    // Simulation of one map: spawning, movement along the routes, tower attacks and rewards.
    // Coordinates are map pixels (1920x1056, 96 px per tile), the same as the tower spots.
    public sealed class TowerDefenseBattle
    {
        // Pixels per range unit of Tower.AttackRange (half a tile).
        public const double RangeUnit = 48;
        // Pixels per second per unit of Enemy.MoveSpeed.
        public const double SpeedUnit = 100;
        // Share of Enemy.Reward that is paid out as gold. Reward grows quickly with the enemy level, so the
        // share falls after wave 15 (RewardFactor) down to LateRewardFactor at wave 50 and stays there
        // afterwards. Otherwise the late waves pay far more gold than all upgrades together cost.
        public const double RewardFactor = 0.6;
        public const double LateRewardFactor = 0.3;
        const int RewardDampingStart = 15, RewardDampingEnd = 50;

        public static double RewardFactorAt(int wave)
        {
            if (wave <= RewardDampingStart) return RewardFactor;
            double t = Math.Min(1.0, (wave - RewardDampingStart) / (double)(RewardDampingEnd - RewardDampingStart));
            return RewardFactor + (LateRewardFactor - RewardFactor) * t;
        }

        public const int MainBuildingMaxHealth = 1000;
        const double ProjectileSpeed = 1300;
        // Range circles are measured from the middle of the pedestal on the ground (the foot point lies 12 px below).
        const double TowerCenterHeight = 12;
        const double DeathDuration = 0.6;

        sealed class Route
        {
            public IList<Point> Points;
            public double[] SegmentStart;
            public double Length;
        }

        readonly List<Route> _Routes = new List<Route>();
        readonly IList<Point> _Spots;
        readonly Tower[] _Towers;
        readonly TowerUpgrades[] _Upgrades;
        readonly double[] _Cooldowns;
        readonly Func<Tower, Vector> _MuzzleOffset;
        readonly Action<int> _AddGold;
        readonly int _MapIndex;
        readonly double _IncomingDamageFactor;
        readonly MainBuilding _Castle;
        readonly Point _CastleGate;
        double _CastleCooldown;
        // Extra castle damage per beaten wave (fraction of the base damage), set by the difficulty.
        readonly double _CastleGrowthPerWave;
        int _CastleDamage;
        readonly Queue<string> _SpawnQueue = new Queue<string>();
        double _SpawnTimer;
        int _SpawnCount;
        bool _WaveCompleted;

        // muzzleOffset: where a tower's projectiles start, relative to its foot point, for a tower facing right.
        public TowerDefenseBattle(int mapIndex, IList<IList<Point>> routes, Point castleGate, IList<Point> spots,
            Tower[] towers, TowerUpgrades[] upgrades, double difficultyMultiplier, int healthPercent,
            Func<Tower, Vector> muzzleOffset, Action<int> addGold, double castleGrowthPerWave = 0)
        {
            if (routes == null || routes.Count == 0) throw new ArgumentException("A map needs at least one route.", nameof(routes));
            if (towers.Length != spots.Count || upgrades.Length != spots.Count) throw new ArgumentException("One tower slot per spot is required.");
            if (difficultyMultiplier <= 0) throw new ArgumentOutOfRangeException(nameof(difficultyMultiplier));

            foreach (IList<Point> points in routes)
            {
                if (points.Count < 2) throw new ArgumentException("A route needs at least two points.", nameof(routes));
                Route route = new Route { Points = points, SegmentStart = new double[points.Count] };
                for (int i = 1; i < points.Count; i++)
                    route.SegmentStart[i] = route.SegmentStart[i - 1] + Distance(points[i - 1], points[i]);
                route.Length = route.SegmentStart[points.Count - 1];
                _Routes.Add(route);
            }

            _MapIndex = mapIndex;
            _Spots = spots;
            _Towers = towers;
            _Upgrades = upgrades;
            _Cooldowns = new double[spots.Count];
            TowerAttackAge = new double[spots.Count];
            TowerFacingLeft = new bool[spots.Count];
            for (int i = 0; i < TowerAttackAge.Length; i++) TowerAttackAge[i] = double.MaxValue;
            _MuzzleOffset = muzzleOffset;
            _AddGold = addGold;
            _IncomingDamageFactor = 1.0 / difficultyMultiplier;
            _CastleGate = castleGate;
            _Castle = new MainBuilding("Burg", 30, 1.0, 5, MainBuildingMaxHealth);
            _CastleGrowthPerWave = Math.Max(0, castleGrowthPerWave);
            _CastleDamage = _Castle.Damage;
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
        // Per spot: seconds since the last shot (drives the attack animation) and the shooting direction.
        public double[] TowerAttackAge { get; private set; }
        public bool[] TowerFacingLeft { get; private set; }
        public int HealthPercent { get { return (int)Math.Ceiling(_Castle.CurrentHealth * 100.0 / _Castle.MaxHealth); } }
        public int RemainingEnemies { get { return Enemies.Count + _SpawnQueue.Count; } }
        public double CastleRange { get { return _Castle.AttackRange * RangeUnit; } }

        // The castle only helps against enemies that come close (range 5 = 240 px, one shot per second).
        // Its damage grows linearly with the wave: base x (1 + growth x (wave - 1)).
        public int CastleDamageAt(int wave)
        {
            return (int)Math.Round(_Castle.Damage * (1 + _CastleGrowthPerWave * Math.Max(0, wave - 1)));
        }

        public double RouteLength(int route) { return _Routes[route].Length; }

        // Tower values including tier and the percentage upgrades (see TowerUpgrades).
        public static int TowerDamage(Tower tower, TowerUpgrades upgrades) { return (int)Math.Round(tower.Damage * upgrades.DamageMultiplier); }
        public static double TowerAttackSpeed(Tower tower, TowerUpgrades upgrades) { return tower.AttackSpeed * upgrades.SpeedMultiplier; }
        public static double TowerRange(Tower tower, TowerUpgrades upgrades) { return tower.AttackRange * RangeUnit * upgrades.RangeMultiplier; }
        public static int TowerEffectDamage(Tower tower, TowerUpgrades upgrades) { return (int)Math.Round(tower.EffectDamagePerSecond * upgrades.DamageMultiplier); }

        // Centre of a tower's range circle for a tower standing on `spot`.
        public static Point RangeCenter(Point spot) { return new Point(spot.X, spot.Y - TowerCenterHeight); }

        // Half of everything spent on this tower.
        public static int SellValue(Tower tower, TowerUpgrades upgrades)
        {
            return upgrades.Invested(tower) / 2;
        }

        public void StartWave(int wave, int endlessSeed = 0)
        {
            if (Phase != TowerDefenseBattlePhase.Building) return;
            CurrentWave = TowerDefenseWaves.Get(_MapIndex, wave, endlessSeed);
            _CastleDamage = CastleDamageAt(wave);
            _SpawnQueue.Clear();
            foreach (string name in CurrentWave.Spawns) _SpawnQueue.Enqueue(name);
            _SpawnTimer = 0.3;
            _SpawnCount = 0;
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
            for (int i = 0; i < TowerAttackAge.Length; i++)
                if (TowerAttackAge[i] < double.MaxValue) TowerAttackAge[i] += seconds;
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
            // Spawns take turns; the last enemy (boss) uses the longest route so it is visible longest.
            int route = _SpawnQueue.Count == 0 && CurrentWave.Boss != null ? LongestRoute() : _SpawnCount % _Routes.Count;
            _SpawnCount++;
            TowerDefenseBattleEnemy spawned = new TowerDefenseBattleEnemy
            {
                Enemy = enemy,
                Name = name,
                IsBoss = enemy is Boss || enemy is MiniBoss,
                Route = route
            };
            PlaceOnPath(spawned);
            Enemies.Add(spawned);
            // With several spawns the pause between two enemies of the same spawn stays the same.
            double interval = TowerDefenseWaves.SpawnInterval(CurrentWave.Number) / _Routes.Count;
            // Bosses enter after a short pause.
            _SpawnTimer = _SpawnQueue.Count == 1 && CurrentWave.Boss != null ? interval * 3 * _Routes.Count : interval;
        }

        int LongestRoute()
        {
            int longest = 0;
            for (int i = 1; i < _Routes.Count; i++) if (_Routes[i].Length > _Routes[longest].Length) longest = i;
            return longest;
        }

        void MoveEnemies(double seconds)
        {
            for (int i = Enemies.Count - 1; i >= 0; i--)
            {
                TowerDefenseBattleEnemy e = Enemies[i];
                e.Age += seconds;
                int dotDamage = e.Enemy.TickStatusEffects(seconds);
                if (dotDamage > 0) e.HitFlash = Math.Max(e.HitFlash, 0.05);
                e.HitFlash = Math.Max(0, e.HitFlash - seconds);
                if (e.Enemy.IsDefeated) continue;

                e.Distance += e.Enemy.CurrentMoveSpeed * SpeedUnit * seconds;
                if (e.Distance >= _Routes[e.Route].Length)
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
                TowerDefenseBattleEnemy target = FindTarget(RangeCenter(_Spots[i]), TowerRange(tower, upgrades));
                if (target == null) continue;

                _Cooldowns[i] = 1.0 / TowerAttackSpeed(tower, upgrades);
                TowerAttackAge[i] = 0;
                TowerFacingLeft[i] = target.X < _Spots[i].X;
                Vector offset = _MuzzleOffset(tower);
                if (TowerFacingLeft[i]) offset.X = -offset.X;
                Point muzzle = _Spots[i] + offset;
                double angle = Math.Atan2(target.Y - EnemyCenterHeight(target) - muzzle.Y, target.X - muzzle.X);
                int damage = TowerDamage(tower, upgrades);

                if (tower.Effect == TowerEffect.ChainLightning)
                {
                    ChainLightning(tower, upgrades, target, damage);
                    continue;
                }
                if (tower.Name == "Flamethrower")
                    AddEffect(TowerDefenseEffectKind.Muzzle, tower.Name, muzzle.X, muzzle.Y, angle, 0.43);

                Projectiles.Add(new TowerDefenseProjectile
                {
                    X = muzzle.X,
                    Y = muzzle.Y,
                    Target = target,
                    Damage = damage,
                    DamageType = tower.DamageType,
                    Source = tower,
                    Splash = tower.AttackMode == AttackMode.AreaDamage,
                    Angle = angle
                });
            }
        }

        void FireCastle(double seconds)
        {
            _CastleCooldown = Math.Max(0, _CastleCooldown - seconds);
            if (_CastleCooldown > 0) return;
            Point origin = new Point(_CastleGate.X, _CastleGate.Y - 96);
            TowerDefenseBattleEnemy target = FindTarget(origin, CastleRange);
            if (target == null) return;
            _CastleCooldown = 1.0 / _Castle.AttackSpeed;
            Projectiles.Add(new TowerDefenseProjectile
            {
                X = origin.X,
                Y = origin.Y - 60,
                Target = target,
                Damage = _CastleDamage,
                DamageType = DamageType.Physical
            });
        }

        // Point to aim at: the middle of the enemy's body.
        static double EnemyCenterHeight(TowerDefenseBattleEnemy enemy)
        {
            return enemy.IsBoss ? 80 : 36;
        }

        double Remaining(TowerDefenseBattleEnemy enemy)
        {
            return _Routes[enemy.Route].Length - enemy.Distance;
        }

        // The enemy that is closest to the castle (shortest remaining way) inside the range.
        TowerDefenseBattleEnemy FindTarget(Point origin, double range)
        {
            TowerDefenseBattleEnemy best = null;
            foreach (TowerDefenseBattleEnemy e in Enemies)
            {
                if (e.Enemy.IsDefeated) continue;
                double dx = e.X - origin.X, dy = e.Y - EnemyCenterHeight(e) / 2 - origin.Y;
                if (dx * dx + dy * dy > range * range) continue;
                if (best == null || Remaining(e) < Remaining(best)) best = e;
            }
            return best;
        }

        void ChainLightning(Tower tower, TowerUpgrades upgrades, TowerDefenseBattleEnemy first, int damage)
        {
            // Enemies ordered by their remaining way, then the tower's own chain rule picks the neighbours.
            List<TowerDefenseBattleEnemy> ordered = new List<TowerDefenseBattleEnemy>();
            foreach (TowerDefenseBattleEnemy e in Enemies) if (!e.Enemy.IsDefeated) ordered.Add(e);
            ordered.Sort((a, b) => Remaining(a).CompareTo(Remaining(b)));
            List<Enemy> enemies = new List<Enemy>();
            foreach (TowerDefenseBattleEnemy e in ordered) enemies.Add(e.Enemy);

            StromMage mage = tower as StromMage;
            IList<Enemy> hits = mage != null
                ? mage.GetChainTargets(enemies, ordered.IndexOf(first))
                : new List<Enemy> { first.Enemy };

            int hitNumber = 0;
            foreach (Enemy hit in hits)
            {
                TowerDefenseBattleEnemy battleEnemy = ordered[enemies.IndexOf(hit)];
                // Chained hits lose a bit of power along the way.
                int chainDamage = Math.Max(1, (int)Math.Round(damage * Math.Pow(0.85, hitNumber)));
                Hit(battleEnemy, chainDamage, tower.DamageType, tower, upgrades);
                AddEffect(TowerDefenseEffectKind.Impact, tower.Name, battleEnemy.X, battleEnemy.Y, 0, 0.25);
                hitNumber++;
            }
        }

        void MoveProjectiles(double seconds)
        {
            for (int i = Projectiles.Count - 1; i >= 0; i--)
            {
                TowerDefenseProjectile p = Projectiles[i];
                p.Age += seconds;
                double tx = p.Target.X, ty = p.Target.Y - EnemyCenterHeight(p.Target);
                double dx = tx - p.X, dy = ty - p.Y;
                double distance = Math.Sqrt(dx * dx + dy * dy);
                double step = ProjectileSpeed * seconds;
                if (distance > step)
                {
                    p.X += dx / distance * step;
                    p.Y += dy / distance * step;
                    p.Angle = Math.Atan2(dy, dx);
                    continue;
                }

                Projectiles.RemoveAt(i);
                if (!Enemies.Contains(p.Target) || p.Target.Enemy.IsDefeated) continue;
                TowerUpgrades upgrades = new TowerUpgrades();
                for (int s = 0; s < _Towers.Length; s++) if (ReferenceEquals(_Towers[s], p.Source) && _Upgrades[s] != null) upgrades = _Upgrades[s];
                Hit(p.Target, p.Damage, p.DamageType, p.Source, upgrades);
                if (p.Source != null && p.Source.Name == "Ice Area Tower")
                    AddEffect(TowerDefenseEffectKind.Impact, p.Source.Name, p.Target.X, p.Target.Y, 0, 0.5);
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
                int gold = Math.Max(1, (int)Math.Round(e.Enemy.Reward * RewardFactorAt(CurrentWave.Number)));
                _AddGold(gold);
                Effects.Add(new TowerDefenseEffect
                {
                    Kind = TowerDefenseEffectKind.Death, Name = e.Name, X = e.X, Y = e.Y, FacingLeft = e.FacingLeft,
                    TimeLeft = DeathDuration, Duration = DeathDuration
                });
                Effects.Add(new TowerDefenseEffect
                {
                    Kind = TowerDefenseEffectKind.Text, Text = "+" + gold,
                    X = e.X, Y = e.Y - (e.IsBoss ? 200 : 96), TimeLeft = 0.9, Duration = 0.9
                });
            }
        }

        void AddEffect(TowerDefenseEffectKind kind, string name, double x, double y, double angle, double duration)
        {
            Effects.Add(new TowerDefenseEffect { Kind = kind, Name = name, X = x, Y = y, Angle = angle, TimeLeft = duration, Duration = duration });
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
            Route route = _Routes[e.Route];
            double d = Math.Max(0, Math.Min(route.Length, e.Distance));
            int segment = 1;
            while (segment < route.Points.Count - 1 && route.SegmentStart[segment] < d) segment++;
            Point a = route.Points[segment - 1], b = route.Points[segment];
            double length = route.SegmentStart[segment] - route.SegmentStart[segment - 1];
            double t = length <= 0 ? 0 : (d - route.SegmentStart[segment - 1]) / length;
            e.X = a.X + (b.X - a.X) * t;
            e.Y = a.Y + (b.Y - a.Y) * t;
            e.DirectionX = b.X - a.X;
            e.DirectionY = b.Y - a.Y;
            if (Math.Abs(b.X - a.X) > 0.1) e.FacingLeft = b.X < a.X;
        }

        static double Distance(Point a, Point b)
        {
            double dx = b.X - a.X, dy = b.Y - a.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
