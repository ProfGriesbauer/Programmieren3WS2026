using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OOPGames;

internal static class Program
{
    private static int _assertions;

    [STAThread]
    private static int Main()
    {
        try
        {
            TestDamageHealingAndDeath();
            TestSlowAndFreezeExpire();
            TestMeleeTargetsAndCooldown();
            TestEnemyUsesSharedWeaponSystem();
            TestProjectileAndFreezeEffect();
            TestDamageOverTimeAndFireSpread();
            TestHealingWeapon();
            TestProgressionAndMagicWandUpgrades();
            TestBowAutoAimAndUpgrades();
            TestSwordUpgradesAndSpin();
            TestMultiplePlayerAbilities();
            TestKeyBindingPersistenceAndConflicts();
            TestInputSeparatesAbilityTypes();
            TestWeaponWheelHoldAndReleaseSelection();
            TestKillsGrantRunProgression();
            TestWeaponSelectionSurvivesRestart();
            TestEquipmentMenuSelection();
            TestProgressionAndKeyBindingMenus();
            TestRangeKnockbackStatsAndGodMode();

            Console.WriteLine("OK: " + _assertions + " arena assertions passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static void TestDamageHealingAndDeath()
    {
        TestEntity target = new TestEntity(100, 100, 30, 10, CombatTeam.Enemies);
        new DamageEffect(12).Apply(null, target);
        Assert(target.CurrentHitPoints == 18, "damage effect reduces hit points");

        new HealingEffect(50).Apply(null, target);
        Assert(target.CurrentHitPoints == target.MaxHitPoints,
            "healing is clamped to maximum hit points");

        target.TakeDamage(target.MaxHitPoints);
        new HealingEffect(25).Apply(null, target);
        Assert(!target.IsAlive && target.CurrentHitPoints == 0,
            "healing does not resurrect a defeated entity");
    }

    private static void TestSlowAndFreezeExpire()
    {
        Rect bounds = new Rect(0, 0, 1000, 1000);
        TestEntity target = new TestEntity(100, 100, 30, 10, CombatTeam.Enemies);

        new SlowEffect(0.5, 1).Apply(null, target);
        target.Move(1, 0, bounds);
        Assert(Math.Abs(target.PositionX - 105) < 0.001,
            "slow modifies effective movement speed");
        target.UpdateStatusEffects(1);
        target.Move(1, 0, bounds);
        Assert(Math.Abs(target.PositionX - 115) < 0.001,
            "movement speed returns after slow expires");

        new FreezeEffect(0.5).Apply(null, target);
        target.Move(1, 0, bounds);
        Assert(Math.Abs(target.PositionX - 115) < 0.001,
            "freeze prevents movement");
        target.UpdateStatusEffects(0.5);
        target.Move(1, 0, bounds);
        Assert(Math.Abs(target.PositionX - 125) < 0.001,
            "movement resumes after freeze expires");
    }

    private static void TestMeleeTargetsAndCooldown()
    {
        TestEntity player = new TestEntity(100, 100, 100, 10, CombatTeam.Player)
        {
            EquippedWeapon = WeaponCatalog.Sword
        };
        TestEntity enemy = new TestEntity(150, 100, 30, 10, CombatTeam.Enemies);
        TestEntity ally = new TestEntity(150, 300, 30, 10, CombatTeam.Player);
        TestEntity outsideCone = new TestEntity(130, 205, 30, 10, CombatTeam.Enemies);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        IReadOnlyList<Entity> entities = new List<Entity>
        {
            player, enemy, ally, outsideCone
        };

        Assert(combat.TryStartAttack(player, 1, 0), "melee attack starts");
        Assert(!combat.TryStartAttack(player, 1, 0),
            "weapon cooldown rejects an immediate repeated attack");
        for (int i = 0; i < 16 && enemy.CurrentHitPoints == enemy.MaxHitPoints; i++)
        {
            combat.Update(0.04, entities);
        }
        Assert(enemy.CurrentHitPoints == 16,
            "melee cone applies one hit to an opponent");
        Assert(enemy.PositionX > 150 && enemy.PositionX <= 158,
            "weapon hits apply a light knockback");
        Assert(ally.CurrentHitPoints == 30 && outsideCone.CurrentHitPoints == 30,
            "melee cone excludes allies and targets outside its range");

        for (int i = 0; i < 5; i++)
        {
            combat.Update(0.04, entities);
        }
        Assert(enemy.CurrentHitPoints == 16,
            "one swing cannot hit the same target repeatedly");
    }

    private static void TestProjectileAndFreezeEffect()
    {
        TestEntity player = new TestEntity(100, 100, 100, 10, CombatTeam.Player)
        {
            EquippedWeapon = CreateTestWeapon(
                "test-projectile", new ProjectileAttackPattern(520, 720, 7),
                new DamageEffect(12))
        };
        TestEntity enemy = new TestEntity(220, 100, 30, 10, CombatTeam.Enemies);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        IReadOnlyList<Entity> entities = new List<Entity> { player, enemy };

        Assert(combat.TryStartAttack(player, 1, 0), "projectile attack starts");
        for (int i = 0; i < 8 && enemy.CurrentHitPoints == 30; i++)
        {
            combat.Update(0.04, entities);
        }
        Assert(enemy.CurrentHitPoints == 18,
            "moving projectile damages the first opponent it intersects");

        for (int i = 0; i < 10; i++)
        {
            combat.Update(0.04, entities);
        }
        player.EquippedWeapon = CreateTestWeapon(
            "test-freeze", new ProjectileAttackPattern(400, 520, 9),
            new CompositeCombatEffect(
                new DamageEffect(2), new FreezeEffect(0.9)));
        Assert(combat.TryStartAttack(player, 1, 0), "freeze projectile starts after cooldown");
        for (int i = 0; i < 12 && enemy.ActiveStatusEffects.Count == 0; i++)
        {
            combat.Update(0.04, entities);
        }
        Assert(enemy.EffectiveSpeed == 0,
            "freeze effect temporarily stops the target");
    }

    private static void TestEnemyUsesSharedWeaponSystem()
    {
        PlayerCharacter player = new PlayerCharacter(100, 100);
        PursuerEnemy enemy = new PursuerEnemy(150, 100, player.MaxSpeed);
        ArenaCombatSystem combat = new ArenaCombatSystem();

        Assert(enemy.TryAttackPlayerInRange(player, combat),
            "enemy starts its equipped weapon attack in range");
        combat.Update(0.04, new List<Entity> { player, enemy });
        Assert(player.CurrentHitPoints == player.MaxHitPoints - 10,
            "enemy weapon applies damage through the shared combat system");
        Assert(player.PositionX < 100,
            "enemy weapon knockback pushes the player away from the attacker");
    }

    private static void TestHealingWeapon()
    {
        TestEntity player = new TestEntity(100, 100, 100, 10, CombatTeam.Player)
        {
            EquippedWeapon = PlayerAbilityCatalog.CreateWeapon(
                PlayerAbilityCatalog.HealingPulse, 1)
        };
        player.TakeDamage(40);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        Assert(combat.TryStartAttack(player, 0, 0),
            "self-targeted healing attack does not require an aim vector");
        combat.Update(0.04, new List<Entity> { player });
        Assert(player.CurrentHitPoints == 80,
            "healing weapon restores health without exceeding its maximum");
    }

    private static void TestDamageOverTimeAndFireSpread()
    {
        TestEntity source = new TestEntity(100, 100, 100, 10, CombatTeam.Player);
        TestEntity firstEnemy = new TestEntity(150, 100, 50, 10, CombatTeam.Enemies);
        TestEntity nearbyEnemy = new TestEntity(205, 100, 50, 10, CombatTeam.Enemies);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        IReadOnlyList<Entity> entities = new List<Entity>
        {
            source, firstEnemy, nearbyEnemy
        };

        new BurningEffect(4, 2, 70).Apply(source, firstEnemy);
        combat.Update(0.04, entities);
        Assert(firstEnemy.ActiveDamageOverTimeEffects.Any(
            effect => effect.Id == "burning"),
            "fire applies a damage-over-time status");
        Assert(nearbyEnemy.ActiveDamageOverTimeEffects.Any(
            effect => effect.Id == "burning"),
            "burning spreads to nearby entities");

        int startingHealth = firstEnemy.CurrentHitPoints;
        combat.Update(1, entities);
        Assert(firstEnemy.CurrentHitPoints < startingHealth,
            "damage-over-time effects deal damage during combat updates");
    }

    private static void TestProgressionAndMagicWandUpgrades()
    {
        ArenaProgression progression = new ArenaProgression();
        Assert(progression.UniversalPoints == 10,
            "a new run starts with ten universal skill points");

        for (int i = 0; i < 4; i++)
        {
            progression.AwardKill(WeaponCatalog.MagicWand);
        }
        Assert(progression.UniversalPoints == 10
            && progression.GetWeaponPoints(WeaponCatalog.MagicWand.Id) == 4,
            "each wand kill grants one wand point before universal milestone");

        progression.AwardKill(WeaponCatalog.MagicWand);
        Assert(progression.UniversalPoints == 11
            && progression.GetWeaponPoints(WeaponCatalog.MagicWand.Id) == 5,
            "every fifth kill grants one universal point");

        Assert(progression.TryUpgradeMagicWand(
            MagicWandUpgradeCatalog.Strike, UpgradeCurrency.Universal),
            "universal points unlock the wand strike ability");
        Assert(progression.TryUpgradeMagicWand(
            MagicWandUpgradeCatalog.StrikeDamage, UpgradeCurrency.Universal),
            "unlocked wand strike damage can be upgraded");
        Assert(progression.GetWeaponAbilities(WeaponCatalog.MagicWand.Id)[0]
            == MagicWandUpgradeCatalog.StrikeAbility,
            "unlocked wand strike is assigned to a weapon ability slot");
        Assert(progression.TryUpgradeMagicWand(
            MagicWandUpgradeCatalog.ProjectileDamage, UpgradeCurrency.Weapon),
            "main projectile damage can be upgraded with wand points");
        Assert(progression.TryUpgradeMagicWand(
            MagicWandUpgradeCatalog.Fire, UpgradeCurrency.Weapon),
            "one elemental branch can be selected with wand points");
        Assert(!progression.TryUpgradeMagicWand(
            MagicWandUpgradeCatalog.Poison, UpgradeCurrency.Universal),
            "elemental upgrade branches are mutually exclusive");
        Assert(progression.TryUpgradeMagicWand(
            MagicWandUpgradeCatalog.ElementPower, UpgradeCurrency.Universal),
            "universal points can improve the selected element");

        Weapon upgradedWand = MagicWandUpgradeCatalog.CreateMagicWand(progression);
        Weapon wandStrike = progression.GetWeaponAbilityWeapon(
            WeaponCatalog.MagicWand.Id, 0);
        Assert(upgradedWand.AttackPattern is ProjectileAttackPattern
            && upgradedWand.Effect.Kind == CombatEffectKind.Mixed,
            "wand main attack shoots projectiles with the selected element");
        Assert(wandStrike.AttackPattern is MeleeConeAttackPattern strikePattern
            && strikePattern.Range >= 145
            && strikePattern.DebugHalfAngleRadians <= Math.PI / 36,
            "wand strike upgrade is long, narrow, and aimed along player direction");
    }

    private static void TestBowAutoAimAndUpgrades()
    {
        ArenaProgression progression = new ArenaProgression();
        for (int i = 0; i < 15; i++)
        {
            progression.AwardKill(WeaponCatalog.Bow);
        }

        double previousAngle = 0;
        for (int rank = 1; rank <= 5; rank++)
        {
            Assert(progression.TryUpgradeBow(
                BowUpgradeCatalog.AutoAim, UpgradeCurrency.Weapon),
                "bow autoaim can be improved with bow points");
            double angle = BowUpgradeCatalog.GetAutoAimHalfAngleRadians(rank);
            Assert(angle > previousAngle,
                "each bow autoaim rank widens the target acquisition cone");
            previousAngle = angle;
        }
        Assert(Math.Abs(previousAngle - Math.PI) < 0.001,
            "maximum bow autoaim covers a full 360-degree circle");

        Weapon bow = BowUpgradeCatalog.CreateBow(progression);
        ProjectileAttackPattern bowPattern =
            bow.AttackPattern as ProjectileAttackPattern;
        Assert(bowPattern != null
            && bowPattern.HomingTurnRateRadiansPerSecond > 0,
            "bow fires projectiles with progressively stronger homing");

        TestEntity archer = new TestEntity(100, 100, 100, 10, CombatTeam.Player);
        TestEntity enemyBehind = new TestEntity(50, 100, 60, 10, CombatTeam.Enemies);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        Assert(combat.TryStartAttack(archer, bow, 1, 0),
            "bow projectile starts while aiming forward");
        for (int i = 0; i < 120 && enemyBehind.CurrentHitPoints == 60; i++)
        {
            combat.Update(0.02, new List<Entity> { archer, enemyBehind });
        }
        Assert(enemyBehind.CurrentHitPoints < 60,
            "maximum autoaim can turn a projectile toward an enemy behind the archer");

        Assert(progression.TryUpgradeBow(
            BowUpgradeCatalog.ArrowDamage, UpgradeCurrency.Universal)
            && progression.TryUpgradeBow(
                BowUpgradeCatalog.Fire, UpgradeCurrency.Universal),
            "bow can unlock arrow damage and an elemental upgrade");
        Assert(BowUpgradeCatalog.CreateBow(progression).Effect.Kind
            == CombatEffectKind.Mixed,
            "bow projectile combines damage with its selected element");
    }

    private static void TestSwordUpgradesAndSpin()
    {
        ArenaProgression progression = new ArenaProgression();
        Assert(progression.TryUpgradeSword(
            SwordUpgradeCatalog.StrikeDamage, UpgradeCurrency.Universal),
            "sword strike damage can be upgraded");
        Assert(progression.TryUpgradeSword(
            SwordUpgradeCatalog.Spin, UpgradeCurrency.Universal),
            "sword spin is unlocked from its damage branch");
        Assert(progression.GetWeaponAbilities(WeaponCatalog.Sword.Id)[0]
            == SwordUpgradeCatalog.SpinAbility,
            "the purchased spin is stored as a sword ability, not a player skill");
        Assert(progression.TryUpgradeSword(
            SwordUpgradeCatalog.Fire, UpgradeCurrency.Universal),
            "one selectable sword element can be purchased");
        Assert(!progression.TryUpgradeSword(
            SwordUpgradeCatalog.Poison, UpgradeCurrency.Universal),
            "sword element branches are mutually exclusive");

        Weapon upgradedSword = SwordUpgradeCatalog.CreateSword(progression);
        Weapon spin = progression.GetWeaponAbilityWeapon(
            WeaponCatalog.Sword.Id, 0);
        Assert(upgradedSword.AttackPattern is SweepingArcAttackPattern
            && upgradedSword.Effect.Kind == CombatEffectKind.Mixed,
            "the upgraded sword retains its sweep and adds its selected effect");
        Assert(spin.AttackPattern is SpinningAttackPattern
            && spin.Effect.Kind == CombatEffectKind.Mixed,
            "the spin is a distinct elemental weapon ability");

        TestEntity player = new TestEntity(100, 100, 100, 10, CombatTeam.Player);
        TestEntity enemy = new TestEntity(180, 100, 100, 10, CombatTeam.Enemies);
        Weapon spinDamageFixture = new Weapon(
            "spin-damage-fixture", "Spin fixture", string.Empty,
            spin.AttackPattern, new DamageEffect(8), 4.5,
            WeaponTargetingMode.Opponents, "sword-spin", 3);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        Assert(combat.TryStartAttack(player, spinDamageFixture, 1, 0),
            "three-turn spin starts");
        for (int i = 0; i < 18; i++)
        {
            combat.Update(0.05, new List<Entity> { player, enemy });
        }
        Assert(enemy.CurrentHitPoints == 76,
            "the spinning slash applies one direct hit per rotation");
    }

    private static void TestMultiplePlayerAbilities()
    {
        ArenaProgression progression = new ArenaProgression();
        Assert(progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast),
            "Power Blast can be unlocked with universal points");
        Assert(progression.TryUpgradeSkill(PlayerAbilityCatalog.HealingPulse),
            "healing can also be unlocked in the same run");
        Assert(progression.EntityAbilities[0] == PlayerAbilityCatalog.PowerBlast
            && progression.EntityAbilities[1] == PlayerAbilityCatalog.HealingPulse,
            "multiple player abilities occupy slots separate from weapon abilities");

        TestEntity player = new TestEntity(100, 100, 100, 10, CombatTeam.Player);
        TestEntity enemy = new TestEntity(150, 100, 50, 10, CombatTeam.Enemies);
        player.TakeDamage(40);
        ArenaCombatSystem combat = new ArenaCombatSystem();

        Weapon blast = progression.GetEntityAbilityWeapon(0);
        Weapon heal = progression.GetEntityAbilityWeapon(1);
        Assert(combat.TryStartAttack(player, blast, 1, 0)
            && combat.TryStartAttack(player, heal, 0, 0),
            "different player abilities can activate independently");
        combat.Update(0.04, new List<Entity> { player, enemy });
        Assert(enemy.CurrentHitPoints == 30
            && player.CurrentHitPoints == 80,
            "Power Blast damages nearby enemies while healing restores player health");
    }

    private static void TestKeyBindingPersistenceAndConflicts()
    {
        string path = Path.Combine(
            Path.GetTempPath(), "arena-keybindings-" + Guid.NewGuid() + ".json");
        try
        {
            ArenaKeyBindings bindings = ArenaKeyBindings.Load(path);
            Assert(bindings.GetDisplayName(ArenaAction.WeaponAttack)
                    == "Linke Maustaste"
                && bindings.GetDisplayName(ArenaAction.WeaponAbility1)
                    == "Rechte Maustaste",
                "weapon attack defaults to left click and the first ability to right click");
            bindings.SetKey(ArenaAction.MoveUp, Key.I);
            bindings.SetMouseButton(ArenaAction.WeaponAttack, MouseButton.XButton1);
            bindings.Save(path);
            ArenaKeyBindings loaded = ArenaKeyBindings.Load(path);
            Assert(loaded.GetKey(ArenaAction.MoveUp) == Key.I,
                "custom key bindings persist to the settings file");
            Assert(loaded.MatchesMouseButton(
                ArenaAction.WeaponAttack, MouseButton.XButton1),
                "custom mouse-button bindings persist to the settings file");

            bool rejectedConflict = false;
            try
            {
                loaded.SetMouseButton(
                    ArenaAction.EntityAbility1, MouseButton.XButton1);
            }
            catch (InvalidOperationException)
            {
                rejectedConflict = true;
            }
            Assert(rejectedConflict,
                "one mouse button cannot be assigned to multiple actions");

            File.WriteAllText(path,
                "{\"PrimaryAttack\":\"Space\",\"Ability1\":\"Q\"}");
            ArenaKeyBindings migrated = ArenaKeyBindings.Load(path);
            Assert(migrated.MatchesMouseButton(
                    ArenaAction.WeaponAttack, MouseButton.Left)
                && migrated.GetKey(ArenaAction.EntityAbility1) == Key.Q,
                "legacy bindings migrate to the new weapon and player ability actions");

            File.WriteAllText(path,
                "{\"WeaponAttack\":{\"Device\":\"Mouse\",\"Key\":\"None\",\"MouseButton\":\"Right\"},"
                + "\"WeaponAbility1\":{\"Device\":\"Mouse\",\"Key\":\"None\",\"MouseButton\":\"Left\"}}");
            ArenaKeyBindings swapped = ArenaKeyBindings.Load(path);
            Assert(swapped.MatchesMouseButton(
                    ArenaAction.WeaponAttack, MouseButton.Left)
                && swapped.MatchesMouseButton(
                    ArenaAction.WeaponAbility1, MouseButton.Right),
                "previous default mouse bindings migrate without discarding other settings");

            File.WriteAllText(path,
                "{\"WeaponAbility2\":{\"Device\":\"Keyboard\",\"Key\":\"D1\"}}");
            ArenaKeyBindings migratedWeaponWheel = ArenaKeyBindings.Load(path);
            Assert(migratedWeaponWheel.GetKey(ArenaAction.WeaponWheel) == Key.D1
                && migratedWeaponWheel.GetKey(ArenaAction.WeaponAbility2) == Key.D2,
                "saved ability bindings migrate away from the new weapon-wheel key");
        }
        finally
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static void TestInputSeparatesAbilityTypes()
    {
        ArenaField field = new ArenaField();
        field.KeyBindings.ResetToDefaults();
        field.StartNewGame();
        Assert(field.TryUpgradeWeapon(
            SwordUpgradeCatalog.StrikeDamage, UpgradeCurrency.Universal)
            && field.TryUpgradeWeapon(
                SwordUpgradeCatalog.Spin, UpgradeCurrency.Universal),
            "sword weapon ability can be purchased during a run");
        Assert(field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast),
            "player ability can be purchased independently from the sword ability");
        field.SetAimDirection(1, 0);

        Assert(field.HandleMouseDown(MouseButton.Right),
            "right click activates the first purchased weapon ability");
        Assert(field.HandleKeyDown(Key.Q),
            "entity ability uses its separate keyboard binding");
        Assert(field.HandleMouseDown(MouseButton.Left),
            "left click activates the weapon's standard attack");
        Assert(field.ActiveAttackCount == 3,
            "weapon ability, player ability, and basic attack run independently");
        field.UpdateCombat(0.1);
        Assert(field.IsPlayerSpinning
            && field.PlayerSpinFacing == PlayerFacing.Front,
            "spin animation advances to the matching front-facing sprite");
    }

    private static void TestWeaponWheelHoldAndReleaseSelection()
    {
        ArenaField field = new ArenaField();
        field.KeyBindings.ResetToDefaults();
        field.StartNewGame();
        Assert(field.KeyBindings.GetKey(ArenaAction.WeaponWheel) == Key.D1
            && field.KeyBindings.GetKey(ArenaAction.WeaponAbility2) == Key.D2,
            "weapon wheel defaults to 1 without colliding with weapon ability bindings");

        field.SetAimDirection(0, -1);
        Assert(field.HandleKeyDown(Key.D1) && field.IsWeaponWheelOpen,
            "holding 1 opens the weapon wheel during gameplay");
        Assert(Math.Abs(field.GameTimeScale
            - ArenaField.WeaponWheelSlowMotionScale) < 0.001,
            "the active weapon wheel slows game time");
        Assert(field.WeaponWheelSelectionIndex == 0,
            "mouse direction selects the top weapon segment");

        string previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = FindGameProjectDirectory();
            PainterG9 painter = new PainterG9();
            Canvas canvas = new Canvas { Width = 1024, Height = 768 };
            canvas.Measure(new Size(1024, 768));
            canvas.Arrange(new Rect(0, 0, 1024, 768));
            painter.PaintGameField(canvas, field);
            Assert(canvas.Children.OfType<System.Windows.Shapes.Path>().Count() == 3
                && canvas.Children.OfType<TextBlock>()
                    .Any(label => label.Text == WeaponCatalog.Sword.Name),
                "weapon wheel renders a hollow radial segment for each weapon");
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }

        field.SetAimDirection(1, 1);
        Assert(field.WeaponWheelSelectionIndex == 1
            && field.SelectedPlayerWeapon.Id == WeaponCatalog.Sword.Id,
            "moving the mouse changes the highlighted segment without switching weapons");
        Assert(field.HandleKeyUp(Key.D1)
            && !field.IsWeaponWheelOpen
            && field.SelectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id,
            "releasing 1 equips the weapon under the mouse direction");

        field.HandleKeyDown(Key.D1);
        field.SetAimDirection(-1, 0);
        field.HandleKeyDown(Key.Escape);
        Assert(!field.IsWeaponWheelOpen
            && field.SelectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id,
            "Escape closes the wheel without changing the equipped weapon");

        TestEntity slowMover = new TestEntity(
            100, 100, 20, 10, CombatTeam.Player);
        slowMover.Move(1, 0, new Rect(0, 0, 1000, 1000),
            ArenaField.WeaponWheelSlowMotionScale);
        Assert(Math.Abs(slowMover.PositionX - 101.5) < 0.001,
            "entity movement applies the weapon-wheel slow-motion scale");
    }

    private static void TestKillsGrantRunProgression()
    {
        ArenaField field = new ArenaField();
        field.SelectPlayerWeapon(WeaponCatalog.MagicWand);
        field.StartNewGame();
        for (int kill = 0; kill < ArenaProgression.KillsPerUniversalPoint; kill++)
        {
            PursuerEnemy enemy = field.Enemies[0];
            enemy.PositionX = field.Player.PositionX + 50;
            enemy.PositionY = field.Player.PositionY;

            int attacks = 0;
            while (enemy.IsAlive && attacks < 10)
            {
                field.AttackWithPlayer(1, 0);
                field.UpdateCombat(0.04);
                while (field.GetAttackCooldownRemaining(field.Player) > 0)
                {
                    field.UpdateCombat(0.04);
                }
                attacks++;
            }
            Assert(!enemy.IsAlive, "repeated wand attacks defeat the enemy");
        }

        Assert(field.Score == 5 && field.Progression.TotalKills == 5,
            "player kills update both score and progression");
        Assert(field.Progression.UniversalPoints == 11
            && field.Progression.GetWeaponPoints(WeaponCatalog.MagicWand.Id) == 5,
            "five kills grant a universal point and one wand point per kill");
    }

    private static void TestWeaponSelectionSurvivesRestart()
    {
        ArenaField field = new ArenaField();
        field.SelectPlayerWeapon(WeaponCatalog.MagicWand);
        field.HandleZoomWheel(120);
        field.ToggleDamageRanges();
        field.ToggleDebugHud();
        Assert(field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast),
            "progression can be spent before a run restart");
        double zoomLevel = field.ZoomLevel;

        field.RestartGame();
        Assert(field.Player.EquippedWeapon.Id == WeaponCatalog.MagicWand.Id,
            "selected real weapon is equipped after restart");
        Assert(field.ZoomLevel == zoomLevel
            && field.ShowDamageRanges && field.ShowDebugHud,
            "restart preserves zoom and debug display settings");
        Assert(field.Progression.UniversalPoints
            == ArenaProgression.StartingUniversalPoints
            && field.Progression.GetSkillRank(PlayerAbilityCatalog.PowerBlast) == 0,
            "a new run resets run-local skills and point balances");
    }

    private static void TestEquipmentMenuSelection()
    {
        ArenaField field = new ArenaField();
        PainterG9 painter = new PainterG9();
        Canvas canvas = new Canvas { Width = 1024, Height = 768 };
        canvas.Measure(new Size(1024, 768));
        canvas.Arrange(new Rect(0, 0, 1024, 768));

        painter.PaintGameField(canvas, field);
        Assert(FindMenuButtonIn(canvas, "Waffen testen") == null,
            "the weapon test tab has been removed");
        FindMenuButton(canvas, "Ausrüstung").RaiseEvent(
            new RoutedEventArgs(Button.ClickEvent));
        painter.PaintGameField(canvas, field);
        Assert(!FindMenuButton(canvas, "Magischer Stab — Waffenbaum").IsEnabled,
            "weapon upgrades are unavailable before a run starts");
        FindMenuButton(canvas, "Magischer Stab — Projectile (10 damage)").RaiseEvent(
            new RoutedEventArgs(Button.ClickEvent));
        Assert(field.SelectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id,
            "equipment menu selects a real weapon instead of test weapons");
    }

    private static void TestProgressionAndKeyBindingMenus()
    {
        string previousDirectory = Environment.CurrentDirectory;
        Environment.CurrentDirectory = FindGameProjectDirectory();
        try
        {
            ArenaField field = new ArenaField();
            field.StartNewGame();
            field.ReturnToStartMenu();
            PainterG9 painter = new PainterG9();
            Canvas canvas = new Canvas { Width = 1024, Height = 768 };
            canvas.Measure(new Size(1024, 768));
            canvas.Arrange(new Rect(0, 0, 1024, 768));

            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Skilltree").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Freischalten (3 universelle Punkte)").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            Assert(field.Progression.GetSkillRank(PlayerAbilityCatalog.PowerBlast) == 1,
                "skilltree menu unlocks Power Blast");

            field.HandleKeyDown(Key.Escape);
            painter.PaintGameField(canvas, field);
            field.HandleKeyDown(Key.Escape);
            painter.PaintGameField(canvas, field);
            Assert(FindMenuButton(canvas, "Spiel fortsetzen") != null,
                "pausing again after leaving a submenu returns to the pause menu");
            FindMenuButton(canvas, "Waffen verbessern").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            field.Progression.AwardKill(WeaponCatalog.Sword);
            canvas.Tag = null;
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Verbessern (1 Waffenpunkte)").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            Assert(field.Progression.GetWeaponUpgradeRank(
                SwordUpgradeCatalog.StrikeDamage) == 1,
                "weapon-upgrade menu purchases the selected sword upgrade");

            FindMenuButton(canvas, "Back").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Ausrüstung").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            Button wandUpgradeButton = FindMenuButton(
                canvas, "Magischer Stab — Waffenbaum");
            Assert(wandUpgradeButton.IsEnabled,
                "weapon tree is available for the selected weapon during a run");
            wandUpgradeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            Assert(field.SelectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id,
                "direct weapon-tree access selects the matching weapon");
            FindMenuButton(canvas, "Back").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            Assert(FindMenuButton(canvas, "Magischer Stab — Waffenbaum") != null,
                "weapon-tree Back returns to equipment selection");
            FindMenuButton(canvas, "Back").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Ausrüstung").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Bogen — Waffenbaum").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            Assert(field.SelectedPlayerWeapon.Id == WeaponCatalog.Bow.Id,
                "bow can be selected directly from its upgrade-tree entry");
            field.Progression.AwardKill(WeaponCatalog.Bow);
            canvas.Tag = null;
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Verbessern (1 Waffenpunkte)").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            Assert(field.Progression.GetWeaponUpgradeRank(
                BowUpgradeCatalog.AutoAim) == 1,
                "equipment menu opens and upgrades the bow tree");
            FindMenuButton(canvas, "Back").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Back").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Tastenbelegung").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            Assert(FindMenuButton(canvas, "Nach oben bewegen: "
                + field.KeyBindings.GetDisplayName(ArenaAction.MoveUp)) != null,
                "keybinding menu displays the current control mapping");
            FindMenuButton(canvas, "Back").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Godmode: AUS — aktivieren").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            Assert(field.IsGodMode,
                "main menu can enable god mode");
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Godmode: AN — deaktivieren").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            Assert(!field.IsGodMode,
                "main menu can disable god mode");
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    private static void TestRangeKnockbackStatsAndGodMode()
    {
        ArenaProgression progression = new ArenaProgression();
        progression.AwardKill(WeaponCatalog.Bow);
        Assert(progression.TryUpgradeBow(
            BowUpgradeCatalog.Range, UpgradeCurrency.Weapon, true),
            "weapon range can be upgraded with weapon points");
        progression.AwardKill(WeaponCatalog.Bow);
        Assert(progression.CanUpgradeBow(
            BowUpgradeCatalog.Range, UpgradeCurrency.Weapon, out int nextCost, true),
            "missing weapon points can be covered by skill points");
        progression.GetWeaponUpgradePayment(
            WeaponCatalog.Bow.Id, nextCost,
            out int weaponPointsSpent, out int skillPointsSpent);
        Assert(weaponPointsSpent == 1 && skillPointsSpent == 1
            && progression.TryUpgradeBow(
                BowUpgradeCatalog.Range, UpgradeCurrency.Weapon, true),
            "weapon upgrades spend weapon points first and skills only for the difference");
        Assert(Math.Abs(((ProjectileAttackPattern)BowUpgradeCatalog.CreateBow(progression)
            .AttackPattern).Range - 780) < 0.001,
            "bow range ranks increase projectile range");

        ArenaProgression freeProgression = new ArenaProgression();
        freeProgression.SetGodMode(true);
        freeProgression.AwardKill(WeaponCatalog.Bow);
        freeProgression.SetGodMode(false);
        Assert(freeProgression.GetWeaponPoints(WeaponCatalog.Bow.Id) == 1,
            "weapon kills still accrue normally while god mode is active");
        freeProgression.SetGodMode(true);
        Assert(freeProgression.TryUpgradeSword(
                SwordUpgradeCatalog.Range, UpgradeCurrency.Weapon, true)
            && freeProgression.TryUpgradeSword(
                SwordUpgradeCatalog.Knockback, UpgradeCurrency.Weapon, true)
            && freeProgression.TryUpgradeMagicWand(
                MagicWandUpgradeCatalog.Range, UpgradeCurrency.Weapon, true)
            && freeProgression.TryUpgradeMagicWand(
                MagicWandUpgradeCatalog.Knockback, UpgradeCurrency.Weapon, true)
            && freeProgression.TryUpgradeBow(
                BowUpgradeCatalog.Range, UpgradeCurrency.Weapon, true)
            && freeProgression.TryUpgradeBow(
                BowUpgradeCatalog.Knockback, UpgradeCurrency.Weapon, true),
            "all weapon families support range and knockback upgrades");
        Weapon upgradedSword = SwordUpgradeCatalog.CreateSword(freeProgression);
        Weapon upgradedWand = MagicWandUpgradeCatalog.CreateMagicWand(freeProgression);
        Weapon upgradedBow = BowUpgradeCatalog.CreateBow(freeProgression);
        Assert(Math.Abs(((SweepingArcAttackPattern)upgradedSword.AttackPattern)
                .Range - 110) < 0.001
            && Math.Abs(((ProjectileAttackPattern)upgradedWand.AttackPattern)
                .Range - 572) < 0.001
            && Math.Abs(((ProjectileAttackPattern)upgradedBow.AttackPattern)
                .Range - 715) < 0.001
            && upgradedSword.KnockbackDistance == 13
            && upgradedWand.KnockbackDistance == 13
            && upgradedBow.KnockbackDistance == 13,
            "each weapon upgrade adjusts its range and hit knockback");

        ArenaField field = new ArenaField();
        field.StartNewGame();
        Assert(field.TryUpgradePlayerStat(ArenaProgression.MaximumHealth)
            && field.Player.MaxHitPoints == PlayerCharacter.BaseMaxHitPoints
                + ArenaProgression.HealthIncreasePerRank,
            "skill tree can increase maximum player health");
        Assert(field.TryUpgradePlayerStat(ArenaProgression.MaximumSpeed)
            && field.Player.MaxSpeed == PlayerCharacter.BaseMaxSpeed
                + ArenaProgression.SpeedIncreasePerRank,
            "skill tree can increase maximum player speed");

        field.ToggleGodMode();
        int healthBeforeDamage = field.Player.CurrentHitPoints;
        field.Player.TakeDamage(20);
        Assert(field.IsGodMode
            && field.Progression.UniversalPoints == int.MaxValue
            && field.Progression.GetWeaponPoints(WeaponCatalog.Sword.Id) == int.MaxValue
            && field.Player.CurrentHitPoints == healthBeforeDamage,
            "god mode grants unlimited points and prevents player damage");
        field.ToggleGodMode();
        field.Player.TakeDamage(1);
        Assert(!field.IsGodMode && field.Player.DamageFlashIntensity > 0,
            "leaving god mode restores damage and displays a white damage flash");
        field.Player.UpdateStatusEffects(0.18);
        Assert(field.Player.DamageFlashIntensity == 0,
            "the damage flash ends after a short interval");
    }

    private static Button FindMenuButton(Canvas canvas, string content)
    {
        Button found = FindMenuButtonIn(canvas, content);
        if (found != null)
        {
            return found;
        }

        throw new InvalidOperationException("Menu button not found: " + content);
    }

    private static string FindGameProjectDirectory()
    {
        DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "OOPGames.csproj")))
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate OOPGames.csproj from the test output directory.");
    }

    private static Button FindMenuButtonIn(DependencyObject parent, string content)
    {
        Button button = parent as Button;
        if (button != null && Equals(button.Content, content))
        {
            return button;
        }

        foreach (object child in LogicalTreeHelper.GetChildren(parent))
        {
            DependencyObject dependency = child as DependencyObject;
            if (dependency == null)
            {
                continue;
            }

            Button found = FindMenuButtonIn(dependency, content);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    private static Weapon CreateTestWeapon(
        string id, IAttackPattern pattern, ICombatEffect effect)
    {
        return new Weapon(
            id, id, "Test-only combat fixture.", pattern, effect,
            0.3, WeaponTargetingMode.Opponents, id);
    }

    private static void Assert(bool condition, string description)
    {
        _assertions++;
        if (!condition)
        {
            throw new InvalidOperationException("Assertion failed: " + description);
        }
    }

    private sealed class TestEntity : Entity
    {
        public TestEntity(
            double x, double y, int hitPoints, double speed, CombatTeam team)
            : base(x, y, hitPoints, speed, 20, Brushes.Gray, team)
        {
        }
    }
}
