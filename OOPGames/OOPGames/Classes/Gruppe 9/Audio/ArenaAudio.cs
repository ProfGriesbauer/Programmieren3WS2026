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
        // The race-car engine runs while the player moves: quieter when walking, full volume
        // when sprinting. Stopping plays the brake, turning plays the pass-by "neeow".
        private const string EngineLoopPattern = "sprint_loop_*.wav";
        private const double WalkEngineVolume = 0.45;
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
        private static readonly SoundGroupDefinition[] AllGroups =
        {
            PlayerDamage, PlayerDeath, PlayerLowHealth, NonPlayerEntityDamage,
            EntityDeath, SwordSwing, PaySkill, Turn, Brake, MenuClick
        };

        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private readonly Dictionary<SoundGroupDefinition, ArenaSoundGroup> _groups =
            new Dictionary<SoundGroupDefinition, ArenaSoundGroup>();
        private ArenaField _field;
        private ArenaLoopingSound _walkEngine;
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
                _field.Progression.UpgradePurchased += OnUpgradePurchased;
            }
        }

        // Called every painted frame: keeps the engine loop in step with the game state.
        // It runs only during active gameplay and switches between the walk and sprint level
        // when the sprint toggle changes. Stopping to move plays the brake, while pausing or
        // dying just silences the engine.
        public void Update()
        {
            if (_field == null)
            {
                return;
            }

            bool gameRunning = _field.IsGameStarted && _field.Player.IsAlive;
            ArenaLoopingSound wanted = null;
            if (gameRunning && _field.IsPlayerInMotion)
            {
                wanted = _field.IsSprintEnabled ? GetSprintEngine() : GetWalkEngine();
            }

            ArenaLoopingSound playing = GetPlayingEngine();
            if (ReferenceEquals(wanted, playing))
            {
                return;
            }

            playing?.Stop();
            if (wanted != null)
            {
                wanted.Start();
            }
            else if (gameRunning)
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
            GetWalkEngine();
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

        private void OnNonPlayerEntityDamaged(Entity entity, int lostHitPoints)
        {
            Play(NonPlayerEntityDamage);
        }

        private void OnNonPlayerEntityDied(Entity entity)
        {
            Play(EntityDeath);
        }

        private void OnPlayerSwordSwung()
        {
            Play(SwordSwing);
        }

        private void OnPlayerTurned()
        {
            Play(Turn);
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
                    variants.Add(new ArenaSoundEffect(
                        file, definition.Volume, definition.VoicesPerFile));
                }
                group = new ArenaSoundGroup(variants, definition.MinimumIntervalSeconds);
                _groups.Add(definition, group);
            }
            return group;
        }

        private ArenaLoopingSound GetWalkEngine()
        {
            if (_walkEngine == null)
            {
                _walkEngine = new ArenaLoopingSound(
                    FindSoundFiles(EngineLoopPattern), WalkEngineVolume);
            }
            return _walkEngine;
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

        private ArenaLoopingSound GetPlayingEngine()
        {
            if (_walkEngine != null && _walkEngine.IsPlaying)
            {
                return _walkEngine;
            }
            if (_sprintEngine != null && _sprintEngine.IsPlaying)
            {
                return _sprintEngine;
            }
            return null;
        }

        private void StopEngine()
        {
            _walkEngine?.Stop();
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
                int voicesPerFile)
            {
                FilePattern = filePattern;
                Volume = volume;
                MinimumIntervalSeconds = minimumIntervalSeconds;
                VoicesPerFile = voicesPerFile;
            }

            public string FilePattern { get; }
            public double Volume { get; }
            public double MinimumIntervalSeconds { get; }
            public int VoicesPerFile { get; }
        }
    }
}
