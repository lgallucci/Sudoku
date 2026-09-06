using System;
using System.Collections.Generic;
using System.Linq;

namespace SudokuLib.GameLogic;


public class SudokuGenerator
{
    private const int SIZE = 9;
    private static readonly int[,] BLANK_BOARD = new int[SIZE, SIZE];
    private static readonly List<int> NumArray = Enumerable.Range(1, 9).ToList();
    private static int counter = 0;
    private static int pokeCounter = 0;
    private static Random rand = new Random();

    public static int[,] CloneBoard(int[,] board)
    {
        int[,] clone = new int[SIZE, SIZE];
        Array.Copy(board, clone, board.Length);
        return clone;
    }

    public static List<int> Shuffle(List<int> array)
    {
        var newArray = new List<int>(array);
        for (int i = newArray.Count - 1; i > 0; i--)
        {
            int j = rand.Next(i + 1);
            (newArray[i], newArray[j]) = (newArray[j], newArray[i]);
        }
        return newArray;
    }

    public static bool RowSafe(int[,] puzzle, int row, int num)
    {
        for (int i = 0; i < SIZE; i++)
        {
            if (puzzle[row, i] == num) return false;
        }
        return true;
    }

    public static bool ColSafe(int[,] puzzle, int col, int num)
    {
        for (int i = 0; i < SIZE; i++)
        {
            if (puzzle[i, col] == num) return false;
        }
        return true;
    }

    public static bool BoxSafe(int[,] puzzle, int row, int col, int num)
    {
        int boxStartRow = row - row % 3;
        int boxStartCol = col - col % 3;
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                if (puzzle[boxStartRow + r, boxStartCol + c] == num) return false;
            }
        }
        return true;
    }

    public static bool SafeToPlace(int[,] puzzle, int row, int col, int num)
    {
        return RowSafe(puzzle, row, num) &&
               ColSafe(puzzle, col, num) &&
               BoxSafe(puzzle, row, col, num);
    }

    public static (int row, int col)? NextEmptyCell(int[,] puzzle)
    {
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                if (puzzle[row, col] == 0)
                    return (row, col);
            }
        }
        return null;
    }

    public static bool FillPuzzle(int[,] board)
    {
        var empty = NextEmptyCell(board);
        if (!empty.HasValue) return true;

        var (row, col) = empty.Value;

        foreach (var num in Shuffle(NumArray))
        {
            counter++;
            if (counter > 20000000)
                throw new Exception("Recursion Timeout");

            if (SafeToPlace(board, row, col, num))
            {
                board[row, col] = num;
                if (FillPuzzle(board)) return true;
                board[row, col] = 0;
            }
        }
        return false;
    }

    public static int[,] NewSolvedBoard()
    {
        var board = new int[SIZE, SIZE];
        FillPuzzle(board);
        return board;
    }

    public static (List<(int row, int col, int val)>, int[,]) PokeHoles(int[,] board, int holes)
    {
        var removedVals = new List<(int row, int col, int val)>();
        while (removedVals.Count < holes)
        {
            int val = rand.Next(81);
            int row = val / 9;
            int col = val % 9;

            if (board[row, col] == 0) continue;

            int backup = board[row, col];
            board[row, col] = 0;

            var proposed = CloneBoard(board);
            counter = 0;
            if (!FillPuzzle(proposed))
            {
                board[row, col] = backup;
            }
            else
            {
                removedVals.Add((row, col, backup));
            }
        }
        return (removedVals, board);
    }

    public static (List<(int row, int col, int val)>, int[,], int[,]) NewStartingBoard(int holes)
    {
        while (true)
        {
            try
            {
                counter = 0;
                var solved = NewSolvedBoard();
                var (removedVals, starting) = PokeHoles(CloneBoard(solved), holes);
                if (MultiplePossibleSolutions(starting))
                {
                    continue;
                }
                return (removedVals, starting, solved);
            }
            catch
            {
                continue;
            }
        }
    }

    public static List<(int row, int col)> EmptyCellCoords(int[,] board)
    {
        var coords = new List<(int row, int col)>();
        for (int row = 0; row < SIZE; row++)
        {
            for (int col = 0; col < SIZE; col++)
            {
                if (board[row, col] == 0)
                    coords.Add((row, col));
            }
        }
        return coords;
    }

    public static (int row, int col)? NextStillEmptyCell(int[,] board, List<(int row, int col)> emptyCells)
    {
        foreach (var (row, col) in emptyCells)
        {
            if (board[row, col] == 0)
                return (row, col);
        }
        return null;
    }

    public static bool FillFromArray(int[,] board, List<(int row, int col)> emptyCells)
    {
        var next = NextStillEmptyCell(board, emptyCells);
        if (!next.HasValue) return true;

        var (row, col) = next.Value;
        foreach (var num in Shuffle(NumArray))
        {
            pokeCounter++;
            if (pokeCounter > 60000000)
                throw new Exception("Poke Timeout");

            if (SafeToPlace(board, row, col, num))
            {
                board[row, col] = num;
                if (FillFromArray(board, emptyCells)) return true;
                board[row, col] = 0;
            }
        }
        return false;
    }

    public static bool MultiplePossibleSolutions(int[,] board)
    {
        var possibleSolutions = new HashSet<string>();
        var emptyCells = EmptyCellCoords(board);

        for (int i = 0; i < emptyCells.Count; i++)
        {
            var rotated = new List<(int row, int col)>(emptyCells);
            var start = rotated[i];
            rotated.RemoveAt(i);
            rotated.Insert(0, start);

            var clone = CloneBoard(board);
            pokeCounter = 0;

            FillFromArray(clone, rotated);
            string solution = string.Join(",", clone.Cast<int>());
            possibleSolutions.Add(solution);

            if (possibleSolutions.Count > 1) return true;
        }

        return false;
    }
}
