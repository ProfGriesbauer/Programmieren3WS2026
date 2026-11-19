using System;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class HayDayHumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber;

        public string Name { get { return "Hay Day Spieler"; } }
        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is HayDayRules;
        }

        public IGamePlayer Clone()
        {
            HayDayHumanPlayer clone = new HayDayHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField gameField)
        {
            HayDayField field = gameField as HayDayField;
            if (field == null || _playerNumber < 1 || _playerNumber > 2 || selection == null)
            {
                return null;
            }

            IClickSelection click = selection as IClickSelection;
            if (click != null && (click.ChangedButton == 0 || click.ChangedButton == 1))
            {
                HayDayPainter painter = OOPGamesManager.Singleton.ActivePainter as HayDayPainter;
                if (painter == null)
                {
                    return null;
                }

                int menuHit = painter.MenuHitTest(click.XClickPos, click.YClickPos, field);
                bool menuClick = menuHit != -1;
                PointOnFarm position = painter.FarmPosition(click.XClickPos, click.YClickPos, field);
                return new HayDayMove(_playerNumber, HayDayMoveKind.Click, position.Row,
                    position.Column, click.ChangedButton, Key.None, menuClick,
                    menuHit == HayDayField.ShopMenuOption
                        ? HayDayField.ShopMenuOption
                        : menuHit >= 0 ? menuHit : -1);
            }

            IKeySelection keySelection = selection as IKeySelection;
            if (keySelection != null && IsSupportedKey(keySelection.Key))
            {
                return new HayDayMove(_playerNumber, HayDayMoveKind.Key, 0, 0, 0, keySelection.Key);
            }

            return null;
        }

        private static bool IsSupportedKey(Key key)
        {
            return key == Key.Left || key == Key.Right || key == Key.Up || key == Key.Down
                || key == Key.Add || key == Key.OemPlus || key == Key.Subtract
                || key == Key.OemMinus || key == Key.B || key == Key.D1 || key == Key.D2
                || key == Key.D3 || key == Key.NumPad1 || key == Key.NumPad2 || key == Key.NumPad3;
        }
    }
}
