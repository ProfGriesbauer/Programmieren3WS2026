using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // Draws the weapon sprites from Assets/Weapons: the weapon in the hand of an entity
    // (carried, swung, aimed) and flying projectiles such as arrows. Which clip is shown comes
    // from Weapon.Appearance; this class only decides size, pose, frame and position.
    //
    // Sizes follow the damage areas at the start of a run: a blade reaches to the edge of its
    // slash, a staff to the end of the staff strike; upgrades keep that size. The bow keeps a
    // fixed size; its nocked arrow lies where the projectile is launched, so the flying arrow
    // continues seamlessly from the bow. The bow is fully drawn exactly when it can shoot.
    internal sealed class ArenaWeaponRenderer
    {
        // Weapon frames share the 128x128 pixel grid of the character frames.
        public const double FramePixelSize = 128;
        // Tip of a nocked arrow in front of the bow grip, in weapon pixels.
        private const double NockedArrowTipPixels = 94 - 89;
        // World units per weapon pixel of the bow and its arrows (2.6, bow about 68 units
        // tall); derived so that the drawn arrow tip lies where arrows are launched.
        public const double BowUnitsPerPixel =
            WeaponAppearance.BowArrowTipDistance / NockedArrowTipPixels;
        // Part of the reach in front of the body that blade and staff sprites do not cover.
        private const double AttackGripDistance = AttackGeometry.BladeGripGap;
        // How far the staff is pushed forward out of the hand when it strikes or casts.
        private const double StaffThrustDistance = 8;
        // Seconds the staff stays pointed at the target after a projectile was cast.
        private const double StaffCastPoseSeconds = 0.2;
        // Share of the cooldown after a shot that shows the released, empty bow.
        private const double BowReleasedShare = 0.3;
        // Units a projectile travels before its next animation frame is shown.
        private const double ProjectileFrameDistance = 25;

        private static readonly string[] SpriteSheetPath = { "Weapons", "Weapons.png" };
        private static readonly string[] MetadataPath = { "Weapons", "Weapons.json" };

        // Pivots: blade at the hilt, staff at its lower end, bow at the stave where the hand
        // holds it, flying arrow at its tip (the hit point of the projectile).
        private static readonly SpriteLayout BladeLayout =
            new SpriteLayout(new Int32Rect(62, 36, 20, 40), 71.5, 67, -90, 23);
        private static readonly SpriteLayout StaffLayout =
            new SpriteLayout(new Int32Rect(48, 38, 20, 54), 58, 88.5, -90, 42.5);
        private static readonly SpriteLayout BowLayout =
            new SpriteLayout(new Int32Rect(74, 34, 24, 32), 89, 50.5, 0, 0);
        // Same pivot as the bow, but cropped to the arrow row: the arrow frames contain a
        // stray pixel above the arrow that must not appear on the bow.
        private static readonly SpriteLayout NockedArrowLayout =
            new SpriteLayout(new Int32Rect(74, 44, 24, 10), 89, 50.5, 0, 0);
        // Magic projectiles (the staff's head) are round: drawn centered and unrotated.
        private static readonly SpriteLayout OrbLayout =
            new SpriteLayout(new Int32Rect(46, 40, 20, 18), 57, 48.5, 0, 0);
        private static readonly SpriteLayout FlyingArrowLayout =
            new SpriteLayout(new Int32Rect(74, 44, 24, 10), 94, 50.5, 0, 0);
        // String position of the four draw frames relative to the fully drawn string (x = 80),
        // where the arrow frames place the end of the arrow.
        private static readonly double[] BowStringOffsets = { 6, 5, 4, 0 };

        private readonly Dictionary<string, BitmapSource> _croppedFrames =
            new Dictionary<string, BitmapSource>(StringComparer.Ordinal);
        private SpriteSheetAnimator _sheet;
        private bool _sheetUnavailable;

        // Paints the weapon of an entity. Called twice per frame: once before the entity
        // sprite (behindEntity = true) and once after it, so a weapon on the far side is
        // hidden by the body. unitsPerFramePixel converts character frame pixels to world
        // units.
        public void PaintEntityWeapon(
            Canvas canvas, ArenaField field, Entity entity,
            double centerX, double centerY, double unitsPerFramePixel, bool behindEntity)
        {
            Weapon weapon = entity.EquippedWeapon;
            if (weapon == null || weapon.Appearance == null || !entity.IsAlive)
            {
                return;
            }

            // The weapon lies in the right hand: when that hand is on the far side of the body
            // (seen from behind or from the right), the body covers the weapon.
            PlayerFacing facing = GetShownFacing(field, entity);
            if (IsHandBehindBody(entity, facing) != behindEntity)
            {
                return;
            }
            WeaponPose pose = GetPose(
                field, entity, weapon, facing, GetHandOffset(entity, facing),
                unitsPerFramePixel);

            SpriteLayout layout = GetLayout(pose.Appearance.Kind);
            double unitsPerPixel = GetUnitsPerPixel(
                layout, pose.Appearance.Kind, pose.Reach, unitsPerFramePixel);
            double pivotX = centerX + pose.OffsetX;
            double pivotY = centerY + pose.OffsetY;
            BitmapSource frame = GetCroppedFrame(
                pose.Animation, pose.FrameIndex, layout);
            if (frame != null)
            {
                AddSprite(canvas, frame, layout, 0, pivotX, pivotY,
                    unitsPerPixel, pose.DirectionX, pose.DirectionY);
            }

            if (pose.NockedArrow != null)
            {
                // The arrow frames share the bow grid; shifting by the string position
                // keeps the end of the arrow on the string while the bow is drawn.
                BitmapSource arrow = GetCroppedFrame(pose.NockedArrow, 0, NockedArrowLayout);
                if (arrow != null)
                {
                    AddSprite(canvas, arrow, NockedArrowLayout,
                        BowStringOffsets[Math.Min(pose.FrameIndex, BowStringOffsets.Length - 1)],
                        pivotX, pivotY, unitsPerPixel, pose.DirectionX, pose.DirectionY);
                }
            }
        }

        // Paints arrows and other projectiles whose weapon has projectile sprites. The tip of
        // the sprite lies on the projectile position, where hits are detected.
        public void PaintProjectiles(Canvas canvas, ArenaField field, double left, double top)
        {
            foreach (AttackVisualState visual in field.ActiveAttackVisuals)
            {
                if (visual.VisualKind != AttackVisualKind.Projectile
                    || visual.Appearance == null
                    || !visual.Appearance.HasProjectileSprite)
                {
                    continue;
                }

                IReadOnlyList<string> animations = visual.Appearance.ProjectileAnimations;
                int index = (int)(visual.Progress * visual.Range / ProjectileFrameDistance)
                    % animations.Count;
                bool isOrb = visual.Appearance.Kind == WeaponSpriteKind.Staff;
                SpriteLayout layout = isOrb ? OrbLayout : FlyingArrowLayout;
                BitmapSource frame = GetCroppedFrame(animations[index], 0, layout);
                if (frame == null)
                {
                    continue;
                }

                AddSprite(canvas, frame, layout, 0,
                    visual.ProjectileX - left, visual.ProjectileY - top,
                    BowUnitsPerPixel,
                    isOrb ? 1 : visual.DirectionX, isOrb ? 0 : visual.DirectionY);
            }
        }

        private static WeaponPose GetPose(
            ArenaField field, Entity entity, Weapon weapon, PlayerFacing facing,
            Vector hand, double unitsPerFramePixel)
        {
            WeaponAppearance appearance = weapon.Appearance;
            GetAimDirection(field, entity, out double aimX, out double aimY);
            if (appearance.Kind == WeaponSpriteKind.Bow)
            {
                return GetBowPose(field, entity, weapon, hand, aimX, aimY);
            }

            // Sword slash and spin: the sprite is drawn exactly on the blade the combat system
            // hits with, from the grip in the hand to the tip.
            AttackVisualState melee = FindMeleeAttack(field, entity);
            if (melee != null && melee.HasBlade)
            {
                Vector blade = new Vector(
                    melee.BladeEndX - melee.BladeStartX, melee.BladeEndY - melee.BladeStartY);
                double length = blade.Length;
                if (length > 1e-9)
                {
                    blade /= length;
                    return WeaponPose.Aimed(
                        melee.Appearance, melee.Appearance.AttackAnimation, 0,
                        blade.X, blade.Y,
                        new Vector(melee.BladeStartX - entity.PositionX,
                            melee.BladeStartY - entity.PositionY),
                        length + AttackGripDistance);
                }
            }

            // Staff strike: the grip stays in the hand and the staff points at the current
            // point of the hit line, keeping its carried size.
            if (melee != null)
            {
                double thrust = melee.VisualKind == AttackVisualKind.Cone
                    ? StaffThrustDistance * Math.Sin(Math.Min(1, melee.Progress) * Math.PI)
                    : 0;
                return WeaponPose.FromHand(
                    melee.Appearance, melee.Appearance.AttackAnimation, hand,
                    melee.DirectionX, melee.DirectionY, melee.Appearance.Reach, thrust);
            }

            if (appearance.Kind == WeaponSpriteKind.Staff)
            {
                // The shot head leaves the staff empty until the cooldown has recharged it;
                // right after the cast the staff briefly points at the target.
                double remaining = field.GetAttackCooldownRemaining(entity, weapon);
                double elapsed = weapon.CooldownSeconds - remaining;
                string animation = remaining > 0 && appearance.EmptyAnimation != null
                    ? appearance.EmptyAnimation
                    : appearance.HeldAnimation;
                if (remaining > 0 && elapsed < StaffCastPoseSeconds)
                {
                    return WeaponPose.FromHand(
                        appearance, animation, hand, aimX, aimY, appearance.Reach,
                        StaffThrustDistance * Math.Sin(elapsed / StaffCastPoseSeconds * Math.PI));
                }

                WeaponPose staff = WeaponPose.Held(appearance, facing, hand, unitsPerFramePixel);
                staff.Animation = animation;
                return staff;
            }

            return WeaponPose.Held(appearance, facing, hand, unitsPerFramePixel);
        }

        // The bow always points where the entity aims. After equipping it is drawn once over
        // its equip time and then stays drawn with an arrow on the string; after a shot it
        // shows the released string and is drawn again while the cooldown runs. The last,
        // fully drawn frame appears exactly when the bow can shoot again.
        private static WeaponPose GetBowPose(
            ArenaField field, Entity entity, Weapon weapon, Vector hand,
            double aimX, double aimY)
        {
            WeaponAppearance appearance = weapon.Appearance;
            int drawnFrame = BowStringOffsets.Length - 1;
            int frameIndex = drawnFrame;
            bool arrowNocked = true;
            double remaining = field.GetAttackCooldownRemaining(entity, weapon);
            if (remaining > 0 && weapon.CooldownSeconds > 0)
            {
                double progress = 1 - remaining / weapon.CooldownSeconds;
                if (progress < BowReleasedShare)
                {
                    frameIndex = 0;
                    arrowNocked = false;
                }
                else
                {
                    // Frames before the fully drawn one share the rest of the reload time.
                    frameIndex = 1 + (int)Math.Floor((progress - BowReleasedShare)
                        / (1 - BowReleasedShare) * (drawnFrame - 1));
                }
                frameIndex = Math.Min(drawnFrame - 1, frameIndex);
            }
            else if (entity.WeaponEquippedSeconds < weapon.EquipReadySeconds)
            {
                frameIndex = Math.Min(drawnFrame - 1, (int)Math.Floor(
                    entity.WeaponEquippedSeconds / weapon.EquipReadySeconds * drawnFrame));
            }

            // The grip stays in the hand and the bow turns around it. Arrows are launched at
            // the tip of the nocked arrow (WeaponAppearance.ProjectileLaunchDistance), so the
            // shot arrow leaves the bow exactly there.
            AttackGeometry.NormalizeDirection(aimX, aimY, out double x, out double y);
            WeaponPose pose = WeaponPose.Aimed(
                appearance, appearance.HeldAnimation, frameIndex, x, y, hand, 0);
            pose.NockedArrow = arrowNocked && appearance.HasProjectileSprite
                ? appearance.ProjectileAnimations[0]
                : null;
            return pose;
        }

        private static bool IsHandBehindBody(Entity entity, PlayerFacing facing)
        {
            return entity is PlayerCharacter player
                ? player.IsRightHandBehindBody(facing)
                : facing == PlayerFacing.Back;
        }

        // Frame of a magic projectile sprite, e.g. for lightning traces of the staff strike.
        internal BitmapSource GetOrbFrame(string animation)
        {
            return GetCroppedFrame(animation, 0, OrbLayout);
        }

        // Right hand of players; other entities hold weapons at their center.
        private static Vector GetHandOffset(Entity entity, PlayerFacing facing)
        {
            return entity is PlayerCharacter player
                ? player.GetRightHandOffset(facing)
                : new Vector(0, 0);
        }

        // Facing of the sprite that is actually shown; the spin switches views on its own.
        private static PlayerFacing GetShownFacing(ArenaField field, Entity entity)
        {
            return ReferenceEquals(entity, field.Player) && field.IsPlayerSpinning
                ? field.PlayerSpinFacing
                : entity.Facing;
        }

        private static AttackVisualState FindMeleeAttack(ArenaField field, Entity entity)
        {
            AttackVisualState latest = null;
            foreach (AttackVisualState visual in field.ActiveAttackVisuals)
            {
                if (ReferenceEquals(visual.Source, entity)
                    && visual.Appearance != null
                    && visual.VisualKind != AttackVisualKind.Projectile
                    && (visual.DirectionX != 0 || visual.DirectionY != 0))
                {
                    latest = visual;
                }
            }
            return latest;
        }

        private static void GetAimDirection(
            ArenaField field, Entity entity, out double directionX, out double directionY)
        {
            if (ReferenceEquals(entity, field.Player))
            {
                // Same direction the projectile will fly in, so the bow points at the cursor.
                Vector aim = field.GetProjectileAimDirection(entity.EquippedWeapon);
                directionX = aim.X;
                directionY = aim.Y;
                return;
            }

            switch (entity.Facing)
            {
                case PlayerFacing.Left: directionX = -1; directionY = 0; break;
                case PlayerFacing.Right: directionX = 1; directionY = 0; break;
                case PlayerFacing.Back: directionX = 0; directionY = -1; break;
                default: directionX = 0; directionY = 1; break;
            }
        }

        private static SpriteLayout GetLayout(WeaponSpriteKind kind)
        {
            switch (kind)
            {
                case WeaponSpriteKind.Staff: return StaffLayout;
                case WeaponSpriteKind.Bow: return BowLayout;
                default: return BladeLayout;
            }
        }

        // Blades and staffs are scaled so that the part in front of the grip spans the reach
        // minus the grip distance (for blades exactly the blade length of the damage area,
        // see AttackGeometry.BladeHalfWidthPerLength); the bow keeps its fixed scale.
        private static double GetUnitsPerPixel(
            SpriteLayout layout, WeaponSpriteKind kind, double reach,
            double unitsPerFramePixel)
        {
            if (kind == WeaponSpriteKind.Bow)
            {
                return BowUnitsPerPixel;
            }
            if (reach <= AttackGripDistance || layout.ReachPixels <= 0)
            {
                return unitsPerFramePixel;
            }
            return (reach - AttackGripDistance) / layout.ReachPixels;
        }

        private BitmapSource GetCroppedFrame(
            string animation, int frameIndex, SpriteLayout layout)
        {
            SpriteSheetAnimator sheet = GetSheet();
            if (sheet == null)
            {
                return null;
            }

            int frameCount = sheet.GetFrameCount(animation);
            if (frameCount == 0)
            {
                return null;
            }

            frameIndex = Math.Max(0, Math.Min(frameCount - 1, frameIndex));
            string key = animation + "#" + frameIndex + "#" + layout.Crop;
            if (_croppedFrames.TryGetValue(key, out BitmapSource cropped))
            {
                return cropped;
            }

            BitmapSource frame = sheet.GetClipFrame(animation, frameIndex);
            cropped = new CroppedBitmap(frame, layout.Crop);
            cropped.Freeze();
            _croppedFrames.Add(key, cropped);
            return cropped;
        }

        private SpriteSheetAnimator GetSheet()
        {
            if (_sheet == null && !_sheetUnavailable)
            {
                try
                {
                    _sheet = new SpriteSheetAnimator(
                        PainterG9.FindAssetPath(SpriteSheetPath),
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
                    // Missing or broken weapon assets must not stop the game.
                    _sheetUnavailable = true;
                }
            }
            return _sheet;
        }

        // Places the sprite so its pivot lies on the given point and its weapon axis points
        // along the direction. alongAxisPixels moves the sprite content along its axis (used
        // to keep a nocked arrow on the string). Bow and arrows pointing to the left are
        // flipped vertically instead of being shown upside down; blades pointing to the right
        // are mirrored, so their light edge always faces upwards.
        private static void AddSprite(
            Canvas canvas, BitmapSource frame, SpriteLayout layout, double alongAxisPixels,
            double pivotX, double pivotY, double unitsPerPixel,
            double directionX, double directionY)
        {
            double width = layout.Crop.Width * unitsPerPixel;
            double height = layout.Crop.Height * unitsPerPixel;
            double localPivotX = layout.PivotX - layout.Crop.X - alongAxisPixels;
            double localPivotY = layout.PivotY - layout.Crop.Y;
            double angle = Math.Atan2(directionY, directionX) * 180 / Math.PI;
            bool flipVertically = layout.AxisDegrees == 0 && directionX < 0;
            bool mirrorBlade = ReferenceEquals(layout, BladeLayout) && directionX > 0;

            TransformGroup transform = new TransformGroup();
            transform.Children.Add(new ScaleTransform(
                mirrorBlade ? -1 : 1, flipVertically ? -1 : 1));
            transform.Children.Add(new RotateTransform(angle - layout.AxisDegrees));
            Image image = new Image
            {
                Source = frame,
                Width = width,
                Height = height,
                Stretch = Stretch.Fill,
                RenderTransformOrigin = new Point(
                    localPivotX / layout.Crop.Width, localPivotY / layout.Crop.Height),
                RenderTransform = transform,
                IsHitTestVisible = false
            };
            RenderOptions.SetBitmapScalingMode(image, BitmapScalingMode.NearestNeighbor);
            Canvas.SetLeft(image, pivotX - localPivotX * unitsPerPixel);
            Canvas.SetTop(image, pivotY - localPivotY * unitsPerPixel);
            canvas.Children.Add(image);
        }

        private sealed class SpriteLayout
        {
            public SpriteLayout(
                Int32Rect crop, double pivotX, double pivotY, double axisDegrees,
                double reachPixels)
            {
                Crop = crop;
                PivotX = pivotX;
                PivotY = pivotY;
                AxisDegrees = axisDegrees;
                ReachPixels = reachPixels;
            }

            // Region of the 128x128 frame that contains the weapon.
            public Int32Rect Crop { get; }
            // Pivot in frame pixels; the sprite rotates around it.
            public double PivotX { get; }
            public double PivotY { get; }
            // Direction the weapon points in the unrotated sprite: -90 = up, 0 = right.
            public double AxisDegrees { get; }
            // Pixels from the pivot to the tip; scaled to the reach of the damage area.
            public double ReachPixels { get; }
        }

        private sealed class WeaponPose
        {
            // A carried staff stands on the ground at the height of the feet (frame pixels).
            private const double FeetY = 62;
            // Tilts of carried weapons: blades rest slightly outwards seen from the front and
            // point forwards-up seen from the side, staffs lean a little forwards.
            private const double FrontBladeTiltRadians = Math.PI * 25 / 180;
            private const double SideBladeTiltRadians = Math.PI * 40 / 180;
            private const double SideStaffTiltRadians = Math.PI * 8 / 180;

            public WeaponAppearance Appearance { get; private set; }
            public string Animation { get; set; }
            public int FrameIndex { get; private set; }
            public double DirectionX { get; private set; }
            public double DirectionY { get; private set; }
            // Pivot offset from the body center in world units.
            public double OffsetX { get; private set; }
            public double OffsetY { get; private set; }
            // Reach the sprite is scaled to; 0 for bows.
            public double Reach { get; private set; }
            // Projectile clip drawn on the string of a bow; null without a nocked arrow.
            public string NockedArrow { get; set; }

            public static WeaponPose Aimed(
                WeaponAppearance appearance, string animation, int frameIndex,
                double directionX, double directionY, Vector pivot, double reach)
            {
                return new WeaponPose
                {
                    Appearance = appearance,
                    Animation = animation,
                    FrameIndex = frameIndex,
                    DirectionX = directionX,
                    DirectionY = directionY,
                    OffsetX = pivot.X,
                    OffsetY = pivot.Y,
                    Reach = reach
                };
            }

            // Grip in the hand, pointing from the hand at the point of the attack direction
            // at the reach; thrust pushes the weapon forward along that line.
            public static WeaponPose FromHand(
                WeaponAppearance appearance, string animation, Vector hand,
                double directionX, double directionY, double reach, double thrust)
            {
                AttackGeometry.NormalizeDirection(
                    directionX, directionY, out double x, out double y);
                Vector target = new Vector(x, y) * reach - hand;
                if (target.Length < 1e-9)
                {
                    target = new Vector(x, y);
                }
                target.Normalize();
                return Aimed(appearance, animation, 0, target.X, target.Y,
                    hand + target * thrust, reach);
            }

            // Carried weapon: lies in the right hand and turns with the facing.
            public static WeaponPose Held(
                WeaponAppearance appearance, PlayerFacing facing, Vector hand,
                double unitsPerFramePixel)
            {
                bool isSideView = facing == PlayerFacing.Left || facing == PlayerFacing.Right;
                // Carried weapons lean towards the side the character faces or holds them on.
                double side = isSideView
                    ? (facing == PlayerFacing.Left ? -1 : 1)
                    : (hand.X < 0 ? -1 : 1);
                double directionX;
                double directionY;
                Vector pivot = hand;
                if (appearance.Kind == WeaponSpriteKind.Staff)
                {
                    double tilt = isSideView ? SideStaffTiltRadians : 0;
                    directionX = side * Math.Sin(tilt);
                    directionY = -Math.Cos(tilt);
                    pivot = new Vector(hand.X, FeetY * unitsPerFramePixel);
                }
                else
                {
                    double angleFromVertical = isSideView
                        ? Math.PI / 2 - SideBladeTiltRadians
                        : FrontBladeTiltRadians;
                    directionX = side * Math.Sin(angleFromVertical);
                    directionY = -Math.Cos(angleFromVertical);
                }

                return Aimed(appearance, appearance.HeldAnimation, 0,
                    directionX, directionY, pivot, appearance.Reach);
            }
        }
    }
}
