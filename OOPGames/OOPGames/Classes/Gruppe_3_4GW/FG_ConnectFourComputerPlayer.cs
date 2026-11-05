using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public sealed class FG_ConnectFourComputerPlayer : IFG_ComputerConnectFourPlayer
    {
        private const int SearchDepth = 4;
        private static readonly Random Random = new Random();
        private int _playerNumber;
    
        public string Name { get { return "FG Computer Vier gewinnt"; } }
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
            FG_ConnectFourComputerPlayer clone = new FG_ConnectFourComputerPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }
    
        public IPlayMove GetMove(IGameField field)
        {
            I4GWFeld gameField = field as I4GWFeld;
            if (gameField == null || _playerNumber < 1)
            {
                return null;
            }
    
            int opponent = _playerNumber == 1 ? 2 : 1;
            int bestScore = int.MinValue;
            List<int> bestColumns = new List<int>();
            for (int column = 0; column < FG_ConnectFourField.Columns; column++)
            {
                int row = FG_ConnectFourBoardLogic.GetLandingRow(gameField, column);
                if (row < 0)
                {
                    continue;
                }
    
                gameField[row, column] = _playerNumber;
                int score = HasWon(gameField, row, column, _playerNumber)
                    ? 1000000
                    : Minimax(gameField, SearchDepth - 1, opponent, false, int.MinValue, int.MaxValue);
                gameField[row, column] = 0;
    
                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumns.Clear();
                    bestColumns.Add(column);
                }
                else if (score == bestScore)
                {
                    bestColumns.Add(column);
                }
            }
    
            if (bestColumns.Count == 0)
            {
                return null;
            }
    
            int selectedColumn = bestColumns[Random.Next(bestColumns.Count)];
            return new FG_ConnectFourMove(
                FG_ConnectFourBoardLogic.GetLandingRow(gameField, selectedColumn),
                selectedColumn,
                _playerNumber);
        }
    
        public IFG_ConnectFourMove GetMove(IFG_ConnectFourField field)
        {
            return GetMove((IGameField)field) as IFG_ConnectFourMove;
        }
    
        private int Minimax(
            I4GWFeld field,
            int depth,
            int player,
            bool maximizing,
            int alpha,
            int beta)
        {
            if (depth == 0)
            {
                return Evaluate(field);
            }
    
            List<int> columns = GetAvailableColumns(field);
            if (columns.Count == 0)
            {
                return 0;
            }
    
            int bestScore = maximizing ? int.MinValue : int.MaxValue;
            foreach (int column in columns)
            {
                int row = FG_ConnectFourBoardLogic.GetLandingRow(field, column);
                field[row, column] = player;
                int score;
                if (HasWon(field, row, column, player))
                {
                    score = maximizing ? 1000000 + depth : -1000000 - depth;
                }
                else
                {
                    int nextPlayer = player == 1 ? 2 : 1;
                    score = Minimax(field, depth - 1, nextPlayer, !maximizing, alpha, beta);
                }
    
                field[row, column] = 0;
    
                if (maximizing)
                {
                    bestScore = Math.Max(bestScore, score);
                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    bestScore = Math.Min(bestScore, score);
                    beta = Math.Min(beta, bestScore);
                }
    
                if (beta <= alpha)
                {
                    break;
                }
            }
    
            return bestScore;
        }
    
        private int Evaluate(I4GWFeld field)
        {
            int opponent = _playerNumber == 1 ? 2 : 1;
            int score = 0;
    
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                if (field[row, FG_ConnectFourField.Columns / 2] == _playerNumber)
                {
                    score += 4;
                }
            }
    
            for (int row = 0; row < FG_ConnectFourField.Rows; row++)
            {
                for (int column = 0; column < FG_ConnectFourField.Columns; column++)
                {
                    score += ScoreWindow(field, row, column, 0, 1, _playerNumber, opponent);
                    score += ScoreWindow(field, row, column, 1, 0, _playerNumber, opponent);
                    score += ScoreWindow(field, row, column, 1, 1, _playerNumber, opponent);
                    score += ScoreWindow(field, row, column, 1, -1, _playerNumber, opponent);
                }
            }
    
            return score;
        }
    
        private int ScoreWindow(
            I4GWFeld field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player,
            int opponent)
        {
            int playerCount = 0;
            int opponentCount = 0;
            int emptyCount = 0;
    
            for (int index = 0; index < 4; index++)
            {
                int nextRow = row + rowStep * index;
                int nextColumn = column + columnStep * index;
                if (nextRow < 0 ||
                    nextRow >= FG_ConnectFourField.Rows ||
                    nextColumn < 0 ||
                    nextColumn >= FG_ConnectFourField.Columns)
                {
                    return 0;
                }
    
                int value = field[nextRow, nextColumn];
                if (value == player)
                {
                    playerCount++;
                }
                else if (value == opponent)
                {
                    opponentCount++;
                }
                else if (value == 0)
                {
                    emptyCount++;
                }
                else
                {
                    return 0;
                }
            }
    
            if (playerCount == 4)
            {
                return 1000000;
            }
            if (opponentCount == 4)
            {
                return -1000000;
            }
            if (playerCount == 3 && emptyCount == 1)
            {
                return 80;
            }
            if (playerCount == 2 && emptyCount == 2)
            {
                return 12;
            }
            if (opponentCount == 3 && emptyCount == 1)
            {
                return -100;
            }
            if (opponentCount == 2 && emptyCount == 2)
            {
                return -15;
            }
    
            return 0;
        }
    
        private List<int> GetAvailableColumns(I4GWFeld field)
        {
            List<int> columns = new List<int>();
            for (int column = 0; column < FG_ConnectFourField.Columns; column++)
            {
                if (field[0, column] == 0)
                {
                    columns.Add(column);
                }
            }
    
            return columns;
        }
    
        private bool HasWon(I4GWFeld field, int row, int column, int player)
        {
            return Count(field, row, column, 0, 1, player) +
                       Count(field, row, column, 0, -1, player) - 1 >= 4 ||
                   Count(field, row, column, 1, 0, player) >= 4 ||
                   Count(field, row, column, 1, 1, player) +
                       Count(field, row, column, -1, -1, player) - 1 >= 4 ||
                   Count(field, row, column, 1, -1, player) +
                       Count(field, row, column, -1, 1, player) - 1 >= 4;
        }
    
        private int Count(
            I4GWFeld field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player)
        {
            int count = 0;
            while (row >= 0 &&
                   row < FG_ConnectFourField.Rows &&
                   column >= 0 &&
                   column < FG_ConnectFourField.Columns &&
                   field[row, column] == player)
            {
                count++;
                row += rowStep;
                column += columnStep;
            }
    
            return count;
        }
    }
}
