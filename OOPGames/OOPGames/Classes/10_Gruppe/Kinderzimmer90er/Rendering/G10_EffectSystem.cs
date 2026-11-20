using System;
using System.Collections.Generic;

namespace OOPGames
{
    internal sealed class G10_EffectSystem
    {
        readonly List<G10_SolarShooterBurst> _Bursts = new List<G10_SolarShooterBurst>();
        int _Seed;

        public IReadOnlyList<G10_SolarShooterBurst> Bursts { get { return _Bursts; } }

        public void Attach(IG10_ShooterView session)
        {
            session.EnemyDestroyed += OnEnemyDestroyed;
            session.PlayerHit += OnEntityDestroyed;
            session.BossDefeated += OnEntityDestroyed;
        }

        void OnEnemyDestroyed(object sender, G10_EntityDestroyedEventArgs args)
        {
            AddBurst(args.X, args.Y, args.Large);
        }

        void OnEntityDestroyed(object sender, G10_EntityDestroyedEventArgs args)
        {
            AddBurst(args.X, args.Y, args.Large);
        }

        void AddBurst(float x, float y, bool large)
        {
            if (_Bursts.Count >= 24) _Bursts.RemoveAt(0);
            _Bursts.Add(new G10_SolarShooterBurst(x, y, large, _Seed++));
        }

        public void Update(float deltaSeconds)
        {
            for (int i = _Bursts.Count - 1; i >= 0; i--)
            {
                G10_SolarShooterBurst burst = _Bursts[i];
                burst.Age += deltaSeconds;
                if (burst.Age >= 0.72) _Bursts.RemoveAt(i);
            }
        }
    }
}
