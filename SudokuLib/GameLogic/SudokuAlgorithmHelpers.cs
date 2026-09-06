using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuLib.GameLogic;

internal static class SudokuAlgorithmHelpers
{
    internal static IEnumerable<List<(int Row, int Column)>> Units()
    {
        for (var row = 0; row < 9; row++)
            yield return Enumerable.Range(0, 9).Select(column => (row, column)).ToList();

        for (var column = 0; column < 9; column++)
            yield return Enumerable.Range(0, 9).Select(row => (row, column)).ToList();

        for (var boxRow = 0; boxRow < 9; boxRow += 3)
        {
            for (var boxColumn = 0; boxColumn < 9; boxColumn += 3)
            {
                yield return (from row in Enumerable.Range(boxRow, 3)
                              from column in Enumerable.Range(boxColumn, 3)
                              select (row, column)).ToList();
            }
        }
    }

    internal static bool IsUnsolved(int[,] grid, int row, int column)
        => grid[row, column] == 0;

    internal static bool Sees((int Row, int Column) first, (int Row, int Column) second)
    {
        if (first == second)
            return false;

        return first.Row == second.Row
            || first.Column == second.Column
            || (first.Row / 3 == second.Row / 3 && first.Column / 3 == second.Column / 3);
    }

    internal static IEnumerable<(int Row, int Column)> Peers((int Row, int Column) cell)
    {
        var peers = new HashSet<(int Row, int Column)>();
        for (var index = 0; index < 9; index++)
        {
            peers.Add((cell.Row, index));
            peers.Add((index, cell.Column));
        }

        var boxRow = cell.Row / 3 * 3;
        var boxColumn = cell.Column / 3 * 3;
        for (var row = boxRow; row < boxRow + 3; row++)
            for (var column = boxColumn; column < boxColumn + 3; column++)
                peers.Add((row, column));

        peers.Remove(cell);
        return peers;
    }

    internal static bool RemoveCandidate(int[,] grid, HashSet<int>[,] candidates, int row, int column, int value)
        => IsUnsolved(grid, row, column) && candidates[row, column].Remove(value);

    internal static bool RemoveCandidates(int[,] grid, HashSet<int>[,] candidates,
        IEnumerable<(int Row, int Column)> cells, IEnumerable<int> values)
    {
        var changed = false;
        foreach (var (row, column) in cells)
        {
            foreach (var value in values.ToList())
                changed |= RemoveCandidate(grid, candidates, row, column, value);
        }

        return changed;
    }
}
