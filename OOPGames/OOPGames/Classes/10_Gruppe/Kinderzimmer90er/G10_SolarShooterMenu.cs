using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace OOPGames
{
    internal enum G10_SolarShooterMenuScreen
    {
        Main,
        ModeSelection,
        Leaderboard,
        Help,
        Playing,
        Paused
    }

    internal sealed class G10_SolarShooterMenu
    {
        static readonly string[] MainOptions =
        {
            "NEUES SPIEL", "FORTSETZEN", "MODUS WAEHLEN",
            "ANLEITUNG", "BESTENLISTE", "ZURUECK"
        };
        static readonly string[] ModeOptions = { "KLASSIK", "MISSIONEN" };
        static readonly string[] PauseOptions = { "FORTSETZEN", "ZUM MENUE" };
        static readonly string[] HelpTitles =
        {
            "STEUERUNG", "GEGNER", "POWER-UPS", "MISSIONEN"
        };
        static readonly string[][] HelpPages =
        {
            new[]
            {
                "PFEILTASTEN / WASD: SCHIFF BEWEGEN",
                "Z ODER LEERTASTE: DAUERFEUER",
                "ESC / X: PAUSEMENUE",
                "WEICHE ROTEN GEGNER-SCHUESSEN AUS."
            },
            new[]
            {
                "DRIFTLING: SCHWINGT LANGSAM.",
                "NEEDLEWING: SCHNELL UND WENDIG.",
                "BULWARK: LANGSAM, ABER ROBUST.",
                "BOSSE FEUERN JE NACH STAGE ANDERS."
            },
            new[]
            {
                "POWER-CELLS FALLEN NACH JEDEM",
                "VIERTEN BESIEGTEN GEGNER.",
                "EINSAMMELN VERBESSERT DIE WAFFE.",
                "MAXIMALSTUFE: BONUS-PUNKTE."
            },
            new[]
            {
                "MISSIONEN GEBEN ZEITLICHE ZIELE.",
                "NUR EINE MISSION IST AKTIV.",
                "ERFOLG BRINGT BONUS ODER BOOST.",
                "TREFFER KOENNEN MISSIONEN BEENDEN."
            }
        };
        readonly IReadOnlyList<G10_SolarShooterMode> _Modes;
        int _Selection;
        int _ModeIndex;
        int _HelpPage;
        G10_SolarShooterMenuScreen _HelpReturnScreen;

        public G10_SolarShooterMenu(IReadOnlyList<G10_SolarShooterMode> modes)
        {
            if (modes == null || modes.Count == 0)
                throw new ArgumentException("At least one game mode is required.", "modes");
            _Modes = modes;
            Screen = G10_SolarShooterMenuScreen.Main;
        }

        public event Action NewGameRequested;
        public event Action ContinueRequested;
        public event Action ReturnToMenuRequested;
        public event Action ExitRequested;

        public G10_SolarShooterMenuScreen Screen { get; private set; }
        public int Selection { get { return _Selection; } }
        public int ModeIndex { get { return _ModeIndex; } }
        public G10_SolarShooterMode SelectedMode { get { return _Modes[_ModeIndex]; } }
        public bool WantsExit { get; private set; }
        public string StatusMessage { get; private set; }
        public int MainOptionCount { get { return MainOptions.Length; } }
        public int ModeOptionCount { get { return ModeOptions.Length; } }
        public int PauseOptionCount { get { return PauseOptions.Length; } }

        public string MainOption(int index) { return MainOptions[index]; }
        public string ModeOption(int index) { return ModeOptions[index]; }
        public string PauseOption(int index) { return PauseOptions[index]; }
        public int HelpPageIndex { get { return _HelpPage; } }
        public int HelpPageCount { get { return HelpTitles.Length; } }
        public string HelpTitle { get { return HelpTitles[_HelpPage]; } }
        public IReadOnlyList<string> HelpLines { get { return HelpPages[_HelpPage]; } }

        public void Reset()
        {
            Screen = G10_SolarShooterMenuScreen.Main;
            _Selection = 0;
            _ModeIndex = 0;
            WantsExit = false;
            StatusMessage = null;
        }

        public void SetStatusMessage(string message)
        {
            StatusMessage = message;
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            foreach (Key key in keys)
            {
                if (HandleKey(key)) return;
            }
        }

        public void OpenPause()
        {
            Screen = G10_SolarShooterMenuScreen.Paused;
            _Selection = 0;
            StatusMessage = null;
        }

        public void BeginGameplay()
        {
            Screen = G10_SolarShooterMenuScreen.Playing;
            StatusMessage = null;
        }

        public void ReturnToMainMenu()
        {
            Screen = G10_SolarShooterMenuScreen.Main;
            _Selection = 0;
            StatusMessage = null;
        }

        public void OpenHelp()
        {
            _HelpReturnScreen = Screen;
            Screen = G10_SolarShooterMenuScreen.Help;
            _Selection = 0;
            _HelpPage = 0;
        }

        public void CloseHelp()
        {
            Screen = _HelpReturnScreen;
            _Selection = 0;
        }

        bool HandleKey(Key key)
        {
            if (key == Key.Escape || key == Key.X || key == Key.Back)
            {
                HandleBack();
                return true;
            }

            if (Screen == G10_SolarShooterMenuScreen.Main)
                return HandleMainKey(key);
            if (Screen == G10_SolarShooterMenuScreen.ModeSelection)
                return HandleModeKey(key);
            if (Screen == G10_SolarShooterMenuScreen.Leaderboard)
                return HandleLeaderboardKey(key);
            if (Screen == G10_SolarShooterMenuScreen.Help)
                return HandleHelpKey(key);
            if (Screen == G10_SolarShooterMenuScreen.Paused)
                return HandlePauseKey(key);
            return false;
        }

        bool HandleMainKey(Key key)
        {
            if (IsUp(key) || IsDown(key))
            {
                _Selection = (_Selection + (IsDown(key) ? 1 : -1) + MainOptions.Length) % MainOptions.Length;
                return true;
            }
            if (key == Key.Enter || key == Key.Z)
            {
                ActivateMainOption();
                return true;
            }
            return false;
        }

        bool HandleModeKey(Key key)
        {
            if (IsUp(key) || IsDown(key))
            {
                _Selection = (_Selection + (IsDown(key) ? 1 : -1) + ModeOptions.Length) % ModeOptions.Length;
                return true;
            }
            if (key == Key.Enter || key == Key.Z)
            {
                _ModeIndex = _Selection;
                Screen = G10_SolarShooterMenuScreen.Main;
                _Selection = 0;
                StatusMessage = "MODUS: " + SelectedMode.Name;
                return true;
            }
            return false;
        }

        bool HandleLeaderboardKey(Key key)
        {
            if (IsLeft(key) || IsRight(key))
            {
                _ModeIndex = (_ModeIndex + (IsRight(key) ? 1 : _Modes.Count - 1)) % _Modes.Count;
                return true;
            }
            return false;
        }

        bool HandleHelpKey(Key key)
        {
            if (IsUp(key) || IsLeft(key))
            {
                _HelpPage = Math.Max(0, _HelpPage - 1);
                return true;
            }
            if (IsDown(key) || IsRight(key) || key == Key.Enter || key == Key.Z)
            {
                if (_HelpPage == HelpTitles.Length - 1) CloseHelp();
                else _HelpPage++;
                return true;
            }
            return false;
        }

        bool HandlePauseKey(Key key)
        {
            if (IsUp(key) || IsDown(key))
            {
                _Selection = (_Selection + (IsDown(key) ? 1 : PauseOptions.Length - 1)) % PauseOptions.Length;
                return true;
            }
            if (key == Key.Enter || key == Key.Z)
            {
                if (_Selection == 0) BeginGameplay();
                else
                {
                    Screen = G10_SolarShooterMenuScreen.Main;
                    _Selection = 0;
                    Action handler = ReturnToMenuRequested;
                    if (handler != null) handler();
                }
                return true;
            }
            return false;
        }

        void HandleBack()
        {
            if (Screen == G10_SolarShooterMenuScreen.Main)
            {
                WantsExit = true;
                Action handler = ExitRequested;
                if (handler != null) handler();
            }
            else if (Screen == G10_SolarShooterMenuScreen.Help)
                CloseHelp();
            else if (Screen == G10_SolarShooterMenuScreen.Paused)
                BeginGameplay();
            else
                ReturnToMainMenu();
        }

        void ActivateMainOption()
        {
            switch (_Selection)
            {
                case 0:
                    Action newGame = NewGameRequested;
                    if (newGame != null) newGame();
                    break;
                case 1:
                    Action continueGame = ContinueRequested;
                    if (continueGame != null) continueGame();
                    break;
                case 2:
                    Screen = G10_SolarShooterMenuScreen.ModeSelection;
                    _Selection = _ModeIndex;
                    StatusMessage = null;
                    break;
                case 3:
                    OpenHelp();
                    break;
                case 4:
                    Screen = G10_SolarShooterMenuScreen.Leaderboard;
                    _Selection = 0;
                    break;
                default:
                    WantsExit = true;
                    Action exit = ExitRequested;
                    if (exit != null) exit();
                    break;
            }
        }

        static bool IsLeft(Key key) { return key == Key.Left || key == Key.A; }
        static bool IsRight(Key key) { return key == Key.Right || key == Key.D; }
        static bool IsUp(Key key) { return key == Key.Up || key == Key.W; }
        static bool IsDown(Key key) { return key == Key.Down || key == Key.S; }
    }
}
