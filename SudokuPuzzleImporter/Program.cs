using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SudokuLib.GameLogic;

namespace SudokuPuzzleImporter;

// Reads a large "id,puzzle,solution,clues,difficulty" CSV, grades every puzzle with
// SudokuGenerator.CheckBoardDifficulty (ignoring the CSV's own difficulty column), and
// writes each puzzle into a fixed-width file bucketed by our own difficulty tier so it
// can later be picked at random without loading the whole tier file into memory.
internal static class Program
{
    private const int MinTier = 1;
    private const int MaxTier = 5;

    private static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: SudokuPuzzleImporter <csv-path> [output-dir]");
            return 1;
        }

        string csvPath = args[0];
        string outputDir = args.Length > 1 ? args[1] : "PuzzlesByDifficulty";

        if (!File.Exists(csvPath))
        {
            Console.Error.WriteLine($"CSV file not found: {csvPath}");
            return 1;
        }

        Directory.CreateDirectory(outputDir);

        var writers = new PuzzleTierWriter[MaxTier + 1];
        for (int tier = MinTier; tier <= MaxTier; tier++)
        {
            string path = Path.Combine(outputDir, PuzzleFileFormat.GetTierFileName(tier));
            writers[tier] = new PuzzleTierWriter(path);
        }

        long processed = 0;
        long skipped = 0;
        var tierCounts = new long[MaxTier + 1];
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // File.ReadLines streams the file lazily; Partitioner lets Parallel.ForEach
            // consume that stream across threads without ever materializing all 3M rows.
            var lines = System.Linq.Enumerable.Skip(File.ReadLines(csvPath), 1);
            var partitioner = System.Collections.Concurrent.Partitioner.Create(lines, EnumerablePartitionerOptions.NoBuffering);

            Parallel.ForEach(partitioner, line =>
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    return;
                }

                if (!TryParseRow(line, out int id, out string puzzle, out string solution))
                {
                    Interlocked.Increment(ref skipped);
                    return;
                }

                int[,] board = ToBoard(puzzle);
                BoardDifficultyResult result = SudokuGenerator.CheckBoardDifficulty(id, board);
                int tier = Math.Clamp(result.DifficultyTier, MinTier, MaxTier);

                writers[tier].Write(new PuzzleRecord(id, puzzle, solution, result.HardestAlgorithmUsed));
                Interlocked.Increment(ref tierCounts[tier]);

                long done = Interlocked.Increment(ref processed);
                if (done % 50_000 == 0)
                {
                    Console.WriteLine($"Processed {done:N0} puzzles ({skipped:N0} skipped) in {stopwatch.Elapsed}...");
                }
            });
        }
        finally
        {
            foreach (var writer in writers)
            {
                writer?.Dispose();
            }
        }

        stopwatch.Stop();
        Console.WriteLine($"Done. Processed {processed:N0} puzzles, skipped {skipped:N0}, in {stopwatch.Elapsed}.");
        for (int tier = MinTier; tier <= MaxTier; tier++)
        {
            Console.WriteLine($"  Tier {tier}: {tierCounts[tier]:N0} puzzles -> {Path.Combine(outputDir, PuzzleFileFormat.GetTierFileName(tier))}");
        }

        return 0;
    }

    private static bool TryParseRow(string line, out int id, out string puzzle, out string solution)
    {
        id = 0;
        puzzle = string.Empty;
        solution = string.Empty;

        // Only the first 3 columns (id, puzzle, solution) are needed; clues/difficulty are ignored.
        string[] parts = line.Split(',', 4);
        if (parts.Length < 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], out id))
        {
            return false;
        }

        puzzle = parts[1].Trim();
        solution = parts[2].Trim();

        return puzzle.Length == PuzzleFileFormat.CellCount && solution.Length == PuzzleFileFormat.CellCount;
    }

    private static int[,] ToBoard(string puzzle)
    {
        var board = new int[9, 9];
        for (int i = 0; i < 81; i++)
        {
            char c = puzzle[i];
            int value = c is '.' or '0' ? 0 : c - '0';
            board[i / 9, i % 9] = value;
        }
        return board;
    }
}
