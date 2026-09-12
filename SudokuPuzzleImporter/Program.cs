using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SudokuLib.GameLogic;

namespace SudokuPuzzleImporter;

// Reads a large "id,puzzle,solution,clues,difficulty" CSV, grades every puzzle with
// SudokuGenerator.CheckBoardDifficulty (ignoring the CSV's own difficulty column), and
// writes each puzzle into a fixed-width file bucketed by our own difficulty tier so it
// can later be picked at random without loading the whole tier file into memory.
//
// The CSV is processed in batches. After each batch is fully written, the number of rows
// consumed so far is saved to a progress file in the output directory; re-running the
// importer with the same output directory resumes right after the last completed batch
// instead of starting over (and appends to the existing tier files rather than truncating them).
internal static class Program
{
    private const int MinTier = 1;
    private const int MaxTier = 5;
    private const int BatchSize = 20_000;
    private const string ProgressFileName = "progress.txt";
    private const int LegacyLineLength = 204;

    private static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            Console.Error.WriteLine("Usage: SudokuPuzzleImporter <csv-path> [output-dir]");
            Console.Error.WriteLine("   or: SudokuPuzzleImporter --convert-legacy [output-dir]");
            Console.Error.WriteLine("   or: SudokuPuzzleImporter --build-release-cache <source-dir> <output-dir> [sample-count]");
            return 1;
        }

        if (args[0] == "--convert-legacy")
        {
            string convertOutputDir = args.Length > 1 ? args[1] : ".";
            return ConvertLegacyFiles(convertOutputDir);
        }

        if (args[0] == "--build-release-cache")
        {
            if (args.Length < 3 || !int.TryParse(args.Length > 3 ? args[3] : "10000", out int sampleCount))
            {
                Console.Error.WriteLine("Usage: SudokuPuzzleImporter --build-release-cache <source-dir> <output-dir> [sample-count]");
                return 1;
            }

            return BuildReleaseCache(args[1], args[2], sampleCount);
        }

        string csvPath = args[0];
        string outputDir = args.Length > 1 ? args[1] : "PuzzlesByDifficulty";

        if (!File.Exists(csvPath))
        {
            Console.Error.WriteLine($"CSV file not found: {csvPath}");
            return 1;
        }

        Directory.CreateDirectory(outputDir);

        string progressPath = Path.Combine(outputDir, ProgressFileName);
        long resumeRows = ReadProgress(progressPath);
        bool resuming = resumeRows > 0;

        var writers = new PuzzleTierWriter[MaxTier + 1];
        for (int tier = 4; tier <= MaxTier; tier++)
        {
            string path = Path.Combine(outputDir, PuzzleFileFormat.GetTierFileName(tier));
            writers[tier] = new PuzzleTierWriter(path, append: resuming);
        }

        long processed = 0;
        long skipped = 0;
        var tierCounts = new long[MaxTier + 1];
        var stopwatch = Stopwatch.StartNew();

        if (resuming)
        {
            Console.WriteLine($"Resuming after row {resumeRows:N0} (from {progressPath}).");
        }

        try
        {
            // File.ReadLines streams the file lazily; skip the header plus any rows already
            // completed in a previous run, then process the remainder in fixed-size batches so
            // progress can only ever be saved at a point where every row in it was fully written.
            var rows = System.Linq.Enumerable.Skip(File.ReadLines(csvPath), 1 + (int)resumeRows);

            foreach (List<string> batch in Batch(rows, BatchSize))
            {
                var partitioner = System.Collections.Concurrent.Partitioner.Create(batch, EnumerablePartitionerOptions.NoBuffering);

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

                resumeRows += batch.Count;
                WriteProgress(progressPath, resumeRows);
            }
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

    private static int ConvertLegacyFiles(string outputDir)
    {
        for (int tier = MinTier; tier <= MaxTier; tier++)
        {
            string path = Path.Combine(outputDir, PuzzleFileFormat.GetTierFileName(tier));
            string tempPath = path + ".binary.tmp";

            if (!File.Exists(path))
            {
                Console.Error.WriteLine($"Legacy puzzle file not found: {path}");
                return 1;
            }

            try
            {
                long recordCount = 0;
                long skippedCount = 0;
                using (var input = new StreamReader(path, Encoding.ASCII, detectEncodingFromByteOrderMarks: false))
                {
                    using var output = new PuzzleTierWriter(tempPath);
                    string? line;
                    while ((line = input.ReadLine()) != null)
                    {
                        if (line.Length != LegacyLineLength)
                        {
                            skippedCount++;
                            continue;
                        }

                        output.Write(ParseLegacyRecord(line));
                        recordCount++;
                    }
                }

                File.Move(tempPath, path, overwrite: true);
                Console.WriteLine($"Converted {path}: {recordCount:N0} records, skipped {skippedCount:N0} malformed records");
            }
            catch
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }

                throw;
            }
        }

        return 0;
    }

    private static int BuildReleaseCache(string sourceDir, string outputDir, int sampleCount)
    {
        if (sampleCount <= 0)
        {
            Console.Error.WriteLine("Sample count must be positive.");
            return 1;
        }

        Directory.CreateDirectory(outputDir);
        for (int tier = MinTier; tier <= MaxTier; tier++)
        {
            string sourcePath = Path.Combine(sourceDir, PuzzleFileFormat.GetTierFileName(tier));
            if (!File.Exists(sourcePath))
            {
                Console.Error.WriteLine($"Source puzzle file not found: {sourcePath}");
                return 1;
            }

            WriteRandomSample(sourcePath, Path.Combine(outputDir, PuzzleFileFormat.GetTierFileName(tier)), sampleCount, tier);
        }

        return 0;
    }

    private static void WriteRandomSample(string sourcePath, string outputPath, int sampleCount, int tier)
    {
        using var reader = new PuzzleTierReader(sourcePath);
        int count = Math.Min(sampleCount, reader.Count);
        var random = new Random(20260911 + tier);
        var selectedIndices = new HashSet<int>();
        while (selectedIndices.Count < count)
        {
            selectedIndices.Add(random.Next(reader.Count));
        }

        using var writer = new PuzzleTierWriter(outputPath);
        foreach (int sourceIndex in selectedIndices)
        {
            writer.Write(reader.GetAt(sourceIndex));
        }

        Console.WriteLine($"Randomly sampled {outputPath}: {count:N0} records");
    }

    private static PuzzleRecord ParseLegacyRecord(string line)
    {
        int id = int.Parse(line.AsSpan(0, 10));
        string puzzle = line.Substring(10, PuzzleFileFormat.CellCount);
        string solution = line.Substring(10 + PuzzleFileFormat.CellCount, PuzzleFileFormat.CellCount);
        string algorithm = line.Substring(10 + PuzzleFileFormat.CellCount * 2, 32).TrimEnd();
        return new PuzzleRecord(id, puzzle, solution, algorithm);
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

    private static IEnumerable<List<string>> Batch(IEnumerable<string> source, int size)
    {
        var batch = new List<string>(size);
        foreach (string item in source)
        {
            batch.Add(item);
            if (batch.Count == size)
            {
                yield return batch;
                batch = new List<string>(size);
            }
        }

        if (batch.Count > 0)
        {
            yield return batch;
        }
    }

    private static long ReadProgress(string path)
    {
        if (!File.Exists(path))
        {
            return 0;
        }

        return long.TryParse(File.ReadAllText(path).Trim(), out long rows) ? rows : 0;
    }

    // Write-to-temp-then-move keeps the progress file from ever being read half-written.
    private static void WriteProgress(string path, long rowsProcessed)
    {
        string tempPath = path + ".tmp";
        File.WriteAllText(tempPath, rowsProcessed.ToString());
        File.Move(tempPath, path, overwrite: true);
    }
}
