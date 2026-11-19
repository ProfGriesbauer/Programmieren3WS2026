using System;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_PongAI
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: KI-Gegner für das Pong-Duell. Strategie (nachvollziehbar, keine reine Zufallswahl):
    //               1. Fliegt der Ball auf die KI zu, wird der Auftreffpunkt inkl. Wandabprallern vorausberechnet.
    //               2. Pro Ballwechsel gibt es einen kleinen Zielfehler (Schwierigkeit), sonst wäre sie unschlagbar.
    //               3. Die KI bewegt sich mit begrenzter Geschwindigkeit (langsamer als der Spieler).
    //               4. Fliegt der Ball weg, kehrt sie langsam zur Mitte zurück.
    // =====================================================
    public sealed class G1C_PongAI
    {
        public const double MaxSpeed = 3.2;
        private const double ReturnSpeed = 1.5;
        private readonly Random _random;
        private double _aimError;

        public G1C_PongAI(Random random, double maxError = 14)
        {
            _random = random ?? new Random();
            MaxError = maxError;
            NewRally();
        }

        public double MaxError { get; }

        // Entwickler: Patrick Lupierz – neuer Ballwechsel: neuen Zielfehler würfeln
        public void NewRally()
        {
            _aimError = (_random.NextDouble() * 2 - 1) * MaxError;
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Auftreffhöhe des Balls bei paddleX vorausberechnen (Abpraller an Ober- und Unterkante gespiegelt).
        public static double PredictY(double ballX, double ballY, double vx, double vy, double paddleX, double fieldH)
        {
            if (Math.Abs(vx) < 1e-6) return ballY;
            double t = (paddleX - ballX) / vx;
            if (t < 0) return ballY;
            double y = ballY + vy * t;
            double period = 2 * fieldH;
            y = ((y % period) + period) % period;
            return y > fieldH ? period - y : y;
        }

        // Entwickler: Patrick Lupierz – neue Schlägermitte für diesen Tick
        public double Move(double paddleCenter, double ballX, double ballY, double vx, double vy, double paddleX, double fieldH)
        {
            bool incoming = (paddleX - ballX) * vx > 0;
            double target = incoming ? PredictY(ballX, ballY, vx, vy, paddleX, fieldH) + _aimError : fieldH / 2;
            double speed = incoming ? MaxSpeed : ReturnSpeed;
            double delta = Math.Max(-speed, Math.Min(speed, target - paddleCenter));
            return paddleCenter + delta;
        }
    }
}
