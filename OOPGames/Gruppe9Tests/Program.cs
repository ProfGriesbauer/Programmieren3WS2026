using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
            TestHealingWeapon();
            TestWeaponSelectionSurvivesRestart();
            TestWeaponMenuSelection();

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
            EquippedWeapon = WeaponCatalog.PracticeSword
        };
        TestEntity enemy = new TestEntity(150, 100, 30, 10, CombatTeam.Enemies);
        TestEntity ally = new TestEntity(150, 100, 30, 10, CombatTeam.Player);
        TestEntity outsideCone = new TestEntity(130, 205, 30, 10, CombatTeam.Enemies);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        IReadOnlyList<Entity> entities = new List<Entity>
        {
            player, enemy, ally, outsideCone
        };

        Assert(combat.TryStartAttack(player, 1, 0), "melee attack starts");
        Assert(!combat.TryStartAttack(player, 1, 0),
            "weapon cooldown rejects an immediate repeated attack");
        combat.Update(0.04, entities);
        Assert(enemy.CurrentHitPoints == 20,
            "melee cone applies one hit to an opponent");
        Assert(ally.CurrentHitPoints == 30 && outsideCone.CurrentHitPoints == 30,
            "melee cone excludes allies and targets outside its range");

        for (int i = 0; i < 5; i++)
        {
            combat.Update(0.04, entities);
        }
        Assert(enemy.CurrentHitPoints == 20,
            "one swing cannot hit the same target repeatedly");
    }

    private static void TestProjectileAndFreezeEffect()
    {
        TestEntity player = new TestEntity(100, 100, 100, 10, CombatTeam.Player)
        {
            EquippedWeapon = WeaponCatalog.Pistol
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
        player.EquippedWeapon = WeaponCatalog.FreezeStaff;
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
    }

    private static void TestHealingWeapon()
    {
        TestEntity player = new TestEntity(100, 100, 100, 10, CombatTeam.Player)
        {
            EquippedWeapon = WeaponCatalog.HealingPulse
        };
        player.TakeDamage(40);
        ArenaCombatSystem combat = new ArenaCombatSystem();
        Assert(combat.TryStartAttack(player, 0, 0),
            "self-targeted healing attack does not require an aim vector");
        combat.Update(0.04, new List<Entity> { player });
        Assert(player.CurrentHitPoints == 85,
            "healing weapon restores health without exceeding its maximum");
    }

    private static void TestWeaponSelectionSurvivesRestart()
    {
        ArenaField field = new ArenaField();
        field.SelectPlayerWeapon(WeaponCatalog.Pistol);
        field.HandleZoomWheel(120);
        field.ToggleDamageRanges();
        field.ToggleDebugHud();
        double zoomLevel = field.ZoomLevel;

        field.RestartGame();
        Assert(field.Player.EquippedWeapon.Id == WeaponCatalog.Pistol.Id,
            "selected test weapon is equipped after restart");
        Assert(field.ZoomLevel == zoomLevel
            && field.ShowDamageRanges && field.ShowDebugHud,
            "restart preserves zoom and debug display settings");
    }

    private static void TestWeaponMenuSelection()
    {
        ArenaField field = new ArenaField();
        PainterG9 painter = new PainterG9();
        Canvas canvas = new Canvas { Width = 1024, Height = 768 };
        canvas.Measure(new Size(1024, 768));
        canvas.Arrange(new Rect(0, 0, 1024, 768));

        painter.PaintGameField(canvas, field);
        FindMenuButton(canvas, "Waffen testen").RaiseEvent(
            new RoutedEventArgs(Button.ClickEvent));
        painter.PaintGameField(canvas, field);
        FindMenuButton(canvas, "Testpistole — Projectile").RaiseEvent(
            new RoutedEventArgs(Button.ClickEvent));
        Assert(field.SelectedPlayerWeapon.Id == WeaponCatalog.Pistol.Id,
            "weapon test menu equips the selected weapon");
    }

    private static Button FindMenuButton(Canvas canvas, string content)
    {
        foreach (object child in canvas.Children)
        {
            Border border = child as Border;
            StackPanel panel = border == null ? null : border.Child as StackPanel;
            if (panel == null)
            {
                continue;
            }

            Button button = panel.Children.OfType<Button>()
                .FirstOrDefault(candidate => Equals(candidate.Content, content));
            if (button != null)
            {
                return button;
            }
        }

        throw new InvalidOperationException("Menu button not found: " + content);
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
