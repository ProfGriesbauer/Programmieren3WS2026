using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class G10_SolarShooterGame : IB5RoomOwnBackKey
    {
        readonly G10_SolarShooterRenderer _Renderer;
        readonly G10_ShooterInputMapper _InputMapper;
        readonly G10_SolarShooterMenu _Menu;
        readonly IG10_ShooterSaveStore _Saves;
        G10_SolarShooterRun _Run;
        float _AutosaveTimer;

        public G10_SolarShooterGame()
            : this(new G10_SolarShooterRenderer(),
                new G10_ShooterInputMapper(
                    new G10_WpfKeyStateSource(),
                    G10_ShooterKeyBindings.Default),
                new G10_FileShooterSaveStore()) { }

        internal G10_SolarShooterGame(
            G10_SolarShooterRenderer renderer,
            G10_ShooterInputMapper inputMapper,
            IG10_ShooterSaveStore saves)
        {
            if (renderer == null) throw new ArgumentNullException("renderer");
            if (inputMapper == null) throw new ArgumentNullException("inputMapper");
            if (saves == null) throw new ArgumentNullException("saves");
            _Renderer = renderer;
            _InputMapper = inputMapper;
            _Saves = saves;
            _Menu = new G10_SolarShooterMenu(G10_SolarShooterModeCatalog.CreateModes());
            _Menu.NewGameRequested += StartNewGame;
            _Menu.ContinueRequested += ContinueGame;
            _Menu.ReturnToMenuRequested += ReturnToMenu;
        }

        public string Title { get { return "SolarShooter"; } }
        public int Width { get { return (int)G10_ShooterConfig.WorldWidth; } }
        public int Height { get { return (int)G10_ShooterConfig.WorldHeight; } }
        public bool ExitRequested { get { return _Menu.WantsExit; } }

        public void Start()
        {
            _Run = null;
            _AutosaveTimer = 0;
            _Menu.Reset();
            _InputMapper.Reset();
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            if (_Menu.Screen != G10_SolarShooterMenuScreen.Playing)
            {
                _Menu.Update(keys);
                return;
            }

            G10_SolarShooterInput input;
            bool restart = _InputMapper.Read(out input);
            if (ContainsBackKey(keys))
            {
                SaveProgress(!_Run.IsFinished);
                _Menu.OpenPause();
                _InputMapper.Reset();
                return;
            }

            if (_Run.IsFinished)
            {
                if (restart || ContainsKey(keys, Key.Z))
                    _Menu.ReturnToMainMenu();
                return;
            }

            int previousStage = _Run.Session.StageIndex;
            _Run.Update(G10_ShooterConfig.FixedDeltaSeconds, input);
            _AutosaveTimer += G10_ShooterConfig.FixedDeltaSeconds;
            if (_Run.Session.StageIndex != previousStage)
            {
                _AutosaveTimer = 0;
                SaveProgress(true);
            }
            else if (_AutosaveTimer >= 1)
            {
                _AutosaveTimer = 0;
                SaveProgress(true);
            }
            if (_Run.IsFinished)
                SaveProgress(false);
        }

        public void Draw(Canvas canvas)
        {
            G10_SolarShooterModeSave save = _Saves.GetMode(_Menu.SelectedMode.Id);
            if (_Run == null)
                _Renderer.Draw(canvas, null, _Menu, save, G10_EmptyModeHud.Instance);
            else
                _Renderer.Draw(canvas, _Run.Session, _Menu, save, _Run.Hud);
        }

        void StartNewGame()
        {
            BeginRun(_Menu.SelectedMode.StartRun(null));
            SaveProgress(true);
        }

        void ContinueGame()
        {
            G10_SolarShooterMode mode = _Menu.SelectedMode;
            G10_SolarShooterModeSave save = _Saves.GetMode(mode.Id);
            if (!save.HasResume)
            {
                _Menu.SetStatusMessage("KEIN SPIELSTAND FUER " + mode.Name);
                return;
            }

            BeginRun(mode.StartRun(save));
        }

        void BeginRun(G10_SolarShooterRun run)
        {
            _Run = run;
            _AutosaveTimer = 0;
            _Menu.BeginGameplay();
            _InputMapper.Reset();
        }

        void SaveProgress(bool canResume)
        {
            if (_Run == null) return;
            _Run.WriteSave(_Saves.GetMode(_Run.Mode.Id), canResume);
            _Saves.Save();
        }

        void ReturnToMenu()
        {
            if (_Run != null) SaveProgress(!_Run.IsFinished);
            _Run = null;
            _InputMapper.Reset();
        }

        static bool ContainsBackKey(IReadOnlyCollection<Key> keys)
        {
            return ContainsKey(keys, Key.Escape) || ContainsKey(keys, Key.X);
        }

        static bool ContainsKey(IReadOnlyCollection<Key> keys, Key target)
        {
            foreach (Key key in keys)
                if (key == target)
                    return true;
            return false;
        }
    }
}
