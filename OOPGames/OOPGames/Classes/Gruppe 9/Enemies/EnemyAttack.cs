using System;
using System.Collections.Generic;

namespace OOPGames
{
    // A step of an attack's windup (see EnemyWindupStepDefinition).
    public sealed class EnemyWindupStep
    {
        public EnemyWindupStep(string clip, double seconds, string sound)
        {
            Clip = clip;
            Seconds = seconds;
            Sound = sound;
        }

        public string Clip { get; }
        public double Seconds { get; }
        public string Sound { get; }
    }

    // A resolved attack of an enemy kind: the weapon it fires and when it is chosen.
    public sealed class EnemyAttack
    {
        public const string PatternCone = "Cone";
        public const string PatternArc = "Arc";
        public const string PatternSpin = "Spin";
        public const string PatternProjectile = "Projectile";
        // Pulse (SelfPulseAttackPattern) only affects the attacker itself (healing burst),
        // so enemy attacks do not offer it.
        public const string PatternPulse = "Pulse";
        public const string PatternWave = "Wave";

        public static readonly IReadOnlyList<string> Patterns = new[]
        {
            PatternCone, PatternArc, PatternSpin, PatternProjectile, PatternWave
        };

        // Finished weapons that can serve as template, by id.
        public static readonly IReadOnlyList<Weapon> BaseWeapons = new[]
        {
            WeaponCatalog.EnemyClaws, WeaponCatalog.Sword,
            WeaponCatalog.MagicWand, WeaponCatalog.Bow
        };

        // The standard enemy's only attack: the claws as they always were.
        public static readonly EnemyAttack StandardClaws = new EnemyAttack(
            WeaponCatalog.EnemyClaws, 0, WeaponCatalog.EnemyClaws.AttackPattern.Range, 1);

        private EnemyAttack(Weapon weapon, double useFrom, double useUpTo, double weight)
        {
            Weapon = weapon;
            UseFromDistance = useFrom;
            UseUpToDistance = useUpTo;
            Weight = weight;
            WindupSteps = Array.Empty<EnemyWindupStep>();
        }

        public Weapon Weapon { get; }
        public double UseFromDistance { get; }
        public double UseUpToDistance { get; }
        public double Weight { get; }
        public EnemyAttackTrigger Trigger { get; private set; }
        public EnemyTriggerSource TriggeredBy { get; private set; }
        public IReadOnlyList<EnemyWindupStep> WindupSteps { get; private set; }
        public double WindupSeconds { get; private set; }
        // Null: no sound.
        public string Sound { get; private set; }
        public bool KillsAttacker { get; private set; }

        // Whether the entity may set the attack off (TriggeredBy, else its targets).
        public bool MayBeTriggeredBy(Entity entity)
        {
            switch (TriggeredBy)
            {
                case EnemyTriggerSource.Player:
                    return entity is PlayerCharacter;
                case EnemyTriggerSource.PlayerSide:
                    return entity.Team == CombatTeam.Player;
                case EnemyTriggerSource.Anyone:
                    return entity.Team != CombatTeam.Neutral;
                default:
                    return Weapon.TargetingMode == WeaponTargetingMode.Everyone
                        ? entity.Team != CombatTeam.Neutral
                        : entity.Team == CombatTeam.Player;
            }
        }

        // Whether the attack is chosen for a target this far away (to its hitbox).
        public bool FitsDistance(double distance)
        {
            return distance >= UseFromDistance && distance <= UseUpToDistance
                && distance <= Weapon.AttackPattern.Range;
        }

        public static string GetPatternName(string pattern)
        {
            switch (pattern)
            {
                case PatternCone: return "Kegel (Nahkampf)";
                case PatternArc: return "Bogenhieb";
                case PatternSpin: return "Drehwirbel";
                case PatternProjectile: return "Projektil";
                case PatternPulse: return "Puls um sich";
                case PatternWave: return "Welle";
                default: return pattern;
            }
        }

        public static string GetPatternOf(IAttackPattern pattern)
        {
            if (pattern is SweepingArcAttackPattern) return PatternArc;
            if (pattern is SpinningAttackPattern) return PatternSpin;
            if (pattern is ProjectileAttackPattern) return PatternProjectile;
            if (pattern is SelfPulseAttackPattern) return PatternPulse;
            if (pattern is RadialAttackPattern) return PatternWave;
            return PatternCone;
        }

        public static Weapon FindBaseWeapon(string id)
        {
            foreach (Weapon weapon in BaseWeapons)
            {
                if (String.Equals(weapon.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    return weapon;
                }
            }
            return null;
        }

        internal static EnemyAttack Resolve(EnemyAttackDefinition definition,
            string weaponId, EnemyValueChecker check)
        {
            Weapon template = FindBaseWeapon(definition.BaseWeapon);
            if (template == null)
            {
                if (!String.IsNullOrEmpty(definition.BaseWeapon))
                {
                    check.Report("Waffe", "„" + definition.BaseWeapon + "“ ist unbekannt");
                }
                template = WeaponCatalog.EnemyClaws;
            }

            IAttackPattern basePattern = template.AttackPattern;
            string pattern = GetPatternOf(basePattern);
            if (!String.IsNullOrEmpty(definition.Pattern))
            {
                string requested = null;
                foreach (string known in Patterns)
                {
                    if (String.Equals(known, definition.Pattern, StringComparison.OrdinalIgnoreCase))
                    {
                        requested = known;
                    }
                }
                if (requested == null)
                {
                    check.Report("Muster", "„" + definition.Pattern + "“ ist unbekannt");
                }
                else
                {
                    pattern = requested;
                }
            }

            IAttackPattern attackPattern = BuildPattern(definition, pattern, basePattern, check);
            ICombatEffect effect = BuildEffect(definition, template, check);
            double cooldown = check.Double(definition.CooldownSeconds, template.CooldownSeconds,
                0, 60, "Abklingzeit");
            double knockback = check.Double(definition.Knockback, template.KnockbackDistance,
                0, 500, "Rückstoß");
            int maxHits = check.Int(definition.MaxHitsPerTarget, template.MaximumHitsPerTarget,
                1, 100, "Treffer pro Ziel");

            WeaponTargetingMode targeting = EnemyType.ResolveEnum(definition.Targets,
                WeaponTargetingMode.Opponents, "Ziele", check);
            if (targeting != WeaponTargetingMode.Opponents
                && targeting != WeaponTargetingMode.Everyone)
            {
                check.Report("Ziele", "nur Opponents oder Everyone möglich");
                targeting = WeaponTargetingMode.Opponents;
            }
            Weapon weapon = new Weapon(
                weaponId, template.Name, template.Description, attackPattern, effect, cooldown,
                targeting, template.AnimationPlaceholder,
                maxHits, knockback, template.Appearance);

            double useFrom = check.Double(definition.UseFromDistance, 0, 0, 5000,
                "Einsatz ab Abstand");
            double useUpTo = check.Double(definition.UseUpToDistance, attackPattern.Range,
                useFrom, 5000, "Einsatz bis Abstand", attackPattern.Range);
            double weight = check.Double(definition.Weight, 1, 0.01, 1000, "Gewichtung");
            EnemyAttack attack = new EnemyAttack(weapon, useFrom, useUpTo, weight)
            {
                Trigger = EnemyType.ResolveEnum(definition.Trigger,
                    EnemyAttackTrigger.Distance, "Auslöser", check),
                TriggeredBy = EnemyType.ResolveEnum(definition.TriggeredBy,
                    EnemyTriggerSource.Targets, "Ausgelöst von", check),
                Sound = ResolveSound(definition.Sound, "Sound", check),
                KillsAttacker = definition.KillsAttacker == true
            };
            List<EnemyWindupStep> steps = new List<EnemyWindupStep>();
            foreach (EnemyWindupStepDefinition step in definition.Windup
                ?? new List<EnemyWindupStepDefinition>())
            {
                if (step == null || String.IsNullOrWhiteSpace(step.Clip))
                {
                    check.Report("Vorlauf", "jeder Schritt braucht einen Clip-Namen");
                    continue;
                }
                double seconds = check.Positive(step.Seconds, 0.5, 60, "Vorlauf-Dauer");
                steps.Add(new EnemyWindupStep(step.Clip.Trim(), seconds,
                    ResolveSound(step.Sound, "Vorlauf-Sound", check)));
            }
            attack.WindupSteps = steps;
            foreach (EnemyWindupStep step in steps)
            {
                attack.WindupSeconds += step.Seconds;
            }
            return attack;
        }

        internal static string ResolveSound(string sound, string label, EnemyValueChecker check)
        {
            if (String.IsNullOrWhiteSpace(sound))
            {
                return null;
            }
            foreach (string known in ArenaAudio.EnemySoundNames)
            {
                if (String.Equals(known, sound.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return known;
                }
            }
            check.Report(label, "„" + sound + "“ ist unbekannt");
            return null;
        }

        private static IAttackPattern BuildPattern(EnemyAttackDefinition definition,
            string pattern, IAttackPattern basePattern, EnemyValueChecker check)
        {
            bool samePattern = GetPatternOf(basePattern) == pattern;
            double range = check.Double(definition.Range,
                basePattern.Range > 0 ? basePattern.Range : 60, 1, 3000, "Reichweite");
            double baseDuration = 0.2;
            if (samePattern)
            {
                if (basePattern is MeleeConeAttackPattern cone) baseDuration = cone.DurationSeconds;
                if (basePattern is SweepingArcAttackPattern arcBase) baseDuration = arcBase.DurationSeconds;
                if (basePattern is SpinningAttackPattern spinBase) baseDuration = spinBase.DurationSeconds;
                if (basePattern is SelfPulseAttackPattern pulseBase) baseDuration = pulseBase.DurationSeconds;
                if (basePattern is RadialAttackPattern waveBase) baseDuration = waveBase.DurationSeconds;
            }
            double duration = check.Double(definition.DurationSeconds, baseDuration, 0.05, 10,
                "Dauer");

            switch (pattern)
            {
                case PatternArc:
                    SweepingArcAttackPattern arc = samePattern
                        ? (SweepingArcAttackPattern)basePattern : null;
                    double arcDegrees = check.Double(definition.AngleDegrees,
                        arc != null ? arc.TotalArcRadians * 180 / Math.PI : 130, 1, 359,
                        "Winkel");
                    return new SweepingArcAttackPattern(range, arcDegrees * Math.PI / 180,
                        arc != null ? arc.HitHalfAngleRadians : Math.PI / 10, duration);
                case PatternSpin:
                    SpinningAttackPattern spin = samePattern
                        ? (SpinningAttackPattern)basePattern : null;
                    return new SpinningAttackPattern(range, spin != null ? spin.Rotations : 1,
                        spin != null ? spin.HitHalfAngleRadians : Math.PI / 10, duration);
                case PatternProjectile:
                    ProjectileAttackPattern projectile = samePattern
                        ? (ProjectileAttackPattern)basePattern : null;
                    double speed = check.Double(definition.ProjectileSpeed,
                        projectile != null ? projectile.Speed : 400, 10, 5000, "Projektiltempo");
                    double size = check.Double(definition.ProjectileSize,
                        projectile != null ? projectile.ProjectileRadius : 6, 1, 200,
                        "Projektilgröße");
                    return new ProjectileAttackPattern(range, speed, size);
                case PatternWave:
                    return new RadialAttackPattern(range, duration);
                default:
                    MeleeConeAttackPattern coneBase = samePattern
                        ? (MeleeConeAttackPattern)basePattern : null;
                    double coneDegrees = check.Double(definition.AngleDegrees,
                        coneBase != null ? coneBase.DebugHalfAngleRadians * 360 / Math.PI : 90,
                        1, 359, "Winkel");
                    return new MeleeConeAttackPattern(range, coneDegrees * Math.PI / 360, duration);
            }
        }

        private static ICombatEffect BuildEffect(EnemyAttackDefinition definition,
            Weapon template, EnemyValueChecker check)
        {
            List<ICombatEffect> effects = new List<ICombatEffect>
            {
                definition.Damage.HasValue
                    ? new DamageEffect(check.Int(definition.Damage, 10, 0, 100000, "Schaden",
                        GetTemplateDamage(template)))
                    : template.Effect
            };
            if (definition.PoisonDamagePerSecond.HasValue)
            {
                effects.Add(new PoisonEffect(
                    check.Positive(definition.PoisonDamagePerSecond, 2, 1000, "Gift pro Sekunde"),
                    check.Positive(definition.PoisonSeconds, 3, 60, "Giftdauer")));
            }
            if (definition.FireDamagePerSecond.HasValue)
            {
                effects.Add(new BurningEffect(
                    check.Positive(definition.FireDamagePerSecond, 2, 1000, "Feuer pro Sekunde"),
                    check.Positive(definition.FireSeconds, 3, 60, "Feuerdauer"), 0));
            }
            if (definition.FreezeSeconds.HasValue)
            {
                effects.Add(new FreezeEffect(
                    check.Positive(definition.FreezeSeconds, 1, 60, "Frostdauer")));
            }
            if (definition.StunSeconds.HasValue)
            {
                effects.Add(new StunEffect(
                    check.Positive(definition.StunSeconds, 1, 60, "Betäubungsdauer")));
            }
            if (definition.SlowPercent.HasValue)
            {
                effects.Add(new SlowEffect(
                    check.Double(definition.SlowPercent, 50, 0, 100, "Restgeschwindigkeit") / 100,
                    check.Positive(definition.SlowSeconds, 2, 60, "Verlangsamungsdauer")));
            }
            if (definition.WeaknessPercent.HasValue)
            {
                effects.Add(new WeaknessEffect(
                    check.Double(definition.WeaknessPercent, 60, 0, 99, "Restschaden") / 100,
                    check.Positive(definition.WeaknessSeconds, 3, 60, "Schwächedauer")));
            }

            if (definition.Lethal == true)
            {
                effects.Add(new LethalEffect());
            }
            if (definition.Devour == true)
            {
                effects.Add(new DevourEffect(check.Double(definition.DevourPullSeconds, 0.4, 0, 10,
                    "Heranziehdauer")));
            }

            ICombatEffect effect = effects.Count == 1
                ? effects[0]
                : new CompositeCombatEffect(effects.ToArray());
            if (definition.LifeStealPercent.HasValue)
            {
                double steal = check.Double(definition.LifeStealPercent, 0, 0, 100, "Lebensraub");
                if (steal > 0)
                {
                    effect = new LifeStealEffect(effect, steal / 100);
                }
            }
            return effect;
        }

        private static int GetTemplateDamage(Weapon template)
        {
            return template.Effect is DamageEffect damage ? damage.Amount : 10;
        }
    }
}
