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
        public double Radius { get; }
        public double CenterX { get; internal set; }
        public double CenterY { get; internal set; }
        public double DirectionX { get; internal set; }
        public double DirectionY { get; internal set; }
        public double HalfAngleRadians { get; internal set; }
        public double Progress { get; internal set; }
        public double ProjectileX { get; internal set; }
        public double ProjectileY { get; internal set; }
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
