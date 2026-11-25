using System;
using System.Windows.Input;

namespace OOPGames
{
    /// <summary>
    /// Übersetzt Maus- und Tastatureingaben in die spielinternen Hay-Day-Züge.
    /// </summary>
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
            if (click != null && (click.ChangedButton == 0 || click.ChangedButton == 1 || click.ChangedButton == 2))
            {
                HayDayPainter painter = OOPGamesManager.Singleton.ActivePainter as HayDayPainter;
                if (painter == null)
                {
                    return null;
                }

                int menuHit = painter.MenuHitTest(click.XClickPos, click.YClickPos, field);
                bool menuClick = menuHit >= 0 || menuHit == HayDayField.ShopMenuOption;
                int button = click.ChangedButton == 2 ? 0 : click.ChangedButton;
                FarmBuilding clickedBuilding = button == 1
                    ? painter.BuildingAt(click.XClickPos, click.YClickPos, field)
                    : null;
                PointOnFarm position = clickedBuilding == null
                    ? painter.FarmPosition(click.XClickPos, click.YClickPos, field)
                    : field.BuildingPosition(clickedBuilding);
                return new HayDayMove(_playerNumber, HayDayMoveKind.Click, position.Row,
                    position.Column, button, Key.None, menuClick,
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
                || key == Key.D3 || key == Key.D4 || key == Key.D5 || key == Key.NumPad1
                || key == Key.NumPad2 || key == Key.NumPad3 || key == Key.NumPad4
                || key == Key.NumPad5 || key == Key.Escape;
        }
    }
}
