using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SudokuLib.GameLogic;

namespace Sudoku.Tests;

[TestClass]
public class AdvancedAlgorithmsTests
{
    [TestMethod]
    public void FindNakedQuads_RemovesQuadValuesFromOtherCells()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].UnionWith(new[] { 1, 2 });
        candidates[0, 1].UnionWith(new[] { 1, 3 });
        candidates[0, 2].UnionWith(new[] { 2, 4 });
        candidates[0, 3].UnionWith(new[] { 3, 4 });
        candidates[0, 4].UnionWith(new[] { 1, 5 });

        var changed = AdvancedAlgorithms.FindNakedQuads(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsFalse(candidates[0, 4].Contains(1));
        Assert.IsTrue(candidates[0, 4].Contains(5));
    }

    [TestMethod]
    public void FindSwordfish_RemovesCandidateFromOtherRows()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(5);
        candidates[0, 1].Add(5);
        candidates[1, 1].Add(5);
        candidates[1, 2].Add(5);
        candidates[2, 0].Add(5);
        candidates[2, 2].Add(5);
        candidates[3, 0].Add(5);

        var changed = AdvancedAlgorithms.FindSwordfish(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsFalse(candidates[3, 0].Contains(5));
    }

    [TestMethod]
    public void FindXWings_RemovesCandidateFromOtherColumns()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(5);
        candidates[1, 0].Add(5);
        candidates[0, 1].Add(5);
        candidates[1, 1].Add(5);
        candidates[3, 0].Add(5);

        var changed = AdvancedAlgorithms.FindXWings(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsFalse(candidates[3, 0].Contains(5));
    }

    [TestMethod]
    public void FindXYZWing_RemovesSharedWingCandidate()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].UnionWith(new[] { 1, 2, 3 });
        candidates[0, 1].UnionWith(new[] { 1, 3 });
        candidates[1, 0].UnionWith(new[] { 2, 3 });
        candidates[1, 1].Add(3);

        var changed = AdvancedAlgorithms.FindXYZWing(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsFalse(candidates[1, 1].Contains(3));
    }

    [TestMethod]
    public void FindYWings_RemovesSharedWingCandidate()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].UnionWith(new[] { 1, 2 });
        candidates[0, 1].UnionWith(new[] { 1, 3 });
        candidates[1, 0].UnionWith(new[] { 2, 3 });
        candidates[1, 1].Add(3);

        var changed = AdvancedAlgorithms.FindYWings(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsFalse(candidates[1, 1].Contains(3));
    }

    [TestMethod]
    public void AdvancedAlgorithms_ReturnFalseForEmptyCandidateState()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();

        Assert.IsFalse(AdvancedAlgorithms.FindHiddenQuads(grid, candidates));
        Assert.IsFalse(AdvancedAlgorithms.FindNakedQuads(grid, candidates));
        Assert.IsFalse(AdvancedAlgorithms.FindRectangleElimination(grid, candidates));
        Assert.IsFalse(AdvancedAlgorithms.FindSimpleColouring(grid, candidates));
        Assert.IsFalse(AdvancedAlgorithms.FindSwordfish(grid, candidates));
        Assert.IsFalse(AdvancedAlgorithms.FindUniqueRectangles(grid, candidates));
        Assert.IsFalse(AdvancedAlgorithms.FindWWings(grid, candidates));
        Assert.IsFalse(AdvancedAlgorithms.FindXYZWing(grid, candidates));
        Assert.IsFalse(AdvancedAlgorithms.FindYWings(grid, candidates));
    }

    private static HashSet<int>[,] CreateCandidates()
    {
        var candidates = new HashSet<int>[9, 9];
        for (var row = 0; row < 9; row++)
            for (var column = 0; column < 9; column++)
                candidates[row, column] = new HashSet<int>();
        return candidates;
    }
}
