using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SudokuLib.GameLogic;

namespace Sudoku.Tests;

[TestClass]
public class IntermediateAlgorithmsTests
{
    [TestMethod]
    public void FindPointingPairs_RemovesCandidateFromRestOfSharedRow()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(5);
        candidates[0, 1].Add(5);
        candidates[0, 3].Add(5);

        var changed = IntermediateAlgorithms.FindPointingPairs(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsFalse(candidates[0, 3].Contains(5));
    }

    [TestMethod]
    public void FindPointingPairs_RemovesCandidateForThreeCellsInSharedRow()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[3, 3].Add(7);
        candidates[3, 4].Add(7);
        candidates[3, 5].Add(7);
        candidates[3, 8].Add(7);

        var changed = IntermediateAlgorithms.FindPointingPairs(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsFalse(candidates[3, 8].Contains(7));
    }

    [TestMethod]
    public void FindPointingPairs_DoesNotChangeCandidateOutsideSharedRow()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(5);
        candidates[0, 1].Add(5);
        candidates[1, 3].Add(5);

        var changed = IntermediateAlgorithms.FindPointingPairs(grid, candidates);

        Assert.IsFalse(changed);
        Assert.IsTrue(candidates[1, 3].Contains(5));
    }

    [TestMethod]
    public void FindPointingPairs_RequiresAtLeastTwoCellsInTheBox()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(5);
        candidates[0, 3].Add(5);

        var changed = IntermediateAlgorithms.FindPointingPairs(grid, candidates);

        Assert.IsFalse(changed);
        Assert.IsTrue(candidates[0, 3].Contains(5));
    }

    [TestMethod]
    public void FindPointingPairs_ReturnsFalseWhenThereIsNothingToRemove()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(5);
        candidates[0, 1].Add(5);

        var changed = IntermediateAlgorithms.FindPointingPairs(grid, candidates);

        Assert.IsFalse(changed);
    }

    [TestMethod]
    public void FindPointingPairs_RemovesCandidateFromRestOfSharedColumn()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(5);
        candidates[1, 0].Add(5);
        candidates[3, 0].Add(5);

        var changed = IntermediateAlgorithms.FindPointingPairs(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsFalse(candidates[3, 0].Contains(5));
    }

    [TestMethod]
    public void FindBoxLineReduction_RemovesCandidateFromRestOfSharedBoxRow()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(5);
        candidates[0, 1].Add(5);
        candidates[1, 2].Add(5);
        candidates[0, 3].Add(6);

        var changed = IntermediateAlgorithms.FindBoxLineReduction(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsFalse(candidates[1, 2].Contains(5));
        Assert.IsTrue(candidates[0, 3].Contains(6));
    }

    [TestMethod]
    public void FindBoxLineReduction_RemovesCandidateFromRestOfSharedBoxColumn()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(7);
        candidates[1, 0].Add(7);
        candidates[2, 2].Add(7);
        candidates[3, 0].Add(8);

        var changed = IntermediateAlgorithms.FindBoxLineReduction(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsFalse(candidates[2, 2].Contains(7));
        Assert.IsTrue(candidates[3, 0].Contains(8));
    }

    [TestMethod]
    public void FindBoxLineReduction_DoesNotChangeBoxWhenCandidatesSpanMultipleRows()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(5);
        candidates[1, 1].Add(5);
        candidates[2, 2].Add(5);

        var changed = IntermediateAlgorithms.FindBoxLineReduction(grid, candidates);

        Assert.IsFalse(changed);
        Assert.IsTrue(candidates[2, 2].Contains(5));
    }

    [TestMethod]
    public void FindBoxLineReduction_ReturnsFalseWhenThereIsNothingToRemove()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(5);
        candidates[0, 1].Add(5);

        var changed = IntermediateAlgorithms.FindBoxLineReduction(grid, candidates);

        Assert.IsFalse(changed);
    }

    private static HashSet<int>[,] CreateCandidates()
    {
        var candidates = new HashSet<int>[9, 9];
        for (var row = 0; row < 9; row++)
        {
            for (var column = 0; column < 9; column++)
            {
                candidates[row, column] = new HashSet<int>();
            }
        }

        return candidates;
    }
}
