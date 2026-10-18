using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuLib.GameObjects
{
    // Remaining counts are derived from the board's cells on every read, so they can't drift
    // regardless of how a cell's value changes (set, replace, clear, invalid entry, board reset).
    public class Pile
    {
        private readonly Func<IEnumerable<int>> _cellValues;

        public Pile(Func<IEnumerable<int>> cellValues)
        {
            _cellValues = cellValues ?? throw new ArgumentNullException(nameof(cellValues));
        }

        public IReadOnlyDictionary<int, int> Values
        {
            get
            {
                var remaining = Enumerable.Range(1, 9).ToDictionary(i => i, i => 9);
                foreach (var value in _cellValues())
                {
                    if (value >= 1 && value <= 9)
                    {
                        remaining[value]--;
                    }
                }
                return remaining;
            }
        }
    }
}
