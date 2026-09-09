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
    private string _hardestAlgorithmUsed = "Naked/Hidden Singles";

    public bool IsSolved => _isSolved;
    public int HighestTierUsed => _highestTierUsed;
    public string HardestAlgorithmUsed => _hardestAlgorithmUsed;

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
        { 3, "Hard (Subsets / Box-Line Reduction)" },
        { 4, "Expert (Advanced Strategy / X-Wings)" },
        { 5, "Diabolical (Requires Advanced Chains or Brute Force)" }
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
            if (BeginnerAlgorithms.FindNakedSingles(_grid, _candidates))
            {
                RecordAlgorithmUsage(1, "Naked Singles");
                continue;
            }
            if (BeginnerAlgorithms.FindHiddenSingles(_grid, _candidates))
            {
                RecordAlgorithmUsage(1, "Hidden Singles");
                continue;
            }

            // Tier 2 Escalation (Intermediate)
            if (IntermediateAlgorithms.FindPointingPairs(_grid, _candidates))
            {
                RecordAlgorithmUsage(2, "Pointing Pairs");
                continue;
            }
            if (IntermediateAlgorithms.FindNakedPairs(_grid, _candidates))
            {
                RecordAlgorithmUsage(2, "Naked Pairs");
                continue;
            }

            //Tier 3 Escalation (Intermediate-Advanced)
            if (IntermediateAlgorithms.FindHiddenPairs(_grid, _candidates))
            {
                RecordAlgorithmUsage(3, "Hidden Pairs");
                continue;
            }
            if (IntermediateAlgorithms.FindBoxLineReduction(_grid, _candidates))
            {
                RecordAlgorithmUsage(3, "Box/Line Reduction");
                continue;
            }
            if (IntermediateAlgorithms.FindNakedTriples(_grid, _candidates))
            {
                RecordAlgorithmUsage(3, "Naked Triples");
                continue;
            }
            if (IntermediateAlgorithms.FindHiddenTriples(_grid, _candidates))
            {
                RecordAlgorithmUsage(3, "Hidden Triples");
                continue;
            }
            if (IntermediateAlgorithms.FindBugPlusOne(_grid, _candidates))
            {
                RecordAlgorithmUsage(3, "BUG+1");
                continue;
            }

            // Tier 4 Escalation (Advanced)
            if (AdvancedAlgorithms.FindXWings(_grid, _candidates))
            {
                RecordAlgorithmUsage(4, "X-Wing");
                continue;
            }
            if (AdvancedAlgorithms.FindUniqueRectangles(_grid, _candidates))
            {
                RecordAlgorithmUsage(4, "Unique Rectangle");
                continue;
            }
            if (AdvancedAlgorithms.FindYWings(_grid, _candidates))
            {
                RecordAlgorithmUsage(4, "Y-Wing");
                continue;
            }
            if (AdvancedAlgorithms.FindWWings(_grid, _candidates))
            {
                RecordAlgorithmUsage(4, "W-Wing");
                continue;
            }
            if (AdvancedAlgorithms.FindRectangleElimination(_grid, _candidates))
            {
                RecordAlgorithmUsage(4, "Rectangle Elimination");
                continue;
            }
            if (AdvancedAlgorithms.FindSimpleColouring(_grid, _candidates))
            {
                RecordAlgorithmUsage(4, "Simple Colouring");
                continue;
            }
            if (AdvancedAlgorithms.FindNakedQuads(_grid, _candidates))
            {
                RecordAlgorithmUsage(4, "Naked Quads");
                continue;
            }
            if (AdvancedAlgorithms.FindHiddenQuads(_grid, _candidates))
            {
                RecordAlgorithmUsage(4, "Hidden Quads");
                continue;
            }
            if (AdvancedAlgorithms.FindSwordfish(_grid, _candidates))
            {
                RecordAlgorithmUsage(4, "Swordfish");
                continue;
            }
            if (AdvancedAlgorithms.FindXYZWing(_grid, _candidates))
            {
                RecordAlgorithmUsage(4, "XYZ-Wing");
                continue;
            }

            // Tier 4 Default (If logic loop gets stuck, requires chains, guessing, or brute force)
            RecordAlgorithmUsage(5, "Brute Force / Advanced Chains");
            break;
        }

        // Map highest tier activated to a final readable string
        return TierMapping[_highestTierUsed];
    }

    // Tracks the highest difficulty tier reached and the specific strategy that triggered it.
    private void RecordAlgorithmUsage(int tier, string algorithmName)
    {
        if (tier >= _highestTierUsed)
        {
            _hardestAlgorithmUsed = algorithmName;
        }
        _highestTierUsed = Math.Max(_highestTierUsed, tier);
    }
}
