//P06.11.2026
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
   // Receives keyboard input from the host window and applies it to the arena field.
    public sealed class HumanPlayerSteering : IHumanGamePlayer
    {
        private int _playerNumber;

        public string Name { get { return "Arena Player (WASD)"; } }
        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            // Keep the framework-required player number for compatibility.
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            // This player implementation is only compatible with arena rules.
            return rules is ArenaRules;
        }

        public IGamePlayer Clone()
        {
            // The framework creates a separate player instance when starting a game.
            HumanPlayerSteering clone = new HumanPlayerSteering();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            // The framework packages keyboard input and the active field into this call.
           // Null prevents the host's turn-based logic from switching players.
           return null;
        }
    }
}