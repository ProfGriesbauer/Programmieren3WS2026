//P06.11.2026
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // Holds world dimensions, the player, and movement/collision behavior.
    public sealed class ArenaField : IGameField
    {
        // All world positions and sizes are measured in WPF device-independent units.
        public const double MapWidth = 1800;
        public const double MapHeight = 1200;
        public const double WallThickness = 32;
        public const double MinimumZoom = 0.5;
        public const double MaximumZoom = 2.0;
        private const double ZoomStep = 0.1;
        public const int MaximumEnemyCount = 15;
        public const double EnemySpawnIntervalSeconds = 7;
        public const double MirrorImageMinimumSpawnDistance = 36;
        public const double MirrorImageSpawnRadius = 70;
        // Game time simulated by one framework tick at normal speed. Slow motion keeps the
        // framework timer but simulates less game time per tick (GameTimeScale).
        public const double GameTickDurationSeconds = 0.04;
        public const double PlayerAttackRange = 80;
        public const double PlayerAttackHalfAngleRadians = Math.PI / 6;
        public const int PlayerAttackDamage = 10;
        public const double WeaponWheelSlowMotionScale = 0.15;
        // Godmode only: overall game speed, changed with the GameSpeed key + mouse wheel.
        public const double MinimumGameSpeed = 0.1;
        public const double MaximumGameSpeed = 3;
        public const double GameSpeedStep = 0.1;
        private const double EnemySpawnVisibilityMargin = 50;
        private const int EnemySpawnPositionAttemptLimit = 10000;
        private readonly List<PursuerEnemy> _enemies = new List<PursuerEnemy>();
        private readonly List<MirrorImage> _mirrorImages = new List<MirrorImage>();
        private double _enemySpawnElapsedSeconds;
        private double _slowMotionRemainingSeconds;
        private double _slowMotionTimeScale = 1;
        private bool _isBulletTime;
        private int _spinSwingsRaised;
        private double _motionReferenceX;
        private double _motionReferenceY;
        private ArenaMapDefinition _map;
        // Ctrl toggles sprint on release, unless it was used for Ctrl + mouse-wheel zoom.
        private bool _sprintKeyDown;
        private bool _sprintKeyUsedForZoom;
        private bool _isGameStarted;
        private readonly ArenaCombatSystem _combatSystem = new ArenaCombatSystem();
        private Weapon _selectedPlayerWeapon = WeaponCatalog.Sword;
        private readonly ArenaProgression _progression = new ArenaProgression();
        private readonly ArenaKeyBindings _keyBindings = ArenaKeyBindings.LoadDefault();

        // ---- save games, deaths and the high score list ----------------------------------
        // The running game saves itself automatically into its slot: when pausing or going
        // to a menu, when the program closes and every AutosaveIntervalSeconds of play.
        public const double AutosaveIntervalSeconds = 30;
        // Dying costs points; the player respawns in the middle, protected for a moment.
        public const int DeathSkillPointPenalty = 3;
        public const int DeathWeaponPointPenalty = 5;
        public const double RespawnProtectionSeconds = 2;
        public const double RespawnClearRadius = 250;
        private double _autosaveElapsedSeconds;
        private double _respawnProtectionRemaining;
        // Set by ArenaRules for the real game; tests leave them empty and nothing is written.
        public ArenaSaveGameRepository SaveGames { get; set; }
        public ArenaScoreboard Scoreboard { get; set; }
        // Slot the run is saved in; null until a free slot is found.
        public int? ActiveSaveSlot { get; private set; }
        public string RunId { get; private set; } = NewRunId();
        public int Deaths { get; private set; }
        // High score of the run: kills minus a penalty per death.
        public int TotalScore { get { return ArenaScoreboard.GetScore(Score, Deaths); } }
        public bool HasRespawnProtection { get { return _respawnProtectionRemaining > 0; } }
        // True when the run cannot be saved because all slots are taken by other runs.
        public bool IsRunUnsaved
        {
            get
            {
                return SaveGames != null && HasActiveGame && !ActiveSaveSlot.HasValue
                    && !SaveGames.FindFreeSlot().HasValue;
            }
        }

        // The field owns the current player instance.
        public PlayerCharacter Player { get; private set; }
        // Gameplay events for presentation code such as sounds. Player events follow the
        // current player, including after restarts replace it. A lethal hit raises both the
        // damage and the death event.
        public event Action<int> PlayerDamaged;
        public event Action PlayerDied;
        public event Action<Entity, int> NonPlayerEntityDamaged;
        // Any entity (player or enemy) lost or regained hit points; used for particles.
        // EntityHealed passes the entity the life was stolen from (Leach), else null.
        public event Action<Entity, int> EntityDamaged;
        public event Action<Entity, int, Entity> EntityHealed;
        // A movement status (stun, freeze, slow) was applied to the player or an enemy.
        public event Action<Entity, string> EntityStatusApplied;
        // The player released a healing pulse (also at full health).
        public event Action PlayerHealingPulse;
        // A lightning bolt jumped from one entity to another (Kettenblitz).
        public event Action<Entity, Entity> LightningChained;
        // The player released a Power Blast.
        public event Action PlayerPowerBlast;
        public event Action<Entity> NonPlayerEntityDied;
        // Raised for each sword slash and once per revolution of the sword spin.
        public event Action PlayerSwordSwung;
        // Raised when a hit drops the living player to LowHealthFraction or below.
        public event Action PlayerHealthLow;
        // Raised when the walking or sprinting direction has turned by TurnAngleDegrees or more.
        public event Action PlayerTurned;
        // Raised for each footstep while the player walks (not while sprinting): at once when
        // walking starts, then every FootstepIntervalSeconds of the player's game time. The
        // walk clips take 0.8 s with two foot contacts, so steps match the animation.
        public event Action PlayerFootstep;
        public const double FootstepIntervalSeconds = 0.4;
        private double _footstepTimerSeconds;
        public const double LowHealthFraction = 0.25;
        public const double TurnAngleDegrees = 90;
        // Sprint mode, switched by tapping Ctrl; stays on until tapped again or a new run.
        public bool IsSprintEnabled { get; private set; }
        // True while the player actually moves, walking or sprinting.
        public bool IsPlayerInMotion { get; private set; }
        public bool IsPlayerSprinting { get { return IsPlayerInMotion && IsSprintEnabled; } }
        public IReadOnlyList<PursuerEnemy> Enemies { get { return _enemies; } }
        public IReadOnlyList<MirrorImage> MirrorImages { get { return _mirrorImages; } }
        public double WorldWidth { get { return _map.Width; } }
        public double WorldHeight { get { return _map.Height; } }
        public BitmapSource MapImage { get { return _map.Image; } }
        public string MapFileName { get { return _map.FileName; } }
        public Weapon SelectedPlayerWeapon
        {
            get { return Player == null ? _selectedPlayerWeapon : Player.EquippedWeapon; }
        }
        public ArenaProgression Progression { get { return _progression; } }
        public ArenaKeyBindings KeyBindings { get { return _keyBindings; } }
        public int ActiveAttackCount { get { return _combatSystem.ActiveAttackCount; } }
        public IReadOnlyList<AttackVisualState> ActiveAttackVisuals
        {
            get { return _combatSystem.GetActiveVisuals(); }
        }
        // Fire trails and strike traces lying on the ground.
        public IReadOnlyList<ArenaGroundEffect> GroundEffects
        {
            get { return _combatSystem.GroundEffects; }
        }
        public bool IsGameStarted
        {
            get { return _isGameStarted; }
            private set
            {
                _isGameStarted = value;
                if (!value)
                {
                    // Pausing stops all motion; the sprint mode itself is kept.
                    IsPlayerInMotion = false;
                }
            }
        }
        public bool IsWeaponWheelOpen { get; private set; }
        public int WeaponWheelSelectionIndex
        {
            get
            {
                return ArenaWeaponWheel.GetSelectionIndex(
                    AimDirectionX, AimDirectionY);
            }
        }
        // Fraction of GameTickDurationSeconds simulated per tick. Every time-based system
        // (movement, attacks, projectiles, cooldowns, status effects, spawning, enemy AI and
        // animations) advances by this scaled tick, so slow motion affects all of them alike.
        public double GameTimeScale
        {
            get
            {
                // The strongest slowdown applies; the overall game speed multiplies it.
                double slowdown = 1;
                if (IsWeaponWheelOpen)
                {
                    slowdown = Math.Min(slowdown, WeaponWheelSlowMotionScale);
                }
                if (IsSlowMotionActive)
                {
                    slowdown = Math.Min(slowdown, _slowMotionTimeScale);
                }
                return GameSpeed * slowdown;
            }
        }
        public bool IsSlowMotionActive { get { return _slowMotionRemainingSeconds > 0; } }
        // Remaining slow motion in unscaled ticks (real seconds at the regular tick rate).
        public double SlowMotionRemainingSeconds { get { return _slowMotionRemainingSeconds; } }
        public double SlowMotionTimeScale
        {
            get { return IsSlowMotionActive ? _slowMotionTimeScale : 1; }
        }
        public bool IsBulletTimeActive { get { return IsSlowMotionActive && _isBulletTime; } }
        // Multiplies every time scale, including the slow-motion countdown. Always 1 outside
        // godmode.
        public double GameSpeed { get; private set; } = 1;
        // Time scale of the player: equal to GameTimeScale, except during Bullet Time where
        // only the weapon wheel can slow the player down.
        public double PlayerTimeScale
        {
            get
            {
                return IsBulletTimeActive
                    ? GameSpeed * (IsWeaponWheelOpen ? WeaponWheelSlowMotionScale : 1)
                    : GameTimeScale;
            }
        }
        public bool HasActiveGame { get; private set; }
        public bool IsGameOver { get { return HasActiveGame && !Player.IsAlive; } }
        public bool IsGodMode { get { return _progression.IsGodMode; } }
        public bool ShowDamageRanges { get; private set; }
        public bool ShowDebugHud { get; private set; }
        public int Score { get; private set; }
        public double AimDirectionX { get; private set; }
        public double AimDirectionY { get; private set; } = 1;
        // Distance from the player to the aimed point (the mouse cursor in world units).
        // Projectiles from the hand fly to that point; without a cursor the point lies
        // DefaultAimDistance ahead.
        public double AimDistance { get; private set; } = DefaultAimDistance;
        public const double DefaultAimDistance = 150;
        public double ZoomLevel { get; private set; } = 1.0;
        public static Rect MovementBounds
        {
            get
            {
                return new Rect(WallThickness, WallThickness,
                    MapWidth - 2 * WallThickness, MapHeight - 2 * WallThickness);
            }
        }
        public Rect MovementArea
        {
            get
            {
                return new Rect(WallThickness, WallThickness,
                    WorldWidth - 2 * WallThickness,
                    WorldHeight - 2 * WallThickness);
            }
        }

        public ArenaField()
        {
            _map = ArenaMapAssets.LoadSelectedMap();
            _combatSystem.EntityDefeated += OnEntityDefeated;
            _combatSystem.LightningChained += (from, to) => LightningChained?.Invoke(from, to);
            // Create the player in the middle when a new field is constructed.
            ResetPlayer();
        }

        public void SelectMap(string fileName)
        {
            ArenaMapDefinition map = ArenaMapAssets.LoadMap(fileName);
            ArenaMapAssets.SetDefaultMap(map.FileName);
            bool mapChanged = !String.Equals(
                _map.FileName, map.FileName, StringComparison.OrdinalIgnoreCase);
            _map = map;
            if (HasActiveGame)
            {
                RestartRunAtMenu();
            }
            else if (mapChanged)
            {
                ResetPlayer();
            }
        }

        public void SelectSkillTree(string name)
        {
            ArenaSkillTreeDocument skillTree =
                ArenaSkillTreeRepository.LoadTreeByName(name);
            ArenaSkillTreeRepository.SelectTree(name);
            _progression.SetSkillTree(skillTree);
            if (HasActiveGame)
            {
                RestartRunAtMenu();
            }
        }

        private void RestartRunAtMenu()
        {
            RestartGame();
            ReturnToStartMenu();
        }

        public void ResetPlayer()
        {
            // Resetting starts the player at the center of the arena.
            _combatSystem.Clear();
            if (Player != null)
            {
                Player.Damaged -= OnPlayerDamaged;
                Player.Damaged -= OnEntityDamaged;
                Player.Healed -= OnEntityHealed;
                Player.StatusApplied -= OnEntityStatusApplied;
            }
            Player = new PlayerCharacter(WorldWidth / 2, WorldHeight / 2);
            Player.Damaged += OnPlayerDamaged;
            Player.Damaged += OnEntityDamaged;
            Player.Healed += OnEntityHealed;
            Player.StatusApplied += OnEntityStatusApplied;
            Player.EquippedWeapon = _selectedPlayerWeapon;
            Player.IsInvulnerable = IsGodMode;
            _enemies.Clear();
            _mirrorImages.Clear();
            _enemySpawnElapsedSeconds = 0;
            _spinSwingsRaised = 0;
            EndSlowMotion();
            IsPlayerInMotion = false;
            IsSprintEnabled = false;
            Point spawnPosition = GetRandomEnemySpawnPosition(Player);
            AddEnemy(new PursuerEnemy(
                spawnPosition.X, spawnPosition.Y, Player.MaxSpeed));
        }

        private void AddEnemy(PursuerEnemy enemy)
        {
            enemy.Damaged += OnNonPlayerEntityDamaged;
            enemy.Damaged += OnEntityDamaged;
            enemy.Healed += OnEntityHealed;
            enemy.StatusApplied += OnEntityStatusApplied;
            _enemies.Add(enemy);
        }

        private void OnEntityDamaged(Entity entity, int lostHitPoints)
        {
            EntityDamaged?.Invoke(entity, lostHitPoints);
        }

        private void OnEntityHealed(Entity entity, int healedHitPoints, Entity from)
        {
            EntityHealed?.Invoke(entity, healedHitPoints, from);
        }

        private void OnEntityStatusApplied(Entity entity, string statusId)
        {
            EntityStatusApplied?.Invoke(entity, statusId);
        }

        private void AddMirrorImage(MirrorImage mirrorImage)
        {
            mirrorImage.Damaged += OnNonPlayerEntityDamaged;
            mirrorImage.Damaged += OnEntityDamaged;
            mirrorImage.Healed += OnEntityHealed;
            mirrorImage.StatusApplied += OnEntityStatusApplied;
            _mirrorImages.Add(mirrorImage);
        }

        private void OnPlayerDamaged(Entity player, int lostHitPoints)
        {
            PlayerDamaged?.Invoke(lostHitPoints);
            if (!player.IsAlive)
            {
                IsPlayerInMotion = false;
                Deaths++;
                PlayerDied?.Invoke();
                // The death is saved at once, so it cannot be undone by quitting.
                SaveNow();
                return;
            }

            double lowHealthLimit = player.MaxHitPoints * LowHealthFraction;
            if (player.CurrentHitPoints <= lowHealthLimit
                && player.CurrentHitPoints + lostHitPoints > lowHealthLimit)
            {
                PlayerHealthLow?.Invoke();
            }
        }

        public void ToggleSprint()
        {
            IsSprintEnabled = !IsSprintEnabled;
        }

        // Tracks walking and sprinting for sounds. A turn is reported once the movement
        // direction has rotated by TurnAngleDegrees from the direction at the start of the
        // movement or the last turn, so gradual curves count as well as sharp ones.
        public void UpdatePlayerMotion(bool isMoving, double directionX, double directionY)
        {
            double length = Math.Sqrt(directionX * directionX + directionY * directionY);
            if (!isMoving || length == 0)
            {
                IsPlayerInMotion = false;
                return;
            }

            directionX /= length;
            directionY /= length;
            if (!IsPlayerInMotion)
            {
                IsPlayerInMotion = true;
                _motionReferenceX = directionX;
                _motionReferenceY = directionY;
                return;
            }

            double turnLimit = Math.Cos(TurnAngleDegrees * Math.PI / 180);
            if (directionX * _motionReferenceX + directionY * _motionReferenceY
                <= turnLimit + 1e-9)
            {
                _motionReferenceX = directionX;
                _motionReferenceY = directionY;
                PlayerTurned?.Invoke();
            }
        }

        public void AdvanceFootsteps(double deltaSeconds)
        {
            if (deltaSeconds < 0 || double.IsNaN(deltaSeconds))
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }
            if (!IsPlayerInMotion || IsSprintEnabled)
            {
                // The next walk starts with a step right away.
                _footstepTimerSeconds = 0;
                return;
            }

            if (_footstepTimerSeconds <= 1e-9)
            {
                PlayerFootstep?.Invoke();
                _footstepTimerSeconds += FootstepIntervalSeconds;
            }
            _footstepTimerSeconds -= deltaSeconds;
        }

        private void OnNonPlayerEntityDamaged(Entity entity, int lostHitPoints)
        {
            NonPlayerEntityDamaged?.Invoke(entity, lostHitPoints);
            if (!entity.IsAlive)
            {
                if (entity is MirrorImage mirrorImage)
                {
                    _mirrorImages.Remove(mirrorImage);
                }
                NonPlayerEntityDied?.Invoke(entity);
            }
        }

        public void SelectPlayerWeapon(Weapon weapon)
        {
            if (weapon == null)
            {
                throw new ArgumentNullException(nameof(weapon));
            }

            if (weapon.Id == WeaponCatalog.Sword.Id)
            {
                _selectedPlayerWeapon = WeaponCatalog.Sword;
            }
            else if (weapon.Id == WeaponCatalog.MagicWand.Id)
            {
                _selectedPlayerWeapon = WeaponCatalog.MagicWand;
            }
            else if (weapon.Id == WeaponCatalog.Bow.Id)
            {
                _selectedPlayerWeapon = WeaponCatalog.Bow;
            }
            else
            {
                throw new ArgumentException(
                    "The selected weapon is not available as player equipment.",
                    nameof(weapon));
            }

            RefreshPlayerWeapon();
        }

        public bool TryUpgradeWeapon(string upgradeId, UpgradeCurrency currency)
        {
            bool upgraded;
            if (_selectedPlayerWeapon.Id == WeaponCatalog.Sword.Id)
            {
                upgraded = _progression.TryUpgradeSword(upgradeId, currency);
            }
            else if (_selectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id)
            {
                upgraded = _progression.TryUpgradeMagicWand(upgradeId, currency);
            }
            else
            {
                upgraded = _progression.TryUpgradeBow(upgradeId, currency);
            }
            if (!upgraded)
            {
                return false;
            }

            RefreshPlayerWeapon();
            return true;
        }

        public bool CanUpgradeWeapon(
            string upgradeId, out int cost,
            out int weaponPointsSpent, out int skillPointsSpent)
        {
            bool canUpgrade;
            if (_selectedPlayerWeapon.Id == WeaponCatalog.Sword.Id)
            {
                canUpgrade = _progression.CanUpgradeSword(
                    upgradeId, UpgradeCurrency.Weapon, out cost, true);
            }
            else if (_selectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id)
            {
                canUpgrade = _progression.CanUpgradeMagicWand(
                    upgradeId, UpgradeCurrency.Weapon, out cost, true);
            }
            else
            {
                canUpgrade = _progression.CanUpgradeBow(
                    upgradeId, UpgradeCurrency.Weapon, out cost, true);
            }

            _progression.GetWeaponUpgradePayment(
                _selectedPlayerWeapon.Id, cost,
                out weaponPointsSpent, out skillPointsSpent);
            return canUpgrade;
        }

        public bool TryUpgradeWeaponWithSkillPoints(string upgradeId)
        {
            bool upgraded;
            if (_selectedPlayerWeapon.Id == WeaponCatalog.Sword.Id)
            {
                upgraded = _progression.TryUpgradeSword(
                    upgradeId, UpgradeCurrency.Weapon, true);
            }
            else if (_selectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id)
            {
                upgraded = _progression.TryUpgradeMagicWand(
                    upgradeId, UpgradeCurrency.Weapon, true);
            }
            else
            {
                upgraded = _progression.TryUpgradeBow(
                    upgradeId, UpgradeCurrency.Weapon, true);
            }
            if (!upgraded)
            {
                return false;
            }

            RefreshPlayerWeapon();
            return true;
        }

        public bool TryUpgradePlayerStat(string statId)
        {
            if (!_progression.TryUpgradePlayerStat(statId))
            {
                return false;
            }

            ApplyPlayerStats();
            return true;
        }

        private void ApplyPlayerStats()
        {
            Player.MaxHitPoints = PlayerCharacter.BaseMaxHitPoints
                + _progression.GetSkillRank(ArenaProgression.MaximumHealth)
                    * ArenaProgression.HealthIncreasePerRank;
            Player.MaxSpeed = PlayerCharacter.BaseMaxSpeed
                + _progression.GetSkillRank(ArenaProgression.MaximumSpeed)
                    * ArenaProgression.SpeedIncreasePerRank;
            Player.currentSpeed = Player.MaxSpeed;
        }

        public void ToggleGodMode()
        {
            _progression.SetGodMode(!IsGodMode);
            Player.IsInvulnerable = IsGodMode;
            if (!IsGodMode)
            {
                GameSpeed = 1;
            }
        }

        // Godmode only: buys every skill and weapon upgrade at once and applies them.
        public bool PurchaseAllUpgrades()
        {
            if (!IsGodMode)
            {
                return false;
            }

            _progression.PurchaseAllUpgrades();
            ApplyPlayerStats();
            RefreshPlayerWeapon();
            return true;
        }

        // Godmode only: changes the overall game speed by whole steps.
        public bool ChangeGameSpeed(int steps)
        {
            if (!IsGodMode || steps == 0)
            {
                return false;
            }

            GameSpeed = Math.Round(Math.Max(MinimumGameSpeed, Math.Min(
                MaximumGameSpeed, GameSpeed + steps * GameSpeedStep)), 2);
            return true;
        }

        // Mouse wheel while the GameSpeed key is held; returns false so other wheel uses
        // (zoom) still work when the key is up or godmode is off.
        public bool HandleGameSpeedWheel(int delta)
        {
            if (delta == 0 || !IsGodMode
                || !_keyBindings.IsDown(ArenaAction.GameSpeed))
            {
                return false;
            }
            return ChangeGameSpeed(Math.Sign(delta));
        }

        private void RefreshPlayerWeapon()
        {
            if (_selectedPlayerWeapon.Id == WeaponCatalog.Sword.Id)
            {
                Player.EquippedWeapon = SwordUpgradeCatalog.CreateSword(_progression);
            }
            else if (_selectedPlayerWeapon.Id == WeaponCatalog.MagicWand.Id)
            {
                Player.EquippedWeapon = MagicWandUpgradeCatalog.CreateMagicWand(
                    _progression);
            }
            else
            {
                Player.EquippedWeapon = BowUpgradeCatalog.CreateBow(_progression);
            }
        }

        public void ResetGame()
        {
            ResetPlayer();
            ZoomLevel = 1.0;
            ShowDamageRanges = false;
            ShowDebugHud = false;
            Score = 0;
            _progression.Reset();
            Player.IsInvulnerable = false;
            Deaths = 0;
            RunId = NewRunId();
            _autosaveElapsedSeconds = 0;
            _respawnProtectionRemaining = 0;
            AimDirectionX = 0;
            AimDirectionY = 1;
            IsGameStarted = false;
            IsWeaponWheelOpen = false;
            HasActiveGame = false;
            GameSpeed = 1;
        }

        public void StartNewGame()
        {
            ResetGame();
            StartGame();
        }

        public void RestartGame()
        {
            double zoomLevel = ZoomLevel;
            bool showDamageRanges = ShowDamageRanges;
            bool showDebugHud = ShowDebugHud;

            ResetGame();
            ZoomLevel = zoomLevel;
            ShowDamageRanges = showDamageRanges;
            ShowDebugHud = showDebugHud;
            StartGame();
        }

        public void StartGame()
        {
            IsWeaponWheelOpen = false;
            IsGameStarted = true;
            HasActiveGame = true;
        }

        public void ToggleDamageRanges()
        {
            ShowDamageRanges = !ShowDamageRanges;
        }

        public void ToggleDebugHud()
        {
            ShowDebugHud = !ShowDebugHud;
        }

        // Aims at a point given as world offset from the player, e.g. the mouse cursor.
        public void SetAimTarget(double offsetX, double offsetY)
        {
            double length = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            if (length == 0)
            {
                return;
            }

            SetAimDirection(offsetX, offsetY);
            AimDistance = length;
        }

        // Direction a projectile of the weapon leaves the hand in, so it flies through the
        // aimed point although it starts beside the body center. Weapons without a sprite and
        // non-projectile attacks use the plain aim direction.
        public Vector GetProjectileAimDirection(Weapon weapon)
        {
            Vector aim = new Vector(AimDirectionX, AimDirectionY);
            if (Player == null || weapon == null || weapon.Appearance == null
                || !(weapon.AttackPattern is ProjectileAttackPattern))
            {
                return aim;
            }

            Vector hand = Player.GetRightHandOffset(
                PlayerCharacter.GetFacingForDirection(aim.X, aim.Y));
            Vector toTarget = aim * AimDistance - hand;
            // A cursor on top of the hand gives no usable direction.
            if (toTarget.Length < Player.Size / 2)
            {
                return aim;
            }
            toTarget.Normalize();
            return toTarget;
        }

        public void SetAimDirection(double aimX, double aimY)
        {
            double length = Math.Sqrt(aimX * aimX + aimY * aimY);
            if (length == 0)
            {
                return;
            }

            AimDirectionX = aimX / length;
            AimDirectionY = aimY / length;
        }

        public bool ContinueGame()
        {
            if (!HasActiveGame || !Player.IsAlive)
            {
                return false;
            }

            StartGame();
            return true;
        }

        public void ReturnToStartMenu()
        {
            IsWeaponWheelOpen = false;
            IsGameStarted = false;
            SaveNow();
        }

        // ---- save games -----------------------------------------------------------------

        private static string NewRunId()
        {
            return Guid.NewGuid().ToString("N");
        }

        // Saves the running game into its slot, taking the first free slot when it has none
        // yet. Returns false when nothing could be saved (no repository, no run, no free slot).
        public bool SaveNow()
        {
            if (SaveGames == null || !HasActiveGame)
            {
                return false;
            }
            if (!ActiveSaveSlot.HasValue)
            {
                ActiveSaveSlot = SaveGames.FindFreeSlot();
                if (!ActiveSaveSlot.HasValue)
                {
                    return false;
                }
            }

            _autosaveElapsedSeconds = 0;
            RecordScore();
            return SaveGames.Save(ActiveSaveSlot.Value, CreateSaveGame());
        }

        public ArenaSaveGame CreateSaveGame()
        {
            ArenaSaveGame game = new ArenaSaveGame
            {
                RunId = RunId,
                SavedAt = DateTime.Now,
                MapFileName = MapFileName,
                Score = Score,
                Deaths = Deaths,
                SelectedWeaponId = _selectedPlayerWeapon.Id,
                AnimationProfileName = Player.AnimationProfileName,
                ZoomLevel = ZoomLevel,
                ShowDamageRanges = ShowDamageRanges,
                ShowDebugHud = ShowDebugHud,
                GameSpeed = GameSpeed,
                EnemySpawnElapsedSeconds = _enemySpawnElapsedSeconds,
                AimDirectionX = AimDirectionX,
                AimDirectionY = AimDirectionY,
                Progression = _progression.Export(),
                Player = SaveEntity(Player)
            };
            foreach (PursuerEnemy enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    game.Enemies.Add(SaveEntity(enemy));
                }
            }
            return game;
        }

        // Loads a slot and continues it at once; a run saved after death shows game over.
        public bool LoadSlot(int slot)
        {
            ArenaSaveGame game = SaveGames == null ? null : SaveGames.Load(slot);
            if (game == null)
            {
                return false;
            }
            LoadSaveGame(game, slot);
            return true;
        }

        public void LoadSaveGame(ArenaSaveGame game, int? slot)
        {
            if (game == null)
            {
                throw new ArgumentNullException(nameof(game));
            }

            if (!string.IsNullOrEmpty(game.MapFileName)
                && !String.Equals(game.MapFileName, MapFileName, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    _map = ArenaMapAssets.LoadMap(game.MapFileName);
                }
                catch (Exception exception) when (exception is System.IO.IOException
                    || exception is ArgumentException
                    || exception is InvalidOperationException
                    || exception is NotSupportedException
                    || exception is UnauthorizedAccessException)
                {
                    // A missing map keeps the current one.
                }
            }

            _progression.Import(game.Progression);
            _selectedPlayerWeapon = GetBaseWeapon(game.SelectedWeaponId);
            ResetPlayer();
            _enemies.Clear();
            Player.AnimationProfileName = string.IsNullOrEmpty(game.AnimationProfileName)
                ? EntityHitboxCatalog.BoxingKnight
                : game.AnimationProfileName;
            ApplyPlayerStats();
            RefreshPlayerWeapon();
            RestoreEntity(Player, game.Player);
            if (game.Enemies != null)
            {
                foreach (ArenaEntitySave saved in game.Enemies)
                {
                    PursuerEnemy enemy = new PursuerEnemy(saved.X, saved.Y, Player.MaxSpeed);
                    AddEnemy(enemy);
                    RestoreEntity(enemy, saved);
                }
            }

            Score = game.Score;
            Deaths = game.Deaths;
            RunId = string.IsNullOrEmpty(game.RunId) ? NewRunId() : game.RunId;
            ZoomLevel = Math.Max(MinimumZoom, Math.Min(MaximumZoom, game.ZoomLevel));
            ShowDamageRanges = game.ShowDamageRanges;
            ShowDebugHud = game.ShowDebugHud;
            GameSpeed = IsGodMode
                ? Math.Max(MinimumGameSpeed, Math.Min(MaximumGameSpeed, game.GameSpeed))
                : 1;
            _enemySpawnElapsedSeconds = game.EnemySpawnElapsedSeconds;
            SetAimDirection(game.AimDirectionX, game.AimDirectionY);
            Player.IsInvulnerable = IsGodMode;
            _respawnProtectionRemaining = 0;
            _autosaveElapsedSeconds = 0;
            ActiveSaveSlot = slot;
            HasActiveGame = true;
            IsWeaponWheelOpen = false;
            IsGameStarted = Player.IsAlive;
        }

        // Program start: continue the run saved last, otherwise begin a new one.
        public void ContinueLatestOrStartNew()
        {
            int? latest = SaveGames == null ? null : SaveGames.FindMostRecentSlot();
            if (latest.HasValue && LoadSlot(latest.Value))
            {
                return;
            }
            ActiveSaveSlot = null;
            StartNewGame();
        }

        // Starts a new run in the given (usually empty) slot.
        public void StartNewGameInSlot(int slot)
        {
            StartNewGame();
            ActiveSaveSlot = slot;
            SaveNow();
        }

        // Deletes a save; deleting the running game's slot leaves the run without a slot
        // until a free one is found at the next save.
        public void DeleteSaveSlot(int slot)
        {
            if (SaveGames == null)
            {
                return;
            }
            SaveGames.Delete(slot);
            if (ActiveSaveSlot == slot)
            {
                ActiveSaveSlot = null;
            }
        }

        // Respawn after death: the penalty is paid, the player stands up in the middle with
        // full hit points and is invulnerable for a moment; enemies near the middle are moved
        // away so the player is not hit at once.
        public bool Respawn()
        {
            if (!HasActiveGame || Player.IsAlive)
            {
                return false;
            }

            _progression.ApplyDeathPenalty(
                _selectedPlayerWeapon.Id, DeathSkillPointPenalty, DeathWeaponPointPenalty);
            _combatSystem.Clear();
            Player.Revive();
            ApplyPlayerStats();
            Player.RestoreHitPoints(Player.MaxHitPoints);
            Player.SetPosition(WorldWidth / 2, WorldHeight / 2);
            RefreshPlayerWeapon();
            foreach (PursuerEnemy enemy in _enemies)
            {
                double offsetX = enemy.PositionX - Player.PositionX;
                double offsetY = enemy.PositionY - Player.PositionY;
                if (enemy.IsAlive
                    && offsetX * offsetX + offsetY * offsetY
                        < RespawnClearRadius * RespawnClearRadius)
                {
                    Point away = GetRandomEnemySpawnPosition(Player);
                    enemy.SetPosition(away.X, away.Y);
                }
            }
            Player.IsInvulnerable = true;
            _respawnProtectionRemaining = RespawnProtectionSeconds;
            IsPlayerInMotion = false;
            StartGame();
            SaveNow();
            return true;
        }

        private void RecordScore()
        {
            // Godmode runs do not count for the high score list.
            if (Scoreboard == null || !HasActiveGame || IsGodMode)
            {
                return;
            }
            Scoreboard.Record(RunId,
                ActiveSaveSlot.HasValue
                    ? ArenaSaveGameRepository.GetSlotName(ActiveSaveSlot.Value)
                    : "Ohne Slot",
                Score, Deaths, DateTime.Now);
        }

        private static Weapon GetBaseWeapon(string weaponId)
        {
            if (weaponId == WeaponCatalog.MagicWand.Id)
            {
                return WeaponCatalog.MagicWand;
            }
            if (weaponId == WeaponCatalog.Bow.Id)
            {
                return WeaponCatalog.Bow;
            }
            return WeaponCatalog.Sword;
        }

        private ArenaEntitySave SaveEntity(Entity entity)
        {
            ArenaEntitySave saved = new ArenaEntitySave
            {
                X = entity.PositionX,
                Y = entity.PositionY,
                HitPoints = entity.IsAlive ? entity.CurrentHitPoints : 0,
                Facing = entity.Facing.ToString(),
                WeaknessMultiplier = entity.WeaknessDamageMultiplier,
                WeaknessSeconds = entity.WeaknessRemainingSeconds
            };
            foreach (ActiveStatusEffect status in entity.ActiveStatusEffects)
            {
                saved.Statuses.Add(new ArenaStatusSave
                {
                    Id = status.Id,
                    Name = status.Name,
                    SpeedMultiplier = status.SpeedMultiplier,
                    RemainingSeconds = status.RemainingSeconds
                });
            }
            foreach (ActiveDamageOverTimeEffect effect in entity.ActiveDamageOverTimeEffects)
            {
                saved.DamageOverTime.Add(new ArenaDamageOverTimeSave
                {
                    Id = effect.Id,
                    Name = effect.Name,
                    DamagePerSecond = effect.DamagePerSecond,
                    RemainingSeconds = effect.RemainingSeconds,
                    SpreadRadius = effect.SpreadRadius,
                    FromPlayer = ReferenceEquals(effect.Source, Player)
                });
            }
            return saved;
        }

        private void RestoreEntity(Entity entity, ArenaEntitySave saved)
        {
            if (saved == null)
            {
                return;
            }
            entity.SetPosition(saved.X, saved.Y);
            if (Enum.TryParse(saved.Facing, out PlayerFacing facing))
            {
                entity.Facing = facing;
            }
            entity.RestoreHitPoints(saved.HitPoints);
            if (!entity.IsAlive)
            {
                return;
            }
            foreach (ArenaStatusSave status in saved.Statuses ?? new List<ArenaStatusSave>())
            {
                if (!string.IsNullOrWhiteSpace(status.Id) && status.RemainingSeconds > 0)
                {
                    entity.ApplyMovementStatus(
                        status.Id, string.IsNullOrWhiteSpace(status.Name) ? status.Id : status.Name,
                        Math.Max(0, Math.Min(1, status.SpeedMultiplier)), status.RemainingSeconds);
                }
            }
            foreach (ArenaDamageOverTimeSave effect
                in saved.DamageOverTime ?? new List<ArenaDamageOverTimeSave>())
            {
                if (!string.IsNullOrWhiteSpace(effect.Id) && effect.RemainingSeconds > 0
                    && effect.DamagePerSecond > 0)
                {
                    entity.ApplyDamageOverTime(
                        effect.Id, string.IsNullOrWhiteSpace(effect.Name) ? effect.Id : effect.Name,
                        effect.DamagePerSecond, effect.RemainingSeconds,
                        Math.Max(0, effect.SpreadRadius),
                        effect.FromPlayer ? Player : null);
                }
            }
            if (saved.WeaknessSeconds > 0)
            {
                entity.ApplyWeakness(
                    Math.Max(0, Math.Min(1, saved.WeaknessMultiplier)), saved.WeaknessSeconds);
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            // The arena field has a matching painter that knows its layout.
            return painter is PainterG9;
        }

        // One framework tick: everything advances by the scaled game time, while the slow
        // motion itself counts down in unscaled ticks so it cannot stretch its own duration.
        public void Tick()
        {
            if (!IsGameStarted)
            {
                return;
            }

            double worldScale = GameTimeScale;
            double playerScale = PlayerTimeScale;
            UpdateRespawnProtection(GameTickDurationSeconds * playerScale);
            UpdatePlayerPosition(playerScale);
            UpdateMirrorImagePositions(worldScale);
            UpdateEnemyPositions(worldScale);
            UpdateCombat(
                GameTickDurationSeconds * worldScale,
                GameTickDurationSeconds * playerScale);
            UpdateSlowMotion(GameTickDurationSeconds * GameSpeed);

            // Real playing time, independent of slow motion.
            _autosaveElapsedSeconds += GameTickDurationSeconds;
            if (_autosaveElapsedSeconds >= AutosaveIntervalSeconds)
            {
                SaveNow();
            }
        }

        private void UpdateRespawnProtection(double deltaSeconds)
        {
            if (_respawnProtectionRemaining <= 0)
            {
                return;
            }
            _respawnProtectionRemaining -= deltaSeconds;
            if (_respawnProtectionRemaining <= 0)
            {
                _respawnProtectionRemaining = 0;
                Player.IsInvulnerable = IsGodMode;
            }
        }

        public void ActivateSlowMotion(
            double timeScale, double durationSeconds, bool keepsPlayerAtNormalSpeed = false)
        {
            if (timeScale <= 0 || timeScale >= 1)
            {
                throw new ArgumentOutOfRangeException(nameof(timeScale));
            }
            if (durationSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            _slowMotionTimeScale = timeScale;
            _slowMotionRemainingSeconds = durationSeconds;
            _isBulletTime = keepsPlayerAtNormalSpeed;
        }

        public void UpdateSlowMotion(double unscaledDeltaSeconds)
        {
            if (unscaledDeltaSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(unscaledDeltaSeconds));
            }
            if (!IsSlowMotionActive)
            {
                return;
            }

            _slowMotionRemainingSeconds -= unscaledDeltaSeconds;
            if (_slowMotionRemainingSeconds <= 1e-9)
            {
                EndSlowMotion();
            }
        }

        private void EndSlowMotion()
        {
            _slowMotionRemainingSeconds = 0;
            _slowMotionTimeScale = 1;
            _isBulletTime = false;
        }

        public void UpdatePlayerPosition(double speedScale = 1)
        {
            if (!IsGameStarted)
            {
                return;
            }

            IReadOnlyList<Entity> entities = GetCombatEntities();
            List<EntityPositionSnapshot> previousPositions =
                CaptureEntityPositions(entities);
            double previousX = Player.PositionX;
            double previousY = Player.PositionY;
            double directionX = 0;
            double directionY = 0;

            if (_keyBindings.IsDown(ArenaAction.MoveUp) || Keyboard.IsKeyDown(Key.Up)) directionY -= 1;
            if (_keyBindings.IsDown(ArenaAction.MoveDown) || Keyboard.IsKeyDown(Key.Down)) directionY += 1;
            if (_keyBindings.IsDown(ArenaAction.MoveLeft) || Keyboard.IsKeyDown(Key.Left)) directionX -= 1;
            if (_keyBindings.IsDown(ArenaAction.MoveRight) || Keyboard.IsKeyDown(Key.Right)) directionX += 1;

            Rect movementBounds = MovementArea;
            double sprintScale = IsSprintEnabled ? 1.5 : 1;
            Player.Move(directionX, directionY, movementBounds,
                speedScale * sprintScale);
            UpdatePlayerMotion(Player.IsMoving, directionX, directionY);
            AdvanceFootsteps(GameTickDurationSeconds * speedScale);
            if (!TryResolveEntityOverlaps(
                Player, entities, movementBounds,
                Player.PositionX - previousX,
                Player.PositionY - previousY,
                new HashSet<Entity>()))
            {
                RestoreEntityPositions(previousPositions, Player);
            }
        }

        public void UpdateEnemyPositions(double speedScale = 1)
        {
            if (!IsGameStarted)
            {
                return;
            }

            Rect movementBounds = MovementArea;
            foreach (PursuerEnemy enemy in _enemies)
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                IReadOnlyList<Entity> entities = GetCombatEntities();
                List<EntityPositionSnapshot> previousPositions =
                    CaptureEntityPositions(entities);
                double previousX = enemy.PositionX;
                double previousY = enemy.PositionY;
                enemy.UpdateBehavior(this, speedScale);
                if (!TryResolveEntityOverlaps(
                    enemy, entities, movementBounds,
                    enemy.PositionX - previousX, enemy.PositionY - previousY,
                    new HashSet<Entity>()))
                {
                    RestoreEntityPositions(previousPositions, enemy);
                }
                enemy.TryAttackTargetInRange(enemy.CurrentTarget, _combatSystem);
            }

            if (Player.IsAlive)
            {
                UpdateEnemySpawning(GameTickDurationSeconds * speedScale);
            }
            else
            {
                IsWeaponWheelOpen = false;
                IsGameStarted = false;
            }
        }

        public void UpdateMirrorImagePositions(double speedScale = 1)
        {
            if (!IsGameStarted)
            {
                return;
            }

            foreach (MirrorImage mirrorImage in _mirrorImages)
            {
                if (!mirrorImage.IsAlive)
                {
                    continue;
                }

                mirrorImage.UpdateBehavior(this, speedScale);
            }
        }

        public void UpdateCombat(double deltaSeconds)
        {
            UpdateCombat(deltaSeconds, deltaSeconds);
        }

        // The player and everything it started advance by playerDeltaSeconds, the rest of
        // the arena by deltaSeconds; both are equal outside Bullet Time.
        public void UpdateCombat(double deltaSeconds, double playerDeltaSeconds)
        {
            if (!IsGameStarted)
            {
                return;
            }

            PlayerCharacter player = Player;
            _combatSystem.Update(
                GetCombatEntities(), MovementArea,
                entity => ReferenceEquals(entity, player)
                    ? playerDeltaSeconds
                    : deltaSeconds);
            RaiseSpinSwings();
            if (!Player.IsAlive)
            {
                IsWeaponWheelOpen = false;
                IsGameStarted = false;
            }
        }

        public void SpawnEnemy()
        {
            if (!IsGameStarted || !Player.IsAlive
                || GetLivingEnemyCount() >= MaximumEnemyCount)
            {
                return;
            }

            Point spawnPosition = GetRandomEnemySpawnPosition(Player);
            AddEnemy(new PursuerEnemy(
                spawnPosition.X, spawnPosition.Y, Player.MaxSpeed));
        }

        private void UpdateEnemySpawning(double deltaSeconds)
        {
            _enemySpawnElapsedSeconds += deltaSeconds;
            if (_enemySpawnElapsedSeconds < EnemySpawnIntervalSeconds)
            {
                return;
            }

            _enemySpawnElapsedSeconds %= EnemySpawnIntervalSeconds;
            if (GetLivingEnemyCount() < MaximumEnemyCount)
            {
                SpawnEnemy();
            }
        }

        private int GetLivingEnemyCount()
        {
            int count = 0;
            foreach (PursuerEnemy enemy in _enemies)
            {
                if (enemy.IsAlive)
                {
                    count++;
                }
            }
            return count;
        }

        private void OnEntityDefeated(Entity source, Entity target, Weapon weapon)
        {
            if (target is MirrorImage mirrorImage)
            {
                _mirrorImages.Remove(mirrorImage);
                return;
            }

            if (target is PursuerEnemy enemy)
            {
                _enemies.Remove(enemy);
                if (source.Team == CombatTeam.Player)
                {
                    Score++;
                    RecordScore();
                    Weapon progressionWeapon = weapon;
                    if (weapon != null && Player.EquippedWeapon != null
                        && weapon.Id != Player.EquippedWeapon.Id)
                    {
                        progressionWeapon = Player.EquippedWeapon;
                    }
                    _progression.AwardKill(progressionWeapon);
                }
            }
        }

        private Point GetRandomEnemySpawnPosition(PlayerCharacter player)
        {
            Rect hitbox = PursuerEnemy.GetHitboxOffsetForSize(PursuerEnemy.HitboxSize);
            double minimumX = WallThickness - hitbox.Left;
            double maximumX = WorldWidth - WallThickness - hitbox.Right;
            double minimumY = WallThickness - hitbox.Top;
            double maximumY = WorldHeight - WallThickness - hitbox.Bottom;
            double minimumDistance = PursuerEnemy.SightRange + EnemySpawnVisibilityMargin;
            double minimumDistanceSquared = minimumDistance * minimumDistance;

            for (int attempt = 0; attempt < EnemySpawnPositionAttemptLimit; attempt++)
            {
                double positionX = minimumX + Random.Shared.NextDouble() * (maximumX - minimumX);
                double positionY = minimumY + Random.Shared.NextDouble() * (maximumY - minimumY);
                double offsetX = positionX - player.PositionX;
                double offsetY = positionY - player.PositionY;
                if (offsetX * offsetX + offsetY * offsetY >= minimumDistanceSquared)
                {
                    if (!OverlapsAnyEnemy(new Rect(
                        positionX + hitbox.X, positionY + hitbox.Y,
                        hitbox.Width, hitbox.Height)))
                    {
                        return new Point(positionX, positionY);
                    }
                }
            }

            throw new InvalidOperationException(
                "Could not find a free enemy spawn position without overlapping another entity.");
        }

        // Mirror images block no one, so their spawn positions only keep the base sizes of
        // the entities apart (as when the ability was added), not the full texture hitboxes,
        // which would not leave room for several images near the player.
        private static bool AreSpawnCirclesTooClose(
            double firstX, double firstY, double firstSize,
            double secondX, double secondY, double secondSize)
        {
            double minimumDistance = (firstSize + secondSize) / 2;
            double offsetX = firstX - secondX;
            double offsetY = firstY - secondY;
            return offsetX * offsetX + offsetY * offsetY < minimumDistance * minimumDistance;
        }

        private static List<EntityPositionSnapshot> CaptureEntityPositions(
            IReadOnlyList<Entity> entities)
        {
            List<EntityPositionSnapshot> snapshots =
                new List<EntityPositionSnapshot>(entities.Count);
            foreach (Entity entity in entities)
            {
                snapshots.Add(new EntityPositionSnapshot(
                    entity, entity.PositionX, entity.PositionY, entity.IsMoving));
            }
            return snapshots;
        }

        private static void RestoreEntityPositions(
            IReadOnlyList<EntityPositionSnapshot> snapshots, Entity blockedMover)
        {
            foreach (EntityPositionSnapshot snapshot in snapshots)
            {
                snapshot.Entity.RestorePosition(
                    snapshot.PositionX, snapshot.PositionY,
                    !ReferenceEquals(snapshot.Entity, blockedMover)
                        && snapshot.IsMoving);
            }
        }

        private static bool TryResolveEntityOverlaps(
            Entity entity, IReadOnlyList<Entity> entities,
            Rect movementBounds, double movementX, double movementY,
            HashSet<Entity> resolving)
        {
            if (entity is MirrorImage)
            {
                return true;
            }
            if (!resolving.Add(entity))
            {
                return false;
            }

            try
            {
                foreach (Entity other in entities)
                {
                    if (ReferenceEquals(entity, other)
                        || other is MirrorImage
                        || !other.IsAlive
                        || !AttackGeometry.RectsOverlap(entity.Hitbox, other.Hitbox))
                    {
                        continue;
                    }
                    if (!other.CanBePushed)
                    {
                        return false;
                    }

                    double directionX = other.PositionX - entity.PositionX;
                    double directionY = other.PositionY - entity.PositionY;
                    double distance = Math.Sqrt(
                        directionX * directionX + directionY * directionY);
                    if (distance == 0)
                    {
                        directionX = movementX;
                        directionY = movementY;
                        distance = Math.Sqrt(
                            directionX * directionX + directionY * directionY);
                    }
                    if (distance == 0)
                    {
                        return false;
                    }

                    directionX /= distance;
                    directionY /= distance;
                    double separation = AttackGeometry.GetSeparationDistance(
                        entity.Hitbox, other.Hitbox, directionX, directionY);
                    if (double.IsInfinity(separation))
                    {
                        return false;
                    }
                    separation += 0.01;
                    double pushedX = other.PositionX + directionX * separation;
                    double pushedY = other.PositionY + directionY * separation;
                    if (!IsWithinMovementBounds(other, pushedX, pushedY, movementBounds))
                    {
                        return false;
                    }

                    other.SetPosition(pushedX, pushedY);
                    if (!TryResolveEntityOverlaps(
                            other, entities, movementBounds,
                            directionX, directionY, resolving)
                        || AttackGeometry.RectsOverlap(entity.Hitbox, other.Hitbox))
                    {
                        return false;
                    }
                }

                return true;
            }
            finally
            {
                resolving.Remove(entity);
            }
        }

        private static bool IsWithinMovementBounds(
            Entity entity, double positionX, double positionY, Rect bounds)
        {
            Rect hitbox = entity.GetHitboxAt(positionX, positionY);
            return hitbox.Left >= bounds.Left
                && hitbox.Right <= bounds.Right
                && hitbox.Top >= bounds.Top
                && hitbox.Bottom <= bounds.Bottom;
        }

        private bool OverlapsAnyEnemy(Rect hitbox)
        {
            foreach (PursuerEnemy enemy in _enemies)
            {
                if (enemy.IsAlive
                    && AttackGeometry.RectsOverlap(hitbox, enemy.Hitbox))
                {
                    return true;
                }
            }

            return false;
        }

        private IReadOnlyList<Entity> GetCombatEntities()
        {
            List<Entity> entities = new List<Entity>(
                _enemies.Count + _mirrorImages.Count + 1)
            {
                Player
            };
            foreach (MirrorImage mirrorImage in _mirrorImages)
            {
                entities.Add(mirrorImage);
            }
            foreach (PursuerEnemy enemy in _enemies)
            {
                entities.Add(enemy);
            }
            return entities;
        }

        private bool TryGetMirrorImageSpawnPositions(
            int count, out List<Point> spawnPositions)
        {
            spawnPositions = new List<Point>(count);
            List<Entity> occupiedEntities = new List<Entity> { Player };

            Rect bounds = MovementArea;
            double radius = Player.Size / 2;
            for (int i = 0; i < count; i++)
            {
                bool foundPosition = false;
                for (int attempt = 0;
                    attempt < EnemySpawnPositionAttemptLimit;
                    attempt++)
                {
                    double angle = Random.Shared.NextDouble() * Math.PI * 2;
                    double distance = MirrorImageMinimumSpawnDistance
                        + Random.Shared.NextDouble()
                            * (MirrorImageSpawnRadius
                                - MirrorImageMinimumSpawnDistance);
                    double x = Player.PositionX + Math.Cos(angle) * distance;
                    double y = Player.PositionY + Math.Sin(angle) * distance;
                    if (x - radius < bounds.Left || x + radius > bounds.Right
                        || y - radius < bounds.Top || y + radius > bounds.Bottom)
                    {
                        continue;
                    }

                    bool overlaps = false;
                    foreach (Entity occupied in occupiedEntities)
                    {
                        if (AreSpawnCirclesTooClose(
                            x, y, Player.Size,
                            occupied.PositionX, occupied.PositionY, occupied.Size))
                        {
                            overlaps = true;
                            break;
                        }
                    }
                    if (overlaps)
                    {
                        continue;
                    }

                    foreach (Point existing in spawnPositions)
                    {
                        if (AreSpawnCirclesTooClose(
                            x, y, Player.Size, existing.X, existing.Y, Player.Size))
                        {
                            overlaps = true;
                            break;
                        }
                    }
                    if (overlaps)
                    {
                        continue;
                    }

                    spawnPositions.Add(new Point(x, y));
                    foundPosition = true;
                    break;
                }

                if (!foundPosition)
                {
                    return false;
                }
            }

            return true;
        }

        private void ReplaceMirrorImages(IReadOnlyList<Point> spawnPositions)
        {
            foreach (MirrorImage mirrorImage in new List<MirrorImage>(_mirrorImages))
            {
                if (mirrorImage.IsAlive)
                {
                    mirrorImage.CurrentHitPoints = 0;
                }
            }

            _mirrorImages.Clear();
            foreach (Point position in spawnPositions)
            {
                AddMirrorImage(new MirrorImage(
                    position.X, position.Y, Player.MaxSpeed, Player.Size));
            }
        }

        public double GetAttackCooldownRemaining(Entity entity)
        {
            return _combatSystem.GetCooldownRemaining(entity);
        }

        public double GetAttackCooldownRemaining(Entity entity, Weapon weapon)
        {
            return _combatSystem.GetCooldownRemaining(entity, weapon);
        }

        private sealed class EntityPositionSnapshot
        {
            public EntityPositionSnapshot(
                Entity entity, double positionX, double positionY, bool isMoving)
            {
                Entity = entity;
                PositionX = positionX;
                PositionY = positionY;
                IsMoving = isMoving;
            }

            public Entity Entity { get; }
            public double PositionX { get; }
            public double PositionY { get; }
            public bool IsMoving { get; }
        }

        public void AttackWithPlayer(double aimX, double aimY)
        {
            if (!IsGameStarted || !Player.IsAlive)
            {
                return;
            }

            SetAimDirection(aimX, aimY);
            aimX = AimDirectionX;
            aimY = AimDirectionY;
            Vector shot = GetProjectileAimDirection(Player.EquippedWeapon);
            PlayerFacing strikeFacing = GetFacingForDirection(aimX, aimY);

            bool started;
            // The launch point uses the hand of the strike facing, also when the corrected
            // projectile direction leans across a facing boundary.
            Player.LaunchFacing = strikeFacing;
            try
            {
                started = _combatSystem.TryStartAttack(Player, shot.X, shot.Y);
            }
            finally
            {
                Player.LaunchFacing = null;
            }
            if (started)
            {
                // The player turns into the strike and keeps that facing until the weapon
                // is ready again (slash finished, bow drawn again).
                Player.StartAttack(strikeFacing, Player.EquippedWeapon.CooldownSeconds);
                if (Player.EquippedWeapon.Id == WeaponCatalog.Sword.Id)
                {
                    PlayerSwordSwung?.Invoke();
                }
            }
        }

        // Follows the spin's game-time progress, so swings stay in step with each revolution
        // and slow down together with the weapon-wheel slow motion.
        private void RaiseSpinSwings()
        {
            AttackVisualState spin = GetPlayerSpinVisual();
            if (spin == null)
            {
                _spinSwingsRaised = 0;
                return;
            }

            int rotations = SwordUpgradeCatalog.GetSpinRotations(_progression);
            int reachedRevolutions = Math.Min(
                rotations, (int)Math.Floor(spin.Progress * rotations) + 1);
            while (_spinSwingsRaised < reachedRevolutions)
            {
                _spinSwingsRaised++;
                PlayerSwordSwung?.Invoke();
            }
        }

        public bool AttackWithWeaponAbility(int slotIndex)
        {
            if (!IsGameStarted || !Player.IsAlive)
            {
                return false;
            }

            Weapon ability = _progression.GetWeaponAbilityWeapon(
                _selectedPlayerWeapon.Id, slotIndex);
            if (ability == null)
            {
                return false;
            }

            bool started = _combatSystem.TryStartAttack(
                Player, ability, AimDirectionX, AimDirectionY);
            if (started)
            {
                Player.StartAttack(
                    GetFacingForDirection(AimDirectionX, AimDirectionY),
                    PlayerCharacter.AttackAnimationDurationSeconds);
            }
            return started;
        }

        public bool AttackWithEntityAbility(int slotIndex)
        {
            if (!IsGameStarted || !Player.IsAlive)
            {
                return false;
            }

            Weapon ability = _progression.GetEntityAbilityWeapon(slotIndex);
            if (ability == null)
            {
                return false;
            }

            List<Point> mirrorSpawnPositions = null;
            if (ability.Effect is MirrorImageSummonEffect)
            {
                int rank = _progression.GetSkillRank(ability.Id);
                if (!TryGetMirrorImageSpawnPositions(
                    rank, out mirrorSpawnPositions))
                {
                    return false;
                }
            }

            bool started = _combatSystem.TryStartAttack(
                Player, ability, AimDirectionX, AimDirectionY);
            if (started)
            {
                Player.StartAttack();
                if (ability.Id == PlayerAbilityCatalog.PowerBlast)
                {
                    PlayerPowerBlast?.Invoke();
                }
                if (ability.Effect is HealingEffect)
                {
                    PlayerHealingPulse?.Invoke();
                }
                if (mirrorSpawnPositions != null)
                {
                    ReplaceMirrorImages(mirrorSpawnPositions);
                }
                if (ability.Effect is TimeSlowEffect timeSlow)
                {
                    ActivateSlowMotion(
                        timeSlow.TimeScale, timeSlow.DurationSeconds,
                        timeSlow.KeepsCasterAtNormalSpeed);
                }
            }
            return started;
        }

        private static PlayerFacing GetFacingForDirection(double directionX, double directionY)
        {
            return PlayerCharacter.GetFacingForDirection(directionX, directionY);
        }

        public bool IsPlayerSpinning
        {
            get
            {
                return GetPlayerSpinVisual() != null;
            }
        }

        public PlayerFacing PlayerSpinFacing
        {
            get
            {
                AttackVisualState visual = GetPlayerSpinVisual();
                if (visual == null)
                {
                    return Player.Facing;
                }

                return Math.Abs(visual.DirectionX) >= Math.Abs(visual.DirectionY)
                    ? (visual.DirectionX < 0 ? PlayerFacing.Left : PlayerFacing.Right)
                    : (visual.DirectionY < 0 ? PlayerFacing.Back : PlayerFacing.Front);
            }
        }

        private AttackVisualState GetPlayerSpinVisual()
        {
            foreach (AttackVisualState visual in _combatSystem.GetActiveVisuals())
            {
                if (ReferenceEquals(visual.Source, Player)
                    && visual.AnimationPlaceholder == "sword-spin")
                {
                    return visual;
                }
            }
            return null;
        }

        public bool HandleMouseDown(MouseButton button)
        {
            if (!IsGameStarted || !Player.IsAlive)
            {
                return false;
            }

            if (_keyBindings.MatchesMouseButton(ArenaAction.WeaponAttack, button))
            {
                AttackWithPlayer(AimDirectionX, AimDirectionY);
                return true;
            }

            ArenaAction[] weaponAbilityActions =
            {
                ArenaAction.WeaponAbility1, ArenaAction.WeaponAbility2,
                ArenaAction.WeaponAbility3, ArenaAction.WeaponAbility4
            };
            for (int i = 0; i < weaponAbilityActions.Length; i++)
            {
                if (_keyBindings.MatchesMouseButton(weaponAbilityActions[i], button))
                {
                    AttackWithWeaponAbility(i);
                    return true;
                }
            }

            ArenaAction[] entityAbilityActions =
            {
                ArenaAction.EntityAbility1, ArenaAction.EntityAbility2,
                ArenaAction.EntityAbility3, ArenaAction.EntityAbility4
            };
            for (int i = 0; i < entityAbilityActions.Length; i++)
            {
                if (_keyBindings.MatchesMouseButton(entityAbilityActions[i], button))
                {
                    AttackWithEntityAbility(i);
                    return true;
                }
            }
            return false;
        }

        public bool HandleKeyDown(Key key)
        {
            if (key == Key.None)
            {
                return false;
            }

            if (IsSprintKey(key))
            {
                // The toggle happens on release so Ctrl + mouse wheel can still zoom.
                _sprintKeyDown = true;
                _sprintKeyUsedForZoom = false;
                return false;
            }

            if (IsGameStarted && Player.IsAlive
                && _keyBindings.MatchesKey(ArenaAction.WeaponWheel, key))
            {
                IsWeaponWheelOpen = true;
                return true;
            }

            if (key == _keyBindings.GetKey(ArenaAction.Pause))
            {
                if (IsWeaponWheelOpen)
                {
                    IsWeaponWheelOpen = false;
                    return true;
                }
                if (IsGameStarted)
                {
                    ReturnToStartMenu();
                    return true;
                }
                if (HasActiveGame && Player.IsAlive)
                {
                    ContinueGame();
                    return true;
                }
            }

            if (IsWeaponWheelOpen)
            {
                return true;
            }

            if (IsGameStarted)
            {
                if (_keyBindings.MatchesKey(ArenaAction.WeaponAttack, key))
                {
                    AttackWithPlayer(AimDirectionX, AimDirectionY);
                    return true;
                }
                if (key == _keyBindings.GetKey(ArenaAction.ToggleDebugRanges))
                {
                    ToggleDamageRanges();
                    return true;
                }
                if (key == _keyBindings.GetKey(ArenaAction.ToggleDebugHud))
                {
                    ToggleDebugHud();
                    return true;
                }
                if (key == _keyBindings.GetKey(ArenaAction.SpawnEnemy))
                {
                    SpawnEnemy();
                    return true;
                }

                ArenaAction[] weaponAbilityActions =
                {
                    ArenaAction.WeaponAbility1, ArenaAction.WeaponAbility2,
                    ArenaAction.WeaponAbility3, ArenaAction.WeaponAbility4
                };
                for (int i = 0; i < weaponAbilityActions.Length; i++)
                {
                    if (_keyBindings.MatchesKey(weaponAbilityActions[i], key))
                    {
                        AttackWithWeaponAbility(i);
                        return true;
                    }
                }

                ArenaAction[] entityAbilityActions =
                {
                    ArenaAction.EntityAbility1, ArenaAction.EntityAbility2,
                    ArenaAction.EntityAbility3, ArenaAction.EntityAbility4
                };
                for (int i = 0; i < entityAbilityActions.Length; i++)
                {
                    if (_keyBindings.MatchesKey(entityAbilityActions[i], key))
                    {
                        AttackWithEntityAbility(i);
                        return true;
                    }
                }
            }

            if (key == _keyBindings.GetKey(ArenaAction.ZoomIn))
            {
                HandleZoomKey(Key.K);
                return true;
            }
            if (key == _keyBindings.GetKey(ArenaAction.ZoomOut))
            {
                HandleZoomKey(Key.L);
                return true;
            }
            return false;
        }

        public bool HandleKeyUp(Key key)
        {
            if (IsSprintKey(key))
            {
                bool wasTapped = _sprintKeyDown && !_sprintKeyUsedForZoom;
                _sprintKeyDown = false;
                if (wasTapped && IsGameStarted && Player.IsAlive)
                {
                    ToggleSprint();
                    return true;
                }
                return false;
            }

            if (!IsWeaponWheelOpen
                || !_keyBindings.MatchesKey(ArenaAction.WeaponWheel, key))
            {
                return false;
            }

            int selectedIndex = WeaponWheelSelectionIndex;
            SelectPlayerWeapon(ArenaWeaponWheel.Weapons[selectedIndex]);
            IsWeaponWheelOpen = false;
            return true;
        }

        private static bool IsSprintKey(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl;
        }

        public void CancelWeaponWheel()
        {
            IsWeaponWheelOpen = false;
        }

        // Handle zoom from the routed key event so short key presses are not missed between ticks.
        public void HandleZoomKey(Key key)
        {
            if (key == Key.K)
            {
                ChangeZoom(1);
            }
            else if (key == Key.L)
            {
                ChangeZoom(-1);
            }
        }

        public void HandleZoomWheel(int delta)
        {
            if (_sprintKeyDown)
            {
                _sprintKeyUsedForZoom = true;
            }
            if (delta != 0)
            {
                ChangeZoom(Math.Sign(delta));
            }
        }

        private void ChangeZoom(int direction)
        {
            ZoomLevel = Math.Max(MinimumZoom,
                Math.Min(MaximumZoom, ZoomLevel + direction * ZoomStep));
        }
    }
}