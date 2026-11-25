using System.Collections.Generic;

namespace OOPGames
{
    public enum AttackVisualKind
    {
        Cone,
        Sweep,
        Projectile,
        Burst
    }

    public sealed class AttackVisualState
    {
        internal AttackVisualState(
            Entity source, AttackVisualKind visualKind, CombatEffectKind effectKind,
            string animationPlaceholder, double range, double radius, double halfAngleRadians)
        {
            Source = source;
            VisualKind = visualKind;
            EffectKind = effectKind;
            AnimationPlaceholder = animationPlaceholder;
            Range = range;
            Radius = radius;
            HalfAngleRadians = halfAngleRadians;
            CenterX = source.PositionX;
            CenterY = source.PositionY;
        }

        public Entity Source { get; }
        public AttackVisualKind VisualKind { get; }
        public CombatEffectKind EffectKind { get; }
        public string AnimationPlaceholder { get; }
        public double Range { get; }
        // Current radius of bursts: power blast and pulses grow outwards over their duration.
        public double Radius { get; internal set; }
        public double CenterX { get; internal set; }
        public double CenterY { get; internal set; }
        public double DirectionX { get; internal set; }
        public double DirectionY { get; internal set; }
        public double HalfAngleRadians { get; internal set; }
        public double Progress { get; internal set; }
        public double ProjectileX { get; internal set; }
        public double ProjectileY { get; internal set; }
        // Blade of a sword attack: the damage area is exactly this segment, from the grip in
        // the hand to the tip, BladeHalfWidth wide on each side. Unused when HasBlade is false.
        public double BladeStartX { get; internal set; }
        public double BladeStartY { get; internal set; }
        public double BladeEndX { get; internal set; }
        public double BladeEndY { get; internal set; }
        public double BladeHalfWidth { get; internal set; }
        public bool HasBlade { get { return BladeHalfWidth > 0; } }
        // Sprite clips of the weapon that started the attack; null without sprites.
        public WeaponAppearance Appearance { get; internal set; }
    }

    public interface IAttackInstance
    {
        bool IsComplete { get; }
        AttackVisualState VisualState { get; }
        IReadOnlyList<Entity> Update(double deltaSeconds, IReadOnlyList<Entity> targets);
    }

    public interface IAttackPattern
    {
        string Name { get; }
        double Range { get; }
        AttackVisualKind VisualKind { get; }
        double DebugHalfAngleRadians { get; }
        double DebugRadius { get; }
        IAttackInstance BeginAttack(
            Entity source, double directionX, double directionY, Weapon weapon);
    }
}
