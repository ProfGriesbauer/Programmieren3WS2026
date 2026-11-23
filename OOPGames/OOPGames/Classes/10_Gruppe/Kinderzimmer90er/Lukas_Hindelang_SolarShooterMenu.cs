using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace OOPGames
{
    // Read-only name of the current screen for the renderer and the game. It is derived
    // from the current phase object, so there is only one source of truth.
    internal enum G10_SolarShooterMenuScreen
    {
        Main,
        ModeSelection,
        Leaderboard,
        Help,
        Playing,
        Paused
    }

    // Holds the menu data (cursor, selected mode, help page) and the operations the phases
    // use. Which key does what is decided by the current G10_SolarShooterMenuPhase.
    internal sealed class G10_SolarShooterMenu
    {
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
        G10_SolarShooterMenuPhase _Phase;
        G10_SolarShooterMenuPhase _HelpReturnPhase;
        int _ModeIndex;
        int _HelpPage;

        public G10_SolarShooterMenu(IReadOnlyList<G10_SolarShooterMode> modes)
        {
            if (modes == null || modes.Count == 0)
                throw new ArgumentException("At least one game mode is required.", "modes");
            _Modes = modes;
            _Phase = G10_SolarShooterMenuPhases.Main;
            _HelpReturnPhase = G10_SolarShooterMenuPhases.Main;
        }

        public event Action NewGameRequested;
        public event Action ContinueRequested;
        public event Action ReturnToMenuRequested;
        public event Action ExitRequested;

        public G10_SolarShooterMenuScreen Screen { get { return _Phase.Screen; } }
        public int Selection { get; private set; }
        public int ModeIndex { get { return _ModeIndex; } }
        public IReadOnlyList<G10_SolarShooterMode> Modes { get { return _Modes; } }
        public G10_SolarShooterMode SelectedMode { get { return _Modes[_ModeIndex]; } }
        public bool WantsExit { get; private set; }
        public string StatusMessage { get; private set; }

        public int MainOptionCount { get { return G10_SolarShooterMenuPhases.Main.Entries(this).Count; } }
        public int ModeOptionCount { get { return G10_SolarShooterMenuPhases.ModeSelection.Entries(this).Count; } }
        public int PauseOptionCount { get { return G10_SolarShooterMenuPhases.Paused.Entries(this).Count; } }
        public string MainOption(int index) { return G10_SolarShooterMenuPhases.Main.Entries(this)[index].Label; }
        public string ModeOption(int index) { return G10_SolarShooterMenuPhases.ModeSelection.Entries(this)[index].Label; }
        public string PauseOption(int index) { return G10_SolarShooterMenuPhases.Paused.Entries(this)[index].Label; }

        public int HelpPageIndex { get { return _HelpPage; } }
        public int HelpPageCount { get { return HelpTitles.Length; } }
        public bool IsLastHelpPage { get { return _HelpPage == HelpTitles.Length - 1; } }
        public string HelpTitle { get { return HelpTitles[_HelpPage]; } }
        public IReadOnlyList<string> HelpLines { get { return HelpPages[_HelpPage]; } }

        public void Reset()
        {
            SetPhase(G10_SolarShooterMenuPhases.Main);
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
                if (key == Key.Escape || key == Key.X || key == Key.Back)
                {
                    _Phase.HandleBack(this);
                    return;
                }
                if (_Phase.HandleKey(this, key)) return;
            }
        }

        // ---- Operations used by the game and by the phases ----

        public void OpenPause()
        {
            SetPhase(G10_SolarShooterMenuPhases.Paused);
            StatusMessage = null;
        }

        public void BeginGameplay()
        {
            SetPhase(G10_SolarShooterMenuPhases.Playing);
            StatusMessage = null;
        }

        public void ReturnToMainMenu()
        {
            SetPhase(G10_SolarShooterMenuPhases.Main);
            StatusMessage = null;
        }

        internal void MoveSelection(int delta, int count)
        {
            Selection = ((Selection + delta) % count + count) % count;
        }

        internal void OpenModeSelection()
        {
            SetPhase(G10_SolarShooterMenuPhases.ModeSelection);
            Selection = _ModeIndex;
            StatusMessage = null;
        }

        internal void SelectMode(int modeIndex)
        {
            _ModeIndex = modeIndex;
            SetPhase(G10_SolarShooterMenuPhases.Main);
            StatusMessage = "MODUS: " + SelectedMode.Name;
        }

        internal void OpenLeaderboard()
        {
            SetPhase(G10_SolarShooterMenuPhases.Leaderboard);
        }

        internal void CycleMode(int delta)
        {
            _ModeIndex = ((_ModeIndex + delta) % _Modes.Count + _Modes.Count) % _Modes.Count;
        }

        internal void OpenHelp()
        {
            _HelpReturnPhase = _Phase;
            SetPhase(G10_SolarShooterMenuPhases.Help);
            _HelpPage = 0;
        }

        internal void CloseHelp()
        {
            SetPhase(_HelpReturnPhase);
        }

        internal void NextHelpPage() { _HelpPage = Math.Min(HelpTitles.Length - 1, _HelpPage + 1); }
        internal void PreviousHelpPage() { _HelpPage = Math.Max(0, _HelpPage - 1); }

        internal void RequestNewGame() { Raise(NewGameRequested); }
        internal void RequestContinue() { Raise(ContinueRequested); }

        internal void RequestExit()
        {
            WantsExit = true;
            Raise(ExitRequested);
        }

        internal void LeaveRun()
        {
            SetPhase(G10_SolarShooterMenuPhases.Main);
            Raise(ReturnToMenuRequested);
        }

        // Every screen change starts with the cursor on the first entry.
        void SetPhase(G10_SolarShooterMenuPhase phase)
        {
            if (phase == null) throw new ArgumentNullException("phase");
            _Phase = phase;
            Selection = 0;
        }

        static void Raise(Action handler)
        {
            if (handler != null) handler();
        }
    }
}
