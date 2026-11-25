using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // Draws the particles of Assets/Particals: blood stains, poison bubbles, fire, ice, heal
    // bubbles, life-steal drops, weapon swooshes, the power blast wave and the ground traces
    // (fire trails, staff strike traces). Particles are presentation only and run on the game
    // time of the arena, so they freeze during pauses and slow down in slow motion; the ground
    // traces themselves belong to the combat system.
    internal sealed class ArenaParticleRenderer
    {
        private const int MaximumParticles = 600;
        private const double DefaultFrameSeconds = 0.1;
        private const double MaximumFrameDelta = 0.1;
        private static readonly string[] SheetPath = { "Particals", "Particals.png" };
        private static readonly string[] MetadataPath = { "Particals", "Particals.json" };

        private const string IceClip = "IceStrike";
        private const string FireClip = "FireTrail";
        private const string PoisonClip = "PoisenBubbels";
        private const string BloodClip = "BloodSplat";
        private const string LeachClip = "Leach";
        private const string WooshClip = "WeaponWoosh";
        private const string HealClip = "HealBubbels";
        private const string BlastClip = "PowerBlast";

        private readonly ArenaWeaponRenderer _weaponRenderer;
        private readonly List<Particle> _particles = new List<Particle>();
        private readonly List<LightningBolt> _bolts = new List<LightningBolt>();
        private const double BoltSeconds = 0.3;
        private static readonly Brush BoltGlowBrush =
            new SolidColorBrush(Color.FromArgb(170, 255, 240, 90));
        private readonly Dictionary<object, double> _emissionCredit =
            new Dictionary<object, double>();
        private readonly Dictionary<AttackVisualState, Point> _lastBladeTips =
            new Dictionary<AttackVisualState, Point>();
        // Game time at which each entity was frozen, for the growing ice crystals.
        private readonly Dictionary<Entity, double> _frozenSince =
            new Dictionary<Entity, double>();

        // Fixed ice crystals around a frozen entity: positions on the hitbox edge as fractions
        // (x, y), tilt in degrees and clip frame. Bottom crystals grow out of the ground, side
        // ones lean outwards, top ones sit on the head.
        private static readonly double[][] IceCrystalLayout =
        {
            new[] { 0.15, 1.0, -8, 1 }, new[] { 0.5, 1.0, 0, 0 }, new[] { 0.85, 1.0, 8, 1 },
            new[] { 0.0, 0.7, -35, 0 }, new[] { 1.0, 0.7, 35, 2 },
            new[] { 0.0, 0.35, -50, 2 }, new[] { 1.0, 0.35, 50, 0 },
            new[] { 0.3, 0.0, -15, 1 }, new[] { 0.7, 0.0, 15, 0 }
        };
        private const double IceGrowSeconds = 0.15;
        private const double IceFadeSeconds = 0.25;
        private readonly Random _random = new Random();
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly Dictionary<string, BitmapSource> _frames =
            new Dictionary<string, BitmapSource>(StringComparer.Ordinal);
        private double _lastClockSeconds = -1;
        private double _time;
        private ArenaField _field;
        private SpriteSheetAnimator _sheet;
        private bool _sheetUnavailable;

        public ArenaParticleRenderer(ArenaWeaponRenderer weaponRenderer)
        {
            _weaponRenderer = weaponRenderer
                ?? throw new ArgumentNullException(nameof(weaponRenderer));
        }

        public int ParticleCount { get { return _particles.Count; } }

        public void Attach(ArenaField field)
        {
            if (ReferenceEquals(_field, field))
            {
                return;
            }
            if (_field != null)
            {
                _field.EntityDamaged -= OnEntityDamaged;
                _field.EntityHealed -= OnEntityHealed;
                _field.LightningChained -= OnLightningChained;
            }

            _field = field;
            _particles.Clear();
            _bolts.Clear();
            _emissionCredit.Clear();
            _lastBladeTips.Clear();
            _frozenSince.Clear();
            if (_field != null)
            {
                _field.EntityDamaged += OnEntityDamaged;
                _field.EntityHealed += OnEntityHealed;
                _field.LightningChained += OnLightningChained;
            }
        }

        // Advances all particles by the arena's game time since the last painted frame and
        // emits new ones for statuses, attacks and ground traces.
        public void Update(ArenaField field)
        {
            double now = _clock.Elapsed.TotalSeconds;
            double realDelta = _lastClockSeconds < 0 ? 0 : now - _lastClockSeconds;
            _lastClockSeconds = now;
            double deltaSeconds = field.IsGameStarted
                ? Math.Min(MaximumFrameDelta, realDelta) * field.GameTimeScale
                : 0;
            Advance(field, deltaSeconds);
        }

        // Game-time step; separated from the clock so tests can drive it.
        public void Advance(ArenaField field, double deltaSeconds)
        {
            if (deltaSeconds <= 0)
            {
                return;
            }

            _time += deltaSeconds;
            EmitStatusParticles(field, deltaSeconds);
            EmitAttackParticles(field, deltaSeconds);
            EmitGroundEffectParticles(field, deltaSeconds);

            for (int i = _bolts.Count - 1; i >= 0; i--)
            {
                _bolts[i].Age += deltaSeconds;
                if (_bolts[i].Age >= BoltSeconds)
                {
                    _bolts.RemoveAt(i);
                }
            }

            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                Particle particle = _particles[i];
                particle.Age += deltaSeconds;
                if (particle.Target != null)
                {
                    SteerTowardTarget(particle, deltaSeconds);
                }
                double drag = Math.Exp(-particle.Drag * deltaSeconds);
                particle.VelocityX *= drag;
                particle.VelocityY = particle.VelocityY * drag + particle.Gravity * deltaSeconds;
                particle.X += particle.VelocityX * deltaSeconds;
                particle.Y += particle.VelocityY * deltaSeconds;
                particle.Rotation += particle.Spin * deltaSeconds;
                if (particle.LandY.HasValue && !particle.OnGround
                    && particle.VelocityY > 0 && particle.Y >= particle.LandY.Value)
                {
                    Land(particle);
                }
                if (particle.Age >= particle.Life)
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        // Ground layer, below the entities: blood stains and traces lying on the floor.
        public void PaintGround(Canvas canvas, ArenaField field, double left, double top)
        {
            foreach (ArenaGroundEffect groundEffect in field.GroundEffects)
            {
                PaintGroundEffect(canvas, groundEffect, left, top);
            }
            foreach (Particle particle in _particles)
            {
                if (particle.OnGround)
                {
                    PaintParticle(canvas, particle, left, top);
                }
            }
        }

        // Air layer, above the entities: everything else, the power blast wave and the ice
        // that encloses frozen entities.
        public void PaintAir(Canvas canvas, ArenaField field, double left, double top)
        {
            PaintFrozenEntities(canvas, field, left, top);
            foreach (AttackVisualState visual in field.ActiveAttackVisuals)
            {
                if (IsBlastWave(visual))
                {
                    PaintBlastWave(canvas, visual, left, top);
                }
            }
            foreach (Particle particle in _particles)
            {
                if (!particle.OnGround)
                {
                    PaintParticle(canvas, particle, left, top);
                }
            }
            foreach (LightningBolt bolt in _bolts)
            {
                PaintBolt(canvas, bolt, left, top);
            }
        }

        public int BoltCount { get { return _bolts.Count; } }

        // Kettenblitz: a flickering bolt between the two entities and a spark at the target.
        private void OnLightningChained(Entity from, Entity to)
        {
            _bolts.Add(new LightningBolt(
                new Point(from.PositionX, from.PositionY),
                new Point(to.PositionX, to.PositionY)));
            Add(new Particle("ProjetilLighning")
            {
                X = to.PositionX,
                Y = to.PositionY,
                Life = BoltSeconds,
                Size = 22,
                Rotation = _random.Next(4) * 90,
                FadeStart = 0.5
            });
        }

        // A jagged line that is drawn new every frame, so the bolt flickers.
        private void PaintBolt(Canvas canvas, LightningBolt bolt, double left, double top)
        {
            Vector line = bolt.To - bolt.From;
            double length = line.Length;
            if (length < 1)
            {
                return;
            }
            Vector normal = new Vector(-line.Y / length, line.X / length);
            int segments = Math.Max(3, (int)(length / 18));
            PointCollection points = new PointCollection();
            for (int i = 0; i <= segments; i++)
            {
                double along = (double)i / segments;
                double offset = i == 0 || i == segments
                    ? 0
                    : (_random.NextDouble() * 2 - 1) * Math.Min(12, length * 0.12);
                Point point = bolt.From + line * along + normal * offset;
                points.Add(new Point(point.X - left, point.Y - top));
            }

            double opacity = 1 - bolt.Age / BoltSeconds;
            canvas.Children.Add(new System.Windows.Shapes.Polyline
            {
                Points = points,
                Stroke = BoltGlowBrush,
                StrokeThickness = 6,
                StrokeLineJoin = PenLineJoin.Round,
                Opacity = opacity,
                IsHitTestVisible = false
            });
            canvas.Children.Add(new System.Windows.Shapes.Polyline
            {
                Points = points.Clone(),
                Stroke = Brushes.White,
                StrokeThickness = 2,
                StrokeLineJoin = PenLineJoin.Round,
                Opacity = opacity,
                IsHitTestVisible = false
            });
        }

        // ---- events ----------------------------------------------------------------------

        // Hurt entities spray blood: drops fly up and out of the body, fall down and land
        // around the feet as stains that fade after a short time.
        private void OnEntityDamaged(Entity entity, int lostHitPoints)
        {
            Rect hitbox = entity.Hitbox;
            int count = Math.Max(5, Math.Min(12, lostHitPoints / 3 + 4));
            for (int i = 0; i < count; i++)
            {
                double angle = -Math.PI / 2 + (_random.NextDouble() * 2 - 1) * Math.PI * 0.45;
                double speed = 90 + _random.NextDouble() * 110;
                Add(new Particle(BloodClip)
                {
                    X = entity.PositionX + (_random.NextDouble() - 0.5) * hitbox.Width * 0.5,
                    Y = hitbox.Top + hitbox.Height * (0.3 + _random.NextDouble() * 0.3),
                    VelocityX = Math.Cos(angle) * speed * 1.3,
                    VelocityY = Math.Sin(angle) * speed,
                    Gravity = 520,
                    Drag = 0.6,
                    LandY = hitbox.Bottom - _random.NextDouble() * Math.Min(14, hitbox.Height * 0.2)
                        + _random.NextDouble() * 10,
                    Life = 3,
                    Size = 13 + _random.NextDouble() * 8,
                    Rotation = _random.Next(4) * 90,
                    Spin = (_random.NextDouble() - 0.5) * 400,
                    FadeStart = 0.7
                });
            }
        }

        // Healing makes bubbles rise from the entity; stolen life flies from the hurt entity
        // to the one that hurt it.
        private void OnEntityHealed(Entity entity, int healedHitPoints, Entity from)
        {
            if (from != null)
            {
                int drops = Math.Max(3, Math.Min(8, healedHitPoints));
                for (int i = 0; i < drops; i++)
                {
                    double angle = _random.NextDouble() * Math.PI * 2;
                    Add(new Particle(LeachClip)
                    {
                        X = from.PositionX + (_random.NextDouble() - 0.5) * from.Hitbox.Width * 0.6,
                        Y = from.PositionY + (_random.NextDouble() - 0.5) * from.Hitbox.Height * 0.4,
                        VelocityX = Math.Cos(angle) * 60,
                        VelocityY = Math.Sin(angle) * 60,
                        Target = entity,
                        Speed = 170 + _random.NextDouble() * 60,
                        Life = 1.6,
                        Loop = true,
                        Phase = _random.NextDouble(),
                        Size = 10 + _random.NextDouble() * 5
                    });
                }
                return;
            }

            // Healing bubbles rise clearly visible from the whole body.
            Rect hitbox = entity.Hitbox;
            int bubbles = Math.Max(12, Math.Min(24, healedHitPoints / 2 + 8));
            for (int i = 0; i < bubbles; i++)
            {
                Add(new Particle(HealClip)
                {
                    X = hitbox.Left - 6 + _random.NextDouble() * (hitbox.Width + 12),
                    Y = hitbox.Top + hitbox.Height * (0.2 + _random.NextDouble() * 0.8),
                    VelocityX = (_random.NextDouble() - 0.5) * 25,
                    VelocityY = -(45 + _random.NextDouble() * 45),
                    Drag = 0.4,
                    Life = 1.4 + _random.NextDouble() * 0.7,
                    Loop = true,
                    Phase = _random.NextDouble(),
                    Size = 15 + _random.NextDouble() * 10,
                    FadeStart = 0.65
                });
            }
        }

        // ---- continuous emitters -------------------------------------------------------------

        private void EmitStatusParticles(ArenaField field, double deltaSeconds)
        {
            List<Entity> entities = new List<Entity>();
            if (field.Player != null)
            {
                entities.Add(field.Player);
            }
            foreach (NonPlayerCharacter enemy in field.Enemies)
            {
                entities.Add(enemy);
            }

            foreach (Entity entity in entities)
            {
                if (!entity.IsAlive)
                {
                    continue;
                }

                bool poisoned = false;
                bool burning = false;
                foreach (ActiveDamageOverTimeEffect effect in entity.ActiveDamageOverTimeEffects)
                {
                    poisoned |= effect.Id == "poison" && !effect.IsExpired;
                    burning |= effect.Id == "burning" && !effect.IsExpired;
                }

                Rect hitbox = entity.Hitbox;
                if (poisoned)
                {
                    // Poison bubbles rise from the entity.
                    foreach (int unused in Emit(Tuple.Create(entity, PoisonClip), 8, deltaSeconds))
                    {
                        Add(new Particle(PoisonClip)
                        {
                            X = hitbox.Left + _random.NextDouble() * hitbox.Width,
                            Y = hitbox.Top + hitbox.Height * (0.2 + _random.NextDouble() * 0.6),
                            VelocityX = (_random.NextDouble() - 0.5) * 15,
                            VelocityY = -(25 + _random.NextDouble() * 30),
                            Life = 0.7 + _random.NextDouble() * 0.5,
                            Loop = true,
                            Phase = _random.NextDouble(),
                            Size = 9 + _random.NextDouble() * 8,
                            FadeStart = 0.5
                        });
                    }
                }
                if (burning)
                {
                    // Flames flicker all around the burning entity.
                    foreach (int unused in Emit(Tuple.Create(entity, FireClip), 16, deltaSeconds))
                    {
                        Add(new Particle(FireClip)
                        {
                            X = hitbox.Left + _random.NextDouble() * hitbox.Width,
                            Y = hitbox.Top + hitbox.Height * (0.15 + _random.NextDouble() * 0.85),
                            VelocityX = (_random.NextDouble() - 0.5) * 20,
                            VelocityY = -(35 + _random.NextDouble() * 40),
                            Life = 0.4 + _random.NextDouble() * 0.3,
                            Loop = true,
                            Phase = _random.NextDouble(),
                            Size = 13 + _random.NextDouble() * 10,
                            FadeStart = 0.5
                        });
                    }
                }
            }
        }

        // Swooshes follow every weapon along its movement, inside the area it hits.
        private void EmitAttackParticles(ArenaField field, double deltaSeconds)
        {
            HashSet<AttackVisualState> active = new HashSet<AttackVisualState>();
            foreach (AttackVisualState visual in field.ActiveAttackVisuals)
            {
                active.Add(visual);
                if (visual.HasBlade)
                {
                    EmitBladeSwooshes(visual, deltaSeconds);
                }
                else if (visual.VisualKind == AttackVisualKind.Cone)
                {
                    EmitConeSwooshes(visual, deltaSeconds);
                }
                else if (visual.VisualKind == AttackVisualKind.Projectile)
                {
                    EmitProjectileSwooshes(visual, deltaSeconds);
                }
                else if (IsBlastWave(visual))
                {
                    EmitBlastSwooshes(visual, deltaSeconds);
                }
                else if (visual.VisualKind == AttackVisualKind.Burst
                    && visual.EffectKind == CombatEffectKind.Healing)
                {
                    EmitHealPulse(visual, deltaSeconds);
                }
            }

            foreach (AttackVisualState finished in new List<AttackVisualState>(_lastBladeTips.Keys))
            {
                if (!active.Contains(finished))
                {
                    _lastBladeTips.Remove(finished);
                }
            }
            foreach (object key in new List<object>(_emissionCredit.Keys))
            {
                if (key is AttackVisualState visual && !active.Contains(visual))
                {
                    _emissionCredit.Remove(key);
                }
            }
        }

        private void EmitBladeSwooshes(AttackVisualState visual, double deltaSeconds)
        {
            Vector blade = new Vector(
                visual.BladeEndX - visual.BladeStartX, visual.BladeEndY - visual.BladeStartY);
            double length = blade.Length;
            if (length < 1)
            {
                return;
            }

            Point tip = new Point(visual.BladeEndX, visual.BladeEndY);
            Vector motion = _lastBladeTips.TryGetValue(visual, out Point lastTip)
                ? tip - lastTip
                : new Vector(-blade.Y, blade.X);
            _lastBladeTips[visual] = tip;
            if (motion.Length < 1e-6)
            {
                motion = new Vector(-blade.Y, blade.X);
            }
            motion.Normalize();
            double angle = Math.Atan2(motion.Y, motion.X) * 180 / Math.PI;

            foreach (int unused in Emit(visual, 55, deltaSeconds))
            {
                double along = 0.45 + _random.NextDouble() * 0.55;
                Add(new Particle(WooshClip)
                {
                    X = visual.BladeStartX + blade.X * along,
                    Y = visual.BladeStartY + blade.Y * along,
                    VelocityX = motion.X * 50,
                    VelocityY = motion.Y * 50,
                    Drag = 4,
                    Life = 0.22,
                    Size = length * (0.22 + _random.NextDouble() * 0.12),
                    Rotation = angle,
                    FadeStart = 0.4
                });
            }
        }

        private void EmitConeSwooshes(AttackVisualState visual, double deltaSeconds)
        {
            double directionAngle = Math.Atan2(visual.DirectionY, visual.DirectionX);
            foreach (int unused in Emit(visual, 40, deltaSeconds))
            {
                double along = visual.Range * (0.25 + _random.NextDouble() * 0.75);
                double angle = directionAngle
                    + (_random.NextDouble() * 2 - 1) * visual.HalfAngleRadians;
                Add(new Particle(WooshClip)
                {
                    X = visual.CenterX + Math.Cos(angle) * along,
                    Y = visual.CenterY + Math.Sin(angle) * along,
                    VelocityX = visual.DirectionX * 70,
                    VelocityY = visual.DirectionY * 70,
                    Drag = 5,
                    Life = 0.22,
                    Size = Math.Max(14, Math.Min(34, visual.Range * 0.22)),
                    Rotation = directionAngle * 180 / Math.PI,
                    FadeStart = 0.4
                });
            }
        }

        private void EmitProjectileSwooshes(AttackVisualState visual, double deltaSeconds)
        {
            double angle = Math.Atan2(visual.DirectionY, visual.DirectionX) * 180 / Math.PI;
            foreach (int unused in Emit(visual, 30, deltaSeconds))
            {
                double behind = 4 + _random.NextDouble() * 12;
                Add(new Particle(WooshClip)
                {
                    X = visual.ProjectileX - visual.DirectionX * behind,
                    Y = visual.ProjectileY - visual.DirectionY * behind,
                    VelocityX = -visual.DirectionX * 25,
                    VelocityY = -visual.DirectionY * 25,
                    Drag = 5,
                    Life = 0.16,
                    Size = 11 + _random.NextDouble() * 4,
                    Rotation = angle,
                    FadeStart = 0.3
                });
            }
        }

        // Swooshes ride on the front of the power blast wave, all around its center.
        private void EmitBlastSwooshes(AttackVisualState visual, double deltaSeconds)
        {
            if (visual.Radius < 2)
            {
                return;
            }
            foreach (int unused in Emit(visual, 140, deltaSeconds))
            {
                double angle = _random.NextDouble() * Math.PI * 2;
                double outwardX = Math.Cos(angle);
                double outwardY = Math.Sin(angle);
                Add(new Particle(WooshClip)
                {
                    X = visual.CenterX + outwardX * visual.Radius,
                    Y = visual.CenterY + outwardY * visual.Radius,
                    VelocityX = outwardX * 90,
                    VelocityY = outwardY * 90,
                    Drag = 3,
                    Life = 0.2,
                    Size = 16 + _random.NextDouble() * 8,
                    Rotation = angle * 180 / Math.PI,
                    FadeStart = 0.35
                });
            }
        }

        // The healing pulse sends bubbles up all along its growing ring.
        private void EmitHealPulse(AttackVisualState visual, double deltaSeconds)
        {
            foreach (int unused in Emit(visual, 90, deltaSeconds))
            {
                double angle = _random.NextDouble() * Math.PI * 2;
                double radius = Math.Max(8, visual.Radius);
                Add(new Particle(HealClip)
                {
                    X = visual.CenterX + Math.Cos(angle) * radius,
                    Y = visual.CenterY + Math.Sin(angle) * radius * 0.6,
                    VelocityX = (_random.NextDouble() - 0.5) * 20,
                    VelocityY = -(60 + _random.NextDouble() * 50),
                    Drag = 0.5,
                    Life = 1.1 + _random.NextDouble() * 0.5,
                    Loop = true,
                    Phase = _random.NextDouble(),
                    Size = 14 + _random.NextDouble() * 9,
                    FadeStart = 0.6
                });
            }
        }

        // Fire trails send up flames; other traces glitter while they last.
        private void EmitGroundEffectParticles(ArenaField field, double deltaSeconds)
        {
            HashSet<ArenaGroundEffect> active = new HashSet<ArenaGroundEffect>();
            foreach (ArenaGroundEffect groundEffect in field.GroundEffects)
            {
                active.Add(groundEffect);
                if (!IsParticleClip(groundEffect.Visual))
                {
                    continue;
                }
                foreach (int unused in Emit(groundEffect, 4, deltaSeconds))
                {
                    Add(new Particle(groundEffect.Visual)
                    {
                        X = groundEffect.CenterX + (_random.NextDouble() - 0.5) * groundEffect.Radius,
                        Y = groundEffect.CenterY - _random.NextDouble() * groundEffect.Radius * 0.5,
                        VelocityX = (_random.NextDouble() - 0.5) * 10,
                        VelocityY = -(20 + _random.NextDouble() * 25),
                        Life = 0.4 + _random.NextDouble() * 0.3,
                        Loop = true,
                        Phase = _random.NextDouble(),
                        Size = groundEffect.Radius * (0.7 + _random.NextDouble() * 0.5),
                        FadeStart = 0.4
                    });
                }
            }
            foreach (object key in new List<object>(_emissionCredit.Keys))
            {
                if (key is ArenaGroundEffect groundEffect && !active.Contains(groundEffect))
                {
                    _emissionCredit.Remove(key);
                }
            }
        }

        // Yields one item per particle to spawn: rate per second, carrying fractions over.
        private IEnumerable<int> Emit(object key, double ratePerSecond, double deltaSeconds)
        {
            _emissionCredit.TryGetValue(key, out double credit);
            credit += ratePerSecond * deltaSeconds;
            int count = (int)credit;
            _emissionCredit[key] = credit - count;
            for (int i = 0; i < count; i++)
            {
                yield return i;
            }
        }

        private void Add(Particle particle)
        {
            if (_particles.Count >= MaximumParticles)
            {
                _particles.RemoveAt(0);
            }
            _particles.Add(particle);
        }

        // A falling drop hits the ground: it stops, spreads into a larger stain and stays there.
        private static void Land(Particle particle)
        {
            particle.Y = particle.LandY.Value;
            particle.VelocityX = 0;
            particle.VelocityY = 0;
            particle.Gravity = 0;
            particle.Spin = 0;
            particle.OnGround = true;
            particle.LandedAge = particle.Age;
            particle.Size *= 1.7;
        }

        private static void SteerTowardTarget(Particle particle, double deltaSeconds)
        {
            Entity target = particle.Target;
            Vector toTarget = new Vector(
                target.PositionX - particle.X, target.PositionY - particle.Y);
            double distance = toTarget.Length;
            if (distance < 10 || !target.IsAlive)
            {
                particle.Age = particle.Life;
                return;
            }

            toTarget /= distance;
            double blend = Math.Min(1, 6 * deltaSeconds);
            particle.VelocityX += (toTarget.X * particle.Speed - particle.VelocityX) * blend;
            particle.VelocityY += (toTarget.Y * particle.Speed - particle.VelocityY) * blend;
        }

        // ---- painting ---------------------------------------------------------------------------

        private static bool IsBlastWave(AttackVisualState visual)
        {
            return visual.VisualKind == AttackVisualKind.Burst
                && visual.EffectKind != CombatEffectKind.Healing
                && visual.EffectKind != CombatEffectKind.Slow;
        }

        // The blast ring is exactly as large as the wave: drawn around the feet with the
        // radius the combat system hits with.
        private void PaintBlastWave(Canvas canvas, AttackVisualState visual, double left, double top)
        {
            if (visual.Radius < 2)
            {
                return;
            }
            int frameCount = GetFrameCount(BlastClip);
            int frameIndex = Math.Min(frameCount - 1,
                1 + (int)(visual.Progress * Math.Max(1, frameCount - 1)));
            BitmapSource frame = GetFrame(BlastClip, frameIndex);
            if (frame == null)
            {
                return;
            }

            double size = visual.Radius * 2;
            Image ring = new Image
            {
                Source = frame,
                Width = size,
                Height = size,
                Stretch = Stretch.Fill,
                Opacity = 1 - visual.Progress * 0.5,
                IsHitTestVisible = false
            };
            RenderOptions.SetBitmapScalingMode(ring, BitmapScalingMode.NearestNeighbor);
            Canvas.SetLeft(ring, visual.CenterX - left - size / 2);
            Canvas.SetTop(ring, visual.CenterY - top - size / 2);
            canvas.Children.Add(ring);
        }

        // Large, fixed ice crystals enclose a frozen entity; they grow in when it freezes and
        // fade out shortly before it thaws.
        private void PaintFrozenEntities(Canvas canvas, ArenaField field, double left, double top)
        {
            List<Entity> entities = new List<Entity>();
            if (field.Player != null)
            {
                entities.Add(field.Player);
            }
            foreach (NonPlayerCharacter enemy in field.Enemies)
            {
                entities.Add(enemy);
            }

            foreach (Entity entity in entities)
            {
                double remaining = 0;
                foreach (ActiveStatusEffect status in entity.ActiveStatusEffects)
                {
                    if (status.Id == "freeze")
                    {
                        remaining = Math.Max(remaining, status.RemainingSeconds);
                    }
                }
                if (remaining <= 0 || !entity.IsAlive)
                {
                    _frozenSince.Remove(entity);
                    continue;
                }
                if (!_frozenSince.TryGetValue(entity, out double since))
                {
                    since = _time;
                    _frozenSince.Add(entity, since);
                }

                double growth = Math.Min(1, (_time - since) / IceGrowSeconds);
                double opacity = Math.Min(1, remaining / IceFadeSeconds);
                Rect hitbox = entity.Hitbox;
                double crystalSize = Math.Max(18, Math.Min(44, Math.Min(hitbox.Width, hitbox.Height) * 0.9));
                foreach (double[] crystal in IceCrystalLayout)
                {
                    BitmapSource frame = GetFrame(IceClip, (int)crystal[3]);
                    if (frame == null)
                    {
                        return;
                    }
                    double size = crystalSize * Math.Max(0.2, growth);
                    double x = hitbox.Left + hitbox.Width * crystal[0];
                    double y = hitbox.Top + hitbox.Height * crystal[1];
                    Image image = new Image
                    {
                        Source = frame,
                        Width = size,
                        Height = size,
                        Stretch = Stretch.Fill,
                        Opacity = opacity,
                        // The crystal stands on its base, tilted around it.
                        RenderTransformOrigin = new Point(0.5, 1),
                        RenderTransform = new RotateTransform(crystal[2]),
                        IsHitTestVisible = false
                    };
                    RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
                    Canvas.SetLeft(image, x - left - size / 2);
                    Canvas.SetTop(image, y - top - size);
                    canvas.Children.Add(image);
                }
            }
        }

        private void PaintGroundEffect(
            Canvas canvas, ArenaGroundEffect groundEffect, double left, double top)
        {
            bool isParticle = IsParticleClip(groundEffect.Visual);
            int frameCount = isParticle ? GetFrameCount(groundEffect.Visual) : 1;
            double phase = (groundEffect.GetHashCode() & 0xff) / 255.0;
            int frameIndex = (int)((_time + phase) / DefaultFrameSeconds) % Math.Max(1, frameCount);
            BitmapSource frame = isParticle
                ? GetFrame(groundEffect.Visual, frameIndex)
                : _weaponRenderer.GetOrbFrame(groundEffect.Visual);
            if (frame == null)
            {
                return;
            }

            // Lightning traces jitter like sparks; all traces fade out at the end.
            double jitterX = isParticle ? 0 : (_random.NextDouble() - 0.5) * 4;
            double jitterY = isParticle ? 0 : (_random.NextDouble() - 0.5) * 4;
            double size = groundEffect.Radius * 2.2;
            Image image = new Image
            {
                Source = frame,
                Width = size,
                Height = size,
                Stretch = Stretch.Uniform,
                Opacity = Math.Min(1, groundEffect.RemainingFraction * 2.5),
                IsHitTestVisible = false
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            Canvas.SetLeft(image, groundEffect.CenterX - left - size / 2 + jitterX);
            Canvas.SetTop(image, groundEffect.CenterY - top - size / 2 + jitterY);
            canvas.Children.Add(image);
        }

        private void PaintParticle(Canvas canvas, Particle particle, double left, double top)
        {
            BitmapSource frame;
            if (IsParticleClip(particle.Clip))
            {
                int frameCount = GetFrameCount(particle.Clip);
                if (frameCount == 0)
                {
                    return;
                }
                int frameIndex;
                if (particle.LandY.HasValue)
                {
                    // Flying drops show the first, small frames; landing plays the splat.
                    frameIndex = particle.OnGround
                        ? Math.Min(frameCount - 1,
                            1 + (int)((particle.Age - particle.LandedAge) / 0.05))
                        : (int)(particle.Age / 0.08) % Math.Min(2, frameCount);
                }
                else if (particle.Loop)
                {
                    frameIndex = (int)((particle.Age + particle.Phase) / DefaultFrameSeconds)
                        % frameCount;
                }
                else
                {
                    double frameSeconds = particle.FrameSeconds > 0
                        ? particle.FrameSeconds
                        : particle.Life / frameCount;
                    frameIndex = Math.Min(frameCount - 1, (int)(particle.Age / frameSeconds));
                }
                frame = GetFrame(particle.Clip, frameIndex);
            }
            else
            {
                frame = _weaponRenderer.GetOrbFrame(particle.Clip);
            }
            if (frame == null)
            {
                return;
            }

            double opacity = 1;
            double fadeStartAge = particle.Life * particle.FadeStart;
            if (particle.Age > fadeStartAge && particle.Life > fadeStartAge)
            {
                opacity = 1 - (particle.Age - fadeStartAge) / (particle.Life - fadeStartAge);
            }

            Image image = new Image
            {
                Source = frame,
                Width = particle.Size,
                Height = particle.Size,
                Stretch = Stretch.Fill,
                Opacity = Math.Max(0, Math.Min(1, opacity)),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(particle.Rotation),
                IsHitTestVisible = false
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            Canvas.SetLeft(image, particle.X - left - particle.Size / 2);
            Canvas.SetTop(image, particle.Y - top - particle.Size / 2);
            canvas.Children.Add(image);
        }

        private bool IsParticleClip(string clip)
        {
            return GetFrameCount(clip) > 0;
        }

        private int GetFrameCount(string clip)
        {
            SpriteSheetAnimator sheet = GetSheet();
            return sheet == null ? 0 : sheet.GetFrameCount(clip);
        }

        private BitmapSource GetFrame(string clip, int frameIndex)
        {
            string key = clip + "#" + frameIndex;
            if (_frames.TryGetValue(key, out BitmapSource frame))
            {
                return frame;
            }
            SpriteSheetAnimator sheet = GetSheet();
            frame = sheet == null ? null : sheet.GetClipFrame(clip, frameIndex);
            _frames[key] = frame;
            return frame;
        }

        private SpriteSheetAnimator GetSheet()
        {
            if (_sheet == null && !_sheetUnavailable)
            {
                try
                {
                    _sheet = new SpriteSheetAnimator(
                        PainterG9.FindAssetPath(SheetPath),
                        PainterG9.FindAssetPath(MetadataPath));
                }
                catch (Exception exception) when (
                    exception is System.IO.IOException
                    || exception is UnauthorizedAccessException
                    || exception is System.Text.Json.JsonException
                    || exception is InvalidOperationException
                    || exception is KeyNotFoundException
                    || exception is NotSupportedException)
                {
                    // Missing particle assets must not stop the game.
                    _sheetUnavailable = true;
                }
            }
            return _sheet;
        }

        private sealed class LightningBolt
        {
            public LightningBolt(Point from, Point to)
            {
                From = from;
                To = to;
            }

            public Point From { get; }
            public Point To { get; }
            public double Age { get; set; }
        }

        private sealed class Particle
        {
            public Particle(string clip)
            {
                Clip = clip;
            }

            public string Clip { get; }
            public double X { get; set; }
            public double Y { get; set; }
            public double VelocityX { get; set; }
            public double VelocityY { get; set; }
            // Velocity decay per second and constant downward acceleration.
            public double Drag { get; set; }
            public double Gravity { get; set; }
            public double Age { get; set; }
            public double Life { get; set; } = 0.5;
            public double Size { get; set; } = 12;
            public double Rotation { get; set; }
            public double Spin { get; set; }
            // Looping clips cycle; others play once over their life (or FrameSeconds per
            // frame) and hold the last frame.
            public bool Loop { get; set; }
            public double Phase { get; set; }
            public double FrameSeconds { get; set; }
            // Share of the life after which the particle fades out.
            public double FadeStart { get; set; } = 0.6;
            public bool OnGround { get; set; }
            // Falling particles (blood) land at this height and stay there as a stain.
            public double? LandY { get; set; }
            public double LandedAge { get; set; }
            // Homing particles (stolen life) fly to this entity at Speed.
            public Entity Target { get; set; }
            public double Speed { get; set; }
        }
    }
}
