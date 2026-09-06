using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SudokuLib.GameLogic;

namespace Sudoku.Tests;

[TestClass]
public class BeginnerAlgorithmsTests
{
    [TestMethod]
    public void FindNakedSingles_ReturnsTrueWhenAnUnsolvedCellHasOneCandidate()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[4, 5].Add(7);

        var changed = BeginnerAlgorithms.FindNakedSingles(grid, candidates);

        Assert.IsTrue(changed);
    }

    [TestMethod]
    public void FindNakedSingles_IgnoresFilledCells()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        grid[4, 5] = 7;
        candidates[4, 5].Add(7);

        var changed = BeginnerAlgorithms.FindNakedSingles(grid, candidates);

        Assert.IsFalse(changed);
    }

    [TestMethod]
    public void FindNakedSingles_ReturnsFalseWhenNoCellHasOneCandidate()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].UnionWith(new[] { 1, 2 });

        var changed = BeginnerAlgorithms.FindNakedSingles(grid, candidates);

        Assert.IsFalse(changed);
    }

    [TestMethod]
    public void FindHiddenSingles_ReducesUniqueRowCandidateToOneValue()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(5);
        candidates[0, 1].Add(1);

        var changed = BeginnerAlgorithms.FindHiddenSingles(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsTrue(candidates[0, 0].SetEquals(new[] { 5 }));
    }

    [TestMethod]
    public void FindHiddenSingles_ReducesUniqueColumnCandidateToOneValue()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        candidates[0, 0].Add(1);
        candidates[1, 0].Add(6);
        candidates[2, 0].Add(6);

        var changed = BeginnerAlgorithms.FindHiddenSingles(grid, candidates);

        Assert.IsTrue(changed);
        Assert.IsTrue(candidates[0, 0].SetEquals(new[] { 1 }));
    }

    [TestMethod]
    public void FindHiddenSingles_IgnoresFilledCells()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        grid[0, 0] = 5;
        candidates[0, 0].Add(5);

        var changed = BeginnerAlgorithms.FindHiddenSingles(grid, candidates);

        Assert.IsFalse(changed);
    }

    [TestMethod]
    public void FindHiddenSingles_ReturnsFalseWhenNoValueHasOneLocation()
    {
        var grid = new int[9, 9];
        var candidates = CreateCandidates();
        for (var row = 0; row < 9; row++)
            for (var column = 0; column < 9; column++)
                candidates[row, column].UnionWith(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
        candidates[0, 0].Clear();
        candidates[0, 0].UnionWith(new[] { 1, 2 });
        candidates[0, 1].Clear();
        candidates[0, 1].UnionWith(new[] { 1, 2 });

        var changed = BeginnerAlgorithms.FindHiddenSingles(grid, candidates);

        Assert.IsFalse(changed);
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
