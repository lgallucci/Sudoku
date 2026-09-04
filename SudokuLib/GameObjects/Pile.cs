using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuLib.GameObjects
{
    public class Pile
    {
        public Dictionary<int, int> Values { get; private set; }

        public Pile()
        {
            Values = new Dictionary<int, int>();
            for (int i = 1; i <= 9; i++)
            {
                Values[i] = 9; // Each number from 1 to 9 can appear 9 times in a Sudoku puzzle
            }
        }

        public bool TryUseValue(int value)
        {
            if (value < 1 || value > 9)
            {
                throw new ArgumentOutOfRangeException("Value must be between 1 and 9.");
            }

            if (Values[value] > 0)
            {
                Values[value]--;
                return true;
            }
            return false;
        }
    }
}