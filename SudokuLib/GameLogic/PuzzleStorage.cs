using System;
using System.IO;
using System.Text;

namespace SudokuLib.GameLogic;

public record PuzzleRecord(int Id, string Puzzle, string Solution, string HardestAlgorithm);

// Fixed-width, single-byte-per-char record layout so any puzzle can be located and read
// by index alone (offset = index * RecordLength), without loading the whole tier file.
public static class PuzzleFileFormat
{
    public const int IdLength = 10;
    public const int CellCount = 81;
    public const int AlgorithmLength = 32; // fits the longest strategy name ("Brute Force / Advanced Chains") plus padding
    public const int RecordLength = IdLength + CellCount + CellCount + AlgorithmLength + 1; // + '\n'
    public static readonly Encoding Encoding = Encoding.ASCII;

    public static string GetTierFileName(int tier) => $"tier{tier}.puzzles";

    public static byte[] Serialize(PuzzleRecord record)
    {
        if (record.Puzzle.Length != CellCount)
            throw new ArgumentException($"Puzzle must be {CellCount} characters long.", nameof(record));
        if (record.Solution.Length != CellCount)
            throw new ArgumentException($"Solution must be {CellCount} characters long.", nameof(record));
        if (record.HardestAlgorithm.Length > AlgorithmLength)
            throw new ArgumentException($"HardestAlgorithm must be at most {AlgorithmLength} characters long.", nameof(record));

        string id = record.Id.ToString();
        if (id.Length > IdLength)
            throw new ArgumentException($"Id {record.Id} does not fit in {IdLength} characters.", nameof(record));

        string line = id.PadLeft(IdLength, '0') + record.Puzzle + record.Solution
            + record.HardestAlgorithm.PadRight(AlgorithmLength) + "\n";
        return Encoding.GetBytes(line);
    }

    public static PuzzleRecord Deserialize(byte[] buffer)
    {
        string line = Encoding.GetString(buffer).TrimStart().TrimStart('\n');
        int id = int.Parse(line.AsSpan(0, IdLength));
        string puzzle = line.Substring(IdLength, CellCount);
        string solution = line.Substring(IdLength + CellCount, CellCount);
        string algorithm = line.Substring(IdLength + CellCount + CellCount, AlgorithmLength).TrimEnd();
        return new PuzzleRecord(id, puzzle, solution, algorithm);
    }
}

// Appends puzzle records to a single tier's file. Safe to call Write from multiple threads.
public sealed class PuzzleTierWriter : IDisposable
{
    private readonly FileStream _stream;
    private readonly object _lock = new();

    // append=true resumes an existing tier file instead of truncating it; falls back to
    // creating the file if it doesn't exist yet (e.g. first run, or a tier with no puzzles so far).
    public PuzzleTierWriter(string filePath, bool append = false)
    {
        FileMode mode = append && File.Exists(filePath) ? FileMode.Append : FileMode.Create;
        _stream = new FileStream(filePath, mode, FileAccess.Write, FileShare.Read, bufferSize: 1 << 20);
    }

    public void Write(PuzzleRecord record)
    {
        byte[] bytes = PuzzleFileFormat.Serialize(record);
        lock (_lock)
        {
            _stream.Write(bytes, 0, bytes.Length);
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            _stream.Flush();
            _stream.Dispose();
        }
    }
}

// Reads puzzle records from a tier file by index, enabling O(1) random selection
// (e.g. GetRandom) without ever loading the entire file into memory.
public sealed class PuzzleTierReader : IDisposable
{
    private readonly FileStream _stream;
    private readonly object _lock = new();

    public int Count { get; }

    public PuzzleTierReader(string filePath)
    {
        _stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 1 << 16);
        Count = (int)(_stream.Length / PuzzleFileFormat.RecordLength);
    }

    public PuzzleRecord GetAt(int index)
    {
        if (index < 0 || index >= Count)
            throw new ArgumentOutOfRangeException(nameof(index));

        byte[] buffer = new byte[PuzzleFileFormat.RecordLength];
        lock (_lock)
        {
            _stream.Seek((long)index * PuzzleFileFormat.RecordLength, SeekOrigin.Begin);
            int read = 0;
            while (read < buffer.Length)
            {
                int n = _stream.Read(buffer, read, buffer.Length - read);
                if (n == 0) throw new EndOfStreamException("Tier file ended before a full record could be read.");
                read += n;
            }
        }
        return PuzzleFileFormat.Deserialize(buffer);
    }

    public PuzzleRecord GetRandom(Random rand) => GetAt(rand.Next(Count));

    public void Dispose() => _stream.Dispose();
}
