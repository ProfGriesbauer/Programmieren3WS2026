namespace OOPGames
{
    /// <summary>Spielfeld für das Framework: enthält den Spielzustand und die aktuelle Bildschirm-Skalierung.</summary>
    public class G1C_CasinoField : IGameField
    {
        public G1C_GameManager Manager { get; set; } = new G1C_GameManager();

        /// <summary>Wird vom Painter bei jedem Zeichnen gesetzt; damit werden Klicks umgerechnet.</summary>
        public G1C_Viewport Viewport { get; set; } = new G1C_Viewport();

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter is IG1C_CasinoPainter;
        }
    }

    /// <summary>Ein "Zug" = eine Eingabe (Taste oder Klick), die über das Framework hereinkommt.</summary>
    public class G1C_CasinoMove : IPlayMove
    {
        public G1C_CasinoMove(int playerNumber, IMoveSelection selection)
        {
            PlayerNumber = playerNumber;
            Selection = selection;
        }

        public int PlayerNumber { get; }
        public IMoveSelection Selection { get; }
    }

    /// <summary>
    /// Framework-Regeln (IGameRules2) des Casino-Spiels.
    ///  - DoMove:          leitet jede Eingabe an den GameManager weiter
    ///  - TickGameCall:    Echtzeit-Update alle 40 ms (Laufen, Animationen, KI-Züge)
    ///  - ClearField:      startet ein neues Casino-Spiel (beim Klick auf "Start New Game")
    /// Es gibt keinen Gewinner im Framework-Sinn (-1), Eingaben sind immer möglich.
    /// </summary>
    public class G1C_CasinoRules : IG1C_CasinoRules
    {
        private readonly G1C_CasinoField _field = new G1C_CasinoField();
        private readonly G1C_InputManager _input = new G1C_InputManager();

        public string Name { get { return "Gruppe 1 - Casino (Spiel 2)"; } }

        public IGameField CurrentField { get { return _field; } }

        public bool MovesPossible { get { return true; } }

        public void DoMove(IPlayMove move)
        {
            G1C_CasinoMove casinoMove = move as G1C_CasinoMove;
            if (casinoMove == null || casinoMove.Selection == null) return;

            G1C_InputEvent input = _input.Translate(casinoMove.Selection, _field.Viewport);
            if (input != null) _field.Manager.HandleInput(input);
        }

        public void ClearField()
        {
            _field.Manager = new G1C_GameManager();
        }

        public int CheckIfPLayerWon()
        {
            return -1;
        }

        public void StartedGameCall()
        {
            // Zurücksetzen passiert in ClearField, das das Framework direkt danach aufruft
        }

        public void TickGameCall()
        {
            _field.Manager.Update();
        }
    }
}
