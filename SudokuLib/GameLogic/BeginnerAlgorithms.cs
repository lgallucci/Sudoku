using System.Collections.Generic;
using System.Linq;

namespace SudokuLib.GameLogic
{
    public static class BeginnerAlgorithms
    {
        public static bool FindNakedSingles(int[,] _grid, HashSet<int>[,] _candidates)
        {
            for (int r = 0; r < 9; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (_grid[r, c] == 0 && _candidates[r, c].Count == 1)
                    {
                        // Found via standard refresh, Tier 1 technique
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool FindHiddenSingles(int[,] _grid, HashSet<int>[,] _candidates)
        {
            // Check rows
            for (int r = 0; r < 9; r++)
            {
                for (int val = 1; val <= 9; val++)
                {
                    var possibleCols = Enumerable.Range(0, 9)
                        .Where(c => _grid[r, c] == 0 && _candidates[r, c].Contains(val))
                        .ToList();
                    if (possibleCols.Count == 1)
                    {
                        _candidates[r, possibleCols[0]] = new HashSet<int> { val };
                        return true;
                    }
                }
            }

            // Check columns
            for (int c = 0; c < 9; c++)
            {
                for (int val = 1; val <= 9; val++)
                {
                    var possibleRows = Enumerable.Range(0, 9)
                        .Where(r => _grid[r, c] == 0 && _candidates[r, c].Contains(val))
                        .ToList();
                    if (possibleRows.Count == 1)
                    {
                        _candidates[possibleRows[0], c] = new HashSet<int> { val };
                        return true;
                    }
                }
            }

            return false;
        }
    }
}