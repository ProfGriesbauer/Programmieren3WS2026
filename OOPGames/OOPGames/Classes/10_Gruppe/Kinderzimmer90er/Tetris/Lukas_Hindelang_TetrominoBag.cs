using System;
using System.Collections.Generic;

namespace OOPGames
{
    // 7-bag randomizer with a preview queue: every shape comes once per bag of seven.
    internal sealed class G10_TetrominoBag
    {
        readonly Random _Random;
        readonly IReadOnlyList<G10_Tetromino> _AllPieces = G10_Tetromino.CreateAll();
        readonly List<G10_Tetromino> _Queue = new List<G10_Tetromino>();

        public G10_TetrominoBag(Random random)
        {
            if (random == null) throw new ArgumentNullException("random");
            _Random = random;
        }

        public IReadOnlyList<G10_Tetromino> Upcoming { get { return _Queue; } }

        public void Reset()
        {
            _Queue.Clear();
            Fill();
        }

        public G10_Tetromino Draw()
        {
            Fill();
            G10_Tetromino piece = _Queue[0];
            _Queue.RemoveAt(0);
            Fill();
            return piece;
        }

        void Fill()
        {
            while (_Queue.Count <= G10_TetrisConfig.PreviewCount)
            {
                int start = _Queue.Count;
                _Queue.AddRange(_AllPieces);
                for (int i = _Queue.Count - 1; i > start; i--)
                {
                    int swapIndex = start + _Random.Next(i - start + 1);
                    G10_Tetromino piece = _Queue[i];
                    _Queue[i] = _Queue[swapIndex];
                    _Queue[swapIndex] = piece;
                }
            }
        }
    }
}
