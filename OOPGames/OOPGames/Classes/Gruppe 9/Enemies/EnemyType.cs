using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace OOPGames
{
    // A resolved enemy kind: every value of its definition, with the standard enemy's value
    // wherever the definition leaves a field empty or holds an invalid value.
    public sealed class EnemyType
    {
        public const string StandardId = "Standard";
        // Enemies spawn at least this far beyond their sight range from the player.
        public const double SpawnVisibilityMargin = 50;
        public const double MaximumSize = 400;
        // Values of the standard enemy (the enemy before enemy kinds existed).
        public const double DefaultSize = 34;
        public const double DefaultSightRange = 200;
        public const double DefaultSightBonusAfterDetection = 50;
        // A direct hit tells an NPC where the player is: it hunts the player for this long
        // (game time) or until the player was within its sight range once.
        public const double DefaultAlertDurationSeconds = 20;
        // The hit NPC warns NPCs of its kind within this radius, and they warn theirs.
        public const double DefaultAlertRadius = 100;
        public const string DefaultSpriteSheet = "Enemie1.png";
        private static readonly Regex IdPattern = new Regex("^[A-Za-z0-9_-]{1,40}$");
        private static readonly Regex SpritePattern = new Regex(@"^[A-Za-z0-9_ \-]{1,60}\.png$",
            RegexOptions.IgnoreCase);
        private static readonly Regex TintPattern = new Regex("^#[0-9A-Fa-f]{6}$");

        // Status ids an enemy can be immune to, with their German names.
        public static readonly IReadOnlyDictionary<string, string> StatusNames =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "poison", "Gift" },
                { "burning", "Feuer" },
                { "freeze", "Frost" },
                { "stun", "Betäubung" },
                { "slow", "Verlangsamung" },
                { "weakness", "Schwäche" }
            };

        // The enemy as the game had it before enemy kinds existed.
        public static readonly EnemyType Standard =
            Resolve(new EnemyTypeDefinition { Id = StandardId, Name = "Standardgegner" });

        private EnemyType()
        {
        }

        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public bool Enabled { get; private set; }

        public int MaxHitPoints { get; private set; }
        public double SpeedFactor { get; private set; }
        public double Size { get; private set; }
        public bool CanBePushed { get; private set; }
        public bool BlocksMovement { get; private set; }
        public double KnockbackResistance { get; private set; }
        public double DamageTakenMultiplier { get; private set; }
        public IReadOnlyCollection<string> StatusImmunities { get; private set; }
        public double RegenerationPerSecond { get; private set; }

        public int ScoreValue { get; private set; }
        public int WeaponPointsPerKill { get; private set; }
        public int KillsTowardSkillPoint { get; private set; }

        // Never empty; the standard enemy has the claws.
        public IReadOnlyList<EnemyAttack> Attacks { get; private set; }
        public EnemyBehavior Behavior { get; private set; }
        public double PreferredDistance { get; private set; }
        public double FleeBelowHealthPercent { get; private set; }
        public EnemyTargetSelection TargetSelection { get; private set; }
        public double RetargetSeconds { get; private set; }
        public EnemyMovementStyle MovementStyle { get; private set; }
        public double HopSeconds { get; private set; }
        public double HopPauseSeconds { get; private set; }

        // Null: the kind makes no idle sound.
        public string IdleSound { get; private set; }
        public double IdleSoundMinSeconds { get; private set; }
        public double IdleSoundMaxSeconds { get; private set; }

        public bool ShowHealthBar { get; private set; }
        public string SpriteSheet { get; private set; }
        // Null: no tint.
        public Color? Tint { get; private set; }
        public double Opacity { get; private set; }

        public double SightRange { get; private set; }
        public double SightBonusAfterDetection { get; private set; }
        public bool TargetsMirrorImages { get; private set; }
        public double AlertDurationSeconds { get; private set; }
        public double AlertRadius { get; private set; }
        public bool WarnsOthers { get; private set; }
        public bool CanBeWarned { get; private set; }
        public double WanderSpeedFactor { get; private set; }
        public double WanderMinSeconds { get; private set; }
        public double WanderMaxSeconds { get; private set; }

        public int StartCount { get; private set; }
        public double SpawnIntervalSeconds { get; private set; }
        public double FirstSpawnDelaySeconds { get; private set; }
        public double SpawnChancePercent { get; private set; }
        public int MaxAlive { get; private set; }
        public int GroupSizeMin { get; private set; }
        public int GroupSizeMax { get; private set; }
        public double MinDistanceToPlayer { get; private set; }
        public int MinKills { get; private set; }
        // Null: no upper limit.
        public int? MaxKills { get; private set; }
        public double MinRunSeconds { get; private set; }
        public IReadOnlyList<string> Regions { get; private set; }
        public EnemySpawnPlayerCondition PlayerCondition { get; private set; }

        public static bool IsValidId(string id)
        {
            return id != null && IdPattern.IsMatch(id);
        }

        public static EnemyType Resolve(EnemyTypeDefinition definition)
        {
            return Resolve(definition, null);
        }

        // Invalid values fall back to the standard value; each one is described in problems
        // (German, for the dev tool) when a list is given. An invalid id cannot be repaired.
        public static EnemyType Resolve(
            EnemyTypeDefinition definition, ICollection<string> problems)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }
            if (!IsValidId(definition.Id))
            {
                throw new ArgumentException(
                    "Die Gegner-Id darf nur Buchstaben, Ziffern, - und _ enthalten (1–40 Zeichen).",
                    nameof(definition));
            }

            EnemyValueChecker check = new EnemyValueChecker(definition.Id, problems);
            EnemyType type = new EnemyType
            {
                Id = definition.Id,
                Name = String.IsNullOrWhiteSpace(definition.Name)
                    ? definition.Id
                    : definition.Name.Trim(),
                Description = definition.Description ?? String.Empty,
                Enabled = definition.Enabled ?? true,

                MaxHitPoints = check.Int(definition.MaxHitPoints, 30, 1, 100000,
                    "Lebenspunkte"),
                SpeedFactor = check.Double(definition.SpeedFactor, 0.75, 0, 10,
                    "Tempo-Faktor"),
                Size = check.Double(definition.Size, DefaultSize, 4, MaximumSize,
                    "Größe"),
                CanBePushed = definition.CanBePushed ?? true,
                BlocksMovement = definition.BlocksMovement ?? true,
                ShowHealthBar = definition.ShowHealthBar ?? true,
                KnockbackResistance = check.Double(definition.KnockbackResistance, 0, 0, 1,
                    "Rückstoßresistenz"),
                DamageTakenMultiplier = check.Double(definition.DamageTakenMultiplier, 1, 0, 10,
                    "Schadensfaktor"),
                RegenerationPerSecond = check.Double(definition.RegenerationPerSecond, 0, 0,
                    10000, "Regeneration"),
                ScoreValue = check.Int(definition.ScoreValue, 1, 0, 1000, "Zählt als Kills"),
                WeaponPointsPerKill = check.Int(definition.WeaponPointsPerKill, 1, 0, 1000,
                    "Waffenpunkte pro Kill"),
                KillsTowardSkillPoint = check.Int(definition.KillsTowardSkillPoint, 1, 0, 1000,
                    "Kills für Skillpunkte"),
                PreferredDistance = check.Double(definition.PreferredDistance, 150, 0, 5000,
                    "Wunschabstand"),
                FleeBelowHealthPercent = check.Double(definition.FleeBelowHealthPercent, 30,
                    0, 100, "Flucht unter Leben"),
                Opacity = check.Double(definition.Opacity, 1, 0.05, 1, "Deckkraft"),
                RetargetSeconds = check.Double(definition.RetargetSeconds, 10, 0.5, 600,
                    "Neues Ziel nach"),
                HopSeconds = check.Double(definition.HopSeconds, 0.4, 0.05, 60, "Sprungdauer"),
                HopPauseSeconds = check.Double(definition.HopPauseSeconds, 0.8, 0, 60,
                    "Sprungpause"),

                SightRange = check.Double(definition.SightRange, DefaultSightRange,
                    0, 5000, "Sichtweite"),
                SightBonusAfterDetection = check.Double(definition.SightBonusAfterDetection,
                    DefaultSightBonusAfterDetection, 0, 5000,
                    "Sichtbonus nach Entdeckung"),
                TargetsMirrorImages = definition.TargetsMirrorImages ?? true,
                AlertDurationSeconds = check.Double(definition.AlertDurationSeconds,
                    DefaultAlertDurationSeconds, 0, 600, "Alarmdauer"),
                AlertRadius = check.Double(definition.AlertRadius,
                    DefaultAlertRadius, 0, 5000, "Alarmradius"),
                WarnsOthers = definition.WarnsOthers ?? true,
                CanBeWarned = definition.CanBeWarned ?? true,
                WanderSpeedFactor = check.Double(definition.WanderSpeedFactor, 0.5, 0, 10,
                    "Streun-Tempo"),
                WanderMinSeconds = check.Double(definition.WanderMinSeconds, 1.5, 0.1, 600,
                    "Streunen min."),

                StartCount = check.Int(definition.StartCount, 1, 0, ArenaField.MaximumEnemyCount,
                    "Anzahl zu Run-Beginn"),
                SpawnIntervalSeconds = check.Double(definition.SpawnIntervalSeconds,
                    ArenaField.EnemySpawnIntervalSeconds, 0.1, 3600, "Spawn-Takt"),
                SpawnChancePercent = check.Double(definition.SpawnChancePercent, 100, 0, 100,
                    "Spawn-Wahrscheinlichkeit"),
                MaxAlive = check.Int(definition.MaxAlive, ArenaField.MaximumEnemyCount,
                    0, 1000, "Max. gleichzeitig"),
                GroupSizeMin = check.Int(definition.GroupSizeMin, 1, 1,
                    ArenaField.MaximumEnemyCount, "Gruppengröße min."),
                MinKills = check.Int(definition.MinKills, 0, 0, 100000, "Ab Kills"),
                MinRunSeconds = check.Double(definition.MinRunSeconds, 0, 0, 86400,
                    "Ab Spielzeit")
            };

            double standardWanderMax = Math.Max(4, type.WanderMinSeconds);
            type.WanderMaxSeconds = check.Double(definition.WanderMaxSeconds,
                standardWanderMax, type.WanderMinSeconds, 600, "Streunen max.");
            type.FirstSpawnDelaySeconds = check.Double(definition.FirstSpawnDelaySeconds,
                type.SpawnIntervalSeconds, 0, 86400, "Erster Spawn nach",
                type.SpawnIntervalSeconds);
            type.GroupSizeMax = check.Int(definition.GroupSizeMax, type.GroupSizeMin,
                type.GroupSizeMin, ArenaField.MaximumEnemyCount, "Gruppengröße max.",
                type.GroupSizeMin);
            double defaultDistance = type.SightRange + SpawnVisibilityMargin;
            type.MinDistanceToPlayer = check.Double(definition.MinDistanceToPlayer,
                defaultDistance, 0, 10000, "Mindestabstand zum Spieler", defaultDistance);
            type.StatusImmunities = ResolveImmunities(definition.StatusImmunities, check);
            type.Regions = (definition.Regions ?? new List<string>())
                .Where(name => !String.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
            type.PlayerCondition = ResolvePlayerCondition(
                definition.PlayerCondition, type.Regions.Count > 0, check);
            type.Behavior = ResolveBehavior(definition.BehaviorType, check);
            type.TargetSelection = ResolveEnum(definition.TargetSelection,
                EnemyTargetSelection.Player, "Zielwahl", check);
            type.MovementStyle = ResolveEnum(definition.MovementStyle,
                EnemyMovementStyle.Walk, "Bewegungsart", check);
            type.Attacks = ResolveAttacks(definition, check);
            type.IdleSound = EnemyAttack.ResolveSound(definition.IdleSound, "Gelegentlicher Sound", check);
            type.IdleSoundMinSeconds = check.Double(definition.IdleSoundMinSeconds, 2, 0.1, 600,
                "Sound frühestens");
            type.IdleSoundMaxSeconds = check.Double(definition.IdleSoundMaxSeconds,
                Math.Max(6, type.IdleSoundMinSeconds), type.IdleSoundMinSeconds, 600,
                "Sound spätestens");
            type.SpriteSheet = DefaultSpriteSheet;
            if (!String.IsNullOrWhiteSpace(definition.SpriteSheet))
            {
                if (SpritePattern.IsMatch(definition.SpriteSheet.Trim()))
                {
                    type.SpriteSheet = definition.SpriteSheet.Trim();
                }
                else
                {
                    check.Report("Spritesheet", "muss ein PNG-Dateiname aus Assets/Enemies sein");
                }
            }
            if (!String.IsNullOrWhiteSpace(definition.Tint))
            {
                if (TintPattern.IsMatch(definition.Tint.Trim()))
                {
                    type.Tint = (Color)ColorConverter.ConvertFromString(definition.Tint.Trim());
                }
                else
                {
                    check.Report("Tönung", "muss die Form #RRGGBB haben");
                }
            }
            if (definition.MaxKills.HasValue && definition.MaxKills.Value <= type.MinKills)
            {
                check.Report("Bis Kills", "muss größer als „Ab Kills“ sein");
            }
            else
            {
                type.MaxKills = definition.MaxKills;
            }
            return type;
        }

        private static IReadOnlyCollection<string> ResolveImmunities(
            IEnumerable<string> immunities, EnemyValueChecker check)
        {
            HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string id in immunities ?? Enumerable.Empty<string>())
            {
                if (id != null && StatusNames.ContainsKey(id.Trim()))
                {
                    result.Add(id.Trim().ToLowerInvariant());
                }
                else
                {
                    check.Report("Immunität", "„" + id + "“ ist unbekannt");
                }
            }
            return result;
        }

        private static EnemySpawnPlayerCondition ResolvePlayerCondition(
            string condition, bool hasRegions, EnemyValueChecker check)
        {
            if (String.IsNullOrWhiteSpace(condition))
            {
                return EnemySpawnPlayerCondition.Always;
            }
            if (!Enum.TryParse(condition.Trim(), true, out EnemySpawnPlayerCondition result)
                || !Enum.IsDefined(typeof(EnemySpawnPlayerCondition), result))
            {
                check.Report("Spielerbedingung", "„" + condition + "“ ist unbekannt");
                return EnemySpawnPlayerCondition.Always;
            }
            if (result != EnemySpawnPlayerCondition.Always && !hasRegions)
            {
                check.Report("Spielerbedingung", "braucht mindestens eine Spawn-Region");
                return EnemySpawnPlayerCondition.Always;
            }
            return result;
        }

        internal static TEnum ResolveEnum<TEnum>(string value, TEnum standard, string label,
            EnemyValueChecker check) where TEnum : struct
        {
            if (String.IsNullOrWhiteSpace(value))
            {
                return standard;
            }
            if (Enum.TryParse(value.Trim(), true, out TEnum result)
                && Enum.IsDefined(typeof(TEnum), result))
            {
                return result;
            }
            check.Report(label, "„" + value + "“ ist unbekannt");
            return standard;
        }

        private static EnemyBehavior ResolveBehavior(string behavior, EnemyValueChecker check)
        {
            if (String.IsNullOrWhiteSpace(behavior))
            {
                return EnemyBehavior.Chaser;
            }
            if (Enum.TryParse(behavior.Trim(), true, out EnemyBehavior result)
                && Enum.IsDefined(typeof(EnemyBehavior), result))
            {
                return result;
            }
            check.Report("Verhalten", "„" + behavior + "“ ist unbekannt");
            return EnemyBehavior.Chaser;
        }

        private static IReadOnlyList<EnemyAttack> ResolveAttacks(
            EnemyTypeDefinition definition, EnemyValueChecker check)
        {
            if (definition.Attacks == null || definition.Attacks.Count == 0)
            {
                return new[] { EnemyAttack.StandardClaws };
            }
            List<EnemyAttack> attacks = new List<EnemyAttack>();
            for (int i = 0; i < definition.Attacks.Count; i++)
            {
                EnemyAttackDefinition attack = definition.Attacks[i]
                    ?? new EnemyAttackDefinition();
                attacks.Add(EnemyAttack.Resolve(attack,
                    "enemy-" + definition.Id + "-" + (i + 1),
                    check.For("Angriff " + (i + 1))));
            }
            return attacks;
        }
    }
}
