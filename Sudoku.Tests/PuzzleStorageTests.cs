using Microsoft.VisualStudio.TestTools.UnitTesting;
using SudokuLib.GameLogic;

namespace Sudoku.Tests;

[TestClass]
public class PuzzleStorageTests
{
    [TestMethod]
    public void SerializeDeserialize_PreservesPuzzleRecord()
    {
        var record = new PuzzleRecord(
            123456,
            "53..7....6..195....98....6.8...6...34..8.3..17...2...6.6....28....419..5....8..79",
            "534678912672195348198342567859761423426853791713924856961537284287419635345286179",
            "Brute Force / Advanced Chains");

        byte[] serialized = PuzzleFileFormat.Serialize(record);
        PuzzleRecord deserialized = PuzzleFileFormat.Deserialize(serialized);

        Assert.AreEqual(PuzzleFileFormat.RecordLength, serialized.Length);
        Assert.AreEqual(record, deserialized);
    }

    [TestMethod]
    public void Serialize_PacksTwoCellsIntoEachByte()
    {
        var record = new PuzzleRecord(1, "123456789" + new string('.', 72), new string('1', 81), "Naked Singles");

        byte[] serialized = PuzzleFileFormat.Serialize(record);

        Assert.AreEqual(0x21, serialized[sizeof(int)]);
        Assert.AreEqual(0x43, serialized[sizeof(int) + 1]);
        Assert.AreEqual(0x09, serialized[sizeof(int) + 4]);
    }
}