using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace OOPGames
{
    // State pattern: every menu screen is a phase object that decides itself what a key
    // or the back key does. The menu only holds the shared data and delegates to its phase.
    internal abstract class G10_SolarShooterMenuPhase
    {
        public abstract G10_SolarShooterMenuScreen Screen { get; }

        public virtual bool HandleKey(G10_SolarShooterMenu menu, Key key) { return false; }

        // Default for sub screens: back leads to the main menu.
        public virtual void HandleBack(G10_SolarShooterMenu menu) { menu.ReturnToMainMenu(); }

        protected static bool IsLeft(Key key) { return key == Key.Left || key == Key.A; }
        protected static bool IsRight(Key key) { return key == Key.Right || key == Key.D; }
        protected static bool IsUp(Key key) { return key == Key.Up || key == Key.W; }
        protected static bool IsDown(Key key) { return key == Key.Down || key == Key.S; }
        protected static bool IsConfirm(Key key) { return key == Key.Enter || key == Key.Z; }
    }

    // A selectable menu entry (Command pattern): label plus the action it triggers.
    internal sealed class G10_SolarShooterMenuEntry
    {
        readonly Action<G10_SolarShooterMenu> _Activate;

        public G10_SolarShooterMenuEntry(string label, Action<G10_SolarShooterMenu> activate)
        {
            if (activate == null) throw new ArgumentNullException("activate");
            Label = label;
            _Activate = activate;
        }

        public string Label { get; }
        public void Activate(G10_SolarShooterMenu menu) { _Activate(menu); }
    }

    // Template Method for list screens: up/down moves the cursor with wrap-around,
    // confirm activates the selected entry. Subclasses only supply their entries.
    internal abstract class G10_SolarShooterListPhase : G10_SolarShooterMenuPhase
    {
        public abstract IReadOnlyList<G10_SolarShooterMenuEntry> Entries(G10_SolarShooterMenu menu);

        public override bool HandleKey(G10_SolarShooterMenu menu, Key key)
        {
            IReadOnlyList<G10_SolarShooterMenuEntry> entries = Entries(menu);
            if (IsUp(key) || IsDown(key))
            {
                menu.MoveSelection(IsDown(key) ? 1 : -1, entries.Count);
                return true;
            }
            if (IsConfirm(key))
            {
                entries[menu.Selection].Activate(menu);
                return true;
            }
            return false;
        }
    }

    internal sealed class G10_SolarShooterMainPhase : G10_SolarShooterListPhase
    {
        static readonly G10_SolarShooterMenuEntry[] MainEntries =
        {
            new G10_SolarShooterMenuEntry("NEUES SPIEL", menu => menu.RequestNewGame()),
            new G10_SolarShooterMenuEntry("FORTSETZEN", menu => menu.RequestContinue()),
            new G10_SolarShooterMenuEntry("MODUS WAEHLEN", menu => menu.OpenModeSelection()),
            new G10_SolarShooterMenuEntry("ANLEITUNG", menu => menu.OpenHelp()),
            new G10_SolarShooterMenuEntry("BESTENLISTE", menu => menu.OpenLeaderboard()),
            new G10_SolarShooterMenuEntry("ZURUECK", menu => menu.RequestExit())
        };

        public override G10_SolarShooterMenuScreen Screen { get { return G10_SolarShooterMenuScreen.Main; } }
        public override IReadOnlyList<G10_SolarShooterMenuEntry> Entries(G10_SolarShooterMenu menu) { return MainEntries; }
        public override void HandleBack(G10_SolarShooterMenu menu) { menu.RequestExit(); }
    }

    internal sealed class G10_SolarShooterModePhase : G10_SolarShooterListPhase
    {
        public override G10_SolarShooterMenuScreen Screen { get { return G10_SolarShooterMenuScreen.ModeSelection; } }

        // One entry per available mode, so a new mode needs no change here.
        public override IReadOnlyList<G10_SolarShooterMenuEntry> Entries(G10_SolarShooterMenu menu)
        {
            List<G10_SolarShooterMenuEntry> entries = new List<G10_SolarShooterMenuEntry>();
            for (int i = 0; i < menu.Modes.Count; i++)
            {
                int modeIndex = i;
                entries.Add(new G10_SolarShooterMenuEntry(menu.Modes[i].Name, m => m.SelectMode(modeIndex)));
            }
            return entries;
        }
    }

    internal sealed class G10_SolarShooterPausedPhase : G10_SolarShooterListPhase
    {
        static readonly G10_SolarShooterMenuEntry[] PauseEntries =
        {
            new G10_SolarShooterMenuEntry("FORTSETZEN", menu => menu.BeginGameplay()),
            new G10_SolarShooterMenuEntry("ZUM MENUE", menu => menu.LeaveRun())
        };

        public override G10_SolarShooterMenuScreen Screen { get { return G10_SolarShooterMenuScreen.Paused; } }
        public override IReadOnlyList<G10_SolarShooterMenuEntry> Entries(G10_SolarShooterMenu menu) { return PauseEntries; }
        public override void HandleBack(G10_SolarShooterMenu menu) { menu.BeginGameplay(); }
    }

    internal sealed class G10_SolarShooterLeaderboardPhase : G10_SolarShooterMenuPhase
    {
        public override G10_SolarShooterMenuScreen Screen { get { return G10_SolarShooterMenuScreen.Leaderboard; } }

        public override bool HandleKey(G10_SolarShooterMenu menu, Key key)
        {
            if (!IsLeft(key) && !IsRight(key)) return false;
            menu.CycleMode(IsRight(key) ? 1 : -1);
            return true;
        }
    }

    internal sealed class G10_SolarShooterHelpPhase : G10_SolarShooterMenuPhase
    {
        public override G10_SolarShooterMenuScreen Screen { get { return G10_SolarShooterMenuScreen.Help; } }

        public override bool HandleKey(G10_SolarShooterMenu menu, Key key)
        {
            if (IsUp(key) || IsLeft(key))
            {
                menu.PreviousHelpPage();
                return true;
            }
            if (IsDown(key) || IsRight(key) || IsConfirm(key))
            {
                if (menu.IsLastHelpPage) menu.CloseHelp();
                else menu.NextHelpPage();
                return true;
            }
            return false;
        }

        public override void HandleBack(G10_SolarShooterMenu menu) { menu.CloseHelp(); }
    }

    // While playing, the game itself reads the keys; the menu ignores them.
    internal sealed class G10_SolarShooterPlayingPhase : G10_SolarShooterMenuPhase
    {
        public override G10_SolarShooterMenuScreen Screen { get { return G10_SolarShooterMenuScreen.Playing; } }
    }

    internal static class G10_SolarShooterMenuPhases
    {
        public static readonly G10_SolarShooterMainPhase Main = new G10_SolarShooterMainPhase();
        public static readonly G10_SolarShooterModePhase ModeSelection = new G10_SolarShooterModePhase();
        public static readonly G10_SolarShooterLeaderboardPhase Leaderboard = new G10_SolarShooterLeaderboardPhase();
        public static readonly G10_SolarShooterHelpPhase Help = new G10_SolarShooterHelpPhase();
        public static readonly G10_SolarShooterPlayingPhase Playing = new G10_SolarShooterPlayingPhase();
        public static readonly G10_SolarShooterPausedPhase Paused = new G10_SolarShooterPausedPhase();
    }
}
