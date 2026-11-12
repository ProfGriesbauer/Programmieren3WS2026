namespace OOPGames
{
    /// <summary>
    /// Menschlicher Spieler für das Casino. Wandelt jede Tasten- oder Klick-Auswahl
    /// des Frameworks in einen G1C_CasinoMove um; ausgewertet wird sie in G1C_CasinoRules.
    ///
    /// Wichtig: Im Framework als Player 1 UND Player 2 auswählen, weil das Hauptfenster
    /// nach jeder Eingabe den Spieler wechselt.
    /// </summary>
    public class G1C_CasinoHumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber;

        public string Name { get { return "Gruppe 1 - Casino Spieler"; } }

        public int PlayerNumber { get { return _playerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is G1C_CasinoRules;
        }

        public IGamePlayer Clone()
        {
            G1C_CasinoHumanPlayer clone = new G1C_CasinoHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (selection == null || !(field is G1C_CasinoField)) return null;
            return new G1C_CasinoMove(_playerNumber, selection);
        }
    }
}
