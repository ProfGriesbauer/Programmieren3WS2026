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
using Rectangle = System.Windows.Shapes.Rectangle;
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
        // Tests use their own (empty) enemy-kind folder: kinds made by the team must
        // neither change the results nor be overwritten.
        string enemyTypeFolder = Path.Combine(
            Path.GetTempPath(), "arena-enemy-types-" + Guid.NewGuid().ToString("N"));
        string regionFolder = Path.Combine(
            Path.GetTempPath(), "arena-regions-" + Guid.NewGuid().ToString("N"));
        try
        {
            TestShippedEnemyTypesLoad();
            EnemyTypeRepository.DirectoryOverride = enemyTypeFolder;
            Directory.CreateDirectory(regionFolder);
            ArenaMapRegionRepository.DirectoryOverride = regionFolder;
            ArenaSkillTreeRepository.SelectTree(
                ArenaSkillTreeRepository.DefaultTreeName);
            TestEnemyTypeDefaultsAndValidation();
            TestEnemyTypeRepositoryRoundTrip();
            TestEnemyKindsSpawnOnOwnTimers();
            TestEnemySpawnChance();
            TestDecimalNumberInput();
            TestEnemyEditorWorkflow();
            TestEnemyAttackResolution();
            TestEnemyResistancesAndRegeneration();
            TestEnemyKillRewards();
            TestEnemyBehaviorTypes();
            TestEnemyChoosesAttackByDistance();
            TestEnemySpriteSheetsAndTint();
            TestEnemyEditorAttacks();
            TestMapRegionModelAndRepository();
            TestEnemySpawnPositionFinder();
            TestEnemiesSpawnInRegions();
            TestRegionEditor();
            TestAnimatorMirrorsMissingSides();
            TestFrogAttackAnimationNames();
            TestFrogDevoursEntities();
            TestFrogHopsAndRespawnsAlone();
            TestFrogCroaks();
            TestMineTurtleExplodesWhenSteppedOn();
            TestDevourRingAndHiddenHealthBar();
            TestWindupTextFormat();
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
            TestEnemyHitsPushPlayer();
            TestEnemyAlertAfterDirectHits();
            TestShockAndHealSoundEvents();
            TestChainLightning();
            TestSkillPointQuestionPreference();
            TestSaveGamesRoundTrip();
            TestDeathRespawnAndHighScores();
            TestFootstepEvents();
            TestTextureHitboxes();
            TestFireTrailsAndStrikeTraces();
            TestStaffSpritesAndHealEvents();
            TestParticlesFollowEvents();
            TestMultiplePlayerAbilities();
            TestMirrorImageAbility();
            TestMirrorImageEnemyTargeting();
            TestArenaSkillTreeDependencies();
            TestArenaSkillTreeRankSplitting();
            TestArenaSkillTreeStartNode();
            TestArenaSkillTreeForcedFork();
            TestArenaSkillTreeForkInputThreshold();
            TestArenaMapDimensionInput();
            TestWindowKeyboardAllowsTextEntry();
            TestArenaEditorAssets();
            TestArenaMapGridSnapping();
            TestRandomBushPixelMatching();
            TestArenaMapPlacementTransforms();
            TestArenaDevToolMenu();
            TestArenaMapChangeKeepsRun();
            TestArenaSkillTreeChangeKeepsRun();
            TestKeyBindingPersistenceAndConflicts();
            TestInputSeparatesAbilityTypes();
            TestWeaponWheelHoldAndReleaseSelection();
            TestDevShortcutsSaveShortcutAndWeaponTreeKey();
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
            EnemyTypeRepository.DirectoryOverride = null;
            if (Directory.Exists(enemyTypeFolder))
            {
                Directory.Delete(enemyTypeFolder, true);
            }
            ArenaMapRegionRepository.DirectoryOverride = null;
            if (Directory.Exists(regionFolder))
            {
                Directory.Delete(regionFolder, true);
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

    // The kinds in the repository (Assets/Enemies/Types) load; only reads them.
    private static void TestShippedEnemyTypesLoad()
    {
        List<string> problems = new List<string>();
        IReadOnlyList<EnemyType> types = EnemyTypeRepository.LoadAll(problems);
        Assert(types.Count >= 1 && types[0].Id == EnemyType.StandardId,
            "the shipped enemy kinds load with Standard first");
        Assert(problems.Count == 0,
            "the shipped enemy kinds have no problems: " + String.Join(" | ", problems));
        _shippedFrog = types.FirstOrDefault(type => type.Id == "Frog");
        Assert(_shippedFrog != null && _shippedFrog.MaxHitPoints == 1
            && _shippedFrog.StartCount == 1 && _shippedFrog.MaxAlive == 1
            && _shippedFrog.TargetSelection == EnemyTargetSelection.RandomEntity
            && _shippedFrog.MovementStyle == EnemyMovementStyle.Hop
            && _shippedFrog.SpriteSheet == "Frog.png"
            && _shippedFrog.Attacks.Count == 1
            && _shippedFrog.Attacks[0].Weapon.TargetingMode == WeaponTargetingMode.Everyone
            && _shippedFrog.Attacks[0].Weapon.Effect.Description.Contains("devours"),
            "the shipped frog: one hit point, alone, hops to random entities and devours");
        _shippedTurtle = types.FirstOrDefault(type => type.Id == "MineTurtle");
        EnemyAttack blast = _shippedTurtle?.Attacks[0];
        Assert(_shippedTurtle != null && !_shippedTurtle.BlocksMovement
            && !_shippedTurtle.CanBePushed && _shippedTurtle.SpeedFactor == 0
            && _shippedTurtle.Behavior == EnemyBehavior.Turret
            && _shippedTurtle.SpriteSheet == "Mine Turtel.png"
            && blast.Trigger == EnemyAttackTrigger.Contact
            && blast.TriggeredBy == EnemyTriggerSource.Player
            && blast.WindupSteps.Select(step => step.Clip).SequenceEqual(new[] { "Aktive", "Hello" })
            && blast.WindupSteps[1].Sound == "MineTurtleHello" && blast.Sound == "PowerBlast"
            && blast.KillsAttacker && blast.Weapon.AttackPattern is RadialAttackPattern
            && blast.Weapon.TargetingMode == WeaponTargetingMode.Everyone
            && blast.Weapon.Effect.Description.Contains("kills at once"),
            "the shipped mine turtle stands still, lets the player step on it and explodes lethally");
    }

    private static EnemyType _shippedFrog;
    private static EnemyType _shippedTurtle;

    private static void TestEnemyTypeDefaultsAndValidation()
    {
        EnemyType standard = EnemyType.Resolve(new EnemyTypeDefinition { Id = "Leer" });
        Assert(standard.Name == "Leer" && standard.Enabled
            && standard.MaxHitPoints == 30 && Math.Abs(standard.SpeedFactor - 0.75) < 1e-9
            && standard.Size == PursuerEnemy.HitboxSize && standard.CanBePushed
            && standard.SightRange == 200 && standard.SightBonusAfterDetection == 50
            && standard.AlertDurationSeconds == 20 && standard.AlertRadius == 100
            && standard.StartCount == 1
            && standard.SpawnIntervalSeconds == ArenaField.EnemySpawnIntervalSeconds
            && standard.FirstSpawnDelaySeconds == ArenaField.EnemySpawnIntervalSeconds
            && standard.MaxAlive == ArenaField.MaximumEnemyCount
            && standard.GroupSizeMin == 1 && standard.GroupSizeMax == 1
            && standard.MinDistanceToPlayer == 250 && !standard.MaxKills.HasValue,
            "an empty enemy kind has the values of the original enemy");

        List<string> problems = new List<string>();
        EnemyType repaired = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Kaputt",
            MaxHitPoints = 0,
            SpawnIntervalSeconds = -1,
            GroupSizeMin = 3,
            GroupSizeMax = 2,
            MinKills = 5,
            MaxKills = 5,
            SightRange = 300
        }, problems);
        Assert(repaired.MaxHitPoints == 30
            && repaired.SpawnIntervalSeconds == ArenaField.EnemySpawnIntervalSeconds
            && repaired.GroupSizeMin == 3 && repaired.GroupSizeMax == 3
            && !repaired.MaxKills.HasValue && problems.Count == 4,
            "invalid values fall back to the standard values and are reported");
        Assert(repaired.MinDistanceToPlayer == 350,
            "the spawn distance follows the kind's own sight range");

        bool rejected = false;
        try
        {
            EnemyType.Resolve(new EnemyTypeDefinition { Id = "../böse" });
        }
        catch (ArgumentException)
        {
            rejected = true;
        }
        Assert(rejected, "enemy kind ids cannot be file paths");
    }

    private static void TestEnemyTypeRepositoryRoundTrip()
    {
        EnemyTypeRepository.SaveDefinition(new EnemyTypeDefinition
        {
            Id = "Brute",
            Name = "Schläger",
            MaxHitPoints = 80,
            SpeedFactor = 0.5
        });
        string json = File.ReadAllText(
            Path.Combine(EnemyTypeRepository.TypesDirectory, "Brute.json"));
        Assert(json.Contains("\"MaxHitPoints\": 80") && !json.Contains("SightRange"),
            "only the values that differ from the standard enemy are written");

        List<string> problems = new List<string>();
        IReadOnlyList<EnemyType> types = EnemyTypeRepository.LoadAll(problems);
        EnemyType brute = types.FirstOrDefault(type => type.Id == "Brute");
        Assert(types[0].Id == EnemyType.StandardId && brute != null
            && brute.Name == "Schläger" && brute.MaxHitPoints == 80
            && brute.SightRange == 200 && problems.Count == 0,
            "saved kinds load again, unset values use the standard enemy");

        File.WriteAllText(Path.Combine(EnemyTypeRepository.TypesDirectory, "Defekt.json"),
            "{ kein json");
        problems.Clear();
        types = EnemyTypeRepository.LoadAll(problems);
        Assert(types.All(type => type.Id != "Defekt") && problems.Count == 1,
            "a broken file is skipped and reported instead of stopping the game");

        EnemyTypeRepository.DeleteDefinition("Brute");
        EnemyTypeRepository.DeleteDefinition("Defekt");
        Assert(EnemyTypeRepository.GetTypeIds().SequenceEqual(new[] { EnemyType.StandardId }),
            "deleted kinds disappear; Standard always exists");
    }

    private static void TestEnemySpawnChance()
    {
        Assert(EnemyType.Standard.SpawnChancePercent == 100,
            "enemies spawn on every attempt by default");
        List<string> problems = new List<string>();
        EnemyType.Resolve(new EnemyTypeDefinition { Id = "Zuviel", SpawnChancePercent = 150 },
            problems);
        Assert(problems.Count == 1, "a spawn chance above 100 % is reported");

        EnemyType never = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Never", StartCount = 0, SpawnIntervalSeconds = 1, SpawnChancePercent = 0
        });
        EnemyType always = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Always", StartCount = 0, SpawnIntervalSeconds = 1, MaxAlive = 3
        });
        EnemyType coin = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Coin", StartCount = 0, SpawnIntervalSeconds = 1, SpawnChancePercent = 50,
            MaxAlive = 1000
        });
        ArenaField field = new ArenaField();
        field.SetEnemyTypes(new[] { never, always });
        field.StartNewGame();
        field.Player.IsInvulnerable = true;
        for (int tick = 0; tick < 3 / ArenaField.GameTickDurationSeconds + 1; tick++)
        {
            field.UpdateEnemyPositions();
        }
        Assert(field.Enemies.Count(enemy => enemy.TypeId == "Never") == 0
            && field.Enemies.Count(enemy => enemy.TypeId == "Always") == 3,
            "0 % never spawns, 100 % spawns on every attempt");

        // Many attempts at 50 %: some succeed, some fail (statistically certain).
        EnemySpawnDirector director = new EnemySpawnDirector();
        CountingSpawnHost host = new CountingSpawnHost();
        director.Reset(new[] { coin });
        for (int attempt = 0; attempt < 400; attempt++)
        {
            director.Update(1, new[] { coin }, host);
        }
        Assert(host.Spawned > 100 && host.Spawned < 300,
            "a 50 % chance spawns on about half of the attempts (" + host.Spawned + "/400)");
    }

    private sealed class CountingSpawnHost : IEnemySpawnHost
    {
        public int Spawned;
        public int LivingEnemyCount { get { return 0; } }
        public int CountLivingEnemies(string typeId) { return 0; }
        public int Kills { get { return 0; } }
        public double RunSeconds { get { return 1000; } }
        public bool IsPlayerConditionMet(EnemyType type) { return true; }
        public bool TrySpawnEnemy(EnemyType type)
        {
            Spawned++;
            return true;
        }
    }

    private static void TestDecimalNumberInput()
    {
        ArenaIntegerTextBox integerBox = new ArenaIntegerTextBox { Text = "1" };
        integerBox.CaretIndex = 1;
        Assert(!integerBox.ProcessEditorKey(Key.OemComma, ModifierKeys.None)
            && integerBox.Text == "1",
            "integer fields ignore the decimal separator");
        ArenaIntegerTextBox numberBox = new ArenaIntegerTextBox
        {
            Text = "0",
            AllowDecimalSeparator = true
        };
        numberBox.CaretIndex = 1;
        numberBox.ProcessEditorKey(Key.OemPeriod, ModifierKeys.None);
        numberBox.ProcessEditorKey(Key.D7, ModifierKeys.None);
        numberBox.ProcessEditorKey(Key.OemComma, ModifierKeys.None);
        numberBox.ProcessEditorKey(Key.D5, ModifierKeys.None);
        Assert(numberBox.Text == "0,75",
            "decimal fields accept one separator (comma or point, shown as comma)");
    }

    private static void TestEnemyEditorWorkflow()
    {
        string previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = FindGameProjectDirectory();
            ArenaField field = new ArenaField();
            List<string> errors = new List<string>();
            ArenaEnemyEditorControl editor = new ArenaEnemyEditorControl(field)
            {
                Confirm = message => true,
                ShowError = errors.Add
            };
            Assert(editor.LoadedId == EnemyType.StandardId && !editor.IsDirty
                && editor.GetFieldValue("Lebenspunkte") == "",
                "the editor opens with the standard enemy; empty fields mean standard values");

            FindMenuButtonIn(editor, "Neu").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert(editor.LoadedId == null && editor.IsDirty
                && editor.GetFieldValue("Id (Dateiname)") == "NeuerGegner",
                "New starts an unsaved kind with a free id");
            editor.SetFieldValue("Id (Dateiname)", "Tank");
            editor.SetFieldValue("Lebenspunkte", "120");
            editor.SetFieldValue("Tempo (× Spielertempo)", "0,4");
            editor.SetFieldValue("Lässt sich schubsen", "nein");
            editor.SetFieldValue("Spawn-Wahrscheinlichkeit (%)", "50");
            Assert(editor.PreviewText.Contains("120 LP") && editor.PreviewText.Contains("50 %"),
                "the preview follows the form");
            Assert(editor.SaveType() && editor.LoadedId == "Tank" && !editor.IsDirty,
                "a valid kind is saved");
            string json = File.ReadAllText(
                Path.Combine(EnemyTypeRepository.TypesDirectory, "Tank.json"));
            EnemyType tank = field.EnemyTypes.FirstOrDefault(type => type.Id == "Tank");
            Assert(json.Contains("\"SpeedFactor\": 0.4") && json.Contains("\"CanBePushed\": false")
                && !json.Contains("SightRange") && tank != null && tank.MaxHitPoints == 120
                && tank.SpawnChancePercent == 50,
                "saving writes only the set values and reloads the field's kinds");

            editor.SetFieldValue("Lebenspunkte", "0");
            Assert(editor.PreviewText.Contains("Probleme") && !editor.SaveType()
                && errors.Count == 1 && errors[0].Contains("Lebenspunkte"),
                "values out of range are shown and block saving");
            editor.SetFieldValue("Lebenspunkte", "1,2");
            Assert(!editor.SaveType() && errors[1].Contains("keine ganze Zahl"),
                "unreadable numbers block saving");
            editor.SetFieldValue("Lebenspunkte", "120");

            editor.SpawnTestEnemy();
            Assert(errors.Count == 3 && errors[2].Contains("laufenden Run"),
                "the test spawn needs a running game");
            field.StartNewGame();
            field.ReturnToStartMenu();
            int enemiesBefore = field.Enemies.Count;
            editor.SetFieldValue("Größe", "60");
            editor.SpawnTestEnemy();
            NonPlayerCharacter spawned = field.Enemies.LastOrDefault();
            double distance = spawned == null ? double.MaxValue : Math.Sqrt(
                Math.Pow(spawned.PositionX - field.Player.PositionX, 2)
                + Math.Pow(spawned.PositionY - field.Player.PositionY, 2));
            Assert(field.Enemies.Count == enemiesBefore + 1 && spawned.TypeId == "Tank"
                && spawned.Size == 60 && distance < 200 && !field.IsGameStarted,
                "the test spawn places the current (unsaved) version beside the paused player");
            field.RemoveAllEnemies();
            Assert(field.Enemies.Count == 0, "all enemies can be removed for testing");

            FindMenuButtonIn(editor, "Duplizieren").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            Assert(editor.GetFieldValue("Id (Dateiname)") == "Tank_Kopie" && editor.SaveType(),
                "Duplicate copies the form under a new id");
            FindMenuButtonIn(editor, "Löschen").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            editor.SelectType("Tank");
            FindMenuButtonIn(editor, "Löschen").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert(EnemyTypeRepository.GetTypeIds().SequenceEqual(new[] { EnemyType.StandardId })
                && field.EnemyTypes.All(type => type.Id == EnemyType.StandardId)
                && editor.LoadedId == EnemyType.StandardId,
                "Delete removes the kind from the files and the field");
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    // A field without start enemies, so tests place exactly the enemies they need.
    private static ArenaField CreateEmptyEnemyField(params EnemyType[] types)
    {
        ArenaField field = new ArenaField();
        field.SetEnemyTypes(new[]
        {
            EnemyType.Resolve(new EnemyTypeDefinition
            {
                Id = EnemyType.StandardId, StartCount = 0, Enabled = false
            })
        }.Concat(types));
        field.StartNewGame();
        field.Player.IsInvulnerable = true;
        return field;
    }

    private static NonPlayerCharacter PlaceEnemy(
        ArenaField field, EnemyType type, double offsetX, double offsetY)
    {
        Assert(field.SpawnEnemy(type), "the test enemy " + type.Id + " spawns");
        NonPlayerCharacter enemy = field.Enemies.Last();
        enemy.SetPosition(field.Player.PositionX + offsetX, field.Player.PositionY + offsetY);
        return enemy;
    }

    private static void TestEnemyAttackResolution()
    {
        EnemyType standard = EnemyType.Standard;
        Assert(standard.Attacks.Count == 1
            && standard.Attacks[0].Weapon == WeaponCatalog.EnemyClaws
            && standard.Behavior == EnemyBehavior.Chaser
            && standard.SpriteSheet == EnemyType.DefaultSpriteSheet && !standard.Tint.HasValue
            && standard.DamageTakenMultiplier == 1 && standard.KnockbackResistance == 0
            && standard.StatusImmunities.Count == 0 && standard.ScoreValue == 1,
            "the standard enemy keeps the claws, chases and has no resistances");

        EnemyType archer = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Archer",
            Attacks = new List<EnemyAttackDefinition>
            {
                new EnemyAttackDefinition
                {
                    BaseWeapon = "bow", Damage = 5, PoisonDamagePerSecond = 2, LifeStealPercent = 50
                },
                new EnemyAttackDefinition
                {
                    Pattern = "Cone", Range = 80, AngleDegrees = 120, CooldownSeconds = 2
                }
            }
        });
        Weapon bow = archer.Attacks[0].Weapon;
        Weapon cone = archer.Attacks[1].Weapon;
        Assert(bow.Id == "enemy-Archer-1" && bow.AttackPattern is ProjectileAttackPattern
            && bow.AttackPattern.Range == WeaponCatalog.Bow.AttackPattern.Range
            && bow.Effect is LifeStealEffect && bow.Effect.Description.Contains("5 damage")
            && bow.Effect.Description.Contains("damage/s"),
            "a finished weapon keeps its pattern; damage and effects are overridden and added");
        Assert(cone.AttackPattern is MeleeConeAttackPattern coneShape
            && coneShape.Range == 80
            && Math.Abs(coneShape.DebugHalfAngleRadians - Math.PI / 3) < 1e-9
            && cone.CooldownSeconds == 2
            && archer.Attacks[1].UseUpToDistance == 80,
            "a pattern built from the toolbox uses the given range, angle and cooldown");

        List<string> problems = new List<string>();
        EnemyType broken = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Broken",
            BehaviorType = "Flieger",
            Tint = "rot",
            StatusImmunities = new List<string> { "poison", "lava" },
            Attacks = new List<EnemyAttackDefinition>
            {
                new EnemyAttackDefinition { BaseWeapon = "laser", Pattern = "Strahl", AngleDegrees = 400 }
            }
        }, problems);
        Assert(problems.Count == 6 && broken.Behavior == EnemyBehavior.Chaser
            && !broken.Tint.HasValue && broken.StatusImmunities.SequenceEqual(new[] { "poison" })
            && broken.Attacks[0].Weapon.AttackPattern is MeleeConeAttackPattern,
            "unknown behaviour, tint, immunity, weapon, pattern and angle are reported ("
                + problems.Count + ")");
        Assert(problems.Any(problem => problem.StartsWith("Broken Angriff 1: Waffe")),
            "attack problems name the attack");
    }

    private static void TestEnemyResistancesAndRegeneration()
    {
        EnemyType tank = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Tank",
            MaxHitPoints = 100,
            DamageTakenMultiplier = 0.5,
            KnockbackResistance = 1,
            StatusImmunities = new List<string> { "poison", "stun" },
            RegenerationPerSecond = 10
        });
        PursuerEnemy enemy = new PursuerEnemy(540, 500, PlayerCharacter.BaseMaxSpeed, tank);
        enemy.TakeDamage(10);
        Assert(enemy.CurrentHitPoints == 95, "armour halves the damage taken");
        new PoisonEffect(5, 3).Apply(null, enemy);
        new StunEffect(2).Apply(null, enemy);
        new SlowEffect(0.5, 2).Apply(null, enemy);
        Assert(enemy.ActiveDamageOverTimeEffects.Count == 0
            && enemy.ActiveStatusEffects.Count(effect => effect.Id == "stun") == 0
            && enemy.ActiveStatusEffects.Count(effect => effect.Id == "slow") == 1,
            "immunities ignore exactly the chosen statuses");

        TestEntity attacker = new TestEntity(500, 500, 30, 10, CombatTeam.Player)
        {
            EquippedWeapon = WeaponCatalog.EnemyClaws
        };
        ArenaCombatSystem combat = new ArenaCombatSystem();
        Assert(combat.TryStartAttack(attacker, 1, 0), "the test hit starts");
        combat.Update(ArenaField.GameTickDurationSeconds,
            new List<Entity> { attacker, enemy });
        Assert(enemy.PositionX == 540 && enemy.CurrentHitPoints == 90,
            "full knockback resistance keeps the enemy in place");

        enemy.UpdateStatusEffects(1.05);
        Assert(enemy.CurrentHitPoints == 100,
            "regeneration heals over time up to the maximum");
    }

    private static void TestEnemyKillRewards()
    {
        EnemyType boss = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Boss", StartCount = 0, ScoreValue = 3, WeaponPointsPerKill = 4,
            KillsTowardSkillPoint = 5
        });
        ArenaField field = CreateEmptyEnemyField(boss);
        int pointsBefore = field.Progression.UniversalPoints;
        int weaponPointsBefore = field.Progression.GetWeaponPoints(WeaponCatalog.Sword.Id);
        NonPlayerCharacter enemy = PlaceEnemy(field, boss, 50, 0);
        enemy.CurrentHitPoints = 1;
        field.AttackWithPlayer(1, 0);
        for (int i = 0; i < 20 && enemy.IsAlive; i++)
        {
            field.UpdateCombat(0.04);
        }
        Assert(!enemy.IsAlive && field.Score == 3
            && field.Progression.TotalKills == 5
            && field.Progression.UniversalPoints == pointsBefore + 1
            && field.Progression.GetWeaponPoints(WeaponCatalog.Sword.Id) == weaponPointsBefore + 4,
            "a kill rewards score, skill-point kills and weapon points of the kind");
    }

    private static void TestEnemyBehaviorTypes()
    {
        EnemyType turret = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Turret", StartCount = 0, BehaviorType = "Turret"
        });
        EnemyType ranged = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Ranged", StartCount = 0, BehaviorType = "Ranged", PreferredDistance = 150,
            SightRange = 400, SpeedFactor = 2
        });
        EnemyType coward = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Coward", StartCount = 0, BehaviorType = "Coward", FleeBelowHealthPercent = 50
        });

        ArenaField field = CreateEmptyEnemyField(turret);
        NonPlayerCharacter tower = PlaceEnemy(field, turret, 150, 0);
        double towerX = tower.PositionX;
        for (int i = 0; i < 25; i++)
        {
            field.UpdateEnemyPositions();
        }
        Assert(tower.PositionX == towerX && tower.CurrentTarget == field.Player,
            "a turret sees the player but does not move");

        field = CreateEmptyEnemyField(ranged);
        NonPlayerCharacter shooter = PlaceEnemy(field, ranged, 320, 0);
        for (int i = 0; i < 60; i++)
        {
            field.UpdateEnemyPositions();
        }
        double distance = shooter.PositionX - field.Player.PositionX;
        Assert(Math.Abs(distance - 150) <= 15,
            "a ranged enemy approaches to its preferred distance (" + distance + ")");
        shooter.SetPosition(field.Player.PositionX + 70, field.Player.PositionY);
        for (int i = 0; i < 60; i++)
        {
            field.UpdateEnemyPositions();
        }
        distance = shooter.PositionX - field.Player.PositionX;
        Assert(Math.Abs(distance - 150) <= 15,
            "a ranged enemy backs away to its preferred distance (" + distance + ")");

        field = CreateEmptyEnemyField(coward);
        PursuerEnemy runner = (PursuerEnemy)PlaceEnemy(field, coward, 120, 0);
        field.UpdateEnemyPositions();
        Assert(!runner.IsFleeing && runner.PositionX - field.Player.PositionX < 120,
            "a healthy coward attacks like a chaser");
        runner.CurrentHitPoints = 10;
        double before = runner.PositionX - field.Player.PositionX;
        field.UpdateEnemyPositions();
        Assert(runner.IsFleeing && runner.PositionX - field.Player.PositionX > before
            && !runner.TryAttackTargetInRange(field.Player, new ArenaCombatSystem()),
            "a hurt coward flees and does not attack");
    }

    private static void TestEnemyChoosesAttackByDistance()
    {
        EnemyType mixed = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Mixed", StartCount = 0, SightRange = 500,
            Attacks = new List<EnemyAttackDefinition>
            {
                new EnemyAttackDefinition { BaseWeapon = "enemy-claws" },
                new EnemyAttackDefinition { BaseWeapon = "bow", UseFromDistance = 100 }
            }
        });
        ArenaField field = CreateEmptyEnemyField(mixed);
        PursuerEnemy enemy = (PursuerEnemy)PlaceEnemy(field, mixed, 300, 0);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        Assert(enemy.TryAttackTargetInRange(field.Player, combat)
            && combat.GetCooldownRemaining(enemy, mixed.Attacks[1].Weapon) > 0
            && combat.GetCooldownRemaining(enemy, mixed.Attacks[0].Weapon) == 0,
            "far away the enemy shoots with the bow");
        Assert(!enemy.TryAttackTargetInRange(field.Player, combat),
            "a new attack waits until the running one has ended");

        combat = new ArenaCombatSystem();
        enemy.SetPosition(field.Player.PositionX + 40, field.Player.PositionY);
        Assert(enemy.TryAttackTargetInRange(field.Player, combat)
            && combat.GetCooldownRemaining(enemy, mixed.Attacks[0].Weapon) > 0
            && combat.GetCooldownRemaining(enemy, mixed.Attacks[1].Weapon) == 0,
            "close by the enemy uses its claws");
    }

    private static void TestEnemySpriteSheetsAndTint()
    {
        Rect? standardHitbox = EntityHitboxCatalog.GetHitboxOffset(EntityHitboxCatalog.Enemy, 68);
        Assert(standardHitbox.HasValue
            && EntityHitboxCatalog.GetEnemyHitboxOffset("Enemie1.png", 68) == standardHitbox
            && !EntityHitboxCatalog.GetEnemyHitboxOffset("Fehlt.png", 68).HasValue
            && PursuerEnemy.GetHitboxOffsetForSize(34, "Fehlt.png") == new Rect(-17, -17, 34, 34),
            "hitboxes are measured per sprite sheet; missing sheets use a square");

        string previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = FindGameProjectDirectory();
            EnemyType red = EnemyType.Resolve(new EnemyTypeDefinition
            {
                Id = "Red", StartCount = 0, Tint = "#FF0000", Opacity = 0.8,
                SpriteSheet = "Fehlt.png"
            });
            ArenaField field = CreateEmptyEnemyField(red);
            PlaceEnemy(field, red, 120, 0);
            PainterG9 painter = new PainterG9();
            Canvas canvas = new Canvas { Width = 800, Height = 600 };
            canvas.Measure(new Size(800, 600));
            canvas.Arrange(new Rect(0, 0, 800, 600));
            painter.PaintGameField(canvas, field);
            List<Rectangle> rectangles = new List<Rectangle>();
            CollectRectangles(canvas, rectangles);
            Assert(rectangles.Any(rectangle => rectangle.Fill is SolidColorBrush brush
                    && brush.Color == Colors.Red && rectangle.OpacityMask is ImageBrush),
                "a tinted kind is drawn with a coloured copy of its sprite (missing sheet: standard)");
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    private static void CollectRectangles(DependencyObject parent, List<Rectangle> found)
    {
        if (parent is Rectangle rectangle)
        {
            found.Add(rectangle);
        }
        foreach (object child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is DependencyObject dependency)
            {
                CollectRectangles(dependency, found);
            }
        }
    }

    private static void TestEnemyEditorAttacks()
    {
        string previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = FindGameProjectDirectory();
            ArenaField field = new ArenaField();
            List<string> errors = new List<string>();
            ArenaEnemyEditorControl editor = new ArenaEnemyEditorControl(field)
            {
                Confirm = message => true,
                ShowError = errors.Add
            };
            FindMenuButtonIn(editor, "Neu").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            editor.SetFieldValue(ArenaEnemyEditorControl.IdLabel, "Schuetze");
            editor.SetFieldValue("Verhaltenstyp", "Ranged");
            editor.SetFieldValue("Immun gegen", "poison,stun");
            editor.SetFieldValue("Tönung (#RRGGBB)", "#00FF00");
            FindMenuButtonIn(editor, "+ Angriff").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            editor.SetAttackFieldValue(0, "Waffe", "bow");
            Assert(editor.AttackCount == 1
                && !editor.IsAttackFieldVisible(0, "Winkel (°)")
                && editor.IsAttackFieldVisible(0, "Projektiltempo"),
                "attack rows show the fields of the weapon's pattern");
            editor.SetAttackFieldValue(0, "Muster", "Cone");
            editor.SetAttackFieldValue(0, "Schaden", "7");
            editor.SetAttackFieldValue(0, "Frost (s)", "1,5");
            Assert(editor.IsAttackFieldVisible(0, "Winkel (°)")
                && !editor.IsAttackFieldVisible(0, "Projektiltempo")
                && editor.PreviewText.Contains("Fernkämpfer")
                && editor.PreviewText.Contains("Angriff 1: Bogen, Kegel"),
                "choosing another pattern switches the fields; the preview follows");
            FindMenuButtonIn(editor, "+ Angriff").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            editor.RemoveAttackAt(1);
            Assert(editor.AttackCount == 1, "attacks can be removed again");

            Assert(editor.SaveType() && errors.Count == 0, "the kind with an attack is saved");
            string json = File.ReadAllText(
                Path.Combine(EnemyTypeRepository.TypesDirectory, "Schuetze.json"));
            EnemyType saved = field.EnemyTypes.First(type => type.Id == "Schuetze");
            Assert(json.Contains("\"BaseWeapon\": \"bow\"") && json.Contains("\"FreezeSeconds\": 1.5")
                && saved.Behavior == EnemyBehavior.Ranged
                && saved.StatusImmunities.Count == 2 && saved.Tint == Colors.Lime
                && saved.Attacks.Count == 1
                && saved.Attacks[0].Weapon.AttackPattern is MeleeConeAttackPattern,
                "attacks, behaviour, immunities and tint are saved and used by the game");
            editor.SelectType(EnemyType.StandardId);
            editor.SelectType("Schuetze");
            Assert(editor.AttackCount == 1 && editor.GetFieldValue("Immun gegen") == "poison,stun",
                "loading a kind restores its attacks and immunities");

            editor.SetFieldValue("Spritesheet", "Fehlt.png");
            Assert(!editor.SaveType() && errors.Last().Contains("Fehlt.png"),
                "a missing sprite sheet blocks saving");
            EnemyTypeRepository.DeleteDefinition("Schuetze");
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    private static void TestMapRegionModelAndRepository()
    {
        ArenaMapRegion box = new ArenaMapRegion
        {
            Name = "Box", Shape = ArenaRegionShape.Rectangle, X = 100, Y = 50, Width = 200, Height = 100
        };
        ArenaMapRegion ring = new ArenaMapRegion
        {
            Name = "Ring", Shape = ArenaRegionShape.Circle, X = 500, Y = 500, Radius = 40
        };
        Assert(box.Contains(150, 60) && !box.Contains(310, 60)
            && ring.Contains(530, 520) && !ring.Contains(540, 540)
            && Math.Abs(ring.Area - Math.PI * 1600) < 1e-6
            && ring.Bounds == new Rect(460, 460, 80, 80),
            "rectangle and circle regions know their area and contain points");
        Random random = new Random(7);
        Assert(Enumerable.Range(0, 200).All(i =>
            {
                Point inBox = box.GetRandomPoint(random);
                Point inRing = ring.GetRandomPoint(random);
                return box.Contains(inBox.X, inBox.Y) && ring.Contains(inRing.X, inRing.Y);
            }),
            "random spawn points lie inside their shape");
        Assert(new ArenaMapRegion { Name = " ", Width = 5, Height = 5 }.Validate() != null
            && new ArenaMapRegion { Name = "A", Shape = ArenaRegionShape.Circle }.Validate() != null,
            "regions need a name and a size");

        const string map = "RegionTest.png";
        ArenaMapRegionRepository.Save(map, new[] { box, ring });
        IReadOnlyList<ArenaMapRegion> loaded = ArenaMapRegionRepository.Load(map);
        Assert(loaded.Count == 2 && loaded[1].Shape == ArenaRegionShape.Circle
            && loaded[1].Radius == 40 && loaded[0].Width == 200
            && File.ReadAllText(ArenaMapRegionRepository.GetPath(map)).Contains("\"Circle\"")
            && ArenaMapRegionRepository.GetAllRegionNames().SequenceEqual(new[] { "Box", "Ring" }),
            "regions are saved next to the map and loaded again");
        ArenaMapRegionRepository.Save(map, new ArenaMapRegion[0]);
        Assert(!File.Exists(ArenaMapRegionRepository.GetPath(map))
            && ArenaMapRegionRepository.Load(map).Count == 0,
            "saving no regions removes the file");
        File.WriteAllText(ArenaMapRegionRepository.GetPath(map), "[ { \"Name\": \"\" } ]");
        bool rejected = false;
        try
        {
            ArenaMapRegionRepository.Load(map);
        }
        catch (InvalidDataException)
        {
            rejected = true;
        }
        Assert(rejected, "invalid region files are rejected");
        File.Delete(ArenaMapRegionRepository.GetPath(map));
    }

    private static void TestEnemySpawnPositionFinder()
    {
        PlayerCharacter player = new PlayerCharacter(500, 400);
        Rect area = new Rect(32, 32, 936, 736);
        List<Rect> occupied = new List<Rect>();
        EnemySpawnPositionFinder finder = new EnemySpawnPositionFinder(
            () => area, () => player,
            hitbox => occupied.Any(other => AttackGeometry.RectsOverlap(hitbox, other)));
        Rect enemyHitbox = PursuerEnemy.GetHitboxOffsetForSize(34);
        Func<Point, Rect> place = point => new Rect(point.X + enemyHitbox.X,
            point.Y + enemyHitbox.Y, enemyHitbox.Width, enemyHitbox.Height);

        for (int i = 0; i < 20; i++)
        {
            Point? away = finder.FindAwayFromPlayer(34, EnemyType.DefaultSpriteSheet, 250);
            Assert(away.HasValue && area.Contains(place(away.Value))
                && (away.Value - new Point(500, 400)).Length >= 250
                && !occupied.Any(other => AttackGeometry.RectsOverlap(place(away.Value), other)),
                "positions away from the player keep the distance, stay inside and do not overlap");
            occupied.Add(place(away.Value));
        }

        ArenaMapRegion corner = new ArenaMapRegion
        {
            Name = "Ecke", Shape = ArenaRegionShape.Circle, X = 150, Y = 150, Radius = 80
        };
        Point? inRegion = finder.FindAwayFromPlayer(34, EnemyType.DefaultSpriteSheet, 0,
            new[] { corner });
        Assert(inRegion.HasValue && corner.Contains(inRegion.Value.X, inRegion.Value.Y),
            "with regions the position lies inside one of them");

        Point? beside = finder.FindBesidePlayer(34, EnemyType.DefaultSpriteSheet);
        Assert(beside.HasValue && (beside.Value - new Point(500, 400)).Length < 150
            && !AttackGeometry.RectsOverlap(place(beside.Value), player.Hitbox),
            "beside the player the enemy stands close without touching the player");

        Assert(!finder.FindAwayFromPlayer(34, EnemyType.DefaultSpriteSheet, 5000).HasValue,
            "no position is found when the distance cannot be met (the spawn is skipped)");
    }

    private static void TestEnemiesSpawnInRegions()
    {
        List<string> problems = new List<string>();
        EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "NoRegion", PlayerCondition = "PlayerInsideRegion"
        }, problems);
        EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Odd", Regions = new List<string> { "Nord" }, PlayerCondition = "Manchmal"
        }, problems);
        Assert(problems.Count == 2,
            "a player condition needs regions and must be known");

        EnemyType north = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "North", StartCount = 0, MinDistanceToPlayer = 0,
            Regions = new List<string> { "Nord", "Gibtsnicht" }
        });
        EnemyType inside = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Inside", StartCount = 0, SpawnIntervalSeconds = 1, MinDistanceToPlayer = 0,
            Regions = new List<string> { "Nord" }, PlayerCondition = "PlayerInsideRegion"
        });
        EnemyType outside = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Outside", Regions = new List<string> { "Nord" },
            PlayerCondition = "PlayerOutsideRegion"
        });
        EnemyType missing = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Missing", StartCount = 0, Regions = new List<string> { "Gibtsnicht" },
            PlayerCondition = "PlayerInsideRegion"
        });

        ArenaField field = CreateEmptyEnemyField(north, inside, missing);
        ArenaMapRegion northRegion = new ArenaMapRegion
        {
            Name = "Nord", Shape = ArenaRegionShape.Rectangle, X = 100, Y = 100,
            Width = 300, Height = 200
        };
        ArenaMapRegionRepository.Save(field.MapFileName, new[] { northRegion });
        field.ReloadMapRegions();
        try
        {
            Assert(field.MapRegions.Count == 1 && field.GetSpawnRegions(north).Count == 1
                && field.GetSpawnRegions(missing).Count == 0,
                "the field loads its map's regions; unknown names are ignored");
            for (int i = 0; i < 5; i++)
            {
                Assert(field.SpawnEnemy(north), "an enemy spawns in its region");
            }
            Assert(field.Enemies.All(enemy => northRegion.Contains(enemy.PositionX, enemy.PositionY)),
                "enemies of a kind with regions spawn inside them");

            Assert(!field.IsPlayerConditionMet(inside) && field.IsPlayerConditionMet(outside)
                && field.IsPlayerConditionMet(missing),
                "outside the region only the outside condition holds; missing regions are ignored");
            field.RemoveAllEnemies();
            int ticksPerSecond = (int)Math.Round(1 / ArenaField.GameTickDurationSeconds);
            for (int tick = 0; tick < 2 * ticksPerSecond; tick++)
            {
                field.UpdateEnemyPositions();
            }
            Assert(field.Enemies.Count(enemy => enemy.TypeId == "Inside") == 0,
                "a kind waiting for the player in its region does not spawn elsewhere");
            field.Player.SetPosition(250, 200);
            Assert(field.IsPlayerConditionMet(inside) && !field.IsPlayerConditionMet(outside),
                "inside the region the inside condition holds");
            for (int tick = 0; tick < 2 * ticksPerSecond; tick++)
            {
                field.UpdateEnemyPositions();
            }
            Assert(field.Enemies.Any(enemy => enemy.TypeId == "Inside"),
                "the kind spawns once the player entered its region");

            string previousDirectory = Environment.CurrentDirectory;
            try
            {
                Environment.CurrentDirectory = FindGameProjectDirectory();
                field.ToggleDamageRanges();
                PainterG9 painter = new PainterG9();
                Canvas canvas = new Canvas { Width = 800, Height = 600 };
                canvas.Measure(new Size(800, 600));
                canvas.Arrange(new Rect(0, 0, 800, 600));
                painter.PaintGameField(canvas, field);
                Assert(ContainsText(canvas, "Nord"), "F4 + B shows the spawn regions");
            }
            finally
            {
                Environment.CurrentDirectory = previousDirectory;
            }
        }
        finally
        {
            ArenaMapRegionRepository.Save(field.MapFileName, new ArenaMapRegion[0]);
        }
    }

    private static bool ContainsText(DependencyObject parent, string text)
    {
        if (parent is TextBlock block && block.Text == text)
        {
            return true;
        }
        foreach (object child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is DependencyObject dependency && ContainsText(dependency, text))
            {
                return true;
            }
        }
        return false;
    }

    private static void TestRegionEditor()
    {
        string previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = FindGameProjectDirectory();
            ArenaField field = new ArenaField();
            int savedCalls = 0;
            List<string> errors = new List<string>();
            ArenaRegionEditorControl editor = new ArenaRegionEditorControl(field, () => savedCalls++)
            {
                Confirm = message => true,
                ShowError = errors.Add
            };
            Assert(editor.MapFileName == field.MapFileName && editor.Regions.Count == 0,
                "the regions tab opens with the played map");

            editor.PointerDown(new Point(50, 50));
            editor.PointerMove(new Point(150, 100));
            editor.PointerUp(new Point(250, 150));
            Assert(editor.Regions.Count == 1 && editor.Regions[0].Name == "Region1"
                && editor.Regions[0].Width == 200 && editor.Regions[0].Height == 100
                && editor.SelectedIndex == 0 && editor.IsDirty,
                "dragging on free ground draws a rectangle");

            editor.DrawCircles = true;
            editor.NewShapeName = "Lichtung";
            editor.PointerDown(new Point(600, 600));
            editor.PointerUp(new Point(650, 600));
            Assert(editor.Regions.Count == 2 && editor.Regions[1].Shape == ArenaRegionShape.Circle
                && Math.Abs(editor.Regions[1].Radius - 50) < 1e-9
                && editor.Regions[1].Name == "Lichtung",
                "with the circle tool a drag draws a circle with the chosen name");

            editor.PointerDown(new Point(700, 700));
            editor.PointerUp(new Point(702, 701));
            Assert(editor.Regions.Count == 2, "a click without dragging adds no shape");

            editor.PointerDown(new Point(100, 100));
            editor.PointerUp(new Point(200, 120));
            Assert(editor.Regions[0].X == 150 && editor.Regions[0].Y == 70,
                "dragging a shape moves it");
            editor.NewShapeName = "Nordlager";
            editor.RenameSelected();
            Assert(editor.Regions[0].Name == "Nordlager", "the selected shape can be renamed");

            Assert(editor.Save() && savedCalls == 1 && !editor.IsDirty
                && field.MapRegions.Count == 2
                && ArenaMapRegionRepository.GetAllRegionNames().Contains("Lichtung"),
                "saving writes the regions and the played map uses them at once");

            ArenaEnemyEditorControl enemyEditor = new ArenaEnemyEditorControl(field)
            {
                Confirm = message => true,
                ShowError = errors.Add
            };
            enemyEditor.SetFieldValue(ArenaEnemyEditorControl.RegionsLabel, "Lichtung,Verloren");
            Assert(enemyEditor.GetFieldValue(ArenaEnemyEditorControl.RegionsLabel)
                    == "Lichtung,Verloren",
                "the enemy editor offers saved regions and keeps unknown names");
            editor.DeleteSelected();
            editor.Save();
            enemyEditor.RefreshRegionNames();
            Assert(enemyEditor.GetFieldValue(ArenaEnemyEditorControl.RegionsLabel)
                    == "Lichtung,Verloren" && errors.Count == 0,
                "refreshing the region names keeps the choice");

            ArenaMapRegionRepository.Save(field.MapFileName, new ArenaMapRegion[0]);
            field.ReloadMapRegions();
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    private static byte[] GetPixels(BitmapSource image)
    {
        FormatConvertedBitmap converted = new FormatConvertedBitmap(
            image, PixelFormats.Bgra32, null, 0);
        byte[] pixels = new byte[converted.PixelWidth * converted.PixelHeight * 4];
        converted.CopyPixels(pixels, converted.PixelWidth * 4, 0);
        return pixels;
    }

    private static bool IsMirrorOf(BitmapSource mirrored, BitmapSource original)
    {
        byte[] a = GetPixels(mirrored);
        byte[] b = GetPixels(original);
        int width = original.PixelWidth;
        int height = original.PixelHeight;
        if (mirrored.PixelWidth != width || mirrored.PixelHeight != height)
        {
            return false;
        }
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                for (int c = 0; c < 4; c++)
                {
                    if (a[(y * width + x) * 4 + c] != b[(y * width + (width - 1 - x)) * 4 + c])
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }

    private static void TestAnimatorMirrorsMissingSides()
    {
        string previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = FindGameProjectDirectory();
            Assert(SpriteSheetAnimator.GetMirroredClipName("Walk Attack Right Down")
                    == "Walk Attack Left Down"
                && SpriteSheetAnimator.GetMirroredClipName("idleLeft") == "idleRight"
                && SpriteSheetAnimator.GetMirroredClipName("Idle Front") == null,
                "mirrored clip names swap Left and Right");

            SpriteSheetAnimator frog = new SpriteSheetAnimator(
                PainterG9.FindAssetPath("Enemies", "Frog.png"),
                PainterG9.FindAssetPath("Enemies", "Frog.json"));
            Assert(frog.HasClip("Attack Right Down") && frog.HasClip("Walk Attack Right Down")
                && frog.GetFrameCount("Walk Attack Right Down") == 2
                && IsMirrorOf(frog.GetClipFrame("Attack Right Down", 0),
                    frog.GetClipFrame("Attack Left Down", 0))
                && IsMirrorOf(frog.GetFrame("Walk Attack Right Down"),
                    frog.GetClipFrame("Walk Attack Left Down", 0)),
                "a missing right-side clip is the mirrored left-side clip");
            Assert(!frog.HasClip("Attack Diagonal")
                && !IsMirrorOf(frog.GetClipFrame("Idle Right", 0),
                    frog.GetClipFrame("Idle Left", 0)),
                "existing clips are used as they are, unknown ones are not invented");

            byte[] notFound = GetPixels(frog.GetClipFrame("NotFound", 0));
            Assert(frog.GetFrameCount("Walk Attack Left Up") == 2
                && !GetPixels(frog.GetClipFrame("Walk Attack Left Up", 1)).SequenceEqual(notFound)
                && frog.HasClip("Idle Front") && frog.HasClip("Walk Back"),
                "the corrected frog sheet has its walk frames in order and readable names");

            SpriteSheetAnimator enemy = new SpriteSheetAnimator(
                PainterG9.FindAssetPath("Enemies", "Enemie1.png"),
                PainterG9.FindAssetPath("Enemies", "Enemie1.json"));
            Assert(IsMirrorOf(enemy.GetClipFrame("idleRight", 0),
                    enemy.GetClipFrame("idleLeft", 0)),
                "the standard enemy gets its missing right idle clip by mirroring");
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    private static void TestFrogAttackAnimationNames()
    {
        Assert(PainterG9.GetAttackAnimationName(PlayerFacing.Right, false, 1, 1)
                == "Attack Right Down"
            && PainterG9.GetAttackAnimationName(PlayerFacing.Left, false, -1, -1)
                == "Attack Left Up"
            && PainterG9.GetAttackAnimationName(PlayerFacing.Front, false, -1, 1)
                == "Attack Front Left"
            && PainterG9.GetAttackAnimationName(PlayerFacing.Back, true, 1, -1)
                == "Walk Attack Back Right"
            && PainterG9.GetAttackAnimationName(PlayerFacing.Front, false, 0, 1) == "Attack Front"
            && PainterG9.GetAttackAnimationName(PlayerFacing.Left, true, -1, 0)
                == "Walk Attack Left",
            "the tongue clip follows the attack direction and the side the frog faces");

        List<string> problems = new List<string>();
        EnemyType selfPulse = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "SelfPulse",
            Attacks = new List<EnemyAttackDefinition> { new EnemyAttackDefinition { Pattern = "Pulse" } }
        }, problems);
        Assert(problems.Count == 1 && !EnemyAttack.Patterns.Contains(EnemyAttack.PatternPulse)
            && selfPulse.Attacks[0].Weapon.AttackPattern is MeleeConeAttackPattern,
            "enemy attacks do not offer the self pulse (it would only hit the enemy itself)");
    }

    private static NonPlayerCharacter GetFrog(ArenaField field)
    {
        return field.Enemies.FirstOrDefault(enemy => enemy.TypeId == "Frog" && enemy.IsAlive);
    }

    private static void RunArena(ArenaField field, double seconds)
    {
        int ticks = (int)Math.Round(seconds / ArenaField.GameTickDurationSeconds);
        for (int tick = 0; tick < ticks; tick++)
        {
            field.UpdateEnemyPositions();
            field.UpdateCombat(ArenaField.GameTickDurationSeconds);
        }
    }

    private static void TestFrogDevoursEntities()
    {
        EnemyType victimType = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Victim", StartCount = 0, BehaviorType = "Turret", MaxHitPoints = 500,
            DamageTakenMultiplier = 0.1
        });
        ArenaField field = CreateEmptyEnemyField(_shippedFrog, victimType);
        NonPlayerCharacter frog = GetFrog(field);
        Assert(frog != null && field.Enemies.Count == 1,
            "a run starts with exactly one frog");
        frog.SetPosition(field.Player.PositionX + 400, field.Player.PositionY);
        NonPlayerCharacter victim = PlaceEnemy(field, victimType, 450, 0);
        int scoreBefore = field.Score;
        RunArena(field, 0.2);
        Assert(frog.IsDevouring && victim.IsAlive
            && Math.Abs(victim.PositionX - frog.PositionX) < 50,
            "an enemy in the tongue radius is held and pulled towards the frog");
        RunArena(field, 1);
        Assert(!victim.IsAlive && !field.Enemies.Contains(victim) && field.Score == scoreBefore
            && !frog.IsDevouring,
            "the pulled-in enemy dies at once (despite armour), leaves the arena, no kill");

        field.Player.IsInvulnerable = true;
        frog.SetPosition(field.Player.PositionX + 50, field.Player.PositionY);
        RunArena(field, 1.5);
        Assert(field.Player.IsAlive && !frog.IsDevouring,
            "godmode (or the respawn protection) keeps the player from being eaten");

        field.Player.IsInvulnerable = false;
        int deathsBefore = field.Deaths;
        frog.SetPosition(field.Player.PositionX + 50, field.Player.PositionY);
        RunArena(field, 1.5);
        Assert(!field.Player.IsAlive && field.IsGameOver && field.Deaths == deathsBefore + 1,
            "the frog also eats the player, which ends the life like any death");
    }

    private static void TestFrogHopsAndRespawnsAlone()
    {
        EnemyType bystanderType = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Bystander", StartCount = 0, BehaviorType = "Turret"
        });
        ArenaField field = CreateEmptyEnemyField(_shippedFrog, bystanderType);
        NonPlayerCharacter frog = GetFrog(field);
        frog.SetPosition(field.Player.PositionX + 400, field.Player.PositionY + 200);
        int stillTicks = 0;
        int movingTicks = 0;
        Entity firstTarget = null;
        double startDistance = 0;
        for (int tick = 0; tick < 75; tick++)
        {
            double x = frog.PositionX;
            double y = frog.PositionY;
            field.UpdateEnemyPositions();
            if (firstTarget == null)
            {
                firstTarget = frog.CurrentTarget;
                startDistance = Math.Sqrt(Math.Pow(firstTarget.PositionX - x, 2)
                    + Math.Pow(firstTarget.PositionY - y, 2));
            }
            if (frog.PositionX == x && frog.PositionY == y)
            {
                stillTicks++;
            }
            else
            {
                movingTicks++;
            }
        }
        double endDistance = Math.Sqrt(Math.Pow(firstTarget.PositionX - frog.PositionX, 2)
            + Math.Pow(firstTarget.PositionY - frog.PositionY, 2));
        Assert(firstTarget == field.Player && stillTicks > movingTicks && movingTicks > 0
            && endDistance < startDistance,
            "the frog hops (mostly pausing) towards a random entity, here the only one: the player");

        PlaceEnemy(field, bystanderType, -400, -200);
        HashSet<Entity> targets = new HashSet<Entity>();
        for (int i = 0; i < 40; i++)
        {
            ((PursuerEnemy)frog).GetPlannedDestination(field);
            targets.Add(frog.CurrentTarget);
            ((PursuerEnemy)frog).ForceRetarget();
        }
        Assert(targets.Contains(field.Player) && targets.Count == 2,
            "random targets include other enemies, not the frog itself");

        frog.TakeDamage(1);
        Assert(!frog.IsAlive, "a single hit kills the frog");
        field.RemoveAllEnemies();
        Assert(GetFrog(field) == null, "the dead frog is gone");
        RunArena(field, _shippedFrog.SpawnIntervalSeconds + 0.1);
        Assert(field.Enemies.Count(enemy => enemy.TypeId == "Frog") == 1,
            "the frog respawns at its next spawn attempt (at most one interval later)");
        field.Player.IsInvulnerable = true;
        RunArena(field, 20);
        Assert(field.Enemies.Count(enemy => enemy.TypeId == "Frog" && enemy.IsAlive) <= 1,
            "there is never more than one frog");

        string previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = FindGameProjectDirectory();
            PainterG9 painter = new PainterG9();
            Canvas canvas = new Canvas { Width = 800, Height = 600 };
            canvas.Measure(new Size(800, 600));
            canvas.Arrange(new Rect(0, 0, 800, 600));
            painter.PaintGameField(canvas, field);
            Assert(canvas.Children.Count > 0, "the frog is drawn with its own sprite sheet");
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    private static void TestFrogCroaks()
    {
        Assert(_shippedFrog.IdleSound == "FrogCroak" && _shippedFrog.IdleSoundMinSeconds == 2
            && _shippedFrog.IdleSoundMaxSeconds == 6
            && _shippedFrog.Attacks[0].Sound == "FrogCroak"
            && ArenaAudio.EnemySoundNames.Contains("FrogCroak")
            && ArenaAudio.SoundFilePatterns.Contains("frogCroak_*.wav"),
            "the frog croaks now and then and when its tongue shoots out");

        ArenaField field = CreateEmptyEnemyField(_shippedFrog);
        NonPlayerCharacter frog = GetFrog(field);
        frog.SetPosition(field.Player.PositionX + 600, field.Player.PositionY);
        List<string> sounds = new List<string>();
        field.EnemySoundRequested += (enemy, sound) => sounds.Add(sound);
        for (int tick = 0; tick < 12 / ArenaField.GameTickDurationSeconds; tick++)
        {
            // Keep it far from the player so only idle croaks count.
            frog.SetPosition(field.Player.PositionX + 600, field.Player.PositionY);
            field.UpdateEnemyPositions();
        }
        Assert(sounds.All(sound => sound == "FrogCroak") && sounds.Count >= 2 && sounds.Count <= 6,
            "idle croaks come every 2-6 s of game time (" + sounds.Count + " in 12 s)");
    }

    private static void TestDevourRingAndHiddenHealthBar()
    {
        ArenaHudRenderer hud = new ArenaHudRenderer();
        TestEntity held = new TestEntity(100, 100, 30, 10, CombatTeam.Enemies);
        held.ApplyMovementStatus("devoured", "Verschlungen", 0, 1);
        Canvas heldCanvas = new Canvas();
        hud.PaintStatusPlaceholder(heldCanvas, held, 100, 100);
        TestEntity stunned = new TestEntity(100, 100, 30, 10, CombatTeam.Enemies);
        stunned.ApplyMovementStatus("stun", "Stun", 0, 1);
        Canvas stunnedCanvas = new Canvas();
        hud.PaintStatusPlaceholder(stunnedCanvas, stunned, 100, 100);
        Assert(heldCanvas.Children.Count == 0 && stunnedCanvas.Children.Count > 0,
            "being devoured shows no white ring (a stun still does)");

        Assert(!_shippedTurtle.ShowHealthBar && EnemyType.Standard.ShowHealthBar
            && _shippedFrog.ShowHealthBar,
            "the mine turtle hides its health bar, other kinds show theirs");
        string previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = FindGameProjectDirectory();
            ArenaField field = CreateEmptyEnemyField(_shippedTurtle);
            field.RemoveAllEnemies();
            PlaceEnemy(field, _shippedTurtle, 150, 0);
            PainterG9 painter = new PainterG9();
            Canvas canvas = new Canvas { Width = 800, Height = 600 };
            canvas.Measure(new Size(800, 600));
            canvas.Arrange(new Rect(0, 0, 800, 600));
            painter.PaintGameField(canvas, field);
            int withoutBar = CountBorders(canvas);
            EnemyType visibleTurtle = EnemyType.Resolve(new EnemyTypeDefinition
            {
                Id = "VisibleTurtle", StartCount = 0, SpriteSheet = "Mine Turtel.png"
            });
            field.SetEnemyTypes(new[] { visibleTurtle });
            field.RemoveAllEnemies();
            PlaceEnemy(field, visibleTurtle, 150, 0);
            painter.PaintGameField(canvas, field);
            Assert(CountBorders(canvas) == withoutBar + 1,
                "a kind with ShowHealthBar false is drawn without its health bar");
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    private static int CountBorders(DependencyObject parent)
    {
        int count = parent is Border ? 1 : 0;
        foreach (object child in LogicalTreeHelper.GetChildren(parent))
        {
            if (child is DependencyObject dependency)
            {
                count += CountBorders(dependency);
            }
        }
        return count;
    }

    private static void TestMineTurtleExplodesWhenSteppedOn()
    {
        EnemyType victimType = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Victim", StartCount = 0, BehaviorType = "Turret", MaxHitPoints = 500,
            DamageTakenMultiplier = 0.1
        });
        ArenaField field = CreateEmptyEnemyField(_shippedTurtle, victimType);
        Assert(field.Enemies.Count(enemy => enemy.TypeId == "MineTurtle") == 2,
            "a run starts with the mine turtles of its start count");
        field.RemoveAllEnemies();
        List<string> sounds = new List<string>();
        field.EnemySoundRequested += (enemy, sound) => sounds.Add(sound);
        PursuerEnemy turtle = (PursuerEnemy)PlaceEnemy(field, _shippedTurtle, 300, 0);
        NonPlayerCharacter neighbour = PlaceEnemy(field, victimType, 300, 120);
        NonPlayerCharacter farAway = PlaceEnemy(field, victimType, -400, 0);
        double turtleX = turtle.PositionX;
        double turtleY = turtle.PositionY;

        RunArena(field, 1);
        Assert(turtle.PositionX == turtleX && turtle.PositionY == turtleY
            && !turtle.IsWindingUp && turtle.IsAlive,
            "the mine turtle does not move and waits");
        neighbour.SetPosition(turtleX, turtleY);
        RunArena(field, 0.5);
        Assert(!turtle.IsWindingUp, "other enemies on the turtle do not set it off");
        neighbour.SetPosition(turtleX, turtleY + 120);

        // The player stays protected here so the game keeps running during the blast.
        int scoreBefore = field.Score;
        field.Player.SetPosition(turtleX + 5, turtleY);
        RunArena(field, 0.1);
        Assert(turtle.IsWindingUp && turtle.CurrentWindupClip == "Aktive" && sounds.Count == 0,
            "stepping on the turtle presses its button");
        RunArena(field, 0.4);
        Assert(turtle.CurrentWindupClip == "Hello" && sounds.SequenceEqual(new[] { "MineTurtleHello" })
            && neighbour.IsAlive,
            "then it says hello");
        RunArena(field, 1);
        Assert(sounds.SequenceEqual(new[] { "MineTurtleHello", "PowerBlast" })
            && !turtle.IsAlive && !field.Enemies.Contains(turtle)
            && !neighbour.IsAlive && !field.Enemies.Contains(neighbour)
            && farAway.IsAlive && field.Player.IsAlive && field.Score == scoreBefore,
            "and explodes: everything in the radius dies (also the turtle), nothing outside; "
                + "godmode protects the player");

        field = CreateEmptyEnemyField(_shippedTurtle);
        field.RemoveAllEnemies();
        turtle = (PursuerEnemy)PlaceEnemy(field, _shippedTurtle, 300, 0);
        field.Player.IsInvulnerable = false;
        int deathsBefore = field.Deaths;
        field.Player.SetPosition(turtle.PositionX, turtle.PositionY);
        RunArena(field, 1.6);
        Assert(!field.Player.IsAlive && field.Deaths == deathsBefore + 1,
            "an unprotected player on the mine dies in the explosion");

        string previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = FindGameProjectDirectory();
            field = CreateEmptyEnemyField(_shippedTurtle);
            PainterG9 painter = new PainterG9();
            Canvas canvas = new Canvas { Width = 800, Height = 600 };
            canvas.Measure(new Size(800, 600));
            canvas.Arrange(new Rect(0, 0, 800, 600));
            painter.PaintGameField(canvas, field);
            Assert(canvas.Children.Count > 0,
                "a sheet with a single Idle clip and no NotFound clip is drawn");
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
    }

    private static void TestWindupTextFormat()
    {
        List<EnemyWindupStepDefinition> steps =
            ArenaEnemyEditorControl.ParseWindup("Aktive:0,3; Hello:0,7:MineTurtleHello");
        Assert(steps.Count == 2 && steps[0].Clip == "Aktive" && steps[0].Seconds == 0.3
            && steps[1].Sound == "MineTurtleHello"
            && ArenaEnemyEditorControl.FormatWindup(steps)
                == "Aktive:0,3; Hello:0,7:MineTurtleHello"
            && ArenaEnemyEditorControl.ParseWindup("  ") == null,
            "the windup text field reads and writes clip:seconds:sound steps");

        List<string> problems = new List<string>();
        EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "BadWindup",
            Attacks = new List<EnemyAttackDefinition>
            {
                new EnemyAttackDefinition
                {
                    Windup = ArenaEnemyEditorControl.ParseWindup("Knopf:abc; :1"),
                    Sound = "Trompete",
                    Trigger = "Zufall"
                }
            }
        }, problems);
        Assert(problems.Count == 4 && ArenaAudio.EnemySoundNames.Contains("MineTurtleHello"),
            "unreadable windup steps, unknown sounds and triggers are reported (" + problems.Count + ")");
    }

    private static void TestEnemyKindsSpawnOnOwnTimers()
    {
        EnemyType fast = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Fast",
            StartCount = 0,
            SpawnIntervalSeconds = 2,
            MaxAlive = 2,
            MaxHitPoints = 50,
            SpeedFactor = 1,
            Size = 50,
            SightRange = 100
        });
        EnemyType late = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Late",
            StartCount = 0,
            SpawnIntervalSeconds = 1,
            MaxAlive = 1,
            MinRunSeconds = 5
        });
        EnemyType hunter = EnemyType.Resolve(new EnemyTypeDefinition
        {
            Id = "Hunter",
            StartCount = 3,
            MinKills = 1
        });
        ArenaField field = new ArenaField();
        field.SetEnemyTypes(new[] { EnemyType.Standard, fast, late, hunter });
        field.StartNewGame();
        field.Player.IsInvulnerable = true;
        Func<string, int> count = id => field.Enemies.Count(enemy => enemy.TypeId == id);
        Assert(count(EnemyType.StandardId) == 1 && field.Enemies.Count == 1,
            "a run starts with each kind's start count; kinds waiting for kills stay away");

        int ticksPerSecond = (int)Math.Round(1 / ArenaField.GameTickDurationSeconds);
        for (int tick = 0; tick < 2 * ticksPerSecond; tick++)
        {
            field.UpdateEnemyPositions();
        }
        NonPlayerCharacter fastEnemy = field.Enemies.FirstOrDefault(enemy => enemy.TypeId == "Fast");
        Assert(count("Fast") == 1 && count(EnemyType.StandardId) == 1 && count("Late") == 0,
            "each kind spawns on its own timer");
        Assert(fastEnemy.MaxHitPoints == 50 && fastEnemy.Size == 50
            && Math.Abs(fastEnemy.MaxSpeed - field.Player.MaxSpeed) < 1e-9
            && fastEnemy.SightRange == 100,
            "a spawned enemy takes its values from its kind");

        for (int tick = 0; tick < 4 * ticksPerSecond; tick++)
        {
            field.UpdateEnemyPositions();
        }
        Assert(count("Fast") == 2 && count("Late") == 1 && count("Hunter") == 0,
            "kinds respect their maximum, the run-time condition and the kill condition");
        Assert(Math.Abs(field.RunSeconds - 6) < 1e-6, "the run time is counted in game time");

        ArenaSaveGame save = field.CreateSaveGame();
        Assert(save.Enemies.Count(enemy => enemy.EnemyTypeId == "Fast") == 2
            && save.EnemySpawnTimers.ContainsKey("Late") && save.RunSeconds > 5,
            "save games keep each enemy's kind, the spawn timers and the run time");
        save.Enemies.First(enemy => enemy.EnemyTypeId == EnemyType.StandardId)
            .EnemyTypeId = null;
        field.LoadSaveGame(save, null);
        Assert(count("Fast") == 2 && count(EnemyType.StandardId) == 1
            && field.Enemies.First(enemy => enemy.TypeId == "Fast").MaxHitPoints == 50
            && Math.Abs(field.RunSeconds - save.RunSeconds) < 1e-9,
            "loading restores the kinds; enemies without a kind become Standard");

        NonPlayerCharacter standardEnemy =
            field.Enemies.First(enemy => enemy.TypeId == EnemyType.StandardId);
        NonPlayerCharacter otherKind = field.Enemies.First(enemy => enemy.TypeId == "Fast");
        standardEnemy.SetPosition(field.Player.PositionX + 500, field.Player.PositionY);
        otherKind.SetPosition(field.Player.PositionX + 500, field.Player.PositionY + 60);
        field.AlertEnemyAndNeighbours(standardEnemy);
        Assert(standardEnemy.IsAlerted && !otherKind.IsAlerted,
            "an alert only spreads to enemies of the same kind");
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
                && upgrade.Id != SwordUpgradeCatalog.Leach
                && upgrade.Id != SwordUpgradeCatalog.ChainLightningUpgrade)
            {
                weaponsMaxed &= progression.GetWeaponUpgradeRank(upgrade.Id)
                    == upgrade.MaximumRank;
            }
        }
        foreach (WeaponUpgradeDefinition upgrade in BowUpgradeCatalog.Upgrades)
        {
            // Kettenblitz builds on the lightning element, so it stays open as well.
            if (!BowUpgradeCatalog.IsElement(upgrade.Id)
                && upgrade.Id != BowUpgradeCatalog.ElementPower
                && upgrade.Id != BowUpgradeCatalog.ChainLightningUpgrade)
            {
                weaponsMaxed &= progression.GetWeaponUpgradeRank(upgrade.Id)
                    == upgrade.MaximumRank;
            }
        }
        foreach (WeaponUpgradeDefinition upgrade in MagicWandUpgradeCatalog.Upgrades)
        {
            if (!MagicWandUpgradeCatalog.IsElement(upgrade.Id)
                && upgrade.Id != MagicWandUpgradeCatalog.ElementPower
                && upgrade.Id != MagicWandUpgradeCatalog.ChainLightningUpgrade)
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
                    && migrated.GetKey(ArenaAction.GameSpeed) == Key.U,
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
        combat.Update(1, entities);
        Assert(!nearbyEnemy.ActiveDamageOverTimeEffects.Any(
            effect => effect.Id == "burning"),
            "fire no longer jumps to entities that are merely nearby");

        int startingHealth = firstEnemy.CurrentHitPoints;
        combat.Update(0.5, entities);
        Assert(firstEnemy.CurrentHitPoints < startingHealth,
            "damage-over-time effects deal damage during combat updates");

        // A burning entity sets what it hits on fire.
        TestEntity burningAttacker = new TestEntity(100, 300, 100, 10, CombatTeam.Enemies)
        {
            EquippedWeapon = WeaponCatalog.EnemyClaws
        };
        TestEntity hitPlayer = new TestEntity(140, 300, 100, 10, CombatTeam.Player);
        new BurningEffect(4, 3, 0).Apply(null, burningAttacker);
        ArenaCombatSystem hitCombat = new ArenaCombatSystem();
        Assert(hitCombat.TryStartAttack(burningAttacker, 1, 0), "the burning enemy attacks");
        hitCombat.Update(0.04, new List<Entity> { burningAttacker, hitPlayer });
        Assert(hitPlayer.ActiveDamageOverTimeEffects.Any(effect => effect.Id == "burning")
            && hitPlayer.ActiveDamageOverTimeEffects.First(effect => effect.Id == "burning")
                .RemainingSeconds >= ArenaCombatSystem.IgnitionBurnSeconds - 0.1,
            "a hit from a burning entity sets the target on fire");
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

        Assert(field.Player.IsRightHandBehindBody(PlayerFacing.Back)
            && field.Player.IsRightHandBehindBody(PlayerFacing.Right)
            && !field.Player.IsRightHandBehindBody(PlayerFacing.Front)
            && !field.Player.IsRightHandBehindBody(PlayerFacing.Left),
            "the right hand is behind the body seen from behind or from the right");
        Assert(field.Player.GetRightHandOffset(PlayerFacing.Front).X < 0
            && field.Player.GetRightHandOffset(PlayerFacing.Back).X > 0,
            "seen from the front the right hand is on the left of the screen");

        // Kurt (a cactus) holds weapons at the tip of his raised right arm, above his center.
        field.Player.AnimationProfileName = "Kurt";
        Assert(field.Player.GetRightHandOffset(PlayerFacing.Front).X < 0
            && field.Player.GetRightHandOffset(PlayerFacing.Front).Y < 0
            && field.Player.GetRightHandOffset(PlayerFacing.Back).X > 0
            && field.Player.GetRightHandOffset(PlayerFacing.Left).Y < 0,
            "Kurt holds weapons in his raised right arm");
        // Kurt's right arm is the lower one: drawn on the trunk seen from the right, hidden
        // seen from the left, and at the same height in every view, so it never jumps arms.
        Assert(!field.Player.IsRightHandBehindBody(PlayerFacing.Right)
            && field.Player.IsRightHandBehindBody(PlayerFacing.Left)
            && new[] { PlayerFacing.Front, PlayerFacing.Back, PlayerFacing.Left, PlayerFacing.Right }
                .All(facing => Math.Abs(field.Player.GetRightHandOffset(facing).Y
                    - field.Player.GetRightHandOffset(PlayerFacing.Front).Y) < 2),
            "Kurt keeps the weapon in his right arm when he turns");
        field.Player.AnimationProfileName = "BoxingKnight";

        field.SelectPlayerWeapon(WeaponCatalog.Bow);
        field.Player.UpdateStatusEffects(field.Player.EquippedWeapon.EquipReadySeconds);
        // Aim at a point (the mouse cursor) 120 units below the player.
        field.SetAimTarget(0, 120);
        Vector shot = field.GetProjectileAimDirection(field.Player.EquippedWeapon);
        field.AttackWithPlayer(0, 1);
        AttackVisualState arrow = field.ActiveAttackVisuals.Last(
            visual => visual.VisualKind == AttackVisualKind.Projectile);
        Vector hand = field.Player.GetRightHandOffset(PlayerFacing.Front);
        Point launch = new Point(field.Player.PositionX, field.Player.PositionY) + hand
            + shot * WeaponAppearance.BowArrowTipDistance;
        Assert(Math.Abs(arrow.ProjectileX - launch.X) < 1e-6
            && Math.Abs(arrow.ProjectileY - launch.Y) < 1e-6,
            "arrows are launched at the tip of the arrow nocked on the bow in the right hand");
        Point aimed = new Point(field.Player.PositionX, field.Player.PositionY + 120);
        Assert(AttackGeometry.DistanceToSegment(aimed,
                new Point(arrow.ProjectileX, arrow.ProjectileY),
                new Point(arrow.ProjectileX + arrow.DirectionX * 500,
                    arrow.ProjectileY + arrow.DirectionY * 500)) < 1e-6,
            "an arrow from the hand flies through the aimed point");
    }

    private static void TestTextureHitboxes()
    {
        // BoxingKnight: visible pixels x 38..90, y 13..121 of the 128-pixel frame, drawn
        // 4 * Size = 136 units tall.
        PlayerCharacter knight = new PlayerCharacter(500, 500);
        double unitsPerPixel = knight.Size * 4 / 128;
        Rect knightBox = knight.HitboxOffset;
        Assert(Math.Abs(knightBox.Width - 52 * unitsPerPixel) < 1e-6
            && Math.Abs(knightBox.Height - 108 * unitsPerPixel) < 1e-6
            && Math.Abs(knightBox.Left - (38 - 64) * unitsPerPixel) < 1e-6,
            "the knight's hitbox is the rectangle around its visible texture");

        PlayerCharacter kurt = new PlayerCharacter(500, 500) { AnimationProfileName = "Kurt" };
        Assert(kurt.HitboxOffset.Height > knightBox.Height
            && Math.Abs(kurt.HitboxOffset.Height - 128 * unitsPerPixel) < 1e-6,
            "Kurt's taller texture gives a taller hitbox");

        PursuerEnemy enemy = new PursuerEnemy(700, 500, PlayerCharacter.BaseMaxSpeed);
        double enemyUnitsPerPixel = enemy.Size * 2 / 128;
        Assert(Math.Abs(enemy.HitboxOffset.Width - 32 * enemyUnitsPerPixel) < 1e-6
            && Math.Abs(enemy.HitboxOffset.Height - 32 * enemyUnitsPerPixel) < 1e-6,
            "the enemy's hitbox is the rectangle around its idle texture");

        // An enemy walking towards the player stops where the rectangles touch.
        ArenaField field = new ArenaField();
        field.StartNewGame();
        List<NonPlayerCharacter> enemies = field.Enemies.ToList();
        for (int i = 1; i < enemies.Count; i++)
        {
            enemies[i].TakeDamage(enemies[i].MaxHitPoints);
        }
        NonPlayerCharacter walker = enemies[0];
        walker.PositionX = field.Player.PositionX;
        walker.PositionY = field.Player.PositionY + 150;
        for (int i = 0; i < 40; i++)
        {
            field.UpdateEnemyPositions();
        }
        Assert(!AttackGeometry.RectsOverlap(walker.Hitbox, field.Player.Hitbox)
            && walker.Hitbox.Top - field.Player.Hitbox.Bottom < 1,
            "an approaching enemy stops at the edge of the player's texture");
        Assert(walker.TryAttackTargetInRange(field.Player, new ArenaCombatSystem()),
            "an enemy touching the tall player from below can attack");
    }

    private static void TestFootstepEvents()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        int steps = 0;
        field.PlayerFootstep += () => steps++;

        field.UpdatePlayerMotion(true, 1, 0);
        for (int i = 0; i < 20; i++)
        {
            field.AdvanceFootsteps(0.05);
        }
        Assert(steps == 3,
            "walking for one second plays a step at once and then every 0.4 s");

        field.UpdatePlayerMotion(false, 0, 0);
        field.AdvanceFootsteps(0.05);
        field.UpdatePlayerMotion(true, 0, 1);
        field.AdvanceFootsteps(0.05);
        Assert(steps == 4, "a new walk starts with a step right away");

        field.ToggleSprint();
        for (int i = 0; i < 20; i++)
        {
            field.AdvanceFootsteps(0.05);
        }
        Assert(steps == 4, "sprinting plays no footsteps");
    }

    // Uses the combat system directly: ArenaField.Tick reads the real keyboard, so a key
    // pressed while the tests run must not move the player.
    private static void TestShockAndHealSoundEvents()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        List<string> statuses = new List<string>();
        int playerHeals = 0;
        field.EntityStatusApplied += (entity, id) => statuses.Add(id);
        field.PlayerHealingPulse += () => playerHeals++;

        new StunEffect(0.5).Apply(null, field.Enemies[0]);
        new StunEffect(0.5).Apply(null, field.Player);
        new FreezeEffect(0.5).Apply(null, field.Player);
        Assert(statuses.SequenceEqual(new[] { "stun", "stun", "freeze" }),
            "stuns (electric shocks) of enemies and the player are reported");

        field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.HealingPulse);
        int healSlot = Array.IndexOf(
            field.Progression.EntityAbilities.ToArray(), PlayerAbilityCatalog.HealingPulse);
        Assert(field.Player.CurrentHitPoints == field.Player.MaxHitPoints
            && field.AttackWithEntityAbility(healSlot) && playerHeals == 1,
            "every healing pulse is reported for its sound, also at full health");

        int blasts = 0;
        field.PlayerPowerBlast += () => blasts++;
        field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast);
        int blastSlot = Array.IndexOf(
            field.Progression.EntityAbilities.ToArray(), PlayerAbilityCatalog.PowerBlast);
        Assert(field.AttackWithEntityAbility(blastSlot) && blasts == 1,
            "releasing a Power Blast is reported for its sound");
    }

    private static void TestChainLightning()
    {
        ArenaProgression progression = new ArenaProgression();
        progression.SetGodMode(true);
        Assert(!progression.TryUpgradeBow(
                BowUpgradeCatalog.ChainLightningUpgrade, UpgradeCurrency.Universal),
            "Kettenblitz needs the lightning element");
        Assert(progression.TryUpgradeBow(BowUpgradeCatalog.ArrowDamage, UpgradeCurrency.Universal)
            && progression.TryUpgradeBow(BowUpgradeCatalog.Lightning, UpgradeCurrency.Universal)
            && BowUpgradeCatalog.CreateBow(progression).ChainLightning == null,
            "lightning arrows without the upgrade do not jump");
        Assert(progression.TryUpgradeBow(
                BowUpgradeCatalog.ChainLightningUpgrade, UpgradeCurrency.Universal),
            "Kettenblitz can be bought on top of lightning");
        ChainLightning first = BowUpgradeCatalog.CreateBow(progression).ChainLightning;
        progression.TryUpgradeBow(BowUpgradeCatalog.ChainLightningUpgrade, UpgradeCurrency.Universal);
        ChainLightning second = BowUpgradeCatalog.CreateBow(progression).ChainLightning;
        Assert(first.MaximumJumps == 1 && second.MaximumJumps == 2
            && second.JumpRadius > first.JumpRadius,
            "each rank lets the bolt jump to one more enemy over a larger radius");

        // A hit stuns the target; the bolt jumps to the two nearest enemies in reach.
        Weapon bolt = new Weapon(
            "chain-test", "Chain", string.Empty,
            new MeleeConeAttackPattern(60, Math.PI / 4, 0.1),
            new CompositeCombatEffect(new DamageEffect(1), new StunEffect(0.5)), 1,
            WeaponTargetingMode.Opponents, "chain-test",
            chainLightning: second);
        TestEntity caster = new TestEntity(100, 100, 100, 10, CombatTeam.Player);
        TestEntity target = new TestEntity(150, 100, 100, 0, CombatTeam.Enemies);
        TestEntity near = new TestEntity(150 + 20 + second.JumpRadius - 5, 100, 100, 0,
            CombatTeam.Enemies);
        TestEntity further = new TestEntity(near.PositionX + 40, 100, 100, 0, CombatTeam.Enemies);
        TestEntity outOfReach = new TestEntity(further.PositionX + 20 + second.JumpRadius + 20,
            100, 100, 0, CombatTeam.Enemies);
        TestEntity ally = new TestEntity(150, 130, 100, 0, CombatTeam.Player);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        List<Tuple<Entity, Entity>> jumps = new List<Tuple<Entity, Entity>>();
        combat.LightningChained += (from, to) => jumps.Add(Tuple.Create(from, to));
        Assert(combat.TryStartAttack(caster, bolt, 1, 0), "the lightning attack starts");
        combat.Update(0.04, new List<Entity> { caster, target, near, further, outOfReach, ally });
        Func<Entity, bool> stunned = entity =>
            entity.ActiveStatusEffects.Any(status => status.Id == "stun");
        Assert(stunned(target) && stunned(near) && stunned(further)
            && !stunned(outOfReach) && !stunned(ally),
            "the bolt jumps from enemy to enemy, never to allies or beyond its radius");
        Assert(jumps.Count == 2 && ReferenceEquals(jumps[0].Item1, target)
            && ReferenceEquals(jumps[0].Item2, near) && ReferenceEquals(jumps[1].Item2, further),
            "each jump starts from the last struck enemy");
    }

    private static string CreateTemporaryFolder()
    {
        string folder = Path.Combine(Path.GetTempPath(),
            "arena-saves-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folder);
        return folder;
    }

    private static ArenaField CreateSavingField(string folder)
    {
        return new ArenaField
        {
            SaveGames = new ArenaSaveGameRepository(folder),
            Scoreboard = ArenaScoreboard.Load(Path.Combine(folder, "highscores.json"))
        };
    }

    private static void TestSaveGamesRoundTrip()
    {
        string folder = CreateTemporaryFolder();
        try
        {
            ArenaField field = CreateSavingField(folder);
            field.StartNewGame();
            field.SelectPlayerWeapon(WeaponCatalog.Bow);
            field.Progression.TryUpgradeBow(BowUpgradeCatalog.ArrowDamage, UpgradeCurrency.Universal);
            field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast);
            field.Progression.AwardKill(WeaponCatalog.Bow);
            field.Player.AnimationProfileName = "Kurt";
            field.Player.PositionX = 400;
            field.Player.PositionY = 300;
            field.Player.TakeDamage(17);
            NonPlayerCharacter enemy = field.Enemies[0];
            enemy.PositionX = 900;
            enemy.PositionY = 700;
            enemy.TakeDamage(4);
            new PoisonEffect(3, 5).Apply(field.Player, enemy);
            new FreezeEffect(2).Apply(null, enemy);
            Assert(field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.MirrorImage)
                && field.AttackWithEntityAbility(Array.IndexOf(
                    field.Progression.EntityAbilities.ToArray(),
                    PlayerAbilityCatalog.MirrorImage))
                && field.MirrorImages.Count == 1,
                "a mirror image is active when the run is saved");
            MirrorImage image = field.MirrorImages[0];
            double mirrorCooldown = field.GetAttackCooldownRemaining(
                field.Player, field.Progression.GetEntityAbilityWeapon(Array.IndexOf(
                    field.Progression.EntityAbilities.ToArray(),
                    PlayerAbilityCatalog.MirrorImage)));
            Assert(field.SaveNow() && field.ActiveSaveSlot == 0,
                "the first save of a run takes the first free slot");

            ArenaField loaded = CreateSavingField(folder);
            Assert(loaded.LoadSlot(0), "a saved slot can be loaded");
            Assert(loaded.SelectedPlayerWeapon.Id == WeaponCatalog.Bow.Id
                && loaded.Progression.GetWeaponUpgradeRank(BowUpgradeCatalog.ArrowDamage) == 1
                && loaded.Progression.GetSkillRank(PlayerAbilityCatalog.PowerBlast) == 1
                && loaded.Progression.UniversalPoints == field.Progression.UniversalPoints
                && loaded.Progression.GetWeaponPoints(WeaponCatalog.Bow.Id)
                    == field.Progression.GetWeaponPoints(WeaponCatalog.Bow.Id)
                && loaded.Progression.TotalKills == 1
                && loaded.Progression.EntityAbilities[0] == PlayerAbilityCatalog.PowerBlast,
                "the run progress is restored");
            Assert(loaded.Player.AnimationProfileName == "Kurt"
                && Math.Abs(loaded.Player.PositionX - 400) < 1e-6
                && Math.Abs(loaded.Player.PositionY - 300) < 1e-6
                && loaded.Player.CurrentHitPoints == field.Player.CurrentHitPoints,
                "the player is restored with character, position and hit points");
            NonPlayerCharacter loadedEnemy = loaded.Enemies.Single();
            Assert(Math.Abs(loadedEnemy.PositionX - 900) < 1e-6
                && loadedEnemy.CurrentHitPoints == enemy.CurrentHitPoints
                && loadedEnemy.ActiveDamageOverTimeEffects.Any(effect => effect.Id == "poison"
                    && ReferenceEquals(effect.Source, loaded.Player))
                && loadedEnemy.ActiveStatusEffects.Any(status => status.Id == "freeze"),
                "enemies are restored with hit points and statuses");
            Assert(loaded.IsGameStarted && loaded.ActiveSaveSlot == 0
                && loaded.RunId == field.RunId,
                "a loaded run continues at once in its slot");
            MirrorImage loadedImage = loaded.MirrorImages.Single();
            Assert(Math.Abs(loadedImage.PositionX - image.PositionX) < 1e-6
                && Math.Abs(loadedImage.PositionY - image.PositionY) < 1e-6
                && loadedImage.CurrentHitPoints == 1
                && loadedImage.IsInvulnerable
                && Math.Abs(loadedImage.SpawnInvulnerabilityRemainingSeconds
                    - image.SpawnInvulnerabilityRemainingSeconds) < 1e-6,
                "mirror images are restored with position and spawn protection");
            int mirrorSlot = Array.IndexOf(
                loaded.Progression.EntityAbilities.ToArray(), PlayerAbilityCatalog.MirrorImage);
            Assert(mirrorCooldown > 0
                && Math.Abs(loaded.GetAttackCooldownRemaining(loaded.Player,
                    loaded.Progression.GetEntityAbilityWeapon(mirrorSlot)) - mirrorCooldown) < 1e-6
                && !loaded.AttackWithEntityAbility(mirrorSlot),
                "ability cooldowns survive loading, so Mirror Image cannot be reused at once");

            // Program start continues the run saved last.
            ArenaField second = CreateSavingField(folder);
            second.StartNewGameInSlot(3);
            ArenaField restarted = CreateSavingField(folder);
            restarted.ContinueLatestOrStartNew();
            Assert(restarted.ActiveSaveSlot == 3 && restarted.RunId == second.RunId,
                "the program start continues the most recently saved run");

            // Full slots: a new run is not saved until a slot is deleted.
            for (int slot = 0; slot < ArenaSaveGameRepository.SlotCount; slot++)
            {
                if (!new ArenaSaveGameRepository(folder).IsUsed(slot))
                {
                    CreateSavingField(folder).StartNewGameInSlot(slot);
                }
            }
            ArenaField overflow = CreateSavingField(folder);
            overflow.StartNewGame();
            Assert(!overflow.SaveNow() && overflow.IsRunUnsaved,
                "with all slots taken the new run is not saved over another one");
            overflow.DeleteSaveSlot(2);
            Assert(overflow.SaveNow() && overflow.ActiveSaveSlot == 2,
                "after deleting a save the run takes the freed slot");
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    private static void TestDeathRespawnAndHighScores()
    {
        string folder = CreateTemporaryFolder();
        try
        {
            ArenaField field = CreateSavingField(folder);
            field.StartNewGame();
            for (int kill = 0; kill < 7; kill++)
            {
                field.Progression.AwardKill(WeaponCatalog.Sword);
            }
            int skillPoints = field.Progression.UniversalPoints;
            int weaponPoints = field.Progression.GetWeaponPoints(WeaponCatalog.Sword.Id);
            field.Player.TakeDamage(field.Player.MaxHitPoints);
            Assert(field.IsGameOver && field.Deaths == 1
                && new ArenaSaveGameRepository(folder).Load(field.ActiveSaveSlot.Value)
                    .Player.HitPoints == 0,
                "a death is counted and saved at once");

            field.Player.PositionX = 100;
            NonPlayerCharacter nearMiddle = field.Enemies[0];
            nearMiddle.PositionX = field.WorldWidth / 2 + 40;
            nearMiddle.PositionY = field.WorldHeight / 2;
            Assert(field.Respawn(), "a dead player can respawn");
            Assert(field.Player.IsAlive
                && field.Player.CurrentHitPoints == field.Player.MaxHitPoints
                && Math.Abs(field.Player.PositionX - field.WorldWidth / 2) < 1e-6
                && field.IsGameStarted,
                "the player respawns in the middle with full hit points");
            Assert(field.Progression.UniversalPoints
                    == Math.Max(0, skillPoints - ArenaField.DeathSkillPointPenalty)
                && field.Progression.GetWeaponPoints(WeaponCatalog.Sword.Id)
                    == Math.Max(0, weaponPoints - ArenaField.DeathWeaponPointPenalty),
                "dying costs skill points and weapon points of the equipped weapon");
            double nearDistance = Math.Sqrt(
                Math.Pow(nearMiddle.PositionX - field.Player.PositionX, 2)
                + Math.Pow(nearMiddle.PositionY - field.Player.PositionY, 2));
            Assert(nearDistance >= ArenaField.RespawnClearRadius,
                "enemies near the middle are moved away from the respawned player");
            int hitPoints = field.Player.CurrentHitPoints;
            field.Player.TakeDamage(10);
            Assert(field.HasRespawnProtection && field.Player.CurrentHitPoints == hitPoints,
                "the respawned player is protected for a moment");
            for (int tick = 0; tick < 60 && field.HasRespawnProtection; tick++)
            {
                field.Tick();
            }
            field.Player.TakeDamage(10);
            Assert(!field.HasRespawnProtection && field.Player.CurrentHitPoints < hitPoints,
                "the protection ends after a moment");

            // Penalty never below zero (each death after the protection has ended).
            for (int death = 0; death < 2; death++)
            {
                field.Player.TakeDamage(field.Player.MaxHitPoints);
                field.Respawn();
                for (int tick = 0; tick < 60 && field.HasRespawnProtection; tick++)
                {
                    field.Tick();
                }
            }
            Assert(field.Progression.GetWeaponPoints(WeaponCatalog.Sword.Id) == 0
                && field.Deaths == 3,
                "points do not drop below zero");

            // High scores: kills minus a penalty per death, best first, live for the run.
            ArenaScoreboard board = ArenaScoreboard.Load(Path.Combine(folder, "board.json"));
            board.Record("a", "Slot 1", 20, 0, DateTime.Now);
            board.Record("b", "Slot 2", 30, 3, DateTime.Now);
            board.Record("c", "Slot 3", 12, 0, DateTime.Now);
            Assert(board.Entries.Select(entry => entry.RunId).SequenceEqual(new[] { "a", "b", "c" })
                && board.Entries[1].Score == 30 - 3 * ArenaScoreboard.DeathPenalty,
                "deaths lower the score and the list is sorted by score");
            Assert(board.Record("c", "Slot 3", 40, 0, DateTime.Now) == 1,
                "a run climbs the list while its score grows");
            for (int run = 0; run < 15; run++)
            {
                board.Record("x" + run, "Slot", run, 0, DateTime.Now);
            }
            Assert(board.Entries.Count == ArenaScoreboard.MaximumEntries
                && ArenaScoreboard.Load(Path.Combine(folder, "board.json")).Entries.Count
                    == ArenaScoreboard.MaximumEntries,
                "the list keeps the best ten runs and is saved");

            ArenaScoreEntry fieldEntry = field.Scoreboard.Entries.Single(
                entry => entry.RunId == field.RunId);
            Assert(fieldEntry.Deaths == 3 && fieldEntry.Score == field.TotalScore,
                "the running run is entered live into the high score list");
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }

    private static void TestSkillPointQuestionPreference()
    {
        string path = Path.Combine(Path.GetTempPath(),
            "arena-preferences-" + Guid.NewGuid().ToString("N") + ".json");
        try
        {
            ArenaPreferences preferences = ArenaPreferences.Load(path);
            Assert(preferences.AskBeforeSkillPointPayment,
                "the skill point question is asked by default");
            preferences.AskBeforeSkillPointPayment = false;
            Assert(!ArenaPreferences.Load(path).AskBeforeSkillPointPayment,
                "\"Nicht erneut fragen\" is remembered after a restart");
            File.WriteAllText(path, "{ broken");
            Assert(ArenaPreferences.Load(path).AskBeforeSkillPointPayment,
                "a broken preferences file falls back to asking");
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static void TestEnemyHitsPushPlayer()
    {
        foreach (bool invulnerable in new[] { false, true })
        {
            PlayerCharacter player = new PlayerCharacter(500, 500)
            {
                IsInvulnerable = invulnerable
            };
            TestEntity attacker = new TestEntity(540, 500, 30, 10, CombatTeam.Enemies)
            {
                EquippedWeapon = WeaponCatalog.EnemyClaws
            };
            ArenaCombatSystem combat = new ArenaCombatSystem();
            Assert(combat.TryStartAttack(attacker, -1, 0), "the enemy attack starts");
            combat.Update(ArenaField.GameTickDurationSeconds,
                new List<Entity> { player, attacker });
            Assert(500 - player.PositionX
                    >= WeaponCatalog.EnemyClawKnockbackDistance - 1e-6,
                invulnerable
                    ? "enemy hits push the player even in godmode"
                    : "enemy hits push the player back noticeably");
        }
    }

    private static void TestEnemyAlertAfterDirectHits()
    {
        // Only direct hits are reported, damage over time is not.
        TestEntity shooter = new TestEntity(500, 500, 30, 10, CombatTeam.Player)
        {
            EquippedWeapon = WeaponCatalog.EnemyClaws
        };
        TestEntity struckEnemy = new TestEntity(540, 500, 100, 10, CombatTeam.Enemies);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        List<Entity> struck = new List<Entity>();
        combat.EntityStruck += (source, target) => struck.Add(target);
        Assert(combat.TryStartAttack(shooter, 1, 0), "the test attack starts");
        combat.Update(ArenaField.GameTickDurationSeconds,
            new List<Entity> { shooter, struckEnemy });
        Assert(struck.Count == 1 && struck[0] == struckEnemy,
            "a direct hit is reported as a strike");
        TestEntity poisoned = new TestEntity(900, 900, 100, 10, CombatTeam.Enemies);
        new PoisonEffect(3, 5).Apply(shooter, poisoned);
        for (int i = 0; i < 20; i++)
        {
            combat.Update(0.1, new List<Entity> { poisoned });
        }
        Assert(poisoned.CurrentHitPoints < 100 && struck.Count == 1,
            "poison damage over time is not a strike and alerts no one");

        ArenaField field = new ArenaField();
        field.StartNewGame();
        field.Player.IsInvulnerable = true;
        while (field.Enemies.Count < 4)
        {
            field.SpawnEnemy();
        }
        double playerX = field.Player.PositionX;
        double playerY = field.Player.PositionY;
        NonPlayerCharacter hit = field.Enemies[0];
        NonPlayerCharacter near = field.Enemies[1];
        NonPlayerCharacter chained = field.Enemies[2];
        NonPlayerCharacter far = field.Enemies[3];
        hit.SetPosition(playerX + 500, playerY);
        near.SetPosition(playerX + 500, playerY + 80);
        chained.SetPosition(playerX + 500, playerY + 160);
        far.SetPosition(playerX + 500, playerY - 300);

        field.AlertEnemyAndNeighbours(hit);
        Assert(hit.IsAlerted && near.IsAlerted && chained.IsAlerted && !far.IsAlerted
            && Math.Abs(hit.AlertRemainingSeconds - EnemyType.DefaultAlertDurationSeconds) < 1e-9,
            "a hit alerts the enemy and, as a chain within 100 units, its neighbours");

        double distanceBefore = playerX + 500 - playerX;
        field.UpdateEnemyPositions();
        Assert(hit.CurrentTarget == field.Player && far.CurrentTarget == null
            && hit.PositionX - field.Player.PositionX < distanceBefore,
            "alerted enemies walk to the player outside their sight range");

        far.RestoreAlert(ArenaField.GameTickDurationSeconds);
        field.UpdateEnemyPositions();
        field.UpdateEnemyPositions();
        Assert(!far.IsAlerted && far.CurrentTarget == null,
            "the alert ends when its time is up");

        ArenaSaveGame save = field.CreateSaveGame();
        Assert(save.Enemies.Count(enemy => enemy.AlertSeconds > 0) == 3,
            "save games keep running alerts");
        field.LoadSaveGame(save, null);
        Assert(field.Enemies.Count(enemy => enemy.IsAlerted) == 3,
            "loading restores running alerts");

        NonPlayerCharacter seeing = field.Enemies.First(enemy => enemy.IsAlerted);
        seeing.SetPosition(field.Player.PositionX + 150, field.Player.PositionY);
        field.UpdateEnemyPositions();
        Assert(!seeing.IsAlerted && seeing.CurrentTarget == field.Player,
            "once the player is in sight the alert ends and normal pursuit continues");
        seeing.SetPosition(field.Player.PositionX + 600, field.Player.PositionY);
        field.UpdateEnemyPositions();
        Assert(seeing.CurrentTarget == null,
            "after the alert ended the enemy only follows players it can see");

        ArenaField attackField = new ArenaField();
        attackField.StartNewGame();
        if (attackField.Enemies.Count == 0)
        {
            attackField.SpawnEnemy();
        }
        NonPlayerCharacter target = attackField.Enemies[0];
        target.SetPosition(attackField.Player.PositionX + 50, attackField.Player.PositionY);
        attackField.AttackWithPlayer(1, 0);
        for (int i = 0; i < 20 && !target.IsAlerted; i++)
        {
            attackField.UpdateCombat(0.04);
        }
        Assert(target.IsAlerted, "a real player attack alerts the hit enemy");

        ArenaField killField = new ArenaField();
        killField.StartNewGame();
        while (killField.Enemies.Count < 2)
        {
            killField.SpawnEnemy();
        }
        NonPlayerCharacter victim = killField.Enemies[0];
        NonPlayerCharacter neighbour = killField.Enemies[1];
        victim.SetPosition(killField.Player.PositionX + 50, killField.Player.PositionY);
        neighbour.SetPosition(killField.Player.PositionX + 50, killField.Player.PositionY + 60);
        victim.CurrentHitPoints = 1;
        killField.AttackWithPlayer(1, 0);
        for (int i = 0; i < 20 && victim.IsAlive; i++)
        {
            killField.UpdateCombat(0.04);
        }
        Assert(!victim.IsAlive && !victim.IsAlerted && !neighbour.IsAlerted,
            "an enemy killed by the hit warns no one");
    }

    private static void TestFireTrailsAndStrikeTraces()
    {
        // A burning entity that moves drops fire at its feet; others touching it catch fire,
        // the entity itself is not set on fire again by its own trail.
        TestEntity burning = new TestEntity(100, 100, 100, 10, CombatTeam.Enemies);
        TestEntity bystander = new TestEntity(300, 300, 100, 10, CombatTeam.Enemies);
        new BurningEffect(4, 3, 0).Apply(null, burning);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        List<Entity> entities = new List<Entity> { burning, bystander };
        for (int i = 0; i < 6; i++)
        {
            burning.PositionX += 10;
            combat.Update(0.05, entities);
        }
        Assert(combat.GroundEffects.Count(effect => effect.Visual == "FireTrail") >= 2,
            "a moving burning entity leaves a fire trail");
        ArenaGroundEffect patch = combat.GroundEffects.First(
            effect => effect.Visual == "FireTrail");
        Assert(patch.Effect == null, "the fire trail only shows the fire");
        bystander.PositionX = patch.CenterX;
        bystander.PositionY = patch.CenterY - bystander.Hitbox.Height / 2;
        combat.Update(0.01, entities);
        Assert(!bystander.ActiveDamageOverTimeEffects.Any(effect => effect.Id == "burning"),
            "walking through a fire trail does not set an entity on fire");
        for (int i = 0; i < 40; i++)
        {
            combat.Update(0.05, new List<Entity>());
        }
        Assert(combat.GroundEffects.Count == 0, "fire trails burn out after a while");

        // The staff strike leaves traces of its element along its path that pass it on.
        ArenaProgression progression = new ArenaProgression();
        Assert(progression.TryUpgradeMagicWand(
                MagicWandUpgradeCatalog.ProjectileDamage, UpgradeCurrency.Universal)
            && progression.TryUpgradeMagicWand(
                MagicWandUpgradeCatalog.Poison, UpgradeCurrency.Universal)
            && progression.TryUpgradeMagicWand(
                MagicWandUpgradeCatalog.Strike, UpgradeCurrency.Universal),
            "the poison staff strike can be unlocked");
        Weapon strike = MagicWandUpgradeCatalog.CreateStrikeWeapon(progression);
        Assert(strike.HasTrail && strike.TrailVisual == "PoisenBubbels"
            && strike.TrailEffect is PoisonEffect,
            "the poison strike leaves poison traces");
        TestEntity caster = new TestEntity(100, 100, 100, 10, CombatTeam.Player);
        TestEntity victim = new TestEntity(500, 500, 100, 10, CombatTeam.Enemies);
        ArenaCombatSystem strikeCombat = new ArenaCombatSystem();
        Assert(strikeCombat.TryStartAttack(caster, strike, 1, 0), "the strike starts");
        int expectedTraces = (int)Math.Ceiling(
            strike.AttackPattern.Range / ArenaCombatSystem.StrikeTraceSpacing);
        strikeCombat.Update(0.05, new List<Entity> { caster, victim });
        int earlyTraces = strikeCombat.GroundEffects.Count;
        for (int i = 0; i < 10; i++)
        {
            strikeCombat.Update(0.05, new List<Entity> { caster, victim });
        }
        Assert(earlyTraces > 0 && earlyTraces < expectedTraces
            && strikeCombat.GroundEffects.Count == expectedTraces,
            "strike traces appear progressively along the whole strike");
        ArenaGroundEffect trace = strikeCombat.GroundEffects.Last();
        victim.PositionX = trace.CenterX;
        victim.PositionY = trace.CenterY;
        strikeCombat.Update(0.01, new List<Entity> { caster, victim });
        Assert(victim.ActiveDamageOverTimeEffects.Any(effect => effect.Id == "poison")
            && !caster.ActiveDamageOverTimeEffects.Any(),
            "strike traces poison opponents but not the caster");
    }

    private static void TestStaffSpritesAndHealEvents()
    {
        ArenaProgression progression = new ArenaProgression();
        WeaponAppearance plain = MagicWandUpgradeCatalog.CreateMagicWand(progression).Appearance;
        Assert(plain.EmptyAnimation == MagicWandUpgradeCatalog.EmptyStaffAnimation
            && plain.ProjectileAnimations.SequenceEqual(new[] { "ProjetilPlain" }),
            "the staff shoots its head and is empty until recharged");
        progression.TryUpgradeMagicWand(
            MagicWandUpgradeCatalog.ProjectileDamage, UpgradeCurrency.Universal);
        progression.TryUpgradeMagicWand(MagicWandUpgradeCatalog.Freeze, UpgradeCurrency.Universal);
        Assert(MagicWandUpgradeCatalog.CreateMagicWand(progression).Appearance
                .ProjectileAnimations.SequenceEqual(new[] { "ProjetilIce" }),
            "the ice staff shoots ice projectiles");

        TestEntity thief = new TestEntity(100, 100, 100, 10, CombatTeam.Player);
        TestEntity victim = new TestEntity(150, 100, 100, 10, CombatTeam.Enemies);
        thief.TakeDamage(50);
        Entity healedFrom = null;
        int healed = 0;
        thief.Healed += (entity, amount, from) => { healed = amount; healedFrom = from; };
        new LifeStealEffect(new DamageEffect(20), 0.5).Apply(thief, victim);
        Assert(healed == 10 && ReferenceEquals(healedFrom, victim),
            "stolen life reports the entity it was stolen from");
        new HealingEffect(5).Apply(null, thief);
        Assert(healed == 5 && healedFrom == null, "ordinary healing has no origin");
    }

    private static void TestParticlesFollowEvents()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        ArenaParticleRenderer particles = new ArenaParticleRenderer(new ArenaWeaponRenderer());
        particles.Attach(field);
        field.Player.TakeDamage(10);
        Assert(particles.ParticleCount >= 5, "a hurt entity sprays blood");
        for (int i = 0; i < 80; i++)
        {
            particles.Advance(field, 0.05);
        }
        Assert(particles.ParticleCount == 0, "blood stains vanish after a short time");

        new PoisonEffect(2, 2).Apply(null, field.Player);
        particles.Advance(field, 0.5);
        Assert(particles.ParticleCount > 0, "poisoned entities give off poison bubbles");
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
        Assert(enemy.CurrentHitPoints == 50 && player.CurrentHitPoints == 80,
            "healing is instant while the Power Blast wave has not yet reached the enemy");
        for (int i = 0; i < 8; i++)
        {
            combat.Update(0.04, new List<Entity> { player, enemy });
        }
        Assert(enemy.CurrentHitPoints == 30
            && player.CurrentHitPoints == 80,
            "the Power Blast wave runs outwards and damages the enemy once when it arrives");
    }

    private static void TestMirrorImageAbility()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        field.Player.IsInvulnerable = true;
        Assert(field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.MirrorImage)
            && field.Progression.EntityAbilities[0] == PlayerAbilityCatalog.MirrorImage,
            "Mirror Image can be unlocked and assigned to an entity-ability slot");

        Weapon ability = field.Progression.GetEntityAbilityWeapon(0);
        Assert(Math.Abs(ability.CooldownSeconds
                - PlayerAbilityCatalog.MirrorImageBaseCooldownSeconds) < 1e-9,
            "Mirror Image starts with a 20-second cooldown");
        Assert(field.AttackWithEntityAbility(0)
            && field.MirrorImages.Count == 1,
            "using rank one spawns one mirror image");

        MirrorImage firstImage = field.MirrorImages[0];
        double spawnOffsetX = field.Player.PositionX - firstImage.PositionX;
        double spawnOffsetY = field.Player.PositionY - firstImage.PositionY;
        double spawnDistance = Math.Sqrt(
            spawnOffsetX * spawnOffsetX + spawnOffsetY * spawnOffsetY);
        Assert(firstImage.Team == CombatTeam.Player
            && firstImage.CurrentHitPoints == 1
            && firstImage.MaxHitPoints == 1
            && firstImage.EquippedWeapon == null
            && firstImage.IsInvulnerable
            && spawnDistance >= ArenaField.MirrorImageMinimumSpawnDistance
            && spawnDistance <= ArenaField.MirrorImageSpawnRadius,
            "the image has one HP, cannot attack, is protected, and spawns near the player");
        double stationaryX = firstImage.PositionX;
        double stationaryY = firstImage.PositionY;
        firstImage.UpdateBehavior(field);
        Assert(firstImage.PositionX == stationaryX
            && firstImage.PositionY == stationaryY
            && !firstImage.CanBePushed,
            "a newly spawned image stays still for its first half-second");
        Assert(!field.AttackWithEntityAbility(0),
            "Mirror Image respects its cooldown");

        field.Progression.SetGodMode(true);
        for (int rank = 1; rank < PlayerAbilityCatalog.MirrorImageMaximumRank; rank++)
        {
            Assert(field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.MirrorImage),
                "Mirror Image can be upgraded through its maximum rank");
        }
        Assert(field.Progression.GetSkillRank(PlayerAbilityCatalog.MirrorImage) == 4,
            "Mirror Image has a maximum rank of four");

        field.UpdateCombat(21);
        Assert(field.AttackWithEntityAbility(0)
            && field.MirrorImages.Count == PlayerAbilityCatalog.MirrorImageMaximumRank
            && !firstImage.IsAlive,
            "each activation replaces old images and spawns the full rank-based count");

        MirrorImage movingImage = field.MirrorImages[0];
        double previousX = movingImage.PositionX;
        double previousY = movingImage.PositionY;
        movingImage.UpdateBehavior(field);
        Assert(movingImage.PositionX == previousX
            && movingImage.PositionY == previousY
            && movingImage.IsInvulnerable,
            "a fresh image is stationary and invulnerable");
        field.UpdateCombat(MirrorImage.SpawnStationaryDurationSeconds);
        movingImage.UpdateBehavior(field);
        Assert(previousX != movingImage.PositionX || previousY != movingImage.PositionY,
            "the image starts moving after half a second");
        Assert(movingImage.IsInvulnerable && !movingImage.CanBePushed,
            "the image remains invulnerable and collisionless after it starts moving");

        NonPlayerCharacter overlappingEnemy = field.Enemies[0];
        overlappingEnemy.PositionX = movingImage.PositionX;
        overlappingEnemy.PositionY = movingImage.PositionY;
        double enemyX = overlappingEnemy.PositionX;
        double enemyY = overlappingEnemy.PositionY;
        field.UpdateMirrorImagePositions();
        Assert(overlappingEnemy.PositionX == enemyX
            && overlappingEnemy.PositionY == enemyY,
            "mirror-image movement does not push overlapping enemies");

        field.UpdateCombat(
            MirrorImage.SpawnInvulnerabilityDurationSeconds
                - MirrorImage.SpawnStationaryDurationSeconds);
        Assert(!movingImage.IsInvulnerable,
            "the image becomes vulnerable after three seconds");
        movingImage.TakeDamage(1);
        Assert(!movingImage.IsAlive && !field.MirrorImages.Contains(movingImage),
            "a vulnerable killed image is removed from the arena");
    }

    private static void TestMirrorImageEnemyTargeting()
    {
        ArenaField field = new ArenaField();
        field.StartNewGame();
        field.Player.IsInvulnerable = true;
        field.Progression.SetGodMode(true);
        Assert(field.Progression.TryUpgradeSkill(PlayerAbilityCatalog.MirrorImage)
            && field.AttackWithEntityAbility(0),
            "Mirror Image can be summoned for enemy-targeting tests");

        MirrorImage mirrorImage = field.MirrorImages[0];
        field.Player.PositionX = 800;
        field.Player.PositionY = 600;
        mirrorImage.PositionX = 500;
        mirrorImage.PositionY = 600;
        NonPlayerCharacter enemy = field.Enemies[0];
        enemy.PositionX = 400;
        enemy.PositionY = 600;

        enemy.UpdateBehavior(field);
        Assert(ReferenceEquals(enemy.CurrentTarget, mirrorImage)
            && enemy.PositionX > 400,
            "an enemy pursues the closer mirror image instead of the player");

        for (int tick = 0; tick < 20 && !enemy.HasActiveAttack; tick++)
        {
            field.UpdateEnemyPositions();
        }
        Assert(enemy.HasActiveAttack
            && ReferenceEquals(enemy.CurrentTarget, mirrorImage),
            "an enemy in range attacks its selected mirror-image target");
        field.UpdateCombat(0.5);
        Assert(mirrorImage.IsAlive && mirrorImage.CurrentHitPoints == 1
            && mirrorImage.IsInvulnerable,
            "an enemy targets the image but cannot damage it during spawn protection");
        field.UpdateCombat(
            MirrorImage.SpawnInvulnerabilityDurationSeconds);
        Assert(!mirrorImage.IsInvulnerable,
            "the mirror image loses invulnerability after three seconds");
        field.UpdateEnemyPositions();
        Assert(enemy.HasActiveAttack,
            "the enemy can attack the image after its protection expires");
        field.UpdateCombat(0.5);
        Assert(!mirrorImage.IsAlive && !field.MirrorImages.Contains(mirrorImage),
            "an enemy attack can kill and remove a mirror image");
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
                && migratedWeaponWheel.GetKey(ArenaAction.WeaponAbility2) == Key.G
                && migratedWeaponWheel.GetKey(ArenaAction.OpenWeaponTree) == Key.D2,
                "saved ability bindings migrate away from the weapon-wheel and weapon-tree keys");

            File.WriteAllText(path,
                "{\"WeaponAbility2\":{\"Device\":\"Keyboard\",\"Key\":\"D2\"},"
                + "\"ZoomIn\":{\"Device\":\"Keyboard\",\"Key\":\"G\"}}");
            ArenaKeyBindings migratedWeaponTree = ArenaKeyBindings.Load(path);
            Assert(migratedWeaponTree.GetKey(ArenaAction.OpenWeaponTree) == Key.D2
                && migratedWeaponTree.GetKey(ArenaAction.ZoomIn) == Key.G
                && migratedWeaponTree.GetKey(ArenaAction.WeaponAbility2) == Key.U,
                "ability 2 leaves key 2 for the weapon tree and avoids keys already in use");

            File.WriteAllText(path,
                "{\"WeaponAbility3\":{\"Device\":\"Keyboard\",\"Key\":\"D3\"}}");
            ArenaKeyBindings migratedSkillTree = ArenaKeyBindings.Load(path);
            Assert(migratedSkillTree.GetKey(ArenaAction.OpenSkillTree) == Key.D3
                && migratedSkillTree.GetKey(ArenaAction.WeaponAbility3) == Key.Y,
                "ability 3 leaves key 3 for the skill tree and moves to Y");

            bool rejectedDevModifier = false;
            try
            {
                migratedWeaponTree.SetKey(ArenaAction.MoveUp, Key.F4);
            }
            catch (InvalidOperationException)
            {
                rejectedDevModifier = true;
            }
            Assert(rejectedDevModifier,
                "F4 stays reserved for the debug key combinations");
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
            && field.KeyBindings.GetKey(ArenaAction.OpenWeaponTree) == Key.D2
            && field.KeyBindings.GetKey(ArenaAction.WeaponAbility2) == Key.G
            && field.KeyBindings.GetKey(ArenaAction.OpenSkillTree) == Key.D3
            && field.KeyBindings.GetKey(ArenaAction.WeaponAbility3) == Key.Y,
            "1 opens the weapon wheel, 2 the weapon tree, 3 the skill tree; abilities use G and Y");

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

    private static void TestDevShortcutsSaveShortcutAndWeaponTreeKey()
    {
        ArenaField field = new ArenaField();
        field.KeyBindings.ResetToDefaults();
        field.StartNewGame();

        Assert(!field.HandleKeyDown(Key.B) && !field.HandleKeyDown(Key.H)
            && !field.ShowDamageRanges && !field.ShowDebugHud,
            "B and H alone no longer toggle the debug views");
        Assert(field.HandleKeyDown(Key.F4)
            && field.HandleKeyDown(Key.B) && field.ShowDamageRanges
            && field.HandleKeyDown(Key.H) && field.ShowDebugHud,
            "F4 + B and F4 + H toggle the debug ranges and the debug HUD");
        field.HandleKeyUp(Key.F4);
        Assert(!field.HandleKeyDown(Key.B) && field.ShowDamageRanges,
            "releasing F4 ends the debug key combination");
        field.HandleKeyDown(Key.F4);
        field.ReleaseHeldModifiers();
        Assert(!field.HandleKeyDown(Key.H) && field.ShowDebugHud,
            "losing window focus releases a held F4");

        double zoomBefore = field.ZoomLevel;
        Assert(field.HandleKeyDown(Key.K) && field.ZoomLevel > zoomBefore
            && field.HandleKeyDown(Key.L) && Math.Abs(field.ZoomLevel - zoomBefore) < 1e-9,
            "K zooms in and L zooms out");
        field.KeyBindings.SetKey(ArenaAction.ZoomIn, Key.I);
        Assert(!field.HandleKeyDown(Key.K) && field.HandleKeyDown(Key.I)
            && field.ZoomLevel > zoomBefore,
            "a rebound zoom key zooms instead of the fixed K");
        field.KeyBindings.ResetToDefaults();

        string folder = CreateTemporaryFolder();
        try
        {
            ArenaField savingField = CreateSavingField(folder);
            savingField.KeyBindings.ResetToDefaults();
            savingField.StartNewGame();
            bool sprintBefore = savingField.IsSprintEnabled;
            savingField.HandleKeyDown(Key.LeftCtrl);
            Assert(savingField.HandleKeyDown(Key.S)
                && savingField.ActiveSaveSlot.HasValue
                && savingField.SaveGames.Load(savingField.ActiveSaveSlot.Value) != null
                && savingField.SaveNoticeRemainingSeconds > 0
                && savingField.SaveNoticeText == "Spiel gespeichert",
                "Ctrl + S saves the running game and shows a confirmation");
            savingField.HandleKeyUp(Key.LeftCtrl);
            Assert(savingField.IsSprintEnabled == sprintBefore
                && savingField.IsGameStarted,
                "Ctrl + S neither toggles sprint nor pauses the game");
        }
        finally
        {
            Directory.Delete(folder, true);
        }

        ArenaField menuField = new ArenaField();
        menuField.KeyBindings.ResetToDefaults();
        Assert(!menuField.HandleKeyDown(Key.D2) && !menuField.HandleKeyDown(Key.D3)
            && menuField.MenuRequest == ArenaMenuRequest.None,
            "keys 2 and 3 do nothing without an active run");

        string previousDirectory = Environment.CurrentDirectory;
        try
        {
            Environment.CurrentDirectory = FindGameProjectDirectory();
            ArenaField treeField = new ArenaField();
            treeField.KeyBindings.ResetToDefaults();
            treeField.StartNewGame();
            treeField.SelectPlayerWeapon(WeaponCatalog.Bow);
            treeField.Progression.AwardKill(WeaponCatalog.Bow);
            PainterG9 painter = new PainterG9();
            Canvas canvas = new Canvas { Width = 1024, Height = 768 };
            canvas.Measure(new Size(1024, 768));
            canvas.Arrange(new Rect(0, 0, 1024, 768));
            painter.PaintGameField(canvas, treeField);

            Assert(treeField.HandleKeyDown(Key.D2)
                && !treeField.IsGameStarted
                && treeField.MenuRequest == ArenaMenuRequest.WeaponTree,
                "key 2 pauses the run and requests the weapon tree");
            painter.PaintGameField(canvas, treeField);
            Assert(treeField.MenuRequest == ArenaMenuRequest.None
                && FindMenuButtonIn(canvas, "Spiel fortsetzen") == null,
                "the painter opens the weapon tree instead of the pause menu");
            FindMenuButton(canvas, "Verbessern (1 Waffenpunkte)").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            Assert(treeField.Progression.GetWeaponUpgradeRank(
                BowUpgradeCatalog.AutoAim) == 1,
                "key 2 opens the tree of the equipped weapon (bow)");
            FindMenuButton(canvas, "Back").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            Assert(treeField.IsGameStarted,
                "Back from a weapon tree opened in the game returns to the game");
            painter.PaintGameField(canvas, treeField);

            Assert(treeField.HandleKeyDown(Key.D3)
                && treeField.MenuRequest == ArenaMenuRequest.SkillTree,
                "key 3 pauses the run and requests the skill tree");
            painter.PaintGameField(canvas, treeField);
            Assert(FindMenuButton(canvas, "Freischalten (3 universelle Punkte)") != null,
                "key 3 opens the skill tree");
            Assert(painter.HandleMenuEscape(canvas, treeField) && treeField.IsGameStarted,
                "Escape in a skill tree opened in the game returns to the game");
            painter.PaintGameField(canvas, treeField);

            treeField.ReturnToStartMenu();
            painter.PaintGameField(canvas, treeField);
            FindMenuButton(canvas, "Skilltree").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, treeField);
            Assert(painter.HandleMenuEscape(canvas, treeField)
                && !treeField.IsGameStarted,
                "Escape in a skill tree opened from the menu stays in the menu");
            painter.PaintGameField(canvas, treeField);
            Assert(FindMenuButton(canvas, "Spiel fortsetzen") != null,
                "... and returns to the pause menu it was opened from");
            Assert(!painter.HandleMenuEscape(canvas, treeField),
                "Escape on the pause menu itself is left to the game (continue)");

            FindMenuButton(canvas, "Ausrüstung").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, treeField);
            FindMenuButton(canvas, "Bogen — Waffenbaum").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, treeField);
            painter.HandleMenuEscape(canvas, treeField);
            painter.PaintGameField(canvas, treeField);
            Assert(FindMenuButton(canvas, "Bogen — Waffenbaum") != null,
                "Escape in a weapon tree returns to the equipment page");
            painter.HandleMenuEscape(canvas, treeField);
            painter.PaintGameField(canvas, treeField);
            Assert(FindMenuButton(canvas, "Spiel fortsetzen") != null,
                "Escape in the equipment page returns to the pause menu");

            Assert(treeField.HandleKeyDown(Key.D2),
                "key 2 also opens the weapon tree from the pause menu");
            canvas.Tag = null;
            painter.PaintGameField(canvas, treeField);
            Assert(FindMenuButtonIn(canvas, "Spiel fortsetzen") == null
                && FindMenuButton(canvas, "Back") != null,
                "the pause menu switches to the weapon tree");
            FindMenuButton(canvas, "Back").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, treeField);
            Assert(!treeField.IsGameStarted
                && FindMenuButton(canvas, "Spiel fortsetzen") != null,
                "Back from a weapon tree opened in the pause menu returns there");
        }
        finally
        {
            Environment.CurrentDirectory = previousDirectory;
        }
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
            NonPlayerCharacter enemy = field.Enemies[0];
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
            Assert(FindMenuButtonIn(canvas, "Waffen verbessern") == null,
                "the pause menu has no separate weapon-upgrade button any more");
            FindMenuButton(canvas, "Ausrüstung").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, WeaponCatalog.Sword.Name + " — Waffenbaum").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            painter.PaintGameField(canvas, field);
            field.Progression.AwardKill(WeaponCatalog.Sword);
            canvas.Tag = null;
            painter.PaintGameField(canvas, field);
            FindMenuButton(canvas, "Verbessern (1 Waffenpunkte)").RaiseEvent(
                new RoutedEventArgs(Button.ClickEvent));
            Assert(field.Progression.GetWeaponUpgradeRank(
                SwordUpgradeCatalog.StrikeDamage) == 1,
                "weapon tree from the equipment selection purchases the sword upgrade");

            FindMenuButton(canvas, "Back").RaiseEvent(
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

    private static void TestArenaSkillTreeRankSplitting()
    {
        string firstNodeId = "mirror-image-first";
        string secondNodeId = "mirror-image-second";
        ArenaSkillTreeDocument tree = new ArenaSkillTreeDocument
        {
            Name = "Split rank test",
            Nodes = new List<ArenaSkillTreeNode>
            {
                new ArenaSkillTreeNode
                {
                    SkillId = ArenaSkillTreeRepository.StartNodeId,
                    NodeId = ArenaSkillTreeRepository.StartNodeId
                },
                new ArenaSkillTreeNode
                {
                    SkillId = PlayerAbilityCatalog.MirrorImage,
                    NodeId = firstNodeId,
                    RankStages = 2,
                    X = 300
                },
                new ArenaSkillTreeNode
                {
                    SkillId = PlayerAbilityCatalog.MirrorImage,
                    NodeId = secondNodeId,
                    RankStages = 2,
                    X = 600
                }
            },
            Connections = new List<ArenaSkillTreeConnection>
            {
                new ArenaSkillTreeConnection
                {
                    FromSkillId = ArenaSkillTreeRepository.StartNodeId,
                    ToSkillId = firstNodeId
                },
                new ArenaSkillTreeConnection
                {
                    FromSkillId = firstNodeId,
                    ToSkillId = secondNodeId
                }
            }
        };
        ArenaSkillTreeRepository.ValidateTree(tree);

        ArenaProgression progression = new ArenaProgression();
        progression.SetSkillTree(tree);
        progression.SetGodMode(true);
        Assert(progression.CanUpgradeSkillNode(firstNodeId)
            && !progression.CanUpgradeSkillNode(secondNodeId),
            "a later duplicate skill tile remains unavailable until its rank range");
        Assert(progression.TryUpgradeSkillNode(firstNodeId)
            && progression.TryUpgradeSkillNode(firstNodeId)
            && progression.GetSkillRank(PlayerAbilityCatalog.MirrorImage) == 2
            && progression.CanUpgradeSkillNode(secondNodeId),
            "each skill tile grants only the allocated ranks before proceeding");
        Assert(progression.TryUpgradeSkillNode(secondNodeId)
            && progression.TryUpgradeSkillNode(secondNodeId)
            && progression.GetSkillRank(PlayerAbilityCatalog.MirrorImage) == 4
            && !progression.CanUpgradeSkill(PlayerAbilityCatalog.MirrorImage),
            "split ranks never allow the ability to exceed its maximum");

        string treeName = "ArenaRankSplitTest_" + Guid.NewGuid().ToString("N");
        string treePath = Path.Combine(
            ArenaSkillTreeRepository.TreeDirectory, treeName + ".json");
        try
        {
            ArenaSkillTreeRepository.SaveTree(treeName, tree);
            ArenaSkillTreeDocument loadedTree =
                ArenaSkillTreeRepository.LoadTreeByName(treeName);
            Assert(loadedTree.Nodes[1].NodeId == firstNodeId
                && loadedTree.Nodes[1].RankStages == 2
                && loadedTree.Nodes[2].NodeId == secondNodeId
                && loadedTree.Nodes[2].RankStages == 2,
                "split rank limits and unique node ids survive saving and loading");
        }
        finally
        {
            if (File.Exists(treePath))
            {
                File.Delete(treePath);
            }
        }

        tree.Nodes[2].RankStages = 3;
        bool rejectedExcessRanks = false;
        try
        {
            ArenaSkillTreeRepository.ValidateTree(tree);
        }
        catch (InvalidDataException)
        {
            rejectedExcessRanks = true;
        }
        Assert(rejectedExcessRanks,
            "skill tree validation rejects rank allocations above the ability maximum");
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
        Assert(ArenaSkillTreeRepository.GetImplementedSkills().Count == 6
            && ArenaSkillTreeRepository.GetImplementedSkills().Any(
                skill => skill.Id == PlayerAbilityCatalog.MirrorImage),
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

    private static void TestRandomBushPixelMatching()
    {
        Color background = Color.FromRgb(40, 170, 55);
        Assert(ArenaMapEditorControl.PixelMatchesBackground(
                background, background)
            && ArenaMapEditorControl.PixelMatchesBackground(
                Color.FromRgb(45, 165, 60), background)
            && !ArenaMapEditorControl.PixelMatchesBackground(
                Color.FromRgb(80, 170, 55), background),
            "random bush placement accepts matching green pixels within tolerance");
    }

    private static void TestArenaSkillTreeStartNode()
    {
        ArenaSkillTreeDocument centeredDefaultTree =
            ArenaSkillTreeRepository.CreateDefaultTree();
        ArenaSkillTreeNode defaultStart = centeredDefaultTree.Nodes.Single(
            node => ArenaSkillTreeRepository.IsStartNode(node.SkillId));
        Assert(ArenaSkillTreeVisuals.EditorBoardWidth == 4096
            && ArenaSkillTreeVisuals.EditorBoardHeight == 3072
            && defaultStart.X + ArenaSkillTreeVisuals.NodeWidth / 2
                == ArenaSkillTreeVisuals.EditorBoardWidth / 2
            && defaultStart.Y + ArenaSkillTreeVisuals.NodeHeight / 2
                == ArenaSkillTreeVisuals.EditorBoardHeight / 2,
            "the editor board is larger and the default start tile is centered");
        ArenaSkillTreeRepository.ValidateTree(centeredDefaultTree);

        PainterG9 characterPainter = new PainterG9();
        BitmapSource boxingKnightFrontIdle =
            characterPainter.GetSelectedCharacterFrontIdleFrame();
        Border startTile =
            ArenaSkillTreeVisuals.CreateStartNodeTile(boxingKnightFrontIdle);
        Assert(startTile.Child is Image startImage
            && ReferenceEquals(startImage.Source, boxingKnightFrontIdle),
            "the in-game start tile displays the selected character's front idle sprite");
        characterPainter.ResetAnimationProfile("Kurt");
        BitmapSource kurtFrontIdle =
            characterPainter.GetSelectedCharacterFrontIdleFrame();
        Assert(!ReferenceEquals(boxingKnightFrontIdle, kurtFrontIdle),
            "the start tile sprite follows the selected character profile");

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
        Border lockedTile = ArenaSkillTreeVisuals.CreateNodeTile(
            "Locked", "Gesperrt", ArenaSkillTreeTileState.Locked);
        Assert(lockedTile.BorderBrush is SolidColorBrush lockedBrush
            && lockedBrush.Color == Color.FromArgb(235, 235, 76, 76),
            "skills that cannot currently be purchased use a red tile state");

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
        Assert(!progression.HasChosenForcedForkBranch(fork.SkillId),
            "a forced fork has no selected branch before purchasing an exit");
        AssertForkMarkerColor(
            ArenaSkillTreeVisuals.CreateForkMarker(false, false),
            Colors.DeepSkyBlue,
            "free-choice forks use a blue marker");
        AssertForkMarkerColor(
            ArenaSkillTreeVisuals.CreateForkMarker(true, false),
            Colors.MediumPurple,
            "undecided forced forks use a purple marker");
        AssertForkMarkerColor(
            ArenaSkillTreeVisuals.CreateForkMarker(true, true),
            Colors.Red,
            "decided forced forks use a red marker");
        Border editorForkTile = ArenaSkillTreeVisuals.CreateForkTile(true);
        Assert(editorForkTile.Width == ArenaSkillTreeVisuals.NodeWidth
            && editorForkTile.Height == ArenaSkillTreeVisuals.NodeHeight,
            "the editor keeps its existing fork-tile visualization");
        Assert(progression.CanUpgradeSkill(PlayerAbilityCatalog.PowerBlast)
            && progression.CanUpgradeSkill(PlayerAbilityCatalog.HealingPulse),
            "both exits of a forced fork are available before the player chooses");
        Assert(progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast)
            && progression.HasChosenForcedForkBranch(fork.SkillId)
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

    private static void TestArenaSkillTreeForkInputThreshold()
    {
        ArenaSkillTreeNode fork = new ArenaSkillTreeNode
        {
            SkillId = ArenaSkillTreeRepository.CreateForkNodeId(),
            X = 400,
            Y = 100,
            IsFork = true,
            ForceDecision = true
        };
        ArenaSkillTreeDocument tree = new ArenaSkillTreeDocument
        {
            Name = "Fork input threshold test",
            Nodes = new List<ArenaSkillTreeNode>
            {
                new ArenaSkillTreeNode
                {
                    SkillId = ArenaSkillTreeRepository.StartNodeId,
                    X = 0,
                    Y = 100
                },
                new ArenaSkillTreeNode
                {
                    SkillId = PlayerAbilityCatalog.PowerBlast,
                    X = 150,
                    Y = 0
                },
                new ArenaSkillTreeNode
                {
                    SkillId = PlayerAbilityCatalog.HealingPulse,
                    X = 150,
                    Y = 200
                },
                fork,
                new ArenaSkillTreeNode
                {
                    SkillId = PlayerAbilityCatalog.SlowMotion,
                    X = 700,
                    Y = 100
                }
            },
            Connections = new List<ArenaSkillTreeConnection>
            {
                ArenaSkillTreeRepository.CreateStartConnection(
                    PlayerAbilityCatalog.PowerBlast),
                ArenaSkillTreeRepository.CreateStartConnection(
                    PlayerAbilityCatalog.HealingPulse),
                new ArenaSkillTreeConnection
                {
                    FromSkillId = PlayerAbilityCatalog.PowerBlast,
                    ToSkillId = fork.SkillId
                },
                new ArenaSkillTreeConnection
                {
                    FromSkillId = PlayerAbilityCatalog.HealingPulse,
                    ToSkillId = fork.SkillId
                },
                new ArenaSkillTreeConnection
                {
                    FromSkillId = fork.SkillId,
                    ToSkillId = PlayerAbilityCatalog.SlowMotion
                }
            }
        };
        ArenaSkillTreeRepository.ValidateTree(tree);

        ArenaProgression progression = new ArenaProgression();
        progression.SetSkillTree(tree);
        progression.SetGodMode(true);
        Assert(progression.GetForkInputsRemaining(fork.SkillId) == 2
            && !progression.CanUpgradeSkill(PlayerAbilityCatalog.SlowMotion),
            "a fork remains locked until its configured number of inputs is skilled");

        Assert(progression.TryUpgradeSkill(PlayerAbilityCatalog.PowerBlast)
            && progression.GetForkInputsRemaining(fork.SkillId) == 1
            && !progression.CanUpgradeSkill(PlayerAbilityCatalog.SlowMotion),
            "the default fork setting requires all inputs and tracks the missing one");
        fork.RequiredInputs = 1;
        Assert(progression.GetForkInputsRemaining(fork.SkillId) == 0
            && progression.CanUpgradeSkill(PlayerAbilityCatalog.SlowMotion),
            "the fork can be configured to accept one skilled input");
        fork.RequiredInputs = 2;
        Assert(progression.GetForkInputsRemaining(fork.SkillId) == 1
            && !progression.CanUpgradeSkill(PlayerAbilityCatalog.SlowMotion),
            "raising the threshold relocks downstream skills until the extra input is skilled");
        Grid waitingMarker =
            (Grid)ArenaSkillTreeVisuals.CreateForkMarkerVisual(true, false, 1);
        Assert(waitingMarker.Children.OfType<System.Windows.Shapes.Ellipse>()
                .Single().Fill == Brushes.Gray
            && waitingMarker.Children.OfType<TextBlock>()
                .Single().Text == "1",
            "a waiting fork is gray and shows the remaining input count");

        Assert(progression.TryUpgradeSkill(PlayerAbilityCatalog.HealingPulse)
            && progression.GetForkInputsRemaining(fork.SkillId) == 0
            && progression.CanUpgradeSkill(PlayerAbilityCatalog.SlowMotion),
            "the fork unlocks downstream skills once its input threshold is met");
        Grid readyMarker =
            (Grid)ArenaSkillTreeVisuals.CreateForkMarkerVisual(true, false, 0);
        Assert(readyMarker.Children.OfType<System.Windows.Shapes.Ellipse>()
                .Single().Fill == Brushes.MediumPurple
            && !readyMarker.Children.OfType<TextBlock>().Any(),
            "a satisfied fork returns to its normal decision color");

        string treeName = "ForkInputThreshold_" + Guid.NewGuid().ToString("N");
        string treePath = Path.Combine(
            ArenaSkillTreeRepository.TreeDirectory, treeName + ".json");
        try
        {
            ArenaSkillTreeRepository.SaveTree(treeName, tree);
            ArenaSkillTreeDocument loaded =
                ArenaSkillTreeRepository.LoadTreeByName(treeName);
            Assert(loaded.Nodes.Single(node => node.IsFork).RequiredInputs == 2,
                "the fork input threshold survives saving and loading");
        }
        finally
        {
            if (File.Exists(treePath))
            {
                File.Delete(treePath);
            }
        }

        Assert(ArenaSkillTreeRepository.GetPlayerStatName(
                    ArenaProgression.MaximumHealth) == "Gesundheit erhöhen um 20"
            && ArenaSkillTreeRepository.GetPlayerStatName(
                    ArenaProgression.MaximumSpeed)
                == "Geschwindigkeit erhöhen um 2",
            "health and speed skill labels state their per-rank increase");
    }

    private static void AssertForkMarkerColor(
        System.Windows.Shapes.Ellipse marker,
        Color expectedColor, string description)
    {
        Assert(marker.Width > 0 && marker.Height == marker.Width
            && marker.Fill is SolidColorBrush brush
            && brush.Color == expectedColor
            && marker.IsHitTestVisible == false,
            description);
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

    private static void TestWindowKeyboardAllowsTextEntry()
    {
        TextBox nameInput = new TextBox();
        Canvas gameCanvas = new Canvas();
        Assert(MainWindow.IsTextEntryFocused(nameInput)
            && !MainWindow.IsTextEntryFocused(gameCanvas)
            && !MainWindow.IsTextEntryFocused(null),
            "window keyboard routing distinguishes text inputs from game controls");
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
        Assert(FindMenuButtonIn(canvas, "Gegner neben mir spawnen") != null,
            "the dev tool has the enemy editor tab");
        Assert(FindMenuButton(canvas, "Map & Skilltree übernehmen") != null
            && FindMenuButtonIn(canvas, "Als Spielhintergrund verwenden") == null,
            "map and skill tree are applied together from one bar in the dev tool");

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

    private static void TestArenaMapChangeKeepsRun()
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
            NonPlayerCharacter enemy = field.Enemies[0];
            enemy.SetPosition(1500, 1100);
            field.ReturnToStartMenu();
            string runId = field.RunId;

            Assert(field.ApplyMapAndSkillTree(savedFileName, field.SkillTreeName)
                && field.MapFileName == savedFileName
                && field.WorldWidth == 512 && field.WorldHeight == 768
                && field.HasActiveGame && !field.IsGameStarted
                && field.RunId == runId
                && field.Progression.TotalKills == 1,
                "applying a map keeps the running game and its progress");
            Assert(Math.Abs(field.Player.PositionX - 256) < 1e-9
                && Math.Abs(field.Player.PositionY - 384) < 1e-9
                && field.HasRespawnProtection,
                "the player appears in the middle of the new map with spawn protection");
            Assert(field.MovementArea.Contains(enemy.PositionX, enemy.PositionY),
                "enemies outside the new map are moved onto it");
            Assert(!field.ApplyMapAndSkillTree(savedFileName, field.SkillTreeName),
                "applying the active map and skill tree again changes nothing");
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

    private static void TestArenaSkillTreeChangeKeepsRun()
    {
        string directory = ArenaSkillTreeRepository.TreeDirectory;
        string selectionPath = Path.Combine(directory, "SelectedSkillTree.txt");
        bool hadSelection = File.Exists(selectionPath);
        string previousSelection = hadSelection
            ? File.ReadAllText(selectionPath)
            : null;
        string treeName = "ArenaTest-" + Guid.NewGuid().ToString("N");
        string treePath = Path.Combine(directory, treeName + ".json");

        try
        {
            ArenaSkillTreeRepository.SaveTree(treeName,
                ArenaSkillTreeRepository.LoadTreeByName(
                    ArenaSkillTreeRepository.DefaultTreeName));
            ArenaField field = new ArenaField();
            field.StartNewGame();
            field.Progression.AwardKill(WeaponCatalog.Sword);
            field.Player.SetPosition(300, 300);
            field.ReturnToStartMenu();

            Assert(field.ApplyMapAndSkillTree(field.MapFileName, treeName)
                && field.SkillTreeName == treeName
                && field.HasActiveGame && !field.IsGameStarted
                && field.Progression.TotalKills == 1
                && Math.Abs(field.Player.PositionX - field.WorldWidth / 2) < 1e-9
                && Math.Abs(field.Player.PositionY - field.WorldHeight / 2) < 1e-9,
                "applying a skill tree keeps the run and places the player in the middle");

            ArenaSaveGame save = field.CreateSaveGame();
            Assert(save.SkillTreeName == treeName,
                "save games remember the skill tree of the run");
            field.ApplyMapAndSkillTree(
                field.MapFileName, ArenaSkillTreeRepository.DefaultTreeName);
            field.LoadSaveGame(save, null);
            Assert(field.SkillTreeName == treeName,
                "loading a save game restores its skill tree");
        }
        finally
        {
            if (File.Exists(treePath))
            {
                File.Delete(treePath);
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
            "leaving god mode restores damage and lets the player light up white");
        field.Player.UpdateStatusEffects(0.35);
        Assert(field.Player.DamageFlashIntensity == 0,
            "the damage flash ends after a short interval");
    }

    private static void TestPushableEntityMovement()
    {
        ArenaField pushableField = new ArenaField();
        pushableField.StartNewGame();
        pushableField.Player.PositionX = 650;
        pushableField.Player.PositionY = 600;
        NonPlayerCharacter pusher = pushableField.Enemies[0];
        pusher.PositionX = 500;
        pusher.PositionY = 600;
        pushableField.SpawnEnemy();
        NonPlayerCharacter pushable = pushableField.Enemies[1];
        // A small gap between the hitboxes, closed by the pusher's next step.
        double pushableStartX = 500 + pusher.Hitbox.Width + 3;
        pushable.PositionX = pushableStartX;
        pushable.PositionY = 600;
        pushable.CanBePushed = true;

        pushableField.UpdateEnemyPositions();

        Assert(pusher.PositionX > 500
            && pushable.PositionX > pushableStartX
            && pushable.Hitbox.Left >= pusher.Hitbox.Right - 1e-6,
            "moving entities push marked entities out of their way");

        ArenaField fixedField = new ArenaField();
        fixedField.StartNewGame();
        fixedField.Player.PositionX = 650;
        fixedField.Player.PositionY = 600;
        NonPlayerCharacter blockedMover = fixedField.Enemies[0];
        blockedMover.PositionX = 500;
        blockedMover.PositionY = 600;
        fixedField.SpawnEnemy();
        NonPlayerCharacter fixedEntity = fixedField.Enemies[1];
        double fixedStartX = 500 + blockedMover.Hitbox.Width + 3;
        fixedEntity.PositionX = fixedStartX;
        fixedEntity.PositionY = 600;
        fixedEntity.CanBePushed = false;

        fixedField.UpdateEnemyPositions();

        Assert(Math.Abs(blockedMover.PositionX - 500) < 0.001
            && fixedEntity.PositionX > fixedStartX,
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
        NonPlayerCharacter enemy = field.Enemies[0];
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
        // The patterns come from ArenaAudio itself, so new groups need no second list.
        IReadOnlyList<string> patterns = ArenaAudio.SoundFilePatterns;
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
        Assert(group.Play(100.21, true) >= 0,
            "a forced sound (lethal hit) is never merged into an earlier one");
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
