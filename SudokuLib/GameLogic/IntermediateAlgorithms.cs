using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuLib.GameLogic
{
    public static class IntermediateAlgorithms
    {
        // Tier 2: Candidates locked into a single row/col inside a 3x3 block can be
        // eliminated from the rest of that row/col outside the block.
        public static bool FindPointingPairs(int[,] _grid, HashSet<int>[,] _candidates)
        {
            for (int boxR = 0; boxR < 9; boxR += 3)
            {
                for (int boxCol = 0; boxCol < 9; boxCol += 3)
                {
                    for (int val = 1; val <= 9; val++)
                    {
                        var cells = new List<(int Row, int Col)>();
                        for (int r = boxR; r < boxR + 3; r++)
                        {
                            for (int c = boxCol; c < boxCol + 3; c++)
                            {
                                if (_grid[r, c] == 0 && _candidates[r, c].Contains(val))
                                {
                                    cells.Add((r, c));
                                }
                            }
                        }

                        if (cells.Count == 2 || cells.Count == 3)
                        {
                            // Check if they share the same row
                            var rows = cells.Select(cell => cell.Row).Distinct().ToList();
                            if (rows.Count == 1)
                            {
                                int targetRow = rows[0];
                                bool eliminated = false;
                                for (int c = 0; c < 9; c++)
                                {
                                    if (c < boxCol || c >= boxCol + 3)
                                    {
                                        if (_candidates[targetRow, c].Remove(val))
                                        {
                                            eliminated = true;
                                        }
                                    }
                                }
                                if (eliminated)
                                {
                                    return true;
                                }
                            }

                            var columns = cells.Select(cell => cell.Col).Distinct().ToList();
                            if (columns.Count == 1)
                            {
                                int targetColumn = columns[0];
                                bool eliminated = false;
                                for (int r = 0; r < 9; r++)
                                {
                                    if (r < boxR || r >= boxR + 3)
                                    {
                                        if (_candidates[r, targetColumn].Remove(val))
                                            eliminated = true;
                                    }
                                }
                                if (eliminated)
                                    return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        internal static bool FindBoxLineReduction(int[,] grid, HashSet<int>[,] candidates)
        {
            foreach (var unit in SudokuAlgorithmHelpers.Units().Take(18))
            {
                for (var value = 1; value <= 9; value++)
                {
                    var locations = unit.Where(cell => SudokuAlgorithmHelpers.IsUnsolved(grid, cell.Row, cell.Column)
                        && candidates[cell.Row, cell.Column].Contains(value)).ToList();
                    if (locations.Count < 2)
                        continue;

                    var boxes = locations.Select(cell => (cell.Row / 3, cell.Column / 3)).Distinct().ToList();
                    if (boxes.Count != 1)
                        continue;

                    var (boxRow, boxColumn) = boxes[0];
                    var box = SudokuAlgorithmHelpers.Units().Skip(18 + boxRow * 3 + boxColumn).First();
                    if (SudokuAlgorithmHelpers.RemoveCandidates(grid, candidates,
                        box.Except(unit), new[] { value }))
                        return true;
                }
            }

            return false;
        }

        internal static bool FindBugPlusOne(int[,] grid, HashSet<int>[,] candidates)
        {
            var unsolved = Enumerable.Range(0, 9).SelectMany(row => Enumerable.Range(0, 9)
                .Select(column => (row, column)))
                .Where(cell => SudokuAlgorithmHelpers.IsUnsolved(grid, cell.row, cell.column)).ToList();
            var trivalue = unsolved.Where(cell => candidates[cell.row, cell.column].Count == 3).ToList();
            if (trivalue.Count != 1 || unsolved.Any(cell => candidates[cell.row, cell.column].Count != 2
                && candidates[cell.row, cell.column].Count != 3))
                return false;

            var cell = (Row: trivalue[0].row, Column: trivalue[0].column);
            var valid = candidates[cell.Row, cell.Column].Where(value =>
                CountInUnit(cell.Row, cell.Column, value, candidates, grid, true) == 3
                && CountInUnit(cell.Row, cell.Column, value, candidates, grid, false) == 3
                && CountInBox(cell.Row, cell.Column, value, candidates, grid) == 3).ToList();
            return valid.Count == 1 && SudokuAlgorithmHelpers.RemoveCandidates(grid, candidates,
                new[] { cell }, candidates[cell.Row, cell.Column].Except(valid));
        }

        internal static bool FindHiddenPairs(int[,] grid, HashSet<int>[,] candidates)
            => FindHiddenSubset(grid, candidates, 2);

        internal static bool FindHiddenTriples(int[,] grid, HashSet<int>[,] candidates)
            => FindHiddenSubset(grid, candidates, 3);

        internal static bool FindNakedPairs(int[,] grid, HashSet<int>[,] candidates)
            => FindNakedSubset(grid, candidates, 2);

        internal static bool FindNakedTriples(int[,] grid, HashSet<int>[,] candidates)
            => FindNakedSubset(grid, candidates, 3);

        private static bool FindNakedSubset(int[,] grid, HashSet<int>[,] candidates, int size)
        {
            foreach (var unit in SudokuAlgorithmHelpers.Units())
            {
                var cells = unit.Where(cell => SudokuAlgorithmHelpers.IsUnsolved(grid, cell.Row, cell.Column)
                    && candidates[cell.Row, cell.Column].Count <= size).ToList();
                foreach (var group in Combinations(cells, size))
                {
                    var values = group.SelectMany(cell => candidates[cell.Row, cell.Column]).Distinct().ToList();
                    if (values.Count != size)
                        continue;

                    if (SudokuAlgorithmHelpers.RemoveCandidates(grid, candidates, unit.Except(group), values))
                        return true;
                }
            }

            return false;
        }

        private static bool FindHiddenSubset(int[,] grid, HashSet<int>[,] candidates, int size)
        {
            foreach (var unit in SudokuAlgorithmHelpers.Units())
            {
                foreach (var values in Combinations(Enumerable.Range(1, 9).ToList(), size))
                {
                    var cells = unit.Where(cell => SudokuAlgorithmHelpers.IsUnsolved(grid, cell.Row, cell.Column)
                        && values.Any(value => candidates[cell.Row, cell.Column].Contains(value))).ToList();
                    if (cells.Count != size || values.Any(value => !cells.Any(cell => candidates[cell.Row, cell.Column].Contains(value))))
                        continue;

                    var changed = false;
                    foreach (var cell in cells)
                        changed |= candidates[cell.Row, cell.Column].RemoveWhere(value => !values.Contains(value)) > 0;
                    if (changed)
                        return true;
                }
            }

            return false;
        }

        private static IEnumerable<List<T>> Combinations<T>(IReadOnlyList<T> items, int size, int start = 0)
        {
            if (size == 0)
            {
                yield return new List<T>();
                yield break;
            }

            for (var index = start; index <= items.Count - size; index++)
            {
                foreach (var tail in Combinations(items, size - 1, index + 1))
                {
                    tail.Insert(0, items[index]);
                    yield return tail;
                }
            }
        }

        private static int CountInUnit(int row, int column, int value, HashSet<int>[,] candidates, int[,] grid, bool rowUnit)
        {
            var count = 0;
            for (var index = 0; index < 9; index++)
            {
                var candidateRow = rowUnit ? row : index;
                var candidateColumn = rowUnit ? index : column;
                if (SudokuAlgorithmHelpers.IsUnsolved(grid, candidateRow, candidateColumn)
                    && candidates[candidateRow, candidateColumn].Contains(value))
                    count++;
            }
            return count;
        }

        private static int CountInBox(int row, int column, int value, HashSet<int>[,] candidates, int[,] grid)
        {
            var count = 0;
            for (var candidateRow = row / 3 * 3; candidateRow < row / 3 * 3 + 3; candidateRow++)
                for (var candidateColumn = column / 3 * 3; candidateColumn < column / 3 * 3 + 3; candidateColumn++)
                    if (SudokuAlgorithmHelpers.IsUnsolved(grid, candidateRow, candidateColumn)
                        && candidates[candidateRow, candidateColumn].Contains(value))
                        count++;
            return count;
        }
    }
}