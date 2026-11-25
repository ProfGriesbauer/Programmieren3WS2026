using System.Collections.Generic;

namespace OOPGames
{
    // One enemy kind as stored in Assets/Enemies/Types/<Id>.json. Every value except Id is
    // optional: null (or a missing JSON property) means "use the standard enemy's value",
    // see EnemyType.Resolve (Docs/Gegner-Editor-Plan.md).
    public sealed class EnemyTypeDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Enabled { get; set; }

        // ---- values ----
        public int? MaxHitPoints { get; set; }
        // Speed relative to the player's base speed.
        public double? SpeedFactor { get; set; }
        // Size of the enemy; sprite and hitbox scale with it.
        public double? Size { get; set; }
        public bool? CanBePushed { get; set; }
        // Whether others are stopped by it; false lets the player step onto it (mines).
        public bool? BlocksMovement { get; set; }
        // 0 = full knockback, 1 = none.
        public double? KnockbackResistance { get; set; }
        // Armour (< 1) or vulnerability (> 1) against all damage.
        public double? DamageTakenMultiplier { get; set; }
        // Status ids the kind ignores: poison, burning, freeze, stun, slow, weakness.
        public List<string> StatusImmunities { get; set; }
        public double? RegenerationPerSecond { get; set; }

        // ---- reward for the player ----
        // Counts as this many kills for the score (high score, kill conditions).
        public int? ScoreValue { get; set; }
        public int? WeaponPointsPerKill { get; set; }
        // Kills towards the "+1 skill point every 5 kills" milestone.
        public int? KillsTowardSkillPoint { get; set; }

        // ---- attacks (empty: the standard claws) ----
        public List<EnemyAttackDefinition> Attacks { get; set; }

        // ---- behaviour type: Chaser, Ranged, Turret, Coward ----
        public string BehaviorType { get; set; }
        public double? PreferredDistance { get; set; }
        public double? FleeBelowHealthPercent { get; set; }
        // Player (default) or RandomEntity.
        public string TargetSelection { get; set; }
        // A random target is chosen anew after this time (and when it dies).
        public double? RetargetSeconds { get; set; }
        // Walk (default) or Hop.
        public string MovementStyle { get; set; }
        public double? HopSeconds { get; set; }
        public double? HopPauseSeconds { get; set; }

        // ---- perception and behaviour ----
        public double? SightRange { get; set; }
        public double? SightBonusAfterDetection { get; set; }
        public bool? TargetsMirrorImages { get; set; }
        public double? AlertDurationSeconds { get; set; }
        public double? AlertRadius { get; set; }
        public bool? WarnsOthers { get; set; }
        public bool? CanBeWarned { get; set; }
        // Wander speed relative to the enemy's own speed.
        public double? WanderSpeedFactor { get; set; }
        public double? WanderMinSeconds { get; set; }
        public double? WanderMaxSeconds { get; set; }

        // ---- spawning (own timer per kind) ----
        public int? StartCount { get; set; }
        public double? SpawnIntervalSeconds { get; set; }
        public double? FirstSpawnDelaySeconds { get; set; }
        // Chance (0-100 %) that a spawn attempt of the timer really spawns.
        public double? SpawnChancePercent { get; set; }
        public int? MaxAlive { get; set; }
        public int? GroupSizeMin { get; set; }
        public int? GroupSizeMax { get; set; }
        public double? MinDistanceToPlayer { get; set; }
        public int? MinKills { get; set; }
        public int? MaxKills { get; set; }
        public double? MinRunSeconds { get; set; }
        // Names of spawn regions (dev tool tab "Spawn-Regionen"); empty = anywhere. Names
        // that the played map does not have are ignored; with none left: anywhere.
        public List<string> Regions { get; set; }
        // Always, PlayerInsideRegion or PlayerOutsideRegion (of the kind's regions).
        public string PlayerCondition { get; set; }

        // ---- sounds ----
        // Played now and then (e.g. a frog croaking) at a random pause in this range.
        public string IdleSound { get; set; }
        public double? IdleSoundMinSeconds { get; set; }
        public double? IdleSoundMaxSeconds { get; set; }

        // ---- look ----
        // Whether the health bar is drawn above it (e.g. not for a hidden mine).
        public bool? ShowHealthBar { get; set; }
        // Sprite sheet in Assets/Enemies (PNG with a LibreSprite JSON of the same name).
        public string SpriteSheet { get; set; }
        // Tint colour #RRGGBB laid over the sprite.
        public string Tint { get; set; }
        public double? Opacity { get; set; }
    }

    // One attack of an enemy kind. BaseWeapon picks a finished weapon (enemy-claws, sword,
    // magic-wand, bow) as template; every other value overrides it. Pattern switches to
    // another attack pattern (Cone, Arc, Spin, Projectile, Pulse, Wave).
    public sealed class EnemyAttackDefinition
    {
        public string BaseWeapon { get; set; }
        public string Pattern { get; set; }
        public double? Range { get; set; }
        // Opening angle (cone) or swept arc (arc) in degrees.
        public double? AngleDegrees { get; set; }
        public double? DurationSeconds { get; set; }
        public double? ProjectileSpeed { get; set; }
        public double? ProjectileSize { get; set; }
        public int? Damage { get; set; }
        public double? CooldownSeconds { get; set; }
        public double? Knockback { get; set; }
        public int? MaxHitsPerTarget { get; set; }

        // ---- additional effects (each active when its main value is set) ----
        public double? PoisonDamagePerSecond { get; set; }
        public double? PoisonSeconds { get; set; }
        public double? FireDamagePerSecond { get; set; }
        public double? FireSeconds { get; set; }
        public double? FreezeSeconds { get; set; }
        public double? StunSeconds { get; set; }
        // Remaining speed in percent while slowed (e.g. 50).
        public double? SlowPercent { get; set; }
        public double? SlowSeconds { get; set; }
        // Remaining damage of the target in percent while weakened (e.g. 60).
        public double? WeaknessPercent { get; set; }
        public double? WeaknessSeconds { get; set; }
        public double? LifeStealPercent { get; set; }
        // Pulls every hit entity to the attacker and kills it there (frog tongue).
        public bool? Devour { get; set; }
        public double? DevourPullSeconds { get; set; }
        // Opponents (default: the player's side) or Everyone (any entity but itself).
        public string Targets { get; set; }
        // Kills every hit entity at once (explosions).
        public bool? Lethal { get; set; }

        // ---- trigger and sequence ----
        // Distance (default) or Contact (a hitbox touches the enemy's hitbox).
        public string Trigger { get; set; }
        // Targets (default), Player, PlayerSide or Anyone.
        public string TriggeredBy { get; set; }
        // Played one after another between trigger and attack (e.g. button down, "Hello").
        public List<EnemyWindupStepDefinition> Windup { get; set; }
        // Sound group played when the attack fires (see ArenaAudio.EnemySounds).
        public string Sound { get; set; }
        // The enemy dies once this attack is over (self-destruct).
        public bool? KillsAttacker { get; set; }

        // ---- choice ----
        // Distance to the target's hitbox in which the attack is used.
        public double? UseFromDistance { get; set; }
        public double? UseUpToDistance { get; set; }
        // How often it is chosen when several attacks fit.
        public double? Weight { get; set; }
    }

    // One step of an attack's windup: a clip of the enemy's sprite sheet shown for some
    // seconds, optionally with a sound when the step begins.
    public sealed class EnemyWindupStepDefinition
    {
        public string Clip { get; set; }
        public double? Seconds { get; set; }
        public string Sound { get; set; }
    }
}
