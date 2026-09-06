using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SudokuLib.GameLogic
{
    public class AdvancedAlgorithms
    {
        
        // Tier 3: Advanced Fish Pattern. A candidate is limited to exactly two columns
        // in exactly two rows, forming a rectangle.
        public static bool FindXWings(int[,] _grid, HashSet<int>[,] _candidates)
        {
            for (var value = 1; value <= 9; value++)
                if (FindFish(_grid, _candidates, value, true, 2)
                    || FindFish(_grid, _candidates, value, false, 2))
                    return true;
            return false;
        }

        internal static bool FindHiddenQuads(int[,] grid, HashSet<int>[,] candidates)
            => FindHiddenSubset(grid, candidates, 4);

        internal static bool FindNakedQuads(int[,] grid, HashSet<int>[,] candidates)
            => FindNakedSubset(grid, candidates, 4);

        internal static bool FindRectangleElimination(int[,] grid, HashSet<int>[,] candidates)
        {
            for (var value = 1; value <= 9; value++)
            {
                foreach (var box in SudokuAlgorithmHelpers.Units().Skip(18))
                {
                    var locations = box.Where(cell => SudokuAlgorithmHelpers.IsUnsolved(grid, cell.Row, cell.Column)
                        && candidates[cell.Row, cell.Column].Contains(value)).ToList();
                    if (locations.Count < 2)
                        continue;

                    var rows = locations.Select(cell => cell.Row).Distinct().ToList();
                    var columns = locations.Select(cell => cell.Column).Distinct().ToList();
                    if (rows.Count == 1)
                    {
                        var row = rows[0];
                        for (var column = 0; column < 9; column++)
                        {
                            if (column / 3 == locations[0].Column / 3 || !candidates[row, column].Contains(value))
                                continue;
                            if (StrongLinkInColumn(grid, candidates, value, column, row))
                            {
                                var intersection = (row, column);
                                if (SudokuAlgorithmHelpers.RemoveCandidate(grid, candidates, intersection.row, intersection.column, value))
                                    return true;
                            }
                        }
                    }
                    else if (columns.Count == 1)
                    {
                        var column = columns[0];
                        for (var row = 0; row < 9; row++)
                        {
                            if (row / 3 == locations[0].Row / 3 || !candidates[row, column].Contains(value))
                                continue;
                            if (StrongLinkInRow(grid, candidates, value, row, column)
                                && SudokuAlgorithmHelpers.RemoveCandidate(grid, candidates, row, column, value))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        internal static bool FindSimpleColouring(int[,] grid, HashSet<int>[,] candidates)
        {
            for (var value = 1; value <= 9; value++)
            {
                var graph = BuildStrongLinkGraph(grid, candidates, value);
                var colours = new Dictionary<(int Row, int Column), int>();
                foreach (var start in graph.Keys)
                {
                    if (colours.ContainsKey(start))
                        continue;
                    var queue = new Queue<(int Row, int Column)>();
                    colours[start] = 0;
                    queue.Enqueue(start);
                    while (queue.Count > 0)
                    {
                        var current = queue.Dequeue();
                        foreach (var next in graph[current])
                        {
                            if (!colours.TryGetValue(next, out var colour))
                            {
                                colours[next] = 1 - colours[current];
                                queue.Enqueue(next);
                            }
                            else if (colour == colours[current] && SudokuAlgorithmHelpers.Sees(current, next))
                            {
                                var impossible = colour;
                                foreach (var cell in colours.Where(pair => pair.Value == impossible).Select(pair => pair.Key))
                                    if (SudokuAlgorithmHelpers.RemoveCandidate(grid, candidates, cell.Row, cell.Column, value))
                                        return true;
                            }
                        }
                    }
                }

                foreach (var cell in CandidateCells(grid, candidates).Where(cell => candidates[cell.Row, cell.Column].Contains(value)))
                {
                    if (colours.TryGetValue(cell, out _))
                        continue;
                    var visible = colours.Where(pair => pair.Value != 0 && SudokuAlgorithmHelpers.Sees(cell, pair.Key))
                        .Select(pair => pair.Value).Distinct().ToList();
                    if (visible.Count == 2 && SudokuAlgorithmHelpers.RemoveCandidate(grid, candidates, cell.Row, cell.Column, value))
                        return true;
                }
            }

            return false;
        }

        internal static bool FindSwordfish(int[,] grid, HashSet<int>[,] candidates)
        {
            for (var value = 1; value <= 9; value++)
            {
                if (FindFish(grid, candidates, value, true) || FindFish(grid, candidates, value, false))
                    return true;
            }
            return false;
        }

        internal static bool FindUniqueRectangles(int[,] grid, HashSet<int>[,] candidates)
        {
            for (var row1 = 0; row1 < 8; row1++)
                for (var row2 = row1 + 1; row2 < 9; row2++)
                    for (var column1 = 0; column1 < 8; column1++)
                        for (var column2 = column1 + 1; column2 < 9; column2++)
                        {
                            var cells = new[] { (row1, column1), (row1, column2), (row2, column1), (row2, column2) };
                            if (cells.Any(cell => !SudokuAlgorithmHelpers.IsUnsolved(grid, cell.Item1, cell.Item2)))
                                continue;
                            var pairs = cells.Select(cell => candidates[cell.Item1, cell.Item2]).ToList();
                            var pair = pairs.Where(set => set.Count == 2).Select(set => set.ToHashSet()).FirstOrDefault();
                            if (pair == null || pairs.Count(set => set.SetEquals(pair)) != 3)
                                continue;
                            var target = cells.First(cell => !candidates[cell.Item1, cell.Item2].SetEquals(pair));
                            if (candidates[target.Item1, target.Item2].IsSupersetOf(pair)
                                && candidates[target.Item1, target.Item2].Count > 2)
                            {
                                var extra = candidates[target.Item1, target.Item2].Except(pair).ToList();
                                if (SudokuAlgorithmHelpers.RemoveCandidates(grid, candidates,
                                    new[] { (target.Item1, target.Item2) }, extra))
                                    return true;
                            }
                        }
            return false;
        }

        internal static bool FindWWings(int[,] grid, HashSet<int>[,] candidates)
        {
            var cells = CandidateCells(grid, candidates).Where(cell => candidates[cell.Row, cell.Column].Count == 2).ToList();
            foreach (var first in cells)
                foreach (var second in cells.Where(cell => cell.CompareTo(first) > 0))
                {
                    var pair = candidates[first.Row, first.Column].Intersect(candidates[second.Row, second.Column]).ToList();
                    if (pair.Count != 2)
                        continue;
                    foreach (var strongValue in pair)
                    {
                        var otherValue = pair[0] == strongValue ? pair[1] : pair[0];
                        foreach (var link in StrongLinks(grid, candidates, strongValue))
                        {
                            if (!SudokuAlgorithmHelpers.Sees(first, link.First) || !SudokuAlgorithmHelpers.Sees(second, link.Second))
                                continue;
                            var targets = SudokuAlgorithmHelpers.Peers(first).Intersect(SudokuAlgorithmHelpers.Peers(second));
                            if (SudokuAlgorithmHelpers.RemoveCandidates(grid, candidates, targets, new[] { otherValue }))
                                return true;
                        }
                    }
                }
            return false;
        }

        internal static bool FindXYZWing(int[,] grid, HashSet<int>[,] candidates)
        {
            var pivots = CandidateCells(grid, candidates).Where(cell => candidates[cell.Row, cell.Column].Count == 3);
            foreach (var pivot in pivots)
            {
                var wings = SudokuAlgorithmHelpers.Peers(pivot).Where(cell => SudokuAlgorithmHelpers.IsUnsolved(grid, cell.Row, cell.Column)
                    && candidates[cell.Row, cell.Column].Count == 2).ToList();
                foreach (var first in wings)
                    foreach (var second in wings.Where(cell => cell.CompareTo(first) > 0))
                    {
                        var common = candidates[first.Row, first.Column].Intersect(candidates[second.Row, second.Column]).ToList();
                        if (common.Count != 1 || !candidates[pivot.Row, pivot.Column].IsSupersetOf(common))
                            continue;
                        var pivotOnly = candidates[pivot.Row, pivot.Column].Except(common).ToList();
                        if (pivotOnly.Count != 2 || !candidates[first.Row, first.Column].Contains(pivotOnly[0])
                            || !candidates[second.Row, second.Column].Contains(pivotOnly[1]))
                            continue;
                        var targets = SudokuAlgorithmHelpers.Peers(first).Intersect(SudokuAlgorithmHelpers.Peers(second));
                        if (SudokuAlgorithmHelpers.RemoveCandidates(grid, candidates, targets, common))
                            return true;
                    }
            }
            return false;
        }

        internal static bool FindYWings(int[,] grid, HashSet<int>[,] candidates)
        {
            var pivots = CandidateCells(grid, candidates).Where(cell => candidates[cell.Row, cell.Column].Count == 2);
            foreach (var pivot in pivots)
            {
                var wings = SudokuAlgorithmHelpers.Peers(pivot).Where(cell => SudokuAlgorithmHelpers.IsUnsolved(grid, cell.Row, cell.Column)
                    && candidates[cell.Row, cell.Column].Count == 2).ToList();
                foreach (var first in wings)
                    foreach (var second in wings.Where(cell => cell.CompareTo(first) > 0))
                    {
                        var common = candidates[first.Row, first.Column].Intersect(candidates[second.Row, second.Column]).ToList();
                        if (common.Count != 1)
                            continue;
                        var z = common[0];
                        var firstOnly = candidates[first.Row, first.Column].Except(new[] { z }).Single();
                        var secondOnly = candidates[second.Row, second.Column].Except(new[] { z }).Single();
                        if (!candidates[pivot.Row, pivot.Column].SetEquals(new[] { firstOnly, secondOnly }))
                            continue;
                        var targets = SudokuAlgorithmHelpers.Peers(first).Intersect(SudokuAlgorithmHelpers.Peers(second));
                        if (SudokuAlgorithmHelpers.RemoveCandidates(grid, candidates, targets, new[] { z }))
                            return true;
                    }
            }
            return false;
        }

        private static IEnumerable<(int Row, int Column)> CandidateCells(int[,] grid, HashSet<int>[,] candidates)
            => Enumerable.Range(0, 9).SelectMany(row => Enumerable.Range(0, 9)
                .Where(column => SudokuAlgorithmHelpers.IsUnsolved(grid, row, column))
                .Select(column => (row, column)));

        private static bool FindNakedSubset(int[,] grid, HashSet<int>[,] candidates, int size)
        {
            foreach (var unit in SudokuAlgorithmHelpers.Units())
                foreach (var group in Combinations(unit.Where(cell => SudokuAlgorithmHelpers.IsUnsolved(grid, cell.Row, cell.Column)
                    && candidates[cell.Row, cell.Column].Count <= size).ToList(), size))
                {
                    var values = group.SelectMany(cell => candidates[cell.Row, cell.Column]).Distinct().ToList();
                    if (values.Count == size && SudokuAlgorithmHelpers.RemoveCandidates(grid, candidates, unit.Except(group), values))
                        return true;
                }
            return false;
        }

        private static bool FindHiddenSubset(int[,] grid, HashSet<int>[,] candidates, int size)
        {
            foreach (var unit in SudokuAlgorithmHelpers.Units())
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
            return false;
        }

        private static IEnumerable<List<T>> Combinations<T>(IReadOnlyList<T> items, int size, int start = 0)
        {
            if (size == 0) { yield return new List<T>(); yield break; }
            for (var index = start; index <= items.Count - size; index++)
                foreach (var tail in Combinations(items, size - 1, index + 1)) { tail.Insert(0, items[index]); yield return tail; }
        }

            private static bool FindFish(int[,] grid, HashSet<int>[,] candidates, int value, bool byRows, int fishSize = 3)
        {
            var lines = new List<(int Line, HashSet<int> Positions)>();
            for (var line = 0; line < 9; line++)
            {
                var positions = Enumerable.Range(0, 9).Where(position =>
                    SudokuAlgorithmHelpers.IsUnsolved(grid, byRows ? line : position, byRows ? position : line)
                    && candidates[byRows ? line : position, byRows ? position : line].Contains(value)).ToHashSet();
                if (positions.Count >= 2 && positions.Count <= fishSize)
                    lines.Add((line, positions));
            }
            foreach (var group in Combinations(lines, fishSize))
            {
                var positions = group.SelectMany(item => item.Positions).Distinct().ToList();
                if (positions.Count != fishSize)
                    continue;
                var otherLines = Enumerable.Range(0, 9).Except(group.Select(item => item.Line));
                foreach (var position in positions)
                    foreach (var other in otherLines)
                        if (SudokuAlgorithmHelpers.RemoveCandidate(grid, candidates, byRows ? other : position, byRows ? position : other, value))
                            return true;
            }
            return false;
        }

        private static Dictionary<(int Row, int Column), HashSet<(int Row, int Column)>> BuildStrongLinkGraph(int[,] grid, HashSet<int>[,] candidates, int value)
        {
            var graph = CandidateCells(grid, candidates).Where(cell => candidates[cell.Row, cell.Column].Contains(value))
                .ToDictionary(cell => cell, _ => new HashSet<(int Row, int Column)>());
            foreach (var unit in SudokuAlgorithmHelpers.Units())
            {
                var cells = unit.Where(cell => graph.ContainsKey(cell)).ToList();
                if (cells.Count == 2) { graph[cells[0]].Add(cells[1]); graph[cells[1]].Add(cells[0]); }
            }
            return graph;
        }

        private static IEnumerable<((int Row, int Column) First, (int Row, int Column) Second)> StrongLinks(int[,] grid, HashSet<int>[,] candidates, int value)
            => SudokuAlgorithmHelpers.Units().Select(unit => unit.Where(cell => SudokuAlgorithmHelpers.IsUnsolved(grid, cell.Row, cell.Column)
                && candidates[cell.Row, cell.Column].Contains(value)).ToList()).Where(cells => cells.Count == 2)
                .Select(cells => (cells[0], cells[1]));

        private static bool StrongLinkInRow(int[,] grid, HashSet<int>[,] candidates, int value, int row, int excludedColumn)
            => Enumerable.Range(0, 9).Count(column => column != excludedColumn && SudokuAlgorithmHelpers.IsUnsolved(grid, row, column)
                && candidates[row, column].Contains(value)) == 1;

        private static bool StrongLinkInColumn(int[,] grid, HashSet<int>[,] candidates, int value, int column, int excludedRow)
            => Enumerable.Range(0, 9).Count(row => row != excludedRow && SudokuAlgorithmHelpers.IsUnsolved(grid, row, column)
                && candidates[row, column].Contains(value)) == 1;
    }
}