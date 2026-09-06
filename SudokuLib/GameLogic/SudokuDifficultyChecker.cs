using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuLib.GameLogic;

public class SudokuDifficultyChecker
{
    private readonly int[,] _grid;
    private readonly HashSet<int>[,] _candidates;
    private int _highestTierUsed = 1;
    private bool _isSolved = false;

    public bool IsSolved => _isSolved;
    public int HighestTierUsed => _highestTierUsed;

    // puzzle is a 9x9 grid where 0 represents a hole
    public SudokuDifficultyChecker(int[,] puzzle)
    {
        _grid = (int[,])puzzle.Clone();

        // Keep track of potential values for each cell (1-9)
        _candidates = new HashSet<int>[9, 9];
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                _candidates[r, c] = puzzle[r, c] == 0
                    ? new HashSet<int>(Enumerable.Range(1, 9))
                    : new HashSet<int> { puzzle[r, c] };
            }
        }
    }

    // Standard constraint propagation: eliminate known cell values from row/col/box.
    public bool RefreshCandidates()
    {
        bool changed = false;
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                if (_candidates[r, c].Count == 1)
                {
                    int val = _candidates[r, c].First();

                    // Update the grid if empty
                    if (_grid[r, c] == 0)
                    {
                        _grid[r, c] = val;
                        changed = true;
                    }

                    // Remove from row and column
                    for (int i = 0; i < 9; i++)
                    {
                        if (i != c && _candidates[r, i].Remove(val))
                        {
                            changed = true;
                        }
                        if (i != r && _candidates[i, c].Remove(val))
                        {
                            changed = true;
                        }
                    }

                    // Remove from 3x3 box
                    int br = 3 * (r / 3);
                    int bc = 3 * (c / 3);
                    for (int i = br; i < br + 3; i++)
                    {
                        for (int j = bc; j < bc + 3; j++)
                        {
                            if ((i != r || j != c) && _candidates[i, j].Remove(val))
                            {
                                changed = true;
                            }
                        }
                    }
                }
            }
        }
        return changed;
    }

    private static readonly Dictionary<int, string> TierMapping = new()
    {
        { 1, "Easy (Naked/Hidden Singles Only)" },
        { 2, "Medium (Intersections / Pointing Pairs)" },
        { 3, "Hard (Advanced Strategy / X-Wings)" },
        { 4, "Expert/Diabolic (Requires Advanced Chains or Brute Force)" }
    };

    // Iteratively solves the board strictly by tier level escalation.
    public string CheckDifficulty()
    {
        while (true)
        {
            // Always propagate standard cell completions first
            while (RefreshCandidates())
            {
            }

            // Check if completely filled
            bool isFull = true;
            for (int r = 0; r < 9 && isFull; r++)
            {
                for (int c = 0; c < 9; c++)
                {
                    if (_grid[r, c] == 0)
                    {
                        isFull = false;
                        break;
                    }
                }
            }
            if (isFull)
            {
                _isSolved = true;
                break;
            }

            // Tier 1 Escalation
            if (BeginnerAlgorithms.FindNakedSingles(_grid, _candidates) 
                || BeginnerAlgorithms.FindHiddenSingles(_grid, _candidates))
            {
                _highestTierUsed = 1;
                continue;
            }

            // Tier 2 Escalation (Intermediate)
            if (IntermediateAlgorithms.FindPointingPairs(_grid, _candidates) 
                || IntermediateAlgorithms.FindNakedPairs(_grid, _candidates) 
                || IntermediateAlgorithms.FindHiddenPairs(_grid, _candidates) 
                || IntermediateAlgorithms.FindBoxLineReduction(_grid, _candidates) 
                || IntermediateAlgorithms.FindNakedTriples(_grid, _candidates) 
                || IntermediateAlgorithms.FindHiddenTriples(_grid, _candidates)
                || IntermediateAlgorithms.FindBugPlusOne(_grid, _candidates))
            {

                _highestTierUsed = Math.Max(_highestTierUsed, 2);
                continue;
            }

            // Tier 3 Escalation (Advanced)
            if (AdvancedAlgorithms.FindXWings(_grid, _candidates) 
                || AdvancedAlgorithms.FindUniqueRectangles(_grid, _candidates) 
                || AdvancedAlgorithms.FindYWings(_grid, _candidates) 
                || AdvancedAlgorithms.FindWWings(_grid, _candidates) 
                || AdvancedAlgorithms.FindRectangleElimination(_grid, _candidates)
                || AdvancedAlgorithms.FindSimpleColouring(_grid, _candidates) 
                || AdvancedAlgorithms.FindNakedQuads(_grid, _candidates) 
                || AdvancedAlgorithms.FindHiddenQuads(_grid, _candidates)
                || AdvancedAlgorithms.FindSwordfish(_grid, _candidates) 
                || AdvancedAlgorithms.FindXYZWing(_grid, _candidates))
            {
                _highestTierUsed = Math.Max(_highestTierUsed, 3);
                continue;
            }

            // Tier 4 Default (If logic loop gets stuck, requires chains, guessing, or brute force)
            _highestTierUsed = Math.Max(_highestTierUsed, 4);
            break;
        }

        // Map highest tier activated to a final readable string
        return TierMapping[_highestTierUsed];
    }
}
