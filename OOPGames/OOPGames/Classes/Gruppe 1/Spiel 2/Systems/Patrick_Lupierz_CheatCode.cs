using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_CheatCode
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: Admin-Cheat (SprintPlanning): Das Wort ADMIN, am Spielautomaten
    //               eingetippt (A D M I N), schaltet die Jackpot-Garantie an bzw. wieder aus.
    //               Änderung 16.11.2026: vorher A B A B ..., dann Pfeiltasten; jetzt das Wort ADMIN (Wunsch Patrick), nur im Spielautomaten. Die Folge wird unabhängig vom Spielzustand
    //               mitgelesen; jede andere Taste dazwischen setzt sie zurück. Wirkung: G1C_Slots.GuaranteedJackpot (100 % Jackpot) und
    //               dauerhaft erhöhte Laufgeschwindigkeit (SpeedStepTicks), bis ADMIN erneut getippt wird.
    // =====================================================
    public sealed class G1C_CheatCode
    {
        public static readonly Key[] Sequence = { Key.A, Key.D, Key.M, Key.I, Key.N };

        private readonly List<Key> _recent = new List<Key>();

        /// <summary>Schritt-Ticks bei aktivem Cheat (Gehen 5, Fahrrad 3): dauerhaftes Turbo-Tempo. 16.11.2026</summary>
        public const int SpeedStepTicks = 2;

        public bool JackpotActive { get; private set; }

        /// <summary>Taste mitlesen. Gibt true zurück, wenn die Folge gerade vollständig eingegeben wurde.</summary>
        public bool Feed(Key key)
        {
            _recent.Add(key);
            if (_recent.Count > Sequence.Length) _recent.RemoveAt(0);
            if (_recent.Count < Sequence.Length || !_recent.SequenceEqual(Sequence)) return false;
            _recent.Clear();
            JackpotActive = !JackpotActive;
            return true;
        }
    }
}
