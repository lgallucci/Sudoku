using System;

namespace SudokuLib.GameObjects
{
    public class Board
    {
        private Cell[,] cells = new Cell[9, 9];     
        public Pile Pile { get; private set; } = new Pile();

        public Board(int[,] initialValues)
        {
            Pile = new Pile();            

            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    int value = initialValues[row, col];
                    var cell = new Cell(row, col)
                    {
                        Value = value,
                        IsGiven = value > 0
                    };
                    cells[row, col] = cell;
                    Pile.TryUseValue(value);
                }
            }
        }

        public bool TrySetCellValue(int row, int col, int value)
        {
            if (value < 0 || value > 9)
            {
                throw new ArgumentOutOfRangeException("Value must be between 0 and 9.");
            }

            if (IsValidMove(row, col, value))
            {
                var previousValue = cells[row, col].Value;
                cells[row, col].SetValue(value);
                Pile.TryUseValue(value, previousValue);
                cells[row, col].IsNumberInvalid = false;
                return true;
            }
            return false;
        }

        public void SetInvalidCellValue(int row, int col, int value)
        {
            if (row < 0 || row >= 9 || col < 0 || col >= 9)
            {
                throw new ArgumentOutOfRangeException("Row and column must be between 0 and 8.");
            }

            cells[row, col].SetValue(value);
            cells[row, col].IsNumberInvalid = true;
        }

        public Cell GetCell(int row, int col)
        {
            if (row < 0 || row >= 9 || col < 0 || col >= 9)
            {
                throw new ArgumentOutOfRangeException("Row and column must be between 0 and 8.");
            }
            return cells[row, col];
        }

        public void ClearBoard()
        {
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    cells[row, col] = new Cell(row, col);
                }
            }
        }

        private bool IsValidMove(int row, int col, int value)
        {
            if (value == 0) // clearing a cell is always allowed
            {
                return true;
            }

            // Check if the value is already in the same row or column
            for (int i = 0; i < 9; i++)
            {
                if (cells[row, i].Value == value || cells[i, col].Value == value)
                {
                    return false;
                }
            }

            // Check if the value is already in the same 3x3 subgrid
            int startRow = (row / 3) * 3;
            int startCol = (col / 3) * 3;
            for (int r = startRow; r < startRow + 3; r++)
            {
                for (int c = startCol; c < startCol + 3; c++)
                {
                    if (cells[r, c].Value == value)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}