using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace OOPGames
{
    // Connects arena events to sound groups. Every file in Classes\Gruppe 9\Assets\Sounds
    // that matches a group's pattern becomes a random variant of that group, so new
    // recordings only need the right file name. Missing files make the event silent.
    public sealed class ArenaAudio
    {
        private const string SoundFolder = "Sounds";
        // The race-car sounds belong to sprinting only: the engine runs while the player
        // sprints, ending the sprint plays the brake, turning while sprinting plays the
        // pass-by "neeow". Walking only has footsteps.
        private const string EngineLoopPattern = "sprint_loop_*.wav";
        private const double SprintEngineVolume = 1.0;

        // Pattern, volume, minimum seconds between two plays, voices per file.
        // The minimum interval merges many simultaneous hits into one sound; extra voices let
        // the same file overlap itself for sounds that come in quick succession.
        private static readonly SoundGroupDefinition PlayerDamage =
            new SoundGroupDefinition("playerDamage_*.wav", 0.8, 0.08, 1);
        private static readonly SoundGroupDefinition PlayerDeath =
            new SoundGroupDefinition("player_death*.wav", 0.9, 0, 1);
        private static readonly SoundGroupDefinition PlayerLowHealth =
            new SoundGroupDefinition("playerLowHealth*.wav", 0.8, 0, 1);
        private static readonly SoundGroupDefinition NonPlayerEntityDamage =
            new SoundGroupDefinition("nonPlayerEntityDamage_*.wav", 0.7, 0.05, 2);
        private static readonly SoundGroupDefinition EntityDeath =
            new SoundGroupDefinition("entity_death_*.wav", 0.7, 0.08, 1);
        private static readonly SoundGroupDefinition SwordSwing =
            new SoundGroupDefinition("sword_swing_*.wav", 0.6, 0, 2);
        private static readonly SoundGroupDefinition PaySkill =
            new SoundGroupDefinition("paySkill*.wav", 0.8, 0, 2);
        // The 0.4 s cooldown keeps rapid direction changes from stacking pass-by sounds.
        private static readonly SoundGroupDefinition Turn =
            new SoundGroupDefinition("sprint_turn_neeow_*.wav", 0.8, 0.4, 1);
        private static readonly SoundGroupDefinition Brake =
            new SoundGroupDefinition("sprint_stop_brake*.wav", 0.8, 0, 1);
        private static readonly SoundGroupDefinition MenuClick =
            new SoundGroupDefinition("ui_menu_click*.wav", 0.7, 0, 2);
        // Footsteps while walking; sprinting has the engine loop instead.
        private static readonly SoundGroupDefinition Footstep =
            new SoundGroupDefinition("playerFootstep_*.wav", 0.6, 0, 2);
        // Electric shock (lightning stun) on anyone, on top of the damage sound: one of the
        // long variants, or the short 04 when shocks follow each other quickly.
        private static readonly SoundGroupDefinition ElectricDamage =
            new SoundGroupDefinition("electricDamage_*.wav", 0.7, 0.05, 1, "electricDamage_04");
        private static readonly SoundGroupDefinition ElectricDamageShort =
            new SoundGroupDefinition("electricDamage_04*.wav", 0.7, 0.05, 2);
        private const double RapidShockSeconds = 0.6;
        // Plays on every healing pulse; a second voice lets quick pulses overlap.
        private static readonly SoundGroupDefinition PlayerHeal =
            new SoundGroupDefinition("playerHeal*.wav", 0.8, 0, 2);
        private static readonly SoundGroupDefinition PowerBlast =
            new SoundGroupDefinition("powerBlast*.wav", 0.8, 0, 2);
        // The mine turtle's "Hello!" right before it explodes.
        private static readonly SoundGroupDefinition MineTurtleHello =
            new SoundGroupDefinition("mineTurtleHello*.wav", 0.9, 0, 2);
        // Frog croaks (0.4-0.6 s): one voice and a pause longer than a croak, so croaks
        // never overlap.
        private static readonly SoundGroupDefinition FrogCroak =
            new SoundGroupDefinition("frogCroak_*.wav", 0.7, 0.7, 1);
        private static readonly SoundGroupDefinition[] AllGroups =
        {
            PlayerDamage, PlayerDeath, PlayerLowHealth, NonPlayerEntityDamage,
            EntityDeath, SwordSwing, PaySkill, Turn, Brake, MenuClick, Footstep,
            ElectricDamage, ElectricDamageShort, PlayerHeal, PowerBlast, MineTurtleHello,
            FrogCroak
        };
        // Sounds an enemy kind may name for an attack or a windup step (enemy editor).
        private static readonly Dictionary<string, SoundGroupDefinition> EnemySounds =
            new Dictionary<string, SoundGroupDefinition>(StringComparer.OrdinalIgnoreCase)
            {
                { "PowerBlast", PowerBlast },
                { "MineTurtleHello", MineTurtleHello },
                { "FrogCroak", FrogCroak },
                { "SwordSwing", SwordSwing },
                { "ElectricDamage", ElectricDamage },
                { "EntityDeath", EntityDeath }
            };

        public static IReadOnlyList<string> EnemySoundNames
        {
            get { return new List<string>(EnemySounds.Keys); }
        }

        // File patterns of every sound group and the sprint engine loop; each WAV in
        // Assets/Sounds should match one (checked by the test harness).
        internal static IReadOnlyList<string> SoundFilePatterns
        {
            get
            {
                List<string> patterns = new List<string> { EngineLoopPattern };
                foreach (SoundGroupDefinition group in AllGroups)
                {
                    patterns.Add(group.FilePattern);
                }
                return patterns;
            }
        }
        private double _lastShockSeconds = double.NegativeInfinity;
        // The death sound follows the lethal hit's damage sound a moment later, so it does not
        // cover it.
        private const double DeathSoundDelaySeconds = 0.15;
        private readonly List<double> _pendingDeathSounds = new List<double>();
        // Weapon wheel state of the previous frame: opening and moving to another segment
        // click, choosing a different weapon confirms with the purchase sound.
        private bool _wheelWasOpen;
        private int _wheelSelection = -1;
        private string _weaponBeforeWheel;

        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly Dictionary<SoundGroupDefinition, ArenaSoundGroup> _groups =
            new Dictionary<SoundGroupDefinition, ArenaSoundGroup>();
        private ArenaField _field;
        private ArenaLoopingSound _sprintEngine;

        // Follows the field shown by the painter; switching fields moves the subscriptions.
        public void Attach(ArenaField field)
        {
            if (ReferenceEquals(_field, field))
            {
                return;
            }
            if (_field != null)
            {
                StopEngine();
                _field.PlayerDamaged -= OnPlayerDamaged;
                _field.PlayerDied -= OnPlayerDied;
                _field.PlayerHealthLow -= OnPlayerHealthLow;
                _field.NonPlayerEntityDamaged -= OnNonPlayerEntityDamaged;
                _field.NonPlayerEntityDied -= OnNonPlayerEntityDied;
                _field.PlayerSwordSwung -= OnPlayerSwordSwung;
                _field.PlayerTurned -= OnPlayerTurned;
                _field.PlayerFootstep -= OnPlayerFootstep;
                _field.EntityStatusApplied -= OnEntityStatusApplied;
                _field.PlayerHealingPulse -= OnPlayerHealingPulse;
                _field.PlayerPowerBlast -= OnPlayerPowerBlast;
                _field.EnemySoundRequested -= OnEnemySoundRequested;
                _field.Progression.UpgradePurchased -= OnUpgradePurchased;
            }

            _field = field;
            if (_field != null)
            {
                _field.PlayerDamaged += OnPlayerDamaged;
                _field.PlayerDied += OnPlayerDied;
                _field.PlayerHealthLow += OnPlayerHealthLow;
                _field.NonPlayerEntityDamaged += OnNonPlayerEntityDamaged;
                _field.NonPlayerEntityDied += OnNonPlayerEntityDied;
                _field.PlayerSwordSwung += OnPlayerSwordSwung;
                _field.PlayerTurned += OnPlayerTurned;
                _field.PlayerFootstep += OnPlayerFootstep;
                _field.EntityStatusApplied += OnEntityStatusApplied;
                _field.PlayerHealingPulse += OnPlayerHealingPulse;
                _field.PlayerPowerBlast += OnPlayerPowerBlast;
                _field.EnemySoundRequested += OnEnemySoundRequested;
                _field.Progression.UpgradePurchased += OnUpgradePurchased;
            }
        }

        // Called every painted frame: keeps the engine loop in step with the game state.
        // It runs only while the player sprints during active gameplay. Ending the sprint
        // (stopping, or switching sprint off while walking) plays the brake, while pausing or
        // dying just silences the engine.
        public void Update()
        {
            PlayDueDeathSounds();
            if (_field == null)
            {
                return;
            }
            UpdateWeaponWheelSounds();

            bool gameRunning = _field.IsGameStarted && _field.Player.IsAlive;
            bool wanted = gameRunning && _field.IsPlayerSprinting;
            ArenaLoopingSound engine = GetSprintEngine();
            if (wanted == engine.IsPlaying)
            {
                return;
            }

            if (wanted)
            {
                engine.Start();
                return;
            }

            engine.Stop();
            if (gameRunning)
            {
                Play(Brake);
            }
        }

        // Opens all sound files ahead of time so the first hit is not delayed by loading.
        public void PreloadAll()
        {
            foreach (SoundGroupDefinition definition in AllGroups)
            {
                GetGroup(definition);
            }
            GetSprintEngine();
        }

        public void PlayMenuClick()
        {
            Play(MenuClick);
        }

        private void OnPlayerDamaged(int lostHitPoints)
        {
            // A lethal hit is covered by the death sound alone.
            if (_field.Player.IsAlive)
            {
                Play(PlayerDamage);
            }
        }

        private void OnPlayerDied()
        {
            // The engine loop must not run over the death sound.
            StopEngine();
            Play(PlayerDeath);
        }

        private void OnPlayerHealthLow()
        {
            Play(PlayerLowHealth);
        }

        // Every hit is heard, also the lethal one: it is never merged into an earlier hit.
        private void OnNonPlayerEntityDamaged(Entity entity, int lostHitPoints)
        {
            GetGroup(NonPlayerEntityDamage).Play(
                _clock.Elapsed.TotalSeconds, !entity.IsAlive);
        }

        private void OnNonPlayerEntityDied(Entity entity)
        {
            _pendingDeathSounds.Add(_clock.Elapsed.TotalSeconds + DeathSoundDelaySeconds);
        }

        private void UpdateWeaponWheelSounds()
        {
            bool isOpen = _field.IsWeaponWheelOpen;
            if (isOpen)
            {
                int selection = _field.WeaponWheelSelectionIndex;
                if (!_wheelWasOpen)
                {
                    _weaponBeforeWheel = _field.SelectedPlayerWeapon.Id;
                    Play(MenuClick);
                }
                else if (selection != _wheelSelection)
                {
                    Play(MenuClick);
                }
                _wheelSelection = selection;
            }
            else if (_wheelWasOpen)
            {
                // Escape or releasing on the current weapon changes nothing and stays silent.
                if (_field.SelectedPlayerWeapon.Id != _weaponBeforeWheel)
                {
                    Play(PaySkill);
                }
                _wheelSelection = -1;
            }
            _wheelWasOpen = isOpen;
        }

        private void PlayDueDeathSounds()
        {
            double now = _clock.Elapsed.TotalSeconds;
            for (int i = _pendingDeathSounds.Count - 1; i >= 0; i--)
            {
                if (_pendingDeathSounds[i] <= now)
                {
                    _pendingDeathSounds.RemoveAt(i);
                    Play(EntityDeath);
                }
            }
        }

        private void OnPlayerSwordSwung()
        {
            Play(SwordSwing);
        }

        private void OnPlayerTurned()
        {
            if (_field.IsPlayerSprinting)
            {
                Play(Turn);
            }
        }

        private void OnPlayerFootstep()
        {
            Play(Footstep);
        }

        private void OnEntityStatusApplied(Entity entity, string statusId)
        {
            if (statusId != "stun")
            {
                return;
            }

            double now = _clock.Elapsed.TotalSeconds;
            Play(now - _lastShockSeconds < RapidShockSeconds
                ? ElectricDamageShort
                : ElectricDamage);
            _lastShockSeconds = now;
        }

        // Tied to releasing the pulse, so it also plays at full health.
        private void OnPlayerHealingPulse()
        {
            Play(PlayerHeal);
        }

        private void OnEnemySoundRequested(NonPlayerCharacter enemy, string sound)
        {
            if (sound != null && EnemySounds.TryGetValue(sound, out SoundGroupDefinition group))
            {
                Play(group);
            }
        }

        private void OnPlayerPowerBlast()
        {
            Play(PowerBlast);
        }

        private void OnUpgradePurchased()
        {
            Play(PaySkill);
        }

        private void Play(SoundGroupDefinition definition)
        {
            GetGroup(definition).Play(_clock.Elapsed.TotalSeconds);
        }

        private ArenaSoundGroup GetGroup(SoundGroupDefinition definition)
        {
            if (!_groups.TryGetValue(definition, out ArenaSoundGroup group))
            {
                List<IArenaSoundEffect> variants = new List<IArenaSoundEffect>();
                foreach (string file in FindSoundFiles(definition.FilePattern))
                {
                    if (definition.ExcludedFileName != null
                        && Path.GetFileName(file).StartsWith(
                            definition.ExcludedFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    variants.Add(new ArenaSoundEffect(
                        file, definition.Volume, definition.VoicesPerFile));
                }
                group = new ArenaSoundGroup(variants, definition.MinimumIntervalSeconds);
                _groups.Add(definition, group);
            }
            return group;
        }

        private ArenaLoopingSound GetSprintEngine()
        {
            if (_sprintEngine == null)
            {
                _sprintEngine = new ArenaLoopingSound(
                    FindSoundFiles(EngineLoopPattern), SprintEngineVolume);
            }
            return _sprintEngine;
        }

        private void StopEngine()
        {
            _sprintEngine?.Stop();
        }

        private static string[] FindSoundFiles(string filePattern)
        {
            try
            {
                string folder = PainterG9.FindAssetDirectory(SoundFolder);
                string[] files = Directory.GetFiles(folder, filePattern);
                Array.Sort(files, StringComparer.OrdinalIgnoreCase);
                return files;
            }
            catch (Exception exception) when (exception is IOException
                || exception is UnauthorizedAccessException)
            {
                // Without the sound folder the game keeps running silently.
                return new string[0];
            }
        }

        private sealed class SoundGroupDefinition
        {
            public SoundGroupDefinition(
                string filePattern, double volume, double minimumIntervalSeconds,
                int voicesPerFile, string excludedFileName = null)
            {
                FilePattern = filePattern;
                Volume = volume;
                MinimumIntervalSeconds = minimumIntervalSeconds;
                VoicesPerFile = voicesPerFile;
                ExcludedFileName = excludedFileName;
            }

            // Files whose name starts with this are left out (e.g. the short variant).
            public string ExcludedFileName { get; }

            public string FilePattern { get; }
            public double Volume { get; }
            public double MinimumIntervalSeconds { get; }
            public int VoicesPerFile { get; }
        }
    }
}
