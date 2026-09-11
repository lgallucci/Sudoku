using System;
using System.Buffers.Binary;
using System.IO;

namespace SudokuLib.GameLogic;

public record PuzzleRecord(int Id, string Puzzle, string Solution, string HardestAlgorithm);

// Fixed-width binary record layout so any puzzle can be located and read by index alone
// (offset = index * RecordLength), without loading the whole tier file.
public static class PuzzleFileFormat
{
    public const int CellCount = 81;
    public const int PackedCellLength = (CellCount + 1) / 2;
    public const int RecordLength = sizeof(int) + PackedCellLength + PackedCellLength + sizeof(byte);

    private static readonly string[] Algorithms =
    {
        "Naked/Hidden Singles",
        "Naked Singles",
        "Hidden Singles",
        "Pointing Pairs",
        "Naked Pairs",
        "Hidden Pairs",
        "Box/Line Reduction",
        "Naked Triples",
        "Hidden Triples",
        "X-Wing",
        "Naked Quads",
        "Hidden Quads",
        "Swordfish",
        "Y-Wing",
        "W-Wing",
        "BUG+1",
        "Unique Rectangle",
        "Rectangle Elimination",
        "Simple Colouring",
        "XYZ-Wing",
        "Brute Force / Advanced Chains"
    };

    public static string GetTierFileName(int tier) => $"tier{tier}.puzzles";

    public static byte[] Serialize(PuzzleRecord record)
    {
        if (record.Puzzle.Length != CellCount)
            throw new ArgumentException($"Puzzle must be {CellCount} characters long.", nameof(record));
        if (record.Solution.Length != CellCount)
            throw new ArgumentException($"Solution must be {CellCount} characters long.", nameof(record));
        if (record.Id < 0)
            throw new ArgumentException("Id must not be negative.", nameof(record));

        int algorithmIndex = Array.IndexOf(Algorithms, record.HardestAlgorithm);
        if (algorithmIndex < 0)
            throw new ArgumentException($"Unknown hardest algorithm: {record.HardestAlgorithm}", nameof(record));

        byte[] buffer = new byte[RecordLength];
        BinaryPrimitives.WriteInt32LittleEndian(buffer, record.Id);
        PackCells(record.Puzzle, buffer, sizeof(int));
        PackCells(record.Solution, buffer, sizeof(int) + PackedCellLength);
        buffer[^1] = (byte)(algorithmIndex + 1);
        return buffer;
    }

    public static PuzzleRecord Deserialize(byte[] buffer)
    {
        if (buffer.Length != RecordLength)
            throw new ArgumentException($"A puzzle record must be {RecordLength} bytes long.", nameof(buffer));

        int id = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        string puzzle = UnpackCells(buffer, sizeof(int), useDotsForZero: true);
        string solution = UnpackCells(buffer, sizeof(int) + PackedCellLength, useDotsForZero: false);
        int algorithmIndex = buffer[^1] - 1;
        if (algorithmIndex < 0 || algorithmIndex >= Algorithms.Length)
            throw new InvalidDataException($"Unknown algorithm ID in puzzle record: {buffer[^1]}.");

        return new PuzzleRecord(id, puzzle, solution, Algorithms[algorithmIndex]);
    }

    private static void PackCells(string cells, byte[] buffer, int offset)
    {
        for (int index = 0; index < CellCount; index += 2)
        {
            byte first = ParseCell(cells[index]);
            byte second = index + 1 < CellCount ? ParseCell(cells[index + 1]) : (byte)0;
            buffer[offset + index / 2] = (byte)(first | (second << 4));
        }
    }

    private static string UnpackCells(byte[] buffer, int offset, bool useDotsForZero)
    {
        char[] cells = new char[CellCount];
        for (int index = 0; index < CellCount; index++)
        {
            byte value = (byte)((buffer[offset + index / 2] >> (index % 2 * 4)) & 0x0F);
            if (value > 9)
                throw new InvalidDataException($"Invalid cell value in puzzle record: {value}.");

            cells[index] = value == 0 && useDotsForZero ? '.' : (char)('0' + value);
        }

        return new string(cells);
    }

    private static byte ParseCell(char value)
    {
        if (value == '.')
            return 0;
        if (value >= '0' && value <= '9')
            return (byte)(value - '0');

        throw new ArgumentException($"Invalid cell value: {value}.");
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
        if (_stream.Length % PuzzleFileFormat.RecordLength != 0)
        {
            _stream.Dispose();
            throw new InvalidDataException($"Puzzle file size is not a multiple of {PuzzleFileFormat.RecordLength} bytes. It may use an older format.");
        }

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
