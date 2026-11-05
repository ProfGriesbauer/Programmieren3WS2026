using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    public sealed class Gruppe1_ConnectFourComputerPlayer : IComputerGamePlayer
    {
        private readonly Random _random = new Random();
        private int _playerNumber;

        public string Name { get { return "Gruppe 1 Connect Four Computer Player"; } }

        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }

        public IGamePlayer Clone()
        {
            Gruppe1_ConnectFourComputerPlayer clone = new Gruppe1_ConnectFourComputerPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IGameField gameField)
        {
            I4GWFeld field = gameField as I4GWFeld;
            if (field == null || _playerNumber < 1 || _playerNumber > 2
                || G1_ConnectFourLogic.Winner(field) > 0)
            {
                return null;
            }

            int opponent = _playerNumber == 1 ? 2 : 1;
            List<int> openColumns = GetOpenColumns(field);
            if (openColumns.Count == 0)
            {
                return null;
            }

            foreach (int column in openColumns)
            {
                if (G1_ConnectFourLogic.WinsAfterDrop(field, column, _playerNumber))
                {
                    return CreateMove(field, column);
                }
            }

            foreach (int column in openColumns)
            {
                if (G1_ConnectFourLogic.WinsAfterDrop(field, column, opponent))
                {
                    return CreateMove(field, column);
                }
            }

            List<int> safeColumns = openColumns
                .Where(column => !GivesOpponentImmediateWin(field, column, opponent))
                .ToList();
            List<int> candidates = safeColumns.Count > 0 ? safeColumns : openColumns;
            int bestDistance = candidates.Min(column => Math.Abs(column - G1_ConnectFourLogic.Columns / 2));
            List<int> preferredColumns = candidates
                .Where(column => Math.Abs(column - G1_ConnectFourLogic.Columns / 2) == bestDistance)
                .ToList();

            return CreateMove(field, preferredColumns[_random.Next(preferredColumns.Count)]);
        }

        private static List<int> GetOpenColumns(I4GWFeld field)
        {
            List<int> openColumns = new List<int>();
            for (int column = 0; column < G1_ConnectFourLogic.Columns; column++)
            {
                if (G1_ConnectFourLogic.FreeRow(field, column) >= 0)
                {
                    openColumns.Add(column);
                }
            }

            return openColumns;
        }

        private static bool GivesOpponentImmediateWin(I4GWFeld field, int column, int opponent)
        {
            int row = G1_ConnectFourLogic.FreeRow(field, column);
            if (row < 0)
            {
                return false;
            }

            Gruppe1_ConnectFourField copy = G1_ConnectFourLogic.Copy(field);
            copy[row, column] = opponent == 1 ? 2 : 1;
            return GetOpenColumns(copy)
                .Any(nextColumn => G1_ConnectFourLogic.WinsAfterDrop(copy, nextColumn, opponent));
        }

        private Gruppe1_ConnectFourMove CreateMove(I4GWFeld field, int column)
        {
            return new Gruppe1_ConnectFourMove(
                G1_ConnectFourLogic.FreeRow(field, column), column, _playerNumber);
        }
    }
}
