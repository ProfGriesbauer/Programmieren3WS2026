//p06.11.2026
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // Connects the arena implementations to the framework's existing selection lists.
    internal static class ArenaGameRegistration
    {
        // Register on module load so the arena is available without changing the existing window.
        [ModuleInitializer]
        internal static void RegisterArenaGame()
        {
            OOPGamesManager.Singleton.RegisterPainter(new ArenaPainter());
            OOPGamesManager.Singleton.RegisterRules(new ArenaRules());
            OOPGamesManager.Singleton.RegisterPlayer(new ArenaPlayer());
        }
    }
}