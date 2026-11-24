using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Polygon = System.Windows.Shapes.Polygon;
using Polyline = System.Windows.Shapes.Polyline;
using OOPGames;

internal static class Program
{
    private static int _assertions;

    [STAThread]
    private static int Main()
    {
        string selectionPath = Path.Combine(
            ArenaSkillTreeRepository.TreeDirectory, "SelectedSkillTree.txt");
        bool hadSelection = File.Exists(selectionPath);
        string previousSelection = hadSelection
            ? File.ReadAllText(selectionPath)
            : null;
        try
        {
            ArenaSkillTreeRepository.SelectTree(
                ArenaSkillTreeRepository.DefaultTreeName);
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
            TestBlackSteelWeaknessAndLeach();
            TestWeaponSpriteAppearances();
            TestPlayerKeepsStrikeFacing();
            TestMultiplePlayerAbilities();
            TestArenaSkillTreeDependencies();
            TestArenaSkillTreeStartNode();
            TestArenaSkillTreeForcedFork();
            TestArenaMapDimensionInput();
            TestArenaEditorAssets();
            TestArenaMapGridSnapping();
            TestArenaMapPlacementTransforms();
            TestArenaDevToolMenu();
            TestArenaMapRestart();
            TestArenaSkillTreeRestart();
            TestKeyBindingPersistenceAndConflicts();
            TestInputSeparatesAbilityTypes();
            TestWeaponWheelHoldAndReleaseSelection();
            TestKillsGrantRunProgression();
            TestWeaponSelectionSurvivesRestart();
            TestEquipmentMenuSelection();
            TestProgressionAndKeyBindingMenus();
            TestRangeKnockbackStatsAndGodMode();
            TestPushableEntityMovement();
            TestTimedEnemySpawningAndLimit();
            TestPlayerDamageEvents();
            TestSoundGroupSelection();
            TestDeathEnemyAndPurchaseSoundEvents();
            TestSwordSwingSoundEvents();
            TestSoundFilesMatchGroups();
            TestSprintAndLowHealthSoundEvents();
            TestSlowMotionScalesGameTime();
            TestAbilityCooldownAndDurationUpgrades();
            TestWeaponAttackCooldownUpgrades();
            TestBulletTimeKeepsPlayerAtNormalSpeed();
            TestGodModeBuyAllAndGameSpeed();

            Console.WriteLine("OK: " + _assertions + " arena assertions passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
        finally
        {
            if (hadSelection)
            {
                File.WriteAllText(selectionPath, previousSelection);
            }
            else if (File.Exists(selectionPath))
            {
                File.Delete(selectionPath);
            }
        }
    }

    private static void TestSlowMotionScalesGameTime()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        field.Player.IsInvulnerable = true;
        Assert(field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.SlowMotion)
            && field.Progression.EntityAbilities[0] == PlayerAbilityCatalog.SlowMotion,
            "slow motion can be unlocked as a player ability");
        Assert(field.AttackWithEntityAbility(0) && field.IsSlowMotionActive,
            "using the ability starts slow motion");
        double scale = PlayerAbilityCatalog.GetSlowMotionTimeScale(1);
        Assert(Math.Abs(field.GameTimeScale - scale) < 1e-9,
            "slow motion lowers the game time scale");

        Weapon slowMotion = field.Progression.GetEntityAbilityWeapon(0);
        double cooldownBefore = field.GetAttackCooldownRemaining(field.Player, slowMotion);
        field.Tick();
        double cooldownAfter = field.GetAttackCooldownRemaining(field.Player, slowMotion);
        Assert(Math.Abs(cooldownBefore - cooldownAfter
                - ArenaField.GameTickDurationSeconds * scale) < 1e-9,
            "cooldowns advance with the slowed tick speed");
        Assert(!field.AttackWithEntityAbility(0),
            "slow motion respects its own cooldown");

        field.Player.StartAttack();
        int attackTicks = 0;
        while (field.Player.IsAttacking && attackTicks < 100)
        {
            field.Tick();
            attackTicks++;
        }
        Assert(attackTicks == (int)Math.Ceiling(
                0.2 / (ArenaField.GameTickDurationSeconds * scale) - 1e-9),
            "the attack pose lasts longer in slow motion because it uses game time");

        int ticks = 1 + attackTicks;
        while (field.IsSlowMotionActive && ticks < 1000)
        {
            field.Tick();
            ticks++;
        }
        Assert(ticks == (int)Math.Ceiling(
                PlayerAbilityCatalog.SlowMotionBaseDurationSeconds
                    / ArenaField.GameTickDurationSeconds - 1e-9)
            && Math.Abs(field.GameTimeScale - 1) < 1e-9,
            "slow motion ends after its duration in unscaled ticks");

        field.ActivateSlowMotion(0.4, 1);
        field.HandleKeyDown(field.KeyBindings.GetKey(ArenaAction.WeaponWheel));
        Assert(Math.Abs(field.GameTimeScale - ArenaField.WeaponWheelSlowMotionScale) < 1e-9,
            "weapon wheel and slow motion use the stronger slowdown");
        field.CancelWeaponWheel();
        field.RestartGame();
        Assert(!field.IsSlowMotionActive, "a new run ends slow motion");

        // Projectiles travel by game time: half the time scale, half the distance per tick.
        ArenaField normal = new ArenaField();
        ArenaField slowed = new ArenaField();
        foreach (ArenaField arena in new[] { normal, slowed })
        {
            arena.StartNewGame();
            arena.SelectPlayerWeapon(WeaponCatalog.MagicWand);
            arena.UpdateCombat(0);
            arena.AttackWithPlayer(1, 0);
        }
        double startX = normal.ActiveAttackVisuals[0].ProjectileX;
        slowed.ActivateSlowMotion(0.5, 5);
        normal.UpdateCombat(ArenaField.GameTickDurationSeconds * normal.GameTimeScale);
        slowed.UpdateCombat(ArenaField.GameTickDurationSeconds * slowed.GameTimeScale);
        double normalTravel = normal.ActiveAttackVisuals[0].ProjectileX - startX;
        double slowedTravel = slowed.ActiveAttackVisuals[0].ProjectileX - startX;
        Assert(normalTravel > 0 && Math.Abs(slowedTravel * 2 - normalTravel) < 1e-6,
            "projectiles move proportionally slower in slow motion");
    }

    private static void TestAbilityCooldownAndDurationUpgrades()
    {
        ArenaProgression progression = new ArenaProgression();
        PlayerAbilityDefinition blast =
            PlayerAbilityCatalog.GetDefinition(PlayerAbilityCatalog.PowerBlast);
        Assert(!blast.HasDuration
            && PlayerAbilityCatalog.GetDefinition(PlayerAbilityCatalog.SlowMotion).HasDuration,
            "only abilities with a lasting effect offer a duration upgrade");
        Assert(!progression.TryUpgradeAbilityModifier(blast.CooldownUpgradeId),
            "cooldown upgrades need the ability to be unlocked");
        Assert(progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast)
            && progression.TryUpgradeAbilityModifier(blast.CooldownUpgradeId)
            && progression.TryUpgradeAbilityModifier(blast.CooldownUpgradeId),
            "cooldown upgrades can be bought after unlocking");
        Weapon upgradedBlast = progression.GetEntityAbilityWeapon(0);
        Assert(Math.Abs(upgradedBlast.CooldownSeconds
                - 3.5 * (1 - 2 * PlayerAbilityCatalog.CooldownReductionPerRank)) < 1e-9,
            "each cooldown rank lowers the ability cooldown");

        PlayerAbilityDefinition slow =
            PlayerAbilityCatalog.GetDefinition(PlayerAbilityCatalog.SlowMotion);
        Assert(progression.TryUpgradeSkill(PlayerAbilityCatalog.SlowMotion)
            && progression.TryUpgradeAbilityModifier(slow.DurationUpgradeId),
            "slow-motion duration can be upgraded");
        TimeSlowEffect slowEffect =
            (TimeSlowEffect)progression.GetEntityAbilityWeapon(1).Effect;
        Assert(Math.Abs(slowEffect.DurationSeconds
                - (PlayerAbilityCatalog.SlowMotionBaseDurationSeconds
                    + PlayerAbilityCatalog.SlowMotionDurationPerRank)) < 1e-9,
            "duration ranks extend the slow motion");
        Assert(Math.Abs(PlayerAbilityCatalog.GetSlowMotionTimeScale(1)
                - PlayerAbilityCatalog.GetSlowMotionTimeScale(5)
                - 4 * PlayerAbilityCatalog.SlowMotionTimeScalePerRank) < 1e-9,
            "higher slow-motion ranks slow down more");

        progression.SetGodMode(true);
        for (int i = 0; i < 10; i++)
        {
            progression.TryUpgradeAbilityModifier(blast.CooldownUpgradeId);
        }
        Assert(progression.GetSkillRank(blast.CooldownUpgradeId)
                == PlayerAbilityCatalog.ModifierMaximumRank,
            "modifier ranks stop at their maximum");
        progression.SetGodMode(false);

        ArenaField field = new ArenaField();
        field.StartNewGame();
        Assert(field.Progression.TryUpgradeSword(
                SwordUpgradeCatalog.StrikeDamage, UpgradeCurrency.Universal)
            && field.Progression.TryUpgradeSword(
                SwordUpgradeCatalog.Spin, UpgradeCurrency.Universal)
            && field.Progression.TryUpgradeSword(
                SwordUpgradeCatalog.SpinCooldown, UpgradeCurrency.Universal)
            && field.Progression.TryUpgradeSword(
                SwordUpgradeCatalog.SpinDuration, UpgradeCurrency.Universal),
            "sword spin cooldown and duration can be upgraded");
        Weapon spin = SwordUpgradeCatalog.CreateSpinWeapon(field.Progression);
        Assert(Math.Abs(spin.CooldownSeconds - SwordUpgradeCatalog.SpinBaseCooldownSeconds
                * (1 - PlayerAbilityCatalog.CooldownReductionPerRank)) < 1e-9
            && SwordUpgradeCatalog.GetSpinRotations(field.Progression)
                == SwordUpgradeCatalog.SpinRotations + 1,
            "spin upgrades shorten the cooldown and add a revolution");
        int swings = 0;
        field.PlayerSwordSwung += () => swings++;
        field.AttackWithWeaponAbility(0);
        for (int index = 0; index < 60; index++)
        {
            field.UpdateCombat(0.04);
        }
        Assert(swings == SwordUpgradeCatalog.SpinRotations + 1,
            "a longer spin swings once per added revolution");

        field.Progression.SetGodMode(true);
        Assert(field.Progression.TryUpgradeMagicWand(
                MagicWandUpgradeCatalog.Strike, UpgradeCurrency.Universal)
            && field.Progression.TryUpgradeMagicWand(
                MagicWandUpgradeCatalog.StrikeCooldown, UpgradeCurrency.Universal)
            && Math.Abs(MagicWandUpgradeCatalog.CreateStrikeWeapon(field.Progression)
                .CooldownSeconds - MagicWandUpgradeCatalog.StrikeBaseCooldownSeconds
                    * (1 - PlayerAbilityCatalog.CooldownReductionPerRank)) < 1e-9,
            "the wand strike cooldown can be upgraded");
    }

    private static void TestWeaponAttackCooldownUpgrades()
    {
        ArenaProgression progression = new ArenaProgression();
        double factor = 1 - PlayerAbilityCatalog.CooldownReductionPerRank;
        Assert(progression.TryUpgradeSword(
                SwordUpgradeCatalog.AttackCooldown, UpgradeCurrency.Universal)
            && progression.TryUpgradeMagicWand(
                MagicWandUpgradeCatalog.AttackCooldown, UpgradeCurrency.Universal)
            && progression.TryUpgradeBow(
                BowUpgradeCatalog.AttackCooldown, UpgradeCurrency.Universal),
            "every weapon offers a primary attack cooldown upgrade without prerequisite");

        Weapon sword = SwordUpgradeCatalog.CreateSword(progression);
        Assert(Math.Abs(sword.CooldownSeconds
                - SwordUpgradeCatalog.BaseCooldownSeconds * factor) < 1e-9
            && Math.Abs(((SweepingArcAttackPattern)sword.AttackPattern).DurationSeconds
                - SwordUpgradeCatalog.BaseSwingSeconds * factor) < 1e-9,
            "sword cooldown and swing duration shrink together");
        foreach (Weapon slashingSword in new[] { WeaponCatalog.Sword, sword })
        {
            TestEntity swordsman = new TestEntity(100, 100, 100, 10, CombatTeam.Player);
            ArenaCombatSystem slashCombat = new ArenaCombatSystem();
            Assert(slashCombat.TryStartAttack(swordsman, slashingSword, 1, 0),
                "a sword slash starts");
            int ticks = 0;
            while (slashCombat.ActiveAttackCount > 0 && ticks < 100)
            {
                slashCombat.Update(ArenaField.GameTickDurationSeconds,
                    new List<Entity> { swordsman });
                ticks++;
                if (slashCombat.ActiveAttackCount > 0)
                {
                    Assert(!slashCombat.TryStartAttack(swordsman, slashingSword, 1, 0),
                        "no new slash starts while one is still running");
                }
            }
            Assert(slashCombat.TryStartAttack(swordsman, slashingSword, 1, 0),
                "the next slash can start in the tick the previous one is finished");
        }
        Assert(Math.Abs(MagicWandUpgradeCatalog.CreateMagicWand(progression).CooldownSeconds
                - MagicWandUpgradeCatalog.BaseCooldownSeconds * factor) < 1e-9,
            "magic wand cooldown can be upgraded");
        Assert(Math.Abs(BowUpgradeCatalog.CreateBow(progression).CooldownSeconds
                - BowUpgradeCatalog.BaseCooldownSeconds * factor) < 1e-9,
            "bow cooldown can be upgraded");

        progression.SetGodMode(true);
        for (int i = 0; i < 10; i++)
        {
            progression.TryUpgradeSword(
                SwordUpgradeCatalog.AttackCooldown, UpgradeCurrency.Universal);
        }
        Weapon fastestSword = SwordUpgradeCatalog.CreateSword(progression);
        Assert(((SweepingArcAttackPattern)fastestSword.AttackPattern).DurationSeconds
                <= fastestSword.CooldownSeconds,
            "fully upgraded sword slashes still never overlap");
    }

    private static void TestBulletTimeKeepsPlayerAtNormalSpeed()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        field.Player.IsInvulnerable = true;
        ArenaProgression progression = field.Progression;
        Assert(progression.TryUpgradeSkill(PlayerAbilityCatalog.SlowMotion)
            && !progression.CanUnlockBulletTime(),
            "bullet time needs a higher slow-motion rank");
        progression.SetGodMode(true);
        progression.TryUpgradeSkill(PlayerAbilityCatalog.SlowMotion);
        progression.TryUpgradeSkill(PlayerAbilityCatalog.SlowMotion);
        progression.SetGodMode(false);
        for (int i = 0; i < ArenaProgression.KillsPerUniversalPoint; i++)
        {
            progression.AwardKill(WeaponCatalog.Sword);
        }
        int pointsBefore = progression.UniversalPoints;
        Assert(progression.TryUnlockBulletTime() && progression.HasBulletTime
            && progression.UniversalPoints
                == pointsBefore - PlayerAbilityCatalog.BulletTimeCost
            && !progression.TryUnlockBulletTime(),
            "bullet time is a one-off purchase for its skill-point price");

        Assert(field.AttackWithEntityAbility(0) && field.IsBulletTimeActive,
            "the slow-motion ability starts bullet time once it is unlocked");
        double worldScale = PlayerAbilityCatalog.GetSlowMotionTimeScale(3);
        Assert(Math.Abs(field.GameTimeScale - worldScale) < 1e-9
            && Math.Abs(field.PlayerTimeScale - 1) < 1e-9,
            "bullet time slows the arena but not the player");

        field.SelectPlayerWeapon(WeaponCatalog.MagicWand);
        Weapon wand = field.Player.EquippedWeapon;
        field.AttackWithPlayer(1, 0);
        double playerCooldown = field.GetAttackCooldownRemaining(field.Player, wand);
        field.Tick();
        Assert(Math.Abs(playerCooldown
                - field.GetAttackCooldownRemaining(field.Player, wand)
                - ArenaField.GameTickDurationSeconds) < 1e-9,
            "player cooldowns run at normal speed during bullet time");

        // A player-team entity follows the player time, other entities the arena time.
        ArenaCombatSystem combat = new ArenaCombatSystem();
        TestEntity fast = new TestEntity(0, 0, 30, 10, CombatTeam.Player)
        {
            EquippedWeapon = WeaponCatalog.Sword
        };
        TestEntity slow = new TestEntity(500, 0, 30, 10, CombatTeam.Enemies)
        {
            EquippedWeapon = WeaponCatalog.EnemyClaws
        };
        combat.TryStartAttack(fast, 1, 0);
        combat.TryStartAttack(slow, -1, 0);
        combat.Update(new Entity[] { fast, slow }, null,
            entity => ReferenceEquals(entity, fast) ? 0.1 : 0.02);
        Assert(Math.Abs(combat.GetCooldownRemaining(fast)
                - (WeaponCatalog.Sword.CooldownSeconds - 0.1)) < 1e-9
            && Math.Abs(combat.GetCooldownRemaining(slow)
                - (WeaponCatalog.EnemyClaws.CooldownSeconds - 0.02)) < 1e-9,
            "the combat system advances each entity by its own time");

        while (field.IsSlowMotionActive)
        {
            field.Tick();
        }
        Assert(!field.IsBulletTimeActive && Math.Abs(field.PlayerTimeScale - 1) < 1e-9,
            "bullet time ends together with the slow motion");
    }

    private static void TestGodModeBuyAllAndGameSpeed()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        Assert(!field.PurchaseAllUpgrades() && !field.ChangeGameSpeed(1)
                && field.Progression.GetSkillRank(PlayerAbilityCatalog.PowerBlast) == 0,
            "buy-all and game speed are godmode-only tools");

        int purchaseEvents = 0;
        field.Progression.UpgradePurchased += () => purchaseEvents++;
        field.ToggleGodMode();
        Assert(field.PurchaseAllUpgrades() && purchaseEvents == 1,
            "buying everything reports one purchase instead of one per rank");

        ArenaProgression progression = field.Progression;
        bool allSkillsMaxed = true;
        foreach (PlayerAbilityDefinition definition in PlayerAbilityCatalog.Definitions)
        {
            allSkillsMaxed &= progression.GetSkillRank(definition.Id) == definition.MaximumRank
                && progression.GetSkillRank(definition.CooldownUpgradeId)
                    == PlayerAbilityCatalog.ModifierMaximumRank
                && (!definition.HasDuration
                    || progression.GetSkillRank(definition.DurationUpgradeId)
                        == PlayerAbilityCatalog.ModifierMaximumRank);
        }
        Assert(allSkillsMaxed && progression.HasBulletTime,
            "all player abilities, their modifiers and bullet time are maxed");
        Assert(field.Player.MaxHitPoints == PlayerCharacter.BaseMaxHitPoints
                + ArenaProgression.PlayerStatMaximumRank * ArenaProgression.HealthIncreasePerRank
            && Math.Abs(field.Player.MaxSpeed - (PlayerCharacter.BaseMaxSpeed
                + ArenaProgression.PlayerStatMaximumRank
                    * ArenaProgression.SpeedIncreasePerRank)) < 1e-9,
            "player stat upgrades are bought and applied to the player");

        bool weaponsMaxed = true;
        foreach (WeaponUpgradeDefinition upgrade in SwordUpgradeCatalog.Upgrades)
        {
            // Leach builds on the Blacksteel element, so it stays open like ElementPower.
            if (!SwordUpgradeCatalog.IsElement(upgrade.Id)
                && upgrade.Id != SwordUpgradeCatalog.ElementPower
                && upgrade.Id != SwordUpgradeCatalog.Leach)
            {
                weaponsMaxed &= progression.GetWeaponUpgradeRank(upgrade.Id)
                    == upgrade.MaximumRank;
            }
        }
        foreach (WeaponUpgradeDefinition upgrade in BowUpgradeCatalog.Upgrades)
        {
            if (!BowUpgradeCatalog.IsElement(upgrade.Id)
                && upgrade.Id != BowUpgradeCatalog.ElementPower)
            {
                weaponsMaxed &= progression.GetWeaponUpgradeRank(upgrade.Id)
                    == upgrade.MaximumRank;
            }
        }
        foreach (WeaponUpgradeDefinition upgrade in MagicWandUpgradeCatalog.Upgrades)
        {
            if (!MagicWandUpgradeCatalog.IsElement(upgrade.Id)
                && upgrade.Id != MagicWandUpgradeCatalog.ElementPower)
            {
                weaponsMaxed &= progression.GetWeaponUpgradeRank(upgrade.Id)
                    == upgrade.MaximumRank;
            }
        }
        Assert(weaponsMaxed
                && progression.SelectedSwordElement == WeaponElement.None
                && progression.SelectedBowElement == WeaponElement.None
                && progression.SelectedMagicWandElement == WeaponElement.None,
            "all weapon upgrades are maxed while element choices stay open");
        Assert(Math.Abs(field.Player.EquippedWeapon.CooldownSeconds
                - SwordUpgradeCatalog.BaseCooldownSeconds
                    * PlayerAbilityCatalog.GetCooldownMultiplier(
                        PlayerAbilityCatalog.ModifierMaximumRank)) < 1e-9,
            "the equipped weapon is rebuilt with the bought upgrades");

        ArenaField elementField = new ArenaField();
        elementField.StartNewGame();
        elementField.ToggleGodMode();
        elementField.Progression.TryUpgradeSword(
            SwordUpgradeCatalog.StrikeDamage, UpgradeCurrency.Universal);
        elementField.Progression.TryUpgradeSword(
            SwordUpgradeCatalog.Fire, UpgradeCurrency.Universal);
        elementField.PurchaseAllUpgrades();
        Assert(elementField.Progression.GetWeaponUpgradeRank(SwordUpgradeCatalog.ElementPower)
                == SwordUpgradeCatalog.GetDefinition(SwordUpgradeCatalog.ElementPower).MaximumRank
            && elementField.Progression.GetWeaponUpgradeRank(SwordUpgradeCatalog.Poison) == 0,
            "an already chosen element is maxed, other elements stay locked");

        ArenaField blackSteelField = new ArenaField();
        blackSteelField.StartNewGame();
        blackSteelField.ToggleGodMode();
        blackSteelField.Progression.TryUpgradeSword(
            SwordUpgradeCatalog.StrikeDamage, UpgradeCurrency.Universal);
        blackSteelField.Progression.TryUpgradeSword(
            SwordUpgradeCatalog.BlackSteel, UpgradeCurrency.Universal);
        blackSteelField.PurchaseAllUpgrades();
        Assert(blackSteelField.Progression.GetWeaponUpgradeRank(SwordUpgradeCatalog.Leach) == 1,
            "buying everything with Blacksteel chosen also buys leach");

        Assert(field.ChangeGameSpeed(5) && Math.Abs(field.GameSpeed - 1.5) < 1e-9
                && Math.Abs(field.GameTimeScale - 1.5) < 1e-9
                && Math.Abs(field.PlayerTimeScale - 1.5) < 1e-9,
            "godmode can speed the game up");
        field.ActivateSlowMotion(0.5, 1);
        Assert(Math.Abs(field.GameTimeScale - 0.75) < 1e-9,
            "game speed combines with slow motion");
        field.Tick();
        Assert(Math.Abs(field.SlowMotionRemainingSeconds
                - (1 - ArenaField.GameTickDurationSeconds * 1.5)) < 1e-9,
            "the slow-motion countdown follows the overall game speed");
        field.ChangeGameSpeed(100);
        Assert(Math.Abs(field.GameSpeed - ArenaField.MaximumGameSpeed) < 1e-9,
            "game speed stops at its maximum");
        field.ChangeGameSpeed(-100);
        Assert(Math.Abs(field.GameSpeed - ArenaField.MinimumGameSpeed) < 1e-9,
            "game speed stops at its minimum");
        field.ToggleGodMode();
        Assert(Math.Abs(field.GameSpeed - 1) < 1e-9,
            "leaving godmode restores normal game speed");

        string path = Path.Combine(
            Path.GetTempPath(), "arena-keybindings-" + Guid.NewGuid() + ".json");
        try
        {
            Assert(ArenaKeyBindings.Load(path).GetKey(ArenaAction.GameSpeed) == Key.T,
                "the game-speed modifier defaults to T");
            File.WriteAllText(path, "{\"MoveUp\":\"T\"}");
            ArenaKeyBindings migrated = ArenaKeyBindings.Load(path);
            Assert(migrated.GetKey(ArenaAction.MoveUp) == Key.T
                    && migrated.GetKey(ArenaAction.GameSpeed) == Key.G,
                "older settings that already use T move the new action to a free key");
        }
        finally
        {
            File.Delete(path);
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
        TestEntity enemy = new TestEntity(150, 100, 30, 10, CombatTeam.Enemies)
        {
            CanBePushed = true
        };
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

    private static void TestBlackSteelWeaknessAndLeach()
    {
        TestEntity weakenedEnemy = new TestEntity(100, 100, 100, 10, CombatTeam.Enemies);
        TestEntity target = new TestEntity(150, 100, 100, 10, CombatTeam.Player);
        new WeaknessEffect(0.5, 2).Apply(null, weakenedEnemy);
        new DamageEffect(10).Apply(weakenedEnemy, target);
        Assert(weakenedEnemy.IsWeakened && target.CurrentHitPoints == 95,
            "a weakened attacker deals reduced damage");
        weakenedEnemy.UpdateStatusEffects(2);
        new DamageEffect(10).Apply(weakenedEnemy, target);
        Assert(!weakenedEnemy.IsWeakened && target.CurrentHitPoints == 85,
            "weakness expires and full damage returns");

        TestEntity leacher = new TestEntity(100, 100, 100, 10, CombatTeam.Player);
        TestEntity victim = new TestEntity(150, 100, 100, 10, CombatTeam.Enemies);
        leacher.TakeDamage(50);
        new LifeStealEffect(new DamageEffect(20), 0.5).Apply(leacher, victim);
        Assert(victim.CurrentHitPoints == 80 && leacher.CurrentHitPoints == 60,
            "life steal heals the attacker by a share of the dealt damage");

        ArenaProgression progression = new ArenaProgression();
        Assert(progression.TryUpgradeSword(
            SwordUpgradeCatalog.StrikeDamage, UpgradeCurrency.Universal)
            && !progression.TryUpgradeSword(
                SwordUpgradeCatalog.Leach, UpgradeCurrency.Universal),
            "leach requires the Blacksteel blade");
        Assert(progression.TryUpgradeSword(
            SwordUpgradeCatalog.BlackSteel, UpgradeCurrency.Universal)
            && progression.SelectedSwordElement == WeaponElement.BlackSteel,
            "Blacksteel is a selectable sword element");
        Assert(!progression.TryUpgradeSword(
            SwordUpgradeCatalog.Poison, UpgradeCurrency.Universal),
            "Blacksteel excludes the other blade elements");
        Assert(SwordUpgradeCatalog.CreateSword(progression).Appearance.HeldAnimation
            == "SwordBlackSteel",
            "the Blacksteel sword uses its own sprite");
        Assert(progression.TryUpgradeSword(
            SwordUpgradeCatalog.Leach, UpgradeCurrency.Universal),
            "leach can be bought on top of Blacksteel");
        Weapon leachSword = SwordUpgradeCatalog.CreateSword(progression);
        Assert(leachSword.Effect is LifeStealEffect
            && leachSword.Appearance.HeldAnimation == "SwordBlackSteelLeach",
            "the leach sword steals life and shows the leach sprite");

        TestEntity swordsman = new TestEntity(100, 100, 100, 10, CombatTeam.Player);
        TestEntity enemy = new TestEntity(100, 100, 100, 10, CombatTeam.Enemies);
        swordsman.TakeDamage(50);
        leachSword.Effect.Apply(swordsman, enemy);
        Assert(enemy.IsWeakened && swordsman.CurrentHitPoints > 50,
            "a leach sword hit weakens the enemy and heals the player");
    }

    private static void TestWeaponSpriteAppearances()
    {
        ArenaProgression progression = new ArenaProgression();
        Assert(SwordUpgradeCatalog.CreateSword(progression).Appearance.HeldAnimation == "Sword"
            && MagicWandUpgradeCatalog.CreateMagicWand(progression).Appearance.HeldAnimation
                == "WizardStaff",
            "base sword and staff use their plain sprites");
        WeaponAppearance bow = BowUpgradeCatalog.CreateBow(progression).Appearance;
        Assert(bow.HeldAnimation == "BowShooting"
            && bow.ProjectileAnimations.SequenceEqual(new[] { "Arow" }),
            "the bow starts without scope and shoots plain arrows");

        Assert(progression.TryUpgradeMagicWand(
            MagicWandUpgradeCatalog.ProjectileDamage, UpgradeCurrency.Universal)
            && progression.TryUpgradeMagicWand(
                MagicWandUpgradeCatalog.Poison, UpgradeCurrency.Universal),
            "the staff can take the poison element");
        WeaponAppearance frogStaff =
            MagicWandUpgradeCatalog.CreateMagicWand(progression).Appearance;
        Assert(frogStaff.HeldAnimation == "WizardStaffFrog"
            && frogStaff.AttackAnimation == "WizardStaffFrogAttac",
            "the poison staff shows the frog and its attack frame");

        Assert(progression.TryUpgradeBow(
            BowUpgradeCatalog.AutoAim, UpgradeCurrency.Universal)
            && BowUpgradeCatalog.CreateBow(progression).Appearance.HeldAnimation
                == "BowShootingScope",
            "the first autoaim rank mounts a scope on the bow");
        Assert(progression.TryUpgradeBow(
            BowUpgradeCatalog.ArrowDamage, UpgradeCurrency.Universal)
            && progression.TryUpgradeBow(
                BowUpgradeCatalog.Fire, UpgradeCurrency.Universal)
            && BowUpgradeCatalog.CreateBow(progression).Appearance.ProjectileAnimations
                .SequenceEqual(new[] { "ArowFire", "ArowFireFlying" }),
            "fire arrows flicker between both fire arrow frames");
        progression.SetGodMode(true);
        while (progression.GetWeaponUpgradeRank(BowUpgradeCatalog.AutoAim)
            < BowUpgradeCatalog.RocketArrowAutoAimRank)
        {
            Assert(progression.TryUpgradeBow(
                BowUpgradeCatalog.AutoAim, UpgradeCurrency.Universal),
                "autoaim ranks can be bought up to the maximum");
        }
        WeaponAppearance rocketBow = BowUpgradeCatalog.CreateBow(progression).Appearance;
        Assert(rocketBow.HeldAnimation == "BowShootingScope"
            && rocketBow.ProjectileAnimations.SequenceEqual(new[] { "ArowAutoAim" }),
            "the highest autoaim rank fires rocket arrows");

        Assert(SwordUpgradeCatalog.CreateSword(progression).Appearance.Reach
                == SwordUpgradeCatalog.CreateSword(new ArenaProgression()).AttackPattern.Range
            && MagicWandUpgradeCatalog.CreateMagicWand(progression).Appearance.Reach
                == MagicWandUpgradeCatalog.GetStrikeRange(new ArenaProgression()),
            "blade and staff sprites are sized to their damage areas at the start of a run");
        Assert(progression.TryUpgradeSword(
                SwordUpgradeCatalog.Range, UpgradeCurrency.Universal)
            && SwordUpgradeCatalog.CreateSword(progression).AttackPattern.Range
                > SwordUpgradeCatalog.BaseSlashRange
            && SwordUpgradeCatalog.CreateSword(progression).Appearance.Reach
                == SwordUpgradeCatalog.CreateSword(progression).AttackPattern.Range
            && MagicWandUpgradeCatalog.CreateMagicWand(progression).Appearance.Reach
                == MagicWandUpgradeCatalog.BaseStrikeRange,
            "the sword is its own damage area, so range upgrades lengthen it; the staff keeps its size");

        // The damage area of the sword is the blade itself: from the right hand to the tip.
        ArenaField bladeField = new ArenaField();
        bladeField.StartNewGame();
        foreach (PursuerEnemy existing in bladeField.Enemies)
        {
            existing.TakeDamage(existing.MaxHitPoints);
        }
        PlayerCharacter swordsman = bladeField.Player;
        Weapon sword = SwordUpgradeCatalog.CreateSword(new ArenaProgression());
        TestEntity nearTip = new TestEntity(0, 0, 100, 0, CombatTeam.Enemies);
        TestEntity besideCone = new TestEntity(0, 0, 100, 0, CombatTeam.Enemies);
        ArenaCombatSystem bladeCombat = new ArenaCombatSystem();
        Assert(bladeCombat.TryStartAttack(swordsman, sword, 1, 0),
            "a sword slash starts");
        swordsman.StartAttack(PlayerFacing.Right, sword.CooldownSeconds);
        AttackVisualState slash = bladeCombat.GetActiveVisuals()[0];
        Assert(slash.HasBlade && slash.Appearance != null,
            "the sword slash describes its blade");
        Vector hand = swordsman.GetRightHandOffset(PlayerFacing.Right);
        Assert(Math.Abs(slash.BladeStartX - (swordsman.PositionX + hand.X)) < 1e-6
            && Math.Abs(slash.BladeStartY - (swordsman.PositionY + hand.Y)) < 1e-6,
            "the blade starts at the grip in the right hand");
        Vector bladeVector = new Vector(
            slash.BladeEndX - slash.BladeStartX, slash.BladeEndY - slash.BladeStartY);
        Assert(Math.Abs(bladeVector.Length
                - (sword.AttackPattern.Range - AttackGeometry.BladeGripGap)) < 1e-6,
            "the blade is as long as the drawn sword");

        // A target on the blade's path is hit; one just beyond the blade's tip (including
        // the blade width) is not.
        nearTip.PositionX = swordsman.PositionX + 70;
        nearTip.PositionY = swordsman.PositionY + hand.Y;
        besideCone.PositionX = swordsman.PositionX + hand.X + bladeVector.Length
            + slash.BladeHalfWidth + besideCone.Size / 2 + 1;
        besideCone.PositionY = swordsman.PositionY + hand.Y;
        List<Entity> bladeTargets = new List<Entity> { swordsman, nearTip, besideCone };
        for (int i = 0; i < 20; i++)
        {
            bladeCombat.Update(0.05, bladeTargets);
        }
        Assert(nearTip.CurrentHitPoints < 100,
            "an enemy on the blade's path is hit");
        Assert(besideCone.CurrentHitPoints == 100,
            "an enemy just beyond the blade tip is not hit");

        TestEntity drawingArcher = new TestEntity(100, 100, 100, 10, CombatTeam.Player);
        Weapon drawnBow = BowUpgradeCatalog.CreateBow(progression);
        drawingArcher.EquippedWeapon = drawnBow;
        ArenaCombatSystem bowCombat = new ArenaCombatSystem();
        Assert(drawnBow.EquipReadySeconds == drawnBow.CooldownSeconds
            && !bowCombat.TryStartAttack(drawingArcher, 1, 0),
            "a freshly equipped bow cannot shoot while it is being drawn");
        drawingArcher.UpdateStatusEffects(drawnBow.EquipReadySeconds);
        Assert(bowCombat.TryStartAttack(drawingArcher, 1, 0),
            "the bow shoots once it is fully drawn");

        TestEntity archer = new TestEntity(100, 100, 100, 10, CombatTeam.Player)
        {
            EquippedWeapon = WeaponCatalog.Sword
        };
        archer.UpdateStatusEffects(1);
        archer.EquippedWeapon = BowUpgradeCatalog.CreateBow(progression);
        Assert(archer.WeaponEquippedSeconds == 0,
            "equipping another weapon restarts its equip clock");
        archer.UpdateStatusEffects(0.4);
        archer.EquippedWeapon = BowUpgradeCatalog.CreateBow(progression);
        Assert(Math.Abs(archer.WeaponEquippedSeconds - 0.4) < 1e-9,
            "an upgraded copy of the same weapon keeps the bow drawn");

        string weaponAssets = Path.Combine(
            FindGameProjectDirectory(), "Classes", "Gruppe 9", "Assets", "Weapons");
        SpriteSheetAnimator sheet = new SpriteSheetAnimator(
            Path.Combine(weaponAssets, "Weapons.png"),
            Path.Combine(weaponAssets, "Weapons.json"));
        string[] usedAnimations =
        {
            "Sword", "SwordPoisen", "SwordFire", "SwordFireAttack", "SwordBlackSteel",
            "SwordBlackSteelLeach", "WizardStaff", "WizardStafElektic", "WizardStaffIce",
            "WizardStaffFire", "WizardStaffFrog", "WizardStaffFrogAttac", "BowShooting",
            "BowShootingScope", "Arow", "ArowAutoAim", "ArowPoisen", "ArowFire",
            "ArowFireFlying"
        };
        Assert(usedAnimations.All(name => sheet.GetFrameCount(name) > 0),
            "every weapon sprite clip used by the catalogs exists in Weapons.json");
        Assert(sheet.GetFrameCount("BowShooting") == 4
            && sheet.GetClipFrame("BowShooting", 3) != null
            && sheet.GetClipFrame("Unknown", 0) == null,
            "bow draw frames can be selected by index");
    }

    private static void TestPlayerKeepsStrikeFacing()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        Rect bounds = field.MovementArea;
        field.Player.Facing = PlayerFacing.Front;
        field.AttackWithPlayer(1, 0);
        Assert(field.Player.Facing == PlayerFacing.Right
            && field.Player.AttackFacing == PlayerFacing.Right,
            "the player turns into the direction of the strike");

        field.Player.Move(-1, 0, bounds);
        Assert(field.Player.Facing == PlayerFacing.Right,
            "walking during the strike keeps the strike facing");

        field.AttackWithPlayer(0, -1);
        Assert(field.Player.Facing == PlayerFacing.Right,
            "a click rejected by the cooldown does not turn the player");

        field.Player.UpdateStatusEffects(field.Player.EquippedWeapon.CooldownSeconds);
        field.Player.Move(-1, 0, bounds);
        Assert(field.Player.Facing == PlayerFacing.Left,
            "after the strike walking turns the player again");

        Assert(PlayerCharacter.IsRightHandBehindBody(PlayerFacing.Back)
            && PlayerCharacter.IsRightHandBehindBody(PlayerFacing.Right)
            && !PlayerCharacter.IsRightHandBehindBody(PlayerFacing.Front)
            && !PlayerCharacter.IsRightHandBehindBody(PlayerFacing.Left),
            "the right hand is behind the body seen from behind or from the right");
        Assert(field.Player.GetRightHandOffset(PlayerFacing.Front).X < 0
            && field.Player.GetRightHandOffset(PlayerFacing.Back).X > 0,
            "seen from the front the right hand is on the left of the screen");

        field.SelectPlayerWeapon(WeaponCatalog.Bow);
        field.Player.UpdateStatusEffects(field.Player.EquippedWeapon.EquipReadySeconds);
        field.AttackWithPlayer(0, 1);
        AttackVisualState arrow = field.ActiveAttackVisuals.Last(
            visual => visual.VisualKind == AttackVisualKind.Projectile);
        Vector hand = field.Player.GetRightHandOffset(PlayerFacing.Front);
        Assert(Math.Abs(arrow.ProjectileX - (field.Player.PositionX + hand.X)) < 1e-6
            && Math.Abs(arrow.ProjectileY - (field.Player.PositionY + hand.Y
                + WeaponAppearance.BowArrowTipDistance)) < 1e-6,
            "arrows are launched at the tip of the arrow nocked on the bow in the right hand");
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
            if (field.Enemies.Count == 0)
            {
                field.SpawnEnemy();
            }
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
            Assert(!enemy.IsAlive && field.Enemies.Count == 0,
                "defeated enemies are not replaced immediately");
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
        FindMenuButton(canvas, "Magischer Stab — "
            + WeaponCatalog.MagicWand.AttackPattern.Name).RaiseEvent(
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

    private static void TestArenaSkillTreeDependencies()
    {
        ArenaProgression progression = new ArenaProgression();
        ArenaSkillTreeDocument tree = new ArenaSkillTreeDocument
        {
            Name = "Dependency test",
            Nodes = new List<ArenaSkillTreeNode>
            {
                new ArenaSkillTreeNode
                {
                    SkillId = ArenaProgression.MaximumHealth, X = 0, Y = 0
                },
                new ArenaSkillTreeNode
                {
                    SkillId = ArenaProgression.MaximumSpeed, X = 128, Y = 0
                },
                new ArenaSkillTreeNode
                {
                    SkillId = PlayerAbilityCatalog.PowerBlast, X = 256, Y = 0
                },
                new ArenaSkillTreeNode
                {
                    SkillId = PlayerAbilityCatalog.HealingPulse, X = 384, Y = 0
                }
            },
            Connections = new List<ArenaSkillTreeConnection>
            {
                new ArenaSkillTreeConnection
                {
                    FromSkillId = ArenaProgression.MaximumHealth,
                    ToSkillId = PlayerAbilityCatalog.PowerBlast
                },
                new ArenaSkillTreeConnection
                {
                    FromSkillId = ArenaProgression.MaximumSpeed,
                    ToSkillId = PlayerAbilityCatalog.PowerBlast
                },
                new ArenaSkillTreeConnection
                {
                    FromSkillId = PlayerAbilityCatalog.PowerBlast,
                    ToSkillId = PlayerAbilityCatalog.HealingPulse
                }
            }
        };
        progression.SetSkillTree(tree);
        progression.SetGodMode(true);

        Assert(!progression.CanUpgradeSkill(PlayerAbilityCatalog.PowerBlast)
            && !progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast),
            "a skill with multiple prerequisites stays locked until every parent is bought");
        Assert(progression.TryUpgradePlayerStat(ArenaProgression.MaximumHealth)
            && !progression.CanUpgradeSkill(PlayerAbilityCatalog.PowerBlast),
            "one of two prerequisites is not enough to unlock a skill");
        Assert(progression.TryUpgradePlayerStat(ArenaProgression.MaximumSpeed)
            && progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast)
            && progression.CanUpgradeSkill(PlayerAbilityCatalog.HealingPulse)
            && progression.TryUpgradeSkill(PlayerAbilityCatalog.HealingPulse),
            "purchasing prerequisite skills unlocks the next nodes in order");

        tree.Connections.Add(new ArenaSkillTreeConnection
        {
            FromSkillId = PlayerAbilityCatalog.HealingPulse,
            ToSkillId = ArenaProgression.MaximumHealth
        });
        bool rejectedCycle = false;
        try
        {
            ArenaSkillTreeRepository.ValidateTree(tree);
        }
        catch (InvalidDataException)
        {
            rejectedCycle = true;
        }
        Assert(rejectedCycle,
            "skill tree validation rejects circular unlock dependencies");
    }

    private static void TestArenaEditorAssets()
    {
        IReadOnlyList<ArenaMapElement> mapElements = ArenaMapAssets.LoadMapElements();
        Assert(mapElements.Count == 11
            && mapElements.Any(element => element.Name == "GrassBush")
            && mapElements.Any(element => element.Name == "WayStraight"),
            "map editor reads the sprite names from the BackgroundElements atlas metadata");

        BitmapSource baseImage = ArenaMapAssets.LoadMapBase();
        Assert(baseImage.PixelWidth > 0 && baseImage.PixelHeight > 0,
            "map editor loads the selected BackgroundColour PNG");
        Assert(ArenaSkillTreeRepository.GetImplementedSkills().Count == 5,
            "skilltree editor exposes implemented player stats and abilities");

        bool rejectedLargeMap = false;
        try
        {
            ArenaMapAssets.ValidateDimensions(8192, 8192);
        }
        catch (ArgumentOutOfRangeException)
        {
            rejectedLargeMap = true;
        }
        Assert(rejectedLargeMap,
            "map size validation rejects dimensions exceeding the pixel budget");
    }

    private static void TestArenaMapGridSnapping()
    {
        Assert(ArenaMapEditorControl.SnapGridCoordinate(399, 1544) == 400,
            "map placements snap to the nearest 8-pixel grid coordinate");
        Assert(ArenaMapEditorControl.SnapGridCoordinate(1790, 1544) == 1544,
            "map grid snapping keeps an element inside the map boundary");
        Assert(ArenaMapEditorControl.SnapGridCoordinate(
                0, ArenaField.WallThickness, 1512, 8)
                == ArenaField.WallThickness
            && ArenaMapEditorControl.SnapGridCoordinate(
                1530, ArenaField.WallThickness, 1512, 8) == 1512,
            "map placements stay within the playable area between the arena walls");
    }

    private static void TestArenaSkillTreeStartNode()
    {
        ArenaProgression progression = new ArenaProgression();
        ArenaSkillTreeDocument tree = new ArenaSkillTreeDocument
        {
            Name = "Start node test",
            Nodes = new List<ArenaSkillTreeNode>
            {
                new ArenaSkillTreeNode
                {
                    SkillId = ArenaSkillTreeRepository.StartNodeId, X = 0, Y = 0
                },
                new ArenaSkillTreeNode
                {
                    SkillId = PlayerAbilityCatalog.PowerBlast, X = 300, Y = 0
                },
                new ArenaSkillTreeNode
                {
                    SkillId = PlayerAbilityCatalog.HealingPulse, X = 600, Y = 0
                },
                new ArenaSkillTreeNode
                {
                    SkillId = ArenaProgression.MaximumHealth, X = 300, Y = 200
                }
            },
            Connections = new List<ArenaSkillTreeConnection>
            {
                ArenaSkillTreeRepository.CreateStartConnection(
                    PlayerAbilityCatalog.PowerBlast),
                new ArenaSkillTreeConnection
                {
                    FromSkillId = PlayerAbilityCatalog.PowerBlast,
                    ToSkillId = PlayerAbilityCatalog.HealingPulse
                }
            }
        };
        ArenaSkillTreeRepository.ValidateTree(tree);
        progression.SetSkillTree(tree);
        progression.SetGodMode(true);

        Assert(progression.CanUpgradeSkill(PlayerAbilityCatalog.PowerBlast),
            "a skill connected to the tree start can be unlocked immediately");
        Assert(!progression.CanUpgradeSkill(PlayerAbilityCatalog.HealingPulse)
            && !progression.CanUpgradePlayerStat(ArenaProgression.MaximumHealth),
            "skills behind a prerequisite or disconnected from the start remain locked");
        Assert(progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast)
            && progression.CanUpgradeSkill(PlayerAbilityCatalog.HealingPulse),
            "buying a start-unlocked skill unlocks its dependent skill");

        tree.Connections.Add(new ArenaSkillTreeConnection
        {
            FromSkillId = PlayerAbilityCatalog.HealingPulse,
            ToSkillId = ArenaSkillTreeRepository.StartNodeId
        });
        bool rejectedIncomingStartConnection = false;
        try
        {
            ArenaSkillTreeRepository.ValidateTree(tree);
        }
        catch (InvalidDataException)
        {
            rejectedIncomingStartConnection = true;
        }
        Assert(rejectedIncomingStartConnection,
            "the tree start cannot depend on another skill");
        tree.Connections.RemoveAt(tree.Connections.Count - 1);

        Canvas connectionCanvas = new Canvas
        {
            Width = 900,
            Height = 300,
            Background = Brushes.Black
        };
        ArenaSkillTreeVisuals.DrawConnections(
            connectionCanvas, tree,
            connection => Brushes.DeepSkyBlue);
        Assert(connectionCanvas.Children.OfType<Polyline>().Count() >= 4
            && connectionCanvas.Children.OfType<Polygon>().Count() >= 4,
            "skill connections render as visible outlined wires with arrowheads");
        connectionCanvas.Measure(new Size(900, 300));
        connectionCanvas.Arrange(new Rect(0, 0, 900, 300));
        RenderTargetBitmap renderedConnections = new RenderTargetBitmap(
            900, 300, 96, 96, PixelFormats.Pbgra32);
        renderedConnections.Render(connectionCanvas);
        int wireStride = renderedConnections.PixelWidth * 4;
        byte[] wirePixels = new byte[wireStride * renderedConnections.PixelHeight];
        renderedConnections.CopyPixels(wirePixels, wireStride, 0);
        Assert(wirePixels.Where((value, index) => index % 4 != 3 && value > 32).Any(),
            "skill connection lines produce visible pixels on the editor canvas");

        Border vectorTile = ArenaSkillTreeVisuals.CreateNodeTile(
            "Kraftstoß", "Fähigkeit", ArenaSkillTreeTileState.Available);
        Assert(vectorTile.Width == ArenaSkillTreeVisuals.NodeWidth
            && vectorTile.Height == ArenaSkillTreeVisuals.NodeHeight
            && vectorTile.Child is StackPanel,
            "skill tiles use crisp vector surfaces instead of upscaled sprite pixels");

        ArenaSkillTreeDocument defaultTree =
            ArenaSkillTreeRepository.CreateDefaultTree();
        Assert(defaultTree.Nodes.Any(node =>
                ArenaSkillTreeRepository.IsStartNode(node.SkillId))
            && ArenaSkillTreeRepository.GetImplementedSkills().All(skill =>
                ArenaSkillTreeRepository.ArePrerequisitesMet(
                    defaultTree, skill.Id, id => 0)),
            "the default tree exposes every original skill from its start node");

        string legacyTreeName = "LegacyStartMigration" + Guid.NewGuid().ToString("N");
        string legacyTreePath = Path.Combine(
            ArenaSkillTreeRepository.TreeDirectory, legacyTreeName + ".json");
        try
        {
            ArenaSkillTreeRepository.SaveTree(legacyTreeName, new ArenaSkillTreeDocument
            {
                Name = legacyTreeName,
                Nodes = new List<ArenaSkillTreeNode>
                {
                    new ArenaSkillTreeNode
                    {
                        SkillId = ArenaProgression.MaximumHealth, X = 100, Y = 100
                    },
                    new ArenaSkillTreeNode
                    {
                        SkillId = PlayerAbilityCatalog.PowerBlast, X = 400, Y = 100
                    }
                },
                Connections = new List<ArenaSkillTreeConnection>
                {
                    new ArenaSkillTreeConnection
                    {
                        FromSkillId = ArenaProgression.MaximumHealth,
                        ToSkillId = PlayerAbilityCatalog.PowerBlast
                    }
                }
            });
            ArenaSkillTreeDocument migrated =
                ArenaSkillTreeRepository.LoadTreeByName(legacyTreeName);
            Assert(migrated.Nodes.Any(node =>
                    ArenaSkillTreeRepository.IsStartNode(node.SkillId))
                && migrated.Connections.Any(connection =>
                    ArenaSkillTreeRepository.IsStartNode(connection.FromSkillId)
                    && connection.ToSkillId == ArenaProgression.MaximumHealth)
                && !migrated.Connections.Any(connection =>
                    ArenaSkillTreeRepository.IsStartNode(connection.FromSkillId)
                    && connection.ToSkillId == PlayerAbilityCatalog.PowerBlast),
                "legacy trees gain a start edge only for skills that were initially available");
        }
        finally
        {
            if (File.Exists(legacyTreePath))
            {
                File.Delete(legacyTreePath);
            }
        }
    }

    private static void TestArenaSkillTreeForcedFork()
    {
        ArenaSkillTreeNode fork = new ArenaSkillTreeNode
        {
            SkillId = ArenaSkillTreeRepository.CreateForkNodeId(),
            X = 250,
            Y = 100,
            IsFork = true,
            ForceDecision = true
        };
        ArenaSkillTreeDocument tree = new ArenaSkillTreeDocument
        {
            Name = "Forced fork test",
            Nodes = new List<ArenaSkillTreeNode>
            {
                new ArenaSkillTreeNode
                {
                    SkillId = ArenaSkillTreeRepository.StartNodeId, X = 0, Y = 100
                },
                fork,
                new ArenaSkillTreeNode
                {
                    SkillId = PlayerAbilityCatalog.PowerBlast, X = 550, Y = 0
                },
                new ArenaSkillTreeNode
                {
                    SkillId = PlayerAbilityCatalog.HealingPulse, X = 550, Y = 200
                }
            },
            Connections = new List<ArenaSkillTreeConnection>
            {
                ArenaSkillTreeRepository.CreateStartConnection(fork.SkillId),
                new ArenaSkillTreeConnection
                {
                    FromSkillId = fork.SkillId,
                    ToSkillId = PlayerAbilityCatalog.PowerBlast
                },
                new ArenaSkillTreeConnection
                {
                    FromSkillId = fork.SkillId,
                    ToSkillId = PlayerAbilityCatalog.HealingPulse
                }
            }
        };
        ArenaSkillTreeRepository.ValidateTree(tree);

        ArenaProgression progression = new ArenaProgression();
        progression.SetSkillTree(tree);
        progression.SetGodMode(true);
        Assert(progression.CanUpgradeSkill(PlayerAbilityCatalog.PowerBlast)
            && progression.CanUpgradeSkill(PlayerAbilityCatalog.HealingPulse),
            "both exits of a forced fork are available before the player chooses");
        Assert(progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast)
            && !progression.CanUpgradeSkill(PlayerAbilityCatalog.HealingPulse)
            && !progression.TryUpgradeSkill(PlayerAbilityCatalog.HealingPulse),
            "buying one forced-fork exit permanently locks the other exit for the run");

        progression.Reset();
        progression.SetGodMode(true);
        Assert(progression.CanUpgradeSkill(PlayerAbilityCatalog.PowerBlast)
            && progression.CanUpgradeSkill(PlayerAbilityCatalog.HealingPulse),
            "resetting the run clears its forced-fork choice");

        fork.ForceDecision = false;
        progression.SetSkillTree(tree);
        Assert(progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast)
            && progression.CanUpgradeSkill(PlayerAbilityCatalog.HealingPulse)
            && progression.TryUpgradeSkill(PlayerAbilityCatalog.HealingPulse),
            "a non-exclusive fork allows the player to choose both exits");
    }

    private static void TestArenaMapDimensionInput()
    {
        ArenaIntegerTextBox input = new ArenaIntegerTextBox
        {
            Text = "1800",
            CaretIndex = 2
        };
        Assert(input.ProcessEditorKey(Key.D5, ModifierKeys.None)
            && input.Text == "18500",
            "map dimensions accept digits at the caret");
        Assert(input.ProcessEditorKey(Key.Back, ModifierKeys.None)
            && input.Text == "1800",
            "map dimensions can delete digits with backspace");
        Assert(input.ProcessEditorKey(Key.A, ModifierKeys.Control)
            && input.ProcessEditorKey(Key.NumPad7, ModifierKeys.None)
            && input.Text == "7",
            "map dimensions support replacing the selection with numpad digits");
    }

    private static void TestArenaDevToolMenu()
    {
        ArenaField field = new ArenaField();
        PainterG9 painter = new PainterG9();
        Canvas canvas = new Canvas { Width = 1024, Height = 768 };
        canvas.Measure(new Size(1024, 768));
        canvas.Arrange(new Rect(0, 0, 1024, 768));

        painter.PaintGameField(canvas, field);
        FindMenuButton(canvas, "Dev Tool").RaiseEvent(
            new RoutedEventArgs(Button.ClickEvent));
        painter.PaintGameField(canvas, field);
        Assert(FindMenuButton(canvas, "Zurück zum Startmenü") != null,
            "start menu opens the full-area map and skilltree editor");

        FindMenuButton(canvas, "Zurück zum Startmenü").RaiseEvent(
            new RoutedEventArgs(Button.ClickEvent));
        painter.PaintGameField(canvas, field);
        Assert(FindMenuButton(canvas, "Dev Tool") != null,
            "dev tool can return to the arena start menu");

        field.StartNewGame();
        field.ReturnToStartMenu();
        painter.PaintGameField(canvas, field);
        Assert(FindMenuButton(canvas, "Dev Tool") != null,
            "pause menu exposes the dev tool during an active run");
        FindMenuButton(canvas, "Dev Tool").RaiseEvent(
            new RoutedEventArgs(Button.ClickEvent));
        painter.PaintGameField(canvas, field);
        Assert(FindMenuButton(canvas, "Zurück zum Startmenü") != null,
            "dev tool opens from the pause menu without restarting the OOP window");
    }

    private static void TestArenaMapRestart()
    {
        string directory = ArenaMapAssets.BackgroundDirectory;
        string selectionPath = Path.Combine(directory, "ArenaMapSelection.json");
        bool hadSelection = File.Exists(selectionPath);
        string previousSelection = hadSelection
            ? File.ReadAllText(selectionPath)
            : null;
        string mapFileName = "ArenaTest-" + Guid.NewGuid().ToString("N") + ".png";

        try
        {
            string savedFileName = ArenaMapAssets.SaveMap(
                Path.GetFileNameWithoutExtension(mapFileName),
                512, 768, ArenaMapAssets.LoadMapBase(),
                Array.Empty<ArenaMapPlacement>());
            ArenaField field = new ArenaField();
            field.StartNewGame();
            field.Progression.AwardKill(WeaponCatalog.Sword);
            field.ReturnToStartMenu();

            field.SelectMap(savedFileName);
            Assert(field.MapFileName == savedFileName
                && field.WorldWidth == 512 && field.WorldHeight == 768
                && field.HasActiveGame && !field.IsGameStarted
                && field.Progression.TotalKills == 0,
                "applying a map from the pause menu resets the arena run and returns to pause");
        }
        finally
        {
            string savedPath = Path.Combine(directory, mapFileName);
            if (File.Exists(savedPath))
            {
                File.Delete(savedPath);
            }
            if (hadSelection)
            {
                File.WriteAllText(selectionPath, previousSelection);
            }
            else if (File.Exists(selectionPath))
            {
                File.Delete(selectionPath);
            }
        }
    }

    private static void TestArenaMapPlacementTransforms()
    {
        string directory = ArenaMapAssets.BackgroundDirectory;
        string mapFileName = "ArenaTransformTest-"
            + Guid.NewGuid().ToString("N") + ".png";
        BitmapSource sprite = BitmapSource.Create(
            2, 1, 96, 96, PixelFormats.Bgra32, null,
            new byte[] { 0, 0, 255, 255, 255, 0, 0, 255 }, 8);
        sprite.Freeze();

        try
        {
            ArenaMapAssets.SaveMap(
                Path.GetFileNameWithoutExtension(mapFileName),
                512, 512, ArenaMapAssets.LoadMapBase(),
                new[]
                {
                    new ArenaMapPlacement(sprite, 10, 10, 0, true),
                    new ArenaMapPlacement(sprite, 20, 20, 90)
                });
            ArenaMapDefinition map = ArenaMapAssets.LoadMap(mapFileName);
            byte[] pixels = new byte[512 * 512 * 4];
            map.Image.CopyPixels(pixels, 512 * 4, 0);

            bool IsRed(int x, int y)
            {
                int offset = (y * 512 + x) * 4;
                return pixels[offset] == 0 && pixels[offset + 2] == 255;
            }
            bool IsBlue(int x, int y)
            {
                int offset = (y * 512 + x) * 4;
                return pixels[offset] == 255 && pixels[offset + 2] == 0;
            }

            Assert(IsBlue(10, 10) && IsRed(11, 10),
                "saved map placements preserve horizontal mirroring");
            Assert((IsRed(20, 20) && IsBlue(20, 21))
                || (IsBlue(20, 20) && IsRed(20, 21)),
                "saved map placements preserve 90-degree rotation");
        }
        finally
        {
            string savedPath = Path.Combine(directory, mapFileName);
            if (File.Exists(savedPath))
            {
                File.Delete(savedPath);
            }
        }
    }

    private static void TestArenaSkillTreeRestart()
    {
        string directory = ArenaSkillTreeRepository.TreeDirectory;
        string selectionPath = Path.Combine(directory, "SelectedSkillTree.txt");
        bool hadSelection = File.Exists(selectionPath);
        string previousSelection = hadSelection
            ? File.ReadAllText(selectionPath)
            : null;

        try
        {
            ArenaField field = new ArenaField();
            field.StartNewGame();
            field.Progression.AwardKill(WeaponCatalog.Sword);
            field.ReturnToStartMenu();

            field.SelectSkillTree(ArenaSkillTreeRepository.DefaultTreeName);
            Assert(field.HasActiveGame && !field.IsGameStarted
                && field.Progression.TotalKills == 0
                && field.Progression.SkillTree.Name
                    == ArenaSkillTreeRepository.DefaultTreeName,
                "applying a skilltree from the pause menu resets the run and returns to pause");
        }
        finally
        {
            if (hadSelection)
            {
                File.WriteAllText(selectionPath, previousSelection);
            }
            else if (File.Exists(selectionPath))
            {
                File.Delete(selectionPath);
            }
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

        // Five sword kills earn one skill point but no bow points: the bow can still be
        // upgraded, paid entirely with skill points.
        ArenaField skillOnlyField = new ArenaField();
        skillOnlyField.StartNewGame();
        for (int i = 0; i < ArenaProgression.KillsPerUniversalPoint; i++)
        {
            skillOnlyField.Progression.AwardKill(WeaponCatalog.Sword);
        }
        skillOnlyField.SelectPlayerWeapon(WeaponCatalog.Bow);
        Assert(skillOnlyField.CanUpgradeWeapon(
                BowUpgradeCatalog.AutoAim, out int skillOnlyCost,
                out int skillOnlyWeaponPoints, out int skillOnlySkillPoints)
            && skillOnlyWeaponPoints == 0 && skillOnlySkillPoints == skillOnlyCost,
            "weapons without weapon points can be upgraded with skill points only");
        int skillPointsBefore = skillOnlyField.Progression.UniversalPoints;
        Assert(skillOnlyField.TryUpgradeWeaponWithSkillPoints(BowUpgradeCatalog.AutoAim)
            && skillOnlyField.Progression.UniversalPoints
                == skillPointsBefore - skillOnlyCost
            && skillOnlyField.Progression.GetWeaponPoints(WeaponCatalog.Bow.Id) == 0
            && skillOnlyField.Progression.GetWeaponUpgradeRank(BowUpgradeCatalog.AutoAim) == 1,
            "a skill-point-only weapon upgrade spends the skill points");

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
        field.Progression.SetSkillTree(ArenaSkillTreeRepository.CreateDefaultTree());
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

    private static void TestPushableEntityMovement()
    {
        ArenaField pushableField = new ArenaField();
        pushableField.StartNewGame();
        pushableField.Player.PositionX = 650;
        pushableField.Player.PositionY = 600;
        PursuerEnemy pusher = pushableField.Enemies[0];
        pusher.PositionX = 500;
        pusher.PositionY = 600;
        pushableField.SpawnEnemy();
        PursuerEnemy pushable = pushableField.Enemies[1];
        pushable.PositionX = 540;
        pushable.PositionY = 600;
        pushable.CanBePushed = true;

        pushableField.UpdateEnemyPositions();

        Assert(pushable.PositionX > 551
            && pushable.PositionX - pusher.PositionX >= pusher.Size,
            "moving entities push marked entities out of their way");

        ArenaField fixedField = new ArenaField();
        fixedField.StartNewGame();
        fixedField.Player.PositionX = 650;
        fixedField.Player.PositionY = 600;
        PursuerEnemy blockedMover = fixedField.Enemies[0];
        blockedMover.PositionX = 500;
        blockedMover.PositionY = 600;
        fixedField.SpawnEnemy();
        PursuerEnemy fixedEntity = fixedField.Enemies[1];
        fixedEntity.PositionX = 540;
        fixedEntity.PositionY = 600;
        fixedEntity.CanBePushed = false;

        fixedField.UpdateEnemyPositions();

        Assert(Math.Abs(blockedMover.PositionX - 500) < 0.001
            && fixedEntity.PositionX > 540,
            "unmarked entities remain solid and block other entity movement");
    }

    private static void TestTimedEnemySpawningAndLimit()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        Assert(field.Enemies.Count == 1,
            "a run starts with one enemy");

        int ticksPerSpawnInterval = (int)(
            ArenaField.EnemySpawnIntervalSeconds / 0.04);
        for (int tick = 0; tick < ticksPerSpawnInterval - 1; tick++)
        {
            field.UpdateEnemyPositions();
        }
        Assert(field.Enemies.Count == 1,
            "the spawn timer does not add an enemy before seven seconds");

        field.UpdateEnemyPositions();
        Assert(field.Enemies.Count == 2,
            "the spawn timer adds an enemy after seven seconds");

        while (field.Enemies.Count < ArenaField.MaximumEnemyCount)
        {
            field.SpawnEnemy();
        }
        field.SpawnEnemy();
        Assert(field.Enemies.Count == ArenaField.MaximumEnemyCount,
            "manual and timed spawning never exceed the enemy limit");
    }

    private static void TestDeathEnemyAndPurchaseSoundEvents()
    {
        ArenaField field = new ArenaField();
        field.Progression.SetSkillTree(ArenaSkillTreeRepository.CreateDefaultTree());
        int playerDeaths = 0;
        List<int> enemyDamage = new List<int>();
        List<Entity> enemyDeaths = new List<Entity>();
        int purchases = 0;
        field.PlayerDied += () => playerDeaths++;
        field.NonPlayerEntityDamaged += (entity, amount) => enemyDamage.Add(amount);
        field.NonPlayerEntityDied += enemyDeaths.Add;
        field.Progression.UpgradePurchased += () => purchases++;

        field.Player.TakeDamage(10);
        Assert(playerDeaths == 0, "a survivable hit is not a player death");
        field.Player.TakeDamage(field.Player.CurrentHitPoints);
        field.Player.TakeDamage(10);
        Assert(playerDeaths == 1, "the player death is reported exactly once");

        field.ResetPlayer();
        PursuerEnemy enemy = field.Enemies[0];
        enemy.TakeDamage(5);
        enemy.TakeDamage(enemy.CurrentHitPoints);
        Assert(enemyDamage.Count == 2 && enemyDeaths.SequenceEqual(new Entity[] { enemy })
            && playerDeaths == 1,
            "enemy hits and enemy deaths are reported separately from the player");

        field.StartNewGame();
        for (int index = 0; index < 50 && field.Enemies.Count < 2; index++)
        {
            field.SpawnEnemy();
        }
        field.Enemies[field.Enemies.Count - 1].TakeDamage(1);
        Assert(enemyDamage.Count == 3, "spawned enemies report their damage as well");

        Assert(field.Progression.TryUpgradeSword(
            SwordUpgradeCatalog.StrikeDamage, UpgradeCurrency.Universal)
            && field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast),
            "fixture buys a weapon upgrade and a skill");
        Assert(purchases == 2, "every successful purchase plays the pay sound");

        // Keep buying until points or ranks run out: only successes may play the sound.
        int successes = 0;
        int failures = 0;
        for (int index = 0; index < 30; index++)
        {
            if (field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast))
            {
                successes++;
            }
            else
            {
                failures++;
            }
        }
        Assert(failures > 0 && purchases == 2 + successes,
            "failed purchases play no pay sound");

        field.ToggleGodMode();
        Assert(field.Progression.TryUpgradePlayerStat(ArenaProgression.MaximumHealth)
            && purchases == 3 + successes,
            "godmode purchases and player stat upgrades play the pay sound too");
    }

    private static void TestSwordSwingSoundEvents()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        int swings = 0;
        field.PlayerSwordSwung += () => swings++;

        Assert(field.Progression.TryUpgradeSword(
            SwordUpgradeCatalog.StrikeDamage, UpgradeCurrency.Universal)
            && field.Progression.TryUpgradeSword(
                SwordUpgradeCatalog.Spin, UpgradeCurrency.Universal)
            && field.AttackWithWeaponAbility(0),
            "fixture starts the sword spin");
        for (int index = 0; index < 40; index++)
        {
            field.UpdateCombat(0.04);
        }
        Assert(swings == SwordUpgradeCatalog.SpinRotations,
            "the sword spin swings once per revolution");

        field.AttackWithPlayer(1, 0);
        Assert(swings == SwordUpgradeCatalog.SpinRotations + 1,
            "a normal sword slash swings once");
        field.AttackWithPlayer(1, 0);
        Assert(swings == SwordUpgradeCatalog.SpinRotations + 1,
            "an attack blocked by its cooldown makes no swing");

        field.SelectPlayerWeapon(WeaponCatalog.MagicWand);
        for (int index = 0; index < 40; index++)
        {
            field.UpdateCombat(0.04);
        }
        field.AttackWithPlayer(1, 0);
        Assert(swings == SwordUpgradeCatalog.SpinRotations + 1,
            "other weapons make no sword swing");
    }

    private static void TestSprintAndLowHealthSoundEvents()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        int turns = 0;
        field.PlayerTurned += () => turns++;

        Assert(!field.IsSprintEnabled, "a new run starts without sprint");
        field.HandleKeyDown(Key.LeftCtrl);
        Assert(!field.IsSprintEnabled, "pressing Ctrl alone does not toggle yet");
        field.HandleKeyUp(Key.LeftCtrl);
        Assert(field.IsSprintEnabled, "tapping Ctrl turns sprint on");
        field.HandleKeyDown(Key.RightCtrl);
        field.HandleKeyUp(Key.RightCtrl);
        Assert(!field.IsSprintEnabled, "tapping Ctrl again turns sprint off");
        double zoom = field.ZoomLevel;
        field.HandleKeyDown(Key.LeftCtrl);
        field.HandleZoomWheel(120);
        field.HandleKeyUp(Key.LeftCtrl);
        Assert(!field.IsSprintEnabled && field.ZoomLevel != zoom,
            "Ctrl + mouse wheel zooms without toggling sprint");
        field.HandleKeyUp(Key.LeftCtrl);
        Assert(!field.IsSprintEnabled, "a release without a press toggles nothing");

        field.UpdatePlayerMotion(true, 1, 0);
        Assert(field.IsPlayerInMotion && !field.IsPlayerSprinting && turns == 0,
            "walking starts the engine at walking level without a turn sound");
        field.UpdatePlayerMotion(true, 1, -1);
        Assert(turns == 0, "a 45 degree adjustment is not a turn");
        field.UpdatePlayerMotion(true, 0, -1);
        Assert(turns == 1, "a 90 degree change while walking is a turn");
        field.ToggleSprint();
        Assert(field.IsPlayerSprinting, "toggling sprint while moving switches to sprinting");
        field.UpdatePlayerMotion(true, 0, 1);
        Assert(turns == 2, "reversing while sprinting is a turn");
        field.UpdatePlayerMotion(true, 0, 1);
        Assert(turns == 2, "keeping the direction makes no further turn sound");

        field.UpdatePlayerMotion(false, 0, 1);
        Assert(!field.IsPlayerInMotion && field.IsSprintEnabled,
            "stopping ends the motion but keeps sprint mode on");
        field.UpdatePlayerMotion(true, 1, 0);
        Assert(field.IsPlayerSprinting && turns == 2,
            "moving off in another direction is a start, not a turn");
        field.UpdatePlayerMotion(true, 0, 0);
        Assert(!field.IsPlayerInMotion, "no movement input ends the motion");

        field.UpdatePlayerMotion(true, 1, 0);
        field.HandleKeyDown(Key.Escape);
        Assert(!field.IsGameStarted && !field.IsPlayerInMotion && field.IsSprintEnabled,
            "pausing silences the engine but keeps sprint mode");
        field.HandleKeyDown(Key.LeftCtrl);
        field.HandleKeyUp(Key.LeftCtrl);
        Assert(field.IsSprintEnabled, "Ctrl does not toggle sprint in the pause menu");
        field.HandleKeyDown(Key.Escape);
        field.RestartGame();
        Assert(!field.IsSprintEnabled, "a new run resets sprint mode");

        int lowHealthWarnings = 0;
        field.PlayerHealthLow += () => lowHealthWarnings++;
        int lowHealthLimit = (int)(field.Player.MaxHitPoints * ArenaField.LowHealthFraction);
        field.Player.TakeDamage(field.Player.CurrentHitPoints - lowHealthLimit - 1);
        Assert(lowHealthWarnings == 0, "health above the limit gives no warning");
        field.Player.TakeDamage(1);
        Assert(lowHealthWarnings == 1, "dropping to low health warns once");
        field.Player.TakeDamage(1);
        Assert(lowHealthWarnings == 1, "further hits at low health do not repeat the warning");
        field.Player.Heal(field.Player.MaxHitPoints);
        field.Player.TakeDamage(field.Player.MaxHitPoints - lowHealthLimit);
        Assert(lowHealthWarnings == 2, "healing and dropping again warns again");
        field.Player.TakeDamage(field.Player.CurrentHitPoints);
        Assert(lowHealthWarnings == 2 && !field.IsPlayerSprinting,
            "a lethal hit plays the death sound instead of a warning");
    }

    private static void TestSoundFilesMatchGroups()
    {
        string sounds = Path.Combine(
            FindGameProjectDirectory(), "Classes", "Gruppe 9", "Assets", "Sounds");
        string[] patterns =
        {
            "playerDamage_*.wav", "player_death*.wav", "nonPlayerEntityDamage_*.wav",
            "entity_death_*.wav", "sword_swing_*.wav", "paySkill*.wav",
            "playerLowHealth*.wav", "sprint_loop_*.wav", "sprint_turn_neeow_*.wav",
            "sprint_stop_brake*.wav", "ui_menu_click*.wav"
        };
        foreach (string pattern in patterns)
        {
            Assert(Directory.GetFiles(sounds, pattern).Length > 0,
                "sound group " + pattern + " has at least one file");
        }
        Assert(Directory.GetFiles(sounds, "*.wav").All(file => patterns.Any(pattern =>
                Directory.GetFiles(sounds, pattern).Contains(file))),
            "every sound file belongs to a sound group");
    }

    private static void TestPlayerDamageEvents()
    {
        ArenaField field = new ArenaField();
        List<int> damageEvents = new List<int>();
        field.PlayerDamaged += damageEvents.Add;

        field.Player.TakeDamage(10);
        field.Player.TakeDamage(0);
        Assert(damageEvents.SequenceEqual(new[] { 10 }),
            "player damage is reported once with the hit points lost");

        field.Player.IsInvulnerable = true;
        field.Player.TakeDamage(10);
        field.Player.IsInvulnerable = false;
        Assert(damageEvents.Count == 1, "blocked damage plays no damage sound");

        int remainingHitPoints = field.Player.CurrentHitPoints;
        field.Player.TakeDamage(remainingHitPoints + 50);
        Assert(damageEvents.Count == 2 && damageEvents[1] == remainingHitPoints,
            "a lethal hit reports only the hit points the player still had");

        PlayerCharacter oldPlayer = field.Player;
        field.ResetPlayer();
        oldPlayer.Heal(10);
        field.Player.TakeDamage(5);
        Assert(damageEvents.Count == 3 && damageEvents[2] == 5,
            "after a restart only the new player's damage is reported");

        field.Enemies[0].TakeDamage(5);
        Assert(damageEvents.Count == 3, "enemy damage is not reported as player damage");
    }

    private static void TestSoundGroupSelection()
    {
        CountingSound[] sounds =
        {
            new CountingSound(), new CountingSound(), new CountingSound()
        };
        ArenaSoundGroup group = new ArenaSoundGroup(sounds, 0.1, new Random(7));

        int previous = -1;
        bool repeated = false;
        for (int index = 0; index < 60; index++)
        {
            int played = group.Play(index);
            repeated |= played == previous;
            previous = played;
        }
        Assert(!repeated, "a random damage sound never repeats directly");
        Assert(sounds.All(sound => sound.PlayCount > 0)
            && sounds.Sum(sound => sound.PlayCount) == 60,
            "all damage sound variants are used");

        Assert(group.Play(100) >= 0 && group.Play(100.05) == -1 && group.Play(100.2) >= 0,
            "hits within the minimum interval are merged into one sound");
        Assert(new ArenaSoundGroup(new IArenaSoundEffect[0], 0).Play(0) == -1,
            "a missing sound folder leaves the event silent");
    }

    private sealed class CountingSound : IArenaSoundEffect
    {
        public int PlayCount { get; private set; }

        public void Play()
        {
            PlayCount++;
        }
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
        // Split buttons only show a short label, so their full name is the automation name.
        if (button != null && (Equals(button.Content, content)
            || System.Windows.Automation.AutomationProperties.GetName(button) == content))
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
